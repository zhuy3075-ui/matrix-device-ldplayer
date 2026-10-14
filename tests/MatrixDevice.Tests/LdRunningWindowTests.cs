using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using MatrixDevice.Core;
using MatrixDevice.Host;

public static class LdRunningWindowTests
{
    public static void Register(Action<string, Func<Task>> test)
    {
        test("existing LD window rejects protected stale and foreign identities before showing", RejectionAsync);
        test("existing LD window restores an owned native window and reports focus refusal", RestoreAsync);
        test("existing LD window refuses child and destroyed handles", InvalidHandleAsync);
    }

    private static Task RejectionAsync()
    {
        if (!OperatingSystem.IsWindows()) return Task.CompletedTask;
        var window = CreateOwnWindow();
        using var process = Process.GetCurrentProcess();
        var birth = process.StartTime.ToUniversalTime().Ticks;
        var install = Path.GetDirectoryName(process.MainModule!.FileName)!;
        var instance = OwnInstance(window);
        var calls = 0;
        bool Focus(IntPtr _) { calls++; return true; }
        try
        {
            foreach (var invalid in new[] { instance with { Index = 0 }, instance with { Index = -1 }, instance with { Index = 99999 }, instance with { Index = 100000 }, instance with { Generation = 0 }, instance with { ProcessId = int.MaxValue } })
                ExpectRefusal(() => LdRunningWindow.Show(invalid, install, birth, Focus));
            ExpectRefusal(() => LdRunningWindow.Show(instance, install, birth + 1, Focus));
            ExpectRefusal(() => LdRunningWindow.Show(instance, Path.GetTempPath(), birth, Focus));
            ExpectRefusal(() => LdRunningWindow.Show(instance, "relative-install", birth, Focus));
            Assert(calls == 0 && !IsWindowVisible(window), "Rejected identity reached activation or showed the window.");
        }
        finally { DestroyWindow(window); }
        return Task.CompletedTask;
    }

    private static Task RestoreAsync()
    {
        if (!OperatingSystem.IsWindows()) return Task.CompletedTask;
        var window = CreateOwnWindow();
        using var process = Process.GetCurrentProcess();
        var birth = process.StartTime.ToUniversalTime().Ticks;
        var install = Path.GetDirectoryName(process.MainModule!.FileName)!;
        var instance = OwnInstance(window);
        var calls = 0;
        try
        {
            var foreground = LdRunningWindow.Show(instance, install, birth, handle => { calls++; Assert(handle == window, "Activation changed target."); return false; });
            PumpMessages();
            Assert(!foreground && calls == 1 && IsWindowVisible(window), "Foreground refusal was treated as failure to request restoration or reported as success.");
            Assert(GetParent(window) == IntPtr.Zero, "Restoration reparented the window.");
            LdRunningWindow.Show(instance, install, birth);
            PumpMessages();
            Assert(IsWindowVisible(window), "Native foreground request hid the existing window.");
        }
        finally { DestroyWindow(window); }
        return Task.CompletedTask;
    }

    private static Task InvalidHandleAsync()
    {
        if (!OperatingSystem.IsWindows()) return Task.CompletedTask;
        var parent = CreateOwnWindow(); var child = CreateOwnWindow(parent);
        using var process = Process.GetCurrentProcess();
        var birth = process.StartTime.ToUniversalTime().Ticks;
        var install = Path.GetDirectoryName(process.MainModule!.FileName)!;
        try
        {
            ExpectRefusal(() => LdRunningWindow.Show(OwnInstance(child), install, birth));
            Assert(!IsWindowVisible(parent), "Child-window rejection changed its parent.");
            DestroyWindow(child); child = IntPtr.Zero;
            var destroyed = parent; DestroyWindow(parent); parent = IntPtr.Zero;
            ExpectRefusal(() => LdRunningWindow.Show(OwnInstance(destroyed), install, birth));
        }
        finally { if (child != IntPtr.Zero) DestroyWindow(child); if (parent != IntPtr.Zero) DestroyWindow(parent); }
        return Task.CompletedTask;
    }

    private static LdPlayerInstance OwnInstance(IntPtr window) => new(1, "Owned native test window", window.ToInt64(), 0, false, Environment.ProcessId, -1);

    private static IntPtr CreateOwnWindow(IntPtr parent = default)
    {
        var window = CreateWindowEx(0x08000080, "STATIC", "Owned monitoring window test", parent == IntPtr.Zero ? 0x00CF0000u : 0x40000000u,
            -20000, -20000, 120, 80, parent, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
        if (window == IntPtr.Zero) throw new Win32Exception(Marshal.GetLastWin32Error(), "Cannot create the owned native test window.");
        return window;
    }

    private static void PumpMessages()
    {
        while (PeekMessage(out var message, IntPtr.Zero, 0, 0, 1))
        {
            TranslateMessage(ref message); DispatchMessage(ref message);
        }
    }

    private static void ExpectRefusal(Action action)
    {
        try { action(); }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or Win32Exception) { return; }
        throw new InvalidOperationException("Invalid existing-window target was accepted.");
    }

    private static void Assert(bool valid, string message) { if (!valid) throw new InvalidOperationException(message); }

    [StructLayout(LayoutKind.Sequential)]
    private struct WindowMessage
    {
        public IntPtr Window;
        public uint Message;
        public nuint WParam;
        public IntPtr LParam;
        public uint Time;
        public int X, Y;
        public uint Private;
    }

    [DllImport("user32.dll", EntryPoint = "CreateWindowExW", CharSet = CharSet.Unicode, SetLastError = true)] private static extern IntPtr CreateWindowEx(uint exStyle, string className, string title, uint style, int x, int y, int width, int height, IntPtr parent, IntPtr menu, IntPtr instance, IntPtr parameter);
    [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool DestroyWindow(IntPtr window);
    [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool IsWindowVisible(IntPtr window);
    [DllImport("user32.dll")] private static extern IntPtr GetParent(IntPtr window);
    [DllImport("user32.dll", EntryPoint = "PeekMessageW")] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool PeekMessage(out WindowMessage message, IntPtr window, uint minimum, uint maximum, uint remove);
    [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool TranslateMessage(ref WindowMessage message);
    [DllImport("user32.dll", EntryPoint = "DispatchMessageW")] private static extern IntPtr DispatchMessage(ref WindowMessage message);
}
