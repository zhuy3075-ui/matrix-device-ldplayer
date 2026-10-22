using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using MatrixDevice.Core;
using MatrixDevice.Features;
using MatrixDevice.Host;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    private ReconnectLoadingScope? reconnectLoadingScope;
    private bool ReconnectLoadingFixturePermitted => reconnectLoadingScope?.Matches(this) == true;

    private async Task VerifyReconnectedDebugLoadingAsync()
    {
        if (!ViewModel.Demo || !ViewModel.Runtime.IsDemo || !Dispatcher.CheckAccess() || Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
            throw new InvalidOperationException("恢复首载验收仅限自有一致Demo/STA。");
        var directory = Path.Combine(Path.GetFullPath(ViewModel.DataDirectory), "artifacts", "rl", Guid.NewGuid().ToString("N")[..8]);
        if (!Path.IsPathFullyQualified(directory) || directory.Length > 190) throw new InvalidOperationException("恢复首载验收需要短绝对artifacts路径：" + directory.Length);
        Directory.CreateDirectory(directory);
        var originalMain = Application.Current.MainWindow; var originalShutdown = Application.Current.ShutdownMode; var language = LocalizationService.Current.Language;
        var reports = new List<object>(); var failures = new List<Exception>(); Exception? failure = null; var number = 0;
        try
        {
            await Case("default-demo", false, async state =>
            {
                state.Navigate("节点"); await state.Idle(); var info = state.A.Info; state.Backend.Change(state.A.Serial, info with { TransportId = info.TransportId + 1 }); await state.Sync();
                state.Check(state.Backend.ReadCount == 0 && !state.Window.debugAutoPending && !state.Window.debugAutoLoading && !state.Window.debugAutoQueued, "默认Demo实际导航与新身份同步仍不自动读取");
            });
            await Case("same-row-offline-ready", true, async state =>
            {
                await state.InitialLoad(); var intent = ReconnectIntent.Take(state); var image = state.Vm.DebugImage; var root = state.Vm.Nodes.Single(); var selected = state.Vm.SelectedNode; var reads = state.Backend.ReadCount; var info = state.A.Info;
                state.Backend.Change(state.A.Serial, info with { State = "Offline" }); await state.Sync();
                state.Check(state.A.Info.State == "Offline" && state.A.Info.Generation == info.Generation + 1 && ReferenceEquals(image, state.Vm.DebugImage) && ReferenceEquals(root, state.Vm.Nodes.Single()) && ReferenceEquals(selected, state.Vm.SelectedNode), "真实Offline保留合法历史图/根/选择，不主动重读"); state.NoReads(reads); intent.Same(state);
                var shot = state.Backend.Hold("Screenshot", state.A.Serial); state.Backend.Change(state.A.Serial, state.Backend.Info(state.A.Serial) with { State = "Ready" }); await state.Sync(); var final = state.A.Info;
                state.Check(final.Online && final.Generation == info.Generation + 2, "同row重新Ready实际形成新代次"); await state.Entered(shot); shot.Release.TrySetResult(); await state.Loaded(final); state.Sequence(reads, "Screenshot", "Dump"); intent.Same(state);
            });
            await Case("same-row-ready-transport", true, async state =>
            {
                await state.InitialLoad(); var intent = ReconnectIntent.Take(state); var reads = state.Backend.ReadCount; var before = state.A.Info;
                await state.ChangeAndLoad(before with { TransportId = before.TransportId + 1 }); state.Check(state.A.Info.Generation == before.Generation + 1 && state.A.Info.TransportId == before.TransportId + 1, "Ready传输换代由真实Catalog接纳"); state.Sequence(reads, "Screenshot", "Dump"); intent.Same(state);
            });
            await Case("same-row-ready-geometry", true, async state =>
            {
                await state.InitialLoad(); var intent = ReconnectIntent.Take(state); var reads = state.Backend.ReadCount; var before = state.A.Info;
                await state.ChangeAndLoad(before with { Width = 180, Height = 260 });
                state.Check(state.A.Info.Generation == before.Generation + 1 && state.A.Info.Width == 180 && state.A.Info.Height == 260 && state.A.Info.Rotation == before.Rotation, "仅宽高变化实际换代且旋转不变");
                var resized = state.A.Info; await state.ChangeAndLoad(resized with { Rotation = 1 });
                state.Check(state.A.Info.Generation == resized.Generation + 1 && state.A.Info.Width == resized.Width && state.A.Info.Height == resized.Height && state.A.Info.Rotation == 1, "另一次仅rotation变化实际换代且宽高不变"); state.Sequence(reads, "Screenshot", "Dump", "Screenshot", "Dump"); intent.Same(state);
            });
            await Case("stable-metadata", true, async state =>
            {
                await state.InitialLoad(); var intent = ReconnectIntent.Take(state); var image = state.Vm.DebugImage; var nodes = state.Vm.Nodes.ToArray(); var selected = state.Vm.SelectedNode; var properties = state.Vm.NodeProperties; var reads = state.Backend.ReadCount; var generation = state.A.Info.Generation;
                await state.Sync(); state.Vm.Runtime.Metadata.UpsertMetadata(state.A.Serial, "同身份恢复首载改名", "own-group"); state.Backend.Change(state.A.Serial, state.A.Info with { Model = "metadata-only" }); await state.Sync(); await state.Sync(); await state.Settled();
                state.Check(state.A.Info.Name == "同身份恢复首载改名" && state.A.Info.GroupId == "own-group" && state.A.Info.Generation == generation && ReferenceEquals(image, state.Vm.DebugImage) && nodes.SequenceEqual(state.Vm.Nodes) && ReferenceEquals(selected, state.Vm.SelectedNode) && properties == state.Vm.NodeProperties, "同身份和真实metadata同步保留原检查与选择"); state.NoReads(reads); intent.Same(state); state.Projection();
            });
            foreach (var kind in new[] { "busy", "external", "transfer", "script" }) await Case("blocked-" + kind, true, async state =>
            {
                await state.InitialLoad(); state.SelectPeerOnly(); var intent = ReconnectIntent.Take(state); var blocker = await state.Block(kind); var reads = state.Backend.ReadCount;
                await state.ChangeWhileBlocked(); state.NoReads(reads); state.PendingOnly();
                var shot = state.Backend.Hold("Screenshot", state.A.Serial); var final = state.A.Info; await blocker.Release(); await state.Entered(shot); shot.Release.TrySetResult(); await state.Loaded(final); state.Sequence(reads, "Screenshot", "Dump"); intent.Same(state);
            });
            await Case("dual-transfer-script", true, async state =>
            {
                await state.InitialLoad(); state.SelectPeerOnly(); var intent = ReconnectIntent.Take(state); var transfer = await state.Block("transfer"); var script = await state.Block("script"); var reads = state.Backend.ReadCount;
                await state.ChangeWhileBlocked(); await transfer.Release(); await state.Idle();
                state.Check(!state.Vm.Transferring && state.Scripts.GetStatus(state.B.Serial).State == ScriptRunState.Running && state.Vm.HasActiveOperations, "先释放传输，未换代B脚本仍真实Running"); state.NoReads(reads); state.PendingOnly();
                var shot = state.Backend.Hold("Screenshot", state.A.Serial); var final = state.A.Info; await script.Release(); await state.Entered(shot); shot.Release.TrySetResult(); await state.Loaded(final); state.Sequence(reads, "Screenshot", "Dump"); intent.Same(state);
            });
            foreach (var operation in new[] { "Screenshot", "Dump" }) await Case("inflight-" + operation, true, async state =>
            {
                await state.InitialLoad(); var intent = ReconnectIntent.Take(state); var reads = state.Backend.ReadCount; var publication = state.Published.Count; var imagePublication = state.PublishedImages.Count;
                var first = state.Backend.Hold(operation, state.A.Serial); state.Backend.Change(state.A.Serial, state.A.Info with { TransportId = state.A.Info.TransportId + 1 }); await state.Sync(); await state.Entered(first);
                await state.ChangeWhileBlocked(); var final = state.A.Info; var next = state.Backend.Hold("Screenshot", state.A.Serial);
                state.Check(state.Backend.ActiveShotDump == 1 && state.Backend.MaximumActiveShotDump <= 1, "旧读仍在途时未并发新首载");
                first.Release.TrySetResult(); await state.Entered(next); next.Release.TrySetResult(); await state.Loaded(final);
                state.Sequence(reads, operation == "Screenshot" ? new[] { "Screenshot", "Screenshot", "Dump" } : new[] { "Screenshot", "Dump", "Screenshot", "Dump" });
                var oldNodesAbsent = operation != "Dump" || state.Published.Skip(publication).All(item => item.Marker != first.PayloadMarker);
                var oldImageAbsent = operation != "Screenshot" || state.PublishedImages.Skip(imagePublication).All(item => item.Marker != first.PayloadMarker);
                state.Observe("old-payload-rejected", new { operation, oldNodesAbsent, oldImageAbsent, first.PayloadMarker });
                state.Check(oldNodesAbsent && oldImageAbsent && state.Backend.MaximumActiveShotDump <= 1, "旧冻结Shot或Dump按其真实payload标记从未发布，最终身份单加载器串行完成"); intent.Same(state);
            });
            await Case("self-geometry-normal", true, async state =>
            {
                var before = state.A.Info; state.Backend.ChangeOnNextDiscover(before with { Width = 180, Height = 260, Rotation = 1 });
                var shot = state.Backend.Hold("Screenshot", state.A.Serial); state.Navigate("节点"); await state.Entered(shot); var final = state.A.Info; shot.Release.TrySetResult(); await state.Loaded(final); state.Sequence(0, "Screenshot", "Dump");
                state.Check(final.Generation == before.Generation + 1 && final.Width == 180 && final.Height == 260 && final.Rotation == 1, "自身Discover几何预处理形成合法新基线而非重复首载"); await state.Sync(); await state.Settled(); state.Sequence(0, "Screenshot", "Dump");
            });
            foreach (var cancel in new[] { false, true }) await Case(cancel ? "self-geometry-cancel-no-loop" : "self-geometry-failure-no-loop", true, async state =>
            {
                state.Backend.ChangeOnNextDiscover(state.A.Info with { Width = 180, Height = 260 });
                Exception injected = cancel ? new OperationCanceledException("own reconnect cancellation") : new DeviceException("own reconnect failure", 409, 1003);
                var gate = state.Backend.Hold(cancel ? "Dump" : "Screenshot", state.A.Serial, injected); state.Navigate("节点"); await state.Entered(gate); gate.Release.TrySetResult(); await state.Settled();
                state.FailedFeedback(injected); state.Sequence(0, cancel ? new[] { "Screenshot", "Dump" } : new[] { "Screenshot" }); var reads = state.Backend.ReadCount;
                await state.Sync(); await state.Sync(); await state.Settled(); state.NoReads(reads);
                state.Navigate("设备墙 / 主控"); var retry = state.Backend.Hold("Screenshot", state.A.Serial); state.Navigate("节点"); await state.Entered(retry); retry.Release.TrySetResult(); await state.Loaded(state.A.Info);
            });
            await Case("unavailable-and-no-current", true, async state =>
            {
                await state.InitialLoad(); var intent = ReconnectIntent.Take(state); var reads = state.Backend.ReadCount;
                foreach (var unavailable in new[] { "Unknown", "Unauthorized", "Offline" })
                {
                    state.Backend.Change(state.A.Serial, state.Backend.Info(state.A.Serial) with { State = unavailable }); await state.Sync();
                    state.Check(state.A.Info.State == unavailable && !state.A.Info.Online && !state.Window.debugAutoLoading && !state.Window.debugAutoQueued && !state.Window.debugAutoPending, "实际非Ready状态不产生新读/待办：" + unavailable); state.NoReads(reads); intent.Same(state);
                }
                state.Vm.CurrentDevice = null; await state.Settled(); state.Check(state.Vm.CurrentDevice is null && state.Vm.Nodes.Count == 0, "真实Current=null保持空检查，不用Demo完整快照的自动选首行覆盖此前置"); state.NoReads(reads);
            });
            await Case("pending-leave-return", true, async state =>
            {
                await state.InitialLoad(); var blocker = await state.Block("busy"); var reads = state.Backend.ReadCount; await state.ChangeWhileBlocked();
                state.Navigate("设备墙 / 主控"); await blocker.Release(); await state.Settled(); state.NoReads(reads);
                var shot = state.Backend.Hold("Screenshot", state.A.Serial); state.Navigate("节点"); await state.Entered(shot); shot.Release.TrySetResult(); await state.Loaded(state.A.Info); state.Sequence(reads, "Screenshot", "Dump");
            });
            await Case("close-pending-script", true, async state =>
            {
                await state.InitialLoad(); state.SelectPeerOnly(); await state.Block("script"); var reads = state.Backend.ReadCount; await state.ChangeWhileBlocked(); state.PendingOnly();
                var confirmations = 0; state.Vm.Confirm = (_, _) => { confirmations++; return true; }; state.Window.Close(); await state.Closed.Task.WaitAsync(TimeSpan.FromSeconds(5));
                state.ShutdownByWindow = state.Window.closing && state.Window.closeApproved;
                state.Check(state.ShutdownByWindow && confirmations == 1 && state.Scripts.GetStatus(state.B.Serial).State == ScriptRunState.Cancelled, "真实自有Window.Close/Closing停止B脚本并完成单次Shutdown"); state.NoReads(reads);
            });
            await Case("preflight-adopts-external-pending", true, async state =>
            {
                var original = state.A.Info; var changed = original with { TransportId = original.TransportId + 1 }; state.Backend.Change(state.A.Serial, changed);
                var discover = state.Backend.Hold("Discover", null); var injected = new DeviceException("own adopted pending failure", 409, 1003); var shot = state.Backend.Hold("Screenshot", state.A.Serial, injected);
                await state.Window.Dispatcher.InvokeAsync(() => state.Navigate("节点"), DispatcherPriority.Background).Task.WaitAsync(TimeSpan.FromSeconds(5)); await state.Entered(discover);
                var discovers = state.Backend.Calls().Count(call => call.Operation == "Discover"); long externalCompletedAt = 0;
                async Task ExternalSync() { await state.Vm.RefreshDeviceSnapshotAsync(); externalCompletedAt = Stopwatch.GetTimestamp(); state.Observe("external-sync-completed", new { externalCompletedAt, state.Window.debugAutoPending }); }
                var external = state.Track(ExternalSync());
                state.Precondition(!external.IsCompleted && state.Backend.Calls().Count(call => call.Operation == "Discover") == discovers, "外部真实Snapshot受Catalog semaphore阻塞，未伪称Discover hold期间完成"); state.Observe("external-sync-waiting-catalog");
                discover.Release.TrySetResult(); await external.WaitAsync(TimeSpan.FromSeconds(5)); await state.Entered(shot);
                state.Observe("adoption-order", new { externalCompletedAt, shot.AdmittedAt, source = shot.Source, expectedGeneration = original.Generation + 1 });
                state.Precondition(externalCompletedAt > 0 && externalCompletedAt < shot.AdmittedAt && shot.Source is { } source && source.Generation == original.Generation + 1 && source.TransportId == changed.TransportId && state.A.Info.Generation == source.Generation, "Normal外部Sync实际先完成，Background自身预处理随后确实采用同g2；不是依赖未成立的调度假设");
                shot.Release.TrySetResult(); await state.Settled(); state.FailedFeedback(injected); state.Sequence(0, "Screenshot");
                await state.Sync(); await state.Settled(); state.Sequence(0, "Screenshot");
            });
            if (failures.Count > 0) throw new AggregateException("同row恢复首载专项失败", failures);
        }
        catch (Exception error) { failure = error; Console.Error.WriteLine("RECONNECT-PRIMARY " + error); }
        finally
        {
            try { Application.Current.MainWindow = originalMain; Application.Current.ShutdownMode = originalShutdown; LocalizationService.Current.SetLanguage(language); }
            catch (Exception restoreError) { failure = failure is null ? restoreError : new AggregateException(failure, restoreError); }
            try { await File.WriteAllTextAsync(Path.Combine(directory, "reconnected-loading.json"), JsonSerializer.Serialize(new { status = failure is null ? "PASS-OFFLINE-WPF-RECONNECTED-LOADING" : "FAIL", scope = "Own Demo/STA windows; actual navigation and VM/Catalog sync, frozen backend payloads and TCS. Not Android, physical input, production timers, native dialogs or runtime hot replacement.", caseCount = reports.Count, reports, error = failure?.ToString() }, JsonDefaults.Options)); }
            catch (Exception reportError) { Console.Error.WriteLine(reportError); failure = failure is null ? reportError : new AggregateException(failure, reportError); }
        }
        if (failure is not null) ExceptionDispatchInfo.Capture(failure).Throw();

        async Task Case(string name, bool permit, Func<ReconnectLoadingScope, Task> action)
        {
            ReconnectLoadingScope? state = null; Exception? error = null; var cleaned = false;
            try { state = await ReconnectLoadingScope.Create(this, Path.Combine(directory, "c" + (++number).ToString("00")), permit); await action(state); state.Observe("case-completed"); }
            catch (Exception caught) { error = caught; state?.Observe("case-failed", new { type = caught.GetType().Name, caught.Message }); Console.Error.WriteLine("RECONNECT-CASE " + name + " " + caught); }
            finally
            {
                if (state is not null) try { await state.Cleanup(); cleaned = true; } catch (Exception cleanupError) { error = error is null ? cleanupError : new AggregateException(error, cleanupError); }
                reports.Add(new { name, status = error is null && cleaned ? "PASS" : "FAIL", preconditionFailure = error is ReconnectPreconditionException, cleaned, checks = state?.Checks.ToArray(), events = state?.Events.ToArray(), scriptEvents = state?.ScriptEvents.ToArray(), calls = state?.Backend.Calls(), payloads = state?.Backend.Payloads(), gates = state?.Backend.GateStates(), maximumActiveShotDump = state?.Backend.MaximumActiveShotDump, originalTasks = state?.TaskStates(), published = state?.Published.ToArray(), publishedImages = state?.PublishedImages.ToArray(), failure = error?.ToString() });
            }
            if (error is not null) { failures.Add(new InvalidOperationException(name, error)); if (!cleaned) throw new AggregateException("未完成自有夹具清理，停止接纳后续case", failures); }
        }
    }

    private sealed class ReconnectPreconditionException(string message) : Exception(message);
    private static int? ReconnectId(object? value) => value is null ? null : System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(value);
    private static bool ReconnectSame(DeviceInfo actual, DeviceInfo expected) => actual.Serial == expected.Serial && actual.Generation == expected.Generation && actual.TransportId == expected.TransportId && actual.Width == expected.Width && actual.Height == expected.Height && actual.Rotation == expected.Rotation && actual.State == expected.State;
    private sealed record ReconnectPublished(long At, string? Marker);
    private sealed record ReconnectImagePublished(long At, int ImageId, int Width, int Height, byte[] FirstBgra, string? Marker);
    private sealed record ReconnectIntent(DeviceRow? Current, DeviceRow? Main, DeviceRow[] Rows, bool[] Selected, UnifiedDeviceRow[] Unified, string[] Keys, bool[] UnifiedSelected, DeviceRow[] Targets, PixelSample[] Colors, long InputGeneration)
    {
        internal static ReconnectIntent Take(ReconnectLoadingScope state) => new(state.Vm.CurrentDevice, state.Vm.MainDevice, state.Vm.Devices.ToArray(), state.Vm.Devices.Select(row => row.IsSelected).ToArray(), state.Window.unifiedDevices.ToArray(), state.Window.unifiedDevices.Select(row => row.Key).ToArray(), state.Window.unifiedDevices.Select(row => row.IsSelected).ToArray(), state.Vm.Targets().ToArray(), state.Vm.Colors.ToArray(), state.Input.CurrentGeneration);
        internal void Same(ReconnectLoadingScope state)
        {
            var current = ReferenceEquals(Current, state.Vm.CurrentDevice); var main = ReferenceEquals(Main, state.Vm.MainDevice); var rows = Rows.SequenceEqual(state.Vm.Devices); var selected = Selected.SequenceEqual(state.Vm.Devices.Select(row => row.IsSelected));
            var unified = Unified.SequenceEqual(state.Window.unifiedDevices); var keys = Keys.SequenceEqual(state.Window.unifiedDevices.Select(row => row.Key)); var unifiedSelected = UnifiedSelected.SequenceEqual(state.Window.unifiedDevices.Select(row => row.IsSelected));
            var gated = Rows.Where((row, index) => Selected[index] && !row.Info.Online).ToArray(); var expected = gated.Length > 0 ? Array.Empty<DeviceRow>() : Targets; var targets = expected.SequenceEqual(state.Vm.Targets()); var colors = Colors.SequenceEqual(state.Vm.Colors); var generation = InputGeneration == state.Input.CurrentGeneration;
            state.Observe("stable-intent", new { current, main, rows, selected, unified, keys, unifiedSelected, targets, colors, generation, gate = gated.Select(row => row.Serial).ToArray(), expected = expected.Select(row => row.Serial).ToArray(), actual = state.Vm.Targets().Select(row => row.Serial).ToArray() });
            state.Check(current && main && rows && selected && unified && keys && unifiedSelected && targets && colors && generation && (gated.Length == 0 || !state.Vm.CanWriteTargets()), "Current/Main/原行引用顺序/key/勾选/色值/输入代次保持，有效目标遵循离线门禁");
        }
    }
    private sealed class ReconnectLoadingScope
    {
        internal required string Directory; internal required MainWindow Window; internal required WorkspaceViewModel Vm; internal required MatrixRuntime Runtime; internal required ReconnectBackend Backend; internal required DeviceCatalog Catalog; internal required AutomationService Automation; internal required InputCoordinator Input; internal required ScriptScheduler Scripts; internal required ApiServiceManager Api;
        internal readonly List<string> Checks = []; internal readonly Queue<object> Events = []; internal readonly System.Collections.Concurrent.ConcurrentQueue<object> ScriptEvents = new(); internal readonly List<ReconnectPublished> Published = []; internal readonly List<ReconnectImagePublished> PublishedImages = []; internal readonly TaskCompletionSource Closed = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly List<Task> originals = []; private readonly List<TaskCompletionSource> releases = []; private PropertyChangedEventHandler? observer; private System.Collections.Specialized.NotifyCollectionChangedEventHandler? nodesObserver; private Action<ScriptRunStatus>? scriptObserver; private EventHandler? closedObserver; private DispatcherHookEventHandler? startedObserver; private DispatcherHookEventHandler? finishedObserver; private readonly Stack<DispatcherPriority> priorities = []; private bool cleanupStarted;
        internal bool ShutdownByWindow; internal DeviceRow A => Vm.Devices.Single(row => row.Serial == "DEMO-001"); internal DeviceRow B => Vm.Devices.Single(row => row.Serial == "DEMO-002");
        internal bool Matches(MainWindow window) => ReferenceEquals(window, Window) && ReferenceEquals(window.ViewModel, Vm) && Vm.Demo && Runtime.IsDemo && ReferenceEquals(Vm.Runtime, Runtime) && ReferenceEquals(Runtime.Backend, Backend) && ReferenceEquals(Runtime.Devices, Catalog) && ReferenceEquals(Runtime.Automation, Automation) && ReferenceEquals(Runtime.Input, Input) && ReferenceEquals(Runtime.Scripts, Scripts) && ReferenceEquals(Runtime.Api, Api) && !Api.BasicRunning && !Api.AutomatorRunning && Api.DevicePorts.Count == 0 && Path.IsPathFullyQualified(Directory) && Directory.Length <= 200 && Directory.Contains(Path.DirectorySeparatorChar + "artifacts" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) && window.Dispatcher.CheckAccess() && Thread.CurrentThread.GetApartmentState() == ApartmentState.STA;
        internal static async Task<ReconnectLoadingScope> Create(MainWindow owner, string directory, bool permit)
        {
            System.IO.Directory.CreateDirectory(directory); var backend = new ReconnectBackend(); RollingLog? log = null; CatalogRepository? metadata = null; MatrixRuntime? runtime = null; WorkspaceViewModel? vm = null; MainWindow? window = null; ReconnectLoadingScope? state = null; ScriptScheduler? scripts = null; InputCoordinator? input = null; ApiServiceManager? api = null; Task? initializing = null;
            try
            {
                log = new RollingLog(Path.Combine(directory, "logs")); metadata = new CatalogRepository(Path.Combine(directory, "db.sqlite3")); var catalog = new DeviceCatalog(backend); var automation = new AutomationService(backend, catalog); scripts = new ScriptScheduler(backend); input = new InputCoordinator(catalog, backend); api = new ApiServiceManager(catalog, backend, automation, log);
                var settings = new AppSettings { AutoStartApi = false, AutoConnect = false, SyncMouse = false, SyncKeyboard = false, Language = "zh-CN", WindowWidth = 1000, WindowHeight = 700 };
                runtime = new MatrixRuntime { Backend = backend, Devices = catalog, Automation = automation, Scripts = scripts, Input = input, Api = api, Log = log, Metadata = metadata, Settings = settings, FrozenConnectionSettings = settings, SettingsStore = new SettingsStore(Path.Combine(directory, "AppConfig.json")), IsDemo = true };
                api.DeviceSnapshotProvider = runtime.GetDevices; backend.CatalogSnapshot = () => catalog.Snapshot; catalog.DeviceDisconnected += serial => scripts.Stop(serial);
                vm = new WorkspaceViewModel(runtime, directory, true); initializing = vm.RefreshDevicesAsync(); await initializing.WaitAsync(TimeSpan.FromSeconds(5));
                window = new MainWindow(vm) { Owner = owner, ShowActivated = false }; window.captureTimer.Stop(); window.directoryTimer.Stop(); window.monitorTimer.Stop();
                state = new ReconnectLoadingScope { Directory = directory, Window = window, Vm = vm, Runtime = runtime, Backend = backend, Catalog = catalog, Automation = automation, Input = input, Scripts = scripts, Api = api };
                state.observer = (_, args) => { state.RememberAuto(); if (args.PropertyName == nameof(WorkspaceViewModel.DebugImage) && vm.DebugImage is { } image) { var pixel = Pixel(image, 0, 0); var marker = backend.Payloads().LastOrDefault(payload => payload.Operation == "Screenshot" && payload.FirstBgra is { } first && first.SequenceEqual(pixel))?.Marker; state.PublishedImages.Add(new(Stopwatch.GetTimestamp(), ReconnectId(image)!.Value, image.PixelWidth, image.PixelHeight, pixel, marker)); } state.Observe("vm:" + args.PropertyName); }; vm.PropertyChanged += state.observer;
                state.nodesObserver = (_, _) => { if (vm.Nodes.Count > 0) state.Published.Add(new(Stopwatch.GetTimestamp(), vm.Nodes.First().Source.Text)); state.Observe("nodes"); }; vm.Nodes.CollectionChanged += state.nodesObserver;
                state.scriptObserver = status => { state.ScriptEvents.Enqueue(new { at = Stopwatch.GetTimestamp(), status.Serial, state = status.State.ToString(), status.PackageId, status.Error }); while (state.ScriptEvents.Count > 64) state.ScriptEvents.TryDequeue(out _); }; scripts.StatusChanged += state.scriptObserver;
                state.closedObserver = (_, _) => state.Closed.TrySetResult(); window.Closed += state.closedObserver;
                state.startedObserver = (_, args) => state.priorities.Push(args.Operation.Priority); state.finishedObserver = (_, _) => { if (state.priorities.Count > 0) state.priorities.Pop(); };
                window.Dispatcher.Hooks.OperationStarted += state.startedObserver; window.Dispatcher.Hooks.OperationCompleted += state.finishedObserver; window.Dispatcher.Hooks.OperationAborted += state.finishedObserver;
                if (permit) window.reconnectLoadingScope = state; window.Show(); await state.Idle();
                state.Check(state.Matches(window) && window.ReconnectLoadingFixturePermitted == permit && !window.DebugAutomaticFixturePermitted && backend.ReadCount == 0 && vm.Devices.Count == 2, "独立Demo一致服务/短artifacts/STA/API未启；仅本自有scope可授权，默认保护未绕过");
                await state.Track(vm.SetMainAsync(state.B)).WaitAsync(TimeSpan.FromSeconds(5)); vm.CurrentDevice = state.A; state.A.IsSelected = state.B.IsSelected = true; await state.Idle(); backend.ClearTrace(); state.Observe("created"); return state;
            }
            catch (Exception error)
            {
                var errors = new List<Exception>();
                if (state is not null) { try { await state.Cleanup(); } catch (Exception cleanupError) { errors.Add(cleanupError); } }
                else
                {
                    try { if (window is not null) { LocalizationService.Current.LanguageChanged -= window.OnLanguageChanged; window.AbortStartup(); } } catch (Exception cleanupError) { errors.Add(cleanupError); }
                    if (initializing is not null) await Attempt(() => initializing.WaitAsync(TimeSpan.FromSeconds(5)));
                    if (vm is not null) await Attempt(() => vm.ShutdownAsync().WaitAsync(TimeSpan.FromSeconds(5)));
                    else if (runtime is not null) await Attempt(() => runtime.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(5)));
                    else
                    {
                        if (api is not null) await Attempt(() => api.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(5))); if (input is not null) await Attempt(() => input.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(5))); if (scripts is not null) await Attempt(() => scripts.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(5))); await Attempt(() => backend.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(5)));
                        try { metadata?.Dispose(); } catch (Exception cleanupError) { errors.Add(cleanupError); } if (log is not null) await Attempt(() => log.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(5)));
                    }
                }
                if (errors.Count > 0) throw new AggregateException("恢复首载初始化与清理失败", new[] { error }.Concat(errors)); throw;
                async Task Attempt(Func<Task> action) { try { await action(); } catch (Exception cleanupError) { errors.Add(cleanupError); } }
            }
        }
        internal Task Track(Task task) { if (!originals.Contains(task)) originals.Add(task); return task; }
        private void RememberAuto() { if (Window.debugAutoOperation != Task.CompletedTask) Track(Window.debugAutoOperation); }
        internal object[] TaskStates() => originals.Select(task => (object)new { id = ReconnectId(task), status = task.Status.ToString(), task.IsCompleted, task.IsCanceled, task.IsFaulted }).ToArray();
        internal void Navigate(string tag) { Window.navigation[tag].RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent)); Check(Equals((Window.tabs.SelectedItem as TabItem)?.Tag, tag), "真实导航Click到原Tag：" + tag); RememberAuto(); Observe("navigate:" + tag); }
        internal async Task Idle() { RememberAuto(); await Window.Dispatcher.InvokeAsync(Window.UpdateLayout, DispatcherPriority.ApplicationIdle).Task.WaitAsync(TimeSpan.FromSeconds(5)); await Window.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ContextIdle).Task.WaitAsync(TimeSpan.FromSeconds(5)); RememberAuto(); }
        internal async Task Sync() { var task = Track(Vm.RefreshDeviceSnapshotAsync()); await task.WaitAsync(TimeSpan.FromSeconds(5)); await Idle(); Observe("full-vm-snapshot-completed"); }
        internal async Task Settled()
        {
            var watch = Stopwatch.StartNew();
            do { await Idle(); if (!Window.debugAutoQueued && !Window.debugAutoLoading && !Window.debugAutoPending && !Vm.Busy && Window.debugAutoOperation.IsCompleted && originals.All(task => task.IsCompleted)) { Observe("settled"); return; } await Task.Delay(5); } while (watch.Elapsed < TimeSpan.FromSeconds(5));
            Observe("settle-timeout", new { tasks = TaskStates() }); throw new TimeoutException($"恢复首载未排空：queued={Window.debugAutoQueued}; loading={Window.debugAutoLoading}; pending={Window.debugAutoPending}; Busy={Vm.Busy}; operation={Window.debugAutoOperation.Status}");
        }
        internal async Task Entered(ReconnectGate gate) { try { await gate.Entered.Task.WaitAsync(TimeSpan.FromSeconds(5)); } catch (Exception error) { Observe("gate-not-entered", new { gate.Operation, gate.Serial, gate.Claimed, tasks = TaskStates() }); throw new InvalidOperationException("期望的实际后端门未进入：" + gate.Operation + ":" + gate.Serial, error); } RememberAuto(); Observe("gate-entered", new { gate.Operation, gate.Serial, gate.AdmittedAt, gate.Source, gate.PayloadMarker }); Check(gate.Claimed && gate.Entered.Task.IsCompletedSuccessfully, "后端实际调用已进入TCS：" + gate.Operation); }
        internal async Task InitialLoad() { var shot = Backend.Hold("Screenshot", A.Serial); Navigate("节点"); await Entered(shot); shot.Release.TrySetResult(); await Loaded(A.Info); Vm.SelectedNode = Vm.FlattenNodes().First(row => row.Source.Clickable); Vm.SamplePixel(3, 4); await Idle(); Sequence(0, "Screenshot", "Dump"); }
        internal async Task ChangeAndLoad(DeviceInfo info) { var shot = Backend.Hold("Screenshot", A.Serial); Backend.Change(A.Serial, info); await Sync(); var final = A.Info; await Entered(shot); shot.Release.TrySetResult(); await Loaded(final); }
        internal async Task ChangeWhileBlocked() { Backend.Change(A.Serial, A.Info with { TransportId = A.Info.TransportId + 1 }); await Sync(); Backend.Change(A.Serial, A.Info with { Width = A.Info.Width + 10, Height = A.Info.Height + 10 }); await Sync(); }
        internal async Task Loaded(DeviceInfo expected)
        {
            await Settled(); var row = Vm.CurrentDevice; var current = ReferenceEquals(row, A); var identity = row is not null && ReconnectSame(row.Info, expected) && Catalog.Snapshot.Any(info => ReconnectSame(info, expected)); var image = row?.Screenshot is { } && ReferenceEquals(Vm.DebugImage, row.Screenshot); var root = Vm.Nodes.Count == 1 && Vm.Nodes.Single().Source.Text == Backend.LastDump(A.Serial); var feedback = !Vm.Busy && Vm.Error.Length == 0;
            Observe("loaded-components", new { current, identity, image, root, feedback, expected }); Check(current && identity && image && root && feedback && Backend.MaximumActiveShotDump <= 1, "最终同row身份/图/节点标记/反馈各叶成立且无并发首载"); Projection();
            var payload = Backend.LastShot(A.Serial); Check(payload is not null && Vm.DebugImage!.PixelWidth == expected.Width && Vm.DebugImage.PixelHeight == expected.Height && Pixel(Vm.DebugImage, 0, 0).SequenceEqual(payload.FirstBgra!), "实际动态图像宽高与冻结payload首像素一致");
        }
        internal void Projection() => Check(ReferenceEquals(Window.screenshotImage.Source, Vm.DebugImage) && Window.nodeTree.Items.Count == Vm.Nodes.Count && Window.nodeTreeItems.Count == Vm.FlattenNodes().Count() && Window.nodeRectangles.Count == Vm.FlattenNodes().Count(node => node.Source.Bounds.Width > 0 && node.Source.Bounds.Height > 0) && Window.nodeCanvas.Children.OfType<Image>().Any(image => ReferenceEquals(image.Source, Vm.DebugImage)), "原窗口回调建立真实Tree/Canvas及截屏Source投影");
        private static byte[] Pixel(BitmapSource image, int x, int y) { var converted = new FormatConvertedBitmap(image, PixelFormats.Bgra32, null, 0); var value = new byte[4]; converted.CopyPixels(new Int32Rect(x, y, 1, 1), value, 4, 0); return value; }
        internal void SelectPeerOnly() { A.IsSelected = false; B.IsSelected = true; Vm.UpdateSelection(); }
        internal void NoReads(int start) => Check(Backend.ReadCount == start, "没有额外Screenshot/Dump；expected=" + start + "; actual=" + Backend.ReadCount);
        internal void Sequence(int start, params string[] expected) { var actual = Backend.Calls().Where(call => call.Operation is "Screenshot" or "Dump").Skip(start).ToArray(); Check(actual.Select(call => call.Operation).SequenceEqual(expected) && actual.All(call => call.Serial == A.Serial), "真实读序列只针对A；expected=" + string.Join(",", expected) + "; actual=" + string.Join(",", actual.Select(call => call.Operation + ":" + call.Serial))); }
        internal void PendingOnly() { Observe("pending-components", new { Window.debugAutoPending, Window.debugAutoQueued, Window.debugAutoLoading, Vm.HasActiveOperations, Vm.Busy, Vm.Transferring }); Check(Window.debugAutoPending && !Window.debugAutoQueued && !Window.debugAutoLoading && Vm.HasActiveOperations, "阻塞期间只保留最终身份pending，不越门禁接纳读"); }
        internal void FailedFeedback(Exception injected) { var cancelled = injected is OperationCanceledException; Observe("failure-feedback", new { cancelled, Vm.Status, Vm.Error, nodes = Vm.Nodes.Count }); Check(Vm.Nodes.Count == 0 && !Vm.Busy && (cancelled ? Vm.Error.Length == 0 && Vm.Status.Contains("已停止", StringComparison.Ordinal) : Vm.Error == "刷新截图与节点失败：" + UiErrorPresentation.Describe(injected) && Vm.Status == "操作失败"), "失败/取消使用原安全反馈且不把空节点冒充可用"); }
        internal async Task<ReconnectBlocker> Block(string kind)
        {
            Task task; TaskCompletionSource? release = null; ReconnectGate? gate = null;
            switch (kind)
            {
                case "busy": var busyRelease = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously); release = busyRelease; releases.Add(busyRelease); var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously); releases.Add(entered); task = Track(Vm.ExecuteAsync("自有恢复首载阻塞", async () => { entered.TrySetResult(); await busyRelease.Task; })); await entered.Task.WaitAsync(TimeSpan.FromSeconds(5)); break;
                case "external": Vm.SetExternalBusy(true); task = Task.CompletedTask; break;
                case "transfer": var path = Path.Combine(Directory, "own.txt"); await File.WriteAllTextAsync(path, "own Demo transfer"); gate = Backend.Hold("Push", B.Serial); Vm.QueuePush(path, false); task = Track(Vm.StartTransfersAsync()); await Entered(gate); break;
                case "script": gate = Backend.Hold("StartApp", B.Serial); task = Track(Scripts.RunAsync(B.Serial, new ScriptPackage { Manifest = new() { Id = "own-reconnect", Name = "自有恢复首载阻塞", Version = "1.0" }, Steps = [new() { Action = "launch", Value = "com.matrix.demo" }] })); await Entered(gate); break;
                default: throw new ArgumentOutOfRangeException(nameof(kind));
            }
            _ = Track(task); Check(Vm.HasActiveOperations, "真实全局活动阻塞成立：" + kind); Observe("block:" + kind);
            return new ReconnectBlocker(async () => { if (kind == "external") Vm.SetExternalBusy(false); release?.TrySetResult(); gate?.Release.TrySetResult(); await task.WaitAsync(TimeSpan.FromSeconds(5)); Observe("release:" + kind); });
        }
        internal void Precondition(bool value, string message) { if (!value) { Observe("precondition-failed", new { message }); throw new ReconnectPreconditionException(message); } Checks.Add("PRECONDITION " + message); }
        internal void Check(bool value, string message) { if (!value) { Observe("assertion-failed", new { message }); throw new InvalidOperationException(message); } Checks.Add(message); }
        internal void Observe(string stage, object? detail = null)
        {
            if (Events.Count == 256) Events.Dequeue();
            Events.Enqueue(new { stage, at = Stopwatch.GetTimestamp(), detail, dispatcherPriority = Window.Dispatcher.CheckAccess() && priorities.Count > 0 ? priorities.Peek().ToString() : "outside-dispatch-operation", current = Vm.CurrentDevice?.Serial, currentId = ReconnectId(Vm.CurrentDevice), main = Vm.MainDevice?.Serial, Vm.Busy, Vm.HasActiveOperations, Vm.Transferring, Vm.Status, Vm.Error, pending = Window.debugAutoPending, loading = Window.debugAutoLoading, queued = Window.debugAutoQueued, automaticTask = new { id = ReconnectId(Window.debugAutoOperation), status = Window.debugAutoOperation.Status.ToString() }, page = (Window.tabs.SelectedItem as TabItem)?.Tag as string, imageId = ReconnectId(Vm.DebugImage), nodes = Vm.Nodes.Count, marker = Vm.Nodes.FirstOrDefault()?.Source.Text, rows = Vm.Devices.Select(row => new { row.Serial, id = ReconnectId(row), row.Info, row.IsSelected, image = ReconnectId(row.Screenshot), row.ScreenState, row.AutomationState }).ToArray(), catalog = Catalog.Snapshot.ToArray() });
        }
        internal async Task Cleanup()
        {
            if (cleanupStarted) throw new InvalidOperationException("自有恢复首载scope不得重复清理"); cleanupStarted = true; var errors = new List<Exception>(); RememberAuto();
            Window.reconnectLoadingScope = null;
            if (!ShutdownByWindow) try { Window.AbortStartup(); } catch (Exception error) { errors.Add(error); }
            Backend.ReleaseAll(); foreach (var release in releases) release.TrySetResult(); Vm.SetExternalBusy(false); foreach (var row in Vm.Devices) Scripts.Stop(row.Serial); RememberAuto();
            try { await Task.WhenAll(originals).WaitAsync(TimeSpan.FromSeconds(5)); } catch (Exception error) { errors.Add(error); }
            try { Check(originals.All(task => task.IsCompleted) && releases.All(item => item.Task.IsCompleted) && Backend.GatesDrained && Backend.ActiveShotDump == 0 && !Window.debugAutoPending && !Window.debugAutoLoading && !Window.debugAutoQueued && Window.debugAutoOperation.IsCompleted && !Vm.Busy, "撤自有许可后全部原Task/Entered/Release实际排空，未把撤许可算产品通过"); Observe("cleanup-drained", new { tasks = TaskStates() }); } catch (Exception error) { errors.Add(error); }
            if (observer is not null) Vm.PropertyChanged -= observer; if (nodesObserver is not null) Vm.Nodes.CollectionChanged -= nodesObserver; if (scriptObserver is not null) Scripts.StatusChanged -= scriptObserver; if (closedObserver is not null) Window.Closed -= closedObserver;
            if (startedObserver is not null) Window.Dispatcher.Hooks.OperationStarted -= startedObserver; if (finishedObserver is not null) { Window.Dispatcher.Hooks.OperationCompleted -= finishedObserver; Window.Dispatcher.Hooks.OperationAborted -= finishedObserver; }
            LocalizationService.Current.LanguageChanged -= Window.OnLanguageChanged;
            if (!ShutdownByWindow) try { await Vm.ShutdownAsync().WaitAsync(TimeSpan.FromSeconds(5)); } catch (Exception error) { errors.Add(error); }
            if (errors.Count > 0) throw new AggregateException("恢复首载自有清理失败", errors);
        }
    }
    private sealed class ReconnectBlocker(Func<Task> release) { internal Task Release() => release(); }
    private sealed record ReconnectCall(int Sequence, string Operation, string? Serial, long At, DeviceInfo? Source);
    private sealed record ReconnectPayload(int Call, string Operation, string? Serial, DeviceInfo[]? Discovery, DeviceInfo? Source, string Marker, string? Sha256, byte[]? FirstBgra);
    private sealed class ReconnectGate(string operation, string? serial, Exception? failure)
    {
        internal readonly string Operation = operation; internal readonly string? Serial = serial; internal readonly Exception? Failure = failure; internal bool Claimed; internal long AdmittedAt; internal DeviceInfo? Source; internal string? PayloadMarker;
        internal readonly TaskCompletionSource Entered = new(TaskCreationOptions.RunContinuationsAsynchronously); internal readonly TaskCompletionSource Release = new(TaskCreationOptions.RunContinuationsAsynchronously);
    }
    private sealed class ReconnectBackend : IDeviceBackend
    {
        private readonly DemoDeviceBackend inner = new(); private readonly object sync = new(); private readonly List<ReconnectCall> calls = []; private readonly List<ReconnectPayload> payloads = []; private readonly List<ReconnectGate> gates = []; private int activeShotDump; private int frame; private DeviceInfo? nextDiscovery;
        private readonly Dictionary<string, DeviceInfo> devices = new(StringComparer.Ordinal) { ["DEMO-001"] = new() { Serial = "DEMO-001", Name = "恢复A", State = "Ready", Width = 160, Height = 240, Rotation = 0, TransportId = 11 }, ["DEMO-002"] = new() { Serial = "DEMO-002", Name = "阻塞B", State = "Ready", Width = 160, Height = 240, Rotation = 0, TransportId = 22 } };
        internal Func<IReadOnlyList<DeviceInfo>>? CatalogSnapshot; internal int MaximumActiveShotDump { get; private set; }
        internal ReconnectCall[] Calls() { lock (sync) return calls.ToArray(); } internal ReconnectPayload[] Payloads() { lock (sync) return payloads.ToArray(); }
        internal int ReadCount { get { lock (sync) return calls.Count(call => call.Operation is "Screenshot" or "Dump"); } } internal int ActiveShotDump { get { lock (sync) return activeShotDump; } }
        internal bool GatesDrained { get { lock (sync) return gates.All(gate => gate.Entered.Task.IsCompleted && gate.Release.Task.IsCompleted); } }
        internal object[] GateStates() { lock (sync) return gates.Select(gate => (object)new { gate.Operation, gate.Serial, gate.Claimed, entered = gate.Entered.Task.Status.ToString(), released = gate.Release.Task.Status.ToString(), gate.AdmittedAt, gate.Source, gate.PayloadMarker }).ToArray(); }
        internal DeviceInfo Info(string serial) { lock (sync) return devices[serial]; } internal void Change(string serial, DeviceInfo info) { lock (sync) devices[serial] = info; } internal void ChangeOnNextDiscover(DeviceInfo info) { lock (sync) nextDiscovery = info; }
        internal string? LastDump(string serial) { lock (sync) return payloads.LastOrDefault(payload => payload.Operation == "Dump" && payload.Serial == serial)?.Marker; } internal ReconnectPayload? LastShot(string serial) { lock (sync) return payloads.LastOrDefault(payload => payload.Operation == "Screenshot" && payload.Serial == serial); }
        internal void ClearTrace() { lock (sync) { calls.Clear(); payloads.Clear(); } } internal ReconnectGate Hold(string operation, string? serial, Exception? failure = null) { var gate = new ReconnectGate(operation, serial, failure); lock (sync) gates.Add(gate); return gate; }
        internal void ReleaseAll() { lock (sync) foreach (var gate in gates) { gate.Release.TrySetResult(); if (!gate.Claimed) gate.Entered.TrySetCanceled(); } }
        private async Task<T> Complete<T>(string operation, string? serial, Func<int, DeviceInfo?, T> make, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested(); var source = serial is null ? null : CatalogSnapshot?.Invoke().FirstOrDefault(info => info.Serial == serial); ReconnectGate? gate; int sequence; var admitted = Stopwatch.GetTimestamp(); var read = operation is "Screenshot" or "Dump";
            lock (sync) { sequence = calls.Count + 1; calls.Add(new(sequence, operation, serial, admitted, source)); gate = gates.FirstOrDefault(item => !item.Claimed && item.Operation == operation && item.Serial == serial); if (gate is not null) { gate.Claimed = true; gate.AdmittedAt = admitted; gate.Source = source; } if (read) { activeShotDump++; MaximumActiveShotDump = Math.Max(MaximumActiveShotDump, activeShotDump); } }
            try
            {
                var result = make(sequence, source);
                if (gate is not null) { lock (sync) gate.PayloadMarker = payloads.LastOrDefault(payload => payload.Call == sequence)?.Marker; gate.Entered.TrySetResult(); await gate.Release.Task.WaitAsync(ct); if (gate.Failure is not null) ExceptionDispatchInfo.Capture(gate.Failure).Throw(); }
                return result;
            }
            finally { if (read) lock (sync) activeShotDump--; }
        }
        public Task<IReadOnlyList<DeviceInfo>> DiscoverAsync(CancellationToken ct = default) => Complete<IReadOnlyList<DeviceInfo>>("Discover", null, (sequence, _) =>
        {
            lock (sync) { if (nextDiscovery is { } changed) { devices[changed.Serial] = changed; nextDiscovery = null; } var snapshot = devices.Values.ToArray(); payloads.Add(new(sequence, "Discover", null, snapshot, null, "discover:" + sequence, null, null)); return snapshot; }
        }, ct);
        public Task<byte[]> ScreenshotAsync(string serial, CancellationToken ct = default) => Complete("Screenshot", serial, (sequence, source) =>
        {
            var info = Info(serial); int marker; lock (sync) marker = ++frame; var red = (byte)(marker % 200 + 1); var green = (byte)(serial == "DEMO-001" ? 31 : 73);
            var png = PngImage.Create(info.Width, info.Height, (x, y) => (red, green, (byte)((x + y) % 251))); var text = $"frame:{marker}:{serial}:g{source?.Generation}:t{info.TransportId}:{info.Width}x{info.Height}:r{info.Rotation}";
            lock (sync) payloads.Add(new(sequence, "Screenshot", serial, null, source, text, Convert.ToHexString(SHA256.HashData(png)), [0, green, red, 255])); return png;
        }, ct);
        public Task<AndroidNode> DumpAsync(string serial, CancellationToken ct = default) => Complete("Dump", serial, (sequence, source) =>
        {
            var info = Info(serial); var marker = $"nodes:{serial}:{sequence}:g{source?.Generation}:t{info.TransportId}:{info.Width}x{info.Height}:r{info.Rotation}";
            var node = new AndroidNode { Text = marker, ResourceId = "fixture:root", Enabled = true, Bounds = new(0, 0, info.Width, info.Height), ChildNodeList = [new AndroidNode { Text = marker + ":child", ResourceId = "fixture:child", Clickable = true, Enabled = true, Bounds = new(5, 5, 50, 50) }] };
            lock (sync) payloads.Add(new(sequence, "Dump", serial, null, source, marker, null, null)); return node;
        }, ct);
        public async Task<string> StartAppAsync(string serial, string packageName, string? activity = null, CancellationToken ct = default) { await Complete("StartApp", serial, (_, _) => true, ct); return await inner.StartAppAsync(serial, packageName, activity, ct); }
        public async Task PushAsync(string serial, string localPath, string remotePath, IProgress<double>? progress = null, CancellationToken ct = default) { await Complete("Push", serial, (_, _) => true, ct); await inner.PushAsync(serial, localPath, remotePath, progress, ct); }
        public Task<string> ShellAsync(string serial, string command, bool useRoot = false, CancellationToken ct = default) => inner.ShellAsync(serial, command, useRoot, ct);
        public Task TapAsync(string serial, int x, int y, CancellationToken ct = default) => inner.TapAsync(serial, x, y, ct);
        public Task SwipeAsync(string serial, int x1, int y1, int x2, int y2, int durationMs, CancellationToken ct = default) => inner.SwipeAsync(serial, x1, y1, x2, y2, durationMs, ct);
        public Task KeyEventAsync(string serial, int code, CancellationToken ct = default) => inner.KeyEventAsync(serial, code, ct);
        public Task TextAsync(string serial, string text, CancellationToken ct = default) => inner.TextAsync(serial, text, ct);
        public Task<string> StopAppAsync(string serial, string packageName, CancellationToken ct = default) => inner.StopAppAsync(serial, packageName, ct);
        public Task<bool> ClearAppAsync(string serial, string packageName, CancellationToken ct = default) => inner.ClearAppAsync(serial, packageName, ct);
        public Task<IReadOnlyList<AppInfo>> ListAppsAsync(string serial, bool showSystem = false, bool needVersion = true, CancellationToken ct = default) => inner.ListAppsAsync(serial, showSystem, needVersion, ct);
        public Task InstallAsync(string serial, string localPath, CancellationToken ct = default) => inner.InstallAsync(serial, localPath, ct);
        public Task UninstallAsync(string serial, string packageName, CancellationToken ct = default) => inner.UninstallAsync(serial, packageName, ct);
        public Task PullAsync(string serial, string remotePath, string localPath, IProgress<double>? progress = null, CancellationToken ct = default) => inner.PullAsync(serial, remotePath, localPath, progress, ct);
        public Task SetNodeTextAsync(string serial, NodeSelector selector, string text, CancellationToken ct = default) => inner.SetNodeTextAsync(serial, selector, text, ct);
        public ValueTask DisposeAsync() { ReleaseAll(); return inner.DisposeAsync(); }
    }
}
