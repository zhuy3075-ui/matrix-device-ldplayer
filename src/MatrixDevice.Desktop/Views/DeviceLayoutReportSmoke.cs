using System.Globalization;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Text.Json;
using MatrixDevice.Core;
using System.Windows;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    internal void VerifyDeviceLayoutReportContract()
    {
        var directory = Path.Combine(ViewModel.DataDirectory, "device-layout-report-contract");
        if (!ViewModel.Demo || !ViewModel.Runtime.IsDemo || ViewModel.Runtime.Backend is not DemoDeviceBackend
            || !Path.IsPathFullyQualified(directory) || !directory.Contains(Path.DirectorySeparatorChar + "artifacts" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
            || Directory.Exists(directory) || File.Exists(directory)) throw new InvalidOperationException("报告契约仅限 fresh artifacts Demo profile 内自有新子目录。");
        Directory.CreateDirectory(directory);
        var names = new[] { "finite-original-text", "anonymous-nested-nonfinite", "point-nonfinite", "size-nonfinite", "rect-nonfinite", "nullable-nonfinite", "empty-geometry", "prior-failure", "prior-and-cleanup-failure", "directory-write-failure", "unsupported-type", "destination-factory-failure", "payload-factory-failure", "public-options-and-context-preserved" };
        var reports = new List<object>(); var observations = new List<object>(); var expectedReportErrors = new List<object>(); var failures = new List<Exception>();
        var activeCase = "initial"; var originalError = Console.Error;
        var options = JsonDefaults.Options; var converters = options.Converters.ToArray();
        var configuration = (options.PropertyNamingPolicy, options.DictionaryKeyPolicy, options.PropertyNameCaseInsensitive, options.DefaultIgnoreCondition, options.WriteIndented, options.NumberHandling);
        var current = ViewModel.CurrentDevice; var main = ViewModel.MainDevice; var rows = ViewModel.Devices.ToArray(); var selected = rows.Select(row => row.IsSelected).ToArray();
        var original = Caught("original-device-assertion-marker"); var cleanup = Caught("cleanup-device-marker");
        var finite = new { status = "PASS-OFFLINE-WPF", phase = "finite-marker", observations = new List<object> { new { point = new Point(1.25, -2), size = new Size(3, 4), rect = new Rect(1, 2, 3, 4), nullable = (double?)5.5, missing = (double?)null, nested = new List<object> { new { value = -6.25, text = "中文及完整原值" } } } }, error = (string?)null };
        Exception? failure = null;
        try
        {
            Case(names[0], () =>
            {
                var expected = JsonSerializer.Serialize(finite, JsonDefaults.Options);
                Written("finite.json", finite, root => Check(root.GetProperty("phase").GetString() == "finite-marker" && root.GetProperty("observations")[0].GetProperty("nullable").GetDouble() == 5.5 && !root.GetProperty("observations")[0].TryGetProperty("missing", out _), "有限键值及原 null 忽略合同"));
                Check(File.ReadAllText(Path.Combine(directory, "finite.json")) == expected, "有限 payload 全文与原 JsonDefaults 完全相同");
            });
            Case(names[1], () => Written("nested.json", new { observations = new List<object> { new { value = double.NaN, nested = new List<object> { new { positive = double.PositiveInfinity, negative = double.NegativeInfinity, finite = 17.5 } } } }, values = new[] { double.NaN, double.PositiveInfinity, double.NegativeInfinity, 9.5 }, nullableValues = new double?[] { null, double.NaN, double.PositiveInfinity, double.NegativeInfinity, 11.5 } }, root =>
            {
                var item = root.GetProperty("observations")[0]; Nonfinite(item.GetProperty("value"), "NaN"); var nested = item.GetProperty("nested")[0]; Nonfinite(nested.GetProperty("positive"), "PositiveInfinity"); Nonfinite(nested.GetProperty("negative"), "NegativeInfinity"); Check(nested.GetProperty("finite").GetDouble() == 17.5, "嵌套有限值不改变");
                var values = root.GetProperty("values"); var nullableValues = root.GetProperty("nullableValues"); Check(values.GetArrayLength() == 4 && nullableValues.GetArrayLength() == 5 && nullableValues[0].ValueKind == JsonValueKind.Null && values[3].GetDouble() == 9.5 && nullableValues[4].GetDouble() == 11.5, "实际 double[]/Nullable[] 的 null、有限项及长度保留");
                foreach (var (index, kind) in new[] { (0, "NaN"), (1, "PositiveInfinity"), (2, "NegativeInfinity") }) { Nonfinite(values[index], kind); Nonfinite(nullableValues[index + 1], kind); }
            }));
            Case(names[2], () => Written("point.json", new { point = new Point(double.NaN, double.PositiveInfinity) }, root => { var value = root.GetProperty("point"); Nonfinite(value.GetProperty("x"), "NaN"); Nonfinite(value.GetProperty("y"), "PositiveInfinity"); }));
            Case(names[3], () => Written("size.json", new { size = new Size(double.PositiveInfinity, double.NaN) }, root => { var value = root.GetProperty("size"); Check(!value.GetProperty("isEmpty").GetBoolean(), "原 Size 非 Empty"); Nonfinite(value.GetProperty("width"), "PositiveInfinity"); Nonfinite(value.GetProperty("height"), "NaN"); }));
            Case(names[4], () => Written("rect.json", new { rect = new Rect(double.NaN, double.PositiveInfinity, double.PositiveInfinity, 1) }, root =>
            {
                var value = root.GetProperty("rect"); Nonfinite(value.GetProperty("x"), "NaN"); Nonfinite(value.GetProperty("y"), "PositiveInfinity"); Nonfinite(value.GetProperty("width"), "PositiveInfinity"); Nonfinite(value.GetProperty("right"), "NaN"); Check(value.GetProperty("height").GetDouble() == 1, "Rect 有限派生字段不改变");
            }));
            Case(names[5], () => Written("nullable.json", new { missing = (double?)null, finite = (double?)7.25, nan = (double?)double.NaN, positive = (double?)double.PositiveInfinity, negative = (double?)double.NegativeInfinity }, root =>
            {
                Check(!root.TryGetProperty("missing", out _) && root.GetProperty("finite").GetDouble() == 7.25, "Nullable 有限和 null 合同不变"); Nonfinite(root.GetProperty("nan"), "NaN"); Nonfinite(root.GetProperty("positive"), "PositiveInfinity"); Nonfinite(root.GetProperty("negative"), "NegativeInfinity");
            }));
            Case(names[6], () => Written("empty.json", new { size = Size.Empty, rect = Rect.Empty }, root =>
            {
                var size = root.GetProperty("size"); var rect = root.GetProperty("rect"); Check(size.GetProperty("isEmpty").GetBoolean() && rect.GetProperty("isEmpty").GetBoolean(), "实际 WPF Empty flag 保留"); Nonfinite(size.GetProperty("width"), "NegativeInfinity"); Nonfinite(size.GetProperty("height"), "NegativeInfinity"); Nonfinite(rect.GetProperty("x"), "PositiveInfinity"); Nonfinite(rect.GetProperty("y"), "PositiveInfinity"); Nonfinite(rect.GetProperty("width"), "NegativeInfinity"); Nonfinite(rect.GetProperty("height"), "NegativeInfinity");
            }));
            Case(names[7], () => PreservedFailure("prior.json", original));
            Case(names[8], () => PreservedFailure("prior-cleanup.json", new AggregateException("验收与恢复均失败", original, cleanup)));
            Case(names[9], () =>
            {
                foreach (var prior in new Exception?[] { null, original })
                {
                    var target = Path.Combine(directory, prior is null ? "existing-directory-0" : "existing-directory-1"); Directory.CreateDirectory(target);
                    var call = Call(() => target, () => finite, prior, true); ReportFailure(call, prior, error => error is IOException or UnauthorizedAccessException);
                    Check(call.Destinations == 1 && call.Payloads == 1 && Directory.Exists(target) && !Directory.EnumerateFileSystemEntries(target).Any(), "真实已有目录写失败、两 factory 各一次且未写文件");
                }
            });
            Case(names[10], () =>
            {
                foreach (var prior in new Exception?[] { null, original })
                {
                    var target = Path.Combine(directory, prior is null ? "unsupported-0.json" : "unsupported-1.json");
                    var call = Call(() => target, () => new { unsupported = typeof(MainWindow) }, prior, true); ReportFailure(call, prior, error => error is NotSupportedException);
                    Check(call.Destinations == 1 && call.Payloads == 1 && !File.Exists(target), "unsupported Type 在同一实际出口序列化失败、无文件");
                }
            });
            Case(names[11], () =>
            {
                foreach (var prior in new Exception?[] { null, original })
                {
                    var pathError = Caught("destination-factory-marker"); var call = Call(() => throw pathError, () => finite, prior, true);
                    ReportFailure(call, prior, error => ReferenceEquals(error, pathError)); Check(call.Destinations == 1 && call.Payloads == 0, "destination factory 失败只调用一次，payload factory 零次");
                }
            });
            Case(names[12], () =>
            {
                foreach (var prior in new Exception?[] { null, original })
                {
                    var payloadError = Caught("payload-factory-marker"); var target = Path.Combine(directory, prior is null ? "payload-0.json" : "payload-1.json");
                    var call = Call(() => target, () => throw payloadError, prior, true); ReportFailure(call, prior, error => ReferenceEquals(error, payloadError));
                    Check(call.Destinations == 1 && call.Payloads == 1 && !File.Exists(target), "payload factory 失败时 destination 已调用一次、payload 一次且无文件");
                }
            });
            Case(names[13], () =>
            {
                Check(ReferenceEquals(JsonDefaults.Options, options) && configuration == (options.PropertyNamingPolicy, options.DictionaryKeyPolicy, options.PropertyNameCaseInsensitive, options.DefaultIgnoreCondition, options.WriteIndented, options.NumberHandling)
                    && converters.Length == options.Converters.Count && converters.Zip(options.Converters).All(pair => ReferenceEquals(pair.First, pair.Second)), "全局公开配置与转换器集合/引用未改变，不声明内部缓存不变");
                Exception? strictFailure = null; try { _ = JsonSerializer.Serialize(double.NaN, JsonDefaults.Options); } catch (ArgumentException error) { strictFailure = error; }
                observations.Add(new { caseName = activeCase, globalStrictSerializationError = strictFailure?.ToString(), expectedType = typeof(ArgumentException).FullName }); Check(strictFailure is ArgumentException, "原全局 JsonDefaults 实际序列化 NaN 仍抛 ArgumentException");
                Check(ReferenceEquals(current, ViewModel.CurrentDevice) && ReferenceEquals(main, ViewModel.MainDevice) && rows.SequenceEqual(ViewModel.Devices) && selected.SequenceEqual(rows.Select(row => row.IsSelected)) && ReferenceEquals(Console.Error, originalError), "纯报告契约不改变 VM 上下文或原 stderr writer");
            });
            if (failures.Count != 0) failure = new AggregateException("设备布局报告契约未全部通过", failures);
        }
        catch (Exception error) { failure = error; }
        finally
        {
            Console.SetError(originalError);
            try
            {
                File.WriteAllText(Path.Combine(directory, "summary.json"), JsonSerializer.Serialize(new
                {
                    status = failure is null ? "PASS-DEVICE-LAYOUT-REPORT-CONTRACT" : "FAIL", plannedCaseCount = names.Length, caseCount = reports.Count, reports, observations, expectedReportErrors,
                    notRunCases = names.Skip(reports.Count).ToArray(), offlineGeometry = "NOT-RUN", offlineGeometryReason = failure is null ? "App calls the unchanged geometry diagnostic only after this synchronous fixture returns" : "Report contract failure blocks the following App geometry call",
                    error = failure?.ToString(), scope = "Pure actual report-outlet contract; own fresh profile files, real WPF value types, forwarded expected stderr; no UI input, capture, Focus/Activate, backend or original device-layout execution"
                }, JsonDefaults.Options));
            }
            catch (Exception summaryError) { Console.Error.WriteLine(summaryError); failure = failure is null ? summaryError : new AggregateException("报告契约与汇总保存均失败", failure, summaryError); }
        }
        if (failure is not null) ExceptionDispatchInfo.Capture(failure).Throw();

        void Case(string name, Action action)
        {
            activeCase = name;
            try { action(); reports.Add(new { name, status = "PASS", error = (string?)null }); }
            catch (Exception error) { failures.Add(error); reports.Add(new { name, status = "FAIL", error = error.ToString() }); }
        }
        void Check(bool condition, string message) { if (!condition) throw new InvalidOperationException(activeCase + ": " + message); }
        DeviceLayoutReportCall Call(Func<string> destination, Func<object> payload, Exception? prior = null, bool expectedError = false)
        {
            var destinations = 0; var payloads = 0; var previous = Console.Error; using var captured = new StringWriter(CultureInfo.InvariantCulture); Exception? returned;
            try { Console.SetError(new DeviceLayoutReportTee(previous, captured)); returned = SaveDeviceLayoutReport(() => { destinations++; return destination(); }, () => { payloads++; return payload(); }, prior); }
            finally { Console.SetError(previous); }
            var stderr = captured.ToString(); var classification = stderr.Length == 0 ? "NONE" : expectedError ? "EXPECTED-REPORT-ERROR" : "UNEXPECTED-REPORT-ERROR";
            var entry = new { caseName = activeCase, expectedReportError = expectedError, stderrClassification = classification, stderrForwarded = true, stderr, destinations, payloads, priorError = prior?.ToString(), returnedError = returned?.ToString() };
            observations.Add(entry); if (expectedError) expectedReportErrors.Add(entry);
            return new(returned, stderr, destinations, payloads);
        }
        void Written(string name, object payload, Action<JsonElement> verify, Exception? prior = null)
        {
            var target = Path.Combine(directory, name); var call = Call(() => target, () => payload, prior);
            Check(ReferenceEquals(call.Failure, prior) && call.Stderr.Length == 0 && call.Destinations == 1 && call.Payloads == 1, "实际报告出口成功且保留同一 prior 引用、factory 各一次");
            Check(File.Exists(target), "实际报告文件存在"); using var document = JsonDocument.Parse(File.ReadAllText(target)); FiniteNumbers(document.RootElement); verify(document.RootElement);
        }
        void PreservedFailure(string name, Exception prior)
        {
            void VerifyOriginal(JsonElement root)
            {
                Check(root.GetProperty("status").GetString() == "FAIL" && root.GetProperty("phase").GetString() == "offline-reject-marker" && root.GetProperty("error").GetString() == prior.ToString(), "原 FAIL/phase/error 全文保真，不把已有失败消解为 PASS");
                if (prior is AggregateException aggregate) Check(ReferenceEquals(aggregate.InnerExceptions[0], original) && ReferenceEquals(aggregate.InnerExceptions[1], cleanup), "原 assertion/cleanup 引用及顺序不变");
            }
            Written(name, new { status = "FAIL", phase = "offline-reject-marker", error = prior.ToString(), observations = new List<object> { new { finite = 1.25 } } }, VerifyOriginal, prior);
            Written("nonfinite-" + name, new { status = "FAIL", phase = "offline-reject-marker", error = prior.ToString(), observations = new List<object> { new { finite = 1.25, nonfinite = double.NaN } } }, root => { VerifyOriginal(root); Nonfinite(root.GetProperty("observations")[0].GetProperty("nonfinite"), "NaN"); }, prior);
        }
        void ReportFailure(DeviceLayoutReportCall call, Exception? prior, Func<Exception, bool> expected)
        {
            Exception? reportError = call.Failure;
            if (prior is not null)
            {
                Check(call.Failure is AggregateException aggregate && aggregate.InnerExceptions.Count == 2 && ReferenceEquals(aggregate.InnerExceptions[0], prior) && aggregate.Message.StartsWith("验收与报告保存均失败", StringComparison.Ordinal), "原 prior 在首位，报告异常在第二位，原聚合文案不变");
                reportError = ((AggregateException)call.Failure!).InnerExceptions[1];
            }
            Check(reportError is not null && expected(reportError) && call.Stderr.Contains(reportError.ToString(), StringComparison.Ordinal), "真实报告错误类型/对象及原 stderr 全文保留");
        }
        void Nonfinite(JsonElement value, string kind) => Check(value.ValueKind == JsonValueKind.Object && value.GetProperty("value").ValueKind == JsonValueKind.Null && !value.GetProperty("finite").GetBoolean() && value.GetProperty("kind").GetString() == kind, "非有限数明确 value:null/finite:false/kind=" + kind);
        void FiniteNumbers(JsonElement value)
        {
            if (value.ValueKind == JsonValueKind.Number) Check(double.IsFinite(value.GetDouble()), "标准 JSON 中实际 number 必须有限");
            else if (value.ValueKind == JsonValueKind.Array) foreach (var item in value.EnumerateArray()) FiniteNumbers(item);
            else if (value.ValueKind == JsonValueKind.Object) foreach (var item in value.EnumerateObject()) FiniteNumbers(item.Value);
        }
        static Exception Caught(string marker) { try { throw new InvalidOperationException(marker); } catch (Exception error) { return error; } }
    }
    private sealed record DeviceLayoutReportCall(Exception? Failure, string Stderr, int Destinations, int Payloads);
    private sealed class DeviceLayoutReportTee(TextWriter original, TextWriter captured) : TextWriter
    {
        public override Encoding Encoding => original.Encoding;
        public override void Write(char value) { original.Write(value); captured.Write(value); }
        public override void Write(string? value) { original.Write(value); captured.Write(value); }
        public override void WriteLine(string? value) { original.WriteLine(value); captured.WriteLine(value); }
    }
}
