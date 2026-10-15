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
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using MatrixDevice.Core;
using MatrixDevice.Features;
using MatrixDevice.Host;
using Path = System.IO.Path;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    private async Task VerifyResponsiveScreenshotAsync()
    {
        if (!ViewModel.Demo || !ViewModel.Runtime.IsDemo || !Dispatcher.CheckAccess() || Thread.CurrentThread.GetApartmentState() != ApartmentState.STA) throw new InvalidOperationException("截屏布局验收仅限自有一致Demo/STA。");
        var directory = Path.Combine(Path.GetFullPath(ViewModel.DataDirectory), "artifacts", "sl", Guid.NewGuid().ToString("N")[..8]);
        if (!Path.IsPathFullyQualified(directory) || directory.Length > 190) throw new InvalidOperationException("截屏验收需要短绝对artifacts路径：" + directory.Length);
        Directory.CreateDirectory(directory);
        var originalMain = Application.Current.MainWindow; var originalShutdown = Application.Current.ShutdownMode; var language = LocalizationService.Current.Language;
        var reports = new List<object>(); var failures = new List<Exception>(); Exception? failure = null; var number = 0;
        try
        {
            foreach (var size in new[] { (W: 390, H: 640), (W: 390, H: 844), (W: 768, H: 1024), (W: 1000, H: 700), (W: 1440, H: 900), (W: 1920, H: 1080) })
                await Case($"geometry-{size.W}-{size.H}", size.W, size.H, size.H > size.W, async state =>
                {
                    await state.Refresh(); var calls = state.Backend.Calls().Length; state.ImageGeometry(); state.ToolsGeometry();
                    state.SeedColorsAndCrop(); await state.Settle();
                    await state.ZoomTo(1.5); state.Window.screenshotMode.SelectedIndex = 1; await state.Settle();
                    state.Window.screenshotScroll.ScrollToHorizontalOffset(state.Window.screenshotScroll.ScrollableWidth * .25); state.Window.screenshotScroll.ScrollToVerticalOffset(state.Window.screenshotScroll.ScrollableHeight * .35); await state.Settle(); state.RememberOffsets();
                    await state.Pane("色值"); await state.ReadColors();
                    state.Grid.SelectedItem = state.Vm.Colors[45]; state.Grid.ScrollIntoView(state.Grid.SelectedItem); await state.Settle();
                    var focused = state.Grid.Focus(); await state.Settle(); var actualFocus = Keyboard.FocusedElement as FrameworkElement;
                    state.Observe("color-focus", new { focused, state.Window.IsActive, state.Grid.IsKeyboardFocusWithin, actualType = actualFocus?.GetType().Name, visible = actualFocus?.IsVisible });
                    state.Check(state.Window.IsActive && state.Grid.IsKeyboardFocusWithin && actualFocus?.IsVisible == true && (ReferenceEquals(actualFocus, state.Grid) || state.Grid.IsAncestorOf(actualFocus)), "色值表一次Focus后实际可见焦点归原表，不重试抢焦点");
                    state.RememberOffsets(); var snapshot = ShotSnapshot.Take(state);
                    await state.Resize(390, size.H); snapshot.Same(state); state.Check(state.Grid.IsVisible && state.Grid.IsKeyboardFocusWithin, "色值焦点缩窄仍显示原色值表");
                    await state.Pane("画面"); snapshot.Same(state); state.Save("picture"); await state.Pane("色值"); snapshot.Same(state);
                    await state.Resize(size.W, size.H); snapshot.Same(state);
                    if (size.W == 1000)
                    {
                        foreach (var width in new[] { 639d, 640d, 641d })
                        {
                            await state.Resize(768, size.H); var chrome = state.Window.ActualWidth - state.Body.ActualWidth;
                            await state.Resize(width + chrome, size.H); state.Check(Math.Abs(state.Body.ActualWidth - width) < .6, "截屏实际正文640断点正负1：" + width); snapshot.Same(state);
                        }
                        await state.Resize(size.W, size.H);
                    }
                    state.Save("colors");
                    await state.Invoke(state.Tool("原始大小")); state.Check(state.Zoom.Value == 1 && state.Window.screenshotScale.ScaleX == 1 && state.Window.screenshotScale.ScaleY == 1, "原始大小实际按钮执行并恢复1倍");
                    state.Check(state.Backend.Calls().Length == calls, "Refresh后全部resize/切段/缩放/色表阅读及原始大小不发额外后端请求");
                });
            await Case("no-image", 390, 640, true, async state =>
            {
                state.Vm.CurrentDevice = null; await state.Settle();
                state.Check(state.Vm.DebugImage is null && state.Window.screenshotImage.Source is null && state.Window.screenshotCanvas.Width == 0 && state.Window.crop is null && !state.Tool("刷新截图").IsEnabled && state.Vm.Colors.Count == 0, "无图空态沿原VM和原画布显示且刷新禁用");
                state.Check(state.Backend.Calls().Length == 0, "无图导航不偷偷请求设备"); state.Observe("no-image-save-entry-state", new { png = state.Tool("保存 PNG").IsEnabled, crop = state.Tool("保存裁剪").IsEnabled, invoked = false }); state.Save("empty");
            });
            await Case("busy", 390, 844, true, async state =>
            {
                var gate = state.Backend.Hold(); await state.Invoke(state.Tool("刷新截图")); await gate.Entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
                state.Check(state.Vm.Busy && !state.Tool("刷新截图").IsEnabled && state.Vm.DebugImage is null, "真实截图TCS在途禁用刷新且无假画面"); state.Save("busy");
                gate.Release.TrySetResult(); await state.Loaded(); state.Check(state.Backend.Calls().Count(call => call.Operation == "Screenshot") == 1 && state.Backend.Calls().All(call => call.Operation != "Dump"), "Busy完成只有一次截图而非节点读取");
            });
            await Case("failure-recovery", 390, 640, true, async state =>
            {
                var error = new DeviceException("controlled screenshot layout failure"); var gate = state.Backend.Hold(error); await state.Invoke(state.Tool("刷新截图")); await gate.Entered.Task.WaitAsync(TimeSpan.FromSeconds(5)); gate.Release.TrySetResult(); await state.Wait(() => !state.Vm.Busy, "截图失败未恢复Busy"); await state.Settle();
                state.Check(state.Vm.Error == "刷新截图失败：" + UiErrorPresentation.Describe(error) && state.Vm.Status == "操作失败" && state.Vm.DebugImage is null, "后端单次失败沿原安全中文反馈而非假图"); state.Save("error"); await state.Refresh(); state.Check(state.Vm.Error.Length == 0, "原刷新按钮实际重试恢复");
            });
            await Case("coordinate-route", 1440, 900, false, async state =>
            {
                await state.Refresh(); var calls = state.Backend.Calls().Length; state.Window.screenshotMode.SelectedIndex = 0; await state.Settle();
                if (!GetAppearanceCursorPosition(out var originalPointer)) throw new InvalidOperationException("无法保存自有截屏验收指针");
                Exception? primary = null; var routes = new List<object>(); var count = 0; Point? eventPoint = null;
                MouseButtonEventHandler observer = (_, args) => { count++; var position = args.GetPosition(state.Window.screenshotCanvas); eventPoint = position; routes.Add(new { sourceType = args.OriginalSource?.GetType().Name, owned = args.OriginalSource is DependencyObject value && (ReferenceEquals(value, state.Window.screenshotImage) || state.Window.screenshotImage.IsAncestorOf(value)), position.X, position.Y, args.ClickCount, button = args.ChangedButton.ToString() }); };
                state.Window.screenshotCanvas.AddHandler(MouseLeftButtonDownEvent, observer, true);
                try
                {
                    foreach (var zoom in new[] { .1, .5, 1d, 3d })
                    {
                        await state.ZoomTo(zoom); var scroll = state.Window.screenshotScroll;
                        if (zoom == 3) { state.Check(scroll.ScrollableWidth > 0 && scroll.ScrollableHeight > 0, "3倍原图具有真实双向滚动范围"); scroll.ScrollToHorizontalOffset(scroll.ScrollableWidth * .4); scroll.ScrollToVerticalOffset(scroll.ScrollableHeight * .35); await state.Settle(); state.Check(scroll.HorizontalOffset > 0 && scroll.VerticalOffset > 0, "3倍双向滚动实际生效"); }
                        var visible = Rect.Intersect(DeviceProbeBounds(state.Window.screenshotImage, state.Window.Shell), DeviceProbeViewport(state.Window.Shell, scroll)); visible.Intersect(state.PageClip());
                        state.Check(!visible.IsEmpty && visible.Width > 2 && visible.Height > 2, "缩放后真实Image/SCP交集存在，不把低倍率假定100像素");
                        var shellPoint = new Point(visible.X + visible.Width / 2, visible.Y + visible.Height / 2); var canvasPoint = state.Window.Shell.TransformToDescendant(state.Window.screenshotCanvas).Transform(shellPoint); var requestedScreen = state.Window.screenshotCanvas.PointToScreen(canvasPoint);
                        state.Check(SetAppearanceCursorPosition((int)Math.Round(requestedScreen.X), (int)Math.Round(requestedScreen.Y)), "自有HWND单次系统指针定位成功");
                        var watch = System.Diagnostics.Stopwatch.StartNew(); var pointerKnown = false; AppearancePointer actual = default; Point local = default;
                        do { await state.Window.Dispatcher.InvokeAsync(Mouse.Synchronize, System.Windows.Threading.DispatcherPriority.Input); await state.Window.Dispatcher.InvokeAsync(state.Window.UpdateLayout, System.Windows.Threading.DispatcherPriority.Render); pointerKnown = GetAppearanceCursorPosition(out actual); local = Mouse.GetPosition(state.Window.screenshotCanvas); if (pointerKnown && Math.Abs(actual.X - Math.Round(requestedScreen.X)) <= 1 && Math.Abs(actual.Y - Math.Round(requestedScreen.Y)) <= 1 && (local - canvasPoint).Length <= 1.5 / zoom) break; await Task.Delay(30); } while (watch.Elapsed < TimeSpan.FromMilliseconds(600));
                        var actualShell = state.Window.screenshotCanvas.TransformToAncestor(state.Window.Shell).Transform(local); var hit = state.Window.Shell.InputHitTest(actualShell) as UIElement;
                        var owned = hit is not null && (ReferenceEquals(hit, state.Window.screenshotImage) || state.Window.screenshotImage.IsAncestorOf(hit));
                        state.Observe("coordinate-position", new { zoom, canvasPoint.X, canvasPoint.Y, requestedScreen, pointerKnown, actualScreen = new { actual.X, actual.Y }, actualLocal = local, actualShell, owned, hitType = hit?.GetType().Name, visible = ShotCase.Finite(visible), waitMilliseconds = watch.ElapsedMilliseconds, singlePositionAttempt = true, state.Window.IsActive });
                        state.Check(state.Window.IsActive && pointerKnown && Math.Abs(actual.X - Math.Round(requestedScreen.X)) <= 1 && Math.Abs(actual.Y - Math.Round(requestedScreen.Y)) <= 1 && (local - canvasPoint).Length <= 1.5 / zoom && owned && visible.Contains(actualShell), "一次定位后真实OS/WPF/原Image命中前置一致，不重试Activate");
                        var before = state.Vm.Colors.Count; var beforeRoutes = count; eventPoint = null; hit!.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left) { RoutedEvent = Mouse.MouseDownEvent }); await state.Settle();
                        state.Check(count == beforeRoutes + 1 && eventPoint is not null, "原Canvas恰好收到一次真实GetPosition路由");
                        var route = routes.Last(); var sample = state.Vm.Colors.FirstOrDefault(); var routedPoint = eventPoint!.Value; var frame = state.Vm.DebugImage ?? throw new InvalidOperationException("实际取色时原图丢失"); var x = (int)Math.Clamp(routedPoint.X, 0, frame.PixelWidth - 1); var y = (int)Math.Clamp(routedPoint.Y, 0, frame.PixelHeight - 1);
                        var lastPixel = x == frame.PixelWidth - 1 && y == frame.PixelHeight - 1;
                        var expected = lastPixel ? new PixelSample(x, y, 255, 255, 255) : new PixelSample(x, y, (byte)(x % 251), (byte)(y % 251), 112);
                        state.Observe("coordinate-sample", new { zoom, before, after = state.Vm.Colors.Count, beforeRoutes, count, route, sample, expected, scope = "Actual cursor location plus synthetic WPF base mouse route; not physical click or Android input; low-scale rounding uses actual Canvas position" });
                        state.Check(count == beforeRoutes + 1 && state.Vm.Colors.Count == before + 1 && sample is not null && sample == expected && sample.Hex == expected.Hex && sample.Rgb == expected.Rgb, "原Canvas真实路由一次按实际事件像素取色/RGB/HEX一致");
                    }
                    await state.Invoke(state.Tool("原始大小")); state.Check(state.Zoom.Value == 1 && state.Window.screenshotScale.ScaleX == 1, "坐标路由后原始大小按钮实际恢复"); state.Check(state.Backend.Calls().Length == calls, "Refresh后跨zoom/scroll取色路由及原始大小不发额外后端请求"); state.Save("coordinates");
                }
                catch (Exception error) { primary = error; }
                finally { state.Window.screenshotCanvas.RemoveHandler(MouseLeftButtonDownEvent, observer); try { if (!SetAppearanceCursorPosition(originalPointer.X, originalPointer.Y)) throw new InvalidOperationException("恢复原指针失败"); } catch (Exception cleanup) { primary = primary is null ? cleanup : new AggregateException(primary, cleanup); } }
                if (primary is not null) ExceptionDispatchInfo.Capture(primary).Throw();
            });
            if (failures.Count > 0) throw new AggregateException("截屏正文独立用例失败", failures);
        }
        catch (Exception error) { failure = error; Console.Error.WriteLine(error); }
        finally
        {
            Application.Current.MainWindow = originalMain; Application.Current.ShutdownMode = originalShutdown; LocalizationService.Current.SetLanguage(language);
            try { await File.WriteAllTextAsync(Path.Combine(directory, "screenshot-layout.json"), JsonSerializer.Serialize(new { status = failure is null ? "PASS-OFFLINE-WPF-SCREENSHOT-LAYOUT" : "FAIL", scope = "Own Demo layout and synthetic WPF route; seeded completed crop only; no native save dialog/clipboard/drag-in-progress/physical click/Android/DPI proof", reports, error = failure?.ToString() }, JsonDefaults.Options)); }
            catch (Exception reportError) { Console.Error.WriteLine(reportError); failure = failure is null ? reportError : new AggregateException(failure, reportError); }
        }
        if (failure is not null) ExceptionDispatchInfo.Capture(failure).Throw();
        async Task Case(string name, int width, int height, bool portrait, Func<ShotCase, Task> action)
        {
            ShotCase? state = null; Exception? error = null; var cleaned = false;
            try { state = await ShotCase.Create(this, Path.Combine(directory, "c" + (++number).ToString("00")), width, height, portrait); await action(state); }
            catch (Exception primary) { error = primary; Console.Error.WriteLine(primary); if (state is not null) { state.Observe("primary-failure"); try { state.Save("failure"); } catch (Exception captureError) { error = new AggregateException(primary, captureError); } } }
            finally { if (state is not null) { try { await state.Cleanup(); cleaned = true; } catch (Exception cleanupError) { error = error is null ? cleanupError : new AggregateException(error, cleanupError); } } reports.Add(new { name, status = error is null ? "PASS" : "FAIL", cleaned, checks = state?.Checks.ToArray(), observations = state?.Observations.ToArray(), calls = state?.Backend.Calls(), error = error?.ToString() }); }
            if (error is not null) { failures.Add(new InvalidOperationException(name, error)); if (!cleaned) ExceptionDispatchInfo.Capture(error).Throw(); }
        }
    }

    private sealed class ShotCase
    {
        internal required MainWindow Window; internal required WorkspaceViewModel Vm; internal required ShotBackend Backend; internal required string Directory;
        internal readonly List<string> Checks = []; internal readonly List<object> Observations = [];
        private (double H, double V)? imageIntent, colorIntent;
        internal FrameworkElement Page => (FrameworkElement)Window.tabs.Items.Cast<TabItem>().Single(tab => Equals(tab.Tag, "截图 / 取色")).Content;
        internal DataGrid Grid => VisualDescendants<DataGrid>(Page).Single();
        internal Slider Zoom => VisualDescendants<Slider>(Page).Single();
        internal FrameworkElement Body => Ancestors(Window.screenshotScroll).OfType<Grid>().First(grid => grid.IsAncestorOf(Grid));
        internal ScrollViewer ColorScroll => VisualDescendants<ScrollViewer>(Grid).Single(scroll => ReferenceEquals(scroll.TemplatedParent, Grid));
        internal Button Tool(string caption) => VisualDescendants<Button>(Page).Single(button => button.ToolTip as string == caption || button.ToolTip as string == LocalizationService.Current.Get(caption) || AutomationProperties.GetName(button) == caption || AutomationProperties.GetName(button) == LocalizationService.Current.Get(caption));
        private static IEnumerable<DependencyObject> Ancestors(DependencyObject start) { for (var value = VisualTreeHelper.GetParent(start); value is not null; value = VisualTreeHelper.GetParent(value)) yield return value; }
        internal static object? Finite(Rect value) => value.IsEmpty || !double.IsFinite(value.X) || !double.IsFinite(value.Y) || !double.IsFinite(value.Width) || !double.IsFinite(value.Height) ? null : new { value.X, value.Y, value.Width, value.Height };
        internal static async Task<ShotCase> Create(MainWindow owner, string directory, int width, int height, bool portrait)
        {
            System.IO.Directory.CreateDirectory(directory); var backend = new ShotBackend(portrait); MatrixRuntime? runtime = null; WorkspaceViewModel? vm = null; MainWindow? window = null; CatalogRepository? metadata = null; RollingLog? log = null; ScriptScheduler? scripts = null; InputCoordinator? input = null; ApiServiceManager? api = null;
            try
            {
                metadata = new CatalogRepository(Path.Combine(directory, "db.sqlite3")); log = new RollingLog(Path.Combine(directory, "logs")); var catalog = new DeviceCatalog(backend); var automation = new AutomationService(backend, catalog); scripts = new ScriptScheduler(backend); input = new InputCoordinator(catalog, backend); api = new ApiServiceManager(catalog, backend, automation, log);
                var settings = new AppSettings { AutoStartApi = false, AutoConnect = false, SyncMouse = false, SyncKeyboard = false, Language = "zh-CN", WindowWidth = width, WindowHeight = height };
                runtime = new MatrixRuntime { Backend = backend, Devices = catalog, Automation = automation, Scripts = scripts, Input = input, Api = api, Metadata = metadata, Log = log, Settings = settings, FrozenConnectionSettings = settings, SettingsStore = new SettingsStore(Path.Combine(directory, "AppConfig.json")), IsDemo = true }; api.DeviceSnapshotProvider = runtime.GetDevices;
                vm = new WorkspaceViewModel(runtime, directory, true); await vm.RefreshDevicesAsync().WaitAsync(TimeSpan.FromSeconds(5)); await vm.SetMainAsync(vm.Devices[0]).WaitAsync(TimeSpan.FromSeconds(5)); vm.CurrentDevice = vm.Devices[1]; vm.Devices[0].IsSelected = true; vm.UpdateSelection();
                window = new MainWindow(vm) { Owner = owner, ShowActivated = true }; var state = new ShotCase { Window = window, Vm = vm, Backend = backend, Directory = directory };
                state.Check(vm.Demo && runtime.IsDemo && ReferenceEquals(runtime.Backend, backend) && !api.BasicRunning && !api.AutomatorRunning && api.DevicePorts.Count == 0, "自有一致Demo wrapper/API未启");
                window.Show(); await state.Settle(); state.Check(window.IsLoaded && new WindowInteropHelper(window).Handle != IntPtr.Zero && Math.Abs(window.ActualWidth - width) < .6 && Math.Abs(window.ActualHeight - height) < .6, "实际HWND请求尺寸不Clamp");
                window.navigation["截图 / 取色"].RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent)); await state.Settle(); backend.Clear(); state.Observe("shown-once"); return state;
            }
            catch (Exception primary)
            {
                var errors = new List<Exception> { primary }; async Task Attempt(Func<Task> action) { try { await action().WaitAsync(TimeSpan.FromSeconds(5)); } catch (Exception cleanup) { errors.Add(cleanup); } }
                try { window?.AbortStartup(); } catch (Exception cleanup) { errors.Add(cleanup); }
                if (vm is not null) await Attempt(vm.ShutdownAsync); else if (runtime is not null) await Attempt(() => runtime.DisposeAsync().AsTask()); else { if (api is not null) await Attempt(() => api.DisposeAsync().AsTask()); if (input is not null) await Attempt(() => input.DisposeAsync().AsTask()); if (scripts is not null) await Attempt(() => scripts.DisposeAsync().AsTask()); await Attempt(() => backend.DisposeAsync().AsTask()); try { metadata?.Dispose(); } catch (Exception cleanup) { errors.Add(cleanup); } if (log is not null) await Attempt(() => log.DisposeAsync().AsTask()); }
                if (errors.Count > 1) throw new AggregateException("截屏初始化与owned清理失败", errors); throw;
            }
        }
        internal void Check(bool value, string message) { if (!value) { Observe("check-failed", new { message }); throw new InvalidOperationException(message); } Checks.Add(message); }
        internal void Observe(string stage, object? detail = null) { try { Observations.Add(new { stage, detail, Window.ActualWidth, Window.ActualHeight, Window.IsActive, current = Vm.CurrentDevice?.Serial, main = Vm.MainDevice?.Serial, Vm.Busy, Vm.Error, Vm.Status, colors = Vm.Colors.Count, calls = Backend.Calls().Length, image = Finite(DeviceProbeBounds(Window.screenshotImage, Window.Shell)), imageViewport = Finite(DeviceProbeViewport(Window.Shell, Window.screenshotScroll)), gridVisible = Grid.IsVisible, keyboard = Keyboard.FocusedElement?.GetType().Name }); } catch (Exception error) { Observations.Add(new { stage, diagnosticError = error.ToString() }); } }
        internal Rect PageClip() => Rect.Intersect(new Rect(Window.Shell.RenderSize), DeviceProbeBounds(Page, Window.Shell));
        internal async Task Settle() { await Window.Dispatcher.InvokeAsync(Window.UpdateLayout, System.Windows.Threading.DispatcherPriority.Render); await Window.Dispatcher.InvokeAsync(Window.UpdateLayout, System.Windows.Threading.DispatcherPriority.ContextIdle); await Window.Dispatcher.InvokeAsync(Window.UpdateLayout, System.Windows.Threading.DispatcherPriority.ApplicationIdle); }
        internal async Task Wait(Func<bool> predicate, string message) { var watch = System.Diagnostics.Stopwatch.StartNew(); while (!predicate()) { if (watch.Elapsed > TimeSpan.FromSeconds(5)) throw new TimeoutException(message); await Settle(); await Task.Delay(5); } }
        internal async Task Invoke(ButtonBase button) { Check(button.IsVisible && button.IsEnabled && DeviceProbeContains(PageClip(), DeviceProbeBounds(button, Window.Shell)), "真实可执行完整按钮：" + AutomationProperties.GetName(button)); var peer = UIElementAutomationPeer.CreatePeerForElement(button) ?? throw new InvalidOperationException("缺少实际按钮peer"); if (peer.GetPattern(PatternInterface.Invoke) is IInvokeProvider invoke) invoke.Invoke(); else if (peer.GetPattern(PatternInterface.Toggle) is IToggleProvider toggle) toggle.Toggle(); else throw new InvalidOperationException("实际按钮缺少Invoke/Toggle"); await Window.Dispatcher.InvokeAsync(() => { }, System.Windows.Threading.DispatcherPriority.Input); await Settle(); }
        internal async Task Loaded() { await Wait(() => !Vm.Busy && Vm.DebugImage is not null && ReferenceEquals(Window.screenshotImage.Source, Vm.DebugImage), "实际截图未完成"); await Settle(); Check(Vm.Error.Length == 0, "实际截图成功无错误"); }
        internal async Task Refresh() { await Invoke(Tool("刷新截图")); await Loaded(); Check(Backend.Calls().Any(call => call.Operation == "Screenshot" && call.Serial == "DEMO-002") && Backend.Calls().All(call => call.Operation != "Dump"), "实际原刷新Command从B读取截图，不偷跑节点"); }
        internal void ImageGeometry() { var frame = DeviceProbeBounds(Window.screenshotScroll, Window.Shell); var viewport = DeviceProbeViewport(Window.Shell, Window.screenshotScroll); var visible = Rect.Intersect(DeviceProbeBounds(Window.screenshotImage, Window.Shell), viewport); visible.Intersect(PageClip()); Observe("original-image-geometry", new { frame = Finite(frame), viewport = Finite(viewport), visible = Finite(visible) }); Check(Window.screenshotScroll.IsVisible && frame.Height >= 160 && DeviceProbeContains(PageClip(), frame) && !visible.IsEmpty && visible.Width >= 100 && visible.Height >= 100, "默认1倍原图在真实SCP/祖先视口内至少100x100，图框高160"); }
        internal void ToolsGeometry()
        {
            foreach (var caption in new[] { "刷新截图", "保存 PNG", "保存裁剪", "原始大小" }) DeviceProbe44(Tool(caption), Window.Shell, PageClip(), Check);
            DeviceProbe44(Window.screenshotMode, Window.Shell, PageClip(), Check); Check(!string.IsNullOrWhiteSpace(AutomationProperties.GetName(Window.screenshotMode)), "原取色/裁剪操作Combo具有明确可访问名称");
            var bounds = DeviceProbeBounds(Zoom, Window.Shell); var thumb = VisualDescendants<Thumb>(Zoom).Single(); var hit = Window.Shell.InputHitTest(thumb.TranslatePoint(new Point(thumb.ActualWidth / 2, thumb.ActualHeight / 2), Window.Shell)) as DependencyObject;
            Observe("slider-target", new { bounds = Finite(bounds), thumb = Finite(DeviceProbeBounds(thumb, Window.Shell)), hit = hit?.GetType().Name, wholeControl44Only = true });
            Check(Zoom.ActualWidth >= 44 && Zoom.ActualHeight >= 44 && DeviceProbeContains(PageClip(), bounds) && hit is not null && (ReferenceEquals(hit, thumb) || thumb.IsAncestorOf(hit)), "Slider整体44可达且真实Thumb中心可命中，不声称Thumb或四角44"); Check(!string.IsNullOrWhiteSpace(AutomationProperties.GetName(Zoom)), "原缩放Slider具有明确可访问名称");
            var track = VisualDescendants<Track>(Zoom).Single();
            foreach (var target in new[] { (Role: "decrease", Button: track.DecreaseRepeatButton), (Role: "increase", Button: track.IncreaseRepeatButton) })
            {
                Check(target.Button is not null, "原Slider Track存在实际轨道按钮：" + target.Role);
                var button = target.Button!; var applicable = button.IsVisible && button.ActualWidth > 0 && button.ActualHeight > 0; var targetBounds = DeviceProbeBounds(button, Window.Shell);
                if (!applicable) { Observe("slider-track-center", new { target.Role, applicable, bounds = Finite(targetBounds), tested = false }); continue; }
                var center = new Point(targetBounds.X + targetBounds.Width / 2, targetBounds.Y + targetBounds.Height / 2); var targetHit = Window.Shell.InputHitTest(center) as DependencyObject; var ownsHit = targetHit is not null && (ReferenceEquals(button, targetHit) || button.IsAncestorOf(targetHit));
                Observe("slider-track-center", new { target.Role, applicable, bounds = Finite(targetBounds), targetType = button.GetType().Name, hitType = targetHit?.GetType().Name, ownsHit, tested = true, invoked = false, child44Claimed = false });
                Check(DeviceProbeContains(PageClip(), targetBounds) && ownsHit, "实际可见轨道按钮完整位于页面且中心命中归原按钮：" + target.Role);
            }
        }
        internal async Task ZoomTo(double value) { var peer = UIElementAutomationPeer.CreatePeerForElement(Zoom); var range = peer?.GetPattern(PatternInterface.RangeValue) as IRangeValueProvider ?? throw new InvalidOperationException("缩放缺实际RangeValue provider"); Check(range.Minimum == .1 && range.Maximum == 3, "保留原缩放范围.1至3"); range.SetValue(value); await Settle(); Check(Math.Abs(Zoom.Value - value) < .0001 && Math.Abs(Window.screenshotScale.ScaleX - value) < .0001 && Math.Abs(Window.screenshotScale.ScaleY - value) < .0001, "实际Slider ValueChanged更新原ScaleTransform：" + value); }
        internal void SeedColorsAndCrop() { for (var index = 0; index < 100; index++) Vm.SamplePixel(index * (Vm.DebugImage!.PixelWidth - 1) / 99, index * (Vm.DebugImage.PixelHeight - 1) / 99); Window.crop = new Int32Rect(10, 10, 40, 50); Window.cropStart = null; Window.cropRectangle.Visibility = Visibility.Visible; Window.cropRectangle.Width = 40; Window.cropRectangle.Height = 50; Canvas.SetLeft(Window.cropRectangle, 10); Canvas.SetTop(Window.cropRectangle, 10); Observe("layout-seed", new { legalSamples = Vm.Colors.Count, lastPixel = Vm.Colors[0], completedCropSeedOnly = true, mousePathClaimed = false }); Check(Vm.Colors.Count == 100 && Vm.Colors[0] == new PixelSample(Vm.DebugImage!.PixelWidth - 1, Vm.DebugImage.PixelHeight - 1, 255, 255, 255), "100条跨原图全范围合法样本包含实际图尾白像素和四位坐标，仅作色表布局数据"); }
        internal async Task ReadColors()
        {
            foreach (var sample in new[] { Vm.Colors[0], Vm.Colors[50], Vm.Colors[99] })
                foreach (var column in Grid.Columns)
                {
                    Grid.ScrollIntoView(sample, column); await Settle(); var row = Grid.ItemContainerGenerator.ContainerFromItem(sample) as DataGridRow ?? throw new InvalidOperationException("真实色值行未生成"); var cell = VisualDescendants<DataGridCell>(row).Single(value => ReferenceEquals(value.Column, column)); var text = VisualDescendants<TextBlock>(cell).First();
                    var expected = column.DisplayIndex switch { 0 => sample.Position, 1 => sample.Hex, _ => sample.Rgb }; var viewport = DeviceProbeViewport(Window.Shell, ColorScroll); var textBounds = DeviceProbeBounds(text, Window.Shell); var formatted = new FormattedText(text.Text, CultureInfo.CurrentUICulture, text.FlowDirection, new Typeface(text.FontFamily, text.FontStyle, text.FontWeight, text.FontStretch), text.FontSize, text.Foreground, VisualTreeHelper.GetDpi(text).PixelsPerDip);
                    var fullVisible = text.ActualWidth + .6 >= formatted.WidthIncludingTrailingWhitespace && DeviceProbeContains(viewport, textBounds); var fullTooltip = text.ToolTip?.ToString() == expected || cell.ToolTip?.ToString() == expected;
                    Observe("color-cell", new { sample.X, sample.Y, column = column.DisplayIndex, expected, text.Text, fullVisible, fullTooltip, formattedWidth = formatted.WidthIncludingTrailingWhitespace, textBounds = Finite(textBounds), viewport = Finite(viewport), ColorScroll.HorizontalOffset, ColorScroll.VerticalOffset });
                    Check(text.Text == expected && !Rect.Intersect(textBounds, viewport).IsEmpty && fullVisible, "首中末行各坐标/HEX/RGB文字实际完整可见，不能只拿未打开tooltip或字符串存在当阅读证据");
                }
        }
        internal void RememberOffsets() { if (Window.screenshotScroll.IsVisible && Window.screenshotScroll.ViewportWidth > 0 && Window.screenshotScroll.ViewportHeight > 0) imageIntent = (Window.screenshotScroll.HorizontalOffset, Window.screenshotScroll.VerticalOffset); if (Grid.IsVisible && ColorScroll.ViewportWidth > 0 && ColorScroll.ViewportHeight > 0) colorIntent = (ColorScroll.HorizontalOffset, ColorScroll.VerticalOffset); }
        private void CheckOffsets() { if (Window.screenshotScroll.IsVisible && imageIntent is { } image) CheckOffset(Window.screenshotScroll, image, "画面"); if (Grid.IsVisible && colorIntent is { } color) CheckOffset(ColorScroll, color, "色值"); RememberOffsets(); }
        private void CheckOffset(ScrollViewer scroll, (double H, double V) expected, string caption) { var horizontal = Math.Min(expected.H, scroll.ScrollableWidth); var vertical = Math.Min(expected.V, scroll.ScrollableHeight); Observe("visible-scroll", new { caption, expected.H, expected.V, horizontal, vertical, scroll.HorizontalOffset, scroll.VerticalOffset, scroll.ScrollableWidth, scroll.ScrollableHeight, scroll.ViewportWidth, scroll.ViewportHeight }); Check(scroll.ViewportWidth > 0 && scroll.ViewportHeight > 0 && Math.Abs(scroll.HorizontalOffset - horizontal) < 1 && Math.Abs(scroll.VerticalOffset - vertical) < 1, "回可见按上次有效offset和新scrollable合法clamp，不采隐藏viewport归零：" + caption); }
        internal async Task Resize(double width, double height) { RememberOffsets(); Window.Width = width; Window.Height = height; await Settle(); Check(Math.Abs(Window.ActualWidth - width) < .6 && Math.Abs(Window.ActualHeight - height) < .6, "实际截屏resize不Clamp"); LayoutGeometry(); CheckOffsets(); Observe("resized"); }
        private void LayoutGeometry()
        {
            var image = DeviceProbeBounds(Window.screenshotScroll, Window.Shell); var colors = DeviceProbeBounds(Grid, Window.Shell); var clip = PageClip(); var wide = Body.ActualWidth >= 640;
            Observe("body-layout", new { wide, bodyWidth = Body.ActualWidth, image = Finite(image), colors = Finite(colors), imageVisible = Window.screenshotScroll.IsVisible, colorsVisible = Grid.IsVisible });
            if (wide) Check(Window.screenshotScroll.IsVisible && Grid.IsVisible && image.Right <= colors.Left + .6 && Grid.ActualWidth >= 280 && DeviceProbeContains(clip, image) && DeviceProbeContains(clip, colors), "宽屏原画面/色值横向有序不重叠且均在正文内");
            else { Check(Window.screenshotScroll.IsVisible != Grid.IsVisible, "窄屏只有一个原正文可见"); var active = Window.screenshotScroll.IsVisible ? (FrameworkElement)Window.screenshotScroll : Grid; Check(active.ActualWidth >= Body.ActualWidth - 24 && active.ActualHeight >= 100 && DeviceProbeContains(clip, DeviceProbeBounds(active, Window.Shell)), "窄分段原正文自然填充而非残留窄列"); }
        }
        internal async Task Pane(string caption)
        {
            RememberOffsets(); if (Body.ActualWidth >= 640) { LayoutGeometry(); CheckOffsets(); return; }
            var button = VisualDescendants<ButtonBase>(Page).Single(value => value.IsVisible && AutomationProperties.GetName(value) == caption); var hadFocus = Grid.IsKeyboardFocusWithin;
            DeviceProbe44(button, Window.Shell, PageClip(), Check); await Invoke(button); Check(caption == "画面" ? Window.screenshotScroll.IsVisible && !Grid.IsVisible : Grid.IsVisible && !Window.screenshotScroll.IsVisible, "窄屏同原控件唯一分段：" + caption);
            if (hadFocus && !Grid.IsVisible) Check(!Grid.IsKeyboardFocusWithin && (Keyboard.FocusedElement as FrameworkElement)?.IsVisible == true && (button.IsKeyboardFocusWithin || Window.screenshotScroll.IsKeyboardFocusWithin), "切走色表不遗留隐藏焦点"); LayoutGeometry(); CheckOffsets();
        }
        internal void Save(string name) { var path = Path.Combine(Directory, name + ".png"); Window.SaveRender(path); var bytes = File.ReadAllBytes(path); Check(bytes.Length > 0, "必需截屏布局截图已保存：" + name); Observations.Add(new { stage = "saved-render", name, bytes = bytes.Length, sha256 = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bytes)) }); }
        internal async Task Cleanup() { var errors = new List<Exception>(); try { Backend.ReleaseAll(); await Wait(() => !Vm.Busy, "自有截屏任务未排空"); } catch (Exception error) { errors.Add(error); } try { Check(Backend.GatesComplete, "全部自有截图TCS释放且已进入"); } catch (Exception error) { errors.Add(error); } try { Window.AbortStartup(); } catch (Exception error) { errors.Add(error); } try { await Vm.ShutdownAsync().WaitAsync(TimeSpan.FromSeconds(5)); } catch (Exception error) { errors.Add(error); } if (errors.Count > 0) throw new AggregateException("截屏owned清理失败", errors); }
    }

    private sealed record ShotSnapshot(object Image, object Canvas, object Overlay, object Scroll, object Scale, object Mode, object Grid, object Colors, object? Source, PixelSample[] Samples, object? Selected, double Zoom, int Operation, Int32Rect? Crop, double CropX, double CropY, double CropWidth, double CropHeight, Visibility CropVisible, DeviceRow? Current, DeviceRow? Main, DeviceRow[] Targets, NodeRow[] Nodes, NodeRow? Node)
    {
        internal static ShotSnapshot Take(ShotCase state) => new(state.Window.screenshotImage, state.Window.screenshotCanvas, state.Window.cropRectangle, state.Window.screenshotScroll, state.Window.screenshotScale, state.Window.screenshotMode, state.Grid, state.Vm.Colors, state.Vm.DebugImage, state.Vm.Colors.ToArray(), state.Grid.SelectedItem, state.Zoom.Value, state.Window.screenshotMode.SelectedIndex, state.Window.crop, System.Windows.Controls.Canvas.GetLeft(state.Window.cropRectangle), System.Windows.Controls.Canvas.GetTop(state.Window.cropRectangle), state.Window.cropRectangle.Width, state.Window.cropRectangle.Height, state.Window.cropRectangle.Visibility, state.Vm.CurrentDevice, state.Vm.MainDevice, state.Vm.Targets().ToArray(), state.Vm.Nodes.ToArray(), state.Vm.SelectedNode);
        internal void Same(ShotCase state) { var sameControls = ReferenceEquals(Image, state.Window.screenshotImage) && ReferenceEquals(Canvas, state.Window.screenshotCanvas) && ReferenceEquals(Overlay, state.Window.cropRectangle) && ReferenceEquals(Scroll, state.Window.screenshotScroll) && ReferenceEquals(Scale, state.Window.screenshotScale) && ReferenceEquals(Scale, state.Window.screenshotCanvas.LayoutTransform) && ReferenceEquals(Mode, state.Window.screenshotMode) && ReferenceEquals(Grid, state.Grid) && ReferenceEquals(Colors, state.Vm.Colors) && ReferenceEquals(Colors, state.Grid.ItemsSource); var sameData = ReferenceEquals(Source, state.Vm.DebugImage) && ReferenceEquals(Source, state.Window.screenshotImage.Source) && Samples.Length == state.Vm.Colors.Count && Samples.Zip(state.Vm.Colors).All(pair => ReferenceEquals(pair.First, pair.Second)) && ReferenceEquals(Selected, state.Grid.SelectedItem); var sameCrop = Crop == state.Window.crop && CropX == System.Windows.Controls.Canvas.GetLeft(state.Window.cropRectangle) && CropY == System.Windows.Controls.Canvas.GetTop(state.Window.cropRectangle) && CropWidth == state.Window.cropRectangle.Width && CropHeight == state.Window.cropRectangle.Height && CropVisible == state.Window.cropRectangle.Visibility; var context = ReferenceEquals(Current, state.Vm.CurrentDevice) && ReferenceEquals(Main, state.Vm.MainDevice) && Targets.SequenceEqual(state.Vm.Targets()) && Nodes.SequenceEqual(state.Vm.Nodes) && ReferenceEquals(Node, state.Vm.SelectedNode); var pixels = state.Window.screenshotCanvas.Width == state.Vm.DebugImage?.PixelWidth && state.Window.screenshotCanvas.Height == state.Vm.DebugImage?.PixelHeight && state.Window.screenshotImage.Width == state.Vm.DebugImage?.PixelWidth && state.Window.screenshotImage.Height == state.Vm.DebugImage?.PixelHeight; state.Observe("preserved-state", new { sameControls, sameData, sameCrop, context, pixels, expectedZoom = Zoom, actualZoom = state.Zoom.Value, expectedMode = Operation, actualMode = state.Window.screenshotMode.SelectedIndex }); state.Check(sameControls && sameData && sameCrop && context && pixels && Zoom == state.Zoom.Value && Zoom == state.Window.screenshotScale.ScaleX && Zoom == state.Window.screenshotScale.ScaleY && Operation == state.Window.screenshotMode.SelectedIndex, "同控件重排保留源/色值/选择/完成crop像素/zoom/mode/MainA CurrentB Targets Nodes"); }
    }
    private sealed record ShotCall(int Sequence, string Operation, string? Serial);
    private sealed class ShotGate(Exception? error) { internal readonly TaskCompletionSource Entered = new(TaskCreationOptions.RunContinuationsAsynchronously); internal readonly TaskCompletionSource Release = new(TaskCreationOptions.RunContinuationsAsynchronously); internal readonly Exception? Error = error; internal bool Claimed; }
    private sealed class ShotBackend(bool portrait) : IDeviceBackend
    {
        private readonly DemoDeviceBackend inner = new(); private readonly List<ShotCall> calls = []; private readonly List<ShotGate> gates = [];
        private int Width => portrait ? 900 : 1600; private int Height => portrait ? 1600 : 900;
        internal ShotCall[] Calls() { lock (calls) return calls.ToArray(); } internal void Clear() { lock (calls) calls.Clear(); } private void Record(string operation, string? serial) { lock (calls) calls.Add(new(calls.Count + 1, operation, serial)); }
        internal ShotGate Hold(Exception? error = null) { var gate = new ShotGate(error); gates.Add(gate); return gate; } internal bool GatesComplete => gates.All(gate => gate.Entered.Task.IsCompleted && gate.Release.Task.IsCompleted); internal void ReleaseAll() { foreach (var gate in gates) { gate.Release.TrySetResult(); if (!gate.Claimed) gate.Entered.TrySetCanceled(); } }
        public async Task<IReadOnlyList<DeviceInfo>> DiscoverAsync(CancellationToken ct = default) { Record("Discover", null); return (await inner.DiscoverAsync(ct)).Select(info => info.Serial == "DEMO-002" ? info with { Width = Width, Height = Height } : info).ToArray(); }
        public async Task<byte[]> ScreenshotAsync(string serial, CancellationToken ct = default) { Record("Screenshot", serial); await inner.ScreenshotAsync(serial, ct); var gate = gates.FirstOrDefault(value => !value.Claimed); if (gate is not null) { gate.Claimed = true; gate.Entered.TrySetResult(); await gate.Release.Task.WaitAsync(ct); if (gate.Error is not null) ExceptionDispatchInfo.Capture(gate.Error).Throw(); } return PngImage.Create(Width, Height, (x, y) => x == Width - 1 && y == Height - 1 ? ((byte)255, (byte)255, (byte)255) : ((byte)(x % 251), (byte)(y % 251), (byte)112)); }
        public Task<AndroidNode> DumpAsync(string serial, CancellationToken ct = default) { Record("Dump", serial); return inner.DumpAsync(serial, ct); }
        public Task<string> ShellAsync(string serial, string command, bool useRoot = false, CancellationToken ct = default) { Record("Shell", serial); return inner.ShellAsync(serial, command, useRoot, ct); }
        public Task TapAsync(string serial, int x, int y, CancellationToken ct = default) { Record("Tap", serial); return inner.TapAsync(serial, x, y, ct); }
        public Task SwipeAsync(string serial, int x1, int y1, int x2, int y2, int durationMs, CancellationToken ct = default) { Record("Swipe", serial); return inner.SwipeAsync(serial, x1, y1, x2, y2, durationMs, ct); }
        public Task KeyEventAsync(string serial, int code, CancellationToken ct = default) { Record("Key", serial); return inner.KeyEventAsync(serial, code, ct); }
        public Task TextAsync(string serial, string text, CancellationToken ct = default) { Record("Text", serial); return inner.TextAsync(serial, text, ct); }
        public Task<string> StartAppAsync(string serial, string packageName, string? activity = null, CancellationToken ct = default) { Record("StartApp", serial); return inner.StartAppAsync(serial, packageName, activity, ct); }
        public Task<string> StopAppAsync(string serial, string packageName, CancellationToken ct = default) { Record("StopApp", serial); return inner.StopAppAsync(serial, packageName, ct); }
        public Task<bool> ClearAppAsync(string serial, string packageName, CancellationToken ct = default) { Record("ClearApp", serial); return inner.ClearAppAsync(serial, packageName, ct); }
        public Task<IReadOnlyList<AppInfo>> ListAppsAsync(string serial, bool showSystem = false, bool needVersion = true, CancellationToken ct = default) { Record("ListApps", serial); return inner.ListAppsAsync(serial, showSystem, needVersion, ct); }
        public Task InstallAsync(string serial, string localPath, CancellationToken ct = default) { Record("Install", serial); return inner.InstallAsync(serial, localPath, ct); }
        public Task UninstallAsync(string serial, string packageName, CancellationToken ct = default) { Record("Uninstall", serial); return inner.UninstallAsync(serial, packageName, ct); }
        public Task PushAsync(string serial, string localPath, string remotePath, IProgress<double>? progress = null, CancellationToken ct = default) { Record("Push", serial); return inner.PushAsync(serial, localPath, remotePath, progress, ct); }
        public Task PullAsync(string serial, string remotePath, string localPath, IProgress<double>? progress = null, CancellationToken ct = default) { Record("Pull", serial); return inner.PullAsync(serial, localPath, remotePath, progress, ct); }
        public Task SetNodeTextAsync(string serial, NodeSelector selector, string text, CancellationToken ct = default) { Record("NodeText", serial); return inner.SetNodeTextAsync(serial, selector, text, ct); }
        public ValueTask DisposeAsync() { ReleaseAll(); return inner.DisposeAsync(); }
    }
}
