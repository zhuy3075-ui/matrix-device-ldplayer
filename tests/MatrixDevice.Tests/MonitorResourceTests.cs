using System.ComponentModel;
using System.Diagnostics;
using MatrixDevice.Host;

public static class MonitorResourceTests
{
    public static void Register(Action<string, Func<Task>> test)
    {
        test("monitor host CPU requires two valid samples and measures busy time", HostCpuAsync);
        test("monitor instance CPU uses elapsed time and whole-machine normalization", InstanceCpuAsync);
        test("monitor process identity failure clears CPU history and never changes targets", IdentityAsync);
        test("monitor restart and counter regression reset the baseline", RestartAsync);
        test("monitor invalid targets never reach process sampling", InvalidTargetsAsync);
        test("monitor index zero retains process birth verification and samples both exact targets", PrimaryTargetAsync);
        test("monitor sampling failures are unknown instead of zero and recover cold", FailuresAsync);
        test("monitor concurrent calls serialize sampling and baseline updates", ConcurrentAsync);
        test("monitor native sampling reads only the owned current process", OwnedProcessAsync);
        test("monitor native sampling rejects an exited owned process by its retained handle", ExitedOwnedProcessAsync);
    }

    private static Task HostCpuAsync()
    {
        var current = new ResourceSampler.HostCounters(50, 100, 100, 16000, 8000);
        var sampler = Create(() => current);
        var first = sampler.SampleHost();
        Assert(first.CpuPercent is null && first.Error is null && first.TotalPhysical == 16000 && first.AvailablePhysical == 8000, "First host sample lost memory or invented CPU.");
        current = current with { Idle = 90, Kernel = 200, User = 200 };
        Near(sampler.SampleHost().CpuPercent, 80);
        current = current with { Idle = 190, Kernel = 300 };
        var idle = sampler.SampleHost();
        Near(idle.CpuPercent, 0); Assert(idle.Error is null, "Valid zero CPU was treated as missing.");
        return Task.CompletedTask;
    }

    private static Task InstanceCpuAsync()
    {
        var clock = new ManualClock(); long cpu = 0;
        var reads = new List<int>();
        var sampler = Create(readProcess: (pid, birth) => { reads.Add(pid); return new(birth, cpu, pid == 11 ? 100 : 200); }, clock: clock, processors: 8);
        var target = Target;
        var first = sampler.SampleInstance(target);
        Assert(first.CpuPercent is null && first.WorkingSetBytes == 300 && first.Error is null, "First instance sample invented CPU or lost working set.");
        cpu = TimeSpan.TicksPerSecond; clock.Advance(TimeSpan.FromSeconds(2)); clock.UtcNow = clock.UtcNow.AddHours(-1);
        var second = sampler.SampleInstance(target);
        Near(second.CpuPercent, 12.5);
        Assert(second.ObservedAt < first.ObservedAt && reads.SequenceEqual([11, 22, 11, 22]), "CPU used wall-clock time or sampled an unrelated process.");
        var sameProcess = target with { Index = 2, VBoxPid = target.PlayerPid, VBoxBirth = target.PlayerBirth };
        reads.Clear();
        Assert(sampler.SampleInstance(sameProcess).WorkingSetBytes == 100 && reads.SequenceEqual([11]), "Shared PID was sampled or counted twice.");
        return Task.CompletedTask;
    }

