using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace MatrixDevice.Core;

public sealed class DemoDeviceBackend : IDeviceBackend
{
    private sealed class DeviceState
    {
        public required DeviceInfo Info { get; set; }
        public int Count;
        public string Text = "DEMO INPUT";
        public string ActiveApp = "com.matrix.demo";
        public bool Enabled = true;
        public Dictionary<string, byte[]> Files { get; } = new(StringComparer.Ordinal);
        public Dictionary<string, string> Properties { get; } = new(StringComparer.Ordinal);
        public HashSet<string> Apps { get; } = new(StringComparer.Ordinal) { "com.matrix.demo", "com.android.settings" };
    }
    private readonly SerialCommandQueue _queue = new();
    private readonly ConcurrentDictionary<string, DeviceState> _devices = new(StringComparer.Ordinal);
    public bool IsSimulation => true;

    public DemoDeviceBackend()
    {
        foreach (var (serial, width, height, model) in new[] { ("DEMO-001", 360, 640, "演示设备 A"), ("DEMO-002", 412, 720, "演示设备 B") })
            _devices[serial] = new DeviceState { Info = new DeviceInfo { Serial = serial, Name = model + "（模拟）", Brand = "演示", Model = model, AndroidVersion = "模拟", State = "Ready", ConnectionType = "Simulation", Width = width, Height = height, Rotation = 0 } };
    }
    public void SetOnline(string serial, bool online)
    {
        if (!_devices.TryGetValue(serial, out var device)) throw new DeviceException($"Demo device not found: {serial}.", 404, 1002);
        lock (device) device.Info = device.Info with { State = online ? "Ready" : "Offline", Error = online ? null : "Demo disconnect" };
    }
    public void SetDimensions(string serial, int width, int height)
    {
        if (width <= 0 || height <= 0 || (long)width * height > 24_000_000) throw new DeviceException($"Invalid demo dimensions: {width}x{height}.", 400, 1007);
        var device = Get(serial);
        lock (device) device.Info = device.Info with { Width = width, Height = height };
    }
    public void SetRotation(string serial, int degrees)
    {
        if (degrees is not (0 or 90 or 180 or 270)) throw new DeviceException($"Invalid demo rotation: {degrees}.", 400, 1007);
        var device = Get(serial);
        lock (device)
        {
            var wasLandscape = device.Info.Rotation is 90 or 270;
            var isLandscape = degrees is 90 or 270;
            device.Info = device.Info with
            {
                Rotation = degrees,
                Width = wasLandscape == isLandscape ? device.Info.Width : device.Info.Height,
                Height = wasLandscape == isLandscape ? device.Info.Height : device.Info.Width
            };
        }
    }
    public Task<IReadOnlyList<DeviceInfo>> DiscoverAsync(CancellationToken ct = default)
    { ct.ThrowIfCancellationRequested(); return Task.FromResult<IReadOnlyList<DeviceInfo>>(_devices.Values.Select(device => { lock (device) return device.Info; }).OrderBy(device => device.Serial).ToArray()); }
    private DeviceState Get(string serial)
    {
        if (!_devices.TryGetValue(serial, out var device)) throw new DeviceException($"Demo device not found: serial={serial}.", 404, 1002);
        if (!device.Info.Online) throw new DeviceException($"Demo device offline: serial={serial}.", 410, 1003);
        return device;
    }
    private Task<T> Run<T>(string serial, Func<DeviceState, T> action, CancellationToken ct) => _queue.RunAsync(serial, token => { token.ThrowIfCancellationRequested(); var device = Get(serial); lock (device) return Task.FromResult(action(device)); }, ct);
    private Task Run(string serial, Action<DeviceState> action, CancellationToken ct) => Run(serial, device => { action(device); return true; }, ct);

