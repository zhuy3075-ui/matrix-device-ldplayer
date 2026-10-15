using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MatrixDevice.Features;
using Microsoft.Win32;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    private FrameworkElement BuildExtensionsPage()
    {
        var page = Page();
        var grid = new Grid(); grid.ColumnDefinitions.Add(new ColumnDefinition()); grid.ColumnDefinitions.Add(new ColumnDefinition());
        var proxyPanel = new StackPanel { Margin = new Thickness(0, 0, 18, 0) };
        proxyPanel.Children.Add(Label("代理配置"));
        var address = new PasswordBox { Padding = new Thickness(7), Height = 34, ToolTip = "代理 URL，凭据输入已遮蔽" };
        proxyPanel.Children.Add(Field("代理 URL", address, 100));
        var output = new TextBox { IsReadOnly = true, AcceptsReturn = true, TextWrapping = TextWrapping.Wrap, Height = 225, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, FontFamily = new FontFamily("Consolas") };
        ProxyProfile? profile = null;
        var proxyActions = Toolbar();
        proxyActions.Children.Add(Button("\uE721", "解析配置", async () => await ViewModel.ExecuteAsync("解析代理配置", async () => { profile = ProxyProfileParser.Parse(address.Password); output.Text = ProxyProfileParser.ExportProfileJson(profile); await Task.CompletedTask; })));
        proxyActions.Children.Add(Button("\uE74E", "导出无凭据 JSON", async () =>
        {
            if (profile is null) throw new InvalidOperationException("尚未解析代理配置");
            var dialog = new SaveFileDialog { Filter = "JSON|*.json", FileName = "proxy-profile.json" };
            if (dialog.ShowDialog(this) == true) await File.WriteAllTextAsync(dialog.FileName, ProxyProfileParser.ExportProfileJson(profile));
        }));
        proxyPanel.Children.Add(proxyActions); proxyPanel.Children.Add(output);
        var proxyBoundary = new TextBlock { Foreground = Brushes.DimGray, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 10, 0, 10) }; LocalizationService.Bind(proxyBoundary, TextBlock.TextProperty, "代理运行器与 ROM 路由规则：未恢复，连接能力不可用"); proxyPanel.Children.Add(proxyBoundary);
        proxyPanel.Children.Add(Button("\uE72E", "核验 root", async () => await ViewModel.ExecuteAsync("核验 root", async () =>
        {
            var row = ViewModel.CurrentDevice ?? throw new InvalidOperationException("未选择设备");
            if (ViewModel.Demo) throw new InvalidOperationException("演示结果不能作为真实 root 证据");
            await new ProxyService(ViewModel.Runtime.Backend).EnsureRootAsync(row.Serial);
            output.Text = $"设备 {row.Serial} 已返回 root UID=0";
        }), write: true));
        proxyPanel.Children.Add(Label("环境配置"));
        var timezone = new TextBox { Text = "Asia/Shanghai" }; proxyPanel.Children.Add(Field("时区", timezone, 100));
        proxyPanel.Children.Add(Button("\uE74E", "应用并核验时区", async () =>
        {
            var row = ViewModel.CurrentDevice ?? throw new InvalidOperationException("未选择设备");
            if (!ViewModel.Confirm("修改设备时区", $"设备：{row.Name} ({row.Serial})\n时区：{timezone.Text}\n操作需要 root。")) return;
            await ViewModel.ExecuteAsync("修改时区", async () => await new EnvironmentConfigurationService(ViewModel.Runtime.Backend).ApplyAsync(row.Serial, new EnvironmentConfiguration { Timezone = timezone.Text.Trim() }));
        }, write: true));
        grid.Children.Add(new ScrollViewer { Name = "ExtensionSettingsScroll", Content = proxyPanel, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled });

        var backupPanel = new DockPanel(); Grid.SetColumn(backupPanel, 1);
        var heading = Label("应用资料包"); DockPanel.SetDock(heading, Dock.Top); backupPanel.Children.Add(heading);
        var package = new TextBox { Text = "" }; var packageField = Field("应用包名", package, 100); DockPanel.SetDock(packageField, Dock.Top); backupPanel.Children.Add(packageField);
        var notes = new TextBox(); var notesField = Field("备注", notes, 100); DockPanel.SetDock(notesField, Dock.Top); backupPanel.Children.Add(notesField);
        var storage = System.IO.Path.Combine(ViewModel.DataDirectory, "backups");
        var service = new ApplicationBackupService(ViewModel.Runtime.Backend, storage);
        var records = new ObservableCollection<AppBackupRecord>(service.List());
        var list = new DataGrid { ItemsSource = records, IsReadOnly = true }; TextColumn(list, "包名", nameof(AppBackupRecord.PackageName), 210); TextColumn(list, "创建时间", nameof(AppBackupRecord.CreatedUtc), 190); TextColumn(list, "备注", nameof(AppBackupRecord.Notes), 180);
        var backupActions = Toolbar();
        backupActions.Children.Add(Button("\uE8E5", "验证并导入", async () =>
        {
            var dialog = new OpenFileDialog { Filter = "应用资料包|*.tar.gz" };
            if (dialog.ShowDialog(this) == true)
            {
                var packageName = package.Text.Trim(); var remark = notes.Text; var path = dialog.FileName;
                await ViewModel.ExecuteAsync("导入资料包", async () => { var imported = await Task.Run(() => service.Import(path, packageName, remark)); records.Insert(0, imported); });
            }
        }));
        backupActions.Children.Add(Button("\uE896", "导出所选", async () =>
        {
            if (list.SelectedItem is not AppBackupRecord record) throw new InvalidOperationException("未选择资料包");
            var dialog = new SaveFileDialog { Filter = "资料包|*.tar.gz", FileName = record.PackageName + ".tar.gz" };
            if (dialog.ShowDialog(this) == true) await ViewModel.ExecuteAsync("导出资料包", async () => await Task.Run(() => service.Export(record, dialog.FileName)));
        }));
        backupActions.Children.Add(Button("\uE74D", "删除所选", async () =>
        {
            if (list.SelectedItem is not AppBackupRecord record) throw new InvalidOperationException("未选择资料包");
            if (!ViewModel.Confirm("删除资料包", $"仅删除本地受管备份\n包名：{record.PackageName}\n记录：{record.Id}")) return;
            await ViewModel.ExecuteAsync("删除资料包", async () => { await Task.Run(() => service.Delete(record, true)); records.Remove(record); });
        }));
        DockPanel.SetDock(backupActions, Dock.Top); backupPanel.Children.Add(backupActions);
        var boundary = new TextBlock { TextWrapping = TextWrapping.Wrap, Foreground = Brushes.DimGray, Margin = new Thickness(0, 9, 0, 9) }; LocalizationService.Bind(boundary, TextBlock.TextProperty, "此页导入和导出本地包，不自动恢复设备数据。"); DockPanel.SetDock(boundary, Dock.Top); backupPanel.Children.Add(boundary); backupPanel.Children.Add(list);
        grid.Children.Add(backupPanel);
        page.Children.Add(grid); return page;
    }
}
