using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Threading;
using MatrixDevice.Core;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    internal async Task RunMonitorAcceptanceAsync(string output)
    {
        var runtime = ViewModel.Runtime;
        var bindings = runtime.Settings.LdSerialBindings;
        if (ViewModel.Demo || !Path.IsPathFullyQualified(output) || bindings.Count != 2 || !bindings.ContainsKey(1) || !bindings.ContainsKey(2)
            || runtime.Settings.AdbAllowedSerials.Length != 2 || bindings.Values.Distinct(StringComparer.Ordinal).Count() != 2 || !bindings.Values.ToHashSet(StringComparer.Ordinal).SetEquals(runtime.Settings.AdbAllowedSerials)
            || runtime.Settings.AutoStartApi || runtime.Settings.AgentBaseUrl is not null)
            throw new InvalidOperationException("监控实机验收仅允许独立配置中已授权的测试实例1、2，禁用API与Agent。");
        Directory.CreateDirectory(output);
        var runId = Guid.NewGuid().ToString("N");
        var records = new List<object>();
        var observations = new List<object>();
        var started = DateTimeOffset.UtcNow;
        var passed = false;
        using var suiteDeadline = new CancellationTokenSource(TimeSpan.FromMinutes(8));
        await File.WriteAllTextAsync(Path.Combine(output, "summary.json"), JsonSerializer.Serialize(new { runId, started, running = true, passed = false }, JsonDefaults.Options));
        async Task Observe(string stage)
        {
            observations.Add(new
            {
                stage, at = DateTimeOffset.UtcNow, elapsedSeconds = (DateTimeOffset.UtcNow - started).TotalSeconds,
                page = (tabs.SelectedItem as System.Windows.Controls.TabItem)?.Tag, window = WindowState.ToString(), IsActive,
                monitorEpoch, monitorPaused, active = MonitorActive, compact = monitorCompact,
                viewport = new { monitorScroll.ActualWidth, monitorScroll.ActualHeight, monitorScroll.ViewportHeight },
                host = monitorLatestHost, hostDisplay = monitorHost.Text,
                tiles = monitorTiles.Values.Select(tile => new
                {
                    tile.Key, tile.Signature, serial = tile.Device?.Serial, visible = tile.VisibleIn(monitorScroll),
                    session = tile.Session is null ? (int?)null : System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(tile.Session),
                    running = tile.Session?.IsRunning, tile.Received, tile.Presented, tile.LastFrameAt, tile.Failed,
                    resource = tile.LatestResource, tile.ValidResourceSamples, digest = FrameDigest(tile), detail = tile.Detail
                }).ToArray()
            });
            await File.WriteAllTextAsync(Path.Combine(output, "observations.json"), JsonSerializer.Serialize(observations, JsonDefaults.Options));
        }
        async Task Record(string id, Func<Task> action)
        {
            try { await Observe(id + ":before"); await action(); await Observe(id + ":after"); records.Add(new { id, status = "PASS", at = DateTimeOffset.UtcNow }); }
            catch (Exception ex)
            {
                records.Add(new { id, status = "FAIL", at = DateTimeOffset.UtcNow, error = ex.ToString(), page = (tabs.SelectedItem as System.Windows.Controls.TabItem)?.Tag, window = WindowState.ToString(), monitorPaused, active = MonitorActive, viewport = new { monitorScroll.ActualWidth, monitorScroll.ActualHeight, monitorScroll.ViewportHeight }, tiles = monitorTiles.Values.Select(tile => new { tile.Key, visible = tile.VisibleIn(monitorScroll), tile.Received, tile.Presented, tile.LastFrameAt, detail = tile.Detail }).ToArray() });
                SaveRender(Path.Combine(output, "failure.png"));
                throw;
            }
            finally { await File.WriteAllTextAsync(Path.Combine(output, "monitor-results.json"), JsonSerializer.Serialize(records, JsonDefaults.Options)); }
        }
        var before = ViewModel.CurrentDevice;
        var main = ViewModel.MainDevice;
        var selected = ViewModel.Devices.Where(row => row.IsSelected).Select(row => row.Serial).ToArray();
        var nodes = ViewModel.Nodes.ToArray();
        var seen = bindings.Keys.ToDictionary(index => index, _ => new HashSet<string>());
        var nextMotionRound = 0;
        async Task RequireStoppedAsync()
        {
            await Task.WhenAll(monitorTiles.Values.Select(tile => tile.StopCompletion));
            if (monitorTiles.Values.Any(tile => tile.Session is not null || tile.Failed)) throw new InvalidOperationException("视频会话或清理仍异常：" + string.Join("; ", monitorTiles.Values.Where(tile => tile.Session is not null || tile.Failed).Select(tile => tile.Detail)));
        }
        async Task ReconcileNowAsync()
        {
            while (!monitorReconcile.IsCompleted) await monitorReconcile;
            monitorReconcile = ReconcileMonitorAsync();
            await monitorReconcile;
        }
        try
        {
            await Record("targets-identity", async () =>
            {
                foreach (var pair in bindings)
                {
                    var instance = runtime.LdPlayer?.Instances.Single(item => item.Index == pair.Key) ?? throw new InvalidOperationException("无雷电目录");
                    if (!instance.Name.StartsWith("MatrixAcceptance", StringComparison.Ordinal) || !instance.AndroidStarted || !LdInstanceConnected(pair.Key)) throw new InvalidOperationException("仅已运行且身份核验通过的专用测试实例可验收：" + pair.Key);
                    using var deadline = CancellationTokenSource.CreateLinkedTokenSource(suiteDeadline.Token); deadline.CancelAfter(TimeSpan.FromSeconds(15));
                    var response = await runtime.Backend.ShellAsync(pair.Value, "am start -a android.settings.SETTINGS", ct: deadline.Token);
                    if (response.Contains("Error", StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("打开系统设置失败：" + response);
                }
            });
            ViewModel.Search = "MatrixAcceptance";
            NavigateTo("实时监控");
            await Dispatcher.InvokeAsync(UpdateLayout, DispatcherPriority.Render);
            await Record("two-cards-no-selection", async () =>
            {
                var deadline = DateTimeOffset.UtcNow.AddSeconds(45);
                while (DateTimeOffset.UtcNow < deadline && bindings.Keys.Any(index => !monitorTiles.TryGetValue("ld:" + index, out var tile) || tile.Presented < 2)) await Task.Delay(250);
                if (bindings.Keys.Any(index => !monitorTiles.TryGetValue("ld:" + index, out var tile) || tile.Presented < 2 || tile.Failed))
                    throw new InvalidOperationException("两卡未同时获得连续帧：" + string.Join("; ", monitorTiles.Values.Select(tile => tile.Detail)));
                if (monitorSelected is not null) throw new InvalidOperationException("未选择监看卡片时不应自动选中。");
                SaveRender(Path.Combine(output, "monitor-first-frames.png"));
            });
            var timeline = new List<object>();
            await Record("continuous-190-seconds", async () =>
            {
                var watch = Stopwatch.StartNew();
                var round = 0;
                var subjects = bindings.Keys.ToDictionary(index => index, index => (monitorTiles["ld:" + index].Session, monitorTiles["ld:" + index].Signature));
                var previous = bindings.Keys.ToDictionary(index => index, index => monitorTiles["ld:" + index].Received);
                while (watch.Elapsed < TimeSpan.FromSeconds(190))
                {
                    foreach (var pair in bindings)
                    {
                        var row = ViewModel.Devices.Single(item => item.Serial == pair.Value);
                        var up = (round + pair.Key) % 2 == 0;
                        await runtime.Backend.SwipeAsync(pair.Value, row.Info.Width / 2, row.Info.Height * (up ? 3 : 1) / 4,
                            row.Info.Width / 2, row.Info.Height * (up ? 1 : 3) / 4, 350, suiteDeadline.Token);
                    }
                    await Task.Delay(1600);
                    foreach (var index in bindings.Keys)
                    {
                        var tile = monitorTiles["ld:" + index];
                        if (tile.Failed || !tile.Eligible || !tile.VisibleIn(monitorScroll) || tile.Session?.IsRunning != true || !RecentMonitorFrame(tile.LastFrameAt, DateTimeOffset.UtcNow)) throw new InvalidOperationException("视频会话中断、不可见或超过12秒无新帧：" + tile.Detail);
                        if (tile.Received < previous[index] || tile.Signature != subjects[index].Signature || !ReferenceEquals(tile.Session, subjects[index].Session)) throw new InvalidOperationException("连续阶段会话/身份变化或帧序号回退：" + tile.Detail);
                        previous[index] = tile.Received;
                        if (tile.Frame.Source is System.Windows.Media.Imaging.BitmapSource bitmap)
                        {
                            var bytes = new byte[bitmap.PixelWidth * bitmap.PixelHeight * 4]; bitmap.CopyPixels(bytes, bitmap.PixelWidth * 4, 0);
                            seen[index].Add(Convert.ToHexString(SHA256.HashData(bytes)));
                        }
                        timeline.Add(new { seconds = watch.Elapsed.TotalSeconds, index, tile.Received, tile.Presented, tile.LastFrameAt, resource = tile.Resource.Text, state = tile.State.Text });
                    }
                    round++;
                    nextMotionRound = round;
                    await Observe("continuous-190-seconds:sample");
                    await File.WriteAllTextAsync(Path.Combine(output, "timeline.json"), JsonSerializer.Serialize(timeline, JsonDefaults.Options));
                }
                if (seen.Any(pair => pair.Value.Count < 3)) throw new InvalidOperationException("画面变化不足，不能确认两路差异化视频。");
                SaveRender(Path.Combine(output, "monitor-continuous.png"));
                await File.WriteAllTextAsync(Path.Combine(output, "timeline.json"), JsonSerializer.Serialize(timeline, JsonDefaults.Options));
            });
            await Record("resources-valid", () =>
            {
                if (monitorHostValidSamples < 2 || bindings.Keys.Any(index => monitorTiles["ld:" + index].ValidResourceSamples < 2)) throw new InvalidOperationException("真实CPU/工作集采样不足两次，不能算资源监控通过。");
                var now = DateTimeOffset.UtcNow;
                if (monitorLatestHost is not { CpuPercent: not null, Error: null } host || now - host.ObservedAt > TimeSpan.FromSeconds(12)
                    || bindings.Keys.Any(index => monitorTiles["ld:" + index].LatestResource is not { CpuPercent: not null, WorkingSetBytes: > 0, Error: null } sample || now - sample.ObservedAt > TimeSpan.FromSeconds(12)))
                    throw new InvalidOperationException("最近资源采样未知或陈旧，不能用历史有效次数代替当前状态。");
                return Task.CompletedTask;
            });
            await Record("selection-isolation", () =>
            {
                SelectMonitor("ld:2");
                if (!ReferenceEquals(before, ViewModel.CurrentDevice) || !ReferenceEquals(main, ViewModel.MainDevice) || !nodes.SequenceEqual(ViewModel.Nodes)
                    || !selected.SequenceEqual(ViewModel.Devices.Where(row => row.IsSelected).Select(row => row.Serial))) throw new InvalidOperationException("监看选择污染了控制上下文。");
                return Task.CompletedTask;
            });
            await Record("all-directory-two-authorized-videos-45s", async () =>
            {
                ViewModel.Search = ""; monitorScopeChoice.SelectedIndex = 1;
                await ReconcileNowAsync(); await Dispatcher.InvokeAsync(UpdateLayout, DispatcherPriority.Render);
                var watch = Stopwatch.StartNew();
                var round = nextMotionRound;
                var subjects = bindings.Keys.ToDictionary(index => index, index => (monitorTiles["ld:" + index].Session, monitorTiles["ld:" + index].Signature));
                var previous = bindings.Keys.ToDictionary(index => index, index => monitorTiles["ld:" + index].Received);
                var distinct = bindings.Keys.ToDictionary(index => index, _ => new HashSet<string>());
                while (watch.Elapsed < TimeSpan.FromSeconds(45))
                {
                    foreach (var pair in bindings)
                    {
                        var tile = monitorTiles["ld:" + pair.Key]; var row = tile.Device!;
                        var up = (round + pair.Key) % 2 == 0;
                        await runtime.Backend.SwipeAsync(pair.Value, row.Info.Width / 2, row.Info.Height * (up ? 3 : 1) / 4, row.Info.Width / 2, row.Info.Height * (up ? 1 : 3) / 4, 350, suiteDeadline.Token);
                    }
                    if (monitorTiles.Values.Any(tile => tile.Session is not null && tile.Instance?.Index is not (1 or 2))) throw new InvalidOperationException("出现未批准的视频通道。");
                    await Task.Delay(1500);
                    foreach (var index in bindings.Keys)
                    {
                        var tile = monitorTiles["ld:" + index];
                        if (!tile.VisibleIn(monitorScroll) || !tile.Eligible || tile.Failed || tile.Session?.IsRunning != true || !RecentMonitorFrame(tile.LastFrameAt, DateTimeOffset.UtcNow)) throw new InvalidOperationException("全部目录视图中授权卡片停止更新：" + tile.Detail);
                        if (tile.Received < previous[index] || tile.Signature != subjects[index].Signature || !ReferenceEquals(tile.Session, subjects[index].Session)) throw new InvalidOperationException("全部目录阶段会话/身份变化或帧序号回退：" + tile.Detail);
                        previous[index] = tile.Received;
                        if (tile.Frame.Source is System.Windows.Media.Imaging.BitmapSource bitmap)
                        {
                            var bytes = new byte[bitmap.PixelWidth * bitmap.PixelHeight * 4]; bitmap.CopyPixels(bytes, bitmap.PixelWidth * 4, 0);
                            distinct[index].Add(Convert.ToHexString(SHA256.HashData(bytes)));
                        }
                    }
                    round++;
                    await Observe("all-directory-two-authorized-videos-45s:sample");
                }
                if (distinct.Any(pair => pair.Value.Count < 3)) throw new InvalidOperationException("全部目录阶段真实画面变化不足。");
                await File.WriteAllTextAsync(Path.Combine(output, "all-directory-distinct-frames.json"), JsonSerializer.Serialize(distinct.ToDictionary(pair => pair.Key, pair => pair.Value.Count), JsonDefaults.Options));
                SaveRender(Path.Combine(output, "monitor-all-directory.png"));
                monitorScopeChoice.SelectedIndex = 0;
                await ReconcileNowAsync();
            });
            await Record("hidden-native-window-video-and-open", async () =>
            {
                var tile = monitorTiles["ld:1"];
                var instance = tile.Instance!;
                var target = tile.Target!;
                if (ldWindowVisibility is null) throw new InvalidOperationException("未初始化窗口身份恢复组件。");
                try
                {
                    await ldWindowVisibility.HideAsync(instance, target.PlayerBirth);
                    if (MonitorWindowVisible(new IntPtr(instance.TopWindow))) throw new InvalidOperationException("测试窗口未隐藏。");
                    var hiddenAt = DateTimeOffset.UtcNow;
                    await Task.Delay(700, suiteDeadline.Token);
                    if (MonitorWindowVisible(new IntPtr(instance.TopWindow))) throw new InvalidOperationException("隐藏保持阶段窗口重新可见。");
                    var beforeChange = FrameDigest(tile) ?? throw new InvalidOperationException("隐藏刺激前没有有效画面。");
                    var expectedSession = tile.Session ?? throw new InvalidOperationException("隐藏刺激前没有视频会话。");
                    var expectedSignature = tile.Signature;
                    var count = tile.Presented;
                    var stimulusAt = DateTimeOffset.UtcNow;
                    var row = tile.Device!;
                    await runtime.Backend.SwipeAsync(row.Serial, row.Info.Width / 2, row.Info.Height * 3 / 4, row.Info.Width / 2, row.Info.Height / 4, 350, suiteDeadline.Token);
                    var deadline = DateTimeOffset.UtcNow.AddSeconds(15);
                    bool HasNewHiddenFrame() => tile.Presented >= count + 2 && tile.LastFrameAt is { } at && at >= stimulusAt
                        && tile.Eligible && !tile.Failed && tile.Session?.IsRunning == true && ReferenceEquals(tile.Session, expectedSession)
                        && tile.Signature == expectedSignature && FrameDigest(tile) is { } digest && digest != beforeChange;
                    while (!HasNewHiddenFrame() && DateTimeOffset.UtcNow < deadline)
                    {
                        if (MonitorWindowVisible(new IntPtr(instance.TopWindow))) throw new InvalidOperationException("隐藏等待阶段窗口重新可见。");
                        await Task.Delay(100);
                    }
                    if (!HasNewHiddenFrame()) throw new InvalidOperationException("隐藏雷电窗口后未呈现同一会话的新帧及滚动后的画面变化。");
                    if (MonitorWindowVisible(new IntPtr(instance.TopWindow))) throw new InvalidOperationException("隐藏画面采样时窗口已重新可见。");
                    await File.WriteAllTextAsync(Path.Combine(output, "hidden-frame-proof.json"), JsonSerializer.Serialize(new { hiddenAt, stimulusAt, observedAt = DateTimeOffset.UtcNow, beforeDigest = beforeChange, afterDigest = FrameDigest(tile), nativeWindowVisible = false, tile.LastFrameAt, presentedBefore = count, tile.Presented }, JsonDefaults.Options));
                    await Observe("hidden-native-window:frames-while-hidden");
                    SaveRender(Path.Combine(output, "monitor-hidden-native.png"));
                    await OpenMonitorWindowAsync("ld:1");
                    deadline = DateTimeOffset.UtcNow.AddSeconds(5);
                    while ((!MonitorWindowVisible(new IntPtr(instance.TopWindow)) || MonitorWindowIconic(new IntPtr(instance.TopWindow))) && DateTimeOffset.UtcNow < deadline) await Task.Delay(100);
                    if (monitorNotice.StartsWith("打开窗口失败", StringComparison.Ordinal) || !MonitorWindowVisible(new IntPtr(instance.TopWindow)) || MonitorWindowIconic(new IntPtr(instance.TopWindow))) throw new InvalidOperationException("真实窗口未恢复：" + monitorNotice);
                }
                finally { await ldWindowVisibility.ShowAsync(1); }
            });
            await Record("resource-list-pauses-video", async () =>
            {
                SetMonitorMode(true); await ReconcileNowAsync(); await RequireStoppedAsync();
                var countsBefore = bindings.Keys.ToDictionary(index => index, index => monitorTiles["ld:" + index].ValidResourceSamples);
                await Task.Delay(4500, suiteDeadline.Token);
                if (countsBefore.Any(pair => monitorTiles["ld:" + pair.Key].ValidResourceSamples <= pair.Value)) throw new InvalidOperationException("资源列表中没有新的有效资源采样。");
                await Dispatcher.InvokeAsync(UpdateLayout, DispatcherPriority.Render);
                SaveRender(Path.Combine(output, "monitor-resources.png"));
                SetMonitorMode(false); await ReconcileNowAsync();
                var counts = bindings.Keys.ToDictionary(index => index, index => monitorTiles["ld:" + index].Presented);
                var deadline = DateTimeOffset.UtcNow.AddSeconds(35);
                while (DateTimeOffset.UtcNow < deadline && counts.Any(pair => monitorTiles["ld:" + pair.Key].Presented <= pair.Value)) await Task.Delay(250);
                if (counts.Any(pair => monitorTiles["ld:" + pair.Key].Presented <= pair.Value)) throw new InvalidOperationException("从资源列表回画面墙后视频未恢复。");
            });
            await Record("pause-and-release", async () =>
            {
                monitorPauseChoice.IsChecked = true;
                await ReconcileNowAsync();
                await RequireStoppedAsync();
                var pausedCounts = bindings.Keys.ToDictionary(index => index, index => monitorTiles["ld:" + index].Presented);
                await Task.Delay(700);
                if (pausedCounts.Any(pair => monitorTiles["ld:" + pair.Key].Presented != pair.Value)) throw new InvalidOperationException("暂停后仍在呈现新帧。");
                SaveRender(Path.Combine(output, "monitor-paused.png"));
                monitorPauseChoice.IsChecked = false;
                var deadline = DateTimeOffset.UtcNow.AddSeconds(35);
                var counts = bindings.Keys.ToDictionary(index => index, index => monitorTiles["ld:" + index].Presented);
                while (DateTimeOffset.UtcNow < deadline && counts.Any(pair => monitorTiles["ld:" + pair.Key].Presented <= pair.Value)) await Task.Delay(250);
                if (counts.Any(pair => monitorTiles["ld:" + pair.Key].Presented <= pair.Value)) throw new InvalidOperationException("恢复后视频未继续到帧。");
            });
            await Record("leave-page-cleanup", async () =>
            {
                NavigateTo("设备墙 / 主控");
                await ReconcileNowAsync();
                await RequireStoppedAsync();
            });
            await Record("node-page-auto-load-after-fast-switch", async () =>
            {
                var target = ViewModel.Devices.Single(row => row.Serial == bindings[2]);
                target.Screenshot = null; target.CapturedAt = null;
                var requestedAt = DateTimeOffset.UtcNow;
                ViewModel.CurrentDevice = ViewModel.Devices.Single(row => row.Serial == bindings[1]);
                NavigateTo("节点");
                ViewModel.CurrentDevice = target;
                var deadline = DateTimeOffset.UtcNow.AddSeconds(35);
                while (DateTimeOffset.UtcNow < deadline && (ViewModel.Busy || ViewModel.Nodes.Count == 0 || ViewModel.DebugImage is null)) await Task.Delay(100);
                if (ViewModel.Busy || ViewModel.Nodes.Count == 0 || ViewModel.DebugImage is null || !ReferenceEquals(ViewModel.CurrentDevice, target)
                    || !ReferenceEquals(ViewModel.DebugImage, target.Screenshot) || target.CapturedAt is null || target.CapturedAt < requestedAt)
                    throw new InvalidOperationException("快速切到实例2后未自动取得属于本轮实例2的定位截图和节点：" + ViewModel.Error);
                await File.WriteAllTextAsync(Path.Combine(output, "node-capture-proof.json"), JsonSerializer.Serialize(new { serial = target.Serial, requestedAt, target.CapturedAt, nodes = ViewModel.FlattenNodes().Count(), screenshotBelongsToTarget = ReferenceEquals(ViewModel.DebugImage, target.Screenshot), overlapRaceClaim = false }, JsonDefaults.Options));
                await Dispatcher.InvokeAsync(UpdateLayout, DispatcherPriority.Render);
                SaveRender(Path.Combine(output, "nodes-auto-load.png"));
                NavigateTo("设备墙 / 主控");
            });
            passed = true;
        }
        finally
        {
            try { await StopMonitorAsync(); await RequireStoppedAsync(); }
            catch (Exception ex) { passed = false; records.Add(new { id = "final-cleanup", status = "FAIL", error = ex.ToString() }); throw; }
            finally
            {
                var hashes = new[] { typeof(MainWindow).Assembly, typeof(AdbDeviceBackend).Assembly, typeof(MatrixDevice.Host.ScreenVideoSession).Assembly }
                    .ToDictionary(assembly => assembly.GetName().Name!, assembly => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(assembly.Location))));
                await File.WriteAllTextAsync(Path.Combine(output, "monitor-results.json"), JsonSerializer.Serialize(records, JsonDefaults.Options));
                await File.WriteAllTextAsync(Path.Combine(output, "summary.json"), JsonSerializer.Serialize(new { runId, started, ended = DateTimeOffset.UtcNow, running = false, passed, bindings, distinctFrames = seen.ToDictionary(pair => pair.Key, pair => pair.Value.Count), tests = records.Count, declaredScope = new { noExternalAdbClient = true, noInstanceLaunch = true, noInstance0Operations = true }, capacityClaim = "2路实测，非23路验收", assemblySha256 = hashes[typeof(MainWindow).Assembly.GetName().Name!], loadedAssemblyHashes = hashes }, JsonDefaults.Options));
            }
        }
    }

    private static string? FrameDigest(MonitorTile tile)
    {
        if (tile.Frame.Source is not System.Windows.Media.Imaging.BitmapSource bitmap) return null;
        var bytes = new byte[bitmap.PixelWidth * bitmap.PixelHeight * 4]; bitmap.CopyPixels(bytes, bitmap.PixelWidth * 4, 0);
        return Convert.ToHexString(SHA256.HashData(bytes));
    }

    private static bool RecentMonitorFrame(DateTimeOffset? lastFrameAt, DateTimeOffset now) =>
        lastFrameAt is { } received && received <= now && now - received <= TimeSpan.FromSeconds(12);

    [DllImport("user32.dll", EntryPoint = "IsWindowVisible")] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool MonitorWindowVisible(IntPtr window);
    [DllImport("user32.dll", EntryPoint = "IsIconic")] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool MonitorWindowIconic(IntPtr window);
}
