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
    public int? Index => Instance?.Index;
    public string Number => Index?.ToString() ?? "-";
    public string Name => Device?.Name ?? Instance?.Name ?? "";
    public string State => Connected ? "已连接" : phase is not ("" or "已连接") ? phase : Instance?.Index == 0 ? "受保护" : Instance?.IsStarting == true ? "启动中" : Instance?.AndroidStarted == true ? "待连接" : Instance is not null ? "已关闭" : Device?.State ?? "离线";
    public string Model => Device?.Info.Model ?? "-";
    public string Brand => Device?.Info.Brand ?? "-";
    public string Android => Device?.Info.AndroidVersion ?? "-";
    public string Serial => Device?.Serial ?? "-";
    public string Resolution => Device?.Resolution ?? (Instance?.Width is { } width && Instance.Height is { } height ? $"{width} × {height}" : "-");
    public string Group => Device?.Group ?? "未分组";
    public bool CanSelect => Index is not (0 or 99999);
    public bool IsSelected
    {
        get => Device?.IsSelected ?? selected;
        set { if (!CanSelect) return; selected = value; if (Device is not null) Device.IsSelected = value; Changed(); }
    }
    public string PrimaryLabel => Connected ? "调试" : Instance?.AndroidStarted == true ? "连接" : "启动";
    public string PrimaryGlyph => Connected ? "\uE946" : "\uE768";
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

    public void Update(LdPlayerInstance? instance, DeviceRow? device, bool connected, string operationPhase)
    {
        var retained = IsSelected;
        Instance = instance; Device = device; Connected = connected; phase = operationPhase;
        if (CanSelect && retained && device is not null && !device.IsSelected) device.IsSelected = true;
        selected = retained; Changed(""); RefreshCommands();
    }

    public void RefreshCommands() { PrimaryCommand.Refresh(); ShowWindowCommand.Refresh(); MainCommand.Refresh(); }
}
