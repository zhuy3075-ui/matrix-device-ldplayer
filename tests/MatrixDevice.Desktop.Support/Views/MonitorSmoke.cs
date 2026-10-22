using System.IO;
using System.Windows;
using System.Windows.Threading;
using MatrixDevice.Host;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    private async Task VerifyMonitorProjectionAsync(string? screenshot)
    {
        if (!ViewModel.Demo) throw new InvalidOperationException("离线监控夹具仅限演示模式。");
        var sampledAt = new DateTimeOffset(2026, 10, 15, 0, 0, 0, TimeSpan.Zero);
        if (RecentMonitorFrame(null, sampledAt) || RecentMonitorFrame(sampledAt.AddSeconds(-12).AddTicks(-1), sampledAt)
            || RecentMonitorFrame(sampledAt.AddTicks(1), sampledAt) || !RecentMonitorFrame(sampledAt.AddSeconds(-2.4), sampledAt)
            || !RecentMonitorFrame(sampledAt.AddSeconds(-12), sampledAt)) throw new InvalidOperationException("新帧时间窗口未正确区分短时静止、缺失、陈旧与未来时间。");
        await StopMonitorAsync(); monitorStopping = true;
        var current = ViewModel.CurrentDevice;
        var main = ViewModel.MainDevice;
        var targets = ViewModel.Targets().ToArray();
        var nodes = ViewModel.Nodes.ToArray();
        try
        {
            NavigateTo("实时监控");
            monitorTiles.Clear(); monitorWall.Children.Clear();
            for (var index = 1; index <= 2; index++)
            {
                var key = "fixture:" + index;
                var tile = new MonitorTile(key, () => SelectMonitor(key), () => Task.CompletedTask, () => Task.CompletedTask) { Eligible = true };
                tile.Name.Text = $"离线测试画面 {index}"; tile.Subtitle.Text = "合成像素夹具 · 非真实设备";
                monitorTiles.Add(key, tile); monitorWall.Children.Add(tile.Surface);
                var oldEpoch = tile.BeginVideo();
                tile.ClearUntrustedFrame();
                tile.AcceptFrame(oldEpoch, new(32, 18, new byte[32 * 18 * 4], 1, DateTimeOffset.UtcNow));
                tile.Present(DateTimeOffset.UtcNow, true);
                if (tile.LastFrameAt is not null || tile.Frame.Source is not null) throw new InvalidOperationException("已失效会话仍可发布帧。");
                var stop = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                tile.StopCompletion = stop.Task;
                if (!ReferenceEquals(tile.StopAsync(), stop.Task) || tile.StopAsync().IsCompleted) throw new InvalidOperationException("重复停止未共享尚未完成的清理任务。");
                stop.SetResult(); await tile.StopAsync();
                var pixels = new byte[32 * 18 * 4];
                for (var pixel = 0; pixel < pixels.Length; pixel += 4) { pixels[pixel] = (byte)(index == 1 ? 210 : 40); pixels[pixel + 1] = (byte)(pixel / 4 % 32 * 7); pixels[pixel + 2] = (byte)(index == 1 ? 30 : 220); pixels[pixel + 3] = 255; }
                var epoch = tile.BeginVideo();
                tile.AcceptFrame(epoch, new(32, 18, pixels, 2, DateTimeOffset.UtcNow)); tile.Present(DateTimeOffset.UtcNow, true);
                if (tile.Frame.Source is null || tile.Presented != 1) throw new InvalidOperationException("未选中卡片没有呈现视频帧。");
                if (!tile.State.Text.EndsWith(tile.LastFrameAt!.Value.ToLocalTime().ToString("HH:mm:ss"), StringComparison.Ordinal))
                    throw new InvalidOperationException("卡片到帧时间未按本地时区显示。");
                tile.Present(tile.LastFrameAt.Value.AddSeconds(6), true);
                if (!tile.State.Text.StartsWith("等待新帧", StringComparison.Ordinal) || !tile.State.Text.EndsWith(tile.LastFrameAt.Value.ToLocalTime().ToString("HH:mm:ss"), StringComparison.Ordinal))
                    throw new InvalidOperationException("陈旧帧时间未按本地时区显示。");
                tile.SetResource(new(index, 1, DateTimeOffset.UtcNow.AddSeconds(-2), null, 128 * 1048576, null));
                tile.SetResource(new(index, 1, DateTimeOffset.UtcNow, 7 * index, 160 * 1048576, null));
                if (tile.LatestResource is not { Error: null, WorkingSetBytes: 167772160 } latest || latest.CpuPercent != 7 * index)
                    throw new InvalidOperationException("最近资源观测未保留真实样本。");
                if (tile.Resource.ToolTip is not string resourceTip || !resourceTip.StartsWith(latest.ObservedAt.ToLocalTime().ToString("HH:mm:ss"), StringComparison.Ordinal))
                    throw new InvalidOperationException("资源提示时间未按本地时区显示。");
                tile.State.Text = "离线合成帧（非实时视频验收）";
            }
            SelectMonitor("fixture:2");
            if (!ReferenceEquals(current, ViewModel.CurrentDevice) || !ReferenceEquals(main, ViewModel.MainDevice) || !targets.SequenceEqual(ViewModel.Targets()) || !nodes.SequenceEqual(ViewModel.Nodes))
                throw new InvalidOperationException("监看选择改变控制上下文。");
            await Dispatcher.InvokeAsync(UpdateLayout, DispatcherPriority.Render);
            if (monitorTiles.Values.Any(tile => tile.Frame.ActualHeight < 1 || tile.Surface.ActualWidth > monitorScroll.ViewportWidth)) throw new InvalidOperationException("监控卡片画面为空或超出视口。");
            if (screenshot is not null) SaveRender(Path.Combine(Path.GetDirectoryName(screenshot)!, "monitor-offline.png"));
        }
        finally
        {
            monitorTiles.Clear(); monitorWall.Children.Clear(); monitorStopping = false;
            NavigateTo("设备墙 / 主控");
        }
    }
}
