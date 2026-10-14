using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace MatrixDevice.Core;

public sealed record NetworkScanProgress(int Completed, int Total, int Found, int Connected, string CurrentAddress, string? Error = null);
public sealed record NetworkScanEndpoint(string Address, int Port, bool Found, bool Connected, string? Error = null);
public sealed record NetworkScanResult(int Total, int Found, int Connected, IReadOnlyList<NetworkScanEndpoint> Endpoints);

public sealed class NetworkDeviceScanner(AdbDeviceBackend backend)
{
    public async Task<NetworkScanResult> ScanAsync(string startIp, string endIp, int port, int concurrency = 16, int probeTimeoutMs = 1500, IProgress<NetworkScanProgress>? progress = null, CancellationToken ct = default)
    {
        var start = ParseAddress(startIp); var end = ParseAddress(endIp);
        var count = (long)end - start + 1;
        if (count is < 1 or > 1024) throw new DeviceException($"IPv4 scan must contain 1..1024 endpoints: start={startIp}, end={endIp}, count={count}.", 400, 1007);
        if (port is < 1 or > 65535 || concurrency is < 1 or > 16 || probeTimeoutMs is < 100 or > 30000)
            throw new DeviceException($"Invalid scan options: port={port}, concurrency={concurrency}, timeoutMs={probeTimeoutMs}.", 400, 1007);
        var total = (int)count; var completed = 0; var foundCount = 0; var connectedCount = 0;
        var endpoints = new ConcurrentDictionary<uint, NetworkScanEndpoint>();
        await Parallel.ForEachAsync(Enumerable.Range(0, total), new ParallelOptions { CancellationToken = ct, MaxDegreeOfParallelism = concurrency }, async (index, token) =>
        {
            var numeric = start + (uint)index;
            var bytes = new byte[4]; BinaryPrimitives.WriteUInt32BigEndian(bytes, numeric);
            var address = new IPAddress(bytes).ToString();
            var found = false; var connected = false; string? error = null;
            try
            {
                using (var probe = new TcpClient(AddressFamily.InterNetwork))
                using (var timeout = CancellationTokenSource.CreateLinkedTokenSource(token))
                {
                    timeout.CancelAfter(probeTimeoutMs);
                    await probe.ConnectAsync(IPAddress.Parse(address), port, timeout.Token).ConfigureAwait(false);
                    found = true; Interlocked.Increment(ref foundCount);
                }
                await backend.ConnectNetworkAsync(address, port, token).ConfigureAwait(false);
                connected = true; Interlocked.Increment(ref connectedCount);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested) { throw; }
            catch (OperationCanceledException) { error = $"TCP probe timed out after {probeTimeoutMs} ms."; }
            catch (Exception ex) when (ex is SocketException or DeviceException or IOException) { error = ex.Message; }
            finally
            {
                endpoints[numeric] = new(address, port, found, connected, error);
                var done = Interlocked.Increment(ref completed);
                progress?.Report(new(done, total, Volatile.Read(ref foundCount), Volatile.Read(ref connectedCount), address, error));
            }
        }).ConfigureAwait(false);
        return new(total, foundCount, connectedCount, endpoints.OrderBy(pair => pair.Key).Select(pair => pair.Value).ToArray());
    }
    private static uint ParseAddress(string value)
    {
        if (!IPAddress.TryParse(value, out var address) || address.AddressFamily != AddressFamily.InterNetwork)
            throw new DeviceException($"Invalid IPv4 address: {value}.", 400, 1007);
        return BinaryPrimitives.ReadUInt32BigEndian(address.GetAddressBytes());
    }
}
