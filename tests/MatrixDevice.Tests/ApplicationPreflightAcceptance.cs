using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using MatrixDevice.Core;
using MatrixDevice.Features;
using MatrixDevice.Host;

internal static class ApplicationPreflightAcceptance
{
    private const string Package = "com.github.metacubex.clash.meta";
    private const string ApkSha256 = "AF7F05D8801798A8E75DE682E5F67070138BECB8623B7A341C3E5CD042A3753D";
    private const long ApkBytes = 48_780_036;
    private const int VersionCode = 211035;
    private const string VersionName = "2.11.35.Meta";
    private const int CommandSeconds = 10;
    private const int MaximumReadBytes = 256 * 1024;
    private const string OwnedSourceSha256 = "0D815ED2C25D0E2388B224EE1A49FB704D473E1982ACB9FACFB9B5B500B19AE6";

    public static Task RunAsync(string[] args) => RunCoreAsync(args, lifecycle: false);
    public static Task RunLifecycleAsync(string[] args) => RunCoreAsync(args, lifecycle: true);
    public static Task RunOwnedCleanupAsync(string[] args)
    {
        if (args.Length != 5) throw new ArgumentException("--application-owned-cleanup <absolute-target.json> <absolute-apk> <absolute-source-ledger> <expected-ledger-sha256> <fresh-absolute-evidence-directory>");
        return RunCoreAsync([args[0], args[1], args[4]], lifecycle: false, sourceLedgerPath: args[2], expectedLedgerSha: args[3]);
    }

