using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using MatrixDevice.Core;

namespace MatrixDevice.Desktop;

public sealed class LdWindowVisibility
{
    private const int SwHide = 0;
    private const int SwShowMinNoActive = 7;
    private const int SwShowNoActivate = 8;
    private const string MarkerPrefix = "MatrixDevice.LdVisibility.";
    private static readonly TimeSpan VisibilityTimeout = TimeSpan.FromSeconds(3);
    private readonly string journalDirectory;
    private readonly string installDirectory;
    private readonly SemaphoreSlim operations = new(1, 1);
    private readonly object gate = new();
    private readonly Dictionary<int, RecoveryRecord> owned = [];
    private readonly int controllerPid;
    private readonly long controllerBirth;

    public LdWindowVisibility(string journalDirectory, string expectedInstallDirectory)
    {
        if (!Path.IsPathFullyQualified(journalDirectory) || !Path.IsPathFullyQualified(expectedInstallDirectory) || !Directory.Exists(expectedInstallDirectory))
            throw new ArgumentException("窗口恢复目录与雷电安装目录必须为绝对路径，且安装目录必须存在。");
        installDirectory = ResolveExistingPath(expectedInstallDirectory);
        this.journalDirectory = Path.GetFullPath(Path.Combine(journalDirectory, "ld-window-visibility"));
        Directory.CreateDirectory(this.journalDirectory);
        this.journalDirectory = ResolveExistingPath(this.journalDirectory);
        using var process = Process.GetCurrentProcess();
        controllerPid = process.Id;
        controllerBirth = process.StartTime.ToUniversalTime().Ticks;
    }

