using System.Buffers;

namespace MatrixDevice.Core;

public static class BoundedStreamRead
{
    public static async Task<byte[]> ReadAsync(Stream stream, int maximumBytes, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (maximumBytes < 0) throw new ArgumentOutOfRangeException(nameof(maximumBytes));
        var buffer = ArrayPool<byte>.Shared.Rent(65536);
        try
        {
            using var output = new MemoryStream(Math.Min(maximumBytes, buffer.Length));
            while (true)
            {
                var count = await stream.ReadAsync(buffer.AsMemory(), ct).ConfigureAwait(false);
                if (count == 0) return output.ToArray();
                if (output.Length + count > maximumBytes) throw new DeviceException($"Response exceeds {maximumBytes} bytes: received at least {output.Length + count}.", 413, 1007);
                output.Write(buffer, 0, count);
            }
        }
        finally { ArrayPool<byte>.Shared.Return(buffer); }
    }
}
