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
    private string wallViewMode = "cards";
    private readonly Dictionary<string, ToggleButton> wallViewChoices = [];
    private Grid wallWorkspace = null!;
    private FrameworkElement wallCards = null!;
    private FrameworkElement wallScreens = null!;
    private FrameworkElement wallList = null!;
    private FrameworkElement wallMainPanel = null!;
    private FrameworkElement wallDensity = null!;
    private ToggleButton wallMainToggle = null!;

    internal string WallViewMode => wallViewMode;

    private FrameworkElement BuildWallPage()
    {
        var page = new DockPanel { LastChildFill = true, Background = Brushes.White };
        var top = new StackPanel { Margin = new Thickness(22, 18, 22, 0) };
        DockPanel.SetDock(top, Dock.Top); page.Children.Add(top);
        var heading = new DockPanel { Margin = new Thickness(0, 0, 0, 18) };
        var modes = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
        var modeFrame = new Border { Child = modes, Background = new SolidColorBrush(Color.FromRgb(245, 247, 251)), CornerRadius = new CornerRadius(6), Padding = new Thickness(3) }; DockPanel.SetDock(modeFrame, Dock.Right); heading.Children.Add(modeFrame);
        foreach (var (mode, glyph, title) in new[] { ("cards", "\uE8A9", "设备卡片"), ("screens", "\uE80A", "屏幕视图"), ("list", "\uE8EF", "列表视图") })
        {
            var choice = WallToggle(glyph, title); choice.Margin = new Thickness(0);
            choice.Click += (_, _) => SetWallViewMode(mode);
            wallViewChoices.Add(mode, choice); modes.Children.Add(choice);
        }
        heading.Children.Add(new TextBlock { Text = "设备群控", FontSize = 20, FontWeight = FontWeights.SemiBold, Foreground = WallInk, VerticalAlignment = VerticalAlignment.Center });
        top.Children.Add(heading);
        var tools = new WrapPanel { Margin = new Thickness(0, 0, 0, 12) };
        var select = WallButton("\uE73A", "全选", new ActionCommand(() => ViewModel.SelectFiltered(true))); select.ToolTip = "选择当前筛选中的全部设备"; tools.Children.Add(select);
        tools.Children.Add(WallButton("\uE711", "清空", new ActionCommand(ClearWallSelection)));
        tools.Children.Add(WallButton("\uE72C", "刷新截图", ViewModel.WallRefreshCommand, primary: true));
        var mouse = WallToggle("\uE962", "同步鼠标"); mouse.SetBinding(ToggleButton.IsCheckedProperty, TwoWay(nameof(WorkspaceViewModel.SyncMouse))); tools.Children.Add(mouse);
        var keyboard = WallToggle("\uE765", "同步键盘"); keyboard.SetBinding(ToggleButton.IsCheckedProperty, TwoWay(nameof(WorkspaceViewModel.SyncKeyboard))); tools.Children.Add(keyboard);
        var automatic = WallToggle("\uE787", "自动刷新"); automatic.Checked += (_, _) => { autoCapture = true; captureTimer.Start(); }; automatic.Unchecked += (_, _) => { autoCapture = false; captureTimer.Stop(); }; tools.Children.Add(automatic);
        wallMainToggle = WallToggle("\uE7F4", "主控面板"); wallMainToggle.Checked += (_, _) => UpdateWallMainPanel(); wallMainToggle.Unchecked += (_, _) => UpdateWallMainPanel(); tools.Children.Add(wallMainToggle);
        tools.Children.Add(BuildWallDensity()); top.Children.Add(tools);
        var context = new DockPanel { Margin = new Thickness(0, 0, 0, 13) };
        var mainState = BoundText(nameof(WorkspaceViewModel.MainContext)); mainState.Foreground = WallInk; mainState.TextTrimming = TextTrimming.CharacterEllipsis; mainState.MaxWidth = 330; mainState.SetBinding(ToolTipProperty, new Binding(nameof(WorkspaceViewModel.MainContext))); DockPanel.SetDock(mainState, Dock.Right); context.Children.Add(mainState);
        var targets = BoundText(nameof(WorkspaceViewModel.TargetContext)); targets.Foreground = WallMuted; targets.TextTrimming = TextTrimming.CharacterEllipsis; targets.Margin = new Thickness(0, 0, 20, 0); targets.SetBinding(ToolTipProperty, new Binding(nameof(WorkspaceViewModel.TargetContext))); context.Children.Add(targets); top.Children.Add(context);
        top.Children.Add(new Border { Height = 1, Background = WallBorder });
        wallWorkspace = new Grid { Margin = new Thickness(22, 18, 22, 18) };
        wallWorkspace.ColumnDefinitions.Add(new ColumnDefinition()); wallWorkspace.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0) });
        var views = new Grid(); wallWorkspace.Children.Add(views);
        wallCards = BuildWallItems("cards"); views.Children.Add(wallCards);
        wallScreens = BuildWallItems("screens"); views.Children.Add(wallScreens);
        wallList = BuildWallList(); views.Children.Add(wallList);
        wallMainPanel = BuildWallMainPanel(); Grid.SetColumn(wallMainPanel, 1); wallWorkspace.Children.Add(wallMainPanel);
        page.Children.Add(wallWorkspace); SetWallViewMode("cards"); return page;
    }

    private FrameworkElement BuildWallDensity()
    {
        var density = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(8, 0, 0, 6), ToolTip = "屏幕视图目标列数；窄窗口自动减少列数" };
        density.Children.Add(new TextBlock { Text = "屏数", Foreground = WallMuted, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 9, 0) });
        var slider = new Slider { Minimum = 5, Maximum = 20, TickFrequency = 1, IsSnapToTickEnabled = true, Width = 94, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 9, 0) };
        slider.SetBinding(RangeBase.ValueProperty, TwoWay(nameof(WorkspaceViewModel.WallColumns))); density.Children.Add(slider);
        var decreaseCommand = new ActionCommand(() => ViewModel.WallColumns--, () => ViewModel.WallColumns > 5);
        var increaseCommand = new ActionCommand(() => ViewModel.WallColumns++, () => ViewModel.WallColumns < 20);
        var decrease = WallButton("\uE738", "减少屏数", decreaseCommand); decrease.Content = WallGlyph("\uE738"); decrease.Width = 28; decrease.MinWidth = 0; decrease.Padding = new Thickness(0); density.Children.Add(decrease);
        var count = BoundText(nameof(WorkspaceViewModel.WallColumns)); count.Width = 24; count.TextAlignment = TextAlignment.Center; count.FontWeight = FontWeights.SemiBold; count.Margin = new Thickness(0, 0, 5, 0); density.Children.Add(count);
        var increase = WallButton("\uE710", "增加屏数", increaseCommand); increase.Content = WallGlyph("\uE710"); increase.Width = 28; increase.MinWidth = 0; increase.Padding = new Thickness(0); density.Children.Add(increase);
        ViewModel.PropertyChanged += (_, args) => { if (args.PropertyName == nameof(WorkspaceViewModel.WallColumns)) { decreaseCommand.Refresh(); increaseCommand.Refresh(); } };
        wallDensity = density; return density;
    }

    private FrameworkElement BuildWallItems(string mode)
    {
        var grid = new Grid();
        var items = new ItemsControl { ItemsSource = ViewModel.DeviceView, HorizontalContentAlignment = HorizontalAlignment.Stretch };
        var panel = new FrameworkElementFactory(typeof(UniformGrid));
        var columns = new MultiBinding { Converter = new WallColumnsConverter(mode == "screens" ? 118 : 344, mode == "screens") };
        columns.Bindings.Add(new Binding(nameof(WorkspaceViewModel.WallColumns)) { Source = ViewModel });
        columns.Bindings.Add(new Binding(nameof(ActualWidth)) { RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(ScrollViewer), 1) });
        panel.SetBinding(UniformGrid.ColumnsProperty, columns); items.ItemsPanel = new ItemsPanelTemplate(panel);
        var tile = new FrameworkElementFactory(typeof(WallDeviceTile)); tile.SetBinding(WallDeviceTile.OwnerProperty, new Binding { RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(MainWindow), 1) }); tile.SetValue(WallDeviceTile.ModeProperty, mode);
        items.ItemTemplate = new DataTemplate { VisualTree = tile };
        grid.Children.Add(new ScrollViewer { Content = items, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled });
        var empty = new TextBlock { Text = "当前筛选没有设备", Foreground = WallMuted, FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        empty.SetBinding(VisibilityProperty, new Binding(nameof(ItemsControl.HasItems)) { Source = items, Converter = new WallEmptyVisibilityConverter() }); grid.Children.Add(empty); return grid;
    }

    private FrameworkElement BuildWallList()
    {
        var grid = new DataGrid { ItemsSource = ViewModel.DeviceView, AutoGenerateColumns = false, CanUserAddRows = false, CanUserDeleteRows = false, SelectionMode = DataGridSelectionMode.Single, HeadersVisibility = DataGridHeadersVisibility.Column, RowHeight = 48, ColumnHeaderHeight = 40, Background = Brushes.White, BorderThickness = new Thickness(0), GridLinesVisibility = DataGridGridLinesVisibility.Horizontal, HorizontalGridLinesBrush = WallBorder, IsReadOnly = false };
        grid.SetBinding(Selector.SelectedItemProperty, new Binding(nameof(WorkspaceViewModel.CurrentDevice)) { Mode = BindingMode.OneWay });
        grid.SelectionChanged += (_, _) => { if (wallViewMode == "list" && grid.SelectedItem is DeviceRow row) ViewModel.CurrentDevice = row; };
        var checkbox = new FrameworkElementFactory(typeof(CheckBox)); checkbox.SetValue(VerticalAlignmentProperty, VerticalAlignment.Center); checkbox.SetValue(HorizontalAlignmentProperty, HorizontalAlignment.Center); checkbox.SetBinding(ToggleButton.IsCheckedProperty, TwoWay(nameof(DeviceRow.IsSelected)));
        grid.Columns.Add(new DataGridTemplateColumn { Header = "选择", Width = 54, CellTemplate = new DataTemplate { VisualTree = checkbox } });
        TextColumn(grid, "设备名称", nameof(DeviceRow.Name), 170); TextColumn(grid, "状态", nameof(DeviceRow.State), 90);
        TextColumn(grid, "型号", "Info.Model", 145); TextColumn(grid, "品牌", "Info.Brand", 110); TextColumn(grid, "安卓", "Info.AndroidVersion", 80);
        TextColumn(grid, "设备序列号", nameof(DeviceRow.Serial), 185); TextColumn(grid, "分辨率", nameof(DeviceRow.Resolution), 120); TextColumn(grid, "分组", nameof(DeviceRow.Group), 110);
        var actions = new FrameworkElementFactory(typeof(WallDeviceTile)); actions.SetBinding(WallDeviceTile.OwnerProperty, new Binding { RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(MainWindow), 1) }); actions.SetValue(WallDeviceTile.ModeProperty, "list");
        grid.Columns.Add(new DataGridTemplateColumn { Header = "操作", Width = 214, CellTemplate = new DataTemplate { VisualTree = actions } });
        var page = new Grid(); page.Children.Add(grid);
        var empty = new TextBlock { Text = "当前筛选没有设备", Foreground = WallMuted, FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        empty.SetBinding(VisibilityProperty, new Binding(nameof(ItemsControl.HasItems)) { Source = grid, Converter = new WallEmptyVisibilityConverter() }); page.Children.Add(empty); return page;
    }

    private FrameworkElement BuildWallMainPanel()
    {
        var main = new DockPanel { LastChildFill = true, MinHeight = 360 };
        var viewport = new ScrollViewer { Content = main, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled, Margin = new Thickness(18, 0, 0, 0) };
        main.SetBinding(HeightProperty, new Binding(nameof(ActualHeight)) { Source = viewport });
        var heading = BoundText(nameof(WorkspaceViewModel.MainContext), true); heading.FontWeight = FontWeights.SemiBold; heading.Foreground = WallInk; heading.Margin = new Thickness(0, 0, 0, 12); DockPanel.SetDock(heading, Dock.Top); main.Children.Add(heading);
        var bottom = new StackPanel { Margin = new Thickness(0, 12, 0, 0) }; DockPanel.SetDock(bottom, Dock.Bottom); main.Children.Add(bottom);
        var keys = Toolbar(); keys.Children.Add(Button("\uE72B", "返回", () => ViewModel.KeyAsync(4), canExecute: ViewModel.CanControlMainInput)); keys.Children.Add(Button("\uE80F", "主页", () => ViewModel.KeyAsync(3), canExecute: ViewModel.CanControlMainInput)); keys.Children.Add(Button("\uE7C4", "任务", () => ViewModel.KeyAsync(187), canExecute: ViewModel.CanControlMainInput)); bottom.Children.Add(keys);
        var cast = new Expander { Header = "当前设备投屏", Margin = new Thickness(0, 8, 0, 0) };
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
        foreach (var row in ViewModel.Devices) row.IsSelected = false;
        ViewModel.UpdateSelection();
    }

    internal void SetWallViewMode(string mode)
    {
        if (mode is not ("cards" or "screens" or "list")) throw new ArgumentException("设备视图不合法：" + mode, nameof(mode));
        wallViewMode = mode;
        wallCards.Visibility = mode == "cards" ? Visibility.Visible : Visibility.Collapsed;
        wallScreens.Visibility = mode == "screens" ? Visibility.Visible : Visibility.Collapsed;
        wallList.Visibility = mode == "list" ? Visibility.Visible : Visibility.Collapsed;
        foreach (var choice in wallViewChoices) choice.Value.IsChecked = choice.Key == mode;
        wallDensity.IsEnabled = mode == "screens"; wallMainToggle.IsChecked = mode == "screens"; UpdateWallMainPanel();
    }

    private void UpdateWallMainPanel()
    {
        if (wallMainPanel is null || wallWorkspace is null) return;
        var visible = wallMainToggle.IsChecked == true;
        wallMainPanel.Visibility = visible ? Visibility.Visible : Visibility.Collapsed; wallWorkspace.ColumnDefinitions[1].Width = new GridLength(visible ? 272 : 0);
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

public sealed class WallColumnsConverter(double minimumWidth, bool usePreferred) : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        var preferred = values.FirstOrDefault() is int count ? count : 5;
        var available = values.Length > 1 && values[1] is double width && double.IsFinite(width) && width > 0 ? width : minimumWidth;
        var responsive = Math.Max(1, (int)Math.Floor(available / minimumWidth));
        return usePreferred ? Math.Min(preferred, responsive) : responsive;
    }
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotSupportedException();
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