    private static async Task RunCoreAsync(string[] args, bool lifecycle, string? sourceLedgerPath = null, string? expectedLedgerSha = null)
    {
        var ownedCleanup = sourceLedgerPath is not null;
        var mutableMode = lifecycle || ownedCleanup;
        if (args.Length != 3) throw new ArgumentException((lifecycle ? "--application-lifecycle" : "--application-preflight") + " <absolute-target.json> <absolute-apk> <fresh-absolute-evidence-directory>");
        var targetPath = Absolute(args[0]);
        var apkPath = Absolute(args[1]);
        var output = Absolute(args[2]);
        var ledgerInput = sourceLedgerPath is null ? null : Absolute(sourceLedgerPath);
        Require(!Directory.Exists(output) && !File.Exists(output), "Evidence directory must be fresh: " + output);
        Require(!Within(targetPath, output) && !Within(apkPath, output), "Evidence must not contain either input file.");
        Require(ledgerInput is null || !Within(ledgerInput, output), "Evidence must not contain the source ledger.");
        Directory.CreateDirectory(output);
        var options = new JsonSerializerOptions(JsonDefaults.Options) { WriteIndented = false };
        using var deadline = new CancellationTokenSource(TimeSpan.FromMinutes(mutableMode ? 10 : 3));
        var ct = deadline.Token;
        var clock = Stopwatch.StartNew();
        var steps = new List<object>();
        var sequence = 0;
        Target? target = null;
        MatrixRuntime? runtime = null;
        AdbDeviceBackend? seed = null;
        LdPlayerService? admissionService = null;
        DeviceInfo? frozenDevice = null;
        long? frozenLdGeneration = null;
        object? result = null;
        Exception? failure = null;
        var cleanupErrors = new List<string>();
        var builds = new Dictionary<string, string>();
        FileStream? apkLease = null;
        FileStream? sourceLedgerLease = null;
        SourceLedger? sourceLedger = null;
        var sourceLedgerValidated = false;
        var runId = Guid.NewGuid().ToString("N");
        var installationAuthorized = false;
        var writeAttempted = false;
        var installAttempted = false;
        var owned = false;
        var ledgerState = "NOT-ATTEMPTED";
        OwnedPackageProof? ownedPackage = null;
        object? lifecycleResult = null;
        object? ownedCleanupResult = null;

        async Task Save(string name, object value) => await File.WriteAllTextAsync(Path.Combine(output, name), JsonSerializer.Serialize(value, JsonDefaults.Options));
        async Task Trace(string file, object value) => await File.AppendAllTextAsync(Path.Combine(output, file), JsonSerializer.Serialize(value, options) + "\n");
        async Task<T> Step<T>(string id, Func<Task<T>> operation)
        {
            var started = DateTimeOffset.UtcNow;
            var watch = Stopwatch.StartNew();
            var evidence = $"{++sequence:D3}-{id}.json";
            var passStatus = mutableMode ? "PASS-SHARED-BACKEND-NOT-GUI" : "PASS-READ-ONLY";
            try
            {
                ct.ThrowIfCancellationRequested();
                var value = await operation();
                await Save(evidence, value!);
                var entry = new { id, started, durationMs = watch.ElapsedMilliseconds, status = passStatus, evidence, evidenceSha256 = await HashFile(Path.Combine(output, evidence), ct) };
                steps.Add(entry); await Trace("results.jsonl", entry);
                Console.WriteLine($"{passStatus} {id} {watch.ElapsedMilliseconds}ms");
                return value;
            }
            catch (Exception ex)
            {
                var entry = new { id, started, durationMs = watch.ElapsedMilliseconds, status = "FAIL", error = ex.ToString() };
                steps.Add(entry); await Trace("results.jsonl", entry);
                throw;
            }
        }

        async Task<object> HostIdentity(string stage)
        {
            var expected = target ?? throw new InvalidOperationException("Missing target manifest.");
            ct.ThrowIfCancellationRequested();
            var consoleHash = await HashFile(expected.ConsolePath, ct);
            Require(consoleHash == expected.ConsoleSha256.ToUpperInvariant(), "LD console digest changed: " + consoleHash);
            using var probeDeadline = CancellationTokenSource.CreateLinkedTokenSource(ct);
            probeDeadline.CancelAfter(TimeSpan.FromSeconds(3));
            using var connection = await ExistingAdbSocket.OpenAsync(new IPEndPoint(IPAddress.Loopback, 5037), probeDeadline.Token);
            var owner = AdbServerIdentity.ForConnection(connection.ConnectedSocket);
            await Trace("target-proof.jsonl", new { stage, utc = DateTimeOffset.UtcNow, observedOwner = owner });
            Require(owner.ProcessId == expected.ServerPid && owner.StartedUtcTicks == expected.ServerStartedUtcTicks, "Actual 5037 connection owner differs from target manifest: " + owner);
            var server = ProcessIdentity(expected.ServerPid, expected.ServerStartedUtcTicks);
            Require(string.Equals(server.Path, expected.ServerPath, StringComparison.OrdinalIgnoreCase), "Actual ADB owner executable path changed: " + server.Path);
            var serverHash = await HashFile(server.Path, ct);
            Require(serverHash == expected.ServerSha256.ToUpperInvariant(), "Actual ADB owner executable digest changed: " + serverHash);
            var rows = await admissionService!.RefreshAsync(ct);
            var row = rows.SingleOrDefault(item => item.Index == expected.Index);
            await Trace("target-proof.jsonl", new { stage, utc = DateTimeOffset.UtcNow, observedInstance = row });
            Require(row is not null && row.Name == expected.Name && row.AndroidStarted && row.ProcessId == expected.PlayerPid && row.VBoxProcessId == expected.VBoxPid,
                "Dedicated instance is stopped, missing, renamed, or has changed processes; no instance was launched.");
            var player = ProcessIdentity(expected.PlayerPid, expected.PlayerStartedUtcTicks);
            var vbox = ProcessIdentity(expected.VBoxPid, expected.VBoxStartedUtcTicks);
            await LdPlayerAdbBinding.VerifyAsync(expected.Serial, expected.VBoxPid, expected.VBoxStartedUtcTicks, ct);
            Require(AdbServerIdentity.ForConnection(connection.ConnectedSocket) == owner, "ADB owner changed during host proof.");
            ProcessIdentity(expected.ServerPid, expected.ServerStartedUtcTicks);
            var proof = new { stage, utc = DateTimeOffset.UtcNow, instance = row, player, vbox, owner, server, serverSha256 = serverHash, consoleSha256 = consoleHash };
            await Trace("target-proof.jsonl", proof);
            return proof;
        }

        async Task<object> RuntimeIdentity(string stage)
        {
            await HostIdentity(stage + "-host-before");
            var currentRuntime = runtime ?? throw new InvalidOperationException("Missing runtime.");
            var expected = target!;
            currentRuntime.RequireFrozenConnectionSettings();
            Require(currentRuntime.LdPlayer is not null && currentRuntime.LdPlayerError is null, "Runtime has no verified LD service: " + currentRuntime.LdPlayerError);
            await currentRuntime.RefreshLdPlayerAsync(ct);
            var instance = currentRuntime.LdPlayer!.Instances.SingleOrDefault(item => item.Index == expected.Index);
            Require(instance is not null && instance.Name == expected.Name && instance.ProcessId == expected.PlayerPid && instance.VBoxProcessId == expected.VBoxPid && instance.AndroidStarted,
                "Runtime LD target changed.");
            if (frozenLdGeneration is { } generation) Require(instance!.Generation == generation, "Runtime LD generation changed.");
            var backend = currentRuntime.Backend as AdbDeviceBackend ?? throw new InvalidOperationException("Real ADB backend required.");
            await backend.VerifyIdentityAsync(expected.Serial, ct);
            var registered = await backend.ListRegisteredDevicesAsync(ct);
            RequireRegistered(registered, expected);
            var device = currentRuntime.Devices.Snapshot.SingleOrDefault();
            Require(device is not null && device.Serial == expected.Serial && device.Online && device.TransportId == expected.TransportId && backend.ObservedTransportId(expected.Serial) == expected.TransportId,
                "Runtime did not retain exactly the verified Ready transport: " + currentRuntime.DiscoveryError);
            if (frozenDevice is not null) Require(device!.Generation == frozenDevice.Generation && device.TransportId == frozenDevice.TransportId, "Runtime device generation changed.");
            await HostIdentity(stage + "-host-after");
            var proof = new { stage, utc = DateTimeOffset.UtcNow, device, instance, registered };
            await Trace("target-proof.jsonl", proof);
            return proof;
        }

        async Task<string> Read(string id, string command)
        {
            return await Step(id, async () =>
            {
                await RuntimeIdentity(id + "-before");
                var watch = Stopwatch.StartNew();
                string? value = null;
                Exception? commandError = null;
                try
                {
                    value = await runtime!.Backend.ShellAsync(target!.Serial, command, ct: ct);
                    await Trace("shell-trace.jsonl", new { id, utc = DateTimeOffset.UtcNow, command, stdout = value, shellV2ExitCode = 0, durationMs = watch.ElapsedMilliseconds });
                    Require(Encoding.UTF8.GetByteCount(value) <= MaximumReadBytes, "Preflight read exceeded its 256 KiB output budget: " + command);
                }
                catch (Exception ex)
                {
                    commandError = ex;
                    await Trace("shell-trace.jsonl", new { id, utc = DateTimeOffset.UtcNow, command, stdout = value, durationMs = watch.ElapsedMilliseconds, error = ex.ToString() });
                }
                try { await RuntimeIdentity(id + "-after"); }
                catch (Exception ex) when (commandError is not null) { commandError = new AggregateException(commandError, ex); }
                if (commandError is not null) System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(commandError).Throw();
                return value!;
            });
        }

        async Task Ledger(string state)
        {
            ledgerState = state;
            var entry = new { runId, utc = DateTimeOffset.UtcNow, state, target, device = frozenDevice, ldGeneration = frozenLdGeneration,
                package = Package, apkPath, apkSha256 = ApkSha256, installAttempted, owned, ownedPackage,
                automaticCleanup = false, manualReviewRequired = (installAttempted || ownedCleanup) && state != "UNINSTALL-VERIFIED", level = "SHARED-BATCH-AND-BACKEND-NOT-GUI",
                sourceLedgerPath = ledgerInput, sourceSha256 = ownedCleanup ? expectedLedgerSha : null, sourceRunId = sourceLedger?.RunId, sourceInstallAttempted = sourceLedger?.InstallAttempted, sourceLedgerValidated };
            await Save("installation-ledger.json", entry);
            await Trace("installation-ledger.jsonl", entry);
        }

        async Task<bool> PackageObserved(string id, bool includeRetained) => PackagePresent(await Read(id,
            "pm list packages " + (includeRetained ? "-u " : "") + "--user all " + Package));

        async Task<OwnedPackageProof> ObservePackage(string id)
        {
            var dump = await Read(id + "-package", "dumpsys package " + Package);
            var path = await Read(id + "-path", "pm path " + Package);
            return PackageIdentity(dump, path);
        }

        async Task RequireOwned(string id)
        {
            Require(owned && ownedPackage is not null, "This run has no verified ownership; mutation is prohibited.");
            Require(await PackageObserved(id + "-installed", false) && await PackageObserved(id + "-retained", true), "Owned package is no longer observed by both all-users queries.");
            Require(await ObservePackage(id) == ownedPackage, "Owned package version/codePath/PM path changed; no further mutation or automatic cleanup is allowed.");
        }

        async Task ApkIdentity(string id)
        {
            await Step(id, async () =>
            {
                Require(new FileInfo(apkPath).Length == ApkBytes && await HashFile(apkPath, ct) == ApkSha256, "APK changed; no further mutation is allowed.");
                return new { bytes = ApkBytes, sha256 = ApkSha256, readOnlyLeaseHeld = apkLease is not null };
            });
        }

        async Task Screenshot(string id)
        {
            await Step(id, async () =>
            {
                await RuntimeIdentity(id + "-before");
                var png = await runtime!.Backend.ScreenshotAsync(target!.Serial, ct);
                var dimensions = PngImage.ReadDimensions(png);
                Require(dimensions.Width > 0 && dimensions.Height > 0, "Screenshot is empty.");
                await File.WriteAllBytesAsync(Path.Combine(output, id + ".png"), png, ct);
                await RuntimeIdentity(id + "-after");
                return new { dimensions.Width, dimensions.Height, bytes = png.Length, sha256 = Convert.ToHexString(SHA256.HashData(png)), file = id + ".png", level = "BACKEND-NOT-GUI" };
            });
        }

        async Task Lifecycle(bool advertisedArm64, int sdk, string boot, bool existing)
        {
            Require(lifecycle, "Read-only preflight cannot authorize a lifecycle.");
            Require(!existing && advertisedArm64 && sdk >= 21 && boot == "1", "Lifecycle requires absent all-users/retained package, boot readiness, SDK >=21 and advertised ARM64 ABI candidate; translation or native CPU is not proved by these properties.");
            installationAuthorized = true;
            await Ledger("ELIGIBLE-NOT-ATTEMPTED");
            var frozen = ApplicationTarget.Freeze(frozenDevice!);
            var installCalls = 0;
            var scopedBackend = new LifecycleBackend(runtime!.Backend, target!.Serial, apkPath, async operation =>
            {
                await RuntimeIdentity("mutation-" + operation + "-before");
                if (operation == ApplicationOperation.Install)
                {
                    Require(installCalls++ == 0 && !installAttempted && !owned, "Install may be attempted only once.");
                    Require(!await PackageObserved("install-last-installed-check", false) && !await PackageObserved("install-last-retained-check", true), "Package appeared before actual install; replacement is prohibited.");
                    await ApkIdentity("install-last-apk-check");
                    installAttempted = true;
                }
                else await RequireOwned("mutation-" + operation);
                await RuntimeIdentity("mutation-" + operation + "-admitted");
                writeAttempted = true;
                await Ledger(operation.ToString().ToUpperInvariant() + "-REQUEST-MAY-BE-SENT");
            }, value => Trace("backend-calls.jsonl", value), ct);
            var batch = new ApplicationBatch(scopedBackend, async (applicationTarget, _) =>
            {
                await RuntimeIdentity("batch-validate");
                applicationTarget.RequireSame(runtime.Devices.Snapshot.SingleOrDefault());
            });
            async Task<ApplicationItemResult> Execute(string id, IReadOnlyList<ApplicationBatchItem> plan, ApplicationTaskState expectedState)
            {
                return await Step(id, async () =>
                {
                    Require(plan.Count == 1 && plan[0].Operation != ApplicationOperation.Clear, "Lifecycle permits one fixed application item only.");
                    var results = await batch.ExecuteAsync(plan, confirmed: true, progress: progress => File.AppendAllText(Path.Combine(output, "batch-progress.jsonl"),
                        JsonSerializer.Serialize(new { utc = DateTimeOffset.UtcNow, progress }, options) + "\n"), stopAfterCurrent: ct);
                    await Save(id + "-batch-results.json", results);
                    Require(results.Count == 1 && results[0].State == expectedState, "Shared application batch did not reach expected state: " + JsonSerializer.Serialize(results, options));
                    return results[0];
                });
            }
            IReadOnlyList<ApplicationBatchItem> RowPlan(ApplicationOperation operation) => ApplicationBatch.RowPlan(operation, [new(frozen, operation, Package, IsSystem: false)]);
            var initialList = await Execute("lifecycle-list-before", [new(frozen, ApplicationOperation.List)], ApplicationTaskState.Succeeded);
            Require(initialList.Applications is not null && !initialList.Applications.Any(app => app.PackageName == Package), "Shared initial list already contains the package.");
            var installed = await Execute("lifecycle-install", ApplicationBatch.InstallPlan([frozen], [apkPath]), ApplicationTaskState.CompletedUnverified);
            await ApkIdentity("install-after-apk-check");
            var app = installed.Applications?.SingleOrDefault(item => item.PackageName == Package);
            Require(app is not null && app.VersionName == VersionName && !app.IsSystem, "Install result has no exact non-system CMFA version; ownership remains unverified.");
            Require(await PackageObserved("installed-all-users-proof", false) && await PackageObserved("installed-retained-proof", true), "Installed package is not observed consistently across all-users queries.");
            ownedPackage = await ObservePackage("installed-owned-proof");
            owned = true;
            await Ledger("INSTALL-OWNERSHIP-VERIFIED");
            var list = await Execute("lifecycle-list-installed", [new(frozen, ApplicationOperation.List)], ApplicationTaskState.Succeeded);
            Require(list.Applications?.Count(item => item.PackageName == Package && item.VersionName == VersionName && !item.IsSystem) == 1, "Shared list does not confirm the exact owned application.");

            var launcher = await Read("owned-launcher-resolution", "cmd package resolve-activity --brief -a android.intent.action.MAIN -c android.intent.category.LAUNCHER -p " + Package);
            var launcherComponents = launcher.Split('\n').Select(line => line.Trim()).Where(line => line.StartsWith(Package + "/", StringComparison.Ordinal)
                && Regex.IsMatch(line[(Package.Length + 1)..], @"^[A-Za-z0-9_.$]+$", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1))).ToArray();
            Require(launcherComponents.Length == 1, "The owned package has no unique launcher alias/component; no Start was sent.");
            await Execute("lifecycle-start", RowPlan(ApplicationOperation.Start), ApplicationTaskState.Succeeded);
            await Step("start-settle", async () => { await Task.Delay(1000, ct); return new { delayMs = 1000, observations = 1, retries = 0 }; });
            var startedProcesses = PackageProcesses(await Read("started-processes", "ps -A -o PID,NAME"));
            Require(startedProcesses.Length > 0, "Start returned without any exact package process.");
            var startedActivity = await Read("started-activity", "dumpsys activity activities");
            Require(IsResumed(startedActivity), "The package is not the observed resumed activity.");
            await Screenshot("started-screen");
            await RequireOwned("after-start-owned-proof");
            await Ledger("START-EFFECT-VERIFIED");

            await Execute("lifecycle-stop", RowPlan(ApplicationOperation.Stop), ApplicationTaskState.Succeeded);
            await Step("stop-settle", async () => { await Task.Delay(1000, ct); return new { delayMs = 1000, observations = 1, retries = 0 }; });
            Require(PackageProcesses(await Read("stopped-processes", "ps -A -o PID,NAME")).Length == 0, "An exact package process remains after Stop; no automatic further mutation.");
            Require(!IsResumed(await Read("stopped-activity", "dumpsys activity activities")), "Package is still reported as resumed after Stop.");
            await Screenshot("stopped-screen");
            await RequireOwned("after-stop-owned-proof");
            await Ledger("STOP-EFFECT-VERIFIED-INSTALLATION-RETAINED");

            await Execute("lifecycle-uninstall", RowPlan(ApplicationOperation.Uninstall), ApplicationTaskState.Succeeded);
            Require(!await PackageObserved("uninstalled-all-users-proof", false) && !await PackageObserved("uninstalled-retained-proof", true), "Uninstall did not remove the exact package including retained records across all-users queries.");
            Require(PackageProcesses(await Read("uninstalled-processes", "ps -A -o PID,NAME")).Length == 0, "Package process remains after uninstall.");
            await RuntimeIdentity("lifecycle-final");
            await ApkIdentity("lifecycle-final-apk-check");
            owned = false;
            await Ledger("UNINSTALL-VERIFIED");
            lifecycleResult = new { level = "SHARED-BATCH-AND-BACKEND-NOT-GUI", package = Package, expectedVersionCode = VersionCode, expectedVersionName = VersionName,
                ownershipProof = ownedPackage, launcherComponents, startedProcesses, installBatchState = "CompletedUnverified followed by independent PackageManager identity checks",
                abiScope = "Advertised ARM64 ABI candidate only; observed installation and launch, not proof of native ARM CPU.",
                userInterface = "NOT-RUN", vpnConfiguration = "NOT-TOUCHED", permissions = "NO-PERMISSION-GRANT-OPERATIONS", proxy = "NOT-TOUCHED",
                appInitiatedNetwork = "NOT-CONTROLLED-OR-VERIFIED", automaticCleanup = false, retries = 0 };
        }

