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
        Dictionary<string, string?> options;
        try { options = ParseArguments(e.Args); }
        catch (ArgumentException error)
        {
            Console.Error.WriteLine(error.Message);
            Shutdown(2);
            return;
        }
        if (options.ContainsKey("--help"))
        {
            Console.WriteLine("MatrixDevice.Desktop [--data-dir 绝对路径] [--ldconsole 绝对路径] [--open-monitor]");
            Shutdown(0);
            return;
        }
        try
        {
            var directory = options.GetValueOrDefault("--data-dir") ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MatrixDevice.Reconstructed", "data");
            var settings = new SettingsStore(Path.Combine(directory, "AppConfig.json")).Load() with { Language = "zh-CN" };
            if (options.GetValueOrDefault("--ldconsole") is { } console) settings = settings with { LdConsolePath = console };
            runtime = await MatrixRuntime.CreateAsync(settings, directory);
            var window = new MainWindow(new WorkspaceViewModel(runtime, directory, false));
            MainWindow = window;
            window.Show();
            await window.ViewModel.RefreshDevicesAsync();
            await window.RecoverBackgroundWindowsAsync();
            window.UpdateUnifiedDirectory();
            if (runtime.Settings.AutoStartApi) await window.ViewModel.StartApisAsync();
            if (options.ContainsKey("--open-monitor")) window.NavigateTo("实时监控");
        }
        catch (Exception error)
        {
            MessageBox.Show(UiErrorPresentation.Describe(error), "矩阵设备启动失败", MessageBoxButton.OK, MessageBoxImage.Error);
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            try
            {
                if (MainWindow is MatrixDevice.Desktop.MainWindow window)
                {
                    await window.StopMonitorAfterFailureAsync();
                    window.AbortStartup();
                }
                if (runtime is not null) await runtime.DisposeAsync();
            }
            catch (Exception cleanupError) { Console.Error.WriteLine("启动失败后的资源清理异常：" + cleanupError); }
            finally { Shutdown(1); }
        }
    }

    private static Dictionary<string, string?> ParseArguments(string[] args)
    {
        var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        for (var index = 0; index < args.Length; index++)
        {
            var key = args[index];
            if (key is not ("--data-dir" or "--ldconsole" or "--open-monitor" or "--help") || !values.TryAdd(key, null))
                throw new ArgumentException("正式版本不接受未知、重复或测试参数：" + key);
            if (key is "--open-monitor" or "--help") continue;
            if (++index >= args.Length || args[index].StartsWith("--", StringComparison.Ordinal) || !Path.IsPathFullyQualified(args[index]))
                throw new ArgumentException(key + " 后必须提供绝对路径。");
            if (key == "--ldconsole" && !File.Exists(args[index])) throw new ArgumentException("雷电命令行文件不存在：" + args[index]);
            values[key] = Path.GetFullPath(args[index]);
        }
        return values;
    }
}
