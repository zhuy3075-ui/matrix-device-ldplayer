using System.Text.Json;
using System.Text.Json.Nodes;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;

namespace MatrixDevice.Mcp;

internal abstract class NonSeekableStream : Stream
{
    public override bool CanSeek => false;
    public override long Length => throw new NotSupportedException();
    public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Flush() { }
}

internal sealed class BoundedInputStream(Stream input, ProtocolAdmission admission, Guid epoch) : NonSeekableStream
{
    private readonly OwnedConsoleIo io = new(input);
    private readonly byte[] readBuffer = new byte[65536];
    private int readOffset, readCount;
    private byte[]? frame;
    private int frameOffset;
    private readonly TaskCompletionSource<ITransport> transport = new(TaskCreationOptions.RunContinuationsAsynchronously);
    internal Exception? Failure { get; private set; }
    internal bool Eof { get; private set; }
    internal bool IoTerminal => io.IsTerminal;
    internal void SetTransport(ITransport value) => transport.TrySetResult(value);
    public override bool CanRead => true;
    public override bool CanWrite => false;
    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken token) => ReadAsync(buffer.AsMemory(offset, count), token).AsTask();

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken token = default)
    {
        try
        {
            while (frame is null)
            {
                using var line = new MemoryStream();
                var ended = false;
                while (!ended)
                {
                    if (readOffset == readCount)
                    {
                        readCount = await io.RunAsync(() => input.Read(readBuffer, 0, readBuffer.Length), token).ConfigureAwait(false);
                        readOffset = 0;
                        if (readCount == 0)
                        {
                            Eof = true; admission.Stop();
                            if (line.Length != 0) throw new InvalidDataException("Incomplete final stdio message.");
                            return 0;
                        }
                    }
                    var next = readBuffer.AsSpan(readOffset, readCount - readOffset).IndexOf((byte)'\n');
                    var take = next < 0 ? readCount - readOffset : next;
                    if (line.Length + take > 1048576) throw new InvalidDataException("Stdio request exceeds 1048576 bytes before SDK deserialization.");
                    line.Write(readBuffer.AsSpan(readOffset, take)); readOffset += take;
                    if (next >= 0) { readOffset++; ended = true; }
                }
                var bytes = line.ToArray();
                if (bytes.Length == 0 || bytes.All(b => b is (byte)' ' or (byte)'\t' or (byte)'\r')) continue;
                using var document = JsonDocument.Parse(bytes, new JsonDocumentOptions { MaxDepth = 36 });
                var root = document.RootElement;
                if (root.ValueKind != JsonValueKind.Object) throw new InvalidDataException("MCP stdio requires one JSON-RPC object per line.");
                var hasId = root.TryGetProperty("id", out var idValue);
                if (root.EnumerateObject().Count(p => p.Name == "id") > 1) throw new InvalidDataException("Duplicate outer JSON-RPC id is ambiguous.");
                RequestId id = hasId ? ProtocolAdmission.Id(idValue) : default;
                if (hasId) admission.RegisterSeen(id);
                if (hasId && !root.TryGetProperty("method", out _)) throw new InvalidDataException("Unsolicited client response is not supported by this tools-only server.");
                var isTool = root.TryGetProperty("method", out var method) && method.ValueKind == JsonValueKind.String && method.GetString() == "tools/call";
                string? toolName = null;
                if (isTool && root.TryGetProperty("params", out var parameters) && parameters.ValueKind == JsonValueKind.Object && parameters.TryGetProperty("name", out var name) && name.ValueKind == JsonValueKind.String) toolName = name.GetString();
                try { StrictJson.Validate(root); }
                catch (Exception error)
                {
                    if (!hasId) throw new InvalidDataException("Invalid notification JSON.", error);
                    await RejectAsync(id, isTool, error, token).ConfigureAwait(false); continue;
                }
                if (!hasId)
                {
                    if (!root.TryGetProperty("method", out _) || !admission.Notification(root)) continue;
                }
                else
                {
                    var rejection = admission.Reserve(id, toolName == "matrix.jobs.cancel");
                    if (rejection is not null)
                    {
                        await RejectAsync(id, isTool, new ToolFailure(rejection, "Request admission is full or rate-limited; cancel has a separate reserved channel."), token).ConfigureAwait(false);
                        continue;
                    }
                }
                frame = new byte[bytes.Length + 1]; bytes.CopyTo(frame, 0); frame[^1] = (byte)'\n'; frameOffset = 0;
            }
            var copied = Math.Min(buffer.Length, frame.Length - frameOffset);
            frame.AsMemory(frameOffset, copied).CopyTo(buffer); frameOffset += copied;
            if (frameOffset == frame.Length) frame = null;
            return copied;
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested || admission.Stopping) { return 0; }
        catch (Exception error) { Failure = error; admission.Stop(); Console.Error.WriteLine("mcp-input-failure: " + error.GetType().Name + ": " + error.Message); throw; }
    }

    private async Task RejectAsync(RequestId id, bool isTool, Exception error, CancellationToken token)
    {
        // Rejection is one awaited SDK send, never a per-line detached task or queue.
        var sender = await transport.Task.ConfigureAwait(false);
        JsonRpcMessage response = isTool
            ? new JsonRpcResponse { Id = id, Result = JsonSerializer.SerializeToNode(McpToolHost.Result(McpToolHost.Error(epoch, error)), McpJsonUtilities.DefaultOptions) }
            : new JsonRpcError { Id = id, Error = new JsonRpcErrorDetail { Code = (int)McpErrorCode.InvalidParams, Message = "Request rejected by bounded transport admission." } };
        using var budget = CancellationTokenSource.CreateLinkedTokenSource(token);
        budget.CancelAfter(TimeSpan.FromSeconds(30));
        await sender.SendMessageAsync(response, budget.Token).ConfigureAwait(false);
    }
    internal void Stop() { admission.Stop(); io.Stop(); }
    internal async Task DrainAsync() { try { await io.DrainAsync().ConfigureAwait(false); } catch (OperationCanceledException) { } }
    protected override void Dispose(bool disposing) { if (disposing) { Stop(); io.Dispose(); } base.Dispose(disposing); }
}

