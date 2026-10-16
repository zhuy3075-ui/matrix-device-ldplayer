using System.Collections.Concurrent;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Text.Json;
using MatrixDevice.Core;
using MatrixDevice.Features;

internal static class ScriptJobTests
{
    private static readonly TimeSpan Deadline = TimeSpan.FromSeconds(3);

    public static void Register(Action<string, Func<Task>> test)
    {
        test("Script jobs freeze differentiated target parameters and retain actual Demo results", () => Demo(async probe =>
        {
            probe.Backend.HoldBeforeFirstTap = true;
            var source = BoundPackage();
            var firstValues = new Dictionary<string, JsonElement> { ["point"] = Element("\"80,90\"") };
            var secondValues = new Dictionary<string, JsonElement> { ["point"] = Element("\"100,110\"") };
            var a = probe.Submit("DEMO-001", source, overrides: firstValues);
            await probe.Backend.Entered.Task.WaitAsync(Deadline);
            var b = probe.Submit("DEMO-002", source, overrides: secondValues);
            source.Steps.Clear(); source.Parameters.Clear(); firstValues["point"] = Element("null"); secondValues.Clear();
            probe.Backend.Release.TrySetResult();
            await Task.WhenAll(a.Completion, b.Completion).WaitAsync(Deadline);
            var jobs = probe.Scheduler.GetJobs();
            Check(a.JobId != Guid.Empty && b.JobId != Guid.Empty && a.JobId != b.JobId && jobs.Count == 2, "Accepted targets did not receive distinct jobs.");
            foreach (var handle in new[] { a, b })
            {
                var status = probe.Job(handle);
                Check(status.JobId == handle.JobId && status.State == ScriptRunState.Completed && status.EndedUtc is { } ended && ended >= status.StartedUtc &&
                    status.PackageId == "job-package" && status.PackageVersion == "author-7" && status.StepCount == 2 && status.StepIndex == 1,
                    "A job lost its frozen package, progress or terminal metadata.");
                Check(!string.IsNullOrWhiteSpace(status.Result) && probe.Scheduler.GetJobLogs(handle.JobId).Count > 0, "Terminal result or task logs are missing.");
            }
            Check(probe.Backend.Calls.Count == 4 && probe.Backend.Calls.Where(call => call.Target.Serial == "DEMO-001").All(call => call.Value == "80,90") &&
                probe.Backend.Calls.Where(call => call.Target.Serial == "DEMO-002").All(call => call.Value == "100,110"), "Compiled values or targets drifted after admission.");
            Check(await probe.Backend.Taps("DEMO-001") == "Tap 2" && await probe.Backend.Taps("DEMO-002") == "Tap 2" &&
                await probe.Backend.Taps("DEMO-003") == "Tap 0", "Actual Demo effects were lost or crossed to the unselected third target.");
            Check(probe.Backend.LegacyWrites == 0, "A pinned job fell back to the serial-only backend interface.");
        }));

        test("Script jobs reject invalid parameters identities and pre-cancellation without admission", () => Demo(async probe =>
        {
            var target = probe.Target("DEMO-001");
            foreach (var invalid in new[] { target with { Generation = 0 }, target with { Generation = target.Generation + 1 },
                target with { Serial = "missing" }, target with { IsDemo = false } })
                Reject(() => probe.Scheduler.SubmitJob(invalid, Package(Click())));
            var invalidPackage = BoundPackage();
            Reject(() => probe.Scheduler.SubmitJob(target, invalidPackage, parameterOverrides: new Dictionary<string, JsonElement> { ["point"] = Element("null") }));
            using var cancelled = new CancellationTokenSource(); cancelled.Cancel();
            Exception? cancellation = null;
            try { probe.Scheduler.SubmitJob(target, Package(Click()), cancelled.Token); } catch (Exception error) { cancellation = error; }
            Check(cancellation is OperationCanceledException, "Pre-cancelled submission was admitted or relabelled.");
            Check(probe.Scheduler.GetJobs().Count == 0 && probe.Scheduler.GetStatus(target.Serial).State == ScriptRunState.Idle &&
                probe.Scheduler.GetLogs(target.Serial).Count == 0 && probe.Backend.Calls.IsEmpty && probe.Backend.LegacyWrites == 0,
                "A rejected submission created jobs, state, logs or backend work.");
            Check(await probe.Backend.Taps("DEMO-001") == "Tap 0" && await probe.Backend.Taps("DEMO-002") == "Tap 0", "Rejected work changed Demo state.");
        }));

        test("Script jobs busy rejection preserves the accepted job and its existing logs", () => Demo(async probe =>
        {
            probe.Backend.HoldBeforeFirstTap = true;
            var accepted = probe.Submit("DEMO-001", Package(Click()));
            await probe.Backend.Entered.Task.WaitAsync(Deadline);
            var before = probe.Scheduler.GetJobLogs(accepted.JobId).ToArray();
            var serialBefore = probe.Scheduler.GetLogs("DEMO-001").ToArray();
            Reject(() => probe.Scheduler.SubmitJob(probe.Target("DEMO-001"), Package(Click())));
            Check(probe.Scheduler.GetJobs().Count == 1 && probe.Job(accepted).State == ScriptRunState.Running &&
                before.SequenceEqual(probe.Scheduler.GetJobLogs(accepted.JobId)) && serialBefore.SequenceEqual(probe.Scheduler.GetLogs("DEMO-001")),
                "Busy rejection replaced the accepted job or cleared its logs.");
            probe.Backend.Release.TrySetResult(); await accepted.Completion.WaitAsync(Deadline);
            Check(await probe.Backend.Taps("DEMO-001") == "Tap 1", "Busy rejection duplicated or lost the accepted action.");
        }));

        test("Script jobs old JobId cancellation cannot stop a new job on the same serial", () => Demo(async probe =>
        {
            var old = probe.Submit("DEMO-001", Package(Click())); await old.Completion.WaitAsync(Deadline);
            var oldStatus = probe.Job(old); var oldLogs = probe.Scheduler.GetJobLogs(old.JobId).ToArray();
            var current = probe.Submit("DEMO-001", Package(new ScriptStep { Action = "wait", DurationMs = 10000 }, Click()));
            Check(!probe.Scheduler.CancelJob(old.JobId) && !probe.Scheduler.CancelJob(Guid.NewGuid()), "An old or unknown job was reported cancellable.");
            Check(probe.Job(current).State == ScriptRunState.Running && !current.Completion.IsCompleted &&
                probe.Job(old) == oldStatus && oldLogs.SequenceEqual(probe.Scheduler.GetJobLogs(old.JobId)), "Cancelling an old JobId mutated a newer run or historical record.");
            Check(probe.Scheduler.CancelJob(current.JobId), "Current job did not accept cancellation.");
            await current.Completion.WaitAsync(Deadline);
            Check(probe.Job(current).State == ScriptRunState.Cancelled && await probe.Backend.Taps("DEMO-001") == "Tap 1", "Cancelled successor executed its next action.");
        }));

        test("Script jobs cancellation waits for in-flight work and does not affect another target", () => Demo(async probe =>
        {
            probe.Backend.HoldBeforeFirstTap = true; probe.Backend.IgnoreHeldCancellation = true;
            var a = probe.Submit("DEMO-001", Package(Click(), Click()));
            await probe.Backend.Entered.Task.WaitAsync(Deadline);
            var b = probe.Submit("DEMO-002", Package(Click()));
            Check(probe.Scheduler.CancelJob(a.JobId), "In-flight job rejected cancellation.");
            Check(probe.Job(a).State == ScriptRunState.Cancelling && probe.Job(a).EndedUtc is null && !a.Completion.IsCompleted &&
                probe.Backend.CommandTasks.Any(task => !task.IsCompleted), "Job became terminal before its non-cooperative backend task drained.");
            await b.Completion.WaitAsync(Deadline);
            Check(probe.Job(b).State == ScriptRunState.Completed && await probe.Backend.Taps("DEMO-002") == "Tap 1", "Cancelling A affected B.");
            probe.Backend.Release.TrySetResult(); await a.Completion.WaitAsync(Deadline);
            Check(probe.Job(a).State == ScriptRunState.Cancelled && probe.Job(a).EndedUtc is not null &&
                await probe.Backend.Taps("DEMO-001") == "Tap 1" && probe.Backend.CommandTasks.All(task => task.IsCompleted),
                "Cancellation failed to drain an already accepted action or ran a later action.");
        }));

        test("Script jobs reject changed generation between real Demo steps", () => Demo(async probe =>
        {
            probe.Backend.HoldAfterFirstTap = true;
            var handle = probe.Submit("DEMO-001", Package(Click(), Click()));
            await probe.Backend.Entered.Task.WaitAsync(Deadline);
            var generation = probe.Target("DEMO-001").Generation;
            ((DemoDeviceBackend)probe.Backend.Inner).SetRotation("DEMO-001", 180);
            await probe.Catalog.RefreshAsync();
            Check(probe.Target("DEMO-001").Generation > generation, "Fixture did not produce a new generation.");
            probe.Backend.Release.TrySetResult(); var error = await Observe(handle.Completion);
            Check(error is DeviceException { Status: 409 } && probe.Job(handle).State == ScriptRunState.Failed && !string.IsNullOrWhiteSpace(probe.Job(handle).Error) &&
                await probe.Backend.Taps("DEMO-001") == "Tap 1" && await probe.Backend.Taps("DEMO-002") == "Tap 0",
                "A frozen job continued on the new generation or reported success.");
        }));

        test("Script jobs cancellation while queued cannot finish before the actual ADB predecessor", () => Adb(async (probe, server, backend) =>
        {
            var blocked = probe.Track(backend.ShellAsync("allowed", "gated"));
            await server.GateEntered.Task.WaitAsync(Deadline);
            var handle = probe.Submit("allowed", Package(Click()));
            await probe.Backend.Submitted.Task.WaitAsync(Deadline);
            Check(probe.Scheduler.CancelJob(handle.JobId), "Queued job rejected cancellation.");
            Check(!blocked.IsCompleted && !handle.Completion.IsCompleted && probe.Job(handle).State == ScriptRunState.Cancelling &&
                probe.Job(handle).EndedUtc is null, "Queued cancellation bypassed the actual lane predecessor.");
            server.GateRelease.TrySetResult(); await blocked.WaitAsync(Deadline); await handle.Completion.WaitAsync(Deadline);
            Check(probe.Job(handle).State == ScriptRunState.Cancelled && !server.Requests.Any(IsInput), "Cancelled queued job emitted an input operation.");
            var successor = probe.Submit("allowed", Package(Click())); await successor.Completion.WaitAsync(Deadline);
            Check(server.Requests.Count(IsInput) == 1 && probe.Job(successor).State == ScriptRunState.Completed, "Queue did not recover after the cancelled task drained.");
        }));

        test("Script jobs stale submitted transport rejects before any new ADB request", () => Adb(async (probe, server, backend) =>
        {
            var old = probe.Target("allowed");
            server.TransportId = 2; await probe.Catalog.RefreshAsync();
            Check(backend.ObservedTransportId("allowed") == 2 && probe.Target("allowed").Generation > old.Generation, "Fixture did not discover transport2.");
            var count = server.Requests.Count;
            Reject(() => probe.Scheduler.SubmitJob(old, Package(Click())));
            var commands = ((IIdentityPinnedDeviceBackend)backend).BindIdentity(probe.Catalog);
            var error = await Observe(() => commands.TapAsync(new DeviceCommandTarget(old.Serial, old.Generation, old.TransportId, old.IsDemo), 1, 1));
            Check(error is DeviceException { Status: 409 } && server.Requests.Count == count && probe.Scheduler.GetJobs().Count == 0 && probe.Backend.Calls.IsEmpty,
                "Old transport submission or command reached the new transport, serial fallback, or admission.");
        }));

        test("Script jobs queued transport replacement rejects actual input without serial fallback", () => Adb(async (probe, server, backend) =>
        {
            var blocked = probe.Track(backend.ShellAsync("allowed", "gated"));
            await server.GateEntered.Task.WaitAsync(Deadline);
            var old = probe.Target("allowed"); var handle = probe.Submit("allowed", Package(Click()));
            await probe.Backend.Submitted.Task.WaitAsync(Deadline);
            Check(!handle.Completion.IsCompleted && probe.Backend.Calls.Single().Target.TransportId == old.TransportId, "Fixture did not enqueue the frozen job behind the predecessor.");
            server.TransportId = 2; var refresh = probe.Track(probe.Catalog.RefreshAsync());
            var timer = Stopwatch.StartNew();
            while (backend.ObservedTransportId("allowed") != 2)
            {
                if (timer.Elapsed > Deadline) throw new TimeoutException("Fixture did not observe transport2 before releasing its lane gate.");
                await Task.Delay(5);
            }
            server.GateRelease.TrySetResult(); await blocked.WaitAsync(Deadline);
            var failure = await Observe(handle.Completion); await refresh.WaitAsync(Deadline);
            Check(failure is DeviceException { Status: 409 } && probe.Job(handle).State == ScriptRunState.Failed &&
                !server.Requests.Any(IsInput) && !server.Requests.Any(request => request.StartsWith("host:transport:", StringComparison.Ordinal)),
                "Queued job emitted input on the new transport or fell back to a serial selector.");
            Check(server.Requests.Contains("host:transport-id:2"), "Refresh's legitimate transport2 metadata was not exercised; do not mistake it for job input.");
        }));

        test("Script jobs cannot forge Demo mode on the real identity-pinned backend", () => Adb(async (probe, server, backend) =>
        {
            var count = server.Requests.Count;
            Reject(() => probe.Scheduler.SubmitJob(probe.Target("allowed") with { IsDemo = true, TransportId = null }, Package(Click())));
            Exception? mismatch = null; ScriptScheduler? invalid = null;
            try { invalid = new ScriptScheduler(probe.Backend, probe.Catalog, true); } catch (Exception error) { mismatch = error; }
            finally { if (invalid is not null) await invalid.DisposeAsync().AsTask().WaitAsync(Deadline); }
            Check(mismatch is not null && probe.Scheduler.GetJobs().Count == 0 && server.Requests.Count == count && probe.Backend.Calls.IsEmpty,
                "Caller-controlled Demo flag bypassed the backend mode or caused I/O.");
        }));

        test("Script jobs observer failures cannot break completion or erase bounded task logs", () => Demo(async probe =>
        {
            var changed = 0; var logged = 0;
            probe.Scheduler.JobChanged += _ => { Interlocked.Increment(ref changed); throw new InvalidOperationException("Owned status observer failure"); };
            probe.Scheduler.JobLogAdded += (_, _) => { Interlocked.Increment(ref logged); throw new InvalidOperationException("Owned log observer failure"); };
            var handle = probe.Submit("DEMO-001", Package(Enumerable.Range(0, 550).Select(_ => new ScriptStep { Action = "wait" }).Append(Click()).ToArray()));
            await handle.Completion.WaitAsync(Deadline);
            Check(probe.Job(handle).State == ScriptRunState.Completed && await probe.Backend.Taps("DEMO-001") == "Tap 1" &&
                probe.Scheduler.GetJobLogs(handle.JobId).Count == 500 && probe.Scheduler.GetLogs("DEMO-001").Count == 500 && changed > 0 && logged > 0,
                "An observer broke execution or task/legacy log bounds.");
        }));

        test("Script jobs history retains active work and evicts terminal jobs by completion order", () => Demo(async probe =>
        {
            probe.Backend.HoldBeforeFirstTap = true;
            var active = probe.Submit("DEMO-001", Package(Click())); await probe.Backend.Entered.Task.WaitAsync(Deadline);
            var completed = new List<ScriptJobHandle>();
            for (var i = 0; i < 100; i++)
            {
                var handle = probe.Submit("DEMO-002", Package(new ScriptStep { Action = "wait" }));
                completed.Add(handle); await handle.Completion.WaitAsync(Deadline);
            }
            Check(probe.Scheduler.GetJobs().Count == 101 && probe.Scheduler.GetJob(active.JobId) is not null &&
                completed.All(handle => probe.Scheduler.GetJob(handle.JobId) is not null), "History limit evicted an active task or fewer than 100 terminal tasks.");
            probe.Backend.Release.TrySetResult(); await active.Completion.WaitAsync(Deadline);
            Check(probe.Scheduler.GetJobs().Count == 100 && probe.Scheduler.GetJob(active.JobId)?.State == ScriptRunState.Completed &&
                probe.Scheduler.GetJob(completed[0].JobId) is null && completed.Skip(1).All(handle => probe.Scheduler.GetJob(handle.JobId) is not null),
                "History eviction followed start order rather than terminal completion order.");
        }));

        test("Script jobs retain actual outputs from multiple launch actions", () => Demo(async probe =>
        {
            probe.Backend.LaunchOutputs.Enqueue("first owned launch output");
            probe.Backend.LaunchOutputs.Enqueue("second owned launch output");
            var handle = probe.Submit("DEMO-001", Package(new ScriptStep { Action = "launch", Value = "com.android.settings" },
                new ScriptStep { Action = "launchapp", Value = "com.matrix.demo" }));
            await handle.Completion.WaitAsync(Deadline);
            var result = probe.Job(handle).Result ?? "";
            Check(result.Contains("first owned launch output", StringComparison.Ordinal) && result.Contains("second owned launch output", StringComparison.Ordinal) &&
                (await probe.Backend.DumpAsync("DEMO-001")).PackageName == "com.matrix.demo" &&
                (await probe.Backend.DumpAsync("DEMO-002")).PackageName == "com.matrix.demo" && probe.Backend.Calls.Count == 2,
                "A launch output was replaced by a generic completion string, lost, or dispatched to another target.");
        }));

        test("Script jobs bound Unicode result bytes without corrupting surrogate boundaries", () => Demo(async probe =>
        {
            var output = string.Concat(Enumerable.Repeat("\uD83D\uDE80\u6D4B\u8BD5", 3000));
            probe.Backend.LaunchOutputs.Enqueue(output);
            var handle = probe.Submit("DEMO-001", Package(new ScriptStep { Action = "launch", Value = "com.android.settings" }));
            await handle.Completion.WaitAsync(Deadline);
            var result = probe.Job(handle).Result ?? "";
            Check(new UTF8Encoding(false, true).GetByteCount(result) <= 16 * 1024 && result.Contains("\u622A\u65AD", StringComparison.Ordinal) &&
                result.Contains("\uD83D\uDE80", StringComparison.Ordinal) && !result.Contains('\uFFFD') && result != output &&
                (await probe.Backend.DumpAsync("DEMO-001")).PackageName == "com.android.settings",
                "Long result exceeded its UTF8 bound, lost its truncation marker, corrupted Unicode, or skipped the real launch.");
        }));

        test("Script jobs and legacy runs share occupancy without changing legacy duplicate semantics", () => Demo(async probe =>
        {
            var waiting = Package(new ScriptStep { Action = "wait", DurationMs = 10000 }, Click());
            var legacy = probe.Track(probe.Scheduler.RunAsync("DEMO-001", waiting));
            Reject(() => probe.Scheduler.SubmitJob(probe.Target("DEMO-001"), Package(Click())));
            Check(probe.Scheduler.GetJobs().Count == 0 && !legacy.IsCompleted, "New job admission ignored a legacy run occupying the serial.");
            Check(probe.Scheduler.Stop("DEMO-001"), "Legacy Stop no longer finds its accepted run.");
            await legacy.WaitAsync(Deadline);
            var job = probe.Submit("DEMO-001", waiting);
            await probe.Scheduler.RunAsync("DEMO-001", Package(Click())).WaitAsync(Deadline);
            Check(probe.Scheduler.GetJobs().Count == 1 && probe.Job(job).State == ScriptRunState.Running && !job.Completion.IsCompleted,
                "Legacy duplicate admission replaced a pinned job or manufactured a second job record.");
            Check(probe.Scheduler.CancelJob(job.JobId), "Pinned job cancellation failed after a legacy duplicate.");
            await job.Completion.WaitAsync(Deadline);
            Check(probe.Job(job).State == ScriptRunState.Cancelled && await probe.Backend.Taps("DEMO-001") == "Tap 0" && probe.Backend.LegacyWrites == 0,
                "Shared occupancy executed a duplicate action or changed cancellation semantics.");
        }));

        test("Script jobs recheck generation after SetDevice acknowledgement before sending the command", async () =>
        {
            foreach (var step in new[] { Click(), new ScriptStep { Action = "launch", Value = "com.android.settings" } })
            {
                var server = new SetDeviceBarrierServer(); Exception? primary = null; Exception? cleanup = null;
                try
                {
                    var backend = new AdbDeviceBackend(new AppSettings { AdbAllowedSerials = ["allowed"], CommandTimeoutSeconds = 5 }, server.Endpoint);
                    await Run(backend, null, false, async probe =>
                    {
                        var old = probe.Target("allowed");
                        Check(old.TransportId == 1 && backend.ObservedTransportId("allowed") == 1, "Initial identity was not discovered through the owned wire server.");
                        var transportOrdinal = step.Action == "launch" ? 2 : 1;
                        var baseline = server.Requests.Count;
                        server.ArmNextTransport(transportOrdinal); var handle = probe.Submit("allowed", Package(step));
                        try
                        {
                            await server.Entered.Task.WaitAsync(Deadline);
                            var beforeCommand = server.Requests.ToArray().Skip(baseline).ToArray();
                            Check(beforeCommand.Count(request => request == "host:transport-id:1") == transportOrdinal &&
                                (transportOrdinal == 1 || beforeCommand.Any(request => request.StartsWith("shell,v2,raw:cmd package resolve-activity ", StringComparison.Ordinal))) &&
                                !beforeCommand.Any(IsInput), "Fixture did not reach the intended tap/am-start SetDevice boundary after a valid launcher resolution.");
                            probe.Backend.InventoryOverride = probe.Catalog.Snapshot.Select(info => info with { Width = info.Width + 1 }).ToArray();
                            await probe.Catalog.RefreshAsync();
                            var changed = probe.Target("allowed");
                            Check(changed.Generation > old.Generation && changed.TransportId == old.TransportId && backend.ObservedTransportId("allowed") == 1,
                                "Fixture did not change generation while retaining the same actual transport.");
                        }
                        finally { server.Release.TrySetResult(); }
                        var error = await Observe(handle.Completion);
                        Check(error is DeviceException { Status: 409 } && probe.Job(handle).State == ScriptRunState.Failed &&
                            !server.Requests.Any(IsInput) && !server.Requests.Any(request => request.StartsWith("host:transport:", StringComparison.Ordinal)) &&
                            server.Errors.IsEmpty, "Generation changed during SetDevice but a command was sent, the job succeeded, or the owned wire fixture failed.");
                    });
                }
                catch (Exception error) { primary = error; }
                finally { server.Release.TrySetResult(); try { await server.DisposeAsync().AsTask().WaitAsync(Deadline); } catch (Exception error) { cleanup = error; } }
                if (primary is not null && cleanup is not null) throw new AggregateException(primary, cleanup);
                if (cleanup is not null) throw cleanup;
                if (primary is not null) ExceptionDispatchInfo.Capture(primary).Throw();
            }
        });
    }

