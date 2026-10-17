using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Windows;
using MatrixDevice.Core;
using MatrixDevice.Features;
using MatrixDevice.Host;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    private sealed record DualSyncServer(int Pid, long StartedUtcTicks, string Path, string Sha256);
    private sealed record DualSyncTarget(int Index, string Name, string Serial, bool AllowLaunch, long PlayerStartedUtcTicks, long VBoxStartedUtcTicks, uint? TransportId);
    private sealed record DualSyncManifest(int Version, string ConsolePath, string ConsoleSha256, DualSyncServer Server, DualSyncTarget[] Targets, LdPlayerInstance[] Directory);
    private sealed record DualSyncFrame(string Serial, string Foreground, AndroidNode Root, int Width, int Height, string Png, string PngSha256);
    private sealed class DualSyncFailure(string classification, string message) : Exception(message)
    { internal string Classification { get; } = classification; }

    internal static async Task RunLiveDualSyncAcceptanceAsync(string[] args, bool filesTerminal = false)
    {
        var mode = filesTerminal ? "--verify-live-file-terminal" : "--verify-live-dual-sync";
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        for (var index = 0; index < args.Length; index++)
        {
            var key = args[index];
            if (key == mode) { if (!values.TryAdd(key, "")) throw new ArgumentException("Duplicate mode."); continue; }
            if (key is not ("--manifest" or "--manifest-sha256" or "--data-dir") || !values.TryAdd(key, "")) throw new ArgumentException("Unknown or duplicate dual-sync argument: " + key);
            if (++index >= args.Length || args[index].StartsWith("--", StringComparison.Ordinal)) throw new ArgumentException("Missing value: " + key);
            values[key] = args[index];
        }
        if (values.Count != 4 || !values.ContainsKey(mode) || !values.ContainsKey("--manifest") || !values.ContainsKey("--manifest-sha256") || !values.ContainsKey("--data-dir")) throw new ArgumentException("Dual sync requires exactly mode, manifest, SHA256 and data-dir.");
        var output = DualSyncAbsolute(values["--data-dir"]); var input = DualSyncAbsolute(values["--manifest"]);
        if (!output.Contains(Path.DirectorySeparatorChar + "artifacts" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
            || Directory.Exists(output) || File.Exists(output) || input.StartsWith(output.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("Fresh separate absolute artifacts output required.");
        if (filesTerminal) ValidateFileTerminalPaths(input, output);
        if (!DualSyncSha(values["--manifest-sha256"])) throw new ArgumentException("Manifest SHA256 must be 64 hex digits.");
        using var lease = new FileStream(input, FileMode.Open, FileAccess.Read, FileShare.Read);
        if (lease.Length is <= 0 or > 65536) throw new ArgumentException("Manifest size must be 1..65536 bytes.");
        var bytes = new byte[checked((int)lease.Length)]; lease.ReadExactly(bytes); var manifestSha = Convert.ToHexString(SHA256.HashData(bytes));
        if (!manifestSha.Equals(values["--manifest-sha256"], StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("Manifest SHA256 mismatch.");
        using var json = JsonDocument.Parse(bytes, new JsonDocumentOptions { MaxDepth = 32 }); DualSyncUniqueKeys(json.RootElement);
        var manifest = JsonSerializer.Deserialize<DualSyncManifest>(bytes, new JsonSerializerOptions(JsonDefaults.Options) { UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow }) ?? throw new ArgumentException("Empty manifest.");
        DualSyncValidateManifest(manifest);
        if (filesTerminal && manifest.Targets.Any(target => target.AllowLaunch)) throw new ArgumentException("File/terminal acceptance forbids every launch; both dedicated instances must already be Ready.");
        if (Application.Current.Windows.Count != 0 || Application.Current.MainWindow is not null) throw new InvalidOperationException("Live VM sync must start without an application window.");
        Directory.CreateDirectory(output); await File.WriteAllBytesAsync(Path.Combine(output, "manifest-input.json"), bytes);
        var state = new DualSyncRun(manifest, output, filesTerminal);
        await state.Run(manifestSha);
    }

    private static void DualSyncValidateManifest(DualSyncManifest manifest)
    {
        if (manifest.Version != 1 || manifest.Server is null || manifest.Targets is not { Length: 2 } || manifest.Directory is null || manifest.Directory.Length is < 3 or > 1000
            || manifest.Targets.Any(target => target is null) || manifest.Directory.Any(row => row is null)) throw new ArgumentException("Invalid fixed dual-sync manifest shape.");
        if (!DualSyncAbsolute(manifest.ConsolePath).Equals(@"E:\leidian\LDPlayer14\ldconsole.exe", StringComparison.OrdinalIgnoreCase)
            || !string.Equals(manifest.ConsoleSha256, "AED5E30D66B02FD15FD5D90828C96ECA444B948CC839A583C289BD4E1D09EDF4", StringComparison.OrdinalIgnoreCase)
            || !DualSyncAbsolute(manifest.Server.Path).Equals(@"E:\leidian\LDPlayer14\adb.exe", StringComparison.OrdinalIgnoreCase)
            || !string.Equals(manifest.Server.Sha256, "705DDC21F33AC52105D1B075B019962AD0E44FB3D560BDE69CE8CB3A36BCA183", StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("This acceptance is locked to the approved LDPlayer14 console and ADB executable identities.");
        if (!File.Exists(manifest.ConsolePath) || !File.Exists(manifest.Server.Path) || !DualSyncSha(manifest.ConsoleSha256) || !DualSyncSha(manifest.Server.Sha256)
            || manifest.Server.Pid <= 0 || manifest.Server.StartedUtcTicks <= 0) throw new ArgumentException("Console/server proof is incomplete.");
        if (!manifest.Targets.Select(target => target.Index).Order().SequenceEqual(new[] { 1, 2 }) || manifest.Targets.Select(target => target.Serial).Distinct(StringComparer.Ordinal).Count() != 2
            || manifest.Directory.Select(row => row.Index).Distinct().Count() != manifest.Directory.Length || !manifest.Directory.Any(row => row.Index == 0)) throw new ArgumentException("Only unique exact dedicated instances 1/2 and a complete directory including 0 are accepted.");
        foreach (var target in manifest.Targets)
        {
            var row = manifest.Directory.SingleOrDefault(item => item.Index == target.Index) ?? throw new ArgumentException("Missing target directory row.");
            var approvedName = target.Index == 1 ? "MatrixAcceptance-20261014-184112" : "MatrixAcceptance-20261014-205326";
            if (!string.Equals(target.Name, approvedName, StringComparison.Ordinal)) throw new ArgumentException("Target name is outside the two approved dedicated instances.");
            if (string.IsNullOrWhiteSpace(target.Name) || target.Name != row.Name || string.IsNullOrWhiteSpace(target.Serial) || !Regex.IsMatch(target.Serial, @"^(emulator-[1-9][0-9]{0,4}|127\.0\.0\.1:[1-9][0-9]{0,4})$")
                || row.Width is not > 0 || row.Height is not > 0 || row.Dpi is not > 0 || row.IsStarting) throw new ArgumentException("Target name, serial or original dimensions are invalid.");
            if (target.Index == 1 && (target.AllowLaunch || !row.AndroidStarted || row.ProcessId <= 0 || row.VBoxProcessId <= 0 || target.PlayerStartedUtcTicks <= 0 || target.VBoxStartedUtcTicks <= 0 || target.TransportId is not > 0)) throw new ArgumentException("Instance 1 must already be running with complete fixed process/transport proof.");
            if (target.Index == 2 && target.AllowLaunch && (row.AndroidStarted || row.ProcessId > 0 || row.VBoxProcessId > 0 || row.TopWindow != 0 || row.BindWindow != 0 || target.PlayerStartedUtcTicks != 0 || target.VBoxStartedUtcTicks != 0 || target.TransportId is not null)) throw new ArgumentException("Instance 2 must be explicitly cold with no windows/processes/transport; only it may launch.");
            if (target.Index == 2 && !target.AllowLaunch && (!row.AndroidStarted || row.ProcessId <= 0 || row.VBoxProcessId <= 0 || target.PlayerStartedUtcTicks <= 0 || target.VBoxStartedUtcTicks <= 0 || target.TransportId is not > 0)) throw new ArgumentException("Ready instance 2 requires exact native process births and Ready transport; relaunch is forbidden.");
        }
        var readyTargets = manifest.Targets.Where(target => !target.AllowLaunch).ToArray();
        if (readyTargets.Select(target => target.TransportId).Distinct().Count() != readyTargets.Length) throw new ArgumentException("Approved Ready targets must have unique transports.");
    }

    private sealed partial class DualSyncRun(DualSyncManifest manifest, string output, bool filesTerminal = false)
    {
        private readonly CancellationTokenSource deadline = new(filesTerminal ? TimeSpan.FromMinutes(3) : TimeSpan.FromMinutes(10));
        private readonly List<object> steps = [], cleanupErrors = [];
        private readonly List<Exception> reportErrors = [], observedOperationErrors = [];
        private readonly List<Task> owned = [];
        private readonly ConcurrentQueue<Exception> targetErrors = new();
        private readonly Dictionary<int, (LdPlayerInstance Row, long PlayerBirth, long VBoxBirth)> ready = [];
        private readonly Dictionary<string, DeviceInfo> devices = new(StringComparer.Ordinal);
        private LdPlayerService? service;
        private AdbDeviceBackend? seed;
        private AppSettings? connectionSettings;
        private MatrixRuntime? runtime;
        private Task<MatrixRuntime>? runtimeCreation;
        private WorkspaceViewModel? vm;
        private int sequence, launches;
        private bool runtimeDisposeAttempted, runtimeDisposeCompleted;
        private readonly List<string> planned = filesTerminal
            ? ["bootstrap", "runtime", "sources", "claim-1", "claim-2", "terminal-1", "terminal-2", "upload", "download", "file-shell-1", "file-shell-2"]
            : ["bootstrap", "runtime", "00-tap", "00-swipe", "00-home", "10-tap", "10-swipe", "10-home", "01-tap", "01-swipe", "01-home", "11-tap", "11-swipe", "11-home", "unselected-tap", "unselected-home"];
        private readonly HashSet<string> started = [], completed = [];
        private CancellationToken Ct => deadline.Token;
        private DualSyncTarget Target(int index) => manifest.Targets.Single(target => target.Index == index);
        private DeviceRow Row(int index) => vm!.Devices.Single(row => row.Serial == Target(index).Serial);
        private static void Need(bool condition, string message, string kind = "PRECONDITION-FAIL") { if (!condition) throw new DualSyncFailure(kind, message); }
        private async Task Save(string file, object value) => await File.WriteAllTextAsync(Path.Combine(output, file), JsonSerializer.Serialize(value, JsonDefaults.Options));
        private async Task Trace(string stage, object value) => await File.AppendAllTextAsync(Path.Combine(output, "steps.jsonl"), JsonSerializer.Serialize(new { utc = DateTimeOffset.UtcNow, stage, value }, new JsonSerializerOptions(JsonDefaults.Options) { WriteIndented = false }) + "\n");
        private async Task<T> Step<T>(string name, Func<Task<T>> operation)
        {
            var clock = Stopwatch.StartNew(); var file = (++sequence).ToString("D3") + "-" + name + ".json";
            try
            {
                Ct.ThrowIfCancellationRequested(); var task = operation(); owned.Add(task); var value = await task.WaitAsync(Ct);
                await Save(file, value!); var report = new { name, status = "PASS-LIVE-VM-SDK-NOT-GUI", elapsedMs = clock.Elapsed.TotalMilliseconds, file, sha256 = DualSyncHash(Path.Combine(output, file)) };
                steps.Add(report); await Trace(name, report); Console.WriteLine("PASS-LIVE-VM-SDK-NOT-GUI " + name); return value;
            }
            catch (Exception error)
            {
                var report = new { name, status = error is DualSyncFailure known ? known.Classification : "OPERATION-FAIL", elapsedMs = clock.Elapsed.TotalMilliseconds, error = error.ToString() };
                steps.Add(report);
                try { await Trace(name, report); } catch (Exception writeError) { throw new AggregateException(error, writeError); }
                throw;
            }
        }
        private async Task<T> AfterGuard<T>(Func<Task<T>> action, Func<CancellationToken, Task> after)
        {
            T value = default!; Exception? failure = null;
            try { value = await action(); } catch (Exception error) { failure = error; }
            using var audit = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            try { await after(audit.Token); } catch (Exception error) { failure = failure is null ? error : new AggregateException(failure, error); }
            if (failure is not null) System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(failure).Throw();
            return value;
        }
        internal async Task Run(string manifestSha)
        {
            Exception? failure = null;
            try
            {
                await Save("scope.json", new { manifestSha, scope = filesTerminal ? "Real original VM ShellCommand and file queue, no Window, UI input or physical GUI." : "Real WorkspaceViewModel/InputCoordinator/ADB effects, no Window or physical GUI.",
                    setup = filesTerminal ? "Both Ready/noLaunch. Only exclusive nonce-owned temporary directories; no Settings task or current application change." : "Rebuild only dedicated 1/2 Android Settings UI tasks using NEW_TASK|CLEAR_TASK. This changes the UI task/back stack; no app-data clear or setting toggle. Original back stacks are not restored.",
                    initialization = "Existing runtime discovery reads getprop/wm size/dumpsys input. Their internal successful shell byte streams and SDK dump XML are not exposed; returned snapshots, explicit shell text, PNG bytes and errors are retained. Outer runner captures original stdout/stderr bytes.",
                    forbidden = new[] { "instance 0 guest operations", "quit/reboot/modify/clone", "external adb.exe", "ADB restart/connect", "API/Agent", "accounts/permissions/settings switches", "app install/login", "OS input/Sky" } });
                started.Add("bootstrap"); await Bootstrap(); completed.Add("bootstrap");
                started.Add("runtime"); await CreateRuntime(); completed.Add("runtime");
                if (filesTerminal) await RunFileTerminal();
                else
                {
                foreach (var bits in new[] { (M: false, K: false), (M: true, K: false), (M: false, K: true), (M: true, K: true) })
                {
                    vm!.SyncMouse = bits.M; vm.SyncKeyboard = bits.K; SelectB(true);
                    var tag = (bits.M ? "1" : "0") + (bits.K ? "1" : "0");
                    foreach (var action in new[] { "tap", "swipe", "home" }) { started.Add(tag + "-" + action); await Exercise(tag + "-" + action, action, action == "home" ? bits.K : bits.M); completed.Add(tag + "-" + action); }
                }
                vm!.SyncMouse = vm.SyncKeyboard = true; SelectB(false);
                foreach (var action in new[] { "tap", "home" }) { started.Add("unselected-" + action); await Exercise("unselected-" + action, action, false); completed.Add("unselected-" + action); }
                }
                await HostIdentity("final-before-cleanup", Ct);
            }
            catch (Exception error) { failure = error; deadline.Cancel(); }
            finally
            {
                async Task<bool> Cleanup(string name, Func<Task> action)
                { try { var task = action(); owned.Add(task); await task.WaitAsync(TimeSpan.FromSeconds(20)); return true; } catch (Exception error) { cleanupErrors.Add(new { name, error = error.ToString() }); return false; } }
                if (runtimeCreation is not null && runtime is null)
                {
                    try { runtime = await runtimeCreation.WaitAsync(TimeSpan.FromSeconds(20)); }
                    catch (Exception error) { if (!runtimeCreation.IsCompleted) cleanupErrors.Add(new { name = "runtime-creation-terminal", error = error.ToString() }); else observedOperationErrors.Add(error); }
                }
                if (filesTerminal && vm?.Transferring == true) vm.CancelTransfersCommand.Execute(null);
                await Cleanup("owned-operations-terminal", () => Task.WhenAll(owned.ToArray().Select(async task => { try { await task; } catch (Exception error) { observedOperationErrors.Add(error); } })));
                var creationAtOwnership = runtimeCreation;
                var creationStatusAtOwnership = creationAtOwnership?.Status;
                var ownershipUnresolved = creationAtOwnership is not null && creationStatusAtOwnership is not (TaskStatus.RanToCompletion or TaskStatus.Canceled or TaskStatus.Faulted)
                    || filesTerminal && owned.Any(task => !task.IsCompleted);
                if (!ownershipUnresolved && runtime is null && creationStatusAtOwnership == TaskStatus.RanToCompletion) runtime = creationAtOwnership!.Result;
                if (filesTerminal && !ownershipUnresolved && runtime is not null)
                {
                    await Cleanup("owned-file-terminal-paths", CleanupFileTerminal);
                    ownershipUnresolved = owned.Any(task => !task.IsCompleted);
                }
                if (!ownershipUnresolved && runtime is not null) runtime.Input.TargetFailed -= OnTargetFailed;
                if (!ownershipUnresolved && vm is not null) { runtimeDisposeAttempted = true; runtimeDisposeCompleted = await Cleanup("single-owner-vm-shutdown", () => vm.ShutdownAsync()); }
                else if (!ownershipUnresolved && runtime is not null) { runtimeDisposeAttempted = true; runtimeDisposeCompleted = await Cleanup("single-owner-runtime", () => runtime.DisposeAsync().AsTask()); }
                if (filesTerminal && owned.Any(task => !task.IsCompleted)) ownershipUnresolved = true;
                if (!filesTerminal && ownershipUnresolved) cleanupErrors.Add(new { name = "runtime-creation-still-pending", error = "Late runtime ownership unresolved at owner selection; upstream resources stay retained for this cleanup." });
                using var legacyCleanupDeadline = filesTerminal ? null : new CancellationTokenSource(TimeSpan.FromSeconds(20));
                var cleanupDeadline = legacyCleanupDeadline ?? new CancellationTokenSource(TimeSpan.FromSeconds(20));
                if (service is not null) await Cleanup("final-host-audit", () => HostIdentity("final-after-cleanup", cleanupDeadline.Token));
                if (filesTerminal && owned.Any(task => !task.IsCompleted)) ownershipUnresolved = true;
                if (!ownershipUnresolved && service is not null) await Cleanup("owned-ld-service", () => service.DisposeAsync().AsTask());
                if (filesTerminal && owned.Any(task => !task.IsCompleted)) ownershipUnresolved = true;
                if (!ownershipUnresolved && seed is not null) await Cleanup("owned-host-only-seed", () => seed.DisposeAsync().AsTask());
                if (filesTerminal)
                {
                    if (owned.Any(task => !task.IsCompleted)) ownershipUnresolved = true;
                    else { cleanupDeadline.Dispose(); ioCleanupAuditTokenDisposed = true; }
                    if (ownershipUnresolved) cleanupErrors.Add(new { name = "file-terminal-ownership-unresolved", error = "Runtime, original operation or cleanup ownership unresolved; upstream resources and any live audit token stay retained." });
                }
                var pendingOwned = owned.Count(task => !task.IsCompleted);
                if (pendingOwned > 0) cleanupErrors.Add(new { name = "owned-tasks-still-pending", error = pendingOwned + " owned tasks are not terminal." });
                var deadlineDisposed = !ownershipUnresolved && pendingOwned == 0;
                if (deadlineDisposed) deadline.Dispose();
                if (reportErrors.Count > 0) failure = new AggregateException(new[] { failure }.OfType<Exception>().Concat(reportErrors));
                var summary = new { status = failure is null && cleanupErrors.Count == 0 ? "PASS-LIVE-VM-SDK-NOT-GUI" : "FAIL", manifestSha, steps, started, completed,
                    notRun = planned.Where(name => !started.Contains(name)).ToArray(), error = failure?.ToString(), cleanupErrors, launches,
                    owned = owned.Count, pendingOwned, runtimeDisposeAttempted, runtimeDisposeCompleted, creationStatusAtOwnership, ownershipUnresolved, currentCreationPending = runtimeCreation is { IsCompleted: false }, deadlineDisposed,
                    observedOperationErrors = observedOperationErrors.Select(error => error.ToString()).ToArray(), reportErrors = reportErrors.Select(error => error.ToString()).ToArray(),
                    windows = Application.Current.Windows.Count, mainWindowNull = Application.Current.MainWindow is null,
                    targetErrors = targetErrors.Select(error => error.ToString()).ToArray(), noSimulatorStopIssued = true,
                    physicalGui = "NOT-RUN-AP045-STOPPED", settingsBackStacksRestored = false,
                    fileTerminal = filesTerminal ? FileTerminalSummary() : null };
                try { await Save("summary.json", summary); }
                catch (Exception error) { failure = failure is null ? error : new AggregateException(failure, error); }
            }
            if (failure is not null || cleanupErrors.Count > 0) throw new AggregateException(filesTerminal ? "真实终端与文件验收失败；未关闭模拟器。" : "真实双实例同步失败；未关闭模拟器。", new[] { failure }.OfType<Exception>().Concat(cleanupErrors.Select(error => new Exception(JsonSerializer.Serialize(error, JsonDefaults.Options)))));
        }
        private void OnTargetFailed(string serial, Exception error) => targetErrors.Enqueue(new InvalidOperationException(serial, error));
        private async Task Epoch(string stage, CancellationToken ct)
        {
            await LdPlayerAdbBinding.VerifyAsync("127.0.0.1:5037", manifest.Server.Pid, manifest.Server.StartedUtcTicks, ct);
            var process = DualSyncProcess(manifest.Server.Pid, manifest.Server.StartedUtcTicks);
            Need(process.Path.Equals(manifest.Server.Path, StringComparison.OrdinalIgnoreCase) && DualSyncHash(process.Path).Equals(manifest.Server.Sha256, StringComparison.OrdinalIgnoreCase), "Existing 5037 executable identity changed.");
            Need(DualSyncHash(manifest.ConsolePath).Equals(manifest.ConsoleSha256, StringComparison.OrdinalIgnoreCase), "Console executable changed.");
            await Trace("5037-owner-probe", new { stage, manifest.Server.Pid, manifest.Server.StartedUtcTicks, process.Path, ownerProof = "Exact TCP connection owner via public host-only LdPlayerAdbBinding; no ADB bytes or guest request." });
        }
        private async Task<IReadOnlyList<LdPlayerInstance>> HostIdentity(string stage, CancellationToken ct)
        {
            await Epoch(stage + "-before", ct); var rows = await service!.RefreshAsync(ct);
            Need(rows.Count == manifest.Directory.Length, "Directory membership changed.");
            foreach (var original in manifest.Directory.Where(row => row.Index is not (1 or 2)))
                Need(rows.Any(row => (row with { Generation = 1 }) == (original with { Generation = 1 })), "Non-target directory row changed: " + original.Index);
            foreach (var target in manifest.Targets)
            {
                var row = rows.SingleOrDefault(item => item.Index == target.Index) ?? throw new DualSyncFailure("PRECONDITION-FAIL", "Dedicated instance missing.");
                var original = manifest.Directory.Single(item => item.Index == target.Index);
                Need(row.Name == target.Name && row.Width == original.Width && row.Height == original.Height && row.Dpi == original.Dpi, "Dedicated identity/dimensions changed.");
                if (ready.TryGetValue(target.Index, out var frozen))
                {
                    Need(row.AndroidStarted && !row.IsStarting && row.ProcessId == frozen.Row.ProcessId && row.VBoxProcessId == frozen.Row.VBoxProcessId, "Dedicated process changed after freeze.");
                    DualSyncProcess(row.ProcessId, frozen.PlayerBirth); DualSyncProcess(row.VBoxProcessId, frozen.VBoxBirth);
                    await LdPlayerAdbBinding.VerifyAsync(target.Serial, row.VBoxProcessId, frozen.VBoxBirth, ct);
                }
                else if (!target.AllowLaunch)
                {
                    Need((row with { Generation = 1 }) == (original with { Generation = 1 }), "Approved Ready instance changed; never launch it: " + target.Index);
                    DualSyncProcess(row.ProcessId, target.PlayerStartedUtcTicks); DualSyncProcess(row.VBoxProcessId, target.VBoxStartedUtcTicks);
                    await LdPlayerAdbBinding.VerifyAsync(target.Serial, row.VBoxProcessId, target.VBoxStartedUtcTicks, ct);
                }
            }
            await Epoch(stage + "-after", ct); await Trace("host-identity", new { stage, rows, otherCount = rows.Count(row => row.Index is not (1 or 2)) }); return rows;
        }
        private async Task Bootstrap()
        {
            await Epoch("before-service", Ct); service = new LdPlayerService(new DualSyncLdRunner(this, new LdConsoleRunner(manifest.ConsolePath, 15)));
            var rows = await Step("initial-directory", () => HostIdentity("initial", Ct));
            Need(rows.All(row => manifest.Directory.Any(original => (original with { Generation = 1 }) == (row with { Generation = 1 }))), "Initial full directory differs from explicit manifest.");
            connectionSettings = new AppSettings { LdConsolePath = manifest.ConsolePath, AdbAllowedSerials = [], LdSerialBindings = [], AutoConnect = false, AutoStartApi = false,
                SyncMouse = false, SyncKeyboard = false, AgentBaseUrl = null, AgentSerial = null, CommandTimeoutSeconds = 15, Language = "zh-CN" };
            await Epoch("before-prelaunch-host-only-seed", Ct); seed = new AdbDeviceBackend(connectionSettings);
            var initialRegistered = await AfterGuard(() => Step("initial-ready-host-registration", () => seed.ListRegisteredDevicesAsync(Ct)), ct => Epoch("after-initial-ready-host-registration", ct));
            foreach (var target in manifest.Targets.Where(target => !target.AllowLaunch))
                Need(initialRegistered.Count(row => row.Serial == target.Serial) == 1 && initialRegistered.Any(row => row.Serial == target.Serial && row.State == "device" && row.TransportId == target.TransportId),
                    "Approved Ready serial/state/transport must match before any guest initialization or launch: " + target.Index);
            if (Target(2).AllowLaunch)
            {
                var expected = rows.Single(row => row.Index == 2);
                var launch = new LdLaunchCoordinator(service, TimeSpan.FromMinutes(3), TimeSpan.FromSeconds(2));
                await Step("launch-only-instance2", async () => await launch.StartAsync([2], async (row, ct) =>
                {
                    await Epoch("resource-before", ct); var config = LdLaunchMemory.ReadConfiguration(manifest.ConsolePath, row.Index);
                    var memory = new MemoryStatus { Length = (uint)Marshal.SizeOf<MemoryStatus>() };
                    var host = GlobalMemoryStatusEx(ref memory) ? new LdHostMemory(memory.AvailablePhysical, memory.AvailablePageFile, memory.MemoryLoad) : null;
                    var assessment = LdLaunchMemory.Assess(config, host); await Trace("resource-assessment", new { row.Index, config, host, assessment });
                    Need(!assessment.RequiresConfirmation, "Explicit memory shortage: " + assessment.Message); await Epoch("resource-after", ct);
                }, new DualSyncProgress(value =>
                {
                    try { File.AppendAllText(Path.Combine(output, "launch-progress.jsonl"), JsonSerializer.Serialize(value, new JsonSerializerOptions(JsonDefaults.Options) { WriteIndented = false }) + "\n"); }
                    catch (Exception error) { reportErrors.Add(error); deadline.Cancel(); }
                }), Ct, [expected]));
            }
            else await Trace("reuse-only-ready-instances", new { indices = new[] { 1, 2 }, launches, relaunchForbidden = true });
            rows = await HostIdentity("after-launch-or-ready-reuse", Ct);
            foreach (var target in manifest.Targets)
            {
                var row = rows.Single(item => item.Index == target.Index); Need(row.AndroidStarted && !row.IsStarting && row.ProcessId > 0 && row.VBoxProcessId > 0, "Both dedicated instances must be running.");
                ready[target.Index] = (row, DualSyncProcess(row.ProcessId, target.AllowLaunch ? null : target.PlayerStartedUtcTicks).Birth,
                    DualSyncProcess(row.VBoxProcessId, target.AllowLaunch ? null : target.VBoxStartedUtcTicks).Birth);
            }
        }
        private async Task CreateRuntime()
        {
            var settings = connectionSettings ?? throw new InvalidOperationException("Prelaunch host-only seed settings missing.");
            Need(seed is not null, "The prelaunch host-only seed must be reused.");
            var watch = Stopwatch.StartNew(); IReadOnlyList<AdbRegisteredDevice> registered;
            do
            {
                registered = await Step("host-only-registration", () => seed!.ListRegisteredDevicesAsync(Ct)); await Epoch("registration", Ct);
                if (manifest.Targets.All(target => registered.Any(row => row.Serial == target.Serial && row.State == "device" && row.TransportId is > 0))) break;
                Need(watch.Elapsed < TimeSpan.FromSeconds(90), "Dedicated registration did not become ready; no connect/restart/relaunch."); await Task.Delay(1000, Ct);
            } while (true);
            foreach (var target in manifest.Targets.Where(target => !target.AllowLaunch))
                Need(registered.Count(row => row.Serial == target.Serial) == 1 && registered.Any(row => row.Serial == target.Serial && row.State == "device" && row.TransportId == target.TransportId), "Ready transport changed from explicit manifest: " + target.Index);
            var snapshots = (await HostIdentity("before-admission", Ct)).Where(row => row.Index is 1 or 2).ToArray();
            var admissions = await Step("actual-tcp-admission", () => LdConnectionAdmission.ResolveAsync(seed!, service!, [1, 2], snapshots, ready.ToDictionary(pair => pair.Key, pair => pair.Value.VBoxBirth), Ct));
            Need(admissions.Count == 2 && admissions.All(proof => proof.Name == Target(proof.Index).Name && proof.Serial == Target(proof.Index).Serial), "Resolved serials differ from approved names/serials.");
            Need(admissions.All(proof => Target(proof.Index).AllowLaunch || proof.TransportId == Target(proof.Index).TransportId), "Approved Ready admission transport changed before guest initialization.");
            settings = LdConnectionAdmission.Apply(settings, admissions); await HostIdentity("before-runtime", Ct);
            await Step("create-inherited-epoch-runtime", async () =>
            {
                runtimeCreation = MatrixRuntime.CreateAsync(settings, Path.Combine(output, "runtime"), admissions: admissions, epochSource: seed, ct: Ct);
                runtime = await runtimeCreation;
                return new { runtime.IsDemo, runtime.DiscoveryError, runtime.LdPlayerError, devices = runtime.Devices.Snapshot };
            });
            await HostIdentity("after-runtime", Ct); Need(!runtime.IsDemo && runtime.Backend is AdbDeviceBackend && runtime.DiscoveryError is null && runtime.LdPlayerError is null && runtime.Devices.Snapshot.Count == 2, "Exact real two-target runtime required.");
            foreach (var device in runtime.Devices.Snapshot) { Need(device.Online && admissions.Any(proof => proof.Serial == device.Serial && proof.TransportId == device.TransportId), "Runtime Ready identity mismatch."); devices.Add(device.Serial, device); }
            vm = new WorkspaceViewModel(runtime, Path.Combine(output, "runtime"), false); await vm.RefreshDevicesAsync(); Need(vm.Error.Length == 0 && vm.Devices.Count == 2, "Real VM discovery failed.");
            runtime.Input.TargetFailed += OnTargetFailed; await vm.SetMainAsync(Row(1)); vm.CurrentDevice = Row(2); SelectB(true);
            Need(vm.Error.Length == 0 && ReferenceEquals(vm.MainDevice, Row(1)) && ReferenceEquals(vm.CurrentDevice, Row(2)), "Main1/Current2 setup failed.");
            await Save("frozen-identities.json", new { ready = ready.Select(pair => new { index = pair.Key, pair.Value.Row, pair.Value.PlayerBirth, pair.Value.VBoxBirth }), devices = devices.Values, admissions });
            await Identity("runtime-ready", Ct);
        }
        private void SelectB(bool selected) { foreach (var row in vm!.Devices) row.IsSelected = selected && row.Serial == Target(2).Serial; vm.UpdateSelection(); }
        private async Task Identity(string stage, CancellationToken ct, bool requireCleanOperation = true)
        {
            await HostIdentity(stage + "-host-before", ct); runtime!.RequireFrozenConnectionSettings();
            Need(!runtime.Api.BasicRunning && !runtime.Api.AutomatorRunning && runtime.Api.DevicePorts.Count == 0 && !runtime.Settings.AutoStartApi && !runtime.Settings.AutoConnect && runtime.Settings.AgentBaseUrl is null, "Background services must stay disabled.");
            var backend = (AdbDeviceBackend)runtime.Backend; var registered = await backend.ListRegisteredDevicesAsync(ct);
            foreach (var frozen in devices.Values)
            {
                await backend.VerifyIdentityAsync(frozen.Serial, ct);
                Need(registered.Any(row => row.Serial == frozen.Serial && row.State == "device" && row.TransportId == frozen.TransportId)
                    && runtime.Devices.Snapshot.Any(row => row.Serial == frozen.Serial && row.Online && row.Generation == frozen.Generation && row.TransportId == frozen.TransportId), "Frozen run transport/generation changed.");
            }
            Need(!requireCleanOperation || vm is null || vm.Error.Length == 0, "VM reported operation failure: " + vm?.Error, "PRODUCT-FAIL");
            Need(targetErrors.IsEmpty, "Input target failure: " + string.Join("\n", targetErrors.Select(error => error.ToString())), "PRODUCT-FAIL");
            await HostIdentity(stage + "-host-after", ct);
        }
        private async Task<string> Shell(int index, string name, string command)
        {
            await Identity(name + "-before", Ct);
            return await AfterGuard(() => Step(name, () => runtime!.Backend.ShellAsync(Target(index).Serial, command, ct: Ct)), ct => Identity(name + "-after", ct));
        }
        private async Task<DualSyncFrame> Read(int index, string name)
        {
            await Identity(name + "-before", Ct);
            return await AfterGuard(() => Step(name, async () =>
            {
                var serial = Target(index).Serial; var activity = await runtime!.Backend.ShellAsync(serial, "dumpsys activity activities", ct: Ct);
                var packages = Regex.Matches(activity, @"(?:topResumedActivity|mResumedActivity).*?\b([A-Za-z][A-Za-z0-9_.]*)/").Select(match => match.Groups[1].Value).Distinct(StringComparer.Ordinal).ToArray();
                Need(packages.Length == 1, "Ambiguous actual foreground package.");
                var root = await runtime.Backend.DumpAsync(serial, Ct); var png = await runtime.Backend.ScreenshotAsync(serial, Ct); var size = PngImage.ReadDimensions(png);
                Need(size.Width == devices[serial].Width && size.Height == devices[serial].Height, "Actual PNG differs from frozen device dimensions.");
                var file = name + ".png"; await File.WriteAllBytesAsync(Path.Combine(output, file), png, Ct);
                await Trace("foreground-raw", new { name, serial, activity });
                return new DualSyncFrame(serial, packages[0], root, size.Width, size.Height, file, Convert.ToHexString(SHA256.HashData(png)));
            }), ct => Identity(name + "-after", ct));
        }
        private async Task<(DualSyncFrame A, DualSyncFrame B)> Prepare(string name, bool forSwipe)
        {
            foreach (var index in new[] { 1, 2 }) await Shell(index, name + "-settings-" + index, "am start -W -a android.settings.SETTINGS -f 0x10008000");
            var a = await Read(1, name + "-before-a"); var b = await Read(2, name + "-before-b");
            Need(a.Foreground == "com.android.settings" && b.Foreground == "com.android.settings", "Fixed Settings UI-task setup did not reach Settings on both guests.");
            if (forSwipe) { a = await PrepareSwipeHeader(1, name, a); b = await PrepareSwipeHeader(2, name, b); }
            return (a, b);
        }
        private async Task<DualSyncFrame> PrepareSwipeHeader(int index, string name, DualSyncFrame before)
        {
            var candidate = ScrollContainer(before);
            if (candidate is not null && Anchors(candidate).Split('\n', StringSplitOptions.RemoveEmptyEntries).Length >= 4)
            {
                await Trace("header-setup-skipped", new { setupOnly = true, index, reason = "Original four-anchor precondition already satisfied.", before.Png, before.PngSha256 });
                return before;
            }
            var nodes = NodeSearch.FindAll(before.Root, new()).ToArray();
            AndroidNode Exact(string id, string className)
            {
                var matches = nodes.Where(node => node.ResourceId == "com.android.settings:id/" + id).ToArray();
                Need(matches.Length == 1, "Unique Settings header setup role missing: " + id);
                var node = matches[0];
                Need(node.PackageName == "com.android.settings" && node.ClassName == className && node.Enabled && node.VisibleToUser
                    && !node.Clickable && !node.Checkable && node.Bounds.Width > 0 && node.Bounds.Height > 0, "Unsafe Settings header setup role: " + id);
                return node;
            }
            var outer = Exact("settings_homepage_container", "android.widget.ScrollView");
            var inner = Exact("main_content_scrollable_container", "android.widget.ScrollView");
            var caption = Exact("homepage_title", "android.widget.TextView");
            var descendants = NodeSearch.FindAll(outer, new()).ToArray();
            Need(before.Foreground == "com.android.settings" && outer.Scrollable && inner.Scrollable && descendants.Any(node => ReferenceEquals(node, inner))
                && descendants.Any(node => ReferenceEquals(node, caption)) && !NodeSearch.FindAll(inner, new()).Any(node => ReferenceEquals(node, caption)), "Settings header/list hierarchy is not the approved setup scene.");
            Need(outer.Bounds.Left >= 0 && outer.Bounds.Top >= 0 && outer.Bounds.Right <= before.Width && outer.Bounds.Bottom <= before.Height
                && Inside(outer.Bounds, inner.Bounds.Left, inner.Bounds.Top) && Inside(outer.Bounds, inner.Bounds.Right - 1, inner.Bounds.Bottom - 1)
                && Inside(outer.Bounds, caption.Bounds.Left, caption.Bounds.Top) && Inside(outer.Bounds, caption.Bounds.Right - 1, caption.Bounds.Bottom - 1)
                && caption.Bounds.Bottom <= inner.Bounds.Top, "Settings header setup geometry is not safely contained.");
            var x = outer.Bounds.CenterX; var y = caption.Bounds.CenterY; var endY = outer.Bounds.Top + Math.Max(1, caption.Bounds.Height / 8);
            Need(Inside(outer.Bounds, x, y) && Inside(outer.Bounds, x, endY) && y < inner.Bounds.Top && endY < inner.Bounds.Top
                && y - endY >= 64 && y - endY <= outer.Bounds.Height / 2, "Header setup travel is outside the bounded anti-tap safety margin.");
            var intersections = nodes.Where(node => node.VisibleToUser && (node.Clickable || node.Checkable) && node.Bounds.Width > 0 && node.Bounds.Height > 0
                && x >= node.Bounds.Left && x < node.Bounds.Right && node.Bounds.Top <= y && node.Bounds.Bottom > endY).ToArray();
            Need(intersections.Length == 0, "Header setup line intersects a visible clickable/checkable node; no setup gesture sent.");
            object Role(AndroidNode node) => new { node.ResourceId, node.ClassName, node.PackageName, node.Enabled, node.VisibleToUser, node.Clickable, node.Checkable, node.Scrollable, node.Bounds };
            var plan = new { setupOnly = true, index, serial = Target(index).Serial, x, y, endY, durationMs = 300, minimumAntiTapTravelPixels = 64,
                before.Png, before.PngSha256, outer = Role(outer), inner = Role(inner), caption = Role(caption), interactiveLineIntersections = intersections.Length };
            await Identity(name + "-header-setup-before-" + index, Ct);
            await AfterGuard(() => Step(name + "-header-setup-once-" + index, async () =>
            {
                await Trace("setup-only-header-plan", plan);
                await runtime!.Backend.SwipeAsync(Target(index).Serial, x, y, x, endY, 300, Ct);
                return plan;
            }), ct => Identity(name + "-header-setup-after-" + index, ct));
            var prepared = await Read(index, name + "-header-baseline-" + index);
            var preparedNodes = Nodes(prepared);
            Need(prepared.Foreground == "com.android.settings" && preparedNodes.Count(node => node.ResourceId == outer.ResourceId && node.ClassName == outer.ClassName && node.PackageName == outer.PackageName) == 1
                && preparedNodes.Count(node => node.ResourceId == inner.ResourceId && node.ClassName == inner.ClassName && node.PackageName == inner.PackageName) == 1, "Header setup did not preserve the verified Settings homepage.");
            Scroll(prepared);
            return prepared;
        }
        private async Task Exercise(string name, string action, bool sync)
        {
            var before = await Prepare(name, action == "swipe"); var a = before.A; var b = before.B;
            var targets = vm!.InputTargets(a.Serial, action != "home"); Need(targets.SequenceEqual(sync ? new[] { b.Serial } : []), "Original InputTargets does not match the four-state selection contract.", "PRODUCT-FAIL");
            var bStable = Stable(b); var scrollA = action == "swipe" ? Scroll(a) : null; var scrollB = action == "swipe" ? Scroll(b) : null;
            Need(TitleAnchors(b).Length >= 3 && !SearchEditor(a) && !SearchEditor(b), "Fresh Settings root with stable title anchors required before any input.");
            InputSnapshot? snapshot = null; string? launcherA = null, launcherB = null;
            if (action == "home")
            {
                launcherA = ResolveHome(await Shell(1, name + "-resolve-a", "cmd package resolve-activity --brief -a android.intent.action.MAIN -c android.intent.category.HOME"));
                launcherB = ResolveHome(await Shell(2, name + "-resolve-b", "cmd package resolve-activity --brief -a android.intent.action.MAIN -c android.intent.category.HOME"));
                Need(launcherA != a.Foreground && launcherB != b.Foreground, "Resolved Home launcher must differ from prepared Settings foreground.");
            }
            else
            {
                var bounds = action == "tap" ? Search(a).Bounds : scrollA!.Bounds;
                var x = bounds.CenterX; var y = action == "tap" ? bounds.CenterY : bounds.Top + bounds.Height * 3 / 4;
                var y2 = action == "tap" ? y : bounds.Top + bounds.Height / 4;
                var mapped = InputCoordinator.ScalePoint(x, y, a.Width, a.Height, b.Width, b.Height);
                var mappedEnd = InputCoordinator.ScalePoint(x, y2, a.Width, a.Height, b.Width, b.Height);
                var safe = action == "tap" ? Search(b).Bounds : scrollB!.Bounds;
                Need(Inside(safe, mapped.X, mapped.Y) && Inside(safe, mappedEnd.X, mappedEnd.Y), "Mapped input is outside the verified safe B Settings control; no gesture sent.");
                snapshot = new() { SourceSerial = a.Serial, SourceGeneration = devices[a.Serial].Generation, Generation = runtime!.Input.CurrentGeneration,
                    SourceWidth = a.Width, SourceHeight = a.Height, TargetSerials = targets.ToArray(), Kind = action == "tap" ? InputKind.Tap : InputKind.Swipe,
                    X = x, Y = y, X2 = x, Y2 = y2, DurationMs = 300 };
            }
            await Identity(name + "-input-before", Ct);
            await AfterGuard(() => Step(name + "-input-once", async () =>
            {
                Need(ReferenceEquals(vm.MainDevice, Row(1)) && ReferenceEquals(vm.CurrentDevice, Row(2)) && !Row(1).IsSelected, "Main/Current/selection source drift.");
                if (action == "home") await vm.KeyAsync(3); else await vm.SendInputAsync(snapshot!);
                Need(vm.Error.Length == 0 && targetErrors.IsEmpty, "Original VM input failed: " + vm.Error, "PRODUCT-FAIL");
                return new { action, sync, snapshot, key = action == "home" ? 3 : (int?)null, vm.SyncMouse, vm.SyncKeyboard, main = vm.MainDevice?.Serial, current = vm.CurrentDevice?.Serial, selected = vm.Devices.Where(row => row.IsSelected).Select(row => row.Serial).ToArray() };
            }), ct => Identity(name + "-input-after", ct));
            var afterA = await Read(1, name + "-after-a"); var afterB = await Read(2, name + "-after-b");
            bool Effect(DualSyncFrame old, DualSyncFrame current, AndroidNode? scroll, string? launcher) => action switch
            {
                "tap" => (current.Foreground is "com.android.settings" or "com.android.settings.intelligence") && SearchEditor(current) && !SearchEditor(old),
                "swipe" => current.Foreground == "com.android.settings" && ScrollContainer(current) is { } afterScroll && Anchors(afterScroll).Length > 0 && Anchors(scroll!) != Anchors(afterScroll),
                _ => current.Foreground == launcher && current.Foreground != old.Foreground
            };
            Need(Effect(a, afterA, scrollA, launcherA), "Source A has no objective " + action + " effect.", "PRODUCT-FAIL");
            Need(sync ? Effect(b, afterB, scrollB, launcherB) : Stable(afterB) == bStable && afterB.Foreground == b.Foreground,
                "Target B did not match expected independent/synchronized " + action + " effect.", "PRODUCT-FAIL");
            await Save(name + "-effect.json", new { action, sync, sourceChanged = true, targetChanged = sync, source = a.Serial, target = b.Serial,
                beforeA = Stable(a), afterA = Stable(afterA), beforeB = bStable, afterB = Stable(afterB), main = vm.MainDevice?.Serial, current = vm.CurrentDevice?.Serial });
        }
        private static AndroidNode[] Nodes(DualSyncFrame frame) => NodeSearch.FindAll(frame.Root, new()).Where(node => node.Enabled && node.VisibleToUser && node.Bounds.Width > 0 && node.Bounds.Height > 0).ToArray();
        private static AndroidNode Search(DualSyncFrame frame) => Nodes(frame).FirstOrDefault(node => node.PackageName == "com.android.settings" && !node.Checkable &&
            ((node.Text ?? "").Contains("搜索", StringComparison.Ordinal) || (node.ContentDesc ?? "").Contains("搜索", StringComparison.Ordinal) || (node.Text ?? "").Contains("Search", StringComparison.OrdinalIgnoreCase) || (node.ContentDesc ?? "").Contains("Search", StringComparison.OrdinalIgnoreCase)))
            ?? throw new DualSyncFailure("PRECONDITION-FAIL", "No verified safe Settings search entry.");
        private static bool SearchEditor(DualSyncFrame frame) => Nodes(frame).Any(node => node.ResourceId == "android:id/search_src_text" && node.Focused && (node.ClassName is "android.widget.EditText" or "android.widget.AutoCompleteTextView") && (node.PackageName is "com.android.settings" or "com.android.settings.intelligence"));
        private static AndroidNode Scroll(DualSyncFrame frame)
        {
            var node = ScrollContainer(frame)
                ?? throw new DualSyncFailure("PRECONDITION-FAIL", "No verified safe Settings scroll container.");
            Need(Anchors(node).Split('\n', StringSplitOptions.RemoveEmptyEntries).Length >= 4, "Not enough concrete list anchors; no swipe sent."); return node;
        }
        private static AndroidNode? ScrollContainer(DualSyncFrame frame) => Nodes(frame).FirstOrDefault(node => node.PackageName == "com.android.settings" && node.Scrollable && node.Bounds.Width >= 100 && node.Bounds.Height >= 150);
        private static bool IsTitle(AndroidNode node) => node.ResourceId == "android:id/title" || node.ResourceId?.EndsWith("/title", StringComparison.Ordinal) == true;
        private static string[] TitleAnchors(DualSyncFrame frame) => Nodes(frame).Where(node => IsTitle(node) && !string.IsNullOrWhiteSpace(node.Text)).Select(node => node.ResourceId + "|" + node.Text + "|" + node.Bounds).Order(StringComparer.Ordinal).ToArray();
        private static string Anchors(AndroidNode node) => string.Join("\n", NodeSearch.FindAll(node, new()).Where(item => item.VisibleToUser && IsTitle(item) && !string.IsNullOrWhiteSpace(item.Text)
            && item.Bounds.Width > 0 && item.Bounds.Height > 0 && item.Bounds.Top >= node.Bounds.Top && item.Bounds.Bottom <= node.Bounds.Bottom)
            .Select(item => item.ResourceId + "|" + item.Text + "|" + item.Bounds).Order(StringComparer.Ordinal));
        private static string Stable(DualSyncFrame frame) => frame.Foreground + "\n" + SearchEditor(frame) + "\n" + string.Join("\n", TitleAnchors(frame));
        private static bool Inside(NodeBounds bounds, int x, int y) => x >= bounds.Left && x < bounds.Right && y >= bounds.Top && y < bounds.Bottom;
        private static string ResolveHome(string value) => Regex.Matches(value, @"(?m)^\s*([A-Za-z][A-Za-z0-9_.]*)/[A-Za-z0-9_.$]+\s*$").Select(match => match.Groups[1].Value).Distinct(StringComparer.Ordinal).SingleOrDefault()
            ?? throw new DualSyncFailure("PRECONDITION-FAIL", "Home launcher did not resolve unambiguously.");
        private sealed class DualSyncLdRunner(DualSyncRun owner, ILdConsoleRunner inner) : ILdConsoleRunner
        {
            public async Task<string> RunAsync(IReadOnlyList<string> arguments, CancellationToken ct = default)
            {
                var launch = arguments.SequenceEqual(new[] { "launch", "--index", "2" });
                Need(arguments.SequenceEqual(new[] { "list2" }) || launch, "LD command outside list2/launch2 rejected.");
                Need(!launch || owner.Target(2).AllowLaunch, "Ready continuation forbids every launch command.");
                await owner.Epoch("ld-before", ct); if (launch) Need(Interlocked.Increment(ref owner.launches) == 1, "A second launch attempt is forbidden.");
                return await owner.AfterGuard(async () => { var value = await inner.RunAsync(arguments, ct); await owner.Trace("ld-return", new { arguments, value }); return value; }, after => owner.Epoch("ld-after", after));
            }
        }
        private sealed class DualSyncProgress(Action<LdLaunchProgress> callback) : IProgress<LdLaunchProgress>
        { public void Report(LdLaunchProgress value) => callback(value); }
    }

    private static string DualSyncAbsolute(string value) => Path.IsPathFullyQualified(value) ? Path.GetFullPath(value) : throw new ArgumentException("Absolute path required: " + value);
    private static bool DualSyncSha(string value) => value is not null && Regex.IsMatch(value, "^[0-9a-fA-F]{64}$");
    private static string DualSyncHash(string path) { using var file = File.OpenRead(path); return Convert.ToHexString(SHA256.HashData(file)); }
    private static (string Path, long Birth) DualSyncProcess(int pid, long? expected = null)
    {
        using var process = Process.GetProcessById(pid); var birth = process.StartTime.ToUniversalTime().Ticks;
        if (process.HasExited || expected is { } value && value != birth) throw new DualSyncFailure("PRECONDITION-FAIL", "PID exited/reused: " + pid);
        return (process.MainModule?.FileName ?? throw new InvalidOperationException("Process executable path unavailable."), birth);
    }
    private static void DualSyncUniqueKeys(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var property in element.EnumerateObject()) { if (!keys.Add(property.Name)) throw new ArgumentException("Duplicate manifest JSON key: " + property.Name); DualSyncUniqueKeys(property.Value); }
        }
        else if (element.ValueKind == JsonValueKind.Array) foreach (var item in element.EnumerateArray()) DualSyncUniqueKeys(item);
    }
}
