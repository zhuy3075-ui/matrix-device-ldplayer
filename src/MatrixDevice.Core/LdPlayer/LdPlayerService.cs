using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace MatrixDevice.Core;

public sealed class LdPlayerService : IAsyncDisposable
{
    private sealed record History(long Generation, bool Present);
    private readonly ILdConsoleRunner _runner;
    private readonly object _gate = new();
    private readonly SemaphoreSlim _refresh = new(1, 1);
    private readonly SerialCommandQueue _commands = new();
    private readonly CancellationTokenSource _lifetime = new();
    private readonly Dictionary<int, History> _history = [];
    private IReadOnlyList<LdPlayerInstance> _instances = Array.Empty<LdPlayerInstance>();
    private static long _lastEpoch;
    private const long MaximumSafeGeneration = 9_007_199_254_740_991;
    private readonly long _generationEpoch = NewGenerationEpoch();
    private bool _disposed;
    public bool IsDemo { get; }
    public IReadOnlyList<LdPlayerInstance> Instances { get { lock (_gate) return _instances; } }
    public const string OfficialDocumentation = "https://help.ldmnq.com/docs/LDPlayer-ming-ling-hang-kai-fa-zhe-shi-yong-shuo-ming/n";

    public static IReadOnlyList<LdPlayerCapability> Capabilities { get; } = Array.AsReadOnly(new[]
    {
        new LdPlayerCapability("list2", true, false, "查询实例目录", ["兼容 7 列与 10 列；运行标志不能证明 ADB 或应用就绪。"]),
        new LdPlayerCapability("launch", true, false, "启动指定实例", ["重查 list2 的运行标志与进程字段；未测 Android 首帧。"]),
        new LdPlayerCapability("quit", true, true, "关闭指定实例", ["必须 confirmed:true；可能中断该实例的应用和任务。"]),
        new LdPlayerCapability("reboot", true, true, "重启指定实例", ["必须 confirmed:true；不以退出码证明重启已完成。"]),
        new LdPlayerCapability("runapp", true, false, "启动指定应用", ["需实例运行；仅提交命令，不验证应用业务页面。"]),
        new LdPlayerCapability("killapp", true, false, "停止指定应用", ["需实例运行；不验证目标应用进程已经结束。"]),
        new LdPlayerCapability("installapp", true, false, "安装指定 APK", ["仅接受存在的完整 Windows .apk 路径；未验证安装结果。"]),
        new LdPlayerCapability("input", true, false, "输入文本", ["使用 call.input；允许中文，但未注入测试；拒绝换行、NUL 和选项前缀。"]),
        new LdPlayerCapability("keyevent", true, false, "发送系统按键", ["仅 back、home、volumeup、volumedown；不验证界面效果。"]),
        new LdPlayerCapability("quitall", false, false, "全局退出", ["本适配器不开放全局操作。"]),
        new LdPlayerCapability("remove/restore/reset", false, false, "删除、恢复、重置", ["涉及实例数据，此处不开放。"]),
        new LdPlayerCapability("modify/setprop/locate/network", false, false, "身份、权限、网络或环境修改", ["此处不开放 root、设备身份和权限边界变更。"]),
        new LdPlayerCapability("adb", false, false, "共享 ADB 服务", ["不启动、连接或重启 ADB；索引不自动推导已验证串号。"]),
        new LdPlayerCapability("backupapp", false, false, "应用数据备份", ["官方当前文档标为暂不可用。"]),
        new LdPlayerCapability("restoreapp", false, false, "应用数据恢复", ["官方当前文档标为暂不可用。"]),
        new LdPlayerCapability("globalsetting", false, false, "全局设置", ["官方当前文档标为暂不可用；也不开放全局变更。"]),
        new LdPlayerCapability("keyevent.menu", false, false, "菜单按键", ["官方当前文档明确说明 menu 映射暂不可用。"]),
        new LdPlayerCapability("push/pull/operate/downcpu/scan/zoom", false, false, "暂不可用命令", ["官方当前文档标为暂不可用；CLI 帮助中出现不等于可用。"])
    });

    public LdPlayerService(ILdConsoleRunner runner, bool isDemo = false)
    { _runner = runner ?? throw new ArgumentNullException(nameof(runner)); IsDemo = isDemo; }

