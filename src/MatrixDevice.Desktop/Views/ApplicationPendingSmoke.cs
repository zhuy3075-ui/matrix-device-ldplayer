using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Threading;
using MatrixDevice.Core;
using MatrixDevice.Features;
using MatrixDevice.Host;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    private async Task VerifyApplicationPendingAsync()
    {
        if (!ViewModel.Demo || !ViewModel.Runtime.IsDemo || !Dispatcher.CheckAccess() || Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
            throw new InvalidOperationException("应用挂起读取验收仅限自有Demo/STA");
        var directory = Path.Combine(Path.GetFullPath(ViewModel.DataDirectory), "artifacts", "ap", Guid.NewGuid().ToString("N")[..8]);
        if (directory.Length > 190 || !Path.IsPathFullyQualified(directory)) throw new InvalidOperationException("应用挂起读取验收需要短绝对artifacts目录");
        Directory.CreateDirectory(directory);
        var language = LocalizationService.Current.Language;
        var reports = new List<object>(); var failures = new List<Exception>(); var number = 0; Exception? failure = null;
        try
        {
            await Case("normal-once-global-b-application-a", async state =>
            {
                state.Expect("script-b-entered", "select", "tick", "list", "release-b", "no-duplicate");
                state.Begin("script-b-entered"); var script = await state.BlockScript(state.B.Serial); state.Complete("script-b-entered");
                var read = state.Backend.Hold("List", state.A.Serial); var before = state.TickCount;
                state.Begin("select"); await state.Select(state.A.Serial, true); state.Complete("select");
                state.Begin("tick"); await state.TickAfter(before); state.Complete("tick");
                state.Begin("list"); await state.ReadEntered(read); state.RequireScriptRunning(state.B.Serial); await state.CompleteRead(read); state.AssertLists(state.A.Serial); state.Complete("list");
                state.Begin("release-b"); await state.ReleaseScript(script, "completed"); state.Complete("release-b");
                state.Begin("no-duplicate"); await state.ObservationWindow(); state.AssertLists(state.A.Serial); state.Product(!state.Window.applicationReadPending, "正常读取消费意图"); state.Complete("no-duplicate");
            });
            foreach (var outcome in new[] { "completed", "failed", "cancelled" }) await Case("script-" + outcome, async state =>
            {
                state.Expect("script-entered", "select", "blocked-tick", "terminal", "list");
                state.Begin("script-entered"); var script = await state.BlockScript(state.A.Serial, outcome == "failed"); state.Complete("script-entered");
                var read = state.Backend.Hold("List", state.A.Serial); var before = state.TickCount;
                state.Begin("select"); await state.Select(state.A.Serial, true); state.Complete("select");
                state.Begin("blocked-tick"); var tick = await state.TickAfter(before); state.RequireBlockedTick(tick, script: true); state.Complete("blocked-tick");
                state.Begin("terminal"); await state.ReleaseScript(script, outcome); state.Complete("terminal");
                state.Begin("list"); await state.ReadEntered(read); await state.CompleteRead(read); state.AssertLists(state.A.Serial); state.Product(!state.Window.applicationReadPending, "脚本真实终态后一次读取消费原意图"); state.Complete("list");
            });
            await Case("stable-push", async state =>
            {
                state.Expect("push-entered", "select", "blocked-tick", "transfer-finished", "list");
                state.Begin("push-entered"); var push = await state.BlockPush(); state.Complete("push-entered");
                var read = state.Backend.Hold("List", state.A.Serial); var before = state.TickCount;
                state.Begin("select"); await state.Select(state.A.Serial, true); state.Complete("select");
                state.Begin("blocked-tick"); var tick = await state.TickAfter(before); state.RequireBlockedTick(tick, transfer: true); state.Require(state.OverallProgressNotices == push.ProgressNotices, "Push门期间没有VM进度通知重置debounce"); state.Complete("blocked-tick");
                state.Begin("transfer-finished"); await state.ReleasePush(push); state.Complete("transfer-finished");
                state.Begin("list"); await state.ReadEntered(read); await state.CompleteRead(read); state.AssertLists(state.A.Serial); state.Complete("list");
            });
            await Case("busy-before-tick", async state =>
            {
                state.Expect("select", "busy-entered", "blocked-tick", "busy-finished", "list");
                var read = state.Backend.Hold("List", state.A.Serial); var before = state.TickCount;
                state.Begin("select"); await state.Select(state.A.Serial, true, settle: false); state.Complete("select");
                state.Begin("busy-entered"); var busy = await state.BlockBusy(); state.Complete("busy-entered");
                state.Begin("blocked-tick"); var tick = await state.TickAfter(before); state.RequireBlockedTick(tick, busy: true); state.Complete("blocked-tick");
                state.Begin("busy-finished"); busy.Release.TrySetResult(); await state.Finish(busy.Task); state.Complete("busy-finished");
                state.Begin("list"); await state.ReadEntered(read); await state.CompleteRead(read); state.AssertLists(state.A.Serial); state.Complete("list");
            });
            await Case("script-and-push-release-separately", async state =>
            {
                state.Expect("blockers", "select", "blocked-tick", "one-released", "both-released", "list");
                state.Begin("blockers"); var script = await state.BlockScript(state.A.Serial); var push = await state.BlockPush(); state.Complete("blockers");
                var read = state.Backend.Hold("List", state.A.Serial); var before = state.TickCount;
                state.Begin("select"); await state.Select(state.A.Serial, true); state.Complete("select");
                state.Begin("blocked-tick"); var tick = await state.TickAfter(before); state.RequireBlockedTick(tick, script: true, transfer: true); state.Complete("blocked-tick");
                state.Begin("one-released"); await state.ReleaseScript(script, "completed"); await state.ObservationWindow(); state.Product(state.Vm.Transferring && state.Backend.Lists().Length == 0, "只释放脚本时传输仍阻止自动读取"); state.Complete("one-released");
                state.Begin("both-released"); await state.ReleasePush(push); state.Complete("both-released");
                state.Begin("list"); await state.ReadEntered(read); await state.CompleteRead(read); state.AssertLists(state.A.Serial); state.Complete("list");
            });
            await Case("latest-selection", async state =>
            {
                state.Expect("blockers", "select-a", "blocked-a-tick", "select-b", "blocked-b-tick", "release-push", "list-b-while-a-running", "release-a");
                state.Begin("blockers"); var script = await state.BlockScript(state.A.Serial); var push = await state.BlockPush(); state.Complete("blockers");
                var before = state.TickCount;
                state.Begin("select-a"); await state.Select(state.A.Serial, true); state.Complete("select-a");
                state.Begin("blocked-a-tick"); var tick = await state.TickAfter(before); state.RequireBlockedTick(tick, script: true, transfer: true); state.Complete("blocked-a-tick");
                var read = state.Backend.Hold("List", state.B.Serial);
                before = state.TickCount;
                state.Begin("select-b"); await state.Select(state.A.Serial, false); await state.Select(state.B.Serial, true); state.Complete("select-b");
                state.Begin("blocked-b-tick"); tick = await state.TickAfter(before); state.RequireBlockedTick(tick, script: true, transfer: true); state.Require(state.OverallProgressNotices == push.ProgressNotices, "共同Push阻塞期间没有进度通知掩盖最终选择debounce"); state.Complete("blocked-b-tick");
                state.Begin("release-push"); await state.ReleasePush(push); state.RequireScriptRunning(state.A.Serial); state.Complete("release-push");
                state.Begin("list-b-while-a-running"); await state.ReadEntered(read); state.RequireScriptRunning(state.A.Serial); await state.CompleteRead(read); state.AssertLists(state.B.Serial); state.Complete("list-b-while-a-running");
                state.Begin("release-a"); await state.ReleaseScript(script, "completed"); await state.ObservationWindow(); state.AssertLists(state.B.Serial); state.Complete("release-a");
            });
            await Case("clear-selection", async state =>
            {
                state.Expect("script-entered", "select", "blocked-tick", "clear", "release", "no-list");
                state.Begin("script-entered"); var script = await state.BlockScript(state.A.Serial); state.Complete("script-entered"); var before = state.TickCount;
                state.Begin("select"); await state.Select(state.A.Serial, true); state.Complete("select");
                state.Begin("blocked-tick"); var tick = await state.TickAfter(before); state.RequireBlockedTick(tick, script: true); state.Complete("blocked-tick");
                state.Begin("clear"); await state.Select(state.A.Serial, false); state.Complete("clear");
                state.Begin("release"); await state.ReleaseScript(script, "completed"); state.Complete("release");
                state.Begin("no-list"); await state.ObservationWindow(); state.AssertLists(); state.Product(!state.Window.applicationReadPending && !state.Window.applicationRunning && !state.Window.applicationReadTimer!.IsEnabled, "全部清空后没有待执行读取或活动debounce"); state.Complete("no-list");
            });
            await Case("leave-return", async state =>
            {
                state.Expect("script-entered", "select", "blocked-tick", "leave", "release-hidden", "return-list");
                state.Begin("script-entered"); var script = await state.BlockScript(state.A.Serial); state.Complete("script-entered"); var before = state.TickCount;
                state.Begin("select"); await state.Select(state.A.Serial, true); state.Complete("select");
                state.Begin("blocked-tick"); var tick = await state.TickAfter(before); state.RequireBlockedTick(tick, script: true); state.Complete("blocked-tick");
                state.Begin("leave"); await state.Navigate("设备墙 / 主控"); state.Complete("leave");
                state.Begin("release-hidden"); await state.ReleaseScript(script, "completed"); await state.ObservationWindow(); state.Product(state.Backend.Lists().Length == 0, "离开应用页期间不自动读取"); state.Complete("release-hidden");
                var read = state.Backend.Hold("List", state.A.Serial);
                state.Begin("return-list"); await state.Navigate("应用"); await state.ReadEntered(read); await state.CompleteRead(read); state.AssertLists(state.A.Serial); state.Complete("return-list");
            });
            await Case("close-pending-script", async state =>
            {
                state.Expect("script-entered", "select", "blocked-tick", "close", "no-post-close-list");
                state.Begin("script-entered"); var script = await state.BlockScript(state.A.Serial); state.Complete("script-entered"); var before = state.TickCount;
                state.Begin("select"); await state.Select(state.A.Serial, true); state.Complete("select");
                state.Begin("blocked-tick"); var tick = await state.TickAfter(before); state.RequireBlockedTick(tick, script: true); state.Complete("blocked-tick");
                state.Begin("close"); await state.CloseByUser(); await state.Finish(script.Task); state.Product(state.ScriptStates(state.A.Serial).SequenceEqual(new[] { ScriptRunState.Running, ScriptRunState.Cancelling, ScriptRunState.Cancelled }), "真实Close经Shutdown取消脚本并通知终态"); state.Complete("close");
                state.Begin("no-post-close-list"); await state.ObservationWindow(); state.AssertLists(); state.Product(!state.Window.applicationRunning && !state.Vm.Busy && state.Window.applicationReadTimer?.IsEnabled != true, "关闭后释放通知没有设备读取或活动timer"); state.Complete("no-post-close-list");
            });
            await Case("list-failure-no-loop-and-new-intent", async state =>
            {
                state.Expect("select", "failed-list", "no-loop", "new-intent", "recovered-list");
                var read = state.Backend.Hold("List", state.A.Serial, new InvalidOperationException("own pending list failure")); var before = state.TickCount;
                state.Begin("select"); await state.Select(state.A.Serial, true); await state.TickAfter(before); state.Complete("select");
                state.Begin("failed-list"); await state.ReadEntered(read); await state.CompleteRead(read, failed: true, observeOnce: false); state.AssertLists(state.A.Serial); state.Complete("failed-list");
                state.Begin("no-loop"); await state.ObservationWindow(); state.AssertLists(state.A.Serial); state.Product(!state.Window.applicationReadPending && !state.Window.applicationReadTimer!.IsEnabled, "失败List不自动重试"); state.Complete("no-loop");
                read = state.Backend.Hold("List", state.A.Serial);
                state.Begin("new-intent"); await state.Select(state.A.Serial, false); await state.Select(state.A.Serial, true); state.Complete("new-intent");
                state.Begin("recovered-list"); await state.ReadEntered(read); await state.CompleteRead(read); state.AssertLists(state.A.Serial, state.A.Serial); state.Complete("recovered-list");
            });
            if (failures.Count != 0) throw new AggregateException("应用挂起读取专项失败；原全部旧专项/应用身份/12页均NOT-RUN", failures);
        }
        catch (Exception error) { failure = error; Console.Error.WriteLine("APP-PENDING-PRIMARY " + error); }
        finally
        {
            try { LocalizationService.Current.SetLanguage(language); } catch (Exception error) { failure = failure is null ? error : new AggregateException(failure, error); }
            try
            {
                await File.WriteAllTextAsync(Path.Combine(directory, "application-pending.json"), JsonSerializer.Serialize(new
                {
                    status = failure is null ? "PASS-OFFLINE-APPLICATION-PENDING" : "FAIL", plannedCaseCount = 12, caseCount = reports.Count, reports, error = failure?.ToString(),
                    notRunCases = new[] { "normal-once-global-b-application-a", "script-completed", "script-failed", "script-cancelled", "stable-push", "busy-before-tick", "script-and-push-release-separately", "latest-selection", "clear-selection", "leave-return", "close-pending-script", "list-failure-no-loop-and-new-intent" }.Skip(reports.Count).ToArray(),
                    scope = "Own consistent Demo/STA windows; actual application target CheckBox automation and TwoWay binding, original 350ms timer and post-handler Tick observer. Logical WPF automation, not physical input. Real ScriptScheduler Run/Stop and VM Push/Busy tasks; only own stub backend gates, no guest forwarding. Automatic app path may only Discover/List. API stopped. No API-release/layout/Android/ADB/physical-input/native-dialog/runtime-hot-swap coverage. Timer callback is original async void; its actual backend Tasks, UI completion events and all fixture Tasks/TCS are tracked."
                }, JsonDefaults.Options));
            }
            catch (Exception error) { Console.Error.WriteLine(error); failure = failure is null ? error : new AggregateException(failure, error); }
        }
        if (failure is not null) ExceptionDispatchInfo.Capture(failure).Throw();

        async Task Case(string name, Func<AppPendingState, Task> action)
        {
            var state = new AppPendingState(); Exception? error = null; Exception? writeError = null; Exception? cleanupError = null; state.Expect("case-action");
            try { await state.Initialize(this, Path.Combine(directory, "c" + (++number).ToString("00"))); state.Begin("case-action"); await action(state); state.Context(); state.Complete("case-action"); }
            catch (Exception caught) { error = caught; Console.Error.WriteLine("APP-PENDING-CASE " + name + " " + caught); }
            finally
            {
                state.FailActiveStage(error);
                if (state.Backend is not null) try { state.NoApplicationWrites(); } catch (Exception caught) { writeError = caught; }
                try { await state.Cleanup(); } catch (Exception caught) { cleanupError = caught; Console.Error.WriteLine("APP-PENDING-CLEANUP " + name + " " + caught); }
                var classification = cleanupError is not null ? "CLEANUP-FAIL" : writeError is not null ? writeError is AppPendingProductFailure ? "PRODUCT-FAIL" : "HARNESS-FAIL" : error switch { null => "PASS", AppPendingPreconditionFailure => "PRECONDITION-FAIL", AppPendingProductFailure => "PRODUCT-FAIL", _ => "HARNESS-FAIL" };
                reports.Add(new { name, status = classification, cleaned = cleanupError is null, checks = state.Checks.ToArray(), observations = state.Events.ToArray(), stages = state.StageReport(), ticks = state.Ticks.ToArray(), scriptEvents = state.ScriptEvents.ToArray(), calls = state.Backend?.Calls(), gates = state.Backend?.GateReport(), tasks = state.TaskReport(), failure = error?.ToString(), forbiddenOperationFailure = writeError?.ToString(), cleanupFailure = cleanupError?.ToString() });
            }
            var caseErrors = new[] { error, writeError, cleanupError }.OfType<Exception>().ToArray(); if (caseErrors.Length != 0) failures.Add(new InvalidOperationException(name, new AggregateException(caseErrors)));
            if (cleanupError is not null) throw new AggregateException("自有case未清理，停止后续接纳；余case NOT-RUN", failures);
        }
    }

    private sealed class AppPendingPreconditionFailure(string message, Exception? inner = null) : Exception(message, inner) { }
    private sealed class AppPendingProductFailure(string message, Exception? inner = null) : Exception(message, inner) { }
    private sealed class AppPendingTracked(string label, Task task, Exception? expectedFailure = null, bool allowCancellation = false)
    {
        internal readonly string Label = label; internal readonly Task Task = task; internal readonly Exception? ExpectedFailure = expectedFailure;
        internal readonly bool AllowCancellation = allowCancellation; internal bool FailureObserved;
        internal async Task Drain()
        {
            try { await Task.WaitAsync(TimeSpan.FromSeconds(5)); }
            catch (Exception error) when (ReferenceEquals(error, ExpectedFailure) && Task.IsFaulted) { FailureObserved = true; }
            catch (OperationCanceledException) when (AllowCancellation && Task.IsCanceled) { FailureObserved = true; }
        }
        internal object Report() => new { Label, expectedFailure = ExpectedFailure?.Message, AllowCancellation, FailureObserved, status = Task.Status.ToString(), Task.IsCompleted };
    }
    private sealed record AppPendingTick(int Number, long At, bool Pending, bool Running, bool Busy, bool Transferring, bool Visible, bool Closing, ScriptRunState ScriptA, int Lists);
    private sealed record AppPendingWait(Func<bool> Ready, TaskCompletionSource Completion);
    private sealed record AppPendingBlock(AppPendingGate Gate, Task Task, int ProgressNotices = 0);
    private sealed record AppPendingBusy(TaskCompletionSource Release, Task Task);

    private sealed class AppPendingState
    {
        internal MainWindow Window = null!; internal WorkspaceViewModel Vm = null!; internal AppPendingBackend Backend = null!;
        internal readonly List<string> Checks = []; internal readonly List<object> Events = []; internal readonly List<AppPendingTick> Ticks = [];
        internal readonly ConcurrentQueue<ScriptRunStatus> ScriptEvents = new();
        private readonly List<AppPendingTracked> tasks = []; private readonly List<TaskCompletionSource> releases = []; private readonly List<AppPendingWait> waits = [];
        private readonly List<DispatcherTimer> observationTimers = []; private readonly Dictionary<string, string> stages = [];
        private PropertyChangedEventHandler? propertyObserver; private Action<ScriptRunStatus>? scriptObserver; private EventHandler? tickObserver;
        private DependencyPropertyChangedEventHandler? enabledObserver; private EventHandler? closedObserver;
        private MatrixRuntime? runtime; private CatalogRepository? metadata; private RollingLog? log; private DeviceCatalog? catalog;
        private AutomationService? automation; private InputCoordinator? input; private ScriptScheduler? scripts; private ApiServiceManager? api;
        private string directory = ""; private bool shutdownByWindow; private bool closeRequestedByUser; private bool cleanupStarted;
        private int activeOperationNotices; private int transferNotices; private int overallProgressNotices;
        private DeviceRow? current; private DeviceRow? main; private DeviceRow[] rows = []; private bool[] selected = []; private DeviceRow[] targets = []; private UnifiedDeviceRow[] unified = []; private bool[] unifiedSelected = [];
        private readonly TaskCompletionSource closed = NewCompletion();
        internal DeviceRow A => Vm.Devices.Single(row => row.Serial == "DEMO-001"); internal DeviceRow B => Vm.Devices.Single(row => row.Serial == "DEMO-002");
        internal int TickCount => Ticks.Count;
        internal int OverallProgressNotices => overallProgressNotices;
        private static TaskCompletionSource NewCompletion() => new(TaskCreationOptions.RunContinuationsAsynchronously);

        internal async Task Initialize(MainWindow owner, string path)
        {
            stages.Add("initialize", "STARTED"); directory = path; Directory.CreateDirectory(path);
            try
            {
                Backend = new(); log = new RollingLog(Path.Combine(path, "logs")); metadata = new CatalogRepository(Path.Combine(path, "db.sqlite3"));
                catalog = new DeviceCatalog(Backend); automation = new AutomationService(Backend, catalog); scripts = new ScriptScheduler(Backend); input = new InputCoordinator(catalog, Backend); api = new ApiServiceManager(catalog, Backend, automation, log);
                var settings = new AppSettings { Language = "zh-CN", AutoStartApi = false, AutoConnect = false, SyncMouse = false, SyncKeyboard = false, WindowWidth = 1000, WindowHeight = 700 };
                runtime = new MatrixRuntime { Backend = Backend, Devices = catalog, Automation = automation, Scripts = scripts, Input = input, Api = api, Metadata = metadata, Log = log, Settings = settings, FrozenConnectionSettings = settings, SettingsStore = new SettingsStore(Path.Combine(path, "AppConfig.json")), IsDemo = true };
                api.DeviceSnapshotProvider = runtime.GetDevices; Backend.Snapshot = runtime.GetDevices; Vm = new WorkspaceViewModel(runtime, path, true);
                await Track("initial-discovery", Vm.RefreshDevicesAsync()).WaitAsync(TimeSpan.FromSeconds(5));
                Window = new MainWindow(Vm) { Owner = owner, ShowActivated = false }; Window.captureTimer.Stop(); Window.directoryTimer.Stop(); Window.monitorTimer.Stop();
                propertyObserver = (_, args) => { if (args.PropertyName == nameof(WorkspaceViewModel.HasActiveOperations)) activeOperationNotices++; if (args.PropertyName == nameof(WorkspaceViewModel.Transferring)) transferNotices++; if (args.PropertyName == nameof(WorkspaceViewModel.OverallProgress)) overallProgressNotices++; Observe("vm:" + args.PropertyName); }; Vm.PropertyChanged += propertyObserver;
                scriptObserver = status => { ScriptEvents.Enqueue(status); Window.Dispatcher.BeginInvoke(() => Observe("script:" + status.State)); }; scripts.StatusChanged += scriptObserver;
                enabledObserver = (_, _) => Observe("application-cancel-enabled"); Window.applicationCancel.IsEnabledChanged += enabledObserver;
                closedObserver = (_, _) => { if (closeRequestedByUser && !cleanupStarted) shutdownByWindow = Window.closing && Window.closeApproved; closed.TrySetResult(); Observe("closed"); }; Window.Closed += closedObserver;
                Window.Show(); await Navigate("应用");
                Require(Vm.Demo && runtime.IsDemo && ReferenceEquals(runtime.Backend, Backend) && ReferenceEquals(runtime.Devices, catalog) && ReferenceEquals(runtime.Automation, automation) && ReferenceEquals(runtime.Scripts, scripts) && ReferenceEquals(runtime.Input, input) && ReferenceEquals(runtime.Api, api) && !api.BasicRunning && !api.AutomatorRunning && api.DevicePorts.Count == 0 && !Window.DebugAutomaticFixturePermitted && !Window.ReconnectLoadingFixturePermitted, "一致自有Demo/STA服务，API关闭，默认节点自动许可未开放");
                await Track("set-main-b", Vm.SetMainAsync(B)).WaitAsync(TimeSpan.FromSeconds(5)); Vm.CurrentDevice = B; A.IsSelected = false; B.IsSelected = true; Vm.UpdateSelection(); await Idle();
                Require(Window.applicationTargets.Count == 2 && Window.applicationTargets.All(row => !row.IsSelected && row.Available) && Window.applicationReadTimer is null && !Window.applicationReadPending && Backend.Lists().Length == 0, "初始化不勾应用、不创建timer、不读取列表");
                current = Vm.CurrentDevice; main = Vm.MainDevice; rows = Vm.Devices.ToArray(); selected = Vm.Devices.Select(row => row.IsSelected).ToArray(); targets = Vm.Targets().ToArray(); unified = Window.unifiedDevices.ToArray(); unifiedSelected = Window.unifiedDevices.Select(row => row.IsSelected).ToArray();
                Require(ReferenceEquals(current, B) && ReferenceEquals(main, B) && targets.Length == 1 && ReferenceEquals(targets[0], B), "全局Current/Main/群控仅B，应用初始独立空选择");
                stages["initialize"] = "PASS"; Observe("initialized");
            }
            catch (AppPendingPreconditionFailure) { throw; }
            catch (Exception error) { throw new AppPendingPreconditionFailure("自有应用挂起读取初始化失败", error); }
        }

        internal void Expect(params string[] names) { foreach (var name in names) stages.Add(name, "NOT-RUN"); }
        internal void Begin(string name) { stages[name] = "STARTED"; Observe("stage:" + name); }
        internal void Complete(string name) => stages[name] = "PASS";
        internal void FailActiveStage(Exception? error) { if (error is null) return; foreach (var key in stages.Where(pair => pair.Value == "STARTED").Select(pair => pair.Key).ToArray()) stages[key] = error is AppPendingPreconditionFailure ? "PRECONDITION-FAIL" : error is AppPendingProductFailure ? "PRODUCT-FAIL" : "HARNESS-FAIL"; }
        internal object[] StageReport() => stages.Select(pair => (object)new { stage = pair.Key, status = pair.Value }).ToArray();
        private Task Track(string label, Task task, Exception? expectedFailure = null) { if (!tasks.Any(item => ReferenceEquals(item.Task, task))) tasks.Add(new(label, task, expectedFailure)); return task; }
        internal object[] TaskReport() => tasks.Select(item => item.Report()).Concat(Backend?.TaskReport() ?? []).Concat(releases.Select(item => (object)new { label = "busy-release-tcs", status = item.Task.Status.ToString(), item.Task.IsCompleted })).Append(new { label = "closed-event-tcs", status = closed.Task.Status.ToString(), closed.Task.IsCompleted }).ToArray();
        internal async Task Idle() { await Window.Dispatcher.InvokeAsync(Window.UpdateLayout, DispatcherPriority.ApplicationIdle).Task.WaitAsync(TimeSpan.FromSeconds(5)); await Window.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ContextIdle).Task.WaitAsync(TimeSpan.FromSeconds(5)); ResolveWaits(); }
        internal async Task Finish(Task task) { await task.WaitAsync(TimeSpan.FromSeconds(5)); await Idle(); }

        internal async Task Navigate(string tag)
        {
            try { Window.navigation[tag].RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent)); await Idle(); Require(Equals((Window.tabs.SelectedItem as TabItem)?.Tag, tag), "真实导航Click选中原Tab：" + tag); }
            catch (AppPendingPreconditionFailure) { throw; }
            catch (Exception error) { throw new AppPendingPreconditionFailure("真实导航不可用：" + tag, error); }
            Observe("navigation:" + tag);
        }

        internal async Task Select(string serial, bool value, bool settle = true)
        {
            try
            {
                Require(Window.applicationPage.IsVisible && Window.applicationTargetPanel.IsEnabled && !Window.applicationRunning && !Vm.Busy, "只在真实可操作应用目标区改变勾选");
                var row = Window.applicationTargets.Single(item => item.Serial == serial); var grid = VisualDescendants<DataGrid>(Window.applicationTargetPanel).Single();
                grid.ScrollIntoView(row); await Idle(); var content = grid.Columns[0].GetCellContent(row);
                Require(content is not null, "真实目标选择单元格已实现");
                var check = content as CheckBox ?? VisualDescendants<CheckBox>(content!).Single(); var binding = check.GetBindingExpression(ToggleButton.IsCheckedProperty) ?? throw new AppPendingPreconditionFailure("真实目标CheckBox没有BindingExpression");
                Require(check.IsVisible && check.IsEnabled && ReferenceEquals(binding.DataItem, row) && binding.ParentBinding.Path.Path == nameof(ApplicationTargetRow.IsSelected) && binding.ParentBinding.Mode == BindingMode.TwoWay && row.IsSelected != value, "真实可见CheckBox绑定原目标行，且本次为有效Toggle");
                var peer = new CheckBoxAutomationPeer(check); var toggle = (IToggleProvider?)peer.GetPattern(PatternInterface.Toggle); Require(toggle is not null, "原CheckBox提供Toggle自动化模式"); toggle!.Toggle();
                Require(row.IsSelected == value && check.IsChecked == value, "Toggle经原TwoWay Binding同步目标选择");
                AttachTimer(); Observe("bound-checkbox-toggle", new { serial, value, logicalWpfAutomation = true, physicalInput = false, binding = binding.ParentBinding.Path.Path }); if (settle) await Idle();
            }
            catch (AppPendingPreconditionFailure) { throw; }
            catch (Exception error) { throw new AppPendingPreconditionFailure("应用目标CheckBox/Binding前置失败：" + serial, error); }
        }

        private void AttachTimer()
        {
            var timer = Window.applicationReadTimer;
            Require(timer is not null && timer.Interval == TimeSpan.FromMilliseconds(350) && ReferenceEquals(timer.Dispatcher, Window.Dispatcher), "勾选实际创建原350ms DispatcherTimer");
            if (tickObserver is not null) return;
            // The constructor installed the production callback before this observer is appended.
            tickObserver = (_, _) =>
            {
                var tick = new AppPendingTick(Ticks.Count + 1, Stopwatch.GetTimestamp(), Window.applicationReadPending, Window.applicationRunning, Vm.Busy, Vm.Transferring, Window.applicationPage.IsVisible, Window.closing || Window.closeApproved, scripts!.GetStatus(A.Serial).State, Backend.Lists().Length);
                Ticks.Add(tick); Observe("original-tick-after-production-handler", tick);
            };
            timer!.Tick += tickObserver; Observe("timer-observer-appended-after-production-constructor");
        }

        internal async Task<AppPendingTick> TickAfter(int before)
        {
            try { await WaitFor(() => TickCount > before, "original-tick-observation").WaitAsync(TimeSpan.FromSeconds(5)); }
            catch (TimeoutException error) { throw new AppPendingPreconditionFailure("原350ms timer未触发，不能算产品pending失败", error); }
            return Ticks[before];
        }

        internal void RequireBlockedTick(AppPendingTick tick, bool script = false, bool transfer = false, bool busy = false)
        {
            Require(tick.Visible && !tick.Running && tick.Lists == 0 && (!script || tick.ScriptA is ScriptRunState.Running or ScriptRunState.Cancelling) && (!transfer || tick.Transferring) && tick.Busy == busy, "原timer在真实预期阻塞期间触发且尚无List；pending值仅观察不伪造");
            Observe("blocked-pending-observed", new { tick.Pending, script, transfer, busy });
        }

        internal async Task<AppPendingBlock> BlockScript(string serial, bool fail = false)
        {
            var gate = Backend.Hold("ScriptStart", serial, fail ? new InvalidOperationException("own pending script failure") : null);
            var task = Track("script:" + serial, scripts!.RunAsync(serial, new ScriptPackage { Manifest = new() { Id = "own-pending", Name = "自有挂起读取阻塞", Version = "1.0" }, Steps = [new() { Action = "launch", Value = AppPendingBackend.ScriptPackage }] }), gate.Failure);
            try { await gate.Entered.Task.WaitAsync(TimeSpan.FromSeconds(5)); await Idle(); Require(gate.Claimed && scripts!.GetStatus(serial).State == ScriptRunState.Running && !Vm.Busy, "真实ScriptScheduler已进入自有后端TCS门，未占Busy"); }
            catch (AppPendingPreconditionFailure) { throw; }
            catch (Exception error) { throw new AppPendingPreconditionFailure("脚本未进入真实Running/TCS门", error); }
            return new(gate, task);
        }

        internal async Task ReleaseScript(AppPendingBlock script, string outcome)
        {
            var notices = activeOperationNotices;
            if (outcome == "cancelled") scripts!.Stop(script.Gate.Serial); else script.Gate.Release.TrySetResult();
            try { await script.Task.WaitAsync(TimeSpan.FromSeconds(5)); if (outcome == "failed") throw new AppPendingPreconditionFailure("预期脚本失败未发生"); }
            catch (Exception error) when (outcome == "failed" && ReferenceEquals(error, script.Gate.Failure)) { tasks.Single(item => ReferenceEquals(item.Task, script.Task)).FailureObserved = true; Observe("expected-script-task-failure", new { error.Message, exactMarkerException = true }); }
            await Idle(); var expected = outcome switch { "completed" => ScriptRunState.Completed, "failed" => ScriptRunState.Failed, "cancelled" => ScriptRunState.Cancelled, _ => throw new ArgumentOutOfRangeException(nameof(outcome)) };
            Require(scripts!.GetStatus(script.Gate.Serial).State == expected && ScriptStates(script.Gate.Serial).LastOrDefault() == expected, "真实ScriptScheduler终态与StatusChanged已到达：" + expected);
            Require(activeOperationNotices > notices, "脚本真实终态已投递VM HasActiveOperations通知，不假定此刻没有其它合法活动"); Observe("script-released:" + outcome, new { noticesBefore = notices, noticesAfter = activeOperationNotices });
        }
        internal ScriptRunState[] ScriptStates(string serial) => ScriptEvents.Where(item => item.Serial == serial).Select(item => item.State).ToArray();
        internal void RequireScriptRunning(string serial) => Require(scripts!.GetStatus(serial).State == ScriptRunState.Running && ScriptStates(serial).LastOrDefault() == ScriptRunState.Running, "其它设备合法脚本仍在Running，不作为本次应用目标的全局阻塞：" + serial);

        internal async Task<AppPendingBlock> BlockPush()
        {
            var gate = Backend.Hold("Push", B.Serial); var path = Path.Combine(directory, "own.txt"); await File.WriteAllTextAsync(path, "own pending transfer"); Vm.QueuePush(path, false);
            Require(Vm.Transfers.Count == 1 && Vm.Transfers.Single().Serial == B.Serial && !Vm.Transfers.Single().AutoInstall, "真实VM仅为全局B排队自有文件，AutoInstall关闭");
            var task = Track("push-b", Vm.StartTransfersAsync());
            try { await gate.Entered.Task.WaitAsync(TimeSpan.FromSeconds(5)); await Idle(); Require(gate.Claimed && Vm.Transferring && !Vm.Busy, "真实Push进入稳定TCS门，无guest转发"); }
            catch (AppPendingPreconditionFailure) { throw; }
            catch (Exception error) { throw new AppPendingPreconditionFailure("真实Push未进入TCS门", error); }
            return new(gate, task, overallProgressNotices);
        }
        internal async Task ReleasePush(AppPendingBlock push) { var notices = transferNotices; push.Gate.Release.TrySetResult(); await Finish(push.Task); Require(!Vm.Transferring && Vm.Transfers.Single().State == "传输完成" && transferNotices > notices, "真实VM传输finally结束并投递Transferring通知"); }

        internal async Task<AppPendingBusy> BlockBusy()
        {
            var release = NewCompletion(); var entered = NewCompletion(); releases.Add(release);
            var task = Track("busy-operation", Vm.ExecuteAsync("自有挂起读取Busy", async () => { entered.TrySetResult(); await release.Task; }));
            try { await Track("busy-entered-event", entered.Task).WaitAsync(TimeSpan.FromSeconds(5)); Require(Vm.Busy && !Window.applicationTargetPanel.IsEnabled, "选择之后受控真实VM ExecuteAsync进入Busy，未声称点击另一个按钮，未操作禁用目标区"); }
            catch (AppPendingPreconditionFailure) { throw; }
            catch (Exception error) { throw new AppPendingPreconditionFailure("真实Busy未进入", error); }
            return new(release, task);
        }

        internal async Task ReadEntered(AppPendingGate read)
        {
            try { await read.Entered.Task.WaitAsync(TimeSpan.FromSeconds(5)); }
            catch (TimeoutException error) { throw new AppPendingProductFailure("有效原意图释放后5秒内未进入自动List：" + read.Serial, error); }
            Product(read.Claimed && Window.applicationRunning && Vm.Busy && Backend.ActiveLists == 1 && Backend.MaximumActiveLists == 1, "自动List实际进入唯一批次/TCS门"); Observe("automatic-list-entered", new { read.Serial });
        }
        internal async Task CompleteRead(AppPendingGate read, bool failed = false, bool observeOnce = true)
        {
            read.Release.TrySetResult();
            try { await read.GateExited.Task.WaitAsync(TimeSpan.FromSeconds(5)); await Idle(); await WaitFor(() => !Window.applicationRunning && !Vm.Busy, "read-ui-settled").WaitAsync(TimeSpan.FromSeconds(5)); }
            catch (TimeoutException error) { throw new AppPendingProductFailure("实际自动List或原批次UI未排空", error); }
            Product(Backend.ActiveLists == 0 && Backend.ListTasksCompleted, "GateExited仅表示门退出；原List返回Task及批次UI均实际完成");
            Product(Window.applicationTasks.Count == 1 && Window.applicationTasks.Single().ResultState == (failed ? ApplicationTaskState.Failed : ApplicationTaskState.Succeeded), "原应用批次呈现预期终态");
            Product(failed ? Window.applicationRows.Count == 0 && Window.applicationTasks.Single().Message == read.Failure?.Message && Vm.Error.Length != 0 && Vm.Status == "操作失败" : Window.applicationRows.Count == 1 && Window.applicationRows.Single().Serial == read.Serial && Window.applicationRows.Single().PackageName == AppPendingBackend.ApplicationPackage && Vm.Error.Length == 0, "原列表发布/逐项marker/安全错误反馈与本次结果一致");
            if (observeOnce) await ObservationWindow();
            NoApplicationWrites(); Observe("automatic-list-finished", new { read.Serial, failed });
        }
        internal void AssertLists(params string[] serials) => Product(Backend.Lists().Select(call => call.Serial).SequenceEqual(serials), "真实List顺序 expected=" + string.Join(",", serials) + "; actual=" + string.Join(",", Backend.Lists().Select(call => call.Serial)));
        internal void NoApplicationWrites() => Product(Backend.Calls().All(call => !call.ForbiddenWrite && !call.Operation.StartsWith("UnexpectedRead:", StringComparison.Ordinal)), "应用自动路径没有设备写或无关读取；仅明确脚本stub/Push门被允许");

        private Task WaitFor(Func<bool> ready, string label)
        {
            if (ready()) return Task.CompletedTask; var completion = NewCompletion(); waits.Add(new(ready, completion)); Track(label, completion.Task); return completion.Task;
        }
        private void ResolveWaits() { foreach (var wait in waits.ToArray()) if (wait.Ready()) { waits.Remove(wait); wait.Completion.TrySetResult(); } }
        internal async Task ObservationWindow()
        {
            var completion = NewCompletion(); var timer = new DispatcherTimer(DispatcherPriority.Background, Window.Dispatcher) { Interval = TimeSpan.FromMilliseconds(850) };
            timer.Tick += (_, _) => { timer.Stop(); completion.TrySetResult(); }; observationTimers.Add(timer); _ = Track("bounded-no-poll-observation", completion.Task); timer.Start();
            await completion.Task.WaitAsync(TimeSpan.FromSeconds(5)); await Idle(); Observe("850ms-observation-complete");
        }
        internal async Task CloseByUser()
        {
            Require(!Vm.Busy && Vm.HasActiveOperations && Window.IsVisible, "真实用户Close前无Busy，存在可确认的脚本活动"); var confirmations = 0;
            Vm.Confirm = (_, _) => { confirmations++; return true; }; closeRequestedByUser = true; Window.Close();
            try { await closed.Task.WaitAsync(TimeSpan.FromSeconds(5)); await Idle(); }
            catch (TimeoutException error) { throw new AppPendingPreconditionFailure("自有Window.Close/Shutdown未完成", error); }
            Product(shutdownByWindow && confirmations == 1 && !Window.IsVisible, "真实Close/Closing/单次Shutdown完成，只写自有artifacts配置");
        }

        internal void Context()
        {
            Product(ReferenceEquals(current, Vm.CurrentDevice) && ReferenceEquals(main, Vm.MainDevice) && rows.SequenceEqual(Vm.Devices) && selected.SequenceEqual(Vm.Devices.Select(row => row.IsSelected)) && targets.SequenceEqual(Vm.Targets()) && unified.SequenceEqual(Window.unifiedDevices) && unifiedSelected.SequenceEqual(Window.unifiedDevices.Select(row => row.IsSelected)), "应用独立选择/读取未改变全局B的Current/Main/群控及原行引用");
        }
        internal void Require(bool value, string message) { if (!value) { Observe("precondition-failed", message); throw new AppPendingPreconditionFailure(message); } Checks.Add("PRECONDITION " + message); }
        internal void Product(bool value, string message) { if (!value) { Observe("product-failed", message); throw new AppPendingProductFailure(message); } Checks.Add("PRODUCT " + message); }
        internal void Observe(string stage, object? detail = null)
        {
            if (Events.Count == 256) Events.RemoveAt(0);
            Events.Add(new { stage, at = Stopwatch.GetTimestamp(), detail, busy = Vm?.Busy, transferring = Vm?.Transferring, running = Window?.applicationRunning, pending = Window?.applicationReadPending, timerEnabled = Window?.applicationReadTimer?.IsEnabled, closing = Window?.closing, closeApproved = Window?.closeApproved, page = (Window?.tabs.SelectedItem as TabItem)?.Tag, lists = Backend?.Lists().Length, current = Vm?.CurrentDevice?.Serial, main = Vm?.MainDevice?.Serial, error = Vm?.Error, activeOperationNotices, transferNotices, overallProgressNotices });
            ResolveWaits();
        }

        internal async Task Cleanup()
        {
            if (cleanupStarted) throw new InvalidOperationException("应用挂起读取case不得重复清理"); cleanupStarted = true; var errors = new List<Exception>();
            Observe("cleanup-before-lifecycle");
            if (Window is not null && !shutdownByWindow) try { Window.AbortStartup(); } catch (Exception error) { errors.Add(error); }
            Backend?.ReleaseAll(); foreach (var release in releases) release.TrySetResult();
            if (Vm is not null) foreach (var row in Vm.Devices) scripts?.Stop(row.Serial);
            foreach (var item in tasks.Where(item => !item.Label.StartsWith("original-tick", StringComparison.Ordinal) && item.Label != "read-ui-settled").ToArray())
                try { await item.Drain(); } catch (Exception error) { errors.Add(error); }
            if (Backend is not null) try { await Backend.DrainTasks(); } catch (Exception error) { errors.Add(error); }
            if (Window is not null) try { await Idle(); await WaitFor(() => !Window.applicationRunning && !Vm!.Busy, "cleanup-ui-settled").WaitAsync(TimeSpan.FromSeconds(5)); } catch (Exception error) { errors.Add(error); }
            if (Vm is not null && !shutdownByWindow) try { await Track("own-shutdown", Vm.ShutdownAsync()).WaitAsync(TimeSpan.FromSeconds(5)); } catch (Exception error) { errors.Add(error); }
            else if (Vm is null)
            {
                if (runtime is not null) try { await Track("partial-runtime-shutdown", runtime.DisposeAsync().AsTask()).WaitAsync(TimeSpan.FromSeconds(5)); } catch (Exception error) { errors.Add(error); }
                else
                {
                    if (api is not null) await Attempt(() => api.DisposeAsync().AsTask()); if (input is not null) await Attempt(() => input.DisposeAsync().AsTask()); if (scripts is not null) await Attempt(() => scripts.DisposeAsync().AsTask()); if (Backend is not null) await Attempt(() => Backend.DisposeAsync().AsTask());
                    try { metadata?.Dispose(); } catch (Exception error) { errors.Add(error); } if (log is not null) await Attempt(() => log.DisposeAsync().AsTask());
                }
            }
            if (Window is not null)
            {
                try { if (Window.applicationReadTimer?.IsEnabled == true) await ObservationWindow(); await Idle(); if (Window.applicationReadTimer?.IsEnabled == true || Window.applicationRunning || Vm!.Busy) throw new InvalidOperationException("关闭后原应用timer/批次未排空；未手工Stop或清pending"); } catch (Exception error) { errors.Add(error); }
                if (propertyObserver is not null) Vm!.PropertyChanged -= propertyObserver; if (scriptObserver is not null) scripts!.StatusChanged -= scriptObserver;
                if (enabledObserver is not null) Window.applicationCancel.IsEnabledChanged -= enabledObserver; if (closedObserver is not null) Window.Closed -= closedObserver;
                if (tickObserver is not null && Window.applicationReadTimer is not null) Window.applicationReadTimer.Tick -= tickObserver;
                LocalizationService.Current.LanguageChanged -= Window.OnLanguageChanged;
            }
            foreach (var timer in observationTimers) timer.Stop(); foreach (var wait in waits) wait.Completion.TrySetCanceled(); waits.Clear();
            if (!closed.Task.IsCompleted) closed.TrySetCanceled();
            if (Backend is not null) try { await Backend.DrainTasks(); if (!Backend.GatesDrained || Backend.ActiveLists != 0) throw new InvalidOperationException("原后端Task/TCS未排空"); } catch (Exception error) { errors.Add(error); }
            if (tasks.Any(item => !item.Task.IsCompleted) || releases.Any(item => !item.Task.IsCompleted)) errors.Add(new InvalidOperationException("原夹具Task/TCS未完成"));
            Observe("cleanup-finished"); if (errors.Count != 0) throw new AggregateException("应用挂起读取自有清理失败", errors);
            async Task Attempt(Func<Task> action) { try { await Track("partial-service-shutdown", action()).WaitAsync(TimeSpan.FromSeconds(5)); } catch (Exception error) { errors.Add(error); } }
        }
    }

    private sealed record AppPendingCall(int Sequence, string Operation, string? Serial, string? Subject, bool ForbiddenWrite, long At, DeviceInfo? Source);
    private sealed class AppPendingGate(string operation, string serial, Exception? failure)
    {
        internal readonly string Operation = operation; internal readonly string Serial = serial; internal readonly Exception? Failure = failure;
        internal bool Claimed; internal readonly TaskCompletionSource Entered = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal readonly TaskCompletionSource Release = new(TaskCreationOptions.RunContinuationsAsynchronously); internal readonly TaskCompletionSource GateExited = new(TaskCreationOptions.RunContinuationsAsynchronously);
    }
    private sealed class AppPendingBackend : IDeviceBackend
    {
        internal const string ScriptPackage = "com.matrix.pending.blocker"; internal const string ApplicationPackage = "com.matrix.pending.application";
        private readonly object sync = new(); private readonly List<AppPendingCall> calls = []; private readonly List<AppPendingGate> gates = []; private readonly List<AppPendingTracked> tasks = [];
        private int activeLists; private int maximumActiveLists;
        internal Func<IReadOnlyList<DeviceInfo>>? Snapshot;
        internal int ActiveLists { get { lock (sync) return activeLists; } } internal int MaximumActiveLists { get { lock (sync) return maximumActiveLists; } }
        internal bool ListTasksCompleted { get { lock (sync) return tasks.Where(item => item.Label.StartsWith("backend-list:", StringComparison.Ordinal)).All(item => item.Task.IsCompleted); } }
        internal AppPendingCall[] Calls() { lock (sync) return calls.ToArray(); } internal AppPendingCall[] Lists() => Calls().Where(call => call.Operation == "List").ToArray();
        internal AppPendingGate Hold(string operation, string serial, Exception? failure = null) { var gate = new AppPendingGate(operation, serial, failure); lock (sync) gates.Add(gate); return gate; }
        internal async Task DrainTasks() { AppPendingTracked[] original; lock (sync) original = tasks.ToArray(); foreach (var task in original) await task.Drain(); }
        internal object[] TaskReport() { lock (sync) return tasks.Select(item => item.Report()).ToArray(); }
        internal object[] GateReport() { lock (sync) return gates.Select(gate => (object)new { gate.Operation, gate.Serial, gate.Claimed, failure = gate.Failure?.Message, entered = gate.Entered.Task.Status.ToString(), released = gate.Release.Task.Status.ToString(), gateExited = gate.GateExited.Task.Status.ToString() }).ToArray(); }
        internal bool GatesDrained { get { lock (sync) return gates.All(gate => gate.Entered.Task.IsCompleted && gate.Release.Task.IsCompleted && gate.GateExited.Task.IsCompleted); } }
        internal void ReleaseAll() { lock (sync) foreach (var gate in gates) { gate.Release.TrySetResult(); if (!gate.Claimed) { gate.Entered.TrySetCanceled(); gate.GateExited.TrySetCanceled(); } } }
        private AppPendingGate? Record(string operation, string? serial, string? subject = null, bool forbidden = false)
        {
            var source = serial is null ? null : Snapshot?.Invoke().SingleOrDefault(info => info.Serial == serial);
            lock (sync) { calls.Add(new(calls.Count + 1, operation, serial, subject, forbidden, Stopwatch.GetTimestamp(), source)); var gate = gates.FirstOrDefault(item => !item.Claimed && item.Operation == operation && item.Serial == serial); if (gate is not null) gate.Claimed = true; return gate; }
        }
        private Task<T> Track<T>(string label, Task<T> task, Exception? expectedFailure = null, bool allowCancellation = false) { lock (sync) tasks.Add(new(label, task, expectedFailure, allowCancellation)); return task; }
        private static async Task HoldGate(AppPendingGate gate, CancellationToken ct)
        {
            gate.Entered.TrySetResult(); try { await gate.Release.Task.WaitAsync(ct); if (gate.Failure is not null) ExceptionDispatchInfo.Capture(gate.Failure).Throw(); } finally { gate.GateExited.TrySetResult(); }
        }
        public Task<IReadOnlyList<DeviceInfo>> DiscoverAsync(CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested(); Record("Discover", null);
            return Task.FromResult<IReadOnlyList<DeviceInfo>>([new() { Serial = "DEMO-001", Name = "应用挂起A", State = "Ready", Width = 160, Height = 240, TransportId = 11 }, new() { Serial = "DEMO-002", Name = "全局目标B", State = "Ready", Width = 160, Height = 240, TransportId = 22 }]);
        }
        public Task<IReadOnlyList<AppInfo>> ListAppsAsync(string serial, bool showSystem = false, bool needVersion = true, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested(); var gate = Record("List", serial); return Track("backend-list:" + serial, List(), gate?.Failure);
            async Task<IReadOnlyList<AppInfo>> List()
            {
                lock (sync) { activeLists++; maximumActiveLists = Math.Max(maximumActiveLists, activeLists); }
                try { if (gate is not null) await HoldGate(gate, ct); return [new(ApplicationPackage, "自有应用" + Lists().Length, "1.0")]; }
                finally { lock (sync) activeLists--; }
            }
        }
        public Task<string> StartAppAsync(string serial, string packageName, string? activity = null, CancellationToken ct = default)
        {
            bool allowed; lock (sync) allowed = packageName == ScriptPackage && gates.Any(item => !item.Claimed && item.Operation == "ScriptStart" && item.Serial == serial);
            var gate = Record(allowed ? "ScriptStart" : "Start", serial, packageName, !allowed);
            if (!allowed || gate is null) throw new InvalidOperationException("本专项只允许预先登记的自有脚本stub门，禁止应用启动");
            return Track("backend-script:" + serial, Script(), gate.Failure, allowCancellation: true);
            async Task<string> Script() { await HoldGate(gate, ct); return "own script stub completed; no guest forwarded"; }
        }
        public Task PushAsync(string serial, string localPath, string remotePath, IProgress<double>? progress = null, CancellationToken ct = default)
        {
            bool allowed; lock (sync) allowed = gates.Any(item => !item.Claimed && item.Operation == "Push" && item.Serial == serial) && File.Exists(localPath) && Path.GetFileName(localPath) == "own.txt";
            var gate = Record("Push", serial, localPath, !allowed); if (!allowed || gate is null) throw new InvalidOperationException("本专项只允许预先登记的自有Push门");
            return Track("backend-push:" + serial, Push(), allowCancellation: true);
            async Task<bool> Push() { await HoldGate(gate, ct); return true; }
        }
        private Exception Forbidden(string operation, string serial, string? subject = null) { Record(operation, serial, subject, true); return new InvalidOperationException("本专项禁止设备写：" + operation + ":" + serial); }
        public Task<string> ShellAsync(string serial, string command, bool useRoot = false, CancellationToken ct = default) => throw Forbidden("Shell", serial, command);
        public Task TapAsync(string serial, int x, int y, CancellationToken ct = default) => throw Forbidden("Tap", serial);
        public Task SwipeAsync(string serial, int x1, int y1, int x2, int y2, int durationMs, CancellationToken ct = default) => throw Forbidden("Swipe", serial);
        public Task KeyEventAsync(string serial, int code, CancellationToken ct = default) => throw Forbidden("Key", serial);
        public Task TextAsync(string serial, string text, CancellationToken ct = default) => throw Forbidden("Text", serial, text);
        public Task<string> StopAppAsync(string serial, string packageName, CancellationToken ct = default) => throw Forbidden("Stop", serial, packageName);
        public Task<bool> ClearAppAsync(string serial, string packageName, CancellationToken ct = default) => throw Forbidden("Clear", serial, packageName);
        public Task InstallAsync(string serial, string localPath, CancellationToken ct = default) => throw Forbidden("Install", serial, localPath);
        public Task UninstallAsync(string serial, string packageName, CancellationToken ct = default) => throw Forbidden("Uninstall", serial, packageName);
        public Task PullAsync(string serial, string remotePath, string localPath, IProgress<double>? progress = null, CancellationToken ct = default) => throw Forbidden("Pull", serial, remotePath);
        public Task SetNodeTextAsync(string serial, NodeSelector selector, string text, CancellationToken ct = default) => throw Forbidden("SetNodeText", serial, text);
        public Task<byte[]> ScreenshotAsync(string serial, CancellationToken ct = default) { Record("UnexpectedRead:Screenshot", serial); throw new InvalidOperationException("本专项禁止截图读取"); }
        public Task<AndroidNode> DumpAsync(string serial, CancellationToken ct = default) { Record("UnexpectedRead:Dump", serial); throw new InvalidOperationException("本专项禁止节点读取"); }
        public ValueTask DisposeAsync() { ReleaseAll(); return ValueTask.CompletedTask; }
    }
}
