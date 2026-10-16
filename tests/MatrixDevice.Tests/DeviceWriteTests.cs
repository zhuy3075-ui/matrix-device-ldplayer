using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text.Json;
using MatrixDevice.Core;
using MatrixDevice.Features;
using MatrixDevice.Host;

internal static class DeviceWriteTests
{
    private const string A = "DEMO-001", B = "DEMO-002";
    private static readonly TimeSpan Step = TimeSpan.FromSeconds(5), Drain = TimeSpan.FromSeconds(10);
    private static string? unresolved;
    private static readonly List<object> retained = [];
    public static void Register(Action<string, Func<Task>> test)
    {
        test("Device writes API in-flight rejects same-target script admission", () => Run("W01", async p =>
        {
            p.Backend.Hold("legacy", A);
            var api = p.Post(A); await p.Backend.Entered.Task.WaitAsync(Step);
            ScriptJobHandle? accepted = null; var rejected = Reject(() => accepted = p.Submit(A));
            if (accepted is not null) await p.Backend.PinnedEntered.Task.WaitAsync(Step);
            var observed = await p.Backend.Taps(A);
            p.Observe(new { rejected, accepted = accepted?.JobId, observed, scriptCalls = p.Backend.Count("pinned", A) });
            Check(rejected && p.Backend.Count("pinned", A) == 0 && observed == "Tap 0", "PRODUCT: API in-flight allowed same-target script admission/backend action.");
            p.Backend.Release.TrySetResult(); Check((await api).IsSuccessStatusCode, "Original API writer failed.");
        }));
        test("Device writes script blocks API A but API B and HTTP reads continue", () => Run("W02", async p =>
        {
            p.Backend.Hold("pinned", A); var job = p.Submit(A); await p.Backend.Entered.Task.WaitAsync(Step);
            using var denied = await p.Post(A); using var allowed = await p.Post(B);
            using var read = await p.Track(p.Http.GetAsync(p.Url + "/devices"));
            Check(denied.StatusCode == HttpStatusCode.Conflict && allowed.IsSuccessStatusCode && read.IsSuccessStatusCode, "PRODUCT: same-target exclusion blocked another target or read-only HTTP.");
            Check(p.Backend.Count("legacy", A) == 0 && await p.Backend.Taps(B) == "Tap 1", "Rejected API wrote A or B did not reach real Demo.");
            p.Backend.Release.TrySetResult(); await job.Completion.WaitAsync(Step);
        }));
        test("Device writes competing API requests admit one A while B progresses", () => Run("W03", async p =>
        {
            p.Backend.Hold("legacy", A); var first = p.Post(A); await p.Backend.Entered.Task.WaitAsync(Step);
            using var second = await p.Post(A); using var other = await p.Post(B);
            p.Observe(new { second = (int)second.StatusCode, aCalls = p.Backend.Count("legacy", A), b = await p.Backend.Taps(B) });
            Check(second.StatusCode == HttpStatusCode.Conflict && p.Backend.Count("legacy", A) == 1 && other.IsSuccessStatusCode && await p.Backend.Taps(B) == "Tap 1", "PRODUCT: two overlapping API writers reached A, or independent B was blocked.");
            p.Backend.Release.TrySetResult(); Check((await first).IsSuccessStatusCode, "Original API request did not finish.");
        }));
        test("Device writes aborted API retains ownership until noncooperative backend drains", () => Run("W04", async p =>
        {
            p.Backend.Hold("legacy", A); using var cancel = new CancellationTokenSource();
            var request = p.Post(A, cancel.Token); await p.Backend.Entered.Task.WaitAsync(Step); cancel.Cancel();
            try { await request; } catch (OperationCanceledException) { }
            await p.Backend.CancelObserved.Task.WaitAsync(Step);
            ScriptJobHandle? accepted = null; var rejected = Reject(() => accepted = p.Submit(A));
            if (accepted is not null) await p.Backend.PinnedEntered.Task.WaitAsync(Step);
            p.Observe(new { rejected, accepted = accepted?.JobId, heldTaskPending = p.Backend.Tasks.Any(task => !task.IsCompleted) });
            Check(rejected && p.Backend.Count("pinned", A) == 0, "PRODUCT: HTTP cancellation released ownership before original backend task drained.");
            p.Backend.Release.TrySetResult(); await p.Backend.Drained.Task.WaitAsync(Step);
        }));
        test("Device writes Input fanout rejects atomically when API owns B", () => Run("W05", async p =>
        {
            await p.Input.HandoffAsync(A); p.Backend.Hold("legacy", B); var api = p.Post(B); await p.Backend.Entered.Task.WaitAsync(Step);
            var rejected = await RejectAsync(() => p.Track(p.Input.SendAsync(p.Snapshot(InputKind.Tap, [B]))));
            p.Observe(new { rejected, aCalls = p.Backend.Count("legacy", A), bCalls = p.Backend.Count("legacy", B), a = await p.Backend.Taps(A) });
            Check(rejected && p.Backend.Count("legacy", A) == 0 && p.Backend.Count("legacy", B) == 1 && await p.Backend.Taps(A) == "Tap 0", "PRODUCT: Input batch partly wrote A/B despite B being occupied.");
            p.Backend.Release.TrySetResult(); await api;
        }));
        test("Device writes Input holds its whole target group until all backend actions drain", () => Run("W06", async p =>
        {
            await p.Input.HandoffAsync(A); p.Backend.Hold("legacy", A);
            var input = p.Track(p.Input.SendAsync(p.Snapshot(InputKind.Tap, [B]))); await p.Backend.Entered.Task.WaitAsync(Step);
            await p.Backend.BDrained.Task.WaitAsync(Step);
            using var denied = await p.Post(B);
            Check(denied.StatusCode == HttpStatusCode.Conflict && p.Backend.Count("legacy", B) == 1, "PRODUCT: completed B part released its group ownership while A remained in-flight.");
            p.Backend.Release.TrySetResult(); await input.WaitAsync(Step);
            Check(await p.Backend.Taps(A) == "Tap 1" && await p.Backend.Taps(B) == "Tap 1", "Input group did not execute exactly once per target.");
        }));
        test("Device writes cancelling script keeps A occupied until its actual task finishes", () => Run("W07", async p =>
        {
            p.Backend.Hold("pinned", A); var job = p.Submit(A); await p.Backend.Entered.Task.WaitAsync(Step);
            Check(p.Scheduler.CancelJob(job.JobId), "Accepted job refused cancellation.");
            using var denied = await p.Post(A);
            Check(denied.StatusCode == HttpStatusCode.Conflict && !job.Completion.IsCompleted && p.Scheduler.GetJob(job.JobId)!.State == ScriptRunState.Cancelling, "PRODUCT: cancelling job released A before backend drain.");
            p.Backend.Release.TrySetResult(); await job.Completion.WaitAsync(Step);
            var next = p.Submit(A); await next.Completion.WaitAsync(Step);
            Check(p.Scheduler.GetJob(next.JobId)!.State == ScriptRunState.Completed && await p.Backend.Taps(A) == "Tap 2", "Drained cancellation leaked the lease or cancelled its successor.");
        }));
        test("Device writes backend exception releases API lease for a later request", () => Run("W08", async p =>
        {
            p.Backend.ThrowNext = true; using var failed = await p.Post(A); using var next = await p.Post(A);
            Check(failed.StatusCode == HttpStatusCode.InternalServerError && next.IsSuccessStatusCode && await p.Backend.Taps(A) == "Tap 1", "PRODUCT: backend failure leaked ownership or wrote an extra action.");
        }));
        test("Device writes Acquire AB is atomic and independent B remains usable", () => Contract("W09", () =>
        {
            var gate = Gate.Required(); using var b = gate.Acquire([B]);
            Check(Reject(() => gate.Acquire([A, B])), "Acquire AB succeeded while B was held.");
            using var a = gate.Acquire([A]); Check(gate.Busy(A) && gate.Busy(B), "Failed AB retained A or independent acquisition was lost.");
        }));
        test("Device writes global barrier covers future keys and conflicts in both directions", () => Contract("W10", () =>
        {
            var gate = Gate.Required();
            using (gate.All()) Check(Reject(() => gate.Acquire(["future-C"])), "AcquireAll did not cover a previously unseen serial.");
            using (gate.Acquire(["future-C"])) Check(Reject(() => gate.All()), "AcquireAll ignored an active device lease.");
            using var after = gate.All(); Check(gate.Busy(A) && gate.Busy("future-D"), "Global barrier IsBusy excluded future keys.");
        }));
        test("Device writes old lease double dispose cannot clear a newer same-target lease", () => Contract("W11", () =>
        {
            var gate = Gate.Required(); var old = gate.Acquire([A]); old.Dispose();
            using var current = gate.Acquire([A]); old.Dispose();
            Check(gate.Busy(A) && Reject(() => gate.Acquire([A])), "Old lease disposal cleared the current owner.");
            using var b = gate.Acquire([B]); Check(gate.Busy(B), "A lease blocked unrelated B.");
        }));
        test("Device writes Runtime factory shares one gate across Scheduler Input and API", async () =>
        {
            if (unresolved is not null) throw new InvalidOperationException("NOT-RUN: " + unresolved);
            var runtime = await MatrixRuntime.CreateAsync(new(), Temp(), true); IDisposable? lease = null; var tasks = new List<Task>();
            try
            {
                var property = typeof(MatrixRuntime).GetProperty("Writes") ?? throw new ContractMissing("Runtime.Writes");
                var gate = new Gate(property.GetValue(runtime) ?? throw new ContractMissing("Runtime.Writes value")); lease = gate.Acquire([A]);
                var info = runtime.Devices.GetRequired(A);
                ScriptJobHandle? accepted = null;
                var scriptRejected = Reject(() => { accepted = runtime.Scripts.SubmitJob(new(A, info.Generation, info.TransportId, true), Package()); tasks.Add(accepted.Completion); });
                var inputRejected = await RejectAsync(() => { var task = runtime.Input.SendAsync(new() { SourceSerial = A, Kind = InputKind.Tap, X = 20, Y = 30, SourceWidth = info.Width, SourceHeight = info.Height }); tasks.Add(task); return task; });
                await runtime.Api.StartAutomatorAsync(Port()); using var http = new HttpClient { Timeout = Step };
                var url = "http://127.0.0.1:" + runtime.Api.RunningAutomatorPort;
                using var response = await http.PostAsJsonAsync(url + "/input/tap", new { serial = A, x = 20, y = 30 });
                using var other = await http.PostAsJsonAsync(url + "/input/tap", new { serial = B, x = 20, y = 30 });
                Check(scriptRejected && inputRejected && response.StatusCode == HttpStatusCode.Conflict && other.IsSuccessStatusCode, "Runtime factory failed to inject one shared gate into all writers.");
                Check((await runtime.Backend.DumpAsync(A)).ChildNodeList[1].Text == "Tap 0" && (await runtime.Backend.DumpAsync(B)).ChildNodeList[1].Text == "Tap 1", "Runtime gate rejection still changed A or blocked B.");
            }
            finally
            {
                lease?.Dispose();
                var drain = Task.WhenAll(tasks);
                try { await drain.WaitAsync(Drain); }
                catch (Exception error)
                {
                    if (tasks.Any(task => !task.IsCompleted))
                    {
                        unresolved = "W12 original tasks did not drain; runtime disposal not started: " + error;
                        retained.Add(new { runtime, drain, tasks });
                        Console.WriteLine("DEVICE_WRITE_CASE " + JsonSerializer.Serialize(new { name = "W12", cleanupStatus = "NOT-STARTED-PENDING-TASKS", pending = tasks.Count(task => !task.IsCompleted), error = error.ToString() }));
                        throw;
                    }
                }
                var cleanup = runtime.DisposeAsync().AsTask();
                try { await cleanup.WaitAsync(Drain); }
                catch (Exception error) { unresolved = "W12 runtime cleanup failed: " + error; retained.Add(new { runtime, cleanup, tasks }); throw; }
                finally { Console.WriteLine("DEVICE_WRITE_CASE " + JsonSerializer.Serialize(new { name = "W12", cleanupStatus = cleanup.Status.ToString(), pending = tasks.Count(task => !task.IsCompleted) + (cleanup.IsCompleted ? 0 : 1) })); }
            }
        });
        test("Device writes busy Up invalidates its old gesture before a later Up", () => Run("W13", async p =>
        {
            await p.Input.HandoffAsync(A); await p.Track(p.Input.SendAsync(p.Snapshot(InputKind.Down, [])));
            p.Backend.Hold("legacy", A); var api = p.Post(A); await p.Backend.Entered.Task.WaitAsync(Step);
            var rejected = await RejectAsync(() => p.Track(p.Input.SendAsync(p.Snapshot(InputKind.Up, []))));
            Check(rejected && p.Backend.Count("legacy", A) == 1, "PRODUCT: busy Up reached the backend instead of rejecting the old gesture.");
            p.Backend.Release.TrySetResult(); await api;
            await p.Track(p.Input.SendAsync(p.Snapshot(InputKind.Up, [])));
            Check(p.Backend.Count("legacy", A) == 1 && await p.Backend.Taps(A) == "Tap 1", "PRODUCT: later Up replayed a gesture invalidated by busy rejection.");
        }));
        test("Device writes blocked A terminal observer does not prevent B completion", () => Run("W14", async p =>
        {
            _ = Gate.Required();
            var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            ScriptJobHandle? first = null, second = null;
            Task? observerWait = null; Exception? primary = null;
            void BlockA(ScriptJobStatus status)
            {
                if (status.Target.Serial != A || status.State != ScriptRunState.Completed) return;
                observerWait = release.Task.WaitAsync(TimeSpan.FromSeconds(20));
                entered.TrySetResult();
                observerWait.GetAwaiter().GetResult();
            }
            p.Scheduler.JobChanged += BlockA;
            try
            {
                first = p.Submit(A); await entered.Task.WaitAsync(Step);
                second = p.Submit(B); await p.Backend.BDrained.Task.WaitAsync(Step);
                try { await second.Completion.WaitAsync(Step); }
                catch (TimeoutException error) { throw new InvalidOperationException("PRODUCT: B completion waited for another serial's blocked terminal observer.", error); }
                p.Observe(new { aJob = first.JobId, bJob = second.JobId, aPending = !first.Completion.IsCompleted, bCompleted = second.Completion.IsCompletedSuccessfully, aObserverReleased = release.Task.IsCompleted, observerWait = observerWait?.Status.ToString(), b = await p.Backend.Taps(B) });
                Check(!release.Task.IsCompleted && observerWait is { IsCompleted: false } && !first.Completion.IsCompleted && second.Completion.IsCompletedSuccessfully && p.Scheduler.GetJob(second.JobId)!.State == ScriptRunState.Completed && await p.Backend.Taps(B) == "Tap 1", "PRODUCT: B did not complete its real Demo action independently of A's blocked terminal observer.");
                using var next = await p.Post(B);
                Check(next.IsSuccessStatusCode && !release.Task.IsCompleted && observerWait is { IsCompleted: false } && !first.Completion.IsCompleted && p.Backend.Count("legacy", B) == 1 && await p.Backend.Taps(B) == "Tap 2", "PRODUCT: B completion did not release its lease while A's terminal observer remained blocked.");
            }
            catch (Exception error) { primary = error; throw; }
            finally
            {
                release.TrySetResult();
                p.Scheduler.JobChanged -= BlockA;
                if (observerWait is not null) p.CleanupTasks.Add(observerWait);
                var accepted = new[] { first?.Completion, second?.Completion }.OfType<Task>().ToArray();
                var drain = p.Track(Task.WhenAll(observerWait is null ? accepted : accepted.Append(observerWait)));
                try { await drain.WaitAsync(Drain); }
                catch (Exception cleanup) when (primary is not null) { throw new AggregateException(primary, cleanup); }
            }
        }));
        test("Device writes FIFO queued A waits for the entire earlier AB group", () => Run("W15", async p =>
        {
            _ = Gate.Required(); await p.Input.HandoffAsync(A); p.Backend.Hold("legacy", B);
            Exception? primary = null;
            try
            {
                var first = p.Track(p.Input.SendAsync(p.Snapshot(InputKind.Tap, [B])));
                await p.Backend.Entered.Task.WaitAsync(Step); await p.Backend.ADrained.Task.WaitAsync(Step);
                Check(await p.Backend.Taps(A) == "Tap 1" && await p.Backend.Taps(B) == "Tap 0", "Own first-group A completion/B hold precondition was not reached.");
                var before = p.Backend.Calls.ToArray();
                var second = p.Track(p.Input.SendAsync(p.Snapshot(InputKind.Tap, [])));
                var dispatched = p.Track(p.Input.SendAsync(p.Snapshot(InputKind.Move, []) with { IncludeSource = false }));
                await dispatched.WaitAsync(Step);
                p.Observe(new { phase = "second-group-enqueued", first = first.Status.ToString(), second = second.Status.ToString(), secondError = second.Exception?.ToString(), calls = p.Backend.Calls.ToArray(), a = await p.Backend.Taps(A), b = await p.Backend.Taps(B) });
                Check(!first.IsCompleted && !second.IsCompleted && p.Backend.Count("legacy", A) == 1 && p.Backend.Count("legacy", B) == 1, "PRODUCT: queued A self-conflicted or wrote before the earlier AB group drained.");
                var observation = second.WaitAsync(Step); var timedOut = false;
                try { await observation; }
                catch (TimeoutException) { timedOut = true; }
                catch (DeviceException error) when (error.Status == 409 && error.Code == 1003) { throw new InvalidOperationException("PRODUCT: FIFO queued A self-conflicted with its still-running AB predecessor.", error); }
                p.Observe(new { phase = "single-bounded-pending-observation", timedOut, observation = observation.Status.ToString(), originalSecond = second.Status.ToString(), first = first.Status.ToString(), calls = p.Backend.Calls.ToArray() });
                Check(timedOut && !first.IsCompleted && !second.IsCompleted && p.Backend.Calls.Count == 2, "PRODUCT: queued A did not remain pending while its AB predecessor still owned B.");
                p.Backend.Release.TrySetResult(); await Task.WhenAll(first, second).WaitAsync(Step);
                var after = p.Backend.Calls.ToArray(); p.Observe(new { phase = "both-groups-drained", calls = after });
                Check(after.Length == 3 && after.Take(before.Length).SequenceEqual(before) && after[^1].Serial == A && await p.Backend.Taps(A) == "Tap 2" && await p.Backend.Taps(B) == "Tap 1", "PRODUCT: queued A did not execute once after its complete AB predecessor.");
            }
            catch (Exception error) { primary = error; throw; }
            finally
            {
                p.Backend.Release.TrySetResult(); var drain = p.Track(Task.WhenAll(p.Tasks.ToArray()));
                try { await drain.WaitAsync(Drain); }
                catch when (p.Tasks.All(task => task.IsCompleted)) { }
                catch (Exception cleanup) when (primary is not null) { throw new AggregateException(primary, cleanup); }
            }
        }));
        test("Device writes cancelled queued AB cannot release or write through its predecessor", () => Run("W16", async p =>
        {
            _ = Gate.Required(); await p.Input.HandoffAsync(A); p.Backend.Hold("legacy", A);
            using var cancel = new CancellationTokenSource(); Exception? primary = null;
            try
            {
                var first = p.Track(p.Input.SendAsync(p.Snapshot(InputKind.Tap, [B])));
                await p.Backend.Entered.Task.WaitAsync(Step); await p.Backend.BDrained.Task.WaitAsync(Step);
                var before = p.Backend.Calls.ToArray();
                var second = p.Track(p.Input.SendAsync(p.Snapshot(InputKind.Tap, [B]), cancel.Token));
                var dispatched = p.Track(p.Input.SendAsync(p.Snapshot(InputKind.Move, []) with { IncludeSource = false }));
                await dispatched.WaitAsync(Step); cancel.Cancel();
                using var denied = await p.Post(B);
                p.Observe(new { phase = "queued-group-cancelled", first = first.Status.ToString(), second = second.Status.ToString(), response = (int)denied.StatusCode, calls = p.Backend.Calls.ToArray(), a = await p.Backend.Taps(A), b = await p.Backend.Taps(B) });
                Check(!first.IsCompleted && denied.StatusCode == HttpStatusCode.Conflict && p.Backend.Calls.Count == 2 && await p.Backend.Taps(A) == "Tap 0" && await p.Backend.Taps(B) == "Tap 1", "PRODUCT: cancelling queued AB released its predecessor's lease or sent a later-group action.");
                p.Backend.Release.TrySetResult(); await first.WaitAsync(Step);
                var cancelled = false; try { await second.WaitAsync(Step); } catch (OperationCanceledException) { cancelled = true; }
                Check(cancelled && p.Backend.Calls.Count == 2 && p.Backend.Calls.ToArray().SequenceEqual(before), "PRODUCT: cancelled queued AB wrote an action or failed to drain as cancelled.");
                var next = p.Track(p.Input.SendAsync(p.Snapshot(InputKind.Tap, [B]))); await next.WaitAsync(Step);
                p.Observe(new { phase = "successor-drained", calls = p.Backend.Calls.ToArray() });
                Check(p.Backend.Count("legacy", A) == 2 && p.Backend.Count("legacy", B) == 2 && await p.Backend.Taps(A) == "Tap 2" && await p.Backend.Taps(B) == "Tap 2", "PRODUCT: queued cancellation leaked a group lease or affected its successor.");
            }
            catch (Exception error) { primary = error; throw; }
            finally
            {
                cancel.Cancel(); p.Backend.Release.TrySetResult(); var drain = p.Track(Task.WhenAll(p.Tasks.ToArray()));
                try { await drain.WaitAsync(Drain); }
                catch when (p.Tasks.All(task => task.IsCompleted)) { }
                catch (Exception cleanup) when (primary is not null) { throw new AggregateException(primary, cleanup); }
            }
        }));
    }
    private static ScriptPackage Package() => new() { Manifest = new() { Id = "write-test", Name = "own Demo write", Version = "1" }, Steps = [new() { Action = "click", Value = "20,30" }] };
    private static string Temp() { var path = Path.Combine(Path.GetTempPath(), "matrix-write-tests", Guid.NewGuid().ToString("N")); Directory.CreateDirectory(path); return path; }
    private static int Port() { var listener = new TcpListener(IPAddress.Loopback, 0); listener.Start(); var port = ((IPEndPoint)listener.LocalEndpoint).Port; listener.Stop(); return port; }
    private static void Check(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    private static bool Reject(Action action) { try { action(); return false; } catch (DeviceException error) when (error.Status == 409 && error.Code == 1003) { return true; } }
    private static async Task<bool> RejectAsync(Func<Task> action) { try { await action().WaitAsync(Step); return false; } catch (DeviceException error) when (error.Status == 409 && error.Code == 1003) { return true; } }
    private sealed class ContractMissing(string member) : Exception("CONTRACT-NOT-IMPLEMENTED: public " + member + " is absent; this is not a reproduced backend behavior.");
    private static Task Contract(string name, Action action)
    {
        if (unresolved is not null) throw new InvalidOperationException("NOT-RUN: " + unresolved);
        try { action(); Console.WriteLine("DEVICE_WRITE_CASE " + JsonSerializer.Serialize(new { name, classification = "CONTRACT-PASS", pending = 0 })); return Task.CompletedTask; }
        catch (Exception error) { Console.WriteLine("DEVICE_WRITE_CASE " + JsonSerializer.Serialize(new { name, classification = error is ContractMissing ? "CONTRACT-NOT-IMPLEMENTED" : "PRODUCT-FAIL", error = error.ToString(), pending = 0 })); throw; }
    }
    private static async Task Run(string name, Func<Probe, Task> action)
    {
        if (unresolved is not null) throw new InvalidOperationException("NOT-RUN: " + unresolved);
        var probe = await Probe.Create(); Exception? primary = null, cleanup = null;
        try { await action(probe); } catch (Exception error) { primary = error; }
        try { await probe.DisposeAsync(); } catch (Exception error) { cleanup = error; unresolved = name + " cleanup was not terminal: " + error; retained.Add(probe); }
        Console.WriteLine("DEVICE_WRITE_CASE " + JsonSerializer.Serialize(new { name, gatePresent = probe.GatePresent, observations = probe.Observations, calls = probe.Backend.Calls.ToArray(), tasks = probe.Tasks.Select(task => task.Status.ToString()), backendTasks = probe.Backend.Tasks.Select(task => task.Status.ToString()), cleanupTasks = probe.CleanupTasks.Select(task => task.Status.ToString()), cleanupError = cleanup?.ToString(), primaryError = primary?.ToString(), pending = probe.Tasks.Concat(probe.Backend.Tasks).Concat(probe.CleanupTasks).Distinct().Count(task => !task.IsCompleted) }));
        if (primary is not null && cleanup is not null) throw new AggregateException(primary, cleanup);
        if (primary is not null) ExceptionDispatchInfo.Capture(primary).Throw();
        if (cleanup is not null) ExceptionDispatchInfo.Capture(cleanup).Throw();
    }
    private sealed class Gate(object value)
    {
        internal object Value => value;
        internal static Gate? Optional() { var type = typeof(IDeviceBackend).Assembly.GetType("MatrixDevice.Core.DeviceWriteCoordinator"); return type is null ? null : new(Activator.CreateInstance(type)!); }
        internal static Gate Required() => Optional() ?? throw new ContractMissing("DeviceWriteCoordinator");
        internal IDisposable Acquire(IEnumerable<string> serials) => (IDisposable)Invoke("Acquire", [typeof(IEnumerable<string>)], [serials])!;
        internal IDisposable All() => (IDisposable)Invoke("AcquireAll", [], [])!;
        internal bool Busy(string serial) => (bool)Invoke("IsBusy", [typeof(string)], [serial])!;
        private object? Invoke(string name, Type[] signature, object?[] arguments)
        {
            var method = value.GetType().GetMethod(name, signature) ?? throw new ContractMissing(name);
            try { return method.Invoke(value, arguments); } catch (TargetInvocationException error) when (error.InnerException is not null) { ExceptionDispatchInfo.Capture(error.InnerException).Throw(); throw; }
        }
        internal static T Construct<T>(Gate? gate, params object[] original)
        {
            if (gate is null) return (T)Activator.CreateInstance(typeof(T), original)!;
            var constructor = typeof(T).GetConstructors().SingleOrDefault(item => item.GetParameters().Length == original.Length + 1 && item.GetParameters()[^1].ParameterType == gate.Value.GetType()) ?? throw new ContractMissing(typeof(T).Name + " shared gate constructor");
            return (T)constructor.Invoke(original.Append(gate.Value).ToArray());
        }
    }
    private sealed class Probe : IAsyncDisposable
    {
        internal readonly BlockingBackend Backend = new();
        internal DeviceCatalog Catalog = null!;
        internal ScriptScheduler Scheduler = null!;
        internal InputCoordinator Input = null!;
        internal ApiServiceManager Api = null!;
        internal RollingLog Log = null!;
        internal readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(10) };
        internal readonly List<Task> Tasks = [];
        internal readonly List<Task> CleanupTasks = [];
        internal readonly List<object> Observations = [];
        internal bool GatePresent;
        internal string Url = "";
        internal static async Task<Probe> Create()
        {
            var p = new Probe();
            try
            {
                var gate = Gate.Optional(); p.GatePresent = gate is not null;
                p.Catalog = new(p.Backend); await p.Catalog.RefreshAsync();
                p.Scheduler = Gate.Construct<ScriptScheduler>(gate, p.Backend, p.Catalog, true);
                p.Input = Gate.Construct<InputCoordinator>(gate, p.Catalog, p.Backend);
                p.Log = new(Temp()); p.Api = Gate.Construct<ApiServiceManager>(gate, p.Catalog, p.Backend, new AutomationService(p.Backend, p.Catalog), p.Log);
                p.Api.ScriptWriteBusy = serial => p.Scheduler.GetStatus(serial).State is ScriptRunState.Running or ScriptRunState.Cancelling;
                p.Api.AnyScriptWriteBusy = () => p.Catalog.Snapshot.Any(device => p.Api.ScriptWriteBusy(device.Serial));
                await p.Api.StartAutomatorAsync(Port()); p.Url = "http://127.0.0.1:" + p.Api.RunningAutomatorPort; return p;
            }
            catch (Exception primary)
            {
                try { await p.DisposeAsync(); } catch (Exception cleanup) { unresolved = "Probe construction cleanup failed: " + cleanup; retained.Add(p); throw new AggregateException(primary, cleanup); }
                throw;
            }
        }
        internal T Track<T>(T task) where T : Task { Tasks.Add(task); return task; }
        internal Task<HttpResponseMessage> Post(string serial, CancellationToken ct = default) => Track(Http.PostAsJsonAsync(Url + "/input/tap", new { serial, x = 20, y = 30 }, ct));
        internal ScriptJobHandle Submit(string serial) { var info = Catalog.GetRequired(serial); var handle = Scheduler.SubmitJob(new(serial, info.Generation, info.TransportId, true), Package()); Track(handle.Completion); return handle; }
        internal InputSnapshot Snapshot(InputKind kind, string[] targets) { var info = Catalog.GetRequired(A); return new() { SourceSerial = A, SourceGeneration = info.Generation, Generation = Input.CurrentGeneration, Kind = kind, TargetSerials = targets, SourceWidth = info.Width, SourceHeight = info.Height, X = 20, Y = 30, GestureId = 42 }; }
        internal void Observe(object observation) => Observations.Add(observation);
        public async ValueTask DisposeAsync()
        {
            Backend.Release.TrySetResult();
            var requests = Task.WhenAll(Tasks); CleanupTasks.Add(requests);
            try { await requests.WaitAsync(Drain); } catch when (Tasks.All(task => task.IsCompleted)) { }
            var backend = Task.WhenAll(Backend.Tasks); CleanupTasks.Add(backend);
            try { await backend.WaitAsync(Drain); } catch when (Backend.Tasks.All(task => task.IsCompleted)) { }
            var errors = new List<Exception>();
            async Task Close(Func<Task> dispose)
            {
                Task original;
                try { original = dispose(); CleanupTasks.Add(original); }
                catch (Exception error) { errors.Add(error); return; }
                try { await original.WaitAsync(Step); }
                catch (Exception error)
                {
                    if (!original.IsCompleted) throw new InvalidOperationException("CLEANUP-FAIL: original disposal task is still pending; later components will not be released.", error);
                    errors.Add(error);
                }
            }
            if (Api is not null) await Close(() => Api.DisposeAsync().AsTask());
            if (Input is not null) await Close(() => Input.DisposeAsync().AsTask());
            if (Scheduler is not null) await Close(() => Scheduler.DisposeAsync().AsTask());
            await Close(() => Backend.DisposeAsync().AsTask());
            if (Log is not null) await Close(() => Log.DisposeAsync().AsTask());
            foreach (var response in Tasks.OfType<Task<HttpResponseMessage>>().Where(task => task.IsCompletedSuccessfully)) response.Result.Dispose();
            Http.Dispose(); if (errors.Count > 0) throw new AggregateException("CLEANUP-FAIL", errors);
        }
    }
    private sealed record BackendCall(string Lane, string Serial);
    private sealed class BlockingBackend : IDeviceBackend, IIdentityPinnedDeviceBackend
    {
        private readonly DemoDeviceBackend inner = new();
        private string? holdLane, holdSerial; private int held;
        internal bool ThrowNext;
        internal ConcurrentQueue<BackendCall> Calls { get; } = new();
        internal ConcurrentBag<Task> Tasks { get; } = [];
        internal TaskCompletionSource Entered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal TaskCompletionSource Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal TaskCompletionSource CancelObserved { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal TaskCompletionSource PinnedEntered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal TaskCompletionSource Drained { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal TaskCompletionSource BDrained { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal TaskCompletionSource ADrained { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal void Hold(string lane, string serial) { holdLane = lane; holdSerial = serial; }
        internal int Count(string lane, string serial) => Calls.Count(call => call.Lane == lane && call.Serial == serial);
        internal async Task<string> Taps(string serial) => (await inner.DumpAsync(serial)).ChildNodeList[1].Text!;
        private Task Tap(string lane, string serial, Func<CancellationToken, Task> actual, CancellationToken ct)
        {
            var task = Core(); Tasks.Add(task); return task;
            async Task Core()
            {
                Calls.Enqueue(new(lane, serial)); if (lane == "pinned") PinnedEntered.TrySetResult();
                if (ThrowNext) { ThrowNext = false; throw new IOException("own Demo injected backend failure"); }
                var block = lane == holdLane && serial == holdSerial && Interlocked.CompareExchange(ref held, 1, 0) == 0;
                try
                {
                    if (block) { using var registration = ct.Register(() => CancelObserved.TrySetResult()); Entered.TrySetResult(); await Release.Task; }
                    await actual(block ? CancellationToken.None : ct); if (serial == B) BDrained.TrySetResult();
                    if (serial == A) ADrained.TrySetResult();
                }
                finally { if (block) Drained.TrySetResult(); }
            }
        }
        public IIdentityPinnedDeviceCommands BindIdentity(DeviceCatalog catalog) => new Pinned(this, ((IIdentityPinnedDeviceBackend)inner).BindIdentity(catalog));
        private sealed class Pinned(BlockingBackend owner, IIdentityPinnedDeviceCommands commands) : IIdentityPinnedDeviceCommands
        {
            public bool IsDemo => commands.IsDemo;
            public Task TapAsync(DeviceCommandTarget target, int x, int y, CancellationToken ct = default) => owner.Tap("pinned", target.Serial, token => commands.TapAsync(target, x, y, token), ct);
            public Task SwipeAsync(DeviceCommandTarget target, int x1, int y1, int x2, int y2, int duration, CancellationToken ct = default) => commands.SwipeAsync(target, x1, y1, x2, y2, duration, ct);
            public Task<string> StartAppAsync(DeviceCommandTarget target, string packageName, CancellationToken ct = default) => commands.StartAppAsync(target, packageName, ct);
        }
        public Task TapAsync(string serial, int x, int y, CancellationToken ct = default) => Tap("legacy", serial, token => inner.TapAsync(serial, x, y, token), ct);
        public Task<IReadOnlyList<DeviceInfo>> DiscoverAsync(CancellationToken ct = default) => inner.DiscoverAsync(ct);
        public Task<AndroidNode> DumpAsync(string serial, CancellationToken ct = default) => inner.DumpAsync(serial, ct);
        public Task<byte[]> ScreenshotAsync(string serial, CancellationToken ct = default) => inner.ScreenshotAsync(serial, ct);
        public Task<string> ShellAsync(string serial, string command, bool useRoot = false, CancellationToken ct = default) => inner.ShellAsync(serial, command, useRoot, ct);
        public Task SwipeAsync(string serial, int x1, int y1, int x2, int y2, int duration, CancellationToken ct = default) => inner.SwipeAsync(serial, x1, y1, x2, y2, duration, ct);
        public Task KeyEventAsync(string serial, int code, CancellationToken ct = default) => inner.KeyEventAsync(serial, code, ct);
        public Task TextAsync(string serial, string text, CancellationToken ct = default) => inner.TextAsync(serial, text, ct);
        public Task<string> StartAppAsync(string serial, string packageName, string? activity = null, CancellationToken ct = default) => inner.StartAppAsync(serial, packageName, activity, ct);
        public Task<string> StopAppAsync(string serial, string packageName, CancellationToken ct = default) => inner.StopAppAsync(serial, packageName, ct);
        public Task<bool> ClearAppAsync(string serial, string packageName, CancellationToken ct = default) => inner.ClearAppAsync(serial, packageName, ct);
        public Task<IReadOnlyList<AppInfo>> ListAppsAsync(string serial, bool showSystem = false, bool needVersion = true, CancellationToken ct = default) => inner.ListAppsAsync(serial, showSystem, needVersion, ct);
        public Task InstallAsync(string serial, string path, CancellationToken ct = default) => inner.InstallAsync(serial, path, ct);
        public Task UninstallAsync(string serial, string packageName, CancellationToken ct = default) => inner.UninstallAsync(serial, packageName, ct);
        public Task PushAsync(string serial, string path, string remote, IProgress<double>? progress = null, CancellationToken ct = default) => inner.PushAsync(serial, path, remote, progress, ct);
        public Task PullAsync(string serial, string remote, string path, IProgress<double>? progress = null, CancellationToken ct = default) => inner.PullAsync(serial, remote, path, progress, ct);
        public Task SetNodeTextAsync(string serial, NodeSelector selector, string text, CancellationToken ct = default) => inner.SetNodeTextAsync(serial, selector, text, ct);
        public ValueTask DisposeAsync() => inner.DisposeAsync();
    }
}
