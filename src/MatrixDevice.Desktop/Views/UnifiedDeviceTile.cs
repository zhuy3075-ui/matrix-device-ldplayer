using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

namespace MatrixDevice.Desktop;

public sealed class UnifiedDeviceTile : Border
{
    public static readonly DependencyProperty OwnerProperty = DependencyProperty.Register(nameof(Owner), typeof(MainWindow), typeof(UnifiedDeviceTile), new PropertyMetadata(null, OnChanged));
    public static readonly DependencyProperty ModeProperty = DependencyProperty.Register(nameof(Mode), typeof(string), typeof(UnifiedDeviceTile), new PropertyMetadata("cards", OnChanged));
    public MainWindow? Owner { get => (MainWindow?)GetValue(OwnerProperty); set => SetValue(OwnerProperty, value); }
    public string Mode { get => (string)GetValue(ModeProperty); set => SetValue(ModeProperty, value); }
    private UnifiedDeviceRow? row;
    public UnifiedDeviceTile()
    {
        DataContextChanged += (_, _) => { if (row is not null) row.PropertyChanged -= RowChanged; row = DataContext as UnifiedDeviceRow; if (IsLoaded && row is not null) row.PropertyChanged += RowChanged; Rebuild(); };
        Loaded += (_, _) => { if (row is not null) { row.PropertyChanged -= RowChanged; row.PropertyChanged += RowChanged; } Rebuild(); };
        Unloaded += (_, _) => { if (row is not null) row.PropertyChanged -= RowChanged; };
    }
    private static void OnChanged(DependencyObject value, DependencyPropertyChangedEventArgs _) => ((UnifiedDeviceTile)value).Rebuild();
    private void RowChanged(object? sender, PropertyChangedEventArgs args) { if (args.PropertyName is "" or nameof(UnifiedDeviceRow.Device)) Rebuild(); }
    private void Rebuild()
    {
        if (row is null || Owner is null) { Child = null; return; }
        BorderThickness = new Thickness(0); Padding = new Thickness(0); Margin = new Thickness(0);
        if (row.Connected && row.Device is not null && Mode != "list")
        {
            Child = new WallDeviceTile { Owner = Owner, Mode = Mode, DataContext = row.Device }; return;
        }
        if (Mode == "list") { Child = Actions(); return; }
        var content = new StackPanel { Margin = new Thickness(Mode == "screens" ? 8 : 18) };
        var check = new CheckBox { IsEnabled = row.CanSelect, Margin = new Thickness(0, 0, 0, 10) };
        check.SetBinding(ToggleButton.IsCheckedProperty, new Binding(nameof(UnifiedDeviceRow.IsSelected)) { Mode = BindingMode.TwoWay });
        check.Content = new TextBlock { Text = row.Name, FontWeight = FontWeights.SemiBold, TextTrimming = TextTrimming.CharacterEllipsis, ToolTip = row.Name }; content.Children.Add(check);
        var state = new TextBlock { Foreground = Brushes.DimGray, Margin = new Thickness(0, 6, 0, 12), TextWrapping = TextWrapping.Wrap };
        state.SetBinding(TextBlock.TextProperty, new Binding(nameof(UnifiedDeviceRow.State))); content.Children.Add(state);
        content.Children.Add(new TextBlock { Text = "实例 " + row.Number, Margin = new Thickness(0, 0, 0, 12) });
        var spacer = new Border { Height = Mode == "screens" ? 170 : 132, Background = new SolidColorBrush(Color.FromRgb(247, 249, 252)), Margin = new Thickness(0, 0, 0, 12), Child = new TextBlock { Text = "\uE7F4", FontFamily = new FontFamily("Segoe MDL2 Assets"), FontSize = 32, Foreground = Brushes.LightSlateGray, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center } };
        content.Children.Add(spacer); content.Children.Add(Actions());
        Child = new Border { Child = content, Background = Brushes.White, BorderBrush = new SolidColorBrush(Color.FromRgb(228, 232, 240)), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(6), Margin = new Thickness(0, 0, 12, 12) };
    }
    private FrameworkElement Actions()
    {
        var actions = new WrapPanel { VerticalAlignment = VerticalAlignment.Center };
        var label = new TextBlock { VerticalAlignment = VerticalAlignment.Center }; label.SetBinding(TextBlock.TextProperty, new Binding(nameof(UnifiedDeviceRow.PrimaryLabel)));
        var glyph = new TextBlock { FontFamily = new FontFamily("Segoe MDL2 Assets"), Margin = new Thickness(0, 0, 5, 0), VerticalAlignment = VerticalAlignment.Center }; glyph.SetBinding(TextBlock.TextProperty, new Binding(nameof(UnifiedDeviceRow.PrimaryGlyph)));
        var body = new StackPanel { Orientation = Orientation.Horizontal }; body.Children.Add(glyph); body.Children.Add(label);
        var start = new Button { Content = body, Command = row!.PrimaryCommand, Height = 32, Padding = new Thickness(10, 0, 10, 0), Margin = new Thickness(0, 0, 6, 0), ToolTip = row.PrimaryLabel };
        start.SetResourceReference(StyleProperty, "SubtleButton"); actions.Children.Add(start);
        if (row.Connected && row.Device is not null)
        {
            var main = new Button { Content = new TextBlock { Text = "\uE7F4", FontFamily = new FontFamily("Segoe MDL2 Assets") }, Command = row.MainCommand, ToolTip = "设为主控", Margin = new Thickness(0, 0, 6, 0) };
            main.SetResourceReference(StyleProperty, "IconButton"); actions.Children.Add(main);
        }
        var show = new Button { Content = new TextBlock { Text = "\uE8A7", FontFamily = new FontFamily("Segoe MDL2 Assets") }, Command = row.ShowWindowCommand, ToolTip = "显示模拟器窗口", Visibility = row.ShowWindowCommand.CanExecute(null) ? Visibility.Visible : Visibility.Collapsed };
        show.SetResourceReference(StyleProperty, "IconButton"); actions.Children.Add(show);
        return actions;
    }
}
