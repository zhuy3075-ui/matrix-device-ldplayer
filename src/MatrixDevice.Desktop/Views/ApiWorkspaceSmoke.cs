using System.Collections.Concurrent;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;
using MatrixDevice.Core;
using MatrixDevice.Host;
using ApiProbeToolTip = System.Windows.Controls.ToolTip;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    private static readonly List<object> retainedApiWorkspaces = [];

    internal static async Task RunApiWorkspaceVerificationAsync(string directory, Dispatcher dispatcher)
    {
        if (!dispatcher.CheckAccess() || Thread.CurrentThread.GetApartmentState() != ApartmentState.STA
            || Application.Current.Windows.Count != 0 || Application.Current.MainWindow is not null
            || !Path.IsPathFullyQualified(directory) || Directory.Exists(directory) || File.Exists(directory))
            throw new InvalidOperationException("接口验收要求fresh绝对目录及无用户窗口的自有STA。");
        Directory.CreateDirectory(directory);
        var reports = new List<object>(); var failures = new List<Exception>(); var stop = false;
        foreach (var size in new[] { (W: 390, H: 640), (W: 390, H: 844), (W: 768, H: 1024), (W: 1000, H: 700), (W: 1440, H: 900), (W: 1920, H: 1080) })
            await Case($"geometry-{size.W}-{size.H}", size.W, size.H, async state =>
            {
                await state.Stage("long-content-pressure", state.Presentation);
                await state.Stage("clipped-control-and-reading-geometry", state.Geometry);
                await state.Stage("six-columns-and-long-fields", state.ReadColumns);
                await state.Stage("owned-shell-render-and-identity", () => { state.Render(); state.Identity(); return Task.CompletedTask; });
            });
        await Case("gateway-lifecycle-error-retry", 1000, 700, state => state.Stage("original-gateway-controls", state.Gateways));
        await Case("device-targets-partial-failure-refresh", 1000, 700, state => state.Stage("original-selection-and-partial-start", state.DeviceTargets));
        await Case("request-protocols-validation-recovery", 1000, 700, state => state.Stage("original-request-controls", state.Requests));
        await Case("in-flight-context-aba", 1000, 700, state => state.Stage("actual-kestrel-context-publication", state.InFlight));
        await Case("busy-disabled-recovery", 1000, 700, state => state.Stage("original-execute-busy-and-recovery", state.Busy));
        await Case("resize-navigation-identity", 1440, 900, state => state.Stage("same-controls-bindings-values-and-targets", state.Resize));
        var summary = new
        {
            status = failures.Count == 0 ? "PASS-OWN-WPF-API-WORKSPACE" : "FAIL-API-WORKSPACE", reports,
            scope = "Owned shown Demo WPF controls and their original AsyncCommand, actual owned Kestrel loopback; no Windows physical input or external UIA.",
            windows = Application.Current.Windows.Count, mainWindowNull = Application.Current.MainWindow is null, retainedOwners = retainedApiWorkspaces.Count,
            notRun = new[] { "physical GUI input", "DPI", "native Clipboard double-click", "real devices/ADB/user APIs", "transfer gate", "device-protocol target change during a controlled in-flight request", "system proxy negative", "redirect response negative", "all lifecycle interleavings", "AP042/AP045", "final dual 30 minutes" },
            errors = failures.Select(error => error.ToString()).ToArray()
        };
        try { File.WriteAllText(Path.Combine(directory, "api-workspace.json"), JsonSerializer.Serialize(summary, JsonDefaults.Options)); }
        catch (Exception error) { failures.Add(new IOException("REPORT-FAIL: 接口验收报告写入失败", error)); }
        if (failures.Count != 0) throw new AggregateException("接口工作区验收未通过", failures);

        async Task Case(string name, int width, int height, Func<ApiWorkspaceCase, Task> action)
        {
            if (stop) { reports.Add(new { name, status = "NOT-RUN", reason = "前例清理未证明原任务及owner已结束" }); return; }
            var state = new ApiWorkspaceCase(Path.Combine(directory, name), width, height, dispatcher);
            var classes = new HashSet<string>(); Exception? failure = null; Exception? cleanup = null; var initialized = false;
            try { await state.Initialize(); initialized = true; await action(state); }
            catch (Exception error) { failure = error; classes.Add(error is ApiWorkspacePrecondition || !initialized ? "PRECONDITION-FAIL" : "FIXTURE-FAIL"); }
            finally { try { await state.Cleanup(); } catch (Exception error) { cleanup = error; classes.Add("CLEANUP-FAIL"); stop = true; } }
            if (state.Failures.Count != 0) classes.Add("PRODUCT-FAIL");
            foreach (var error in state.StageErrors) classes.Add(error.Classification);
            var report = new { name, status = classes.Count > 1 ? "MIXED-FAIL" : classes.SingleOrDefault() ?? "PASS", failureClasses = classes,
                initialized, checks = state.Checks, productFailures = state.Failures, stages = state.Stages, stageErrors = state.StageErrors, observations = state.Observations,
                tasks = state.TaskStates(), error = failure?.ToString(), cleanup = cleanup?.ToString(), state.CleanupOwner, state.ShutdownRequests, state.Closed, state.Retained };
            try { File.WriteAllText(Path.Combine(state.Directory, "case.json"), JsonSerializer.Serialize(report, JsonDefaults.Options)); }
            catch (Exception error) { classes.Add("REPORT-FAIL"); failures.Add(error); }
            reports.Add(classes.Contains("REPORT-FAIL") ? new { name, status = "REPORT-FAIL", preservedResult = (object)report } : (object)report);
            if (classes.Count != 0) failures.Add(new InvalidOperationException(name + ": " + string.Join(",", classes),
                new AggregateException(new[] { failure, cleanup }.OfType<Exception>().Concat(state.StageErrors.Select(item => new Exception(item.Error))).Concat(state.Failures.Select(item => new Exception(item))))));
        }
    }

    private sealed class ApiWorkspacePrecondition(string message) : Exception(message);
    private sealed record ApiWorkspaceStageError(string Stage, string Classification, string Error);
    private sealed class ApiWorkspaceCase(string directory, int width, int height, Dispatcher dispatcher)
    {
        internal string Directory => directory;
        internal MainWindow Window = null!;
        internal WorkspaceViewModel Vm = null!;
        internal MatrixRuntime Runtime = null!;
        internal readonly List<string> Checks = [], Failures = [];
        internal readonly List<object> Observations = [];
        internal readonly Dictionary<string, string> Stages = [];
        internal readonly List<ApiWorkspaceStageError> StageErrors = [];
        internal string CleanupOwner = "not-started";
        internal int ShutdownRequests;
        internal bool Closed, Retained;
        private readonly ConcurrentQueue<(string Name, string Kind, Task Task)> tasks = new();
        private readonly List<Action> releases = [];
        private readonly List<TcpListener> listeners = [];
        private readonly List<TcpClient> clients = [];
        private readonly HttpClient http = new(new HttpClientHandler { UseProxy = false, AllowAutoRedirect = false }) { Timeout = TimeSpan.FromSeconds(5) };
        private readonly CancellationTokenSource deadline = new(TimeSpan.FromSeconds(90));
        private readonly TaskCompletionSource loaded = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly Dictionary<Button, System.Windows.Input.ICommand?> commands = new(ReferenceEqualityComparer.Instance);
        private readonly Dictionary<DataGridColumn, BindingBase> columnBindings = new(ReferenceEqualityComparer.Instance);
        private readonly Dictionary<DependencyObject, BindingBase?> controlBindings = new(ReferenceEqualityComparer.Instance);
        private Task<MatrixRuntime>? creation; private Task? shutdown;
        private FrameworkElement page = null!;
        private DataGrid grid = null!;
        private ComboBox protocol = null!, method = null!;
        private TextBox route = null!, payload = null!, response = null!;
        private TextBlock summary = null!;
        private Button start = null!, stop = null!, deviceStart = null!, deviceStop = null!, send = null!;
        private object[] objects = [];
        private DeviceRow[] rows = [];
        private TcpListener? basicReservation, automatorReservation;
        private bool userClose, cleanupStarted;
        private int step;
        private DeviceRow A => Vm.Devices.Single(row => row.Serial == "DEMO-001");
        private DeviceRow B => Vm.Devices.Single(row => row.Serial == "DEMO-002");

        private static void Require(bool value, string message) { if (!value) throw new ApiWorkspacePrecondition(message); }
        private bool Product(bool value, string expected, object? actual = null)
        {
            Checks.Add((value ? "PASS " : "FAIL ") + expected); Observe("assert", new { expected, actual, passed = value });
            if (!value) Failures.Add(expected); return value;
        }
        private void Observe(string operation, object? actual) => Observations.Add(new { step = ++step, operation, actual });
        private T Track<T>(string name, T task, string kind = "operation") where T : Task { tasks.Enqueue((name, kind, task)); return task; }
        private async Task Wait(Task task, string name = "operation", string kind = "operation")
        { if (!tasks.Any(item => ReferenceEquals(item.Task, task))) Track(name, task, kind); await task.WaitAsync(TimeSpan.FromSeconds(10)); }
        private async Task<T> Wait<T>(Task<T> task, string name = "operation", string kind = "operation")
        { if (!tasks.Any(item => ReferenceEquals(item.Task, task))) Track(name, task, kind); return await task.WaitAsync(TimeSpan.FromSeconds(10)); }
        internal object[] TaskStates() => tasks.Select(item => (object)new { item.Name, item.Kind, status = item.Task.Status.ToString(), terminal = item.Task.IsCompleted }).ToArray();
        internal async Task Stage(string name, Func<Task> action)
        {
            var before = Failures.Count; Stages[name] = "STARTED";
            try { Require(!userClose, "用户未关闭自有窗口"); await action(); Stages[name] = Failures.Count == before ? "PASS" : "PRODUCT-FAIL"; }
            catch (Exception error) { var classification = error is ApiWorkspacePrecondition ? "PRECONDITION-FAIL" : "FIXTURE-FAIL"; Stages[name] = classification; StageErrors.Add(new(name, classification, error.ToString())); }
        }
        private async Task Settle()
        {
            await Wait(dispatcher.InvokeAsync(Window.UpdateLayout, DispatcherPriority.DataBind).Task, "settle-bind", "dispatcher");
            await Wait(dispatcher.InvokeAsync(Window.UpdateLayout, DispatcherPriority.Render).Task, "settle-render", "dispatcher");
            await Wait(dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle).Task, "settle-idle", "dispatcher");
        }

        internal async Task Initialize()
        {
            Require(Application.Current.Windows.Count == 0 && Application.Current.MainWindow is null, "无前例或用户窗口");
            System.IO.Directory.CreateDirectory(directory);
            basicReservation = Reserve(); automatorReservation = Reserve();
            var settings = new AppSettings { WindowWidth = width, WindowHeight = height, Language = "zh-CN", AutoStartApi = false, AutoConnect = false,
                BasicApiPort = Port(basicReservation), AutomatorApiPort = Port(automatorReservation), AgentBaseUrl = null, AgentSerial = null,
                SyncMouse = false, SyncKeyboard = false, AdbPath = "", LdConsolePath = "", AdbAllowedSerials = [], LdSerialBindings = [] };
            creation = Track("runtime-create", MatrixRuntime.CreateAsync(settings, directory, demo: true, ct: deadline.Token));
            Runtime = await Wait(creation); Require(Runtime.IsDemo && Runtime.Backend is DemoDeviceBackend, "真实工厂Demo后端与共享Writes链");
            Vm = new WorkspaceViewModel(Runtime, directory, true); await Wait(Vm.RefreshDevicesAsync(), "initial-devices");
            Require(Vm.Devices.Count == 2, "实际Demo两设备"); Vm.CurrentDevice = B; A.IsSelected = true; Vm.UpdateSelection();
            Window = new MainWindow(Vm) { ShowActivated = false }; Window.Closing -= Window.WindowClosing; Window.Closing += OnClosing;
            Window.Loaded += OnLoaded; Window.Closed += OnClosed; Application.Current.MainWindow = Window;
            Window.Show(); Window.NavigateTo("API"); await Wait(loaded.Task, "window-loaded", "observation"); await Settle();
            Observe("window", new { width, height, actualWidth = F(Window.ActualWidth), actualHeight = F(Window.ActualHeight), Window.IsLoaded, Window.IsVisible, Window.ShowActivated, mode = "OWN-WPF-NOT-PHYSICAL-GUI" });
            Require(Window.IsLoaded && Window.IsVisible && PresentationSource.FromVisual(Window) is not null && Math.Abs(Window.ActualWidth - width) < .6 && Math.Abs(Window.ActualHeight - height) < .6, "实际显示外窗尺寸精确匹配");
            page = (FrameworkElement)((TabItem)Window.tabs.SelectedItem).Content;
            var buttons = VisualDescendants<Button>(page).Where(button => button.Command is AsyncCommand).ToArray();
            Require(buttons.Length == 5, "旧原5按钮存在，非新增替身"); foreach (var button in buttons) commands.Add(button, button.Command);
            start = buttons.Single(button => ReferenceEquals(button.Command, Vm.StartApiCommand)); stop = buttons.Single(button => ReferenceEquals(button.Command, Vm.StopApiCommand));
            deviceStart = buttons.Single(button => ReferenceEquals(button.Command, Vm.StartDeviceApiCommand)); deviceStop = buttons.Single(button => ReferenceEquals(button.Command, Vm.StopDeviceApiCommand));
            send = buttons.Single(button => !new[] { start, stop, deviceStart, deviceStop }.Contains(button));
            protocol = Named<ComboBox>("接口类型"); method = Named<ComboBox>("请求方法"); route = Named<TextBox>("请求路径"); payload = Named<TextBox>("请求正文"); response = Named<TextBox>("响应结果");
            grid = VisualDescendants<DataGrid>(page).Single(item => ReferenceEquals(item.ItemsSource, Vm.DeviceView));
            summary = VisualDescendants<TextBlock>(page).Single(text => BindingOperations.GetBinding(text, TextBlock.TextProperty)?.Path.Path == nameof(WorkspaceViewModel.ApiSummary));
            var textBoxes = VisualDescendants<TextBox>(page).ToArray();
            Observe("raw-control-inventory", new { columns = grid.Columns.Count, comboBoxes = VisualDescendants<ComboBox>(page).Count(), rawTextBoxes = textBoxes.Length,
                textBoxes = textBoxes.Select(box => new { automationName = AutomationProperties.GetName(box), box.Name, comboBoxTemplateDescendant = Parent<ComboBox>(box) is not null,
                    dataGridDescendant = Parent<DataGrid>(box) is not null, box.IsVisible }).ToArray() });
            var ownInputs = textBoxes.Where(box => Parent<ComboBox>(box) is null && Parent<DataGrid>(box) is null).ToArray();
            Require(grid.Columns.Count == 6 && VisualDescendants<ComboBox>(page).Count() == 2 && ownInputs.Length == 3
                && new[] { route, payload, response }.All(expected => ownInputs.Count(actual => ReferenceEquals(actual, expected)) == 1), "原6列/2ComboBox/3TextBox完整");
            foreach (var column in grid.Columns) columnBindings.Add(column, ColumnBinding(column));
            controlBindings.Add(grid, BindingOperations.GetBindingBase(grid, Selector.SelectedItemProperty));
            controlBindings.Add(summary, BindingOperations.GetBindingBase(summary, TextBlock.TextProperty));
            foreach (var box in new[] { route, payload, response }) controlBindings.Add(box, BindingOperations.GetBindingBase(box, TextBox.TextProperty));
            foreach (var choice in new[] { protocol, method }) controlBindings.Add(choice, BindingOperations.GetBindingBase(choice, Selector.SelectedIndexProperty));
            rows = Vm.Devices.ToArray(); objects = Objects();
            Require(!Runtime.Api.BasicRunning && !Runtime.Api.AutomatorRunning && Runtime.Api.DevicePorts.Count == 0 && !Runtime.Settings.AutoStartApi && !Runtime.Settings.AutoConnect, "隔离冷起无用户接口");
        }
        private T Named<T>(string name) where T : FrameworkElement => VisualDescendants<T>(page).Single(item => AutomationProperties.GetName(item) == name);
        private void OnLoaded(object sender, RoutedEventArgs args) => loaded.TrySetResult();
        private void OnClosed(object? sender, EventArgs args) => Closed = true;
        private void OnClosing(object? sender, CancelEventArgs args) { args.Cancel = true; userClose = true; }
        private object[] Objects() => [Vm, Runtime, Runtime.Backend, Runtime.Devices, Runtime.Scripts, Runtime.Input, Runtime.Api, Runtime.Writes, Runtime.SettingsStore, Runtime.Metadata,
            page, grid, Vm.DeviceView, protocol, method, route, payload, response, summary, start, stop, deviceStart, deviceStop, send, .. grid.Columns];
        internal void Identity()
        {
            Product(objects.SequenceEqual(Objects(), ReferenceEqualityComparer.Instance) && rows.SequenceEqual(Vm.Devices, ReferenceEqualityComparer.Instance), "保留原Runtime/组件/控件/六列/DeviceView/设备行身份");
            Product(commands.All(pair => ReferenceEquals(pair.Key.Command, pair.Value)), "保留原5个命令实例");
            Product(columnBindings.All(pair => ReferenceEquals(ColumnBinding(pair.Key), pair.Value)), "六列原BindingBase包括State/ApiAddress MultiBinding保持");
            foreach (var pair in controlBindings)
            {
                var property = pair.Key is DataGrid ? Selector.SelectedItemProperty : pair.Key is ComboBox ? Selector.SelectedIndexProperty : pair.Key is TextBlock ? TextBlock.TextProperty : TextBox.TextProperty;
                Product(ReferenceEquals(BindingOperations.GetBindingBase(pair.Key, property), pair.Value), "保留原输入/摘要/选择绑定或原无绑定状态");
            }
            Product(ReferenceEquals(grid.ItemsSource, Vm.DeviceView) && Runtime.Scripts.GetJobs().Count == 0 && !Vm.Transferring, "未用替代表/脚本/伪传输制造结果");
        }
        private TcpListener Reserve(int port = 0) { var listener = new TcpListener(IPAddress.Loopback, port); listener.Start(); listeners.Add(listener); return listener; }
        private static int Port(TcpListener listener) => ((IPEndPoint)listener.LocalEndpoint).Port;
        private void ReleasePorts() { basicReservation?.Stop(); automatorReservation?.Stop(); }
        private async Task<bool> Command(Button button, string name, bool expectError = false)
        {
            var command = (AsyncCommand)button.Command!;
            if (!Product(button.IsVisible && button.IsEnabled && command.CanExecute(null), "原控件可执行：" + name)) return false;
            Observe("original-command", new { name, buttonName = AutomationProperties.GetName(button) });
            await Wait(command.ExecuteAsync(), name); await Settle();
            Product(expectError ? !string.IsNullOrWhiteSpace(Vm.Error) : string.IsNullOrWhiteSpace(Vm.Error), "原命令错误/恢复反馈：" + name, new { Vm.Error, Vm.Status });
            return true;
        }
        private async Task PrepareEndpoints(bool devices = true)
        {
            ReleasePorts(); await Wait(Runtime.StartApisAsync(deadline.Token), "prepare-owned-gateways");
            if (devices) foreach (var row in new[] { A, B }) await Wait(Runtime.Api.StartDeviceAsync(row.Serial, deadline.Token), "prepare-device-" + row.Serial);
            await Wait(Vm.RefreshDevicesAsync(), "publish-prepared-endpoints"); await Settle();
        }
        private async Task Select(DeviceRow row)
        {
            grid.ScrollIntoView(row); await Settle(); grid.SetCurrentValue(Selector.SelectedItemProperty, row); await Settle();
            Product(ReferenceEquals(Vm.CurrentDevice, row), "原DataGrid选择事件更新单设备目标：" + row.Serial);
        }
        private async Task Checked(DeviceRow row, bool value)
        {
            var column = grid.Columns.OfType<DataGridCheckBoxColumn>().Single(); grid.ScrollIntoView(row, column); await Settle();
            grid.CurrentCell = new(row, column); var began = grid.BeginEdit(); await Settle();
            var content = column.GetCellContent(row); var check = content as CheckBox ?? (content is null ? null : VisualDescendants<CheckBox>(content).FirstOrDefault());
            Require(began && check is not null, "原选择单元格实现真实CheckBox编辑器");
            var binding = BindingOperations.GetBindingExpression(check!, ToggleButton.IsCheckedProperty); Require(binding is not null, "原勾选TwoWay绑定存在");
            check!.SetCurrentValue(ToggleButton.IsCheckedProperty, value); binding!.UpdateSource();
            Product(grid.CommitEdit(DataGridEditingUnit.Cell, true) && grid.CommitEdit(DataGridEditingUnit.Row, true), "原选择Cell/Row实际提交"); await Settle();
            Product(row.IsSelected == value, "原勾选绑定提交目标值：" + row.Serial);
        }
        private void SummaryCount(int count) => Product(Regex.IsMatch(summary.Text, @"设备接口[^\d]{0,16}" + count.ToString(CultureInfo.InvariantCulture) + @"(?!\d)"), "实际绑定摘要含设备接口数量" + count, summary.Text);
        private async Task Addresses()
        {
            foreach (var row in new[] { A, B })
            {
                var expected = Runtime.Api.DevicePorts.TryGetValue(row.Serial, out var port) ? $"http://127.0.0.1:{port}" : "未启动";
                Product(row.ApiAddress == expected, "失败/完成后原行地址反映实际端点：" + row.Serial, new { row.ApiAddress, expected });
            }
            SummaryCount(Runtime.Api.DevicePorts.Count); await ReadColumns(false);
        }
        private async Task Health(int port, string kind, string? serial = null)
        {
            await Wait(Read(), "actual-health-" + kind);
            async Task Read()
            {
                using var result = await http.GetAsync($"http://127.0.0.1:{port}/" + (kind == "automator" ? "api/health" : "health"), deadline.Token);
                var text = await result.Content.ReadAsStringAsync(deadline.Token); using var json = JsonDocument.Parse(text);
                Product(result.StatusCode == HttpStatusCode.OK && (kind == "automator" ? json.RootElement.GetProperty("code").GetInt32() == 0 : json.RootElement.GetProperty("ok").GetBoolean()), "自有HTTP实际health成功：" + kind, new { port, text });
                if (serial is not null) Product(json.RootElement.GetProperty("data").GetProperty("serial").GetString() == serial, "设备health实际serial匹配");
            }
        }

        internal async Task Gateways()
        {
            ReleasePorts(); await Command(start, "start-pair"); await Health(Runtime.Settings.BasicApiPort, "basic"); await Health(Runtime.Settings.AutomatorApiPort, "automator");
            Product(!start.IsEnabled && !Vm.StartApiCommand.CanExecute(null), "双网关已有时禁重复启动");
            await Command(deviceStart, "start-selected-A"); await Addresses();
            await Command(stop, "stop-all"); Product(!Runtime.Api.BasicRunning && !Runtime.Api.AutomatorRunning && Runtime.Api.DevicePorts.Count == 0, "原全停同时结束网关和设备接口"); await Addresses();
            var blocker = Reserve(Runtime.Settings.AutomatorApiPort);
            await Command(start, "occupied-port-error", true);
            Product(!Runtime.Api.BasicRunning && !Runtime.Api.AutomatorRunning && !Vm.Busy, "占端错误后无冷起残留且Busy恢复");
            var client = new TcpClient(AddressFamily.InterNetwork); clients.Add(client); await Wait(client.ConnectAsync(IPAddress.Loopback, Port(blocker), deadline.Token).AsTask(), "own-blocker-connect");
            Product(blocker.Server.IsBound && blocker.Pending(), "未停止自有占用端口监听"); await Addresses(); blocker.Stop();
            await Command(start, "released-port-retry"); await Health(Runtime.Settings.BasicApiPort, "basic"); await Health(Runtime.Settings.AutomatorApiPort, "automator");
            await Command(stop, "final-stop-all"); Identity();
        }

        internal async Task DeviceTargets()
        {
            ReleasePorts(); await Checked(A, true); await Checked(B, false); await Select(B);
            Product(Vm.Targets().SequenceEqual(new[] { A }) && ReferenceEquals(Vm.CurrentDevice, B), "当前B/勾选A使用不同职责");
            await Command(deviceStart, "selected-A-only"); Product(Runtime.Api.DevicePorts.Count == 1 && Runtime.Api.DevicePorts.ContainsKey(A.Serial), "原启用仅作用勾选A");
            await Addresses(); if (Runtime.Api.DevicePorts.TryGetValue(A.Serial, out var aPort)) await Health(aPort, "device", A.Serial);
            Product(!deviceStart.IsEnabled && !Vm.StartDeviceApiCommand.CanExecute(null), "全部目标已有接口时禁重复启用");
            var existingA = Runtime.Api.DevicePorts.GetValueOrDefault(A.Serial);
            await Checked(B, true); await Command(deviceStart, "mixed-A-B");
            Product(Runtime.Api.DevicePorts.Count == 2 && Runtime.Api.DevicePorts.GetValueOrDefault(A.Serial) == existingA, "混合目标只补缺失B并保A端口"); await Addresses();
            foreach (var pair in Runtime.Api.DevicePorts) await Health(pair.Value, "device", pair.Key);
            await Checked(A, false); await Select(A); await Command(deviceStop, "stop-selected-B");
            Product(Runtime.Api.DevicePorts.Count == 1 && Runtime.Api.DevicePorts.ContainsKey(A.Serial), "停止设备接口仅作用勾选B而非当前A"); await Addresses();
            await Checked(B, false); Vm.CurrentDevice = null; await Settle();
            Product(Vm.Targets().Count == 0 && stop.IsEnabled && Vm.StopApiCommand.CanExecute(null), "仅设备接口/无选择时原全停可用");
            await Command(stop, "device-only-stop-with-no-target");
            Product(Runtime.Api.DevicePorts.Count == 0 && !Runtime.Api.BasicRunning && !Runtime.Api.AutomatorRunning, "无目标全停确实结束全部接口");
            await Wait(Runtime.StopApisAsync(deadline.Token), "preparation-reset-after-stop-check");
            await Checked(A, true); await Checked(B, true); await Select(A);
            ((DemoDeviceBackend)Runtime.Backend).SetOnline(B.Serial, false); await Wait(Runtime.Devices.RefreshAsync(deadline.Token), "catalog-only-B-offline");
            Require(A.Info.Online && B.Info.Online && !Runtime.Devices.Snapshot.Single(item => item.Serial == B.Serial).Online, "实际catalog B离线而原VM暂旧，建立公开部分成功前置");
            await Command(deviceStart, "partial-A-success-B-failure", true);
            Product(Runtime.Api.DevicePorts.Count == 1 && Runtime.Api.DevicePorts.ContainsKey(A.Serial) && !Runtime.Api.DevicePorts.ContainsKey(B.Serial), "批次A成功/B失败真实保留而不回滚A");
            Product(!string.IsNullOrWhiteSpace(Vm.Error) && Vm.Error.Contains("错误码 1003", StringComparison.Ordinal) && Vm.Status == "操作失败", "部分失败保留正确本地化错误码且不宣称全部完成", new { Vm.Error, Vm.Status });
            await Addresses(); if (Runtime.Api.DevicePorts.TryGetValue(A.Serial, out aPort)) await Health(aPort, "device", A.Serial);
            ((DemoDeviceBackend)Runtime.Backend).SetOnline(B.Serial, true); await Wait(Vm.RefreshDevicesAsync(), "B-recovery");
            await Command(deviceStart, "partial-retry"); Product(Runtime.Api.DevicePorts.Count == 2, "恢复B后原启用命令可重试"); await Addresses(); Identity();
        }

        private async Task Request(int selectedProtocol, int selectedMethod, string path, string body = "{}")
        {
            protocol.SetCurrentValue(Selector.SelectedIndexProperty, selectedProtocol); method.SetCurrentValue(Selector.SelectedIndexProperty, selectedMethod);
            route.SetCurrentValue(TextBox.TextProperty, path); payload.SetCurrentValue(TextBox.TextProperty, body); await Settle();
        }
        private string ResponseContext()
        {
            var texts = VisualDescendants<TextBlock>(page).Where(text => !ReferenceEquals(text, summary) && Parent<DataGrid>(text) is null && Parent<ComboBox>(text) is null && Parent<Button>(text) is null && Parent<TextBox>(text) is null);
            return response.Text + "\n" + string.Join("\n", texts.Where(text => text.IsVisible).Select(text => text.Text));
        }
        private void Display(int status, int port, string path, string verb, string? serial = null)
        {
            Product(Regex.IsMatch(response.Text, @"HTTP\s+" + status + @"(?!\d)"), "原响应控件显示实际HTTP状态" + status, response.Text);
            var context = ResponseContext(); Product(context.Contains(port.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal) && context.Contains(path, StringComparison.Ordinal)
                && context.Contains(verb, StringComparison.Ordinal) && (serial is null || context.Contains(serial, StringComparison.Ordinal)), "实际响应展示含捕获的端口/方法/路径/设备协议serial", context);
        }
        internal async Task Requests()
        {
            await PrepareEndpoints();
            await Request(0, 0, "/health"); await Command(send, "basic-GET"); Display(200, Runtime.Settings.BasicApiPort, "/health", "GET");
            await Request(1, 0, "/api/health"); await Command(send, "automator-GET"); Display(200, Runtime.Settings.AutomatorApiPort, "/api/health", "GET");
            await Select(A); await Request(2, 0, "/health"); await Command(send, "device-A-GET"); Display(200, Runtime.Api.DevicePorts[A.Serial], "/health", "GET", A.Serial);
            await Select(B); Product(!response.Text.Contains(A.Serial, StringComparison.Ordinal), "设备协议换目标立即清除旧A响应");
            await Command(send, "device-B-GET"); Display(200, Runtime.Api.DevicePorts[B.Serial], "/health", "GET", B.Serial);
            await Select(A); Product(!response.Text.Contains(B.Serial, StringComparison.Ordinal), "设备协议B返回A不保留旧B响应");
            await Command(send, "device-A-return-GET"); Display(200, Runtime.Api.DevicePorts[A.Serial], "/health", "GET", A.Serial);
            await Request(0, 0, "/not-a-route"); await Command(send, "actual-404"); Display(404, Runtime.Settings.BasicApiPort, "/not-a-route", "GET");
            await Request(1, 1, "/api/app/list", "{\"serial\":\"DEMO-001\",\"showSystem\":true}"); await Command(send, "actual-Demo-read-POST");
            Display(200, Runtime.Settings.AutomatorApiPort, "/api/app/list", "POST"); Product(response.Text.Contains("com.android.settings", StringComparison.Ordinal), "POST返回真实Demo应用列表");
            foreach (var invalid in new[] { (Path: "/api/app/list", Body: "{"), (Path: "http://127.0.0.1/health", Body: "{}"), (Path: "//127.0.0.1/health", Body: "{}"), (Path: "health", Body: "{}") })
            {
                await Request(1, 1, invalid.Path, invalid.Body); await Command(send, "invalid-request", true);
                Product(!response.Text.Contains("com.android.settings", StringComparison.Ordinal), "错误请求不保留旧成功正文");
                await Request(1, 0, "/api/health"); await Command(send, "corrected-request"); Display(200, Runtime.Settings.AutomatorApiPort, "/api/health", "GET");
            }
            Identity();
        }

        internal async Task InFlight()
        {
            await PrepareEndpoints();
            foreach (var change in new[] { "protocol", "method", "path", "body", "body-A-B-A" })
            {
                var nonce = "old-" + change + "-" + Guid.NewGuid().ToString("N");
                var entered = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
                var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously); var drained = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                releases.Add(() => { release.TrySetResult(); entered.TrySetCanceled(); drained.TrySetCanceled(); });
                Runtime.Automation.ServiceInfoProvider = (serial, ct) => Track("provider-" + change, Provider(serial, ct));
                async Task<object?> Provider(string serial, CancellationToken ct) { entered.TrySetResult(serial); await release.Task.WaitAsync(ct); return new { serial, nonce }; }
                EventHandler onDrained = (_, _) => drained.TrySetResult(); Runtime.Api.RequestsDrained += onDrained;
                try
                {
                    await Request(1, 1, "/api/service/info", "{\"serial\":\"DEMO-001\"}");
                    Require(send.IsEnabled && send.Command!.CanExecute(null), "实际在途请求可接纳");
                    var pending = Track("send-" + change, ((AsyncCommand)send.Command!).ExecuteAsync());
                    var actualSerial = await Wait(entered.Task, "provider-entered", "observation");
                    Require(actualSerial == A.Serial && Runtime.Api.HasActiveRequests && !pending.IsCompleted, "真实Kestrel handler已进入自有屏障");
                    if (change == "protocol") protocol.SetCurrentValue(Selector.SelectedIndexProperty, 0);
                    if (change == "method") method.SetCurrentValue(Selector.SelectedIndexProperty, 0);
                    if (change == "path") route.SetCurrentValue(TextBox.TextProperty, "/api/health");
                    if (change is "body" or "body-A-B-A") payload.SetCurrentValue(TextBox.TextProperty, "{\"serial\":\"DEMO-002\"}");
                    await Settle();
                    if (change == "body-A-B-A") { payload.SetCurrentValue(TextBox.TextProperty, "{\"serial\":\"DEMO-001\"}"); await Settle(); }
                    var modified = InputValues(); release.TrySetResult(); await Wait(pending); await Wait(drained.Task, "handler-drained", "observation"); await Settle();
                    Product(!response.Text.Contains(nonce, StringComparison.Ordinal), "在途旧响应不回填变化后的上下文：" + change, new { nonce, response = response.Text, modified });
                    Product(modified == InputValues(), "旧完成不改新输入：" + change);
                    Observe("in-flight", new { change, nonce, serial = actualSerial, pendingTerminal = pending.IsCompleted, handlersDrained = !Runtime.Api.HasActiveRequests, noPostRollbackClaim = true });
                }
                finally { release.TrySetResult(); Runtime.Api.RequestsDrained -= onDrained; }
            }
            const string valid = "same-context-complete";
            Runtime.Automation.ServiceInfoProvider = (serial, _) => Task.FromResult<object?>(new { serial, nonce = valid });
            await Request(1, 1, "/api/service/info", "{\"serial\":\"DEMO-001\"}"); await Command(send, "valid-same-context");
            Product(response.Text.Contains(valid, StringComparison.Ordinal), "同一上下文合法结果可显示");
            var old = response.Text; await Select(B); Product(response.Text == old, "全局Automator结果不误归单设备CurrentDevice变化");
            await Wait(Vm.RefreshDevicesAsync(), "ordinary-directory-refresh"); await Settle();
            Product(response.Text == old, "实际endpoint未变的普通目录刷新不清合法结果"); Identity();
        }

        internal async Task Busy()
        {
            await PrepareEndpoints(); await Request(0, 0, "/health"); await Command(send, "before-busy-request");
            var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously); releases.Add(() => release.TrySetResult());
            var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously); releases.Add(() => entered.TrySetCanceled());
            var busy = Track("actual-vm-busy", Vm.ExecuteAsync("自有Busy门", async () => { entered.TrySetResult(); await release.Task.WaitAsync(deadline.Token); }));
            await Wait(entered.Task, "busy-entered", "observation"); await Settle();
            Require(Vm.Busy && !busy.IsCompleted, "原ExecuteAsync正在运行");
            foreach (var button in commands.Keys) { Product(!button.IsEnabled && !button.Command!.CanExecute(null), "Busy禁用原按钮：" + AutomationProperties.GetName(button)); await DisabledReason(button); }
            var before = response.Text; await Wait(((AsyncCommand)send.Command!).ExecuteAsync(), "disabled-send-noop"); Product(response.Text == before && Vm.Busy, "Busy拒绝重复请求且不更改旧正文");
            release.TrySetResult(); await Wait(busy); await Settle(); Product(!Vm.Busy && send.IsEnabled && stop.IsEnabled, "Busy真实完成后可用命令恢复");
            await Command(send, "after-busy-request"); Display(200, Runtime.Settings.BasicApiPort, "/health", "GET");
            Observe("not-run", new { step = "transfer-gate", reason = "未引入转发Backend，不伪造Transferring；仅真实Busy已验" }); Identity();
        }

        private string InputValues() => JsonSerializer.Serialize(new { protocol = protocol.SelectedIndex, method = method.SelectedIndex, route = route.Text, payload = payload.Text });
        internal async Task Resize()
        {
            await PrepareEndpoints(); await Checked(A, true); await Checked(B, false); await Select(B);
            await Request(2, 0, "/health"); await Command(send, "before-resize-request"); var values = InputValues(); var result = response.Text;
            var current = Vm.CurrentDevice; var selected = Vm.Devices.Select(row => row.IsSelected).ToArray();
            foreach (var size in new[] { (W: 390, H: 640), (W: 390, H: 844), (W: 768, H: 1024), (W: 1000, H: 700), (W: 1440, H: 900), (W: 1920, H: 1080) })
            {
                Window.Width = size.W; Window.Height = size.H; await Settle();
                Product(Math.Abs(Window.ActualWidth - size.W) < .6 && Math.Abs(Window.ActualHeight - size.H) < .6, "resize实际六尺寸不夹取", new { size.W, size.H, actualW = F(Window.ActualWidth), actualH = F(Window.ActualHeight) });
                Window.NavigateTo("文本"); await Settle(); Window.NavigateTo("API"); await Settle();
                Product(values == InputValues() && result == response.Text && ReferenceEquals(current, Vm.CurrentDevice) && selected.SequenceEqual(Vm.Devices.Select(row => row.IsSelected)), "resize/离页返回保持原输入/合法响应/当前目标/勾选"); Identity();
            }
        }

        internal async Task Presentation()
        {
            await PrepareEndpoints();
            foreach (var row in new[] { A, B }) row.Info = row.Info with { Name = "PRESENTATION-ONLY-" + new string('N', 150) + "-NAME-TAIL", GroupId = new string('G', 110) + "-GROUP-TAIL" };
            Runtime.Automation.ServiceInfoProvider = (serial, _) => Task.FromResult<object?>(new { serial, presentationOnly = true, content = string.Join("\n", Enumerable.Range(0, 100).Select(index => index + ":" + new string('R', 80))) + "-RESPONSE-TAIL" });
            await Request(1, 1, "/api/service/info?presentation=" + new string('q', 260) + "-ROUTE-TAIL", "{\n\"serial\":\"DEMO-001\",\n\"presentationOnly\":\"" + new string('p', 900) + "-PAYLOAD-TAIL\"\n}");
            await Command(send, "presentation-read-response"); Require(response.Text.Contains("RESPONSE-TAIL", StringComparison.Ordinal), "自有Kestrel长响应已真实加载");
            var longError = "接口呈现专用长错误，无设备写\n" + string.Join("\n", Enumerable.Repeat("接口长错误的完整可读正文", 24)) + "\nLONG-ERROR-TAIL";
            await Wait(Vm.ExecuteAsync("仅呈现长错误", () => Task.FromException(new InvalidOperationException(longError))), "presentation-error"); await Settle();
            Observe("long-error-precondition", new { expectedLength = longError.Length, actualLength = Vm.Error.Length, Vm.Error });
            Require(Vm.Error.Length >= longError.Length && Vm.Error.Contains("LONG-ERROR-TAIL", StringComparison.Ordinal), "实际中文长错误完整保留且未归一为短文");
            var error = VisualDescendants<ScrollViewer>(Window.Shell).Single(scroll => scroll.Content is TextBlock text && BindingOperations.GetBinding(text, TextBlock.TextProperty)?.Path.Path == nameof(WorkspaceViewModel.Error));
            Product(Math.Abs(error.ActualHeight - 100) < .6 && error.ScrollableHeight > 0, "公共长错误真实撑满100且可滚读", new { height = F(error.ActualHeight), scrollable = F(error.ScrollableHeight) });
            Observe("presentation-only", new { longNames = true, longGroups = true, longRequest = true, longActualOwnedResponse = true, syntheticDevices = 0, realDeviceWrites = 0 });
        }

        internal async Task Geometry()
        {
            foreach (var pair in new[] { (start, "启动网关"), (stop, "停止全部接口"), (deviceStart, "启用设备接口"), (deviceStop, "停止设备接口"), (send, "发送请求") })
                Product(AutomationProperties.GetName(pair.Item1) == pair.Item2, "原按钮明确职责名称：" + pair.Item2, AutomationProperties.GetName(pair.Item1));
            var address = grid.Columns.Single(column => PathOf(column) == "ApiAddress"); var heading = address.Header?.ToString() ?? "";
            Product(heading.Contains("设备", StringComparison.Ordinal) && heading.Contains("地址", StringComparison.Ordinal) && !heading.Contains("普通", StringComparison.Ordinal), "地址列不误称普通网关地址", heading);
            foreach (var control in commands.Keys.Cast<FrameworkElement>().Concat(new FrameworkElement[] { protocol, method, route, payload, response }))
            { control.BringIntoView(); await Settle(); Hit44(control); if (!control.IsEnabled) await DisabledReason(control); }
            grid.BringIntoView(); await Settle(); var table = Reading(grid); Product(!table.IsEmpty && table.Width >= 100 && table.Height >= 95.9, "表格真实正文视口至少100x96", R(table));
            response.BringIntoView(); await Settle(); var output = Reading(response); Product(!output.IsEmpty && output.Width >= 99.9 && output.Height >= 47.9, "响应真实阅读视口至少100x48", R(output));
            foreach (var box in new[] { route, payload, response }) await ReadBox(box);
            Product(page.ActualWidth <= Window.Shell.ActualWidth + .6, "接口页本体不水平溢出Shell"); SummaryCount(2); Identity();
        }
        private Rect Bounds(FrameworkElement item) => item.TransformToAncestor(Window.Shell).TransformBounds(new Rect(item.RenderSize));
        private Rect Clip(FrameworkElement item)
        {
            var clip = new Rect(Window.Shell.RenderSize);
            for (DependencyObject? current = item; current is Visual && !ReferenceEquals(current, Window.Shell); current = VisualTreeHelper.GetParent(current))
                if (current is FrameworkElement element && (element.ClipToBounds || element is ScrollContentPresenter)) clip.Intersect(Bounds(element));
            return clip;
        }
        private Rect Reading(FrameworkElement item)
        {
            var presenter = VisualDescendants<ScrollContentPresenter>(item).FirstOrDefault(); var bounds = Bounds(presenter ?? item); bounds.Intersect(Clip(presenter ?? item)); return bounds;
        }
        private void Hit44(FrameworkElement control)
        {
            var visible = Bounds(control); visible.Intersect(Clip(control));
            Product(control.IsVisible && !visible.IsEmpty && visible.Width >= 43.9 && visible.Height >= 43.9, "祖先裁剪后真实44命中区：" + AutomationProperties.GetName(control), R(visible));
            if (control.IsEnabled && !visible.IsEmpty && visible.Width > 0 && visible.Height > 0)
            {
                var hit = Window.Shell.InputHitTest(new Point(visible.X + visible.Width / 2, visible.Y + visible.Height / 2)) as DependencyObject;
                Product(hit is not null && (ReferenceEquals(hit, control) || control.IsAncestorOf(hit)), "enabled中心命中归原控件");
            }
            else Product(!control.IsEnabled && control is Button button && button.Command is not null && !button.Command.CanExecute(null), "disabled只核原命令不可执行，不伪造enabled命中");
        }
        private async Task ReadBox(TextBox box)
        {
            box.BringIntoView(); await Settle();
            var expected = box.Text; Product(expected.Length > 100, "长输入/响应样本真实存在");
            box.ScrollToHome(); box.ScrollToHorizontalOffset(0); await Settle(); Character(box, 0, "首字符");
            box.ScrollToEnd(); box.ScrollToHorizontalOffset(double.MaxValue); await Settle(); Character(box, Math.Max(0, expected.Length - 1), "末字符TAIL");
            Product(box.Text == expected, "滚读不修改原完整文本");
        }
        private void Character(TextBox box, int index, string label)
        {
            var character = box.GetRectFromCharacterIndex(index); var visible = character.IsEmpty ? Rect.Empty : box.TransformToAncestor(Window.Shell).TransformBounds(character);
            if (!visible.IsEmpty) visible.Intersect(Reading(box));
            Product(!visible.IsEmpty && visible.Height > 0 && visible.Width >= 0, "实际文本" + label + "进入裁剪后阅读视口", R(visible));
        }
        private async Task DisabledReason(FrameworkElement control)
        {
            var original = control.ToolTip; var text = original as string ?? (original as ApiProbeToolTip)?.Content?.ToString();
            Product(!string.IsNullOrWhiteSpace(text) && text != AutomationProperties.GetName(control), "禁用原因不只是按钮名称", text);
            if (string.IsNullOrWhiteSpace(text)) return;
            await Tooltip(control, text, true);
        }
        private async Task<bool> Tooltip(FrameworkElement owner, string expected, bool disabled = false)
        {
            var original = owner.ToolTip; var binding = BindingOperations.GetBindingBase(owner, ToolTipProperty);
            var tip = original as ApiProbeToolTip ?? new ApiProbeToolTip { Content = original, PlacementTarget = owner, Placement = PlacementMode.Bottom }; var wasOpen = tip.IsOpen;
            try
            {
                if (original is not ApiProbeToolTip) owner.SetCurrentValue(ToolTipProperty, tip); tip.SetCurrentValue(ApiProbeToolTip.IsOpenProperty, true); await Settle();
                var text = VisualDescendants<TextBlock>(tip).FirstOrDefault(item => item.Text == expected);
                var readable = tip.IsOpen && tip.IsVisible && tip.ActualWidth > 0 && tip.ActualWidth <= 361 && text is not null && text.IsVisible
                    && text.TextWrapping == TextWrapping.Wrap && text.ActualHeight + .5 >= text.DesiredSize.Height && (!disabled || ToolTipService.GetShowOnDisabled(owner));
                Product(ReferenceEquals(binding, BindingOperations.GetBindingBase(owner, ToolTipProperty)), "打开提示不替换原BindingBase");
                Observe("actual-tooltip", new { expected, readable, tip.IsOpen, tip.IsVisible, width = F(tip.ActualWidth), height = F(tip.ActualHeight), wrapping = text?.TextWrapping.ToString() });
                if (disabled) Product(readable, "实际禁用原因Popup完整可读"); return readable;
            }
            finally { tip.SetCurrentValue(ApiProbeToolTip.IsOpenProperty, wasOpen); if (original is not ApiProbeToolTip) owner.SetCurrentValue(ToolTipProperty, original); }
        }
        internal async Task ReadColumns() => await ReadColumns(true);
        private async Task ReadColumns(bool full)
        {
            Product(grid.Columns.Select(PathOf).ToHashSet(StringComparer.Ordinal).SetEquals(new[] { "IsSelected", "Serial", "Name", "Group", "State", "ApiAddress" }), "原六列数据路径完整");
            foreach (var row in new[] { A, B }) foreach (var column in grid.Columns)
            {
                grid.BringIntoView(); grid.ScrollIntoView(row, column); await Settle(); var content = column.GetCellContent(row); var path = PathOf(column);
                if (!Product(content is not null, "真实原列已实现：" + row.Serial + "/" + path)) continue;
                if (path == "IsSelected")
                {
                    var check = content as CheckBox ?? VisualDescendants<CheckBox>(content!).FirstOrDefault();
                    Product(check is not null && check.IsChecked == row.IsSelected, "选择列绑定值可见且不当数据文本");
                    if (full)
                    {
                        var cell = Parent<DataGridCell>(content!);
                        if (Product(cell is not null && ReferenceEquals(cell.Column, column), "展示选择glyph由原选择Cell负责进入编辑")) Hit44(cell!);
                    }
                    continue;
                }
                var text = content as TextBlock ?? VisualDescendants<TextBlock>(content!).FirstOrDefault();
                var expected = path switch { "Serial" => row.Serial, "Name" => row.Name, "Group" => row.Group, "State" => row.State, "ApiAddress" => row.ApiAddress, _ => "" };
                if (!Product(text is not null && text.Text == expected, "原字段全文值保留：" + path)) continue;
                var visible = Bounds(text!); visible.Intersect(Reading(grid)); Product(!visible.IsEmpty && visible.Height > 0 && visible.Width >= Math.Min(20, text!.ActualWidth), "全部列可横向滚达：" + path, R(visible));
                if (full)
                {
                    var measured = new FormattedText(expected, CultureInfo.CurrentCulture, text!.FlowDirection, new Typeface(text.FontFamily, text.FontStyle, text.FontWeight, text.FontStretch), text.FontSize, Brushes.Black, VisualTreeHelper.GetDpi(text).PixelsPerDip);
                    var owner = text.ToolTip?.ToString() == expected ? (FrameworkElement)text : Parent<DataGridCell>(text);
                    var readable = measured.WidthIncludingTrailingWhitespace <= visible.Width + .75 || owner?.ToolTip?.ToString() == expected && await Tooltip(owner, expected);
                    Product(readable, "长字段通过真实完整可见文本或实际Popup读全：" + path, new { expected, visible = R(visible), width = F(measured.WidthIncludingTrailingWhitespace) });
                }
            }
        }
        private static BindingBase ColumnBinding(DataGridColumn column) => column switch { DataGridTextColumn text => text.Binding, DataGridCheckBoxColumn check => check.Binding, _ => throw new InvalidOperationException("未预期的数据列类型") };
        private static string PathOf(DataGridColumn column)
        {
            var binding = ColumnBinding(column); if (binding is Binding single) return single.Path.Path;
            if (binding is MultiBinding multi && multi.Bindings.FirstOrDefault() is Binding first && first.Path.Path is "State" or "ApiAddress") return first.Path.Path;
            throw new InvalidOperationException("原列绑定路径未知");
        }
        private static T? Parent<T>(DependencyObject item) where T : DependencyObject
        { for (DependencyObject? current = item; current is Visual; current = VisualTreeHelper.GetParent(current)) if (current is T match) return match; return null; }
        private static double? F(double value) => double.IsFinite(value) ? value : null;
        private static object R(Rect rect) => new { empty = rect.IsEmpty, x = F(rect.X), y = F(rect.Y), width = F(rect.Width), height = F(rect.Height) };
        internal void Render()
        {
            var path = Path.Combine(directory, "owned-api-shell.png"); MainWindow.SaveRender(path, Window.Shell);
            using var stream = File.OpenRead(path); Observe("owned-shell-png", new { path, bytes = stream.Length, sha256 = Convert.ToHexString(SHA256.HashData(stream)), desktopCapture = false });
        }
        internal async Task Cleanup()
        {
            Require(!cleanupStarted, "唯一清理owner"); cleanupStarted = true; foreach (var release in releases) release(); deadline.Cancel();
            var drain = Track("original-task-drain", Task.WhenAll(tasks.Select(item => item.Task).ToArray()), "cleanup");
            try { await drain.WaitAsync(TimeSpan.FromSeconds(15)); } catch { if (!drain.IsCompleted) { Retain(); throw new InvalidOperationException("原任务未排空，不释放其上游owner"); } }
            if (Runtime is null && creation?.IsCompletedSuccessfully == true) Runtime = creation.Result;
            if (creation is not null && !creation.IsCompleted) { Retain(); throw new InvalidOperationException("创建任务owner未解析"); }
            if (Window is not null) { Window.Closing -= OnClosing; Window.Loaded -= OnLoaded; LocalizationService.Current.LanguageChanged -= Window.OnLanguageChanged; Window.AbortStartup(); }
            CleanupOwner = Vm is not null ? "VM-Shutdown" : Runtime is not null ? "Runtime-Dispose" : "factory-no-runtime";
            if (Vm is not null) { ShutdownRequests++; shutdown = Track("VM-shutdown", Vm.ShutdownAsync(), "cleanup"); }
            else if (Runtime is not null) { ShutdownRequests++; shutdown = Track("runtime-dispose", Runtime.DisposeAsync().AsTask(), "cleanup"); }
            if (shutdown is not null) try { await shutdown.WaitAsync(TimeSpan.FromSeconds(15)); } catch { Retain(); throw; }
            if (Window is not null) Window.Closed -= OnClosed;
            foreach (var listener in listeners) listener.Stop(); foreach (var client in clients) client.Dispose(); http.Dispose();
            Require(tasks.All(item => item.Task.IsCompleted) && !userClose && Application.Current.Windows.Count == 0, "原任务全部终态且无用户干扰/残窗");
            if (Runtime is not null) Require(!Runtime.Api.BasicRunning && !Runtime.Api.AutomatorRunning && Runtime.Api.DevicePorts.Count == 0 && !Runtime.Api.HasActiveRequests, "原Runtime全部owned服务/请求结束");
            Application.Current.MainWindow = null; deadline.Dispose(); Observe("cleanup", new { CleanupOwner, ShutdownRequests, Closed, Retained, pending = tasks.Count(item => !item.Task.IsCompleted), windows = Application.Current.Windows.Count });
            void Retain() { Retained = true; retainedApiWorkspaces.Add(this); }
        }
    }
}
