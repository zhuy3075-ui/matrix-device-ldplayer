using System.Formats.Tar;
using System.Globalization;
using System.IO.Compression;
using System.Text.Json;
using MatrixDevice.Core;

namespace MatrixDevice.Features;

public sealed record AppBackupRecord(Guid Id, string Serial, string PackageName, string ArchivePath, DateTimeOffset CreatedUtc, string? Notes = null);
public sealed record BackupProgress(double Percent, string Stage);
public sealed record AppBackupOptions
{
    public bool RequireRoot { get; init; } = true;
    public bool ValidateImportedArchive { get; init; } = true;
    public bool RestoreGoogleAccountManagerData { get; init; } = true;
    public string ArchiveExtension { get; init; } = ".tar.gz";
}

public sealed class ApplicationBackupService(IDeviceBackend backend, string storageDirectory)
{
    private readonly string directory = Path.GetFullPath(storageDirectory);
    private readonly SemaphoreSlim operationGate = new(1, 1);
    public const long MaximumArchiveBytes = 1024L * 1024 * 1024;
    public const long MaximumUncompressedBytes = 4L * 1024 * 1024 * 1024;

    public async Task<AppBackupRecord> BackupAsync(string serial, string packageName, IProgress<BackupProgress>? progress = null, CancellationToken ct = default)
    {
        ScriptPackageService.ValidatePackageName(packageName);
        await operationGate.WaitAsync(ct).ConfigureAwait(false);
        var id = Guid.NewGuid();
        var remote = "/data/local/tmp/matrix-backup-" + id.ToString("N") + ".tar.gz";
        var local = Path.Combine(directory, id.ToString("N") + ".tar.gz");
        try
        {
            Directory.CreateDirectory(directory);
            progress?.Report(new(0, "Checking root and application paths"));
            await EnsureRootAsync(serial, ct).ConfigureAwait(false);
            var privatePath = await ResolvePrivatePathAsync(serial, packageName, ct).ConfigureAwait(false);
            await backend.ShellAsync(serial, "command -v tar", true, ct).ConfigureAwait(false);
            progress?.Report(new(20, "Creating device archive"));
            await backend.ShellAsync(serial, $"tar -czf {Quote(remote)} -C / {Quote(privatePath.TrimStart('/'))}", true, ct).ConfigureAwait(false);
            await backend.ShellAsync(serial, $"chmod 0644 {Quote(remote)} && test -s {Quote(remote)}", true, ct).ConfigureAwait(false);
            progress?.Report(new(50, "Downloading archive"));
            await backend.PullAsync(serial, remote, local, ct: ct).ConfigureAwait(false);
            ValidateImportedArchive(local, packageName);
            var record = new AppBackupRecord(id, serial, packageName, local, DateTimeOffset.UtcNow);
            SaveRecord(record);
            progress?.Report(new(100, "Backup completed"));
            return record;
        }
        catch
        {
            if (File.Exists(local)) File.Delete(local);
            throw;
        }
        finally
        {
            try { await backend.ShellAsync(serial, $"rm -f -- {Quote(remote)}", true, CancellationToken.None).ConfigureAwait(false); }
            catch (Exception error) { System.Diagnostics.Trace.WriteLine($"Backup remote cleanup failed: {error.Message}"); }
            operationGate.Release();
        }
    }

