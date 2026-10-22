using System.Text.Json;

namespace MatrixDevice.Mcp;

internal sealed class StartupOptions : IDisposable
{
    internal required string DataDirectory { get; init; }
    internal required StablePath ImportRoot { get; init; }
    internal required StablePath DataParent { get; init; }
    internal required McpPolicy Policy { get; init; }
    internal static StartupOptions Parse(string[] args)
    {
        var fields = new Dictionary<string, string>(StringComparer.Ordinal);
        var demo = false;
        for (var i = 0; i < args.Length; i++)
        {
            var key = args[i];
            if (key == "--demo") { if (demo) throw new ArgumentException("Duplicate --demo."); demo = true; continue; }
            if (key is not ("--data-dir" or "--policy" or "--import-root") || i + 1 >= args.Length || args[i + 1].StartsWith("--", StringComparison.Ordinal) || !fields.TryAdd(key, args[++i]))
                throw new ArgumentException("Unknown, duplicate or incomplete startup argument: " + key);
        }
        if (!demo || fields.Count != 3) throw new ArgumentException("Only explicit --demo --data-dir --policy --import-root is supported. Real mode is disabled.");
        var data = StablePath.Absolute(fields["--data-dir"]);
        var root = StablePath.Open(fields["--import-root"], true);
        StablePath? parent = null;
        try
        {
            using var policyPath = StablePath.Open(fields["--policy"], false);
            using var stream = new FileStream(policyPath.Leaf, FileAccess.Read);
            if (stream.Length is < 2 or > 16384) throw new ArgumentException("Policy must contain 2..16384 bytes.");
            var bytes = new byte[(int)stream.Length]; stream.ReadExactly(bytes);
            var policy = McpPolicy.Parse(bytes);
            if (data.Equals(root.FullPath, StringComparison.OrdinalIgnoreCase) || data.StartsWith(root.FullPath + "\\", StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("Data directory cannot be inside import root.");
            parent = StablePath.Open(Path.GetDirectoryName(data)!, true);
            if (File.Exists(data) || Directory.Exists(data)) throw new ArgumentException("Data directory must not exist.");
            return new() { DataDirectory = data, ImportRoot = root, DataParent = parent, Policy = policy };
        }
        catch { root.Dispose(); parent?.Dispose(); throw; }
    }
    public void Dispose() { ImportRoot.Dispose(); DataParent.Dispose(); }
}

internal sealed record McpPolicy(string ScopeOwner, string[] Scopes, string[] Serials)
{
    internal static readonly string[] AllScopes = ["devices.read", "scripts.import", "jobs.start", "jobs.read", "jobs.cancel"];
    internal static McpPolicy Parse(byte[] bytes)
    {
        using var document = StrictJson.Parse(bytes);
        var root = document.RootElement;
        StrictJson.Object(root, ["scopeOwner", "scopes", "authorizedSerials"], "scopeOwner", "scopes", "authorizedSerials");
        var owner = StrictJson.String(root, "scopeOwner", 128);
        string[] List(string name)
        {
            if (root.GetProperty(name).ValueKind != JsonValueKind.Array) throw new ArgumentException("Policy array required: " + name);
            var values = root.GetProperty(name).EnumerateArray().Select(v => v.ValueKind == JsonValueKind.String ? v.GetString()! : throw new ArgumentException("Policy values must be strings.")).ToArray();
            if (values.Distinct(StringComparer.Ordinal).Count() != values.Length) throw new ArgumentException("Duplicate policy values: " + name);
            return values;
        }
        var scopes = List("scopes"); var serials = List("authorizedSerials");
        if (scopes.Any(s => !AllScopes.Contains(s, StringComparer.Ordinal)) || serials.Length > 2 || serials.Any(s => s is not ("DEMO-001" or "DEMO-002"))) throw new ArgumentException("Policy contains unsupported scope or unauthorized Demo serial.");
        return new(owner, scopes, serials);
    }
    internal void Require(string scope) { if (!Scopes.Contains(scope, StringComparer.Ordinal)) throw new ToolFailure("UNAUTHORIZED", "Scope is not granted: " + scope); }
    internal void RequireSerial(string serial) { if (!Serials.Contains(serial, StringComparer.Ordinal)) throw new ToolFailure("UNAUTHORIZED", "Target is not authorized."); }
}
