using System.IO.Compression;
using System.Text;
using System.Threading.Channels;

namespace MatrixDevice.Host;

public sealed class RollingLog : IAsyncDisposable
{
    private readonly string _directory;
    private readonly Channel<string> _queue = Channel.CreateBounded<string>(new BoundedChannelOptions(4096) { FullMode = BoundedChannelFullMode.Wait, SingleReader = true });
    private readonly Task _writer;
    private DateTimeOffset _lastCleanup;
    public string? LastError { get; private set; }
    public event Action<string>? EntryAdded;
    public RollingLog(string directory)
    {
        _directory = directory;
        Directory.CreateDirectory(directory);
        _writer = WriteLoopAsync();
    }
    public void Write(string category, string message, string? serial = null)
    {
        var line = $"{DateTimeOffset.UtcNow:O} [{category}] [{serial ?? "host"}] {message.Replace('\r', ' ').Replace('\n', ' ')}";
        if (!_queue.Writer.TryWrite(line)) LastError = "Log queue is full or closed";
        EntryAdded?.Invoke(line);
    }
    private async Task WriteLoopAsync()
    {
        await foreach (var line in _queue.Reader.ReadAllAsync())
        {
            try
            {
                if (DateTimeOffset.UtcNow - _lastCleanup > TimeSpan.FromMinutes(5)) { Cleanup(); _lastCleanup = DateTimeOffset.UtcNow; }
                var stem = DateTime.UtcNow.ToString("yyyy-MM-dd");
                var file = Path.Combine(_directory, stem + ".log");
                var size = Encoding.UTF8.GetByteCount(line) + 1;
                for (var i = 1; File.Exists(file) && new FileInfo(file).Length + size > 10 * 1024 * 1024; i++)
                    file = Path.Combine(_directory, stem + $".{i}.log");
                await File.AppendAllTextAsync(file, line + "\n", new UTF8Encoding(false));
            }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException) { LastError = e.Message; }
        }
    }
    private void Cleanup()
    {
        var today = DateTime.UtcNow.ToString("yyyy-MM-dd") + ".log";
        foreach (var path in Directory.EnumerateFiles(_directory))
        {
            var file = new FileInfo(path);
            if (file.Name == today) continue;
            if (file.LastWriteTimeUtc < DateTime.UtcNow.AddDays(-14)) { file.Delete(); continue; }
            if (file.Extension == ".log" && file.Length >= 5 * 1024 * 1024)
            {
                var destination = path + ".gz";
                using (var input = file.OpenRead())
                using (var output = File.Create(destination))
                using (var gzip = new GZipStream(output, CompressionLevel.Optimal)) input.CopyTo(gzip);
                file.Delete();
            }
        }
        var remaining = new DirectoryInfo(_directory).GetFiles().OrderBy(f => f.LastWriteTimeUtc).ToList();
        var total = remaining.Sum(f => f.Length);
        foreach (var file in remaining)
        {
            if (total <= 200L * 1024 * 1024) break;
            if (file.Name == today) continue;
            total -= file.Length;
            file.Delete();
        }
    }
    public async ValueTask DisposeAsync()
    {
        _queue.Writer.TryComplete();
        await _writer.WaitAsync(TimeSpan.FromSeconds(3));
    }
}
