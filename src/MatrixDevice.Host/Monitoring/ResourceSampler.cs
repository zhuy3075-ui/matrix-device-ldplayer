using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

[assembly: InternalsVisibleTo("MatrixDevice.Tests")]

namespace MatrixDevice.Host;

public sealed record MonitorProcessTarget(int Index, long Generation, int PlayerPid, long PlayerBirth, int VBoxPid, long VBoxBirth);
public sealed record MonitorResourceSample(int Index, long Generation, DateTimeOffset ObservedAt, double? CpuPercent, long? WorkingSetBytes, string? Error);
public sealed record HostResourceSample(DateTimeOffset ObservedAt, double? CpuPercent, long? TotalPhysical, long? AvailablePhysical, string? Error);

public sealed class ResourceSampler
{
    private readonly object gate = new();
    private readonly Func<HostCounters> readHost;
    private readonly Func<int, long, ProcessCounters> readProcess;
    private readonly Func<int> readProcessorCount;
    private readonly TimeProvider clock;
    private readonly Dictionary<int, InstanceBaseline> instances = [];
    private HostCounters? previousHost;

    public ResourceSampler() : this(ReadHost, ReadProcess, ReadProcessorCount, TimeProvider.System) { }

    internal ResourceSampler(Func<HostCounters> readHost, Func<int, long, ProcessCounters> readProcess, Func<int> readProcessorCount, TimeProvider clock)
    {
        this.readHost = readHost ?? throw new ArgumentNullException(nameof(readHost));
        this.readProcess = readProcess ?? throw new ArgumentNullException(nameof(readProcess));
        this.readProcessorCount = readProcessorCount ?? throw new ArgumentNullException(nameof(readProcessorCount));
        this.clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public HostResourceSample SampleHost()
    {
        lock (gate)
        {
            var observedAt = clock.GetUtcNow();
            try
            {
                var current = readHost();
                if (current.TotalPhysical <= 0 || current.AvailablePhysical < 0 || current.AvailablePhysical > current.TotalPhysical)
                    throw new InvalidOperationException($"Invalid physical memory: total {current.TotalPhysical}, available {current.AvailablePhysical}.");
                double? cpu = null;
                if (previousHost is { } previous)
                {
                    if (current.Idle < previous.Idle || current.Kernel < previous.Kernel || current.User < previous.User)
                        throw new InvalidOperationException("Host CPU counters moved backwards.");
                    var total = checked(current.Kernel - previous.Kernel + current.User - previous.User);
                    var idle = current.Idle - previous.Idle;
                    if (total == 0 || idle > total) throw new InvalidOperationException($"Invalid host CPU interval: total {total}, idle {idle}.");
                    cpu = (1d - (double)idle / total) * 100d;
                }
                previousHost = current;
                return new(observedAt, cpu, current.TotalPhysical, current.AvailablePhysical, null);
            }
            catch (Exception ex) when (IsSamplingFailure(ex))
            {
                previousHost = null;
                return new(observedAt, null, null, null, ex.Message);
            }
        }
    }

    public MonitorResourceSample SampleInstance(MonitorProcessTarget target)
    {
        ArgumentNullException.ThrowIfNull(target);
        lock (gate)
        {
            var observedAt = clock.GetUtcNow();
            try
            {
                if (target.Index <= 0 || target.Index == 99999 || target.Generation <= 0 || target.PlayerPid <= 0 || target.VBoxPid <= 0 || target.PlayerBirth <= 0 || target.VBoxBirth <= 0)
                    throw new ArgumentException($"Invalid monitor target: index {target.Index}, generation {target.Generation}, player {target.PlayerPid}/{target.PlayerBirth}, VBox {target.VBoxPid}/{target.VBoxBirth}.");
                if (target.PlayerPid == target.VBoxPid && target.PlayerBirth != target.VBoxBirth)
                    throw new ArgumentException($"Process {target.PlayerPid} has conflicting expected birth times.");
                var processorCount = readProcessorCount();
                if (processorCount <= 0) throw new InvalidOperationException($"Invalid logical processor count: {processorCount}.");
                var player = ReadVerifiedProcess(target.PlayerPid, target.PlayerBirth);
                var vbox = target.VBoxPid == target.PlayerPid ? null : ReadVerifiedProcess(target.VBoxPid, target.VBoxBirth);
                var cpuTicks = checked(player.CpuTicks + (vbox?.CpuTicks ?? 0));
                // Working sets contain shared pages; this is not exclusive or guest memory.
                var workingSet = checked(player.WorkingSetBytes + (vbox?.WorkingSetBytes ?? 0));
                var timestamp = clock.GetTimestamp();
                double? cpu = null;
                if (instances.TryGetValue(target.Index, out var previous) && previous.Target == target)
                {
                    var elapsed = clock.GetElapsedTime(previous.Timestamp, timestamp).TotalSeconds;
                    if (elapsed <= 0 || cpuTicks < previous.CpuTicks)
                        throw new InvalidOperationException($"Invalid CPU interval for instance {target.Index}: elapsed {elapsed}, CPU ticks {previous.CpuTicks} -> {cpuTicks}.");
                    cpu = Math.Min(100d, (cpuTicks - previous.CpuTicks) / (double)TimeSpan.TicksPerSecond / elapsed / processorCount * 100d);
                }
                instances[target.Index] = new(target, cpuTicks, timestamp);
                return new(target.Index, target.Generation, observedAt, cpu, workingSet, null);
            }
            catch (Exception ex) when (IsSamplingFailure(ex))
            {
                instances.Remove(target.Index);
                return new(target.Index, target.Generation, observedAt, null, null, $"Instance {target.Index}: {ex.Message}");
            }
        }
    }

    private ProcessCounters ReadVerifiedProcess(int pid, long birth)
    {
        var value = readProcess(pid, birth);
        if (value.Birth != birth) throw new InvalidOperationException($"Process {pid} birth mismatch: expected {birth}, actual {value.Birth}.");
        if (value.CpuTicks < 0 || value.WorkingSetBytes < 0)
            throw new InvalidOperationException($"Invalid counters for process {pid}: CPU ticks {value.CpuTicks}, working set {value.WorkingSetBytes}.");
        return value;
    }

    private static bool IsSamplingFailure(Exception ex) => ex is Win32Exception or IOException or InvalidOperationException or ArgumentException or NotSupportedException or OverflowException or DllNotFoundException or EntryPointNotFoundException;

    private static HostCounters ReadHost()
    {
        RequireWindows();
        var groups = GetActiveProcessorGroupCount();
        if (groups == 0) throw new Win32Exception(Marshal.GetLastWin32Error(), "Cannot read active processor groups.");
        if (groups != 1) throw new PlatformNotSupportedException($"Host CPU sampling does not support {groups} processor groups.");
        var memory = new MemoryStatus { Length = (uint)Marshal.SizeOf<MemoryStatus>() };
        if (!GlobalMemoryStatusEx(ref memory)) throw new Win32Exception(Marshal.GetLastWin32Error(), "Cannot read physical memory.");
        if (!GetSystemTimes(out var idle, out var kernel, out var user)) throw new Win32Exception(Marshal.GetLastWin32Error(), "Cannot read host CPU times.");
        return new(idle, kernel, user, checked((long)memory.TotalPhysical), checked((long)memory.AvailablePhysical));
    }

    private static int ReadProcessorCount()
    {
        RequireWindows();
        var count = GetActiveProcessorCount(ushort.MaxValue);
        if (count == 0) throw new Win32Exception(Marshal.GetLastWin32Error(), "Cannot read active logical processors.");
        return checked((int)count);
    }

    private static ProcessCounters ReadProcess(int pid, long expectedBirth)
    {
        RequireWindows();
        // Keep one handle so PID reuse cannot switch the process between identity and counters.
        using var handle = OpenProcess(0x1000 | 0x0010 | 0x100000, false, pid);
        if (handle.IsInvalid) throw new Win32Exception(Marshal.GetLastWin32Error(), $"Cannot open process {pid} for resource sampling.");
        RequireRunning(handle, pid);
        if (!GetProcessTimes(handle, out var created, out _, out _, out _))
            throw new Win32Exception(Marshal.GetLastWin32Error(), $"Cannot read process {pid} identity.");
        var birth = DateTime.FromFileTimeUtc(created).Ticks;
        if (birth != expectedBirth) throw new InvalidOperationException($"Process {pid} birth mismatch: expected {expectedBirth}, actual {birth}.");
        var memory = new ProcessMemory { Size = (uint)Marshal.SizeOf<ProcessMemory>() };
        if (!K32GetProcessMemoryInfo(handle, ref memory, memory.Size))
            throw new Win32Exception(Marshal.GetLastWin32Error(), $"Cannot read process {pid} working set.");
        if (!GetProcessTimes(handle, out _, out _, out var kernel, out var user))
            throw new Win32Exception(Marshal.GetLastWin32Error(), $"Cannot read process {pid} CPU times.");
        RequireRunning(handle, pid);
        return new(birth, checked(kernel + user), checked((long)memory.WorkingSet));
    }

    private static void RequireRunning(SafeProcessHandle handle, int pid)
    {
        var state = WaitForSingleObject(handle, 0);
        if (state == 258) return;
        if (state == uint.MaxValue) throw new Win32Exception(Marshal.GetLastWin32Error(), $"Cannot check process {pid} liveness.");
        if (state == 0) throw new InvalidOperationException($"Process {pid} has exited.");
        throw new InvalidOperationException($"Unexpected wait state {state} for process {pid}.");
    }

    private static void RequireWindows()
    {
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException("Resource sampling requires Windows.");
    }

    internal sealed record HostCounters(ulong Idle, ulong Kernel, ulong User, long TotalPhysical, long AvailablePhysical);
    internal sealed record ProcessCounters(long Birth, long CpuTicks, long WorkingSetBytes);
    private sealed record InstanceBaseline(MonitorProcessTarget Target, long CpuTicks, long Timestamp);

    [StructLayout(LayoutKind.Sequential)]
    private struct MemoryStatus
    {
        public uint Length, Load;
        public ulong TotalPhysical, AvailablePhysical, TotalPageFile, AvailablePageFile, TotalVirtual, AvailableVirtual, AvailableExtended;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ProcessMemory
    {
        public uint Size, PageFaultCount;
        public nuint PeakWorkingSet, WorkingSet, QuotaPeakPagedPool, QuotaPagedPool, QuotaPeakNonPagedPool, QuotaNonPagedPool, PageFile, PeakPageFile;
    }

    [DllImport("kernel32.dll", SetLastError = true)] private static extern ushort GetActiveProcessorGroupCount();
    [DllImport("kernel32.dll", SetLastError = true)] private static extern uint GetActiveProcessorCount(ushort group);
    [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool GetSystemTimes(out ulong idle, out ulong kernel, out ulong user);
    [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool GlobalMemoryStatusEx(ref MemoryStatus memory);
    [DllImport("kernel32.dll", SetLastError = true)] private static extern SafeProcessHandle OpenProcess(uint access, [MarshalAs(UnmanagedType.Bool)] bool inherit, int pid);
    [DllImport("kernel32.dll", SetLastError = true)] private static extern uint WaitForSingleObject(SafeProcessHandle handle, uint milliseconds);
    [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool GetProcessTimes(SafeProcessHandle process, out long created, out long exited, out long kernel, out long user);
    [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool K32GetProcessMemoryInfo(SafeProcessHandle process, ref ProcessMemory memory, uint size);
}
