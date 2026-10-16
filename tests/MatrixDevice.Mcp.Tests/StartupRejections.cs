using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace MatrixDevice.Mcp.Tests;

internal static class StartupRejections
{
    internal static string? UnresolvedOwnership { get; private set; }
    private static object? retainedOwnership;
    internal static async Task RunAsync(string output, string dotnet, string server)
    {
        var root = Path.Combine(output, "raw-rejections"); Directory.CreateDirectory(root);
        var failures = new List<Exception>();
        var cases = new[] { "missing-demo", "real", "duplicate-demo", "duplicate-data", "unknown", "missing-value", "bad-policy-json", "bad-policy-field", "existing-data", "oversize-no-newline", "partial-frame-eof", "string-rpc-id-over-1024-utf8" };
        foreach (var name in cases)
        {
            var directory = Path.Combine(root, name); Directory.CreateDirectory(directory);
            if (UnresolvedOwnership is { } stopped)
            {
                var skipped = new { caseId = name, status = "NOT-RUN", reason = stopped, processStarted = false };
                Evidence.Save(Path.Combine(directory, "case.json"), skipped);
                File.AppendAllText(Path.Combine(root, "cases.jsonl"), JsonSerializer.Serialize(skipped) + "\n");
                continue;
            }
            var imports = Path.Combine(directory, "imports"); Directory.CreateDirectory(imports);
            var data = Path.Combine(directory, "profile"); var other = Path.Combine(directory, "other-profile");
            var policy = Path.Combine(directory, "policy.json");
            Evidence.Save(policy, new { scopeOwner = "test-owner", scopes = new[] { "devices.read", "scripts.import", "jobs.start", "jobs.read", "jobs.cancel" }, authorizedSerials = new[] { "DEMO-001", "DEMO-002" } });
            var arguments = new List<string> { "--demo", "--data-dir", data, "--policy", policy, "--import-root", imports };
            var malformed = name is "oversize-no-newline" or "partial-frame-eof" or "string-rpc-id-over-1024-utf8";
            switch (name)
            {
                case "missing-demo": arguments.RemoveAt(0); break;
                case "real": arguments[0] = "--real"; break;
                case "duplicate-demo": arguments.Add("--demo"); break;
                case "duplicate-data": arguments.AddRange(["--data-dir", other]); break;
                case "unknown": arguments.Add("--unknown"); break;
                case "missing-value": arguments.Add("--policy"); break;
                case "bad-policy-json": File.WriteAllText(policy, "{\"scopeOwner\":"); break;
                case "bad-policy-field": File.WriteAllText(policy, "{\"scopeOwner\":\"test-owner\",\"scopes\":[],\"authorizedSerials\":[],\"unexpected\":true}"); break;
                case "existing-data": Directory.CreateDirectory(data); File.WriteAllText(Path.Combine(data, "sentinel.txt"), "owned fixture must stay unchanged"); break;
            }
            var policyHash = Evidence.Hash(policy); var serverHash = Evidence.Hash(server);
            var sentinel = name == "existing-data" ? Evidence.Hash(Path.Combine(data, "sentinel.txt")) : null;
            var payload = name == "oversize-no-newline" ? Enumerable.Repeat((byte)'x', 1024 * 1024 + 1).ToArray()
                : name == "partial-frame-eof" ? Encoding.UTF8.GetBytes("{\"jsonrpc\":\"2.0\",\"id\":1,\"method\":")
                : name == "string-rpc-id-over-1024-utf8" ? Encoding.UTF8.GetBytes("{\"jsonrpc\":\"2.0\",\"id\":\"" + new string('x', 1025) + "\",\"method\":\"tools/list\",\"params\":{}}\n") : [];
            Exception? failure = null; object? processResult = null;
            try
            {
                var result = await Child(directory, dotnet, server, arguments, payload); processResult = result;
                if (result.Killed || !result.Exited || result.PendingStreams != 0 || result.Errors.Count != 0)
                    UnresolvedOwnership = "Raw owned child was killed, nonterminal, or had cleanup errors; no further raw or normal processes may start. " + name;
                Evidence.Check(result.Exited && result.ExitCode != 0 && !result.Killed && result.PendingStreams == 0 && result.Errors.Count == 0, "Raw rejection did not terminate naturally with drained streams.");
                Evidence.Check(new FileInfo(Path.Combine(directory, "stderr.bin")).Length > 0, "Rejected startup/partial transport omitted stderr diagnostics.");
                if (!malformed) Evidence.Check(new FileInfo(Path.Combine(directory, "stdout.bin")).Length == 0, "Rejected startup polluted stdout.");
                else foreach (var line in File.ReadLines(Path.Combine(directory, "stdout.bin")).Where(line => !string.IsNullOrWhiteSpace(line)))
                    using (var message = JsonDocument.Parse(line)) Evidence.Check(message.RootElement.GetProperty("jsonrpc").GetString() == "2.0", "Malformed-input response polluted stdout with non-protocol text.");
                Evidence.Check(Evidence.Hash(server) == serverHash && Evidence.Hash(policy) == policyHash && Directory.GetFileSystemEntries(imports).Length == 0, "Raw negative modified a protected input.");
                if (malformed)
                {
                    using var document = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(data, "mcp-lifecycle.json"))); var life = document.RootElement;
                    Evidence.Check(life.GetProperty("inputFailure").ValueKind != JsonValueKind.Null && life.GetProperty("ownedJobs").GetArrayLength() == 0 && life.GetProperty("handlerDrained").GetBoolean()
                        && life.GetProperty("runtimeDisposed").GetBoolean() && life.GetProperty("pendingProtocolCredits").GetInt32() == 0 && !life.GetProperty("cleanExit").GetBoolean(), "Malformed input lacks actual input-failure/zero-job/drain evidence.");
                    foreach (var field in new[] { "inputIoTerminal", "outputIoTerminal", "ownedJobsTerminal" }) Evidence.Check(life.GetProperty(field).GetBoolean(), "Malformed input retained nonterminal owned work: " + field);
                    Evidence.Check(result.InputAcceptedBytes >= (name == "oversize-no-newline" ? 1024 * 1024 : payload.Length), "Malformed fixture did not send the required boundary bytes.");
                    if (name == "oversize-no-newline") Evidence.Check(File.ReadAllText(Path.Combine(directory, "stderr.bin")).Contains("Stdio request exceeds 1048576 bytes before SDK deserialization", StringComparison.Ordinal), "Oversize negative did not reach the actual pre-SDK byte boundary guard.");
                    if (name == "partial-frame-eof") Evidence.Check(File.ReadAllText(Path.Combine(directory, "stderr.bin")).Contains("Incomplete final stdio message", StringComparison.Ordinal), "Partial-frame negative did not reach the actual incomplete-EOF guard.");
                    if (name == "string-rpc-id-over-1024-utf8") Evidence.Check(File.ReadAllText(Path.Combine(directory, "stderr.bin")).Contains("JSON-RPC string id exceeds 1024 UTF-8 bytes", StringComparison.Ordinal), "Long-id negative did not reach the actual 1024-byte boundary guard.");
                }
                else if (sentinel is null) Evidence.Check(!Directory.Exists(data) && !File.Exists(data) && !Directory.Exists(other) && !File.Exists(other), "Rejected arguments created the target runtime directory.");
                else Evidence.Check(Directory.GetFileSystemEntries(data).Length == 1 && Evidence.Hash(Path.Combine(data, "sentinel.txt")) == sentinel, "Rejected existing profile mutated its sentinel or added runtime files.");
            }
            catch (Exception error)
            {
                failure = error; failures.Add(error);
                if (processResult is null) UnresolvedOwnership = "Raw child did not return ownership evidence; no further raw or normal processes may start. " + name + ": " + error;
            }
            var report = new { caseId = name, status = failure is null ? "PASS-OFFLINE" : failure is ProductFailure ? "PRODUCT-FAIL" : "FIXTURE-FAIL", failure = failure?.ToString(), arguments,
                malformedTransportOnly = malformed, payloadBytes = payload.Length, payloadSha256 = Convert.ToHexString(SHA256.HashData(payload)), process = processResult };
            Evidence.Save(Path.Combine(directory, "case.json"), report);
            File.AppendAllText(Path.Combine(root, "cases.jsonl"), JsonSerializer.Serialize(report) + "\n");
        }
        if (failures.Count > 0) throw new AggregateException("Raw startup/transport rejection subcases failed; originals retained.", failures);
    }
    private sealed record ChildResult(int Pid, long Birth, int? ExitCode, bool Exited, bool Killed, int PendingStreams, long InputAcceptedBytes, string? InputWriteFailure, double ElapsedMs, IReadOnlyList<string> Errors, IReadOnlyList<object> Raw, string? ExitTaskStatus, string[] StreamStatuses, bool ResourcesDisposed);
    private static async Task<ChildResult> Child(string directory, string dotnet, string server, IReadOnlyList<string> arguments, byte[] input)
    {
        var start = new ProcessStartInfo(dotnet) { UseShellExecute = false, CreateNoWindow = true, RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true, WorkingDirectory = directory };
        start.Environment.Clear();
        foreach (var key in new[] { "SystemRoot", "SystemDrive", "TEMP", "TMP", "PATH", "PATHEXT" }) if (Environment.GetEnvironmentVariable(key) is { } value) start.Environment[key] = value;
        start.Environment["PATH"] = Path.GetDirectoryName(dotnet) + Path.PathSeparator + Environment.SystemDirectory;
        start.Environment["DOTNET_ROOT"] = Path.GetDirectoryName(dotnet); start.Environment["DOTNET_NOLOGO"] = "1";
        start.ArgumentList.Add(server); foreach (var value in arguments) start.ArgumentList.Add(value);
        await File.WriteAllBytesAsync(Path.Combine(directory, "stdin.bin"), input);
        var stdout = new FileStream(Path.Combine(directory, "stdout.bin"), FileMode.CreateNew, FileAccess.Write, FileShare.Read, 81920, true);
        var stderr = new FileStream(Path.Combine(directory, "stderr.bin"), FileMode.CreateNew, FileAccess.Write, FileShare.Read, 81920, true);
        var process = new Process { StartInfo = start }; var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(20));
        var clock = Stopwatch.StartNew(); var errors = new List<string>(); var pumps = new List<Task>();
        Task? exit = null; var resourcesDisposed = false;
        var pid = 0; long birth = 0, accepted = 0; int? exitCode = null; var killed = false; var exited = false; string? inputWriteFailure = null;
        try
        {
            if (!process.Start()) throw new IOException("Raw negative child did not start.");
            _ = process.SafeHandle; pid = process.Id; birth = process.StartTime.ToUniversalTime().Ticks;
            pumps.Add(process.StandardOutput.BaseStream.CopyToAsync(stdout)); pumps.Add(process.StandardError.BaseStream.CopyToAsync(stderr));
            var send = Send(); pumps.Add(send);
            exit = process.WaitForExitAsync();
            await exit.WaitAsync(deadline.Token); exited = process.HasExited; exitCode = process.ExitCode;
            await Task.WhenAll(pumps).WaitAsync(TimeSpan.FromSeconds(10));
        }
        catch (Exception error)
        {
            errors.Add(error.ToString());
            if (pid != 0)
            {
                try { if (!process.HasExited) { killed = true; process.Kill(false); } exit ??= process.WaitForExitAsync(); await exit.WaitAsync(TimeSpan.FromSeconds(5)); exited = process.HasExited; exitCode = process.ExitCode; }
                catch (Exception cleanup) { errors.Add("Process cleanup: " + cleanup); }
            }
        }
        finally
        {
            deadline.Cancel();
            try { await Task.WhenAll(pumps).WaitAsync(TimeSpan.FromSeconds(10)); } catch (Exception error) { errors.Add("Stream drain: " + error); }
            var terminal = pumps.All(task => task.IsCompleted) && (pid == 0 || (exited && exit?.IsCompleted == true));
            if (terminal)
            {
                try { await stdout.FlushAsync(); await stderr.FlushAsync(); stdout.Dispose(); stderr.Dispose(); deadline.Dispose(); process.Dispose(); resourcesDisposed = true; }
                catch (Exception error) { errors.Add("Terminal resource disposal: " + error); }
            }
            else
            {
                errors.Add("Original raw child task/pumps remain nonterminal; owned resources retained, not disposed.");
                retainedOwnership = new { process, deadline, stdout, stderr, pumps, exit };
            }
        }
        var raw = new List<object>();
        foreach (var name in new[] { "stdin.bin", "stdout.bin", "stderr.bin" })
        {
            try
            {
                using var stream = new FileStream(Path.Combine(directory, name), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                raw.Add(new { name, bytes = stream.Length, sha256 = Convert.ToHexString(SHA256.HashData(stream)) });
            }
            catch (Exception error) { errors.Add("Raw stream hash: " + error); }
        }
        GC.KeepAlive(retainedOwnership);
        return new(pid, birth, exitCode, exited, killed, pumps.Count(task => !task.IsCompleted), accepted, inputWriteFailure, clock.Elapsed.TotalMilliseconds, errors, raw, exit?.Status.ToString(), pumps.Select(task => task.Status.ToString()).ToArray(), resourcesDisposed);
        async Task Send()
        {
            try
            {
                for (var offset = 0; offset < input.Length; offset += 8192)
                {
                    var count = Math.Min(8192, input.Length - offset);
                    await process.StandardInput.BaseStream.WriteAsync(input.AsMemory(offset, count), deadline.Token); accepted += count;
                }
                await process.StandardInput.BaseStream.FlushAsync(deadline.Token);
            }
            catch (IOException error) { inputWriteFailure = error.ToString(); }
            finally { process.StandardInput.Close(); }
        }
    }
}
