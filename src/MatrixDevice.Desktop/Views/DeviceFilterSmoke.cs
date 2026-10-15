using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Threading;
using MatrixDevice.Core;
using MatrixDevice.Host;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    private async Task VerifyDeviceFilterIsolationAsync()
    {
        if (!ViewModel.Demo) throw new InvalidOperationException("设备筛选隔离验收仅限演示模式。");
        var checks = new List<string>(); var observations = new List<object>(); var matrixCount = 0;
        void Check(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); checks.Add(message); }
        async Task Settle() { await Dispatcher.InvokeAsync(UpdateLayout, DispatcherPriority.ContextIdle); await Dispatcher.InvokeAsync(UpdateLayout, DispatcherPriority.Render); await Dispatcher.InvokeAsync(UpdateLayout, DispatcherPriority.ApplicationIdle); }
        var status = deviceStatusFilter; var scope = monitorScopeChoice.SelectedIndex;
        var current = ViewModel.CurrentDevice; var main = ViewModel.MainDevice;
        var selection = unifiedDevices.ToDictionary(row => row.Key, row => row.IsSelected);
        var appSelection = applicationTargets.Select(row => (row.Serial, row.IsSelected)).ToArray();
        var infos = ViewModel.Devices.ToDictionary(row => row, row => row.Info);
        var a = ViewModel.Devices[0]; var b = ViewModel.Devices[1];
        var metadata = ViewModel.Runtime.Metadata.GetMetadata(b.Serial);
        try
        {
            NavigateTo("设备墙 / 主控"); deviceStatusChoice.SelectedItem = "全部状态"; await Settle();
            Check(deviceStatusChoice.IsVisible && deviceStatusChoice.Items.Cast<string>().SequenceEqual(new[] { "全部状态", "已连接", "已关闭", "启动中", "待连接", "离线", "未授权", "已发现", "异常", "未知", "受保护" }), "列表唯一固定状态下拉可见");
            var grid = VisualDescendants<DataGrid>(wallList).Single();
            var rowA = unifiedDevices.Single(row => row.Device == a); var rowB = unifiedDevices.Single(row => row.Device == b);
            grid.SelectedItem = rowA; await Settle(); Check(ViewModel.CurrentDevice == a && grid.SelectedItem == rowA, "先建立真实列表高亮A");
            b.Info = b.Info with { State = "Offline" }; deviceStatusChoice.SelectedItem = "离线"; await Settle();
            Check(unifiedDeviceView.Cast<UnifiedDeviceRow>().SequenceEqual([rowB]) && ViewModel.CurrentDevice == a && ViewModel.MainDevice == main, "筛选只显示B时不把高亮A的操作目标自动换成B");
            grid.SelectedItem = rowB; await Settle(); Check(ViewModel.CurrentDevice == b && ViewModel.MainDevice == main, "显式选择可见B仍更新单设备目标，不改主控");
            Check(selection.All(pair => unifiedDevices.Single(row => row.Key == pair.Key).IsSelected == pair.Value) && appSelection.SequenceEqual(applicationTargets.Select(row => (row.Serial, row.IsSelected))), "筛选和高亮不改变群控/应用独立选择");

            ShowDeviceDirectory(); await Settle();
            var details = directoryWindow!; var list = VisualDescendants<ListBox>(details).Single(input => input.Name == "DetailsDeviceChoice");
            Check(list.Items.Cast<DeviceRow>().SequenceEqual(ViewModel.Devices) && !ReferenceEquals(list.ItemsSource, ViewModel.DeviceView), "详情独立未过滤目录仍完整");
            Check(!VisualDescendants<ComboBox>(details).Any() && !VisualDescendants<CheckBox>(details).Any() && !VisualDescendants<Button>(details).Any(button => button.ToolTip as string is "全选筛选" or "清空"), "详情无全局筛选/全选清空/群控勾选");
            foreach (var caption in new[] { "画面", "节点", "脚本", "接口" }) Check(VisualDescendants<TextBlock>(details).Any(text => text.Text.StartsWith(caption, StringComparison.Ordinal)), "详情保留子系统状态：" + caption);
            list.SetCurrentValue(Selector.SelectedItemProperty, a); await Settle(); Check(ViewModel.CurrentDevice == a && ViewModel.MainDevice == main, "详情显式单设备选择A生效");
            list.SetCurrentValue(Selector.SelectedItemProperty, b); await Settle();
            const string retainedGroup = "  A05原分组  "; const string renamed = "A05详情实际保存";
            b.Info = b.Info with { GroupId = retainedGroup };
            var name = VisualDescendants<TextBox>(details).Single(input => BindingOperations.GetBinding(input, TextBox.TextProperty)?.Path.Path == nameof(WorkspaceViewModel.DeviceName)); name.SetCurrentValue(TextBox.TextProperty, renamed); name.GetBindingExpression(TextBox.TextProperty)!.UpdateSource();
            var save = VisualDescendants<Button>(details).Single(button => ReferenceEquals(button.Command, ViewModel.SaveMetadataCommand));
            Check(save.IsEnabled, "详情实际保存命令可用");
            ((IInvokeProvider)new ButtonAutomationPeer(save).GetPattern(PatternInterface.Invoke)).Invoke();
            var deadline = System.Diagnostics.Stopwatch.StartNew();
            while (ViewModel.Busy || ViewModel.Runtime.Metadata.GetMetadata(b.Serial).Name != renamed)
            {
                if (deadline.Elapsed >= TimeSpan.FromSeconds(3)) throw new InvalidOperationException("详情实际保存未在3秒内完成：" + ViewModel.Error);
                await Settle(); await Task.Delay(20);
            }
            using (var fresh = new CatalogRepository(Path.Combine(ViewModel.DataDirectory, "db.sqlite3")))
            {
                var stored = fresh.GetMetadata(b.Serial); Check(stored.Name == renamed && stored.GroupId == retainedGroup && b.Info.GroupId == retainedGroup, "新数据库连接证明改名持久化且GroupId原样保留");
            }
            details.Hide(); b.Info = infos[b]; ViewModel.CurrentDevice = current; deviceStatusChoice.SelectedItem = "已关闭"; await Settle();
            Check(unifiedDeviceView.Cast<UnifiedDeviceRow>().Any(row => row.Instance is { Index: > 0 } && row.Device is null), "已关闭筛选覆盖没有Device的全停实例");

            NavigateTo("实时监控"); monitorScopeChoice.SelectedIndex = 1; await Settle(); await monitorReconcile; await ReconcileMonitorAsync(); monitorTimer.Stop();
            Check(monitorTiles.Keys.ToHashSet().SetEquals(unifiedDevices.Select(row => row.Key)), "详情隐藏和列表已关闭筛选不使监控全部目录丢项");
            SelectMonitor(monitorTiles.Keys.First()); await monitorReconcile; monitorTimer.Stop(); await Settle();
            foreach (var connectedOnly in new[] { false, true })
            {
                monitorScopeChoice.SelectedIndex = connectedOnly ? 0 : 1; await monitorReconcile; await ReconcileMonitorAsync(); await Settle(); monitorTimer.Stop();
                var expected = unifiedDevices.Where(row => !connectedOnly || row.Connected).Select(row => row.Key).ToHashSet();
                Check(monitorTiles.Keys.ToHashSet().SetEquals(expected), "监控自身范围生效：" + connectedOnly);
                var epoch = monitorEpoch; var selectedMonitor = monitorSelected;
                var tiles = monitorTiles.ToDictionary(pair => pair.Key, pair => (Tile: pair.Value, pair.Value.Signature, pair.Value.Eligible));
                foreach (var category in DeviceStatusOptions)
                {
                    deviceStatusChoice.SelectedItem = category; await Settle(); await ReconcileMonitorAsync();
                    Check(monitorEpoch == epoch && monitorSelected == selectedMonitor && monitorTiles.Count == tiles.Count && tiles.All(pair => monitorTiles.TryGetValue(pair.Key, out var tile) && ReferenceEquals(tile, pair.Value.Tile) && tile.Signature == pair.Value.Signature && tile.Eligible == pair.Value.Eligible), "列表筛选不改监控epoch/卡片/签名/资格/选择：" + connectedOnly + "/" + category);
                    observations.Add(new { connectedOnly, category, epoch, monitorEpoch, selectedMonitor, monitorSelected, monitorKeys = monitorTiles.Keys.ToArray(), listKeys = unifiedDeviceView.Cast<UnifiedDeviceRow>().Select(row => row.Key).ToArray() });
                }
            }

            var stopped = new LdPlayerInstance(8, "状态矩阵", 0, 0, false, -1, -1);
            var running = stopped with { AndroidStarted = true, ProcessId = 81, VBoxProcessId = 82, TopWindow = 83, BindWindow = 84 };
            var matrix = new (string Case, LdPlayerInstance? Instance, string? Raw, bool Connected, string Phase, string Expected)[]
            {
                ("全停无Device", stopped, null, false, "", "已关闭"), ("全停+旧Ready", stopped, "Ready", false, "已取消", "未知"),
                ("transition2", stopped with { IsStarting = true }, null, false, "已连接", "启动中"),
                ("单播放器PID", stopped with { ProcessId = 1 }, null, false, "", "未知"), ("单虚拟机PID", stopped with { VBoxProcessId = 1 }, null, false, "", "未知"),
                ("单顶层HWND", stopped with { TopWindow = 1 }, null, false, "", "未知"), ("单绑定HWND", stopped with { BindWindow = 1 }, null, false, "", "未知"),
                ("负顶层HWND", stopped with { TopWindow = -1 }, null, false, "", "未知"), ("负绑定HWND", stopped with { BindWindow = -1 }, null, false, "", "未知"),
                ("安卓true无PID", stopped with { AndroidStarted = true }, "Ready", false, "", "未知"),
                ("0保护优先", running with { Index = 0 }, "Ready", true, "已连接", "受保护"), ("99999保护优先", running with { Index = 99999 }, "Ready", true, "", "受保护"),
                ("已核验连接", running, "Ready", true, "启动失败", "已连接"), ("rawReady未核验", running, "Ready", false, "已连接", "待连接"),
                ("运行未发现Device", running, null, false, "", "待连接"), ("未知raw", running, "FutureState", false, "已连接", "未知"),
                ("无实例rawReady未核验", null, "Ready", false, "", "未知"), ("无实例已核验", null, "Ready", true, "已取消", "已连接"),
                ("离线", null, "Offline", false, "已连接", "离线"), ("未授权", running, "Unauthorized", false, "", "未授权"),
                ("已发现", null, "Discovered", false, "", "已发现"), ("异常", null, "Error", false, "", "异常"),
                ("phase伪连接全停", stopped, null, false, "已连接", "已关闭"), ("phase取消运行", running, "Ready", true, "已取消", "已连接"),
                ("全停明确Offline", stopped, "Offline", false, "", "已关闭"), ("全停Offline但Connected", stopped, "Offline", true, "", "未知"),
                ("全停无Device但Connected", stopped, null, true, "", "未知"), ("全停未知raw", stopped, "FutureState", false, "", "未知"),
                ("全停授权状态矛盾", stopped, "Unauthorized", false, "", "未知"), ("运行Offline但Connected", running, "Offline", true, "", "未知"),
                ("运行无Device但Connected", running, null, true, "", "未知"), ("无设备无实例但Connected", null, null, true, "", "未知")
            };
            matrixCount = matrix.Length;
            foreach (var item in matrix)
            {
                var projected = new UnifiedDeviceRow("matrix", _ => Task.CompletedTask, _ => false, _ => Task.CompletedTask, _ => false, _ => Task.CompletedTask);
                var device = item.Raw is null ? null : new DeviceRow(new DeviceInfo { Serial = "matrix-device", State = item.Raw });
                projected.Update(item.Instance, device, item.Connected, item.Phase);
                Check(projected.FilterState == item.Expected && projected.State == item.Expected && projected.StateDetails.Contains(item.Raw ?? "无设备连接", StringComparison.Ordinal) && projected.StateDetails.Contains(string.IsNullOrWhiteSpace(item.Phase) ? "无" : item.Phase, StringComparison.Ordinal), "真实字段分类且不读phase：" + item.Case);
            }
        }
        finally
        {
            ViewModel.Runtime.Metadata.UpsertMetadata(b.Serial, metadata.Name, metadata.GroupId);
            foreach (var pair in infos) pair.Key.Info = pair.Value;
            ViewModel.CurrentDevice = current; deviceStatusChoice.SelectedItem = status; monitorScopeChoice.SelectedIndex = scope;
            foreach (var row in unifiedDevices) row.IsSelected = selection.GetValueOrDefault(row.Key);
            directoryWindow?.Hide(); NavigateTo("设备墙 / 主控"); await StopMonitorAsync();
        }
        Check(ViewModel.MainDevice == main && ViewModel.CurrentDevice == current && selection.All(pair => unifiedDevices.Single(row => row.Key == pair.Key).IsSelected == pair.Value) && appSelection.SequenceEqual(applicationTargets.Select(row => (row.Serial, row.IsSelected))), "隔离验收恢复原目标/主控/选择");
        File.WriteAllText(Path.Combine(ViewModel.DataDirectory, "device-filter-isolation.json"), JsonSerializer.Serialize(new { status = "PASS-OFFLINE", scope = "actual demo directory/list filter and Monitor reconciliation; no real streams or emulators", assertions = checks.Count, checks, observations, matrixCases = matrixCount }, JsonDefaults.Options));
    }
}
