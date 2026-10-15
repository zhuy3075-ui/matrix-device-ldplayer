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
        var previousAction = instanceActionRow;
        var previousGroup = ViewModel.GroupFilter;
        var previousCurrent = ViewModel.CurrentDevice;
        var previousMain = ViewModel.MainDevice;
        var selection = unifiedDevices.ToDictionary(row => row.Key, row => row.IsSelected);
        var cold = unifiedDevices.Single(row => row.Instance is { Index: > 0, AndroidStarted: false });
        var other = unifiedDevices.First(row => row.Instance is null && row.Device is not null);
        var otherInfo = other.Device!.Info;
        const string group = "离线实例目标测试";
        ViewModel.Groups.Add(group);
        try
        {
            var before = ViewModel.Devices.Select(row => row.IsSelected).ToArray();
            SelectUnifiedInstance(cold);
            if (ldSelection?.Index != cold.Index || !ReferenceEquals(instanceActionChoice.SelectedItem, cold))
                throw new InvalidOperationException("实例操作下拉与实际命令目标不一致。");
            SelectUnifiedInstance(other);
            if (ldSelection is not null || ldCommandsByLabel["资源配置"].CanExecute(null) || ldCommandsByLabel["关闭实例"].CanExecute(null))
                throw new InvalidOperationException("非雷电行仍沿用旧实例目标。");
            if (!ReferenceEquals(previousCurrent, ViewModel.CurrentDevice) || !ReferenceEquals(previousMain, ViewModel.MainDevice)
                || !before.SequenceEqual(ViewModel.Devices.Select(row => row.IsSelected)))
                throw new InvalidOperationException("实例操作目标切换隐式改写了群控选择或主控。");

            foreach (var row in unifiedDevices.ToArray()) row.IsSelected = false;
            cold.IsSelected = true; other.IsSelected = true;
            other.Device.Info = otherInfo with { GroupId = group };
            ViewModel.GroupFilter = group;
            await Dispatcher.InvokeAsync(UpdateLayout, DispatcherPriority.Render);
            if (unifiedDeviceView.Cast<UnifiedDeviceRow>().Contains(cold)) throw new InvalidOperationException("隐藏目标反例没有形成。");
            other.IsSelected = false;
            if (!cold.IsSelected || ldSelection?.Index != cold.Index || !ReferenceEquals(instanceActionChoice.SelectedItem, cold))
                throw new InvalidOperationException("筛选后唯一隐藏勾选目标与实例下拉不一致。");

            ViewModel.GroupFilter = previousGroup;
            other.Device.Info = otherInfo;
            var confirm = ViewModel.Confirm;
            ViewModel.Confirm = (_, _) => true;
            try
            {
                SelectUnifiedInstance(cold);
                await ExecuteLdPlayerAsync("launch");
                var running = ViewModel.Runtime.LdPlayer!.Instances.Single(item => item.Index == cold.Index);
                var projected = unifiedDevices.Single(row => row.Index == cold.Index);
                if (!running.AndroidStarted || projected.Instance != running || projected.AndroidState != "已启动" || projected.ProcessId != running.ProcessId.ToString())
                    throw new InvalidOperationException("模拟命令后统一列表仍显示旧进程或安卓状态。");
                await ExecuteLdPlayerAsync("quit");
                var stopped = ViewModel.Runtime.LdPlayer!.Instances.Single(item => item.Index == cold.Index);
                if (stopped.AndroidStarted || projected.Instance != stopped || projected.ProcessId != "-")
                    throw new InvalidOperationException("模拟关闭后统一列表未即时更新。");
            }
            finally { ViewModel.Confirm = confirm; }
            wallActionsViewport.ScrollToTop();
            await Dispatcher.InvokeAsync(UpdateLayout, DispatcherPriority.Render);
            await Task.Delay(60);
            if (instanceActionChoice.ActualWidth <= 0 || instanceActionChoice.ActualHeight < 32)
                throw new InvalidOperationException("实例生命周期目标控件未布局。");
            if (screenshot is not null) SaveRender(Path.Combine(Path.GetDirectoryName(screenshot)!, "merged-device-actions.png"));
            var groupChoice = VisualDescendants<ComboBox>(Shell).Single(input => input.ToolTip as string == "设备分组");
            var groupTop = groupChoice.TranslatePoint(new Point(), Shell).Y;
            var listTop = wallWorkspace.TranslatePoint(new Point(), Shell).Y;
            var footer = workspace.Children.OfType<FrameworkElement>().Single(child => Grid.GetRow(child) == 3);
            var footerTop = footer.TranslatePoint(new Point(), Shell).Y;
            var visibleListHeight = Math.Min(listTop + wallWorkspace.ActualHeight, footerTop) - Math.Max(listTop, 0);
            if (wallWorkspace.ActualHeight < 140 || visibleListHeight < 140 || listTop + wallWorkspace.ActualHeight > footerTop + 1
                || groupTop < 0 || groupTop + groupChoice.ActualHeight > Shell.ActualHeight)
                throw new InvalidOperationException($"统一页操作区挤占列表或裁切全局工具栏：列表高 {wallWorkspace.ActualHeight:F1}，分组顶 {groupTop:F1}。");
            var targetTop = instanceActionChoice.TranslatePoint(new Point(), wallActionsViewport).Y;
            if (targetTop < 0 || targetTop + instanceActionChoice.ActualHeight > wallActionsViewport.ActualHeight)
                throw new InvalidOperationException("实例操作目标超出可见操作区。");
        }
        finally
        {
            wallActionsViewport.ScrollToTop();
            other.Device.Info = otherInfo;
            ViewModel.GroupFilter = previousGroup; ViewModel.Groups.Remove(group);
            foreach (var row in unifiedDevices.ToArray()) row.IsSelected = selection.GetValueOrDefault(row.Key);
            SelectUnifiedInstance(previousAction);
        }
    }
}
