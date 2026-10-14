using System.Diagnostics;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using MatrixDevice.Core;
using MatrixDevice.Features;
using MatrixDevice.Host;

internal static class LiveAcceptance
{
    private const string Package = "com.ss.android.ugc.aweme";
    public static async Task RunAsync(string[] args)
    {
        if (args.Length != 3) throw new ArgumentException("--live-acceptance <verified-target.json> <phase> <output-directory>");
        var target = JsonSerializer.Deserialize<Target>(File.ReadAllText(args[0]), JsonDefaults.Options)!;
        if (target.Index <= 0 || string.IsNullOrWhiteSpace(target.Serial) || target.VBoxPid <= 0 || !Path.IsPathFullyQualified(args[2]))
            throw new ArgumentException("A verified nonzero independent target and absolute evidence directory are required.");
        var output = Path.GetFullPath(args[2]); Directory.CreateDirectory(output);
        var phase = args[1];
        var options = new JsonSerializerOptions(JsonDefaults.Options) { WriteIndented = false };
        var hash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(typeof(AdbDeviceBackend).Assembly.Location)));
        var sequence = 0;
        DeviceInfo? observedIdentity = null;
        async Task<T> Case<T>(string id, string expected, Func<Task<T>> operation, string passStatus = "PASS-LIVE")
        {
            var watch = Stopwatch.StartNew(); var utc = DateTimeOffset.UtcNow;
            try
            {
                var actual = await operation();
                var evidence = Path.Combine(output, $"{++sequence:D3}-{id}.json");
                await File.WriteAllTextAsync(evidence, JsonSerializer.Serialize(actual, JsonDefaults.Options));
                await File.AppendAllTextAsync(Path.Combine(output, "results.jsonl"), JsonSerializer.Serialize(new { caseId = id, phase, utc, buildSha256 = hash, target.Index, target.Serial, generation = observedIdentity?.Generation, transportId = observedIdentity?.TransportId, expected, status = passStatus, durationMs = watch.ElapsedMilliseconds, evidence = Path.GetFileName(evidence), evidenceSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(evidence))) }, options) + "\n");
                Console.WriteLine($"{passStatus} {id} {watch.ElapsedMilliseconds}ms"); return actual;
            }
            catch (Exception ex)
            {
                await File.AppendAllTextAsync(Path.Combine(output, "results.jsonl"), JsonSerializer.Serialize(new { caseId = id, phase, utc, buildSha256 = hash, target.Index, target.Serial, expected, status = "FAIL", durationMs = watch.ElapsedMilliseconds, error = ex.Message }, options) + "\n");
                throw;
            }
        }
        static void Require(bool condition, string message) { if (!condition) throw new Exception(message); }
        static bool SearchField(AndroidNode node) => node.ResourceId == "android:id/search_src_text" && node.PackageName is "com.android.settings" or "com.android.settings.intelligence" && node.Enabled;
        var runner = new LdConsoleRunner(target.ConsolePath, 300);
        var instances = LdPlayerService.ParseList2(await runner.RunAsync(["list2"]));
        var instance = instances.Single(x => x.Index == target.Index);
        Require(instance.Name == target.Name && instance.VBoxProcessId == target.VBoxPid && instance.AndroidStarted, "Target instance identity or readiness changed.");
        using (var process = Process.GetProcessById(target.VBoxPid)) Require(process.StartTime.ToUniversalTime().Ticks == target.VBoxStartedUtcTicks, "VBox PID was reused.");
        using (var process = Process.GetProcessById(target.ServerPid)) Require(process.StartTime.ToUniversalTime().Ticks == target.ServerStartedUtcTicks, "ADB service identity changed.");
        Require(instances.Single(x => x.Index == 0).ProcessId == target.OriginalPid && instances.Single(x => x.Index == 0).VBoxProcessId == target.OriginalVBoxPid, "Original host instance changed.");
        var settings = new AppSettings { AdbAllowedSerials = [target.Serial], LdConsolePath = target.ConsolePath,
            LdSerialBindings = new() { [target.Index] = target.Serial }, CommandTimeoutSeconds = 300,
            BasicApiPort = 18080, AutomatorApiPort = 18171, AutoStartApi = false };
        await using var runtime = await MatrixRuntime.CreateAsync(settings, Path.Combine(output, "runtime"));
        var backend = runtime.Backend;
        var devices = await Case("connect", "Exactly the authorized independent guest is Ready", () =>
        {
            var observed = runtime.Devices.Snapshot;
            Require(observed.Count == 1 && observed[0].Serial == target.Serial && observed[0].Online, "Authorized guest did not become Ready: " + runtime.DiscoveryError);
            observedIdentity = observed[0];
            return Task.FromResult(observed);
        });
        async Task<string> Shell(string command)
        {
            var start = Stopwatch.StartNew();
            try
            {
                var value = await backend.ShellAsync(target.Serial, command);
                await File.AppendAllTextAsync(Path.Combine(output, "shell-trace.jsonl"), JsonSerializer.Serialize(new { utc = DateTimeOffset.UtcNow, target.Serial, command, output = value, durationMs = start.ElapsedMilliseconds }, options) + "\n");
                return value;
            }
            catch (Exception ex) { await File.AppendAllTextAsync(Path.Combine(output, "shell-trace.jsonl"), JsonSerializer.Serialize(new { utc = DateTimeOffset.UtcNow, target.Serial, command, error = ex.Message, durationMs = start.ElapsedMilliseconds }, options) + "\n"); throw; }
        }
        async Task<object> Screen(string name)
        {
            var png = await backend.ScreenshotAsync(target.Serial); var size = PngImage.ReadDimensions(png);
            var path = Path.Combine(output, name + ".png"); await File.WriteAllBytesAsync(path, png);
            Require(size.Width > 0 && size.Height > 0, "Empty screenshot.");
            return new { size.Width, size.Height, bytes = png.Length, file = Path.GetFileName(path), sha256 = Convert.ToHexString(SHA256.HashData(png)) };
        }
        if (phase is "install" or "launch")
        {
            await Case("abi", "Android API >=23 with arm64 or documented native bridge", async () =>
            {
                var value = await Shell("getprop ro.product.cpu.abilist; getprop ro.dalvik.vm.native.bridge; getprop ro.build.version.sdk; getprop sys.boot_completed");
                Require(value.Contains("arm64", StringComparison.Ordinal) || value.Contains("libhoudini", StringComparison.Ordinal), "No supported ARM64 ABI/native bridge: " + value);
                return value;
            });
            if (phase == "install") await Case("install", "APK SHA256 and installed version match supplied APK", async () =>
            {
                Require(Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(target.Apk))) == "DA4ED3EBCD2F76EEAA1061FD07BA10FDA5DA1C0173D22344C23A8ECAF675B8FA", "APK digest changed.");
                var existing = await Shell("pm list packages " + Package);
                Require(!existing.Split('\n').Any(line => line.Trim() == "package:" + Package), "Package already exists; no unapproved replacement.");
                await backend.InstallAsync(target.Serial, target.Apk);
                var installed = await Shell("dumpsys package " + Package);
                Require(installed.Contains("versionCode=400701", StringComparison.Ordinal) && installed.Contains("versionName=40.7.0", StringComparison.Ordinal), "Installed version mismatch.");
                return new { version = "40.7.0", code = 400701, path = await Shell("pm path " + Package) };
            });
            await Case("launch", "Target package has a PID and foreground activity", async () =>
            {
                var result = await backend.StartAppAsync(target.Serial, Package); await Task.Delay(4000);
                var pid = await Shell("pidof " + Package); var activity = await Shell("dumpsys activity activities");
                Require(!string.IsNullOrWhiteSpace(pid) && activity.Contains(Package, StringComparison.Ordinal), "App did not launch.");
                return new { result, pid, activity };
            });
            await Case("first-screen", "Nonempty PNG of installed app", () => Screen("first-screen"));
            await Case("first-nodes", "Parsed real node tree", () => backend.DumpAsync(target.Serial));
        }
        else if (phase == "dump-probe")
        {
            await Case("dump-diagnostic", "Capture uiautomator output and owned XML read result without accepting terms", async () =>
            {
                var path = "/data/local/tmp/matrix-acceptance-" + Guid.NewGuid().ToString("N") + ".xml";
                try
                {
                    var result = await Shell("uiautomator dump " + AdbDeviceBackend.Quote(path));
                    string xml;
                    try { xml = await Shell("cat " + AdbDeviceBackend.Quote(path)); } catch (DeviceException ex) { xml = ex.Message; }
                    return new { result, xml };
                }
                finally { await Shell("rm -f " + AdbDeviceBackend.Quote(path)); }
            });
        }
        else if (phase == "probe")
        {
            await Case("package-info", "Installed component diagnostic; no launch or mutation", () => Shell("dumpsys package " + Package));
            await Case("resolve", "Launcher intent resolution diagnostic", () => Shell("cmd package resolve-activity --brief -a android.intent.action.MAIN -c android.intent.category.LAUNCHER -p " + Package));
            await Case("probe-screen", "Current guest PNG", () => Screen("probe-screen"));
            await Case("probe-nodes", "Current guest node tree", () => backend.DumpAsync(target.Serial));
        }
        else if (phase is "core" or "io-api")
        {
            if (phase == "core") await Case("static-settings", "Safe static settings page for node and API testing", async () => { var result = await Shell("am start -W -a android.settings.SETTINGS"); Require(result.Contains("Status: ok", StringComparison.Ordinal), "Settings did not open."); await Task.Delay(1500); return result; });
            await Case("screen", "Screenshot crop and color use actual PNG", async () =>
            {
                var png = await backend.ScreenshotAsync(target.Serial); var crop = PngImage.Crop(png, 0, 0, 40, 40);
                await File.WriteAllBytesAsync(Path.Combine(output, "crop.png"), crop);
                var size = PngImage.ReadDimensions(png); var color = PngImage.ColorAt(png, 1, 1); var cropSize = PngImage.ReadDimensions(crop);
                return new { size.Width, size.Height, color = new { color.R, color.G, color.B }, cropWidth = cropSize.Width, cropHeight = cropSize.Height };
            });
            await Case("nodes", "Real nonempty node tree", async () => { var root = await backend.DumpAsync(target.Serial); Require(root.ChildNodeList.Count > 0, "Empty tree."); return root; });
            await Case("apps", "Supplied app listed with correct version", async () => { var apps = await backend.ListAppsAsync(target.Serial); Require(apps.Any(x => x.PackageName == Package && x.VersionName == "40.7.0"), "App/version missing."); return apps; });
            if (phase == "core")
            {
                await Case("stop", "App processes absent; installation retained", async () => { await backend.StopAppAsync(target.Serial, Package); var names = (await Shell("ps -A -o NAME")).Split('\n').Select(line => line.Trim()); Require(!names.Any(name => name == Package || name.StartsWith(Package + ":", StringComparison.Ordinal)), "App still running."); var path = await Shell("pm path " + Package); Require(path.StartsWith("package:", StringComparison.Ordinal), "Installation missing after stop."); return path; });
                await Case("home", "Launcher/Settings safe page only", async () => { await backend.KeyEventAsync(target.Serial, 3); return await Shell("dumpsys activity activities"); });
            }
            var remote = "/sdcard/MatrixAcceptance-" + Guid.NewGuid().ToString("N") + ".txt";
            await Case("file-roundtrip", "Push/pull SHA256 exact; only owned temporary file removed", async () =>
            {
                var source = Path.Combine(output, "transfer.txt"); var dest = Path.Combine(output, "pulled.txt");
                await File.WriteAllTextAsync(source, "matrix-test-中文-" + Guid.NewGuid());
                try { await backend.PushAsync(target.Serial, source, remote); await backend.PullAsync(target.Serial, remote, dest); Require(File.ReadAllBytes(source).SequenceEqual(File.ReadAllBytes(dest)), "File transfer mismatch."); return new { sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(dest))), bytes = new FileInfo(dest).Length }; }
                finally { await Shell("rm -f " + AdbDeviceBackend.Quote(remote)); }
            });
            if (phase == "core") await Case("script", "Actual app launch and script cancellation with no later steps", async () =>
            {
                var package = new ScriptPackage { Manifest = new() { Id = "live-acceptance", Name = "独立实例测试", Version = "1.0" }, Steps = [new() { Action = "launch", Value = Package }, new() { Action = "wait", DurationMs = 10000 }, new() { Action = "click", Value = "1,1" }] };
                var path = Path.Combine(output, "test-script.zip"); ScriptPackageService.Save(path, package);
                var loaded = ScriptPackageService.Load(path); var running = runtime.Scripts.RunAsync(target.Serial, loaded);
                await Task.Delay(4000); await runtime.Scripts.RunAsync(target.Serial, loaded);
                Require(!string.IsNullOrWhiteSpace(await Shell("pidof " + Package)), "Script launch did not affect app.");
                Require(runtime.Scripts.Stop(target.Serial), "Script could not be cancelled."); await running;
                var logs = runtime.Scripts.GetLogs(target.Serial);
                Require(runtime.Scripts.GetStatus(target.Serial).State == ScriptRunState.Cancelled && !logs.Any(x => x.Message.Contains("步骤 3/", StringComparison.Ordinal)), "Post-cancel step ran."); return logs;
            });
            await Case("api", "Real loopback API target/crop/tree/shell and guarded writes", async () =>
            {
                await runtime.StartApisAsync(); var port = await runtime.Api.StartDeviceAsync(target.Serial);
                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(300) };
                var device = await client.GetStringAsync($"http://127.0.0.1:{port}/device"); Require(device.Contains(target.Serial, StringComparison.Ordinal), "Per-device API target mismatch.");
                var records = new List<object>();
                foreach (var (route, fields) in new (string, Dictionary<string, object>)[] { ("device/dump", new()), ("device/screenshot", new() { ["x"] = 0, ["y"] = 0, ["width"] = 40, ["height"] = 40 }), ("device/shell", new() { ["command"] = "getprop sys.boot_completed" }), ("app/list-packages", new()), ("function/sleep-ms", new() { ["milliseconds"] = 10 }) })
                {
                    fields["serial"] = target.Serial;
                    records.Add(await Case("api-" + route.Replace('/', '-'), "Real API response with root code 0", async () =>
                    {
                        using var response = await client.PostAsJsonAsync("http://127.0.0.1:18171/" + route, fields);
                        var body = await response.Content.ReadAsStringAsync(); var root = JsonSerializer.Deserialize<JsonElement>(body);
                        Require(response.IsSuccessStatusCode && root.GetProperty("code").GetInt32() == 0, "API failure " + route + ": " + body);
                        return new { route, status = (int)response.StatusCode, body };
                    }));
                }
                await runtime.StopApisAsync(); return new { device, records };
            });
            if (phase == "core") { await backend.StopAppAsync(target.Serial, Package); await backend.KeyEventAsync(target.Serial, 3); }
        }
        else if (phase is "settings" or "input")
        {
            await Case("settings-open", "Only safe system settings becomes foreground with search task reset", async () => { await backend.StopAppAsync(target.Serial, "com.android.settings.intelligence"); await backend.StopAppAsync(target.Serial, "com.android.settings"); var result = await Shell("am start -W -a android.settings.SETTINGS"); Require(result.Contains("Status: ok", StringComparison.Ordinal), "Settings did not open."); await Task.Delay(1500); return result; });
            var root = await Case("settings-nodes", "Static real settings tree", () => backend.DumpAsync(target.Serial));
            await Case("settings-screen", "Real safe settings screen", () => Screen("settings-screen"));
            if (phase == "input")
            {
                await Case("swipe", "Safe settings list moves; node text sequence changes", async () =>
                {
                    var before = NodeSearch.FindAll(root, new()).Where(n => !string.IsNullOrEmpty(n.Text)).Select(n => n.Text).ToArray();
                    var size = PngImage.ReadDimensions(await backend.ScreenshotAsync(target.Serial));
                    await backend.SwipeAsync(target.Serial, size.Width / 2, size.Height * 4 / 5, size.Width / 2, size.Height * 2 / 5, 400); await Task.Delay(700);
                    var after = NodeSearch.FindAll(await backend.DumpAsync(target.Serial), new()).Where(n => !string.IsNullOrEmpty(n.Text)).Select(n => n.Text).ToArray();
                    Require(!before.SequenceEqual(after), "Settings list did not move.");
                    await backend.SwipeAsync(target.Serial, size.Width / 2, size.Height * 2 / 5, size.Width / 2, size.Height * 4 / 5, 400); await Task.Delay(700);
                    return new { before, after };
                });
                root = await backend.DumpAsync(target.Serial);
                var search = NodeSearch.FindAll(root, new()).FirstOrDefault(n => n.PackageName == "com.android.settings" && n.Bounds.Width > 0 &&
                    ((n.Text ?? "").Contains("搜索", StringComparison.Ordinal) || (n.Text ?? "").Contains("Search", StringComparison.OrdinalIgnoreCase) || (n.ContentDesc ?? "").Contains("搜索", StringComparison.Ordinal) || (n.ContentDesc ?? "").Contains("Search", StringComparison.OrdinalIgnoreCase)));
                Require(search is not null, "No verified settings search control; input blocked.");
                await Case("tap", "Known settings search control opens an editable field", async () =>
                {
                    await backend.TapAsync(target.Serial, search!.Bounds.CenterX, search.Bounds.CenterY); await Task.Delay(700);
                    var nodes = NodeSearch.FindAll(await backend.DumpAsync(target.Serial), new()).Where(SearchField).ToArray();
                    Require(nodes.Any(n => n.Enabled && n.Bounds.Width > 0), "Search tap did not open EditText.");
                    var field = nodes.First(n => n.Enabled && n.Bounds.Width > 0); await backend.TapAsync(target.Serial, field.Bounds.CenterX, field.Bounds.CenterY);
                    return field;
                });
                const string text = "Matrix Test 42!?";
                await Case("ascii-input", "ASCII spaces and punctuation appear exactly without submission", async () =>
                {
                    await backend.TextAsync(target.Serial, text); await Task.Delay(600);
                    var fields = NodeSearch.FindAll(await backend.DumpAsync(target.Serial), new()).Where(SearchField).ToArray();
                    Require(fields.Any(n => n.Text == text), "Text readback mismatch."); return fields;
                });
                await Case("ld-chinese-input", "LD call.input adds exact Chinese and space to the same safe field", async () =>
                {
                    var ld = runtime.LdPlayer!; await ld.RefreshAsync(); var current = ld.Instances.Single(x => x.Index == target.Index);
                    const string chinese = "中文 测试";
                    var result = await ld.ExecuteAsync(target.Index, "input", new JsonObject { ["expectedGeneration"] = current.Generation, ["text"] = chinese });
                    await Task.Delay(700); var fields = NodeSearch.FindAll(await backend.DumpAsync(target.Serial), new()).Where(SearchField).ToArray();
                    Require(fields.Any(n => n.Text == text + chinese), "LD Chinese readback mismatch."); return new { result, fields };
                });
                await Case("bound-global-chinese", "Production TextAsync uses only the explicitly bound LD instance", async () =>
                {
                    const string value = " 已绑定"; await backend.TextAsync(target.Serial, value); await Task.Delay(600);
                    var fields = NodeSearch.FindAll(await backend.DumpAsync(target.Serial), new()).Where(SearchField).ToArray();
                    Require(fields.Any(n => n.Text == text + "中文 测试" + value), "Bound Unicode text readback mismatch."); return fields;
                });
                await Case("back", "Back dismisses input and returns to safe settings", async () => { await backend.KeyEventAsync(target.Serial, 4); await backend.KeyEventAsync(target.Serial, 4); await Task.Delay(500); return await backend.DumpAsync(target.Serial); });
            }
        }
        else if (phase == "script-actions")
        {
            await backend.StopAppAsync(target.Serial, "com.android.settings.intelligence"); await backend.StopAppAsync(target.Serial, "com.android.settings");
            await Shell("am start -W -a android.settings.SETTINGS"); await Task.Delay(800);
            var size = PngImage.ReadDimensions(await backend.ScreenshotAsync(target.Serial));
            var control = NodeSearch.FindAll(await backend.DumpAsync(target.Serial), new()).First(n => n.PackageName == "com.android.settings" && (n.Text ?? "").Contains("搜索", StringComparison.Ordinal) && n.Bounds.Width > 0);
            await Case("script-all-actions", "Launch/wait/swipe/click complete and open only safe system search", async () =>
            {
                var package = new ScriptPackage { Manifest = new() { Id = "all-safe-actions", Name = "系统页面步骤验收", Version = "1.0" }, Steps =
                [new() { Action = "launch", Value = "com.android.settings" }, new() { Action = "wait", DurationMs = 600 },
                 new() { Action = "swipe", Value = $"{size.Width / 2},{size.Height * 4 / 5},{size.Width / 2},{size.Height * 2 / 5},400" }, new() { Action = "wait", DurationMs = 600 },
                 new() { Action = "swipe", Value = $"{size.Width / 2},{size.Height * 2 / 5},{size.Width / 2},{size.Height * 4 / 5},400" }, new() { Action = "wait", DurationMs = 600 },
                 new() { Action = "click", Value = $"{control.Bounds.CenterX},{control.Bounds.CenterY}" }] };
                await runtime.Scripts.RunAsync(target.Serial, package);
                Require(runtime.Scripts.GetStatus(target.Serial).State == ScriptRunState.Completed && NodeSearch.FindAll(await backend.DumpAsync(target.Serial), new()).Any(SearchField), "Script actions did not complete with the safe search effect.");
                await Screen("script-finished"); return runtime.Scripts.GetLogs(target.Serial);
            });
        }
        else if (phase == "ld-api")
        {
            await runtime.StartApisAsync(); using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(300) };
            async Task<JsonElement> Ld(string id, string action, JsonObject body)
            {
                await runtime.LdPlayer!.RefreshAsync(); body["expectedGeneration"] = runtime.LdPlayer.Instances.Single(x => x.Index == target.Index).Generation;
                await File.WriteAllTextAsync(Path.Combine(output, "request-" + id + ".json"), body.ToJsonString(JsonDefaults.Options));
                using var response = await client.PostAsJsonAsync($"http://127.0.0.1:18080/ldplayer/instances/{target.Index}/{action}", body);
                var result = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync()); Require(response.IsSuccessStatusCode && result.GetProperty("ok").GetBoolean(), "LD API rejected " + action + ": " + result);
                await File.WriteAllTextAsync(Path.Combine(output, "response-" + id + ".json"), result.GetRawText()); return result;
            }
            await Case("L04", "LD API runapp makes provided app foreground", async () => { var result = await Ld("L04", "runapp", new() { ["packageName"] = Package }); await Task.Delay(1500); var pid = await Shell("pidof " + Package); Require(!string.IsNullOrWhiteSpace(pid), "LD runapp did not start app."); await Screen("ld-runapp"); return new { result, pid, activity = await Shell("dumpsys activity activities") }; });
            await Case("L05", "LD API killapp removes only provided app processes", async () => { var result = await Ld("L05", "killapp", new() { ["packageName"] = Package }); var names = await Shell("ps -A -o NAME"); Require(!names.Split('\n').Any(x => x.Trim().StartsWith(Package, StringComparison.Ordinal)), "LD killapp left processes."); return new { result, installation = await Shell("pm path " + Package) }; });
            await backend.StopAppAsync(target.Serial, "com.android.settings.intelligence"); await backend.StopAppAsync(target.Serial, "com.android.settings"); await Shell("am start -W -a android.settings.SETTINGS"); await Task.Delay(800);
            var control = NodeSearch.FindAll(await backend.DumpAsync(target.Serial), new()).First(n => n.PackageName == "com.android.settings" && (n.Text ?? "").Contains("搜索", StringComparison.Ordinal) && n.Bounds.Width > 0);
            await backend.TapAsync(target.Serial, control.Bounds.CenterX, control.Bounds.CenterY); await Task.Delay(500);
            await Case("L07", "LD HTTP input exact Chinese/space readback on safe system search", async () => { const string text = "雷电接口 测试"; var result = await Ld("L07", "input", new() { ["text"] = text }); await Task.Delay(500); var fields = NodeSearch.FindAll(await backend.DumpAsync(target.Serial), new()).Where(SearchField).ToArray(); Require(fields.Any(n => n.Text == text), "LD HTTP text mismatch."); return new { result, fields }; });
            await Case("L08", "LD HTTP Home leaves settings page", async () => { var result = await Ld("L08", "keyevent", new() { ["key"] = "home" }); await Task.Delay(500); var tree = await backend.DumpAsync(target.Serial); Require(!NodeSearch.FindAll(tree, new()).Any(n => n.PackageName is "com.android.settings" or "com.android.settings.intelligence"), "LD Home had no effect."); return new { result, tree }; });
            await Shell("am start -W -a android.settings.SETTINGS"); await runtime.StopApisAsync();
        }
        else if (phase == "api-matrix")
        {
            async Task<AndroidNode> Reset()
            {
                await backend.StopAppAsync(target.Serial, "com.android.settings.intelligence"); await backend.StopAppAsync(target.Serial, "com.android.settings");
                await Shell("am start -W -a android.settings.SETTINGS"); await Task.Delay(800); return await backend.DumpAsync(target.Serial);
            }
            async Task<AndroidNode> SearchControl()
            {
                var tree = await Reset(); return NodeSearch.FindAll(tree, new()).First(n => n.PackageName == "com.android.settings" && (n.Text ?? "").Contains("搜索", StringComparison.Ordinal) && n.Bounds.Width > 0);
            }
            async Task RequireSearch() { Require(NodeSearch.FindAll(await backend.DumpAsync(target.Serial), new()).Any(SearchField), "API click did not open the safe search field."); }
            await Reset(); await runtime.RefreshAsync(); observedIdentity = runtime.Devices.Snapshot.Single();
            await runtime.StartApisAsync(); var port = await runtime.Api.StartDeviceAsync(target.Serial);
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(300) };
            async Task<JsonElement> Post(string id, string path, Dictionary<string, object> fields, int status = 200, bool basic = false, Func<JsonElement, Task>? verify = null)
            {
                if (!basic) fields["serial"] = target.Serial;
                await File.WriteAllTextAsync(Path.Combine(output, "request-" + id + ".json"), JsonSerializer.Serialize(fields, JsonDefaults.Options));
                return await Case(id, status == 200 ? "Root success envelope and specified real postcondition" : "Rejection without guest mutation; not business feature success", async () =>
                {
                    using var response = await client.PostAsJsonAsync($"http://127.0.0.1:{(basic ? port : 18171)}/{path}", fields);
                    var root = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync());
                    await File.WriteAllTextAsync(Path.Combine(output, "response-" + id + ".json"), root.GetRawText());
                    Require((int)response.StatusCode == status, $"{id} expected HTTP {status}, actual={response.StatusCode}: {root}");
                    if (status == 200) Require(basic ? root.GetProperty("ok").GetBoolean() : root.GetProperty("code").GetInt32() == 0, "Root envelope is not success.");
                    if (verify is not null) await verify(root);
                    if (status == 200 && path is "input/tap" or "input/swipe" or "input/keyevent" or "element/click-if-exists" or "element/click-text" or "element/click-node" or "text" or "tap" or "swipe" or "app/start" or "app/stop")
                        await Screen("after-" + id);
                    return root;
                }, status == 200 ? id == "A16-diagnostic" ? "PASS-DIAGNOSTIC" : "PASS-LIVE" : "PASS-ERROR");
            }
            foreach (var (id, url) in new[] { ("B01", "http://127.0.0.1:18080/health"), ("B02", "http://127.0.0.1:18080/devices"), ("B03", $"http://127.0.0.1:{port}/health"), ("B04", $"http://127.0.0.1:{port}/device") })
                await Case(id, "Bound device or gateway metadata has a valid success envelope", async () => { var root = JsonSerializer.Deserialize<JsonElement>(await client.GetStringAsync(url)); Require(root.GetProperty("ok").GetBoolean(), "GET failed."); if (id != "B01") Require(root.GetRawText().Contains(target.Serial, StringComparison.Ordinal), "Wrong serial."); return root; });
            await Post("A01", "device/dump", new(), verify: root => { Require(root.GetProperty("data").GetProperty("childNodeList").GetArrayLength() > 0, "Empty API tree."); return Task.CompletedTask; });
            await Post("A02", "device/screenshot", new() { ["width"] = 40, ["height"] = 40 }, verify: root => { var size = PngImage.ReadDimensions(Convert.FromBase64String(root.GetProperty("data").GetProperty("base64").GetString()!)); Require(size == (40, 40), "API crop mismatch."); return Task.CompletedTask; });
            foreach (var (id, route) in new[] { ("A03", "device/shell"), ("A04", "device/execute") }) await Post(id, route, new() { ["command"] = "getprop sys.boot_completed" }, verify: root => { Require(root.GetProperty("data").GetProperty("output").GetString()!.Trim() == "1", "Wrong guest output."); return Task.CompletedTask; });
            await Post("A05-error", "device/set-prop", new() { ["prop"] = "invalid space", ["value"] = "test" }, 400);
            await Post("A06-error", "device/set-timezone", new() { ["timezone"] = "Matrix/InvalidZone" }, 400);
            await Post("A07-error", "device/set-sim-info", new(), 501);
            await Post("A08", "app/list", new() { ["showSystem"] = false, ["needVersion"] = true }, verify: root => { Require(root.GetProperty("data").EnumerateArray().Any(x => x.GetProperty("packageName").GetString() == Package && x.GetProperty("versionName").GetString() == "40.7.0"), "Target app missing."); return Task.CompletedTask; });
            await Post("A09", "app/list-packages", new(), verify: root => { Require(root.GetProperty("data").EnumerateArray().Any(x => x.GetString() == "android"), "System package android missing."); return Task.CompletedTask; });
            await Post("A10", "app/start", new() { ["packageName"] = Package }, verify: async root => { Require(!string.IsNullOrWhiteSpace(await Shell("pidof " + Package)), "API app did not launch."); });
            await Post("A11", "app/stop", new() { ["packageName"] = Package }, verify: async root => { Require(!(await Shell("ps -A -o NAME")).Split('\n').Any(x => x.Trim().StartsWith(Package, StringComparison.Ordinal)), "API stop left processes."); });
            await Post("A12-error", "app/clear", new() { ["packageName"] = "-invalid" }, 400);
            var control = await SearchControl();
            await Post("A13", "input/tap", new() { ["x"] = control.Bounds.CenterX, ["y"] = control.Bounds.CenterY }, verify: root => RequireSearch());
            var staticTree = await Reset(); var dimensions = PngImage.ReadDimensions(await backend.ScreenshotAsync(target.Serial));
            var beforeTexts = NodeSearch.FindAll(staticTree, new()).Select(n => n.Text).ToArray();
            await Post("A14", "input/swipe", new() { ["x1"] = dimensions.Width / 2, ["y1"] = dimensions.Height * 4 / 5, ["x2"] = dimensions.Width / 2, ["y2"] = dimensions.Height * 2 / 5 }, verify: async root => { var after = NodeSearch.FindAll(await backend.DumpAsync(target.Serial), new()).Select(n => n.Text); Require(!beforeTexts.SequenceEqual(after), "API swipe had no list effect."); });
            await Post("A15", "input/keyevent", new() { ["code"] = 3 }, verify: async root => { var nodes = NodeSearch.FindAll(await backend.DumpAsync(target.Serial), new()); Require(!nodes.Any(n => n.PackageName == "com.android.settings"), "Home did not leave Settings."); });
            await Post("A16-diagnostic", "service/info", new(), verify: root => { Require(!root.GetProperty("data").GetProperty("agentServiceAvailable").GetBoolean(), "Unexpected agent."); return Task.CompletedTask; });
            await Post("A17-error", "service/stop", new(), 501);
            await Post("A18", "function/sleep", new() { ["seconds"] = 0.01 }); await Post("A19", "function/sleep-ms", new() { ["milliseconds"] = 10 });
            control = await SearchControl(); var selector = new NodeSelector { Text = control.Text, PackageName = "com.android.settings" };
            await Post("A20", "element/find", new() { ["selector"] = selector }, verify: root => { Require(root.GetProperty("data").GetProperty("text").GetString() == control.Text, "Wrong matched node."); return Task.CompletedTask; });
            await Post("A21", "element/find-all", new() { ["selector"] = selector }, verify: root => { Require(root.GetProperty("data").GetArrayLength() > 0, "No matched nodes."); return Task.CompletedTask; });
            await Post("A22", "element/wait-exists", new() { ["selector"] = selector, ["timeoutMs"] = 0 }, verify: root => { Require(root.GetProperty("data").GetProperty("value").GetBoolean(), "Known node absent."); return Task.CompletedTask; });
            await Post("A23", "element/click-if-exists", new() { ["selector"] = selector, ["timeoutMs"] = 0 }, verify: root => RequireSearch());
            control = await SearchControl(); await Post("A24", "element/click-text", new() { ["text"] = control.Text!, ["timeoutMs"] = 0 }, verify: root => RequireSearch());
            await Post("A25-error", "element/set-text", new() { ["selector"] = new NodeSelector { ResourceId = "android:id/search_src_text" }, ["text"] = "测试" }, 501);
            control = await SearchControl(); selector = new() { Text = control.Text, PackageName = "com.android.settings" };
            await Post("A26", "element/wait-any", new() { ["selectors"] = new[] { selector, new NodeSelector { Text = "not-present-matrix-test" } }, ["timeoutMs"] = 0 }, verify: root => { Require(root.GetProperty("data").GetProperty("index").GetInt32() == 0, "Selector priority mismatch."); return Task.CompletedTask; });
            await Post("A27", "element/click-node", new() { ["node"] = control }, verify: root => RequireSearch());
            await Post("B05", "shell", new() { ["Command"] = "getprop sys.boot_completed" }, basic: true, verify: root => { Require(root.GetProperty("data").GetProperty("output").GetString()!.Trim() == "1", "Basic shell mismatch."); return Task.CompletedTask; });
            await Post("B08", "text", new() { ["Text"] = "API中文 Text 7!?" }, basic: true, verify: async root => { var fields = NodeSearch.FindAll(await backend.DumpAsync(target.Serial), new()).Where(SearchField); Require(fields.Any(n => n.Text == "API中文 Text 7!?"), "Basic Unicode text exact readback mismatch."); });
            control = await SearchControl(); await Post("B06", "tap", new() { ["X"] = control.Bounds.CenterX, ["Y"] = control.Bounds.CenterY }, basic: true, verify: root => RequireSearch());
            staticTree = await Reset(); beforeTexts = NodeSearch.FindAll(staticTree, new()).Select(n => n.Text).ToArray(); dimensions = PngImage.ReadDimensions(await backend.ScreenshotAsync(target.Serial));
            await Post("B07", "swipe", new() { ["StartX"] = dimensions.Width / 2, ["StartY"] = dimensions.Height * 4 / 5, ["EndX"] = dimensions.Width / 2, ["EndY"] = dimensions.Height * 2 / 5, ["DurationMs"] = 400 }, basic: true, verify: async root => { Require(!beforeTexts.SequenceEqual(NodeSearch.FindAll(await backend.DumpAsync(target.Serial), new()).Select(n => n.Text)), "Basic swipe no effect."); });
            await Post("B09", "app/start", new() { ["PackageName"] = Package }, basic: true, verify: async root => { Require(!string.IsNullOrWhiteSpace(await Shell("pidof " + Package)), "Basic app start failed."); });
            await Post("B10", "app/stop", new() { ["PackageName"] = Package }, basic: true, verify: async root => { Require(!(await Shell("ps -A -o NAME")).Split('\n').Any(x => x.Trim().StartsWith(Package, StringComparison.Ordinal)), "Basic stop failed."); });
            await Reset(); await runtime.StopApisAsync();
        }
        else if (phase == "soak")
        {
            var clock = Stopwatch.StartNew(); var reads = 0; var screens = 0; var dumps = 0;
            for (var i = 0; clock.Elapsed < TimeSpan.FromMinutes(30) || reads < 200; i++)
            {
                await Case("read-" + i, "Actual boot property equals 1", async () => { var value = (await Shell("getprop sys.boot_completed")).Trim(); Require(value == "1", "Guest not booted."); reads++; return value; });
                if (i % 6 == 0 && screens < 30) { await Case("screen-" + i, "Live PNG", () => Screen("screen-" + i)); screens++; }
                if (i % 18 == 0 && dumps < 10) { await Case("dump-" + i, "Live node tree", () => backend.DumpAsync(target.Serial)); dumps++; }
                if (i % 30 == 0) await Case("resources-" + i, "Host original instance unchanged; sample owned runtime resources", async () =>
                {
                    var current = LdPlayerService.ParseList2(await runner.RunAsync(["list2"])); var original = current.Single(x => x.Index == 0);
                    Require(original.ProcessId == target.OriginalPid && original.VBoxProcessId == target.OriginalVBoxPid, "Original instance changed during soak.");
                    using var process = Process.GetCurrentProcess(); return new { elapsedMs = clock.ElapsedMilliseconds, workingSet = process.WorkingSet64, privateBytes = process.PrivateMemorySize64, handles = process.HandleCount, threads = process.Threads.Count, serverPid = target.ServerPid, originalPid = original.ProcessId, originalVBoxPid = original.VBoxProcessId };
                });
                await Task.Delay(TimeSpan.FromSeconds(8));
            }
            await Case("soak-summary", "30min,200 reads,30 screens,10 dumps without failure", () => { Require(screens >= 30 && dumps >= 10, "Insufficient soak samples."); return Task.FromResult(new { elapsedMs = clock.ElapsedMilliseconds, reads, screens, dumps, memory = Process.GetCurrentProcess().WorkingSet64 }); });
        }
        else throw new ArgumentException("Unsupported acceptance phase: " + phase);
    }
    private sealed record Target(int Index, string Name, string Serial, int VBoxPid, long VBoxStartedUtcTicks, int ServerPid,
        long ServerStartedUtcTicks, int OriginalPid, int OriginalVBoxPid, string ConsolePath, string Apk);
}