        async Task OwnedCleanup()
        {
            var source = sourceLedger ?? throw new InvalidDataException("An independently validated source ledger is required for owned cleanup.");
            Require(ownedCleanup && !lifecycle && sourceLedgerValidated, "This entry cannot authorize owned cleanup.");
            await Ledger("SOURCE-VALIDATED-OWNERSHIP-NOT-YET-REPROVED");
            Require(await PackageObserved("cleanup-initial-installed", false) && await PackageObserved("cleanup-initial-retained", true), "Source-owned package is no longer consistently observed.");
            var currentPackage = await ObservePackage("cleanup-initial-package-proof");
            Require(currentPackage == source.OwnedPackage, "Current package version/codePath/PM path differs from the source-owned installation.");
            var frozen = ApplicationTarget.Freeze(frozenDevice!);
            GuestProcess[]? stopBefore = null;
            var scopedBackend = new LifecycleBackend(runtime!.Backend, target!.Serial, apkPath, async operation =>
            {
                Require(operation is ApplicationOperation.Stop or ApplicationOperation.Uninstall, "Owned cleanup permits no other mutation.");
                await RequireOwned("cleanup-mutation-" + operation);
                if (operation == ApplicationOperation.Stop)
                    stopBefore = PackageProcesses(await Read("cleanup-stop-before-processes", "ps -A -o PID,NAME"));
                await RuntimeIdentity("cleanup-mutation-" + operation + "-admitted");
                writeAttempted = true;
                await Ledger(operation.ToString().ToUpperInvariant() + "-REQUEST-MAY-BE-SENT");
            }, value => Trace("backend-calls.jsonl", value), ct, cleanupOnly: true);
            var batch = new ApplicationBatch(scopedBackend, async (applicationTarget, _) =>
            {
                await RuntimeIdentity("cleanup-batch-validate");
                applicationTarget.RequireSame(runtime.Devices.Snapshot.SingleOrDefault());
            });
            async Task<ApplicationItemResult> Execute(string id, ApplicationOperation operation)
            {
                return await Step(id, async () =>
                {
                    Require(operation is ApplicationOperation.List or ApplicationOperation.Stop or ApplicationOperation.Uninstall, "Operation is outside the owned cleanup scope.");
                    IReadOnlyList<ApplicationBatchItem> plan = operation == ApplicationOperation.List ? [new(frozen, operation)]
                        : ApplicationBatch.RowPlan(operation, [new(frozen, operation, Package, IsSystem: false)]);
                    var results = await batch.ExecuteAsync(plan, confirmed: true, progress: progress => File.AppendAllText(Path.Combine(output, "batch-progress.jsonl"),
                        JsonSerializer.Serialize(new { utc = DateTimeOffset.UtcNow, progress }, options) + "\n"), stopAfterCurrent: ct);
                    await Save(id + "-batch-results.json", results);
                    Require(results.Count == 1 && results[0].State == ApplicationTaskState.Succeeded, "Owned cleanup shared batch did not succeed: " + JsonSerializer.Serialize(results, options));
                    return results[0];
                });
            }
            var before = await Execute("cleanup-list-before", ApplicationOperation.List);
            Require(before.Applications?.Count(app => app.PackageName == Package && app.VersionName == VersionName && !app.IsSystem) == 1, "Shared list does not confirm the exact non-system source-owned application.");
            ownedPackage = currentPackage;
            owned = true;
            await RequireOwned("cleanup-final-ownership-reproof");
            await Ledger("SOURCE-OWNERSHIP-REPROVED");
            await Execute("cleanup-stop", ApplicationOperation.Stop);
            await Step("cleanup-stop-settle", async () => { await Task.Delay(1000, ct); return new { delayMs = 1000, observations = 1, retries = 0 }; });
            var stopAfter = PackageProcesses(await Read("cleanup-stop-after-processes", "ps -A -o PID,NAME"));
            Require(stopBefore is not null && stopAfter.Length == 0, "Exact package processes remain, or no pre-Stop observation was captured.");
            await RequireOwned("cleanup-stopped-installation-retained");
            var stopTransitionProved = stopBefore!.Length > 0;
            await Ledger(stopTransitionProved ? "STOP-EFFECT-VERIFIED-INSTALLATION-RETAINED" : "ALREADY-STOPPED-INSTALLATION-RETAINED");
            await Execute("cleanup-uninstall", ApplicationOperation.Uninstall);
            Require(!await PackageObserved("cleanup-uninstalled-all-users", false) && !await PackageObserved("cleanup-uninstalled-retained", true), "Uninstall did not remove the exact all-users package and retained records.");
            Require(PackageProcesses(await Read("cleanup-uninstalled-processes", "ps -A -o PID,NAME")).Length == 0, "Exact package processes remain after uninstall.");
            var after = await Execute("cleanup-list-after", ApplicationOperation.List);
            Require(after.Applications is not null && !after.Applications.Any(app => app.PackageName == Package), "Shared list still contains the uninstalled package.");
            await RuntimeIdentity("cleanup-final");
            await ApkIdentity("cleanup-final-apk-check");
            await Step("cleanup-final-source-check", async () =>
            {
                Require(await HashFile(ledgerInput!, ct) == OwnedSourceSha256, "Source ledger changed during owned cleanup.");
                return new { sourceSha256 = OwnedSourceSha256, sourceRunId = source.RunId, sourceUnchanged = true };
            });
            owned = false;
            await Ledger("UNINSTALL-VERIFIED");
            ownedCleanupResult = new { level = "SHARED-BATCH-AND-BACKEND-NOT-GUI", sourceRunId = source.RunId, sourceSha256 = OwnedSourceSha256,
                sourceDeviceGeneration = source.Device.Generation, sourceLdGeneration = source.LdGeneration, ownershipProof = ownedPackage,
                stopBefore, stopAfter, stopTransitionProved, stopObservation = stopTransitionProved ? "TERMINATED-OBSERVED-PACKAGE-PROCESSES" : "ALREADY-STOPPED-IDEMPOTENT-NO-TRANSITION-PROVED",
                uninstallObserved = true, install = "NOT-RUN", start = "NOT-RUN", permissionGrantOperations = "NOT-RUN", currentGui = "NOT-RUN", originalLifecycleFailure = "UNCHANGED", retries = 0, automaticCleanup = false };
        }

