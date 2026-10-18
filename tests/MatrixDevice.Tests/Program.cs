using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using MatrixDevice.Core;
using MatrixDevice.Features;
using MatrixDevice.Host;

if (args.Length > 0 && args[0] == "--application-preflight")
{
    try { await ApplicationPreflightAcceptance.RunAsync(args.Skip(1).ToArray()); }
    catch (Exception ex) { Console.Error.WriteLine(ex); Environment.ExitCode = 1; }
    return;
}
if (args.Length > 0 && args[0] == "--application-lifecycle")
{
    try { await ApplicationPreflightAcceptance.RunLifecycleAsync(args.Skip(1).ToArray()); }
    catch (Exception ex) { Console.Error.WriteLine(ex); Environment.ExitCode = 1; }
    return;
}
if (args.Length > 0 && args[0] == "--application-owned-cleanup")
{
    try { await ApplicationPreflightAcceptance.RunOwnedCleanupAsync(args.Skip(1).ToArray()); }
    catch (Exception ex) { Console.Error.WriteLine(ex); Environment.ExitCode = 1; }
    return;
}
if (args.Length > 0 && args[0] == "--multi-acceptance")
{
    try { await MultiInstanceAcceptance.RunAsync(args.Skip(1).ToArray()); }
    catch (Exception ex) { Console.Error.WriteLine(ex); Environment.ExitCode = 1; }
    return;
}
if (args.Length > 0 && args[0] == "--live-acceptance")
{
    try { await LiveAcceptance.RunAsync(args.Skip(1).ToArray()); }
    catch (Exception ex) { Console.Error.WriteLine(ex); Environment.ExitCode = 1; }
    return;
}
if (args.Length > 0 && args[0] == "--adb-owned-test-server")
{
    await using var server = new FakeAdbServer(int.Parse(args[1], System.Globalization.CultureInfo.InvariantCulture), args[2]);
    Console.WriteLine(server.Endpoint.Port); Console.Out.Flush();
    await Task.Delay(Timeout.Infinite);
    return;
}

if (args.Length > 0 && args[0] == "--ldconsole-test-child")
{
    if (args[1] == "sleep") { await Task.Delay(TimeSpan.FromSeconds(20)); return; }
    if (args[1] == "spawn")
    {
        var executable = Environment.ProcessPath!;
        var start = new ProcessStartInfo(executable) { UseShellExecute = false, CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden };
        if (Path.GetFileNameWithoutExtension(executable).Equals("dotnet", StringComparison.OrdinalIgnoreCase)) start.ArgumentList.Add(typeof(ControlledLdRunner).Assembly.Location);
        start.ArgumentList.Add("--ldconsole-test-child"); start.ArgumentList.Add("sleep");
        using var child = Process.Start(start)!;
        await File.WriteAllTextAsync(args[2], child.Id.ToString(System.Globalization.CultureInfo.InvariantCulture));
        await Task.Delay(TimeSpan.FromSeconds(20)); return;
    }
    if (args[1] is "flood" or "stderr-flood")
    {
        var block = new byte[1024 * 1024];
        using var output = args[1] == "flood" ? Console.OpenStandardOutput() : Console.OpenStandardError();
        for (var i = 0; i < (args[1] == "flood" ? 34 : 2); i++) await output.WriteAsync(block);
        return;
    }
    if (args[1] == "gbk")
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        using var output = Console.OpenStandardOutput();
        await output.WriteAsync(Encoding.GetEncoding(936).GetBytes("0,雷电测试,0,0,0,0,0,1600,900,240\r\n"));
        return;
    }
    if (args[1] == "fail") { Console.Error.Write("测试错误"); Environment.ExitCode = 7; return; }
    Console.Write(JsonSerializer.Serialize(args.Skip(2).ToArray()));
    return;
}

var tests = new List<(string Name, Func<Task> Run)>();
var directoryConstructionRetainedOwners = new List<object>();
void Test(string name, Func<Task> run) => tests.Add((name, run));
Test("window geometry preserves supported compact sizes through normalize save and fresh load", () =>
{
    foreach (var (width, height) in new[] { (390, 640), (390, 844), (768, 1024) })
    {
        var requested = new AppSettings { WindowWidth = width, WindowHeight = height };
        var normalized = SettingsStore.Normalize(requested);
        Check(normalized.WindowWidth == width && normalized.WindowHeight == height, $"Supported window {width}x{height} was enlarged during normalize.");
        var path = Path.Combine(Temp(), "AppConfig.json"); var store = new SettingsStore(path); store.Load(); store.Save(requested);
        var loaded = new SettingsStore(path).Load();
        Check(loaded.WindowWidth == width && loaded.WindowHeight == height, $"Fresh load enlarged window {width}x{height}.");
    }
    return Task.CompletedTask;
});
ExistingAdbTests.Register(Test);
ScreenVideoTests.Register(Test);
LdWorkspaceTests.Register(Test);
LdLaunchMemoryTests.Register(Test);
MonitorResourceTests.Register(Test);
LdRunningWindowTests.Register(Test);
ApplicationBatchTests.Register(Test);
Test("LD admission applies only unique explicit local proofs and never mutates old authorization", async () =>
{
    var original = new AppSettings { AdbAllowedSerials = ["emulator-5556"], LdSerialBindings = new() { [1] = "emulator-5556" } };
    var proof = new LdConnectionProof(2, "test", 1, "emulator-5558", 2, 123, 456);
    var applied = LdConnectionAdmission.Apply(original, [proof]);
    Check(applied.AdbAllowedSerials.Length == 2 && applied.LdSerialBindings[2] == proof.Serial && !applied.AutoStartApi, "New authorization missing.");
    Check(original.AdbAllowedSerials.Length == 1 && !original.LdSerialBindings.ContainsKey(2), "Original settings mutated.");
    foreach (var invalid in new[] { proof with { Index = 0 }, proof with { Serial = "" }, proof with { Serial = "10.0.0.1:5555" }, proof with { Generation = 0 }, proof with { TransportId = 0 } })
        await Throws<DeviceException>(() => { LdConnectionAdmission.Apply(original, [invalid]); return Task.CompletedTask; });
    await Throws<DeviceException>(() => { LdConnectionAdmission.Apply(original, [proof, proof]); return Task.CompletedTask; });
    await Throws<DeviceException>(() => { LdConnectionAdmission.Apply(original, [proof with { Serial = "emulator-5556" }]); return Task.CompletedTask; });
});
Test("saved connection path changes require restart before LD workspace edits", async () =>
{
    await using var runtime = await MatrixRuntime.CreateAsync(new(), Temp(), true);
    runtime.RequireFrozenConnectionSettings();
    runtime.Settings = runtime.Settings with { LdConsolePath = "other-installation.exe" };
    await Throws<DeviceException>(() => { runtime.RequireFrozenConnectionSettings(); return Task.CompletedTask; });
});
Test("LD admission cannot replace the confirmed subject while waiting for ADB", async () =>
{
    var frozen = LdPlayerService.ParseList2("1,Original,1,2,1,123,456,1280,720,240").Single();
    LdConnectionAdmission.RequireExpectedInstances([frozen], [1], [frozen]);
    foreach (var changed in new[] { frozen with { Name = "Replacement" }, frozen with { Generation = frozen.Generation + 1 }, frozen with { VBoxProcessId = 999 } })
        await Throws<DeviceException>(() => { LdConnectionAdmission.RequireExpectedInstances([changed], [1], [frozen]); return Task.CompletedTask; });
    await Throws<DeviceException>(() => { LdConnectionAdmission.RequireExpectedInstances([frozen], [1], []); return Task.CompletedTask; });
});
Test("HTTP admission drain counter includes in-flight SDK identity checks", async () =>
{
    var port = Port(); await using var runtime = await MatrixRuntime.CreateAsync(new() { BasicApiPort = port }, Temp(), true);
    await runtime.Api.StartBasicAsync(port);
    var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    runtime.Api.LdCommandIdentityGuard = async (_, _, _) => { entered.TrySetResult(); await release.Task; };
    using var http = new HttpClient();
    var generation = runtime.LdPlayer!.Instances.Single(item => item.Index == 0).Generation;
    var request = http.PostAsJsonAsync($"http://localhost:{port}/ldplayer/instances/0/keyevent", new { expectedGeneration = generation, key = "home" });
    try { await entered.Task.WaitAsync(TimeSpan.FromSeconds(3)); Check(runtime.Api.HasActiveRequests, "In-flight request was not counted."); }
    finally { release.TrySetResult(); }
    using var response = await request; Check(response.IsSuccessStatusCode, "Released request did not complete.");
    var deadline = DateTime.UtcNow.AddSeconds(3);
    while (runtime.Api.HasActiveRequests && DateTime.UtcNow < deadline) await Task.Delay(10);
    Check(!runtime.Api.HasActiveRequests, "Completed request did not drain.");
});
Test("both differentiated workload script roles follow the existing swipe format", () =>
{
    var first = MultiInstanceAcceptance.SettingsScript(1, 0, 150, 50, 360, 640);
    var second = MultiInstanceAcceptance.SettingsScript(2, 1, 150, 50, 360, 640);
    ScriptPackageService.Validate(first); ScriptPackageService.Validate(second);
    Check(first.Steps.Count == 3 && second.Steps.Count == 3 && first.Steps[1].Action == "click" && second.Steps[1].Action == "swipe", "Differentiated scripts lost their distinct actions");
    return Task.CompletedTask;
});
Test("observed settings hint is accepted only in its exact search control", () =>
{
    AndroidNode Search(string text, string package = "com.android.settings.intelligence") => new() { ResourceId = "android:id/search_src_text", ClassName = "android.widget.AutoCompleteTextView", PackageName = package, Text = text };
    Check(MultiInstanceAcceptance.IsEmptySettingsSearch(Search("")) && MultiInstanceAcceptance.IsEmptySettingsSearch(Search("\u641c\u7d22\u2026")), "Observed empty field hint rejected");
    Check(!MultiInstanceAcceptance.IsEmptySettingsSearch(Search("\u5b9e\u4f8b1 Round 3")) && !MultiInstanceAcceptance.IsEmptySettingsSearch(Search("\u641c\u7d22\u2026", "other.package")), "Retained text or unrelated control accepted");
    return Task.CompletedTask;
});
Test("null authorization fails closed and malformed serial configuration recovers to safe defaults", async () =>
{
    Check(SettingsStore.Normalize(new AppSettings { AdbAllowedSerials = null! }).AdbAllowedSerials.Length == 0, "Null authorization was not empty");
    var path = Path.Combine(Temp(), "AppConfig.json"); File.WriteAllText(path, "{\"adbAllowedSerials\":[\"valid\",\"-invalid\"]}");
    var store = new SettingsStore(path); Check(store.Load().AdbAllowedSerials.Length == 0, "Malformed authorization survived recovery");
    File.WriteAllText(path + ".bak", JsonSerializer.Serialize(new AppSettings { AdbAllowedSerials = ["old-authorized"], LdSerialBindings = new() { [1] = "old-authorized" }, AgentSerial = "old-authorized", AgentBaseUrl = "http://127.0.0.1:9008/", BasicApiPort = 18080 }, JsonDefaults.Options));
    var recovered = store.Load();
    Check(recovered.AdbAllowedSerials.Length == 0 && recovered.LdSerialBindings.Count == 0 && recovered.AgentSerial is null && recovered.AgentBaseUrl is null && recovered.BasicApiPort == 18080,
        "Backup silently restored device authority or lost ordinary settings");
    File.Delete(path);
    Check(store.Load().AdbAllowedSerials.Length == 0, "Missing primary restored backup device authority");
    await using var runtime = await MatrixRuntime.CreateAsync(recovered, Temp());
    Check(runtime.Devices.Snapshot.Count == 0 && runtime.DiscoveryError is not null, "Recovered configuration cannot open host-only Settings runtime");
});
void Check(bool condition, string message) { if (!condition) throw new Exception(message); }
async Task Throws<T>(Func<Task> action) where T : Exception
{
    try { await action(); } catch (T) { return; }
    throw new Exception("Expected " + typeof(T).Name);
}
string Temp() { var path = Path.Combine(Path.GetTempPath(), "MatrixDevice.Tests", Guid.NewGuid().ToString("N")); Directory.CreateDirectory(path); return path; }
async Task<MatrixRuntime> DirectoryRuntime(IDeviceBackend backend, ControlledLdRunner? runner)
{
    var owned = new List<(string Name, Func<Task> Dispose)> { ("backend", () => backend.DisposeAsync().AsTask()) };
    try
    {
        var path = Temp();
        var settings = new AppSettings { LdSerialBindings = new() { [1] = "DEMO-001" } };
        var catalog = new DeviceCatalog(backend); var writes = new DeviceWriteCoordinator(); var automation = new AutomationService(backend, catalog);
        var metadata = new CatalogRepository(Path.Combine(path, "db.sqlite3")); owned.Add(("metadata", () => { metadata.Dispose(); return Task.CompletedTask; }));
        var log = new RollingLog(Path.Combine(path, "logs")); owned.Add(("log", () => log.DisposeAsync().AsTask()));
        var service = runner is null ? null : new LdPlayerService(runner, true);
        if (service is not null) owned.Add(("ld-directory", () => service.DisposeAsync().AsTask()));
        var scripts = new ScriptScheduler(backend, catalog, true, writes); owned.Add(("scripts", () => scripts.DisposeAsync().AsTask()));
        var input = new InputCoordinator(catalog, backend, writes); owned.Add(("input", () => input.DisposeAsync().AsTask()));
        var api = new ApiServiceManager(catalog, backend, automation, log, writes) { LdPlayer = service }; owned.Add(("api", () => api.DisposeAsync().AsTask()));
        return new MatrixRuntime
        {
            Backend = backend, Devices = catalog, Writes = writes, Automation = automation, Scripts = scripts, Input = input, Api = api,
            SettingsStore = new SettingsStore(Path.Combine(path, "AppConfig.json")), Metadata = metadata,
            Log = log, Settings = settings, FrozenConnectionSettings = settings with { LdSerialBindings = new(settings.LdSerialBindings) },
            LdPlayer = service, IsDemo = true
        };
    }
    catch (Exception constructionError)
    {
        var cleanupErrors = new List<string>();
        foreach (var resource in owned.AsEnumerable().Reverse())
        {
            Task? disposal = null;
            try { disposal = resource.Dispose(); await disposal.WaitAsync(TimeSpan.FromSeconds(15)); }
            catch (Exception cleanupError)
            {
                cleanupErrors.Add(resource.Name + ": " + cleanupError);
                directoryConstructionRetainedOwners.Add(new { resources = owned.ToArray(), disposal, cleanupError });
                break;
            }
        }
        constructionError.Data["partialConstructionCleanup"] = cleanupErrors.ToArray();
        constructionError.Data["partialConstructionRetainedOwners"] = directoryConstructionRetainedOwners.Count;
        Console.Error.WriteLine($"DIRECTORY-CONSTRUCTION-FAIL retainedOwners={directoryConstructionRetainedOwners.Count}; cleanupErrors={cleanupErrors.Count}");
        foreach (var message in cleanupErrors) Console.Error.WriteLine(message);
        throw;
    }
}
ScriptPackage Package(params ScriptStep[] steps) => new() { Manifest = new() { Id = "test-package", Name = "Test", Version = "1.0" }, Steps = steps.ToList() };
JsonElement Body(object body) => JsonSerializer.SerializeToElement(body, JsonDefaults.Options);
int Port() { var listener = new TcpListener(IPAddress.Loopback, 0); listener.Start(); var port = ((IPEndPoint)listener.LocalEndpoint).Port; listener.Stop(); return port; }

