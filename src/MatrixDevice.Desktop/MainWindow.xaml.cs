using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MatrixDevice.Core;
using Microsoft.Win32;

namespace MatrixDevice.Desktop;

public partial class MainWindow : Window
{
    private readonly Dictionary<string, Process> screenProcesses = [];
    private readonly List<NativeWindowHost> nativeHosts = [];
    private readonly Dictionary<NodeRow, TreeViewItem> nodeTreeItems = [];
    private readonly Dictionary<NodeRow, Rectangle> nodeRectangles = [];
    private readonly DispatcherTimerAdapter captureTimer;
    private readonly DispatcherTimerAdapter directoryTimer;
    private TabControl tabs = null!;
    private TreeView nodeTree = null!;
    private Canvas nodeCanvas = null!;
    private ScrollViewer screenshotScroll = null!;
    private Image screenshotImage = null!;
    private Canvas screenshotCanvas = null!;
    private Rectangle cropRectangle = null!;
    private ComboBox screenshotMode = null!;
    private ScaleTransform screenshotScale = new(1, 1);
    private Int32Rect? crop;
    private Point? cropStart;
    private Point? gestureStart;
    private DeviceRow? gestureDevice;
    private long gestureGeneration;
    private long gestureDeviceGeneration;
    private IReadOnlyList<string> gestureTargets = [];
    private readonly Dictionary<string, long> gestureTargetGenerations = [];
    private int shellHistoryCursor = -1;
    private bool closing;
    private bool closeApproved;
    private bool captureInProgress;
    private Task? captureOperation;
    private bool debugAutoLoading;
    private bool debugAutoPending;
    public WorkspaceViewModel ViewModel { get; }

