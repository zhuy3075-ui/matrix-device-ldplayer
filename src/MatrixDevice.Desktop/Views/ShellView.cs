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
    private FrameworkElement BuildShellPage()
    {
        var page = Page();
        var toolbar = Toolbar(); var input = BoundInput(nameof(WorkspaceViewModel.ShellInput)); input.Width = 470; input.FontFamily = new FontFamily("Consolas"); input.KeyDown += ShellHistoryKey; toolbar.Children.Add(input); toolbar.Children.Add(Button("\uE768", "执行", ViewModel.ShellCommand));
        DockPanel.SetDock(toolbar, Dock.Top); page.Children.Add(toolbar);
        var history = new ListBox { ItemsSource = ViewModel.ShellHistory, BorderThickness = new Thickness(0), HorizontalContentAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0, 10, 0, 0) };
        var panel = new FrameworkElementFactory(typeof(StackPanel)); panel.SetValue(MarginProperty, new Thickness(0, 0, 0, 15));
        var command = new FrameworkElementFactory(typeof(TextBlock)); command.SetBinding(TextBlock.TextProperty, new Binding(nameof(ShellEntry.Command))); command.SetValue(TextBlock.FontWeightProperty, FontWeights.SemiBold); command.SetValue(TextBlock.FontFamilyProperty, new FontFamily("Consolas")); panel.AppendChild(command);
        var time = new FrameworkElementFactory(typeof(TextBlock)); time.SetBinding(TextBlock.TextProperty, new Binding(nameof(ShellEntry.Context))); time.SetValue(TextBlock.ForegroundProperty, Brushes.Gray); panel.AppendChild(time);
        var output = new FrameworkElementFactory(typeof(TextBox)); output.SetBinding(TextBox.TextProperty, new Binding(nameof(ShellEntry.Output))); output.SetValue(TextBox.IsReadOnlyProperty, true); output.SetValue(TextBox.TextWrappingProperty, TextWrapping.Wrap); output.SetValue(TextBox.AcceptsReturnProperty, true); output.SetValue(FontFamilyProperty, new FontFamily("Consolas")); panel.AppendChild(output); history.ItemTemplate = new DataTemplate { VisualTree = panel };
        history.MouseDoubleClick += (_, _) => { if (history.SelectedItem is ShellEntry entry) ViewModel.ShellInput = entry.Command; }; page.Children.Add(history); return page;
    }

    private void ShellHistoryKey(object sender, KeyEventArgs args)
    {
        if (args.Key == Key.Enter) { if (ViewModel.ShellCommand.CanExecute(null)) ViewModel.ShellCommand.Execute(null); shellHistoryCursor = -1; args.Handled = true; return; }
        if (args.Key is not Key.Up and not Key.Down || ViewModel.ShellHistory.Count == 0) return;
        shellHistoryCursor = Math.Clamp(shellHistoryCursor + (args.Key == Key.Up ? 1 : -1), -1, ViewModel.ShellHistory.Count - 1); ViewModel.ShellInput = shellHistoryCursor < 0 ? "" : ViewModel.ShellHistory[shellHistoryCursor].Command; if (sender is TextBox input) input.CaretIndex = input.Text.Length; args.Handled = true;
    }

}
