using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Runtime.ExceptionServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Threading;
using MatrixDevice.Core;
using MatrixDevice.Features;
using MatrixDevice.Host;

namespace MatrixDevice.Desktop;

internal static class ScriptParameterEditingSmoke
{
    internal static async Task RunAsync(string dataDirectory, Dispatcher dispatcher)
    {
        var application = Application.Current;
        var directory = Path.Combine(dataDirectory, "script-parameter-editing");
        if (application is null || !ReferenceEquals(application.Dispatcher, dispatcher) || !dispatcher.CheckAccess() || Thread.CurrentThread.GetApartmentState() != ApartmentState.STA
            || !Path.IsPathFullyQualified(dataDirectory) || !Path.GetFullPath(dataDirectory).Contains(Path.DirectorySeparatorChar + "artifacts" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
            || Directory.Exists(dataDirectory) || File.Exists(dataDirectory) || application.Windows.Count != 0 || application.MainWindow is not null)
            throw new InvalidOperationException("参数编辑契约仅限 fresh artifacts 自有 Demo/STA VM；Application 必须没有窗口。");
        Directory.CreateDirectory(directory);
        var reports = new List<object>(); var failures = new List<Exception>(); var classes = new HashSet<string>(StringComparer.Ordinal);
        var names = new[] { "v1-edited-value-executes", "v1-wire-null-rejects-before-admission", "v1-invalid-json-edit-rejects", "legacy-edit-retains-literal-execution" };
        var entryWindowCount = application.Windows.Count; var entryMainWindow = application.MainWindow;
        Exception? failure = null; var windowsUnchanged = false;
        try
        {
            await Case(names[0], true, false, "\"com.android.settings\"", false, "com.android.settings");
            await Case(names[1], true, true, null, true, "com.matrix.demo");
            await Case(names[2], true, false, "{", true, "com.matrix.demo");
            await Case(names[3], false, false, "\"com.android.settings\"", false, "com.matrix.demo");
        }
        catch (Exception error) { failures.Add(error); classes.Add("FIXTURE-FAIL"); }
        finally
        {
            windowsUnchanged = application.Windows.Count == entryWindowCount && entryWindowCount == 0 && application.MainWindow is null && ReferenceEquals(entryMainWindow, application.MainWindow);
            if (!windowsUnchanged) { failures.Add(new InvalidOperationException("自有参数编辑 VM 创建了窗口或改变了 Application.MainWindow。")); classes.Add("FIXTURE-FAIL"); }
            if (failures.Count > 0) failure = new AggregateException("脚本参数编辑契约未全部通过", failures);
            try
            {
                File.WriteAllText(Path.Combine(directory, "summary.json"), JsonSerializer.Serialize(new
                {
                    status = failure is null ? "PASS-SCRIPT-PARAMETER-EDITING" : classes.Count == 1 ? classes.Single() : "MIXED-FAIL",
                    failureClasses = classes.OrderBy(value => value).ToArray(), plannedCaseCount = names.Length, caseCount = reports.Count,
                    reports, notRunCases = names.Skip(reports.Count).ToArray(), windowsUnchanged,
                    applicationEntry = new { windowCount = entryWindowCount, mainWindowIsNull = entryMainWindow is null },
                    applicationExit = new { windowCount = application.Windows.Count, mainWindowIsNull = application.MainWindow is null }, newWindowsShown = 0, nativeInputRequested = 0,
                    scope = "Independent in-process STA WorkspaceViewModel ImportScriptAsync / ParameterEditor.Value / StartScriptAsync / original ScriptScheduler, forwarding only to an owned DemoDeviceBackend. No Window, GUI click, guest, ADB, API or screenshot acceptance.",
                    completionEvidence = "Original StatusChanged terminal events plus returned public Tasks and every recorded Demo forwarding Task; no claim to access the private scheduler Run.Task.",
                    offlineGeometry = "NOT-RUN", offlineGeometryReason = "Independent VM-only mode never enters geometry or GUI acceptance", error = failure?.ToString()
                }, JsonDefaults.Options));
            }
            catch (Exception reportError) { Console.Error.WriteLine("SCRIPT-PARAMETER-EDITING-REPORT " + reportError); failure = Combine(failure, reportError); }
        }
        if (failure is not null) ExceptionDispatchInfo.Capture(failure).Throw();

        async Task Case(string name, bool v1, bool wireNull, string? edit, bool reject, string expectedApp)
        {
            var state = new ScriptParameterEditingState(Path.Combine(directory, "c" + (reports.Count + 1).ToString("00")), dispatcher);
            var caseClasses = new HashSet<string>(StringComparer.Ordinal); Exception? error = null; object? result = null; var cleaned = false; var initialized = false;
            try
            {
                await state.Initialize(); initialized = true;
                await state.Import(v1, wireNull);
                state.Product(state.Backend.Calls("import").Length == 0, "真实导入仅解析/编辑，不调用后端");
                var editor = state.Vm.Parameters.Single(); var importedValue = editor.Value;
                if (wireNull) state.Product(importedValue == "null", "真实 ZIP 的显式 value:null 在参数表模型中保留，不回退合法 default");
                if (edit is not null) editor.Value = edit;
                var before = state.Values(); state.Watch();
                await state.StartAndDrain(!reject);
                var after = state.Values(); var runCalls = state.Backend.Calls("execute");
                if (reject)
                {
                    state.Product(state.Vm.Error.StartsWith("脚本未启动：", StringComparison.Ordinal), "非法/显式 null 编辑显示启动前错误");
                    state.Product(state.MetadataText() == state.BeforeMetadata && state.Metadata().Count == 0, "拒绝不写 ScriptDeviceConfiguration 元数据");
                    state.Product(state.StatusEvents().Length == 0 && state.RowEvents().Length == 0 && state.LogEvents().Length == 0
                        && state.Vm.Devices.All(row => state.Runtime.Scripts.GetStatus(row.Serial).State == ScriptRunState.Idle)
                        && state.Vm.Devices.All(row => state.Runtime.Scripts.GetLogs(row.Serial).Count == 0), "拒绝无 Running、准备执行瞬态、调度日志或终态副作用");
                    state.Product(runCalls.Length == 0, "拒绝期间实际 Demo 后端调用为零（不含独立准备/读回/清理）");
                }
                else
                {
                    state.Product(state.Vm.Error.Length == 0 && state.StatusEvents().Select(item => item.State).SequenceEqual(new[] { ScriptRunState.Running, ScriptRunState.Completed }), "真实原调度器唯一接纳 A 并完成");
                    state.Product(runCalls.Length == 1 && runCalls[0].Operation == "StartApp" && runCalls[0].Serial == "DEMO-001" && runCalls[0].Subject == expectedApp, "实际后端只启动 A，使用规定的最终步骤值");
                    var saved = state.Metadata();
                    state.Product(saved.Count == 1 && saved[0].Serial == "DEMO-001" && saved[0].Values.TryGetValue("targetApp", out var value)
                        && value.ValueKind == JsonValueKind.String && value.GetString() == "com.android.settings", "所选 A 的页面编辑值仍保存，未选 B 无运行配置");
                }
                state.Backend.Phase = "readback";
                var a = await state.Finish(state.Backend.DumpAsync("DEMO-001", state.Lifetime.Token));
                var b = await state.Finish(state.Backend.DumpAsync("DEMO-002", state.Lifetime.Token));
                state.Product(a.PackageName == expectedApp && b.PackageName == "com.matrix.demo", "自有真实 Demo 节点读回对应执行结果且未选 B 不变");
                state.Product(ReferenceEquals(state.Vm.CurrentDevice, state.A) && state.Vm.MainDevice is null && state.A.IsSelected && !state.B.IsSelected
                    && state.Vm.Targets().SequenceEqual(new[] { state.A }) && !state.Vm.Busy && !state.Vm.Transferring, "编辑与执行不漂移所选 A、Current 或未指定 Main");
                result = new { v1, wireNull, importedValue, editedValue = editor.Value, reject, expectedApp, before, after, runCalls, actualAppA = a.PackageName, actualAppB = b.PackageName };
            }
            catch (Exception caught) { error = caught; caseClasses.Add(initialized ? "FIXTURE-FAIL" : "PRECONDITION-FAIL"); Console.Error.WriteLine("SCRIPT-PARAMETER-EDITING " + name + " " + caught); }
            finally
            {
                if (state.Products.Count > 0) { caseClasses.Add("PRODUCT-FAIL"); error = Combine(error, new AggregateException("原 VM 参数执行合同失败", state.Products)); }
                try { await state.Cleanup(); cleaned = true; }
                catch (Exception cleanup) { caseClasses.Add("CLEANUP-FAIL"); error = Combine(error, cleanup); Console.Error.WriteLine("SCRIPT-PARAMETER-EDITING-CLEANUP " + name + " " + cleanup); }
                reports.Add(new { name, status = caseClasses.Count == 0 ? "PASS" : caseClasses.Count == 1 ? caseClasses.Single() : "MIXED-FAIL", failureClasses = caseClasses.OrderBy(value => value).ToArray(),
                    initialized, cleaned, result, checks = state.Checks.ToArray(), products = state.Products.Select(item => item.Message).ToArray(), observations = state.Observations.ToArray(),
                    statuses = state.StatusEvents(), rowStates = state.RowEvents(), logs = state.LogEvents(), backend = state.Backend.Calls(), forwardingDrained = state.Backend.Drained,
                    backendDisposed = state.Backend.Disposed, tasks = state.TaskReport(), cleanupErrors = state.CleanupErrors.ToArray(), terminal = state.Terminal.Task.Status.ToString(), error = error?.ToString() });
            }
            foreach (var value in caseClasses) classes.Add(value);
            if (error is not null) failures.Add(new InvalidOperationException(name, error));
            if (!cleaned) throw new InvalidOperationException("自有 VM 清理失败，禁止继续接纳下一 case。", error);
        }
        static Exception Combine(Exception? prior, Exception next) => prior is null ? next : new AggregateException(prior, next);
    }

    private sealed class ScriptParameterEditingState(string directory, Dispatcher dispatcher)
    {
        internal readonly ScriptParameterEditingBackend Backend = new();
        internal readonly CancellationTokenSource Lifetime = new(TimeSpan.FromSeconds(20));
        internal readonly TaskCompletionSource<ScriptRunStatus> Terminal = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal readonly List<Exception> Products = []; internal readonly List<string> Checks = []; internal readonly List<object> Observations = [];
        internal readonly List<string> CleanupErrors = [];
        internal WorkspaceViewModel Vm = null!; internal MatrixRuntime Runtime = null!; internal string BeforeMetadata = "";
        private RollingLog? log; private CatalogRepository? metadata; private ScriptScheduler? scripts; private InputCoordinator? input; private ApiServiceManager? api;
        private readonly List<Task> tasks = []; private readonly object sync = new(); private readonly List<ScriptRunStatus> statuses = [];
        private readonly List<object> rowEvents = []; private readonly List<object> logEvents = []; private bool watching;
        internal DeviceRow A => Vm.Devices.Single(row => row.Serial == "DEMO-001"); internal DeviceRow B => Vm.Devices.Single(row => row.Serial == "DEMO-002");
        internal async Task Initialize()
        {
            if (Directory.Exists(directory) || File.Exists(directory)) throw new InvalidOperationException("自有 case 目录已存在。");
            Directory.CreateDirectory(directory);
            var ownLog = new RollingLog(Path.Combine(directory, "logs")); log = ownLog;
            var ownMetadata = new CatalogRepository(Path.Combine(directory, "db.sqlite3")); metadata = ownMetadata;
            var catalog = new DeviceCatalog(Backend); var automation = new AutomationService(Backend, catalog); var scheduler = new ScriptScheduler(Backend, catalog, true); scripts = scheduler;
            var coordinator = new InputCoordinator(catalog, Backend); input = coordinator; var services = new ApiServiceManager(catalog, Backend, automation, ownLog); api = services;
            var settings = new AppSettings { Language = "zh-CN", AutoStartApi = false, AutoConnect = false, SyncMouse = false, SyncKeyboard = false };
            Runtime = new MatrixRuntime { Backend = Backend, Devices = catalog, Automation = automation, Scripts = scheduler, Input = coordinator, Api = services,
                Metadata = ownMetadata, Log = ownLog, Settings = settings, FrozenConnectionSettings = settings, SettingsStore = new SettingsStore(Path.Combine(directory, "AppConfig.json")), IsDemo = true };
            bool ScriptBusy(string serial) => scheduler.GetStatus(serial).State is ScriptRunState.Running or ScriptRunState.Cancelling;
            services.DeviceSnapshotProvider = Runtime.GetDevices; services.ScriptWriteBusy = ScriptBusy; services.AnyScriptWriteBusy = () => catalog.Snapshot.Any(device => ScriptBusy(device.Serial));
            catalog.DeviceDisconnected += serial => scheduler.Stop(serial); scheduler.EntryAdded += (serial, entry) => ownLog.Write("script", entry.Message, serial);
            Vm = new WorkspaceViewModel(Runtime, directory, true); await Finish(Vm.RefreshDevicesAsync());
            if (!Vm.Demo || !Runtime.IsDemo || Backend.Inner.GetType() != typeof(DemoDeviceBackend) || !Backend.Inner.IsSimulation || Vm.Error.Length != 0
                || Vm.Devices.Count != 2 || Vm.Devices.Any(row => !row.Info.Online) || services.BasicRunning || services.AutomatorRunning || services.DevicePorts.Count != 0 || services.HasActiveRequests)
                throw new InvalidOperationException("一致自有 Demo VM/原目录/调度/后端且无 API 的前置未成立。");
            Observations.Add(new { stage = "owned-runtime", concreteBackend = Backend.Inner.GetType().FullName, Runtime.IsDemo, Vm.Demo,
                sameRecordingBackend = ReferenceEquals(Runtime.Backend, Backend), sameCatalog = ReferenceEquals(Runtime.Devices, catalog), sameScheduler = ReferenceEquals(Runtime.Scripts, scheduler),
                sameAutomation = ReferenceEquals(Runtime.Automation, automation), sameInput = ReferenceEquals(Runtime.Input, coordinator), sameApi = ReferenceEquals(Runtime.Api, services),
                settings.AutoStartApi, settings.AutoConnect, settings.SyncMouse, settings.SyncKeyboard, newWindowShown = false });
            Vm.CurrentDevice = A; A.IsSelected = true; B.IsSelected = false; Vm.UpdateSelection();
            if (!Vm.Targets().SequenceEqual(new[] { A }) || Vm.MainDevice is not null) throw new InvalidOperationException("原 VM 仅 A 目标前置未成立。");
            Backend.Phase = "baseline";
            var a = await Finish(Backend.DumpAsync(A.Serial, Lifetime.Token)); var b = await Finish(Backend.DumpAsync(B.Serial, Lifetime.Token));
            if (a.PackageName != "com.matrix.demo" || b.PackageName != "com.matrix.demo" || Metadata().Count != 0) throw new InvalidOperationException("fresh Demo/元数据前置未成立。");
        }
        internal async Task Import(bool v1, bool wireNull)
        {
            var script = new Dictionary<string, object?>
            {
                ["steps"] = new[] { new { action = "launchapp", value = "com.matrix.demo" } },
                ["parameters"] = new[] { new Dictionary<string, object?> { ["key"] = "targetApp", ["type"] = "string", ["value"] = wireNull ? null : "com.matrix.demo", ["defaultValue"] = "com.android.settings" } }
            };
            if (v1) script["executionContract"] = new { version = 1, bindings = new[] { new { stepIndex = 0, field = "value", parameterKey = "targetApp" } } };
            var payload = JsonSerializer.SerializeToUtf8Bytes(script); var path = Path.Combine(directory, "actual-import.jxscript");
            using (var file = File.Create(path))
            using (var archive = new ZipArchive(file, ZipArchiveMode.Create))
            {
                Write("manifest.json", JsonSerializer.SerializeToUtf8Bytes(new { id = "parameter-editing", name = "自有参数执行", version = "1.0" })); Write("script.json", payload);
                void Write(string name, byte[] bytes) { using var stream = archive.CreateEntry(name).Open(); stream.Write(bytes); }
            }
            using (var archive = ZipFile.OpenRead(path))
            using (var stream = archive.GetEntry("script.json")!.Open())
            using (var document = JsonDocument.Parse(stream))
            {
                var root = document.RootElement; var value = root.GetProperty("parameters")[0].GetProperty("value");
                if ((value.ValueKind == JsonValueKind.Null) != wireNull || root.TryGetProperty("executionContract", out _) != v1) throw new InvalidOperationException("真实 ZIP 构造的 null/契约不符合夹具输入。");
                Observations.Add(new { stage = "actual-zip-before-import", path, script = root.GetRawText(), v1, wireNull });
            }
            Backend.Phase = "import"; await Finish(Vm.ImportScriptAsync(path));
            if (Vm.Error.Length != 0 || Vm.Parameters.Count != 1 || Vm.Parameters[0].Key != "targetApp" || Vm.Busy) throw new InvalidOperationException("真实原 ImportScriptAsync 未成功：" + Vm.Error);
        }
        internal void Watch()
        {
            BeforeMetadata = MetadataText(); watching = true; Runtime.Scripts.StatusChanged += OnStatus; Runtime.Scripts.EntryAdded += OnLog;
            foreach (var row in Vm.Devices) row.PropertyChanged += OnRow;
        }
        internal async Task StartAndDrain(bool expectRun)
        {
            Backend.Phase = "execute"; await Finish(Vm.StartScriptAsync());
            var accepted = StatusEvents().Any(item => item.State == ScriptRunState.Running);
            if (expectRun) Product(accepted, "页面入口经原 StatusChanged 真实接纳运行，而非只返回 CompletedTask");
            if (accepted) await Finish(Terminal.Task);
            await Finish(dispatcher.InvokeAsync(() => { }, DispatcherPriority.ContextIdle).Task);
            if (!Backend.Drained) throw new InvalidOperationException("完成观测时仍有实际后端转发任务未终态。");
        }
        private void OnStatus(ScriptRunStatus value)
        {
            lock (sync) statuses.Add(value);
            if (value.Serial == "DEMO-001" && value.State is ScriptRunState.Completed or ScriptRunState.Cancelled or ScriptRunState.Failed) Terminal.TrySetResult(value);
        }
        private void OnLog(string serial, ScriptLogEntry value) { lock (sync) logEvents.Add(new { serial, value.Timestamp, value.Message }); }
        private void OnRow(object? sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(DeviceRow.ScriptState) && sender is DeviceRow row) lock (sync) rowEvents.Add(new { row.Serial, row.ScriptState });
        }
        internal ScriptRunStatus[] StatusEvents() { lock (sync) return statuses.ToArray(); }
        internal object[] LogEvents() { lock (sync) return logEvents.ToArray(); } internal object[] RowEvents() { lock (sync) return rowEvents.ToArray(); }
        internal IReadOnlyList<ScriptDeviceParameters> Metadata() => Runtime.Metadata.GetAll<ScriptDeviceParameters>("ScriptDeviceConfiguration");
        internal string MetadataText() => JsonSerializer.Serialize(Metadata().OrderBy(value => value.Serial, StringComparer.Ordinal).ToArray(), JsonDefaults.Options);
        internal object Values() => new { metadata = MetadataText(), Vm.Error, Vm.Status, Vm.Busy, Vm.Transferring, current = Vm.CurrentDevice?.Serial, main = Vm.MainDevice?.Serial,
            rows = Vm.Devices.Select(row => new { row.Serial, row.IsSelected, row.ScriptState, status = Runtime.Scripts.GetStatus(row.Serial), logs = Runtime.Scripts.GetLogs(row.Serial).ToArray() }).ToArray() };
        internal void Product(bool condition, string message) { Checks.Add(message); if (!condition) Products.Add(new InvalidOperationException(message)); }
        internal async Task Finish(Task task) { tasks.Add(task); await task.WaitAsync(TimeSpan.FromSeconds(5)); }
        internal async Task<T> Finish<T>(Task<T> task) { tasks.Add(task); return await task.WaitAsync(TimeSpan.FromSeconds(5)); }
        internal object[] TaskReport() => tasks.Select(task => (object)new { status = task.Status.ToString(), task.IsCompleted, task.IsCompletedSuccessfully }).ToArray();
        internal async Task Cleanup()
        {
            var errors = new List<Exception>(); Backend.Phase = "cleanup"; Lifetime.Cancel();
            try
            {
                if (Vm is not null) await Finish(Vm.ShutdownAsync());
                else if (Runtime is not null) await Finish(Runtime.DisposeAsync().AsTask());
                else
                {
                    if (api is not null) await Attempt(() => api.DisposeAsync().AsTask());
                    if (input is not null) await Attempt(() => input.DisposeAsync().AsTask());
                    if (scripts is not null) await Attempt(() => scripts.DisposeAsync().AsTask());
                    await Attempt(() => Backend.DisposeAsync().AsTask());
                    try { metadata?.Dispose(); } catch (Exception error) { errors.Add(error); }
                    if (log is not null) await Attempt(() => log.DisposeAsync().AsTask());
                }
            }
            catch (Exception error) { errors.Add(error); }
            finally
            {
                if (watching)
                {
                    Runtime.Scripts.StatusChanged -= OnStatus; Runtime.Scripts.EntryAdded -= OnLog;
                    foreach (var row in Vm.Devices) row.PropertyChanged -= OnRow;
                }
                Terminal.TrySetCanceled(); Lifetime.Dispose();
            }
            try { await Finish(dispatcher.InvokeAsync(() => { }, DispatcherPriority.ContextIdle).Task); }
            catch (Exception error) { errors.Add(error); }
            if (tasks.Any(task => !task.IsCompleted) || !Terminal.Task.IsCompleted || !Backend.Drained || !Backend.Disposed) errors.Add(new InvalidOperationException("自有公开任务/TCS/实际后端任务未排空或未实际释放。"));
            Observations.Add(new { stage = "cleanup", backendDisposed = Backend.Disposed, forwardingDrained = Backend.Drained, pendingPublicTasks = tasks.Count(task => !task.IsCompleted), terminal = Terminal.Task.Status.ToString() });
            CleanupErrors.AddRange(errors.Select(error => error.ToString()));
            if (errors.Count > 0) throw new AggregateException("自有参数编辑 VM 清理失败", errors);
            async Task Attempt(Func<Task> action) { try { await Finish(action()); } catch (Exception error) { errors.Add(error); } }
        }
    }

