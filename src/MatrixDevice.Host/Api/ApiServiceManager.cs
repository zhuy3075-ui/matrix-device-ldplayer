using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Nodes;
using MatrixDevice.Core;
using MatrixDevice.Features;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MatrixDevice.Host;

public sealed class ApiServiceManager(DeviceCatalog catalog, IDeviceBackend backend, AutomationService automation, RollingLog log, DeviceWriteCoordinator? writes = null) : IAsyncDisposable
{
    private readonly SemaphoreSlim _lifecycle = new(1, 1);
    private WebApplication? _basic;
    private WebApplication? _automator;
    private readonly ConcurrentDictionary<string, (int Port, WebApplication App)> _devices = new(StringComparer.Ordinal);
    private readonly Dictionary<int, DateTimeOffset> _cooldown = [];
    private int _basicPort;
    private int _automatorPort;
    private bool gatewayRollbackPending;
    private int activeRequests;
    public bool HasActiveRequests => Volatile.Read(ref activeRequests) != 0;
    public event EventHandler? RequestsDrained;
    private async Task TrackRequestAsync(Func<Task> action)
    {
        Interlocked.Increment(ref activeRequests);
        try { await action(); }
        finally
        {
            if (Interlocked.Decrement(ref activeRequests) == 0)
                foreach (EventHandler observer in RequestsDrained?.GetInvocationList() ?? [])
                {
                    try { observer(this, EventArgs.Empty); }
                    catch (Exception error) { log.Write("api-drain-notification-error", error.ToString()); }
                }
        }
    }
    public bool BasicRunning => _basic is not null;
    public bool AutomatorRunning => _automator is not null;
    public int? RunningBasicPort => BasicRunning ? _basicPort : null;
    public int? RunningAutomatorPort => AutomatorRunning ? _automatorPort : null;
    public Func<IReadOnlyList<DeviceInfo>>? DeviceSnapshotProvider { get; set; }
    public LdPlayerService? LdPlayer { get; set; }
    private int uiWriteBusy;
    public bool UiWriteBusy { get => Volatile.Read(ref uiWriteBusy) != 0; set => Volatile.Write(ref uiWriteBusy, value ? 1 : 0); }
    public Func<string, bool>? ScriptWriteBusy { get; set; }
    public Func<bool>? AnyScriptWriteBusy { get; set; }
    public Func<int, string?>? LdSerialForWriteGuard { get; set; }
    public Func<int, string, CancellationToken, Task>? LdCommandIdentityGuard { get; set; }
    private void RequireWriteAvailable(string? serial = null)
    {
        if (UiWriteBusy || (serial is null ? AnyScriptWriteBusy?.Invoke() == true : ScriptWriteBusy?.Invoke(serial) == true))
            throw new DeviceException("界面操作或脚本正在执行，请等待完成或停止后再通过 API 写入", 409, 1003);
    }
    public IReadOnlyDictionary<string, int> DevicePorts => _devices.ToDictionary(x => x.Key, x => x.Value.Port, StringComparer.Ordinal);

