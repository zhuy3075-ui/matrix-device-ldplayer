using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace MatrixDevice.Core;

internal readonly record struct AdbServerIdentity(int ProcessId, long StartedUtcTicks)
{
    public static AdbServerIdentity ForConnection(Socket socket)
    {
        if (!OperatingSystem.IsWindows()) throw new DeviceException("Existing ADB owner verification requires Windows.", 503, 2000);
        var local = socket.LocalEndPoint as IPEndPoint ?? throw new IOException("Missing ADB local endpoint.");
        var remote = socket.RemoteEndPoint as IPEndPoint ?? throw new IOException("Missing ADB remote endpoint.");
        var size = 0;
        var result = GetExtendedTcpTable(IntPtr.Zero, ref size, false, 2, 4, 0);
        for (var attempt = 0; attempt < 3 && result == 122; attempt++)
        {
            if (size < 4 || size > 8 * 1024 * 1024) throw new IOException($"Invalid TCP table size: {size}.");
            var buffer = Marshal.AllocHGlobal(size);
            try
            {
                var capacity = size;
                result = GetExtendedTcpTable(buffer, ref size, false, 2, 4, 0);
                if (result == 122) continue;
                if (result != 0) throw new IOException($"TCP owner query failed: {result}.");
                var count = Marshal.ReadInt32(buffer);
                var stride = Marshal.SizeOf<TcpRow>();
                if (count < 0 || count > (capacity - 4) / stride) throw new IOException("Malformed TCP owner table.");
                var owners = new HashSet<int>();
                for (var i = 0; i < count; i++)
                {
                    var row = Marshal.PtrToStructure<TcpRow>(buffer + 4 + i * stride);
                    // Identify the server end of this exact established connection, not only its listener.
                    if (row.State == 5 && Address(row.LocalAddress).Equals(remote.Address) && Port(row.LocalPort) == remote.Port &&
                        Address(row.RemoteAddress).Equals(local.Address) && Port(row.RemotePort) == local.Port)
                        owners.Add(checked((int)row.ProcessId));
                }
                if (owners.Count != 1) throw new DeviceException($"ADB connection must have one verified server owner; actual={owners.Count}.", 503, 2000);
                using var process = Process.GetProcessById(owners.Single());
                var identity = new AdbServerIdentity(process.Id, process.StartTime.ToUniversalTime().Ticks);
                if (process.HasExited) throw new IOException("ADB server exited during owner verification.");
                return identity;
            }
            finally { Marshal.FreeHGlobal(buffer); }
        }
        throw new IOException($"TCP owner query failed after bounded retries: {result}.");
    }

    private static IPAddress Address(uint value) => new(BitConverter.GetBytes(value));
    private static int Port(uint value) => (int)(((value & 255) << 8) | ((value >> 8) & 255));
    [StructLayout(LayoutKind.Sequential)]
    private struct TcpRow { public uint State, LocalAddress, LocalPort, RemoteAddress, RemotePort, ProcessId; }
    [DllImport("iphlpapi.dll", ExactSpelling = true)]
    private static extern uint GetExtendedTcpTable(IntPtr table, ref int size, [MarshalAs(UnmanagedType.Bool)] bool ordered, uint family, uint tableClass, uint reserved);
}

internal sealed class AdbServerEpoch
{
    private readonly object gate = new();
    private AdbServerIdentity? observed;
    public AdbServerIdentity Snapshot { get { lock (gate) return observed ?? throw new DeviceException("尚未核验ADB服务身份，不能继承连接。", 409, 1003); } }
    public void Require(AdbServerIdentity current)
    {
        lock (gate)
        {
            if (observed is { } previous && previous != current)
                throw new DeviceException($"ADB server owner changed: previous={previous}, current={current}. Restart this client and rediscover before continuing; server left untouched.", 409, 1003);
            observed = current;
        }
    }
}
