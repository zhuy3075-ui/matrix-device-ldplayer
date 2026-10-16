using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MatrixDevice.Core;
using MatrixDevice.Features;
using MatrixDevice.Host;

namespace MatrixDevice.Desktop;

public sealed partial class WorkspaceViewModel : ObservableObject
{
    private MatrixRuntime runtime;
    private readonly string dataDirectory;
    private readonly bool demo;
    private readonly List<object> commands = [];
    private CancellationTokenSource? transferCancellation;
    private DeviceRow? currentDevice;
    private DeviceRow? mainDevice;
    private bool busy;
    private bool externalBusy;
    private int pendingLaunchSelection;
    private bool blockApiWhileBusy = true;
    private string status = "等待发现设备";
    private string error = "";
    private BitmapSource? debugImage;
    private readonly Dictionary<DeviceRow, CompletedScreenshot> completedScreenshots = [];
    private CompletedScreenshot? completedDebugImage;
    private CompletedNodeTree? completedNodeTree;
    private DebugSourceIdentity? observedCurrentDebugSource;
    private long debugPublicationRevision;
    private AndroidNode? nodeRoot;
    private NodeRow? selectedNode;
    private string shellInput = "";
    private string textInput = "";
    private string remotePath = "/sdcard/MatrixExchange/";
    private bool transferring;
    private bool showSystemApps;
    private AppInfo? selectedApp;
    private ScriptPackage? package;
    private bool syncMouse;
    private bool syncKeyboard;
    private int wallColumns;
    private string deviceName = "";
    private string settingsNotice = "";

    public WorkspaceViewModel(MatrixRuntime initialRuntime, string dataDirectory, bool demo)
    {
        runtime = initialRuntime;
        this.dataDirectory = dataDirectory;
        this.demo = demo;
        error = string.Join("\n", new[] { runtime.DiscoveryError, runtime.ApiStartupError }.Where(message => message is not null));
        syncMouse = runtime.Settings.SyncMouse;
        syncKeyboard = runtime.Settings.SyncKeyboard;
        wallColumns = runtime.Settings.WallColumns;
        Settings = new SettingsEditor(runtime.Settings);
        DeviceView = new ListCollectionView(Devices);
        AppsView = CollectionViewSource.GetDefaultView(Apps);
        RefreshCommand = Async(RefreshDevicesAsync, () => !Busy);
        ScreenshotCommand = Async(() => CaptureAsync(), CanUseDevice);
        WallRefreshCommand = Async(RefreshWallAsync, () => !Busy && Devices.Any(row => row.Info.Online));
        DumpCommand = Async(LoadNodesAsync, CanUseDevice);
        ShellCommand = Async(ExecuteShellAsync, () => CanWriteDevice() && !string.IsNullOrWhiteSpace(ShellInput));
        RefreshAppsCommand = Async(LoadAppsAsync, CanUseDevice);
        LaunchAppCommand = Async(() => ChangeAppAsync("launch"), () => CanWriteTargets() && SelectedApp is not null);
        StopAppCommand = Async(() => ChangeAppAsync("stop"), () => CanWriteTargets() && SelectedApp is not null);
        ClearAppCommand = Async(() => ChangeAppAsync("clear"), () => CanWriteTargets() && SelectedApp is not null);
        UninstallAppCommand = Async(() => ChangeAppAsync("uninstall"), () => CanWriteTargets() && SelectedApp is not null);
        TextCommand = Async(SendTextAsync, () => CanWriteTargets() && !string.IsNullOrWhiteSpace(TextInput));
        StartTransfersCommand = Async(StartTransfersAsync, () => !Busy && !Transferring && Transfers.Any(PendingTransfer));
        CancelTransfersCommand = Action(() => transferCancellation?.Cancel(), () => Transferring);
        StartScriptCommand = Async(StartScriptAsync, () => !Busy && !Transferring && package is not null && Targets().Any() && Targets().All(row => row.Info.Online) && Targets().Any(row => !ScriptBusy(row.Serial)));
        StopScriptCommand = Action(StopScripts, () => Targets().Any(row => ScriptBusy(row.Serial)));
        CancelScriptJobCommand = Action(CancelSelectedScriptJob, () => SelectedScriptJob?.CanCancel == true);
        StartApiCommand = Async(StartApisAsync, () => !Busy && (!runtime.Api.BasicRunning || !runtime.Api.AutomatorRunning));
        StopApiCommand = Async(StopApisAsync, () => !Busy && (runtime.Api.BasicRunning || runtime.Api.AutomatorRunning));
        StartDeviceApiCommand = Async(StartDeviceApisAsync, () => !Busy && Targets().Any() && Targets().All(row => row.Info.Online && !ScriptBusy(row.Serial)));
        StopDeviceApiCommand = Async(StopDeviceApisAsync, () => !Busy && Targets().Any(row => runtime.Api.DevicePorts.ContainsKey(row.Serial)));
        SaveMetadataCommand = Async(SaveMetadataAsync, () => !Busy && CurrentDevice is not null);
        SaveSettingsCommand = Async(SaveSettingsAsync, () => !Busy);
        runtime.Scripts.EntryAdded += OnScriptLog;
        runtime.Scripts.StatusChanged += OnScriptStatus;
        AttachScriptJobs();
        LocalizationService.Current.LanguageChanged += OnLanguageChanged;
    }

