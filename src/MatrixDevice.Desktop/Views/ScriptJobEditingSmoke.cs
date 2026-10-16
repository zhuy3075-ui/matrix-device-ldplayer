using System.Collections.Concurrent;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Windows;
using System.Windows.Threading;
using MatrixDevice.Core;
using MatrixDevice.Features;
using MatrixDevice.Host;

namespace MatrixDevice.Desktop;

internal static class ScriptJobEditingSmoke
{
    internal static async Task RunAsync(string dataDirectory, Dispatcher dispatcher)
    {
        if (!dispatcher.CheckAccess() || Thread.CurrentThread.GetApartmentState() != ApartmentState.STA
            || Application.Current.Windows.Count != 0 || Application.Current.MainWindow is not null)
            throw new InvalidOperationException("脚本任务 VM 验收仅限无窗口的自有 Demo/STA。");
        var directory = Path.Combine(dataDirectory, "script-job-editing");
        if (Directory.Exists(directory)) throw new InvalidOperationException("任务验收目录已存在。");
        Directory.CreateDirectory(directory);
        var reports = new List<object>(); var failures = new List<Exception>(); var failureClasses = new HashSet<string>(); var stopRemaining = false;
        await Case("selection-and-package-edit-do-not-retarget", async state =>
        {
            await state.Import("com.android.settings", 0);
            state.Select(true, false); await state.Vm.StartScriptAsync();
            var job = state.Runtime.Scripts.GetJobs().Single();
            await state.Backend.Entered("DEMO-001");
            state.Select(false, true); state.Vm.CurrentDevice = state.B;
            await state.Import("com.matrix.demo", 0);
            state.Backend.Release("DEMO-001");
            await state.Terminal(job.JobId); await state.Fence();
            Require((await state.Backend.DumpAsync("DEMO-001")).PackageName == "com.android.settings", "编辑包或目标选择改变了已接纳任务的实际 A 效果。");
            Require((await state.Backend.DumpAsync("DEMO-002")).PackageName == "com.matrix.demo", "未接纳的 B 被执行。");
            Require(state.Vm.ScriptJobs.Single().JobId == job.JobId && state.Vm.SelectedScriptJob?.JobId == job.JobId, "任务记录身份漂移。");
            Require(state.Vm.ScriptLogs.Count == state.Runtime.Scripts.GetJobLogs(job.JobId).Count, "切换当前设备覆盖了选中任务日志。");
        });
        await Case("cancel-job-not-current-device-selection", async state =>
        {
            await state.Import("com.android.settings", 0);
            state.Select(true, true); await state.Vm.StartScriptAsync();
            var jobs = state.Runtime.Scripts.GetJobs();
            var a = jobs.Single(job => job.Target.Serial == "DEMO-001"); var b = jobs.Single(job => job.Target.Serial == "DEMO-002");
            await Task.WhenAll(state.Backend.Entered("DEMO-001"), state.Backend.Entered("DEMO-002"));
            state.Vm.SelectedScriptJob = state.Vm.ScriptJobs.Single(row => row.JobId == a.JobId);
            state.Select(false, true); state.Vm.CurrentDevice = state.B;
            Require(state.Vm.CancelScriptJobCommand.CanExecute(null), "活动任务取消命令不可用。");
            state.Vm.CancelScriptJobCommand.Execute(null);
            state.Backend.Release("DEMO-002");
            await Task.WhenAll(state.Terminal(a.JobId), state.Terminal(b.JobId)); await state.Fence();
            Require(state.Runtime.Scripts.GetJob(a.JobId)!.State == ScriptRunState.Cancelled && state.Runtime.Scripts.GetJob(b.JobId)!.State == ScriptRunState.Completed, "按任务取消停错设备或影响并行任务。");
            Require((await state.Backend.DumpAsync("DEMO-001")).PackageName == "com.matrix.demo" && (await state.Backend.DumpAsync("DEMO-002")).PackageName == "com.android.settings", "取消后的实际双目标效果不符合预期。");
        });
        await Case("old-selected-job-cannot-cancel-new-same-device-run", async state =>
        {
            await state.Import("com.matrix.demo", 0); state.Select(true, false); await state.Vm.StartScriptAsync();
            var old = state.Runtime.Scripts.GetJobs().Single(); await state.Backend.Entered("DEMO-001"); state.Backend.Release("DEMO-001"); await state.Terminal(old.JobId); await state.Fence();
            var oldRow = state.Vm.ScriptJobs.Single(); var oldLogs = state.Runtime.Scripts.GetJobLogs(old.JobId).ToArray();
            state.Backend.Block("DEMO-001"); await state.Import("com.android.settings", 0); await state.Vm.StartScriptAsync();
            var next = state.Runtime.Scripts.GetJobs().Single(job => job.JobId != old.JobId);
            await state.Backend.Entered("DEMO-001");
            state.Vm.SelectedScriptJob = oldRow;
            Require(!state.Vm.CancelScriptJobCommand.CanExecute(null), "终态任务仍显示可取消。");
            state.Vm.CancelSelectedScriptJob();
            Require(state.Runtime.Scripts.GetJob(next.JobId)!.State == ScriptRunState.Running, "取消旧记录改变了在途新任务状态。");
            state.Backend.Release("DEMO-001");
            await state.Terminal(next.JobId); await state.Fence();
            Require(state.Runtime.Scripts.GetJob(next.JobId)!.State == ScriptRunState.Completed && (await state.Backend.DumpAsync("DEMO-001")).PackageName == "com.android.settings", "旧任务取消影响同串号新任务。");
            Require(oldLogs.SequenceEqual(state.Runtime.Scripts.GetJobLogs(old.JobId)) && state.Vm.ScriptJobs.Count == 2, "后一次运行覆盖了旧任务日志或记录。");
        });
        await Case("stale-row-rejected-without-metadata-or-effect", async state =>
        {
            await state.Import("com.android.settings", 0); state.Select(true, false);
            state.A.Info = state.A.Info with { Generation = state.A.Info.Generation + 1 };
            await state.Vm.StartScriptAsync(); await state.Fence();
            Require(state.Vm.Error.Length > 0 && state.Runtime.Scripts.GetJobs().Count == 0 && state.Vm.ScriptJobs.Count == 0, "失效身份被接纳或未显示拒绝。");
            Require(state.Runtime.Metadata.GetAll<ScriptDeviceParameters>("ScriptDeviceConfiguration").Count == 0, "未接纳的身份写入参数记录。");
            Require((await state.Backend.DumpAsync("DEMO-001")).PackageName == "com.matrix.demo", "身份拒绝仍触发应用启动。");
        });
        await Case("busy-target-rejected-idle-target-admitted", async state =>
        {
            await state.Import("com.android.settings", 0); state.Select(true, true);
            state.OwnedRuns.Add(state.Runtime.Scripts.RunAsync("DEMO-001", new ScriptPackage
            {
                Manifest = new() { Id = "owned-legacy", Name = "自有占用", Version = "1.0" }, Steps = [new() { Action = "wait", DurationMs = 60000 }]
            }));
            Require(state.Vm.StartScriptCommand.CanExecute(null), "一个忙目标阻止了空闲目标的明确提交。");
            await state.Vm.StartScriptAsync();
            var job = state.Runtime.Scripts.GetJobs().Single();
            Require(job.Target.Serial == "DEMO-002" && state.Vm.Error.Contains("DEMO-001", StringComparison.Ordinal), "未正确汇总 A 拒绝与 B 接纳。");
            var saved = state.Runtime.Metadata.GetAll<ScriptDeviceParameters>("ScriptDeviceConfiguration");
            Require(saved.Count == 1 && saved.Single().Serial == "DEMO-002", "为未接纳的忙目标保存了参数。");
            await state.Backend.Entered("DEMO-002"); state.Backend.Release("DEMO-002"); await state.Terminal(job.JobId); await state.Fence();
            Require((await state.Backend.DumpAsync("DEMO-002")).PackageName == "com.android.settings" && state.Runtime.Scripts.GetStatus("DEMO-001").State == ScriptRunState.Running, "空闲目标无实际效果，或改变了原忙目标。");
        });
        File.WriteAllText(Path.Combine(directory, "summary.json"), JsonSerializer.Serialize(new
        {
            status = failures.Count == 0 ? "PASS-SCRIPT-JOB-EDITING" : failureClasses.Count == 1 ? failureClasses.Single() : "MIXED-FAIL", reports, failureClasses,
            scope = "Owned Demo backend and real WorkspaceViewModel import/start/select/cancel/log paths. No Window, physical input, GUI, ADB, APK, API or real-instance acceptance.",
            windows = Application.Current.Windows.Count, errors = failures.Select(error => error.ToString()).ToArray()
        }, JsonDefaults.Options));
        if (failures.Count > 0) throw new AggregateException("脚本任务 VM 验收失败", failures);

        async Task Case(string name, Func<State, Task> action)
        {
            if (stopRemaining) { reports.Add(new { name, status = "NOT-RUN", reason = "前一案例清理失败" }); return; }
            var state = new State(Path.Combine(directory, name), dispatcher); Exception? failure = null; bool cleaned = false, initialized = false; var classes = new HashSet<string>();
            try { await state.Initialize(); initialized = true; await action(state); }
            catch (Exception error) { failure = error; classes.Add(!initialized ? "PRECONDITION-FAIL" : error is ProductFailure ? "PRODUCT-FAIL" : "FIXTURE-FAIL"); }
            finally
            {
                try { await state.Cleanup(); cleaned = true; }
                catch (Exception error) { failure = failure is null ? error : new AggregateException(failure, error); classes.Add("CLEANUP-FAIL"); }
            }
            foreach (var classification in classes) failureClasses.Add(classification);
            reports.Add(new { name, status = failure is null ? "PASS" : classes.Count == 1 ? classes.Single() : "MIXED-FAIL", failureClasses = classes, initialized, cleaned, jobs = state.Jobs, error = failure?.ToString() });
            if (failure is not null) failures.Add(failure);
            if (!cleaned) stopRemaining = true;
        }
    }

