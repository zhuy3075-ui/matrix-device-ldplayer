using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Globalization;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using MatrixDevice.Core;

internal static class ExistingAdbTests
{
    public static void Register(Action<string, Func<Task>> test)
    {
        test("existing ADB process replacement cannot reuse old transport or receive a request", ProcessReplacementAsync);
        test("LDPlayer binding candidate must belong to the expected process and sends no ADB request", async () =>
        {
            await using var peer = new FakeAdbServer(); using var process = Process.GetCurrentProcess();
            var serial = "127.0.0.1:" + peer.Endpoint.Port;
            await LdPlayerAdbBinding.VerifyAsync(serial, process.Id, process.StartTime.ToUniversalTime().Ticks);
            await Expect(409, () => LdPlayerAdbBinding.VerifyAsync(serial, process.Id, process.StartTime.ToUniversalTime().Ticks + 1));
            await Expect(409, () => LdPlayerAdbBinding.VerifyAsync(serial, int.MaxValue, process.StartTime.ToUniversalTime().Ticks));
            await Expect(501, () => LdPlayerAdbBinding.VerifyAsync("USB-unknown", process.Id, process.StartTime.ToUniversalTime().Ticks));
            Assert(peer.Requests.IsEmpty, "Binding probe sent an ADB or guest command.");
        });
        test("explicit Unicode input binding requires a discovered transport and verified server before callback", async () =>
        {
            await using var server = new FakeAdbServer();
            await using var backend = new AdbDeviceBackend(new AppSettings { AdbAllowedSerials = ["allowed"] }, server.Endpoint);
            var calls = 0; string? sent = null;
            backend.RegisterUnicodeInput("allowed", (text, ct) => { ct.ThrowIfCancellationRequested(); sent = text; calls++; return Task.CompletedTask; });
            await Expect(503, () => backend.TextAsync("allowed", "中文 测试")); Assert(calls == 0, "Cold input callback executed.");
            await backend.DiscoverAsync(); await backend.TextAsync("allowed", "中文 测试");
            Assert(calls == 1 && sent == "中文 测试" && server.Requests.Last() == "shell,v2,raw::", "Bound input did not verify the pinned connection first.");
            await Expect(400, () => backend.TextAsync("allowed", "中文\n测试")); Assert(calls == 1, "Invalid input callback executed.");
            server.InvalidIdentity = "absent"; await backend.DiscoverAsync();
            await Expect(503, () => backend.TextAsync("allowed", "中文")); Assert(calls == 1, "Missing target callback executed.");
        });
        test("UI dump without a confirmed file fails before parsing and cleans only its own path", async () =>
        {
            await using var server = new FakeAdbServer();
            await using var backend = new AdbDeviceBackend(new AppSettings { AdbAllowedSerials = ["allowed"] }, server.Endpoint);
            await backend.DiscoverAsync();
            await Expect(409, () => backend.DumpAsync("allowed"));
            Assert(!server.Requests.Any(request => request.StartsWith("shell,v2,raw:cat ", StringComparison.Ordinal)), "Unconfirmed UI file was read.");
            Assert(server.Requests.Any(request => request.StartsWith("shell,v2,raw:rm -f '/data/local/tmp/matrix-ui-", StringComparison.Ordinal)), "Owned dump file cleanup missing.");
        });
        test("ADB activity start does not require monkey and rejects semantic launch failure", async () =>
        {
            await using var server = new FakeAdbServer();
            await using var backend = new AdbDeviceBackend(new AppSettings { AdbAllowedSerials = ["allowed"] }, server.Endpoint);
            await backend.DiscoverAsync();
            await backend.StartAppAsync("allowed", "com.matrix.test");
            Assert(server.Requests.Any(request => request.Contains("-n com.matrix.test/.LauncherAlias", StringComparison.Ordinal)) && !server.Requests.Any(request => request.Contains("monkey", StringComparison.Ordinal)), "Launcher did not resolve the explicit alias.");
            server.LaunchOutput = "Starting: Intent\nError: Activity not found\n";
            await Expect(409, () => backend.StartAppAsync("allowed", "com.matrix.test"));
            server.LaunchOutput = "Status: timeout\n";
            await Expect(409, () => backend.StartAppAsync("allowed", "com.matrix.test"));
            server.LaunchOutput = "Starting: Intent\n";
            await Expect(502, () => backend.StartAppAsync("allowed", "com.matrix.test", ".Main"));
        });
        test("existing ADB server epoch rejects PID and start-time reuse before any guest request", async () =>
        {
            var epoch = new AdbServerEpoch();
            epoch.Require(new(42, 100)); epoch.Require(new(42, 100));
            await Expect(409, () => { epoch.Require(new(42, 101)); return Task.CompletedTask; });
            await Expect(409, () => { epoch.Require(new(43, 100)); return Task.CompletedTask; });
            epoch.Require(new(42, 100));
        });
        test("existing ADB reconnect pins queued transport and invalidates catalog generation", ReconnectAsync);
        test("existing ADB unknown invalid offline and missing identities never execute guest writes", InvalidIdentityAsync);
        test("existing ADB discovery touches only authorized guest and preserves binary PNG", async () =>
        {
            await using var server = new FakeAdbServer();
            await using var backend = new AdbDeviceBackend(new AppSettings { AdbAllowedSerials = ["allowed"] }, server.Endpoint);
            var devices = await backend.DiscoverAsync();
            Assert(devices.Count == 1 && devices[0].Serial == "allowed" && devices[0].Width == 4 && devices[0].Height == 3, "Discovery did not filter or parse authorized device");
            var png = await backend.ScreenshotAsync("allowed");
            Assert(png.SequenceEqual(server.Png), "Binary screenshot was transformed");
            await Expect(403, () => backend.KeyEventAsync("private", 3));
            await Expect(403, () => backend.SetNodeTextAsync("private", new(), "private"));
            Assert(!server.Requests.Any(request => request.Contains("private", StringComparison.Ordinal)), "Unauthorized guest was queried");
            Assert(!server.Requests.Any(request => request.Contains("kill", StringComparison.Ordinal) || request.Contains("start-server", StringComparison.Ordinal)), "Server control request emitted");
        });
        test("existing ADB empty authorization never queries guest and wrong version stays untouched", async () =>
        {
            await using var server = new FakeAdbServer();
            await using var empty = new AdbDeviceBackend(new AppSettings(), server.Endpoint);
            Assert((await empty.DiscoverAsync()).Count == 0, "Empty allowlist exposed guest");
            Assert(server.Requests.All(request => request is "host:version" or "host:devices-l"), "Empty allowlist queried guest");
            server.Version = "0028";
            await using var backend = new AdbDeviceBackend(new AppSettings { AdbAllowedSerials = ["allowed"] }, server.Endpoint);
            await Expect(503, () => backend.DiscoverAsync());
            Assert(server.Requests.Last() == "host:version", "Version rejection emitted a device/server write");
        });
        test("missing existing ADB server fails without executing configured binary", async () =>
        {
            var listener = new TcpListener(IPAddress.Loopback, 0); listener.Start(); var endpoint = (IPEndPoint)listener.LocalEndpoint; listener.Stop();
            await using var backend = new AdbDeviceBackend(new AppSettings { AdbPath = Environment.ProcessPath!, AdbAllowedSerials = ["allowed"] }, endpoint);
            await Expect(503, () => backend.DiscoverAsync());
        });
        test("existing ADB shell checks exit codes truncation output bounds and recovers after cancellation", async () =>
        {
            await using var server = new FakeAdbServer();
            await using var backend = new AdbDeviceBackend(new AppSettings { AdbAllowedSerials = ["allowed"], CommandTimeoutSeconds = 1 }, server.Endpoint);
            await backend.DiscoverAsync();
            await Expect(503, () => backend.ShellAsync("allowed", "fail"));
            await Expect(503, () => backend.ShellAsync("allowed", "truncated"));
            await Expect(413, () => backend.ShellAsync("allowed", "huge"));
            await Expect(504, () => backend.ShellAsync("allowed", "blocked"));
            using var cancel = new CancellationTokenSource(50);
            try { await backend.ShellAsync("allowed", "blocked", ct: cancel.Token); throw new Exception("Cancellation accepted"); }
            catch (OperationCanceledException) when (cancel.IsCancellationRequested) { }
            Assert((await backend.ShellAsync("allowed", "wm size")).Contains("4x3", StringComparison.Ordinal), "Queue failed to recover");
        });
        test("existing ADB sync roundtrip is byte exact and failed pull cannot replace local file", async () =>
        {
            await using var server = new FakeAdbServer();
            await using var backend = new AdbDeviceBackend(new AppSettings { AdbAllowedSerials = ["allowed"] }, server.Endpoint);
            await backend.DiscoverAsync();
            var directory = Path.Combine(Path.GetTempPath(), "MatrixDevice.Tests", Guid.NewGuid().ToString("N")); Directory.CreateDirectory(directory);
            var upload = Path.Combine(directory, "upload.bin"); var download = Path.Combine(directory, "download.bin");
            var bytes = Enumerable.Range(0, 200000).Select(index => (byte)(index % 251)).ToArray();
            await File.WriteAllBytesAsync(upload, bytes);
            await backend.PushAsync("allowed", upload, "/sdcard/test.bin");
            await backend.PullAsync("allowed", "/sdcard/test.bin", download);
            Assert((await File.ReadAllBytesAsync(download)).SequenceEqual(bytes), "Sync content differs");
            server.TruncatePull = true;
            await Expect(503, () => backend.PullAsync("allowed", "/sdcard/test.bin", download));
            Assert((await File.ReadAllBytesAsync(download)).SequenceEqual(bytes), "Failed pull replaced local file");
            Assert(!Directory.EnumerateFiles(directory, "*.matrix-part-*").Any(), "Partial files leaked");
            server.TruncateStat = true;
            await Expect(503, () => backend.PullAsync("allowed", "/sdcard/test.bin", download));
            server.TruncateStat = false; server.TruncatePull = false; server.EarlyDone = true;
            await Expect(502, () => backend.PullAsync("allowed", "/sdcard/test.bin", download));
            Assert((await File.ReadAllBytesAsync(download)).SequenceEqual(bytes) && !Directory.EnumerateFiles(directory, "*.matrix-part-*").Any(), "Early DONE replaced or leaked local data.");
        });
    }

