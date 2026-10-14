using System.Diagnostics;
using System.Text.Json;
using MatrixDevice.Core;

namespace MatrixDevice.Features;

public sealed class AutomationService(IDeviceBackend backend, DeviceCatalog catalog)
{
    public static IReadOnlySet<string> Routes { get; } = new HashSet<string>(StringComparer.Ordinal)
    {
        "device/dump", "device/screenshot", "device/shell", "device/execute", "device/set-prop",
        "device/set-timezone", "device/set-sim-info", "app/list", "app/list-packages", "app/start", "app/stop",
        "app/clear", "input/tap", "input/swipe", "input/keyevent", "service/info", "service/stop", "function/sleep",
        "function/sleep-ms", "element/find", "element/find-all", "element/wait-exists", "element/click-if-exists",
        "element/click-text", "element/set-text", "element/wait-any", "element/click-node"
    };
    public Func<byte[], int?, int?, int?, int?, int?, byte[]>? ScreenshotTransform { get; set; }
    public Func<string, CancellationToken, Task<object?>>? ServiceInfoProvider { get; set; }
    public Func<string, CancellationToken, Task>? ServiceStop { get; set; }

    public async Task<object?> ExecuteAsync(string route, JsonElement body, CancellationToken ct = default)
    {
        route = NormalizeRoute(route);
        if (!Routes.Contains(route)) throw new DeviceException($"Unknown Automator route: '{route}'.", 404, 1006);
        if (body.ValueKind != JsonValueKind.Object) throw Invalid("Request body must be a JSON object.");
        var serial = RequiredString(body, "serial");
        var device = catalog.GetRequired(serial);
        if (!device.Online) throw new DeviceException($"Device '{serial}' is not Ready: {device.State}.", 409, 1003);
        switch (route)
        {
            case "device/dump": return await backend.DumpAsync(serial, ct).ConfigureAwait(false);
            case "device/screenshot":
                var x = OptionalInt(body, "x"); var y = OptionalInt(body, "y");
                var width = OptionalInt(body, "width"); var height = OptionalInt(body, "height"); var quality = OptionalInt(body, "quality");
                if (x < 0 || y < 0 || width <= 0 || height <= 0 || quality < 0 || quality > 100) throw Invalid("Invalid screenshot crop or quality.");
                var png = await backend.ScreenshotAsync(serial, ct).ConfigureAwait(false);
                if (x is not null || y is not null || width is not null || height is not null || quality is not null)
                {
                    var dimensions = PngImage.ReadDimensions(png);
                    var left = x ?? 0; var top = y ?? 0;
                    var cropWidth = width ?? dimensions.Width - left;
                    var cropHeight = height ?? dimensions.Height - top;
                    if (left >= dimensions.Width || top >= dimensions.Height || cropWidth <= 0 || cropHeight <= 0 ||
                        (long)left + cropWidth > dimensions.Width || (long)top + cropHeight > dimensions.Height)
                        throw Invalid($"Screenshot crop ({left},{top},{cropWidth},{cropHeight}) is outside {dimensions.Width}x{dimensions.Height}.");
                    if (ScreenshotTransform is null) throw Unsupported("Screenshot cropping requires the registered Windows image adapter.");
                    png = ScreenshotTransform(png, x, y, width, height, quality);
                }
                return new { format = "png", base64 = Convert.ToBase64String(png) };
            case "device/shell": case "device/execute":
                return new { output = await backend.ShellAsync(serial, RequiredString(body, "command"), ct: ct).ConfigureAwait(false) };
            case "device/set-prop":
                var prop = RequiredString(body, "prop");
                if (prop.Length > 128 || prop.Any(character => !char.IsAsciiLetterOrDigit(character) && character is not '.' and not '_' and not '-'))
                    throw Invalid($"Invalid Android property name: '{prop}'.");
                var value = RequiredString(body, "value", allowEmpty: true);
                await backend.ShellAsync(serial, $"setprop {Quote(prop)} {Quote(value)}", true, ct).ConfigureAwait(false);
                var actual = (await backend.ShellAsync(serial, $"getprop {Quote(prop)}", ct: ct).ConfigureAwait(false)).TrimEnd('\r', '\n');
                if (!string.Equals(actual, value, StringComparison.Ordinal))
                    throw new DeviceException($"Property verification failed for '{prop}'.", 409, 1003);
                return new { value = true };
            case "device/set-timezone":
                var timezone = RequiredString(body, "timezone");
                try { TimeZoneInfo.FindSystemTimeZoneById(timezone); }
                catch (TimeZoneNotFoundException) { throw Invalid($"Unknown timezone: '{timezone}'."); }
                await backend.ShellAsync(serial, $"setprop persist.sys.timezone {Quote(timezone)}", true, ct).ConfigureAwait(false);
                var zone = (await backend.ShellAsync(serial, "getprop persist.sys.timezone", ct: ct).ConfigureAwait(false)).Trim();
                if (zone != timezone) throw new DeviceException($"Timezone verification failed: '{zone}'.", 409, 1003);
                return new { value = true };
            case "device/set-sim-info": throw Unsupported("SIM information requires the undocumented device ROM service.");
            case "app/list":
                return await backend.ListAppsAsync(serial, OptionalBool(body, "showSystem") ?? false, OptionalBool(body, "needVersion") ?? true, ct).ConfigureAwait(false);
            case "app/list-packages": return (await backend.ListAppsAsync(serial, true, false, ct).ConfigureAwait(false)).Select(app => app.PackageName).ToArray();
            case "app/start": return new { output = await backend.StartAppAsync(serial, Package(body), ct: ct).ConfigureAwait(false) };
            case "app/stop": return new { output = await backend.StopAppAsync(serial, Package(body), ct).ConfigureAwait(false) };
            case "app/clear": return new { value = await backend.ClearAppAsync(serial, Package(body), ct).ConfigureAwait(false) };
            case "input/tap":
                await backend.TapAsync(serial, RequiredInt(body, "x"), RequiredInt(body, "y"), ct).ConfigureAwait(false);
                return new { value = true };
            case "input/swipe":
                var steps = OptionalInt(body, "steps") ?? 55; var stepMs = OptionalInt(body, "stepMs") ?? 5;
                if (steps <= 0 || stepMs <= 0 || (long)steps * stepMs > 60000) throw Invalid($"Invalid swipe timing: steps={steps}, stepMs={stepMs}; duration must be in 1..60000ms.");
                await backend.SwipeAsync(serial, RequiredInt(body, "x1"), RequiredInt(body, "y1"), RequiredInt(body, "x2"), RequiredInt(body, "y2"), checked(steps * stepMs), ct).ConfigureAwait(false);
                return new { value = true };
            case "input/keyevent":
                await backend.KeyEventAsync(serial, RequiredInt(body, "code"), ct).ConfigureAwait(false);
                return new { value = true };
            case "service/info":
                if (ServiceInfoProvider is not null) return await ServiceInfoProvider(serial, ct).ConfigureAwait(false);
                return new { serial, state = device.State, backend = backend.GetType().Name, agentServiceAvailable = false };
            case "service/stop":
                if (ServiceStop is null) throw Unsupported("The backend has no private Automator agent lifecycle adapter.");
                await ServiceStop(serial, ct).ConfigureAwait(false); return new { value = true };
            case "function/sleep":
                var seconds = OptionalDouble(body, "seconds") ?? throw Invalid("seconds is required; the original nullable default was not recovered.");
                if (seconds < 0 || seconds > 600) throw Invalid($"seconds must be in 0..600: {seconds}.");
                await Task.Delay(TimeSpan.FromSeconds(seconds), ct).ConfigureAwait(false); return new { value = true };
            case "function/sleep-ms":
                var milliseconds = OptionalInt(body, "milliseconds") ?? throw Invalid("milliseconds is required; the original nullable default was not recovered.");
                if (milliseconds < 0 || milliseconds > 600000) throw Invalid($"milliseconds must be in 0..600000: {milliseconds}.");
                await Task.Delay(milliseconds, ct).ConfigureAwait(false); return new { value = true };
            case "element/find": return NodeSearch.Find(await backend.DumpAsync(serial, ct).ConfigureAwait(false), Selector(body));
            case "element/find-all": return NodeSearch.FindAll(await backend.DumpAsync(serial, ct).ConfigureAwait(false), Selector(body));
            case "element/wait-exists": return new { value = await WaitNodeAsync(serial, Selector(body), Timeout(body), Poll(body), ct).ConfigureAwait(false) is not null };
            case "element/click-if-exists":
                var found = await WaitNodeAsync(serial, Selector(body), Timeout(body), Poll(body), ct).ConfigureAwait(false);
                if (found is null) return new { value = false };
                await ClickNodeAsync(serial, found, ct).ConfigureAwait(false); return new { value = true };
            case "element/click-text":
                var textNode = await WaitNodeAsync(serial, new NodeSelector { Text = RequiredString(body, "text", true) }, Timeout(body), Poll(body), ct).ConfigureAwait(false);
                if (textNode is null) return new { value = false };
                await ClickNodeAsync(serial, textNode, ct).ConfigureAwait(false); return new { value = true };
            case "element/set-text":
                await backend.SetNodeTextAsync(serial, Selector(body), RequiredString(body, "text", true), ct).ConfigureAwait(false); return new { value = true };
            case "element/wait-any":
                var selectorsElement = RequiredProperty(body, "selectors");
                if (selectorsElement.ValueKind != JsonValueKind.Array || selectorsElement.GetArrayLength() is 0 or > 256)
                    throw Invalid("selectors must be an array containing 1..256 selectors.");
                var selectors = selectorsElement.EnumerateArray().Select(Deserialize<NodeSelector>).ToArray();
                return await WaitAnyAsync(serial, selectors, Timeout(body), Poll(body), ct).ConfigureAwait(false);
            case "element/click-node":
                await ClickNodeAsync(serial, Deserialize<AndroidNode>(RequiredProperty(body, "node")), ct).ConfigureAwait(false); return new { value = true };
            default: throw new UnreachableException();
        }
    }

