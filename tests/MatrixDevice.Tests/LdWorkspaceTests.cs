using System.Globalization;
using MatrixDevice.Core;

public static class LdWorkspaceTests
{
    public static void Register(Action<string, Func<Task>> test)
    {
        test("LD workspace launch validates the whole batch before touching the console", LaunchValidationAsync);
        test("LD workspace launch validates the complete confirmation snapshot before console calls", ConfirmationValidationAsync);
        test("LD workspace launch rejects replacement after user confirmation before launching", ConfirmationReplacementAsync);
        test("LD workspace launch starts once per instance and waits sequentially", SequentialLaunchAsync);
        test("LD workspace launch skips ready instances and waits for existing processes", ExistingProcessesAsync);
        test("LD workspace launch timeout never retries or starts later instances", LaunchTimeoutAsync);
        test("LD workspace launch cancellation leaves launched instances alone", LaunchCancellationAsync);
        test("LD workspace launch resource rejection stops the remaining batch", ResourceRejectionAsync);
        test("LD workspace launch resource cancellation stops the remaining batch without closing guests", ResourceCancellationAsync);
        test("LD workspace launch rejects changed generations after the resource callback", ResourceGenerationAsync);
        test("LD workspace launch rejects changed instance identity while waiting", LaunchIdentityAsync);
        test("LD workspace coordinator serializes overlapping batches", OverlappingBatchesAsync);
        test("LD workspace create validates names before console calls", CreateValidationAsync);
        test("LD workspace create confirms one new index and never copies or deletes", CreateAsync);
        test("LD workspace create retains a nonzero command warning after unique confirmation", CreateReportedFailureAsync);
        test("LD workspace create rejects ambiguous changes without retrying", CreateAmbiguityAsync);
        test("LD workspace create does not swallow cancellation or command timeouts", CreateInterruptionAsync);
        test("LD workspace configure validates all resource values before console calls", ConfigureValidationAsync);
        test("LD workspace configure requires fully stopped and current instances", ConfigurePreconditionsAsync);
        test("LD workspace configure sends only whitelisted resource parameters", ConfigureAsync);
    }

    private static async Task LaunchValidationAsync()
    {
        var runner = new FakeConsole();
        await using var service = new LdPlayerService(runner);
        var coordinator = new LdLaunchCoordinator(service);
        var resources = 0;
        foreach (var indices in new[] { Array.Empty<int>(), new[] { 0 }, new[] { 99999 }, new[] { -1 }, new[] { 1, 1 }, new[] { 1, 0 } })
            await Expect(400, () => coordinator.StartAsync(indices, _ => { resources++; return Task.CompletedTask; }));
        Assert(runner.Calls.Count == 0 && resources == 0, "Invalid launch batch reached the console or resource callback.");
    }

    private static async Task SequentialLaunchAsync()
    {
        var runner = new FakeConsole(); runner.Instances.Add(new(2, "B"));
        runner.OnLaunch = index =>
        {
            if (index == 2) Assert(runner.Instances.Single(item => item.Index == 1).Flag == 1, "Second instance started before the first was ready.");
        };
        await using var service = new LdPlayerService(runner);
        var resources = 0; var checkedIndices = new List<int>(); var progress = new RecordingProgress();
        var result = await Coordinator(service).StartAsync([1, 2], (instance, ct) =>
        {
            ct.ThrowIfCancellationRequested();
            checkedIndices.Add(instance.Index); resources++;
            Assert(instance.ProcessId <= 0 && instance.VBoxProcessId <= 0, "Resource check did not receive the stopped target snapshot.");
            Assert(instance.Index == 1 || runner.Instances.Single(item => item.Index == 1).Flag == 1, "Next resource check ran before the previous instance was ready.");
            return Task.CompletedTask;
        }, progress);
        Assert(resources == 2 && runner.Count("launch") == 2, "Launch or resource checks were skipped or repeated.");
        Assert(checkedIndices.SequenceEqual([1, 2]), "Resource estimates were not associated with each target instance.");
        Assert(result.Select(item => item.Index).SequenceEqual([1, 2]) && result.All(item => item.State == "ready"), "Sequential results were incomplete.");
        Assert(progress.Items.Any(item => item.State == "waiting") && result.All(item => item.Message.Contains("ADB", StringComparison.Ordinal)), "Progress incorrectly claimed guest readiness.");
        Assert(!runner.Calls.Any(call => call.Contains("0") || call[0] is "adb" or "quit" or "quitall"), "Batch touched protected or global operations.");
    }

