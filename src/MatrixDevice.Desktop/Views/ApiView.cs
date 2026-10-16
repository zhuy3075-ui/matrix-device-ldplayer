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
    private sealed record ApiEndpointKey(object Runtime, int Protocol, string? Serial, int? Port);
    private ApiEndpointKey? apiEndpoint;
    private long apiRequestRevision;

    private ApiEndpointKey ReadApiEndpoint()
    {
        var runtime = ViewModel.Runtime;
        var protocol = apiProtocol.SelectedIndex;
        var serial = protocol == 2 ? ViewModel.CurrentDevice?.Serial : null;
        int? port = protocol switch
        {
            0 => runtime.Api.RunningBasicPort,
            1 => runtime.Api.RunningAutomatorPort,
            2 when serial is not null && runtime.Api.DevicePorts.TryGetValue(serial, out var devicePort) => devicePort,
            _ => null
        };
        return new(runtime, protocol, serial, port);
    }
    private bool OwnsApiEndpoint() => ReadApiEndpoint().Port is > 0;
    private void InvalidateApiResponse()
    {
        apiRequestRevision++;
        apiResponse.Clear();
    }
    private void RefreshApiEndpoint()
    {
        var next = ReadApiEndpoint();
        if (next == apiEndpoint) return;
        apiEndpoint = next;
        InvalidateApiResponse();
    }
    private Task SendApiTestRequestAsync() => ViewModel.ExecuteAsync("测试 API", async () =>
    {
        RefreshApiEndpoint();
        InvalidateApiResponse();
        var endpoint = apiEndpoint!;
        var revision = apiRequestRevision;
        var method = apiMethod.SelectedItem?.ToString() ?? "GET";
        var route = apiRoute.Text;
        var payload = apiPayload.Text;
        var port = endpoint.Port ?? throw new InvalidOperationException("所选本机接口未由当前程序启动");
        if (method is not ("GET" or "POST")) throw new InvalidOperationException("请求方法必须为 GET 或 POST");
        if (!route.StartsWith('/') || route.StartsWith("//", StringComparison.Ordinal) || route.Contains("://", StringComparison.Ordinal) || route.Contains('\\') || route.Any(char.IsControl) ||
            !Uri.TryCreate($"http://127.0.0.1:{port}{route}", UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttp ||
            uri.Host != "127.0.0.1" || uri.Port != port || uri.UserInfo.Length != 0 || uri.Fragment.Length != 0)
            throw new InvalidOperationException("路由必须为当前本机接口的相对路径：" + route);
        if (method == "POST") { using var validation = JsonDocument.Parse(payload); }
        var protocol = endpoint.Protocol switch { 1 => "自动化接口", 2 => "设备接口", _ => "普通接口" };
        var source = $"来源：{protocol} http://127.0.0.1:{port}\n" +
            (endpoint.Serial is { } serial ? $"设备串号：{serial}\n" : "") + $"请求：{method} {route}\n";
        using var handler = new HttpClientHandler { UseProxy = false, AllowAutoRedirect = false };
        using var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(30) };
        using var request = new HttpRequestMessage(new HttpMethod(method), uri);
        if (method == "POST") request.Content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json");
        try
        {
            using var result = await client.SendAsync(request);
            var body = await result.Content.ReadAsStringAsync();
            RefreshApiEndpoint();
            // A context changed away and back still invalidates this original request.
            if (revision == apiRequestRevision && endpoint == apiEndpoint)
                apiResponse.Text = source + $"HTTP {(int)result.StatusCode} {result.ReasonPhrase}\n" + body;
        }
        catch (TaskCanceledException ex) { throw new TimeoutException("本机接口请求超时\n" + source, ex); }
    }, blocksApiWrites: false);
    private FrameworkElement BuildApiPage()
    {
        var page = Page(); var toolbar = Toolbar();
        var startGateways = Button("\uE768", "启动网关", ViewModel.StartApiCommand);
        var stopAll = Button("\uE71A", "停止全部接口", ViewModel.StopApiCommand);
        var startDevices = Button("\uE968", "启用设备接口", ViewModel.StartDeviceApiCommand);
        var stopDevices = Button("\uE711", "停止设备接口", ViewModel.StopDeviceApiCommand);
        foreach (var button in new[] { startGateways, stopAll, startDevices, stopDevices }) toolbar.Children.Add(button);
        DockPanel.SetDock(toolbar, Dock.Top); page.Children.Add(toolbar);
        var summary = BoundText(nameof(WorkspaceViewModel.ApiSummary), true); summary.Margin = new Thickness(0, 10, 0, 10); DockPanel.SetDock(summary, Dock.Top); page.Children.Add(summary);
        apiProtocol = new ComboBox { ItemsSource = new[] { "普通 API", "Automator API", "设备接口" }, ItemTemplate = CaptionChoices(), SelectedIndex = 0, MinHeight = 44 };
        apiMethod = new ComboBox { ItemsSource = new[] { "GET", "POST" }, SelectedIndex = 0, MinHeight = 44 };
        apiRoute = new TextBox { Text = "/health", MinHeight = 44, VerticalContentAlignment = VerticalAlignment.Center, HorizontalScrollBarVisibility = ScrollBarVisibility.Auto };
        apiResponse = new TextBox { IsReadOnly = true, AcceptsReturn = true, Height = 180, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, FontFamily = new FontFamily("Consolas"), TextWrapping = TextWrapping.Wrap };
        apiPayload = new TextBox { Text = "{}", AcceptsReturn = true, Height = 120, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, TextWrapping = TextWrapping.Wrap, FontFamily = new FontFamily("Consolas") };
        System.Windows.Automation.AutomationProperties.SetName(apiProtocol, "接口类型"); System.Windows.Automation.AutomationProperties.SetName(apiMethod, "请求方法");
        System.Windows.Automation.AutomationProperties.SetName(apiRoute, "请求路径"); System.Windows.Automation.AutomationProperties.SetName(apiPayload, "请求正文"); System.Windows.Automation.AutomationProperties.SetName(apiResponse, "响应结果");
        apiSend = new AsyncCommand(SendApiTestRequestAsync, () => !ViewModel.Busy && !ViewModel.Transferring && OwnsApiEndpoint());
        var send = Button("\uE768", "发送请求", apiSend);
        send.HorizontalAlignment = HorizontalAlignment.Left; send.Margin = new Thickness(0, 0, 0, 9);
        var requestPanel = new StackPanel();
        requestPanel.Children.Add(Field("接口类型", apiProtocol, 110));
        requestPanel.Children.Add(Field("请求方法", apiMethod, 110));
        requestPanel.Children.Add(Field("请求路径", apiRoute, 110));
        requestPanel.Children.Add(Field("请求正文", apiPayload, 110));
        requestPanel.Children.Add(send);
        requestPanel.Children.Add(Field("响应结果", apiResponse, 110));
        var grid = new DataGrid { ItemsSource = ViewModel.DeviceView, IsSynchronizedWithCurrentItem = false, IsReadOnly = false, Height = 240, MinRowHeight = 44, ColumnHeaderHeight = 44 };
        ScrollViewer.SetHorizontalScrollBarVisibility(grid, ScrollBarVisibility.Auto);
        ScrollViewer.SetVerticalScrollBarVisibility(grid, ScrollBarVisibility.Auto);
        grid.SetBinding(Selector.SelectedItemProperty, new Binding(nameof(WorkspaceViewModel.CurrentDevice)) { Mode = BindingMode.OneWay });
        grid.SelectionChanged += (_, _) => { if (Equals((tabs.SelectedItem as TabItem)?.Tag, "API") && grid.SelectedItem is DeviceRow selected) ViewModel.CurrentDevice = selected; };
        var selectedColumn = new DataGridCheckBoxColumn { Binding = TwoWay(nameof(DeviceRow.IsSelected)), IsReadOnly = false, Width = 44 }; LocalizationService.Bind(selectedColumn, DataGridColumn.HeaderProperty, "选中"); grid.Columns.Add(selectedColumn); TextColumn(grid, "serial", nameof(DeviceRow.Serial), 210); TextColumn(grid, "设备", nameof(DeviceRow.Name), 180); TextColumn(grid, "分组", nameof(DeviceRow.Group), 100); TextColumn(grid, "设备状态", nameof(DeviceRow.State), 120); TextColumn(grid, "设备接口地址", nameof(DeviceRow.ApiAddress), 280); grid.MouseDoubleClick += (_, _) => { if (grid.SelectedItem is DeviceRow row && row.ApiAddress != "未启动") Clipboard.SetText(row.ApiAddress); };
        foreach (var column in grid.Columns.OfType<DataGridTextColumn>())
        {
            var textStyle = new Style(typeof(TextBlock), column.ElementStyle);
            textStyle.Setters.Add(new Setter(TextBlock.TextTrimmingProperty, TextTrimming.CharacterEllipsis));
            textStyle.Setters.Add(new Setter(ToolTipProperty, column.Binding));
            column.ElementStyle = textStyle;
        }
        var sections = new Grid();
        sections.ColumnDefinitions.Add(new ColumnDefinition()); sections.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0) });
        sections.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); sections.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        Grid.SetRow(requestPanel, 1); requestPanel.Margin = new Thickness(0, 18, 0, 0);
        sections.Children.Add(grid); sections.Children.Add(requestPanel);
        var viewport = new ScrollViewer { Content = sections, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled, HorizontalContentAlignment = HorizontalAlignment.Stretch };
        bool? singleColumn = null;
        void ArrangeSections()
        {
            if (sections.ActualWidth <= 0) return;
            var narrow = sections.ActualWidth < 900;
            if (singleColumn != narrow)
            {
                singleColumn = narrow;
                sections.ColumnDefinitions[1].Width = narrow ? new GridLength(0) : new GridLength(1, GridUnitType.Star);
                sections.RowDefinitions[1].Height = narrow ? GridLength.Auto : new GridLength(0);
                Grid.SetColumn(requestPanel, narrow ? 0 : 1); Grid.SetRow(requestPanel, narrow ? 1 : 0);
                requestPanel.Margin = narrow ? new Thickness(0, 18, 0, 0) : new Thickness(18, 0, 0, 0);
            }
            grid.Height = narrow ? 240 : Math.Max(240, viewport.ActualHeight - 4);
        }
        sections.SizeChanged += (_, _) => ArrangeSections(); viewport.SizeChanged += (_, _) => ArrangeSections(); viewport.Loaded += (_, _) => ArrangeSections();
        page.Children.Add(viewport);
        foreach (var control in new Control[] { startGateways, stopAll, startDevices, stopDevices, send, apiProtocol, apiMethod, apiRoute, apiPayload, apiResponse }) control.MinHeight = Math.Max(44, control.MinHeight);
        foreach (var button in new[] { startGateways, stopAll, startDevices, stopDevices, send }) { button.MinWidth = Math.Max(44, button.MinWidth); ToolTipService.SetShowOnDisabled(button, true); }
        void Hint(Button button, string available, string unavailable)
        {
            var hint = button.Command?.CanExecute(null) == true ? available : ViewModel.Busy ? "正在处理其他操作，请等待完成。" : unavailable;
            button.SetCurrentValue(ToolTipProperty, hint);
            System.Windows.Automation.AutomationProperties.SetHelpText(button, hint);
        }
        void RefreshHints()
        {
            var targets = ViewModel.Targets();
            var startReason = targets.Count == 0 ? "请选择已连接的目标设备。" : targets.Any(row => !row.Info.Online) ? "目标设备尚未全部就绪。" :
                targets.Any(row => ViewModel.Runtime.Scripts.GetStatus(row.Serial).State.ToString() is "Running" or "Cancelling") ? "目标设备的脚本正在执行，请等待完成或停止脚本。" : "目标设备接口均已启动。";
            Hint(startGateways, "启动普通接口与自动化接口。", "两个网关均已启动。");
            Hint(stopAll, "停止两个网关和全部设备接口。", "当前没有已启动的接口。");
            Hint(startDevices, "启用所选目标的设备接口。", startReason);
            Hint(stopDevices, "停止所选目标的设备接口。", targets.Count == 0 ? "请选择目标设备。" : "所选目标没有已启动的设备接口。");
            Hint(send, "发送到当前程序启动的本机接口。", ViewModel.Transferring ? "正在传输文件，请等待完成。" : apiProtocol.SelectedIndex == 2 && ViewModel.CurrentDevice is null ? "请选择要测试设备接口的设备。" : "所选本机接口尚未启动。");
        }
        void RefreshRequestState() { RefreshApiEndpoint(); apiSend.Refresh(); RefreshHints(); }
        apiProtocol.SelectionChanged += (_, _) => RefreshRequestState();
        apiMethod.SelectionChanged += (_, _) => InvalidateApiResponse();
        apiRoute.TextChanged += (_, _) => InvalidateApiResponse(); apiPayload.TextChanged += (_, _) => InvalidateApiResponse();
        apiSend.CanExecuteChanged += (_, _) => RefreshHints();
        ViewModel.PropertyChanged += (_, _) => RefreshRequestState();
        RefreshRequestState();
        return page;
    }

}