    public async Task<AndroidNode?> WaitNodeAsync(string serial, NodeSelector selector, int timeoutMs = 3000, int pollMs = 200, CancellationToken ct = default)
    {
        ValidatePolling(timeoutMs, pollMs);
        var timer = Stopwatch.StartNew();
        while (true)
        {
            ct.ThrowIfCancellationRequested();
            var node = NodeSearch.Find(await backend.DumpAsync(serial, ct).ConfigureAwait(false), selector);
            if (node is not null) return node;
            if (timer.ElapsedMilliseconds >= timeoutMs) return null;
            var remaining = timeoutMs - timer.ElapsedMilliseconds;
            if (remaining <= 0) return null;
            await Task.Delay((int)Math.Min(pollMs, remaining), ct).ConfigureAwait(false);
        }
    }

    public async Task<object?> WaitAnyAsync(string serial, IReadOnlyList<NodeSelector> selectors, int timeoutMs = 3000, int pollMs = 200, CancellationToken ct = default)
    {
        if (selectors.Count == 0 || selectors.Any(selector => selector is null)) throw Invalid("selectors must be nonempty and contain no null.");
        ValidatePolling(timeoutMs, pollMs);
        var timer = Stopwatch.StartNew();
        while (true)
        {
            ct.ThrowIfCancellationRequested();
            for (var index = 0; index < selectors.Count; index++)
            {
                var node = NodeSearch.Find(await backend.DumpAsync(serial, ct).ConfigureAwait(false), selectors[index]);
                if (node is not null) return new { index, selector = selectors[index], node };
            }
            if (timer.ElapsedMilliseconds >= timeoutMs) return null;
            var remaining = timeoutMs - timer.ElapsedMilliseconds;
            if (remaining <= 0) return null;
            await Task.Delay((int)Math.Min(pollMs, remaining), ct).ConfigureAwait(false);
        }
    }

