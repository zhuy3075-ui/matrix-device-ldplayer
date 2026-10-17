using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    private static void ValidateFileTerminalPaths(string input, string output)
    {
        foreach (var path in new[] { input, output })
            for (var current = path; !string.IsNullOrEmpty(current); current = Path.GetDirectoryName(current))
                if ((File.Exists(current) || Directory.Exists(current)) && (File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0)
                    throw new ArgumentException("File/terminal acceptance rejects reparse paths: " + current);
    }

    private sealed partial class DualSyncRun
    {
        private sealed class IoFile(int index, string local, string remote, string nonce, byte[] bytes)
        {
            internal int Index { get; } = index;
            internal string Local { get; } = local;
            internal string Directory { get; } = remote;
            internal string Remote => Directory + "/payload.bin";
            internal string Marker => Directory + "/.matrix-owner";
            internal string Nonce { get; } = nonce;
            internal byte[] Bytes { get; } = bytes;
            internal string Sha256 { get; } = Convert.ToHexString(SHA256.HashData(bytes));
            internal bool ClaimAttempted, Claimed, WriteAttempted, RoundtripVerified, Cleaned;
        }

        private readonly List<IoFile> ioFiles = [];
        private readonly List<(string Name, Task Task)> ioCommands = [];
        private bool ioCleanupAuditTokenDisposed;

        private object FileTerminalSummary() => new
        {
            scope = "Original VM commands/queue and actual two-guest bytes; not GUI or parallel transfer execution.",
            noLaunch = true, noUiActions = true, cleanupAuditTokenDisposed = ioCleanupAuditTokenDisposed,
            files = ioFiles.Select(file => new { file.Index, serial = Target(file.Index).Serial, file.Local, file.Directory, file.Remote,
                file.Marker, file.Nonce, bytes = file.Bytes.Length, file.Sha256, file.ClaimAttempted, file.Claimed,
                file.WriteAttempted, file.RoundtripVerified, file.Cleaned, residual = file.ClaimAttempted && !file.Cleaned }).ToArray(),
            originalCommands = ioCommands.Select(item => new { item.Name, status = item.Task.Status.ToString(), terminal = item.Task.IsCompleted }).ToArray(),
            notVerified = new[] { "physical GUI/DPI", "native picker/directory window", "text input", "APK install/uninstall",
                "actual API in-flight conflict", "reconnect handoff", "arbitrary I/O interleavings", "final dual 30 minutes" }
        };

        private Task IoCommand(string name, Func<Task> command)
        {
            var task = command();
            owned.Add(task); ioCommands.Add((name, task)); return task;
        }

        private async Task IoPhase(string name, Func<Task<object>> action)
        {
            started.Add(name);
            await Identity(name + "-before", Ct);
            await AfterGuard(() => Step(name, action), ct => Identity(name + "-after", ct));
            completed.Add(name);
        }

        private void IoSelection(int selected, int current)
        {
            vm!.CurrentDevice = Row(current);
            foreach (var row in vm.Devices) row.IsSelected = row.Serial == Target(selected).Serial;
            vm.UpdateSelection();
            Need(vm.CurrentDevice.Serial == Target(current).Serial && vm.Devices.Count(row => row.IsSelected) == 1
                && Row(selected).IsSelected, "Original Current/checked setup did not hold.", "PRODUCT-FAIL");
        }

        private async Task RunFileTerminal()
        {
            Need(vm is not null && !vm.SyncMouse && !vm.SyncKeyboard && !vm.Busy && !vm.Transferring && vm.Transfers.Count == 0,
                "File/terminal plan requires idle original VM, empty queue and disabled synchronization.");
            await IoPhase("sources", async () =>
            {
                var directory = "/sdcard/Download/matrix-io-" + Guid.NewGuid().ToString("N");
                var nonce = Guid.NewGuid().ToString("N");
                foreach (var index in new[] { 1, 2 })
                {
                    var path = Path.Combine(output, "sources", index.ToString(), "payload.bin");
                    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                    var bytes = Enumerable.Range(0, index == 1 ? 32769 : 65537).Select(value => (byte)((value * 73 + index * 41) % 251)).ToArray();
                    await using (var file = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None)) await file.WriteAsync(bytes, Ct);
                    var source = new IoFile(index, path, directory, nonce + "-" + index, bytes);
                    Need(DualSyncHash(path) == source.Sha256, "Actual local source differs from planned bytes."); ioFiles.Add(source);
                }
                Need(ioFiles[0].Sha256 != ioFiles[1].Sha256 && ioFiles[0].Bytes.Length != ioFiles[1].Bytes.Length, "Sources must distinguish cross-target contamination.");
                return FileTerminalSummary();
            });
            foreach (var file in ioFiles)
            {
                await IoPhase("claim-" + file.Index, async () =>
                {
                    using var write = runtime!.Writes.Acquire([Target(file.Index).Serial]);
                    file.ClaimAttempted = true;
                    var command = $"[ -d '/sdcard/Download' ] && [ ! -e '{file.Directory}' ] && [ ! -L '{file.Directory}' ] && mkdir '{file.Directory}' && printf '%s' '{file.Nonce}' > '{file.Marker}' && [ \"$(cat '{file.Marker}')\" = '{file.Nonce}' ] && printf '%s' 'CLAIMED'";
                    var task = runtime.Backend.ShellAsync(Target(file.Index).Serial, command, ct: Ct); owned.Add(task);
                    var text = await task;
                    Need(text == "CLAIMED", "Exclusive directory claim did not complete."); file.Claimed = true;
                    return new { file.Index, file.Directory, file.Marker, file.Nonce, command, text, file.Claimed };
                });
            }
            foreach (var file in ioFiles)
            {
                var marker = "MATRIX_TERMINAL_" + file.Nonce;
                await IoShell(file.Index, "terminal-" + file.Index, $"printf '%s\\n' '{marker}'; getprop ro.product.model",
                    text => text.Replace("\r\n", "\n").Split('\n') is { Length: 2 } lines && lines[0] == marker && !string.IsNullOrWhiteSpace(lines[1]));
            }
            await IoPhase("upload", async () =>
            {
                foreach (var file in ioFiles)
                {
                    Need(file.Claimed && DualSyncHash(file.Local) == file.Sha256, "Owned source changed before enqueue.");
                    IoSelection(file.Index, 3 - file.Index); vm!.RemotePath = file.Directory;
                    var count = vm.Transfers.Count; vm.QueuePush(file.Local, false);
                    Need(vm.Transfers.Count == count + 1, "QueuePush must enqueue exactly the selected target.", "PRODUCT-FAIL");
                    var row = vm.Transfers[^1];
                    Need(row.Serial == Target(file.Index).Serial && row.LocalPath == file.Local && row.RemotePath == file.Remote
                        && row.Direction == "上传" && !row.AutoInstall, "Original upload row target/path drift.", "PRODUCT-FAIL");
                }
                Need(vm!.Transfers.Count == 2 && vm.StartTransfersCommand.CanExecute(null), "Original upload command was not available.", "PRODUCT-FAIL");
                foreach (var file in ioFiles) file.WriteAttempted = true;
                await IoCommand("original-upload", () => vm.StartTransfersCommand.ExecuteAsync());
                VerifyIoQueue(2); return IoQueue();
            });
            await IoPhase("download", async () =>
            {
                foreach (var file in ioFiles)
                {
                    IoSelection(3 - file.Index, file.Index); vm!.RemotePath = file.Remote;
                    var local = Path.Combine(output, "returned", file.Index.ToString(), "payload.bin");
                    Directory.CreateDirectory(Path.GetDirectoryName(local)!); Need(!File.Exists(local), "Download target must be fresh.");
                    var count = vm.Transfers.Count; vm.QueuePull(local);
                    Need(vm.Transfers.Count == count + 1, "QueuePull must enqueue exactly Current.", "PRODUCT-FAIL");
                    var row = vm.Transfers[^1];
                    Need(row.Serial == Target(file.Index).Serial && row.LocalPath == local && row.RemotePath == file.Remote
                        && row.Direction == "下载" && !row.AutoInstall, "Original download row target/path drift.", "PRODUCT-FAIL");
                }
                Need(vm!.Transfers.Count == 4 && vm.StartTransfersCommand.CanExecute(null), "Original download command was not available.", "PRODUCT-FAIL");
                await IoCommand("original-download", () => vm.StartTransfersCommand.ExecuteAsync()); VerifyIoQueue(4);
                foreach (var file in ioFiles)
                {
                    var local = vm.Transfers.Single(row => row.Serial == Target(file.Index).Serial && row.Direction == "下载").LocalPath;
                    var bytes = await File.ReadAllBytesAsync(local, Ct);
                    Need(bytes.AsSpan().SequenceEqual(file.Bytes) && DualSyncHash(local) == file.Sha256
                        && DualSyncHash(local) != ioFiles.Single(other => other.Index != file.Index).Sha256, "Actual roundtrip or target isolation mismatch.", "PRODUCT-FAIL");
                    file.RoundtripVerified = true;
                }
                return new { queue = IoQueue(), effects = FileTerminalSummary() };
            });
            foreach (var file in ioFiles)
            {
                var command = $"wc -c < '{file.Remote}'; sha256sum '{file.Remote}'";
                await IoShell(file.Index, "file-shell-" + file.Index, command, text =>
                {
                    var lines = text.Replace("\r\n", "\n").Split('\n');
                    return lines.Length == 2 && lines[0].Trim() == file.Bytes.Length.ToString(System.Globalization.CultureInfo.InvariantCulture)
                        && Regex.IsMatch(lines[1], "^" + file.Sha256.ToLowerInvariant() + @"[ \t]+" + Regex.Escape(file.Remote) + "$", RegexOptions.CultureInvariant);
                });
            }
            await Save("file-terminal-results.json", new { effects = FileTerminalSummary(), queue = IoQueue(), history = vm!.ShellHistory.ToArray() });
        }

        private async Task IoShell(int index, string name, string command, Func<string, bool> effect)
        {
            await IoPhase(name, async () =>
            {
                IoSelection(3 - index, index); var count = vm!.ShellHistory.Count; vm.ShellInput = command;
                Need(vm.ShellCommand.CanExecute(null), "Original ShellCommand is disabled.", "PRODUCT-FAIL");
                await IoCommand(name, () => vm.ShellCommand.ExecuteAsync());
                Need(!vm.Busy && vm.Error.Length == 0 && vm.ShellHistory.Count == count + 1 && vm.ShellInput.Length == 0,
                    "Original terminal did not finish one successful request: " + vm.Error, "PRODUCT-FAIL");
                var entry = vm.ShellHistory[0];
                Need(entry.Serial == Target(index).Serial && entry.Command == command && effect(entry.Output), "Actual terminal source/command/output mismatch.", "PRODUCT-FAIL");
                return new { index, current = vm.CurrentDevice?.Serial, selected = vm.Devices.Where(row => row.IsSelected).Select(row => row.Serial).ToArray(), entry, vm.Status, vm.Error };
            });
        }

        private void VerifyIoQueue(int count) => Need(vm!.Transfers.Count == count && !vm.Transferring && !vm.Busy && vm.Error.Length == 0
            && vm.Transfers.All(row => row.State == "传输完成" && row.Progress == 100 && !row.AutoInstall), "Original transfer failed or remained pending: " + vm.Error, "PRODUCT-FAIL");

        private object IoQueue() => vm!.Transfers.Select(row => new { row.Serial, row.Direction, row.LocalPath, row.RemotePath, row.State, row.Progress, row.Size, row.AutoInstall }).ToArray();

        private async Task CleanupFileTerminal()
        {
            using var cleanup = new CancellationTokenSource(TimeSpan.FromSeconds(15)); var ct = cleanup.Token;
            Need(ioCommands.All(item => item.Task.IsCompleted) && !owned.Any(task => !task.IsCompleted), "Original operations must be terminal before remote cleanup.");
            foreach (var file in ioFiles.Where(file => file.ClaimAttempted && !file.Cleaned))
            {
                await Identity("cleanup-" + file.Index + "-before", ct, requireCleanOperation: false);
                using var write = runtime!.Writes.Acquire([Target(file.Index).Serial]);
                async Task<string> Shell(string command)
                {
                    await Identity("cleanup-" + file.Index + "-shell-before", ct, requireCleanOperation: false);
                    return await AfterGuard(async () =>
                    {
                        var task = runtime.Backend.ShellAsync(Target(file.Index).Serial, command, ct: ct); owned.Add(task);
                        var result = await task; await Trace("owned-file-cleanup-shell", new { file.Index, command, result }); return result;
                    }, token => Identity("cleanup-" + file.Index + "-shell-after", token, requireCleanOperation: false));
                }
                var absent = await Shell($"if [ ! -e '{file.Directory}' ] && [ ! -L '{file.Directory}' ]; then printf ABSENT; else printf PRESENT; fi");
                if (absent == "ABSENT") { file.Cleaned = true; continue; }
                Need(absent == "PRESENT", "Ambiguous owned directory presence.");
                Need(await Shell($"[ -d '{file.Directory}' ] && [ ! -L '{file.Directory}' ] && [ -f '{file.Marker}' ] && [ ! -L '{file.Marker}' ] && [ \"$(cat '{file.Marker}')\" = '{file.Nonce}' ] && printf OWNED") == "OWNED", "Owned directory nonce/type changed; preserve residual.");
                var entries = await Shell($"find '{file.Directory}' -mindepth 1 -maxdepth 1 -print0");
                Need(entries.EndsWith('\0'), "Directory entry listing must be NUL-delimited.");
                var names = entries.Split('\0')[..^1];
                Need(names.Length is 1 or 2 && names.Distinct(StringComparer.Ordinal).Count() == names.Length && names.Contains(file.Marker, StringComparer.Ordinal)
                    && names.All(name => name == file.Marker || name == file.Remote), "Unknown entry in owned directory; preserve all residuals.");
                if (names.Contains(file.Remote, StringComparer.Ordinal))
                {
                    Need(file.WriteAttempted, "Payload was not created by this run; preserve residual.");
                    Need(await Shell($"[ -f '{file.Remote}' ] && [ ! -L '{file.Remote}' ] && printf FILE") == "FILE", "Payload type changed.");
                    var local = Path.Combine(output, "cleanup-readback", file.Index + ".bin"); Directory.CreateDirectory(Path.GetDirectoryName(local)!);
                    var pull = runtime.Backend.PullAsync(Target(file.Index).Serial, file.Remote, local, ct: ct); owned.Add(pull); await pull;
                    Need(new FileInfo(local).Length == file.Bytes.Length && DualSyncHash(local) == file.Sha256, "Cleanup payload changed or is partial; preserve residual.");
                    await Identity("cleanup-" + file.Index + "-before-unlink", ct, requireCleanOperation: false);
                    await Shell($"rm '{file.Remote}'");
                }
                await Shell($"rm '{file.Marker}' && rmdir '{file.Directory}'");
                Need(await Shell($"if [ ! -e '{file.Directory}' ] && [ ! -L '{file.Directory}' ]; then printf ABSENT; else printf PRESENT; fi") == "ABSENT", "Owned directory remains after exact cleanup.");
                file.Cleaned = true;
                await Identity("cleanup-" + file.Index + "-after", ct, requireCleanOperation: false);
            }
            Need(ioFiles.All(file => !file.ClaimAttempted || file.Cleaned), "Run-owned residual path remains.");
            await Save("file-terminal-cleanup.json", FileTerminalSummary());
        }
    }
}
