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
    private readonly Dictionary<string, ScrcpySession> screenProcesses = [];
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
    private Rect gestureImageBounds;
    private Size gestureImagePixels;
    private Image? gestureImage;
    private Task mainGestureOperation = Task.CompletedTask;
#if MATRIX_TEST_SUPPORT
    private InputSnapshot? lastMainGestureSnapshot;
    private long mainGestureDispatchCount;
    private long mainCaptureDispatchCount;
    private string[] lastMainCaptureTargets = [];
    private Func<Task>? demoMainCaptureBarrier;
#endif
    private double observedSettingsWindowWidth;
    private double observedSettingsWindowHeight;
    private int shellHistoryCursor = -1;
    private bool closing;
    private bool closeApproved;
    private bool captureInProgress;
    private Task? captureOperation;
    private bool debugAutoLoading;
    private bool debugAutoPending;
    private bool debugAutoQueued;
    private Task debugAutoOperation = Task.CompletedTask;
#if MATRIX_TEST_SUPPORT
    private Action<Window>? responsiveOwnedDialogFixture;
#endif
    public WorkspaceViewModel ViewModel { get; }

    public MainWindow(WorkspaceViewModel viewModel)
    {
        InitializeComponent();
        SetResourceReference(StyleProperty, typeof(Window));
        ViewModel = viewModel;
        DataContext = viewModel;
        LocalizationService.Current.SetLanguage(viewModel.Runtime.Settings.Language);
        Shell.Background = ThemeBrush("CanvasBrush");
        Language = System.Windows.Markup.XmlLanguage.GetLanguage(viewModel.Runtime.Settings.Language);
        Width = viewModel.Runtime.Settings.WindowWidth;
        Height = viewModel.Runtime.Settings.WindowHeight;
        observedSettingsWindowWidth = Width;
        observedSettingsWindowHeight = Height;
        viewModel.SettingsWindowSizeChanges = () =>
        {
            var size = SettingsWindowSize();
            return (size.Width != observedSettingsWindowWidth ? size.Width : null, size.Height != observedSettingsWindowHeight ? size.Height : null);
        };
        viewModel.SettingsWindowSizeSaved = (widthEdited, heightEdited) =>
        {
            if (widthEdited) Width = viewModel.Runtime.Settings.WindowWidth;
            if (heightEdited) Height = viewModel.Runtime.Settings.WindowHeight;
            observedSettingsWindowWidth = WindowState == WindowState.Normal ? Width : SettingsWindowSize().Width;
            observedSettingsWindowHeight = WindowState == WindowState.Normal ? Height : SettingsWindowSize().Height;
        };
        Title = viewModel.Demo ? "矩阵设备 · 演示模式" : "矩阵设备";
        viewModel.DeviceChanged = () => { RefreshScreens(); _ = EnsureDebugLoadedAsync(); };
        viewModel.CurrentDebugReadyChanged = () => _ = EnsureDebugLoadedAsync();
        viewModel.DebugSourceAdopted = source =>
        {
            if (viewModel.IsCurrentDebugSource(source)) debugAutoPending = false;
        };
        viewModel.NodesChanged = RebuildNodes;
        viewModel.NodeSelectionChanged = HighlightNode;
        viewModel.WallChanged = RefreshWall;
        BuildShell();
        SizeChanged += (_, _) => { CancelMainGesture(); ApplyResponsiveWorkspace(); };
        LocationChanged += (_, _) => CancelMainGesture();
        Loaded += (_, _) =>
        {
            var size = SettingsWindowSize();
            observedSettingsWindowWidth = size.Width;
            observedSettingsWindowHeight = size.Height;
            ApplyResponsiveWorkspace();
        };
        PreviewKeyDown += (_, args) => { if (args.Key == Key.Escape && navigationDrawerOpen) { CloseNavigationDrawer(); args.Handled = true; } };
        LocalizationService.Current.LanguageChanged += OnLanguageChanged;
        captureTimer = new DispatcherTimerAdapter(TimeSpan.FromSeconds(5), CaptureTickAsync);
        directoryTimer = new DispatcherTimerAdapter(TimeSpan.FromSeconds(5), UpdateDeviceDirectoryAsync);
        if (!viewModel.Demo) { directoryTimer.Start(); captureTimer.Start(); }
        Closing += WindowClosing;
        Deactivated += async (_, _) =>
        {
            CancelMainGesture();
            if (closing || closeApproved) return;
            try { await ViewModel.Runtime.Input.ReleaseAsync(); }
            catch (Exception ex) { await ViewModel.ExecuteAsync("释放输入", () => Task.FromException(ex)); }
        };
        viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(WorkspaceViewModel.DebugImage)) RefreshScreens();
            if (args.PropertyName == nameof(WorkspaceViewModel.HasActiveOperations) && !ViewModel.HasActiveOperations && debugAutoPending) _ = QueueDebugAutoLoadAsync();
        };
    }

    private void BuildShell()
    {
        BuildWorkspaceLayout();
        tabs = new WorkspaceTabControl { Background = Brushes.White, BorderThickness = new Thickness(0), Template = WorkspaceContentTemplate() };
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
        System.Windows.Documents.TextElement.SetForeground(content, ThemeBrush("TextBrush"));
        System.Windows.Documents.TextElement.SetFontWeight(content, FontWeights.Normal);
        var tab = new TabItem { Content = content, Tag = title };
        tab.Header = LocalizationService.PageDisplayName(title);
        tabs.Items.Add(tab);
    }
    private static DockPanel Page() => new() { Margin = new Thickness(14), LastChildFill = true };
    private static WrapPanel Toolbar() => ResponsiveLayout.Toolbar();
    private static TextBlock Label(string text)
    {
        var label = new TextBlock { FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 6, 0, 7) };
        LocalizationService.Bind(label, TextBlock.TextProperty, text);
        return label;
    }
    private static TextBlock BoundText(string binding, bool wrap = false) { var text = new TextBlock { VerticalAlignment = VerticalAlignment.Center, TextWrapping = wrap ? TextWrapping.Wrap : TextWrapping.NoWrap }; text.SetBinding(TextBlock.TextProperty, new Binding(binding)); return text; }
    private static Binding TwoWay(string path) => new(path) { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, ValidatesOnExceptions = true, ValidatesOnDataErrors = true };
    private static TextBox BoundInput(string binding, bool readOnly = false, bool multiline = false) { var input = new TextBox { IsReadOnly = readOnly, AcceptsReturn = multiline, TextWrapping = multiline ? TextWrapping.Wrap : TextWrapping.NoWrap, VerticalScrollBarVisibility = multiline ? ScrollBarVisibility.Auto : ScrollBarVisibility.Hidden, VerticalContentAlignment = multiline ? VerticalAlignment.Top : VerticalAlignment.Center }; input.SetBinding(TextBox.TextProperty, readOnly ? new Binding(binding) { Mode = BindingMode.OneWay } : TwoWay(binding)); LocalizationService.Bind(input, System.Windows.Automation.AutomationProperties.NameProperty, binding switch { nameof(WorkspaceViewModel.NodeProperties) => "节点属性", nameof(WorkspaceViewModel.ShellInput) => "命令", nameof(WorkspaceViewModel.TextInput) => "文本", nameof(WorkspaceViewModel.RemotePath) => "设备路径", nameof(WorkspaceViewModel.DeviceName) => "名称", _ => "输入" }); System.Windows.Automation.AutomationProperties.SetAutomationId(input, binding); return input; }
    private static CheckBox Check(string text, string binding) { var checkbox = new CheckBox(); LocalizationService.Bind(checkbox, ContentControl.ContentProperty, text); LocalizationService.Bind(checkbox, System.Windows.Automation.AutomationProperties.NameProperty, text); checkbox.SetBinding(ToggleButton.IsCheckedProperty, TwoWay(binding)); return checkbox; }
    private static FrameworkElement Field(string label, FrameworkElement control, double labelWidth = 155)
    {
        return ResponsiveLayout.Field(label, control, labelWidth);
    }
    private static Button Button(string glyph, string title, ICommand command)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal }; panel.Children.Add(new TextBlock { Text = glyph, FontFamily = new FontFamily("Segoe MDL2 Assets"), Margin = new Thickness(0, 0, 6, 0), VerticalAlignment = VerticalAlignment.Center }); var caption = new TextBlock { VerticalAlignment = VerticalAlignment.Center }; LocalizationService.Bind(caption, TextBlock.TextProperty, title); panel.Children.Add(caption); var button = new Button { Content = panel, Command = command }; LocalizationService.Bind(button, ToolTipProperty, title); LocalizationService.Bind(button, System.Windows.Automation.AutomationProperties.NameProperty, title); return button;
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
        UpdateNavigation();
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
        try
        {
#if MATRIX_TEST_SUPPORT
            mainCaptureDispatchCount++; lastMainCaptureTargets = targets.ToArray();
#endif
            async Task Capture()
            {
#if MATRIX_TEST_SUPPORT
                if (ViewModel.Demo && demoMainCaptureBarrier is { } barrier) await barrier();
#endif
                await ViewModel.RefreshAutomaticScreensAsync(targets, deadline.Token);
            }
            captureOperation = Capture(); await captureOperation;
        }
        catch (OperationCanceledException) { }
        finally { captureInProgress = false; }
    }

    private bool NeedsDebugAutoLoad() =>
