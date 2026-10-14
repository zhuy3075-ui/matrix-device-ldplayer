using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using MatrixDevice.Core;
using MatrixDevice.Features;
using MatrixDevice.Host;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

internal static class MultiInstanceAcceptance
{
    private const string Package = "com.ss.android.ugc.aweme";
    internal sealed record Target(int Index, string Name, string Serial, string Model, string Manufacturer,
        int MemoryMb = 0, int VBoxPid = 0, long VBoxBirth = 0, uint TransportId = 0, int PlayerPid = 0, long PlayerBirth = 0);
    internal sealed record Spec(string RunId, string ConsolePath, string Apk, string ApkSha256, int OriginalPid,
        int OriginalVBoxPid, int ServerPid, long ServerBirth, Target[] Targets, int Minutes = 15, int ProxyPort = 0);
    public static async Task RunAsync(string[] args)
    {
        if (args.Length != 3 || args[1] is not ("prepare" or "workload" or "diagnose")) throw new ArgumentException("--multi-acceptance <spec.json> prepare|workload|diagnose <absolute-output>");
        var spec = JsonSerializer.Deserialize<Spec>(File.ReadAllText(args[0]), JsonDefaults.Options)!;
        Require(Path.IsPathFullyQualified(args[2]) && spec.Targets.Length >= 2 && spec.Targets.All(x => x.Index > 0 && !string.IsNullOrWhiteSpace(x.Serial)), "Only explicit independent targets and absolute output allowed");
        Require(spec.Targets.Select(x => x.Index).Distinct().Count() == spec.Targets.Length && spec.Targets.Select(x => x.Serial).Distinct(StringComparer.Ordinal).Count() == spec.Targets.Length, "Duplicate index/serial");
        Require(spec.Targets.All(x => x.Model.Length is > 0 and <= 64 && x.Manufacturer.Length is > 0 and <= 64
            && !x.Model.StartsWith('-') && !x.Manufacturer.StartsWith('-') && !x.Model.Any(char.IsControl) && !x.Manufacturer.Any(char.IsControl)), "Invalid model/manufacturer strings");
        Require(spec.RunId.All(x => char.IsAsciiLetterOrDigit(x) || x == '-'), "Invalid run ID");
        var output = Path.GetFullPath(args[2]); Directory.CreateDirectory(output);
        await File.WriteAllTextAsync(Path.Combine(output, "run-manifest.json"), JsonSerializer.Serialize(new
        {
            utc = DateTimeOffset.UtcNow, phase = args[1], runtime = Environment.Version.ToString(), specSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(args[0]))),
            assemblies = new[] { typeof(MultiInstanceAcceptance).Assembly, typeof(AdbDeviceBackend).Assembly, typeof(ScriptScheduler).Assembly, typeof(MatrixRuntime).Assembly }
                .Select(assembly => new { file = Path.GetFileName(assembly.Location), sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(assembly.Location))) })
        }, JsonDefaults.Options));
        var journal = new Journal(output);
        var runner = new RecordingRunner(new LdConsoleRunner(spec.ConsolePath, 300), journal);
        await using var ld = new LdPlayerService(runner);
        async Task CheckHost()
        {
            var rows = await ld.RefreshAsync(); var original = rows.Single(x => x.Index == 0);
            Require(original.ProcessId == spec.OriginalPid && original.VBoxProcessId == spec.OriginalVBoxPid, "Original instance host identity changed");
            using var server = Process.GetProcessById(spec.ServerPid);
            Require(server.StartTime.ToUniversalTime().Ticks == spec.ServerBirth && !server.HasExited, "Shared ADB epoch changed; stop without restarting it");
            foreach (var target in spec.Targets)
            {
                var row = rows.Single(x => x.Index == target.Index);
                Require(row.Name == target.Name, "Target name/index mismatch");
                if (target.VBoxPid > 0)
                {
                    Require(row.AndroidStarted && row.VBoxProcessId == target.VBoxPid, "Running target instance identity changed");
                    using var process = Process.GetProcessById(target.VBoxPid);
                    Require(process.StartTime.ToUniversalTime().Ticks == target.VBoxBirth, "Running VBox PID was reused");
                    Require(row.ProcessId == target.PlayerPid, "Target player process changed");
                    using var player = Process.GetProcessById(target.PlayerPid);
                    Require(player.StartTime.ToUniversalTime().Ticks == target.PlayerBirth, "Running player PID was reused");
                }
            }
        }
        await CheckHost();
        if (args[1] == "prepare")
        {
            var prepared = new List<Target>();
            foreach (var target in spec.Targets)
            {
                await journal.Case(target, "cold-profile", async () =>
                {
                    var before = (await ld.RefreshAsync()).Single(x => x.Index == target.Index);
                    if (before.AndroidStarted || before.ProcessId > 0 || before.VBoxProcessId > 0)
                    {
                        await ld.ExecuteAsync(target.Index, "quit", new JsonObject { ["expectedGeneration"] = before.Generation, ["confirmed"] = true });
                        await Wait(() => ld.RefreshAsync(), rows => rows.Single(x => x.Index == target.Index) is { AndroidStarted: false, ProcessId: <= 0, VBoxProcessId: <= 0 }, 60);
                    }
                    await CheckHost();
                    var stopped = (await ld.RefreshAsync()).Single(x => x.Index == target.Index);
                    Require(!stopped.AndroidStarted && stopped.ProcessId <= 0 && stopped.VBoxProcessId <= 0, "Profile edit requires a fully stopped target");
                    List<string> command = ["modify", "--index", target.Index.ToString(), "--manufacturer", target.Manufacturer, "--model", target.Model];
                    if (target.MemoryMb > 0) command.AddRange(["--memory", target.MemoryMb.ToString(), "--cpu", "2"]);
                    var result = await runner.RunAsync(command);
                    return new { before, requestedModel = target.Model, requestedManufacturer = target.Manufacturer, result, identifierFlagsRequested = false };
                });
                await journal.Case(target, "launch", async () =>
                {
                    var memory = ReadMemory();
                    await journal.Event("prepare-resources.jsonl", new { utc = DateTimeOffset.UtcNow, target.Index, memory.AvailablePhysical, memory.TotalPhysical, memory.AvailablePageFile, memory.TotalPageFile });
                    var required = (3UL * 1024 * 1024 * 1024) + (ulong)target.MemoryMb * 1024 * 1024;
                    Require(memory.AvailablePhysical >= required && (1.0 - (double)memory.AvailablePageFile / memory.TotalPageFile) < .80, "Insufficient launch reserve; do not add load");
                    var row = (await ld.RefreshAsync()).Single(x => x.Index == target.Index);
                    var result = await ld.ExecuteAsync(target.Index, "launch", new JsonObject { ["expectedGeneration"] = row.Generation });
                    await Wait(() => ld.RefreshAsync(), rows => rows.Single(x => x.Index == target.Index).AndroidStarted, 90);
                    await CheckHost(); return result;
                });
                var current = (await ld.RefreshAsync()).Single(x => x.Index == target.Index);
                using var vbox = Process.GetProcessById(current.VBoxProcessId);
                using var player = Process.GetProcessById(current.ProcessId);
                var fixedTarget = target with { VBoxPid = current.VBoxProcessId, VBoxBirth = vbox.StartTime.ToUniversalTime().Ticks, PlayerPid = current.ProcessId, PlayerBirth = player.StartTime.ToUniversalTime().Ticks };
                // The formula is only a candidate supplied by the receipt. Actual TCP ownership is mandatory before any guest read.
                await Wait(async () => { try { await LdPlayerAdbBinding.VerifyAsync(fixedTarget.Serial, fixedTarget.VBoxPid, fixedTarget.VBoxBirth); return true; } catch (DeviceException) { return false; } }, ready => ready, 120);
                await using var backend = new AdbDeviceBackend(new AppSettings { AdbAllowedSerials = [fixedTarget.Serial], CommandTimeoutSeconds = 300 });
                var devices = await Wait(() => backend.DiscoverAsync(), rows => rows.Any(x => x.Serial == fixedTarget.Serial && x.Online), 120);
                fixedTarget = fixedTarget with { TransportId = checked((uint)devices.Single(x => x.Serial == fixedTarget.Serial).TransportId!.Value) };
                await journal.Case(fixedTarget, "boot-model", async () =>
                {
                    await Wait(() => backend.ShellAsync(fixedTarget.Serial, "getprop sys.boot_completed"), value => value.Trim() == "1", 120);
                    var model = (await backend.ShellAsync(fixedTarget.Serial, "getprop ro.product.model")).Trim();
                    var manufacturer = (await backend.ShellAsync(fixedTarget.Serial, "getprop ro.product.manufacturer")).Trim();
                    Require(model == fixedTarget.Model && manufacturer == fixedTarget.Manufacturer, $"Profile not effective: model={model}, manufacturer={manufacturer}");
                    var png = await backend.ScreenshotAsync(fixedTarget.Serial); var size = PngImage.ReadDimensions(png);
                    await File.WriteAllBytesAsync(Path.Combine(output, $"instance-{fixedTarget.Index}-first.png"), png);
                    var observed = (await backend.DiscoverAsync()).Single(x => x.Serial == fixedTarget.Serial);
                    Require(observed.Model == model, "Catalog model did not refresh");
                    return new { model, manufacturer, size, observed, fixedTarget };
                });
                prepared.Add(fixedTarget);
                await File.WriteAllTextAsync(Path.Combine(output, "targets.json"), JsonSerializer.Serialize(spec with { Targets = prepared.Concat(spec.Targets.Skip(prepared.Count)).ToArray() }, JsonDefaults.Options));
            }
            await CheckHost(); return;
        }
        Require(spec.Minutes is >= 1 and <= 30, "Duration must be 1..30 minutes");
        foreach (var target in spec.Targets)
        {
            using var process = Process.GetProcessById(target.VBoxPid);
            Require(process.StartTime.ToUniversalTime().Ticks == target.VBoxBirth, "Target process changed since receipt");
            await LdPlayerAdbBinding.VerifyAsync(target.Serial, target.VBoxPid, target.VBoxBirth);
        }
        if (args[1] == "diagnose")
        {
            await using var diagnostic = new AdbDeviceBackend(new AppSettings { AdbAllowedSerials = spec.Targets.Select(x => x.Serial).ToArray(), CommandTimeoutSeconds = 10 });
            var observed = await diagnostic.DiscoverAsync();
            foreach (var target in spec.Targets)
            {
                Require(observed.Single(x => x.Serial == target.Serial).TransportId == target.TransportId, "Diagnostic transport changed");
                foreach (var command in new[] { "dumpsys activity activities", "dumpsys activity lastanr", "dumpsys window windows", "ps -A -o PID,NAME" })
                {
                    try { await journal.Event("diagnostics.jsonl", new { target.Index, target.Serial, command, utc = DateTimeOffset.UtcNow, output = await diagnostic.ShellAsync(target.Serial, command) }); }
                    catch (Exception ex) { await journal.Event("diagnostics.jsonl", new { target.Index, target.Serial, command, utc = DateTimeOffset.UtcNow, error = ex.Message }); }
                }
                try
                {
                    var nodes = NodeSearch.FindAll(await diagnostic.DumpAsync(target.Serial), new());
                    await journal.Event("diagnostic-nodes.jsonl", new { target.Index, target.Serial, utc = DateTimeOffset.UtcNow, nodes });
                    await File.WriteAllBytesAsync(Path.Combine(output, $"instance-{target.Index}.png"), await diagnostic.ScreenshotAsync(target.Serial));
                }
                catch (Exception ex) { await journal.Event("diagnostic-nodes.jsonl", new { target.Index, target.Serial, utc = DateTimeOffset.UtcNow, error = ex.Message }); }
            }
            await CheckHost(); return;
        }
        Require(Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(spec.Apk))) == spec.ApkSha256, "Supplied APK hash mismatch");
        var settings = new AppSettings { LdConsolePath = spec.ConsolePath, AdbAllowedSerials = spec.Targets.Select(x => x.Serial).ToArray(),
            LdSerialBindings = spec.Targets.ToDictionary(x => x.Index, x => x.Serial), CommandTimeoutSeconds = 300, BasicApiPort = 19180, AutomatorApiPort = 19171, AutoStartApi = false };
        await using var runtime = await MatrixRuntime.CreateAsync(settings, Path.Combine(output, "runtime"));
        var backendShared = runtime.Backend;
        Require(runtime.Devices.Snapshot.Count == spec.Targets.Length && runtime.Devices.Snapshot.All(x => x.Online), "Not all authorized guests ready");
        foreach (var target in spec.Targets)
        {
            var device = runtime.Devices.Snapshot.Single(x => x.Serial == target.Serial);
            Require(device.TransportId == target.TransportId && device.Model == target.Model, "Transport or model changed since receipt");
        }
        await runtime.StartApisAsync();
        var ports = new Dictionary<string, int>();
        foreach (var target in spec.Targets) ports[target.Serial] = await runtime.Api.StartDeviceAsync(target.Serial);
        var received = new ConcurrentDictionary<string, int>(StringComparer.Ordinal);
        var builder = WebApplication.CreateBuilder(); builder.Logging.ClearProviders(); builder.WebHost.UseUrls("http://127.0.0.1:19281");
        await using var sink = builder.Build();
        sink.MapPost("/return", async (HttpContext context) =>
        {
            var record = await context.Request.ReadFromJsonAsync<JsonElement>();
            var serial = record.GetProperty("serial").GetString()!;
            if (!ports.ContainsKey(serial)) { context.Response.StatusCode = 400; return; }
            received.AddOrUpdate(serial, 1, (_, count) => count + 1);
            await journal.Event("returns.jsonl", record);
            await context.Response.WriteAsJsonAsync(new { serial, nonce = record.GetProperty("nonce").GetString(), accepted = true });
        });
        await sink.StartAsync();
        using var handler = new HttpClientHandler { UseProxy = spec.ProxyPort > 0 };
        if (spec.ProxyPort > 0) handler.Proxy = new WebProxy($"http://127.0.0.1:{spec.ProxyPort}") { BypassProxyOnLocal = false };
        using var http = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(60) };
        using var lifetime = new CancellationTokenSource();
        var resources = Monitor(spec, journal, CheckHost, lifetime);
        async Task<JsonElement> Post(Target target, string path, object body, string task, int expectedStatus = 200, CancellationToken ct = default)
        {
            using var requestBudget = CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token, ct);
            using var request = new HttpRequestMessage(HttpMethod.Post, path) { Content = JsonContent.Create(body, options: JsonDefaults.Options) };
            var id = Guid.NewGuid().ToString("N");
            request.Headers.Add("X-Matrix-Run", spec.RunId); request.Headers.Add("X-Matrix-Instance", target.Index.ToString());
            request.Headers.Add("X-Matrix-Task", task); request.Headers.Add("X-Matrix-Request", id);
            var clock = Stopwatch.StartNew(); using var response = await http.SendAsync(request, requestBudget.Token);
            var text = await response.Content.ReadAsStringAsync(requestBudget.Token);
            await journal.Event("http.jsonl", new { utc = DateTimeOffset.UtcNow, target.Index, target.Serial, task, id, path, body, status = (int)response.StatusCode, response = text, durationMs = clock.ElapsedMilliseconds, proxy = spec.ProxyPort });
            Require((int)response.StatusCode == expectedStatus, $"HTTP {response.StatusCode}, expected={expectedStatus}: {text}"); return JsonDocument.Parse(text).RootElement.Clone();
        }
        var start = Stopwatch.StartNew(); var pressure = new Stopwatch();
        var readyWorkers = 0; var allReady = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        async Task Worker(Target target, int role)
        {
            var token = lifetime.Token; var nonce = $"{spec.RunId}-{target.Index}-" + Guid.NewGuid().ToString("N");
            var remote = "/sdcard/MatrixMulti-" + spec.RunId; var local = Path.Combine(output, "instance-" + target.Index); Directory.CreateDirectory(local);
            await journal.Event("workers.jsonl", new { target.Index, target.Serial, nonce, role, state = "start", utc = DateTimeOffset.UtcNow });
            try
            {
                await backendShared.ShellAsync(target.Serial, "mkdir -p " + remote, ct: token);
                var marker = Path.Combine(local, "identity.txt"); await File.WriteAllTextAsync(marker, nonce, token);
                await backendShared.PushAsync(target.Serial, marker, remote + "/identity.txt", ct: token);
                await journal.Case(target, "install", async () =>
                {
                    await backendShared.InstallAsync(target.Serial, spec.Apk, ct: token);
                    var apps = await backendShared.ListAppsAsync(target.Serial, ct: token);
                    Require(apps.Any(x => x.PackageName == Package && x.VersionName == "40.7.0"), "Wrong installed version");
                    await backendShared.StartAppAsync(target.Serial, Package, ct: token);
                    var activity = await backendShared.ShellAsync(target.Serial, "dumpsys activity activities", ct: token);
                    Require(activity.Split('\n').Any(line => (line.Contains("mResumedActivity", StringComparison.Ordinal) || line.Contains("topResumedActivity", StringComparison.Ordinal)) && line.Contains(Package, StringComparison.Ordinal)), "App is not the resumed foreground activity");
                    var png = await backendShared.ScreenshotAsync(target.Serial, token); await File.WriteAllBytesAsync(Path.Combine(local, "app-start.png"), png, token);
                    await backendShared.StopAppAsync(target.Serial, Package, ct: token);
                    Require(!(await backendShared.ShellAsync(target.Serial, "ps -A -o NAME", ct: token)).Split('\n').Any(line => line.Trim().StartsWith(Package, StringComparison.Ordinal)), "App still running");
                    return new { installedVersion = "40.7.0", package = Package, apkSha256 = spec.ApkSha256, foregroundObserved = true, agreementNotAcceptedByHarness = true };
                });
                if (Interlocked.Increment(ref readyWorkers) == spec.Targets.Length) { pressure.Start(); allReady.SetResult(); }
                await allReady.Task.WaitAsync(token);
                var rounds = 0;
                do
                {
                    token.ThrowIfCancellationRequested(); var round = rounds++;
                    await journal.Case(target, $"round-{round}", async () =>
                    {
                        var bytes = RandomNumberGenerator.GetBytes(role == 0 ? 8 * 1024 * 1024 : 256 * 1024);
                        var source = Path.Combine(local, "upload.bin"); var pulled = Path.Combine(local, "pulled.bin"); await File.WriteAllBytesAsync(source, bytes, token);
                        var hash = Convert.ToHexString(SHA256.HashData(bytes));
                        await backendShared.PushAsync(target.Serial, source, remote + "/payload.bin", ct: token);
                        await backendShared.PullAsync(target.Serial, remote + "/payload.bin", pulled, ct: token);
                        Require(Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(pulled))) == hash, "Cross-instance payload or truncated transfer");
                        using var uiBudget = CancellationTokenSource.CreateLinkedTokenSource(token);
                        uiBudget.CancelAfter(TimeSpan.FromSeconds(45));
                        var uiToken = uiBudget.Token;
                        await journal.Event("ui-stages.jsonl", new { target.Index, target.Serial, round, stage = "open-settings", utc = DateTimeOffset.UtcNow });
                        await backendShared.KeyEventAsync(target.Serial, 3, uiToken);
                        await backendShared.ShellAsync(target.Serial, "am start -a android.settings.SETTINGS -f 0x14000000", ct: uiToken);
                        while (true)
                        {
                            await Task.Delay(700, uiToken);
                            var foreground = await backendShared.ShellAsync(target.Serial, "dumpsys activity activities", ct: uiToken);
                            if (foreground.Split('\n').Any(line => (line.Contains("mResumedActivity", StringComparison.Ordinal) || line.Contains("topResumedActivity", StringComparison.Ordinal)) && line.Contains("com.android.settings/", StringComparison.Ordinal))) break;
                        }
                        await journal.Event("ui-stages.jsonl", new { target.Index, target.Serial, round, stage = "settings-foreground", utc = DateTimeOffset.UtcNow });
                        var root = await backendShared.DumpAsync(target.Serial, uiToken);
                        var search = NodeSearch.FindAll(root, new()).FirstOrDefault(node => node.PackageName == "com.android.settings" && node.Bounds.Width > 0 &&
                            ((node.Text ?? "").Contains("搜索") || (node.ContentDesc ?? "").Contains("搜索") || (node.Text ?? "").Contains("Search", StringComparison.OrdinalIgnoreCase) || (node.ContentDesc ?? "").Contains("Search", StringComparison.OrdinalIgnoreCase)));
                        Require(search is not null, "No safe settings search control");
                        var size = PngImage.ReadDimensions(await backendShared.ScreenshotAsync(target.Serial, uiToken));
                        var package = SettingsScript(target.Index, role, search!.Bounds.CenterX, search.Bounds.CenterY, size.Width, size.Height);
                        await runtime.Scripts.RunAsync(target.Serial, package, uiToken);
                        Require(runtime.Scripts.GetStatus(target.Serial).State == ScriptRunState.Completed, "Script incomplete");
                        if (role != 0)
                        {
                            var afterScroll = await backendShared.DumpAsync(target.Serial, uiToken);
                            var fresh = NodeSearch.FindAll(afterScroll, new()).FirstOrDefault(node => node.PackageName == "com.android.settings" && node.Enabled && node.Bounds.Width > 0 &&
                                ((node.Text ?? "").Contains("搜索") || (node.ContentDesc ?? "").Contains("搜索") || (node.Text ?? "").Contains("Search", StringComparison.OrdinalIgnoreCase) || (node.ContentDesc ?? "").Contains("Search", StringComparison.OrdinalIgnoreCase)));
                            Require(fresh is not null, "Search control disappeared after swipe; no stale tap sent");
                            await backendShared.TapAsync(target.Serial, fresh!.Bounds.CenterX, fresh.Bounds.CenterY, uiToken); await Task.Delay(600, uiToken);
                        }
                        var beforeText = await backendShared.DumpAsync(target.Serial, uiToken);
                        var edit = NodeSearch.FindAll(beforeText, new()).FirstOrDefault(node => node.ResourceId == "android:id/search_src_text" && node.Enabled && node.Bounds.Width > 0);
                        await journal.Event("before-input.jsonl", new { target.Index, target.Serial, round, edit });
                        Require(edit is not null, "No verified editable field; text not sent");
                        Require(IsEmptySettingsSearch(edit!), "Search input retained prior text; no appended text sent");
                        await backendShared.TapAsync(target.Serial, edit!.Bounds.CenterX, edit.Bounds.CenterY, uiToken);
                        var text = $"实例{target.Index} Round {round}";
                        await Post(target, $"http://127.0.0.1:{ports[target.Serial]}/text", new { text }, "text-input", ct: uiToken); await Task.Delay(500, uiToken);
                        var fields = NodeSearch.FindAll(await backendShared.DumpAsync(target.Serial, uiToken), new()).Where(node => node.ResourceId == "android:id/search_src_text").ToArray();
                        await File.WriteAllTextAsync(Path.Combine(local, "latest-input-nodes.json"), JsonSerializer.Serialize(fields, JsonDefaults.Options), token);
                        Require(fields.Any(node => node.Text == text), "Wrong instance text/focus");
                        uiToken.ThrowIfCancellationRequested();
                        await journal.Event("ui-stages.jsonl", new { target.Index, target.Serial, round, stage = "input-verified", utc = DateTimeOffset.UtcNow });
                        var reply = await Post(target, $"http://127.0.0.1:{ports[target.Serial]}/shell", new { command = "cat " + remote + "/identity.txt; sha256sum " + remote + "/payload.bin" }, "identity-return");
                        var actual = reply.GetProperty("data").GetProperty("output").GetString()!;
                        Require(actual.StartsWith(nonce, StringComparison.Ordinal) && actual.Contains(hash, StringComparison.OrdinalIgnoreCase), "Returned identity/payload belongs to another instance");
                        var telemetry = new { runId = spec.RunId, target.Index, serial = target.Serial, nonce, round, sha256 = hash, bytes = bytes.Length, text, model = target.Model, script = runtime.Scripts.GetStatus(target.Serial), data = actual };
                        var ack = await Post(target, "http://127.0.0.1:19281/return", telemetry, "data-return"); Require(ack.GetProperty("nonce").GetString() == nonce && ack.GetProperty("serial").GetString() == target.Serial, "Wrong telemetry acknowledgement");
                        var png = await backendShared.ScreenshotAsync(target.Serial, token); await File.WriteAllBytesAsync(Path.Combine(local, "latest.png"), png, token);
                        await journal.Event("script-logs.jsonl", new { target.Index, target.Serial, round, logs = runtime.Scripts.GetLogs(target.Serial) });
                        return telemetry;
                    });
                    await Task.Delay(TimeSpan.FromSeconds(role == 0 ? 4 : 2), token);
                } while (pressure.Elapsed < TimeSpan.FromMinutes(spec.Minutes));
                await journal.Event("workers.jsonl", new { target.Index, target.Serial, state = "end", rounds, returns = received.GetValueOrDefault(target.Serial), utc = DateTimeOffset.UtcNow });
            }
            catch { lifetime.Cancel(); throw; }
        }
        try
        {
            await journal.Case(spec.Targets[1], "cross-instance-LD-api", async () =>
            {
                var first = spec.Targets[0]; var second = spec.Targets[1];
                var pending = runtime.Scripts.RunAsync(first.Serial, new ScriptPackage { Manifest = new() { Id = "multi-busy", Name = "并行门禁验证", Version = "1.0" }, Steps = [new() { Action = "wait", DurationMs = 10000 }] }, lifetime.Token);
                try
                {
                    var rows = await runtime.LdPlayer!.RefreshAsync(lifetime.Token);
                    var resolved = await backendShared.ShellAsync(second.Serial, "cmd package resolve-activity --brief -a android.intent.action.MAIN -c android.intent.category.HOME", ct: lifetime.Token);
                    var home = resolved.Split('\n').Select(line => line.Trim()).LastOrDefault(line => line.Contains('/'));
                    Require(home is not null, "Cannot resolve a verifiable Home activity");
                    var homePackage = home!.Split('/')[0];
                    await Post(first, $"http://127.0.0.1:19180/ldplayer/instances/{first.Index}/keyevent", new { expectedGeneration = rows.Single(x => x.Index == first.Index).Generation, key = "home" }, "same-instance-reject", 409);
                    var result = await Post(second, $"http://127.0.0.1:19180/ldplayer/instances/{second.Index}/keyevent", new { expectedGeneration = rows.Single(x => x.Index == second.Index).Generation, key = "home" }, "other-instance-home");
                    var state = runtime.Scripts.GetStatus(first.Serial).State;
                    Require(state == ScriptRunState.Running, "No actual overlap between A script and B LD API");
                    var activity = await Wait(async () =>
                    {
                        var value = await backendShared.ShellAsync(second.Serial, "dumpsys activity activities", ct: lifetime.Token);
                        await journal.Event("home-diagnostic.jsonl", new { utc = DateTimeOffset.UtcNow, second.Serial, resolved, activity = value });
                        return value;
                    }, value => value.Split('\n').Any(line => (line.Contains("mResumedActivity") || line.Contains("topResumedActivity")) && line.Contains(homePackage, StringComparison.Ordinal)), 30);
                    return new { busySerial = first.Serial, otherSerial = second.Serial, sameInstanceStatus = 409, otherStatus = 200, state, result, home, activity, homeObserved = true };
                }
                finally { runtime.Scripts.Stop(first.Serial); await pending; }
            });
            await Task.WhenAll(spec.Targets.Select((target, index) => Worker(target, index)));
        }
        finally
        {
            lifetime.Cancel(); foreach (var target in spec.Targets) runtime.Scripts.Stop(target.Serial);
            try { await resources; } catch (OperationCanceledException) when (lifetime.IsCancellationRequested) { }
            await sink.StopAsync();
        }
        await CheckHost();
        await journal.Event("summary.jsonl", new { elapsedMs = start.ElapsedMilliseconds, pressureMs = pressure.ElapsedMilliseconds, targetCount = spec.Targets.Length, returns = received, proxyPort = spec.ProxyPort, status = "PASS-LIVE-MULTI" });
    }
    private static async Task Monitor(Spec spec, Journal journal, Func<Task> checkHost, CancellationTokenSource lifetime)
    {
        var previous = Times(); var clock = Stopwatch.StartNew(); var highCpuSeconds = 0;
        try
        {
        while (!lifetime.IsCancellationRequested)
        {
            var memory = ReadMemory();
            var current = Times(); var total = current.Kernel + current.User - previous.Kernel - previous.User;
            var cpu = total == 0 ? 0 : (1.0 - (double)(current.Idle - previous.Idle) / total) * 100; previous = current;
            var processes = spec.Targets.SelectMany(target => new[] { target.VBoxPid, target.PlayerPid }).Append(Environment.ProcessId).Distinct().Select(pid =>
            { using var process = Process.GetProcessById(pid); return new { pid, process.ProcessName, process.WorkingSet64, process.PrivateMemorySize64, process.HandleCount, cpuTimeMs = process.TotalProcessorTime.TotalMilliseconds }; }).ToArray();
            await checkHost();
            await journal.Event("resources.jsonl", new { utc = DateTimeOffset.UtcNow, elapsedMs = clock.ElapsedMilliseconds, cpuPercent = cpu, memory.AvailablePhysical, memory.TotalPhysical, memory.AvailablePageFile, memory.TotalPageFile, processes });
            highCpuSeconds = cpu > 90 ? highCpuSeconds + 10 : 0;
            if (memory.AvailablePhysical < 2UL * 1024 * 1024 * 1024 || (1.0 - (double)memory.AvailablePageFile / memory.TotalPageFile) > .85 || highCpuSeconds >= 60)
            { lifetime.Cancel(); throw new InvalidOperationException("Resource stop threshold reached; only owned workload cancelled"); }
            await Task.Delay(TimeSpan.FromSeconds(10), lifetime.Token);
        }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            await journal.Event("resource-stop.jsonl", new { utc = DateTimeOffset.UtcNow, error = ex.Message });
            lifetime.Cancel(); throw;
        }
    }
    private static async Task<T> Wait<T>(Func<Task<T>> operation, Func<T, bool> ready, int seconds)
    {
        var clock = Stopwatch.StartNew();
        while (clock.Elapsed < TimeSpan.FromSeconds(seconds))
        {
            try { var value = await operation(); if (ready(value)) return value; }
            catch (DeviceException ex) when (ex.Message.StartsWith("list2 ", StringComparison.Ordinal) && ex.Status is 400 or 502)
            { Console.WriteLine("WAIT directory transition: " + ex.Message); }
            await Task.Delay(2000);
        }
        throw new TimeoutException("Explicit target readiness timeout");
    }
    private static void Require(bool valid, string message) { if (!valid) throw new InvalidOperationException(message); }
    internal static bool IsEmptySettingsSearch(AndroidNode node) => node.ResourceId == "android:id/search_src_text"
        && node.PackageName == "com.android.settings.intelligence" && node.ClassName == "android.widget.AutoCompleteTextView"
        && (string.IsNullOrEmpty(node.Text) || node.Text == "\u641c\u7d22\u2026");
    internal static ScriptPackage SettingsScript(int index, int role, int searchX, int searchY, int width, int height)
    {
        var package = new ScriptPackage { Manifest = new() { Id = "multi-" + index, Name = "独立实例任务" + index, Version = "1.0" }, Steps =
            [new() { Action = "wait", DurationMs = role == 0 ? 900 : 1600 }, new() { Action = "click", Value = $"{searchX},{searchY}" }, new() { Action = "wait", DurationMs = 600 }] };
        if (role != 0) package.Steps[1] = new ScriptStep { Action = "swipe", Value = $"{width / 2},{height * 4 / 5},{width / 2},{height * 2 / 5},350" };
        return package;
    }
    private sealed class Journal(string directory)
    {
        private readonly SemaphoreSlim gate = new(1, 1); private int sequence;
        public async Task Event(string file, object value)
        {
            await gate.WaitAsync(); try { await File.AppendAllTextAsync(Path.Combine(directory, file), JsonSerializer.Serialize(value, new JsonSerializerOptions(JsonDefaults.Options) { WriteIndented = false }) + "\n"); } finally { gate.Release(); }
        }
        public async Task Case(Target target, string task, Func<Task<object>> operation)
        {
            var start = DateTimeOffset.UtcNow; var clock = Stopwatch.StartNew();
            try { var value = await operation(); var file = $"{Interlocked.Increment(ref sequence):D4}-instance-{target.Index}-{task}.json"; await File.WriteAllTextAsync(Path.Combine(directory, file), JsonSerializer.Serialize(value, JsonDefaults.Options)); await Event("results.jsonl", new { target.Index, target.Serial, target.TransportId, task, start, end = DateTimeOffset.UtcNow, status = "PASS-LIVE", durationMs = clock.ElapsedMilliseconds, file, sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Path.Combine(directory, file)))) }); Console.WriteLine($"PASS {target.Index} {task} {clock.ElapsedMilliseconds}ms"); }
            catch (Exception ex) { await Event("results.jsonl", new { target.Index, target.Serial, task, start, end = DateTimeOffset.UtcNow, status = "FAIL", durationMs = clock.ElapsedMilliseconds, error = ex.Message }); throw; }
        }
    }
    private sealed class RecordingRunner(ILdConsoleRunner inner, Journal journal) : ILdConsoleRunner
    {
        public async Task<string> RunAsync(IReadOnlyList<string> arguments, CancellationToken ct = default)
        {
            var start = DateTimeOffset.UtcNow;
            try { var output = await inner.RunAsync(arguments, ct); await journal.Event("ld-console.jsonl", new { start, arguments, output }); return output; }
            catch (Exception ex) { await journal.Event("ld-console.jsonl", new { start, arguments, error = ex.Message }); throw; }
        }
    }
    [StructLayout(LayoutKind.Sequential)] private struct Memory { public uint Length, Load; public ulong TotalPhysical, AvailablePhysical, TotalPageFile, AvailablePageFile, TotalVirtual, AvailableVirtual, AvailableExtended; }
    private static Memory ReadMemory() { var value = new Memory { Length = (uint)Marshal.SizeOf<Memory>() }; if (!GlobalMemoryStatusEx(ref value)) throw new InvalidOperationException("Cannot sample memory"); return value; }
    [DllImport("kernel32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool GlobalMemoryStatusEx(ref Memory memory);
    [DllImport("kernel32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool GetSystemTimes(out long idle, out long kernel, out long user);
    private static (ulong Idle, ulong Kernel, ulong User) Times() { if (!GetSystemTimes(out var idle, out var kernel, out var user)) throw new InvalidOperationException("Cannot sample CPU"); return ((ulong)idle, (ulong)kernel, (ulong)user); }
}
