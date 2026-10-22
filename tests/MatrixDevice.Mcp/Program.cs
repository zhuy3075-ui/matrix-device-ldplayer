using System.Text.Json;
using MatrixDevice.Core;
using MatrixDevice.Host;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;

namespace MatrixDevice.Mcp;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        StartupOptions? options = null;
        MatrixRuntime? runtime = null;
        ImportedScriptStore? scripts = null;
        McpToolHost? tools = null;
        IHost? host = null;
        BoundedInputStream? input = null;
        BoundedOutputStream? output = null;
        StablePath? dataIdentity = null;
        StablePath? stagingIdentity = null;
        var epoch = Guid.NewGuid();
        object[] ownedJobs = [];
        var runtimeDisposed = false;
        var handlerDrained = false;
        var ownedJobsTerminal = false;
        var failure = (string?)null;
        var created = false;
        var admission = new ProtocolAdmission();
        try
        {
            // All startup policy and real-mode rejection precede filesystem mutation.
            options = StartupOptions.Parse(args);
            if (!Console.IsInputRedirected || !Console.IsOutputRedirected) throw new ArgumentException("MCP requires redirected standard input and output pipes.");
            StablePath.CreateFreshDirectory(options.DataDirectory); created = true;
            dataIdentity = StablePath.Open(options.DataDirectory, true);
            var staging = Path.Combine(options.DataDirectory, "staging");
            StablePath.CreateFreshDirectory(staging);
            stagingIdentity = StablePath.Open(staging, true);
            var settings = new AppSettings { AutoStartApi = false, AutoConnect = false, SyncMouse = false, SyncKeyboard = false, AdbAllowedSerials = [], LdSerialBindings = [], AdbPath = "", LdConsolePath = "", AgentBaseUrl = null, AgentSerial = null };
            runtime = await MatrixRuntime.CreateAsync(settings, options.DataDirectory, demo: true).ConfigureAwait(false);
            scripts = new(options.ImportRoot.FullPath, staging);
            tools = new(runtime, options.Policy, scripts, epoch);
            admission.OnStop = tools.StopAccepting;
            input = new(Console.OpenStandardInput(), admission, epoch);
            admission.OnFatal = input.Stop;
            output = new(Console.OpenStandardOutput(), admission);
            output.OnFailure = input.Stop;
            var builder = Microsoft.Extensions.Hosting.Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings { Args = [], ApplicationName = "MatrixDevice.Mcp" });
            builder.Logging.ClearProviders();
            builder.Services.Configure<HostOptions>(o => o.ShutdownTimeout = TimeSpan.FromSeconds(10));
            // The official stream transport uses real stdio, with pre-deserialization
            // bounds that StdioServerTransport's hard-coded Console streams lack.
            builder.Services.AddMcpServer(o =>
            {
                o.ServerInfo = new Implementation { Name = "matrix-device-mcp", Version = "1.0.0" };
                o.ProtocolVersion = "2025-11-25";
                o.InitializationTimeout = TimeSpan.FromSeconds(30);
                o.ServerInstructions = "每次连接的 JSON-RPC 请求 id 必须唯一，已完成或拒绝的 id 也不能复用；字符串 id 最多 1024 UTF-8 字节，每连接最多 65536 个请求 id。超过这些传输限制将关闭连接并清理本进程拥有的作业。业务防重请使用 operationId，并以新的 JSON-RPC id 查询或重放原请求。工具参数按实际序列化 UTF-8 计数，最多 65536 字节。";
            })
                .WithStreamServerTransport(input, output)
                .WithListToolsHandler((_, _) => ValueTask.FromResult(tools.ListTools()))
                .WithCallToolHandler(tools.CallAsync)
                .WithMessageFilters(filters => filters.AddIncomingFilter(next => async (context, ct) =>
                {
                    try { await next(context, ct).ConfigureAwait(false); }
                    finally { admission.IncomingEnded(context.JsonRpcMessage, ct.IsCancellationRequested); }
                }));
            host = builder.Build();
            input.SetTransport(host.Services.GetRequiredService<ITransport>());
            await host.RunAsync().ConfigureAwait(false);
            if (input.Failure is not null) throw new IOException("Bounded stdin transport failed.", input.Failure);
            if (output.Failure is not null) throw new IOException("Bounded stdout transport failed.", output.Failure);
            if (admission.Failure is not null) throw new IOException("Protocol admission stopped after a fatal capacity failure.", admission.Failure);
        }
        catch (Exception error)
        {
            failure = error.GetType().Name + ": " + StrictJson.Short(error.Message, 1024).Text;
            Console.Error.WriteLine("mcp-server-failure: " + failure);
        }
        finally
        {
            admission.Stop(); input?.Stop();
            async Task Cleanup(Func<Task> action)
            {
                try { await action().ConfigureAwait(false); }
                catch (Exception error) { failure ??= "Cleanup failed: " + error.GetType().Name; Console.Error.WriteLine("mcp-cleanup-failure: " + error.GetType().Name); }
            }
            if (tools is not null) await Cleanup(async () =>
            {
                ownedJobs = await tools.StopAsync().ConfigureAwait(false);
                ownedJobsTerminal = JsonSerializer.SerializeToElement(ownedJobs, StrictJson.Options).EnumerateArray().All(job => job.GetProperty("terminal").GetBoolean());
                handlerDrained = true;
            });
            if (input is not null) await Cleanup(input.DrainAsync);
            if (output is not null) await Cleanup(output.DrainAsync);
            if (runtime is not null) await Cleanup(async () => { await runtime.DisposeAsync().ConfigureAwait(false); runtimeDisposed = true; });
            scripts?.Dispose();
            if (scripts?.CleanupFailures > 0) failure ??= "Owned staging cleanup could not prove file identity.";
            if (host is not null) await Cleanup(() => { host.Dispose(); return Task.CompletedTask; });
            input?.Dispose(); output?.Dispose();
            if (created && dataIdentity is not null && options is not null)
            {
                var inputIoTerminal = input?.IoTerminal ?? false;
                var outputIoTerminal = output?.IoTerminal ?? false;
                var pendingProtocolCredits = admission.InFlight;
                var shutdownVerified = handlerDrained && runtimeDisposed && ownedJobsTerminal && pendingProtocolCredits == 0
                    && inputIoTerminal && outputIoTerminal && input?.Failure is null && output?.Failure is null && admission.Failure is null
                    && (scripts?.CleanupFailures ?? 0) == 0;
                if (!shutdownVerified) failure ??= "Owned shutdown invariants were not satisfied.";
                var lifecycle = new
                {
                    version = 1, serverEpoch = epoch, processId = Environment.ProcessId,
                    mode = "demo", scopes = options.Policy.Scopes, authorizedSerials = options.Policy.Serials,
                    stdinEof = input?.Eof ?? false, handlerDrained, runtimeDisposed, ownedJobs, ownedJobsTerminal,
                    pendingProtocolCredits, inputIoTerminal, outputIoTerminal,
                    stagingCleanupFailures = scripts?.CleanupFailures ?? 0,
                    inputFailure = input?.Failure?.GetType().Name, outputFailure = output?.Failure?.GetType().Name,
                    protocolFailure = admission.Failure?.GetType().Name,
                    cleanExit = failure is null && shutdownVerified, failure
                };
                await Cleanup(async () =>
                {
                    await using var report = new FileStream(Path.Combine(options.DataDirectory, "mcp-lifecycle.json"), FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous);
                    await report.WriteAsync(JsonSerializer.SerializeToUtf8Bytes(lifecycle, StrictJson.Options)).ConfigureAwait(false);
                    await report.FlushAsync().ConfigureAwait(false);
                });
            }
            else if (created) Console.Error.WriteLine("mcp-lifecycle-unavailable: data directory ownership was not pinned; no report file written.");
            stagingIdentity?.Dispose(); dataIdentity?.Dispose(); options?.Dispose();
        }
        return failure is null ? 0 : 1;
    }
}
