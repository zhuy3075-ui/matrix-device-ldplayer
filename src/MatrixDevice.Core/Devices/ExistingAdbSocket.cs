using System.Net;
using System.Net.Sockets;
using System.Buffers.Binary;
using System.Text;
using AdvancedSharpAdbClient;
using AdvancedSharpAdbClient.Models;

namespace MatrixDevice.Core;

// The library's single-send methods permit short writes. Complete each owned write before returning.
internal sealed class ExistingAdbSocket(TcpSocket tcp) : AdbSocket(tcp), IAdbSocket
{
    internal System.Net.Sockets.Socket ConnectedSocket => tcp.Socket;
    public static async Task<ExistingAdbSocket> OpenAsync(IPEndPoint endpoint, CancellationToken ct)
    {
        var tcp = new TcpSocket();
        try
        {
            await tcp.ConnectAsync(endpoint, ct).ConfigureAwait(false);
            return new ExistingAdbSocket(tcp);
        }
        catch { tcp.Dispose(); throw; }
    }

    public override void Send(ReadOnlySpan<byte> data)
    {
        while (!data.IsEmpty)
        {
            var written = tcp.Socket.Send(data, SocketFlags.None);
            if (written <= 0) throw new IOException("ADB socket closed during send.");
            data = data[written..];
        }
    }
    public override void Send(byte[] data) => Send(data.AsSpan());
    public override void Send(byte[] data, int length) => Send(data, 0, length);
    public override void Send(byte[] data, int offset, int length) => Send(data.AsSpan(offset, length == -1 ? data.Length - offset : length));
    public override async ValueTask SendAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        while (!data.IsEmpty)
        {
            var written = await tcp.Socket.SendAsync(data, SocketFlags.None, ct).ConfigureAwait(false);
            if (written <= 0) throw new IOException("ADB socket closed during send.");
            data = data[written..];
        }
    }
    public override Task SendAsync(byte[] data, CancellationToken ct = default) => SendAsync(data.AsMemory(), ct).AsTask();
    public override Task SendAsync(byte[] data, int length, CancellationToken ct = default) => SendAsync(data, 0, length, ct);
    public override Task SendAsync(byte[] data, int offset, int length, CancellationToken ct = default) => SendAsync(data.AsMemory(offset, length == -1 ? data.Length - offset : length), ct).AsTask();

    public override int Read(Span<byte> data)
    {
        var length = data.Length;
        while (!data.IsEmpty)
        {
            var count = tcp.Socket.Receive(data, SocketFlags.None);
            if (count == 0) throw new EndOfStreamException("Truncated ADB response.");
            data = data[count..];
        }
        return length;
    }
    public override int Read(byte[] data) => Read(data.AsSpan());
    public override int Read(byte[] data, int length) => Read(data, 0, length);
    public override int Read(byte[] data, int offset, int length) => Read(data.AsSpan(offset, length == -1 ? data.Length - offset : length));
    public override async ValueTask<int> ReadAsync(Memory<byte> data, CancellationToken ct = default)
    {
        var length = data.Length;
        while (!data.IsEmpty)
        {
            var count = await tcp.Socket.ReceiveAsync(data, SocketFlags.None, ct).ConfigureAwait(false);
            if (count == 0) throw new EndOfStreamException("Truncated ADB response.");
            data = data[count..];
        }
        return length;
    }
    public override Task<int> ReadAsync(byte[] data, CancellationToken ct = default) => ReadAsync(data.AsMemory(), ct).AsTask();
    public override Task<int> ReadAsync(byte[] data, int length, CancellationToken ct = default) => ReadAsync(data, 0, length, ct);
    public override Task<int> ReadAsync(byte[] data, int offset, int length, CancellationToken ct = default) => ReadAsync(data.AsMemory(offset, length == -1 ? data.Length - offset : length), ct).AsTask();
    private static int SyncLength(byte[] bytes)
    {
        var length = BinaryPrimitives.ReadInt32LittleEndian(bytes);
        if (length < 0) throw new IOException($"Invalid ADB sync string length: {length}.");
        if (length > AdbDeviceBackend.MaximumStderrBytes) throw new DeviceException($"ADB sync message too large: {length}.", 413, 1007);
        return length;
    }
    public override string ReadSyncString()
    {
        var prefix = new byte[4]; Read(prefix);
        var bytes = new byte[SyncLength(prefix)]; Read(bytes);
        return Encoding.UTF8.GetString(bytes);
    }
    public override async Task<string> ReadSyncStringAsync(CancellationToken ct = default)
    {
        var prefix = new byte[4]; await ReadAsync(prefix, ct).ConfigureAwait(false);
        var bytes = new byte[SyncLength(prefix)]; await ReadAsync(bytes, ct).ConfigureAwait(false);
        return Encoding.UTF8.GetString(bytes);
    }
    SyncCommand IAdbSocket.ReadSyncResponse()
    {
        var response = base.ReadSyncResponse();
        if (response is SyncCommand.DONE or SyncCommand.OKAY)
        {
            var trailer = new byte[4]; Read(trailer);
            if (BinaryPrimitives.ReadInt32LittleEndian(trailer) != 0) throw new IOException("Malformed ADB sync completion.");
        }
        return response;
    }
    async Task<SyncCommand> IAdbSocket.ReadSyncResponseAsync(CancellationToken ct)
    {
        var response = await base.ReadSyncResponseAsync(ct).ConfigureAwait(false);
        if (response is SyncCommand.DONE or SyncCommand.OKAY)
        {
            var trailer = new byte[4]; await ReadAsync(trailer, ct).ConfigureAwait(false);
            if (BinaryPrimitives.ReadInt32LittleEndian(trailer) != 0) throw new IOException("Malformed ADB sync completion.");
        }
        return response;
    }
}
