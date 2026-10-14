using System.Diagnostics;
using System.Text;
using MatrixDevice.Core;

namespace MatrixDevice.Host;

public sealed record ScreenVideoFrame(int Width, int Height, byte[] Bgra, long Sequence, DateTimeOffset ReceivedAt);

public sealed class ScreenVideoSession : IAsyncDisposable
{
    private readonly AdbDeviceBackend _backend;
    private readonly string _serial;
    private readonly string _ffmpegPath;
    private readonly int _width;
    private readonly int _height;
    private readonly Action<ScreenVideoFrame> _frame;
    private readonly Action<string> _status;
    private readonly CancellationTokenSource _shutdown = new();
    private readonly object _gate = new();
    private Task? _completion;
    private bool _disposed;
    private string? _lastError;
    private long _sequence;

    public ScreenVideoSession(AdbDeviceBackend backend, string serial, string ffmpegPath, int width, int height,
        Action<ScreenVideoFrame> frame, Action<string> status)
    {
        ArgumentNullException.ThrowIfNull(backend);
        ArgumentException.ThrowIfNullOrWhiteSpace(serial);
        ArgumentException.ThrowIfNullOrWhiteSpace(ffmpegPath);
        ArgumentNullException.ThrowIfNull(frame);
        ArgumentNullException.ThrowIfNull(status);
        if (width is < 16 or > 1920 || height is < 16 or > 1920 || width % 2 != 0 || height % 2 != 0 || (long)width * height > 2073600)
            throw new ArgumentOutOfRangeException(nameof(width), $"Invalid video dimensions: {width}x{height}.");
        _backend = backend;
        _serial = serial;
        _ffmpegPath = Path.GetFullPath(ffmpegPath);
        _width = width;
        _height = height;
        _frame = frame;
        _status = status;
    }

    public Task Completion { get { lock (_gate) return _completion ?? Task.CompletedTask; } }
    public bool IsRunning { get { lock (_gate) return _completion is { IsCompleted: false }; } }
    public string? LastError => Volatile.Read(ref _lastError);

