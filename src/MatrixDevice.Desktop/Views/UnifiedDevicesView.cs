using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    private readonly List<(string Tag, ToggleButton Button)> deviceModes = [];
    private WrapPanel pendingInstances = null!;
    private Expander pendingSection = null!;
    private DateTimeOffset lastGuestDiscovery;

    private FrameworkElement BuildDeviceModes()
    {
        var row = new WrapPanel { Margin = new Thickness(0, 0, 0, 12) };
        foreach (var (tag, glyph, label) in new[] { ("设备墙 / 主控", "\uE8A9", "设备群控"), ("雷电实例", "\uE7F4", "本机模拟器") })
        {
            var choice = WallToggle(glyph, label);
            choice.Click += (_, _) => NavigateTo(tag);
            deviceModes.Add((tag, choice)); row.Children.Add(choice);
        }
        var summary = new TextBlock { VerticalAlignment = VerticalAlignment.Center, Foreground = WallMuted, Margin = new Thickness(8, 0, 0, 6), TextWrapping = TextWrapping.Wrap };
        deviceSummaries.Add(summary); row.Children.Add(summary);
        return row;
    }

    private FrameworkElement BuildPendingInstances()
    {
        pendingInstances = new WrapPanel();
        pendingSection = new Expander
        {
            Header = "待接入的本机模拟器", IsExpanded = true, Margin = new Thickness(0, 12, 0, 0),
            Content = new ScrollViewer { Content = pendingInstances, MaxHeight = 125, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled }
        };
        return pendingSection;
    }

    internal void UpdateUnifiedDirectory()
    {
        var instances = ViewModel.Runtime.LdPlayer?.Instances ?? [];
        var connected = ViewModel.Devices.Count(row => row.Info.Online);
        var admitted = instances.Count(instance => ViewModel.Runtime.Settings.LdSerialBindings.ContainsKey(instance.Index));
        foreach (var summary in deviceSummaries) summary.Text = $"本机 {instances.Count} 台 · 已接入 {admitted} 台 · 已连接 {connected} 台";
        foreach (var mode in deviceModes) mode.Button.IsChecked = Equals((tabs.SelectedItem as TabItem)?.Tag, mode.Tag);
        if (pendingInstances is null) return;
        pendingInstances.Children.Clear();
        foreach (var instance in instances.Where(instance => !LdInstanceConnected(instance.Index)))
        {
            if (!string.IsNullOrWhiteSpace(ViewModel.Search) && !instance.Name.Contains(ViewModel.Search, StringComparison.OrdinalIgnoreCase) && !instance.Index.ToString().Contains(ViewModel.Search, StringComparison.OrdinalIgnoreCase)) continue;
            if (ViewModel.GroupFilter is not ("全部分组" or "未分组")) continue;
            var item = new StackPanel { Width = 218, Margin = new Thickness(10) };
            item.Children.Add(new TextBlock { Text = instance.Name, FontWeight = FontWeights.SemiBold, TextTrimming = TextTrimming.CharacterEllipsis, ToolTip = instance.Name });
            item.Children.Add(new TextBlock { Text = $"实例 {instance.Index} · {InstanceConnectionState(instance)}", Foreground = WallMuted, Margin = new Thickness(0, 6, 0, 8) });
            var index = instance.Index;
            item.Children.Add(Button("\uE768", instance.AndroidStarted ? "连接" : "启动并连接", () => ConnectLdInstancesAsync([index]), canExecute: () => !ViewModel.Demo && index > 0 && !ldBusy && !captureInProgress && !ViewModel.HasActiveOperations && ViewModel.ConnectionReloadAvailable));
            pendingInstances.Children.Add(new Border { Child = item, BorderBrush = WallBorder, BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(6), Margin = new Thickness(0, 6, 10, 0) });
        }
        pendingSection.Visibility = pendingInstances.Children.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        pendingSection.Header = $"待接入的本机模拟器（{pendingInstances.Children.Count}台）";
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
            if (ViewModel.Runtime.LdPlayer is not null) await ViewModel.Runtime.RefreshLdPlayerAsync(deadline.Token);
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
            ldFeedback.Text = "自动更新失败：" + ViewModel.PresentError(ex);
        }
    }

    private async Task RunLdWorkspaceOperationAsync(string title, Func<CancellationToken, Task> operation)
    {
        if (ldBusy || captureInProgress || ViewModel.HasActiveOperations || scanOperation is { IsCompleted: false }) throw new DeviceException("请等待当前操作或网络扫描结束。", 409, 1003);
        if (!ViewModel.ConnectionReloadAvailable) throw new DeviceException("请先停止接口服务、脚本和文件传输。", 409, 1003);
        if (ViewModel.Demo) throw new DeviceException("演示模式不会操作真实模拟器。", 409, 1003);
        ViewModel.Runtime.RequireFrozenConnectionSettings();
        ldBusy = true; ViewModel.SetExternalBusy(true); RefreshLdButtons();
        using var cancellation = new CancellationTokenSource(); ldOperationCancellation = cancellation;
        try
        {
            if (directoryOperation is not null) await directoryOperation;
            ldFeedback.Text = title;
            await operation(cancellation.Token);
            ViewModel.Runtime.Log.Write("ld-workspace-complete", title);
        }
        catch (OperationCanceledException) { ldFeedback.Text = "已停止等待及后续任务，已启动的实例保持运行。"; ViewModel.Runtime.Log.Write("ld-workspace-cancel", title); }
        catch (Exception ex) { ldFeedback.Text = title + "失败：" + ViewModel.PresentError(ex); ViewModel.Runtime.Log.Write("ld-workspace-error", ex.ToString()); }
        finally
        {
            ldOperationCancellation = null; ldBusy = false; ViewModel.SetExternalBusy(false);
            UpdateLdPlayerRows(); UpdateUnifiedDirectory(); RefreshLdButtons();
        }
    }

    private IProgress<LdLaunchProgress> LaunchProgress() => new Progress<LdLaunchProgress>(progress =>
    {
        ldFeedback.Text = $"实例 {progress.Index}：" + (progress.State switch { "checking-resources" => "检查资源", "starting" => "正在启动", "waiting" => "等待安卓就绪", "ready" or "skipped" => "已启动，等待接入", "cancelled" => "已取消", _ => "启动失败" });
        ViewModel.Runtime.Log.Write("ld-launch-" + progress.State, $"index={progress.Index}; {progress.Message}");
        UpdateLdPlayerRows(); UpdateUnifiedDirectory();
    });

    private async Task StartAllLdInstancesAsync()
    {
        var confirmedInstances = ViewModel.Runtime.LdPlayer?.Instances.Where(item => item.Index > 0 && item.Index != 99999).ToArray() ?? [];
        var indices = confirmedInstances.Select(item => item.Index).ToArray();
        if (indices.Length == 0) throw new DeviceException("没有可启动的独立实例。", 404, 1002);
        if (!ViewModel.Confirm("启动全部雷电实例", $"将依次检查并启动 {indices.Length} 台独立实例，跳过已运行实例和受保护实例0。\n每台启动前要求至少11 GiB可用内存（8 GiB实例预算及3 GiB系统预留）；资源不足立即停止。\n此操作不会自动授权新设备；启动后可用“接入已运行实例”加入群控。继续？")) return;
        await RunLdWorkspaceOperationAsync("启动全部实例", async ct =>
        {
            var coordinator = new LdLaunchCoordinator(ViewModel.Runtime.LdPlayer!);
            await coordinator.StartAsync(indices, EnsureLaunchResourcesAsync, LaunchProgress(), ct, expectedInstances: confirmedInstances);
            ldFeedback.Text = "批次启动完成；新设备待接入。";
        });
    }

    private Task ConnectRunningLdInstancesAsync()
    {
        var indices = ViewModel.Runtime.LdPlayer?.Instances.Where(item => item.Index > 0 && item.Index != 99999 && item.AndroidStarted && !LdInstanceConnected(item.Index)).Select(item => item.Index).ToArray() ?? [];
        return ConnectLdInstancesAsync(indices);
    }

    private async Task ConnectLdInstancesAsync(int[] indices)
    {
        if (indices.Length == 0) throw new DeviceException("没有待接入的已运行实例。", 404, 1002);
        if (indices.Any(index => index <= 0 || index == 99999)) throw new DeviceException("实例0受保护，不能自动接入。", 403, 1003);
        var service = ViewModel.Runtime.LdPlayer ?? throw new DeviceException("雷电服务未配置。", 503, 2001);
        var confirmedInstances = indices.Select(index => service.Instances.Single(item => item.Index == index)).ToArray();
        var expectedBirths = confirmedInstances.Where(item => item.VBoxProcessId > 0).ToDictionary(item => item.Index, VBoxBirth);
        var names = string.Join("\n", confirmedInstances.Select(item => $"{item.Index}：{item.Name}"));
        if (!ViewModel.Confirm("接入雷电实例", names + "\n将启动已关闭实例，并核验实际ADB连接、安卓启动和截图；通过后保存授权并加入群控。不会登录、清数据、接受协议或授权敏感权限。继续？")) return;
        await RunLdWorkspaceOperationAsync("接入雷电设备", async ct =>
        {
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
                { ldFeedback.Text = "等待雷电注册ADB连接…"; await Task.Delay(1500, deadline.Token); }
            }
            ldFeedback.Text = "正在核验安卓启动与截图…";
            await ViewModel.ApplyLdConnectionsAsync(proofs, deadline.Token);
            UpdateLdPlayerRows(); UpdateUnifiedDirectory(); NavigateTo("设备墙 / 主控");
            ldFeedback.Text = $"已接入 {proofs.Count} 台设备。";
        });
    }

    private static long VBoxBirth(LdPlayerInstance instance)
    {
        using var process = System.Diagnostics.Process.GetProcessById(instance.VBoxProcessId);
        var birth = process.StartTime.ToUniversalTime().Ticks;
        if (process.HasExited) throw new DeviceException($"实例{instance.Index}的虚拟机进程已退出。", 409, 1003);
        return birth;
    }

    private static Task EnsureLaunchResourcesAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var memory = new MemoryStatus { Length = (uint)Marshal.SizeOf<MemoryStatus>() };
        if (!GlobalMemoryStatusEx(ref memory)) throw new DeviceException("无法读取系统内存，未启动新实例。", 503, 2000);
        if (memory.AvailablePhysical < 11UL * 1024 * 1024 * 1024 || memory.MemoryLoad >= 80)
            throw new DeviceException($"资源不足：可用内存 {memory.AvailablePhysical / 1073741824d:0.0} GiB，负载 {memory.MemoryLoad}%；至少需要11 GiB可用内存且负载低于80%。已运行实例保持不变。", 409, 1003);
        return Task.CompletedTask;
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
        var dialog = new Window { Owner = this, Title = create ? "新建空白模拟器" : "冷态资源配置", Width = 420, SizeToContent = SizeToContent.Height, ResizeMode = ResizeMode.NoResize, WindowStartupLocation = WindowStartupLocation.CenterOwner, Content = content };
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
        content.Children.Add(submit);
        if (dialog.ShowDialog() != true || configuration is null) return;
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
