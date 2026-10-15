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
    public static readonly DependencyProperty CompactProperty = DependencyProperty.Register(nameof(Compact), typeof(bool), typeof(UnifiedDeviceTile), new PropertyMetadata(false, OnCompactChanged));
    public MainWindow? Owner { get => (MainWindow?)GetValue(OwnerProperty); set => SetValue(OwnerProperty, value); }
    public bool Compact { get => (bool)GetValue(CompactProperty); set => SetValue(CompactProperty, value); }
    private UnifiedDeviceRow? row;
    private TextBlock? primaryLabel, controlLabel, primaryGlyph;
    public UnifiedDeviceTile()
    {
        DataContextChanged += (_, _) => { if (row is not null) row.PropertyChanged -= RowChanged; row = DataContext as UnifiedDeviceRow; if (IsLoaded && row is not null) row.PropertyChanged += RowChanged; Rebuild(); };
        Loaded += (_, _) => { if (row is not null) { row.PropertyChanged -= RowChanged; row.PropertyChanged += RowChanged; } Rebuild(); };
        Unloaded += (_, _) => { if (row is not null) row.PropertyChanged -= RowChanged; };
    }
    private static void OnChanged(DependencyObject value, DependencyPropertyChangedEventArgs _) => ((UnifiedDeviceTile)value).Rebuild();
    private static void OnCompactChanged(DependencyObject value, DependencyPropertyChangedEventArgs _) => ((UnifiedDeviceTile)value).ApplyCompactLayout();
    private void RowChanged(object? sender, PropertyChangedEventArgs args) { if (args.PropertyName is "" or nameof(UnifiedDeviceRow.Device)) Rebuild(); }
    private void Rebuild()
    {
        if (row is null || Owner is null) { Child = null; return; }
        BorderThickness = new Thickness(0); Padding = new Thickness(0); Margin = new Thickness(0);
        Child = Actions(); ApplyCompactLayout();
    }
    private FrameworkElement Actions()
    {
        var actions = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
        var label = new TextBlock { VerticalAlignment = VerticalAlignment.Center }; label.SetBinding(TextBlock.TextProperty, new Binding(nameof(UnifiedDeviceRow.PrimaryLabel)));
        primaryLabel = label; controlLabel = null;
        var glyph = new TextBlock { FontFamily = new FontFamily("Segoe MDL2 Assets"), Margin = new Thickness(0, 0, 5, 0), VerticalAlignment = VerticalAlignment.Center }; glyph.SetBinding(TextBlock.TextProperty, new Binding(nameof(UnifiedDeviceRow.PrimaryGlyph)));
        primaryGlyph = glyph;
        var body = new StackPanel { Orientation = Orientation.Horizontal }; body.Children.Add(glyph); body.Children.Add(label);
        var start = new Button { Name = "DevicePrimaryAction", Content = body, Command = row!.PrimaryCommand, Height = 44, MinWidth = 44, Padding = new Thickness(10, 0, 10, 0), Margin = new Thickness(0, 0, 4, 0) };
        start.SetBinding(System.Windows.Automation.AutomationProperties.NameProperty, new Binding(nameof(UnifiedDeviceRow.PrimaryLabel)));
        start.SetBinding(ToolTipProperty, new Binding(nameof(UnifiedDeviceRow.PrimaryDescription))); ToolTipService.SetShowOnDisabled(start, true);
        start.SetResourceReference(StyleProperty, "SubtleButton"); actions.Children.Add(start);
        if (row.Connected && row.Device is not null)
        {
            var control = new StackPanel { Orientation = Orientation.Horizontal }; control.Children.Add(new TextBlock { Text = "\uE7F4", FontFamily = new FontFamily("Segoe MDL2 Assets"), VerticalAlignment = VerticalAlignment.Center });
            controlLabel = new TextBlock { Text = "控制", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(4, 0, 0, 0) }; control.Children.Add(controlLabel);
            var main = new Button { Name = "DeviceMainAction", Content = control, Command = row.MainCommand, ToolTip = "打开此设备的主控面板", Height = 44, MinWidth = 44, Padding = new Thickness(6, 0, 6, 0), Margin = new Thickness(0, 0, 4, 0) };
            System.Windows.Automation.AutomationProperties.SetName(main, "控制此设备");
            main.SetResourceReference(StyleProperty, "SubtleButton"); actions.Children.Add(main);
        }
        var show = new Button { Name = "DeviceShowWindow", Width = 44, Height = 44, Padding = new Thickness(0), Content = new TextBlock { Text = "\uE8A7", FontFamily = new FontFamily("Segoe MDL2 Assets") }, Command = row.ShowWindowCommand, ToolTip = "显示模拟器窗口", Visibility = row.ShowWindowCommand.CanExecute(null) ? Visibility.Visible : Visibility.Collapsed };
        System.Windows.Automation.AutomationProperties.SetName(show, "显示模拟器窗口");
        show.SetResourceReference(StyleProperty, "IconButton"); actions.Children.Add(show);
        return actions;
    }
    private void ApplyCompactLayout()
    {
        if (Child is not Panel panel) return;
        if (primaryLabel is not null) primaryLabel.Visibility = Compact ? Visibility.Collapsed : Visibility.Visible;
        if (controlLabel is not null) controlLabel.Visibility = Compact ? Visibility.Collapsed : Visibility.Visible;
        if (primaryGlyph is not null) primaryGlyph.Margin = Compact ? new Thickness(0) : new Thickness(0, 0, 5, 0);
        foreach (var button in panel.Children.OfType<Button>().Where(button => button.Name != "DeviceShowWindow"))
        { button.Width = Compact ? 44 : double.NaN; button.Padding = Compact ? new Thickness(0) : new Thickness(button.Name == "DeviceMainAction" ? 6 : 10, 0, button.Name == "DeviceMainAction" ? 6 : 10, 0); }
    }
}
