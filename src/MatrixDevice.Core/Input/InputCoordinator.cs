using System.Collections.Concurrent;
using System.Diagnostics;

namespace MatrixDevice.Core;

public enum InputKind { Down, Move, Up, Cancel, Tap, Swipe, Key, Text }
public sealed record InputSnapshot
{
    public required string SourceSerial { get; init; }
    public InputKind Kind { get; init; }
    public IReadOnlyList<string> TargetSerials { get; init; } = Array.Empty<string>();
    public long Generation { get; init; }
    public long SourceGeneration { get; init; }
    public long GestureId { get; init; }
    public int X { get; init; }
    public int Y { get; init; }
    public int X2 { get; init; }
    public int Y2 { get; init; }
    public int SourceWidth { get; init; }
    public int SourceHeight { get; init; }
    public int DurationMs { get; init; } = 300;
    public int KeyCode { get; init; }
    public string Text { get; init; } = "";
    public bool IncludeSource { get; init; } = true;
}

public sealed class InputCoordinator : IAsyncDisposable
{
    private sealed record Target(string Serial, long Generation, int Width, int Height, int? Rotation);
    private sealed class Work(InputSnapshot snapshot, Target[] targets, CancellationToken cancellation)
    {
        public InputSnapshot Snapshot { get; } = snapshot;
        public Target[] Targets { get; } = targets;
        public CancellationToken Cancellation { get; } = cancellation;
        public List<TaskCompletionSource> Waiters { get; } = [];
    }
    private sealed record Gesture(string Source, long SourceGeneration, long Generation, long Id, long DeviceGeneration, int X, int Y, int LastX, int LastY, long Started);
    private readonly DeviceCatalog _catalog;
    private readonly IDeviceBackend _backend;
    private readonly DeviceWriteCoordinator? _writes;
    private readonly object _gate = new();
    private readonly LinkedList<Work> _pending = new();
    private readonly SemaphoreSlim _signal = new(0);
    private readonly CancellationTokenSource _lifetime = new();
    private readonly SerialCommandQueue _targets = new();
    private readonly ConcurrentDictionary<string, Gesture> _gestures = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<long, Task> _inflight = new();
    private readonly Task _worker;
    private long _generation = 1;
    private long _dispatchId;
    private string? _source;
    private bool _disposed;
    public long CurrentGeneration { get { lock (_gate) return _generation; } }
    public string InputMode => "ADB completed gestures; not real-time touch";
    public event Action<string, Exception>? TargetFailed;

    public InputCoordinator(DeviceCatalog catalog, IDeviceBackend backend, DeviceWriteCoordinator? writes = null)
    {
        _catalog = catalog; _backend = backend; _writes = writes;
        _catalog.DeviceDisconnected += OnDisconnected;
        _worker = Task.Run(WorkerAsync);
    }

