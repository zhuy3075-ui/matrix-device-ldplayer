using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using ModelContextProtocol.Protocol;

namespace MatrixDevice.Mcp.Tests;

internal sealed class OwnedStdioProcess
{
    private readonly string directory;
    private readonly Process process;
    private readonly List<Task> io = [];
    private readonly List<string> errors = [];
    private readonly Stopwatch clock = Stopwatch.StartNew();
    private readonly FileStream inputRaw, outputRaw, errorRaw;
    private readonly TaskCompletionSource outputEof = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private static readonly List<OwnedStdioProcess> retained = [];
    private Task? exit, stderrPump;
    private WireStream? input, output;
    private bool started, stdinClosed, killed, exited, resourcesDisposed;
    private int? pid, exitCode;
    private long? birth;
    internal StreamClientTransport Transport => new(input ?? throw new InvalidOperationException("Owned process not started."), output!);

    internal OwnedStdioProcess(string directory, string dotnet, string server, string policy, string imports)
    {
        this.directory = directory;
        var start = new ProcessStartInfo(dotnet)
        {
            UseShellExecute = false, CreateNoWindow = true, RedirectStandardInput = true,
            RedirectStandardOutput = true, RedirectStandardError = true, WorkingDirectory = directory
        };
        start.Environment.Clear();
        foreach (var key in new[] { "SystemRoot", "SystemDrive", "TEMP", "TMP", "PATHEXT" })
            if (Environment.GetEnvironmentVariable(key) is { } value) start.Environment[key] = value;
        start.Environment["PATH"] = Path.GetDirectoryName(dotnet) + Path.PathSeparator + Environment.SystemDirectory;
        start.Environment["DOTNET_ROOT"] = Path.GetDirectoryName(dotnet); start.Environment["DOTNET_NOLOGO"] = "1";
        foreach (var value in new[] { server, "--demo", "--data-dir", Path.Combine(directory, "data"), "--policy", policy, "--import-root", imports }) start.ArgumentList.Add(value);
        process = new Process { StartInfo = start };
        inputRaw = Raw("stdin.bin"); outputRaw = Raw("stdout.bin"); errorRaw = Raw("stderr.bin");
        FileStream Raw(string name) => new(Path.Combine(directory, name), FileMode.CreateNew, FileAccess.Write, FileShare.Read, 81920, true);
    }
    internal void Start()
    {
        if (!process.Start()) throw new IOException("Owned Demo server did not start.");
        started = true; _ = process.SafeHandle; pid = process.Id; birth = process.StartTime.ToUniversalTime().Ticks;
        stderrPump = PumpErrorAsync(); io.Add(stderrPump);
        input = new WireStream(this, process.StandardInput.BaseStream, inputRaw, write: true);
        output = new WireStream(this, process.StandardOutput.BaseStream, outputRaw, write: false);
        exit = process.WaitForExitAsync();
    }
    internal void SaveStart() => Evidence.Save(Path.Combine(directory, "server-start.json"), new
    {
        pid, birth, server = process.StartInfo.ArgumentList[0], scope = "owned-demo-only",
        transport = "official StreamClientTransport", transportName = Transport.Name, actualStreams = "external process OS pipes", shell = false, killEntireTree = false
    });
    private Task<T> Track<T>(Task<T> task) { lock (io) io.Add(task); return task; }
    private Task Track(Task task) { lock (io) io.Add(task); return task; }
    private async Task PumpErrorAsync()
    {
        var buffer = new byte[81920];
        int count;
        while ((count = await process.StandardError.BaseStream.ReadAsync(buffer)) != 0)
        {
            await errorRaw.WriteAsync(buffer.AsMemory(0, count)); await errorRaw.FlushAsync();
        }
    }
    internal bool IoTerminal { get { lock (io) return io.All(task => task.IsCompleted); } }
    internal bool ProcessTerminal => !started || (exited && exit?.IsCompleted == true);
    internal bool OutputEofObserved => outputEof.Task.IsCompletedSuccessfully;
    internal async Task FinishPipesAsync(bool callsTerminal)
    {
        if (!callsTerminal) errors.Add("Original calls remain pending; stdin is not closed underneath active writes.");
        else if (started)
        {
            try { process.StandardInput.Close(); stdinClosed = true; }
            catch (Exception error) { errors.Add("Close actual stdin: " + error); }
        }
        if (started)
        {
            try { await exit!.WaitAsync(TimeSpan.FromSeconds(15)); exited = process.HasExited; exitCode = process.ExitCode; }
            catch (Exception error)
            {
                errors.Add("Original process exit: " + error);
                try
                {
                    if (!process.HasExited) { killed = true; process.Kill(false); }
                    await exit!.WaitAsync(TimeSpan.FromSeconds(5)); exited = process.HasExited; exitCode = process.ExitCode;
                }
                catch (Exception cleanup) { errors.Add("Owned process cleanup: " + cleanup); }
            }
            if (output?.HasRead == true)
            {
                try { await outputEof.Task.WaitAsync(TimeSpan.FromSeconds(10)); }
                catch (Exception error) { errors.Add("Actual stdout EOF: " + error); }
            }
            try { await stderrPump!.WaitAsync(TimeSpan.FromSeconds(10)); }
            catch (Exception error) { errors.Add("Actual stderr drain: " + error); }
        }
        Task[] original;
        lock (io) original = io.ToArray();
        try { await Task.WhenAll(original).WaitAsync(TimeSpan.FromSeconds(10)); }
        catch (Exception error) { errors.Add("Original pipe I/O: " + error); }
    }
    internal JsonElement SaveEnd(bool sdkTerminal, bool callsTerminal)
    {
        Task[] original;
        lock (io) original = io.ToArray();
        var terminal = (!started || (exited && exit?.IsCompleted == true)) && original.All(task => task.IsCompleted) && sdkTerminal && callsTerminal;
        if (terminal)
        {
            try
            {
                inputRaw.Flush(); outputRaw.Flush(); errorRaw.Flush();
                inputRaw.Dispose(); outputRaw.Dispose(); errorRaw.Dispose();
                if (started) { process.StandardInput.Dispose(); process.StandardOutput.Dispose(); process.StandardError.Dispose(); }
                process.Dispose(); resourcesDisposed = true;
            }
            catch (Exception error) { errors.Add("Terminal resource disposal: " + error); }
        }
        else
        {
            errors.Add("Original process/SDK/calls/I/O are nonterminal; resources retained, not disposed.");
            lock (retained) retained.Add(this);
        }
        var streams = new List<object>();
        foreach (var name in new[] { "stdin.bin", "stdout.bin", "stderr.bin" })
        {
            try
            {
                using var source = new FileStream(Path.Combine(directory, name), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                streams.Add(new { name, bytes = source.Length, sha256 = Convert.ToHexString(SHA256.HashData(source)) });
            }
            catch (Exception error) { errors.Add("Raw stream hash: " + error); }
        }
        var report = Evidence.Json(new
        {
            pid, birth, exitCode, exited, eof = stdinClosed, stdinClosed, stdoutEof = outputEof.Task.IsCompletedSuccessfully, killed,
            elapsedMs = clock.Elapsed.TotalMilliseconds, pendingPumps = original.Count(task => !task.IsCompleted), errors, streams,
            exitTaskStatus = exit?.Status.ToString(), stderrTaskStatus = stderrPump?.Status.ToString(),
            ioStatuses = original.Select(task => task.Status.ToString()).ToArray(), ownedResourcesDisposed = resourcesDisposed, ownedResourcesRetained = !terminal,
            transport = "official StreamClientTransport", transportName = "in-memory-stream", actualStreams = "external process OS pipes",
            stdoutConsumers = 1, shell = false, killEntireTree = false, runtimeVersion = Environment.Version.ToString()
        });
        Evidence.Save(Path.Combine(directory, "owned-process.json"), report);
        return report;
    }
    private sealed class WireStream(OwnedStdioProcess owner, Stream pipe, FileStream raw, bool write) : Stream
    {
        internal bool HasRead { get; private set; }
        public override bool CanRead => !write;
        public override bool CanWrite => write;
        public override bool CanSeek => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException("SDK pipe reads must use async I/O.");
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException("SDK pipe writes must use async I/O.");
        public override void Flush() => throw new NotSupportedException("SDK pipe flushes must use async I/O.");
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken ct) => ReadAsync(buffer.AsMemory(offset, count), ct).AsTask();
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken ct = default)
        {
            if (write) throw new NotSupportedException();
            HasRead = true;
            return new(owner.Track(ReadCore(buffer, ct)));
        }
        private async Task<int> ReadCore(Memory<byte> buffer, CancellationToken ct)
        {
            var count = await pipe.ReadAsync(buffer, ct);
            if (count == 0) owner.outputEof.TrySetResult();
            else { await raw.WriteAsync(buffer[..count], ct); await raw.FlushAsync(ct); }
            return count;
        }
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken ct) => WriteAsync(buffer.AsMemory(offset, count), ct).AsTask();
        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken ct = default)
        {
            if (!write) throw new NotSupportedException();
            return new(owner.Track(WriteCore(buffer, ct)));
        }
        private async Task WriteCore(ReadOnlyMemory<byte> buffer, CancellationToken ct)
        {
            await pipe.WriteAsync(buffer, ct); await raw.WriteAsync(buffer, ct); await raw.FlushAsync(ct);
        }
        public override Task FlushAsync(CancellationToken ct) => owner.Track(FlushCore(ct));
        private async Task FlushCore(CancellationToken ct) { await pipe.FlushAsync(ct); await raw.FlushAsync(ct); }
        protected override void Dispose(bool disposing) { }
    }
}