        try
        {
            Require(OperatingSystem.IsWindows(), "Existing ADB owner preflight requires Windows.");
            await Step("inputs", async () =>
            {
                foreach (var assembly in new[] { typeof(ApplicationPreflightAcceptance).Assembly, typeof(AdbDeviceBackend).Assembly, typeof(MatrixRuntime).Assembly, typeof(ScriptScheduler).Assembly }.Distinct())
                    builds[assembly.GetName().Name!] = await HashFile(assembly.Location, ct);
                Require(new FileInfo(targetPath).Length is > 0 and <= 64 * 1024, "Target manifest must be 1..65536 bytes.");
                var bytes = await File.ReadAllBytesAsync(targetPath, ct);
                await File.WriteAllBytesAsync(Path.Combine(output, "target-input.json"), bytes);
                target = JsonSerializer.Deserialize<Target>(bytes, JsonDefaults.Options) ?? throw new InvalidDataException("Empty target manifest.");
                Validate(target);
                if (ownedCleanup)
                {
                    Require(IsSha256(expectedLedgerSha!) && string.Equals(expectedLedgerSha, OwnedSourceSha256, StringComparison.OrdinalIgnoreCase), "Only the explicitly authorized source ledger SHA256 is accepted.");
                    sourceLedgerLease = new FileStream(ledgerInput!, FileMode.Open, FileAccess.Read, FileShare.Read);
                    Require(sourceLedgerLease.Length is > 0 and <= 64 * 1024, "Source ledger must be 1..65536 bytes.");
                    var sourceBytes = new byte[(int)sourceLedgerLease.Length];
                    await sourceLedgerLease.ReadExactlyAsync(sourceBytes, ct);
                    Require(Convert.ToHexString(SHA256.HashData(sourceBytes)) == OwnedSourceSha256, "Source ledger bytes do not match the authorized digest.");
                    sourceLedger = JsonSerializer.Deserialize<SourceLedger>(sourceBytes, JsonDefaults.Options) ?? throw new InvalidDataException("Empty source ledger.");
                    ValidateSource(sourceLedger, target, apkPath);
                    sourceLedgerValidated = true;
                    await File.WriteAllBytesAsync(Path.Combine(output, "source-ledger-input.json"), sourceBytes, ct);
                }
                if (mutableMode) apkLease = new FileStream(apkPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                Require(new FileInfo(apkPath).Length == ApkBytes && await HashFile(apkPath, ct) == ApkSha256, "Supplied CMFA APK length or SHA256 differs from the fixed candidate.");
                return new { targetPath, targetSha256 = Convert.ToHexString(SHA256.HashData(bytes)), apkPath, apkBytes = ApkBytes, apkSha256 = ApkSha256, package = Package, expectedVersionCode = VersionCode, expectedVersionName = VersionName, signatureVerified = false, builds,
                    sourceLedgerPath = ledgerInput, sourceSha256 = ownedCleanup ? OwnedSourceSha256 : null, sourceRunId = sourceLedger?.RunId };
            });
            var expected = target!;
            var runner = new LdConsoleRunner(expected.ConsolePath, CommandSeconds);
            admissionService = new LdPlayerService(new ReadOnlyLdRunner(runner, async value => await Trace("ldconsole-trace.jsonl", value)));
            await Step("host-before-seed", () => HostIdentity("before-seed"));
            var settings = new AppSettings { AdbAllowedSerials = [], LdSerialBindings = [], LdConsolePath = expected.ConsolePath, CommandTimeoutSeconds = mutableMode ? 60 : CommandSeconds, AutoStartApi = false };
            seed = new AdbDeviceBackend(settings);
            var registered = await Step("host-only-seed", () => seed.ListRegisteredDevicesAsync(ct));
            RequireRegistered(registered, expected);
            await Step("host-after-seed", () => HostIdentity("after-seed"));
            var admissions = await Step("admission", async () =>
            {
                var snapshot = admissionService.Instances.Single(item => item.Index == expected.Index);
                var proofs = await LdConnectionAdmission.ResolveAsync(seed, admissionService, [expected.Index], [snapshot], new Dictionary<int, long> { [expected.Index] = expected.VBoxStartedUtcTicks }, ct);
                Require(proofs.Count == 1 && proofs[0].Index == expected.Index && proofs[0].Name == expected.Name && proofs[0].Serial == expected.Serial && proofs[0].TransportId == expected.TransportId && proofs[0].VBoxPid == expected.VBoxPid && proofs[0].VBoxBirth == expected.VBoxStartedUtcTicks, "Resolved admission differs from the frozen manifest.");
                return proofs;
            });
            settings = LdConnectionAdmission.Apply(settings, admissions);
            await Step("host-before-runtime", () => HostIdentity("before-runtime"));
            await Step("runtime-create", async () =>
            {
                await Save("initialization-scope.json", new
                {
                    scope = "One real runtime; existing guarded discovery also reads getprop, wm size, and dumpsys input before the explicit probes. Their raw shell streams are not exposed by MatrixRuntime; the resulting device snapshot and owned runtime logs are retained.",
                    host = new[] { "ldconsole list2", "host:version", "host:devices-l", "TCP owner and VBox binding probes" },
                    guest = new[] { "getprop", "wm size", "dumpsys input" }, autoStartApi = false, externalAdbClient = false, guestWrites = false,
                    lifecycleBatchReads = mutableMode ? "Shared ApplicationBatch.ListApps(true,true) also lists installed/system packages and runs dumpsys package for every listed package before/after operations. Raw internal shell success streams are not exposed by the existing backend; returned app lists, progress and original errors are retained." : null
                });
                Exception? creationError = null;
                try { runtime = await MatrixRuntime.CreateAsync(settings, Path.Combine(output, "runtime"), admissions: admissions, epochSource: seed, ct: ct); }
                catch (Exception ex) { creationError = ex; }
                try { await HostIdentity("after-runtime-create"); }
                catch (Exception ex) when (creationError is not null) { creationError = new AggregateException(creationError, ex); }
                if (creationError is not null) System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(creationError).Throw();
                return new { runtime!.DiscoveryError, runtime.LdPlayerError, devices = runtime.Devices.Snapshot, runtime.ApiStartupError };
            });
            await Step("runtime-verified", () => RuntimeIdentity("after-runtime"));
            frozenDevice = runtime!.Devices.Snapshot.Single();
            frozenLdGeneration = runtime.LdPlayer!.Instances.Single(item => item.Index == expected.Index).Generation;
            await Save("frozen-identity.json", new { target, admissions, device = frozenDevice, ldGeneration = frozenLdGeneration });

            var abi = AbiList(await Read("abi-list", "getprop ro.product.cpu.abilist"), false);
            var abi64 = AbiList(await Read("abi-list64", "getprop ro.product.cpu.abilist64"), true);
            var bridge = SingleLine(await Read("native-bridge", "getprop ro.dalvik.vm.native.bridge"));
            var sdkText = SingleLine(await Read("sdk", "getprop ro.build.version.sdk"));
            Require(int.TryParse(sdkText, NumberStyles.None, CultureInfo.InvariantCulture, out var sdk) && sdk is > 0 and < 1000, "Malformed Android SDK property: " + sdkText);
            var boot = SingleLine(await Read("boot", "getprop sys.boot_completed"));
            Require(boot is "" or "0" or "1", "Malformed boot property: " + boot);
            var installed = PackagePresent(await Read("package-installed-all-users", "pm list packages --user all " + Package));
            var retained = PackagePresent(await Read("package-including-retained-all-users", "pm list packages -u --user all " + Package));
            Require(!installed || retained, "PackageManager snapshots disagree; package state changed during preflight.");
            string? packageDump = null;
            string? resolve = null;
            if (retained) packageDump = await Read("existing-package-diagnostic", "dumpsys package " + Package);
            if (installed) resolve = await Read("existing-launcher-diagnostic", "cmd package resolve-activity --brief -a android.intent.action.MAIN -c android.intent.category.LAUNCHER -p " + Package);
            var nativeArm64 = abi.Contains("arm64-v8a", StringComparer.Ordinal) && abi64.Contains("arm64-v8a", StringComparer.Ordinal);
            var bridgeAdvertised = bridge.Length > 0 && bridge != "0";
            var classification = retained ? "PROTECTED-EXISTING-OR-RETAINED-PACKAGE" : boot != "1" ? "BOOT-NOT-READY" : sdk < 21 ? "SDK-NON-COMPATIBLE" : nativeArm64 ? "NATIVE-ABI-CANDIDATE-NOT-INSTALL-VERIFIED" : bridgeAdvertised ? "NATIVE-BRIDGE-ADVERTISED-UNVERIFIED" : "NATIVE-ABI-NON-COMPATIBLE";
            result = new
            {
                classification, abi, abi64, nativeArm64, nativeBridgeRaw = bridge, nativeBridgeAdvertised = bridgeAdvertised, sdk, boot,
                packageScope = "PackageManager --user all observation, including -u retained-package records; not a proof about every possible on-disk data remnant.",
                installedObserved = installed, existingOrRetainedObserved = retained, retainedOnlyObserved = retained && !installed,
                protectedPackage = retained, installationAuthorized = false, signatureVerified = false,
                launcherResolution = resolve is null ? "NOT-RESOLVED" : "READ-ONLY-DIAGNOSTIC-NOT-LAUNCHED", packageDump, resolve,
                gui = "NOT-RUN", installation = "NOT-RUN", launch = "NOT-RUN", stop = "NOT-RUN", uninstall = "NOT-RUN"
            };
            await Step("final-identity", () => RuntimeIdentity("final"));
            await Step("final-apk-digest", async () =>
            {
                Require(new FileInfo(apkPath).Length == ApkBytes && await HashFile(apkPath, ct) == ApkSha256, "APK changed during preflight.");
                return new { bytes = ApkBytes, sha256 = ApkSha256 };
            });
            if (lifecycle) await Lifecycle(nativeArm64, sdk, boot, retained);
            if (ownedCleanup) await OwnedCleanup();
        }
        catch (Exception ex)
        {
            failure = ex;
            if (mutableMode) await Ledger(ownedCleanup || installAttempted ? "PENDING-MANUAL-REVIEW-NO-AUTOMATIC-CLEANUP" : "FAILED-BEFORE-INSTALL");
            var entry = new { id = "preflight", utc = DateTimeOffset.UtcNow, status = "FAIL", error = ex.ToString() };
            steps.Add(entry); await Trace("results.jsonl", entry);
        }
        finally
        {
            async Task Cleanup(string name, Func<Task> operation)
            {
                try { await operation().WaitAsync(TimeSpan.FromSeconds(10)); }
                catch (Exception ex) { cleanupErrors.Add(name + ": " + ex); }
            }
            if (runtime is not null) await Cleanup("runtime", () => runtime.DisposeAsync().AsTask());
            if (admissionService is not null) await Cleanup("admission-service", () => admissionService.DisposeAsync().AsTask());
            if (seed is not null) await Cleanup("host-only-seed", () => seed.DisposeAsync().AsTask());
            if (apkLease is not null) await Cleanup("apk-read-only-lease", () => apkLease.DisposeAsync().AsTask());
            if (sourceLedgerLease is not null) await Cleanup("source-ledger-read-only-lease", () => sourceLedgerLease.DisposeAsync().AsTask());
            await Save("summary.json", new
            {
                mode = ownedCleanup ? "application-owned-cleanup" : lifecycle ? "application-lifecycle" : "application-preflight", status = failure is null && cleanupErrors.Count == 0 ? ownedCleanup ? "COMPLETED-SHARED-BACKEND-OWNED-CLEANUP-NOT-GUI" : lifecycle ? "COMPLETED-SHARED-BACKEND-LIFECYCLE-NOT-GUI" : "COMPLETED-READ-ONLY" : "FAIL",
                durationMs = clock.ElapsedMilliseconds, target, frozenDevice, frozenLdGeneration, builds, result, error = failure?.ToString(), cleanupErrors, steps,
                limits = new { totalSeconds = mutableMode ? 600 : 180, commandSeconds = mutableMode ? 60 : CommandSeconds, recommendedOuterProcessSeconds = mutableMode ? 900 : 240, probeOutputBytes = MaximumReadBytes, transportStdoutBytes = AdbDeviceBackend.MaximumStdoutBytes, transportStderrBytes = AdbDeviceBackend.MaximumStderrBytes, automaticRetries = 0 },
                guestWrites = writeAttempted, apiStarted = false, externalAdbClient = false, instanceLaunchQuitModify = false, installationAuthorized,
                lifecycleResult, ownedCleanupResult, ledgerState = mutableMode ? ledgerState : null, installAttempted, owned, ownedPackage, automaticGuestCleanup = false,
                sourceLedgerPath = ledgerInput, sourceSha256 = ownedCleanup ? OwnedSourceSha256 : null, sourceRunId = sourceLedger?.RunId, sourceLedgerValidated
            });
        }
        if (failure is not null) System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(failure).Throw();
        if (cleanupErrors.Count > 0) throw new IOException("Preflight resource cleanup failed; see summary.json.");
        Console.WriteLine(ownedCleanup ? "COMPLETED shared application backend owned cleanup; original lifecycle failure and current GUI remain unchanged." : lifecycle ? "COMPLETED shared application backend lifecycle; current GUI remains NOT-RUN." : "COMPLETED-READ-ONLY application-preflight; installation remains unauthorized.");
    }

