using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Schema;
using ModelContextProtocol;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace MatrixDevice.Mcp.Tests;

internal sealed class ProductFailure(string message) : Exception(message);
internal sealed class SessionPreconditionFailure(string message, Exception inner) : Exception(message, inner);
internal sealed class SessionNotRun(string message) : Exception(message);

internal static class Evidence
{
    internal static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
    internal static void Check(bool condition, string message) { if (!condition) throw new ProductFailure(message); }
    internal static JsonElement Json(object? value) => JsonSerializer.SerializeToElement(value);
    internal static JsonElement ProtocolJson<T>(T value) => JsonSerializer.SerializeToElement(value, McpJsonUtilities.DefaultOptions.GetTypeInfo<T>());
    internal static string Hash(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
    internal static void Save(string path, object value) => File.WriteAllText(path, JsonSerializer.Serialize(value, Options));
    internal static IEnumerable<string> SnapshotLines(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        var remaining = stream.Length;
        using var line = new MemoryStream();
        var buffer = new byte[8192];
        while (remaining > 0)
        {
            var count = stream.Read(buffer, 0, (int)Math.Min(buffer.Length, remaining));
            if (count == 0) break;
            remaining -= count;
            for (var index = 0; index < count; index++)
            {
                if (buffer[index] != (byte)'\n') { line.WriteByte(buffer[index]); continue; }
                yield return new System.Text.UTF8Encoding(false, true).GetString(line.GetBuffer(), 0, checked((int)line.Length)).TrimEnd('\r');
                line.SetLength(0);
            }
        }
    }
}

internal static class Arguments
{
    internal static Dictionary<string, string> Parse(string[] args, string[] names)
    {
        if (args.Length != names.Length * 2) throw new ArgumentException("Every required option must occur exactly once with a value.");
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        for (var index = 0; index < args.Length; index += 2)
            if (!names.Contains(args[index], StringComparer.Ordinal) || args[index + 1].StartsWith("--", StringComparison.Ordinal) || !result.TryAdd(args[index], args[index + 1]))
                throw new ArgumentException("Unknown, duplicate or missing option: " + args[index]);
        return result;
    }
    internal static string Absolute(string path, bool directory)
    {
        if (!Path.IsPathFullyQualified(path) || Path.GetFullPath(path) != path || (directory ? !Directory.Exists(path) : !File.Exists(path)))
            throw new ArgumentException("Expected an existing canonical absolute path: " + path);
        return path;
    }
}

internal sealed class Schemas
{
    private readonly JsonObject contract = Load("ContractSchema.json");
    private readonly JsonObject protocol = Load("ProtocolSchema.json");
    internal static readonly string[] Names = ["matrix.capabilities", "matrix.devices.list", "matrix.scripts.import", "matrix.jobs.start", "matrix.jobs.get", "matrix.jobs.cancel"];
    private static readonly string[] Definitions = ["Capabilities", "DevicesList", "ScriptImport", "JobStart", "JobGet", "JobCancel"];
    internal void Protocol(string definition, JsonElement value) => Validate(Root(protocol, definition), value, "MCP wire " + definition);
    internal void Contract(string tool, bool output, JsonElement value)
        => Validate(Root(contract, Definitions[Array.IndexOf(Names, tool)] + (output ? "Output" : "Input")), value, tool + (output ? " output" : " input"));
    internal static void Validate(JsonElement schema, JsonElement value, string label)
    {
        RejectExternalReferences(schema);
        var compiled = JsonSchema.Build(schema);
        var result = compiled.Evaluate(value, new EvaluationOptions { RequireFormatValidation = true });
        Evidence.Check(result.IsValid, "Schema rejected " + label + ": " + JsonSerializer.Serialize(result));
    }
    internal static bool Rejects(JsonElement schema, JsonElement value)
    {
        RejectExternalReferences(schema);
        return !JsonSchema.Build(schema).Evaluate(value, new EvaluationOptions { RequireFormatValidation = true }).IsValid;
    }
    private static JsonElement Root(JsonObject bundle, string name) => Evidence.Json(new JsonObject
    {
        ["$schema"] = "https://json-schema.org/draft/2020-12/schema", ["type"] = "object",
        ["$ref"] = "#/$defs/" + name, ["$defs"] = bundle["$defs"]!.DeepClone()
    });
    private static JsonObject Load(string name)
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("MatrixDevice.Mcp.Tests." + name) ?? throw new InvalidOperationException("Missing embedded schema " + name);
        return JsonNode.Parse(stream)!.AsObject();
    }
    private static void RejectExternalReferences(JsonElement node)
    {
        if (node.ValueKind == JsonValueKind.Object)
            foreach (var field in node.EnumerateObject())
            {
                if (field.Name == "$ref") Evidence.Check(field.Value.GetString()!.StartsWith("#", StringComparison.Ordinal), "External schema resolution is forbidden.");
                RejectExternalReferences(field.Value);
            }
        else if (node.ValueKind == JsonValueKind.Array) foreach (var item in node.EnumerateArray()) RejectExternalReferences(item);
    }
}

