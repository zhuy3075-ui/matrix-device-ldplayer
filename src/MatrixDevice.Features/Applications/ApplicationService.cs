using MatrixDevice.Core;

namespace MatrixDevice.Features;

public sealed class ApplicationService(IDeviceBackend backend)
{
    public async Task<IReadOnlyList<AppInfo>> ListAsync(string serial, string? filter = null, bool showSystem = false, CancellationToken ct = default)
    {
        var applications = await backend.ListAppsAsync(serial, showSystem, true, ct).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(filter)) return applications;
        return applications.Where(app => app.PackageName.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
            (app.Name?.Contains(filter, StringComparison.OrdinalIgnoreCase) ?? false)).ToArray();
    }
    public Task<string> StartAsync(string serial, string packageName, string? activity = null, CancellationToken ct = default)
    {
        ScriptPackageService.ValidatePackageName(packageName);
        return backend.StartAppAsync(serial, packageName, activity, ct);
    }
    public Task<string> StopAsync(string serial, string packageName, CancellationToken ct = default)
    {
        ScriptPackageService.ValidatePackageName(packageName);
        return backend.StopAppAsync(serial, packageName, ct);
    }
    public Task InstallAsync(string serial, string path, CancellationToken ct = default)
    {
        if (!File.Exists(path)) throw new FileNotFoundException("APK does not exist.", path);
        return backend.InstallAsync(serial, Path.GetFullPath(path), ct);
    }
    public async Task<IReadOnlyDictionary<string, bool>> ClearAsync(IReadOnlyList<string> serials, string packageName, bool confirmed, CancellationToken ct = default)
    {
        Confirm(serials, packageName, confirmed);
        var results = new Dictionary<string, bool>(StringComparer.Ordinal);
        foreach (var serial in serials.Distinct(StringComparer.Ordinal))
            results[serial] = await backend.ClearAppAsync(serial, packageName, ct).ConfigureAwait(false);
        return results;
    }
    public async Task UninstallAsync(IReadOnlyList<string> serials, string packageName, bool confirmed, CancellationToken ct = default)
    {
        Confirm(serials, packageName, confirmed);
        foreach (var serial in serials.Distinct(StringComparer.Ordinal))
            await backend.UninstallAsync(serial, packageName, ct).ConfigureAwait(false);
    }
    private static void Confirm(IReadOnlyList<string> serials, string packageName, bool confirmed)
    {
        ArgumentNullException.ThrowIfNull(serials);
        ScriptPackageService.ValidatePackageName(packageName);
        if (serials.Count == 0 || serials.Any(string.IsNullOrWhiteSpace)) throw new DeviceException("At least one explicit device is required.", 400, 1007);
        if (!confirmed) throw new DeviceException($"Confirmation is required for '{packageName}' on {serials.Count} device(s).", 409, 1003);
    }
}
