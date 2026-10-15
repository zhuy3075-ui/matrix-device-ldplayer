using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using MatrixDevice.Core;
using MatrixDevice.Host;

internal static class ScreenVideoTests
{
    public static void Register(Action<string, Func<Task>> test)
    {
        test("video requires allowlist identity guard transport and bounded even dimensions before guest access", AdmissionAsync);
        test("video rejects an untrusted server digest before uploading or launching it", ServerDigestAsync);
        test("video decoder publishes complete timestamped frames and rejects truncation without synthetic FPS", FramesAsync);
        test("video decoder process arguments force H264 BGRA passthrough with no shell", DecoderArgumentsAsync);
        test("video decoder preserves cleanup failures through wrapped cancellation errors", CleanupClassificationAsync);
        test("video local cancellation returns cancellation when normal server exit wins before copy", CompletionCancellationAsync);
        test("video completion keeps identity cleanup and abnormal exit failures during local cancellation", CompletionFailuresAsync);
        if (FindServer() is null)
        {
            Console.WriteLine("SKIP video component integration fixtures: set MATRIX_TEST_SCRCPY_SERVER to official scrcpy-server 3.3.4.");
            return;
        }
        test("video streams beyond screenshot limit without occupying the serial command lane", StreamingAsync);
        test("video duplicate lease is rejected and cancellation releases only its owned socket", CancellationAsync);
        test("video periodically rejects changed transport and bound identity", IdentityAsync);
        test("video rejects unsupported raw H264 with explicit diagnostics and never substitutes screenshots", UnsupportedAsync);
        test("video backend disposal drains owned video streams", DisposalAsync);
        test("video stalled cleanup signals only the session PID after birth verification", OwnedCleanupAsync);
        test("video mismatched cleanup proof blocks another stream on the same serial", CleanupRefusalAsync);
        test("video cleanup confirms process disappearance during snapshot signal and exit recheck", CleanupDisappearanceAsync);
        test("video cleanup preserves permission failures when the owned PID still exists", CleanupReadFailureAsync);
        test("video cancellation during startup OKAY handshake blocks unproven retry and preserves its artifact", StartupCancellationAsync);
        test("video disconnect after startup OKAY never releases an unproven session for retry", StartupDisconnectAsync);
        if (FindFfmpeg() is not null) test("video local official components decode H264 frames and dispose the owned decoder", DecoderIntegrationAsync);
    }