    public MatrixRuntime Runtime => runtime;
    internal string DataDirectory => dataDirectory;
    public bool Demo => demo;
    public string ModeLabel => LocalizationService.Current.Get(demo ? "演示设备 · 不连接真实 ADB" : "真实设备模式");
    public ObservableCollection<DeviceRow> Devices { get; } = [];
    public System.ComponentModel.ICollectionView DeviceView { get; }
    public ObservableCollection<NodeRow> Nodes { get; } = [];
    public ObservableCollection<ShellEntry> ShellHistory { get; } = [];
    public ObservableCollection<PixelSample> Colors { get; } = [];
    public ObservableCollection<AppInfo> Apps { get; } = [];
    public System.ComponentModel.ICollectionView AppsView { get; }
    public ObservableCollection<TransferRow> Transfers { get; } = [];
    public ObservableCollection<ParameterEditor> Parameters { get; } = [];
    public ObservableCollection<string> ScriptLogs { get; } = [];
    public ObservableCollection<string> Activity { get; } = [];
    public SettingsEditor Settings { get; }
    public AsyncCommand RefreshCommand { get; }
    public AsyncCommand ScreenshotCommand { get; }
    public AsyncCommand WallRefreshCommand { get; }
    public AsyncCommand DumpCommand { get; }
    public AsyncCommand ShellCommand { get; }
    public AsyncCommand RefreshAppsCommand { get; }
    public AsyncCommand LaunchAppCommand { get; }
    public AsyncCommand StopAppCommand { get; }
    public AsyncCommand ClearAppCommand { get; }
    public AsyncCommand UninstallAppCommand { get; }
    public AsyncCommand TextCommand { get; }
    public AsyncCommand StartTransfersCommand { get; }
    public ActionCommand CancelTransfersCommand { get; }
    public AsyncCommand StartScriptCommand { get; }
    public ActionCommand StopScriptCommand { get; }
    public AsyncCommand StartApiCommand { get; }
    public AsyncCommand StopApiCommand { get; }
    public AsyncCommand StartDeviceApiCommand { get; }
    public AsyncCommand StopDeviceApiCommand { get; }
    public AsyncCommand SaveMetadataCommand { get; }
    public AsyncCommand SaveSettingsCommand { get; }
    public Action? NodesChanged { get; set; }
    public Action? DeviceChanged { get; set; }
    internal Action? CurrentDebugReadyChanged { get; set; }
    internal Action<DebugSourceIdentity>? DebugSourceAdopted { get; set; }
    public Action? WallChanged { get; set; }
    public Action? NodeSelectionChanged { get; set; }
    public Func<string, string, bool> Confirm { get; set; } = (title, message) => MessageBox.Show(message, title, MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK;

    public DeviceRow? CurrentDevice
    {
        get => currentDevice;
        set
        {
            ReconcileCompletedDebugCaches();
            if (ReferenceEquals(currentDevice, value)) return;
            debugPublicationRevision++;
            Set(ref currentDevice, value);
            observedCurrentDebugSource = SnapshotCurrentDebugSource();
            DeviceName = value?.Name ?? "";
            ShowCompletedScreenshot(value);
            ClearNodes();
            Apps.Clear();
            LoadScriptLogs();
            LoadParameters();
            Changed(nameof(DeviceContext));
            RefreshCommands();
            DeviceChanged?.Invoke();
        }
    }
    public DeviceRow? MainDevice { get => mainDevice; private set { Set(ref mainDevice, value); Changed(nameof(MainContext)); WallChanged?.Invoke(); } }
    public string DeviceContext => CurrentDevice is { } row ? $"{row.Name} · {row.Serial} · {LocalizationService.Current.Get(row.State)}" : LocalizationService.Current.Get("未选择设备");
    public string MainContext => MainDevice is { } row ? $"{LocalizationService.Current.Get("主控")}: {row.Name} · {row.Serial}" : $"{LocalizationService.Current.Get("主控")}: {LocalizationService.Current.Get("未指定")}";
    public bool Busy { get => busy || externalBusy; private set { Set(ref busy, value); SyncApiWriteGuard(); RefreshCommands(); Changed(nameof(HasActiveOperations)); } }
    internal void SetExternalBusy(bool value) { externalBusy = value; Changed(nameof(Busy)); SyncApiWriteGuard(); RefreshCommands(); Changed(nameof(HasActiveOperations)); }
    private void SyncApiWriteGuard() => runtime.Api.UiWriteBusy = externalBusy || (busy && blockApiWhileBusy) || Transferring;
    public string Status { get => LocalizationService.Current.Get(status); private set => Set(ref status, value); }
    public string Error { get => error; private set => Set(ref error, value); }
    public string Counts => $"{LocalizationService.Current.Get("发现")} {Devices.Count}   {LocalizationService.Current.Get("就绪")} {Devices.Count(row => row.Info.Online)}   {LocalizationService.Current.Get("筛选")} {DeviceView.Cast<object>().Count()}   {LocalizationService.Current.Get("选中")} {Devices.Count(row => row.IsSelected)}";
    public string TargetContext => pendingLaunchSelection > 0 ? $"已选目标包含 {pendingLaunchSelection} 台未连接设备" : $"{LocalizationService.Current.Get("目标")} {Targets().Count}: {string.Join(", ", Targets().Select(row => row.Name))}";
    public BitmapSource? DebugImage { get => debugImage; private set => Set(ref debugImage, value); }
    public NodeRow? SelectedNode { get => selectedNode; set { Set(ref selectedNode, value); Changed(nameof(NodeProperties)); NodeSelectionChanged?.Invoke(); } }
    public string NodeProperties => SelectedNode is { } row ? JsonSerializer.Serialize(row.Source with { ChildNodeList = [] }, JsonDefaults.Options) : "未选中节点";
    public string ShellInput { get => shellInput; set { Set(ref shellInput, value); RefreshCommands(); } }
    public string TextInput { get => textInput; set { Set(ref textInput, value); RefreshCommands(); } }
    public string RemotePath { get => remotePath; set => Set(ref remotePath, value); }
    public bool Transferring { get => transferring; private set { Set(ref transferring, value); SyncApiWriteGuard(); RefreshCommands(); Changed(nameof(HasActiveOperations)); } }
    public double OverallProgress => Transfers.Count == 0 ? 0 : Transfers.Average(row => row.Progress);
    public bool ShowSystemApps { get => showSystemApps; set => Set(ref showSystemApps, value); }
    public AppInfo? SelectedApp { get => selectedApp; set { Set(ref selectedApp, value); RefreshCommands(); } }
    public string PackageSummary => package is { } value ? $"{value.Manifest.Name} · {value.Manifest.Version}\n{value.Manifest.Description}" : LocalizationService.Current.Get("未导入脚本");
    public bool SyncMouse { get => syncMouse; set => Set(ref syncMouse, value); }
    public bool SyncKeyboard { get => syncKeyboard; set => Set(ref syncKeyboard, value); }
    public int WallColumns { get => wallColumns; set { Set(ref wallColumns, Math.Clamp(value, 5, 20)); WallChanged?.Invoke(); } }
    public string DeviceName { get => deviceName; set => Set(ref deviceName, value); }
    public string SettingsNotice { get => settingsNotice; private set => Set(ref settingsNotice, value); }
    public string ApiSummary => $"普通接口：{(runtime.Api.RunningBasicPort is { } basic ? $"http://127.0.0.1:{basic}" : "未启动")}    自动化接口：{(runtime.Api.RunningAutomatorPort is { } automator ? $"http://127.0.0.1:{automator}" : "未启动")}";
    public bool CanUseDevice() => !Busy && CurrentDevice?.Info.Online == true;
    public bool CanWriteDevice() => CanUseDevice() && !Transferring && !ScriptBusy(CurrentDevice!.Serial);
    public bool CanWriteTargets() => !Busy && !Transferring && Targets().Count > 0 && Targets().All(row => row.Info.Online && !ScriptBusy(row.Serial));
    public bool CanControlMainInput()
    {
        if (pendingLaunchSelection > 0 && (SyncMouse || SyncKeyboard)) return false;
        var source = MainDevice ?? CurrentDevice;
        return !Busy && !Transferring && source?.Info.Online == true && !ScriptBusy(source.Serial) &&
            InputTargets(source.Serial, false).All(serial => Devices.Any(row => row.Serial == serial && row.Info.Online && !ScriptBusy(serial)));
    }
    public IReadOnlyList<DeviceRow> Targets() { if (pendingLaunchSelection > 0) return []; var selected = Devices.Where(row => row.IsSelected).ToList(); return selected.Count > 0 ? selected : CurrentDevice is null ? [] : [CurrentDevice]; }
    internal void SetPendingLaunchSelection(int count)
    {
        if (pendingLaunchSelection == count) return;
        pendingLaunchSelection = count; RefreshCommands();
    }

    private AsyncCommand Async(Func<Task> action, Func<bool> canExecute) { var command = new AsyncCommand(action, canExecute); commands.Add(command); return command; }
    private ActionCommand Action(Action action, Func<bool> canExecute) { var command = new ActionCommand(action, canExecute); commands.Add(command); return command; }
    private void RefreshCommands() { foreach (var command in commands) { if (command is AsyncCommand asyncCommand) asyncCommand.Refresh(); else if (command is ActionCommand actionCommand) actionCommand.Refresh(); } Changed(nameof(TargetContext)); }
    private bool ScriptBusy(string serial) => runtime.Scripts.GetStatus(serial).State.ToString() is "Running" or "Cancelling";
    private void RefreshDeviceView() { DeviceView.Refresh(); Changed(nameof(Counts)); WallChanged?.Invoke(); }
    public void UpdateSelection() { Changed(nameof(Counts)); RefreshCommands(); }

    public async Task ExecuteAsync(string label, Func<Task> action, bool blocksApiWrites = true)
    {
        if (Busy) return;
        blockApiWhileBusy = blocksApiWrites; Busy = true;
        Error = "";
        Status = $"{label}…";
        try { await action(); Status = $"{label}完成"; AddActivity(Status); }
        catch (OperationCanceledException) { Status = $"{label}已停止"; AddActivity(Status); }
        catch (DebugReadFailure failure)
        {
            var message = PresentDebugError(failure.OriginalError, failure.Publication.Owner, failure.SourceSerial);
            if (!DebugPublicationIsCurrent(failure.Publication) || failure.DeviceSource is { } source && !DebugSourceIsValid(source))
            {
                Status = $"{label}已停止"; AddActivity(Status);
            }
            else { Error = $"{label}失败：{message}"; Status = "操作失败"; AddActivity(Error); }
        }
        catch (Exception ex) { Error = $"{label}失败：{PresentError(ex)}"; Status = "操作失败"; AddActivity(Error); }
        finally { Busy = false; blockApiWhileBusy = true; }
    }

    private void AddActivity(string message) { Activity.Insert(0, $"{DateTime.Now:HH:mm:ss}  {message}"); while (Activity.Count > 500) Activity.RemoveAt(Activity.Count - 1); }
    public string PresentError(Exception error)
    {
        try { runtime.Log.Write("desktop-error", error.ToString(), CurrentDevice?.Serial); }
        catch (Exception logError) { System.Diagnostics.Trace.WriteLine("界面诊断日志写入失败：" + logError); }
        return UiErrorPresentation.Describe(error);
    }
    public Task RefreshDevicesAsync() => ExecuteAsync("发现设备", () => RefreshDeviceSnapshotAsync());

    internal async Task RefreshDeviceSnapshotAsync(CancellationToken ct = default)
    {
        await runtime.RefreshAsync(ct);
        await ApplyCachedDeviceSnapshotAsync();
    }

    private async Task ApplyCachedDeviceSnapshotAsync()
    {
        var rows = runtime.GetDevices();
        foreach (var info in rows)
        {
            var row = Devices.FirstOrDefault(item => item.Serial == info.Serial);
            if (row is null)
            {
                row = new DeviceRow(info);
                row.PropertyChanged += (_, args) =>
                {
                    if (args.PropertyName == nameof(DeviceRow.IsSelected)) UpdateSelection();
                    if (args.PropertyName == nameof(DeviceRow.Info)) { ReconcileCompletedDebugCaches(); Changed(nameof(DeviceContext)); Changed(nameof(MainContext)); Changed(nameof(Counts)); RefreshCommands(); WallChanged?.Invoke(); }
                };
                Devices.Add(row);
            }
            else row.Info = info;
        }
        foreach (var row in Devices.Where(row => !rows.Any(info => info.Serial == row.Serial)).ToList()) Devices.Remove(row);
        ReconcileCompletedDebugCaches();
        if (CurrentDevice is not null && !Devices.Contains(CurrentDevice)) CurrentDevice = null;
        if (MainDevice is not null && !Devices.Contains(MainDevice)) { await runtime.Input.HandoffAsync(null); MainDevice = null; }
        RefreshApiRows();
        RefreshDeviceView();
        if (demo && CurrentDevice is null) CurrentDevice = Devices.FirstOrDefault();
        ObserveCurrentDebugReadyIdentity();
        Changed(nameof(HasActiveOperations));
    }

    internal bool ConnectionReloadAvailable => !Transferring && !Devices.Any(row => ScriptBusy(row.Serial)) && !runtime.Api.BasicRunning && !runtime.Api.AutomatorRunning && runtime.Api.DevicePorts.Count == 0 && !runtime.Api.HasActiveRequests;

    internal async Task ApplyLdConnectionsAsync(IReadOnlyList<LdConnectionProof> proofs, CancellationToken ct)
    {
        if (!ConnectionReloadAvailable) throw new DeviceException("请先停止接口服务、脚本和文件传输，再接入设备。", 409, 1003);
        runtime.RequireFrozenConnectionSettings();
        var old = runtime;
        var source = old.Backend as AdbDeviceBackend ?? throw new DeviceException("演示模式不接入真实设备。", 409, 1003);
        var service = old.LdPlayer ?? throw new DeviceException("雷电服务未配置。", 409, 1003);
        var settings = LdConnectionAdmission.Apply(old.Settings, proofs);
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(ct); deadline.CancelAfter(TimeSpan.FromSeconds(60));
        await old.Input.HandoffAsync(null, deadline.Token);
        await using (var probe = new AdbDeviceBackend(settings with { AdbAllowedSerials = proofs.Select(proof => proof.Serial).ToArray(), CommandTimeoutSeconds = 15 }))
        {
            probe.InheritServerEpoch(source);
            foreach (var proof in proofs)
                probe.RegisterIdentityGuard(proof.Serial, token => LdPlayerAdbBinding.VerifyAsync(proof.Serial, proof.VBoxPid, proof.VBoxBirth, token));
            var observed = await probe.DiscoverAsync(deadline.Token);
            foreach (var proof in proofs)
            {
                if (!observed.Any(device => device.Serial == proof.Serial && device.Online && device.TransportId == proof.TransportId)) throw new DeviceException("已核验实例的ADB连接已变化。", 409, 1003);
                while ((await probe.ShellAsync(proof.Serial, "getprop sys.boot_completed", ct: deadline.Token)).Trim() != "1") await Task.Delay(500, deadline.Token);
                PngImage.ReadDimensions(await probe.ScreenshotAsync(proof.Serial, deadline.Token));
                var current = (await service.RefreshAsync(deadline.Token)).Single(item => item.Index == proof.Index);
                if (current.Generation != proof.Generation) throw new DeviceException("接入过程中实例发生变化，未保存新授权。", 409, 1003);
            }
        }
        MatrixRuntime? replacement = null;
        try
        {
            replacement = await MatrixRuntime.CreateAsync(settings, dataDirectory, false, proofs, source, deadline.Token);
            foreach (var proof in proofs)
                if (!replacement.GetDevices().Any(device => device.Serial == proof.Serial && device.Online && device.TransportId == proof.TransportId)) throw new DeviceException("新连接未就绪，原工作区保持不变。", 409, 1003);
            if (!ConnectionReloadAvailable) throw new DeviceException("工作区出现活动任务，取消接入。", 409, 1003);
            deadline.Token.ThrowIfCancellationRequested();
            replacement.Settings = replacement.SettingsStore.Save(settings);
        }
        catch { if (replacement is not null) await replacement.DisposeAsync(); throw; }
        old.Scripts.EntryAdded -= OnScriptLog; old.Scripts.StatusChanged -= OnScriptStatus;
        DetachScriptJobs(old.Scripts);
        runtime = replacement;
        ReconcileCompletedDebugCaches();
        runtime.Scripts.EntryAdded += OnScriptLog; runtime.Scripts.StatusChanged += OnScriptStatus;
        SelectedScriptJob = null; ScriptJobs.Clear(); AttachScriptJobs();
        SyncApiWriteGuard();
        Settings.AdbSerialsJson = JsonSerializer.Serialize(runtime.Settings.AdbAllowedSerials, JsonDefaults.Options);
        Settings.LdBindingsJson = JsonSerializer.Serialize(runtime.Settings.LdSerialBindings, JsonDefaults.Options);
        Settings.Changed(nameof(SettingsEditor.AdbSerialsJson)); Settings.Changed(nameof(SettingsEditor.LdBindingsJson));
        MainDevice = null; ClearNodes(); Apps.Clear();
        try { await old.DisposeAsync(); } catch (Exception ex) { runtime.Log.Write("previous-runtime-cleanup-error", ex.ToString()); }
        await DrainScriptJobObserversAsync();
        await ApplyCachedDeviceSnapshotAsync();
        Changed(nameof(ApiSummary)); AddActivity($"已接入{proofs.Count}台雷电设备");
    }

    public Task SaveMetadataAsync() => ExecuteAsync("保存设备信息", async () =>
    {
        var row = CurrentDevice ?? throw new InvalidOperationException("未选择设备");
        runtime.Metadata.UpsertMetadata(row.Serial, DeviceName.Trim(), row.Info.GroupId);
        row.Info = row.Info with { Name = DeviceName.Trim() };
        RefreshDeviceView();
        Changed(nameof(DeviceContext));
        await Task.CompletedTask;
    });

    public Task CaptureAsync() => ExecuteAsync("刷新截图", async () =>
    {
        var publication = BeginDebugPublication();
        DebugSourceIdentity? source = null;
        try
        {
            var row = publication.Current ?? throw new InvalidOperationException("未选择设备");
            await RefreshGeometryAsync(publication, requireCurrent: true);
            RequireDebugPublication(publication);
            var identity = CaptureDebugSource(publication.Owner, row); source = identity;
            var bytes = await publication.Owner.Backend.ScreenshotAsync(identity.Info.Serial);
            PublishScreenshotCache(identity, bytes);
            RequireDebugPublication(publication);
            ClearNodes();
            ShowCompletedScreenshot(row);
            WallChanged?.Invoke();
        }
        catch (Exception error) when (error is not OperationCanceledException) { throw new DebugReadFailure(publication, source, source?.Info.Serial ?? publication.SourceSerial, error); }
    });

    public Task RefreshWallAsync() => ExecuteAsync("刷新设备墙", async () =>
    {
        var publication = BeginDebugPublication();
        try
        {
            await RefreshGeometryAsync(publication, requireCurrent: false);
            DebugSourceIdentity? updatedCurrent = null;
            foreach (var row in Devices.Where(row => row.Info.Online).ToArray())
            {
                DebugSourceIdentity? source = null; var serial = row.Serial;
                try
                {
                    source = CaptureDebugSource(publication.Owner, row);
                    var bytes = await publication.Owner.Backend.ScreenshotAsync(source.Value.Info.Serial);
                    PublishScreenshotCache(source.Value, bytes);
                    if (ReferenceEquals(row, publication.Current)) updatedCurrent = source;
                }
                catch (DebugSourceExpiredException) { RequireDebugRuntime(publication.Owner); }
                catch (Exception error)
                {
                    var message = PresentDebugError(error, publication.Owner, source?.Info.Serial ?? serial);
                    RequireDebugRuntime(publication.Owner);
                    if (source is { } failed && DebugSourceIsValid(failed)) row.ScreenState = "截图失败：" + message;
                }
            }
            if (updatedCurrent is { } current && DebugPublicationIsCurrent(publication) && DebugSourceIsValid(current))
            {
                ClearNodes();
                ShowCompletedScreenshot(current.Row);
            }
            WallChanged?.Invoke();
        }
        catch (Exception error) when (error is not OperationCanceledException) { throw new DebugReadFailure(publication, null, null, error); }
    });

    internal async Task RefreshAutomaticScreensAsync(string[] serials, CancellationToken ct)
    {
        var publication = SnapshotDebugPublication();
        var rows = serials.ToArray().Select(serial => Devices.FirstOrDefault(row => row.Serial == serial && row.Info.Online)).OfType<DeviceRow>().ToArray();
        DebugSourceIdentity? updatedCurrent = null;
        foreach (var row in rows)
        {
            ct.ThrowIfCancellationRequested();
            DebugSourceIdentity? source = null; var serial = row.Serial;
            try
            {
                source = CaptureDebugSource(publication.Owner, row);
                var bytes = await publication.Owner.Backend.ScreenshotAsync(source.Value.Info.Serial, ct);
                ct.ThrowIfCancellationRequested();
                PublishScreenshotCache(source.Value, bytes);
                if (ReferenceEquals(row, publication.Current)) updatedCurrent = source;
            }
            catch (DebugSourceExpiredException)
            {
                ct.ThrowIfCancellationRequested();
                RequireDebugRuntime(publication.Owner);
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception error)
            {
                var message = PresentDebugError(error, publication.Owner, source?.Info.Serial ?? serial, automatic: true);
                RequireDebugRuntime(publication.Owner);
                if (source is { } failed && DebugSourceIsValid(failed))
                    row.ScreenState = "截图失败：" + message;
            }
        }
        if (updatedCurrent is { } current && DebugPublicationIsCurrent(publication) && DebugSourceIsValid(current)) { ClearNodes(); ShowCompletedScreenshot(current.Row); }
        WallChanged?.Invoke();
    }

    public async Task SetMainAsync(DeviceRow row)
    {
        await ExecuteAsync("切换主控", async () => { await runtime.Input.HandoffAsync(row.Serial); MainDevice = row; CurrentDevice = row; });
    }

    public Task SendInputAsync(InputSnapshot input) => ExecuteAsync("发送输入", async () =>
    {
        if (pendingLaunchSelection > 0 && (input.Kind is InputKind.Key or InputKind.Text ? SyncKeyboard : SyncMouse)) throw new DeviceException("同步目标中存在未连接设备，请先连接或取消选择。", 409, 1003);
        if (Transferring) throw new DeviceException("文件传输进行中，完成或停止传输后再控制");
        var source = Devices.FirstOrDefault(row => row.Serial == input.SourceSerial);
        if (input.Kind is InputKind.Tap or InputKind.Swipe && source is not null &&
            (input.SourceWidth != source.Info.Width || input.SourceHeight != source.Info.Height))
            throw new DeviceException("截图与设备当前尺寸不一致，请刷新截图后重试；未发送手势");
        if (ScriptBusy(input.SourceSerial) || input.TargetSerials.Any(ScriptBusy)) throw new DeviceException("目标设备正在执行脚本，停止脚本后再控制");
        await runtime.Input.SendAsync(input);
    });

    public IReadOnlyList<string> InputTargets(string source, bool mouse) => (mouse ? SyncMouse : SyncKeyboard) ? Devices.Where(row => row.IsSelected && row.Serial != source).Select(row => row.Serial).ToArray() : [];
    public Task KeyAsync(int code)
    {
        var row = MainDevice ?? CurrentDevice;
        if (row is null) return Task.CompletedTask;
        return SendInputAsync(new InputSnapshot { SourceSerial = row.Serial, SourceGeneration = row.Info.Generation, Generation = runtime.Input.CurrentGeneration, Kind = InputKind.Key, KeyCode = code, TargetSerials = InputTargets(row.Serial, false) });
    }

    public Task SendTextAsync() => ExecuteAsync("发送文本", async () =>
    {
        var targets = RequireTargets();
        var text = TextInput;
        foreach (var row in targets) await runtime.Backend.TextAsync(row.Serial, text);
    });

    public Task LoadNodesAsync() => ExecuteAsync("刷新截图与节点", async () =>
    {
        var publication = BeginDebugPublication();
        DebugSourceIdentity? source = null;
        try
        {
            var row = publication.Current ?? throw new InvalidOperationException("未选择设备");
            await RefreshGeometryAsync(publication, requireCurrent: true);
            RequireDebugPublication(publication);
            var identity = CaptureDebugSource(publication.Owner, row); source = identity;
            RequireDebugPublication(publication);
            observedCurrentDebugSource = identity;
            DebugSourceAdopted?.Invoke(identity);
            var bytes = await publication.Owner.Backend.ScreenshotAsync(identity.Info.Serial);
            PublishScreenshotCache(identity, bytes);
            RequireDebugPublication(publication);
            ClearNodes();
            ShowCompletedScreenshot(row);
            RequireDebugSource(identity);
            RequireDebugPublication(publication);
            var freshRoot = await publication.Owner.Backend.DumpAsync(identity.Info.Serial);
            RequireDebugSource(identity);
            RequireDebugPublication(publication);
            nodeRoot = freshRoot;
            completedNodeTree = new CompletedNodeTree(identity, freshRoot);
            Nodes.Clear(); Nodes.Add(new NodeRow(nodeRoot));
            SelectedNode = null;
            row.AutomationState = "节点可用";
            NodesChanged?.Invoke();
        }
        catch (Exception error) when (error is not OperationCanceledException) { throw new DebugReadFailure(publication, source, source?.Info.Serial ?? publication.SourceSerial, error); }
    });

    private async Task RefreshGeometryAsync(DebugPublicationContext publication, bool requireCurrent)
    {
        await publication.Owner.RefreshAsync();
        RequireDebugRuntime(publication.Owner);
        if (requireCurrent) RequireDebugPublication(publication);
        var current = publication.Owner.GetDevices();
        foreach (var row in Devices.ToArray())
            row.Info = current.FirstOrDefault(device => device.Serial == row.Serial) ?? row.Info with { State = "Offline" };
    }

    // Source caches follow device identity; the current inspection additionally follows its revision.
    private readonly record struct DebugPublicationContext(MatrixRuntime Owner, DeviceRow? Current, string? SourceSerial, long Revision);
    internal readonly record struct DebugSourceIdentity(MatrixRuntime Owner, DeviceRow Row, DeviceInfo Info);
    private sealed record CompletedScreenshot(DebugSourceIdentity Source, BitmapSource Image, DateTimeOffset CapturedAt);
    private sealed record CompletedNodeTree(DebugSourceIdentity Source, AndroidNode Root);
    private sealed class DebugSourceExpiredException() : OperationCanceledException("设备身份或几何已变化，旧结果未发布。") { }
    private sealed class DebugReadFailure(DebugPublicationContext publication, DebugSourceIdentity? source, string? sourceSerial, Exception error) : Exception(error.Message, error)
    {
        internal DebugPublicationContext Publication { get; } = publication;
        internal DebugSourceIdentity? DeviceSource { get; } = source;
        internal string? SourceSerial { get; } = sourceSerial;
        internal Exception OriginalError { get; } = error;
    }
    private static string PresentDebugError(Exception error, MatrixRuntime owner, string? serial, bool automatic = false)
    {
        try
        {
            owner.Log.Write("desktop-error", error.ToString(), serial);
            if (automatic) owner.Log.Write("auto-screenshot-error", error.Message, serial);
        }
        catch (Exception logError) { System.Diagnostics.Trace.WriteLine("读取原异常：" + error + "\n界面诊断日志写入失败：" + logError); }
        return UiErrorPresentation.Describe(error);
    }
    private DebugPublicationContext SnapshotDebugPublication() => new(runtime, CurrentDevice, CurrentDevice?.Serial, debugPublicationRevision);
    private DebugPublicationContext BeginDebugPublication() { debugPublicationRevision++; return SnapshotDebugPublication(); }
    private bool DebugPublicationIsCurrent(DebugPublicationContext publication) => ReferenceEquals(runtime, publication.Owner)
        && debugPublicationRevision == publication.Revision && ReferenceEquals(CurrentDevice, publication.Current)
        && (publication.Current is null || Devices.Contains(publication.Current));
    private void RequireDebugPublication(DebugPublicationContext publication)
    {
        if (!DebugPublicationIsCurrent(publication)) throw new OperationCanceledException("检查目标或检查请求已变化，旧结果未发布。");
    }
    private void RequireDebugRuntime(MatrixRuntime owner)
    {
        if (!ReferenceEquals(runtime, owner)) throw new OperationCanceledException("设备连接上下文已变化，旧结果未发布。");
    }
    private DebugSourceIdentity CaptureDebugSource(MatrixRuntime owner, DeviceRow row)
    {
        var source = new DebugSourceIdentity(owner, row, row.Info);
        RequireDebugSource(source);
        return source;
    }
    private bool DebugSourceIsValid(DebugSourceIdentity source) => ReferenceEquals(runtime, source.Owner) && Devices.Contains(source.Row)
        && SameDebugIdentity(source.Row.Info, source.Info) && source.Owner.Devices.Snapshot.Any(info => SameDebugIdentity(info, source.Info));
    internal bool IsCurrentDebugSource(DebugSourceIdentity source) => ReferenceEquals(CurrentDevice, source.Row) && DebugSourceIsValid(source);
    private DebugSourceIdentity? SnapshotCurrentDebugSource()
    {
        if (CurrentDevice is not { } row) return null;
        var source = new DebugSourceIdentity(runtime, row, row.Info);
        return DebugSourceIsValid(source) ? source : null;
    }
    private void ObserveCurrentDebugReadyIdentity()
    {
        if (SnapshotCurrentDebugSource() is not { } source) return;
        var previous = observedCurrentDebugSource;
        observedCurrentDebugSource = source;
        if (previous is { } observed && ReferenceEquals(observed.Owner, source.Owner) && ReferenceEquals(observed.Row, source.Row)
            && SameDebugIdentity(observed.Info, source.Info)) return;
        CurrentDebugReadyChanged?.Invoke();
    }
    private static bool SameDebugIdentity(DeviceInfo actual, DeviceInfo expected) => actual.Online && expected.Online && actual.Serial == expected.Serial
        && actual.Generation == expected.Generation && actual.TransportId == expected.TransportId && actual.Width == expected.Width
        && actual.Height == expected.Height && actual.Rotation == expected.Rotation;
    private void RequireDebugSource(DebugSourceIdentity source)
    {
        if (!DebugSourceIsValid(source)) throw new DebugSourceExpiredException();
    }
    private void PublishScreenshotCache(DebugSourceIdentity source, byte[] bytes)
    {
        RequireDebugSource(source);
        var screenshot = DecodeImage(bytes);
        RequireDebugSource(source);
        var capturedAt = DateTimeOffset.UtcNow;
        completedScreenshots[source.Row] = new CompletedScreenshot(source, screenshot, capturedAt);
        source.Row.Screenshot = screenshot;
        source.Row.CapturedAt = capturedAt;
        source.Row.ScreenState = "截图可用";
    }

    private bool CompletedDebugSourceIsValid(DebugSourceIdentity source) => ReferenceEquals(runtime, source.Owner) && Devices.Contains(source.Row)
        && CompletedDebugIdentityMatches(source.Row.Info, source.Info) && source.Owner.Devices.Snapshot.Any(info => CompletedDebugIdentityMatches(info, source.Info));
    // Only completed local data may survive one observed, otherwise unchanged Offline transition.
    private static bool CompletedDebugIdentityMatches(DeviceInfo actual, DeviceInfo expected) => SameDebugIdentity(actual, expected)
        || expected.Online && actual.State == "Offline" && expected.Generation < long.MaxValue && actual.Generation == expected.Generation + 1
        && actual.Serial == expected.Serial && actual.TransportId == expected.TransportId && actual.Width == expected.Width
        && actual.Height == expected.Height && actual.Rotation == expected.Rotation;

    private void ReconcileCompletedDebugCaches()
    {
        foreach (var (row, cache) in completedScreenshots.ToArray())
        {
            var sourceValid = CompletedDebugSourceIsValid(cache.Source);
            if (sourceValid && ReferenceEquals(row.Screenshot, cache.Image)) continue;
            completedScreenshots.Remove(row);
            if (!sourceValid && row.AutomationState == "节点可用") row.AutomationState = "尚未刷新节点";
            if (!ReferenceEquals(row.Screenshot, cache.Image)) continue;
            row.Screenshot = null;
            if (row.CapturedAt == cache.CapturedAt) row.CapturedAt = null;
            if (row.ScreenState == "截图可用") row.ScreenState = "未载入";
        }
        var imageInvalid = completedDebugImage is { } image && (!ReferenceEquals(DebugImage, image.Image) || !ReferenceEquals(CurrentDevice, image.Source.Row) || !CompletedDebugSourceIsValid(image.Source));
        var nodesInvalid = completedNodeTree is { } nodes && (!ReferenceEquals(nodeRoot, nodes.Root) || !ReferenceEquals(CurrentDevice, nodes.Source.Row) || !CompletedDebugSourceIsValid(nodes.Source));
        if (!imageInvalid && !nodesInvalid) return;
        completedDebugImage = null; completedNodeTree = null;
        DebugImage = null;
        if (nodeRoot is not null || Nodes.Count != 0 || SelectedNode is not null) ClearNodes();
    }

    private void ShowCompletedScreenshot(DeviceRow? row)
    {
        completedDebugImage = row is not null && completedScreenshots.TryGetValue(row, out var cache)
            && ReferenceEquals(row.Screenshot, cache.Image) && CompletedDebugSourceIsValid(cache.Source) ? cache : null;
        DebugImage = completedDebugImage?.Image;
    }

    private void ClearNodes()
    {
        completedNodeTree = null;
        if (CurrentDevice is { } row) row.AutomationState = "尚未刷新节点";
        nodeRoot = null;
        Nodes.Clear();
        SelectedNode = null;
        NodesChanged?.Invoke();
    }

    public Task ExportNodesAsync(string path) => ExecuteAsync("导出节点", async () =>
    {
        if (nodeRoot is null) throw new InvalidOperationException("尚未刷新节点");
        ReconcileCompletedDebugCaches();
        if (completedNodeTree is not { } tree || !ReferenceEquals(nodeRoot, tree.Root)) throw new InvalidOperationException("节点来源已变化，请重新刷新节点。");
        var json = JsonSerializer.Serialize(tree.Root, JsonDefaults.Options);
        await File.WriteAllTextAsync(path, json);
    });

    public IEnumerable<NodeRow> FlattenNodes()
    {
        IEnumerable<NodeRow> Traverse(NodeRow row) { yield return row; foreach (var child in row.Children) foreach (var nested in Traverse(child)) yield return nested; }
        return Nodes.SelectMany(Traverse);
    }

    public void SamplePixel(int x, int y)
    {
        ReconcileCompletedDebugCaches();
        if (DebugImage is null || x < 0 || y < 0 || x >= DebugImage.PixelWidth || y >= DebugImage.PixelHeight) return;
        var converted = new FormatConvertedBitmap(DebugImage, PixelFormats.Bgra32, null, 0);
        var pixel = new byte[4];
        converted.CopyPixels(new Int32Rect(x, y, 1, 1), pixel, 4, 0);
        Colors.Insert(0, new PixelSample(x, y, pixel[2], pixel[1], pixel[0]));
        while (Colors.Count > 100) Colors.RemoveAt(Colors.Count - 1);
    }

    public Task SaveScreenshotAsync(string path, Int32Rect? crop = null) => ExecuteAsync(crop is null ? "保存截图" : "保存裁剪", async () =>
    {
        if (DebugImage is null) throw new InvalidOperationException("尚未加载截图");
        ReconcileCompletedDebugCaches();
        if (completedDebugImage is not { } image || !ReferenceEquals(DebugImage, image.Image)) throw new InvalidOperationException("截图来源已变化，请重新刷新截图。");
        BitmapSource source = crop is { } region ? new CroppedBitmap(image.Image, region) : image.Image;
        var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(source));
        using var memory = new MemoryStream(); encoder.Save(memory);
        await File.WriteAllBytesAsync(path, memory.ToArray());
    });