    private static async Task ProcessReplacementAsync()
    {
        var directory = Path.Combine(Path.GetTempPath(), "MatrixDevice.Tests", Guid.NewGuid().ToString("N")); Directory.CreateDirectory(directory);
        async Task<Process> Start(int port, string receipt)
        {
            var info = new ProcessStartInfo(Environment.ProcessPath!) { UseShellExecute = false, CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden, RedirectStandardOutput = true, RedirectStandardError = true };
            if (string.Equals(Path.GetFileNameWithoutExtension(Environment.ProcessPath), "dotnet", StringComparison.OrdinalIgnoreCase)) info.ArgumentList.Add(typeof(ExistingAdbTests).Assembly.Location);
            foreach (var value in new[] { "--adb-owned-test-server", port.ToString(CultureInfo.InvariantCulture), receipt }) info.ArgumentList.Add(value);
            var child = Process.Start(info)!;
            try { var ready = await child.StandardOutput.ReadLineAsync().WaitAsync(TimeSpan.FromSeconds(10)); Assert(int.TryParse(ready, out var actual) && actual == port, "Owned fixture did not become ready."); return child; }
            catch { if (!child.HasExited) child.Kill(false); await child.WaitForExitAsync(); child.Dispose(); throw; }
        }
        async Task Stop(Process child) { if (!child.HasExited) child.Kill(false); await child.WaitForExitAsync(); child.Dispose(); }
        var listener = new TcpListener(IPAddress.Loopback, 0); listener.Start(); var port = ((IPEndPoint)listener.LocalEndpoint).Port; listener.Stop();
        var first = await Start(port, Path.Combine(directory, "first.txt")); Process? second = null;
        try
        {
            await using var old = new AdbDeviceBackend(new AppSettings { AdbAllowedSerials = ["allowed"] }, new(IPAddress.Loopback, port));
            await old.DiscoverAsync(); await Stop(first); first = null!;
            var receipt = Path.Combine(directory, "second.txt"); second = await Start(port, receipt);
            await Expect(409, () => old.KeyEventAsync("allowed", 3)); await Expect(409, () => old.DiscoverAsync());
            Assert(!File.Exists(receipt) || new FileInfo(receipt).Length == 0, "Replacement server received a request from the old client.");
            await using var fresh = new AdbDeviceBackend(new AppSettings { AdbAllowedSerials = ["allowed"] }, new(IPAddress.Loopback, port));
            Assert((await fresh.DiscoverAsync()).Single().Online, "Fresh discovery could not establish a new epoch.");
        }
        finally { if (first is not null) await Stop(first); if (second is not null) await Stop(second); }
    }
    private static async Task InvalidIdentityAsync()
    {
        await using var server = new FakeAdbServer();
        await using var backend = new AdbDeviceBackend(new AppSettings { AdbAllowedSerials = ["allowed"] }, server.Endpoint);
        var handler = new CaptureHandler();
        backend.RegisterAgent("allowed", new PhoneAgentClient("http://127.0.0.1:9999", handler));
        await Expect(503, () => backend.KeyEventAsync("allowed", 3));
        await Expect(503, () => backend.SetNodeTextAsync("allowed", new(), "text"));
        Assert(server.Requests.Count == 0 && handler.Path.Length == 0, "Cold guest action performed I/O");
        foreach (var identity in new[] { "", "0", "4294967296", "invalid", "offline", "absent" })
        {
            server.InvalidIdentity = null; await backend.DiscoverAsync();
            server.InvalidIdentity = identity;
            var devices = await backend.DiscoverAsync();
            Assert(devices.All(device => !device.Online), "Invalid identity remained Ready");
            var requests = server.Requests.Count;
            await Expect(503, () => backend.KeyEventAsync("allowed", 3));
            await Expect(503, () => backend.SetNodeTextAsync("allowed", new(), "text"));
            Assert(server.Requests.Count == requests && handler.Path.Length == 0, "Invalidated guest identity performed I/O");
        }
    }
    private static async Task ReconnectAsync()
    {
        await using var server = new FakeAdbServer();
        await using var backend = new AdbDeviceBackend(new AppSettings { AdbAllowedSerials = ["allowed"] }, server.Endpoint);
        var catalog = new DeviceCatalog(backend); await catalog.RefreshAsync();
        var generation = catalog.GetRequired("allowed").Generation;
        var blocked = backend.ShellAsync("allowed", "gated"); await server.GateEntered.Task.WaitAsync(TimeSpan.FromSeconds(2));
        var key = backend.KeyEventAsync("allowed", 3);
        server.TransportId = 2;
        var refresh = catalog.RefreshAsync();
        try
        {
            var timer = System.Diagnostics.Stopwatch.StartNew();
            while (backend.ObservedTransportId("allowed") != 2)
            {
                if (timer.Elapsed > TimeSpan.FromSeconds(2)) throw new TimeoutException("New transport not observed");
                await Task.Delay(5);
            }
        }
        finally { server.GateRelease.TrySetResult(); }
        await blocked;
        await Expect(409, () => key);
        await refresh;
        Assert(catalog.GetRequired("allowed").Generation > generation, "Transport reconnect retained generation");
        Assert(!server.Requests.Any(request => request.Contains("input keyevent", StringComparison.Ordinal) || request.StartsWith("host:transport:", StringComparison.Ordinal)), "Stale input or serial fallback emitted");
    }
    private static void Assert(bool value, string message) { if (!value) throw new Exception(message); }
    private static async Task Expect(int status, Func<Task> action)
    {
        try { await action(); }
        catch (DeviceException ex) when (ex.Status == status) { return; }
        throw new Exception($"Expected DeviceException status={status}");
    }
}