    public async Task RestoreAsync(string serial, AppBackupRecord record, bool confirmed, AppBackupOptions? options = null,
        IProgress<BackupProgress>? progress = null, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(record);
        ScriptPackageService.ValidatePackageName(record.PackageName);
        if (!confirmed) throw new DeviceException($"Restore clears '{record.PackageName}' on device '{serial}'; explicit confirmation is required.", 409, 1003);
        options ??= new AppBackupOptions();
        if (options.ArchiveExtension != ".tar.gz") throw new DeviceException($"Unsupported backup extension: '{options.ArchiveExtension}'.", 501, 2001);
        if (options.RestoreGoogleAccountManagerData)
            throw new DeviceException("Account-manager archive semantics were not recovered; set RestoreGoogleAccountManagerData=false for application-only restore.", 501, 2001);
        ValidateImportedArchive(record.ArchivePath, record.PackageName);
        await operationGate.WaitAsync(ct).ConfigureAwait(false);
        var remote = "/data/local/tmp/matrix-restore-" + Guid.NewGuid().ToString("N") + ".tar.gz";
        try
        {
            progress?.Report(new(0, "Checking restore prerequisites"));
            await EnsureRootAsync(serial, ct).ConfigureAwait(false);
            var privatePath = await ResolvePrivatePathAsync(serial, record.PackageName, ct).ConfigureAwait(false);
            var uidText = (await backend.ShellAsync(serial, $"stat -c %u {Quote(privatePath)}", true, ct).ConfigureAwait(false)).Trim();
            if (!int.TryParse(uidText, NumberStyles.None, CultureInfo.InvariantCulture, out var uid) || uid <= 0)
                throw new DeviceException($"Cannot resolve application owner UID: '{uidText}'.", 409, 1003);
            await backend.ShellAsync(serial, "command -v tar && command -v restorecon && command -v chown", true, ct).ConfigureAwait(false);
            await backend.PushAsync(serial, record.ArchivePath, remote, ct: ct).ConfigureAwait(false);
            progress?.Report(new(30, "Stopping and clearing application"));
            await backend.StopAppAsync(serial, record.PackageName, ct).ConfigureAwait(false);
            if (!await backend.ClearAppAsync(serial, record.PackageName, ct).ConfigureAwait(false))
                throw new DeviceException($"Clear data failed for '{record.PackageName}'; archive was not extracted.", 409, 1003);
            progress?.Report(new(60, "Extracting archive"));
            await backend.ShellAsync(serial, $"tar -xzf {Quote(remote)} -C /", true, ct).ConfigureAwait(false);
            await backend.ShellAsync(serial, $"chown -R {uid}:{uid} {Quote(privatePath)} && restorecon -RF {Quote(privatePath)}", true, ct).ConfigureAwait(false);
            progress?.Report(new(100, "Restore completed"));
        }
        finally
        {
            try { await backend.ShellAsync(serial, $"rm -f -- {Quote(remote)}", true, CancellationToken.None).ConfigureAwait(false); }
            catch (Exception error) { System.Diagnostics.Trace.WriteLine($"Restore remote cleanup failed: {error.Message}"); }
            operationGate.Release();
        }
    }

    public AppBackupRecord Import(string path, string packageName, string? notes = null)
    {
        ValidateImportedArchive(path, packageName);
        Directory.CreateDirectory(directory);
        var id = Guid.NewGuid();
        var destination = Path.Combine(directory, id.ToString("N") + ".tar.gz");
        File.Copy(path, destination);
        var record = new AppBackupRecord(id, "", packageName, destination, DateTimeOffset.UtcNow, notes);
        try { SaveRecord(record); }
        catch { File.Delete(destination); throw; }
        return record;
    }

    public IReadOnlyList<AppBackupRecord> List()
    {
        if (!Directory.Exists(directory)) return [];
        return Directory.EnumerateFiles(directory, "*.json", SearchOption.TopDirectoryOnly)
            .Select(path => JsonSerializer.Deserialize<AppBackupRecord>(File.ReadAllText(path), JsonDefaults.Options)
                ?? throw new InvalidDataException($"Null backup metadata: {path}"))
            .OrderByDescending(record => record.CreatedUtc).ToArray();
    }

    public void Export(AppBackupRecord record, string destination)
    {
        ArgumentNullException.ThrowIfNull(record);
        ValidateImportedArchive(record.ArchivePath, record.PackageName);
        var path = Path.GetFullPath(destination);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.Copy(record.ArchivePath, path, false);
    }

    public void Delete(AppBackupRecord record, bool confirmed)
    {
        ArgumentNullException.ThrowIfNull(record);
        if (!confirmed) throw new DeviceException("Backup deletion requires confirmation.", 409, 1003);
        var expected = Path.Combine(directory, record.Id.ToString("N") + ".tar.gz");
        if (!Path.GetFullPath(record.ArchivePath).Equals(expected, StringComparison.OrdinalIgnoreCase))
            throw new DeviceException("Only a managed backup archive can be deleted.", 400, 1007);
        File.Delete(expected);
        File.Delete(Path.Combine(directory, record.Id.ToString("N") + ".json"));
    }