    private sealed record ScriptParameterEditingCall(string Phase, string Operation, string? Serial, string? Subject, string Status, bool Completed, string? Error);
    private sealed class ScriptParameterEditingBackend : IDeviceBackend, IIdentityPinnedDeviceBackend
    {
        private sealed class Call(string phase, string operation, string? serial, string? subject)
        { internal readonly string Phase = phase, Operation = operation; internal readonly string? Serial = serial, Subject = subject; internal Task? Task; internal string? Error; }
        internal readonly DemoDeviceBackend Inner = new(); private readonly object sync = new(); private readonly List<Call> calls = [];
        internal string Phase { get; set; } = "initialize";
        internal ScriptParameterEditingCall[] Calls(string? phase = null) { lock (sync) return calls.Where(call => phase is null || call.Phase == phase).Select(call => new ScriptParameterEditingCall(call.Phase, call.Operation, call.Serial, call.Subject, call.Task?.Status.ToString() ?? "SynchronousThrow", call.Task?.IsCompleted ?? call.Error is not null, call.Error ?? call.Task?.Exception?.ToString())).ToArray(); }
        internal bool Drained { get { lock (sync) return calls.All(call => call.Task?.IsCompleted ?? call.Error is not null); } }
        internal bool Disposed { get { lock (sync) return calls.Any(call => call.Operation == "Dispose" && call.Task?.IsCompletedSuccessfully == true); } }
        public IIdentityPinnedDeviceCommands BindIdentity(DeviceCatalog catalog) => new Pinned(this, Inner.BindIdentity(catalog));
        private sealed class Pinned(ScriptParameterEditingBackend owner, IIdentityPinnedDeviceCommands inner) : IIdentityPinnedDeviceCommands
        {
            public bool IsDemo => inner.IsDemo;
            public Task<string> StartAppAsync(DeviceCommandTarget target, string packageName, CancellationToken ct = default) => owner.Forward("StartApp", target.Serial, packageName, () => inner.StartAppAsync(target, packageName, ct));
            public Task TapAsync(DeviceCommandTarget target, int x, int y, CancellationToken ct = default) => owner.Forward("Tap", target.Serial, $"{x},{y}", () => inner.TapAsync(target, x, y, ct));
            public Task SwipeAsync(DeviceCommandTarget target, int x1, int y1, int x2, int y2, int durationMs, CancellationToken ct = default) => owner.Forward("Swipe", target.Serial, $"{x1},{y1},{x2},{y2},{durationMs}", () => inner.SwipeAsync(target, x1, y1, x2, y2, durationMs, ct));
        }
        private Task Forward(string operation, string? serial, string? subject, Func<Task> action)
        {
            var call = new Call(Phase, operation, serial, subject); lock (sync) calls.Add(call);
            try { var task = action(); lock (sync) call.Task = task; return task; } catch (Exception error) { lock (sync) call.Error = error.ToString(); throw; }
        }
        private Task<T> Forward<T>(string operation, string? serial, string? subject, Func<Task<T>> action)
        {
            var call = new Call(Phase, operation, serial, subject); lock (sync) calls.Add(call);
            try { var task = action(); lock (sync) call.Task = task; return task; } catch (Exception error) { lock (sync) call.Error = error.ToString(); throw; }
        }
        public Task<IReadOnlyList<DeviceInfo>> DiscoverAsync(CancellationToken ct = default) => Forward("Discover", null, null, () => Inner.DiscoverAsync(ct));
        public Task<string> ShellAsync(string serial, string command, bool useRoot = false, CancellationToken ct = default) => Forward("Shell", serial, command, () => Inner.ShellAsync(serial, command, useRoot, ct));
        public Task<byte[]> ScreenshotAsync(string serial, CancellationToken ct = default) => Forward("Screenshot", serial, null, () => Inner.ScreenshotAsync(serial, ct));
        public Task<AndroidNode> DumpAsync(string serial, CancellationToken ct = default) => Forward("Dump", serial, null, () => Inner.DumpAsync(serial, ct));
        public Task TapAsync(string serial, int x, int y, CancellationToken ct = default) => Forward("Tap", serial, $"{x},{y}", () => Inner.TapAsync(serial, x, y, ct));
        public Task SwipeAsync(string serial, int x1, int y1, int x2, int y2, int durationMs, CancellationToken ct = default) => Forward("Swipe", serial, $"{x1},{y1},{x2},{y2},{durationMs}", () => Inner.SwipeAsync(serial, x1, y1, x2, y2, durationMs, ct));
        public Task KeyEventAsync(string serial, int code, CancellationToken ct = default) => Forward("Key", serial, code.ToString(), () => Inner.KeyEventAsync(serial, code, ct));
        public Task TextAsync(string serial, string text, CancellationToken ct = default) => Forward("Text", serial, text, () => Inner.TextAsync(serial, text, ct));
        public Task<string> StartAppAsync(string serial, string packageName, string? activity = null, CancellationToken ct = default) => Forward("StartApp", serial, packageName, () => Inner.StartAppAsync(serial, packageName, activity, ct));
        public Task<string> StopAppAsync(string serial, string packageName, CancellationToken ct = default) => Forward("StopApp", serial, packageName, () => Inner.StopAppAsync(serial, packageName, ct));
        public Task<bool> ClearAppAsync(string serial, string packageName, CancellationToken ct = default) => Forward("ClearApp", serial, packageName, () => Inner.ClearAppAsync(serial, packageName, ct));
        public Task<IReadOnlyList<AppInfo>> ListAppsAsync(string serial, bool showSystem = false, bool needVersion = true, CancellationToken ct = default) => Forward("ListApps", serial, null, () => Inner.ListAppsAsync(serial, showSystem, needVersion, ct));
        public Task InstallAsync(string serial, string localPath, CancellationToken ct = default) => Forward("Install", serial, localPath, () => Inner.InstallAsync(serial, localPath, ct));
        public Task UninstallAsync(string serial, string packageName, CancellationToken ct = default) => Forward("Uninstall", serial, packageName, () => Inner.UninstallAsync(serial, packageName, ct));
        public Task PushAsync(string serial, string localPath, string remotePath, IProgress<double>? progress = null, CancellationToken ct = default) => Forward("Push", serial, remotePath, () => Inner.PushAsync(serial, localPath, remotePath, progress, ct));
        public Task PullAsync(string serial, string remotePath, string localPath, IProgress<double>? progress = null, CancellationToken ct = default) => Forward("Pull", serial, remotePath, () => Inner.PullAsync(serial, remotePath, localPath, progress, ct));
        public Task SetNodeTextAsync(string serial, NodeSelector selector, string text, CancellationToken ct = default) => Forward("SetNodeText", serial, text, () => Inner.SetNodeTextAsync(serial, selector, text, ct));
        public ValueTask DisposeAsync() => new(Forward("Dispose", null, null, () => Inner.DisposeAsync().AsTask()));
    }
}
