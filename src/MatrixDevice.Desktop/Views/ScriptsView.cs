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
    private FrameworkElement BuildScriptPage()
    {
        var page = Page(); var toolbar = Toolbar(); toolbar.Children.Add(Button("\uE8E5", "导入脚本包", async () => { var dialog = new OpenFileDialog { Filter = "脚本包|*.jxscript;*.zip;*.json|所有文件|*.*" }; if (dialog.ShowDialog(this) == true) await ViewModel.ImportScriptAsync(dialog.FileName); })); toolbar.Children.Add(Button("\uE768", "运行所选", ViewModel.StartScriptCommand)); toolbar.Children.Add(Button("\uE71A", "停止所选", ViewModel.StopScriptCommand)); DockPanel.SetDock(toolbar, Dock.Top); page.Children.Add(toolbar);
        var summary = BoundText(nameof(WorkspaceViewModel.PackageSummary), true); summary.FontWeight = FontWeights.SemiBold; summary.Margin = new Thickness(0, 10, 0, 8); DockPanel.SetDock(summary, Dock.Top); page.Children.Add(summary);
        var targets = BoundText(nameof(WorkspaceViewModel.TargetContext), true); DockPanel.SetDock(targets, Dock.Top); page.Children.Add(targets);
        var grid = new Grid { Margin = new Thickness(0, 10, 0, 0) }; grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(230) }); grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); grid.RowDefinitions.Add(new RowDefinition());
        var parameters = new DataGrid { ItemsSource = ViewModel.Parameters }; TextColumn(parameters, "参数", nameof(ParameterEditor.Key), 170, true); TextColumn(parameters, "名称", nameof(ParameterEditor.Label), 180, true); TextColumn(parameters, "类型", nameof(ParameterEditor.Type), 100, true); TextColumn(parameters, "范围/单位", nameof(ParameterEditor.Range), 150, true); TextColumn(parameters, "值（JSON）", nameof(ParameterEditor.Value), 230, false); grid.Children.Add(parameters);
        var label = Label("脚本日志 · 最近 500 条"); label.Margin = new Thickness(0, 10, 0, 8); Grid.SetRow(label, 1); grid.Children.Add(label);
        var logs = new ListBox { ItemsSource = ViewModel.ScriptLogs, FontFamily = new FontFamily("Consolas"), FontSize = 12, BorderBrush = ThemeBrush("BorderBrush") }; Grid.SetRow(logs, 2); grid.Children.Add(logs); page.Children.Add(grid); return page;
    }

}
