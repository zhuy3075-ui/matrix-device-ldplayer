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
