using System.Collections.Concurrent;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace MatrixDevice.Core;

public sealed class AdbDeviceBackend : IDeviceBackend
{
    private readonly AppSettings _settings;
    private readonly ExistingAdbTransport _transport;
    private readonly HashSet<string> _allowedSerials;
    private readonly SerialCommandQueue _queue = new();
    private readonly SemaphoreSlim _discoveryGate = new(1, 1);
    private readonly ConcurrentDictionary<string, PhoneAgentClient> _agents = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, Func<string, CancellationToken, Task>> _unicodeInputs = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, Func<CancellationToken, Task>> _identityGuards = new(StringComparer.Ordinal);
    private readonly object _videoGate = new();
    private readonly CancellationTokenSource _videoShutdown = new();
    private readonly Dictionary<string, TaskCompletionSource> _videoLeases = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _blockedVideos = new(StringComparer.Ordinal);
    public const int MaximumStdoutBytes = 32 * 1024 * 1024;
    public const int MaximumStderrBytes = 1024 * 1024;
    public const string MonitorServerVersion = "3.3.4";
    public const string MonitorServerSha256 = "8588238c9a5a00aa542906b6ec7e6d5541d9ffb9b5d0f6e1bc0e365e2303079e";
    private int _disposed;
    public AdbDeviceBackend(AppSettings settings, IPEndPoint? serverEndpoint = null)
    {
        _settings = settings;
        _allowedSerials = settings.AdbAllowedSerials.ToHashSet(StringComparer.Ordinal);
        foreach (var serial in _allowedSerials) ValidateSerial(serial);
        var endpoint = serverEndpoint ?? new IPEndPoint(IPAddress.Loopback, 5037);
        if (!IPAddress.IsLoopback(endpoint.Address)) throw new ArgumentException("ADB server must be on loopback.", nameof(serverEndpoint));
        _transport = new ExistingAdbTransport(settings, endpoint);
    }

    public Task<string> ConnectNetworkAsync(string ip, int port, CancellationToken ct = default)
    {
        if (!IPAddress.TryParse(ip, out var address) || address.AddressFamily != AddressFamily.InterNetwork || port is < 1 or > 65535)
            throw new DeviceException($"Invalid ADB network endpoint: {ip}:{port}.", 400, 1007);
        var endpoint = address + ":" + port.ToString(CultureInfo.InvariantCulture);
        return _queue.RunAsync("$adb-network-control", async token =>
        {
            var output = Decode(await ExecuteAsync(null, ["connect", endpoint], token).ConfigureAwait(false));
            var lines = output.Split('\n').Select(line => line.Trim()).ToArray();
            if (!lines.Any(line => line.Equals("connected to " + endpoint, StringComparison.OrdinalIgnoreCase) || line.Equals("already connected to " + endpoint, StringComparison.OrdinalIgnoreCase)))
                throw new DeviceException($"ADB did not confirm connection to {endpoint}: {output}", 503, 2000);
            return output;
        }, ct);
    }

    public void RegisterAgent(string serial, PhoneAgentClient client)
    {
        AuthorizeSerial(serial); ArgumentNullException.ThrowIfNull(client);
        if (!_agents.TryAdd(serial, client)) throw new DeviceException($"An agent is already registered for serial={serial}.");
    }
    public void RegisterUnicodeInput(string serial, Func<string, CancellationToken, Task> send)
    {
        AuthorizeSerial(serial); ArgumentNullException.ThrowIfNull(send);
        if (!_unicodeInputs.TryAdd(serial, send)) throw new DeviceException($"Unicode input is already bound: serial={serial}.");
    }
    public void RegisterIdentityGuard(string serial, Func<CancellationToken, Task> verify)
    {
        AuthorizeSerial(serial); ArgumentNullException.ThrowIfNull(verify);
        if (!_identityGuards.TryAdd(serial, verify)) throw new DeviceException($"An identity guard is already bound: serial={serial}.", 409, 1003);
    }
    public void InheritServerEpoch(AdbDeviceBackend source) => _transport.InheritServerEpoch(source._transport);
    public Task VerifyIdentityAsync(string serial, CancellationToken ct = default)
    {
        AuthorizeSerial(serial);
        if (!_identityGuards.ContainsKey(serial)) throw new DeviceException("设备身份尚未绑定。", 403, 1003);
        return RunDeviceAsync(serial, async token => { await ExecuteAsync(null, ["devices", "-l"], token).ConfigureAwait(false); }, ct);
    }

