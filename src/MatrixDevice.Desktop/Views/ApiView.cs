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
    private ComboBox apiProtocol = null!, apiMethod = null!;
    private TextBox apiRoute = null!, apiResponse = null!, apiPayload = null!;
    private AsyncCommand apiSend = null!;
    private bool OwnsApiEndpoint() => apiProtocol.SelectedIndex switch
    {
        1 => ViewModel.Runtime.Api.AutomatorRunning,
        2 => ViewModel.CurrentDevice is { } row && ViewModel.Runtime.Api.DevicePorts.ContainsKey(row.Serial),
        _ => ViewModel.Runtime.Api.BasicRunning
    };
    private Task SendApiTestRequestAsync() => ViewModel.ExecuteAsync("测试 API", async () =>
    {
        apiResponse.Clear();
        if (!OwnsApiEndpoint()) throw new InvalidOperationException("所选本机接口未由当前程序启动");
        var port = apiProtocol.SelectedIndex switch
        {
            1 => ViewModel.Runtime.Api.RunningAutomatorPort ?? throw new InvalidOperationException("自动化接口未启动"),
            2 => ViewModel.Runtime.Api.DevicePorts[ViewModel.CurrentDevice!.Serial],
            _ => ViewModel.Runtime.Api.RunningBasicPort ?? throw new InvalidOperationException("普通接口未启动")
        };
        if (!apiRoute.Text.StartsWith('/') || apiRoute.Text.Contains("://", StringComparison.Ordinal) || apiRoute.Text.StartsWith("//", StringComparison.Ordinal)) throw new InvalidOperationException("路由必须为本机 API 的相对路径");
        using var handler = new HttpClientHandler { AllowAutoRedirect = false };
        using var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(30) };
        using var request = new HttpRequestMessage(new HttpMethod(apiMethod.SelectedItem?.ToString() ?? "GET"), $"http://127.0.0.1:{port}{apiRoute.Text}");
        if (apiMethod.SelectedIndex == 1) { using var validation = JsonDocument.Parse(apiPayload.Text); request.Content = new StringContent(apiPayload.Text, System.Text.Encoding.UTF8, "application/json"); }
        try
        {
            using var result = await client.SendAsync(request);
            apiResponse.Text = $"HTTP {(int)result.StatusCode} {result.ReasonPhrase}\n" + await result.Content.ReadAsStringAsync();
        }
        catch (TaskCanceledException ex) { throw new TimeoutException("本机接口请求超时", ex); }
    }, blocksApiWrites: false);
    private FrameworkElement BuildApiPage()
    {
        var page = Page(); var toolbar = Toolbar(); toolbar.Children.Add(Button("\uE768", "启动两个网关", ViewModel.StartApiCommand)); toolbar.Children.Add(Button("\uE71A", "停止网关", ViewModel.StopApiCommand)); toolbar.Children.Add(Button("\uE968", "启用设备接口", ViewModel.StartDeviceApiCommand)); toolbar.Children.Add(Button("\uE711", "停止设备接口", ViewModel.StopDeviceApiCommand)); DockPanel.SetDock(toolbar, Dock.Top); page.Children.Add(toolbar);
        var summary = BoundText(nameof(WorkspaceViewModel.ApiSummary), true); summary.Margin = new Thickness(0, 10, 0, 10); DockPanel.SetDock(summary, Dock.Top); page.Children.Add(summary);
        var requestPanel = new DockPanel { Margin = new Thickness(0, 10, 0, 0), Height = 260 }; DockPanel.SetDock(requestPanel, Dock.Bottom);
        var requestToolbar = Toolbar();
        apiProtocol = new ComboBox { ItemsSource = new[] { "普通 API", "Automator API", "设备接口" }, ItemTemplate = CaptionChoices(), SelectedIndex = 0, Width = 165 }; requestToolbar.Children.Add(apiProtocol);
        apiMethod = new ComboBox { ItemsSource = new[] { "GET", "POST" }, SelectedIndex = 0, Width = 85 }; requestToolbar.Children.Add(apiMethod);
        apiRoute = new TextBox { Text = "/health", Width = 190 }; requestToolbar.Children.Add(apiRoute);
        apiResponse = new TextBox { IsReadOnly = true, AcceptsReturn = true, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, FontFamily = new FontFamily("Consolas"), TextWrapping = TextWrapping.Wrap };
        apiPayload = new TextBox { Text = "{}", AcceptsReturn = true, Height = 60, FontFamily = new FontFamily("Consolas"), Margin = new Thickness(0, 7, 0, 7) };
        System.Windows.Automation.AutomationProperties.SetName(apiProtocol, "接口类型"); System.Windows.Automation.AutomationProperties.SetName(apiMethod, "请求方法");
        System.Windows.Automation.AutomationProperties.SetName(apiRoute, "请求路径"); System.Windows.Automation.AutomationProperties.SetName(apiPayload, "请求正文"); System.Windows.Automation.AutomationProperties.SetName(apiResponse, "响应结果");
        apiSend = new AsyncCommand(SendApiTestRequestAsync, () => !ViewModel.Busy && !ViewModel.Transferring && OwnsApiEndpoint());
        apiProtocol.SelectionChanged += (_, _) => apiSend.Refresh(); ViewModel.PropertyChanged += (_, _) => apiSend.Refresh();
        requestToolbar.Children.Add(Button("\uE768", "发送请求", apiSend));
        DockPanel.SetDock(requestToolbar, Dock.Top); requestPanel.Children.Add(requestToolbar); DockPanel.SetDock(apiPayload, Dock.Top); requestPanel.Children.Add(apiPayload); requestPanel.Children.Add(apiResponse); page.Children.Add(requestPanel);
        var grid = new DataGrid { ItemsSource = ViewModel.DeviceView, IsSynchronizedWithCurrentItem = false, IsReadOnly = false };
        grid.SetBinding(Selector.SelectedItemProperty, new Binding(nameof(WorkspaceViewModel.CurrentDevice)) { Mode = BindingMode.OneWay });
        grid.SelectionChanged += (_, _) => { if (Equals((tabs.SelectedItem as TabItem)?.Tag, "API") && grid.SelectedItem is DeviceRow selected) ViewModel.CurrentDevice = selected; };
        var selectedColumn = new DataGridCheckBoxColumn { Binding = TwoWay(nameof(DeviceRow.IsSelected)), IsReadOnly = false }; LocalizationService.Bind(selectedColumn, DataGridColumn.HeaderProperty, "选中"); grid.Columns.Add(selectedColumn); TextColumn(grid, "serial", nameof(DeviceRow.Serial), 210); TextColumn(grid, "设备", nameof(DeviceRow.Name), 180); TextColumn(grid, "分组", nameof(DeviceRow.Group), 100); TextColumn(grid, "设备状态", nameof(DeviceRow.State), 120); TextColumn(grid, "普通 API 地址", nameof(DeviceRow.ApiAddress), 280); grid.MouseDoubleClick += (_, _) => { if (grid.SelectedItem is DeviceRow row && row.ApiAddress != "未启动") Clipboard.SetText(row.ApiAddress); }; page.Children.Add(grid); return page;
    }

}