    public Task ClickNodeAsync(string serial, AndroidNode node, CancellationToken ct = default)
    {
        if (node.Bounds.Width <= 0 || node.Bounds.Height <= 0) throw Invalid("Node bounds must have positive width and height.");
        return backend.TapAsync(serial, node.Bounds.CenterX, node.Bounds.CenterY, ct);
    }

    private static void ValidatePolling(int timeout, int poll)
    {
        if (timeout < 0 || timeout > 600000 || poll <= 0 || poll > 60000) throw Invalid($"Invalid polling: timeoutMs={timeout}, pollMs={poll}.");
    }
    private static int Timeout(JsonElement body) => OptionalInt(body, "timeoutMs") ?? 3000;
    private static int Poll(JsonElement body) => OptionalInt(body, "pollMs") ?? 200;
    private static string Package(JsonElement body) { var name = RequiredString(body, "packageName"); ScriptPackageService.ValidatePackageName(name); return name; }
    private static NodeSelector Selector(JsonElement body) => Deserialize<NodeSelector>(RequiredProperty(body, "selector"));
    public static string NormalizeRoute(string route)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(route);
        route = route.Trim().Trim('/');
        if (route.StartsWith("api/", StringComparison.OrdinalIgnoreCase)) route = route[4..];
        return route.ToLowerInvariant();
    }
    public static string Quote(string value) => "'" + value.Replace("'", "'\"'\"'", StringComparison.Ordinal) + "'";
    private static JsonElement RequiredProperty(JsonElement body, string name) => Property(body, name) ?? throw Invalid($"Missing property: {name}.");
    private static JsonElement? Property(JsonElement body, string name)
    {
        JsonElement? found = null;
        foreach (var property in body.EnumerateObject())
            if (property.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                if (found is not null) throw Invalid($"Duplicate property: {name}.");
                found = property.Value;
            }
        return found;
    }
    private static string RequiredString(JsonElement body, string name, bool allowEmpty = false)
    {
        var value = RequiredProperty(body, name);
        if (value.ValueKind != JsonValueKind.String) throw Invalid($"Property {name} must be a string.");
        var text = value.GetString()!;
        if (!allowEmpty && string.IsNullOrWhiteSpace(text)) throw Invalid($"Property {name} must not be blank.");
        return text;
    }
    private static int RequiredInt(JsonElement body, string name) => OptionalInt(body, name) ?? throw Invalid($"Missing integer property: {name}.");
    private static int? OptionalInt(JsonElement body, string name)
    {
        var value = Property(body, name);
        if (value is null || value.Value.ValueKind == JsonValueKind.Null) return null;
        if (value.Value.ValueKind != JsonValueKind.Number || !value.Value.TryGetInt32(out var result)) throw Invalid($"Property {name} must be an Int32.");
        return result;
    }
    private static double? OptionalDouble(JsonElement body, string name)
    {
        var value = Property(body, name);
        if (value is null || value.Value.ValueKind == JsonValueKind.Null) return null;
        if (value.Value.ValueKind != JsonValueKind.Number || !value.Value.TryGetDouble(out var result) || !double.IsFinite(result)) throw Invalid($"Property {name} must be a finite number.");
        return result;
    }
    private static bool? OptionalBool(JsonElement body, string name)
    {
        var value = Property(body, name);
        if (value is null || value.Value.ValueKind == JsonValueKind.Null) return null;
        if (value.Value.ValueKind is not JsonValueKind.True and not JsonValueKind.False) throw Invalid($"Property {name} must be boolean.");
        return value.Value.GetBoolean();
    }
    private static T Deserialize<T>(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object) throw Invalid($"{typeof(T).Name} must be an object.");
        try { return element.Deserialize<T>(JsonDefaults.Options) ?? throw Invalid($"{typeof(T).Name} must not be null."); }
        catch (JsonException error) { throw Invalid($"Invalid {typeof(T).Name}: {error.Message}"); }
    }
    private static DeviceException Invalid(string message) => new(message, 400, 1007);
    private static DeviceException Unsupported(string message) => new(message, 501, 2001);
}