    public static void ValidateImportedArchive(string path, string packageName)
    {
        ScriptPackageService.ValidatePackageName(packageName);
        using var file = File.OpenRead(path);
        if (file.Length is <= 0 or > MaximumArchiveBytes) throw Invalid($"Archive size is outside 1..{MaximumArchiveBytes}: {file.Length}.");
        using var gzip = new GZipStream(file, CompressionMode.Decompress);
        using var bounded = new LimitedReadStream(gzip, MaximumUncompressedBytes);
        using var reader = new TarReader(bounded);
        var count = 0;
        long total = 0;
        var names = new HashSet<string>(StringComparer.Ordinal);
        TarEntry? entry;
        while ((entry = reader.GetNextEntry()) is not null)
        {
            if (++count > 100000) throw Invalid("Archive contains more than 100000 entries.");
            var name = entry.Name.TrimEnd('/');
            if (string.IsNullOrWhiteSpace(name) || name.StartsWith('/') || name.Contains('\\') || name.Contains(':') ||
                name.Split('/').Any(component => component is ".." or ".") || name.IndexOfAny(['\r', '\n', '\0']) >= 0)
                throw Invalid($"Unsafe archive path: '{name}'.");
            if (!names.Add(name)) throw Invalid($"Duplicate archive entry: '{name}'.");
            var allowed = new[] { "data/data/" + packageName, "data/user/0/" + packageName };
            if (!allowed.Any(prefix => name == prefix || name.StartsWith(prefix + "/", StringComparison.Ordinal)))
                throw Invalid($"Archive path is outside application '{packageName}': '{name}'.");
            if (entry.EntryType is not TarEntryType.Directory and not TarEntryType.RegularFile and not TarEntryType.V7RegularFile)
                throw Invalid($"Unsupported archive entry type at '{name}': {entry.EntryType}.");
            total = checked(total + entry.Length);
            if (entry.Length < 0 || total > MaximumUncompressedBytes) throw Invalid("Uncompressed archive content exceeds its size limit.");
        }
        if (count == 0) throw Invalid("Archive is empty.");
        bounded.CopyTo(Stream.Null);
    }

    private void SaveRecord(AppBackupRecord record)
    {
        var path = Path.Combine(directory, record.Id.ToString("N") + ".json");
        var temporary = path + ".tmp";
        try
        {
            File.WriteAllText(temporary, JsonSerializer.Serialize(record, JsonDefaults.Options), new System.Text.UTF8Encoding(false));
            File.Move(temporary, path, true);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }
    private async Task EnsureRootAsync(string serial, CancellationToken ct)
    {
        if ((await backend.ShellAsync(serial, "id -u", true, ct).ConfigureAwait(false)).Trim() != "0")
            throw new DeviceException($"Root is required for application backup on '{serial}'.", 409, 1003);
    }
    private async Task<string> ResolvePrivatePathAsync(string serial, string packageName, CancellationToken ct)
    {
        var primary = "/data/user/0/" + packageName; var fallback = "/data/data/" + packageName;
        var output = (await backend.ShellAsync(serial,
            $"if [ -d {Quote(primary)} ]; then printf '%s' {Quote(primary)}; elif [ -d {Quote(fallback)} ]; then printf '%s' {Quote(fallback)}; else exit 1; fi", true, ct).ConfigureAwait(false)).Trim();
        if (output != primary && output != fallback) throw new DeviceException($"Application data path is unavailable for '{packageName}'.", 409, 1003);
        return output;
    }
    private static string Quote(string value) => AutomationService.Quote(value);
    private static DeviceException Invalid(string message) => new(message, 400, 1007);

    private sealed class LimitedReadStream(Stream source, long limit) : Stream
    {
        private long consumed;
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => consumed; set => throw new NotSupportedException(); }
        public override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));
        public override int Read(Span<byte> buffer)
        {
            var count = source.Read(buffer);
            consumed = checked(consumed + count);
            if (consumed > limit) throw Invalid("Uncompressed archive stream exceeds its read limit.");
            return count;
        }
        public override void Flush() { }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