#if MATRIX_TEST_SUPPORT
        !(ViewModel.Demo && !(DebugAutomaticFixturePermitted || ReconnectLoadingFixturePermitted)) &&
#else
        !ViewModel.Demo &&
#endif
        !closing && !closeApproved && IsLoaded
        && Equals((tabs?.SelectedItem as TabItem)?.Tag, "节点") && ViewModel.CurrentDevice?.Info.Online == true && ViewModel.Nodes.Count == 0;

    private Task EnsureDebugLoadedAsync()
    {
        if (!NeedsDebugAutoLoad()) { debugAutoPending = false; return Task.CompletedTask; }
        debugAutoPending = true;
        return QueueDebugAutoLoadAsync();
    }

    private Task QueueDebugAutoLoadAsync()
    {
        if (!NeedsDebugAutoLoad()) { debugAutoPending = false; return Task.CompletedTask; }
        if (!debugAutoPending || debugAutoLoading || debugAutoQueued || ViewModel.HasActiveOperations) return debugAutoOperation;
        return debugAutoOperation = RunDebugAutoLoadAsync();
    }

    private async Task RunDebugAutoLoadAsync()
    {
        debugAutoQueued = true;
        try
        {
            await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.Background);
            debugAutoQueued = false;
            if (!NeedsDebugAutoLoad()) { debugAutoPending = false; return; }
            if (!debugAutoPending || ViewModel.HasActiveOperations) return;
            debugAutoPending = false;
            debugAutoLoading = true;
            await ViewModel.LoadNodesAsync();
        }
        finally
        {
            debugAutoQueued = false;
            debugAutoLoading = false;
            if (debugAutoPending) _ = QueueDebugAutoLoadAsync();
        }
    }

    private (double Width, double Height) SettingsWindowSize()
    {
        var bounds = WindowState == WindowState.Normal ? new Size(ActualWidth, ActualHeight) : RestoreBounds.Size;
        return (double.IsFinite(bounds.Width) && bounds.Width > 0 ? bounds.Width : Width, double.IsFinite(bounds.Height) && bounds.Height > 0 ? bounds.Height : Height);
    }

    private async void WindowClosing(object? sender, CancelEventArgs e)
    {
        if (closeApproved) return;
        e.Cancel = true;
        if (closing) return;
        if (ldBusy || ViewModel.Busy) { MessageBox.Show(this, "当前操作尚未结束，请等待操作完成后退出。", "操作进行中", MessageBoxButton.OK, MessageBoxImage.Information); return; }
        if (ViewModel.HasActiveOperations && !ViewModel.Confirm("退出矩阵设备", "仍有脚本或文件传输正在运行。停止这些任务并退出？\n设备与模拟器将保持运行。")) return;
        closing = true;
        debugAutoPending = false;
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
            foreach (var session in screenProcesses.Values) { if (session.IsCurrentProcess()) { session.Process.Kill(entireProcessTree: true); await session.Process.WaitForExitAsync(); } session.Process.Dispose(); }
            closeApproved = true; Close();
        }
        catch (Exception ex) { closing = false; MessageBox.Show(this, "退出未完成：" + ViewModel.PresentError(ex) + "\n投屏恢复记录已保留。", "退出失败", MessageBoxButton.OK, MessageBoxImage.Error); }
    }

    internal void AbortStartup()
    {
        closing = closeApproved = true;
        debugAutoPending = false;
        captureTimer?.Stop();
        directoryTimer?.Stop(); directoryCancellation.Cancel();
        monitorTimer.Stop();
        scanCancellation?.Cancel();
        Close();
    }

    private void ShowText(string title, string text)
    {
        var window = new Window { Owner = this, Title = title, Width = 700, Height = 500, Content = new TextBox { Text = text, IsReadOnly = true, AcceptsReturn = true, TextWrapping = TextWrapping.Wrap, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, FontFamily = new FontFamily("Consolas"), Padding = new Thickness(12) } }; ResponsiveLayout.FitOwnedWindow(window, this, 700, 500); window.Show();
    }

    private bool? ShowOwnedDialog(Window dialog, double preferredWidth, double preferredHeight)
    {
        ResponsiveLayout.FitOwnedWindow(dialog, this, preferredWidth, preferredHeight);
#if MATRIX_TEST_SUPPORT
        if (ViewModel.Demo) responsiveOwnedDialogFixture?.Invoke(dialog);
#endif
        return dialog.ShowDialog();
    }

    private void SaveRender(string path)
    {
        foreach (var label in new[] { "WorkspaceSettings", "DeviceDetails" })
        {
            var control = VisualDescendants<Button>(Shell).Single(button => button.Name == label);
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