    public Task ExecuteShellAsync() => ExecuteAsync("执行 Shell", async () =>
    {
        var row = CurrentDevice ?? throw new InvalidOperationException("未选择设备");
        var command = ShellInput.Trim();
        if (command.Length == 0) return;
        if (ScriptBusy(row.Serial)) throw new DeviceException("设备正在执行脚本");
        try
        {
            var output = await runtime.Backend.ShellAsync(row.Serial, command);
            ShellHistory.Insert(0, new ShellEntry(ShellHistory.Count + 1, DateTime.Now.ToString("HH:mm:ss"), command, output, row.Serial));
        }
        catch (Exception ex)
        {
            ShellHistory.Insert(0, new ShellEntry(ShellHistory.Count + 1, DateTime.Now.ToString("HH:mm:ss"), command, "执行失败：" + PresentError(ex), row.Serial));
            throw;
        }
        finally { ShellInput = ""; }
    });

    public Task LoadAppsAsync() => ExecuteAsync("读取应用列表", async () =>
    {
        var row = CurrentDevice ?? throw new InvalidOperationException("未选择设备");
        var apps = await runtime.Backend.ListAppsAsync(row.Serial, ShowSystemApps);
        if (!ReferenceEquals(CurrentDevice, row)) return;
        Apps.Clear(); foreach (var app in apps.OrderBy(app => app.PackageName)) Apps.Add(app);
        SelectedApp = null;
    });

