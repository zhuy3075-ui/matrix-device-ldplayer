using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Threading;
using MatrixDevice.Core;

namespace MatrixDevice.Desktop;

public static class LdWindowVisibilitySmoke
{
    public static async Task VerifyAsync(string evidenceDir)
    {
        if (!Path.IsPathFullyQualified(evidenceDir)) throw new ArgumentException("窗口自测证据目录必须为绝对路径。");
        if (Application.Current is null || !Application.Current.Dispatcher.CheckAccess())
            throw new InvalidOperationException("窗口自测必须在本程序 WPF Dispatcher 上运行。");
        var directory = Path.Combine(evidenceDir, "self-window-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        using var process = Process.GetCurrentProcess();
        var birth = process.StartTime.ToUniversalTime().Ticks;
        var executable = process.MainModule?.FileName ?? throw new InvalidOperationException("无法读取自测进程路径。");
        var install = Path.GetDirectoryName(executable)!;
        var visibility = new LdWindowVisibility(directory, install);
        var passed = new List<string>();
        var windows = new List<Window>();
        var window = MakeWindow(); windows.Add(window);
        await RenderAsync();
        var handle = new WindowInteropHelper(window).Handle;
        var instance = new LdPlayerInstance(1, "本程序窗口自测，不是真实模拟器", handle.ToInt64(), 0, false, process.Id, -1);
        var parent = GetParent(handle);
        var style = GetWindowLongPtr(handle, -16).ToInt64();
        var exStyle = GetWindowLongPtr(handle, -20).ToInt64();
        try
        {
            Assert(IsWindowVisible(handle), "自测窗口没有正常显示。");
            await ExpectAsync(400, () => visibility.HideAsync(instance with { Index = 0 }, birth));
            await ExpectAsync(409, () => visibility.HideAsync(instance, birth + 1));
            await ExpectAsync(409, () => visibility.HideAsync(instance with { ProcessId = int.MaxValue }, birth));
            Assert(IsWindowVisible(handle) && JournalCount(directory) == 0, "错误身份触碰了窗口或建立恢复记录。");
            passed.Add("protected-index-and-wrong-owner-refused");

            var outside = Path.Combine(directory, "empty-sdk"); Directory.CreateDirectory(outside);
            var wrongInstall = new LdWindowVisibility(Path.Combine(directory, "wrong-install"), outside);
            await ExpectAsync(403, () => wrongInstall.HideAsync(instance, birth));
            Assert(IsWindowVisible(handle), "安装路径拒绝后窗口被隐藏。");
            passed.Add("executable-outside-frozen-install-refused");

            Assert(await visibility.HideAsync(instance, birth), "自有可见窗口未获得隐藏所有权。");
            Assert(!IsWindowVisible(handle) && visibility.IsHidden(1) && JournalCount(directory) == 1, "隐藏状态或预写恢复记录不正确。");
            Assert((GetWindowLongPtr(handle, -16).ToInt64() & ~0x10000000L) == (style & ~0x10000000L) && GetParent(handle) == parent && GetWindowLongPtr(handle, -20).ToInt64() == exStyle,
                "隐藏改变了可见位以外的样式或父窗口。");
            await visibility.ShowAsync(1);
            Assert(IsWindowVisible(handle) && !visibility.IsHidden(1) && JournalCount(directory) == 0 && GetForegroundWindow() != handle,
                "恢复未显示自有窗口、遗留记录或激活了窗口。");
            Assert(GetWindowLongPtr(handle, -16).ToInt64() == style && GetParent(handle) == parent && GetWindowLongPtr(handle, -20).ToInt64() == exStyle,
                "恢复改变了原窗口样式或父窗口。");
            passed.Add("journal-before-hide-and-nonactivating-show");

            window.Hide(); await RenderAsync();
            Assert(!await visibility.HideAsync(instance, birth) && !visibility.IsHidden(1) && JournalCount(directory) == 0,
                "此前已经隐藏的窗口被本组件占有。");
            await visibility.ShowAsync(1);
            Assert(!IsWindowVisible(handle), "显示操作修改了并非本组件隐藏的窗口。");
            window.Show(); await RenderAsync();
            passed.Add("preexisting-hidden-window-not-owned");

            await visibility.HideAsync(instance, birth);
            var recordPath = Directory.EnumerateFiles(directory, "ld-window-*.json", SearchOption.AllDirectories).Single();
            var recordText = await File.ReadAllTextAsync(recordPath);
            var record = JsonNode.Parse(recordText)!.AsObject();
            var marker = record["marker"]!.GetValue<string>();
            RemoveProp(handle, marker);
            await ExpectAsync(409, () => visibility.ShowAsync(1));
            Assert(!IsWindowVisible(handle) && File.Exists(recordPath), "窗口标记丢失后仍恢复窗口或删除恢复记录。");
            if (!SetProp(handle, marker, new IntPtr(1))) throw new Win32Exception(Marshal.GetLastWin32Error(), "无法恢复自测窗口标记");
            await visibility.ShowAsync(1);
            passed.Add("missing-marker-reuse-guard-refused");

            await visibility.HideAsync(instance, birth);
            recordPath = Directory.EnumerateFiles(directory, "ld-window-*.json", SearchOption.AllDirectories).Single();
            recordText = await File.ReadAllTextAsync(recordPath);
            record = JsonNode.Parse(recordText)!.AsObject();
            record["processStartUtcTicks"] = birth + 1;
            await File.WriteAllTextAsync(recordPath, record.ToJsonString(JsonDefaults.Options));
            var recovery = new LdWindowVisibility(directory, install);
            var refused = await recovery.RecoverPendingAsync();
            Assert(refused.Count == 1 && !IsWindowVisible(handle) && File.Exists(recordPath), "恢复错误出生时刻的记录没有被拒绝。");
            await File.WriteAllTextAsync(recordPath, recordText);
            Assert((await recovery.RecoverPendingAsync()).Count == 0 && IsWindowVisible(handle) && JournalCount(directory) == 0,
                "同进程恢复记录回放失败。");
            await visibility.RestoreAllAsync();
            passed.Add("journal-replay-and-wrong-birth-refusal");

            var second = MakeWindow(); windows.Add(second); await RenderAsync();
            var secondHandle = new WindowInteropHelper(second).Handle;
            var secondInstance = instance with { Index = 2, TopWindow = secondHandle.ToInt64() };
            await visibility.HideAsync(instance, birth);
            await visibility.HideAsync(secondInstance, birth);
            await visibility.RestoreAllAsync();
            Assert(IsWindowVisible(handle) && IsWindowVisible(secondHandle) && JournalCount(directory) == 0,
                "批量恢复没有恢复所有自有窗口。");
            passed.Add("restore-all-owned-windows");

            await visibility.HideAsync(secondInstance, birth);
            second.Close(); windows.Remove(second);
            await visibility.ShowAsync(2);
            Assert(JournalCount(directory) == 0 && !visibility.IsHidden(2), "窗口关闭后未安全清理自有恢复记录。");
            passed.Add("closed-window-record-cleanup");

            using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
            try { await visibility.HideAsync(instance, birth, cancellation.Token); throw new InvalidOperationException("取消状态被忽略。"); }
            catch (OperationCanceledException) when (cancellation.IsCancellationRequested) { }
            Assert(IsWindowVisible(handle) && JournalCount(directory) == 0, "已取消操作仍隐藏了窗口。");
            passed.Add("pre-cancelled-hide-no-mutation");

            await File.WriteAllTextAsync(Path.Combine(directory, "result.json"), JsonSerializer.Serialize(new
            {
                subject = "own-process-wpf-windows-only", processId = process.Id, processStartUtcTicks = birth,
                executable, externalWindowsTouched = false, realLdPlayerTested = false,
                desktopSha256 = Convert.ToHexString(SHA256.HashData(await File.ReadAllBytesAsync(typeof(LdWindowVisibility).Assembly.Location))),
                passed, count = passed.Count, pendingRecords = JournalCount(directory)
            }, JsonDefaults.Options));
        }
        finally
        {
            foreach (var ownWindow in windows) ownWindow.Close();
            await visibility.RestoreAllAsync();
        }
    }

    private static Window MakeWindow()
    {
        var window = new Window
        {
            Title = "本程序窗口恢复自测", Width = 280, Height = 120, Left = -10000, Top = -10000,
            WindowStartupLocation = WindowStartupLocation.Manual, ShowActivated = false, ShowInTaskbar = false,
            Content = new TextBlock { Text = "本程序创建的测试窗口，不是真实雷电实例。", TextWrapping = TextWrapping.Wrap }
        };
        window.Show(); return window;
    }
    private static async Task RenderAsync() => await Application.Current.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Render);
    private static int JournalCount(string directory) => Directory.EnumerateFiles(directory, "ld-window-*.json", SearchOption.AllDirectories).Count();
    private static void Assert(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    private static async Task ExpectAsync(int status, Func<Task> action)
    {
        try { await action(); }
        catch (DeviceException error) when (error.Status == status) { return; }
        throw new InvalidOperationException($"预期窗口操作被拒绝，HTTP 类状态码={status}。");
    }
    [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool IsWindowVisible(IntPtr window);
    [DllImport("user32.dll")] private static extern IntPtr GetParent(IntPtr window);
    [DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetWindowLongPtrW")] private static extern IntPtr GetWindowLongPtr(IntPtr window, int index);
    [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "SetPropW", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool SetProp(IntPtr window, string name, IntPtr value);
    [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "RemovePropW")] private static extern IntPtr RemoveProp(IntPtr window, string name);
}
