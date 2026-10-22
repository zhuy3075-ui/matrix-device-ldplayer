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
    private static readonly List<object> retainedFilesWorkspaces = [];
    private static readonly (int W, int H)[] filesWorkspaceSizes = [(390, 640), (390, 844), (768, 1024), (1000, 700), (1440, 900), (1920, 1080)];

    internal static async Task RunFilesWorkspaceVerificationAsync(string directory, Dispatcher dispatcher)
    {
        if (!dispatcher.CheckAccess() || Thread.CurrentThread.GetApartmentState() != ApartmentState.STA || Application.Current.Windows.Count != 0 || Application.Current.MainWindow is not null
            || !Path.IsPathFullyQualified(directory) || !directory.Contains(Path.DirectorySeparatorChar + "artifacts" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) || Directory.Exists(directory) || File.Exists(directory))
            throw new InvalidOperationException("PRECONDITION-FAIL: 文件验收要求 fresh artifacts 绝对目录及无用户窗口的自有 STA。");
        Directory.CreateDirectory(directory);
        var reports = new List<object>(); var failures = new List<Exception>(); var stop = false;
        foreach (var size in filesWorkspaceSizes) await Case($"geometry-{size.W}-{size.H}", size.W, size.H, state => state.Geometry());
        await Case("completed-push-then-queued-push-progress", 1000, 700, state => state.QueueProgress(false));
        await Case("completed-push-then-queued-pull-progress", 1000, 700, state => state.QueueProgress(true));
        await Case("pull-missing-local-same-realized-size-cell", 1000, 700, state => state.SizeCell(false));
        await Case("pull-existing-local-same-realized-size-cell", 1000, 700, state => state.SizeCell(true));
        await Case("current-targets-and-enqueued-action-snapshot", 1000, 700, state => state.TargetSnapshot());
        await Case("cancel-original-forward-task-and-retry", 1000, 700, state => state.CancelRetry());
        await Case("real-missing-file-failure-and-recovery", 1000, 700, state => state.FailureRecovery());
        await Case("original-queue-script-and-shared-writes-gates", 1000, 700, state => state.Gates());
        await Case("resize-navigation-original-identity", 1440, 900, state => state.Resize());
        var summary = new
        {
            status = failures.Count == 0 ? "PASS-OWN-WPF-FILES-WORKSPACE" : "FAIL-FILES-WORKSPACE", reports,
            scope = "Shown owned Demo WPF, original files page/bindings/commands and actual Demo Push/Pull bytes. QueuePull direct method uses Current; picker UI gates are inspected but native dialogs are not invoked. No reconnect handoff is claimed.",
            windows = Application.Current.Windows.Count, mainWindowNull = Application.Current.MainWindow is null, retainedOwners = retainedFilesWorkspaces.Count,
            notRun = new[] { "physical GUI/input", "DPI", "native upload/download picker", "directory owned non-modal window", "APK installation", "actual API in-flight transfer conflict", "real emulator/ADB", "reconnect generation/transport handoff", "arbitrary file-I/O interleavings", "final dual 30 minutes" },
            errors = failures.Select(error => error.ToString()).ToArray()
        };
        try { File.WriteAllText(Path.Combine(directory, "files-workspace.json"), JsonSerializer.Serialize(summary, JsonDefaults.Options)); }
        catch (Exception error) { failures.Add(new IOException("REPORT-FAIL: 文件验收 summary 写入失败", error)); }
        if (failures.Count != 0) throw new AggregateException("文件工作区验收未通过", failures);

        async Task Case(string name, int width, int height, Func<FilesWorkspaceCase, Task> action)
        {
            if (stop) { reports.Add(new { name, status = "NOT-RUN", reason = "前例清理未证明任务及 owner 终态" }); return; }
            var state = new FilesWorkspaceCase(Path.Combine(directory, name), width, height, dispatcher);
            var classes = new HashSet<string>(); Exception? failure = null, cleanup = null; var initialized = false;
            try { await state.Initialize(); initialized = true; await action(state); }
            catch (Exception error) { failure = error; classes.Add(error is FilesWorkspacePrecondition || !initialized ? "PRECONDITION-FAIL" : "FIXTURE-FAIL"); }
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

    private sealed class FilesWorkspacePrecondition(string message) : Exception(message);
    private sealed class FilesWorkspaceCase(string directory, int width, int height, Dispatcher dispatcher)
    {
        internal string Directory => directory;
        internal readonly FilesDemoBackend Backend = new();
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
        private TextBox remote = null!;
        private DataGrid queue = null!;
        private ProgressBar progress = null!;
        private CheckBox autoInstall = null!;
        private Button[] buttons = [];
        private System.Windows.Input.ICommand?[] buttonCommands = [];
        private BindingBase? remoteBinding, progressBinding;
        private BindingExpressionBase? progressExpression;
        private DataGridTextColumn[] columns = [];
        private BindingBase?[] columnBindings = [];
        private object[] objects = [];
        private DeviceRow[] rows = [];
        private bool cleanupStarted, userClose;
        private DeviceRow A => Vm.Devices.Single(row => row.Serial == "DEMO-001");
        private DeviceRow B => Vm.Devices.Single(row => row.Serial == "DEMO-002");
        private static void Require(bool value, string message) { if (!value) throw new FilesWorkspacePrecondition(message); }
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
            Application.Current.MainWindow = Window; Window.Show(); Window.NavigateTo("文件"); await Settle();
            Require(Window.IsLoaded && Window.IsVisible && PresentationSource.FromVisual(Window) is not null && Math.Abs(Window.ActualWidth - width) < .6 && Math.Abs(Window.ActualHeight - height) < .6, "真实显示外窗尺寸匹配");
            page = (FrameworkElement)((TabItem)Window.tabs.SelectedItem).Content;
            remote = VisualDescendants<TextBox>(page).Single(box => AutomationProperties.GetAutomationId(box) == nameof(WorkspaceViewModel.RemotePath));
            queue = VisualDescendants<DataGrid>(page).Single(grid => ReferenceEquals(grid.ItemsSource, Vm.Transfers));
            progress = VisualDescendants<ProgressBar>(page).Single(bar => BindingOperations.GetBinding(bar, RangeBase.ValueProperty)?.Path.Path == nameof(WorkspaceViewModel.OverallProgress));
            autoInstall = VisualDescendants<CheckBox>(page).Single();
            buttons = VisualDescendants<Button>(page).Where(button => new[] { "查看目录", "添加上传", "添加下载", "开始", "停止" }.Contains(AutomationProperties.GetName(button))).ToArray();
            Require(buttons.Length == 5 && buttons.Any(button => ReferenceEquals(button.Command, Vm.StartTransfersCommand)) && buttons.Any(button => ReferenceEquals(button.Command, Vm.CancelTransfersCommand)), "原文件页五职责按钮及开始/停止命令存在");
            buttonCommands = buttons.Select(button => button.Command).ToArray();
            remoteBinding = BindingOperations.GetBindingBase(remote, TextBox.TextProperty); progressBinding = BindingOperations.GetBindingBase(progress, RangeBase.ValueProperty); progressExpression = BindingOperations.GetBindingExpressionBase(progress, RangeBase.ValueProperty);
            Require(remoteBinding is Binding { Mode: BindingMode.TwoWay, UpdateSourceTrigger: UpdateSourceTrigger.PropertyChanged } binding && binding.Path.Path == nameof(WorkspaceViewModel.RemotePath), "原路径即时 TwoWay 绑定");
            columns = queue.Columns.Cast<DataGridTextColumn>().ToArray(); columnBindings = columns.Select(column => column.Binding).ToArray();
            Require(columns.Select(ColumnPath).SequenceEqual(new[] { "Serial", "Direction", "Name", "Size", "RemotePath", "Progress", "State" }) && !autoInstall.IsChecked.GetValueOrDefault(), "原七列顺序/绑定及 APK 安装默认关闭");
            Require(Runtime.IsDemo && Vm.Demo && !Runtime.Api.BasicRunning && !Runtime.Api.AutomatorRunning && Runtime.Api.DevicePorts.Count == 0 && !Runtime.Settings.AutoStartApi && !Runtime.Settings.AutoConnect && Runtime.Settings.AdbAllowedSerials.Length == 0, "隔离 Demo 无授权 API Agent 或真实 ADB");
            rows = Vm.Devices.ToArray(); objects = Objects();
        }
        private void OnClosing(object? sender, CancelEventArgs args) { args.Cancel = true; userClose = true; }
        private void OnClosed(object? sender, EventArgs args) => Closed = true;
        private void Select(bool multiple)
        { foreach (var row in Vm.Devices) row.IsSelected = multiple; Vm.CurrentDevice = A; Vm.UpdateSelection(); }
        private static string ColumnPath(DataGridTextColumn column) => column.Binding switch
        { Binding value => value.Path.Path, MultiBinding value when value.Bindings[0] is Binding first => first.Path.Path, _ => "" };
        private object[] Objects() => [Vm, Runtime, Backend, Runtime.Devices, Runtime.Scripts, Runtime.Input, Runtime.Api, Runtime.Writes, Runtime.Metadata, Runtime.SettingsStore, page, remote, queue, progress, autoInstall, .. buttons, .. columns];
        private Button ButtonNamed(string name) => buttons.Single(button => AutomationProperties.GetName(button) == name);
        private void Identity()
        {
            var actualPage = (FrameworkElement)((TabItem)Window.tabs.SelectedItem).Content;
            var expectedNames = new[] { "查看目录", "添加上传", "添加下载", "开始", "停止" };
            var actualButtons = VisualDescendants<Button>(actualPage).Where(button => expectedNames.Contains(AutomationProperties.GetName(button))).ToArray();
            Check(ReferenceEquals(Window.ViewModel, Vm) && ReferenceEquals(Vm.Runtime, Runtime) && ReferenceEquals(actualPage, page)
                && VisualDescendants<TextBox>(actualPage).Any(box => ReferenceEquals(box, remote))
                && VisualDescendants<DataGrid>(actualPage).Any(grid => ReferenceEquals(grid, queue)) && ReferenceEquals(queue.ItemsSource, Vm.Transfers)
                && VisualDescendants<ProgressBar>(actualPage).Any(bar => ReferenceEquals(bar, progress))
                && VisualDescendants<CheckBox>(actualPage).Any(box => ReferenceEquals(box, autoInstall))
                && objects.SequenceEqual(Objects(), ReferenceEqualityComparer.Instance) && rows.SequenceEqual(Vm.Devices, ReferenceEqualityComparer.Instance), "原文件页/控件/VM/Runtime/目标行身份保持");
            Check(ReferenceEquals(remoteBinding, BindingOperations.GetBindingBase(remote, TextBox.TextProperty)) && ReferenceEquals(progressBinding, BindingOperations.GetBindingBase(progress, RangeBase.ValueProperty))
                && ReferenceEquals(progressExpression, BindingOperations.GetBindingExpressionBase(progress, RangeBase.ValueProperty)) && columns.SequenceEqual(queue.Columns.Cast<DataGridTextColumn>(), ReferenceEqualityComparer.Instance)
                && columnBindings.SequenceEqual(columns.Select(column => column.Binding), ReferenceEqualityComparer.Instance)
                && actualButtons.Length == 5 && actualButtons.Select(AutomationProperties.GetName).ToHashSet().SetEquals(expectedNames)
                && buttons.SequenceEqual(actualButtons, ReferenceEqualityComparer.Instance) && buttonCommands.SequenceEqual(actualButtons.Select(button => button.Command), ReferenceEqualityComparer.Instance), "原路径/进度/七列绑定与当前页重枚举的五原按钮及命令不重绑");
        }
        private async Task EditPath(string value)
        {
            Require(remote.IsEnabled && !remote.IsReadOnly, "原路径可编辑"); var binding = BindingOperations.GetBindingExpression(remote, TextBox.TextProperty);
            Require(binding is not null, "原路径绑定存在"); remote.SetCurrentValue(TextBox.TextProperty, value); binding!.UpdateSource(); await Settle();
            Check(Vm.RemotePath == value && remote.Text == value, "原路径 TwoWay 完整提交", new { value, model = Vm.RemotePath, editor = remote.Text });
        }
        private async Task<string> Sample(string name, int length)
        {
            var path = Path.Combine(directory, name); var bytes = Enumerable.Range(0, length).Select(index => (byte)(index * 37 + 19)).ToArray();
            await Wait(File.WriteAllBytesAsync(path, bytes), "actual-local-write-" + name); return path;
        }
        private async Task RunQueue(string name)
        {
            Require(Vm.StartTransfersCommand.CanExecute(null), "原开始命令有实际 pending 队列");
            await Wait(Vm.StartTransfersCommand.ExecuteAsync(), name); await DrainForwarded();
        }
        private async Task DrainForwarded()
        {
            var original = Backend.OperationTasks();
            if (original.Length > 0)
            {
                var drain = Task.WhenAll(original);
                try { await Wait(drain, "actual-forwarded-task-drain"); }
                catch when (drain.IsCompleted) { Observations.Add(new { operation = "terminal-backend-faults-observed", faulted = original.Count(task => task.IsFaulted), cancelled = original.Count(task => task.IsCanceled) }); }
            }
            await Settle(); await Settle();
            Require(Backend.OperationTasks().All(task => task.IsCompleted), "真实转发/inner Task 已终态且 Dispatcher 正常排空");
        }
        private async Task RemoteBytes(string serial, string remotePath, string expectedLocal, string label)
        {
            var path = Path.Combine(directory, "readback-" + Guid.NewGuid().ToString("N") + ".bin");
            await Wait(Backend.PullAsync(serial, remotePath, path), "actual-demo-pull-readback-" + label);
            var actual = File.ReadAllBytes(path); var expected = File.ReadAllBytes(expectedLocal);
            Check(actual.AsSpan().SequenceEqual(expected) && SHA256.HashData(actual).AsSpan().SequenceEqual(SHA256.HashData(expected)), "真实 Demo 远端文件全文 bytes/SHA 回读：" + label,
                new { serial, remotePath, expectedBytes = expected.Length, actualBytes = actual.Length, expectedSha256 = Convert.ToHexString(SHA256.HashData(expected)), actualSha256 = Convert.ToHexString(SHA256.HashData(actual)) });
        }
        internal async Task QueueProgress(bool pull)
        {
            Select(false); await EditPath("/sdcard/files-progress"); var first = await Sample("first.bin", 4097); Vm.QueuePush(first, false);
            await RunQueue("original-first-completed-push"); var row = Vm.Transfers.Single();
            Require(row.State == "传输完成" && row.Progress == 100 && Vm.OverallProgress == 100 && progress.Value == 100 && !Vm.Transferring, "AP073 前置：真实队列完成/原 Task 排空/同一进度条已100");
            var notifications = 0; PropertyChangedEventHandler handler = (_, args) => { if (args.PropertyName == nameof(WorkspaceViewModel.OverallProgress)) notifications++; };
            Vm.PropertyChanged += handler;
            try
            {
                await Settle(); var before = notifications; await Settle(); Require(before == notifications, "AP073 新增前无迟到进度通知");
                if (pull) { await EditPath("/sdcard/new-pending-pull"); Vm.QueuePull(Path.Combine(directory, "pending-pull.bin")); }
                else Vm.QueuePush(await Sample("pending-push.bin", 8129), false);
                await Settle(); var added = Vm.Transfers.Last();
                Check(Vm.Transfers.Count == 2 && added.State == "待传输" && added.Progress == 0 && row.Progress == 100 && Vm.OverallProgress == 50, "原新增零行且平均算法仍50，未手设状态或进度");
                Check(progress.Value == 50 && ReferenceEquals(progressExpression, BindingOperations.GetBindingExpressionBase(progress, RangeBase.ValueProperty)), "同一原 ProgressBar 新增零行即时显示50：" + (pull ? "QueuePull" : "QueuePush"),
                    new { model = Vm.OverallProgress, control = progress.Value, notifications, existing = row.Progress, added = added.Progress });
            }
            finally { Vm.PropertyChanged -= handler; }
            Identity();
        }
        private async Task<CellIdentity> RealizeSize(TransferRow row)
        {
            var column = columns.Single(value => ColumnPath(value) == nameof(TransferRow.Size));
            queue.BringIntoView(); queue.ScrollIntoView(row, column); await Settle();
            var text = column.GetCellContent(row) as TextBlock; var container = queue.ItemContainerGenerator.ContainerFromItem(row) as DataGridRow;
            var cell = text is null ? null : Parent<DataGridCell>(text);
            Require(text is not null && container is not null && cell is not null && ReferenceEquals(text.DataContext, row) && ReferenceEquals(cell.DataContext, row), "AP074 原 Size 单元格已自然实现且绑定当前原行");
            return new CellIdentity(row, column, container!, cell!, text!);
        }
        private sealed class CellIdentity : IDisposable
        {
            internal readonly TransferRow Row; internal readonly DataGridTextColumn Column; internal readonly DataGridRow Container; internal readonly DataGridCell Cell; internal readonly TextBlock Text;
            internal readonly BindingBase? Binding; internal readonly BindingExpressionBase? Expression; internal int Unloaded, DataChanges;
            internal CellIdentity(TransferRow row, DataGridTextColumn column, DataGridRow container, DataGridCell cell, TextBlock text)
            {
                Row = row; Column = column; Container = container; Cell = cell; Text = text; Binding = BindingOperations.GetBindingBase(text, TextBlock.TextProperty); Expression = BindingOperations.GetBindingExpressionBase(text, TextBlock.TextProperty);
                foreach (var item in new FrameworkElement[] { container, cell, text }) { item.Unloaded += OnUnload; item.DataContextChanged += OnContext; }
            }
            private void OnUnload(object sender, RoutedEventArgs args) { if (ReferenceEquals(sender, args.OriginalSource)) Unloaded++; }
            private void OnContext(object sender, DependencyPropertyChangedEventArgs args) => DataChanges++;
            public void Dispose() { foreach (var item in new FrameworkElement[] { Container, Cell, Text }) { item.Unloaded -= OnUnload; item.DataContextChanged -= OnContext; } }
        }
        internal async Task SizeCell(bool existing)
        {
            Select(false); var source = await Sample("remote-source.bin", 32769); var remotePath = "/sdcard/size-" + Guid.NewGuid().ToString("N") + ".bin";
            await Wait(Backend.PushAsync(A.Serial, source, remotePath), "seed-real-demo-file"); await EditPath(remotePath);
            var local = Path.Combine(directory, "download.bin"); if (existing) await Wait(File.WriteAllBytesAsync(local, new byte[1024]), "write-different-existing-local");
            Vm.QueuePull(local); var row = Vm.Transfers.Single(); using var held = await RealizeSize(row);
            var oldValue = row.Size; Require(held.Text.Text == oldValue && oldValue != $"{new FileInfo(source).Length / 1024d:F1} KiB", "AP074 已实现同一 Size cell 确实为缺失/不同旧大小");
            var before = new { oldValue, text = held.Text.Text, existing, bytes = File.Exists(local) ? new FileInfo(local).Length : (long?)null };
            await RunQueue("original-download-to-same-realized-cell");
            var expected = File.ReadAllBytes(source); var actual = File.ReadAllBytes(local); var display = $"{actual.Length / 1024d:F1} KiB";
            Check(row.State == "传输完成" && actual.AsSpan().SequenceEqual(expected) && SHA256.HashData(actual).AsSpan().SequenceEqual(SHA256.HashData(expected)), "原下载真实本地 bytes/SHA 与远端源相同",
                new { before, expectedBytes = expected.Length, actualBytes = actual.Length, sha256 = Convert.ToHexString(SHA256.HashData(actual)), row.State });
            Check(row.Size == display, "原 Size getter 真实读取完成文件大小", new { expected = display, actual = row.Size });
            Check(held.Unloaded == 0 && held.DataChanges == 0 && ReferenceEquals(queue.ItemContainerGenerator.ContainerFromItem(row), held.Container) && ReferenceEquals(held.Column.GetCellContent(row), held.Text)
                && ReferenceEquals(held.Text.DataContext, row) && ReferenceEquals(held.Cell.DataContext, row) && ReferenceEquals(held.Binding, BindingOperations.GetBindingBase(held.Text, TextBlock.TextProperty))
                && ReferenceEquals(held.Expression, BindingOperations.GetBindingExpressionBase(held.Text, TextBlock.TextProperty)), "下载期间同一原 row/cell/TextBlock/BindingExpression 无回收或重绑",
                new { held.Unloaded, held.DataChanges });
            Check(held.Text.Text == display, "同一已实现 Size 单元格下载后即时显示真实大小，无刷新/重绑/后滚动",
                new { existing, oldValue, getter = row.Size, cell = held.Text.Text, expected = display }); Identity();
        }
        internal async Task TargetSnapshot()
        {
            var local = await Sample("target.bin", 6123); foreach (var row in Vm.Devices) row.IsSelected = ReferenceEquals(row, B); Vm.CurrentDevice = A; Vm.UpdateSelection(); await EditPath("/sdcard/selected-B");
            Vm.QueuePush(local, false); var upload = Vm.Transfers.Single();
            var source = await Sample("pull-source.bin", 9889); var fromA = "/sdcard/from-A.bin"; await Wait(Backend.PushAsync(A.Serial, source, fromA), "seed-current-A-download"); await EditPath(fromA);
            var destination = Path.Combine(directory, "current-A-pull.bin"); Vm.QueuePull(destination); var download = Vm.Transfers.Last();
            Check(upload.Serial == B.Serial && download.Serial == A.Serial && upload.RemotePath == "/sdcard/selected-B/" + Path.GetFileName(local) && download.RemotePath == fromA, "QueuePush 按 Targets(B)，QueuePull 仅 Current(A)，保入队路径快照");
            Select(true); Vm.CurrentDevice = B; Vm.UpdateSelection(); await EditPath("/sdcard/changed-after-queue");
            var gate = Backend.Hold("push", B.Serial, upload.RemotePath); var run = Track("original-enqueued-snapshot-run", Vm.StartTransfersCommand.ExecuteAsync()); await Wait(gate.Entered.Task);
            Require(Vm.Transferring && !run.IsCompleted && Runtime.Writes.IsBusy(A.Serial) && Runtime.Writes.IsBusy(B.Serial), "开始时完整 pending A/B 组真实占共同 Writes");
            var later = await Sample("later-pending.bin", 1223); Vm.QueuePull(later); var added = Vm.Transfers.Last(); gate.Release(); await Wait(run); await DrainForwarded();
            Check(upload.State == "传输完成" && download.State == "传输完成" && added.State == "待传输" && Backend.Calls("pull").All(call => call.Value != added.RemotePath), "开始后的新行不进入已冻结 pending 批次");
            await RemoteBytes(B.Serial, upload.RemotePath, local, "selected-B-upload");
            Check(File.ReadAllBytes(destination).AsSpan().SequenceEqual(File.ReadAllBytes(source)) && Vm.Error.Length == 0, "Current A 下载真实字节正确，后续目标/路径不接管旧队列");
            // QueuePull's public method is intentionally weaker than the native-picker command.
            Select(false); Backend.SetOnline(A.Serial, false); await Wait(Vm.RefreshDevicesAsync()); await Settle(); var count = Vm.Transfers.Count;
            Check(!ButtonNamed("添加下载").Command.CanExecute(null), "离线 Current 的原下载按钮禁用"); Vm.QueuePull(Path.Combine(directory, "offline-method-only.bin"));
            Check(Vm.Transfers.Count == count + 1 && Vm.Transfers.Last().Serial == A.Serial, "原 QueuePull 直接方法仍仅 Current，不虚构 CanWriteDevice 校验"); Backend.SetOnline(A.Serial, true); await Wait(Vm.RefreshDevicesAsync()); Identity();
        }
        internal async Task CancelRetry()
        {
            Select(false); await EditPath("/sdcard/cancel-retry"); var first = await Sample("cancel-first.bin", 9011); var second = await Sample("cancel-second.bin", 9777);
            Vm.QueuePush(first, false); Vm.QueuePush(second, false); var original = Vm.Transfers.ToArray(); var gate = Backend.Hold("push", A.Serial, original[0].RemotePath, true);
            var transfer = Track("original-cancellable-start-command", Vm.StartTransfersCommand.ExecuteAsync()); await Wait(gate.Entered.Task); await Settle();
            Require(gate.Claimed && !gate.InnerInvoked && Vm.Transferring && Runtime.Writes.IsBusy(A.Serial), "透明 forwarding gate 真实阻住原调用，inner 尚未调用");
            Require(Vm.CancelTransfersCommand.CanExecute(null) && ButtonNamed("停止").IsEnabled, "原停止命令及按钮真实启用"); Vm.CancelTransfersCommand.Execute(null); await Settle();
            Check(gate.Token.IsCancellationRequested && !transfer.IsCompleted && Vm.Transferring && !gate.Returned.Task.IsCompleted && !gate.Released.Task.IsCompleted && Runtime.Writes.IsBusy(A.Serial), "取消只发请求，原 Task 未返回前仍保 owner/租约而不假报停止",
                new { tokenCancelled = gate.Token.IsCancellationRequested, transferStatus = transfer.Status.ToString(), gate.InnerInvoked, Vm.Transferring });
            var conflict = false; try { using var unexpected = Runtime.Writes.Acquire([A.Serial]); } catch (DeviceException error) { conflict = error.Status == 409 && error.Code == 1003; }
            using (Runtime.Writes.Acquire([B.Serial])) Check(conflict, "取消排空前 A 实际409，独立 B 不被错误占用");
            gate.Release(); await Wait(transfer); await Settle();
            Check(original[0].State == "已停止" && original[1].State == "待传输" && !Vm.Transferring && !Runtime.Writes.IsBusy(A.Serial) && !Vm.CancelTransfersCommand.CanExecute(null), "同一真实取消任务终态后标停止/保后行并释放租约");
            Check(Backend.Calls("push").Count(call => call.Value == original[1].RemotePath) == 0, "取消后没有写入后续 pending 行");
            await RunQueue("original-cancelled-queue-retry"); Check(original.All(row => row.State == "传输完成") && Vm.Error.Length == 0, "原停止行/待传输行通过原开始命令真实重试恢复");
            await RemoteBytes(A.Serial, original[0].RemotePath, first, "cancel-first-retry"); await RemoteBytes(A.Serial, original[1].RemotePath, second, "cancel-second-retry"); Identity();
        }
        internal async Task FailureRecovery()
        {
            Select(false); await EditPath("/sdcard/recovery"); var absent = Path.Combine(directory, "not-yet-existing.bin"); Vm.QueuePush(absent, false); var upload = Vm.Transfers.Single(); await RunQueue("actual-missing-local-push");
            Check(upload.State.StartsWith("失败", StringComparison.Ordinal) && Vm.Error.Length > 0 && !Vm.Transferring && !Runtime.Writes.IsBusy(A.Serial), "真实 missing-local 后端失败可见/原任务终态/租约释放", new { upload.State, Vm.Error });
            await Wait(File.WriteAllBytesAsync(absent, new byte[] { 91, 12, 33, 0, 255 }), "create-real-local-for-retry"); await RunQueue("original-failed-upload-retry");
            Check(upload.State == "传输完成" && Vm.Error.Length == 0, "失败上传原行真实重试恢复"); await RemoteBytes(A.Serial, upload.RemotePath, absent, "failed-upload-retry");
            var missingRemote = "/sdcard/missing-" + Guid.NewGuid().ToString("N"); await EditPath(missingRemote); var local = Path.Combine(directory, "failed-pull.bin"); Vm.QueuePull(local); var download = Vm.Transfers.Last();
            await RunQueue("actual-missing-remote-pull"); Check(download.State.StartsWith("失败", StringComparison.Ordinal) && Vm.Error.Length > 0 && !File.Exists(local) && !Vm.Transferring, "真实 missing-remote 下载失败保不存在目标并释放门禁");
            await Wait(Backend.PushAsync(A.Serial, absent, missingRemote), "seed-missing-remote-for-retry"); await RunQueue("original-failed-download-retry");
            Check(download.State == "传输完成" && File.ReadAllBytes(local).AsSpan().SequenceEqual(File.ReadAllBytes(absent)) && Vm.Error.Length == 0, "下载失败同原行真实恢复且字节正确"); Identity();
        }
        internal async Task Gates()
        {
            var file = await Sample("gated.bin", 5011);
            Check(!Vm.StartTransfersCommand.CanExecute(null) && !Vm.CancelTransfersCommand.CanExecute(null), "空队列原开始/停止命令禁用");
            foreach (var device in Vm.Devices) device.IsSelected = false; Vm.CurrentDevice = null; Vm.UpdateSelection(); await Settle();
            var noPush = false; var noPull = false;
            try { Vm.QueuePush(file, false); } catch (InvalidOperationException) { noPush = true; }
            try { Vm.QueuePull(Path.Combine(directory, "no-current.bin")); } catch (InvalidOperationException) { noPull = true; }
            Check(noPush && noPull && Vm.Transfers.Count == 0 && !ButtonNamed("添加上传").Command.CanExecute(null) && !ButtonNamed("添加下载").Command.CanExecute(null), "无目标/Current 时原方法拒绝且 picker 命令禁用");
            Select(true); await EditPath("/sdcard/shared-gates"); Vm.QueuePush(file, false);
            using (Runtime.Writes.Acquire([B.Serial]))
            {
                var before = Backend.Calls("push").Length; await RunQueue("shared-Writes-conflict");
                Check(Backend.Calls("push").Length == before && Vm.Error.Length > 0 && Vm.Transfers.All(row => row.State == "待传输") && !Runtime.Writes.IsBusy(A.Serial), "原 pending A/B 全组 Writes 冲突零目标写且不遗留 A 租约");
            }
            var app = Backend.Hold("app", A.Serial, "com.android.settings"); var package = new ScriptPackage { Manifest = new ScriptManifest { Id = "files-gate", Name = "Files gate", Version = "1" }, Steps = [new ScriptStep { Action = "launchapp", Value = "com.android.settings" }] };
            var script = Track("actual-original-script", Runtime.Scripts.RunAsync(A.Serial, package)); await Wait(app.Entered.Task); await Settle();
            Require(Runtime.Scripts.GetStatus(A.Serial).State == ScriptRunState.Running && Runtime.Writes.IsBusy(A.Serial), "真实脚本与队列共用同一 Writes");
            Check(!ButtonNamed("添加上传").Command.CanExecute(null) && !ButtonNamed("添加下载").Command.CanExecute(null), "原 picker 按钮脚本门禁，不实际打开 picker");
            var pushBefore = Backend.Calls("push").Length; await Wait(Vm.StartTransfersCommand.ExecuteAsync(), "script-shared-Writes-blocks-pending-group"); await Settle();
            Check(Backend.Calls("push").Length == pushBefore && Vm.Error.Length > 0 && !Runtime.Writes.IsBusy(B.Serial), "真实脚本占共同租约时原队列全组零写，不假设独立 ScriptBusy 原子前置");
            app.Release(); await Wait(script); await Settle(); await RunQueue("original-recovery-after-script"); Check(Vm.Transfers.All(row => row.State == "传输完成"), "脚本及冲突解除后原队列恢复");
            foreach (var row in Vm.Transfers) await RemoteBytes(row.Serial, row.RemotePath, file, "gated-recovery-" + row.Serial);
            Backend.SetOnline(B.Serial, false); await Wait(Vm.RefreshDevicesAsync()); Select(true); await Settle(); var count = Vm.Transfers.Count; var rejected = false;
            try { Vm.QueuePush(file, false); } catch (InvalidOperationException) { rejected = true; } catch (DeviceException) { rejected = true; }
            Check(!ButtonNamed("添加上传").Command.CanExecute(null) && rejected && Vm.Transfers.Count == count, "QueuePush 原 RequireTargets 含离线整组拒绝且不新增行");
            Backend.SetOnline(B.Serial, true); await Wait(Vm.RefreshDevicesAsync()); Select(false); Identity();
        }
        internal async Task Geometry()
        {
            Select(false); var file = await Sample("文件-FIRST-" + new string('N', 68) + "-FILE-LAST.bin", 17219); await EditPath("/sdcard/" + new string('R', 150) + "-REMOTE-LAST"); Vm.QueuePush(file, false);
            await RunQueue("actual-geometry-completed-transfer"); var row = Vm.Transfers.Single(); var second = await Sample("geometry-held.bin", 111); Vm.QueuePush(second, false); var pending = Vm.Transfers.Last();
            ButtonNamed("开始").BringIntoView(); await Settle(); Hit44(ButtonNamed("开始"));
            var gate = Backend.Hold("push", A.Serial, pending.RemotePath); var transferring = Track("actual-geometry-held-transfer", Vm.StartTransfersCommand.ExecuteAsync()); await Wait(gate.Entered.Task); await Settle();
            Require(Vm.Transferring && ButtonNamed("停止").IsEnabled && !transferring.IsCompleted, "几何原 Stop 在真实 held 传输时启用");
            try
            {
                foreach (var button in buttons) { button.BringIntoView(); await Settle(); Hit44(button); }
                autoInstall.BringIntoView(); await Settle(); Hit44(autoInstall); Check(!autoInstall.IsChecked.GetValueOrDefault(), "原 APK 安装开关默认关闭且未执行安装");
                Check(ButtonNamed("查看目录").Command.CanExecute(null), "原目录读取 CanUseDevice 在传输时允许，未执行 owned ShowText 窗口");
                await ReadPath();
                queue.BringIntoView(); await Settle(); var viewport = Reading(queue);
                Check(!viewport.IsEmpty && viewport.Width >= 99.9 && viewport.Height >= 95.9, "原队列祖先裁剪后阅读视口至少100x96", R(viewport));
                Check(page.ActualWidth <= Window.Shell.ActualWidth + .6 && Bounds(page).Right <= Window.Shell.ActualWidth + .6, "文件页本体不水平溢出 Shell", R(Bounds(page)));
                foreach (var column in columns)
                {
                    queue.ScrollIntoView(row, column); await Settle(); var text = column.GetCellContent(row) as TextBlock;
                    Check(text is not null && ReferenceEquals(text.DataContext, row), "七列原行自然横滚可实现：" + ColumnPath(column));
                    if (text is null) continue; await ReadCell(text, CellValue(row, ColumnPath(column)), ColumnPath(column));
                }
                Identity();
            }
            finally { gate.Release(); await Wait(transferring); await Settle(); }
            var path = Path.Combine(directory, "owned-files-shell.png"); SaveRender(path, Window.Shell); using var stream = File.OpenRead(path);
            Observations.Add(new { operation = "owned-shell-png", width, height, actualWidth = Window.ActualWidth, actualHeight = Window.ActualHeight, path, bytes = stream.Length, sha256 = Convert.ToHexString(SHA256.HashData(stream)), desktopCapture = false, timing = "After natural column reading and actual held-transfer completion; not a failure-instant desktop capture" });
        }
        private static string CellValue(TransferRow row, string path) => path switch
        { "Serial" => row.Serial, "Direction" => row.Direction, "Name" => row.Name, "Size" => row.Size, "RemotePath" => row.RemotePath, "Progress" => row.Progress.ToString(CultureInfo.CurrentCulture), "State" => LocalizationService.Current.Get(row.State), _ => throw new FilesWorkspacePrecondition("未知原列：" + path) };
        private async Task ReadCell(TextBlock text, string expected, string name)
        {
            var visible = Bounds(text); visible.Intersect(Clip(text));
            var measured = new FormattedText(expected, CultureInfo.CurrentCulture, text.FlowDirection, new Typeface(text.FontFamily, text.FontStyle, text.FontWeight, text.FontStretch), text.FontSize, Brushes.Black, null, TextOptions.GetTextFormattingMode(text), VisualTreeHelper.GetDpi(text).PixelsPerDip);
            if (text.TextWrapping != TextWrapping.NoWrap) measured.MaxTextWidth = Math.Max(1, text.ActualWidth);
            var full = !visible.IsEmpty && measured.WidthIncludingTrailingWhitespace <= visible.Width + .75 && measured.Height <= visible.Height + .75;
            if (!full && text.ToolTip is not null) full = await ReadTip(text, expected);
            Check(text.Text == expected && full, "原七列全文值及完整可读/原全文提示：" + name, new { expected, actual = text.Text, visible = R(visible), measuredWidth = F(measured.WidthIncludingTrailingWhitespace), measuredHeight = F(measured.Height) });
        }
        private async Task<bool> ReadTip(FrameworkElement owner, string expected)
        {
            var original = owner.ToolTip; var binding = BindingOperations.GetBindingBase(owner, ToolTipProperty);
            var tip = original as System.Windows.Controls.ToolTip ?? new System.Windows.Controls.ToolTip { Content = original, PlacementTarget = owner, Placement = PlacementMode.Bottom }; var open = tip.IsOpen;
            try
            {
                if (original is not System.Windows.Controls.ToolTip) owner.SetCurrentValue(ToolTipProperty, tip); tip.SetCurrentValue(System.Windows.Controls.ToolTip.IsOpenProperty, true); await Settle();
                var text = VisualDescendants<TextBlock>(tip).FirstOrDefault(item => item.Text == expected);
                var valid = tip.IsOpen && tip.IsVisible && tip.ActualWidth > 0 && tip.ActualWidth <= Math.Max(360, Window.ActualWidth) && text is not null && text.IsVisible;
                if (valid)
                {
                    var measured = new FormattedText(expected, CultureInfo.CurrentCulture, text!.FlowDirection, new Typeface(text.FontFamily, text.FontStyle, text.FontWeight, text.FontStretch), text.FontSize, Brushes.Black, null, TextOptions.GetTextFormattingMode(text), VisualTreeHelper.GetDpi(text).PixelsPerDip);
                    if (text.TextWrapping != TextWrapping.NoWrap) measured.MaxTextWidth = Math.Max(1, text.ActualWidth);
                    valid = measured.WidthIncludingTrailingWhitespace <= text.ActualWidth + .75 && measured.Height <= text.ActualHeight + .75 && text.ActualHeight <= tip.ActualHeight + .75;
                }
                Check(ReferenceEquals(binding, BindingOperations.GetBindingBase(owner, ToolTipProperty)), "读取原 tooltip 保留原 BindingBase"); Observations.Add(new { operation = "actual-original-tooltip", expected, valid, width = F(tip.ActualWidth), height = F(tip.ActualHeight) }); return valid;
            }
            finally { tip.SetCurrentValue(System.Windows.Controls.ToolTip.IsOpenProperty, open); if (original is not System.Windows.Controls.ToolTip) owner.SetCurrentValue(ToolTipProperty, original); }
        }
        private async Task ReadPath()
        {
            remote.BringIntoView(); await Settle(); Require(remote.Text.Length > 100, "路径确有长文本压力");
            remote.ScrollToHome(); remote.ScrollToHorizontalOffset(0); await Settle(); Character(0, "路径首字符");
            remote.ScrollToEnd(); remote.ScrollToHorizontalOffset(double.MaxValue); await Settle(); Character(remote.Text.Length - 1, "路径末字符");
        }
        private void Character(int index, string label)
        {
            var leading = remote.GetRectFromCharacterIndex(index, false); var trailing = remote.GetRectFromCharacterIndex(index, true);
            var character = leading.IsEmpty || trailing.IsEmpty ? Rect.Empty : new Rect(new Point(Math.Min(leading.X, trailing.X), Math.Min(leading.Y, trailing.Y)), new Point(Math.Max(leading.Right, trailing.Right), Math.Max(leading.Bottom, trailing.Bottom)));
            var full = character.IsEmpty ? Rect.Empty : remote.TransformToAncestor(Window.Shell).TransformBounds(character); var visible = full; if (!visible.IsEmpty) visible.Intersect(Reading(remote));
            Check(!full.IsEmpty && full.Width > 0 && full.Height > 0 && !visible.IsEmpty && visible.Width + .75 >= full.Width && visible.Height + .75 >= full.Height, "原路径首尾完整字符处于祖先裁剪后视口：" + label, new { glyph = remote.Text[index].ToString(), full = R(full), visible = R(visible) });
        }
        internal async Task Resize()
        {
            Select(true); await EditPath("/sdcard/resize-" + new string('P', 160) + "-TAIL"); var file = await Sample("resize.bin", 1007); Vm.QueuePush(file, false);
            var draft = remote.Text; var transfers = Vm.Transfers.ToArray(); var selected = Vm.CurrentDevice; var selections = Vm.Devices.Select(row => row.IsSelected).ToArray(); var calls = Backend.Calls("push").Length;
            foreach (var size in filesWorkspaceSizes)
            {
                Window.Width = size.W; Window.Height = size.H; await Settle(); Window.NavigateTo("文本"); await Settle(); Window.NavigateTo("文件"); await Settle(); Identity();
                Check(Window.IsVisible && Math.Abs(Window.ActualWidth - size.W) < .6 && Math.Abs(Window.ActualHeight - size.H) < .6, "逐格实际外窗尺寸匹配：" + size, new { requestedWidth = size.W, requestedHeight = size.H, actualWidth = F(Window.ActualWidth), actualHeight = F(Window.ActualHeight) });
                Check(remote.Text == draft && Vm.RemotePath == draft && transfers.SequenceEqual(Vm.Transfers, ReferenceEqualityComparer.Instance) && ReferenceEquals(selected, Vm.CurrentDevice) && selections.SequenceEqual(Vm.Devices.Select(row => row.IsSelected)) && calls == Backend.Calls("push").Length, "缩放导航保原路径/原队列/目标/勾选且不发文件写入");
            }
        }
        private static T? Parent<T>(DependencyObject element) where T : DependencyObject
        { for (DependencyObject? current = element; current is Visual; current = VisualTreeHelper.GetParent(current)) if (current is T match) return match; return null; }
        private Rect Bounds(FrameworkElement item) => item.TransformToAncestor(Window.Shell).TransformBounds(new Rect(item.RenderSize));
        private Rect Clip(FrameworkElement item)
        {
            var clip = new Rect(Window.Shell.RenderSize);
            for (DependencyObject? current = item; current is Visual && !ReferenceEquals(current, Window.Shell); current = VisualTreeHelper.GetParent(current))
                if (current is FrameworkElement element)
                {
                    if (element.ClipToBounds || element is ScrollContentPresenter) clip.Intersect(Bounds(element));
                    var actualClip = VisualTreeHelper.GetClip(element); if (actualClip is not null) clip.Intersect(element.TransformToAncestor(Window.Shell).TransformBounds(actualClip.Bounds));
                }
            return clip;
        }
        private Rect Reading(FrameworkElement item) { var presenter = VisualDescendants<ScrollContentPresenter>(item).FirstOrDefault(); var bounds = Bounds(presenter ?? item); bounds.Intersect(Clip(presenter ?? item)); return bounds; }
        private static double? F(double value) => double.IsFinite(value) ? value : null;
        private static object R(Rect rect) => new { empty = rect.IsEmpty, x = F(rect.X), y = F(rect.Y), width = F(rect.Width), height = F(rect.Height) };
        private void Hit44(FrameworkElement control)
        {
            var visible = Bounds(control); visible.Intersect(Clip(control)); Check(control.IsVisible && !visible.IsEmpty && visible.Width >= 43.9 && visible.Height >= 43.9, "原控件祖先裁剪后44命中区域：" + AutomationProperties.GetName(control), R(visible));
            if (!control.IsEnabled || visible.IsEmpty) { Check(!control.IsEnabled && control is Button button && button.Command is not null && !button.Command.CanExecute(null), "禁用按钮只核原命令不可执行，不假造命中"); return; }
            var hit = Window.Shell.InputHitTest(new Point(visible.X + visible.Width / 2, visible.Y + visible.Height / 2)) as DependencyObject;
            Check(hit is not null && (ReferenceEquals(hit, control) || control.IsAncestorOf(hit)), "实际 enabled 中心命中原控件");
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
            void Retain() { if (!Retained) { Retained = true; retainedFilesWorkspaces.Add(this); } }
        }
    }

    private sealed class FilesDemoBackend : IDeviceBackend, IIdentityPinnedDeviceBackend
    {
        internal sealed record Call(string Kind, string Serial, string Value);
        internal sealed class Gate(string kind, string serial, string value, bool holdUntilReleased)
        {
            internal readonly string Kind = kind, Serial = serial, Value = value;
            internal readonly TaskCompletionSource Entered = new(TaskCreationOptions.RunContinuationsAsynchronously), Released = new(TaskCreationOptions.RunContinuationsAsynchronously), Returned = new(TaskCreationOptions.RunContinuationsAsynchronously);
            internal bool Claimed, InnerInvoked;
            internal CancellationToken Token;
            internal readonly bool HoldUntilReleased = holdUntilReleased;
            internal void Release() => Released.TrySetResult();
        }
        private readonly DemoDeviceBackend inner = new();
        private readonly ConcurrentQueue<Call> calls = new();
        private readonly ConcurrentQueue<Task> tasks = new();
        private readonly List<Gate> gates = [];
        internal bool Disposed;
        internal Call[] Calls(string kind) => calls.Where(call => call.Kind == kind).ToArray();
        internal Task[] OperationTasks() => tasks.ToArray();
        internal Task[] Tasks() { lock (gates) return tasks.Concat(gates.SelectMany(gate => new[] { gate.Entered.Task, gate.Released.Task, gate.Returned.Task })).ToArray(); }
        internal bool GatesTerminal { get { lock (gates) return gates.All(gate => gate.Entered.Task.IsCompleted && gate.Released.Task.IsCompleted && gate.Returned.Task.IsCompleted); } }
        internal Gate Hold(string kind, string serial, string value, bool holdUntilReleased = false) { var gate = new Gate(kind, serial, value, holdUntilReleased); lock (gates) gates.Add(gate); return gate; }
        internal void SetOnline(string serial, bool online) => inner.SetOnline(serial, online);
        internal void ReleaseAll() { lock (gates) foreach (var gate in gates) { gate.Release(); if (!gate.Claimed) { gate.Entered.TrySetCanceled(); gate.Returned.TrySetCanceled(); } } }
        internal object Report() { lock (gates) return new { Disposed, calls = calls.ToArray(), tasks = Tasks().Select(task => new { status = task.Status.ToString(), terminal = task.IsCompleted }).ToArray(), gates = gates.Select(gate => new { gate.Kind, gate.Serial, gate.Value, gate.Claimed, gate.InnerInvoked, gate.HoldUntilReleased, tokenCancellationRequested = gate.Token.IsCancellationRequested, entered = gate.Entered.Task.Status.ToString(), released = gate.Released.Task.Status.ToString(), returned = gate.Returned.Task.Status.ToString() }).ToArray(), forwarding = "Every operation forwards to one actual DemoDeviceBackend. Gates only delay original calls; no fake results, Busy, nodes or counters." }; }
        private Task<T> Forward<T>(string kind, string serial, string value, Func<Task<T>> action, CancellationToken ct = default)
        {
            var task = Run(); tasks.Enqueue(task); return task;
            async Task<T> Run()
            {
                calls.Enqueue(new(kind, serial, value)); Gate? gate; lock (gates) { gate = gates.FirstOrDefault(item => !item.Claimed && item.Kind == kind && item.Serial == serial && item.Value == value); if (gate is not null) gate.Claimed = true; }
                try
                {
                    if (gate is not null) { gate.Token = ct; gate.Entered.TrySetResult(); if (gate.HoldUntilReleased) await gate.Released.Task; else await gate.Released.Task.WaitAsync(ct); gate.InnerInvoked = true; }
                    var original = action(); tasks.Enqueue(original); return await original;
                }
                finally { gate?.Returned.TrySetResult(); }
            }
        }
        private Task Forward(string kind, string serial, string value, Func<Task> action, CancellationToken ct = default)
            => Forward(kind, serial, value, async () => { var original = action(); tasks.Enqueue(original); await original; return true; }, ct);
        public IIdentityPinnedDeviceCommands BindIdentity(DeviceCatalog catalog) => new Pinned(this, inner.BindIdentity(catalog));
        private sealed class Pinned(FilesDemoBackend owner, IIdentityPinnedDeviceCommands commands) : IIdentityPinnedDeviceCommands
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


