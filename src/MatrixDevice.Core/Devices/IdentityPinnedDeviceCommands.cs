namespace MatrixDevice.Core;

public sealed record DeviceCommandTarget(string Serial, long Generation, uint? TransportId, bool IsDemo);

public interface IIdentityPinnedDeviceBackend
{
    IIdentityPinnedDeviceCommands BindIdentity(DeviceCatalog catalog);
}

public interface IIdentityPinnedDeviceCommands
{
    bool IsDemo { get; }
    Task<string> StartAppAsync(DeviceCommandTarget target, string packageName, CancellationToken ct = default);
    Task TapAsync(DeviceCommandTarget target, int x, int y, CancellationToken ct = default);
    Task SwipeAsync(DeviceCommandTarget target, int x1, int y1, int x2, int y2, int durationMs, CancellationToken ct = default);
}

internal static class DeviceCommandIdentity
{
    internal static void Require(DeviceCatalog catalog, DeviceCommandTarget target, bool isDemo)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(target);
        if (string.IsNullOrWhiteSpace(target.Serial) || target.Generation <= 0 || target.IsDemo != isDemo
            || (isDemo ? target.TransportId is not null : target.TransportId is null or 0))
            throw new DeviceException($"任务目标身份无效：serial={target.Serial}, generation={target.Generation}, transport={target.TransportId}, demo={target.IsDemo}。", 400, 1007);
        var current = catalog.GetRequired(target.Serial);
        if (current.Generation != target.Generation || current.TransportId != target.TransportId)
            throw new DeviceException($"任务目标身份已变化：serial={target.Serial}, expected generation={target.Generation}/transport={target.TransportId}, actual generation={current.Generation}/transport={current.TransportId}。", 409, 1003);
    }
}
