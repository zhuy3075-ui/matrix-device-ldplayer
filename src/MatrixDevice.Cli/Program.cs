using System.Text.Json;
using MatrixDevice.Core;
using MatrixDevice.Host;

var allowed = new HashSet<string> { "--demo", "--serve", "--data", "--adb", "--serial", "--ldconsole", "--ldplayer-list", "--basic-port", "--automator-port", "--help" };
var values = new Dictionary<string, string?>();
for (var i = 0; i < args.Length; i++)
{
    if (!allowed.Contains(args[i])) throw new ArgumentException("未知参数：" + args[i]);
    var key = args[i];
    values[key] = key is "--data" or "--adb" or "--serial" or "--ldconsole" or "--basic-port" or "--automator-port"
        ? ++i < args.Length ? args[i] : throw new ArgumentException("参数缺少值：" + key) : null;
}
if (values.ContainsKey("--help"))
{
    Console.WriteLine("MatrixDevice.Cli [--demo] [--serve] [--data 路径] [--serial 授权串号] [--ldconsole 绝对路径] [--ldplayer-list] [--basic-port 端口] [--automator-port 端口]");
    return;
}
var directory = Path.GetFullPath(values.GetValueOrDefault("--data") ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MatrixDevice.Reconstructed"));
var store = new SettingsStore(Path.Combine(directory, "AppConfig.json"));
var settings = store.Load();
if (values.GetValueOrDefault("--adb") is { } adb) settings = settings with { AdbPath = adb };
if (values.GetValueOrDefault("--serial") is { } serial) settings = settings with { AdbAllowedSerials = [serial] };
if (values.GetValueOrDefault("--ldconsole") is { } ldConsole) settings = settings with { LdConsolePath = ldConsole };
if (values.GetValueOrDefault("--basic-port") is { } basic) settings = settings with { BasicApiPort = int.Parse(basic) };
if (values.GetValueOrDefault("--automator-port") is { } automator) settings = settings with { AutomatorApiPort = int.Parse(automator) };
using var cancellation = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cancellation.Cancel(); };
if (values.ContainsKey("--ldplayer-list"))
{
    // Read-only mode must never construct the ADB backend or invoke an ADB client.
    await using var service = new LdPlayerService(values.ContainsKey("--demo")
        ? new DemoLdConsoleRunner() : new LdConsoleRunner(settings.LdConsolePath, settings.CommandTimeoutSeconds), values.ContainsKey("--demo"));
    await service.RefreshAsync(cancellation.Token);
    Console.WriteLine(JsonSerializer.Serialize(service.Instances, JsonDefaults.Options));
    return;
}
await using var runtime = await MatrixRuntime.CreateAsync(settings with { AutoStartApi = false }, directory, values.ContainsKey("--demo"));
Console.WriteLine(JsonSerializer.Serialize(new { mode = runtime.IsDemo ? "demo" : runtime.LdPlayer is not null && settings.AdbAllowedSerials.Length == 0 ? "ldplayer" : "adb-existing-server", discoveryError = runtime.DiscoveryError, devices = runtime.GetDevices(), ldPlayerError = runtime.LdPlayerError, ldPlayerInstances = runtime.LdPlayer?.Instances }, JsonDefaults.Options));
if (values.ContainsKey("--serve"))
{
    await runtime.StartApisAsync(cancellation.Token);
    foreach (var device in runtime.Devices.Snapshot.Where(x => x.Online)) await runtime.Api.StartDeviceAsync(device.Serial, cancellation.Token);
    Console.WriteLine(JsonSerializer.Serialize(new
    {
        basicGateway = $"http://localhost:{settings.BasicApiPort}/",
        automatorGateway = $"http://127.0.0.1:{settings.AutomatorApiPort}/",
        devicePorts = runtime.Api.DevicePorts
    }, JsonDefaults.Options));
    try
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(3));
        while (await timer.WaitForNextTickAsync(cancellation.Token))
        {
            try
            {
                if (runtime.IsDemo || settings.AdbAllowedSerials.Length > 0) await runtime.RefreshAsync(cancellation.Token);
                if (runtime.LdPlayer is not null) await runtime.RefreshLdPlayerAsync(cancellation.Token);
            }
            catch (Exception e) when (e is not OperationCanceledException) { runtime.Log.Write("discovery-error", e.Message); Console.Error.WriteLine(e.Message); }
        }
    }
    catch (OperationCanceledException) when (cancellation.IsCancellationRequested) { }
}
