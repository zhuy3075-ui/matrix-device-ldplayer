using System.Collections.Concurrent;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using MatrixDevice.Core;
using MatrixDevice.Features;
using MatrixDevice.Host;
using TerminalProbeToolTip = System.Windows.Controls.ToolTip;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    private static readonly List<object> retainedTerminalWorkspaces = [];

    internal static async Task RunTerminalWorkspaceVerificationAsync(string directory, Dispatcher dispatcher)
    {
        if (!dispatcher.CheckAccess() || Thread.CurrentThread.GetApartmentState() != ApartmentState.STA
            || Application.Current.Windows.Count != 0 || Application.Current.MainWindow is not null
            || !Path.IsPathFullyQualified(directory) || Directory.Exists(directory) || File.Exists(directory))
            throw new InvalidOperationException("终端验收要求 fresh 绝对目录及无用户窗口的自有 STA。");
        Directory.CreateDirectory(directory);
        var reports = new List<object>(); var failures = new List<Exception>(); var stop = false;
        foreach (var size in new[] { (W: 390, H: 640), (W: 390, H: 844), (W: 768, H: 1024), (W: 1000, H: 700), (W: 1440, H: 900), (W: 1920, H: 1080) })
            await Case($"geometry-{size.W}-{size.H}", size.W, size.H, async state =>
            {
                await state.Stage("actual-history-pressure", state.Presentation);
                await state.Stage("original-control-and-reading-geometry", state.Geometry);
                await state.Stage("history-first-middle-last-and-long-output", state.ReadHistory);
                await state.Stage("owned-render-and-original-identity", () => { state.Render(); state.Identity(); return Task.CompletedTask; });
            });
        await Case("success-drafts-unchanged-ab-aba", 1000, 700, state => state.Stage("actual-success-and-edit-revisions", () => state.Drafts(false)));
        await Case("failure-drafts-unchanged-ab-aba", 1000, 700, state => state.Stage("actual-failure-and-edit-revisions", () => state.Drafts(true)));
        await Case("original-target-and-history-attribution", 1000, 700, state => state.Stage("single-target-not-selected-targets", state.Targets));
        await Case("original-enter-history-and-double-click", 1000, 700, state => state.Stage("owned-routed-handlers-not-physical-input", state.Keys));
        await Case("original-empty-offline-busy-script-transfer-gates", 1000, 700, state => state.Stage("actual-owned-operations-and-original-command", state.Gates));
        await Case("resize-navigation-and-control-identity", 1440, 900, state => state.Stage("same-original-controls-and-bindings", state.Resize));
        var summary = new
        {
            status = failures.Count == 0 ? "PASS-OWN-WPF-TERMINAL-WORKSPACE" : "FAIL-TERMINAL-WORKSPACE", reports,
            scope = "Owned shown WPF original ShellInput binding, ShellCommand, history and routed handlers; transparent Demo backend, actual shared Writes and actual script/transfer tasks. No physical Windows input or external UIA.",
            windows = Application.Current.Windows.Count, mainWindowNull = Application.Current.MainWindow is null, retainedOwners = retainedTerminalWorkspaces.Count,
            notRun = new[] { "physical GUI/Windows keyboard and mouse", "DPI", "native Clipboard contents and system copy", "real ADB/guest Shell", "all I/O and Runtime replacement interleavings", "AP042/AP045", "real application GUI install/uninstall", "RPA/MCP/capture full flow", "final dual 30 minutes" },
            errors = failures.Select(error => error.ToString()).ToArray()
        };
        try { File.WriteAllText(Path.Combine(directory, "terminal-workspace.json"), JsonSerializer.Serialize(summary, JsonDefaults.Options)); }
        catch (Exception error) { failures.Add(new IOException("REPORT-FAIL: 终端验收报告写入失败", error)); }
        if (failures.Count != 0) throw new AggregateException("终端工作区验收未通过", failures);

        async Task Case(string name, int width, int height, Func<TerminalWorkspaceCase, Task> action)
        {
            if (stop) { reports.Add(new { name, status = "NOT-RUN", reason = "前例清理未证明原任务及 owner 已结束" }); return; }
            var state = new TerminalWorkspaceCase(Path.Combine(directory, name), width, height, dispatcher);
            var classes = new HashSet<string>(); Exception? failure = null; Exception? cleanup = null; var initialized = false;
            try { await state.Initialize(); initialized = true; await action(state); }
            catch (Exception error) { failure = error; classes.Add(error is TerminalWorkspacePrecondition || !initialized ? "PRECONDITION-FAIL" : "FIXTURE-FAIL"); }
            finally { try { await state.Cleanup(); } catch (Exception error) { cleanup = error; classes.Add("CLEANUP-FAIL"); stop = true; } }
            if (state.Failures.Count != 0) classes.Add("PRODUCT-FAIL");
            foreach (var error in state.StageErrors) classes.Add(error.Classification);
            var report = new { name, status = classes.Count > 1 ? "MIXED-FAIL" : classes.SingleOrDefault() ?? "PASS", failureClasses = classes,
                initialized, checks = state.Checks, productFailures = state.Failures, stages = state.Stages, stageErrors = state.StageErrors,
                observations = state.Observations, tasks = state.TaskStates(), backend = state.Backend.Report(), error = failure?.ToString(), cleanup = cleanup?.ToString(),
                state.CleanupOwner, state.ShutdownRequests, state.Closed, state.Retained };
            try { File.WriteAllText(Path.Combine(state.Directory, "case.json"), JsonSerializer.Serialize(report, JsonDefaults.Options)); }
            catch (Exception error) { classes.Add("REPORT-FAIL"); failures.Add(error); }
            reports.Add(classes.Contains("REPORT-FAIL") ? new { name, status = "REPORT-FAIL", preservedResult = (object)report } : (object)report);
            if (classes.Count != 0) failures.Add(new InvalidOperationException(name + ": " + string.Join(",", classes),
                new AggregateException(new[] { failure, cleanup }.OfType<Exception>().Concat(state.StageErrors.Select(item => new Exception(item.Error))).Concat(state.Failures.Select(item => new Exception(item))))));
        }
    }

    private sealed class TerminalWorkspacePrecondition(string message) : Exception(message);
    private sealed record TerminalWorkspaceStageError(string Stage, string Classification, string Error);
    private sealed class TerminalWorkspaceCase(string directory, int width, int height, Dispatcher dispatcher)
    {
        internal string Directory => directory;
        internal readonly TerminalWorkspaceBackend Backend = new();
        internal MainWindow Window = null!;
        internal WorkspaceViewModel Vm = null!;
        internal MatrixRuntime Runtime = null!;
        internal readonly List<string> Checks = [], Failures = [];
        internal readonly List<object> Observations = [];
        internal readonly Dictionary<string, string> Stages = [];
        internal readonly List<TerminalWorkspaceStageError> StageErrors = [];
        internal string CleanupOwner = "not-started";
        internal int ShutdownRequests;
        internal bool Closed, Retained;
        private readonly ConcurrentQueue<(string Name, string Kind, Task Task)> tasks = new();
        private readonly List<(string Name, Func<Task> Dispose)> unadoptedOwners = [];
        private readonly ConcurrentDictionary<Guid, TaskCompletionSource> scriptTerminals = new();
        private readonly CancellationTokenSource deadline = new(TimeSpan.FromSeconds(90));
        private FrameworkElement page = null!;
        private TextBox input = null!;
        private Button execute = null!;
        private ListBox history = null!;
        private BindingBase? inputBinding;
        private DataTemplate? historyTemplate;
        private object[] objects = [];
        private DeviceRow[] rows = [];
        private bool cleanupStarted, userClose;
        private int step;
        private string longCommand = "", longOutput = "";
        private ShellEntry? longEntry;
        private DeviceRow A => Vm.Devices.Single(row => row.Serial == "DEMO-001");
        private DeviceRow B => Vm.Devices.Single(row => row.Serial == "DEMO-002");

        private static void Require(bool value, string message) { if (!value) throw new TerminalWorkspacePrecondition(message); }
        private bool Product(bool value, string expected, object? actual = null)
        {
            Checks.Add((value ? "PASS " : "FAIL ") + expected); Observe("assert", new { expected, actual, passed = value });
            if (!value) Failures.Add(expected); return value;
        }
        private void Observe(string operation, object? actual) => Observations.Add(new { step = ++step, operation, actual });
        private T Track<T>(string name, T task, string kind = "operation") where T : Task { tasks.Enqueue((name, kind, task)); return task; }
        private async Task Wait(Task task, string name = "operation", string kind = "operation")
        { if (!tasks.Any(item => ReferenceEquals(item.Task, task))) Track(name, task, kind); await task.WaitAsync(TimeSpan.FromSeconds(10), deadline.Token); }
        internal object[] TaskStates() => tasks.Select(item => (object)new { item.Name, item.Kind, status = item.Task.Status.ToString(), terminal = item.Task.IsCompleted }).ToArray();
        internal async Task Stage(string name, Func<Task> action)
        {
            var before = Failures.Count; Stages[name] = "STARTED";
            try { Require(!userClose, "用户未关闭自有窗口"); await action(); Stages[name] = Failures.Count == before ? "PASS" : "PRODUCT-FAIL"; }
            catch (Exception error) { var classification = error is TerminalWorkspacePrecondition ? "PRECONDITION-FAIL" : "FIXTURE-FAIL"; Stages[name] = classification; StageErrors.Add(new(name, classification, error.ToString())); }
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
            var settings = new AppSettings { WindowWidth = width, WindowHeight = height, Language = "zh-CN", AutoStartApi = false, AutoConnect = false,
                AgentBaseUrl = null, AgentSerial = null, SyncMouse = false, SyncKeyboard = false, AdbPath = "", LdConsolePath = "", AdbAllowedSerials = [], LdSerialBindings = [] };
            var catalog = new DeviceCatalog(Backend); var writes = new DeviceWriteCoordinator();
            var metadata = new CatalogRepository(Path.Combine(directory, "db.sqlite3")); unadoptedOwners.Add(("metadata", () => { metadata.Dispose(); return Task.CompletedTask; }));
            var log = new RollingLog(Path.Combine(directory, "logs")); unadoptedOwners.Add(("log", () => log.DisposeAsync().AsTask()));
            var scripts = new ScriptScheduler(Backend, catalog, true, writes); unadoptedOwners.Add(("scripts", () => scripts.DisposeAsync().AsTask()));
            var automation = new AutomationService(Backend, catalog);
            var coordinator = new InputCoordinator(catalog, Backend, writes); unadoptedOwners.Add(("input", () => coordinator.DisposeAsync().AsTask()));
            var api = new ApiServiceManager(catalog, Backend, automation, log, writes); unadoptedOwners.Add(("api", () => api.DisposeAsync().AsTask()));
            Runtime = new MatrixRuntime { Backend = Backend, Devices = catalog, Scripts = scripts, Automation = automation,
                Input = coordinator, Api = api, Writes = writes,
                Settings = settings, FrozenConnectionSettings = settings, SettingsStore = new SettingsStore(Path.Combine(directory, "AppConfig.json")),
                Metadata = metadata, Log = log, IsDemo = true };
            unadoptedOwners.Clear();
            Runtime.Api.ScriptWriteBusy = serial => scripts.GetStatus(serial).State is ScriptRunState.Running or ScriptRunState.Cancelling;
            Runtime.Api.AnyScriptWriteBusy = () => catalog.Snapshot.Any(row => scripts.GetStatus(row.Serial).State is ScriptRunState.Running or ScriptRunState.Cancelling);
            Runtime.Scripts.JobChanged += OnJob;
            Vm = new WorkspaceViewModel(Runtime, directory, true); await Wait(Vm.RefreshDevicesAsync(), "initial-devices");
            Require(Vm.Devices.Count == 2 && Vm.Devices.All(row => row.Info.Online), "透明 Demo 实际发现两台在线设备");
            Vm.CurrentDevice = A; B.IsSelected = true; Vm.UpdateSelection();
            Window = new MainWindow(Vm) { ShowActivated = false }; Window.Closing -= Window.WindowClosing; Window.Closing += OnClosing; Window.Closed += OnClosed;
            Application.Current.MainWindow = Window; Window.Show(); Window.NavigateTo("Shell"); await Settle();
            Observe("actual-window", new { width, height, actualWidth = F(Window.ActualWidth), actualHeight = F(Window.ActualHeight), Window.IsLoaded, Window.IsVisible, mode = "OWN-WPF-NOT-PHYSICAL-GUI" });
            Require(Window.IsLoaded && Window.IsVisible && PresentationSource.FromVisual(Window) is not null && Math.Abs(Window.ActualWidth - width) < .6 && Math.Abs(Window.ActualHeight - height) < .6, "实际显示外窗尺寸精确匹配");
            page = (FrameworkElement)((TabItem)Window.tabs.SelectedItem).Content;
            input = VisualDescendants<TextBox>(page).Single(box => AutomationProperties.GetAutomationId(box) == nameof(WorkspaceViewModel.ShellInput));
            execute = VisualDescendants<Button>(page).Single(button => ReferenceEquals(button.Command, Vm.ShellCommand));
            history = VisualDescendants<ListBox>(page).Single(list => ReferenceEquals(list.ItemsSource, Vm.ShellHistory));
            var textBoxes = VisualDescendants<TextBox>(page).ToArray();
            Observe("raw-control-inventory", new { buttons = VisualDescendants<Button>(page).Select(button => new { name = AutomationProperties.GetName(button), type = button.Command?.GetType().FullName }).ToArray(),
                lists = VisualDescendants<ListBox>(page).Count(), rawTextBoxes = textBoxes.Select(box => new { automationName = AutomationProperties.GetName(box), id = AutomationProperties.GetAutomationId(box), elementName = box.Name, box.IsReadOnly,
                    binding = BindingOperations.GetBinding(box, TextBox.TextProperty)?.Path.Path, comboTemplate = Parent<ComboBox>(box) is not null, historyTemplate = Parent<ListBoxItem>(box) is not null }).ToArray() });
            Require(VisualDescendants<Button>(page).Count(button => button.Command is AsyncCommand) == 1 && VisualDescendants<ListBox>(page).Count() == 1
                && ReferenceEquals(execute.Command, Vm.ShellCommand) && ReferenceEquals(history.ItemsSource, Vm.ShellHistory), "原输入/唯一执行/唯一原历史存在");
            inputBinding = BindingOperations.GetBindingBase(input, TextBox.TextProperty); historyTemplate = history.ItemTemplate;
            Require(inputBinding is Binding { Mode: BindingMode.TwoWay, UpdateSourceTrigger: UpdateSourceTrigger.PropertyChanged } binding && binding.Path.Path == nameof(WorkspaceViewModel.ShellInput), "原即时 TwoWay 绑定存在");
            rows = Vm.Devices.ToArray(); objects = Objects();
            Require(Runtime.IsDemo && Vm.Demo && !Runtime.Api.BasicRunning && !Runtime.Api.AutomatorRunning && Runtime.Api.DevicePorts.Count == 0 && Vm.ShellHistory.Count == 0
                && Runtime.Settings.AdbAllowedSerials.Length == 0 && !Runtime.Settings.AutoStartApi && !Runtime.Settings.AutoConnect, "隔离 Demo 无真实授权/接口/历史替身");
        }
        private void OnClosing(object? sender, CancelEventArgs args) { args.Cancel = true; userClose = true; }
        private void OnClosed(object? sender, EventArgs args) => Closed = true;
        private void OnJob(ScriptJobStatus status)
        {
            if (status.State is ScriptRunState.Completed or ScriptRunState.Cancelled or ScriptRunState.Failed)
                scriptTerminals.GetOrAdd(status.JobId, _ => new(TaskCreationOptions.RunContinuationsAsynchronously)).TrySetResult();
        }
        private object[] Objects() => [Vm, Runtime, Backend, Runtime.Devices, Runtime.Scripts, Runtime.Input, Runtime.Api, Runtime.Writes, Runtime.Metadata, Runtime.SettingsStore, page, input, execute, history, Vm.ShellHistory];
        internal void Identity()
        {
            Product(objects.SequenceEqual(Objects(), ReferenceEqualityComparer.Instance) && rows.SequenceEqual(Vm.Devices, ReferenceEqualityComparer.Instance), "原 Runtime/组件/终端控件/历史集合及设备行身份保持");
            Product(ReferenceEquals(inputBinding, BindingOperations.GetBindingBase(input, TextBox.TextProperty)) && ReferenceEquals(execute.Command, Vm.ShellCommand)
                && ReferenceEquals(history.ItemsSource, Vm.ShellHistory) && ReferenceEquals(historyTemplate, history.ItemTemplate), "原输入 BindingBase/执行命令/历史源及模板保持");
        }
        private async Task Edit(string text)
        {
            Require(input.IsVisible && input.IsEnabled && !input.IsReadOnly, "原编辑器仍可编辑");
            var binding = BindingOperations.GetBindingExpression(input, TextBox.TextProperty); Require(binding is not null, "原输入绑定尚在");
            input.SetCurrentValue(TextBox.TextProperty, text); binding!.UpdateSource(); await Settle();
            Product(Vm.ShellInput == text && input.Text == text, "实际原绑定提交完整输入", new { characters = text.Length, tail = text.Length > 32 ? text[^32..] : text });
        }
        private async Task<bool> Execute(string name, bool failure = false)
        {
            if (!Product(execute.IsVisible && execute.IsEnabled && Vm.ShellCommand.CanExecute(null), "原执行按钮可用：" + name)) return false;
            await Wait(Vm.ShellCommand.ExecuteAsync(), name); await Settle();
            Product(failure ? !string.IsNullOrWhiteSpace(Vm.Error) : string.IsNullOrWhiteSpace(Vm.Error), "原命令成功/失败反馈：" + name, new { Vm.Error, Vm.Status }); return true;
        }
        private async Task Select(DeviceRow row)
        {
            var choice = VisualDescendants<ComboBox>(Window.Shell).Single(box => box.Name == "SingleDeviceTarget");
            var binding = BindingOperations.GetBindingExpression(choice, Selector.SelectedItemProperty); Require(binding is not null, "原单设备目标选择绑定存在");
            choice.SetCurrentValue(Selector.SelectedItemProperty, row); binding!.UpdateSource(); await Settle(); Product(ReferenceEquals(Vm.CurrentDevice, row), "原目标选择实际更新单设备目标：" + row.Serial);
        }
        internal async Task Drafts(bool failure)
        {
            foreach (var mode in new[] { "unchanged", "ab", "aba" })
            {
                var command = failure ? "unsupported-terminal-owned-" + mode : "getprop ro.product.model";
                var initial = "  " + command + "  "; await Edit(initial);
                var gate = Backend.Hold("shell", A.Serial, command); var before = Vm.ShellHistory.Count;
                if (!Product(Vm.ShellCommand.CanExecute(null), "合法原命令接纳前可用")) return;
                var run = Track("draft-" + mode, Vm.ShellCommand.ExecuteAsync()); await Wait(gate.Entered.Task, "shell-entered", "observation"); await Settle();
                Require(!run.IsCompleted && gate.Claimed && !gate.Returned.Task.IsCompleted, "真实后端 Shell Task 已进入且等待");
                Product(Vm.Busy && !Vm.ShellCommand.CanExecute(null) && !execute.IsEnabled && input.IsEnabled, "实际 Shell 等待禁重复执行但保留编辑能力");
                Product(Runtime.Writes.IsBusy(A.Serial) && !Runtime.Writes.IsBusy(B.Serial), "原 Shell 持有共同 Writes 的原单目标租约");
                var calls = Backend.ShellCalls; await Wait(Vm.ShellCommand.ExecuteAsync(), "busy-duplicate-rejected"); Product(Backend.ShellCalls == calls, "Busy 原命令拒绝重复且后端零新调用");
                if (mode != "unchanged") await Edit("下一条草稿-B-" + mode);
                if (mode == "aba") await Edit(initial);
                var expected = mode == "unchanged" ? "" : mode == "aba" ? initial : "下一条草稿-B-ab";
                gate.Release(); await Wait(run); await Wait(gate.Returned.Task, "shell-returned", "observation"); await Settle();
                Product(!Vm.Busy && !Runtime.Writes.IsBusy(A.Serial), "实际任务完成后 Busy 和写租约释放");
                Product(Vm.ShellInput == expected && input.Text == expected, "成功/失败结束仅消费未编辑提交版本：" + mode, new { failure, expected, actual = Vm.ShellInput });
                Product(Vm.ShellHistory.Count == before + 1, "每次原执行只新增一条真实历史");
                if (Vm.ShellHistory.Count == before + 1) CheckEntry(Vm.ShellHistory[0], gate, command, A.Serial, failure);
            }
            if (!failure)
            {
                var submitted = "getprop ro.product.model"; var notificationDraft = "wm size";
                await Edit(submitted); var gate = Backend.Hold("shell", A.Serial, notificationDraft); var before = Vm.ShellHistory.Count; var notifications = 0; Task? run = null;
                PropertyChangedEventHandler? handler = null;
                handler = (_, args) =>
                {
                    if (args.PropertyName != nameof(WorkspaceViewModel.Busy) || !Vm.Busy) return;
                    Vm.PropertyChanged -= handler; notifications++;
                    var binding = BindingOperations.GetBindingExpression(input, TextBox.TextProperty); Require(binding is not null, "同步 Busy 通知时原输入绑定仍在");
                    input.SetCurrentValue(TextBox.TextProperty, notificationDraft); binding!.UpdateSource();
                    Observe("synchronous-busy-notification-edit", new { notifications, editorText = input.Text, modelText = Vm.ShellInput, Vm.Busy });
                };
                Vm.PropertyChanged += handler;
                try
                {
                    if (!Product(Vm.ShellCommand.CanExecute(null), "同步 Busy 场景原提交命令可执行")) return;
                    run = Track("synchronous-busy-original-command", Vm.ShellCommand.ExecuteAsync()); await Wait(gate.Entered.Task, "synchronous-busy-actual-B-entered", "observation");
                    Require(notifications == 1 && gate.Claimed && !run.IsCompleted && !gate.Returned.Task.IsCompleted, "同步 Busy 通知已通过原绑定写 B 且实际 B 后端 Task 等待");
                    Product(input.Text == notificationDraft && Vm.ShellInput == notificationDraft, "Busy 同步通知新草稿 B 立即可见");
                    gate.Release(); await Wait(run); await Wait(gate.Returned.Task, "synchronous-busy-B-returned", "observation"); await Settle();
                    Product(Vm.ShellInput == notificationDraft && input.Text == notificationDraft, "提交前版本不消费 Busy 同步通知中的新草稿 B");
                    Product(Vm.ShellHistory.Count == before + 1, "同步 Busy 提交仅新增一条实际历史");
                    if (Vm.ShellHistory.Count == before + 1) CheckEntry(Vm.ShellHistory[0], gate, notificationDraft, A.Serial, false);
                }
                finally { Vm.PropertyChanged -= handler; gate.Release(); if (run is not null) await Wait(run, "synchronous-busy-finally-drain"); }
            }
            if (!failure)
            {
                var command = "getprop ro.product.model"; var gate = Backend.Hold("shell", A.Serial, command); var before = Vm.ShellHistory.Count; var notifications = 0; Task? run = null;
                PropertyChangedEventHandler? handler = null;
                handler = (_, args) =>
                {
                    if (args.PropertyName != nameof(WorkspaceViewModel.ShellInput)) return;
                    Vm.PropertyChanged -= handler; notifications++;
                    Observe("synchronous-input-notification-submit", new { notifications, editorText = input.Text, modelText = Vm.ShellInput, eligible = Vm.ShellCommand.CanExecute(null) });
                    if (Product(Vm.ShellCommand.CanExecute(null), "ShellInput 同步通知可通过原命令提交当前 A"))
                        run = Track("synchronous-input-original-command", Vm.ShellCommand.ExecuteAsync());
                };
                Vm.PropertyChanged += handler;
                try
                {
                    await Edit(command); Require(notifications == 1 && run is not null, "原 ShellInput 通知同步提交一次且保留原任务");
                    await Wait(gate.Entered.Task, "synchronous-input-A-entered", "observation");
                    Require(gate.Claimed && !run!.IsCompleted && !gate.Returned.Task.IsCompleted, "原输入通知中的实际 A 后端 Task 已进入且等待");
                    Product(input.Text == command && Vm.ShellInput == command, "同步提交 A 后没有再次编辑");
                    gate.Release(); await Wait(run!); await Wait(gate.Returned.Task, "synchronous-input-A-returned", "observation"); await Settle();
                    Product(Vm.ShellInput.Length == 0 && input.Text.Length == 0, "setter 通知前登记版本保证无再次编辑仍清空 A");
                    Product(Vm.ShellHistory.Count == before + 1, "ShellInput 同步提交仅新增一条实际历史");
                    if (Vm.ShellHistory.Count == before + 1) CheckEntry(Vm.ShellHistory[0], gate, command, A.Serial, false);
                }
                finally { Vm.PropertyChanged -= handler; gate.Release(); if (run is not null) await Wait(run, "synchronous-input-finally-drain"); }
            }
            Identity();
        }
        private void CheckEntry(ShellEntry entry, TerminalWorkspaceBackend.Gate gate, string command, string serial, bool failure)
        {
            Product(entry.Command == command && entry.Serial == serial && entry.Context.EndsWith(" · " + serial, StringComparison.Ordinal) && entry.Time.Length > 0, "历史命令 Trim/原串号/真实时间归属正确", entry);
            Product(failure ? gate.Error is not null && entry.Output == "执行失败：" + UiErrorPresentation.Describe(gate.Error) && Vm.Status == "操作失败" && Vm.Error.Contains("2001", StringComparison.Ordinal)
                : gate.Error is null && entry.Output == gate.Result && Vm.Error.Length == 0, "历史输出来自实际 Demo 返回或原本地化错误", new { entry.Output, gate.Result, backendError = gate.Error?.ToString(), Vm.Status, workspaceError = Vm.Error });
        }
        internal async Task Targets()
        {
            Product(ReferenceEquals(Vm.CurrentDevice, A) && Vm.Targets().SequenceEqual(new[] { B }), "单设备 A 与勾选 B 职责分离前置");
            var command = "getprop ro.product.model"; await Edit(command); var gate = Backend.Hold("shell", A.Serial, command);
            var run = Track("target-A-run", Vm.ShellCommand.ExecuteAsync()); await Wait(gate.Entered.Task, "target-A-entered", "observation");
            Require(!run.IsCompleted, "实际 A Shell 处于等待"); await Select(B); A.IsSelected = true; B.IsSelected = false; Vm.UpdateSelection(); await Settle();
            gate.Release(); await Wait(run); await Settle();
            Product(Vm.ShellHistory.Count == 1 && Backend.Snapshot().Count(call => call.Kind == "shell") == 1, "切目标/勾选不重发已经提交的命令");
            if (Vm.ShellHistory.Count == 1) CheckEntry(Vm.ShellHistory[0], gate, command, A.Serial, false);
            Product(ReferenceEquals(Vm.CurrentDevice, B) && Vm.Targets().SequenceEqual(new[] { A }), "完成不会反向改写当前目标或勾选");
            await Edit(command); await Execute("new-B-command"); Product(Vm.ShellHistory.Count == 2 && Vm.ShellHistory[0].Serial == B.Serial && Vm.ShellHistory[1].Serial == A.Serial, "后续命令实际发送 B 而历史 A 保留");
            Product(Backend.Snapshot().Where(call => call.Kind == "shell").Select(call => call.Serial).SequenceEqual(new[] { A.Serial, B.Serial }), "实际后端调用顺序及原串号一致"); Identity();
        }
        private KeyEventArgs RouteKey(Key key)
        {
            var source = PresentationSource.FromVisual(input); Require(source is not null, "原输入有真实 WPF PresentationSource");
            var args = new KeyEventArgs(Keyboard.PrimaryDevice, source!, Environment.TickCount, key) { RoutedEvent = Keyboard.KeyDownEvent };
            Observe("owned-key-before-route", new { key = key.ToString(), actualKey = args.Key.ToString(), args.Handled, cursor = Window.shellHistoryCursor, editorText = input.Text, modelText = Vm.ShellInput });
            input.RaiseEvent(args); Observe("owned-key-handler", new { key = key.ToString(), actualKey = args.Key.ToString(), args.Handled, cursor = Window.shellHistoryCursor, editorText = input.Text, modelText = Vm.ShellInput, physicalInput = false }); return args;
        }
        internal async Task Keys()
        {
            var command = "getprop ro.product.model"; await Edit(command);
            if (!Product(Vm.ShellCommand.CanExecute(null) && execute.IsEnabled, "原 Enter 场景合法命令可执行")) return;
            var gate = Backend.Hold("shell", A.Serial, command);
            var completed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            PropertyChangedEventHandler onChanged = (_, args) => { if (args.PropertyName == nameof(WorkspaceViewModel.Busy) && !Vm.Busy && gate.Returned.Task.IsCompleted) completed.TrySetResult(); };
            Vm.PropertyChanged += onChanged;
            try
            {
                if (!Product(RouteKey(Key.Enter).Handled, "原 Enter 处理器消费按键")) return;
                await Wait(gate.Entered.Task, "enter-backend-entered", "observation");
                Require(gate.Claimed && !gate.Returned.Task.IsCompleted && Vm.Busy, "Enter async-void 已进入真实后台而非仅事件返回");
                Product(Vm.ShellHistory.Count == 0, "后台未返回时没有伪完成历史"); gate.Release();
                await Wait(gate.Returned.Task, "enter-original-backend-terminal", "observation"); await Wait(completed.Task, "enter-VM-finally", "observation"); await Settle();
            }
            finally { Vm.PropertyChanged -= onChanged; gate.Release(); if (!completed.Task.IsCompleted) completed.TrySetCanceled(); }
            Product(!Vm.Busy && Vm.ShellHistory.Count == 1 && input.Text.Length == 0, "Enter 实际完成历史/清空/Busy 恢复");
            await Edit("wm size"); await Execute("second-history-command"); var entries = Vm.ShellHistory.ToArray(); Require(entries.Length == 2, "两条真实历史建立");
            foreach (var expected in new[] { (Key.Up, entries[0].Command), (Key.Up, entries[1].Command), (Key.Down, entries[0].Command), (Key.Down, "") })
            { Product(RouteKey(expected.Item1).Handled, "原上下键处理器消费"); await Settle(); Product(input.Text == expected.Item2 && Vm.ShellInput == expected.Item2, "原上下历史顺序/边界/回到空草稿", new { key = expected.Item1.ToString(), expected = expected.Item2, actual = input.Text }); }
            history.SetCurrentValue(Selector.SelectedItemProperty, entries[1]);
            history.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left) { RoutedEvent = Control.MouseDoubleClickEvent }); await Settle();
            Product(input.Text == entries[1].Command && Vm.ShellInput == entries[1].Command, "原历史双击处理器回填所选原命令");
            Product(entries.SequenceEqual(Vm.ShellHistory, ReferenceEqualityComparer.Instance) && Backend.ShellCalls == 2, "历史浏览与双击不重新发命令/不重建历史"); Identity();
        }
        private async Task Rejected(string name)
        {
            await Settle(); var calls = Backend.ShellCalls; Product(!Vm.ShellCommand.CanExecute(null) && !execute.IsEnabled, "原命令门禁禁用：" + name);
            await Wait(Vm.ShellCommand.ExecuteAsync(), "reject-" + name); Product(Backend.ShellCalls == calls, "原门禁实际零 Shell 调用：" + name);
        }
        internal async Task Gates()
        {
            await Edit(""); await Rejected("empty"); await Edit(" \t "); await Rejected("whitespace");
            await Edit("getprop ro.product.model"); Backend.SetOnline(A.Serial, false); await Wait(Vm.RefreshDevicesAsync(), "actual-demo-offline"); await Select(A); await Rejected("offline");
            Backend.SetOnline(A.Serial, true); await Wait(Vm.RefreshDevicesAsync(), "actual-demo-online"); await Select(A); A.IsSelected = true; B.IsSelected = false; Vm.UpdateSelection();
            var busyGate = Backend.Hold("shell", A.Serial, "getprop ro.product.model"); var busy = Track("actual-shell-busy", Vm.ShellCommand.ExecuteAsync());
            await Wait(busyGate.Entered.Task, "actual-busy-entered", "observation"); Require(!busy.IsCompleted, "真实 Shell 在途建立 Busy"); await Rejected("actual-Shell-Busy"); busyGate.Release(); await Wait(busy); await Settle();
            var appGate = Backend.Hold("app", A.Serial, "com.android.settings");
            var package = new ScriptPackage { Manifest = new() { Id = "terminal-owned-script", Name = "终端门禁实任务", Version = "1.0" }, Steps = [new() { Action = "launchapp", Value = "com.android.settings" }] };
            var scriptPath = Path.Combine(directory, "gate.jxscript"); ScriptPackageService.Save(scriptPath, package); await Wait(Vm.ImportScriptAsync(scriptPath), "original-import-script");
            Require(Vm.StartScriptCommand.CanExecute(null), "合法原脚本命令可接纳"); await Wait(Vm.StartScriptCommand.ExecuteAsync(), "original-start-script"); await Wait(appGate.Entered.Task, "actual-script-entered", "observation"); await Settle();
            Require(Runtime.Scripts.GetStatus(A.Serial).State == ScriptRunState.Running && Runtime.Writes.IsBusy(A.Serial), "真实原调度任务及同 Writes 租约成立");
            await Edit("wm size"); await Select(A); await Rejected("script-target-A"); await Select(B);
            Product(Vm.ShellCommand.CanExecute(null) && execute.IsEnabled && !Runtime.Writes.IsBusy(B.Serial), "A 脚本不错误阻塞当前 B 终端"); await Execute("B-while-A-script");
            Product(Vm.ShellHistory[0].Serial == B.Serial && Runtime.Scripts.GetStatus(A.Serial).State == ScriptRunState.Running, "B 实际执行不停止 A 原脚本");
            var job = Runtime.Scripts.GetJobs().Single(); var terminal = scriptTerminals.GetOrAdd(job.JobId, _ => new(TaskCreationOptions.RunContinuationsAsynchronously));
            appGate.Release(); await Wait(terminal.Task, "actual-script-terminal", "observation"); await Wait(appGate.Returned.Task, "actual-script-backend-returned", "observation"); await Settle();
            var leaseObservation = System.Diagnostics.Stopwatch.StartNew(); var leasePolls = 0;
            while (Runtime.Writes.IsBusy(A.Serial) && leaseObservation.Elapsed < TimeSpan.FromSeconds(10))
                await Wait(Task.Delay(10, deadline.Token), "actual-script-finally-lease-poll-" + ++leasePolls, "observation");
            Observe("actual-script-finally-lease", new { elapsedMilliseconds = leaseObservation.Elapsed.TotalMilliseconds, leasePolls, busy = Runtime.Writes.IsBusy(A.Serial) });
            Product(Runtime.Scripts.GetJobs().Single().State == ScriptRunState.Completed && !Runtime.Writes.IsBusy(A.Serial), "原脚本实际完成且释放租约");
            var file = Path.Combine(directory, "owned-transfer.txt"); File.WriteAllText(file, "TERMINAL-TRANSFER-GATE-" + Guid.NewGuid().ToString("N"));
            await Select(A); Vm.QueuePush(file, false); var transfer = Vm.Transfers.Single(); var pushGate = Backend.Hold("push", A.Serial, transfer.RemotePath);
            Require(Vm.StartTransfersCommand.CanExecute(null), "原文件队列可启动"); var transferring = Track("original-start-transfer", Vm.StartTransfersCommand.ExecuteAsync());
            await Wait(pushGate.Entered.Task, "actual-transfer-entered", "observation"); Require(Vm.Transferring && !transferring.IsCompleted && Runtime.Writes.IsBusy(A.Serial), "真实原传输正在占用");
            await Edit("wm size"); await Select(B); await Rejected("actual-transfer-global-gate"); pushGate.Release(); await Wait(transferring); await Settle();
            Product(!Vm.Transferring && transfer.State == "传输完成" && Vm.ShellCommand.CanExecute(null), "真实传输完成后终端原命令恢复");
            var pulled = Path.Combine(directory, "owned-transfer-return.txt"); await Wait(Backend.PullAsync(A.Serial, transfer.RemotePath, pulled), "verify-transparent-demo-transfer");
            Product(File.ReadAllBytes(file).SequenceEqual(File.ReadAllBytes(pulled)), "传输屏障真实转发 Demo 文件字节而非假传输"); Identity();
        }
        internal async Task Presentation()
        {
            var key = "terminal.presentation." + new string('K', 260) + ".KEY-TAIL";
            longCommand = "getprop " + key;
            longOutput = "OUTPUT-FIRST\n" + string.Join("\n", Enumerable.Range(0, 100).Select(index => $"实际Demo属性行{index:D3}-" + new string('O', 90))) + "\nOUTPUT-LAST-TAIL";
            await Wait(Backend.ShellAsync(A.Serial, "setprop " + key + " " + longOutput), "seed-real-demo-property");
            await Edit(longCommand); await Execute("actual-long-command-output"); longEntry = Vm.ShellHistory.Single();
            Require(longEntry.Command == longCommand && longEntry.Output == longOutput && longEntry.Output.Length > 9000 && longEntry.Output.Count(character => character == '\n') >= 100, "长命令/多行输出压力真实由原 VM 和 Demo 后端产生");
            for (var index = 0; index < 22; index++) { await Edit(index % 2 == 0 ? "wm size" : "getprop ro.product.model"); await Execute("actual-history-" + index); }
            Require(Vm.ShellHistory.Count == 23 && Vm.ShellHistory[^1] == longEntry && Vm.ShellHistory.Select(entry => entry.Index).SequenceEqual(Enumerable.Range(1, 23).Reverse()), "23 条真实完成历史与原编号顺序");
            await Edit("DRAFT-FIRST-" + new string('D', 400) + "-DRAFT-LAST-TAIL"); await Settle();
            Observe("actual-pressure", new { rows = Vm.ShellHistory.Count, longCommandCharacters = longEntry.Command.Length, outputCharacters = longEntry.Output.Length, lines = longEntry.Output.Count(character => character == '\n') + 1, context = longEntry.Context,
                contextPressure = "Original Time and fixed Demo serial retained; no fabricated serial or synthetic ShellEntry", backendShellCalls = Backend.ShellCalls });
        }
        internal async Task Geometry()
        {
            foreach (var control in new FrameworkElement[] { input, execute }) { control.BringIntoView(); await Settle(); Hit44(control); }
            history.BringIntoView(); await Settle(); var viewport = Reading(history);
            Product(!viewport.IsEmpty && viewport.Width >= 99.9 && viewport.Height >= 95.9, "历史真实正文阅读视口至少100x96", R(viewport));
            Product(page.ActualWidth <= Window.Shell.ActualWidth + .6 && Bounds(page).Right <= Window.Shell.ActualWidth + .6, "终端页本体不水平溢出 Shell", R(Bounds(page)));
            await ReadBox(input, "长命令草稿"); Identity();
        }
        private async Task<ListBoxItem?> Realize(ShellEntry entry)
        {
            history.BringIntoView(); history.ScrollIntoView(entry); await Settle();
            var container = history.ItemContainerGenerator.ContainerFromItem(entry) as ListBoxItem;
            if (!Product(container is not null && ReferenceEquals(container.DataContext, entry), "原历史行真实实现：" + entry.Index)) return null;
            return container;
        }
        internal async Task ReadHistory()
        {
            Require(longEntry is not null && Vm.ShellHistory.Count == 23, "已建立真实多行长历史压力");
            var before = Vm.ShellHistory.ToArray(); var calls = Backend.ShellCalls;
            foreach (var entry in new[] { before[0], before[before.Length / 2], before[^1] })
            {
                var container = await Realize(entry); if (container is null) continue;
                var command = VisualDescendants<TextBlock>(container).SingleOrDefault(text => BindingOperations.GetBinding(text, TextBlock.TextProperty)?.Path.Path == nameof(ShellEntry.Command));
                var context = VisualDescendants<TextBlock>(container).SingleOrDefault(text => BindingOperations.GetBinding(text, TextBlock.TextProperty)?.Path.Path == nameof(ShellEntry.Context));
                var output = VisualDescendants<TextBox>(container).SingleOrDefault(box => BindingOperations.GetBinding(box, TextBox.TextProperty)?.Path.Path == nameof(ShellEntry.Output));
                if (!Product(command is not null && context is not null && output is not null, "原历史命令/来源 TextBlock 与输出 TextBox 绑定完整")) continue;
                Product(command!.Text == entry.Command && context!.Text == entry.Context && output!.Text == entry.Output && output.IsReadOnly && output.AcceptsReturn, "原历史全文值及只读可复制 TextBox 保留");
                await ReadLabel(command!, entry.Command, "历史命令"); await ReadLabel(context!, entry.Context, "来源时间与串号");
                output!.BringIntoView(); await Settle(); var viewport = Reading(output);
                Product(!viewport.IsEmpty && viewport.Width >= 99.9 && viewport.Height > 0, "原短输出真实阅读宽度及非零正文高度", R(viewport));
                if (ReferenceEquals(entry, longEntry))
                {
                    Product(!viewport.IsEmpty && viewport.Width >= 99.9 && viewport.Height >= 47.9, "长输出实际阅读视口至少100x48", R(viewport));
                    await ReadBox(output, "真实多行输出首尾");
                }
            }
            Product(before.SequenceEqual(Vm.ShellHistory, ReferenceEqualityComparer.Instance) && calls == Backend.ShellCalls, "滚读原历史不重建记录且零新 Shell 调用"); Identity();
        }
        private async Task ReadLabel(TextBlock text, string expected, string name)
        {
            text.BringIntoView(); await Settle(); var visible = Bounds(text); visible.Intersect(Clip(text));
            var measured = new FormattedText(expected, CultureInfo.CurrentCulture, text.FlowDirection, new Typeface(text.FontFamily, text.FontStyle, text.FontWeight, text.FontStretch), text.FontSize, Brushes.Black, VisualTreeHelper.GetDpi(text).PixelsPerDip);
            if (text.TextWrapping != TextWrapping.NoWrap) measured.MaxTextWidth = Math.Max(1, text.ActualWidth);
            var full = !visible.IsEmpty && measured.WidthIncludingTrailingWhitespace <= visible.Width + .75 && measured.Height <= visible.Height + .75;
            if (!full && text.ToolTip is not null) full = await ReadTip(text, expected);
            Product(text.Text == expected && full, name + "完整可见或通过原控件全文提示读全", new { expected, visible = R(visible), measuredWidth = F(measured.WidthIncludingTrailingWhitespace), measuredHeight = F(measured.Height), text.TextWrapping });
        }
        private async Task<bool> ReadTip(FrameworkElement owner, string expected)
        {
            var original = owner.ToolTip; var binding = BindingOperations.GetBindingBase(owner, ToolTipProperty);
            var tip = original as TerminalProbeToolTip ?? new TerminalProbeToolTip { Content = original, PlacementTarget = owner, Placement = PlacementMode.Bottom }; var open = tip.IsOpen;
            try
            {
                if (original is not TerminalProbeToolTip) owner.SetCurrentValue(ToolTipProperty, tip); tip.SetCurrentValue(TerminalProbeToolTip.IsOpenProperty, true); await Settle();
                var text = VisualDescendants<TextBlock>(tip).FirstOrDefault(item => item.Text == expected);
                var valid = tip.IsOpen && tip.IsVisible && tip.ActualWidth > 0 && tip.ActualWidth <= 361 && text is not null && text.IsVisible && text.TextWrapping == TextWrapping.Wrap && text.ActualHeight + .5 >= text.DesiredSize.Height;
                Product(ReferenceEquals(binding, BindingOperations.GetBindingBase(owner, ToolTipProperty)), "阅读原提示保留其 BindingBase"); Observe("actual-tooltip", new { expected, valid, width = F(tip.ActualWidth), height = F(tip.ActualHeight) }); return valid;
            }
            finally { tip.SetCurrentValue(TerminalProbeToolTip.IsOpenProperty, open); if (original is not TerminalProbeToolTip) owner.SetCurrentValue(ToolTipProperty, original); }
        }
        private async Task ReadBox(TextBox box, string name)
        {
            box.BringIntoView(); await Settle(); var expected = box.Text; Require(expected.Length > 100, "首尾阅读确有长文本压力：" + name);
            box.ScrollToHome(); box.ScrollToHorizontalOffset(0); await Settle(); Character(box, 0, name + "首字符");
            box.ScrollToEnd(); box.ScrollToHorizontalOffset(double.MaxValue); await Settle(); Character(box, expected.Length - 1, name + "末尾 TAIL");
            Product(box.Text == expected, "滚读不修改原完整文本：" + name);
        }
        private void Character(TextBox box, int index, string name)
        {
            var character = box.GetRectFromCharacterIndex(index); var visible = character.IsEmpty ? Rect.Empty : box.TransformToAncestor(Window.Shell).TransformBounds(character);
            if (!visible.IsEmpty) visible.Intersect(Reading(box)); Product(!visible.IsEmpty && visible.Height > 0 && visible.Width >= 0, "实际字符进入祖先裁剪后阅读视口：" + name, R(visible));
        }
        internal async Task Resize()
        {
            await Edit("getprop ro.product.model"); await Execute("identity-seed"); await Edit("保留终端草稿-" + new string('N', 100) + "-TAIL");
            var draft = input.Text; var entries = Vm.ShellHistory.ToArray(); var selected = Vm.CurrentDevice; var checks = Vm.Devices.Select(row => row.IsSelected).ToArray(); var calls = Backend.ShellCalls;
            foreach (var size in new[] { (390, 640), (390, 844), (768, 1024), (1000, 700), (1440, 900), (1920, 1080) })
            {
                Window.Width = size.Item1; Window.Height = size.Item2; await Settle();
                Observe("resize-actual-outer-window", new { requestedWidth = size.Item1, requestedHeight = size.Item2, actualWidth = F(Window.ActualWidth), actualHeight = F(Window.ActualHeight), Window.IsLoaded, Window.IsVisible, windowState = Window.WindowState.ToString() });
                Product(Window.IsLoaded && Window.IsVisible && Math.Abs(Window.ActualWidth - size.Item1) < .6 && Math.Abs(Window.ActualHeight - size.Item2) < .6, "缩放逐格实际外窗尺寸精确匹配");
                Window.NavigateTo("文本"); await Settle(); Window.NavigateTo("Shell"); await Settle(); Identity();
                Product(input.Text == draft && Vm.ShellInput == draft && entries.SequenceEqual(Vm.ShellHistory, ReferenceEqualityComparer.Instance), "缩放/离页返回保留草稿和原历史对象");
                Product(ReferenceEquals(selected, Vm.CurrentDevice) && checks.SequenceEqual(Vm.Devices.Select(row => row.IsSelected)) && calls == Backend.ShellCalls, "缩放/导航不改目标勾选、不发新命令");
            }
        }
        private Rect Bounds(FrameworkElement element) => element.TransformToAncestor(Window.Shell).TransformBounds(new Rect(element.RenderSize));
        private Rect Clip(FrameworkElement element)
        {
            var clip = new Rect(Window.Shell.RenderSize);
            for (DependencyObject? current = element; current is Visual && !ReferenceEquals(current, Window.Shell); current = VisualTreeHelper.GetParent(current))
                if (current is FrameworkElement ancestor && (ancestor.ClipToBounds || ancestor is ScrollContentPresenter)) clip.Intersect(Bounds(ancestor));
            return clip;
        }
        private Rect Reading(FrameworkElement element)
        {
            var presenter = VisualDescendants<ScrollContentPresenter>(element).FirstOrDefault(); var actual = Bounds(presenter ?? element); actual.Intersect(Clip(presenter ?? element)); return actual;
        }
        private void Hit44(FrameworkElement control)
        {
            var visible = Bounds(control); visible.Intersect(Clip(control)); Product(control.IsVisible && !visible.IsEmpty && visible.Width >= 43.9 && visible.Height >= 43.9, "祖先裁剪后原44命中区：" + AutomationProperties.GetName(control), R(visible));
            if (control.IsEnabled && !visible.IsEmpty && visible.Width > 0 && visible.Height > 0)
            {
                var hit = Window.Shell.InputHitTest(new Point(visible.X + visible.Width / 2, visible.Y + visible.Height / 2)) as DependencyObject;
                Product(hit is not null && (ReferenceEquals(hit, control) || control.IsAncestorOf(hit)), "enabled 中心命中归原控件");
            }
            else Product(!control.IsEnabled && control is Button button && button.Command is not null && !button.Command.CanExecute(null), "disabled 仅核原命令，不伪造 enabled 命中");
        }
        private static T? Parent<T>(DependencyObject element) where T : DependencyObject
        { for (DependencyObject? current = element; current is Visual; current = VisualTreeHelper.GetParent(current)) if (current is T match) return match; return null; }
        private static double? F(double value) => double.IsFinite(value) ? value : null;
        private static object R(Rect rectangle) => new { empty = rectangle.IsEmpty, x = F(rectangle.X), y = F(rectangle.Y), width = F(rectangle.Width), height = F(rectangle.Height) };
        internal void Render()
        {
            var path = Path.Combine(directory, "owned-terminal-shell.png"); MainWindow.SaveRender(path, Window.Shell);
            using var stream = File.OpenRead(path); Observe("owned-shell-png", new { path, bytes = stream.Length, sha256 = Convert.ToHexString(SHA256.HashData(stream)), desktopCapture = false });
        }
        internal async Task Cleanup()
        {
            Require(!cleanupStarted, "唯一清理 owner"); cleanupStarted = true; Backend.ReleaseAll(); deadline.Cancel();
            var originalTasks = tasks.Select(item => item.Task).Concat(Backend.Tasks()).ToArray();
            var drain = Track("original-task-drain", Task.WhenAll(originalTasks), "cleanup");
            try { await drain.WaitAsync(TimeSpan.FromSeconds(15)); } catch { if (!drain.IsCompleted) { Retain(); throw new InvalidOperationException("原 Task 未排空，不释放其上游 owner"); } }
            if (Window is not null) { Window.Closing -= OnClosing; LocalizationService.Current.LanguageChanged -= Window.OnLanguageChanged; Window.AbortStartup(); }
            CleanupOwner = Vm is not null ? "VM-Shutdown" : Runtime is not null ? "Runtime-Dispose" : "owned-backend-only";
            Task shutdown;
            if (Vm is not null) { ShutdownRequests++; shutdown = Track("VM-shutdown", Vm.ShutdownAsync(), "cleanup"); }
            else if (Runtime is not null) { ShutdownRequests++; shutdown = Track("runtime-dispose", Runtime.DisposeAsync().AsTask(), "cleanup"); }
            else
            {
                foreach (var owner in unadoptedOwners.AsEnumerable().Reverse())
                {
                    var dispose = Track("unadopted-" + owner.Name, owner.Dispose(), "cleanup");
                    try { await dispose.WaitAsync(TimeSpan.FromSeconds(15)); } catch { Retain(); throw; }
                }
                unadoptedOwners.Clear(); shutdown = Track("backend-dispose", Backend.DisposeAsync().AsTask(), "cleanup");
            }
            try { await shutdown.WaitAsync(TimeSpan.FromSeconds(15)); } catch { Retain(); throw; }
            if (Runtime is not null) Runtime.Scripts.JobChanged -= OnJob;
            if (Window is not null) Window.Closed -= OnClosed;
            Require(tasks.All(item => item.Task.IsCompleted) && Backend.Tasks().All(task => task.IsCompleted) && Backend.Disposed && Backend.AllGatesTerminal, "原任务/后端/屏障全部终态并实际释放");
            Require(!userClose && Application.Current.Windows.Count == 0 && (Window is null || Closed), "无用户干扰、无残窗且唯一 Close 已完成");
            if (Runtime is not null) Require(!Runtime.Api.BasicRunning && !Runtime.Api.AutomatorRunning && Runtime.Api.DevicePorts.Count == 0 && !Runtime.Api.HasActiveRequests
                && Runtime.Scripts.GetJobs().All(job => job.State is ScriptRunState.Completed or ScriptRunState.Cancelled or ScriptRunState.Failed), "原 Runtime 无活动接口/脚本残留");
            Application.Current.MainWindow = null; deadline.Dispose(); Observe("cleanup", new { CleanupOwner, ShutdownRequests, Closed, Retained, pending = tasks.Count(item => !item.Task.IsCompleted), backendPending = Backend.Tasks().Count(task => !task.IsCompleted), windows = Application.Current.Windows.Count });
            void Retain() { Retained = true; retainedTerminalWorkspaces.Add(this); }
        }
    }

    private sealed class TerminalWorkspaceBackend : IDeviceBackend, IIdentityPinnedDeviceBackend
    {
        internal sealed record Call(string Kind, string Serial, string Value, bool Root);
        internal sealed class Gate(string kind, string serial, string value)
        {
            internal readonly string Kind = kind, Serial = serial, Value = value;
            internal readonly TaskCompletionSource Entered = new(TaskCreationOptions.RunContinuationsAsynchronously);
            internal readonly TaskCompletionSource Released = new(TaskCreationOptions.RunContinuationsAsynchronously);
            internal readonly TaskCompletionSource Returned = new(TaskCreationOptions.RunContinuationsAsynchronously);
            internal bool Claimed;
            internal string? Result;
            internal Exception? Error;
            internal void Release() => Released.TrySetResult();
        }
        private readonly DemoDeviceBackend inner = new();
        private readonly object sync = new();
        private readonly List<Gate> gates = [];
        private readonly ConcurrentQueue<Call> calls = new();
        private readonly ConcurrentQueue<Task> tasks = new();
        internal bool Disposed;
        internal int ShellCalls => calls.Count(call => call.Kind == "shell");
        internal Call[] Snapshot() => calls.ToArray();
        internal Task[] Tasks() => tasks.ToArray();
        internal bool AllGatesTerminal { get { lock (sync) return gates.All(gate => gate.Entered.Task.IsCompleted && gate.Released.Task.IsCompleted && gate.Returned.Task.IsCompleted); } }
        internal Gate Hold(string kind, string serial, string value) { var gate = new Gate(kind, serial, value); lock (sync) gates.Add(gate); return gate; }
        internal void SetOnline(string serial, bool online) => inner.SetOnline(serial, online);
        internal object Report() => new { Disposed, calls = Snapshot(), tasks = Tasks().Select(task => new { status = task.Status.ToString(), terminal = task.IsCompleted }).ToArray(),
            gates = GateReport(), transparent = "No fake responses, histories or Busy. All actual operations forward to the same DemoDeviceBackend; held gates only delay forwarding." };
        private object[] GateReport() { lock (sync) return gates.Select(gate => (object)new { gate.Kind, gate.Serial, gate.Value, gate.Claimed, entered = gate.Entered.Task.Status.ToString(), released = gate.Released.Task.Status.ToString(), returned = gate.Returned.Task.Status.ToString(), gate.Result, error = gate.Error?.ToString() }).ToArray(); }
        internal void ReleaseAll()
        {
            lock (sync) foreach (var gate in gates) { gate.Release(); if (!gate.Claimed) { gate.Entered.TrySetCanceled(); gate.Returned.TrySetCanceled(); } }
        }
        private Task<T> Forward<T>(string kind, string serial, string value, bool root, Func<Task<T>> action, CancellationToken ct)
        {
            var task = Execute(); tasks.Enqueue(task); return task;
            async Task<T> Execute()
            {
                calls.Enqueue(new(kind, serial, value, root)); Gate? held;
                lock (sync) { held = gates.FirstOrDefault(gate => !gate.Claimed && gate.Kind == kind && gate.Serial == serial && gate.Value == value); if (held is not null) held.Claimed = true; }
                try
                {
                    if (held is not null) { held.Entered.TrySetResult(); await held.Released.Task.WaitAsync(ct); }
                    var original = action(); tasks.Enqueue(original); var result = await original;
                    if (held is not null) held.Result = result is string text ? text : null; return result;
                }
                catch (Exception error) { if (held is not null) held.Error = error; throw; }
                finally { held?.Returned.TrySetResult(); }
            }
        }
        private Task Forward(string kind, string serial, string value, Func<Task> action, CancellationToken ct = default)
            => Forward(kind, serial, value, false, async () => { var original = action(); tasks.Enqueue(original); await original; return true; }, ct);
        public IIdentityPinnedDeviceCommands BindIdentity(DeviceCatalog catalog) => new Pinned(this, inner.BindIdentity(catalog));
        private sealed class Pinned(TerminalWorkspaceBackend owner, IIdentityPinnedDeviceCommands commands) : IIdentityPinnedDeviceCommands
        {
            public bool IsDemo => commands.IsDemo;
            public Task<string> StartAppAsync(DeviceCommandTarget target, string packageName, CancellationToken ct = default) => owner.Forward("app", target.Serial, packageName, false, () => commands.StartAppAsync(target, packageName, ct), ct);
            public Task TapAsync(DeviceCommandTarget target, int x, int y, CancellationToken ct = default) => owner.Forward("tap", target.Serial, $"{x},{y}", () => commands.TapAsync(target, x, y, ct), ct);
            public Task SwipeAsync(DeviceCommandTarget target, int x1, int y1, int x2, int y2, int durationMs, CancellationToken ct = default) => owner.Forward("swipe", target.Serial, $"{x1},{y1},{x2},{y2}", () => commands.SwipeAsync(target, x1, y1, x2, y2, durationMs, ct), ct);
        }
        public Task<IReadOnlyList<DeviceInfo>> DiscoverAsync(CancellationToken ct = default) => Forward("discover", "", "", false, () => inner.DiscoverAsync(ct), ct);
        public Task<string> ShellAsync(string serial, string command, bool useRoot = false, CancellationToken ct = default) => Forward("shell", serial, command, useRoot, () => inner.ShellAsync(serial, command, useRoot, ct), ct);
        public Task<byte[]> ScreenshotAsync(string serial, CancellationToken ct = default) => Forward("screenshot", serial, "", false, () => inner.ScreenshotAsync(serial, ct), ct);
        public Task<AndroidNode> DumpAsync(string serial, CancellationToken ct = default) => Forward("dump", serial, "", false, () => inner.DumpAsync(serial, ct), ct);
        public Task TapAsync(string serial, int x, int y, CancellationToken ct = default) => Forward("tap", serial, $"{x},{y}", () => inner.TapAsync(serial, x, y, ct), ct);
        public Task SwipeAsync(string serial, int x1, int y1, int x2, int y2, int durationMs, CancellationToken ct = default) => Forward("swipe", serial, $"{x1},{y1},{x2},{y2}", () => inner.SwipeAsync(serial, x1, y1, x2, y2, durationMs, ct), ct);
        public Task KeyEventAsync(string serial, int keyCode, CancellationToken ct = default) => Forward("key", serial, keyCode.ToString(CultureInfo.InvariantCulture), () => inner.KeyEventAsync(serial, keyCode, ct), ct);
        public Task TextAsync(string serial, string text, CancellationToken ct = default) => Forward("text", serial, text, () => inner.TextAsync(serial, text, ct), ct);
        public Task<string> StartAppAsync(string serial, string packageName, string? activity = null, CancellationToken ct = default) => Forward("app", serial, packageName, false, () => inner.StartAppAsync(serial, packageName, activity, ct), ct);
        public Task<string> StopAppAsync(string serial, string packageName, CancellationToken ct = default) => Forward("stop", serial, packageName, false, () => inner.StopAppAsync(serial, packageName, ct), ct);
        public Task<bool> ClearAppAsync(string serial, string packageName, CancellationToken ct = default) => Forward("clear", serial, packageName, false, () => inner.ClearAppAsync(serial, packageName, ct), ct);
        public Task<IReadOnlyList<AppInfo>> ListAppsAsync(string serial, bool showSystem = false, bool needVersion = true, CancellationToken ct = default) => Forward("apps", serial, "", false, () => inner.ListAppsAsync(serial, showSystem, needVersion, ct), ct);
        public Task InstallAsync(string serial, string localPath, CancellationToken ct = default) => Forward("install", serial, localPath, () => inner.InstallAsync(serial, localPath, ct), ct);
        public Task UninstallAsync(string serial, string packageName, CancellationToken ct = default) => Forward("uninstall", serial, packageName, () => inner.UninstallAsync(serial, packageName, ct), ct);
        public Task PushAsync(string serial, string localPath, string remotePath, IProgress<double>? progress = null, CancellationToken ct = default) => Forward("push", serial, remotePath, () => inner.PushAsync(serial, localPath, remotePath, progress, ct), ct);
        public Task PullAsync(string serial, string remotePath, string localPath, IProgress<double>? progress = null, CancellationToken ct = default) => Forward("pull", serial, remotePath, () => inner.PullAsync(serial, remotePath, localPath, progress, ct), ct);
        public Task SetNodeTextAsync(string serial, NodeSelector selector, string text, CancellationToken ct = default) => Forward("node-text", serial, text, () => inner.SetNodeTextAsync(serial, selector, text, ct), ct);
        public async ValueTask DisposeAsync() { ReleaseAll(); await Task.WhenAll(Tasks()).ContinueWith(_ => { }, TaskScheduler.Default); await inner.DisposeAsync(); Disposed = true; }
    }
}
