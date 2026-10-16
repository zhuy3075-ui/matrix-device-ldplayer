using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using MatrixDevice.Features;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;

namespace MatrixDevice.Mcp.Tests;

internal static class Program
{
    private static readonly List<object> results = [];
    private static string output = "", dotnet = "", server = "";
    private static readonly Schemas schemas = new();
    private static int failures;
    private static int sessions;
    private static string? normalPreconditionFailure;
    internal static async Task<int> Main(string[] args)
    {
        try
        {
            var options = Arguments.Parse(args, ["--dotnet", "--server", "--output"]);
            dotnet = Arguments.Absolute(options["--dotnet"], false); server = Arguments.Absolute(options["--server"], false);
            output = options["--output"];
            if (!Path.IsPathFullyQualified(output) || Path.GetFullPath(output) != output || Directory.Exists(output) || File.Exists(output)) throw new ArgumentException("Output must be an absolute fresh directory.");
            Directory.CreateDirectory(output);
            Evidence.Save(Path.Combine(output, "inputs.json"), new { dotnet, dotnetSha256 = Evidence.Hash(dotnet), server, serverSha256 = Evidence.Hash(server), client = typeof(Program).Assembly.Location,
                clientSha256 = Evidence.Hash(typeof(Program).Assembly.Location), sdk = typeof(ModelContextProtocol.Client.McpClient).Assembly.GetName().Version?.ToString(), protocol = "2025-11-25", scope = "OFFLINE-DEMO-STDIO-NOT-GUI-NOT-LIVE" });
            await Run("P00-strict-startup-and-malformed-transport", async () =>
            {
                try { await StartupRejections.RunAsync(output, dotnet, server); }
                finally { if (StartupRejections.UnresolvedOwnership is { } reason) normalPreconditionFailure = reason; }
            });
            await Run("N01-six-tools-wire-schemas-and-real-results", () => Session(async s =>
            {
                Check(new HashSet<string?>(s.Devices.GetProperty("devices").EnumerateArray().Select(row => row.GetProperty("target").GetProperty("serial").GetString())).SetEquals(["DEMO-001", "DEMO-002"]), "Authorized Demo A/B catalog differs.");
                var reference = await Import(s, "parameterized.zip", Parameterized());
                var a = await Start(s, reference, "DEMO-001", overrides: new { app = "com.android.settings" });
                var b = await Start(s, reference, "DEMO-002", overrides: new { app = "com.matrix.demo" });
                var ar = await s.Terminal(Id(a)); var br = await s.Terminal(Id(b));
                var av = Result(ar); var bv = Result(br);
                Check(av.Contains("演示应用已启动：com.android.settings", StringComparison.Ordinal) && bv.Contains("演示应用已启动：com.matrix.demo", StringComparison.Ordinal) && av != bv, "Original Demo launch output is missing or crossed targets.");
                Check(Id(a) != Id(b), "Different targets shared a JobId.");
                var unknown = false;
                using var unknownDeadline = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                var unknownTask = s.Track(() => s.Client.CallToolAsync("matrix.not-a-tool", cancellationToken: unknownDeadline.Token).AsTask());
                try { await unknownTask; } catch (McpException) { unknown = true; }
                Check(unknown, "Unknown tool did not produce a protocol error.");
            }));
            await Run("C01-concurrent-id-conflict-busy-ledger-cancel-isolation", () => Session(async s =>
            {
                var wait = await Import(s, "wait.zip", Package(new() { Action = "wait", DurationMs = 60000 }, Launch("com.android.settings")));
                var quick = await Import(s, "quick.zip", Package(Launch("com.matrix.demo")));
                var operation = Guid.NewGuid().ToString(); var input = StartArguments(s, wait, "DEMO-001", operation);
                var calls = Enumerable.Range(0, 3).Select(_ => s.Call("matrix.jobs.start", input, paced: false)).ToArray();
                var admitted = await Task.WhenAll(calls);
                Check(admitted.Select(Id).Distinct().Count() == 1 && admitted.Count(reply => !reply.GetProperty("replayed").GetBoolean()) == 1, "Same operation was admitted more than once.");
                await s.Call("matrix.jobs.start", StartArguments(s, wait, "DEMO-002", operation), "INVALID_ARGUMENT");
                var busyOperation = Guid.NewGuid().ToString(); var busy = StartArguments(s, quick, "DEMO-001", busyOperation);
                await s.Call("matrix.jobs.start", busy, "BUSY");
                var b = await Start(s, quick, "DEMO-002"); Check(Result(await s.Terminal(Id(b))).Contains("com.matrix.demo", StringComparison.Ordinal), "B did not continue independently.");
                var cancel = await s.Call("matrix.jobs.cancel", new { serverEpoch = s.Epoch, jobId = Id(admitted[0]) });
                Check(cancel.GetProperty("cancelRequested").GetBoolean(), "A cancellation was not requested.");
                var stopped = await s.Terminal(Id(admitted[0]), "Cancelled"); Check(stopped.GetProperty("job").GetProperty("result").ValueKind == JsonValueKind.Null, "Cancelled wait ran its following launch.");
                await s.Call("matrix.jobs.start", busy, "BUSY");
                var next = await Start(s, wait, "DEMO-001");
                var oldCancel = await s.Call("matrix.jobs.cancel", new { serverEpoch = s.Epoch, jobId = Id(admitted[0]) });
                Check(!oldCancel.GetProperty("cancelRequested").GetBoolean() && oldCancel.GetProperty("terminal").GetBoolean(), "Old job was re-cancelled as current work.");
                var current = await s.Call("matrix.jobs.get", new { serverEpoch = s.Epoch, jobId = Id(next) });
                Check(current.GetProperty("job").GetProperty("state").GetString() == "Running", "Old JobId cancellation stopped the successor.");
                await s.Call("matrix.jobs.cancel", new { serverEpoch = s.Epoch, jobId = Id(next) }); await s.Terminal(Id(next), "Cancelled");
            }));
            await Run("E01-input-identity-null-extra-unknown-references", () => Session(async s =>
            {
                var imported = await Import(s, "parameterized.zip", Parameterized());
                foreach (var change in new Action<JsonObject>[]
                {
                    node => node["unexpected"] = 1,
                    node => node.Remove("serial"),
                    node => node["operationId"] = "bad-uuid",
                    node => node["expectedIdentity"]!["generation"] = "9223372036854775808",
                    node => node["expectedIdentity"]!["generation"] = 1,
                    node => node["expectedIdentity"]!["transportId"] = 1,
                    node => node["timeoutMs"] = 999
                })
                {
                    var input = JsonNode.Parse(Evidence.Json(StartArguments(s, imported, "DEMO-001")).GetRawText())!.AsObject(); change(input);
                    await s.Call("matrix.jobs.start", Evidence.Json(input), "INVALID_ARGUMENT", validInput: false);
                }
                var stale = JsonNode.Parse(Evidence.Json(StartArguments(s, imported, "DEMO-001")).GetRawText())!.AsObject(); stale["expectedIdentity"]!["generation"] = "999999";
                await s.Call("matrix.jobs.start", Evidence.Json(stale), "IDENTITY_CHANGED");
                await s.Call("matrix.jobs.start", StartArguments(s, imported, "DEMO-001", overrides: new { app = (string?)null }), "INVALID_ARGUMENT");
                await s.Call("matrix.jobs.get", new { serverEpoch = s.Epoch, jobId = Guid.NewGuid().ToString() }, "UNKNOWN_REFERENCE");
                await s.Call("matrix.jobs.cancel", new { serverEpoch = s.Epoch, jobId = Guid.NewGuid().ToString() }, "UNKNOWN_REFERENCE");
                var badRef = JsonNode.Parse(Evidence.Json(StartArguments(s, imported, "DEMO-001")).GetRawText())!.AsObject(); badRef["scriptRef"] = Guid.NewGuid().ToString();
                await s.Call("matrix.jobs.start", Evidence.Json(badRef), "UNKNOWN_REFERENCE");
                var duplicate = await s.RawCall("matrix.jobs.get", "{\"serverEpoch\":" + JsonSerializer.Serialize(s.Epoch) + ",\"jobId\":\"00000000-0000-4000-8000-000000000001\",\"jobId\":\"00000000-0000-4000-8000-000000000002\"}");
                Check(duplicate.GetProperty("isError").GetBoolean() && duplicate.GetProperty("structuredContent").GetProperty("error").GetProperty("code").GetString() == "INVALID_ARGUMENT", "Duplicate property did not reach strict rejection.");
                using (var last = JsonDocument.Parse(Evidence.SnapshotLines(Path.Combine(s.EvidenceDirectory, "stdin.bin")).Last()))
                    Check(last.RootElement.GetProperty("params").GetProperty("arguments").EnumerateObject().Count(field => field.Name == "jobId") == 2, "SDK did not preserve both duplicate properties on the actual wire.");
                Evidence.Save(Path.Combine(s.EvidenceDirectory, "duplicate-response.json"), duplicate);
                var valid = await Start(s, imported, "DEMO-001", overrides: new { app = "com.android.settings" }); await s.Terminal(Id(valid));
            }));
            await Run("E04-completed-RPC-id-reuse-rejects-own-connection", () => Session(async s =>
            {
                const string rpcId = "mcp-independent-completed-id";
                await s.RawCall("matrix.capabilities", "{}", rpcId);
                Exception? rejected = null;
                try { await s.RawCall("matrix.capabilities", "{}", rpcId); }
                catch (Exception error) { rejected = error; }
                var sent = WireMessages(Path.Combine(s.EvidenceDirectory, "stdin.bin")).Where(message => message.TryGetProperty("id", out var id) && id.ValueKind == JsonValueKind.String && id.GetString() == rpcId && message.GetProperty("method").GetString() == "tools/call").ToArray();
                if (sent.Length != 2) throw new SessionNotRun("Official public RequestId API did not emit both same-id requests; product lifetime-id rejection is NOT-RUN.");
                if (rejected is McpException or IOException) s.ExpectRpcIdRejection();
                Check(rejected is McpException or IOException, "Completed RPC-id reuse did not stop the actual connection with a protocol/transport error.");
                var replies = WireMessages(Path.Combine(s.EvidenceDirectory, "stdout.bin")).Where(message => message.TryGetProperty("id", out var id) && id.ValueKind == JsonValueKind.String && id.GetString() == rpcId).ToArray();
                Check(replies.Length == 1, "Reused completed RPC-id received a second response instead of a bounded connection rejection.");
                Check(string.Join("\n", Evidence.SnapshotLines(Path.Combine(s.EvidenceDirectory, "stderr.bin"))).Contains("JSON-RPC id reuse", StringComparison.Ordinal), "RPC-id rejection lacks its actual typed-id fault diagnostic.");
                Evidence.Save(Path.Combine(s.EvidenceDirectory, "rpc-id-reuse.json"), new { rpcId, sent, replies, rejection = rejected!.ToString() });
            }));
            await Run("B01-paths-bad-hash-package-and-frozen-source", () => Session(async s =>
            {
                var package = Package(Launch("com.android.settings"));
                var reference = await Import(s, "source.zip", package); var source = Path.Combine(s.ImportRoot, "source.zip");
                var sha = reference.GetProperty("sha256").GetString();
                foreach (var path in new[] { source, "../source.zip", "./source.zip", "source.zip:stream", "sub//source.zip", "\\\\localhost\\share\\source.zip", "missing.zip" })
                    await s.Call("matrix.scripts.import", new { serverEpoch = s.Epoch, relativePath = path, sha256 = sha }, "INVALID_ARGUMENT");
                await s.Call("matrix.scripts.import", new { serverEpoch = s.Epoch, relativePath = "source.zip", sha256 = new string('0', 64) }, "INVALID_ARGUMENT");
                File.WriteAllText(Path.Combine(s.ImportRoot, "plain.json"), "{\"not\":\"zip\"}");
                await s.Call("matrix.scripts.import", new { serverEpoch = s.Epoch, relativePath = "plain.json", sha256 = Evidence.Hash(Path.Combine(s.ImportRoot, "plain.json")) }, "INVALID_ARGUMENT");
                ScriptPackageService.Save(source, Package(Launch("com.matrix.demo")));
                var result = await Start(s, reference, "DEMO-001");
                Check(result.GetProperty("sha256").GetString() == sha && Result(await s.Terminal(Id(result))).Contains("com.android.settings", StringComparison.Ordinal), "Frozen import followed modified source bytes.");
                Check(File.Exists(source), "Importer deleted the source.");
            }));
            await Run("B02-policy-A-only-denies-existing-demo-B", () => Session(async s =>
            {
                Check(s.Devices.GetProperty("devices").GetArrayLength() == 1 && s.Identity("DEMO-001").GetProperty("serial").GetString() == "DEMO-001", "A-only policy exposed B.");
                var reference = await Import(s, "one.zip", Package(Launch("com.android.settings")));
                await s.Call("matrix.jobs.start", new { serverEpoch = s.Epoch, operationId = Guid.NewGuid().ToString(), scriptRef = reference.GetProperty("scriptRef").GetString(), serial = "DEMO-002", expectedIdentity = new { generation = "1", transportId = (uint?)null } }, "UNAUTHORIZED");
                await s.Terminal(Id(await Start(s, reference, "DEMO-001")));
            }, serials: ["DEMO-001"]));
            await Run("B03-missing-start-scope-is-not-import-authority", () => Session(async s =>
            {
                var reference = await Import(s, "one.zip", Package(Launch("com.android.settings")));
                await s.Call("matrix.jobs.start", StartArguments(s, reference, "DEMO-001"), "UNAUTHORIZED");
            }, scopes: ["devices.read", "scripts.import", "jobs.read", "jobs.cancel"]));
            await Run("F01-deadline-and-EOF-owned-active-job", () => Session(async s =>
            {
                var reference = await Import(s, "wait.zip", Package(new() { Action = "wait", DurationMs = 60000 }, Launch("com.android.settings")));
                var start = await Start(s, reference, "DEMO-001", timeout: 1000);
                var terminal = await s.Terminal(Id(start), "Cancelled");
                Check(terminal.GetProperty("job").GetProperty("terminalReason").GetString() == "deadline" && terminal.GetProperty("job").GetProperty("result").ValueKind == JsonValueKind.Null, "Deadline did not stop the following launch.");
                var active = await Start(s, reference, "DEMO-002");
                var running = await s.Call("matrix.jobs.get", new { serverEpoch = s.Epoch, jobId = Id(active) });
                Check(running.GetProperty("job").GetProperty("state").GetString() == "Running", "EOF fixture has no accepted live job.");
                Evidence.Save(Path.Combine(s.EvidenceDirectory, "eof-active.json"), new { jobId = Id(active), before = running });
                s.ExpectShutdown(Id(active));
                // Session disposal closes actual stdin; the server lifecycle report must prove drain.
            }));
            await Run("T01-script-reference-capacity", () => Session(async s =>
            {
                for (var index = 0; index < 16; index++) await Import(s, index + ".zip", Package(Launch("com.android.settings")) with { Manifest = Manifest("distinct-" + index) });
                await Import(s, "seventeen.zip", Package(Launch("com.matrix.demo")), "CAPACITY");
            }));
            await Run("T09-70KiB-schema-valid-override-does-not-reserve-operation", () => Session(async s =>
            {
                var package = Parameterized();
                package.Parameters.Add(new() { Key = "unused", Type = "string", DefaultValue = Evidence.Json("default") });
                var reference = await Import(s, "unbound-parameter.zip", package);
                var operation = Guid.NewGuid().ToString();
                await s.Call("matrix.jobs.start", StartArguments(s, reference, "DEMO-001", operation, new { unused = new string('x', 70 * 1024) }), "CAPACITY");
                var request = WireMessages(Path.Combine(s.EvidenceDirectory, "stdin.bin")).Single(message => message.TryGetProperty("method", out var method) && method.GetString() == "tools/call"
                    && message.GetProperty("params").TryGetProperty("arguments", out var input) && input.TryGetProperty("operationId", out var id) && id.GetString() == operation);
                var argumentsBytes = System.Text.Encoding.UTF8.GetByteCount(request.GetProperty("params").GetProperty("arguments").GetRawText());
                var wireBytes = System.Text.Encoding.UTF8.GetByteCount(request.GetRawText());
                Check(argumentsBytes > 65536 && wireBytes < 1024 * 1024, "Oversize business-body fixture did not actually cross 64KiB within the 1MiB wire limit.");
                var accepted = await Start(s, reference, "DEMO-001", operation, new { unused = "short" });
                Check(!accepted.GetProperty("replayed").GetBoolean(), "Rejected oversize body consumed the operation ledger key.");
                Check(Result(await s.Terminal(Id(accepted))).Contains("com.android.settings", StringComparison.Ordinal), "Short legal follow-up did not reach the original Demo engine.");
                Evidence.Save(Path.Combine(s.EvidenceDirectory, "oversize-business-body.json"), new { operation, argumentsBytes, wireBytes, accepted });
            }));
            await Run("T02-operation-ledger-100-and-replay-at-capacity", () => Session(async s =>
            {
                var reference = await Import(s, "wait.zip", Package(new ScriptStep { Action = "wait", DurationMs = 60000 }));
                var key = Guid.NewGuid().ToString(); var input = StartArguments(s, reference, "DEMO-001", key); var accepted = await s.Call("matrix.jobs.start", input);
                for (var index = 1; index < 100; index++) await s.Call("matrix.jobs.start", StartArguments(s, reference, "DEMO-001"), "BUSY");
                await s.Call("matrix.jobs.start", StartArguments(s, reference, "DEMO-002"), "CAPACITY");
                var replay = await s.Call("matrix.jobs.start", input); Check(Id(replay) == Id(accepted) && replay.GetProperty("replayed").GetBoolean(), "Capacity blocked accepted-key replay.");
                await s.Call("matrix.jobs.cancel", new { serverEpoch = s.Epoch, jobId = Id(accepted) }); await s.Terminal(Id(accepted), "Cancelled");
            }));
            await Run("T03-500-log-window-and-real-Unicode-result-budget", () => Session(async s =>
            {
                var package = Package(Enumerable.Range(0, 600).Select(_ => Launch("com.android.settings")).ToArray());
                var reference = await Import(s, "many.zip", package); var accepted = await Start(s, reference, "DEMO-001"); var terminal = await s.Terminal(Id(accepted));
                Check(terminal.GetProperty("availableLogs").GetInt32() == 500 && terminal.GetProperty("logs").GetArrayLength() <= 100 && terminal.GetProperty("omittedLogCount").GetInt32() == 500 - terminal.GetProperty("logs").GetArrayLength(), "500 log window or omitted count is wrong.");
                var withoutLogs = await s.Call("matrix.jobs.get", new { serverEpoch = s.Epoch, jobId = Id(accepted), logLimit = 0 });
                var result = Result(withoutLogs); Check(result.Contains("[输出已截断]", StringComparison.Ordinal) && new System.Text.UTF8Encoding(false, true).GetByteCount(result) <= 16384, "Actual Chinese Demo result was not bounded at a valid Unicode boundary.");
            }));
            await Run("R01-new-process-epoch-rejects-old-request", async () =>
            {
                string? prior = null;
                await Session(s => { prior = s.Epoch; return Task.CompletedTask; });
                await Session(async s =>
                {
                    Check(s.Epoch != prior, "New server reused the prior epoch.");
                    await s.Call("matrix.jobs.get", new { serverEpoch = prior, jobId = Guid.NewGuid().ToString() }, "UNKNOWN_REFERENCE");
                });
            });
            NotRun("E02-transport-generation-change-and-lost-response", "The six tools expose no Demo connection mutation or delivery gate; no test RPC or same-process substitute was introduced.");
            NotRun("F02-cancel-reserved-channel-under-three-imports", "A deterministic external importer blocking boundary is not available; concurrent quick calls do not prove saturation.");
            NotRun("F03-cancel-exact-admission-boundary", "No public pre-admission barrier; pre-cancel token alone cannot prove both sides of the admission race.");
            NotRun("B04-reparse-and-source-replacement-during-copy", "No privileged junction/symlink creation or timing-based race is performed.");
            NotRun("T04-64MiB-staging-and-16MiB-archive-32MiB-extracted-128-entries", "Separate archive/quota fixtures remain required; reference capacity is not a substitute.");
            NotRun("T05-deep-JSON-escape-expansion-and-long-errors", "The separate >1MiB no-newline and half-frame EOF cases cover only those actual preparse paths; depth, escaping and long-error budgets remain untested.");
            NotRun("T06-crash-not-reading-output-and-forced-termination", "Only natural client EOF is exercised, not forced kill or backpressure.");
            NotRun("T07-101st-terminal-job-eviction", "Unreachable through the 100-operation lifetime limit; existing Scheduler tests remain the evidence.");
            NotRun("N02-tap-counter-and-unselected-device-backend-readback", "No readback tool is registered. Original launch-return text is checked, not claimed as node/tap counter proof; A-only policy tests real Demo B authorization.");
            NotRun("R02-real-owner-competition-and-live-1-2", "This suite is strictly Demo and does not construct real runtime, contact ADB or invoke a simulator.");
            NotRun("E03-protocol-mismatch-rejections", "Strict startup is tested separately, but a deliberately mismatched official client protocol remains untested.");
            NotRun("T08-metadata-8KiB-parameter-keys-64KiB-and-non-BMP-response-budget", "Reference/result limits do not replace independent metadata, full parameter list or non-BMP/escaped-output expansion fixtures.");
            NotRun("T10-65536-lifetime-RPC-id-capacity", "Completed-id reuse and 1024-byte id bounds do not replace the separate 65536-entry lifetime-hash stress test.");
        }
        catch (Exception error) { failures++; results.Add(new { caseId = "harness", status = "FIXTURE-FAIL", error = error.ToString() }); }
        finally
        {
            if (Directory.Exists(output)) Evidence.Save(Path.Combine(output, "summary.json"), new { status = failures == 0 ? "PASS-LIMITED" : "FAIL", failures, results, sessions,
                scope = "official-SDK-external-stdio-Demo-only", noGuiOrGuest = true, ownPendingWorkProof = "session-end.json and owned-process.json per session", unimplementedCoverageIsNotPassed = true });
        }
        return failures == 0 ? 0 : 1;
    }
    private static async Task Run(string id, Func<Task> action)
    {
        var watch = Stopwatch.StartNew(); string status; string? error = null;
        try { await action(); status = "PASS-OFFLINE"; }
        catch (Exception failure)
        {
            error = failure.ToString();
            if (failure is SessionNotRun) status = "NOT-RUN";
            else
            {
                failures++;
                status = failure is SessionPreconditionFailure ? "PRECONDITION-FAIL" : failure is ProductFailure or McpException ? "PRODUCT-FAIL"
                    : failure is AggregateException aggregate ? aggregate.Message.StartsWith("CLEANUP-FAIL", StringComparison.Ordinal) ? "CLEANUP-FAIL" : "MIXED-FAIL" : "FIXTURE-FAIL";
            }
        }
        var result = new { caseId = id, status, elapsedMs = watch.Elapsed.TotalMilliseconds, error }; results.Add(result);
        File.AppendAllText(Path.Combine(output, "cases.jsonl"), JsonSerializer.Serialize(result) + "\n"); Console.WriteLine(id + " " + status);
    }
    private static async Task Session(Func<StdioSession, Task> action, string[]? scopes = null, string[]? serials = null)
    {
        if (normalPreconditionFailure is not null) throw new SessionNotRun("Normal sessions stopped after the prior common precondition failure: " + normalPreconditionFailure);
        StdioSession session;
        try { session = await StdioSession.Open(output, "session-" + Interlocked.Increment(ref sessions).ToString("D2"), dotnet, server, schemas, scopes, serials); }
        catch (Exception error)
        {
            normalPreconditionFailure = error.ToString();
            if (error is SessionPreconditionFailure) throw;
            throw new SessionPreconditionFailure("Owned session construction failed; no further normal process may start.", error);
        }
        Exception? primary = null, cleanup = null;
        try { await action(session); } catch (Exception error) { primary = error; }
        try { await session.DisposeAsync(); } catch (Exception error) { cleanup = error; normalPreconditionFailure = "Prior owned-session cleanup failed; no further normal server will be started: " + error; }
        if (primary is not null && cleanup is not null) throw new AggregateException("MIXED-FAIL: primary and cleanup", primary, cleanup);
        if (primary is not null) System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(primary).Throw();
        if (cleanup is not null) System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(cleanup).Throw();
    }
    private static void NotRun(string id, string reason)
    {
        var result = new { caseId = id, status = "NOT-RUN", reason }; results.Add(result);
        File.AppendAllText(Path.Combine(output, "cases.jsonl"), JsonSerializer.Serialize(result) + "\n");
    }
    private static void Check(bool value, string message) => Evidence.Check(value, message);
    private static JsonElement[] WireMessages(string path) => Evidence.SnapshotLines(path).Where(line => !string.IsNullOrWhiteSpace(line)).Select(line =>
    {
        using var document = JsonDocument.Parse(line); return document.RootElement.Clone();
    }).ToArray();
    private static string Id(JsonElement response) => response.GetProperty("jobId").GetString()!;
    private static string Result(JsonElement response) => response.GetProperty("job").GetProperty("result").GetString() ?? "";
    private static ScriptStep Launch(string app) => new() { Action = "launch", Value = app };
    private static ScriptManifest Manifest(string id) => new() { Id = id, Name = "独立MCP测试 " + id, Version = "1.0" };
    private static ScriptPackage Package(params ScriptStep[] steps) => new() { Manifest = Manifest("mcp-test"), Steps = steps.ToList() };
    private static ScriptPackage Parameterized() => Package(Launch("com.android.settings")) with
    {
        Parameters = [new() { Key = "app", Type = "string", DefaultValue = Evidence.Json("com.android.settings") }],
        ExecutionContract = new() { Version = 1, Bindings = [new() { StepIndex = 0, Field = "value", ParameterKey = "app" }] }
    };
    private static async Task<JsonElement> Import(StdioSession session, string name, ScriptPackage package, string? error = null)
    {
        var path = Path.Combine(session.ImportRoot, name); ScriptPackageService.Save(path, package);
        return await session.Call("matrix.scripts.import", new { serverEpoch = session.Epoch, relativePath = name, sha256 = Evidence.Hash(path) }, error);
    }
    private static object StartArguments(StdioSession session, JsonElement reference, string serial, string? operation = null, object? overrides = null, int timeout = 300000)
    {
        var identity = session.Identity(serial);
        var result = new Dictionary<string, object?> { ["serverEpoch"] = session.Epoch, ["operationId"] = operation ?? Guid.NewGuid().ToString(), ["scriptRef"] = reference.GetProperty("scriptRef").GetString(), ["serial"] = serial,
            ["expectedIdentity"] = new { generation = identity.GetProperty("generation").GetString(), transportId = identity.GetProperty("transportId").ValueKind == JsonValueKind.Null ? (uint?)null : identity.GetProperty("transportId").GetUInt32() }, ["timeoutMs"] = timeout };
        if (overrides is not null) result["parameterOverrides"] = overrides;
        return result;
    }
    private static Task<JsonElement> Start(StdioSession session, JsonElement reference, string serial, string? operation = null, object? overrides = null, int timeout = 300000)
        => session.Call("matrix.jobs.start", StartArguments(session, reference, serial, operation, overrides, timeout));
}
