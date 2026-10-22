using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using MatrixDevice.Core;
using MatrixDevice.Features;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    internal async Task RunLiveUiAcceptanceAsync(string output)
    {
        if (ViewModel.Demo || !Path.IsPathFullyQualified(output) || ViewModel.Runtime.Settings.AdbAllowedSerials.Length != 1 ||
            ViewModel.Runtime.Settings.LdSerialBindings.Count != 1 || ViewModel.Runtime.Settings.LdSerialBindings.Single().Key <= 0)
            throw new InvalidOperationException("实机界面验收只允许独立目录内明确授权的一个非零雷电实例");
        Directory.CreateDirectory(output);
        var records = new List<object>();
        async Task Check(string id, Func<Task> action)
        {
            var start = DateTimeOffset.UtcNow;
            try
            {
                await action();
                if (!string.IsNullOrEmpty(ViewModel.Error)) throw new InvalidOperationException(ViewModel.Error);
                records.Add(new { id, start, status = "PASS-LIVE-UI", serial = ViewModel.CurrentDevice?.Serial, generation = ViewModel.CurrentDevice?.Info.Generation });
            }
            catch (Exception ex) { records.Add(new { id, start, status = "FAIL", error = ex.Message }); throw; }
            finally { await File.WriteAllTextAsync(Path.Combine(output, "ui-results.json"), JsonSerializer.Serialize(records, JsonDefaults.Options)); }
        }
        await Check("UI-connect", async () =>
        {
            await ViewModel.RefreshDevicesAsync();
            if (ViewModel.Devices.Count != 1 || !ViewModel.Devices[0].Info.Online || ViewModel.Devices[0].Serial != ViewModel.Runtime.Settings.AdbAllowedSerials.Single())
                throw new InvalidOperationException("独立测试目标未就绪或串号不符");
            await ViewModel.SetMainAsync(ViewModel.Devices[0]);
        });
        await Check("UI-screenshot", async () =>
        {
            await ViewModel.CaptureAsync(); ViewModel.SamplePixel(10, 10);
            await ViewModel.SaveScreenshotAsync(Path.Combine(output, "guest.png"));
            await ViewModel.SaveScreenshotAsync(Path.Combine(output, "crop.png"), new Int32Rect(10, 10, 40, 40));
            if (PngImage.ReadDimensions(await File.ReadAllBytesAsync(Path.Combine(output, "crop.png"))) != (40, 40)) throw new InvalidOperationException("界面裁剪尺寸不符");
        });
        await Check("UI-nodes", async () =>
        {
            await ViewModel.LoadNodesAsync();
            var node = ViewModel.FlattenNodes().FirstOrDefault(row => row.Source.Bounds.Width > 0) ?? throw new InvalidOperationException("界面真实节点为空");
            ViewModel.SelectedNode = node; NavigateTo("节点"); await Dispatcher.InvokeAsync(UpdateLayout, DispatcherPriority.Render);
            if (!nodeTreeItems.TryGetValue(node, out var item) || !item.IsSelected || !nodeRectangles.ContainsKey(node)) throw new InvalidOperationException("真实节点树与矩形选择未联动");
            await ViewModel.ExportNodesAsync(Path.Combine(output, "nodes.json"));
        });
        await Check("UI-apps-shell", async () =>
        {
            await ViewModel.LoadAppsAsync();
            if (!ViewModel.Apps.Any(app => app.PackageName == "com.ss.android.ugc.aweme" && app.VersionName == "40.7.0")) throw new InvalidOperationException("界面APP版本列表不符");
            ViewModel.ShellInput = "getprop sys.boot_completed"; await ViewModel.ExecuteShellAsync();
        });
        await Check("UI-file-queue", async () =>
        {
            var row = ViewModel.CurrentDevice!; var local = Path.Combine(output, "ui-upload.txt"); var pulled = Path.Combine(output, "ui-pulled.txt");
            var remote = "/sdcard/matrix-ui-acceptance-" + Guid.NewGuid().ToString("N") + ".txt";
            await File.WriteAllTextAsync(local, "界面传输验收 " + Guid.NewGuid());
            try
            {
                ViewModel.Transfers.Add(new TransferRow { Serial = row.Serial, Direction = "上传", LocalPath = local, RemotePath = remote });
                ViewModel.Transfers.Add(new TransferRow { Serial = row.Serial, Direction = "下载", LocalPath = pulled, RemotePath = remote });
                await ViewModel.StartTransfersAsync();
                if (ViewModel.Transfers.Any(task => task.State != "传输完成") || !File.ReadAllBytes(local).SequenceEqual(File.ReadAllBytes(pulled))) throw new InvalidOperationException("界面文件队列失败或哈希不一致");
            }
            finally { await ViewModel.Runtime.Backend.ShellAsync(row.Serial, "rm -f " + AdbDeviceBackend.Quote(remote)); }
        });
        await Check("UI-text", async () =>
        {
            var serial = ViewModel.CurrentDevice!.Serial;
            await ViewModel.Runtime.Backend.StopAppAsync(serial, "com.android.settings.intelligence"); await ViewModel.Runtime.Backend.StopAppAsync(serial, "com.android.settings");
            await ViewModel.Runtime.Backend.ShellAsync(serial, "am start -W -a android.settings.SETTINGS"); await Task.Delay(800);
            var control = NodeSearch.FindAll(await ViewModel.Runtime.Backend.DumpAsync(serial), new()).First(node => node.PackageName == "com.android.settings" && (node.Text ?? "").Contains("搜索", StringComparison.Ordinal) && node.Bounds.Width > 0);
            await ViewModel.CaptureAsync();
            var row = ViewModel.CurrentDevice!;
            await ViewModel.SendInputAsync(new InputSnapshot { SourceSerial = serial, SourceGeneration = row.Info.Generation, Generation = ViewModel.Runtime.Input.CurrentGeneration,
                Kind = InputKind.Tap, X = control.Bounds.CenterX, Y = control.Bounds.CenterY, SourceWidth = row.Screenshot!.PixelWidth, SourceHeight = row.Screenshot.PixelHeight });
            await Task.Delay(500);
            ViewModel.TextInput = "界面中文 Test 8";
            if (!ViewModel.TextCommand.CanExecute(null)) throw new InvalidOperationException("实机文本按钮不允许操作");
            await ViewModel.SendTextAsync();
            var fields = NodeSearch.FindAll(await ViewModel.Runtime.Backend.DumpAsync(serial), new() { ResourceId = "android:id/search_src_text" });
            if (!fields.Any(node => node.Text == ViewModel.TextInput)) throw new InvalidOperationException("界面中文输入精确回读不符");
            await File.WriteAllTextAsync(Path.Combine(output, "text-readback.json"), JsonSerializer.Serialize(fields, JsonDefaults.Options));
        });
        await Check("UI-clear-stale-nodes", async () =>
        {
            await ViewModel.CaptureAsync();
            if (ViewModel.Nodes.Count != 0 || ViewModel.SelectedNode is not null || nodeTree.Items.Count != 0 || nodeRectangles.Count != 0) throw new InvalidOperationException("实机新截图仍留旧节点");
            await ViewModel.LoadNodesAsync();
        });
        await Check("UI-navigation", async () =>
        {
            var selected = ViewModel.CurrentDevice; var main = ViewModel.MainDevice;
            foreach (var tab in tabs.Items.Cast<TabItem>())
            {
                NavigateTo(tab.Tag!.ToString()!); await Dispatcher.InvokeAsync(UpdateLayout, DispatcherPriority.Render);
                if (!ReferenceEquals(selected, ViewModel.CurrentDevice) || !ReferenceEquals(main, ViewModel.MainDevice)) throw new InvalidOperationException("实机导航改变目标");
                SaveRender(Path.Combine(output, $"page-{tabs.SelectedIndex:D2}.png"));
            }
            NavigateTo("设备墙 / 主控");
            SetWallViewMode("list"); await Dispatcher.InvokeAsync(UpdateLayout, DispatcherPriority.Render);
            if (WallViewMode != "list") throw new InvalidOperationException("设备管理不再保持唯一列表模式。");
            SaveRender(Path.Combine(output, "wall-list.png"));
        });
        await File.WriteAllTextAsync(Path.Combine(output, "build-sha256.txt"), Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(typeof(MainWindow).Assembly.Location))));
    }
}
