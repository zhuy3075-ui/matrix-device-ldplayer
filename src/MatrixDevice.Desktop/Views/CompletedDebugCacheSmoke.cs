using System.ComponentModel;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using MatrixDevice.Core;
using MatrixDevice.Features;
using MatrixDevice.Host;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    private async Task VerifyCompletedDebugCacheAsync()
    {
        if (!ViewModel.Demo || !ViewModel.Runtime.IsDemo || !Dispatcher.CheckAccess() || Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
            throw new InvalidOperationException("完成缓存验收仅限自有一致Demo/STA。");
        var directory = Path.Combine(Path.GetFullPath(ViewModel.DataDirectory), "artifacts", "cc", Guid.NewGuid().ToString("N")[..8]);
        if (!Path.IsPathFullyQualified(directory) || directory.Length > 190) throw new InvalidOperationException("完成缓存验收需要短绝对artifacts路径：" + directory.Length);
        Directory.CreateDirectory(directory);
        var originalMain = Application.Current.MainWindow; var originalShutdown = Application.Current.ShutdownMode; var language = LocalizationService.Current.Language;
        var reports = new List<object>(); var failures = new List<Exception>(); var number = 0; Exception? primary = null;
        try
        {
            foreach (var change in new[] { "transport", "geometry", "rotation" }) await Case("ready-" + change, async state =>
            {
                await state.Seed(); var before = CompletedCacheSnapshot.Take(state); var info = state.A.Info; var calls = state.Backend.Calls().Length;
                state.Backend.Change(state.A.Serial, change switch { "transport" => info with { TransportId = info.TransportId + 1 }, "geometry" => info with { Width = 180, Height = 260 }, _ => info with { Rotation = 1 } });
                await state.Sync(); state.NewReady(info);
                state.Check(change switch { "transport" => state.A.Info.TransportId == info.TransportId + 1, "geometry" => state.A.Info.Width == 180 && state.A.Info.Height == 260, _ => state.A.Info.Rotation == 1 }, "真实VM行已应用指定Ready身份字段：" + change);
                state.Cleared(state.A); await state.RejectExports("replaced"); before.ContextSame(state); state.NoReadsAfter(calls);
                await state.Recover(); state.Check(!ReferenceEquals(before.Image, state.Vm.DebugImage) && state.Vm.Nodes.Single().Source.Text != before.Marker, "新显式刷新只发布新身份内容而非旧引用/节点标记");
            });
            await Case("offline-history-reconnect", async state =>
            {
                await state.Seed(); var before = CompletedCacheSnapshot.Take(state); var info = state.A.Info; var calls = state.Backend.Calls().Length;
                state.Backend.Change(state.A.Serial, info with { State = "Offline" }); await state.Sync();
                state.Check(ReferenceEquals(state.A, before.Current) && state.A.Info.State == "Offline" && state.A.Info.Generation == info.Generation + 1 && state.SameTransportGeometry(state.A.Info, info), "真实目录只发生同源一次Offline转换且保留原行");
                before.InspectionSame(state); before.RowCacheSame(state); await state.ExportPair("offline", before.Image!, before.Marker!);
                var offline = state.A.Info; state.Backend.Change(state.A.Serial, state.Backend.Info(state.A.Serial) with { Name = "离线历史改名" }); await state.Sync();
                state.Check(state.A.Info.State == "Offline" && state.A.Info.Generation == offline.Generation && state.SameTransportGeometry(state.A.Info, offline), "重复Offline发现不再增代且源几何传输未变");
                before.InspectionSame(state); before.RowCacheSame(state); await state.ExportPair("offline-repeat", before.Image!, before.Marker!); state.NoReadsAfter(calls);
                state.Backend.Change(state.A.Serial, state.Backend.Info(state.A.Serial) with { State = "Ready" }); await state.Sync(); state.NewReady(info, 2); state.Cleared(state.A); before.ContextSame(state);
                await state.Recover(); await state.ExportPair("reconnected", state.Vm.DebugImage!, state.Vm.Nodes.Single().Source.Text!);
            });
            await Case("noncurrent-isolation", async state =>
            {
                await state.Seed(); var cacheA = state.A.Screenshot; state.Vm.CurrentDevice = state.B; await state.Recover(); var before = CompletedCacheSnapshot.Take(state); var calls = state.Backend.Calls().Length;
                state.Backend.Change(state.A.Serial, state.A.Info with { TransportId = state.A.Info.TransportId + 1 }); await state.Sync();
                state.Check(cacheA is not null && state.A.Screenshot is null && state.A.CapturedAt is null, "非当前A换代撤下A旧行截图/时间"); before.InspectionSame(state); before.ContextSame(state); state.NoReadsAfter(calls);
                await state.ExportPair("b-untouched", before.Image!, before.Marker!);
            });
            await Case("stable-metadata", async state =>
            {
                await state.Seed(); var before = CompletedCacheSnapshot.Take(state); var originalInfo = state.A.Info; var calls = state.Backend.Calls().Length;
                await state.Sync(); state.Vm.Runtime.Metadata.UpsertMetadata(state.A.Serial, "同身份新名称", "layout-group"); state.Backend.Change(state.A.Serial, state.A.Info with { Model = "model-change" }); await state.Sync(); await state.Sync();
                state.Check(state.A.Info.Name == "同身份新名称" && state.A.Info.GroupId == "layout-group" && state.A.Info.Generation == originalInfo.Generation, "真实metadata-only同步已生效且代次稳定");
                before.InspectionSame(state); before.RowCacheSame(state); before.ContextSame(state); state.Check(state.NodeEvents == before.NodeEvents && state.DebugEvents == before.DebugEvents, "稳定与metadata-only不重复清节点或重发图像变化"); state.NoReadsAfter(calls);
            });
            foreach (var consume in new[] { "return", "same-row", "png", "json" }) await Case("catalog-only-" + consume, async state =>
            {
                await state.Seed(); var before = CompletedCacheSnapshot.Take(state); var original = state.A.Info;
                if (consume == "return") state.Vm.CurrentDevice = state.B;
                state.Backend.Change(state.A.Serial, original with { TransportId = original.TransportId + 1 }); await state.Run(state.Vm.Runtime.RefreshAsync()); await state.Idle();
                state.Check(ReferenceEquals(state.A.Info, original) && state.Vm.Runtime.GetDevices().Single(info => info.Serial == state.A.Serial).Generation == original.Generation + 1, "Catalog-only新Ready身份已变化但VM原行Info刻意尚未同步");
                var calls = state.Backend.Calls().Length;
                if (consume is "return" or "same-row") { state.Vm.CurrentDevice = state.A; await state.Idle(); state.Cleared(state.A); }
                else
                {
                    var path = Path.Combine(state.Directory, consume == "png" ? "stale.png" : "stale.json");
                    await state.Run(consume == "png" ? state.Vm.SaveScreenshotAsync(path) : state.Vm.ExportNodesAsync(path)); await state.Idle();
                    state.ObserveFile("stale-consumption", path); state.Check(!File.Exists(path), "Catalog-only失效本地" + consume + "消费不创建错误旧产物");
                }
                state.NoReadsAfter(calls); await state.Sync(); await state.Recover(); state.Check(!ReferenceEquals(before.Image, state.Vm.DebugImage) && state.Vm.Nodes.Single().Source.Text != before.Marker, "Catalog-only消费后新的显式请求可恢复真实新内容");
            });
            await Case("background-cache-not-current-inspection", async state =>
            {
                await state.Seed(); var gate = state.Backend.HoldShot(state.A.Serial); var background = state.Run(state.Vm.RefreshAutomaticScreensAsync([state.A.Serial], CancellationToken.None)); await gate.Entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
                await state.Recover(); var before = CompletedCacheSnapshot.Take(state); var calls = state.Backend.Calls().Length; gate.Release.TrySetResult(); await background; await state.Idle();
                state.Check(!ReferenceEquals(before.Image, state.A.Screenshot) && ReferenceEquals(before.Image, state.Vm.DebugImage), "同身份后台完成可更新行缓存但不以最新Bitmap引用否定较新显式检查");
                before.InspectionSame(state); before.ContextSame(state); await state.ExportPair("inspection-not-latest-cache", before.Image!, before.Marker!); state.NoReadsAfter(calls);
            });
            await Case("legal-background-return", async state =>
            {
                await state.Seed(); var old = state.A.Screenshot; state.Vm.CurrentDevice = state.B;
                await state.Run(state.Vm.RefreshAutomaticScreensAsync([state.A.Serial], CancellationToken.None)); await state.Idle(); var fresh = state.A.Screenshot; var calls = state.Backend.Calls().Length;
                state.Vm.CurrentDevice = state.A; await state.Idle();
                state.Check(fresh is not null && !ReferenceEquals(fresh, old) && ReferenceEquals(fresh, state.Vm.DebugImage) && state.Vm.Nodes.Count == 0 && state.Vm.SelectedNode is null, "合法后台A缓存经Current A→B→A恢复，原节点不伪造缓存恢复");
                await state.ExportPng("legal-return.png", fresh!); state.NoReadsAfter(calls);
            });
            foreach (var invalid in new[] { "Unknown", "Unauthorized" }) await Case("not-offline-" + invalid, async state =>
            {
                await state.Seed(); var before = CompletedCacheSnapshot.Take(state); var calls = state.Backend.Calls().Length;
                state.Backend.Change(state.A.Serial, state.A.Info with { State = invalid }); await state.Sync();
                state.Check(state.A.Info.State == invalid && !state.A.Info.Online, "实际目录状态为" + invalid + "而非Offline"); state.Cleared(state.A); before.ContextSame(state); state.NoReadsAfter(calls);
            });
            foreach (var native in new[] { "scrcpy 进程已启动 · 待窗口核验", "已嵌入自有 scrcpy 窗口" }) await Case("native-status-" + (native.StartsWith("scrcpy", StringComparison.Ordinal) ? "pending" : "embedded"), async state =>
            {
                await state.Seed(); state.A.ScreenState = native; var before = CompletedCacheSnapshot.Take(state); var calls = state.Backend.Calls().Length;
                state.Backend.Change(state.A.Serial, state.A.Info with { TransportId = state.A.Info.TransportId + 1 }); await state.Sync(); state.Cleared(state.A);
                state.Observe("native-status-seed-only", new { native, processStarted = false, windowEmbedded = false });
                state.Check(state.A.ScreenState == native, "仅种下已有投屏状态文本，截图换代不抹独立原状态；没有启动或嵌入进程"); before.ContextSame(state); state.NoReadsAfter(calls);
            });
            await Case("offline-reconnect-offline-no-revival", async state =>
            {
                await state.Seed(); var before = CompletedCacheSnapshot.Take(state); var original = state.A.Info; var calls = state.Backend.Calls().Length;
                state.Backend.Change(state.A.Serial, original with { State = "Offline" }); await state.Sync(); before.InspectionSame(state); before.RowCacheSame(state);
                state.Backend.Change(state.A.Serial, state.Backend.Info(state.A.Serial) with { State = "Ready" }); await state.Sync();
                var readyGeneration = state.A.Info.Generation; var readyCleared = state.A.Screenshot is null && state.Vm.DebugImage is null && state.Vm.Nodes.Count == 0;
                state.Observe("new-ready-before-second-offline", new { readyGeneration, readyCleared });
                state.Backend.Change(state.A.Serial, state.Backend.Info(state.A.Serial) with { State = "Offline" }); await state.Sync();
                state.Check(readyGeneration == original.Generation + 2 && state.A.Info.State == "Offline" && state.A.Info.Generation == original.Generation + 3 && readyCleared, "Ready g→Offline g+1历史不改源戳；Ready g+2已撤旧内容再到Offline g+3");
                state.Cleared(state.A); before.ContextSame(state); state.NoReadsAfter(calls);
            });
            await Case("offline-changed-geometry-not-history", async state =>
            {
                await state.Seed(); var before = CompletedCacheSnapshot.Take(state); var calls = state.Backend.Calls().Length;
                state.Backend.Change(state.A.Serial, state.A.Info with { State = "Offline", Width = 180, Height = 260 }); await state.Sync();
                state.Check(state.A.Info.State == "Offline" && state.A.Info.Width == 180 && state.A.Info.Height == 260, "真实Offline同时几何变化，不满足同源历史许可");
                state.Cleared(state.A); before.ContextSame(state); state.NoReadsAfter(calls);
            });
            if (failures.Count > 0) throw new AggregateException("完成缓存专项存在失败", failures);
        }
        catch (Exception error) { primary = error; Console.Error.WriteLine("COMPLETED-CACHE-PRIMARY " + error); }
        finally
        {
            try { Application.Current.MainWindow = originalMain; Application.Current.ShutdownMode = originalShutdown; LocalizationService.Current.SetLanguage(language); }
            catch (Exception restoreError) { primary = primary is null ? restoreError : new AggregateException("原失败与外层上下文恢复失败", primary, restoreError); }
            try { await File.WriteAllTextAsync(Path.Combine(directory, "completed-cache.json"), JsonSerializer.Serialize(new { scope = "自有Demo/STA，真实Catalog与VM/已显示WPF投影/本地PNG JSON；非Android、非物理输入、非原生保存框、非自动首载、非runtime热换", cases = reports, totalCases = reports.Count, passed = reports.Count(report => report is CompletedCacheReport { Passed: true }), failure = primary?.ToString() }, JsonDefaults.Options)); }
            catch (Exception reportError) { primary = primary is null ? reportError : new AggregateException("原失败与报告写入失败", primary, reportError); }
        }
        if (primary is not null) ExceptionDispatchInfo.Capture(primary).Throw();

        async Task Case(string name, Func<CompletedCacheState, Task> action)
        {
            var path = Path.Combine(directory, "c" + (++number).ToString("00")); CompletedCacheState? state = null; Exception? error = null; var cleaned = false;
            try { state = await CompletedCacheState.Create(this, path); await action(state); state.Observe("case-completed"); }
            catch (Exception caught) { error = caught; state?.Observe("case-failed"); Console.Error.WriteLine("COMPLETED-CACHE-CASE " + name + " " + caught); }
            finally
            {
                if (state is not null) try { await state.Cleanup(); cleaned = true; } catch (Exception cleanupError) { error = error is null ? cleanupError : new AggregateException(error, cleanupError); }
                reports.Add(new CompletedCacheReport(name, error is null && cleaned, cleaned, state?.Checks.ToArray() ?? [], state?.Observations.ToArray() ?? [], state?.Backend.Calls() ?? [], state?.Backend.Proofs() ?? [], error?.ToString()));
            }
            if (error is not null)
            {
                failures.Add(new InvalidOperationException(name, error));
                if (state is null || !cleaned) throw new AggregateException("夹具创建/清理未完成，停止接纳后续case", failures);
            }
        }
    }

    private sealed record CompletedCacheReport(string Name, bool Passed, bool Cleanup, string[] Checks, object[] Observations, CompletedCacheCall[] Calls, CompletedCachePayload[] Payloads, string? Failure);
    private static int? CompletedCacheId(object? value) => value is null ? null : System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(value);
    private sealed record CompletedCacheSnapshot(DeviceRow? Current, DeviceRow? Main, DeviceRow[] Targets, DeviceRow[] Rows, bool[] Selections, UnifiedDeviceRow[] UnifiedRows, string[] UnifiedKeys, bool[] UnifiedSelections, PixelSample[] Colors, long InputGeneration, BitmapSource? Image, NodeRow[] Nodes, NodeRow? Selected, string Properties, string? Marker, DateTimeOffset? CapturedAt, string ScreenState, string AutomationState, int NodeEvents, int DebugEvents)
    {
        internal static CompletedCacheSnapshot Take(CompletedCacheState state) => new(state.Vm.CurrentDevice, state.Vm.MainDevice, state.Vm.Targets().ToArray(), state.Vm.Devices.ToArray(), state.Vm.Devices.Select(row => row.IsSelected).ToArray(), state.Window.unifiedDevices.ToArray(), state.Window.unifiedDevices.Select(row => row.Key).ToArray(), state.Window.unifiedDevices.Select(row => row.IsSelected).ToArray(), state.Vm.Colors.ToArray(), state.Vm.Runtime.Input.CurrentGeneration, state.Vm.DebugImage, state.Vm.Nodes.ToArray(), state.Vm.SelectedNode, state.Vm.NodeProperties, state.Vm.Nodes.FirstOrDefault()?.Source.Text, state.Vm.CurrentDevice?.CapturedAt, state.Vm.CurrentDevice?.ScreenState ?? "", state.Vm.CurrentDevice?.AutomationState ?? "", state.NodeEvents, state.DebugEvents);
        internal void ContextSame(CompletedCacheState state)
        {
            var actualTargets = state.Vm.Targets().ToArray();
            var currentSame = ReferenceEquals(Current, state.Vm.CurrentDevice); var mainSame = ReferenceEquals(Main, state.Vm.MainDevice); var targetsSame = Targets.SequenceEqual(actualTargets);
            var selectionsSame = Selections.SequenceEqual(state.Vm.Devices.Select(row => row.IsSelected)); var colorsSame = Colors.SequenceEqual(state.Vm.Colors);
            var inputGenerationSame = InputGeneration == state.Vm.Runtime.Input.CurrentGeneration; var busyReleased = !state.Vm.Busy;
            var rowReferencesSame = Rows.SequenceEqual(state.Vm.Devices); var unifiedReferencesSame = UnifiedRows.SequenceEqual(state.Window.unifiedDevices);
            var unifiedKeysSame = UnifiedKeys.SequenceEqual(state.Window.unifiedDevices.Select(row => row.Key)); var unifiedSelectionsSame = UnifiedSelections.SequenceEqual(state.Window.unifiedDevices.Select(row => row.IsSelected));
            var selectedNonOnline = Rows.Where((row, index) => Selections[index] && !row.Info.Online).ToArray();
            var expectedEffective = selectedNonOnline.Length > 0 ? Array.Empty<DeviceRow>() : Targets;
            var effectiveTargetsSame = expectedEffective.SequenceEqual(actualTargets); var canWriteTargets = state.Vm.CanWriteTargets();
            state.Observe("context-components", new
            {
                currentSame, mainSame, targetsSame, selectionsSame, colorsSame, inputGenerationSame, busyReleased,
                rowReferencesSame, unifiedReferencesSame, unifiedKeysSame, unifiedSelectionsSame, effectiveTargetsSame, canWriteTargets,
                expectedCurrent = new { serial = Current?.Serial, id = CompletedCacheId(Current) }, actualCurrent = new { serial = state.Vm.CurrentDevice?.Serial, id = CompletedCacheId(state.Vm.CurrentDevice) },
                expectedMain = new { serial = Main?.Serial, id = CompletedCacheId(Main) }, actualMain = new { serial = state.Vm.MainDevice?.Serial, id = CompletedCacheId(state.Vm.MainDevice) },
                expectedTargets = Targets.Select(row => new { row.Serial, id = CompletedCacheId(row), row.IsSelected, row.Info.State }).ToArray(),
                expectedEffectiveTargets = expectedEffective.Select(row => new { row.Serial, id = CompletedCacheId(row) }).ToArray(), gateSelectedNonOnline = selectedNonOnline.Select(row => new { row.Serial, id = CompletedCacheId(row), row.Info.State }).ToArray(),
                actualTargets = actualTargets.Select(row => new { row.Serial, id = CompletedCacheId(row), row.IsSelected, row.Info.State }).ToArray(),
                expectedSelections = Selections, expectedRowIds = Rows.Select(CompletedCacheId).ToArray(), actualRows = state.Vm.Devices.Select(row => new { row.Serial, id = CompletedCacheId(row), row.IsSelected, row.Info.State }).ToArray(),
                expectedUnified = UnifiedRows.Select((row, index) => new { id = CompletedCacheId(row), key = UnifiedKeys[index], selected = UnifiedSelections[index] }).ToArray(), actualUnified = state.Window.unifiedDevices.Select(row => new { id = CompletedCacheId(row), row.Key, row.IsSelected, state = row.Device?.Info.State, hasInstance = row.Instance is not null }).ToArray(),
                expectedColors = Colors, actualColors = state.Vm.Colors.ToArray(), expectedInputGeneration = InputGeneration, actualInputGeneration = state.Vm.Runtime.Input.CurrentGeneration, state.Vm.Busy
            });
            state.Check(currentSame && mainSame && rowReferencesSame && selectionsSame && unifiedReferencesSame && unifiedKeysSame && unifiedSelectionsSame && colorsSame && inputGenerationSame && busyReleased, "身份清理保留Current/Main/原设备与统一行引用顺序/key/勾选意图/颜色历史/输入代次且Busy释放");
            state.Check(state.Window.unifiedDevices.All(row => row.Instance is null && row.Device is not null) && effectiveTargetsSame && (selectedNonOnline.Length == 0 || !canWriteTargets), "无LD绑定夹具按冻结勾选意图验证有效目标：含非在线行必须空目标且禁止写入，其余精确保持原目标序列");
        }
        internal void RowCacheSame(CompletedCacheState state) => state.Check(ReferenceEquals(Current?.Screenshot, Image) && Current?.CapturedAt == CapturedAt && Current?.ScreenState == ScreenState && Current?.AutomationState == AutomationState, "合法同源历史保留行截图/接纳时间/截图与自动化状态");
        internal void InspectionSame(CompletedCacheState state)
        {
            var image = ReferenceEquals(Image, state.Vm.DebugImage); var nodes = Nodes.SequenceEqual(state.Vm.Nodes); var selected = ReferenceEquals(Selected, state.Vm.SelectedNode); var properties = Properties == state.Vm.NodeProperties;
            state.Observe("inspection-equality", new { image, nodes, selected, properties, imageBefore = CompletedCacheId(Image), imageAfter = CompletedCacheId(state.Vm.DebugImage), Marker, actualMarker = state.Vm.Nodes.FirstOrDefault()?.Source.Text });
            state.Check(image && nodes && selected && properties && state.Vm.CurrentDevice?.AutomationState == AutomationState && state.Vm.Error.Length == 0, "合法已完成检查保留原图/根/选择/属性/自动化状态且无错误"); state.Projection();
        }
    }
    private sealed class CompletedCacheState
    {
        internal required string Directory; internal required CompletedCacheBackend Backend; internal required WorkspaceViewModel Vm; internal required MainWindow Window;
        internal readonly List<string> Checks = []; internal readonly List<object> Observations = []; private readonly List<Task> operations = [];
        private PropertyChangedEventHandler? vmObserver; private System.Collections.Specialized.NotifyCollectionChangedEventHandler? nodesObserver; private bool cleanupStarted;
        internal int NodeEvents; internal int DebugEvents;
        internal DeviceRow A => Vm.Devices.Single(row => row.Serial == "DEMO-001"); internal DeviceRow B => Vm.Devices.Single(row => row.Serial == "DEMO-002");
        internal static async Task<CompletedCacheState> Create(MainWindow owner, string directory)
        {
            System.IO.Directory.CreateDirectory(directory); var backend = new CompletedCacheBackend(); RollingLog? log = null; CatalogRepository? metadata = null; MatrixRuntime? runtime = null; WorkspaceViewModel? vm = null; MainWindow? window = null; CompletedCacheState? state = null; ScriptScheduler? scripts = null; InputCoordinator? input = null; ApiServiceManager? api = null; Task? initializing = null;
            try
            {
                log = new RollingLog(Path.Combine(directory, "logs")); metadata = new CatalogRepository(Path.Combine(directory, "db.sqlite3"));
                var catalog = new DeviceCatalog(backend); var automation = new AutomationService(backend, catalog); scripts = new ScriptScheduler(backend); input = new InputCoordinator(catalog, backend); api = new ApiServiceManager(catalog, backend, automation, log);
                var settings = new AppSettings { AutoStartApi = false, AutoConnect = false, SyncMouse = false, SyncKeyboard = false, Language = "zh-CN", WindowWidth = 1000, WindowHeight = 700 };
                runtime = new MatrixRuntime { Backend = backend, Devices = catalog, Automation = automation, Scripts = scripts, Input = input, Api = api, Log = log, Metadata = metadata, Settings = settings, FrozenConnectionSettings = settings, SettingsStore = new SettingsStore(Path.Combine(directory, "AppConfig.json")), IsDemo = true };
                api.DeviceSnapshotProvider = runtime.GetDevices; vm = new WorkspaceViewModel(runtime, directory, true); initializing = vm.RefreshDevicesAsync(); await initializing.WaitAsync(TimeSpan.FromSeconds(5));
                window = new MainWindow(vm) { Owner = owner, ShowActivated = false }; state = new CompletedCacheState { Directory = directory, Backend = backend, Vm = vm, Window = window };
                state.vmObserver = (_, args) => { if (args.PropertyName == nameof(WorkspaceViewModel.DebugImage)) state.DebugEvents++; }; vm.PropertyChanged += state.vmObserver;
                state.nodesObserver = (_, _) => state.NodeEvents++; vm.Nodes.CollectionChanged += state.nodesObserver;
                window.Show(); await state.Idle(); window.NavigateTo("节点"); await state.Idle();
                state.Check(vm.Demo && runtime.IsDemo && ReferenceEquals(runtime.Backend, backend) && ReferenceEquals(runtime.Devices, catalog) && ReferenceEquals(runtime.Automation, automation) && ReferenceEquals(runtime.Input, input) && ReferenceEquals(runtime.Scripts, scripts) && ReferenceEquals(runtime.Api, api) && !api.BasicRunning && !api.AutomatorRunning && api.DevicePorts.Count == 0 && !window.DebugAutomaticFixturePermitted && !window.debugAutoLoading && !window.debugAutoPending && backend.ReadCount == 0, "一致自有Demo/默认自动首载保护，API未启、导航无设备读");
                await state.Run(vm.SetMainAsync(state.B)); vm.CurrentDevice = state.A; state.A.IsSelected = state.B.IsSelected = true; await state.Idle(); backend.ClearTrace(); state.NodeEvents = state.DebugEvents = 0; state.Observe("created"); return state;
            }
            catch (Exception error)
            {
                var cleanupErrors = new List<Exception>();
                if (state is not null) { try { await state.Cleanup(); } catch (Exception cleanupError) { cleanupErrors.Add(cleanupError); } }
                else
                {
                    try { if (window is not null) { LocalizationService.Current.LanguageChanged -= window.OnLanguageChanged; window.AbortStartup(); } } catch (Exception cleanupError) { cleanupErrors.Add(cleanupError); }
                    if (initializing is not null) await Attempt(() => initializing.WaitAsync(TimeSpan.FromSeconds(5)));
                    if (vm is not null) await Attempt(() => vm.ShutdownAsync().WaitAsync(TimeSpan.FromSeconds(5)));
                    else if (runtime is not null) await Attempt(() => runtime.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(5)));
                    else
                    {
                        if (api is not null) await Attempt(() => api.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(5)));
                        if (input is not null) await Attempt(() => input.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(5)));
                        if (scripts is not null) await Attempt(() => scripts.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(5)));
                        await Attempt(() => backend.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(5)));
                        try { metadata?.Dispose(); } catch (Exception cleanupError) { cleanupErrors.Add(cleanupError); }
                        if (log is not null) await Attempt(() => log.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(5)));
                    }
                }
                if (cleanupErrors.Count > 0) throw new AggregateException("夹具初始化与清理失败", new[] { error }.Concat(cleanupErrors)); throw;
                async Task Attempt(Func<Task> action) { try { await action(); } catch (Exception cleanupError) { cleanupErrors.Add(cleanupError); } }
            }
        }
        internal Task Run(Task task) { operations.Add(task); return task.WaitAsync(TimeSpan.FromSeconds(5)); }
        internal async Task Idle() { await Window.Dispatcher.InvokeAsync(Window.UpdateLayout, DispatcherPriority.ApplicationIdle).Task.WaitAsync(TimeSpan.FromSeconds(5)); await Window.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ContextIdle).Task.WaitAsync(TimeSpan.FromSeconds(5)); }
        internal async Task Sync() { await Run(Vm.RefreshDeviceSnapshotAsync()); await Idle(); Observe("catalog-applied"); }
        internal async Task Seed() { await Recover(); Vm.SelectedNode = Vm.FlattenNodes().First(node => node.Source.Clickable); Vm.SamplePixel(3, 4); await Idle(); Check(Vm.SelectedNode is not null && Vm.Colors.Count == 1, "显式完成内容建立真实节点选择与像素历史"); }
        internal async Task Recover()
        {
            var calls = Backend.Calls().Length; var serial = Vm.CurrentDevice!.Serial; await Run(Vm.LoadNodesAsync()); await Idle();
            Check(Backend.Calls().Skip(calls).Select(call => call.Operation + ":" + call.Serial).SequenceEqual(new[] { "Discover:", "Screenshot:" + serial, "Dump:" + serial }), "显式请求真实调用顺序严格为Discover→当前源Screenshot→Dump");
            Check(!Vm.Busy && Vm.Error.Length == 0 && Vm.Nodes.Count == 1 && ReferenceEquals(Vm.DebugImage, Vm.CurrentDevice?.Screenshot) && Vm.Nodes.Single().Source.Text == Backend.LastDump(serial), "显式完成内容有可追溯节点标记且无错误"); Projection(); Observe("explicit-completed");
        }
        internal void Projection() => Check(ReferenceEquals(Window.screenshotImage.Source, Vm.DebugImage) && Window.nodeTree.Items.Count == Vm.Nodes.Count && Window.nodeTreeItems.Count == Vm.FlattenNodes().Count() && Window.nodeRectangles.Count == Vm.FlattenNodes().Count(node => node.Source.Bounds.Width > 0 && node.Source.Bounds.Height > 0) && (Vm.DebugImage is null ? !Window.nodeCanvas.Children.OfType<Image>().Any() : Window.nodeCanvas.Children.OfType<Image>().Any(image => ReferenceEquals(image.Source, Vm.DebugImage))), "原MainWindow回调的真实截图/Tree/Canvas投影与VM一致");
        internal bool SameTransportGeometry(DeviceInfo a, DeviceInfo b) => a.Serial == b.Serial && a.TransportId == b.TransportId && a.Width == b.Width && a.Height == b.Height && a.Rotation == b.Rotation;
        internal void NewReady(DeviceInfo original, long increment = 1) => Check(A.Info.Online && A.Info.Generation == original.Generation + increment, "真实Catalog换代后的Ready代次精确增长" + increment);
        internal void Cleared(DeviceRow row)
        {
            Observe("expect-invalidated"); Check(row.Screenshot is null && row.CapturedAt is null && row.ScreenState != "截图可用" && row.AutomationState != "节点可用", "换代行撤下旧Screenshot/时间/可用状态");
            if (ReferenceEquals(row, Vm.CurrentDevice)) { Check(Vm.DebugImage is null && Vm.Nodes.Count == 0 && Vm.SelectedNode is null && Vm.NodeProperties == "未选中节点" && Window.nodeTree.Items.Count == 0 && Window.nodeRectangles.Count == 0 && Window.crop is null, "换代当前检查撤下旧图/根/选择/属性及实际节点覆盖，原画布裁剪字段为空"); Projection(); }
        }
        internal void NoReadsAfter(int callCount) => Check(Backend.Calls().Skip(callCount).All(call => call.Operation == "Discover"), "身份同步/本地消费没有额外Screenshot或Dump设备读");
        internal async Task RejectExports(string prefix)
        {
            foreach (var format in new[] { "png", "json" })
            {
                var path = Path.Combine(Directory, prefix + "." + format); await Run(format == "png" ? Vm.SaveScreenshotAsync(path) : Vm.ExportNodesAsync(path)); ObserveFile("invalidated-export", path); Check(!File.Exists(path), "身份换代清理后的" + format + "不导出旧产物");
            }
        }
        internal async Task ExportPair(string prefix, BitmapSource image, string marker) { await ExportPng(prefix + ".png", image); var path = Path.Combine(Directory, prefix + ".json"); await Run(Vm.ExportNodesAsync(path)); ObserveFile("json-export", path); Check(File.Exists(path), "合法节点JSON实际创建"); var node = JsonSerializer.Deserialize<AndroidNode>(await File.ReadAllTextAsync(path), JsonDefaults.Options); Check(node?.Text == marker, "磁盘JSON包含当前合法检查根标记而非其他缓存"); }
        internal async Task ExportPng(string name, BitmapSource image)
        {
            var path = Path.Combine(Directory, name); await Run(Vm.SaveScreenshotAsync(path)); ObserveFile("png-export", path); Check(File.Exists(path), "合法截图PNG实际创建");
            using var stream = File.OpenRead(path); var actual = BitmapFrame.Create(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad); var expectedPixels = Pixels(image); var actualPixels = Pixels(actual);
            Observe("png-pixels", new { expectedWidth = image.PixelWidth, expectedHeight = image.PixelHeight, actual.PixelWidth, actual.PixelHeight, expected = Convert.ToHexString(SHA256.HashData(expectedPixels)), actual = Convert.ToHexString(SHA256.HashData(actualPixels)) });
            Check(actual.PixelWidth == image.PixelWidth && actual.PixelHeight == image.PixelHeight && actualPixels.SequenceEqual(expectedPixels), "磁盘PNG完整像素与合法检查源一致");
        }
        private static byte[] Pixels(BitmapSource image) { var converted = new FormatConvertedBitmap(image, PixelFormats.Bgra32, null, 0); var bytes = new byte[checked(image.PixelWidth * image.PixelHeight * 4)]; converted.CopyPixels(bytes, checked(image.PixelWidth * 4), 0); return bytes; }
        internal void ObserveFile(string stage, string path)
        {
            object? content = null; var exists = File.Exists(path);
            if (exists && Path.GetExtension(path) == ".json") { var root = JsonSerializer.Deserialize<AndroidNode>(File.ReadAllText(path), JsonDefaults.Options); content = new { rootMarker = root?.Text, children = root?.ChildNodeList.Count }; }
            if (exists && Path.GetExtension(path) == ".png")
            {
                using var stream = File.OpenRead(path); var image = BitmapFrame.Create(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad); var pixels = Pixels(image);
                content = new { image.PixelWidth, image.PixelHeight, pixelSha256 = Convert.ToHexString(SHA256.HashData(pixels)), firstBgra = pixels.Take(4).ToArray(), lastBgra = pixels.TakeLast(4).ToArray() };
            }
            Observe(stage, new { file = Path.GetFileName(path), exists, bytes = exists ? new FileInfo(path).Length : 0, sha256 = exists ? Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))) : null, content });
        }
        internal void Check(bool value, string message) { if (!value) { Observe("assertion-failed", new { message }); throw new InvalidOperationException(message); } Checks.Add(message); }
        internal void Observe(string stage, object? detail = null) => Observations.Add(new { stage, detail, Vm.Busy, Vm.Error, Vm.Status, current = Vm.CurrentDevice?.Serial, currentId = CompletedCacheId(Vm.CurrentDevice), main = Vm.MainDevice?.Serial, imageId = CompletedCacheId(Vm.DebugImage), rootMarker = Vm.Nodes.FirstOrDefault()?.Source.Text, nodes = Vm.Nodes.Count, selectedId = CompletedCacheId(Vm.SelectedNode), tree = Window.nodeTree.Items.Count, rectangles = Window.nodeRectangles.Count, NodeEvents, DebugEvents, rows = Vm.Devices.Select(row => new { row.Serial, row.Info.State, row.Info.Generation, row.Info.TransportId, row.Info.Width, row.Info.Height, row.Info.Rotation, id = CompletedCacheId(row), image = CompletedCacheId(row.Screenshot), row.CapturedAt, row.ScreenState, row.AutomationState }).ToArray(), catalog = Vm.Runtime.GetDevices().ToArray() });
        internal async Task Cleanup()
        {
            if (cleanupStarted) throw new InvalidOperationException("自有夹具不得重复清理"); cleanupStarted = true; Backend.ReleaseAll(); var errors = new List<Exception>();
            try { await Task.WhenAll(operations).WaitAsync(TimeSpan.FromSeconds(5)); } catch (Exception error) { errors.Add(error); }
            if (operations.Any(task => !task.IsCompleted)) errors.Add(new InvalidOperationException("自有已接纳操作未排空"));
            try { Observe("cleanup-drained"); Check(!Vm.Busy && !Window.debugAutoLoading && !Window.debugAutoPending && Backend.GatesDrained, "自有操作/TCS排空且默认Demo自动泵未启动"); } catch (Exception error) { errors.Add(error); }
            if (vmObserver is not null) Vm.PropertyChanged -= vmObserver; if (nodesObserver is not null) Vm.Nodes.CollectionChanged -= nodesObserver;
            try { LocalizationService.Current.LanguageChanged -= Window.OnLanguageChanged; Window.AbortStartup(); } catch (Exception error) { errors.Add(error); }
            try { await Vm.ShutdownAsync().WaitAsync(TimeSpan.FromSeconds(5)); } catch (Exception error) { errors.Add(error); }
            if (errors.Count > 0) throw new AggregateException("自有完成缓存夹具清理失败", errors);
        }
    }

    private sealed record CompletedCacheCall(int Sequence, string Operation, string? Serial);
    private sealed record CompletedCachePayload(int Call, string Operation, string Serial, string Marker, string? Sha256, int Width, int Height);
    private sealed class CompletedCacheGate(string serial)
    {
        internal readonly string Serial = serial; internal readonly TaskCompletionSource Entered = new(TaskCreationOptions.RunContinuationsAsynchronously); internal readonly TaskCompletionSource Release = new(TaskCreationOptions.RunContinuationsAsynchronously); internal bool Claimed;
    }
    private sealed class CompletedCacheBackend : IDeviceBackend
    {
        private readonly DemoDeviceBackend inner = new(); private readonly object sync = new(); private readonly List<CompletedCacheCall> calls = []; private readonly List<CompletedCachePayload> payloads = []; private readonly List<CompletedCacheGate> gates = []; private int frame;
        private readonly Dictionary<string, DeviceInfo> devices = new(StringComparer.Ordinal) { ["DEMO-001"] = new() { Serial = "DEMO-001", Name = "缓存A", State = "Ready", Width = 160, Height = 240, Rotation = 0, TransportId = 11 }, ["DEMO-002"] = new() { Serial = "DEMO-002", Name = "缓存B", State = "Ready", Width = 160, Height = 240, Rotation = 0, TransportId = 22 } };
        internal CompletedCacheCall[] Calls() { lock (sync) return calls.ToArray(); } internal CompletedCachePayload[] Proofs() { lock (sync) return payloads.ToArray(); }
        internal int ReadCount { get { lock (sync) return calls.Count(call => call.Operation is "Screenshot" or "Dump"); } }
        internal bool GatesDrained { get { lock (sync) return gates.All(gate => gate.Claimed && gate.Entered.Task.IsCompletedSuccessfully && gate.Release.Task.IsCompletedSuccessfully); } }
        internal DeviceInfo Info(string serial) { lock (sync) return devices[serial]; } internal void Change(string serial, DeviceInfo value) { lock (sync) devices[serial] = value; }
        internal string? LastDump(string serial) { lock (sync) return payloads.LastOrDefault(payload => payload.Operation == "Dump" && payload.Serial == serial)?.Marker; }
        internal void ClearTrace() { lock (sync) { calls.Clear(); payloads.Clear(); } }
        internal CompletedCacheGate HoldShot(string serial) { var gate = new CompletedCacheGate(serial); lock (sync) gates.Add(gate); return gate; }
        internal void ReleaseAll() { lock (sync) foreach (var gate in gates) gate.Release.TrySetResult(); }
        private int Record(string operation, string? serial) { lock (sync) { var n = calls.Count + 1; calls.Add(new(n, operation, serial)); return n; } }
        public Task<IReadOnlyList<DeviceInfo>> DiscoverAsync(CancellationToken ct = default) { ct.ThrowIfCancellationRequested(); Record("Discover", null); lock (sync) return Task.FromResult<IReadOnlyList<DeviceInfo>>(devices.Values.ToArray()); }
        public async Task<byte[]> ScreenshotAsync(string serial, CancellationToken ct = default)
        {
            var call = Record("Screenshot", serial); CompletedCacheGate? gate; lock (sync) { gate = gates.FirstOrDefault(item => !item.Claimed && item.Serial == serial); if (gate is not null) gate.Claimed = true; }
            if (gate is not null) { gate.Entered.TrySetResult(); await gate.Release.Task.WaitAsync(ct); } ct.ThrowIfCancellationRequested(); DeviceInfo info; int marker; lock (sync) { info = devices[serial]; marker = ++frame; }
            var png = PngImage.Create(info.Width, info.Height, (x, y) => ((byte)(marker % 200 + 1), (byte)(serial == "DEMO-001" ? 31 : 73), (byte)((x + y) % 251)));
            lock (sync) payloads.Add(new(call, "Screenshot", serial, "frame:" + marker, Convert.ToHexString(SHA256.HashData(png)), info.Width, info.Height)); return png;
        }
        public Task<AndroidNode> DumpAsync(string serial, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested(); var call = Record("Dump", serial); var info = Info(serial); var marker = $"nodes:{serial}:{call}:t{info.TransportId}:{info.Width}x{info.Height}:r{info.Rotation}";
            var root = new AndroidNode { Text = marker, ResourceId = "fixture:root", Enabled = true, Bounds = new(0, 0, info.Width, info.Height), ChildNodeList = [new AndroidNode { Text = marker + ":child", ResourceId = "fixture:child", Clickable = true, Enabled = true, Bounds = new(5, 5, 50, 50) }] };
            lock (sync) payloads.Add(new(call, "Dump", serial, marker, null, info.Width, info.Height)); return Task.FromResult(root);
        }
        public Task<string> ShellAsync(string serial, string command, bool useRoot = false, CancellationToken ct = default) => inner.ShellAsync(serial, command, useRoot, ct);
        public Task TapAsync(string serial, int x, int y, CancellationToken ct = default) => inner.TapAsync(serial, x, y, ct);
        public Task SwipeAsync(string serial, int x1, int y1, int x2, int y2, int durationMs, CancellationToken ct = default) => inner.SwipeAsync(serial, x1, y1, x2, y2, durationMs, ct);
        public Task KeyEventAsync(string serial, int code, CancellationToken ct = default) => inner.KeyEventAsync(serial, code, ct);
        public Task TextAsync(string serial, string text, CancellationToken ct = default) => inner.TextAsync(serial, text, ct);
        public Task<string> StartAppAsync(string serial, string packageName, string? activity = null, CancellationToken ct = default) => inner.StartAppAsync(serial, packageName, activity, ct);
        public Task<string> StopAppAsync(string serial, string packageName, CancellationToken ct = default) => inner.StopAppAsync(serial, packageName, ct);
        public Task<bool> ClearAppAsync(string serial, string packageName, CancellationToken ct = default) => inner.ClearAppAsync(serial, packageName, ct);
        public Task<IReadOnlyList<AppInfo>> ListAppsAsync(string serial, bool showSystem = false, bool needVersion = true, CancellationToken ct = default) => inner.ListAppsAsync(serial, showSystem, needVersion, ct);
        public Task InstallAsync(string serial, string localPath, CancellationToken ct = default) => inner.InstallAsync(serial, localPath, ct);
        public Task UninstallAsync(string serial, string packageName, CancellationToken ct = default) => inner.UninstallAsync(serial, packageName, ct);
        public Task PushAsync(string serial, string localPath, string remotePath, IProgress<double>? progress = null, CancellationToken ct = default) => inner.PushAsync(serial, localPath, remotePath, progress, ct);
        public Task PullAsync(string serial, string remotePath, string localPath, IProgress<double>? progress = null, CancellationToken ct = default) => inner.PullAsync(serial, remotePath, localPath, progress, ct);
        public Task SetNodeTextAsync(string serial, NodeSelector selector, string text, CancellationToken ct = default) => inner.SetNodeTextAsync(serial, selector, text, ct);
        public ValueTask DisposeAsync() { ReleaseAll(); return inner.DisposeAsync(); }
    }
}