    private static string? FindServer()
    {
        var configured = Environment.GetEnvironmentVariable("MATRIX_TEST_SCRCPY_SERVER");
        if (!string.IsNullOrWhiteSpace(configured)) return configured;
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var path = Path.Combine(directory.FullName, ".tools", "scrcpy-server-3.3.4", "scrcpy-server-v3.3.4");
            if (File.Exists(path)) return path;
        }
        return null;
    }
    private static string? FindFfmpeg()
    {
        var path = Environment.GetEnvironmentVariable("MATRIX_TEST_FFMPEG") ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "WinGet", "Links", "ffmpeg.exe");
        return File.Exists(path) ? path : null;
    }
    private static AdbDeviceBackend Backend(FakeScreenVideoServer server) => new(new AppSettings { AdbAllowedSerials = ["allowed"], CommandTimeoutSeconds = 2, MonitorServerPath = FindServer() ?? "" }, server.Endpoint);
    private static void Assert(bool condition, string message) { if (!condition) throw new Exception(message); }
    private static async Task Expect(int status, Func<Task> action)
    {
        try { await action(); throw new Exception("Expected video rejection."); }
        catch (DeviceException ex) { Assert(ex.Status == status, $"Expected {status}, actual {ex.Status}: {ex.Message}"); }
    }
    private static async Task Canceled(Task task)
    {
        try { await task.WaitAsync(TimeSpan.FromSeconds(5)); throw new Exception("Video cancellation was ignored."); }
        catch (OperationCanceledException) { }
    }

    private static async Task AdmissionAsync()
    {
        await using var server = new FakeScreenVideoServer();
        await using var backend = Backend(server);
        await Expect(403, () => backend.StreamScreenVideoAsync("private", 32, 32, Stream.Null));
        await Expect(403, () => backend.StreamScreenVideoAsync("allowed", 32, 32, Stream.Null));
        backend.RegisterIdentityGuard("allowed", _ => Task.CompletedTask);
        await Expect(503, () => backend.StreamScreenVideoAsync("allowed", 32, 32, Stream.Null));
        foreach (var size in new[] { (0, 32), (31, 32), (32, 33), (1922, 32), (1920, 1920) })
            await Expect(400, () => backend.StreamScreenVideoAsync("allowed", size.Item1, size.Item2, Stream.Null));
        Assert(server.Requests.IsEmpty, "Invalid video admission reached ADB.");
    }

    private static async Task CompletionCancellationAsync()
    {
        using var stop = new CancellationTokenSource();
        stop.Cancel();
        var pendingCopy = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        await Canceled(ExistingAdbTransport.AwaitVideoCompletionAsync(pendingCopy.Task, Task.FromResult(0), "allowed", () => "normal exit", stop.Token));
        Assert(!pendingCopy.Task.IsCompleted, "The regression must force server exit to win before the copy completes.");
        var pendingServer = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        await Canceled(ExistingAdbTransport.AwaitVideoCompletionAsync(Task.CompletedTask, pendingServer.Task, "allowed", () => "EOF", stop.Token));
    }

    private static async Task CompletionFailuresAsync()
    {
        using var stop = new CancellationTokenSource(); stop.Cancel();
        var pending = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        await Expect(503, () => ExistingAdbTransport.AwaitVideoCompletionAsync(pending.Task, Task.FromResult(0), "allowed", () => "unexpected exit", CancellationToken.None));
        await Expect(503, () => ExistingAdbTransport.AwaitVideoCompletionAsync(pending.Task, Task.FromResult(7), "allowed", () => "encoder failed", stop.Token));
        var identity = new DeviceException("Bound device identity changed.", 409, 1003);
        var cleanup = new DeviceException("Owned video process exit unconfirmed.", 409, 1003); cleanup.Data["VideoCleanupUnconfirmed"] = true;
        foreach (var failure in new Exception[] { identity, cleanup, new IOException("Wrapped cleanup failure.", cleanup) })
        {
            foreach (var serverWins in new[] { true, false })
            {
                var copy = serverWins ? pending.Task : Task.FromException(failure);
                var server = serverWins ? Task.FromException<int>(failure) : new TaskCompletionSource<int>().Task;
                try { await ExistingAdbTransport.AwaitVideoCompletionAsync(copy, server, "allowed", () => "fault", stop.Token); throw new Exception("Video completion swallowed a fault."); }
                catch (Exception actual) when (ReferenceEquals(actual, failure)) { }
            }
        }
    }

    private static async Task StreamingAsync()
    {
        await using var server = new FakeScreenVideoServer { Flood = true };
        await using var backend = Backend(server);
        backend.RegisterIdentityGuard("allowed", _ => Task.CompletedTask);
        await backend.DiscoverAsync();
        using var stop = new CancellationTokenSource();
        using var output = new CountingVideoStream(AdbDeviceBackend.MaximumStdoutBytes + 65536L);
        var video = backend.StreamScreenVideoAsync("allowed", 32, 32, output, stop.Token);
        await output.Reached.Task.WaitAsync(TimeSpan.FromSeconds(5));
        var query = await backend.ShellAsync("allowed", "wm size").WaitAsync(TimeSpan.FromSeconds(2));
        Assert(query.Contains("32x32", StringComparison.Ordinal), "Long video occupied the serial command queue.");
        stop.Cancel(); await Canceled(video);
        Assert(output.Count > AdbDeviceBackend.MaximumStdoutBytes, "Video still used the screenshot aggregation limit.");
        Assert(!server.Requests.Any(request => request.Contains("screencap", StringComparison.Ordinal) || request.Contains("kill", StringComparison.Ordinal)), "Video used PNG or global process cleanup.");
    }

    private static async Task CancellationAsync()
    {
        await using var server = new FakeScreenVideoServer();
        await using var backend = Backend(server);
        backend.RegisterIdentityGuard("allowed", _ => Task.CompletedTask);
        await backend.DiscoverAsync();
        using var stop = new CancellationTokenSource();
        var video = backend.StreamScreenVideoAsync("allowed", 32, 32, Stream.Null, stop.Token);
        await server.VideoEntered.Task.WaitAsync(TimeSpan.FromSeconds(3));
        await Expect(409, () => backend.StreamScreenVideoAsync("allowed", 32, 32, Stream.Null));
        stop.Cancel(); await Canceled(video);
        await server.VideoClosed.Task.WaitAsync(TimeSpan.FromSeconds(3));
        server.EndImmediately = true;
        await Expect(503, () => backend.StreamScreenVideoAsync("allowed", 32, 32, Stream.Null).WaitAsync(TimeSpan.FromSeconds(3)));
        Assert((await backend.ShellAsync("allowed", "wm size")).Contains("32x32", StringComparison.Ordinal), "Video cancellation broke unrelated requests.");
    }

    private static async Task IdentityAsync()
    {
        foreach (var changeTransport in new[] { true, false })
        {
            await using var server = new FakeScreenVideoServer();
            await using var backend = Backend(server);
            var changed = false;
            backend.RegisterIdentityGuard("allowed", _ => changed ? throw new DeviceException("Bound instance replaced.", 409, 1003) : Task.CompletedTask);
            await backend.DiscoverAsync();
            var video = backend.StreamScreenVideoAsync("allowed", 32, 32, Stream.Null);
            await server.VideoEntered.Task.WaitAsync(TimeSpan.FromSeconds(3));
            if (changeTransport) server.TransportId = 2; else changed = true;
            await Expect(409, () => video.WaitAsync(TimeSpan.FromSeconds(5)));
            await server.VideoClosed.Task.WaitAsync(TimeSpan.FromSeconds(3));
            Assert(server.Requests.Count(request => request.Contains("com.genymobile.scrcpy.Server", StringComparison.Ordinal)) == 1, "Identity rejection silently reopened capture.");
        }
    }

    private static async Task UnsupportedAsync()
    {
        await using var server = new FakeScreenVideoServer { Unsupported = true };
        await using var backend = Backend(server);
        backend.RegisterIdentityGuard("allowed", _ => Task.CompletedTask);
        await backend.DiscoverAsync();
        using var output = new MemoryStream();
        await Expect(501, () => backend.StreamScreenVideoAsync("allowed", 32, 32, output));
        Assert(output.Length == 0, "Non-H264 error bytes were forwarded to the decoder.");
        Assert(server.Requests.All(request => !request.Contains("screencap", StringComparison.Ordinal)), "Unsupported video fell back to PNG.");
    }

    private static async Task DisposalAsync()
    {
        await using var server = new FakeScreenVideoServer();
        var backend = Backend(server);
        backend.RegisterIdentityGuard("allowed", _ => Task.CompletedTask);
        await backend.DiscoverAsync();
        var video = backend.StreamScreenVideoAsync("allowed", 32, 32, Stream.Null);
        await server.VideoEntered.Task.WaitAsync(TimeSpan.FromSeconds(3));
        await backend.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(5));
        await Canceled(video);
        await server.VideoClosed.Task.WaitAsync(TimeSpan.FromSeconds(3));
    }

    private static async Task ServerDigestAsync()
    {
        await using var server = new FakeScreenVideoServer();
        var artifact = Path.GetTempFileName();
        try
        {
            await File.WriteAllBytesAsync(artifact, [1, 2, 3]);
            await using var backend = new AdbDeviceBackend(new() { AdbAllowedSerials = ["allowed"], MonitorServerPath = artifact }, server.Endpoint);
            backend.RegisterIdentityGuard("allowed", _ => Task.CompletedTask);
            await backend.DiscoverAsync();
            await Expect(403, () => backend.StreamScreenVideoAsync("allowed", 32, 32, Stream.Null));
            Assert(!server.Requests.Any(request => request == "sync:" || request.Contains("app_process", StringComparison.Ordinal)), "Untrusted artifact reached guest.");
        }
        finally { File.Delete(artifact); }
    }

    private static async Task DecoderIntegrationAsync()
    {
        var ffmpeg = FindFfmpeg()!;
        var info = new System.Diagnostics.ProcessStartInfo(ffmpeg) { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true };
        foreach (var value in new[] { "-hide_banner", "-loglevel", "error", "-f", "lavfi", "-i", "testsrc=size=32x32:rate=5", "-frames:v", "8", "-pix_fmt", "yuv420p", "-c:v", "libx264", "-tune", "zerolatency", "-f", "h264", "pipe:1" }) info.ArgumentList.Add(value);
        using var generator = System.Diagnostics.Process.Start(info)!;
        using var fixture = new MemoryStream();
        var copy = generator.StandardOutput.BaseStream.CopyToAsync(fixture); var errors = generator.StandardError.ReadToEndAsync();
        await Task.WhenAll(copy, generator.WaitForExitAsync()).WaitAsync(TimeSpan.FromSeconds(5));
        Assert(generator.ExitCode == 0, "H264 fixture generation failed: " + await errors);
        for (var iteration = 0; iteration < 5; iteration++)
        {
        var unconfirmed = iteration == 4;
        await using var server = new FakeScreenVideoServer { VideoBytes = fixture.ToArray(), StallAfterDisconnect = unconfirmed, ChangedProcessBirth = unconfirmed };
        await using var backend = Backend(server);
        backend.RegisterIdentityGuard("allowed", _ => Task.CompletedTask);
        await backend.DiscoverAsync();
        var frames = new ConcurrentQueue<ScreenVideoFrame>();
        var ready = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        await using var session = new ScreenVideoSession(backend, "allowed", ffmpeg, 32, 32,
            frame => { frames.Enqueue(frame); if (frames.Count >= 3) ready.TrySetResult(); }, _ => { });
        session.Start();
        await ready.Task.WaitAsync(TimeSpan.FromSeconds(10));
        await session.DisposeAsync();
        var count = frames.Count;
        Assert((unconfirmed ? session.LastError?.Contains("清理未确认", StringComparison.Ordinal) == true : session.LastError is null) && !session.IsRunning && count is >= 3 and <= 8, "Real decoder misclassified shutdown or fabricated frames: " + session.LastError);
        Assert(frames.All(frame => frame.Bgra.Length == 32 * 32 * 4) && frames.First().Bgra.Distinct().Count() > 10, "FFmpeg did not decode actual image pixels.");
        Assert(frames.Count == count, "Frame callbacks continued after disposal.");
        }
    }

    private static Task CleanupClassificationAsync()
    {
        var method = typeof(ScreenVideoSession).GetMethod("HasUnconfirmedCleanup", BindingFlags.Static | BindingFlags.NonPublic)!;
        var cleanup = new DeviceException("Owned process exit unconfirmed."); cleanup.Data["VideoCleanupUnconfirmed"] = true;
        foreach (var error in new Exception[] { cleanup, new IOException("Decoder cancelled.", cleanup), new AggregateException(new OperationCanceledException(), cleanup) })
            Assert((bool)method.Invoke(null, [error])!, "Wrapped cleanup failure could be suppressed as normal cancellation.");
        Assert(!(bool)method.Invoke(null, [new IOException("Decoder stopped after cancellation.")])!, "Expected decoder cancellation was classified as unconfirmed cleanup.");
        return Task.CompletedTask;
    }

    private static async Task OwnedCleanupAsync()
    {
        await using var server = new FakeScreenVideoServer { StallAfterDisconnect = true };
        await using var backend = Backend(server);
        backend.RegisterIdentityGuard("allowed", _ => Task.CompletedTask);
        await backend.DiscoverAsync();
        using var stop = new CancellationTokenSource();
        var video = backend.StreamScreenVideoAsync("allowed", 32, 32, Stream.Null, stop.Token);
        await server.VideoEntered.Task.WaitAsync(TimeSpan.FromSeconds(3));
        stop.Cancel(); await Canceled(video);
        var signals = server.Requests.Where(request => request.Contains("kill -TERM", StringComparison.Ordinal)).ToArray();
        Assert(signals.Length == 1 && signals[0].Contains("kill -TERM 12345", StringComparison.Ordinal) && signals[0].Contains("789", StringComparison.Ordinal), "Cleanup did not freeze a single owned PID and birth.");
        Assert(server.Requests.All(request => !request.Contains("pkill", StringComparison.Ordinal) && !request.Contains("killall", StringComparison.Ordinal)), "Cleanup used a process name sweep.");
    }

    private static async Task CleanupRefusalAsync()
    {
        await using var server = new FakeScreenVideoServer { StallAfterDisconnect = true, ChangedProcessBirth = true };
        await using var backend = Backend(server);
        backend.RegisterIdentityGuard("allowed", _ => Task.CompletedTask);
        await backend.DiscoverAsync();
        using var stop = new CancellationTokenSource();
        var video = backend.StreamScreenVideoAsync("allowed", 32, 32, Stream.Null, stop.Token);
        await server.VideoEntered.Task.WaitAsync(TimeSpan.FromSeconds(3));
        stop.Cancel(); await Expect(409, () => video.WaitAsync(TimeSpan.FromSeconds(6)));
        var count = server.Requests.Count;
        await Expect(409, () => backend.StreamScreenVideoAsync("allowed", 32, 32, Stream.Null));
        Assert(server.Requests.Count == count && server.Requests.All(request => !request.Contains("kill -TERM", StringComparison.Ordinal)), "Unproven cleanup killed a replacement or permitted overlapping retry.");
    }

    private static async Task StartupCancellationAsync()
    {
        await using var server = new FakeScreenVideoServer { HoldServerOkay = true };
        await using var backend = Backend(server);
        backend.RegisterIdentityGuard("allowed", _ => Task.CompletedTask);
        await backend.DiscoverAsync();
        using var stop = new CancellationTokenSource();
        var video = backend.StreamScreenVideoAsync("allowed", 32, 32, Stream.Null, stop.Token);
        await server.ServerRequestEntered.Task.WaitAsync(TimeSpan.FromSeconds(3));
        stop.Cancel();
        await AssertUnprovenStartupAsync(server, backend, video);
    }

    private static async Task CleanupDisappearanceAsync()
    {
        foreach (var stage in new[] { "snapshot", "signal", "recheck" })
        {
            await using var server = new FakeScreenVideoServer { StallAfterDisconnect = true, ProcessDisappearsAt = stage };
            await using var backend = Backend(server);
            backend.RegisterIdentityGuard("allowed", _ => Task.CompletedTask);
            await backend.DiscoverAsync();
            using var stop = new CancellationTokenSource();
            var video = backend.StreamScreenVideoAsync("allowed", 32, 32, Stream.Null, stop.Token);
            await server.VideoEntered.Task.WaitAsync(TimeSpan.FromSeconds(3));
            stop.Cancel(); await Canceled(video);
            Assert(server.Requests.Any(request => request.Contains("MATRIX_VIDEO_PID_12345_ABSENT", StringComparison.Ordinal)), "Disappeared process was accepted without an explicit second absence check: " + stage);
            Assert(server.Requests.Any(request => request.StartsWith("shell,v2,raw:rm -f", StringComparison.Ordinal)), "Confirmed exited process retained its owned artifact: " + stage);
            server.EndImmediately = true;
            await Expect(503, () => backend.StreamScreenVideoAsync("allowed", 32, 32, Stream.Null).WaitAsync(TimeSpan.FromSeconds(5)));
        }
    }

    private static async Task CleanupReadFailureAsync()
    {
        foreach (var stage in new[] { "snapshot", "signal", "recheck" })
        {
            await using var server = new FakeScreenVideoServer { StallAfterDisconnect = true, ProcessReadFailsAt = stage };
            await using var backend = Backend(server);
            backend.RegisterIdentityGuard("allowed", _ => Task.CompletedTask);
            await backend.DiscoverAsync();
            using var stop = new CancellationTokenSource();
            var video = backend.StreamScreenVideoAsync("allowed", 32, 32, Stream.Null, stop.Token);
            await server.VideoEntered.Task.WaitAsync(TimeSpan.FromSeconds(3));
            stop.Cancel(); await Expect(409, () => video.WaitAsync(TimeSpan.FromSeconds(6)));
            var count = server.Requests.Count;
            await Expect(409, () => backend.StreamScreenVideoAsync("allowed", 32, 32, Stream.Null));
            Assert(server.Requests.Count == count && server.Requests.All(request => !request.StartsWith("shell,v2,raw:rm -f", StringComparison.Ordinal)), "Still-existing process read failure was treated as a successful exit: " + stage);
        }
    }

    private static async Task StartupDisconnectAsync()
    {
        await using var server = new FakeScreenVideoServer { DisconnectAfterServerOkay = true };
        await using var backend = Backend(server);
        backend.RegisterIdentityGuard("allowed", _ => Task.CompletedTask);
        await backend.DiscoverAsync();
        var video = backend.StreamScreenVideoAsync("allowed", 32, 32, Stream.Null);
        await AssertUnprovenStartupAsync(server, backend, video);
    }

    private static async Task AssertUnprovenStartupAsync(FakeScreenVideoServer server, AdbDeviceBackend backend, Task video)
    {
        await Expect(409, () => video.WaitAsync(TimeSpan.FromSeconds(6)));
        var count = server.Requests.Count;
        await Expect(409, () => backend.StreamScreenVideoAsync("allowed", 32, 32, Stream.Null));
        Assert(server.Requests.Count == count, "Unproven startup permitted another guest request.");
        Assert(server.Requests.All(request => !request.Contains("kill -TERM", StringComparison.Ordinal) && !request.StartsWith("shell,v2,raw:rm -f", StringComparison.Ordinal)), "Unproven startup signalled a PID or removed its evidence artifact.");
    }

    private static async Task FramesAsync()
    {
        await using var backend = new AdbDeviceBackend(new());
        var received = new List<ScreenVideoFrame>();
        await using var session = new ScreenVideoSession(backend, "allowed", "unused.exe", 16, 16, received.Add, _ => { });
        var method = typeof(ScreenVideoSession).GetMethod("ReadFramesAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var bytes = Enumerable.Range(0, 16 * 16 * 4 * 3).Select(index => (byte)(index % 251)).ToArray();
        using var input = new FragmentedVideoStream(bytes);
        var before = DateTimeOffset.UtcNow;
        await (Task)method.Invoke(session, [input, CancellationToken.None])!;
        Assert(received.Count == 3 && received.Select(frame => frame.Sequence).SequenceEqual(new long[] { 1, 2, 3 }), "Decoder lost, repeated or reordered complete frames.");
        Assert(received.All(frame => frame.Width == 16 && frame.Height == 16 && frame.ReceivedAt >= before), "Frame geometry or receive clock missing.");
        Assert(received.SelectMany(frame => frame.Bgra).SequenceEqual(bytes), "Fragmented output corrupted BGRA frames.");
        using var truncated = new FragmentedVideoStream(new byte[100]);
        try { await (Task)method.Invoke(session, [truncated, CancellationToken.None])!; throw new Exception("Truncated frame published."); }
        catch (IOException) { }
        Assert(received.Count == 3, "Partial frame was published.");
    }

    private static Task DecoderArgumentsAsync()
    {
        var method = typeof(ScreenVideoSession).GetMethod("CreateStartInfo", BindingFlags.Static | BindingFlags.NonPublic)!;
        var info = (System.Diagnostics.ProcessStartInfo)method.Invoke(null, ["C:\\trusted path\\ffmpeg.exe", 320, 240])!;
        var arguments = info.ArgumentList.ToArray();
        Assert(!info.UseShellExecute && info.CreateNoWindow && info.RedirectStandardInput && info.RedirectStandardOutput && info.RedirectStandardError, "Decoder process isolation missing.");
        Assert(info.FileName == "C:\\trusted path\\ffmpeg.exe" && arguments.Contains("h264") && arguments.Contains("bgra") && arguments.Contains("scale=320:240") && arguments.Contains("passthrough"), "Fixed video decode contract missing.");
        Assert(!arguments.Contains("-r") && !arguments.Any(argument => argument.Contains("fps=", StringComparison.Ordinal)), "Decoder fabricated repeated frames.");
        return Task.CompletedTask;
    }
}

