using System.IO;
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
using System.Windows.Threading;
using MatrixDevice.Core;
using MatrixDevice.Features;
using MatrixDevice.Host;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    internal async Task RunResponsiveShellAcceptanceAsync(string[] args)
    {
        if (!ViewModel.Demo) throw new InvalidOperationException("共享壳层验收仅限演示工作区。");
        string? Argument(string flag) { var index = Array.IndexOf(args, flag); return index < 0 ? null : args[index + 1]; }
        double Requested(string flag, double fallback) => Argument(flag) is string value ? double.Parse(value, System.Globalization.CultureInfo.InvariantCulture) : fallback;
        var requestedWidth = Requested("--width", 1440); var requestedHeight = Requested("--height", 900);
        var screenshot = Argument("--screenshot"); var checks = new List<string>(); var geometries = new List<object>(); var breakpoints = new List<object>(); var fieldStates = new List<object>();
        void Check(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); checks.Add(message); }
        async Task Settle()
        {
            await Dispatcher.InvokeAsync(UpdateLayout, DispatcherPriority.ContextIdle);
            await Dispatcher.InvokeAsync(UpdateLayout, DispatcherPriority.Render);
            await Dispatcher.InvokeAsync(UpdateLayout, DispatcherPriority.ApplicationIdle);
        }
        async Task Resize(double width, double height) { Width = width; Height = height; await Settle(); Check(ActualWidth == width && ActualHeight == height, $"实际外窗 {width}×{height}"); }
        static Rect In(FrameworkElement element, FrameworkElement surface) => element.TransformToAncestor(surface).TransformBounds(new Rect(new Point(), element.RenderSize));
        static bool VisibleInside(FrameworkElement element, FrameworkElement surface)
        {
            var bounds = In(element, surface);
            return element.IsVisible && bounds.Width > 0 && bounds.Height > 0 && bounds.Left >= -1 && bounds.Top >= -1 && bounds.Right <= surface.ActualWidth + 1 && bounds.Bottom <= surface.ActualHeight + 1;
        }
        void Click(ButtonBase button) { Check(button.IsEnabled && button.IsVisible, "可点击：" + AutomationProperties.GetName(button)); button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent)); }
        void Escape(UIElement source)
        {
            var presentation = PresentationSource.FromVisual(source) ?? throw new InvalidOperationException("键盘测试对象没有显示源。");
            source.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, presentation, Environment.TickCount, Key.Escape) { RoutedEvent = Keyboard.PreviewKeyDownEvent });
        }
        void Capture(string suffix)
        {
            if (screenshot is null) return;
            var path = Path.Combine(Path.GetDirectoryName(screenshot)!, Path.GetFileNameWithoutExtension(screenshot) + suffix + ".png");
            SaveRender(path);
        }

        await Settle(); Activate();
        Check(IsLoaded && IsVisible && PresentationSource.FromVisual(this) is not null && new WindowInteropHelper(this).Handle != IntPtr.Zero, "已显示真实 WPF/HWND 窗口");
        Check(ActualWidth == requestedWidth && ActualHeight == requestedHeight, $"原始请求 {requestedWidth}×{requestedHeight} 与实际外窗一致");
        foreach (var (width, height) in new[] { (390, 640), (390, 844), (768, 1024) })
        {
            var applied = new SettingsEditor(new AppSettings()) { WindowWidth = width, WindowHeight = height }.ToSettings();
            var storePath = Path.Combine(ViewModel.DataDirectory, $"geometry-{width}-{height}", "AppConfig.json");
            var store = new SettingsStore(storePath); store.Load(); store.Save(applied);
            var loaded = new SettingsStore(storePath).Load();
            Check(loaded.WindowWidth == width && loaded.WindowHeight == height, $"设置 Apply/Save/新 Store Load 保留 {width}×{height}");
        }
        foreach (var bad in new[] { double.NaN, double.PositiveInfinity, 389d, 7681d })
        {
            var rejected = false; try { _ = new SettingsEditor(new AppSettings()) { WindowWidth = bad }.ToSettings(); } catch (InvalidOperationException) { rejected = true; }
            Check(rejected, "设置拒绝非法宽度：" + bad);
        }
        var main = ViewModel.MainDevice; var current = ViewModel.CurrentDevice; var selected = ViewModel.Devices.Select(row => (row.Serial, row.IsSelected)).ToArray();
        var originalShell = ViewModel.ShellInput; var originalText = ViewModel.TextInput; var originalPage = ((TabItem)tabs.SelectedItem).Tag!.ToString()!;
        var originalPreference = sidebarPreference;
        var nav = navigation.ToDictionary(pair => pair.Key, pair => pair.Value);
        var expectedTags = new[] { "设备墙 / 主控", "实时监控", "节点", "截图 / 取色", "Shell", "应用", "文件", "文本", "脚本", "API", "扩展", "设置" };
        Check(expectedTags.SequenceEqual(navigation.Keys) && navigation.Values.Select(AutomationProperties.GetAutomationId).Distinct().Count() == 12, "原十二导航/稳定 ID 唯一");
        try
        {
            sidebarPreference = null;
            foreach (var (width, expected) in new[] { (699, 0), (700, 72), (1099, 72), (1100, 212) })
            {
                await Resize(width, 900);
                Check(sidebarColumn.ActualWidth == expected, $"断点 {width} 的侧栏为 {expected}");
                Check(navigationPane.IsVisible == (width >= 700), "抽屉关闭与侧栏可见性一致：" + width);
                breakpoints.Add(new { outerWidth = width, actualWidth = ActualWidth, sidebarWidth = sidebarColumn.ActualWidth, drawer = navigationDrawerOpen });
            }
            await Resize(768, 900); SetSidebarCollapsed(false); await Settle();
            Check(sidebarColumn.ActualWidth == 212, "中屏手工展开");
            await Resize(390, 640); navigationMenuButton.Focus(); Click(navigationMenuButton); await Settle();
            Check(navigationDrawerOpen && navigationPane.IsVisible && navigationBackdrop.IsVisible && !workspace.IsEnabled && sidebarColumn.ActualWidth == 0, "手机为单一拦截式抽屉，不占固定栏");
            Check(KeyboardNavigation.GetTabNavigation(navigationPane) == KeyboardNavigationMode.Cycle, "抽屉内键盘焦点循环");
            foreach (var pair in nav)
            {
                Check(ReferenceEquals(pair.Value, navigation[pair.Key]), "导航未复制：" + pair.Key);
                pair.Value.BringIntoView(); await Settle();
                Check(VisibleInside(pair.Value, navigationPane), "抽屉可滚达导航：" + pair.Key);
            }
            Capture("-drawer");
            Escape(navigation["设置"]); await Settle();
            Check(!navigationDrawerOpen && !navigationPane.IsVisible && workspace.IsEnabled && navigationMenuButton.IsKeyboardFocused, "Escape 关闭抽屉并回到入口焦点");
            Click(navigationMenuButton); await Settle(); Click(navigation["节点"]); await Settle();
            Check(!navigationDrawerOpen && navigationMenuButton.IsKeyboardFocused && Equals(((TabItem)tabs.SelectedItem).Tag, "节点"), $"选页关闭抽屉并返还焦点：open={navigationDrawerOpen}; focus={Keyboard.FocusedElement}; page={((TabItem)tabs.SelectedItem).Tag}");
            Click(navigationMenuButton); await Settle();
            navigationBackdrop.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left) { RoutedEvent = Mouse.MouseDownEvent }); await Settle();
            Check(!navigationDrawerOpen && navigationMenuButton.IsKeyboardFocused, "遮罩关闭抽屉并返还焦点");
            await Resize(700, 900); Check(sidebarColumn.ActualWidth == 212, "手机抽屉不覆盖桌面展开偏好");
            SetSidebarCollapsed(true); await Resize(1440, 900); Check(sidebarColumn.ActualWidth == 72, "宽窗保留手工折叠偏好");
            await Resize(requestedWidth, requestedHeight); sidebarPreference = originalPreference; ApplyResponsiveWorkspace(); await Settle();
            NavigateTo("节点"); await Settle();
            Check(Grid.GetRow(singleTargetGroup) == (requestedWidth < 700 ? 1 : 0) && VisibleInside(singleTargetGroup, Shell) && singleTargetChoice.ActualWidth > 0, "单设备目标换行且控件完整可见");
            foreach (var name in new[] { "DeviceDetails", "WorkspaceMore", "WorkspaceSettings" }) Check(VisibleInside(VisualDescendants<Button>(Shell).Single(button => button.Name == name), Shell), "全局入口在视口内：" + name);
            var settings = VisualDescendants<Button>(Shell).Single(button => button.Name == "WorkspaceSettings");
            Check(settings.ContextMenu is null, "设置入口不再承载网络/目录动作");
            var more = VisualDescendants<Button>(Shell).Single(button => button.Name == "WorkspaceMore"); more.Focus(); Click(more); await Settle();
            Check(more.ContextMenu is { IsOpen: true } && more.ContextMenu.Items.Count == 2 && ((MenuItem)more.ContextMenu.Items[0]).Header as string == "网络连接（高级）" && ((MenuItem)more.ContextMenu.Items[1]).Header as string == "立即更新目录", "更多菜单承载网络与更新目录");
            Escape(more.ContextMenu!); await Settle(); Check(!more.ContextMenu!.IsOpen && more.IsKeyboardFocused, "更多菜单 Escape 返回按钮焦点");
            Click(settings); await Settle(); Check(Equals(((TabItem)tabs.SelectedItem).Tag, "设置"), "设置按钮仅进入设置");
            var countsBeforeFixture = workspaceCounts.Text;
            try
            {
                workspaceCounts.SetCurrentValue(TextBlock.TextProperty, new string('数', 300)); workspaceStatus.SetCurrentValue(TextBlock.TextProperty, new string('状', 300)); await Settle();
                Check(VisibleInside(workspaceCounts, Shell) && VisibleInside(workspaceStatus, Shell) && workspaceCounts.TextTrimming == TextTrimming.CharacterEllipsis && workspaceStatus.TextTrimming == TextTrimming.CharacterEllipsis && workspaceStatus.ToolTip is string, "长页脚保持有界且完整信息可通过提示读取");
            }
            finally { workspaceCounts.Text = countsBeforeFixture; workspaceStatus.GetBindingExpression(TextBlock.TextProperty)!.UpdateTarget(); }
            var notice = workspace.Children.OfType<Border>().Single(border => Grid.GetRow(border) == 1); var errorScroll = (ScrollViewer)notice.Child; var errorText = (TextBlock)errorScroll.Content;
            try
            {
                notice.SetCurrentValue(VisibilityProperty, Visibility.Visible); errorText.SetCurrentValue(TextBlock.TextProperty, string.Join("\n", Enumerable.Repeat(new string('错', 150), 40))); await Settle();
                Check(errorScroll.ActualHeight <= 100 && errorScroll.ScrollableHeight > 0 && VisibleInside(errorScroll, Shell), "长错误正文限制高度并可滚动");
            }
            finally { notice.GetBindingExpression(VisibilityProperty)!.UpdateTarget(); errorText.GetBindingExpression(TextBlock.TextProperty)!.UpdateTarget(); }

            await Resize(768, 900); NavigateTo("Shell"); await Settle();
            var overlay = new Border { Width = 620, Height = 280, Background = ThemeBrush("SurfaceBrush"), HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top };
            Grid.SetColumnSpan(overlay, 2); Panel.SetZIndex(overlay, 1000); Shell.Children.Add(overlay);
            var fixture = new StackPanel(); overlay.Child = fixture;
            var fieldInput = BoundInput(nameof(WorkspaceViewModel.ShellInput)); var field = (Grid)Field("命令", fieldInput, 120); field.Width = 459; field.HorizontalAlignment = HorizontalAlignment.Left; fixture.Children.Add(field);
            var toolbar = Toolbar(); toolbar.Width = 280; toolbar.HorizontalAlignment = HorizontalAlignment.Left; var toolbarInput = new TextBox { Width = 470, MaxWidth = 500 }; toolbar.Children.Add(toolbarInput); fixture.Children.Add(toolbar);
            try
            {
                ViewModel.ShellInput = "responsive-field-state"; await Settle();
                var focusDeadline = System.Diagnostics.Stopwatch.StartNew();
                do
                {
                    Activate(); Keyboard.Focus(fieldInput); await Settle();
                    if (IsActive && fieldInput.IsKeyboardFocused) break;
                    await Task.Delay(20);
                } while (focusDeadline.Elapsed < TimeSpan.FromSeconds(1));
                fieldInput.Select(3, 5);
                var selectionStart = fieldInput.SelectionStart; var selectionLength = fieldInput.SelectionLength; var caret = fieldInput.CaretIndex;
                Check(IsActive && fieldInput.IsKeyboardFocused && fieldInput.Text == "responsive-field-state" && selectionStart == 3 && selectionLength == 5, $"字段初态已激活并持有真实键盘焦点和选区：active={IsActive}; focus={fieldInput.IsKeyboardFocused}; text={fieldInput.Text}; selection={selectionStart}/{selectionLength}; waitedMs={focusDeadline.ElapsedMilliseconds}");
                fieldStates.Add(new { phase = "before", field.ActualWidth, fieldInput.Text, fieldInput.IsKeyboardFocused, fieldInput.SelectionStart, fieldInput.SelectionLength, fieldInput.CaretIndex });
                foreach (var width in new[] { 459, 460, 459, 460, 459 })
                {
                    field.Width = width; await Settle();
                    Check(field.ActualWidth == width && Grid.GetRow(fieldInput) == (width < 460 ? 1 : 0), "字段自身 459/460 断点：" + width);
                    Check(field.RowDefinitions.Count == 2 && field.Children.Contains(fieldInput) && BindingOperations.GetBinding(fieldInput, TextBox.TextProperty)?.Path.Path == nameof(WorkspaceViewModel.ShellInput), "字段保持原输入/绑定/固定行定义：" + width);
                    Check(fieldInput.Text == "responsive-field-state" && fieldInput.IsKeyboardFocused && fieldInput.SelectionStart == selectionStart && fieldInput.SelectionLength == selectionLength && fieldInput.CaretIndex == caret, $"字段换行保留输入/焦点/选区/光标：{width}; text={fieldInput.Text}; focus={fieldInput.IsKeyboardFocused}; selection={fieldInput.SelectionStart}/{fieldInput.SelectionLength}; caret={fieldInput.CaretIndex}; expected={selectionStart}/{selectionLength}/{caret}");
                    fieldStates.Add(new { phase = "after", field.ActualWidth, fieldInput.Text, fieldInput.IsKeyboardFocused, fieldInput.SelectionStart, fieldInput.SelectionLength, fieldInput.CaretIndex });
                }
                Check(toolbarInput.ActualWidth <= 280 && toolbarInput.MaxWidth == 280, "工具栏窄宽度限制实际输入");
                toolbar.Width = 600; await Settle(); Check(toolbarInput.ActualWidth == 470 && toolbarInput.MaxWidth == 500, "工具栏返回宽屏恢复原尺寸上限");
            }
            finally { Shell.Children.Remove(overlay); ViewModel.ShellInput = originalShell; }

            var negativeOwner = new Rect(-1700, 50, 768, 1024); var negativeWork = new Rect(-1920, 0, 1920, 1080);
            var negative = ResponsiveLayout.CalculateOwnedBounds(negativeOwner, negativeWork, 700, 900, 360, 480);
            Check(negativeOwner.Contains(negative.Bounds) && negativeWork.Contains(negative.Bounds) && negative.Bounds.X < 0 && negative.MinimumHeight == 480, "负坐标副屏几何保留工作区与旧最小值");
            foreach (var operation in new Action[]
            {
                () => ResponsiveLayout.CalculateOwnedBounds(new Rect(double.NaN, 0, 390, 640), negativeWork, 420, 640, 360, 480),
                () => ResponsiveLayout.CalculateOwnedBounds(negativeOwner, negativeWork, double.NaN, 640, 360, 480),
                () => ResponsiveLayout.CalculateOwnedBounds(new Rect(3000, 0, 390, 640), new Rect(0, 0, 1920, 1080), 420, 640, 360, 480)
            })
            {
                var rejected = false; try { operation(); } catch (InvalidOperationException) { rejected = true; }
                Check(rejected, "弹窗非法/不可见几何快速拒绝");
            }
            await Resize(requestedWidth, requestedHeight); sidebarPreference = originalPreference; ApplyResponsiveWorkspace(); await Settle();
            void CheckOwned(Window dialog)
            {
                var bounds = new Rect(dialog.Left, dialog.Top, dialog.ActualWidth, dialog.ActualHeight);
                Check(dialog.IsLoaded && dialog.IsVisible && new WindowInteropHelper(dialog).Handle != IntPtr.Zero, "真实弹窗已显示：" + dialog.Title);
                Check(new Rect(Left, Top, ActualWidth, ActualHeight).Contains(bounds) && ResponsiveLayout.OwnerWorkArea(this).Contains(bounds), "弹窗在 owner/当前显示器工作区内：" + dialog.Title);
                geometries.Add(new { dialog.Title, dialog.Left, dialog.Top, dialog.ActualWidth, dialog.ActualHeight, dialog.MinWidth, dialog.MinHeight, dialog.MaxWidth, dialog.MaxHeight });
            }
            ShowDeviceDirectory(); await Settle(); CheckOwned(directoryWindow!);
            Check(VisibleInside(VisualDescendants<Button>(directoryWindow!).Single(button => button.ToolTip as string == "保存信息"), (FrameworkElement)directoryWindow!.Content), "设备详情保存按钮在视口内"); directoryWindow.Hide();
            ShowScanner(); await Settle(); CheckOwned(scannerWindow!);
            var scannerScroll = (ScrollViewer)scannerWindow!.Content; scannerScroll.ScrollToEnd(); await Settle();
            Check(VisualDescendants<Button>(scannerWindow).Where(button => button.IsVisible).All(button => VisibleInside(button, (FrameworkElement)scannerWindow.Content)), "网络弹窗滚动后按钮可见"); scannerWindow.Hide();
            ShowText("响应式长日志", string.Join("\n", Enumerable.Repeat(new string('X', 300), 80))); await Settle();
            var log = OwnedWindows.Cast<Window>().Single(window => window.Title == "响应式长日志"); CheckOwned(log);
            var logInput = (TextBox)log.Content; logInput.ScrollToEnd(); await Settle(); Check(logInput.TextWrapping == TextWrapping.Wrap && logInput.VerticalOffset > 0, "长日志换行且可滚动至结尾"); log.Close();
            var nativeGeometryOnly = new Window { Owner = this, Title = "原生容器几何夹具", MinWidth = 500, MinHeight = 700, Content = new Border() };
            ResponsiveLayout.FitOwnedWindow(nativeGeometryOnly, this, 600, 850); nativeGeometryOnly.Show(); await Settle(); CheckOwned(nativeGeometryOnly); nativeGeometryOnly.Close();

            Exception? modalFailure = null; var modalCount = 0;
            responsiveOwnedDialogFixture = dialog =>
            {
                var timeout = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
                timeout.Tick += (_, _) => { timeout.Stop(); modalFailure = new InvalidOperationException("弹窗夹具超时：" + dialog.Title); dialog.Close(); };
                dialog.Closed += (_, _) => timeout.Stop(); timeout.Start();
                dialog.ContentRendered += async (_, _) =>
                {
                    try
                    {
                        await Settle(); CheckOwned(dialog); modalCount++;
                        var buttons = VisualDescendants<Button>(dialog).ToArray(); var cancel = buttons.Single(button => button.IsCancel);
                        Check(buttons.Where(button => button.IsDefault || button.IsCancel).All(button => VisibleInside(button, (FrameworkElement)dialog.Content)), "模态确认/取消按钮可见：" + dialog.Title);
                        foreach (var scroll in VisualDescendants<ScrollViewer>(dialog).Where(scroll => scroll.ScrollableHeight > 0)) scroll.ScrollToEnd();
                        await Settle();
                        if (screenshot is not null) SaveRender(Path.Combine(Path.GetDirectoryName(screenshot)!, "modal-" + modalCount + ".png"), dialog);
                        var peer = new ButtonAutomationPeer(cancel); ((IInvokeProvider)peer.GetPattern(PatternInterface.Invoke)).Invoke();
                    }
                    catch (Exception exception) { modalFailure = exception; dialog.Close(); }
                };
            };
            try
            {
                var target = ViewModel.CurrentDevice ?? ViewModel.Devices.First();
                Check(!ConfirmApplicationPlan([new(new(target.Serial, target.Name, target.Info.Generation, target.Info.TransportId), ApplicationOperation.Uninstall, "com.matrix.demo")]), "应用确认实际取消，未调用卸载");
                if (modalFailure is not null) throw modalFailure;
                await EditLdInstanceAsync(true); if (modalFailure is not null) throw modalFailure;
                Check(modalCount == 2, "实际应用确认与雷电编辑均仅取消");
            }
            finally { responsiveOwnedDialogFixture = null; }
            ShowDeviceDirectory(); await Settle(); CheckOwned(directoryWindow!); directoryWindow!.Hide();
            ShowScanner(); await Settle(); CheckOwned(scannerWindow!); scannerWindow!.Hide();
            var follow = new Window { Owner = this, Title = "跟随 owner 几何夹具", MinWidth = 360, MinHeight = 480, Content = new Border() };
            ResponsiveLayout.FitOwnedWindow(follow, this, 700, 700); follow.Show(); await Settle();
            await Resize(390, 640); CheckOwned(follow); await Resize(requestedWidth, requestedHeight); CheckOwned(follow);
            var lastValid = new Rect(follow.Left, follow.Top, follow.ActualWidth, follow.ActualHeight); var ownerLeft = Left;
            try
            {
                Left = -100000; await Settle();
                Check(ResponsiveLayout.FollowFailure(follow) is not null && new Rect(follow.Left, follow.Top, follow.ActualWidth, follow.ActualHeight) == lastValid, "owner 移出工作区时保留最后边界并记录诊断，不冒泡崩溃");
            }
            finally { Left = ownerLeft; await Settle(); }
            CheckOwned(follow); Check(ResponsiveLayout.FollowFailure(follow) is null, "owner 回到工作区后恢复跟随"); follow.Close();
        }
        finally
        {
            responsiveOwnedDialogFixture = null; directoryWindow?.Hide(); scannerWindow?.Hide();
            if (navigationDrawerOpen) CloseNavigationDrawer();
            ViewModel.ShellInput = originalShell; ViewModel.TextInput = originalText;
            sidebarPreference = originalPreference; await Resize(requestedWidth, requestedHeight); ApplyResponsiveWorkspace(); NavigateTo(originalPage); await Settle();
        }
        Check(ReferenceEquals(main, ViewModel.MainDevice) && ReferenceEquals(current, ViewModel.CurrentDevice) && selected.SequenceEqual(ViewModel.Devices.Select(row => (row.Serial, row.IsSelected))) && ViewModel.ShellInput == originalShell && ViewModel.TextInput == originalText, "调整布局不改变主控/目标/勾选/输入");
        Check(ActualWidth == requestedWidth && ActualHeight == requestedHeight, "最终取证恢复原始请求尺寸");
        Capture("");
        var report = JsonSerializer.Serialize(new { scope = "A04 shared shell only; fixed page multi-column bodies deferred; no device/process/network operations; native MessageBox/file chooser and physical mixed-DPI not tested; routed keyboard/click fixtures, not OS mouse synthesis", requestedWidth, requestedHeight, actualWidth = ActualWidth, actualHeight = ActualHeight, hwnd = new WindowInteropHelper(this).Handle.ToInt64(), loaded = IsLoaded, visible = IsVisible, captureState = "idle", assertions = checks.Count, checks, breakpoints, fieldStates, geometries }, JsonDefaults.Options);
        await File.WriteAllTextAsync(Path.Combine(ViewModel.DataDirectory, "responsive-shell.json"), report);
        if (screenshot is not null) await File.WriteAllTextAsync(Path.ChangeExtension(screenshot, ".json"), report);
    }
}
