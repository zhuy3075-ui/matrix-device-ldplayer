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
    private DataGrid wallGrid = null!;
    private DataGridColumn wallSelectionColumn = null!, wallActionColumn = null!, wallNameColumn = null!, wallNumberColumn = null!;
    private StackPanel wallHeading = null!;
    private ScrollViewer mainBodyScroll = null!;
    private Image mainControlImage = null!;
    private Button mainControlBack = null!;
    private bool mainControlOpen;
    private bool? compactDeviceList;
    private (double Horizontal, double Vertical)? hiddenListOffset;
    private long listRestoreTicket;

    internal string WallViewMode => wallViewMode;

    private FrameworkElement BuildWallPage()
    {
        var page = new DockPanel { LastChildFill = true, Background = Brushes.White };
        var top = new StackPanel { Margin = new Thickness(22, 18, 22, 0) }; wallHeading = top;
        wallActionsViewport = new ScrollViewer { Content = top, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled };
        page.SizeChanged += (_, _) => wallActionsViewport.MaxHeight = Math.Max(120, page.ActualHeight - 176);
        DockPanel.SetDock(wallActionsViewport, Dock.Top); page.Children.Add(wallActionsViewport);
        top.Children.Add(BuildDeviceSummary());
        var heading = new DockPanel(); var status = BuildDeviceStatusFilter(); DockPanel.SetDock(status, Dock.Right); heading.Children.Add(status);
        heading.Children.Add(new TextBlock { Name = "PageHeading", Text = LocalizationService.PageDisplayName("设备墙 / 主控"), FontSize = 20, FontWeight = FontWeights.SemiBold, Foreground = WallInk, Margin = new Thickness(0, 0, 0, 14) }); top.Children.Add(heading);
        top.Children.Add(BuildOneClickLaunchTools());
        top.Children.Add(new Border { Height = 1, Background = WallBorder });
        wallWorkspace = new Grid { Margin = new Thickness(22, 18, 22, 18) };
        wallWorkspace.SizeChanged += (_, _) => { CancelMainGesture(); ApplyDeviceWorkspaceLayout(); };
        wallWorkspace.RowDefinitions.Add(new RowDefinition());
        wallWorkspace.ColumnDefinitions.Add(new ColumnDefinition()); wallWorkspace.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0) });
        wallList = BuildWallList(); wallWorkspace.Children.Add(wallList);
        wallMainPanel = BuildWallMainPanel(); Grid.SetColumn(wallMainPanel, 1); wallWorkspace.Children.Add(wallMainPanel);
        page.Children.Add(wallWorkspace); CloseMainControl(); return page;
    }

    private FrameworkElement BuildWallList()
    {
        var grid = new DataGrid { ItemsSource = unifiedDeviceView, IsSynchronizedWithCurrentItem = false, AutoGenerateColumns = false, CanUserAddRows = false, CanUserDeleteRows = false, SelectionMode = DataGridSelectionMode.Single, HeadersVisibility = DataGridHeadersVisibility.Column, RowHeight = 52, ColumnHeaderHeight = 48, Background = Brushes.White, BorderThickness = new Thickness(0), GridLinesVisibility = DataGridGridLinesVisibility.Horizontal, HorizontalGridLinesBrush = WallBorder, IsReadOnly = true };
        wallGrid = grid; grid.Tag = false; grid.SizeChanged += (_, _) => ApplyDeviceListLayout();
        grid.PreviewMouseDown += (_, _) => CancelPendingListRestore();
        grid.AddHandler(Mouse.MouseWheelEvent, new MouseWheelEventHandler((_, _) => CancelPendingListRestore()), true);
        grid.PreviewKeyDown += (_, args) => { if (args.Key is Key.Up or Key.Down or Key.Left or Key.Right or Key.PageUp or Key.PageDown or Key.Home or Key.End) CancelPendingListRestore(); };
        grid.SelectionChanged += (_, _) => { if (!updatingUnifiedDirectory && !refreshingUnifiedFilter && wallViewMode == "list" && grid.SelectedItem is UnifiedDeviceRow { Device: { } device }) ViewModel.CurrentDevice = device; };
        var checkbox = new FrameworkElementFactory(typeof(CheckBox)); checkbox.SetValue(VerticalAlignmentProperty, VerticalAlignment.Center); checkbox.SetValue(HorizontalAlignmentProperty, HorizontalAlignment.Center); checkbox.SetBinding(ToggleButton.IsCheckedProperty, TwoWay(nameof(UnifiedDeviceRow.IsSelected))); checkbox.SetBinding(IsEnabledProperty, new Binding(nameof(UnifiedDeviceRow.CanSelect)));
        checkbox.SetValue(WidthProperty, 44d); checkbox.SetValue(HeightProperty, 44d); checkbox.SetValue(MarginProperty, new Thickness(0));
        wallSelectAll = new CheckBox { Name = "SelectAllDevices", IsThreeState = true, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, ToolTip = "勾选当前筛选结果中可勾选的设备；取消勾选清除全部选择（包括被筛选隐藏的设备），不删除设备或数据。" };
        System.Windows.Automation.AutomationProperties.SetName(wallSelectAll, "选择当前筛选中可勾选的设备");
        wallSelectAll.Width = wallSelectAll.Height = 44; wallSelectAll.Margin = new Thickness(0);
        wallSelectAll.Click += (_, _) =>
        {
            var select = wallSelectAll.IsChecked == true;
            if (select) foreach (var row in unifiedDeviceView.Cast<UnifiedDeviceRow>().Where(row => row.CanSelect).ToArray()) row.IsSelected = true;
            else ClearWallSelection();
            UpdateWallSelectionToggle();
        };
        var actionCell = new Style(typeof(DataGridCell), TryFindResource(typeof(DataGridCell)) as Style); actionCell.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(0))); actionCell.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(0)));
        var selectionHeader = new Style(typeof(DataGridColumnHeader), TryFindResource(typeof(DataGridColumnHeader)) as Style); selectionHeader.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(0))); selectionHeader.Setters.Add(new Setter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Center));
        wallSelectionColumn = new DataGridTemplateColumn { Header = wallSelectAll, Width = 54, CanUserResize = false, CellTemplate = new DataTemplate { VisualTree = checkbox }, CellStyle = actionCell, HeaderStyle = selectionHeader }; grid.Columns.Add(wallSelectionColumn);
        TextColumn(grid, "编号", nameof(UnifiedDeviceRow.Number), 58);
        wallNumberColumn = grid.Columns[^1];
        TextColumn(grid, "设备名称", nameof(UnifiedDeviceRow.Name), 175);
        wallNameColumn = grid.Columns[^1];
        var stateStyle = new Style(typeof(TextBlock)); stateStyle.Setters.Add(new Setter(ToolTipProperty, new Binding(nameof(UnifiedDeviceRow.StateDetails))));
        grid.Columns.Add(new DataGridTextColumn { Header = "状态", Binding = new Binding(nameof(UnifiedDeviceRow.State)), Width = 110, ElementStyle = stateStyle });
        var actions = new FrameworkElementFactory(typeof(UnifiedDeviceTile)); actions.SetBinding(UnifiedDeviceTile.OwnerProperty, new Binding { RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(MainWindow), 1) });
        actions.SetBinding(UnifiedDeviceTile.CompactProperty, new Binding(nameof(Tag)) { RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(DataGrid), 1) });
        wallActionColumn = new DataGridTemplateColumn { Header = "操作", Width = 214, CanUserResize = false, CellTemplate = new DataTemplate { VisualTree = actions }, CellStyle = actionCell }; grid.Columns.Add(wallActionColumn);
        TextColumn(grid, "型号", nameof(UnifiedDeviceRow.Model), 145); TextColumn(grid, "品牌", nameof(UnifiedDeviceRow.Brand), 110); TextColumn(grid, "安卓", nameof(UnifiedDeviceRow.Android), 80);
        TextColumn(grid, "设备序列号", nameof(UnifiedDeviceRow.Serial), 185); TextColumn(grid, "分辨率", nameof(UnifiedDeviceRow.Resolution), 120);
        TextColumn(grid, "安卓状态", nameof(UnifiedDeviceRow.AndroidState), 100);
        TextColumn(grid, "播放器进程", nameof(UnifiedDeviceRow.ProcessId), 105); TextColumn(grid, "虚拟机进程", nameof(UnifiedDeviceRow.VBoxProcessId), 105);
        TextColumn(grid, "密度", nameof(UnifiedDeviceRow.Dpi), 75); TextColumn(grid, "会话代次", nameof(UnifiedDeviceRow.Generation), 175);
        TextColumn(grid, "设备绑定", nameof(UnifiedDeviceRow.Binding), 240);
        foreach (var column in grid.Columns.OfType<DataGridTextColumn>().Where(column => (column.Binding as Binding)?.Path.Path != nameof(UnifiedDeviceRow.State)))
        {
            var textStyle = new Style(typeof(TextBlock)); textStyle.Setters.Add(new Setter(TextBlock.TextTrimmingProperty, TextTrimming.CharacterEllipsis)); textStyle.Setters.Add(new Setter(ToolTipProperty, column.Binding)); column.ElementStyle = textStyle;
        }
        var page = new Grid(); page.Children.Add(grid);
        var empty = new TextBlock { Text = "当前筛选没有设备", Foreground = WallMuted, FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        empty.SetBinding(VisibilityProperty, new Binding(nameof(ItemsControl.HasItems)) { Source = grid, Converter = new WallEmptyVisibilityConverter() }); page.Children.Add(empty); return page;
    }

    private FrameworkElement BuildWallMainPanel()
    {
        var panel = new Grid(); panel.RowDefinitions.Add(new() { Height = GridLength.Auto }); panel.RowDefinitions.Add(new());
        var heading = new DockPanel { Margin = new Thickness(0, 0, 0, 10) };
        var close = Button("\uE711", "返回设备列表", CloseMainControl); close.Name = "MainControlBack"; close.Content = WallGlyph("\uE711"); close.Width = close.MinWidth = close.Height = 44; close.Margin = new Thickness(4, 0, 0, 0); close.Padding = new Thickness(0); mainControlBack = close; DockPanel.SetDock(close, Dock.Right); heading.Children.Add(close);
        var caption = BoundText(nameof(WorkspaceViewModel.MainContext)); caption.TextTrimming = TextTrimming.CharacterEllipsis; caption.SetBinding(ToolTipProperty, new Binding(nameof(WorkspaceViewModel.MainContext))); caption.FontWeight = FontWeights.SemiBold; caption.Foreground = WallInk; heading.Children.Add(caption); panel.Children.Add(heading);
        var main = new StackPanel();
        mainBodyScroll = new ScrollViewer { Name = "MainControlBody", Content = main, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled };
        Grid.SetRow(mainBodyScroll, 1); panel.Children.Add(mainBodyScroll);
        var sync = new StackPanel { Margin = new Thickness(0, 0, 0, 10) };
        mainSyncSummary = new TextBlock { Foreground = WallMuted, Margin = new Thickness(0, 0, 0, 6), TextWrapping = TextWrapping.Wrap }; sync.Children.Add(mainSyncSummary);
        var mouse = Check("同步点击滑动", nameof(WorkspaceViewModel.SyncMouse)); mouse.MinHeight = 44; mouse.ToolTip = "将主控面板嵌入画面上的点击、滑动同步到列表勾选的其他设备；不监听雷电窗口的鼠标"; sync.Children.Add(mouse);
        var keyboard = Check("同步按键", nameof(WorkspaceViewModel.SyncKeyboard)); keyboard.MinHeight = 44; keyboard.ToolTip = "将程序内主控面板的主页、返回、任务和音量键同步到列表勾选的其他设备；不是物理键盘镜像，文字请使用文本输入"; keyboard.Margin = new Thickness(0, 6, 0, 0); sync.Children.Add(keyboard);
        main.Children.Add(sync);
        var image = new Image { Stretch = Stretch.Uniform, Height = 260, MinHeight = 100, Focusable = true, Cursor = Cursors.Cross };
        mainControlImage = image;
        image.SetBinding(Image.SourceProperty, new Binding("MainDevice.Screenshot")); image.MouseLeftButtonDown += MainImageDown; image.MouseLeftButtonUp += MainImageUp;
        image.LostMouseCapture += (_, _) => CancelMainGesture(); image.SizeChanged += (_, _) => CancelMainGesture();
        var screen = new Grid { Background = ThemeBrush("CanvasBrush") }; screen.Children.Add(image);
        var empty = new TextBlock { Text = "未指定主控", Foreground = WallMuted, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        empty.SetBinding(VisibilityProperty, new Binding(nameof(WorkspaceViewModel.MainDevice)) { Converter = new WallNullVisibilityConverter() }); screen.Children.Add(empty); main.Children.Add(screen);
        mainBodyScroll.ScrollChanged += (_, args) =>
        {
            if (args.ViewportHeightChange != 0 && mainBodyScroll.ViewportHeight > 0) image.Height = Math.Clamp(mainBodyScroll.ViewportHeight * .7, 140, 420);
            if (args.VerticalChange != 0 || args.HorizontalChange != 0) CancelMainGesture();
        };
        var bottom = new StackPanel { Margin = new Thickness(0, 12, 0, 0) }; main.Children.Add(bottom);
        var keys = Toolbar(); keys.Children.Add(Button("\uE72B", "返回", () => ViewModel.KeyAsync(4), canExecute: ViewModel.CanControlMainInput)); keys.Children.Add(Button("\uE80F", "主页", () => ViewModel.KeyAsync(3), canExecute: ViewModel.CanControlMainInput)); keys.Children.Add(Button("\uE7C4", "任务", () => ViewModel.KeyAsync(187), canExecute: ViewModel.CanControlMainInput));
        foreach (var (glyph, label, key, name) in new[] { ("\uE995", "增大音量", 24, "MainVolumeUp"), ("\uE993", "减小音量", 25, "MainVolumeDown") })
        {
            var volume = Button(glyph, label, () => ViewModel.KeyAsync(key), canExecute: ViewModel.CanControlMainInput);
            volume.Name = name; volume.Content = WallGlyph(glyph); volume.Width = volume.MinWidth = 44; volume.Height = 44; volume.Padding = new Thickness(0);
            System.Windows.Automation.AutomationProperties.SetName(volume, label); keys.Children.Add(volume);
        }
        foreach (var key in keys.Children.OfType<Button>()) { key.MinHeight = 44; key.Height = 44; }
        bottom.Children.Add(keys);
        var cast = new Expander { Header = "设备投屏", Margin = new Thickness(0, 8, 0, 0) };
        var castActions = new StackPanel { Margin = new Thickness(0, 8, 0, 0) };
        castActions.Children.Add(Button("\uE714", "启动 scrcpy 投屏", OpenScreenAsync, canExecute: CanLaunchScrcpy));
        castActions.Children.Add(Button("\uE8F2", "嵌入自有投屏窗口", EmbedScreenAsync, canExecute: CanEmbedScrcpy));
        castActions.Children.Add(Button("\uE8A7", "弹出投屏窗口", RestoreScreensAsync)); cast.Content = castActions; bottom.Children.Add(cast);
        foreach (var action in castActions.Children.OfType<Button>()) action.MinHeight = 44;
        return panel;
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
        mainControlOpen = true; ApplyDeviceWorkspaceLayout();
        UpdateWallSelectionToggle();
        await CaptureTickAsync();
    }

    internal void CloseMainControl()
    {
        CancelMainGesture();
        if (mainControlOpen && wallList.Visibility == Visibility.Visible) CancelPendingListRestore();
        mainControlOpen = false; ApplyDeviceWorkspaceLayout();
    }

    private void ApplyDeviceWorkspaceLayout()
    {
        if (wallWorkspace is null || wallMainPanel is null) return;
        var narrow = wallWorkspace.ActualWidth < 920;
        var hideList = mainControlOpen && narrow;
        if (hideList && wallList.Visibility == Visibility.Visible && DeviceListScrollViewer() is { } beforeHide)
        {
            listRestoreTicket++;
            hiddenListOffset ??= (beforeHide.HorizontalOffset, beforeHide.VerticalOffset);
        }
        var restoreList = !hideList && wallList.Visibility != Visibility.Visible;
        wallMainPanel.Visibility = mainControlOpen ? Visibility.Visible : Visibility.Collapsed;
        wallList.Visibility = hideList ? Visibility.Collapsed : Visibility.Visible;
        Grid.SetColumn(wallMainPanel, narrow ? 0 : 1); Grid.SetColumnSpan(wallMainPanel, narrow ? 2 : 1);
        wallWorkspace.ColumnDefinitions[1].Width = new GridLength(mainControlOpen && !narrow ? 300 : 0);
        wallMainPanel.Margin = narrow ? new Thickness(0) : new Thickness(18, 0, 0, 0);
        var available = (wallWorkspace.Parent as FrameworkElement)?.ActualWidth ?? wallWorkspace.ActualWidth;
        var margin = available < 644 ? 12 : 22;
        wallWorkspace.Margin = new Thickness(margin, 18, margin, 18); wallHeading.Margin = new Thickness(margin, 18, margin, 0);
        ApplyDeviceListLayout();
        if (restoreList && hiddenListOffset is { } saved)
        {
            var ticket = ++listRestoreTicket;
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ContextIdle, (Action)(() =>
            {
                if (ticket != listRestoreTicket || wallList.Visibility != Visibility.Visible || DeviceListScrollViewer() is not { } scroll) return;
                UpdateLayout();
                if (ticket != listRestoreTicket || wallList.Visibility != Visibility.Visible) return;
                // Keep the return position through temporary wide-layout clamping.
                if (!mainControlOpen) hiddenListOffset = null;
                scroll.ScrollToHorizontalOffset(Math.Min(saved.Horizontal, scroll.ScrollableWidth)); scroll.ScrollToVerticalOffset(Math.Min(saved.Vertical, scroll.ScrollableHeight));
            }));
        }
    }

    private void CancelPendingListRestore()
    {
        if (wallList.Visibility != Visibility.Visible || hiddenListOffset is null) return;
        hiddenListOffset = null; listRestoreTicket++;
    }

    private ScrollViewer? DeviceListScrollViewer() => wallGrid is null ? null : VisualDescendants<ScrollViewer>(wallGrid).FirstOrDefault(scroll => scroll.Name == "DG_ScrollViewer");

    private void ApplyDeviceListLayout()
    {
        if (wallGrid is null || wallGrid.ActualWidth <= 0 || wallGrid.Visibility != Visibility.Visible) return;
        var compact = wallGrid.ActualWidth < 600;
        if (compactDeviceList == compact) return;
        compactDeviceList = compact; wallGrid.Tag = compact; wallGrid.FrozenColumnCount = 0;
        wallActionColumn.DisplayIndex = compact ? 1 : 4; wallNameColumn.DisplayIndex = 2; wallNumberColumn.DisplayIndex = compact ? 3 : 1;
        wallSelectionColumn.Width = compact ? 48 : 54; wallActionColumn.Width = compact ? 148 : 214;
        wallNameColumn.Width = compact ? Math.Max(120, Math.Min(175, wallGrid.ActualWidth - 213)) : 175;
        wallGrid.FrozenColumnCount = compact ? 2 : 0;
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

    private static Brush WallInk => ThemeBrush("TextBrush");
    private static Brush WallMuted => ThemeBrush("MutedBrush");
    private static Brush WallBorder => ThemeBrush("BorderBrush");
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
        System.Windows.Automation.AutomationProperties.SetName(toggle, title);
        if (TryFindResource("SegmentedToggle") is Style style) toggle.Style = style; return toggle;
    }

    private void MainImageDown(object sender, MouseButtonEventArgs e)
    {
        CancelMainGesture();
        if (sender is not Image image || ViewModel.MainDevice is not { } row || !row.Info.Online || ViewModel.Busy) return;
        if (MapImage(image, e.GetPosition(image)) is not { } point) return;
        gestureStart = point; gestureDevice = row; gestureGeneration = ViewModel.Runtime.Input.CurrentGeneration; gestureDeviceGeneration = row.Info.Generation;
        gestureImage = image; gestureImageBounds = MainImageBounds(image); gestureImagePixels = MainImagePixels(image);
        gestureTargets = ViewModel.InputTargets(row.Serial, true).ToArray(); gestureTargetGenerations.Clear();
        foreach (var serial in gestureTargets) if (ViewModel.Devices.FirstOrDefault(device => device.Serial == serial) is { } target) gestureTargetGenerations[serial] = target.Info.Generation;
        if (!image.CaptureMouse()) { CancelMainGesture(); return; } e.Handled = true;
    }
    private async void MainImageUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Image image || gestureStart is not { } start || gestureDevice is not { } row) return;
        var end = MapImage(image, e.GetPosition(image)); var generation = gestureGeneration; var sourceGeneration = gestureDeviceGeneration;
        var targets = gestureTargets.ToArray(); var targetGenerations = gestureTargetGenerations.ToDictionary(); var pixels = gestureImagePixels;
        var sameGeometry = ReferenceEquals(image, gestureImage) && MainImageBounds(image) == gestureImageBounds && MainImagePixels(image) == pixels;
        CancelMainGesture();
        if (end is null || !sameGeometry || !mainControlOpen || !IsActive || !Equals((tabs.SelectedItem as TabItem)?.Tag, "设备墙 / 主控") || !row.Info.Online || ViewModel.Busy
            || !ReferenceEquals(row, ViewModel.MainDevice) || row.Info.Generation != sourceGeneration || ViewModel.Runtime.Input.CurrentGeneration != generation) return;
        if (targetGenerations.Any(saved => ViewModel.Devices.FirstOrDefault(target => target.Serial == saved.Key) is not { Info.Online: true } target || target.Info.Generation != saved.Value)) return;
        var swipe = (start - end.Value).Length > 8;
        var snapshot = new InputSnapshot { SourceSerial = row.Serial, Kind = swipe ? InputKind.Swipe : InputKind.Tap, X = (int)start.X, Y = (int)start.Y, X2 = (int)end.Value.X, Y2 = (int)end.Value.Y, DurationMs = 300, SourceWidth = (int)pixels.Width, SourceHeight = (int)pixels.Height, SourceGeneration = sourceGeneration, Generation = generation, TargetSerials = targets };
        mainGestureDispatchCount++; lastMainGestureSnapshot = snapshot; mainGestureOperation = ViewModel.SendInputAsync(snapshot);
        await mainGestureOperation; e.Handled = true;
    }

    private Rect MainImageBounds(Image image) => new(image.TranslatePoint(new Point(), Shell), image.RenderSize);
    private static Size MainImagePixels(Image image) => image.Source is BitmapSource source ? new(source.PixelWidth, source.PixelHeight) : new();
    private void CancelMainGesture()
    {
        var captured = gestureImage; gestureStart = null; gestureDevice = null; gestureImage = null; gestureTargets = []; gestureTargetGenerations.Clear();
        if (captured?.IsMouseCaptured == true) captured.ReleaseMouseCapture();
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
