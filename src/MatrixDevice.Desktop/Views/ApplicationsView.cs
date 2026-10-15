using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Threading;
using MatrixDevice.Core;
using MatrixDevice.Features;
using MatrixDevice.Host;
using Microsoft.Win32;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    private readonly ObservableCollection<ApplicationTargetRow> applicationTargets = [];
    private readonly ObservableCollection<InstalledApplicationRow> applicationRows = [];
    private readonly ObservableCollection<ApplicationTaskRow> applicationTasks = [];
    private readonly List<AsyncCommand> applicationCommands = [];
    private readonly HashSet<DeviceRow> applicationObservedDevices = [];
    private TextBlock applicationSummary = null!;
    private TextBlock applicationResult = null!;
    private ProgressBar applicationProgress = null!;
    private FrameworkElement applicationTargetPanel = null!;
    private FrameworkElement applicationPage = null!;
    private CheckBox applicationSystemToggle = null!;
    private Button applicationCancel = null!;
    private ICollectionView applicationRowsView = null!;
    private DispatcherTimer? applicationReadTimer;
    private CancellationTokenSource? applicationCancellation;
    private bool applicationRunning;
    private bool applicationReadPending;
    private bool applicationSyncing;
    private Func<string, string, bool>? applicationConfirmationFixture;

    private FrameworkElement BuildAppsPage()
    {
        var page = Page(); applicationPage = page;
        var heading = new DockPanel { Margin = new Thickness(0, 0, 0, 10) };
        var title = Label(LocalizationService.PageDisplayName("应用")); title.Name = "PageHeading"; title.FontSize = 20; heading.Children.Add(title);
        applicationSummary = new TextBlock { VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Right };
        heading.Children.Add(applicationSummary); DockPanel.SetDock(heading, Dock.Top); page.Children.Add(heading);
        var content = new Grid(); content.ColumnDefinitions.Add(new() { Width = new GridLength(228), MinWidth = 180 });
        content.ColumnDefinitions.Add(new() { Width = new GridLength(12) }); content.ColumnDefinitions.Add(new() { Width = new GridLength(1, GridUnitType.Star), MinWidth = 200 }); page.Children.Add(content);
        var targetPanel = new DockPanel(); applicationTargetPanel = targetPanel; content.Children.Add(targetPanel);
        var targetTitle = Label("目标模拟器"); DockPanel.SetDock(targetTitle, Dock.Top); targetPanel.Children.Add(targetTitle);
        var targetToolbar = Toolbar();
        targetToolbar.Children.Add(Button("\uE14E", "全选可用", () => { foreach (var row in applicationTargets.Where(row => row.Available)) row.IsSelected = true; }));
        targetToolbar.Children.Add(Button("\uE711", "清空", () => { foreach (var row in applicationTargets) row.IsSelected = false; })); DockPanel.SetDock(targetToolbar, Dock.Top); targetPanel.Children.Add(targetToolbar);
        var targets = ApplicationGrid(applicationTargets); targets.RowHeight = 60; targets.Columns.Add(ApplicationSelectionColumn(nameof(ApplicationTargetRow.IsSelected), nameof(ApplicationTargetRow.Available)));
        var targetText = new FrameworkElementFactory(typeof(TextBlock)); targetText.SetBinding(TextBlock.TextProperty, new Binding(nameof(ApplicationTargetRow.Caption)));
        targetText.SetValue(TextBlock.TextTrimmingProperty, TextTrimming.CharacterEllipsis); targetText.SetBinding(FrameworkElement.ToolTipProperty, new Binding(nameof(ApplicationTargetRow.Caption)));
        targetText.SetValue(FrameworkElement.MarginProperty, new Thickness(2, 6, 2, 6));
        targets.Columns.Add(new DataGridTemplateColumn { Header = "模拟器 / 连接状态", CellTemplate = new DataTemplate { VisualTree = targetText }, IsReadOnly = true, Width = new DataGridLength(1, DataGridLengthUnitType.Star) }); targetPanel.Children.Add(targets);
        var body = new DockPanel(); Grid.SetColumn(body, 2); content.Children.Add(body);
        var toolbar = Toolbar(); toolbar.Margin = new Thickness(0, 0, 0, 6);
        toolbar.Children.Add(ApplicationButton("\uE72C", "读取应用", ReadApplicationListsAsync, CanManageApplicationTargets));
        toolbar.Children.Add(ApplicationButton("\uE898", "安装 APK", ChooseApplicationPackagesAsync, CanManageApplicationTargets));
        toolbar.Children.Add(ApplicationButton("\uE74D", "卸载所选", () => ChangeSelectedApplicationsAsync(ApplicationOperation.Uninstall), CanManageApplicationRows));
        var more = Button("\uE712", "更多", () => { }); more.ToolTip = "启动、停止或清除所选应用数据"; var menu = new ContextMenu();
        foreach (var operation in new[] { ApplicationOperation.Start, ApplicationOperation.Stop, ApplicationOperation.Clear })
            menu.Items.Add(new MenuItem { Header = ApplicationOperationCaption(operation), Command = ApplicationCommand(() => ChangeSelectedApplicationsAsync(operation), CanManageApplicationRows) });
        more.ContextMenu = menu; more.Click += (_, _) => { menu.PlacementTarget = more; menu.IsOpen = true; }; toolbar.Children.Add(more);
        applicationSystemToggle = new CheckBox { Content = "系统应用", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(8, 0, 8, 0) };
        applicationSystemToggle.Checked += (_, _) => { applicationRowsView.Refresh(); RefreshApplicationControls(); }; applicationSystemToggle.Unchecked += (_, _) => { applicationRowsView.Refresh(); RefreshApplicationControls(); }; toolbar.Children.Add(applicationSystemToggle);
        DockPanel.SetDock(toolbar, Dock.Top); body.Children.Add(toolbar);
        var taskPanel = new DockPanel { Margin = new Thickness(0, 10, 0, 0), Height = 182 };
        var taskHeading = new DockPanel { Margin = new Thickness(0, 0, 0, 5) };
        applicationCancel = Button("\uE71A", "停止后续", () => { applicationCancellation?.Cancel(); applicationResult.Text = "停止已请求，当前项完成后不再执行后续任务；已完成项不回滚。"; });
        applicationCancel.IsEnabled = false; DockPanel.SetDock(applicationCancel, Dock.Right); taskHeading.Children.Add(applicationCancel);
        applicationProgress = new ProgressBar { Minimum = 0, Maximum = 1, Height = 5, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 12, 0) };
        taskHeading.Children.Add(applicationProgress); DockPanel.SetDock(taskHeading, Dock.Top); taskPanel.Children.Add(taskHeading);
        applicationResult = new TextBlock { Text = "未执行任务", TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 5) }; DockPanel.SetDock(applicationResult, Dock.Top); taskPanel.Children.Add(applicationResult);
        var tasks = ApplicationGrid(applicationTasks); TextColumn(tasks, "模拟器", nameof(ApplicationTaskRow.Serial), 120); TextColumn(tasks, "操作", nameof(ApplicationTaskRow.Operation), 72);
        TextColumn(tasks, "应用 / 文件", nameof(ApplicationTaskRow.Subject), 210); TextColumn(tasks, "状态", nameof(ApplicationTaskRow.State), 106);
        tasks.Columns.Add(new DataGridTextColumn { Header = "结果", Binding = new Binding(nameof(ApplicationTaskRow.Message)), IsReadOnly = true, Width = new DataGridLength(1, DataGridLengthUnitType.Star), MinWidth = 220 });
        taskPanel.Children.Add(tasks); DockPanel.SetDock(taskPanel, Dock.Bottom); body.Children.Add(taskPanel);
        applicationRowsView = CollectionViewSource.GetDefaultView(applicationRows);
        applicationRowsView.Filter = value => value is InstalledApplicationRow row && (applicationSystemToggle.IsChecked == true || !row.App.IsSystem);
        var applications = ApplicationGrid(applicationRowsView); var appSelection = ApplicationSelectionColumn(nameof(InstalledApplicationRow.IsSelected));
        var selectVisible = new CheckBox { ToolTip = "全选或清除可见应用", HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0) };
        selectVisible.Checked += (_, _) => { foreach (var row in applicationRowsView.Cast<InstalledApplicationRow>()) row.IsSelected = true; };
        selectVisible.Unchecked += (_, _) => { foreach (var row in applicationRowsView.Cast<InstalledApplicationRow>()) row.IsSelected = false; };
        appSelection.Header = selectVisible; applications.Columns.Add(appSelection);
        TextColumn(applications, "模拟器", nameof(InstalledApplicationRow.TargetName), 135); TextColumn(applications, "序列号", nameof(InstalledApplicationRow.Serial), 125);
        applications.Columns.Add(new DataGridTextColumn { Header = "包名", Binding = new Binding(nameof(InstalledApplicationRow.PackageName)), Width = new DataGridLength(1, DataGridLengthUnitType.Star), MinWidth = 190, IsReadOnly = true });
        TextColumn(applications, "名称", nameof(InstalledApplicationRow.Name), 150); TextColumn(applications, "版本", nameof(InstalledApplicationRow.Version), 95); TextColumn(applications, "类型", nameof(InstalledApplicationRow.Kind), 70); body.Children.Add(applications);
        foreach (var grid in new[] { applications, tasks })
            foreach (var column in grid.Columns.Where(column => column.Width.IsAbsolute)) column.MinWidth = column.Width.Value;
        ViewModel.Devices.CollectionChanged += (_, _) => SyncApplicationTargets();
        ViewModel.PropertyChanged += (_, _) => { SyncApplicationTargets(); RefreshApplicationControls(); if (!ViewModel.Busy && applicationReadPending && !applicationRunning) ScheduleApplicationRead(); };
        page.Loaded += (_, _) => SyncApplicationTargets(); page.IsVisibleChanged += (_, _) => { if (page.IsVisible && applicationReadPending) ScheduleApplicationRead(); };
        SyncApplicationTargets(); RefreshApplicationControls(); return page;
    }
    private static DataGrid ApplicationGrid(object source) => new() { ItemsSource = (System.Collections.IEnumerable)source, IsReadOnly = false, AutoGenerateColumns = false, CanUserAddRows = false, CanUserDeleteRows = false, SelectionMode = DataGridSelectionMode.Extended, HeadersVisibility = DataGridHeadersVisibility.Column };
    private static DataGridTemplateColumn ApplicationSelectionColumn(string selection, string? enabled = null)
    {
        var check = new FrameworkElementFactory(typeof(CheckBox)); check.SetBinding(ToggleButton.IsCheckedProperty, TwoWay(selection)); check.SetValue(FrameworkElement.MarginProperty, new Thickness(0));
        check.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center); check.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
        if (enabled is not null) check.SetBinding(UIElement.IsEnabledProperty, new Binding(enabled));
        return new DataGridTemplateColumn { Header = "选择", CellTemplate = new DataTemplate { VisualTree = check }, Width = 44, IsReadOnly = true };
    }
    private AsyncCommand ApplicationCommand(Func<Task> execute, Func<bool> enabled)
    {
        var command = new AsyncCommand(async () => { try { await execute(); } catch (Exception error) { applicationResult.Text = ViewModel.PresentError(error); } }, enabled);
        applicationCommands.Add(command); return command;
    }
    private Button ApplicationButton(string icon, string caption, Func<Task> execute, Func<bool> enabled) => Button(icon, caption, ApplicationCommand(execute, enabled));
    private bool ApplicationTargetProtected(string serial)
    {
        if (ViewModel.Demo) return false;
        var settings = ViewModel.Runtime.FrozenConnectionSettings; var bindings = settings.LdSerialBindings.Where(pair => pair.Value == serial).ToArray();
        return bindings.Length != 1 || bindings[0].Key <= 0 || bindings[0].Key == 99999 || !settings.AdbAllowedSerials.Contains(serial, StringComparer.Ordinal);
    }
    private void SyncApplicationTargets()
    {
        if (applicationSyncing || applicationSummary is null) return;
        applicationSyncing = true;
        try
        {
            foreach (var device in ViewModel.Devices)
            {
                if (applicationObservedDevices.Add(device)) device.PropertyChanged += (_, args) => { if (args.PropertyName == nameof(DeviceRow.Info)) SyncApplicationTargets(); };
                var row = applicationTargets.FirstOrDefault(target => target.Serial == device.Serial);
                if (row is null)
                {
                    row = new(device.Info, ApplicationTargetProtected(device.Serial));
                    row.PropertyChanged += (_, args) => { if (args.PropertyName == nameof(ApplicationTargetRow.IsSelected)) ApplicationTargetSelectionChanged(); }; applicationTargets.Add(row);
                }
                else row.Update(device.Info, ApplicationTargetProtected(device.Serial));
            }
            foreach (var row in applicationTargets.Where(row => !ViewModel.Devices.Any(device => device.Serial == row.Serial)).ToArray()) applicationTargets.Remove(row);
            foreach (var row in applicationRows.ToArray())
            {
                var target = applicationTargets.FirstOrDefault(target => target.IsSelected && target.Available && ApplicationTargetIdentity(ApplicationTarget.Freeze(target.Info)) == ApplicationTargetIdentity(row.Target));
                if (target is null) applicationRows.Remove(row);
                else row.UpdateTargetName(target.Info.Name);
            }
        }
        finally { applicationSyncing = false; }
        RefreshApplicationControls();
    }
    private void ApplicationTargetSelectionChanged()
    {
        if (applicationSyncing) return;
        foreach (var row in applicationRows.Where(row => !applicationTargets.Any(target => target.IsSelected && target.Serial == row.Serial)).ToArray()) applicationRows.Remove(row);
        applicationReadPending = applicationTargets.Any(row => row.IsSelected);
        if (applicationReadPending) ScheduleApplicationRead();
        else applicationReadTimer?.Stop();
        RefreshApplicationControls();
    }
    private void ScheduleApplicationRead()
    {
        if (closing || closeApproved || !applicationReadPending) { applicationReadTimer?.Stop(); return; }
        applicationReadTimer ??= new DispatcherTimer(TimeSpan.FromMilliseconds(350), DispatcherPriority.Background, async (_, _) =>
        {
            applicationReadTimer!.Stop();
            if (closing || closeApproved || !applicationReadPending || !applicationPage.IsVisible || applicationRunning || ViewModel.Busy) return;
            if (CanManageApplicationTargets())
            {
                applicationReadPending = false;
                try { await ReadApplicationListsAsync(); }
                catch (Exception error) { applicationResult.Text = ViewModel.PresentError(error); }
            }
        }, Dispatcher);
        applicationReadTimer.Stop(); applicationReadTimer.Start();
    }
    private ApplicationTarget[] SelectedApplicationTargets() => applicationTargets.Where(row => row.IsSelected).Select(row => ApplicationTarget.Freeze(row.Info)).ToArray();
    private static (string Serial, long Generation, uint? TransportId) ApplicationTargetIdentity(ApplicationTarget target) => (target.Serial, target.Generation, target.TransportId);
    private bool CanManageApplicationTargets() => !applicationRunning && !ViewModel.Busy && !ViewModel.Transferring && !ViewModel.Runtime.Api.HasActiveRequests
        && applicationTargets.Any(row => row.IsSelected) && applicationTargets.Where(row => row.IsSelected).All(row => row.Available && !ApplicationScriptBusy(ViewModel.Runtime, row.Serial));
    private bool CanManageApplicationRows() => CanManageApplicationTargets() && applicationRows.Any(row => row.IsSelected && (applicationSystemToggle.IsChecked == true || !row.App.IsSystem));
    private static bool ApplicationScriptBusy(MatrixRuntime runtime, string serial) => runtime.Scripts.GetStatus(serial).State is ScriptRunState.Running or ScriptRunState.Cancelling;
    private void RefreshApplicationControls()
    {
        if (applicationSummary is null) return;
        applicationSummary.Text = $"已选 {applicationTargets.Count(row => row.IsSelected)} 台 · {applicationRowsView.Cast<object>().Count()} 个应用";
        applicationTargetPanel.IsEnabled = !applicationRunning && !ViewModel.Busy; applicationSystemToggle.IsEnabled = !applicationRunning;
        foreach (var command in applicationCommands) command.Refresh();
    }
    private Task ReadApplicationListsAsync() => RunApplicationPlanAsync(SelectedApplicationTargets().Select(target => new ApplicationBatchItem(target, ApplicationOperation.List)).ToArray());
    private async Task ChooseApplicationPackagesAsync()
    {
        var targets = SelectedApplicationTargets(); var chooser = new OpenFileDialog { Filter = "Android 应用 (*.apk)|*.apk", Multiselect = true, CheckFileExists = true };
        if (chooser.ShowDialog(this) != true) return;
        await RunApplicationPlanAsync(ApplicationBatch.InstallPlan(targets, chooser.FileNames), true);
    }
    private Task ChangeSelectedApplicationsAsync(ApplicationOperation operation)
    {
        var rows = applicationRows.Where(row => row.IsSelected && (applicationSystemToggle.IsChecked == true || !row.App.IsSystem)).Select(row => new ApplicationBatchItem(row.Target with { Name = row.TargetName }, operation, row.PackageName, IsSystem: row.App.IsSystem)).ToArray();
        return RunApplicationPlanAsync(ApplicationBatch.RowPlan(operation, rows), operation is ApplicationOperation.Uninstall or ApplicationOperation.Clear);
    }
    private async Task RunApplicationPlanAsync(IReadOnlyList<ApplicationBatchItem> plan, bool askConfirmation = false)
    {
        if (!CanManageApplicationTargets()) throw new DeviceException("请明确勾选可用模拟器，并等待接口、脚本或传输任务完成。", 409, 1003);
        if (plan.Count == 0) throw new DeviceException("未选择应用任务，未操作设备。", 400, 1007);
        var runtime = ViewModel.Runtime; var frozenSelection = SelectedApplicationTargets();
        foreach (var item in plan)
        {
            if (!frozenSelection.Select(ApplicationTargetIdentity).Contains(ApplicationTargetIdentity(item.Target))) throw new DeviceException($"应用行不属于本次目标：{item.Target.Serial}。", 409, 1003);
            item.Target.RequireSame(runtime.GetDevices().SingleOrDefault(device => device.Serial == item.Target.Serial));
        }
        var confirmed = !askConfirmation || ConfirmApplicationPlan(plan);
        if (!confirmed) { applicationResult.Text = "已取消确认，未操作任何模拟器。"; return; }
        if (!CanManageApplicationTargets() || !ReferenceEquals(runtime, ViewModel.Runtime) || !frozenSelection.Select(ApplicationTargetIdentity).ToHashSet().SetEquals(SelectedApplicationTargets().Select(ApplicationTargetIdentity)))
            throw new DeviceException("确认期间工作区或勾选目标已变化，未发送命令。", 409, 1003);
        applicationReadTimer?.Stop(); applicationReadPending = false; applicationTasks.Clear(); foreach (var item in plan) applicationTasks.Add(new(item));
        applicationCancellation = new(); applicationRunning = true; applicationCancel.IsEnabled = true; RefreshApplicationControls();
        try
        {
            await ViewModel.ExecuteAsync("应用批次", async () =>
            {
                async Task Validate(ApplicationTarget target, CancellationToken token)
                {
                    await Dispatcher.InvokeAsync(() => RequireApplicationContext(runtime, target));
                    await runtime.Devices.RefreshAsync(token);
                    await Dispatcher.InvokeAsync(() => RequireApplicationContext(runtime, target));
                }
                var batch = new ApplicationBatch(runtime.Backend, Validate);
                var results = await batch.ExecuteAsync(plan, true, result => Dispatcher.Invoke(() =>
                {
                    applicationTasks[result.Index].SetResult(result);
                    if (result.Applications is not null && ReferenceEquals(runtime, ViewModel.Runtime)) ReplaceApplicationRows(result.Item.Target, result.Applications);
                    applicationProgress.Value = (double)applicationTasks.Count(row => row.Terminal) / applicationTasks.Count; applicationResult.Text = ApplicationBatchSummary(applicationTasks);
                    runtime.Log.Write("application-batch-item", JsonSerializer.Serialize(new { result.Index, result.Item.Target.Serial, result.Item.Operation, result.Item.PackageName, result.State, result.Message }, JsonDefaults.Options), result.Item.Target.Serial);
                }), applicationCancellation.Token);
                if (results.Any(result => result.State == ApplicationTaskState.Failed)) throw new DeviceException(applicationResult.Text + "；请查看逐项结果。", 409, 1003);
            });
        }
        finally
        {
            applicationCancellation.Dispose(); applicationCancellation = null; applicationRunning = false; applicationCancel.IsEnabled = false; RefreshApplicationControls();
        }
    }
    private void RequireApplicationContext(MatrixRuntime runtime, ApplicationTarget target)
    {
        if (!ReferenceEquals(runtime, ViewModel.Runtime) || ViewModel.Transferring || runtime.Api.HasActiveRequests || ApplicationTargetProtected(target.Serial) || ApplicationScriptBusy(runtime, target.Serial)
            || !SelectedApplicationTargets().Select(ApplicationTargetIdentity).Contains(ApplicationTargetIdentity(target)))
            throw new DeviceException($"工作区或并行任务已变化，未继续操作：{target.Serial}。", 409, 1003);
        target.RequireSame(ViewModel.Devices.SingleOrDefault(row => row.Serial == target.Serial)?.Info); target.RequireSame(runtime.GetDevices().SingleOrDefault(device => device.Serial == target.Serial));
    }
    private void ReplaceApplicationRows(ApplicationTarget target, IReadOnlyList<AppInfo> applications)
    {
        var identity = ApplicationTargetIdentity(target);
        var current = applicationTargets.FirstOrDefault(row => row.IsSelected && row.Available && ApplicationTargetIdentity(ApplicationTarget.Freeze(row.Info)) == identity);
        if (current is null) return;
        var selected = applicationRows.Where(row => ApplicationTargetIdentity(row.Target) == identity && row.IsSelected).Select(row => row.PackageName).ToHashSet(StringComparer.Ordinal);
        foreach (var row in applicationRows.Where(row => ApplicationTargetIdentity(row.Target) == identity).ToArray()) applicationRows.Remove(row);
        foreach (var application in applications.OrderBy(app => app.PackageName, StringComparer.Ordinal))
        {
            var row = new InstalledApplicationRow(target with { Name = current.Info.Name }, application) { IsSelected = selected.Contains(application.PackageName) };
            row.PropertyChanged += (_, _) => RefreshApplicationControls(); applicationRows.Add(row);
        }
        RefreshApplicationControls();
    }
    private bool ConfirmApplicationPlan(IReadOnlyList<ApplicationBatchItem> plan)
    {
        var operation = plan[0].Operation; var title = ApplicationOperationCaption(operation);
        var details = string.Join("\n", plan.Select(item => $"{item.Target.Name} ({item.Target.Serial})\n  {item.PackageName ?? Path.GetFileName(item.LocalPath)}"));
        var warning = operation switch
        {
            ApplicationOperation.Uninstall or ApplicationOperation.Clear => "此操作会删除对应应用数据，不可撤销。",
            ApplicationOperation.Install => "已有相同应用可能被覆盖安装；安装后仅刷新列表，不声明 APK 包名已核验。",
            ApplicationOperation.Start => "将向下列应用发送启动命令；命令完成不等于已核验实际运行效果。",
            ApplicationOperation.Stop => "将强制停止下列应用，可能中断未保存的操作；命令完成不等于已核验实际停止效果。",
            _ => "将读取下列模拟器的应用列表。"
        };
        var message = $"共 {plan.Count} 项，{plan.Select(item => item.Target.Serial).Distinct().Count()} 台模拟器。\n{warning}\n\n{details}";
        if (applicationConfirmationFixture is not null) return applicationConfirmationFixture(title, message);
        var dialog = new Window { Owner = this, Title = title, Width = 560, Height = 460, MinWidth = 380, MinHeight = 280, WindowStartupLocation = WindowStartupLocation.CenterOwner, ShowInTaskbar = false };
        var panel = new DockPanel { Margin = new Thickness(16) }; var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 12, 0, 0) };
        var cancel = new Button { Content = "取消", IsCancel = true, MinWidth = 78 }; var accept = new Button { Content = "确认执行", IsDefault = true, MinWidth = 92 };
        accept.Click += (_, _) => dialog.DialogResult = true; buttons.Children.Add(cancel); buttons.Children.Add(accept); DockPanel.SetDock(buttons, Dock.Bottom); panel.Children.Add(buttons);
        panel.Children.Add(new TextBox { Text = message, IsReadOnly = true, TextWrapping = TextWrapping.Wrap, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, AcceptsReturn = true }); dialog.Content = panel; return ShowOwnedDialog(dialog, 560, 460) == true;
    }
    private static string ApplicationOperationCaption(ApplicationOperation operation) => operation switch { ApplicationOperation.List => "读取应用", ApplicationOperation.Install => "安装应用", ApplicationOperation.Uninstall => "卸载应用", ApplicationOperation.Start => "启动应用", ApplicationOperation.Stop => "停止应用", _ => "清除数据" };
    private static string ApplicationBatchSummary(IEnumerable<ApplicationTaskRow> rows) => $"进度 {rows.Count(row => row.Terminal)}/{rows.Count()} · 已完成 {rows.Count(row => row.ResultState == ApplicationTaskState.Succeeded)} · 安装命令完成（包名未核验）{rows.Count(row => row.ResultState == ApplicationTaskState.CompletedUnverified)} · 失败 {rows.Count(row => row.ResultState == ApplicationTaskState.Failed)} · 取消 {rows.Count(row => row.ResultState == ApplicationTaskState.Cancelled)}";
}
