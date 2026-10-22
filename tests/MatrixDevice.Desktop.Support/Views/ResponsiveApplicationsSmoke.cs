using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using MatrixDevice.Core;
using MatrixDevice.Features;
using MatrixDevice.Host;
using Path = System.IO.Path;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    private async Task VerifyResponsiveApplicationsAsync()
    {
        if (!ViewModel.Demo || !ViewModel.Runtime.IsDemo || !Dispatcher.CheckAccess() || Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
            throw new InvalidOperationException("应用正文验收仅限自有一致 Demo/STA。");
        var directory = Path.Combine(Path.GetFullPath(ViewModel.DataDirectory), "artifacts", "al", Guid.NewGuid().ToString("N")[..8]);
        if (!Path.IsPathFullyQualified(directory) || directory.Length > 185) throw new InvalidOperationException("应用布局验收需要短绝对 artifacts 路径。");
        Directory.CreateDirectory(directory);
        var main = Application.Current.MainWindow; var shutdown = Application.Current.ShutdownMode; var language = LocalizationService.Current.Language;
        var reports = new List<object>(); var failures = new List<Exception>(); Exception? failure = null; var number = 0;
        var planned = new List<string>();
        var sizes = new[] { (W: 390, H: 640), (W: 390, H: 844), (W: 768, H: 1024), (W: 1000, H: 700), (W: 1440, H: 900), (W: 1920, H: 1080) };
        planned.AddRange(sizes.Select(size => $"geometry-{size.W}-{size.H}"));
        planned.AddRange(["empty", "in-flight-cancel", "failure-recovery", "resize-state", "glyph-150", "header-visible-selection"]);
        try
        {
            foreach (var size in sizes)
                await Case($"geometry-{size.W}-{size.H}", size.W, size.H, async state =>
                {
                    await state.Stage("loaded", state.LoadMany);
                    await state.Stage("geometry", state.Geometry);
                    await state.Stage("applications-scroll", () => state.ReadTable("应用", state.Apps));
                    await state.Stage("targets-scroll", () => state.ReadTable("目标", state.Targets));
                    await state.Stage("tasks-scroll", () => state.ReadTable("任务", state.Tasks));
                    await state.Stage("no-extra-backend", () => { state.SameCalls(); state.Context(); return Task.CompletedTask; });
                });
            await Case("empty", 390, 640, async state =>
            {
                await state.Stage("empty-original-state", async () =>
                {
                    state.Product(state.Window.applicationRows.Count == 0 && state.Window.applicationTasks.Count == 0 && state.Window.applicationTargets.All(row => !row.IsSelected), "原集合空态与独立空选择");
                    state.Product(!state.Tool("读取应用").IsEnabled && !state.Tool("安装 APK").IsEnabled && !state.Tool("卸载所选").IsEnabled && !state.Window.applicationCancel.IsEnabled, "空态不允许设备命令");
                    state.Product(state.Backend.ListCalls == 0 && state.Backend.Calls().All(call => call.Operation == "Discover"), "空态不偷偷读取或写设备");
                    await state.Geometry(); state.Save("empty");
                });
            });
            await Case("in-flight-cancel", 390, 844, async state =>
            {
                await state.Stage("in-flight", async () =>
                {
                    await state.Resize(1440, 900); var gate = state.Backend.Hold(); await state.SelectTargets();
                    await state.Wait(() => gate.Entered.Task.IsCompleted, "原自动 List 未进入门");
                    state.Product(gate.Claimed && state.Window.applicationRunning && state.Vm.Busy && state.Backend.ActiveLists == 1 && state.Backend.ListCalls == 1, "真实唯一 List Task/TCS 在途而非伪造 Busy");
                    await state.Resize(state.RequestedWidth, state.RequestedHeight); await state.Pane("任务");
                    state.Product(state.Window.applicationCancel.IsEnabled && !state.Window.applicationTargetPanel.IsEnabled, "在途停止后续可用且原目标禁止改选");
                    await state.InFlightSegmentFocus();
                    state.Hit44(state.Window.applicationCancel); state.Save("in-flight");
                    await state.Invoke(state.Window.applicationCancel); state.Product(state.Window.applicationCancellation?.IsCancellationRequested == true, "原停止后续按钮实际取消批次 token");
                    state.Product(state.Backend.ActiveLists == 1 && !gate.Returned.Task.IsCompleted, "停止后续不虚报当前 List 已停止");
                    gate.Release.TrySetResult(); await state.Finished();
                    state.Product(state.Backend.ListCalls == 1 && state.Window.applicationTasks.Count == 12 && state.Window.applicationTasks.Count(row => row.ResultState == ApplicationTaskState.Succeeded) == 1 && state.Window.applicationTasks.Count(row => row.ResultState == ApplicationTaskState.Cancelled) == 11, "当前项完成、11 个后续项取消且未发命令");
                    state.Product(!state.Window.applicationCancel.IsEnabled && state.Window.applicationTasks.All(row => row.Terminal), "终态停止后续禁用、任务全部终态"); state.Context(); state.Save("cancelled");
                });
            });
            await Case("failure-recovery", 390, 640, async state =>
            {
                await state.Stage("failure", async () =>
                {
                    await state.Resize(1440, 900); await state.Pane("目标"); var gate = state.Backend.Hold(new DeviceException(LayoutAppsBackend.FailureMarker));
                    await state.SelectOne(); await state.Wait(() => gate.Entered.Task.IsCompleted, "单次失败 List 未进入门"); gate.Release.TrySetResult(); await state.Finished();
                    state.Product(state.Window.applicationTasks.Count == 1 && state.Window.applicationTasks[0].ResultState == ApplicationTaskState.Failed && state.Window.applicationTasks[0].Message == LayoutAppsBackend.FailureMarker && state.Vm.Error.Length > 0 && state.Window.applicationRows.Count == 0, "真实单次失败沿原逐项结果及安全错误反馈");
                    await state.Resize(state.RequestedWidth, state.RequestedHeight); await state.Pane("任务"); await state.ReadLongResult(); state.Save("failure");
                });
                await state.Stage("recovery", async () =>
                {
                    await state.Pane("应用"); await state.Invoke(state.Tool("读取应用")); await state.Finished();
                    state.Product(state.Backend.ListCalls == 2 && state.Window.applicationTasks.Single().ResultState == ApplicationTaskState.Succeeded && state.Window.applicationRows.Count == 8 && state.Vm.Error.Length == 0, "原读取按钮新意图实际恢复，不是改假集合"); state.Context(); state.Save("recovered");
                });
            });
            await Case("resize-state", 1000, 700, async state =>
            {
                await state.Stage("loaded", state.LoadMany);
                await state.Stage("identity-and-context", async () =>
                {
                    await state.Resize(1440, 900); await state.Pane("应用"); await state.SelectApplication();
                    await state.Invoke(state.Window.applicationSystemToggle); state.Product(state.Window.applicationSystemToggle.IsChecked == true, "原系统过滤 Toggle 实际显示系统行");
                    state.Window.applicationRowsView.SortDescriptions.Add(new(nameof(InstalledApplicationRow.PackageName), ListSortDirection.Descending)); await state.Settle();
                    var snapshot = state.Snapshot(); var calls = state.Backend.Calls();
                    var focused = state.Apps.Focus(); await state.Settle(); var actual = Keyboard.FocusedElement as DependencyObject;
                    state.Observe("focus-before-resize", new { focused, state.Apps.IsKeyboardFocusWithin, actualType = actual?.GetType().Name, state.Window.IsActive });
                    state.Product(focused && state.Apps.IsKeyboardFocusWithin && actual is UIElement { IsVisible: true } && (ReferenceEquals(actual, state.Apps) || state.Apps.IsAncestorOf(actual)), "一次逻辑 Focus 后原可见应用表持有焦点，不抢原生焦点");
                    await state.Resize(390, 640); actual = Keyboard.FocusedElement as DependencyObject;
                    state.Product(state.Apps.IsVisible && state.Apps.IsKeyboardFocusWithin && actual is UIElement { IsVisible: true } && (ReferenceEquals(actual, state.Apps) || state.Apps.IsAncestorOf(actual)), "宽屏原应用表焦点缩窄后仍归可见原表，不靠隐藏活动区"); snapshot.Same(state);
                    foreach (var size in new[] { (W: 390d, H: 640d), (W: 768d, H: 1024d), (W: 1440d, H: 900d), (W: 1000d, H: 700d) })
                    {
                        await state.Resize(size.W, size.H); foreach (var role in new[] { "目标", "任务", "应用" }) { await state.Pane(role); snapshot.Same(state); }
                    }
                    var workspace = state.CommonWorkspace(); var chrome = state.Window.ActualWidth - workspace.ActualWidth;
                    foreach (var width in new[] { 639d, 640d, 641d }) { await state.Resize(width + chrome, 700); state.Product(Math.Abs(workspace.ActualWidth - width) < .6, "实际正文 640 断点正负 1：" + width); snapshot.Same(state); }
                    state.Product(state.Backend.Calls().SequenceEqual(calls), "全部纯 resize/切段/排序/过滤未新增 backend 请求");
                    await state.Pane("应用"); state.Save("state");
                    await state.AppSegmentFocus(); snapshot.Same(state); state.Product(state.Backend.Calls().SequenceEqual(calls), "新增窄段焦点宽化合同不触发 backend");
                });
            });
            await Case("glyph-150", 390, 844, async state =>
            {
                await state.Stage("loaded", state.LoadMany);
                await state.Stage("glyph-layout", async () =>
                {
                    state.GlyphScale = 1.5; await state.ApplyGlyphScale(); await state.Geometry();
                    await state.ReadTable("应用", state.Apps); await state.ReadTable("目标", state.Targets); await state.ReadTable("任务", state.Tasks);
                    state.RecordGlyphScaleDiagnostic();
                    var currentFonts = VisualDescendants<FrameworkElement>(state.Page).Where(element => element is Control or TextBlock).ToArray();
                    state.Product(currentFonts.Length > 20 && currentFonts.All(element => state.Fonts.TryGetValue(element, out var baseline) && Math.Abs(LayoutAppsCase.FontSize(element) - baseline * 1.5) < .01), "当前页面全部字体控件已登记且实际 150%，包括隐藏成员，不是整页 ScaleTransform");
                    state.Product(state.Window.applicationPage.LayoutTransform.Value.IsIdentity && state.Window.applicationPage.RenderTransform.Value.IsIdentity, "应用正文不整页缩放取绿"); state.SameCalls(); state.Save("glyph-150");
                });
            });
            await Case("header-visible-selection", 390, 640, async state =>
            {
                await state.Stage("loaded", state.LoadMany);
                await state.Stage("header-visible-selection", async () =>
                {
                    await state.Pane("应用"); var header = state.Apps.Columns[0].Header as CheckBox; state.Require(header is not null, "原应用选择列头提供唯一原 CheckBox");
                    var snapshot = state.Snapshot(); var calls = state.Backend.Calls(); state.HeaderHit44(header!);
                    await state.InvokeHeader(header!);
                    state.Product(header!.IsChecked == true && state.Window.applicationRows.Count(row => row.IsSelected && !row.App.IsSystem) == 84 && state.Window.applicationRows.Where(row => row.App.IsSystem).All(row => !row.IsSelected), "原表头 peer 全选 84 可见用户应用，不勾选 12 个隐藏系统应用");
                    await state.InvokeHeader(header); state.Product(header.IsChecked == false && state.Window.applicationRows.All(row => !row.IsSelected), "原表头 peer 清除可见应用");
                    await state.Invoke(state.Window.applicationSystemToggle); state.Product(state.Window.applicationRowsView.Cast<object>().Count() == 96, "原系统过滤显示全部 96 行");
                    await state.InvokeHeader(header); state.Product(header.IsChecked == true && state.Window.applicationRows.All(row => row.IsSelected), "原表头 peer 全选包含系统过滤后的 96 可见行");
                    await state.InvokeHeader(header); state.Product(header.IsChecked == false && state.Window.applicationRows.All(row => !row.IsSelected), "原表头 peer 清除全部 96 可见行");
                    await state.Invoke(state.Window.applicationSystemToggle); snapshot.Same(state); state.SameCalls();
                    state.Product(state.Backend.Calls().SequenceEqual(calls), "表头勾选/清除及原系统过滤不增加 backend 调用"); state.Context(); state.Save("header-selection");
                });
            });
            if (failures.Count != 0) throw new AggregateException("应用正文响应式独立用例失败", failures);
        }
        catch (Exception error) { failure = error; Console.Error.WriteLine(error); }
        finally
        {
            Application.Current.MainWindow = main; Application.Current.ShutdownMode = shutdown; LocalizationService.Current.SetLanguage(language);
            try { await File.WriteAllTextAsync(Path.Combine(directory, "application-layout.json"), JsonSerializer.Serialize(new { status = failure is null ? "PASS-OFFLINE-WPF-APPLICATION-LAYOUT" : "FAIL", scope = "Own Demo HWND, original bindings/timer/commands, nonforwarding backend, logical WPF peers/Focus/hit tests; no physical input, native file chooser, Android or system DPI proof", planned, reports, notRun = planned.Skip(reports.Count), oldSmoke = failure is null ? "CONTINUE-ORIGINAL-ORDER" : "NOT-RUN", error = failure?.ToString() }, JsonDefaults.Options)); }
            catch (Exception error) { failure = failure is null ? error : new AggregateException(failure, error); }
        }
        if (failure is not null) ExceptionDispatchInfo.Capture(failure).Throw();

        async Task Case(string name, int width, int height, Func<LayoutAppsCase, Task> action)
        {
            var state = new LayoutAppsCase(width, height, Path.Combine(directory, "c" + (++number).ToString("00"))); Exception? error = null; Exception? cleanup = null;
            state.Expect(name.StartsWith("geometry-", StringComparison.Ordinal) ? ["loaded", "geometry", "applications-scroll", "targets-scroll", "tasks-scroll", "no-extra-backend"] : name switch { "empty" => ["empty-original-state"], "in-flight-cancel" => ["in-flight"], "failure-recovery" => ["failure", "recovery"], "resize-state" => ["loaded", "identity-and-context"], "header-visible-selection" => ["loaded", "header-visible-selection"], _ => ["loaded", "glyph-layout"] });
            try { await state.Initialize(this); await action(state); if (state.Failures.Count > 0) throw new AggregateException(state.Failures); }
            catch (Exception caught) { error = caught; state.Observe("case-failure", caught.ToString()); try { if (state.Window?.IsVisible == true) state.Save("failure"); } catch (Exception capture) { error = new AggregateException(caught, capture); } }
            finally
            {
                try { await state.Cleanup(); } catch (Exception caught) { cleanup = caught; }
                var failureClasses = state.Stages.Values.Where(value => value is "PRODUCT-FAIL" or "PRECONDITION-FAIL" or "FIXTURE-FAIL").ToList();
                if (state.Failures.Count > 0 || error is LayoutAppsProductFailure) failureClasses.Add("PRODUCT-FAIL");
                if (error is LayoutAppsPreconditionFailure) failureClasses.Add("PRECONDITION-FAIL");
                else if (error is not null && error is not LayoutAppsProductFailure && !(error is AggregateException aggregate && state.Failures.Count > 0 && aggregate.InnerExceptions.SequenceEqual(state.Failures))) failureClasses.Add("FIXTURE-FAIL");
                if (cleanup is not null) failureClasses.Add("CLEANUP-FAIL");
                var classes = failureClasses.Distinct(StringComparer.Ordinal).ToArray();
                var status = classes.Length > 1 ? "MIXED-FAIL" : classes.SingleOrDefault() ?? "PASS";
                reports.Add(new { name, status, failureClasses = classes, cleaned = cleanup is null, stages = state.Stages, checks = state.Checks, observations = state.Observations, calls = state.Backend.Calls(), gates = state.Backend.GateReport(), tasks = state.Backend.TaskReport(), error = error?.ToString(), cleanup = cleanup?.ToString() });
            }
            if (error is not null || cleanup is not null) failures.Add(new InvalidOperationException(name, new AggregateException(new[] { error, cleanup }.OfType<Exception>())));
            if (cleanup is not null) throw new AggregateException("应用正文自有清理失败，余项 NOT-RUN", failures);
        }
    }

    private sealed class LayoutAppsProductFailure(string message) : Exception(message) { }
    private sealed class LayoutAppsPreconditionFailure(string message) : Exception(message) { }

    private sealed class LayoutAppsCase(int requestedWidth, int requestedHeight, string directory)
    {
        internal MainWindow Window = null!; internal WorkspaceViewModel Vm = null!; internal readonly LayoutAppsBackend Backend = new();
        internal int RequestedWidth => requestedWidth; internal int RequestedHeight => requestedHeight;
        internal readonly List<string> Checks = []; internal readonly List<object> Observations = []; internal readonly List<Exception> Failures = [];
        internal readonly Dictionary<string, string> Stages = []; internal readonly Dictionary<DependencyObject, double> Fonts = [];
        private readonly HashSet<string> tooltipHeightCaptures = [];
        internal double GlyphScale = 1;
        private MatrixRuntime? runtime; private CatalogRepository? metadata; private RollingLog? log; private ScriptScheduler? scripts; private InputCoordinator? input; private ApiServiceManager? api;
        private DeviceRow? current; private DeviceRow? main; private DeviceRow[] devices = []; private DeviceRow[] globalTargets = []; private bool[] selections = []; private UnifiedDeviceRow[] unified = []; private bool[] unifiedSelection = [];
        private LayoutAppsCall[]? frozenCalls; private bool cleaned;
        internal DataGrid Targets => VisualDescendants<DataGrid>(Window.applicationPage).Single(grid => ReferenceEquals(grid.ItemsSource, Window.applicationTargets));
        internal DataGrid Apps => VisualDescendants<DataGrid>(Window.applicationPage).Single(grid => ReferenceEquals(grid.ItemsSource, Window.applicationRowsView));
        internal DataGrid Tasks => VisualDescendants<DataGrid>(Window.applicationPage).Single(grid => ReferenceEquals(grid.ItemsSource, Window.applicationTasks));
        internal FrameworkElement Page => Window.applicationPage;
        internal async Task Initialize(MainWindow owner)
        {
            Directory.CreateDirectory(directory);
            try
            {
                metadata = new CatalogRepository(Path.Combine(directory, "db.sqlite3")); log = new RollingLog(Path.Combine(directory, "logs")); var catalog = new DeviceCatalog(Backend); var automation = new AutomationService(Backend, catalog);
                scripts = new ScriptScheduler(Backend); input = new InputCoordinator(catalog, Backend); api = new ApiServiceManager(catalog, Backend, automation, log);
                var settings = new AppSettings { Language = "zh-CN", AutoStartApi = false, AutoConnect = false, SyncMouse = false, SyncKeyboard = false, WindowWidth = requestedWidth, WindowHeight = requestedHeight };
                runtime = new MatrixRuntime { Backend = Backend, Devices = catalog, Automation = automation, Scripts = scripts, Input = input, Api = api, Metadata = metadata, Log = log, Settings = settings, FrozenConnectionSettings = settings, SettingsStore = new SettingsStore(Path.Combine(directory, "AppConfig.json")), IsDemo = true }; api.DeviceSnapshotProvider = runtime.GetDevices;
                Vm = new WorkspaceViewModel(runtime, directory, true); await Vm.RefreshDevicesAsync().WaitAsync(TimeSpan.FromSeconds(5));
                await Vm.SetMainAsync(Vm.Devices[1]).WaitAsync(TimeSpan.FromSeconds(5)); Vm.CurrentDevice = Vm.Devices[1]; Vm.Devices[1].IsSelected = true; Vm.UpdateSelection();
                Window = new MainWindow(Vm) { Owner = owner, ShowActivated = true }; Window.captureTimer.Stop(); Window.directoryTimer.Stop(); Window.monitorTimer.Stop(); Window.Show(); await Settle();
                Require(new WindowInteropHelper(Window).Handle != IntPtr.Zero && Window.IsLoaded && Math.Abs(Window.ActualWidth - requestedWidth) < .6 && Math.Abs(Window.ActualHeight - requestedHeight) < .6, "自有实际 HWND 尺寸不 Clamp");
                Window.navigation["应用"].RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent)); await Settle();
                Require(Vm.Demo && runtime.IsDemo && ReferenceEquals(runtime.Backend, Backend) && !api.BasicRunning && !api.AutomatorRunning && api.DevicePorts.Count == 0 && !Window.DebugAutomaticFixturePermitted && !Window.ReconnectLoadingFixturePermitted, "一致自有 Demo，非转发 stub，API 与自动节点许可未开启");
                Require(Equals((Window.tabs.SelectedItem as TabItem)?.Tag, "应用") && Page.IsVisible && Window.applicationTargets.Count == 12 && Window.applicationRows.Count == 0 && Window.applicationReadTimer is null, "原应用导航、12 目标与空集合前置");
                current = Vm.CurrentDevice; main = Vm.MainDevice; devices = Vm.Devices.ToArray(); selections = devices.Select(row => row.IsSelected).ToArray(); globalTargets = Vm.Targets().ToArray(); unified = Window.unifiedDevices.ToArray(); unifiedSelection = unified.Select(row => row.IsSelected).ToArray();
                Observe("initialized", new { hwnd = new WindowInteropHelper(Window).Handle.ToInt64(), requestedWidth, requestedHeight, systemDpi = VisualTreeHelper.GetDpi(Window).DpiScaleX });
            }
            catch (LayoutAppsPreconditionFailure) { throw; }
            catch (Exception error) { throw new LayoutAppsPreconditionFailure("应用正文自有初始化失败：" + error); }
        }
        internal async Task Stage(string name, Func<Task> action)
        {
            Stages[name] = "STARTED";
            try { await action(); Stages[name] = "PASS"; }
            catch (LayoutAppsProductFailure error) { Stages[name] = "PRODUCT-FAIL"; Failures.Add(error); Observe("product-failure", new { stage = name, error.Message }); }
            catch (LayoutAppsPreconditionFailure) { Stages[name] = "PRECONDITION-FAIL"; throw; }
            catch { Stages[name] = "FIXTURE-FAIL"; throw; }
        }
        internal void Expect(string[] names) { foreach (var name in names) Stages.Add(name, "NOT-RUN"); }
        internal void Product(bool value, string message) { if (!value) { Observe("product-check-failed", message); throw new LayoutAppsProductFailure(message); } Checks.Add("PRODUCT " + message); }
        internal void Require(bool value, string message) { if (!value) throw new LayoutAppsPreconditionFailure(message); Checks.Add("PRECONDITION " + message); }
        internal async Task Settle()
        {
            await Window.Dispatcher.InvokeAsync(Window.UpdateLayout, DispatcherPriority.Render).Task.WaitAsync(TimeSpan.FromSeconds(5));
            await Window.Dispatcher.InvokeAsync(Window.UpdateLayout, DispatcherPriority.ContextIdle).Task.WaitAsync(TimeSpan.FromSeconds(5));
            await Window.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle).Task.WaitAsync(TimeSpan.FromSeconds(5));
        }
        internal async Task Wait(Func<bool> predicate, string message)
        {
            var watch = Stopwatch.StartNew(); while (!predicate()) { if (watch.Elapsed > TimeSpan.FromSeconds(5)) throw new LayoutAppsProductFailure(message); await Settle(); await Task.Delay(5); } await Settle();
        }
        internal async Task Resize(double width, double height)
        {
            Window.Width = width; Window.Height = height; await Settle(); await ApplyGlyphScale();
            Product(Math.Abs(Window.ActualWidth - width) < .6 && Math.Abs(Window.ActualHeight - height) < .6, "实际 HWND resize 不 Clamp：" + width + "x" + height); Observe("resize");
        }
        internal Button Tool(string caption) => VisualDescendants<Button>(Page).Single(button => AutomationProperties.GetName(button) == caption || button.ToolTip as string == caption);
        internal async Task Invoke(ButtonBase button)
        {
            Product(button.IsVisible && button.IsEnabled && DeviceProbeContains(Clip(button), DeviceProbeBounds(button, Window.Shell)), "只调用实际可见启用且完整位于祖先视口的原控件：" + AutomationProperties.GetName(button));
            var point = button.TranslatePoint(new Point(button.ActualWidth / 2, button.ActualHeight / 2), Window.Shell); var hit = Window.Shell.InputHitTest(point) as DependencyObject;
            Product(hit is not null && (ReferenceEquals(hit, button) || button.IsAncestorOf(hit)), "原控件中心命中归属一致");
            var peer = UIElementAutomationPeer.CreatePeerForElement(button) ?? throw new LayoutAppsPreconditionFailure("原控件缺少逻辑 peer");
            if (peer.GetPattern(PatternInterface.Invoke) is IInvokeProvider invoke) invoke.Invoke(); else if (peer.GetPattern(PatternInterface.Toggle) is IToggleProvider toggle) toggle.Toggle(); else throw new LayoutAppsPreconditionFailure("原控件缺少 Invoke/Toggle 模式");
            await Settle(); Observe("logical-invoke", new { caption = AutomationProperties.GetName(button), physicalInput = false });
        }
        internal async Task Pane(string role)
        {
            var candidates = VisualDescendants<ToggleButton>(Page).Where(button => button is not CheckBox && (AutomationProperties.GetName(button) == role || button.Content as string == role)).ToArray();
            if (candidates.Length == 0) { Observe("original-no-segment", role); return; }
            Require(candidates.Length == 1, "公开分段角色唯一：" + role); var button = candidates[0];
            if (!button.IsVisible) { Observe("wide-no-segment", role); return; }
            if (button.IsChecked != true) await Invoke(button); await ApplyGlyphScale();
            Product((role == "目标" ? Targets : role == "任务" ? Tasks : Apps).IsVisible, "公开分段显示原表：" + role);
        }
        private ToggleButton? FocusSegment(string role)
        {
            var candidates = VisualDescendants<ToggleButton>(Page).Where(button => button is not CheckBox && (AutomationProperties.GetName(button) == role || button.Content as string == role)).ToArray();
            if (candidates.Length == 0) { Observe("segment-focus", new { role, status = "NOT-APPLICABLE", reason = "Original page has no segment; no baseline claim added" }); return null; }
            Require(candidates.Length == 1, "新增焦点合同原分段角色唯一：" + role); return candidates[0];
        }
        internal void HeaderHit44(CheckBox header)
        {
            // Column headers are outside the row ScrollContentPresenter, not outside the grid.
            var clip = Rect.Intersect(PageClip(), DeviceProbeBounds(Apps, Window.Shell));
            DeviceProbe44(header, Window.Shell, clip, Product);
        }
        internal async Task InvokeHeader(CheckBox header)
        {
            Product(header.IsEnabled, "原可见表头 CheckBox 启用"); HeaderHit44(header);
            var peer = new CheckBoxAutomationPeer(header); var toggle = peer.GetPattern(PatternInterface.Toggle) as IToggleProvider;
            Require(toggle is not null, "原表头 CheckBox 提供 Toggle 模式"); toggle!.Toggle(); await Settle();
            Observe("header-logical-toggle", new { header.IsChecked, physicalInput = false });
        }
        private async Task FocusOnce(ToggleButton button)
        {
            Product(button.IsVisible && button.IsEnabled && DeviceProbeContains(Clip(button), DeviceProbeBounds(button, Window.Shell)), "新增焦点合同仅操作可见启用原分段");
            var focused = button.Focus(); await Settle(); var actual = Keyboard.FocusedElement as DependencyObject;
            Observe("segment-focused-once", new { caption = AutomationProperties.GetName(button), focused, button.IsKeyboardFocusWithin, actualType = actual?.GetType().Name, physicalInput = false });
            Product(focused && button.IsKeyboardFocusWithin && actual is UIElement { IsVisible: true } && (ReferenceEquals(actual, button) || button.IsAncestorOf(actual)), "原分段一次逻辑 Focus 实际生效，不 Activate 或重试");
        }
        internal async Task AppSegmentFocus()
        {
            await Resize(390, 640); await Pane("应用"); var button = FocusSegment("应用"); if (button is null) return;
            await FocusOnce(button); await Resize(1440, 900); var actual = Keyboard.FocusedElement as DependencyObject;
            Observe("application-segment-widened", new { Apps.IsVisible, Apps.IsKeyboardFocusWithin, actualType = actual?.GetType().Name });
            Product(Apps.IsVisible && Apps.IsKeyboardFocusWithin && actual is UIElement { IsVisible: true } && (ReferenceEquals(actual, Apps) || Apps.IsAncestorOf(actual)), "窄应用分段焦点宽化后落可见原应用表，不遗留隐藏分段");
        }
        internal async Task InFlightSegmentFocus()
        {
            var button = FocusSegment("目标"); if (button is null) return;
            await Pane("目标"); await FocusOnce(button); Product(!Window.applicationTargetPanel.IsEnabled && Window.applicationCancel.IsEnabled, "焦点宽化前原目标区被真实批次禁用，停止后续仍可用");
            await Resize(1440, 900); var actual = Keyboard.FocusedElement as DependencyObject;
            Observe("disabled-target-segment-widened", new { Window.applicationCancel.IsVisible, Window.applicationCancel.IsKeyboardFocusWithin, actualType = actual?.GetType().Name });
            Product(Window.applicationCancel.IsVisible && Window.applicationCancel.IsEnabled && Window.applicationCancel.IsKeyboardFocusWithin && actual is UIElement { IsVisible: true } && (ReferenceEquals(actual, Window.applicationCancel) || Window.applicationCancel.IsAncestorOf(actual)), "在途目标段宽化不能聚焦禁用目标表，实际焦点落可见停止后续");
            await Resize(requestedWidth, requestedHeight); await Pane("任务");
        }
        internal async Task SelectOne()
        {
            await Pane("目标"); var row = Window.applicationTargets[0]; Targets.ScrollIntoView(row); await Settle();
            var content = Targets.Columns[0].GetCellContent(row); Require(content is not null, "原目标选择单元格已实现");
            var check = content as CheckBox ?? VisualDescendants<CheckBox>(content!).Single(); var binding = check.GetBindingExpression(ToggleButton.IsCheckedProperty);
            Require(binding is not null && ReferenceEquals(binding.DataItem, row) && binding.ParentBinding.Mode == BindingMode.TwoWay && binding.ParentBinding.Path.Path == nameof(ApplicationTargetRow.IsSelected) && !row.IsSelected, "原目标选择 TwoWay 绑定原行");
            await Invoke(check); Product(row.IsSelected && check.IsChecked == true && Window.applicationReadPending && Window.applicationReadTimer is { IsEnabled: true } && Window.applicationReadTimer.Interval == TimeSpan.FromMilliseconds(350), "原 CheckBox 经 TwoWay 建立原 350 ms 待读意图");
        }
        internal async Task SelectTargets() { await SelectOne(); await Invoke(Tool("全选可用")); Product(Window.applicationTargets.All(row => row.IsSelected), "原全选可用按钮只改变应用独立目标"); }
        internal async Task Finished() { await Wait(() => !Window.applicationRunning && !Vm.Busy && Backend.ActiveLists == 0 && Backend.ListTasksCompleted, "原应用批次与后端 Task 未完成"); Product(!Window.applicationReadPending && Window.applicationReadTimer?.IsEnabled != true, "批次结束原 pending 与 timer 排空"); }
        internal async Task LoadMany()
        {
            await Resize(1440, 900); await SelectTargets(); await Wait(() => Backend.ListCalls == 12 && !Window.applicationRunning && !Vm.Busy, "原 350 ms 批次未完成 12 个 List"); await Finished();
            Product(Window.applicationRows.Count == 96 && Window.applicationRowsView.Cast<object>().Count() == 84 && Window.applicationTasks.Count == 12 && Window.applicationTasks.All(row => row.ResultState == ApplicationTaskState.Succeeded), "真实 12 个 List 发布 96 应用/84 用户应用与 12 任务");
            frozenCalls = Backend.Calls(); await Resize(requestedWidth, requestedHeight); Context(); Save("loaded");
        }
        internal async Task SelectApplication()
        {
            var row = Window.applicationRowsView.Cast<InstalledApplicationRow>().First(); Apps.ScrollIntoView(row); await Settle(); var content = Apps.Columns[0].GetCellContent(row); Require(content is not null, "原应用选择单元格已实现");
            var check = content as CheckBox ?? VisualDescendants<CheckBox>(content!).Single(); var binding = check.GetBindingExpression(ToggleButton.IsCheckedProperty);
            Require(binding is not null && ReferenceEquals(binding.DataItem, row) && binding.ParentBinding.Mode == BindingMode.TwoWay, "原应用选择 TwoWay 绑定原行"); await Invoke(check); Product(row.IsSelected, "原应用勾选实际生效");
        }
        internal Rect PageClip() => Rect.Intersect(new Rect(Window.Shell.RenderSize), DeviceProbeBounds(Page, Window.Shell));
        internal Rect Clip(FrameworkElement element)
        {
            var clip = PageClip();
            for (DependencyObject? value = element; value is not null && !ReferenceEquals(value, Window.Shell); value = VisualTreeHelper.GetParent(value))
            {
                if (value is FrameworkElement parent && parent.ClipToBounds) clip.Intersect(DeviceProbeBounds(parent, Window.Shell));
                if (value is ScrollViewer scroll) clip.Intersect(DeviceProbeViewport(Window.Shell, scroll));
            }
            return clip;
        }
        internal void Hit44(Control control) => DeviceProbe44(control, Window.Shell, Clip(control), Product);
        internal void CompactHit(Control control)
        {
            if (Page.ActualWidth < 640) Hit44(control);
            else Product(control.IsVisible && control.ActualWidth > 0 && control.ActualHeight > 0 && DeviceProbeContains(Clip(control), DeviceProbeBounds(control, Window.Shell)), "宽屏原控件完整可见，不将默认桌面高度当作 compact 红：" + AutomationProperties.GetName(control));
        }
        internal async Task Geometry()
        {
            foreach (var role in new[] { "目标", "应用", "任务" })
            {
                await Pane(role); var grid = role == "目标" ? Targets : role == "任务" ? Tasks : Apps; var bounds = DeviceProbeBounds(grid, Window.Shell); var scroll = Scroll(grid); var viewport = DeviceProbeViewport(Window.Shell, scroll); var clip = Clip(grid);
                Observe("grid-geometry", new { role, bounds = Finite(bounds), viewport = Finite(viewport), clip = Finite(clip), grid.ActualWidth, grid.ActualHeight, scroll.ScrollableWidth, scroll.ScrollableHeight });
                Product(grid.IsVisible && grid.ActualWidth >= 140 && viewport.Height >= (role == "任务" ? 48 : 100) && viewport.Width >= 100 && DeviceProbeContains(PageClip(), bounds) && DeviceProbeContains(clip, viewport), "原表及其真实 SCP 具有可读面积且不越出正文：" + role);
                if (role == "目标") { foreach (var caption in new[] { "全选可用", "清空" }) CompactHit(Tool(caption)); }
                if (role == "应用") { foreach (var caption in new[] { "读取应用", "安装 APK", "卸载所选", "更多" }) CompactHit(Tool(caption)); CompactHit(Window.applicationSystemToggle); }
                CompactHit(Window.applicationCancel); Save("geometry-" + role);
            }
        }
        private static ScrollViewer Scroll(DataGrid grid) => VisualDescendants<ScrollViewer>(grid).Single(scroll => ReferenceEquals(scroll.TemplatedParent, grid));
        internal async Task ReadTable(string role, DataGrid grid)
        {
            await Pane(role); Product(grid.IsVisible, "只滚读可见原表：" + role); var items = grid.Items.Cast<object>().ToArray(); Product(items.Length >= 8, "滚读具有多行真实数据：" + role);
            var scroll = Scroll(grid); var maxVertical = 0d; var maxHorizontal = 0d;
            foreach (var index in new[] { 0, items.Length / 2, items.Length - 1 })
            {
                var item = items[index]; grid.ScrollIntoView(item); await Settle(); await ApplyGlyphScale();
                var row = grid.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow; Product(row is not null && row.IsVisible, "首/中/末原行已实现：" + role + ":" + index);
                foreach (var column in grid.Columns.OrderBy(column => column.DisplayIndex))
                {
                    grid.ScrollIntoView(item, column); await Settle(); await ApplyGlyphScale();
                    var content = column.GetCellContent(item); Product(content is not null && content.IsVisible, "真实列已滚入并实现：" + role + ":" + column.DisplayIndex);
                    var cell = DeviceProbeCell(content!); var viewport = Rect.Intersect(Clip(grid), DeviceProbeViewport(Window.Shell, scroll)); var cellBounds = DeviceProbeBounds(cell, Window.Shell); var visible = Rect.Intersect(cellBounds, viewport);
                    Product(!visible.IsEmpty && visible.Width >= Math.Min(36, cell.ActualWidth - 1) && visible.Height >= 12, "首/中/末真实单元格在实际祖先 SCP 交集可见：" + role);
                    if (column.DisplayIndex == 0 && role != "任务")
                    {
                        var check = content as CheckBox ?? VisualDescendants<CheckBox>(content!).Single(); CompactHit(check);
                        var binding = check.GetBindingExpression(ToggleButton.IsCheckedProperty);
                        var selected = item is ApplicationTargetRow target ? target.IsSelected : item is InstalledApplicationRow application ? application.IsSelected : throw new LayoutAppsPreconditionFailure("选择列不是原目标或应用行");
                        Product(check.Content is null && binding is not null && ReferenceEquals(binding.DataItem, item) && binding.ParentBinding.Mode == BindingMode.TwoWay && binding.ParentBinding.Path.Path == nameof(ApplicationTargetRow.IsSelected) && check.IsChecked == selected, "原选择列无文本内容，TwoWay 绑定原行且命中不变，不把模板勾选图标当数据字段");
                        Observe("selection-cell-read", new { role, index, column.DisplayIndex, check.IsChecked, binding = binding?.ParentBinding.Path.Path, textField = false, reason = "Original bound selection CheckBox; template CheckMark is not application data" });
                        continue;
                    }
                    var text = content as TextBlock ?? VisualDescendants<TextBlock>(content!).FirstOrDefault();
                    if (text is not null && !string.IsNullOrEmpty(text.Text))
                    {
                        var formatted = new FormattedText(text.Text, CultureInfo.CurrentUICulture, text.FlowDirection, new Typeface(text.FontFamily, text.FontStyle, text.FontWeight, text.FontStretch), text.FontSize, text.Foreground, VisualTreeHelper.GetDpi(text).PixelsPerDip);
                        var full = text.ActualWidth + .6 >= formatted.WidthIncludingTrailingWhitespace && text.ActualHeight + .6 >= formatted.Height && DeviceProbeContains(viewport, DeviceProbeBounds(text, Window.Shell));
                        var tooltipOwner = text.ToolTip?.ToString() == text.Text ? (FrameworkElement)text : cell.ToolTip?.ToString() == text.Text ? cell : row!.ToolTip?.ToString() == text.Text ? row : null;
                        var tooltip = tooltipOwner is not null;
                        Product(full || tooltip, "长字段可完整阅读或有完整原值 ToolTip：" + role + ":" + column.DisplayIndex);
                        if (!full && tooltipOwner is not null) await ReadTooltip(tooltipOwner, text.Text);
                        Observe("cell-read", new { role, index, column.DisplayIndex, text.Text, text.FontSize, full, tooltip, visible = Finite(visible), scroll.HorizontalOffset, scroll.VerticalOffset });
                    }
                    maxHorizontal = Math.Max(maxHorizontal, scroll.HorizontalOffset); maxVertical = Math.Max(maxVertical, scroll.VerticalOffset);
                }
            }
            Product(scroll.ScrollableHeight <= .5 || maxVertical > 0, "有纵向溢出时实际滚到后续原行：" + role);
            Product(scroll.ScrollableWidth <= .5 || maxHorizontal > 0, "有水平预算溢出时实际水平滚读而非删列：" + role); Save("read-" + role);
        }
        internal async Task ReadLongResult()
        {
            var text = Window.applicationResult; Product(text.IsVisible && text.Text.Contains("失败 1", StringComparison.Ordinal), "长失败后原批次摘要保留");
            var grid = Tasks; var item = grid.Items.Cast<object>().Single(); grid.ScrollIntoView(item, grid.Columns[^1]); await Settle();
            var content = grid.Columns[^1].GetCellContent(item); Product(content is not null && content.IsVisible, "失败结果原末列实际实现");
            var cell = DeviceProbeCell(content!); var message = content as TextBlock ?? VisualDescendants<TextBlock>(content!).Single();
            Product(message.Text == LayoutAppsBackend.FailureMarker, "失败结果原值含首尾 marker，未截断或替换");
            var owner = message.ToolTip?.ToString() == message.Text ? (FrameworkElement)message : cell.ToolTip?.ToString() == message.Text ? cell : null;
            var formatted = new FormattedText(message.Text, CultureInfo.CurrentUICulture, message.FlowDirection, new Typeface(message.FontFamily, message.FontStyle, message.FontWeight, message.FontStretch), message.FontSize, message.Foreground, VisualTreeHelper.GetDpi(message).PixelsPerDip);
            var full = message.ActualWidth + .6 >= formatted.WidthIncludingTrailingWhitespace && DeviceProbeContains(Rect.Intersect(Clip(grid), DeviceProbeViewport(Window.Shell, Scroll(grid))), DeviceProbeBounds(message, Window.Shell));
            Product(full || owner is not null, "长失败结果具有实际完整阅读路径"); if (!full) await ReadTooltip(owner!, message.Text); CompactHit(Window.applicationCancel);
        }
        private async Task ReadTooltip(FrameworkElement owner, string expected)
        {
            var attached = owner.ToolTip; Require(attached?.ToString() == expected, "ToolTip 内容来自原控件完整附加值");
            var tooltip = new ToolTip { Content = attached, PlacementTarget = owner }; Exception? primary = null;
            try
            {
                owner.SetCurrentValue(FrameworkElement.ToolTipProperty, tooltip); tooltip.IsOpen = true; await Settle();
                Observe("tooltip-before-assert", new
                {
                    expected, tooltip.IsOpen, tooltip.IsVisible, tooltip.IsLoaded,
                    actualWidth = double.IsFinite(tooltip.ActualWidth) ? (double?)tooltip.ActualWidth : null,
                    actualHeight = double.IsFinite(tooltip.ActualHeight) ? (double?)tooltip.ActualHeight : null,
                    maxWidth = double.IsFinite(tooltip.MaxWidth) ? (double?)tooltip.MaxWidth : null,
                    maxWidthFinite = double.IsFinite(tooltip.MaxWidth), hasPresentationSource = PresentationSource.FromVisual(tooltip) is not null,
                    style = tooltip.Style is null ? null : new { type = tooltip.Style.GetType().FullName, targetType = tooltip.Style.TargetType?.FullName, basedOnTargetType = tooltip.Style.BasedOn?.TargetType?.FullName, setters = tooltip.Style.Setters.Count },
                    textBlocks = VisualDescendants<TextBlock>(tooltip).Select(block => new { block.Text, wrapping = block.TextWrapping.ToString(), block.ActualWidth, block.ActualHeight, block.IsVisible, block.IsLoaded, block.FontSize }).ToArray()
                });
                var text = VisualDescendants<TextBlock>(tooltip).SingleOrDefault(block => block.Text == expected);
                Product(tooltip.IsOpen && tooltip.IsVisible && tooltip.ActualWidth > 0 && tooltip.ActualHeight > 0 && tooltip.MaxWidth <= 360 && tooltip.ActualWidth <= tooltip.MaxWidth + .6 && text is { IsVisible: true, TextWrapping: TextWrapping.Wrap }, "自有默认 WPF ToolTip 实际打开、原值完整且有界换行");
                var formatted = new FormattedText(expected, CultureInfo.CurrentUICulture, text!.FlowDirection, new Typeface(text.FontFamily, text.FontStyle, text.FontWeight, text.FontStretch), text.FontSize, text.Foreground, null, TextOptions.GetTextFormattingMode(text), VisualTreeHelper.GetDpi(text).PixelsPerDip) { MaxTextWidth = Math.Max(1, text.ActualWidth) };
                RecordTooltipHeightDiagnostic(tooltip, text, owner, expected, formatted);
                Product(text.ActualHeight + 1 >= formatted.Height && text.ActualWidth > 0, "ToolTip 首尾完整字形实际测量适合已渲染正文");
                Observe("tooltip-read", new { expected, tooltip.ActualWidth, tooltip.ActualHeight, tooltip.MaxWidth, textHeight = text.ActualHeight, formatted.Height, scope = "Original attached value in default owned WPF ToolTip; logical open, not native hover" });
            }
            catch (Exception error) { primary = error; }
            finally { tooltip.IsOpen = false; owner.SetCurrentValue(FrameworkElement.ToolTipProperty, attached); await Settle(); }
            if (primary is not null) ExceptionDispatchInfo.Capture(primary).Throw();
            Product(!tooltip.IsOpen && ReferenceEquals(owner.ToolTip, attached), "自有 ToolTip 已关闭且原附加值/绑定未改");
        }
        private void RecordTooltipHeightDiagnostic(ToolTip tooltip, TextBlock text, FrameworkElement owner, string expected, FormattedText original)
        {
            try
            {
                var entryMetrics = (text.DesiredSize, text.RenderSize, text.ActualWidth, text.ActualHeight, text.FontSize, TextOptions.GetTextFormattingMode(text), tooltip.DesiredSize, tooltip.RenderSize, tooltip.ActualWidth, tooltip.ActualHeight, tooltip.FontSize, TextOptions.GetTextFormattingMode(tooltip), owner.DesiredSize, owner.RenderSize, owner.ActualWidth, owner.ActualHeight, TextOptions.GetTextFormattingMode(owner));
                var entryIsOpen = tooltip.IsOpen; var entryText = text.Text; var entryOwnerToolTip = owner.ToolTip;
                var entry = new { text = Light(text), tooltip = Light(tooltip), owner = Light(owner), tooltipIsOpen = entryIsOpen, textValue = entryText, ownerToolTipType = entryOwnerToolTip?.GetType().FullName };
                var dpi = VisualTreeHelper.GetDpi(text); var typeface = new Typeface(text.FontFamily, text.FontStyle, text.FontWeight, text.FontStretch);
                var effectiveMode = TextOptions.GetTextFormattingMode(text);
                object Compare(TextFormattingMode mode)
                {
                    var value = new FormattedText(expected, CultureInfo.CurrentUICulture, text.FlowDirection, typeface, text.FontSize, text.Foreground, null, mode, dpi.PixelsPerDip) { MaxTextWidth = original.MaxTextWidth };
                    return new { mode = mode.ToString(), height = Number(value.Height), width = Number(value.Width), widthIncludingTrailingWhitespace = Number(value.WidthIncludingTrailingWhitespace), maxTextWidth = Number(value.MaxTextWidth) };
                }
                object Element(FrameworkElement value)
                {
                    var scale = VisualTreeHelper.GetDpi(value);
                    return new
                    {
                        type = value.GetType().FullName, formattingMode = TextOptions.GetTextFormattingMode(value).ToString(), renderingMode = TextOptions.GetTextRenderingMode(value).ToString(), hintingMode = TextOptions.GetTextHintingMode(value).ToString(),
                        value.UseLayoutRounding, value.SnapsToDevicePixels, value.IsVisible, value.IsLoaded, value.ClipToBounds,
                        dpi = new { x = scale.DpiScaleX, y = scale.DpiScaleY, scale.PixelsPerDip, scale.PixelsPerInchX, scale.PixelsPerInchY },
                        font = value is Control control ? new { family = control.FontFamily.Source, size = control.FontSize, style = control.FontStyle.ToString(), weight = control.FontWeight.ToString(), stretch = control.FontStretch.ToString() } : value is TextBlock block ? new { family = block.FontFamily.Source, size = block.FontSize, style = block.FontStyle.ToString(), weight = block.FontWeight.ToString(), stretch = block.FontStretch.ToString() } : null,
                        flowDirection = value.FlowDirection.ToString(), language = value.Language.IetfLanguageTag, effectiveCulture = value.Language.GetEquivalentCulture().Name, specificCulture = value.Language.GetSpecificCulture().Name,
                        desired = Size(value.DesiredSize), render = Size(value.RenderSize), actual = new { width = Number(value.ActualWidth), height = Number(value.ActualHeight) }, visualClip = Clip(value)
                    };
                }
                var textBounds = Bounds(text, tooltip); var visible = Rect.Intersect(new Rect(tooltip.RenderSize), textBounds); var clips = new List<object>();
                for (DependencyObject? ancestor = text; ancestor is not null; ancestor = VisualTreeHelper.GetParent(ancestor))
                {
                    if (ancestor is FrameworkElement element)
                    {
                        var bounds = Bounds(element, tooltip); var clip = VisualTreeHelper.GetClip(element);
                        if (element.ClipToBounds) visible.Intersect(bounds);
                        if (clip is not null) visible.Intersect(ReferenceEquals(element, tooltip) ? clip.Bounds : element.TransformToAncestor(tooltip).TransformBounds(clip.Bounds));
                        clips.Add(new { type = element.GetType().FullName, bounds = Finite(bounds), element.ClipToBounds, visualClip = Clip(element) });
                    }
                    if (ReferenceEquals(ancestor, tooltip)) break;
                }
                var frames = VisualDescendants<FrameworkElement>(tooltip).Where(value => value is Border or ContentPresenter).Select(value => new { type = value.GetType().FullName, boundsInTooltip = Finite(Bounds(value, tooltip)), textBoundsInElement = value.IsAncestorOf(text) ? Finite(Bounds(text, value)) : null, desired = Size(value.DesiredSize), render = Size(value.RenderSize), value.ClipToBounds, visualClip = Clip(value) }).ToArray();
                var textLayoutValid = text.IsMeasureValid && text.IsArrangeValid && text.ContentStart.HasValidLayout && text.ContentEnd.HasValidLayout;
                object characterRects = textLayoutValid ? new
                {
                    status = "READ-VALID-LAYOUT", fullText = new System.Windows.Documents.TextRange(text.ContentStart, text.ContentEnd).Text,
                    first = Finite(text.ContentStart.GetCharacterRect(System.Windows.Documents.LogicalDirection.Forward)),
                    last = Finite(text.ContentEnd.GetCharacterRect(System.Windows.Documents.LogicalDirection.Backward)),
                    coordinateSpace = "Original TextBlock local coordinates"
                } : new { status = "NOT-READ-NO-VALID-LAYOUT" };
                var beforeCapture = new { tooltipIsOpen = tooltip.IsOpen, text = Element(text), tooltip = Element(tooltip), owner = Element(owner) };
                var originalMetrics = (text.DesiredSize, text.RenderSize, text.ActualWidth, text.ActualHeight, text.FontSize, TextOptions.GetTextFormattingMode(text), tooltip.DesiredSize, tooltip.RenderSize, tooltip.ActualWidth, tooltip.ActualHeight, tooltip.FontSize, TextOptions.GetTextFormattingMode(tooltip), owner.DesiredSize, owner.RenderSize, owner.ActualWidth, owner.ActualHeight, TextOptions.GetTextFormattingMode(owner));
                object? capture = null;
                if (!tooltipHeightCaptures.Contains(expected))
                {
                    const string criticalVersion = "2026.10.16-preview-long";
                    var reservedVersionSlot = tooltipHeightCaptures.Contains(criticalVersion) ? 0 : 1;
                    if (tooltipHeightCaptures.Count < 24 - reservedVersionSlot || expected == criticalVersion && tooltipHeightCaptures.Count < 24)
                    {
                        var captureWatch = Stopwatch.StartNew();
                        var scale = VisualTreeHelper.GetDpi(tooltip);
                        var pixelWidth = (int)Math.Ceiling(tooltip.ActualWidth * scale.DpiScaleX); var pixelHeight = (int)Math.Ceiling(tooltip.ActualHeight * scale.DpiScaleY);
                        if (pixelWidth <= 0 || pixelHeight <= 0 || pixelWidth > 2048 || pixelHeight > 2048) throw new InvalidOperationException("诊断 Tip 位图尺寸超出有界采样范围：" + pixelWidth + "x" + pixelHeight);
                        var bitmap = new RenderTargetBitmap(pixelWidth, pixelHeight, scale.PixelsPerInchX, scale.PixelsPerInchY, PixelFormats.Pbgra32); bitmap.Render(tooltip);
                        var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap));
                        var marker = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(expected)))[..8];
                        var fileName = "tooltip-height-" + (tooltipHeightCaptures.Count + 1).ToString("00", CultureInfo.InvariantCulture) + "-" + marker + ".png"; var path = Path.Combine(directory, fileName);
                        using (var file = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.Read)) encoder.Save(file);
                        var bytes = File.ReadAllBytes(path); tooltipHeightCaptures.Add(expected); capture = new { status = "CAPTURED", fileName, pixelWidth, pixelHeight, bytes = bytes.Length, sha256 = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bytes)), captureMilliseconds = captureWatch.Elapsed.TotalMilliseconds, criticalVersion = expected == criticalVersion, scope = "Current owned ToolTip RenderTargetBitmap only; no OS window capture or input" };
                    }
                    else capture = new { status = "NOT-CAPTURED-BOUND", reason = "Per-case bound reached; one slot is reserved for the critical version value until captured", maximumDifferentExpectedPerCase = 24, capturedDifferentExpected = tooltipHeightCaptures.Count, expected };
                }
                else capture = new { status = "ALREADY-CAPTURED-EXPECTED" };
                var afterCapture = new { tooltipIsOpen = tooltip.IsOpen, text = Element(text), tooltip = Element(tooltip), owner = Element(owner) };
                var metricsUnchanged = originalMetrics == (text.DesiredSize, text.RenderSize, text.ActualWidth, text.ActualHeight, text.FontSize, TextOptions.GetTextFormattingMode(text), tooltip.DesiredSize, tooltip.RenderSize, tooltip.ActualWidth, tooltip.ActualHeight, tooltip.FontSize, TextOptions.GetTextFormattingMode(tooltip), owner.DesiredSize, owner.RenderSize, owner.ActualWidth, owner.ActualHeight, TextOptions.GetTextFormattingMode(owner));
                var originalRecord = new { height = Number(original.Height), width = Number(original.Width), widthIncludingTrailingWhitespace = Number(original.WidthIncludingTrailingWhitespace), maxTextWidth = Number(original.MaxTextWidth), extent = Number(original.Extent), baseline = Number(original.Baseline), constructor = "Current assertion 9-argument FormattedText with effective TextBlock formatting mode; WrapWithOverflow paragraph" };
                var comparisons = new { effective = Compare(effectiveMode), ideal = Compare(TextFormattingMode.Ideal), display = Compare(TextFormattingMode.Display) };
                var textLayout = new { wrapping = text.TextWrapping.ToString(), trimming = text.TextTrimming.ToString(), lineHeight = Number(text.LineHeight), lineHeightFinite = double.IsFinite(text.LineHeight), lineStackingStrategy = text.LineStackingStrategy.ToString() };
                var exitMetrics = (text.DesiredSize, text.RenderSize, text.ActualWidth, text.ActualHeight, text.FontSize, TextOptions.GetTextFormattingMode(text), tooltip.DesiredSize, tooltip.RenderSize, tooltip.ActualWidth, tooltip.ActualHeight, tooltip.FontSize, TextOptions.GetTextFormattingMode(tooltip), owner.DesiredSize, owner.RenderSize, owner.ActualWidth, owner.ActualHeight, TextOptions.GetTextFormattingMode(owner));
                var exitIsOpen = tooltip.IsOpen; var exitText = text.Text; var ownerToolTipReferenceUnchanged = ReferenceEquals(entryOwnerToolTip, owner.ToolTip);
                var exit = new { text = Light(text), tooltip = Light(tooltip), owner = Light(owner), tooltipIsOpen = exitIsOpen, textValue = exitText, ownerToolTipType = owner.ToolTip?.GetType().FullName };
                var wholeDiagnosticUnchanged = entryMetrics == exitMetrics && entryIsOpen == exitIsOpen && entryText == exitText && ownerToolTipReferenceUnchanged;
                Observe("tooltip-height-diagnostic", new
                {
                    expected, heightPredicateUnchanged = true, culture = CultureInfo.CurrentUICulture.Name,
                    original = originalRecord, comparisons, textLayout, entry, exit, ownerToolTipReferenceUnchanged, wholeDiagnosticUnchanged,
                    text = afterCapture.text, tooltip = afterCapture.tooltip, owner = afterCapture.owner, textBoundsInTooltip = Finite(textBounds), visibleTextBoundsInTooltip = Finite(visible), clipScope = "Visual clip bounds within current ToolTip subtree, not OS desktop occlusion", clips, frames, characterRects, beforeCapture, afterCapture, metricsUnchanged, capture
                });
            }
            catch (Exception error) { Observe("tooltip-height-diagnostic-error", new { failureClass = "FIXTURE-FAIL", error = error.ToString(), originalAssertionStillRuns = true }); }

            static double? Number(double value) => double.IsFinite(value) ? value : null;
            static object Size(Size value) => new { width = Number(value.Width), height = Number(value.Height) };
            static object Light(FrameworkElement value) => new { desired = Size(value.DesiredSize), render = Size(value.RenderSize), width = Number(value.ActualWidth), height = Number(value.ActualHeight), formattingMode = TextOptions.GetTextFormattingMode(value).ToString(), fontSize = value is Control control ? Number(control.FontSize) : value is TextBlock block ? Number(block.FontSize) : null };
            static Rect Bounds(FrameworkElement value, Visual ancestor) => ReferenceEquals(value, ancestor) ? new Rect(value.RenderSize) : value.TransformToAncestor(ancestor).TransformBounds(new Rect(value.RenderSize));
            static object? Clip(Visual value) { var clip = VisualTreeHelper.GetClip(value); return clip is null ? null : new { type = clip.GetType().FullName, bounds = Finite(clip.Bounds) }; }
        }
        internal FrameworkElement CommonWorkspace()
        {
            for (var value = VisualTreeHelper.GetParent(Targets); value is not null; value = VisualTreeHelper.GetParent(value))
                if (value is Grid grid && grid.IsAncestorOf(Apps) && grid.IsAncestorOf(Tasks)) return grid;
            throw new LayoutAppsPreconditionFailure("原三表缺少共享正文 Grid");
        }
        internal LayoutAppsSnapshot Snapshot() => new(this);
        internal void SameCalls() { Product(frozenCalls is not null && Backend.Calls().SequenceEqual(frozenCalls), "载入后纯布局/阅读没有额外 backend 调用"); }
        internal void Context() => Product(ReferenceEquals(current, Vm.CurrentDevice) && ReferenceEquals(main, Vm.MainDevice) && devices.SequenceEqual(Vm.Devices) && selections.SequenceEqual(Vm.Devices.Select(row => row.IsSelected)) && globalTargets.SequenceEqual(Vm.Targets()) && unified.SequenceEqual(Window.unifiedDevices) && unifiedSelection.SequenceEqual(Window.unifiedDevices.Select(row => row.IsSelected)), "独立应用操作不改全局 Current/Main/Targets/群控选择及对象引用");
        internal static double FontSize(DependencyObject value) => value is Control control ? control.FontSize : ((TextBlock)value).FontSize;
        internal void RecordGlyphScaleDiagnostic()
        {
            try
            {
                var entryPairs = Fonts.ToArray(); var entryFonts = entryPairs.Select(pair => FontSize(pair.Key)).ToArray();
                var entryPage = VisualDescendants<FrameworkElement>(Page).ToArray(); var entryPageFonts = entryPage.Where(value => value is Control or TextBlock).ToArray();
                var entryPageValues = entryPageFonts.Select(value => FontSize(value)).ToArray(); var source = PresentationSource.FromVisual(Page);
                var pageSet = new HashSet<DependencyObject>(entryPage, ReferenceEqualityComparer.Instance); var identities = new Dictionary<DependencyObject, int>(ReferenceEqualityComparer.Instance);
                foreach (var pair in entryPairs) Id(pair.Key); foreach (var value in entryPage) Id(value);
                var rows = entryPairs.Select((pair, index) => new { pair.Key, Baseline = pair.Value, Current = entryFonts[index], Expected = pair.Value * 1.5, Matches = Math.Abs(entryFonts[index] - pair.Value * 1.5) < .01 }).ToArray();
                var mismatches = rows.Where(row => !row.Matches).Select(row => Detail(row.Key, row.Baseline, row.Current, row.Expected)).ToArray();
                var matches = rows.Where(row => row.Matches).Take(6).Select(row => Detail(row.Key, row.Baseline, row.Current, row.Expected)).ToArray();
                var unregistered = entryPageFonts.Where(value => !Fonts.ContainsKey(value)).ToArray();
                var missingSample = unregistered.Take(16).Select(value => Detail(value, null, FontSize(value), null)).ToArray();
                var exitPairs = Fonts.ToArray(); var exitFonts = exitPairs.Select(pair => FontSize(pair.Key)).ToArray();
                var exitPage = VisualDescendants<FrameworkElement>(Page).ToArray(); var exitPageFonts = exitPage.Where(value => value is Control or TextBlock).ToArray();
                var exitPageValues = exitPageFonts.Select(value => FontSize(value)).ToArray();
                var keyReferencesUnchanged = entryPairs.Length == exitPairs.Length && entryPairs.Zip(exitPairs).All(pair => ReferenceEquals(pair.First.Key, pair.Second.Key));
                var baselinesUnchanged = entryPairs.Select(pair => pair.Value).SequenceEqual(exitPairs.Select(pair => pair.Value));
                var fontsUnchanged = entryFonts.SequenceEqual(exitFonts);
                var pageReferencesUnchanged = entryPage.Length == exitPage.Length && entryPage.Zip(exitPage).All(pair => ReferenceEquals(pair.First, pair.Second));
                var pageFontReferencesUnchanged = entryPageFonts.Length == exitPageFonts.Length && entryPageFonts.Zip(exitPageFonts).All(pair => ReferenceEquals(pair.First, pair.Second));
                var pageFontsUnchanged = entryPageValues.SequenceEqual(exitPageValues);
                Observe("glyph-scale-diagnostic", new
                {
                    predicateScope = "Current Page Control/TextBlock members, including hidden; all registered and abs(current - first baseline * 1.5) < .01; historical dictionary retained for observation", diagnosticOnly = true,
                    count = rows.Length, countPredicate = rows.Length > 20, matches = rows.Count(row => row.Matches), mismatchCount = mismatches.Length,
                    coverage = new { pageVisualMembers = entryPage.Length, pageFontMembers = entryPageFonts.Length, pageFontMembersRegistered = entryPageFonts.Length - unregistered.Length, pageFontMembersUnregistered = unregistered.Length, registeredInPage = rows.Count(row => pageSet.Contains(row.Key)), registeredOutsidePage = rows.Count(row => !pageSet.Contains(row.Key)), hiddenRegisteredInPage = rows.Count(row => pageSet.Contains(row.Key) && row.Key is UIElement { IsVisible: false }), visibleRegisteredInPage = rows.Count(row => pageSet.Contains(row.Key) && row.Key is UIElement { IsVisible: true }), mismatchesInPage = rows.Count(row => !row.Matches && pageSet.Contains(row.Key)), mismatchesOutsidePage = rows.Count(row => !row.Matches && !pageSet.Contains(row.Key)), unregisteredSampleLimit = 16 },
                    mismatches, matchSample = matches, matchSampleLimit = 6, unregisteredCurrentSample = missingSample,
                    entry = new { count = entryPairs.Length, keys = entryPairs.Select(pair => Id(pair.Key)).ToArray(), baselines = entryPairs.Select(pair => Number(pair.Value)).ToArray(), fonts = entryFonts.Select(Number).ToArray(), pageMembers = entryPage.Select(Id).ToArray(), pageFontMembers = entryPageFonts.Select(Id).ToArray(), pageFonts = entryPageValues.Select(Number).ToArray() },
                    exit = new { count = exitPairs.Length, keys = exitPairs.Select(pair => Id(pair.Key)).ToArray(), baselines = exitPairs.Select(pair => Number(pair.Value)).ToArray(), fonts = exitFonts.Select(Number).ToArray(), pageMembers = exitPage.Select(Id).ToArray(), pageFontMembers = exitPageFonts.Select(Id).ToArray(), pageFonts = exitPageValues.Select(Number).ToArray() },
                    keyReferencesUnchanged, baselinesUnchanged, fontsUnchanged, pageReferencesUnchanged, pageFontReferencesUnchanged, pageFontsUnchanged,
                    wholeDiagnosticUnchanged = keyReferencesUnchanged && baselinesUnchanged && fontsUnchanged && pageReferencesUnchanged && pageFontReferencesUnchanged && pageFontsUnchanged,
                    scope = "Read-only final snapshot; no filtering, font reapplication, layout wait, dispatcher scheduling or registration-history inference"
                });

                int Id(DependencyObject value) { if (!identities.TryGetValue(value, out var id)) { id = identities.Count + 1; identities.Add(value, id); } return id; }
                object Detail(DependencyObject value, double? baseline, double current, double? expected)
                {
                    var element = (FrameworkElement)value; var currentSource = PresentationSource.FromVisual(element); var parents = new List<object>(); var parent = VisualTreeHelper.GetParent(value);
                    for (var depth = 0; parent is not null && depth < 8; depth++, parent = VisualTreeHelper.GetParent(parent))
                        parents.Add(new { id = Id(parent), type = parent.GetType().FullName, name = (parent as FrameworkElement)?.Name, currentFont = Number((double)parent.GetValue(System.Windows.Documents.TextElement.FontSizeProperty)), registered = Fonts.ContainsKey(parent), inPageVisualMembers = pageSet.Contains(parent), valueSource = FontSource(parent) });
                    var pureContent = value is TextBlock text ? text.Text : value is TextBox input ? input.Text : value is ContentControl control ? control.Content as string : null;
                    return new
                    {
                        id = Id(value), type = value.GetType().FullName, element.Name, pureContent = pureContent is { Length: > 240 } ? pureContent[..240] : pureContent, pureContentTruncated = pureContent?.Length > 240,
                        baseline = baseline is { } before ? Number(before) : null, current = Number(current), expected = expected is { } target ? Number(target) : null, delta = expected is { } wanted ? Number(current - wanted) : null,
                        element.IsLoaded, element.IsVisible, visibility = element.Visibility.ToString(), inPageVisualMembers = pageSet.Contains(value), isPageDescendant = ReferenceEquals(value, Page) || Page.IsAncestorOf(value), hasPresentationSource = currentSource is not null, samePagePresentationSource = currentSource is not null && ReferenceEquals(currentSource, source),
                        fontValueSource = FontSource(value), parentLimit = 8, parentChainTruncated = parent is not null, parents
                    };
                }
                static object FontSource(DependencyObject value)
                {
                    var property = System.Windows.Documents.TextElement.FontSizeProperty; var origin = DependencyPropertyHelper.GetValueSource(value, property); var local = value.ReadLocalValue(property);
                    return new { baseValueSource = origin.BaseValueSource.ToString(), origin.IsAnimated, origin.IsCoerced, origin.IsCurrent, origin.IsExpression, localKind = ReferenceEquals(local, DependencyProperty.UnsetValue) ? "UnsetValue" : local?.GetType().FullName, localNumber = local is double number ? Number(number) : null };
                }
                static double? Number(double value) => double.IsFinite(value) ? value : null;
            }
            catch (Exception error) { Observe("glyph-scale-diagnostic-error", new { failureClass = "FIXTURE-FAIL", error = error.ToString(), originalAssertionStillRuns = true }); }
        }
        internal async Task ApplyGlyphScale()
        {
            if (GlyphScale == 1 || Window is null) return;
            // Freeze the whole realized batch before changing any inherited font value.
            var batch = VisualDescendants<FrameworkElement>(Page).Where(element => element is Control or TextBlock).Where(element => !Fonts.ContainsKey(element)).Select(element => (Element: element, Original: BaselineFont(element))).ToArray();
            foreach (var item in batch) Fonts.Add(item.Element, item.Original);
            foreach (var item in batch)
            {
                // A local override establishes only this owned typography stress environment.
                if (item.Element is Control control) control.SetValue(Control.FontSizeProperty, item.Original * GlyphScale); else ((TextBlock)item.Element).SetValue(TextBlock.FontSizeProperty, item.Original * GlyphScale);
            }
            await Settle();
            double BaselineFont(FrameworkElement element)
            {
                var currentFont = FontSize(element); var fontProperty = System.Windows.Documents.TextElement.FontSizeProperty;
                if (DependencyPropertyHelper.GetValueSource(element, fontProperty).BaseValueSource != BaseValueSource.Inherited) return currentFont;
                for (var ancestor = VisualTreeHelper.GetParent(element); ancestor is not null; ancestor = VisualTreeHelper.GetParent(ancestor))
                {
                    if (DependencyPropertyHelper.GetValueSource(ancestor, fontProperty).BaseValueSource == BaseValueSource.Inherited) continue;
                    return Fonts.ContainsKey(ancestor) ? currentFont / GlyphScale : currentFont;
                }
                return currentFont;
            }
        }
        internal void Save(string name)
        {
            var path = Path.Combine(directory, name + ".png"); Window.SaveRender(path); var bytes = File.ReadAllBytes(path); Require(bytes.Length > 0, "必需应用布局截图已保存：" + name);
            Observe("saved-render", new { name, bytes = bytes.Length, sha256 = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bytes)) });
        }
        private static object? Finite(Rect value) => value.IsEmpty || !double.IsFinite(value.X) || !double.IsFinite(value.Y) || !double.IsFinite(value.Width) || !double.IsFinite(value.Height) ? null : new { value.X, value.Y, value.Width, value.Height };
        internal void Observe(string stage, object? detail = null)
        {
            Observations.Add(new { stage, at = Stopwatch.GetTimestamp(), detail, width = Window?.ActualWidth, height = Window?.ActualHeight, body = Window is null ? null : Finite(DeviceProbeBounds(Page, Window.Shell)), busy = Vm?.Busy, running = Window?.applicationRunning, pending = Window?.applicationReadPending, apps = Window?.applicationRows.Count, tasks = Window?.applicationTasks.Count, listCalls = Backend.ListCalls });
        }
        internal async Task Cleanup()
        {
            if (cleaned) throw new InvalidOperationException("应用正文不得重复清理"); cleaned = true; var errors = new List<Exception>();
            if (Window is not null) { try { Window.AbortStartup(); } catch (Exception error) { errors.Add(error); } }
            Backend.ReleaseAll();
            try { await Backend.Drain(); } catch (Exception error) { errors.Add(error); }
            if (Window is not null) try { await Wait(() => !Window.applicationRunning && !Vm.Busy, "关闭后原批次未排空"); } catch (Exception error) { errors.Add(error); }
            if (Vm is not null) try { await Vm.ShutdownAsync().WaitAsync(TimeSpan.FromSeconds(5)); } catch (Exception error) { errors.Add(error); }
            else if (runtime is not null) try { await runtime.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(5)); } catch (Exception error) { errors.Add(error); }
            else
            {
                if (api is not null) await Attempt(() => api.DisposeAsync().AsTask()); if (input is not null) await Attempt(() => input.DisposeAsync().AsTask()); if (scripts is not null) await Attempt(() => scripts.DisposeAsync().AsTask()); await Attempt(() => Backend.DisposeAsync().AsTask());
                try { metadata?.Dispose(); } catch (Exception error) { errors.Add(error); } if (log is not null) await Attempt(() => log.DisposeAsync().AsTask());
            }
            if (Window is not null) { LocalizationService.Current.LanguageChanged -= Window.OnLanguageChanged; try { await Settle(); if (Window.applicationReadTimer?.IsEnabled == true) { await Task.Delay(400); await Settle(); } if (Window.applicationReadTimer?.IsEnabled == true || Window.applicationRunning || Vm!.Busy || Window.IsVisible) throw new InvalidOperationException("原应用 timer/批次/自有窗口未排空"); } catch (Exception error) { errors.Add(error); } }
            try { await Backend.Drain(); if (!Backend.AllTerminal || Backend.ActiveLists != 0 || Backend.Calls().Any(call => call.Forbidden)) throw new InvalidOperationException("非转发后端 Task/TCS/禁止调用未通过清理核查"); } catch (Exception error) { errors.Add(error); }
            if (errors.Count > 0) throw new AggregateException("应用正文 owned 清理失败", errors);
            async Task Attempt(Func<Task> action) { try { await action().WaitAsync(TimeSpan.FromSeconds(5)); } catch (Exception error) { errors.Add(error); } }
        }
    }

    private sealed class LayoutAppsSnapshot
    {
        private readonly object targets; private readonly object apps; private readonly object tasks; private readonly object targetSource; private readonly object appSource; private readonly object taskSource;
        private readonly ApplicationTargetRow[] targetRows; private readonly InstalledApplicationRow[] appRows; private readonly ApplicationTaskRow[] taskRows; private readonly bool[] targetSelected; private readonly bool[] appSelected;
        private readonly (ApplicationTaskState State, string Message)[] taskStates; private readonly object[] visible; private readonly SortDescription[] sorts; private readonly bool? system; private readonly string result; private readonly double progress;
        internal LayoutAppsSnapshot(LayoutAppsCase state)
        {
            var window = state.Window; targets = state.Targets; apps = state.Apps; tasks = state.Tasks; targetSource = state.Targets.ItemsSource; appSource = state.Apps.ItemsSource; taskSource = state.Tasks.ItemsSource;
            targetRows = window.applicationTargets.ToArray(); appRows = window.applicationRows.ToArray(); taskRows = window.applicationTasks.ToArray(); targetSelected = targetRows.Select(row => row.IsSelected).ToArray(); appSelected = appRows.Select(row => row.IsSelected).ToArray();
            taskStates = taskRows.Select(row => (row.ResultState, row.Message)).ToArray(); visible = window.applicationRowsView.Cast<object>().ToArray(); sorts = window.applicationRowsView.SortDescriptions.ToArray(); system = window.applicationSystemToggle.IsChecked; result = window.applicationResult.Text; progress = window.applicationProgress.Value;
        }
        internal void Same(LayoutAppsCase state)
        {
            var window = state.Window;
            state.Product(ReferenceEquals(targets, state.Targets) && ReferenceEquals(apps, state.Apps) && ReferenceEquals(tasks, state.Tasks) && ReferenceEquals(targetSource, state.Targets.ItemsSource) && ReferenceEquals(appSource, state.Apps.ItemsSource) && ReferenceEquals(taskSource, state.Tasks.ItemsSource), "resize/切段始终同一三表与 ItemsSource");
            state.Product(targetRows.SequenceEqual(window.applicationTargets) && appRows.SequenceEqual(window.applicationRows) && taskRows.SequenceEqual(window.applicationTasks) && targetSelected.SequenceEqual(window.applicationTargets.Select(row => row.IsSelected)) && appSelected.SequenceEqual(window.applicationRows.Select(row => row.IsSelected)), "原行对象、集合次序及目标/应用勾选保持");
            state.Product(taskStates.SequenceEqual(window.applicationTasks.Select(row => (row.ResultState, row.Message))) && visible.SequenceEqual(window.applicationRowsView.Cast<object>()) && sorts.SequenceEqual(window.applicationRowsView.SortDescriptions) && system == window.applicationSystemToggle.IsChecked && result == window.applicationResult.Text && progress == window.applicationProgress.Value, "过滤/排序/逐项任务/批次反馈保持"); state.Context();
        }
    }

    private sealed record LayoutAppsCall(int Sequence, string Operation, string? Serial, bool Forbidden);
    private sealed class LayoutAppsGate(Exception? failure)
    {
        internal readonly Exception? Failure = failure; internal bool Claimed;
        internal readonly TaskCompletionSource Entered = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal readonly TaskCompletionSource Release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal readonly TaskCompletionSource Returned = new(TaskCreationOptions.RunContinuationsAsynchronously);
    }
    private sealed class LayoutAppsBackend : IDeviceBackend
    {
        internal const string FailureMarker = "受控应用列表失败；原始结果需完整阅读。first-marker / " + "应用正文长结果保留、滚动与停止后续可达。" + "com.matrix.layout.failure.with.a.long.package.name / " + "last-marker";
        private readonly object sync = new(); private readonly List<LayoutAppsCall> calls = []; private readonly List<LayoutAppsGate> gates = []; private readonly List<Task> tasks = []; private int active;
        internal int ActiveLists { get { lock (sync) return active; } } internal int ListCalls { get { lock (sync) return calls.Count(call => call.Operation == "List"); } }
        internal bool ListTasksCompleted { get { lock (sync) return tasks.All(task => task.IsCompleted); } }
        internal bool AllTerminal { get { lock (sync) return tasks.All(task => task.IsCompleted) && gates.All(gate => gate.Entered.Task.IsCompleted && gate.Release.Task.IsCompleted && gate.Returned.Task.IsCompleted); } }
        internal LayoutAppsCall[] Calls() { lock (sync) return calls.ToArray(); }
        internal object[] TaskReport() { lock (sync) return tasks.Select((task, index) => (object)new { index, status = task.Status.ToString(), task.IsCompleted }).ToArray(); }
        internal object[] GateReport() { lock (sync) return gates.Select(gate => (object)new { gate.Claimed, entered = gate.Entered.Task.Status.ToString(), released = gate.Release.Task.Status.ToString(), returned = gate.Returned.Task.Status.ToString(), expectedFailure = gate.Failure?.Message }).ToArray(); }
        internal LayoutAppsGate Hold(Exception? failure = null) { var gate = new LayoutAppsGate(failure); lock (sync) gates.Add(gate); return gate; }
        internal void ReleaseAll() { lock (sync) foreach (var gate in gates) { gate.Release.TrySetResult(); if (!gate.Claimed) { gate.Entered.TrySetCanceled(); gate.Returned.TrySetCanceled(); } } }
        internal async Task Drain()
        {
            Task[] original; lock (sync) original = tasks.ToArray();
            foreach (var task in original) { try { await task.WaitAsync(TimeSpan.FromSeconds(5)); } catch (DeviceException error) when (error.Message == FailureMarker && task.IsFaulted) { } }
        }
        private void Record(string operation, string? serial, bool forbidden = false) { lock (sync) calls.Add(new(calls.Count + 1, operation, serial, forbidden)); }
        public Task<IReadOnlyList<DeviceInfo>> DiscoverAsync(CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested(); Record("Discover", null);
            return Task.FromResult<IReadOnlyList<DeviceInfo>>(Enumerable.Range(1, 12).Select(index => new DeviceInfo { Serial = $"DEMO-{index:000}", Name = $"布局目标 {index:00} · 长名称用于原字段阅读与独立上下文", State = "Ready", Width = 160, Height = 240, TransportId = (uint)(100 + index) }).ToArray());
        }
        public Task<IReadOnlyList<AppInfo>> ListAppsAsync(string serial, bool showSystem = false, bool needVersion = true, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested(); Record("List", serial); LayoutAppsGate? gate;
            lock (sync) { gate = gates.FirstOrDefault(item => !item.Claimed); if (gate is not null) gate.Claimed = true; }
            var task = List(); lock (sync) tasks.Add(task); return task;
            async Task<IReadOnlyList<AppInfo>> List()
            {
                lock (sync) active++;
                try
                {
                    if (gate is not null) { gate.Entered.TrySetResult(); await gate.Release.Task.WaitAsync(ct); if (gate.Failure is not null) ExceptionDispatchInfo.Capture(gate.Failure).Throw(); }
                    return Enumerable.Range(1, 8).Select(index => new AppInfo($"com.matrix.responsive.{serial.Replace('-', '_').ToLowerInvariant()}.long_package_name_for_horizontal_reading.app{index:00}", $"应用 {index:00} · 长名称用于检查原列完整阅读、系统过滤与宽窄往返", "2026.10.16-preview-long", index == 8)).ToArray();
                }
                finally { lock (sync) active--; gate?.Returned.TrySetResult(); }
            }
        }
        private Exception Forbidden(string operation, string serial) { Record(operation, serial, true); return new InvalidOperationException("应用布局专项禁止转发或设备写：" + operation + ":" + serial); }
        public Task<string> ShellAsync(string serial, string command, bool useRoot = false, CancellationToken ct = default) => throw Forbidden("Shell", serial);
        public Task TapAsync(string serial, int x, int y, CancellationToken ct = default) => throw Forbidden("Tap", serial);
        public Task SwipeAsync(string serial, int x1, int y1, int x2, int y2, int durationMs, CancellationToken ct = default) => throw Forbidden("Swipe", serial);
        public Task KeyEventAsync(string serial, int code, CancellationToken ct = default) => throw Forbidden("Key", serial);
        public Task TextAsync(string serial, string text, CancellationToken ct = default) => throw Forbidden("Text", serial);
        public Task<string> StartAppAsync(string serial, string packageName, string? activity = null, CancellationToken ct = default) => throw Forbidden("Start", serial);
        public Task<string> StopAppAsync(string serial, string packageName, CancellationToken ct = default) => throw Forbidden("Stop", serial);
        public Task<bool> ClearAppAsync(string serial, string packageName, CancellationToken ct = default) => throw Forbidden("Clear", serial);
        public Task InstallAsync(string serial, string localPath, CancellationToken ct = default) => throw Forbidden("Install", serial);
        public Task UninstallAsync(string serial, string packageName, CancellationToken ct = default) => throw Forbidden("Uninstall", serial);
        public Task PushAsync(string serial, string localPath, string remotePath, IProgress<double>? progress = null, CancellationToken ct = default) => throw Forbidden("Push", serial);
        public Task PullAsync(string serial, string remotePath, string localPath, IProgress<double>? progress = null, CancellationToken ct = default) => throw Forbidden("Pull", serial);
        public Task SetNodeTextAsync(string serial, NodeSelector selector, string text, CancellationToken ct = default) => throw Forbidden("SetNodeText", serial);
        public Task<byte[]> ScreenshotAsync(string serial, CancellationToken ct = default) => throw Forbidden("UnexpectedRead:Screenshot", serial);
        public Task<AndroidNode> DumpAsync(string serial, CancellationToken ct = default) => throw Forbidden("UnexpectedRead:Dump", serial);
        public ValueTask DisposeAsync() { ReleaseAll(); return ValueTask.CompletedTask; }
    }
}
