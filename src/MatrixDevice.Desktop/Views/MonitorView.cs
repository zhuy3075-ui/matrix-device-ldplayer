using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Path = System.IO.Path;
using System.Windows.Threading;
using MatrixDevice.Core;
using MatrixDevice.Host;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    private readonly Dictionary<string, MonitorTile> monitorTiles = new(StringComparer.Ordinal);
    private ResourceSampler monitorResources = new();
    private readonly DispatcherTimer monitorTimer = new() { Interval = TimeSpan.FromMilliseconds(125) };
    private WrapPanel monitorWall = null!;
    private ScrollViewer monitorScroll = null!;
    private TextBlock monitorSummary = null!;
    private TextBlock monitorHost = null!;
    private TextBlock monitorDetail = null!;
    private MatrixRuntime? monitorRuntime;
    private Task monitorReconcile = Task.CompletedTask;
    private Task monitorSampling = Task.CompletedTask;
    private DateTimeOffset monitorLastReconcile;
    private DateTimeOffset monitorLastSample;
    private string? monitorSelected;
    private bool monitorPaused;
    private bool monitorStopping;
    private bool monitorWasActive;
    private long monitorEpoch;
    private int monitorHostValidSamples;
    private HostResourceSample? monitorLatestHost;
    private bool monitorCompact;
    private bool monitorConnectedOnly = true;
    private string monitorNotice = "";
    private ToggleButton monitorPauseChoice = null!;
    private ToggleButton monitorWallChoice = null!;
    private ToggleButton monitorListChoice = null!;
    private ComboBox monitorScopeChoice = null!;

    private bool MonitorActive => !closing && !monitorStopping && !monitorPaused && Equals((tabs.SelectedItem as TabItem)?.Tag, "实时监控");

    private FrameworkElement BuildMonitorPage()
    {
        var page = Page();
        var header = new DockPanel { Margin = new Thickness(0, 0, 0, 10) };
        var pause = WallToggle("\uE769", "暂停监控"); pause.Content = ShellGlyph("\uE769"); pause.Width = 34; pause.MinWidth = 0; pause.Padding = new Thickness(0);
        monitorPauseChoice = pause;
        pause.Checked += (_, _) => { monitorPaused = true; monitorEpoch++; pause.ToolTip = "恢复监控"; pause.Content = ShellGlyph("\uE768"); monitorLastReconcile = default; MonitorTick(); };
        pause.Unchecked += (_, _) => { monitorPaused = false; monitorEpoch++; pause.ToolTip = "暂停监控"; pause.Content = ShellGlyph("\uE769"); monitorLastReconcile = default; monitorTimer.Start(); MonitorTick(); };
        DockPanel.SetDock(pause, Dock.Right); header.Children.Add(pause);
        var modes = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Top };
        var wall = WallToggle("\uE8A9", "画面墙"); var list = WallToggle("\uE9D5", "资源列表"); wall.IsChecked = true;
        monitorWallChoice = wall; monitorListChoice = list;
        wall.Click += (_, _) => { wall.IsChecked = true; list.IsChecked = false; SetMonitorMode(false); };
        list.Click += (_, _) => { wall.IsChecked = false; list.IsChecked = true; SetMonitorMode(true); };
        modes.Children.Add(wall); modes.Children.Add(list);
        var scope = new ComboBox { Width = 118, Height = 34, Margin = new Thickness(0, 0, 8, 0), ItemsSource = new[] { "已连接设备", "全部目录" }, SelectedIndex = 0 };
        monitorScopeChoice = scope;
        scope.SelectionChanged += (_, _) => { monitorConnectedOnly = scope.SelectedIndex == 0; monitorEpoch++; monitorLastReconcile = default; MonitorTick(); };
        modes.Children.Add(scope); DockPanel.SetDock(modes, Dock.Right); header.Children.Add(modes);
        var heading = new StackPanel();
        heading.Children.Add(new TextBlock { Text = "实时监控", FontSize = 20, FontWeight = FontWeights.SemiBold });
        monitorHost = new TextBlock { Text = "主机资源：待采样", Foreground = WallMuted, FontSize = 12, Margin = new Thickness(0, 6, 0, 0), TextWrapping = TextWrapping.Wrap };
        heading.Children.Add(monitorHost); header.Children.Add(heading);
        DockPanel.SetDock(header, Dock.Top); page.Children.Add(header);
        monitorSummary = new TextBlock { Foreground = WallMuted, Margin = new Thickness(0, 0, 0, 8), TextWrapping = TextWrapping.Wrap, ToolTip = "视频到帧率不等于应用帧率；静止画面可能不产生新帧。" };
        DockPanel.SetDock(monitorSummary, Dock.Top); page.Children.Add(monitorSummary);
        monitorDetail = new TextBlock { Foreground = WallMuted, TextWrapping = TextWrapping.Wrap, MinHeight = 24, Margin = new Thickness(0, 8, 0, 0), Text = "未选中监看设备" };
        DockPanel.SetDock(monitorDetail, Dock.Bottom); page.Children.Add(monitorDetail);
        monitorWall = new WrapPanel { Orientation = Orientation.Horizontal };
        monitorScroll = new ScrollViewer { Content = monitorWall, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled, CanContentScroll = false };
        monitorScroll.ScrollChanged += (_, _) => { monitorEpoch++; monitorLastReconcile = default; };
        page.Children.Add(monitorScroll);
        monitorTimer.Tick += (_, _) => MonitorTick();
        ViewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName is nameof(WorkspaceViewModel.StatusFilter) or nameof(WorkspaceViewModel.Counts)) { monitorEpoch++; monitorLastReconcile = default; }
        };
        return page;
    }

    private void MonitorNavigationChanged()
    {
        if (monitorWall is null) return;
        monitorEpoch++;
        var active = Equals((tabs.SelectedItem as TabItem)?.Tag, "实时监控");
        if (active || monitorWasActive) { monitorLastReconcile = default; monitorTimer.Start(); MonitorTick(); }
        monitorWasActive = active;
    }

    private void MonitorTick()
    {
        if (monitorWall is null || monitorStopping) return;
        var now = DateTimeOffset.UtcNow;
        if (monitorRuntime is not null && !ReferenceEquals(monitorRuntime, ViewModel.Runtime))
            foreach (var tile in monitorTiles.Values) { tile.Eligible = false; tile.ClearUntrustedFrame(); tile.State.Text = "连接上下文已变化"; }
        foreach (var tile in monitorTiles.Values) { tile.ApplyLayout(monitorCompact, monitorScroll.ViewportWidth); tile.Present(now, MonitorActive && !monitorCompact && tile.VisibleIn(monitorScroll)); }
        LogMonitorFailures();
        if (monitorSelected is { } selected && monitorTiles.TryGetValue(selected, out var current))
            monitorDetail.Text = string.IsNullOrEmpty(monitorNotice) ? current.Detail : monitorNotice;
        var playing = monitorTiles.Values.Count(tile => tile.Session is { IsRunning: true } && tile.LastFrameAt is not null);
        monitorSummary.Text = monitorPaused ? "监控已暂停" : !MonitorActive ? "监控已停止" : $"可见 {monitorTiles.Values.Count(tile => tile.VisibleIn(monitorScroll))} 台 · 视频 {playing} 路";
        if (monitorReconcile.IsCompleted && now - monitorLastReconcile >= TimeSpan.FromSeconds(1))
        {
            monitorLastReconcile = now;
            monitorReconcile = ReconcileMonitorAsync();
        }
        if (MonitorActive && monitorSampling.IsCompleted && now - monitorLastSample >= TimeSpan.FromSeconds(2))
        {
            monitorLastSample = now;
            monitorSampling = SampleMonitorAsync();
        }
        if (!MonitorActive && monitorReconcile.IsCompleted && monitorTiles.Values.All(tile => tile.Session is null)) monitorTimer.Stop();
    }

    private async Task ReconcileMonitorAsync()
    {
        try
        {
            var runtime = ViewModel.Runtime;
            var epoch = monitorEpoch;
            bool Current() => epoch == monitorEpoch && ReferenceEquals(runtime, ViewModel.Runtime) && MonitorActive && !ldBusy;
            if (!ReferenceEquals(monitorRuntime, runtime))
            {
                await Task.WhenAll(monitorTiles.Values.Select(tile => tile.StopAsync()).ToArray());
                monitorTiles.Clear(); monitorWall.Children.Clear(); monitorSelected = null; monitorDetail.Text = "未选中监看设备";
                monitorRuntime = runtime; monitorResources = new ResourceSampler();
            }
            if (!MonitorActive || ldBusy)
            {
                await Task.WhenAll(monitorTiles.Values.Select(tile => tile.StopAsync()).ToArray());
                monitorResources = new ResourceSampler();
                return;
            }
            var expected = new HashSet<string>();
            foreach (var row in unifiedDeviceView.Cast<UnifiedDeviceRow>().Where(row => !monitorConnectedOnly || row.Connected).ToArray())
            {
                if (!Current()) return;
                expected.Add(row.Key);
                if (!monitorTiles.TryGetValue(row.Key, out var tile))
                {
                    tile = new MonitorTile(row.Key, () => SelectMonitor(row.Key), () => OpenMonitorWindowAsync(row.Key), () => RetryMonitorAsync(row.Key));
                    monitorTiles.Add(row.Key, tile); monitorWall.Children.Add(tile.Surface);
                }
                var instance = runtime.LdPlayer?.Instances.FirstOrDefault(instance => instance.Index == row.Index);
                var signature = $"{row.Serial}/{row.Device?.Info.Generation}/{instance?.Generation}/{instance?.ProcessId}/{instance?.VBoxProcessId}";
                if (tile.Signature != signature)
                {
                    tile.Eligible = false; tile.ClearUntrustedFrame(); tile.Resource.Text = "CPU 待采样 · 工作集 未知"; tile.State.Text = "实例身份已更新";
                    await tile.StopAsync();
                    if (!Current()) return;
                    tile.Reset(signature, instance, row.Device);
                }
                tile.Name.Text = row.Name;
                tile.Subtitle.Text = instance is null ? row.Serial : $"实例 {instance.Index} · {row.Serial}";
                var eligible = row.Connected && row.Device?.Info.Online == true && instance is { Index: > 0 and < 99999, AndroidStarted: true }
                    && runtime.Settings.AdbAllowedSerials.Contains(row.Serial, StringComparer.Ordinal) && LdInstanceConnected(instance.Index);
                tile.Eligible = eligible;
                tile.Open.IsEnabled = eligible;
                if (!eligible || monitorCompact || !tile.VisibleIn(monitorScroll))
                {
                    if (!eligible) { tile.ClearUntrustedFrame(); tile.Resource.Text = "CPU 未知 · 工作集 未知"; if (!tile.Failed) tile.State.Text = "未连接/未授权"; }
                    await tile.StopAsync();
                    if (!Current()) return;
                    if (!eligible) tile.ClearUntrustedFrame();
                    if (!tile.Failed) tile.State.Text = eligible ? monitorCompact ? "资源监控 · 视频已暂停" : "画面已暂停 · 不在可视区域" : instance?.Index == 0 ? "受保护 · 未监控" : !row.Connected ? "未连接" : "未授权视频";
                    continue;
                }
                if (tile.Session is not null || tile.Failed || ViewModel.Demo) continue;
                await tile.StopCompletion;
                if (!Current() || tile.Session is not null || tile.Failed) return;
                if (!tile.VisibleIn(monitorScroll) || !unifiedDeviceView.Cast<UnifiedDeviceRow>().Contains(row) || !LdInstanceConnected(instance!.Index)) continue;
                if (runtime.Backend is not AdbDeviceBackend backend) { tile.Fail("当前后端不支持实时视频"); continue; }
                var ffmpeg = ResolveMonitorFfmpeg(runtime.Settings.MonitorFfmpegPath);
                if (ffmpeg is null) { tile.Fail("未配置 FFmpeg，前往环境设置"); continue; }
                if (runtime.Settings.MonitorServerPath != runtime.FrozenConnectionSettings.MonitorServerPath) { tile.Fail("视频组件配置已修改，重启程序后生效"); continue; }
                if (!File.Exists(runtime.Settings.MonitorServerPath)) { tile.Fail("未配置官方视频组件，前往环境设置"); continue; }
                var width = row.Device!.Info.Width; var height = row.Device.Info.Height;
                var scale = 320d / Math.Max(width, height);
                var frameWidth = Math.Max(16, (int)(width * scale) / 2 * 2);
                var frameHeight = Math.Max(16, (int)(height * scale) / 2 * 2);
                tile.State.Text = "视频连接中";
                var videoEpoch = tile.BeginVideo();
                tile.Session = new ScreenVideoSession(backend, row.Serial, ffmpeg, frameWidth, frameHeight,
                    frame => tile.AcceptFrame(videoEpoch, frame),
                    status => tile.AcceptStatus(videoEpoch, status));
                tile.Session.Start();
                runtime.Log.Write("monitor-video-start", $"index={instance!.Index}; serial={row.Serial}; generation={instance.Generation}; size={frameWidth}x{frameHeight}");
            }
            foreach (var key in monitorTiles.Keys.Where(key => !expected.Contains(key)).ToArray())
            {
                await monitorTiles[key].StopAsync(); LogMonitorFailures(); monitorWall.Children.Remove(monitorTiles[key].Surface); monitorTiles.Remove(key);
                if (monitorSelected == key) { monitorSelected = null; monitorDetail.Text = "未选中监看设备"; }
            }
        }
        catch (Exception ex)
        {
            monitorSummary.Text = "监控更新失败：" + ViewModel.PresentError(ex);
            ViewModel.Runtime.Log.Write("monitor-reconcile-error", ex.ToString());
        }
    }

    private async Task SampleMonitorAsync()
    {
        var runtime = ViewModel.Runtime;
        var epoch = monitorEpoch;
        var sampler = monitorResources;
        var targets = monitorTiles.Values.Where(tile => tile.Eligible && tile.Target is not null).Select(tile => (tile.Key, Target: tile.Target!)).ToArray();
        try
        {
            var result = await Task.Run(() => (Host: sampler.SampleHost(), Rows: targets.Select(item => (item.Key, Sample: sampler.SampleInstance(item.Target))).ToArray()));
            if (closing || epoch != monitorEpoch || !ReferenceEquals(runtime, ViewModel.Runtime) || !MonitorActive) return;
            var host = result.Host;
            monitorLatestHost = host;
            if (host.CpuPercent is not null && host.TotalPhysical is > 0 && host.AvailablePhysical is not null) monitorHostValidSamples++;
            monitorHost.Text = $"主机 CPU {Percent(host.CpuPercent, host.Error is null)} · 内存 {Memory(host.TotalPhysical is { } total && host.AvailablePhysical is { } free ? total - free : null)} / {Memory(host.TotalPhysical)} · {host.ObservedAt.ToLocalTime():HH:mm:ss}";
            monitorHost.ToolTip = host.Error;
            foreach (var item in result.Rows)
            {
                if (monitorTiles.TryGetValue(item.Key, out var tile) && tile.Target?.Generation == item.Sample.Generation && targets.Any(target => target.Key == item.Key && target.Target == tile.Target)) tile.SetResource(item.Sample);
            }
        }
        catch (Exception ex) { monitorHost.Text = "主机资源未知：" + ViewModel.PresentError(ex); ViewModel.Runtime.Log.Write("monitor-resource-error", ex.ToString()); }
    }

    private static string Percent(double? value, bool pending = false) => value is { } number ? $"{number:0.0}%" : pending ? "待采样" : "未知";
    private static string Memory(long? value) => value is { } bytes ? bytes >= 1024L * 1024 * 1024 ? $"{bytes / 1073741824d:0.00} GB" : $"{bytes / 1048576d:0} MB" : "未知";
    private static string? ResolveMonitorFfmpeg(string configured)
    {
        if (!string.IsNullOrWhiteSpace(configured)) return Path.IsPathFullyQualified(configured) && File.Exists(configured) ? configured : null;
        return (Environment.GetEnvironmentVariable("PATH") ?? "").Split(Path.PathSeparator).Where(path => !string.IsNullOrWhiteSpace(path)).Select(path => Path.Combine(path.Trim('"'), "ffmpeg.exe")).FirstOrDefault(File.Exists);
    }

    private void SelectMonitor(string key)
    {
        monitorSelected = key; monitorNotice = "";
        foreach (var tile in monitorTiles.Values) tile.Surface.BorderBrush = WorkspaceBrush(tile.Key == key ? "#3274F6" : "#DFE5EE");
        MonitorTick();
    }

    private void SetMonitorMode(bool compact)
    {
        monitorCompact = compact; monitorEpoch++; monitorLastReconcile = default;
        monitorWallChoice.IsChecked = !compact; monitorListChoice.IsChecked = compact;
        foreach (var tile in monitorTiles.Values) tile.ApplyLayout(compact, monitorScroll.ViewportWidth);
        MonitorTick();
    }

    private async Task RetryMonitorAsync(string key)
    {
        if (!monitorTiles.TryGetValue(key, out var tile)) return;
        await tile.StopAsync(); tile.Failed = false; tile.Frame.Source = null; tile.LastFrameAt = null; tile.State.Text = "等待视频";
        monitorTimer.Start();
        monitorLastReconcile = default; MonitorTick();
    }

    private async Task OpenMonitorWindowAsync(string key)
    {
        if (!monitorTiles.TryGetValue(key, out var tile) || !tile.Eligible || tile.Instance is not { } instance || tile.Device is not { } device) return;
        try
        {
            var runtime = ViewModel.Runtime;
            var target = tile.Target ?? throw new InvalidOperationException("未取得已核验的进程身份。");
            runtime.RequireFrozenConnectionSettings();
            if (runtime.Backend is not AdbDeviceBackend backend) throw new InvalidOperationException("仅真实雷电实例支持打开窗口。");
            await backend.VerifyIdentityAsync(device.Serial);
            if (!ReferenceEquals(runtime, ViewModel.Runtime) || !LdInstanceConnected(instance.Index) || runtime.LdPlayer?.Instances.FirstOrDefault(item => item.Index == instance.Index) != instance) throw new InvalidOperationException("实例身份已变化，请重新接入。");
            if (ldWindowVisibility?.IsHidden(instance.Index) == true) await ldWindowVisibility.ShowAsync(instance.Index);
            if (!ReferenceEquals(runtime, ViewModel.Runtime) || !LdInstanceConnected(instance.Index) || runtime.LdPlayer?.Instances.FirstOrDefault(item => item.Index == instance.Index) != instance) throw new InvalidOperationException("窗口恢复期间实例身份已变化。");
            var foreground = LdRunningWindow.Show(instance, Path.GetDirectoryName(runtime.Settings.LdConsolePath)!, target.PlayerBirth);
            monitorDetail.Text = monitorNotice = foreground ? $"已请求显示 {tile.Name.Text} 的模拟器窗口" : "已请求显示窗口，Windows 未允许切换前台";
        }
        catch (Exception ex) { monitorDetail.Text = monitorNotice = "打开窗口失败：" + ViewModel.PresentError(ex); ViewModel.Runtime.Log.Write("monitor-window-error", ex.ToString()); }
    }

    private async Task StopMonitorAsync()
    {
        monitorStopping = true; monitorEpoch++; monitorTimer.Stop();
        await monitorReconcile;
        await Task.WhenAll(monitorTiles.Values.Select(tile => tile.StopAsync()).ToArray());
        LogMonitorFailures();
        await monitorSampling;
        monitorResources = new ResourceSampler();
        monitorStopping = false;
    }
    internal Task StopMonitorAfterFailureAsync() => StopMonitorAsync();

    private void LogMonitorFailures()
    {
        foreach (var tile in monitorTiles.Values.Where(tile => tile.Failed && !tile.FailureLogged))
        {
            ViewModel.Runtime.Log.Write("monitor-video-error", $"key={tile.Key}; serial={tile.Device?.Serial}; {tile.FailureReason}");
            tile.FailureLogged = true;
        }
    }

    private sealed class MonitorTile
    {
        internal readonly string Key;
        internal readonly Border Surface;
        internal readonly Image Frame = new() { Stretch = Stretch.Uniform };
        internal readonly TextBlock Name = new() { FontWeight = FontWeights.SemiBold, TextTrimming = TextTrimming.CharacterEllipsis };
        internal readonly TextBlock Subtitle = new() { FontSize = 11, Foreground = WallMuted, TextTrimming = TextTrimming.CharacterEllipsis };
        internal readonly TextBlock State = new() { Text = "等待视频", FontSize = 11, TextTrimming = TextTrimming.CharacterEllipsis };
        internal readonly TextBlock Resource = new() { Text = "CPU 待采样 · 工作集 未知", FontSize = 11, Foreground = WallMuted };
        internal readonly Button Open;
        private readonly Border videoSurface;
        private readonly Polyline cpuLine = new() { Stroke = WorkspaceBrush("#3274F6"), StrokeThickness = 1.5 };
        private readonly Polyline memoryLine = new() { Stroke = WorkspaceBrush("#159878"), StrokeThickness = 1.5 };
        private readonly List<MonitorResourceSample> history = [];
        private WriteableBitmap? bitmap;
        internal string Signature = "";
        internal LdPlayerInstance? Instance;
        internal DeviceRow? Device;
        internal MonitorProcessTarget? Target;
        internal ScreenVideoSession? Session;
        private sealed record PendingVideo(long Epoch, ScreenVideoFrame Frame);
        private PendingVideo? pendingFrame;
        private readonly object frameGate = new();
        private string? pendingStatus;
        private long videoEpoch;
        internal Task StopCompletion = Task.CompletedTask;
        internal DateTimeOffset? LastFrameAt;
        internal bool Eligible;
        internal bool Failed;
        internal string? FailureReason;
        internal bool FailureLogged;
        internal long Received;
        internal long Presented;
        internal int ValidResourceSamples;
        internal MonitorResourceSample? LatestResource;
        private long previousCount;
        private DateTimeOffset previousRateAt;
        private double? arrivalRate;
        internal string Detail => $"{Name.Text} · {Subtitle.Text} · {State.Text} · {Resource.Text} · 最近 {history.Count} 个资源样本";

        internal MonitorTile(string key, Action select, Func<Task> open, Func<Task> retry)
        {
            Key = key;
            Surface = new Border { Width = 330, Height = 306, Margin = new Thickness(0, 0, 12, 12), Padding = new Thickness(9), BorderThickness = new Thickness(1), BorderBrush = WorkspaceBrush("#DFE5EE"), CornerRadius = new CornerRadius(6), Background = Brushes.White };
            var stack = new StackPanel(); Surface.Child = stack;
            var title = new DockPanel();
            Open = MonitorIcon("\uE8A7", "打开模拟器窗口", open);
            DockPanel.SetDock(Open, Dock.Right); title.Children.Add(Open);
            var again = MonitorIcon("\uE72C", "重试视频", retry); DockPanel.SetDock(again, Dock.Right); title.Children.Add(again);
            var names = new StackPanel(); names.Children.Add(Name); names.Children.Add(Subtitle); title.Children.Add(names); stack.Children.Add(title);
            videoSurface = new Border { Height = 180, Background = WorkspaceBrush("#161B23"), Margin = new Thickness(0, 7, 0, 6), Child = Frame }; stack.Children.Add(videoSurface);
            stack.Children.Add(State); stack.Children.Add(Resource);
            var chart = new Canvas { Height = 24, Width = 308, ClipToBounds = true, ToolTip = "最近15分钟：蓝色 CPU；绿色进程工作集（独立比例）", Margin = new Thickness(0, 5, 0, 0) };
            chart.Children.Add(cpuLine); chart.Children.Add(memoryLine); stack.Children.Add(chart);
            Surface.MouseLeftButtonDown += async (_, args) =>
            {
                if (args.OriginalSource is DependencyObject source && FindButton(source) is not null) return;
                select();
                if (args.ClickCount == 2) await open();
            };
        }

        internal void ApplyLayout(bool compact, double viewportWidth)
        {
            Surface.Width = compact && double.IsFinite(viewportWidth) ? Math.Max(330, viewportWidth - 14) : 330;
            Surface.Height = compact ? 122 : 306;
            videoSurface.Visibility = compact ? Visibility.Collapsed : Visibility.Visible;
        }

        private static Button? FindButton(DependencyObject source)
        {
            for (var current = source; current is not null; current = VisualTreeHelper.GetParent(current)) if (current is Button button) return button;
            return null;
        }
        private static Button MonitorIcon(string glyph, string tooltip, Func<Task> action)
        {
            var button = new Button { Content = ShellGlyph(glyph, 14), Width = 29, Height = 28, MinWidth = 0, ToolTip = tooltip, Padding = new Thickness(0) };
            button.SetResourceReference(StyleProperty, "IconButton");
            button.Click += async (_, _) => { button.IsEnabled = false; try { await action(); } finally { button.IsEnabled = true; } };
            return button;
        }
        internal bool VisibleIn(ScrollViewer viewport)
        {
            if (!Surface.IsVisible || !Surface.IsLoaded || viewport.ActualHeight <= 0) return false;
            try { return Surface.TransformToAncestor(viewport).TransformBounds(new Rect(Surface.RenderSize)).IntersectsWith(new Rect(0, 0, viewport.ActualWidth, viewport.ViewportHeight)); }
            catch (InvalidOperationException) { return false; }
        }
        internal void Reset(string signature, LdPlayerInstance? instance, DeviceRow? device)
        {
            Signature = signature; Instance = instance; Device = device; Failed = false; FailureReason = null; FailureLogged = false; history.Clear(); cpuLine.Points.Clear(); memoryLine.Points.Clear(); ClearUntrustedFrame(); Received = Presented = previousCount = 0; ValidResourceSamples = 0; LatestResource = null; previousRateAt = default; arrivalRate = null;
            Resource.Text = "CPU 待采样 · 工作集 未知";
            Target = null;
            if (instance is { ProcessId: > 0 })
            {
                try { Target = new(instance.Index, instance.Generation, instance.ProcessId, Birth(instance.ProcessId), instance.VBoxProcessId, Birth(instance.VBoxProcessId)); }
                catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or System.ComponentModel.Win32Exception) { Resource.ToolTip = ex.Message; }
            }
        }
        private static long Birth(int pid) { if (pid <= 0) return 0; using var process = Process.GetProcessById(pid); return process.StartTime.ToUniversalTime().Ticks; }
        internal long BeginVideo()
        {
            lock (frameGate) { videoEpoch++; pendingFrame = null; pendingStatus = null; previousRateAt = default; previousCount = Received = 0; arrivalRate = null; return videoEpoch; }
        }
        internal void AcceptFrame(long epoch, ScreenVideoFrame frame)
        {
            lock (frameGate) { if (videoEpoch == epoch) pendingFrame = new(epoch, frame); }
        }
        internal void AcceptStatus(long epoch, string status)
        {
            lock (frameGate) { if (videoEpoch == epoch) pendingStatus = status; }
        }
        internal void ClearUntrustedFrame()
        {
            lock (frameGate) { videoEpoch++; pendingFrame = null; pendingStatus = null; }
            Frame.Source = null; bitmap = null; LastFrameAt = null;
        }
        internal void Present(DateTimeOffset now, bool active)
        {
            PendingVideo? pending;
            string? status;
            lock (frameGate) { pending = pendingFrame; pendingFrame = null; status = pendingStatus; }
            if (pending is { } packet && packet.Epoch == videoEpoch && active && Eligible)
            {
                var frame = packet.Frame;
                if (bitmap is null || bitmap.PixelWidth != frame.Width || bitmap.PixelHeight != frame.Height)
                    bitmap = new WriteableBitmap(frame.Width, frame.Height, 96, 96, PixelFormats.Bgra32, null);
                bitmap.WritePixels(new Int32Rect(0, 0, frame.Width, frame.Height), frame.Bgra, frame.Width * 4, 0);
                Frame.Source = bitmap; LastFrameAt = frame.ReceivedAt; Received = frame.Sequence; Presented++;
            }
            if (now - previousRateAt >= TimeSpan.FromSeconds(2))
            {
                if (previousRateAt != default) arrivalRate = (Received - previousCount) / (now - previousRateAt).TotalSeconds;
                previousRateAt = now; previousCount = Received;
            }
            if (Session is { Completion.IsCompleted: true, LastError: { } error } && !Failed) Fail(error);
            if (Failed) return;
            if (!Eligible) return;
            if (!active) { if (LastFrameAt is not null) State.Text = "画面已暂停 · 历史帧"; return; }
            if (LastFrameAt is { } received)
                State.Text = now - received > TimeSpan.FromSeconds(5) ? $"等待新帧 · 上次 {received.ToLocalTime():HH:mm:ss}" : $"视频 · 到帧 {arrivalRate?.ToString("0.0") ?? "--"} 帧/秒 · {received.ToLocalTime():HH:mm:ss}";
            else if (Session is not null) { State.Text = status ?? "视频连接中"; State.ToolTip = status; }
        }
        internal void Fail(string reason) { Failed = true; if (FailureReason != reason) FailureLogged = false; FailureReason = reason; State.Text = "视频不可用：" + reason; State.ToolTip = reason; }
        internal void SetResource(MonitorResourceSample sample)
        {
            LatestResource = sample;
            if (sample.CpuPercent is not null && sample.WorkingSetBytes is > 0) ValidResourceSamples++;
            history.Add(sample); history.RemoveAll(item => sample.ObservedAt - item.ObservedAt > TimeSpan.FromMinutes(15));
            if (history.Count > 451) history.RemoveRange(0, history.Count - 451);
            Resource.Text = $"CPU {Percent(sample.CpuPercent, sample.Error is null)} · 工作集 {Memory(sample.WorkingSetBytes)}";
            Resource.ToolTip = $"{sample.ObservedAt.ToLocalTime():HH:mm:ss} · Player+VBox工作集含共享页，非安卓内存\n{sample.Error}";
            var memoryMax = Math.Max(1, history.Max(item => item.WorkingSetBytes ?? 0));
            cpuLine.Points.Clear(); memoryLine.Points.Clear();
            var cpuStart = history.FindLastIndex(item => item.CpuPercent is null) + 1;
            var memoryStart = history.FindLastIndex(item => item.WorkingSetBytes is null) + 1;
            for (var i = 0; i < history.Count; i++)
            {
                var x = i * 308d / Math.Max(1, history.Count - 1);
                if (i >= cpuStart && history[i].CpuPercent is { } cpu) cpuLine.Points.Add(new Point(x, 23 - Math.Clamp(cpu, 0, 100) * .22));
                if (i >= memoryStart && history[i].WorkingSetBytes is { } bytes) memoryLine.Points.Add(new Point(x, 23 - bytes * 22d / memoryMax));
            }
        }
        internal Task StopAsync()
        {
            var session = Session;
            if (session is null) return StopCompletion;
            Session = null;
            lock (frameGate) { videoEpoch++; pendingFrame = null; pendingStatus = null; }
            return StopCompletion = StopCoreAsync(session);
        }
        private async Task StopCoreAsync(ScreenVideoSession session)
        {
            try { await session.DisposeAsync(); }
            catch (Exception ex) { Fail("视频清理失败：" + ex.Message); }
            if (session.LastError is { } error) Fail(error);
        }
    }
}
