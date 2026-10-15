using System.Collections.ObjectModel;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Controls;
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

    private FrameworkElement BuildInstanceActions()
    {
        var toolbar = Toolbar();
        toolbar.Children.Add(LdButton("\uE710", "新建空白实例", () => EditLdInstanceAsync(true), requireSelection: false));
        var menu = new ContextMenu();
        AddMenu("\uE713", "资源配置", () => EditLdInstanceAsync(false), requireStopped: true);
        AddMenu("\uE71A", "关闭实例", () => ExecuteLdPlayerAsync("quit"), requireProcess: true);
        AddMenu("\uE777", "重启实例", () => ExecuteLdPlayerAsync("reboot"), requireProcess: true);
        menu.Items.Add(new Separator());
        var show = new AsyncCommand(async () =>
        {
            foreach (var row in unifiedDevices.Where(row => row.IsSelected).ToArray()) await ShowUnifiedWindowAsync(row);
        }, () => !ldBusy && !ViewModel.Busy && unifiedDevices.Any(row => row.IsSelected && row.Index is { } index && ldWindowVisibility?.IsHidden(index) == true));
        launchCommands.Add(show);
        menu.Items.Add(new MenuItem { Header = "恢复旧版隐藏窗口", Icon = WallGlyph("\uE8A7"), Command = show });
        var more = WallButton("\uE712", "更多操作", new ActionCommand(() => { }));
        more.ContextMenu = menu; menu.PlacementTarget = more; menu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
        more.Click += (_, _) => menu.IsOpen = true;
        toolbar.Children.Add(more);
        UpdateLdPlayerRows();
        ViewModel.PropertyChanged += (_, _) => RefreshLdButtons();
        return toolbar;

        void AddMenu(string glyph, string label, Func<Task> action, bool requireStopped = false, bool requireProcess = false)
        {
            var button = LdButton(glyph, label, action, requireStopped: requireStopped, requireProcess: requireProcess);
            menu.Items.Add(new MenuItem { Header = label, Icon = WallGlyph(glyph), Command = button.Command, ToolTip = label + "：仅操作列表中唯一勾选的实例" });
        }
    }

    private void SelectUnifiedInstance(UnifiedDeviceRow? row)
    {
        var selected = unifiedDevices.Where(item => item.IsSelected).ToArray();
        if (selected.Length != 1 || !ReferenceEquals(row, selected[0])) row = null;
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
        var selected = unifiedDevices.Where(row => row.IsSelected).Take(2).ToArray();
        SelectUnifiedInstance(selected.Length == 1 ? selected[0] : null);
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
