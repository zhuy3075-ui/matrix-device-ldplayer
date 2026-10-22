using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using MatrixDevice.Core;
using MatrixDevice.Host;

namespace MatrixDevice.Desktop;

internal static class WorkspaceClientObservation
{
    internal sealed record Options(string Directory, string Nonce, string Arm, double Width, double Height);
    internal static Options Parse(string[] args)
    {
        var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        for (var index = 0; index < args.Length; index++)
        {
            var key = args[index];
            if (key is not ("--probe-workspace-client" or "--demo" or "--data-dir" or "--nonce" or "--arm" or "--width" or "--height") || values.ContainsKey(key))
                throw new ArgumentException("客户端观察参数未知、混合或重复：" + key);
            if (key is "--probe-workspace-client" or "--demo") { values.Add(key, null); continue; }
            if (++index >= args.Length || args[index].StartsWith("--", StringComparison.Ordinal)) throw new ArgumentException("客户端观察参数缺值：" + key);
            values.Add(key, args[index]);
        }
        string Required(string key) => values.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value : throw new ArgumentException("客户端观察必须显式指定：" + key);
        if (!values.ContainsKey("--probe-workspace-client") || !values.ContainsKey("--demo")) throw new ArgumentException("客户端观察必须显式指定模式及 --demo。");
        var directory = Required("--data-dir");
        if (!Path.IsPathFullyQualified(directory)) throw new ArgumentException("客户端观察目录必须是绝对路径。");
        directory = Path.GetFullPath(directory);
        if (!directory.Contains(Path.DirectorySeparatorChar + "artifacts" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
            || System.IO.Directory.Exists(directory) || File.Exists(directory)) throw new ArgumentException("客户端观察必须使用尚不存在的独立 artifacts 目录。");
        var nonce = Required("--nonce");
        if (nonce.Length != 32 || nonce.Any(character => !Uri.IsHexDigit(character))) throw new ArgumentException("--nonce 必须是32位十六进制。");
        var arm = Required("--arm"); if (arm is not ("client" or "control")) throw new ArgumentException("--arm 必须是 client 或 control。");
        double Dimension(string key, double minimum, double maximum) => double.TryParse(Required(key), NumberStyles.Float, CultureInfo.InvariantCulture, out var value)
            && double.IsFinite(value) && value >= minimum && value <= maximum ? value : throw new ArgumentException("客户端观察尺寸无效：" + key);
        return new(directory, nonce.ToLowerInvariant(), arm, Dimension("--width", AppSettings.MinimumWindowWidth, AppSettings.MaximumWindowWidth), Dimension("--height", AppSettings.MinimumWindowHeight, AppSettings.MaximumWindowHeight));
    }

    internal static async Task<int> RunAsync(Options options)
    {
        var application = Application.Current ?? throw new InvalidOperationException("自有 Application 不存在。");
        if (!application.Dispatcher.CheckAccess() || Thread.CurrentThread.GetApartmentState() != ApartmentState.STA || application.Windows.Count != 0 || application.MainWindow is not null)
            throw new InvalidOperationException("客户端观察仅限尚无窗口的自有 STA Application。");
        if (Directory.Exists(options.Directory) || File.Exists(options.Directory)) throw new InvalidOperationException("客户端观察目录已存在。");
        Directory.CreateDirectory(options.Directory);
        using var lifetime = new CancellationTokenSource(); using var process = Process.GetCurrentProcess();
        var loaded = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously); var tasks = new List<Task>(); var cleanupErrors = new List<string>();
        var classes = new HashSet<string>(StringComparer.Ordinal); var phase = "setup"; Exception? failure = null; MatrixRuntime? ownedRuntime = null; WorkspaceViewModel? vm = null; MainWindow? window = null;
        Task? cleanupTask = null; var cleanupCalls = 0; var userCloseRequested = false; var controlledAbort = false; var closed = false; var readyPublished = false;
        object? before = null, after = null; JsonElement? contract = null; string? readySha = null, releaseIntent = null; double? elapsedMs = null; long? readyTicks = null, releaseTicks = null;
        var startUtc = ""; var assemblyPath = typeof(App).Assembly.Location; var assemblySha = ""; var title = "MatrixDevice Workspace Probe " + options.Arm + " " + options.Nonce;
        CancelEventHandler closing = (_, args) => { args.Cancel = true; userCloseRequested = true; lifetime.Cancel(); };
        RoutedEventHandler onLoaded = (_, _) => loaded.TrySetResult(); EventHandler onClosed = (_, _) => { closed = true; lifetime.Cancel(); };
        (object?[] References, string Values)? entryContext = null; var contextUnchanged = false;
        try
        {
            startUtc = process.StartTime.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture); assemblySha = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(assemblyPath)));
            var settings = new AppSettings { Language = "zh-CN", AutoStartApi = false, AutoConnect = false, AgentBaseUrl = null, AgentSerial = null,
                SyncMouse = false, SyncKeyboard = false, WindowWidth = options.Width, WindowHeight = options.Height, AdbAllowedSerials = [], LdSerialBindings = new() };
            var runtimeTask = MatrixRuntime.CreateAsync(settings, options.Directory, demo: true, ct: lifetime.Token); tasks.Add(runtimeTask); ownedRuntime = await runtimeTask;
            if (!ownedRuntime.IsDemo || ownedRuntime.Backend is not DemoDeviceBackend) throw new InvalidOperationException("观察入口未构造实际 Demo 后端。");
            vm = new WorkspaceViewModel(ownedRuntime, options.Directory, true); window = new MainWindow(vm) { ShowActivated = false };
            window.AttachWorkspaceClientClosing(closing); window.Loaded += onLoaded; window.Closed += onClosed; application.MainWindow = window; tasks.Add(loaded.Task); window.Show();
            await loaded.Task.WaitAsync(TimeSpan.FromSeconds(5), lifetime.Token);
            var refresh = vm.RefreshDevicesAsync(); tasks.Add(refresh); await refresh.WaitAsync(TimeSpan.FromSeconds(5), lifetime.Token); window.UpdateUnifiedDirectory();
            if (!window.IsLoaded || !window.IsVisible || window.WindowState != WindowState.Normal || !double.IsFinite(window.ActualWidth) || !double.IsFinite(window.ActualHeight)
                || Math.Abs(window.ActualWidth - options.Width) >= .5 || Math.Abs(window.ActualHeight - options.Height) >= .5)
                throw new InvalidOperationException("ready 前实际窗口加载、可见性或请求尺寸前置不符。");
            window.RequireWorkspaceClientIdle(); window.Title = title; entryContext = window.CaptureWorkspaceClientContext(); before = window.ObserveWorkspaceClientCondition();
            readyTicks = Stopwatch.GetTimestamp(); var readyUtc = DateTimeOffset.UtcNow;
            var readyBytes = JsonSerializer.SerializeToUtf8Bytes(new { nonce = options.Nonce, arm = options.Arm, pid = process.Id, processStartUtc = startUtc,
                assemblyPath, assemblySha256 = assemblySha, title, requestedWidth = options.Width, requestedHeight = options.Height,
                readyUtc, readyTimestamp = readyTicks, stopwatchFrequency = Stopwatch.Frequency, before, gateTimeoutSeconds = 180 }, JsonDefaults.Options);
            readySha = Convert.ToHexString(SHA256.HashData(readyBytes));
            using (var readyStream = new FileStream(Path.Combine(options.Directory, "ready.tmp"), FileMode.CreateNew, FileAccess.Write, FileShare.None, 8192, FileOptions.Asynchronous))
                await readyStream.WriteAsync(readyBytes, lifetime.Token);
            File.Move(Path.Combine(options.Directory, "ready.tmp"), Path.Combine(options.Directory, "ready.json")); readyPublished = true; phase = "gate";
            using var gate = CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token); gate.CancelAfter(TimeSpan.FromSeconds(180));
            var releaseTask = ReadRelease(); tasks.Add(releaseTask); var release = await releaseTask; releaseIntent = release.Intent;
            releaseTicks = Stopwatch.GetTimestamp(); elapsedMs = Stopwatch.GetElapsedTime(readyTicks.Value, releaseTicks.Value).TotalMilliseconds;
            lifetime.Token.ThrowIfCancellationRequested();
            after = window.ObserveWorkspaceClientCondition(); var exitContext = window.CaptureWorkspaceClientContext();
            contextUnchanged = entryContext.Value.References.SequenceEqual(exitContext.References) && entryContext.Value.Values == exitContext.Values;
            if (!contextUnchanged) throw new InvalidOperationException("观察期间原业务引用、选择、输入代次或调度状态发生变化。");
            window.RequireWorkspaceClientIdle();
            if (release.Intent == "abort") throw new InvalidOperationException("外部观察明确中止：" + release.Reason);
            phase = "workspace-contract"; var verify = window.VerifyWorkspaceAccessibilityAsync(); tasks.Add(verify); await verify;

            async Task<(string Intent, string Reason)> ReadRelease()
            {
                var path = Path.Combine(options.Directory, "release.json");
                while (!File.Exists(path)) await Task.Delay(100, gate.Token);
                using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 8192, FileOptions.Asynchronous);
                if (stream.Length is <= 0 or > 8192) throw new InvalidDataException("release.json 长度必须是1..8192字节。");
                var bytes = new byte[(int)stream.Length]; await stream.ReadExactlyAsync(bytes, gate.Token);
                if (stream.ReadByte() != -1) throw new InvalidDataException("release.json 在读取时长度发生变化。");
                using var document = JsonDocument.Parse(bytes, new JsonDocumentOptions { MaxDepth = 8 }); var root = document.RootElement;
                var allowed = new HashSet<string>(new[] { "nonce", "arm", "pid", "processStartUtc", "assemblySha256", "title", "readySha256", "intent", "reason" }, StringComparer.Ordinal);
                if (root.ValueKind != JsonValueKind.Object) throw new InvalidDataException("release 必须是完整对象。");
                foreach (var property in root.EnumerateObject()) if (!allowed.Remove(property.Name)) throw new InvalidDataException("release 字段未知或重复：" + property.Name);
                if (allowed.Count != 0) throw new InvalidDataException("release 缺少字段：" + string.Join(",", allowed));
                string Field(string name) => root.GetProperty(name).GetString() ?? throw new InvalidDataException("release 字段不可为null：" + name);
                if (Field("nonce") != options.Nonce || Field("arm") != options.Arm || root.GetProperty("pid").GetInt32() != process.Id || Field("processStartUtc") != startUtc
                    || Field("assemblySha256") != assemblySha || Field("title") != title || Field("readySha256") != readySha) throw new InvalidDataException("release 与本次 ready 身份不匹配。");
                var intent = Field("intent"); var reason = Field("reason");
                if (intent is not ("success" or "abort") || intent == "abort" && string.IsNullOrWhiteSpace(reason)) throw new InvalidDataException("release 必须明确 success 或带原因的 abort。");
                return (intent, reason);
            }
        }
        catch (Exception error)
        {
            failure = error; classes.Add(phase == "workspace-contract" ? "CONTRACT-FAIL" : phase == "gate" ? "GATE-FAIL" : "PRECONDITION-FAIL");
            Console.Error.WriteLine("WORKSPACE-CLIENT-PRIMARY " + error);
        }
        finally
        {
            lifetime.Cancel(); loaded.TrySetCanceled();
            if (readyTicks is not null && releaseTicks is null) { releaseTicks = Stopwatch.GetTimestamp(); elapsedMs = Stopwatch.GetElapsedTime(readyTicks.Value, releaseTicks.Value).TotalMilliseconds; }
            if (window is not null && readyPublished && after is null)
            {
                try { after = window.ObserveWorkspaceClientCondition(); var exitContext = window.CaptureWorkspaceClientContext(); contextUnchanged = entryContext is { } entry && entry.References.SequenceEqual(exitContext.References) && entry.Values == exitContext.Values; }
                catch (Exception error) { classes.Add("FIXTURE-FAIL"); failure = Append(failure, error); Console.Error.WriteLine("WORKSPACE-CLIENT-AFTER " + error); }
            }
            if (window is not null)
            {
                try { window.Loaded -= onLoaded; window.Closed -= onClosed; window.DetachWorkspaceClientClosing(closing); controlledAbort = true; window.AbortStartup(); closed = !application.Windows.Cast<Window>().Contains(window); }
                catch (Exception error) { CleanupError(error); }
            }
            try
            {
                if (vm is not null) { cleanupCalls++; cleanupTask = vm.ShutdownAsync(); }
                else if (ownedRuntime is not null) { cleanupCalls++; cleanupTask = ownedRuntime.DisposeAsync().AsTask(); }
                if (cleanupTask is not null) { tasks.Add(cleanupTask); await cleanupTask.WaitAsync(TimeSpan.FromSeconds(15)); }
            }
            catch (Exception error) { CleanupError(error); }
            if (tasks.Any(task => !task.IsCompleted)) CleanupError(new InvalidOperationException("底层原任务未排空，不能将观察限时当成取消完成。"));
            var contractPath = Path.Combine(options.Directory, "workspace-accessibility", "summary.json");
            if (File.Exists(contractPath))
            {
                try
                {
                    using var document = JsonDocument.Parse(await File.ReadAllBytesAsync(contractPath)); contract = document.RootElement.Clone();
                    if (phase == "workspace-contract" && failure is not null)
                    {
                        classes.Remove("CONTRACT-FAIL"); foreach (var value in document.RootElement.GetProperty("failureClasses").EnumerateArray()) classes.Add(value.GetString()!);
                    }
                }
                catch (Exception error) { classes.Add("REPORT-FAIL"); failure = Append(failure, error); Console.Error.WriteLine("WORKSPACE-CLIENT-CONTRACT-REPORT " + error); }
            }
            try
            {
                await File.WriteAllTextAsync(Path.Combine(options.Directory, "probe-summary.json"), JsonSerializer.Serialize(new
                {
                    status = failure is null ? "PASS-WORKSPACE-CLIENT-OBSERVATION" : classes.Count == 1 ? classes.Single() : "MIXED-FAIL", failureClasses = classes.OrderBy(value => value).ToArray(),
                    options.Nonce, options.Arm, pid = process.Id, processStartUtc = startUtc, assemblySha256 = assemblySha, title, readyPublished, readySha256 = readySha, readyTicks, releaseTicks, elapsedMs,
                    releaseIntent, elapsedMeaning = releaseIntent is null ? "ready-to-failure-observation" : "ready-to-validated-release", before, after, contextUnchanged, entryBusinessContext = entryContext?.Values, contract, userCloseRequested, controlledAbort, closed,
                    cleanupCalls, cleanupTaskStatus = cleanupTask?.Status.ToString(), cleanupTaskCompleted = cleanupTask?.IsCompleted, cleanupErrors,
                    tasks = tasks.Select(task => new { status = task.Status.ToString(), task.IsCompleted }).ToArray(), error = failure?.ToString(),
                    scope = "Own Demo client/control observation gate; no Sky call by this process, no input/activation/screenshot/guest/API. Existing Window/tabs peers only. Controlled AbortStartup is not normal Close. Original contract result is preserved; this is not an AP042 repair or provider-causality proof."
                }, JsonDefaults.Options));
            }
            catch (Exception error) { failure = Append(failure, error); Console.Error.WriteLine("WORKSPACE-CLIENT-REPORT " + error); }
        }
        return failure is null ? 0 : 1;
        void CleanupError(Exception error) { cleanupErrors.Add(error.ToString()); classes.Add("CLEANUP-FAIL"); failure = Append(failure, error); Console.Error.WriteLine("WORKSPACE-CLIENT-CLEANUP " + error); }
        static Exception Append(Exception? prior, Exception next) => prior is null ? next : new AggregateException(prior, next);
    }
}

