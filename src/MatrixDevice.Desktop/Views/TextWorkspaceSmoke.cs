using System.Collections.Concurrent;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;
using MatrixDevice.Core;
using MatrixDevice.Features;
using MatrixDevice.Host;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    private static readonly List<object> retainedTextWorkspaces = [];
    private static readonly (int W, int H)[] textWorkspaceSizes = [(390, 640), (390, 844), (768, 1024), (1000, 700), (1440, 900), (1920, 1080)];

    internal static async Task RunTextWorkspaceVerificationAsync(string directory, Dispatcher dispatcher)
    {
        if (!dispatcher.CheckAccess() || Thread.CurrentThread.GetApartmentState() != ApartmentState.STA || Application.Current.Windows.Count != 0 || Application.Current.MainWindow is not null
            || !Path.IsPathFullyQualified(directory) || !directory.Contains(Path.DirectorySeparatorChar + "artifacts" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) || Directory.Exists(directory) || File.Exists(directory))
            throw new InvalidOperationException("PRECONDITION-FAIL: 文本验收要求 fresh artifacts 绝对目录及无用户窗口的自有 STA。");
        Directory.CreateDirectory(directory);
        var reports = new List<object>(); var failures = new List<Exception>(); var stop = false;
        foreach (var size in textWorkspaceSizes) await Case($"geometry-{size.W}-{size.H}", size.W, size.H, state => state.Geometry());
        await Case("real-files-encoding-empty-error-recovery", 1000, 700, state => state.Files());
        foreach (var mode in new[] { "edit-B", "edit-ABA", "same-value" }) await Case("load-busy-notification-" + mode, 1000, 700, state => state.LoadBoundary(mode));
        await Case("load-reentrant-text-notification", 1000, 700, state => state.ReentrantLoad());
        await Case("original-command-single-multiple-node-readback", 1000, 700, state => state.Sends());
        await Case("send-draft-and-action-snapshot-boundaries", 1000, 700, state => state.SendBoundaries());
        await Case("empty-offline-script-transfer-shared-writes-gates", 1000, 700, state => state.Gates());
        await Case("resize-navigation-original-identity", 1440, 900, state => state.Resize());
        var summary = new
        {
            status = failures.Count == 0 ? "PASS-OWN-WPF-TEXT-WORKSPACE" : "FAIL-TEXT-WORKSPACE", reports,
            scope = "Shown owned Demo WPF, original text page/bindings/commands, real file I/O and actual Demo node readback. Busy notifications define the tested load/edit boundary; arbitrary mid-I/O interleavings are not claimed.",
            windows = Application.Current.Windows.Count, mainWindowNull = Application.Current.MainWindow is null, retainedOwners = retainedTextWorkspaces.Count,
            notRun = new[] { "physical GUI/input", "DPI", "native Clipboard", "native file picker", "real emulator/ADB/Chinese input", "arbitrary file-I/O interleavings", "final dual 30 minutes" },
            errors = failures.Select(error => error.ToString()).ToArray()
        };
        try { File.WriteAllText(Path.Combine(directory, "text-workspace.json"), JsonSerializer.Serialize(summary, JsonDefaults.Options)); }
        catch (Exception error) { failures.Add(new IOException("REPORT-FAIL: 文本验收 summary 写入失败", error)); }
        if (failures.Count != 0) throw new AggregateException("文本工作区验收未通过", failures);

        async Task Case(string name, int width, int height, Func<TextWorkspaceCase, Task> action)
        {
            if (stop) { reports.Add(new { name, status = "NOT-RUN", reason = "前例清理未证明任务及 owner 终态" }); return; }
            var state = new TextWorkspaceCase(Path.Combine(directory, name), width, height, dispatcher);
            var classes = new HashSet<string>(); Exception? failure = null, cleanup = null; var initialized = false;
            try { await state.Initialize(); initialized = true; await action(state); }
            catch (Exception error) { failure = error; classes.Add(error is TextWorkspacePrecondition || !initialized ? "PRECONDITION-FAIL" : "FIXTURE-FAIL"); }
            finally { try { await state.Cleanup(); } catch (Exception error) { cleanup = error; classes.Add("CLEANUP-FAIL"); stop = true; } }
            if (state.Failures.Count != 0) classes.Add("PRODUCT-FAIL");
            var report = new { name, status = classes.Count > 1 ? "MIXED-FAIL" : classes.SingleOrDefault() ?? "PASS", failureClasses = classes, initialized,
                checks = state.Checks, productFailures = state.Failures, observations = state.Observations, tasks = state.TaskStates(), backend = state.Backend.Report(),
                error = failure?.ToString(), cleanup = cleanup?.ToString(), state.CleanupOwner, state.ShutdownRequests, state.Closed, state.Retained };
            try { File.WriteAllText(Path.Combine(state.Directory, "case.json"), JsonSerializer.Serialize(report, JsonDefaults.Options)); }
            catch (Exception error) { classes.Add("REPORT-FAIL"); failures.Add(error); }
            reports.Add(classes.Contains("REPORT-FAIL") ? new { name, status = "REPORT-FAIL", preservedResult = (object)report } : (object)report);
            if (classes.Count != 0) failures.Add(new InvalidOperationException(name + ": " + string.Join(",", classes),
                new AggregateException(new[] { failure, cleanup }.OfType<Exception>().Concat(state.Failures.Select(item => new Exception(item))))));
        }
    }

    private sealed class TextWorkspacePrecondition(string message) : Exception(message);
    private sealed class TextWorkspaceCase(string directory, int width, int height, Dispatcher dispatcher)
    {
        internal string Directory => directory;
        internal readonly TextDemoBackend Backend = new();
        internal MainWindow Window = null!;
        internal WorkspaceViewModel Vm = null!;
        internal MatrixRuntime Runtime = null!;
        internal readonly List<string> Checks = [], Failures = [];
        internal readonly List<object> Observations = [];
        internal string CleanupOwner = "not-started";
        internal int ShutdownRequests;
        internal bool Closed, Retained;
        private readonly ConcurrentQueue<(string Name, Task Task)> tasks = new();
        private readonly List<(string Name, Func<Task> Dispose)> unadopted = [];
        private FrameworkElement page = null!;
        private TextBox input = null!;
        private TextBlock target = null!;
        private Button[] buttons = [];
        private System.Windows.Input.ICommand?[] buttonCommands = [];
        private BindingBase? inputBinding, targetBinding;
        private object[] objects = [];
        private DeviceRow[] rows = [];
        private bool cleanupStarted, userClose;
        private DeviceRow A => Vm.Devices.Single(row => row.Serial == "DEMO-001");
        private DeviceRow B => Vm.Devices.Single(row => row.Serial == "DEMO-002");
        private static void Require(bool value, string message) { if (!value) throw new TextWorkspacePrecondition(message); }
        private void Check(bool value, string expected, object? actual = null)
        { Checks.Add((value ? "PASS " : "FAIL ") + expected); Observations.Add(new { expected, actual, passed = value }); if (!value) Failures.Add(expected); }
        private T Track<T>(string name, T task) where T : Task { tasks.Enqueue((name, task)); return task; }
        private async Task Wait(Task task, string name = "operation")
        { if (!tasks.Any(item => ReferenceEquals(item.Task, task))) _ = Track(name, task); await task.WaitAsync(TimeSpan.FromSeconds(12)); }
        internal object[] TaskStates() => tasks.Select(item => (object)new { item.Name, status = item.Task.Status.ToString(), terminal = item.Task.IsCompleted }).ToArray();
        private async Task Settle()
        { foreach (var priority in new[] { DispatcherPriority.DataBind, DispatcherPriority.Render, DispatcherPriority.ApplicationIdle }) await Wait(dispatcher.InvokeAsync(Window.UpdateLayout, priority).Task, "settle-" + priority); }
        internal async Task Initialize()
        {
            Require(Application.Current.Windows.Count == 0 && Application.Current.MainWindow is null, "无前例或用户窗口"); System.IO.Directory.CreateDirectory(directory);
            var settings = new AppSettings { WindowWidth = width, WindowHeight = height, Language = "zh-CN", AutoStartApi = false, AutoConnect = false,
                AgentBaseUrl = null, AgentSerial = null, SyncMouse = false, SyncKeyboard = false, AdbPath = "", LdConsolePath = "", AdbAllowedSerials = [], LdSerialBindings = [] };
            var catalog = new DeviceCatalog(Backend); var writes = new DeviceWriteCoordinator();
            var metadata = new CatalogRepository(Path.Combine(directory, "db.sqlite3")); unadopted.Add(("metadata", () => { metadata.Dispose(); return Task.CompletedTask; }));
            var log = new RollingLog(Path.Combine(directory, "logs")); unadopted.Add(("log", () => log.DisposeAsync().AsTask()));
            var scripts = new ScriptScheduler(Backend, catalog, true, writes); unadopted.Add(("scripts", () => scripts.DisposeAsync().AsTask()));
            var automation = new AutomationService(Backend, catalog); var coordinator = new InputCoordinator(catalog, Backend, writes); unadopted.Add(("input", () => coordinator.DisposeAsync().AsTask()));
            var api = new ApiServiceManager(catalog, Backend, automation, log, writes); unadopted.Add(("api", () => api.DisposeAsync().AsTask()));
            Runtime = new MatrixRuntime { Backend = Backend, Devices = catalog, Scripts = scripts, Automation = automation, Input = coordinator, Api = api, Writes = writes,
                Settings = settings, FrozenConnectionSettings = settings, SettingsStore = new SettingsStore(Path.Combine(directory, "AppConfig.json")), Metadata = metadata, Log = log, IsDemo = true }; unadopted.Clear();
            Runtime.Api.ScriptWriteBusy = serial => scripts.GetStatus(serial).State is ScriptRunState.Running or ScriptRunState.Cancelling;
            Vm = new WorkspaceViewModel(Runtime, directory, true); await Wait(Vm.RefreshDevicesAsync(), "discover-demo");
            Require(Vm.Devices.Count == 2 && Vm.Devices.All(row => row.Info.Online), "真实 Demo 发现两在线设备"); Select(false);
            Window = new MainWindow(Vm) { ShowActivated = false }; Window.Closing -= Window.WindowClosing; Window.Closing += OnClosing; Window.Closed += OnClosed;
            Application.Current.MainWindow = Window; Window.Show(); Window.NavigateTo("文本"); await Settle();
            Require(Window.IsLoaded && Window.IsVisible && PresentationSource.FromVisual(Window) is not null && Math.Abs(Window.ActualWidth - width) < .6 && Math.Abs(Window.ActualHeight - height) < .6, "真实显示外窗尺寸匹配");
            page = (FrameworkElement)((TabItem)Window.tabs.SelectedItem).Content;
            input = VisualDescendants<TextBox>(page).Single(box => AutomationProperties.GetAutomationId(box) == nameof(WorkspaceViewModel.TextInput));
            target = VisualDescendants<TextBlock>(page).Single(text => BindingOperations.GetBinding(text, TextBlock.TextProperty)?.Path.Path == nameof(WorkspaceViewModel.TargetContext));
            buttons = VisualDescendants<Button>(page).ToArray();
            Require(buttons.Length == 3 && buttons.Count(button => ReferenceEquals(button.Command, Vm.TextCommand)) == 1
                && buttons.Select(AutomationProperties.GetName).ToHashSet().SetEquals(new[] { "发送文本", "粘贴", "加载文本文件" }), "原文本页三个职责按钮存在");
            buttonCommands = buttons.Select(button => button.Command).ToArray();
            inputBinding = BindingOperations.GetBindingBase(input, TextBox.TextProperty); targetBinding = BindingOperations.GetBindingBase(target, TextBlock.TextProperty);
            Require(inputBinding is Binding { Mode: BindingMode.TwoWay, UpdateSourceTrigger: UpdateSourceTrigger.PropertyChanged } binding && binding.Path.Path == nameof(WorkspaceViewModel.TextInput), "原即时 TwoWay 输入绑定");
            Require(Runtime.IsDemo && Vm.Demo && !Runtime.Api.BasicRunning && !Runtime.Api.AutomatorRunning && Runtime.Api.DevicePorts.Count == 0 && !Runtime.Settings.AutoStartApi && !Runtime.Settings.AutoConnect && Runtime.Settings.AdbAllowedSerials.Length == 0, "隔离 Demo 无授权 API Agent 或真实 ADB");
            rows = Vm.Devices.ToArray(); objects = Objects();
        }
        private void OnClosing(object? sender, CancelEventArgs args) { args.Cancel = true; userClose = true; }
        private void OnClosed(object? sender, EventArgs args) => Closed = true;
        private void Select(bool multiple)
        { foreach (var row in Vm.Devices) row.IsSelected = multiple; Vm.CurrentDevice = A; Vm.UpdateSelection(); }
        private object[] Objects() => [Vm, Runtime, Backend, Runtime.Devices, Runtime.Scripts, Runtime.Input, Runtime.Api, Runtime.Writes, Runtime.Metadata, Runtime.SettingsStore, page, input, target, .. buttons];
        private void Identity()
        {
            var actualPage = (FrameworkElement)((TabItem)Window.tabs.SelectedItem).Content;
            var actualInputs = VisualDescendants<TextBox>(actualPage).Where(box => AutomationProperties.GetAutomationId(box) == nameof(WorkspaceViewModel.TextInput)).ToArray();
            var actualTargets = VisualDescendants<TextBlock>(actualPage).Where(text => BindingOperations.GetBinding(text, TextBlock.TextProperty)?.Path.Path == nameof(WorkspaceViewModel.TargetContext)).ToArray();
            var actualButtons = VisualDescendants<Button>(actualPage).ToArray();
            Check(ReferenceEquals(Window.ViewModel, Vm) && ReferenceEquals(Vm.Runtime, Runtime) && ReferenceEquals(page, actualPage)
                && actualInputs.Length == 1 && ReferenceEquals(actualInputs[0], input) && actualTargets.Length == 1 && ReferenceEquals(actualTargets[0], target)
                && buttons.SequenceEqual(actualButtons, ReferenceEqualityComparer.Instance) && objects.SequenceEqual(Objects(), ReferenceEqualityComparer.Instance) && rows.SequenceEqual(Vm.Devices, ReferenceEqualityComparer.Instance), "从当前选中页真实重查原 VM Runtime 控件及设备行身份保持");
            Check(ReferenceEquals(inputBinding, BindingOperations.GetBindingBase(input, TextBox.TextProperty)) && ReferenceEquals(targetBinding, BindingOperations.GetBindingBase(target, TextBlock.TextProperty))
                && buttonCommands.SequenceEqual(actualButtons.Select(button => button.Command), ReferenceEqualityComparer.Instance) && buttons.Any(button => ReferenceEquals(button.Command, Vm.TextCommand)), "原输入和目标 BindingBase 及三个原命令保持");
        }
        private void EditNow(string text)
        {
            Require(input.IsEnabled && !input.IsReadOnly, "原文本编辑器可编辑"); var binding = BindingOperations.GetBindingExpression(input, TextBox.TextProperty); Require(binding is not null, "原输入绑定存在");
            input.SetCurrentValue(TextBox.TextProperty, text); binding!.UpdateSource();
        }
        private async Task Edit(string text) { EditNow(text); await Settle(); Check(Vm.TextInput == text && input.Text == text, "原输入绑定提交完整草稿", new { text, actual = Vm.TextInput }); }
        private async Task<string> FileSample(string name, string content, bool bom = false)
        { var path = Path.Combine(directory, name); await Wait(File.WriteAllTextAsync(path, content, new UTF8Encoding(bom)), "write-file-" + name); return path; }
        private async Task Load(string path) { await Wait(Vm.LoadTextFileAsync(path), "actual-load-" + Path.GetFileName(path)); await Settle(); }
        internal async Task Files()
        {
            foreach (var bom in new[] { false, true })
            {
                var expected = "中文首行 ASCII  spaces  \r\n第二行\nTAIL"; var path = await FileSample(bom ? "bom.txt" : "utf8.txt", expected, bom); await Edit("原草稿"); await Load(path);
                Check(Vm.TextInput == expected && input.Text == expected && Vm.Error.Length == 0 && Vm.Status == "加载文本完成", "真实 UTF8/BOM 中文空格换行精确加载：" + bom, new { expected, actual = Vm.TextInput, Vm.Status });
            }
            var empty = await FileSample("empty.txt", ""); await Load(empty); Check(Vm.TextInput.Length == 0 && input.Text.Length == 0 && !Vm.TextCommand.CanExecute(null) && Vm.Status == "加载文本完成", "空文件正常替换并禁用空发送");
            await Edit("missing-file 保留草稿"); var draft = Vm.TextInput; await Load(Path.Combine(directory, "missing.txt"));
            Check(Vm.Error.Length > 0 && Vm.Status == "操作失败" && Vm.TextInput == draft && input.Text == draft && !Vm.Busy, "真实 missing-file 错误可见且不改草稿并释放 Busy", new { Vm.Error, Vm.Status });
            var recovery = await FileSample("recovery.txt", "恢复中文\nTAIL"); await Load(recovery); Check(Vm.Error.Length == 0 && Vm.TextInput == "恢复中文\nTAIL" && !Vm.Busy && Vm.Status == "加载文本完成", "读取失败后真实后续加载恢复"); Identity();
        }
        internal async Task LoadBoundary(string mode)
        {
            const string initial = "草稿-A", changed = "新编辑-B"; var path = await FileSample("load.txt", "文件新内容\nTAIL"); await Edit(initial); var notifications = 0; var edits = 0;
            PropertyChangedEventHandler handler = (_, args) =>
            {
                if (args.PropertyName == nameof(WorkspaceViewModel.TextInput)) edits++;
                if (args.PropertyName != nameof(WorkspaceViewModel.Busy) || !Vm.Busy || notifications++ != 0) return;
                EditNow(mode == "same-value" ? initial : changed); if (mode == "edit-ABA") EditNow(initial);
            };
            Vm.PropertyChanged += handler; try { await Load(path); } finally { Vm.PropertyChanged -= handler; }
            var expected = mode == "same-value" ? "文件新内容\nTAIL" : mode == "edit-ABA" ? initial : changed;
            Require(notifications == 1, "真实 ExecuteAsync Busy 同步通知恰好一次");
            Check(Vm.TextInput == expected && input.Text == expected, "请求后 Busy 同步通知边界保护新草稿/ABA，同值仍加载：" + mode, new { expected, actual = Vm.TextInput, edits, notifications });
            Check(Vm.Status == (mode == "same-value" ? "加载文本完成" : "加载文本已停止"), "有新编辑的旧读取不冒称已替换，同值正常完成", Vm.Status);
            Check(!Vm.Busy && Vm.Error.Length == 0, "边界用例实际任务结束无读取错误"); Identity();
        }
        internal async Task ReentrantLoad()
        {
            var path = await FileSample("reentrant.txt", "通知内加载文件\nTAIL"); await Edit("A"); Task? loading = null; PropertyChangedEventHandler? handler = null;
            handler = (_, args) => { if (args.PropertyName != nameof(WorkspaceViewModel.TextInput)) return; Vm.PropertyChanged -= handler; loading = Track("load-inside-TextInput-notification", Vm.LoadTextFileAsync(path)); };
            Vm.PropertyChanged += handler; try { EditNow("B"); Require(loading is not null, "实际 TextInput 通知触发嵌套加载"); await Wait(loading!); await Settle(); } finally { Vm.PropertyChanged -= handler; }
            Check(Vm.TextInput == "通知内加载文件\nTAIL" && input.Text == Vm.TextInput && Vm.Error.Length == 0 && Vm.Status == "加载文本完成", "编辑通知内开始读取，没有更新编辑仍正常替换");
        }
        private async Task Readback(string serial, string expected)
        {
            var reading = Track("actual-demo-dump-" + serial, Backend.DumpAsync(serial)); await Wait(reading);
            var actual = reading.Result.ChildNodeList.Single(node => node.ResourceId == "com.matrix.demo:id/input").Text;
            Check(actual == expected, "真实 Demo 原输入节点全文回读：" + serial, new { expected, actual });
        }
        internal async Task Sends()
        {
            foreach (var multiple in new[] { false, true }) foreach (var sample in new[] { "ASCII plain", "中文输入", "  leading and trailing  ", "首行\nsecond\r\nTAIL" })
            {
                Select(multiple); await Edit(sample); var before = Backend.TextCalls.Length; Require(Vm.TextCommand.CanExecute(null), "合法原 TextCommand 可执行");
                await Wait(Vm.TextCommand.ExecuteAsync(), "original-text-command"); await Settle(); var calls = Backend.TextCalls.Skip(before).ToArray();
                Check(calls.Select(call => call.Serial).SequenceEqual(multiple ? new[] { A.Serial, B.Serial } : new[] { A.Serial }) && calls.All(call => call.Value == sample), "原命令逐目标精确调用一次", calls);
                foreach (var serial in multiple ? new[] { A.Serial, B.Serial } : new[] { A.Serial }) await Readback(serial, sample);
                Check(Vm.Error.Length == 0 && Vm.TextInput == sample && input.Text == sample && !Vm.Busy, "实际发送成功不清草稿，Busy 恢复");
            }
        }
        internal async Task SendBoundaries()
        {
            foreach (var mode in new[] { "unchanged", "edit-B", "edit-ABA" })
            {
                Select(true); const string original = "发送原文 A\nTAIL"; await Edit(original); var gate = Backend.Hold("text", A.Serial, original); var before = Backend.TextCalls.Length;
                var sending = Track("original-held-send-" + mode, Vm.TextCommand.ExecuteAsync()); await Wait(gate.Entered.Task, "actual-text-entered");
                Require(gate.Claimed && !sending.IsCompleted, "真实转发任务等待而非假 Busy"); Check(Vm.Busy && !Vm.TextCommand.CanExecute(null) && !buttons.Single(button => ReferenceEquals(button.Command, Vm.TextCommand)).IsEnabled && input.IsEnabled && Runtime.Writes.IsBusy(A.Serial) && Runtime.Writes.IsBusy(B.Serial), "原多目标发送持共同 Writes 并保留编辑");
                await Wait(Vm.TextCommand.ExecuteAsync(), "busy-duplicate-rejected"); Check(Backend.TextCalls.Length == before + 1, "忙时原命令拒绝重复后端调用");
                if (mode != "unchanged") await Edit("发送期间新草稿 B"); if (mode == "edit-ABA") await Edit(original); var expected = Vm.TextInput;
                Select(false); gate.Release(); await Wait(sending); await Settle();
                Check(Backend.TextCalls.Skip(before).Select(call => call.Serial).SequenceEqual(new[] { A.Serial, B.Serial }) && Backend.TextCalls.Skip(before).All(call => call.Value == original), "发送 action 内快照在等待期间保留原文和原目标");
                await Readback(A.Serial, original); await Readback(B.Serial, original); Check(Vm.TextInput == expected && input.Text == expected && !Runtime.Writes.IsBusy(A.Serial) && !Runtime.Writes.IsBusy(B.Serial), "发送完成保留未编辑/B/ABA 草稿并释放全部租约：" + mode);
            }
            Select(false); await Edit("通知前 A"); var notificationText = "Busy 通知内 B"; var notified = 0; var held = Backend.Hold("text", B.Serial, notificationText);
            PropertyChangedEventHandler? handler = null; handler = (_, args) => { if (args.PropertyName != nameof(WorkspaceViewModel.Busy) || !Vm.Busy) return; Vm.PropertyChanged -= handler; notified++; EditNow(notificationText); Vm.CurrentDevice = B; Vm.UpdateSelection(); };
            Vm.PropertyChanged += handler;
            try { var send = Track("action-snapshot-after-Busy-notification", Vm.TextCommand.ExecuteAsync()); await Wait(held.Entered.Task); Require(notified == 1 && held.Claimed, "原 Busy 通知实际更改文本和单目标"); held.Release(); await Wait(send); }
            finally { Vm.PropertyChanged -= handler; }
            await Readback(B.Serial, notificationText); Check(Vm.TextInput == notificationText && Vm.Error.Length == 0, "原发送快照仍在 action 内，采纳 Busy 通知编辑且不清草稿");
        }
        private async Task Rejected(string reason, bool direct = true)
        {
            await Settle(); var before = Backend.TextCalls.Length; Check(!Vm.TextCommand.CanExecute(null) && !buttons.Single(button => ReferenceEquals(button.Command, Vm.TextCommand)).IsEnabled, "原发送命令及实际按钮门禁：" + reason); await Wait(Vm.TextCommand.ExecuteAsync(), "rejected-command-" + reason);
            if (direct) await Wait(Vm.SendTextAsync(), "original-VM-boundary-" + reason);
            Check(Backend.TextCalls.Length == before, "门禁整组零后端写入：" + reason);
        }
        internal async Task Gates()
        {
            await Edit("  \r\n "); await Rejected("empty-text", false); await Edit("门禁保留草稿");
            foreach (var row in Vm.Devices) row.IsSelected = false; Vm.CurrentDevice = null; Vm.UpdateSelection(); await Rejected("no-target"); Check(Vm.Error.Length > 0, "无目标原错误可见");
            Select(true); Backend.SetOnline(B.Serial, false); await Wait(Vm.RefreshDevicesAsync()); await Rejected("offline-whole-group"); Check(Vm.Error.Length > 0, "含离线目标整组拒绝可见"); Backend.SetOnline(B.Serial, true); await Wait(Vm.RefreshDevicesAsync()); Select(true);
            using (Runtime.Writes.Acquire([B.Serial]))
            { var before = Backend.TextCalls.Length; await Wait(Vm.TextCommand.ExecuteAsync(), "shared-Writes-conflict"); Check(Backend.TextCalls.Length == before && Vm.Error.Length > 0 && !Runtime.Writes.IsBusy(A.Serial), "共享 Writes 冲突原命令整组原子拒绝且不遗留 A 租约"); }
            var app = Backend.Hold("app", A.Serial, "com.android.settings"); var package = new ScriptPackage { Manifest = new ScriptManifest { Id = "text-gate", Name = "Text gate", Version = "1" }, Steps = [new ScriptStep { Action = "launchapp", Value = "com.android.settings" }] };
            var script = Track("original-script-run", Runtime.Scripts.RunAsync(A.Serial, package)); await Wait(app.Entered.Task); await Settle();
            Require(Runtime.Scripts.GetStatus(A.Serial).State == ScriptRunState.Running && Runtime.Writes.IsBusy(A.Serial), "真实脚本运行并持共同写租约"); await Rejected("script-whole-group"); app.Release(); await Wait(script); await Settle();
            Select(false); var file = await FileSample("transfer.txt", "真实传输中文"); Vm.QueuePush(file, false); var push = Backend.Hold("push", A.Serial, Vm.Transfers.Last().RemotePath);
            var transfer = Track("original-transfer-run", Vm.StartTransfersAsync()); await Wait(push.Entered.Task); Require(Vm.Transferring && Runtime.Writes.IsBusy(A.Serial), "真实上传等待并持共同 Writes"); await Rejected("transfer"); push.Release(); await Wait(transfer); await Settle();
            Check(!Vm.Transferring && Vm.Transfers.Last().State == "传输完成" && !Runtime.Writes.IsBusy(A.Serial), "原上传实际成功并释放传输门禁");
            Check(Vm.TextInput == "门禁保留草稿" && input.Text == Vm.TextInput, "各拒绝路径不清原草稿"); await Wait(Vm.TextCommand.ExecuteAsync(), "gate-recovery-send"); await Readback(A.Serial, Vm.TextInput); Check(Vm.Error.Length == 0, "所有原门禁解除后发送恢复");
        }
        internal async Task Geometry()
        {
            foreach (var row in new[] { A, B }) { Vm.CurrentDevice = row; Vm.DeviceName = "长目标中文-" + row.Serial + "-" + new string('W', 48) + "-目标尾"; await Wait(Vm.SaveMetadataAsync(), "original-metadata-pressure"); }
            Select(true); var text = "HEAD-中文首行\n" + string.Join('\n', Enumerable.Range(0, 80).Select(index => $"原多行正文 {index:D3} 中文 ASCII spaces " + new string('W', 100))) + "\nTAIL"; await Edit(text);
            foreach (var button in buttons) { button.BringIntoView(); await Settle(); Hit44(button); }
            input.BringIntoView(); await Settle(); var viewport = Reading(input); Check(input.AcceptsReturn && !input.IsReadOnly && viewport.Width >= 99.9 && viewport.Height >= 95.9, "原多行编辑器真实裁剪后阅读视口至少100x96", R(viewport));
            input.ScrollToHome(); input.ScrollToHorizontalOffset(0); await Settle(); Character(0, "HEAD"); input.ScrollToEnd(); input.ScrollToHorizontalOffset(double.MaxValue); await Settle(); Character(text.Length - 1, "TAIL");
            Check(Vm.TextInput == text && input.Text == text, "正文首尾滚读保持原完整长文本"); target.BringIntoView(); await Settle();
            var visible = Bounds(target); visible.Intersect(Clip(target)); var measured = new FormattedText(target.Text, CultureInfo.CurrentCulture, target.FlowDirection, new Typeface(target.FontFamily, target.FontStyle, target.FontWeight, target.FontStretch), target.FontSize, Brushes.Black, null, TextOptions.GetTextFormattingMode(target), VisualTreeHelper.GetDpi(target).PixelsPerDip) { MaxTextWidth = Math.Max(1, target.ActualWidth) };
            Check(target.Text == Vm.TargetContext && target.Text.Contains(A.Name, StringComparison.Ordinal) && target.Text.Contains(B.Name, StringComparison.Ordinal), "原 TargetContext 保留真实双目标长名称全文");
            Check(!visible.IsEmpty && measured.Height <= visible.Height + .75 && measured.WidthIncludingTrailingWhitespace <= visible.Width + .75, "长目标摘要实际完整可读", new { visible = R(visible), measuredHeight = measured.Height, targetCharacters = target.Text.Length, firstNameCharacters = A.Name.Length, secondNameCharacters = B.Name.Length, target.Text });
            Identity(); var path = Path.Combine(directory, "owned-text-shell.png"); SaveRender(path, Window.Shell); using var stream = File.OpenRead(path);
            Observations.Add(new { operation = "owned-shell-png", width, height, actualWidth = Window.ActualWidth, actualHeight = Window.ActualHeight, bytes = stream.Length, sha256 = Convert.ToHexString(SHA256.HashData(stream)), desktopCapture = false });
        }
        internal async Task Resize()
        {
            Select(true); await Edit("缩放导航保留草稿\n" + new string('W', 500) + "TAIL"); var draft = Vm.TextInput; var selected = Vm.CurrentDevice; var selections = Vm.Devices.Select(row => row.IsSelected).ToArray(); var calls = Backend.TextCalls.Length;
            foreach (var size in textWorkspaceSizes)
            {
                Window.Width = size.W; Window.Height = size.H; await Settle(); Window.NavigateTo("Shell"); await Settle(); Window.NavigateTo("文本"); await Settle(); Identity();
                Check(Window.IsVisible && Math.Abs(Window.ActualWidth - size.W) < .6 && Math.Abs(Window.ActualHeight - size.H) < .6, "缩放实际外窗匹配：" + size);
                Check(Vm.TextInput == draft && input.Text == draft && ReferenceEquals(selected, Vm.CurrentDevice) && selections.SequenceEqual(Vm.Devices.Select(row => row.IsSelected)) && Backend.TextCalls.Length == calls, "缩放导航保留草稿/目标/勾选且零发送");
            }
        }
        private Rect Bounds(FrameworkElement item) => item.TransformToAncestor(Window.Shell).TransformBounds(new Rect(item.RenderSize));
        private Rect Clip(FrameworkElement item)
        {
            var clip = new Rect(Window.Shell.RenderSize);
            for (DependencyObject? current = item; current is Visual && !ReferenceEquals(current, Window.Shell); current = VisualTreeHelper.GetParent(current))
                if (current is FrameworkElement element && (element.ClipToBounds || element is ScrollContentPresenter)) clip.Intersect(Bounds(element));
            return clip;
        }
        private Rect Reading(FrameworkElement item) { var presenter = VisualDescendants<ScrollContentPresenter>(item).FirstOrDefault(); var bounds = Bounds(presenter ?? item); bounds.Intersect(Clip(presenter ?? item)); return bounds; }
        private static object R(Rect rect) => rect.IsEmpty ? new { empty = true, x = 0d, y = 0d, width = 0d, height = 0d } : new { empty = false, x = rect.X, y = rect.Y, width = rect.Width, height = rect.Height };
        private void Hit44(Button button)
        {
            var visible = Bounds(button); visible.Intersect(Clip(button)); Check(button.IsVisible && !visible.IsEmpty && visible.Width >= 43.9 && visible.Height >= 43.9, "原按钮祖先裁剪后44命中区域：" + AutomationProperties.GetName(button), R(visible));
            if (!button.IsEnabled || visible.IsEmpty) { Check(!button.IsEnabled && button.Command is not null && !button.Command.CanExecute(null), "禁用按钮原命令不可执行"); return; }
            var hit = Window.Shell.InputHitTest(new Point(visible.X + visible.Width / 2, visible.Y + visible.Height / 2)) as DependencyObject;
            Check(hit is not null && (ReferenceEquals(hit, button) || button.IsAncestorOf(hit)), "实际 enabled 中心命中原按钮");
        }
        private void Character(int index, string label)
        {
            var leading = input.GetRectFromCharacterIndex(index, false); var trailing = input.GetRectFromCharacterIndex(index, true);
            var character = leading.IsEmpty || trailing.IsEmpty ? Rect.Empty : new Rect(new Point(Math.Min(leading.X, trailing.X), Math.Min(leading.Y, trailing.Y)), new Point(Math.Max(leading.Right, trailing.Right), Math.Max(leading.Bottom, trailing.Bottom)));
            var full = character.IsEmpty ? Rect.Empty : input.TransformToAncestor(Window.Shell).TransformBounds(character); var visible = full; if (!visible.IsEmpty) visible.Intersect(Reading(input));
            Check(!full.IsEmpty && full.Width > 0 && full.Height > 0 && !visible.IsEmpty && visible.Width + .75 >= full.Width && visible.Height + .75 >= full.Height,
                "正文首尾完整字符矩形处于裁剪后视口：" + label, new { glyph = input.Text[index].ToString(), full = R(full), visible = R(visible) });
        }
        internal async Task Cleanup()
        {
            Require(!cleanupStarted, "唯一清理 owner"); cleanupStarted = true; Backend.ReleaseAll();
            try { if (Window is not null) { Window.Closing -= OnClosing; LocalizationService.Current.LanguageChanged -= Window.OnLanguageChanged; Window.AbortStartup(); } }
            catch { Retain(); throw; }
            var drain = Track("original-task-drain", Task.WhenAll(tasks.Select(item => item.Task).Concat(Backend.Tasks()).ToArray()));
            try { await drain.WaitAsync(TimeSpan.FromSeconds(15)); } catch { if (!drain.IsCompleted) { Retain(); throw new InvalidOperationException("原任务未排空，保留 owner 并停止后继"); } }
            CleanupOwner = Vm is not null ? "VM-Shutdown" : Runtime is not null ? "Runtime-Dispose" : "unadopted-owners";
            try
            {
                if (Vm is not null) { ShutdownRequests++; await Wait(Track("VM-shutdown", Vm.ShutdownAsync())); }
                else if (Runtime is not null) { ShutdownRequests++; await Wait(Track("runtime-dispose", Runtime.DisposeAsync().AsTask())); }
                else { foreach (var owner in unadopted.AsEnumerable().Reverse()) await Wait(owner.Dispose(), "unadopted-" + owner.Name); unadopted.Clear(); await Wait(Backend.DisposeAsync().AsTask(), "backend-dispose"); }
                Require(tasks.All(item => item.Task.IsCompleted) && Backend.Tasks().All(task => task.IsCompleted) && Backend.Disposed && Backend.GatesTerminal, "全部原任务/后端/屏障终态且实际释放");
                Require(!userClose && Application.Current.Windows.Count == 0 && (Window is null || Closed), "无用户干扰及残窗");
                if (Runtime is not null) { using var released = Runtime.Writes.AcquireAll(); Require(!Runtime.Api.BasicRunning && !Runtime.Api.AutomatorRunning && Runtime.Api.DevicePorts.Count == 0 && !Runtime.Api.HasActiveRequests, "无残留服务/请求/写租约"); }
                Application.Current.MainWindow = null; if (Window is not null) Window.Closed -= OnClosed;
                Observations.Add(new { operation = "cleanup", CleanupOwner, ShutdownRequests, Closed, Retained, windows = Application.Current.Windows.Count, pending = tasks.Count(item => !item.Task.IsCompleted) });
            }
            catch { Retain(); throw; }
            void Retain() { if (!Retained) { Retained = true; retainedTextWorkspaces.Add(this); } }
        }
    }

    private sealed class TextDemoBackend : IDeviceBackend, IIdentityPinnedDeviceBackend
    {
        internal sealed record Call(string Kind, string Serial, string Value);
        internal sealed class Gate(string kind, string serial, string value)
        {
            internal readonly string Kind = kind, Serial = serial, Value = value;
            internal readonly TaskCompletionSource Entered = new(TaskCreationOptions.RunContinuationsAsynchronously), Released = new(TaskCreationOptions.RunContinuationsAsynchronously), Returned = new(TaskCreationOptions.RunContinuationsAsynchronously);
            internal bool Claimed;
            internal void Release() => Released.TrySetResult();
        }
        private readonly DemoDeviceBackend inner = new();
        private readonly ConcurrentQueue<Call> calls = new();
        private readonly ConcurrentQueue<Task> tasks = new();
        private readonly List<Gate> gates = [];
        internal bool Disposed;
        internal Call[] TextCalls => calls.Where(call => call.Kind == "text").ToArray();
        internal Task[] Tasks() { lock (gates) return tasks.Concat(gates.SelectMany(gate => new[] { gate.Entered.Task, gate.Released.Task, gate.Returned.Task })).ToArray(); }
        internal bool GatesTerminal { get { lock (gates) return gates.All(gate => gate.Entered.Task.IsCompleted && gate.Released.Task.IsCompleted && gate.Returned.Task.IsCompleted); } }
        internal Gate Hold(string kind, string serial, string value) { var gate = new Gate(kind, serial, value); lock (gates) gates.Add(gate); return gate; }
        internal void SetOnline(string serial, bool online) => inner.SetOnline(serial, online);
        internal void ReleaseAll() { lock (gates) foreach (var gate in gates) { gate.Release(); if (!gate.Claimed) { gate.Entered.TrySetCanceled(); gate.Returned.TrySetCanceled(); } } }
        internal object Report() { lock (gates) return new { Disposed, calls = calls.ToArray(), tasks = Tasks().Select(task => new { status = task.Status.ToString(), terminal = task.IsCompleted }).ToArray(), gates = gates.Select(gate => new { gate.Kind, gate.Serial, gate.Value, gate.Claimed, entered = gate.Entered.Task.Status.ToString(), released = gate.Released.Task.Status.ToString(), returned = gate.Returned.Task.Status.ToString() }).ToArray(), forwarding = "Every operation forwards to one actual DemoDeviceBackend. Gates only delay original calls; no fake results, Busy, nodes or counters." }; }
        private Task<T> Forward<T>(string kind, string serial, string value, Func<Task<T>> action, CancellationToken ct = default)
        {
            var task = Run(); tasks.Enqueue(task); return task;
            async Task<T> Run()
            {
                calls.Enqueue(new(kind, serial, value)); Gate? gate; lock (gates) { gate = gates.FirstOrDefault(item => !item.Claimed && item.Kind == kind && item.Serial == serial && item.Value == value); if (gate is not null) gate.Claimed = true; }
                try { if (gate is not null) { gate.Entered.TrySetResult(); await gate.Released.Task.WaitAsync(ct); } var original = action(); tasks.Enqueue(original); return await original; }
                finally { gate?.Returned.TrySetResult(); }
            }
        }
        private Task Forward(string kind, string serial, string value, Func<Task> action, CancellationToken ct = default)
            => Forward(kind, serial, value, async () => { var original = action(); tasks.Enqueue(original); await original; return true; }, ct);
        public IIdentityPinnedDeviceCommands BindIdentity(DeviceCatalog catalog) => new Pinned(this, inner.BindIdentity(catalog));
        private sealed class Pinned(TextDemoBackend owner, IIdentityPinnedDeviceCommands commands) : IIdentityPinnedDeviceCommands
        {
            public bool IsDemo => commands.IsDemo;
            public Task<string> StartAppAsync(DeviceCommandTarget target, string packageName, CancellationToken ct = default) => owner.Forward("app", target.Serial, packageName, () => commands.StartAppAsync(target, packageName, ct), ct);
            public Task TapAsync(DeviceCommandTarget target, int x, int y, CancellationToken ct = default) => owner.Forward("tap", target.Serial, "", () => commands.TapAsync(target, x, y, ct), ct);
            public Task SwipeAsync(DeviceCommandTarget target, int x1, int y1, int x2, int y2, int durationMs, CancellationToken ct = default) => owner.Forward("swipe", target.Serial, "", () => commands.SwipeAsync(target, x1, y1, x2, y2, durationMs, ct), ct);
        }
        public Task<IReadOnlyList<DeviceInfo>> DiscoverAsync(CancellationToken ct = default) => Forward("discover", "", "", () => inner.DiscoverAsync(ct), ct);
        public Task<string> ShellAsync(string serial, string command, bool useRoot = false, CancellationToken ct = default) => Forward("shell", serial, command, () => inner.ShellAsync(serial, command, useRoot, ct), ct);
        public Task<byte[]> ScreenshotAsync(string serial, CancellationToken ct = default) => Forward("screenshot", serial, "", () => inner.ScreenshotAsync(serial, ct), ct);
        public Task<AndroidNode> DumpAsync(string serial, CancellationToken ct = default) => Forward("dump", serial, "", () => inner.DumpAsync(serial, ct), ct);
        public Task TapAsync(string serial, int x, int y, CancellationToken ct = default) => Forward("tap", serial, "", () => inner.TapAsync(serial, x, y, ct), ct);
        public Task SwipeAsync(string serial, int x1, int y1, int x2, int y2, int durationMs, CancellationToken ct = default) => Forward("swipe", serial, "", () => inner.SwipeAsync(serial, x1, y1, x2, y2, durationMs, ct), ct);
        public Task KeyEventAsync(string serial, int code, CancellationToken ct = default) => Forward("key", serial, code.ToString(CultureInfo.InvariantCulture), () => inner.KeyEventAsync(serial, code, ct), ct);
        public Task TextAsync(string serial, string text, CancellationToken ct = default) => Forward("text", serial, text, () => inner.TextAsync(serial, text, ct), ct);
        public Task<string> StartAppAsync(string serial, string packageName, string? activity = null, CancellationToken ct = default) => Forward("app", serial, packageName, () => inner.StartAppAsync(serial, packageName, activity, ct), ct);
        public Task<string> StopAppAsync(string serial, string packageName, CancellationToken ct = default) => Forward("stop", serial, packageName, () => inner.StopAppAsync(serial, packageName, ct), ct);
        public Task<bool> ClearAppAsync(string serial, string packageName, CancellationToken ct = default) => Forward("clear", serial, packageName, () => inner.ClearAppAsync(serial, packageName, ct), ct);
        public Task<IReadOnlyList<AppInfo>> ListAppsAsync(string serial, bool showSystem = false, bool needVersion = true, CancellationToken ct = default) => Forward("apps", serial, "", () => inner.ListAppsAsync(serial, showSystem, needVersion, ct), ct);
        public Task InstallAsync(string serial, string path, CancellationToken ct = default) => Forward("install", serial, path, () => inner.InstallAsync(serial, path, ct), ct);
        public Task UninstallAsync(string serial, string packageName, CancellationToken ct = default) => Forward("uninstall", serial, packageName, () => inner.UninstallAsync(serial, packageName, ct), ct);
        public Task PushAsync(string serial, string localPath, string remotePath, IProgress<double>? progress = null, CancellationToken ct = default) => Forward("push", serial, remotePath, () => inner.PushAsync(serial, localPath, remotePath, progress, ct), ct);
        public Task PullAsync(string serial, string remotePath, string localPath, IProgress<double>? progress = null, CancellationToken ct = default) => Forward("pull", serial, remotePath, () => inner.PullAsync(serial, remotePath, localPath, progress, ct), ct);
        public Task SetNodeTextAsync(string serial, NodeSelector selector, string text, CancellationToken ct = default) => Forward("node-text", serial, text, () => inner.SetNodeTextAsync(serial, selector, text, ct), ct);
        public async ValueTask DisposeAsync() { ReleaseAll(); await Task.WhenAll(tasks.ToArray()).ContinueWith(_ => { }, TaskScheduler.Default); await inner.DisposeAsync(); Disposed = true; }
    }
}
