using MatrixDevice.Core;
using MatrixDevice.Features;

internal static class ApplicationBatchTests
{
    public static void Register(Action<string, Func<Task>> test)
    {
        test("Application batch requires explicit targets and preserves exact row pairs without Cartesian uninstall", async () =>
        {
            using var apk = new TestApk();
            await Throws(() => ApplicationBatch.InstallPlan([], [apk.Path]));
            var targets = Targets();
            var install = ApplicationBatch.InstallPlan(targets, [apk.Path, apk.Path]);
            Check(install.Count == 2 && install.Select(item => item.Target.Serial).Distinct().Count() == 2, "Install target/file plan duplicated or omitted targets.");
            var rows = ApplicationBatch.RowPlan(ApplicationOperation.Uninstall, [new(targets[0], ApplicationOperation.List, "com.a.one"), new(targets[1], ApplicationOperation.List, "com.b.two"), new(targets[0], ApplicationOperation.List, "com.a.one")]);
            Check(rows.Count == 2 && rows[0].Target.Serial == "DEMO-001" && rows[0].PackageName == "com.a.one" && rows[1].Target.Serial == "DEMO-002" && rows[1].PackageName == "com.b.two", "Uninstall expanded across devices or lost exact package pairs.");
            await Throws(() => ApplicationBatch.RowPlan(ApplicationOperation.Clear, [new(targets[0], ApplicationOperation.List, "com.android.settings", IsSystem: true)]));
        });
        test("Application confirmation cancellation performs no discovery or application write", async () =>
        {
            await using var backend = new BatchBackend(); var checks = 0;
            var batch = new ApplicationBatch(backend, (_, _) => { checks++; return Task.CompletedTask; });
            var result = await batch.ExecuteAsync([new(Targets()[0], ApplicationOperation.Uninstall, "com.matrix.demo")], false);
            Check(checks == 0 && backend.ListCalls == 0 && backend.Writes.Count == 0 && result.Single().State == ApplicationTaskState.Cancelled, "Cancelled confirmation touched a guest.");
            using var apk = new TestApk();
            result = await batch.ExecuteAsync(ApplicationBatch.InstallPlan([Targets()[0]], [apk.Path]), false);
            Check(checks == 0 && backend.ListCalls == 0 && backend.Writes.Count == 0 && result.Single().State == ApplicationTaskState.Cancelled, "Cancelled install confirmation touched a guest.");
        });
        test("Application batch reports partial failures and verifies exact uninstall absence", async () =>
        {
            await using var backend = new BatchBackend { FailSerial = "DEMO-002" };
            var batch = new ApplicationBatch(backend, (_, _) => Task.CompletedTask);
            var result = await batch.ExecuteAsync(Targets().Select(target => new ApplicationBatchItem(target, ApplicationOperation.Uninstall, "com.matrix.demo")).ToArray(), true);
            Check(result[0].State == ApplicationTaskState.Succeeded && result[1].State == ApplicationTaskState.Failed && backend.Writes.Count == 2, "Partial failure was hidden or stopped unrelated target.");
            Check(!(await backend.Inner.ListAppsAsync("DEMO-001", true)).Any(app => app.PackageName == "com.matrix.demo") && (await backend.Inner.ListAppsAsync("DEMO-002", true)).Any(app => app.PackageName == "com.matrix.demo"), "Application write hit wrong device.");
        });
        test("Application backend false uninstall success and post-write list failure are not success", async () =>
        {
            await using var backend = new BatchBackend { IgnoreUninstall = true };
            var batch = new ApplicationBatch(backend, (_, _) => Task.CompletedTask);
            var result = await batch.ExecuteAsync([new(Targets()[0], ApplicationOperation.Uninstall, "com.matrix.demo")], true);
            Check(result.Single().State == ApplicationTaskState.Failed, "Still-installed package was reported removed.");
            using var apk = new TestApk(); backend.FailListAfterWrite = true;
            result = await batch.ExecuteAsync(ApplicationBatch.InstallPlan([Targets()[0]], [apk.Path]), true);
            Check(result.Single().State == ApplicationTaskState.Failed, "List refresh failure was reported as an install success.");
        });
        test("Application install distinguishes command completion from APK package verification", async () =>
        {
            await using var backend = new BatchBackend(); using var apk = new TestApk();
            var states = new List<ApplicationTaskState>(); var batch = new ApplicationBatch(backend, (_, _) => Task.CompletedTask);
            var result = await batch.ExecuteAsync(ApplicationBatch.InstallPlan([Targets()[0]], [apk.Path]), true, value => states.Add(value.State));
            Check(result.Single().State == ApplicationTaskState.CompletedUnverified && result[0].Applications is not null && result[0].Message.Contains("包名未核验", StringComparison.Ordinal), "Install claimed package identity verification without APK parsing.");
            Check(states.SequenceEqual(new[] { ApplicationTaskState.Waiting, ApplicationTaskState.Running, ApplicationTaskState.CompletedUnverified }), "Per-item progress missing.");
        });
        test("Application batch protects system apps using a fresh backend list even if UI row is false", async () =>
        {
            await using var backend = new BatchBackend(); var batch = new ApplicationBatch(backend, (_, _) => Task.CompletedTask);
            foreach (var operation in new[] { ApplicationOperation.Uninstall, ApplicationOperation.Clear })
            {
                var result = await batch.ExecuteAsync([new(Targets()[0], operation, "com.android.settings", IsSystem: false)], true);
                Check(result[0].State == ApplicationTaskState.Failed && backend.Writes.Count == 0, "Stale UI bypassed system-app protection.");
            }
        });
        test("Application cancellation completes in-flight observation and cancels only subsequent items", async () =>
        {
            await using var backend = new BatchBackend(); using var stop = new CancellationTokenSource();
            backend.AfterWrite = () => stop.Cancel(); var batch = new ApplicationBatch(backend, (_, _) => Task.CompletedTask);
            var result = await batch.ExecuteAsync(Targets().Select(target => new ApplicationBatchItem(target, ApplicationOperation.Uninstall, "com.matrix.demo")).ToArray(), true, stopAfterCurrent: stop.Token);
            Check(result[0].State == ApplicationTaskState.Succeeded && result[1].State == ApplicationTaskState.Cancelled && backend.Writes.Count == 1 && backend.ListCalls == 2, "Cancellation aborted observation, rolled back, or sent later writes.");
        });
        test("Application target generation, transport, offline and runtime gate changes reject writes", async () =>
        {
            foreach (var current in new[] { Info() with { Generation = 2 }, Info() with { TransportId = 20 }, Info() with { State = "Offline" } })
            {
                await using var backend = new BatchBackend(); var batch = new ApplicationBatch(backend, (target, _) => { target.RequireSame(current); return Task.CompletedTask; });
                var result = await batch.ExecuteAsync([new(Targets()[0], ApplicationOperation.Uninstall, "com.matrix.demo")], true);
                Check(result[0].State == ApplicationTaskState.Failed && backend.Writes.Count == 0 && backend.ListCalls == 0, "Changed device target was operated.");
            }
            await using var changedBackend = new BatchBackend(); var calls = 0;
            var changed = new ApplicationBatch(changedBackend, (_, _) => { if (++calls >= 2) throw new DeviceException("运行时已变化"); return Task.CompletedTask; });
            var late = await changed.ExecuteAsync([new(Targets()[0], ApplicationOperation.Uninstall, "com.matrix.demo")], true);
            Check(late[0].State == ApplicationTaskState.Failed && changedBackend.Writes.Count == 0 && changedBackend.ListCalls == 1, "Change during read was not checked before write.");
        });
        test("Application empty plans and invalid package names fail before any guest call", async () =>
        {
            await using var backend = new BatchBackend(); var batch = new ApplicationBatch(backend, (_, _) => Task.CompletedTask);
            await ThrowsAsync(() => batch.ExecuteAsync([], true));
            await ThrowsAsync(() => batch.ExecuteAsync([new(Targets()[0], ApplicationOperation.Uninstall, "invalid package")], true));
            Check(backend.ListCalls == 0 && backend.Writes.Count == 0, "Invalid batch touched backend.");
        });
    }
    private static DeviceInfo Info() => new() { Serial = "DEMO-001", Name = "演示 A", Generation = 1, State = "Ready" };
    private static ApplicationTarget[] Targets() => [ApplicationTarget.Freeze(Info()), ApplicationTarget.Freeze(Info() with { Serial = "DEMO-002", Name = "演示 B" })];
    private static void Check(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    private static Task Throws(Action action) { try { action(); } catch (DeviceException) { return Task.CompletedTask; } throw new InvalidOperationException("Expected DeviceException."); }
    private static async Task ThrowsAsync(Func<Task> action) { try { await action(); } catch (DeviceException) { return; } throw new InvalidOperationException("Expected DeviceException."); }
    private sealed class TestApk : IDisposable
    {
        public string Path { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "matrix-application-fixture-" + Guid.NewGuid().ToString("N") + ".apk");
        public TestApk() => File.WriteAllText(Path, "Offline fixture, not an Android package.");
        public void Dispose() => File.Delete(Path);
    }
    private sealed class BatchBackend : IDeviceBackend
    {
        public DemoDeviceBackend Inner { get; } = new();
        public List<(string Serial, string Package)> Writes { get; } = [];
        public string? FailSerial { get; set; }
        public bool IgnoreUninstall { get; set; }
        public bool FailListAfterWrite { get; set; }
        public Action? AfterWrite { get; set; }
        public int ListCalls { get; private set; }
        public Task<IReadOnlyList<AppInfo>> ListAppsAsync(string serial, bool showSystem = false, bool needVersion = true, CancellationToken ct = default)
        { ListCalls++; if (FailListAfterWrite && Writes.Count > 0) throw new DeviceException("fixture list failure"); return Inner.ListAppsAsync(serial, showSystem, needVersion, ct); }
        private void Write(string serial, string package) { Writes.Add((serial, package)); if (serial == FailSerial) throw new DeviceException("fixture write failure"); }
        public async Task UninstallAsync(string serial, string packageName, CancellationToken ct = default) { Write(serial, packageName); if (!IgnoreUninstall) await Inner.UninstallAsync(serial, packageName, ct); AfterWrite?.Invoke(); }
        public Task InstallAsync(string serial, string localPath, CancellationToken ct = default) { Write(serial, localPath); AfterWrite?.Invoke(); return Task.CompletedTask; }
        public Task<string> StartAppAsync(string serial, string packageName, string? activity = null, CancellationToken ct = default) { Write(serial, packageName); return Inner.StartAppAsync(serial, packageName, activity, ct); }
        public Task<string> StopAppAsync(string serial, string packageName, CancellationToken ct = default) { Write(serial, packageName); return Inner.StopAppAsync(serial, packageName, ct); }
        public Task<bool> ClearAppAsync(string serial, string packageName, CancellationToken ct = default) { Write(serial, packageName); return Inner.ClearAppAsync(serial, packageName, ct); }
        public Task<IReadOnlyList<DeviceInfo>> DiscoverAsync(CancellationToken ct = default) => Inner.DiscoverAsync(ct);
        public Task<string> ShellAsync(string serial, string command, bool useRoot = false, CancellationToken ct = default) => Inner.ShellAsync(serial, command, useRoot, ct);
        public Task<byte[]> ScreenshotAsync(string serial, CancellationToken ct = default) => Inner.ScreenshotAsync(serial, ct);
        public Task<AndroidNode> DumpAsync(string serial, CancellationToken ct = default) => Inner.DumpAsync(serial, ct);
        public Task TapAsync(string serial, int x, int y, CancellationToken ct = default) => Inner.TapAsync(serial, x, y, ct);
        public Task SwipeAsync(string serial, int x1, int y1, int x2, int y2, int durationMs, CancellationToken ct = default) => Inner.SwipeAsync(serial, x1, y1, x2, y2, durationMs, ct);
        public Task KeyEventAsync(string serial, int code, CancellationToken ct = default) => Inner.KeyEventAsync(serial, code, ct);
        public Task TextAsync(string serial, string text, CancellationToken ct = default) => Inner.TextAsync(serial, text, ct);
        public Task PushAsync(string serial, string localPath, string remotePath, IProgress<double>? progress = null, CancellationToken ct = default) => Inner.PushAsync(serial, localPath, remotePath, progress, ct);
        public Task PullAsync(string serial, string remotePath, string localPath, IProgress<double>? progress = null, CancellationToken ct = default) => Inner.PullAsync(serial, remotePath, localPath, progress, ct);
        public Task SetNodeTextAsync(string serial, NodeSelector selector, string text, CancellationToken ct = default) => Inner.SetNodeTextAsync(serial, selector, text, ct);
        public ValueTask DisposeAsync() => Inner.DisposeAsync();
    }
}
