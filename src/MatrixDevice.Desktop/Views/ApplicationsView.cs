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
    private FrameworkElement BuildAppsPage()
    {
        var page = Page(); var toolbar = Toolbar(); toolbar.Children.Add(Button("\uE72C", "刷新", ViewModel.RefreshAppsCommand)); toolbar.Children.Add(Check("系统应用", nameof(WorkspaceViewModel.ShowSystemApps))); var search = BoundInput(nameof(WorkspaceViewModel.AppSearch)); search.Width = 200; toolbar.Children.Add(search);
        toolbar.Children.Add(Button("\uE898", "安装 APK", async () => { var dialog = new OpenFileDialog { Filter = "Android 应用|*.apk" }; if (dialog.ShowDialog(this) == true) await ViewModel.InstallAppAsync(dialog.FileName); }, canExecute: ViewModel.CanWriteTargets));
        DockPanel.SetDock(toolbar, Dock.Top); page.Children.Add(toolbar);
        var actions = Toolbar(); actions.Margin = new Thickness(0, 8, 0, 8); actions.Children.Add(Button("\uE768", "启动", ViewModel.LaunchAppCommand)); actions.Children.Add(Button("\uE71A", "停止", ViewModel.StopAppCommand)); actions.Children.Add(Button("\uE894", "清数据", ViewModel.ClearAppCommand)); actions.Children.Add(Button("\uE74D", "卸载", ViewModel.UninstallAppCommand)); DockPanel.SetDock(actions, Dock.Top); page.Children.Add(actions);
        var target = BoundText(nameof(WorkspaceViewModel.TargetContext), true); target.Margin = new Thickness(0, 0, 0, 8); DockPanel.SetDock(target, Dock.Top); page.Children.Add(target);
        var grid = new DataGrid { ItemsSource = ViewModel.AppsView, IsReadOnly = true }; grid.SetBinding(Selector.SelectedItemProperty, TwoWay(nameof(WorkspaceViewModel.SelectedApp))); TextColumn(grid, "包名", nameof(AppInfo.PackageName), 350); TextColumn(grid, "名称", nameof(AppInfo.Name), 180); TextColumn(grid, "版本", nameof(AppInfo.VersionName), 130); var systemColumn = new DataGridCheckBoxColumn { Binding = new Binding(nameof(AppInfo.IsSystem)), IsReadOnly = true }; LocalizationService.Bind(systemColumn, DataGridColumn.HeaderProperty, "系统"); grid.Columns.Add(systemColumn); page.Children.Add(grid); return page;
    }

}
