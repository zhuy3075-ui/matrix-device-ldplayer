using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MatrixDevice.Core;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    private const string wallViewMode = "list";
    private Grid wallWorkspace = null!;
    private FrameworkElement wallList = null!;
    private FrameworkElement wallMainPanel = null!;
    private CheckBox wallSelectAll = null!;
    private TextBlock mainSyncSummary = null!;
    private ScrollViewer wallActionsViewport = null!;

    internal string WallViewMode => wallViewMode;

    private FrameworkElement BuildWallPage()
    {
        var page = new DockPanel { LastChildFill = true, Background = Brushes.White };
        var top = new StackPanel { Margin = new Thickness(22, 18, 22, 0) };
        wallActionsViewport = new ScrollViewer { Content = top, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled };
        page.SizeChanged += (_, _) => wallActionsViewport.MaxHeight = Math.Max(120, page.ActualHeight - 176);
        DockPanel.SetDock(wallActionsViewport, Dock.Top); page.Children.Add(wallActionsViewport);
        top.Children.Add(BuildDeviceSummary());
        top.Children.Add(new TextBlock { Text = "设备管理", FontSize = 20, FontWeight = FontWeights.SemiBold, Foreground = WallInk, Margin = new Thickness(0, 0, 0, 14) });
        top.Children.Add(BuildOneClickLaunchTools());
        top.Children.Add(new Border { Height = 1, Background = WallBorder });
        wallWorkspace = new Grid { Margin = new Thickness(22, 18, 22, 18) };
        wallWorkspace.RowDefinitions.Add(new RowDefinition());
        wallWorkspace.ColumnDefinitions.Add(new ColumnDefinition()); wallWorkspace.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0) });
        wallList = BuildWallList(); wallWorkspace.Children.Add(wallList);
        wallMainPanel = BuildWallMainPanel(); Grid.SetColumn(wallMainPanel, 1); wallWorkspace.Children.Add(wallMainPanel);
        page.Children.Add(wallWorkspace); CloseMainControl(); return page;
    }

    private FrameworkElement BuildWallList()
    {
        var grid = new DataGrid { ItemsSource = unifiedDeviceView, AutoGenerateColumns = false, CanUserAddRows = false, CanUserDeleteRows = false, SelectionMode = DataGridSelectionMode.Single, HeadersVisibility = DataGridHeadersVisibility.Column, RowHeight = 48, ColumnHeaderHeight = 40, Background = Brushes.White, BorderThickness = new Thickness(0), GridLinesVisibility = DataGridGridLinesVisibility.Horizontal, HorizontalGridLinesBrush = WallBorder, IsReadOnly = true };
        grid.SelectionChanged += (_, _) => { if (wallViewMode == "list" && grid.SelectedItem is UnifiedDeviceRow { Device: { } device }) ViewModel.CurrentDevice = device; };
        var checkbox = new FrameworkElementFactory(typeof(CheckBox)); checkbox.SetValue(VerticalAlignmentProperty, VerticalAlignment.Center); checkbox.SetValue(HorizontalAlignmentProperty, HorizontalAlignment.Center); checkbox.SetBinding(ToggleButton.IsCheckedProperty, TwoWay(nameof(UnifiedDeviceRow.IsSelected))); checkbox.SetBinding(IsEnabledProperty, new Binding(nameof(UnifiedDeviceRow.CanSelect)));
        wallSelectAll = new CheckBox { Name = "SelectAllDevices", IsThreeState = true, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, ToolTip = "勾选全部可操作设备；取消勾选清除所有选择，不删除设备或数据" };
        System.Windows.Automation.AutomationProperties.SetName(wallSelectAll, "选择全部设备");
        wallSelectAll.Click += (_, _) =>
        {
            var select = wallSelectAll.IsChecked == true;
            if (select) foreach (var row in unifiedDeviceView.Cast<UnifiedDeviceRow>().Where(row => row.CanSelect).ToArray()) row.IsSelected = true;
            else ClearWallSelection();
            UpdateWallSelectionToggle();
        };
        grid.Columns.Add(new DataGridTemplateColumn { Header = wallSelectAll, Width = 54, CellTemplate = new DataTemplate { VisualTree = checkbox } });
        TextColumn(grid, "编号", nameof(UnifiedDeviceRow.Number), 58);
        TextColumn(grid, "设备名称", nameof(UnifiedDeviceRow.Name), 175); TextColumn(grid, "状态", nameof(UnifiedDeviceRow.State), 110);
        var actions = new FrameworkElementFactory(typeof(UnifiedDeviceTile)); actions.SetBinding(UnifiedDeviceTile.OwnerProperty, new Binding { RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(MainWindow), 1) });
        grid.Columns.Add(new DataGridTemplateColumn { Header = "操作", Width = 214, CellTemplate = new DataTemplate { VisualTree = actions } });
        TextColumn(grid, "型号", nameof(UnifiedDeviceRow.Model), 145); TextColumn(grid, "品牌", nameof(UnifiedDeviceRow.Brand), 110); TextColumn(grid, "安卓", nameof(UnifiedDeviceRow.Android), 80);
        TextColumn(grid, "设备序列号", nameof(UnifiedDeviceRow.Serial), 185); TextColumn(grid, "分辨率", nameof(UnifiedDeviceRow.Resolution), 120);
        TextColumn(grid, "安卓状态", nameof(UnifiedDeviceRow.AndroidState), 100);
        TextColumn(grid, "播放器进程", nameof(UnifiedDeviceRow.ProcessId), 105); TextColumn(grid, "虚拟机进程", nameof(UnifiedDeviceRow.VBoxProcessId), 105);
        TextColumn(grid, "密度", nameof(UnifiedDeviceRow.Dpi), 75); TextColumn(grid, "会话代次", nameof(UnifiedDeviceRow.Generation), 175);
        TextColumn(grid, "设备绑定", nameof(UnifiedDeviceRow.Binding), 240);
        var page = new Grid(); page.Children.Add(grid);
        var empty = new TextBlock { Text = "当前筛选没有设备", Foreground = WallMuted, FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        empty.SetBinding(VisibilityProperty, new Binding(nameof(ItemsControl.HasItems)) { Source = grid, Converter = new WallEmptyVisibilityConverter() }); page.Children.Add(empty); return page;
    }

    private FrameworkElement BuildWallMainPanel()
    {
        var main = new DockPanel { LastChildFill = true, MinHeight = 360 };
        var viewport = new ScrollViewer { Content = main, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled, Margin = new Thickness(18, 0, 0, 0) };
        main.SetBinding(HeightProperty, new Binding(nameof(ActualHeight)) { Source = viewport });
        var heading = new DockPanel { Margin = new Thickness(0, 0, 0, 10) };
        var close = Button("\uE711", "关闭控制面板", CloseMainControl); close.Content = WallGlyph("\uE711"); close.Width = close.MinWidth = 32; close.Padding = new Thickness(0); DockPanel.SetDock(close, Dock.Right); heading.Children.Add(close);
        var caption = BoundText(nameof(WorkspaceViewModel.MainContext), true); caption.FontWeight = FontWeights.SemiBold; caption.Foreground = WallInk; heading.Children.Add(caption); DockPanel.SetDock(heading, Dock.Top); main.Children.Add(heading);
        var sync = new StackPanel { Margin = new Thickness(0, 0, 0, 10) };
        mainSyncSummary = new TextBlock { Foreground = WallMuted, Margin = new Thickness(0, 0, 0, 6), TextWrapping = TextWrapping.Wrap }; sync.Children.Add(mainSyncSummary);
        var mouse = Check("同步点击滑动", nameof(WorkspaceViewModel.SyncMouse)); mouse.ToolTip = "将主控面板嵌入画面上的点击、滑动同步到列表勾选的其他设备；不监听雷电窗口的鼠标"; sync.Children.Add(mouse);
        var keyboard = Check("同步按键", nameof(WorkspaceViewModel.SyncKeyboard)); keyboard.ToolTip = "将程序内主控面板的主页、返回、任务和音量键同步到列表勾选的其他设备；不是物理键盘镜像，文字请使用文本输入"; keyboard.Margin = new Thickness(0, 6, 0, 0); sync.Children.Add(keyboard);
        DockPanel.SetDock(sync, Dock.Top); main.Children.Add(sync);
        var bottom = new StackPanel { Margin = new Thickness(0, 12, 0, 0) }; DockPanel.SetDock(bottom, Dock.Bottom); main.Children.Add(bottom);
        var keys = Toolbar(); keys.Children.Add(Button("\uE72B", "返回", () => ViewModel.KeyAsync(4), canExecute: ViewModel.CanControlMainInput)); keys.Children.Add(Button("\uE80F", "主页", () => ViewModel.KeyAsync(3), canExecute: ViewModel.CanControlMainInput)); keys.Children.Add(Button("\uE7C4", "任务", () => ViewModel.KeyAsync(187), canExecute: ViewModel.CanControlMainInput));
        foreach (var (glyph, label, key, name) in new[] { ("\uE995", "增大音量", 24, "MainVolumeUp"), ("\uE993", "减小音量", 25, "MainVolumeDown") })
        {
            var volume = Button(glyph, label, () => ViewModel.KeyAsync(key), canExecute: ViewModel.CanControlMainInput);
            volume.Name = name; volume.Content = WallGlyph(glyph); volume.Width = volume.MinWidth = 34; volume.Height = 34; volume.Padding = new Thickness(0);
            System.Windows.Automation.AutomationProperties.SetName(volume, label); keys.Children.Add(volume);
        }
        bottom.Children.Add(keys);
        var cast = new Expander { Header = "设备投屏", Margin = new Thickness(0, 8, 0, 0) };
        var castActions = new StackPanel { Margin = new Thickness(0, 8, 0, 0) };
        castActions.Children.Add(Button("\uE714", "启动 scrcpy 投屏", OpenScreenAsync, canExecute: CanLaunchScrcpy));
        castActions.Children.Add(Button("\uE8F2", "嵌入自有投屏窗口", EmbedScreenAsync, canExecute: CanEmbedScrcpy));
        castActions.Children.Add(Button("\uE8A7", "弹出投屏窗口", RestoreScreensAsync)); cast.Content = castActions; bottom.Children.Add(cast);
        var image = new Image { Stretch = Stretch.Uniform, MinHeight = 100, Focusable = true, Cursor = Cursors.Cross };
        image.SetBinding(Image.SourceProperty, new Binding("MainDevice.Screenshot")); image.MouseLeftButtonDown += MainImageDown; image.MouseLeftButtonUp += MainImageUp;
        var screen = new Grid { Background = new SolidColorBrush(Color.FromRgb(247, 249, 252)) }; screen.Children.Add(image);
        var empty = new TextBlock { Text = "未指定主控", Foreground = WallMuted, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        empty.SetBinding(VisibilityProperty, new Binding(nameof(WorkspaceViewModel.MainDevice)) { Converter = new WallNullVisibilityConverter() }); screen.Children.Add(empty); main.Children.Add(screen); return viewport;
    }

    internal void ClearWallSelection()
    {
        foreach (var row in unifiedDevices.ToArray()) row.IsSelected = false;
        foreach (var row in ViewModel.Devices) row.IsSelected = false;
        ViewModel.UpdateSelection(); RefreshLaunchCommands();
    }

    internal void SetWallViewMode(string mode)
    {
        if (mode != "list") throw new ArgumentException("设备管理仅支持列表：" + mode, nameof(mode));
    }

    internal async Task OpenMainControlAsync(DeviceRow row)
    {
        if (!ViewModel.Devices.Contains(row) || !row.Info.Online || ViewModel.Busy) throw new DeviceException("控制目标不可用，请选择已连接设备。", 409, 1003);
        await ViewModel.SetMainAsync(row);
        if (!ReferenceEquals(ViewModel.MainDevice, row)) return;
        wallMainPanel.Visibility = Visibility.Visible; wallWorkspace.ColumnDefinitions[1].Width = new GridLength(300);
        UpdateWallSelectionToggle();
        await CaptureTickAsync();
    }

    internal void CloseMainControl()
    {
        gestureStart = null; gestureDevice = null; gestureTargets = []; gestureTargetGenerations.Clear();
        if (Mouse.Captured is Image image) image.ReleaseMouseCapture();
        wallMainPanel.Visibility = Visibility.Collapsed; wallWorkspace.ColumnDefinitions[1].Width = new GridLength(0);
    }

    private void UpdateWallSelectionToggle()
    {
        if (wallSelectAll is not null)
        {
            var visible = unifiedDeviceView.Cast<UnifiedDeviceRow>().Where(row => row.CanSelect).ToArray();
            var selected = visible.Count(row => row.IsSelected);
            var hiddenSelected = unifiedDevices.Any(row => row.IsSelected && !visible.Contains(row));
            wallSelectAll.IsChecked = visible.Length == 0 && hiddenSelected ? null : selected == 0 ? false : selected == visible.Length ? true : null;
            wallSelectAll.IsEnabled = visible.Length != 0 || unifiedDevices.Any(row => row.IsSelected);
        }
        if (mainSyncSummary is not null)
        {
            var targets = unifiedDevices.Where(row => row.IsSelected && row.Device != ViewModel.MainDevice).ToArray();
            mainSyncSummary.Text = $"同步目标 {targets.Length} 台" + (targets.Any(row => !row.Connected) ? " · 含未连接设备，连接前不可同步" : "");
        }
    }

    private static readonly Brush WallInk = new SolidColorBrush(Color.FromRgb(24, 34, 57));
    private static readonly Brush WallMuted = new SolidColorBrush(Color.FromRgb(99, 113, 138));
    private static readonly Brush WallBorder = new SolidColorBrush(Color.FromRgb(228, 232, 240));
    private static TextBlock WallGlyph(string glyph) => new() { Text = glyph, FontFamily = new FontFamily("Segoe MDL2 Assets"), FontSize = 14, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
    private Button WallButton(string glyph, string title, ICommand command, bool primary = false)
    {
        var button = Button(glyph, title, command); button.Height = 34; button.Margin = new Thickness(0, 0, 8, 6); button.Padding = new Thickness(11, 0, 11, 0);
        if (TryFindResource(primary ? "PrimaryButton" : "SubtleButton") is Style style) button.Style = style; return button;
    }
    private ToggleButton WallToggle(string glyph, string title)
    {
        var content = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center }; content.Children.Add(WallGlyph(glyph)); content.Children.Add(new TextBlock { Text = title, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(7, 0, 0, 0) });
        var toggle = new ToggleButton { Content = content, ToolTip = title, Height = 34, Padding = new Thickness(11, 0, 11, 0), Margin = new Thickness(0, 0, 8, 6) };
        if (TryFindResource("SegmentedToggle") is Style style) toggle.Style = style; return toggle;
    }

    private void MainImageDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Image image || ViewModel.MainDevice is not { } row || !row.Info.Online || ViewModel.Busy) return;
        if (MapImage(image, e.GetPosition(image)) is not { } point) return;
        gestureStart = point; gestureDevice = row; gestureGeneration = ViewModel.Runtime.Input.CurrentGeneration; gestureDeviceGeneration = row.Info.Generation;
        gestureTargets = ViewModel.InputTargets(row.Serial, true).ToArray(); gestureTargetGenerations.Clear();
        foreach (var serial in gestureTargets) if (ViewModel.Devices.FirstOrDefault(device => device.Serial == serial) is { } target) gestureTargetGenerations[serial] = target.Info.Generation;
        image.CaptureMouse(); e.Handled = true;
    }
    private async void MainImageUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Image image || gestureStart is not { } start || gestureDevice is not { } row) return;
        var end = MapImage(image, e.GetPosition(image)); gestureStart = null; gestureDevice = null; image.ReleaseMouseCapture();
        if (end is null || !ReferenceEquals(row, ViewModel.MainDevice) || row.Info.Generation != gestureDeviceGeneration || ViewModel.Runtime.Input.CurrentGeneration != gestureGeneration) return;
        if (gestureTargetGenerations.Any(saved => ViewModel.Devices.FirstOrDefault(target => target.Serial == saved.Key)?.Info.Generation != saved.Value)) return;
        var swipe = (start - end.Value).Length > 8;
        await ViewModel.SendInputAsync(new InputSnapshot { SourceSerial = row.Serial, Kind = swipe ? InputKind.Swipe : InputKind.Tap, X = (int)start.X, Y = (int)start.Y, X2 = (int)end.Value.X, Y2 = (int)end.Value.Y, DurationMs = 300, SourceWidth = (image.Source as BitmapSource)?.PixelWidth ?? row.Info.Width, SourceHeight = (image.Source as BitmapSource)?.PixelHeight ?? row.Info.Height, SourceGeneration = gestureDeviceGeneration, Generation = gestureGeneration, TargetSerials = gestureTargets }); e.Handled = true;
    }
}

public sealed class WallEmptyVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is true ? Visibility.Collapsed : Visibility.Visible;
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
}

public sealed class WallNullVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is null ? Visibility.Visible : Visibility.Collapsed;
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
}
