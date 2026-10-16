using System.Text.Json;
using MatrixDevice.Core;

namespace MatrixDevice.Features;

public enum ScriptRunState { Idle, Running, Cancelling, Completed, Cancelled, Failed }
public sealed record ScriptLogEntry(DateTimeOffset Timestamp, string Message);
public sealed record ScriptRunStatus(string Serial, ScriptRunState State, string? PackageId = null,
    string? Error = null, DateTimeOffset? StartedUtc = null, DateTimeOffset? EndedUtc = null);

public sealed class ScriptScheduler(IDeviceBackend backend) : IAsyncDisposable
{
    private sealed class Run(CancellationTokenSource cancellation)
    {
        public CancellationTokenSource Cancellation { get; } = cancellation;
        public Task Task { get; set; } = Task.CompletedTask;
        public TaskCompletionSource Activation { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    }
    private readonly object gate = new();
    private readonly Queue<ScriptRunStatus> notifications = new();
    private int publishing;
    private readonly Dictionary<string, Run> runs = new(StringComparer.Ordinal);
    private readonly Dictionary<string, ScriptRunStatus> statuses = new(StringComparer.Ordinal);
    private readonly Dictionary<string, List<ScriptLogEntry>> logs = new(StringComparer.Ordinal);
    private bool disposed;
    public event Action<ScriptRunStatus>? StatusChanged;
    public event Action<string, ScriptLogEntry>? EntryAdded;

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
                var run = new Run(CancellationTokenSource.CreateLinkedTokenSource(ct));
                logs[serial] = [];
                started = new ScriptRunStatus(serial, ScriptRunState.Running, snapshot.Manifest.Id, StartedUtc: DateTimeOffset.UtcNow);
                statuses[serial] = started;
                notifications.Enqueue(started);
                runs[serial] = run;
                launched = run;
                task = run.Task = Task.Run(() => ExecuteAsync(serial, snapshot, run));
            }
        }
        if (started is null) Append(serial, "脚本正在运行，未重复启动。");
        else
        {
            PublishStatuses();
            Append(serial, $"开始运行：{snapshot.Manifest.Name} ({snapshot.Manifest.Id})。");
            launched!.Activation.TrySetResult();
        }
        return task;
    }

    public bool Stop(string serial)
    {
        ScriptRunStatus? status;
        lock (gate)
        {
            if (!runs.TryGetValue(serial, out var run)) return false;
            status = statuses[serial] with { State = ScriptRunState.Cancelling };
            statuses[serial] = status;
            notifications.Enqueue(status);
            run.Cancellation.Cancel();
        }
        PublishStatuses();
        Append(serial, "已请求取消脚本。");
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

    private async Task ExecuteAsync(string serial, ScriptPackage package, Run run)
    {
        await run.Activation.Task.ConfigureAwait(false);
        var state = ScriptRunState.Completed;
        string? failure = null;
        try
        {
            for (var index = 0; index < package.Steps.Count; index++)
            {
                var ct = run.Cancellation.Token;
                ct.ThrowIfCancellationRequested();
                var step = package.Steps[index];
                var actionName = step.Action.Trim().ToLowerInvariant() switch { "launch" or "launchapp" => "启动应用", "wait" or "sleep" => "等待", "click" => "点击", "swipe" => "滑动", _ => step.Action };
                Append(serial, $"步骤 {index + 1}/{package.Steps.Count}：{actionName} {step.Value}");
                switch (step.Action.Trim().ToLowerInvariant())
                {
                    case "launch": case "launchapp":
                        await backend.StartAppAsync(serial, step.Value!, ct: ct).ConfigureAwait(false);
                        break;
                    case "wait": case "sleep":
                        await Task.Delay(step.DurationMs, ct).ConfigureAwait(false);
                        break;
                    case "click":
                        var point = ScriptPackageService.ParseIntegers(step.Value, 2, index);
                        await backend.TapAsync(serial, point[0], point[1], ct).ConfigureAwait(false);
                        break;
                    case "swipe":
                        var swipe = ScriptPackageService.ParseIntegers(step.Value, 5, index);
                        await backend.SwipeAsync(serial, swipe[0], swipe[1], swipe[2], swipe[3], swipe[4], ct).ConfigureAwait(false);
                        break;
                }
            }
            run.Cancellation.Token.ThrowIfCancellationRequested();
        }
        catch (OperationCanceledException) when (run.Cancellation.IsCancellationRequested)
        {
            state = ScriptRunState.Cancelled;
            Append(serial, "脚本已取消。");
        }
        catch (Exception error)
        {
            state = ScriptRunState.Failed;
            failure = error.Message;
            Append(serial, $"执行失败：{error.Message}");
            throw;
        }
        finally
        {
            ScriptRunStatus ended;
            var stateName = state switch { ScriptRunState.Completed => "已完成", ScriptRunState.Cancelled => "已取消", ScriptRunState.Failed => "失败", _ => "已结束" };
            Append(serial, $"运行结束：{stateName}。");
            lock (gate)
            {
                ended = statuses[serial] with { State = state, Error = failure, EndedUtc = DateTimeOffset.UtcNow };
                if (runs.TryGetValue(serial, out var current) && ReferenceEquals(current, run))
                {
                    runs.Remove(serial);
                    statuses[serial] = ended;
                    notifications.Enqueue(ended);
                }
                run.Cancellation.Dispose();
            }
            PublishStatuses();
        }
    }

    private void Append(string serial, string message)
    {
        var entry = new ScriptLogEntry(DateTimeOffset.UtcNow, message);
        lock (gate)
        {
            if (!logs.TryGetValue(serial, out var entries)) logs[serial] = entries = [];
            entries.Add(entry);
            if (entries.Count > 500) entries.RemoveRange(0, entries.Count - 500);
        }
        if (EntryAdded is { } added)
            foreach (Action<string, ScriptLogEntry> handler in added.GetInvocationList())
                try { handler(serial, entry); } catch (Exception error) { RecordObserverFailure(serial, error); }
    }

    private void PublishStatuses()
    {
        while (true)
        {
            if (Interlocked.CompareExchange(ref publishing, 1, 0) != 0) return;
            try
            {
                while (true)
                {
                    ScriptRunStatus status;
                    lock (gate)
                    {
                        if (notifications.Count == 0) break;
                        status = notifications.Dequeue();
                    }
                    if (StatusChanged is { } changed)
                        foreach (Action<ScriptRunStatus> handler in changed.GetInvocationList())
                            try { handler(status); } catch (Exception error) { RecordObserverFailure(status.Serial, error); }
                }
            }
            finally { Volatile.Write(ref publishing, 0); }
            lock (gate) if (notifications.Count == 0) return;
        }
    }

    private void RecordObserverFailure(string serial, Exception error)
    {
        lock (gate)
        {
            if (!logs.TryGetValue(serial, out var entries)) logs[serial] = entries = [];
            entries.Add(new(DateTimeOffset.UtcNow, $"状态监听失败：{error.Message}"));
            if (entries.Count > 500) entries.RemoveAt(0);
        }
    }

    public async ValueTask DisposeAsync()
    {
        Task[] pending;
        lock (gate)
        {
            if (disposed) return;
            disposed = true;
            foreach (var run in runs.Values) run.Cancellation.Cancel();
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
