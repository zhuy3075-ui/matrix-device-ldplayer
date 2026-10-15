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
    public MainWindow? Owner { get => (MainWindow?)GetValue(OwnerProperty); set => SetValue(OwnerProperty, value); }
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
        Child = Actions();
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
            var main = new Button { Content = "控制", Command = row.MainCommand, ToolTip = "打开此设备的主控面板", Height = 32, Padding = new Thickness(10, 0, 10, 0), Margin = new Thickness(0, 0, 6, 0) };
            main.SetResourceReference(StyleProperty, "SubtleButton"); actions.Children.Add(main);
        }
        var show = new Button { Content = new TextBlock { Text = "\uE8A7", FontFamily = new FontFamily("Segoe MDL2 Assets") }, Command = row.ShowWindowCommand, ToolTip = "显示模拟器窗口", Visibility = row.ShowWindowCommand.CanExecute(null) ? Visibility.Visible : Visibility.Collapsed };
        show.SetResourceReference(StyleProperty, "IconButton"); actions.Children.Add(show);
        return actions;
    }
}