internal sealed class StdioSession : IAsyncDisposable
{
    private readonly Schemas schemas;
    private readonly string directory;
    private readonly OwnedStdioProcess owned;
    private readonly List<Task> pending = [];
    private readonly List<string> cleanupErrors = [];
    private readonly List<Tool> tools = [];
    private readonly HashSet<string> acceptedJobs = new(StringComparer.Ordinal);
    private readonly HashSet<string> shutdownJobs = new(StringComparer.Ordinal);
    private static readonly List<StdioSession> retained = [];
    private long lastCall;
    private bool expectedRpcIdRejection;
    private bool closing;
    private Task? sdkDisposeTask;
    private Task<ClientCompletionDetails>? sdkCompletionTask;
    private readonly SemaphoreSlim pace = new(1, 1);
    internal required McpClient Client { get; init; }
    internal string Epoch { get; private set; } = "";
    internal string ImportRoot { get; }
    internal string DataDirectory { get; }
    internal JsonElement Devices { get; private set; }
    internal string EvidenceDirectory => directory;
    private StdioSession(string directory, Schemas schemas, OwnedStdioProcess owned)
    {
        this.directory = directory; this.schemas = schemas; this.owned = owned;
        ImportRoot = Path.Combine(directory, "imports"); DataDirectory = Path.Combine(directory, "data");
    }
    internal static async Task<StdioSession> Open(string root, string label, string dotnet, string server, Schemas schemas,
        string[]? scopes = null, string[]? serials = null, string protocol = "2025-11-25")
    {
        var directory = Path.Combine(root, label); Directory.CreateDirectory(directory);
        var imports = Path.Combine(directory, "imports"); Directory.CreateDirectory(imports);
        var policy = Path.Combine(directory, "policy.json");
        Evidence.Save(policy, new { scopeOwner = "test-owner", scopes = scopes ?? new[] { "devices.read", "scripts.import", "jobs.start", "jobs.read", "jobs.cancel" }, authorizedSerials = serials ?? new[] { "DEMO-001", "DEMO-002" } });
        var owned = new OwnedStdioProcess(directory, dotnet, server, policy, imports);
        using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        McpClient client; Task<McpClient>? creation = null;
        try
        {
            owned.Start(); owned.SaveStart();
            creation = McpClient.CreateAsync(owned.Transport, new McpClientOptions { ProtocolVersion = protocol, InitializationTimeout = TimeSpan.FromSeconds(20), ClientInfo = new() { Name = "matrix-independent-stdio-tests", Version = "1" } }, cancellationToken: deadline.Token);
            client = await creation;
        }
        catch (Exception error)
        {
            Exception? transportError = error;
            while (transportError is not null && transportError is not ClientTransportClosedException) transportError = transportError.InnerException;
            var details = (transportError as ClientTransportClosedException)?.Details;
            JsonElement? processEnd = null;
            var evidenceErrors = new List<string>();
            try { await owned.FinishPipesAsync(callsTerminal: true); processEnd = owned.SaveEnd(sdkTerminal: creation is null || (creation.IsCompleted && owned.OutputEofObserved), callsTerminal: true); }
            catch (Exception cleanup) { evidenceErrors.Add("Owned process cleanup: " + cleanup); }
            var raw = new List<object>();
            var metadata = new Dictionary<string, JsonElement>();
            foreach (var name in new[] { "stdin.bin", "stdout.bin", "stderr.bin", "server-start.json", "owned-process.json" })
            {
                var path = Path.Combine(directory, name);
                try
                {
                    if (!File.Exists(path)) { raw.Add(new { name, exists = false }); continue; }
                    using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    raw.Add(new { name, exists = true, snapshotBytes = stream.Length, sha256 = Convert.ToHexString(SHA256.HashData(stream)), terminalProven = false });
                    if (name is "server-start.json" or "owned-process.json")
                    {
                        stream.Position = 0;
                        using var document = JsonDocument.Parse(stream); metadata[name] = document.RootElement.Clone();
                    }
                }
                catch (Exception evidenceError) { evidenceErrors.Add(name + ": " + evidenceError); }
            }
            Exception primary = error;
            try
            {
                Evidence.Save(Path.Combine(directory, "initialization-failure.json"), new
                {
                    error = error.ToString(), creationStatus = creation?.Status.ToString(), creationTerminal = creation?.IsCompleted,
                    sdkCleanupOwner = true, secondSdkOwnerCleanupAttempted = false, actualProcessOwner = "test owned process",
                    completionDetailsType = details?.GetType().FullName, sdkCompletionError = details?.Exception?.ToString(),
                    processEnd, raw, metadata, evidenceErrors
                });
            }
            catch (Exception evidenceError) { primary = new AggregateException("Initialization and evidence preservation failed.", error, evidenceError); }
            throw new SessionPreconditionFailure("Official session initialization failed; subsequent normal sessions must not repeat this common precondition.", primary);
        }
        var session = new StdioSession(directory, schemas, owned) { Client = client };
        try
        {
            Evidence.Check(client.NegotiatedProtocolVersion == protocol, "Negotiated protocol was not pinned.");
            var listed = await client.ListToolsAsync(new ListToolsRequestParams(), deadline.Token);
            schemas.Protocol("ListToolsResult", Evidence.ProtocolJson(listed));
            var wireList = session.WireResults("tools/list").Single(); session.ValidateToolList(wireList);
            Evidence.Check(listed.Tools.Select(tool => tool.Name).SequenceEqual(Schemas.Names), "Six tool names/order differs from the approved contract.");
            foreach (var tool in listed.Tools)
            {
                schemas.Protocol("Tool", Evidence.ProtocolJson(tool));
                Evidence.Check(tool.InputSchema.GetProperty("type").GetString() == "object" && tool.OutputSchema is { } output && output.GetProperty("type").GetString() == "object", "Tool schema root must explicitly be object: " + tool.Name);
                Evidence.Check(Schemas.Rejects(tool.InputSchema, Evidence.Json(new { unexpected = 1 })) && Schemas.Rejects(tool.OutputSchema!.Value, Evidence.Json(new { ok = true })), "Actual schema failed to reject incomplete/extra-field instances: " + tool.Name);
                session.tools.Add(tool);
            }
            Evidence.Save(Path.Combine(directory, "tools-list.json"), wireList);
            var capabilities = await session.Call("matrix.capabilities", new { });
            session.Epoch = capabilities.GetProperty("serverEpoch").GetString()!;
            session.Devices = await session.Call("matrix.devices.list", new { });
            return session;
        }
        catch (Exception primary)
        {
            try { await session.DisposeAsync(); }
            catch (Exception cleanup) { throw new SessionPreconditionFailure("Common session precondition and cleanup failed.", new AggregateException(primary, cleanup)); }
            throw new SessionPreconditionFailure("Common tools-list/schema/capabilities/devices precondition failed.", primary);
        }
    }
    internal JsonElement Identity(string serial) => Devices.GetProperty("devices").EnumerateArray().Single(row => row.GetProperty("target").GetProperty("serial").GetString() == serial).GetProperty("target").Clone();
    internal T Track<T>(Func<T> create) where T : Task
    {
        lock (pending)
        {
            if (closing) throw new InvalidOperationException("Owned session is closing; no new calls are accepted.");
            var task = create(); pending.Add(task); return task;
        }
    }
    internal async Task<JsonElement> Call(string name, object arguments, string? error = null, bool validInput = true, bool paced = true)
    {
        if (paced)
        {
            await pace.WaitAsync();
            try
            {
                var remaining = 120 - Stopwatch.GetElapsedTime(lastCall).TotalMilliseconds;
                if (remaining > 0) await Task.Delay(TimeSpan.FromMilliseconds(remaining));
                lastCall = Stopwatch.GetTimestamp();
            }
            finally { pace.Release(); }
        }
        var input = arguments is JsonElement element ? element : Evidence.Json(arguments);
        if (validInput) { schemas.Contract(name, false, input); Schemas.Validate(tools.Single(tool => tool.Name == name).InputSchema, input, name + " actual input"); }
        using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var task = Track(() => Client.CallToolAsync(new CallToolRequestParams { Name = name, Arguments = input.EnumerateObject().ToDictionary(field => field.Name, field => field.Value.Clone()) }, deadline.Token).AsTask());
        var response = await task;
        schemas.Protocol("CallToolResult", Evidence.ProtocolJson(response));
        Evidence.Check(response.StructuredContent is not null, "Missing structuredContent: " + name);
        var body = response.StructuredContent!.Value.Clone();
        var text = response.Content.OfType<TextContentBlock>().Single().Text;
        Evidence.Check(JsonElement.DeepEquals(body, JsonDocument.Parse(text).RootElement), "Text and structured content differ.");
        schemas.Contract(name, true, body);
        Schemas.Validate(tools.Single(tool => tool.Name == name).OutputSchema!.Value, body, name + " actual output");
        Evidence.Check(response.IsError == (error is not null) && body.GetProperty("ok").GetBoolean() == (error is null), "isError/ok disagrees with expected outcome: " + name);
        if (error is not null) Evidence.Check(body.GetProperty("error").GetProperty("code").GetString() == error, "Wrong business error: " + body);
        if (name == "matrix.jobs.start" && error is null) lock (acceptedJobs) acceptedJobs.Add(body.GetProperty("jobId").GetString()!);
        lock (pending) File.AppendAllText(Path.Combine(directory, "calls.jsonl"), JsonSerializer.Serialize(new { name, input, expectedError = error, response, bodyBytes = System.Text.Encoding.UTF8.GetByteCount(body.GetRawText()) }) + "\n");
        Evidence.Check(System.Text.Encoding.UTF8.GetByteCount(body.GetRawText()) <= 65536, "Business response exceeded 64KiB.");
        return body;
    }
    internal async Task<JsonElement> RawCall(string name, string arguments, string? rpcId = null)
    {
        using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        using var doc = JsonDocument.Parse("{\"name\":" + JsonSerializer.Serialize(name) + ",\"arguments\":" + arguments + "}");
        var task = Track(() => Client.SendRequestAsync<JsonElement, JsonElement>("tools/call", doc.RootElement.Clone(), requestId: rpcId is null ? default : new RequestId(rpcId), cancellationToken: deadline.Token).AsTask());
        var reply = await task;
        schemas.Protocol("CallToolResult", reply);
        var body = reply.GetProperty("structuredContent"); schemas.Contract(name, true, body);
        Schemas.Validate(tools.Single(tool => tool.Name == name).OutputSchema!.Value, body, name + " raw actual output");
        Evidence.Check(JsonElement.DeepEquals(body, JsonDocument.Parse(reply.GetProperty("content")[0].GetProperty("text").GetString()!).RootElement), "Raw negative structured/text content differs.");
        return reply;
    }
    internal void ExpectShutdown(string jobId) => shutdownJobs.Add(jobId);
    internal void ExpectRpcIdRejection() => expectedRpcIdRejection = true;
    internal async Task<JsonElement> Terminal(string jobId, string? expected = "Completed")
    {
        var clock = Stopwatch.StartNew();
        while (clock.Elapsed < TimeSpan.FromSeconds(12))
        {
            var reply = await Call("matrix.jobs.get", new { serverEpoch = Epoch, jobId, logLimit = 100 });
            if (reply.GetProperty("job").GetProperty("state").GetString() is "Completed" or "Failed" or "Cancelled")
            {
                if (expected is not null) Evidence.Check(reply.GetProperty("job").GetProperty("state").GetString() == expected, "Unexpected actual terminal state: " + reply);
                return reply;
            }
        }
        throw new ProductFailure("Job did not reach a terminal state in 12s.");
    }
    public async ValueTask DisposeAsync()
    {
        Task[] originalCalls;
        lock (pending) { closing = true; originalCalls = pending.ToArray(); }
        try { await Task.WhenAll(originalCalls).WaitAsync(TimeSpan.FromSeconds(10)); }
        catch (Exception error) { if (originalCalls.Any(task => !task.IsCompleted)) cleanupErrors.Add("Pending requests: " + error); }
        var callsTerminal = originalCalls.All(task => task.IsCompleted);
        try { await owned.FinishPipesAsync(callsTerminal); }
        catch (Exception error) { cleanupErrors.Add("Owned pipe/process shutdown: " + error); }
        ClientCompletionDetails? completion = null;
        if (callsTerminal && owned.IoTerminal && owned.ProcessTerminal)
        {
            try { sdkDisposeTask = Client.DisposeAsync().AsTask(); await sdkDisposeTask.WaitAsync(TimeSpan.FromSeconds(55)); }
            catch (Exception error) { cleanupErrors.Add("SDK disposal: " + error); }
            try { sdkCompletionTask = Client.Completion; completion = await sdkCompletionTask.WaitAsync(TimeSpan.FromSeconds(5)); }
            catch (Exception error) { cleanupErrors.Add("SDK completion: " + error); }
        }
        else cleanupErrors.Add("SDK disposal not attempted while original calls/process/pipe I/O remain nonterminal.");
        var sdkTerminal = sdkDisposeTask?.IsCompleted == true && sdkCompletionTask?.IsCompleted == true;
        if (!callsTerminal || !sdkTerminal || !owned.IoTerminal || !owned.ProcessTerminal) lock (retained) retained.Add(this);
        JsonElement? processEnd = null, lifecycle = null;
        try
        {
            processEnd = this.owned.SaveEnd(sdkTerminal, callsTerminal);
            if (!expectedRpcIdRejection) Evidence.Check(processEnd.Value.GetProperty("exited").GetBoolean() && processEnd.Value.GetProperty("exitCode").GetInt32() == 0 && processEnd.Value.GetProperty("stdinClosed").GetBoolean()
                && processEnd.Value.GetProperty("stdoutEof").GetBoolean() && !processEnd.Value.GetProperty("killed").GetBoolean() && processEnd.Value.GetProperty("pendingPumps").GetInt32() == 0
                && processEnd.Value.GetProperty("errors").GetArrayLength() == 0 && processEnd.Value.GetProperty("ownedResourcesDisposed").GetBoolean(), "Original owned server/pipes did not drain naturally.");
            lifecycle = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(DataDirectory, "mcp-lifecycle.json"))).RootElement.Clone();
            var life = lifecycle.Value;
            if (!expectedRpcIdRejection) Evidence.Check(life.GetProperty("version").GetInt32() == 1 && life.GetProperty("serverEpoch").GetString() == Epoch && life.GetProperty("processId").GetInt32() == processEnd.Value.GetProperty("pid").GetInt32()
                && life.GetProperty("mode").GetString() == "demo" && life.GetProperty("stdinEof").GetBoolean() && life.GetProperty("handlerDrained").GetBoolean() && life.GetProperty("runtimeDisposed").GetBoolean()
                && life.GetProperty("cleanExit").GetBoolean() && life.GetProperty("pendingProtocolCredits").GetInt32() == 0, "Server's actual EOF/ownership report does not prove natural drain.");
            if (!expectedRpcIdRejection) foreach (var field in new[] { "inputFailure", "outputFailure", "protocolFailure", "failure" }) Evidence.Check(life.GetProperty(field).ValueKind == JsonValueKind.Null, "Server lifecycle failure: " + field);
            Evidence.Check(life.GetProperty("stagingCleanupFailures").GetInt32() == 0, "Server retained a staging cleanup failure.");
            foreach (var field in new[] { "inputIoTerminal", "outputIoTerminal", "ownedJobsTerminal" }) Evidence.Check(life.GetProperty(field).GetBoolean(), "Server retained nonterminal owned I/O or jobs: " + field);
            if (expectedRpcIdRejection)
            {
                Evidence.Check(processEnd.Value.GetProperty("exited").GetBoolean() && processEnd.Value.GetProperty("exitCode").GetInt32() != 0 && !processEnd.Value.GetProperty("killed").GetBoolean()
                    && processEnd.Value.GetProperty("stdoutEof").GetBoolean() && processEnd.Value.GetProperty("pendingPumps").GetInt32() == 0 && processEnd.Value.GetProperty("errors").GetArrayLength() == 0
                    && processEnd.Value.GetProperty("ownedResourcesDisposed").GetBoolean(), "Expected RPC-id fault did not drain the owned server naturally.");
                Evidence.Check(life.GetProperty("version").GetInt32() == 1 && life.GetProperty("serverEpoch").GetString() == Epoch && life.GetProperty("processId").GetInt32() == processEnd.Value.GetProperty("pid").GetInt32()
                    && life.GetProperty("mode").GetString() == "demo" && life.GetProperty("handlerDrained").GetBoolean() && life.GetProperty("runtimeDisposed").GetBoolean()
                    && !life.GetProperty("cleanExit").GetBoolean() && life.GetProperty("pendingProtocolCredits").GetInt32() == 0 && life.GetProperty("outputFailure").ValueKind == JsonValueKind.Null
                    && (life.GetProperty("inputFailure").ValueKind != JsonValueKind.Null || life.GetProperty("protocolFailure").ValueKind != JsonValueKind.Null), "RPC-id rejection lacks actual fault/owned drain evidence.");
                Evidence.Check(completion is not null && sdkTerminal, "Expected RPC-id fault has no terminal SDK stream session.");
            }
            var owned = life.GetProperty("ownedJobs").EnumerateArray().ToArray();
            Evidence.Check(acceptedJobs.SetEquals(owned.Select(job => job.GetProperty("jobId").GetString()!)), "Lifecycle has missing or unobserved accepted jobs.");
            foreach (var job in owned)
            {
                Evidence.Check(job.GetProperty("terminal").GetBoolean() && job.GetProperty("state").GetString() is "Completed" or "Cancelled" or "Failed", "Accepted job did not drain to a real terminal state.");
                if (shutdownJobs.Contains(job.GetProperty("jobId").GetString()!)) Evidence.Check(job.GetProperty("state").GetString() == "Cancelled" && job.GetProperty("terminalReason").GetString() == "shutdown", "EOF did not cancel the accepted active job.");
            }
            if (!expectedRpcIdRejection) Evidence.Check(completion is { Exception: null } && sdkTerminal, "SDK stream session did not complete cleanly.");
            ValidateRaw();
        }
        catch (Exception error) { cleanupErrors.Add("Evidence/lifecycle: " + error); }
        Evidence.Save(Path.Combine(directory, "session-end.json"), new { completionType = completion?.GetType().FullName, sdkCompletionError = completion?.Exception?.ToString(),
            processEnd, lifecycle, expectedRpcIdRejection, sdkDisposePending = sdkDisposeTask is { IsCompleted: false }, sdkDisposeStatus = sdkDisposeTask?.Status.ToString(),
            sdkCompletionPending = sdkCompletionTask is { IsCompleted: false }, sdkCompletionStatus = sdkCompletionTask?.Status.ToString(), pendingRequests = pending.Count(task => !task.IsCompleted), cleanupErrors });
        if (callsTerminal && sdkTerminal) pace.Dispose();
        if (cleanupErrors.Count != 0) throw new AggregateException("CLEANUP-FAIL", cleanupErrors.Select(message => new InvalidOperationException(message)));
    }
    private void ValidateRaw()
    {
        var handshake = false;
        var requests = Evidence.SnapshotLines(Path.Combine(directory, "stdin.bin")).Where(line => !string.IsNullOrWhiteSpace(line)).Select(line =>
        {
            using var document = JsonDocument.Parse(line); return document.RootElement.Clone();
        }).ToArray();
        foreach (var line in File.ReadLines(Path.Combine(directory, "stdout.bin")).Where(line => !string.IsNullOrWhiteSpace(line)))
        {
            Evidence.Check(System.Text.Encoding.UTF8.GetByteCount(line) <= 1024 * 1024, "Wire response exceeded 1MiB.");
            using var message = JsonDocument.Parse(line);
            Evidence.Check(message.RootElement.GetProperty("jsonrpc").GetString() == "2.0", "Non-protocol stdout pollution.");
            if (message.RootElement.TryGetProperty("result", out var result) && result.ValueKind == JsonValueKind.Object && result.TryGetProperty("protocolVersion", out var version)) handshake |= version.GetString() == "2025-11-25";
            if (!message.RootElement.TryGetProperty("result", out result) || !message.RootElement.TryGetProperty("id", out var id)) continue;
            var request = requests.First(row => row.TryGetProperty("id", out var requestId) && JsonElement.DeepEquals(requestId, id));
            if (request.GetProperty("method").GetString() == "tools/list") ValidateToolList(result);
            if (request.GetProperty("method").GetString() == "tools/call") schemas.Protocol("CallToolResult", result);
        }
        Evidence.Check(handshake, "Raw initialize result lacks pinned protocol proof.");
    }
    private JsonElement[] WireResults(string method)
    {
        var requests = Evidence.SnapshotLines(Path.Combine(directory, "stdin.bin")).Select(line =>
        {
            using var document = JsonDocument.Parse(line); return document.RootElement.Clone();
        }).Where(row => row.TryGetProperty("id", out _) && row.GetProperty("method").GetString() == method).ToArray();
        var results = Evidence.SnapshotLines(Path.Combine(directory, "stdout.bin")).Select(line =>
        {
            using var document = JsonDocument.Parse(line); return document.RootElement.Clone();
        }).Where(row => row.TryGetProperty("id", out var replyId) && requests.Any(request => JsonElement.DeepEquals(request.GetProperty("id"), replyId)))
            .Select(row => row.GetProperty("result").Clone()).ToArray();
        Evidence.Check(results.Length > 0, "Actual raw result is absent for the matching stdin request id: " + method);
        return results;
    }
    private void ValidateToolList(JsonElement result)
    {
        schemas.Protocol("ListToolsResult", result);
        foreach (var tool in result.GetProperty("tools").EnumerateArray()) schemas.Protocol("Tool", tool);
    }
}
