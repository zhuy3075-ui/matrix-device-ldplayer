using MatrixDevice.Core;

namespace MatrixDevice.Desktop;

public sealed class UnifiedDeviceRow : ObservableObject
{
    private bool selected;
    public string Key { get; }
    public LdPlayerInstance? Instance { get; private set; }
    public DeviceRow? Device { get; private set; }
    public bool Connected { get; private set; }
    private string phase = "";
    private string? configuredSerial;
    public int? Index => Instance?.Index;
    public string Number => Index?.ToString() ?? "-";
    public string Name => Device?.Name ?? Instance?.Name ?? "";
    public string State => FilterState;
    public string StateDetails => $"运行状态：{FilterState}\n设备原始状态：{Device?.Info.State ?? "无设备连接"}\n操作阶段：{(string.IsNullOrWhiteSpace(phase) ? "无" : phase)}";
    public string FilterState
    {
        get
        {
            if (Index is 0 or 99999) return "受保护";
            if (Instance is { } instance)
            {
                if (instance.IsStarting) return "启动中";
                var stopped = !instance.AndroidStarted && instance.ProcessId <= 0 && instance.VBoxProcessId <= 0 && instance.TopWindow == 0 && instance.BindWindow == 0;
                if (stopped) return !Connected && (Device is null || Device.Info.State == "Offline") ? "已关闭" : "未知";
                if (!instance.AndroidStarted || instance.ProcessId <= 0 || instance.VBoxProcessId <= 0) return "未知";
            }
            if (Connected && Device?.Info.Online != true) return "未知";
            return Device?.Info.State switch
            {
                "Ready" when Connected => "已连接",
                "Ready" when Instance is not null => "待连接",
                "Offline" => "离线", "Unauthorized" => "未授权", "Discovered" => "已发现", "Error" => "异常",
                null when Instance?.AndroidStarted == true => "待连接",
                _ => "未知"
            };
        }
    }
    public string Model => Device?.Info.Model ?? "-";
    public string Brand => Device?.Info.Brand ?? "-";
    public string Android => Device?.Info.AndroidVersion ?? "-";
    public string Serial => Device?.Serial ?? "-";
    public string Resolution => Device?.Resolution ?? (Instance?.Width is { } width && Instance.Height is { } height ? $"{width} × {height}" : "-");
    public string Group => Device?.Group ?? "未分组";
    public string AndroidState => Instance is null ? "-" : Instance.AndroidStarted ? "已启动" : "未启动";
    public string ProcessId => Instance?.ProcessId is > 0 ? Instance.ProcessId.ToString() : "-";
    public string VBoxProcessId => Instance?.VBoxProcessId is > 0 ? Instance.VBoxProcessId.ToString() : "-";
    public string Dpi => Instance?.Dpi?.ToString() ?? "-";
    public string Generation => Instance?.Generation.ToString() ?? "-";
    public string Binding => Instance is null ? "-" : Connected ? Serial + "（已核验）" : string.IsNullOrWhiteSpace(configuredSerial) ? "尚未绑定" : configuredSerial + "（配置绑定，尚未核验）";
    public bool CanSelect => Index is not (0 or 99999);
    public bool IsSelected
    {
        get => Device?.IsSelected ?? selected;
        set { if (!CanSelect) return; selected = value; if (Device is not null) Device.IsSelected = value; Changed(); }
    }
    public string PrimaryLabel => Connected && Device is not null ? "调试" : Index is 0 or 99999 ? "受保护"
        : FilterState == "已关闭" ? "启动" : Instance?.IsStarting == true ? "等待连接"
        : FilterState == "未知" ? "接入" : Instance?.AndroidStarted == true ? "连接" : "接入";
    private const string AdmissionDescription = "重新检查实例；明确关闭时尝试启动，过渡或信息不完整时等待，核验ADB、安卓启动与截图后连接；可能失败或超时。";
    public string PrimaryDescription => PrimaryLabel switch
    {
        "调试" => "打开此设备的界面检查，不启动或重启模拟器。",
        "受保护" => "受保护编号；此入口不自动启动或接入该实例。",
        "启动" => "当前观察为已关闭。" + AdmissionDescription,
        "等待连接" => "当前观察为启动中。" + AdmissionDescription,
        "连接" => "当前观察为安卓已启动。" + AdmissionDescription,
        _ => "当前状态尚未确认。" + AdmissionDescription
    };
    public string PrimaryGlyph => PrimaryLabel == "调试" ? "\uE946" : "\uE768";
    public AsyncCommand PrimaryCommand { get; }
    public AsyncCommand ShowWindowCommand { get; }
    public AsyncCommand MainCommand { get; }

    public UnifiedDeviceRow(string key, Func<UnifiedDeviceRow, Task> activate, Func<UnifiedDeviceRow, bool> canActivate, Func<UnifiedDeviceRow, Task> showWindow, Func<UnifiedDeviceRow, bool> canShow, Func<UnifiedDeviceRow, Task> setMain)
    {
        Key = key;
        PrimaryCommand = new AsyncCommand(() => activate(this), () => canActivate(this));
        ShowWindowCommand = new AsyncCommand(() => showWindow(this), () => canShow(this));
        MainCommand = new AsyncCommand(() => setMain(this), () => Connected && Device is not null && canActivate(this));
    }

    public void Update(LdPlayerInstance? instance, DeviceRow? device, bool connected, string operationPhase, string? configuredBinding = null)
    {
        var retained = IsSelected;
        Instance = instance; Device = device; Connected = connected; phase = operationPhase; configuredSerial = configuredBinding;
        if (CanSelect && retained && device is not null && !device.IsSelected) device.IsSelected = true;
        selected = retained; Changed(""); RefreshCommands();
    }

    public void RefreshCommands() { PrimaryCommand.Refresh(); ShowWindowCommand.Refresh(); MainCommand.Refresh(); }
}
