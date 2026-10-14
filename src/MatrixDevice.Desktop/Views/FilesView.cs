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
    private FrameworkElement BuildFilesPage()
    {
        var page = Page(); var path = Toolbar(); var pathLabel = new TextBlock { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 8, 0) }; LocalizationService.Bind(pathLabel, TextBlock.TextProperty, "设备路径"); path.Children.Add(pathLabel); var remote = BoundInput(nameof(WorkspaceViewModel.RemotePath)); remote.Width = 440; path.Children.Add(remote); path.Children.Add(Button("\uE72C", "查看目录", async () =>
        {
            await ViewModel.ExecuteAsync("读取目录", async () =>
            {
                var row = ViewModel.CurrentDevice ?? throw new InvalidOperationException("未选择设备");
                var remotePath = ViewModel.RemotePath;
                if (remotePath.Any(character => !char.IsLetterOrDigit(character) && character is not '/' and not '_' and not '-' and not '.')) throw new InvalidOperationException("目录路径含不支持字符");
                var output = await ViewModel.Runtime.Backend.ShellAsync(row.Serial, $"ls -la '{remotePath}'");
                ShowText("设备目录 · " + row.Serial, output);
            });
        }, canExecute: ViewModel.CanUseDevice)); DockPanel.SetDock(path, Dock.Top); page.Children.Add(path);
        var actions = Toolbar(); actions.Margin = new Thickness(0, 8, 0, 8); var autoInstall = new CheckBox { VerticalAlignment = VerticalAlignment.Center }; LocalizationService.Bind(autoInstall, ContentControl.ContentProperty, "上传 APK 后安装"); actions.Children.Add(autoInstall);
        actions.Children.Add(Button("\uE898", "添加上传", async () => { var dialog = new OpenFileDialog { Multiselect = true }; if (dialog.ShowDialog(this) == true) await ViewModel.ExecuteAsync("添加上传", async () => { foreach (var file in dialog.FileNames) ViewModel.QueuePush(file, autoInstall.IsChecked == true); await Task.CompletedTask; }); }, canExecute: ViewModel.CanWriteTargets));
        actions.Children.Add(Button("\uE896", "添加下载", async () => { var dialog = new SaveFileDialog { FileName = System.IO.Path.GetFileName(ViewModel.RemotePath.TrimEnd('/')) }; if (dialog.ShowDialog(this) == true) await ViewModel.ExecuteAsync("添加下载", async () => { ViewModel.QueuePull(dialog.FileName); await Task.CompletedTask; }); }, canExecute: ViewModel.CanWriteDevice));
        actions.Children.Add(Button("\uE768", "开始", ViewModel.StartTransfersCommand)); actions.Children.Add(Button("\uE71A", "停止", ViewModel.CancelTransfersCommand)); DockPanel.SetDock(actions, Dock.Top); page.Children.Add(actions);
        var progress = new ProgressBar { Minimum = 0, Maximum = 100, Height = 5, Margin = new Thickness(0, 0, 0, 10) }; progress.SetBinding(RangeBase.ValueProperty, new Binding(nameof(WorkspaceViewModel.OverallProgress)) { Mode = BindingMode.OneWay }); DockPanel.SetDock(progress, Dock.Top); page.Children.Add(progress);
        var queue = new DataGrid { ItemsSource = ViewModel.Transfers, IsReadOnly = true }; TextColumn(queue, "设备", nameof(TransferRow.Serial), 160); TextColumn(queue, "方向", nameof(TransferRow.Direction), 65); TextColumn(queue, "文件", nameof(TransferRow.Name), 170); TextColumn(queue, "大小", nameof(TransferRow.Size), 100); TextColumn(queue, "设备路径", nameof(TransferRow.RemotePath), 230); TextColumn(queue, "%", nameof(TransferRow.Progress), 60); TextColumn(queue, "状态", nameof(TransferRow.State), 190); page.Children.Add(queue); return page;
    }

}