internal sealed class CountingVideoStream(long target) : Stream
{
    public long Count { get; private set; }
    public TaskCompletionSource Reached { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested(); Count += buffer.Length;
        if (Count >= target) Reached.TrySetResult();
        return ValueTask.CompletedTask;
    }
    public override bool CanRead => false; public override bool CanSeek => false; public override bool CanWrite => true;
    public override long Length => Count; public override long Position { get => Count; set => throw new NotSupportedException(); }
    public override void Flush() { }
    public override void Write(byte[] buffer, int offset, int count) => WriteAsync(buffer.AsMemory(offset, count)).GetAwaiter().GetResult();
    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
}

internal sealed class FragmentedVideoStream(byte[] bytes) : MemoryStream(bytes)
{
    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken ct = default) => base.ReadAsync(buffer[..Math.Min(buffer.Length, 37)], ct);
}

internal sealed class FakeScreenVideoServer : IAsyncDisposable
{
    private readonly TcpListener _listener = new(IPAddress.Loopback, 0);
    private readonly CancellationTokenSource _stop = new();
    private readonly ConcurrentBag<Task> _clients = [];
    private readonly Task _accepting;
    public IPEndPoint Endpoint => (IPEndPoint)_listener.LocalEndpoint;
    public ConcurrentQueue<string> Requests { get; } = [];
    public TaskCompletionSource VideoEntered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource VideoClosed { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public uint TransportId { get; set; } = 1;
    public bool Flood { get; set; }
    public bool Unsupported { get; set; }
    public bool EndImmediately { get; set; }
    public byte[]? VideoBytes { get; set; }
    public bool StallAfterDisconnect { get; set; }
    public bool ChangedProcessBirth { get; set; }
    public bool HoldServerOkay { get; set; }
    public bool DisconnectAfterServerOkay { get; set; }
    public string? ProcessDisappearsAt { get; set; }
    public string? ProcessReadFailsAt { get; set; }
    public TaskCompletionSource ServerRequestEntered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private bool _processAlive = true;
    private string _processCommandLine = "";
    private static string Stat(bool changed = false) => "12345 (matrix (test)) S " + string.Join(' ', Enumerable.Repeat("0", 18)) + (changed ? " 999 0\n" : " 789 0\n");
    private TaskCompletionSource? _activeServer;

    public FakeScreenVideoServer() { _listener.Start(); _accepting = AcceptAsync(); }
    private async Task AcceptAsync()
    {
        try
        {
            while (!_stop.IsCancellationRequested) _clients.Add(HandleAsync(await _listener.AcceptTcpClientAsync(_stop.Token)));
        }
        catch (OperationCanceledException) { }
    }
    private async Task HandleAsync(TcpClient client)
    {
        using (client)
        {
            try
            {
                var stream = client.GetStream();
                while (!_stop.IsCancellationRequested)
                {
                    var prefix = new byte[4]; await stream.ReadExactlyAsync(prefix, _stop.Token);
                    var body = new byte[int.Parse(Encoding.ASCII.GetString(prefix), NumberStyles.HexNumber, CultureInfo.InvariantCulture)];
                    await stream.ReadExactlyAsync(body, _stop.Token);
                    var request = Encoding.UTF8.GetString(body); Requests.Enqueue(request);
                    if (request == "host:version") { await HostAsync(stream, "0029"); return; }
                    if (request == "host:devices-l") { await HostAsync(stream, $"allowed\tdevice model:Fixture transport_id:{TransportId}\n"); return; }
                    var startsServer = request.Contains("com.genymobile.scrcpy.Server", StringComparison.Ordinal);
                    if (startsServer)
                    {
                        ServerRequestEntered.TrySetResult();
                        if (HoldServerOkay) await Task.Delay(Timeout.Infinite, _stop.Token);
                    }
                    await stream.WriteAsync("OKAY"u8.ToArray(), _stop.Token);
                    if (startsServer && DisconnectAfterServerOkay) return;
                    if (request.StartsWith("host:transport-id:", StringComparison.Ordinal)) continue;
                    if (request == "sync:") { await SyncAsync(stream); return; }
                    if (request.StartsWith("localabstract:scrcpy_", StringComparison.Ordinal))
                    {
                        var active = _activeServer;
                        if (Unsupported)
                        {
                            try { await stream.WriteAsync("not h264"u8.ToArray(), _stop.Token); var closed = await stream.ReadAsync(new byte[1], _stop.Token); if (closed != 0) throw new IOException("Unexpected video input."); }
                            finally { active?.TrySetResult(); }
                            return;
                        }
                        await stream.WriteAsync(VideoBytes ?? [0, 0, 0, 1, 0x67, 0x42, 0, 0x1f], _stop.Token);
                        VideoEntered.TrySetResult();
                        try
                        {
                            if (EndImmediately) return;
                            if (Flood)
                            {
                                var block = new byte[65536];
                                while (!_stop.IsCancellationRequested) await stream.WriteAsync(block, _stop.Token);
                            }
                            else { var closed = await stream.ReadAsync(new byte[1], _stop.Token); if (closed != 0) throw new IOException("Unexpected client video bytes."); }
                        }
                        finally { if (!StallAfterDisconnect) active?.TrySetResult(); VideoClosed.TrySetResult(); }
                        return;
                    }
                    if (!request.StartsWith("shell,v2,raw:", StringComparison.Ordinal)) throw new IOException("Unexpected video fixture request: " + request);
                    await stream.ReadExactlyAsync(new byte[5], _stop.Token);
                    if (request.Contains("com.genymobile.scrcpy.Server", StringComparison.Ordinal))
                    {
                        var active = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                        _activeServer = active;
                        _processAlive = true;
                        var marker = Regex.Match(request, @"MATRIX_VIDEO_[0-9a-f]{32}:").Value;
                        var nonce = Regex.Match(request, @"matrix\.video\.session=([0-9a-f]{32})").Groups[1].Value;
                        var scid = Regex.Match(request, @"scid=([0-9a-f]{8})").Groups[1].Value;
                        _processCommandLine = "app_process\0-Dmatrix.video.session=" + nonce + "\0/\0com.genymobile.scrcpy.Server\0" + "3.3.4\0scid=" + scid + "\0";
                        await PacketAsync(stream, 1, Encoding.UTF8.GetBytes(marker + "12345\n" + Stat()));
                        await active.Task.WaitAsync(_stop.Token);
                        await PacketAsync(stream, 3, [0]); return;
                    }
                    var command = request[13..];
                    string text;
                    var stage = command.Contains("kill -TERM 12345", StringComparison.Ordinal) ? "signal" : command.Contains("cmdline", StringComparison.Ordinal) ? "snapshot" : command == "cat /proc/12345/stat" ? "recheck" : null;
                    if (stage is not null && (ProcessDisappearsAt == stage || ProcessReadFailsAt == stage))
                    {
                        var disappeared = ProcessDisappearsAt == stage;
                        if (disappeared) { _processAlive = false; _activeServer?.TrySetResult(); }
                        await PacketAsync(stream, 2, Encoding.UTF8.GetBytes(disappeared ? "cat: /proc/12345/stat: No such file or directory" : "cat: /proc/12345/stat: Permission denied"));
                        await PacketAsync(stream, 3, [1]); return;
                    }
                    if (command.Contains("MATRIX_VIDEO_PID_12345_ABSENT", StringComparison.Ordinal)) text = _processAlive ? "" : "MATRIX_VIDEO_PID_12345_ABSENT";
                    else if (command.Contains("kill -TERM 12345", StringComparison.Ordinal))
                    {
                        _processAlive = ProcessDisappearsAt == "recheck" || ProcessReadFailsAt == "recheck";
                        if (!_processAlive) _activeServer?.TrySetResult();
                        text = "";
                    }
                    else if (command.Contains("/proc/12345/stat", StringComparison.Ordinal))
                    {
                        if (!_processAlive) { await PacketAsync(stream, 2, "cat: /proc/12345/stat: No such file or directory"u8.ToArray()); await PacketAsync(stream, 3, [1]); return; }
                        text = Stat(ChangedProcessBirth) + (command.Contains("cmdline", StringComparison.Ordinal) ? "\n" + _processCommandLine : "");
                    }
                    else text = command switch { "getprop" => "[ro.product.model]: [Fixture]\n", "wm size" => "Physical size: 32x32\n", "dumpsys input" => "SurfaceOrientation: 0\n", _ => "fixture\n" };
                    await PacketAsync(stream, 1, Encoding.UTF8.GetBytes(text));
                    await PacketAsync(stream, 3, [0]); return;
                }
            }
            catch (Exception ex) when (ex is IOException or SocketException or OperationCanceledException) { }
        }
    }
    private async Task SyncAsync(Stream stream)
    {
        var header = new byte[8];
        while (true)
        {
            await stream.ReadExactlyAsync(header, _stop.Token);
            var name = Encoding.ASCII.GetString(header, 0, 4); var length = BinaryPrimitives.ReadInt32LittleEndian(header.AsSpan(4));
            if (name == "QUIT") return;
            if (name == "DONE") { await stream.WriteAsync(new byte[] { 0x4f, 0x4b, 0x41, 0x59, 0, 0, 0, 0 }, _stop.Token); continue; }
            if (length < 0 || length > 1024 * 1024) throw new IOException("Invalid sync fixture length.");
            await stream.ReadExactlyAsync(new byte[length], _stop.Token);
        }
    }
    private async Task HostAsync(Stream stream, string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        await stream.WriteAsync(Encoding.ASCII.GetBytes("OKAY" + bytes.Length.ToString("X4", CultureInfo.InvariantCulture)), _stop.Token);
        await stream.WriteAsync(bytes, _stop.Token);
    }
    private async Task PacketAsync(Stream stream, byte channel, byte[] bytes)
    {
        var header = new byte[5]; header[0] = channel; BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(1), bytes.Length);
        await stream.WriteAsync(header, _stop.Token); await stream.WriteAsync(bytes, _stop.Token);
    }
    public async ValueTask DisposeAsync()
    {
        _stop.Cancel(); _listener.Stop(); await _accepting; await Task.WhenAll(_clients); _stop.Dispose();
    }
}