    public MainWindow(WorkspaceViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = viewModel;
        LocalizationService.Current.SetLanguage(viewModel.Runtime.Settings.Language);
        Shell.Background = new SolidColorBrush(Color.FromRgb(247, 249, 252));
        Language = System.Windows.Markup.XmlLanguage.GetLanguage(viewModel.Runtime.Settings.Language);
        Width = viewModel.Runtime.Settings.WindowWidth;
        Height = viewModel.Runtime.Settings.WindowHeight;
        Title = viewModel.Demo ? "矩阵设备 · 演示模式" : "矩阵设备";
        viewModel.DeviceChanged = () => { RefreshScreens(); _ = EnsureDebugLoadedAsync(); };
        viewModel.NodesChanged = RebuildNodes;
        viewModel.NodeSelectionChanged = HighlightNode;
        viewModel.WallChanged = RefreshWall;
        BuildShell();
        LocalizationService.Current.LanguageChanged += OnLanguageChanged;
        captureTimer = new DispatcherTimerAdapter(TimeSpan.FromSeconds(5), CaptureTickAsync);
        directoryTimer = new DispatcherTimerAdapter(TimeSpan.FromSeconds(5), UpdateDeviceDirectoryAsync);
        if (!viewModel.Demo) { directoryTimer.Start(); captureTimer.Start(); }
        Closing += WindowClosing;
        Deactivated += async (_, _) =>
        {
            gestureStart = null; gestureDevice = null;
            if (closing || closeApproved) return;
            try { await ViewModel.Runtime.Input.ReleaseAsync(); }
            catch (Exception ex) { await ViewModel.ExecuteAsync("释放输入", () => Task.FromException(ex)); }
        };
        viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(WorkspaceViewModel.DebugImage)) RefreshScreens();
            if (args.PropertyName == nameof(WorkspaceViewModel.Busy) && !ViewModel.Busy && debugAutoPending) _ = EnsureDebugLoadedAsync();
        };
    }

    private void BuildShell()
    {
        BuildWorkspaceLayout();
        tabs = new TabControl { Background = Brushes.White, BorderThickness = new Thickness(0), Template = WorkspaceContentTemplate() };
        InitializeUnifiedDevices();
        AddTab("设备墙 / 主控", BuildWallPage());
        AddTab("节点", BuildNodesPage());
        AddTab("截图 / 取色", BuildScreenshotPage());
        AddTab("Shell", BuildShellPage());
        AddTab("应用", BuildAppsPage());
        AddTab("文件", BuildFilesPage());
        AddTab("文本", BuildTextPage());
        AddTab("脚本", BuildScriptPage());
        AddTab("API", BuildApiPage());
        AddTab("扩展", BuildExtensionsPage());
        AddTab("设置", BuildSettingsPage());
        AddTab("实时监控", BuildMonitorPage());
        tabs.SelectionChanged += (_, args) => { if (ReferenceEquals(args.Source, tabs)) { UpdateNavigation(); RebuildNodes(); MonitorNavigationChanged(); _ = EnsureDebugLoadedAsync(); } };
        AttachWorkspacePages();
        NavigateTo("设备墙 / 主控");
    }












    private void AddTab(string title, FrameworkElement content)
    {
        System.Windows.Documents.TextElement.SetForeground(content, WorkspaceBrush("#182239"));
        System.Windows.Documents.TextElement.SetFontWeight(content, FontWeights.Normal);
        var tab = new TabItem { Content = content, Tag = title };
        LocalizationService.Bind(tab, HeaderedContentControl.HeaderProperty, title);
        tabs.Items.Add(tab);
    }
    private static DockPanel Page() => new() { Margin = new Thickness(14), LastChildFill = true };
    private static WrapPanel Toolbar() => new() { Orientation = Orientation.Horizontal };
    private static TextBlock Label(string text)
    {
        var label = new TextBlock { FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 6, 0, 7) };
        LocalizationService.Bind(label, TextBlock.TextProperty, text);
        return label;
    }
    private static TextBlock BoundText(string binding, bool wrap = false) { var text = new TextBlock { VerticalAlignment = VerticalAlignment.Center, TextWrapping = wrap ? TextWrapping.Wrap : TextWrapping.NoWrap }; text.SetBinding(TextBlock.TextProperty, new Binding(binding)); return text; }
    private static Binding TwoWay(string path) => new(path) { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, ValidatesOnExceptions = true, ValidatesOnDataErrors = true };
    private static TextBox BoundInput(string binding, bool readOnly = false, bool multiline = false) { var input = new TextBox { IsReadOnly = readOnly, AcceptsReturn = multiline, TextWrapping = multiline ? TextWrapping.Wrap : TextWrapping.NoWrap, VerticalScrollBarVisibility = multiline ? ScrollBarVisibility.Auto : ScrollBarVisibility.Hidden, VerticalContentAlignment = multiline ? VerticalAlignment.Top : VerticalAlignment.Center }; input.SetBinding(TextBox.TextProperty, readOnly ? new Binding(binding) { Mode = BindingMode.OneWay } : TwoWay(binding)); return input; }
    private static CheckBox Check(string text, string binding) { var checkbox = new CheckBox(); LocalizationService.Bind(checkbox, ContentControl.ContentProperty, text); checkbox.SetBinding(ToggleButton.IsCheckedProperty, TwoWay(binding)); return checkbox; }
    private static FrameworkElement Field(string label, FrameworkElement control, double labelWidth = 155)
    {
        var row = new Grid { Margin = new Thickness(0, 0, 0, 9) }; row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(labelWidth) }); row.ColumnDefinitions.Add(new ColumnDefinition()); var text = new TextBlock { VerticalAlignment = VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap }; LocalizationService.Bind(text, TextBlock.TextProperty, label); row.Children.Add(text); Grid.SetColumn(control, 1); row.Children.Add(control); return row;
    }
    private static Button Button(string glyph, string title, ICommand command)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal }; panel.Children.Add(new TextBlock { Text = glyph, FontFamily = new FontFamily("Segoe MDL2 Assets"), Margin = new Thickness(0, 0, 6, 0), VerticalAlignment = VerticalAlignment.Center }); var caption = new TextBlock { VerticalAlignment = VerticalAlignment.Center }; LocalizationService.Bind(caption, TextBlock.TextProperty, title); panel.Children.Add(caption); var button = new Button { Content = panel, Command = command }; LocalizationService.Bind(button, ToolTipProperty, title); return button;
    }
    private Button Button(string glyph, string title, Action action) => Button(glyph, title, new ActionCommand(action));
    private Button Button(string glyph, string title, Func<Task> action, bool write = false, Func<bool>? canExecute = null)
    {
        var command = new AsyncCommand(async () => { try { await action(); } catch (Exception ex) { MessageBox.Show(this, ViewModel.PresentError(ex), title, MessageBoxButton.OK, MessageBoxImage.Error); } }, canExecute ?? (write ? ViewModel.CanWriteDevice : () => !ViewModel.Busy));
        ViewModel.PropertyChanged += (_, _) => command.Refresh(); return Button(glyph, title, command);
    }
    private static void TextColumn(DataGrid grid, string header, string path, double width, bool readOnly = true)
    {
        BindingBase binding = readOnly ? new Binding(path) : TwoWay(path);
        if (path is nameof(DeviceRow.ApiAddress) or nameof(DeviceRow.ScriptState) or nameof(DeviceRow.State))
        {
            var stateBinding = new MultiBinding { Converter = new StateCaptionConverter() };
            stateBinding.Bindings.Add(new Binding(path)); stateBinding.Bindings.Add(new Binding(nameof(LocalizationService.Language)) { Source = LocalizationService.Current });
            binding = stateBinding;
        }
        var column = new DataGridTextColumn { Binding = binding, Width = width, IsReadOnly = readOnly };
        LocalizationService.Bind(column, DataGridColumn.HeaderProperty, header);
        grid.Columns.Add(column);
    }
    private static DataTemplate CaptionChoices()
    {
        var caption = new FrameworkElementFactory(typeof(TextBlock));
        var binding = new MultiBinding { Converter = new CaptionChoiceConverter() };
        binding.Bindings.Add(new Binding("."));
        binding.Bindings.Add(new Binding(nameof(LocalizationService.Language)) { Source = LocalizationService.Current });
        caption.SetBinding(TextBlock.TextProperty, binding);
        return new DataTemplate { VisualTree = caption };
    }
    private void OnLanguageChanged(object? sender, EventArgs args)
    {
        Language = System.Windows.Markup.XmlLanguage.GetLanguage(LocalizationService.Current.Language);
        ViewModel.Settings.Language = LocalizationService.Current.Language;
        ViewModel.Settings.Changed(nameof(SettingsEditor.Language));
        Title = LocalizationService.Current.Get("矩阵设备");
        foreach (var row in ViewModel.Devices) row.Changed(nameof(DeviceRow.FrameLabel));
    }

    private void RefreshWall() { UpdateUnifiedDirectory(); }
    private void RefreshScreens()
    {
        if (screenshotImage is null) return;
        screenshotImage.Source = ViewModel.DebugImage;
        if (ViewModel.DebugImage is { } image)
        {
            screenshotImage.Width = screenshotCanvas.Width = image.PixelWidth;
            screenshotImage.Height = screenshotCanvas.Height = image.PixelHeight;
        }
        else { screenshotImage.Width = screenshotCanvas.Width = 0; screenshotImage.Height = screenshotCanvas.Height = 0; }
        crop = null; cropStart = null; cropRectangle.Visibility = Visibility.Collapsed;
        RebuildNodes();
    }





    private async Task CaptureTickAsync()
    {
        if (captureInProgress || closing || ldBusy || ViewModel.HasActiveOperations || directoryOperation is { IsCompleted: false }
            || !Equals((tabs.SelectedItem as TabItem)?.Tag, "设备墙 / 主控") || !IsActive
            || wallMainPanel.Visibility != Visibility.Visible || ViewModel.MainDevice is not { Info.Online: true } main) return;
        var targets = new[] { main.Serial };
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(directoryCancellation.Token); deadline.CancelAfter(TimeSpan.FromSeconds(20));
        captureInProgress = true;
        try { captureOperation = ViewModel.RefreshAutomaticScreensAsync(targets, deadline.Token); await captureOperation; }
        catch (OperationCanceledException) { }
        finally { captureInProgress = false; }
    }

    private async Task EnsureDebugLoadedAsync()
    {
        if (ViewModel.Demo || closing || ViewModel.HasActiveOperations || !Equals((tabs?.SelectedItem as TabItem)?.Tag, "节点") || ViewModel.CurrentDevice?.Info.Online != true || ViewModel.Nodes.Count != 0) return;
        if (debugAutoLoading || ViewModel.Busy) { debugAutoPending = true; return; }
        var requested = ViewModel.CurrentDevice;
        debugAutoPending = false;
        debugAutoLoading = true;
        try { await ViewModel.LoadNodesAsync(); }
        finally
        {
            debugAutoLoading = false;
            if (debugAutoPending || !ReferenceEquals(requested, ViewModel.CurrentDevice)) { debugAutoPending = false; await EnsureDebugLoadedAsync(); }
        }
    }

    private async void WindowClosing(object? sender, CancelEventArgs e)
    {
        if (closeApproved) return;
        e.Cancel = true;
        if (closing) return;
        if (ldBusy || ViewModel.Busy) { MessageBox.Show(this, "当前操作尚未结束，请等待操作完成后退出。", "操作进行中", MessageBoxButton.OK, MessageBoxImage.Information); return; }
        if (ViewModel.HasActiveOperations && !ViewModel.Confirm("退出矩阵设备", "仍有脚本或文件传输正在运行。停止这些任务并退出？\n设备与模拟器将保持运行。")) return;
        closing = true;
        directoryCancellation.Cancel();
        directoryTimer.Stop(); captureTimer.Stop();
        await StopMonitorAsync();
        if (directoryOperation is not null) { try { await directoryOperation; } catch (OperationCanceledException) { } }
        if (captureOperation is not null) { try { await captureOperation; } catch (OperationCanceledException) { } }
        try
        {
            scanCancellation?.Cancel();
            if (scanOperation is not null) { try { await scanOperation; } catch (OperationCanceledException) { } }
            foreach (var host in nativeHosts.ToList()) host.Restore();
            if (ldWindowVisibility is not null) await ldWindowVisibility.RestoreAllAsync();
            var saved = ViewModel.Runtime.Settings with { WindowWidth = WindowState == WindowState.Normal ? ActualWidth : RestoreBounds.Width, WindowHeight = WindowState == WindowState.Normal ? ActualHeight : RestoreBounds.Height, WallColumns = ViewModel.WallColumns, SyncMouse = ViewModel.SyncMouse, SyncKeyboard = ViewModel.SyncKeyboard };
            ViewModel.Runtime.Settings = ViewModel.Runtime.SettingsStore.Save(saved);
            await ViewModel.ShutdownAsync();
            LocalizationService.Current.LanguageChanged -= OnLanguageChanged;
            foreach (var process in screenProcesses.Values) { if (!process.HasExited) { process.Kill(entireProcessTree: true); await process.WaitForExitAsync(); } process.Dispose(); }
            closeApproved = true; Close();
        }
        catch (Exception ex) { closing = false; MessageBox.Show(this, "退出未完成：" + ViewModel.PresentError(ex) + "\n投屏恢复记录已保留。", "退出失败", MessageBoxButton.OK, MessageBoxImage.Error); }
    }

    internal void AbortStartup()
    {
        closing = closeApproved = true;
        captureTimer?.Stop();
        directoryTimer?.Stop(); directoryCancellation.Cancel();
        monitorTimer.Stop();
        scanCancellation?.Cancel();
        Close();
    }

    private void ShowText(string title, string text)
    {
        var window = new Window { Owner = this, Title = title, Width = 700, Height = 500, Content = new TextBox { Text = text, IsReadOnly = true, AcceptsReturn = true, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, FontFamily = new FontFamily("Consolas"), Padding = new Thickness(12) }, WindowStartupLocation = WindowStartupLocation.CenterOwner }; window.Show();
    }

    public async Task RunSmokeAsync(string? screenshotPath = null)
    {
        if (!ViewModel.Demo) throw new InvalidOperationException("启动验收仅允许显式演示模式");
        if (tabs.SelectedIndex != 0 || wallViewMode != "list") throw new InvalidOperationException("初始页面必须是统一设备列表。");
        await VerifyListConsoleSimplificationAsync();
        if (screenshotPath is not null)
        {
            if (!System.IO.Path.IsPathFullyQualified(screenshotPath)) throw new InvalidOperationException("截图输出必须为绝对路径");
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(screenshotPath)!);
        }
        await ViewModel.RefreshWallAsync();
        if (ViewModel.CurrentDevice is { } row) await ViewModel.SetMainAsync(row);
        await VerifyShellSimplificationAsync();
        await ViewModel.LoadNodesAsync();
        ViewModel.SelectedNode = ViewModel.FlattenNodes().FirstOrDefault(node => node.Source.Clickable);
        await ViewModel.CaptureAsync();
        if (ViewModel.Nodes.Count != 0 || ViewModel.SelectedNode is not null)
            throw new InvalidOperationException("单独刷新截图后仍保留旧节点覆盖框与选中节点");
        if (nodeTree.Items.Count != 0 || nodeRectangles.Count != 0) throw new InvalidOperationException("旧节点视觉覆盖未清空");
        await ViewModel.LoadNodesAsync();
        ViewModel.SelectedNode = ViewModel.FlattenNodes().FirstOrDefault(node => node.Source.Clickable);
        await ViewModel.RefreshWallAsync();
        if (ViewModel.Nodes.Count != 0 || ViewModel.SelectedNode is not null)
            throw new InvalidOperationException("设备墙刷新截图后仍保留旧节点");
        await ViewModel.LoadNodesAsync();
        ViewModel.SelectedNode = ViewModel.FlattenNodes().FirstOrDefault(node => node.Source.Clickable);
        await ViewModel.LoadAppsAsync();
        await VerifyOperationGatesAsync();
        await VerifyOneClickProjectionAsync();
        await VerifyCanonicalDeviceActionsAsync(screenshotPath);
        await VerifyApplicationManagementAsync(screenshotPath);
        await VerifyLaunchMemoryUiAsync();
        await LdWindowVisibilitySmoke.VerifyAsync(System.IO.Path.Combine(ViewModel.DataDirectory, "window-visibility-smoke"));
        ViewModel.SamplePixel(10, 10);
        ViewModel.ShellInput = "wm size"; await ViewModel.ExecuteShellAsync();
        var acceptanceDirectory = ViewModel.DataDirectory;
        var scriptPath = System.IO.Path.Combine(acceptanceDirectory, "desktop-smoke.jxscript");
        MatrixDevice.Features.ScriptPackageService.Save(scriptPath, new MatrixDevice.Features.ScriptPackage
        {
            Manifest = new MatrixDevice.Features.ScriptManifest { Id = "desktop-smoke", Name = "桌面演示验收", Version = "1.0" },
            Steps = [new MatrixDevice.Features.ScriptStep { Action = "wait", DurationMs = 20 }, new MatrixDevice.Features.ScriptStep { Action = "click", Value = "80,240" }],
            Parameters = [new MatrixDevice.Features.ScriptParameter { Key = "limit", Type = "integer", Min = 1, Max = 100, DefaultValue = JsonSerializer.SerializeToElement(2) }]
        });
        await ViewModel.ImportScriptAsync(scriptPath);
        ViewModel.Parameters.Single(parameter => parameter.Key == "limit").Value = "3";
        await ViewModel.StartScriptAsync();
        var deadline = DateTime.UtcNow.AddSeconds(5);
        while (ViewModel.Runtime.Scripts.GetStatus(ViewModel.CurrentDevice!.Serial).State is MatrixDevice.Features.ScriptRunState.Running or MatrixDevice.Features.ScriptRunState.Cancelling)
        {
            if (DateTime.UtcNow > deadline) throw new TimeoutException("演示脚本验收未在期限内结束");
            await Task.Delay(20);
        }
        if (ViewModel.Runtime.Scripts.GetStatus(ViewModel.CurrentDevice!.Serial).State != MatrixDevice.Features.ScriptRunState.Completed) throw new InvalidOperationException("演示脚本未成功完成");
        var source = ViewModel.CurrentDevice;
        ViewModel.CurrentDevice = ViewModel.Devices.First(device => !ReferenceEquals(device, source));
        ViewModel.CurrentDevice = source;
        if (ViewModel.Parameters.Single(parameter => parameter.Key == "limit").Value != "3") throw new InvalidOperationException("逐设备脚本参数未恢复");
        await ViewModel.LoadNodesAsync();
        ViewModel.SelectedNode = ViewModel.FlattenNodes().FirstOrDefault(node => node.Source.Clickable);
        await ViewModel.LoadAppsAsync();
        LocalizationService.Current.SetLanguage("zh-CN");
        await Dispatcher.InvokeAsync(UpdateLayout, System.Windows.Threading.DispatcherPriority.Render);
        var settingsTab = tabs.Items.Cast<TabItem>().Single(tab => Equals(tab.Tag, "设置"));
        if ((tabs.Items[0] as TabItem)?.Header?.ToString() != "设备墙 / 主控" || settingsTab.Header?.ToString() != "设置") throw new InvalidOperationException("简体中文导航未生效");
        tabs.SelectedItem = settingsTab;
        await Dispatcher.InvokeAsync(UpdateLayout, System.Windows.Threading.DispatcherPriority.Render);
        var labels = VisualDescendants<TextBlock>(Shell).Select(text => text.Text).ToHashSet(StringComparer.Ordinal);
        foreach (var expected in new[] { "普通接口端口", "界面语言", "连接与接口", "雷电命令行路径", "简体中文" })
            if (!labels.Contains(expected)) throw new InvalidOperationException("中文设置字段缺失：" + expected);
        var unsupportedFields = VisualDescendants<TextBox>(Shell).Where(input => BindingOperations.GetBinding(input, TextBox.TextProperty)?.Path.Path is "Settings.MiniMaxSize" or "Settings.MiniMaxFps" or "Settings.MiniBitRate").ToArray();
        if (unsupportedFields.Length != 3 || unsupportedFields.Any(input => input.IsEnabled)) throw new InvalidOperationException("未实现的小屏视频参数必须禁用");
        var hardwareField = VisualDescendants<CheckBox>(Shell).Single(input => BindingOperations.GetBinding(input, ToggleButton.IsCheckedProperty)?.Path.Path == "Settings.ForceHardwareDecode");
        if (hardwareField.IsEnabled) throw new InvalidOperationException("未实现的硬件解码控件必须禁用");
        await RefreshLdPlayerUiAsync();
        if (ldRows.Count != 2) throw new InvalidOperationException("演示雷电实例目录不完整");
        if (tabs.Items.Count != WorkspacePages.Length) throw new InvalidOperationException("桌面页面数量不完整");
        if (navigation.Count != WorkspacePages.Length || navigation.ContainsKey("雷电实例") || deviceSummaries.Count != 1 || tabs.Items.Cast<TabItem>().Any(item => Equals(item.Tag, "雷电实例"))) throw new InvalidOperationException("设备管理统一入口不完整");
        await VerifyMonitorProjectionAsync(screenshotPath);
        NavigateTo("设备墙 / 主控");
        var removedLabels = new[] { "刷新设备", "扫描设备", "刷新截图" };
        if (VisualDescendants<TextBlock>(Shell).Any(text => removedLabels.Contains(text.Text))) throw new InvalidOperationException("旧重复刷新入口仍可见");
        ClearWallSelection();
        unifiedDevices.Single(row => row.Index == ldRows.Single(item => !item.Instance.AndroidStarted).Index).IsSelected = true;
        var runningOnly = new[] { "关闭实例", "重启实例" };
        if (!ldCommandsByLabel["资源配置"].CanExecute(null) || runningOnly.Any(label => ldCommandsByLabel[label].CanExecute(null))) throw new InvalidOperationException("未启动实例的生命周期操作状态不正确");
        await ExecuteLdPlayerAsync("launch");
        if (ldCommandsByLabel["资源配置"].CanExecute(null) || runningOnly.Any(label => !ldCommandsByLabel[label].CanExecute(null))) throw new InvalidOperationException("已启动实例的生命周期操作状态不正确");
        ldBusy = true; ldFeedback.Text = "正在执行所选实例命令…"; RefreshLdButtons();
        if (ldCommands.Any(command => command.CanExecute(null)) || !ldFeedback.Text.Contains("正在执行", StringComparison.Ordinal)) throw new InvalidOperationException("雷电忙状态未锁定操作");
        ldBusy = false; RefreshLdButtons();
        var originalConfirm = ViewModel.Confirm;
        var confirmationCount = 0; var beforeConfirmation = ldSelection!.Generation;
        ViewModel.Confirm = (title, message) =>
        {
            if (!title.Contains("雷电实例", StringComparison.Ordinal) || !message.Contains("会话代次", StringComparison.Ordinal)) throw new InvalidOperationException("雷电确认缺少实例作用范围");
            confirmationCount++; return false;
        };
        try { await ExecuteLdPlayerAsync("quit"); await ExecuteLdPlayerAsync("reboot"); }
        finally { ViewModel.Confirm = originalConfirm; }
        if (confirmationCount != 2 || ldSelection.Generation != beforeConfirmation || !ldSelection.Instance.AndroidStarted || ldFeedback.Text != "操作已取消，未发送命令。") throw new InvalidOperationException("雷电关闭或重启的取消确认未生效");
        UpdateLdPlayerRows();
        var selectedInstance = unifiedDevices.Single(row => row.IsSelected);
        if (selectedInstance.Index != ldSelection.Index || selectedInstance.Instance?.Generation != ldSelection.Generation)
            throw new InvalidOperationException("目录勾选与真实命令目标不一致");
        ViewModel.Confirm = (_, _) => true;
        try { await ExecuteLdPlayerAsync("quit"); }
        finally { ViewModel.Confirm = originalConfirm; }
        await VerifyUnifiedDeviceActionsAsync(screenshotPath);
        var currentBeforeNavigation = ViewModel.CurrentDevice;
        var mainBeforeNavigation = ViewModel.MainDevice;
        ViewModel.ShellInput = "echo navigation-state"; ViewModel.TextInput = "跨页面保留输入";
        var selectedBeforeNavigation = ViewModel.Targets().Select(device => device.Serial).ToArray();
        crop = new Int32Rect(10, 10, 40, 40); cropRectangle.Visibility = Visibility.Visible;
        cropRectangle.Width = cropRectangle.Height = 40; Canvas.SetLeft(cropRectangle, 10); Canvas.SetTop(cropRectangle, 10);
        foreach (var route in WorkspacePages)
        {
            navigation[route.Tag].RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            if (!Equals((tabs.SelectedItem as TabItem)?.Tag, route.Tag) || navigation.Count(item => item.Value.IsChecked == true) != 1 || navigation[route.Tag].IsChecked != true)
                throw new InvalidOperationException("侧栏导航或选中态失效：" + route.Label);
        }
        if (!ReferenceEquals(currentBeforeNavigation, ViewModel.CurrentDevice) || !ReferenceEquals(mainBeforeNavigation, ViewModel.MainDevice) ||
            !selectedBeforeNavigation.SequenceEqual(ViewModel.Targets().Select(device => device.Serial)) || ViewModel.ShellInput != "echo navigation-state" || ViewModel.TextInput != "跨页面保留输入" ||
            crop != new Int32Rect(10, 10, 40, 40) || ViewModel.Parameters.Single(parameter => parameter.Key == "limit").Value != "3")
            throw new InvalidOperationException("跨页面导航丢失设备、目标、输入、裁剪或脚本参数");
        SetSidebarCollapsed(true);
        NavigateTo("设备墙 / 主控");
        if (sidebarColumn.Width.Value != 72 || sidebarCaptions.Any(caption => caption.Visibility != Visibility.Collapsed)) throw new InvalidOperationException("侧栏折叠未生效");
        SetSidebarCollapsed(false);
        ClearWallSelection();
        foreach (var rowToSelect in ViewModel.Devices) rowToSelect.IsSelected = true;
        ViewModel.UpdateSelection();
        var filteredDevice = ViewModel.Devices[1];
        var originalDeviceInfos = ViewModel.Devices.ToDictionary(device => device, device => device.Info);
        var previousStatus = ViewModel.StatusFilter;
        try
        {
            filteredDevice.Info = filteredDevice.Info with { State = "Offline" };
            foreach (var status in new[] { "Ready", "Offline", "全部状态" })
            {
                ViewModel.StatusFilter = status;
                await Dispatcher.InvokeAsync(UpdateLayout, System.Windows.Threading.DispatcherPriority.Render);
                if (!ReferenceEquals(ViewModel.CurrentDevice, currentBeforeNavigation) || !ReferenceEquals(ViewModel.MainDevice, mainBeforeNavigation) || ViewModel.Devices.Count(device => device.IsSelected) != 2)
                    throw new InvalidOperationException($"状态筛选改变了上下文：状态={status}，勾选={ViewModel.Devices.Count(device => device.IsSelected)}");
            }
            ViewModel.StatusFilter = "Offline";
            await Dispatcher.InvokeAsync(UpdateLayout, System.Windows.Threading.DispatcherPriority.Render);
            if (ViewModel.DeviceView.Cast<DeviceRow>().Count() != 1 || ViewModel.Devices.Count(device => device.IsSelected) != 2)
                throw new InvalidOperationException("状态筛选没有形成唯一可见设备或改变了群控勾选。");
            var selectAll = VisualDescendants<CheckBox>(Shell).Single(input => input.Name == "SelectAllDevices");
            selectAll.IsChecked = false; selectAll.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            if (ViewModel.Devices.Any(device => device.IsSelected) || unifiedDevices.Any(device => device.IsSelected))
                throw new InvalidOperationException("取消表头全选没有清除隐藏设备的选择。");
            foreach (var device in ViewModel.Devices) device.Info = device.Info with { State = "Ready" };
            ViewModel.StatusFilter = "Offline";
            await Dispatcher.InvokeAsync(UpdateLayout, System.Windows.Threading.DispatcherPriority.Render);
            if (ViewModel.DeviceView.Cast<DeviceRow>().Any() || !unifiedDeviceView.IsEmpty) throw new InvalidOperationException("空目录状态筛选失效。");
            if (screenshotPath is not null) SaveRender(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(screenshotPath)!, System.IO.Path.GetFileNameWithoutExtension(screenshotPath) + "-empty.png"));
        }
        finally
        {
            foreach (var pair in originalDeviceInfos) pair.Key.Info = pair.Value;
            ViewModel.StatusFilter = previousStatus;
        }
        if (ViewModel.DebugImage is not { PixelWidth: > 0 }) throw new InvalidOperationException("状态筛选丢失了当前截图。");
        SetWallViewMode("list");
        foreach (var unsupported in new[] { "cards", "screens" })
        {
            try { SetWallViewMode(unsupported); throw new InvalidOperationException("已移除的设备视图仍被接受：" + unsupported); }
            catch (ArgumentException) { }
        }
        if (WallViewMode != "list") throw new InvalidOperationException("设备管理未保持唯一列表模式。");
        await OpenMainControlAsync(mainBeforeNavigation!);
        var expandedControls = VisualDescendants<Expander>(wallMainPanel).Single(); expandedControls.IsExpanded = true;
        await Dispatcher.InvokeAsync(UpdateLayout, System.Windows.Threading.DispatcherPriority.Render);
        if (wallMainPanel is ScrollViewer mainScroll) mainScroll.ScrollToEnd();
        await Dispatcher.InvokeAsync(UpdateLayout, System.Windows.Threading.DispatcherPriority.Render);
        if (screenshotPath is not null) SaveRender(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(screenshotPath)!, System.IO.Path.GetFileNameWithoutExtension(screenshotPath) + "-expanded.png"));
        expandedControls.IsExpanded = false;
        CloseMainControl();
        ShowDeviceDirectory();
        var statusChoice = VisualDescendants<ComboBox>(directoryWindow!).Single(input => BindingOperations.GetBinding(input, Selector.SelectedItemProperty)?.Path.Path == nameof(WorkspaceViewModel.StatusFilter));
        statusChoice.IsDropDownOpen = true;
        await Dispatcher.InvokeAsync(UpdateLayout, System.Windows.Threading.DispatcherPriority.Render);
        if (statusChoice.Template.FindName("PART_Popup", statusChoice) is not Popup { IsOpen: true }) throw new InvalidOperationException("状态筛选下拉模板未打开。");
        statusChoice.SelectedItem = "Offline";
        if (ViewModel.StatusFilter != "Offline") throw new InvalidOperationException("状态筛选下拉绑定失效。");
        await Dispatcher.InvokeAsync(UpdateLayout, System.Windows.Threading.DispatcherPriority.Render);
        if (!ReferenceEquals(ViewModel.CurrentDevice, currentBeforeNavigation) || !ReferenceEquals(ViewModel.MainDevice, mainBeforeNavigation)) throw new InvalidOperationException("设备目录筛选改变了当前设备或主控");
        statusChoice.IsDropDownOpen = false; statusChoice.SelectedItem = previousStatus; directoryWindow!.Hide();
        ShowScanner();
        await Dispatcher.InvokeAsync(UpdateLayout, System.Windows.Threading.DispatcherPriority.Render);
        if (screenshotPath is not null) SaveRender(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(screenshotPath)!, System.IO.Path.GetFileNameWithoutExtension(screenshotPath) + "-scanner.png"), (FrameworkElement)scannerWindow!.Content);
        scannerWindow!.Hide();
        if (ViewModel.DebugImage is { } frame && (frame.PixelWidth < 1 || frame.PixelHeight < 1)) throw new InvalidOperationException("演示截图为空");
        if (screenshotPath is not null)
        {
            var directory = System.IO.Path.GetDirectoryName(screenshotPath)!; Directory.CreateDirectory(directory);
            await ViewModel.SaveScreenshotAsync(System.IO.Path.Combine(directory, "desktop-crop.png"), new Int32Rect(10, 10, 40, 40));
            await ViewModel.ExportNodesAsync(System.IO.Path.Combine(directory, "desktop-nodes.json"));
        }
        var controlStates = new List<object>();
        foreach (TabItem tab in tabs.Items)
        {
            NavigateTo(tab.Tag.ToString()!);
            await Dispatcher.InvokeAsync(() => { UpdateLayout(); }, System.Windows.Threading.DispatcherPriority.Render);
            var pageName = tab.Tag.ToString()!;
            controlStates.AddRange(CaptureControlStates(pageName, "idle", Shell));
            var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var busyOperation = ViewModel.ExecuteAsync("控件忙状态检查", () => release.Task);
            try
            {
                await Dispatcher.InvokeAsync(UpdateLayout, System.Windows.Threading.DispatcherPriority.Render);
                controlStates.AddRange(CaptureControlStates(pageName, "busy", Shell));
            }
            finally { release.SetResult(); await busyOperation; }
            var current = ViewModel.CurrentDevice!; var originalInfo = current.Info;
            try
            {
                current.Info = originalInfo with { State = "Offline" }; ViewModel.UpdateSelection();
                await Dispatcher.InvokeAsync(UpdateLayout, System.Windows.Threading.DispatcherPriority.Render);
                controlStates.AddRange(CaptureControlStates(pageName, "offline", Shell));
            }
            finally { current.Info = originalInfo; ViewModel.UpdateSelection(); }
            if (Equals(tab.Tag, "扩展"))
            {
                var settingsScroll = VisualDescendants<ScrollViewer>(Shell).Single(scroll => scroll.Name == "ExtensionSettingsScroll");
                settingsScroll.ScrollToEnd();
                await Dispatcher.InvokeAsync(UpdateLayout, System.Windows.Threading.DispatcherPriority.Render);
                var timezoneAction = VisualDescendants<Button>(settingsScroll).Single(button => button.ToolTip as string == "应用并核验时区");
                var point = timezoneAction.TransformToAncestor(settingsScroll).Transform(new Point());
                if (point.Y < 0 || point.Y + timezoneAction.ActualHeight > settingsScroll.ActualHeight + 2) throw new InvalidOperationException("扩展页时区按钮滚动后仍不可达");
            }
            if (screenshotPath is not null)
            {
                if (!System.IO.Path.IsPathFullyQualified(screenshotPath)) throw new InvalidOperationException("截图输出必须为绝对路径");
                var directory = System.IO.Path.GetDirectoryName(screenshotPath)!;
                Directory.CreateDirectory(directory);
                var output = System.IO.Path.Combine(directory, System.IO.Path.GetFileNameWithoutExtension(screenshotPath) + $"-tab{tabs.SelectedIndex}.png");
                SaveRender(output);
            }
        }
        if (screenshotPath is not null)
            await File.WriteAllTextAsync(System.IO.Path.ChangeExtension(screenshotPath, ".controls.json"), JsonSerializer.Serialize(controlStates, JsonDefaults.Options));
        NavigateTo("设备墙 / 主控");
        await Dispatcher.InvokeAsync(() => { }, System.Windows.Threading.DispatcherPriority.ContextIdle);
        await Dispatcher.InvokeAsync(() => { UpdateLayout(); }, System.Windows.Threading.DispatcherPriority.Render);
        var tiles = VisualDescendants<UnifiedDeviceTile>(Shell).Where(tile => tile.IsVisible).ToArray();
        if (screenshotPath is not null) await File.WriteAllTextAsync(System.IO.Path.ChangeExtension(screenshotPath, ".rows.json"), JsonSerializer.Serialize(tiles.Select(tile => new { view = "list", owner = tile.Owner is not null, context = tile.DataContext?.GetType().Name, child = tile.Child?.GetType().Name, tile.ActualWidth, tile.ActualHeight }), JsonDefaults.Options));
        if (!unifiedDeviceView.IsEmpty && tiles.Length == 0) throw new InvalidOperationException("非空统一目录没有生成可见列表操作行。");
        if (tiles.Any(tile => tile.Child is null || tile.ActualHeight < 1)) throw new InvalidOperationException("统一设备视图出现空白条目。");
        if (screenshotPath is not null) SaveRender(screenshotPath);
        var path = System.IO.Path.Combine(ViewModel.DataDirectory, "desktop-smoke.json");
        if (!string.IsNullOrWhiteSpace(ViewModel.Error)) throw new InvalidOperationException("桌面验收产生错误：" + ViewModel.Error);
        var report = JsonSerializer.Serialize(new { width = ActualWidth, height = ActualHeight, deviceCount = ViewModel.Devices.Count, ldPlayerInstances = ldRows.Count, ldStateButtonsVerified = true, ldBusyVerified = true, ldConfirmationCancellationVerified = confirmationCount == 2, navigationVerified = true, navigationStatePreserved = true, sidebarCollapseVerified = true, listOnlyVerified = WallViewMode == "list", groupControlsRemoved = true, selectionTargetUnified = true, filteredContextPreserved = true, clearHiddenSelectionVerified = true, emptyStateVerified = true, statusDropdownVerified = true, directoryFilterContextPreserved = true, nodes = ViewModel.FlattenNodes().Count(), selectedNode = ViewModel.SelectedNode?.Id, apps = ViewModel.Apps.Count, colors = ViewModel.Colors.Count, shell = ViewModel.ShellHistory.Count, script = LocalizationService.Current.Get(ViewModel.Runtime.Scripts.GetStatus(ViewModel.CurrentDevice!.Serial).State.ToString()), tabs = tabs.Items.Count, language = "zh-CN", chineseInterfaceVerified = true, unavailableControlsDisabled = true, error = ViewModel.Error }, JsonDefaults.Options);
        await File.WriteAllTextAsync(path, report);
        if (screenshotPath is not null) await File.WriteAllTextAsync(System.IO.Path.ChangeExtension(screenshotPath, ".json"), report);
    }

    private void SaveRender(string path)
    {
        foreach (var label in new[] { "环境设置", "设备详情" })
        {
            var control = VisualDescendants<Button>(Shell).Single(button => button.ToolTip as string == label);
            var position = control.TranslatePoint(new Point(), Shell);
            if (position.X < 0 || position.Y < 0 || position.X + control.ActualWidth > Shell.ActualWidth + 1 || position.Y + control.ActualHeight > Shell.ActualHeight + 1)
                throw new InvalidOperationException("截图前全局工具栏超出客户区：" + label);
        }
        SaveRender(path, Shell);
    }
    private static void SaveRender(string path, FrameworkElement surface)
    {
        var bitmap = new RenderTargetBitmap(Math.Max(1, (int)Math.Ceiling(surface.ActualWidth)), Math.Max(1, (int)Math.Ceiling(surface.ActualHeight)), 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(surface);
        var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap)); using var file = File.Create(path); encoder.Save(file);
    }
    private static IEnumerable<T> VisualDescendants<T>(DependencyObject source) where T : DependencyObject
    {
        if (source is T match) yield return match;
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(source); index++)
            foreach (var nested in VisualDescendants<T>(VisualTreeHelper.GetChild(source, index))) yield return nested;
    }
}

public sealed class NonEmptyVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) => string.IsNullOrWhiteSpace(value as string) ? Visibility.Collapsed : Visibility.Visible;
    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) => throw new NotSupportedException();
}

public sealed class DispatcherTimerAdapter
{
    private readonly System.Windows.Threading.DispatcherTimer timer;
    public DispatcherTimerAdapter(TimeSpan interval, Func<Task> tick)
    {
        timer = new System.Windows.Threading.DispatcherTimer { Interval = interval };
        timer.Tick += async (_, _) => await tick();
    }
    public void Start() => timer.Start();
    public void Stop() => timer.Stop();
}