    private static void ValidateSource(SourceLedger source, Target target, string apkPath)
    {
        Require(Guid.TryParseExact(source.RunId, "N", out var id) && id != Guid.Empty, "Source runId must be a nonempty N-format GUID.");
        Require(source.State == "PENDING-MANUAL-REVIEW-NO-AUTOMATIC-CLEANUP" && source.Owned && source.InstallAttempted && !source.AutomaticCleanup && source.ManualReviewRequired
            && source.Package == Package && source.Level == "SHARED-BATCH-AND-BACKEND-NOT-GUI", "Source ledger is not the authorized pending owned installation.");
        Require(source.Target == target, "Every source target field must equal the current manifest before any host operation.");
        Require(source.Device is not null && source.Device.Serial == target.Serial && source.Device.TransportId == target.TransportId && source.Device.Online
            && source.Device.Generation > 0 && source.LdGeneration > 0, "Source device serial/transport or source generation proof is invalid.");
        Require(source.ApkPath == apkPath && source.ApkSha256 == ApkSha256, "Source APK path or digest differs from the fixed input.");
        var package = source.OwnedPackage;
        Require(package is not null && package.VersionCode == VersionCode && package.VersionName == VersionName && package.CodePath.StartsWith("/data/app/", StringComparison.Ordinal)
            && !package.CodePath.Any(char.IsWhiteSpace) && !package.CodePath.Any(char.IsControl) && package.PackageManagerPath == package.CodePath + "/base.apk",
            "Source package does not have the exact version and non-system codePath/base.apk proof.");
    }