    private sealed class ProductFailure(string message) : Exception(message);
    private static void Require(bool condition, string message) { if (!condition) throw new ProductFailure(message); }
    private sealed class State(string directory, Dispatcher dispatcher)
    {
        internal MatrixRuntime Runtime = null!;
        internal WorkspaceViewModel Vm = null!;
        internal readonly JobBackend Backend = new();
        internal DeviceRow A => Vm.Devices.Single(row => row.Serial == "DEMO-001");
        internal DeviceRow B => Vm.Devices.Single(row => row.Serial == "DEMO-002");
        internal ScriptJobStatus[] Jobs = [];
        internal readonly List<Task> OwnedRuns = [];
        private readonly ConcurrentDictionary<Guid, TaskCompletionSource> terminals = new();
        internal async Task Initialize()
        {
            Directory.CreateDirectory(directory);
            var settings = new AppSettings { Language = "zh-CN" };
            var catalog = new DeviceCatalog(Backend); var scheduler = new ScriptScheduler(Backend, catalog, true);
            var automation = new AutomationService(Backend, catalog); var log = new RollingLog(Path.Combine(directory, "logs"));
            Runtime = new MatrixRuntime
            {
                Backend = Backend, Devices = catalog, Scripts = scheduler, Automation = automation,
                Input = new InputCoordinator(catalog, Backend), Log = log,
                Metadata = new CatalogRepository(Path.Combine(directory, "db.sqlite3")), Settings = settings, FrozenConnectionSettings = settings,
                SettingsStore = new SettingsStore(Path.Combine(directory, "AppConfig.json")), IsDemo = true,
                Api = new ApiServiceManager(catalog, Backend, automation, log)
            };
            Runtime.Scripts.JobChanged += OnJob;
            Vm = new WorkspaceViewModel(Runtime, directory, true); await Vm.RefreshDevicesAsync();
            Require(Runtime.IsDemo && Vm.Demo && Vm.Devices.Count == 2 && !Runtime.Api.BasicRunning && !Runtime.Api.AutomatorRunning, "自有无接口 Demo 前置不成立。");
            Vm.CurrentDevice = A;
        }
        private void OnJob(ScriptJobStatus status)
        {
            if (status.State is ScriptRunState.Completed or ScriptRunState.Cancelled or ScriptRunState.Failed)
                terminals.GetOrAdd(status.JobId, _ => new(TaskCreationOptions.RunContinuationsAsynchronously)).TrySetResult();
        }
        internal Task Terminal(Guid id) => terminals.GetOrAdd(id, _ => new(TaskCreationOptions.RunContinuationsAsynchronously)).Task.WaitAsync(TimeSpan.FromSeconds(10));
        internal Task Fence() => dispatcher.InvokeAsync(() => { }, DispatcherPriority.ContextIdle).Task;
        internal void Select(bool a, bool b) { A.IsSelected = a; B.IsSelected = b; Vm.UpdateSelection(); }
        internal async Task Import(string app, int wait)
        {
            var package = new ScriptPackage { Manifest = new() { Id = "owned-job", Name = "自有任务验收", Version = "1.0" }, Steps = [new() { Action = "wait", DurationMs = wait }, new() { Action = "launchapp", Value = app }] };
            var path = Path.Combine(directory, Guid.NewGuid().ToString("N") + ".jxscript");
            ScriptPackageService.Save(path, package); await Vm.ImportScriptAsync(path);
            Require(Vm.Error.Length == 0, "真实脚本导入失败：" + Vm.Error);
        }
        internal async Task Cleanup()
        {
            if (Runtime is null) { await Backend.DisposeAsync(); return; }
            if (Vm is null) { await Runtime.DisposeAsync(); return; }
            await Vm.ShutdownAsync().WaitAsync(TimeSpan.FromSeconds(10));
            await Task.WhenAll(OwnedRuns).WaitAsync(TimeSpan.FromSeconds(10));
            Runtime.Scripts.JobChanged -= OnJob;
            await Fence(); Jobs = Runtime.Scripts.GetJobs().ToArray();
            Require(Jobs.All(job => job.State is ScriptRunState.Completed or ScriptRunState.Cancelled or ScriptRunState.Failed), "清理后仍有活动任务。");
            Require(Application.Current.Windows.Count == 0 && Application.Current.MainWindow is null, "无窗验收创建了窗口。");
        }
    }