    private static Task IdentityAsync()
    {
        var clock = new ManualClock(); long cpu = 0; var wrongPid = 0; var reads = new List<int>();
        var sampler = Create(readProcess: (pid, birth) => { reads.Add(pid); return new(pid == wrongPid ? birth + 1 : birth, cpu, 10); }, clock: clock);
        sampler.SampleInstance(Target);
        wrongPid = 11; reads.Clear(); clock.Advance(TimeSpan.FromSeconds(1));
        var wrong = sampler.SampleInstance(Target);
        Assert(wrong.Error?.Contains("11", StringComparison.Ordinal) == true && wrong.CpuPercent is null && wrong.WorkingSetBytes is null && reads.SequenceEqual([11]), "Identity mismatch published counters or continued into another target.");
        wrongPid = 22; reads.Clear(); clock.Advance(TimeSpan.FromSeconds(1));
        var wrongVBox = sampler.SampleInstance(Target);
        Assert(wrongVBox.Error?.Contains("22", StringComparison.Ordinal) == true && wrongVBox.CpuPercent is null && wrongVBox.WorkingSetBytes is null && reads.SequenceEqual([11, 22]), "VBox identity mismatch published partial counters.");
        wrongPid = 0; cpu = 100; clock.Advance(TimeSpan.FromSeconds(1));
        Assert(sampler.SampleInstance(Target).CpuPercent is null, "Recovery reused the rejected CPU interval.");
        clock.Advance(TimeSpan.FromSeconds(1)); cpu += TimeSpan.TicksPerSecond;
        Near(sampler.SampleInstance(Target).CpuPercent, 50);
        return Task.CompletedTask;
    }

    private static Task RestartAsync()
    {
        var clock = new ManualClock(); long cpu = TimeSpan.TicksPerSecond;
        var sampler = Create(readProcess: (_, birth) => new(birth, cpu, 10), clock: clock);
        sampler.SampleInstance(Target);
        clock.Advance(TimeSpan.FromSeconds(1)); cpu += TimeSpan.TicksPerSecond;
        Near(sampler.SampleInstance(Target).CpuPercent, 50);
        var restarted = Target with { Generation = 2, PlayerBirth = 1111, VBoxBirth = 2222 };
        clock.Advance(TimeSpan.FromSeconds(1)); cpu = 0;
        var first = sampler.SampleInstance(restarted);
        Assert(first.CpuPercent is null && first.Generation == 2 && first.Error is null, "Restart retained old CPU counters.");
        clock.Advance(TimeSpan.FromSeconds(1)); cpu = TimeSpan.TicksPerSecond;
        Near(sampler.SampleInstance(restarted).CpuPercent, 50);
        clock.Advance(TimeSpan.FromSeconds(1)); cpu = 1;
        var regression = sampler.SampleInstance(restarted);
        Assert(regression.CpuPercent is null && regression.Error is not null, "CPU counter regression became zero or a negative percentage.");
        clock.Advance(TimeSpan.FromSeconds(1));
        Assert(sampler.SampleInstance(restarted).CpuPercent is null, "Counter regression retained the baseline.");
        return Task.CompletedTask;
    }

    private static Task InvalidTargetsAsync()
    {
        var reads = 0;
        var sampler = Create(readProcess: (_, birth) => { reads++; return new(birth, 0, 10); });
        foreach (var target in new[] { Target with { Index = -1 }, Target with { Index = 99999 }, Target with { Generation = 0 }, Target with { PlayerPid = 0 }, Target with { VBoxBirth = 0 }, Target with { VBoxPid = 11, VBoxBirth = 999 } })
        {
            var sample = sampler.SampleInstance(target);
            Assert(sample.Error is not null && sample.CpuPercent is null && sample.WorkingSetBytes is null, "Invalid target produced a resource value.");
        }
        Assert(reads == 0, "Invalid or protected target reached the process reader.");
        return Task.CompletedTask;
    }

    private static Task PrimaryTargetAsync()
    {
        var reads = new List<int>(); var wrongBirth = false;
        var sampler = Create(readProcess: (pid, birth) => { reads.Add(pid); return new(wrongBirth ? birth + 1 : birth, 0, 10); });
        var target = Target with { Index = 0 };
        var sample = sampler.SampleInstance(target);
        Assert(sample.Error is null && sample.WorkingSetBytes == 20 && sample.CpuPercent is null && reads.SequenceEqual([11, 22]), "Index zero did not sample its exact two verified processes.");
        reads.Clear(); wrongBirth = true;
        var rejected = sampler.SampleInstance(target);
        Assert(rejected.Error is not null && rejected.WorkingSetBytes is null && rejected.CpuPercent is null && reads.SequenceEqual([11]), "Index zero bypassed birth verification or continued after identity failure.");
        return Task.CompletedTask;
    }