    public Task InstallAppAsync(string path) => ExecuteAsync("安装 APK", async () =>
    {
        var targets = RequireTargets();
        var results = new List<string>();
        foreach (var row in targets)
        {
            try { await runtime.Backend.InstallAsync(row.Serial, path); results.Add($"{row.Serial}：安装完成"); }
            catch (Exception ex) { results.Add($"{row.Serial}：失败 {PresentError(ex)}"); }
        }
        AddActivity(string.Join("；", results));
        if (results.Any(result => result.Contains("：失败", StringComparison.Ordinal))) throw new InvalidOperationException(string.Join("；", results));
    });

    private Task ChangeAppAsync(string operation) => ExecuteAsync("应用操作", async () =>
    {
        var app = SelectedApp ?? throw new InvalidOperationException("未选择应用");
        var targets = RequireTargets();
        var label = operation switch { "clear" => "清除应用数据", "uninstall" => "卸载应用", "launch" => "启动应用", _ => "停止应用" };
        if (operation is "clear" or "uninstall")
        {
            var detail = $"{label}\n包名：{app.PackageName}\n设备数量：{targets.Count}\n{string.Join("\n", targets.Select(row => $"{row.Name} ({row.Serial})"))}\n此操作会删除应用数据。";
            if (!Confirm(label, detail)) return;
        }
        var failures = new List<string>();
        foreach (var row in targets)
        {
            try
            {
                switch (operation)
                {
                    case "launch": await runtime.Backend.StartAppAsync(row.Serial, app.PackageName); break;
                    case "stop": await runtime.Backend.StopAppAsync(row.Serial, app.PackageName); break;
                    case "clear": if (!await runtime.Backend.ClearAppAsync(row.Serial, app.PackageName)) throw new InvalidOperationException("设备未确认清除完成"); break;
                    case "uninstall": await runtime.Backend.UninstallAsync(row.Serial, app.PackageName); break;
                }
                AddActivity($"{label} · {row.Serial} · {app.PackageName}");
            }
            catch (Exception ex) { failures.Add($"{row.Serial}：{PresentError(ex)}"); }
        }
        if (failures.Count > 0) throw new InvalidOperationException(string.Join("；", failures));
        if (operation == "uninstall") Apps.Remove(app);
    });

