using System.Collections.Concurrent;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using MatrixDevice.Core;
using MatrixDevice.Features;

internal static class ScriptParameterTests
{
    public static void Register(Action<string, Func<Task>> test)
    {
        test("Script bindings wire roundtrip executes parameter Value through the original scheduler", async () =>
        {
            using var files = new PackageFiles();
            await using var backend = new RecordingBackend(); await using var scheduler = new ScriptScheduler(backend);
            var package = files.RoundTrip(Wire(value: "\"40,50\"", fallback: "\"60,70\""));
            Check(backend.Writes.IsEmpty, "Import/save/reload invoked an input operation.");
            await scheduler.RunAsync("DEMO-001", package);
            Check(backend.Writes.Single() == new Write("DEMO-001", "click", "40,50"), "PRODUCT-WRONG-EFFECT: parameter Value did not reach the original backend call.");
            Check(await backend.Taps("DEMO-001") == "Tap 1" && scheduler.GetStatus("DEMO-001").State == ScriptRunState.Completed, "Actual Demo execution did not complete.");
        });
        test("Script bindings missing Value uses DefaultValue after wire save and reload", async () =>
        {
            using var files = new PackageFiles(); await using var backend = new RecordingBackend(); await using var scheduler = new ScriptScheduler(backend);
            var package = files.RoundTrip(Wire(fallback: "\"60,70\""));
            await scheduler.RunAsync("DEMO-001", package);
            Check(backend.Writes.Single().Value == "60,70", "PRODUCT-WRONG-EFFECT: missing Value did not use DefaultValue.");
        });
        test("Script bindings explicit JSON null presence survives wire save and reload", () =>
        {
            using var files = new PackageFiles();
            var package = files.RoundTrip(Wire(value: "null", fallback: "\"60,70\""));
            Check(package.Parameters.Single().Value is { ValueKind: JsonValueKind.Null }, "PRODUCT-NULL-PRESENCE: explicit JSON null collapsed into a missing Value.");
            using var archive = ZipFile.OpenRead(files.SavedPath);
            using var stream = archive.GetEntry("script.json")!.Open(); using var document = JsonDocument.Parse(stream);
            Check(document.RootElement.GetProperty("parameters")[0].TryGetProperty("value", out var value) && value.ValueKind == JsonValueKind.Null, "Saved V1 ZIP omitted the explicit null Value.");
            return Task.CompletedTask;
        });
        test("Script bindings explicit package null never falls back or admits a run", async () =>
        {
            using var files = new PackageFiles();
            await RejectBeforeRun(files.RoundTrip(Wire(value: "null", fallback: "\"60,70\"")));
        });
        test("Script bindings absent sources and explicit null defaults reject before a run", async () =>
        {
            foreach (var wire in new[] { Wire(), Wire(fallback: "null") })
            {
                using var files = new PackageFiles(); await RejectBeforeRun(files.RoundTrip(wire));
            }
        });
        test("Script bindings selected undefined package values reject as 400 rather than serializer failures", async () =>
        {
            using var files = new PackageFiles(); var package = files.RoundTrip(Wire(value: "\"40,50\"", fallback: "\"60,70\""));
            var parameter = package.Parameters.Single();
            await RejectBeforeRun(package with { Parameters = [parameter with { Value = default(JsonElement) }] });
            await RejectBeforeRun(package with { Parameters = [parameter with { Value = null, DefaultValue = default(JsonElement) }] });
        });
        test("Script bindings present Value wins null Default and explicit override wins null Value", async () =>
        {
            using var files = new PackageFiles(); await using var backend = new RecordingBackend(); await using var scheduler = new ScriptScheduler(backend);
            var package = files.RoundTrip(Wire(value: "\"40,50\"", fallback: "null"));
            await scheduler.RunAsync("DEMO-001", package);
            Check(backend.Writes.Single().Value == "40,50", "Present Value was replaced by its null DefaultValue.");
            var explicitNull = files.RoundTrip(Wire(value: "null", fallback: "\"60,70\""));
            await RunWithOverrides(scheduler, "DEMO-002", explicitNull, new Dictionary<string, JsonElement> { ["point"] = Element("\"80,90\"") });
            Check(backend.Writes.Last().Value == "80,90" && await backend.Taps("DEMO-002") == "Tap 1", "Explicit override did not take precedence over null Value.");
        });
        test("Script bindings explicit overrides take precedence and reject null without fallback", async () =>
        {
            using var files = new PackageFiles(); await using var backend = new RecordingBackend(); await using var scheduler = new ScriptScheduler(backend);
            var package = files.RoundTrip(Wire(value: "\"40,50\"", fallback: "\"60,70\""));
            var overrides = new Dictionary<string, JsonElement> { ["point"] = Element("\"80,90\"") };
            await RunWithOverrides(scheduler, "DEMO-001", package, overrides);
            Check(backend.Writes.Single().Value == "80,90" && await backend.Taps("DEMO-001") == "Tap 1", "Override did not determine the actual Demo input.");
            overrides["point"] = Element("null");
            await RejectBeforeRun(package, overrides);
            overrides["point"] = default;
            await RejectBeforeRun(package, overrides);
        });
        test("Script bindings differentiated concurrent targets execute distinct values and leave third target unchanged", async () =>
        {
            using var files = new PackageFiles(); await using var backend = new RecordingBackend(); await using var scheduler = new ScriptScheduler(backend);
            var package = files.RoundTrip(Wire(value: "\"40,50\"", fallback: "\"60,70\""));
            Check(await backend.Taps("DEMO-003") == "Tap 0" && backend.Writes.IsEmpty, "Unselected third Demo did not start clean.");
            await Task.WhenAll(RunWithOverrides(scheduler, "DEMO-001", package, new Dictionary<string, JsonElement> { ["point"] = Element("\"20,30\"") }),
                RunWithOverrides(scheduler, "DEMO-002", package, new Dictionary<string, JsonElement> { ["point"] = Element("\"100,110\"") }));
            Check(backend.Writes.Count == 2 && backend.Writes.Contains(new Write("DEMO-001", "click", "20,30")) && backend.Writes.Contains(new Write("DEMO-002", "click", "100,110")), "Parallel target parameters were mixed, dropped or duplicated.");
            Check(await backend.Taps("DEMO-001") == "Tap 1" && await backend.Taps("DEMO-002") == "Tap 1" && await backend.Taps("DEMO-003") == "Tap 0", "Actual differentiated Demo state crossed target boundaries.");
            Check(!backend.Writes.Any(write => write.Serial == "DEMO-003"), "Unselected third target received a write.");
        });
        test("Script bindings compiler freezes ordinary literal steps and preserves all source inputs", async () =>
        {
            using var files = new PackageFiles();
            var package = files.RoundTrip(Wire(value: "\"40,50\"", fallback: "\"60,70\""));
            var overrides = new Dictionary<string, JsonElement> { ["point"] = Element("\"80,90\"") };
            var before = JsonSerializer.Serialize(package, JsonDefaults.Options); var overrideBefore = overrides["point"].GetRawText();
            var compiled = Compile(package, overrides);
            Check(compiled.Steps.Single().Value == "80,90", "Compiler did not produce the selected literal.");
            var compiledJson = JsonSerializer.SerializeToElement(compiled, JsonDefaults.Options);
            Check(!compiledJson.TryGetProperty("executionContract", out var contract) || contract.ValueKind == JsonValueKind.Null, "Compiled execution package can be rebound on scheduler entry.");
            Check(before == JsonSerializer.Serialize(package, JsonDefaults.Options) && overrides["point"].GetRawText() == overrideBefore, "Compilation changed package or overrides.");
            Check(!ReferenceEquals(compiled.Steps, package.Steps) && !ReferenceEquals(compiled.Parameters, package.Parameters) && !ReferenceEquals(compiled.Manifest, package.Manifest), "Execution snapshot shares mutable source collections or manifest.");
            await using var backend = new RecordingBackend(); await using var scheduler = new ScriptScheduler(backend);
            await scheduler.RunAsync("DEMO-001", compiled);
            Check(backend.Writes.Single().Value == "80,90", "Scheduler rebound a previously compiled override to package Value.");
        });
        test("Script bindings accepted run is isolated from later package and override mutation", async () =>
        {
            using var files = new PackageFiles(); await using var backend = new RecordingBackend { HoldFirstTap = true }; await using var scheduler = new ScriptScheduler(backend);
            var wire = Wire(value: "\"40,50\"");
            wire["steps"]!.AsArray().Add(JsonNode.Parse("{\"action\":\"click\",\"value\":\"10,20\"}"));
            wire["executionContract"]!["bindings"]!.AsArray().Add(Binding(1, "value", "point"));
            var package = files.RoundTrip(wire); var overrides = new Dictionary<string, JsonElement> { ["point"] = Element("\"80,90\"") };
            var running = RunWithOverrides(scheduler, "DEMO-001", package, overrides);
            try
            {
                await backend.FirstTapEntered.Task.WaitAsync(TimeSpan.FromSeconds(2));
                package.Steps.Clear(); package.Parameters.Clear(); overrides["point"] = Element("null");
            }
            finally { backend.ReleaseFirstTap.TrySetResult(); }
            await running.WaitAsync(TimeSpan.FromSeconds(2));
            Check(backend.Writes.Count == 2 && backend.Writes.All(write => write.Value == "80,90") && await backend.Taps("DEMO-001") == "Tap 2", "Accepted steps changed after source mutation.");
            Check(package.Steps.Count == 0 && package.Parameters.Count == 0 && overrides["point"].ValueKind == JsonValueKind.Null, "Executor restored or modified caller-owned inputs.");
        });
        test("Script bindings version and binding shape errors reject before admission", async () =>
        {
            var invalid = new List<JsonObject>();
            foreach (var version in new[] { 0, 2, -1 }) { var wire = Wire(value: "\"40,50\""); wire["executionContract"]!["version"] = version; invalid.Add(wire); }
            var noVersion = Wire(value: "\"40,50\""); noVersion["executionContract"]!.AsObject().Remove("version"); invalid.Add(noVersion);
            var noBindings = Wire(value: "\"40,50\""); noBindings["executionContract"]!.AsObject().Remove("bindings"); invalid.Add(noBindings);
            var nullBindings = Wire(value: "\"40,50\""); nullBindings["executionContract"]!["bindings"] = null; invalid.Add(nullBindings);
            foreach (var bindings in new[] { "[null]", "[{\"field\":\"value\",\"parameterKey\":\"point\"}]", "[{\"stepIndex\":-1,\"field\":\"value\",\"parameterKey\":\"point\"}]", "[{\"stepIndex\":1,\"field\":\"value\",\"parameterKey\":\"point\"}]", "[{\"stepIndex\":0,\"field\":\"Value\",\"parameterKey\":\"point\"}]", "[{\"stepIndex\":0,\"field\":\"value\",\"parameterKey\":\"Point\"}]", "[{\"stepIndex\":0,\"field\":\"value\",\"parameterKey\":\" \"}]" })
            { var wire = Wire(value: "\"40,50\""); wire["executionContract"]!["bindings"] = JsonNode.Parse(bindings); invalid.Add(wire); }
            var duplicate = Wire(value: "\"40,50\""); duplicate["executionContract"]!["bindings"]!.AsArray().Add(Binding(0, "value", "point")); invalid.Add(duplicate);
            foreach (var wire in invalid) await RejectWire(wire);
        });
        test("Script bindings fields must be consumed by the selected action", async () =>
        {
            var waitValue = Wire(value: "\"40,50\""); waitValue["steps"]![0] = JsonNode.Parse("{\"action\":\"wait\",\"durationMs\":0}");
            await RejectWire(waitValue);
            foreach (var action in new[] { "click", "swipe", "launch", "launchapp" })
            {
                var wire = DurationWire("0"); wire["steps"]![0] = new JsonObject { ["action"] = action, ["value"] = action is "launch" or "launchapp" ? "com.matrix.demo" : action == "click" ? "10,20" : "10,20,30,40,100" };
                await RejectWire(wire);
            }
        });
        test("Script bindings nonnull Probability rejects only the new execution contract", async () =>
        {
            foreach (var probability in new[] { 0, 0.5, 1 })
            {
                var wire = Wire(value: "\"40,50\""); wire["steps"]![0]!["probability"] = probability;
                await RejectWire(wire);
            }
        });
        test("Script bindings resolved action syntax is validated before a Running state", async () =>
        {
            foreach (var (action, literal, value) in new[] { ("click", "10,20", "not-coordinates"), ("launch", "com.matrix.demo", "invalid package"), ("launchapp", "com.matrix.demo", "1.bad"), ("swipe", "10,20,30,40,100", "10,20,30,40,0") })
            {
                var wire = Wire(value: JsonSerializer.Serialize(value)); wire["steps"]![0] = new JsonObject { ["action"] = action, ["value"] = literal };
                await RejectWire(wire);
            }
        });
        test("Script bindings full Value refuses JSON coercion and undefined override values", async () =>
        {
            foreach (var value in new[] { "42", "true", "[]", "{}", "\"\"" })
            {
                var wire = Wire(value: value, fallback: "\"60,70\""); wire["parameters"]![0]!.AsObject().Remove("type"); await RejectWire(wire);
            }
        });
        test("Script bindings duration accepts zero and Int32 maximum without changing literal package", async () =>
        {
            foreach (var value in new[] { "0", "2147483647" })
            {
                using var files = new PackageFiles(); var package = files.RoundTrip(DurationWire(value));
                var compiled = Compile(package, null);
                Check(compiled.Steps.Single().DurationMs == int.Parse(value, System.Globalization.CultureInfo.InvariantCulture) && package.Steps.Single().DurationMs == 0, "Int32 duration binding or source isolation failed.");
            }
            using var runnableFiles = new PackageFiles(); await using var backend = new RecordingBackend(); await using var scheduler = new ScriptScheduler(backend);
            await scheduler.RunAsync("DEMO-001", runnableFiles.RoundTrip(DurationWire("0")));
            Check(scheduler.GetStatus("DEMO-001").State == ScriptRunState.Completed && backend.Writes.IsEmpty, "Zero-duration wait did not complete without input.");
        });
        test("Script bindings duration rejects negative overflow fraction exponent and nonnumeric values", async () =>
        {
            foreach (var value in new[] { "-1", "2147483648", "1.5", "1.0", "1e0", "\"1\"", "true", "[]", "{}", "null" }) await RejectWire(DurationWire(value));
        });
        test("Script bindings nonzero duration reaches the original wait and cancellation prevents its next action", async () =>
        {
            using var files = new PackageFiles(); await using var backend = new RecordingBackend(); await using var scheduler = new ScriptScheduler(backend);
            var wire = DurationWire("10000"); wire["steps"]!.AsArray().Add(JsonNode.Parse("{\"action\":\"click\",\"value\":\"10,20\"}"));
            var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            scheduler.EntryAdded += (_, entry) => { if (entry.Message.StartsWith("步骤 1/2：", StringComparison.Ordinal)) entered.TrySetResult(); };
            var package = files.RoundTrip(wire); var running = scheduler.RunAsync("DEMO-001", package);
            try
            {
                await entered.Task.WaitAsync(TimeSpan.FromSeconds(2));
                await Task.Delay(50);
                Check(!running.IsCompleted && backend.Writes.IsEmpty, "PRODUCT-WRONG-EFFECT: bound nonzero duration did not wait before the next action.");
            }
            finally
            {
                scheduler.Stop("DEMO-001");
                await running.WaitAsync(TimeSpan.FromSeconds(2));
            }
            Check(scheduler.GetStatus("DEMO-001").State == ScriptRunState.Cancelled && backend.Writes.IsEmpty && await backend.Taps("DEMO-001") == "Tap 0", "Bound wait cancellation ran its next action or did not become terminal Cancelled.");
            Check(Compile(package, null).Steps[0].DurationMs == 10000, "Bound nonzero duration did not preserve its exact Int32 value.");
        });
        test("Script bindings overrides respect declared type numeric bounds and exact keys", async () =>
        {
            using var files = new PackageFiles(); var wire = DurationWire("0"); wire["parameters"]![0]!["type"] = "int"; wire["parameters"]![0]!["min"] = 0; wire["parameters"]![0]!["max"] = 5;
            var package = files.RoundTrip(wire);
            foreach (var overrides in new[]
            {
                new Dictionary<string, JsonElement> { ["delay"] = Element("6") },
                new Dictionary<string, JsonElement> { ["delay"] = Element("\"2\"") },
                new Dictionary<string, JsonElement> { ["Delay"] = Element("2") },
                new Dictionary<string, JsonElement> { ["unknown"] = Element("2") }
            }) await RejectBeforeRun(package, overrides);
        });
        test("Script bindings ordinary literal actions and parameterized launch and swipe use the existing backend", async () =>
        {
            using var files = new PackageFiles(); await using var backend = new RecordingBackend(); await using var scheduler = new ScriptScheduler(backend);
            var wire = Wire(value: "\"10,20,30,40,100\""); wire["steps"] = JsonNode.Parse("[{\"action\":\"launchapp\",\"value\":\"com.matrix.demo\"},{\"action\":\"swipe\",\"value\":\"1,2,3,4,50\"},{\"action\":\"click\",\"value\":\"15,25\"}]");
            wire["parameters"]!.AsArray().Add(new JsonObject { ["key"] = "app", ["type"] = "string", ["defaultValue"] = "com.android.settings" });
            wire["executionContract"]!["bindings"] = new JsonArray(Binding(0, "value", "app"), Binding(1, "value", "point"));
            await scheduler.RunAsync("DEMO-001", files.RoundTrip(wire));
            Check(backend.Writes.SequenceEqual(new[] { new Write("DEMO-001", "launch", "com.android.settings"), new Write("DEMO-001", "swipe", "10,20,30,40,100"), new Write("DEMO-001", "click", "15,25") }), "Parameterized launch/swipe or unbound literal call changed.");
            Check((await backend.DumpAsync("DEMO-001")).PackageName == "com.android.settings" && await backend.Taps("DEMO-001") == "Tap 2", "Original Demo engine did not observe launch and both input effects.");
        });
        test("Script bindings legacy packages retain Probability raw steps and nullable values after resource-bearing import", async () =>
        {
            using var files = new PackageFiles(); await using var backend = new RecordingBackend(); await using var scheduler = new ScriptScheduler(backend);
            var wire = Wire(value: "null", fallback: "\"60,70\""); wire.Remove("executionContract"); wire["steps"]![0]!["probability"] = 0;
            var package = files.RoundTrip(wire);
            Check(package.Parameters.Single().Value is null && package.Steps.Single().Probability == 0, "Legacy nullable/Probability interpretation changed.");
            await scheduler.RunAsync("DEMO-001", package);
            Check(backend.Writes.Single().Value == "10,20" && await backend.Taps("DEMO-001") == "Tap 1", "Legacy parameters or Probability acquired new execution meaning.");
            wire["executionContract"] = null;
            await scheduler.RunAsync("DEMO-002", files.RoundTrip(wire));
            Check(backend.Writes.Last().Value == "10,20" && await backend.Taps("DEMO-002") == "Tap 1", "Explicit null contract no longer uses the legacy path.");
            await RunWithOverrides(scheduler, "DEMO-001", package, new Dictionary<string, JsonElement>());
            Check(backend.Writes.Last().Value == "10,20" && await backend.Taps("DEMO-001") == "Tap 2", "Empty overrides changed legacy execution.");
        });
        test("Script bindings nonempty overrides on a legacy package reject rather than silently disappear", async () =>
        {
            using var files = new PackageFiles(); var wire = Wire(value: "\"40,50\""); wire.Remove("executionContract");
            await RejectBeforeRun(files.RoundTrip(wire), new Dictionary<string, JsonElement> { ["point"] = Element("\"80,90\"") });
        });
        test("Script bindings scoped null handling never changes global JsonDefaults behavior", () =>
        {
            var options = JsonDefaults.Options; var converters = options.Converters.ToArray(); var ignore = options.DefaultIgnoreCondition;
            var legacyBefore = JsonSerializer.Serialize(new ScriptParameter { Key = "probe", Value = null }, options);
            using var files = new PackageFiles(); files.RoundTrip(Wire(value: "null", fallback: "\"60,70\""));
            var legacyAfter = JsonSerializer.Deserialize<ScriptParameter>("{\"key\":\"probe\",\"value\":null}", options)!;
            Check(ReferenceEquals(options, JsonDefaults.Options) && options.DefaultIgnoreCondition == ignore && options.Converters.SequenceEqual(converters), "V1 parsing changed global serializer configuration.");
            Check(legacyAfter.Value is null && legacyBefore == JsonSerializer.Serialize(new ScriptParameter { Key = "probe", Value = null }, options), "V1 converter leaked to ordinary model serialization.");
            return Task.CompletedTask;
        });
    }