    private static Task FailuresAsync()
    {
        var clock = new ManualClock(); var fail = false;
        var current = new ResourceSampler.HostCounters(10, 100, 100, 16000, 8000);
        var sampler = Create(() => fail ? throw new Win32Exception(5, "Denied test host") : current,
            (_, birth) => fail ? throw new Win32Exception(5, "Denied test process") : new(birth, 0, 10), clock);
        sampler.SampleHost(); sampler.SampleInstance(Target);
        fail = true; clock.Advance(TimeSpan.FromSeconds(1));
        var hostFailure = sampler.SampleHost(); var processFailure = sampler.SampleInstance(Target);
        Assert(hostFailure.CpuPercent is null && hostFailure.TotalPhysical is null && hostFailure.Error?.Contains("Denied", StringComparison.Ordinal) == true, "Host failure became fake zero.");
        Assert(processFailure.CpuPercent is null && processFailure.WorkingSetBytes is null && processFailure.Error?.Contains("Denied", StringComparison.Ordinal) == true, "Process failure became fake zero.");
        fail = false; clock.Advance(TimeSpan.FromSeconds(1));
        Assert(sampler.SampleHost().CpuPercent is null && sampler.SampleInstance(Target).CpuPercent is null, "Recovery did not reset CPU baselines.");
        var noAdvance = sampler.SampleHost();
        Assert(noAdvance.CpuPercent is null && noAdvance.Error is not null, "Non-advancing host counters invented zero.");
        Assert(sampler.SampleInstance(Target).Error is not null, "Non-advancing monotonic time was accepted.");
        current = current with { AvailablePhysical = 17000 };
        Assert(sampler.SampleHost().Error is not null, "Impossible physical memory was accepted.");
        var zeroProcessors = Create(processors: 0).SampleInstance(Target);
        Assert(zeroProcessors.Error is not null && zeroProcessors.CpuPercent is null, "Invalid processor count produced a percentage.");
        return Task.CompletedTask;
    }

    private static async Task ConcurrentAsync()
    {
        var clock = new ManualClock(); long cpu = 0;
        var sampler = Create(readProcess: (_, birth) => { clock.Advance(TimeSpan.FromSeconds(1)); cpu += TimeSpan.TicksPerSecond; return new(birth, cpu, 10); }, clock: clock);
        var target = Target with { VBoxPid = 11, VBoxBirth = 111 };
        var samples = await Task.WhenAll(Enumerable.Range(0, 64).Select(_ => Task.Run(() => sampler.SampleInstance(target))));
        Assert(samples.Count(sample => sample.CpuPercent is null) == 1 && samples.All(sample => sample.Error is null), "Concurrent sampling lost or raced its baseline.");
        foreach (var sample in samples.Where(sample => sample.CpuPercent.HasValue)) Near(sample.CpuPercent, 25);
    }

