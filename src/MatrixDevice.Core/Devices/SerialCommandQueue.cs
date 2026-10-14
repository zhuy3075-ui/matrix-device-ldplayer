using System.Collections.Concurrent;

namespace MatrixDevice.Core;

public sealed class SerialCommandQueue : IAsyncDisposable
{
    private readonly ConcurrentDictionary<string, Lane> _lanes = new(StringComparer.Ordinal);
    private readonly CancellationTokenSource _lifetime = new();
    private int _disposed;

    private sealed class Lane
    {
        public object Gate { get; } = new();
        public Task Tail { get; set; } = Task.CompletedTask;
    }

    public async Task<T> RunAsync<T>(string serial, Func<CancellationToken, Task<T>> action, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serial);
        ArgumentNullException.ThrowIfNull(action);
        ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed) != 0, this);
        var lane = _lanes.GetOrAdd(serial, _ => new Lane());
        Task previous;
        var completed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        lock (lane.Gate)
        {
            previous = lane.Tail;
            lane.Tail = completed.Task;
        }
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct, _lifetime.Token);
        try
        {
            // Wait for predecessors even when cancelled, so successors cannot overtake them.
            await previous.ConfigureAwait(false);
            linked.Token.ThrowIfCancellationRequested();
            return await action(linked.Token).ConfigureAwait(false);
        }
        finally { completed.TrySetResult(); }
    }

    public Task RunAsync(string serial, Func<CancellationToken, Task> action, CancellationToken ct = default) =>
        RunAsync(serial, async token => { await action(token).ConfigureAwait(false); return true; }, ct);

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0) return;
        _lifetime.Cancel();
        await Task.WhenAll(_lanes.Values.Select(lane => { lock (lane.Gate) return lane.Tail; })).ConfigureAwait(false);
        _lifetime.Dispose();
    }
}