    public Task<string> ShellAsync(string serial, string command, bool useRoot = false, CancellationToken ct = default) => Run(serial, device =>
    {
        var input = command.Trim();
        if (input == "wm size") return $"Physical size: {device.Info.Width}x{device.Info.Height}";
        if (input.StartsWith("getprop ", StringComparison.Ordinal)) return device.Properties.GetValueOrDefault(input[8..].Trim().Trim('\'', '"'), "");
        if (input == "getprop") return "[ro.product.model]: [" + device.Info.Model + "]\n[ro.build.version.release]: [Demo]";
        if (input.StartsWith("setprop ", StringComparison.Ordinal))
        { var split = input[8..].Split(' ', 2); if (split.Length != 2) throw new DeviceException("Demo setprop requires key and value.", 400, 1007); device.Properties[split[0].Trim('\'', '"')] = split[1].Trim('\'', '"'); return ""; }
        if (input == "id" || input == "id -u") return useRoot ? "0 (simulation)" : "2000 (simulation)";
        if (input.StartsWith("pm list packages", StringComparison.Ordinal)) return string.Join('\n', device.Apps.Select(package => "package:" + package));
        if (input.StartsWith("am force-stop ", StringComparison.Ordinal)) { device.ActiveApp = "com.matrix.demo"; return ""; }
        if (input.StartsWith("pm clear ", StringComparison.Ordinal)) { device.Count = 0; device.Text = "DEMO INPUT"; return "Success"; }
        if (input.StartsWith("monkey -p ", StringComparison.Ordinal)) { device.ActiveApp = input.Split(' ')[2]; return "Events injected: 1 (simulation)"; }
        if (input.StartsWith("input tap ", StringComparison.Ordinal)) { device.Count++; return ""; }
        if (input.StartsWith("input swipe ", StringComparison.Ordinal)) { device.Count++; return ""; }
        if (input.StartsWith("input keyevent ", StringComparison.Ordinal)) { device.ActiveApp = "com.matrix.demo"; return ""; }
        if (input.StartsWith("input text ", StringComparison.Ordinal)) { device.Text = input[11..].Trim('\'', '"').Replace("%s", " ", StringComparison.Ordinal); return ""; }
        if (input.StartsWith("ls", StringComparison.Ordinal)) return string.Join('\n', device.Files.Keys);
        throw new DeviceException($"Demo shell does not implement: {command}. This is a simulated backend.", 501, 2001);
    }, ct);

    public Task<byte[]> ScreenshotAsync(string serial, CancellationToken ct = default) => Run(serial, device =>
    {
        var width = device.Info.Width; var height = device.Info.Height;
        var accent = serial == "DEMO-001" ? (R: (byte)20, G: (byte)150, B: (byte)132) : (R: (byte)198, G: (byte)76, B: (byte)107);
        var words = new List<(string Text, int X, int Y, int Scale)>
        {
            ("SIMULATED DEVICE", 22, 22, 2), (serial, 22, 68, 3), ("MATRIX DEMO", 22, 126, 3),
            ("TAP " + device.Count, 48, 214, 4), (device.Text.ToUpperInvariant(), 26, 326, 2),
            ("SETTINGS", 30, 420, 2), ("APPS", 30, 470, 2), ("FILES", 30, 520, 2)
        };
        return PngImage.Create(width, height, (x, y) =>
        {
            (byte R, byte G, byte B) color = (242, 246, 247);
            if (y < 100) color = accent;
            else if (y > 190 && y < 282 && x > 22 && x < width - 22) color = (222, 234, 238);
            else if (y > 307 && y < 372 && x > 22 && x < width - 22) color = (255, 255, 255);
            else if (y > height - 48) color = (26, 36, 39);
            if (words.Any(word => GlyphPixel(word.Text, x - word.X, y - word.Y, word.Scale))) color = y < 100 ? ((byte)255, (byte)255, (byte)255) : ((byte)25, (byte)41, (byte)46);
            if (y > height - 28 && y < height - 24 && x > width / 2 - 42 && x < width / 2 + 42) color = (255, 255, 255);
            return color;
        });
    }, ct);

