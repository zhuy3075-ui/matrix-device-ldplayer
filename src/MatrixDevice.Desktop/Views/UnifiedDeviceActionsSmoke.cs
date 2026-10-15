using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    private async Task VerifyUnifiedDeviceActionsAsync(string? screenshot)
    {
        if (!ViewModel.Demo) throw new InvalidOperationException("统一实例操作验收仅限演示模式。");
        NavigateTo("设备墙 / 主控"); SetWallViewMode("list"); UpdateUnifiedDirectory();
        await Dispatcher.InvokeAsync(UpdateLayout, DispatcherPriority.Render);
        var previousStatus = ViewModel.StatusFilter;
        var previousCurrent = ViewModel.CurrentDevice;
        var previousMain = ViewModel.MainDevice;
        var selection = unifiedDevices.ToDictionary(row => row.Key, row => row.IsSelected);
        var cold = unifiedDevices.Single(row => row.Instance is { Index: > 0, AndroidStarted: false });
        var other = unifiedDevices.First(row => row.Instance is null && row.Device is not null);
        var otherInfo = other.Device!.Info;
        var more = VisualDescendants<Button>(Shell).Single(button => button.ToolTip as string == "更多操作");
        var menu = more.ContextMenu ?? throw new InvalidOperationException("更多操作按钮没有实际菜单。");
        var menuActions = new[] { "资源配置", "关闭实例", "重启实例" }.ToDictionary(label => label,
            label => menu.Items.OfType<MenuItem>().Single(item => item.Header as string == label));
        if (menuActions.Any(pair => !ReferenceEquals(pair.Value.Command, ldCommandsByLabel[pair.Key])))
            throw new InvalidOperationException("更多菜单条目没有绑定实际生命周期命令。");
        async Task CheckMenuAsync(bool configuration, bool running)
        {
            menu.IsOpen = false;
            await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
            more.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
            var opened = System.Diagnostics.Stopwatch.StartNew();
            while (true)
            {
                await Dispatcher.InvokeAsync(() => { UpdateLayout(); menu.UpdateLayout(); }, DispatcherPriority.ApplicationIdle);
                if (menu.IsOpen && menu.IsLoaded && menu.IsVisible && menu.ActualWidth > 0 && menu.ActualHeight > 0) break;
                if (opened.Elapsed >= TimeSpan.FromSeconds(1))
                    throw new InvalidOperationException($"更多菜单打开超时：open={menu.IsOpen}, loaded={menu.IsLoaded}, visible={menu.IsVisible}, size={menu.ActualWidth}x{menu.ActualHeight}, active={IsActive}。");
                await Task.Delay(20);
            }
            foreach (var pair in menuActions)
            {
                var expected = pair.Key == "资源配置" ? configuration : running;
                if (pair.Value.Command!.CanExecute(null) != expected || pair.Value.IsEnabled != expected)
                    throw new InvalidOperationException("更多菜单的可用状态与实际实例目标不一致：" + pair.Key);
            }
        }
        try
        {
            ClearWallSelection(); await CheckMenuAsync(false, false);
            cold.IsSelected = true; await CheckMenuAsync(true, false);
            if (ldSelection is not { } coldSelection || coldSelection.Index != cold.Index || coldSelection.Generation != cold.Instance?.Generation)
                throw new InvalidOperationException("统一目录单选与实际实例命令目标不一致。");
            cold.IsSelected = false; other.IsSelected = true;
            if (ldSelection is not null || ldCommandsByLabel["资源配置"].CanExecute(null) || ldCommandsByLabel["关闭实例"].CanExecute(null))
                throw new InvalidOperationException("非雷电行仍沿用旧实例目标。");
            if (!ReferenceEquals(previousCurrent, ViewModel.CurrentDevice) || !ReferenceEquals(previousMain, ViewModel.MainDevice)
                || !other.Device.IsSelected)
                throw new InvalidOperationException("目录勾选改写当前设备、主控或没有同步对应群控目标。");

            cold.IsSelected = true;
            await CheckMenuAsync(false, false);
            if (ldSelection is not null || ldCommandsByLabel["资源配置"].CanExecute(null))
                throw new InvalidOperationException("多选目录仍保留单实例命令目标。");
            other.Device.Info = otherInfo with { State = "Ready" };
            ViewModel.StatusFilter = "Ready";
            await Dispatcher.InvokeAsync(UpdateLayout, DispatcherPriority.Render);
            if (unifiedDeviceView.Cast<UnifiedDeviceRow>().Contains(cold)) throw new InvalidOperationException("隐藏目标反例没有形成。");
            other.IsSelected = false;
            if (!cold.IsSelected || ldSelection is not { } hiddenSelection || hiddenSelection.Index != cold.Index || hiddenSelection.Generation != cold.Instance?.Generation)
                throw new InvalidOperationException("状态筛选后的唯一隐藏勾选目标与实例命令快照不一致。");
            var selectAll = VisualDescendants<CheckBox>(Shell).Single(input => input.Name == "SelectAllDevices");
            selectAll.IsChecked = false; selectAll.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
            if (unifiedDevices.Any(row => row.IsSelected) || ldSelection is not null)
                throw new InvalidOperationException("表头取消选择没有清除隐藏目标或实例命令快照。");

            var beforeEmptyInfos = ViewModel.Devices.ToDictionary(row => row, row => row.Info);
            var beforeEmptyStatus = ViewModel.StatusFilter;
            try
            {
                ClearWallSelection(); cold.IsSelected = true;
                foreach (var row in ViewModel.Devices) row.Info = row.Info with { State = "Ready" };
                ViewModel.StatusFilter = "Offline";
                await Dispatcher.InvokeAsync(UpdateLayout, DispatcherPriority.Render);
                if (!unifiedDeviceView.IsEmpty || !cold.IsSelected || selectAll.IsChecked is not null || !selectAll.IsEnabled)
                    throw new InvalidOperationException("空可见列表的隐藏勾选未显示为可取消的表头中间态。");
                selectAll.IsChecked = false; selectAll.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
                if (unifiedDevices.Any(row => row.IsSelected) || ViewModel.Devices.Any(row => row.IsSelected) || ldSelection is not null)
                    throw new InvalidOperationException("空可见列表中的表头取消未清除隐藏目标或实例命令快照。");
            }
            finally
            {
                foreach (var pair in beforeEmptyInfos) pair.Key.Info = pair.Value;
                ViewModel.StatusFilter = beforeEmptyStatus;
            }

            ViewModel.StatusFilter = previousStatus;
            other.Device.Info = otherInfo;
            var confirm = ViewModel.Confirm;
            ViewModel.Confirm = (_, _) => true;
            try
            {
                cold.IsSelected = true;
                await ExecuteLdPlayerAsync("launch");
                var running = ViewModel.Runtime.LdPlayer!.Instances.Single(item => item.Index == cold.Index);
                var projected = unifiedDevices.Single(row => row.Index == cold.Index);
                if (!running.AndroidStarted || projected.Instance != running || projected.AndroidState != "已启动" || projected.ProcessId != running.ProcessId.ToString())
                    throw new InvalidOperationException("模拟命令后统一列表仍显示旧进程或安卓状态。");
                await CheckMenuAsync(false, true);
                if (screenshot is not null) SaveRender(Path.Combine(Path.GetDirectoryName(screenshot)!, "instance-actions-menu.png"), menu);
                await ExecuteLdPlayerAsync("quit");
                var stopped = ViewModel.Runtime.LdPlayer!.Instances.Single(item => item.Index == cold.Index);
                if (stopped.AndroidStarted || projected.Instance != stopped || projected.ProcessId != "-")
                    throw new InvalidOperationException("模拟关闭后统一列表未即时更新。");
            }
            finally { ViewModel.Confirm = confirm; }
            wallActionsViewport.ScrollToTop();
            await Dispatcher.InvokeAsync(UpdateLayout, DispatcherPriority.Render);
            await Task.Delay(60);
            if (screenshot is not null) SaveRender(Path.Combine(Path.GetDirectoryName(screenshot)!, "merged-device-actions.png"));
            var settings = VisualDescendants<Button>(Shell).Single(input => input.Name == "WorkspaceSettings");
            var settingsTop = settings.TranslatePoint(new Point(), Shell).Y;
            var listTop = wallWorkspace.TranslatePoint(new Point(), Shell).Y;
            var footer = workspace.Children.OfType<FrameworkElement>().Single(child => Grid.GetRow(child) == 3);
            var footerTop = footer.TranslatePoint(new Point(), Shell).Y;
            var visibleListHeight = Math.Min(listTop + wallWorkspace.ActualHeight, footerTop) - Math.Max(listTop, 0);
            if (wallWorkspace.ActualHeight < 140 || visibleListHeight < 140 || listTop + wallWorkspace.ActualHeight > footerTop + 1
                || settingsTop < 0 || settingsTop + settings.ActualHeight > Shell.ActualHeight)
                throw new InvalidOperationException($"统一页操作区挤占列表或裁切全局工具栏：列表高 {wallWorkspace.ActualHeight:F1}，设置顶 {settingsTop:F1}。");
            var create = VisualDescendants<Button>(Shell).Single(button => button.ToolTip as string == "新建空白实例");
            var actionTop = create.TranslatePoint(new Point(), wallActionsViewport).Y;
            if (actionTop < 0 || actionTop + create.ActualHeight > wallActionsViewport.ActualHeight)
                throw new InvalidOperationException("实例生命周期操作超出可见操作区。");
        }
        finally
        {
            menu.IsOpen = false;
            wallActionsViewport.ScrollToTop();
            other.Device.Info = otherInfo;
            ViewModel.StatusFilter = previousStatus;
            foreach (var row in unifiedDevices.ToArray()) row.IsSelected = selection.GetValueOrDefault(row.Key);
        }
    }
}
