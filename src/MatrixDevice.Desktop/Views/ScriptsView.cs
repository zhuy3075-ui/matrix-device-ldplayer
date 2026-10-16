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
        var page = Page(); var toolbar = Toolbar();
        var import = Button("\uE8E5", "导入脚本包", async () => { var dialog = new OpenFileDialog { Filter = "脚本包|*.jxscript;*.zip|所有文件|*.*" }; if (dialog.ShowDialog(this) == true) await ViewModel.ImportScriptAsync(dialog.FileName); });
        import.Content = new TextBlock { Text = "\uE8E5", FontFamily = new FontFamily("Segoe MDL2 Assets"), FontSize = 14 };
        import.Width = 44; import.Padding = new Thickness(0); toolbar.Children.Add(import);
        toolbar.Children.Add(Button("\uE768", "运行所选", ViewModel.StartScriptCommand));
        var stopSelected = Button("\uE71A", "停止所选设备", ViewModel.StopScriptCommand); stopSelected.ToolTip = "停止当前勾选设备的运行任务";
        toolbar.Children.Add(stopSelected); DockPanel.SetDock(toolbar, Dock.Top); page.Children.Add(toolbar);
        var summary = BoundText(nameof(WorkspaceViewModel.PackageSummary), true); summary.FontWeight = FontWeights.SemiBold;
        var summaryPanel = ScriptContext(summary, 54); summaryPanel.Margin = new Thickness(0, 8, 0, 6);
        DockPanel.SetDock(summaryPanel, Dock.Top); page.Children.Add(summaryPanel);
        var targets = ScriptContext(BoundText(nameof(WorkspaceViewModel.TargetContext), true), 36);
        DockPanel.SetDock(targets, Dock.Top); page.Children.Add(targets);
        var sections = new TabControl { Margin = new Thickness(0, 8, 0, 0) };
        var parameters = new DataGrid { ItemsSource = ViewModel.Parameters };
        TextColumn(parameters, "参数", nameof(ParameterEditor.Key), 120);
        TextColumn(parameters, "值（JSON）", nameof(ParameterEditor.Value), 180, false);
        TextColumn(parameters, "名称", nameof(ParameterEditor.Label), 180); TextColumn(parameters, "类型", nameof(ParameterEditor.Type), 90); TextColumn(parameters, "范围/单位", nameof(ParameterEditor.Range), 150);
        AddScriptSection("参数", parameters);
        var records = new DockPanel();
        var cancelJob = Button("\uE71A", "取消此任务", ViewModel.CancelScriptJobCommand); cancelJob.ToolTip = "仅取消选中的脚本任务，不跟随设备勾选变化";
        cancelJob.Content = new TextBlock { Text = "\uE71A", FontFamily = new FontFamily("Segoe MDL2 Assets"), FontSize = 14 };
        cancelJob.Width = 44; cancelJob.Padding = new Thickness(0); cancelJob.Margin = new Thickness(0);
        var jobs = new DataGrid { ItemsSource = ViewModel.ScriptJobs, IsReadOnly = true, CanUserAddRows = false, SelectionMode = DataGridSelectionMode.Single };
        jobs.SetBinding(DataGrid.SelectedItemProperty, TwoWay(nameof(WorkspaceViewModel.SelectedScriptJob)));
        System.Windows.Automation.AutomationProperties.SetName(jobs, "脚本运行记录");
        TextColumn(jobs, "目标", nameof(ScriptJobRow.Identity), 160); TextColumn(jobs, "状态", nameof(ScriptJobRow.State), 80); TextColumn(jobs, "进度", nameof(ScriptJobRow.Progress), 64);
        TextColumn(jobs, "脚本", nameof(ScriptJobRow.Package), 180); TextColumn(jobs, "任务编号", nameof(ScriptJobRow.JobId), 300); TextColumn(jobs, "开始", nameof(ScriptJobRow.Started), 130); TextColumn(jobs, "结束", nameof(ScriptJobRow.Ended), 130); TextColumn(jobs, "结果", nameof(ScriptJobRow.Result), 240);
        records.Children.Add(jobs); var recordsSection = AddScriptSection("运行记录", records);
        var recordHeader = new DockPanel(); DockPanel.SetDock(cancelJob, Dock.Right); recordHeader.Children.Add(cancelJob);
        var recordCaption = (TextBlock)recordsSection.Header;
        recordsSection.Header = null;
        recordHeader.Children.Add(new Border { MinHeight = 44, Padding = new Thickness(12, 0, 12, 0), Child = recordCaption });
        recordsSection.Header = recordHeader; recordsSection.Padding = new Thickness(0);
        var logPanel = new DockPanel();
        var label = BoundText(nameof(WorkspaceViewModel.ScriptLogContext), true); label.FontWeight = FontWeights.SemiBold;
        var logContext = ScriptContext(label, 40); logContext.Margin = new Thickness(0, 6, 0, 8);
        DockPanel.SetDock(logContext, Dock.Top); logPanel.Children.Add(logContext);
        var logs = new ListBox { ItemsSource = ViewModel.ScriptLogs, FontFamily = new FontFamily("Consolas"), FontSize = 12, BorderBrush = ThemeBrush("BorderBrush") };
        ScrollViewer.SetHorizontalScrollBarVisibility(logs, ScrollBarVisibility.Auto);
        ScrollViewer.SetVerticalScrollBarVisibility(logs, ScrollBarVisibility.Auto);
        logPanel.Children.Add(logs); AddScriptSection("日志", logPanel);
        var result = new TextBox { IsReadOnly = true, AcceptsReturn = true, TextWrapping = TextWrapping.NoWrap, HorizontalScrollBarVisibility = ScrollBarVisibility.Auto, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, VerticalContentAlignment = VerticalAlignment.Top, FontFamily = new FontFamily("Consolas"), FontSize = 12 };
        result.SetBinding(TextBox.TextProperty, new Binding("SelectedScriptJob.Result") { Mode = BindingMode.OneWay });
        LocalizationService.Bind(result, System.Windows.Automation.AutomationProperties.NameProperty, "任务结果");
        AddScriptSection("结果", result);
        foreach (var button in toolbar.Children.OfType<Button>().Append(cancelJob)) { button.MinWidth = 44; button.MinHeight = 44; }
        foreach (var table in new[] { parameters, jobs })
        {
            table.RowHeight = 44; table.ColumnHeaderHeight = 44;
            foreach (var column in table.Columns.OfType<DataGridTextColumn>())
            {
                column.MinWidth = column.Width.Value;
                var textStyle = new Style(typeof(TextBlock), column.ElementStyle);
                textStyle.Setters.Add(new Setter(TextBlock.TextTrimmingProperty, TextTrimming.CharacterEllipsis));
                textStyle.Setters.Add(new Setter(ToolTipProperty, column.Binding));
                column.ElementStyle = textStyle;
                if (!column.IsReadOnly)
                {
                    var editorStyle = new Style(typeof(TextBox), column.EditingElementStyle);
                    editorStyle.Setters.Add(new Setter(MinHeightProperty, 44d));
                    editorStyle.Setters.Add(new Setter(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Auto));
                    column.EditingElementStyle = editorStyle;
                }
            }
        }
        parameters.RowHeight = 48;
        page.SizeChanged += (_, _) => ArrangeContext(); page.Loaded += (_, _) => ArrangeContext();
        page.Children.Add(sections); return page;

        void ArrangeContext()
        {
            var shortPage = page.ActualHeight < 420;
            summaryPanel.MaxHeight = shortPage ? 20 : 54;
            targets.MaxHeight = shortPage ? 20 : 36;
        }

        TabItem AddScriptSection(string caption, FrameworkElement content)
        {
            var title = new TextBlock { VerticalAlignment = VerticalAlignment.Center };
            LocalizationService.Bind(title, TextBlock.TextProperty, caption);
            var section = new TabItem { Header = title, Content = content, MinHeight = 44, MinWidth = 44 };
            LocalizationService.Bind(section, System.Windows.Automation.AutomationProperties.NameProperty, caption);
            sections.Items.Add(section); return section;
        }
    }

    private static ScrollViewer ScriptContext(TextBlock text, double maximumHeight)
    {
        text.SetBinding(ToolTipProperty, new Binding(nameof(TextBlock.Text)) { Source = text });
        return new ScrollViewer { Content = text, MaxHeight = maximumHeight, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
    }
}
