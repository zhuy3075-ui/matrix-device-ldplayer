using System.Text;
using System.Globalization;
using MatrixDevice.Host;

internal static class LdLaunchMemoryTests
{
    private const ulong Mib = 1024 * 1024;
    private const ulong Gib = 1024 * Mib;

    public static void Register(Action<string, Func<Task>> test)
    {
        test("LD memory reads only the confirmed literal RAM key and leaves the configuration intact", ReadConfigurationAsync);
        test("LD memory reads the same instance current machine RAM without guessing missing settings", MachineConfigurationAsync);
        test("LD memory rejects unsafe ambiguous and unsupported machine XML without leaking configuration", UnknownMachineConfigurationAsync);
        test("LD memory conflicting sources and invalid explicit RAM remain unknown", SourceConflictsAsync);
        test("LD memory missing malformed ambiguous and unreasonable RAM remain unknown without leaking JSON", UnknownConfigurationAsync);
        test("LD memory invalid paths indices missing files and oversized files return warnings", ConfigurationBoundsAsync);
        test("LD memory 7.8 GiB at 75 percent accepts 2 and 6 GiB but warns for 8 GiB", TypicalCapacityAsync);
        test("LD memory only measured capacity below configured RAM requires confirmation", RiskThresholdsAsync);
        test("LD memory unknown sampling and diagnostic warnings remain nonblocking without guessing zero", UnknownAssessmentAsync);
        test("LD memory messages show sampled RAM capacity load and estimates with invariant numeric formatting", AssessmentMessageAsync);
    }

    private static void Assert(bool condition, string message) { if (!condition) throw new Exception(message); }
    private static Task ReadConfigurationAsync()
    {
        using var fixture = new MemoryFixture();
        const string json = "{\"advancedSettings.memorySize\":6144,\"privateUnrelated\":\"PRIVATE_CONFIG_MUST_NOT_APPEAR\"}";
        fixture.Write(json);
        var configuration = LdLaunchMemory.ReadConfiguration(fixture.ConsolePath, 2);
        Assert(configuration.MemoryMb == 6144 && configuration.Warning is null, "Confirmed literal integer RAM was not read.");
        Assert(File.ReadAllText(fixture.ConfigurationPath) == json && new FileInfo(fixture.ConsolePath).Length == 0, "ReadConfiguration mutated a fixture or used the console executable.");
        foreach (var ram in new[] { 256, 2048, 8192, 65536 })
        {
            fixture.Write("{\"advancedSettings.memorySize\":" + ram + "}");
            Assert(LdLaunchMemory.ReadConfiguration(fixture.ConsolePath, 2).MemoryMb == ram, "Valid custom RAM was rejected: " + ram);
        }
        return Task.CompletedTask;
    }

    private static Task UnknownConfigurationAsync()
    {
        using var fixture = new MemoryFixture();
        foreach (var json in new[]
        {
            "{}", "[]", "{\"advancedSettings\":{\"memorySize\":2048}}", "{\"performanceSettings\":{\"memorySize\":2048}}",
            "{\"advancedSettings.memorySize\":\"2048\"}", "{\"advancedSettings.memorySize\":2048.5}", "{\"advancedSettings.memorySize\":null}",
            "{\"advancedSettings.memorySize\":0}", "{\"advancedSettings.memorySize\":-1}", "{\"advancedSettings.memorySize\":65537}",
            "{\"advancedSettings.memorySize\":2048,\"advancedSettings.memorySize\":8192}", "{\"PRIVATE_CONFIG_MUST_NOT_APPEAR\":broken}"
        })
        {
            fixture.Write(json);
            var configuration = LdLaunchMemory.ReadConfiguration(fixture.ConsolePath, 2);
            Assert(configuration.MemoryMb is null && !string.IsNullOrWhiteSpace(configuration.Warning), "Unknown RAM was guessed instead of warned.");
            Assert(!configuration.Warning!.Contains("PRIVATE_CONFIG_MUST_NOT_APPEAR", StringComparison.Ordinal) && !configuration.Warning.Contains(json, StringComparison.Ordinal), "Configuration contents leaked into its warning.");
        }
        return Task.CompletedTask;
    }

    private static string MachineXml(string memory, string extra = "") => "<VirtualBox xmlns=\"http://www.virtualbox.org/\"><Machine><Hardware><Memory RAMSize=\"" + memory + "\"/></Hardware>" + extra + "</Machine></VirtualBox>";

