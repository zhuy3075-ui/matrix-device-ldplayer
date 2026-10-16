using System.Text;
using System.Text.Json;
using MatrixDevice.Core;

namespace MatrixDevice.Features;

public enum ScriptRunState { Idle, Running, Cancelling, Completed, Cancelled, Failed }
public sealed record ScriptLogEntry(DateTimeOffset Timestamp, string Message);
public sealed record ScriptRunStatus(string Serial, ScriptRunState State, string? PackageId = null,
    string? Error = null, DateTimeOffset? StartedUtc = null, DateTimeOffset? EndedUtc = null);

public sealed class ScriptScheduler : IAsyncDisposable
{
    public const int MaximumTerminalJobs = 100;
    public const int MaximumJobLogEntries = 500;
    public const int MaximumResultBytes = 16 * 1024;
    private readonly IDeviceBackend backend;
    private readonly DeviceCatalog? catalog;
    private readonly IIdentityPinnedDeviceCommands? identityCommands;
    private readonly DeviceWriteCoordinator? writes;
    private readonly bool isDemo;
    private sealed class Run(CancellationTokenSource cancellation, Guid? jobId = null, ScriptJobTarget? target = null, IDisposable? writeLease = null)
    {
        public CancellationTokenSource Cancellation { get; } = cancellation;
        public Task Task { get; set; } = Task.CompletedTask;
        public TaskCompletionSource Activation { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public Guid? JobId { get; } = jobId;
        public ScriptJobTarget? Target { get; } = target;
        public IDisposable? WriteLease { get; } = writeLease;
        public bool Finishing { get; set; }
    }
    private readonly object gate = new();
    private readonly Queue<(ScriptRunStatus? Status, ScriptJobStatus? Job)> notifications = new();
    private int publishing;
    private readonly Dictionary<string, Run> runs = new(StringComparer.Ordinal);
    private readonly Dictionary<string, ScriptRunStatus> statuses = new(StringComparer.Ordinal);
    private readonly Dictionary<string, List<ScriptLogEntry>> logs = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Guid?> latestJobProjection = new(StringComparer.Ordinal);
    private readonly Dictionary<Guid, ScriptJobStatus> jobs = new();
    private readonly Dictionary<Guid, List<ScriptLogEntry>> jobLogs = new();
    private readonly Queue<Guid> completedJobs = new();
    private bool disposed;
    public event Action<ScriptRunStatus>? StatusChanged;
    public event Action<string, ScriptLogEntry>? EntryAdded;
    public event Action<ScriptJobStatus>? JobChanged;
    public event Action<Guid, ScriptLogEntry>? JobLogAdded;

    public ScriptScheduler(IDeviceBackend backend, DeviceWriteCoordinator? writes = null)
    {
        ArgumentNullException.ThrowIfNull(backend);
        this.backend = backend;
        this.writes = writes;
    }

    public ScriptScheduler(IDeviceBackend backend, DeviceCatalog catalog, bool isDemo, DeviceWriteCoordinator? writes = null) : this(backend, writes)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        this.catalog = catalog;
        this.isDemo = isDemo;
        identityCommands = (backend as IIdentityPinnedDeviceBackend)?.BindIdentity(catalog)
            ?? throw new DeviceException("后端未提供绑定目录身份的任务执行能力。", 501, 2001);
        if (identityCommands.IsDemo != isDemo) throw new DeviceException("任务模式与后端身份能力不一致。", 400, 1007);
    }