    private sealed class JobBackend : IDeviceBackend, IIdentityPinnedDeviceBackend
    {
        private sealed class Gate
        {
            internal readonly TaskCompletionSource Entered = new(TaskCreationOptions.RunContinuationsAsynchronously);
            internal readonly TaskCompletionSource Released = new(TaskCreationOptions.RunContinuationsAsynchronously);
        }
        private readonly DemoDeviceBackend inner = new();
        private readonly ConcurrentDictionary<string, Gate> gates = new();
        internal void Block(string serial) => gates[serial] = new();
        internal Task Entered(string serial) => gates.GetOrAdd(serial, _ => new()).Entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
        internal void Release(string serial) => gates.GetOrAdd(serial, _ => new()).Released.TrySetResult();
        public IIdentityPinnedDeviceCommands BindIdentity(DeviceCatalog catalog) => new Pinned(this, inner.BindIdentity(catalog));
        private sealed class Pinned(JobBackend owner, IIdentityPinnedDeviceCommands commands) : IIdentityPinnedDeviceCommands
        {
            public bool IsDemo => commands.IsDemo;
            public async Task<string> StartAppAsync(DeviceCommandTarget target, string packageName, CancellationToken ct = default)
            {
                var gate = owner.gates.GetOrAdd(target.Serial, _ => new()); gate.Entered.TrySetResult();
                await gate.Released.Task.WaitAsync(ct);
                return await commands.StartAppAsync(target, packageName, ct);
            }
            public Task TapAsync(DeviceCommandTarget target, int x, int y, CancellationToken ct = default) => commands.TapAsync(target, x, y, ct);
            public Task SwipeAsync(DeviceCommandTarget target, int x1, int y1, int x2, int y2, int durationMs, CancellationToken ct = default) => commands.SwipeAsync(target, x1, y1, x2, y2, durationMs, ct);
        }
        public Task<IReadOnlyList<DeviceInfo>> DiscoverAsync(CancellationToken ct = default) => inner.DiscoverAsync(ct);
        public Task<string> ShellAsync(string serial, string command, bool useRoot = false, CancellationToken ct = default) => inner.ShellAsync(serial, command, useRoot, ct);
        public Task<byte[]> ScreenshotAsync(string serial, CancellationToken ct = default) => inner.ScreenshotAsync(serial, ct);
        public Task<AndroidNode> DumpAsync(string serial, CancellationToken ct = default) => inner.DumpAsync(serial, ct);
        public Task TapAsync(string serial, int x, int y, CancellationToken ct = default) => inner.TapAsync(serial, x, y, ct);
        public Task SwipeAsync(string serial, int x1, int y1, int x2, int y2, int durationMs, CancellationToken ct = default) => inner.SwipeAsync(serial, x1, y1, x2, y2, durationMs, ct);
        public Task KeyEventAsync(string serial, int code, CancellationToken ct = default) => inner.KeyEventAsync(serial, code, ct);
        public Task TextAsync(string serial, string text, CancellationToken ct = default) => inner.TextAsync(serial, text, ct);
        public Task<string> StartAppAsync(string serial, string packageName, string? activity = null, CancellationToken ct = default) => inner.StartAppAsync(serial, packageName, activity, ct);
        public Task<string> StopAppAsync(string serial, string packageName, CancellationToken ct = default) => inner.StopAppAsync(serial, packageName, ct);
        public Task<bool> ClearAppAsync(string serial, string packageName, CancellationToken ct = default) => inner.ClearAppAsync(serial, packageName, ct);
        public Task<IReadOnlyList<AppInfo>> ListAppsAsync(string serial, bool showSystem = false, bool needVersion = true, CancellationToken ct = default) => inner.ListAppsAsync(serial, showSystem, needVersion, ct);
        public Task InstallAsync(string serial, string localPath, CancellationToken ct = default) => inner.InstallAsync(serial, localPath, ct);
        public Task UninstallAsync(string serial, string packageName, CancellationToken ct = default) => inner.UninstallAsync(serial, packageName, ct);
        public Task PushAsync(string serial, string localPath, string remotePath, IProgress<double>? progress = null, CancellationToken ct = default) => inner.PushAsync(serial, localPath, remotePath, progress, ct);
        public Task PullAsync(string serial, string remotePath, string localPath, IProgress<double>? progress = null, CancellationToken ct = default) => inner.PullAsync(serial, remotePath, localPath, progress, ct);
        public Task SetNodeTextAsync(string serial, NodeSelector selector, string text, CancellationToken ct = default) => inner.SetNodeTextAsync(serial, selector, text, ct);
        public ValueTask DisposeAsync() => inner.DisposeAsync();
    }
}
