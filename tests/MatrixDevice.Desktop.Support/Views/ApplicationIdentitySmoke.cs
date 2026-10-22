using System.Diagnostics;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Threading;
using MatrixDevice.Core;
using MatrixDevice.Features;
using MatrixDevice.Host;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    private async Task VerifyApplicationIdentityAsync()
    {
        if (!ViewModel.Demo || !ViewModel.Runtime.IsDemo || !Dispatcher.CheckAccess() || Thread.CurrentThread.GetApartmentState() != ApartmentState.STA) throw new InvalidOperationException("应用改名身份验收仅限自有Demo/STA");
        var directory = Path.Combine(Path.GetFullPath(ViewModel.DataDirectory), "artifacts", "ai", Guid.NewGuid().ToString("N")[..8]);
        if (directory.Length > 190 || !Path.IsPathFullyQualified(directory)) throw new InvalidOperationException("应用身份验收需要短绝对artifacts目录");
        Directory.CreateDirectory(directory); var language = LocalizationService.Current.Language; var reports = new List<object>(); var failures = new List<Exception>(); var number = 0; Exception? failure = null;
        try
        {
            await Case("completed-rename-same-name", async state =>
            {
                state.ExpectStages("rename", "start", "stop"); var selected = state.SelectedApp; var target = state.TargetA; var before = state.A.Info; var reads = state.Backend.Count("List");
                state.Begin("rename"); await state.Rename(state.B.Name); var rowSame = ReferenceEquals(selected, state.SelectedApp); var selectionSame = selected.IsSelected; var identitySame = AppIdentitySame(before, state.A.Info); var name = await state.AppName(selected); state.Observe("rename-components", new { rowSame, selectionSame, identitySame, expected = state.B.Name, actual = selected.TargetName, actualCell = name });
                state.Check(rowSame && selectionSame && identitySame && ReferenceEquals(target, state.TargetA) && state.Backend.Count("List") == reads, "只改显示名保持原应用行/勾选/目标引用和真实连接身份，不用重读取绿"); state.Complete("rename");
                state.Begin("start"); await state.RunMenu(ApplicationOperation.Start); var startSucceeded = state.Succeeded("启动"); state.Complete("start", startSucceeded);
                state.Begin("stop"); await state.RunMenu(ApplicationOperation.Stop); var stopSucceeded = state.Succeeded("停止"); state.Complete("stop", stopSucceeded);
                var writes = state.Backend.Writes(); var selectedAfter = state.Window.applicationRows.SingleOrDefault(row => row.Serial == state.A.Serial && row.PackageName == AppIdentityState.Package && row.IsSelected); var latestName = selectedAfter?.TargetName; var latestCell = selectedAfter is null ? null : await state.AppName(selectedAfter);
                state.Observe("rename-command-aggregate", new { startSucceeded, stopSucceeded, nameBeforeStart = name, expectedName = state.B.Name, latestName, latestCell, writes }); state.Context();
                state.Check(startSucceeded && stopSucceeded && name == state.B.Name && latestName == state.B.Name && latestCell == state.B.Name && writes.Select(call => call.Operation).SequenceEqual(new[] { "Start", "Stop" }) && writes.All(call => call.Serial == state.A.Serial && call.PackageName == AppIdentityState.Package), "同名改名仍从真实菜单成功启停各一次，只写A原包；改名后即显示新名单元格且启停后不回退");
            });
            await Case("inflight-rename", async state =>
            {
                state.ExpectStages("list", "start"); state.Begin("list"); var lists = state.Backend.Count("List"); var gate = state.Backend.Hold("List"); var read = state.StartRead(); await state.Entered(gate); var before = state.A.Info;
                await state.Rename("在途读取新名称"); state.Check(AppIdentitySame(before, state.A.Info), "在途改名不制造新连接身份"); gate.Release.TrySetResult(); await state.Finish(read); state.Success("读取");
                state.Check(state.Backend.Count("List") == lists + 1 && ReferenceEquals(state.SelectedApp.App, gate.Applications!.Single(app => app.PackageName == AppIdentityState.Package)) && state.SelectedApp.TargetName == state.A.Name && await state.AppName(state.SelectedApp) == state.A.Name, "原冻结List payload正常发布且用最新名，包勾选保留，无额外重读"); state.Complete("list");
                state.Begin("start"); lists = state.Backend.Count("List"); var writes = state.Backend.Writes().Length; gate = state.Backend.Hold("Start"); var start = await state.StartMenu(ApplicationOperation.Start); await state.Entered(gate);
                await state.Rename("在途启动新名称"); gate.Release.TrySetResult(); await state.Finish(start); state.Success("启动");
                state.Check(state.Backend.Writes().Length == writes + 1 && state.Backend.Writes().Last().Serial == state.A.Serial && state.Backend.Count("List") == lists + 2 && state.SelectedApp.TargetName == state.A.Name && await state.AppName(state.SelectedApp) == state.A.Name, "Start已入门那次写确实已发送；改名后结果成功且不额外/错目标写，不回退旧名"); state.Complete("start"); state.Context();
            });
            await Case("confirm-rename", async state =>
            {
                var before = state.A.Info; var called = 0; state.Window.applicationConfirmationFixture = (_, detail) => { called++; state.Observe("confirm-intent", new { detail }); state.RenameDuringConfirmation("确认后应用新名"); return true; };
                await state.RunButton("卸载所选"); state.RequireConfirmationSync(); state.Check(called == 1 && AppIdentitySame(before, state.A.Info), "确认内真实metadata+完整VM刷新仍同身份"); state.Success("卸载");
                state.Check(state.Backend.Writes().Length == 1 && state.Backend.Writes().Single().Operation == "Uninstall" && state.Backend.Writes().Single().Serial == state.A.Serial && state.Backend.Writes().Single().PackageName == AppIdentityState.Package && state.Window.applicationRows.All(row => row.Serial != state.A.Serial || row.PackageName != AppIdentityState.Package) && state.TargetA.Caption.Contains(state.A.Name, StringComparison.Ordinal), "只改名不撤原确认意图，唯一A/原包卸载并正常刷新新名"); state.Context();
            });
            foreach (var change in new[] { "selection", "generation", "transport", "offline" }) await Case("confirm-reject-" + change, async state =>
            {
                var writes = state.Backend.Writes().Length; var called = 0; var before = state.A.Info;
                state.Window.applicationConfirmationFixture = (_, _) => { called++; if (change == "selection") state.TargetA.IsSelected = false; else state.ChangeDuringConfirmation(change); return true; };
                await state.RunButton("卸载所选");
                if (change != "selection") state.RequireConfirmationSync();
                state.Check(called == 1 && state.Backend.Writes().Length == writes && !state.Window.applicationRunning && !state.Vm.Busy && state.Window.applicationResult.Text.Length > 0 && !state.Window.applicationResult.Text.Contains("已完成，列表已刷新", StringComparison.Ordinal), "确认期间" + change + "变化拒绝，后端零写且非成功反馈");
                state.AssertChange(before, change); state.Context();
            });
            foreach (var change in new[] { "generation", "transport", "offline" }) await Case("inflight-list-reject-" + change, async state =>
            {
                var gate = state.Backend.Hold("List"); var read = state.StartRead(); await state.Entered(gate); var before = state.A.Info; await state.Change(change); state.AssertChange(before, change); gate.Release.TrySetResult(); await state.Finish(read);
                state.Check(state.Window.applicationTasks.Count == 1 && state.Window.applicationTasks.Single().ResultState == ApplicationTaskState.Failed && !state.Window.applicationRows.Any(row => row.Serial == state.A.Serial) && state.Backend.Writes().Length == 0, "在途旧List经真实" + change + "失效不发布旧payload或冒成功，读取本身无写");
                await state.Change("ready"); await state.Read(); state.Success("读取"); state.Check(state.Window.applicationRows.Any(row => row.Serial == state.A.Serial && row.PackageName == AppIdentityState.Package), "失效后后续有效真实读取可以恢复"); state.Context();
            });
            if (failures.Count > 0) throw new AggregateException("应用改名身份专项失败；旧应用smoke和其后套件NOT-RUN", failures);
        }
        catch (Exception error) { failure = error; Console.Error.WriteLine("APP-IDENTITY-PRIMARY " + error); }
        finally
        {
            try { LocalizationService.Current.SetLanguage(language); } catch (Exception error) { failure = failure is null ? error : new AggregateException(failure, error); }
            try { await File.WriteAllTextAsync(Path.Combine(directory, "application-identity.json"), JsonSerializer.Serialize(new { status = failure is null ? "PASS-OFFLINE-APPLICATION-IDENTITY" : "FAIL", caseCount = reports.Count, reports, error = failure?.ToString(), scope = "Own consistent Demo windows. Actual application-page bound Button/ContextMenu commands; logical WPF invocation, not physical input. Confirmation uses existing own-Demo fixture callback, not a real modal. No Android/ADB/config/API/video/node/clipboard/native-dialog/runtime-hot-swap claim." }, JsonDefaults.Options)); }
            catch (Exception error) { Console.Error.WriteLine(error); failure = failure is null ? error : new AggregateException(failure, error); }
        }
        if (failure is not null) ExceptionDispatchInfo.Capture(failure).Throw();
        async Task Case(string name, Func<AppIdentityState, Task> action)
        {
            AppIdentityState? state = null; Exception? error = null; var cleaned = false;
            try { state = await AppIdentityState.Create(this, Path.Combine(directory, "c" + (++number).ToString("00"))); await action(state); }
            catch (Exception caught) { error = caught; Console.Error.WriteLine("APP-IDENTITY-CASE " + name + " " + caught); }
            finally
            {
                if (state is not null) try { await state.Cleanup(); cleaned = true; } catch (Exception cleanupError) { error = error is null ? cleanupError : new AggregateException(error, cleanupError); }
                reports.Add(new { name, status = error is null && cleaned ? "PASS" : "FAIL", cleaned, confirmationSyncSucceeded = state?.ConfirmationSyncSucceeded, confirmationSyncError = state?.ConfirmationSyncFailure?.ToString(), checks = state?.Checks.ToArray(), observations = state?.Events.ToArray(), stages = state?.StageReport(), calls = state?.Backend.Calls(), gates = state?.Backend.GateReport(), tasks = state?.Tasks(), failure = error?.ToString() });
            }
            if (error is not null) { failures.Add(new InvalidOperationException(name, error)); if (!cleaned) throw new AggregateException("自有case未清理，停止后续接纳", failures); }
        }
    }

    private static bool AppIdentitySame(DeviceInfo a, DeviceInfo b) => a.Serial == b.Serial && a.Generation == b.Generation && a.TransportId == b.TransportId && a.State == b.State;
    private static int? AppIdentityId(object? value) => value is null ? null : System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(value);
    private sealed class AppIdentityState
    {
        internal const string Package = "com.matrix.identity";
        internal required MainWindow Window; internal required WorkspaceViewModel Vm; internal required AppIdentityBackend Backend;
        internal readonly List<string> Checks = []; internal readonly List<object> Events = []; private readonly List<Task> originals = []; private readonly Dictionary<string, string> stages = [];
        internal bool ConfirmationSyncSucceeded; internal Exception? ConfirmationSyncFailure;
        private DeviceRow? current; private DeviceRow? main; private DeviceRow[] rows = []; private bool[] selected = []; private DeviceRow[] targets = []; private UnifiedDeviceRow[] unified = []; private bool[] unifiedSelected = []; private bool cleanupStarted;
        internal DeviceRow A => Vm.Devices.Single(row => row.Serial == "DEMO-001"); internal DeviceRow B => Vm.Devices.Single(row => row.Serial == "DEMO-002");
        internal ApplicationTargetRow TargetA => Window.applicationTargets.Single(row => row.Serial == A.Serial);
        internal InstalledApplicationRow SelectedApp => Window.applicationRows.Single(row => row.Serial == A.Serial && row.PackageName == Package && row.IsSelected);
        internal static async Task<AppIdentityState> Create(MainWindow owner, string directory)
        {
            Directory.CreateDirectory(directory); var backend = new AppIdentityBackend(); RollingLog? log = null; CatalogRepository? metadata = null; MatrixRuntime? runtime = null; WorkspaceViewModel? vm = null; MainWindow? window = null; AppIdentityState? state = null; InputCoordinator? input = null; ScriptScheduler? scripts = null; ApiServiceManager? api = null; Task? initializing = null;
            try
            {
                log = new RollingLog(Path.Combine(directory, "logs")); metadata = new CatalogRepository(Path.Combine(directory, "db.sqlite3")); var catalog = new DeviceCatalog(backend); var automation = new AutomationService(backend, catalog); scripts = new ScriptScheduler(backend); input = new InputCoordinator(catalog, backend); api = new ApiServiceManager(catalog, backend, automation, log);
                var settings = new AppSettings { Language = "zh-CN", AutoStartApi = false, AutoConnect = false, SyncMouse = false, SyncKeyboard = false, WindowWidth = 1000, WindowHeight = 700 };
                runtime = new MatrixRuntime { Backend = backend, Devices = catalog, Automation = automation, Scripts = scripts, Input = input, Api = api, Metadata = metadata, Log = log, Settings = settings, FrozenConnectionSettings = settings, SettingsStore = new SettingsStore(Path.Combine(directory, "AppConfig.json")), IsDemo = true };
                api.DeviceSnapshotProvider = runtime.GetDevices; backend.Snapshot = runtime.GetDevices; vm = new WorkspaceViewModel(runtime, directory, true); initializing = vm.RefreshDevicesAsync(); await initializing.WaitAsync(TimeSpan.FromSeconds(5));
                window = new MainWindow(vm) { Owner = owner, ShowActivated = false }; window.captureTimer.Stop(); window.directoryTimer.Stop(); window.monitorTimer.Stop(); state = new() { Window = window, Vm = vm, Backend = backend }; window.Show(); window.NavigateTo("应用"); await state.Idle();
                state.Check(vm.Demo && runtime.IsDemo && ReferenceEquals(runtime.Backend, backend) && ReferenceEquals(runtime.Devices, catalog) && ReferenceEquals(runtime.Automation, automation) && ReferenceEquals(runtime.Scripts, scripts) && ReferenceEquals(runtime.Input, input) && ReferenceEquals(runtime.Api, api) && !api.BasicRunning && !api.AutomatorRunning && api.DevicePorts.Count == 0 && !window.DebugAutomaticFixturePermitted && !window.ReconnectLoadingFixturePermitted, "一致自有Demo/STA窗口、默认设备读许可未开放/API未启");
                await state.Finish(state.Track(vm.SetMainAsync(state.B))); vm.CurrentDevice = state.B; state.A.IsSelected = false; state.B.IsSelected = true; vm.UpdateSelection();
                state.TargetA.IsSelected = true; state.QuiesceRead(); await state.Read(); state.Success("读取"); window.applicationRows.Single(row => row.Serial == state.A.Serial && row.PackageName == Package).IsSelected = true;
                state.current = vm.CurrentDevice; state.main = vm.MainDevice; state.rows = vm.Devices.ToArray(); state.selected = vm.Devices.Select(row => row.IsSelected).ToArray(); state.targets = vm.Targets().ToArray(); state.unified = window.unifiedDevices.ToArray(); state.unifiedSelected = window.unifiedDevices.Select(row => row.IsSelected).ToArray(); state.Observe("created"); return state;
            }
            catch (Exception error)
            {
                var errors = new List<Exception>(); if (state is not null) { try { await state.Cleanup(); } catch (Exception cleanupError) { errors.Add(cleanupError); } }
                else
                {
                    backend.ReleaseAll(); try { if (window is not null) { LocalizationService.Current.LanguageChanged -= window.OnLanguageChanged; window.AbortStartup(); } } catch (Exception cleanupError) { errors.Add(cleanupError); }
                    if (initializing is not null) await Attempt(() => initializing.WaitAsync(TimeSpan.FromSeconds(5)));
                    if (vm is not null) await Attempt(() => vm.ShutdownAsync().WaitAsync(TimeSpan.FromSeconds(5))); else if (runtime is not null) await Attempt(() => runtime.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(5)));
                    else { if (api is not null) await Attempt(() => api.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(5))); if (input is not null) await Attempt(() => input.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(5))); if (scripts is not null) await Attempt(() => scripts.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(5))); await Attempt(() => backend.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(5))); try { metadata?.Dispose(); } catch (Exception cleanupError) { errors.Add(cleanupError); } if (log is not null) await Attempt(() => log.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(5))); }
                }
                if (errors.Count > 0) throw new AggregateException("应用身份初始化与清理失败", new[] { error }.Concat(errors)); throw;
                async Task Attempt(Func<Task> action) { try { await action(); } catch (Exception cleanupError) { errors.Add(cleanupError); } }
            }
        }
        internal void ExpectStages(params string[] values) { foreach (var value in values) stages.Add(value, "NOT-RUN"); } internal void Begin(string value) { stages[value] = "STARTED"; Observe("stage:" + value); } internal void Complete(string value, bool passed = true) => stages[value] = passed ? "PASS" : "FAIL"; internal object[] StageReport() => stages.Select(pair => (object)new { stage = pair.Key, status = pair.Value }).ToArray();
        private Task Track(Task task) { if (!originals.Contains(task)) originals.Add(task); return task; } internal object[] Tasks() => originals.Select(task => (object)new { task.IsCompleted, status = task.Status.ToString() }).ToArray();
        internal async Task Finish(Task task) { await Track(task).WaitAsync(TimeSpan.FromSeconds(5)); await Idle(); }
        internal async Task Idle() { await Window.Dispatcher.InvokeAsync(Window.UpdateLayout, DispatcherPriority.ApplicationIdle).Task.WaitAsync(TimeSpan.FromSeconds(5)); await Window.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ContextIdle).Task.WaitAsync(TimeSpan.FromSeconds(5)); }
        private Button Button(string caption) => Descendants<Button>(Window.applicationPage).Single(button => AutomationProperties.GetName(button) == caption);
        private void QuiesceRead() { Window.applicationReadTimer?.Stop(); Window.applicationReadPending = false; }
        internal Task StartRead() { QuiesceRead(); var button = Button("读取应用"); Check(button.IsVisible && button.IsEnabled && button.Command is AsyncCommand && button.Command.CanExecute(null), "实际应用读取Button命令可执行"); Observe("button-command", new { caption = "读取应用", buttonId = AppIdentityId(button), commandId = AppIdentityId(button.Command), physical = false }); return Track(((AsyncCommand)button.Command!).ExecuteAsync()); }
        internal Task Read() => Finish(StartRead());
        internal async Task RunButton(string caption) { var button = Button(caption); Check(button.IsVisible && button.IsEnabled && button.Command is AsyncCommand && button.Command.CanExecute(null), "实际应用按钮可执行：" + caption); Observe("button-command", new { caption, buttonId = AppIdentityId(button), commandId = AppIdentityId(button.Command), physical = false }); await Finish(((AsyncCommand)button.Command!).ExecuteAsync()); }
        internal async Task<Task> StartMenu(ApplicationOperation operation)
        {
            var more = Button("更多"); Check(more.IsVisible && more.IsEnabled, "真实更多入口可见可用"); var peer = new ButtonAutomationPeer(more); ((IInvokeProvider)peer.GetPattern(PatternInterface.Invoke)!).Invoke(); await Idle();
            var menu = more.ContextMenu ?? throw new InvalidOperationException("原更多ContextMenu不存在"); var caption = ApplicationOperationCaption(operation); var item = menu.Items.Cast<MenuItem>().Single(row => Equals(row.Header, caption));
            Check(menu.IsOpen && ReferenceEquals(more.ContextMenu, menu) && item.IsEnabled && item.Command is AsyncCommand && item.Command.CanExecute(null), "真实挂接ContextMenu及MenuItem命令可执行：" + caption);
            Observe("menu-command", new { caption, moreId = AppIdentityId(more), menuId = AppIdentityId(menu), itemId = AppIdentityId(item), commandId = AppIdentityId(item.Command), menu.IsOpen, physical = false, directPageHelperCalled = false }); var task = Track(((AsyncCommand)item.Command!).ExecuteAsync()); menu.IsOpen = false; return task;
        }
        internal async Task RunMenu(ApplicationOperation operation) => await Finish(await StartMenu(operation));
        internal bool Succeeded(string operation) { var success = Window.applicationTasks.Count == 1 && Window.applicationTasks.Single().Operation == operation && Window.applicationTasks.Single().ResultState == ApplicationTaskState.Succeeded && !Window.applicationRunning && !Vm.Busy && Vm.Error.Length == 0; Observe("expected-success", new { operation, success }); return success; }
        internal void Success(string operation) => Check(Succeeded(operation), "实际当前应用批次成功：" + operation);
        internal async Task<string?> AppName(InstalledApplicationRow row)
        {
            var grid = Descendants<DataGrid>(Window.applicationPage).Single(item => ReferenceEquals(item.ItemsSource, Window.applicationRowsView)); var column = grid.Columns.OfType<DataGridTextColumn>().Single(item => item.Binding is Binding binding && binding.Path.Path == nameof(InstalledApplicationRow.TargetName)); grid.ScrollIntoView(row, column); await Idle(); var cell = column.GetCellContent(row) as TextBlock; Observe("actual-name-cell", new { gridId = AppIdentityId(grid), rowId = AppIdentityId(row), row.TargetName, cellText = cell?.Text, cellVisible = cell?.IsVisible, binding = nameof(InstalledApplicationRow.TargetName) }); return cell is { IsVisible: true } ? cell.Text : null;
        }
        internal async Task Rename(string name) { Vm.Runtime.Metadata.UpsertMetadata(A.Serial, name, A.Info.GroupId); await Finish(Vm.RefreshDeviceSnapshotAsync()); QuiesceRead(); Observe("metadata-full-sync", new { expectedName = name, A.Info }); Check(A.Name == name && TargetA.Caption.Contains(name, StringComparison.Ordinal), "真实metadata完整同步更新目标名"); }
        internal void RenameDuringConfirmation(string name) { Vm.Runtime.Metadata.UpsertMetadata(A.Serial, name, A.Info.GroupId); ConfirmationSync(); Observe("confirmation-rename", new { name, A.Info }); }
        private void ConfirmationSync() { try { var task = Track(Vm.RefreshDeviceSnapshotAsync()); ConfirmationSyncSucceeded = task.IsCompletedSuccessfully; Observe("confirmation-sync-precondition", new { ConfirmationSyncSucceeded, status = task.Status.ToString() }); Check(ConfirmationSyncSucceeded, "确认同步必须已完整成功，不阻塞UI/不泵消息/不伪Info"); QuiesceRead(); } catch (Exception error) { ConfirmationSyncFailure = error; throw; } }
        internal void RequireConfirmationSync() { if (ConfirmationSyncFailure is not null) ExceptionDispatchInfo.Capture(ConfirmationSyncFailure).Throw(); Check(ConfirmationSyncSucceeded, "独立前置：生产命令wrapper不能把确认内同步失败吞成guard成功"); }
        internal async Task Change(string change) { Backend.Change(A.Serial, Changed(change)); await Finish(Vm.RefreshDeviceSnapshotAsync()); QuiesceRead(); Observe("actual-catalog-change", new { change, A.Info }); }
        internal void ChangeDuringConfirmation(string change) { Backend.Change(A.Serial, Changed(change)); ConfirmationSync(); Observe("confirmation-source-change", new { change, A.Info }); }
        private DeviceInfo Changed(string change) => change switch { "generation" => A.Info with { Width = A.Info.Width + 1 }, "transport" => A.Info with { TransportId = (A.Info.TransportId ?? 0) + 1 }, "offline" => A.Info with { State = "Offline" }, "ready" => A.Info with { State = "Ready" }, _ => throw new ArgumentOutOfRangeException(nameof(change)) };
        internal void AssertChange(DeviceInfo before, string change) { var correct = change switch { "selection" => !TargetA.IsSelected, "generation" => A.Info.Generation == before.Generation + 1 && A.Info.TransportId == before.TransportId, "transport" => A.Info.TransportId != before.TransportId && A.Info.Generation == before.Generation + 1, "offline" => !A.Info.Online && A.Info.Generation == before.Generation + 1, _ => false }; Observe("source-change-components", new { change, before, actual = A.Info, correct }); Check(correct, "真实确认/在途变化前置成立：" + change); }
        internal async Task Entered(AppIdentityGate gate) { await gate.Entered.Task.WaitAsync(TimeSpan.FromSeconds(5)); Check(gate.Claimed && gate.Entered.Task.IsCompletedSuccessfully, "真实后端已进入：" + gate.Operation); Observe("backend-held", new { gate.Operation, gate.Source, payload = gate.Applications?.Select(app => new { app.PackageName, app.Name }).ToArray(), writesAlreadySent = Backend.Writes().Length }); }
        internal void Context() { var sameCurrent = ReferenceEquals(current, Vm.CurrentDevice); var sameMain = ReferenceEquals(main, Vm.MainDevice); var sameRows = rows.SequenceEqual(Vm.Devices); var sameSelection = selected.SequenceEqual(Vm.Devices.Select(row => row.IsSelected)); var sameTargets = targets.SequenceEqual(Vm.Targets()); var sameUnified = unified.SequenceEqual(Window.unifiedDevices); var sameUnifiedSelection = unifiedSelected.SequenceEqual(Window.unifiedDevices.Select(row => row.IsSelected)); Observe("global-context-components", new { sameCurrent, sameMain, sameRows, sameSelection, sameTargets, sameUnified, sameUnifiedSelection }); Check(sameCurrent && sameMain && sameRows && sameSelection && sameTargets && sameUnified && sameUnifiedSelection, "应用独立选择/改名/失效未改CurrentB/MainB/群控行引用勾选和有效目标"); }
        internal void Check(bool value, string message) { if (!value) { Observe("assertion-failed", new { message }); throw new InvalidOperationException(message); } Checks.Add(message); }
        internal void Observe(string stage, object? detail = null) { if (Events.Count == 128) Events.RemoveAt(0); Events.Add(new { stage, at = Stopwatch.GetTimestamp(), detail, Vm.Busy, Vm.Error, Vm.Status, result = Window.applicationResult.Text, running = Window.applicationRunning, pending = Window.applicationReadPending, current = Vm.CurrentDevice?.Serial, main = Vm.MainDevice?.Serial, targets = Window.applicationTargets.Select(row => new { row.Serial, row.Info, row.IsSelected, row.Available, row.Caption }).ToArray(), applications = Window.applicationRows.Select(row => new { id = AppIdentityId(row), row.Serial, row.TargetName, row.PackageName, row.IsSelected, row.Target }).ToArray(), taskRows = Window.applicationTasks.Select(row => new { row.Serial, row.Operation, row.Subject, row.ResultState, row.Message }).ToArray() }); }
        internal async Task Cleanup()
        {
            if (cleanupStarted) throw new InvalidOperationException("自有应用scope不得重复清理"); cleanupStarted = true; Window.applicationConfirmationFixture = null; QuiesceRead(); Window.applicationCancellation?.Cancel(); Backend.ReleaseAll(); var errors = new List<Exception>();
            try { await Task.WhenAll(originals).WaitAsync(TimeSpan.FromSeconds(5)); await Idle(); } catch (Exception error) { errors.Add(error); }
            try { QuiesceRead(); Check(originals.All(task => task.IsCompleted) && Backend.Drained && !Window.applicationRunning && !Vm.Busy && !Window.applicationReadPending, "自有原Task/TCS与应用批次排空，停止仅夹具debounce"); Observe("cleanup-drained"); } catch (Exception error) { errors.Add(error); }
            try { LocalizationService.Current.LanguageChanged -= Window.OnLanguageChanged; Window.AbortStartup(); } catch (Exception error) { errors.Add(error); }
            try { await Vm.ShutdownAsync().WaitAsync(TimeSpan.FromSeconds(5)); } catch (Exception error) { errors.Add(error); }
            if (errors.Count > 0) throw new AggregateException("自有应用身份清理失败", errors);
        }
        private static IEnumerable<T> Descendants<T>(DependencyObject parent) where T : DependencyObject { for (var index = 0; index < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); index++) { var child = System.Windows.Media.VisualTreeHelper.GetChild(parent,index); if (child is T match) yield return match; foreach (var nested in Descendants<T>(child)) yield return nested; } }
    }

    private sealed record AppIdentityCall(string Operation, string? Serial, string? PackageName, long At, DeviceInfo? Source);
    private sealed class AppIdentityGate(string operation)
    {
        internal readonly string Operation = operation; internal bool Claimed; internal DeviceInfo? Source; internal AppInfo[]? Applications;
        internal readonly TaskCompletionSource Entered = new(TaskCreationOptions.RunContinuationsAsynchronously); internal readonly TaskCompletionSource Release = new(TaskCreationOptions.RunContinuationsAsynchronously);
    }
    private sealed class AppIdentityBackend : IDeviceBackend
    {
        private readonly object sync = new(); private readonly List<AppIdentityCall> calls = []; private readonly List<AppIdentityGate> gates = []; private int reads;
        private readonly Dictionary<string, DeviceInfo> devices = new(StringComparer.Ordinal) { ["DEMO-001"] = new() { Serial = "DEMO-001", Name = "应用身份A", State = "Ready", Width = 160, Height = 240, TransportId = 11 }, ["DEMO-002"] = new() { Serial = "DEMO-002", Name = "共同应用名", State = "Ready", Width = 160, Height = 240, TransportId = 22 } };
        private readonly Dictionary<string, List<AppInfo>> apps = new(StringComparer.Ordinal) { ["DEMO-001"] = [new(AppIdentityState.Package, "自有A", "1.0")], ["DEMO-002"] = [new(AppIdentityState.Package, "自有B", "1.0")] };
        internal Func<IReadOnlyList<DeviceInfo>>? Snapshot; internal void Change(string serial, DeviceInfo info) { lock (sync) devices[serial] = info; }
        internal AppIdentityCall[] Calls() { lock (sync) return calls.ToArray(); } internal AppIdentityCall[] Writes() { lock (sync) return calls.Where(call => call.Operation is "Start" or "Stop" or "Uninstall" || call.Operation.StartsWith("UnexpectedWrite:", StringComparison.Ordinal)).ToArray(); } internal int Count(string operation) { lock (sync) return calls.Count(call => call.Operation == operation); }
        internal AppIdentityGate Hold(string operation) { var gate = new AppIdentityGate(operation); lock (sync) gates.Add(gate); return gate; } internal bool Drained { get { lock (sync) return gates.All(gate => gate.Entered.Task.IsCompleted && gate.Release.Task.IsCompleted); } }
        internal object[] GateReport() { lock (sync) return gates.Select(gate => (object)new { gate.Operation, gate.Claimed, gate.Source, entered = gate.Entered.Task.Status.ToString(), released = gate.Release.Task.Status.ToString(), payload = gate.Applications?.Select(app => new { app.PackageName, app.Name }).ToArray() }).ToArray(); }
        internal void ReleaseAll() { lock (sync) foreach (var gate in gates) { gate.Release.TrySetResult(); if (!gate.Claimed) gate.Entered.TrySetCanceled(); } }
        private AppIdentityGate? Record(string operation, string? serial, string? package)
        {
            var source = serial is null ? null : Snapshot?.Invoke().SingleOrDefault(info => info.Serial == serial);
            lock (sync) { calls.Add(new(operation, serial, package, Stopwatch.GetTimestamp(), source)); var gate = gates.FirstOrDefault(item => !item.Claimed && item.Operation == operation); if (gate is not null) { gate.Claimed = true; gate.Source = source; } return gate; }
        }
        public Task<IReadOnlyList<DeviceInfo>> DiscoverAsync(CancellationToken ct = default) { ct.ThrowIfCancellationRequested(); Record("Discover", null, null); lock (sync) return Task.FromResult<IReadOnlyList<DeviceInfo>>(devices.Values.ToArray()); }
        public async Task<IReadOnlyList<AppInfo>> ListAppsAsync(string serial, bool showSystem = false, bool needVersion = true, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested(); var gate = Record("List", serial, null); AppInfo[] payload; lock (sync) { var read = ++reads; payload = apps[serial].Select(app => app with { Name = app.Name + "#read" + read }).ToArray(); if (gate is not null) gate.Applications = payload; }
            if (gate is not null) { gate.Entered.TrySetResult(); await gate.Release.Task.WaitAsync(ct); } return payload;
        }
        public async Task<string> StartAppAsync(string serial, string packageName, string? activity = null, CancellationToken ct = default) { var gate = Record("Start", serial, packageName); if (gate is not null) { gate.Entered.TrySetResult(); await gate.Release.Task.WaitAsync(ct); } return "own Demo start sent"; }
        public Task<string> StopAppAsync(string serial, string packageName, CancellationToken ct = default) { ct.ThrowIfCancellationRequested(); Record("Stop", serial, packageName); return Task.FromResult("own Demo stop sent"); }
        public Task UninstallAsync(string serial, string packageName, CancellationToken ct = default) { ct.ThrowIfCancellationRequested(); Record("Uninstall", serial, packageName); lock (sync) apps[serial].RemoveAll(app => app.PackageName == packageName); return Task.CompletedTask; }
        public Task<byte[]> ScreenshotAsync(string serial, CancellationToken ct = default) => throw new InvalidOperationException("本专项不允许截图读"); public Task<AndroidNode> DumpAsync(string serial, CancellationToken ct = default) => throw new InvalidOperationException("本专项不允许节点读");
        private Exception UnexpectedWrite(string operation, string serial, string? subject = null) { Record("UnexpectedWrite:" + operation, serial, subject); return new InvalidOperationException("本专项不允许设备写入口：" + operation + ":" + serial); }
        public Task<string> ShellAsync(string serial, string command, bool useRoot = false, CancellationToken ct = default) => throw UnexpectedWrite("Shell", serial, command); public Task TapAsync(string serial, int x, int y, CancellationToken ct = default) => throw UnexpectedWrite("Tap", serial); public Task SwipeAsync(string serial,int x1,int y1,int x2,int y2,int durationMs,CancellationToken ct=default)=>throw UnexpectedWrite("Swipe", serial); public Task KeyEventAsync(string serial,int code,CancellationToken ct=default)=>throw UnexpectedWrite("Key", serial); public Task TextAsync(string serial,string text,CancellationToken ct=default)=>throw UnexpectedWrite("Text", serial, text);
        public Task<bool> ClearAppAsync(string serial,string packageName,CancellationToken ct=default)=>throw UnexpectedWrite("Clear", serial, packageName); public Task InstallAsync(string serial,string localPath,CancellationToken ct=default)=>throw UnexpectedWrite("Install", serial, localPath); public Task PushAsync(string serial,string localPath,string remotePath,IProgress<double>? progress=null,CancellationToken ct=default)=>throw UnexpectedWrite("Push", serial, remotePath); public Task PullAsync(string serial,string remotePath,string localPath,IProgress<double>? progress=null,CancellationToken ct=default)=>throw UnexpectedWrite("Pull", serial, remotePath); public Task SetNodeTextAsync(string serial,NodeSelector selector,string text,CancellationToken ct=default)=>throw UnexpectedWrite("SetNodeText", serial, text);
        public ValueTask DisposeAsync() { ReleaseAll(); return ValueTask.CompletedTask; }
    }
}
