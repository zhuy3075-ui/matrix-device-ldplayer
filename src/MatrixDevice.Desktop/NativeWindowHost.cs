using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Interop;
using MatrixDevice.Core;

namespace MatrixDevice.Desktop;

public sealed class NativeWindowHost : HwndHost
{
    private const int GwlStyle = -16;
    private const int GwlExStyle = -20;
    private const long WsChild = 0x40000000;
    private const long WsPopup = 0x80000000;
    private const long WsCaption = 0x00C00000;
    private const long WsThickFrame = 0x00040000;
    private readonly Process ownedProcess;
    private readonly string journalPath;
    private WindowRecoveryRecord? record;
    private IntPtr container;
    public int ProcessId => ownedProcess.Id;
    public bool IsAttached { get; private set; }

    public NativeWindowHost(Process ownedProcess, string journalDirectory)
    {
        this.ownedProcess = ownedProcess;
        Directory.CreateDirectory(journalDirectory);
        journalPath = Path.Combine(journalDirectory, $"{ownedProcess.Id}-{ownedProcess.MainWindowHandle.ToInt64()}.json");
    }

    protected override HandleRef BuildWindowCore(HandleRef hwndParent)
    {
        ownedProcess.Refresh();
        var window = ownedProcess.MainWindowHandle;
        ValidateOwner(window, ownedProcess.Id, ownedProcess.StartTime.ToUniversalTime().Ticks, ownedProcess.MainModule?.FileName ?? "");
        if (!GetWindowRect(window, out var bounds)) throw new Win32Exception(Marshal.GetLastWin32Error(), "无法读取投屏窗口位置");
        record = new WindowRecoveryRecord(window.ToInt64(), ownedProcess.Id, ownedProcess.StartTime.ToUniversalTime().Ticks, ownedProcess.MainModule?.FileName ?? "", GetParent(window).ToInt64(), GetWindowLongPtr(window, GwlStyle).ToInt64(), GetWindowLongPtr(window, GwlExStyle).ToInt64(), bounds.Left, bounds.Top, bounds.Right - bounds.Left, bounds.Bottom - bounds.Top, IsWindowVisible(window));
        var temporary = journalPath + ".tmp";
        File.WriteAllText(temporary, JsonSerializer.Serialize(record, JsonDefaults.Options));
        File.Move(temporary, journalPath, true);
        container = CreateWindowEx(0, "STATIC", "MatrixDevice.Host", WsChild, 0, 0, 1, 1, hwndParent.Handle, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
        if (container == IntPtr.Zero) throw new Win32Exception(Marshal.GetLastWin32Error(), "无法创建自有承载窗口");
        try
        {
            SetStyle(window, GwlStyle, (record.Style & ~(WsPopup | WsCaption | WsThickFrame)) | WsChild);
            SetCheckedParent(window, container);
            IsAttached = true;
            ShowWindow(window, 5);
            Layout(window, ActualWidth, ActualHeight);
            return new HandleRef(this, container);
        }
        catch
        {
            Restore();
            if (container != IntPtr.Zero) DestroyWindow(container);
            container = IntPtr.Zero;
            throw;
        }
    }

    protected override void OnWindowPositionChanged(Rect rcBoundingBox)
    {
        base.OnWindowPositionChanged(rcBoundingBox);
        if (IsAttached && record is not null) Layout(new IntPtr(record.Window), rcBoundingBox.Width, rcBoundingBox.Height);
    }

    private void Layout(IntPtr window, double width, double height)
    {
        var dpi = System.Windows.Media.VisualTreeHelper.GetDpi(this);
        if (!SetWindowPos(window, IntPtr.Zero, 0, 0, Math.Max(1, (int)(width * dpi.DpiScaleX)), Math.Max(1, (int)(height * dpi.DpiScaleY)), 0x0004 | 0x0010 | 0x0020)) throw new Win32Exception(Marshal.GetLastWin32Error(), "投屏窗口布局失败");
    }

    protected override void DestroyWindowCore(HandleRef hwnd)
    {
        Restore();
        if (hwnd.Handle != IntPtr.Zero && !DestroyWindow(hwnd.Handle)) throw new Win32Exception(Marshal.GetLastWin32Error(), "自有承载窗口关闭失败");
        container = IntPtr.Zero;
    }

    public void Restore()
    {
        if (record is null) return;
        RestoreRecord(record);
        IsAttached = false;
        record = null;
        if (File.Exists(journalPath)) File.Delete(journalPath);
    }

    public static IReadOnlyList<string> RecoverPending(string journalDirectory)
    {
        if (!Directory.Exists(journalDirectory)) return [];
        var errors = new List<string>();
        foreach (var path in Directory.EnumerateFiles(journalDirectory, "*.json"))
        {
            try
            {
                var saved = JsonSerializer.Deserialize<WindowRecoveryRecord>(File.ReadAllText(path), JsonDefaults.Options) ?? throw new InvalidDataException("恢复记录为空");
                RestoreRecord(saved);
                File.Delete(path);
            }
            catch (Exception ex) { errors.Add($"{Path.GetFileName(path)}：{ex.Message}"); }
        }
        return errors;
    }

    private static void RestoreRecord(WindowRecoveryRecord saved)
    {
        var window = new IntPtr(saved.Window);
        ValidateOwner(window, saved.ProcessId, saved.ProcessStartUtcTicks, saved.ExecutablePath);
        var parent = new IntPtr(saved.Parent);
        if (parent != IntPtr.Zero && !IsWindow(parent)) throw new InvalidOperationException($"原父窗口已失效，保留恢复记录：{saved.Window}");
        SetCheckedParent(window, parent);
        SetStyle(window, GwlStyle, saved.Style);
        SetStyle(window, GwlExStyle, saved.ExStyle);
        var point = new WinPoint { X = saved.Left, Y = saved.Top };
        if (parent != IntPtr.Zero && !ScreenToClient(parent, ref point)) throw new Win32Exception(Marshal.GetLastWin32Error(), "恢复坐标换算失败");
        if (!SetWindowPos(window, IntPtr.Zero, point.X, point.Y, saved.Width, saved.Height, 0x0004 | 0x0010 | 0x0020)) throw new Win32Exception(Marshal.GetLastWin32Error(), "投屏窗口恢复位置失败");
        ShowWindow(window, saved.Visible ? 5 : 0);
        if (GetParent(window) != parent || GetWindowLongPtr(window, GwlStyle).ToInt64() != saved.Style) throw new InvalidOperationException("窗口恢复核验不一致");
    }

    private static void ValidateOwner(IntPtr window, int processId, long started, string executable)
    {
        if (!IsWindow(window)) throw new InvalidOperationException($"投屏窗口已失效：{window}");
        GetWindowThreadProcessId(window, out var actualId);
        if (actualId != processId) throw new InvalidOperationException("窗口 PID 已改变，拒绝修改");
        using var process = Process.GetProcessById(processId);
        if (process.StartTime.ToUniversalTime().Ticks != started || !string.Equals(process.MainModule?.FileName, executable, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("进程启动时间或路径不符，拒绝修改重用窗口");
    }

    private static void SetCheckedParent(IntPtr window, IntPtr parent)
    {
        Marshal.SetLastPInvokeError(0);
        var previous = SetParent(window, parent);
        var error = Marshal.GetLastWin32Error();
        if (previous == IntPtr.Zero && error != 0) throw new Win32Exception(error, "窗口父子关系修改失败");
        if (GetParent(window) != parent) throw new InvalidOperationException("窗口父子关系核验失败");
    }
    private static void SetStyle(IntPtr window, int index, long value)
    {
        Marshal.SetLastPInvokeError(0);
        var result = SetWindowLongPtr(window, index, new IntPtr(value));
        if (result == IntPtr.Zero && Marshal.GetLastWin32Error() != 0) throw new Win32Exception(Marshal.GetLastWin32Error(), "窗口样式修改失败");
    }

    public sealed record WindowRecoveryRecord(long Window, int ProcessId, long ProcessStartUtcTicks, string ExecutablePath, long Parent, long Style, long ExStyle, int Left, int Top, int Width, int Height, bool Visible);
    [StructLayout(LayoutKind.Sequential)] private struct WinRect { public int Left; public int Top; public int Right; public int Bottom; }
    [StructLayout(LayoutKind.Sequential)] private struct WinPoint { public int X; public int Y; }
    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "CreateWindowExW")] private static extern IntPtr CreateWindowEx(int exStyle, string className, string title, long style, int x, int y, int width, int height, IntPtr parent, IntPtr menu, IntPtr instance, IntPtr parameter);
    [DllImport("user32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool DestroyWindow(IntPtr window);
    [DllImport("user32.dll", SetLastError = true)] private static extern IntPtr SetParent(IntPtr window, IntPtr parent);
    [DllImport("user32.dll")] private static extern IntPtr GetParent(IntPtr window);
    [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetWindowLongPtrW", SetLastError = true)] private static extern IntPtr GetWindowLongPtr(IntPtr window, int index);
    [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "SetWindowLongPtrW", SetLastError = true)] private static extern IntPtr SetWindowLongPtr(IntPtr window, int index, IntPtr value);
    [DllImport("user32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool GetWindowRect(IntPtr window, out WinRect rect);
    [DllImport("user32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool SetWindowPos(IntPtr window, IntPtr after, int x, int y, int width, int height, uint flags);
    [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool ShowWindow(IntPtr window, int command);
    [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool IsWindow(IntPtr window);
    [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool IsWindowVisible(IntPtr window);
    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr window, out int processId);
    [DllImport("user32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool ScreenToClient(IntPtr window, ref WinPoint point);
}
