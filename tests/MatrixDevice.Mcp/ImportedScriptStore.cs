using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using MatrixDevice.Features;

namespace MatrixDevice.Mcp;

internal sealed record ImportedScript(Guid Reference, string Sha256, ScriptPackage Package, long Bytes, FileStream FrozenHandle);

internal sealed class ImportedScriptStore(string importRoot, string stagingDirectory) : IDisposable
{
    private readonly object gate = new();
    private readonly Dictionary<Guid, ImportedScript> scripts = [];
    private int reservations;
    private long reservedBytes;
    private bool disposed;
    private bool stopping;
    internal int CleanupFailures { get; private set; }
    internal void StopAccepting() { lock (gate) stopping = true; }

    internal ImportedScript Get(Guid reference)
    { lock (gate) return scripts.GetValueOrDefault(reference) ?? throw new ToolFailure("UNKNOWN_REFERENCE", "Unknown script reference for this server epoch."); }

    internal async Task<(ImportedScript Script, JsonObject Response)> ImportAsync(Guid epoch, string relativePath, string expectedHash, CancellationToken ct)
    {
        if (relativePath.Length > 4096 || Path.IsPathRooted(relativePath) || relativePath.Contains(':') || relativePath.Contains('/') || relativePath.Contains('%')
            || relativePath.Split('\\').Any(s => s.Length == 0 || s is "." or ".." || s.EndsWith('.') || s.EndsWith(' ') || s.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0))
            throw new ToolFailure("INVALID_ARGUMENT", "relativePath must contain unambiguous root-relative Windows file segments.");
        if (expectedHash.Length != 64 || expectedHash.Any(c => !char.IsAsciiHexDigitLower(c))) throw new ToolFailure("INVALID_ARGUMENT", "sha256 must contain 64 lowercase hexadecimal digits.");
        var full = Path.GetFullPath(Path.Combine(importRoot, relativePath));
        if (!full.StartsWith(importRoot + "\\", StringComparison.OrdinalIgnoreCase)) throw new ToolFailure("INVALID_ARGUMENT", "Import path escaped the authorized root.");
        using var sourceIdentity = StablePath.Open(full, false);
        using var source = new FileStream(sourceIdentity.Leaf, FileAccess.Read);
        var size = source.Length;
        if (size is <= 0 or > 16777216) throw new ToolFailure("CAPACITY", "Archive byte limit is 16777216.");
        lock (gate)
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            if (stopping) throw new ToolFailure("CANCELLED", "Server is stopping; no new script references are accepted.");
            if (scripts.Count + reservations >= 16 || reservedBytes + size > 67108864) throw new ToolFailure("CAPACITY", "Immutable script reference or staging byte capacity reached.");
            reservations++; reservedBytes += size;
        }
        var path = Path.Combine(stagingDirectory, Guid.NewGuid().ToString("N") + ".zip");
        FileStream? frozen = null;
        var committed = false;
        var created = false;
        string? fileIdentity = null;
        try
        {
            ct.ThrowIfCancellationRequested();
            frozen = new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read, 65536, FileOptions.Asynchronous);
            created = true; fileIdentity = StablePath.Identity(frozen.SafeFileHandle);
            using var digest = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            var buffer = new byte[65536]; long copied = 0;
            while (true)
            {
                var read = await source.ReadAsync(buffer, ct).ConfigureAwait(false);
                if (read == 0) break;
                copied += read;
                if (copied > size || copied > 16777216) throw new ToolFailure("CAPACITY", "Source changed size or exceeded the archive byte limit.");
                digest.AppendData(buffer.AsSpan(0, read));
                await frozen.WriteAsync(buffer.AsMemory(0, read), ct).ConfigureAwait(false);
            }
            if (copied != size) throw new ToolFailure("INVALID_ARGUMENT", "Source size changed while freezing.");
            await frozen.FlushAsync(ct).ConfigureAwait(false);
            var hash = Convert.ToHexStringLower(digest.GetHashAndReset());
            if (!string.Equals(hash, expectedHash, StringComparison.Ordinal)) throw new ToolFailure("INVALID_ARGUMENT", "Archive SHA256 mismatch.");
            frozen.Dispose(); frozen = null;
            // File.OpenRead in the original importer cannot share an existing writer.
            // Reopen read-only, then prove the same file identity and complete bytes.
            frozen = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 65536, FileOptions.Asynchronous);
            if (StablePath.Identity(frozen.SafeFileHandle) != fileIdentity || frozen.Length != size
                || Convert.ToHexStringLower(await SHA256.HashDataAsync(frozen, ct).ConfigureAwait(false)) != hash)
                throw new ToolFailure("INVALID_ARGUMENT", "Frozen file identity or bytes changed during the read-only handoff.");
            frozen.Position = 0;
            // The legacy importer reads only our immutable, write-locked copy.
            var package = await Task.Run(() => ScriptPackageService.Load(path), CancellationToken.None).ConfigureAwait(false);
            ct.ThrowIfCancellationRequested();
            var manifest = package.Manifest;
            var metadata = JsonSerializer.SerializeToUtf8Bytes(new { packageId = manifest.Id, packageVersion = manifest.Version, displayName = manifest.Name }, StrictJson.Options);
            if (metadata.Length > 8192) throw new ToolFailure("CAPACITY", "Manifest response metadata exceeds 8192 serialized bytes.");
            var reference = Guid.NewGuid();
            var body = StrictJson.Node(new
            {
                ok = true, serverEpoch = epoch, scriptRef = reference, sha256 = hash,
                packageId = manifest.Id, packageVersion = manifest.Version, displayName = manifest.Name,
                stepCount = package.Steps.Count, parameterCount = package.Parameters.Count,
                bindingVersion = package.ExecutionContract?.Version,
                parameterKeys = package.Parameters.Select(p => p.Key).ToArray(),
                retention = "process-lifetime; immutable; no-execution-authority"
            });
            if (JsonSerializer.SerializeToUtf8Bytes(body, StrictJson.Options).Length > 65536) throw new ToolFailure("CAPACITY", "Complete parameter key list exceeds the response byte budget.");
            var script = new ImportedScript(reference, hash, package, size, frozen);
            lock (gate)
            {
                ct.ThrowIfCancellationRequested();
                ObjectDisposedException.ThrowIf(disposed, this);
                if (stopping) throw new ToolFailure("CANCELLED", "Server stopped before script reference registration.");
                scripts.Add(reference, script);
                committed = true;
            }
            return (script, body);
        }
        finally
        {
            var cleaned = !created;
            if (!committed)
            {
                frozen?.Dispose();
                if (created) cleaned = StablePath.DeleteOwned(path, fileIdentity);
                if (!cleaned)
                {
                    lock (gate) { CleanupFailures++; stopping = true; }
                    Console.Error.WriteLine("mcp-staging-cleanup-refused: identity not proven; byte reservation retained; imports stopped.");
                }
            }
            lock (gate) { reservations--; if (!committed && cleaned) reservedBytes -= size; }
        }
    }

    public void Dispose()
    {
        lock (gate)
        {
            if (disposed) return;
            disposed = true;
            foreach (var script in scripts.Values) script.FrozenHandle.Dispose();
        }
    }
}
