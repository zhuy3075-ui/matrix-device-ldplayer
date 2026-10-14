using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using MatrixDevice.Core;

namespace MatrixDevice.Host;

public sealed class SettingsStore(string path)
{
    private static readonly JsonSerializerOptions MergeJson = new(JsonDefaults.Options) { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never };
    private readonly string _path = Path.GetFullPath(path);
    private AppSettings? _baseline;
    public List<string> Warnings { get; } = [];

    public AppSettings Load()
    {
        Warnings.Clear();
        var settings = Read(_path);
        if (settings is null)
        {
            settings = Read(_path + ".bak");
            if (settings is not null)
            {
                settings = settings with { AdbAllowedSerials = [], LdSerialBindings = [], AgentSerial = null, AgentBaseUrl = null };
                Warnings.Add("已从备份恢复一般设置；设备授权与雷电绑定已清空，请重新确认。");
            }
        }
        settings ??= new AppSettings();
        var normalized = Normalize(settings);
        _baseline = Normalize(normalized);
        return normalized;
    }

    public AppSettings Save(AppSettings current)
    {
        ArgumentNullException.ThrowIfNull(current);
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        var name = "MatrixDevice.Settings." + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(_path.ToUpperInvariant())));
        using var gate = new Mutex(false, name);
        var acquired = false;
        try
        {
            try { acquired = gate.WaitOne(TimeSpan.FromSeconds(10)); }
            catch (AbandonedMutexException) { acquired = true; }
            if (!acquired) throw new IOException("Settings file is busy: " + _path);
            var disk = Read(_path);
            var merged = current;
            if (_baseline is not null && disk is not null)
            {
                var baselineJson = JsonSerializer.SerializeToNode(_baseline, MergeJson)!.AsObject();
                var localJson = JsonSerializer.SerializeToNode(current, MergeJson)!.AsObject();
                var diskJson = JsonSerializer.SerializeToNode(disk, MergeJson)!.AsObject();
                foreach (var property in localJson)
                {
                    if (property.Key == "ldSerialBindings" && property.Value is JsonObject localBindings && baselineJson[property.Key] is JsonObject originalBindings)
                    {
                        var bindings = diskJson[property.Key] as JsonObject;
                        if (bindings is null) { bindings = new JsonObject(); diskJson[property.Key] = bindings; }
                        foreach (var key in originalBindings.Select(x => x.Key).Union(localBindings.Select(x => x.Key)).ToArray())
                            if (!JsonNode.DeepEquals(localBindings[key], originalBindings[key]))
                            {
                                if (localBindings.TryGetPropertyValue(key, out var changed)) bindings[key] = changed?.DeepClone();
                                else bindings.Remove(key);
                            }
                    }
                    else if (!JsonNode.DeepEquals(property.Value, baselineJson[property.Key]))
                        diskJson[property.Key] = property.Value?.DeepClone();
                }
                merged = diskJson.Deserialize<AppSettings>(JsonDefaults.Options)!;
            }
            merged = Normalize(merged);
            var temp = _path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                using (var stream = new FileStream(temp, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    JsonSerializer.Serialize(stream, merged, JsonDefaults.Options);
                    stream.Flush(true);
                }
                if (File.Exists(_path)) File.Replace(temp, _path, _path + ".bak", true);
                else File.Move(temp, _path);
                _baseline = Normalize(merged);
                return merged;
            }
            finally { if (File.Exists(temp)) File.Delete(temp); }
        }
        finally { if (acquired) gate.ReleaseMutex(); }
    }

    private AppSettings? Read(string file)
    {
        if (!File.Exists(file)) return null;
        try
        {
            return Normalize(JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(file, Encoding.UTF8), JsonDefaults.Options)
                ?? throw new JsonException("Configuration is null"));
        }
        catch (Exception e) when (e is IOException or JsonException or ArgumentException or NotSupportedException)
        {
            Warnings.Add(file + ": " + e.Message);
            return null;
        }
    }

    public static AppSettings Normalize(AppSettings value) => value with
    {
        AdbAllowedSerials = NormalizeAuthorizedSerials(value.AdbAllowedSerials),
        BasicApiPort = value.BasicApiPort is >= 1 and <= 65535 ? value.BasicApiPort : 8080,
        AutomatorApiPort = value.AutomatorApiPort is >= 1 and <= 65535 ? value.AutomatorApiPort : 17171,
        WallColumns = Math.Clamp(value.WallColumns, 5, 20),
        MainMaxSize = Math.Clamp(value.MainMaxSize, 180, 4096),
        MiniMaxSize = Math.Clamp(value.MiniMaxSize, 180, 4096),
        MainMaxFps = Math.Clamp(value.MainMaxFps, 1, 120),
        MiniMaxFps = Math.Clamp(value.MiniMaxFps, 1, 120),
        WindowWidth = double.IsFinite(value.WindowWidth) ? Math.Clamp(value.WindowWidth, 1100, 7680) : 1440,
        WindowHeight = double.IsFinite(value.WindowHeight) ? Math.Clamp(value.WindowHeight, 720, 4320) : 900,
        CommandTimeoutSeconds = Math.Clamp(value.CommandTimeoutSeconds, 1, 300),
        LdSerialBindings = (value.LdSerialBindings ?? []).Where(x => x.Key >= 0 && !string.IsNullOrWhiteSpace(x.Value))
            .ToDictionary(x => x.Key, x => x.Value.Trim()),
        Language = "zh-CN"
    };
    private static string[] NormalizeAuthorizedSerials(string[]? serials)
    {
        if (serials is null) return [];
        if (serials.Any(serial => string.IsNullOrWhiteSpace(serial) || serial.StartsWith('-') || serial.Any(char.IsWhiteSpace) || serial.Contains('\0')))
            throw new JsonException("授权设备串号包含无效值，拒绝此配置");
        return serials.Distinct(StringComparer.Ordinal).ToArray();
    }
}
