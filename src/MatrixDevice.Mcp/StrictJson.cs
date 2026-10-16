using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace MatrixDevice.Mcp;

internal sealed class ToolFailure(string code, string message, int? projectCode = null) : Exception(message)
{
    internal string Code { get; } = code;
    internal int? ProjectCode { get; } = projectCode;
}

internal static class StrictJson
{
    internal static readonly JsonSerializerOptions Options = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, MaxDepth = 40 };
    internal static JsonDocument Parse(ReadOnlyMemory<byte> bytes, int depth = 32)
    {
        var document = JsonDocument.Parse(bytes, new JsonDocumentOptions { MaxDepth = depth });
        try { Validate(document.RootElement); return document; }
        catch { document.Dispose(); throw; }
    }

    internal static void Validate(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var property in value.EnumerateObject())
            {
                if (!names.Add(property.Name)) throw new ToolFailure("INVALID_ARGUMENT", "Duplicate JSON property: " + Short(property.Name, 80).Text);
                Validate(property.Value);
            }
        }
        else if (value.ValueKind == JsonValueKind.Array) foreach (var child in value.EnumerateArray()) Validate(child);
        else if (value.ValueKind == JsonValueKind.Number)
        {
            var raw = value.GetRawText();
            if (!raw.Contains('.') && !raw.Contains('e') && !raw.Contains('E'))
            {
                if (!value.TryGetInt64(out _) && !value.TryGetUInt64(out _)) throw new ToolFailure("INVALID_ARGUMENT", "Integer is outside the supported 64-bit range.");
            }
            else if (!value.TryGetDecimal(out _)) throw new ToolFailure("INVALID_ARGUMENT", "Number is outside the supported decimal range.");
        }
        else if (value.ValueKind == JsonValueKind.String)
        {
            string text;
            try { text = value.GetString()!; }
            catch (InvalidOperationException) { throw new ToolFailure("INVALID_ARGUMENT", "Invalid Unicode string."); }
            for (var i = 0; i < text.Length; i++)
                if (char.IsSurrogate(text[i]) && !(char.IsHighSurrogate(text[i]) && i + 1 < text.Length && char.IsLowSurrogate(text[++i])))
                    throw new ToolFailure("INVALID_ARGUMENT", "Invalid Unicode surrogate.");
        }
    }

    internal static void Object(JsonElement value, string[] allowed, params string[] required)
    {
        if (value.ValueKind != JsonValueKind.Object) throw new ToolFailure("INVALID_ARGUMENT", "Expected a JSON object.");
        Validate(value);
        foreach (var p in value.EnumerateObject()) if (!allowed.Contains(p.Name, StringComparer.Ordinal)) throw new ToolFailure("INVALID_ARGUMENT", "Unknown field: " + Short(p.Name, 80).Text);
        foreach (var name in required) if (!value.TryGetProperty(name, out _)) throw new ToolFailure("INVALID_ARGUMENT", "Missing field: " + name);
    }

    internal static string String(JsonElement obj, string name, int maximum = 256)
    {
        if (!obj.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.String) throw new ToolFailure("INVALID_ARGUMENT", "Expected string field: " + name);
        var text = value.GetString()!;
        if (text.Length == 0 || text.EnumerateRunes().Count() > maximum) throw new ToolFailure("INVALID_ARGUMENT", "String length invalid: " + name);
        return text;
    }

    internal static Guid Uuid(JsonElement obj, string name)
    {
        var value = String(obj, name, 36);
        if (!Guid.TryParseExact(value, "D", out var result) || result.ToString("D") != value) throw new ToolFailure("INVALID_ARGUMENT", "Expected canonical UUID field: " + name);
        return result;
    }

    internal static int Integer(JsonElement obj, string name, int minimum, int maximum, int fallback)
    {
        if (!obj.TryGetProperty(name, out var value)) return fallback;
        if (value.ValueKind != JsonValueKind.Number || !value.TryGetInt32(out var result) || result < minimum || result > maximum) throw new ToolFailure("INVALID_ARGUMENT", "Integer range invalid: " + name);
        return result;
    }

    internal static (string Text, bool Truncated) Short(string text, int maximum, string marker = "[truncated]")
    {
        var runes = text.EnumerateRunes().ToArray();
        if (runes.Length <= maximum) return (text, false);
        return (string.Concat(runes.Take(Math.Max(0, maximum - marker.EnumerateRunes().Count())).Select(r => r.ToString())) + marker, true);
    }

    internal static string Digest(JsonElement value)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream)) Canonical(writer, value);
        return Convert.ToHexStringLower(SHA256.HashData(stream.ToArray()));
    }

    private static void Canonical(Utf8JsonWriter writer, JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Object)
        {
            writer.WriteStartObject();
            foreach (var p in value.EnumerateObject().OrderBy(p => p.Name, StringComparer.Ordinal)) { writer.WritePropertyName(p.Name); Canonical(writer, p.Value); }
            writer.WriteEndObject();
        }
        else if (value.ValueKind == JsonValueKind.Array)
        { writer.WriteStartArray(); foreach (var item in value.EnumerateArray()) Canonical(writer, item); writer.WriteEndArray(); }
        else value.WriteTo(writer);
    }

    internal static JsonObject Node(object value) => JsonSerializer.SerializeToNode(value, Options)!.AsObject();
}
