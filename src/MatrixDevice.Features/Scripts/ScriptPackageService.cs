using System.Globalization;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;
using MatrixDevice.Core;

namespace MatrixDevice.Features;

public sealed record ScriptManifest
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Version { get; init; } = "";
    public string Description { get; init; } = "";
    public string? Entry { get; init; }
    public string? MinimumAppVersion { get; init; }
    public string? AppName { get; init; }
    public string? Category { get; init; }
    public string? AppDownloadUrl { get; init; }
}

public sealed record ScriptStep
{
    public string Action { get; init; } = "";
    public int DurationMs { get; init; }
    public double? Probability { get; init; }
    public string? Value { get; init; }
}

public sealed record ScriptParameter
{
    public string Key { get; init; } = "";
    public string? Type { get; init; }
    public string? LabelZh { get; init; }
    public string? LabelEn { get; init; }
    public string? Unit { get; init; }
    public double? Min { get; init; }
    public double? Max { get; init; }
    public double? Step { get; init; }
    public JsonElement? DefaultValue { get; init; }
    public JsonElement? Value { get; init; }
}

public sealed record ScriptPackage
{
    public ScriptManifest Manifest { get; init; } = new();
    public List<ScriptStep> Steps { get; init; } = [];
    public List<ScriptParameter> Parameters { get; init; } = [];
    public ScriptExecutionContract? ExecutionContract { get; init; }
}

public sealed record ScriptExecutionContract
{
    public int Version { get; init; }
    public List<ScriptParameterBinding>? Bindings { get; init; }
}

public sealed record ScriptParameterBinding
{
    public int StepIndex { get; init; } = -1;
    public string Field { get; init; } = "";
    public string ParameterKey { get; init; } = "";
}

public static class ScriptPackageService
{
    public const int MaximumArchiveBytes = 16 * 1024 * 1024;
    public const int MaximumJsonBytes = 4 * 1024 * 1024;
    private static readonly HashSet<string> Actions = new(StringComparer.OrdinalIgnoreCase)
    { "launchapp", "launch", "wait", "sleep", "click", "swipe" };
    private static readonly JsonSerializerOptions ExecutionOptions = CreateExecutionOptions();

    public static ScriptPackage Load(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        using var file = File.OpenRead(path);
        if (file.Length > MaximumArchiveBytes) throw Invalid($"Script archive exceeds {MaximumArchiveBytes} bytes.");
        using var archive = OpenArchive(file);
        if (archive.Entries.Count > 128) throw Invalid("Script archive has more than 128 entries.");
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        long total = 0;
        foreach (var entry in archive.Entries)
        {
            ValidateEntryName(entry.FullName);
            if (!names.Add(entry.FullName)) throw Invalid($"Duplicate ZIP entry: {entry.FullName}");
            total += entry.Length;
            if (entry.Length > MaximumArchiveBytes || total > 32L * 1024 * 1024)
                throw Invalid($"Uncompressed ZIP limit exceeded at: {entry.FullName}");
        }
        var manifest = Read<ScriptManifest>(archive, "manifest.json");
        var package = Read<ScriptPackage>(archive, "script.json") with { Manifest = manifest };
        package = package with { Steps = package.Steps ?? [], Parameters = package.Parameters ?? [] };
        Validate(package);
        return package;
    }

    private static ZipArchive OpenArchive(Stream file)
    {
        try { return new ZipArchive(file, ZipArchiveMode.Read); }
        catch (InvalidDataException)
        {
            throw Invalid("脚本包格式无效：仅支持包含 manifest.json 和 script.json 的 ZIP 脚本包（.jxscript/.zip）；暂不支持顶层 JSON、Python 或 JavaScript。");
        }
    }

