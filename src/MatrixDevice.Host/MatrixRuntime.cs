using MatrixDevice.Core;
using MatrixDevice.Features;

namespace MatrixDevice.Host;

public sealed class MatrixRuntime : IAsyncDisposable
{
    public required IDeviceBackend Backend { get; init; }
    public required DeviceCatalog Devices { get; init; }
    public required ScriptScheduler Scripts { get; init; }
    public required InputCoordinator Input { get; init; }
    public required ApiServiceManager Api { get; init; }
    public required SettingsStore SettingsStore { get; init; }
    public required CatalogRepository Metadata { get; init; }
    public required AutomationService Automation { get; init; }
    public required RollingLog Log { get; init; }
    public required AppSettings Settings { get; set; }
    public required AppSettings FrozenConnectionSettings { get; init; }
    public LdPlayerService? LdPlayer { get; init; }
    public LdInstanceEditor? LdEditor { get; init; }
    public string? LdPlayerError { get; private set; }
    public bool IsDemo { get; init; }
    public string? DiscoveryError { get; private set; }
    public string? ApiStartupError { get; private set; }

    public static async Task<MatrixRuntime> CreateAsync(AppSettings settings, string dataDirectory, bool demo = false, IReadOnlyList<LdConnectionProof>? admissions = null, AdbDeviceBackend? epochSource = null, CancellationToken ct = default)
    {
        settings = settings with { Language = "zh-CN" };
        Directory.CreateDirectory(dataDirectory);
        var store = new SettingsStore(Path.Combine(dataDirectory, "AppConfig.json"));
        store.Load();
        if (!string.IsNullOrWhiteSpace(settings.AgentBaseUrl) && string.IsNullOrWhiteSpace(settings.AgentSerial))
            throw new ArgumentException("AgentSerial is required when AgentBaseUrl is configured");
        if (!string.IsNullOrWhiteSpace(settings.AgentBaseUrl) && (!Uri.TryCreate(settings.AgentBaseUrl, UriKind.Absolute, out var uri) || uri.Scheme is not ("http" or "https")))
            throw new ArgumentException("Invalid AgentBaseUrl: " + settings.AgentBaseUrl);
        CatalogRepository? metadata = null;
        RollingLog? log = null;
        IDeviceBackend? backend = null;
        ScriptScheduler? scripts = null;
        InputCoordinator? input = null;
        ApiServiceManager? api = null;
        LdPlayerService? ldPlayer = null;
        LdInstanceEditor? ldEditor = null;
        string? ldPlayerError = null;
        try
        {
        metadata = new CatalogRepository(Path.Combine(dataDirectory, "db.sqlite3"));
        log = new RollingLog(Path.Combine(dataDirectory, "logs"));
        backend = demo ? (IDeviceBackend)new DemoDeviceBackend() : new AdbDeviceBackend(settings);
        if (backend is AdbDeviceBackend inheritedBackend && epochSource is not null) inheritedBackend.InheritServerEpoch(epochSource);
        var catalog = new DeviceCatalog(backend);
        var automation = new AutomationService(backend, catalog);
        if (backend is AdbDeviceBackend adb && !string.IsNullOrWhiteSpace(settings.AgentBaseUrl))
        {
            var client = new PhoneAgentClient(settings.AgentBaseUrl);
            adb.RegisterAgent(settings.AgentSerial!, client);
            automation.ServiceInfoProvider = async (serial, ct) =>
            {
                if (serial != settings.AgentSerial) throw new DeviceException("No agent is bound to serial=" + serial, 501, 2001);
                if (settings.LdSerialBindings.Values.Contains(serial, StringComparer.Ordinal)) await adb.VerifyIdentityAsync(serial, ct);
                return await client.ServiceInfoAsync(ct);
            };
            automation.ServiceStop = async (serial, ct) =>
            {
                if (serial != settings.AgentSerial) throw new DeviceException("No agent is bound to serial=" + serial, 501, 2001);
                if (settings.LdSerialBindings.Values.Contains(serial, StringComparer.Ordinal)) await adb.VerifyIdentityAsync(serial, ct);
                await client.StopServiceAsync(ct);
            };
        }
        automation.ScreenshotTransform = (png, x, y, width, height, quality) =>
        {
            var size = PngImage.ReadDimensions(png);
            var left = x ?? 0;
            var top = y ?? 0;
            return PngImage.Crop(png, left, top, width ?? size.Width - left, height ?? size.Height - top);
        };
        scripts = new ScriptScheduler(backend);
        input = new InputCoordinator(catalog, backend);
        if (demo) ldPlayer = new LdPlayerService(new DemoLdConsoleRunner(), true);
        else if (!string.IsNullOrWhiteSpace(settings.LdConsolePath))
        {
            try
            {
                var runner = new LdConsoleRunner(settings.LdConsolePath, settings.CommandTimeoutSeconds);
                ldPlayer = new LdPlayerService(runner);
                ldEditor = new LdInstanceEditor(runner, ldPlayer);
            }
            catch (DeviceException e) { ldPlayerError = e.Message; log.Write("ldplayer-configuration-error", e.Message); }
        }
        api = new ApiServiceManager(catalog, backend, automation, log) { LdPlayer = ldPlayer };
        if (admissions is { Count: > 0 } && (ldPlayer is null || admissions.Any(proof => !settings.LdSerialBindings.TryGetValue(proof.Index, out var serial) || serial != proof.Serial || !settings.AdbAllowedSerials.Contains(proof.Serial, StringComparer.Ordinal))))
            throw new DeviceException("接入凭据与运行时授权不一致。", 409, 1003);
        bool ScriptBusy(string serial) => scripts.GetStatus(serial).State is ScriptRunState.Running or ScriptRunState.Cancelling;
        api.ScriptWriteBusy = ScriptBusy;
        api.AnyScriptWriteBusy = () => catalog.Snapshot.Any(device => ScriptBusy(device.Serial));
        var runtime = new MatrixRuntime
        {
            Backend = backend, Devices = catalog, Automation = automation, Scripts = scripts,
            Input = input, Api = api,
            SettingsStore = store, Settings = settings, FrozenConnectionSettings = settings with { AdbAllowedSerials = settings.AdbAllowedSerials.ToArray(), LdSerialBindings = new(settings.LdSerialBindings) }, Metadata = metadata,
            Log = log, IsDemo = demo, LdPlayer = ldPlayer, LdEditor = ldEditor, LdPlayerError = ldPlayerError
        };
        catalog.DeviceDisconnected += serial => { scripts.Stop(serial); log.Write("device-offline", "Device disconnected; script cancelled", serial); };
        runtime.Api.DeviceSnapshotProvider = runtime.GetDevices;
        scripts.EntryAdded += (serial, entry) => log.Write("script", entry.Message, serial);
        if (demo)
        {
            try { await runtime.RefreshAsync(ct); }
            catch (DeviceException e) { log.Write("discovery-error", e.Message); runtime.DiscoveryError = e.Message; }
        }
        if (ldPlayer is not null)
        {
            try { await ldPlayer.RefreshAsync(ct); }
            catch (OperationCanceledException) { throw; }
            catch (Exception e) { runtime.LdPlayerError = e.Message; log.Write("ldplayer-discovery-error", e.Message); }
            var writeBindings = new Dictionary<int, (string Serial, long Generation)>();
            api.LdSerialForWriteGuard = index => writeBindings.TryGetValue(index, out var binding)
                && ldPlayer.Instances.Any(instance => instance.Index == index && instance.Generation == binding.Generation)
                ? binding.Serial : null;
            if (!demo && backend is AdbDeviceBackend identityBackend)
                api.LdCommandIdentityGuard = async (index, operation, token) =>
                {
                    if (index <= 0) throw new DeviceException("原实例0受保护，未发送雷电命令。", 403, 1003);
                    if (operation is "launch" or "quit" or "reboot") return;
                    var serial = api.LdSerialForWriteGuard(index) ?? throw new DeviceException("请先接入并核验该实例。", 403, 1003);
                    await identityBackend.VerifyIdentityAsync(serial, token).ConfigureAwait(false);
                };
            if (demo)
                foreach (var binding in settings.LdSerialBindings.Where(pair => settings.AdbAllowedSerials.Contains(pair.Value, StringComparer.Ordinal)
                    && settings.LdSerialBindings.Count(other => other.Value == pair.Value) == 1))
                {
                    var instance = ldPlayer.Instances.FirstOrDefault(item => item.Index == binding.Key && item.AndroidStarted);
                    if (instance is not null && catalog.Snapshot.Any(device => device.Serial == binding.Value && device.Online))
                        writeBindings[binding.Key] = (binding.Value, instance.Generation);
                }
            if (!demo && backend is AdbDeviceBackend adbBackend)
            {
                var boundService = ldPlayer;
                foreach (var binding in settings.LdSerialBindings.Where(pair => settings.AdbAllowedSerials.Contains(pair.Value, StringComparer.Ordinal)).GroupBy(pair => pair.Value, StringComparer.Ordinal).Select(group => group.First()))
                {
                    var instance = ldPlayer.Instances.FirstOrDefault(item => item.Index == binding.Key && item.AndroidStarted);
                    var expectedGeneration = instance?.Generation ?? 0;
                    long expectedStarted = 0;
                    var identityReady = false;
                    adbBackend.RegisterIdentityGuard(binding.Value, async ct =>
                    {
                        if (!identityReady || instance is null || !boundService.Instances.Any(row => row.Index == binding.Key && row.Generation == expectedGeneration))
                            throw new DeviceException("雷电连接身份已变化或尚未核验，请在设备管理中重新接入。", 409, 1003);
                        await LdPlayerAdbBinding.VerifyAsync(binding.Value, instance.VBoxProcessId, expectedStarted, ct).ConfigureAwait(false);
                    });
                    if (instance is null || settings.LdSerialBindings.Count(pair => pair.Value == binding.Value) != 1) continue;
                    try
                    {
                        using var process = System.Diagnostics.Process.GetProcessById(instance.VBoxProcessId);
                        expectedStarted = process.StartTime.ToUniversalTime().Ticks;
                        if (process.HasExited) continue;
                    }
                    catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or System.ComponentModel.Win32Exception)
                    { log.Write("ldplayer-binding-error", "Cannot freeze the configured instance process: " + ex.Message, binding.Value); continue; }
                    var proof = admissions?.SingleOrDefault(item => item.Index == binding.Key);
                    if (epochSource is not null && proof is null)
                    {
                        try { await epochSource.VerifyIdentityAsync(binding.Value, ct); }
                        catch (DeviceException ex) { log.Write("ldplayer-binding-error", "Existing authorization was not renewed: " + ex.Message, binding.Value); continue; }
                    }
                    try { await LdPlayerAdbBinding.VerifyAsync(binding.Value, instance.VBoxProcessId, expectedStarted, ct); }
                    catch (DeviceException ex) { log.Write("ldplayer-binding-error", ex.Message, binding.Value); continue; }
                    if (proof is not null && (proof.Serial != binding.Value || proof.Name != instance.Name || proof.VBoxPid != instance.VBoxProcessId || proof.VBoxBirth != expectedStarted))
                        throw new DeviceException("实例在接入期间发生变化，旧授权未替换。", 409, 1003);
                    identityReady = true;
                    writeBindings[binding.Key] = (binding.Value, expectedGeneration);
                    adbBackend.RegisterUnicodeInput(binding.Value, async (text, ct) =>
                    {
                        await LdPlayerAdbBinding.VerifyAsync(binding.Value, instance.VBoxProcessId, expectedStarted, ct).ConfigureAwait(false);
                        await boundService.ExecuteAsync(binding.Key, "input", new System.Text.Json.Nodes.JsonObject
                        { ["expectedGeneration"] = expectedGeneration, ["text"] = text }, ct).ConfigureAwait(false);
                    });
                }
            }
        }
        if (!demo && settings.AdbAllowedSerials.Length > 0)
        {
            try { await runtime.RefreshAsync(ct); }
            catch (DeviceException e) { log.Write("discovery-error", e.Message); runtime.DiscoveryError = e.Message; }
        }
        else if (!demo) runtime.DiscoveryError = "请选择需要接入的本机实例";
        if (settings.AutoStartApi)
        {
            try { await runtime.StartApisAsync(ct); }
            catch (OperationCanceledException) { throw; }
            catch (Exception e) { runtime.ApiStartupError = e.Message; log.Write("api-startup-error", e.Message); }
        }
        return runtime;
        }
        catch
        {
            async Task Cleanup(Func<Task> action) { try { await action(); } catch { /* Preserve the construction error. */ } }
            if (api is not null) await Cleanup(() => api.DisposeAsync().AsTask());
            if (ldPlayer is not null) await Cleanup(() => ldPlayer.DisposeAsync().AsTask());
            if (input is not null) await Cleanup(() => input.DisposeAsync().AsTask());
            if (scripts is not null) await Cleanup(() => scripts.DisposeAsync().AsTask());
            if (backend is not null) await Cleanup(() => backend.DisposeAsync().AsTask());
            metadata?.Dispose();
            if (log is not null) await Cleanup(() => log.DisposeAsync().AsTask());
            throw;
        }
    }
    public async Task RefreshAsync(CancellationToken ct = default)
    {
        try { await Devices.RefreshAsync(ct); DiscoveryError = null; }
        catch (Exception e) { DiscoveryError = e.Message; throw; }
    }
    public void RequireFrozenConnectionSettings()
    {
        var frozen = FrozenConnectionSettings;
        if (Settings.LdConsolePath != frozen.LdConsolePath || Settings.AdbPath != frozen.AdbPath || Settings.CommandTimeoutSeconds != frozen.CommandTimeoutSeconds || Settings.AgentBaseUrl != frozen.AgentBaseUrl || Settings.AgentSerial != frozen.AgentSerial || !Settings.AdbAllowedSerials.SequenceEqual(frozen.AdbAllowedSerials) || !Settings.LdSerialBindings.OrderBy(pair => pair.Key).SequenceEqual(frozen.LdSerialBindings.OrderBy(pair => pair.Key)))
            throw new DeviceException("连接配置已保存但尚未生效，请重新启动矩阵设备后再接入或管理实例。", 409, 1003);
    }
    public async Task RefreshLdPlayerAsync(CancellationToken ct = default)
    {
        if (LdPlayer is null) throw new DeviceException("请先配置雷电命令行程序路径，并重新启动本程序", 503, 2001);
        try { await LdPlayer.RefreshAsync(ct); LdPlayerError = null; }
        catch (Exception e) { LdPlayerError = e.Message; throw; }
    }
    public IReadOnlyList<DeviceInfo> GetDevices() => Devices.Snapshot.Select(device =>
    {
        var metadata = Metadata.GetMetadata(device.Serial);
        return device with { Name = string.IsNullOrWhiteSpace(metadata.Name) ? device.Name : metadata.Name, GroupId = metadata.GroupId };
    }).ToArray();
    public async Task StartApisAsync(CancellationToken ct = default)
    {
        try
        {
            await Api.StartBasicAsync(Settings.BasicApiPort, ct);
            await Api.StartAutomatorAsync(Settings.AutomatorApiPort, ct);
            ApiStartupError = null;
        }
        catch { await Api.StopAsync(CancellationToken.None); throw; }
    }
    public Task StopApisAsync(CancellationToken ct = default) => Api.StopAsync(ct);
    public async ValueTask DisposeAsync()
    {
        List<Exception> errors = [];
        async Task Try(Func<Task> action) { try { await action(); } catch (Exception e) { errors.Add(e); } }
        await Try(() => Api.DisposeAsync().AsTask());
        if (LdPlayer is not null) await Try(() => LdPlayer.DisposeAsync().AsTask());
        await Try(() => Input.DisposeAsync().AsTask());
        await Try(() => Scripts.DisposeAsync().AsTask());
        await Try(() => Backend.DisposeAsync().AsTask());
        try { Metadata.Dispose(); } catch (Exception e) { errors.Add(e); }
        await Try(() => Log.DisposeAsync().AsTask());
        if (errors.Count > 0) throw new AggregateException("Runtime cleanup errors", errors);
    }
}
