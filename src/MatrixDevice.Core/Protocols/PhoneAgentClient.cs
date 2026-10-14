using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;

namespace MatrixDevice.Core;

public sealed class PhoneAgentClient : IAsyncDisposable
{
    private readonly HttpClient _http;
    public const int MaximumResponseBytes = 32 * 1024 * 1024;
    public Uri BaseUrl { get; }
    public PhoneAgentClient(string baseUrl, HttpMessageHandler? handler = null)
    {
        if (!Uri.TryCreate(baseUrl.EndsWith('/') ? baseUrl : baseUrl + "/", UriKind.Absolute, out var uri) || uri.Scheme is not ("http" or "https"))
            throw new ArgumentException($"Invalid agent BaseURL: {baseUrl}", nameof(baseUrl));
        BaseUrl = uri;
        _http = handler is null ? new HttpClient() : new HttpClient(handler);
        _http.BaseAddress = uri;
        _http.Timeout = TimeSpan.FromSeconds(15);
        _http.DefaultRequestHeaders.ConnectionClose = true;
    }

    public async Task<JsonElement> SendAsync(HttpMethod method, string path, object? body = null, IReadOnlyDictionary<string, string?>? query = null, CancellationToken ct = default)
    {
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(ct);
        deadline.CancelAfter(TimeSpan.FromSeconds(15));
        try { return await SendCoreAsync(method, path, body, query, deadline.Token).ConfigureAwait(false); }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested) { throw new DeviceException($"Agent HTTP request timed out: {method} {path}.", 504, 2000); }
    }

    private async Task<JsonElement> SendCoreAsync(HttpMethod method, string path, object? body, IReadOnlyDictionary<string, string?>? query, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (Uri.TryCreate(path, UriKind.Absolute, out _)) throw new ArgumentException("Agent paths must be relative.", nameof(path));
        var target = path.TrimStart('/');
        if (query is not null)
        {
            var values = query.Where(item => item.Value is not null).Select(item => Uri.EscapeDataString(item.Key) + "=" + Uri.EscapeDataString(item.Value!));
            var encoded = string.Join("&", values);
            if (encoded.Length > 0) target += "?" + encoded;
        }
        using var request = new HttpRequestMessage(method, target);
        if (body is not null) request.Content = JsonContent.Create(body, options: JsonDefaults.Options);
        HttpResponseMessage response;
        try { response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false); }
        catch (HttpRequestException ex) { throw new DeviceException($"Agent HTTP request failed: {method} {path}: {ex.Message}", 503, 2000); }
        using (response)
        {
            if (response.Content.Headers.ContentLength > MaximumResponseBytes) throw new DeviceException($"Agent response exceeds {MaximumResponseBytes} bytes: {path}.", 413, 1007);
            await using var stream = await response.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
            var bytes = await BoundedStreamRead.ReadAsync(stream, MaximumResponseBytes, ct).ConfigureAwait(false);
            var text = System.Text.Encoding.UTF8.GetString(bytes);
            if (!response.IsSuccessStatusCode) throw new DeviceException($"Agent returned HTTP {(int)response.StatusCode}: {path}: {text}", 502, 2000);
            try
            {
                using var doc = JsonDocument.Parse(text);
                var root = doc.RootElement;
                if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty("code", out var code) || code.ValueKind != JsonValueKind.Number || !code.TryGetInt32(out var value))
                    throw new JsonException("Envelope must be an object with an integer code.");
                if (value != 0)
                {
                    var hasMessage = root.TryGetProperty("message", out var detail) && detail.ValueKind == JsonValueKind.String;
                    var message = hasMessage ? detail.GetString() : "message omitted or invalid";
                    throw new DeviceException($"Agent API failed: {path}, code={value}, message={message}", hasMessage ? 409 : 502, value);
                }
                return root.TryGetProperty("data", out var data) ? data.Clone() : JsonSerializer.SerializeToElement<object?>(null);
            }
            catch (JsonException ex) { throw new DeviceException($"Agent returned invalid JSON: {path}: {ex.Message}", 502, 2000); }
        }
    }

    public async Task<AndroidNode> DumpAsync(bool compressed = true, CancellationToken ct = default)
    {
        var data = await SendAsync(HttpMethod.Get, "device/dump", query: new Dictionary<string, string?> { ["compressed"] = compressed ? "true" : "false" }, ct: ct);
        if (data.ValueKind != JsonValueKind.Object) throw new DeviceException($"Agent dump must be an AndroidNode object; received {data.ValueKind}.", 502, 2000);
        try { return data.Deserialize<AndroidNode>(JsonDefaults.Options) ?? throw new DeviceException("Agent dump was null.", 502, 2000); }
        catch (JsonException ex) { throw new DeviceException("Invalid agent AndroidNode data: " + ex.Message, 502, 2000); }
    }

    public Task<JsonElement> ShellAsync(string command, CancellationToken ct = default) => SendAsync(HttpMethod.Post, "device/cmd", new { cmd = command }, ct: ct);
    public Task<JsonElement> ListAppsAsync(CancellationToken ct = default) => SendAsync(HttpMethod.Get, "app/list", ct: ct);
    public Task<JsonElement> StartAppAsync(string packageName, CancellationToken ct = default) => SendAsync(HttpMethod.Post, "app/start", new { package = packageName }, ct: ct);
    public Task<JsonElement> StopAppAsync(string packageName, CancellationToken ct = default) => SendAsync(HttpMethod.Post, "app/stop", new { package = packageName }, ct: ct);
    public Task<JsonElement> TapAsync(int x, int y, CancellationToken ct = default) => SendAsync(HttpMethod.Post, "input/tap", new { x, y }, ct: ct);
    public Task<JsonElement> SwipeAsync(int x1, int y1, int x2, int y2, int steps = 55, int stepMs = 5, CancellationToken ct = default) =>
        SendAsync(HttpMethod.Post, "input/swipe", new { x1, y1, x2, y2, steps, stepMs }, ct: ct);
    public Task<JsonElement> KeyEventAsync(int code, CancellationToken ct = default) => SendAsync(HttpMethod.Post, "input/keyevent", new { code }, ct: ct);
    public Task<JsonElement> ServiceInfoAsync(CancellationToken ct = default) => SendAsync(HttpMethod.Get, "service/info", ct: ct);
    public Task<JsonElement> StopServiceAsync(CancellationToken ct = default) => SendAsync(HttpMethod.Get, "service/stop", ct: ct);
    public Task<JsonElement> InfoAsync(CancellationToken ct = default) => SendAsync(HttpMethod.Get, "info/detail", ct: ct);

    public Task<JsonElement> NodeInfoAsync(NodeSelector selector, CancellationToken ct = default) => SendAsync(HttpMethod.Get, "node/info", query: SelectorQuery(selector), ct: ct);
    public Task<JsonElement> NodeExistsAsync(NodeSelector selector, CancellationToken ct = default) => SendAsync(HttpMethod.Get, "node/existsVisible", query: SelectorQuery(selector), ct: ct);
    public Task<JsonElement> ClickNodeAsync(NodeSelector selector, bool longClick = false, CancellationToken ct = default) => SendAsync(HttpMethod.Post, longClick ? "node/long-click" : "node/click", selector, ct: ct);
    public Task<JsonElement> SetNodeTextAsync(NodeSelector selector, string text, CancellationToken ct = default)
    {
        var fields = SelectorQuery(selector).ToDictionary(item => item.Key, item => (object?)item.Value);
        foreach (var property in typeof(NodeSelector).GetProperties())
        {
            var value = property.GetValue(selector);
            if (value is not null) fields[JsonNamingPolicy.CamelCase.ConvertName(property.Name)] = value;
        }
        fields["set_text"] = text;
        return SendAsync(HttpMethod.Post, "node/set-text", fields, ct: ct);
    }

    public async Task<byte[]> ScreenshotAsync(int? x = null, int? y = null, int? width = null, int? height = null, int? quality = null, CancellationToken ct = default)
    {
        var values = new Dictionary<string, string?> { ["x"] = x?.ToString(CultureInfo.InvariantCulture), ["y"] = y?.ToString(CultureInfo.InvariantCulture), ["width"] = width?.ToString(CultureInfo.InvariantCulture), ["height"] = height?.ToString(CultureInfo.InvariantCulture), ["quality"] = quality?.ToString(CultureInfo.InvariantCulture) };
        var data = await SendAsync(HttpMethod.Get, "device/screenshot", query: values, ct: ct);
        string? encoded = null;
        if (data.ValueKind == JsonValueKind.String) encoded = data.GetString();
        else if (data.ValueKind == JsonValueKind.Object && data.TryGetProperty("base64", out var base64) && base64.ValueKind == JsonValueKind.String) encoded = base64.GetString();
        if (string.IsNullOrEmpty(encoded)) throw new DeviceException("Agent screenshot did not contain base64 PNG bytes.", 502, 2000);
        if (encoded.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        {
            var comma = encoded.IndexOf(',');
            if (comma < 0) throw new DeviceException("Invalid screenshot data URI: comma separator is missing.", 502, 2000);
            encoded = encoded[(comma + 1)..];
        }
        try { var png = Convert.FromBase64String(encoded); PngImage.ReadDimensions(png); return png; }
        catch (Exception ex) when (ex is FormatException or InvalidDataException) { throw new DeviceException($"Invalid agent PNG screenshot: {ex.Message}", 502, 2000); }
    }

    private static IReadOnlyDictionary<string, string?> SelectorQuery(NodeSelector selector) =>
        typeof(NodeSelector).GetProperties().Where(property => property.GetValue(selector) is not null).ToDictionary(property => JsonNamingPolicy.CamelCase.ConvertName(property.Name), property =>
        {
            var value = property.GetValue(selector);
            return value is bool flag ? flag.ToString().ToLowerInvariant() : Convert.ToString(value, CultureInfo.InvariantCulture);
        });
    public ValueTask DisposeAsync() { _http.Dispose(); return ValueTask.CompletedTask; }
}