    private IReadOnlyList<DeviceRow> RequireTargets()
    {
        if (pendingLaunchSelection > 0) throw new DeviceException($"选中目标包含 {pendingLaunchSelection} 台未连接设备，请先连接或取消选择；未操作其它设备。", 409, 1003);
        if (Transferring) throw new DeviceException("文件传输进行中，完成或停止传输后再操作");
        var targets = Targets();
        if (targets.Count == 0) throw new InvalidOperationException("未选择设备");
        var invalid = targets.FirstOrDefault(row => !row.Info.Online || ScriptBusy(row.Serial));
        if (invalid is not null) throw new DeviceException($"设备 {invalid.Serial} 离线或正在执行脚本");
        return targets;
    }

    public void QueuePush(string path, bool autoInstall)
    {
        foreach (var row in RequireTargets()) Transfers.Add(new TransferRow { Serial = row.Serial, Direction = "上传", LocalPath = path, RemotePath = RemotePath.TrimEnd('/') + "/" + Path.GetFileName(path), AutoInstall = autoInstall && Path.GetExtension(path).Equals(".apk", StringComparison.OrdinalIgnoreCase) });
        RefreshCommands();
    }

    public void QueuePull(string localPath)
    {
        var row = CurrentDevice ?? throw new InvalidOperationException("未选择设备");
        Transfers.Add(new TransferRow { Serial = row.Serial, Direction = "下载", LocalPath = localPath, RemotePath = RemotePath });
        RefreshCommands();
    }

