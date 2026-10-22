using System.Collections;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using MatrixDevice.Core;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    internal async Task RunOfflineGeometryDiagnosticsAsync(string[] args)
    {
        var options = new JsonSerializerOptions(JsonDefaults.Options) { DefaultIgnoreCondition = JsonIgnoreCondition.Never, NumberHandling = JsonNumberHandling.Strict };
        var observations = new List<object>(); var checks = new List<string>(); var errors = new List<object>(); Exception? failure = null;
        var identities = new Dictionary<object, int>(ReferenceEqualityComparer.Instance); var path = Path.Combine(ViewModel.DataDirectory, "offline-geometry.json");
        DeviceRow? a = null, b = null; DeviceInfo? ready = null, other = null; bool[]? selected = null; DeviceRow[] rows = []; var prepared = false; var cleanup = false;
        var mainCalls = 0; var screenshotPublications = 0; var dumpReads = 0; var infoWrites = 0; var snapshots = 0;
        var tracked = new List<(string Label, Task Task)>(); var deadlines = new List<CancellationTokenSource>(); using var lifetime = new CancellationTokenSource();
        string? reportPayload = null;
        var originalOpen = mainControlOpen; var originalTag = (tabs.SelectedItem as TabItem)?.Tag as string ?? "设备墙 / 主控";
        var dispatches = mainGestureDispatchCount; var captures = mainCaptureDispatchCount; long preparedInputGeneration = 0;
        object? counterBefore = null, counterAfter = null; bool? sameGenerationHypothesis = null, incrementedGenerationHypothesis = null;
        try
        {
            Require(ViewModel.Demo && ViewModel.Runtime.IsDemo && ViewModel.Runtime.Backend is DemoDeviceBackend && Dispatcher.CheckAccess() && Thread.CurrentThread.GetApartmentState() == ApartmentState.STA, "一致自有 DemoDeviceBackend/STA");
            Require(Path.IsPathFullyQualified(ViewModel.DataDirectory) && Path.GetFullPath(ViewModel.DataDirectory).Contains(Path.DirectorySeparatorChar + "artifacts" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase), "独立 artifacts 绝对目录");
            Require(!ShowActivated && !ViewModel.Runtime.Settings.AutoStartApi && !ViewModel.Runtime.Settings.AutoConnect && !ViewModel.Runtime.Api.BasicRunning && !ViewModel.Runtime.Api.AutomatorRunning && ViewModel.Runtime.Api.DevicePorts.Count == 0 && !ViewModel.Runtime.Api.HasActiveRequests, "窗口不请求激活、接口与自动接入关闭");
            Require(captureOperation is null && !captureInProgress && !ViewModel.Busy && !ViewModel.Transferring && !ViewModel.HasActiveOperations && !DebugAutomaticFixturePermitted && !ReconnectLoadingFixturePermitted, "没有后台截图/业务或自动节点许可；Demo 构造不启动自动截图定时器");
            a = ViewModel.Devices.Single(row => row.Serial == "DEMO-001"); b = ViewModel.Devices.Single(row => row.Serial == "DEMO-002");
            Require(a.Info.Online && b.Info.Online && a.Info.Generation < long.MaxValue, "两自有目标 Ready、代次可递增");
            ready = a.Info; other = b.Info; rows = ViewModel.Devices.ToArray(); selected = rows.Select(row => row.IsSelected).ToArray();
            await Finish("prepare-main", ViewModel.SetMainAsync(a)); mainCalls++;
            ViewModel.CurrentDevice = b; NavigateTo("设备墙 / 主控"); mainControlOpen = true; ApplyDeviceWorkspaceLayout(); UpdateWallSelectionToggle();
            preparedInputGeneration = ViewModel.Runtime.Input.CurrentGeneration; prepared = true;
            Require(ReferenceEquals(ViewModel.MainDevice, a) && ReferenceEquals(ViewModel.CurrentDevice, b), "已准备的自有诊断基线 MainA/CurrentB；不声明恢复启动前 Main=null");
            counterBefore = await Counters();
            var examples = FiniteExamples();
            await Publish(); await Flush();
            var widthIndex = Array.IndexOf(args, "--width"); var heightIndex = Array.IndexOf(args, "--height");
            var requestedWidth = widthIndex >= 0 ? double.Parse(args[widthIndex + 1], CultureInfo.InvariantCulture) : 1440; var requestedHeight = heightIndex >= 0 ? double.Parse(args[heightIndex + 1], CultureInfo.InvariantCulture) : 900;
            observations.Add(new { phase = "window-precondition", requestedWidth = N(requestedWidth), requestedHeight = N(requestedHeight), widthProvided = widthIndex >= 0, heightProvided = heightIndex >= 0, actualWidth = N(ActualWidth), actualHeight = N(ActualHeight), IsLoaded, IsVisible, windowState = WindowState.ToString(), ShowActivated });
            Require(IsLoaded && IsVisible && WindowState == WindowState.Normal && !ShowActivated && Math.Abs(ActualWidth - requestedWidth) < .5 && Math.Abs(ActualHeight - requestedHeight) < .5, "现有固定 Flush 后窗口已加载可见、Normal，实际尺寸与请求相差小于 0.5");
            Require(mainControlImage.Source is BitmapSource && ReferenceEquals(mainControlImage.Source, a.Screenshot) && MainImagePixels(mainControlImage).Width > 0, "原完成截图发布与原 Image 绑定真正就绪");
            observations.Add(new { phase = "safety", backend = ViewModel.Runtime.Backend.GetType().FullName, backgroundWindowRecovery = "SKIPPED-BY-NEW-MODE", ShowActivated, finiteExamples = examples });
            Snapshot("ready-seeded");
            a.Info = ready with { State = "Offline" }; infoWrites++; Snapshot("same-generation-offline-immediate");
            await Flush(); var same = Snapshot("same-generation-offline-settled");
            sameGenerationHypothesis = same.SourceAbsent && same.Visible && same.PixelsZero && !same.FormulaFinite;
            a.Info = ready; infoWrites++; await Publish(); await Flush();
            Require(mainControlImage.Source is BitmapSource && ReferenceEquals(mainControlImage.Source, a.Screenshot), "恢复 Ready 后重新经原完成截图发布"); Snapshot("ready-republished");
            var completed = mainControlImage.Source;
            a.Info = ready with { State = "Offline", Generation = checked(ready.Generation + 1) }; infoWrites++; Snapshot("incremented-offline-immediate");
            await Flush(); var incremented = Snapshot("incremented-offline-settled");
            incrementedGenerationHypothesis = ReferenceEquals(completed, mainControlImage.Source) && !incremented.SourceAbsent && incremented.FormulaFinite;
            counterAfter = await Counters(); Require(JsonSerializer.Serialize(counterBefore, options) == JsonSerializer.Serialize(counterAfter, options) && mainGestureDispatchCount == dispatches && mainCaptureDispatchCount == captures, "Demo 业务计数/输入派发/自动接纳计数无额外动作");
            Require(ViewModel.Runtime.Input.CurrentGeneration == preparedInputGeneration, "准备后诊断不改变输入代次");
        }
        catch (Exception error) { failure = error; errors.Add(new { failureClass = prepared ? "DIAGNOSTIC-FAIL" : "PRECONDITION-FAIL", error = error.ToString() }); }
        finally
        {
            try
            {
                lifetime.Cancel();
                try { await Task.WhenAll(tracked.Select(item => item.Task)).WaitAsync(TimeSpan.FromSeconds(5)); }
                catch (Exception error) { errors.Add(new { failureClass = tracked.All(item => item.Task.IsCompleted) ? "TASK-FAULT-OBSERVED" : "CLEANUP-DRAIN-FAIL", error = error.ToString() }); failure = failure is null ? error : new AggregateException(failure, error); }
                if (a is not null && ready is not null) { a.Info = ready; infoWrites++; }
                if (b is not null && other is not null) b.Info = other;
                if (prepared)
                {
                    if (!ReferenceEquals(ViewModel.MainDevice, a)) { await Finish("restore-main", ViewModel.SetMainAsync(a!)); mainCalls++; }
                    ViewModel.CurrentDevice = b;
                    if (selected is not null) for (var index = 0; index < rows.Length; index++) rows[index].IsSelected = selected[index];
                    ViewModel.UpdateSelection(); if (!originalOpen) CloseMainControl(); NavigateTo(originalTag); await Flush();
                    Require(ReferenceEquals(a!.Info, ready) && ReferenceEquals(b!.Info, other) && ReferenceEquals(ViewModel.MainDevice, a) && ReferenceEquals(ViewModel.CurrentDevice, b) && rows.SequenceEqual(ViewModel.Devices) && selected!.SequenceEqual(rows.Select(row => row.IsSelected)) && mainControlOpen == originalOpen, "已准备自有基线 Info/MainA/CurrentB/选择/页面恢复");
                    Require(mainGestureDispatchCount == dispatches && mainCaptureDispatchCount == captures && !ViewModel.Busy && !ViewModel.HasActiveOperations && !ViewModel.Runtime.Api.HasActiveRequests, "恢复无额外输入、业务或接口任务");
                }
                Require(tracked.All(item => item.Task.IsCompleted), "所有自有原 Task 已进入终态");
                foreach (var deadline in deadlines) deadline.Dispose();
                cleanup = true;
            }
            catch (Exception error) { errors.Add(new { failureClass = "CLEANUP-FAIL", error = error.ToString() }); failure = failure is null ? error : new AggregateException(failure, error); }
            try
            {
                reportPayload = JsonSerializer.Serialize(new { status = failure is null ? "PASS-OWN-DEMO-DIAGNOSTIC" : "FAIL", scope = "Prepared own MainA/CurrentB, row-only Offline transitions; catalog stays Ready; no pointer/physical input, original negative-case, Android, DPI or s01 field proof", observations, checks, errors, cleanup, sameGenerationHypothesis, incrementedGenerationHypothesis, hypothesisOutcome = sameGenerationHypothesis == true && incrementedGenerationHypothesis == true ? "SUPPORTED-IN-THIS-CONTROLLED-DEMO" : "NOT-SUPPORTED-OR-INCOMPLETE", actions = new { mainCalls, screenshotPublications, dumpReads, infoWrites, snapshots }, tasks = tracked.Select(item => new { item.Label, status = item.Task.Status.ToString(), item.Task.IsCompleted }).ToArray(), counterBefore, counterAfter, runtimeClose = "App closes and disposes own window/runtime after this report" }, options);
                using var parsed = JsonDocument.Parse(reportPayload); Require(parsed.RootElement.GetProperty("observations").GetArrayLength() == observations.Count, "最终标准 JSON 可重新解析且观察数量保持");
                File.WriteAllText(path, reportPayload);
            }
            catch (Exception error) { Console.Error.WriteLine("OFFLINE-GEOMETRY-REPORT-FAIL " + error); Console.Error.WriteLine("OFFLINE-GEOMETRY-PARTIAL " + JsonSerializer.Serialize(new { status = "FAIL", failureClass = "REPORT-FAIL", error = error.ToString(), partialPayload = reportPayload, observations, checks, errors, cleanup }, options)); failure = failure is null ? error : new AggregateException(failure, error); }
        }
        if (failure is not null) ExceptionDispatchInfo.Capture(failure).Throw();

        void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); checks.Add(message); }
        int Id(object? value) { if (value is null) return 0; if (!identities.TryGetValue(value, out var id)) { id = identities.Count + 1; identities.Add(value, id); } return id; }
        CancellationToken Deadline() { var deadline = CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token); deadline.CancelAfter(TimeSpan.FromSeconds(5)); deadlines.Add(deadline); return deadline.Token; }
        async Task Finish(string label, Task task) { tracked.Add((label, task)); await task.WaitAsync(TimeSpan.FromSeconds(5)); }
        async Task<T> Read<T>(string label, Task<T> task) { tracked.Add((label, task)); return await task.WaitAsync(TimeSpan.FromSeconds(5)); }
        async Task Publish() { screenshotPublications++; await Finish("screenshot-publication-" + screenshotPublications, ViewModel.RefreshAutomaticScreensAsync([a!.Serial], Deadline())); }
        async Task Flush()
        {
            await Finish("fixed-databind", Dispatcher.InvokeAsync(() => { }, DispatcherPriority.DataBind).Task);
            await Finish("fixed-render", Dispatcher.InvokeAsync(UpdateLayout, DispatcherPriority.Render).Task);
            await Finish("fixed-idle", Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle).Task);
        }
        async Task<object> Counters()
        {
            async Task<object> ReadCounter(string serial)
            {
                dumpReads++; var node = await Read("demo-counter-" + dumpReads, ViewModel.Runtime.Backend.DumpAsync(serial, Deadline()));
                return new { serial, node.PackageName, tap = node.ChildNodeList.Single(child => child.ResourceId == "com.matrix.demo:id/tap").Text, input = node.ChildNodeList.Single(child => child.ResourceId == "com.matrix.demo:id/input").Text };
            }
            return new { a = await ReadCounter(a!.Serial), b = await ReadCounter(b!.Serial) };
        }
        (bool SourceAbsent, bool Visible, bool PixelsZero, bool FormulaFinite) Snapshot(string phase)
        {
            var before = Signature(); var beforeCache = Cache(); var pixels = MainImagePixels(mainControlImage);
            var scaleX = mainControlImage.ActualWidth / pixels.Width; var scaleY = mainControlImage.ActualHeight / pixels.Height; var scale = Math.Min(scaleX, scaleY);
            var scaledWidth = pixels.Width * scale; var scaledHeight = pixels.Height * scale;
            var left = (mainControlImage.ActualWidth - scaledWidth) / 2; var top = (mainControlImage.ActualHeight - scaledHeight) / 2;
            var point = new Point(left + scaledWidth * .5, top + scaledHeight * .5); var finite = double.IsFinite(point.X) && double.IsFinite(point.Y);
            var center = new Point(mainControlImage.ActualWidth / 2, mainControlImage.ActualHeight / 2);
            Require(double.IsFinite(center.X) && double.IsFinite(center.Y), "仅向原 MapImage 传有限控件中心点"); var mapped = MapImage(mainControlImage, center);
            var binding = BindingOperations.GetBindingExpression(mainControlImage, Image.SourceProperty);
            var catalog = ViewModel.Runtime.Devices.Snapshot.Single(info => info.Serial == a!.Serial);
            var after = Signature(); var afterCache = Cache(); var cacheUnchanged = ReferenceEquals(beforeCache.Debug, afterCache.Debug) && ReferenceEquals(beforeCache.Nodes, afterCache.Nodes) && beforeCache.Entries.Length == afterCache.Entries.Length && beforeCache.Entries.Zip(afterCache.Entries).All(pair => ReferenceEquals(pair.First.Key, pair.Second.Key) && ReferenceEquals(pair.First.Value, pair.Second.Value));
            var unchanged = before == after && ReferenceEquals(before.AInfo, a!.Info) && ReferenceEquals(before.BInfo, b!.Info) && cacheUnchanged;
            observations.Add(new { phase, row = Info(a!.Info), catalog = Info(catalog), rowCatalogExactEqual = a.Info == catalog, rowCatalogIdentityGeometryEqual = a.Info.Serial == catalog.Serial && a.Info.Generation == catalog.Generation && a.Info.TransportId == catalog.TransportId && a.Info.Width == catalog.Width && a.Info.Height == catalog.Height && a.Info.Rotation == catalog.Rotation, image = new { sourceId = Id(mainControlImage.Source), rowScreenshotId = Id(a.Screenshot), sourceSameRow = ReferenceEquals(mainControlImage.Source, a.Screenshot), sourceAbsent = mainControlImage.Source is null, pixels = S(pixels), actual = S(new Size(mainControlImage.ActualWidth, mainControlImage.ActualHeight)), render = S(mainControlImage.RenderSize), mainControlImage.IsVisible, visibility = mainControlImage.Visibility.ToString(), bindingPath = binding?.ParentBinding.Path.Path }, formula = new { scaleX = N(scaleX), scaleY = N(scaleY), scale = N(scale), scaledWidth = N(scaledWidth), scaledHeight = N(scaledHeight), left = N(left), top = N(top), point = P(point), finite }, productionMap = new { center = P(center), mapped = mapped is { } value ? P(value) : null }, before = State(before), after = State(after), cacheBefore = CacheReport(beforeCache), cacheAfter = CacheReport(afterCache), cacheUnchanged, unchanged }); snapshots++;
            Require(unchanged, "同步只读快照未改变身份/图像/几何/上下文：" + phase);
            return (mainControlImage.Source is null, mainControlImage.IsVisible, pixels.Width == 0 && pixels.Height == 0, finite);
        }
        (DeviceInfo AInfo, DeviceInfo BInfo, object? Main, object? Current, object? AFrame, object? BFrame, object? Source, Size Pixels, Size Render, double Width, double Height, bool Visible, bool ASelected, bool BSelected, long InputGeneration) Signature()
            => (a!.Info, b!.Info, ViewModel.MainDevice, ViewModel.CurrentDevice, a.Screenshot, b.Screenshot, mainControlImage.Source, MainImagePixels(mainControlImage), mainControlImage.RenderSize, mainControlImage.ActualWidth, mainControlImage.ActualHeight, mainControlImage.IsVisible, a.IsSelected, b.IsSelected, ViewModel.Runtime.Input.CurrentGeneration);
        object State((DeviceInfo AInfo, DeviceInfo BInfo, object? Main, object? Current, object? AFrame, object? BFrame, object? Source, Size Pixels, Size Render, double Width, double Height, bool Visible, bool ASelected, bool BSelected, long InputGeneration) value)
            => new { aInfo = Info(value.AInfo), bInfo = Info(value.BInfo), mainId = Id(value.Main), currentId = Id(value.Current), aFrameId = Id(value.AFrame), bFrameId = Id(value.BFrame), sourceId = Id(value.Source), pixels = S(value.Pixels), render = S(value.Render), width = N(value.Width), height = N(value.Height), value.Visible, value.ASelected, value.BSelected, value.InputGeneration };
        object FiniteExamples()
        {
            var samples = new[] { 1.25, double.NaN, double.PositiveInfinity, double.NegativeInfinity }.Select(N).ToArray();
            var json = JsonSerializer.Serialize(new { samples, point = P(new Point(double.NaN, double.PositiveInfinity)), size = S(Size.Empty), rect = R(Rect.Empty), missing = (object?)null }, options);
            using var parsed = JsonDocument.Parse(json); var values = parsed.RootElement.GetProperty("samples").EnumerateArray().ToArray();
            Require(values.Length == 4 && values[0].GetProperty("value").GetDouble() == 1.25 && values[0].GetProperty("finite").GetBoolean() && values.Skip(1).All(value => value.GetProperty("value").ValueKind == JsonValueKind.Null && !value.GetProperty("finite").GetBoolean()) && values.Select(value => value.GetProperty("kind").GetString()).SequenceEqual(new[] { "Finite", "NaN", "PositiveInfinity", "NegativeInfinity" }) && parsed.RootElement.GetProperty("size").GetProperty("isEmpty").GetBoolean() && parsed.RootElement.GetProperty("rect").GetProperty("isEmpty").GetBoolean() && parsed.RootElement.GetProperty("missing").ValueKind == JsonValueKind.Null, "有限/NaN/正负无穷/Empty/null 标准 JSON 回读保留分类与 finite flag");
            return JsonSerializer.Deserialize<JsonElement>(json);
        }
        static object Info(DeviceInfo value) => new { value.Serial, value.State, value.Generation, value.TransportId, value.Width, value.Height, value.Rotation };
        (object? Debug, object? Nodes, DictionaryEntry[] Entries) Cache()
        {
            object? Field(string name) => name is "completedScreenshots" or "completedDebugImage" or "completedNodeTree" ? (typeof(WorkspaceViewModel).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new InvalidOperationException("缺少白名单缓存字段：" + name)).GetValue(ViewModel) : throw new InvalidOperationException("拒绝非白名单反射读取");
            var dictionary = Field("completedScreenshots") as IDictionary ?? throw new InvalidOperationException("完成缓存不是预期只读枚举字典");
            var entries = new List<DictionaryEntry>(); var enumerator = dictionary.GetEnumerator(); while (enumerator.MoveNext()) entries.Add(enumerator.Entry);
            return (Field("completedDebugImage"), Field("completedNodeTree"), entries.ToArray());
        }
        object CacheReport((object? Debug, object? Nodes, DictionaryEntry[] Entries) value) => new { debugId = Id(value.Debug), nodeId = Id(value.Nodes), count = value.Entries.Length, entries = value.Entries.Select(entry => new { rowId = Id(entry.Key), serial = (entry.Key as DeviceRow)?.Serial, completedEntryId = Id(entry.Value) }).ToArray(), reflection = "Only three named cache fields; read-only identity/enumeration, no setter or method invocation" };
        static OfflineNumber N(double value) => new(double.IsFinite(value) ? value : null, double.IsFinite(value), double.IsNaN(value) ? "NaN" : double.IsPositiveInfinity(value) ? "PositiveInfinity" : double.IsNegativeInfinity(value) ? "NegativeInfinity" : "Finite");
        static object P(Point value) => new { x = N(value.X), y = N(value.Y) };
        static object S(Size value) => new { value.IsEmpty, width = N(value.Width), height = N(value.Height) };
        static object R(Rect value) => new { value.IsEmpty, x = N(value.X), y = N(value.Y), width = N(value.Width), height = N(value.Height) };
    }
    private sealed record OfflineNumber(double? Value, bool Finite, string Kind);
}