    private static void Validate(Target target)
    {
        Require(target.SchemaVersion == 1 && target.Index is 1 or 2 && target.Name.StartsWith("MatrixAcceptance-", StringComparison.Ordinal) && target.Name.Length > "MatrixAcceptance-".Length,
            "Only schema 1 and named dedicated MatrixAcceptance- instances 1/2 are accepted.");
        Require(target.PlayerPid > 0 && target.VBoxPid > 0 && target.ServerPid > 0 && target.PlayerPid != target.VBoxPid && target.PlayerStartedUtcTicks > 0 && target.VBoxStartedUtcTicks > 0 && target.ServerStartedUtcTicks > 0 && target.TransportId > 0,
            "Positive, distinct player/VBox identities, process births, and transport are required.");
        Require(!string.IsNullOrWhiteSpace(target.Serial) && !target.Serial.Any(char.IsWhiteSpace) && !target.Serial.Contains('\0'), "Invalid target serial.");
        var localSerial = target.Serial.StartsWith("emulator-", StringComparison.Ordinal)
            ? int.TryParse(target.Serial.AsSpan(9), NumberStyles.None, CultureInfo.InvariantCulture, out var port) && port is > 0 and < 65535
            : IPEndPoint.TryParse(target.Serial, out var endpoint) && IPAddress.IsLoopback(endpoint.Address) && endpoint.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && endpoint.Port > 0;
        Require(localSerial, "Only a loopback LD serial is accepted.");
        Require(Absolute(target.ServerPath) == target.ServerPath && Absolute(target.ConsolePath) == target.ConsolePath, "Executable paths must be canonical absolute paths.");
        Require(Path.GetFileName(target.ConsolePath).Equals("ldconsole.exe", StringComparison.OrdinalIgnoreCase), "Only the explicitly frozen ldconsole.exe is accepted.");
        Require(IsSha256(target.ServerSha256) && IsSha256(target.ConsoleSha256), "Executable SHA256 values must contain 64 hexadecimal characters.");
    }