internal sealed class BoundedOutputStream(Stream output, ProtocolAdmission admission) : NonSeekableStream
{
    private readonly OwnedConsoleIo io = new(output);
    private readonly MemoryStream message = new();
    internal Exception? Failure { get; private set; }
    internal bool IoTerminal => io.IsTerminal;
    internal Action? OnFailure { get; set; }
    public override bool CanRead => false;
    public override bool CanWrite => true;
    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => WriteAsync(buffer.AsMemory(offset, count)).AsTask().GetAwaiter().GetResult();
    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken token) => WriteAsync(buffer.AsMemory(offset, count), token).AsTask();
    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken token = default)
    {
        try
        {
            // After the SDK acquired its send lock, finish this complete frame even
            // if its request was cancelled. Only our owned I/O deadline may abort it.
            if (message.Length + buffer.Length > 1048577) throw new InvalidDataException("SDK response exceeds 1048576 bytes plus newline.");
            message.Write(buffer.Span);
            return ValueTask.CompletedTask;
        }
        catch (Exception error) { Fail(error); throw; }
    }
    public override async Task FlushAsync(CancellationToken token)
    {
        try
        {
            var bytes = message.ToArray();
            if (bytes.Length is 0 or > 1048577 || bytes[^1] != (byte)'\n') throw new InvalidDataException("Invalid outbound SDK message boundary.");
            using var budget = new CancellationTokenSource();
            budget.CancelAfter(TimeSpan.FromSeconds(30));
            await io.RunAsync(() => { output.Write(bytes); output.Flush(); return true; }, budget.Token).ConfigureAwait(false);
            using var document = JsonDocument.Parse(bytes.AsMemory(0, bytes.Length - 1));
            if (document.RootElement.TryGetProperty("id", out var id)) admission.ResponseFlushed(ProtocolAdmission.Id(id));
            message.SetLength(0);
        }
        catch (Exception error) { Fail(error); throw; }
    }
    private void Fail(Exception error) { Failure ??= error; admission.Stop(); io.Stop(); OnFailure?.Invoke(); }
    internal async Task DrainAsync() { try { await io.DrainAsync().ConfigureAwait(false); } catch (OperationCanceledException) { } }
    protected override void Dispose(bool disposing) { if (disposing) { io.Dispose(); message.Dispose(); } base.Dispose(disposing); }
}