    public async Task<IReadOnlyList<LdPlayerInstance>> RefreshAsync(CancellationToken ct = default)
    {
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct, _lifetime.Token);
        await _refresh.WaitAsync(linked.Token).ConfigureAwait(false);
        try
        {
            lock (_gate) ObjectDisposedException.ThrowIf(_disposed, this);
            var parsed = ParseList2(await _runner.RunAsync(["list2"], linked.Token).ConfigureAwait(false));
            lock (_gate)
            {
                var next = new List<LdPlayerInstance>();
                var present = parsed.Select(instance => instance.Index).ToHashSet();
                var previous = _instances.ToDictionary(instance => instance.Index);
                foreach (var instance in parsed)
                {
                    var generation = _history.TryGetValue(instance.Index, out var history) ? (history.Present ? history.Generation : NextGeneration(history.Generation)) : _generationEpoch;
                    if (history?.Present == true && previous.TryGetValue(instance.Index, out var old) && (old with { Generation = 1 }) != (instance with { Generation = 1 })) generation = NextGeneration(generation);
                    _history[instance.Index] = new(generation, true);
                    next.Add(instance with { Generation = generation });
                }
                foreach (var pair in _history.Where(pair => pair.Value.Present && !present.Contains(pair.Key)).ToArray())
                    _history[pair.Key] = new(NextGeneration(pair.Value.Generation), false);
                _instances = next.AsReadOnly();
                return _instances;
            }
        }
        finally { _refresh.Release(); }
    }

    public async Task<LdPlayerCommandResult> ExecuteAsync(int index, string operation, JsonObject body, CancellationToken ct = default)
    {
        ValidateIndex(index);
        ArgumentNullException.ThrowIfNull(body);
        operation = operation?.Trim().ToLowerInvariant() ?? "";
        var frozen = body.DeepClone().AsObject();
        var expected = ExpectedGeneration(frozen);
        BuildArguments(index, operation, frozen);
        await RefreshAsync(ct).ConfigureAwait(false);
        RequireGeneration(index, expected);
        return await _commands.RunAsync<LdPlayerCommandResult>(index.ToString(CultureInfo.InvariantCulture), async token =>
        {
            await RefreshAsync(token).ConfigureAwait(false);
            var before = RequireGeneration(index, expected);
            if (operation is not ("launch" or "quit" or "reboot") && !before.AndroidStarted)
                throw new DeviceException($"实例 {index} 尚未运行，不能执行 {operation}。", 409, 1003);
            await _runner.RunAsync(BuildArguments(index, operation, frozen), token).ConfigureAwait(false);
            LdPlayerInstance? after;
            try { await RefreshAsync(token).ConfigureAwait(false); after = Instances.FirstOrDefault(instance => instance.Index == index); }
            catch (DeviceException error) { return new(index, operation, true, false, $"命令已返回，但无法复查实例状态：{error.Message}", null); }
            if (after is null)
                return new(index, operation, true, false, "命令已返回，但实例消失或代次变化，结果无法确认。", after);
            var identityChanged = after.Name != before.Name || after.Width != before.Width || after.Height != before.Height || after.Dpi != before.Dpi;
            if (after.Generation != before.Generation && (operation is not ("launch" or "quit") || identityChanged))
                return new(index, operation, true, false, "命令已返回，但实例观察指纹变化，结果无法确认。", after);
            var verified = operation switch
            {
                "launch" => after.AndroidStarted && after.ProcessId > 0,
                "quit" => !after.AndroidStarted && after.ProcessId <= 0,
                _ => false
            };
            var label = operation switch { "launch" => "启动", "quit" => "关闭", "reboot" => "重启", "runapp" => "应用启动", "killapp" => "应用停止", "installapp" => "APK 安装", "input" => "文本输入", "keyevent" => "系统按键", _ => operation };
            var message = verified ? $"{label}命令已返回，list2 已确认目标运行状态；未验证 ADB、首帧或业务页面。" : $"{label}命令已返回；CLI 退出码仅表示受理，实际设备效果尚未验证。";
            if (after.Generation != before.Generation) message += $" 实例代次已更新为 {after.Generation}。";
            if (IsDemo) message = "模拟模式：" + message;
            return new(index, operation, true, verified, message, after);
        }, ct).ConfigureAwait(false);
    }

    private LdPlayerInstance RequireGeneration(int index, long expected)
    {
        var instance = Instances.FirstOrDefault(item => item.Index == index) ?? throw new DeviceException($"雷电实例不存在：索引={index}", 404, 1002);
        if (instance.Generation != expected) throw new DeviceException($"雷电实例代次已变化：索引={index}，请求={expected}，当前={instance.Generation}", 409, 1003);
        return instance;
    }
    private static IReadOnlyList<string> BuildArguments(int index, string operation, JsonObject body)
    {
        var allowed = operation switch
        {
            "launch" => new[] { "expectedGeneration" }, "quit" or "reboot" => ["expectedGeneration", "confirmed"],
            "runapp" or "killapp" => ["expectedGeneration", "packageName"], "installapp" => ["expectedGeneration", "file"],
            "input" => ["expectedGeneration", "text"], "keyevent" => ["expectedGeneration", "key"],
            _ => throw new DeviceException($"雷电操作未开放：{operation}", 501, 2001)
        };
        foreach (var pair in body)
            if (!allowed.Contains(pair.Key, StringComparer.OrdinalIgnoreCase)) throw new DeviceException($"不允许的雷电参数：{pair.Key}", 400, 1007);
        List<string> args = [operation is "input" or "keyevent" ? "action" : operation, "--index", index.ToString(CultureInfo.InvariantCulture)];
        if (operation is "quit" or "reboot")
        {
            if (Property(body, "confirmed") is not JsonValue flag || !flag.TryGetValue<bool>(out var value) || !value)
                throw new DeviceException("关闭或重启前必须提供 confirmed:true。", 403, 1003);
        }
        if (operation is "runapp" or "killapp")
        {
            var package = String(body, "packageName");
            if (!Regex.IsMatch(package, @"^[A-Za-z][A-Za-z0-9_]*(\.[A-Za-z][A-Za-z0-9_]*)+$")) throw new DeviceException($"应用包名不合法：{package}", 400, 1007);
            args.AddRange(["--packagename", package]);
        }
        else if (operation == "installapp")
        {
            var file = String(body, "file");
            if (!Path.IsPathFullyQualified(file) || !file.EndsWith(".apk", StringComparison.OrdinalIgnoreCase) || !File.Exists(file))
                throw new DeviceException($"APK 必须为已存在的完整 Windows .apk 路径：{file}", 400, 1007);
            args.AddRange(["--filename", Path.GetFullPath(file)]);
        }
        else if (operation == "input")
        {
            var text = String(body, "text");
            if (text.Length > 4096 || text.IndexOfAny(['\0', '\r', '\n']) >= 0 || text.TrimStart().StartsWith("--", StringComparison.Ordinal))
                throw new DeviceException("输入文本不可超过 4096 字符，不得包含 NUL、换行或选项前缀。", 400, 1007);
            args.AddRange(["--key", "call.input", "--value", text]);
        }
        else if (operation == "keyevent")
        {
            var key = String(body, "key").ToLowerInvariant();
            if (key is not ("back" or "home" or "volumeup" or "volumedown")) throw new DeviceException($"系统按键未开放：{key}", 400, 1007);
            args.AddRange(["--key", "call.keyboard", "--value", key]);
        }
        return args;
    }
    private static long ExpectedGeneration(JsonObject body)
    {
        if (Property(body, "expectedGeneration") is JsonValue number)
        {
            if (number.TryGetValue<long>(out var value) && value > 0) return value;
            if (number.TryGetValue<int>(out var small) && small > 0) return small;
        }
        throw new DeviceException("expectedGeneration 必须为正整数。", 400, 1007);
    }
    private static JsonNode? Property(JsonObject body, string name)
    {
        var matches = body.Where(pair => pair.Key.Equals(name, StringComparison.OrdinalIgnoreCase)).ToArray();
        if (matches.Length > 1) throw new DeviceException($"参数重复：{name}", 400, 1007);
        return matches.Length == 0 ? null : matches[0].Value;
    }
    private static string String(JsonObject body, string name)
    {
        if (Property(body, name) is JsonValue value && value.TryGetValue<string>(out var text) && !string.IsNullOrWhiteSpace(text)) return text;
        throw new DeviceException($"参数 {name} 必须为非空字符串。", 400, 1007);
    }
    private static void ValidateIndex(int index)
    { if (index < 0 || index == 99999) throw new DeviceException($"雷电实例索引不合法：{index}", 400, 1007); }

    private static long NewGenerationEpoch()
    {
        Span<byte> bytes = stackalloc byte[sizeof(ulong)];
        RandomNumberGenerator.Fill(bytes);
        var candidate = checked((long)(BitConverter.ToUInt64(bytes) & ((1UL << 40) - 1)) * 1024);
        while (true)
        {
            var previous = Volatile.Read(ref _lastEpoch);
            var next = Math.Max(candidate, checked(previous + 65_536));
            if (next > MaximumSafeGeneration) throw new DeviceException("雷电实例代次空间已耗尽，请重新启动服务。", 503, 2000);
            if (Interlocked.CompareExchange(ref _lastEpoch, next, previous) == previous) return next;
        }
    }

    private static long NextGeneration(long generation)
    {
        if (generation >= MaximumSafeGeneration) throw new DeviceException("雷电实例代次空间已耗尽，请重新启动服务。", 503, 2000);
        return checked(generation + 1);
    }

    public static IReadOnlyList<LdPlayerInstance> ParseList2(string output)
    {
        ArgumentNullException.ThrowIfNull(output);
        var instances = new List<LdPlayerInstance>();
        var indices = new HashSet<int>();
        var lineNumber = 0;
        foreach (var raw in output.TrimStart('\ufeff').Split('\n'))
        {
            lineNumber++;
            var line = raw.TrimEnd('\r'); if (string.IsNullOrWhiteSpace(line)) continue;
            var fields = line.Split(',');
            if (fields.Length < 2 || !int.TryParse(fields[0].Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var index))
                throw new DeviceException($"list2 第 {lineNumber} 行索引不合法。", 400, 1007);
            if (index == 99999) continue;
            ValidateIndex(index);
            bool Candidate(int suffix, out LdPlayerInstance? instance)
            {
                instance = null;
                if (fields.Length < suffix + 2) return false;
                var start = fields.Length - suffix;
                var name = string.Join(',', fields.Skip(1).Take(start - 1));
                if (string.IsNullOrWhiteSpace(name)) return false;
                var numbers = new long[suffix];
                for (var position = 0; position < suffix; position++)
                    if (!long.TryParse(fields[start + position].Trim(), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out numbers[position])) return false;
                var knownStartupTransition = suffix == 8 && numbers[2] == 2;
                if (numbers[0] < 0 || numbers[1] < 0 || numbers[2] is not (0 or 1) && !knownStartupTransition || numbers[3] is < -1 or > int.MaxValue || numbers[4] is < -1 or > int.MaxValue) return false;
                if (suffix == 8 && numbers.Skip(5).Any(value => value is <= 0 or > int.MaxValue)) return false;
                instance = new(index, name, numbers[0], numbers[1], numbers[2] == 1, (int)numbers[3], (int)numbers[4], suffix == 8 ? (int)numbers[5] : null, suffix == 8 ? (int)numbers[6] : null, suffix == 8 ? (int)numbers[7] : null);
                return true;
            }
            var oldValid = Candidate(5, out var old);
            var modernValid = Candidate(8, out var modern);
            if (oldValid && modernValid) throw new DeviceException($"list2 第 {lineNumber} 行存在 7 列/10 列歧义；请确认名称中的数字及逗号。", 502, 2000);
            if (oldValid && !modernValid && fields.Length >= 10 && fields.TakeLast(8).All(field => long.TryParse(field.Trim(), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out _)))
                throw new DeviceException($"list2 第 {lineNumber} 行疑似损坏的 10 列格式，或名称中的数字及逗号产生歧义；拒绝将其解析为旧版窗口和进程字段。", 502, 2000);
            if (!oldValid && !modernValid) throw new DeviceException($"list2 第 {lineNumber} 行字段不合法，要求旧版 7 列或新版 10 列数值后缀。", 400, 1007);
            if (!indices.Add(index)) throw new DeviceException($"list2 包含重复索引：{index}", 502, 2000);
            instances.Add((modern ?? old)!);
        }
        return instances.AsReadOnly();
    }

    public async ValueTask DisposeAsync()
    {
        lock (_gate) { if (_disposed) return; _disposed = true; }
        _lifetime.Cancel();
        await _commands.DisposeAsync().ConfigureAwait(false);
        await _refresh.WaitAsync(CancellationToken.None).ConfigureAwait(false);
        _refresh.Release(); _refresh.Dispose(); _lifetime.Dispose();
        if (_runner is IAsyncDisposable disposable) await disposable.DisposeAsync().ConfigureAwait(false);
    }
}
