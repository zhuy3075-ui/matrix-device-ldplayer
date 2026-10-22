using System.ComponentModel;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using MatrixDevice.Core;
using MatrixDevice.Features;
using MatrixDevice.Host;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    private DebugAutomaticFixtureScope? debugAutomaticFixtureScope;
    private bool DebugAutomaticFixturePermitted => debugAutomaticFixtureScope?.Matches(this) == true;

    private async Task VerifyDebugAutomaticLoadingAsync()
    {
        if (!ViewModel.Demo || !ViewModel.Runtime.IsDemo || !Dispatcher.CheckAccess() || Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
            throw new InvalidOperationException("自动首载验收仅限一致Demo/STA。");
        var directory = Path.Combine(Path.GetFullPath(ViewModel.DataDirectory), "artifacts", "al", Guid.NewGuid().ToString("N")[..8]);
        if (!Path.IsPathFullyQualified(directory) || directory.Length > 190) throw new InvalidOperationException("自动首载验收需要短绝对artifacts路径：" + directory.Length);
        Directory.CreateDirectory(directory);
        var originalMain = Application.Current.MainWindow; var originalShutdown = Application.Current.ShutdownMode; var originalLanguage = LocalizationService.Current.Language;
        var reports = new List<object>(); var failures = new List<Exception>(); Exception? failure = null; var index = 0;
        try
        {
            await Case("default-demo-guard", false, async state =>
            {
                state.Navigate("节点"); await state.Idle();
                state.Check(state.Backend.Reads().Length == 0 && state.Vm.Nodes.Count == 0 && !state.Window.debugAutoLoading && !state.Window.debugAutoPending, "默认Demo真实导航仍早退，无自动设备读");
            });
            await Case("normal-and-existing-return", true, async state =>
            {
                var shot = state.Backend.Hold("Screenshot", state.A.Serial); state.Navigate("节点"); await state.Entered(shot); shot.Release.TrySetResult(); await state.Loaded(state.A.Serial);
                state.ReadSequence("Screenshot:" + state.A.Serial, "Dump:" + state.A.Serial);
                var root = state.Vm.Nodes.Single(); var selected = state.Vm.FlattenNodes().First(row => row.Source.Clickable); state.Vm.SelectedNode = selected;
                var count = state.Backend.Reads().Length; state.Navigate("设备墙 / 主控"); state.Navigate("节点"); await state.Idle();
                state.Check(state.Backend.Reads().Length == count && ReferenceEquals(root, state.Vm.Nodes.Single()) && ReferenceEquals(selected, state.Vm.SelectedNode), "已有节点真实离开返回不重载并保留根与选择");
            });
            foreach (var target in new[] { "a", "b", "aba" }) await Blocked("busy", target);
            await Blocked("external", "b"); await Blocked("transfer", "b"); await Blocked("script", "aba");
            await Case("dual-transfer-script", true, async state =>
            {
                var transfer = await state.Block("transfer"); var script = await state.Block("script"); state.Navigate("节点"); state.Vm.CurrentDevice = state.B; await state.Idle(); state.NoReads("双阻塞仍在等待");
                await transfer.Release(); await state.Idle();
                state.Check(!state.Vm.Transferring && state.Vm.Runtime.Scripts.GetStatus(state.A.Serial).State == ScriptRunState.Running && state.Vm.HasActiveOperations, "传输先完成但真实脚本仍Running"); state.NoReads("不得越过剩余脚本阻塞");
                var shot = state.Backend.Hold("Screenshot", state.B.Serial); await script.Release(); await state.Entered(shot); shot.Release.TrySetResult(); await state.Loaded(state.B.Serial);
                state.ReadSequence("Screenshot:" + state.B.Serial, "Dump:" + state.B.Serial);
            });
            foreach (var scenario in new[] { (Operation: "Screenshot", Aba: false), (Operation: "Dump", Aba: true) })
            {
                await Case("inflight-" + scenario.Operation + (scenario.Aba ? "-aba" : "-b"), true, async state =>
                {
                    var first = state.Backend.Hold(scenario.Operation, state.A.Serial); state.Navigate("节点"); await state.Entered(first);
                    state.Vm.CurrentDevice = state.B; if (scenario.Aba) state.Vm.CurrentDevice = state.A;
                    var final = state.Vm.CurrentDevice!; var second = state.Backend.Hold("Screenshot", final.Serial);
                    first.Release.TrySetResult(); await state.Entered(second); second.Release.TrySetResult(); await state.Loaded(final.Serial);
                    state.ReadSequence(scenario.Operation == "Screenshot" ? new[] { "Screenshot:" + state.A.Serial, "Screenshot:" + final.Serial, "Dump:" + final.Serial }
                        : new[] { "Screenshot:" + state.A.Serial, "Dump:" + state.A.Serial, "Screenshot:" + final.Serial, "Dump:" + final.Serial });
                    state.Check(state.Backend.MaximumActiveRead <= 1, "在途切换或ABA不并发两个自动首载读");
                });
            }
            await Case("waiting-leave-return", true, async state =>
            {
                var blocker = await state.Block("busy"); state.Navigate("节点"); state.Navigate("设备墙 / 主控"); await blocker.Release(); await state.Idle(); state.NoReads("等待中离页不新增首载");
                var shot = state.Backend.Hold("Screenshot", state.A.Serial); state.Navigate("节点"); await state.Entered(shot); shot.Release.TrySetResult(); await state.Loaded(state.A.Serial);
            });
            await Case("inflight-leave", true, async state =>
            {
                var shot = state.Backend.Hold("Screenshot", state.A.Serial); state.Navigate("节点"); await state.Entered(shot); state.Navigate("设备墙 / 主控"); shot.Release.TrySetResult(); await state.Settled();
                state.ReadSequence("Screenshot:" + state.A.Serial, "Dump:" + state.A.Serial);
                state.Check(Equals((state.Window.tabs.SelectedItem as TabItem)?.Tag, "设备墙 / 主控") && state.Vm.Nodes.Count > 0, "离页不伪称已取消在途合法读取，完成后不继续新增读取");
            });
            foreach (var cancel in new[] { false, true })
            {
                await Case(cancel ? "cancelled-no-loop" : "failed-no-loop", true, async state =>
                {
                    Exception injected = cancel ? new OperationCanceledException("controlled automatic stop") : new DeviceException("controlled automatic failure");
                    var gate = state.Backend.Hold(cancel ? "Dump" : "Screenshot", state.A.Serial, injected); state.Navigate("节点"); await state.Entered(gate); gate.Release.TrySetResult(); await state.Settled();
                    state.ReadSequence(cancel ? new[] { "Screenshot:" + state.A.Serial, "Dump:" + state.A.Serial } : new[] { "Screenshot:" + state.A.Serial });
                    state.Check(state.Vm.Nodes.Count == 0 && (cancel ? state.Vm.Error.Length == 0 && state.Vm.Status.Contains("已停止", StringComparison.Ordinal)
                        : state.Vm.Error == "刷新截图与节点失败：" + UiErrorPresentation.Describe(injected) && state.Vm.Status == "操作失败"), "失败或取消沿原安全反馈且不无限自动重试");
                    state.Navigate("设备墙 / 主控"); var next = state.Backend.Hold("Screenshot", state.A.Serial); state.Navigate("节点"); await state.Entered(next); next.Release.TrySetResult(); await state.Loaded(state.A.Serial);
                });
            }
            await Case("close-pending-script", true, async state =>
            {
                await state.Block("script"); state.Navigate("节点"); await state.Idle(); state.NoReads("真实脚本阻塞的待办在关闭前未读取");
                var confirmations = 0; state.Vm.Confirm = (_, _) => { confirmations++; return true; };
                state.Window.Close(); await state.Closed.Task.WaitAsync(TimeSpan.FromSeconds(5)); state.ShutdownByWindow = state.Window.closeApproved && state.Window.closing;
                state.Check(state.ShutdownByWindow && confirmations == 1 && state.Vm.Runtime.Scripts.GetStatus(state.A.Serial).State == ScriptRunState.Cancelled, "实际Window.Close/Closing确认并停止自有脚本，窗口单一Shutdown完成");
                state.NoReads("窗口关闭清理不再接纳pending首载");
            });
            if (failures.Count > 0) throw new AggregateException("自动首载独立用例失败", failures);
        }
        catch (Exception error) { failure = error; Console.Error.WriteLine(error); }
        finally
        {
            Application.Current.MainWindow = originalMain; Application.Current.ShutdownMode = originalShutdown; LocalizationService.Current.SetLanguage(originalLanguage);
            try { await File.WriteAllTextAsync(Path.Combine(directory, "automatic-loading.json"), JsonSerializer.Serialize(new { status = failure is null ? "PASS-OFFLINE-WPF-AUTOLOAD" : "FAIL", scope = "Own Demo windows; actual navigation, Current callback and VM activity; not physical input, Android, production timers, identity-expiry compensation or offline/reconnect", reports, error = failure?.ToString() }, JsonDefaults.Options)); }
            catch (Exception reportError) { Console.Error.WriteLine(reportError); failure = failure is null ? reportError : new AggregateException(failure, reportError); }
        }
        if (failure is not null) ExceptionDispatchInfo.Capture(failure).Throw();

        async Task Blocked(string kind, string target)
        {
            await Case("blocked-" + kind + "-" + target, true, async state =>
            {
                var blocker = await state.Block(kind); state.Navigate("节点");
                if (target != "a") state.Vm.CurrentDevice = state.B; if (target == "aba") state.Vm.CurrentDevice = state.A;
                var final = state.Vm.CurrentDevice!; await state.Idle(); state.NoReads("真实活动期间只保留最终目标意图");
                var shot = state.Backend.Hold("Screenshot", final.Serial); await blocker.Release(); await state.Entered(shot); shot.Release.TrySetResult(); await state.Loaded(final.Serial);
                state.ReadSequence("Screenshot:" + final.Serial, "Dump:" + final.Serial);
            });
        }
        async Task Case(string name, bool permit, Func<DebugAutomaticFixtureScope, Task> action)
        {
            DebugAutomaticFixtureScope? state = null; Exception? error = null; var cleaned = false;
            try { state = await DebugAutomaticFixtureScope.Create(this, Path.Combine(directory, "c" + (++index).ToString("00")), permit); await action(state); }
            catch (Exception primary) { error = primary; Console.Error.WriteLine(primary); }
            finally
            {
                if (state is not null)
                {
                    try { await state.Cleanup(); cleaned = true; }
                    catch (Exception cleanupError) { error = error is null ? cleanupError : new AggregateException(error, cleanupError); }
                }
                reports.Add(new { name, status = error is null ? "PASS" : "FAIL", cleaned, checks = state?.Checks, events = state?.Events.ToArray(), calls = state?.Backend.Calls(), maximumActiveRead = state?.Backend.MaximumActiveRead, error = error?.ToString() });
            }
            if (error is not null) { failures.Add(new InvalidOperationException(name, error)); if (!cleaned) ExceptionDispatchInfo.Capture(error).Throw(); }
        }
    }

    private sealed class DebugAutomaticFixtureScope
    {
        internal required MainWindow Window;
        internal required WorkspaceViewModel Vm;
        internal required AutomaticFixtureBackend Backend;
        internal required MatrixRuntime Runtime;
        internal required string Directory;
        internal required DeviceCatalog Catalog;
        internal required AutomationService Automation;
        internal required InputCoordinator Input;
        internal required ScriptScheduler Scripts;
        internal required ApiServiceManager Api;
        internal readonly List<string> Checks = [];
        internal readonly Queue<object> Events = [];
        internal readonly TaskCompletionSource Closed = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly List<Task> operations = [];
        private readonly List<TaskCompletionSource> releases = [];
        private PropertyChangedEventHandler? observer;
        private System.Collections.Specialized.NotifyCollectionChangedEventHandler? nodesObserver;
        private Action<ScriptRunStatus>? scriptObserver;
        internal bool ShutdownByWindow;
        internal DeviceRow A => Vm.Devices.Single(row => row.Serial == "DEMO-001");
        internal DeviceRow B => Vm.Devices.Single(row => row.Serial == "DEMO-002");

        internal bool Matches(MainWindow window) => ReferenceEquals(window, Window) && ReferenceEquals(window.ViewModel, Vm) && Vm.Demo && Runtime.IsDemo
            && ReferenceEquals(Vm.Runtime, Runtime) && ReferenceEquals(Runtime.Backend, Backend) && ReferenceEquals(Runtime.Devices, Catalog) && ReferenceEquals(Runtime.Automation, Automation)
            && ReferenceEquals(Runtime.Input, Input) && ReferenceEquals(Runtime.Scripts, Scripts) && ReferenceEquals(Runtime.Api, Api)
            && !Api.BasicRunning && !Api.AutomatorRunning && Api.DevicePorts.Count == 0 && Path.IsPathFullyQualified(Directory) && Directory.Length <= 200
            && Directory.Contains(Path.DirectorySeparatorChar + "artifacts" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
            && window.Dispatcher.CheckAccess() && Thread.CurrentThread.GetApartmentState() == ApartmentState.STA;

        internal static async Task<DebugAutomaticFixtureScope> Create(MainWindow owner, string directory, bool permit)
        {
            System.IO.Directory.CreateDirectory(directory);
            var backend = new AutomaticFixtureBackend(); RollingLog? log = null; CatalogRepository? metadata = null; MatrixRuntime? runtime = null; WorkspaceViewModel? vm = null; MainWindow? window = null; DebugAutomaticFixtureScope? scope = null;
            try
            {
                log = new RollingLog(Path.Combine(directory, "logs")); metadata = new CatalogRepository(Path.Combine(directory, "db.sqlite3"));
                var catalog = new DeviceCatalog(backend); var automation = new AutomationService(backend, catalog); var scripts = new ScriptScheduler(backend); var input = new InputCoordinator(catalog, backend); var api = new ApiServiceManager(catalog, backend, automation, log);
                var settings = new AppSettings { AutoStartApi = false, AutoConnect = false, SyncMouse = false, SyncKeyboard = false, Language = "zh-CN", WindowWidth = 1000, WindowHeight = 700 };
                runtime = new MatrixRuntime { Backend = backend, Devices = catalog, Automation = automation, Scripts = scripts, Input = input, Api = api, Log = log, Metadata = metadata, Settings = settings, FrozenConnectionSettings = settings, SettingsStore = new SettingsStore(Path.Combine(directory, "AppConfig.json")), IsDemo = true };
                api.DeviceSnapshotProvider = runtime.GetDevices; vm = new WorkspaceViewModel(runtime, directory, true);
                await vm.RefreshDevicesAsync().WaitAsync(TimeSpan.FromSeconds(5));
                window = new MainWindow(vm) { Owner = owner, ShowActivated = false };
                scope = new DebugAutomaticFixtureScope { Window = window, Vm = vm, Backend = backend, Runtime = runtime, Directory = directory, Catalog = catalog, Automation = automation, Input = input, Scripts = scripts, Api = api };
                scope.Check(scope.Matches(window) && TimersStopped(window) && vm.Devices.Count == 2 && vm.Devices.All(row => row.Info.Online), "许可只绑定一致自有Demo服务与短artifacts/STA，API未启且原Demo计时器停用");
                scope.observer = (_, args) => scope.Observe("property:" + args.PropertyName); vm.PropertyChanged += scope.observer;
                scope.nodesObserver = (_, args) => scope.Observe("nodes:" + args.Action); vm.Nodes.CollectionChanged += scope.nodesObserver;
                scope.scriptObserver = status => Application.Current.Dispatcher.BeginInvoke(() => scope.Observe("script:" + status.State)); scripts.StatusChanged += scope.scriptObserver;
                window.Closed += (_, _) => scope.Closed.TrySetResult();
                if (permit) window.debugAutomaticFixtureScope = scope;
                backend.ClearCalls(); window.Show(); await scope.Idle(); scope.Observe("created"); return scope;
            }
            catch
            {
                if (scope is not null) await scope.Cleanup();
                else if (vm is not null) { window?.AbortStartup(); await vm.ShutdownAsync(); }
                else if (runtime is not null) await runtime.DisposeAsync();
                else { metadata?.Dispose(); if (log is not null) await log.DisposeAsync(); await backend.DisposeAsync(); }
                throw;
            }
        }
        private static bool TimersStopped(MainWindow window)
        {
            var field = typeof(DispatcherTimerAdapter).GetField("timer", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic) ?? throw new InvalidOperationException("已核timer字段不存在");
            if (field.FieldType != typeof(DispatcherTimer)) throw new InvalidOperationException("已核timer类型变化");
            return field.GetValue(window.captureTimer) is DispatcherTimer { IsEnabled: false } && field.GetValue(window.directoryTimer) is DispatcherTimer { IsEnabled: false };
        }
        internal void Check(bool condition, string message) { if (!condition) { Observe("failed:" + message); throw new InvalidOperationException(message); } Checks.Add(message); }
        internal void Observe(string stage)
        {
            if (Events.Count == 128) Events.Dequeue();
            Events.Enqueue(new { stage, timestamp = DateTimeOffset.UtcNow, current = Vm.CurrentDevice?.Serial, Vm.Busy, Vm.Transferring, Vm.HasActiveOperations, Vm.Status, Vm.Error,
                loading = Window.debugAutoLoading, pending = Window.debugAutoPending, Window.closing, Window.closeApproved, permit = Window.DebugAutomaticFixturePermitted,
                page = (Window.tabs.SelectedItem as TabItem)?.Tag as string, nodes = Vm.Nodes.Count, tree = Window.nodeTree.Items.Count, canvas = Window.nodeCanvas.Children.Count, rectangles = Window.nodeRectangles.Count,
                debugWidth = Vm.DebugImage?.PixelWidth, debugHeight = Vm.DebugImage?.PixelHeight, scriptA = Scripts.GetStatus("DEMO-001").State.ToString(), readCalls = Backend.Reads().Length });
        }
        internal void Navigate(string tag)
        {
            Observe("before-navigation:" + tag); Window.navigation[tag].RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            Check(Equals((Window.tabs.SelectedItem as TabItem)?.Tag, tag), "实际导航Click选择原Tab：" + tag); Observe("after-navigation:" + tag);
        }
        internal async Task Idle()
        {
            await Window.Dispatcher.InvokeAsync(Window.UpdateLayout, DispatcherPriority.ApplicationIdle).Task.WaitAsync(TimeSpan.FromSeconds(5));
            await Window.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ContextIdle).Task.WaitAsync(TimeSpan.FromSeconds(5));
        }
        internal async Task Settled()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            do { await Idle(); if (!Window.debugAutoLoading && !Vm.Busy) { await Idle(); if (!Window.debugAutoLoading && !Vm.Busy) { Observe("settled"); return; } } await Task.Delay(5); } while (watch.Elapsed < TimeSpan.FromSeconds(5));
            Observe("settle-timeout"); throw new TimeoutException("自动首载未在5秒内排空；未重试设备操作。");
        }
        internal async Task Entered(AutomaticGate gate) { await gate.Entered.Task.WaitAsync(TimeSpan.FromSeconds(5)); Observe("gate-entered:" + gate.Operation + ":" + gate.Serial); Check(gate.Claimed, "真实后端调用已进入TCS门：" + gate.Operation); }
        internal async Task Loaded(string serial)
        {
            await Settled(); var row = Vm.Devices.Single(row => row.Serial == serial);
            Check(Vm.CurrentDevice?.Serial == serial && Vm.Nodes.Count > 0 && ReferenceEquals(Vm.DebugImage, row.Screenshot) && Vm.Error.Length == 0
                && Window.nodeTree.Items.Count == Vm.Nodes.Count && Window.nodeTreeItems.Count == Vm.FlattenNodes().Count() && Vm.FlattenNodes().All(node => Window.nodeTreeItems.ContainsKey(node))
                && Window.nodeCanvas.Children.OfType<Image>().Any(image => ReferenceEquals(image.Source, Vm.DebugImage)) && Window.nodeRectangles.Count > 0,
                "实际自动调度建立最终目标的VM节点、真实Tree与Canvas：" + serial);
        }
        internal void NoReads(string reason) => Check(Backend.Reads().Length == 0, reason + "; 实际reads=" + string.Join(",", Backend.Reads().Select(call => call.Operation + ":" + call.Serial)));
        internal void ReadSequence(params string[] expected) => Check(Backend.Reads().Select(call => call.Operation + ":" + call.Serial).SequenceEqual(expected), "真实读取顺序 expected=" + string.Join(",", expected) + "; actual=" + string.Join(",", Backend.Reads().Select(call => call.Operation + ":" + call.Serial)));
        internal async Task<AutomaticBlocker> Block(string kind)
        {
            Task pending; TaskCompletionSource? release = null; AutomaticGate? gate = null;
            switch (kind)
            {
                case "busy":
                    release = new(TaskCreationOptions.RunContinuationsAsynchronously); releases.Add(release); var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                    pending = Vm.ExecuteAsync("自有阻塞", async () => { entered.TrySetResult(); await release.Task; }); await entered.Task.WaitAsync(TimeSpan.FromSeconds(5)); break;
                case "external": Vm.SetExternalBusy(true); pending = Task.CompletedTask; break;
                case "transfer":
                    var local = Path.Combine(Directory, "own.txt"); await File.WriteAllTextAsync(local, "own Demo transfer"); Vm.QueuePush(local, false); gate = Backend.Hold("Push", A.Serial); pending = Vm.StartTransfersAsync(); await Entered(gate); break;
                case "script":
                    gate = Backend.Hold("StartApp", A.Serial); pending = Scripts.RunAsync(A.Serial, new ScriptPackage { Manifest = new() { Id = "own-auto", Name = "自有自动首载阻塞", Version = "1.0" }, Steps = [new() { Action = "launch", Value = "com.matrix.demo" }] }); await Entered(gate); break;
                default: throw new ArgumentOutOfRangeException(nameof(kind), kind, "未知阻塞");
            }
            operations.Add(pending); Check(Vm.HasActiveOperations, "真实活动阻塞已成立：" + kind); Observe("blocked:" + kind);
            return new AutomaticBlocker(async () => { if (kind == "external") Vm.SetExternalBusy(false); release?.TrySetResult(); gate?.Release.TrySetResult(); await pending.WaitAsync(TimeSpan.FromSeconds(5)); Observe("released:" + kind); });
        }
        internal async Task Cleanup()
        {
            Backend.ReleaseAll(); foreach (var release in releases) release.TrySetResult(); Vm.SetExternalBusy(false);
            await Task.WhenAll(operations).WaitAsync(TimeSpan.FromSeconds(5));
            if (ShutdownByWindow) Check(!Window.debugAutoLoading && !Vm.Busy, "真实关闭后无在途自动读与Busy"); else await Settled();
            Check(Backend.GatesComplete && TimersStopped(Window), "finally全部自有TCS与活动排空，Demo计时器仍停用");
            Vm.PropertyChanged -= observer; Vm.Nodes.CollectionChanged -= nodesObserver; Scripts.StatusChanged -= scriptObserver;
            Window.debugAutomaticFixtureScope = null;
            if (!ShutdownByWindow) { Window.AbortStartup(); await Vm.ShutdownAsync().WaitAsync(TimeSpan.FromSeconds(5)); }
            Observe("cleanup-complete");
        }
    }
    private sealed class AutomaticBlocker(Func<Task> release) { internal Task Release() => release(); }
    private sealed record AutomaticCall(int Sequence, string Operation, string? Serial);
    private sealed class AutomaticGate(string operation, string serial, Exception? failure)
    {
        internal readonly string Operation = operation;
        internal readonly string Serial = serial;
        internal readonly Exception? Failure = failure;
        internal bool Claimed;
        internal readonly TaskCompletionSource Entered = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal readonly TaskCompletionSource Release = new(TaskCreationOptions.RunContinuationsAsynchronously);
    }
    private sealed class AutomaticFixtureBackend : IDeviceBackend
    {
        private readonly DemoDeviceBackend inner = new();
        private readonly object sync = new();
        private readonly List<AutomaticCall> calls = [];
        private readonly List<AutomaticGate> gates = [];
        private int activeRead;
        internal int MaximumActiveRead { get; private set; }
        internal AutomaticCall[] Calls() { lock (sync) return calls.ToArray(); }
        internal AutomaticCall[] Reads() => Calls().Where(call => call.Operation is "Screenshot" or "Dump").ToArray();
        internal bool GatesComplete { get { lock (sync) return gates.All(gate => gate.Entered.Task.IsCompleted && gate.Release.Task.IsCompleted); } }
        internal void ClearCalls() { lock (sync) calls.Clear(); }
        internal AutomaticGate Hold(string operation, string serial, Exception? failure = null) { var gate = new AutomaticGate(operation, serial, failure); lock (sync) gates.Add(gate); return gate; }
        internal void ReleaseAll() { lock (sync) foreach (var gate in gates) { gate.Release.TrySetResult(); if (!gate.Claimed) gate.Entered.TrySetCanceled(); } }
        private async Task<T> Run<T>(string operation, string? serial, Func<Task<T>> action, CancellationToken ct)
        {
            AutomaticGate? gate; var read = operation is "Screenshot" or "Dump";
            lock (sync) { calls.Add(new(calls.Count + 1, operation, serial)); gate = gates.FirstOrDefault(item => !item.Claimed && item.Operation == operation && item.Serial == serial); if (gate is not null) gate.Claimed = true; if (read) { activeRead++; MaximumActiveRead = Math.Max(MaximumActiveRead, activeRead); } }
            try { var result = await action(); if (gate is not null) { gate.Entered.TrySetResult(); await gate.Release.Task.WaitAsync(ct); if (gate.Failure is not null) ExceptionDispatchInfo.Capture(gate.Failure).Throw(); } return result; }
            finally { if (read) lock (sync) activeRead--; }
        }
        public async Task<IReadOnlyList<DeviceInfo>> DiscoverAsync(CancellationToken ct = default) => await Run("Discover", null, () => inner.DiscoverAsync(ct), ct);
        public Task<byte[]> ScreenshotAsync(string serial, CancellationToken ct = default) => Run("Screenshot", serial, () => inner.ScreenshotAsync(serial, ct), ct);
        public Task<AndroidNode> DumpAsync(string serial, CancellationToken ct = default) => Run("Dump", serial, () => inner.DumpAsync(serial, ct), ct);
        public Task<string> StartAppAsync(string serial, string packageName, string? activity = null, CancellationToken ct = default) => Run("StartApp", serial, () => inner.StartAppAsync(serial, packageName, activity, ct), ct);
        public async Task PushAsync(string serial, string localPath, string remotePath, IProgress<double>? progress = null, CancellationToken ct = default) => await Run("Push", serial, async () => { await inner.PushAsync(serial, localPath, remotePath, progress, ct); return true; }, ct);
        public Task<string> ShellAsync(string serial, string command, bool useRoot = false, CancellationToken ct = default) => inner.ShellAsync(serial, command, useRoot, ct);
        public Task TapAsync(string serial, int x, int y, CancellationToken ct = default) => inner.TapAsync(serial, x, y, ct);
        public Task SwipeAsync(string serial, int x1, int y1, int x2, int y2, int durationMs, CancellationToken ct = default) => inner.SwipeAsync(serial, x1, y1, x2, y2, durationMs, ct);
        public Task KeyEventAsync(string serial, int code, CancellationToken ct = default) => inner.KeyEventAsync(serial, code, ct);
        public Task TextAsync(string serial, string text, CancellationToken ct = default) => inner.TextAsync(serial, text, ct);
        public Task<string> StopAppAsync(string serial, string packageName, CancellationToken ct = default) => inner.StopAppAsync(serial, packageName, ct);
        public Task<bool> ClearAppAsync(string serial, string packageName, CancellationToken ct = default) => inner.ClearAppAsync(serial, packageName, ct);
        public Task<IReadOnlyList<AppInfo>> ListAppsAsync(string serial, bool showSystem = false, bool needVersion = true, CancellationToken ct = default) => inner.ListAppsAsync(serial, showSystem, needVersion, ct);
        public Task InstallAsync(string serial, string localPath, CancellationToken ct = default) => inner.InstallAsync(serial, localPath, ct);
        public Task UninstallAsync(string serial, string packageName, CancellationToken ct = default) => inner.UninstallAsync(serial, packageName, ct);
        public Task PullAsync(string serial, string remotePath, string localPath, IProgress<double>? progress = null, CancellationToken ct = default) => inner.PullAsync(serial, remotePath, localPath, progress, ct);
        public Task SetNodeTextAsync(string serial, NodeSelector selector, string text, CancellationToken ct = default) => inner.SetNodeTextAsync(serial, selector, text, ct);
        public ValueTask DisposeAsync() { ReleaseAll(); return inner.DisposeAsync(); }
    }
}
