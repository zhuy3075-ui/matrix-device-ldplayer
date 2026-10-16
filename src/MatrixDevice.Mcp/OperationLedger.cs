using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using MatrixDevice.Core;
using MatrixDevice.Features;
using MatrixDevice.Host;

namespace MatrixDevice.Mcp;

internal sealed class OwnedJob(Guid operationId, ImportedScript script, ScriptJobTarget target, int timeoutMs, CancellationTokenSource cancellation)
{
    internal Guid OperationId { get; } = operationId;
    internal ImportedScript Script { get; } = script;
    internal ScriptJobTarget Target { get; } = target;
    internal int TimeoutMs { get; } = timeoutMs;
    internal CancellationTokenSource Cancellation { get; } = cancellation;
    internal required ScriptJobHandle Handle { get; init; }
    internal Timer? Deadline { get; set; }
    internal string? CancellationReason;
}

internal sealed class OperationLedger(MatrixRuntime runtime, McpPolicy policy, ImportedScriptStore scripts, Guid epoch)
{
    private sealed record Operation(string Digest, JsonObject Result);
    private readonly object gate = new();
    private readonly Dictionary<Guid, Operation> operations = [];
    private readonly Dictionary<Guid, OwnedJob> jobs = [];
    private bool stopping;
    internal void StopAccepting()
    { lock (gate) { stopping = true; foreach (var job in jobs.Values) CancelWithReason(job, "shutdown"); } }
    internal OwnedJob Get(Guid id)
    {
        lock (gate)
        {
            var job = jobs.GetValueOrDefault(id) ?? throw new ToolFailure("UNKNOWN_REFERENCE", "Unknown job reference for this server epoch.");
            policy.RequireSerial(job.Target.Serial);
            return job;
        }
    }

