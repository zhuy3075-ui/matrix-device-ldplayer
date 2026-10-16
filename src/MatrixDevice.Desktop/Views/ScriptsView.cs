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
        var page = Page(); var toolbar = Toolbar(); toolbar.Children.Add(Button("\uE8E5", "导入脚本包", async () => { var dialog = new OpenFileDialog { Filter = "脚本包|*.jxscript;*.zip|所有文件|*.*" }; if (dialog.ShowDialog(this) == true) await ViewModel.ImportScriptAsync(dialog.FileName); })); toolbar.Children.Add(Button("\uE768", "运行所选", ViewModel.StartScriptCommand));
        var stopSelected = Button("\uE71A", "停止所选设备", ViewModel.StopScriptCommand); stopSelected.ToolTip = "停止当前勾选设备的运行任务"; toolbar.Children.Add(stopSelected); DockPanel.SetDock(toolbar, Dock.Top); page.Children.Add(toolbar);
        var summary = BoundText(nameof(WorkspaceViewModel.PackageSummary), true); summary.FontWeight = FontWeights.SemiBold; summary.Margin = new Thickness(0, 10, 0, 8); DockPanel.SetDock(summary, Dock.Top); page.Children.Add(summary);
        var targets = BoundText(nameof(WorkspaceViewModel.TargetContext), true); DockPanel.SetDock(targets, Dock.Top); page.Children.Add(targets);
        var grid = new Grid { Margin = new Thickness(0, 10, 0, 0) }; grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(3, GridUnitType.Star), MinHeight = 140 }); grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(2, GridUnitType.Star), MinHeight = 60 });
        var sections = new TabControl();
        var parameters = new DataGrid { ItemsSource = ViewModel.Parameters }; TextColumn(parameters, "参数", nameof(ParameterEditor.Key), 170, true); TextColumn(parameters, "名称", nameof(ParameterEditor.Label), 180, true); TextColumn(parameters, "类型", nameof(ParameterEditor.Type), 100, true); TextColumn(parameters, "范围/单位", nameof(ParameterEditor.Range), 150, true); TextColumn(parameters, "值（JSON）", nameof(ParameterEditor.Value), 230, false);
        sections.Items.Add(new TabItem { Header = "参数", Content = parameters });
        var records = new DockPanel(); var recordActions = Toolbar();
        var cancelJob = Button("\uE711", "取消此任务", ViewModel.CancelScriptJobCommand); cancelJob.ToolTip = "仅取消下方选中的任务，不跟随设备勾选变化"; recordActions.Children.Add(cancelJob); DockPanel.SetDock(recordActions, Dock.Top); records.Children.Add(recordActions);
        var result = BoundText("SelectedScriptJob.Result", true);
        var resultPanel = new Expander { Header = "结果", Content = new ScrollViewer { Content = result, MaxHeight = 100, VerticalScrollBarVisibility = ScrollBarVisibility.Auto }, Margin = new Thickness(0, 6, 0, 6) };
        DockPanel.SetDock(resultPanel, Dock.Bottom); records.Children.Add(resultPanel);
        var jobs = new DataGrid { ItemsSource = ViewModel.ScriptJobs, IsReadOnly = true, CanUserAddRows = false, SelectionMode = DataGridSelectionMode.Single };
        jobs.SetBinding(DataGrid.SelectedItemProperty, TwoWay(nameof(WorkspaceViewModel.SelectedScriptJob)));
        System.Windows.Automation.AutomationProperties.SetName(jobs, "脚本运行记录");
        TextColumn(jobs, "目标", nameof(ScriptJobRow.Identity), 290); TextColumn(jobs, "状态", nameof(ScriptJobRow.State), 90); TextColumn(jobs, "进度", nameof(ScriptJobRow.Progress), 70);
        TextColumn(jobs, "脚本", nameof(ScriptJobRow.Package), 180); TextColumn(jobs, "任务编号", nameof(ScriptJobRow.JobId), 300); TextColumn(jobs, "开始", nameof(ScriptJobRow.Started), 130); TextColumn(jobs, "结束", nameof(ScriptJobRow.Ended), 130); TextColumn(jobs, "结果", nameof(ScriptJobRow.Result), 240);
        records.Children.Add(jobs); sections.Items.Add(new TabItem { Header = "运行记录", Content = records }); grid.Children.Add(sections);
        var label = BoundText(nameof(WorkspaceViewModel.ScriptLogContext), true); label.FontWeight = FontWeights.SemiBold; label.Margin = new Thickness(0, 10, 0, 8); Grid.SetRow(label, 1); grid.Children.Add(label);
        var logs = new ListBox { ItemsSource = ViewModel.ScriptLogs, FontFamily = new FontFamily("Consolas"), FontSize = 12, BorderBrush = ThemeBrush("BorderBrush") }; Grid.SetRow(logs, 2); grid.Children.Add(logs); page.Children.Add(grid); return page;
    }

}