Test("coordinate mapping preserves corners and proportions", () =>
{
    Check(InputCoordinator.ScalePoint(180, 320, 360, 640, 412, 720) == (206, 360), "Center mapped incorrectly");
    Check(InputCoordinator.ScalePoint(359, 639, 360, 640, 412, 720) == (410, 718), "Last pixel mapping changed");
    return Throws<DeviceException>(() => Task.FromResult(InputCoordinator.ScalePoint(-1, 0, 360, 640, 412, 720)));
});
Test("touch packet has exact big-endian field layout", () =>
{
    var packet = ScrcpyProtocol.EncodeTouch(0, 0x0102030405060708, 100, 200, 360, 640, 4);
    Check(packet.Length == 32 && packet[0] == 2 && packet[1] == 0, "Packet header incorrect");
    Check(Convert.ToHexString(packet.AsSpan(2, 8)) == "0102030405060708", "Pointer endian incorrect");
    Check(BinaryPrimitives.ReadInt32BigEndian(packet.AsSpan(10)) == 100 && BinaryPrimitives.ReadUInt16BigEndian(packet.AsSpan(20)) == 640, "Position fields incorrect");
    var up = ScrcpyProtocol.EncodeTouch(1, 1, 0, 0, 360, 640, 4);
    Check(up.AsSpan(22, 2).SequenceEqual(new byte[2]) && up.AsSpan(28, 4).SequenceEqual(new byte[4]), "UP retained pressure/buttons");
    return Throws<InvalidDataException>(() => Task.FromResult(ScrcpyProtocol.EncodeTouch(0, 1, 0, 0, 0, 1)));
});
Test("video reader handles flags, short reads and truncated payloads", async () =>
{
    var bytes = new byte[15]; BinaryPrimitives.WriteUInt64BigEndian(bytes, (1UL << 63) | (1UL << 62) | 99); BinaryPrimitives.WriteInt32BigEndian(bytes.AsSpan(8), 3);
    bytes[12] = 1; bytes[13] = 2; bytes[14] = 3;
    using var stream = new MemoryStream(bytes);
    var packet = await ScrcpyProtocol.ReadPacketAsync(stream);
    Check(packet!.Value.Header.IsCodecConfig && packet.Value.Header.IsKeyFrame && packet.Value.Payload.SequenceEqual(new byte[] { 1, 2, 3 }), "Video flags/payload incorrect");
    using var truncated = new MemoryStream(bytes[..14]);
    await Throws<EndOfStreamException>(async () => { await ScrcpyProtocol.ReadPacketAsync(truncated); });
});
Test("PNG crop and pixel sampling use decoded coordinates", () =>
{
    var png = PngImage.Create(4, 3, (x, y) => ((byte)(x * 50), (byte)(y * 50), (byte)99));
    var cropped = PngImage.Crop(png, 1, 1, 2, 2);
    Check(PngImage.ReadDimensions(cropped) == (2, 2), "Crop dimensions incorrect");
    Check(PngImage.ColorAt(cropped, 1, 1) == ((byte)100, (byte)100, (byte)99), "Crop selected wrong pixels");
    return Throws<InvalidDataException>(() => Task.FromResult(PngImage.Crop(png, 3, 2, 2, 2)));
});
Test("per-serial queue preserves order and allows other devices", async () =>
{
    await using var queue = new SerialCommandQueue();
    List<int> order = [];
    var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    var first = queue.RunAsync("a", async ct => { order.Add(1); await release.Task.WaitAsync(ct); order.Add(2); });
    var second = queue.RunAsync("a", ct => { order.Add(3); return Task.CompletedTask; });
    var different = queue.RunAsync("b", ct => Task.CompletedTask);
    await different.WaitAsync(TimeSpan.FromSeconds(2));
    Check(!second.IsCompleted, "Same-device command overtook blocked predecessor");
    release.SetResult(); await Task.WhenAll(first, second);
    Check(order.SequenceEqual(new[] { 1, 2, 3 }), "Same-device order changed");
});
Test("cancelled queue item cannot let successors overtake", async () =>
{
    await using var queue = new SerialCommandQueue();
    var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    var first = queue.RunAsync("a", ct => release.Task.WaitAsync(ct));
    using var cancel = new CancellationTokenSource();
    var middle = queue.RunAsync("a", ct => Task.CompletedTask, cancel.Token); cancel.Cancel();
    var last = queue.RunAsync("a", ct => Task.CompletedTask);
    await Task.Delay(20); Check(!last.IsCompleted, "Cancellation released the queue before predecessor");
    release.SetResult(); await first; await Throws<OperationCanceledException>(() => middle); await last;
});
Test("catalog retains offline identity and changes reconnect generation", async () =>
{
    await using var backend = new DemoDeviceBackend(); var catalog = new DeviceCatalog(backend); await catalog.RefreshAsync();
    var before = catalog.GetRequired("DEMO-001"); var notifications = 0; catalog.DeviceDisconnected += _ => notifications++;
    backend.SetOnline("DEMO-001", false); await catalog.RefreshAsync();
    Check(catalog.Snapshot.Single(x => x.Serial == "DEMO-001").Generation > before.Generation && notifications == 1, "Offline transition did not invalidate old generation");
    await Throws<DeviceException>(() => Task.FromResult(catalog.GetRequired("DEMO-001")));
    await catalog.RefreshAsync(); Check(notifications == 1, "Offline notification repeated without change");
    backend.SetOnline("DEMO-001", true); await catalog.RefreshAsync();
    Check(catalog.GetRequired("DEMO-001").Generation > before.Generation + 1, "Reconnect retained old generation");
});
Test("runtime hides deleted bound offline LD devices without deleting catalog identities", async () =>
{
    var backend = new DemoDeviceBackend(); backend.SetOnline("DEMO-001", false); backend.SetOnline("DEMO-002", false);
    var runner = new ControlledLdRunner { List = "1,bound,0,0,0,0,0,360,640,240" };
    await using var runtime = await DirectoryRuntime(backend, runner);
    await runtime.RefreshAsync(); await runtime.RefreshLdPlayerAsync();
    Check(runtime.GetDevices().Count == 2, "A stopped but existing LD instance disappeared.");
    var catalog = runtime.Devices.Snapshot.ToArray();
    runner.List = ""; await runtime.RefreshLdPlayerAsync();
    Check(runtime.GetDevices().Select(device => device.Serial).SequenceEqual(["DEMO-002"]), "Deleted LD offline identity remained visible or ordinary offline identity disappeared.");
    Check(runtime.Devices.Snapshot.SequenceEqual(catalog), "Projection changed catalog identity or generation.");
    Check(runtime.FrozenConnectionSettings.LdSerialBindings[1] == "DEMO-001", "Projection removed the frozen authorization.");
    runner.List = "not-a-list2-row"; await Throws<DeviceException>(() => runtime.RefreshLdPlayerAsync());
    Check(runtime.GetDevices().Select(device => device.Serial).SequenceEqual(["DEMO-002"]), "Failed query resurrected an identity already confirmed deleted.");
    using (var cancellation = new CancellationTokenSource())
    {
        cancellation.Cancel(); await Throws<OperationCanceledException>(() => runtime.RefreshLdPlayerAsync(cancellation.Token));
        Check(runtime.GetDevices().Select(device => device.Serial).SequenceEqual(["DEMO-002"]), "Cancelled query resurrected an identity already confirmed deleted.");
    }
    Check(runtime.Devices.Snapshot.SequenceEqual(catalog), "Failed or cancelled query changed catalog identity or generation.");
    runner.List = "1,bound,0,0,0,0,0,360,640,240"; await runtime.RefreshLdPlayerAsync();
    Check(runtime.GetDevices().Count == 2, "A stopped instance did not become visible after returning to the directory.");
    Check(runner.Commands.Count == 0, "Directory projection issued a console write.");
});
Test("runtime retains offline devices before a successful LD snapshot or without LD", async () =>
{
    foreach (var configured in new[] { false, true })
    {
        var backend = new DemoDeviceBackend(); backend.SetOnline("DEMO-001", false); backend.SetOnline("DEMO-002", false);
        var runner = configured ? new ControlledLdRunner { List = "not-a-list2-row" } : null;
        await using var runtime = await DirectoryRuntime(backend, runner);
        await runtime.RefreshAsync();
        Check(runtime.GetDevices().Count == 2, "Uninitialized or unconfigured LD was treated as an authoritative empty directory.");
        if (configured)
        {
            await Throws<DeviceException>(() => runtime.RefreshLdPlayerAsync());
            Check(runtime.LdPlayerError is not null && runtime.GetDevices().Count == 2, "Failed first directory query removed offline identities.");
            using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
            await Throws<OperationCanceledException>(() => runtime.RefreshLdPlayerAsync(cancellation.Token));
            Check(runtime.GetDevices().Count == 2, "Cancelled first directory query was treated as a successful empty snapshot.");
            Check(runner!.Commands.Count == 0, "Failed directory lookup issued a console write.");
        }
    }
});
Test("runtime preserves stopped LD and ordinary offline devices after a directory failure", async () =>
{
    var backend = new DemoDeviceBackend(); backend.SetOnline("DEMO-001", false); backend.SetOnline("DEMO-002", false);
    var runner = new ControlledLdRunner { List = "1,bound,0,0,0,0,0,360,640,240" };
    await using var runtime = await DirectoryRuntime(backend, runner);
    await runtime.RefreshAsync(); await runtime.RefreshLdPlayerAsync();
    var instances = runtime.LdPlayer!.Instances.ToArray(); var devices = runtime.GetDevices().ToArray();
    runner.List = "not-a-list2-row"; await Throws<DeviceException>(() => runtime.RefreshLdPlayerAsync());
    Check(runtime.LdPlayerError is not null && runtime.LdPlayer.Instances.SequenceEqual(instances), "Directory failure replaced the last successful snapshot.");
    Check(runtime.GetDevices().SequenceEqual(devices), "Directory failure pruned an offline device.");
    Check(runner.Commands.Count == 0, "Directory failure issued a console write.");
});
Test("runtime keeps non-Offline LD diagnostics and ordinary offline devices with an empty directory", async () =>
{
    foreach (var state in new[] { "Ready", "Unauthorized", "Error", "Discovered", "Failed", "Unknown" })
    {
        var backend = new KeyGateBackend { DiscoveryState = state }; backend.Devices.SetOnline("DEMO-002", false);
        var runner = new ControlledLdRunner { List = "" };
        await using var runtime = await DirectoryRuntime(backend, runner);
        await runtime.RefreshAsync(); await runtime.RefreshLdPlayerAsync();
        Check(runtime.GetDevices().Select(device => device.Serial).SequenceEqual(["DEMO-001", "DEMO-002"]), "Empty LD directory hid a diagnostic or ordinary offline device: " + state);
        Check(runtime.GetDevices().Single(device => device.Serial == "DEMO-001").State == state, "Diagnostic retention did not cover the actual requested catalog state: " + state);
        Check(runner.Commands.Count == 0, "Diagnostic retention issued a console write.");
    }
});
Test("runtime deletion projection uses frozen LD bindings instead of unsaved connection changes", async () =>
{
    var backend = new DemoDeviceBackend(); backend.SetOnline("DEMO-001", false); backend.SetOnline("DEMO-002", false);
    var runner = new ControlledLdRunner { List = "" };
    await using var runtime = await DirectoryRuntime(backend, runner);
    await runtime.RefreshAsync(); await runtime.RefreshLdPlayerAsync();
    runtime.Settings = runtime.Settings with { LdSerialBindings = new() { [1] = "DEMO-002" } };
    Check(runtime.GetDevices().Select(device => device.Serial).SequenceEqual(["DEMO-002"]), "Mutable settings were mistaken for the runtime's frozen LD ownership.");
    Check(runtime.Settings.LdSerialBindings[1] == "DEMO-002" && runtime.FrozenConnectionSettings.LdSerialBindings[1] == "DEMO-001", "Projection changed either settings snapshot.");
    Check(runtime.Devices.Snapshot.Count == 2 && runner.Commands.Count == 0, "Projection removed catalog data or sent a console write.");
});
Test("runtime factory initial LD snapshot retires absent bound offline identities", async () =>
{
    await using var runtime = await MatrixRuntime.CreateAsync(new() { AutoConnect = false, AutoStartApi = false, LdSerialBindings = new() { [99] = "DEMO-001" } }, Temp(), true);
    Check(runtime.IsDemo && runtime.Backend is DemoDeviceBackend && runtime.LdPlayer is not null && runtime.LdPlayer.Instances.Count > 0
        && runtime.LdPlayer.Instances.All(instance => instance.Index != 99), "Factory did not create the expected owned Demo directory.");
    ((DemoDeviceBackend)runtime.Backend).SetOnline("DEMO-001", false);
    await runtime.RefreshAsync();
    Check(runtime.GetDevices().Select(device => device.Serial).SequenceEqual(["DEMO-002"]), "Factory's initial successful LD snapshot was not used without a manual LD refresh.");
    Check(runtime.Devices.Snapshot.Count == 2 && runtime.Devices.Snapshot.Single(device => device.Serial == "DEMO-001").State == "Offline"
        && runtime.FrozenConnectionSettings.LdSerialBindings[99] == "DEMO-001", "Factory projection removed catalog data or frozen authorization.");
});
Test("sync excludes duplicate source and commits one action per device", async () =>
{
    await using var backend = new DemoDeviceBackend(); var catalog = new DeviceCatalog(backend); await catalog.RefreshAsync();
    await using var input = new InputCoordinator(catalog, backend);
    await input.HandoffAsync("DEMO-001");
    await input.SendAsync(new() { SourceSerial = "DEMO-001", Kind = InputKind.Tap, TargetSerials = ["DEMO-001", "DEMO-002", "DEMO-002"], X = 30, Y = 40 });
    foreach (var serial in new[] { "DEMO-001", "DEMO-002" })
        Check((await backend.DumpAsync(serial)).ChildNodeList[1].Text == "Tap 1", "Duplicate or missing sync delivery: " + serial);
});
Test("source handoff cancels unfinished gestures and rejects stale actions", async () =>
{
    await using var backend = new DemoDeviceBackend(); var catalog = new DeviceCatalog(backend); await catalog.RefreshAsync();
    await using var input = new InputCoordinator(catalog, backend); var generation = await input.HandoffAsync("DEMO-001");
    var down = new InputSnapshot { SourceSerial = "DEMO-001", Kind = InputKind.Down, TargetSerials = ["DEMO-002"], X = 20, Y = 20, Generation = generation, GestureId = 12 };
    await input.SendAsync(down); await input.HandoffAsync("DEMO-002");
    await Throws<DeviceException>(() => input.SendAsync(down with { Kind = InputKind.Tap }));
    Check((await backend.DumpAsync("DEMO-001")).ChildNodeList[1].Text == "Tap 0", "Handoff committed an unfinished gesture");
});
Test("node search is preorder, case-sensitive and nullable-aware", () =>
{
    var root = new AndroidNode { Text = "Root", Enabled = true, ChildNodeList = [new() { Text = "Match", Clickable = true }, new() { Text = "match", Clickable = false }] };
    var all = NodeSearch.FindAll(root, new NodeSelector());
    Check(all.Count == 3 && ReferenceEquals(all[0], root), "Preorder root lost");
    Check(ReferenceEquals(NodeSearch.Find(root, new() { Text = "Match", Clickable = true }), root.ChildNodeList[0]), "AND match failed");
    Check(NodeSearch.FindAll(root, new() { Clickable = false }).Count == 2, "Nullable false condition confused with absent");
    Check(NodeSearch.Find(root, new() { Text = "MATCH" }) is null, "Search became case-insensitive");
    return Task.CompletedTask;
});
Test("debug node selection preserves reference and does not tap", async () =>
{
    await using var backend = new DemoDeviceBackend(); var debug = new DebugWorkspace(backend); await debug.RefreshAsync("DEMO-001");
    var overlay = debug.Overlays.First(x => x.SourceNode.Clickable); debug.SelectOverlay(overlay);
    Check(ReferenceEquals(debug.SelectedNode, overlay.SourceNode) && overlay.IsSelected, "Tree/overlay selection disconnected");
    Check((await backend.DumpAsync("DEMO-001")).ChildNodeList[1].Text == "Tap 0", "Selecting overlay injected a tap");
});
Test("script package roundtrip preserves metadata and validates actions", async () =>
{
    var path = Path.Combine(Temp(), "sample.jxscript");
    var package = Package(new ScriptStep { Action = "click", Value = "10,20", Probability = 0.75 });
    ScriptPackageService.Save(path, package); var loaded = ScriptPackageService.Load(path);
    Check(loaded.Manifest.Id == package.Manifest.Id && loaded.Steps.Single().Value == "10,20", "Script package changed");
    await Throws<DeviceException>(() => { ScriptPackageService.Validate(Package(new ScriptStep { Action = "eval", Value = "anything" })); return Task.CompletedTask; });
});
Test("script import rejects ZIP traversal and missing entries", async () =>
{
    var path = Path.Combine(Temp(), "bad.jxscript");
    using (var archive = ZipFile.Open(path, ZipArchiveMode.Create)) archive.CreateEntry("../manifest.json");
    await Throws<DeviceException>(() => Task.FromResult(ScriptPackageService.Load(path)));
    var empty = Path.Combine(Temp(), "empty.jxscript"); using (ZipFile.Open(empty, ZipArchiveMode.Create)) { }
    await Throws<DeviceException>(() => Task.FromResult(ScriptPackageService.Load(empty)));
});
Test("script duplicate start runs only once and stops before next step", async () =>
{
    await using var backend = new DemoDeviceBackend(); await using var scripts = new ScriptScheduler(backend);
    var package = Package(new() { Action = "wait", DurationMs = 10000 }, new() { Action = "click", Value = "20,20" });
    var running = scripts.RunAsync("DEMO-001", package); await Task.Delay(30);
    await scripts.RunAsync("DEMO-001", package);
    var stopwatch = Stopwatch.StartNew(); scripts.Stop("DEMO-001"); await running.WaitAsync(TimeSpan.FromSeconds(2));
    Check(stopwatch.Elapsed < TimeSpan.FromSeconds(2), "Cancellation exceeded two seconds");
    Check(scripts.GetStatus("DEMO-001").State == ScriptRunState.Cancelled, "Cancellation reported success");
    Check((await backend.DumpAsync("DEMO-001")).ChildNodeList[1].Text == "Tap 0", "Cancelled script ran next action");
    await scripts.RunAsync("DEMO-001", Package(new ScriptStep { Action = "click", Value = "20,20" }));
    Check((await backend.DumpAsync("DEMO-001")).ChildNodeList[1].Text == "Tap 1", "Restart after cancellation failed");
});
Test("script errors propagate and per-device logs stay at 500", async () =>
{
    await using var backend = new DemoDeviceBackend(); await using var scripts = new ScriptScheduler(backend);
    await Throws<DeviceException>(() => scripts.RunAsync("DEMO-001", Package(new ScriptStep { Action = "click", Value = "99999,99999" })));
    Check(scripts.GetStatus("DEMO-001").State == ScriptRunState.Failed, "Failed action marked completed");
    var steps = Enumerable.Range(0, 550).Select(_ => new ScriptStep { Action = "wait", DurationMs = 0 }).ToArray();
    await scripts.RunAsync("DEMO-002", Package(steps));
    Check(scripts.GetLogs("DEMO-002").Count == 500, "Log retention differs from 500");
});
foreach (var (name, contents) in new (string, byte[])[]
{
    ("top-level JSON", JsonSerializer.SerializeToUtf8Bytes(Package(new ScriptStep { Action = "click", Value = "20,20" }), JsonDefaults.Options)),
    ("Python", Encoding.UTF8.GetBytes("print('not an executable script package')")),
    ("JavaScript", Encoding.UTF8.GetBytes("console.log('not an executable script package');")),
    ("random binary", [0, 255, 17, 128, 23, 44, 91, 201]),
    ("truncated ZIP", [0x50, 0x4b, 3, 4, 0, 0, 0, 0]),
    ("empty file", [])
})
{
    Test($"script format explicitly rejects {name} without execution", async () =>
    {
        var path = Path.Combine(Temp(), "unsupported.jxscript");
        await File.WriteAllBytesAsync(path, contents);
        await using var backend = new DemoDeviceBackend();
        await using var scripts = new ScriptScheduler(backend);
        Exception? failure = null;
        try
        {
            var imported = ScriptPackageService.Load(path);
            await scripts.RunAsync("DEMO-001", imported).WaitAsync(TimeSpan.FromSeconds(2));
        }
        catch (Exception error) { failure = error; }
        Check((await backend.DumpAsync("DEMO-001")).ChildNodeList[1].Text == "Tap 0", "Rejected content executed a step");
        var formatError = failure is DeviceException { Status: 400, Code: 1007 } formatFailure ? formatFailure :
            throw new Exception($"Expected script format DeviceException 400/1007, got {failure?.GetType().Name ?? "success"}: {failure?.Message}");
        Check(formatError.Message.StartsWith("脚本包格式无效：", StringComparison.Ordinal) &&
            formatError.Message.Contains("ZIP", StringComparison.Ordinal) && formatError.Message.Contains("JSON", StringComparison.Ordinal),
            "Unsupported content did not report the supported package format in Chinese");
    });
}
Test("script format accepts ZIP content named JSON with attached resources and executes it", async () =>
{
    var path = Path.Combine(Temp(), "valid.json");
    var package = Package(new ScriptStep { Action = "click", Value = "20,20" });
    ScriptPackageService.Save(path, package);
    using (var archive = ZipFile.Open(path, ZipArchiveMode.Update))
    {
        using var resource = archive.CreateEntry("assets/resource.bin").Open();
        resource.Write([0, 255, 11, 28]);
    }
    var imported = ScriptPackageService.Load(path);
    Check(imported.Manifest.Id == package.Manifest.Id && imported.Manifest.Version == package.Manifest.Version &&
        imported.Steps.Single().Value == "20,20", "ZIP content or metadata changed because of its suffix/resources");
    await using var backend = new DemoDeviceBackend();
    await using var scripts = new ScriptScheduler(backend);
    Check((await backend.DumpAsync("DEMO-001")).ChildNodeList[1].Text == "Tap 0", "Import itself executed a step");
    await scripts.RunAsync("DEMO-001", imported).WaitAsync(TimeSpan.FromSeconds(2));
    Check((await backend.DumpAsync("DEMO-001")).ChildNodeList[1].Text == "Tap 1", "Valid imported ZIP did not execute its actual step");
});
Test("script format does not relabel missing or locked file I/O errors", async () =>
{
    var path = Path.Combine(Temp(), "missing.zip");
    await Throws<FileNotFoundException>(() => Task.FromResult(ScriptPackageService.Load(path)));
    File.WriteAllBytes(path, [0, 1, 2, 3]);
    using var locked = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
    await Throws<IOException>(() => Task.FromResult(ScriptPackageService.Load(path)));
});
Test("script format preserves inner JSON errors", () =>
{
    var path = Path.Combine(Temp(), "bad-json.zip");
    using (var archive = ZipFile.Open(path, ZipArchiveMode.Create))
    {
        using (var manifest = new StreamWriter(archive.CreateEntry("manifest.json").Open()))
            manifest.Write(JsonSerializer.Serialize(Package().Manifest, JsonDefaults.Options));
        using var script = new StreamWriter(archive.CreateEntry("script.json").Open());
        script.Write("{invalid JSON");
    }
    try { ScriptPackageService.Load(path); throw new Exception("Invalid inner JSON was accepted"); }
    catch (DeviceException error)
    {
        Check(error.Status == 400 && error.Code == 1007 && error.Message.StartsWith("Invalid JSON in script.json:", StringComparison.Ordinal),
            "Inner JSON error was relabeled as an archive format error");
    }
    return Task.CompletedTask;
});
Test("script format preserves action validation errors", () =>
{
    var path = Path.Combine(Temp(), "bad-action.zip");
    using (var archive = ZipFile.Open(path, ZipArchiveMode.Create))
    {
        using (var manifest = new StreamWriter(archive.CreateEntry("manifest.json").Open()))
            manifest.Write(JsonSerializer.Serialize(Package().Manifest, JsonDefaults.Options));
        using var script = new StreamWriter(archive.CreateEntry("script.json").Open());
        script.Write(JsonSerializer.Serialize(Package(new ScriptStep { Action = "eval", Value = "not executable" }), JsonDefaults.Options));
    }
    try { ScriptPackageService.Load(path); throw new Exception("Invalid action was accepted"); }
    catch (DeviceException error)
    {
        Check(error.Status == 400 && error.Code == 1007 && error.Message.StartsWith("Unknown action at step 0:", StringComparison.Ordinal),
            "Action validation was relabeled as an archive format error");
    }
    return Task.CompletedTask;
});
Test("script format retains the archive size gate before format parsing", () =>
{
    var path = Path.Combine(Temp(), "oversized.zip");
    using (var file = File.Create(path)) file.SetLength(ScriptPackageService.MaximumArchiveBytes + 1L);
    try { ScriptPackageService.Load(path); throw new Exception("Oversized archive was accepted"); }
    catch (DeviceException error)
    {
        Check(error.Status == 400 && error.Code == 1007 && error.Message.StartsWith("Script archive exceeds ", StringComparison.Ordinal),
            "Archive size limit changed or was relabeled as an archive format error");
    }
    return Task.CompletedTask;
});
Test("configuration recovery and three-way merge preserve other writes", () =>
{
    var path = Path.Combine(Temp(), "AppConfig.json"); var first = new SettingsStore(path); first.Load(); first.Save(new() { BasicApiPort = 18880 });
    var second = new SettingsStore(path); var baseline = second.Load();
    first.Save(new() { BasicApiPort = 18880, Language = "en-US", CommandTimeoutSeconds = 40 });
    var merged = second.Save(baseline with { WallColumns = 8 });
    Check(merged.Language == "zh-CN" && merged.CommandTimeoutSeconds == 40 && merged.WallColumns == 8, "Concurrent configuration write lost or English language was retained");
    File.WriteAllText(path, "{broken");
    var recovery = new SettingsStore(path); Check(recovery.Load().Language == "zh-CN" && recovery.Warnings.Count == 2
        && recovery.Warnings.Any(x => x.Contains("设备授权与雷电绑定已清空", StringComparison.Ordinal)), "Backup recovery failed or authority warning missing");
    Check(!File.ReadAllBytes(path + ".bak").Take(3).SequenceEqual(new byte[] { 239, 187, 191 }), "Settings added UTF-8 BOM");
    return Task.CompletedTask;
});
Test("configuration can clear a previously configured nullable field", () =>
{
    var path = Path.Combine(Temp(), "AppConfig.json"); var store = new SettingsStore(path); store.Load();
    var previous = store.Save(new() { AgentBaseUrl = "http://127.0.0.1:9008/", AgentSerial = "test" });
    store.Save(previous with { AgentBaseUrl = null, AgentSerial = null });
    Check(new SettingsStore(path).Load().AgentBaseUrl is null, "Clearing AgentBaseUrl retained the old endpoint");
    return Task.CompletedTask;
});
Test("unconfigured ADB leaves runtime available for Settings", async () =>
{
    await using var runtime = await MatrixRuntime.CreateAsync(new(), Temp());
    Check(runtime.DiscoveryError is not null && runtime.Devices.Snapshot.Count == 0, "Missing ADB path did not expose a setup state");
    await Throws<ArgumentException>(async () => { await MatrixRuntime.CreateAsync(new() { AgentBaseUrl = "http://127.0.0.1:9008" }, Temp()); });
});
Test("occupied startup gateway leaves the Settings runtime available", async () =>
{
    using var occupied = new TcpListener(IPAddress.Loopback, 0); occupied.Start(); var port = ((IPEndPoint)occupied.LocalEndpoint).Port;
    await using var runtime = await MatrixRuntime.CreateAsync(new() { AutoStartApi = true, BasicApiPort = port, AutomatorApiPort = Port() }, Temp(), true);
    Check(runtime.ApiStartupError is not null && !runtime.Api.BasicRunning && runtime.Devices.Snapshot.Count == 2, "API startup failure killed the device runtime or claimed running");
});
Test("corrupt database startup releases its file handle", async () =>
{
    var directory = Temp(); var path = Path.Combine(directory, "db.sqlite3"); await File.WriteAllTextAsync(path, "This is not SQLite");
    await Throws<Microsoft.Data.Sqlite.SqliteException>(async () => { await MatrixRuntime.CreateAsync(new(), directory, true); });
    using var writable = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
    Check(writable.Length > 0, "Corrupt database was overwritten");
});
Test("target dimension change invalidates catalog generation", async () =>
{
    await using var backend = new DemoDeviceBackend(); var catalog = new DeviceCatalog(backend); await catalog.RefreshAsync();
    var before = catalog.GetRequired("DEMO-002"); backend.SetDimensions("DEMO-002", 720, 412); await catalog.RefreshAsync();
    Check(catalog.GetRequired("DEMO-002").Generation > before.Generation, "Rotation retained the input generation");
});
Test("SQLite metadata and entity parameters survive restart", () =>
{
    var path = Path.Combine(Temp(), "db.sqlite3");
    using (var repo = new CatalogRepository(path)) { repo.UpsertMetadata("a' OR 1=1", "Phone", "Group A"); repo.SetRemark("a' OR 1=1", "note"); repo.Put("ScriptDeviceConfiguration", "p/a", new { value = 7 }); }
    using var reopened = new CatalogRepository(path);
    Check(reopened.GetMetadata("a' OR 1=1").Remark == "note" && reopened.GetAllMetadata().Count == 1, "Metadata persistence failed");
    Check(reopened.GetAll<JsonElement>("ScriptDeviceConfiguration").Single().GetProperty("value").GetInt32() == 7, "Entity payload lost");
    return Throws<ArgumentException>(() => Task.FromResult(reopened.GetAll<JsonElement>("User]; DROP TABLE User;--")));
});
Test("phone client maps command/package fields and rejects bad envelopes", async () =>
{
    var handler = new CaptureHandler(); await using var client = new PhoneAgentClient("http://127.0.0.1:9999/", handler);
    await client.ShellAsync("echo 测试");
    Check(handler.Path == "/device/cmd" && handler.Body!.Value.GetProperty("cmd").GetString() == "echo 测试", "Phone protocol command mapped to gateway DTO");
    await client.StartAppAsync("com.matrix.demo"); Check(handler.Body!.Value.TryGetProperty("package", out _), "Phone start lacks package field");
    handler.Response = "not json"; await Throws<DeviceException>(() => client.ServiceInfoAsync());
});
Test("phone client normalizes malformed envelope errors", async () =>
{
    foreach (var json in new[] { "[]", "{\"code\":\"wrong\"}", "{\"code\":7}", "null" })
    {
        var handler = new CaptureHandler { Response = json }; await using var client = new PhoneAgentClient("http://127.0.0.1:9999/", handler);
        await Throws<DeviceException>(() => client.ServiceInfoAsync());
    }
});
Test("all 27 Automator routes have an explicit outcome", async () =>
{
    await using var runtime = await MatrixRuntime.CreateAsync(new(), Temp(), true);
    var samples = new Dictionary<string, object>
    {
        ["device/dump"] = new { }, ["device/screenshot"] = new { }, ["device/shell"] = new { command = "id" }, ["device/execute"] = new { command = "id" },
        ["device/set-prop"] = new { prop = "matrix.test", value = "verified" }, ["device/set-timezone"] = new { timezone = "Asia/Shanghai" }, ["device/set-sim-info"] = new { },
        ["app/list"] = new { }, ["app/list-packages"] = new { }, ["app/start"] = new { packageName = "com.matrix.demo" }, ["app/stop"] = new { packageName = "com.matrix.demo" }, ["app/clear"] = new { packageName = "com.matrix.demo" },
        ["input/tap"] = new { x = 10, y = 20 }, ["input/swipe"] = new { x1 = 10, y1 = 20, x2 = 30, y2 = 40 }, ["input/keyevent"] = new { code = 3 },
        ["service/info"] = new { }, ["service/stop"] = new { }, ["function/sleep"] = new { seconds = 0 }, ["function/sleep-ms"] = new { milliseconds = 0 },
        ["element/find"] = new { selector = new { resourceId = "com.matrix.demo:id/input" } }, ["element/find-all"] = new { selector = new { } },
        ["element/wait-exists"] = new { selector = new { resourceId = "com.matrix.demo:id/input" }, timeoutMs = 0 },
        ["element/click-if-exists"] = new { selector = new { resourceId = "com.matrix.demo:id/input" }, timeoutMs = 0 },
        ["element/click-text"] = new { text = "Settings", timeoutMs = 0 }, ["element/set-text"] = new { selector = new { resourceId = "com.matrix.demo:id/input" }, text = "updated" },
        ["element/wait-any"] = new { selectors = new[] { new { resourceId = "com.matrix.demo:id/input" } }, timeoutMs = 0 },
        ["element/click-node"] = new { node = new AndroidNode { Bounds = new(10, 10, 30, 30) } }
    };
    foreach (var route in AutomationService.Routes)
    {
        var payload = JsonSerializer.SerializeToNode(samples[route], JsonDefaults.Options)!.AsObject();
        payload["serial"] = "DEMO-001";
        try { await runtime.Automation.ExecuteAsync(route, JsonSerializer.SerializeToElement(payload)); }
        catch (DeviceException e) when ((route is "device/set-sim-info" or "service/stop") && e.Status == 501) { }
    }
});
Test("HTTP gateways keep distinct envelopes and invalid requests visible", async () =>
{
    var basicPort = Port(); var autoPort = Port();
    await using var runtime = await MatrixRuntime.CreateAsync(new() { BasicApiPort = basicPort, AutomatorApiPort = autoPort }, Temp(), true);
    await runtime.StartApisAsync(); var devicePort = await runtime.Api.StartDeviceAsync("DEMO-001");
    runtime.Settings = runtime.Settings with { BasicApiPort = basicPort == 65535 ? 65534 : basicPort + 1 };
    Check(runtime.Api.RunningBasicPort == basicPort && runtime.Api.RunningAutomatorPort == autoPort, "Merged settings changed the owned running endpoint");
    Check(await runtime.Api.StartDeviceAsync("DEMO-001") == devicePort, "Repeated start changed serial endpoint");
    using var http = new HttpClient();
    using var health = await http.GetAsync($"http://localhost:{basicPort}/health/"); var basicBody = await health.Content.ReadFromJsonAsync<JsonElement>();
    Check(health.IsSuccessStatusCode && basicBody.GetProperty("ok").GetBoolean() && !basicBody.TryGetProperty("code", out _), "Basic gateway envelope changed");
    using var tap = await http.PostAsJsonAsync($"http://localhost:{devicePort}/tap", new { X = 20, Y = 20 }); Check(tap.IsSuccessStatusCode, "Basic tap failed");
    var beforeBusy = await runtime.Backend.ScreenshotAsync("DEMO-001"); runtime.Api.UiWriteBusy = true;
    using var busyTap = await http.PostAsJsonAsync($"http://localhost:{devicePort}/tap", new { X = 30, Y = 40 }); Check((int)busyTap.StatusCode == 409, "UI busy did not reject API writes");
    using var busyDump = await http.PostAsJsonAsync($"http://localhost:{autoPort}/device/dump", new { serial = "DEMO-001" }); Check(busyDump.IsSuccessStatusCode, "UI busy blocked read-only node queries");
    var afterBusy = await runtime.Backend.ScreenshotAsync("DEMO-001");
    Check(beforeBusy.SequenceEqual(afterBusy), "Busy rejection still mutated the guest"); runtime.Api.UiWriteBusy = false;
    var activeScript = runtime.Scripts.RunAsync("DEMO-001", Package(new ScriptStep { Action = "wait", DurationMs = 10000 }));
    using var scriptTap = await http.PostAsJsonAsync($"http://localhost:{autoPort}/input/tap", new { serial = "DEMO-001", x = 20, y = 20 }); Check((int)scriptTap.StatusCode == 409, "Running script did not reject external writes");
    runtime.Scripts.Stop("DEMO-001"); await activeScript;
    using var malformed = await http.PostAsync($"http://localhost:{devicePort}/tap", new StringContent("{\"X\":\"bad\",\"Y\":20}", Encoding.UTF8, "application/json")); Check((int)malformed.StatusCode == 400, "Bad number became server failure");
    using var autoHealth = await http.GetAsync($"http://localhost:{autoPort}/api/health"); var envelope = await autoHealth.Content.ReadFromJsonAsync<JsonElement>();
    Check(envelope.GetProperty("code").GetInt32() == 0 && !envelope.TryGetProperty("ok", out _), "Automator envelope mixed with basic");
    using var unknown = await http.PostAsJsonAsync($"http://localhost:{autoPort}/unknown", new { serial = "DEMO-001" });
    Check((int)unknown.StatusCode == 404 && (await unknown.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetInt32() == 1006, "Unknown route mapping wrong");
    using var wrongMethod = await http.PutAsJsonAsync($"http://localhost:{autoPort}/input/tap", new { serial = "DEMO-001" }); Check((int)wrongMethod.StatusCode == 405, "Wrong method accepted");
    using var missingSerial = await http.PostAsJsonAsync($"http://localhost:{autoPort}/input/tap", new { x = 1, y = 1 }); Check((int)missingSerial.StatusCode == 400, "Missing serial accepted");
    using var badJson = await http.PostAsync($"http://localhost:{autoPort}/input/tap", new StringContent("{bad", Encoding.UTF8, "application/json")); Check((int)badJson.StatusCode == 400, "Invalid JSON reported success");
    using var sim = await http.PostAsJsonAsync($"http://localhost:{autoPort}/device/set-sim-info", new { serial = "DEMO-001" }); Check((int)sim.StatusCode == 501, "Unknown ROM effect reported success");
    ((DemoDeviceBackend)runtime.Backend).SetOnline("DEMO-001", false); await runtime.RefreshAsync();
    using var offline = await http.GetAsync($"http://localhost:{devicePort}/health"); Check((int)offline.StatusCode == 410, "Offline device reported API ready");
});
Test("Automator crop and ordered wait-any return real backend results", async () =>
{
    await using var runtime = await MatrixRuntime.CreateAsync(new(), Temp(), true);
    var image = Body((await runtime.Automation.ExecuteAsync("device/screenshot", Body(new { serial = "DEMO-001", x = 10, y = 20, width = 30, height = 40 })))!);
    Check(PngImage.ReadDimensions(Convert.FromBase64String(image.GetProperty("base64").GetString()!)) == (30, 40), "API crop ignored request bounds");
    var match = Body((await runtime.Automation.ExecuteAsync("element/wait-any", Body(new { serial = "DEMO-001", selectors = new[] { new NodeSelector { Text = "Settings" }, new NodeSelector { Text = "Tap 0" } }, timeoutMs = 0 })))!);
    Check(match.GetProperty("index").GetInt32() == 0, "WaitAny ignored selector input priority");
    Check(AutomationService.Routes.Count == 27, "Route count differs from API.md");
});
Test("file transfers preserve bytes and device isolation", async () =>
{
    await using var backend = new DemoDeviceBackend(); var folder = Temp(); var original = Path.Combine(folder, "source.bin"); var copied = Path.Combine(folder, "received.bin");
    var payload = Enumerable.Range(0, 256).Select(x => (byte)x).ToArray(); await File.WriteAllBytesAsync(original, payload);
    await backend.PushAsync("DEMO-001", original, "/sdcard/MatrixExchange/test.bin"); await backend.PullAsync("DEMO-001", "/sdcard/MatrixExchange/test.bin", copied);
    Check((await File.ReadAllBytesAsync(copied)).SequenceEqual(payload), "Transfer changed bytes");
    await Throws<DeviceException>(() => backend.PullAsync("DEMO-002", "/sdcard/MatrixExchange/test.bin", copied));
});
Test("occupied device ports are skipped and released ports cool down", async () =>
{
    using var occupied = new TcpListener(IPAddress.Loopback, 8001); try { occupied.Start(); } catch (SocketException) { }
    await using var runtime = await MatrixRuntime.CreateAsync(new(), Temp(), true);
    var first = await runtime.Api.StartDeviceAsync("DEMO-001"); Check(first != 8001, "Occupied port was assigned");
    await runtime.Api.StopDeviceAsync("DEMO-001"); var next = await runtime.Api.StartDeviceAsync("DEMO-002"); Check(next != first, "Cooling endpoint rebound to another serial");
});
Test("ordinary logs flush at shutdown", async () =>
{
    var directory = Temp(); var log = new RollingLog(directory); log.Write("test", "verified message", "DEMO-001"); await log.DisposeAsync();
    Check(Directory.GetFiles(directory, "*.log").Any(path => File.ReadAllText(path).Contains("verified message")), "Queued log did not flush");
});
Test("bounded stream accepts its limit and rejects one extra byte", async () =>
{
    using var exact = new MemoryStream(new byte[8]); Check((await BoundedStreamRead.ReadAsync(exact, 8)).Length == 8, "Exact limit failed");
    using var large = new MemoryStream(new byte[9]); await Throws<DeviceException>(() => BoundedStreamRead.ReadAsync(large, 8));
});
Test("phone screenshot and dump reject malformed data shapes", async () =>
{
    foreach (var json in new[] { "{\"code\":0,\"data\":null}", "{\"code\":0,\"data\":[]}", "{\"code\":0,\"data\":{\"base64\":7}}" })
    {
        var handler = new CaptureHandler { Response = json }; await using var client = new PhoneAgentClient("http://127.0.0.1:9999/", handler);
        await Throws<DeviceException>(() => client.ScreenshotAsync());
    }
    var dumpHandler = new CaptureHandler { Response = "{\"code\":0,\"data\":[]}" };
    await using var dumpClient = new PhoneAgentClient("http://127.0.0.1:9999/", dumpHandler); await Throws<DeviceException>(() => dumpClient.DumpAsync());
});
Test("XML dump rejects external entities and excessive depth", async () =>
{
    await Throws<System.Xml.XmlException>(() => Task.FromResult(AdbDeviceBackend.ParseUiXml("<!DOCTYPE hierarchy [<!ENTITY e SYSTEM 'file:///not-read'>]><hierarchy>&e;</hierarchy>")));
    var deep = "<hierarchy>" + string.Concat(Enumerable.Repeat("<node bounds='[0,0][1,1]'>", 300)) + string.Concat(Enumerable.Repeat("</node>", 300)) + "</hierarchy>";
    await Throws<InvalidDataException>(() => Task.FromResult(AdbDeviceBackend.ParseUiXml(deep)));
});
Test("scan rejects invalid or oversized ranges before touching the network", async () =>
{
    await using var backend = new AdbDeviceBackend(new()); var scanner = new NetworkDeviceScanner(backend);
    await Throws<DeviceException>(() => scanner.ScanAsync("10.0.0.1", "10.0.9.255", 5555));
    await Throws<DeviceException>(() => scanner.ScanAsync("::1", "::1", 5555));
});
Test("unsupported device input uses the documented error code", async () =>
{
    await using var backend = new AdbDeviceBackend(new() { AdbAllowedSerials = ["test"] });
    foreach (var operation in new Func<Task>[] { () => backend.TextAsync("test", "中文"), () => backend.SetNodeTextAsync("test", new(), "中文") })
    {
        try { await operation(); }
        catch (DeviceException e) { Check(e.Status == 501 && e.Code == 2001, "Unsupported input returned an undocumented code"); continue; }
        throw new Exception("Unconfigured Unicode input was accepted");
    }
});
Test("crop outside the device returns a validation error", async () =>
{
    await using var runtime = await MatrixRuntime.CreateAsync(new(), Temp(), true);
    try { await runtime.Automation.ExecuteAsync("device/screenshot", Body(new { serial = "DEMO-001", x = 99999, width = 20, height = 20 })); }
    catch (DeviceException e) { Check(e.Status == 400, "Invalid crop did not return400"); return; }
    throw new Exception("Invalid crop was accepted");
});
Test("180 degree rotation invalidates input even with unchanged dimensions", async () =>
{
    await using var backend = new DemoDeviceBackend(); var catalog = new DeviceCatalog(backend); await catalog.RefreshAsync();
    var before = catalog.GetRequired("DEMO-001"); backend.SetRotation("DEMO-001", 180); await catalog.RefreshAsync();
    var after = catalog.GetRequired("DEMO-001");
    Check(after.Width == before.Width && after.Height == before.Height && after.Generation > before.Generation, "Same-size rotation retained the old input context");
    var unknown = AdbDeviceBackend.ParseDisplayInfo("Physical size: 360x640", "unrecognized dump"); Check(unknown.Rotation is null, "Unknown orientation silently became zero");
});
Test("queued keys and text cannot cross a source reconnect", async () =>
{
    await using var backend = new KeyGateBackend(); var catalog = new DeviceCatalog(backend); await catalog.RefreshAsync();
    await using var input = new InputCoordinator(catalog, backend); await input.HandoffAsync("DEMO-001");
    var snapshot = new InputSnapshot { SourceSerial = "DEMO-001", TargetSerials = ["DEMO-002"], IncludeSource = false, Kind = InputKind.Key, KeyCode = 4 };
    var first = input.SendAsync(snapshot); await backend.Entered.Task.WaitAsync(TimeSpan.FromSeconds(2));
    var key = input.SendAsync(snapshot with { KeyCode = 3 }); var text = input.SendAsync(snapshot with { Kind = InputKind.Text, Text = "stale" });
    backend.Devices.SetOnline("DEMO-001", false); await catalog.RefreshAsync(); backend.Devices.SetOnline("DEMO-001", true); await catalog.RefreshAsync();
    backend.Release.SetResult(); await first;
    await Throws<DeviceException>(() => key); await Throws<DeviceException>(() => text);
    Check(backend.Actions.Count == 1, "Old source context delivered extra queued actions");
});

Test("LDPlayer parser supports current and legacy Chinese titles", () =>
{
    var latest = LdPlayerService.ParseList2("0,雷电模拟器,4986608,1967810,1,49596,75704,1600,900,240\r\n");
    Check(latest.Count == 1 && latest[0].Name == "雷电模拟器" && latest[0].Width == 1600 && latest[0].VBoxProcessId == 75704, "10-column list changed");
    var legacy = LdPlayerService.ParseList2("2,测试,分身,0,0,0,0,0");
    Check(legacy.Single().Name == "测试,分身" && legacy[0].Width is null, "Legacy comma title lost");
    Check(LdPlayerService.ParseList2("99999,电脑桌面,0,0,0,0,0").Count == 0, "Desktop sentinel became controllable");
    return Task.CompletedTask;
});
Test("observed LDPlayer transition flag two never becomes Android ready", () =>
{
    var row = LdPlayerService.ParseList2("2,MatrixAcceptance,30279482,17367550,2,35824,49164,1280,720,240").Single();
    Check(!row.AndroidStarted && row.ProcessId == 35824 && row.VBoxProcessId == 49164 && row.Width == 1280, "Transition identity was lost or reported ready");
    return Task.CompletedTask;
});
Test("LDPlayer rejects malformed and duplicate instance rows", async () =>
{
    foreach (var malformed in new[] { "0,模拟器,bad,0,1,7,8", "-1,模拟器,0,0,0,0,0", "0,模拟器,0,0,3,0,0", "0,模拟器,0,0,0,0,0\n0,重复,0,0,0,0,0", "0,模拟器,100,200,1,7,8,1,900,240", "0,模拟器,100,200,1,7,8,0,900,240" })
        await Throws<DeviceException>(() => Task.FromResult(LdPlayerService.ParseList2(malformed)));
});
Test("LDPlayer new service epochs reject old clients and remain JSON safe", async () =>
{
    await using var first = new LdPlayerService(new ControlledLdRunner()); await first.RefreshAsync();
    var previous = first.Instances.Single().Generation;
    await using var second = new LdPlayerService(new ControlledLdRunner()); await second.RefreshAsync();
    var current = second.Instances.Single().Generation;
    Check(current != previous && current <= 9_007_199_254_740_991 && (long)(double)current == current, "Epoch reused or cannot roundtrip JavaScript number");
    await Throws<DeviceException>(() => second.ExecuteAsync(0, "input", new JsonObject { ["expectedGeneration"] = previous, ["text"] = "过期服务请求" }));
});
Test("LDPlayer generations are stable but invalidate restarted instances", async () =>
{
    var runner = new ControlledLdRunner(); await using var service = new LdPlayerService(runner);
    await service.RefreshAsync(); var first = service.Instances.Single().Generation;
    await service.RefreshAsync(); Check(service.Instances.Single().Generation == first, "Polling invalidated the UI selection");
    runner.List = "0,雷电测试,100,200,1,8,10,1600,900,240";
    await service.RefreshAsync(); Check(service.Instances.Single().Generation > first, "Restart retained stale generation");
    await Throws<DeviceException>(() => service.ExecuteAsync(0, "input", new JsonObject { ["expectedGeneration"] = first, ["text"] = "中文" }));
    Check(runner.Commands.Count == 0, "Stale command reached console");
});
Test("LDPlayer guards confirmations whitelist and command fields", async () =>
{
    var runner = new ControlledLdRunner(); await using var service = new LdPlayerService(runner); await service.RefreshAsync();
    var generation = service.Instances.Single().Generation;
    foreach (var operation in new[] { "quit", "reboot", "quitall", "remove", "restore", "modify" })
        await Throws<DeviceException>(() => service.ExecuteAsync(0, operation, new JsonObject { ["expectedGeneration"] = generation }));
    await Throws<DeviceException>(() => service.ExecuteAsync(0, "keyevent", new JsonObject { ["expectedGeneration"] = generation, ["key"] = "menu" }));
    await Throws<DeviceException>(() => service.ExecuteAsync(0, "runapp", new JsonObject { ["expectedGeneration"] = generation, ["packageName"] = "--index 7" }));
    await Throws<DeviceException>(() => service.ExecuteAsync(0, "input", new JsonObject { ["text"] = "中文" }));
    Check(runner.Commands.Count == 0, "Invalid request performed a command");
    var result = await service.ExecuteAsync(0, "input", new JsonObject { ["expectedGeneration"] = generation, ["text"] = "中文 空格\"引号" });
    Check(result.Accepted && !result.Verified, "Input claimed verified device effect");
    Check(runner.Commands.Single().SequenceEqual(new[] { "action", "--index", "0", "--key", "call.input", "--value", "中文 空格\"引号" }), "Chinese input arguments changed");
});
Test("LDPlayer removed indices do not reuse old generations", async () =>
{
    var runner = new ControlledLdRunner(); await using var service = new LdPlayerService(runner); await service.RefreshAsync();
    var first = service.Instances.Single().Generation; var original = runner.List;
    runner.List = ""; await service.RefreshAsync(); runner.List = original; await service.RefreshAsync();
    Check(service.Instances.Single().Generation > first, "Recreated instance reused generation");
    await Throws<DeviceException>(() => service.ExecuteAsync(0, "keyevent", new JsonObject { ["expectedGeneration"] = first, ["key"] = "home" }));
    Check(runner.Commands.Count == 0, "Removed instance accepted stale action");
});
Test("LDPlayer queued input rejects restart and allows another instance", async () =>
{
    var runner = new GatedLdRunner(); await using var service = new LdPlayerService(runner); await service.RefreshAsync();
    var generation = service.Instances.First(x => x.Index == 0).Generation;
    var first = service.ExecuteAsync(0, "input", new JsonObject { ["expectedGeneration"] = generation, ["text"] = "第一条" });
    await runner.Entered.Task.WaitAsync(TimeSpan.FromSeconds(2));
    var next = service.ExecuteAsync(0, "input", new JsonObject { ["expectedGeneration"] = generation, ["text"] = "过期条目" });
    await service.ExecuteAsync(1, "keyevent", new JsonObject { ["expectedGeneration"] = service.Instances.First(x => x.Index == 1).Generation, ["key"] = "home" }).WaitAsync(TimeSpan.FromSeconds(2));
    Check(!next.IsCompleted, "Same-instance queue overtook its predecessor");
    runner.Restarted = true; await service.RefreshAsync(); runner.Release.SetResult(); await first;
    await Throws<DeviceException>(() => next);
    Check(runner.Writes.Count == 2, "Stale queued input reached restarted instance");
});
Test("LDPlayer real runner preserves argument boundaries and GBK output", async () =>
{
    var executable = Environment.ProcessPath!; var assembly = typeof(ControlledLdRunner).Assembly.Location;
    var prefix = Path.GetFileNameWithoutExtension(executable).Equals("dotnet", StringComparison.OrdinalIgnoreCase) ? new[] { assembly, "--ldconsole-test-child" } : new[] { "--ldconsole-test-child" };
    var runner = new LdConsoleRunner(executable, 5);
    var values = new[] { "中文 空格", "a\"b", "C:\\测试 路径\\x.apk", "--index" };
    var output = await runner.RunAsync(prefix.Concat(new[] { "echo" }).Concat(values).ToArray(), CancellationToken.None);
    Check(JsonSerializer.Deserialize<string[]>(output)!.SequenceEqual(values), "Process arguments lost boundaries");
    var gbk = await runner.RunAsync(prefix.Concat(new[] { "gbk" }).ToArray(), CancellationToken.None);
    Check(LdPlayerService.ParseList2(gbk).Single().Name == "雷电测试", "GBK Chinese title corrupted");
    await Throws<DeviceException>(() => runner.RunAsync(prefix.Concat(new[] { "fail" }).ToArray(), CancellationToken.None));
});
Test("LDPlayer runner cancels times out and limits its own output", async () =>
{
    var executable = Environment.ProcessPath!; var assembly = typeof(ControlledLdRunner).Assembly.Location;
    var prefix = Path.GetFileNameWithoutExtension(executable).Equals("dotnet", StringComparison.OrdinalIgnoreCase) ? new[] { assembly, "--ldconsole-test-child" } : new[] { "--ldconsole-test-child" };
    var runner = new LdConsoleRunner(executable, 1);
    try { await runner.RunAsync(prefix.Concat(new[] { "sleep" }).ToArray(), CancellationToken.None); throw new Exception("Timeout was accepted"); }
    catch (DeviceException e) { Check(e.Status == 504, "Timeout did not preserve its API status"); }
    using var cancellation = new CancellationTokenSource(100);
    await Throws<OperationCanceledException>(() => runner.RunAsync(prefix.Concat(new[] { "sleep" }).ToArray(), cancellation.Token));
    foreach (var stream in new[] { "flood", "stderr-flood" })
    {
        try { await new LdConsoleRunner(executable, 5).RunAsync(prefix.Concat(new[] { stream }).ToArray(), CancellationToken.None); throw new Exception("Output limit accepted"); }
        catch (DeviceException e) { Check(e.Status == 413, "Output size error did not preserve 413"); }
    }
});
Test("LDPlayer confirmed demo lifecycle reports observed states and fresh tokens", async () =>
{
    await using var service = new LdPlayerService(new DemoLdConsoleRunner(), true); await service.RefreshAsync();
    var before = service.Instances.Single(x => x.Index == 1);
    var launch = await service.ExecuteAsync(1, "launch", new JsonObject { ["expectedGeneration"] = before.Generation });
    Check(launch.Accepted && launch.Verified && launch.Instance?.AndroidStarted == true && launch.Instance.Generation > before.Generation, "Launch state not observed");
    var stop = await service.ExecuteAsync(1, "quit", new JsonObject { ["expectedGeneration"] = launch.Instance!.Generation, ["confirmed"] = true });
    Check(stop.Accepted && stop.Verified && stop.Instance?.AndroidStarted == false && stop.Instance.Generation > launch.Instance.Generation, "Confirmed quit not observed");
    await Throws<DeviceException>(() => service.ExecuteAsync(1, "input", new JsonObject { ["expectedGeneration"] = stop.Instance!.Generation, ["text"] = "停机拒绝" }));
});
Test("LDPlayer timeout never kills descendants of its CLI process", async () =>
{
    var executable = Environment.ProcessPath!; var assembly = typeof(ControlledLdRunner).Assembly.Location;
    var prefix = Path.GetFileNameWithoutExtension(executable).Equals("dotnet", StringComparison.OrdinalIgnoreCase) ? new[] { assembly, "--ldconsole-test-child" } : new[] { "--ldconsole-test-child" };
    var pidFile = Path.Combine(Temp(), "test-child.pid"); Process? child = null;
    try
    {
        try { await new LdConsoleRunner(executable, 2).RunAsync(prefix.Concat(new[] { "spawn", pidFile }).ToArray(), CancellationToken.None); throw new Exception("CLI did not time out"); }
        catch (DeviceException e) { Check(e.Status == 504, "Wrong timeout status"); }
        Check(File.Exists(pidFile), "Test descendant was not started");
        child = Process.GetProcessById(int.Parse(await File.ReadAllTextAsync(pidFile)));
        Check(!child.HasExited, "Timeout killed a descendant which may represent the emulator");
    }
    finally
    {
        if (child is null && File.Exists(pidFile)) { try { child = Process.GetProcessById(int.Parse(await File.ReadAllTextAsync(pidFile))); } catch (ArgumentException) { } }
        if (child is not null) { if (!child.HasExited) { child.Kill(entireProcessTree: false); await child.WaitForExitAsync(); } child.Dispose(); }
    }
});
Test("LDPlayer HTTP routes separate support from verified effects", async () =>
{
    var port = Port(); await using var runtime = await MatrixRuntime.CreateAsync(new() { BasicApiPort = port }, Temp(), true);
    await runtime.Api.StartBasicAsync(port); using var http = new HttpClient { BaseAddress = new Uri($"http://localhost:{port}/") };
    using var instances = await http.GetAsync("ldplayer/instances"); var body = await instances.Content.ReadFromJsonAsync<JsonElement>();
    Check(body.GetProperty("ok").GetBoolean() && body.GetProperty("data").GetArrayLength() == 2, "Demo LD instances unavailable");
    var selected = body.GetProperty("data")[0]; var generation = selected.GetProperty("generation").GetInt64(); var index = selected.GetProperty("index").GetInt32();
    using var capabilities = await http.GetAsync("ldplayer/capabilities"); Check(capabilities.IsSuccessStatusCode, "Capabilities unavailable");
    using var missingConfirm = await http.PostAsJsonAsync($"ldplayer/instances/{index}/quit", new { expectedGeneration = generation }); Check(!missingConfirm.IsSuccessStatusCode, "Quit lacked confirmation");
    using var text = await http.PostAsJsonAsync($"ldplayer/instances/{index}/input", new { expectedGeneration = generation, text = "测试" });
    Check(text.IsSuccessStatusCode && !(await text.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("data").GetProperty("verified").GetBoolean(), "Text API claimed guest effect");
    using var malformed = await http.PostAsJsonAsync($"ldplayer/instances/{index}/input", new { expectedGeneration = "bad", text = "测试" }); Check((int)malformed.StatusCode == 400, "Malformed generation not rejected");
    using var wrongContent = await http.PostAsync($"ldplayer/instances/{index}/input", new StringContent("{}")); Check((int)wrongContent.StatusCode == 415, "Simple cross-site content accepted");
    using var crossSite = new HttpRequestMessage(HttpMethod.Post, $"ldplayer/instances/{index}/input") { Content = JsonContent.Create(new { expectedGeneration = generation, text = "测试" }) };
    crossSite.Headers.Add("Origin", "http://example.invalid"); using var rejected = await http.SendAsync(crossSite); Check((int)rejected.StatusCode == 403, "Cross-site controller accepted");
    using var blocked = await http.PostAsJsonAsync($"ldplayer/instances/{index}/quitall", new { expectedGeneration = generation, confirmed = true }); Check(!blocked.IsSuccessStatusCode, "Global quit exposed");
});
Test("invalid LDPlayer configuration leaves Chinese settings available", async () =>
{
    await using var runtime = await MatrixRuntime.CreateAsync(new() { LdConsolePath = "missing-ldconsole.exe", Language = "en-US" }, Temp());
    Check(runtime.LdPlayer is null && runtime.LdPlayerError is not null, "Bad CLI path destroyed settings runtime");
    Check(SettingsStore.Normalize(runtime.Settings).Language == "zh-CN", "English persisted");
});
Test("LDPlayer API script write guard isolates uniquely bound instances and keeps unknown mappings conservative", async () =>
{
    var basicPort = Port(); var autoPort = Port();
    await using var runtime = await MatrixRuntime.CreateAsync(new() { BasicApiPort = basicPort, AutomatorApiPort = autoPort,
        AdbAllowedSerials = ["DEMO-001", "DEMO-002"], LdSerialBindings = new() { [0] = "DEMO-001", [1] = "DEMO-002" } }, Temp(), true);
    await runtime.StartApisAsync();
    var initial = await runtime.LdPlayer!.RefreshAsync();
    await runtime.LdPlayer.ExecuteAsync(1, "launch", new JsonObject { ["expectedGeneration"] = initial.Single(x => x.Index == 1).Generation });
    runtime.Api.LdSerialForWriteGuard = index => index switch { 0 => "DEMO-001", 1 => "DEMO-002", _ => null };
    runtime.Api.ScriptWriteBusy = serial => serial == "DEMO-001";
    runtime.Api.AnyScriptWriteBusy = () => true;
    using var http = new HttpClient();
    var rows = await runtime.LdPlayer!.RefreshAsync();
    async Task<HttpResponseMessage> Send(int index) => await http.PostAsJsonAsync($"http://localhost:{basicPort}/ldplayer/instances/{index}/keyevent",
        new { expectedGeneration = rows.Single(x => x.Index == index).Generation, key = "home" });
    using var first = await Send(0); Check((int)first.StatusCode == 409, "Busy instance write was not rejected");
    using var second = await Send(1); Check(second.IsSuccessStatusCode, "Script on A blocked unrelated B");
    runtime.Api.LdSerialForWriteGuard = null;
    using var unknown = await Send(1); Check((int)unknown.StatusCode == 409, "Unknown mapping bypassed conservative script guard");
    runtime.Api.UiWriteBusy = true;
    using var uiBusy = await Send(1); Check((int)uiBusy.StatusCode == 409, "Global UI busy protection was lost");
});
Test("all device writes reject browser simple and cross-site requests", async () =>
{
    var basicPort = Port(); var autoPort = Port();
    await using var runtime = await MatrixRuntime.CreateAsync(new() { BasicApiPort = basicPort, AutomatorApiPort = autoPort }, Temp(), true);
    await runtime.StartApisAsync(); var devicePort = await runtime.Api.StartDeviceAsync("DEMO-001"); using var http = new HttpClient();
    foreach (var url in new[] { $"http://localhost:{devicePort}/tap", $"http://localhost:{autoPort}/input/tap" })
    {
        using var plain = await http.PostAsync(url, new StringContent("{\"serial\":\"DEMO-001\",\"x\":20,\"y\":20}"));
        Check((int)plain.StatusCode == 415, "Simple cross-site content accepted: " + url);
        using var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(new { serial = "DEMO-001", x = 20, y = 20 }) };
        request.Headers.Add("Origin", "http://example.invalid"); using var crossSite = await http.SendAsync(request);
        Check((int)crossSite.StatusCode == 403, "Cross-site Origin accepted: " + url);
    }
    Check((await runtime.Backend.DumpAsync("DEMO-001")).ChildNodeList[1].Text == "Tap 0", "Rejected request had guest effect");
});
Test("serial binding edits cannot mutate the configuration merge baseline", () =>
{
    var path = Path.Combine(Temp(), "AppConfig.json"); var first = new SettingsStore(path); first.Load();
    first.Save(new() { LdSerialBindings = new() { [0] = "emulator-5554" } });
    var second = new SettingsStore(path); var loaded = second.Load();
    loaded.LdSerialBindings[0] = "explicit-new-serial";
    first.Save(first.Load() with { WallColumns = 8 });
    var merged = second.Save(loaded);
    Check(merged.LdSerialBindings[0] == "explicit-new-serial" && merged.WallColumns == 8, "In-place binding edit lost during merge");
    merged.LdSerialBindings[1] = "serial-1"; second.Save(merged);
    Check(new SettingsStore(path).Load().LdSerialBindings[1] == "serial-1", "Save result exposed internal baseline map");
    return Task.CompletedTask;
});
Test("concurrent serial binding edits merge by instance index", () =>
{
    var path = Path.Combine(Temp(), "AppConfig.json"); var first = new SettingsStore(path); var second = new SettingsStore(path);
    var a = first.Load(); var b = second.Load(); a.LdSerialBindings[0] = "serial-a"; first.Save(a);
    b.LdSerialBindings[1] = "serial-b"; var merged = second.Save(b);
    Check(merged.LdSerialBindings.Count == 2 && merged.LdSerialBindings[0] == "serial-a" && merged.LdSerialBindings[1] == "serial-b", "Independent instance binding was lost");
    a = first.Load(); b = second.Load(); a.LdSerialBindings.Remove(0); first.Save(a);
    b.LdSerialBindings[1] = "serial-b-new"; merged = second.Save(b);
    Check(!merged.LdSerialBindings.ContainsKey(0) && merged.LdSerialBindings[1] == "serial-b-new", "Untouched local key resurrected a removed binding");
    return Task.CompletedTask;
});

CatalogPathTests.Register(Test);

var liveIndex = Array.IndexOf(args, "--ldplayer-live-query");
if (liveIndex >= 0)
{
    var path = args.ElementAtOrDefault(liveIndex + 1) ?? throw new ArgumentException("Missing ldconsole path");
    Test("LIVE read-only LDPlayer HTTP list without ADB client", async () =>
    {
        var port = Port(); await using var runtime = await MatrixRuntime.CreateAsync(new() { BasicApiPort = port, LdConsolePath = path }, Temp());
        await runtime.Api.StartBasicAsync(port); using var http = new HttpClient();
        using var result = await http.GetAsync($"http://localhost:{port}/ldplayer/instances");
        Check(result.IsSuccessStatusCode, "Real read-only API query failed");
        Console.WriteLine("LIVE-LDPLAYER " + await result.Content.ReadAsStringAsync());
        Check(runtime.Devices.Snapshot.Count == 0 && runtime.Settings.AdbPath == "", "Read-only query entered ADB discovery");
    });
}

ScriptParameterTests.Register(Test);
ScriptJobTests.Register(Test);
DeviceWriteTests.Register(Test);
ApiStartupTests.Register(Test);

var failures = 0; var totalTimer = Stopwatch.StartNew();
foreach (var (name, run) in tests)
{
    try { await run().WaitAsync(TimeSpan.FromSeconds(30)); Console.WriteLine("PASS " + name); }
    catch (Exception error) { failures++; Console.WriteLine("FAIL " + name + "\n" + error); }
}
Console.WriteLine($"RESULT {tests.Count - failures}/{tests.Count} passed in {totalTimer.Elapsed.TotalSeconds:F2}s");
Environment.ExitCode = failures == 0 ? 0 : 1;

sealed class CaptureHandler : HttpMessageHandler
{
    public string Path { get; private set; } = "";
    public JsonElement? Body { get; private set; }
    public string Response { get; set; } = "{\"code\":0,\"message\":\"ok\",\"data\":\"done\"}";
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Path = request.RequestUri!.AbsolutePath;
        Body = request.Content is null ? null : JsonDocument.Parse(await request.Content.ReadAsStringAsync(cancellationToken)).RootElement.Clone();
        return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(Response, Encoding.UTF8, "application/json") };
    }
}

