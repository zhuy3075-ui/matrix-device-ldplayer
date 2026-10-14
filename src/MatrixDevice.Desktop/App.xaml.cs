using System.IO;
using System.Windows;
using MatrixDevice.Core;
using MatrixDevice.Host;

namespace MatrixDevice.Desktop;

public partial class App : Application
{
    private MatrixRuntime? runtime;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        try
        {
            var smoke = e.Args.Contains("--smoke", StringComparer.OrdinalIgnoreCase);
            var liveVerification = e.Args.Contains("--verify-live", StringComparer.OrdinalIgnoreCase);
            var demo = smoke || e.Args.Contains("--demo", StringComparer.OrdinalIgnoreCase);
            var dataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MatrixDevice.Reconstructed", demo ? "demo" : "data");
            var directoryIndex = Array.FindIndex(e.Args, arg => arg.Equals("--data-dir", StringComparison.OrdinalIgnoreCase));
            if (directoryIndex >= 0)
            {
                if (directoryIndex + 1 >= e.Args.Length || !Path.IsPathFullyQualified(e.Args[directoryIndex + 1]))
                    throw new ArgumentException("--data-dir 后必须提供独立配置目录的绝对路径");
                dataDirectory = Path.GetFullPath(e.Args[directoryIndex + 1]);
            }
            Directory.CreateDirectory(dataDirectory);
            var store = new SettingsStore(Path.Combine(dataDirectory, "AppConfig.json"));
            var settings = store.Load() with { Language = "zh-CN" };
            var ldConsoleIndex = Array.FindIndex(e.Args, arg => arg.Equals("--ldconsole", StringComparison.OrdinalIgnoreCase));
            if (ldConsoleIndex >= 0)
            {
                if (ldConsoleIndex + 1 >= e.Args.Length || e.Args[ldConsoleIndex + 1].StartsWith("--", StringComparison.Ordinal)) throw new ArgumentException("--ldconsole 后必须提供雷电命令行程序的完整路径");
                settings = settings with { LdConsolePath = Path.GetFullPath(e.Args[ldConsoleIndex + 1]) };
            }
            if (smoke)
            {
                settings = settings with { WindowWidth = 1440, WindowHeight = 900, AutoStartApi = false, AutoConnect = false };
                var widthIndex = Array.FindIndex(e.Args, arg => arg == "--width"); var heightIndex = Array.FindIndex(e.Args, arg => arg == "--height");
                if (widthIndex >= 0 && widthIndex + 1 < e.Args.Length && double.TryParse(e.Args[widthIndex + 1], out var width)) settings = settings with { WindowWidth = Math.Clamp(width, 1000, 2400) };
                if (heightIndex >= 0 && heightIndex + 1 < e.Args.Length && double.TryParse(e.Args[heightIndex + 1], out var height)) settings = settings with { WindowHeight = Math.Clamp(height, 700, 1800) };
            }
            runtime = await MatrixRuntime.CreateAsync(settings, dataDirectory, demo);
            var window = new MainWindow(new WorkspaceViewModel(runtime, dataDirectory, demo));
            MainWindow = window;
            window.Show();
            if (demo || runtime.Settings.AutoConnect) await window.ViewModel.RefreshDevicesAsync();
            if (runtime.Settings.AutoStartApi) await window.ViewModel.StartApisAsync();
            if (smoke)
            {
                var screenshotIndex = Array.FindIndex(e.Args, arg => arg.Equals("--screenshot", StringComparison.OrdinalIgnoreCase));
                var screenshotPath = screenshotIndex >= 0 && screenshotIndex + 1 < e.Args.Length ? e.Args[screenshotIndex + 1] : null;
                await window.RunSmokeAsync(screenshotPath);
                window.Close();
            }
            if (liveVerification)
            {
                var outputIndex = Array.FindIndex(e.Args, arg => arg.Equals("--evidence-dir", StringComparison.OrdinalIgnoreCase));
                if (directoryIndex < 0 || outputIndex < 0 || outputIndex + 1 >= e.Args.Length) throw new ArgumentException("实机界面验收必须提供独立 --data-dir 和绝对 --evidence-dir");
                await window.RunLiveUiAcceptanceAsync(e.Args[outputIndex + 1]); window.Close();
            }
        }
        catch (Exception ex)
        {
            if (e.Args.Contains("--smoke", StringComparer.OrdinalIgnoreCase) || e.Args.Contains("--verify-live", StringComparer.OrdinalIgnoreCase)) Console.Error.WriteLine(ex);
            else MessageBox.Show(UiErrorPresentation.Describe(ex), "矩阵设备启动失败", MessageBoxButton.OK, MessageBoxImage.Error);
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            try
            {
                if (MainWindow is MatrixDevice.Desktop.MainWindow failedWindow) failedWindow.AbortStartup();
                if (runtime is not null) await runtime.DisposeAsync();
            }
            catch (Exception cleanupError) { Console.Error.WriteLine("启动失败后的资源清理异常：" + cleanupError); }
            finally { Shutdown(1); }
        }
    }
}
