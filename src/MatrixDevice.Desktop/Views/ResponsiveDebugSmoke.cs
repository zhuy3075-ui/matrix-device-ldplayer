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
using System.Windows.Shapes;
using System.Windows.Threading;
using MatrixDevice.Core;
using MatrixDevice.Features;
using MatrixDevice.Host;
using Path = System.IO.Path;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    private async Task VerifyResponsiveDebugAsync()
    {
        if (!ViewModel.Demo || !ViewModel.Runtime.IsDemo || !Dispatcher.CheckAccess() || Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
            throw new InvalidOperationException("节点布局验收仅限自有一致Demo/STA。");
        var directory = Path.Combine(Path.GetFullPath(ViewModel.DataDirectory), "artifacts", "nl", Guid.NewGuid().ToString("N")[..8]);
        if (!Path.IsPathFullyQualified(directory) || directory.Length > 190) throw new InvalidOperationException("节点布局验收需要短绝对artifacts路径：" + directory.Length);
        Directory.CreateDirectory(directory);
        var originalMain = Application.Current.MainWindow; var originalShutdown = Application.Current.ShutdownMode; var language = LocalizationService.Current.Language;
        var reports = new List<object>(); var failures = new List<Exception>(); Exception? failure = null; var number = 0;
        try
        {
            foreach (var size in new[] { (W: 390, H: 640), (W: 390, H: 844), (W: 768, H: 1024), (W: 1000, H: 700), (W: 1440, H: 900), (W: 1920, H: 1080) })
            {
                await Case($"geometry-{size.W}-{size.H}", size.W, size.H, size.H > size.W, async state =>
                {
                    await state.Refresh();
                    state.PictureGeometry();
                    state.LayoutGeometry("画面");
                    foreach (var tool in state.Tools) DeviceProbe44(tool, state.Window.Shell, state.PageClip(), state.Check);
                    await state.Mode("节点"); state.Check(state.Window.nodeTree.IsVisible && state.Window.nodeTree.ActualHeight >= 100, "节点树拥有实际可滚正文");
                    var leaves = state.Vm.FlattenNodes().Where(node => node.Source.Clickable).ToArray();
                    foreach (var leaf in new[] { leaves.First(), leaves[leaves.Length / 2], leaves.Last() })
                    {
                        var item = state.Window.nodeTreeItems[leaf]; item.BringIntoView(); await state.Settle(); state.HeaderHit(item);
                    }
                    var chosen = leaves[leaves.Length / 2]; var chosenItem = state.Window.nodeTreeItems[chosen]; chosenItem.BringIntoView(); await state.Settle();
                    var provider = new TreeViewItemAutomationPeer(chosenItem).GetPattern(PatternInterface.SelectionItem) as ISelectionItemProvider ?? throw new InvalidOperationException("真实TreeItem缺少选择provider");
                    provider.Select(); await state.Settle();
                    state.Check(ReferenceEquals(state.Vm.SelectedNode, chosen) && chosenItem.IsSelected && state.Window.nodeRectangles[chosen].StrokeThickness == 4 && state.Properties.Text == state.Vm.NodeProperties, "真实树选择联动VM/属性与原矩形高亮");
                    await state.Invoke(state.Tool("折叠")); state.Check(state.Window.nodeTreeItems.Values.All(item => !item.IsExpanded), "原折叠按钮实际执行Command");
                    await state.Invoke(state.Tool("展开")); state.Check(state.Window.nodeTreeItems.Values.All(item => item.IsExpanded), "原展开按钮实际执行Command");
                    state.Window.nodeTreeItems[leaves.First()].BringIntoView(); await state.Settle();
                    state.Check(Rect.Intersect(DeviceProbeBounds(state.Window.nodeTreeItems[leaves.Last()], state.Window.Shell), DeviceProbeViewport(state.Window.Shell, state.TreeScroll)).IsEmpty, "画布点选前末端节点确实位于原树视口外");
                    await state.Mode("画面"); state.PictureGeometry();
                    var fromPicture = leaves.Last(); var rectangle = state.Window.nodeRectangles[fromPicture]; var rectangleBounds = DeviceProbeBounds(rectangle, state.Window.Shell);
                    var center = new Point(rectangleBounds.X + rectangleBounds.Width / 2, rectangleBounds.Y + rectangleBounds.Height / 2);
                    var hit = state.Window.Shell.InputHitTest(center) as DependencyObject;
                    state.Check(hit is UIElement && (ReferenceEquals(hit, rectangle) || rectangle.IsAncestorOf(hit)), "实际画布像素框中心命中原矩形，不放大设备坐标");
                    ((UIElement)hit!).RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left) { RoutedEvent = Mouse.MouseDownEvent }); await state.Settle();
                    state.Check(ReferenceEquals(state.Vm.SelectedNode, fromPicture) && state.Window.nodeTreeItems[fromPicture].IsSelected && rectangle.StrokeThickness == 4, "命中元素的WPF底层鼠标路由更新树选择与高亮，非物理指针证据");
                    await state.Mode("节点"); state.HeaderHit(state.Window.nodeTreeItems[fromPicture]);
                    state.Check(state.Window.nodeTreeItems[fromPicture].IsSelected && state.Properties.Text == state.Vm.NodeProperties, "隐藏树中点选末节点后，实际节点分段显示并高亮该项及对应属性");
                    var longNode = leaves.Last(); var longItem = state.Window.nodeTreeItems[longNode]; longItem.BringIntoView(); await state.Settle();
                    ((ISelectionItemProvider)new TreeViewItemAutomationPeer(longItem).GetPattern(PatternInterface.SelectionItem)).Select(); await state.Settle();
                    var fold = state.Window.nodeTreeItems.Values.First(item => item.Items.Count > 0 && !item.IsAncestorOf(longItem)); fold.IsExpanded = false;
                    var treeFocusReturned = state.Window.nodeTree.Focus(); await state.Settle();
                    var treeFocused = Keyboard.FocusedElement as FrameworkElement;
                    var treeFocusedVisible = treeFocused?.IsVisible == true;
                    var treeFocusedOwned = treeFocused is not null && (ReferenceEquals(treeFocused, state.Window.nodeTree) || state.Window.nodeTree.IsAncestorOf(treeFocused));
                    state.Observe("tree-focus", new { treeFocusReturned, state.Window.IsActive, keyboardFocusWithin = state.Window.nodeTree.IsKeyboardFocusWithin, focusedType = treeFocused?.GetType().Name, focusedName = treeFocused?.Name, treeFocusedVisible, treeFocusedOwned });
                    state.Check(state.Window.IsActive && state.Window.nodeTree.IsKeyboardFocusWithin && treeFocusedVisible && treeFocusedOwned, "自有树一次Focus后实际可见键盘焦点归原树，不强求容器返回true，不Activate或重试抢焦点");
                    var treeScroll = state.TreeScroll; treeScroll.ScrollToVerticalOffset(treeScroll.ScrollableHeight * .6); await state.Settle();
                    var treeIntent = state.TreeIntent();
                    var treeSnapshot = NodeLayoutSnapshot.Take(state, treeIntent); var treeCalls = state.Backend.Calls().Length;
                    await state.Resize(390, size.H); state.Check(state.Window.nodeTree.IsVisible && state.Window.nodeTree.IsKeyboardFocusWithin, "焦点树缩窄后仍处可见分段且持焦"); treeSnapshot.References(state);
                    await state.Resize(size.W, size.H); treeSnapshot.References(state); state.Check(state.Window.nodeTree.IsVisible && state.Window.nodeTree.IsKeyboardFocusWithin, "树恢复原尺寸仍可见且持焦"); state.CheckTreeIntent(treeIntent);
                    state.Check(state.Backend.Calls().Length == treeCalls, "树焦点resize不新增任何后端调用");
                    await state.Mode("属性"); state.Check(state.Properties.IsVisible && state.Properties.ActualHeight >= 100 && state.Properties.Text.Length > 1500, "长属性保持原只读文本与独立正文");
                    state.Properties.ScrollToEnd(); await state.Settle(); state.PropertyTail();
                    state.Properties.Select(state.Properties.Text.Length - 24, 12); state.Properties.ScrollToEnd(); await state.Settle();
                    var focusReturned = state.Properties.Focus(); state.Observe("properties-focus", new { focusReturned, state.Window.IsActive, keyboardFocused = state.Properties.IsKeyboardFocused });
                    state.Check(focusReturned && state.Window.IsActive && state.Properties.IsKeyboardFocused, "自有窗口一次Focus真实前置成立，不Activate或重试抢焦点");
                    var snapshot = NodeLayoutSnapshot.Take(state, treeIntent); var calls = state.Backend.Calls().Length;
                    await state.Resize(390, size.H); state.Check(state.Properties.IsVisible && state.Properties.IsKeyboardFocusWithin, "焦点属性缩窄后仍处可见分段"); snapshot.References(state);
                    await state.Resize(size.W, size.H); snapshot.References(state); state.Check(state.Properties.IsVisible, "恢复原尺寸属性仍可达"); snapshot.PropertyState(state);
                    if (size.W == 1000)
                    {
                        foreach (var target in new[] { 639d, 640d, 641d, 919d, 920d, 921d })
                        {
                            await state.Resize(target < 700 ? 768 : 1440, size.H); var chrome = state.Window.ActualWidth - state.Body.ActualWidth;
                            await state.Resize(target + chrome, size.H); state.Check(Math.Abs(state.Body.ActualWidth - target) < .6, "实际正文断点±1：" + target); snapshot.References(state);
                            state.Check(state.Properties.IsVisible && state.Properties.IsKeyboardFocusWithin, "正文断点不把实际焦点留在隐藏属性");
                        }
                        await state.Resize(size.W, size.H); snapshot.References(state); snapshot.PropertyState(state);
                    }
                    await state.Mode("节点"); state.Check(!state.Properties.IsKeyboardFocusWithin || state.Properties.IsVisible, "切走属性不把焦点遗留在Collapsed正文");
                    await state.Settle(); state.CheckTreeIntent(treeIntent); snapshot.References(state);
                    state.Check(state.Backend.Calls().Length == calls, "纯resize/分段/展开与选择不新增Discover/截图/Dump或其他后端调用");
                    await state.Mode("画面"); state.PictureGeometry(); state.Save("picture"); await state.Mode("节点"); state.Save("tree"); await state.Mode("属性"); state.Save("properties");
                });
            }
            await Case("empty", 390, 640, true, async state =>
            {
                state.Vm.CurrentDevice = null; await state.Settle();
                state.Check(state.Vm.DebugImage is null && state.Vm.Nodes.Count == 0 && state.Window.nodeTree.Items.Count == 0 && state.Window.nodeRectangles.Count == 0 && !state.Tool("刷新截图和节点").IsEnabled, "无目标空态是真实VM/画布/树与禁用刷新");
                state.Check(state.Backend.Calls().Length == 0, "空态导航未偷偷自动加载"); state.Save("empty");
            });
            await Case("busy", 390, 844, true, async state =>
            {
                var gate = state.Backend.HoldShot(); await state.Invoke(state.Tool("刷新截图和节点")); await gate.Entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
                state.Check(state.Vm.Busy && !state.Tool("刷新截图和节点").IsEnabled, "实际截图门阻塞时原刷新Command禁用"); state.Observe("busy-entered"); state.Save("busy");
                gate.Release.TrySetResult(); await state.Loaded(); state.Check(state.Backend.Calls().Count(call => call.Operation == "Screenshot") == 1 && state.Backend.Calls().Count(call => call.Operation == "Dump") == 1, "Busy完成无重入设备读取");
            });
            await Case("failure-recovery", 390, 640, true, async state =>
            {
                var injected = new DeviceException("controlled node layout failure"); var gate = state.Backend.HoldShot(injected); await state.Invoke(state.Tool("刷新截图和节点")); await gate.Entered.Task.WaitAsync(TimeSpan.FromSeconds(5)); gate.Release.TrySetResult(); await state.Wait(() => !state.Vm.Busy, "失败未恢复Busy"); await state.Settle();
                state.Check(state.Vm.Error == "刷新截图与节点失败：" + UiErrorPresentation.Describe(injected) && state.Vm.Status == "操作失败" && state.Vm.Nodes.Count == 0 && state.Window.nodeTree.Items.Count == 0, "真实后端错误沿原安全中文反馈且没有假节点"); state.Save("error");
                await state.Refresh(); state.Check(state.Vm.Error.Length == 0 && state.Vm.Nodes.Count > 0, "原刷新按钮实际重试恢复");
            });
            await Case("property-new-input", 1440, 900, true, async state =>
            {
                await state.Refresh();
                foreach (var signal in new[] { "wheel", "thumb", "key" }) await state.ReplacePropertyIntent(signal);
            });
            await Case("property-new-content", 1440, 900, true, async state =>
            {
                state.Backend.LongSecondLeaf = true;
                await state.Refresh();
                foreach (var signal in new[] { "selection", "text" }) await state.ReplacePropertyIntent(signal);
            });
            if (failures.Count > 0) throw new AggregateException("节点正文独立布局用例失败", failures);
        }
        catch (Exception error) { failure = error; Console.Error.WriteLine(error); }
        finally
        {
            Application.Current.MainWindow = originalMain; Application.Current.ShutdownMode = originalShutdown; LocalizationService.Current.SetLanguage(language);
            try { await File.WriteAllTextAsync(Path.Combine(directory, "node-layout.json"), JsonSerializer.Serialize(new { status = failure is null ? "PASS-OFFLINE-WPF-NODE-LAYOUT" : "FAIL", scope = "Own Demo HWND/control layout, command seed, UIA selection and synthetic WPF rectangle route; no automatic-load/physical pointer/Android/native export dialog/font-scale/DPI proof", reports, error = failure?.ToString() }, JsonDefaults.Options)); }
            catch (Exception reportError) { Console.Error.WriteLine(reportError); failure = failure is null ? reportError : new AggregateException(failure, reportError); }
        }
        if (failure is not null) ExceptionDispatchInfo.Capture(failure).Throw();

        async Task Case(string name, int width, int height, bool portrait, Func<NodeLayoutCase, Task> action)
        {
            NodeLayoutCase? state = null; Exception? error = null; var cleaned = false;
            try { state = await NodeLayoutCase.Create(this, Path.Combine(directory, "c" + (++number).ToString("00")), width, height, portrait); await action(state); }
            catch (Exception primary)
            {
                error = primary; Console.Error.WriteLine(primary);
                if (state is not null)
                {
                    state.Observe("primary-failure");
                    try { state.Save("failure"); }
                    catch (Exception captureError) { error = new AggregateException("节点验收首错与失败截图保存均失败", primary, captureError); Console.Error.WriteLine(captureError); }
                }
            }
            finally
            {
                if (state is not null) { try { await state.Cleanup(); cleaned = true; } catch (Exception cleanupError) { error = error is null ? cleanupError : new AggregateException(error, cleanupError); } }
                reports.Add(new { name, status = error is null ? "PASS" : "FAIL", cleaned, checks = state?.Checks.ToArray(), observations = state?.Observations.ToArray(), calls = state?.Backend.Calls(), error = error?.ToString() });
            }
            if (error is not null) { failures.Add(new InvalidOperationException(name, error)); if (!cleaned) ExceptionDispatchInfo.Capture(error).Throw(); }
        }
    }

    private sealed class NodeLayoutCase
    {
        internal required MainWindow Window;
        internal required WorkspaceViewModel Vm;
        internal required NodeLayoutBackend Backend;
        internal required string Directory;
        internal readonly List<string> Checks = [];
        internal readonly List<object> Observations = [];
        internal FrameworkElement Page => (FrameworkElement)Window.tabs.Items.Cast<TabItem>().Single(tab => Equals(tab.Tag, "节点")).Content;
        internal TextBox Properties => VisualDescendants<TextBox>(Page).Single(text => AutomationProperties.GetAutomationId(text) == nameof(WorkspaceViewModel.NodeProperties));
        internal Viewbox Picture => Ancestors(Window.nodeCanvas).OfType<Viewbox>().First();
        internal Grid Body => Ancestors(Picture).OfType<Grid>().First(grid => grid.IsAncestorOf(Window.nodeTree) && grid.IsAncestorOf(Properties));
        internal ScrollViewer TreeScroll => VisualDescendants<ScrollViewer>(Window.nodeTree).First(scroll => ReferenceEquals(scroll.TemplatedParent, Window.nodeTree));
        internal ScrollViewer PropertyScroll => VisualDescendants<ScrollViewer>(Properties).First(scroll => ReferenceEquals(scroll.TemplatedParent, Properties));
        internal Button[] Tools => new[] { Tool("刷新截图和节点"), Tool("导出 JSON"), Tool("展开"), Tool("折叠") };
        internal Button Tool(string text) => VisualDescendants<Button>(Page).Single(button => button.ToolTip as string == text || AutomationProperties.GetName(button) == text);
        internal static IEnumerable<DependencyObject> Ancestors(DependencyObject start) { for (var item = VisualTreeHelper.GetParent(start); item is not null; item = VisualTreeHelper.GetParent(item)) yield return item; }
        internal static object? FiniteRect(Rect value) => value.IsEmpty || !double.IsFinite(value.X) || !double.IsFinite(value.Y) || !double.IsFinite(value.Width) || !double.IsFinite(value.Height) ? null : new { value.X, value.Y, value.Width, value.Height };
        private static int? Id(object? value) => value is null ? null : System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(value);
        internal static async Task<NodeLayoutCase> Create(MainWindow owner, string directory, int width, int height, bool portrait)
        {
            System.IO.Directory.CreateDirectory(directory); var backend = new NodeLayoutBackend(portrait); MatrixRuntime? runtime = null; WorkspaceViewModel? vm = null; MainWindow? window = null; CatalogRepository? metadata = null; RollingLog? log = null;
            ScriptScheduler? scripts = null; InputCoordinator? input = null; ApiServiceManager? api = null;
            try
            {
                metadata = new CatalogRepository(Path.Combine(directory, "db.sqlite3")); log = new RollingLog(Path.Combine(directory, "logs")); var catalog = new DeviceCatalog(backend); var automation = new AutomationService(backend, catalog); scripts = new ScriptScheduler(backend); input = new InputCoordinator(catalog, backend); api = new ApiServiceManager(catalog, backend, automation, log);
                var settings = new AppSettings { AutoStartApi = false, AutoConnect = false, SyncMouse = false, SyncKeyboard = false, Language = "zh-CN", WindowWidth = width, WindowHeight = height };
                runtime = new MatrixRuntime { Backend = backend, Devices = catalog, Automation = automation, Scripts = scripts, Input = input, Api = api, Metadata = metadata, Log = log, Settings = settings, FrozenConnectionSettings = settings, SettingsStore = new SettingsStore(Path.Combine(directory, "AppConfig.json")), IsDemo = true }; api.DeviceSnapshotProvider = runtime.GetDevices;
                vm = new WorkspaceViewModel(runtime, directory, true); await vm.RefreshDevicesAsync().WaitAsync(TimeSpan.FromSeconds(5)); await vm.SetMainAsync(vm.Devices[0]).WaitAsync(TimeSpan.FromSeconds(5)); vm.CurrentDevice = vm.Devices[1]; vm.Devices[0].IsSelected = true; vm.UpdateSelection();
                window = new MainWindow(vm) { Owner = owner, ShowActivated = true }; var state = new NodeLayoutCase { Window = window, Vm = vm, Backend = backend, Directory = directory };
                state.Check(vm.Demo && runtime.IsDemo && ReferenceEquals(runtime.Backend, backend) && !api.BasicRunning && !api.AutomatorRunning && api.DevicePorts.Count == 0, "自有Demo一致runtime所有服务同计数wrapper，API未启");
                window.Show(); await state.Settle(); state.Observe("show-once", new { window.IsActive, focused = Keyboard.FocusedElement?.GetType().Name });
                state.Check(window.IsLoaded && new WindowInteropHelper(window).Handle != IntPtr.Zero && window.ActualWidth == width && window.ActualHeight == height, "真实HWND与请求尺寸一致非clamp");
                window.navigation["节点"].RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent)); await state.Settle(); backend.ClearCalls(); return state;
            }
            catch (Exception primary)
            {
                var errors = new List<Exception> { primary };
                async Task Attempt(Func<Task> action) { try { await action().WaitAsync(TimeSpan.FromSeconds(5)); } catch (Exception cleanupError) { errors.Add(cleanupError); } }
                try { window?.AbortStartup(); } catch (Exception closeError) { errors.Add(closeError); }
                if (vm is not null) await Attempt(vm.ShutdownAsync);
                else if (runtime is not null) await Attempt(() => runtime.DisposeAsync().AsTask());
                else
                {
                    if (api is not null) await Attempt(() => api.DisposeAsync().AsTask());
                    if (input is not null) await Attempt(() => input.DisposeAsync().AsTask());
                    if (scripts is not null) await Attempt(() => scripts.DisposeAsync().AsTask());
                    await Attempt(() => backend.DisposeAsync().AsTask());
                    try { metadata?.Dispose(); } catch (Exception cleanupError) { errors.Add(cleanupError); }
                    if (log is not null) await Attempt(() => log.DisposeAsync().AsTask());
                }
                if (errors.Count > 1) throw new AggregateException("节点夹具初始化与owned清理均失败", errors);
                throw;
            }
        }
        internal void Check(bool condition, string message) { if (!condition) { Observe("check-failed", new { message }); throw new InvalidOperationException(message); } Checks.Add(message); }
        internal void Observe(string stage, object? detail = null)
        {
            try { Observations.Add(new { stage, detail, Window.ActualWidth, Window.ActualHeight, body = FiniteRect(DeviceProbeBounds(Body, Window.Shell)), picture = FiniteRect(DeviceProbeBounds(Picture, Window.Shell)), tree = FiniteRect(DeviceProbeBounds(Window.nodeTree, Window.Shell)), properties = FiniteRect(DeviceProbeBounds(Properties, Window.Shell)), pageClip = FiniteRect(PageClip()), Vm.Busy, Vm.Error, Vm.Status, current = Vm.CurrentDevice?.Serial, main = Vm.MainDevice?.Serial, nodeCount = Vm.FlattenNodes().Count(), selectedId = Vm.SelectedNode?.Id, nodesId = Id(Vm.Nodes), imageId = Id(Vm.DebugImage), backendCalls = Backend.Calls().Length, keyboard = Keyboard.FocusedElement?.GetType().Name }); }
            catch (Exception diagnosticError) { Observations.Add(new { stage, diagnosticError = diagnosticError.ToString() }); Console.Error.WriteLine(diagnosticError); }
        }
        internal Rect PageClip() => Rect.Intersect(new Rect(Window.Shell.RenderSize), DeviceProbeBounds(Page, Window.Shell));
        internal async Task Settle() { await Window.Dispatcher.InvokeAsync(Window.UpdateLayout, DispatcherPriority.Render); await Window.Dispatcher.InvokeAsync(Window.UpdateLayout, DispatcherPriority.ContextIdle); await Window.Dispatcher.InvokeAsync(Window.UpdateLayout, DispatcherPriority.ApplicationIdle); }
        internal async Task Wait(Func<bool> condition, string message) { var watch = System.Diagnostics.Stopwatch.StartNew(); while (!condition()) { if (watch.Elapsed > TimeSpan.FromSeconds(5)) { Observe("wait-timeout"); throw new TimeoutException(message); } await Settle(); await Task.Delay(5); } }
        internal async Task Resize(double width, double height) { Window.Width = width; Window.Height = height; await Settle(); Check(Math.Abs(Window.ActualWidth - width) < .6 && Math.Abs(Window.ActualHeight - height) < .6, $"实际resize {width}×{height}"); Observe("resized"); LayoutGeometry(); }
        internal async Task Invoke(ButtonBase button)
        {
            Check(button.IsVisible && button.IsEnabled && DeviceProbeContains(PageClip(), DeviceProbeBounds(button, Window.Shell)), "实际可执行完整按钮：" + AutomationProperties.GetName(button));
            var peer = UIElementAutomationPeer.CreatePeerForElement(button) ?? throw new InvalidOperationException("按钮没有实际automation peer");
            if (peer.GetPattern(PatternInterface.Invoke) is IInvokeProvider invoke) invoke.Invoke(); else if (peer.GetPattern(PatternInterface.Toggle) is IToggleProvider toggle) toggle.Toggle(); else throw new InvalidOperationException("按钮没有Invoke/Toggle provider");
            await Window.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Input); await Settle();
        }
        internal async Task Refresh() { var count = Backend.Calls().Length; await Invoke(Tool("刷新截图和节点")); await Wait(() => !Vm.Busy && Backend.Calls().Skip(count).Any(call => call.Operation == "Dump") && Vm.Nodes.Count > 0, "实际刷新按钮未建立节点"); await Loaded(); }
        internal async Task Loaded() { await Wait(() => !Vm.Busy && Vm.Nodes.Count > 0, "节点请求未完成"); await Settle(); Check(Vm.Error.Length == 0 && Window.nodeTree.Items.Count == Vm.Nodes.Count && Window.nodeRectangles.Count > 0 && ReferenceEquals(Window.nodeCanvas.Children.OfType<Image>().Single().Source, Vm.DebugImage), "实际刷新完成建立原Canvas Image/Tree/矩形"); }
        internal void PictureGeometry()
        {
            var image = Window.nodeCanvas.Children.OfType<Image>().Single(); var rendered = DeviceProbeBounds(image, Window.Shell); var clip = Rect.Intersect(PageClip(), DeviceProbeBounds(Body, Window.Shell)); var slot = System.Windows.Controls.Primitives.LayoutInformation.GetLayoutSlot(Picture);
            Observe("picture-before-check", new { rendered = FiniteRect(rendered), clip = FiniteRect(clip), slot = FiniteRect(slot), Picture.IsVisible });
            Check(Picture.IsVisible && rendered.Width >= 100 && rendered.Height >= 100 && slot.Height >= 160 && DeviceProbeContains(clip, rendered), "实际图像至少100×100/图框高160且完整位于正文与祖先clip，先于新增分段存在性");
        }
        internal void LayoutGeometry(string? expectedMode = null)
        {
            var picture = DeviceProbeBounds(Picture, Window.Shell); var tree = DeviceProbeBounds(Window.nodeTree, Window.Shell); var properties = DeviceProbeBounds(Properties, Window.Shell); var clip = PageClip();
            static bool Separate(Rect first, Rect second) { var overlap = Rect.Intersect(first, second); return overlap.IsEmpty || overlap.Width <= .5 || overlap.Height <= .5; }
            Observe("layout-order", new { expectedMode, width = Body.ActualWidth, picture = FiniteRect(picture), tree = FiniteRect(tree), properties = FiniteRect(properties), Picture.IsVisible, treeVisible = Window.nodeTree.IsVisible, propertiesVisible = Properties.IsVisible });
            if (Body.ActualWidth >= 640)
            {
                Check(Picture.IsVisible && Window.nodeTree.IsVisible && Properties.IsVisible && tree.Width >= 240 && properties.Width >= 220
                    && DeviceProbeContains(clip, picture) && DeviceProbeContains(clip, tree) && DeviceProbeContains(clip, properties), "宽中屏三种原内容真实可见、树宽240/属性220且在PageClip内");
                if (Body.ActualWidth >= 920) Check(picture.Right <= tree.Left + .5 && tree.Right <= properties.Left + .5 && Separate(picture, tree) && Separate(tree, properties), "宽屏原画面/树/属性横向有序且不重叠");
                else Check(picture.Right <= Math.Min(tree.Left, properties.Left) + .5 && tree.Bottom <= properties.Top + .5 && Separate(picture, tree) && Separate(picture, properties) && Separate(tree, properties), "中屏原画面在左，树/属性在右上下有序且不重叠");
            }
            else
            {
                var visible = new[] { (Name: "画面", Element: (FrameworkElement)Picture), (Name: "节点", Element: (FrameworkElement)Window.nodeTree), (Name: "属性", Element: (FrameworkElement)Properties) }.Where(item => item.Element.IsVisible).ToArray();
                Check(visible.Length == 1 && (expectedMode is null || visible[0].Name == expectedMode), "窄屏只有本次选择的原正文可见");
                var active = visible[0].Element; var slot = System.Windows.Controls.Primitives.LayoutInformation.GetLayoutSlot(active); var actual = DeviceProbeBounds(active, Window.Shell);
                Check(slot.Width >= Body.ActualWidth - 24 && slot.Height >= Math.Max(100, Body.ActualHeight - 96) && DeviceProbeContains(clip, actual), "窄屏本次原正文使用完整可用区而非窄残栏");
            }
        }
        internal async Task Mode(string caption)
        {
            if (Body.ActualWidth >= 640) { Check(caption == "画面" ? Picture.IsVisible : caption == "节点" ? Window.nodeTree.IsVisible : Properties.IsVisible, "宽中屏原内容可见：" + caption); LayoutGeometry(); return; }
            var mode = VisualDescendants<ButtonBase>(Page).Single(button => AutomationProperties.GetName(button) == caption && button.IsVisible);
            var oldTreeFocus = Window.nodeTree.IsKeyboardFocusWithin; var oldPropertyFocus = Properties.IsKeyboardFocusWithin;
            DeviceProbe44(mode, Window.Shell, PageClip(), Check); await Invoke(mode);
            Check(caption == "画面" ? Picture.IsVisible : caption == "节点" ? Window.nodeTree.IsVisible : Properties.IsVisible, "实际窄屏分段：" + caption);
            LayoutGeometry(caption);
            if (oldTreeFocus && !Window.nodeTree.IsVisible || oldPropertyFocus && !Properties.IsVisible)
            {
                FrameworkElement active = caption == "画面" ? Picture : caption == "节点" ? Window.nodeTree : Properties;
                var focused = Keyboard.FocusedElement as FrameworkElement;
                Observe("mode-focus-transfer", new { caption, oldTreeFocus, oldPropertyFocus, modeFocus = mode.IsKeyboardFocusWithin, activeFocus = active.IsKeyboardFocusWithin, focusedVisible = focused?.IsVisible, focusedType = focused?.GetType().Name });
                Check((mode.IsKeyboardFocusWithin || active.IsKeyboardFocusWithin) && focused?.IsVisible == true && (Window.nodeTree.IsVisible || !Window.nodeTree.IsKeyboardFocusWithin) && (Properties.IsVisible || !Properties.IsKeyboardFocusWithin), "隐藏有焦点正文后，真实焦点落在本次可见分段或其正文");
            }
        }
        internal FrameworkElement HeaderHit(TreeViewItem item)
        {
            var presenter = VisualDescendants<ContentPresenter>(item).First(value => Equals(value.Content, item.Header) && Ancestors(value).OfType<TreeViewItem>().First() == item);
            FrameworkElement header = presenter;
            foreach (var candidate in Ancestors(presenter).TakeWhile(value => !ReferenceEquals(value, item)).OfType<FrameworkElement>()) if (candidate.ActualHeight is >= 43.9 and <= 80 && candidate.ActualWidth >= 44) { header = candidate; break; }
            var bounds = DeviceProbeBounds(header, Window.Shell); var visible = Rect.Intersect(bounds, DeviceProbeViewport(Window.Shell, TreeScroll));
            Observations.Add(new { stage = "tree-header-hit", node = (item.Tag as NodeRow)?.Id, bounds = FiniteRect(bounds), visible = FiniteRect(visible) });
            Check(!visible.IsEmpty && visible.Width >= 43.9 && visible.Height >= 43.9 && DeviceProbeContains(PageClip(), visible), "树头真实选择区可见至少44，不扩大设备像素框");
            foreach (var point in new[] { new Point(visible.X + visible.Width / 2, visible.Y + 2), new Point(visible.X + visible.Width / 2, visible.Bottom - 2) })
            {
                var hit = Window.Shell.InputHitTest(point) as DependencyObject; Check(hit is not null && (ReferenceEquals(item, hit) || item.IsAncestorOf(hit)), "树头可见选择区实际命中原TreeItem");
            }
            return header;
        }
        internal (NodeRow Node, double Y, double Horizontal, double Vertical) TreeIntent()
        {
            var clip = DeviceProbeViewport(Window.Shell, TreeScroll);
            var row = Window.nodeTreeItems.Where(pair => pair.Key.Children.Count == 0 && pair.Value.IsVisible).Select(pair => (pair.Key, Bounds: DeviceProbeBounds(pair.Value, Window.Shell)))
                .Where(pair => pair.Bounds.Top >= clip.Top - .5 && pair.Bounds.Bottom <= clip.Bottom + .5 && Rect.Intersect(pair.Bounds, clip).Width > 0).OrderBy(pair => pair.Bounds.Top).First();
            return (row.Key, row.Bounds.Top - clip.Top, TreeScroll.HorizontalOffset, TreeScroll.VerticalOffset);
        }
        internal void CheckTreeIntent((NodeRow Node, double Y, double Horizontal, double Vertical) expected)
        {
            var actual = TreeIntent(); Observe("tree-intent", new { expected = new { expected.Node.Id, expected.Y, expected.Horizontal, expected.Vertical }, actual = new { actual.Node.Id, actual.Y, actual.Horizontal, actual.Vertical }, TreeScroll.ScrollableHeight, TreeScroll.ViewportHeight });
            Check(ReferenceEquals(expected.Node, actual.Node) && Math.Abs(expected.Y - actual.Y) < 1 && Math.Abs(expected.Horizontal - actual.Horizontal) < 1 && Math.Abs(expected.Vertical - actual.Vertical) < 1, "恢复同尺寸后的树滚动意图/锚点不被隐藏重排清零");
        }
        internal void PropertyTail()
        {
            var leading = Properties.GetRectFromCharacterIndex(Properties.Text.Length - 1, false); var trailing = Properties.GetRectFromCharacterIndex(Properties.Text.Length - 1, true);
            var local = leading.IsEmpty || trailing.IsEmpty ? Rect.Empty : Rect.Union(leading, trailing); var tail = local.IsEmpty ? Rect.Empty : Properties.TransformToAncestor(Window.Shell).TransformBounds(local); var viewport = DeviceProbeViewport(Window.Shell, PropertyScroll);
            Observe("property-tail", new { tail = FiniteRect(tail), viewport = FiniteRect(viewport), Properties.Text.Length, PropertyScroll.VerticalOffset, PropertyScroll.ScrollableHeight });
            Check(!tail.IsEmpty && DeviceProbeContains(viewport, tail), "长属性末字符实际滚入文本视口，不只比字符串/DesiredSize");
        }
        internal async Task ReplacePropertyIntent(string signal)
        {
            var original = Vm.FlattenNodes().Single(node => node.Id == "layout.leaf.47");
            var item = Window.nodeTreeItems[original]; item.BringIntoView(); await Settle(); HeaderHit(item);
            ((ISelectionItemProvider)new TreeViewItemAutomationPeer(item).GetPattern(PatternInterface.SelectionItem)).Select(); await Settle();
            var text = Properties; var scroll = PropertyScroll; var binding = BindingOperations.GetBindingExpression(text, TextBox.TextProperty);
            Check(binding is not null && text.IsReadOnly && ReferenceEquals(Vm.SelectedNode, original), "新意图使用原只读属性绑定和实际选中节点：" + signal);
            var focusReturned = text.Focus(); await Settle();
            Check(Window.IsActive && text.IsKeyboardFocusWithin && text.IsVisible, "新意图自有属性正文单次Focus前置：" + signal);
            text.Select(text.Text.Length / 3, 8); await Settle();
            Check(scroll.ScrollableHeight > 100, "新意图原属性具有足够真实滚动范围：" + signal);
            scroll.ScrollToVerticalOffset(scroll.ScrollableHeight * .75); Window.UpdateLayout(); await Settle();
            var oldOffset = scroll.VerticalOffset; var oldMaximum = scroll.ScrollableHeight;
            Check(oldOffset > 2 && oldMaximum - oldOffset > 2, "旧意图为实际中段而非仅底部：" + signal);
            var nodes = Vm.Nodes.ToArray(); var source = Vm.DebugImage; var current = Vm.CurrentDevice; var main = Vm.MainDevice; var targets = Vm.Targets().ToArray();
            var mappings = Window.nodeTreeItems.ToDictionary(pair => pair.Key, pair => pair.Value); var rectangles = Window.nodeRectangles.ToDictionary(pair => pair.Key, pair => pair.Value); var calls = Backend.Calls().Length;
            var width = Window.ActualWidth; var height = Window.ActualHeight; var oldText = text.Text;
            var events = new List<object>(); var posts = new List<DispatcherOperation>(); var postedOverflow = false; var sizeChanges = 0;
            var wheelCount = 0; var downCount = 0; var keyCount = 0; var selectionCount = 0; var textCount = 0;
            double expectedOffset = double.NaN; var expectedStart = -1; var expectedLength = -1; var expectedCaret = -1; var expectedNode = original;
            DispatcherOperation? injection = null;
            bool Owns(DependencyObject? value) => value is not null && (ReferenceEquals(value, text) || text.IsAncestorOf(value));
            object EventProof(RoutedEventArgs args) => new { originalType = args.OriginalSource?.GetType().Name, originalName = (args.OriginalSource as FrameworkElement)?.Name, owned = Owns(args.OriginalSource as DependencyObject), args.Handled };
            object[] PostedProof() { lock (posts) return posts.Select(operation => (object)new { id = Id(operation), priority = operation.Priority.ToString(), status = operation.Status.ToString() }).ToArray(); }
            DispatcherHookEventHandler posted = (_, args) => { if (args.Operation.Priority != DispatcherPriority.ContextIdle) return; lock (posts) { if (posts.Count < 64) posts.Add(args.Operation); else postedOverflow = true; } };
            SizeChangedEventHandler sized = (_, _) => sizeChanges++;
            MouseWheelEventHandler wheel = (_, args) => { wheelCount++; events.Add(new { kind = "wheel", args.Delta, proof = EventProof(args) }); };
            MouseButtonEventHandler down = (_, args) => { downCount++; events.Add(new { kind = "preview-down", button = args.ChangedButton.ToString(), proof = EventProof(args) }); };
            KeyEventHandler key = (_, args) => { keyCount++; events.Add(new { kind = "preview-key", key = args.Key.ToString(), proof = EventProof(args) }); };
            RoutedEventHandler selected = (_, args) => { selectionCount++; events.Add(new { kind = "selection", text.SelectionStart, text.SelectionLength, text.CaretIndex, proof = EventProof(args) }); };
            TextChangedEventHandler changed = (_, args) => { textCount++; events.Add(new { kind = "text", length = text.Text.Length, boundValueMatches = text.Text == Vm.NodeProperties, proof = EventProof(args) }); };
            text.AddHandler(Mouse.PreviewMouseWheelEvent, wheel, true); text.AddHandler(Mouse.PreviewMouseDownEvent, down, true); text.AddHandler(Keyboard.PreviewKeyDownEvent, key, true);
            text.SelectionChanged += selected; text.TextChanged += changed; Window.SizeChanged += sized; Window.Dispatcher.Hooks.OperationPosted += posted;
            try
            {
                // Synchronous real layout leaves lower-priority idle work pending; no helper drains it first.
                Window.Width = 390; Window.Height = 1080; Window.UpdateLayout();
                var narrowOffset = scroll.VerticalOffset; var narrowMaximum = scroll.ScrollableHeight;
                Check(Math.Abs(Window.ActualWidth - 390) < .6 && Math.Abs(Window.ActualHeight - 1080) < .6 && text.IsVisible && Math.Abs(narrowOffset - oldOffset) > 2, "实际窄屏重排已改变中段offset，尚未排空idle：" + signal);
                Window.Width = width; Window.Height = height; Window.UpdateLayout();
                injection = Window.Dispatcher.InvokeAsync(() =>
                {
                    bool pending; lock (posts) pending = posts.Any(operation => operation.Status == DispatcherOperationStatus.Pending);
                    Observe("new-intent-before-signal", new { signal, focusReturned, oldOffset, oldMaximum, narrowOffset, narrowMaximum, currentOffset = scroll.VerticalOffset, currentMaximum = scroll.ScrollableHeight, sizeChanges, postedOverflow, pending, posts = PostedProof(), scope = "Hooks prove pending ContextIdle in this resize interval, not private restore-ticket identity; synthetic WPF route is not physical input" });
                    Check(!postedOverflow && pending && sizeChanges >= 2 && Math.Abs(Window.ActualWidth - width) < .6 && Math.Abs(Window.ActualHeight - height) < .6, "真实resize后Input阶段仍有有界待处理ContextIdle：" + signal);
                    Check(ReferenceEquals(text, Properties) && ReferenceEquals(scroll, PropertyScroll) && text.IsVisible && text.IsKeyboardFocusWithin, "注入前原属性/ScrollViewer仍可见持焦：" + signal);
                    var beforeWheel = wheelCount; var beforeDown = downCount; var beforeKey = keyCount; var beforeSelection = selectionCount; var beforeText = textCount;
                    if (signal is "wheel" or "thumb")
                    {
                        FrameworkElement target = text;
                        object? range = null;
                        Rect bounds;
                        if (signal == "thumb")
                        {
                            var bar = VisualDescendants<ScrollBar>(scroll).Single(value => value.Orientation == Orientation.Vertical && value.IsVisible);
                            target = VisualDescendants<Thumb>(bar).Single(value => value.IsVisible && value.ActualWidth > 0 && value.ActualHeight > 0);
                            range = new { bar.Minimum, bar.Maximum, bar.Value, bar.ViewportSize, bar.IsEnabled };
                            bounds = DeviceProbeBounds(target, Window.Shell);
                        }
                        else bounds = DeviceProbeViewport(Window.Shell, scroll);
                        Check(!bounds.IsEmpty && bounds.Width > 0 && bounds.Height > 0 && DeviceProbeContains(PageClip(), bounds), "新输入真实目标完整位于原属性页：" + signal);
                        var point = new Point(bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 2); var hit = Window.Shell.InputHitTest(point) as UIElement;
                        var targetOwnsHit = hit is not null && (ReferenceEquals(target, hit) || target.IsAncestorOf(hit));
                        Observe("new-intent-hit", new { signal, targetType = target.GetType().Name, range, bounds = FiniteRect(bounds), point.X, point.Y, hitType = hit?.GetType().Name, hitName = (hit as FrameworkElement)?.Name, targetOwnsHit, textOwnsHit = Owns(hit) });
                        Check(hit is not null && targetOwnsHit && Owns(hit), "实际Shell命中归原属性或原滚条Thumb：" + signal);
                        if (signal == "wheel") hit!.RaiseEvent(new MouseWheelEventArgs(Mouse.PrimaryDevice, Environment.TickCount, -120) { RoutedEvent = Mouse.PreviewMouseWheelEvent });
                        else hit!.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left) { RoutedEvent = Mouse.PreviewMouseDownEvent });
                        Check(signal == "wheel" ? wheelCount - beforeWheel == 1 : downCount - beforeDown == 1, "原属性实际收到唯一一次合成路由：" + signal);
                    }
                    else if (signal == "key")
                    {
                        var presentation = PresentationSource.FromVisual(text) ?? throw new InvalidOperationException("原属性没有实际PresentationSource");
                        text.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, presentation, Environment.TickCount, Key.PageDown) { RoutedEvent = Keyboard.PreviewKeyDownEvent });
                        Check(keyCount - beforeKey == 1, "原属性实际收到唯一一次PageDown预览路由");
                    }
                    else if (signal == "selection")
                    {
                        text.Select(text.Text.Length / 2, 13);
                        Check(selectionCount - beforeSelection == 1 && text.SelectionStart == text.Text.Length / 2 && text.SelectionLength == 13, "原属性实际接受一次新选区而非改旧快照");
                    }
                    else
                    {
                        expectedNode = Vm.FlattenNodes().Single(node => node.Id == "layout.leaf.46"); var next = Window.nodeTreeItems[expectedNode]; HeaderHit(next);
                        ((ISelectionItemProvider)new TreeViewItemAutomationPeer(next).GetPattern(PatternInterface.SelectionItem)).Select();
                        Check(ReferenceEquals(Vm.SelectedNode, expectedNode) && next.IsSelected, "真实原树UIA选择长46发布合法新属性文本");
                    }
                    Observe("new-intent-signal", new { signal, beforeWheel, wheelCount, beforeDown, downCount, beforeKey, keyCount, beforeSelection, selectionCount, beforeText, textCount, posts = PostedProof() });
                }, DispatcherPriority.Input);
                await injection.Task.WaitAsync(TimeSpan.FromSeconds(5));
                injection = Window.Dispatcher.InvokeAsync(() =>
                {
                    if (signal == "text") Check(textCount > 0 && text.Text != oldText && text.Text == Vm.NodeProperties && ReferenceEquals(Vm.SelectedNode, expectedNode), "原绑定实际TextChanged且新长文本已发布，不直接写只读TextBox");
                    Check(scroll.ScrollableHeight > 100, "新输入后的实际滚动范围足够量化中段：" + signal);
                    expectedOffset = scroll.ScrollableHeight * .27;
                    Check(expectedOffset > 2 && scroll.ScrollableHeight - expectedOffset > 2 && Math.Abs(expectedOffset - oldOffset) > 2, "新意图S1为可区别S0的合法中段：" + signal);
                    scroll.ScrollToVerticalOffset(expectedOffset); Window.UpdateLayout();
                    expectedStart = text.SelectionStart; expectedLength = text.SelectionLength; expectedCaret = text.CaretIndex;
                    Observe("new-intent-explicit-scroll", new { signal, expectedOffset, actualOffset = scroll.VerticalOffset, scroll.ScrollableHeight, expectedStart, expectedLength, expectedCaret, posts = PostedProof(), scope = "Explicit ScrollTo only quantifies the new intent after the single routed/model signal; not proof of physical scrolling" });
                }, DispatcherPriority.Input);
                await injection.Task.WaitAsync(TimeSpan.FromSeconds(5)); await Settle();
                var stable = ReferenceEquals(text, Properties) && ReferenceEquals(scroll, PropertyScroll) && ReferenceEquals(binding, BindingOperations.GetBindingExpression(text, TextBox.TextProperty)) && ReferenceEquals(source, Vm.DebugImage) && nodes.SequenceEqual(Vm.Nodes)
                    && mappings.Count == Window.nodeTreeItems.Count && mappings.All(pair => ReferenceEquals(Window.nodeTreeItems[pair.Key], pair.Value)) && rectangles.Count == Window.nodeRectangles.Count && rectangles.All(pair => ReferenceEquals(Window.nodeRectangles[pair.Key], pair.Value))
                    && ReferenceEquals(current, Vm.CurrentDevice) && ReferenceEquals(main, Vm.MainDevice) && targets.SequenceEqual(Vm.Targets()) && Backend.Calls().Length == calls;
                Observe("new-intent-final", new { signal, stable, oldOffset, expectedOffset, actualOffset = scroll.VerticalOffset, scroll.ScrollableHeight, text.SelectionStart, text.SelectionLength, text.CaretIndex, expectedStart, expectedLength, expectedCaret, selected = Vm.SelectedNode?.Id, textLength = text.Text.Length, wheelCount, downCount, keyCount, selectionCount, textCount, events = events.ToArray(), posts = PostedProof() });
                Check(stable && ReferenceEquals(Vm.SelectedNode, expectedNode) && text.Text == Vm.NodeProperties, "新意图保持原对象/绑定/源/上下文且仅允许新文本节点合法变化：" + signal);
                Check(Math.Abs(Window.ActualWidth - width) < .6 && Math.Abs(Window.ActualHeight - height) < .6 && double.IsFinite(expectedOffset) && Math.Abs(scroll.VerticalOffset - expectedOffset) < 1 && Math.Abs(scroll.VerticalOffset - oldOffset) > 2, "回原尺寸且idle排空后S1不被旧S0覆盖：" + signal);
                Check(text.SelectionStart == expectedStart && text.SelectionLength == expectedLength && text.CaretIndex == expectedCaret, "新选区和caret不被旧恢复覆盖：" + signal);
                Save("new-intent-" + signal);
            }
            finally
            {
                if (injection?.Status == DispatcherOperationStatus.Pending) injection.Abort();
                Window.Dispatcher.Hooks.OperationPosted -= posted; Window.SizeChanged -= sized;
                text.RemoveHandler(Mouse.PreviewMouseWheelEvent, wheel); text.RemoveHandler(Mouse.PreviewMouseDownEvent, down); text.RemoveHandler(Keyboard.PreviewKeyDownEvent, key);
                text.SelectionChanged -= selected; text.TextChanged -= changed;
                Observe("new-intent-handlers-detached", new { signal, wheelCount, downCount, keyCount, selectionCount, textCount, expectedOffset = double.IsFinite(expectedOffset) ? (double?)expectedOffset : null, events = events.ToArray(), posts = PostedProof() });
            }
        }
        internal void Save(string stage)
        {
            var path = Path.Combine(Directory, stage + ".png"); Window.SaveRender(path);
            var bytes = File.ReadAllBytes(path); Check(bytes.Length > 0, "必需节点布局截图已实际保存：" + stage);
            Observations.Add(new { stage = "saved-render", name = Path.GetFileName(path), bytes = bytes.Length, sha256 = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bytes)) });
        }
        internal async Task Cleanup() { Backend.ReleaseAll(); await Wait(() => !Vm.Busy, "自有节点任务未排空"); Check(Backend.GatesComplete, "全部自有节点TCS已释放/排空"); Window.AbortStartup(); await Vm.ShutdownAsync().WaitAsync(TimeSpan.FromSeconds(5)); }
    }

    private sealed record NodeLayoutSnapshot(Viewbox Picture, Canvas Canvas, TreeView Tree, TextBox Properties, Image Image, object? Source, NodeRow[] Nodes, NodeRow? Selected, string Text, int SelectionStart, int SelectionLength, int Caret, double PropertyVertical, double PropertyHorizontal,
        Dictionary<NodeRow, TreeViewItem> Items, Dictionary<NodeRow, Rectangle> Rectangles, Dictionary<NodeRow, bool> Expanded, DeviceRow? Current, DeviceRow? Main, DeviceRow[] Targets)
    {
        internal static NodeLayoutSnapshot Take(NodeLayoutCase state, (NodeRow Node, double Y, double Horizontal, double Vertical) treeIntent) => new(state.Picture, state.Window.nodeCanvas, state.Window.nodeTree, state.Properties, state.Window.nodeCanvas.Children.OfType<Image>().Single(), state.Vm.DebugImage,
            state.Vm.Nodes.ToArray(), state.Vm.SelectedNode, state.Properties.Text, state.Properties.SelectionStart, state.Properties.SelectionLength, state.Properties.CaretIndex, state.PropertyScroll.VerticalOffset, state.PropertyScroll.HorizontalOffset,
            state.Window.nodeTreeItems.ToDictionary(pair => pair.Key, pair => pair.Value), state.Window.nodeRectangles.ToDictionary(pair => pair.Key, pair => pair.Value), state.Window.nodeTreeItems.ToDictionary(pair => pair.Key, pair => pair.Value.IsExpanded), state.Vm.CurrentDevice, state.Vm.MainDevice, state.Vm.Targets().ToArray());
        internal void References(NodeLayoutCase state)
        {
            var controls = ReferenceEquals(Picture, state.Picture) && ReferenceEquals(Canvas, state.Window.nodeCanvas) && ReferenceEquals(Tree, state.Window.nodeTree) && ReferenceEquals(Properties, state.Properties) && ReferenceEquals(Image, state.Window.nodeCanvas.Children.OfType<Image>().Single());
            var data = ReferenceEquals(Source, state.Vm.DebugImage) && Nodes.SequenceEqual(state.Vm.Nodes) && ReferenceEquals(Selected, state.Vm.SelectedNode) && Text == state.Properties.Text;
            var mappings = Items.Count == state.Window.nodeTreeItems.Count && Items.All(pair => state.Window.nodeTreeItems.TryGetValue(pair.Key, out var actual) && ReferenceEquals(pair.Value, actual)) && Rectangles.Count == state.Window.nodeRectangles.Count && Rectangles.All(pair => state.Window.nodeRectangles.TryGetValue(pair.Key, out var actual) && ReferenceEquals(pair.Value, actual));
            var folds = Expanded.All(pair => state.Window.nodeTreeItems[pair.Key].IsExpanded == pair.Value); var context = ReferenceEquals(Current, state.Vm.CurrentDevice) && ReferenceEquals(Main, state.Vm.MainDevice) && Targets.SequenceEqual(state.Vm.Targets());
            state.Observe("reference-state", new { controls, data, mappings, folds, context }); state.Check(controls && data && mappings && folds && context, $"纯重排保留控件/数据/映射/fold/上下文 controls={controls};data={data};mappings={mappings};folds={folds};context={context}");
        }
        internal void PropertyState(NodeLayoutCase state)
        {
            var selection = SelectionStart == state.Properties.SelectionStart && SelectionLength == state.Properties.SelectionLength && Caret == state.Properties.CaretIndex;
            var expectedVertical = Math.Min(PropertyVertical, state.PropertyScroll.ScrollableHeight); var expectedHorizontal = Math.Min(PropertyHorizontal, state.PropertyScroll.ScrollableWidth);
            state.Observe("property-state", new { selection, SelectionStart, SelectionLength, Caret, actualStart = state.Properties.SelectionStart, actualLength = state.Properties.SelectionLength, actualCaret = state.Properties.CaretIndex, expectedVertical, expectedHorizontal, actualVertical = state.PropertyScroll.VerticalOffset, actualHorizontal = state.PropertyScroll.HorizontalOffset, state.PropertyScroll.ScrollableHeight, state.PropertyScroll.ViewportHeight });
            state.Check(selection && Math.Abs(expectedVertical - state.PropertyScroll.VerticalOffset) < 1 && Math.Abs(expectedHorizontal - state.PropertyScroll.HorizontalOffset) < 1, "同尺寸恢复保留文本选区/caret/合法clamp后的滚动意图");
        }
    }
    private sealed record NodeLayoutCall(int Sequence, string Operation, string? Serial);
    private sealed class NodeLayoutGate(Exception? error)
    {
        internal readonly Exception? Error = error;
        internal readonly TaskCompletionSource Entered = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal readonly TaskCompletionSource Release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal bool Claimed;
    }
    private sealed class NodeLayoutBackend(bool portrait) : IDeviceBackend
    {
        private readonly DemoDeviceBackend inner = new(); private readonly List<NodeLayoutCall> calls = []; private readonly List<NodeLayoutGate> gates = [];
        internal bool LongSecondLeaf;
        private int Width => portrait ? 360 : 720; private int Height => portrait ? 640 : 360;
        internal NodeLayoutCall[] Calls() { lock (calls) return calls.ToArray(); }
        internal void ClearCalls() { lock (calls) calls.Clear(); }
        private void Record(string operation, string? serial) { lock (calls) calls.Add(new(calls.Count + 1, operation, serial)); }
        internal NodeLayoutGate HoldShot(Exception? error = null) { var gate = new NodeLayoutGate(error); gates.Add(gate); return gate; }
        internal bool GatesComplete => gates.All(gate => gate.Entered.Task.IsCompleted && gate.Release.Task.IsCompleted);
        internal void ReleaseAll() { foreach (var gate in gates) { gate.Release.TrySetResult(); if (!gate.Claimed) gate.Entered.TrySetCanceled(); } }
        public async Task<IReadOnlyList<DeviceInfo>> DiscoverAsync(CancellationToken ct = default) { Record("Discover", null); return (await inner.DiscoverAsync(ct)).Select(info => info.Serial == "DEMO-002" ? info with { Width = Width, Height = Height } : info).ToArray(); }
        public async Task<byte[]> ScreenshotAsync(string serial, CancellationToken ct = default)
        {
            Record("Screenshot", serial); await inner.ScreenshotAsync(serial, ct); var payload = PngImage.Create(Width, Height, (x, y) => ((byte)(x % 251), (byte)(y % 251), (byte)112));
            var gate = gates.FirstOrDefault(value => !value.Claimed); if (gate is not null) { gate.Claimed = true; gate.Entered.TrySetResult(); await gate.Release.Task.WaitAsync(ct); if (gate.Error is not null) ExceptionDispatchInfo.Capture(gate.Error).Throw(); } return payload;
        }
        public async Task<AndroidNode> DumpAsync(string serial, CancellationToken ct = default)
        {
            Record("Dump", serial); await inner.DumpAsync(serial, ct);
            var branches = Enumerable.Range(0, 3).Select(group => new AndroidNode { Text = "分组" + group, ResourceId = "layout.group." + group, ClassName = "android.view.ViewGroup", Bounds = new(0, 0, Width, Height), ChildNodeList = Enumerable.Range(0, 16).Select(index =>
            {
                var id = group * 16 + index; var top = 8 + id * (Height - 24) / 48;
                return new AndroidNode { Text = id == 47 ? "长属性" + new string('X', 2400) : LongSecondLeaf && id == 46 ? "另一长属性" + new string('Z', 2600) : "节点" + id, ContentDesc = "节点说明" + new string('Y', 200), ResourceId = "layout.leaf." + id, ClassName = "android.widget.Button", Clickable = true, Enabled = true, Bounds = new(20, top, Width - 20, top + 8) };
            }).ToList() }).ToList();
            return new AndroidNode { Text = "自有布局fixture，非真实应用页面", ResourceId = "layout.root", ClassName = "android.view.ViewGroup", Bounds = new(0, 0, Width, Height), ChildNodeList = branches };
        }
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
        public Task PullAsync(string serial, string remotePath, string localPath, IProgress<double>? progress = null, CancellationToken ct = default) { Record("Pull", serial); return inner.PullAsync(serial, remotePath, localPath, progress, ct); }
        public Task SetNodeTextAsync(string serial, NodeSelector selector, string text, CancellationToken ct = default) { Record("NodeText", serial); return inner.SetNodeTextAsync(serial, selector, text, ct); }
        public ValueTask DisposeAsync() { ReleaseAll(); return inner.DisposeAsync(); }
    }
}
