using System.Text.Json;
using System.Text.Json.Serialization;

namespace MatrixDevice.Core;

public static class JsonDefaults
{
    public static JsonSerializerOptions Options { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true
    };
}

public sealed class DeviceException(string message, int status = 409, int code = 1003) : Exception(message)
{
    public int Status { get; } = status;
    public int Code { get; } = code;
}

public sealed record DeviceInfo
{
    public required string Serial { get; init; }
    public string Name { get; init; } = "";
    public string Brand { get; init; } = "";
    public string Model { get; init; } = "";
    public string AndroidVersion { get; init; } = "";
    public string State { get; init; } = "Discovered";
    public string ConnectionType { get; init; } = "USB";
    public string GroupId { get; init; } = "";
    public int Width { get; init; }
    public int Height { get; init; }
    public int? Rotation { get; init; }
    public long Generation { get; init; } = 1;
    public uint? TransportId { get; init; }
    public string? Error { get; init; }
    public bool Online => State == "Ready";
}

public sealed record NodeBounds(int Left, int Top, int Right, int Bottom)
{
    public int Width => Right - Left;
    public int Height => Bottom - Top;
    public int CenterX => Left + Width / 2;
    public int CenterY => Top + Height / 2;
}

public sealed record AndroidNode
{
    public string? Text { get; init; }
    public string? ResourceId { get; init; }
    public string? ClassName { get; init; }
    public string? PackageName { get; init; }
    public string? ContentDesc { get; init; }
    public int Index { get; init; }
    public int Instance { get; init; }
    public bool Clickable { get; init; }
    public bool Enabled { get; init; }
    public bool Focusable { get; init; }
    public bool Focused { get; init; }
    public bool Scrollable { get; init; }
    public bool LongClickable { get; init; }
    public bool Checkable { get; init; }
    public bool Checked { get; init; }
    public bool Selected { get; init; }
    public bool VisibleToUser { get; init; } = true;
    public NodeBounds Bounds { get; init; } = new(0, 0, 0, 0);
    public List<AndroidNode> ChildNodeList { get; init; } = [];
}

public sealed record NodeSelector
{
    public string? Text { get; init; }
    public string? TextContains { get; init; }
    public string? ResourceId { get; init; }
    public string? ClassName { get; init; }
    public string? PackageName { get; init; }
    public int? Instance { get; init; }
    public int? Index { get; init; }
    public bool? Clickable { get; init; }
    public bool? Enabled { get; init; }
    public bool? Focusable { get; init; }
    public bool? Focused { get; init; }
    public bool? Scrollable { get; init; }
    public bool? LongClickable { get; init; }
    public bool? Checkable { get; init; }
    public bool? Checked { get; init; }
    public bool? Selected { get; init; }
    public bool? VisibleToUser { get; init; }
}

public sealed record AppInfo(string PackageName, string? Name = null, string? VersionName = null, bool IsSystem = false);

public interface IDeviceBackend : IAsyncDisposable
{
    Task<IReadOnlyList<DeviceInfo>> DiscoverAsync(CancellationToken ct = default);
    Task<string> ShellAsync(string serial, string command, bool useRoot = false, CancellationToken ct = default);
    Task<byte[]> ScreenshotAsync(string serial, CancellationToken ct = default);
    Task<AndroidNode> DumpAsync(string serial, CancellationToken ct = default);
    Task TapAsync(string serial, int x, int y, CancellationToken ct = default);
    Task SwipeAsync(string serial, int x1, int y1, int x2, int y2, int durationMs, CancellationToken ct = default);
    Task KeyEventAsync(string serial, int code, CancellationToken ct = default);
    Task TextAsync(string serial, string text, CancellationToken ct = default);
    Task<string> StartAppAsync(string serial, string packageName, string? activity = null, CancellationToken ct = default);
    Task<string> StopAppAsync(string serial, string packageName, CancellationToken ct = default);
    Task<bool> ClearAppAsync(string serial, string packageName, CancellationToken ct = default);
    Task<IReadOnlyList<AppInfo>> ListAppsAsync(string serial, bool showSystem = false, bool needVersion = true, CancellationToken ct = default);
    Task InstallAsync(string serial, string localPath, CancellationToken ct = default);
    Task UninstallAsync(string serial, string packageName, CancellationToken ct = default);
    Task PushAsync(string serial, string localPath, string remotePath, IProgress<double>? progress = null, CancellationToken ct = default);
    Task PullAsync(string serial, string remotePath, string localPath, IProgress<double>? progress = null, CancellationToken ct = default);
    Task SetNodeTextAsync(string serial, NodeSelector selector, string text, CancellationToken ct = default);
}

public sealed record AppSettings
{
    public const double MinimumWindowWidth = 390;
    public const double MinimumWindowHeight = 640;
    public const double MaximumWindowWidth = 7680;
    public const double MaximumWindowHeight = 4320;
    public const double DefaultWindowWidth = 1440;
    public const double DefaultWindowHeight = 900;
    public string LdConsolePath { get; init; } = "";
    public Dictionary<int, string> LdSerialBindings { get; init; } = [];
    public string AdbPath { get; init; } = "";
    public string MonitorServerPath { get; init; } = "";
    public string MonitorFfmpegPath { get; init; } = "";
    public string[] AdbAllowedSerials { get; init; } = [];
    public string ScrcpyPath { get; init; } = "";
    public string? AgentBaseUrl { get; init; }
    public string? AgentSerial { get; init; }
    public int BasicApiPort { get; init; } = 8080;
    public int AutomatorApiPort { get; init; } = 17171;
    public bool AutoStartApi { get; init; }
    public bool AutoConnect { get; init; }
    public bool SyncMouse { get; init; }
    public bool SyncKeyboard { get; init; }
    public int WallColumns { get; init; } = 5;
    public int MainMaxSize { get; init; } = 1280;
    public int MiniMaxSize { get; init; } = 240;
    public int MainMaxFps { get; init; } = 45;
    public int MiniMaxFps { get; init; } = 25;
    public int MainBitRate { get; init; } = 6000000;
    public int MiniBitRate { get; init; } = 100000;
    public bool ForceHardwareDecode { get; init; }
    public bool ShowFps { get; init; }
    public string Language { get; init; } = "zh-CN";
    public double WindowWidth { get; init; } = DefaultWindowWidth;
    public double WindowHeight { get; init; } = DefaultWindowHeight;
    public string FileRoot { get; init; } = "/sdcard/MatrixExchange";
    public int CommandTimeoutSeconds { get; init; } = 30;
}
