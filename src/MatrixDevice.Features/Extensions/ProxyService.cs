using System.Text.Json;
using System.Web;
using MatrixDevice.Core;

namespace MatrixDevice.Features;

public enum ProxyType { Socks5, Vless, Hysteria2 }
public sealed record ProxyProfile
{
    public ProxyType ProxyType { get; init; }
    public required string Host { get; init; }
    public required int Port { get; init; }
    public string? RawUrl { get; init; }
    public string? User { get; init; }
    public string? Password { get; init; }
    public string? Area { get; init; }
    public string? Sni { get; init; }
    public bool Insecure { get; init; }
    public override string ToString() => $"{ProxyType}://{Host}:{Port}";
}

public static class ProxyProfileParser
{
    public static ProxyProfile Parse(string url)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || string.IsNullOrWhiteSpace(uri.Host) || uri.Port is < 1 or > 65535)
            throw Invalid("Proxy URL requires a host and an explicit port in 1..65535.");
        var type = uri.Scheme.ToLowerInvariant() switch
        {
            "socks5" or "socks" => ProxyType.Socks5,
            "vless" => ProxyType.Vless,
            "hy2" or "hysteria2" => ProxyType.Hysteria2,
            _ => throw Invalid($"Unsupported proxy scheme: '{uri.Scheme}'.")
        };
        var credentials = uri.UserInfo.Split(':', 2).Select(Uri.UnescapeDataString).ToArray();
        var query = HttpUtility.ParseQueryString(uri.Query);
        string? user = null; string? password = null;
        switch (type)
        {
            case ProxyType.Socks5:
                user = credentials[0].Length == 0 ? null : credentials[0];
                password = credentials.Length == 2 ? credentials[1] : null;
                break;
            case ProxyType.Vless:
                user = credentials[0];
                if (!Guid.TryParse(user, out _)) throw Invalid("VLESS user information must contain a UUID.");
                break;
            case ProxyType.Hysteria2:
                password = credentials[0];
                if (string.IsNullOrWhiteSpace(password)) throw Invalid("Hysteria2 user information must contain a password.");
                break;
        }
        var insecureText = query["insecure"];
        if (insecureText is not null && insecureText is not "0" and not "1" && !bool.TryParse(insecureText, out _))
            throw Invalid("insecure must be 0, 1, true, or false.");
        return new ProxyProfile
        {
            ProxyType = type, Host = uri.IdnHost, Port = uri.Port, RawUrl = url,
            User = user, Password = password, Area = query["area"], Sni = query["sni"],
            Insecure = insecureText == "1" || bool.TryParse(insecureText, out var insecure) && insecure
        };
    }

    public static string ExportProfileJson(ProxyProfile profile, bool includeCredentials = false)
    {
        ArgumentNullException.ThrowIfNull(profile);
        if (profile.Port is < 1 or > 65535 || string.IsNullOrWhiteSpace(profile.Host)) throw Invalid("Invalid profile host or port.");
        if (!includeCredentials) profile = profile with { User = null, Password = null, RawUrl = profile.ToString() };
        return JsonSerializer.Serialize(profile, JsonDefaults.Options);
    }

    // JSON is a structured YAML 1.2 representation; this is a profile, not a recovered runner configuration.
    public static string ExportProfileYaml(ProxyProfile profile, bool includeCredentials = false) => ExportProfileJson(profile, includeCredentials);
    private static DeviceException Invalid(string message) => new(message, 400, 1007);
}

public sealed class ProxyService(IDeviceBackend backend)
{
    public async Task EnsureRootAsync(string serial, CancellationToken ct = default)
    {
        var uid = (await backend.ShellAsync(serial, "id -u", true, ct).ConfigureAwait(false)).Trim();
        if (uid != "0") throw new DeviceException($"Root is unavailable for device '{serial}'.", 409, 1003);
    }
    public Task StartAsync(string serial, ProxyProfile profile, CancellationToken ct = default) =>
        throw new DeviceException("Proxy runner binaries and device routing rules were not recovered; profile parsing and export are available.", 501, 2001);
    public Task StopAsync(string serial, CancellationToken ct = default) =>
        throw new DeviceException("No proxy runner owned by this application is deployed.", 501, 2001);
}

public sealed record EnvironmentConfiguration
{
    public string? Country { get; init; }
    public string? Language { get; init; }
    public string? Timezone { get; init; }
    public string? OperatorName { get; init; }
    public string? Mcc { get; init; }
    public string? Mnc { get; init; }
    public string? Iccid { get; init; }
    public string? Imsi { get; init; }
    public string? PhoneNumber { get; init; }
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    public string? KeyboxXml { get; init; }
    public string? PifJson { get; init; }
}

public sealed class EnvironmentConfigurationService(IDeviceBackend backend)
{
    public async Task ApplyAsync(string serial, EnvironmentConfiguration configuration, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        if (configuration.Country is not null || configuration.Language is not null || configuration.OperatorName is not null ||
            configuration.Mcc is not null || configuration.Mnc is not null || configuration.Iccid is not null || configuration.Imsi is not null ||
            configuration.PhoneNumber is not null || configuration.Latitude is not null || configuration.Longitude is not null ||
            configuration.KeyboxXml is not null || configuration.PifJson is not null)
            throw new DeviceException("These environment fields require undocumented ROM APIs; no fields were applied.", 501, 2001);
        if (string.IsNullOrWhiteSpace(configuration.Timezone)) throw new DeviceException("A timezone is required.", 400, 1007);
        try { TimeZoneInfo.FindSystemTimeZoneById(configuration.Timezone); }
        catch (TimeZoneNotFoundException) { throw new DeviceException($"Unknown timezone: '{configuration.Timezone}'.", 400, 1007); }
        await backend.ShellAsync(serial, "setprop persist.sys.timezone " + AutomationService.Quote(configuration.Timezone), true, ct).ConfigureAwait(false);
        var actual = (await backend.ShellAsync(serial, "getprop persist.sys.timezone", ct: ct).ConfigureAwait(false)).Trim();
        if (actual != configuration.Timezone) throw new DeviceException($"Timezone verification failed: '{actual}'.", 409, 1003);
    }
}
