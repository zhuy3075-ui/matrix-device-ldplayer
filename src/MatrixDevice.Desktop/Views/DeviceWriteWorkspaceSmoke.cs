using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Windows;
using System.Windows.Threading;
using MatrixDevice.Core;
using MatrixDevice.Features;
using MatrixDevice.Host;

namespace MatrixDevice.Desktop;

internal static class DeviceWriteWorkspaceSmoke
{
    internal static async Task RunAsync(string directory, Dispatcher dispatcher)
    {
        var app = Application.Current;
        if (app is null || !ReferenceEquals(app.Dispatcher, dispatcher) || !dispatcher.CheckAccess()
            || Thread.CurrentThread.GetApartmentState() != ApartmentState.STA || app.Windows.Count != 0 || app.MainWindow is not null
            || !Path.IsPathFullyQualified(directory) || !directory.Contains(Path.DirectorySeparatorChar + "artifacts" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
            || Directory.Exists(directory) || File.Exists(directory))
            throw new InvalidOperationException("Write ownership acceptance requires a fresh artifacts directory and an owned windowless STA application.");
        Directory.CreateDirectory(directory);
        var results = new List<CaseResult>();
        var retained = new List<Fixture>();
        await Case("text-A-holds-API-A-rejects-B-and-read-continue", async f =>
        {
            f.Backend.HoldText = true; f.Vm.TextInput = "owned-A";
            var writing = f.Track(f.Vm.SendTextAsync()); await f.Observe(f.Backend.Entered.Task);
            f.Check(f.Writes.IsBusy("DEMO-001") && !f.Runtime.Api.UiWriteBusy, "VM text has a targeted lease, not a global HTTP block.");
            await f.CheckApi(HttpStatusCode.Conflict, HttpStatusCode.OK);
            f.Backend.Release.TrySetResult(); await f.Wait(writing);
            f.Check(f.Vm.Error.Length == 0 && !f.Writes.IsBusy("DEMO-001"), "Completed text releases its original lease.");
        });
        await Case("upload-install-keeps-lease-during-cancel", async f =>
        {
            f.Backend.HoldPush = true; f.Backend.HoldInstall = true;
            var apk = Path.Combine(f.Directory, "owned-demo.apk"); await File.WriteAllBytesAsync(apk, [1, 2, 3]);
            f.Vm.QueuePush(apk, true); var transferring = f.Track(f.Vm.StartTransfersAsync());
            await f.Observe(f.Backend.Entered.Task); f.Check(f.Writes.IsBusy("DEMO-001") && !f.Runtime.Api.UiWriteBusy, "Upload holds A without blocking B.");
            await f.CheckApi(HttpStatusCode.Conflict, HttpStatusCode.OK);
            f.Backend.Release.TrySetResult(); await f.Observe(f.Backend.InstallEntered.Task);
            f.Check(f.Writes.IsBusy("DEMO-001"), "The lease spans Push through Install.");
            f.Vm.CancelTransfersCommand.Execute(null);
            f.Check(!transferring.IsCompleted && f.Writes.IsBusy("DEMO-001"), "Cancellation does not release an ignored in-flight install.");
            await f.CheckApi(HttpStatusCode.Conflict, HttpStatusCode.OK);
            f.Backend.InstallRelease.TrySetResult(); await f.Wait(transferring);
            f.Check(!f.Writes.IsBusy("DEMO-001") && !f.Vm.Transferring, "Original transfer and install drained before release.");
        });
        await Case("busy-B-rejects-whole-VM-text-group", async f =>
        {
            f.Vm.Devices.Single(row => row.Serial == "DEMO-002").IsSelected = true;
            using (f.Writes.Acquire(["DEMO-002"]))
            {
                f.Vm.TextInput = "must-not-send"; await f.Wait(f.Vm.SendTextAsync());
                f.Check(f.Vm.Error.Length > 0 && !f.Writes.IsBusy("DEMO-001") && f.Backend.Calls.All(call => call.Operation != "Text"), "Busy B rejects the entire group without writing or retaining A.");
            }
        });
        await Case("shell-failure-releases-VM-lease", async f =>
        {
            f.Backend.HoldShell = true; f.Vm.ShellInput = "owned-failure";
            var writing = f.Track(f.Vm.ExecuteShellAsync()); await f.Observe(f.Backend.Entered.Task);
            f.Check(f.Writes.IsBusy("DEMO-001"), "Shell holds its complete operation.");
            f.Backend.Release.TrySetResult(); await f.Wait(writing);
            f.Check(f.Vm.Error.Length > 0 && f.Vm.ShellInput.Length == 0 && !f.Writes.IsBusy("DEMO-001"), "Original shell failure is visible and its lease is released.");
        });
        await Case("VM-busy-Up-clears-cached-Down", async f =>
        {
            var a = f.Vm.CurrentDevice!.Info;
            var input = new InputSnapshot { SourceSerial = a.Serial, SourceGeneration = a.Generation,
                Generation = f.Runtime.Input.CurrentGeneration, GestureId = 701, SourceWidth = a.Width, SourceHeight = a.Height,
                X = 10, Y = 10, Kind = InputKind.Down };
            await f.Wait(f.Vm.SendInputAsync(input));
            f.Backend.HoldStart = true;
            var script = new ScriptPackage { Manifest = new ScriptManifest { Id = "owned-write", Name = "Owned write", Version = "1" },
                Steps = [new ScriptStep { Action = "launchapp", Value = "com.android.settings" }] };
            var running = f.Track(f.Runtime.Scripts.RunAsync(a.Serial, script));
            await f.Observe(f.Backend.Entered.Task);
            await f.Wait(f.Vm.SendInputAsync(input with { Kind = InputKind.Up }));
            f.Check(f.Vm.Error.Length > 0, "The original UI script-busy refusal remains visible.");
            f.Backend.Release.TrySetResult(); await f.Wait(running);
            await f.Wait(f.Vm.SendInputAsync(input with { Kind = InputKind.Up }));
            f.Check(f.Backend.Calls.All(call => call.Operation != "Tap" && call.Operation != "Swipe"), "A repeated UI Up cannot send the abandoned gesture after the script completes.");
        });
        var windowsUnchanged = app.Windows.Count == 0 && app.MainWindow is null;
        var pass = windowsUnchanged && retained.Count == 0 && results.Count == 5 && results.All(row => row.Passed);
        File.WriteAllText(Path.Combine(directory, "summary.json"), JsonSerializer.Serialize(new
        {
            status = pass ? "PASS-WRITE-OWNERSHIP-VM-NOT-GUI" : "FAIL-WRITE-OWNERSHIP-VM", results, windowsUnchanged, retainedOwners = retained.Count,
            scope = "Owned Demo backend, actual WorkspaceViewModel text/transfer/auto-install/Shell/Input and loopback HTTP. No Window, physical input, real emulator or ADB. The APK is inert bytes accepted only by Demo.",
            untested = new[] { "physical GUI", "ApplicationsView actual picker/confirmation", "real simulator", "cross-process ownership", "Runtime swap", "final dual 30 minutes" }
        }, JsonDefaults.Options));
        if (!pass) throw new InvalidOperationException("Write ownership VM cases did not all pass; preserve the report.");

        async Task Case(string name, Func<Fixture, Task> body)
        {
            if (retained.Count != 0)
            {
                results.Add(new(name, "NOT-RUN", false, "A previous fixture did not prove cleanup; no later owner was created.", null, [], [], []));
                return;
            }
            var f = new Fixture(Path.Combine(directory, "case-" + (results.Count + 1)));
            Exception? error = null; Exception? cleanup = null;
            try { await f.Initialize(); await body(f); }
            catch (Exception failure) { error = failure; }
            finally { try { await f.Cleanup(); } catch (Exception failure) { cleanup = failure; } }
            results.Add(new CaseResult(name, error is null && cleanup is null ? "PASS" : "FAIL", error is null && cleanup is null, error?.ToString(), cleanup?.ToString(), f.Checks.ToArray(), f.Backend.Calls.ToArray(), f.Tasks.Select(task => task.Status.ToString()).ToArray()));
            if (cleanup is not null) retained.Add(f);
        }
    }

    private sealed record CaseResult(string Name, string Status, bool Passed, string? Error, string? CleanupError, string[] Checks, Call[] Calls, string[] Tasks);
    private sealed record Call(string Operation, string Serial);

    private sealed class Fixture(string directory)
    {
        internal string Directory => directory;
        internal readonly DeviceWriteCoordinator Writes = new();
        internal readonly HeldDemoBackend Backend = new();
        internal readonly List<Task> Tasks = [];
        internal readonly List<string> Checks = [];
        internal MatrixRuntime Runtime = null!;
        internal WorkspaceViewModel Vm = null!;
        private RollingLog? log;
        private CatalogRepository? metadata;
        private ScriptScheduler? scripts;
        private InputCoordinator? input;
        private ApiServiceManager? api;
        private readonly HttpClient client = new() { Timeout = TimeSpan.FromSeconds(8) };
        private int portA, portB;
        internal Task Track(Task task) { Tasks.Add(task); return task; }
        internal Task<T> Track<T>(Task<T> task) { Tasks.Add(task); return task; }
        internal async Task Wait(Task task) { if (!Tasks.Contains(task)) Tasks.Add(task); await task.WaitAsync(TimeSpan.FromSeconds(10)); }
        internal Task Observe(Task signal) => signal.WaitAsync(TimeSpan.FromSeconds(10));
        internal void Check(bool value, string message) { Checks.Add(message); if (!value) throw new InvalidOperationException(message); }
        internal async Task Initialize()
        {
            System.IO.Directory.CreateDirectory(directory);
            var settings = new AppSettings { AutoStartApi = false, AutoConnect = false, SyncKeyboard = false, SyncMouse = false, AdbAllowedSerials = [], LdSerialBindings = [], AdbPath = "", LdConsolePath = "" };
            var catalog = new DeviceCatalog(Backend); var ownLog = new RollingLog(Path.Combine(directory, "logs")); log = ownLog;
            var automation = new AutomationService(Backend, catalog);
            var ownScripts = new ScriptScheduler(Backend, Writes); scripts = ownScripts;
            var ownInput = new InputCoordinator(catalog, Backend, Writes); input = ownInput;
            var ownApi = new ApiServiceManager(catalog, Backend, automation, ownLog, Writes); api = ownApi;
            var ownMetadata = new CatalogRepository(Path.Combine(directory, "db.sqlite3")); metadata = ownMetadata;
            Runtime = new MatrixRuntime { Writes = Writes, Backend = Backend, Devices = catalog, Scripts = ownScripts, Input = ownInput, Api = ownApi, Automation = automation,
                Log = ownLog, Settings = settings, FrozenConnectionSettings = settings, SettingsStore = new SettingsStore(Path.Combine(directory, "AppConfig.json")),
                Metadata = ownMetadata, IsDemo = true };
            Vm = new WorkspaceViewModel(Runtime, directory, true);
            await Wait(Vm.RefreshDevicesAsync());
            Vm.CurrentDevice = Vm.Devices.Single(row => row.Serial == "DEMO-001");
            foreach (var row in Vm.Devices) row.IsSelected = row.Serial == "DEMO-001";
            portA = await ownApi.StartDeviceAsync("DEMO-001"); portB = await ownApi.StartDeviceAsync("DEMO-002");
        }
        internal async Task CheckApi(HttpStatusCode a, HttpStatusCode b)
        {
            var beforeA = Backend.Calls.Count(call => call.Serial == "DEMO-001" && call.Operation == "Text");
            var beforeB = Backend.Calls.Count(call => call.Serial == "DEMO-002" && call.Operation == "Text");
            using var responseA = await Track(client.PostAsJsonAsync($"http://127.0.0.1:{portA}/text", new { Text = "peer-A" }));
            using var responseB = await Track(client.PostAsJsonAsync($"http://127.0.0.1:{portB}/text", new { Text = "peer-B" }));
            using var read = await Track(client.GetAsync($"http://127.0.0.1:{portA}/device"));
            Check(responseA.StatusCode == a && responseB.StatusCode == b && read.StatusCode == HttpStatusCode.OK, "HTTP A/B admission and read status match the held VM operation.");
            Check(Backend.Calls.Count(call => call.Serial == "DEMO-001" && call.Operation == "Text") == beforeA, "This rejected HTTP request did not write A before returning conflict.");
            Check(Backend.Calls.Count(call => call.Serial == "DEMO-002" && call.Operation == "Text") == beforeB + 1, "This HTTP request reached B exactly once while A remained held.");
        }
        internal async Task Cleanup()
        {
            Backend.Release.TrySetResult(); Backend.InstallRelease.TrySetResult();
            foreach (var task in Tasks.ToArray()) { try { await task.WaitAsync(TimeSpan.FromSeconds(12)); } catch when (task.IsCompleted) { } }
            foreach (var task in Backend.Tasks.ToArray()) { if (!Tasks.Contains(task)) Tasks.Add(task); try { await task.WaitAsync(TimeSpan.FromSeconds(12)); } catch when (task.IsCompleted) { } }
            if (Vm is not null) await ObserveCleanup(Vm.ShutdownAsync());
            else if (Runtime is not null) await ObserveCleanup(Runtime.DisposeAsync().AsTask());
            else
            {
                if (api is not null) await ObserveCleanup(api.DisposeAsync().AsTask());
                if (input is not null) await ObserveCleanup(input.DisposeAsync().AsTask());
                if (scripts is not null) await ObserveCleanup(scripts.DisposeAsync().AsTask());
                await ObserveCleanup(Backend.DisposeAsync().AsTask());
                metadata?.Dispose();
                if (log is not null) await ObserveCleanup(log.DisposeAsync().AsTask());
            }
            client.Dispose();
            Check(Tasks.All(task => task.IsCompleted) && Backend.Tasks.All(task => task.IsCompleted), "Original tasks and forwarding operations are terminal.");
            using var noOutstandingLeases = Writes.AcquireAll();
        }
        private async Task ObserveCleanup(Task task) { Tasks.Add(task); await task.WaitAsync(TimeSpan.FromSeconds(12)); }
    }

    private sealed class HeldDemoBackend : IDeviceBackend
    {
        private readonly DemoDeviceBackend inner = new();
        internal readonly ConcurrentQueue<Call> Calls = new();
        internal readonly ConcurrentQueue<Task> Tasks = new();
        internal readonly TaskCompletionSource Entered = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal readonly TaskCompletionSource Release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal readonly TaskCompletionSource InstallEntered = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal readonly TaskCompletionSource InstallRelease = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal bool HoldText, HoldPush, HoldInstall, HoldShell, HoldStart;
        private Task Forward(string operation, string serial, Func<Task> action)
        {
            Calls.Enqueue(new(operation, serial)); var task = Execute(); Tasks.Enqueue(task); return task;
            async Task Execute()
            {
                if (serial == "DEMO-001" && (operation == "Text" && HoldText || operation == "Push" && HoldPush || operation == "Shell" && HoldShell || operation == "StartApp" && HoldStart))
                { Entered.TrySetResult(); await Release.Task; }
                if (serial == "DEMO-001" && operation == "Install" && HoldInstall) { InstallEntered.TrySetResult(); await InstallRelease.Task; }
                await action();
            }
        }
        public Task<IReadOnlyList<DeviceInfo>> DiscoverAsync(CancellationToken ct = default) => inner.DiscoverAsync(ct);
        public async Task<string> ShellAsync(string serial, string command, bool useRoot = false, CancellationToken ct = default)
        {
            string output = "";
            await Forward("Shell", serial, async () => { if (HoldShell && serial == "DEMO-001") throw new InvalidOperationException("Owned shell failure."); output = await inner.ShellAsync(serial, command, useRoot, ct); });
            return output;
        }
        public Task<byte[]> ScreenshotAsync(string serial, CancellationToken ct = default) => inner.ScreenshotAsync(serial, ct);
        public Task<AndroidNode> DumpAsync(string serial, CancellationToken ct = default) => inner.DumpAsync(serial, ct);
        public Task TapAsync(string serial, int x, int y, CancellationToken ct = default) => Forward("Tap", serial, () => inner.TapAsync(serial, x, y, ct));
        public Task SwipeAsync(string serial, int x1, int y1, int x2, int y2, int durationMs, CancellationToken ct = default) => Forward("Swipe", serial, () => inner.SwipeAsync(serial, x1, y1, x2, y2, durationMs, ct));
        public Task KeyEventAsync(string serial, int code, CancellationToken ct = default) => inner.KeyEventAsync(serial, code, ct);
        public Task TextAsync(string serial, string text, CancellationToken ct = default) => Forward("Text", serial, () => inner.TextAsync(serial, text, ct));
        public async Task<string> StartAppAsync(string serial, string packageName, string? activity = null, CancellationToken ct = default)
        {
            string output = "";
            await Forward("StartApp", serial, async () => output = await inner.StartAppAsync(serial, packageName, activity, ct));
            return output;
        }
        public Task<string> StopAppAsync(string serial, string packageName, CancellationToken ct = default) => inner.StopAppAsync(serial, packageName, ct);
        public Task<bool> ClearAppAsync(string serial, string packageName, CancellationToken ct = default) => inner.ClearAppAsync(serial, packageName, ct);
        public Task<IReadOnlyList<AppInfo>> ListAppsAsync(string serial, bool showSystem = false, bool needVersion = true, CancellationToken ct = default) => inner.ListAppsAsync(serial, showSystem, needVersion, ct);
        public Task InstallAsync(string serial, string localPath, CancellationToken ct = default) => Forward("Install", serial, () => inner.InstallAsync(serial, localPath, CancellationToken.None));
        public Task UninstallAsync(string serial, string packageName, CancellationToken ct = default) => inner.UninstallAsync(serial, packageName, ct);
        public Task PushAsync(string serial, string localPath, string remotePath, IProgress<double>? progress = null, CancellationToken ct = default) => Forward("Push", serial, () => inner.PushAsync(serial, localPath, remotePath, progress, ct));
        public Task PullAsync(string serial, string remotePath, string localPath, IProgress<double>? progress = null, CancellationToken ct = default) => inner.PullAsync(serial, remotePath, localPath, progress, ct);
        public Task SetNodeTextAsync(string serial, NodeSelector selector, string text, CancellationToken ct = default) => inner.SetNodeTextAsync(serial, selector, text, ct);
        public ValueTask DisposeAsync() => inner.DisposeAsync();
    }
}
