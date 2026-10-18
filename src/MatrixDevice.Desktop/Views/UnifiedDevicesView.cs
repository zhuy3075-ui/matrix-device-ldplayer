using System.Runtime.InteropServices;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using MatrixDevice.Core;
using MatrixDevice.Host;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    private readonly CancellationTokenSource directoryCancellation = new();
    private Task? directoryOperation;
    private CancellationTokenSource? ldOperationCancellation;
    private readonly List<TextBlock> deviceSummaries = [];
    private readonly ObservableCollection<UnifiedDeviceRow> unifiedDevices = [];
    private ICollectionView unifiedDeviceView = null!;
    private static readonly string[] DeviceStatusOptions = ["全部状态", "已连接", "已关闭", "启动中", "待连接", "离线", "未授权", "已发现", "异常", "未知", "受保护"];
    private string deviceStatusFilter = "全部状态";
    private ComboBox deviceStatusChoice = null!;
    private bool refreshingUnifiedFilter;
    private readonly Dictionary<int, string> launchPhases = [];
    private readonly List<TextBlock> launchFeedback = [];
    private readonly List<AsyncCommand> launchCommands = [];
    private readonly List<ActionCommand> launchCancelCommands = [];
    private Button? launchCancelButton;
    private bool updatingUnifiedDirectory;
    private readonly HashSet<int> coldLaunched = [];
    private LdWindowVisibility? ldWindowVisibility;
    private DateTimeOffset lastGuestDiscovery;

    private FrameworkElement BuildDeviceSummary()
    {
        var summary = new TextBlock { Foreground = WallMuted, Margin = new Thickness(0, 0, 0, 10), TextWrapping = TextWrapping.Wrap };
        deviceSummaries.Add(summary);
        return summary;
    }

    private void InitializeUnifiedDevices()
    {
        unifiedDeviceView = new ListCollectionView(unifiedDevices);
        unifiedDeviceView.Filter = item => item is UnifiedDeviceRow row
            && (deviceStatusFilter == "全部状态" || row.FilterState == deviceStatusFilter);
        ViewModel.PropertyChanged += (_, args) => { if (args.PropertyName is nameof(WorkspaceViewModel.Counts)) UpdateUnifiedDirectory(); RefreshLaunchCommands(); };
    }

    private ComboBox BuildDeviceStatusFilter()
    {
        var choice = new ComboBox { Name = "DeviceStatusFilter", ItemsSource = DeviceStatusOptions, SelectedItem = deviceStatusFilter, Width = 145, MinHeight = 34, Margin = new Thickness(12, 0, 0, 10) };
        System.Windows.Automation.AutomationProperties.SetName(choice, "设备状态筛选"); System.Windows.Automation.AutomationProperties.SetAutomationId(choice, choice.Name);
        deviceStatusChoice = choice;
        choice.SelectionChanged += (_, _) => { deviceStatusFilter = (string)choice.SelectedItem; RefreshUnifiedFilter(); };
        return choice;
    }

    private void RefreshUnifiedFilter()
    {
        refreshingUnifiedFilter = true;
        try { unifiedDeviceView.Refresh(); }
        finally { refreshingUnifiedFilter = false; }
        SynchronizeUnifiedSelection();
    }

    internal void UpdateUnifiedDirectory()
    {
        if (updatingUnifiedDirectory || unifiedDeviceView is null) return;
        updatingUnifiedDirectory = true;
        try
        {
        var instances = ViewModel.Runtime.LdPlayer?.Instances ?? [];
        var admitted = instances.Count(instance => ViewModel.Runtime.Settings.LdSerialBindings.ContainsKey(instance.Index));
        var expected = new HashSet<string>(StringComparer.Ordinal);
        var boundSerials = new HashSet<string>(StringComparer.Ordinal);
        foreach (var instance in instances)
        {
            ViewModel.Runtime.Settings.LdSerialBindings.TryGetValue(instance.Index, out var serial);
            var device = ViewModel.Devices.FirstOrDefault(row => row.Serial == serial);
            if (device is not null) boundSerials.Add(device.Serial);
            Upsert("ld:" + instance.Index, instance, device, LdInstanceConnected(instance.Index));
        }
        foreach (var device in ViewModel.Devices.Where(row => !boundSerials.Contains(row.Serial))) Upsert("device:" + device.Serial, null, device, device.Info.Online);
        foreach (var stale in unifiedDevices.Where(row => !expected.Contains(row.Key)).ToArray()) unifiedDevices.Remove(stale);
        foreach (var summary in deviceSummaries) summary.Text = $"本机 {instances.Count} 台 · 已接入 {admitted} 台 · 已连接 {unifiedDevices.Count(row => row.Connected)} 台";
        RefreshUnifiedFilter();
        void Upsert(string key, LdPlayerInstance? instance, DeviceRow? device, bool connected)
        {
            expected.Add(key);
            var row = unifiedDevices.FirstOrDefault(item => item.Key == key);
            if (row is null)
            {
                row = new UnifiedDeviceRow(key, ActivateUnifiedRowAsync, CanActivateUnifiedRow, ShowUnifiedWindowAsync, item => !ldBusy && !ViewModel.Busy && item.Index is { } index && ldWindowVisibility?.IsHidden(index) == true, item => item.Device is null ? Task.CompletedTask : OpenMainControlAsync(item.Device));
                row.PropertyChanged += (_, args) => { if (args.PropertyName == nameof(UnifiedDeviceRow.IsSelected)) SynchronizeUnifiedSelection(); };
                unifiedDevices.Add(row);
            }
            row.Update(instance, device, connected, instance is not null && launchPhases.TryGetValue(instance.Index, out var phase) ? phase : "",
                instance is null ? null : ViewModel.Runtime.Settings.LdSerialBindings.GetValueOrDefault(instance.Index));
        }
        }
        finally { updatingUnifiedDirectory = false; }
    }

    private bool CanStartInstances() => !ldBusy && !captureInProgress && !ViewModel.HasActiveOperations && ViewModel.ConnectionReloadAvailable && scanOperation is not { IsCompleted: false };
    private bool CanActivateUnifiedRow(UnifiedDeviceRow row) => row.Connected ? !ViewModel.Busy : row.Index is > 0 and < 99999 && !ViewModel.Demo && CanStartInstances();
    private async Task ActivateUnifiedRowAsync(UnifiedDeviceRow row)
    {
        try
        {
            if (row.Connected && row.Device is not null) { ViewModel.CurrentDevice = row.Device; NavigateTo("节点"); }
            else if (row.Index is { } index) await ConnectLdInstancesAsync([index]);
        }
        catch (Exception ex) { SetLaunchFeedback("操作失败：" + ViewModel.PresentError(ex)); }
    }
    private async Task ShowUnifiedWindowAsync(UnifiedDeviceRow row)
    {
        try { if (row.Index is { } index && ldWindowVisibility is not null) await ldWindowVisibility.ShowAsync(index); UpdateUnifiedDirectory(); }
        catch (Exception ex) { SetLaunchFeedback("显示窗口失败：" + ViewModel.PresentError(ex)); }
    }
    private void SetLaunchFeedback(string text) { if (ldFeedback is not null) ldFeedback.Text = text; foreach (var feedback in launchFeedback) feedback.Text = text; }
    private void SynchronizeUnifiedSelection()
    {
        var selected = unifiedDevices.Where(row => row.IsSelected).Take(2).ToArray();
        SelectUnifiedInstance(selected.Length == 1 ? selected[0] : null);
        ViewModel.SetPendingLaunchSelection(unifiedDevices.Count(row => row.IsSelected && !row.Connected));
        workspaceCounts.Text = $"目录 {unifiedDevices.Count} · 已连接 {unifiedDevices.Count(row => row.Connected)} · 筛选 {unifiedDeviceView.Cast<UnifiedDeviceRow>().Count()} · 已选 {unifiedDevices.Count(row => row.IsSelected)}";
        UpdateWallSelectionToggle();
        RefreshLaunchCommands();
    }
    private void RefreshLaunchCommands()
    {
        foreach (var command in launchCommands) command.Refresh();
        foreach (var command in launchCancelCommands) command.Refresh();
        foreach (var row in unifiedDevices) row.RefreshCommands();
        if (launchCancelButton is not null) launchCancelButton.Visibility = ldOperationCancellation is not null ? Visibility.Visible : Visibility.Collapsed;
    }

    private FrameworkElement BuildOneClickLaunchTools()
    {
        var section = new StackPanel { Margin = new Thickness(0, 0, 0, 8) };
        var feedback = new TextBlock { Foreground = WallMuted, TextWrapping = TextWrapping.Wrap, MinHeight = 19, Margin = new Thickness(0, 0, 0, 4) }; ldFeedback = feedback; launchFeedback.Add(feedback);
        var tools = Toolbar();
        tools.Children.Add(LaunchButton("\uE768", "启动所选", () => ConnectLdInstancesAsync(unifiedDevices.Where(row => row.IsSelected && !row.Connected && row.Index is > 0 and < 99999).Select(row => row.Index!.Value).ToArray()), () => !ViewModel.Demo && CanStartInstances() && unifiedDevices.Any(row => row.IsSelected && !row.Connected && row.Index is > 0 and < 99999), true));
        tools.Children.Add(LaunchButton("\uE768", "一键启动全部", StartAllLdInstancesAsync, () => !ViewModel.Demo && CanStartInstances() && unifiedDevices.Any(row => !row.Connected && row.Index is > 0 and < 99999)));
        tools.Children.Add(BuildInstanceActions());
        var stop = new ActionCommand(() => ldOperationCancellation?.Cancel(), () => ldOperationCancellation is not null);
        launchCancelCommands.Add(stop);
        var stopButton = WallButton("\uE71A", "取消启动", stop); stopButton.Visibility = Visibility.Collapsed; stopButton.Content = WallGlyph("\uE71A"); stopButton.Width = 34; stopButton.MinWidth = 0; stopButton.Padding = new Thickness(0); launchCancelButton = stopButton; tools.Children.Add(stopButton);
        ViewModel.PropertyChanged += (_, _) => stop.Refresh();
        section.Children.Add(tools);
        section.Children.Add(feedback);
        return section;
    }
    private Button LaunchButton(string glyph, string label, Func<Task> operation, Func<bool> enabled, bool primary = false)
    {
        var command = new AsyncCommand(async () => { try { await operation(); } catch (Exception ex) { SetLaunchFeedback(label + "失败：" + ViewModel.PresentError(ex)); } }, enabled);
        launchCommands.Add(command); var button = WallButton(glyph, label, command, primary);
        button.ToolTip = label switch
        {
            "启动所选" => "启动所选：操作列表中已勾选且尚未连接的可接入实例，包括被当前筛选隐藏的已勾选实例；已运行的实例仅连接，不重复启动；不包含受保护实例。完成核验后自动连接并加入群控。",
            "一键启动全部" => "一键启动全部：操作全部尚未连接的可接入实例，包括被当前筛选隐藏的实例；已运行的实例仅连接，不重复启动；不包含受保护实例。完成核验后自动连接并加入群控。",
            _ => label
        };
        ToolTipService.SetShowOnDisabled(button, true);
        return button;
    }

    private bool LdInstanceConnected(int index) => ViewModel.Runtime.Settings.LdSerialBindings.TryGetValue(index, out var serial)
        && ViewModel.Runtime.Api.LdSerialForWriteGuard?.Invoke(index) == serial
        && ViewModel.Devices.Any(row => row.Serial == serial && row.Info.Online);

    private string InstanceConnectionState(LdPlayerInstance instance) => instance.Index == 0 && !ViewModel.Demo ? "受保护" : LdInstanceConnected(instance.Index) ? "已连接"
        : instance.IsStarting || (!instance.AndroidStarted && (instance.ProcessId > 0 || instance.VBoxProcessId > 0)) ? "启动中"
        : !instance.AndroidStarted ? "已关闭" : ViewModel.Runtime.Settings.LdSerialBindings.ContainsKey(instance.Index) ? "需重新接入" : "待接入";

    private Task UpdateDeviceDirectoryAsync()
    {
        if (ViewModel.Demo || closing || directoryCancellation.IsCancellationRequested || ldBusy || captureInProgress || ViewModel.HasActiveOperations || directoryOperation is { IsCompleted: false }) return Task.CompletedTask;
        directoryOperation = RefreshDirectoryCoreAsync();
        return directoryOperation;
    }

    private async Task RefreshDirectoryCoreAsync()
    {
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(directoryCancellation.Token);
        deadline.CancelAfter(TimeSpan.FromSeconds(20));
        try
        {
            if (ViewModel.Runtime.LdPlayer is { } service)
            {
                var hadInstances = service.Instances.Count > 0;
                await ViewModel.Runtime.RefreshLdPlayerAsync(deadline.Token);
                await ViewModel.ApplyCachedDeviceSnapshotAsync();
                if (hadInstances && service.Instances.Count == 0) SetLaunchFeedback("本机实例已清空，设备目录已更新。");
            }
            UpdateLdPlayerRows();
            if (DateTimeOffset.UtcNow - lastGuestDiscovery >= TimeSpan.FromSeconds(15))
            {
                lastGuestDiscovery = DateTimeOffset.UtcNow;
                await ViewModel.RefreshDeviceSnapshotAsync(deadline.Token);
            }
            if (ldFeedback.Text.StartsWith("自动更新失败：", StringComparison.Ordinal)) ldFeedback.Text = "";
            UpdateUnifiedDirectory();
        }
        catch (OperationCanceledException) when (directoryCancellation.IsCancellationRequested) { }
        catch (Exception ex)
        {
            ViewModel.Runtime.Log.Write("directory-refresh-error", ex.ToString());
            SetLaunchFeedback("自动更新失败：" + ViewModel.PresentError(ex));
        }
    }

    private async Task RunLdWorkspaceOperationAsync(string title, Func<CancellationToken, Task> operation)
    {
        if (ldBusy || captureInProgress || ViewModel.HasActiveOperations || scanOperation is { IsCompleted: false }) throw new DeviceException("请等待当前操作或网络扫描结束。", 409, 1003);
        if (!ViewModel.ConnectionReloadAvailable) throw new DeviceException("请先停止接口服务、脚本和文件传输。", 409, 1003);
        if (ViewModel.Demo) throw new DeviceException("演示模式不会操作真实模拟器。", 409, 1003);
        ViewModel.Runtime.RequireFrozenConnectionSettings();
        using var write = ViewModel.Runtime.Writes.AcquireAll();
        ldBusy = true; ViewModel.SetExternalBusy(true); RefreshLdButtons();
        await StopMonitorAsync();
        using var cancellation = new CancellationTokenSource(); ldOperationCancellation = cancellation;
        coldLaunched.Clear(); RefreshLaunchCommands();
        var complete = false;
        try
        {
            if (directoryOperation is not null) await directoryOperation;
            SetLaunchFeedback(title);
            await operation(cancellation.Token);
            complete = true;
            ViewModel.Runtime.Log.Write("ld-workspace-complete", title);
        }
        catch (OperationCanceledException) { SetLaunchFeedback("已停止等待及后续任务，已启动的实例保持运行。"); ViewModel.Runtime.Log.Write("ld-workspace-cancel", title); }
        catch (Exception ex) { SetLaunchFeedback(title + "失败：" + ViewModel.PresentError(ex)); ViewModel.Runtime.Log.Write("ld-workspace-error", ex.ToString()); }
        finally
        {
            if (!complete)
            {
                foreach (var index in launchPhases.Keys.ToArray())
                    if (launchPhases[index] is not ("已连接" or "启动失败")) launchPhases[index] = cancellation.IsCancellationRequested ? "已取消" : "未完成";
            }
            ldOperationCancellation = null; ldBusy = false; ViewModel.SetExternalBusy(false);
            UpdateLdPlayerRows(); UpdateUnifiedDirectory(); RefreshLdButtons();
            MonitorNavigationChanged();
        }
    }

    private sealed class InlineLaunchProgress(MainWindow owner) : IProgress<LdLaunchProgress>
    {
        public void Report(LdLaunchProgress value) => owner.Dispatcher.Invoke(() => owner.OnLaunchProgress(value));
    }
    private IProgress<LdLaunchProgress> LaunchProgress() => new InlineLaunchProgress(this);
    private void OnLaunchProgress(LdLaunchProgress progress)
    {
        var phase = progress.State switch { "checking-resources" => "检查资源", "starting" => "正在启动", "waiting" => "等待安卓就绪", "ready" or "skipped" => "等待连接", "cancelled" => "已取消", _ => "启动失败" };
        launchPhases[progress.Index] = phase;
        if (progress.State == "starting") coldLaunched.Add(progress.Index);
        SetLaunchFeedback($"实例 {progress.Index}：{phase}");
        ViewModel.Runtime.Log.Write("ld-launch-" + progress.State, $"index={progress.Index}; {progress.Message}");
        UpdateLdPlayerRows(); UpdateUnifiedDirectory();
    }

    internal async Task RecoverBackgroundWindowsAsync()
    {
        if (ViewModel.Demo || ViewModel.Runtime.LdPlayer is null || string.IsNullOrWhiteSpace(ViewModel.Runtime.FrozenConnectionSettings.LdConsolePath)) return;
        ldWindowVisibility ??= new LdWindowVisibility(System.IO.Path.Combine(ViewModel.DataDirectory, "window-recovery"), System.IO.Path.GetDirectoryName(ViewModel.Runtime.FrozenConnectionSettings.LdConsolePath)!);
        var errors = await ldWindowVisibility.RecoverPendingAsync();
        if (errors.Count > 0) { SetLaunchFeedback("部分后台窗口恢复失败，请检查日志。"); foreach (var error in errors) ViewModel.Runtime.Log.Write("ld-window-recovery-error", error); }
    }

    private async Task StartAllLdInstancesAsync()
    {
        var confirmedInstances = ViewModel.Runtime.LdPlayer?.Instances.Where(item => item.Index > 0 && item.Index != 99999 && !LdInstanceConnected(item.Index)).ToArray() ?? [];
        var indices = confirmedInstances.Select(item => item.Index).ToArray();
        if (indices.Length == 0) throw new DeviceException("没有可启动的独立实例。", 404, 1002);
        await ConnectLdInstancesAsync(indices);
    }

    private async Task ConnectLdInstancesAsync(int[] indices, IReadOnlyDictionary<int, string>? expectedSerials = null)
    {
        if (indices.Length == 0) throw new DeviceException("没有待接入的已运行实例。", 404, 1002);
        if (indices.Any(index => index <= 0 || index == 99999)) throw new DeviceException("实例0受保护，不能自动接入。", 403, 1003);
        var approvedSerials = expectedSerials is null ? null : new Dictionary<int, string>(expectedSerials);
        if (approvedSerials is not null && (approvedSerials.Count != indices.Length || indices.Any(index => !approvedSerials.TryGetValue(index, out var serial) || string.IsNullOrWhiteSpace(serial))))
            throw new DeviceException("验收预批准串号必须与目标实例逐一对应。", 400, 1007);
        var service = ViewModel.Runtime.LdPlayer ?? throw new DeviceException("雷电服务未配置。", 503, 2001);
        var confirmedInstances = indices.Select(index => service.Instances.Single(item => item.Index == index)).ToArray();
        var expectedBirths = confirmedInstances.Where(item => item.VBoxProcessId > 0).ToDictionary(item => item.Index, VBoxBirth);
        await RunLdWorkspaceOperationAsync("接入雷电设备", async ct =>
        {
            foreach (var index in indices) launchPhases[index] = "排队中";
            var started = await new LdLaunchCoordinator(service).StartAsync(indices, EnsureLaunchResourcesAsync, LaunchProgress(), ct, expectedInstances: confirmedInstances);
            var expected = started.Select(result => result.Instance ?? throw new DeviceException("启动结果缺少实例快照。", 409, 1003)).ToArray();
            foreach (var instance in expected)
                if (!expectedBirths.ContainsKey(instance.Index)) expectedBirths.Add(instance.Index, VBoxBirth(instance));
            using var deadline = CancellationTokenSource.CreateLinkedTokenSource(ct); deadline.CancelAfter(TimeSpan.FromSeconds(90));
            IReadOnlyList<LdConnectionProof> proofs;
            while (true)
            {
                try { proofs = await LdConnectionAdmission.ResolveAsync((AdbDeviceBackend)ViewModel.Runtime.Backend, service, indices, expected, expectedBirths, deadline.Token); break; }
                catch (DeviceException ex) when (ex.Status == 409 && ex.Code == 2002)
                { SetLaunchFeedback("等待雷电注册ADB连接…"); await Task.Delay(1500, deadline.Token); }
            }
            if (approvedSerials is not null && proofs.Any(proof => !approvedSerials.TryGetValue(proof.Index, out var serial) || serial != proof.Serial))
                throw new DeviceException("实际连接与验收预批准串号不符，未读取设备或保存授权。", 403, 1003);
            SetLaunchFeedback("正在核验安卓启动与截图…");
            await ViewModel.ApplyLdConnectionsAsync(proofs, deadline.Token);
            foreach (var index in indices) launchPhases[index] = "已连接";
            UpdateLdPlayerRows(); UpdateUnifiedDirectory();
            SetLaunchFeedback($"已启动并连接 {proofs.Count} 台设备。");
        });
    }

    private static long VBoxBirth(LdPlayerInstance instance)
    {
        using var process = System.Diagnostics.Process.GetProcessById(instance.VBoxProcessId);
        var birth = process.StartTime.ToUniversalTime().Ticks;
        if (process.HasExited) throw new DeviceException($"实例{instance.Index}的虚拟机进程已退出。", 409, 1003);
        return birth;
    }

    private async Task EnsureLaunchResourcesAsync(LdPlayerInstance instance, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var configuration = LdLaunchMemory.ReadConfiguration(ViewModel.Runtime.FrozenConnectionSettings.LdConsolePath, instance.Index);
        var memory = new MemoryStatus { Length = (uint)Marshal.SizeOf<MemoryStatus>() };
        var host = GlobalMemoryStatusEx(ref memory)
            ? new LdHostMemory(memory.AvailablePhysical, memory.AvailablePageFile, memory.MemoryLoad) : null;
        var assessment = LdLaunchMemory.Assess(configuration, host);
        var proceed = await ConfirmLaunchMemoryAsync(instance.Index, instance.Name, assessment, ct);
        if (!proceed) ldOperationCancellation?.Cancel();
        ct.ThrowIfCancellationRequested();
        if (!proceed) throw new OperationCanceledException("用户取消了资源风险启动。", ct);
    }

    private async Task<bool> ConfirmLaunchMemoryAsync(int index, string name, LdLaunchMemoryAssessment assessment, CancellationToken ct)
    {
        return await Dispatcher.InvokeAsync(() =>
        {
            ct.ThrowIfCancellationRequested();
            SetLaunchFeedback($"实例 {index}：{assessment.Message}");
            ViewModel.Runtime.Log.Write("ld-launch-memory-assessment", $"index={index}; confirmation={assessment.RequiresConfirmation}; {assessment.Message}");
            if (!assessment.RequiresConfirmation) return true;
            var accepted = ViewModel.Confirm("启动资源风险", $"实例 {index}：{name}\n\n{assessment.Message}\n\n以上为检查时快照，配置和资源可能继续变化。这不是雷电官方的启动限制。继续可能导致卡顿或启动失败。\n确定：仍然启动本实例；取消：停止本批次后续启动，不关闭已运行实例。");
            ct.ThrowIfCancellationRequested();
            ViewModel.Runtime.Log.Write(accepted ? "ld-launch-memory-accepted" : "ld-launch-memory-declined", $"index={index}");
            return accepted;
        });
    }

    private async Task VerifyLaunchMemoryUiAsync()
    {
        if (!ViewModel.Demo) throw new InvalidOperationException("内存提示离线验收仅限演示模式。");
        var originalConfirm = ViewModel.Confirm;
        var confirmations = 0; var accept = true;
        CancellationTokenSource? cancelDuringConfirmation = null;
        ViewModel.Confirm = (title, message) =>
        {
            if (!Dispatcher.CheckAccess() || title != "启动资源风险" || !message.Contains("实例 2：离线内存测试", StringComparison.Ordinal) || !message.Contains("不是雷电官方", StringComparison.Ordinal))
                throw new InvalidOperationException("资源风险提示线程或目标范围错误。");
            confirmations++; cancelDuringConfirmation?.Cancel(); return accept;
        };
        try
        {
            var good = LdLaunchMemory.Assess(new(6144), new((ulong)(7.8 * 1073741824), 16UL * 1073741824, 75));
            if (!await Task.Run(() => ConfirmLaunchMemoryAsync(2, "离线内存测试", good, CancellationToken.None)) || confirmations != 0)
                throw new InvalidOperationException("7.8 GiB/75%的6 GiB实例被拦截或出现普通启动确认。");
            foreach (var nonblocking in new[]
            {
                LdLaunchMemory.Assess(new(null, "实例 RAM 未知"), new(11UL * 1073741824, 20UL * 1073741824, 65)),
                LdLaunchMemory.Assess(new(6144), null),
                LdLaunchMemory.Assess(new(6144), new(6UL * 1073741824, 6UL * 1073741824, 99))
            })
                if (!await Task.Run(() => ConfirmLaunchMemoryAsync(2, "离线内存测试", nonblocking, CancellationToken.None)) || confirmations != 0)
                    throw new InvalidOperationException("未知、采样失败或仅估算余量不足仍触发资源确认。");
            var risk = LdLaunchMemory.Assess(new(8192), new((ulong)(7.8 * 1073741824), 16UL * 1073741824, 75));
            if (!await Task.Run(() => ConfirmLaunchMemoryAsync(2, "离线内存测试", risk, CancellationToken.None)) || confirmations != 1)
                throw new InvalidOperationException("用户无法确认继续风险启动。");
            accept = false;
            if (await Task.Run(() => ConfirmLaunchMemoryAsync(2, "离线内存测试", risk, CancellationToken.None)) || confirmations != 2)
                throw new InvalidOperationException("用户取消资源风险提示仍被接受。");
            using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
            try
            {
                await ConfirmLaunchMemoryAsync(2, "离线内存测试", risk, cancellation.Token);
                throw new InvalidOperationException("已取消的资源检查仍可继续。");
            }
            catch (OperationCanceledException) when (cancellation.IsCancellationRequested) { }
            if (confirmations != 2) throw new InvalidOperationException("已取消的资源检查仍弹出提示。");
            using var duringConfirmation = new CancellationTokenSource();
            accept = true; cancelDuringConfirmation = duringConfirmation;
            try
            {
                await Task.Run(() => ConfirmLaunchMemoryAsync(2, "离线内存测试", risk, duringConfirmation.Token));
                throw new InvalidOperationException("提示期间取消后仍然接受启动。");
            }
            catch (OperationCanceledException) when (duringConfirmation.IsCancellationRequested) { }
            if (confirmations != 3) throw new InvalidOperationException("提示期间取消验收未覆盖确认回调。");
        }
        finally { ViewModel.Confirm = originalConfirm; }
    }

    private async Task EditLdInstanceAsync(bool create)
    {
        var selected = ldSelection;
        if (!create && selected is null) throw new DeviceException("请选择已停止的实例。", 400, 1007);
        var name = new TextBox { Text = create ? "矩阵测试-" + DateTime.Now.ToString("MMdd-HHmmss") : selected!.Name, IsReadOnly = !create };
        var cpu = new ComboBox { ItemsSource = new[] { 1, 2, 3, 4 }, SelectedItem = create ? 2 : (object?)null };
        var memory = new ComboBox { ItemsSource = new[] { 256, 512, 768, 1024, 1536, 2048, 4096, 8192 }, SelectedItem = create ? 2048 : (object?)null };
        var width = new TextBox { Text = (selected?.Instance.Width ?? 1280).ToString() };
        var height = new TextBox { Text = (selected?.Instance.Height ?? 720).ToString() };
        var dpi = new TextBox { Text = (selected?.Instance.Dpi ?? 240).ToString() };
        var content = new StackPanel { Margin = new Thickness(20) };
        foreach (var field in new[] { Field("名称", name, 90), Field("新CPU核数", cpu, 90), Field("新内存 MiB", memory, 90), Field("宽度", width, 90), Field("高度", height, 90), Field("DPI", dpi, 90) }) content.Children.Add(field);
        var error = new TextBlock { TextWrapping = TextWrapping.Wrap, Foreground = Brushes.Firebrick, Margin = new Thickness(0, 0, 0, 8) }; content.Children.Add(error);
        var dialog = new Window { Owner = this, Title = create ? "新建空白模拟器" : "冷态资源配置", Width = 420, ResizeMode = ResizeMode.NoResize, WindowStartupLocation = WindowStartupLocation.CenterOwner };
        var layout = new DockPanel(); var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(20, 8, 20, 16) };
        DockPanel.SetDock(buttons, Dock.Bottom); layout.Children.Add(buttons); layout.Children.Add(new ScrollViewer { Content = content, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled }); dialog.Content = layout;
        LdInstanceConfiguration? configuration = null;
        var submit = new Button { Content = create ? "创建并配置" : "保存配置", IsDefault = true, Margin = new Thickness(0, 6, 0, 0) };
        submit.Click += (_, _) =>
        {
            try
            {
                if (!int.TryParse(width.Text, out var w) || !int.TryParse(height.Text, out var h) || !int.TryParse(dpi.Text, out var d)) throw new DeviceException("宽度、高度和DPI必须是整数。");
                if (cpu.SelectedItem is null || memory.SelectedItem is null) throw new DeviceException("请选择新的CPU核数和内存值。");
                configuration = new((int)cpu.SelectedItem, (int)memory.SelectedItem, w, h, d); LdInstanceEditor.Validate(configuration);
                dialog.DialogResult = true;
            }
            catch (Exception ex) { error.Text = ViewModel.PresentError(ex); }
        };
        var cancel = new Button { Content = "取消", IsCancel = true, MinWidth = 78, Name = "CancelInstanceEdit" }; buttons.Children.Add(cancel); buttons.Children.Add(submit);
        if (ShowOwnedDialog(dialog, 420, 560) != true || configuration is null) return;
        if (!ViewModel.Confirm(create ? "新建空白雷电实例" : "修改雷电实例资源", create ? $"创建空白实例“{name.Text.Trim()}”，配置 {configuration.Cpu}核 / {configuration.MemoryMb} MiB / {configuration.Width}×{configuration.Height}。\n不会复制账号或数据，不自动启动。继续？" : $"仅修改实例 {selected!.Index}：{selected.Name} 的CPU、内存和分辨率；不会修改设备身份。配置后需启动验证。继续？")) return;
        await RunLdWorkspaceOperationAsync(create ? "新建空白实例" : "配置实例", async ct =>
        {
            var editor = ViewModel.Runtime.LdEditor ?? throw new DeviceException("雷电编辑能力未配置，请重启程序。", 503, 2001);
            var target = selected?.Instance;
            if (create)
            {
                var result = await editor.CreateAsync(name.Text.Trim(), ct); target = result.Instance;
                ViewModel.Runtime.Log.Write("ld-create", result.Message);
            }
            LdPlayerCommandResult configured;
            try { configured = await editor.ConfigureAsync(target!.Index, target.Generation, configuration, ct); }
            catch (Exception ex) when (create && ex is not OperationCanceledException)
            { throw new DeviceException($"已创建实例 {target!.Index}：{target.Name}，但配置失败：{ex.Message}。未删除或重复创建，请检查目录。", 409, 1003); }
            ViewModel.Runtime.Log.Write("ld-configure", configured.Message);
            ldFeedback.Text = $"实例 {target.Index} 已提交资源配置，启动后核验生效情况。";
        });
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MemoryStatus { public uint Length, MemoryLoad; public ulong TotalPhysical, AvailablePhysical, TotalPageFile, AvailablePageFile, TotalVirtual, AvailableVirtual, AvailableExtendedVirtual; }
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GlobalMemoryStatusEx(ref MemoryStatus status);
}