    public async Task StartTransfersAsync()
    {
        if (Busy || Transferring) return;
        transferCancellation = new CancellationTokenSource();
        Transferring = true;
        Error = "";
        try
        {
            foreach (var row in Transfers.Where(PendingTransfer).ToList())
            {
                if (transferCancellation.IsCancellationRequested) break;
                row.State = "传输中";
                row.Progress = 0;
                try
                {
                    if (ScriptBusy(row.Serial)) throw new DeviceException("设备正在执行脚本");
                    var progress = new Progress<double>(value => { row.Progress = Math.Clamp(value > 1 ? value : value * 100, 0, 100); Changed(nameof(OverallProgress)); });
                    if (row.Direction == "上传") await runtime.Backend.PushAsync(row.Serial, row.LocalPath, row.RemotePath, progress, transferCancellation.Token);
                    else await runtime.Backend.PullAsync(row.Serial, row.RemotePath, row.LocalPath, progress, transferCancellation.Token);
                    row.Progress = 100;
                    row.State = "传输完成";
                    if (row.AutoInstall) { row.State = "已传输 · 安装中"; await runtime.Backend.InstallAsync(row.Serial, row.LocalPath, transferCancellation.Token); row.State = "已传输 · 已安装"; }
                }
                catch (OperationCanceledException) { row.State = "已停止"; break; }
                catch (Exception ex) { row.State = $"失败：{PresentError(ex)}"; Error = $"传输 {row.Serial} / {row.Name}：{PresentError(ex)}"; }
            }
            Changed(nameof(OverallProgress));
        }
        finally { Transferring = false; transferCancellation.Dispose(); transferCancellation = null; }
    }

    private static bool PendingTransfer(TransferRow row) => row.State is "待传输" or "已停止" || row.State.StartsWith("失败", StringComparison.Ordinal);

    public Task ImportScriptAsync(string path) => ExecuteAsync("导入脚本", async () =>
    {
        package = ScriptPackageService.Load(path);
        LoadParameters();
        Changed(nameof(PackageSummary));
        await Task.CompletedTask;
    });

    private void LoadParameters()
    {
        Parameters.Clear();
        if (package is null) return;
        var saved = CurrentDevice is null ? null : runtime.Metadata.GetAll<ScriptDeviceParameters>("ScriptDeviceConfiguration").FirstOrDefault(value => value.Serial == CurrentDevice.Serial && value.ScriptId == package.Manifest.Id && value.Version == package.Manifest.Version);
        foreach (var parameter in package.Parameters)
        {
            var value = saved is not null && saved.Values.TryGetValue(parameter.Key, out var stored) ? stored.GetRawText() : (parameter.Value ?? parameter.DefaultValue)?.GetRawText() ?? "null";
            Parameters.Add(new ParameterEditor { Key = parameter.Key, Type = parameter.Type ?? "未声明", Label = parameter.LabelZh ?? parameter.LabelEn ?? parameter.Key, Value = value, Range = $"{parameter.Min} .. {parameter.Max} {parameter.Unit}" });
        }
    }

