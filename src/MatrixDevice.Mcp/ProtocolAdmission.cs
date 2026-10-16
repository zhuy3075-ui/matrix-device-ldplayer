using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;

namespace MatrixDevice.Mcp;

internal sealed class ProtocolAdmission
{
    private sealed record Credit(bool Cancel);
    private readonly object gate = new();
    private readonly Dictionary<RequestId, Credit> active = [];
    private readonly HashSet<string> seen = new(StringComparer.Ordinal);
    private readonly HashSet<RequestId> cancellationNotifications = [];
    private int ordinary, cancellation, notifications;
    private bool initializedNotification;
    private double ordinaryTokens = 18, cancelTokens = 2;
    private long lastRefill = Stopwatch.GetTimestamp();
    internal bool Stopping { get; private set; }
    internal Action? OnStop { get; set; }
    internal Action? OnFatal { get; set; }
    internal Exception? Failure { get; private set; }
    internal int InFlight { get { lock (gate) return active.Count + notifications; } }

    internal static RequestId Id(JsonElement element)
        => element.ValueKind switch
        {
            JsonValueKind.String => new RequestId(element.GetString()!),
            JsonValueKind.Number when element.TryGetInt64(out var number) => new RequestId(number),
            _ => throw new InvalidDataException("JSON-RPC id must be a string or exact Int64; null is not a notification.")
        };

    internal void RegisterSeen(RequestId id)
    {
        lock (gate)
        {
            var value = id.Id switch
            {
                string text when Encoding.UTF8.GetByteCount(text) <= 1024 => "s:" + text,
                long number => "i:" + number.ToString(CultureInfo.InvariantCulture),
                _ => throw Fatal("JSON-RPC string id exceeds 1024 UTF-8 bytes or has an unsupported type.")
            };
            var digest = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
            if (seen.Contains(digest)) throw Fatal("JSON-RPC id reuse is forbidden for this connection, including completed and rejected requests.");
            if (seen.Count >= 65536) throw Fatal("Connection request-id capacity reached; no further request is accepted or queued for rejection.");
            seen.Add(digest);
        }
    }

    internal bool Notification(JsonElement document)
    {
        lock (gate)
        {
            if (Stopping || notifications >= 8) return false;
            var method = document.GetProperty("method").GetString();
            if (method == "notifications/initialized" && !initializedNotification) { initializedNotification = true; notifications++; return true; }
            if (method != "notifications/cancelled" || !document.TryGetProperty("params", out var parameters) || !parameters.TryGetProperty("requestId", out var value)) return false;
            var id = Id(value);
            if (!active.ContainsKey(id) || !cancellationNotifications.Add(id)) return false;
            notifications++; return true;
        }
    }

    internal string? Reserve(RequestId id, bool cancel)
    {
        lock (gate)
        {
            if (active.ContainsKey(id)) throw Fatal("Concurrent JSON-RPC id reuse is forbidden.");
            if (Stopping) return "CANCELLED";
            var now = Stopwatch.GetTimestamp();
            var seconds = Stopwatch.GetElapsedTime(lastRefill, now).TotalSeconds; lastRefill = now;
            ordinaryTokens = Math.Min(18, ordinaryTokens + seconds * 8); cancelTokens = Math.Min(2, cancelTokens + seconds * 2);
            if (cancel ? cancellation >= 1 || cancelTokens < 1 : ordinary >= 3 || ordinaryTokens < 1) return "BUSY";
            if (cancel) { cancellation++; cancelTokens--; } else { ordinary++; ordinaryTokens--; }
            active.Add(id, new(cancel));
            return null;
        }
    }

    internal void ResponseFlushed(RequestId id) { lock (gate) Release(id); }
    private void Release(RequestId id)
    {
        if (!active.Remove(id, out var credit)) return;
        if (credit.Cancel) cancellation--; else ordinary--;
        cancellationNotifications.Remove(id);
    }

    internal void IncomingEnded(JsonRpcMessage message, bool requestCancelled)
    {
        lock (gate)
        {
            if (message is JsonRpcNotification) { notifications--; return; }
            if (requestCancelled && message is JsonRpcRequest request)
            {
                Release(request.Id);
            }
        }
    }
    private InvalidDataException Fatal(string message)
    {
        var failure = new InvalidDataException(message);
        Failure ??= failure;
        Stop();
        OnFatal?.Invoke();
        return failure;
    }
    internal void Stop() { lock (gate) Stopping = true; OnStop?.Invoke(); }
}
