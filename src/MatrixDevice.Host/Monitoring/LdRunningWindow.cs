using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using MatrixDevice.Core;
using Microsoft.Win32.SafeHandles;

namespace MatrixDevice.Host;

public static class LdRunningWindow
{
    // True means foreground activation was accepted; restoration itself is asynchronous.
    public static bool Show(LdPlayerInstance instance, string expectedInstallDirectory, long expectedPlayerBirth)
        => Show(instance, expectedInstallDirectory, expectedPlayerBirth, SetForegroundWindow);

    internal static bool Show(LdPlayerInstance instance, string expectedInstallDirectory, long expectedPlayerBirth, Func<IntPtr, bool> requestForeground)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(requestForeground);
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException("Showing an existing instance window requires Windows.");
        if (instance.Index < 0 || instance.Index >= 99999 || instance.Generation <= 0 || instance.ProcessId <= 0 || instance.TopWindow <= 0 || expectedPlayerBirth <= 0)
            throw new ArgumentException($"Invalid running window target: index {instance.Index}, generation {instance.Generation}, PID {instance.ProcessId}, window {instance.TopWindow}, birth {expectedPlayerBirth}.");
        if (string.IsNullOrWhiteSpace(expectedInstallDirectory) || !Path.IsPathFullyQualified(expectedInstallDirectory) || !Directory.Exists(expectedInstallDirectory))
            throw new ArgumentException($"The expected installation directory must exist and be absolute: {expectedInstallDirectory}.", nameof(expectedInstallDirectory));
        var install = Path.TrimEndingDirectorySeparator(Path.GetFullPath(expectedInstallDirectory));
        var window = new IntPtr(instance.TopWindow);
        ValidateWindow(window, instance.ProcessId);
        using var process = OpenProcess(0x1000 | 0x100000, false, instance.ProcessId);
        if (process.IsInvalid) throw new Win32Exception(Marshal.GetLastWin32Error(), $"Cannot open player process {instance.ProcessId}.");
        RequireRunning(process, instance.ProcessId);
        if (!GetProcessTimes(process, out var created, out _, out _, out _))
            throw new Win32Exception(Marshal.GetLastWin32Error(), $"Cannot read player process {instance.ProcessId} birth.");
        var birth = DateTime.FromFileTimeUtc(created).Ticks;
        if (birth != expectedPlayerBirth)
            throw new InvalidOperationException($"Player process {instance.ProcessId} birth mismatch: expected {expectedPlayerBirth}, actual {birth}.");
        var executable = new StringBuilder(32768); var size = (uint)executable.Capacity;
        if (!QueryFullProcessImageName(process, 0, executable, ref size))
            throw new Win32Exception(Marshal.GetLastWin32Error(), $"Cannot read player process {instance.ProcessId} executable path.");
        var executableDirectory = Path.GetDirectoryName(Path.GetFullPath(executable.ToString()));
        if (executableDirectory is null || !Path.TrimEndingDirectorySeparator(executableDirectory).Equals(install, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Player process {instance.ProcessId} executable directory {executableDirectory} does not match {install}.");
        RequireRunning(process, instance.ProcessId);
        ValidateWindow(window, instance.ProcessId);
        if (!ShowWindowAsync(window, 9)) throw new InvalidOperationException($"Restore request was rejected for instance {instance.Index}, window {instance.TopWindow}.");
        RequireRunning(process, instance.ProcessId);
        ValidateWindow(window, instance.ProcessId);
        return requestForeground(window);
    }

    private static void ValidateWindow(IntPtr window, int expectedPid)
    {
        if (!IsWindow(window) || GetAncestor(window, 2) != window)
            throw new InvalidOperationException($"Window {window} is not an existing top-level window.");
        if (GetWindowThreadProcessId(window, out var pid) == 0 || pid != expectedPid)
            throw new InvalidOperationException($"Window {window} owner mismatch: expected PID {expectedPid}, actual {pid}.");
    }

    private static void RequireRunning(SafeProcessHandle process, int pid)
    {
        var state = WaitForSingleObject(process, 0);
        if (state == 258) return;
        if (state == uint.MaxValue) throw new Win32Exception(Marshal.GetLastWin32Error(), $"Cannot check player process {pid} liveness.");
        throw new InvalidOperationException($"Player process {pid} is not running (wait state {state}).");
    }

    [DllImport("kernel32.dll", SetLastError = true)] private static extern SafeProcessHandle OpenProcess(uint access, [MarshalAs(UnmanagedType.Bool)] bool inherit, int pid);
    [DllImport("kernel32.dll", SetLastError = true)] private static extern uint WaitForSingleObject(SafeProcessHandle handle, uint milliseconds);
    [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool GetProcessTimes(SafeProcessHandle process, out long created, out long exited, out long kernel, out long user);
    [DllImport("kernel32.dll", EntryPoint = "QueryFullProcessImageNameW", CharSet = CharSet.Unicode, SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool QueryFullProcessImageName(SafeProcessHandle process, uint flags, StringBuilder path, ref uint size);
    [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool IsWindow(IntPtr window);
    [DllImport("user32.dll")] private static extern IntPtr GetAncestor(IntPtr window, uint flags);
    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr window, out int pid);
    [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool ShowWindowAsync(IntPtr window, int command);
    [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool SetForegroundWindow(IntPtr window);
}