    public async Task<IReadOnlyList<DeviceInfo>> DiscoverAsync(CancellationToken ct = default)
    {
        await _discoveryGate.WaitAsync(ct).ConfigureAwait(false);
        try { return await DiscoverCoreAsync(ct).ConfigureAwait(false); }
        finally { _discoveryGate.Release(); }
    }
    public async Task<IReadOnlyList<AdbRegisteredDevice>> ListRegisteredDevicesAsync(CancellationToken ct = default)
    {
        var output = Decode(await ExecuteAsync(null, ["devices", "-l"], ct).ConfigureAwait(false));
        return ParseRegisteredDevices(output);
    }
    private static IReadOnlyList<AdbRegisteredDevice> ParseRegisteredDevices(string output)
    {
        var devices = new List<AdbRegisteredDevice>();
        foreach (var line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var value = line.Trim();
            if (value.StartsWith("List of devices", StringComparison.Ordinal) || value.StartsWith('*') || value.Length == 0) continue;
            var fields = Regex.Split(value, @"\s+");
            if (fields.Length < 2) throw new DeviceException("Malformed ADB host directory row.", 502, 2000);
            ValidateSerial(fields[0]);
            var identity = fields.FirstOrDefault(field => field.StartsWith("transport_id:", StringComparison.Ordinal));
            uint? transport = uint.TryParse(identity?[13..], NumberStyles.None, CultureInfo.InvariantCulture, out var number) && number > 0 ? number : null;
            devices.Add(new(fields[0], fields[1], transport));
        }
        return devices.AsReadOnly();
    }
    private async Task<IReadOnlyList<DeviceInfo>> DiscoverCoreAsync(CancellationToken ct)
    {
        try
        {
        var result = await ExecuteAsync(null, ["devices", "-l"], ct).ConfigureAwait(false);
        var discovered = new List<DeviceInfo>();
        foreach (var line in Decode(result).Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var value = line.Trim();
            if (value.StartsWith("List of devices", StringComparison.Ordinal) || value.StartsWith('*') || value.Length == 0) continue;
            var fields = Regex.Split(value, @"\s+");
            if (fields.Length < 2) throw new DeviceException($"Malformed ADB discovery row: {value}", 503, 2000);
            var serial = fields[0]; ValidateSerial(serial);
            if (!_allowedSerials.Contains(serial)) continue;
            var state = fields[1] switch { "device" => "Ready", "offline" => "Offline", "unauthorized" => "Unauthorized", _ => "Failed" };
            var identity = fields.FirstOrDefault(field => field.StartsWith("transport_id:", StringComparison.Ordinal));
            uint? transport = uint.TryParse(identity?[13..], NumberStyles.None, CultureInfo.InvariantCulture, out var id) && id > 0 ? id : null;
            if (state == "Ready" && transport is null) state = "Failed";
            var model = fields.FirstOrDefault(field => field.StartsWith("model:", StringComparison.Ordinal))?[6..].Replace('_', ' ') ?? serial;
            var device = new DeviceInfo { Serial = serial, Name = model, Model = model, State = state, TransportId = transport, ConnectionType = serial.StartsWith("emulator-", StringComparison.Ordinal) ? "Emulator" : serial.Contains(':') ? "TCP" : "USB", Error = state == "Ready" ? null : $"ADB state: {fields[1]}; transport_id={identity ?? "missing"}" };
            discovered.Add(device);
        }
        _transport.ReplaceTransports(discovered.Where(device => device.Online).ToDictionary(device => device.Serial, device => device.TransportId!.Value, StringComparer.Ordinal));
        var devices = new List<DeviceInfo>();
        foreach (var observed in discovered)
        {
            var device = observed; var serial = observed.Serial;
            if (device.Online)
            {
                try
                {
                    device = await RunDeviceAsync(serial, async token =>
                    {
                        var props = Decode(await ExecuteAsync(serial, ["shell", "getprop"], token)).Split('\n');
                        string Get(string key) => props.FirstOrDefault(item => item.StartsWith("[" + key + "]: [", StringComparison.Ordinal))?.Trim().Split("]: [", 2)[1].TrimEnd(']') ?? "";
                        var size = Decode(await ExecuteAsync(serial, ["shell", "wm", "size"], token));
                        var input = ""; string? rotationError = null;
                        try { input = Decode(await ExecuteAsync(serial, ["shell", "dumpsys input"], token)); }
                        catch (DeviceException ex) when (ex.Status is not (409 or 413)) { rotationError = "Rotation unavailable: " + ex.Message; }
                        var display = ParseDisplayInfo(size, input);
                        return device with { Brand = Get("ro.product.brand"), Model = Get("ro.product.model"), AndroidVersion = Get("ro.build.version.release"), Width = display.Width, Height = display.Height, Rotation = display.Rotation, Error = rotationError };
                    }, ct).ConfigureAwait(false);
                }
                catch (DeviceException ex) { _transport.InvalidateTransport(serial); device = device with { State = "Failed", Error = ex.Message }; }
            }
            devices.Add(device);
        }
        return devices.AsReadOnly();
        }
        catch { _transport.ReplaceTransports(new(StringComparer.Ordinal)); throw; }
    }

