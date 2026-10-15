using System.Globalization;
using System.Text.Json;

namespace MatrixDevice.Host;

public sealed record LdMemoryConfiguration(int? MemoryMb, string? Warning = null);
public sealed record LdHostMemory(ulong AvailablePhysicalBytes, ulong AvailableCommitBytes, uint LoadPercent);
public sealed record LdLaunchMemoryAssessment(bool RequiresConfirmation, ulong? RecommendedPhysicalBytes, string Message);

public static class LdLaunchMemory
{
    private const ulong Mib = 1024 * 1024;
    private const int MaximumConfigurationBytes = 1024 * 1024;

    public static LdMemoryConfiguration ReadConfiguration(string consolePath, int index)
    {
        if (index is <= 0 or >= 99999) return new(null, "实例编号无效，内存配置未知。");
        try
        {
            if (string.IsNullOrWhiteSpace(consolePath) || !Path.IsPathFullyQualified(consolePath) || !File.Exists(consolePath))
                return new(null, "模拟器命令行路径无效，内存配置未知。");
            var directory = Path.GetDirectoryName(Path.GetFullPath(consolePath))!;
            var path = Path.Combine(directory, "vms", "config", "leidian" + index.ToString(CultureInfo.InvariantCulture) + ".config");
            using var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            if (file.Length > MaximumConfigurationBytes) return new(null, "实例配置文件过大，内存配置未知。");
            using var document = JsonDocument.Parse(file, new JsonDocumentOptions { MaxDepth = 32 });
            if (document.RootElement.ValueKind != JsonValueKind.Object) return new(null, "实例配置格式无效，内存配置未知。");
            JsonElement? ram = null;
            foreach (var property in document.RootElement.EnumerateObject())
            {
                if (!property.NameEquals("advancedSettings.memorySize")) continue;
                if (ram is not null) return new(null, "实例内存配置字段重复，内存配置未知。");
                ram = property.Value;
            }
            if (ram is not { ValueKind: JsonValueKind.Number } value || !value.TryGetInt32(out var memoryMb) || !ValidRam(memoryMb))
                return new(null, "实例内存字段缺失或无效，请在模拟器设置中确认 RAM。");
            return new(memoryMb);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException or JsonException or System.Security.SecurityException)
        {
            return new(null, "无法读取实例内存配置，请在模拟器设置中确认 RAM。");
        }
    }

    public static LdLaunchMemoryAssessment Assess(LdMemoryConfiguration configuration, LdHostMemory? host)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        var warnings = new List<string>();
        if (!string.IsNullOrWhiteSpace(configuration.Warning)) warnings.Add(configuration.Warning.TrimEnd('。'));
        ulong? recommended = null;
        ulong? commit = null;
        if (configuration.MemoryMb is { } memoryMb && ValidRam(memoryMb))
        {
            commit = checked(((ulong)memoryMb + 512) * Mib);
            recommended = checked(commit.Value + 1024 * Mib);
        }
        else warnings.Add("实例 RAM 未知，无法计算本次启动的建议预算");
        var hostKnown = host is not null && host.LoadPercent <= 100;
        if (!hostKnown) warnings.Add("主机内存状态未知");
        else
        {
            if (recommended is { } physical && host!.AvailablePhysicalBytes < physical) warnings.Add("可用物理内存低于本实例 RAM、512 MiB 进程开销及 1024 MiB 系统预留的建议预算");
            if (commit is { } requiredCommit && host!.AvailableCommitBytes < requiredCommit) warnings.Add("可用提交内存低于本实例 RAM 与 512 MiB 进程开销的建议预算");
            if (host!.LoadPercent >= 90) warnings.Add("主机内存负载已达到 90% 或更高");
        }
        var ram = recommended is not null ? configuration.MemoryMb!.Value.ToString(CultureInfo.InvariantCulture) + " MiB" : "未知";
        var load = hostKnown ? host!.LoadPercent.ToString(CultureInfo.InvariantCulture) + "%" : "未知";
        var summary = $"配置 RAM {ram}；可用物理 {GiB(hostKnown ? host!.AvailablePhysicalBytes : null)}；可用提交 {GiB(hostKnown ? host!.AvailableCommitBytes : null)}；负载 {load}；物理建议预算 {GiB(recommended)}";
        if (recommended is not null) summary += "（RAM + 512 MiB 进程开销 + 1024 MiB 系统预留）";
        return warnings.Count == 0
            ? new(false, recommended, summary + "。当前内存满足建议预算；该预算按采样时刻估算，非官方最低要求，也不代表已预留资源。")
            : new(true, recommended, summary + "。" + string.Join("；", warnings) + "。建议预算按采样时刻估算，非官方最低要求，也不代表已预留资源；确认风险后仍可继续。");
    }

    private static bool ValidRam(int memoryMb) => memoryMb is >= 256 and <= 65536;
    private static string GiB(ulong? bytes) => bytes is { } value ? (value / (double)(1024 * Mib)).ToString("0.0", CultureInfo.InvariantCulture) + " GiB" : "未知";
}
