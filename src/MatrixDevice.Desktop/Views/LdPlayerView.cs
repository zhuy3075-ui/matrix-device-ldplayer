using System.Collections.ObjectModel;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MatrixDevice.Core;
using Microsoft.Win32;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    private readonly ObservableCollection<LdInstanceRow> ldRows = [];
    private readonly List<AsyncCommand> ldCommands = [];
    private readonly Dictionary<string, AsyncCommand> ldCommandsByLabel = [];
    private LdInstanceRow? ldSelection;
    private bool ldBusy;
    private TextBlock ldFeedback = null!;
    private TextBlock ldContext = null!;
    private TextBox ldPackage = null!;
    private TextBox ldText = null!;
    private DataGrid ldGrid = null!;

    private FrameworkElement BuildLdPlayerPage()
    {
        var page = Page();
        var toolbar = Toolbar();
        toolbar.Children.Add(LdButton("\uE72C", "刷新实例", RefreshLdPlayerUiAsync, requireSelection: false));
        toolbar.Children.Add(LdButton("\uE768", "启动实例", () => ExecuteLdPlayerAsync("launch"), requireStopped: true));
        toolbar.Children.Add(LdButton("\uE71A", "关闭实例", () => ExecuteLdPlayerAsync("quit"), requireProcess: true));
        toolbar.Children.Add(LdButton("\uE777", "重启实例", () => ExecuteLdPlayerAsync("reboot"), requireProcess: true));
        DockPanel.SetDock(toolbar, Dock.Top); page.Children.Add(toolbar);
        ldFeedback = new TextBlock { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 9, 0, 9), Foreground = Brushes.DimGray };
        DockPanel.SetDock(ldFeedback, Dock.Top); page.Children.Add(ldFeedback);
        var body = new Grid(); body.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); body.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        ldGrid = new DataGrid { ItemsSource = ldRows, IsReadOnly = true, SelectionMode = DataGridSelectionMode.Single };
        TextColumn(ldGrid, "实例编号", nameof(LdInstanceRow.Index), 80);
        TextColumn(ldGrid, "名称", nameof(LdInstanceRow.Name), 175);
        TextColumn(ldGrid, "安卓状态", nameof(LdInstanceRow.AndroidState), 110);
        TextColumn(ldGrid, "进程状态", nameof(LdInstanceRow.ProcessState), 110);
        TextColumn(ldGrid, "播放器进程", nameof(LdInstanceRow.ProcessId), 110);
        TextColumn(ldGrid, "虚拟机进程", nameof(LdInstanceRow.VBoxProcessId), 110);
        TextColumn(ldGrid, "分辨率", nameof(LdInstanceRow.Size), 120);
        TextColumn(ldGrid, "密度", nameof(LdInstanceRow.Dpi), 85);
        TextColumn(ldGrid, "会话代次", nameof(LdInstanceRow.Generation), 180);
        TextColumn(ldGrid, "显式设备绑定", nameof(LdInstanceRow.Binding), 240);
        ldGrid.SelectionChanged += (_, _) =>
        {
            ldSelection = ldGrid.SelectedItem as LdInstanceRow;
            ldContext.Text = ldSelection is null ? "当前实例：未选择" : $"当前实例：{ldSelection.Name} · 编号 {ldSelection.Index} · 代次 {ldSelection.Generation}";
            RefreshLdButtons();
        };
        body.Children.Add(ldGrid);
        var actions = new StackPanel { Margin = new Thickness(0, 12, 0, 0) }; Grid.SetRow(actions, 1); body.Children.Add(actions);
        ldContext = new TextBlock { Text = "当前实例：未选择", FontWeight = FontWeights.SemiBold, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 8) }; actions.Children.Add(ldContext);
        var packageRow = Toolbar(); packageRow.Children.Add(new TextBlock { Text = "应用包名", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 8, 0) });
        ldPackage = new TextBox { Width = 300, Margin = new Thickness(0, 0, 8, 0) }; ldPackage.TextChanged += (_, _) => RefreshLdButtons(); packageRow.Children.Add(ldPackage);
        packageRow.Children.Add(LdButton("\uE768", "启动应用", () => ExecuteLdPlayerAsync("runapp", new JsonObject { ["packageName"] = ldPackage.Text.Trim() }), requirePackage: true, requireStarted: true));
        packageRow.Children.Add(LdButton("\uE71A", "停止应用", () => ExecuteLdPlayerAsync("killapp", new JsonObject { ["packageName"] = ldPackage.Text.Trim() }), requirePackage: true, requireStarted: true));
        packageRow.Children.Add(LdButton("\uE898", "安装应用", async () =>
        {
            var dialog = new OpenFileDialog { Filter = "安卓应用|*.apk" };
            if (dialog.ShowDialog(this) == true) await ExecuteLdPlayerAsync("installapp", new JsonObject { ["file"] = dialog.FileName });
        }, requireStarted: true));
        actions.Children.Add(packageRow);
        var textRow = Toolbar(); textRow.Margin = new Thickness(0, 8, 0, 8); textRow.Children.Add(new TextBlock { Text = "输入文字", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 8, 0) });
        ldText = new TextBox { Width = 420, Margin = new Thickness(0, 0, 8, 0) }; ldText.TextChanged += (_, _) => RefreshLdButtons(); textRow.Children.Add(ldText);
        textRow.Children.Add(LdButton("\uE8F4", "发送文字", () => ExecuteLdPlayerAsync("input", new JsonObject { ["text"] = ldText.Text }), requireText: true, requireStarted: true));
        actions.Children.Add(textRow);
        var keys = Toolbar();
        keys.Children.Add(LdButton("\uE80F", "主页", () => ExecuteLdPlayerAsync("keyevent", new JsonObject { ["key"] = "home" }), requireStarted: true));
        keys.Children.Add(LdButton("\uE72B", "返回", () => ExecuteLdPlayerAsync("keyevent", new JsonObject { ["key"] = "back" }), requireStarted: true));
        keys.Children.Add(LdButton("\uE995", "增大音量", () => ExecuteLdPlayerAsync("keyevent", new JsonObject { ["key"] = "volumeup" }), requireStarted: true));
        keys.Children.Add(LdButton("\uE993", "减小音量", () => ExecuteLdPlayerAsync("keyevent", new JsonObject { ["key"] = "volumedown" }), requireStarted: true));
        actions.Children.Add(keys);
        page.Children.Add(body);
        UpdateLdPlayerRows();
        ViewModel.PropertyChanged += (_, _) => RefreshLdButtons();
        return page;
    }

    private Button LdButton(string glyph, string label, Func<Task> action, bool requireSelection = true, bool requirePackage = false, bool requireText = false, bool requireStarted = false, bool requireStopped = false, bool requireProcess = false)
    {
        var command = new AsyncCommand(async () =>
        {
            try { await action(); }
            catch (Exception error) { ldFeedback.Text = label + "失败：" + ViewModel.PresentError(error); }
        }, () => !ldBusy && !ViewModel.HasActiveOperations && ViewModel.Runtime.LdPlayer is not null && (!requireSelection || ldSelection is not null) && (!requirePackage || !string.IsNullOrWhiteSpace(ldPackage?.Text)) && (!requireText || !string.IsNullOrWhiteSpace(ldText?.Text)) && (!requireStarted || ldSelection?.Instance.AndroidStarted == true) && (!requireStopped || ldSelection?.Instance is { AndroidStarted: false, ProcessId: <= 0 }) && (!requireProcess || ldSelection?.Instance is { ProcessId: > 0 } || ldSelection?.Instance.AndroidStarted == true));
        ldCommands.Add(command);
        ldCommandsByLabel.Add(label, command);
        return Button(glyph, label, command);
    }

    private void RefreshLdButtons() { foreach (var command in ldCommands) command.Refresh(); }
    private void UpdateLdPlayerRows()
    {
        var index = ldSelection?.Index;
        ldRows.Clear();
        if (ViewModel.Runtime.LdPlayer is not { } service)
        {
            ldFeedback.Text = "雷电能力未接入：请在设置中选择雷电命令行程序路径，保存并重新启动。本页不会自动配置设备序列号。";
            RefreshLdButtons(); return;
        }
        foreach (var instance in service.Instances)
        {
            var binding = ViewModel.Runtime.Settings.LdSerialBindings.TryGetValue(instance.Index, out var serial) ? serial + "（配置绑定，尚未核验）" : "尚未绑定";
            ldRows.Add(new LdInstanceRow(instance, binding));
        }
        ldGrid.SelectedItem = index is { } selected ? ldRows.FirstOrDefault(row => row.Index == selected) : null;
        if (ViewModel.Runtime.LdPlayerError is { } error) ldFeedback.Text = "雷电实例发现失败：" + ViewModel.PresentError(new InvalidOperationException(error));
        else ldFeedback.Text = $"已发现 {ldRows.Count} 个雷电实例" + (ViewModel.Demo ? " · 演示模式，不连接真实雷电" : " · 本机实例");
        RefreshLdButtons();
    }

    private async Task RefreshLdPlayerUiAsync()
    {
        if (ViewModel.HasActiveOperations) return;
        ldBusy = true; ViewModel.SetExternalBusy(true); ldFeedback.Text = "正在刷新雷电实例…"; RefreshLdButtons();
        try { await ViewModel.Runtime.RefreshLdPlayerAsync(); UpdateLdPlayerRows(); }
        catch (Exception error) { ldFeedback.Text = "刷新雷电实例失败：" + ViewModel.PresentError(error); }
        finally { ldBusy = false; ViewModel.SetExternalBusy(false); RefreshLdButtons(); }
    }

    private async Task ExecuteLdPlayerAsync(string operation, JsonObject? body = null)
    {
        if (ViewModel.HasActiveOperations) throw new DeviceException("请先结束界面操作、脚本或文件传输，再操作雷电实例");
        var selected = ldSelection ?? throw new InvalidOperationException("未选择雷电实例");
        var service = ViewModel.Runtime.LdPlayer ?? throw new InvalidOperationException("雷电命令行能力尚未接入");
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
            var result = await service.ExecuteAsync(selected.Index, operation, body, CancellationToken.None);
            UpdateLdPlayerRows();
            ldFeedback.Text = $"实例 {result.Index}：{result.Message}\n接受状态：{(result.Accepted ? "已接受" : "未接受")}；结果核验：{(result.Verified ? "已核验" : "待核验")}";
        }
        catch (Exception error) { UpdateLdPlayerRows(); ldFeedback.Text = $"实例 {selected.Index} 操作失败：{ViewModel.PresentError(error)}"; }
        finally { ldBusy = false; ViewModel.SetExternalBusy(false); RefreshLdButtons(); }
    }
}

public sealed class LdInstanceRow(LdPlayerInstance instance, string binding)
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
}