    private static async Task ConfirmationValidationAsync()
    {
        var runner = new FakeConsole(); runner.Instances.Add(new(2, "B"));
        await using var service = new LdPlayerService(runner);
        var snapshot = (await service.RefreshAsync()).ToArray();
        var before = runner.Calls.Count;
        var resources = 0;
        var invalidSnapshots = new IReadOnlyList<LdPlayerInstance>[]
        {
            [], [snapshot[0]], [snapshot[0], snapshot[0]],
            [snapshot[0], snapshot[1], snapshot[0] with { Index = 3 }],
            [snapshot[0], snapshot[1] with { Index = 3 }]
        };
        var coordinator = Coordinator(service);
        foreach (var invalid in invalidSnapshots)
            await Expect(400, () => coordinator.StartAsync([1, 2], _ => { resources++; return Task.CompletedTask; }, expectedInstances: invalid));
        Assert(runner.Calls.Count == before && resources == 0, "Invalid confirmation snapshot reached the console or resource callback.");
        var result = await coordinator.StartAsync([1, 2], _ => { resources++; return Task.CompletedTask; }, expectedInstances: snapshot.Reverse().ToArray());
        Assert(result.Count == 2 && result.All(item => item.State == "ready") && runner.Count("launch") == 2, "Valid unordered confirmation snapshot blocked normal launch generations.");
    }

    private static async Task ConfirmationReplacementAsync()
    {
        foreach (var rename in new[] { true, false })
        {
            var runner = new FakeConsole(); runner.Instances.Add(new(2, "B"));
            await using var service = new LdPlayerService(runner);
            var snapshot = (await service.RefreshAsync()).ToArray();
            if (rename) runner.Instances[0].Name = "replacement";
            else
            {
                runner.Instances[0].Width++;
                await service.RefreshAsync();
                runner.Instances[0].Width--;
                await service.RefreshAsync();
            }
            var resources = 0;
            await Expect(409, () => Coordinator(service).StartAsync([1, 2], _ => { resources++; return Task.CompletedTask; }, expectedInstances: snapshot));
            Assert(resources == 0 && runner.Count("launch") == 0, "Replacement after confirmation launched a changed or later instance.");
        }
    }

    private static async Task ExistingProcessesAsync()
    {
        var runner = new FakeConsole();
        runner.Instances[0] = new(1, "A", 1, 101, 201);
        runner.Instances.Add(new(2, "B", 2, 102, -1) { BootReads = 3 });
        await using var service = new LdPlayerService(runner);
        var result = await Coordinator(service).StartAsync([1, 2], (instance, ct) => throw new InvalidOperationException("Existing processes must not reserve another launch."));
        Assert(result[0].State == "skipped" && result[1].State == "ready" && runner.Count("launch") == 0, "Existing processes were relaunched or marked ready too early.");
        var zeroPid = new FakeConsole(); zeroPid.Instances[0] = new(1, "A", 2) { BootReads = 3 };
        await using var zeroPidService = new LdPlayerService(zeroPid);
        var progress = new RecordingProgress();
        var zeroPidResult = await Coordinator(zeroPidService).StartAsync([1], _ => throw new InvalidOperationException("Starting flag must not reserve another launch."), progress);
        Assert(zeroPidResult[0].State == "ready" && zeroPid.Count("launch") == 0 && progress.Items.Any(item => item.Instance is { IsStarting: true, ProcessId: <= 0, VBoxProcessId: <= 0 }), "Zero-PID startup flag was relaunched or never observed while waiting.");
    }

    private static async Task LaunchTimeoutAsync()
    {
        var runner = new FakeConsole { NeverReady = true }; runner.Instances.Add(new(2, "B"));
        await using var service = new LdPlayerService(runner);
        await Expect(504, () => new LdLaunchCoordinator(service, TimeSpan.FromMilliseconds(40), TimeSpan.FromMilliseconds(2))
            .StartAsync([1, 2], _ => Task.CompletedTask));
        Assert(runner.Count("launch") == 1 && runner.Calls.Where(call => call[0] == "launch").All(call => call[2] == "1"), "Timed out launch was retried or the next instance started.");
        Assert(runner.Count("quit") == 0, "Timed out launch closed a guest.");
    }