    private static Task MachineConfigurationAsync()
    {
        using var fixture = new MemoryFixture();
        fixture.Write("{}");
        var xml = MachineXml("6144", "<Snapshot><Hardware><Memory RAMSize=\"1024\"/></Hardware></Snapshot>");
        fixture.WriteMachine(xml);
        var configuration = LdLaunchMemory.ReadConfiguration(fixture.ConsolePath, 2);
        Assert(configuration.MemoryMb == 6144 && configuration.Warning is null, "Missing JSON RAM did not read current machine RAM, or selected a snapshot.");
        Assert(File.ReadAllText(fixture.ConfigurationPath) == "{}" && File.ReadAllText(fixture.MachinePath) == xml && new FileInfo(fixture.ConsolePath).Length == 0, "RAM reads mutated files or executed the console.");
        fixture.Write("{\"advancedSettings.memorySize\":2048}"); fixture.WriteMachine(MachineXml("2048"));
        Assert(LdLaunchMemory.ReadConfiguration(fixture.ConsolePath, 2).MemoryMb == 2048, "Matching configuration sources were not accepted.");
        File.WriteAllText(fixture.MachinePath, MachineXml("2048"), new UTF8Encoding(true));
        Assert(LdLaunchMemory.ReadConfiguration(fixture.ConsolePath, 2).MemoryMb == 2048, "XML UTF-8 BOM was rejected.");
        return Task.CompletedTask;
    }