    private static void RequireRegistered(IReadOnlyList<AdbRegisteredDevice> rows, Target target)
    {
        var matches = rows.Where(row => row.Serial == target.Serial).ToArray();
        Require(matches.Length == 1 && matches[0].State == "device" && matches[0].TransportId == target.TransportId, "Host directory no longer has the exact frozen Ready transport.");
    }

    private static ProcessProof ProcessIdentity(int pid, long birth)
    {
        using var process = Process.GetProcessById(pid);
        var actual = process.StartTime.ToUniversalTime().Ticks;
        var path = process.MainModule?.FileName;
        Require(actual == birth && !process.HasExited && !string.IsNullOrWhiteSpace(path), $"Process is missing, inaccessible, or its PID was reused: pid={pid}, expectedBirth={birth}, actualBirth={actual}.");
        Require(process.StartTime.ToUniversalTime().Ticks == birth && !process.HasExited, "Process changed during identity proof: " + pid);
        return new(pid, actual, System.IO.Path.GetFullPath(path!));
    }

    private static async Task<string> HashFile(string path, CancellationToken ct)
    {
        await using var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 64 * 1024, FileOptions.Asynchronous | FileOptions.SequentialScan);
        Require(file.Length <= 64 * 1024 * 1024, "Hash input exceeds 64 MiB: " + path);
        return Convert.ToHexString(await SHA256.HashDataAsync(file, ct));
    }
    private static string Absolute(string path)
    {
        Require(!string.IsNullOrWhiteSpace(path) && Path.IsPathFullyQualified(path) && path.IndexOfAny(['\0', '\r', '\n']) < 0, "A valid absolute path is required: " + path);
        return Path.GetFullPath(path);
    }
    private static bool Within(string path, string directory) => path.Equals(directory, StringComparison.OrdinalIgnoreCase) || path.StartsWith(Path.TrimEndingDirectorySeparator(directory) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    private static bool IsSha256(string value) => value is { Length: 64 } && value.All(char.IsAsciiHexDigit);
    private static string SingleLine(string value)
    {
        value = value.Trim();
        Require(value.Length <= 4096 && !value.Any(char.IsControl), "Expected one bounded property line.");
        return value;
    }
    private static string[] AbiList(string value, bool allowEmpty)
    {
        value = SingleLine(value);
        if (value.Length == 0) { Require(allowEmpty, "Primary ABI list is empty."); return []; }
        var parts = value.Split(',');
        Require(parts.All(part => part.Length is > 0 and <= 64 && part.All(character => char.IsAsciiLetterOrDigit(character) || character is '-' or '_' or '.')) && parts.Distinct(StringComparer.Ordinal).Count() == parts.Length, "Malformed ABI list: " + value);
        return parts;
    }
    private static bool PackagePresent(string value)
    {
        var present = false;
        foreach (var raw in value.Split('\n'))
        {
            var line = raw.Trim();
            if (line.Length == 0) continue;
            Require(line.StartsWith("package:", StringComparison.Ordinal) && line.Length <= 512 && line[8..].Split('.').All(part => part.Length > 0 && part.All(character => char.IsAsciiLetterOrDigit(character) || character == '_')),
                "Unexpected PackageManager output; absence is not established: " + line);
            if (string.Equals(line, "package:" + Package, StringComparison.Ordinal)) present = true;
        }
        return present;
    }
    private static void Require(bool condition, string message) { if (!condition) throw new InvalidDataException(message); }

    private static OwnedPackageProof PackageIdentity(string dump, string paths)
    {
        string Field(string pattern)
        {
            var values = Regex.Matches(dump, pattern, RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1)).Cast<Match>().Select(match => match.Groups[1].Value.Trim()).Distinct(StringComparer.Ordinal).ToArray();
            Require(values.Length == 1, "PackageManager did not return one unambiguous package identity field: " + pattern);
            return values[0];
        }
        var code = Field(@"(?m)^\s*versionCode=(\d+)(?:\s|$)");
        var name = Field(@"(?m)^\s*versionName=([^\r\n]+)");
        var codePath = Field(@"(?m)^\s*codePath=([^\r\n]+)");
        var lines = paths.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(line => line.Trim()).ToArray();
        Require(int.TryParse(code, NumberStyles.None, CultureInfo.InvariantCulture, out var versionCode) && versionCode == VersionCode && name == VersionName,
            "Installed package version does not exactly match the fixed APK.");
        Require(codePath.StartsWith("/data/app/", StringComparison.Ordinal) && !codePath.Any(char.IsWhiteSpace) && !codePath.Any(char.IsControl)
            && lines.Length == 1 && lines[0] == "package:" + codePath + "/base.apk", "PackageManager path does not prove one non-system base APK under the observed codePath.");
        return new(versionCode, name, codePath, lines[0][8..]);
    }

    private static GuestProcess[] PackageProcesses(string output)
    {
        var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(line => line.Trim()).ToArray();
        Require(lines.Length > 0 && lines[0].Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).SequenceEqual(["PID", "NAME"]), "Unexpected process table header; process absence is not established.");
        var result = new List<GuestProcess>();
        foreach (var line in lines.Skip(1))
        {
            var fields = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            Require(fields.Length == 2 && int.TryParse(fields[0], NumberStyles.None, CultureInfo.InvariantCulture, out var pid) && pid > 0, "Malformed process table row: " + line);
            if (fields[1] == Package || fields[1].StartsWith(Package + ":", StringComparison.Ordinal)) result.Add(new(int.Parse(fields[0], CultureInfo.InvariantCulture), fields[1]));
        }
        return result.ToArray();
    }

    private static bool IsResumed(string output) => Regex.IsMatch(output,
        @"(?m)^\s*(?:mResumedActivity|topResumedActivity|ResumedActivity)\s*[:=]\s*[^\r\n]*(?<![A-Za-z0-9_.])" + Regex.Escape(Package) + @"/[A-Za-z0-9_.$]+(?:\s|\})",
        RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));

    private sealed record OwnedPackageProof(int VersionCode, string VersionName, string CodePath, string PackageManagerPath);
    private sealed record GuestProcess(int Pid, string Name);
    private sealed record ProcessProof(int Pid, long StartedUtcTicks, string Path);

    // Keep the shared batch implementation and real backend, but guard the actual mutation call after its list reads.
    private sealed class LifecycleBackend(IDeviceBackend backend, string serial, string apkPath, Func<ApplicationOperation, Task> authorize,
        Func<object, Task> trace, CancellationToken budget, bool cleanupOnly = false) : IDeviceBackend
    {
        private void Target(string actual) => Require(actual == serial, "Lifecycle backend cannot address another serial.");
        private async Task<T> Call<T>(string operation, Func<Task<T>> action, bool rawTextExposed = false)
        {
            var watch = Stopwatch.StartNew();
            try
            {
                var result = await action();
                await trace(new { utc = DateTimeOffset.UtcNow, operation, durationMs = watch.ElapsedMilliseconds, result, rawTextExposed });
                return result;
            }
            catch (Exception ex) { await trace(new { utc = DateTimeOffset.UtcNow, operation, durationMs = watch.ElapsedMilliseconds, error = ex.ToString() }); throw; }
        }
        public Task<IReadOnlyList<AppInfo>> ListAppsAsync(string actual, bool showSystem = false, bool needVersion = true, CancellationToken ct = default)
        {
            Target(actual);
            Require(showSystem && needVersion, "Shared batch must retain its complete list/version behavior.");
            return Call("ListApps(true,true)", () => backend.ListAppsAsync(actual, showSystem, needVersion, budget));
        }
        public async Task InstallAsync(string actual, string path, CancellationToken ct = default)
        {
            Require(!cleanupOnly, "Owned cleanup cannot install.");
            Target(actual); Require(Path.GetFullPath(path) == apkPath, "Only the fixed leased APK path can be installed.");
            await authorize(ApplicationOperation.Install);
            await Call("Install", async () => { await backend.InstallAsync(actual, path, ct); return new { completed = true, stdoutNotExposedByExistingBackend = true }; });
        }
        public async Task UninstallAsync(string actual, string packageName, CancellationToken ct = default)
        {
            Target(actual); Require(packageName == Package, "Only the fixed owned package can be uninstalled.");
            await authorize(ApplicationOperation.Uninstall);
            await Call("Uninstall", async () => { await backend.UninstallAsync(actual, packageName, ct); return new { completed = true, stdoutNotExposedByExistingBackend = true }; });
        }
        public async Task<string> StartAppAsync(string actual, string packageName, string? activity = null, CancellationToken ct = default)
        {
            Require(!cleanupOnly, "Owned cleanup cannot start an application.");
            Target(actual); Require(packageName == Package && activity is null, "Only the owned package's backend-resolved launcher may start.");
            await authorize(ApplicationOperation.Start);
            return await Call("StartApp", () => backend.StartAppAsync(actual, packageName, activity, ct), rawTextExposed: true);
        }
        public async Task<string> StopAppAsync(string actual, string packageName, CancellationToken ct = default)
        {
            Target(actual); Require(packageName == Package, "Only the owned package may stop.");
            await authorize(ApplicationOperation.Stop);
            return await Call("StopApp", () => backend.StopAppAsync(actual, packageName, ct), rawTextExposed: true);
        }
        public Task<IReadOnlyList<DeviceInfo>> DiscoverAsync(CancellationToken ct = default) => throw new NotSupportedException();
        public Task<string> ShellAsync(string actual, string command, bool useRoot = false, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<byte[]> ScreenshotAsync(string actual, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<AndroidNode> DumpAsync(string actual, CancellationToken ct = default) => throw new NotSupportedException();
        public Task TapAsync(string actual, int x, int y, CancellationToken ct = default) => throw new NotSupportedException();
        public Task SwipeAsync(string actual, int x1, int y1, int x2, int y2, int durationMs, CancellationToken ct = default) => throw new NotSupportedException();
        public Task KeyEventAsync(string actual, int code, CancellationToken ct = default) => throw new NotSupportedException();
        public Task TextAsync(string actual, string text, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<bool> ClearAppAsync(string actual, string packageName, CancellationToken ct = default) => throw new NotSupportedException();
        public Task PushAsync(string actual, string localPath, string remotePath, IProgress<double>? progress = null, CancellationToken ct = default) => throw new NotSupportedException();
        public Task PullAsync(string actual, string remotePath, string localPath, IProgress<double>? progress = null, CancellationToken ct = default) => throw new NotSupportedException();
        public Task SetNodeTextAsync(string actual, NodeSelector selector, string text, CancellationToken ct = default) => throw new NotSupportedException();
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    private sealed class ReadOnlyLdRunner(ILdConsoleRunner runner, Func<object, Task> trace) : ILdConsoleRunner
    {
        public async Task<string> RunAsync(IReadOnlyList<string> arguments, CancellationToken ct = default)
        {
            Require(arguments.Count == 1 && arguments[0] == "list2", "Only list2 is permitted in application preflight.");
            var watch = Stopwatch.StartNew();
            try
            {
                var value = await runner.RunAsync(arguments, ct);
                await trace(new { utc = DateTimeOffset.UtcNow, command = "list2", stdout = value, exitCode = 0, durationMs = watch.ElapsedMilliseconds });
                Require(Encoding.UTF8.GetByteCount(value) <= MaximumReadBytes, "LD directory exceeds preflight output budget.");
                return value;
            }
            catch (Exception ex) { await trace(new { utc = DateTimeOffset.UtcNow, command = "list2", durationMs = watch.ElapsedMilliseconds, error = ex.ToString() }); throw; }
        }
    }

    private sealed record SourceLedger
    {
        public required string RunId { get; init; }
        public required string State { get; init; }
        public required Target Target { get; init; }
        public required DeviceInfo Device { get; init; }
        public required long LdGeneration { get; init; }
        public required string Package { get; init; }
        public required string ApkPath { get; init; }
        public required string ApkSha256 { get; init; }
        public required bool InstallAttempted { get; init; }
        public required bool Owned { get; init; }
        public required OwnedPackageProof OwnedPackage { get; init; }
        public required bool AutomaticCleanup { get; init; }
        public required bool ManualReviewRequired { get; init; }
        public required string Level { get; init; }
    }

    private sealed record Target
    {
        public required int SchemaVersion { get; init; }
        public required int Index { get; init; }
        public required string Name { get; init; }
        public required string Serial { get; init; }
        public required uint TransportId { get; init; }
        public required int PlayerPid { get; init; }
        public required long PlayerStartedUtcTicks { get; init; }
        public required int VBoxPid { get; init; }
        public required long VBoxStartedUtcTicks { get; init; }
        public required int ServerPid { get; init; }
        public required long ServerStartedUtcTicks { get; init; }
        public required string ServerPath { get; init; }
        public required string ServerSha256 { get; init; }
        public required string ConsolePath { get; init; }
        public required string ConsoleSha256 { get; init; }
    }
}