    private static async Task LaunchCancellationAsync()
    {
        var runner = new FakeConsole { NeverReady = true }; runner.Instances.Add(new(2, "B"));
        await using var service = new LdPlayerService(runner);
        using var cancellation = new CancellationTokenSource();
        var progress = new RecordingProgress(item => { if (item.State == "waiting") cancellation.Cancel(); });
        try { await Coordinator(service).StartAsync([1, 2], _ => Task.CompletedTask, progress, cancellation.Token); throw new InvalidOperationException("Cancellation was ignored."); }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested) { }
        Assert(runner.Count("launch") == 1 && runner.Count("quit") == 0, "Cancellation retried, continued, or closed the owned launch.");
    }

    private static async Task ResourceRejectionAsync()
    {
        var runner = new FakeConsole(); runner.Instances.Add(new(2, "B"));
        await using var service = new LdPlayerService(runner);
        var resources = 0;
        await Expect(503, () => Coordinator(service).StartAsync([1, 2], _ => ++resources == 1 ? Task.CompletedTask : throw new DeviceException("reserve rejected", 503, 2000)));
        Assert(resources == 2 && runner.Count("launch") == 1 && runner.Count("quit") == 0, "Resource rejection did not stop after the first ready instance.");
    }

    private static async Task ResourceCancellationAsync()
    {
        var runner = new FakeConsole(); runner.Instances.Add(new(2, "B")); runner.Instances.Add(new(3, "C"));
        await using var service = new LdPlayerService(runner);
        using var cancellation = new CancellationTokenSource();
        var checkedIndices = new List<int>();
        try
        {
            await Coordinator(service).StartAsync([1, 2, 3], (instance, ct) =>
            {
                checkedIndices.Add(instance.Index);
                if (instance.Index == 2) cancellation.Cancel();
                ct.ThrowIfCancellationRequested();
                return Task.CompletedTask;
            }, ct: cancellation.Token);
            throw new InvalidOperationException("Resource warning cancellation was ignored.");
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested) { }
        Assert(checkedIndices.SequenceEqual([1, 2]) && runner.Count("launch") == 1 && runner.Count("quit") == 0,
            "Declined resource warning started the declined or later instance, or closed an already started instance.");
    }

    private static async Task LaunchIdentityAsync()
    {
        var runner = new FakeConsole { RenameAfterLaunch = true }; runner.Instances.Add(new(2, "B"));
        await using var service = new LdPlayerService(runner);
        await Expect(409, () => Coordinator(service).StartAsync([1, 2], _ => Task.CompletedTask));
        Assert(runner.Count("launch") == 1, "Changed target identity was retried or another target started.");
    }

    private static async Task ResourceGenerationAsync()
    {
        var runner = new FakeConsole(); runner.Instances.Add(new(2, "B"));
        await using var service = new LdPlayerService(runner);
        var resourceChecks = 0;
        await Expect(409, () => Coordinator(service).StartAsync([1, 2], async ct =>
        {
            resourceChecks++;
            var instance = runner.Instances[0];
            var width = instance.Width;
            instance.Width++;
            await service.RefreshAsync(ct);
            instance.Width = width;
            await service.RefreshAsync(ct);
        }));
        Assert(resourceChecks == 1 && runner.Count("launch") == 0, "Changed original generation was silently replaced or a later target started.");
    }

    private static async Task OverlappingBatchesAsync()
    {
        var runner = new FakeConsole(); runner.Instances.Add(new(2, "B"));
        await using var service = new LdPlayerService(runner);
        var coordinator = Coordinator(service);
        await Task.WhenAll(coordinator.StartAsync([1, 2], _ => Task.CompletedTask), coordinator.StartAsync([1, 2], _ => Task.CompletedTask));
        Assert(runner.Count("launch") == 2, "Overlapping batches launched an instance more than once.");
    }

    private static async Task CreateValidationAsync()
    {
        var runner = new FakeConsole();
        await using var service = new LdPlayerService(runner);
        var editor = new LdInstanceEditor(runner, service);
        foreach (var name in new[] { "", " ", "--option", "bad\nname", "bad\n", "\tbad", "bad\0name", "bad,name", new string('a', 81) })
            await Expect(400, () => editor.CreateAsync(name));
        Assert(runner.Calls.Count == 0, "Invalid creation name reached the CLI.");
        await Expect(409, () => editor.CreateAsync("A"));
        Assert(runner.Count("add") == 0, "Duplicate name created another instance.");
    }

    private static async Task CreateAsync()
    {
        var runner = new FakeConsole();
        await using var service = new LdPlayerService(runner);
        var result = await new LdInstanceEditor(runner, service).CreateAsync("测试实例 B");
        Assert(result.Instance.Index == 2 && result.Instance.Name == "测试实例 B" && !result.CommandReportedFailure, "Creation result was not the unique new instance.");
        Assert(runner.Count("add") == 1 && runner.Calls.All(call => call[0] is "list2" or "add"), "Creation retried or used unrelated operations.");
    }

    private static async Task CreateReportedFailureAsync()
    {
        var runner = new FakeConsole { AddReportsFailure = true };
        await using var service = new LdPlayerService(runner);
        var result = await new LdInstanceEditor(runner, service).CreateAsync("B");
        Assert(result.CommandReportedFailure && result.Instance.Index == 2 && result.Message.Contains("退出码=1", StringComparison.Ordinal), "Creation command failure warning was lost.");
        Assert(runner.Count("add") == 1, "Nonzero add was blindly retried.");
    }

    private static async Task CreateAmbiguityAsync()
    {
        var runner = new FakeConsole { AddExtraInstance = true };
        await using var service = new LdPlayerService(runner);
        await Expect(409, () => new LdInstanceEditor(runner, service).CreateAsync("B"));
        Assert(runner.Count("add") == 1 && runner.Count("remove") == 0, "Ambiguous creation retried or deleted data.");
        var wrongNameRunner = new FakeConsole { CreatedNameOverride = "external" };
        await using var wrongNameService = new LdPlayerService(wrongNameRunner);
        await Expect(409, () => new LdInstanceEditor(wrongNameRunner, wrongNameService).CreateAsync("B"));
        Assert(wrongNameRunner.Count("add") == 1, "Wrong-name creation was accepted or retried.");
    }

    private static async Task CreateInterruptionAsync()
    {
        var timedOut = new FakeConsole { AddReportsTimeout = true };
        await using var timeoutService = new LdPlayerService(timedOut);
        await Expect(504, () => new LdInstanceEditor(timedOut, timeoutService).CreateAsync("B"));
        Assert(timedOut.Count("add") == 1 && timedOut.Count("list2") == 1, "Timed out creation issued more CLI commands.");
        using var cancellation = new CancellationTokenSource();
        var cancelled = new FakeConsole { OnAdd = cancellation.Cancel };
        await using var cancelledService = new LdPlayerService(cancelled);
        try { await new LdInstanceEditor(cancelled, cancelledService).CreateAsync("B", cancellation.Token); throw new InvalidOperationException("Create cancellation was ignored."); }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested) { }
        Assert(cancelled.Count("add") == 1 && cancelled.Count("list2") == 1 && cancelled.Count("remove") == 0, "Cancelled creation retried, queried again, or deleted an instance.");
    }

    private static async Task ConfigureValidationAsync()
    {
        var runner = new FakeConsole();
        await using var service = new LdPlayerService(runner);
        var editor = new LdInstanceEditor(runner, service);
        var valid = new LdInstanceConfiguration(2, 2048, 720, 1280, 240);
        foreach (var configuration in new[] { valid with { Cpu = 0 }, valid with { Cpu = 6 }, valid with { MemoryMb = 0 }, valid with { MemoryMb = 3072 }, valid with { Width = 0 }, valid with { Height = 100_000 }, valid with { Dpi = 0 } })
            await Expect(400, () => editor.ConfigureAsync(1, 1, configuration));
        foreach (var index in new[] { 0, -1, 99999 }) await Expect(400, () => editor.ConfigureAsync(index, 1, valid));
        await Expect(400, () => editor.ConfigureAsync(1, 0, valid));
        Assert(runner.Calls.Count == 0, "Invalid configuration reached the console.");
    }

    private static async Task ConfigurePreconditionsAsync()
    {
        var runner = new FakeConsole();
        await using var service = new LdPlayerService(runner);
        var editor = new LdInstanceEditor(runner, service);
        var configuration = new LdInstanceConfiguration(2, 2048, 720, 1280, 240);
        foreach (var instance in new[] { new FakeInstance(1, "A", 1, 101, 201), new FakeInstance(1, "A", 2, 101, -1), new FakeInstance(1, "A", 2), new FakeInstance(1, "A", 0, -1, 201), new FakeInstance(1, "A") { TopWindow = 1 } })
        {
            runner.Instances[0] = instance;
            var current = (await service.RefreshAsync()).Single();
            await Expect(409, () => editor.ConfigureAsync(1, current.Generation, configuration));
        }
        runner.Instances[0] = new(1, "A");
        var stopped = (await service.RefreshAsync()).Single(); runner.Instances[0].Width++;
        await Expect(409, () => editor.ConfigureAsync(1, stopped.Generation, configuration));
        Assert(runner.Count("modify") == 0, "Running, starting, or stale instance was configured.");
    }

    private static async Task ConfigureAsync()
    {
        var runner = new FakeConsole();
        await using var service = new LdPlayerService(runner);
        var current = (await service.RefreshAsync()).Single();
        var result = await new LdInstanceEditor(runner, service).ConfigureAsync(1, current.Generation, new(4, 4096, 1080, 1920, 480));
        Assert(result.Accepted && !result.Verified && result.Instance?.Width == 1080 && result.Instance.Height == 1920, "Resource command result overclaimed verification or lost the observation.");
        var call = runner.Calls.Single(args => args[0] == "modify");
        Assert(call.SequenceEqual(["modify", "--index", "1", "--cpu", "4", "--memory", "4096", "--resolution", "1080,1920,480"]), "Unapproved configuration arguments were sent.");
    }

    private static LdLaunchCoordinator Coordinator(LdPlayerService service) => new(service, TimeSpan.FromSeconds(2), TimeSpan.FromMilliseconds(1));
    private static void Assert(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    private static async Task Expect(int status, Func<Task> action)
    {
        try { await action(); }
        catch (DeviceException error) when (error.Status == status) { return; }
        throw new InvalidOperationException($"Expected DeviceException status={status}.");
    }

    private sealed class RecordingProgress(Action<LdLaunchProgress>? observer = null) : IProgress<LdLaunchProgress>
    {
        public List<LdLaunchProgress> Items { get; } = [];
        public void Report(LdLaunchProgress value) { Items.Add(value); observer?.Invoke(value); }
    }

    private sealed class FakeInstance(int index, string name, int flag = 0, int processId = -1, int vboxPid = -1)
    {
        public int Index { get; } = index;
        public string Name { get; set; } = name;
        public int Flag { get; set; } = flag;
        public int ProcessId { get; set; } = processId;
        public int VBoxPid { get; set; } = vboxPid;
        public long TopWindow { get; set; }
        public int Width { get; set; } = 720;
        public int Height { get; set; } = 1280;
        public int Dpi { get; set; } = 240;
        public int BootReads { get; set; }
        public string List2() => string.Join(',', Index, Name, TopWindow, 0, Flag, ProcessId, VBoxPid, Width, Height, Dpi);
    }

    private sealed class FakeConsole : ILdConsoleRunner
    {
        public List<FakeInstance> Instances { get; } = [new(1, "A")];
        public List<string[]> Calls { get; } = [];
        public bool NeverReady { get; init; }
        public bool RenameAfterLaunch { get; init; }
        public bool AddReportsFailure { get; init; }
        public bool AddReportsTimeout { get; init; }
        public bool AddExtraInstance { get; init; }
        public string? CreatedNameOverride { get; init; }
        public Action<int>? OnLaunch { get; set; }
        public Action? OnAdd { get; init; }
        public int Count(string command) => Calls.Count(call => call[0] == command);
        public Task<string> RunAsync(IReadOnlyList<string> arguments, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            var args = arguments.ToArray(); Calls.Add(args);
            if (args[0] == "list2")
            {
                foreach (var instance in Instances.Where(item => item.Flag == 2 && item.BootReads > 0))
                    if (--instance.BootReads == 0) { instance.Flag = 1; instance.ProcessId = 100 + instance.Index; instance.VBoxPid = 200 + instance.Index; }
                return Task.FromResult(string.Join('\n', Instances.Select(item => item.List2())));
            }
            if (args[0] == "launch")
            {
                var index = int.Parse(args[2], CultureInfo.InvariantCulture); OnLaunch?.Invoke(index);
                var instance = Instances.Single(item => item.Index == index);
                instance.Flag = 2; instance.ProcessId = 100 + index; instance.VBoxPid = 200 + index;
                instance.BootReads = NeverReady ? 0 : 3;
                if (RenameAfterLaunch) instance.Name = "changed";
            }
            else if (args[0] == "add")
            {
                Instances.Add(new(Instances.Max(item => item.Index) + 1, CreatedNameOverride ?? args[2]));
                if (AddExtraInstance) Instances.Add(new(Instances.Max(item => item.Index) + 1, "external"));
                OnAdd?.Invoke();
                if (AddReportsTimeout) throw new DeviceException("雷电 CLI 超时：命令=add，结果未知", 504, 2000);
                if (AddReportsFailure) throw new DeviceException("雷电 CLI 执行失败：命令=add，退出码=1", 503, 2000);
            }
            else if (args[0] == "modify")
            {
                var instance = Instances.Single(item => item.Index == int.Parse(args[2], CultureInfo.InvariantCulture));
                var size = args[8].Split(',').Select(value => int.Parse(value, CultureInfo.InvariantCulture)).ToArray();
                instance.Width = size[0]; instance.Height = size[1]; instance.Dpi = size[2];
            }
            else throw new InvalidOperationException("Unexpected fake console command: " + args[0]);
            return Task.FromResult("");
        }
    }
}