    public Task SendAsync(InputSnapshot snapshot, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ct.ThrowIfCancellationRequested();
        var source = _catalog.GetRequired(snapshot.SourceSerial);
        if (snapshot.SourceGeneration != 0 && snapshot.SourceGeneration != source.Generation)
            throw new DeviceException($"Source generation changed: {snapshot.SourceSerial}.");
        var serials = snapshot.TargetSerials.Where(serial => !string.Equals(serial, source.Serial, StringComparison.OrdinalIgnoreCase)).Distinct(StringComparer.Ordinal).ToList();
        if (snapshot.IncludeSource) serials.Insert(0, source.Serial);
        var targets = serials.Select(serial => _catalog.GetRequired(serial)).Select(info => new Target(info.Serial, info.Generation, info.Width, info.Height, info.Rotation)).ToArray();
        var completed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            var resolved = snapshot with { Generation = snapshot.Generation == 0 ? _generation : snapshot.Generation, SourceGeneration = source.Generation, SourceWidth = snapshot.SourceWidth > 0 ? snapshot.SourceWidth : source.Width, SourceHeight = snapshot.SourceHeight > 0 ? snapshot.SourceHeight : source.Height, TargetSerials = serials.ToArray() };
            if (_source is not null && resolved.SourceSerial != _source && resolved.Kind is not (InputKind.Up or InputKind.Cancel))
                throw new DeviceException($"Input source changed: requested={resolved.SourceSerial}, current={_source}.");
            if (resolved.Generation != _generation && resolved.Kind is not (InputKind.Up or InputKind.Cancel))
                throw new DeviceException($"Input generation {resolved.Generation} is stale; current={_generation}.");
            var work = new Work(resolved, targets, ct); work.Waiters.Add(completed);
            if (_pending.Last is { } tail && CanReplaceMove(tail.Value, work))
            {
                work.Waiters.AddRange(tail.Value.Waiters);
                tail.Value = work;
            }
            else { _pending.AddLast(work); _signal.Release(); }
        }
        return completed.Task;
    }

    private static bool CanReplaceMove(Work previous, Work next) => previous.Snapshot.Kind == InputKind.Move && next.Snapshot.Kind == InputKind.Move
        && previous.Snapshot.Generation == next.Snapshot.Generation && previous.Snapshot.GestureId == next.Snapshot.GestureId
        && previous.Snapshot.SourceGeneration == next.Snapshot.SourceGeneration && previous.Snapshot.SourceSerial == next.Snapshot.SourceSerial
        && previous.Targets.SequenceEqual(next.Targets) && previous.Cancellation == next.Cancellation;

    private async Task WorkerAsync()
    {
        try
        {
            while (true)
            {
                await _signal.WaitAsync(_lifetime.Token).ConfigureAwait(false);
                Work work;
                lock (_gate) { work = _pending.First!.Value; _pending.RemoveFirst(); }
                var id = Interlocked.Increment(ref _dispatchId);
                var completion = DispatchAsync(work);
                _inflight[id] = completion;
                _ = RemoveCompletedAsync(id, completion);
            }
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested) { }
        finally
        {
            lock (_gate) { foreach (var work in _pending) foreach (var waiter in work.Waiters) waiter.TrySetCanceled(); _pending.Clear(); }
        }
    }
    private async Task RemoveCompletedAsync(long id, Task task) { await task.ConfigureAwait(false); _inflight.TryRemove(id, out _); }
    private async Task DispatchAsync(Work work)
    {
        try
        {
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(work.Cancellation, _lifetime.Token);
            linked.Token.ThrowIfCancellationRequested();
            var ready = _writes is not null && work.Targets.Length != 0 && work.Snapshot.Kind is
                (InputKind.Up or InputKind.Tap or InputKind.Swipe or InputKind.Key or InputKind.Text)
                ? new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously) : null;
            var finished = ready is null ? null : new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var remaining = work.Targets.Length;
            var unfinished = work.Targets.Length;
            IDisposable? lease = null;
            void FinishTarget()
            {
                if (finished is null || Interlocked.Decrement(ref unfinished) != 0) return;
                try { lease?.Dispose(); finished.TrySetResult(); }
                catch (Exception error) { finished.TrySetException(error); }
            }
            async Task QueueTargetAsync(Target target)
            {
                var entered = false;
                try
                {
                    await _targets.RunAsync(target.Serial, async token =>
                    {
                        entered = true;
                        try
                        {
                            if (ready is not null)
                            {
                                // The single worker enqueues a complete group before the next work.
                                // All FIFO lanes arrive before taking Writes; no lane lock or input lock is held.
                                if (Interlocked.Decrement(ref remaining) == 0)
                                {
                                    try
                                    {
                                        Target[] targets = work.Snapshot.Generation != CurrentGeneration ? [] : work.Targets.Where(item =>
                                            work.Snapshot.Kind != InputKind.Up || _gestures.TryGetValue(item.Serial, out var gesture) && Matches(gesture, work.Snapshot, item)).ToArray();
                                        if (targets.Length != 0) lease = _writes!.Acquire(targets.Select(item => item.Serial));
                                        ready.TrySetResult();
                                    }
                                    catch (Exception error)
                                    {
                                        if (work.Snapshot.Kind == InputKind.Up && error is DeviceException { Status: 409, Code: 1003 })
                                            foreach (var item in work.Targets)
                                                if (_gestures.TryGetValue(item.Serial, out var gesture) && Matches(gesture, work.Snapshot, item))
                                                    _gestures.TryRemove(new KeyValuePair<string, Gesture>(item.Serial, gesture));
                                        ready.TrySetException(error);
                                    }
                                }
                                await ready.Task.WaitAsync(token).ConfigureAwait(false);
                            }
                            await SendTargetAsync(work.Snapshot, target, token).ConfigureAwait(false);
                        }
                        catch (Exception error) { TargetFailed?.Invoke(target.Serial, error); throw; }
                        finally
                        {
                            // Keep each FIFO lane occupied until the complete group releases Writes.
                            FinishTarget();
                            if (finished is not null) await finished.Task.ConfigureAwait(false);
                        }
                    }, linked.Token).ConfigureAwait(false);
                }
                catch (Exception error) { ready?.TrySetException(error); throw; }
                finally
                {
                    // A cancelled/failed queue entry never enters its callback, but still owns one arrival.
                    if (!entered)
                    {
                        FinishTarget();
                        if (finished is not null) await finished.Task.ConfigureAwait(false);
                    }
                }
            }
            try { await Task.WhenAll(work.Targets.Select(QueueTargetAsync)).ConfigureAwait(false); }
            finally { ready?.TrySetCanceled(); _ = ready?.Task.Exception; lease?.Dispose(); }
            foreach (var waiter in work.Waiters) waiter.TrySetResult();
        }
        catch (OperationCanceledException) { foreach (var waiter in work.Waiters) waiter.TrySetCanceled(); }
        catch (Exception error) { foreach (var waiter in work.Waiters) waiter.TrySetException(error); }
    }

    private async Task SendTargetAsync(InputSnapshot snapshot, Target target, CancellationToken ct)
    {
        var current = _catalog.GetRequired(target.Serial);
        if (current.Generation != target.Generation || current.Width != target.Width || current.Height != target.Height || current.Rotation != target.Rotation)
        { _gestures.TryRemove(target.Serial, out _); throw new DeviceException($"Target generation or dimensions changed: serial={target.Serial}."); }
        var stale = snapshot.Generation != CurrentGeneration;
        if (stale)
        {
            // Completed-gesture ADB mode has no outstanding injected DOWN to release.
            if (_gestures.TryGetValue(target.Serial, out var old) && Matches(old, snapshot, target)) _gestures.TryRemove(target.Serial, out _);
            return;
        }
        if (snapshot.Kind != InputKind.Cancel)
        {
            var source = _catalog.GetRequired(snapshot.SourceSerial);
            if (source.Generation != snapshot.SourceGeneration) { _gestures.TryRemove(target.Serial, out _); throw new DeviceException($"Source reconnected: serial={source.Serial}."); }
        }
        (int X, int Y) Point(int x, int y) => ScalePoint(x, y, snapshot.SourceWidth, snapshot.SourceHeight, target.Width, target.Height);
        switch (snapshot.Kind)
        {
            case InputKind.Down:
                var down = Point(snapshot.X, snapshot.Y);
                _gestures[target.Serial] = new(snapshot.SourceSerial, snapshot.SourceGeneration, snapshot.Generation, snapshot.GestureId, target.Generation, down.X, down.Y, down.X, down.Y, Stopwatch.GetTimestamp());
                break;
            case InputKind.Move:
                if (_gestures.TryGetValue(target.Serial, out var move) && Matches(move, snapshot, target))
                { var next = Point(snapshot.X, snapshot.Y); _gestures[target.Serial] = move with { LastX = next.X, LastY = next.Y }; }
                break;
            case InputKind.Up:
                if (_gestures.TryGetValue(target.Serial, out var gesture) && Matches(gesture, snapshot, target))
                {
                    _gestures.TryRemove(target.Serial, out _);
                    var end = Point(snapshot.X, snapshot.Y);
                    var duration = Math.Clamp((int)Stopwatch.GetElapsedTime(gesture.Started).TotalMilliseconds, 30, 60000);
                    if (Math.Abs(gesture.X - end.X) <= 2 && Math.Abs(gesture.Y - end.Y) <= 2 && duration < 500)
                        await _backend.TapAsync(target.Serial, end.X, end.Y, ct).ConfigureAwait(false);
                    else await _backend.SwipeAsync(target.Serial, gesture.X, gesture.Y, end.X, end.Y, duration, ct).ConfigureAwait(false);
                }
                break;
            case InputKind.Cancel:
                if (_gestures.TryGetValue(target.Serial, out var cancel) && Matches(cancel, snapshot, target)) _gestures.TryRemove(target.Serial, out _);
                break;
            case InputKind.Tap:
                var tap = Point(snapshot.X, snapshot.Y); await _backend.TapAsync(target.Serial, tap.X, tap.Y, ct).ConfigureAwait(false); break;
            case InputKind.Swipe:
                var start = Point(snapshot.X, snapshot.Y); var stop = Point(snapshot.X2, snapshot.Y2);
                await _backend.SwipeAsync(target.Serial, start.X, start.Y, stop.X, stop.Y, snapshot.DurationMs, ct).ConfigureAwait(false); break;
            case InputKind.Key: await _backend.KeyEventAsync(target.Serial, snapshot.KeyCode, ct).ConfigureAwait(false); break;
            case InputKind.Text: await _backend.TextAsync(target.Serial, snapshot.Text, ct).ConfigureAwait(false); break;
            default: throw new DeviceException($"Unknown input action: {snapshot.Kind}.", 400, 1007);
        }
    }
    private static bool Matches(Gesture gesture, InputSnapshot snapshot, Target target) => gesture.Source == snapshot.SourceSerial && gesture.SourceGeneration == snapshot.SourceGeneration && gesture.Generation == snapshot.Generation && gesture.Id == snapshot.GestureId && gesture.DeviceGeneration == target.Generation;
    public static (int X, int Y) ScalePoint(int x, int y, int sourceWidth, int sourceHeight, int targetWidth, int targetHeight)
    {
        if (sourceWidth <= 0 || sourceHeight <= 0 || targetWidth <= 0 || targetHeight <= 0) throw new DeviceException($"Input dimensions are unavailable: source={sourceWidth}x{sourceHeight}, target={targetWidth}x{targetHeight}.", 409, 1003);
        if (x < 0 || y < 0 || x >= sourceWidth || y >= sourceHeight) throw new DeviceException($"Source point ({x},{y}) is outside {sourceWidth}x{sourceHeight}.", 400, 1007);
        return (Math.Clamp((int)((long)x * targetWidth / sourceWidth), 0, targetWidth - 1), Math.Clamp((int)((long)y * targetHeight / sourceHeight), 0, targetHeight - 1));
    }

    public async Task<long> HandoffAsync(string? sourceSerial, CancellationToken ct = default)
    {
        if (sourceSerial is not null) _catalog.GetRequired(sourceSerial);
        long generation;
        lock (_gate) { ObjectDisposedException.ThrowIf(_disposed, this); generation = ++_generation; _source = sourceSerial; }
        await ReleaseGesturesAsync(generation, ct).ConfigureAwait(false);
        return generation;
    }
    public async Task ReleaseAsync(CancellationToken ct = default)
    {
        long generation;
        lock (_gate) { ObjectDisposedException.ThrowIf(_disposed, this); generation = ++_generation; }
        await ReleaseGesturesAsync(generation, ct).ConfigureAwait(false);
    }
    private async Task ReleaseGesturesAsync(long generation, CancellationToken ct)
    {
        var devices = _catalog.Snapshot.Select(device => device.Serial).Concat(_gestures.Keys).Distinct(StringComparer.Ordinal).ToArray();
        await Task.WhenAll(devices.Select(serial => _targets.RunAsync(serial, token =>
        {
            if (_gestures.TryGetValue(serial, out var gesture) && gesture.Generation < generation)
                _gestures.TryRemove(new KeyValuePair<string, Gesture>(serial, gesture));
            return Task.CompletedTask;
        }, ct))).ConfigureAwait(false);
    }
    private void OnDisconnected(string serial)
    {
        foreach (var pair in _gestures.Where(pair => pair.Key == serial || pair.Value.Source == serial))
            _gestures.TryRemove(new KeyValuePair<string, Gesture>(pair.Key, pair.Value));
    }
    public async ValueTask DisposeAsync()
    {
        lock (_gate) { if (_disposed) return; _disposed = true; ++_generation; }
        _catalog.DeviceDisconnected -= OnDisconnected;
        _lifetime.Cancel();
        await _worker.ConfigureAwait(false);
        await Task.WhenAll(_inflight.Values).ConfigureAwait(false);
        await _targets.DisposeAsync().ConfigureAwait(false);
        _gestures.Clear(); _signal.Dispose(); _lifetime.Dispose();
    }
}
