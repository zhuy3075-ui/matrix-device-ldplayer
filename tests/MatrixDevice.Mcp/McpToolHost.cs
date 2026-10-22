using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using MatrixDevice.Core;
using MatrixDevice.Features;
using MatrixDevice.Host;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace MatrixDevice.Mcp;

internal sealed class McpToolHost(MatrixRuntime runtime, McpPolicy policy, ImportedScriptStore scripts, Guid epoch)
{
    internal static readonly string[] Names = ["matrix.capabilities", "matrix.devices.list", "matrix.scripts.import", "matrix.jobs.start", "matrix.jobs.get", "matrix.jobs.cancel"];
    private static readonly string[] Types = ["Capabilities", "DevicesList", "ScriptImport", "JobStart", "JobGet", "JobCancel"];
    private static readonly string[] Descriptions =
    [
        "查看本进程能力、授权范围与限额；真实设备写入尚未启用。",
        "列出授权目录中的设备与当前身份，不扫描网络、不启动模拟器。",
        "校验并冻结授权导入根中的脚本包，只创建引用，不执行脚本。",
        "按防重 operationId 向一个明确目标接纳脚本作业，返回原调度器 JobId。",
        "按 JobId 读取原任务状态、实际结果及有界日志。",
        "仅请求取消指定 JobId，不影响其他任务，不回滚已经发送的操作。"
    ];
    private readonly OperationLedger ledger = new(runtime, policy, scripts, epoch);
    private readonly object handlerGate = new();
    private int handlers;
    private bool stopping;
    private TaskCompletionSource? drained;
    private static readonly JsonObject Definitions = ReadDefinitions();
    internal void StopAccepting() { lock (handlerGate) stopping = true; ledger.StopAccepting(); scripts.StopAccepting(); }

