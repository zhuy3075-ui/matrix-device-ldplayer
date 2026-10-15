using MatrixDevice.Core;

namespace MatrixDevice.Features;

public sealed record ApplicationTarget(string Serial, string Name, long Generation, uint? TransportId)
{
    public static ApplicationTarget Freeze(DeviceInfo device) => new(device.Serial, device.Name, device.Generation, device.TransportId);
    public void RequireSame(DeviceInfo? device)
    {
        if (device is null || !device.Online || device.Serial != Serial || device.Generation != Generation || device.TransportId != TransportId)
            throw new DeviceException($"模拟器连接已变化，请重新选择并读取应用：{Serial}。", 409, 1003);
    }
}

public enum ApplicationOperation { List, Install, Uninstall, Start, Stop, Clear }
public enum ApplicationTaskState { Waiting, Running, Succeeded, CompletedUnverified, Failed, Cancelled }
public sealed record ApplicationBatchItem(ApplicationTarget Target, ApplicationOperation Operation, string? PackageName = null, string? LocalPath = null, bool IsSystem = false);
public sealed record ApplicationItemResult(int Index, ApplicationBatchItem Item, ApplicationTaskState State, string Message, IReadOnlyList<AppInfo>? Applications = null);

public sealed class ApplicationBatch(IDeviceBackend backend, Func<ApplicationTarget, CancellationToken, Task> validateTarget)
{
    public static IReadOnlyList<ApplicationBatchItem> InstallPlan(IReadOnlyList<ApplicationTarget> targets, IReadOnlyList<string> paths)
    {
        RequireTargets(targets);
        if (paths.Count == 0) throw new DeviceException("未选择 APK 文件。", 400, 1007);
        var files = paths.Select(Path.GetFullPath).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        foreach (var file in files)
            if (!File.Exists(file) || !Path.GetExtension(file).Equals(".apk", StringComparison.OrdinalIgnoreCase))
                throw new DeviceException($"APK 文件不存在或格式不支持：{file}。", 400, 1007);
        return targets.SelectMany(target => files.Select(path => new ApplicationBatchItem(target, ApplicationOperation.Install, LocalPath: path))).ToArray();
    }

    public static IReadOnlyList<ApplicationBatchItem> RowPlan(ApplicationOperation operation, IReadOnlyList<ApplicationBatchItem> selectedRows)
    {
        if (operation is ApplicationOperation.Install or ApplicationOperation.List) throw new ArgumentOutOfRangeException(nameof(operation));
        if (selectedRows.Count == 0) throw new DeviceException("请勾选要操作的应用行。", 400, 1007);
        var result = selectedRows.Select(row => row with { Operation = operation, LocalPath = null }).DistinctBy(row => (row.Target.Serial, row.PackageName)).ToArray();
        foreach (var row in result)
        {
            ScriptPackageService.ValidatePackageName(row.PackageName ?? "");
            if (row.IsSystem && operation is ApplicationOperation.Uninstall or ApplicationOperation.Clear)
                throw new DeviceException($"系统应用受保护：{row.Target.Serial} / {row.PackageName}。", 403, 1003);
        }
        return result;
    }

    private static void RequireTargets(IReadOnlyList<ApplicationTarget> targets)
    {
        if (targets.Count == 0 || targets.Any(target => string.IsNullOrWhiteSpace(target.Serial) || target.Generation <= 0)
            || targets.Select(target => target.Serial).Distinct(StringComparer.Ordinal).Count() != targets.Count)
            throw new DeviceException("必须选择互不重复的明确模拟器目标。", 400, 1007);
    }