    private static Task UnknownMachineConfigurationAsync()
    {
        using var fixture = new MemoryFixture(); fixture.Write("{}");
        foreach (var xml in new[]
        {
            "broken PRIVATE_CONFIG_MUST_NOT_APPEAR", "<VirtualBox/>", MachineXml("0"), MachineXml("-1"), MachineXml("65537"), MachineXml("2048.5"), MachineXml("2GB"),
            "<VirtualBox xmlns=\"http://www.virtualbox.org/\"><Machine><Hardware><Memory RAMSize=\"2048\"/><Memory RAMSize=\"8192\"/></Hardware></Machine></VirtualBox>",
            "<VirtualBox xmlns=\"http://www.virtualbox.org/\"><Machine><Hardware><Memory RAMSize=\"2048\"/></Hardware></Machine><Machine><Hardware><Memory RAMSize=\"8192\"/></Hardware></Machine></VirtualBox>",
            "<VirtualBox xmlns=\"http://www.virtualbox.org/\"><Machine><Snapshot><Hardware><Memory RAMSize=\"2048\"/></Hardware></Snapshot></Machine></VirtualBox>",
            "<VirtualBox xmlns=\"http://www.virtualbox.org/\"><Machine><Hardware><Memory RAMSize=\"2048\"/></Hardware><Hardware><Memory RAMSize=\"8192\"/></Hardware></Machine></VirtualBox>",
            MachineXml("2048").Replace("http://www.virtualbox.org/", "https://untrusted.example/"),
            "<!DOCTYPE VirtualBox [<!ENTITY ram \"2048\">]><VirtualBox xmlns=\"http://www.virtualbox.org/\"><Machine><Hardware><Memory RAMSize=\"&ram;\"/></Hardware></Machine></VirtualBox>",
            "<!DOCTYPE VirtualBox SYSTEM \"file:///PRIVATE_CONFIG_MUST_NOT_APPEAR\">" + MachineXml("2048"), new string(' ', 1024 * 1024 + 1)
        })
        {
            fixture.WriteMachine(xml);
            var configuration = LdLaunchMemory.ReadConfiguration(fixture.ConsolePath, 2);
            Assert(configuration.MemoryMb is null && !string.IsNullOrWhiteSpace(configuration.Warning), "Unsafe or ambiguous machine configuration supplied RAM.");
            Assert(!configuration.Warning!.Contains("PRIVATE_CONFIG_MUST_NOT_APPEAR", StringComparison.Ordinal), "XML configuration contents or parser details leaked.");
        }
        fixture.WriteMachine(MachineXml("2048"));
        using (var locked = new FileStream(fixture.MachinePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            Assert(LdLaunchMemory.ReadConfiguration(fixture.ConsolePath, 2).MemoryMb is null, "Locked XML was accepted.");
        return Task.CompletedTask;
    }

    private static Task SourceConflictsAsync()
    {
        using var fixture = new MemoryFixture(); fixture.WriteMachine(MachineXml("6144"));
        foreach (var json in new[] { "{\"advancedSettings.memorySize\":2048}", "{\"advancedSettings.memorySize\":0}", "{\"advancedSettings.memorySize\":\"6144\"}", "{\"advancedSettings.memorySize\":6144,\"advancedSettings.memorySize\":8192}" })
        {
            fixture.Write(json);
            var configuration = LdLaunchMemory.ReadConfiguration(fixture.ConsolePath, 2);
            Assert(configuration.MemoryMb is null && !string.IsNullOrWhiteSpace(configuration.Warning), "Conflicting or explicit invalid RAM was bypassed by XML fallback.");
        }
        fixture.Write("{\"advancedSettings.memorySize\":6144}");
        Assert(LdLaunchMemory.ReadConfiguration(fixture.ConsolePath, 2).MemoryMb == 6144, "Matching RAM did not recover.");
        fixture.WriteMachine("broken");
        var unverifiedMachine = LdLaunchMemory.ReadConfiguration(fixture.ConsolePath, 2);
        Assert(unverifiedMachine.MemoryMb == 6144 && !string.IsNullOrWhiteSpace(unverifiedMachine.Warning), "Unreadable second source was silently claimed consistent or discarded known JSON RAM.");
        fixture.WriteMachine(MachineXml("6144"));
        using (var locked = new FileStream(fixture.MachinePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
        {
            var configuration = LdLaunchMemory.ReadConfiguration(fixture.ConsolePath, 2);
            Assert(configuration.MemoryMb == 6144 && !string.IsNullOrWhiteSpace(configuration.Warning), "Locked second source did not preserve a diagnostic alongside known JSON RAM.");
        }
        return Task.CompletedTask;
    }

    private static Task ConfigurationBoundsAsync()
    {
        using var fixture = new MemoryFixture();
        fixture.Write("{\"advancedSettings.memorySize\":2048}");
        foreach (var input in new[] { ("", 2), ("ldconsole.exe", 2), (fixture.ConsolePath + '\0', 2), (fixture.ConsolePath, 0), (fixture.ConsolePath, -1), (fixture.ConsolePath, 99999), (fixture.ConsolePath, 1) })
        {
            var configuration = LdLaunchMemory.ReadConfiguration(input.Item1, input.Item2);
            Assert(configuration.MemoryMb is null && !string.IsNullOrWhiteSpace(configuration.Warning), "Invalid or missing configuration did not remain unknown.");
        }
        using (var locked = new FileStream(fixture.ConfigurationPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            Assert(LdLaunchMemory.ReadConfiguration(fixture.ConsolePath, 2).MemoryMb is null, "Unreadable configuration did not remain unknown.");
        fixture.Write(new string(' ', 1024 * 1024 + 1));
        var oversized = LdLaunchMemory.ReadConfiguration(fixture.ConsolePath, 2);
        Assert(oversized.MemoryMb is null && !string.IsNullOrWhiteSpace(oversized.Warning), "Oversized configuration was accepted.");
        return Task.CompletedTask;
    }

    private static Task TypicalCapacityAsync()
    {
        var host = new LdHostMemory(78 * Gib / 10, 16 * Gib, 75);
        foreach (var ram in new[] { 2048, 6144 })
        {
            var result = LdLaunchMemory.Assess(new(ram), host);
            Assert(!result.RequiresConfirmation && result.RecommendedPhysicalBytes == ((ulong)ram + 1536) * Mib, "Fixed 11 GiB or 80-percent gate still rejects normal RAM: " + ram);
        }
        var high = LdLaunchMemory.Assess(new(8192), host);
        Assert(high.RequiresConfirmation && high.RecommendedPhysicalBytes == 9728 * Mib && !string.IsNullOrWhiteSpace(high.Message), "8 GiB instance did not receive an advisory warning.");
        return Task.CompletedTask;
    }

    private static Task RiskThresholdsAsync()
    {
        var ram = 4096 * Mib; var recommended = 5632 * Mib;
        foreach (var load in new uint[] { 0, 80, 89, 90, 100, 101 })
            Assert(!LdLaunchMemory.Assess(new(4096), new(ram, ram, load)).RequiresConfirmation, "Headroom, load or unknown load triggered confirmation with sufficient configured RAM capacity.");
        foreach (var host in new[] { new LdHostMemory(ram - 1, ram, 0), new LdHostMemory(ram, ram - 1, 0), new LdHostMemory(0, ram, 100), new LdHostMemory(ram - 1, ram, 101) })
        {
            var result = LdLaunchMemory.Assess(new(4096), host);
            Assert(result.RequiresConfirmation && result.RecommendedPhysicalBytes == recommended && !string.IsNullOrWhiteSpace(result.Message), "A measured RAM capacity deficit was missed.");
        }
        Assert(LdLaunchMemory.Assess(new(4096, "存在非阻断读取提示"), new(ram - 1, ram, 0)).RequiresConfirmation, "A diagnostic warning suppressed a known deficit.");
        return Task.CompletedTask;
    }

    private static Task UnknownAssessmentAsync()
    {
        foreach (var ram in new int?[] { null, 0, -1, int.MaxValue })
        {
            var result = LdLaunchMemory.Assess(new(ram, "RAM 未知"), new(16 * Gib, 16 * Gib, 10));
            Assert(!result.RequiresConfirmation && result.RecommendedPhysicalBytes is null && !string.IsNullOrWhiteSpace(result.Message) && !result.Message.Contains("满足建议预算", StringComparison.Ordinal), "Unknown RAM was guessed or escalated to confirmation.");
            Assert(!LdLaunchMemory.Assess(new(ram, "RAM 未知"), new(0, 0, 100)).RequiresConfirmation, "Unknown RAM fabricated a deficit comparison.");
        }
        var noHost = LdLaunchMemory.Assess(new(6144), null);
        Assert(!noHost.RequiresConfirmation && noHost.RecommendedPhysicalBytes == 7680 * Mib && noHost.Message.Contains("未知", StringComparison.Ordinal), "Missing host became a capacity deficit or discarded known RAM.");
        var warning = LdLaunchMemory.Assess(new(2048, "配置读取存在警告"), new(16 * Gib, 16 * Gib, 0));
        Assert(!warning.RequiresConfirmation && warning.Message.Contains("配置读取存在警告", StringComparison.Ordinal), "Diagnostic warning was hidden or became a modal.");
        Assert(!LdLaunchMemory.Assess(new(2048), new(16 * Gib, 16 * Gib, 101)).RequiresConfirmation, "Malformed load became a capacity deficit.");
        var screenshot = LdLaunchMemory.Assess(new(null, "实例内存字段缺失"), new(11 * Gib, 20 * Gib, 65));
        Assert(!screenshot.RequiresConfirmation && screenshot.Message.Contains("未知", StringComparison.Ordinal) && !screenshot.Message.Contains("确认风险后", StringComparison.Ordinal), "Reported 11 GiB 65-percent unknown RAM still needs confirmation.");
        return Task.CompletedTask;
    }

    private static Task AssessmentMessageAsync()
    {
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            var host = new LdHostMemory(78 * Gib / 10, 16 * Gib, 75);
            var known = LdLaunchMemory.Assess(new(6144), host);
            foreach (var text in new[] { "配置 RAM 6144 MiB", "可用物理 7.8 GiB", "可用提交 16.0 GiB", "负载 75%", "物理建议预算 7.5 GiB", "512 MiB", "1024 MiB", "非官方最低要求", "采样时刻" })
                Assert(known.Message.Contains(text, StringComparison.Ordinal), "Known memory message omitted a reproducible value: " + text);
            var warning = LdLaunchMemory.Assess(new(8192), host);
            Assert(warning.Message.Contains("物理建议预算 9.5 GiB", StringComparison.Ordinal) && warning.Message.Contains("仍可继续", StringComparison.Ordinal), "Risk message hid its budget or became a hard refusal.");
            var unknownHost = LdLaunchMemory.Assess(new(6144), null);
            foreach (var text in new[] { "配置 RAM 6144 MiB", "可用物理 未知", "可用提交 未知", "负载 未知", "物理建议预算 7.5 GiB" })
                Assert(unknownHost.Message.Contains(text, StringComparison.Ordinal), "Unknown host message fabricated or omitted a value: " + text);
            var unknownRam = LdLaunchMemory.Assess(new(null), host);
            Assert(unknownRam.Message.Contains("配置 RAM 未知", StringComparison.Ordinal) && unknownRam.Message.Contains("物理建议预算 未知", StringComparison.Ordinal), "Unknown RAM message guessed a numeric budget.");
        }
        finally { CultureInfo.CurrentCulture = previous; }
        return Task.CompletedTask;
    }

    private sealed class MemoryFixture : IDisposable
    {
        private readonly string directory = Path.Combine(Path.GetTempPath(), "MatrixDevice.Tests", "LdLaunchMemory", Guid.NewGuid().ToString("N"));
        public string ConsolePath => Path.Combine(directory, "ldconsole.exe");
        public string ConfigurationPath => Path.Combine(directory, "vms", "config", "leidian2.config");
        public string MachinePath => Path.Combine(directory, "vms", "leidian2", "leidian.vbox");
        public MemoryFixture()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ConfigurationPath)!);
            File.WriteAllBytes(ConsolePath, []);
        }
        public void Write(string json) => File.WriteAllText(ConfigurationPath, json, new UTF8Encoding(false));
        public void WriteMachine(string xml) { Directory.CreateDirectory(Path.GetDirectoryName(MachinePath)!); File.WriteAllText(MachinePath, xml, new UTF8Encoding(false)); }
        public void Dispose()
        {
            if (File.Exists(ConfigurationPath)) File.Delete(ConfigurationPath);
            File.Delete(ConsolePath);
            if (File.Exists(MachinePath)) File.Delete(MachinePath);
            if (Directory.Exists(Path.GetDirectoryName(MachinePath)!)) Directory.Delete(Path.GetDirectoryName(MachinePath)!);
            Directory.Delete(Path.GetDirectoryName(ConfigurationPath)!);
            Directory.Delete(Path.Combine(directory, "vms"));
            Directory.Delete(directory);
        }
    }
}