    private static Task OwnedProcessAsync()
    {
        var sampler = new ResourceSampler();
        if (!OperatingSystem.IsWindows())
        {
            Assert(sampler.SampleHost().Error is not null, "Unsupported host platform appeared successful.");
            Assert(sampler.SampleInstance(Target).Error is not null, "Unsupported process platform appeared successful.");
            return Task.CompletedTask;
        }
        using var owned = Process.GetCurrentProcess();
        var host = sampler.SampleHost();
        if (host.Error?.Contains("processor groups", StringComparison.Ordinal) == true)
            Assert(host.CpuPercent is null && host.TotalPhysical is null, "Unsupported processor groups published incomplete host counters.");
        else
            Assert(host.Error is null && host.CpuPercent is null && host.TotalPhysical > 0 && host.AvailablePhysical >= 0 && host.AvailablePhysical <= host.TotalPhysical, "Native first host sample failed: " + host.Error);
        var target = new MonitorProcessTarget(1, 1, owned.Id, owned.StartTime.ToUniversalTime().Ticks, owned.Id, owned.StartTime.ToUniversalTime().Ticks);
        var first = sampler.SampleInstance(target);
        Assert(first.Error is null && first.CpuPercent is null && first.WorkingSetBytes > 0, "Native first sample failed on the owned process: " + first.Error);
        var wrong = sampler.SampleInstance(target with { PlayerBirth = target.PlayerBirth + 1, VBoxBirth = target.VBoxBirth + 1 });
        Assert(wrong.Error?.Contains("birth mismatch", StringComparison.Ordinal) == true && wrong.CpuPercent is null && wrong.WorkingSetBytes is null, "Native reader accepted the wrong process birth.");
        Assert(sampler.SampleInstance(target).CpuPercent is null, "Native birth failure retained the old baseline.");
        return Task.CompletedTask;
    }

    private static async Task ExitedOwnedProcessAsync()
    {
        if (!OperatingSystem.IsWindows()) return;
        var executable = Environment.ProcessPath ?? throw new InvalidOperationException("Test process executable is unavailable.");
        var start = new ProcessStartInfo(executable) { UseShellExecute = false, CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden };
        if (Path.GetFileNameWithoutExtension(executable).Equals("dotnet", StringComparison.OrdinalIgnoreCase))
            start.ArgumentList.Add(typeof(MonitorResourceTests).Assembly.Location);
        start.ArgumentList.Add("--ldconsole-test-child"); start.ArgumentList.Add("sleep");
        using var child = Process.Start(start) ?? throw new InvalidOperationException("Cannot start the owned test child.");
        try
        {
            var birth = child.StartTime.ToUniversalTime().Ticks;
            _ = child.Handle;
            child.Kill();
            await child.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(5));
            Assert(child.WaitForExit(5000) && child.Id != Environment.ProcessId, "Owned child termination did not complete.");
            var target = new MonitorProcessTarget(1, 1, child.Id, birth, child.Id, birth);
            var sample = new ResourceSampler().SampleInstance(target);
            Assert(sample.Error?.Contains("has exited", StringComparison.Ordinal) == true && sample.CpuPercent is null && sample.WorkingSetBytes is null,
                $"Exited owned process was not rejected by handle liveness: child {child.Id}, exited {child.HasExited}, code {child.ExitCode}, sample {sample}.");
        }
        finally
        {
            if (!child.HasExited) { child.Kill(); await child.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(5)); }
        }
    }

    private static MonitorProcessTarget Target => new(1, 1, 11, 111, 22, 222);

    private static ResourceSampler Create(Func<ResourceSampler.HostCounters>? readHost = null, Func<int, long, ResourceSampler.ProcessCounters>? readProcess = null, ManualClock? clock = null, int processors = 4)
        => new(readHost ?? (() => new(10, 100, 100, 16000, 8000)), readProcess ?? ((_, birth) => new(birth, 0, 10)), () => processors, clock ?? new());

    private static void Near(double? actual, double expected) => Assert(actual.HasValue && Math.Abs(actual.Value - expected) < .000001, $"Expected CPU {expected}, actual {actual}.");
    private static void Assert(bool valid, string message) { if (!valid) throw new InvalidOperationException(message); }

    private sealed class ManualClock : TimeProvider
    {
        private long timestamp;
        public DateTimeOffset UtcNow { get; set; } = new(2026, 10, 15, 0, 0, 0, TimeSpan.Zero);
        public override long TimestampFrequency => TimeSpan.TicksPerSecond;
        public override DateTimeOffset GetUtcNow() => UtcNow;
        public override long GetTimestamp() => timestamp;
        public void Advance(TimeSpan elapsed) { timestamp += elapsed.Ticks; UtcNow += elapsed; }
    }
}
