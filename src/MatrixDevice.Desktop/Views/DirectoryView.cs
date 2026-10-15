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

public partial class MainWindow
{
    private FrameworkElement BuildDirectory()
    {
        var panel = new DockPanel { Margin = new Thickness(12, 0, 8, 0) };
        var filters = new StackPanel(); DockPanel.SetDock(filters, Dock.Top);
        filters.Children.Add(Label("设备目录"));
        panel.Children.Add(filters);

        var edit = new StackPanel { Margin = new Thickness(0, 10, 0, 8) }; DockPanel.SetDock(edit, Dock.Bottom);
        foreach (var (label, property) in new[] { ("画面", nameof(DeviceRow.ScreenState)), ("节点", nameof(DeviceRow.AutomationState)), ("脚本", nameof(DeviceRow.ScriptState)), ("API", nameof(DeviceRow.ApiAddress)) })
        {
            var status = BoundText("CurrentDevice." + property, true);
            var binding = new MultiBinding { Converter = new SubsystemCaptionConverter(label) };
            binding.Bindings.Add(new Binding("CurrentDevice." + property)); binding.Bindings.Add(new Binding(nameof(LocalizationService.Language)) { Source = LocalizationService.Current });
            status.SetBinding(TextBlock.TextProperty, binding);
            status.FontSize = 11; status.Margin = new Thickness(0, 2, 0, 0); edit.Children.Add(status);
        }
        edit.Children.Add(Field("名称", BoundInput(nameof(WorkspaceViewModel.DeviceName)), 50));
        edit.Children.Add(Button("\uE74E", "保存信息", ViewModel.SaveMetadataCommand));
        panel.Children.Add(edit);

        var list = new ListBox { Name = "DetailsDeviceChoice", ItemsSource = new ListCollectionView(ViewModel.Devices), IsSynchronizedWithCurrentItem = false, Background = Brushes.Transparent, BorderThickness = new Thickness(0), HorizontalContentAlignment = HorizontalAlignment.Stretch, ItemTemplate = DeviceTemplate() };
        System.Windows.Automation.AutomationProperties.SetName(list, "详情设备");
        list.SetBinding(Selector.SelectedItemProperty, new Binding(nameof(WorkspaceViewModel.CurrentDevice)) { Mode = BindingMode.OneWay });
        list.SelectionChanged += (_, _) => { if (list.SelectedItem is DeviceRow selected) ViewModel.CurrentDevice = selected; };
        panel.Children.Add(list);
        return panel;
    }

    private static DataTemplate DeviceTemplate()
    {
        var root = new FrameworkElementFactory(typeof(StackPanel)); root.SetValue(MarginProperty, new Thickness(0, 5, 0, 6));
        var row = new FrameworkElementFactory(typeof(DockPanel));
        var name = new FrameworkElementFactory(typeof(TextBlock)); name.SetBinding(TextBlock.TextProperty, new Binding(nameof(DeviceRow.Name))); name.SetValue(TextBlock.FontWeightProperty, FontWeights.SemiBold); name.SetValue(TextBlock.TextTrimmingProperty, TextTrimming.CharacterEllipsis); row.AppendChild(name); root.AppendChild(row);
        foreach (var property in new[] { nameof(DeviceRow.Serial), nameof(DeviceRow.Resolution), nameof(DeviceRow.State) })
        {
            var text = new FrameworkElementFactory(typeof(TextBlock)); text.SetBinding(TextBlock.TextProperty, new Binding(property)); text.SetValue(TextBlock.FontSizeProperty, 11d); text.SetValue(TextBlock.ForegroundProperty, Brushes.DimGray); text.SetValue(TextBlock.TextTrimmingProperty, TextTrimming.CharacterEllipsis); text.SetValue(MarginProperty, new Thickness(0, 2, 0, 0)); root.AppendChild(text);
        }
        return new DataTemplate { VisualTree = root };
    }

}
