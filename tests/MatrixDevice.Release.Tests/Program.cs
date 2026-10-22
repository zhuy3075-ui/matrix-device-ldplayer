using System.Diagnostics;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text.Json;
using MatrixDevice.Core;
using MatrixDevice.Host;

if (args.Length != 3 || args.Any(value => !Path.IsPathFullyQualified(value)) || Directory.Exists(args[2]))
    throw new ArgumentException("Expected dotnet, production project root, fresh output directory.");
var dotnet = args[0]; var root = args[1]; var output = args[2];
Directory.CreateDirectory(output);
var checks = new List<string>();
void Check(bool ok, string description) { if (!ok) throw new InvalidOperationException(description); checks.Add(description); Console.WriteLine("PASS " + description); }
var desktop = Path.Combine(root, "src/MatrixDevice.Desktop/bin/Release/net10.0-windows/MatrixDevice.Desktop.dll");
var cli = Path.Combine(root, "src/MatrixDevice.Cli/bin/Release/net10.0/MatrixDevice.Cli.dll");
var core = Path.Combine(root, "src/MatrixDevice.Core/bin/Release/net10.0/MatrixDevice.Core.dll");
string[] Names(string path, bool methods)
{
    using var stream = File.OpenRead(path); using var pe = new PEReader(stream); var metadata = pe.GetMetadataReader();
    return methods ? metadata.MethodDefinitions.Select(handle => metadata.GetString(metadata.GetMethodDefinition(handle).Name)).ToArray()
        : metadata.TypeDefinitions.Select(handle => metadata.GetString(metadata.GetTypeDefinition(handle).Name)).ToArray();
}
Check(!Names(core, false).Any(name => name.Contains("Demo", StringComparison.Ordinal)), "production Core has no Demo generator types");
Check(!Names(desktop, false).Any(name => name.Contains("Smoke", StringComparison.Ordinal) || name.EndsWith("Acceptance", StringComparison.Ordinal)), "production Desktop has no fixture or acceptance types");
Check(!Names(desktop, true).Any(name => name is "RunSmokeAsync" or "RunPrimaryInstanceVerificationAsync" or "RunLiveUiAcceptanceAsync" or "RunScriptWorkspaceVerificationAsync"), "production Desktop has no acceptance methods");
Check(!Names(desktop, true).Any(name => name.Contains("Fixture", StringComparison.Ordinal)), "production Desktop has no fixture override methods or properties");
Check(typeof(DeviceCatalog).Assembly.GetType("MatrixDevice.Core.DemoDeviceBackend") is null, "release test references non-fixture Core");
var denied = Path.Combine(output, "denied-runtime");
try { await using var unexpected = await MatrixRuntime.CreateAsync(new AppSettings(), denied, demo: true); throw new InvalidOperationException("Production factory accepted Demo."); }
catch (ArgumentException error) when (error.ParamName == "demo") { Check(!Directory.Exists(denied), "production factory rejects Demo before directory creation"); }
await using (var runtime = await MatrixRuntime.CreateAsync(new AppSettings { AutoStartApi = false, AutoConnect = false }, Path.Combine(output, "real-empty")))
{
    Check(!runtime.IsDemo && runtime.Backend is AdbDeviceBackend && runtime.GetDevices().Count == 0 && runtime.LdPlayer is null, "unconfigured real Runtime is empty, not synthesized devices");
    Check(!runtime.Api.BasicRunning && !runtime.Api.AutomatorRunning && runtime.Api.DevicePorts.Count == 0, "real empty Runtime does not start APIs");
}
async Task Reject(string assembly, params string[] flags)
{
    var data = Path.Combine(output, "reject-" + checks.Count);
    var start = new ProcessStartInfo(dotnet) { UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true };
    start.ArgumentList.Add(assembly);
    start.ArgumentList.Add(assembly == desktop ? "--data-dir" : "--data"); start.ArgumentList.Add(data);
    foreach (var flag in flags) start.ArgumentList.Add(flag);
    using var process = Process.Start(start) ?? throw new InvalidOperationException("Cannot start owned contract process.");
    var stdout = process.StandardOutput.ReadToEndAsync(); var stderr = process.StandardError.ReadToEndAsync();
    try { await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(15)); }
    catch { process.Kill(entireProcessTree: true); await process.WaitForExitAsync(); throw; }
    var standardOutput = await stdout; var standardError = await stderr;
    File.WriteAllText(Path.Combine(output, "process-" + checks.Count + ".json"), JsonSerializer.Serialize(new { assembly, flags, process.ExitCode, dataDirectoryCreated = Directory.Exists(data), stdout = standardOutput, stderr = standardError }, new JsonSerializerOptions { WriteIndented = true }));
    Check(process.ExitCode != 0 && !Directory.Exists(data) && flags.Any(flag => standardError.Contains(flag, StringComparison.Ordinal)), Path.GetFileName(assembly) + " rejects " + string.Join(' ', flags) + " before workspace initialization");
}
foreach (var flag in new[] { "--demo", "--smoke", "--verify-primary-instance", "--probe-workspace-client", "--unknown" }) await Reject(desktop, flag);
await Reject(desktop, "--data-dir", "relative");
await Reject(desktop, "--ldconsole", Path.Combine(output, "missing-console.exe"));
await Reject(cli, "--demo");
await Reject(cli, "--demo", "--ldplayer-list");
File.WriteAllText(Path.Combine(output, "summary.json"), JsonSerializer.Serialize(new { scope = "release-contracts-only-no-guest-no-launch", checks, count = checks.Count }, new JsonSerializerOptions { WriteIndented = true }));
Console.WriteLine($"RESULT {checks.Count}/{checks.Count} release contracts passed");
