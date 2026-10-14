using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    private async Task VerifyOneClickProjectionAsync()
    {
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
            var all = VisualDescendants<Button>(Shell).Single(button => button.ToolTip as string == "选择当前筛选中的全部设备");
            all.Command!.Execute(null);
            if (unifiedDevices.Any(row => row.CanSelect && !row.IsSelected) || ViewModel.Devices.Count != originalDevices) throw new InvalidOperationException("全选未涵盖关闭实例或伪造了可控设备。");
            UpdateUnifiedDirectory();
            if (unifiedDevices.Any(row => row.CanSelect && !row.IsSelected)) throw new InvalidOperationException("自动目录更新丢失选择。");
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
