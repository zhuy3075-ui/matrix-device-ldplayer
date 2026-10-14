namespace MatrixDevice.Core;

public sealed record LdLaunchProgress(int Index, string State, string Message, LdPlayerInstance? Instance = null);

public sealed class LdLaunchCoordinator
{
    private readonly LdPlayerService service;
    private readonly TimeSpan instanceTimeout;
    private readonly TimeSpan pollInterval;
    private readonly SemaphoreSlim batch = new(1, 1);

    public LdLaunchCoordinator(LdPlayerService service, TimeSpan? instanceTimeout = null, TimeSpan? pollInterval = null)
    {
        this.service = service ?? throw new ArgumentNullException(nameof(service));
        this.instanceTimeout = instanceTimeout ?? TimeSpan.FromMinutes(3);
        this.pollInterval = pollInterval ?? TimeSpan.FromSeconds(2);
        if (this.instanceTimeout <= TimeSpan.Zero || this.instanceTimeout > TimeSpan.FromMinutes(10))
            throw new ArgumentOutOfRangeException(nameof(instanceTimeout), "实例启动等待必须大于零且不超过十分钟。");
        if (this.pollInterval <= TimeSpan.Zero || this.pollInterval > this.instanceTimeout)
            throw new ArgumentOutOfRangeException(nameof(pollInterval), "轮询间隔必须大于零且不超过实例等待期限。");
    }

    public async Task<IReadOnlyList<LdLaunchProgress>> StartAsync(int[] indices,
        Func<CancellationToken, Task> ensureResourceReserve, IProgress<LdLaunchProgress>? progress = null,
        CancellationToken ct = default, IReadOnlyList<LdPlayerInstance>? expectedInstances = null)
    {
        if (indices is null || indices.Length == 0 || indices.Any(index => index <= 0 || index == 99999) || indices.Distinct().Count() != indices.Length)
            throw new DeviceException("启动批次必须包含不同的正整数实例编号，不允许原实例 0 或桌面编号 99999。", 400, 1007);
        ArgumentNullException.ThrowIfNull(ensureResourceReserve);
        var requested = indices.ToArray();
        var expected = expectedInstances?.ToArray();
        if (expected is not null && (expected.Length != requested.Length || expected.Any(instance => instance is null) ||
            !expected.Select(instance => instance.Index).ToHashSet().SetEquals(requested)))
            throw new DeviceException("确认快照必须完整且唯一地对应本次启动编号，不能缺少、重复或包含额外实例。", 400, 1007);
        var confirmed = expected?.ToDictionary(instance => instance.Index);
        await batch.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            var results = new List<LdLaunchProgress>();
            foreach (var index in requested)
            {
                ct.ThrowIfCancellationRequested();
                using var deadline = CancellationTokenSource.CreateLinkedTokenSource(ct);
                deadline.CancelAfter(instanceTimeout);
                var token = deadline.Token;
                try
                {
                    await service.RefreshAsync(token).ConfigureAwait(false);
                    var original = Require(index);
                    if (confirmed is not null && (original.Generation != confirmed[index].Generation || original.Name != confirmed[index].Name))
                        throw new DeviceException($"确认后实例身份或代次已变化：索引={index}；未提交启动命令，请刷新后重新确认。", 409, 1003);
                    if (IsReady(original))
                    {
                        var skipped = new LdLaunchProgress(index, "skipped", "实例已运行，未重复启动；尚未核验 ADB、设备授权或应用页面。", original);
                        progress?.Report(skipped); results.Add(skipped); continue;
                    }
                    if (IsStopped(original))
                    {
                        progress?.Report(new(index, "checking-resources", "正在检查本次启动的资源预留，不推测未知实例内存。", original));
                        await ensureResourceReserve(token).ConfigureAwait(false);
                        token.ThrowIfCancellationRequested();
                        await service.RefreshAsync(token).ConfigureAwait(false);
                        var before = RequireSameIdentity(original);
                        if (before.Generation != original.Generation)
                            throw new DeviceException($"资源检查期间实例代次已变化：索引={index}，原代次={original.Generation}，当前={before.Generation}；未提交启动命令，请刷新后重新确认。", 409, 1003);
                        if (IsStopped(before))
                        {
                            progress?.Report(new(index, "starting", "正在提交一次启动命令；取消或超时不会关闭已启动实例。", before));
                            await service.ExecuteAsync(index, "launch", new System.Text.Json.Nodes.JsonObject
                            { ["expectedGeneration"] = before.Generation }, token).ConfigureAwait(false);
                        }
                    }
                    while (true)
                    {
                        token.ThrowIfCancellationRequested();
                        await service.RefreshAsync(token).ConfigureAwait(false);
                        var observed = RequireSameIdentity(original);
                        if (IsReady(observed))
                        {
                            var ready = new LdLaunchProgress(index, "ready", "雷电目录已确认安卓启动标志及播放器、虚拟机进程；尚未核验 ADB、设备授权或应用页面。", observed);
                            progress?.Report(ready); results.Add(ready); break;
                        }
                        progress?.Report(new(index, "waiting", "等待现有启动过程，不重复提交 launch；ADB 尚未核验。", observed));
                        await Task.Delay(pollInterval, token).ConfigureAwait(false);
                    }
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    progress?.Report(new(index, "cancelled", "已取消本程序等待及后续启动；未关闭任何已启动实例。"));
                    throw;
                }
                catch (OperationCanceledException) when (deadline.IsCancellationRequested)
                {
                    var message = $"实例 {index} 在 {instanceTimeout.TotalSeconds:0.###} 秒内未确认启动完成；结果未知，不重发 launch，不启动后续实例，也不关闭已启动实例。";
                    progress?.Report(new(index, "failed", message));
                    throw new DeviceException(message, 504, 2000);
                }
                catch (Exception error)
                {
                    progress?.Report(new(index, "failed", $"实例启动批次已停止：{error.Message}；未关闭已启动实例。"));
                    throw;
                }
            }
            return results.AsReadOnly();
        }
        finally { batch.Release(); }
    }

    private LdPlayerInstance Require(int index) => service.Instances.FirstOrDefault(instance => instance.Index == index)
        ?? throw new DeviceException($"雷电实例不存在：索引={index}", 404, 1002);

    private LdPlayerInstance RequireSameIdentity(LdPlayerInstance original)
    {
        var current = Require(original.Index);
        if (current.Name != original.Name || current.Width != original.Width || current.Height != original.Height || current.Dpi != original.Dpi)
            throw new DeviceException($"等待期间实例身份或配置已变化：索引={original.Index}；停止当前批次，请刷新目录后重新确认。", 409, 1003);
        return current;
    }

    private static bool IsReady(LdPlayerInstance instance) => !instance.IsStarting && instance.AndroidStarted && instance.ProcessId > 0 && instance.VBoxProcessId > 0;
    private static bool IsStopped(LdPlayerInstance instance) => !instance.IsStarting && !instance.AndroidStarted && instance.ProcessId <= 0 && instance.VBoxProcessId <= 0 && instance.TopWindow == 0 && instance.BindWindow == 0;
}