    private static JsonObject ReadDefinitions()
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("MatrixDevice.Mcp.Schemas.json")!;
        return JsonNode.Parse(stream)!["$defs"]!.AsObject();
    }

    internal ListToolsResult ListTools()
    {
        var tools = new List<Tool>();
        for (var i = 0; i < Names.Length; i++)
        {
            JsonElement Schema(string suffix) => JsonSerializer.SerializeToElement(new JsonObject
            {
                ["type"] = "object", ["$ref"] = "#/$defs/" + Types[i] + suffix,
                ["$defs"] = Definitions.DeepClone(), ["$schema"] = "https://json-schema.org/draft/2020-12/schema"
            });
            tools.Add(new Tool { Name = Names[i], Description = Descriptions[i], InputSchema = Schema("Input"), OutputSchema = Schema("Output"), Annotations = new ToolAnnotations { ReadOnlyHint = i is 0 or 1 or 4, DestructiveHint = i is 3 or 5, IdempotentHint = i is not 2 } });
        }
        return new() { Tools = tools };
    }

    internal async ValueTask<CallToolResult> CallAsync(RequestContext<CallToolRequestParams> context, CancellationToken requestToken)
    {
        var parameters = context.Params ?? throw new McpProtocolException("Tool call parameters are required.", McpErrorCode.InvalidParams);
        if (!Names.Contains(parameters.Name, StringComparer.Ordinal)) throw new McpProtocolException("Unknown tool.", McpErrorCode.InvalidParams);
        lock (handlerGate)
        {
            if (stopping) return Result(Error(epoch, new ToolFailure("CANCELLED", "Server is stopping.")));
            handlers++;
        }
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(requestToken);
        deadline.CancelAfter(TimeSpan.FromSeconds(30));
        try
        {
            var argumentsBytes = JsonSerializer.SerializeToUtf8Bytes(parameters.Arguments ?? new Dictionary<string, JsonElement>(), StrictJson.Options);
            if (argumentsBytes.Length > 65536) throw new ToolFailure("CAPACITY", "Tool arguments exceed 65536 serialized UTF-8 bytes.");
            using var validated = StrictJson.Parse(argumentsBytes, 32);
            var input = validated.RootElement;
            deadline.Token.ThrowIfCancellationRequested();
            JsonObject body;
            switch (parameters.Name)
            {
                case "matrix.capabilities":
                    StrictJson.Object(input, []);
                    var limits = new JsonObject();
                    foreach (var item in Definitions["Limits"]!["properties"]!.AsObject()) limits[item.Key] = item.Value!["const"]!.DeepClone();
                    body = StrictJson.Node(new
                    {
                        ok = true, serverEpoch = epoch, contractVersion = "matrix-mcp/1", protocolVersion = "2025-11-25", mode = "demo",
                        scopes = policy.Scopes, authorizedSerials = policy.Serials, realWritesEnabled = false,
                        supportedTools = Names, supportedActions = new[] { "launch", "launchapp", "wait", "sleep", "click", "swipe" },
                        unsupportedCapabilities = new[] { "real-writes", "http", "capture", "host-exec", "instance-lifecycle", "apk-management", "restart-resume", "protocol-tasks" },
                        limits, retention = "process-lifetime; terminal-jobs-oldest-completion-100; no-restart-resume"
                    });
                    break;
                case "matrix.devices.list":
                    StrictJson.Object(input, []); policy.Require("devices.read");
                    body = StrictJson.Node(new
                    {
                        ok = true, serverEpoch = epoch, mode = "demo", snapshotUtc = DateTimeOffset.UtcNow,
                        devices = runtime.Devices.Snapshot.Where(d => policy.Serials.Contains(d.Serial, StringComparer.Ordinal)).Select(d => new
                        {
                            target = new { serial = d.Serial, generation = d.Generation.ToString(CultureInfo.InvariantCulture), transportId = d.TransportId },
                            instanceIndex = (int?)null, displayName = StrictJson.Short(d.Name, 256).Text, online = d.Online,
                            writeAllowed = d.Online && policy.Scopes.Contains("jobs.start", StringComparer.Ordinal)
                        }).ToArray()
                    });
                    break;
                case "matrix.scripts.import":
                    StrictJson.Object(input, ["serverEpoch", "relativePath", "sha256"], "serverEpoch", "relativePath", "sha256");
                    policy.Require("scripts.import"); ledger.RequireEpoch(input);
                    body = (await scripts.ImportAsync(epoch, StrictJson.String(input, "relativePath", 4096), StrictJson.String(input, "sha256", 64), deadline.Token).ConfigureAwait(false)).Response;
                    break;
                case "matrix.jobs.start": body = ledger.Start(input, deadline.Token); break;
                case "matrix.jobs.get":
                    StrictJson.Object(input, ["serverEpoch", "jobId", "logLimit"], "serverEpoch", "jobId");
                    policy.Require("jobs.read"); ledger.RequireEpoch(input);
                    body = GetJob(StrictJson.Uuid(input, "jobId"), StrictJson.Integer(input, "logLimit", 0, 100, 50));
                    break;
                case "matrix.jobs.cancel":
                    StrictJson.Object(input, ["serverEpoch", "jobId"], "serverEpoch", "jobId");
                    policy.Require("jobs.cancel"); ledger.RequireEpoch(input);
                    body = ledger.Cancel(StrictJson.Uuid(input, "jobId"));
                    break;
                default: throw new InvalidOperationException();
            }
            return Result(body);
        }
        catch (Exception error)
        {
            if (error is OperationCanceledException && !requestToken.IsCancellationRequested) error = new ToolFailure("DEADLINE_EXCEEDED", "Tool request exceeded its 30-second budget before completing.");
            return Result(Error(epoch, error));
        }
        finally
        {
            lock (handlerGate) { handlers--; if (handlers == 0) drained?.TrySetResult(); }
        }
    }

    private JsonObject GetJob(Guid id, int logLimit)
    {
        var owned = ledger.Get(id);
        var status = runtime.Scripts.GetJob(id) ?? throw new ToolFailure("UNKNOWN_REFERENCE", "Job is not retained.");
        var allLogs = runtime.Scripts.GetJobLogs(id);
        var error = status.Error is null ? (Text: (string?)null, Truncated: false) : StrictJson.Short(status.Error, 2048);
        var logs = allLogs.TakeLast(logLimit).Select(entry =>
        {
            var message = StrictJson.Short(entry.Message, 2048);
            return StrictJson.Node(new { timestampUtc = entry.Timestamp, message = message.Text, truncated = message.Truncated });
        }).ToList();
        var body = StrictJson.Node(new
        {
            ok = true, serverEpoch = epoch, snapshotUtc = DateTimeOffset.UtcNow,
            job = new
            {
                jobId = id, operationId = owned.OperationId, target = OperationLedger.Target(owned.Target), scriptRef = owned.Script.Reference,
                sha256 = owned.Script.Sha256, packageId = status.PackageId, packageVersion = status.PackageVersion,
                state = status.State.ToString(), stepIndex = status.StepIndex, stepCount = status.StepCount,
                startedUtc = status.StartedUtc, endedUtc = status.EndedUtc, error = error.Text, result = status.Result,
                errorTruncated = error.Truncated, resultTruncatedForResponse = false, terminalReason = OperationLedger.Reason(owned, status)
            },
            availableLogs = allLogs.Count, logWindowLimit = 500, omittedLogCount = allLogs.Count - logs.Count, logs
        });
        while (Bytes(body) > 65536 && body["logs"]!.AsArray().Count > 0)
        {
            body["logs"]!.AsArray().RemoveAt(0);
            body["omittedLogCount"] = allLogs.Count - body["logs"]!.AsArray().Count;
        }
        var originalResult = status.Result;
        if (Bytes(body) > 65536 && originalResult is not null)
        {
            var runes = originalResult.EnumerateRunes().ToArray(); var low = 0; var high = runes.Length;
            body["job"]!["resultTruncatedForResponse"] = true;
            while (low < high)
            {
                var mid = (low + high + 1) / 2;
                body["job"]!["result"] = string.Concat(runes.Take(mid).Select(r => r.ToString())) + "[MCP响应预算截断]";
                if (Bytes(body) <= 65536) low = mid; else high = mid - 1;
            }
            body["job"]!["result"] = string.Concat(runes.Take(low).Select(r => r.ToString())) + "[MCP响应预算截断]";
        }
        return body;
    }

    internal static int Bytes(JsonObject body) => JsonSerializer.SerializeToUtf8Bytes(body, StrictJson.Options).Length;
    internal static CallToolResult Result(JsonObject body)
    {
        if (Bytes(body) > 65536) body = Error(body["serverEpoch"]!.GetValue<Guid>(), new ToolFailure("CAPACITY", "Business response exceeds 65536 serialized bytes."));
        var text = JsonSerializer.Serialize(body, StrictJson.Options);
        return new() { IsError = !body["ok"]!.GetValue<bool>(), StructuredContent = JsonSerializer.SerializeToElement(body, StrictJson.Options), Content = [new TextContentBlock { Text = text }] };
    }

    internal static JsonObject Error(Guid epoch, Exception error)
    {
        var code = error switch
        {
            ToolFailure failure => failure.Code,
            OperationCanceledException => "CANCELLED",
            DeviceException device => device.Code switch { 1007 => "INVALID_ARGUMENT", 2001 => "NOT_SUPPORTED", 1003 => "IDENTITY_CHANGED", _ => "INVALID_ARGUMENT" },
            JsonException or ArgumentException or InvalidDataException => "INVALID_ARGUMENT",
            IOException => "INVALID_ARGUMENT",
            _ => "INTERNAL"
        };
        var safeMessage = error is IOException ? "File access failed within the authorized import or staging boundary." : error.Message;
        var message = StrictJson.Short(code == "INTERNAL" ? "Internal MCP operation failure. See local diagnostics." : safeMessage, 2048);
        if (code == "INTERNAL") Console.Error.WriteLine("mcp-internal: " + error.GetType().Name);
        return StrictJson.Node(new { ok = false, serverEpoch = epoch, error = new { code, message = message.Text, errorTruncated = message.Truncated, projectCode = error is DeviceException original ? (int?)original.Code : null, retryable = false } });
    }

    internal async Task<object[]> StopAsync()
    {
        Task wait;
        lock (handlerGate) { stopping = true; wait = handlers == 0 ? Task.CompletedTask : (drained ??= new(TaskCreationOptions.RunContinuationsAsynchronously)).Task; }
        // Stop new submissions first; cancellation follows the original JobId, not serial.
        var drain = ledger.DrainAsync();
        await wait.ConfigureAwait(false);
        return await drain.ConfigureAwait(false);
    }
}