    public static void Save(string path, ScriptPackage package)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        Validate(package);
        var manifest = JsonSerializer.SerializeToUtf8Bytes(package.Manifest, JsonDefaults.Options);
        var script = JsonSerializer.SerializeToUtf8Bytes(package, OptionsFor(package));
        if (manifest.Length > MaximumJsonBytes || script.Length > MaximumJsonBytes)
            throw Invalid("Serialized script JSON exceeds the package limit.");
        var fullPath = Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        var temporary = fullPath + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            using (var file = File.Create(temporary))
            using (var archive = new ZipArchive(file, ZipArchiveMode.Create))
            {
                Write(archive, "manifest.json", manifest);
                Write(archive, "script.json", script);
            }
            File.Move(temporary, fullPath, true);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }

    public static void Validate(ScriptPackage package)
    {
        ArgumentNullException.ThrowIfNull(package);
        if (package.Manifest is null || string.IsNullOrWhiteSpace(package.Manifest.Id))
            throw Invalid("manifest.json must contain a nonblank Id.");
        if (package.Steps is null || package.Parameters is null) throw Invalid("Steps and Parameters must be arrays.");
        if (package.Steps.Count > 10000 || package.Parameters.Count > 1000) throw Invalid("Script item count limit exceeded.");
        for (var index = 0; index < package.Steps.Count; index++)
        {
            var step = package.Steps[index];
            if (step is null) throw Invalid($"Step {index} is null.");
            var action = step.Action?.Trim() ?? "";
            if (!Actions.Contains(action)) throw Invalid($"Unknown action at step {index}: '{step.Action}'.");
            if (step.DurationMs < 0) throw Invalid($"Step {index} DurationMs is negative: {step.DurationMs}.");
            if (step.Probability is { } probability && !double.IsFinite(probability))
                throw Invalid($"Step {index} Probability is not finite.");
            switch (action.ToLowerInvariant())
            {
                case "launch": case "launchapp":
                    ValidatePackageName(step.Value);
                    break;
                case "click":
                    ParseIntegers(step.Value, 2, index);
                    break;
                case "swipe":
                    var values = ParseIntegers(step.Value, 5, index);
                    if (values[4] <= 0) throw Invalid($"Step {index} swipe duration must be positive: {values[4]}.");
                    break;
            }
        }
        var keys = new HashSet<string>(StringComparer.Ordinal);
        foreach (var parameter in package.Parameters)
        {
            if (parameter is null || string.IsNullOrWhiteSpace(parameter.Key)) throw Invalid("A parameter has a blank Key.");
            if (!keys.Add(parameter.Key)) throw Invalid($"Duplicate parameter Key: {parameter.Key}.");
            if (parameter.Min is { } min && !double.IsFinite(min) || parameter.Max is { } max && !double.IsFinite(max))
                throw Invalid($"Parameter {parameter.Key} bounds must be finite.");
            if (parameter.Min > parameter.Max) throw Invalid($"Parameter {parameter.Key} Min exceeds Max.");
            if (parameter.Step is { } interval && (!double.IsFinite(interval) || interval <= 0))
                throw Invalid($"Parameter {parameter.Key} Step must be positive: {interval}.");
            ValidateValue(parameter, parameter.DefaultValue, "DefaultValue");
            ValidateValue(parameter, parameter.Value, "Value");
        }
        ValidateExecutionContract(package);
    }

    private static void ValidateExecutionContract(ScriptPackage package)
    {
        if (package.ExecutionContract is not { } contract) return;
        if (contract.Version != 1) throw Invalid($"不支持脚本执行契约版本：{contract.Version}，仅支持 version=1。");
        if (contract.Bindings is null) throw Invalid("执行契约 bindings 必须为数组。");
        if (contract.Bindings.Count > 10000) throw Invalid($"执行契约绑定数量超限：{contract.Bindings.Count}，最多 10000。");
        for (var index = 0; index < package.Steps.Count; index++)
            if (package.Steps[index].Probability is not null) throw Invalid($"步骤 {index} 的 Probability 尚无受支持的执行语义；version=1 不接受非空概率。");
        foreach (var parameter in package.Parameters)
        {
            if (parameter.Value is { ValueKind: JsonValueKind.Undefined }) throw Invalid($"参数 {parameter.Key} 的 Value 为 undefined，不能用于 version=1。");
            if (parameter.DefaultValue is { ValueKind: JsonValueKind.Undefined }) throw Invalid($"参数 {parameter.Key} 的 DefaultValue 为 undefined，不能用于 version=1。");
        }
        var keys = package.Parameters.Select(parameter => parameter.Key).ToHashSet(StringComparer.Ordinal);
        var fields = new HashSet<(int, string)>();
        for (var index = 0; index < contract.Bindings.Count; index++)
        {
            var binding = contract.Bindings[index];
            if (binding is null) throw Invalid($"绑定 {index} 不能为 null。");
            if (binding.StepIndex < 0 || binding.StepIndex >= package.Steps.Count)
                throw Invalid($"绑定 {index} 的 stepIndex={binding.StepIndex} 越界，步骤数量为 {package.Steps.Count}。");
            if (string.IsNullOrWhiteSpace(binding.ParameterKey) || !keys.Contains(binding.ParameterKey))
                throw Invalid($"绑定 {index} 的 parameterKey='{binding.ParameterKey}' 为空或未声明（区分大小写）。");
            if (binding.Field is not ("value" or "durationMs")) throw Invalid($"绑定 {index} 的 field='{binding.Field}' 无效，仅支持 value 或 durationMs。");
            if (!fields.Add((binding.StepIndex, binding.Field))) throw Invalid($"步骤 {binding.StepIndex} 的字段 {binding.Field} 重复绑定。");
            var action = package.Steps[binding.StepIndex].Action.Trim().ToLowerInvariant();
            if (binding.Field == "value" && action is not ("launch" or "launchapp" or "click" or "swipe") ||
                binding.Field == "durationMs" && action is not ("wait" or "sleep"))
                throw Invalid($"步骤 {binding.StepIndex} 的动作 {action} 不使用字段 {binding.Field}，不能绑定参数 {binding.ParameterKey}。");
        }
    }

    internal static ScriptPackage CloneForExecution(ScriptPackage package)
    {
        var options = OptionsFor(package);
        return JsonSerializer.Deserialize<ScriptPackage>(JsonSerializer.SerializeToUtf8Bytes(package, options), options)
            ?? throw Invalid("脚本执行快照不能为空。");
    }

    private static JsonSerializerOptions OptionsFor(ScriptPackage package) => package.ExecutionContract is { Version: 1 } ? ExecutionOptions : JsonDefaults.Options;

    private static JsonSerializerOptions CreateExecutionOptions()
    {
        var options = new JsonSerializerOptions(JsonDefaults.Options);
        options.Converters.Add(new ExplicitNullElementConverter());
        return options;
    }

    private sealed class ExplicitNullElementConverter : JsonConverter<JsonElement?>
    {
        public override bool HandleNull => true;
        public override JsonElement? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var document = JsonDocument.ParseValue(ref reader);
            return document.RootElement.Clone();
        }
        public override void Write(Utf8JsonWriter writer, JsonElement? value, JsonSerializerOptions options)
        {
            if (value is null) writer.WriteNullValue();
            else value.Value.WriteTo(writer);
        }
    }

    public static int[] ParseIntegers(string? value, int count, int stepIndex = -1)
    {
        var parts = (value ?? "").Split(',');
        if (parts.Length != count) throw Invalid($"Step {stepIndex} requires {count} comma-separated integers: '{value}'.");
        var result = new int[count];
        for (var index = 0; index < count; index++)
            if (!int.TryParse(parts[index].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out result[index]))
                throw Invalid($"Step {stepIndex} has an invalid integer: '{parts[index]}'.");
        return result;
    }

    public static void ValidatePackageName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 255 ||
            value.Split('.').Any(part => part.Length == 0 || !char.IsAsciiLetter(part[0]) ||
                part.Any(character => !char.IsAsciiLetterOrDigit(character) && character != '_')))
            throw Invalid($"Invalid Android package name: '{value}'.");
    }

    internal static void ValidateValue(ScriptParameter parameter, JsonElement? value, string field)
    {
        if (value is not { } element || element.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined) return;
        var type = parameter.Type?.Trim().ToLowerInvariant();
        bool compatible = type switch
        {
            "int" or "integer" => element.ValueKind == JsonValueKind.Number && element.TryGetInt64(out _),
            "number" or "double" or "float" => element.ValueKind == JsonValueKind.Number,
            "bool" or "boolean" => element.ValueKind is JsonValueKind.True or JsonValueKind.False,
            "string" or "text" => element.ValueKind == JsonValueKind.String,
            _ => true
        };
        if (!compatible) throw Invalid($"Parameter {parameter.Key} {field} does not match declared Type '{parameter.Type}'.");
        if (element.ValueKind == JsonValueKind.Number)
        {
            if (!element.TryGetDouble(out var number) || !double.IsFinite(number)) throw Invalid($"Parameter {parameter.Key} {field} is not finite.");
            if (parameter.Min is { } min && number < min || parameter.Max is { } max && number > max)
                throw Invalid($"Parameter {parameter.Key} {field} is outside its declared bounds: {number}.");
        }
    }

    private static void ValidateEntryName(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Contains('\\') || name.Contains(':') || name.StartsWith('/') ||
            name.Split('/').Any(part => part is ".." or "."))
            throw Invalid($"Unsafe ZIP entry name: '{name}'.");
    }

    private static T Read<T>(ZipArchive archive, string name)
    {
        var entry = archive.GetEntry(name) ?? throw Invalid($"Required ZIP entry is missing: {name}.");
        if (entry.Length > MaximumJsonBytes) throw Invalid($"ZIP JSON entry is too large: {name}.");
        using var stream = entry.Open();
        using var bytes = new MemoryStream();
        var buffer = new byte[8192];
        while (true)
        {
            var read = stream.Read(buffer);
            if (read == 0) break;
            if (bytes.Length + read > MaximumJsonBytes) throw Invalid($"ZIP JSON entry exceeds its read limit: {name}.");
            bytes.Write(buffer, 0, read);
        }
        try
        {
            var payload = bytes.ToArray();
            var value = JsonSerializer.Deserialize<T>(payload, JsonDefaults.Options) ?? throw Invalid($"ZIP JSON is null: {name}.");
            if (value is ScriptPackage { ExecutionContract.Version: 1 })
                value = JsonSerializer.Deserialize<T>(payload, ExecutionOptions) ?? throw Invalid($"ZIP JSON is null: {name}.");
            return value;
        }
        catch (JsonException error) { throw Invalid($"Invalid JSON in {name}: {error.Message}"); }
    }

    private static void Write(ZipArchive archive, string name, byte[] bytes)
    {
        using var stream = archive.CreateEntry(name, CompressionLevel.Optimal).Open();
        stream.Write(bytes);
    }

    private static DeviceException Invalid(string message) => new(message, 400, 1007);
}