    private ScriptPackage ParameterizedPackage()
    {
        if (package is null) throw new InvalidOperationException("未导入脚本");
        if (package.ExecutionContract is { Version: 1 })
        {
            return package with
            {
                Parameters = package.Parameters.Select(parameter =>
                {
                    var editor = Parameters.FirstOrDefault(value => value.Key == parameter.Key);
                    if (editor is null) return parameter;
                    using var document = JsonDocument.Parse(editor.Value);
                    return parameter with { Value = document.RootElement.Clone() };
                }).ToList()
            };
        }
        var clone = JsonNode.Parse(JsonSerializer.Serialize(package, JsonDefaults.Options))!.AsObject();
        if (clone["parameters"] is JsonArray array)
        {
            foreach (var node in array.OfType<JsonObject>())
            {
                var key = node["key"]?.GetValue<string>();
                var editor = Parameters.FirstOrDefault(parameter => parameter.Key == key);
                if (editor is not null) node["value"] = JsonNode.Parse(editor.Value);
            }
        }
        return clone.Deserialize<ScriptPackage>(JsonDefaults.Options) ?? throw new InvalidOperationException("脚本参数无效");
    }

    public Task StartScriptAsync()
    {
        try
        {
            var targets = RequireScriptTargets().Select(row => new ScriptJobTarget(row.Serial, row.Info.Generation, row.Info.TransportId, demo)).ToArray();
            var selectedPackage = ScriptExecutionCompiler.Compile(ParameterizedPackage());
            var values = selectedPackage.Parameters.ToDictionary(parameter => parameter.Key, parameter => parameter.Value ?? parameter.DefaultValue ?? JsonSerializer.SerializeToElement<object?>(null));
            Error = "";
            var failures = new List<string>(); var accepted = 0;
            foreach (var target in targets)
            {
                ScriptJobHandle handle;
                try { handle = runtime.Scripts.SubmitJob(target, selectedPackage); }
                catch (Exception ex) { failures.Add($"{target.Serial} 未接纳：{PresentError(ex)}"); continue; }
                accepted++;
                RefreshScriptJobs();
                if (accepted == 1) SelectedScriptJob = ScriptJobs.First(row => row.JobId == handle.JobId);
                TrackScriptJob(runtime.Scripts, handle, target.Serial);
                try { runtime.Metadata.Put("ScriptDeviceConfiguration", target.Serial + "\u001f" + selectedPackage.Manifest.Id, new ScriptDeviceParameters(target.Serial, selectedPackage.Manifest.Id, selectedPackage.Manifest.Version, values)); }
                catch (Exception ex) { failures.Add($"{target.Serial} 任务已接纳，但参数保存失败：{PresentError(ex)}"); }
            }
            Status = $"已接纳 {accepted}/{targets.Length} 个脚本任务";
            if (failures.Count > 0) Error = "脚本提交结果：" + string.Join("；", failures);
        }
        catch (Exception ex) { Error = $"脚本未启动：{PresentError(ex)}"; }
        finally { RefreshCommands(); }
        return Task.CompletedTask;
    }

    private void StopScripts() { foreach (var row in Targets()) runtime.Scripts.Stop(row.Serial); RefreshCommands(); }
    private void OnScriptLog(string serial, ScriptLogEntry entry) => Application.Current.Dispatcher.BeginInvoke(() => { if (SelectedScriptJob is null && CurrentDevice?.Serial == serial) LoadScriptLogs(); });
    private void OnScriptStatus(ScriptRunStatus state) => Application.Current.Dispatcher.BeginInvoke(() => { var row = Devices.FirstOrDefault(row => row.Serial == state.Serial); if (row is not null) row.ScriptState = LocalizationService.Current.Get(state.State.ToString()); RefreshCommands(); Changed(nameof(HasActiveOperations)); });
    private ScriptScheduler? scriptLogSource;
    private Guid? scriptLogJob;
    private string? scriptLogSerial;
    private IReadOnlyList<ScriptLogEntry> displayedScriptLogEntries = [];
    private void LoadScriptLogs()
    {
        var source = runtime.Scripts;
        var job = SelectedScriptJob?.JobId;
        var serial = job is null ? CurrentDevice?.Serial : null;
        var entries = job is { } id ? source.GetJobLogs(id) : serial is not null ? source.GetLogs(serial) : [];
        var sameSource = ReferenceEquals(source, scriptLogSource) && job == scriptLogJob && serial == scriptLogSerial;
        var removed = 0; var retained = 0;
        if (sameSource && ScriptLogs.Count == displayedScriptLogEntries.Count && entries.Count > 0)
        {
            // Snapshots keep the original entry objects, including legitimate repeated messages.
            while (removed < displayedScriptLogEntries.Count && !ReferenceEquals(displayedScriptLogEntries[removed], entries[0])) removed++;
            retained = displayedScriptLogEntries.Count - removed;
            if (retained > entries.Count || Enumerable.Range(0, retained).Any(index => !ReferenceEquals(displayedScriptLogEntries[removed + index], entries[index]))) retained = 0;
        }
        if (retained == 0)
        {
            if (ScriptLogs.Count > 0) ScriptLogs.Clear();
        }
        else for (var index = 0; index < removed; index++) ScriptLogs.RemoveAt(0);
        for (var index = retained; index < entries.Count; index++) ScriptLogs.Add(FormatScriptLog(entries[index]));
        scriptLogSource = source; scriptLogJob = job; scriptLogSerial = serial; displayedScriptLogEntries = entries;
        if (!sameSource) Changed(nameof(ScriptLogContext));
    }
    private static string FormatScriptLog(ScriptLogEntry entry)
    {
        var message = entry.Message;
        foreach (var prefix in new[] { "执行失败：", "状态监听失败：" })
        {
            if (!message.StartsWith(prefix, StringComparison.Ordinal)) continue;
            message = prefix + UiErrorPresentation.Describe(new InvalidOperationException(message[prefix.Length..]));
            break;
        }
        return $"{entry.Timestamp:HH:mm:ss}  {message}";
    }

    public Task StartApisAsync() => ExecuteAsync("启动 API", async () => { await runtime.StartApisAsync(CancellationToken.None); RefreshApiRows(); });
    public Task StopApisAsync() => ExecuteAsync("停止 API", async () => { await runtime.StopApisAsync(CancellationToken.None); RefreshApiRows(); });
    private Task StartDeviceApisAsync() => ExecuteAsync("启动设备 API", async () => { foreach (var row in RequireTargets()) await runtime.Api.StartDeviceAsync(row.Serial, CancellationToken.None); RefreshApiRows(); });
    private Task StopDeviceApisAsync() => ExecuteAsync("停止设备 API", async () => { foreach (var row in Targets()) await runtime.Api.StopDeviceAsync(row.Serial, CancellationToken.None); RefreshApiRows(); });
    private void RefreshApiRows() { foreach (var row in Devices) row.ApiAddress = runtime.Api.DevicePorts.TryGetValue(row.Serial, out var port) ? $"http://127.0.0.1:{port}" : "未启动"; Changed(nameof(ApiSummary)); RefreshCommands(); }

    public Task SaveSettingsAsync() => ExecuteAsync("保存设置", async () =>
    {
        var settings = Settings.ToSettings() with { WallColumns = WallColumns, SyncMouse = SyncMouse, SyncKeyboard = SyncKeyboard };
        if ((runtime.Api.BasicRunning && settings.BasicApiPort != runtime.Settings.BasicApiPort) || (runtime.Api.AutomatorRunning && settings.AutomatorApiPort != runtime.Settings.AutomatorApiPort)) throw new InvalidOperationException("请先停止正在运行的 API，再修改端口");
        runtime.Settings = runtime.SettingsStore.Save(settings);
        SettingsNotice = "设置已保存；设备调试服务、自动化服务、雷电连接与监控视频组件配置重启后生效";
        AddActivity(SettingsNotice);
        await Task.CompletedTask;
    });

    public async Task ShutdownAsync()
    {
        transferCancellation?.Cancel();
        foreach (var row in Devices) runtime.Scripts.Stop(row.Serial);
        await runtime.Input.ReleaseAsync(CancellationToken.None);
        runtime.Scripts.EntryAdded -= OnScriptLog;
        runtime.Scripts.StatusChanged -= OnScriptStatus;
        DetachScriptJobs(runtime.Scripts);
        LocalizationService.Current.LanguageChanged -= OnLanguageChanged;
        await runtime.DisposeAsync();
        await DrainScriptJobObserversAsync();
    }
    private void OnLanguageChanged(object? sender, EventArgs args)
    {
        Changed(nameof(ModeLabel)); Changed(nameof(Counts)); Changed(nameof(DeviceContext)); Changed(nameof(MainContext));
        Changed(nameof(TargetContext)); Changed(nameof(PackageSummary)); Changed(nameof(ApiSummary));
        Changed(nameof(Status));
    }