    public Task<string> ShellAsync(string serial, string command, bool useRoot = false, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(command);
        return RunDeviceAsync(serial, async token => Decode(await ExecuteAsync(serial, useRoot ? ["shell", "su", "-c", Quote(command)] : ["shell", command], token)), ct);
    }
    public Task<byte[]> ScreenshotAsync(string serial, CancellationToken ct = default) => RunDeviceAsync(serial, async token =>
    {
        var png = await ExecuteAsync(serial, ["exec-out", "screencap", "-p"], token).ConfigureAwait(false);
        try { PngImage.ReadDimensions(png); }
        catch (Exception ex) when (ex is InvalidDataException or EndOfStreamException) { throw new DeviceException($"Invalid Android screenshot: serial={serial}: {ex.Message}", 502, 2000); }
        return png;
    }, ct);

    public Task<AndroidNode> DumpAsync(string serial, CancellationToken ct = default) => RunDeviceAsync(serial, async token =>
    {
        var path = "/data/local/tmp/matrix-ui-" + Guid.NewGuid().ToString("N") + ".xml";
        try
        {
            var output = Decode(await ExecuteAsync(serial, ["shell", "uiautomator dump " + Quote(path) + " 2>&1"], token));
            if (output.Contains("ERROR", StringComparison.OrdinalIgnoreCase) || !output.Contains(path, StringComparison.Ordinal))
                throw new DeviceException($"UI dump did not create a confirmed node snapshot: serial={serial}. Pause moving content or open a static page and retry. {output}", 409, 1003);
            var xml = Decode(await ExecuteAsync(serial, ["shell", "cat " + Quote(path)], token));
            try { return ParseUiXml(xml); }
            catch (Exception ex) when (ex is XmlException or InvalidDataException or FormatException or OverflowException)
            { throw new DeviceException($"Invalid UI node snapshot: serial={serial}: {ex.Message}", 502, 2000); }
        }
        finally
        {
            if (!token.IsCancellationRequested)
                await ExecuteAsync(serial, ["shell", "rm -f " + Quote(path)], token).ConfigureAwait(false);
        }
    }, ct);

