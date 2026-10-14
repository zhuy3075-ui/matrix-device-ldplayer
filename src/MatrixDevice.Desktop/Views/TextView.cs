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
    private FrameworkElement BuildTextPage()
    {
        var page = Page(); var toolbar = Toolbar(); toolbar.Children.Add(Button("\uE8F4", "发送文本", ViewModel.TextCommand)); toolbar.Children.Add(Button("\uE8A5", "粘贴", () => { if (Clipboard.ContainsText()) ViewModel.TextInput = Clipboard.GetText(); }));
        toolbar.Children.Add(Button("\uE8E5", "加载文本文件", async () => { var dialog = new OpenFileDialog { Filter = "文本|*.txt;*.md;*.json|所有文件|*.*" }; if (dialog.ShowDialog(this) == true) await ViewModel.ExecuteAsync("加载文本", async () => ViewModel.TextInput = await File.ReadAllTextAsync(dialog.FileName)); })); DockPanel.SetDock(toolbar, Dock.Top); page.Children.Add(toolbar);
        var target = BoundText(nameof(WorkspaceViewModel.TargetContext), true); target.Margin = new Thickness(0, 8, 0, 8); DockPanel.SetDock(target, Dock.Top); page.Children.Add(target);
        var input = BoundInput(nameof(WorkspaceViewModel.TextInput), multiline: true); input.VerticalContentAlignment = VerticalAlignment.Top; page.Children.Add(input); return page;
    }

}
