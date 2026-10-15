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
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using MatrixDevice.Core;
using MatrixDevice.Host;
using Path = System.IO.Path;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    internal async Task RunMonitorLayoutAcceptanceAsync(string[] args)
    {
        if (!ViewModel.Demo || !Path.GetFullPath(ViewModel.DataDirectory).Contains(Path.DirectorySeparatorChar + "artifacts" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("监控正文验收仅限artifacts内隔离Demo工作区。");
        string? Argument(string flag) { var index = Array.IndexOf(args, flag); return index < 0 ? null : args[index + 1]; }
        var width = double.Parse(Argument("--width") ?? "1440", CultureInfo.InvariantCulture); var height = double.Parse(Argument("--height") ?? "900", CultureInfo.InvariantCulture);
        var screenshot = Argument("--screenshot"); var phase = "initial"; var checks = new List<string>(); var observations = new List<object>(); Exception? failure = null; var actionInvocations = 0;
        void Check(bool condition, string message) { if (!condition) throw new InvalidOperationException(phase + ": " + message); checks.Add(message); }
        async Task Settle() { await Dispatcher.InvokeAsync(UpdateLayout, DispatcherPriority.Render); await Dispatcher.InvokeAsync(UpdateLayout, DispatcherPriority.ContextIdle); await Dispatcher.InvokeAsync(UpdateLayout, DispatcherPriority.ApplicationIdle); }
        void Save(string suffix)
        {
            if (screenshot is null) return;
            var path = Path.Combine(Path.GetDirectoryName(screenshot)!, Path.GetFileNameWithoutExtension(screenshot) + "-" + suffix + ".png"); Directory.CreateDirectory(Path.GetDirectoryName(path)!); SaveRender(path);
        }
        var originalTag = (tabs.SelectedItem as TabItem)?.Tag?.ToString() ?? "设备墙 / 主控";
        var originalTimerEnabled = monitorTimer.IsEnabled; var originalPaused = monitorPaused; var originalStopping = monitorStopping; var originalCompact = monitorCompact; var originalConnectedOnly = monitorConnectedOnly;
        var originalSelected = monitorSelected; var originalNotice = monitorNotice; var originalRuntime = monitorRuntime; var originalSampler = monitorResources;
        var originalTiles = monitorTiles.ToArray(); var originalChildren = monitorWall.Children.Cast<UIElement>().ToArray();
        var originalCurrent = ViewModel.CurrentDevice; var originalMain = ViewModel.MainDevice; var originalNodes = ViewModel.Nodes.ToArray(); var originalSelectedNode = ViewModel.SelectedNode;
        var originalSelection = ViewModel.Devices.ToDictionary(row => row, row => row.IsSelected);
        var originalHostText = monitorHost.Text; var originalSummaryText = monitorSummary.Text; var originalDetailText = monitorDetail.Text;
        var originalScopeIndex = monitorScopeChoice.SelectedIndex; var releases = new List<TaskCompletionSource>(); Window? focusWindow = null;
        var focusEvents = new Queue<object>(); var focusEventCount = 0; bool? focusActivateResult = null; Window? observedFocusWindow = null;
        void RecordFocus(string reason)
        {
            var currentTag = (tabs.SelectedItem as TabItem)?.Tag?.ToString(); var focused = Keyboard.FocusedElement;
            focusEventCount++; if (focusEvents.Count == 128) focusEvents.Dequeue();
            focusEvents.Enqueue(new
            {
                sequence = focusEventCount, phase, reason, observedAt = DateTimeOffset.UtcNow, focusActivateResult,
                focusWindowObserved = observedFocusWindow is not null, focusWindowActive = observedFocusWindow?.IsActive, ownerActive = IsActive, ownerInactive = !IsActive, monitorActive = MonitorActive,
                notClosing = !closing, notStopping = !monitorStopping, notPaused = !monitorPaused, monitorPage = currentTag == "实时监控", closing, monitorStopping, monitorPaused, currentTag,
                inputGeneration = ViewModel.Runtime.Input.CurrentGeneration, reconcileStatus = monitorReconcile.Status.ToString(), samplingStatus = monitorSampling.Status.ToString(), keyboardFocus = focused is null ? null : focused.GetType().FullName + "#" + System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(focused) + (focused is FrameworkElement element ? "/" + element.Name + "/" + AutomationProperties.GetName(element) : "")
            });
        }
        EventHandler ownerActivatedObserver = (_, _) => RecordFocus("owner-activated");
        EventHandler ownerDeactivatedObserver = (_, _) => RecordFocus("owner-deactivated");
        EventHandler focusActivatedObserver = (_, _) => RecordFocus("focus-window-activated");
        EventHandler focusDeactivatedObserver = (_, _) => RecordFocus("focus-window-deactivated");
        void DetachFocusObservers()
        {
            Activated -= ownerActivatedObserver; Deactivated -= ownerDeactivatedObserver;
            if (observedFocusWindow is not { } observed) return;
            observed.Activated -= focusActivatedObserver; observed.Deactivated -= focusDeactivatedObserver; observedFocusWindow = null;
        }
        var fixtures = new List<MonitorTile>();
        try
        {
            phase = "window"; await StopMonitorAsync(); monitorStopping = true; monitorTimer.Stop(); NavigateTo("实时监控"); monitorTimer.Stop(); await Settle(); Activate();
            Check(IsLoaded && IsVisible && new WindowInteropHelper(this).Handle != IntPtr.Zero && Math.Abs(ActualWidth - width) < .5 && Math.Abs(ActualHeight - height) < .5, "实际HWND和请求六尺寸一致");
            monitorTiles.Clear(); monitorWall.Children.Clear(); monitorRuntime = ViewModel.Runtime; monitorCompact = false;
            for (var index = 0; index < 16; index++)
            {
                var key = "layout:" + index;
                var tile = new MonitorTile(key, () => SelectMonitor(key), () => { actionInvocations++; return Task.CompletedTask; }, () => { actionInvocations++; return Task.CompletedTask; }) { Eligible = true, Signature = "geometry-signature:" + index };
                tile.Name.Text = "离线几何卡片 " + index; tile.Subtitle.Text = "合成像素与资源夹具，非设备";
                fixtures.Add(tile); monitorTiles.Add(key, tile); monitorWall.Children.Add(tile.Surface);
                tile.ApplyLayout(false, monitorScroll.ViewportWidth);
            }
            await Settle();
            phase = "visible-viewport";
            var first = fixtures[0]; var presenter = DeviceProbePresenter(monitorScroll); var contentViewport = new Rect(presenter.RenderSize);
            var rightInScroll = presenter.TranslatePoint(new Point(presenter.ActualWidth, 0), monitorScroll).X; var scrollbarStrip = monitorScroll.ActualWidth - rightInScroll;
            Check(monitorScroll.ScrollableHeight > 0 && scrollbarStrip > 2, "16卡形成真实滚动条和内容视口外条带");
            var originalTransform = first.Surface.RenderTransform;
            try
            {
                async Task VisibilityCase(string name, double x, double y, bool expected)
                {
                    first.Surface.RenderTransform = originalTransform; await Settle(); var originalBounds = DeviceProbeBounds(first.Surface, presenter);
                    first.Surface.RenderTransform = new TranslateTransform(x - originalBounds.Left, y - originalBounds.Top); await Settle();
                    var bounds = DeviceProbeBounds(first.Surface, presenter); var intersection = Rect.Intersect(bounds, new Rect(presenter.RenderSize)); var actual = first.VisibleIn(monitorScroll);
                    observations.Add(new { phase, name, expected, actual, bounds, contentViewport = new Rect(presenter.RenderSize), intersection = intersection.IsEmpty ? (Rect?)null : intersection, scrollbarStrip, geometryTransformFixture = true });
                    Check(actual == expected, $"真实SCP可见性{name}: expected={expected}; actual={actual}; bounds={bounds}; viewport={new Rect(presenter.RenderSize)}; intersection={intersection}");
                }
                await VisibilityCase("scrollbar-only", contentViewport.Width + 1, 10, false);
                await VisibilityCase("partial-one-pixel", contentViewport.Width - 1, 10, true);
                await VisibilityCase("touch-right-zero-area", contentViewport.Width, 10, false);
                await VisibilityCase("fully-outside", contentViewport.Width + scrollbarStrip + 10, 10, false);
                await VisibilityCase("touch-bottom-zero-area", 10, contentViewport.Height, false);
                await VisibilityCase("partial-bottom-one-pixel", 10, contentViewport.Height - 1, true);
            }
            finally { first.Surface.RenderTransform = originalTransform; await Settle(); }
            monitorScroll.ScrollToBottom(); await Settle();
            Check(!first.VisibleIn(monitorScroll) && fixtures[^1].VisibleIn(monitorScroll), "真实ScrollBottom使首卡离屏并显示末卡");
            monitorScroll.ScrollToTop(); await Settle(); Check(first.VisibleIn(monitorScroll), "真实ScrollTop恢复首卡可见");
            Save("visibility");
            phase = "control-context";
            var a = ViewModel.Devices[0]; var b = ViewModel.Devices[1]; await ViewModel.SetMainAsync(a); ViewModel.CurrentDevice = b; b.IsSelected = true; ViewModel.UpdateSelection(); await ViewModel.LoadNodesAsync();
            ViewModel.SelectedNode = ViewModel.FlattenNodes().FirstOrDefault();
            var controlTargets = ViewModel.Targets().ToArray(); var nodes = ViewModel.Nodes.ToArray(); var selectedNode = ViewModel.SelectedNode; var inputGeneration = ViewModel.Runtime.Input.CurrentGeneration; var deviceFilter = deviceStatusFilter;
            int? ReferenceId(object? value) => value is null ? null : System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(value);
            string? InputIdentity(IInputElement? value) => value is null ? null : value.GetType().FullName + "#" + ReferenceId(value) + (value is FrameworkElement element ? "/" + element.Name + "/" + AutomationProperties.GetName(element) : "");
            var baselineActive = IsActive; var baselineFocus = InputIdentity(Keyboard.FocusedElement); var baselineCapture = InputIdentity(Mouse.Captured);
            void ContextUnchanged(string reason, bool checkGeneration = true)
            {
                var actualMain = ViewModel.MainDevice; var actualCurrent = ViewModel.CurrentDevice; var actualTargets = ViewModel.Targets().ToArray(); var actualNodes = ViewModel.Nodes.ToArray(); var actualSelectedNode = ViewModel.SelectedNode; var actualGeneration = ViewModel.Runtime.Input.CurrentGeneration; var actualFilter = deviceStatusFilter;
                var sameMain = ReferenceEquals(actualMain, a); var sameCurrent = ReferenceEquals(actualCurrent, b); var sameTargets = controlTargets.SequenceEqual(actualTargets); var sameNodes = nodes.SequenceEqual(actualNodes); var sameSelectedNode = ReferenceEquals(selectedNode, actualSelectedNode); var sameGeneration = inputGeneration == actualGeneration; var sameFilter = deviceFilter == actualFilter;
                observations.Add(new
                {
                    phase = phase + "-context", reason, checkGeneration,
                    before = new { mainId = ReferenceId(a), mainSerial = a.Serial, currentId = ReferenceId(b), currentSerial = b.Serial, targets = controlTargets.Select(row => new { id = ReferenceId(row), row.Serial }).ToArray(), nodeIds = nodes.Select(node => ReferenceId(node)).ToArray(), selectedNodeId = ReferenceId(selectedNode), inputGeneration, deviceFilter, active = baselineActive, focus = baselineFocus, capture = baselineCapture },
                    after = new { mainId = ReferenceId(actualMain), mainSerial = actualMain?.Serial, currentId = ReferenceId(actualCurrent), currentSerial = actualCurrent?.Serial, targets = actualTargets.Select(row => new { id = ReferenceId(row), row.Serial }).ToArray(), nodeIds = actualNodes.Select(node => ReferenceId(node)).ToArray(), selectedNodeId = ReferenceId(actualSelectedNode), inputGeneration = actualGeneration, deviceFilter = actualFilter, active = IsActive, focus = InputIdentity(Keyboard.FocusedElement), capture = InputIdentity(Mouse.Captured) },
                    sameMain, sameCurrent, sameTargets, sameNodes, sameSelectedNode, sameGeneration, sameFilter
                });
                Check(sameMain && sameCurrent && sameTargets && sameNodes && sameSelectedNode && (!checkGeneration || sameGeneration) && sameFilter, "监看不改变MainA/CurrentB/目标/节点/设备筛选" + (checkGeneration ? "/输入代次" : "，失焦释放输入代次单独观察") + ": " + reason + $"; sameMain={sameMain}; sameCurrent={sameCurrent}; sameTargets={sameTargets}; sameNodes={sameNodes}; sameSelectedNode={sameSelectedNode}; sameFilter={sameFilter} ({deviceFilter}->{actualFilter}); sameGeneration={sameGeneration} ({inputGeneration}->{actualGeneration}); checkGeneration={checkGeneration}; active={baselineActive}->{IsActive}; focus={baselineFocus}->{InputIdentity(Keyboard.FocusedElement)}; capture={baselineCapture}->{InputIdentity(Mouse.Captured)}");
            }
            async Task Click(ButtonBase button)
            {
                DeviceProbe44(button, Shell, new Rect(Shell.RenderSize), Check);
                button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent)); monitorTimer.Stop(); await Settle();
            }
            async Task Scope(int index)
            {
                DeviceProbe44(monitorScopeChoice, Shell, new Rect(Shell.RenderSize), Check);
                var expand = (IExpandCollapseProvider)new ComboBoxAutomationPeer(monitorScopeChoice).GetPattern(PatternInterface.ExpandCollapse); expand.Expand(); await Settle();
                try
                {
                    var item = monitorScopeChoice.ItemContainerGenerator.ContainerFromIndex(index) as ComboBoxItem ?? throw new InvalidOperationException("真实监控范围选项未生成");
                    Check(item.IsVisible, "监控范围Popup中的选项实际可见");
                    var ownerPeer = UIElementAutomationPeer.CreatePeerForElement(monitorScopeChoice) as ComboBoxAutomationPeer ?? new ComboBoxAutomationPeer(monitorScopeChoice);
                    var selection = ownerPeer.GetChildren()?.Where(peer => peer.GetName() == item.Content?.ToString()).Select(peer => peer.GetPattern(PatternInterface.SelectionItem)).OfType<ISelectionItemProvider>().Single()
                        ?? throw new InvalidOperationException("实际监控范围缺少SelectionItem provider");
                    selection.Select(); monitorTimer.Stop(); await Settle();
                    Check(monitorScopeChoice.SelectedIndex == index && monitorConnectedOnly == (index == 0), "真实范围选择改变对应独立监控状态");
                }
                finally { expand.Collapse(); await Settle(); }
            }
            async Task Pause(bool paused)
            {
                DeviceProbe44(monitorPauseChoice, Shell, new Rect(Shell.RenderSize), Check);
                if (monitorPauseChoice.IsChecked != paused) ((IToggleProvider)new ToggleButtonAutomationPeer(monitorPauseChoice).GetPattern(PatternInterface.Toggle)).Toggle();
                monitorTimer.Stop(); await Settle(); Check(monitorPaused == paused && monitorPauseChoice.IsChecked == paused && AutomationProperties.GetName(monitorPauseChoice) == (paused ? "恢复监控" : "暂停监控"), "真实暂停Toggle状态与中文动作一致");
            }
            async Task ScrollField(FrameworkElement element)
            {
                element.BringIntoView(); await Settle(); var viewport = DeviceProbeViewport(monitorScroll, monitorScroll); var bounds = DeviceProbeBounds(element, monitorScroll);
                observations.Add(new { phase, element = element.GetType().Name, automation = AutomationProperties.GetName(element), bounds, viewport });
                Check(DeviceProbeContains(viewport, bounds), "卡片字段/动作可以完整滚入真实SCP: " + element.GetType().Name);
            }
            async Task CardGeometry(MonitorTile tile, bool compact)
            {
                tile.Surface.BringIntoView(); await Settle(); var bounds = DeviceProbeBounds(tile.Surface, presenter);
                Check(bounds.Left - tile.Surface.Margin.Left >= -.5 && bounds.Right + tile.Surface.Margin.Right <= presenter.ActualWidth + .5, "卡片及水平margin完整位于实际SCP: " + tile.Key);
                foreach (var button in VisualDescendants<Button>(tile.Surface)) { await ScrollField(button); DeviceProbe44(button, monitorScroll, DeviceProbeViewport(monitorScroll, monitorScroll), Check); Check(AutomationProperties.GetName(button) is "打开模拟器窗口" or "重试视频", "卡片动作中文名保持"); }
                foreach (var field in new FrameworkElement[] { tile.Name, tile.Subtitle, tile.State, tile.Resource, MonitorProbeField<Canvas>(tile, "chart") }) await ScrollField(field);
                var videoSurface = MonitorProbeField<Border>(tile, "videoSurface");
                if (compact) Check(videoSurface.Visibility == Visibility.Collapsed, "资源模式按原规则折叠视频容器: " + tile.Key);
                else { await ScrollField(videoSurface); Check(videoSurface.IsVisible && videoSurface.ActualHeight >= 120, "画面模式保留空或有帧视频容器: " + tile.Key); }
            }
            async Task ReadTail(TextBlock text, string expected, double maximum, string name)
            {
                var scroll = MonitorProbeAncestor<ScrollViewer>(text); scroll.ScrollToTop(); await Settle(); var top = scroll.VerticalOffset;
                Check(text.Text == expected && scroll.ActualHeight <= maximum + .5 && scroll.ScrollableHeight > 0 && DeviceProbeContains(new Rect(Shell.RenderSize), DeviceProbeBounds(scroll, Shell)), name + "原文完整且有界滚动容器可见");
                scroll.ScrollToBottom(); await Settle(); var tail = text.ContentEnd.GetCharacterRect(LogicalDirection.Backward); var tailInScroll = text.TransformToAncestor(scroll).TransformBounds(tail); var clip = DeviceProbeViewport(scroll, scroll);
                observations.Add(new { phase, name, textLength = expected.Length, top, scroll.VerticalOffset, scroll.ScrollableHeight, tailInScroll = tailInScroll.IsEmpty ? (Rect?)null : tailInScroll, clip, fullTextInViewNotTooltip = true });
                Check(scroll.VerticalOffset > top && Math.Abs(scroll.VerticalOffset - scroll.ScrollableHeight) < .5 && tailInScroll.Height > 0 && tailInScroll.Top >= clip.Top - 1 && tailInScroll.Bottom <= clip.Bottom + 1, name + "实际滚到全文末尾，不以ToolTip替代");
            }
            phase = "header-and-cards";
            foreach (var control in new Control[] { monitorWallChoice, monitorListChoice, monitorScopeChoice, monitorPauseChoice }) DeviceProbe44(control, Shell, new Rect(Shell.RenderSize), Check);
            var headers = new FrameworkElement[] { monitorWallChoice, monitorListChoice, monitorScopeChoice, monitorPauseChoice }.Select(control => DeviceProbeBounds(control, Shell)).ToArray();
            Check(headers.SelectMany((bounds, index) => headers.Skip(index + 1).Select(other => Rect.Intersect(bounds, other))).All(overlap => overlap.IsEmpty || overlap.Width <= .5 || overlap.Height <= .5), "四个头部控件真实bounds无重叠");
            var clock = DateTimeOffset.UtcNow;
            for (var index = 0; index < fixtures.Count; index++)
            {
                var tile = fixtures[index]; var epoch = tile.BeginVideo(); if (index != 2) { tile.AcceptFrame(epoch, new(32, 18, MonitorProbePixels(index), 1, clock)); tile.Present(clock, true); }
                tile.Target = new(1000 + index, 1, 101, 201, 102, 202);
            }
            phase = "single-card-scrollbar-threshold";
            var savedChildren = monitorWall.Children.Cast<UIElement>().ToArray(); var savedScrollHeight = monitorScroll.Height; var singleSnapshot = MonitorProbeSnapshot.Take(first);
            var transitions = new List<object>(); var scrollEvents = 0; Exception? thresholdFailure = null;
            ScrollChangedEventHandler countScroll = (_, change) =>
            {
                if (!ReferenceEquals(change.OriginalSource, monitorScroll)) return;
                scrollEvents++;
                if (transitions.Count < 64) transitions.Add(new { scrollEvents, monitorScroll.ComputedVerticalScrollBarVisibility, monitorScroll.ViewportWidth, monitorScroll.ViewportHeight, monitorScroll.ExtentHeight, cardWidth = first.Surface.ActualWidth, cardHeight = first.Surface.ActualHeight, viewportWidthChange = change.ViewportWidthChange, extentHeightChange = change.ExtentHeightChange });
            };
            observations.Add(new { phase, transitions, scope = "Bounded single existing tile/frame Auto-scrollbar threshold observation; temporary 390x844 and explicit scroll height; not a live video test" });
            monitorScroll.ScrollChanged += countScroll;
            try
            {
                monitorWall.Children.Clear(); monitorWall.Children.Add(first.Surface); Width = 390; Height = 844; await Settle().WaitAsync(TimeSpan.FromSeconds(3));
                foreach (var threshold in new[] { 315d, 316d, 317d, 318d, 319d, 320d, 323d, 324d, 325d })
                {
                    var beforeEvents = scrollEvents; monitorScroll.Height = threshold;
                    var states = new List<(Visibility Bar, double Width, double Height, double CardWidth, double CardHeight)>();
                    for (var sample = 0; sample < 3; sample++)
                    {
                        await Settle().WaitAsync(TimeSpan.FromSeconds(3)); await Task.Delay(30);
                        states.Add((monitorScroll.ComputedVerticalScrollBarVisibility, monitorScroll.ViewportWidth, monitorScroll.ViewportHeight, first.Surface.ActualWidth, first.Surface.ActualHeight));
                    }
                    var previous = states[^2]; var current = states[^1];
                    observations.Add(new { phase, requestedScrollHeight = threshold, actualScrollHeight = monitorScroll.ActualHeight, events = scrollEvents - beforeEvents, states = states.Select(state => new { state.Bar, state.Width, state.Height, state.CardWidth, state.CardHeight }).ToArray(), temporaryDimensions = new { width = ActualWidth, height = ActualHeight } });
                    Check(scrollEvents - beforeEvents <= 24 && previous.Bar == current.Bar && Math.Abs(previous.Width - current.Width) <= .1 && Math.Abs(previous.Height - current.Height) <= .1 && Math.Abs(previous.CardWidth - current.CardWidth) <= .1 && Math.Abs(previous.CardHeight - current.CardHeight) <= .1, "单卡Auto滚动条阈值在固定观察窗口内收敛: " + threshold);
                    singleSnapshot.CheckUnchanged(first, Check);
                }
            }
            catch (Exception error) { thresholdFailure = error; }
            finally
            {
                monitorScroll.ScrollChanged -= countScroll;
                try { monitorScroll.Height = savedScrollHeight; monitorWall.Children.Clear(); foreach (var child in savedChildren) monitorWall.Children.Add(child); Width = width; Height = height; await Settle().WaitAsync(TimeSpan.FromSeconds(3)); }
                catch (Exception restoreError) { thresholdFailure = thresholdFailure is null ? restoreError : new AggregateException("单卡阈值观察与恢复均失败", thresholdFailure, restoreError); }
            }
            if (thresholdFailure is not null) ExceptionDispatchInfo.Capture(thresholdFailure).Throw();
            Check(Math.Abs(ActualWidth - width) < .5 && Math.Abs(ActualHeight - height) < .5 && monitorWall.Children.Count == fixtures.Count && monitorWall.Children.Cast<UIElement>().SequenceEqual(savedChildren), "恢复原请求尺寸/Auto高度/同卡片后继续canonical验收");
            phase = "resource-and-card-geometry";
            first.Name.Text = "长名称-" + new string('测', 90) + "-NAME-END"; first.Subtitle.Text = "实例1000 · " + new string('S', 160) + "-SERIAL-END"; first.Fail(string.Concat(Enumerable.Repeat("故障原文测试 ", 60)) + "-FAILURE-END");
            for (var index = 0; index < 6; index++) first.SetResource(new(1000, 1, clock.AddSeconds(index), index == 2 ? null : index * 7, index == 3 ? null : (128 + index * 16) * 1048576L, index == 2 ? "synthetic unknown" : null));
            fixtures[3].SetResource(new(1003, 1, clock, 0, 0, null));
            fixtures[4].SetResource(new(1004, 1, clock, 12, 100, null)); fixtures[4].SetResource(new(1004, 1, clock.AddSeconds(1), null, null, "synthetic denied")); fixtures[4].SetResource(new(1004, 1, clock.AddSeconds(2), 33, 300, null));
            fixtures[5].SetResource(new(1005, 1, clock, null, null, "synthetic resource error"));
            var capped = new List<MonitorResourceSample>();
            for (var index = 0; index < 500; index++) { var sample = new MonitorResourceSample(1006, 1, clock.AddMilliseconds(index * 100), index % 100, 100 + index, null); capped.Add(sample); fixtures[6].SetResource(sample); }
            var expired = new MonitorResourceSample(1007, 1, clock.AddSeconds(-901), 1, 1, null); var recent = new MonitorResourceSample(1007, 1, clock, 2, 2, null); fixtures[7].SetResource(expired); fixtures[7].SetResource(recent);
            await Settle();
            Check(MonitorProbeField<List<MonitorResourceSample>>(fixtures[2], "history").Count == 0 && fixtures[2].LatestResource is null && fixtures[2].ValidResourceSamples == 0, "零资源样本不制造统计");
            Check(MonitorProbeField<List<MonitorResourceSample>>(fixtures[3], "history").Count == 1 && fixtures[3].ValidResourceSamples == 0 && fixtures[3].Resource.Text.Contains("0.0%", StringComparison.Ordinal), "单样本保留有效0值，工作集0不计有效次数");
            Check(MonitorProbeField<List<MonitorResourceSample>>(fixtures[6], "history").SequenceEqual(capped.Skip(49)) && fixtures[6].ValidResourceSamples == 500, "451上限只裁历史，Valid保持原累计语义");
            Check(MonitorProbeField<List<MonitorResourceSample>>(fixtures[7], "history").Count == 1 && ReferenceEquals(fixtures[7].LatestResource, recent) && fixtures[7].ValidResourceSamples == 2, "超过15分钟样本移除，Latest和累计次数不伪造");
            foreach (var tile in fixtures)
            {
                await CardGeometry(tile, false);
                Check((ReferenceEquals(tile, fixtures[2]) ? tile.Frame.Source is null : tile.Frame.Source is BitmapSource) && tile.Session is null, "分别覆盖默认空画面/合成像素，明确无真实Session: " + tile.Key); MonitorProbeChart(tile, Check, observations, phase);
            }
            phase = "actual-card-selection";
            monitorScroll.ScrollToTop(); await Settle(); await Pause(true); monitorStopping = false; monitorNotice = "fixture previous notice";
            await ScrollField(first.Name); var nameCenter = first.Name.TranslatePoint(new Point(first.Name.ActualWidth / 2, first.Name.ActualHeight / 2), Shell); var actualHit = Shell.InputHitTest(nameCenter) as DependencyObject;
            var ancestry = new List<DependencyObject>();
            for (var current = actualHit; current is not null; current = current is Visual ? VisualTreeHelper.GetParent(current) : LogicalTreeHelper.GetParent(current)) ancestry.Add(current);
            Check(ancestry.Contains(first.Name) && ancestry.Contains(first.Surface) && ancestry.All(item => item is not System.Windows.Controls.Button), "Name真实中心由Shell实际命中到同Name/Surface且无Button");
            var surfaceEvents = 0; object? eventSource = null; MouseButton? eventButton = null; var eventClicks = 0;
            MouseButtonEventHandler selectedObserver = (_, input) => { surfaceEvents++; eventSource = input.OriginalSource; eventButton = input.ChangedButton; eventClicks = input.ClickCount; observations.Add(new { phase, surfaceEvents, originalSourceType = input.OriginalSource?.GetType().FullName, originalName = ReferenceEquals(input.OriginalSource, first.Name), input.ChangedButton, input.ClickCount, monitorSelected, monitorNotice, detail = monitorDetail.Text, syntheticWpfBottomMouseRoute = true }); };
            first.Surface.AddHandler(UIElement.MouseLeftButtonDownEvent, selectedObserver, true);
            try
            {
                first.Name.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left) { RoutedEvent = Mouse.MouseDownEvent });
                await monitorReconcile; monitorTimer.Stop(); monitorStopping = true; await Settle();
            }
            finally { first.Surface.RemoveHandler(UIElement.MouseLeftButtonDownEvent, selectedObserver); }
            Check(surfaceEvents == 1 && ReferenceEquals(eventSource, first.Name) && eventButton == MouseButton.Left && eventClicks == 1, "底层MouseDown经WPF真实推广一次Surface LeftDown，未伪造Source或调用SelectMonitor");
            Check(string.IsNullOrEmpty(monitorNotice) && first.Surface.BorderBrush is SolidColorBrush selectedBrush && selectedBrush.Color == ((SolidColorBrush)ThemeBrush("AccentBrush")).Color && fixtures.Skip(1).All(tile => tile.Surface.BorderBrush is SolidColorBrush brush && brush.Color == ((SolidColorBrush)ThemeBrush("BorderBrush")).Color), "真实选卡清notice并只高亮对应边框");
            Check(monitorSelected == first.Key && monitorDetail.Text == first.Detail && monitorDetail.Text.Contains("NAME-END", StringComparison.Ordinal) && monitorDetail.Text.Contains("SERIAL-END", StringComparison.Ordinal) && monitorDetail.Text.Contains("FAILURE-END", StringComparison.Ordinal), "真实选卡路由将长名称/串号/失败原值送到详情"); ContextUnchanged("实际选卡");
            monitorHost.Text = string.Concat(Enumerable.Repeat("主机资源布局长文 ", 60)) + "HOST-END"; monitorSummary.Text = string.Concat(Enumerable.Repeat("监控状态布局长文 ", 60)) + "SUMMARY-END"; await Settle();
            await ReadTail(monitorHost, monitorHost.Text, 48, "主机信息合成布局文本"); await ReadTail(monitorSummary, monitorSummary.Text, 48, "摘要合成布局文本"); await ReadTail(monitorDetail, first.Detail, 72, "实际选卡详情");
            Check(DeviceProbePresenter(monitorScroll).ActualHeight >= 100, "长文本极端下仍保留至少100高监控内容视口"); Save("long-content");
            phase = "pure-resize-pending-frame";
            var baseline = fixtures.ToDictionary(tile => tile.Key, MonitorProbeSnapshot.Take); var sampler = monitorResources;
            var videoEpoch = MonitorProbeField<long>(first, "videoEpoch"); first.AcceptFrame(videoEpoch, new(32, 18, MonitorProbePixels(99), 2, clock.AddSeconds(1))); var pending = MonitorProbeField<object>(first, "pendingFrame");
            Width = width + 19; await Settle();
            foreach (var tile in fixtures) baseline[tile.Key].CheckUnchanged(tile, Check, checkPending: false);
            Check(ReferenceEquals(MonitorProbeField<object>(first, "pendingFrame"), pending) && ReferenceEquals(sampler, monitorResources), "同nonce待呈现F2经真实resize不丢，不重置sampler");
            first.Present(clock.AddSeconds(1), true); Check(first.Received == 2 && first.Presented == baseline[first.Key].Presented + 1 && ReferenceEquals(first.Frame.Source, baseline[first.Key].Source), "resize后同Bitmap原引用可发布同nonce F2，非Session连续性证明");
            Width = width; await Settle(); ContextUnchanged("画面墙resize"); Save("wall-projection");
            phase = "resource-mode-resize"; await Click(monitorListChoice); Check(monitorCompact && monitorListChoice.IsChecked == true && monitorWallChoice.IsChecked == false, "实际资源列表模式入口");
            baseline = fixtures.ToDictionary(tile => tile.Key, MonitorProbeSnapshot.Take); var chartBefore = MonitorProbeField<Canvas>(first, "chart").ActualWidth;
            Width = width + 73; await Settle();
            Check(Math.Abs(MonitorProbeField<Canvas>(first, "chart").ActualWidth - chartBefore) > 1, "真实资源模式resize确实改变图表ActualWidth，非330墙上限的伪宽度测试");
            foreach (var tile in fixtures) { baseline[tile.Key].CheckUnchanged(tile, Check); await CardGeometry(tile, true); MonitorProbeChart(tile, Check, observations, phase); }
            Width = width; await Settle(); ContextUnchanged("资源模式resize"); Save("resource-projection");
            await Scope(1); await Scope(0); await Click(monitorWallChoice); Check(!monitorCompact && monitorWallChoice.IsChecked == true && monitorListChoice.IsChecked == false, "真实画面墙模式入口");
            await Pause(false); monitorTimer.Stop(); await Pause(true); ContextUnchanged("范围与两模式及暂停");
            phase = "demo-coordinator";
            monitorTiles.Clear(); monitorWall.Children.Clear(); monitorSelected = null; monitorNotice = ""; monitorRuntime = ViewModel.Runtime;
            var samplingHold = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously); releases.Add(samplingHold); monitorSampling = samplingHold.Task;
            monitorStopping = false; await Pause(false); monitorTimer.Stop(); await monitorReconcile;
            await Scope(1); await monitorReconcile; monitorTimer.Stop(); Check(monitorTiles.Keys.ToHashSet().SetEquals(unifiedDevices.Select(row => row.Key)), "真实Demo协调器全部目录不读设备筛选");
            await Scope(0); await monitorReconcile; monitorTimer.Stop(); Check(monitorTiles.Keys.ToHashSet().SetEquals(unifiedDevices.Where(row => row.Connected).Select(row => row.Key)), "真实Demo协调器已连接范围");
            ContextUnchanged("Demo范围协调");
            phase = "demo-empty-directory";
            var savedDirectory = unifiedDevices; var savedRows = unifiedDevices.ToArray(); var savedRowSelection = savedRows.ToDictionary(row => row, row => row.IsSelected);
            var savedDeviceSelection = ViewModel.Devices.ToDictionary(row => row, row => row.IsSelected); var savedUpdating = updatingUnifiedDirectory; var savedEmptyScope = monitorScopeChoice.SelectedIndex;
            Exception? emptyFailure = null;
            try
            {
                Check(savedRows.Length > 0 && monitorTiles.Count > 0, "空目录测试从实际非空Demo目录与监控卡开始");
                var selectedDemoTile = monitorTiles.Values.First(); await ScrollField(selectedDemoTile.Name);
                var demoNameCenter = selectedDemoTile.Name.TranslatePoint(new Point(selectedDemoTile.Name.ActualWidth / 2, selectedDemoTile.Name.ActualHeight / 2), Shell);
                var demoNameHit = Shell.InputHitTest(demoNameCenter) as DependencyObject; var demoAncestry = new List<DependencyObject>();
                for (var current = demoNameHit; current is not null; current = current is Visual ? VisualTreeHelper.GetParent(current) : LogicalTreeHelper.GetParent(current)) demoAncestry.Add(current);
                Check(demoAncestry.Contains(selectedDemoTile.Name) && demoAncestry.Contains(selectedDemoTile.Surface) && demoAncestry.All(item => item is not System.Windows.Controls.Button), "实际非空Demo卡Name中心命中可选择区域");
                selectedDemoTile.Name.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left) { RoutedEvent = Mouse.MouseDownEvent });
                await monitorReconcile; monitorTimer.Stop(); await Settle();
                Check(monitorSelected == selectedDemoTile.Key && monitorDetail.Text == selectedDemoTile.Detail, "真实WPF卡片路由先建立非空详情，空态不靠直接赋文案");
                updatingUnifiedDirectory = true; unifiedDevices.Clear();
                foreach (var scopeIndex in new[] { 1, 0 })
                {
                    await Scope(scopeIndex); monitorLastReconcile = default; MonitorTick(); await monitorReconcile.WaitAsync(TimeSpan.FromSeconds(3)); monitorTimer.Stop(); MonitorTick(); await Settle();
                    observations.Add(new { phase, scopeIndex, directoryCount = unifiedDevices.Count, tileCount = monitorTiles.Count, childCount = monitorWall.Children.Count, monitorSelected, summary = monitorSummary.Text, detail = monitorDetail.Text, sameDirectory = ReferenceEquals(savedDirectory, unifiedDevices), syntheticEmptyDirectory = true });
                    Check(ReferenceEquals(savedDirectory, unifiedDevices) && unifiedDevices.Count == 0 && monitorTiles.Count == 0 && monitorWall.Children.Count == 0, "真实Tick/Reconcile空目录两范围均无卡片或残留子项: " + scopeIndex);
                    Check(monitorSelected is null && monitorSummary.Text == "可见 0 台 · 视频 0 路" && monitorDetail.Text == "未选中监看设备" && monitorSummary.IsVisible && monitorDetail.IsVisible, "空目录显示既有中文摘要并清除已移除卡片详情: " + scopeIndex);
                    ContextUnchanged("空目录范围 " + scopeIndex);
                    Check(savedRows.All(row => row.IsSelected == savedRowSelection[row]) && savedDeviceSelection.All(pair => pair.Key.IsSelected == pair.Value), "临时空目录不改变原行或设备群控选择: " + scopeIndex);
                    Save(scopeIndex == 1 ? "empty-directory-all" : "empty-directory-connected");
                }
            }
            catch (Exception error) { emptyFailure = error; Console.Error.WriteLine(error); }
            finally
            {
                try
                {
                    updatingUnifiedDirectory = true; unifiedDevices.Clear(); foreach (var row in savedRows) unifiedDevices.Add(row);
                    foreach (var pair in savedRowSelection) if (pair.Key.IsSelected != pair.Value) pair.Key.IsSelected = pair.Value;
                    foreach (var pair in savedDeviceSelection) if (pair.Key.IsSelected != pair.Value) pair.Key.IsSelected = pair.Value;
                    updatingUnifiedDirectory = false; UpdateUnifiedDirectory();
                    await Scope(savedEmptyScope); monitorLastReconcile = default; MonitorTick(); await monitorReconcile.WaitAsync(TimeSpan.FromSeconds(3)); monitorTimer.Stop(); MonitorTick(); await Settle();
                    Check(ReferenceEquals(savedDirectory, unifiedDevices) && savedRows.Length == unifiedDevices.Count && savedRows.Select((row, index) => ReferenceEquals(row, unifiedDevices[index])).All(same => same), "空目录finally恢复同集合及全部原行引用和顺序");
                    Check(savedRows.All(row => row.IsSelected == savedRowSelection[row]) && savedDeviceSelection.All(pair => pair.Key.IsSelected == pair.Value), "空目录finally恢复原行与设备选择");
                    Check(monitorTiles.Keys.ToHashSet().SetEquals(savedRows.Where(row => !monitorConnectedOnly || row.Connected).Select(row => row.Key)) && monitorWall.Children.Count == monitorTiles.Count && monitorTiles.Values.All(tile => monitorWall.Children.Contains(tile.Surface)), "恢复原目录后实际Tick/Reconcile重新生成对应监控卡和可视子项");
                    ContextUnchanged("空目录恢复实际协调");
                }
                catch (Exception restoreError) { emptyFailure = emptyFailure is null ? restoreError : new AggregateException("空目录验收与恢复均失败", emptyFailure, restoreError); }
                finally { updatingUnifiedDirectory = savedUpdating; }
            }
            Check(updatingUnifiedDirectory == savedUpdating, "空目录finally恢复原目录更新guard");
            if (emptyFailure is not null) ExceptionDispatchInfo.Capture(emptyFailure).Throw();
            phase = "demo-coordinator";
            focusWindow = new Window { Owner = this, Title = "Isolated monitor focus fixture", Width = 240, Height = 140, ShowInTaskbar = false, WindowStartupLocation = WindowStartupLocation.CenterOwner, Content = new TextBlock { Text = "Demo focus fixture" } };
            var beforeFocusGeneration = ViewModel.Runtime.Input.CurrentGeneration;
            observedFocusWindow = focusWindow; Activated += ownerActivatedObserver; Deactivated += ownerDeactivatedObserver; focusWindow.Activated += focusActivatedObserver; focusWindow.Deactivated += focusDeactivatedObserver;
            RecordFocus("before-show"); focusWindow.Show(); RecordFocus("after-show"); focusActivateResult = focusWindow.Activate(); RecordFocus("after-activate"); await Settle(); RecordFocus("after-settle");
            Check(focusWindow.IsActive && !IsActive && MonitorActive, $"自有窗口失焦不改变既有MonitorActive策略; focusWindowActive={focusWindow.IsActive}; ownerInactive={!IsActive}; monitorActive={MonitorActive}; ActivateResult={focusActivateResult}; notClosing={!closing}; notStopping={!monitorStopping}; notPaused={!monitorPaused}; monitorPage={Equals((tabs.SelectedItem as TabItem)?.Tag, "实时监控")}; currentTag={(tabs.SelectedItem as TabItem)?.Tag}; inputGeneration={beforeFocusGeneration}->{ViewModel.Runtime.Input.CurrentGeneration}");
            ContextUnchanged("自有窗口失焦", checkGeneration: false); observations.Add(new { phase = "focus-policy", beforeFocusGeneration, afterFocusGeneration = ViewModel.Runtime.Input.CurrentGeneration, monitorActive = MonitorActive, ownerActive = IsActive, existingOwnerDeactivatedReleasePolicy = true });
            DetachFocusObservers(); focusWindow.Close(); focusWindow = null; Activate(); await Settle(); inputGeneration = ViewModel.Runtime.Input.CurrentGeneration;
            var stopHold = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously); releases.Add(stopHold);
            var subject = monitorTiles.Values.First(); subject.StopCompletion = stopHold.Task;
            Check(ReferenceEquals(subject.StopAsync(), stopHold.Task) && ReferenceEquals(subject.StopAsync(), stopHold.Task) && !stopHold.Task.IsCompleted, "重复停止共享同一未完成清理任务，Session为null不是decoder释放证明");
            monitorPauseChoice.IsChecked = true; monitorTimer.Stop(); Check(monitorPaused && !monitorReconcile.IsCompleted, "真实暂停协调路径等待共享StopCompletion");
            stopHold.TrySetResult(); await monitorReconcile.WaitAsync(TimeSpan.FromSeconds(3)); monitorTimer.Stop(); Check(monitorTiles.Values.All(tile => tile.Session is null && tile.StopCompletion.IsCompleted), "暂停等待自有清理屏障完成，无会话或任务残留");
            await Pause(false); await monitorReconcile; monitorTimer.Stop(); ContextUnchanged("暂停恢复协调");
            NavigateTo("设备墙 / 主控"); monitorTimer.Stop(); await monitorReconcile; Check(!MonitorActive && monitorTiles.Values.All(tile => tile.Session is null), "实际离页使Demo协调器停止，无真实视频清理声明");
            samplingHold.TrySetResult(); await monitorSampling; monitorSampling = Task.CompletedTask;
            phase = "complete"; Check(actionInvocations == 0, "Open/Retry纯几何回调从未Invoke"); Save("final");
        }
        catch (Exception ex) { failure = ex; Console.Error.WriteLine(ex); }
        finally
        {
            DetachFocusObservers(); observations.Add(new { phase = "focus-event-timeline", eventCount = focusEventCount, retainedCount = focusEvents.Count, droppedCount = focusEventCount - focusEvents.Count, maximum = 128, events = focusEvents.ToArray(), observersRemovedBeforeWindowClose = true });
            foreach (var release in releases) release.TrySetResult();
            try
            {
                if (focusWindow is not null) focusWindow.Close();
                monitorStopping = true; monitorTimer.Stop(); await monitorReconcile; await monitorSampling;
                await Task.WhenAll(fixtures.Select(tile => tile.StopAsync()));
                Check(releases.All(release => release.Task.IsCompleted) && monitorReconcile.IsCompleted && monitorSampling.IsCompleted && fixtures.All(tile => tile.Session is null && tile.StopCompletion.IsCompleted), "finally已释放自有TCS并等待全部协调/采样/fixture清理任务");
                monitorTiles.Clear(); monitorWall.Children.Clear(); foreach (var pair in originalTiles) monitorTiles.Add(pair.Key, pair.Value); foreach (var child in originalChildren) monitorWall.Children.Add(child);
                monitorCompact = originalCompact; monitorConnectedOnly = originalConnectedOnly; monitorScopeChoice.SelectedIndex = originalScopeIndex;
                monitorPauseChoice.IsChecked = originalPaused; monitorPaused = originalPaused; monitorSelected = originalSelected; monitorNotice = originalNotice; monitorRuntime = originalRuntime; monitorResources = originalSampler;
                if (originalMain is not null) await ViewModel.SetMainAsync(originalMain); ViewModel.CurrentDevice = originalCurrent;
                foreach (var pair in originalSelection) pair.Key.IsSelected = pair.Value; ViewModel.UpdateSelection();
                ViewModel.Nodes.Clear(); foreach (var node in originalNodes) ViewModel.Nodes.Add(node); ViewModel.SelectedNode = originalSelectedNode; RebuildNodes();
                monitorHost.Text = originalHostText; monitorSummary.Text = originalSummaryText; monitorDetail.Text = originalDetailText;
                Width = width; Height = height; NavigateTo(originalTag); monitorTimer.Stop(); monitorStopping = originalStopping; if (originalTimerEnabled) monitorTimer.Start(); await Settle();
            }
            catch (Exception cleanupError) { failure = failure is null ? cleanupError : new AggregateException("监控验收与恢复均失败", failure, cleanupError); }
            try { monitorTimer.Stop(); monitorStopping = originalStopping; if (originalTimerEnabled) monitorTimer.Start(); }
            catch (Exception timerError) { failure = failure is null ? timerError : new AggregateException("监控验收与定时器恢复均失败", failure, timerError); }
            try { if (screenshot is not null) { Directory.CreateDirectory(Path.GetDirectoryName(screenshot)!); SaveRender(screenshot); } }
            catch (Exception screenshotError) { failure = failure is null ? screenshotError : new AggregateException("监控验收与最终截图均失败", failure, screenshotError); }
            try { File.WriteAllText(Path.Combine(ViewModel.DataDirectory, "monitor-layout.json"), JsonSerializer.Serialize(new { status = failure is null ? "PASS-OFFLINE-WPF" : "FAIL", scope = "Isolated Demo WPF geometry/routed controls and explicit synthetic projection; not physical click, Android, ADB, real decoder/video or DPI acceptance", width, height, actualWidth = ActualWidth, actualHeight = ActualHeight, phase, checks, observations, actionInvocations, error = failure?.ToString() }, JsonDefaults.Options)); }
            catch (Exception reportError) { Console.Error.WriteLine(reportError); failure = failure is null ? reportError : new AggregateException("监控验收与报告保存均失败", failure, reportError); }
        }
        if (failure is not null) ExceptionDispatchInfo.Capture(failure).Throw();
    }

    private static T MonitorProbeField<T>(MonitorTile tile, string name)
    {
        var field = typeof(MonitorTile).GetField(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("已核监控私有字段不存在: " + name);
        return field.GetValue(tile) is T value ? value : throw new InvalidOperationException("监控字段类型不符或为空: " + name + "; expected=" + typeof(T).FullName);
    }
    private static T MonitorProbeAncestor<T>(DependencyObject child) where T : DependencyObject
    {
        for (var current = VisualTreeHelper.GetParent(child); current is not null; current = VisualTreeHelper.GetParent(current)) if (current is T found) return found;
        throw new InvalidOperationException("缺少实际监控祖先: " + typeof(T).FullName);
    }
    private static byte[] MonitorProbePixels(int seed)
    {
        var bytes = new byte[32 * 18 * 4];
        for (var offset = 0; offset < bytes.Length; offset += 4) { bytes[offset] = (byte)((offset / 4 + seed) % 256); bytes[offset + 1] = (byte)((seed * 19) % 256); bytes[offset + 2] = (byte)((offset / 128 * 17) % 256); bytes[offset + 3] = 255; }
        return bytes;
    }
    private static void MonitorProbeChart(MonitorTile tile, Action<bool, string> check, List<object> observations, string phase)
    {
        var history = MonitorProbeField<List<MonitorResourceSample>>(tile, "history"); var chart = MonitorProbeField<Canvas>(tile, "chart");
        var cpu = MonitorProbeField<Polyline>(tile, "cpuLine"); var memory = MonitorProbeField<Polyline>(tile, "memoryLine");
        var cpuStart = history.FindLastIndex(sample => sample.CpuPercent is null) + 1; var memoryStart = history.FindLastIndex(sample => sample.WorkingSetBytes is null) + 1;
        var memoryMax = history.Count == 0 ? 1 : Math.Max(1, history.Max(sample => sample.WorkingSetBytes ?? 0)); var expectedCpu = new List<Point>(); var expectedMemory = new List<Point>();
        for (var index = 0; index < history.Count; index++)
        {
            var x = index * chart.ActualWidth / Math.Max(1, history.Count - 1);
            if (index >= cpuStart && history[index].CpuPercent is { } value) expectedCpu.Add(new(x, 23 - Math.Clamp(value, 0, 100) * .22));
            if (index >= memoryStart && history[index].WorkingSetBytes is { } bytes) expectedMemory.Add(new(x, 23 - bytes * 22d / memoryMax));
        }
        bool Same(PointCollection actual, List<Point> expected) => actual.Count == expected.Count && actual.Select((point, index) => (point - expected[index]).Length <= .01).All(equal => equal);
        observations.Add(new { phase = phase + "-chart", tile.Key, width = chart.ActualWidth, height = chart.ActualHeight, samples = history.Count, cpuStart, memoryStart, cpuPoints = cpu.Points.ToArray(), memoryPoints = memory.Points.ToArray(), valid = tile.ValidResourceSamples, latest = tile.LatestResource, fullStrokePixelsClaimed = false });
        check(chart.ActualWidth > 0 && double.IsFinite(chart.ActualWidth) && Same(cpu.Points, expectedCpu) && Same(memory.Points, expectedMemory), "ActualWidth投影及最后unknown后段语义正确: " + tile.Key);
        check(cpu.Points.Concat(memory.Points).All(point => point.X >= -.01 && point.X <= chart.ActualWidth + .01 && point.Y >= -.01 && point.Y <= 24.01), "图表点不越有限画布，非完整stroke像素证明: " + tile.Key);
    }
    private sealed record MonitorProbeSnapshot(Border Surface, Image Frame, ImageSource? Source, MonitorResourceSample[] History, MonitorResourceSample? Latest, int Valid, string Signature, MonitorProcessTarget? Target, bool Eligible, ScreenVideoSession? Session, Task StopCompletion, long Epoch, long Received, long Presented, DateTimeOffset? LastFrameAt, object? Pending, string ResourceText)
    {
        internal static MonitorProbeSnapshot Take(MonitorTile tile)
        {
            var pending = typeof(MonitorTile).GetField("pendingFrame", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic) ?? throw new InvalidOperationException("缺少pendingFrame字段");
            return new(tile.Surface, tile.Frame, tile.Frame.Source, MonitorProbeField<List<MonitorResourceSample>>(tile, "history").ToArray(), tile.LatestResource, tile.ValidResourceSamples, tile.Signature, tile.Target, tile.Eligible, tile.Session, tile.StopCompletion, MonitorProbeField<long>(tile, "videoEpoch"), tile.Received, tile.Presented, tile.LastFrameAt, pending.GetValue(tile), tile.Resource.Text);
        }
        internal void CheckUnchanged(MonitorTile tile, Action<bool, string> check, bool checkPending = true)
        {
            var after = Take(tile);
            check(ReferenceEquals(Surface, after.Surface) && ReferenceEquals(Frame, after.Frame) && ReferenceEquals(Source, after.Source) && ReferenceEquals(Latest, after.Latest) && ReferenceEquals(Target, after.Target) && ReferenceEquals(Session, after.Session) && ReferenceEquals(StopCompletion, after.StopCompletion), "纯布局保留同tile/frame/source/Latest/Target/Session/Stop引用: " + tile.Key);
            check(History.Length == after.History.Length && History.Select((sample, index) => ReferenceEquals(sample, after.History[index])).All(same => same) && Valid == after.Valid && Signature == after.Signature && Eligible == after.Eligible && Epoch == after.Epoch && Received == after.Received && Presented == after.Presented && LastFrameAt == after.LastFrameAt && ResourceText == after.ResourceText && (!checkPending || ReferenceEquals(Pending, after.Pending)), "纯布局不造历史/次数/ObservedAt/nonce/帧/资源值: " + tile.Key);
        }
    }
}