sealed class ControlledLdRunner : ILdConsoleRunner
{
    public string List { get; set; } = "0,雷电测试,100,200,1,7,9,1600,900,240";
    public List<string[]> Commands { get; } = [];
    public Task<string> RunAsync(IReadOnlyList<string> arguments, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        if (arguments[0] == "list2") return Task.FromResult(List);
        if (arguments[0] == "isrunning") return Task.FromResult("running");
        Commands.Add(arguments.ToArray()); return Task.FromResult("");
    }
}

sealed class GatedLdRunner : ILdConsoleRunner
{
    public bool Restarted { get; set; }
    public TaskCompletionSource Entered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public ConcurrentBag<string[]> Writes { get; } = [];
    public async Task<string> RunAsync(IReadOnlyList<string> arguments, CancellationToken ct)
    {
        if (arguments[0] == "list2") return $"0,测试零,100,200,1,{(Restarted ? 17 : 7)},9,1600,900,240\n1,测试一,101,201,1,8,10,1280,720,240";
        Writes.Add(arguments.ToArray());
        if (arguments[2] == "0") { Entered.TrySetResult(); await Release.Task.WaitAsync(ct); }
        return "";
    }
}

sealed class KeyGateBackend : IDeviceBackend, IIdentityPinnedDeviceBackend
{
    public DemoDeviceBackend Devices { get; } = new();
    public string? DiscoveryState { get; init; }
    public IIdentityPinnedDeviceCommands BindIdentity(DeviceCatalog catalog) => Devices.BindIdentity(catalog);
    public TaskCompletionSource Entered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public ConcurrentBag<string> Actions { get; } = [];
    public Task<IReadOnlyList<DeviceInfo>> DiscoverAsync(CancellationToken ct = default) => DiscoveryState is null ? Devices.DiscoverAsync(ct) : DiscoverStateAsync(ct);
    private async Task<IReadOnlyList<DeviceInfo>> DiscoverStateAsync(CancellationToken ct)
        => (await Devices.DiscoverAsync(ct)).Select(device => device.Serial == "DEMO-001" ? device with { State = DiscoveryState! } : device).ToArray();
    public async Task KeyEventAsync(string serial, int code, CancellationToken ct = default)
    { if (code == 4) { Entered.SetResult(); await Release.Task.WaitAsync(ct); } Actions.Add(serial + ":key:" + code); }
    public Task TextAsync(string serial, string text, CancellationToken ct = default) { Actions.Add(serial + ":text:" + text); return Task.CompletedTask; }
    public Task<string> ShellAsync(string s, string c, bool root = false, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<byte[]> ScreenshotAsync(string s, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<AndroidNode> DumpAsync(string s, CancellationToken ct = default) => throw new NotSupportedException();
    public Task TapAsync(string s, int x, int y, CancellationToken ct = default) => throw new NotSupportedException();
    public Task SwipeAsync(string s, int x, int y, int x2, int y2, int ms, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<string> StartAppAsync(string s, string p, string? a = null, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<string> StopAppAsync(string s, string p, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<bool> ClearAppAsync(string s, string p, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<IReadOnlyList<AppInfo>> ListAppsAsync(string s, bool system = false, bool version = true, CancellationToken ct = default) => throw new NotSupportedException();
    public Task InstallAsync(string s, string p, CancellationToken ct = default) => throw new NotSupportedException();
    public Task UninstallAsync(string s, string p, CancellationToken ct = default) => throw new NotSupportedException();
    public Task PushAsync(string s, string p, string r, IProgress<double>? progress = null, CancellationToken ct = default) => throw new NotSupportedException();
    public Task PullAsync(string s, string r, string p, IProgress<double>? progress = null, CancellationToken ct = default) => throw new NotSupportedException();
    public Task SetNodeTextAsync(string s, NodeSelector selector, string text, CancellationToken ct = default) => throw new NotSupportedException();
    public ValueTask DisposeAsync() => Devices.DisposeAsync();
}