    public async Task StartBasicAsync(int port, CancellationToken ct = default)
    {
        await _lifecycle.WaitAsync(ct);
        try { _ = await StartBasicCoreAsync(port, ct); }
        finally { _lifecycle.Release(); }
    }
    public async Task StartAutomatorAsync(int port, CancellationToken ct = default)
    {
        await _lifecycle.WaitAsync(ct);
        try { await StartAutomatorCoreAsync(port, ct); }
        finally { _lifecycle.Release(); }
    }
    public async Task StartGatewaysAsync(int basicPort, int automatorPort, CancellationToken ct = default)
    {
        await _lifecycle.WaitAsync(ct);
        try
        {
            WebApplication? newBasic = null;
            try
            {
                newBasic = await StartBasicCoreAsync(basicPort, ct);
                ct.ThrowIfCancellationRequested();
                await StartAutomatorCoreAsync(automatorPort, ct);
            }
            catch (Exception startError)
            {
                List<Exception> errors = [startError];
                if (newBasic is not null)
                {
                    try { await newBasic.StopAsync(CancellationToken.None); }
                    catch (Exception error) { errors.Add(error); }
                    var disposed = false;
                    try { await newBasic.DisposeAsync(); disposed = true; }
                    catch (Exception error) { gatewayRollbackPending = true; errors.Add(error); }
                    if (disposed && ReferenceEquals(_basic, newBasic)) { _basic = null; _basicPort = 0; }
                }
                if (errors.Count > 1) throw new AggregateException("Gateway startup failed and rollback cleanup also failed", errors);
                throw;
            }
        }
        finally { _lifecycle.Release(); }
    }
    private async Task<WebApplication?> StartBasicCoreAsync(int port, CancellationToken ct)
    {
        RequireGatewayStartAvailable();
        if (_basic is not null)
        {
            if (_basicPort != port) throw new DeviceException("Stop the basic gateway before changing its port");
            return null;
        }
        ValidatePort(port);
        if (_automatorPort == port || _devices.Values.Any(x => x.Port == port)) throw new DeviceException("Port already reserved: " + port);
        var app = CreateServer(port);
        app.Run(context => TrackRequestAsync(() => HandleBasicGatewayAsync(context, port)));
        await StartServerAsync(app, ct);
        _basic = app;
        _basicPort = port;
        return app;
    }
    private async Task StartAutomatorCoreAsync(int port, CancellationToken ct)
    {
        RequireGatewayStartAvailable();
        if (_automator is not null)
        {
            if (_automatorPort != port) throw new DeviceException("Stop Automator before changing its port");
            return;
        }
        ValidatePort(port);
        if (_basicPort == port || _devices.Values.Any(x => x.Port == port)) throw new DeviceException("Port already reserved: " + port);
        var app = CreateServer(port);
        app.Run(context => TrackRequestAsync(() => HandleAutomatorAsync(context)));
        await StartServerAsync(app, ct);
        _automator = app;
        _automatorPort = port;
    }
    private void RequireGatewayStartAvailable()
    {
        if (gatewayRollbackPending) throw new DeviceException("上次网关回滚清理未完成，请先停止 API 服务，再重试启动。", 409, 1003);
    }
    public async Task<int> StartDeviceAsync(string serial, CancellationToken ct = default)
    {
        await _lifecycle.WaitAsync(ct);
        try
        {
            catalog.GetRequired(serial);
            if (_devices.TryGetValue(serial, out var existing)) return existing.Port;
            foreach (var expired in _cooldown.Where(x => x.Value <= DateTimeOffset.UtcNow).Select(x => x.Key).ToArray()) _cooldown.Remove(expired);
            Exception? lastError = null;
            for (var port = 8001; port <= 8999; port++)
            {
                if (port == _basicPort || port == _automatorPort || _devices.Values.Any(x => x.Port == port) || _cooldown.ContainsKey(port)) continue;
                var app = CreateServer(port);
                var reservedPort = port;
                app.Run(context => TrackRequestAsync(() => HandleDeviceAsync(context, serial, reservedPort)));
                try
                {
                    await app.StartAsync(ct);
                    _devices[serial] = (port, app);
                    return port;
                }
                catch (IOException e)
                {
                    lastError = e;
                    _cooldown[port] = DateTimeOffset.UtcNow.AddSeconds(30);
                    await app.DisposeAsync();
                }
                catch { await app.DisposeAsync(); throw; }
            }
            throw new DeviceException("No available device API port in 8001..8999: " + lastError?.Message, 503, 2000);
        }
        finally { _lifecycle.Release(); }
    }
    public async Task StopDeviceAsync(string serial, CancellationToken ct = default)
    {
        await _lifecycle.WaitAsync(ct);
        try
        {
            if (_devices.TryRemove(serial, out var server))
            {
                try { await server.App.StopAsync(ct); }
                finally { await server.App.DisposeAsync(); _cooldown[server.Port] = DateTimeOffset.UtcNow.AddSeconds(30); }
            }
        }
        finally { _lifecycle.Release(); }
    }
    public async Task StopAsync(CancellationToken ct = default)
    {
        await _lifecycle.WaitAsync(ct);
        try
        {
            List<Exception> errors = [];
            foreach (var server in _devices.Values.Select(x => x.App).Concat(new[] { _basic, _automator }).OfType<WebApplication>().ToArray())
            {
                try { await server.StopAsync(ct); }
                catch (Exception e) { errors.Add(e); }
                finally { try { await server.DisposeAsync(); } catch (Exception e) { errors.Add(e); } }
            }
            foreach (var port in _devices.Values.Select(x => x.Port)) _cooldown[port] = DateTimeOffset.UtcNow.AddSeconds(30);
            _devices.Clear();
            _basic = null;
            _automator = null;
            _basicPort = _automatorPort = 0;
            gatewayRollbackPending = false;
            if (errors.Count > 0) throw new AggregateException("API shutdown errors", errors);
        }
        finally { _lifecycle.Release(); }
    }