    public ScriptJobHandle SubmitJob(ScriptJobTarget target, ScriptPackage package, CancellationToken ct = default,
        IReadOnlyDictionary<string, JsonElement>? parameterOverrides = null)
    {
        ct.ThrowIfCancellationRequested();
        var snapshot = ScriptExecutionCompiler.Compile(package, parameterOverrides);
        ArgumentNullException.ThrowIfNull(target);
        var frozen = target with { };
        RequireJobTarget(frozen);
        ScriptJobStatus accepted;
        Run run;
        lock (gate)
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            ct.ThrowIfCancellationRequested();
            RequireJobTarget(frozen);
            if (runs.ContainsKey(frozen.Serial)) throw new DeviceException($"设备 {frozen.Serial} 已有脚本运行，任务未接纳。", 409, 1003);
            var cancellation = CancellationTokenSource.CreateLinkedTokenSource(ct);
            IDisposable? lease = null;
            try
            {
                lease = writes?.Acquire([frozen.Serial]);
                var id = Guid.NewGuid();
                var started = DateTimeOffset.UtcNow;
                accepted = new(id, frozen, snapshot.Manifest.Id, snapshot.Manifest.Version, ScriptRunState.Running, -1, snapshot.Steps.Count, started);
                run = new(cancellation, id, frozen, lease);
                jobs.Add(id, accepted); jobLogs.Add(id, []);
                logs[frozen.Serial] = [];
                latestJobProjection[frozen.Serial] = id;
                var projected = new ScriptRunStatus(frozen.Serial, ScriptRunState.Running, snapshot.Manifest.Id, StartedUtc: started);
                statuses[frozen.Serial] = projected;
                runs.Add(frozen.Serial, run);
                notifications.Enqueue((projected, accepted));
                run.Task = Task.Run(() => ExecuteOwnedAsync(frozen.Serial, snapshot, run));
            }
            catch { cancellation.Dispose(); lease?.Dispose(); throw; }
        }
        try
        {
            PublishStatuses();
            Append(frozen.Serial, $"开始运行：{snapshot.Manifest.Name} ({snapshot.Manifest.Id})。", accepted.JobId);
        }
        finally { run.Activation.TrySetResult(); }
        return new(accepted.JobId, run.Task);
    }

    public IReadOnlyList<ScriptJobStatus> GetJobs()
    {
        lock (gate) return jobs.Values.OrderByDescending(job => job.StartedUtc).ThenBy(job => job.JobId).ToArray();
    }

    public ScriptJobStatus? GetJob(Guid jobId) { lock (gate) return jobs.GetValueOrDefault(jobId); }
    public IReadOnlyList<ScriptLogEntry> GetJobLogs(Guid jobId)
    { lock (gate) return jobLogs.TryGetValue(jobId, out var entries) ? entries.ToArray() : []; }

    public bool CancelJob(Guid jobId)
    {
        string serial;
        lock (gate)
        {
            if (!jobs.TryGetValue(jobId, out var job) || !runs.TryGetValue(job.Target.Serial, out var run) || run.JobId != jobId || run.Finishing) return false;
            serial = job.Target.Serial;
            if (job.State == ScriptRunState.Cancelling) return true;
            var projected = statuses[serial] with { State = ScriptRunState.Cancelling };
            statuses[serial] = projected;
            var updated = job with { State = ScriptRunState.Cancelling };
            jobs[jobId] = updated;
            notifications.Enqueue((projected, updated));
            run.Cancellation.Cancel();
        }
        PublishStatuses();
        Append(serial, "已请求取消脚本。", jobId);
        return true;
    }

    private void RequireJobTarget(ScriptJobTarget target)
    {
        if (catalog is null || identityCommands is null) throw new DeviceException("当前调度器未配置任务身份执行能力。", 501, 2001);
        if (string.IsNullOrWhiteSpace(target.Serial) || target.Generation <= 0 || target.IsDemo != isDemo
            || (isDemo ? target.TransportId is not null : target.TransportId is null or 0))
            throw new DeviceException($"任务目标身份无效：serial={target.Serial}, generation={target.Generation}, transport={target.TransportId}, demo={target.IsDemo}。", 400, 1007);
        var current = catalog.GetRequired(target.Serial);
        if (current.Generation != target.Generation || current.TransportId != target.TransportId)
            throw new DeviceException($"任务目标身份已变化：serial={target.Serial}, expected generation={target.Generation}/transport={target.TransportId}, actual generation={current.Generation}/transport={current.TransportId}。", 409, 1003);
    }

    public Task RunAsync(string serial, ScriptPackage package, CancellationToken ct = default)
        => RunAsync(serial, package, ct, null);

    public Task RunAsync(string serial, ScriptPackage package, CancellationToken ct, IReadOnlyDictionary<string, JsonElement>? parameterOverrides)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serial);
        var snapshot = ScriptExecutionCompiler.Compile(package, parameterOverrides);
        Task task;
        ScriptRunStatus? started = null;
        Run? launched = null;
        lock (gate)
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            if (runs.ContainsKey(serial)) task = Task.CompletedTask;
            else
            {
                var cancellation = CancellationTokenSource.CreateLinkedTokenSource(ct);
                IDisposable? lease = null;
                try
                {
                    lease = writes?.Acquire([serial]);
                    var run = new Run(cancellation, writeLease: lease);
                    logs[serial] = [];
                    latestJobProjection[serial] = null;
                    started = new ScriptRunStatus(serial, ScriptRunState.Running, snapshot.Manifest.Id, StartedUtc: DateTimeOffset.UtcNow);
                    statuses[serial] = started;
                    notifications.Enqueue((started, null));
                    runs[serial] = run;
                    launched = run;
                    task = run.Task = Task.Run(() => ExecuteOwnedAsync(serial, snapshot, run));
                }
                catch { cancellation.Dispose(); lease?.Dispose(); throw; }
            }
        }
        if (started is null) Append(serial, "脚本正在运行，未重复启动。");
        else
        {
            try
            {
                PublishStatuses();
                Append(serial, $"开始运行：{snapshot.Manifest.Name} ({snapshot.Manifest.Id})。");
            }
            finally { launched!.Activation.TrySetResult(); }
        }
        return task;
    }

    public bool Stop(string serial)
    {
        ScriptRunStatus? status;
        Guid? jobId;
        lock (gate)
        {
            if (!runs.TryGetValue(serial, out var run) || run.Finishing) return false;
            jobId = run.JobId;
            status = statuses[serial] with { State = ScriptRunState.Cancelling };
            statuses[serial] = status;
            ScriptJobStatus? updated = null;
            if (run.JobId is { } id)
            {
                updated = jobs[id] with { State = ScriptRunState.Cancelling };
                jobs[id] = updated;
            }
            notifications.Enqueue((status, updated));
            run.Cancellation.Cancel();
        }
        PublishStatuses();
        Append(serial, "已请求取消脚本。", jobId);
        return true;
    }

    public bool OnDeviceDisconnected(string serial) => Stop(serial);
    public IReadOnlyList<ScriptLogEntry> GetLogs(string serial)
    {
        lock (gate) return logs.TryGetValue(serial, out var entries) ? entries.ToArray() : [];
    }
    public ScriptRunStatus GetStatus(string serial)
    {
        lock (gate) return statuses.GetValueOrDefault(serial) ?? new(serial, ScriptRunState.Idle);
    }

    private async Task ExecuteOwnedAsync(string serial, ScriptPackage package, Run run)
    {
        try { await ExecuteAsync(serial, package, run).ConfigureAwait(false); }
        finally
        {
            lock (gate)
            {
                if (runs.TryGetValue(serial, out var current) && ReferenceEquals(current, run)) runs.Remove(serial);
                run.WriteLease?.Dispose();
            }
        }
    }

    private async Task ExecuteAsync(string serial, ScriptPackage package, Run run)
    {
        await run.Activation.Task.ConfigureAwait(false);
        var state = ScriptRunState.Completed;
        string? failure = null;
        var result = run.JobId is null ? null : new JobResultBuffer();
        try
        {
            for (var index = 0; index < package.Steps.Count; index++)
            {
                var ct = run.Cancellation.Token;
                ct.ThrowIfCancellationRequested();
                if (run.Target is { } target) RequireJobTarget(target);
                var step = package.Steps[index];
                if (run.JobId is { } id)
                {
                    lock (gate)
                    {
                        var job = jobs[id] with { StepIndex = index };
                        jobs[id] = job;
                        notifications.Enqueue((null, job));
                    }
                    PublishStatuses();
                }
                var actionName = step.Action.Trim().ToLowerInvariant() switch { "launch" or "launchapp" => "启动应用", "wait" or "sleep" => "等待", "click" => "点击", "swipe" => "滑动", _ => step.Action };
                Append(serial, $"步骤 {index + 1}/{package.Steps.Count}：{actionName} {step.Value}", run.JobId);
                switch (step.Action.Trim().ToLowerInvariant())
                {
                    case "launch": case "launchapp":
                        if (run.Target is { } launchTarget)
                        {
                            var output = await identityCommands!.StartAppAsync(CommandTarget(launchTarget), step.Value!, ct).ConfigureAwait(false);
                            result!.Add(index, output);
                        }
                        else await backend.StartAppAsync(serial, step.Value!, ct: ct).ConfigureAwait(false);
                        break;
                    case "wait": case "sleep":
                        await Task.Delay(step.DurationMs, ct).ConfigureAwait(false);
                        break;
                    case "click":
                        var point = ScriptPackageService.ParseIntegers(step.Value, 2, index);
                        if (run.Target is { } tapTarget) await identityCommands!.TapAsync(CommandTarget(tapTarget), point[0], point[1], ct).ConfigureAwait(false);
                        else await backend.TapAsync(serial, point[0], point[1], ct).ConfigureAwait(false);
                        break;
                    case "swipe":
                        var swipe = ScriptPackageService.ParseIntegers(step.Value, 5, index);
                        if (run.Target is { } swipeTarget) await identityCommands!.SwipeAsync(CommandTarget(swipeTarget), swipe[0], swipe[1], swipe[2], swipe[3], swipe[4], ct).ConfigureAwait(false);
                        else await backend.SwipeAsync(serial, swipe[0], swipe[1], swipe[2], swipe[3], swipe[4], ct).ConfigureAwait(false);
                        break;
                }
            }
            run.Cancellation.Token.ThrowIfCancellationRequested();
            if (run.Target is { } completedTarget) RequireJobTarget(completedTarget);
        }
        catch (OperationCanceledException) when (run.Cancellation.IsCancellationRequested)
        {
            state = ScriptRunState.Cancelled;
            Append(serial, "脚本已取消。", run.JobId);
        }
        catch (Exception error)
        {
            state = ScriptRunState.Failed;
            failure = error.Message;
            Append(serial, $"执行失败：{error.Message}", run.JobId);
            throw;
        }
        finally
        {
            ScriptRunStatus ended;
            lock (gate)
            {
                run.Finishing = true;
                if (run.JobId is not null && state == ScriptRunState.Completed && run.Cancellation.IsCancellationRequested) state = ScriptRunState.Cancelled;
            }
            var stateName = state switch { ScriptRunState.Completed => "已完成", ScriptRunState.Cancelled => "已取消", ScriptRunState.Failed => "失败", _ => "已结束" };
            Append(serial, $"运行结束：{stateName}。", run.JobId);
            lock (gate)
            {
                ended = statuses[serial] with { State = state, Error = failure, EndedUtc = DateTimeOffset.UtcNow };
                if (runs.TryGetValue(serial, out var current) && ReferenceEquals(current, run))
                {
                    statuses[serial] = ended;
                    ScriptJobStatus? completed = null;
                    if (run.JobId is { } id)
                    {
                        completed = jobs[id] with { State = state, Error = failure, EndedUtc = ended.EndedUtc,
                            Result = result!.Text ?? (state == ScriptRunState.Completed ? "已完成" : null) };
                        jobs[id] = completed;
                        completedJobs.Enqueue(id);
                        while (completedJobs.Count > MaximumTerminalJobs)
                        {
                            var expired = completedJobs.Dequeue();
                            jobs.Remove(expired); jobLogs.Remove(expired);
                        }
                    }
                    notifications.Enqueue((ended, completed));
                }
                run.Cancellation.Dispose();
            }
            PublishStatuses();
        }
    }

    private static DeviceCommandTarget CommandTarget(ScriptJobTarget target)
        => new(target.Serial, target.Generation, target.TransportId, target.IsDemo);

    private sealed class JobResultBuffer
    {
        private const string Truncation = "\n[输出已截断]";
        private readonly StringBuilder text = new();
        private int bytes;
        private bool truncated;
        internal string? Text => text.Length == 0 ? null : text.ToString();
        internal void Add(int stepIndex, string output)
        {
            if (truncated) return;
            var value = $"步骤 {stepIndex + 1}：{output}\n";
            if (bytes + Encoding.UTF8.GetByteCount(value) <= MaximumResultBytes)
            {
                text.Append(value); bytes += Encoding.UTF8.GetByteCount(value); return;
            }
            // Keep valid Unicode and reserve space for the explicit truncation marker.
            var budget = MaximumResultBytes - Encoding.UTF8.GetByteCount(Truncation);
            if (bytes > budget)
            {
                value = text.ToString() + value; text.Clear(); bytes = 0;
            }
            foreach (var rune in value.EnumerateRunes())
            {
                if (bytes + rune.Utf8SequenceLength > budget) break;
                text.Append(rune.ToString()); bytes += rune.Utf8SequenceLength;
            }
            text.Append(Truncation); truncated = true;
        }
    }

    private void Append(string serial, string message, Guid? jobId = null)
    {
        var entry = new ScriptLogEntry(DateTimeOffset.UtcNow, message);
        bool project;
        lock (gate)
        {
            project = ProjectsToSerial(serial, jobId);
            if (project)
            {
                if (!logs.TryGetValue(serial, out var entries)) logs[serial] = entries = [];
                entries.Add(entry);
                if (entries.Count > 500) entries.RemoveRange(0, entries.Count - 500);
            }
            if (jobId is { } id && jobLogs.TryGetValue(id, out var owned))
            {
                owned.Add(entry);
                if (owned.Count > MaximumJobLogEntries) owned.RemoveRange(0, owned.Count - MaximumJobLogEntries);
            }
        }
        if (project && EntryAdded is { } added)
            foreach (Action<string, ScriptLogEntry> handler in added.GetInvocationList())
                try { handler(serial, entry); } catch (Exception error) { RecordObserverFailure(serial, error, jobId); }
        if (jobId is { } owner && JobLogAdded is { } jobAdded)
            foreach (Action<Guid, ScriptLogEntry> handler in jobAdded.GetInvocationList())
                try { handler(owner, entry); } catch (Exception error) { RecordObserverFailure(serial, error, jobId); }
    }

    private bool ProjectsToSerial(string serial, Guid? jobId)
        => jobId is null || latestJobProjection.TryGetValue(serial, out var current) && current == jobId;

    private void PublishStatuses()
    {
        while (true)
        {
            if (Interlocked.CompareExchange(ref publishing, 1, 0) != 0) return;
            try
            {
                while (true)
                {
                    (ScriptRunStatus? Status, ScriptJobStatus? Job) notification;
                    lock (gate)
                    {
                        if (notifications.Count == 0) break;
                        notification = notifications.Dequeue();
                    }
                    if (notification.Status is { } status && StatusChanged is { } changed)
                        foreach (Action<ScriptRunStatus> handler in changed.GetInvocationList())
                            try { handler(status); } catch (Exception error) { RecordObserverFailure(status.Serial, error, notification.Job?.JobId); }
                    if (notification.Job is { } job && JobChanged is { } jobChanged)
                        foreach (Action<ScriptJobStatus> handler in jobChanged.GetInvocationList())
                            try { handler(job); } catch (Exception error) { RecordObserverFailure(job.Target.Serial, error, job.JobId); }
                }
            }
            finally { Volatile.Write(ref publishing, 0); }
            lock (gate) if (notifications.Count == 0) return;
        }
    }

    private void RecordObserverFailure(string serial, Exception error, Guid? jobId = null)
    {
        lock (gate)
        {
            var entry = new ScriptLogEntry(DateTimeOffset.UtcNow, $"状态监听失败：{error.Message}");
            if (ProjectsToSerial(serial, jobId))
            {
                if (!logs.TryGetValue(serial, out var entries)) logs[serial] = entries = [];
                entries.Add(entry);
                if (entries.Count > 500) entries.RemoveAt(0);
            }
            if (jobId is { } id && jobLogs.TryGetValue(id, out var owned))
            {
                owned.Add(entry);
                if (owned.Count > MaximumJobLogEntries) owned.RemoveAt(0);
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        Task[] pending;
        lock (gate)
        {
            if (disposed) return;
            disposed = true;
            foreach (var run in runs.Values) if (!run.Finishing) run.Cancellation.Cancel();
            pending = runs.Values.Select(run => run.Task).ToArray();
        }
        try { await Task.WhenAll(pending).ConfigureAwait(false); }
        catch (Exception error)
        {
            // Run failures remain in device status and are rethrown to callers.
            System.Diagnostics.Trace.WriteLine($"Script shutdown observed: {error.Message}");
        }
    }
}