    public async Task<IReadOnlyList<ApplicationItemResult>> ExecuteAsync(IReadOnlyList<ApplicationBatchItem> items, bool confirmed,
        Action<ApplicationItemResult>? progress = null, CancellationToken stopAfterCurrent = default)
    {
        if (items.Count == 0) throw new DeviceException("应用任务为空，未操作设备。", 400, 1007);
        foreach (var item in items)
        {
            RequireTargets([item.Target]);
            if (!Enum.IsDefined(item.Operation)) throw new ArgumentOutOfRangeException(nameof(items));
            if (item.Operation is not (ApplicationOperation.List or ApplicationOperation.Install)) ScriptPackageService.ValidatePackageName(item.PackageName ?? "");
        }
        var results = new List<ApplicationItemResult>();
        for (var index = 0; index < items.Count; index++) progress?.Invoke(new(index, items[index], ApplicationTaskState.Waiting, "等待"));
        for (var index = 0; index < items.Count; index++)
        {
            var item = items[index];
            ApplicationItemResult result;
            if (stopAfterCurrent.IsCancellationRequested || (!confirmed && item.Operation != ApplicationOperation.List))
                result = new(index, item, ApplicationTaskState.Cancelled, "已取消，未发送命令");
            else
            {
                progress?.Invoke(new(index, item, ApplicationTaskState.Running, "正在执行"));
                try
                {
                    // Cancellation stops only future items; an in-flight install is fully observed, not reported as rolled back.
                    await validateTarget(item.Target, CancellationToken.None).ConfigureAwait(false);
                    var before = await backend.ListAppsAsync(item.Target.Serial, true, true).ConfigureAwait(false);
                    await validateTarget(item.Target, CancellationToken.None).ConfigureAwait(false);
                    if (item.Operation != ApplicationOperation.List && item.Operation != ApplicationOperation.Install)
                    {
                        var app = before.SingleOrDefault(app => app.PackageName == item.PackageName)
                            ?? throw new DeviceException($"应用已不存在：{item.Target.Serial} / {item.PackageName}。", 409, 1003);
                        if (app.IsSystem && item.Operation is ApplicationOperation.Uninstall or ApplicationOperation.Clear)
                            throw new DeviceException($"系统应用受保护：{item.Target.Serial} / {item.PackageName}。", 403, 1003);
                    }
                    switch (item.Operation)
                    {
                        case ApplicationOperation.Install:
                            if (!File.Exists(item.LocalPath) || !string.Equals(Path.GetExtension(item.LocalPath), ".apk", StringComparison.OrdinalIgnoreCase))
                                throw new DeviceException($"APK 文件已不存在或扩展名无效：{item.LocalPath}。", 400, 1007);
                            await backend.InstallAsync(item.Target.Serial, item.LocalPath).ConfigureAwait(false); break;
                        case ApplicationOperation.Uninstall: await backend.UninstallAsync(item.Target.Serial, item.PackageName!).ConfigureAwait(false); break;
                        case ApplicationOperation.Start: await backend.StartAppAsync(item.Target.Serial, item.PackageName!).ConfigureAwait(false); break;
                        case ApplicationOperation.Stop: await backend.StopAppAsync(item.Target.Serial, item.PackageName!).ConfigureAwait(false); break;
                        case ApplicationOperation.Clear:
                            if (!await backend.ClearAppAsync(item.Target.Serial, item.PackageName!).ConfigureAwait(false)) throw new DeviceException("设备未确认清除完成。", 409, 1003);
                            break;
                    }
                    await validateTarget(item.Target, CancellationToken.None).ConfigureAwait(false);
                    var after = item.Operation == ApplicationOperation.List ? before : await backend.ListAppsAsync(item.Target.Serial, true, true).ConfigureAwait(false);
                    await validateTarget(item.Target, CancellationToken.None).ConfigureAwait(false);
                    if (item.Operation == ApplicationOperation.Uninstall && after.Any(app => app.PackageName == item.PackageName))
                        throw new DeviceException($"卸载命令返回后应用仍存在：{item.Target.Serial} / {item.PackageName}。", 502, 2000);
                    result = new(index, item, item.Operation == ApplicationOperation.Install ? ApplicationTaskState.CompletedUnverified : ApplicationTaskState.Succeeded,
                        item.Operation == ApplicationOperation.Install ? "安装命令已完成，列表已刷新；APK 包名未核验" : item.Operation == ApplicationOperation.Uninstall ? "已核验应用移除，列表已刷新" : "已完成，列表已刷新", after);
                }
                catch (Exception error)
                {
                    result = new(index, item, ApplicationTaskState.Failed, error.Message);
                }
            }
            results.Add(result); progress?.Invoke(result);
        }
        return results;
    }
}
