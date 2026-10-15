using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MatrixDevice.Core;
using MatrixDevice.Features;
using MatrixDevice.Host;
using Microsoft.Win32;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    private sealed record MainScreenTarget(MatrixRuntime Runtime, DeviceRow Row, string Serial, long Generation, uint? TransportId, long InputGeneration);
    private sealed record ScrcpySession(MainScreenTarget Target, Process Process, int ProcessId, long BirthUtcTicks, string ExecutablePath)
    {
        public bool IsCurrentProcess()
        {
            try
            {
                if (Process.HasExited || Process.Id != ProcessId) return false;
                using var current = Process.GetProcessById(ProcessId);
                return !current.HasExited && current.StartTime.ToUniversalTime().Ticks == BirthUtcTicks
                    && string.Equals(current.MainModule?.FileName, ExecutablePath, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or Win32Exception) { return false; }
        }
    }

    private MainScreenTarget? MainScreenSnapshot()
    {
        if (ViewModel.Busy || ViewModel.MainDevice is not { } row) return null;
        var target = new MainScreenTarget(ViewModel.Runtime, row, row.Serial, row.Info.Generation, row.Info.TransportId, ViewModel.Runtime.Input.CurrentGeneration);
        try { RequireMainScreenTarget(target); return target; }
        catch (DeviceException) { return null; }
    }

    private void RequireMainScreenTarget(MainScreenTarget target)
    {
        if (closing || ldBusy || !ReferenceEquals(target.Runtime, ViewModel.Runtime) || !ReferenceEquals(target.Row, ViewModel.MainDevice)
            || !ViewModel.Devices.Contains(target.Row) || ViewModel.Transferring || target.Runtime.Api.HasActiveRequests
            || target.Runtime.Scripts.GetStatus(target.Serial).State is ScriptRunState.Running or ScriptRunState.Cancelling
            || target.Runtime.Input.CurrentGeneration != target.InputGeneration || !SameScreenDevice(target.Row.Info, target)
            || !SameScreenDevice(target.Runtime.GetDevices().SingleOrDefault(row => row.Serial == target.Serial), target))
            throw new DeviceException($"主控或连接身份已变化，未继续投屏：{target.Serial}。", 409, 1003);
        target.Runtime.RequireFrozenConnectionSettings();
    }

    private static bool SameScreenDevice(DeviceInfo? device, MainScreenTarget target) => device is { Online: true }
        && device.Serial == target.Serial && device.Generation == target.Generation && device.TransportId == target.TransportId;

    private async Task VerifyMainScreenIdentityAsync(MainScreenTarget target)
    {
        RequireMainScreenTarget(target);
        if (target.Runtime.Backend is AdbDeviceBackend backend) await backend.VerifyIdentityAsync(target.Serial);
        RequireMainScreenTarget(target);
    }

    private bool CanLaunchScrcpy() => !ViewModel.Demo && MainScreenSnapshot() is { } target && File.Exists(target.Runtime.Settings.ScrcpyPath)
        && (!screenProcesses.TryGetValue(target.Serial, out var session) || !session.IsCurrentProcess());
    private bool CanEmbedScrcpy() => MainScreenSnapshot() is { } target && screenProcesses.TryGetValue(target.Serial, out var session)
        && SessionMatchesTarget(session, target) && !nativeHosts.Any(host => host.ProcessId == session.ProcessId && host.IsAttached);

    private static bool SessionMatchesTarget(ScrcpySession session, MainScreenTarget target) => ReferenceEquals(session.Target.Runtime, target.Runtime)
        && ReferenceEquals(session.Target.Row, target.Row) && session.Target.Serial == target.Serial && session.Target.Generation == target.Generation
        && session.Target.TransportId == target.TransportId && session.IsCurrentProcess();

    private void RequireScrcpySession(MainScreenTarget target, ScrcpySession session)
    {
        RequireMainScreenTarget(target);
        if (!screenProcesses.TryGetValue(target.Serial, out var owned) || !ReferenceEquals(session, owned) || !SessionMatchesTarget(session, target))
            throw new DeviceException($"投屏进程或设备身份已变化，未嵌入窗口：{target.Serial}。", 409, 1003);
        if (nativeHosts.Any(host => host.ProcessId == session.ProcessId && host.IsAttached)) throw new InvalidOperationException("窗口已嵌入");
    }
    private async Task OpenScreenAsync()
    {
        if (!CanLaunchScrcpy()) return;
        var target = MainScreenSnapshot() ?? throw new DeviceException("主控不可用，请重新打开控制面板。", 409, 1003);
        var settings = target.Runtime.Settings;
        if (!ViewModel.Confirm("外部投屏客户端风险", "启动scrcpy会运行其外部ADB客户端，可能影响现有ADB服务。当前程序会在服务身份变化时拒绝继续。确认已核验该工具并接受此风险？")) return;
        await ViewModel.ExecuteAsync("启动 scrcpy", async () =>
        {
            if (ViewModel.Demo) throw new InvalidOperationException("演示模式不启动外部 scrcpy");
            await VerifyMainScreenIdentityAsync(target);
            var path = System.IO.Path.GetFullPath(settings.ScrcpyPath);
            if (!File.Exists(path)) throw new FileNotFoundException("请在设置中配置有效 scrcpy 路径", path);
            if (screenProcesses.TryGetValue(target.Serial, out var existing) && existing.IsCurrentProcess()) throw new InvalidOperationException("该设备已有本程序启动的投屏窗口");
            var start = new ProcessStartInfo(path) { UseShellExecute = false, CreateNoWindow = true };
            foreach (var argument in new[] { "--serial", target.Serial, "--window-title", "MatrixDevice · " + target.Serial, "--max-size", settings.MainMaxSize.ToString(), "--max-fps", settings.MainMaxFps.ToString(), "--video-bit-rate", settings.MainBitRate.ToString(), "--no-audio" }) start.ArgumentList.Add(argument);
            if (settings.ShowFps) start.ArgumentList.Add("--print-fps");
            var process = Process.Start(start) ?? throw new InvalidOperationException("无法创建 scrcpy 进程");
            screenProcesses[target.Serial] = new(target, process, process.Id, process.StartTime.ToUniversalTime().Ticks, path);
            target.Row.ScreenState = "scrcpy 进程已启动 · 待窗口核验";
            await Task.Delay(400);
            RequireMainScreenTarget(target);
            if (process.HasExited) throw new InvalidOperationException($"scrcpy 已退出，退出码 {process.ExitCode}");
        });
    }

    private async Task EmbedScreenAsync()
    {
        if (!CanEmbedScrcpy()) return;
        var target = MainScreenSnapshot() ?? throw new DeviceException("主控不可用，请重新打开控制面板。", 409, 1003);
        var session = screenProcesses[target.Serial];
        await ViewModel.ExecuteAsync("嵌入投屏窗口", async () =>
        {
            await VerifyMainScreenIdentityAsync(target);
            RequireScrcpySession(target, session);
            var process = session.Process;
            for (var retry = 0; retry < 20 && process.MainWindowHandle == IntPtr.Zero; retry++)
            {
                await Task.Delay(100);
                RequireScrcpySession(target, session);
                process.Refresh();
            }
            if (process.MainWindowHandle == IntPtr.Zero) throw new InvalidOperationException("投屏窗口尚未出现");
            await VerifyMainScreenIdentityAsync(target);
            RequireScrcpySession(target, session);
            var window = new Window { Owner = this, Title = "投屏 · " + target.Serial, Width = 600, Height = 850, WindowStartupLocation = WindowStartupLocation.CenterOwner };
            ResponsiveLayout.FitOwnedWindow(window, this, 600, 850);
            var host = new NativeWindowHost(process, System.IO.Path.Combine(ViewModel.DataDirectory, "window-recovery"));
            window.Content = host; nativeHosts.Add(host);
            window.Closing += (_, args) => { try { host.Restore(); nativeHosts.Remove(host); } catch (Exception ex) { args.Cancel = true; MessageBox.Show(window, ViewModel.PresentError(ex), "窗口恢复失败", MessageBoxButton.OK, MessageBoxImage.Error); } };
            window.Show(); target.Row.ScreenState = "已嵌入自有 scrcpy 窗口";
        });
    }

    private Task RestoreScreensAsync() => ViewModel.ExecuteAsync("恢复投屏窗口", async () =>
    {
        var failures = new List<string>(); foreach (var host in nativeHosts.ToList()) { try { host.Restore(); } catch (Exception ex) { failures.Add(ViewModel.PresentError(ex)); } }
        if (failures.Count > 0) throw new InvalidOperationException(string.Join("\n", failures)); await Task.CompletedTask;
    });

}
