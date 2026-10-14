using System.Buffers.Binary;

namespace MatrixDevice.Core;

public sealed record VideoPacketHeader(ulong Flags, bool IsCodecConfig, bool IsKeyFrame, int PayloadLength)
{
    public ulong TimestampBits => Flags & 0x3fffffffffffffff;
}

public static class ScrcpyProtocol
{
    public const int VideoHeaderLength = 12;
    public const int TouchPacketLength = 32;
    public const int DefaultMaximumPayload = 16 * 1024 * 1024;

    public static VideoPacketHeader ParseVideoHeader(ReadOnlySpan<byte> bytes, int maximumPayload = DefaultMaximumPayload)
    {
        if (bytes.Length != VideoHeaderLength) throw new InvalidDataException($"Video header must be 12 bytes, received {bytes.Length}.");
        if (maximumPayload <= 0) throw new ArgumentOutOfRangeException(nameof(maximumPayload));
        var flags = BinaryPrimitives.ReadUInt64BigEndian(bytes);
        var length = BinaryPrimitives.ReadInt32BigEndian(bytes[8..]);
        if (length > maximumPayload) throw new InvalidDataException($"Video payload {length} exceeds the configured {maximumPayload} bytes.");
        return new(flags, (flags & (1UL << 63)) != 0, (flags & (1UL << 62)) != 0, length);
    }

    public static async Task<(VideoPacketHeader Header, byte[] Payload)?> ReadPacketAsync(Stream stream, CancellationToken ct = default)
    {
        var headerBytes = new byte[VideoHeaderLength];
        while (true)
        {
            var first = await stream.ReadAsync(headerBytes.AsMemory(0, 1), ct).ConfigureAwait(false);
            if (first == 0) return null;
            await stream.ReadExactlyAsync(headerBytes.AsMemory(1), ct).ConfigureAwait(false);
            var header = ParseVideoHeader(headerBytes);
            if (header.PayloadLength <= 0) continue;
            var payload = new byte[header.PayloadLength];
            await stream.ReadExactlyAsync(payload, ct).ConfigureAwait(false);
            return (header, payload);
        }
    }

    public static byte[] EncodeTouch(byte action, ulong pointerId, int x, int y, ushort width, ushort height, uint buttons = 1)
    {
        if (width == 0 || height == 0) throw new InvalidDataException($"Touch screen size must be positive: {width}x{height}.");
        if (x < 0 || x >= width || y < 0 || y >= height) throw new InvalidDataException($"Touch point ({x},{y}) is outside {width}x{height}.");
        if (action > 6) throw new InvalidDataException($"Unknown Android touch action: {action}.");
        var packet = new byte[TouchPacketLength];
        var released = action is 1 or 3;
        packet[0] = 2;
        packet[1] = action;
        BinaryPrimitives.WriteUInt64BigEndian(packet.AsSpan(2), pointerId);
        BinaryPrimitives.WriteInt32BigEndian(packet.AsSpan(10), x);
        BinaryPrimitives.WriteInt32BigEndian(packet.AsSpan(14), y);
        BinaryPrimitives.WriteUInt16BigEndian(packet.AsSpan(18), width);
        BinaryPrimitives.WriteUInt16BigEndian(packet.AsSpan(20), height);
        BinaryPrimitives.WriteUInt16BigEndian(packet.AsSpan(22), released ? (ushort)0 : ushort.MaxValue);
        BinaryPrimitives.WriteUInt32BigEndian(packet.AsSpan(24), 1);
        BinaryPrimitives.WriteUInt32BigEndian(packet.AsSpan(28), released ? 0 : buttons);
        return packet;
    }
}
