using System.Diagnostics;
using System.Text;

namespace MatrixDevice.Core;

public sealed class LdConsoleRunner : ILdConsoleRunner
{
    public const int MaximumStdoutBytes = 32 * 1024 * 1024;
    public const int MaximumStderrBytes = 1024 * 1024;
    private readonly string _executable;
    private readonly TimeSpan _timeout;
    private static readonly Encoding StrictUtf8 = new UTF8Encoding(false, true);
    private static readonly Encoding Gbk = CreateGbk();

    public LdConsoleRunner(string executable, int timeoutSeconds = 30)
    {
        if (string.IsNullOrWhiteSpace(executable) || !Path.IsPathFullyQualified(executable) || !File.Exists(executable))
            throw new DeviceException($"雷电 CLI 路径必须为已存在的完整 Windows 路径：{executable}", 400, 1007);
        if (timeoutSeconds is < 1 or > 600) throw new DeviceException($"雷电 CLI 超时必须为 1 至 600 秒：{timeoutSeconds}", 400, 1007);
        _executable = Path.GetFullPath(executable);
        _timeout = TimeSpan.FromSeconds(timeoutSeconds);
    }

    public async Task<string> RunAsync(IReadOnlyList<string> arguments, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        if (arguments.Count == 0 || arguments.Any(argument => argument is null || argument.Contains('\0')))
            throw new DeviceException("雷电 CLI 参数不能为空，也不能包含 NUL。", 400, 1007);
        ct.ThrowIfCancellationRequested();
        var info = new ProcessStartInfo(_executable)
        {
            WorkingDirectory = Path.GetDirectoryName(_executable)!, UseShellExecute = false,
            CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden,
            RedirectStandardOutput = true, RedirectStandardError = true
        };
        foreach (var argument in arguments) info.ArgumentList.Add(argument);
        using var process = new Process { StartInfo = info };
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(ct);
        deadline.CancelAfter(_timeout);
        try
        {
            if (!process.Start()) throw new DeviceException("无法启动雷电 CLI 子进程。", 503, 2000);
        }
        catch (System.ComponentModel.Win32Exception error)
        { throw new DeviceException($"无法启动雷电 CLI：{error.Message}", 503, 2000); }

        async Task<byte[]> ReadAsync(Stream stream, int maximumBytes)
        {
            try { return await BoundedStreamRead.ReadAsync(stream, maximumBytes, deadline.Token).ConfigureAwait(false); }
            catch { deadline.Cancel(); throw; }
        }
        var stdout = ReadAsync(process.StandardOutput.BaseStream, MaximumStdoutBytes);
        var stderr = ReadAsync(process.StandardError.BaseStream, MaximumStderrBytes);
        try
        {
            await Task.WhenAll(stdout, stderr, process.WaitForExitAsync(deadline.Token)).ConfigureAwait(false);
            var output = Decode(await stdout.ConfigureAwait(false));
            var error = Decode(await stderr.ConfigureAwait(false));
            if (process.ExitCode != 0)
                throw new DeviceException($"雷电 CLI 执行失败：命令={arguments[0]}，退出码={process.ExitCode}，信息={error.Trim()} {output.Trim()}", 503, 2000);
            return output;
        }
        catch (Exception original)
        {
            deadline.Cancel();
            try
            {
                // Launch/reboot descendants may be the emulator, not disposable CLI workers.
                if (!process.HasExited) process.Kill(entireProcessTree: false);
                await process.WaitForExitAsync(CancellationToken.None).WaitAsync(TimeSpan.FromSeconds(2)).ConfigureAwait(false);
            }
            catch (Exception cleanup) when (cleanup is InvalidOperationException or System.ComponentModel.Win32Exception or TimeoutException) { }
            // Observe both readers before releasing their process streams.
            try { await Task.WhenAll(stdout, stderr).ConfigureAwait(false); } catch { }
            var limit = new[] { stdout.Exception, stderr.Exception }.OfType<AggregateException>()
                .SelectMany(error => error.Flatten().InnerExceptions).OfType<DeviceException>().FirstOrDefault(error => error.Status == 413);
            if (limit is not null) throw new DeviceException($"雷电 CLI 输出过大：命令={arguments[0]}；标准输出上限 32 MiB，错误输出上限 1 MiB。", 413, 1007);
            if (original is OperationCanceledException)
            {
                if (ct.IsCancellationRequested) throw;
                throw new DeviceException($"雷电 CLI 超时：命令={arguments[0]}，限制={_timeout.TotalSeconds} 秒。结果未知，不自动重试；仅尝试终止本次 CLI 进程，不关闭模拟器子进程。", 504, 2000);
            }
            throw;
        }
    }

    public static string Decode(ReadOnlySpan<byte> bytes)
    {
        try { return StrictUtf8.GetString(bytes).TrimStart('\ufeff'); }
        catch (DecoderFallbackException)
        {
            try { return Gbk.GetString(bytes); }
            catch (DecoderFallbackException error) { throw new DeviceException($"雷电 CLI 输出无法按 UTF-8 或 GBK 解码：{error.Message}", 502, 2000); }
        }
    }
    private static Encoding CreateGbk()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        return Encoding.GetEncoding(936, EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback);
    }
}