    internal JsonObject Start(JsonElement input, CancellationToken requestToken)
    {
        StrictJson.Object(input, ["serverEpoch", "operationId", "scriptRef", "serial", "expectedIdentity", "parameterOverrides", "timeoutMs"], "serverEpoch", "operationId", "scriptRef", "serial", "expectedIdentity");
        policy.Require("jobs.start"); RequireEpoch(input);
        var operationId = StrictJson.Uuid(input, "operationId");
        var reference = StrictJson.Uuid(input, "scriptRef");
        var serial = StrictJson.String(input, "serial"); policy.RequireSerial(serial);
        var identity = input.GetProperty("expectedIdentity");
        StrictJson.Object(identity, ["generation", "transportId"], "generation", "transportId");
        var generationText = StrictJson.String(identity, "generation", 19);
        if (!long.TryParse(generationText, NumberStyles.None, CultureInfo.InvariantCulture, out var generation) || generation <= 0 || generation.ToString(CultureInfo.InvariantCulture) != generationText)
            throw new ToolFailure("INVALID_ARGUMENT", "generation must be a canonical positive Int64 string.");
        if (identity.GetProperty("transportId").ValueKind != JsonValueKind.Null) throw new ToolFailure("INVALID_ARGUMENT", "Demo transportId must be JSON null.");
        var timeout = StrictJson.Integer(input, "timeoutMs", 1000, 300000, 300000);
        var script = scripts.Get(reference);
        Dictionary<string, JsonElement> overrides = new(StringComparer.Ordinal);
        if (input.TryGetProperty("parameterOverrides", out var values))
        {
            if (values.ValueKind != JsonValueKind.Object || values.EnumerateObject().Count() > 1000) throw new ToolFailure("INVALID_ARGUMENT", "parameterOverrides must be an object containing at most 1000 keys.");
            foreach (var value in values.EnumerateObject()) overrides.Add(value.Name, value.Value.Clone());
        }
        var digestInput = JsonSerializer.SerializeToElement(new { contractVersion = "matrix-mcp/1", scopeOwner = policy.ScopeOwner, scriptRef = reference, sha256 = script.Sha256, serial, expectedIdentity = identity, parameterOverrides = overrides, timeoutMs = timeout }, StrictJson.Options);
        var digest = StrictJson.Digest(digestInput);
        lock (gate)
        {
            requestToken.ThrowIfCancellationRequested();
            if (operations.TryGetValue(operationId, out var previous))
            {
                if (previous.Digest != digest) throw new ToolFailure("INVALID_ARGUMENT", "operationId conflicts with its immutable original request.");
                var replay = previous.Result.DeepClone().AsObject();
                if (replay["ok"]!.GetValue<bool>()) replay["replayed"] = true;
                return replay;
            }
            if (stopping) throw new ToolFailure("CANCELLED", "Server is stopping; no new jobs are accepted.");
            if (operations.Count >= 100) throw new ToolFailure("CAPACITY", "Operation ledger capacity reached; existing operationIds remain replayable.");
            JsonObject result;
            CancellationTokenSource? ownedCancellation = null;
            try
            {
                ScriptExecutionCompiler.Compile(script.Package, overrides);
                var current = runtime.Devices.Snapshot.SingleOrDefault(d => d.Serial == serial);
                if (current is null || !current.Online) throw new ToolFailure("TARGET_OFFLINE", "Authorized target is not online.");
                if (current.Generation != generation || current.TransportId is not null) throw new ToolFailure("IDENTITY_CHANGED", "Expected target generation does not match the current directory.");
                if (jobs.Values.Count(j => !j.Handle.Completion.IsCompleted) >= 2) throw new ToolFailure("CAPACITY", "Maximum active jobs is 2.");
                if (runtime.Scripts.GetStatus(serial).State is ScriptRunState.Running or ScriptRunState.Cancelling) throw new ToolFailure("BUSY", "Target already has an active script.");
                requestToken.ThrowIfCancellationRequested();
                ownedCancellation = new CancellationTokenSource();
                var target = new ScriptJobTarget(serial, generation, null, true);
                var handle = runtime.Scripts.SubmitJob(target, script.Package, ownedCancellation.Token, overrides);
                var owned = new OwnedJob(operationId, script, target, timeout, ownedCancellation) { Handle = handle };
                jobs.Add(handle.JobId, owned);
                owned.Deadline = new Timer(_ => CancelWithReason(owned, "deadline"), null, timeout, Timeout.Infinite);
                _ = handle.Completion.ContinueWith(_ => owned.Deadline.Dispose(), CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
                result = StrictJson.Node(new { ok = true, serverEpoch = epoch, operationId, jobId = handle.JobId, target = Target(target), scriptRef = reference, sha256 = script.Sha256, accepted = true, replayed = false, timeoutMs = timeout });
            }
            catch (OperationCanceledException) { ownedCancellation?.Dispose(); throw; }
            catch (Exception error)
            {
                ownedCancellation?.Dispose();
                result = McpToolHost.Error(epoch, error);
                result["error"]!["operationId"] = operationId.ToString("D");
            }
            operations.Add(operationId, new(digest, result));
            return result.DeepClone().AsObject();
        }
    }

    internal void RequireEpoch(JsonElement input)
    { if (StrictJson.Uuid(input, "serverEpoch") != epoch) throw new ToolFailure("UNKNOWN_REFERENCE", "serverEpoch belongs to a different process lifetime."); }

    internal JsonObject Cancel(Guid id)
    {
        var owned = Get(id);
        lock (gate)
        {
            var status = runtime.Scripts.GetJob(id) ?? throw new ToolFailure("UNKNOWN_REFERENCE", "Job is no longer retained.");
            var terminal = IsTerminal(status.State);
            var requested = !terminal && CancelWithReason(owned, "cancelled");
            status = runtime.Scripts.GetJob(id)!;
            return StrictJson.Node(new { ok = true, serverEpoch = epoch, jobId = id, cancelRequested = requested, state = status.State.ToString(), terminal = IsTerminal(status.State) });
        }
    }

    private bool CancelWithReason(OwnedJob owned, string reason)
    {
        lock (gate)
        {
            var status = runtime.Scripts.GetJob(owned.Handle.JobId);
            if (status is null || IsTerminal(status.State)) return false;
            Interlocked.CompareExchange(ref owned.CancellationReason, reason, null);
            var requested = runtime.Scripts.CancelJob(owned.Handle.JobId);
            return requested;
        }
    }

    internal async Task<object[]> DrainAsync()
    {
        OwnedJob[] snapshot;
        lock (gate) { stopping = true; snapshot = jobs.Values.ToArray(); foreach (var job in snapshot) CancelWithReason(job, "shutdown"); }
        foreach (var job in snapshot)
        {
            try { await job.Handle.Completion.ConfigureAwait(false); }
            catch { /* The actual failure remains in the original job status and lifecycle report. */ }
            job.Deadline?.Dispose(); job.Cancellation.Dispose();
        }
        return snapshot.Select(j => (object)new { jobId = j.Handle.JobId, operationId = j.OperationId, serial = j.Target.Serial, state = runtime.Scripts.GetJob(j.Handle.JobId)?.State.ToString(), terminal = runtime.Scripts.GetJob(j.Handle.JobId) is { } status && IsTerminal(status.State), terminalReason = Reason(j, runtime.Scripts.GetJob(j.Handle.JobId)!) }).ToArray();
    }

    internal static bool IsTerminal(ScriptRunState state) => state is ScriptRunState.Completed or ScriptRunState.Cancelled or ScriptRunState.Failed;
    internal static string? Reason(OwnedJob job, ScriptJobStatus status) => status.State switch { ScriptRunState.Completed => "completed", ScriptRunState.Failed => "failed", ScriptRunState.Cancelled => job.CancellationReason ?? "cancelled", _ => null };
    internal static object Target(ScriptJobTarget target) => new { serial = target.Serial, generation = target.Generation.ToString(CultureInfo.InvariantCulture), transportId = target.TransportId };
}
