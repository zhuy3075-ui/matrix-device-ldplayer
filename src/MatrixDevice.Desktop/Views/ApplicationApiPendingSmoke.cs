using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.ExceptionServices;
using System.Text;
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
    private async Task VerifyApplicationApiPendingAsync()
    {
        if (!ViewModel.Demo || !ViewModel.Runtime.IsDemo || !Dispatcher.CheckAccess() || Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
            throw new InvalidOperationException("应用接口挂起读取验收仅限自有Demo/STA");
        var directory = Path.Combine(Path.GetFullPath(ViewModel.DataDirectory), "artifacts", "aa", Guid.NewGuid().ToString("N")[..8]);
        if (!Path.IsPathFullyQualified(directory) || directory.Length > 190) throw new InvalidOperationException("应用接口挂起读取验收需要短绝对artifacts目录");
        Directory.CreateDirectory(directory);
        var language = LocalizationService.Current.Language;
        var names = new[] { "api-single-no-vm-notification", "api-two-release-a-first", "api-two-release-b-first", "api-latest-selection-b", "api-clear-selection", "api-leave-return", "api-and-push-push-first", "api-and-script-api-first", "api-http-failure-releases-counter", "api-list-failure-new-intent", "api-request-cancelled", "api-close-in-flight", "api-throwing-observer-isolated", "api-queued-idle-before-close" };
        var reports = new List<object>(); var failures = new List<Exception>(); Exception? failure = null; var number = 0;
        try
        {
            await Case(names[0], async (state, api) =>
            {
                var request = await api.Block(state.A.Serial); var read = state.Backend.Hold("List", state.A.Serial);
                await SelectDuringApi(state, api, state.A.Serial);
                var notices = api.Notices.Count;
                await api.Release(request); await state.ReadEntered(read);
                state.Product(!api.Notices.Skip(notices).Any(notice => notice.Property == nameof(WorkspaceViewModel.Transferring) || notice.Property == nameof(WorkspaceViewModel.HasActiveOperations) && !notice.Busy), "仅HTTP终态释放，无额外脚本/传输VM通知；自动List自身Busy/HasActiveOperations通知允许存在");
                await state.CompleteRead(read); state.AssertLists(state.A.Serial);
                state.Product(!state.Window.applicationReadPending, "接口最后释放后一次读取消费原挂起意图");
            });
            foreach (var first in new[] { 0, 1 }) await Case(names[first + 1], async (state, api) =>
            {
                var a = await api.Block(state.A.Serial); var b = await api.Block(state.B.Serial);
                var read = state.Backend.Hold("List", state.A.Serial); await SelectDuringApi(state, api, state.A.Serial);
                await api.Release(first == 0 ? a : b, expectActive: true);
                await state.ObservationWindow(); state.AssertLists();
                state.Product(state.Vm.Runtime.Api.HasActiveRequests && state.Window.applicationReadPending && !state.Window.applicationReadTimer!.IsEnabled, "并发请求仅释放首个仍保留意图，原debounce已停，零List");
                state.Product(api.DrainNoticeCount == 0, "首个并发请求完成后850ms仍零RequestsDrained通知，不把任意请求完成冒充全空闲");
                await api.Release(first == 0 ? b : a); await state.ReadEntered(read);
                state.Product(api.DrainNoticeCount == 1, "最后并发请求归零并实际List进入后，RequestsDrained恰好一次");
                await state.CompleteRead(read); state.AssertLists(state.A.Serial);
            });
            await Case(names[3], async (state, api) =>
            {
                var request = await api.Block(state.A.Serial); await SelectDuringApi(state, api, state.A.Serial);
                var before = state.TickCount; await state.Select(state.A.Serial, false); await state.Select(state.B.Serial, true);
                await BlockedTick(state, api, before); var read = state.Backend.Hold("List", state.B.Serial);
                await api.Release(request); await state.ReadEntered(read); await state.CompleteRead(read); state.AssertLists(state.B.Serial);
            });
            await Case(names[4], async (state, api) =>
            {
                var request = await api.Block(state.A.Serial); await SelectDuringApi(state, api, state.A.Serial);
                await state.Select(state.A.Serial, false); await api.Release(request); await state.ObservationWindow(); state.AssertLists();
                state.Product(!state.Window.applicationReadPending && !state.Window.applicationRunning && !state.Window.applicationReadTimer!.IsEnabled, "清空选择后接口释放不重新读取");
            });
            await Case(names[5], async (state, api) =>
            {
                var request = await api.Block(state.A.Serial); await SelectDuringApi(state, api, state.A.Serial);
                await state.Navigate("设备墙 / 主控"); await api.Release(request); await state.ObservationWindow(); state.AssertLists();
                state.Product(state.Window.applicationReadPending && !state.Window.applicationReadTimer!.IsEnabled, "离页时接口释放不读取，保留返回时的原意图");
                var read = state.Backend.Hold("List", state.A.Serial); await state.Navigate("应用");
                await state.ReadEntered(read); await state.CompleteRead(read); state.AssertLists(state.A.Serial);
            });
            await Case(names[6], async (state, api) =>
            {
                var request = await api.Block(state.A.Serial); var push = await state.BlockPush();
                await SelectDuringApi(state, api, state.A.Serial, transfer: true);
                var before = state.TickCount; await state.ReleasePush(push); await BlockedTick(state, api, before);
                state.Product(state.Vm.Runtime.Api.HasActiveRequests && !state.Vm.Transferring, "Push真实终态后原timer仍被在途HTTP阻止");
                var read = state.Backend.Hold("List", state.A.Serial); await api.Release(request);
                await state.ReadEntered(read); await state.CompleteRead(read); state.AssertLists(state.A.Serial);
            });
            await Case(names[7], async (state, api) =>
            {
                var request = await api.Block(state.A.Serial); var script = await state.BlockScript(state.A.Serial);
                await SelectDuringApi(state, api, state.A.Serial, script: true); await api.Release(request);
                await state.ObservationWindow(); state.RequireScriptRunning(state.A.Serial); state.AssertLists();
                state.Product(state.Window.applicationReadPending, "接口先释放不能越过同目标真实脚本门");
                var read = state.Backend.Hold("List", state.A.Serial); await state.ReleaseScript(script, "completed");
                await state.ReadEntered(read); await state.CompleteRead(read); state.AssertLists(state.A.Serial);
            });
            await Case(names[8], async (state, api) =>
            {
                var request = await api.Block(state.A.Serial, new DeviceException("own API pending service failure", 503, 2000));
                var read = state.Backend.Hold("List", state.A.Serial); await SelectDuringApi(state, api, state.A.Serial);
                await api.Release(request); await state.ReadEntered(read); await state.CompleteRead(read); state.AssertLists(state.A.Serial);
            });
            await Case(names[9], async (state, api) =>
            {
                var request = await api.Block(state.A.Serial);
                var read = state.Backend.Hold("List", state.A.Serial, new InvalidOperationException("own API pending list failure"));
                await SelectDuringApi(state, api, state.A.Serial); await api.Release(request);
                await state.ReadEntered(read); await state.CompleteRead(read, failed: true, observeOnce: false); await state.ObservationWindow(); state.AssertLists(state.A.Serial);
                state.Product(!state.Window.applicationReadPending && !state.Window.applicationReadTimer!.IsEnabled, "接口释放后List失败不产生循环重试");
                read = state.Backend.Hold("List", state.A.Serial); await state.Select(state.A.Serial, false); await state.Select(state.A.Serial, true);
                await state.ReadEntered(read); await state.CompleteRead(read); state.AssertLists(state.A.Serial, state.A.Serial);
            });
            await Case(names[10], async (state, api) =>
            {
                var request = await api.Block(state.A.Serial); var read = state.Backend.Hold("List", state.A.Serial);
                await SelectDuringApi(state, api, state.A.Serial); await api.Cancel(request);
                await state.ReadEntered(read); await state.CompleteRead(read); state.AssertLists(state.A.Serial);
            });
            await CloseCase(names[11]);
            var observerMarker = "own API observer " + Guid.NewGuid().ToString("N");
            await Case(names[12], async (state, api) =>
            {
                var throwingCalls = 0; var normalCalls = 0; var markerLogs = 0; var normalSeen = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                var markerFailure = new InvalidOperationException(observerMarker);
                EventHandler throwing = (_, _) => { Interlocked.Increment(ref throwingCalls); api.Observe("throwing-observer-called", new { observerMarker, thread = Environment.CurrentManagedThreadId }); throw markerFailure; };
                EventHandler normal = (_, _) => { Interlocked.Increment(ref normalCalls); api.Observe("following-normal-observer-called", new { thread = Environment.CurrentManagedThreadId }); normalSeen.TrySetResult(); };
                Action<string> logged = line => { if (line.Contains("[api-drain-notification-error]", StringComparison.Ordinal) && line.Contains(observerMarker, StringComparison.Ordinal)) Interlocked.Increment(ref markerLogs); };
                state.Vm.Runtime.Log.EntryAdded += logged; state.Vm.Runtime.Api.RequestsDrained += throwing; state.Vm.Runtime.Api.RequestsDrained += normal;
                api.Observe("observer-subscription-order", "View subscribed during AppPendingState.Initialize; then Scope read-only drain-count observer; then throwing observer; then normal observer. Throwing observer is NOT before View.");
                try
                {
                    var request = await api.Block(state.A.Serial); var read = state.Backend.Hold("List", state.A.Serial);
                    await SelectDuringApi(state, api, state.A.Serial); await api.Release(request);
                    try { await normalSeen.Task.WaitAsync(TimeSpan.FromSeconds(5)); }
                    catch (TimeoutException error) { throw new AppPendingProductFailure("抛异常的订阅者阻断了后订正常订阅者", error); }
                    await state.ReadEntered(read); await state.CompleteRead(read); state.AssertLists(state.A.Serial);
                    state.Product(Volatile.Read(ref throwingCalls) == 1 && Volatile.Read(ref normalCalls) == 1 && Volatile.Read(ref markerLogs) == 1 && state.Vm.Runtime.Log.LastError is null, "真实HTTP保持200/body不变；异常订阅者只调用一次，后订正常订阅者及marker日志均一次，自动List一次");
                }
                finally
                {
                    state.Vm.Runtime.Api.RequestsDrained -= throwing; state.Vm.Runtime.Api.RequestsDrained -= normal; state.Vm.Runtime.Log.EntryAdded -= logged;
                    if (!normalSeen.Task.IsCompleted) normalSeen.TrySetCanceled();
                    api.Observe("observer-case-finally-unsubscribed", new { throwingCalls, normalCalls, markerLogs, normalSeen = normalSeen.Task.Status.ToString() });
                }
            }, (state, api) =>
            {
                var files = Directory.GetFiles(Path.Combine(state.Vm.DataDirectory, "logs"), "*.log");
                state.Require(files.Length is > 0 and <= 8 && files.All(file => new FileInfo(file).Length <= 64 * 1024), "只在原Shutdown排空日志后读取本case有界自有日志文件");
                var count = files.SelectMany(File.ReadAllLines).Count(line => line.Contains("[api-drain-notification-error]", StringComparison.Ordinal) && line.Contains(observerMarker, StringComparison.Ordinal));
                state.Product(count == 1, "异常订阅者marker已由真实RollingLog写入自有日志文件一次"); api.Observe("observer-marker-persisted-after-shutdown", new { count, files = files.Select(Path.GetFileName).ToArray() }); return Task.CompletedTask;
            });
            await CloseCase(names[13], queued: true);
            if (failures.Count != 0) throw new AggregateException("应用接口挂起读取专项失败；全部旧专项/应用身份/12页均NOT-RUN", failures);
        }
        catch (Exception error) { failure = error; Console.Error.WriteLine("APP-API-PENDING-PRIMARY " + error); }
        finally
        {
            try { LocalizationService.Current.SetLanguage(language); } catch (Exception error) { failure = failure is null ? error : new AggregateException(failure, error); }
            try
            {
                await File.WriteAllTextAsync(Path.Combine(directory, "application-api-pending.json"), JsonSerializer.Serialize(new
                {
                    status = failure is null ? "PASS-OFFLINE-APPLICATION-API-PENDING" : "FAIL", plannedCaseCount = names.Length, caseCount = reports.Count, reports, error = failure?.ToString(), notRunCases = names.Skip(reports.Count).ToArray(),
                    scope = "Own consistent Demo/STA windows and existing AppPendingState; actual visible CheckBoxAutomationPeer/TwoWay Binding, original 350ms DispatcherTimer. Logical WPF automation, not physical input. Real public StartAutomatorAsync and loopback HTTP POST /service/info through public Automation.ServiceInfoProvider gates; no private counter/reflection/direct scheduling/VM notification injection. Backend is the own AppPendingBackend only. Actual HTTP response tasks, provider tasks/TCS, server HasActiveRequests and Close tasks are bounded and drained separately. The original first 12 paths/assertions are retained, with added concurrent drain-count assertions; throwing-observer and queued-idle-before-Close are two NEW public RequestsDrained contract cases. This r2 source also uses RequestsDrained read-only observers in all scopes: original production baseline evidence belongs ONLY to frozen r1a, not this source. Queued case uses read-only Dispatcher.Hooks and pre/View/post public observers, one at-most-3s pure-TCS synchronous barrier, then same-stack Close before yielding. No Android/ADB/shared API/real guest/native-dialog/physical-input/layout coverage."
                }, JsonDefaults.Options));
            }
            catch (Exception error) { failure = failure is null ? error : new AggregateException(failure, error); }
        }
        if (failure is not null) ExceptionDispatchInfo.Capture(failure).Throw();

        async Task Case(string name, Func<AppPendingState, AppApiPendingScope, Task> action, Func<AppPendingState, AppApiPendingScope, Task>? afterCleanup = null)
        {
            var state = new AppPendingState(); var api = new AppApiPendingScope(state); Exception? error = null; Exception? writeError = null; var cleanupErrors = new List<Exception>();
            state.Expect("api-initialize", "case-action", "api-drain", "window-cleanup");
            if (afterCleanup is not null) state.Expect("after-cleanup-verification");
            try
            {
                await state.Initialize(this, Path.Combine(directory, "c" + (++number).ToString("00")));
                state.Begin("api-initialize"); await api.Initialize(); state.Complete("api-initialize");
                state.Begin("case-action"); await action(state, api); state.Context(); state.Complete("case-action");
            }
            catch (Exception caught) { error = caught; Console.Error.WriteLine("APP-API-PENDING-CASE " + name + " " + caught); }
            finally
            {
                state.FailActiveStage(error);
                if (state.Backend is not null) try { state.NoApplicationWrites(); } catch (Exception caught) { writeError = caught; }
                state.Begin("api-drain"); try { await api.Cleanup(); state.Complete("api-drain"); } catch (Exception caught) { cleanupErrors.Add(caught); state.FailActiveStage(caught); }
                state.Begin("window-cleanup"); try { await state.Cleanup(); state.Complete("window-cleanup"); } catch (Exception caught) { cleanupErrors.Add(caught); state.FailActiveStage(caught); }
                if (afterCleanup is not null && error is null && writeError is null && cleanupErrors.Count == 0)
                {
                    state.Begin("after-cleanup-verification");
                    try { await afterCleanup(state, api); state.Complete("after-cleanup-verification"); }
                    catch (Exception caught) { error = caught; state.FailActiveStage(caught); }
                }
                var classification = cleanupErrors.Count != 0 ? "CLEANUP-FAIL" : writeError is not null ? writeError is AppPendingProductFailure ? "PRODUCT-FAIL" : "HARNESS-FAIL" : error switch { null => "PASS", AppPendingPreconditionFailure => "PRECONDITION-FAIL", AppPendingProductFailure => "PRODUCT-FAIL", _ => "HARNESS-FAIL" };
                reports.Add(new { name, status = classification, cleaned = cleanupErrors.Count == 0, checks = state.Checks.ToArray(), observations = state.Events.ToArray(), stages = state.StageReport(), ticks = state.Ticks.ToArray(), scriptEvents = state.ScriptEvents.ToArray(), calls = state.Backend?.Calls(), gates = state.Backend?.GateReport(), tasks = state.TaskReport(), api = api.Report(), failure = error?.ToString(), forbiddenOperationFailure = writeError?.ToString(), cleanupFailures = cleanupErrors.Select(item => item.ToString()).ToArray() });
            }
            var errors = new[] { error, writeError }.OfType<Exception>().Concat(cleanupErrors).ToArray(); if (errors.Length != 0) failures.Add(new InvalidOperationException(name, new AggregateException(errors)));
            if (cleanupErrors.Count != 0) throw new AggregateException("自有API case未清理，停止后续接纳；余case NOT-RUN", failures);
        }

        async Task CloseCase(string name, bool queued = false)
        {
            var own = new AppApiCloseState(queued); Exception? error = null; Exception? cleanupError = null;
            try { await own.Run(this, Path.Combine(directory, "c" + (++number).ToString("00"))); }
            catch (Exception caught) { error = caught; Console.Error.WriteLine("APP-API-PENDING-CASE " + name + " " + caught); }
            finally
            {
                try { await own.Cleanup(); } catch (Exception caught) { cleanupError = caught; }
                var classification = cleanupError is not null ? "CLEANUP-FAIL" : error switch { null => "PASS", AppPendingPreconditionFailure => "PRECONDITION-FAIL", AppPendingProductFailure => "PRODUCT-FAIL", _ => "HARNESS-FAIL" };
                reports.Add(new { name, status = classification, cleaned = cleanupError is null, report = own.Report(), failure = error?.ToString(), cleanupFailure = cleanupError?.ToString() });
            }
            var errors = new[] { error, cleanupError }.OfType<Exception>().ToArray(); if (errors.Length != 0) failures.Add(new InvalidOperationException(name, new AggregateException(errors)));
            if (cleanupError is not null) throw new AggregateException("纯API Close case未清理，停止后续接纳", failures);
        }

        static async Task SelectDuringApi(AppPendingState state, AppApiPendingScope api, string serial, bool script = false, bool transfer = false)
        {
            var before = state.TickCount; await state.Select(serial, true, settle: false); api.AttachTimer(state.Window); await state.Idle(); await BlockedTick(state, api, before, script, transfer);
        }
        static async Task BlockedTick(AppPendingState state, AppApiPendingScope api, int before, bool script = false, bool transfer = false)
        {
            var tick = await state.TickAfter(before); state.RequireBlockedTick(tick, script, transfer);
            api.RequireActiveTick(tick.Number);
            state.Require(state.Vm.Runtime.Api.HasActiveRequests && state.Vm.HasActiveOperations == (script || transfer), "真实API计数仍活动；VM活动与预设脚本/传输一致");
            state.Product(tick.Pending && state.Window.applicationReadPending && !state.Window.applicationReadTimer!.IsEnabled, "真实HTTP阻止自动读取，保留意图而不是反复轮询timer");
            api.Observe("blocked-original-tick", new { tick.Number, tick.Pending });
        }
    }

    private sealed class AppApiPendingGate(string serial, string marker, Exception? failure)
    {
        internal readonly string Serial = serial; internal readonly string Marker = marker; internal readonly Exception? Failure = failure;
        internal bool Claimed; internal Task<object?>? ProviderTask; internal Task<AppApiPendingReply>? Request;
        internal readonly CancellationTokenSource RequestCancellation = new(); internal bool CancellationExpected; internal bool CancellationObserved;
        internal readonly TaskCompletionSource Entered = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal readonly TaskCompletionSource Release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal readonly TaskCompletionSource Exited = new(TaskCreationOptions.RunContinuationsAsynchronously);
    }
    private sealed record AppApiPendingReply(int Status, string Body);
    private sealed record AppApiPendingNotice(string? Property, long At, bool Busy);
    private sealed record AppApiDrainNotice(bool SourceMatches, bool Active, int Thread, long At);

    private sealed class AppApiCloseState(bool queued = false)
    {
        private WorkspaceViewModel? vm; private MainWindow? window; private AppPendingBackend? backend; private MatrixRuntime? runtime;
        private CatalogRepository? metadata; private RollingLog? log; private ApiServiceManager? apiService; private InputCoordinator? input; private ScriptScheduler? scripts;
        private AppApiPendingScope? api; private EventHandler? closedObserver; private EventHandler? tickObserver;
        private readonly TaskCompletionSource closed = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource ticked = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly List<string> checks = []; private readonly List<object> events = []; private readonly List<AppPendingTracked> tasks = [];
        private bool closeRequested; private bool shutdownByWindow; private bool cleaned; private int confirmations;
        private AppApiQueuedCloseProbe? queuedProbe;

        internal async Task Run(MainWindow owner, string path)
        {
            try
            {
                Directory.CreateDirectory(path); backend = new(); log = new RollingLog(Path.Combine(path, "logs")); metadata = new CatalogRepository(Path.Combine(path, "db.sqlite3"));
                var catalog = new DeviceCatalog(backend); var automation = new AutomationService(backend, catalog); scripts = new ScriptScheduler(backend); input = new InputCoordinator(catalog, backend); apiService = new ApiServiceManager(catalog, backend, automation, log);
                var settings = new AppSettings { Language = "zh-CN", AutoStartApi = false, AutoConnect = false, SyncMouse = false, SyncKeyboard = false, WindowWidth = 1000, WindowHeight = 700 };
                runtime = new MatrixRuntime { Backend = backend, Devices = catalog, Automation = automation, Scripts = scripts, Input = input, Api = apiService, Metadata = metadata, Log = log, Settings = settings, FrozenConnectionSettings = settings, SettingsStore = new SettingsStore(Path.Combine(path, "AppConfig.json")), IsDemo = true };
                apiService.DeviceSnapshotProvider = runtime.GetDevices; backend.Snapshot = runtime.GetDevices; vm = new WorkspaceViewModel(runtime, path, true);
                await Track("own-discover", vm.RefreshDevicesAsync()).WaitAsync(TimeSpan.FromSeconds(5));
                if (queued) queuedProbe = new AppApiQueuedCloseProbe(apiService, owner.Dispatcher, Require, Product);
                window = new MainWindow(vm) { Owner = owner, ShowActivated = false }; window.captureTimer.Stop(); window.directoryTimer.Stop(); window.monitorTimer.Stop();
                queuedProbe?.SubscribeAfterView(window);
                closedObserver = (_, _) => { shutdownByWindow = closeRequested && window.closing && window.closeApproved; closed.TrySetResult(); events.Add(new { stage = "window-closed", shutdownByWindow }); }; window.Closed += closedObserver;
                window.Show(); window.navigation["应用"].RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent)); await Idle();
                Require(vm.Demo && ReferenceEquals(vm.Runtime.Backend, backend) && ReferenceEquals(vm.Runtime.Devices, catalog) && ReferenceEquals(vm.Runtime.Automation, automation) && ReferenceEquals(vm.Runtime.Scripts, scripts) && ReferenceEquals(vm.Runtime.Api, apiService) && ReferenceEquals(vm.Runtime.Input, input) && window.applicationPage.IsVisible && !window.DebugAutomaticFixturePermitted && !window.ReconnectLoadingFixturePermitted, "纯API Close使用独立一致Demo运行时及原应用页面");
                api = new AppApiPendingScope(() => vm, Require, Product); await api.Initialize();
                var request = await api.Block("DEMO-001");
                var row = window.applicationTargets.Single(item => item.Serial == "DEMO-001"); var grid = VisualDescendants<DataGrid>(window.applicationTargetPanel).Single();
                grid.ScrollIntoView(row); await Idle(); var content = grid.Columns[0].GetCellContent(row); Require(content is not null, "纯API Close目标单元格已实现");
                var check = content as CheckBox ?? VisualDescendants<CheckBox>(content!).Single(); var binding = check.GetBindingExpression(ToggleButton.IsCheckedProperty);
                Require(check.IsVisible && check.IsEnabled && binding is not null && ReferenceEquals(binding.DataItem, row) && binding.ParentBinding.Mode == BindingMode.TwoWay && binding.ParentBinding.Path.Path == nameof(ApplicationTargetRow.IsSelected) && !row.IsSelected, "纯API Close经原可见CheckBox及TwoWay绑定选择目标");
                var toggle = (IToggleProvider?)new CheckBoxAutomationPeer(check).GetPattern(PatternInterface.Toggle); Require(toggle is not null, "原选择控件提供Toggle模式"); toggle!.Toggle();
                Require(row.IsSelected && check.IsChecked == true, "逻辑WPF Toggle同步原行，非物理输入"); api.AttachTimer(window);
                tickObserver = (_, _) =>
                {
                    events.Add(new { stage = "original-tick-after-handler", pending = window.applicationReadPending, running = window.applicationRunning, visible = window.applicationPage.IsVisible, apiActive = apiService.HasActiveRequests, busy = vm.Busy, vm.HasActiveOperations, lists = backend.Lists().Length }); ticked.TrySetResult();
                }; window.applicationReadTimer!.Tick += tickObserver;
                await Track("own-original-tick", ticked.Task).WaitAsync(TimeSpan.FromSeconds(5)); api.RequireActiveTick(1);
                Require(apiService.HasActiveRequests && !vm.Busy && !vm.Transferring && !vm.HasActiveOperations && backend.Lists().Length == 0 && !window.applicationRunning && window.applicationReadPending && !window.applicationReadTimer.IsEnabled, "原Tick真实阻塞且仅API活动，没有额外脚本或传输造Close前置");
                vm.Confirm = (_, _) => { confirmations++; return true; }; api.TrackClose(closed.Task);
                if (!queued)
                {
                    closeRequested = true; window.Close();
                    Require(window.closing && !closed.Task.IsCompleted && apiService.HasActiveRequests && request.Request is { IsCompleted: false }, "纯API真实Close开始且原HTTP仍在途，未先关服务再伪称在途");
                    await api.Release(request);
                }
                else
                {
                    queuedProbe!.ReleaseAndProvePending(request);
                    closeRequested = true; queuedProbe.MarkCloseRequested(); window.Close();
                    await api.FinishReleased(request); await queuedProbe.ObserveCompleted();
                }
                await closed.Task.WaitAsync(TimeSpan.FromSeconds(5)); await Idle();
                Product(shutdownByWindow && confirmations == 0 && !window.IsVisible && !apiService.AutomatorRunning, "纯API Close经原Closing/Shutdown一次完成，不伪造活动或确认");
                await ObserveWindow();
                Product(backend.Lists().Length == 0 && !window.applicationRunning && window.applicationReadTimer.IsEnabled == false && !vm.Busy && !apiService.HasActiveRequests, "纯API关闭后850ms没有新增List、timer或活动请求");
                NoWrites();
            }
            catch (AppPendingPreconditionFailure) { throw; }
            catch (AppPendingProductFailure) { throw; }
            catch (Exception error) { throw new AppPendingPreconditionFailure("纯API Close自有路径初始化或原生命周期失败", error); }
        }

        private async Task Idle() { await window!.Dispatcher.InvokeAsync(window.UpdateLayout, DispatcherPriority.ApplicationIdle).Task.WaitAsync(TimeSpan.FromSeconds(5)); await window.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ContextIdle).Task.WaitAsync(TimeSpan.FromSeconds(5)); }
        private Task Track(string label, Task task) { tasks.Add(new(label, task)); return task; }
        private async Task ObserveWindow()
        {
            var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously); var timer = new DispatcherTimer(DispatcherPriority.Background, window!.Dispatcher) { Interval = TimeSpan.FromMilliseconds(850) };
            timer.Tick += (_, _) => { timer.Stop(); completion.TrySetResult(); }; timer.Start();
            try { await Track("own-close-observation", completion.Task).WaitAsync(TimeSpan.FromSeconds(5)); await Idle(); }
            finally { timer.Stop(); if (!completion.Task.IsCompleted) completion.TrySetCanceled(); }
        }
        private void Require(bool value, string message) { if (!value) throw new AppPendingPreconditionFailure(message); checks.Add("PRECONDITION " + message); }
        private void Product(bool value, string message) { if (!value) throw new AppPendingProductFailure(message); checks.Add("PRODUCT " + message); }
        private void NoWrites() => Product(backend!.Calls().All(call => !call.ForbiddenWrite && call.Operation == "Discover"), "纯API Close没有任何guest写或List/截图/节点等无关读取");

        internal async Task Cleanup()
        {
            if (cleaned) throw new InvalidOperationException("纯API Close不得重复清理"); cleaned = true; var errors = new List<Exception>();
            var abortedForQueuedCleanup = false;
            if (queued && !closeRequested && window is not null)
                try { window.AbortStartup(); abortedForQueuedCleanup = true; } catch (Exception error) { errors.Add(error); }
            if (api is not null) try { await api.Cleanup(); } catch (Exception error) { errors.Add(error); }
            backend?.ReleaseAll();
            if (closeRequested)
            {
                try { await closed.Task.WaitAsync(TimeSpan.FromSeconds(5)); if (!shutdownByWindow) throw new InvalidOperationException("原Close未证实Shutdown完成，不二次Dispose"); } catch (Exception error) { errors.Add(error); }
            }
            else
            {
                if (window is not null && !abortedForQueuedCleanup) try { window.AbortStartup(); } catch (Exception error) { errors.Add(error); }
                if (vm is not null) await Attempt("own-shutdown", vm.ShutdownAsync());
                else if (runtime is not null) await Attempt("partial-runtime-shutdown", runtime.DisposeAsync().AsTask());
                else
                {
                    if (apiService is not null) await Attempt("partial-api", apiService.DisposeAsync().AsTask());
                    if (input is not null) await Attempt("partial-input", input.DisposeAsync().AsTask());
                    if (scripts is not null) await Attempt("partial-scripts", scripts.DisposeAsync().AsTask());
                    if (backend is not null) await Attempt("partial-backend", backend.DisposeAsync().AsTask());
                    try { metadata?.Dispose(); } catch (Exception error) { errors.Add(error); }
                    if (log is not null) await Attempt("partial-log", log.DisposeAsync().AsTask());
                }
            }
            if (window is not null)
            {
                if (tickObserver is not null && window.applicationReadTimer is not null) window.applicationReadTimer.Tick -= tickObserver;
                if (closedObserver is not null) window.Closed -= closedObserver; LocalizationService.Current.LanguageChanged -= window.OnLanguageChanged;
            }
            if (queuedProbe is not null) try { await queuedProbe.Cleanup(); } catch (Exception error) { errors.Add(error); }
            if (!closed.Task.IsCompleted) closed.TrySetCanceled(); if (!ticked.Task.IsCompleted) ticked.TrySetCanceled();
            foreach (var task in tasks) try { await task.Drain(); } catch (Exception error) { errors.Add(error); }
            if (backend is not null) try { NoWrites(); await backend.DrainTasks(); if (!backend.GatesDrained) throw new InvalidOperationException("纯API Close后端门未清理"); } catch (Exception error) { errors.Add(error); }
            if (tasks.Any(task => !task.Task.IsCompleted) || !closed.Task.IsCompleted || !ticked.Task.IsCompleted) errors.Add(new InvalidOperationException("纯API Close原Task/TCS未排空"));
            if (errors.Count != 0) throw new AggregateException("纯API Close自有清理失败", errors);
            async Task Attempt(string label, Task task) { try { await Track(label, task).WaitAsync(TimeSpan.FromSeconds(5)); } catch (Exception error) { errors.Add(error); } }
        }

        internal object Report() => new { queued, cleaned, closeRequested, shutdownByWindow, confirmations, checks = checks.ToArray(), observations = events.ToArray(), closed = closed.Task.Status.ToString(), ticked = ticked.Task.Status.ToString(), tasks = tasks.Select(task => task.Report()).ToArray(), calls = backend?.Calls(), api = api?.Report(), queuedProbe = queuedProbe?.Report() };
    }

    private sealed class AppApiQueuedCloseProbe
    {
        private readonly ApiServiceManager api; private readonly Dispatcher dispatcher; private readonly Action<bool, string> require; private readonly Action<bool, string> product;
        private readonly object sync = new(); private readonly List<DispatcherOperation> posted = []; private readonly ConcurrentQueue<object> events = new(); private readonly ConcurrentQueue<string> boundaryErrors = new();
        private readonly TaskCompletionSource postObserved = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource started = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource completed = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly EventHandler beforeView; private readonly EventHandler afterView;
        private readonly DispatcherHookEventHandler postedObserver; private readonly DispatcherHookEventHandler startedObserver; private readonly DispatcherHookEventHandler completedObserver;
        private int intervalOpen; private int serverThread; private int preCalls; private int postCalls; private int startCalls; private int completeCalls;
        private long closeAt; private long startedAt; private bool postSubscribed; private bool cleaned;

        internal AppApiQueuedCloseProbe(ApiServiceManager api, Dispatcher dispatcher, Action<bool, string> require, Action<bool, string> product)
        {
            this.api = api; this.dispatcher = dispatcher; this.require = require; this.product = product;
            beforeView = (sender, _) =>
            {
                Interlocked.Increment(ref preCalls); var thread = Environment.CurrentManagedThreadId;
                if (!ReferenceEquals(sender, api) || api.HasActiveRequests) boundaryErrors.Enqueue("pre observer source/count mismatch");
                if (Interlocked.CompareExchange(ref intervalOpen, 1, 0) != 0) boundaryErrors.Enqueue("overlapping RequestsDrained observer intervals");
                Volatile.Write(ref serverThread, thread); events.Enqueue(new { stage = "pre-view-observer", thread, at = Stopwatch.GetTimestamp() });
            };
            afterView = (sender, _) =>
            {
                Interlocked.Increment(ref postCalls); var thread = Environment.CurrentManagedThreadId;
                if (!ReferenceEquals(sender, api) || Volatile.Read(ref intervalOpen) != 1 || Volatile.Read(ref serverThread) != thread) boundaryErrors.Enqueue("post observer source/thread/interval mismatch");
                events.Enqueue(new { stage = "post-view-observer", thread, at = Stopwatch.GetTimestamp() }); Volatile.Write(ref intervalOpen, 0); postObserved.TrySetResult();
            };
            postedObserver = (_, args) =>
            {
                if (!ReferenceEquals(args.Operation.Dispatcher, dispatcher) || args.Operation.Priority != DispatcherPriority.Background || Volatile.Read(ref intervalOpen) != 1 || Volatile.Read(ref serverThread) != Environment.CurrentManagedThreadId) return;
                lock (sync) posted.Add(args.Operation);
                events.Enqueue(new { stage = "background-operation-posted-in-view-interval", thread = Environment.CurrentManagedThreadId, at = Stopwatch.GetTimestamp(), priority = args.Operation.Priority.ToString(), status = args.Operation.Status.ToString() });
            };
            startedObserver = (_, args) =>
            {
                lock (sync) if (!posted.Contains(args.Operation)) return;
                Interlocked.Increment(ref startCalls); Interlocked.Exchange(ref startedAt, Stopwatch.GetTimestamp());
                events.Enqueue(new { stage = "captured-operation-started", thread = Environment.CurrentManagedThreadId, at = Interlocked.Read(ref startedAt), closeAt = Interlocked.Read(ref closeAt) }); started.TrySetResult();
            };
            completedObserver = (_, args) =>
            {
                lock (sync) if (!posted.Contains(args.Operation)) return;
                Interlocked.Increment(ref completeCalls); events.Enqueue(new { stage = "captured-operation-completed", thread = Environment.CurrentManagedThreadId, at = Stopwatch.GetTimestamp(), status = args.Operation.Status.ToString() }); completed.TrySetResult();
            };
            try
            {
                dispatcher.Hooks.OperationPosted += postedObserver; dispatcher.Hooks.OperationStarted += startedObserver; dispatcher.Hooks.OperationCompleted += completedObserver;
                api.RequestsDrained += beforeView;
            }
            catch
            {
                api.RequestsDrained -= beforeView; dispatcher.Hooks.OperationPosted -= postedObserver; dispatcher.Hooks.OperationStarted -= startedObserver; dispatcher.Hooks.OperationCompleted -= completedObserver; throw;
            }
        }

        internal void SubscribeAfterView(MainWindow window)
        {
            require(ReferenceEquals(window.Dispatcher, dispatcher) && !postSubscribed, "queued Close pre observer在View构建前，post observer在同一View构建后订阅一次");
            api.RequestsDrained += afterView; postSubscribed = true;
            events.Enqueue(new { stage = "subscription-order-pre-view-post", at = Stopwatch.GetTimestamp() });
        }

        internal void ReleaseAndProvePending(AppApiPendingGate gate)
        {
            require(dispatcher.CheckAccess() && postSubscribed && gate.Claimed && gate.Request is { IsCompleted: false } && !gate.Release.Task.IsCompleted && api.HasActiveRequests, "queued Close仅对原真实在途HTTP门进行一次受控释放");
            require(Volatile.Read(ref preCalls) == 0 && Volatile.Read(ref postCalls) == 0 && !postObserved.Task.IsCompleted, "本case尚无RequestsDrained通知");
            using (dispatcher.DisableProcessing())
            {
                require(gate.Release.TrySetResult(), "同步屏障内只释放原provider TCS一次");
                if (!postObserved.Task.Wait(TimeSpan.FromSeconds(3))) throw new AppPendingPreconditionFailure("3秒纯Host TCS屏障未到达post observer；未等待HTTP/Dispatcher/Close任务");
                DispatcherOperation[] original; lock (sync) original = posted.ToArray();
                require(Volatile.Read(ref preCalls) == 1 && Volatile.Read(ref postCalls) == 1 && boundaryErrors.IsEmpty && original.Length == 1 && Volatile.Read(ref serverThread) != Environment.CurrentManagedThreadId, "同一服务器线程pre/View/post区间内唯一当前Dispatcher Background操作；不唯一不认定竞态");
                require(original[0].Status == DispatcherOperationStatus.Pending && !started.Task.IsCompleted && Volatile.Read(ref startCalls) == 0, "Close前捕获的原操作Status确为Pending且OperationStarted尚未发生");
                events.Enqueue(new { stage = "pending-proved-with-dispatcher-disabled", at = Stopwatch.GetTimestamp(), status = original[0].Status.ToString(), postObserved = postObserved.Task.Status.ToString() });
            }
        }

        internal void MarkCloseRequested()
        {
            DispatcherOperation operation; lock (sync) operation = posted.Single();
            require(dispatcher.CheckAccess() && operation.Status == DispatcherOperationStatus.Pending && !started.Task.IsCompleted, "退出DisableProcessing后同一UI栈尚未yield，原回调仍Pending");
            Interlocked.Exchange(ref closeAt, Stopwatch.GetTimestamp()); events.Enqueue(new { stage = "same-stack-close-requested-next", at = Interlocked.Read(ref closeAt) });
        }

        internal async Task ObserveCompleted()
        {
            try { await completed.Task.WaitAsync(TimeSpan.FromSeconds(5)); }
            catch (TimeoutException error) { throw new AppPendingProductFailure("Close后已捕获的原Background回调未实际执行完成", error); }
            DispatcherOperation operation; lock (sync) operation = posted.Single();
            await operation.Task.WaitAsync(TimeSpan.FromSeconds(5));
            product(started.Task.IsCompletedSuccessfully && Volatile.Read(ref startCalls) == 1 && Volatile.Read(ref completeCalls) == 1 && Interlocked.Read(ref closeAt) > 0 && Interlocked.Read(ref startedAt) >= Interlocked.Read(ref closeAt) && operation.Status == DispatcherOperationStatus.Completed, "原Background回调确在同栈Close之后开始并完成一次，不以仅排队冒充执行");
        }

        internal async Task Cleanup()
        {
            if (cleaned) throw new InvalidOperationException("queued Close探针不得重复清理"); cleaned = true; var errors = new List<Exception>();
            api.RequestsDrained -= beforeView; if (postSubscribed) api.RequestsDrained -= afterView;
            DispatcherOperation[] original; lock (sync) original = posted.ToArray();
            try
            {
                foreach (var operation in original) try { await operation.Task.WaitAsync(TimeSpan.FromSeconds(5)); } catch (Exception error) { errors.Add(error); }
            }
            finally
            {
                dispatcher.Hooks.OperationPosted -= postedObserver; dispatcher.Hooks.OperationStarted -= startedObserver; dispatcher.Hooks.OperationCompleted -= completedObserver;
                if (!postObserved.Task.IsCompleted) postObserved.TrySetCanceled(); if (!started.Task.IsCompleted) started.TrySetCanceled(); if (!completed.Task.IsCompleted) completed.TrySetCanceled();
            }
            if (original.Any(operation => !operation.Task.IsCompleted) || !postObserved.Task.IsCompleted || !started.Task.IsCompleted || !completed.Task.IsCompleted) errors.Add(new InvalidOperationException("queued Close原操作Task/纯TCS未排空"));
            events.Enqueue(new { stage = "all-public-observers-and-hooks-unsubscribed", at = Stopwatch.GetTimestamp() });
            if (errors.Count != 0) throw new AggregateException("queued Close探针清理失败", errors);
        }

        internal object Report()
        {
            DispatcherOperation[] original; lock (sync) original = posted.ToArray();
            return new { cleaned, postSubscribed, preCalls, postCalls, serverThread, startCalls, completeCalls, closeAt, startedAt, postObserved = postObserved.Task.Status.ToString(), started = started.Task.Status.ToString(), completed = completed.Task.Status.ToString(), boundaryErrors = boundaryErrors.ToArray(), operations = original.Select(operation => new { priority = operation.Priority.ToString(), status = operation.Status.ToString(), taskStatus = operation.Task.Status.ToString(), operation.Task.IsCompleted }).ToArray(), observations = events.ToArray() };
        }
    }

    private sealed class AppApiPendingScope
    {
        private readonly Func<WorkspaceViewModel?> viewModel; private readonly Action<bool, string> require; private readonly Action<bool, string> product;
        private WorkspaceViewModel Vm => viewModel() ?? throw new AppPendingPreconditionFailure("自有API运行时尚未创建");
        internal AppApiPendingScope(AppPendingState state) : this(() => state.Vm, state.Require, state.Product) { }
        internal AppApiPendingScope(Func<WorkspaceViewModel?> viewModel, Action<bool, string> require, Action<bool, string> product) { this.viewModel = viewModel; this.require = require; this.product = product; }
        private readonly object sync = new(); private readonly List<AppApiPendingGate> gates = []; private readonly ConcurrentQueue<object> events = new();
        private readonly List<(int Number, bool Active, long At)> ticks = []; private DispatcherTimer? timer; private EventHandler? tickObserver;
        internal readonly ConcurrentQueue<AppApiPendingNotice> Notices = new();
        private readonly ConcurrentQueue<AppApiDrainNotice> drainNotices = new(); private EventHandler? drainObserver; private ApiServiceManager? subscribedApi;
        internal int DrainNoticeCount => drainNotices.Count;
        private readonly CancellationTokenSource cancellation = new(); private HttpClient? client; private Task? start; private Task? close; private int port; private bool cleaned;
        private PropertyChangedEventHandler? propertyObserver;

        internal async Task Initialize()
        {
            try
            {
                require(Vm.Runtime.IsDemo && Vm.Runtime.Backend is AppPendingBackend && !Vm.Runtime.Api.HasActiveRequests && !Vm.Runtime.Api.AutomatorRunning, "API接入沿用原一致Demo运行时，起始零在途请求");
                Vm.Runtime.Automation.ServiceInfoProvider = Provide;
                using (var listener = new TcpListener(IPAddress.Loopback, 0)) { listener.Start(); port = ((IPEndPoint)listener.LocalEndpoint).Port; listener.Stop(); }
                require(port is > 1024 and <= 65535, "仅当前run随机空闲loopback端口，不占用共享API");
                start = Vm.Runtime.Api.StartAutomatorAsync(port, cancellation.Token); await start.WaitAsync(TimeSpan.FromSeconds(5));
                client = new HttpClient(new SocketsHttpHandler { UseProxy = false, AllowAutoRedirect = false }) { BaseAddress = new Uri($"http://127.0.0.1:{port}/"), Timeout = TimeSpan.FromSeconds(30) };
                var observedApi = Vm.Runtime.Api; subscribedApi = observedApi; drainObserver = (sender, _) => drainNotices.Enqueue(new(ReferenceEquals(sender, observedApi), observedApi.HasActiveRequests, Environment.CurrentManagedThreadId, Stopwatch.GetTimestamp())); observedApi.RequestsDrained += drainObserver;
                propertyObserver = (_, args) => Notices.Enqueue(new(args.PropertyName, Stopwatch.GetTimestamp(), Vm.Busy)); Vm.PropertyChanged += propertyObserver;
                require(Vm.Runtime.Api.AutomatorRunning && Vm.Runtime.Api.RunningAutomatorPort == port && !Vm.Runtime.Api.BasicRunning && Vm.Runtime.Api.DevicePorts.Count == 0, "真实Automator服务已绑定自有端口，未启动基础或设备接口");
                Observe("own-loopback-api-started");
            }
            catch (AppPendingPreconditionFailure) { throw; }
            catch (Exception error) { throw new AppPendingPreconditionFailure("自有loopback API初始化失败，端口竞争不重试", error); }
        }

        private Task<object?> Provide(string serial, CancellationToken ct)
        {
            lock (sync)
            {
                var gate = gates.FirstOrDefault(item => item.Serial == serial && !item.Claimed) ?? throw new InvalidOperationException("未登记的自有service/info请求：" + serial);
                gate.Claimed = true; gate.ProviderTask = Hold();
                gate.Entered.TrySetResult(); Observe("provider-entered", new { gate.Serial, gate.Marker });
                return gate.ProviderTask;
                async Task<object?> Hold()
                {
                    try
                    {
                        await gate.Release.Task.WaitAsync(ct).ConfigureAwait(false);
                        if (gate.Failure is not null) ExceptionDispatchInfo.Capture(gate.Failure).Throw();
                        return new { serial = gate.Serial, marker = gate.Marker };
                    }
                    finally { gate.Exited.TrySetResult(); Observe("provider-exited", new { gate.Serial, gate.Marker }); }
                }
            }
        }

        internal async Task<AppApiPendingGate> Block(string serial, Exception? failure = null)
        {
            var gate = new AppApiPendingGate(serial, Guid.NewGuid().ToString("N"), failure); lock (sync) gates.Add(gate);
            gate.Request = Send(gate);
            try { await gate.Entered.Task.WaitAsync(TimeSpan.FromSeconds(5)); }
            catch (Exception error) { throw new AppPendingPreconditionFailure("真实HTTP未进入登记的ServiceInfoProvider门", error); }
            require(gate.Claimed && gate.ProviderTask is { IsCompleted: false } && !gate.Request.IsCompleted && Vm.Runtime.Api.HasActiveRequests && !Vm.Busy, "原HTTP请求与provider均实际在途，真实计数非零，未借VM Busy伪造阻塞");
            Observe("http-blocked", new { gate.Serial, gate.Marker }); return gate;
        }

        private async Task<AppApiPendingReply> Send(AppApiPendingGate gate)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "service/info") { Content = new StringContent(JsonSerializer.Serialize(new { serial = gate.Serial }), Encoding.UTF8, "application/json") };
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellation.Token, gate.RequestCancellation.Token);
            using var response = await client!.SendAsync(request, HttpCompletionOption.ResponseContentRead, linked.Token).ConfigureAwait(false);
            var body = await response.Content.ReadAsStringAsync(linked.Token).ConfigureAwait(false);
            Observe("http-response-disposed-on-return", new { gate.Serial, gate.Marker, status = (int)response.StatusCode });
            return new((int)response.StatusCode, body);
        }

        internal async Task Release(AppApiPendingGate gate, bool expectActive = false)
        {
            require(gate.Claimed && !gate.Release.Task.IsCompleted && gate.Request is not null, "只释放实际进入的原HTTP门一次");
            gate.Release.TrySetResult();
            await FinishReleased(gate, expectActive);
        }

        internal async Task FinishReleased(AppApiPendingGate gate, bool expectActive = false)
        {
            require(gate.Claimed && gate.Release.Task.IsCompletedSuccessfully && gate.Request is not null, "仅等待已释放原门的相同HTTP请求，不再次释放或更换Task");
            AppApiPendingReply reply;
            try { reply = await gate.Request!.WaitAsync(TimeSpan.FromSeconds(5)); await gate.Exited.Task.WaitAsync(TimeSpan.FromSeconds(5)); }
            catch (Exception error) { throw new AppPendingPreconditionFailure("自有HTTP响应/provider未在界限内完成", error); }
            RequireReply(gate, reply);
            if (expectActive) require(Vm.Runtime.Api.HasActiveRequests && gates.Any(item => item.Claimed && !item.Release.Task.IsCompleted), "首请求响应终态后另一个原provider仍阻塞真实服务器计数");
            else
            {
                try { await Until(() => !Vm.Runtime.Api.HasActiveRequests); }
                catch (TimeoutException error) { throw new AppPendingProductFailure("原HTTP响应完成后真实服务器计数未归零", error); }
                product(!Vm.Runtime.Api.HasActiveRequests, "独立观察真实服务器计数归零，不把HTTP响应完成当作finally已执行");
            }
            Observe("http-released-and-server-observed", new { gate.Serial, gate.Marker, expectActive });
        }

        private void RequireReply(AppApiPendingGate gate, AppApiPendingReply reply)
        {
            try
            {
                using var json = JsonDocument.Parse(reply.Body);
                if (gate.Failure is DeviceException expected)
                    product(reply.Status == expected.Status && json.RootElement.GetProperty("code").GetInt32() == expected.Code && json.RootElement.GetProperty("message").GetString() == expected.Message, "真实HTTP保留provider失败状态、code和原marker消息");
                else
                    product(reply.Status == 200 && json.RootElement.GetProperty("code").GetInt32() == 0 && json.RootElement.GetProperty("data").GetProperty("serial").GetString() == gate.Serial && json.RootElement.GetProperty("data").GetProperty("marker").GetString() == gate.Marker, "真实HTTP返回当前门的200、serial与独立marker");
            }
            catch (Exception error) when (error is JsonException or KeyNotFoundException or InvalidOperationException or FormatException or OverflowException)
            {
                throw new AppPendingProductFailure($"真实HTTP响应结构不符；status={reply.Status}；body={reply.Body}", error);
            }
        }

        internal void TrackClose(Task task) { if (close is not null) throw new InvalidOperationException("自有Close只能登记一次"); close = task; }
        private static async Task Until(Func<bool> ready) { var watch = Stopwatch.StartNew(); while (!ready()) { if (watch.Elapsed >= TimeSpan.FromSeconds(5)) throw new TimeoutException("有界API状态观察未完成"); await Task.Delay(10).ConfigureAwait(false); } }
        internal void Observe(string stage, object? detail = null) => events.Enqueue(new { stage, at = Stopwatch.GetTimestamp(), detail, serverActive = viewModel()?.Runtime.Api.HasActiveRequests });

        internal void AttachTimer(MainWindow window)
        {
            if (tickObserver is not null) return;
            timer = window.applicationReadTimer;
            require(timer is not null && timer.Interval == TimeSpan.FromMilliseconds(350), "追加API只读观察器到原350ms timer，不替换回调");
            tickObserver = (_, _) => { ticks.Add((ticks.Count + 1, Vm.Runtime.Api.HasActiveRequests, Stopwatch.GetTimestamp())); Observe("original-tick-api-counter", new { number = ticks.Count, active = Vm.Runtime.Api.HasActiveRequests }); };
            timer!.Tick += tickObserver;
        }
        internal void RequireActiveTick(int number) => require(ticks.Any(tick => tick.Number == number && tick.Active), "原同一次timer Tick的API只读后置观察器确认HasActiveRequests=true");

        internal async Task Cancel(AppApiPendingGate gate)
        {
            require(gate.Claimed && gate.Request is { IsCompleted: false } && !gate.Release.Task.IsCompleted, "仅取消真实在途HTTP，未释放provider TCS");
            gate.CancellationExpected = true; gate.RequestCancellation.Cancel();
            try { await gate.Request!.WaitAsync(TimeSpan.FromSeconds(5)); throw new AppPendingProductFailure("已取消HTTP请求却返回普通成功"); }
            catch (OperationCanceledException) when (gate.Request!.IsCanceled && gate.RequestCancellation.IsCancellationRequested) { gate.CancellationObserved = true; }
            try { await gate.Exited.Task.WaitAsync(TimeSpan.FromSeconds(5)); await Until(() => !Vm.Runtime.Api.HasActiveRequests); }
            catch (TimeoutException error) { throw new AppPendingProductFailure("客户端取消后原provider或服务器请求计数未释放", error); }
            product(gate.ProviderTask is { IsCanceled: true } && !gate.Release.Task.IsCompleted && !Vm.Runtime.Api.HasActiveRequests, "真实RequestAborted取消provider等待，原Task已取消且计数零；不是释放TCS伪装取消");
            Observe("http-cancellation-observed", new { gate.Serial, gate.Marker });
        }

        internal async Task Cleanup()
        {
            if (cleaned) throw new InvalidOperationException("自有API不得重复清理"); cleaned = true; var errors = new List<Exception>();
            AppApiPendingGate[] original; lock (sync) original = gates.ToArray();
            foreach (var gate in original)
            {
                gate.Release.TrySetResult(); if (!gate.Claimed) { gate.Entered.TrySetCanceled(); gate.Exited.TrySetCanceled(); }
            }
            foreach (var gate in original)
            {
                if (gate.Request is not null) try { await gate.Request.WaitAsync(TimeSpan.FromSeconds(5)); } catch (OperationCanceledException) when (gate.CancellationExpected && gate.CancellationObserved && gate.Request.IsCanceled) { } catch (Exception error) { errors.Add(error); cancellation.Cancel(); }
                if (gate.ProviderTask is not null)
                {
                    try { await gate.ProviderTask.WaitAsync(TimeSpan.FromSeconds(5)); }
                    catch (Exception error) when (ReferenceEquals(error, gate.Failure) && gate.ProviderTask.IsFaulted) { Observe("expected-provider-failure-drained", new { gate.Marker, error.Message }); }
                    catch (OperationCanceledException) when (gate.CancellationExpected && gate.ProviderTask.IsCanceled) { }
                    catch (Exception error) { errors.Add(error); cancellation.Cancel(); }
                }
            }
            if (start is not null) try { await start.WaitAsync(TimeSpan.FromSeconds(5)); } catch (Exception error) { errors.Add(error); cancellation.Cancel(); }
            if (close is not null) try { await close.WaitAsync(TimeSpan.FromSeconds(5)); } catch (Exception error) { errors.Add(error); }
            if (viewModel() is not null)
            {
                try { await Until(() => !Vm.Runtime.Api.HasActiveRequests); } catch (Exception error) { errors.Add(error); }
                Vm.Runtime.Automation.ServiceInfoProvider = null;
                if (propertyObserver is not null) Vm.PropertyChanged -= propertyObserver;
            }
            if (drainObserver is not null && subscribedApi is not null) subscribedApi.RequestsDrained -= drainObserver;
            if (original.Any(gate => !gate.Entered.Task.IsCompleted || !gate.Release.Task.IsCompleted || !gate.Exited.Task.IsCompleted || gate.Request is { IsCompleted: false } || gate.ProviderTask is { IsCompleted: false })) errors.Add(new InvalidOperationException("自有HTTP原Task/provider/TCS未排空"));
            if (tickObserver is not null && timer is not null) timer.Tick -= tickObserver;
            foreach (var gate in original) gate.RequestCancellation.Dispose();
            client?.Dispose(); cancellation.Dispose(); Observe("api-cleanup-finished");
            if (errors.Count != 0) throw new AggregateException("应用API挂起读取自有清理失败", errors);
        }

        internal object Report()
        {
            AppApiPendingGate[] original; lock (sync) original = gates.ToArray();
            return new { port, startStatus = start?.Status.ToString(), closeStatus = close?.Status.ToString(), cleaned, notices = Notices.ToArray(), drainNotices = drainNotices.ToArray(), observations = events.ToArray(), ticks = ticks.Select(tick => new { tick.Number, tick.Active, tick.At }).ToArray(), gates = original.Select(gate => new { gate.Serial, gate.Marker, gate.Claimed, gate.CancellationExpected, gate.CancellationObserved, failure = gate.Failure?.Message, entered = gate.Entered.Task.Status.ToString(), released = gate.Release.Task.Status.ToString(), exited = gate.Exited.Task.Status.ToString(), provider = gate.ProviderTask?.Status.ToString(), request = gate.Request?.Status.ToString(), response = gate.Request is { Status: TaskStatus.RanToCompletion } ? gate.Request.Result : null }).ToArray() };
        }
    }
}
