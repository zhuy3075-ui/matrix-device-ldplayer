using System.Globalization;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;

namespace MatrixDevice.Host;

public sealed record LdMemoryConfiguration(int? MemoryMb, string? Warning = null, string? Source = null);
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
            var machinePath = Path.Combine(directory, "vms", "leidian" + index.ToString(CultureInfo.InvariantCulture), "leidian.vbox");
            if (ram is null) return ReadMachineConfiguration(machinePath);
            if (ram is not { ValueKind: JsonValueKind.Number } value || !value.TryGetInt32(out var memoryMb) || !ValidRam(memoryMb))
                return new(null, "实例内存字段无效，RAM 未知；请在模拟器设置中核对。");
            var machine = ReadMachineConfiguration(machinePath);
            if (machine.MemoryMb is { } machineMb && machineMb != memoryMb)
                return new(null, $"实例设置与虚拟机内存配置不一致（{memoryMb} / {machineMb} MiB），RAM 待核验。");
            return new(memoryMb, machine.MemoryMb is null && File.Exists(machinePath) ? "虚拟机配置未能核验，本次仅按实例设置中的 RAM 评估。" : null,
                machine.MemoryMb is not null ? "实例设置与虚拟机配置" : "实例设置");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException or JsonException or System.Security.SecurityException)
        {
            return new(null, "无法读取实例内存配置，请在模拟器设置中确认 RAM。");
        }
    }

    private static LdMemoryConfiguration ReadMachineConfiguration(string path)
    {
        const string warning = "未读到有效的实例 RAM 配置，请在模拟器设置中核对；未猜测默认内存。";
        try
        {
            using var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            if (file.Length > MaximumConfigurationBytes) return new(null, warning);
            var settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null, MaxCharactersInDocument = MaximumConfigurationBytes };
            using (var bounds = XmlReader.Create(file, settings))
                while (bounds.Read())
                    if (bounds.Depth > 32) return new(null, warning);
            file.Position = 0;
            using var reader = XmlReader.Create(file, settings);
            var document = XDocument.Load(reader);
            XNamespace ns = "http://www.virtualbox.org/";
            if (document.Root?.Name != ns + "VirtualBox") return new(null, warning);
            var machines = document.Root.Elements(ns + "Machine").ToArray();
            if (machines.Length != 1) return new(null, warning);
            var hardware = machines[0].Elements(ns + "Hardware").ToArray();
            if (hardware.Length != 1) return new(null, warning);
            var memories = hardware[0].Elements(ns + "Memory").ToArray();
            if (memories.Length != 1 || !int.TryParse(memories[0].Attribute("RAMSize")?.Value, NumberStyles.None, CultureInfo.InvariantCulture, out var memoryMb) || !ValidRam(memoryMb))
                return new(null, warning);
            return new(memoryMb, Source: "本实例虚拟机配置");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException or XmlException or System.Security.SecurityException)
        {
            return new(null, warning);
        }
    }

    public static LdLaunchMemoryAssessment Assess(LdMemoryConfiguration configuration, LdHostMemory? host)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        var warnings = new List<string>();
        var shortages = new List<string>();
        if (!string.IsNullOrWhiteSpace(configuration.Warning)) warnings.Add(configuration.Warning.TrimEnd('。'));
        ulong? recommended = null;
        ulong? ramBytes = null;
        if (configuration.MemoryMb is { } memoryMb && ValidRam(memoryMb))
        {
            ramBytes = checked((ulong)memoryMb * Mib);
            recommended = checked(ramBytes.Value + 1536 * Mib);
        }
        else if (warnings.Count == 0) warnings.Add("实例 RAM 未知，无法核验启动容量");
        var hostKnown = host is not null;
        var loadKnown = hostKnown && host!.LoadPercent <= 100;
        if (!hostKnown) warnings.Add("主机内存状态未知");
        else
        {
            if (ramBytes is { } required)
            {
                if (host!.AvailablePhysicalBytes < required) shortages.Add("可用物理内存低于本实例配置 RAM");
                else if (host.AvailablePhysicalBytes < recommended!.Value) warnings.Add("物理内存未达到估算预留建议，仅作提示");
                if (host!.AvailableCommitBytes < required) shortages.Add("可用提交额度低于本实例配置 RAM");
                else if (host.AvailableCommitBytes < required + 512 * Mib) warnings.Add("提交额度未达到估算开销建议，仅作提示");
            }
            if (!loadKnown) warnings.Add("主机内存负载未知，容量数值仍单独评估");
            else if (host!.LoadPercent >= 90) warnings.Add("主机内存负载较高，仅作提示");
        }
        var ram = recommended is not null ? configuration.MemoryMb!.Value.ToString(CultureInfo.InvariantCulture) + " MiB" : "未知";
        var load = loadKnown ? host!.LoadPercent.ToString(CultureInfo.InvariantCulture) + "%" : "未知";
        var summary = $"配置 RAM {ram}；可用物理 {GiB(hostKnown ? host!.AvailablePhysicalBytes : null)}；可用提交 {GiB(hostKnown ? host!.AvailableCommitBytes : null)}；负载 {load}；物理建议预算 {GiB(recommended)}";
        if (recommended is not null) summary += "（RAM + 512 MiB 进程开销 + 1024 MiB 系统预留）";
        if (ramBytes is not null && !string.IsNullOrWhiteSpace(configuration.Source)) summary += "；RAM 来源：" + configuration.Source;
        var requiresConfirmation = shortages.Count > 0;
        var details = shortages.Concat(warnings).ToArray();
        if (details.Length > 0) summary += "。" + string.Join("；", details);
        var conclusion = requiresConfirmation ? "检测到明确容量不足，确认后仍可继续。"
            : ramBytes is null || !hostKnown ? "本次无法核验容量是否充足，仅在页面提示，不要求确认。"
            : "可用物理与提交容量均未低于配置 RAM，不要求确认。";
        return new(requiresConfirmation, recommended, summary + "。" + conclusion + "检查为采样时刻快照；建议预算是估算，非官方最低要求，也不代表已预留资源或保证启动成功。");
    }

    private static bool ValidRam(int memoryMb) => memoryMb is >= 256 and <= 65536;
    private static string GiB(ulong? bytes) => bytes is { } value ? (value / (double)(1024 * Mib)).ToString("0.0", CultureInfo.InvariantCulture) + " GiB" : "未知";
}
