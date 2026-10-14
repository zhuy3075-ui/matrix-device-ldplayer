using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace MatrixDevice.Desktop;

public sealed class WallDeviceTile : Border
{
    public static readonly DependencyProperty OwnerProperty = DependencyProperty.Register(nameof(Owner), typeof(MainWindow), typeof(WallDeviceTile), new PropertyMetadata(null, RebuildOnChange));
    public static readonly DependencyProperty ModeProperty = DependencyProperty.Register(nameof(Mode), typeof(string), typeof(WallDeviceTile), new PropertyMetadata("cards", RebuildOnChange));
    private static readonly Brush Ink = new SolidColorBrush(Color.FromRgb(24, 34, 57));
    private static readonly Brush Muted = new SolidColorBrush(Color.FromRgb(99, 113, 138));
    private static readonly Brush Line = new SolidColorBrush(Color.FromRgb(228, 232, 240));
    private static readonly Brush Accent = new SolidColorBrush(Color.FromRgb(50, 116, 246));
    private static readonly Brush AccentText = new SolidColorBrush(Color.FromRgb(33, 90, 206));
    private DeviceRow? row;
    private WorkspaceViewModel? subscribed;
    private readonly List<ICommand> commands = [];
    private TextBlock? status;
    private TextBlock? mainBadge;
    public MainWindow? Owner { get => (MainWindow?)GetValue(OwnerProperty); set => SetValue(OwnerProperty, value); }
    public string Mode { get => (string)GetValue(ModeProperty); set => SetValue(ModeProperty, value); }

    public WallDeviceTile()
    {
        DataContextChanged += (_, _) => Rebuild();
        Loaded += (_, _) => { Subscribe(); RefreshState(); }; Unloaded += (_, _) => Unsubscribe();
        MouseLeftButtonDown += (_, e) =>
        {
            if (Owner is null || row is null || FindParent<ButtonBase>(e.OriginalSource as DependencyObject) is not null) return;
            Owner.ViewModel.CurrentDevice = row;
        };
    }

    private static void RebuildOnChange(DependencyObject target, DependencyPropertyChangedEventArgs args) => ((WallDeviceTile)target).Rebuild();
    private void Rebuild()
    {
        Unsubscribe(); row = DataContext as DeviceRow;
        if (row is null || Owner is null) { Child = null; return; }
        commands.Clear(); status = null; mainBadge = null;
        Background = Brushes.White; CornerRadius = new CornerRadius(8); BorderThickness = Mode == "list" ? new Thickness(0) : new Thickness(1); BorderBrush = Line;
        Margin = Mode == "cards" ? new Thickness(0, 0, 14, 14) : Mode == "screens" ? new Thickness(0, 0, 8, 12) : new Thickness(0);
        VerticalAlignment = VerticalAlignment.Top; Padding = Mode == "cards" ? new Thickness(19, 16, 19, 16) : Mode == "screens" ? new Thickness(7) : new Thickness(0);
        Child = Mode switch { "screens" => BuildScreen(), "list" => BuildActions(compact: true), _ => BuildCard() };
        if (IsLoaded) Subscribe(); RefreshState();
    }

    private FrameworkElement BuildCard()
    {
        var body = new StackPanel();
        var top = new DockPanel { Margin = new Thickness(0, 0, 0, 11) };
        var selection = Selection(); DockPanel.SetDock(selection, Dock.Left); top.Children.Add(selection);
        status = new TextBlock { FontSize = 11, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(10, 0, 0, 0) }; DockPanel.SetDock(status, Dock.Right); top.Children.Add(status);
        top.Children.Add(Bound(nameof(DeviceRow.Name), 14, bold: true)); body.Children.Add(top);
        var model = new DockPanel { Margin = new Thickness(0, 0, 0, 9) };
        var phone = new TextBlock { Text = "\uE8EA", FontFamily = new FontFamily("Segoe MDL2 Assets"), Foreground = Accent, FontSize = 23, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(14, 0, 0, 0) }; DockPanel.SetDock(phone, Dock.Right); model.Children.Add(phone);
        var modelText = new StackPanel(); modelText.Children.Add(new TextBlock { Text = "设备型号", Foreground = Muted, FontSize = 11, Margin = new Thickness(0, 0, 0, 3) }); modelText.Children.Add(Bound("Info.Model", 17, bold: true)); model.Children.Add(modelText); body.Children.Add(model);
        var badges = new WrapPanel { Margin = new Thickness(0, 0, 0, 10) };
        badges.Children.Add(Badge("Info.Brand", false)); badges.Children.Add(Badge("Info.AndroidVersion", true));
        mainBadge = new TextBlock { FontSize = 11, Foreground = AccentText, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(2, 0, 0, 0) }; badges.Children.Add(mainBadge); body.Children.Add(badges);
        body.Children.Add(Detail("序列号", nameof(DeviceRow.Serial))); body.Children.Add(Detail("分辨率", nameof(DeviceRow.Resolution))); body.Children.Add(Detail("分组", nameof(DeviceRow.Group))); body.Children.Add(Detail("脚本", nameof(DeviceRow.ScriptState)));
        body.Children.Add(new Border { Background = Line, Height = 1, Margin = new Thickness(0, 13, 0, 13) });
        body.Children.Add(BuildActions()); return body;
    }

