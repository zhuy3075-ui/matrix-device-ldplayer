using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using MatrixDevice.Core;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    private async Task<object> SettleIdlePageCaptureAsync(string page, IReadOnlyDictionary<DeviceRow, DeviceInfo> restoredDevices, string restoredFilter)
    {
        var deadline = System.Diagnostics.Stopwatch.StartNew();
        var reason = "尚未布局";
        while (deadline.Elapsed < TimeSpan.FromSeconds(3))
        {
            await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ContextIdle);
            await Dispatcher.InvokeAsync(UpdateLayout, DispatcherPriority.Render);
            await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
            await Dispatcher.InvokeAsync(UpdateLayout, DispatcherPriority.Render);
            if (ViewModel.Busy || ViewModel.Transferring || deviceStatusFilter != restoredFilter || restoredDevices.Any(pair => !ViewModel.Devices.Contains(pair.Key) || pair.Key.Info != pair.Value))
                throw new InvalidOperationException("分页截图不是已恢复的idle状态：" + page);
            try
            {
                var rows = page == "设备墙 / 主控" ? CaptureIdleDeviceRows() : [];
                var connected = unifiedDevices.Count(row => row.Connected);
                if (!workspaceCounts.Text.Contains($"已连接 {connected}", StringComparison.Ordinal)) throw new InvalidOperationException("页脚连接数未恢复。");
                return new
                {
                    page, captureState = "idle", statusFilter = deviceStatusFilter, busy = ViewModel.Busy, transferring = ViewModel.Transferring,
                    restoredDevices = ViewModel.Devices.Select(row => new { row.Serial, row.Info.State, row.Info.Generation, row.Info.TransportId }).ToArray(),
                    expectedRestoredDevices = restoredDevices.Select(pair => new { pair.Value.Serial, pair.Value.State, pair.Value.Generation, pair.Value.TransportId }).ToArray(),
                    directoryCount = unifiedDevices.Count, filteredCount = unifiedDeviceView.Cast<UnifiedDeviceRow>().Count(), connectedCount = connected,
                    footer = workspaceCounts.Text, actualWidth = ActualWidth, actualHeight = ActualHeight, renderedDeviceRows = rows, settledMilliseconds = deadline.ElapsedMilliseconds
                };
            }
            catch (InvalidOperationException ex) { reason = ex.Message; }
            await Task.Delay(20);
        }
        throw new InvalidOperationException($"分页截图布局未在3秒内恢复：{page}；{reason}");
    }

    private object[] CaptureIdleDeviceRows()
    {
        var grid = VisualDescendants<DataGrid>(wallList).Single();
        var expected = unifiedDeviceView.Cast<UnifiedDeviceRow>().ToArray();
        var scroll = DeviceListScrollViewer() ?? throw new InvalidOperationException("设备列表缺少实际滚动容器。");
        var presenter = VisualDescendants<ScrollContentPresenter>(scroll).FirstOrDefault(item => item.Name == "PART_ScrollContentPresenter")
            ?? throw new InvalidOperationException("设备列表缺少实际正文视口。");
        var viewport = presenter.TransformToAncestor(grid).TransformBounds(new Rect(presenter.RenderSize));
        var rows = new List<object>();
        foreach (var item in expected)
        {
            if (grid.ItemContainerGenerator.ContainerFromItem(item) is not DataGridRow row || !row.IsVisible || row.ActualWidth <= 0 || row.ActualHeight <= 0)
                throw new InvalidOperationException("非空目录尚未生成实际行：" + item.Name);
            var bounds = row.TransformToAncestor(grid).TransformBounds(new Rect(row.RenderSize));
            var visible = Rect.Intersect(bounds, viewport);
            if (visible.IsEmpty || visible.Width <= 0 || bounds.Top < viewport.Top || bounds.Bottom > viewport.Bottom) throw new InvalidOperationException("设备行没有完整高度进入实际视口：" + item.Name);
            bool Visible(FrameworkElement control)
            {
                if (!control.IsVisible || control.ActualWidth <= 0 || control.ActualHeight <= 0) return false;
                var controlBounds = control.TransformToAncestor(grid).TransformBounds(new Rect(control.RenderSize));
                return viewport.Contains(controlBounds);
            }
            if (!VisualDescendants<TextBlock>(row).Any(text => text.Text == item.Name && Visible(text))) throw new InvalidOperationException("设备名称尚未呈现在视口：" + item.Name);
            var action = VisualDescendants<Button>(row).FirstOrDefault(button => Visible(button) && VisualDescendants<TextBlock>(button).Any(text => text.Text == item.PrimaryLabel && Visible(text)))
                ?? throw new InvalidOperationException("设备操作尚未呈现在视口：" + item.Name);
            foreach (var button in VisualDescendants<UnifiedDeviceTile>(row).SelectMany(tile => VisualDescendants<Button>(tile)).Where(button => button.Visibility == Visibility.Visible))
            {
                var cell = VisualDescendants<DataGridCell>(row).FirstOrDefault(cell => VisualDescendants<Button>(cell).Contains(button))
                    ?? throw new InvalidOperationException("设备操作没有实际表格单元：" + item.Name);
                var actionBounds = button.TransformToAncestor(grid).TransformBounds(new Rect(button.RenderSize));
                var cellBounds = cell.TransformToAncestor(grid).TransformBounds(new Rect(cell.RenderSize));
                if (button.ActualWidth < 44 || button.ActualHeight < 44 || !Visible(button) || !cellBounds.Contains(actionBounds) || !bounds.Contains(actionBounds))
                    throw new InvalidOperationException("设备操作未完整进入实际单元/行/视口或不足44命中区：" + item.Name + ":" + button.Name);
            }
            rows.Add(new { item.Key, item.Name, item.State, item.PrimaryLabel, row.ActualWidth, row.ActualHeight, visibleBounds = new { visible.X, visible.Y, visible.Width, visible.Height }, action = System.Windows.Automation.AutomationProperties.GetName(action) });
        }
        if (expected.Length != 0 && rows.Count == 0) throw new InvalidOperationException("非空目录截图没有实际设备行。");
        return rows.ToArray();
    }

    private async Task VerifyListConsoleSimplificationAsync()
    {
        NavigateTo("设备墙 / 主控");
        await Dispatcher.InvokeAsync(UpdateLayout, System.Windows.Threading.DispatcherPriority.Render);
        if (VisualDescendants<ComboBox>(Shell).Any(input => input.Items.Cast<object>().Any(item => item as string == "全部分组")))
            throw new InvalidOperationException("设备管理仍显示已删除的全部分组筛选。");
        var obsolete = new[] { "设备卡片", "屏幕视图", "启动并连接", "接入已运行实例", "选择当前筛选中的全部设备" };
        if (VisualDescendants<FrameworkElement>(Shell).Any(control => control.ToolTip is string tip && obsolete.Any(label => tip.StartsWith(label, StringComparison.Ordinal))))
            throw new InvalidOperationException("设备管理仍显示重复视图、启动或全选入口。");
        if (VisualDescendants<FrameworkElement>(Shell).Any(control => control.ToolTip as string is "同步鼠标" or "同步键盘" or "自动截图"))
            throw new InvalidOperationException("设备列表仍显示脱离主控场景的同步或截图开关。");
    }

    private async Task VerifyOneClickProjectionAsync()
    {
        NavigateTo("设备墙 / 主控");
        SetWallViewMode("list");
        await Dispatcher.InvokeAsync(UpdateLayout, System.Windows.Threading.DispatcherPriority.Render);
        if (VisualDescendants<Expander>(Shell).Any(expander => expander.Header as string == "实例快捷操作")
            || VisualDescendants<TextBlock>(Shell).Any(text => text.Text is "应用包名" or "输入文字"))
            throw new InvalidOperationException("设备管理仍保留与应用管理/文本输入重复的实例快捷表单。");
        if (VisualDescendants<TextBlock>(Shell).Any(text => text.Text == "后台启动") ||
            VisualDescendants<FrameworkElement>(Shell).Any(control => control.ToolTip is string tip && tip.Contains("隐藏本次启动", StringComparison.Ordinal)))
            throw new InvalidOperationException("已删除的后台启动入口仍存在。");
        var expectedActions = new[] { "启动所选", "一键启动全部", "新建空白实例" };
        foreach (var label in expectedActions)
            if (!VisualDescendants<Button>(Shell).Any(button => button.ToolTip is string tip && tip.StartsWith(label, StringComparison.Ordinal)))
                throw new InvalidOperationException("统一设备页遗漏有效操作：" + label);
        var clarityFailures = new List<string>();
        var instanceActions = VisualDescendants<Button>(Shell).Single(button => button.ContextMenu is { } menu
            && new[] { "资源配置", "关闭实例", "重启实例" }.All(label => menu.Items.OfType<MenuItem>().Any(item => item.Header as string == label)));
        var instanceCaption = string.Join("", VisualDescendants<TextBlock>(instanceActions).Where(text => text.FontFamily.Source != "Segoe MDL2 Assets").Select(text => text.Text));
        if (instanceActions.Name != "InstanceActions" || instanceCaption != "实例操作" || instanceActions.ToolTip as string != "实例操作"
            || System.Windows.Automation.AutomationProperties.GetName(instanceActions) != "实例操作")
            clarityFailures.Add($"实例菜单应使用稳定Name InstanceActions及可见/无障碍名称实例操作；actual name={instanceActions.Name}, caption={instanceCaption}, automation={System.Windows.Automation.AutomationProperties.GetName(instanceActions)}, tooltip={instanceActions.ToolTip}");
        var workspaceMore = VisualDescendants<Button>(Shell).Single(button => button.Name == "WorkspaceMore");
        if (ReferenceEquals(instanceActions, workspaceMore) || ReferenceEquals(instanceActions.ContextMenu, workspaceMore.ContextMenu))
            clarityFailures.Add("实例操作与工作区更多入口必须保持独立菜单和目标范围");
        var launchToolTips = new Dictionary<string, string>
        {
            ["启动所选"] = "启动所选：操作列表中已勾选且尚未连接的可接入实例，包括被当前筛选隐藏的已勾选实例；已运行的实例仅连接，不重复启动；不包含受保护实例。完成核验后自动连接并加入群控。",
            ["一键启动全部"] = "一键启动全部：操作全部尚未连接的可接入实例，包括被当前筛选隐藏的实例；已运行的实例仅连接，不重复启动；不包含受保护实例。完成核验后自动连接并加入群控。"
        };
        foreach (var (label, expectedTip) in launchToolTips)
        {
            var launch = VisualDescendants<Button>(Shell).Single(button => System.Windows.Automation.AutomationProperties.GetName(button) == label);
            if (launch.ToolTip as string != expectedTip)
                clarityFailures.Add($"{label}提示必须说明所选/全部、筛选隐藏、运行态仅连接和保护范围；actual={launch.ToolTip}");
            if (launch.IsEnabled || !ToolTipService.GetShowOnDisabled(launch) || !ToolTipService.GetIsEnabled(launch))
                clarityFailures.Add($"{label}在真实Demo禁用状态仍须配置可读提示；enabled={launch.IsEnabled}, showOnDisabled={ToolTipService.GetShowOnDisabled(launch)}, tooltipEnabled={ToolTipService.GetIsEnabled(launch)}");
        }
        if (clarityFailures.Count > 0) throw new InvalidOperationException("设备操作清晰度验收失败：" + string.Join("；", clarityFailures));
        foreach (var label in new[] { "资源配置", "关闭实例", "重启实例" })
            if (!ldCommandsByLabel.ContainsKey(label)) throw new InvalidOperationException("更多菜单遗漏生命周期命令：" + label);
        var cancel = VisualDescendants<Button>(Shell).Single(button => button.ToolTip as string == "取消启动");
        if (cancel.Visibility != Visibility.Collapsed) throw new InvalidOperationException("没有启动任务时仍显示取消启动按钮。");
        NavigateTo("设备墙 / 主控"); UpdateUnifiedDirectory();
        await Dispatcher.InvokeAsync(UpdateLayout, System.Windows.Threading.DispatcherPriority.Render);
        var originalDevices = ViewModel.Devices.Count;
        var originalSelection = unifiedDevices.ToDictionary(row => row.Key, row => row.IsSelected);
        if (unifiedDevices.Select(row => row.Key).Distinct().Count() != unifiedDevices.Count || wallWorkspace.RowDefinitions.Count != 1) throw new InvalidOperationException("统一目录重复或仍有底部独立卡片区。");
        if (!unifiedDevices.Any(row => row.Instance is { AndroidStarted: false } && row.Device is null)) throw new InvalidOperationException("统一目录遗漏关闭实例。");
        try
        {
            foreach (var row in unifiedDevices.ToArray()) row.IsSelected = false;
            var cold = unifiedDevices.First(row => row.Instance is { Index: > 0, AndroidStarted: false } && row.Device is null);
            cold.Update(cold.Instance, null, false, "已连接");
            if (cold.State == "已连接") throw new InvalidOperationException("连接失效后仍显示历史已连接阶段。");
            UpdateUnifiedDirectory();
            cold.IsSelected = true;
            if (!workspaceCounts.Text.EndsWith("已选 1", StringComparison.Ordinal)) throw new InvalidOperationException("关闭实例的选择数量没有立即更新。");
            if (ViewModel.Targets().Count != 0 || ViewModel.CanWriteTargets()) throw new InvalidOperationException("只选择关闭实例时仍回落到旧当前设备。");
            var transfers = ViewModel.Transfers.Count;
            try { ViewModel.QueuePush(typeof(MainWindow).Assembly.Location, false); throw new InvalidOperationException("未连接选择被静默忽略。"); }
            catch (MatrixDevice.Core.DeviceException) { }
            if (ViewModel.Transfers.Count != transfers) throw new InvalidOperationException("未连接选择导致其它设备被加入传输。");
            ViewModel.Devices[0].IsSelected = true;
            if (ViewModel.Targets().Count != 0 || ViewModel.CanWriteTargets()) throw new InvalidOperationException("混合选择静默遗漏未连接设备。");
            var all = VisualDescendants<CheckBox>(Shell).Single(input => input.Name == "SelectAllDevices");
            all.IsChecked = true; all.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            if (unifiedDevices.Any(row => row.CanSelect && !row.IsSelected) || ViewModel.Devices.Count != originalDevices) throw new InvalidOperationException("全选未涵盖关闭实例或伪造了可控设备。");
            if (all.IsChecked != true || !all.IsThreeState) throw new InvalidOperationException("全选表头没有反映完整选择或三态语义。");
            UpdateUnifiedDirectory();
            if (unifiedDevices.Any(row => row.CanSelect && !row.IsSelected)) throw new InvalidOperationException("自动目录更新丢失选择。");
            unifiedDevices.First(row => row.CanSelect).IsSelected = false;
            if (all.IsChecked is not null) throw new InvalidOperationException("部分选择未显示表头中间态。");
            all.IsChecked = false; all.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            if (unifiedDevices.Any(row => row.IsSelected) || ViewModel.Devices.Any(row => row.IsSelected) || all.IsChecked != false)
                throw new InvalidOperationException("取消全选没有清除统一目录及设备选择。");
        }
        finally { foreach (var row in unifiedDevices.ToArray()) row.IsSelected = originalSelection.GetValueOrDefault(row.Key); }
        var originalConfirm = ViewModel.Confirm; var confirmations = 0;
        ViewModel.Confirm = (_, _) => { confirmations++; return false; };
        try
        {
            foreach (var invalid in new Dictionary<int, string>[] { new(), new() { [1] = "demo", [2] = "extra" }, new() { [1] = "" } })
            {
                try { await ConnectLdInstancesAsync([1], invalid); throw new InvalidOperationException("不完整的预批准串号进入启动流程。"); }
                catch (MatrixDevice.Core.DeviceException ex) when (ex.Status == 400 && ex.Code == 1007) { }
            }
            try { await ConnectLdInstancesAsync([1]); throw new InvalidOperationException("演示模式进入真实启动。"); }
            catch (MatrixDevice.Core.DeviceException ex) when (ex.Message.Contains("演示", StringComparison.Ordinal)) { }
            if (confirmations != 0) throw new InvalidOperationException("普通启动仍弹出确认框。");
        }
        finally { ViewModel.Confirm = originalConfirm; }
    }

    private async Task VerifyCanonicalDeviceActionsAsync(string? screenshot)
    {
        if (!ViewModel.Demo) throw new InvalidOperationException("入口归属验收仅限演示模式。");
        var previousTag = (tabs.SelectedItem as TabItem)?.Tag?.ToString() ?? "设备墙 / 主控";
        var previousMode = wallViewMode;
        var current = ViewModel.CurrentDevice; var main = ViewModel.MainDevice ?? throw new InvalidOperationException("主控夹具缺失。");
        var info = main.Info; var text = ViewModel.TextInput; var sync = ViewModel.SyncKeyboard; var mouseSync = ViewModel.SyncMouse;
        var selections = ViewModel.Devices.ToDictionary(row => row, row => row.IsSelected);
        var unifiedSelections = unifiedDevices.ToDictionary(row => row.Key, row => row.IsSelected);
        try
        {
            NavigateTo("设备墙 / 主控"); SetWallViewMode("list");
            if (wallMainPanel.Visibility != Visibility.Collapsed) throw new InvalidOperationException("主控面板没有默认收起。");
            ClearWallSelection();
            Activate();
            await Dispatcher.InvokeAsync(UpdateLayout, System.Windows.Threading.DispatcherPriority.Render);
            if (!IsActive) throw new InvalidOperationException("截图调度验收窗口未激活，不能证明可见主控请求路径。");
            var hiddenFrames = ViewModel.Devices.ToDictionary(row => row, row => row.CapturedAt);
            CloseMainControl(); await CaptureTickAsync();
            if (hiddenFrames.Any(pair => pair.Key.CapturedAt != pair.Value))
                throw new InvalidOperationException("隐藏主控面板仍请求了设备截图。");
            await Task.Delay(20);
            await OpenMainControlAsync(main);
            await Dispatcher.InvokeAsync(UpdateLayout, System.Windows.Threading.DispatcherPriority.Render);
            if (wallMainPanel.Visibility != Visibility.Visible || !mainControlOpen || mainBodyScroll.ActualHeight <= 0 || mainControlBack.ActualWidth != 44 || mainControlBack.ActualHeight != 44)
                throw new InvalidOperationException("列表主控入口没有展开嵌入面板。");
            if (!IsActive || main.CapturedAt is not { } captured || hiddenFrames[main] is { } beforeCapture && captured <= beforeCapture
                || hiddenFrames.Any(pair => !ReferenceEquals(pair.Key, main) && pair.Key.CapturedAt != pair.Value))
                throw new InvalidOperationException("可见主控没有请求新截图，窗口失焦，或刷新了其它设备。");
            foreach (var (path, caption, scope) in new[]
            {
                (nameof(WorkspaceViewModel.SyncMouse), "同步点击滑动", "嵌入画面"),
                (nameof(WorkspaceViewModel.SyncKeyboard), "同步按键", "程序")
            })
            {
                var controls = VisualDescendants<CheckBox>(Shell).Where(input => BindingOperations.GetBinding(input, ToggleButton.IsCheckedProperty)?.Path.Path == path).ToArray();
                if (controls.Length != 1 || !VisualDescendants<CheckBox>(wallMainPanel).Contains(controls[0]) || controls[0].Content as string != caption
                    || controls[0].ToolTip is not string tip || !tip.Contains("主控", StringComparison.Ordinal) || !tip.Contains("勾选", StringComparison.Ordinal) || !tip.Contains(scope, StringComparison.Ordinal))
                    throw new InvalidOperationException("主控同步控件的唯一归属或作用范围提示不清晰：" + path);
            }
            var expected = new Dictionary<string, string> { ["MainVolumeUp"] = "增大音量", ["MainVolumeDown"] = "减小音量" };
            var buttons = VisualDescendants<Button>(wallMainPanel).Where(button => expected.ContainsKey(button.Name)).ToArray();
            if (buttons.Length != 2 || buttons.Any(button => button.ToolTip as string != expected[button.Name] || button.Content is not TextBlock || button.ActualWidth != 44 || button.ActualHeight != 44 || button.Command is not AsyncCommand))
                throw new InvalidOperationException("主控音量图标、提示或稳定尺寸缺失。");
            var other = ViewModel.Devices.First(row => !ReferenceEquals(row, main));
            foreach (var row in ViewModel.Devices) row.IsSelected = ReferenceEquals(row, other);
            foreach (var enabled in new[] { false, true })
            {
                ViewModel.SyncKeyboard = enabled; ViewModel.SyncMouse = enabled; ViewModel.UpdateSelection();
                string[] expectedTargets = enabled ? [other.Serial] : [];
                if (!ViewModel.InputTargets(main.Serial, false).SequenceEqual(expectedTargets) || !ViewModel.InputTargets(main.Serial, true).SequenceEqual(expectedTargets))
                    throw new InvalidOperationException("主控鼠标或按键同步没有沿用勾选目标规则。");
                foreach (var button in buttons)
                {
                    if (!button.Command!.CanExecute(null)) throw new InvalidOperationException("有效主控的音量命令不可用。");
                    await ((AsyncCommand)button.Command).ExecuteAsync();
                }
            }
            ViewModel.SetExternalBusy(true);
            try { if (buttons.Any(button => button.Command!.CanExecute(null))) throw new InvalidOperationException("繁忙时主控音量仍可操作。"); }
            finally { ViewModel.SetExternalBusy(false); }
            main.Info = info with { State = "Offline" }; ViewModel.UpdateSelection();
            if (buttons.Any(button => button.Command!.CanExecute(null))) throw new InvalidOperationException("离线主控音量仍可操作。");
            main.Info = info; ViewModel.SyncKeyboard = sync; ViewModel.SyncMouse = mouseSync;
            foreach (var pair in selections) pair.Key.IsSelected = pair.Value;
            ViewModel.UpdateSelection();
            mainBodyScroll.ScrollToEnd();
            await Dispatcher.InvokeAsync(UpdateLayout, System.Windows.Threading.DispatcherPriority.Render);
            foreach (var button in buttons)
            {
                var bounds = button.TransformToAncestor(mainBodyScroll).TransformBounds(new Rect(button.RenderSize));
                if (!new Rect(0, 0, mainBodyScroll.ViewportWidth, mainBodyScroll.ViewportHeight).Contains(bounds)) throw new InvalidOperationException("主控音量按钮未完整滚动到正文视口。");
            }
            var cast = VisualDescendants<Expander>(wallMainPanel).Single(); cast.IsExpanded = true; await Dispatcher.InvokeAsync(UpdateLayout, DispatcherPriority.Render); mainBodyScroll.ScrollToEnd(); await Dispatcher.InvokeAsync(UpdateLayout, DispatcherPriority.Render);
            var lastCast = VisualDescendants<Button>(cast).Last(); var castBounds = lastCast.TransformToAncestor(mainBodyScroll).TransformBounds(new Rect(lastCast.RenderSize));
            if (!new Rect(0, 0, mainBodyScroll.ViewportWidth, mainBodyScroll.ViewportHeight).Contains(castBounds) || !mainControlBack.IsVisible) throw new InvalidOperationException("投屏末端或固定返回不可达。");
            cast.IsExpanded = false;
            if (screenshot is not null) SaveRender(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(screenshot)!, "canonical-main-controls.png"));
            NavigateTo("应用"); SyncApplicationTargets();
            await Dispatcher.InvokeAsync(UpdateLayout, System.Windows.Threading.DispatcherPriority.Render);
            var routedFrames = ViewModel.Devices.ToDictionary(row => row, row => row.CapturedAt);
            await CaptureTickAsync();
            if (routedFrames.Any(pair => pair.Key.CapturedAt != pair.Value))
                throw new InvalidOperationException("切换到应用页后仍请求设备管理的主控截图。");
            if (applicationTargets.Any(row => row.IsSelected) || CanManageApplicationTargets() || !VisualDescendants<Button>(Shell).Any(button => button.ToolTip as string == "安装 APK"))
                throw new InvalidOperationException("应用入口缺失，或统一目录勾选隐式授权了应用目标。");
            NavigateTo("文本"); ViewModel.TextInput = "入口去重中文验证";
            await Dispatcher.InvokeAsync(UpdateLayout, System.Windows.Threading.DispatcherPriority.Render);
            if (!VisualDescendants<TextBox>(Shell).Any(input => BindingOperations.GetBinding(input, TextBox.TextProperty)?.Path.Path == nameof(WorkspaceViewModel.TextInput))
                || !VisualDescendants<Button>(Shell).Any(button => ReferenceEquals(button.Command, ViewModel.TextCommand)))
                throw new InvalidOperationException("独立文本输入或发送命令入口缺失。");
            if (!ReferenceEquals(current, ViewModel.CurrentDevice) || !ReferenceEquals(main, ViewModel.MainDevice) || selections.Any(pair => pair.Key.IsSelected != pair.Value))
                throw new InvalidOperationException("入口切换改写了设备上下文、群控勾选或主控。");
        }
        finally
        {
            main.Info = info; ViewModel.TextInput = text; ViewModel.SyncKeyboard = sync; ViewModel.SyncMouse = mouseSync;
            CloseMainControl();
            foreach (var row in unifiedDevices) row.IsSelected = unifiedSelections.GetValueOrDefault(row.Key);
            foreach (var pair in selections) pair.Key.IsSelected = pair.Value;
            ViewModel.UpdateSelection();
            SetWallViewMode(previousMode); NavigateTo(previousTag);
            if (wallMainPanel.Visibility != Visibility.Collapsed || wallWorkspace.ColumnDefinitions[1].Width.Value != 0)
                throw new InvalidOperationException("关闭主控后没有归还列表宽度。");
        }
    }

    private async Task VerifyOperationGatesAsync()
    {
        var current = ViewModel.CurrentDevice!; var originalInfo = current.Info;
        try
        {
            current.Info = originalInfo with { State = "Offline" };
            if (!ViewModel.DeviceContext.Contains("离线", StringComparison.Ordinal) || ViewModel.CanUseDevice()) throw new InvalidOperationException("当前设备标题或命令仍显示旧在线状态");
        }
        finally { current.Info = originalInfo; }
        if (ViewModel.Devices.Count > 1)
        {
            var selections = ViewModel.Devices.Select(row => row.IsSelected).ToArray(); var previousText = ViewModel.TextInput;
            try
            {
                foreach (var row in ViewModel.Devices) row.IsSelected = false;
                var other = ViewModel.Devices.First(row => !ReferenceEquals(row, current)); other.IsSelected = true;
                current.Info = originalInfo with { State = "Offline" }; ViewModel.TextInput = "测试"; ViewModel.UpdateSelection();
                if (!ViewModel.CanWriteTargets() || ViewModel.CanWriteDevice() || !ViewModel.TextCommand.CanExecute(null)) throw new InvalidOperationException("批量目标和当前设备的门控作用域混淆");
                NavigateTo("文件"); await Dispatcher.InvokeAsync(UpdateLayout, System.Windows.Threading.DispatcherPriority.Render);
                var download = VisualDescendants<Button>(Shell).Single(button => button.ToolTip as string == "添加下载");
                var upload = VisualDescendants<Button>(Shell).Single(button => button.ToolTip as string == "添加上传");
                if (download.Command!.CanExecute(null) || !upload.Command!.CanExecute(null)) throw new InvalidOperationException("上传/下载控件作用域与执行目标不一致");
            }
            finally { current.Info = originalInfo; ViewModel.TextInput = previousText; for (var i = 0; i < selections.Length; i++) ViewModel.Devices[i].IsSelected = selections[i]; ViewModel.UpdateSelection(); }
        }
        var pending = new TransferRow { Serial = ViewModel.CurrentDevice!.Serial, Direction = "上传", LocalPath = "offline-gate.bin", RemotePath = "/sdcard/MatrixAcceptance/offline-gate.bin" };
        ViewModel.Transfers.Add(pending);
        try
        {
            ViewModel.SetExternalBusy(true);
            try
            {
                if (!ViewModel.Busy || !ViewModel.Runtime.Api.UiWriteBusy || ViewModel.StartTransfersCommand.CanExecute(null) || ViewModel.CanWriteDevice())
                    throw new InvalidOperationException("雷电忙状态没有阻止普通操作或API写入");
            }
            finally { ViewModel.SetExternalBusy(false); }
            var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var busy = ViewModel.ExecuteAsync("忙状态退出与传输检查", () => release.Task);
            try
            {
                if (ViewModel.StartTransfersCommand.CanExecute(null) || !ViewModel.HasActiveOperations)
                    throw new InvalidOperationException("普通忙操作没有阻止传输开始或未纳入退出活动守卫");
            }
            finally { release.SetResult(); await busy; }
            pending.State = "已停止";
            if (!ViewModel.StartTransfersCommand.CanExecute(null)) throw new InvalidOperationException("已停止的文件传输不能重试");
        }
        finally { ViewModel.Transfers.Remove(pending); ViewModel.UpdateSelection(); }
        apiProtocol.SelectedIndex = 2;
        if (apiSend.CanExecute(null)) throw new InvalidOperationException("未启动的接口允许发送请求");
        var serial = ViewModel.CurrentDevice!.Serial;
        await ViewModel.Runtime.Api.StartDeviceAsync(serial);
        try
        {
            apiMethod.SelectedIndex = 1; apiRoute.Text = "/tap"; apiPayload.Text = "{\"X\":20,\"Y\":20}"; apiSend.Refresh();
            await apiSend.ExecuteAsync();
            if (!apiResponse.Text.StartsWith("HTTP 200", StringComparison.Ordinal) || !string.IsNullOrEmpty(ViewModel.Error)) throw new InvalidOperationException("API面板忙状态阻止了自己的写请求");
        }
        finally { await ViewModel.Runtime.Api.StopDeviceAsync(serial); apiMethod.SelectedIndex = 0; apiRoute.Text = "/health"; apiPayload.Text = "{}"; apiResponse.Clear(); apiProtocol.SelectedIndex = 0; }
    }

    private object[] CaptureControlStates(string page, string state, FrameworkElement surface)
    {
        return VisualDescendants<FrameworkElement>(surface)
            .Where(control => control is ButtonBase or TextBox or Selector or RangeBase)
            .Select((control, index) =>
            {
                var property = control switch
                {
                    TextBox => TextBox.TextProperty,
                    ToggleButton => ToggleButton.IsCheckedProperty,
                    Selector => Selector.SelectedItemProperty,
                    RangeBase => RangeBase.ValueProperty,
                    _ => null
                };
                var binding = property is null ? null : BindingOperations.GetBinding(control, property)?.Path?.Path;
                var command = (control as ICommandSource)?.Command;
                return (object)new
                {
                    caseId = $"UI-04-{page}-{index}", page, state, type = control.GetType().Name,
                    label = control.ToolTip as string ?? (control as ContentControl)?.Content as string ?? binding ?? control.Name,
                    binding, control.IsEnabled, control.IsVisible,
                    commandAvailable = command?.CanExecute((control as ICommandSource)?.CommandParameter),
                    result = "OBSERVED-NOT-EXERCISED", mode = "demo"
                };
            }).ToArray();
    }
}
