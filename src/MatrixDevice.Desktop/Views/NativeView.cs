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
using Microsoft.Win32;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    private bool CanLaunchScrcpy() => !ViewModel.Demo && ViewModel.CanWriteDevice() && File.Exists(ViewModel.Runtime.Settings.ScrcpyPath) &&
        (!screenProcesses.TryGetValue(ViewModel.CurrentDevice!.Serial, out var process) || process.HasExited);
    private bool CanEmbedScrcpy() => ViewModel.CanWriteDevice() && screenProcesses.TryGetValue(ViewModel.CurrentDevice!.Serial, out var process) && !process.HasExited &&
        !nativeHosts.Any(host => host.ProcessId == process.Id && host.IsAttached);
    private async Task OpenScreenAsync()
    {
        if (!CanLaunchScrcpy()) return;
        if (!ViewModel.Confirm("外部投屏客户端风险", "启动scrcpy会运行其外部ADB客户端，可能影响现有ADB服务。当前程序会在服务身份变化时拒绝继续。确认已核验该工具并接受此风险？")) return;
        await ViewModel.ExecuteAsync("启动 scrcpy", async () =>
        {
            var row = ViewModel.CurrentDevice ?? throw new InvalidOperationException("未选择设备");
            if (ViewModel.Demo) throw new InvalidOperationException("演示模式不启动外部 scrcpy");
            var path = ViewModel.Runtime.Settings.ScrcpyPath;
            if (!File.Exists(path)) throw new FileNotFoundException("请在设置中配置有效 scrcpy 路径", path);
            if (screenProcesses.TryGetValue(row.Serial, out var existing) && !existing.HasExited) throw new InvalidOperationException("该设备已有本程序启动的投屏窗口");
            var start = new ProcessStartInfo(path) { UseShellExecute = false, CreateNoWindow = true };
            foreach (var argument in new[] { "--serial", row.Serial, "--window-title", "MatrixDevice · " + row.Serial, "--max-size", ViewModel.Runtime.Settings.MainMaxSize.ToString(), "--max-fps", ViewModel.Runtime.Settings.MainMaxFps.ToString(), "--video-bit-rate", ViewModel.Runtime.Settings.MainBitRate.ToString(), "--no-audio" }) start.ArgumentList.Add(argument);
            if (ViewModel.Runtime.Settings.ShowFps) start.ArgumentList.Add("--print-fps");
            var process = Process.Start(start) ?? throw new InvalidOperationException("无法创建 scrcpy 进程"); screenProcesses[row.Serial] = process; row.ScreenState = "scrcpy 进程已启动 · 待窗口核验";
            await Task.Delay(400); if (process.HasExited) throw new InvalidOperationException($"scrcpy 已退出，退出码 {process.ExitCode}");
        });
    }

    private async Task EmbedScreenAsync()
    {
        await ViewModel.ExecuteAsync("嵌入投屏窗口", async () =>
        {
            var row = ViewModel.CurrentDevice ?? throw new InvalidOperationException("未选择设备");
            if (!screenProcesses.TryGetValue(row.Serial, out var process) || process.HasExited) throw new InvalidOperationException("请先启动本程序管理的 scrcpy 投屏");
            if (nativeHosts.Any(host => host.ProcessId == process.Id && host.IsAttached)) throw new InvalidOperationException("窗口已嵌入");
            for (var retry = 0; retry < 20 && process.MainWindowHandle == IntPtr.Zero; retry++) { await Task.Delay(100); process.Refresh(); }
            if (process.MainWindowHandle == IntPtr.Zero) throw new InvalidOperationException("投屏窗口尚未出现");
            var host = new NativeWindowHost(process, System.IO.Path.Combine(ViewModel.DataDirectory, "window-recovery"));
            nativeHosts.Add(host);
            var window = new Window { Owner = this, Title = "投屏 · " + row.Serial, Width = 600, Height = 850, Content = host, WindowStartupLocation = WindowStartupLocation.CenterOwner };
            window.Closing += (_, args) => { try { host.Restore(); nativeHosts.Remove(host); } catch (Exception ex) { args.Cancel = true; MessageBox.Show(window, ViewModel.PresentError(ex), "窗口恢复失败", MessageBoxButton.OK, MessageBoxImage.Error); } };
            window.Show(); row.ScreenState = "已嵌入自有 scrcpy 窗口";
        });
    }

    private Task RestoreScreensAsync() => ViewModel.ExecuteAsync("恢复投屏窗口", async () =>
    {
        var failures = new List<string>(); foreach (var host in nativeHosts.ToList()) { try { host.Restore(); } catch (Exception ex) { failures.Add(ViewModel.PresentError(ex)); } }
        if (failures.Count > 0) throw new InvalidOperationException(string.Join("\n", failures)); await Task.CompletedTask;
    });

}