    private static bool IsInput(string request) => request.Contains("input tap", StringComparison.Ordinal) || request.Contains("input swipe", StringComparison.Ordinal) || request.Contains("am start", StringComparison.Ordinal);
    private static ScriptStep Click() => new() { Action = "click", Value = "1,1" };
    private static ScriptPackage Package(params ScriptStep[] steps) => new() { Manifest = new() { Id = "job-package", Name = "Owned job package", Version = "author-7" }, Steps = [.. steps] };
    private static ScriptPackage BoundPackage() => Package(Click(), Click()) with
    {
        Parameters = [new() { Key = "point", Type = "string", Value = Element("\"40,50\"") }],
        ExecutionContract = new() { Version = 1, Bindings = [new() { StepIndex = 0, Field = "value", ParameterKey = "point" }, new() { StepIndex = 1, Field = "value", ParameterKey = "point" }] }
    };
    private static JsonElement Element(string json) { using var document = JsonDocument.Parse(json); return document.RootElement.Clone(); }
    private static void Check(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    private static void Reject(Action action)
    {
        try { action(); } catch (DeviceException) { return; }
        throw new InvalidOperationException("Rejected submission did not throw DeviceException before admission.");
    }
    private static async Task<Exception?> Observe(Task task)
    {
        try { await task.WaitAsync(Deadline); return null; } catch (TimeoutException) { throw; } catch (Exception error) { return error; }
    }
    private static async Task<Exception?> Observe(Func<Task> action)
    { try { return await Observe(action()); } catch (TimeoutException) { throw; } catch (Exception error) { return error; } }
    private static Task Demo(Func<Probe, Task> run) => Run(new DemoDeviceBackend(), null, true, run);
    private static async Task Adb(Func<Probe, FakeAdbServer, AdbDeviceBackend, Task> run)
    {
        var server = new FakeAdbServer();
        var backend = new AdbDeviceBackend(new AppSettings { AdbAllowedSerials = ["allowed"], CommandTimeoutSeconds = 5 }, server.Endpoint);
        await Run(backend, server, false, probe => run(probe, server, backend));
    }
    private static async Task Run(IDeviceBackend inner, FakeAdbServer? server, bool demo, Func<Probe, Task> run)
    {
        var backend = new RecordingBackend(inner, demo); var probe = new Probe(backend, demo);
        Exception? failure = null; var cleanup = new List<Exception>();
        try { await probe.Catalog.RefreshAsync(); probe.Scheduler = new ScriptScheduler(backend, probe.Catalog, demo); await run(probe); }
        catch (Exception error) { failure = error; }
        finally
        {
            backend.Release.TrySetResult(); server?.GateRelease.TrySetResult();
            if (probe.Scheduler is not null)
            {
                try { foreach (var job in probe.Scheduler.GetJobs()) if (job.State is ScriptRunState.Running or ScriptRunState.Cancelling) probe.Scheduler.CancelJob(job.JobId); }
                catch (Exception error) { cleanup.Add(error); }
                try { await probe.Scheduler.DisposeAsync().AsTask().WaitAsync(Deadline); } catch (Exception error) { cleanup.Add(error); }
            }
            foreach (var task in probe.Tasks.Concat(backend.CommandTasks).Distinct())
            {
                try { await task.WaitAsync(Deadline); } catch (Exception error) when (task.IsCompleted && error is not TimeoutException)
                { Console.WriteLine($"OBSERVED script-job-task {task.Status}: {error.GetType().Name}: {error.Message}"); }
                catch (Exception error) { cleanup.Add(error); }
            }
            try { await backend.DisposeAsync().AsTask().WaitAsync(Deadline); } catch (Exception error) { cleanup.Add(error); }
            if (server is not null) try { await server.DisposeAsync().AsTask().WaitAsync(Deadline); } catch (Exception error) { cleanup.Add(error); }
        }
        if (cleanup.Count > 0) throw new AggregateException("Owned script job fixture cleanup failed.", failure is null ? cleanup : new[] { failure }.Concat(cleanup));
        if (failure is not null) ExceptionDispatchInfo.Capture(failure).Throw();
    }

    private sealed class Probe(RecordingBackend backend, bool demo)
    {
        public RecordingBackend Backend { get; } = backend;
        public DeviceCatalog Catalog { get; } = new(backend);
        public ScriptScheduler Scheduler { get; set; } = null!;
        public List<Task> Tasks { get; } = [];
        public ScriptJobTarget Target(string serial) { var info = Catalog.GetRequired(serial); return new(info.Serial, info.Generation, info.TransportId, demo); }
        public ScriptJobHandle Submit(string serial, ScriptPackage package, CancellationToken ct = default, IReadOnlyDictionary<string, JsonElement>? overrides = null)
        { var handle = Scheduler.SubmitJob(Target(serial), package, ct, overrides); Track(handle.Completion); return handle; }
        public ScriptJobStatus Job(ScriptJobHandle handle) => Scheduler.GetJob(handle.JobId) ?? throw new InvalidOperationException("Accepted job disappeared.");
        public Task<T> Track<T>(Task<T> task) { Tasks.Add(task); return task; }
        public Task Track(Task task) { Tasks.Add(task); return task; }
    }

    private sealed record Call(DeviceCommandTarget Target, string Action, string Value);
    private sealed class RecordingBackend(IDeviceBackend inner, bool third) : IDeviceBackend, IIdentityPinnedDeviceBackend
    {
        private readonly DemoDeviceBackend? other = third ? new() : null;
        private int firstTap;
        public IDeviceBackend Inner { get; } = inner;
        public ConcurrentQueue<Call> Calls { get; } = new();
        public ConcurrentBag<Task> CommandTasks { get; } = [];
        public ConcurrentQueue<string> LaunchOutputs { get; } = new();
        public bool HoldBeforeFirstTap { get; set; }
        public bool HoldAfterFirstTap { get; set; }
        public bool IgnoreHeldCancellation { get; set; }
        public IReadOnlyList<DeviceInfo>? InventoryOverride { get; set; }
        public int LegacyWrites { get; private set; }
        public TaskCompletionSource Entered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Submitted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public IIdentityPinnedDeviceCommands BindIdentity(DeviceCatalog catalog) => new Commands(this, ((IIdentityPinnedDeviceBackend)inner).BindIdentity(catalog));
        public async Task<IReadOnlyList<DeviceInfo>> DiscoverAsync(CancellationToken ct = default)
        { if (InventoryOverride is { } inventory) { ct.ThrowIfCancellationRequested(); return inventory; } var result = await inner.DiscoverAsync(ct); return other is null ? result : result.Concat((await other.DiscoverAsync(ct)).Take(1).Select(info => info with { Serial = "DEMO-003" })).ToArray(); }
        public Task<AndroidNode> DumpAsync(string serial, CancellationToken ct = default) => serial == "DEMO-003" && other is not null ? other.DumpAsync("DEMO-001", ct) : inner.DumpAsync(serial, ct);
        public async Task<string> Taps(string serial) => (await DumpAsync(serial)).ChildNodeList[1].Text!;
        private Task Forbidden() { LegacyWrites++; throw new InvalidOperationException("Pinned script used the legacy serial-only command interface."); }
        public Task TapAsync(string serial, int x, int y, CancellationToken ct = default) => Forbidden();
        public Task SwipeAsync(string serial, int x1, int y1, int x2, int y2, int durationMs, CancellationToken ct = default) => Forbidden();
        public Task<string> StartAppAsync(string serial, string packageName, string? activity = null, CancellationToken ct = default) { Forbidden(); return Task.FromResult(""); }
        public Task<string> ShellAsync(string serial, string command, bool useRoot = false, CancellationToken ct = default) => inner.ShellAsync(serial, command, useRoot, ct);
        public Task<byte[]> ScreenshotAsync(string serial, CancellationToken ct = default) => inner.ScreenshotAsync(serial, ct);
        public Task KeyEventAsync(string serial, int code, CancellationToken ct = default) => inner.KeyEventAsync(serial, code, ct);
        public Task TextAsync(string serial, string text, CancellationToken ct = default) => inner.TextAsync(serial, text, ct);
        public Task<string> StopAppAsync(string serial, string packageName, CancellationToken ct = default) => inner.StopAppAsync(serial, packageName, ct);
        public Task<bool> ClearAppAsync(string serial, string packageName, CancellationToken ct = default) => inner.ClearAppAsync(serial, packageName, ct);
        public Task<IReadOnlyList<AppInfo>> ListAppsAsync(string serial, bool showSystem = false, bool needVersion = true, CancellationToken ct = default) => inner.ListAppsAsync(serial, showSystem, needVersion, ct);
        public Task InstallAsync(string serial, string localPath, CancellationToken ct = default) => inner.InstallAsync(serial, localPath, ct);
        public Task UninstallAsync(string serial, string packageName, CancellationToken ct = default) => inner.UninstallAsync(serial, packageName, ct);
        public Task PushAsync(string serial, string localPath, string remotePath, IProgress<double>? progress = null, CancellationToken ct = default) => inner.PushAsync(serial, localPath, remotePath, progress, ct);
        public Task PullAsync(string serial, string remotePath, string localPath, IProgress<double>? progress = null, CancellationToken ct = default) => inner.PullAsync(serial, remotePath, localPath, progress, ct);
        public Task SetNodeTextAsync(string serial, NodeSelector selector, string text, CancellationToken ct = default) => inner.SetNodeTextAsync(serial, selector, text, ct);
        public async ValueTask DisposeAsync() { Release.TrySetResult(); await inner.DisposeAsync(); if (other is not null) await other.DisposeAsync(); }

        private sealed class Commands(RecordingBackend owner, IIdentityPinnedDeviceCommands innerCommands) : IIdentityPinnedDeviceCommands
        {
            public bool IsDemo => innerCommands.IsDemo;
            public Task TapAsync(DeviceCommandTarget target, int x, int y, CancellationToken ct = default)
            {
                var task = TapCore(target, x, y, ct); owner.CommandTasks.Add(task); return task;
            }
            private async Task TapCore(DeviceCommandTarget target, int x, int y, CancellationToken ct)
            {
                owner.Calls.Enqueue(new(target, "click", $"{x},{y}"));
                var held = target.Serial == "DEMO-001" && Interlocked.Increment(ref owner.firstTap) == 1;
                if (held && owner.HoldBeforeFirstTap)
                { owner.Entered.TrySetResult(); if (owner.IgnoreHeldCancellation) await owner.Release.Task; else await owner.Release.Task.WaitAsync(ct); }
                var actual = innerCommands.TapAsync(target, x, y, held && owner.IgnoreHeldCancellation ? CancellationToken.None : ct);
                owner.Submitted.TrySetResult(); await actual;
                if (held && owner.HoldAfterFirstTap) { owner.Entered.TrySetResult(); await owner.Release.Task; }
            }
            public Task SwipeAsync(DeviceCommandTarget target, int x1, int y1, int x2, int y2, int durationMs, CancellationToken ct = default)
            { owner.Calls.Enqueue(new(target, "swipe", $"{x1},{y1},{x2},{y2},{durationMs}")); var task = innerCommands.SwipeAsync(target, x1, y1, x2, y2, durationMs, ct); owner.CommandTasks.Add(task); return task; }
            public Task<string> StartAppAsync(DeviceCommandTarget target, string packageName, CancellationToken ct = default)
            { owner.Calls.Enqueue(new(target, "launch", packageName)); var task = LaunchCore(target, packageName, ct); owner.CommandTasks.Add(task); return task; }
            private async Task<string> LaunchCore(DeviceCommandTarget target, string packageName, CancellationToken ct)
            { var actual = await innerCommands.StartAppAsync(target, packageName, ct); return owner.LaunchOutputs.TryDequeue(out var output) ? output : actual; }
        }
    }

    private sealed class SetDeviceBarrierServer : IAsyncDisposable
    {
        private readonly TcpListener listener = new(IPAddress.Loopback, 0);
        private readonly CancellationTokenSource stop = new();
        private readonly ConcurrentBag<Task> clients = [];
        private readonly Task accepting;
        private int armed;
        public ConcurrentQueue<string> Requests { get; } = new();
        public ConcurrentQueue<string> Errors { get; } = new();
        public TaskCompletionSource Entered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public IPEndPoint Endpoint => (IPEndPoint)listener.LocalEndpoint;
        public SetDeviceBarrierServer() { listener.Start(); accepting = Accept(); }
        public void ArmNextTransport(int nth = 1)
        { if (nth is not (1 or 2)) throw new ArgumentOutOfRangeException(nameof(nth)); Interlocked.Exchange(ref armed, nth); }
        private async Task Accept()
        {
            try { while (!stop.IsCancellationRequested) { var client = await listener.AcceptTcpClientAsync(stop.Token); clients.Add(Handle(client)); } }
            catch (Exception error) when (stop.IsCancellationRequested && error is OperationCanceledException or ObjectDisposedException or SocketException) { }
        }
        private async Task Handle(TcpClient client)
        {
            using (client)
            {
                try
                {
                    var stream = client.GetStream();
                    while (!stop.IsCancellationRequested)
                    {
                        var prefix = new byte[4]; await stream.ReadExactlyAsync(prefix, stop.Token);
                        var length = int.Parse(Encoding.ASCII.GetString(prefix), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                        if (length is < 1 or > 8192) throw new InvalidDataException("Unexpected owned ADB request length.");
                        var bytes = new byte[length]; await stream.ReadExactlyAsync(bytes, stop.Token);
                        var request = Encoding.UTF8.GetString(bytes); Requests.Enqueue(request);
                        if (request is "host:version" or "host:devices-l")
                        {
                            var payload = Encoding.UTF8.GetBytes(request == "host:version" ? "0029" : "allowed\tdevice model:Test transport_id:1\n");
                            await stream.WriteAsync(Encoding.ASCII.GetBytes("OKAY" + payload.Length.ToString("X4", CultureInfo.InvariantCulture)), stop.Token);
                            await stream.WriteAsync(payload, stop.Token); return;
                        }
                        if (request == "host:transport-id:1")
                        {
                            if (Volatile.Read(ref armed) > 0 && Interlocked.Decrement(ref armed) == 0) { Entered.TrySetResult(); await Release.Task.WaitAsync(stop.Token); }
                            await stream.WriteAsync("OKAY"u8.ToArray(), stop.Token); continue;
                        }
                        if (!request.StartsWith("shell,v2,raw:", StringComparison.Ordinal)) throw new InvalidDataException("Unexpected owned ADB request: " + request);
                        await stream.WriteAsync("OKAY"u8.ToArray(), stop.Token);
                        var stdin = new byte[5]; await stream.ReadExactlyAsync(stdin, stop.Token);
                        var command = request[13..];
                        var output = command.StartsWith("cmd package resolve-activity ", StringComparison.Ordinal) ? "com.android.settings/.Settings\n" :
                            command.StartsWith("am start -W ", StringComparison.Ordinal) ? "Starting: Intent\nStatus: ok\n" : command switch
                        {
                            "getprop" => "[ro.product.brand]: [Fixture]\n[ro.product.model]: [Test]\n[ro.build.version.release]: [14]\n",
                            "wm size" => "Physical size: 4x3\n",
                            "dumpsys input" => "SurfaceOrientation: 0\n",
                            _ => "fixture\n"
                        };
                        await Packet(stream, 1, Encoding.UTF8.GetBytes(output)); await Packet(stream, 3, [0]); return;
                    }
                }
                catch (EndOfStreamException) { }
                catch (Exception error) when (stop.IsCancellationRequested && error is OperationCanceledException or ObjectDisposedException or IOException or SocketException) { }
                catch (Exception error) { Errors.Enqueue(error.ToString()); }
            }
        }
        private async Task Packet(Stream stream, byte channel, byte[] payload)
        {
            var header = new byte[5]; header[0] = channel; BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(1), payload.Length);
            await stream.WriteAsync(header, stop.Token); await stream.WriteAsync(payload, stop.Token);
        }
        public async ValueTask DisposeAsync() { Release.TrySetResult(); stop.Cancel(); listener.Stop(); await accepting; await Task.WhenAll(clients); stop.Dispose(); }
    }
}
