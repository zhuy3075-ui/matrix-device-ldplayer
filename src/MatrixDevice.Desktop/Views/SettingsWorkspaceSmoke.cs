using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;
using MatrixDevice.Core;
using MatrixDevice.Host;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    private static readonly List<object> retainedSettingsWorkspaces = [];

    internal static async Task RunSettingsWorkspaceVerificationAsync(string directory, Dispatcher dispatcher)
    {
        if (!dispatcher.CheckAccess() || Thread.CurrentThread.GetApartmentState() != ApartmentState.STA
            || Application.Current.Windows.Count != 0 || Application.Current.MainWindow is not null
            || !Path.IsPathFullyQualified(directory) || Directory.Exists(directory) || File.Exists(directory))
            throw new InvalidOperationException("设置验收要求 fresh artifacts 目录及无用户窗口的自有 STA。");
        Directory.CreateDirectory(directory);
        var reports = new List<object>(); var failures = new List<Exception>(); var stop = false;
        var conditionalNotRun = new List<object>();
        foreach (var size in new[] { (W: 390, H: 640), (W: 390, H: 844), (W: 768, H: 1024), (W: 1000, H: 700), (W: 1440, H: 900), (W: 1920, H: 1080) })
            await Case($"geometry-{size.W}-{size.H}", size.W, size.H, state => state.Geometry());
        await Case("numeric-binding-abc-and-empty", 1000, 700, async state =>
        {
            foreach (var text in new[] { "abc", "" })
            {
                await state.Edit("BasicApiPort", text);
                state.Product(Validation.GetHasError(state.Input("BasicApiPort")), "实际数值绑定出现转换错误", new { text });
                await state.Reject("原数值转换错误不得保存旧整数");
                await state.Edit("BasicApiPort", "18080");
                state.Product(!Validation.GetHasError(state.Input("BasicApiPort")), "纠正数值后绑定错误消失");
                await state.Save(); state.Saved("纠正后保存", value => value.BasicApiPort == 18080);
            }
        });
        await Case("valid-boundaries-and-nullable-clear", 1000, 700, async state =>
        {
            foreach (var values in new[] { new[] { "180", "1", "10000", "1" }, new[] { "4096", "120", "100000000", "300" } })
            {
                await state.Edit("MainMaxSize", values[0]); await state.Edit("MainMaxFps", values[1]);
                await state.Edit("MainBitRate", values[2]); await state.Edit("CommandTimeoutSeconds", values[3]);
                await state.Save(); state.Saved("有效上下边界精确保留", value => value.MainMaxSize == int.Parse(values[0], CultureInfo.InvariantCulture)
                    && value.MainMaxFps == int.Parse(values[1], CultureInfo.InvariantCulture) && value.MainBitRate == int.Parse(values[2], CultureInfo.InvariantCulture)
                    && value.CommandTimeoutSeconds == int.Parse(values[3], CultureInfo.InvariantCulture));
            }
            await state.Edit("AdbSerialsJson", "[\"DEMO-001\"]"); await state.Edit("AgentSerial", "DEMO-001");
            await state.Edit("AgentBaseUrl", "https://settings-owned.invalid/"); await state.Save();
            state.Saved("合法URL只解析保存", value => value.AgentBaseUrl == "https://settings-owned.invalid/");
            await state.Edit("AgentBaseUrl", ""); await state.Edit("AgentSerial", ""); await state.Save();
            state.Saved("nullable清空不恢复旧地址", value => value.AgentBaseUrl is null && value.AgentSerial is null);
            foreach (var property in new[] { "AutoConnect", "AutoStartApi", "ShowFps" }) await state.Toggle(property, true);
            await state.Save(); state.Saved("原复选框只保存偏好、不隐式启动服务", value => value.AutoConnect && value.AutoStartApi && value.ShowFps && !state.Runtime.Api.BasicRunning);
        });
        await Case("out-of-range-and-nonfinite", 1000, 700, async state =>
        {
            foreach (var item in new[] { ("MainMaxSize", "179", "1280"), ("MainMaxSize", "4097", "1280"),
                ("MainMaxFps", "121", "45"), ("CommandTimeoutSeconds", "301", "30"),
                ("MainBitRate", "9999", "6000000"), ("WindowWidth", "NaN", "1000"), ("WindowHeight", "Infinity", "700") })
            {
                await state.Edit(item.Item1, item.Item2); await state.Reject(item.Item1 + "=" + item.Item2);
                await state.Edit(item.Item1, item.Item3);
            }
            await state.Save(); state.Saved("纠正越界值后可恢复", value => value.MainMaxSize == 1280 && value.MainMaxFps == 45 && value.CommandTimeoutSeconds == 30);
            try
            {
                const double target = AppSettings.MaximumWindowWidth + 100;
                state.Window.Width = target; await state.Settle();
                state.Observe("actual-out-of-range-resize", new { target, observedWidth = state.Window.Width, state.Window.ActualWidth, AppSettings.MaximumWindowWidth });
                if (state.Window.ActualWidth > AppSettings.MaximumWindowWidth)
                { state.RequireActualOutOfRangeWidth(); await state.Reject("实际手动resize超界不得绕过最终尺寸校验"); }
                else
                {
                    var limitation = new { step = "actual-oversize-window-save", status = "NOT-RUN", reason = "实际窗口超界前置未建立，不推断平台或产品原因", target,
                        observedWidth = state.Window.Width, state.Window.ActualWidth };
                    state.Observe("actual-out-of-range-resize-not-run", limitation); conditionalNotRun.Add(limitation);
                }
            }
            finally { state.Window.Width = 1000; await state.Settle(); }
            await state.Save(); state.Saved("恢复实际窗口尺寸后原保存可重试", value => value.WindowWidth == 1000);
        });
        await Case("URL-scheme-and-relative-path", 1000, 700, async state =>
        {
            await state.Edit("AdbSerialsJson", "[\"DEMO-001\"]"); await state.Edit("AgentSerial", "DEMO-001");
            foreach (var address in new[] { "ftp://settings-owned.invalid/", "relative/agent" })
            { await state.Edit("AgentBaseUrl", address); await state.Reject("非法Agent地址只校验不连接：" + address); }
            foreach (var address in new[] { "http://settings-owned.invalid/", "https://settings-owned.invalid/" })
            { await state.Edit("AgentBaseUrl", address); await state.Save(); state.Saved("HTTP(S)只保存，不创建新连接", value => value.AgentBaseUrl == address); }
        });
        await Case("JSON-and-duplicate-port-recovery", 1000, 700, async state =>
        {
            foreach (var item in new[] { ("AdbSerialsJson", "[", "[]"), ("AdbSerialsJson", "[\"DEMO-001\",\"DEMO-001\"]", "[]"),
                ("LdBindingsJson", "{", "{}"), ("LdBindingsJson", "{\"1\":\"DEMO-001\",\"2\":\"DEMO-001\"}", "{}"),
                ("AutomatorApiPort", "8080", "17171") })
            { await state.Edit(item.Item1, item.Item2); await state.Reject("坏JSON/重复身份/端口：" + item.Item1); await state.Edit(item.Item1, item.Item3); }
            await state.Save(); state.Saved("纠正后保存恢复", value => value.AutomatorApiPort == 17171 && value.AdbAllowedSerials.Length == 0);
        });
        await Case("write-failure-and-retry", 1000, 700, async state =>
        {
            await state.Edit("CommandTimeoutSeconds", "31");
            using (var held = new FileStream(state.ConfigPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                await state.Reject("自有主配置禁止替换的真实写入故障");
            await state.Save(); state.Saved("释放原文件句柄后重试", value => value.CommandTimeoutSeconds == 31);
        });
        await Case("external-writer-two-consecutive-saves", 1000, 700, async state =>
        {
            foreach (var item in new[] { (Root: "/sdcard/External-One", Timeout: "31", Width: 1200d), (Root: "/sdcard/External-Two", Timeout: "32", Width: 1300d) })
            {
                var external = new SettingsStore(state.ConfigPath); var disk = external.Load(); external.Save(disk with { FileRoot = item.Root, WindowWidth = item.Width });
                state.Observe("external-write", state.Config()); await state.Edit("CommandTimeoutSeconds", item.Timeout); await state.Save();
                state.Saved("连续Save保留外writer未编辑字段与未手动调整的geometry", value => value.FileRoot == item.Root && value.WindowWidth == item.Width && value.CommandTimeoutSeconds == int.Parse(item.Timeout, CultureInfo.InvariantCulture));
                state.Product(state.Input("FileRoot").Text == item.Root && state.Vm.Settings.FileRoot == item.Root, "同一Editor与原TextBox吸收外部合并结果", new { state.Vm.Settings.FileRoot, text = state.Input("FileRoot").Text });
                state.Product(Math.Abs(state.Window.ActualWidth - 1000) < .6 && state.Input("WindowWidth").Text == item.Width.ToString(CultureInfo.CurrentCulture),
                    "外writer尺寸被保存但不冒充本窗口用户resize或强制套用", new { state.Window.ActualWidth, text = state.Input("WindowWidth").Text });
            }
        });
        await Case("explicit-size-save-and-original-close", 1000, 700, async state =>
        {
            await state.Edit("WindowWidth", "768"); await state.Edit("WindowHeight", "1024"); await state.Save();
            state.Saved("显式尺寸保存", value => value.WindowWidth == 768 && value.WindowHeight == 1024);
            state.Product(Math.Abs(state.Window.ActualWidth - 768) < .6 && Math.Abs(state.Window.ActualHeight - 1024) < .6,
                "原保存命令应用明确编辑的实际窗口尺寸", new { state.Window.ActualWidth, state.Window.ActualHeight });
            await state.NormalClose(); var reloaded = new SettingsStore(state.ConfigPath).Load();
            state.Product(reloaded.WindowWidth == 768 && reloaded.WindowHeight == 1024, "原WindowClosing完成后fresh Store仍保留显式尺寸", new { reloaded.WindowWidth, reloaded.WindowHeight });
        });
        await Case("manual-resize-unrelated-save-and-single-axis", 1000, 700, async state =>
        {
            state.Window.Width = 1100; state.Window.Height = 780; await state.Settle();
            await state.Edit("CommandTimeoutSeconds", "31"); await state.Save();
            state.Product(Math.Abs(state.Window.ActualWidth - 1100) < .6 && Math.Abs(state.Window.ActualHeight - 780) < .6, "无关保存不重置手动resize", new { state.Window.ActualWidth, state.Window.ActualHeight });
            state.Saved("手动resize后的普通保存同步Editor/Runtime/Store", value => value.WindowWidth == 1100 && value.WindowHeight == 780);
            await state.Edit("WindowWidth", "1200"); await state.Save();
            state.Product(Math.Abs(state.Window.ActualWidth - 1200) < .6 && Math.Abs(state.Window.ActualHeight - 780) < .6, "只提交宽度保留手动调整的另一轴", new { state.Window.ActualWidth, state.Window.ActualHeight });
            state.Saved("单轴提交后另一轴三方仍为780", value => value.WindowWidth == 1200 && value.WindowHeight == 780);
            var external = new SettingsStore(state.ConfigPath); var disk = external.Load(); external.Save(disk with { WindowWidth = 1250 });
            foreach (var timeout in new[] { "32", "33" })
            {
                await state.Edit("CommandTimeoutSeconds", timeout); await state.Save();
                state.Saved("程序化应用尺寸之后的连续Save不覆写外writer", value => value.WindowWidth == 1250 && value.WindowHeight == 780);
                state.Product(Math.Abs(state.Window.ActualWidth - 1200) < .6, "外writer width不冒充本窗口实际resize", new { state.Window.ActualWidth });
            }
            state.Window.Height = 800; await state.Settle(); await state.Save();
            state.Saved("后续真实手动height保留外writer width", value => value.WindowWidth == 1250 && value.WindowHeight == 800);
            state.Product(Math.Abs(state.Window.ActualWidth - 1200) < .6 && Math.Abs(state.Window.ActualHeight - 800) < .6, "手动另一轴不强制套外部width", new { state.Window.ActualWidth, state.Window.ActualHeight });
        });
        await Case("busy-and-owned-running-port", 1000, 700, async state =>
        {
            var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously); state.ReleaseOnCleanup(release);
            var busy = state.Track(state.Vm.ExecuteAsync("owned-settings-busy", () => release.Task, blocksApiWrites: false));
            try
            {
                await state.Settle(); var before = state.Config(); var settings = state.Runtime.Settings;
                state.Product(!state.SaveButton.IsEnabled && !state.Vm.SaveSettingsCommand.CanExecute(null), "Busy禁用原保存控件与命令");
                await state.Save(); state.Product(before == state.Config() && ReferenceEquals(settings, state.Runtime.Settings), "Busy保存不落盘或替换设置");
            }
            finally { release.TrySetResult(); await state.Wait(busy); }
            state.Product(state.SaveButton.IsEnabled && !state.Vm.Busy, "Busy原任务完成后保存恢复");
            var listener = new TcpListener(IPAddress.Loopback, 0); int port;
            try { listener.Start(); port = ((IPEndPoint)listener.LocalEndpoint).Port; }
            finally { listener.Stop(); }
            await state.Edit("BasicApiPort", port.ToString(CultureInfo.InvariantCulture)); await state.Save();
            state.Saved("自有端口保存", value => value.BasicApiPort == port);
            await state.Wait(state.Runtime.Api.StartBasicAsync(port));
            await state.Edit("BasicApiPort", (port == 65535 ? port - 1 : port + 1).ToString(CultureInfo.InvariantCulture));
            await state.Reject("运行中自有API拒绝修改端口");
            using (var client = new HttpClient(new HttpClientHandler { UseProxy = false, AllowAutoRedirect = false }) { Timeout = TimeSpan.FromSeconds(5) })
            {
                var response = state.Track(client.GetAsync($"http://127.0.0.1:{port}/health")); using var actual = await response;
                state.Product(actual.StatusCode == HttpStatusCode.OK && state.Runtime.Api.RunningBasicPort == port, "拒绝后原owned loopback监听仍在", new { actual.StatusCode, state.Runtime.Api.RunningBasicPort });
            }
            await state.Wait(state.Runtime.Api.StopAsync()); await state.Save();
            state.Saved("停止自有API后可重试", value => value.BasicApiPort != port);
        });
        var summary = new { status = failures.Count == 0 ? "PASS-OWN-WPF-SETTINGS" : "FAIL-SETTINGS-WORKSPACE", reports,
            windows = Application.Current.Windows.Count, mainWindowNull = Application.Current.MainWindow is null, retainedOwners = retainedSettingsWorkspaces.Count,
            scope = "Owned shown Demo WPF, actual TextBox bindings and original save command; original WindowClosing only in its explicit case. Not external GUI input or real-device acceptance.",
            notRun = new object[] { "native picker", "real legacy window recovery", "maximized/RestoreBounds save", "physical GUI input", "DPI", "Agent network", "real simulator/ADB", "AP042/AP045", "final dual 30 minutes" }.Concat(conditionalNotRun).ToArray() };
        try { File.WriteAllText(Path.Combine(directory, "settings-workspace.json"), JsonSerializer.Serialize(summary, JsonDefaults.Options)); }
        catch (Exception error) { failures.Add(new IOException("REPORT-FAIL: 设置验收报告保存失败", error)); }
        if (failures.Count > 0) throw new AggregateException("设置工作区验收未全部通过", failures);

        async Task Case(string name, int width, int height, Func<SettingsWorkspaceCase, Task> body)
        {
            if (stop) { reports.Add(new { name, status = "NOT-RUN", reason = "前例资源/原任务未证明安全清理" }); return; }
            var state = new SettingsWorkspaceCase(Path.Combine(directory, name), width, height, dispatcher);
            var classes = new HashSet<string>(); Exception? error = null; Exception? cleanup = null; var initialized = false;
            try { await state.Initialize(); initialized = true; await body(state); }
            catch (Exception failure) { error = failure; classes.Add(failure is SettingsWorkspacePrecondition || !initialized ? "PRECONDITION-FAIL" : "FIXTURE-FAIL"); }
            finally { try { await state.Cleanup(); } catch (Exception failure) { cleanup = failure; classes.Add("CLEANUP-FAIL"); stop = true; } }
            if (state.Failures.Count > 0) classes.Add("PRODUCT-FAIL");
            var report = new { name, status = classes.Count > 1 ? "MIXED-FAIL" : classes.SingleOrDefault() ?? "PASS", failureClasses = classes,
                initialized, checks = state.Checks, observations = state.Observations, productFailures = state.Failures, error = error?.ToString(), cleanup = cleanup?.ToString(),
                tasks = state.TaskStates(), state.NormalCloseRequests, state.ShutdownRequests, state.CleanupOwner, state.Closed, state.Retained };
            try { File.WriteAllText(Path.Combine(state.Directory, "case.json"), JsonSerializer.Serialize(report, JsonDefaults.Options)); }
            catch (Exception failure) { classes.Add("REPORT-FAIL"); failures.Add(failure); }
            reports.Add(classes.Contains("REPORT-FAIL") ? new { name, status = "REPORT-FAIL", failureClasses = classes, preservedResult = (object)report } : (object)report);
            if (classes.Count > 0) failures.Add(new InvalidOperationException(name + ": " + string.Join(",", classes), new AggregateException(new[] { error, cleanup }.OfType<Exception>().Concat(state.Failures.Select(message => new Exception(message))))));
        }
    }

    private sealed record SettingsConfigEvidence(bool Exists, long Bytes, string? Sha256, long? WriteTicks, string? Base64);
    private sealed class SettingsWorkspacePrecondition(string message) : Exception(message);
    private sealed class SettingsWorkspaceCase(string directory, int width, int height, Dispatcher dispatcher)
    {
        internal string Directory => directory;
        internal string ConfigPath => Path.Combine(directory, "AppConfig.json");
        internal MainWindow Window = null!;
        internal WorkspaceViewModel Vm = null!;
        internal MatrixRuntime Runtime = null!;
        internal Button SaveButton = null!;
        internal readonly List<string> Checks = [], Failures = [];
        internal readonly List<object> Observations = [];
        internal int NormalCloseRequests, ShutdownRequests;
        internal string CleanupOwner = "not-started";
        internal bool Closed, Retained;
        private readonly List<Task> tasks = [];
        private readonly List<TaskCompletionSource> releases = [];
        private readonly CancellationTokenSource deadline = new(TimeSpan.FromSeconds(90));
        private readonly TaskCompletionSource loaded = new(TaskCreationOptions.RunContinuationsAsynchronously), closed = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly Dictionary<string, TextBox> inputs = new(StringComparer.Ordinal);
        private readonly Dictionary<DependencyObject, BindingBase> originalBindings = new(ReferenceEqualityComparer.Instance);
        private readonly Dictionary<Button, System.Windows.Input.ICommand?> originalCommands = new();
        private Task<MatrixRuntime>? creation; private Task? shutdown;
        private FrameworkElement page = null!;
        private object[] identities = []; private DeviceRow[] devices = []; private bool[] selected = [];
        private bool closeAttempted, userClose, cleanupStarted;
        private int step;

        internal Task Track(Task task) { tasks.Add(task); return task; }
        internal Task<T> Track<T>(Task<T> task) { tasks.Add(task); return task; }
        internal async Task Wait(Task task) { if (!tasks.Contains(task)) tasks.Add(task); await task.WaitAsync(TimeSpan.FromSeconds(10)); }
        internal async Task Settle()
        {
            await Wait(dispatcher.InvokeAsync(Window.UpdateLayout, DispatcherPriority.DataBind).Task);
            await Wait(dispatcher.InvokeAsync(Window.UpdateLayout, DispatcherPriority.Render).Task);
            await Wait(dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle).Task);
        }
        internal void Product(bool value, string expected, object? actual = null)
        {
            Checks.Add((value ? "PASS " : "FAIL ") + expected); Observe("assert", new { expected, actual, passed = value });
            if (!value) Failures.Add(expected);
        }
        private static void Require(bool value, string expected) { if (!value) throw new SettingsWorkspacePrecondition(expected); }
        internal void Observe(string operation, object? actual) => Observations.Add(new { step = ++step, operation, actual });
        internal TextBox Input(string property) => inputs["Settings." + property];
        internal object[] TaskStates() => tasks.Select(task => (object)new { status = task.Status.ToString(), task.IsCompleted }).ToArray();
        internal void ReleaseOnCleanup(TaskCompletionSource signal) => releases.Add(signal);
        internal void RequireActualOutOfRangeWidth() => Require(Window.ActualWidth > AppSettings.MaximumWindowWidth, "平台实际显示超界宽度，非Width属性被平台夹取");
        private object[] Identity() => [Vm, Vm.Settings, Runtime, Runtime.Backend, Runtime.Devices, Runtime.Scripts, Runtime.Input, Runtime.Api, Runtime.Metadata, Runtime.SettingsStore, Runtime.Writes, Runtime.FrozenConnectionSettings, page, SaveButton, Vm.SaveSettingsCommand, Vm.CurrentDevice!, Vm.MainDevice!];
        private void Context()
        {
            Product(identities.SequenceEqual(Identity(), ReferenceEqualityComparer.Instance) && devices.SequenceEqual(Vm.Devices, ReferenceEqualityComparer.Instance)
                && selected.SequenceEqual(Vm.Devices.Select(row => row.IsSelected)), "保持Editor/Runtime/组件/页面/命令/设备对象与选择身份");
            foreach (var pair in originalBindings)
            {
                var property = pair.Key is TextBox ? TextBox.TextProperty : ToggleButton.IsCheckedProperty;
                Product(ReferenceEquals(BindingOperations.GetBindingBase(pair.Key, property), pair.Value), "保留原Binding实例：" + ((pair.Key as FrameworkElement)?.Name ?? ""));
            }
            Product(originalCommands.All(pair => ReferenceEquals(pair.Key.Command, pair.Value)), "全部原按钮命令实例保留");
            Product(inputs.Values.ToHashSet(ReferenceEqualityComparer.Instance).SetEquals(VisualDescendants<TextBox>(page))
                && originalCommands.Keys.ToHashSet(ReferenceEqualityComparer.Instance).SetEquals(VisualDescendants<Button>(page))
                && originalBindings.Keys.OfType<CheckBox>().ToHashSet(ReferenceEqualityComparer.Instance).SetEquals(VisualDescendants<CheckBox>(page)), "当前真实页面仍包含同一组原控件而非旧引用或替身");
            Product(!Runtime.Api.AutomatorRunning && Runtime.Api.DevicePorts.Count == 0 && Runtime.Scripts.GetJobs().Count == 0, "无隐式自动化接口/设备接口/脚本接纳");
        }
        internal async Task Initialize()
        {
            Require(Application.Current.Windows.Count == 0 && Application.Current.MainWindow is null, "前例窗口已释放");
            System.IO.Directory.CreateDirectory(directory);
            var settings = new AppSettings { Language = "zh-CN", WindowWidth = width, WindowHeight = height, AutoStartApi = false, AutoConnect = false,
                AgentBaseUrl = null, AgentSerial = null, SyncMouse = false, SyncKeyboard = false, AdbPath = "", LdConsolePath = "", AdbAllowedSerials = [], LdSerialBindings = [] };
            creation = Track(MatrixRuntime.CreateAsync(settings, directory, demo: true, ct: deadline.Token));
            Runtime = await creation.WaitAsync(TimeSpan.FromSeconds(15)); Require(Runtime.IsDemo && Runtime.Backend is DemoDeviceBackend, "实际自有Demo后端");
            Vm = new WorkspaceViewModel(Runtime, directory, true); await Wait(Vm.RefreshDevicesAsync());
            Require(Vm.Devices.Count == 2, "自有Demo两设备"); await Wait(Vm.SetMainAsync(Vm.Devices[0]));
            Vm.CurrentDevice = Vm.Devices[1]; Vm.Devices[1].IsSelected = true; Vm.UpdateSelection();
            Window = new MainWindow(Vm) { ShowActivated = false }; Window.Closing -= Window.WindowClosing; Window.Closing += OnClose;
            Window.Loaded += OnLoaded; Window.Closed += OnClosed; Application.Current.MainWindow = Window;
            Window.Show(); Window.NavigateTo("设置"); await Wait(loaded.Task); await Settle();
            Observe("actual-window", new { width, height, Window.ActualWidth, Window.ActualHeight, Window.IsLoaded, Window.IsVisible, Window.ShowActivated });
            Require(Window.IsLoaded && Window.IsVisible && PresentationSource.FromVisual(Window) is not null && Math.Abs(Window.ActualWidth - width) < .6 && Math.Abs(Window.ActualHeight - height) < .6, "真实外窗尺寸等于请求");
            page = (FrameworkElement)((TabItem)Window.tabs.SelectedItem).Content;
            foreach (var box in VisualDescendants<TextBox>(page))
            {
                var binding = BindingOperations.GetBinding(box, TextBox.TextProperty) ?? throw new InvalidOperationException("原设置输入缺少Binding");
                Require(binding.Path?.Path.StartsWith("Settings.", StringComparison.Ordinal) == true, "设置输入有原Binding");
                inputs.Add(binding.Path!.Path, box); originalBindings.Add(box, binding);
            }
            foreach (var check in VisualDescendants<CheckBox>(page))
            { var binding = BindingOperations.GetBinding(check, ToggleButton.IsCheckedProperty); Require(binding is not null, "原复选框绑定"); originalBindings.Add(check, binding!); }
            foreach (var button in VisualDescendants<Button>(page)) originalCommands.Add(button, button.Command);
            Require(inputs.Count == 21 && originalBindings.Count == 25 && originalCommands.Count == 7, "原21输入/4复选框/7按钮完整");
            SaveButton = originalCommands.Keys.Single(button => ReferenceEquals(button.Command, Vm.SaveSettingsCommand));
            identities = Identity(); devices = Vm.Devices.ToArray(); selected = devices.Select(row => row.IsSelected).ToArray();
            await Save(); Require(Vm.Error.Length == 0 && File.Exists(ConfigPath), "原控件初始合法保存可用"); Context();
        }
        private void OnLoaded(object sender, RoutedEventArgs args) => loaded.TrySetResult();
        private void OnClosed(object? sender, EventArgs args) { Closed = true; closed.TrySetResult(); }
        private void OnClose(object? sender, CancelEventArgs args) { args.Cancel = true; userClose = true; }
        internal SettingsConfigEvidence Config()
        {
            if (!File.Exists(ConfigPath)) return new(false, 0, null, null, null);
            var bytes = File.ReadAllBytes(ConfigPath); return new(true, bytes.LongLength, Convert.ToHexString(SHA256.HashData(bytes)), File.GetLastWriteTimeUtc(ConfigPath).Ticks, Convert.ToBase64String(bytes));
        }
        internal async Task Edit(string property, string text)
        {
            var input = Input(property); var expression = input.GetBindingExpression(TextBox.TextProperty);
            Require(expression is not null && input.IsEnabled && !input.IsReadOnly, "编辑原可用输入：" + property);
            input.SetCurrentValue(TextBox.TextProperty, text); expression!.UpdateSource(); await Settle();
            Product(ReferenceEquals(expression, input.GetBindingExpression(TextBox.TextProperty)), "实际编辑保持原BindingExpression：" + property);
            Observe("edit", new { property, requested = text, actual = input.Text, hasError = Validation.GetHasError(input), errors = Validation.GetErrors(input).Select(error => error.ErrorContent?.ToString()).ToArray() });
        }
        internal async Task Toggle(string property, bool value)
        {
            var control = originalBindings.Keys.OfType<CheckBox>().Single(check => BindingOperations.GetBinding(check, ToggleButton.IsCheckedProperty)?.Path.Path == "Settings." + property);
            Require(control.IsEnabled, "实际复选框可用：" + property);
            var expression = control.GetBindingExpression(ToggleButton.IsCheckedProperty); Require(expression is not null, "原复选框BindingExpression");
            control.SetCurrentValue(ToggleButton.IsCheckedProperty, value); expression!.UpdateSource(); await Settle();
            Product(ReferenceEquals(expression, control.GetBindingExpression(ToggleButton.IsCheckedProperty)) && control.IsChecked == value, "实际复选框编辑保留原绑定：" + property);
        }
        internal async Task Save()
        {
            Require(ReferenceEquals(SaveButton.Command, Vm.SaveSettingsCommand), "实际保存按钮仍用原VM命令");
            Observe("save-before", new { config = Config(), Vm.Busy, SaveButton.IsEnabled, Vm.SettingsNotice, Vm.Error });
            await Wait(((AsyncCommand)SaveButton.Command!).ExecuteAsync()); await Settle();
            Observe("save-after", new { config = Config(), Vm.Busy, Vm.SettingsNotice, Vm.Error, settings = Runtime.Settings }); Context();
        }
        internal async Task Reject(string reason)
        {
            var before = Config(); var settings = Runtime.Settings; var notice = Vm.SettingsNotice;
            await Save();
            Product(Config() == before && ReferenceEquals(settings, Runtime.Settings), "拒绝保持原文件字节/时间及Runtime.Settings：" + reason, new { before, after = Config(), sameSettings = ReferenceEquals(settings, Runtime.Settings) });
            Product(Vm.Error.Length > 0 && (string.IsNullOrWhiteSpace(Vm.SettingsNotice) || Vm.SettingsNotice == notice) && !Vm.Busy,
                "拒绝有明确反馈、不新增成功通知且Busy恢复：" + reason, new { Vm.Error, Vm.SettingsNotice, Vm.Busy });
        }
        internal void Saved(string reason, Func<AppSettings, bool> expected)
        {
            var disk = new SettingsStore(ConfigPath).Load(); var editor = Vm.Settings.ToSettings();
            Product(Vm.Error.Length == 0 && !Vm.Busy && expected(disk) && expected(Runtime.Settings) && expected(editor), "Editor/Runtime/freshStore一致：" + reason,
                new { disk, runtime = Runtime.Settings, editor, Vm.Error });
        }
        internal async Task NormalClose()
        {
            Require(!Vm.HasActiveOperations && !Vm.Busy && !Window.ldBusy, "原正常关闭前无活动操作");
            closeAttempted = true; NormalCloseRequests++; CleanupOwner = "original-WindowClosing";
            Window.Closing -= OnClose; Window.Closing += Window.WindowClosing; _ = Track(closed.Task); Window.Close();
            await closed.Task.WaitAsync(TimeSpan.FromSeconds(15));
            Product(Window.closeApproved && Closed && !Application.Current.Windows.Cast<Window>().Contains(Window), "原WindowClosing→Shutdown→Closed组件路径完成");
            Observe("normal-close", new { NormalCloseRequests, Window.closeApproved, Closed, fixtureShutdownRequests = ShutdownRequests });
        }
        internal async Task Geometry()
        {
            foreach (var property in new[] { "AdbPath", "ScrcpyPath", "MonitorServerPath", "MonitorFfmpegPath", "LdConsolePath", "FileRoot" })
                await Edit(property, "E:/PRESENTATION-ONLY/" + string.Concat(Enumerable.Repeat("长路径segment/", 18)) + "VISIBLE-TAIL.exe");
            await Edit("AdbSerialsJson", JsonSerializer.Serialize(Enumerable.Range(1, 24).Select(index => "PRESENTATION-ONLY-SERIAL-" + index)));
            await Edit("LdBindingsJson", "{\"1\":\"PRESENTATION-ONLY-LONG-SERIAL-A\",\"2\":\"PRESENTATION-ONLY-LONG-SERIAL-B\"}");
            await Wait(Vm.ExecuteAsync("仅呈现长错误", () => Task.FromException(new InvalidOperationException("仅呈现长错误，不调用设备或网络\n" + string.Join("\n", Enumerable.Repeat("PRESENTATION-ONLY 长错误完整正文 不调用设备或网络", 20))))));
            await Settle();
            var errorScroll = VisualDescendants<ScrollViewer>(Window.Shell).Single(scroll => scroll.Content is TextBlock text && BindingOperations.GetBinding(text, TextBlock.TextProperty)?.Path.Path == nameof(WorkspaceViewModel.Error));
            Product(Math.Abs(errorScroll.ActualHeight - 100) < .6 && errorScroll.ScrollableHeight > 0, "真实公共长错误栏撑满100且可滚动", new { errorScroll.ActualHeight, errorScroll.ScrollableHeight });
            foreach (var control in inputs.Values.Cast<FrameworkElement>().Concat(originalBindings.Keys.OfType<CheckBox>()).Concat(originalCommands.Keys))
            {
                control.BringIntoView(); await Settle(); Hit44(control);
                if (control is TextBox box && box.Text.Length > 120)
                {
                    var original = box.Text; box.Select(box.Text.Length - 1, 1); box.ScrollToEnd(); box.ScrollToHorizontalOffset(double.MaxValue); await Settle();
                    var rect = box.GetRectFromCharacterIndex(box.Text.Length - 1); var visible = rect.IsEmpty ? Rect.Empty : box.TransformToAncestor(Window.Shell).TransformBounds(rect);
                    if (!visible.IsEmpty) visible.Intersect(Clip(box));
                    Product(box.Text == original && !visible.IsEmpty && visible.Height > 0 && visible.Width >= 0, "原长输入可滚读到真实末字且全文保留", new { path = BindingOperations.GetBinding(box, TextBox.TextProperty)?.Path.Path, box.HorizontalOffset, box.VerticalOffset, tail = RectValue(visible) });
                }
            }
            var captions = VisualDescendants<TextBlock>(page).Where(text => text.Text is "连接与接口" or "投屏与窗体" or "简体中文").ToArray();
            Product(captions.Length == 3, "两原分区与固定简体中文展示完整");
            foreach (var caption in captions)
            {
                caption.BringIntoView(); await Settle(); var visible = Bounds(caption); visible.Intersect(Clip(caption));
                Product(caption.IsVisible && !visible.IsEmpty && visible.Width >= caption.ActualWidth - .5 && visible.Height >= caption.ActualHeight - .5, "分区/固定中文语言完整可滚达：" + caption.Text, RectValue(visible));
            }
            foreach (var property in new[] { "MiniMaxSize", "MiniMaxFps", "MiniBitRate" })
            {
                var input = Input(property); Product(!input.IsEnabled, "未接入小屏参数继续禁用：" + property);
                var owner = ParentWithToolTip(input); Require(owner is not null, "禁用参数有原说明"); await ReadDisabledTip(owner!);
            }
            var hardware = originalBindings.Keys.OfType<CheckBox>().Single(check => BindingOperations.GetBinding(check, ToggleButton.IsCheckedProperty)?.Path.Path == "Settings.ForceHardwareDecode");
            Product(!hardware.IsEnabled, "未接入硬件解码仍禁用"); await ReadDisabledTip(hardware);
            var texts = inputs.ToDictionary(pair => pair.Key, pair => pair.Value.Text);
            var checks = originalBindings.Keys.OfType<CheckBox>().ToDictionary(check => check, check => check.IsChecked);
            Window.Width = width == 390 ? 1440 : 390; Window.Height = 900; await Settle(); Window.Width = width; Window.Height = height; await Settle();
            Window.NavigateTo("文本"); await Settle(); Window.NavigateTo("设置"); await Settle();
            Product(texts.All(pair => inputs[pair.Key].Text == pair.Value) && checks.All(pair => pair.Key.IsChecked == pair.Value), "resize/导航往返保持全部原输入与复选框值"); Context();
            Observe("geometry-context", new { width, height, Window.ActualWidth, Window.ActualHeight, controls = inputs.Count + originalCommands.Count + 4, presentationOnly = true });
        }
        private Rect Bounds(FrameworkElement item) => item.TransformToAncestor(Window.Shell).TransformBounds(new Rect(item.RenderSize));
        private Rect Clip(FrameworkElement item)
        {
            var clip = new Rect(Window.Shell.RenderSize);
            for (DependencyObject? parent = item; parent is Visual && !ReferenceEquals(parent, Window.Shell); parent = VisualTreeHelper.GetParent(parent))
                if (parent is FrameworkElement ancestor && (ancestor.ClipToBounds || ancestor is ScrollContentPresenter)) clip.Intersect(Bounds(ancestor));
            return clip;
        }
        private void Hit44(FrameworkElement item)
        {
            var visible = Bounds(item); visible.Intersect(Clip(item));
            Product(item.IsVisible && !visible.IsEmpty && visible.Width >= 43.9 && visible.Height >= 43.9, "实际祖先裁剪后44交互区：" + AutomationProperties.GetName(item), RectValue(visible));
            if (item.IsEnabled && !visible.IsEmpty && visible.Width > 0 && visible.Height > 0)
            {
                var hit = Window.Shell.InputHitTest(new Point(visible.X + visible.Width / 2, visible.Y + visible.Height / 2)) as DependencyObject;
                Product(hit is not null && (ReferenceEquals(item, hit) || item.IsAncestorOf(hit)), "enabled实际命中归原控件");
            }
            else Product(item is TextBox box && new[] { "Settings.MiniMaxSize", "Settings.MiniMaxFps", "Settings.MiniBitRate" }.Contains(BindingOperations.GetBinding(box, TextBox.TextProperty)?.Path.Path)
                || item is CheckBox check && BindingOperations.GetBinding(check, ToggleButton.IsCheckedProperty)?.Path.Path == "Settings.ForceHardwareDecode", "仅4个明确未接入项允许disabled，不以InputHitTest判假红");
        }
        private static FrameworkElement? ParentWithToolTip(FrameworkElement item)
        {
            for (DependencyObject? current = item; current is Visual; current = VisualTreeHelper.GetParent(current))
                if (current is FrameworkElement element && element.ToolTip is not null) return element;
            return null;
        }
        private async Task ReadDisabledTip(FrameworkElement owner)
        {
            owner.BringIntoView(); await Settle(); var original = owner.ToolTip;
            var expected = original as string ?? throw new InvalidOperationException("原禁用原因必须为完整文本");
            var originalBinding = BindingOperations.GetBindingBase(owner, ToolTipProperty);
            var tip = new System.Windows.Controls.ToolTip { Content = expected, PlacementTarget = owner, Placement = PlacementMode.Bottom };
            try
            {
                owner.SetCurrentValue(ToolTipProperty, tip); tip.SetCurrentValue(System.Windows.Controls.ToolTip.IsOpenProperty, true); await Settle();
                Product(ReferenceEquals(originalBinding, BindingOperations.GetBindingBase(owner, ToolTipProperty)), "打开自有原因Popup不替换原BindingBase");
                var text = VisualDescendants<TextBlock>(tip).FirstOrDefault(item => item.Text == expected);
                Product(ToolTipService.GetShowOnDisabled(owner) && tip.IsOpen && tip.IsVisible && tip.ActualWidth > 0 && tip.ActualWidth <= 361
                    && text is not null && text.TextWrapping == TextWrapping.Wrap && text.IsVisible && text.ActualHeight >= text.DesiredSize.Height - .5,
                    "真实禁用原因Popup完整换行可读", new { expected, tip.IsOpen, tip.IsVisible, tip.ActualWidth, tip.ActualHeight, wrap = text?.TextWrapping.ToString(), textHeight = text?.ActualHeight, desiredHeight = text?.DesiredSize.Height });
            }
            finally
            {
                tip.SetCurrentValue(System.Windows.Controls.ToolTip.IsOpenProperty, false); owner.SetCurrentValue(ToolTipProperty, original);
                Product(ReferenceEquals(originalBinding, BindingOperations.GetBindingBase(owner, ToolTipProperty)) && Equals(owner.ToolTip, original), "关闭原因Popup后原值与原绑定完整恢复");
            }
        }
        private static object RectValue(Rect rect) => new { empty = rect.IsEmpty, x = Finite(rect.X), y = Finite(rect.Y), width = Finite(rect.Width), height = Finite(rect.Height) };
        private static double? Finite(double number) => double.IsFinite(number) ? number : null;
        internal async Task Cleanup()
        {
            Require(!cleanupStarted, "唯一清理请求"); cleanupStarted = true; foreach (var signal in releases) signal.TrySetResult(); deadline.Cancel();
            var drain = Task.WhenAll(tasks.ToArray()); try { await drain.WaitAsync(TimeSpan.FromSeconds(15)); } catch { if (!drain.IsCompleted) { Retain(); throw new InvalidOperationException("原任务尚未终态，不释放其资源"); } }
            if (Runtime is null && creation?.IsCompletedSuccessfully == true) Runtime = creation.Result;
            if (creation is not null && !creation.IsCompleted) { Retain(); throw new InvalidOperationException("Runtime创建所有权未解析"); }
            if (closeAttempted)
            {
                if (!closed.Task.IsCompletedSuccessfully || !Window.closeApproved) { Retain(); throw new InvalidOperationException("原正常关闭未完成；不启动第二清理owner"); }
            }
            else
            {
                CleanupOwner = Vm is not null ? "fixture-VM-Shutdown" : Runtime is not null ? "fixture-Runtime-Dispose" : "factory-no-runtime";
                if (Window is not null) { Window.Closing -= OnClose; Window.Loaded -= OnLoaded; LocalizationService.Current.LanguageChanged -= Window.OnLanguageChanged; Window.AbortStartup(); }
                if (Vm is not null) { ShutdownRequests++; shutdown = Vm.ShutdownAsync(); }
                else if (Runtime is not null) { ShutdownRequests++; shutdown = Runtime.DisposeAsync().AsTask(); }
                if (shutdown is not null)
                {
                    _ = Track(shutdown); try { await shutdown.WaitAsync(TimeSpan.FromSeconds(15)); } catch { if (!shutdown.IsCompleted) Retain(); throw; }
                }
            }
            if (Window is not null) { Window.Loaded -= OnLoaded; Window.Closed -= OnClosed; }
            Require(!userClose && tasks.All(task => task.IsCompleted) && Application.Current.Windows.Count == 0, "无用户关闭干扰/未终态任务/残窗");
            if (Runtime is not null) Require(!Runtime.Api.BasicRunning && !Runtime.Api.AutomatorRunning && Runtime.Api.DevicePorts.Count == 0, "自有接口全部结束");
            Application.Current.MainWindow = null; deadline.Dispose();
            Observe("cleanup", new { CleanupOwner, NormalCloseRequests, ShutdownRequests, Closed, Retained, originalTasksTerminal = tasks.All(task => task.IsCompleted) });
            void Retain() { Retained = true; retainedSettingsWorkspaces.Add(this); }
        }
    }
}