internal sealed class FakeAdbServer : IAsyncDisposable
{
    private readonly TcpListener listener;
    private readonly string? receipt;
    private readonly CancellationTokenSource stop = new();
    private readonly ConcurrentBag<Task> clients = [];
    private readonly Task accepting;
    public ConcurrentQueue<string> Requests { get; } = [];
    public string Version { get; set; } = "0029";
    public string LaunchOutput { get; set; } = "Starting: Intent\nStatus: ok\n";
    public uint TransportId { get; set; } = 1;
    public string? InvalidIdentity { get; set; }
    public TaskCompletionSource GateEntered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource GateRelease { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public bool TruncatePull { get; set; }
    public bool TruncateStat { get; set; }
    public bool EarlyDone { get; set; }
    public byte[] Png { get; } = PngImage.Create(4, 3, (x, y) => ((byte)(x * 20), (byte)(y * 40), (byte)90));
    private byte[] file = [];
    public IPEndPoint Endpoint => (IPEndPoint)listener.LocalEndpoint;
    public FakeAdbServer(int port = 0, string? receipt = null) { this.receipt = receipt; listener = new(IPAddress.Loopback, port); listener.Start(); accepting = AcceptAsync(); }
    private async Task AcceptAsync()
    {
        try
        {
            while (!stop.IsCancellationRequested)
            {
                var client = await listener.AcceptTcpClientAsync(stop.Token);
                clients.Add(HandleAsync(client));
            }
        }
        catch (OperationCanceledException) { }
        catch (ObjectDisposedException) when (stop.IsCancellationRequested) { }
    }
    private async Task HandleAsync(TcpClient client)
    {
        using (client)
        {
            try
            {
                var stream = client.GetStream();
                while (!stop.IsCancellationRequested)
                {
                    var prefix = new byte[4]; await stream.ReadExactlyAsync(prefix, stop.Token);
                    var length = int.Parse(Encoding.ASCII.GetString(prefix), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                    var body = new byte[length]; await stream.ReadExactlyAsync(body, stop.Token);
                    var request = Encoding.UTF8.GetString(body); Requests.Enqueue(request);
                    if (receipt is not null) await File.AppendAllTextAsync(receipt, request + "\n", stop.Token);
                    if (request == "host:version") { await HostAsync(stream, Version); return; }
                    if (request == "host:devices-l")
                    {
                        var allowed = InvalidIdentity == "absent" ? "" :
                            $"allowed\t{(InvalidIdentity == "offline" ? "offline" : "device")} model:Test" +
                            (InvalidIdentity == "" ? "" : " transport_id:" + (InvalidIdentity is null or "offline" ? TransportId.ToString(CultureInfo.InvariantCulture) : InvalidIdentity)) + "\n";
                        await HostAsync(stream, allowed + "private\tdevice model:Private transport_id:99\n"); return;
                    }
                    if (request.StartsWith("host:transport-id:", StringComparison.Ordinal)) { await stream.WriteAsync("OKAY"u8.ToArray(), stop.Token); continue; }
                    await stream.WriteAsync("OKAY"u8.ToArray(), stop.Token);
                    if (request == "sync:") { await SyncAsync(stream); return; }
                    if (request.StartsWith("exec:", StringComparison.Ordinal)) { await stream.WriteAsync(Png, stop.Token); return; }
                    if (!request.StartsWith("shell,v2,raw:", StringComparison.Ordinal)) throw new IOException("Unexpected fixture request: " + request);
                    var closeInput = new byte[5]; await stream.ReadExactlyAsync(closeInput, stop.Token);
                    var command = request[13..];
                    if (command == "gated") { GateEntered.TrySetResult(); await GateRelease.Task.WaitAsync(stop.Token); }
                    if (command == "blocked") { await Task.Delay(Timeout.Infinite, stop.Token); return; }
                    if (command == "truncated") { await stream.WriteAsync(new byte[] { 1, 20, 0, 0, 0, 1 }, stop.Token); return; }
                    if (command == "huge") { var header = new byte[5]; header[0] = 1; BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(1), AdbDeviceBackend.MaximumStdoutBytes + 1); await stream.WriteAsync(header, stop.Token); return; }
                    var text = command.StartsWith("cmd package resolve-activity ", StringComparison.Ordinal) ? "priority=0 isDefault=false\ncom.matrix.test/.LauncherAlias\n" : command.StartsWith("am start -W ", StringComparison.Ordinal) ? LaunchOutput : command switch { "getprop" => "[ro.product.brand]: [Fixture]\n[ro.product.model]: [Test]\n[ro.build.version.release]: [14]\n", "wm size" => "Physical size: 4x3\n", "dumpsys input" => "SurfaceOrientation: 0\n", _ => "fixture\n" };
                    await ShellPacketAsync(stream, command == "fail" ? (byte)2 : (byte)1, Encoding.UTF8.GetBytes(text));
                    await ShellPacketAsync(stream, 3, [command == "fail" ? (byte)7 : (byte)0]);
                    return;
                }
            }
            catch (Exception ex) when (ex is IOException or SocketException or OperationCanceledException) { }
        }
    }
    private async Task HostAsync(Stream stream, string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        await stream.WriteAsync(Encoding.ASCII.GetBytes("OKAY" + bytes.Length.ToString("X4", CultureInfo.InvariantCulture)), stop.Token);
        await stream.WriteAsync(bytes, stop.Token);
    }
    private async Task ShellPacketAsync(Stream stream, byte channel, byte[] value)
    {
        var header = new byte[5]; header[0] = channel; BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(1), value.Length);
        await stream.WriteAsync(header, stop.Token); await stream.WriteAsync(value, stop.Token);
    }
    private async Task SyncAsync(Stream stream)
    {
        using var upload = new MemoryStream();
        while (!stop.IsCancellationRequested)
        {
            var header = new byte[8]; await stream.ReadExactlyAsync(header, stop.Token);
            var command = Encoding.ASCII.GetString(header, 0, 4); var length = BinaryPrimitives.ReadInt32LittleEndian(header.AsSpan(4));
            if (command == "DONE") { file = upload.ToArray(); await SyncPacketAsync(stream, "OKAY", []); continue; }
            if (command == "QUIT") return;
            var bytes = new byte[length]; await stream.ReadExactlyAsync(bytes, stop.Token);
            if (command == "DATA") { await upload.WriteAsync(bytes, stop.Token); continue; }
            if (command == "SEND") continue;
            if (command == "STAT")
            {
                if (TruncateStat) { await stream.WriteAsync("STAT"u8.ToArray(), stop.Token); return; }
                var response = new byte[16]; "STAT"u8.CopyTo(response); BinaryPrimitives.WriteInt32LittleEndian(response.AsSpan(4), 33206); BinaryPrimitives.WriteInt32LittleEndian(response.AsSpan(8), file.Length);
                await stream.WriteAsync(response, stop.Token); continue;
            }
            if (command == "RECV")
            {
                if (EarlyDone) { await SyncPacketAsync(stream, "DATA", file.Take(100).ToArray()); await SyncPacketAsync(stream, "DONE", []); continue; }
                for (var offset = 0; offset < file.Length; offset += 65536)
                    await SyncPacketAsync(stream, "DATA", file.AsSpan(offset, Math.Min(65536, file.Length - offset)).ToArray());
                if (TruncatePull) { await stream.WriteAsync("DONE"u8.ToArray(), stop.Token); return; }
                await SyncPacketAsync(stream, "DONE", []); continue;
            }
            throw new IOException("Unexpected sync command: " + command);
        }
    }
    private async Task SyncPacketAsync(Stream stream, string command, byte[] value)
    {
        var header = new byte[8]; Encoding.ASCII.GetBytes(command).CopyTo(header, 0); BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(4), value.Length);
        await stream.WriteAsync(header, stop.Token); await stream.WriteAsync(value, stop.Token);
    }
    public async ValueTask DisposeAsync()
    {
        stop.Cancel(); listener.Stop(); await accepting;
        await Task.WhenAll(clients); stop.Dispose();
    }
}