public partial class MainWindow
{
    internal void AttachWorkspaceClientClosing(CancelEventHandler handler) { Closing -= WindowClosing; Closing += handler; }
    internal void DetachWorkspaceClientClosing(CancelEventHandler handler) { Closing -= handler; LocalizationService.Current.LanguageChanged -= OnLanguageChanged; }
    internal void RequireWorkspaceClientIdle()
    {
        if (!ViewModel.Demo || !ViewModel.Runtime.IsDemo || ViewModel.Runtime.Backend is not DemoDeviceBackend || ViewModel.Busy || ViewModel.Transferring || ViewModel.HasActiveOperations
            || ViewModel.Runtime.Settings.AutoStartApi || ViewModel.Runtime.Settings.AutoConnect || ViewModel.Runtime.Settings.AgentBaseUrl is not null || ViewModel.Runtime.Settings.AgentSerial is not null
            || ViewModel.SyncMouse || ViewModel.SyncKeyboard || ViewModel.Runtime.Api.BasicRunning || ViewModel.Runtime.Api.AutomatorRunning || ViewModel.Runtime.Api.HasActiveRequests || ViewModel.Runtime.Api.DevicePorts.Count != 0
            || applicationRunning || applicationReadPending || applicationReadTimer?.IsEnabled == true || applicationTargets.Any(row => row.IsSelected) || applicationTasks.Count != 0 || applicationRows.Count != 0
            || DebugAutomaticFixturePermitted || ReconnectLoadingFixturePermitted || captureInProgress || captureOperation is not null || directoryOperation is { IsCompleted: false }
            || monitorWasActive || monitorTimer.IsEnabled || !monitorReconcile.IsCompleted || !monitorSampling.IsCompleted || navigationDrawerOpen || mainControlOpen || gestureStart is not null)
            throw new InvalidOperationException("客户端观察业务/后台调度门未关闭。");
    }
    internal (object?[] References, string Values) CaptureWorkspaceClientContext() =>
        (new object?[] { ViewModel.Runtime, ViewModel.Runtime.Settings, ViewModel.Runtime.Input, ViewModel.Runtime.Api, ViewModel.CurrentDevice, ViewModel.MainDevice, tabs.SelectedItem, tabs.Template, tabs.Style }
            .Concat(ViewModel.Devices.Cast<object?>()).Concat(ViewModel.Devices.Select(row => (object?)row.Info)).Concat(applicationTargets.Cast<object?>()).ToArray(),
        JsonSerializer.Serialize(new { Title, ViewModel.TextInput, ViewModel.ShellInput, ViewModel.Status, ViewModel.Error, ViewModel.Busy, ViewModel.Transferring,
            ViewModel.SyncMouse, ViewModel.SyncKeyboard, inputGeneration = ViewModel.Runtime.Input.CurrentGeneration, mainGestureDispatchCount, mainCaptureDispatchCount,
            selection = ViewModel.Devices.Select(row => new { row.Serial, row.IsSelected }).ToArray(), targets = applicationTargets.Select(row => new { row.Serial, row.IsSelected }).ToArray(),
            applicationRunning, applicationReadPending, applicationRows = applicationRows.Count, applicationTasks = applicationTasks.Count,
            settings = ViewModel.Runtime.Settings, monitorWasActive, monitorTimer.IsEnabled, captureInProgress, capturePending = captureOperation is not null,
            monitorReconcile = monitorReconcile.Status.ToString(), monitorSampling = monitorSampling.Status.ToString() }, JsonDefaults.Options));
    internal object ObserveWorkspaceClientCondition()
    {
        var windowPeer = UIElementAutomationPeer.FromElement(this); var tabsPeer = UIElementAutomationPeer.FromElement(tabs); var focus = Keyboard.FocusedElement;
        return new { Title, IsLoaded, IsVisible, IsActive, IsKeyboardFocusWithin, ActualWidth, ActualHeight, windowState = WindowState.ToString(),
            windowPeerExists = windowPeer is not null, windowPeerType = windowPeer?.GetType().FullName, windowPeerId = windowPeer is null ? 0 : System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(windowPeer),
            tabsPeerExists = tabsPeer is not null, tabsPeerType = tabsPeer?.GetType().FullName, tabsPeerId = tabsPeer is null ? 0 : System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(tabsPeer),
            structureListener = AutomationPeer.ListenerExists(AutomationEvents.StructureChanged), propertyListener = AutomationPeer.ListenerExists(AutomationEvents.PropertyChanged),
            focusType = focus?.GetType().FullName, focusName = (focus as FrameworkElement)?.Name, ViewModel.Status, ViewModel.Error, selectedTag = (tabs.SelectedItem as TabItem)?.Tag,
            peerPresenceDoesNotProveProviderConnection = true };
    }
}
