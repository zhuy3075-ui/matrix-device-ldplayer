using System.Collections.ObjectModel;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using MatrixDevice.Core;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    private readonly ObservableCollection<LdInstanceRow> ldRows = [];
    private readonly List<AsyncCommand> ldCommands = [];
    private readonly Dictionary<string, AsyncCommand> ldCommandsByLabel = [];
    private LdInstanceRow? ldSelection;
    private bool ldBusy;
    private TextBlock ldFeedback = null!;
    private ComboBox instanceActionChoice = null!;
    private UnifiedDeviceRow? instanceActionRow;

    private FrameworkElement BuildInstanceActions()
    {
        var page = new StackPanel { Margin = new Thickness(0, 0, 0, 10) };
        var toolbar = Toolbar();
        instanceActionChoice = new ComboBox { ItemsSource = new ListCollectionView(unifiedDevices), DisplayMemberPath = nameof(UnifiedDeviceRow.ActionCaption),
            IsSynchronizedWithCurrentItem = false, Width = 230, MinHeight = 34, Margin = new Thickness(0, 0, 8, 6), ToolTip = "实例操作目标" };
        instanceActionChoice.SelectionChanged += (_, _) => SelectUnifiedInstance(instanceActionChoice.SelectedItem as UnifiedDeviceRow);
        toolbar.Children.Add(instanceActionChoice);
        toolbar.Children.Add(LdButton("\uE768", "启动并连接", () => ConnectLdInstancesAsync([ldSelection!.Index])));
        toolbar.Children.Add(LdButton("\uE8D4", "接入已运行实例", ConnectRunningLdInstancesAsync, requireSelection: false));
        toolbar.Children.Add(LdButton("\uE710", "新建空白实例", () => EditLdInstanceAsync(true), requireSelection: false));
        toolbar.Children.Add(LdButton("\uE713", "资源配置", () => EditLdInstanceAsync(false), requireStopped: true));
        toolbar.Children.Add(LdButton("\uE71A", "关闭实例", () => ExecuteLdPlayerAsync("quit"), requireProcess: true));
        toolbar.Children.Add(LdButton("\uE777", "重启实例", () => ExecuteLdPlayerAsync("reboot"), requireProcess: true));
        page.Children.Add(toolbar);
        UpdateLdPlayerRows();
        ViewModel.PropertyChanged += (_, _) => RefreshLdButtons();
        return page;
    }

    private void SelectUnifiedInstance(UnifiedDeviceRow? row)
    {
        if (row is not null && !unifiedDevices.Contains(row)) row = null;
        instanceActionRow = row;
        if (instanceActionChoice is not null && !ReferenceEquals(instanceActionChoice.SelectedItem, row)) instanceActionChoice.SelectedItem = row;
        var instance = row?.Index is { } index ? ViewModel.Runtime.LdPlayer?.Instances.FirstOrDefault(item => item.Index == index) : null;
        ldSelection = instance is null ? null : new LdInstanceRow(instance, row!.Binding, row.State);
        RefreshLdButtons();
    }

    private Button LdButton(string glyph, string label, Func<Task> action, bool requireSelection = true, bool requireStopped = false, bool requireProcess = false)
    {
        var command = new AsyncCommand(async () =>
        {
            try { await action(); }
            catch (Exception error) { ldFeedback.Text = label + "失败：" + ViewModel.PresentError(error); }
        }, () => !ldBusy && !captureInProgress && !ViewModel.HasActiveOperations && ViewModel.ConnectionReloadAvailable && ViewModel.Runtime.LdPlayer is not null && (!requireSelection || ldSelection is not null && (ViewModel.Demo || ldSelection.Index is > 0 and < 99999)) && (!requireStopped || ldSelection?.Instance is { IsStarting: false, AndroidStarted: false, ProcessId: <= 0, VBoxProcessId: <= 0, TopWindow: 0, BindWindow: 0 }) && (!requireProcess || ldSelection?.Instance is { ProcessId: > 0 } || ldSelection?.Instance.AndroidStarted == true));
        ldCommands.Add(command);
        ldCommandsByLabel.Add(label, command);
        return Button(glyph, label, command);
    }

    private void RefreshLdButtons() { foreach (var command in ldCommands) command.Refresh(); RefreshLaunchCommands(); }
    private void UpdateLdPlayerRows()
    {
        ldRows.Clear();
        if (ViewModel.Runtime.LdPlayer is not { } service)
        {
            ldFeedback.Text = "雷电能力未接入：请在设置中选择雷电命令行程序路径，保存并重新启动。本页不会自动配置设备序列号。";
            RefreshLdButtons(); return;
        }
        foreach (var instance in service.Instances)
        {
            var binding = ViewModel.Runtime.Settings.LdSerialBindings.TryGetValue(instance.Index, out var serial) ? serial + "（配置绑定，尚未核验）" : "尚未绑定";
            ldRows.Add(new LdInstanceRow(instance, binding, InstanceConnectionState(instance)));
        }
        SelectUnifiedInstance(instanceActionRow);
        if (ViewModel.Runtime.LdPlayerError is { } error) ldFeedback.Text = "雷电实例发现失败：" + ViewModel.PresentError(new InvalidOperationException(error));
        else if (string.IsNullOrWhiteSpace(ldFeedback.Text)) ldFeedback.Text = ViewModel.Demo ? "演示模式" : "";
        RefreshLdButtons();
    }

    private async Task RefreshLdPlayerUiAsync()
    {
        if (ViewModel.HasActiveOperations) return;
        ldBusy = true; ViewModel.SetExternalBusy(true); ldFeedback.Text = "正在刷新雷电实例…"; RefreshLdButtons();
        try { await ViewModel.Runtime.RefreshLdPlayerAsync(); UpdateLdPlayerRows(); UpdateUnifiedDirectory(); }
        catch (Exception error) { ldFeedback.Text = "刷新雷电实例失败：" + ViewModel.PresentError(error); }
        finally { ldBusy = false; ViewModel.SetExternalBusy(false); RefreshLdButtons(); }
    }

    private async Task ExecuteLdPlayerAsync(string operation, JsonObject? body = null)
    {
        if (ViewModel.HasActiveOperations) throw new DeviceException("请先结束界面操作、脚本或文件传输，再操作雷电实例");
        var selected = ldSelection ?? throw new InvalidOperationException("未选择雷电实例");
        var service = ViewModel.Runtime.LdPlayer ?? throw new InvalidOperationException("雷电命令行能力尚未接入");
        if (!ViewModel.Demo && selected.Index is <= 0 or >= 99999) throw new DeviceException("原实例0和系统保留编号受保护。", 403, 1003);
        if (!ViewModel.Demo && operation is not ("launch" or "quit" or "reboot") && !LdInstanceConnected(selected.Index)) throw new DeviceException("请先接入该实例，再执行应用或输入操作。", 403, 1003);
        body ??= new JsonObject(); body["expectedGeneration"] = selected.Generation;
        if (operation is "quit" or "reboot")
        {
            var label = operation == "quit" ? "关闭雷电实例" : "重启雷电实例";
            if (!ViewModel.Confirm(label, $"实例：{selected.Name}\n编号：{selected.Index}\n会话代次：{selected.Generation}\n此操作会中断该实例中的活动应用。确认继续？")) { ldFeedback.Text = "操作已取消，未发送命令。"; return; }
            body["confirmed"] = true;
        }
        ldBusy = true; ViewModel.SetExternalBusy(true); ldFeedback.Text = "正在执行所选实例命令…"; RefreshLdButtons();
        try
        {
            ViewModel.Runtime.RequireFrozenConnectionSettings();
            if (ViewModel.Runtime.Api.LdCommandIdentityGuard is { } verify) await verify(selected.Index, operation, CancellationToken.None);
            var result = await service.ExecuteAsync(selected.Index, operation, body, CancellationToken.None);
            UpdateLdPlayerRows(); UpdateUnifiedDirectory();
            ViewModel.Runtime.Log.Write("ld-command-result", $"index={result.Index}; accepted={result.Accepted}; verified={result.Verified}; {result.Message}");
            ldFeedback.Text = $"实例 {result.Index}：" + (result.Verified ? "操作已完成" : "命令已提交，等待状态更新");
        }
        catch (Exception error) { UpdateLdPlayerRows(); UpdateUnifiedDirectory(); ldFeedback.Text = $"实例 {selected.Index} 操作失败：{ViewModel.PresentError(error)}"; }
        finally { ldBusy = false; ViewModel.SetExternalBusy(false); RefreshLdButtons(); }
    }
}

public sealed class LdInstanceRow(LdPlayerInstance instance, string binding, string connectionState = "待接入")
{
    public LdPlayerInstance Instance { get; } = instance;
    public int Index => Instance.Index;
    public string Name => Instance.Name;
    public string AndroidState => Instance.AndroidStarted ? "已启动" : "未启动";
    public string ProcessState => Instance.ProcessId > 0 ? "已发现进程" : "未发现进程";
    public int ProcessId => Instance.ProcessId;
    public int VBoxProcessId => Instance.VBoxProcessId;
    public string Size => Instance.Width is { } width && Instance.Height is { } height ? $"{width} × {height}" : "未核验";
    public string Dpi => Instance.Dpi?.ToString() ?? "未核验";
    public long Generation => Instance.Generation;
    public string Binding { get; } = binding;
    public string ConnectionState { get; } = connectionState;
}