    public Task<AndroidNode> DumpAsync(string serial, CancellationToken ct = default) => Run(serial, device => new AndroidNode
    {
        ClassName = "android.widget.FrameLayout", PackageName = device.ActiveApp, Enabled = true, Bounds = new(0, 0, device.Info.Width, device.Info.Height),
        ChildNodeList =
        [
            new() { Text = "SIMULATED DEVICE " + serial, ResourceId = "com.matrix.demo:id/title", ClassName = "android.widget.TextView", PackageName = device.ActiveApp, Enabled = true, Bounds = new(22, 10, device.Info.Width - 22, 98) },
            new() { Text = "Tap " + device.Count, ResourceId = "com.matrix.demo:id/tap", ClassName = "android.widget.Button", PackageName = device.ActiveApp, Clickable = true, Enabled = true, Index = 1, Bounds = new(22, 190, device.Info.Width - 22, 282) },
            new() { Text = device.Text, ResourceId = "com.matrix.demo:id/input", ClassName = "android.widget.EditText", PackageName = device.ActiveApp, Clickable = true, Enabled = true, Focusable = true, Focused = true, Index = 2, Bounds = new(22, 307, device.Info.Width - 22, 372) },
            new() { Text = "Settings", ResourceId = "com.matrix.demo:id/settings", ClassName = "android.widget.Button", PackageName = device.ActiveApp, Clickable = true, Enabled = true, Instance = 1, Index = 3, Bounds = new(22, 402, device.Info.Width - 22, 450) }
        ]
    }, ct);
    public Task TapAsync(string serial, int x, int y, CancellationToken ct = default) => Run(serial, device => { CheckPoint(device, x, y); device.Count++; }, ct);
    public Task SwipeAsync(string serial, int x1, int y1, int x2, int y2, int durationMs, CancellationToken ct = default) => Run(serial, device => { CheckPoint(device, x1, y1); CheckPoint(device, x2, y2); if (durationMs <= 0 || durationMs > 60000) throw new DeviceException("Swipe duration must be in 1..60000ms.", 400, 1007); device.Count++; }, ct);
    public Task KeyEventAsync(string serial, int code, CancellationToken ct = default) => Run(serial, device => { if (code < 0 || code > 400) throw new DeviceException("Invalid demo key code.", 400, 1007); if (code == 3) device.ActiveApp = "com.matrix.demo"; }, ct);
    public Task TextAsync(string serial, string text, CancellationToken ct = default) => Run(serial, device => device.Text = text, ct);
    public Task<string> StartAppAsync(string serial, string packageName, string? activity = null, CancellationToken ct = default) => Run(serial, device => { if (!device.Apps.Contains(packageName)) throw new DeviceException($"演示应用未安装：{packageName}", 404, 1002); device.ActiveApp = packageName; return "演示应用已启动：" + packageName; }, ct);
    public Task<string> StopAppAsync(string serial, string packageName, CancellationToken ct = default) => Run(serial, device => { device.ActiveApp = "com.matrix.demo"; return "演示应用已停止：" + packageName; }, ct);
    public Task<bool> ClearAppAsync(string serial, string packageName, CancellationToken ct = default) => Run(serial, device => { if (!device.Apps.Contains(packageName)) return false; device.Count = 0; device.Text = "DEMO INPUT"; return true; }, ct);
    public Task<IReadOnlyList<AppInfo>> ListAppsAsync(string serial, bool showSystem = false, bool needVersion = true, CancellationToken ct = default) => Run<IReadOnlyList<AppInfo>>(serial, device => device.Apps.Where(package => showSystem || package != "com.android.settings").Select(package => new AppInfo(package, package, needVersion ? "1.0-demo" : null, package == "com.android.settings")).ToArray(), ct);
    public Task InstallAsync(string serial, string localPath, CancellationToken ct = default) => Run(serial, device => { if (!File.Exists(localPath)) throw new DeviceException($"APK file does not exist: {localPath}.", 400, 1007); device.Apps.Add("com.matrix.demo.imported"); }, ct);
    public Task UninstallAsync(string serial, string packageName, CancellationToken ct = default) => Run(serial, device => { if (!device.Apps.Remove(packageName)) throw new DeviceException($"Demo package not installed: {packageName}.", 404, 1002); }, ct);
    public Task PushAsync(string serial, string localPath, string remotePath, IProgress<double>? progress = null, CancellationToken ct = default) => _queue.RunAsync(serial, async token =>
    { var bytes = await File.ReadAllBytesAsync(localPath, token); var device = Get(serial); lock (device) device.Files[remotePath] = bytes; progress?.Report(1); }, ct);
    public Task PullAsync(string serial, string remotePath, string localPath, IProgress<double>? progress = null, CancellationToken ct = default) => _queue.RunAsync(serial, async token =>
    { var device = Get(serial); byte[] bytes; lock (device) bytes = device.Files.TryGetValue(remotePath, out var file) ? file.ToArray() : throw new DeviceException($"Demo device file not found: {remotePath}.", 404, 1002); await File.WriteAllBytesAsync(localPath, bytes, token); progress?.Report(1); }, ct);
    public Task SetNodeTextAsync(string serial, NodeSelector selector, string text, CancellationToken ct = default) => Run(serial, device =>
    {
        if (selector.ResourceId is not null && selector.ResourceId != "com.matrix.demo:id/input") throw new DeviceException("Demo set-text target is not an editable node.", 409, 1003);
        if (selector.ClassName is not null && selector.ClassName != "android.widget.EditText") throw new DeviceException("Demo set-text target is not an editable node.", 409, 1003);
        if (selector.Text is not null && selector.Text != device.Text) throw new DeviceException("Demo editable node does not match selector.", 404, 1002);
        var editable = new AndroidNode { Text = device.Text, ResourceId = "com.matrix.demo:id/input", ClassName = "android.widget.EditText", PackageName = device.ActiveApp, Clickable = true, Enabled = true, Focusable = true, Focused = true, Index = 2 };
        foreach (var property in typeof(NodeSelector).GetProperties())
        {
            var expected = property.GetValue(selector);
            if (expected is null) continue;
            var matches = property.Name == nameof(NodeSelector.TextContains)
                ? editable.Text?.Contains((string)expected, StringComparison.Ordinal) == true
                : Equals(typeof(AndroidNode).GetProperty(property.Name)!.GetValue(editable), expected);
            if (!matches) throw new DeviceException("Demo editable node does not match selector: " + property.Name, 404, 1002);
        }
        device.Text = text;
    }, ct);
    private static void CheckPoint(DeviceState device, int x, int y) { if (x < 0 || y < 0 || x >= device.Info.Width || y >= device.Info.Height) throw new DeviceException($"Demo point ({x},{y}) outside {device.Info.Width}x{device.Info.Height}.", 400, 1007); }
    private static readonly Dictionary<char, string> Glyphs = new()
    {
        ['A']="01110100011000111111100011000110001",['B']="11110100011000111110100011000111110",['C']="01111100001000010000100001000001111",['D']="11110100011000110001100011000111110",['E']="11111100001000011110100001000011111",['F']="11111100001000011110100001000010000",['G']="01111100001000010111100011000101111",['H']="10001100011000111111100011000110001",['I']="11111001000010000100001000010011111",['J']="00111000100001000010100101001001100",['K']="10001100101010011000101001001010001",['L']="10000100001000010000100001000011111",['M']="10001110111010110101100011000110001",['N']="10001110011010110011100011000110001",['O']="01110100011000110001100011000101110",['P']="11110100011000111110100001000010000",['Q']="01110100011000110001101011001001101",['R']="11110100011000111110101001001010001",['S']="01111100001000001110000010000111110",['T']="11111001000010000100001000010000100",['U']="10001100011000110001100011000101110",['V']="10001100011000110001100010101000100",['W']="10001100011000110101101011101110001",['X']="10001100010101000100010101000110001",['Y']="10001100010101000100001000010000100",['Z']="11111000010001000100010001000011111",['0']="01110100011001110101110011000101110",['1']="00100011000010000100001000010001110",['2']="01110100010000100010001000100011111",['3']="11110000010000101110000010000111110",['4']="00010001100101010010111110001000010",['5']="11111100001000011110000010000111110",['6']="01110100001000011110100011000101110",['7']="11111000010001000100010000100001000",['8']="01110100011000101110100011000101110",['9']="01110100011000101111000010000101110",['-']="00000000000000011111000000000000000",[' ']="00000000000000000000000000000000000"
    };
    private static bool GlyphPixel(string text, int x, int y, int scale)
    {
        if (x < 0 || y < 0 || y >= 7 * scale) return false;
        var character = x / (6 * scale); if (character >= text.Length || x % (6 * scale) >= 5 * scale) return false;
        if (!Glyphs.TryGetValue(text[character], out var pattern)) return false;
        return pattern[(y / scale) * 5 + x % (6 * scale) / scale] == '1';
    }
    public ValueTask DisposeAsync() => _queue.DisposeAsync();
}