    private FrameworkElement BuildScreen()
    {
        var body = new StackPanel();
        var heading = new DockPanel { Margin = new Thickness(0, 0, 0, 6) }; heading.Children.Add(Selection()); heading.Children.Add(Bound(nameof(DeviceRow.Name), 11, bold: true)); body.Children.Add(heading);
        var screen = new Grid { Height = 230, Background = new SolidColorBrush(Color.FromRgb(244, 247, 251)) };
        var image = new Image { Stretch = Stretch.Uniform, Cursor = Cursors.Hand }; image.SetBinding(Image.SourceProperty, new Binding(nameof(DeviceRow.Screenshot))); screen.Children.Add(image);
        var empty = new TextBlock { Text = "无截图", FontSize = 11, Foreground = Muted, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        empty.SetBinding(VisibilityProperty, new Binding(nameof(DeviceRow.Screenshot)) { Converter = new WallNullVisibilityConverter() }); screen.Children.Add(empty); body.Children.Add(screen);
        var state = new DockPanel { Margin = new Thickness(0, 6, 0, 4) }; status = new TextBlock { FontSize = 10 }; state.Children.Add(status);
        mainBadge = new TextBlock { FontSize = 10, Foreground = AccentText, HorizontalAlignment = HorizontalAlignment.Right }; state.Children.Add(mainBadge); body.Children.Add(state);
        var frame = Bound(nameof(DeviceRow.FrameLabel), 10); frame.Margin = new Thickness(0, 0, 0, 5); frame.Foreground = Muted; body.Children.Add(frame);
        var script = Bound(nameof(DeviceRow.ScriptState), 10); script.Foreground = Muted; script.Margin = new Thickness(0, 0, 0, 5); body.Children.Add(script);
        body.Children.Add(BuildActions(compact: true)); return body;
    }

    private FrameworkElement BuildActions(bool compact = false)
    {
        var grid = new Grid(); grid.ColumnDefinitions.Add(new ColumnDefinition()); grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var debug = new ActionCommand(() => { if (row is null || Owner is null) return; Owner.ViewModel.CurrentDevice = row; Owner.NavigateTo("节点"); }, () => Owner is not null && !Owner.ViewModel.Busy);
        commands.Add(debug);
        var inspect = TileButton("\uE946", compact ? "调试" : "开始调试", debug); inspect.Margin = new Thickness(0, 0, 6, 0); grid.Children.Add(inspect);
        var control = new AsyncCommand(async () => { if (row is not null && Owner is not null) await Owner.ViewModel.SetMainAsync(row); }, () => row?.Info.Online == true && Owner is not null && !Owner.ViewModel.Busy);
        commands.Add(control);
        var main = TileButton("\uE7F4", "设为主控", control); main.ToolTip = "设为主控";
        if (compact && Mode == "screens") { main.Content = new TextBlock { Text = "\uE7F4", FontFamily = new FontFamily("Segoe MDL2 Assets"), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center }; main.Width = 28; main.Padding = new Thickness(0); }
        Grid.SetColumn(main, 1); grid.Children.Add(main); return grid;
    }

    private Button TileButton(string glyph, string caption, ICommand command)
    {
        var content = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };
        content.Children.Add(new TextBlock { Text = glyph, FontFamily = new FontFamily("Segoe MDL2 Assets"), VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 5, 0) }); content.Children.Add(new TextBlock { Text = caption, VerticalAlignment = VerticalAlignment.Center });
        var button = new Button { Content = content, Command = command, ToolTip = caption, Height = Mode == "cards" ? 34 : 28, Margin = new Thickness(0), Padding = new Thickness(8, 0, 8, 0), FontSize = Mode == "screens" ? 10 : 12 };
        if (Owner?.TryFindResource("SubtleButton") is Style style) button.Style = style; return button;
    }

    private static CheckBox Selection()
    {
        var check = new CheckBox { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 8, 0), ToolTip = "选择设备" };
        check.SetBinding(ToggleButton.IsCheckedProperty, new Binding(nameof(DeviceRow.IsSelected)) { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged }); return check;
    }
    private static TextBlock Bound(string path, double size, bool bold = false)
    {
        var text = new TextBlock { FontSize = size, Foreground = Ink, FontWeight = bold ? FontWeights.SemiBold : FontWeights.Normal, TextTrimming = TextTrimming.CharacterEllipsis, VerticalAlignment = VerticalAlignment.Center };
        text.SetBinding(TextBlock.TextProperty, new Binding(path) { Converter = new WallValueConverter() }); text.SetBinding(ToolTipProperty, new Binding(path)); return text;
    }
    private static FrameworkElement Detail(string title, string path)
    {
        var grid = new Grid { Margin = new Thickness(0, 0, 0, 5) }; grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) }); grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.Children.Add(new TextBlock { Text = title, FontSize = 11, Foreground = Muted, VerticalAlignment = VerticalAlignment.Center }); var value = Bound(path, 11); Grid.SetColumn(value, 1); grid.Children.Add(value); return grid;
    }
    private static FrameworkElement Badge(string path, bool android)
    {
        var text = Bound(path, 11); text.Foreground = android ? AccentText : Muted;
        text.SetBinding(TextBlock.TextProperty, new Binding(path) { Converter = new WallValueConverter(android ? "安卓 " : "") });
        return new Border { Child = text, Background = new SolidColorBrush(android ? Color.FromRgb(239, 245, 255) : Color.FromRgb(244, 246, 249)), CornerRadius = new CornerRadius(5), Padding = new Thickness(7, 3, 7, 3), Margin = new Thickness(0, 0, 7, 0) };
    }

    private void Subscribe()
    {
        if (subscribed is not null || Owner is null || row is null) return;
        subscribed = Owner.ViewModel; subscribed.PropertyChanged += ViewModelChanged; row.PropertyChanged += RowChanged;
    }
    private void Unsubscribe()
    {
        if (subscribed is not null) subscribed.PropertyChanged -= ViewModelChanged;
        if (row is not null) row.PropertyChanged -= RowChanged; subscribed = null;
    }
    private void ViewModelChanged(object? sender, PropertyChangedEventArgs args) => RefreshState();
    private void RowChanged(object? sender, PropertyChangedEventArgs args) => RefreshState();
    private void RefreshState()
    {
        if (row is null || Owner is null) return;
        BorderBrush = ReferenceEquals(Owner.ViewModel.CurrentDevice, row) ? Accent : Line;
        if (status is not null) { status.Text = row.State; status.Foreground = row.Info.Online ? new SolidColorBrush(Color.FromRgb(8, 115, 76)) : Muted; }
        if (mainBadge is not null) mainBadge.Text = ReferenceEquals(Owner.ViewModel.MainDevice, row) ? "主控" : "";
        foreach (var command in commands) { if (command is ActionCommand action) action.Refresh(); if (command is AsyncCommand asyncCommand) asyncCommand.Refresh(); }
    }
    private static T? FindParent<T>(DependencyObject? value) where T : DependencyObject
    {
        while (value is not null) { if (value is T parent) return parent; value = value is Visual ? VisualTreeHelper.GetParent(value) : LogicalTreeHelper.GetParent(value); }
        return null;
    }
}

public sealed class WallValueConverter(string prefix = "") : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => prefix + (value is null || string.IsNullOrWhiteSpace(value.ToString()) ? "未核验" : value);
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
}
