using System.IO;
using System.Runtime.ExceptionServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Media.Imaging;
using MatrixDevice.Core;
using MatrixDevice.Features;
using MatrixDevice.Host;

namespace MatrixDevice.Desktop;

internal static class DebugPublicationSmoke
{
    internal static async Task VerifyAsync(string parentDataDirectory)
    {
        var parent = Path.GetFullPath(parentDataDirectory);
        if (!Path.IsPathFullyQualified(parentDataDirectory) || !parent.Contains(Path.DirectorySeparatorChar + "artifacts" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
            || Application.Current?.Dispatcher.CheckAccess() != true || Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
            throw new InvalidOperationException("调试发布验收仅限artifacts内独立Desktop/STA Demo工作区。");
        var directory = Path.Combine(parent, "debug-publication", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + "-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var reports = new List<object>(); var caseFailures = new List<Exception>(); Exception? failure = null; var phase = "normal";
        try
        {
            await RunCase("normal", async state =>
            {
                var vm = state.ViewModel;
                vm.CurrentDevice = state.A;
                await state.Track(vm.CaptureAsync()).WaitAsync(TimeSpan.FromSeconds(5));
                state.Check(!vm.Busy && vm.Error.Length == 0 && ReferenceEquals(vm.DebugImage, state.A.Screenshot) && vm.DebugImage is { PixelWidth: > 0 }, "正常Capture发布A截图且恢复Busy");
                state.Check(state.Backend.Calls().Count(call => call.Operation == "Screenshot" && call.Serial == state.A.Serial) == 1 && state.Backend.Calls().All(call => call.Serial is null || call.Serial == state.A.Serial), "正常Capture实际请求仅来自A");
                await state.Track(vm.LoadNodesAsync()).WaitAsync(TimeSpan.FromSeconds(5));
                var node = vm.FlattenNodes().FirstOrDefault(row => row.Source.Clickable);
                vm.SelectedNode = node;
                state.Check(!vm.Busy && vm.Error.Length == 0 && vm.Nodes.Count > 0 && node is not null && ReferenceEquals(vm.SelectedNode, node), "正常LoadNodes发布树并可选择节点");
                state.Check(state.Backend.Calls().Count(call => call.Operation == "Dump" && call.Serial == state.A.Serial) == 1 && state.NodeEvents > 0, "正常节点请求及真实NodesChanged回调存在");
                var export = Path.Combine(state.Directory, "normal-nodes.json");
                await state.Track(vm.ExportNodesAsync(export)).WaitAsync(TimeSpan.FromSeconds(5));
                using var json = JsonDocument.Parse(await File.ReadAllTextAsync(export));
                state.Check(vm.Error.Length == 0 && json.RootElement.GetProperty("childNodeList")[0].GetProperty("text").GetString()?.Contains(state.A.Serial, StringComparison.Ordinal) == true, "正常导出根属于A，非只核Task返回");
                state.Observe("normal-complete");
            });
            foreach (var nodes in new[] { false, true })
            foreach (var aba in new[] { false, true })
            {
                await RunCase($"discover-{(nodes ? "nodes" : "capture")}-{(aba ? "aba" : "to-b")}", async state =>
                {
                    var vm = state.ViewModel; await SeedA(state); var gate = state.Backend.Hold("Discover"); var pending = Explicit(state, nodes);
                    await Entered(state, gate, pending);
                    state.Check(state.Backend.Calls().All(call => call.Operation == "Discover"), "首RefreshGeometry实际Discover已进入且截图和Dump尚未发生");
                    vm.CurrentDevice = state.B; if (aba) vm.CurrentDevice = state.A;
                    var projection = Projection.Take(state); state.Observe("setter-before-discover-release");
                    gate.Release.TrySetResult(); await pending.WaitAsync(TimeSpan.FromSeconds(5));
                    state.Check(state.Backend.Calls().All(call => call.Operation == "Discover"), "首次等待后的过期单设备请求不偷换来源或继续设备请求");
                    projection.CheckSame(state, "首次Discover目标变化"); Cancelled(state); await Recover(state);
                });
            }
            foreach (var scenario in new[] { (Nodes: false, Operation: "Screenshot"), (Nodes: true, Operation: "Screenshot"), (Nodes: true, Operation: "Dump") })
            {
                await RunCase($"payload-aba-{(scenario.Nodes ? "nodes" : "capture")}-{scenario.Operation}", async state =>
                {
                    var vm = state.ViewModel; await SeedA(state); var gate = state.Backend.Hold(scenario.Operation, state.A.Serial); var pending = Explicit(state, scenario.Nodes);
                    await Entered(state, gate, pending); vm.CurrentDevice = state.B; vm.CurrentDevice = state.A;
                    var projection = Projection.Take(state); var automation = state.A.AutomationState; state.Observe("setter-aba-before-payload-release");
                    gate.Release.TrySetResult(); await pending.WaitAsync(TimeSpan.FromSeconds(5));
                    projection.CheckSame(state, "ABA的setter合法恢复缓存后不得再发布旧payload");
                    if (scenario.Operation == "Screenshot") state.Check(!state.Backend.Calls().Any(call => call.Operation == "Dump"), "检查已过期的LoadNodes截图不得继续Dump；不强求合法源缓存不变");
                    else state.Check(state.A.AutomationState == automation && state.A.AutomationState != "节点可用", "未接纳且无节点缓存的迟到Dump不假报节点可用");
                    Cancelled(state); await Recover(state);
                });
            }
            foreach (var scenario in new[] { (Nodes: false, Operation: "Screenshot"), (Nodes: true, Operation: "Screenshot"), (Nodes: true, Operation: "Dump") })
            foreach (var identity in new[] { "transport-row", "geometry-row", "offline-row", "transport-catalog-only" })
            {
                await RunCase("identity-" + (scenario.Nodes ? "nodes" : "capture") + "-" + scenario.Operation + "-" + identity, async state =>
                {
                    var vm = state.ViewModel; await SeedA(state); var gate = state.Backend.Hold(scenario.Operation, state.A.Serial); var pending = Explicit(state, scenario.Nodes);
                    await Entered(state, gate, pending); var oldInfo = state.A.Info;
                    state.Backend.Override(state.A.Serial, info => identity.StartsWith("transport", StringComparison.Ordinal) ? info with { TransportId = (info.TransportId ?? 0) + 17 }
                        : identity == "geometry-row" ? info with { Width = 720, Height = 360, Rotation = 1 } : info with { State = "Offline" });
                    if (identity.EndsWith("catalog-only", StringComparison.Ordinal)) await state.Track(vm.Runtime.RefreshAsync()).WaitAsync(TimeSpan.FromSeconds(5)); else await state.Track(vm.RefreshDeviceSnapshotAsync()).WaitAsync(TimeSpan.FromSeconds(5));
                    var catalog = vm.Runtime.GetDevices().Single(info => info.Serial == state.A.Serial);
                    state.Check(catalog.Generation > oldInfo.Generation && (identity.EndsWith("catalog-only", StringComparison.Ordinal) ? ReferenceEquals(state.A.Info, oldInfo) : state.A.Info.Generation == catalog.Generation), "真实Catalog推进Generation且row同步/故意旧row前置成立: " + identity);
                    var cache = Cache.Take(state.A); var projection = Projection.Take(state); state.Observe("identity-invalidated-before-release");
                    gate.Release.TrySetResult(); await pending.WaitAsync(TimeSpan.FromSeconds(5));
                    cache.CheckSame(state, state.A, "身份失效不得改源缓存/时间/截图状态/节点状态"); projection.CheckSame(state, "身份失效不发布当前检查");
                    if (scenario.Operation == "Screenshot") state.Check(!state.Backend.Calls().Any(call => call.Operation == "Dump"), "失效截图不继续Dump");
                    Cancelled(state); state.Backend.Override(state.A.Serial, null); await state.Track(vm.RefreshDeviceSnapshotAsync()).WaitAsync(TimeSpan.FromSeconds(5)); await Recover(state);
                });
            }
            foreach (var nodes in new[] { false, true })
            {
                await RunCase("normal-geometry-" + (nodes ? "nodes" : "capture"), async state =>
                {
                    state.ViewModel.CurrentDevice = state.A; var generation = state.A.Info.Generation;
                    state.Backend.Override(state.A.Serial, info => info with { Width = 720, Height = 360, Rotation = 1 });
                    await Explicit(state, nodes).WaitAsync(TimeSpan.FromSeconds(5));
                    Completed(state); state.Check(state.A.Info.Generation > generation && state.ViewModel.DebugImage is { PixelWidth: 720, PixelHeight: 360 } && ReferenceEquals(state.ViewModel.DebugImage, state.A.Screenshot), "正常首次几何预处理建立新身份基线后继续新尺寸截图");
                    if (nodes) state.Check(state.ViewModel.Nodes.Count > 0 && Root(state.ViewModel)?.Bounds is { Width: 720, Height: 360 }, "正常新几何仍可取得匹配Demo节点根");
                });
            }
            await RunCase("automatic-a-preserves-new-b", async state =>
            {
                var vm = state.ViewModel; await state.Track(vm.SetMainAsync(state.A)).WaitAsync(TimeSpan.FromSeconds(5)); vm.CurrentDevice = state.B;
                var beforeA = Cache.Take(state.A); var gate = state.Backend.Hold("Screenshot", state.A.Serial); var pending = state.Track(vm.RefreshAutomaticScreensAsync([state.A.Serial], CancellationToken.None));
                await Entered(state, gate, pending, busy: false); await state.Track(vm.LoadNodesAsync()).WaitAsync(TimeSpan.FromSeconds(5)); vm.SelectedNode = vm.FlattenNodes().First(row => row.Source.Clickable);
                var export = await Export(state, "new-b-before"); var projection = Projection.Take(state); state.Observe("new-b-before-a-release");
                gate.Release.TrySetResult(); await pending.WaitAsync(TimeSpan.FromSeconds(5));
                projection.CheckSame(state, "仅拍A的后台完成不得清B新检查", feedback: true); state.Check(ReferenceEquals(vm.MainDevice, state.A) && !ReferenceEquals(state.A.Screenshot, beforeA.Image) && state.A.CapturedAt is not null, "MainA合法源缓存仍完成且不被CurrentB门禁取消");
                state.Check(await Export(state, "new-b-after") == export, "自动A完成后B真实导出根保持"); Completed(state);
            });
            await RunCase("automatic-ab-preserves-new-same-b", async state =>
            {
                var vm = state.ViewModel; vm.CurrentDevice = state.B;
                var gate = state.Backend.Hold("Screenshot", state.A.Serial); var pending = state.Track(vm.RefreshAutomaticScreensAsync([state.A.Serial, state.B.Serial], CancellationToken.None));
                await Entered(state, gate, pending, busy: false); await state.Track(vm.LoadNodesAsync()).WaitAsync(TimeSpan.FromSeconds(5)); vm.SelectedNode = vm.FlattenNodes().First(row => row.Source.Clickable);
                var export = await Export(state, "same-b-before"); var projection = Projection.Take(state); var bCache = Cache.Take(state.B);
                gate.Release.TrySetResult(); await pending.WaitAsync(TimeSpan.FromSeconds(5));
                projection.CheckSame(state, "Current始终B但较旧Automatic批次不得清期间新显式B检查", feedback: true);
                state.Check(state.Backend.Calls().Count(call => call.Operation == "Screenshot" && call.Serial == state.B.Serial) == 2 && !ReferenceEquals(state.B.Screenshot, bCache.Image), "旧批仍完成身份有效B缓存，不靠丢后台能力过关");
                state.Check(await Export(state, "same-b-after") == export, "同Current较新B导出根保持"); Completed(state);
            });
            foreach (var automatic in new[] { false, true })
            foreach (var noCurrent in new[] { false, true })
            {
                await RunCase($"batch-{(automatic ? "automatic" : "wall")}-{(noCurrent ? "null" : "current-b")}", async state =>
                {
                    var vm = state.ViewModel; vm.CurrentDevice = noCurrent ? null : state.B;
                    var before = vm.Devices.ToDictionary(row => row, Cache.Take);
                    var serials = new[] { state.A.Serial, state.B.Serial };
                    await state.Track(automatic ? vm.RefreshAutomaticScreensAsync(serials, CancellationToken.None) : vm.RefreshWallAsync()).WaitAsync(TimeSpan.FromSeconds(5));
                    state.Check(vm.Devices.All(row => row.Screenshot is not null && !ReferenceEquals(row.Screenshot, before[row].Image) && row.CapturedAt is not null), "Wall/Automatic在null或非首行Current仍更新全部合法源缓存");
                    state.Check(state.Backend.Calls().Where(call => call.Operation == "Screenshot").Select(call => call.Serial).SequenceEqual(serials), "后台/墙批次实际来源顺序完整");
                    if (noCurrent) state.Check(vm.CurrentDevice is null && vm.DebugImage is null && vm.Nodes.Count == 0 && Root(vm) is null, "null Current不伪造检查目标");
                    else state.Check(ReferenceEquals(vm.DebugImage, state.B.Screenshot), "批次成功拍当前B时仍可发布新图");
                    state.Check(!vm.Busy && vm.Error.Length == 0, "正常批次恢复Busy且无错误"); state.Observe("normal-batch");
                });
            }
            await RunCase("automatic-explicit-a-cache-return", async state =>
            {
                var vm = state.ViewModel; vm.CurrentDevice = state.B; await state.Track(vm.LoadNodesAsync()).WaitAsync(TimeSpan.FromSeconds(5)); vm.SelectedNode = vm.FlattenNodes().First();
                var projection = Projection.Take(state); var bCache = Cache.Take(state.B);
                await state.Track(vm.RefreshAutomaticScreensAsync([state.A.Serial], CancellationToken.None)).WaitAsync(TimeSpan.FromSeconds(5));
                projection.CheckSame(state, "显式串号A后台不拍B也不清B旧节点", feedback: true); bCache.CheckSame(state, state.B, "Automatic显式集合不改B缓存");
                state.Check(state.Backend.Calls().Where(call => call.Operation == "Screenshot").Select(call => call.Serial).SequenceEqual(new[] { state.B.Serial, state.A.Serial }), "实际自动截图仅追加指定A");
                vm.CurrentDevice = state.A; state.Check(state.A.Screenshot is not null && ReferenceEquals(vm.DebugImage, state.A.Screenshot) && vm.Nodes.Count == 0 && Root(vm) is null, "返回A仍可恢复后台合法缓存而不是删除缓存掩盖竞态");
            });
            await RunCase("wall-does-not-project-unphotographed-b", async state =>
            {
                var vm = state.ViewModel; vm.CurrentDevice = state.B; await state.Track(vm.LoadNodesAsync()).WaitAsync(TimeSpan.FromSeconds(5)); vm.SelectedNode = vm.FlattenNodes().First();
                state.Backend.Override(state.B.Serial, info => info with { State = "Offline" }); await state.Track(vm.RefreshDeviceSnapshotAsync()).WaitAsync(TimeSpan.FromSeconds(5));
                var projection = Projection.Take(state); var bCache = Cache.Take(state.B); state.Backend.ClearCalls();
                await state.Track(vm.RefreshWallAsync()).WaitAsync(TimeSpan.FromSeconds(5));
                projection.CheckSame(state, "Wall未拍当前离线B时不清B已有检查"); bCache.CheckSame(state, state.B, "Wall未拍B不改B缓存");
                state.Check(state.A.Screenshot is not null && state.Backend.Calls().Where(call => call.Operation == "Screenshot").Select(call => call.Serial).SequenceEqual(new[] { state.A.Serial }), "无当前拍摄目标仍保留Wall合法A缓存"); Completed(state);
            });
            foreach (var automatic in new[] { false, true })
            foreach (var failPayload in new[] { false, true })
            {
                await RunCase("batch-invalid-a-continues-b-" + (automatic ? "automatic" : "wall") + (failPayload ? "-error" : "-success"), async state =>
                {
                    var vm = state.ViewModel; vm.CurrentDevice = null;
                    var marker = "controlled-invalid-source-" + automatic; if (failPayload) state.ExpectLog(marker, state.A.Serial);
                    var gate = state.Backend.Hold("Screenshot", state.A.Serial, failPayload ? new DeviceException(marker) : null); var pending = state.Track(automatic ? vm.RefreshAutomaticScreensAsync([state.A.Serial, state.B.Serial], CancellationToken.None) : vm.RefreshWallAsync());
                    await Entered(state, gate, pending, busy: !automatic); state.Backend.Override(state.A.Serial, info => info with { TransportId = 91 }); await state.Track(vm.Runtime.RefreshAsync()).WaitAsync(TimeSpan.FromSeconds(5));
                    var cache = Cache.Take(state.A); gate.Release.TrySetResult(); await pending.WaitAsync(TimeSpan.FromSeconds(5));
                    cache.CheckSame(state, state.A, "批次A仅Catalog身份失效不写旧源cache");
                    state.Check(state.B.Screenshot is not null && state.Backend.Calls().Any(call => call.Operation == "Screenshot" && call.Serial == state.B.Serial) && vm.CurrentDevice is null && vm.DebugImage is null && !vm.Busy && vm.Error.Length == 0, "跳过失效A仍采合法B，不取消整个批次或制造Current");
                });
            }
            foreach (var nodes in new[] { false, true })
            {
                await RunCase("busy-noop-preserves-" + (nodes ? "nodes" : "capture"), async state =>
                {
                    var vm = state.ViewModel; vm.CurrentDevice = state.A; var gate = state.Backend.Hold("Screenshot", state.A.Serial); var pending = Explicit(state, nodes);
                    await Entered(state, gate, pending); var calls = state.Backend.Calls(); var status = vm.Status;
                    await state.Track(vm.CaptureAsync()).WaitAsync(TimeSpan.FromSeconds(5)); await state.Track(vm.LoadNodesAsync()).WaitAsync(TimeSpan.FromSeconds(5)); await state.Track(vm.RefreshWallAsync()).WaitAsync(TimeSpan.FromSeconds(5));
                    var originalBusy = vm.Busy; var originalPending = !pending.IsCompleted; var sameCalls = state.Backend.Calls().SequenceEqual(calls); var sameStatus = vm.Status == status; var noError = vm.Error.Length == 0;
                    state.Observations.Add(new { phase = "busy-noop-before-release", originalBusy, originalPending, sameCalls, sameStatus, noError, originalStatus = status, actualStatus = vm.Status, vm.Error, expectedCalls = calls, actualCalls = state.Backend.Calls() });
                    state.Check(originalBusy && originalPending && sameCalls && sameStatus && noError, $"Busy三入口重入是no-op，不加请求/改变反馈或废弃在途检查修订号; busy={originalBusy}; pending={originalPending}; calls={sameCalls}; status={sameStatus}; errorEmpty={noError}");
                    gate.Release.TrySetResult(); await pending.WaitAsync(TimeSpan.FromSeconds(5)); Completed(state);
                    state.Check(ReferenceEquals(vm.DebugImage, state.A.Screenshot) && vm.DebugImage is not null && (!nodes || vm.Nodes.Count > 0), "原有效读在Busy no-op后仍被接纳");
                });
            }
            foreach (var scenario in new[] { (Nodes: false, Operation: "Screenshot"), (Nodes: true, Operation: "Screenshot"), (Nodes: true, Operation: "Dump"), (Nodes: true, Operation: "Discover") })
            {
                await RunCase($"stale-error-{(scenario.Nodes ? "nodes" : "capture")}-{scenario.Operation}", async state =>
                {
                    var vm = state.ViewModel; await SeedA(state); var marker = "controlled-stale-" + scenario.Nodes + "-" + scenario.Operation;
                    state.ExpectLog(marker, state.A.Serial);
                    var gate = state.Backend.Hold(scenario.Operation, scenario.Operation == "Discover" ? null : state.A.Serial, new DeviceException(marker)); var pending = Explicit(state, scenario.Nodes);
                    await Entered(state, gate, pending); vm.CurrentDevice = state.B; var projection = Projection.Take(state); state.Observe("new-b-before-stale-error");
                    gate.Release.TrySetResult(); await pending.WaitAsync(TimeSpan.FromSeconds(5));
                    projection.CheckSame(state, "过期A异常不污染B检查"); Cancelled(state); await Recover(state);
                });
            }
            foreach (var automatic in new[] { false, true })
            {
                await RunCase("batch-source-error-" + (automatic ? "automatic" : "wall"), async state =>
                {
                    var vm = state.ViewModel; vm.CurrentDevice = state.B; await state.Track(vm.LoadNodesAsync()).WaitAsync(TimeSpan.FromSeconds(5)); vm.SelectedNode = vm.FlattenNodes().First();
                    var projection = Projection.Take(state); var marker = "controlled-batch-source-" + automatic; state.ExpectLog(marker, state.A.Serial);
                    var injected = new DeviceException(marker); var expectedScreenState = "截图失败：" + UiErrorPresentation.Describe(injected);
                    var gate = state.Backend.Hold("Screenshot", state.A.Serial, injected);
                    var pending = state.Track(automatic ? vm.RefreshAutomaticScreensAsync([state.A.Serial], CancellationToken.None) : vm.RefreshWallAsync());
                    await Entered(state, gate, pending, busy: !automatic); gate.Release.TrySetResult(); await pending.WaitAsync(TimeSpan.FromSeconds(5));
                    state.Observations.Add(new { phase = "batch-source-safe-feedback", expectedScreenState, actualScreenState = state.A.ScreenState, vm.Error, vm.Busy, markerExposed = state.A.ScreenState.Contains(marker, StringComparison.Ordinal) });
                    state.Check(state.A.ScreenState == expectedScreenState && !state.A.ScreenState.Contains(marker, StringComparison.Ordinal) && vm.Error.Length == 0 && !vm.Busy, "身份仍有效A失败使用既有安全中文映射，只更新对应源截图状态且不误报B全局Error");
                    if (automatic) projection.CheckSame(state, "仅A失败的后台不能清B检查或改B显式反馈", feedback: true);
                    else state.Check(state.B.Screenshot is not null && state.Backend.Calls().Any(call => call.Operation == "Screenshot" && call.Serial == state.B.Serial), "Wall在A异常后仍继续合法B");
                    await Recover(state);
                });
            }
            foreach (var scenario in new[] { (Nodes: false, Operation: "Screenshot", Cancelled: false), (Nodes: true, Operation: "Screenshot", Cancelled: false), (Nodes: true, Operation: "Dump", Cancelled: false), (Nodes: true, Operation: "Screenshot", Cancelled: true) })
            {
                await RunCase("backend-failure-recovery-" + (scenario.Nodes ? "nodes" : "capture") + "-" + scenario.Operation + (scenario.Cancelled ? "-cancelled" : "-error"), async state =>
                {
                    var vm = state.ViewModel; vm.CurrentDevice = state.A;
                    var marker = "controlled " + scenario.Operation + " failure";
                    Exception injected = scenario.Cancelled ? new OperationCanceledException("controlled cancellation") : new DeviceException(marker);
                    var gate = state.Backend.Hold(scenario.Operation, state.A.Serial, injected);
                    var pending = Explicit(state, scenario.Nodes); await Entered(state, gate, pending); gate.Release.TrySetResult(); await pending.WaitAsync(TimeSpan.FromSeconds(5));
                    if (scenario.Cancelled) Cancelled(state);
                    else
                    {
                        var expectedError = (scenario.Nodes ? "刷新截图与节点" : "刷新截图") + "失败：" + UiErrorPresentation.Describe(injected);
                        state.Observations.Add(new { phase = "same-target-safe-feedback", expectedError, vm.Error, vm.Busy, vm.Status, nodeCount = vm.Nodes.Count, exportRootAbsent = Root(vm) is null, markerExposed = vm.Error.Contains(marker, StringComparison.Ordinal) });
                        state.Check(!vm.Busy && vm.Error == expectedError && !vm.Error.Contains(marker, StringComparison.Ordinal) && vm.Status == "操作失败" && vm.Nodes.Count == 0 && Root(vm) is null, "真实后端异常通过既有安全中文Error/Status反馈且无假节点成功，不以Task返回当通过");
                    }
                    await Recover(state);
                });
            }
            foreach (var externalCancellation in new[] { false, true })
            {
                await RunCase("automatic-cancellation-" + (externalCancellation ? "external-token" : "backend-oce"), async state =>
                {
                    var vm = state.ViewModel; await SeedA(state); vm.CurrentDevice = state.B;
                    await state.Track(vm.LoadNodesAsync()).WaitAsync(TimeSpan.FromSeconds(5)); vm.SelectedNode = vm.FlattenNodes().First(row => row.Source.Clickable); state.Backend.ClearCalls();
                    var cacheA = Cache.Take(state.A); var cacheB = Cache.Take(state.B); var projection = Projection.Take(state);
                    using var cancellation = new CancellationTokenSource();
                    var gate = state.Backend.Hold("Screenshot", state.A.Serial, externalCancellation ? null : new OperationCanceledException("controlled automatic backend cancellation"));
                    var pending = vm.RefreshAutomaticScreensAsync([state.A.Serial, state.B.Serial], externalCancellation ? cancellation.Token : CancellationToken.None);
                    Exception? outcome = null;
                    // Drain the observer, then assert the original task outcome without treating expected cancellation as cleanup failure.
                    var observed = state.Track(ObserveOutcomeAsync());
                    await Entered(state, gate, pending, busy: false);
                    if (externalCancellation) cancellation.Cancel();
                    gate.Release.TrySetResult(); await observed.WaitAsync(TimeSpan.FromSeconds(5));
                    var observedCancellation = outcome is OperationCanceledException; var taskCancelled = pending.IsCanceled;
                    var onlySourceA = state.Backend.Calls().Where(call => call.Operation == "Screenshot").Select(call => call.Serial).SequenceEqual(new[] { state.A.Serial });
                    state.Observations.Add(new { phase = "automatic-cancellation-outcome", externalCancellation, observedCancellation, taskCancelled, originalTaskStatus = pending.Status.ToString(), outcomeType = outcome?.GetType().FullName, outcome = outcome?.ToString(), onlySourceA, cancellationTokenRequested = cancellation.IsCancellationRequested });
                    state.Observe("automatic-cancellation-before-recovery");
                    state.Check(observedCancellation && taskCancelled && onlySourceA, $"Automatic取消必须真实观察OCE且原Task取消、不继续B; outcome={outcome?.GetType().FullName ?? "none"}; task={pending.Status}; onlyA={onlySourceA}");
                    cacheA.CheckSame(state, state.A, "Automatic取消不改A缓存与状态"); cacheB.CheckSame(state, state.B, "Automatic取消不改B缓存与状态");
                    projection.CheckSame(state, "Automatic取消不污染当前B检查或显式反馈", feedback: true);
                    var callCount = state.Backend.Calls().Length;
                    await state.Track(vm.RefreshAutomaticScreensAsync([state.A.Serial, state.B.Serial], CancellationToken.None)).WaitAsync(TimeSpan.FromSeconds(5));
                    state.Check(state.Backend.Calls().Skip(callCount).Where(call => call.Operation == "Screenshot").Select(call => call.Serial).SequenceEqual(new[] { state.A.Serial, state.B.Serial })
                        && state.A.Screenshot is not null && !ReferenceEquals(state.A.Screenshot, cacheA.Image) && state.B.Screenshot is not null && !ReferenceEquals(state.B.Screenshot, cacheB.Image)
                        && ReferenceEquals(vm.DebugImage, state.B.Screenshot) && !vm.Busy && vm.Error.Length == 0, "Automatic取消后真实无取消批次仍可恢复A/B截图及当前B投影");

                    async Task ObserveOutcomeAsync()
                    {
                        try { await pending; }
                        catch (Exception error) { outcome = error; }
                    }
                });
            }
            await RunCase("wall-backend-oce-is-source-failure", async state =>
            {
                var vm = state.ViewModel; await SeedA(state); vm.CurrentDevice = state.B;
                await state.Track(vm.LoadNodesAsync()).WaitAsync(TimeSpan.FromSeconds(5)); vm.SelectedNode = vm.FlattenNodes().First(row => row.Source.Clickable); state.Backend.ClearCalls();
                var cacheA = Cache.Take(state.A); var cacheB = Cache.Take(state.B); var marker = "controlled wall backend cancellation";
                var injected = new OperationCanceledException(marker); var expectedScreenState = "截图失败：" + UiErrorPresentation.Describe(injected); state.ExpectLog(marker, state.A.Serial);
                var gate = state.Backend.Hold("Screenshot", state.A.Serial, injected); var pending = state.Track(vm.RefreshWallAsync());
                await Entered(state, gate, pending); gate.Release.TrySetResult(); await pending.WaitAsync(TimeSpan.FromSeconds(5));
                state.Observations.Add(new { phase = "wall-oce-source-feedback", expectedScreenState, actualScreenState = state.A.ScreenState, taskStatus = pending.Status.ToString(), vm.Error, vm.Status, vm.Busy });
                state.Check(state.A.ScreenState == expectedScreenState && !state.A.ScreenState.Contains(marker, StringComparison.Ordinal) && ReferenceEquals(state.A.Screenshot, cacheA.Image) && state.A.CapturedAt == cacheA.CapturedAt
                    && vm.Error.Length == 0 && !vm.Busy && !pending.IsCanceled, "Wall将单源OCE按既有安全源失败处理，不取消整批或伪造A缓存成功");
                state.Check(state.Backend.Calls().Where(call => call.Operation == "Screenshot").Select(call => call.Serial).SequenceEqual(new[] { state.A.Serial, state.B.Serial })
                    && state.B.Screenshot is not null && !ReferenceEquals(state.B.Screenshot, cacheB.Image) && ReferenceEquals(vm.DebugImage, state.B.Screenshot), "Wall的A单源OCE后仍实际截图B并更新合法缓存和当前投影");
                Completed(state);
            });
            phase = "complete";
            if (caseFailures.Count > 0) throw new AggregateException("独立Demo调试发布用例失败", caseFailures);
        }
        catch (Exception error) { failure = error; Console.Error.WriteLine(error); }
        finally
        {
            try
            {
                await File.WriteAllTextAsync(Path.Combine(directory, "debug-publication.json"), JsonSerializer.Serialize(new
                {
                    status = failure is null ? "PASS-OFFLINE-DESKTOP-VM" : "FAIL",
                    scope = "Isolated Desktop STA VM entrypoints, deterministic Demo wrapper completion; not routed buttons, automatic timer/first-load, Android or ADB transport proof",
                    phase, reports, runtimeHotSwap = "NOT-APPLICABLE: Demo has no legal same-VM runtime replacement; no reflection mutation or forged authorization", error = failure?.ToString()
                }, JsonDefaults.Options));
            }
            catch (Exception reportError) { Console.Error.WriteLine(reportError); failure = failure is null ? reportError : new AggregateException("调试发布验收与报告保存均失败", failure, reportError); }
        }
        if (failure is not null) ExceptionDispatchInfo.Capture(failure).Throw();

        async Task RunCase(string id, Func<CaseState, Task> action)
        {
            CaseState? state = null; Exception? caseFailure = null; var cleanupCompleted = false;
            phase = id;
            try { state = await CaseState.CreateAsync(Path.Combine(directory, id)); await action(state); }
            catch (Exception error) { caseFailure = error; Console.Error.WriteLine(error); }
            finally
            {
                if (state is not null)
                {
                    try { await state.CleanupAsync(); cleanupCompleted = true; }
                    catch (Exception cleanupError) { caseFailure = caseFailure is null ? cleanupError : new AggregateException("调试发布用例与清理均失败", caseFailure, cleanupError); }
                    if (cleanupCompleted)
                    {
                        try { await state.VerifyLogsAsync(); }
                        catch (Exception logError) { caseFailure = caseFailure is null ? logError : new AggregateException("调试发布用例与已flush日志核验均失败", caseFailure, logError); }
                    }
                }
                reports.Add(new { id, status = caseFailure is null ? "PASS-OFFLINE-DESKTOP-VM" : "FAIL", checks = state?.Checks.ToArray(), observations = state?.Observations.ToArray(), calls = state?.Backend.Calls(), payloads = state?.Backend.Payloads(), gates = state?.Backend.GateStates(), cleanupCompleted, error = caseFailure?.ToString() });
            }
            if (caseFailure is not null)
            {
                caseFailures.Add(new InvalidOperationException(id, caseFailure));
                if (state is null || !cleanupCompleted) ExceptionDispatchInfo.Capture(caseFailure).Throw();
            }
        }

        static Task Explicit(CaseState state, bool nodes) => state.Track(nodes ? state.ViewModel.LoadNodesAsync() : state.ViewModel.CaptureAsync());
        static async Task SeedA(CaseState state) { state.ViewModel.CurrentDevice = state.A; await state.Track(state.ViewModel.LoadNodesAsync()).WaitAsync(TimeSpan.FromSeconds(5)); Completed(state); state.Backend.ClearCalls(); }
        static async Task Entered(CaseState state, CompletionGate gate, Task pending, bool busy = true) { await gate.Entered.Task.WaitAsync(TimeSpan.FromSeconds(5)); state.Observe("gate-entered-" + gate.Operation); state.Check(!pending.IsCompleted && state.ViewModel.Busy == busy, $"TCS已取得旧payload并确定挂起，Busy符合真实入口; pending={!pending.IsCompleted}; expectedBusy={busy}; actualBusy={state.ViewModel.Busy}"); }
        static void Completed(CaseState state) => state.Check(!state.ViewModel.Busy && state.ViewModel.Error.Length == 0 && state.ViewModel.Status.Contains("完成", StringComparison.Ordinal), $"有效显式请求恢复Busy/Error且明确完成; busy={state.ViewModel.Busy}; error={state.ViewModel.Error}; status={state.ViewModel.Status}");
        static void Cancelled(CaseState state) => state.Check(!state.ViewModel.Busy && state.ViewModel.Error.Length == 0 && state.ViewModel.Status.Contains("已停止", StringComparison.Ordinal), $"过期或取消显式读按停止反馈且无假错误; busy={state.ViewModel.Busy}; error={state.ViewModel.Error}; status={state.ViewModel.Status}");
        static async Task Recover(CaseState state) { await state.Track(state.ViewModel.LoadNodesAsync()).WaitAsync(TimeSpan.FromSeconds(5)); Completed(state); state.Check(state.ViewModel.Nodes.Count > 0 && ReferenceEquals(state.ViewModel.DebugImage, state.ViewModel.CurrentDevice?.Screenshot), "后续真实新请求可恢复图和节点"); }
        static async Task<string> Export(CaseState state, string name) { var path = Path.Combine(state.Directory, name + ".json"); await state.Track(state.ViewModel.ExportNodesAsync(path)).WaitAsync(TimeSpan.FromSeconds(5)); state.Check(state.ViewModel.Error.Length == 0 && File.Exists(path), "真实节点导出成功: " + name); return await File.ReadAllTextAsync(path); }
    }

    private static AndroidNode? Root(WorkspaceViewModel viewModel)
    {
        var field = typeof(WorkspaceViewModel).GetField("nodeRoot", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("已核导出根字段nodeRoot不存在");
        if (field.FieldType != typeof(AndroidNode)) throw new InvalidOperationException("nodeRoot字段类型已变化");
        return field.GetValue(viewModel) as AndroidNode;
    }
    private static int? Id(object? value) => value is null ? null : System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(value);
    private sealed record Cache(BitmapSource? Image, DateTimeOffset? CapturedAt, string ScreenState, string AutomationState)
    {
        internal static Cache Take(DeviceRow row) => new(row.Screenshot, row.CapturedAt, row.ScreenState, row.AutomationState);
        internal void CheckSame(CaseState state, DeviceRow row, string reason)
        {
            var sameImage = ReferenceEquals(Image, row.Screenshot); var sameTime = CapturedAt == row.CapturedAt; var sameScreen = ScreenState == row.ScreenState; var sameAutomation = AutomationState == row.AutomationState;
            state.Observations.Add(new { reason, row.Serial, sameImage, sameTime, sameScreen, sameAutomation, before = new { imageId = Id(Image), CapturedAt, ScreenState, AutomationState }, after = new { imageId = Id(row.Screenshot), row.CapturedAt, row.ScreenState, row.AutomationState } });
            state.Check(sameImage && sameTime && sameScreen && sameAutomation, $"{reason}; image={sameImage}; timestamp={sameTime}; screen={sameScreen} ({ScreenState}->{row.ScreenState}); automation={sameAutomation} ({AutomationState}->{row.AutomationState})");
        }
    }
    private sealed record Projection(DeviceRow? Current, DeviceRow? Main, BitmapSource? Image, NodeRow[] Nodes, NodeRow? Selected, AndroidNode? ExportRoot, string Properties, string? AutomationState, DeviceRow[] Targets, int NodeEvents, string Error, string Status)
    {
        internal static Projection Take(CaseState state)
        {
            var vm = state.ViewModel;
            return new(vm.CurrentDevice, vm.MainDevice, vm.DebugImage, vm.Nodes.ToArray(), vm.SelectedNode, Root(vm), vm.NodeProperties, vm.CurrentDevice?.AutomationState, vm.Targets().ToArray(), state.NodeEvents, vm.Error, vm.Status);
        }
        internal void CheckSame(CaseState state, string reason, bool feedback = false)
        {
            var vm = state.ViewModel; var sameCurrent = ReferenceEquals(Current, vm.CurrentDevice); var sameMain = ReferenceEquals(Main, vm.MainDevice); var sameImage = ReferenceEquals(Image, vm.DebugImage); var sameNodes = Nodes.SequenceEqual(vm.Nodes);
            var sameSelected = ReferenceEquals(Selected, vm.SelectedNode); var sameExport = ReferenceEquals(ExportRoot, Root(vm)); var sameProperties = Properties == vm.NodeProperties; var sameAutomation = AutomationState == vm.CurrentDevice?.AutomationState;
            var sameTargets = Targets.SequenceEqual(vm.Targets()); var sameEvents = NodeEvents == state.NodeEvents; var sameError = Error == vm.Error; var sameStatus = Status == vm.Status;
            state.Observations.Add(new { reason, sameCurrent, sameMain, sameImage, sameNodes, sameSelected, sameExport, sameProperties, sameAutomation, sameTargets, sameEvents, sameError, sameStatus, feedback,
                before = new { currentId = Id(Current), mainId = Id(Main), imageId = Id(Image), nodes = Nodes.Select(node => Id(node)).ToArray(), selectedId = Id(Selected), rootId = Id(ExportRoot), AutomationState, NodeEvents, Error, Status },
                after = new { currentId = Id(vm.CurrentDevice), mainId = Id(vm.MainDevice), imageId = Id(vm.DebugImage), nodes = vm.Nodes.Select(node => Id(node)).ToArray(), selectedId = Id(vm.SelectedNode), rootId = Id(Root(vm)), automationState = vm.CurrentDevice?.AutomationState, state.NodeEvents, vm.Error, vm.Status } });
            state.Check(sameCurrent && sameMain && sameImage && sameNodes && sameSelected && sameExport && sameProperties && sameAutomation && sameTargets && sameEvents && (!feedback || sameError && sameStatus),
                $"{reason}; current={sameCurrent}; main={sameMain}; image={sameImage}; nodes={sameNodes}; selected={sameSelected}; export={sameExport}; properties={sameProperties}; automation={sameAutomation}; targets={sameTargets}; nodeEvents={sameEvents} ({NodeEvents}->{state.NodeEvents}); error={sameError}; status={sameStatus}; feedback={feedback}");
        }
    }

    private sealed class CaseState
    {
        internal readonly string Directory;
        internal readonly ControlledBackend Backend;
        internal readonly WorkspaceViewModel ViewModel;
        internal readonly List<string> Checks = [];
        internal readonly List<object> Observations = [];
        private readonly List<Task> operations = [];
        private readonly List<(string Marker, string Serial)> expectedLogs = [];
        internal int NodeEvents;
        internal DeviceRow A => ViewModel.Devices.Single(row => row.Serial == "DEMO-001");
        internal DeviceRow B => ViewModel.Devices.Single(row => row.Serial == "DEMO-002");
        private CaseState(string directory, ControlledBackend backend, WorkspaceViewModel viewModel)
        {
            Directory = directory; Backend = backend; ViewModel = viewModel;
            viewModel.NodesChanged = () => NodeEvents++;
        }
        internal static async Task<CaseState> CreateAsync(string directory)
        {
            System.IO.Directory.CreateDirectory(directory);
            var backend = new ControlledBackend(); CatalogRepository? metadata = null; RollingLog? log = null; MatrixRuntime? runtime = null; CaseState? state = null;
            try
            {
                metadata = new CatalogRepository(Path.Combine(directory, "db.sqlite3")); log = new RollingLog(Path.Combine(directory, "logs"));
                var catalog = new DeviceCatalog(backend); var automation = new AutomationService(backend, catalog); var scripts = new ScriptScheduler(backend); var input = new InputCoordinator(catalog, backend);
                var api = new ApiServiceManager(catalog, backend, automation, log); var settings = new AppSettings { AutoStartApi = false, AutoConnect = false, SyncMouse = false, SyncKeyboard = false };
                runtime = new MatrixRuntime { Backend = backend, Devices = catalog, Automation = automation, Scripts = scripts, Input = input, Api = api, SettingsStore = new SettingsStore(Path.Combine(directory, "AppConfig.json")), Settings = settings, FrozenConnectionSettings = settings, Metadata = metadata, Log = log, IsDemo = true };
                api.DeviceSnapshotProvider = runtime.GetDevices;
                var vm = new WorkspaceViewModel(runtime, directory, true); state = new CaseState(directory, backend, vm);
                await state.Track(vm.RefreshDevicesAsync()).WaitAsync(TimeSpan.FromSeconds(5));
                state.Check(vm.Demo && runtime.IsDemo && !api.BasicRunning && !api.AutomatorRunning && api.DevicePorts.Count == 0 && vm.Devices.Count == 2 && vm.Devices.All(row => row.Info.Online), "隔离Demo一致运行时已初始化且未启动API");
                backend.ClearCalls(); state.NodeEvents = 0;
                return state;
            }
            catch (Exception error)
            {
                try
                {
                    if (state is not null) await state.CleanupAsync();
                    else if (runtime is not null) await runtime.DisposeAsync();
                    else { await backend.DisposeAsync(); metadata?.Dispose(); if (log is not null) await log.DisposeAsync(); }
                }
                catch (Exception cleanupError) { throw new AggregateException("夹具初始化与清理均失败", error, cleanupError); }
                throw;
            }
        }
        internal Task Track(Task operation) { operations.Add(operation); return operation; }
        internal void ExpectLog(string marker, string serial) => expectedLogs.Add((marker, serial));
        internal void Check(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); Checks.Add(message); }
        internal void Observe(string stage)
        {
            int? Id(object? value) => value is null ? null : System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(value);
            var vm = ViewModel;
            Observations.Add(new
            {
                stage, vm.Busy, vm.Error, vm.Status, currentSerial = vm.CurrentDevice?.Serial, currentId = Id(vm.CurrentDevice), mainSerial = vm.MainDevice?.Serial,
                debugImageId = Id(vm.DebugImage), debugWidth = vm.DebugImage?.PixelWidth, debugHeight = vm.DebugImage?.PixelHeight, exportRootId = Id(Root(vm)), exportMarker = Root(vm)?.Text,
                nodes = vm.Nodes.Select(row => new { id = Id(row), row.Source.Text, row.Source.ResourceId }).ToArray(), selectedNodeId = Id(vm.SelectedNode), NodeEvents,
                rows = vm.Devices.Select(row => new { row.Serial, id = Id(row), row.Info.Generation, row.Info.TransportId, row.Info.Width, row.Info.Height, row.Info.Rotation, row.Info.State, screenshotId = Id(row.Screenshot), row.CapturedAt, row.ScreenState, row.AutomationState }).ToArray(),
                catalog = vm.Runtime.GetDevices().Select(info => new { info.Serial, info.Generation, info.TransportId, info.Width, info.Height, info.Rotation, info.State }).ToArray()
            });
        }
        internal async Task CleanupAsync()
        {
            Backend.ReleaseAll();
            Exception? failure = null;
            try { await Task.WhenAll(operations).WaitAsync(TimeSpan.FromSeconds(5)); }
            catch (Exception error) { failure = error; }
            if (operations.Any(operation => !operation.IsCompleted)) throw new InvalidOperationException("自有待飞行任务未排空，未谎称完成清理。", failure);
            try { Observe("cleanup-drained-before-shutdown"); Check(Backend.GateStates().All(gate => gate.EnteredCompleted && gate.ReleaseCompleted) && !ViewModel.Busy, "finally已释放全部自有TCS且实际VM操作均已排空并恢复Busy"); }
            catch (Exception error) { failure = failure is null ? error : new AggregateException(failure, error); }
            ViewModel.NodesChanged = null;
            try { await ViewModel.ShutdownAsync().WaitAsync(TimeSpan.FromSeconds(5)); }
            catch (Exception error) { failure = failure is null ? error : new AggregateException(failure, error); }
            if (failure is not null) ExceptionDispatchInfo.Capture(failure).Throw();
        }
        internal async Task VerifyLogsAsync()
        {
            if (expectedLogs.Count == 0) return;
            var files = System.IO.Directory.GetFiles(Path.Combine(Directory, "logs"), "*.log"); var lines = new List<string>();
            foreach (var file in files) lines.AddRange(await File.ReadAllLinesAsync(file));
            foreach (var expected in expectedLogs)
            {
                var matched = lines.Where(line => line.Contains(expected.Marker, StringComparison.Ordinal)).Select(line => System.Text.RegularExpressions.Regex.Match(line, @"^\S+ \[(?<category>[^\]]+)\] \[(?<serial>[^\]]+)\] (?<message>.*)$")).ToArray();
                var parsed = matched.All(match => match.Success); var sameSerial = matched.All(match => match.Success && match.Groups["serial"].Value == expected.Serial);
                Observations.Add(new { phase = "after-shutdown-disk-log", expected.Marker, expected.Serial, matchedCount = matched.Length, parsed, sameSerial, entries = matched.Select(match => new { category = match.Groups["category"].Value, serial = match.Groups["serial"].Value }).ToArray(), files = files.Select(file => new { name = Path.GetFileName(file), sha256 = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(file))) }).ToArray(), lastLogError = ViewModel.Runtime.Log.LastError, realUtf8RollingLogNotJsonl = true });
                Check(matched.Length > 0 && parsed && sameSerial && ViewModel.Runtime.Log.LastError is null, $"真实RollingLog已flush且故障归属固定源{expected.Serial}; marker={expected.Marker}; matches={matched.Length}; parsed={parsed}; sourceSerialOnly={sameSerial}; writerError={ViewModel.Runtime.Log.LastError}");
            }
        }
    }

    private sealed record BackendCall(int Sequence, string Operation, string? Serial);
    private sealed record PayloadProof(int Sequence, string Operation, string? Serial, string Marker, string? Sha256, DeviceInfo[]? Discovery, int? Width, int? Height);
    private sealed record GateState(string Operation, string? Serial, bool Claimed, bool EnteredCompleted, bool ReleaseCompleted);
    private sealed class CompletionGate(string operation, string? serial, Exception? failure)
    {
        internal readonly string Operation = operation;
        internal readonly string? Serial = serial;
        internal readonly Exception? Failure = failure;
        internal readonly TaskCompletionSource Entered = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal readonly TaskCompletionSource Release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal bool Claimed;
    }
    private sealed class ControlledBackend : IDeviceBackend
    {
        private readonly DemoDeviceBackend inner = new();
        private readonly object sync = new();
        private readonly List<BackendCall> calls = [];
        private readonly List<CompletionGate> gates = [];
        private readonly List<PayloadProof> payloads = [];
        private readonly Dictionary<string, Func<DeviceInfo, DeviceInfo>> overrides = [];
        private readonly Dictionary<string, DeviceInfo> reported = [];
        internal BackendCall[] Calls() { lock (sync) return calls.ToArray(); }
        internal PayloadProof[] Payloads() { lock (sync) return payloads.ToArray(); }
        internal GateState[] GateStates() { lock (sync) return gates.Select(gate => new GateState(gate.Operation, gate.Serial, gate.Claimed, gate.Entered.Task.IsCompleted, gate.Release.Task.IsCompleted)).ToArray(); }
        internal void ClearCalls() { lock (sync) { calls.Clear(); payloads.Clear(); } }
        internal CompletionGate Hold(string operation, string? serial = null, Exception? failure = null) { var gate = new CompletionGate(operation, serial, failure); lock (sync) gates.Add(gate); return gate; }
        internal void Override(string serial, Func<DeviceInfo, DeviceInfo>? transform) { lock (sync) { if (transform is null) overrides.Remove(serial); else overrides[serial] = transform; } }
        internal void ReleaseAll() { lock (sync) foreach (var gate in gates) { gate.Release.TrySetResult(); if (!gate.Claimed) gate.Entered.TrySetCanceled(); } }
        private int Record(string operation, string? serial) { lock (sync) { var sequence = calls.Count + 1; calls.Add(new(sequence, operation, serial)); return sequence; } }
        private async Task CompleteAsync(string operation, string? serial, CancellationToken ct)
        {
            CompletionGate? gate;
            lock (sync) { gate = gates.FirstOrDefault(item => !item.Claimed && item.Operation == operation && item.Serial == serial); if (gate is not null) gate.Claimed = true; }
            if (gate is null) return;
            gate.Entered.TrySetResult(); await gate.Release.Task.WaitAsync(ct); if (gate.Failure is not null) ExceptionDispatchInfo.Capture(gate.Failure).Throw();
        }
        public async Task<IReadOnlyList<DeviceInfo>> DiscoverAsync(CancellationToken ct = default)
        {
            var sequence = Record("Discover", null); var source = await inner.DiscoverAsync(ct); DeviceInfo[] payload;
            lock (sync) { payload = source.Select(info => overrides.TryGetValue(info.Serial, out var transform) ? transform(info) : info).ToArray(); foreach (var info in payload) reported[info.Serial] = info; payloads.Add(new(sequence, "Discover", null, "discover:" + sequence, null, payload, null, null)); }
            await CompleteAsync("Discover", null, ct); return payload;
        }
        public async Task<byte[]> ScreenshotAsync(string serial, CancellationToken ct = default)
        {
            var sequence = Record("Screenshot", serial); var payload = await inner.ScreenshotAsync(serial, ct); var size = PngImage.ReadDimensions(payload); DeviceInfo? info;
            lock (sync) reported.TryGetValue(serial, out info);
            if (info is not null && (info.Width != size.Width || info.Height != size.Height)) { payload = PngImage.Create(info.Width, info.Height, (x, y) => ((byte)(x % 251), (byte)(y % 251), (byte)sequence)); size = PngImage.ReadDimensions(payload); }
            lock (sync) payloads.Add(new(sequence, "Screenshot", serial, "png:" + sequence, Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(payload)), null, size.Width, size.Height));
            await CompleteAsync("Screenshot", serial, ct); return payload;
        }
        public async Task<AndroidNode> DumpAsync(string serial, CancellationToken ct = default)
        {
            var sequence = Record("Dump", serial); var payload = await inner.DumpAsync(serial, ct); DeviceInfo? info;
            lock (sync) reported.TryGetValue(serial, out info);
            if (info is not null && (info.Width != payload.Bounds.Width || info.Height != payload.Bounds.Height))
            {
                var scaleX = (double)info.Width / payload.Bounds.Width; var scaleY = (double)info.Height / payload.Bounds.Height;
                AndroidNode Scale(AndroidNode node) => node with { Bounds = new((int)(node.Bounds.Left * scaleX), (int)(node.Bounds.Top * scaleY), (int)(node.Bounds.Right * scaleX), (int)(node.Bounds.Bottom * scaleY)), ChildNodeList = node.ChildNodeList.Select(Scale).ToList() };
                payload = Scale(payload);
            }
            payload = payload with { Text = "dump:" + sequence + ":" + serial };
            lock (sync) payloads.Add(new(sequence, "Dump", serial, payload.Text!, null, null, payload.Bounds.Width, payload.Bounds.Height));
            await CompleteAsync("Dump", serial, ct); return payload;
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