    private object Snapshot(string serial, int port)
    {
        var device = (DeviceSnapshotProvider?.Invoke() ?? catalog.Snapshot).FirstOrDefault(x => x.Serial == serial) ?? new DeviceInfo { Serial = serial, State = "Offline" };
        return new { device.Serial, device.Name, device.Brand, device.Model, device.AndroidVersion, device.ConnectionType, device.GroupId, port, baseUrl = $"http://localhost:{port}/" };
    }
    public static string NormalizePath(string? path)
    {
        var normalized = (path ?? "").Trim().Trim('/').ToLowerInvariant();
        return normalized.Length == 0 ? "/" : "/" + normalized;
    }
    private async Task HandleBasicGatewayAsync(HttpContext context, int port)
    {
        var path = NormalizePath(context.Request.Path.Value);
        if (path.StartsWith("/ldplayer", StringComparison.Ordinal))
        {
            await HandleLdPlayerAsync(context, path);
            return;
        }
        if (context.Request.Method == "GET" && path is "/" or "/health")
            await WriteAsync(context, 200, new { ok = true, data = new { port, runningCount = _devices.Count, status = "running" } });
        else if (context.Request.Method == "GET" && path == "/devices")
            await WriteAsync(context, 200, new { ok = true, data = _devices.Select(x => Snapshot(x.Key, x.Value.Port)).ToArray() });
        else await WriteAsync(context, 404, new { ok = false, message = "Route not found: " + path });
    }
    private async Task HandleLdPlayerAsync(HttpContext context, string path)
    {
        IDisposable? writeLease = null;
        try
        {
            object data;
            if (path == "/ldplayer/capabilities" && context.Request.Method == "GET")
                data = LdPlayerService.Capabilities;
            else if (path == "/ldplayer/instances" && context.Request.Method == "GET")
            {
                var service = LdPlayer ?? throw new DeviceException("尚未配置雷电命令行程序", 503, 2001);
                await service.RefreshAsync(context.RequestAborted);
                data = service.Instances;
            }
            else
            {
                var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 4 || parts[0] != "ldplayer" || parts[1] != "instances" ||
                    !int.TryParse(parts[2], System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var index))
                    throw new DeviceException("雷电接口不存在：" + path, 404, 1006);
                if (context.Request.Method != "POST") throw new DeviceException("此接口仅接受 POST 请求", 405, 1005);
                ValidateWriteRequest(context);
                var service = LdPlayer ?? throw new DeviceException("尚未配置雷电命令行程序", 503, 2001);
                var body = await ReadBodyAsync(context);
                var serial = LdSerialForWriteGuard?.Invoke(index);
                if (string.IsNullOrWhiteSpace(serial)) serial = null;
                RequireWriteAvailable(serial);
                writeLease = serial is null ? writes?.AcquireAll() : writes?.Acquire([serial]);
                if (LdCommandIdentityGuard is not null) await LdCommandIdentityGuard(index, parts[3], context.RequestAborted);
                data = await service.ExecuteAsync(index, parts[3], JsonNode.Parse(body.GetRawText())!.AsObject(), context.RequestAborted);
                log.Write("ldplayer-command", $"index={index}, operation={parts[3]}");
            }
            await WriteAsync(context, 200, new { ok = true, data });
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested) { }
        catch (Exception e)
        {
            log.Write("ldplayer-api-error", e.Message);
            await WriteAsync(context, ErrorStatus(e), new { ok = false, message = e.Message });
        }
        finally { writeLease?.Dispose(); }
    }
    private async Task HandleDeviceAsync(HttpContext context, string serial, int port)
    {
        IDisposable? writeLease = null;
        try
        {
            var device = catalog.Snapshot.FirstOrDefault(x => x.Serial == serial);
            if (device?.Online != true) throw new DeviceException("Device unavailable: " + serial, 410, 1003);
            var path = NormalizePath(context.Request.Path.Value);
            if (context.Request.Method == "GET")
            {
                object response = path switch
                {
                    "/" or "/health" => new { ok = true, data = new { serial, port, status = "running" } },
                    "/device" => new { ok = true, data = Snapshot(serial, port) },
                    _ => throw new DeviceException("Route not found: " + path, 404, 1006)
                };
                await WriteAsync(context, 200, response);
                return;
            }
            if (context.Request.Method != "POST") throw new DeviceException("Method not allowed", 405, 1005);
            if (path is not ("/shell" or "/tap" or "/swipe" or "/text" or "/app/start" or "/app/stop"))
                throw new DeviceException("Route not found: " + path, 404, 1006);
            ValidateWriteRequest(context);
            var body = await ReadBodyAsync(context);
            RequireWriteAvailable(serial);
            writeLease = writes?.Acquire([serial]);
            var ct = context.RequestAborted;
            object? data = null;
            switch (path)
            {
                case "/shell": data = new { output = await backend.ShellAsync(serial, RequiredString(body, "Command"), OptionalBool(body, "UseRoot"), ct) }; break;
                case "/tap": await backend.TapAsync(serial, RequiredInt(body, "X"), RequiredInt(body, "Y"), ct); break;
                case "/swipe":
                    var duration = OptionalInt(body, "DurationMs") ?? 300;
                    await backend.SwipeAsync(serial, RequiredInt(body, "StartX"), RequiredInt(body, "StartY"), RequiredInt(body, "EndX"), RequiredInt(body, "EndY"), duration <= 0 ? 300 : duration, ct);
                    break;
                case "/text": await backend.TextAsync(serial, RequiredString(body, "Text"), ct); break;
                case "/app/start": data = new { output = await backend.StartAppAsync(serial, RequiredString(body, "PackageName"), OptionalString(body, "Activity"), ct) }; break;
                case "/app/stop": data = new { output = await backend.StopAppAsync(serial, RequiredString(body, "PackageName"), ct) }; break;
            }
            await WriteAsync(context, 200, new { ok = true, data });
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested) { }
        catch (Exception e)
        {
            log.Write("basic-api-error", e.Message, serial);
            await WriteAsync(context, ErrorStatus(e), new { ok = false, message = e.Message });
        }
        finally { writeLease?.Dispose(); }
    }
    private async Task HandleAutomatorAsync(HttpContext context)
    {
        IDisposable? writeLease = null;
        try
        {
            var path = NormalizePath(context.Request.Path.Value).Trim('/');
            if (path.StartsWith("api/", StringComparison.Ordinal)) path = path[4..];
            if (context.Request.Method == "GET")
            {
                object data = path switch
                {
                    "health" => new { running = true, prefix = $"http://127.0.0.1:{_automatorPort}/" },
                    "devices" => DeviceSnapshotProvider?.Invoke() ?? catalog.Snapshot,
                    _ => throw new DeviceException("Route not found: " + path, 404, 1006)
                };
                await WriteAsync(context, 200, new { code = 0, message = "ok", data });
                return;
            }
            if (context.Request.Method != "POST") throw new DeviceException("Method not allowed", 405, 1005);
            if (!AutomationService.Routes.Contains(path)) throw new DeviceException("Route not found: " + path, 404, 1006);
            ValidateWriteRequest(context);
            var body = await ReadBodyAsync(context);
            if (path is "device/shell" or "device/execute" or "device/set-prop" or "device/set-timezone" or "device/set-sim-info" or
                "app/start" or "app/stop" or "app/clear" or "input/tap" or "input/swipe" or "input/keyevent" or "service/stop" or
                "element/click-if-exists" or "element/click-text" or "element/set-text" or "element/click-node")
            {
                var serial = RequiredString(body, "serial");
                RequireWriteAvailable(serial);
                writeLease = writes?.Acquire([serial]);
            }
            var dataResult = await automation.ExecuteAsync(path, body, context.RequestAborted);
            await WriteAsync(context, 200, new { code = 0, message = "ok", data = dataResult });
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested) { }
        catch (Exception e)
        {
            log.Write("automator-api-error", e.Message);
            var code = e is DeviceException de ? de.Code : e is JsonException or ArgumentException ? 1007 : 2000;
            await WriteAsync(context, ErrorStatus(e), new { code, message = e.Message });
        }
        finally { writeLease?.Dispose(); }
    }
    private static void ValidateWriteRequest(HttpContext context)
    {
        if (!context.Request.HasJsonContentType()) throw new DeviceException("请求必须使用 application/json", 415, 1007);
        if (context.Request.Headers.TryGetValue("Origin", out var origin) &&
            (!Uri.TryCreate(origin.ToString(), UriKind.Absolute, out var source) || source.Scheme != "http" ||
             source.Port != context.Connection.LocalPort || source.Host is not ("localhost" or "127.0.0.1" or "[::1]")))
            throw new DeviceException("拒绝跨站设备控制请求", 403, 1007);
    }
    private static WebApplication CreateServer(int port)
    {
        var builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions { Args = [] });
        builder.Logging.ClearProviders();
        builder.WebHost.UseKestrel(options => { options.ListenLocalhost(port); options.Limits.MaxRequestBodySize = 1024 * 1024; });
        return builder.Build();
    }
    private static async Task StartServerAsync(WebApplication app, CancellationToken ct)
    {
        try { await app.StartAsync(ct); }
        catch (Exception startError)
        {
            try { await app.DisposeAsync(); }
            catch (Exception cleanupError) { throw new AggregateException("Server startup failed and cleanup also failed", startError, cleanupError); }
            throw;
        }
    }
    private static void ValidatePort(int port)
    {
        if (port is < 1 or > 65535) throw new ArgumentException("Invalid port: " + port);
    }
    private static int ErrorStatus(Exception e) => e switch
    {
        DeviceException de => de.Status,
        JsonException or ArgumentException or BadHttpRequestException => 400,
        TimeoutException => 504,
        _ => 500
    };
    private static async Task<JsonElement> ReadBodyAsync(HttpContext context)
    {
        using var document = await JsonDocument.ParseAsync(context.Request.Body, new JsonDocumentOptions { MaxDepth = 64 }, context.RequestAborted);
        if (document.RootElement.ValueKind != JsonValueKind.Object) throw new ArgumentException("Request body must be a JSON object");
        return document.RootElement.Clone();
    }
    private static JsonElement? Property(JsonElement body, string name)
    {
        JsonElement? result = null;
        foreach (var item in body.EnumerateObject())
        {
            if (!item.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) continue;
            if (result is not null) throw new ArgumentException("Duplicate property: " + name);
            result = item.Value;
        }
        return result;
    }
    private static string RequiredString(JsonElement body, string name) => OptionalString(body, name) is { Length: > 0 } value && !string.IsNullOrWhiteSpace(value) ? value : throw new ArgumentException("Required string: " + name);
    private static string? OptionalString(JsonElement body, string name)
    {
        var value = Property(body, name);
        if (value is null || value.Value.ValueKind == JsonValueKind.Null) return null;
        if (value.Value.ValueKind != JsonValueKind.String) throw new ArgumentException("Expected string: " + name);
        return value.Value.GetString();
    }
    private static int RequiredInt(JsonElement body, string name) => OptionalInt(body, name) ?? throw new ArgumentException("Required integer: " + name);
    private static int? OptionalInt(JsonElement body, string name)
    {
        var value = Property(body, name);
        if (value is null || value.Value.ValueKind == JsonValueKind.Null) return null;
        return value.Value.ValueKind == JsonValueKind.Number && value.Value.TryGetInt32(out var number) ? number : throw new ArgumentException("Expected integer: " + name);
    }
    private static bool OptionalBool(JsonElement body, string name)
    {
        var value = Property(body, name);
        if (value is null) return false;
        if (value.Value.ValueKind is not (JsonValueKind.True or JsonValueKind.False)) throw new ArgumentException("Expected boolean: " + name);
        return value.Value.GetBoolean();
    }
    private static Task WriteAsync(HttpContext context, int status, object value)
    {
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json; charset=utf-8";
        return context.Response.WriteAsync(JsonSerializer.Serialize(value, JsonDefaults.Options), context.RequestAborted);
    }
    public async ValueTask DisposeAsync() { await StopAsync(); _lifecycle.Dispose(); }
}
