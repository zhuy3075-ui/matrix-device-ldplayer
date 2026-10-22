using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace MatrixDevice.Mcp;

// Each blocking Windows console-pipe operation has one retained native thread.
// Cancellation repeatedly targets that exact thread until the original I/O ends.
internal sealed class OwnedConsoleIo(Stream stream) : IDisposable
{
    private readonly CancellationTokenSource stopping = new();
    private readonly object gate = new();
    private Task? pending;
    private bool disposed;

    internal Task<T> RunAsync<T>(Func<T> action, CancellationToken token)
    {
        lock (gate)
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            if (pending is { IsCompleted: false }) throw new InvalidOperationException("Concurrent access to one standard stream is not allowed.");
            var operation = RunOperationAsync(action, token);
            pending = operation;
            return operation;
        }
    }

    private async Task<T> RunOperationAsync<T>(Func<T> action, CancellationToken token)
    {
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(token, stopping.Token);
        var completion = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        var thread = new Thread(() =>
        {
            SafeFileHandle? nativeThread = null;
            Timer? cancellationTimer = null;
            T? result = default;
            Exception? failure = null;
            try
            {
                if (!DuplicateHandle(GetCurrentProcess(), GetCurrentThread(), GetCurrentProcess(), out nativeThread, 0, false, 2)) throw new Win32Exception(Marshal.GetLastWin32Error());
                var cancellationGate = new object();
                cancellationTimer = new Timer(_ =>
                {
                    lock (cancellationGate)
                        if (linked.IsCancellationRequested && nativeThread is { IsClosed: false }) CancelSynchronousIo(nativeThread);
                }, null, 0, 10);
                linked.Token.ThrowIfCancellationRequested();
                result = action();
            }
            catch (Exception error) { failure = error; }
            finally
            {
                Exception? cleanupFailure = null;
                try
                {
                    // Wait for queued timer callbacks before closing their exact-thread handle.
                    cancellationTimer?.DisposeAsync().AsTask().GetAwaiter().GetResult();
                }
                catch (Exception error) { cleanupFailure = error; }
                finally
                {
                    try { nativeThread?.Dispose(); }
                    catch (Exception error) { cleanupFailure ??= error; }
                }
                if (cleanupFailure is not null) completion.TrySetException(cleanupFailure);
                else if (failure is null) completion.TrySetResult(result!);
                else if (linked.IsCancellationRequested) completion.TrySetCanceled(linked.Token);
                else completion.TrySetException(failure);
            }
        }) { IsBackground = true, Name = "matrix-mcp-owned-stdio" };
        thread.Start();
        try { return await completion.Task.ConfigureAwait(false); }
        finally
        {
            // Never recycle the buffer, token or native-thread handle before this thread ends.
            thread.Join();
        }
    }

    internal void Stop() => stopping.Cancel();
    internal bool IsTerminal { get { lock (gate) return pending is null || pending.IsCompleted; } }
    internal Task DrainAsync() { lock (gate) return pending ?? Task.CompletedTask; }
    public void Dispose() { lock (gate) { if (disposed) return; disposed = true; stopping.Cancel(); } stream.Dispose(); }

    [DllImport("kernel32.dll")] private static extern IntPtr GetCurrentProcess();
    [DllImport("kernel32.dll")] private static extern IntPtr GetCurrentThread();
    [DllImport("kernel32.dll", SetLastError = true)] private static extern bool DuplicateHandle(IntPtr sourceProcess, IntPtr sourceHandle, IntPtr targetProcess, out SafeFileHandle target, uint access, bool inherit, uint options);
    [DllImport("kernel32.dll", SetLastError = true)] private static extern bool CancelSynchronousIo(SafeFileHandle thread);
}