    public void Start()
    {
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (_completion is not null) throw new InvalidOperationException("A video session can only be started once.");
            _completion = Task.Run(() => RunAsync(_shutdown.Token));
        }
    }

    private async Task RunAsync(CancellationToken ct)
    {
        try
        {
            _status("正在启动 H.264 视频流");
            await RunSegmentAsync(ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            _status("视频流已停止");
        }
        catch (Exception ex)
        {
            Volatile.Write(ref _lastError, ex.Message);
            _status("视频已停止：" + ex.Message);
        }
    }

    private async Task RunSegmentAsync(CancellationToken ct)
    {
        if (!File.Exists(_ffmpegPath)) throw new FileNotFoundException("找不到视频解码组件。", _ffmpegPath);
        using var process = new Process { StartInfo = CreateStartInfo(_ffmpegPath, _width, _height) };
        if (!process.Start()) throw new IOException("无法启动视频解码组件。");
        using var segment = CancellationTokenSource.CreateLinkedTokenSource(ct);
        using var cancellation = segment.Token.Register(() => StopDecoder(process));
        var errors = new StringBuilder();
        var stderr = CaptureErrorsAsync(process.StandardError, errors);
        var stream = PumpAsync(process.StandardInput.BaseStream, segment.Token);
        var frames = ReadFramesAsync(process.StandardOutput.BaseStream, segment.Token);
        var exit = process.WaitForExitAsync();
        try
        {
            var completed = await Task.WhenAny(stream, frames, exit).ConfigureAwait(false);
            if (completed == exit)
            {
                await exit.ConfigureAwait(false);
                if (!stream.IsCompleted) throw new IOException($"视频解码器提前退出（代码 {process.ExitCode}）。");
            }
            if (completed == frames)
            {
                await frames.ConfigureAwait(false);
                if (!stream.IsCompleted) throw new IOException("视频解码器在采集结束前关闭了输出。");
            }
            await stream.ConfigureAwait(false);
            await Task.WhenAll(frames, exit).WaitAsync(TimeSpan.FromSeconds(5), ct).ConfigureAwait(false);
            await stderr.ConfigureAwait(false);
            if (process.ExitCode != 0) throw new IOException($"视频解码失败（代码 {process.ExitCode}）。");
            throw new IOException("视频采集连接已结束。");
        }
        catch (Exception ex) when (!ct.IsCancellationRequested)
        {
            await segment.CancelAsync().ConfigureAwait(false);
            await ObserveCleanupAsync(stderr).ConfigureAwait(false);
            throw new IOException(ex.Message + (errors.Length == 0 ? "" : " 解码器：" + errors), ex);
        }
        finally
        {
            await segment.CancelAsync().ConfigureAwait(false);
            await ObserveCleanupAsync(Task.WhenAll(stream, frames, exit, stderr)).ConfigureAwait(false);
            var cleanupError = stream.Exception?.GetBaseException();
            if (cleanupError?.Data.Contains("VideoCleanupUnconfirmed") == true)
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(cleanupError).Throw();
        }
    }

    private static ProcessStartInfo CreateStartInfo(string ffmpegPath, int width, int height)
    {
        var info = new ProcessStartInfo(ffmpegPath)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        foreach (var argument in new[] { "-hide_banner", "-loglevel", "error", "-probesize", "32", "-analyzeduration", "0",
            "-threads", "1", "-f", "h264", "-i", "pipe:0", "-an", "-vf", FormattableString.Invariant($"scale={width}:{height}"),
            "-threads", "1", "-pix_fmt", "bgra", "-fps_mode", "passthrough", "-flush_packets", "1", "-f", "rawvideo", "pipe:1" })
            info.ArgumentList.Add(argument);
        return info;
    }

    private async Task PumpAsync(Stream stdin, CancellationToken ct)
    {
        try { await _backend.StreamScreenVideoAsync(_serial, _width, _height, stdin, ct).ConfigureAwait(false); }
        finally { await stdin.DisposeAsync().ConfigureAwait(false); }
    }

    private async Task ReadFramesAsync(Stream stdout, CancellationToken ct)
    {
        var first = true;
        while (true)
        {
            var bytes = new byte[checked(_width * _height * 4)];
            var offset = 0;
            using var deadline = CancellationTokenSource.CreateLinkedTokenSource(ct);
            if (first) deadline.CancelAfter(TimeSpan.FromSeconds(20));
            try
            {
                while (offset < bytes.Length)
                {
                    var read = await stdout.ReadAsync(bytes.AsMemory(offset), deadline.Token).ConfigureAwait(false);
                    if (read == 0)
                    {
                        if (offset > 0) throw new IOException($"视频帧被截断：{offset}/{bytes.Length} 字节。");
                        if (first) throw new IOException("视频解码器未输出首帧，设备可能不支持 H.264 采集。");
                        return;
                    }
                    offset += read;
                }
            }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested)
            {
                throw new TimeoutException("等待视频首帧超过 20 秒，已停止当前采集。");
            }
            // Deliver synchronously: the consumer replaces its latest frame; no frame or dispatcher queue is built here.
            _frame(new(_width, _height, bytes, Interlocked.Increment(ref _sequence), DateTimeOffset.UtcNow));
            if (first) { _status("H.264 视频流已连接"); first = false; }
        }
    }

    private static async Task CaptureErrorsAsync(StreamReader stderr, StringBuilder errors)
    {
        var block = new char[1024];
        int count;
        while ((count = await stderr.ReadAsync(block).ConfigureAwait(false)) != 0)
        {
            errors.Append(block, 0, count);
            if (errors.Length > 8192) errors.Remove(0, errors.Length - 8192);
        }
    }

    private static void StopDecoder(Process process)
    {
        try { if (!process.HasExited) process.Kill(entireProcessTree: false); }
        catch (InvalidOperationException) { }
    }

    private static async Task ObserveCleanupAsync(Task task)
    {
        try { await task.WaitAsync(TimeSpan.FromSeconds(15)).ConfigureAwait(false); }
        catch (TimeoutException ex) { throw new IOException("视频会话清理超过 15 秒，退出状态未确认。", ex); }
        catch (Exception) { /* The main task retains failures; cleanup only drains already stopped owned streams. */ }
    }

    public async ValueTask DisposeAsync()
    {
        Task? completion;
        lock (_gate)
        {
            if (_disposed) return;
            _disposed = true;
            completion = _completion;
        }
        await _shutdown.CancelAsync().ConfigureAwait(false);
        if (completion is not null) await completion.ConfigureAwait(false);
        _shutdown.Dispose();
    }
}