    private static JsonObject Wire(string? value = null, string? fallback = null)
    {
        var parameter = new JsonObject { ["key"] = "point", ["type"] = "string" };
        if (value is not null) parameter["value"] = JsonNode.Parse(value);
        if (fallback is not null) parameter["defaultValue"] = JsonNode.Parse(fallback);
        return new JsonObject
        {
            ["steps"] = JsonNode.Parse("[{\"action\":\"click\",\"value\":\"10,20\"}]"),
            ["parameters"] = new JsonArray(parameter),
            ["executionContract"] = new JsonObject { ["version"] = 1, ["bindings"] = new JsonArray(Binding(0, "value", "point")) }
        };
    }
    private static JsonObject DurationWire(string value)
    {
        var wire = Wire(); wire["steps"] = JsonNode.Parse("[{\"action\":\"wait\",\"durationMs\":0}]");
        wire["parameters"] = new JsonArray(new JsonObject { ["key"] = "delay", ["value"] = JsonNode.Parse(value) });
        wire["executionContract"]!["bindings"] = new JsonArray(Binding(0, "durationMs", "delay")); return wire;
    }
    private static JsonObject Binding(int index, string field, string key) => new() { ["stepIndex"] = index, ["field"] = field, ["parameterKey"] = key };
    private static JsonElement Element(string json) { using var document = JsonDocument.Parse(json); return document.RootElement.Clone(); }
    private static Task RunWithOverrides(ScriptScheduler scheduler, string serial, ScriptPackage package, IReadOnlyDictionary<string, JsonElement> overrides)
    {
        var method = typeof(ScriptScheduler).GetMethod("RunAsync", [typeof(string), typeof(ScriptPackage), typeof(CancellationToken), typeof(IReadOnlyDictionary<string, JsonElement>)]);
        if (method is null) throw new InvalidOperationException("CONTRACT-NOT-IMPLEMENTED: RunAsync(string, ScriptPackage, CancellationToken, IReadOnlyDictionary<string, JsonElement>).");
        return (Task)Invoke(method, scheduler, [serial, package, CancellationToken.None, overrides])!;
    }
    private static ScriptPackage Compile(ScriptPackage package, IReadOnlyDictionary<string, JsonElement>? overrides)
    {
        var type = typeof(ScriptPackage).Assembly.GetType("MatrixDevice.Features.ScriptExecutionCompiler");
        var method = type?.GetMethod("Compile", BindingFlags.Public | BindingFlags.Static, binder: null, types: [typeof(ScriptPackage), typeof(IReadOnlyDictionary<string, JsonElement>)], modifiers: null);
        if (method is null) throw new InvalidOperationException("CONTRACT-NOT-IMPLEMENTED: ScriptExecutionCompiler.Compile(ScriptPackage, IReadOnlyDictionary<string, JsonElement>).");
        return (ScriptPackage)Invoke(method, null, [package, overrides])!;
    }
    private static object? Invoke(MethodInfo method, object? target, object?[] arguments)
    {
        try { return method.Invoke(target, arguments); }
        catch (TargetInvocationException error) when (error.InnerException is not null) { ExceptionDispatchInfo.Capture(error.InnerException).Throw(); throw; }
    }
    private static async Task RejectWire(JsonObject wire)
    {
        using var files = new PackageFiles(); ScriptPackage package;
        try { package = files.RoundTrip(wire); }
        catch (DeviceException error) { Check(error.Status == 400 && error.Code == 1007, "Invalid wire was not rejected as 400/1007."); return; }
        await RejectBeforeRun(package);
    }
    private static async Task RejectBeforeRun(ScriptPackage package, IReadOnlyDictionary<string, JsonElement>? overrides = null)
    {
        await using var backend = new RecordingBackend(); await using var scheduler = new ScriptScheduler(backend); var statuses = new List<ScriptRunStatus>(); scheduler.StatusChanged += statuses.Add;
        DeviceException? failure = null;
        try { if (overrides is null) await scheduler.RunAsync("DEMO-001", package); else await RunWithOverrides(scheduler, "DEMO-001", package, overrides); }
        catch (DeviceException error) { failure = error; }
        Check(failure is { Status: 400, Code: 1007 }, "PRODUCT-INVALID-ADMITTED: invalid bound input was not rejected as 400/1007 before scheduling.");
        Check(backend.Writes.IsEmpty && statuses.Count == 0 && scheduler.GetStatus("DEMO-001").State == ScriptRunState.Idle && scheduler.GetLogs("DEMO-001").Count == 0, "Rejected bindings touched the backend or created a Running/log state.");
        Check(await backend.Taps("DEMO-001") == "Tap 0" && await backend.Taps("DEMO-002") == "Tap 0" && await backend.Taps("DEMO-003") == "Tap 0", "Rejected bindings changed a Demo target.");
    }
    private static void Check(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }

    private sealed class PackageFiles : IDisposable
    {
        private readonly string directory = Path.Combine(Path.GetTempPath(), "matrix-script-binding-tests-" + Guid.NewGuid().ToString("N"));
        public string SavedPath => Path.Combine(directory, "saved.jxscript");
        public ScriptPackage RoundTrip(JsonObject wire)
        {
            Directory.CreateDirectory(directory); var path = Path.Combine(directory, "source.jxscript");
            using (var archive = ZipFile.Open(path, ZipArchiveMode.Create))
            {
                Write("manifest.json", "{\"id\":\"binding-test\",\"name\":\"Binding test\",\"version\":\"author-42\"}");
                Write("script.json", wire.ToJsonString()); Write("resources/not-executable.txt", "Attached data is not code.");
                void Write(string name, string text) { using var writer = new StreamWriter(archive.CreateEntry(name).Open()); writer.Write(text); }
            }
            var imported = ScriptPackageService.Load(path);
            ScriptPackageService.Save(SavedPath, imported); var loaded = ScriptPackageService.Load(SavedPath);
            Check(loaded.Manifest.Version == "author-42", "Execution contract reused the manifest author version.");
            File.Delete(path); return loaded;
        }
        public void Dispose() { foreach (var file in new[] { Path.Combine(directory, "source.jxscript"), SavedPath }) if (File.Exists(file)) File.Delete(file); if (Directory.Exists(directory)) Directory.Delete(directory); }
    }
    private sealed record Write(string Serial, string Action, string Value);
    private sealed class RecordingBackend : IDeviceBackend
    {
        private readonly DemoDeviceBackend first = new();
        private readonly DemoDeviceBackend third = new();
        public ConcurrentQueue<Write> Writes { get; } = new();
        public bool HoldFirstTap { get; init; }
        public TaskCompletionSource FirstTapEntered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource ReleaseFirstTap { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private int taps;
        public bool IsSimulation => true;
        private (DemoDeviceBackend Backend, string Serial) Target(string serial) => serial switch { "DEMO-001" or "DEMO-002" => (first, serial), "DEMO-003" => (third, "DEMO-001"), _ => throw new InvalidOperationException("Unknown fixture target: " + serial) };
        public async Task<string> Taps(string serial) => (await DumpAsync(serial)).ChildNodeList[1].Text!;
        public async Task TapAsync(string serial, int x, int y, CancellationToken ct = default)
        {
            Writes.Enqueue(new(serial, "click", $"{x},{y}"));
            if (HoldFirstTap && Interlocked.Increment(ref taps) == 1) { FirstTapEntered.TrySetResult(); await ReleaseFirstTap.Task.WaitAsync(ct); }
            var target = Target(serial); await target.Backend.TapAsync(target.Serial, x, y, ct);
        }
        public Task SwipeAsync(string serial, int x1, int y1, int x2, int y2, int durationMs, CancellationToken ct = default)
        { Writes.Enqueue(new(serial, "swipe", $"{x1},{y1},{x2},{y2},{durationMs}")); var target = Target(serial); return target.Backend.SwipeAsync(target.Serial, x1, y1, x2, y2, durationMs, ct); }
        public Task<string> StartAppAsync(string serial, string packageName, string? activity = null, CancellationToken ct = default)
        { Writes.Enqueue(new(serial, "launch", packageName)); var target = Target(serial); return target.Backend.StartAppAsync(target.Serial, packageName, activity, ct); }
        public Task<AndroidNode> DumpAsync(string serial, CancellationToken ct = default) { var target = Target(serial); return target.Backend.DumpAsync(target.Serial, ct); }
        public async Task<IReadOnlyList<DeviceInfo>> DiscoverAsync(CancellationToken ct = default) => (await first.DiscoverAsync(ct)).Concat((await third.DiscoverAsync(ct)).Take(1).Select(info => info with { Serial = "DEMO-003" })).ToArray();
        public Task<string> ShellAsync(string serial, string command, bool useRoot = false, CancellationToken ct = default) => throw new InvalidOperationException("Scripts must not gain arbitrary shell execution.");
        public Task<byte[]> ScreenshotAsync(string serial, CancellationToken ct = default) => throw new NotSupportedException();
        public Task KeyEventAsync(string serial, int code, CancellationToken ct = default) => throw new NotSupportedException();
        public Task TextAsync(string serial, string text, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<string> StopAppAsync(string serial, string packageName, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<bool> ClearAppAsync(string serial, string packageName, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<IReadOnlyList<AppInfo>> ListAppsAsync(string serial, bool showSystem = false, bool needVersion = true, CancellationToken ct = default) => throw new NotSupportedException();
        public Task InstallAsync(string serial, string localPath, CancellationToken ct = default) => throw new NotSupportedException();
        public Task UninstallAsync(string serial, string packageName, CancellationToken ct = default) => throw new NotSupportedException();
        public Task PushAsync(string serial, string localPath, string remotePath, IProgress<double>? progress = null, CancellationToken ct = default) => throw new NotSupportedException();
        public Task PullAsync(string serial, string remotePath, string localPath, IProgress<double>? progress = null, CancellationToken ct = default) => throw new NotSupportedException();
        public Task SetNodeTextAsync(string serial, NodeSelector selector, string text, CancellationToken ct = default) => throw new NotSupportedException();
        public async ValueTask DisposeAsync() { ReleaseFirstTap.TrySetResult(); await first.DisposeAsync(); await third.DisposeAsync(); }
    }
}