    public bool HasActiveOperations => Busy || Transferring || Devices.Any(row => ScriptBusy(row.Serial));
    public static BitmapSource DecodeImage(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);
        var image = new BitmapImage(); image.BeginInit(); image.CacheOption = BitmapCacheOption.OnLoad; image.StreamSource = stream; image.EndInit(); image.Freeze(); return image;
    }
}

public sealed class ParameterEditor : ObservableObject
{
    private string value = "null";
    public required string Key { get; init; }
    public required string Type { get; init; }
    public required string Label { get; init; }
    public string Range { get; init; } = "";
    public string Value { get => value; set => Set(ref this.value, value); }
}

public sealed record ScriptDeviceParameters(string Serial, string ScriptId, string Version, Dictionary<string, JsonElement> Values);

public sealed class SettingsEditor : ObservableObject
{
    private readonly AppSettings original;
    public SettingsEditor(AppSettings settings)
    {
        original = settings;
        MonitorServerPath = settings.MonitorServerPath; MonitorFfmpegPath = settings.MonitorFfmpegPath;
        AdbPath = settings.AdbPath; AdbSerialsJson = JsonSerializer.Serialize(settings.AdbAllowedSerials, JsonDefaults.Options); ScrcpyPath = settings.ScrcpyPath; LdConsolePath = settings.LdConsolePath; LdBindingsJson = JsonSerializer.Serialize(settings.LdSerialBindings, JsonDefaults.Options); AgentBaseUrl = settings.AgentBaseUrl ?? ""; AgentSerial = settings.AgentSerial ?? "";
        BasicApiPort = settings.BasicApiPort; AutomatorApiPort = settings.AutomatorApiPort; AutoConnect = settings.AutoConnect; AutoStartApi = settings.AutoStartApi;
        Language = "zh-CN"; MainMaxSize = settings.MainMaxSize; MiniMaxSize = settings.MiniMaxSize; MainMaxFps = settings.MainMaxFps; MiniMaxFps = settings.MiniMaxFps;
        MainBitRate = settings.MainBitRate; MiniBitRate = settings.MiniBitRate; ForceHardwareDecode = settings.ForceHardwareDecode; ShowFps = settings.ShowFps;
        WindowWidth = settings.WindowWidth; WindowHeight = settings.WindowHeight; FileRoot = settings.FileRoot; CommandTimeoutSeconds = settings.CommandTimeoutSeconds;
    }
    public string AdbPath { get; set; }
    public string AdbSerialsJson { get; set; }
    public string ScrcpyPath { get; set; }
    public string MonitorServerPath { get; set; }
    public string MonitorFfmpegPath { get; set; }
    public string LdConsolePath { get; set; }
    public string LdBindingsJson { get; set; }
    public string AgentBaseUrl { get; set; }
    public string AgentSerial { get; set; }
    public int BasicApiPort { get; set; }
    public int AutomatorApiPort { get; set; }
    public bool AutoConnect { get; set; }
    public bool AutoStartApi { get; set; }
    public string Language { get; set; }
    public int MainMaxSize { get; set; }
    public int MiniMaxSize { get; set; }
    public int MainMaxFps { get; set; }
    public int MiniMaxFps { get; set; }
    public int MainBitRate { get; set; }
    public int MiniBitRate { get; set; }
    public bool ForceHardwareDecode { get; set; }
    public bool ShowFps { get; set; }
    public double WindowWidth { get; set; }
    public double WindowHeight { get; set; }
    public string FileRoot { get; set; }
    public int CommandTimeoutSeconds { get; set; }
    public AppSettings ToSettings()
    {
        if (BasicApiPort is < 1024 or > 65535 || AutomatorApiPort is < 1024 or > 65535 || BasicApiPort == AutomatorApiPort) throw new InvalidOperationException("API 端口必须为不同的 1024–65535 整数");
        if (MainMaxSize is < 64 or > 8192 || MiniMaxSize is < 64 or > 8192 || MainMaxFps is < 1 or > 240 || MiniMaxFps is < 1 or > 240) throw new InvalidOperationException("投屏尺寸或帧率超出有效范围");
        if (MainBitRate is < 10000 or > 100000000 || MiniBitRate is < 10000 or > 100000000) throw new InvalidOperationException("码率必须为 10000–100000000 bit/s");
        if (!double.IsFinite(WindowWidth) || !double.IsFinite(WindowHeight) || WindowWidth < AppSettings.MinimumWindowWidth || WindowHeight < AppSettings.MinimumWindowHeight || WindowWidth > AppSettings.MaximumWindowWidth || WindowHeight > AppSettings.MaximumWindowHeight)
            throw new InvalidOperationException($"窗体尺寸必须为 {AppSettings.MinimumWindowWidth}–{AppSettings.MaximumWindowWidth} × {AppSettings.MinimumWindowHeight}–{AppSettings.MaximumWindowHeight}");
        if (CommandTimeoutSeconds is < 1 or > 600) throw new InvalidOperationException("命令超时必须为 1–600 秒");
        var bindings = JsonSerializer.Deserialize<Dictionary<int, string>>(LdBindingsJson, JsonDefaults.Options) ?? [];
        var adbSerials = JsonSerializer.Deserialize<string[]>(AdbSerialsJson, JsonDefaults.Options) ?? throw new InvalidOperationException("ADB授权串号必须是JSON数组");
        if (adbSerials.Any(serial => string.IsNullOrWhiteSpace(serial) || serial.StartsWith('-') || serial.Contains('\0') || serial.Any(char.IsWhiteSpace)) || adbSerials.Distinct(StringComparer.Ordinal).Count() != adbSerials.Length)
            throw new InvalidOperationException("ADB授权串号必须是非空、不重复且不含空白的数组");
        if (bindings.Any(binding => binding.Key < 0 || binding.Key == 99999 || string.IsNullOrWhiteSpace(binding.Value))) throw new InvalidOperationException("雷电绑定必须使用有效实例编号与非空设备序列号");
        if (bindings.Values.Distinct(StringComparer.OrdinalIgnoreCase).Count() != bindings.Count) throw new InvalidOperationException("不同雷电实例不能绑定相同设备序列号");
        if (!string.IsNullOrWhiteSpace(AgentBaseUrl) && string.IsNullOrWhiteSpace(AgentSerial)) throw new InvalidOperationException("配置 Agent 地址时必须同时指定唯一设备 serial");
        if (!string.IsNullOrWhiteSpace(AgentBaseUrl) && !adbSerials.Contains(AgentSerial.Trim(), StringComparer.Ordinal)) throw new InvalidOperationException("Agent串号必须位于ADB授权串号数组中");
        return original with { AdbPath = AdbPath.Trim(), AdbAllowedSerials = adbSerials, ScrcpyPath = ScrcpyPath.Trim(), MonitorServerPath = MonitorServerPath.Trim(), MonitorFfmpegPath = MonitorFfmpegPath.Trim(), LdConsolePath = LdConsolePath.Trim(), LdSerialBindings = bindings, AgentBaseUrl = string.IsNullOrWhiteSpace(AgentBaseUrl) ? null : AgentBaseUrl.Trim(), AgentSerial = string.IsNullOrWhiteSpace(AgentSerial) ? null : AgentSerial.Trim(), BasicApiPort = BasicApiPort, AutomatorApiPort = AutomatorApiPort, AutoConnect = AutoConnect, AutoStartApi = AutoStartApi, Language = "zh-CN", MainMaxSize = MainMaxSize, MiniMaxSize = MiniMaxSize, MainMaxFps = MainMaxFps, MiniMaxFps = MiniMaxFps, MainBitRate = MainBitRate, MiniBitRate = MiniBitRate, ForceHardwareDecode = ForceHardwareDecode, ShowFps = ShowFps, WindowWidth = WindowWidth, WindowHeight = WindowHeight, FileRoot = FileRoot.Trim(), CommandTimeoutSeconds = CommandTimeoutSeconds };
    }
}
