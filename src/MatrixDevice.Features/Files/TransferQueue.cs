using MatrixDevice.Core;

namespace MatrixDevice.Features;

public enum TransferDirection { Push, Pull }
public enum TransferState { Pending, Transferring, Installing, Completed, Failed, Cancelled }
public sealed record TransferItem
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Serial { get; init; }
    public required string LocalPath { get; init; }
    public required string RemotePath { get; init; }
    public TransferDirection Direction { get; init; }
    public bool AutoInstall { get; init; }
    public bool IsApk => LocalPath.EndsWith(".apk", StringComparison.OrdinalIgnoreCase);
    public string FileName => Path.GetFileName(LocalPath);
    public TransferState State { get; internal set; }
    public double ProgressPercent { get; internal set; }
    public string? Error { get; internal set; }
}

public sealed class TransferQueue(IDeviceBackend backend) : IAsyncDisposable
{
    private sealed class DirectProgress(Action<double> report) : IProgress<double>
    { public void Report(double value) => report(value); }
    private readonly object gate = new();
    private readonly List<TransferItem> items = [];
    private CancellationTokenSource? current;
    private Task? active;
    private bool disposed;
    public bool IsTransferring { get { lock (gate) return current is not null; } }
    public IReadOnlyList<TransferItem> Items { get { lock (gate) return items.Select(item => item with { }).ToArray(); } }
    public double OverallProgressPercent { get { lock (gate) return items.Count == 0 ? 0 : items.Average(item => item.ProgressPercent); } }
    public event Action<TransferItem>? ItemChanged;

    public Guid Add(TransferItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentException.ThrowIfNullOrWhiteSpace(item.Serial);
        ArgumentException.ThrowIfNullOrWhiteSpace(item.LocalPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(item.RemotePath);
        if (!Enum.IsDefined(item.Direction)) throw new DeviceException($"Unknown transfer direction: {item.Direction}.", 400, 1007);
        if (!item.RemotePath.StartsWith('/') || item.RemotePath.IndexOfAny(['\r', '\n', '\0']) >= 0)
            throw new DeviceException($"Remote path must be absolute and contain no control characters: '{item.RemotePath}'.", 400, 1007);
        if (item.Direction == TransferDirection.Push && !File.Exists(item.LocalPath)) throw new FileNotFoundException("Transfer source does not exist.", item.LocalPath);
        if (item.AutoInstall && (item.Direction != TransferDirection.Push || !item.IsApk)) throw new DeviceException("AutoInstall requires an APK upload.", 400, 1007);
        lock (gate)
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            if (items.Any(existing => existing.Id == item.Id)) throw new DeviceException($"Transfer id already exists: {item.Id}.");
            items.Add(item with { LocalPath = Path.GetFullPath(item.LocalPath), State = TransferState.Pending, ProgressPercent = 0, Error = null });
        }
        return item.Id;
    }

    public Task RunAsync(CancellationToken ct = default)
    {
        lock (gate)
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            if (current is not null) throw new DeviceException("The transfer queue is already running.");
            current = CancellationTokenSource.CreateLinkedTokenSource(ct);
            var source = current;
            return active = Task.Run(() => ProcessAsync(source));
        }
    }

    public bool Cancel()
    {
        lock (gate)
        {
            if (current is null) return false;
            current.Cancel(); return true;
        }
    }

    private async Task ProcessAsync(CancellationTokenSource source)
    {
        try
        {
            while (true)
            {
                source.Token.ThrowIfCancellationRequested();
                TransferItem? item;
                lock (gate) item = items.FirstOrDefault(candidate => candidate.State == TransferState.Pending);
                if (item is null) return;
                Update(item, TransferState.Transferring, 0, null);
                try
                {
                    var progress = new DirectProgress(value =>
                    {
                        if (!double.IsFinite(value)) throw new DeviceException($"Transfer reported a nonfinite progress value: {value}.", 500, 2000);
                        Update(item, TransferState.Transferring, Math.Clamp(value, 0, 1) * 100, null);
                    });
                    if (item.Direction == TransferDirection.Push)
                        await backend.PushAsync(item.Serial, item.LocalPath, item.RemotePath, progress, source.Token).ConfigureAwait(false);
                    else
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(item.LocalPath)!);
                        await backend.PullAsync(item.Serial, item.RemotePath, item.LocalPath, progress, source.Token).ConfigureAwait(false);
                    }
                    source.Token.ThrowIfCancellationRequested();
                    if (item.AutoInstall)
                    {
                        Update(item, TransferState.Installing, 100, null);
                        await backend.InstallAsync(item.Serial, item.LocalPath, source.Token).ConfigureAwait(false);
                    }
                    Update(item, TransferState.Completed, 100, null);
                }
                catch (OperationCanceledException) when (source.IsCancellationRequested)
                {
                    Update(item, TransferState.Cancelled, item.ProgressPercent, "Cancelled.");
                    throw;
                }
                catch (Exception error) { Update(item, TransferState.Failed, item.ProgressPercent, error.Message); }
            }
        }
        finally
        {
            lock (gate)
            {
                if (ReferenceEquals(current, source)) current = null;
                source.Dispose();
            }
        }
    }

    private void Update(TransferItem item, TransferState state, double progress, string? error)
    {
        TransferItem snapshot;
        lock (gate)
        {
            item.State = state;
            item.ProgressPercent = Math.Max(item.ProgressPercent, progress);
            item.Error = error;
            snapshot = item with { };
        }
        if (ItemChanged is { } changed)
            foreach (Action<TransferItem> handler in changed.GetInvocationList())
                try { handler(snapshot); }
                catch (Exception observerError) { System.Diagnostics.Trace.WriteLine($"Transfer observer failed: {observerError.Message}"); }
    }

    public async ValueTask DisposeAsync()
    {
        Task? pending;
        lock (gate)
        {
            if (disposed) return;
            disposed = true;
            current?.Cancel(); pending = active;
        }
        if (pending is not null)
            try { await pending.ConfigureAwait(false); }
            catch (OperationCanceledException) { }
    }
}
