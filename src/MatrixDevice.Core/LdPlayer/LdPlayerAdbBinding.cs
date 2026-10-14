using System.Globalization;
using System.Net;
using System.Net.Sockets;

namespace MatrixDevice.Core;

public static class LdPlayerAdbBinding
{
    public static async Task VerifyAsync(string serial, int expectedVBoxPid, long expectedStartedUtcTicks, CancellationToken ct = default)
    {
        IPEndPoint endpoint;
        if (serial.StartsWith("emulator-", StringComparison.Ordinal) &&
            int.TryParse(serial[9..], NumberStyles.None, CultureInfo.InvariantCulture, out var consolePort) && consolePort is > 0 and < 65535)
            endpoint = new(IPAddress.Loopback, consolePort + 1);
        else if (IPEndPoint.TryParse(serial, out var parsed) && parsed.AddressFamily == AddressFamily.InterNetwork && parsed.Port > 0)
            endpoint = parsed;
        else throw new DeviceException($"Local LDPlayer endpoint cannot be verified for serial={serial}; Unicode input disabled.", 501, 2001);
        if (expectedVBoxPid <= 0 || expectedStartedUtcTicks <= 0) throw new DeviceException("Invalid expected LDPlayer process identity.", 409, 1003);
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(ct); deadline.CancelAfter(TimeSpan.FromSeconds(3));
        try
        {
            // The serial supplies only a candidate port. Its actual owner must match the frozen instance process.
            // No ADB request, guest command, or host:connect is sent over this owned probe.
            using var connection = await ExistingAdbSocket.OpenAsync(endpoint, deadline.Token).ConfigureAwait(false);
            var actual = AdbServerIdentity.ForConnection(connection.ConnectedSocket);
            if (actual.ProcessId != expectedVBoxPid || actual.StartedUtcTicks != expectedStartedUtcTicks)
                throw new DeviceException($"LDPlayer serial/index binding mismatch: serial={serial}, expectedPid={expectedVBoxPid}, actualPid={actual.ProcessId}. No text was sent.", 409, 1003);
        }
        catch (Exception ex) when (ex is SocketException or IOException or ObjectDisposedException || ex is OperationCanceledException && !ct.IsCancellationRequested)
        { throw new DeviceException($"Cannot verify the local LDPlayer binding: serial={serial}: {ex.Message}. No text was sent.", 503, 2000); }
    }
}
