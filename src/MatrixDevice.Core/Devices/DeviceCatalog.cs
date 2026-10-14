namespace MatrixDevice.Core;

public sealed class DeviceCatalog(IDeviceBackend backend)
{
    private readonly SemaphoreSlim _refresh = new(1, 1);
    private readonly object _gate = new();
    private IReadOnlyList<DeviceInfo> _snapshot = Array.Empty<DeviceInfo>();
    public event Action<string>? DeviceDisconnected;
    public IReadOnlyList<DeviceInfo> Snapshot { get { lock (_gate) return _snapshot; } }

    public DeviceInfo GetRequired(string serial)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serial);
        DeviceInfo? device;
        lock (_gate) device = _snapshot.FirstOrDefault(item => item.Serial == serial);
        if (device is null) throw new DeviceException($"Device not found: serial={serial}", 404, 1002);
        if (!device.Online) throw new DeviceException($"Device unavailable: serial={serial}, state={device.State}", 410, 1003);
        return device;
    }

    public async Task<IReadOnlyList<DeviceInfo>> RefreshAsync(CancellationToken ct = default)
    {
        await _refresh.WaitAsync(ct).ConfigureAwait(false);
        var disconnected = new List<string>();
        try
        {
            var discovered = await backend.DiscoverAsync(ct).ConfigureAwait(false);
            if (discovered.GroupBy(item => item.Serial, StringComparer.Ordinal).Any(group => group.Count() > 1))
                throw new DeviceException("Discovery returned duplicate serial numbers.", 503, 2000);
            lock (_gate)
            {
                var old = _snapshot.ToDictionary(item => item.Serial, StringComparer.Ordinal);
                var next = new List<DeviceInfo>();
                foreach (var item in discovered)
                {
                    ArgumentException.ThrowIfNullOrWhiteSpace(item.Serial);
                    var generation = 1L;
                    if (old.Remove(item.Serial, out var previous))
                    {
                        generation = previous.Generation;
                        if (previous.Online != item.Online || previous.Width != item.Width || previous.Height != item.Height || previous.Rotation != item.Rotation || previous.TransportId != item.TransportId) generation++;
                        if (previous.Online && (!item.Online || previous.TransportId != item.TransportId)) disconnected.Add(item.Serial);
                    }
                    next.Add(item with { Generation = generation });
                }
                foreach (var previous in old.Values)
                {
                    if (previous.Online) disconnected.Add(previous.Serial);
                    next.Add(previous with
                    {
                        State = "Offline", Error = "Device absent from the latest ADB discovery.",
                        Generation = previous.Generation + (previous.Online ? 1 : 0)
                    });
                }
                _snapshot = next.AsReadOnly();
            }
        }
        finally { _refresh.Release(); }
        foreach (var serial in disconnected) DeviceDisconnected?.Invoke(serial);
        return Snapshot;
    }
}