    public async Task<bool> HideAsync(LdPlayerInstance instance, long expectedPlayerBirth, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ValidateIndex(instance.Index);
        if (instance.ProcessId <= 0 || instance.TopWindow <= 0 || expectedPlayerBirth <= 0)
            throw new DeviceException("隐藏窗口需要真实顶层窗口、播放器 PID 与已冻结的进程出生时刻。", 400, 1007);
        await operations.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            var window = new IntPtr(instance.TopWindow);
            var executable = ValidateTarget(window, instance.ProcessId, expectedPlayerBirth, null);
            RecoveryRecord? existing;
            lock (gate) owned.TryGetValue(instance.Index, out existing);
            if (existing is not null)
            {
                if (existing.Window != instance.TopWindow || existing.ProcessId != instance.ProcessId || existing.ProcessStartUtcTicks != expectedPlayerBirth)
                    throw new DeviceException($"实例 {instance.Index} 的窗口或进程已变化，不能复用旧隐藏记录。", 409, 1003);
                ValidateRecordOwner(existing);
                await ChangeVisibilityAsync(existing, false, ct).ConfigureAwait(false);
                return true;
            }
            if (!IsWindowVisible(window)) return false;
            if (Directory.EnumerateFiles(journalDirectory, $"ld-window-{instance.Index}-*.json").Any())
                throw new DeviceException($"实例 {instance.Index} 存在未处理的恢复记录，请先恢复或检查记录，未隐藏新窗口。", 409, 1003);
            var id = Guid.NewGuid().ToString("N");
            var record = new RecoveryRecord(1, id, instance.Index, instance.TopWindow, instance.ProcessId,
                expectedPlayerBirth, executable, true, IsIconic(window), MarkerPrefix + id, controllerPid, controllerBirth);
            var markerInstalled = false;
            try
            {
                // The marker disappears with the HWND, including reuse inside the same process.
                if (!SetProp(window, record.Marker, new IntPtr(1))) throw new Win32Exception(Marshal.GetLastWin32Error(), "无法标记自有隐藏窗口，未执行隐藏");
                markerInstalled = true;
                ValidateRecordOwner(record);
                await WriteRecordAsync(record, ct).ConfigureAwait(false);
            }
            catch
            {
                if (markerInstalled && IsWindow(window) && GetProp(window, record.Marker) == new IntPtr(1)) RemoveProp(window, record.Marker);
                throw;
            }
            lock (gate) owned.Add(instance.Index, record);
            await ChangeVisibilityAsync(record, false, ct).ConfigureAwait(false);
            return true;
        }
        finally { operations.Release(); }
    }

    public bool IsHidden(int index)
    {
        RecoveryRecord? record;
        lock (gate) owned.TryGetValue(index, out record);
        if (record is null) return false;
        var window = new IntPtr(record.Window);
        return IsWindow(window) && GetProp(window, record.Marker) == new IntPtr(1) &&
            GetWindowThreadProcessId(window, out var pid) != 0 && pid == record.ProcessId && !IsWindowVisible(window);
    }

    public async Task ShowAsync(int index, CancellationToken ct = default)
    {
        ValidateIndex(index);
        await operations.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            RecoveryRecord? record;
            lock (gate) owned.TryGetValue(index, out record);
            if (record is not null) await RestoreAsync(record, RecordPath(record), ct).ConfigureAwait(false);
        }
        finally { operations.Release(); }
    }

    public async Task RestoreAllAsync(CancellationToken ct = default)
    {
        await operations.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            RecoveryRecord[] records;
            lock (gate) records = owned.Values.ToArray();
            var failures = new List<Exception>();
            foreach (var record in records)
            {
                ct.ThrowIfCancellationRequested();
                try { await RestoreAsync(record, RecordPath(record), ct).ConfigureAwait(false); }
                catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
                catch (Exception error) { failures.Add(error); }
            }
            if (failures.Count > 0) throw new AggregateException("部分窗口未能安全恢复，已保留对应恢复记录。", failures);
        }
        finally { operations.Release(); }
    }

    public async Task<IReadOnlyList<string>> RecoverPendingAsync(CancellationToken ct = default)
    {
        await operations.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            var errors = new List<string>();
            foreach (var path in Directory.EnumerateFiles(journalDirectory, "ld-window-*.json").Order(StringComparer.Ordinal))
            {
                ct.ThrowIfCancellationRequested();
                try
                {
                    if ((File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0 || new FileInfo(path).Length > 64 * 1024)
                        throw new InvalidDataException("恢复记录不能是链接或超过 64 KiB。");
                    var record = JsonSerializer.Deserialize<RecoveryRecord>(await File.ReadAllTextAsync(path, ct).ConfigureAwait(false), JsonDefaults.Options)
                        ?? throw new InvalidDataException("恢复记录为空。");
                    ValidateRecord(record, path);
                    if (record.ControllerPid != controllerPid && IsSameProcessAlive(record.ControllerPid, record.ControllerStartUtcTicks))
                        throw new InvalidOperationException("原控制程序仍在运行，未接管其隐藏窗口。");
                    await RestoreAsync(record, path, ct).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
                catch (Exception error) { errors.Add($"{Path.GetFileName(path)}：{error.Message}"); }
            }
            return errors.AsReadOnly();
        }
        finally { operations.Release(); }
    }

    private async Task RestoreAsync(RecoveryRecord record, string path, CancellationToken ct)
    {
        if (!File.Exists(path)) { Forget(record); return; }
        ValidateRecord(record, path);
        if (HasExited(record.ProcessId) || !IsWindow(new IntPtr(record.Window)))
        {
            DeleteRecord(path); Forget(record); return;
        }
        ValidateRecordOwner(record);
        await ChangeVisibilityAsync(record, record.WasVisible, ct).ConfigureAwait(false);
        DeleteRecord(path);
        if (IsWindow(new IntPtr(record.Window)) && GetProp(new IntPtr(record.Window), record.Marker) == new IntPtr(1)) RemoveProp(new IntPtr(record.Window), record.Marker);
        Forget(record);
    }

    private async Task ChangeVisibilityAsync(RecoveryRecord record, bool visible, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ValidateRecordOwner(record);
        var window = new IntPtr(record.Window);
        // ShowWindowAsync reports queueing, not the resulting visibility.
        ShowWindowAsync(window, visible ? record.WasMinimized ? SwShowMinNoActive : SwShowNoActivate : SwHide);
        var deadline = Stopwatch.StartNew();
        while (true)
        {
            ct.ThrowIfCancellationRequested();
            ValidateRecordOwner(record);
            if (IsWindowVisible(window) == visible) return;
            if (deadline.Elapsed >= VisibilityTimeout) throw new TimeoutException($"窗口可见状态未在期限内变为 {visible}，恢复记录已保留：实例 {record.Index}");
            await Task.Delay(25, ct).ConfigureAwait(false);
        }
    }

    private string ValidateTarget(IntPtr window, int processId, long birth, string? expectedExecutable)
    {
        if (!IsWindow(window)) throw new DeviceException("目标窗口已经失效，未修改窗口。", 409, 1003);
        if (GetWindowThreadProcessId(window, out var actualPid) == 0 || actualPid != processId)
            throw new DeviceException("窗口所属 PID 不匹配，拒绝操作复用窗口。", 409, 1003);
        try
        {
            using var process = Process.GetProcessById(processId);
            if (process.HasExited || process.StartTime.ToUniversalTime().Ticks != birth)
                throw new DeviceException("播放器进程出生时刻不匹配，拒绝操作复用 PID。", 409, 1003);
            var executable = ResolveExistingPath(process.MainModule?.FileName ?? throw new InvalidOperationException("无法读取目标进程路径。"));
            var relative = Path.GetRelativePath(installDirectory, executable);
            if (Path.IsPathFullyQualified(relative) || relative == ".." || relative.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal))
                throw new DeviceException("目标进程不属于冻结的模拟器安装目录，未修改窗口。", 403, 1007);
            if (expectedExecutable is not null && !executable.Equals(expectedExecutable, StringComparison.OrdinalIgnoreCase))
                throw new DeviceException("目标可执行文件路径已变化，拒绝修改窗口。", 409, 1003);
            return executable;
        }
        catch (ArgumentException) { throw new DeviceException("目标进程已退出，未修改窗口。", 409, 1003); }
    }

    private void ValidateRecordOwner(RecoveryRecord record)
    {
        ValidateTarget(new IntPtr(record.Window), record.ProcessId, record.ProcessStartUtcTicks, record.ExecutablePath);
        if (GetProp(new IntPtr(record.Window), record.Marker) != new IntPtr(1))
            throw new DeviceException("窗口标记不匹配，可能已销毁或复用，拒绝恢复其它窗口。", 409, 1003);
    }

    private void ValidateRecord(RecoveryRecord record, string path)
    {
        ValidateIndex(record.Index);
        if (record.Version != 1 || !Guid.TryParseExact(record.Id, "N", out _) || record.Window <= 0 || record.ProcessId <= 0 ||
            record.ProcessStartUtcTicks <= 0 || !record.WasVisible || record.Marker != MarkerPrefix + record.Id ||
            record.ControllerPid <= 0 || record.ControllerStartUtcTicks <= 0 || !Path.IsPathFullyQualified(record.ExecutablePath) ||
            !Path.GetFullPath(path).Equals(RecordPath(record), StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("恢复记录字段或文件名不合法，未修改窗口。");
    }

    private async Task WriteRecordAsync(RecoveryRecord record, CancellationToken ct)
    {
        var path = RecordPath(record);
        var temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            var bytes = new UTF8Encoding(false).GetBytes(JsonSerializer.Serialize(record, JsonDefaults.Options));
            await using (var file = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, true))
            {
                await file.WriteAsync(bytes, ct).ConfigureAwait(false);
                await file.FlushAsync(ct).ConfigureAwait(false);
                file.Flush(true);
            }
            ct.ThrowIfCancellationRequested();
            File.Move(temporary, path, false);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }

    private string RecordPath(RecoveryRecord record) => Path.Combine(journalDirectory, $"ld-window-{record.Index}-{record.Id}.json");
    private void DeleteRecord(string path)
    {
        if (!Path.GetDirectoryName(Path.GetFullPath(path))!.Equals(journalDirectory, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("恢复记录不在私有目录内，拒绝清理。");
        File.Delete(path);
    }
    private void Forget(RecoveryRecord record)
    {
        lock (gate)
            if (owned.TryGetValue(record.Index, out var current) && current.Id == record.Id) owned.Remove(record.Index);
    }
    private static void ValidateIndex(int index)
    {
        if (index <= 0 || index == 99999) throw new DeviceException($"窗口隐藏只允许非零独立实例：{index}", 400, 1007);
    }
    private static bool HasExited(int pid)
    {
        try { using var process = Process.GetProcessById(pid); return process.HasExited; }
        catch (ArgumentException) { return true; }
    }
    private static bool IsSameProcessAlive(int pid, long birth)
    {
        try { using var process = Process.GetProcessById(pid); return !process.HasExited && process.StartTime.ToUniversalTime().Ticks == birth; }
        catch (ArgumentException) { return false; }
    }
    private static string ResolveExistingPath(string path)
    {
        var absolute = Path.GetFullPath(path);
        var current = Path.GetPathRoot(absolute)!;
        foreach (var part in absolute[current.Length..].Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries))
        {
            var next = Path.Combine(current, part);
            var attributes = File.GetAttributes(next);
            if ((attributes & FileAttributes.ReparsePoint) != 0)
            {
                FileSystemInfo info = (attributes & FileAttributes.Directory) != 0 ? new DirectoryInfo(next) : new FileInfo(next);
                current = info.ResolveLinkTarget(true)?.FullName ?? throw new IOException("无法解析路径中的链接，拒绝修改窗口。");
            }
            else current = next;
        }
        return Path.TrimEndingDirectorySeparator(current);
    }

    private sealed record RecoveryRecord(int Version, string Id, int Index, long Window, int ProcessId,
        long ProcessStartUtcTicks, string ExecutablePath, bool WasVisible, bool WasMinimized, string Marker,
        int ControllerPid, long ControllerStartUtcTicks);
    [DllImport("user32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool ShowWindowAsync(IntPtr window, int command);
    [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool IsWindow(IntPtr window);
    [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool IsWindowVisible(IntPtr window);
    [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool IsIconic(IntPtr window);
    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr window, out int pid);
    [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "SetPropW", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool SetProp(IntPtr window, string name, IntPtr value);
    [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetPropW")] private static extern IntPtr GetProp(IntPtr window, string name);
    [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "RemovePropW")] private static extern IntPtr RemoveProp(IntPtr window, string name);
}
