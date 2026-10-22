using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using MatrixDevice.Core;
using MatrixDevice.Features;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    internal async Task VerifyApplicationManagementAsync(string? screenshotPath)
    {
        if (!ViewModel.Demo) throw new InvalidOperationException("应用管理离线验收仅限演示模式。");
        var current = ViewModel.CurrentDevice; var main = ViewModel.MainDevice;
        var controlSelection = ViewModel.Devices.Select(row => (row.Serial, row.IsSelected)).ToArray();
        var controlTargets = ViewModel.Targets().Select(row => row.Serial).ToArray();
        NavigateTo("应用"); SyncApplicationTargets();
        if (applicationTargets.Count != 2 || applicationTargets.Any(row => row.IsSelected) || CanManageApplicationTargets()) throw new InvalidOperationException("应用管理缺少独立空选择，或回落到了群控目标。");
        foreach (var target in applicationTargets) target.IsSelected = true;
        applicationReadTimer?.Stop(); applicationReadPending = false;
        await ReadApplicationListsAsync();
        if (applicationTasks.Count != 2 || applicationTasks.Any(row => row.ResultState != ApplicationTaskState.Succeeded)
            || applicationRows.Count != 4 || applicationRowsView.Cast<object>().Count() != 2) throw new InvalidOperationException("未按两台明确目标独立读取应用或系统过滤失效。");
        if (!ReferenceEquals(current, ViewModel.CurrentDevice) || !ReferenceEquals(main, ViewModel.MainDevice)
            || !controlSelection.SequenceEqual(ViewModel.Devices.Select(row => (row.Serial, row.IsSelected)))
            || !controlTargets.SequenceEqual(ViewModel.Targets().Select(row => row.Serial))) throw new InvalidOperationException("应用目标选择改写了群控上下文。");
        var selected = applicationRows.First(row => row.Serial == applicationTargets[1].Serial && !row.App.IsSystem); selected.IsSelected = true;
        var requested = ""; applicationConfirmationFixture = (_, detail) => { requested = detail; return false; };
        try
        {
            await ChangeSelectedApplicationsAsync(ApplicationOperation.Uninstall);
            if (!requested.Contains(selected.Serial, StringComparison.Ordinal) || !requested.Contains(selected.PackageName, StringComparison.Ordinal)
                || requested.Contains(applicationTargets[0].Serial, StringComparison.Ordinal) || !applicationResult.Text.Contains("未操作任何", StringComparison.Ordinal))
                throw new InvalidOperationException("卸载确认未列精确应用行，或取消确认后仍执行。");
            if (!(await ViewModel.Runtime.Backend.ListAppsAsync(selected.Serial, true)).Any(app => app.PackageName == selected.PackageName)) throw new InvalidOperationException("取消确认仍卸载了应用。");
        }
        finally { applicationConfirmationFixture = null; }
        foreach (var operation in new[] { ApplicationOperation.Start, ApplicationOperation.Stop })
        {
            requested = "";
            applicationConfirmationFixture = (_, detail) => { requested = detail; return false; };
            try
            {
                var tasksBefore = applicationTasks.ToArray();
                var resultBefore = applicationResult.Text;
                var plan = ApplicationBatch.RowPlan(operation, [new(selected.Target, operation, selected.PackageName)]);
                if (ConfirmApplicationPlan(plan)) throw new InvalidOperationException("文案确认用例未取消。");
                if (!requested.Contains(selected.Serial, StringComparison.Ordinal) || !requested.Contains(selected.PackageName, StringComparison.Ordinal)
                    || requested.Contains("覆盖安装", StringComparison.Ordinal) || requested.Contains("删除对应应用数据", StringComparison.Ordinal)
                    || !requested.Contains(operation == ApplicationOperation.Start ? "启动命令" : "强制停止", StringComparison.Ordinal)
                    || !tasksBefore.SequenceEqual(applicationTasks) || applicationResult.Text != resultBefore)
                    throw new InvalidOperationException("公共启停文案不准确，或文案生成意外改变任务。");
            }
            finally { applicationConfirmationFixture = null; }
        }
        applicationConfirmationFixture = (_, _) => { applicationTargets[1].IsSelected = false; return true; };
        try
        {
            try { await ChangeSelectedApplicationsAsync(ApplicationOperation.Uninstall); throw new InvalidOperationException("确认期间目标变化仍执行了旧任务。"); }
            catch (DeviceException) { }
            if (!(await ViewModel.Runtime.Backend.ListAppsAsync(selected.Serial, true)).Any(app => app.PackageName == selected.PackageName)) throw new InvalidOperationException("确认期间改变目标仍发送了卸载命令。");
        }
        finally { applicationConfirmationFixture = null; applicationTargets[1].IsSelected = true; applicationReadTimer?.Stop(); applicationReadPending = false; }
        ViewModel.SetExternalBusy(true);
        try { if (CanManageApplicationTargets() || CanManageApplicationRows()) throw new InvalidOperationException("外部繁忙状态仍允许应用任务。"); }
        finally { ViewModel.SetExternalBusy(false); }
        var device = ViewModel.Devices.Single(row => row.Serial == selected.Serial); var saved = device.Info;
        try
        {
            device.Info = saved with { Generation = saved.Generation + 1 };
            try { RequireApplicationContext(ViewModel.Runtime, selected.Target); throw new InvalidOperationException("旧代次目标未拒绝。"); } catch (DeviceException) { }
            device.Info = saved with { TransportId = (saved.TransportId ?? 0) + 1 };
            try { RequireApplicationContext(ViewModel.Runtime, selected.Target); throw new InvalidOperationException("旧传输目标未拒绝。"); } catch (DeviceException) { }
            try { RequireApplicationContext(null!, selected.Target); throw new InvalidOperationException("旧运行时未拒绝。"); } catch (DeviceException) { }
        }
        finally { device.Info = saved; }
        applicationReadTimer?.Stop(); applicationReadPending = false; await ReadApplicationListsAsync();
        await Dispatcher.InvokeAsync(UpdateLayout, DispatcherPriority.Render);
        await Task.Delay(60);
        await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
        if (applicationPage.ActualWidth <= 0 || applicationTargetPanel.ActualWidth < 180) throw new InvalidOperationException("应用管理布局为空或目标栏被挤压。");
        var settings = VisualDescendants<Button>(Shell).Single(input => input.Name == "WorkspaceSettings");
        var settingsTop = settings.TranslatePoint(new Point(), Shell).Y;
        if (settingsTop < 0 || settingsTop + settings.ActualHeight > Shell.ActualHeight) throw new InvalidOperationException("应用页全局工具栏超出客户区。");
        if (screenshotPath is not null) SaveRender(Path.Combine(Path.GetDirectoryName(screenshotPath)!, "applications-management.png"));
        foreach (var target in applicationTargets) target.IsSelected = false;
        applicationReadTimer?.Stop(); applicationReadPending = false;
        NavigateTo("设备墙 / 主控");
    }
}
