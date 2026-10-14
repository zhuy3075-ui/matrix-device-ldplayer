using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using MatrixDevice.Core;

namespace MatrixDevice.Desktop;

public abstract class ObservableObject : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected bool Set<T>(ref T field, T value, [CallerMemberName] string? property = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        Changed(property);
        return true;
    }
    public void Changed([CallerMemberName] string? property = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
}

public sealed class ActionCommand(Action execute, Func<bool>? canExecute = null) : ICommand
{
    public event EventHandler? CanExecuteChanged;
    public bool CanExecute(object? parameter) => canExecute?.Invoke() ?? true;
    public void Execute(object? parameter) => execute();
    public void Refresh() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}

public sealed class AsyncCommand(Func<Task> execute, Func<bool>? canExecute = null) : ICommand
{
    private bool running;
    public event EventHandler? CanExecuteChanged;
    public bool CanExecute(object? parameter) => !running && (canExecute?.Invoke() ?? true);
    public async void Execute(object? parameter)
    { await ExecuteAsync(parameter); }
    public async Task ExecuteAsync(object? parameter = null)
    {
        if (!CanExecute(parameter)) return;
        running = true;
        Refresh();
        try { await execute(); }
        finally { running = false; Refresh(); }
    }
    public void Refresh() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}

public sealed class DeviceRow : ObservableObject
{
    private DeviceInfo info;
    private bool selected;
    private BitmapSource? screenshot;
    private string screenState = "未载入";
    private string automationState = "未连接";
    private string scriptState = "未运行";
    private string apiAddress = "未启动";
    private DateTimeOffset? capturedAt;
    public DeviceRow(DeviceInfo info) => this.info = info;
    public DeviceInfo Info { get => info; set { info = value; Changed(); Changed(nameof(Name)); Changed(nameof(Model)); Changed(nameof(Group)); Changed(nameof(State)); Changed(nameof(Resolution)); } }
    public string Serial => info.Serial;
    public string Name => string.IsNullOrWhiteSpace(info.Name) ? info.Serial : info.Name;
    public string Model => $"{info.Brand} {info.Model}".Trim();
    public string Group => string.IsNullOrWhiteSpace(info.GroupId) ? "未分组" : info.GroupId;
    public string State => LocalizationService.Current.Get(info.State);
    public string Resolution => info.Width > 0 && info.Height > 0 ? $"{info.Width} × {info.Height}" : "未核验";
    public bool IsSelected { get => selected; set => Set(ref selected, value); }
    public BitmapSource? Screenshot { get => screenshot; set => Set(ref screenshot, value); }
    public string ScreenState { get => screenState; set => Set(ref screenState, value); }
    public string AutomationState { get => automationState; set => Set(ref automationState, value); }
    public string ScriptState { get => scriptState; set => Set(ref scriptState, value); }
    public string ApiAddress { get => apiAddress; set => Set(ref apiAddress, value); }
    public DateTimeOffset? CapturedAt { get => capturedAt; set { Set(ref capturedAt, value); Changed(nameof(FrameLabel)); } }
    public string FrameLabel => capturedAt is { } at ? $"{LocalizationService.Current.Get("截图")} {at.ToLocalTime():HH:mm:ss}" : LocalizationService.Current.Get("无截图");
}

public sealed class NodeRow(AndroidNode source, int depth = 0)
{
    public AndroidNode Source { get; } = source;
    public int Depth { get; } = depth;
    public string Label => string.IsNullOrEmpty(Source.Text) ? Source.ClassName?.Split('.').Last() ?? "node" : Source.Text;
    public string Id => Source.ResourceId ?? "";
    public string Bounds => $"[{Source.Bounds.Left},{Source.Bounds.Top}][{Source.Bounds.Right},{Source.Bounds.Bottom}]";
    public ObservableCollection<NodeRow> Children { get; } = new(source.ChildNodeList.Select(node => new NodeRow(node, depth + 1)));
}

public sealed record ShellEntry(int Index, string Time, string Command, string Output, string Serial = "")
{ public string Context => $"{Time} · {Serial}"; }
public sealed record PixelSample(int X, int Y, byte R, byte G, byte B)
{
    public string Hex => $"#{R:X2}{G:X2}{B:X2}";
    public string Rgb => $"RGB({R}, {G}, {B})";
    public string Position => $"{X}, {Y}";
}

public sealed class TransferRow : ObservableObject
{
    private double progress;
    private string state = "待传输";
    public required string Serial { get; init; }
    public required string Direction { get; init; }
    public required string LocalPath { get; init; }
    public required string RemotePath { get; init; }
    public bool AutoInstall { get; init; }
    public string Name => System.IO.Path.GetFileName(LocalPath);
    public string Size => FileSize(LocalPath);
    public double Progress { get => progress; set => Set(ref progress, value); }
    public string State { get => state; set => Set(ref state, value); }
    private static string FileSize(string path)
    {
        try { return System.IO.File.Exists(path) ? $"{new System.IO.FileInfo(path).Length / 1024d:F1} KiB" : "待下载"; }
        catch (Exception) { return "未核验"; }
    }
}
