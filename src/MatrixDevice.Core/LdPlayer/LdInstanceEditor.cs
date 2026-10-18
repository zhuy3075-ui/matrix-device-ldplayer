using System.Globalization;

namespace MatrixDevice.Core;

public sealed record LdInstanceConfiguration(int Cpu, int MemoryMb, int Width, int Height, int Dpi);
public sealed record LdInstanceCreationResult(LdPlayerInstance Instance, bool CommandReportedFailure, string Message);

public sealed class LdInstanceEditor(ILdConsoleRunner runner, LdPlayerService service)
{
    public const int MaximumNameLength = 80;
    public const int MinimumScreenDimension = 240;
    public const int MaximumScreenDimension = 4096;
    public const int MinimumDpi = 72;
    public const int MaximumDpi = 640;
    private readonly ILdConsoleRunner runner = runner ?? throw new ArgumentNullException(nameof(runner));
    private readonly LdPlayerService service = service ?? throw new ArgumentNullException(nameof(service));
    private readonly SemaphoreSlim edits = new(1, 1);

    public async Task<LdInstanceCreationResult> CreateAsync(string name, CancellationToken ct = default)
    {
        var hasControlCharacters = name?.Any(char.IsControl) == true;
        name = name?.Trim() ?? "";
        if (name.Length is 0 or > MaximumNameLength || name.StartsWith("--", StringComparison.Ordinal) || hasControlCharacters || name.Contains(','))
            throw new DeviceException($"实例名称必须为 1 至 {MaximumNameLength} 字符，不能含控制字符、逗号或选项前缀。", 400, 1007);
        await edits.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            var before = await service.RefreshAsync(ct).ConfigureAwait(false);
            if (before.Any(instance => instance.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                throw new DeviceException($"实例名称已经存在：{name}；未创建或复制任何实例。", 409, 1003);
            DeviceException? commandFailure = null;
            try { await runner.RunAsync(["add", "--name", name], ct).ConfigureAwait(false); }
            catch (DeviceException error) when (error.Status == 503) { commandFailure = error; }
            ct.ThrowIfCancellationRequested();
            IReadOnlyList<LdPlayerInstance> after;
            try { after = await service.RefreshAsync(ct).ConfigureAwait(false); }
            catch (DeviceException error)
            {
                throw new DeviceException($"创建命令之后无法核对目录：{error.Message}；结果未知，未再次创建，请手动刷新。" +
                    (commandFailure is null ? "" : $" 原命令错误：{commandFailure.Message}"), error.Status, error.Code);
            }
            var oldIndices = before.Select(instance => instance.Index).ToHashSet();
            var added = after.Where(instance => !oldIndices.Contains(instance.Index)).ToArray();
            if (added.Length != 1 || added[0].Index < 0 || added[0].Index == 99999 || added[0].Name != name || before.Any(instance => !after.Any(current => current.Index == instance.Index)))
            {
                var observed = string.Join(", ", added.Select(instance => $"{instance.Index}:{instance.Name}"));
                throw new DeviceException($"无法唯一确认新建实例：名称={name}，新增目录=[{observed}]；未重试、复制或删除实例，请刷新目录检查。" +
                    (commandFailure is null ? "" : $" 原命令错误：{commandFailure.Message}"), 409, 1003);
            }
            var message = commandFailure is null
                ? "目录差集已确认唯一新实例；未自动启动、安装应用或核验 ADB。"
                : $"命令报告失败，但目录差集确认了唯一同名新实例；未重发 add，也未自动启动。原命令错误：{commandFailure.Message}";
            return new(added[0], commandFailure is not null, message);
        }
        finally { edits.Release(); }
    }

    public async Task<LdPlayerCommandResult> ConfigureAsync(int index, long expectedGeneration,
        LdInstanceConfiguration configuration, CancellationToken ct = default)
    {
        if (index < 0 || index == 99999) throw new DeviceException($"资源配置不允许实例编号 {index}；编号必须非负，不能为桌面编号99999。", 400, 1007);
        if (expectedGeneration <= 0) throw new DeviceException("expectedGeneration 必须为正整数。", 400, 1007);
        Validate(configuration);
        await edits.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            await service.RefreshAsync(ct).ConfigureAwait(false);
            var before = service.Instances.FirstOrDefault(instance => instance.Index == index)
                ?? throw new DeviceException($"雷电实例不存在：索引={index}", 404, 1002);
            if (before.Generation != expectedGeneration)
                throw new DeviceException($"实例代次已变化：索引={index}，请求={expectedGeneration}，当前={before.Generation}；未修改资源。", 409, 1003);
            if (before.IsStarting || before.AndroidStarted || before.ProcessId > 0 || before.VBoxProcessId > 0 || before.TopWindow != 0 || before.BindWindow != 0)
                throw new DeviceException($"实例 {index} 尚未完全停止；请确认播放器、虚拟机与窗口全部退出后配置。", 409, 1003);
            await runner.RunAsync(["modify", "--index", Number(index), "--cpu", Number(configuration.Cpu),
                "--memory", Number(configuration.MemoryMb), "--resolution",
                string.Join(',', Number(configuration.Width), Number(configuration.Height), Number(configuration.Dpi))], ct).ConfigureAwait(false);
            try { await service.RefreshAsync(ct).ConfigureAwait(false); }
            catch (DeviceException error)
            {
                return new(index, "modify", true, false, $"资源配置命令已返回，但目录复查失败：{error.Message}；结果待核验，不自动重试。", null);
            }
            var after = service.Instances.FirstOrDefault(instance => instance.Index == index);
            return new(index, "modify", true, false,
                "资源配置命令已返回；尚未核验实际 CPU、内存或 Android 分辨率，需要启动后另行验证。未请求修改型号、设备身份或 root。", after);
        }
        finally { edits.Release(); }
    }

    public static void Validate(LdInstanceConfiguration configuration)
    {
        if (configuration is null) throw new DeviceException("资源配置不能为空。", 400, 1007);
        if (configuration.Cpu is not (1 or 2 or 3 or 4)) throw new DeviceException($"CPU 核数必须为 1、2、3 或 4：{configuration.Cpu}", 400, 1007);
        if (configuration.MemoryMb is not (256 or 512 or 768 or 1024 or 1536 or 2048 or 4096 or 8192))
            throw new DeviceException($"内存值不在雷电官方枚举中：{configuration.MemoryMb} MiB", 400, 1007);
        if (configuration.Width is < MinimumScreenDimension or > MaximumScreenDimension || configuration.Height is < MinimumScreenDimension or > MaximumScreenDimension)
            throw new DeviceException($"宽高必须分别为 {MinimumScreenDimension} 至 {MaximumScreenDimension} 的正整数：{configuration.Width}x{configuration.Height}", 400, 1007);
        if (configuration.Dpi is < MinimumDpi or > MaximumDpi)
            throw new DeviceException($"DPI 必须为 {MinimumDpi} 至 {MaximumDpi} 的正整数：{configuration.Dpi}", 400, 1007);
    }

    private static string Number(int value) => value.ToString(CultureInfo.InvariantCulture);
}
