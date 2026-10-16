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
            if (e.Args.Contains("--probe-workspace-client", StringComparer.OrdinalIgnoreCase))
            {
                var observation = WorkspaceClientObservation.Parse(e.Args);
                ShutdownMode = ShutdownMode.OnExplicitShutdown;
                var observationExit = await WorkspaceClientObservation.RunAsync(observation);
                Shutdown(observationExit);
                return;
            }
            if (e.Args.Contains("--verify-script-parameters", StringComparer.OrdinalIgnoreCase))
            {
                ValidateLayoutArguments(e.Args, "--verify-script-parameters", "脚本参数");
                if (e.Args.Any(arg => arg.Equals("--width", StringComparison.OrdinalIgnoreCase) || arg.Equals("--height", StringComparison.OrdinalIgnoreCase) || arg.Equals("--screenshot", StringComparison.OrdinalIgnoreCase)))
                    throw new ArgumentException("脚本参数 VM 验收无窗口，不接受 --width、--height 或 --screenshot。");
                var scriptDirectoryIndex = Array.FindIndex(e.Args, arg => arg.Equals("--data-dir", StringComparison.OrdinalIgnoreCase));
                var scriptDirectory = Path.GetFullPath(e.Args[scriptDirectoryIndex + 1]);
                if (Directory.Exists(scriptDirectory) || File.Exists(scriptDirectory)) throw new ArgumentException("脚本参数 VM 验收必须使用尚不存在的独立 artifacts 目录。");
                ShutdownMode = ShutdownMode.OnExplicitShutdown;
                await ScriptParameterEditingSmoke.RunAsync(scriptDirectory, Dispatcher);
                Shutdown(0);
                return;
            }
            var smoke = e.Args.Contains("--smoke", StringComparer.OrdinalIgnoreCase);
            var shellVerification = e.Args.Contains("--verify-responsive-shell", StringComparer.OrdinalIgnoreCase);
            var deviceLayoutVerification = e.Args.Contains("--verify-device-layout", StringComparer.OrdinalIgnoreCase);
            var monitorLayoutVerification = e.Args.Contains("--verify-monitor-layout", StringComparer.OrdinalIgnoreCase);
            var liveVerification = e.Args.Contains("--verify-live", StringComparer.OrdinalIgnoreCase);
            var unifiedVerification = e.Args.Contains("--verify-unified-access", StringComparer.OrdinalIgnoreCase);
            var oneClickVerification = e.Args.Contains("--verify-one-click-start", StringComparer.OrdinalIgnoreCase);
            var monitorVerification = e.Args.Contains("--verify-monitor", StringComparer.OrdinalIgnoreCase);
            var offlineDiagnosis = e.Args.Contains("--diagnose-offline-geometry", StringComparer.OrdinalIgnoreCase);
            if (new[] { smoke, shellVerification, deviceLayoutVerification, monitorLayoutVerification, liveVerification, unifiedVerification, oneClickVerification, monitorVerification, offlineDiagnosis }.Count(enabled => enabled) > 1)
                throw new ArgumentException("一次只能选择一种自动验收模式。");
            if (deviceLayoutVerification) ValidateLayoutArguments(e.Args, "--verify-device-layout", "设备正文");
            if (monitorLayoutVerification) ValidateLayoutArguments(e.Args, "--verify-monitor-layout", "监控正文");
            if (offlineDiagnosis) ValidateLayoutArguments(e.Args, "--diagnose-offline-geometry", "离线图像诊断");
            string? unifiedCandidate = null;
            var demo = smoke || shellVerification || e.Args.Contains("--demo", StringComparer.OrdinalIgnoreCase);
            var dataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MatrixDevice.Reconstructed", demo ? "demo" : "data");
            var directoryIndex = Array.FindIndex(e.Args, arg => arg.Equals("--data-dir", StringComparison.OrdinalIgnoreCase));
            if (directoryIndex >= 0)
            {
                if (directoryIndex + 1 >= e.Args.Length || !Path.IsPathFullyQualified(e.Args[directoryIndex + 1]))
                    throw new ArgumentException("--data-dir 后必须提供独立配置目录的绝对路径");
                dataDirectory = Path.GetFullPath(e.Args[directoryIndex + 1]);
            }
            if (offlineDiagnosis && (Directory.Exists(dataDirectory) || File.Exists(dataDirectory))) throw new ArgumentException("离线图像诊断必须使用尚不存在的独立 artifacts 目录。");
            if (offlineDiagnosis && e.Args.Contains("--screenshot", StringComparer.OrdinalIgnoreCase)) throw new ArgumentException("离线图像诊断只写 JSON，不接受 --screenshot 参数。");
            Directory.CreateDirectory(dataDirectory);
            var store = new SettingsStore(Path.Combine(dataDirectory, "AppConfig.json"));
            var settings = store.Load() with { Language = "zh-CN" };
            var ldConsoleIndex = Array.FindIndex(e.Args, arg => arg.Equals("--ldconsole", StringComparison.OrdinalIgnoreCase));
            if (ldConsoleIndex >= 0)
            {
                if (ldConsoleIndex + 1 >= e.Args.Length || e.Args[ldConsoleIndex + 1].StartsWith("--", StringComparison.Ordinal)) throw new ArgumentException("--ldconsole 后必须提供雷电命令行程序的完整路径");
                settings = settings with { LdConsolePath = Path.GetFullPath(e.Args[ldConsoleIndex + 1]) };
            }
            if (smoke || shellVerification || deviceLayoutVerification || monitorLayoutVerification || offlineDiagnosis)
            {
                settings = settings with { WindowWidth = 1440, WindowHeight = 900, AutoStartApi = false, AutoConnect = false };
                var widthIndex = Array.FindIndex(e.Args, arg => arg == "--width"); var heightIndex = Array.FindIndex(e.Args, arg => arg == "--height");
                if (deviceLayoutVerification || monitorLayoutVerification || offlineDiagnosis)
                {
                    if (widthIndex >= 0) settings = settings with { WindowWidth = double.Parse(e.Args[widthIndex + 1], System.Globalization.CultureInfo.InvariantCulture) };
                    if (heightIndex >= 0) settings = settings with { WindowHeight = double.Parse(e.Args[heightIndex + 1], System.Globalization.CultureInfo.InvariantCulture) };
                }
                else
                {
                    if (widthIndex >= 0 && widthIndex + 1 < e.Args.Length && double.TryParse(e.Args[widthIndex + 1], out var width)) settings = settings with { WindowWidth = Math.Clamp(width, AppSettings.MinimumWindowWidth, AppSettings.MaximumWindowWidth) };
                    if (heightIndex >= 0 && heightIndex + 1 < e.Args.Length && double.TryParse(e.Args[heightIndex + 1], out var height)) settings = settings with { WindowHeight = Math.Clamp(height, AppSettings.MinimumWindowHeight, AppSettings.MaximumWindowHeight) };
                }
            }
            if (offlineDiagnosis) settings = settings with { AgentBaseUrl = null, AgentSerial = null, SyncMouse = false, SyncKeyboard = false };
            if (unifiedVerification)
            {
                if (directoryIndex < 0 || !dataDirectory.Contains(Path.DirectorySeparatorChar + "artifacts" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("接入验收必须使用artifacts下的独立配置。");
                if (!settings.LdSerialBindings.TryGetValue(2, out var approved) || !settings.AdbAllowedSerials.Contains(approved, StringComparer.Ordinal)) throw new ArgumentException("独立实例2未在复制配置中授权。");
                if (!settings.LdSerialBindings.TryGetValue(1, out var candidate) || !settings.AdbAllowedSerials.Contains(candidate, StringComparer.Ordinal) || candidate == approved) throw new ArgumentException("接入验收仅允许复制配置中已经明确授权的独立实例1/2。");
                unifiedCandidate = candidate;
                settings = settings with { AutoStartApi = false, AutoConnect = false, AdbAllowedSerials = [approved], LdSerialBindings = new() { [2] = approved } };
            }
            if (oneClickVerification)
            {
                if (demo || unifiedVerification || directoryIndex < 0 || !dataDirectory.Contains(Path.DirectorySeparatorChar + "artifacts" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) || !settings.LdSerialBindings.TryGetValue(1, out var candidate) || !settings.AdbAllowedSerials.Contains(candidate, StringComparer.Ordinal)) throw new ArgumentException("一键冷启动验收必须使用包含原批准实例1的独立artifacts配置，不能混用其它验收模式。");
                unifiedCandidate = candidate;
                settings = settings with { AutoStartApi = false, AutoConnect = false, AdbAllowedSerials = [], LdSerialBindings = new(), AgentBaseUrl = null, AgentSerial = null };
            }
            if (monitorVerification && (demo || directoryIndex < 0 || !dataDirectory.Contains(Path.DirectorySeparatorChar + "artifacts" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
                || settings.LdSerialBindings.Count != 2 || !settings.LdSerialBindings.ContainsKey(1) || !settings.LdSerialBindings.ContainsKey(2) || settings.AdbAllowedSerials.Length != 2 || settings.LdSerialBindings.Values.Distinct(StringComparer.Ordinal).Count() != 2
                || !settings.LdSerialBindings.Values.ToHashSet(StringComparer.Ordinal).SetEquals(settings.AdbAllowedSerials) || settings.AutoStartApi || settings.AgentBaseUrl is not null))
                throw new ArgumentException("监控验收必须使用artifacts下仅授权专用实例1、2的配置，禁用API与Agent。");
            runtime = await MatrixRuntime.CreateAsync(settings, dataDirectory, demo);
            var window = new MainWindow(new WorkspaceViewModel(runtime, dataDirectory, demo));
            if (offlineDiagnosis) window.ShowActivated = false;
            MainWindow = window;
            window.Show();
            await window.ViewModel.RefreshDevicesAsync();
            if (!offlineDiagnosis) await window.RecoverBackgroundWindowsAsync();
            window.UpdateUnifiedDirectory();
            if (runtime.Settings.AutoStartApi) await window.ViewModel.StartApisAsync();
            if (e.Args.Contains("--open-monitor", StringComparer.OrdinalIgnoreCase) && !smoke && !shellVerification && !deviceLayoutVerification && !monitorLayoutVerification && !liveVerification && !unifiedVerification && !oneClickVerification && !monitorVerification) window.NavigateTo("实时监控");
            if (offlineDiagnosis)
            {
                window.VerifyDeviceLayoutReportContract();
                await window.VerifyWorkspaceAccessibilityAsync();
                window.VerifyApplicationFeedbackContract();
                await window.RunOfflineGeometryDiagnosticsAsync(e.Args); window.Close();
            }
            if (monitorLayoutVerification)
            {
                await window.RunMonitorLayoutAcceptanceAsync(e.Args); window.Close();
            }
            if (deviceLayoutVerification)
            {
                await window.RunDeviceLayoutAcceptanceAsync(e.Args); window.Close();
            }
            if (shellVerification)
            {
                await window.RunResponsiveShellAcceptanceAsync(e.Args); window.Close();
            }
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
            if (unifiedVerification)
            {
                await window.RunUnifiedAccessAcceptanceAsync(Path.Combine(dataDirectory, "evidence"), unifiedCandidate!, e.Args.Contains("--verify-background-window", StringComparer.OrdinalIgnoreCase)); window.Close();
            }
            if (oneClickVerification)
            {
                await window.RunOneClickStartAcceptanceAsync(Path.Combine(dataDirectory, "evidence"), unifiedCandidate!); window.Close();
            }
            if (monitorVerification)
            {
                await window.RunMonitorAcceptanceAsync(Path.Combine(dataDirectory, "evidence")); window.Close();
            }
        }
        catch (Exception ex)
        {
            if (e.Args.Contains("--probe-workspace-client", StringComparer.OrdinalIgnoreCase))
            {
                Console.Error.WriteLine(ex);
                ShutdownMode = ShutdownMode.OnExplicitShutdown;
                Shutdown(1);
                return;
            }
            if (e.Args.Contains("--verify-script-parameters", StringComparer.OrdinalIgnoreCase) || e.Args.Contains("--smoke", StringComparer.OrdinalIgnoreCase) || e.Args.Contains("--verify-responsive-shell", StringComparer.OrdinalIgnoreCase) || e.Args.Contains("--verify-device-layout", StringComparer.OrdinalIgnoreCase) || e.Args.Contains("--verify-monitor-layout", StringComparer.OrdinalIgnoreCase) || e.Args.Contains("--verify-live", StringComparer.OrdinalIgnoreCase) || e.Args.Contains("--verify-unified-access", StringComparer.OrdinalIgnoreCase) || e.Args.Contains("--verify-one-click-start", StringComparer.OrdinalIgnoreCase) || e.Args.Contains("--verify-monitor", StringComparer.OrdinalIgnoreCase) || e.Args.Contains("--diagnose-offline-geometry", StringComparer.OrdinalIgnoreCase)) Console.Error.WriteLine(ex);
            else MessageBox.Show(UiErrorPresentation.Describe(ex), "矩阵设备启动失败", MessageBoxButton.OK, MessageBoxImage.Error);
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            try
            {
                if (MainWindow is MatrixDevice.Desktop.MainWindow failedWindow) { await failedWindow.StopMonitorAfterFailureAsync(); failedWindow.AbortStartup(); }
                if (runtime is not null) await runtime.DisposeAsync();
            }
            catch (Exception cleanupError) { Console.Error.WriteLine("启动失败后的资源清理异常：" + cleanupError); }
            finally { Shutdown(1); }
        }
    }

    private static void ValidateLayoutArguments(string[] args, string mode, string context)
    {
        var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        for (var index = 0; index < args.Length; index++)
        {
            var key = args[index];
            if (key != mode && key is not ("--demo" or "--data-dir" or "--width" or "--height" or "--screenshot") || values.ContainsKey(key)) throw new ArgumentException(context + "验收参数未知或重复：" + key);
            if (key == mode || key == "--demo") { values.Add(key, null); continue; }
            if (++index >= args.Length || args[index].StartsWith("--", StringComparison.Ordinal)) throw new ArgumentException(context + "验收参数缺值：" + key);
            values.Add(key, args[index]);
        }
        if (!values.ContainsKey("--demo") || !values.TryGetValue("--data-dir", out var directory) || directory is null || !Path.IsPathFullyQualified(directory)
            || !Path.GetFullPath(directory).Contains(Path.DirectorySeparatorChar + "artifacts" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) throw new ArgumentException(context + "验收必须显式指定--demo与artifacts下独立绝对--data-dir。");
        foreach (var (key, minimum, maximum) in new[] { ("--width", AppSettings.MinimumWindowWidth, AppSettings.MaximumWindowWidth), ("--height", AppSettings.MinimumWindowHeight, AppSettings.MaximumWindowHeight) })
            if (values.TryGetValue(key, out var value) && (!double.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var number) || !double.IsFinite(number) || number < minimum || number > maximum)) throw new ArgumentException(context + "验收尺寸无效：" + key + "=" + value);
        if (values.TryGetValue("--screenshot", out var screenshot) && !Path.IsPathFullyQualified(screenshot!)) throw new ArgumentException("截图路径必须为绝对路径。");
    }
}
