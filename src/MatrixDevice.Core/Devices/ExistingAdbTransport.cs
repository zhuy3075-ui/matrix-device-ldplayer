using System.Buffers.Binary;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using AdvancedSharpAdbClient;
using AdvancedSharpAdbClient.Exceptions;
using AdvancedSharpAdbClient.Models;

namespace MatrixDevice.Core;

internal sealed class ExistingAdbTransport(AppSettings settings, IPEndPoint endpoint)
{
    private readonly HashSet<string> allowedSerials = settings.AdbAllowedSerials.ToHashSet(StringComparer.Ordinal);
    private readonly object transportGate = new();
    private readonly AdbServerEpoch serverEpoch = new();
    private Dictionary<string, uint> transports = new(StringComparer.Ordinal);
    private readonly AsyncLocal<uint?> pinnedTransport = new();
    public void InheritServerEpoch(ExistingAdbTransport source) => serverEpoch.Require(source.serverEpoch.Snapshot);
    public uint? CurrentTransport(string serial) { lock (transportGate) return transports.TryGetValue(serial, out var id) ? id : null; }
    public void ReplaceTransports(Dictionary<string, uint> current) { lock (transportGate) transports = current; }
    public void InvalidateTransport(string serial) { lock (transportGate) transports.Remove(serial); }
    public void RequireTransport(string serial, uint? expected)
    {
        if (expected != CurrentTransport(serial)) throw new DeviceException($"ADB transport changed while queued: serial={serial}, expected={expected}, actual={CurrentTransport(serial)}.", 409, 1003);
    }
    public async Task<T> RunPinnedAsync<T>(string serial, uint? expected, Func<CancellationToken, Task<T>> action, CancellationToken ct)
    {
        RequireTransport(serial, expected);
        var previous = pinnedTransport.Value;
        pinnedTransport.Value = expected;
        try { return await action(ct).ConfigureAwait(false); }
        finally { pinnedTransport.Value = previous; }
    }
    public async Task<byte[]> ExecuteAsync(string? serial, string[] args, CancellationToken ct)
    {
        if (serial is not null && !allowedSerials.Contains(serial))
            throw new DeviceException($"ADB serial is not authorized: {serial}.", 403, 1007);
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(ct);
        deadline.CancelAfter(TimeSpan.FromSeconds(Math.Clamp(settings.CommandTimeoutSeconds, 1, 600)));
        try
        {
            var token = deadline.Token;
            await CheckVersionAsync(token).ConfigureAwait(false);
            using var socket = await OpenVerifiedAsync(token).ConfigureAwait(false);
            using var cancellation = token.Register(socket.Dispose);
            if (serial is null)
            {
                var request = args is ["devices", "-l"] ? "host:devices-l" :
                    args is ["connect", var address] && allowedSerials.Contains(address) ? "host:connect:" + address :
                    throw new DeviceException($"Unsupported or unauthorized ADB host operation: {string.Join(' ', args)}.", 403, 1007);
                await socket.SendAdbRequestAsync(request, token).ConfigureAwait(false);
                await socket.ReadAdbResponseAsync(token).ConfigureAwait(false);
                var text = await socket.ReadStringAsync(token).ConfigureAwait(false);
                return Encoding.UTF8.GetBytes(args[0] == "devices" ? "List of devices attached\n" + text : text);
            }
            var transport = pinnedTransport.Value ?? CurrentTransport(serial) ?? throw new DeviceException($"ADB transport has not been discovered: {serial}.", 503, 2000);
            RequireTransport(serial, transport);
            var device = new DeviceData { Serial = serial, TransportId = transport.ToString(CultureInfo.InvariantCulture) };
            if (args[0] is "push" or "pull")
            {
                // SyncService opens synchronously; cancellation closes only this connection, never the server.
                return await Task.Run(async () =>
                {
                    using var sync = new SyncService(socket, device);
                    if (args[0] == "push")
                    {
                        using var file = File.OpenRead(args[1]);
                        await sync.PushAsync(file, args[2], UnixFileStatus.DefaultFileMode, DateTimeOffset.UtcNow, null, false, token).ConfigureAwait(false);
                    }
                    else
                    {
                        var part = args[2] + ".matrix-part-" + Guid.NewGuid().ToString("N");
                        Exception? failure = null;
                        try
                        {
                            var expected = await sync.StatAsync(args[1], token).ConfigureAwait(false);
                            using (var file = File.Create(part))
                            {
                                await sync.PullAsync(args[1], file, null, false, token).ConfigureAwait(false);
                                if (file.Length != expected.Size) throw new DeviceException($"ADB pull byte count mismatch: expected={expected.Size}, actual={file.Length}, remote={args[1]}.", 502, 2000);
                            }
                            var after = await sync.StatAsync(args[1], token).ConfigureAwait(false);
                            if (after.Size != expected.Size || after.Time != expected.Time) throw new DeviceException($"Remote file changed during download: {args[1]}; local file left untouched.", 409, 1003);
                            token.ThrowIfCancellationRequested();
                            File.Move(part, args[2], overwrite: true);
                        }
                        catch (Exception ex) { failure = ex; throw; }
                        finally
                        {
                            try { if (File.Exists(part)) File.Delete(part); }
                            catch (Exception ex) when (failure is not null && ex is IOException or UnauthorizedAccessException) { failure.Data["PartialFileCleanupError"] = $"{part}: {ex.Message}"; }
                        }
                    }
                    return Array.Empty<byte>();
                }, token).ConfigureAwait(false);
            }
            await socket.SetDeviceAsync(device, token).ConfigureAwait(false);
            if (args[0] == "install")
            {
                using var apk = File.OpenRead(args[^1]);
                await socket.SendAdbRequestAsync("exec:cmd package install -r -S " + apk.Length.ToString(CultureInfo.InvariantCulture), token).ConfigureAwait(false);
                await socket.ReadAdbResponseAsync(token).ConfigureAwait(false);
                var block = new byte[64 * 1024];
                int count;
                while ((count = await apk.ReadAsync(block, token).ConfigureAwait(false)) > 0)
                    await socket.SendAsync(block, count, token).ConfigureAwait(false);
                using var stream = socket.Socket.GetStream();
                return await BoundedStreamRead.ReadAsync(stream, AdbDeviceBackend.MaximumStderrBytes, token).ConfigureAwait(false);
            }
            if (args[0] == "exec-out")
            {
                await socket.SendAdbRequestAsync("exec:" + string.Join(' ', args.Skip(1).Select(AdbDeviceBackend.Quote)), token).ConfigureAwait(false);
                await socket.ReadAdbResponseAsync(token).ConfigureAwait(false);
                // GetShellStream normalizes CRLF and would corrupt binary PNG data.
                using var stream = socket.Socket.GetStream();
                return await BoundedStreamRead.ReadAsync(stream, AdbDeviceBackend.MaximumStdoutBytes, token).ConfigureAwait(false);
            }
            var command = args[0] switch
            {
                "shell" => string.Join(' ', args.Skip(1)),
                "uninstall" => "pm uninstall " + AdbDeviceBackend.Quote(args[1]),
                _ => throw new DeviceException($"Unsupported ADB operation: {args[0]}.", 400, 1007)
            };
            await socket.SendAdbRequestAsync("shell,v2,raw:" + command, token).ConfigureAwait(false);
            await socket.ReadAdbResponseAsync(token).ConfigureAwait(false);
            await socket.SendAsync(new byte[] { 4, 0, 0, 0, 0 }, token).ConfigureAwait(false);
            using var outputStream = socket.Socket.GetStream();
            return await ReadShellAsync(outputStream, command, token).ConfigureAwait(false);
        }
        catch (Exception) when (deadline.IsCancellationRequested)
        {
            ct.ThrowIfCancellationRequested();
            throw new DeviceException($"ADB command timed out: serial={serial ?? "host"}, operation={args[0]}.", 504, 2000);
        }
        catch (AggregateException ex) when (ex.GetBaseException() is DeviceException)
        {
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex.GetBaseException()).Throw();
            throw;
        }
        catch (Exception ex) when (ex.GetBaseException() is SocketException or IOException or AdbException or ObjectDisposedException)
        {
            throw new DeviceException($"Existing ADB server request failed: serial={serial ?? "host"}, operation={args[0]}: {ex.Message}", 503, 2000);
        }
    }

    public async Task StreamScreenVideoAsync(string serial, uint expectedTransport, string remoteServer, string scid, int maximumSize, Stream output, Func<CancellationToken, Task> verifyIdentity, CancellationToken ct)
    {
        if (!allowedSerials.Contains(serial)) throw new DeviceException($"ADB serial is not authorized: {serial}.", 403, 1007);
        try
        {
            RequireTransport(serial, expectedTransport);
            await CheckVersionAsync(ct).ConfigureAwait(false);
            using var shell = await OpenVerifiedAsync(ct).ConfigureAwait(false);
            using var serverStop = new CancellationTokenSource();
            using var shellCancellation = serverStop.Token.Register(shell.Dispose);
            var device = new DeviceData { Serial = serial, TransportId = expectedTransport.ToString(CultureInfo.InvariantCulture) };
            await shell.SetDeviceAsync(device, ct).ConfigureAwait(false);
            var nonce = Guid.NewGuid().ToString("N");
            var marker = "MATRIX_VIDEO_" + nonce + ":";
            var command = FormattableString.Invariant($"printf '{marker}%s\\n' $$; cat /proc/$$/stat; CLASSPATH={remoteServer} exec app_process -Dmatrix.video.session={nonce} / com.genymobile.scrcpy.Server 3.3.4 scid={scid} tunnel_forward=true video=true audio=false control=false raw_stream=true cleanup=false video_codec=h264 max_size={maximumSize} max_fps=10 video_bit_rate=1000000 power_on=false clipboard_autosync=false power_off_on_close=false");
            await shell.SendAdbRequestAsync("shell,v2,raw:" + command, ct).ConfigureAwait(false);
            await shell.ReadAdbResponseAsync(ct).ConfigureAwait(false);
            await shell.SendAsync(new byte[] { 4, 0, 0, 0, 0 }, ct).ConfigureAwait(false);
            using var shellStream = shell.Socket.GetStream();
            var log = new VideoServerLog(marker);
            var server = ReadVideoServerAsync(shellStream, log, serverStop.Token);
            ExistingAdbSocket? video = null;
            Task? copy = null;
            using var copying = CancellationTokenSource.CreateLinkedTokenSource(ct);
            try
            {
                using var startup = CancellationTokenSource.CreateLinkedTokenSource(ct);
                startup.CancelAfter(TimeSpan.FromSeconds(10));
                while (video is null)
                {
                    startup.Token.ThrowIfCancellationRequested();
                    if (server.IsCompleted) throw new DeviceException($"Video server exited before opening its socket: serial={serial}, {log.Snapshot}", 501, 2001);
                    RequireTransport(serial, expectedTransport);
                    var candidate = await OpenVerifiedAsync(startup.Token).ConfigureAwait(false);
                    try
                    {
                        await candidate.SetDeviceAsync(device, startup.Token).ConfigureAwait(false);
                        await candidate.SendAdbRequestAsync("localabstract:scrcpy_" + scid, startup.Token).ConfigureAwait(false);
                        await candidate.ReadAdbResponseAsync(startup.Token).ConfigureAwait(false);
                        video = candidate;
                    }
                    catch (AdbException ex) when (IsSocketNotReady(ex))
                    {
                        candidate.Dispose();
                        await Task.Delay(100, startup.Token).ConfigureAwait(false);
                    }
                    catch { candidate.Dispose(); throw; }
                }
                using var cancellation = copying.Token.Register(video.Dispose);
                using var stream = video.Socket.GetStream();
                copy = CopyH264Async(stream, output, copying.Token);
                var completed = await Task.WhenAny(copy, server).ConfigureAwait(false);
                if (completed == server)
                {
                    var code = await server.ConfigureAwait(false);
                    throw new DeviceException($"Video server exited: serial={serial}, code={code}, {log.Snapshot}", 503, 2000);
                }
                await copy.ConfigureAwait(false);
                throw new DeviceException($"Video connection closed: serial={serial}, {log.Snapshot}", 503, 2000);
            }
            finally
            {
                await copying.CancelAsync().ConfigureAwait(false);
                video?.Dispose();
                if (copy is not null)
                {
                    try { await copy.ConfigureAwait(false); }
                    catch (Exception ex) when (ex is IOException or DeviceException or OperationCanceledException or ObjectDisposedException) { }
                }
                try
                {
                    // First let normal socket disconnection terminate the encoder. Kill only a proven owned PID if it stalls.
                    try { await server.WaitAsync(TimeSpan.FromSeconds(3)).ConfigureAwait(false); }
                    catch (Exception ex) when (ex is IOException or OperationCanceledException or TimeoutException)
                    {
                        try { await StopOwnedVideoServerAsync(serial, expectedTransport, scid, nonce, log, verifyIdentity).ConfigureAwait(false); }
                        catch (Exception cleanupError)
                        {
                            var unconfirmed = new DeviceException($"视频服务退出清理未确认，已禁止本实例继续叠加会话：serial={serial}, jar={remoteServer}: {cleanupError.Message}", 409, 1003);
                            unconfirmed.Data["VideoCleanupUnconfirmed"] = true;
                            throw unconfirmed;
                        }
                    }
                }
                finally
                {
                    await serverStop.CancelAsync().ConfigureAwait(false);
                    try { await server.ConfigureAwait(false); }
                    catch (Exception ex) when (serverStop.IsCancellationRequested && ex is IOException or OperationCanceledException or ObjectDisposedException) { }
                }
            }
        }
        catch (Exception ex) when (ct.IsCancellationRequested && ex is not DeviceException) { throw new OperationCanceledException(ct); }
        catch (Exception ex) when (ex.GetBaseException() is SocketException or IOException or AdbException or ObjectDisposedException)
        {
            throw new DeviceException($"Existing ADB video stream failed: serial={serial}: {ex.Message}", 503, 2000);
        }
    }

    private async Task StopOwnedVideoServerAsync(string serial, uint transport, string scid, string nonce, VideoServerLog log, Func<CancellationToken, Task> verifyIdentity)
    {
        using var cleanup = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var ct = cleanup.Token;
        if (log.Identity is not { } identity) throw new DeviceException($"视频服务退出未确认，未获得本次进程出生证明；请停止重连并检查实例：serial={serial}。", 409, 1003);
        await verifyIdentity(ct).ConfigureAwait(false);
        await RunPinnedAsync(serial, transport, async token =>
        {
            var snapshot = Encoding.UTF8.GetString(await ExecuteAsync(serial, ["shell", $"if [ -e /proc/{identity.ProcessId}/stat ]; then cat /proc/{identity.ProcessId}/stat; printf '\\n'; cat /proc/{identity.ProcessId}/cmdline; fi"], token).ConfigureAwait(false));
            if (snapshot.Length == 0) return true;
            var lineEnd = snapshot.IndexOf('\n');
            if (lineEnd < 0 || ParseProcessIdentity(snapshot[..lineEnd]) != identity)
                throw new DeviceException($"视频服务 PID 已变化，拒绝清理其他进程：serial={serial}, pid={identity.ProcessId}。", 409, 1003);
            var commandLine = snapshot[(lineEnd + 1)..];
            var arguments = commandLine.Split('\0', StringSplitOptions.RemoveEmptyEntries);
            if (!arguments.Contains("-Dmatrix.video.session=" + nonce, StringComparer.Ordinal) ||
                !arguments.Contains("com.genymobile.scrcpy.Server", StringComparer.Ordinal) ||
                !arguments.Contains("3.3.4", StringComparer.Ordinal) || !arguments.Contains("scid=" + scid, StringComparer.Ordinal))
                throw new DeviceException($"无法核验本次视频进程命令行，拒绝终止：serial={serial}, pid={identity.ProcessId}, scid={scid}。", 409, 1003);
            // Recheck birth immediately in the same shell before signalling this numeric PID.
            var kill = FormattableString.Invariant($"record=$(cat /proc/{identity.ProcessId}/stat) || exit 41; fields=${{record##*) }}; set -- $fields; shift 19; [ \"$1\" = \"{identity.StartedTicks}\" ] || exit 42; kill -TERM {identity.ProcessId}");
            await ExecuteAsync(serial, ["shell", kill], token).ConfigureAwait(false);
            for (var attempt = 0; attempt < 10; attempt++)
            {
                var after = Encoding.UTF8.GetString(await ExecuteAsync(serial, ["shell", $"if [ -e /proc/{identity.ProcessId}/stat ]; then cat /proc/{identity.ProcessId}/stat; fi"], token).ConfigureAwait(false)).Trim();
                if (after.Length == 0 || ParseProcessIdentity(after) != identity) return true;
                await Task.Delay(100, token).ConfigureAwait(false);
            }
            throw new DeviceException($"本次视频进程终止未确认，请停止重连并检查实例：serial={serial}, pid={identity.ProcessId}。", 409, 1003);
        }, ct).ConfigureAwait(false);
    }

    private static VideoProcessIdentity? ParseProcessIdentity(string stat)
    {
        var open = stat.IndexOf('('); var close = stat.LastIndexOf(')');
        if (open < 1 || close <= open || !int.TryParse(stat.AsSpan(0, open).Trim(), out var pid) || pid < 1) return null;
        var fields = stat[(close + 1)..].Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return fields.Length > 19 && ulong.TryParse(fields[19], NumberStyles.None, CultureInfo.InvariantCulture, out var ticks) && ticks > 0 ? new(pid, ticks) : null;
    }

    private sealed record VideoProcessIdentity(int ProcessId, ulong StartedTicks);

    private static bool IsSocketNotReady(AdbException error) =>
        error.Message.Contains("connect", StringComparison.OrdinalIgnoreCase) ||
        error.Message.Contains("closed", StringComparison.OrdinalIgnoreCase) ||
        error.Message.Contains("not found", StringComparison.OrdinalIgnoreCase) ||
        error.Message.Contains("No such file", StringComparison.OrdinalIgnoreCase);

    private static async Task CopyH264Async(Stream stream, Stream output, CancellationToken ct)
    {
        var prefix = new byte[512];
        var count = 0;
        while (count < 5)
        {
            var read = await stream.ReadAsync(prefix.AsMemory(count), ct).ConfigureAwait(false);
            if (read == 0) break;
            count += read;
        }
        var start = count >= 5 && prefix[0] == 0 && prefix[1] == 0
            ? prefix[2] == 1 ? 3 : prefix[2] == 0 && prefix[3] == 1 ? 4 : -1 : -1;
        if (start < 0 || (prefix[start] & 0x1f) is < 1 or > 12)
            throw new DeviceException("Video server did not produce raw H.264 Annex-B data.", 501, 2001);
        await output.WriteAsync(prefix.AsMemory(0, count), ct).ConfigureAwait(false);
        await stream.CopyToAsync(output, 64 * 1024, ct).ConfigureAwait(false);
    }

    private static async Task<int> ReadVideoServerAsync(Stream stream, VideoServerLog log, CancellationToken ct)
    {
        var header = new byte[5];
        var block = new byte[4096];
        while (true)
        {
            await stream.ReadExactlyAsync(header, ct).ConfigureAwait(false);
            var remaining = BinaryPrimitives.ReadUInt32LittleEndian(header.AsSpan(1));
            if (header[0] is not (1 or 2 or 3) || (header[0] == 3 && remaining != 1)) throw new IOException("Malformed video server shell packet.");
            if (header[0] == 3) { await stream.ReadExactlyAsync(block.AsMemory(0, 1), ct).ConfigureAwait(false); return block[0]; }
            while (remaining > 0)
            {
                var length = (int)Math.Min(remaining, (uint)block.Length);
                await stream.ReadExactlyAsync(block.AsMemory(0, length), ct).ConfigureAwait(false);
                log.Append(Encoding.UTF8.GetString(block, 0, length));
                remaining -= (uint)length;
            }
        }
    }

    private sealed class VideoServerLog(string marker)
    {
        private readonly StringBuilder text = new();
        private VideoProcessIdentity? identity;
        public VideoProcessIdentity? Identity { get { lock (text) return identity; } }
        public string Snapshot { get { lock (text) return text.ToString(); } }
        public void Append(string value)
        {
            lock (text)
            {
                text.Append(value);
                if (identity is null)
                {
                    var snapshot = text.ToString(); var start = snapshot.IndexOf(marker, StringComparison.Ordinal);
                    if (start >= 0)
                    {
                        var pidEnd = snapshot.IndexOf('\n', start); var statEnd = pidEnd < 0 ? -1 : snapshot.IndexOf('\n', pidEnd + 1);
                        if (statEnd > pidEnd && int.TryParse(snapshot.AsSpan(start + marker.Length, pidEnd - start - marker.Length).Trim(), out var pid) && ParseProcessIdentity(snapshot[(pidEnd + 1)..statEnd]) is { } parsed && parsed.ProcessId == pid)
                            identity = parsed;
                    }
                }
                if (text.Length > 8192) text.Remove(0, text.Length - 8192);
            }
        }
    }

    private async Task CheckVersionAsync(CancellationToken ct)
    {
        using var socket = await OpenVerifiedAsync(ct).ConfigureAwait(false);
        using var cancellation = ct.Register(socket.Dispose);
        await socket.SendAdbRequestAsync("host:version", ct).ConfigureAwait(false);
        await socket.ReadAdbResponseAsync(ct).ConfigureAwait(false);
        var version = await socket.ReadStringAsync(ct).ConfigureAwait(false);
        if (!int.TryParse(version, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var number) || number != 41)
            throw new DeviceException($"Unsupported existing ADB server protocol: {version}; expected 0029. Server left untouched.", 503, 2000);
    }

    private async Task<ExistingAdbSocket> OpenVerifiedAsync(CancellationToken ct)
    {
        var socket = await ExistingAdbSocket.OpenAsync(endpoint, ct).ConfigureAwait(false);
        try
        {
            ct.ThrowIfCancellationRequested();
            serverEpoch.Require(AdbServerIdentity.ForConnection(socket.ConnectedSocket));
            return socket;
        }
        catch { socket.Dispose(); throw; }
    }

    private static async Task<byte[]> ReadShellAsync(Stream stream, string command, CancellationToken ct)
    {
        using var stdout = new MemoryStream();
        using var stderr = new MemoryStream();
        var header = new byte[5];
        while (true)
        {
            await stream.ReadExactlyAsync(header, ct).ConfigureAwait(false);
            var length = BinaryPrimitives.ReadUInt32LittleEndian(header.AsSpan(1));
            if (header[0] is not (1 or 2 or 3) || (header[0] == 3 && length != 1)) throw new IOException($"Malformed ADB shell packet: channel={header[0]}, length={length}.");
            var limit = header[0] == 2 ? AdbDeviceBackend.MaximumStderrBytes : AdbDeviceBackend.MaximumStdoutBytes;
            if (length > limit) throw new DeviceException($"ADB shell output too large: {command}.", 413, 1007);
            var payload = new byte[(int)length];
            await stream.ReadExactlyAsync(payload, ct).ConfigureAwait(false);
            if (header[0] == 3)
            {
                if (payload.Length != 1) throw new IOException("Malformed ADB shell exit packet.");
                if (payload[0] != 0) throw new DeviceException($"ADB shell failed: exit={payload[0]}, command={command}, stderr={Encoding.UTF8.GetString(stderr.ToArray())}", 503, 2000);
                return stdout.ToArray();
            }
            var target = header[0] switch { 1 => stdout, 2 => stderr, _ => throw new IOException($"Unexpected ADB shell channel: {header[0]}.") };
            if (target.Length + payload.Length > limit) throw new DeviceException($"ADB shell output too large: {command}.", 413, 1007);
            await target.WriteAsync(payload, ct).ConfigureAwait(false);
        }
    }
}
