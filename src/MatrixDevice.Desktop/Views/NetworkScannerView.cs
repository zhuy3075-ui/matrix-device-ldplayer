using System.Windows;
using System.Windows.Controls;
using MatrixDevice.Core;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    private CancellationTokenSource? scanCancellation;
    private Task<NetworkScanResult>? scanOperation;

    private FrameworkElement BuildScanner()
    {
        var panel = new StackPanel { Margin = new Thickness(0, 5, 0, 7) };
        var startIp = new TextBox { Text = "127.0.0.1" };
        var endIp = new TextBox { Text = "127.0.0.1" };
        var port = new TextBox { Text = "5555" };
        var timeout = new TextBox { Text = "1500" };
        panel.Children.Add(Field("起始 IP", startIp, 70));
        panel.Children.Add(Field("结束 IP", endIp, 70));
        panel.Children.Add(Field("端口", port, 70));
        panel.Children.Add(Field("超时 ms", timeout, 70));
        var summary = new TextBlock { Text = "未扫描", TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 6, 0, 0) };
        AsyncCommand? scan = null;
        ActionCommand? cancel = null;
        scan = new AsyncCommand(async () =>
        {
            if (ViewModel.Runtime.Backend is not AdbDeviceBackend backend || ViewModel.Demo) return;
            if (!int.TryParse(port.Text, out var remotePort) || !int.TryParse(timeout.Text, out var timeoutMs)) { summary.Text = "端口和超时必须为整数"; return; }
            scanCancellation = new CancellationTokenSource(); cancel?.Refresh(); scan?.Refresh();
            startIp.IsEnabled = endIp.IsEnabled = port.IsEnabled = timeout.IsEnabled = false;
            try
            {
                var progress = new Progress<NetworkScanProgress>(value => summary.Text = $"探测 {value.Completed}/{value.Total}\n发现 {value.Found} 连接 {value.Connected}\n{value.CurrentAddress}");
                scanOperation = new NetworkDeviceScanner(backend).ScanAsync(startIp.Text.Trim(), endIp.Text.Trim(), remotePort, probeTimeoutMs: timeoutMs, progress: progress, ct: scanCancellation.Token);
                var result = await scanOperation;
                if (closing) return;
                summary.Text = $"扫描结束\n目标 {result.Total} 发现 {result.Found} 连接 {result.Connected}";
                await ViewModel.RefreshDevicesAsync();
                if (result.Endpoints.Any(endpoint => endpoint.Found && !endpoint.Connected)) ShowText("网络发现结果", string.Join("\n", result.Endpoints.Select(endpoint => $"{endpoint.Address}:{endpoint.Port} · {(endpoint.Connected ? "已连接" : endpoint.Found ? "连接失败" : "未发现")} · {endpoint.Error}")));
            }
            catch (OperationCanceledException) { summary.Text += "\n扫描已停止"; }
            catch (Exception ex) { summary.Text = "扫描失败：" + ViewModel.PresentError(ex); }
            finally
            {
                scanCancellation.Dispose(); scanCancellation = null; scanOperation = null;
                startIp.IsEnabled = endIp.IsEnabled = port.IsEnabled = timeout.IsEnabled = true;
                cancel?.Refresh(); scan?.Refresh();
            }
        }, () => scanCancellation is null && !ViewModel.Demo && ViewModel.Runtime.Backend is AdbDeviceBackend && !ViewModel.Busy);
        cancel = new ActionCommand(() => scanCancellation?.Cancel(), () => scanCancellation is not null);
        var controls = Toolbar(); controls.Children.Add(Button("\uE721", "扫描", scan)); controls.Children.Add(Button("\uE71A", "停止", cancel)); panel.Children.Add(controls); panel.Children.Add(summary);
        ViewModel.PropertyChanged += (_, _) => scan.Refresh();
        var header = new TextBlock { MaxWidth = 185, TextWrapping = TextWrapping.Wrap };
        LocalizationService.Bind(header, TextBlock.TextProperty, ViewModel.Demo ? "网络发现 · 演示模式禁用" : "网络发现");
        var expander = new Expander { Header = header, Content = panel, Margin = new Thickness(0, 6, 0, 0) };
        return expander;
    }
}
