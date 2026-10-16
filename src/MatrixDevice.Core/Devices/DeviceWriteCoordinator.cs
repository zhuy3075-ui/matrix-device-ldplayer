namespace MatrixDevice.Core;

public sealed class DeviceWriteCoordinator
{
    private readonly object gate = new();
    private readonly Dictionary<string, Lease> owners = new(StringComparer.Ordinal);
    private Lease? barrier;

    public IDisposable Acquire(IEnumerable<string> serials)
    {
        if (serials is null) throw new DeviceException("Write targets must not be null.", 400, 1007);
        var frozen = serials.ToArray();
        if (frozen.Length == 0) throw new DeviceException("Write targets must not be empty.", 400, 1007);
        foreach (var serial in frozen) ValidateSerial(serial);
        frozen = frozen.Distinct(StringComparer.Ordinal).ToArray();
        lock (gate)
        {
            if (barrier is not null || frozen.Any(owners.ContainsKey))
                throw new DeviceException("A write target is already reserved.", 409, 1003);
            var lease = new Lease(this, frozen, all: false);
            foreach (var serial in frozen) owners.Add(serial, lease);
            return lease;
        }
    }

    public IDisposable AcquireAll()
    {
        lock (gate)
        {
            if (barrier is not null || owners.Count != 0)
                throw new DeviceException("Device writes are already reserved.", 409, 1003);
            return barrier = new Lease(this, [], all: true);
        }
    }

    public bool IsBusy(string serial)
    {
        ValidateSerial(serial);
        lock (gate) return barrier is not null || owners.ContainsKey(serial);
    }

    private static void ValidateSerial(string serial)
    {
        if (string.IsNullOrWhiteSpace(serial) || serial.Any(char.IsWhiteSpace) || serial.Any(char.IsControl))
            throw new DeviceException($"Invalid write target serial: '{serial}'.", 400, 1007);
    }

    private void Release(Lease lease)
    {
        lock (gate)
        {
            if (lease.All)
            {
                if (ReferenceEquals(barrier, lease)) barrier = null;
            }
            else foreach (var serial in lease.Serials)
                if (owners.TryGetValue(serial, out var owner) && ReferenceEquals(owner, lease)) owners.Remove(serial);
        }
    }

    private sealed class Lease(DeviceWriteCoordinator coordinator, string[] serials, bool all) : IDisposable
    {
        private DeviceWriteCoordinator? owner = coordinator;
        internal string[] Serials { get; } = serials;
        internal bool All { get; } = all;
        public void Dispose() => Interlocked.Exchange(ref owner, null)?.Release(this);
    }
}
