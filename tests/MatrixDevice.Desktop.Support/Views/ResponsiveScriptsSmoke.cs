using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using MatrixDevice.Core;
using MatrixDevice.Features;
using MatrixDevice.Host;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    internal static async Task RunScriptWorkspaceVerificationAsync(string directory, Dispatcher dispatcher)
    {
        if (!dispatcher.CheckAccess() || Thread.CurrentThread.GetApartmentState() != ApartmentState.STA
            || Application.Current.Windows.Count != 0 || Application.Current.MainWindow is not null)
            throw new InvalidOperationException("自动化工作区验收必须由无用户窗口的自有 Demo/STA 启动。");
        Directory.CreateDirectory(directory);
        var reports = new List<object>(); var failures = new List<Exception>(); var stop = false;
        await Case("active-log-reading-anchor", 1000, 700, async state =>
        {
            await state.Stage("actual-run-and-live-log", state.LiveLogReading);
        });
        var sizes = new[] { (W: 390, H: 640), (W: 390, H: 844), (W: 768, H: 1024), (W: 1000, H: 700), (W: 1440, H: 900), (W: 1920, H: 1080) };
        foreach (var size in sizes)
            await Case($"geometry-{size.W}-{size.H}", size.W, size.H, async state =>
            {
                await state.Stage("presentation-loaded", state.LoadPresentation);
                await state.Stage("geometry", state.Geometry);
                await state.Stage("parameters-first-middle-last-all-columns", () => state.ReadTable(state.Parameters, ["Key", "Label", "Type", "Range", "Value"]));
                await state.Stage("jobs-first-middle-last-all-columns", () => state.ReadTable(state.Jobs, ["Identity", "State", "Progress", "Package", "JobId", "Started", "Ended", "Result"]));
                await state.Stage("logs-first-middle-last-horizontal", state.ReadLogs);
                await state.Stage("result-full-body", state.ReadResult);
                await state.Stage("render-and-context", () => { state.Save("presentation"); state.PresentationContext(); return Task.CompletedTask; });
                await state.Stage("value-editors-first-middle-last", state.ReadValueEditors);
                await state.Stage("result-16k-and-visible-tail", state.ReadCompleteResult);
                await state.Stage("log-visible-tail", state.ReadLogTail);
                await state.Stage("different-job-result-binding", state.SwitchResultSelection);
            });
        await Case("actual-parameter-control-and-run", 1440, 900, state => state.Stage("actual-edit-submit-demo-effect", state.EditAndRun));
        await Case("invalid-json-zero-admission", 1440, 900, state => state.Stage("actual-invalid-edit-rejected", state.InvalidEdit));
        await Case("actual-job-selection-and-cancel", 1440, 900, state => state.Stage("actual-selected-job-cancel-b-continues", state.CancelSelection));
        await Case("resize-and-view-state", 1440, 900, state => state.Stage("same-controls-values-targets-and-logs", state.ResizeState));
        await Case("actual-500-log-tail-and-source", 1000, 700, state => state.Stage("real-window-tail-and-source-isolation", state.RealLogTail));
        var summary = new
        {
            status = failures.Count == 0 ? "PASS-OWN-WPF-SCRIPT-WORKSPACE" : "FAIL", reports,
            scope = "Real shown owned Demo WPF controls, original bindings and scheduler. Managed WPF render only; no OS input, external UIA, Sky, Android, ADB, API, user settings or native picker.",
            nativePicker = "NOT-RUN", functionalEditingAndCancellation = "OWN-WPF-CONTROLS-ONLY", oldAcceptance = "NOT-RUN-IN-THIS-MODE",
            windows = Application.Current.Windows.Count, mainWindowNull = Application.Current.MainWindow is null,
            errors = failures.Select(error => error.ToString()).ToArray()
        };
        try { File.WriteAllText(Path.Combine(directory, "script-workspace.json"), JsonSerializer.Serialize(summary, JsonDefaults.Options)); }
        catch (Exception error) { failures.Add(new IOException("FIXTURE-FAIL: 工作区报告写入失败", error)); }
        if (failures.Count != 0) throw new AggregateException("自动化工作区独立验收失败", failures);

        async Task Case(string name, int width, int height, Func<ScriptWorkspaceCase, Task> action)
        {
            if (stop) { reports.Add(new { name, status = "NOT-RUN", reason = "先前案例清理未完成" }); return; }
            var state = new ScriptWorkspaceCase(Path.Combine(directory, name), width, height, dispatcher);
            var classes = new HashSet<string>(); Exception? failure = null; Exception? cleanup = null; var initialized = false;
            try { await state.Initialize(); initialized = true; await action(state); }
            catch (Exception error) { failure = error; classes.Add(initialized ? "FIXTURE-FAIL" : "PRECONDITION-FAIL"); }
            finally { try { await state.Cleanup(); } catch (Exception error) { cleanup = error; classes.Add("CLEANUP-FAIL"); stop = true; } }
            if (state.Failures.Count > 0) classes.Add("PRODUCT-FAIL");
            foreach (var item in state.StageFailures) classes.Add(item.Classification);
            reports.Add(new { name, status = classes.Count > 1 ? "MIXED-FAIL" : classes.SingleOrDefault() ?? "PASS", failureClasses = classes,
                initialized, cleaned = cleanup is null, checks = state.Checks, stages = state.Stages, observations = state.Observations,
                productFailures = state.Failures, stageFailures = state.StageFailures, calls = state.Backend.Calls.ToArray(),
                jobs = state.TerminalJobs, backendDisposed = state.Backend.Disposed, backendActive = state.Backend.Active,
                error = failure?.ToString(), cleanup = cleanup?.ToString() });
            if (classes.Count > 0) failures.Add(new InvalidOperationException(name + ": " + string.Join(", ", classes),
                new AggregateException(new[] { failure, cleanup }.OfType<Exception>().Concat(state.StageFailures.Select(item => new Exception(item.Error))).Concat(state.Failures.Select(message => new Exception(message))))));
        }
    }

    private sealed class ScriptWorkspacePrecondition(string message) : Exception(message);
    private sealed record ScriptWorkspaceStageFailure(string Stage, string Classification, string Error);
    private sealed class ScriptWorkspaceCase(string directory, int width, int height, Dispatcher dispatcher)
    {
        internal readonly ScriptWorkspaceBackend Backend = new();
        internal MainWindow Window = null!;
        internal WorkspaceViewModel Vm = null!;
        internal MatrixRuntime Runtime = null!;
        internal readonly List<string> Checks = [], Failures = [];
        internal readonly List<object> Observations = [];
        internal readonly Dictionary<string, string> Stages = [];
        internal readonly List<ScriptWorkspaceStageFailure> StageFailures = [];
        internal object[] TerminalJobs = [];
        private readonly ConcurrentDictionary<Guid, TaskCompletionSource> terminals = new();
        private readonly List<Task> ownedTasks = [];
        private bool userClose, disposed;
        private FrameworkElement page = null!;
        private DataGrid parameters = null!, jobs = null!;
        private ListBox logs = null!;
        private FrameworkElement result = null!;
        private object[] parameterRows = [], jobRows = [];
        private string[] presentationLogs = [], callsBeforeLayout = [];
        private ScriptJobRow? presentationJob;
        private const string LongResult = "PRESENTATION-ONLY-BEGIN\n";
        private string resultText = "";
        internal DataGrid Parameters => parameters;
        internal DataGrid Jobs => jobs;
        private DeviceRow A => Vm.Devices.Single(row => row.Serial == "DEMO-001");
        private DeviceRow B => Vm.Devices.Single(row => row.Serial == "DEMO-002");

        internal async Task Initialize()
        {
            Directory.CreateDirectory(directory);
            var settings = new AppSettings { Language = "zh-CN", WindowWidth = width, WindowHeight = height, AutoStartApi = false,
                AutoConnect = false, SyncKeyboard = false, SyncMouse = false, AgentBaseUrl = null, AgentSerial = null };
            var catalog = new DeviceCatalog(Backend); var scheduler = new ScriptScheduler(Backend, catalog, true);
            var automation = new AutomationService(Backend, catalog); var log = new RollingLog(Path.Combine(directory, "logs"));
            Runtime = new MatrixRuntime { Backend = Backend, Devices = catalog, Scripts = scheduler, Automation = automation,
                Input = new InputCoordinator(catalog, Backend), Log = log, Metadata = new CatalogRepository(Path.Combine(directory, "db.sqlite3")),
                Settings = settings, FrozenConnectionSettings = settings, SettingsStore = new SettingsStore(Path.Combine(directory, "AppConfig.json")), IsDemo = true,
                Api = new ApiServiceManager(catalog, Backend, automation, log) };
            Runtime.Scripts.JobChanged += OnJob;
            Vm = new WorkspaceViewModel(Runtime, directory, true); await Vm.RefreshDevicesAsync().WaitAsync(TimeSpan.FromSeconds(5));
            Vm.CurrentDevice = A; A.IsSelected = true; Vm.UpdateSelection();
            Window = new MainWindow(Vm) { ShowActivated = false };
            Window.Closing -= Window.WindowClosing; Window.Closing += OnClose;
            Application.Current.MainWindow = Window;
            Window.Show(); Window.NavigateTo("脚本"); await Settle();
            Observe("actual-window-before-precondition", new { width, height, Window.ActualWidth, Window.ActualHeight, Window.IsLoaded, Window.IsVisible, Window.WindowState });
            Require(Window.IsLoaded && Window.IsVisible && PresentationSource.FromVisual(Window) is not null
                && Math.Abs(Window.ActualWidth - width) < .6 && Math.Abs(Window.ActualHeight - height) < .6, "实际已显示窗口尺寸必须等于请求尺寸");
            Require(Runtime.IsDemo && Vm.Demo && ReferenceEquals(Runtime.Backend, Backend) && !Runtime.Api.BasicRunning && !Runtime.Api.AutomatorRunning
                && Runtime.Api.DevicePorts.Count == 0 && !Runtime.Settings.AutoStartApi && !Runtime.Settings.AutoConnect
                && !Runtime.Settings.SyncKeyboard && !Runtime.Settings.SyncMouse, "自有 Demo 禁用所有外部接口、自动连接和同步");
            page = (FrameworkElement)((TabItem)Window.tabs.SelectedItem).Content;
            Require(Equals(((TabItem)Window.tabs.SelectedItem).Tag, "脚本"), "实际脚本页面已选中");
            var sections = VisualDescendants<TabControl>(page).Single();
            foreach (TabItem tab in sections.Items)
            {
                sections.SelectedItem = tab; await Settle();
                foreach (var expander in VisualDescendants<Expander>(page)) expander.IsExpanded = true;
                await Settle();
                parameters ??= VisualDescendants<DataGrid>(page).FirstOrDefault(element => ReferenceEquals(element.ItemsSource, Vm.Parameters))!;
                jobs ??= VisualDescendants<DataGrid>(page).FirstOrDefault(element => ReferenceEquals(element.ItemsSource, Vm.ScriptJobs))!;
                logs ??= VisualDescendants<ListBox>(page).FirstOrDefault(element => ReferenceEquals(element.ItemsSource, Vm.ScriptLogs))!;
                result ??= VisualDescendants<FrameworkElement>(page).FirstOrDefault(element => TextPath(element) == "SelectedScriptJob.Result")!;
            }
            Require(parameters is not null && jobs is not null && logs is not null && result is not null, "原参数、任务、日志和结果绑定控件存在（兼容原两标签及四视图）");
            Observe("initialized", new { requested = new { width, height }, actual = new { Window.ActualWidth, Window.ActualHeight }, Window.ShowActivated,
                views = sections.Items.Cast<TabItem>().Select(tab => tab.Header?.ToString()).ToArray(), backend = "owned forwarding Demo", nativePicker = "NOT-RUN" });
        }
        private void OnClose(object? sender, CancelEventArgs e) { e.Cancel = true; userClose = true; }
        private void OnJob(ScriptJobStatus value)
        {
            if (value.State is ScriptRunState.Completed or ScriptRunState.Cancelled or ScriptRunState.Failed)
                terminals.GetOrAdd(value.JobId, _ => new(TaskCreationOptions.RunContinuationsAsynchronously)).TrySetResult();
        }
        internal async Task Stage(string name, Func<Task> action)
        {
            var before = Failures.Count; Stages[name] = "STARTED";
            try { Require(!userClose, "用户未关闭自有验收窗口"); await action(); Stages[name] = Failures.Count == before ? "PASS" : "PRODUCT-FAIL"; }
            catch (Exception error) { var kind = error is ScriptWorkspacePrecondition ? "PRECONDITION-FAIL" : "FIXTURE-FAIL"; Stages[name] = kind; StageFailures.Add(new(name, kind, error.ToString())); }
        }
        private void Product(bool condition, string message)
        { if (condition) Checks.Add(message); else { Failures.Add(message); Observe("product-failure", message); } }
        private static void Require(bool condition, string message) { if (!condition) throw new ScriptWorkspacePrecondition(message); }
        private void Observe(string phase, object value) => Observations.Add(new { phase, value });
        private Task Track(Task task) { ownedTasks.Add(task); return task.WaitAsync(TimeSpan.FromSeconds(10)); }
        private async Task Settle()
        {
            await dispatcher.InvokeAsync(Window.UpdateLayout, DispatcherPriority.Render).Task.WaitAsync(TimeSpan.FromSeconds(5));
            await dispatcher.InvokeAsync(Window.UpdateLayout, DispatcherPriority.ContextIdle).Task.WaitAsync(TimeSpan.FromSeconds(5));
            await dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle).Task.WaitAsync(TimeSpan.FromSeconds(5));
        }
        private async Task Reveal(FrameworkElement element)
        {
            for (DependencyObject? item = element; item is not null && !ReferenceEquals(item, page); item = LogicalTreeHelper.GetParent(item) ?? (item is Visual ? VisualTreeHelper.GetParent(item) : null))
            {
                if (item is TabItem tab && ItemsControl.ItemsControlFromItemContainer(tab) is TabControl tabs) tabs.SelectedItem = tab;
                if (item is Expander expander) expander.IsExpanded = true;
            }
            await Settle();
        }
        private Button Tool(string caption) => VisualDescendants<Button>(page).Single(button => AutomationProperties.GetName(button) == caption);
        private async Task Invoke(Button button)
        {
            Require(button.IsVisible && button.IsEnabled, "实际按钮可见可执行：" + AutomationProperties.GetName(button));
            var peer = UIElementAutomationPeer.CreatePeerForElement(button);
            Require(peer?.GetPattern(PatternInterface.Invoke) is IInvokeProvider, "原 WPF 控件具有 Invoke provider");
            ((IInvokeProvider)peer!.GetPattern(PatternInterface.Invoke)).Invoke(); await Settle();
        }
        private async Task Import(ScriptPackage package)
        {
            var file = Path.Combine(directory, Guid.NewGuid().ToString("N") + ".jxscript"); ScriptPackageService.Save(file, package);
            await Track(Vm.ImportScriptAsync(file)); await Settle(); Require(Vm.Error.Length == 0, "真实 ZIP 导入成功：" + Vm.Error);
        }
        internal async Task LoadPresentation()
        {
            await Import(new ScriptPackage
            {
                Manifest = new() { Id = "presentation-only", Name = "呈现样本-" + new string('N', 90), Version = "1.0", Description = string.Join(" ", Enumerable.Repeat("长说明完整内容用于真实布局滚动阅读", 16)) },
                Parameters = Enumerable.Range(0, 24).Select(index => new ScriptParameter
                {
                    Key = $"key-{index:D2}-" + new string('K', 70), LabelZh = $"参数名称{index:D2}-" + new string('L', 90), Type = "string",
                    Unit = "呈现单位-" + new string('U', 60), Min = -1000, Max = 1000000,
                    Value = JsonSerializer.SerializeToElement($"value-{index:D2}-" + new string('V', 140) + "-TAIL")
                }).ToList(), Steps = [new() { Action = "wait", DurationMs = 0 }]
            });
            A.Info = A.Info with { Name = "目标 A-" + new string('A', 100) }; B.Info = B.Info with { Name = "目标 B-" + new string('B', 100) }; A.IsSelected = B.IsSelected = true; Vm.UpdateSelection();
            resultText = LongResult + string.Join("\n", Enumerable.Range(0, 128).Select(index => $"第{index:D3}行 " + new string('R', 100))) + "\nPRESENTATION-ONLY-TAIL";
            for (var index = 0; index < 100; index++) Vm.ScriptJobs.Add(new ScriptJobRow(new ScriptJobStatus(Guid.NewGuid(),
                new("PRESENTATION-ONLY-" + index + new string('S', 45), index + 1, null, true), "呈现脚本-" + new string('P', 80), "1.0",
                ScriptRunState.Completed, 9, 10, DateTimeOffset.UtcNow.AddMinutes(-index - 1), DateTimeOffset.UtcNow, Result: resultText)));
            presentationJob = Vm.ScriptJobs[0]; Vm.SelectedScriptJob = presentationJob;
            presentationLogs = Enumerable.Range(0, 500).Select(index => $"PRESENTATION-ONLY-LOG-{index:D3} " + new string('G', 140) + " TAIL").ToArray();
            foreach (var item in presentationLogs) Vm.ScriptLogs.Add(item);
            var beforeError = Backend.Calls.ToArray();
            await Track(Vm.ExecuteAsync("PRESENTATION-ONLY 错误压力", () => Task.FromException(new DeviceException(string.Join("\n", Enumerable.Range(0, 20).Select(index => "呈现用错误上下文，不是后端失败 " + index))))));
            Product(Backend.Calls.ToArray().SequenceEqual(beforeError), "原 ExecuteAsync 生成呈现错误压力不调用后端");
            await Settle(); parameterRows = Vm.Parameters.Cast<object>().ToArray(); jobRows = Vm.ScriptJobs.Cast<object>().ToArray(); callsBeforeLayout = Backend.Calls.ToArray();
            Observe("presentation-only", new { parameters = Vm.Parameters.Count, jobs = Vm.ScriptJobs.Count, logs = Vm.ScriptLogs.Count,
                resultCharacters = resultText.Length, backendRuns = Runtime.Scripts.GetJobs().Count, errorCharacters = Vm.Error.Length });
            Require(Runtime.Scripts.GetJobs().Count == 0, "呈现样本不假冒真实执行任务");
        }
        internal async Task Geometry()
        {
            var error = VisualDescendants<TextBlock>(Window.Shell).Single(text => TextPath(text) == nameof(WorkspaceViewModel.Error));
            var errorScroll = Parent<ScrollViewer>(error); Require(errorScroll is not null, "公共错误栏真实滚动容器");
            Observe("shared-error-pressure", new { error.Text, error.ActualHeight, scrollHeight = errorScroll!.ActualHeight, maxHeight = N(errorScroll.MaxHeight), bounds = R(Bounds(errorScroll)) });
            Product(errorScroll.IsVisible && Math.Abs(errorScroll.ActualHeight - 100) < .6, "长多行错误真实撑满公共错误栏100高，再测正文预算");
            foreach (var control in new[] { Tool("导入脚本包"), Tool("运行所选"), Tool("停止所选设备") }) Hit44(control);
            foreach (var control in new FrameworkElement[] { parameters, jobs, logs, result })
            {
                await Reveal(control); var viewport = ReadingViewport(control); var clip = Clip(control);
                Observe("reading-viewport", new { type = control.GetType().Name, source = TextPath(control), actual = R(Bounds(control)), viewport = R(viewport), clip = R(clip) });
                var minimum = control is DataGrid ? 96 : 48;
                Product(control.IsVisible && viewport.Width >= 100 && viewport.Height >= minimum, $"{control.GetType().Name} 真实可见阅读视口 >=100x{minimum}");
                if (control is DataGrid grid)
                {
                    Product(grid.ColumnHeaderHeight >= 43.9 && grid.RowHeight >= 43.9, "表格原行/列头实际配置支持44命中");
                    foreach (TabItem tab in VisualDescendants<TabControl>(page).Single().Items)
                    {
                        Hit44(tab);
                        if (AutomationProperties.GetName(tab) == "运行记录" && tab.Header is FrameworkElement) RecordCaptionHit(tab);
                    }
                }
            }
            await Reveal(parameters); Save("parameters"); await Reveal(jobs); Save("jobs"); await Reveal(logs); Save("logs"); await Reveal(result); Save("result");
        }
        private Rect Bounds(FrameworkElement element) => element.TransformToAncestor(Window.Shell).TransformBounds(new Rect(element.RenderSize));
        private Rect Clip(FrameworkElement element)
        {
            var clip = new Rect(Window.Shell.RenderSize);
            for (DependencyObject? item = element; item is Visual && !ReferenceEquals(item, Window.Shell); item = VisualTreeHelper.GetParent(item))
                if (item is FrameworkElement ancestor && (ancestor.ClipToBounds || ancestor is ScrollContentPresenter)) clip.Intersect(Bounds(ancestor));
            return clip;
        }
        private Rect ReadingViewport(FrameworkElement control)
        {
            var viewport = Bounds(control); viewport.Intersect(Clip(control));
            var presenter = VisualDescendants<ScrollContentPresenter>(control).FirstOrDefault();
            if (presenter is not null) viewport.Intersect(Bounds(presenter));
            else if (control is TextBlock)
            {
                var scroll = Parent<ScrollViewer>(control); if (scroll is not null) viewport.Intersect(Bounds(scroll));
            }
            return viewport;
        }
        private void Hit44(FrameworkElement control)
        {
            var bounds = Bounds(control); var clipped = bounds; clipped.Intersect(Clip(control));
            Product(control.IsVisible && clipped.Width >= 43.9 && clipped.Height >= 43.9, "祖先裁剪后44命中：" + (AutomationProperties.GetName(control) is { Length: > 0 } name ? name : control.GetType().Name));
            if (!control.IsEnabled)
                Product(presentationJob is not null && ReferenceEquals(control, Tool("停止所选设备"))
                    && ReferenceEquals(((Button)control).Command, Vm.StopScriptCommand) && !Vm.StopScriptCommand.CanExecute(null)
                    && Runtime.Scripts.GetJobs().All(job => job.State is ScriptRunState.Completed or ScriptRunState.Cancelled or ScriptRunState.Failed),
                    "仅呈现案例无真实活动任务时停止所选设备应禁用，不要求 disabled 控件的 enabledHit");
            else if (!clipped.IsEmpty && clipped.Width > 0 && clipped.Height > 0)
            {
                var point = new Point(clipped.X + clipped.Width / 2, clipped.Y + clipped.Height / 2);
                var hit = Window.Shell.InputHitTest(point) as DependencyObject;
                Product(hit is not null && (ReferenceEquals(hit, control) || control.IsAncestorOf(hit)), "实际命中仍归原控件：" + control.GetType().Name);
            }
            Observe("hit44", new { control = control.GetType().Name, name = AutomationProperties.GetName(control), bounds = R(bounds), clipped = R(clipped) });
        }
        private void RecordCaptionHit(TabItem recordTab)
        {
            var header = (FrameworkElement)recordTab.Header;
            var caption = VisualDescendants<TextBlock>(header).SingleOrDefault(text => text.Text == AutomationProperties.GetName(recordTab));
            var border = caption is null ? null : Parent<Border>(caption);
            Require(border is not null && header.IsAncestorOf(border), "记录Header包含原本地化caption的独立Border");
            Hit44(border!); var bounds = Bounds(border!); var hit = Window.Shell.InputHitTest(new Point(bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 2)) as DependencyObject;
            var belongs = false;
            for (var item = hit; item is not null; item = item is Visual ? VisualTreeHelper.GetParent(item) : LogicalTreeHelper.GetParent(item))
            {
                if (item is Button) break;
                if (ReferenceEquals(item, border) || ReferenceEquals(item, recordTab)) { belongs = true; break; }
            }
            Product(belongs, "记录caption中心实际命中归caption/Tab，不是Header取消按钮");
            Observe("record-caption-hit", new { bounds = R(bounds), actualHit = hit?.GetType().Name, belongs });
        }
        internal async Task ReadTable(DataGrid grid, string[] paths)
        {
            await Reveal(grid); var columns = grid.Columns.OfType<DataGridTextColumn>().ToDictionary(ColumnPath);
            Product(paths.All(columns.ContainsKey) && columns.Count == paths.Length, "原全部数据列保留：" + string.Join(",", paths));
            var source = grid.Items.Cast<object>().Where(item => item != CollectionView.NewItemPlaceholder).ToArray();
            Require(source.Length >= 24, "多行首/中/末真实滚动样本已加载");
            foreach (var index in new[] { 0, source.Length / 2, source.Length - 1 }) foreach (var path in paths)
            {
                if (!columns.TryGetValue(path, out var column)) continue;
                grid.ScrollIntoView(source[index], column); await Settle();
                var content = column.GetCellContent(source[index]);
                Require(content is not null, "实际原列虚拟化内容已实现：" + path);
                var text = content as TextBlock ?? VisualDescendants<TextBlock>(content!).FirstOrDefault();
                Require(text is not null, "原列实际 TextBlock：" + path);
                var expected = source[index].GetType().GetProperty(path)!.GetValue(source[index])?.ToString() ?? "";
                var visible = Bounds(text!); visible.Intersect(ReadingViewport(grid));
                Product(text!.Text == expected && visible.Width >= Math.Min(36, text.ActualWidth) && visible.Height >= 10, $"首中末横向阅读原字段 {index}/{path}");
                var fullWidth = Measure(text).WidthIncludingTrailingWhitespace <= visible.Width + .75;
                var editor = path == "Value" && !grid.IsReadOnly;
                var resultAlternative = path == "Result";
                var tipOwner = text.ToolTip?.ToString() == expected ? (FrameworkElement)text : Parent<DataGridCell>(text);
                var tooltip = !fullWidth && !editor && !resultAlternative && tipOwner?.ToolTip?.ToString() == expected && await ReadTooltip(tipOwner, expected);
                Product(fullWidth || editor || resultAlternative || tooltip, $"长字段有真实完整阅读路径 {index}/{path}");
                Observe("table-field", new { path, index, expected, actual = text.Text, visible = R(visible), desiredWidth = Measure(text).WidthIncludingTrailingWhitespace,
                    fullWidth, editor, resultAlternative, tooltip, horizontal = VisualDescendants<ScrollViewer>(grid).First().HorizontalOffset });
            }
        }
        private async Task<bool> ReadTooltip(FrameworkElement owner, string expected)
        {
            var original = owner.ToolTip; var tip = original as System.Windows.Controls.ToolTip ?? new System.Windows.Controls.ToolTip { Content = original, PlacementTarget = owner };
            var wasOpen = tip.IsOpen;
            try
            {
                if (original is not System.Windows.Controls.ToolTip) owner.ToolTip = tip;
                tip.IsOpen = true; await Settle();
                var text = VisualDescendants<TextBlock>(tip).FirstOrDefault(item => item.Text == expected);
                var readable = text is not null && text.IsVisible && tip.IsOpen && tip.IsVisible && text.TextWrapping == TextWrapping.Wrap
                    && text.ActualWidth > 0 && tip.ActualWidth <= 361;
                if (text is not null)
                {
                    var measured = Measure(text); measured.MaxTextWidth = Math.Max(1, text.ActualWidth);
                    readable &= text.ActualHeight + 1 >= measured.Height;
                    Observe("actual-tooltip", new { expected, tip.IsOpen, tip.IsVisible, tip.ActualWidth, tip.ActualHeight, tip.MaxWidth,
                        text.Text, text.TextWrapping, textWidth = text.ActualWidth, textHeight = text.ActualHeight, measured.Height });
                }
                return readable;
            }
            finally { tip.IsOpen = wasOpen; owner.ToolTip = original; }
        }
        internal async Task ReadValueEditors()
        {
            foreach (var index in new[] { 0, Vm.Parameters.Count / 2, Vm.Parameters.Count - 1 })
                await Edit(Vm.Parameters[index], null);
        }
        private async Task<bool> Edit(ParameterEditor row, string? value)
        {
            await Reveal(parameters); var column = parameters.Columns.OfType<DataGridTextColumn>().Single(item => ColumnPath(item) == "Value");
            parameters.ScrollIntoView(row, column); await Settle(); parameters.SelectedItem = row; parameters.CurrentCell = new(row, column);
            var began = parameters.BeginEdit(); await Settle(); Product(began, "原 Value 单元格真实 BeginEdit"); if (!began) return false;
            var content = column.GetCellContent(row); var editor = content as TextBox ?? (content is null ? null : VisualDescendants<TextBox>(content).FirstOrDefault());
            Require(editor is not null, "原 DataGrid 实现真实 Value TextBox 编辑器");
            Hit44(editor!); var before = row.Value;
            if (value is not null) { editor!.SetCurrentValue(TextBox.TextProperty, value); editor.GetBindingExpression(TextBox.TextProperty)!.UpdateSource(); }
            var expected = value ?? before; Product(editor!.Text == expected && row.Value == expected, "真实 TextBox 绑定值全文准确，没有直接写 ParameterEditor");
            editor.ScrollToHorizontalOffset(0); await Settle(); var home = editor.HorizontalOffset; editor.ScrollToHorizontalOffset(editor.ExtentWidth); await Settle();
            var last = editor.Text.Length == 0 ? Rect.Empty : editor.GetRectFromCharacterIndex(editor.Text.Length - 1);
            var lastInShell = last.IsEmpty ? Rect.Empty : editor.TransformToAncestor(Window.Shell).TransformBounds(last); var clip = ReadingViewport(editor);
            Product(editor.Text.Length == 0 || !lastInShell.IsEmpty && lastInShell.IntersectsWith(clip), "真实 Value 编辑器末尾字符可见而非仅有完整字符串");
            var cellCommitted = parameters.CommitEdit(DataGridEditingUnit.Cell, true); var rowCommitted = parameters.CommitEdit(DataGridEditingUnit.Row, true); await Settle();
            Product(cellCommitted && rowCommitted && row.Value == expected, "原 DataGrid Cell/Row CommitEdit 提交值");
            Observe("actual-value-editor", new { row.Key, before, expected, row.Value, began, cellCommitted, rowCommitted, home, end = editor.HorizontalOffset, last = R(lastInShell), viewport = R(clip) });
            return cellCommitted && rowCommitted && row.Value == expected;
        }
        internal async Task ReadCompleteResult()
        {
            Require(presentationJob is not null, "独立结果全文阅读沿当前真实选中呈现行");
            var body = string.Join("\n", Enumerable.Range(0, 220).Select(index => $"LINE-{index:D3} " + new string('R', 60)));
            const string tail = "\nPRESENTATION-ONLY-TAIL";
            resultText = LongResult + body.PadRight(16384 - LongResult.Length - tail.Length, 'R') + tail;
            presentationJob!.Update(presentationJob.Status with { Result = resultText }); await Settle();
            Product(System.Text.Encoding.UTF8.GetByteCount(resultText) == 16384 && ReferenceEquals(Vm.SelectedScriptJob, presentationJob), "精确16KiB ASCII结果仍绑定原选中JobId而非替代控件");
            await ReadResult();
            if (result is TextBox box)
            {
                box.ScrollToHome(); await Settle(); var first = box.GetRectFromCharacterIndex(0);
                Product(!first.IsEmpty && box.TransformToAncestor(Window.Shell).TransformBounds(first).IntersectsWith(ReadingViewport(box)), "结果首字符真实可见");
                box.ScrollToEnd(); await Settle(); var last = box.GetRectFromCharacterIndex(box.Text.Length - 1);
                Product(!last.IsEmpty && box.TransformToAncestor(Window.Shell).TransformBounds(last).IntersectsWith(ReadingViewport(box)), "结果TAIL末字符真实可见");
                Product(box.IsReadOnly, "结果正文为原绑定只读可滚动控件");
            }
            else
            {
                var text = (TextBlock)result; var scroll = Parent<ScrollViewer>(text)!;
                scroll.ScrollToHome(); await Settle(); Product(CharacterVisible(text, false), "结果首字符真实可见");
                scroll.ScrollToEnd(); await Settle(); Product(CharacterVisible(text, true), "结果TAIL末字符真实可见");
            }
        }
        internal async Task ReadLogTail()
        {
            await Reveal(logs);
            foreach (var index in new[] { 0, 250, 499 })
            {
                logs.ScrollIntoView(Vm.ScriptLogs[index]); await Settle(); var scroll = VisualDescendants<ScrollViewer>(logs).First(); scroll.ScrollToRightEnd(); await Settle();
                var item = (ListBoxItem?)logs.ItemContainerGenerator.ContainerFromIndex(index); Require(item is not null, "原日志首中末行可实现");
                var text = VisualDescendants<TextBlock>(item!).FirstOrDefault(block => block.Text == Vm.ScriptLogs[index]);
                Product(text is not null && CharacterVisible(text, true), "长日志TAIL末字符真实进入裁剪后视口：" + index);
            }
        }
        internal async Task SwitchResultSelection()
        {
            Require(presentationJob is not null && Vm.ScriptJobs.Count >= 3, "两个不同JobId结果切换使用既有原行");
            var rows = Vm.ScriptJobs.Skip(1).Take(2).ToArray(); var statuses = rows.Select(row => row.Status).ToArray(); var errors = new List<Exception>();
            try
            {
                foreach (var row in rows)
                {
                    var expected = "不同任务结果 " + row.JobId + "\nRESULT-TAIL-" + row.JobId.ToString("N");
                    row.Update(row.Status with { Result = expected }); await Reveal(jobs); jobs.ScrollIntoView(row); jobs.SelectedItem = row; await Settle();
                    Product(ReferenceEquals(Vm.SelectedScriptJob, row) && Vm.SelectedScriptJob?.JobId == row.JobId, "原任务表TwoWay选中对应不同JobId");
                    await Reveal(result); var actual = result is TextBox box ? box.Text : ((TextBlock)result).Text;
                    Product(actual == expected && ReferenceEquals(result.DataContext, Vm) && result.IsVisible, "同一原结果控件随不同JobId显示准确不同全文");
                    Observe("different-job-result", new { row.JobId, expected, actual, viewport = R(ReadingViewport(result)), selected = Vm.SelectedScriptJob?.JobId });
                }
            }
            catch (Exception error) { errors.Add(error); }
            finally
            {
                try
                {
                    for (var index = 0; index < rows.Length; index++) rows[index].Update(statuses[index]);
                    await Reveal(jobs); jobs.SelectedItem = presentationJob; await Settle(); await Reveal(result);
                    var restored = result is TextBox box ? box.Text : ((TextBlock)result).Text;
                    Product(ReferenceEquals(Vm.SelectedScriptJob, presentationJob) && restored == resultText, "结果切换专项恢复原presentationJob及精确16KiB正文");
                }
                catch (Exception error) { errors.Add(error); }
            }
            if (errors.Count > 0) throw new AggregateException("不同任务结果绑定/恢复失败", errors);
        }
        private bool CharacterVisible(TextBlock text, bool last)
        {
            if (!text.IsArrangeValid || !text.IsMeasureValid) return false;
            var pointer = last ? text.ContentEnd.GetPositionAtOffset(-1, LogicalDirection.Backward) : text.ContentStart.GetPositionAtOffset(1, LogicalDirection.Forward);
            var rect = pointer?.GetCharacterRect(last ? LogicalDirection.Backward : LogicalDirection.Forward) ?? Rect.Empty;
            var bounds = rect.IsEmpty ? Rect.Empty : text.TransformToAncestor(Window.Shell).TransformBounds(rect); var clip = Clip(text);
            Observe("actual-character", new { last, textLength = text.Text.Length, bounds = R(bounds), clip = R(clip) });
            return !bounds.IsEmpty && bounds.Height >= 1 && bounds.IntersectsWith(clip);
        }
        private static ScriptPackage EditablePackage() => new()
        {
            Manifest = new() { Id = "real-control-edit", Name = "真实控件参数提交", Version = "1.0" },
            Parameters = [new() { Key = "app", Type = "string", Value = JsonSerializer.SerializeToElement("com.matrix.demo") }],
            ExecutionContract = new() { Version = 1, Bindings = [new() { StepIndex = 0, Field = "value", ParameterKey = "app" }] },
            Steps = [new() { Action = "launchapp", Value = "com.matrix.demo" }]
        };
        internal async Task EditAndRun()
        {
            await Import(EditablePackage()); var edited = JsonSerializer.Serialize("com.android.settings");
            if (!await Edit(Vm.Parameters.Single(), edited)) return;
            await Invoke(Tool("运行所选")); var job = Runtime.Scripts.GetJobs().Single();
            await Track(terminals.GetOrAdd(job.JobId, _ => new(TaskCreationOptions.RunContinuationsAsynchronously)).Task); await Settle();
            var a = await TrackDump("DEMO-001"); var b = await TrackDump("DEMO-002");
            Product(job.Target.Serial == "DEMO-001" && Runtime.Scripts.GetJob(job.JobId)!.State == ScriptRunState.Completed && a.PackageName == "com.android.settings" && b.PackageName == "com.matrix.demo", "实际编辑+原Run按钮经原Scheduler只改变A真实Demo画面");
            var stored = Runtime.Metadata.GetAll<ScriptDeviceParameters>("ScriptDeviceConfiguration");
            Product(stored.Count == 1 && stored.Single().Values["app"].GetString() == "com.android.settings", "真实已接纳任务保存实际控件编辑值");
            Observe("actual-edit-effect", new { job.JobId, a.PackageName, bPackage = b.PackageName, jobs = Runtime.Scripts.GetJobs().Count, metadata = stored.Count });
        }
        private async Task<AndroidNode> TrackDump(string serial)
        { var task = Backend.DumpAsync(serial); await Track(task); return await task; }
        internal async Task InvalidEdit()
        {
            await Import(EditablePackage()); if (!await Edit(Vm.Parameters.Single(), "{")) return;
            var calls = Backend.Calls.ToArray(); var states = Vm.Devices.Select(row => row.ScriptState).ToArray(); var logsBefore = Vm.ScriptLogs.ToArray();
            await Invoke(Tool("运行所选"));
            Product(Vm.Error.Length > 0 && Runtime.Scripts.GetJobs().Count == 0 && Vm.ScriptJobs.Count == 0, "实际非法JSON编辑在接纳任务前拒绝并显示原错误");
            Product(Backend.Calls.ToArray().SequenceEqual(calls) && Runtime.Metadata.GetAll<ScriptDeviceParameters>("ScriptDeviceConfiguration").Count == 0
                && Vm.ScriptLogs.SequenceEqual(logsBefore) && Vm.Devices.Select(row => row.ScriptState).SequenceEqual(states), "非法编辑零后端动作/metadata/任务/运行态/日志副作用");
        }
        internal async Task CancelSelection()
        {
            Backend.BlockSerial("DEMO-001", 1); Backend.BlockSerial("DEMO-002", 1);
            await Import(EditablePackage()); if (!await Edit(Vm.Parameters.Single(), JsonSerializer.Serialize("com.android.settings"))) return;
            A.IsSelected = B.IsSelected = true; Vm.UpdateSelection(); await Invoke(Tool("运行所选"));
            await Track(Task.WhenAll(Backend.EnteredSerial("DEMO-001", 1), Backend.EnteredSerial("DEMO-002", 1))); await Settle();
            var actual = Runtime.Scripts.GetJobs(); var a = actual.Single(job => job.Target.Serial == "DEMO-001"); var b = actual.Single(job => job.Target.Serial == "DEMO-002");
            A.IsSelected = false; B.IsSelected = true; Vm.CurrentDevice = B; Vm.UpdateSelection();
            await Reveal(jobs); jobs.SelectedItem = Vm.ScriptJobs.Single(row => row.JobId == a.JobId); await Settle();
            Product(Vm.SelectedScriptJob?.JobId == a.JobId, "原任务表SelectedItem TwoWay选择确切A JobId");
            var sections = VisualDescendants<TabControl>(page).Single();
            var recordTab = sections.Items.Cast<TabItem>().SingleOrDefault(tab => AutomationProperties.GetName(tab) == "运行记录");
            var cancel = Tool("取消此任务"); var headerCancel = recordTab?.Header is FrameworkElement header && header.IsAncestorOf(cancel);
            if (headerCancel) await Reveal(parameters);
            var selectedPage = sections.SelectedItem;
            Hit44(cancel); await Invoke(cancel);
            if (headerCancel)
            {
                await Track(terminals.GetOrAdd(a.JobId, _ => new(TaskCreationOptions.RunContinuationsAsynchronously)).Task); await Settle();
                Product(ReferenceEquals(sections.SelectedItem, selectedPage) && parameters.IsVisible && ReferenceEquals(Vm.CurrentDevice, B) && !A.IsSelected && B.IsSelected
                    && Vm.SelectedScriptJob?.JobId == a.JobId && Runtime.Scripts.GetJob(a.JobId)!.State == ScriptRunState.Cancelled
                    && Runtime.Scripts.GetJob(b.JobId)!.State == ScriptRunState.Running && Backend.Active == 1,
                    "参数页一次Header取消只停A，B仍在途且不误切视图/当前设备/目标/所选JobId");
                RecordCaptionHit(recordTab!);
            }
            else Observe("header-cancel-contract", new { status = "NOT-APPLICABLE", reason = "原基线取消按钮尚未迁入具AutomationName的记录Header，原取消路径照测" });
            Backend.ReleaseSerial("DEMO-002", 1);
            await Track(Task.WhenAll(terminals.GetOrAdd(a.JobId, _ => new(TaskCreationOptions.RunContinuationsAsynchronously)).Task, terminals.GetOrAdd(b.JobId, _ => new(TaskCreationOptions.RunContinuationsAsynchronously)).Task)); await Settle();
            var nodeA = await TrackDump("DEMO-001"); var nodeB = await TrackDump("DEMO-002");
            Product(Runtime.Scripts.GetJob(a.JobId)!.State == ScriptRunState.Cancelled && Runtime.Scripts.GetJob(b.JobId)!.State == ScriptRunState.Completed
                && nodeA.PackageName == "com.matrix.demo" && nodeB.PackageName == "com.android.settings", "原取消按钮取消所选A任务，改勾选/当前设备后B继续真实执行");
            Product(Vm.SelectedScriptJob?.JobId == a.JobId && Vm.ScriptLogContext.Contains(a.JobId.ToString(), StringComparison.Ordinal), "任务取消后日志来源仍是选中A而非当前B");
            if (headerCancel)
            {
                Product(!cancel.IsEnabled && recordTab!.IsEnabled, "任务已终态时仅取消按钮disabled，记录Tab仍可选");
                var parentPeer = UIElementAutomationPeer.CreatePeerForElement(sections) as TabControlAutomationPeer;
                var peer = parentPeer?.GetChildren()?.OfType<TabItemAutomationPeer>().SingleOrDefault(item => ReferenceEquals(item.Item, recordTab));
                Require(peer?.GetPattern(PatternInterface.SelectionItem) is ISelectionItemProvider, "原实际TabControl提供对应原TabItem的SelectionItem provider");
                ((ISelectionItemProvider)peer!.GetPattern(PatternInterface.SelectionItem)).Select(); await Settle();
                Product(ReferenceEquals(sections.SelectedItem, recordTab) && jobs.IsVisible && Vm.SelectedScriptJob?.JobId == a.JobId
                    && ReferenceEquals(Vm.CurrentDevice, B) && !A.IsSelected && B.IsSelected, "disabled取消不阻止记录caption自身peer逻辑选页，业务目标/JobId不变");
                RecordCaptionHit(recordTab!);
            }
        }
        internal async Task ResizeState()
        {
            await LoadPresentation(); if (!await Edit(Vm.Parameters[0], JsonSerializer.Serialize("resize-committed-value"))) return;
            var originals = Vm.Parameters.ToArray(); var values = originals.Select(row => row.Value).ToArray(); var selected = Vm.SelectedScriptJob;
            var originalJobs = Vm.ScriptJobs.ToArray(); var originalLogs = Vm.ScriptLogs.ToArray(); var targets = Vm.Targets().ToArray(); var calls = Backend.Calls.ToArray();
            var main = Vm.MainDevice; var summary = Vm.PackageSummary; var logContext = Vm.ScriptLogContext;
            foreach (var size in new[] { (W: 390, H: 640), (W: 390, H: 844), (W: 768, H: 1024), (W: 1000, H: 700), (W: 1440, H: 900), (W: 1920, H: 1080) })
            {
                Window.Width = size.W; Window.Height = size.H; await Settle(); Product(Math.Abs(Window.ActualWidth - size.W) < .6 && Math.Abs(Window.ActualHeight - size.H) < .6, "resize实际六尺寸不clamp");
                foreach (var control in new FrameworkElement[] { parameters, jobs, logs, result })
                {
                    await Reveal(control);
                    Product(ReferenceEquals(((TabItem)Window.tabs.SelectedItem).Content, page) && control.IsVisible && Window.Shell.IsAncestorOf(control)
                        && ReferenceEquals(parameters.ItemsSource, Vm.Parameters) && ReferenceEquals(jobs.ItemsSource, Vm.ScriptJobs) && ReferenceEquals(logs.ItemsSource, Vm.ScriptLogs)
                        && Vm.Parameters.SequenceEqual(originals) && Vm.Parameters.Select(row => row.Value).SequenceEqual(values) && Vm.ScriptJobs.SequenceEqual(originalJobs)
                        && Vm.ScriptLogs.SequenceEqual(originalLogs) && ReferenceEquals(Vm.SelectedScriptJob, selected) && Vm.Targets().SequenceEqual(targets)
                        && ReferenceEquals(Vm.CurrentDevice, A) && ReferenceEquals(Vm.MainDevice, main) && Vm.PackageSummary == summary && Vm.ScriptLogContext == logContext
                        && Backend.Calls.ToArray().SequenceEqual(calls), "resize/切视图保留原控件集合、提交值、目标、JobId和日志且零后端调用");
                }
            }
        }
        internal async Task RealLogTail()
        {
            foreach (var serial in new[] { "DEMO-001", "DEMO-002" }) { Backend.BlockSerial(serial, 1); Backend.BlockSerial(serial, 2); }
            await Import(new ScriptPackage { Manifest = new() { Id = "real-500-tail", Name = "真实500日志滑窗", Version = "1.0" },
                Steps = Enumerable.Range(0, 575).Select(_ => new ScriptStep { Action = "wait", DurationMs = 0 }).Concat([new ScriptStep { Action = "launchapp", Value = "com.android.settings" }, new ScriptStep { Action = "launchapp", Value = "com.matrix.demo" }]).ToList() });
            A.IsSelected = B.IsSelected = true; Vm.UpdateSelection(); await Invoke(Tool("运行所选"));
            await Track(Task.WhenAll(Backend.EnteredSerial("DEMO-001", 1), Backend.EnteredSerial("DEMO-002", 1))); await Settle();
            var a = Runtime.Scripts.GetJobs().Single(job => job.Target.Serial == "DEMO-001"); var b = Runtime.Scripts.GetJobs().Single(job => job.Target.Serial == "DEMO-002");
            await Reveal(jobs); jobs.SelectedItem = Vm.ScriptJobs.Single(row => row.JobId == b.JobId); await Settle(); await Reveal(logs);
            var before = Vm.ScriptLogs.ToArray(); Product(Vm.SelectedScriptJob?.JobId == b.JobId && before.Length == 500, "真实575步骤产生500尾窗且任务表选中B");
            Backend.ReleaseSerial("DEMO-001", 1); await Track(Backend.EnteredSerial("DEMO-001", 2)); await Settle();
            Product(Vm.SelectedScriptJob?.JobId == b.JobId && Vm.ScriptLogs.SequenceEqual(before), "切B后A的新真实日志不会串入B");
            Backend.ReleaseSerial("DEMO-002", 1); await Track(Backend.EnteredSerial("DEMO-002", 2)); await Settle();
            var expected = Runtime.Scripts.GetJobLogs(b.JobId);
            Product(expected.Count == 500 && Vm.ScriptLogs.Count == 500 && Vm.ScriptLogs.Zip(expected).All(pair => pair.First.EndsWith("  " + pair.Second.Message, StringComparison.Ordinal)
                && pair.First.StartsWith(pair.Second.Timestamp.ToString("HH:mm:ss"), StringComparison.Ordinal)), "Count不变时真实500滑窗完整尾部文本与Scheduler逐条一致");
            Observe("real-log-window", new { aJob = a.JobId, bJob = b.JobId, before = before.Length, after = Vm.ScriptLogs.Count, beforeFirst = before.First(), actualFirst = Vm.ScriptLogs.First(), actualLast = Vm.ScriptLogs.Last() });
            Backend.ReleaseAll(); await Track(Task.WhenAll(terminals.GetOrAdd(a.JobId, _ => new(TaskCreationOptions.RunContinuationsAsynchronously)).Task, terminals.GetOrAdd(b.JobId, _ => new(TaskCreationOptions.RunContinuationsAsynchronously)).Task)); await Settle();
        }
        internal async Task ReadLogs()
        {
            await Reveal(logs); Require(Vm.ScriptLogs.Count == 500, "500条明确呈现样本");
            foreach (var index in new[] { 0, 250, 499 })
            {
                logs.ScrollIntoView(Vm.ScriptLogs[index]); await Settle();
                var item = (ListBoxItem?)logs.ItemContainerGenerator.ContainerFromIndex(index); Require(item is not null, "原日志行已实现");
                var visible = Bounds(item!); visible.Intersect(ReadingViewport(logs));
                Product(visible.Height >= 12 && visible.Width >= 100, "日志首中末进入真实可见视口：" + index);
                var scroll = VisualDescendants<ScrollViewer>(logs).First(); scroll.ScrollToLeftEnd(); await Settle(); var left = scroll.HorizontalOffset;
                scroll.ScrollToRightEnd(); await Settle();
                var text = VisualDescendants<TextBlock>(item!).FirstOrDefault(block => block.Text == Vm.ScriptLogs[index]);
                Product(text is not null && (Measure(text).WidthIncludingTrailingWhitespace <= ReadingViewport(logs).Width + .75 || scroll.HorizontalOffset > left), "长日志全文可横向阅读：" + index);
                Observe("log-read", new { index, actual = text?.Text, visible = R(visible), left, right = scroll.HorizontalOffset });
            }
        }
        internal async Task ReadResult()
        {
            await Reveal(result);
            var actual = result is TextBox box ? box.Text : ((TextBlock)result).Text;
            Product(actual == resultText && actual.StartsWith(LongResult, StringComparison.Ordinal) && actual.EndsWith("PRESENTATION-ONLY-TAIL", StringComparison.Ordinal), "16KiB级结果原绑定首尾全文保留");
            var scroll = result is TextBox ? VisualDescendants<ScrollViewer>(result).FirstOrDefault() : Parent<ScrollViewer>(result);
            Require(scroll is not null, "实际结果滚动容器存在");
            scroll!.ScrollToHome(); await Settle(); var top = scroll.VerticalOffset;
            scroll.ScrollToEnd(); await Settle();
            Product(scroll.VerticalOffset > top && ReadingViewport(result).Height >= 48 && ReadingViewport(result).Width >= 100, "结果有独立可读视口并可实际滚到末尾");
            Observe("result-read", new { characters = actual.Length, top, bottom = scroll.VerticalOffset, viewport = R(ReadingViewport(result)), maxHeight = N(scroll.MaxHeight) });
        }
        internal void PresentationContext()
        {
            Product(Vm.Parameters.Cast<object>().SequenceEqual(parameterRows) && Vm.ScriptJobs.Cast<object>().SequenceEqual(jobRows)
                && Vm.ScriptLogs.SequenceEqual(presentationLogs) && ReferenceEquals(Vm.SelectedScriptJob, presentationJob)
                && A.IsSelected && B.IsSelected && ReferenceEquals(Vm.CurrentDevice, A), "切视图和滚动保留原集合成员、目标、当前设备与任务");
            Product(Backend.Calls.ToArray().SequenceEqual(callsBeforeLayout) && Runtime.Scripts.GetJobs().Count == 0, "纯布局与阅读没有后端请求或真实任务");
        }
        internal async Task LiveLogReading()
        {
            Backend.Block(1); Backend.Block(2);
            await Import(new ScriptPackage { Manifest = new() { Id = "real-live-log", Name = "真实日志阅读锚点", Version = "1.0" },
                Steps = Enumerable.Range(0, 36).Select(_ => new ScriptStep { Action = "wait", DurationMs = 0 }).Concat([new ScriptStep { Action = "launchapp", Value = "com.android.settings" }, new ScriptStep { Action = "launchapp", Value = "com.matrix.demo" }]).ToList() });
            await Invoke(Tool("运行所选")); await Track(Backend.Entered(1)); await Settle();
            var job = Runtime.Scripts.GetJobs().Single();
            Require(Vm.SelectedScriptJob?.JobId == job.JobId && Vm.ScriptLogs.Count >= 20, "真实 Scheduler 已接纳且日志在第一 TCS 门处稳定");
            await Reveal(logs); var scroll = VisualDescendants<ScrollViewer>(logs).First();
            var selected = Vm.ScriptLogs[Vm.ScriptLogs.Count / 3]; logs.SelectedItem = selected; logs.ScrollIntoView(selected); await Settle();
            var anchor = Anchor(); var selectedIndex = logs.SelectedIndex; var beforeOffset = scroll.VerticalOffset; var beforeCount = Vm.ScriptLogs.Count;
            var reset = 0; var added = 0;
            void Changed(object? sender, NotifyCollectionChangedEventArgs e) { if (e.Action == NotifyCollectionChangedAction.Reset) reset++; if (e.Action == NotifyCollectionChangedAction.Add) added += e.NewItems?.Count ?? 0; }
            Vm.ScriptLogs.CollectionChanged += Changed;
            try
            {
                Backend.Release(1); await Track(Backend.Entered(2)); await Settle(); var afterAnchor = Anchor();
                Observe("actual-live-log-before-after", new { job.JobId, beforeCount, afterCount = Vm.ScriptLogs.Count, selected, selectedIndex, actualSelection = logs.SelectedItem?.ToString(),
                    actualIndex = logs.SelectedIndex, beforeAnchor = new { anchor.Text, anchor.Y }, afterAnchor = new { afterAnchor.Text, afterAnchor.Y }, beforeOffset, afterOffset = scroll.VerticalOffset, reset, added,
                    schedulerCount = Runtime.Scripts.GetJobLogs(job.JobId).Count, actualJob = Vm.SelectedScriptJob?.JobId, backendActive = Backend.Active });
                Product(Vm.ScriptLogs.Count > beforeCount && Vm.SelectedScriptJob?.JobId == job.JobId && Backend.Active == 1, "同一实际运行任务在第二 TCS 门新增日志");
                Product(Equals(logs.SelectedItem, selected) && logs.SelectedIndex == selectedIndex, "活动日志追加保留用户选中原项");
                Product(anchor.Text == afterAnchor.Text && Math.Abs(anchor.Y - afterAnchor.Y) < .75 && Math.Abs(beforeOffset - scroll.VerticalOffset) < .01, "活动日志追加保留非末尾阅读锚点与滚动位置");
                Save("live-log-after");
            }
            finally { Vm.ScriptLogs.CollectionChanged -= Changed; Backend.Release(1); Backend.Release(2); }
            await Track(terminals.GetOrAdd(job.JobId, _ => new(TaskCreationOptions.RunContinuationsAsynchronously)).Task); await Settle();
            Product(Runtime.Scripts.GetJob(job.JobId)!.State == ScriptRunState.Completed && Backend.Active == 0, "实际任务和两个门全部终态后才完成案例");
        }
        private (string Text, double Y) Anchor()
        {
            var viewport = ReadingViewport(logs);
            var item = VisualDescendants<ListBoxItem>(logs).Where(element => element.IsVisible && Bounds(element).Top >= viewport.Top - .5
                && Bounds(element).Bottom <= viewport.Bottom + .5).OrderBy(element => Bounds(element).Top).FirstOrDefault();
            Require(item is not null, "日志阅读锚点必须是实际完整可见原行"); return (item!.Content?.ToString() ?? "", Bounds(item).Top - viewport.Top);
        }
        internal void Save(string name)
        {
            var visual = Window.Shell; var dpi = VisualTreeHelper.GetDpi(visual); var pixelWidth = (int)Math.Ceiling(visual.ActualWidth * dpi.DpiScaleX); var pixelHeight = (int)Math.Ceiling(visual.ActualHeight * dpi.DpiScaleY);
            Require(pixelWidth > 0 && pixelHeight > 0, "自有 WPF 渲染尺寸有效");
            var bitmap = new RenderTargetBitmap(pixelWidth, pixelHeight, dpi.PixelsPerInchX, dpi.PixelsPerInchY, PixelFormats.Pbgra32); bitmap.Render(visual);
            var pixels = new byte[pixelWidth * pixelHeight * 4]; bitmap.CopyPixels(pixels, pixelWidth * 4, 0);
            var colors = new HashSet<int>(); for (var index = 0; index < pixels.Length; index += 4 * 37) colors.Add(BitConverter.ToInt32(pixels, index));
            Product(colors.Count > 12, "自有 WPF 渲染非空白：" + name);
            var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap)); using var file = File.Create(Path.Combine(directory, name + ".png")); encoder.Save(file);
            Observe("managed-render", new { name, pixelWidth, pixelHeight, colors = colors.Count, noDesktopCapture = true });
        }
        internal async Task Cleanup()
        {
            if (disposed) throw new InvalidOperationException("重复清理"); disposed = true;
            Backend.ReleaseAll();
            if (Vm is not null) await Vm.ShutdownAsync().WaitAsync(TimeSpan.FromSeconds(15));
            else if (Runtime is not null) await Runtime.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(15));
            else await Backend.DisposeAsync();
            await Task.WhenAll(ownedTasks).WaitAsync(TimeSpan.FromSeconds(10));
            if (Runtime is not null)
            {
                Runtime.Scripts.JobChanged -= OnJob;
                var statuses = Runtime.Scripts.GetJobs(); TerminalJobs = statuses.Select(job => (object)new { job.JobId, job.State, job.Target.Serial }).ToArray();
                Require(statuses.All(job => job.State is ScriptRunState.Completed or ScriptRunState.Cancelled or ScriptRunState.Failed), "清理后所有已接纳任务终态");
            }
            if (Window is not null)
            {
                Window.Closing -= OnClose; LocalizationService.Current.LanguageChanged -= Window.OnLanguageChanged;
                Window.AbortStartup();
            }
            await dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle).Task.WaitAsync(TimeSpan.FromSeconds(5));
            Application.Current.MainWindow = null;
            Require(Backend.Disposed && Backend.Active == 0 && ownedTasks.All(task => task.IsCompleted) && Application.Current.Windows.Count == 0, "单 owner 有界清理：无活动后端、未终态任务或残留窗口");
        }
        private static T? Parent<T>(DependencyObject element) where T : DependencyObject
        { for (var item = VisualTreeHelper.GetParent(element); item is not null; item = VisualTreeHelper.GetParent(item)) if (item is T found) return found; return null; }
        private static string? TextPath(FrameworkElement element) => element is TextBox box ? BindingOperations.GetBinding(box, TextBox.TextProperty)?.Path.Path
            : element is TextBlock text ? BindingOperations.GetBinding(text, TextBlock.TextProperty)?.Path.Path : null;
        private static string ColumnPath(DataGridTextColumn column)
        {
            if (column.Binding is Binding binding) return binding.Path.Path;
            if (column.Binding is MultiBinding multi && multi.Bindings.Count == 2 && multi.Bindings[0] is Binding { Path.Path: "State" }
                && multi.Bindings[1] is Binding { Path.Path: nameof(LocalizationService.Language) }) return "State";
            throw new ScriptWorkspacePrecondition("原数据列绑定路径不受夹具支持：" + column.Binding.GetType().Name);
        }
        private static FormattedText Measure(TextBlock text) => new(text.Text, CultureInfo.CurrentCulture, text.FlowDirection,
            new Typeface(text.FontFamily, text.FontStyle, text.FontWeight, text.FontStretch), text.FontSize, text.Foreground,
            null, TextOptions.GetTextFormattingMode(text), VisualTreeHelper.GetDpi(text).PixelsPerDip);
        private static double? N(double value) => double.IsFinite(value) ? value : null;
        private static object R(Rect rect) => new { empty = rect.IsEmpty, x = N(rect.X), y = N(rect.Y), width = N(rect.Width), height = N(rect.Height) };
    }

    private sealed class ScriptWorkspaceBackend : IDeviceBackend, IIdentityPinnedDeviceBackend
    {
        private sealed class Gate
        {
            internal readonly TaskCompletionSource Entered = new(TaskCreationOptions.RunContinuationsAsynchronously);
            internal readonly TaskCompletionSource Released = new(TaskCreationOptions.RunContinuationsAsynchronously);
        }
        private readonly DemoDeviceBackend inner = new(); private readonly ConcurrentDictionary<int, Gate> gates = new();
        private readonly ConcurrentDictionary<string, Gate> serialGates = new(); private readonly ConcurrentDictionary<string, int> serialLaunches = new();
        internal readonly ConcurrentQueue<string> Calls = new(); internal int Active, Disposals; private int launch;
        internal bool Disposed => Disposals == 1;
        internal void Block(int ordinal) => gates[ordinal] = new();
        internal Task Entered(int ordinal) => gates[ordinal].Entered.Task;
        internal void Release(int ordinal) { if (gates.TryGetValue(ordinal, out var gate)) gate.Released.TrySetResult(); }
        internal void BlockSerial(string serial, int ordinal) => serialGates[serial + "#" + ordinal] = new();
        internal Task EnteredSerial(string serial, int ordinal) => serialGates[serial + "#" + ordinal].Entered.Task;
        internal void ReleaseSerial(string serial, int ordinal) { if (serialGates.TryGetValue(serial + "#" + ordinal, out var gate)) gate.Released.TrySetResult(); }
        internal void ReleaseAll() { foreach (var gate in gates.Values.Concat(serialGates.Values)) gate.Released.TrySetResult(); }
        public IIdentityPinnedDeviceCommands BindIdentity(DeviceCatalog catalog) => new Pinned(this, inner.BindIdentity(catalog));
        private sealed class Pinned(ScriptWorkspaceBackend owner, IIdentityPinnedDeviceCommands commands) : IIdentityPinnedDeviceCommands
        {
            public bool IsDemo => commands.IsDemo;
            public async Task<string> StartAppAsync(DeviceCommandTarget target, string packageName, CancellationToken ct = default)
            {
                var ordinal = Interlocked.Increment(ref owner.launch); var serialOrdinal = owner.serialLaunches.AddOrUpdate(target.Serial, 1, (_, previous) => previous + 1);
                owner.Calls.Enqueue("PinnedStart:" + target.Serial + ":" + packageName); Interlocked.Increment(ref owner.Active);
                try { if (owner.gates.TryGetValue(ordinal, out var gate) || owner.serialGates.TryGetValue(target.Serial + "#" + serialOrdinal, out gate)) { gate.Entered.TrySetResult(); await gate.Released.Task.WaitAsync(ct); } return await commands.StartAppAsync(target, packageName, ct); }
                finally { Interlocked.Decrement(ref owner.Active); }
            }
            public Task TapAsync(DeviceCommandTarget target, int x, int y, CancellationToken ct = default) { owner.Calls.Enqueue("PinnedTap:" + target.Serial); return commands.TapAsync(target, x, y, ct); }
            public Task SwipeAsync(DeviceCommandTarget target, int x1, int y1, int x2, int y2, int durationMs, CancellationToken ct = default) { owner.Calls.Enqueue("PinnedSwipe:" + target.Serial); return commands.SwipeAsync(target, x1, y1, x2, y2, durationMs, ct); }
        }
        private Task<T> Call<T>(string name, Func<Task<T>> action) { Calls.Enqueue(name); return action(); }
        private Task Call(string name, Func<Task> action) { Calls.Enqueue(name); return action(); }
        public Task<IReadOnlyList<DeviceInfo>> DiscoverAsync(CancellationToken ct = default) => Call("Discover", () => inner.DiscoverAsync(ct));
        public Task<string> ShellAsync(string serial, string command, bool useRoot = false, CancellationToken ct = default) => Call("Shell:" + serial, () => inner.ShellAsync(serial, command, useRoot, ct));
        public Task<byte[]> ScreenshotAsync(string serial, CancellationToken ct = default) => Call("Screenshot:" + serial, () => inner.ScreenshotAsync(serial, ct));
        public Task<AndroidNode> DumpAsync(string serial, CancellationToken ct = default) => Call("Dump:" + serial, () => inner.DumpAsync(serial, ct));
        public Task TapAsync(string serial, int x, int y, CancellationToken ct = default) => Call("Tap:" + serial, () => inner.TapAsync(serial, x, y, ct));
        public Task SwipeAsync(string serial, int x1, int y1, int x2, int y2, int durationMs, CancellationToken ct = default) => Call("Swipe:" + serial, () => inner.SwipeAsync(serial, x1, y1, x2, y2, durationMs, ct));
        public Task KeyEventAsync(string serial, int code, CancellationToken ct = default) => Call("Key:" + serial, () => inner.KeyEventAsync(serial, code, ct));
        public Task TextAsync(string serial, string text, CancellationToken ct = default) => Call("Text:" + serial, () => inner.TextAsync(serial, text, ct));
        public Task<string> StartAppAsync(string serial, string packageName, string? activity = null, CancellationToken ct = default) => Call("Start:" + serial, () => inner.StartAppAsync(serial, packageName, activity, ct));
        public Task<string> StopAppAsync(string serial, string packageName, CancellationToken ct = default) => Call("Stop:" + serial, () => inner.StopAppAsync(serial, packageName, ct));
        public Task<bool> ClearAppAsync(string serial, string packageName, CancellationToken ct = default) => Call("Clear:" + serial, () => inner.ClearAppAsync(serial, packageName, ct));
        public Task<IReadOnlyList<AppInfo>> ListAppsAsync(string serial, bool showSystem = false, bool needVersion = true, CancellationToken ct = default) => Call("List:" + serial, () => inner.ListAppsAsync(serial, showSystem, needVersion, ct));
        public Task InstallAsync(string serial, string localPath, CancellationToken ct = default) => Call("Install:" + serial, () => inner.InstallAsync(serial, localPath, ct));
        public Task UninstallAsync(string serial, string packageName, CancellationToken ct = default) => Call("Uninstall:" + serial, () => inner.UninstallAsync(serial, packageName, ct));
        public Task PushAsync(string serial, string localPath, string remotePath, IProgress<double>? progress = null, CancellationToken ct = default) => Call("Push:" + serial, () => inner.PushAsync(serial, localPath, remotePath, progress, ct));
        public Task PullAsync(string serial, string remotePath, string localPath, IProgress<double>? progress = null, CancellationToken ct = default) => Call("Pull:" + serial, () => inner.PullAsync(serial, remotePath, localPath, progress, ct));
        public Task SetNodeTextAsync(string serial, NodeSelector selector, string text, CancellationToken ct = default) => Call("NodeText:" + serial, () => inner.SetNodeTextAsync(serial, selector, text, ct));
        public async ValueTask DisposeAsync() { Interlocked.Increment(ref Disposals); await inner.DisposeAsync(); }
    }
}