    public static AndroidNode ParseUiXml(string xml)
    {
        var settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null, MaxCharactersInDocument = 16 * 1024 * 1024 };
        using (var inspection = XmlReader.Create(new StringReader(xml), settings))
        {
            var elements = 0;
            while (inspection.Read())
            {
                if (inspection.Depth > 256) throw new InvalidDataException("UI dump exceeds the maximum XML depth of 256.");
                if (inspection.NodeType == XmlNodeType.Element && ++elements > 100000) throw new InvalidDataException("UI dump exceeds 100000 elements.");
            }
        }
        using var reader = XmlReader.Create(new StringReader(xml), settings);
        var root = XDocument.Load(reader).Root ?? throw new InvalidDataException("UI dump has no root.");
        var counts = new Dictionary<string, int>(StringComparer.Ordinal);
        AndroidNode ConvertNode(XElement element)
        {
            string? Text(string name) => (string?)element.Attribute(name);
            bool Flag(string name) => bool.TryParse(Text(name), out var flag) && flag;
            var boundsText = Text("bounds") ?? "[0,0][0,0]";
            var match = Regex.Match(boundsText, @"^\[(-?\d+),(-?\d+)\]\[(-?\d+),(-?\d+)\]$");
            if (!match.Success) throw new InvalidDataException($"Invalid UI node bounds: {boundsText}");
            var numbers = match.Groups.Cast<Group>().Skip(1).Select(group => int.Parse(group.Value, CultureInfo.InvariantCulture)).ToArray();
            var nodeClass = Text("class") ?? "";
            counts.TryGetValue(nodeClass, out var instance); counts[nodeClass] = instance + 1;
            return new AndroidNode
            {
                Text = Text("text"), ResourceId = Text("resource-id"), ClassName = nodeClass, PackageName = Text("package"), ContentDesc = Text("content-desc"),
                Index = int.TryParse(Text("index"), out var index) ? index : 0, Instance = instance,
                Clickable = Flag("clickable"), Enabled = Flag("enabled"), Focusable = Flag("focusable"), Focused = Flag("focused"), Scrollable = Flag("scrollable"),
                LongClickable = Flag("long-clickable"), Checkable = Flag("checkable"), Checked = Flag("checked"), Selected = Flag("selected"),
                VisibleToUser = Text("visible-to-user") is null || Flag("visible-to-user"), Bounds = new(numbers[0], numbers[1], numbers[2], numbers[3]),
                ChildNodeList = element.Elements("node").Select(ConvertNode).ToList()
            };
        }
        if (root.Name.LocalName == "node") return ConvertNode(root);
        return new AndroidNode { ClassName = "hierarchy", Enabled = true, ChildNodeList = root.Elements("node").Select(ConvertNode).ToList() };
    }

    public static (int Width, int Height, int? Rotation) ParseDisplayInfo(string wmSize, string inputDump)
    {
        var sizes = Regex.Matches(wmSize, @"(\d+)x(\d+)");
        var width = sizes.Count == 0 ? 0 : int.Parse(sizes[^1].Groups[1].Value, CultureInfo.InvariantCulture);
        var height = sizes.Count == 0 ? 0 : int.Parse(sizes[^1].Groups[2].Value, CultureInfo.InvariantCulture);
        var viewport = inputDump.Split('\n').FirstOrDefault(line => Regex.IsMatch(line, @"\bdisplayId\s*[=:]\s*0\b") && Regex.IsMatch(line, @"\borientation\s*[=:]\s*[0-3]\b")) ?? "";
        var orientation = Regex.Match(inputDump, @"\bSurfaceOrientation\s*:\s*([0-3])\b");
        if (!orientation.Success) orientation = Regex.Match(viewport, @"\borientation\s*[=:]\s*([0-3])\b");
        int? rotation = orientation.Success ? int.Parse(orientation.Groups[1].Value, CultureInfo.InvariantCulture) * 90 : null;
        var surfaceWidth = Regex.Match(inputDump, @"\bSurfaceWidth\s*:\s*(\d+)\b");
        var surfaceHeight = Regex.Match(inputDump, @"\bSurfaceHeight\s*:\s*(\d+)\b");
        if (surfaceWidth.Success && surfaceHeight.Success)
            return (int.Parse(surfaceWidth.Groups[1].Value, CultureInfo.InvariantCulture), int.Parse(surfaceHeight.Groups[1].Value, CultureInfo.InvariantCulture), rotation);
        var frame = Regex.Match(viewport, @"logicalFrame\s*=\s*\[(-?\d+),\s*(-?\d+)\]\[(-?\d+),\s*(-?\d+)\]");
        if (!frame.Success) frame = Regex.Match(viewport, @"logicalFrame\s*=\s*\[(-?\d+),\s*(-?\d+),\s*(-?\d+),\s*(-?\d+)\]");
        if (frame.Success)
        {
            var x = int.Parse(frame.Groups[1].Value, CultureInfo.InvariantCulture); var y = int.Parse(frame.Groups[2].Value, CultureInfo.InvariantCulture);
            var right = int.Parse(frame.Groups[3].Value, CultureInfo.InvariantCulture); var bottom = int.Parse(frame.Groups[4].Value, CultureInfo.InvariantCulture);
            if (right > x && bottom > y) return (right - x, bottom - y, rotation);
        }
        // Logical viewport dimensions take priority; never double-swap an already-landscape override.
        if (rotation is 90 or 270 && width < height) (width, height) = (height, width);
        return (width, height, rotation);
    }

    public Task TapAsync(string serial, int x, int y, CancellationToken ct = default) { ValidatePoint(x, y); return CommandAsync(serial, $"input tap {x} {y}", ct); }
    public Task SwipeAsync(string serial, int x1, int y1, int x2, int y2, int durationMs, CancellationToken ct = default)
    { ValidatePoint(x1, y1); ValidatePoint(x2, y2); if (durationMs < 1 || durationMs > 60000) throw new DeviceException($"Invalid swipe duration: {durationMs}.", 400, 1007); return CommandAsync(serial, $"input swipe {x1} {y1} {x2} {y2} {durationMs}", ct); }
    public Task KeyEventAsync(string serial, int code, CancellationToken ct = default)
    { if (code < 0 || code > 400) throw new DeviceException($"Invalid Android key code: {code}.", 400, 1007); return CommandAsync(serial, $"input keyevent {code}", ct); }
    public Task TextAsync(string serial, string text, CancellationToken ct = default)
    {
        AuthorizeSerial(serial);
        ArgumentNullException.ThrowIfNull(text);
        if (text.Any(value => value < 32 || value > 126) || text.Contains("%s", StringComparison.Ordinal))
        {
            if (!_unicodeInputs.TryGetValue(serial, out var send))
                throw new DeviceException("Global Unicode input requires an explicitly bound LDPlayer instance; node set-text still requires a phone agent.", 501, 2001);
            if (text.Length > 4096 || text.Any(value => value < 32) || text.TrimStart().StartsWith("--", StringComparison.Ordinal))
                throw new DeviceException("LDPlayer text must have at most 4096 characters, no control characters or option prefix.", 400, 1007);
            return RunDeviceAsync(serial, async token =>
            {
                await ExecuteAsync(serial, ["shell", ":"], token).ConfigureAwait(false);
                await send(text, token).ConfigureAwait(false);
            }, ct);
        }
        return CommandAsync(serial, "input text " + Quote(text.Replace(" ", "%s", StringComparison.Ordinal)), ct);
    }
    public Task<string> StartAppAsync(string serial, string packageName, string? activity = null, CancellationToken ct = default)
    {
        ValidatePackage(packageName);
        if (activity is not null && !Regex.IsMatch(activity, @"^[A-Za-z0-9_.$]+$")) throw new DeviceException($"Invalid activity: {activity}.", 400, 1007);
        return RunDeviceAsync(serial, async token =>
        {
            var component = packageName + "/" + activity;
            if (activity is null)
            {
                // Launcher aliases need not have CATEGORY_DEFAULT, so resolve before an explicit start.
                var resolution = Decode(await ExecuteAsync(serial, ["shell", "cmd package resolve-activity --brief -a android.intent.action.MAIN -c android.intent.category.LAUNCHER -p " + packageName], token).ConfigureAwait(false));
                var candidates = resolution.Split('\n').Select(line => line.Trim()).Where(line => line.StartsWith(packageName + "/", StringComparison.Ordinal) && Regex.IsMatch(line[(packageName.Length + 1)..], @"^[A-Za-z0-9_.$]+$")).ToArray();
                if (candidates.Length != 1) throw new DeviceException($"No unique enabled launcher activity: serial={serial}, package={packageName}: {resolution}", 409, 1003);
                component = candidates[0];
            }
            var command = "am start -W -a android.intent.action.MAIN -c android.intent.category.LAUNCHER -n " + component;
            var output = Decode(await ExecuteAsync(serial, ["shell", command], token).ConfigureAwait(false));
            if (output.Split('\n').Any(line => line.TrimStart().StartsWith("Error", StringComparison.OrdinalIgnoreCase)) ||
                Regex.IsMatch(output, @"(?im)^\s*Status:\s*(?!ok\s*$)\S+"))
                throw new DeviceException($"Android activity launch failed: serial={serial}, package={packageName}: {output}", 409, 1003);
            if (!output.Contains("Status: ok", StringComparison.OrdinalIgnoreCase))
                throw new DeviceException($"Android activity launch was not confirmed: serial={serial}, package={packageName}: {output}", 502, 2000);
            return output;
        }, ct);
    }
    public Task<string> StopAppAsync(string serial, string packageName, CancellationToken ct = default) { ValidatePackage(packageName); return ShellAsync(serial, "am force-stop " + packageName, ct: ct); }
    public async Task<bool> ClearAppAsync(string serial, string packageName, CancellationToken ct = default) { ValidatePackage(packageName); return (await ShellAsync(serial, "pm clear " + packageName, ct: ct)).Trim() == "Success"; }
    public Task<IReadOnlyList<AppInfo>> ListAppsAsync(string serial, bool showSystem = false, bool needVersion = true, CancellationToken ct = default) => RunDeviceAsync<IReadOnlyList<AppInfo>>(serial, async token =>
    {
        var output = Decode(await ExecuteAsync(serial, showSystem ? ["shell", "pm list packages"] : ["shell", "pm list packages -3"], token));
        var system = showSystem ? Decode(await ExecuteAsync(serial, ["shell", "pm list packages -s"], token)).Split('\n').Select(line => line.Trim().Replace("package:", "", StringComparison.Ordinal)).ToHashSet(StringComparer.Ordinal) : [];
        var apps = new List<AppInfo>();
        foreach (var line in output.Split('\n'))
        {
            if (!line.Trim().StartsWith("package:", StringComparison.Ordinal)) continue;
            var package = line.Trim()[8..]; ValidatePackage(package);
            string? version = null;
            if (needVersion)
            {
                var detail = Decode(await ExecuteAsync(serial, ["shell", "dumpsys package " + package], token));
                version = Regex.Match(detail, @"versionName=([^\r\n]+)").Groups[1].Value.Trim();
            }
            apps.Add(new(package, package, version, system.Contains(package)));
        }
        return apps.AsReadOnly();
    }, ct);
    public Task InstallAsync(string serial, string localPath, CancellationToken ct = default)
    {
        ValidateLocalFile(localPath);
        return RunDeviceAsync(serial, async token => { var output = Decode(await ExecuteAsync(serial, ["install", "-r", Path.GetFullPath(localPath)], token)); if (output.Trim() != "Success") throw new DeviceException($"APK install failed: serial={serial}: {output}", 409, 1003); }, ct);
    }
    public Task UninstallAsync(string serial, string packageName, CancellationToken ct = default)
    { ValidatePackage(packageName); return RunDeviceAsync(serial, async token => { var output = Decode(await ExecuteAsync(serial, ["uninstall", packageName], token)); if (output.Trim() != "Success") throw new DeviceException($"Uninstall failed: serial={serial}: {output}", 409, 1003); }, ct); }
    public Task PushAsync(string serial, string localPath, string remotePath, IProgress<double>? progress = null, CancellationToken ct = default)
    { ValidateLocalFile(localPath); ValidateRemotePath(remotePath); return RunDeviceAsync(serial, async token => { progress?.Report(0); await ExecuteAsync(serial, ["push", Path.GetFullPath(localPath), remotePath], token); progress?.Report(1); }, ct); }
    public Task PullAsync(string serial, string remotePath, string localPath, IProgress<double>? progress = null, CancellationToken ct = default)
    {
        ValidateRemotePath(remotePath); ArgumentException.ThrowIfNullOrWhiteSpace(localPath);
        return RunDeviceAsync(serial, async token => { progress?.Report(0); await ExecuteAsync(serial, ["pull", remotePath, Path.GetFullPath(localPath)], token); if (!File.Exists(localPath)) throw new DeviceException($"Pull returned without the requested file: {localPath}", 502, 2000); progress?.Report(1); }, ct);
    }
    public Task SetNodeTextAsync(string serial, NodeSelector selector, string text, CancellationToken ct = default)
    {
        AuthorizeSerial(serial);
        if (!_agents.TryGetValue(serial, out var agent)) throw new DeviceException($"Node set-text requires an explicitly registered phone agent: serial={serial}.", 501, 2001);
        return RunDeviceAsync(serial, token => agent.SetNodeTextAsync(selector, text, token), ct);
    }

    public async Task StreamScreenVideoAsync(string serial, int width, int height, Stream output, CancellationToken ct = default)
    {
        AuthorizeSerial(serial);
        ArgumentNullException.ThrowIfNull(output);
        if (!output.CanWrite) throw new ArgumentException("Video output must be writable.", nameof(output));
        if (width is < 16 or > 1920 || height is < 16 or > 1920 || width % 2 != 0 || height % 2 != 0 || (long)width * height > 2073600)
            throw new DeviceException($"Invalid video dimensions: {width}x{height}; require even dimensions 16..1920 and at most 2073600 pixels.", 400, 1007);
        if (!_identityGuards.TryGetValue(serial, out var verify)) throw new DeviceException($"Video requires an explicitly bound device identity: serial={serial}.", 403, 1003);
        var expected = _transport.CurrentTransport(serial) ?? throw new DeviceException($"ADB transport has not been discovered: {serial}; refresh devices first.", 503, 2000);
        var finished = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        CancellationTokenSource running;
        lock (_videoGate)
        {
            ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed) != 0, this);
            if (_blockedVideos.TryGetValue(serial, out var reason)) throw new DeviceException(reason, 409, 1003);
            if (!_videoLeases.TryAdd(serial, finished)) throw new DeviceException($"A video stream is already active: serial={serial}.", 409, 1003);
            running = CancellationTokenSource.CreateLinkedTokenSource(ct, _videoShutdown.Token);
        }
        var scid = RandomNumberGenerator.GetInt32(1, int.MaxValue).ToString("x8", CultureInfo.InvariantCulture);
        var remoteServer = "/data/local/tmp/matrix-scrcpy-" + Guid.NewGuid().ToString("N") + ".jar";
        var uploaded = false;
        Exception? failure = null;
        try
        {
            await VerifyVideoIdentityAsync(serial, expected, verify, running.Token).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(_settings.MonitorServerPath) || !File.Exists(_settings.MonitorServerPath))
                throw new DeviceException("官方视频服务组件未配置或不存在；请配置经过校验的 scrcpy-server 3.3.4。", 501, 2001);
            // Keep a read-only file handle open through push so the validated artifact cannot be replaced mid-upload.
            using (var artifact = File.OpenRead(_settings.MonitorServerPath))
            {
                var digest = Convert.ToHexString(await SHA256.HashDataAsync(artifact, running.Token).ConfigureAwait(false));
                if (!digest.Equals(MonitorServerSha256, StringComparison.OrdinalIgnoreCase))
                    throw new DeviceException($"视频服务组件 SHA256 不匹配：expected={MonitorServerSha256}, actual={digest}。", 403, 1007);
                uploaded = true;
                await _transport.RunPinnedAsync(serial, expected, token => _transport.ExecuteAsync(serial, ["push", Path.GetFullPath(_settings.MonitorServerPath), remoteServer], token), running.Token).ConfigureAwait(false);
            }
            await VerifyVideoIdentityAsync(serial, expected, verify, running.Token).ConfigureAwait(false);
            var watch = WatchVideoIdentityAsync(serial, expected, verify, running.Token);
            var stream = _transport.StreamScreenVideoAsync(serial, expected, remoteServer, scid, Math.Max(width, height), output,
                token => VerifyVideoIdentityAsync(serial, expected, verify, token, allowDisposed: true), running.Token);
            try
            {
                var completed = await Task.WhenAny(watch, stream).ConfigureAwait(false);
                await completed.ConfigureAwait(false);
                if (watch.IsFaulted) await watch.ConfigureAwait(false);
            }
            finally
            {
                await running.CancelAsync().ConfigureAwait(false);
                try { await Task.WhenAll(watch, stream).ConfigureAwait(false); }
                catch
                {
                    var cleanupError = stream.Exception?.GetBaseException();
                    if (cleanupError?.Data.Contains("VideoCleanupUnconfirmed") == true)
                    {
                        lock (_videoGate) _blockedVideos[serial] = cleanupError.Message;
                        System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(cleanupError).Throw();
                    }
                }
            }
        }
        catch (Exception ex) { failure = ex; throw; }
        finally
        {
            bool blocked;
            lock (_videoGate) blocked = _blockedVideos.ContainsKey(serial);
            if (uploaded && !blocked)
            {
                try
                {
                    using var cleanup = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                    await VerifyVideoIdentityAsync(serial, expected, verify, cleanup.Token, allowDisposed: true).ConfigureAwait(false);
                    await _transport.RunPinnedAsync(serial, expected, token => _transport.ExecuteAsync(serial, ["shell", "rm -f " + Quote(remoteServer)], token), cleanup.Token).ConfigureAwait(false);
                }
                catch (Exception ex) when (ex is DeviceException or IOException or OperationCanceledException or ObjectDisposedException)
                {
                    if (failure is not null) failure.Data["VideoServerFileCleanupError"] = ex.Message;
                    else failure = ex;
                }
            }
            lock (_videoGate) _videoLeases.Remove(serial);
            running.Dispose();
            finished.TrySetResult();
        }
    }

    private async Task VerifyVideoIdentityAsync(string serial, uint expected, Func<CancellationToken, Task> verify, CancellationToken ct, bool allowDisposed = false)
    {
        _transport.RequireTransport(serial, expected);
        await verify(ct).ConfigureAwait(false);
        var rows = allowDisposed
            ? ParseRegisteredDevices(Decode(await _transport.ExecuteAsync(null, ["devices", "-l"], ct).ConfigureAwait(false)))
            : await ListRegisteredDevicesAsync(ct).ConfigureAwait(false);
        var matches = rows.Where(row => row.Serial == serial).ToArray();
        if (matches.Length != 1 || matches[0].State != "device" || matches[0].TransportId != expected)
            throw new DeviceException($"Video device identity or transport changed: serial={serial}, expected={expected}.", 409, 1003);
        _transport.RequireTransport(serial, expected);
    }

    private async Task WatchVideoIdentityAsync(string serial, uint expected, Func<CancellationToken, Task> verify, CancellationToken ct)
    {
        while (true)
        {
            await Task.Delay(TimeSpan.FromSeconds(2), ct).ConfigureAwait(false);
            await VerifyVideoIdentityAsync(serial, expected, verify, ct).ConfigureAwait(false);
        }
    }

    private Task CommandAsync(string serial, string command, CancellationToken ct) => RunDeviceAsync(serial, async token => { await ExecuteAsync(serial, ["shell", command], token); }, ct);
    private Task<T> RunDeviceAsync<T>(string serial, Func<CancellationToken, Task<T>> action, CancellationToken ct)
    {
        AuthorizeSerial(serial);
        var expected = _transport.CurrentTransport(serial) ?? throw new DeviceException($"ADB transport has not been discovered: {serial}; refresh devices first.", 503, 2000);
        return _queue.RunAsync(serial, token => _transport.RunPinnedAsync(serial, expected, async pinned =>
        {
            if (_identityGuards.TryGetValue(serial, out var verify)) await verify(pinned).ConfigureAwait(false);
            return await action(pinned).ConfigureAwait(false);
        }, token), ct);
    }
    internal uint? ObservedTransportId(string serial) => _transport.CurrentTransport(serial);
    private Task RunDeviceAsync(string serial, Func<CancellationToken, Task> action, CancellationToken ct) => RunDeviceAsync(serial, async token => { await action(token).ConfigureAwait(false); return true; }, ct);
    private Task<byte[]> ExecuteAsync(string? serial, string[] arguments, CancellationToken ct)
    {
        ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed) != 0, this);
        if (serial is not null) AuthorizeSerial(serial);
        return _transport.ExecuteAsync(serial, arguments, ct);
    }
    public static string Quote(string value) => "'" + value.Replace("'", "'\"'\"'", StringComparison.Ordinal) + "'";
    private static string Decode(byte[] bytes) => Encoding.UTF8.GetString(bytes).TrimEnd('\r', '\n');
    private void AuthorizeSerial(string serial)
    {
        ValidateSerial(serial);
        if (!_allowedSerials.Contains(serial)) throw new DeviceException($"ADB serial is not authorized: {serial}.", 403, 1007);
    }
    private static void ValidateSerial(string serial) { if (string.IsNullOrWhiteSpace(serial) || serial.StartsWith('-') || serial.Any(char.IsWhiteSpace) || serial.Contains('\0')) throw new DeviceException($"Invalid device serial: {serial}.", 400, 1007); }
    private static void ValidatePoint(int x, int y) { if (x < 0 || y < 0) throw new DeviceException($"Negative device point: ({x},{y}).", 400, 1007); }
    private static void ValidatePackage(string value) { if (value != "android" && !Regex.IsMatch(value ?? "", @"^[A-Za-z][A-Za-z0-9_]*(\.[A-Za-z][A-Za-z0-9_]*)+$")) throw new DeviceException($"Invalid package name: {value}.", 400, 1007); }
    private static void ValidateLocalFile(string path) { if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) throw new DeviceException($"Local file does not exist: {path}.", 400, 1007); }
    private static void ValidateRemotePath(string path) { if (string.IsNullOrWhiteSpace(path) || !path.StartsWith('/') || path.Contains('\0') || path.Contains('\n')) throw new DeviceException($"Invalid absolute Android path: {path}.", 400, 1007); }
    public async ValueTask DisposeAsync()
    {
        Task[] videos;
        lock (_videoGate)
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0) return;
            videos = _videoLeases.Values.Select(lease => lease.Task).ToArray();
        }
        await _videoShutdown.CancelAsync().ConfigureAwait(false);
        await Task.WhenAll(videos).ConfigureAwait(false);
        _videoShutdown.Dispose();
        await _queue.DisposeAsync();
        foreach (var agent in _agents.Values) await agent.DisposeAsync();
    }
}

public sealed record AdbRegisteredDevice(string Serial, string State, uint? TransportId);
