using System.Collections.Concurrent;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Runtime.ExceptionServices;
using System.Text.Json;
using MatrixDevice.Core;
using MatrixDevice.Host;

internal static class ApiStartupTests
{
    private static readonly List<Probe> Retained = [];
    private static bool cleanupBlocked;

    public static void Register(Action<string, Func<Task>> test)
    {
        test("API startup cold pair and repeated start preserve actual endpoints", () => Run(async p =>
        {
            p.ReleaseBoth();
            await p.Wait("cold-pair", p.Runtime.StartApisAsync(p.Token));
            var devicePort = await p.Device();
            await p.Healths(devicePort);
            await p.Wait("repeated-pair", p.Runtime.StartApisAsync(p.Token));
            Check(await p.Device() == devicePort, "Repeated device start changed its actual port");
            await p.Healths(devicePort);
        }));
        test("API startup cold second-port conflict rolls back only new basic and retries", () => Run(async p =>
        {
            p.ReleaseBasic();
            await p.PortFailure();
            Check(!p.Api.BasicRunning && !p.Api.AutomatorRunning && p.Api.DevicePorts.Count == 0, "Cold conflict left a new listener");
            p.Released(p.BasicPort);
            await p.Blocker(p.AutomatorPort);
            p.ReleaseAutomator();
            await p.Wait("retry-pair", p.Runtime.StartApisAsync(p.Token));
            await p.Healths();
        }));
        test("API startup rollback preserves an existing device endpoint", () => Run(async p =>
        {
            var devicePort = await p.Device();
            await p.Health(devicePort, "device");
            p.ReleaseBasic();
            await p.PortFailure();
            Check(!p.Api.BasicRunning && !p.Api.AutomatorRunning, "New basic was not rolled back");
            p.Released(p.BasicPort);
            await p.ExistingDevice(devicePort);
            await p.Blocker(p.AutomatorPort);
            p.ReleaseAutomator();
            await p.Wait("retry-with-device", p.Runtime.StartApisAsync(p.Token));
            await p.Healths(devicePort);
        }));
        test("API startup failed automator supplement preserves existing basic and device", () => Run(async p =>
        {
            p.ReleaseBasic();
            await p.Wait("existing-basic", p.Api.StartBasicAsync(p.BasicPort, p.Token));
            var devicePort = await p.Device();
            await p.Health(p.BasicPort, "basic");
            await p.ExistingDevice(devicePort);
            await p.PortFailure();
            Check(p.Api.BasicRunning && p.Api.RunningBasicPort == p.BasicPort && !p.Api.AutomatorRunning, "Supplement failure removed the pre-existing basic gateway");
            await p.Health(p.BasicPort, "basic");
            await p.ExistingDevice(devicePort);
            await p.Blocker(p.AutomatorPort);
            p.ReleaseAutomator();
            await p.Wait("retry-supplement", p.Runtime.StartApisAsync(p.Token));
            await p.Healths(devicePort);
        }));
        test("API startup first-port conflict preserves existing automator and device", () => Run(async p =>
        {
            p.ReleaseAutomator();
            await p.Wait("existing-automator", p.Api.StartAutomatorAsync(p.AutomatorPort, p.Token));
            var devicePort = await p.Device();
            await p.Health(p.AutomatorPort, "automator");
            await p.PortFailure(p.BasicPort);
            Check(!p.Api.BasicRunning && p.Api.AutomatorRunning && p.Api.RunningAutomatorPort == p.AutomatorPort, "First-port failure removed the pre-existing automator");
            await p.Health(p.AutomatorPort, "automator");
            await p.ExistingDevice(devicePort);
            await p.Blocker(p.BasicPort);
        }));
        test("API startup basic port mismatch preserves all existing services and original error", () => Run(async p =>
        {
            var oldBasic = p.ExtraPort();
            await p.Wait("old-basic", p.Api.StartBasicAsync(oldBasic, p.Token));
            p.ReleaseBoth();
            await p.Wait("old-automator", p.Api.StartAutomatorAsync(p.AutomatorPort, p.Token));
            var devicePort = await p.Device();
            var error = await p.Failure(p.Runtime.StartApisAsync(p.Token));
            Check(error is DeviceException && error.Message == "Stop the basic gateway before changing its port", "Basic mismatch did not preserve the original public startup error: " + error);
            Check(p.Api.RunningBasicPort == oldBasic && p.Api.RunningAutomatorPort == p.AutomatorPort, "Basic mismatch stopped an existing gateway");
            await p.Health(oldBasic, "basic");
            await p.Health(p.AutomatorPort, "automator");
            await p.ExistingDevice(devicePort);
            p.Released(p.BasicPort);
        }));
        test("API startup automator port mismatch removes only the new basic", () => Run(async p =>
        {
            var oldAutomator = p.ExtraPort();
            await p.Wait("old-automator", p.Api.StartAutomatorAsync(oldAutomator, p.Token));
            var devicePort = await p.Device();
            p.ReleaseBoth();
            var error = await p.Failure(p.Runtime.StartApisAsync(p.Token));
            Check(error is DeviceException && error.Message == "Stop Automator before changing its port", "Automator mismatch did not preserve the original public startup error: " + error);
            Check(!p.Api.BasicRunning && p.Api.RunningAutomatorPort == oldAutomator, "Automator mismatch removed an old listener or retained the new one");
            p.Released(p.BasicPort);
            p.Released(p.AutomatorPort);
            await p.Health(oldAutomator, "automator");
            await p.ExistingDevice(devicePort);
        }));
        test("API startup pre-cancellation leaves existing gateways and device available", () => Run(async p =>
        {
            p.ReleaseBoth();
            await p.Wait("initial-pair", p.Runtime.StartApisAsync(p.Token));
            var devicePort = await p.Device();
            using var cancelled = new CancellationTokenSource();
            cancelled.Cancel();
            var error = await p.Failure(p.Runtime.StartApisAsync(cancelled.Token));
            Check(error is OperationCanceledException cancellation && cancellation.CancellationToken == cancelled.Token, "Pre-cancellation lost its original token/error: " + error);
            await p.Healths(devicePort);
        }));
        test("API startup concurrent idempotent pair and device calls have one consistent result", () => Run(async p =>
        {
            p.ReleaseBoth();
            var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            async Task Start() { await gate.Task; await p.Runtime.StartApisAsync(p.Token); }
            async Task<int> Device() { await gate.Task; return await p.Api.StartDeviceAsync("DEMO-001", p.Token); }
            var first = p.Track("concurrent-pair-1", Start());
            var second = p.Track("concurrent-pair-2", Start());
            var device = p.Track("concurrent-device", Device());
            gate.SetResult();
            await p.Wait("concurrent-all", Task.WhenAll(first, second, device));
            await p.Healths(device.Result);
            Check(p.Api.DevicePorts.Count == 1, "Concurrent idempotent calls created extra device registrations");
        }));
        test("API startup concurrent start and stop finish in a serializable lifecycle state", () => Run(async p =>
        {
            p.ReleaseBoth();
            var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            async Task Start() { await gate.Task; await p.Runtime.StartApisAsync(p.Token); }
            async Task Stop() { await gate.Task; await p.Runtime.StopApisAsync(p.Token); }
            var start = p.Track("concurrent-start", Start());
            var stop = p.Track("concurrent-stop", Stop());
            gate.SetResult();
            await p.Wait("concurrent-start-stop", Task.WhenAll(start, stop));
            Check(p.Api.BasicRunning == p.Api.AutomatorRunning, "Completed start/stop calls left a partial pair that no serial ordering permits");
            if (p.Api.BasicRunning) await p.Healths();
            else { p.Released(p.BasicPort); p.Released(p.AutomatorPort); }
            Console.WriteLine("API-STARTUP-OBSERVE concurrent start/stop: no specific interleaving claimed");
            await p.Wait("post-concurrent-retry", p.Runtime.StartApisAsync(p.Token));
            await p.Healths();
        }));
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException("PRODUCT: " + message);
    }

    private static async Task Run(Func<Probe, Task> action)
    {
        if (cleanupBlocked) throw new InvalidOperationException("NOT-RUN: an earlier API startup fixture retained an unresolved owner");
        var probe = new Probe();
        Exception? failure = null;
        try { await probe.Create(); await action(probe); }
        catch (Exception error) { failure = error; }
        var cleanup = await probe.Cleanup();
        if (cleanup.Count != 0)
            throw new AggregateException("API startup fixture cleanup failed", (failure is null ? cleanup : new[] { failure }.Concat(cleanup)).ToArray());
        if (failure is not null) ExceptionDispatchInfo.Capture(failure).Throw();
    }

    private sealed class Probe
    {
        private readonly ConcurrentQueue<(string Name, Task Task)> tasks = new();
        private readonly List<TcpClient> clients = [];
        private readonly CancellationTokenSource deadline = new(TimeSpan.FromSeconds(12));
        private readonly HttpClient http = new(new HttpClientHandler { UseProxy = false, AllowAutoRedirect = false }) { Timeout = TimeSpan.FromSeconds(4) };
        private readonly TcpListener basic;
        private readonly TcpListener automator;
        private Task<MatrixRuntime>? creation;
        private MatrixRuntime? runtime;
        public int BasicPort { get; }
        public int AutomatorPort { get; }
        public MatrixRuntime Runtime => runtime ?? throw new InvalidOperationException("PRECONDITION: Demo runtime not created");
        public ApiServiceManager Api => Runtime.Api;
        public CancellationToken Token => deadline.Token;

        public Probe()
        {
            basic = Reserve();
            try { automator = Reserve(); }
            catch { basic.Stop(); http.Dispose(); deadline.Dispose(); throw; }
            BasicPort = Port(basic);
            AutomatorPort = Port(automator);
        }

        public async Task Create()
        {
            var settings = new AppSettings
            {
                BasicApiPort = BasicPort, AutomatorApiPort = AutomatorPort,
                AutoStartApi = false, AutoConnect = false, SyncMouse = false, SyncKeyboard = false,
                LdConsolePath = "", AdbPath = "", AgentBaseUrl = null, AgentSerial = null,
                AdbAllowedSerials = [], LdSerialBindings = []
            };
            var directory = Path.Combine(Path.GetTempPath(), "MatrixDevice.ApiStartupTests", Guid.NewGuid().ToString("N"));
            creation = MatrixRuntime.CreateAsync(settings, directory, demo: true, ct: Token);
            runtime = await Wait("create-demo", creation);
            if (!Runtime.IsDemo || Runtime.Backend is not DemoDeviceBackend || Api.BasicRunning || Api.AutomatorRunning || Api.DevicePorts.Count != 0)
                throw new InvalidOperationException("PRECONDITION: expected an isolated cold Demo runtime");
            Console.WriteLine($"API-STARTUP-OBSERVE fresh={directory} basic={BasicPort} automator={AutomatorPort}");
        }

        public T Track<T>(string name, T task) where T : Task { tasks.Enqueue((name, task)); return task; }
        public async Task Wait(string name, Task task) => await Track(name, task).WaitAsync(TimeSpan.FromSeconds(6));
        public async Task<T> Wait<T>(string name, Task<T> task) => await Track(name, task).WaitAsync(TimeSpan.FromSeconds(6));
        public void ReleaseBasic() => basic.Stop();
        public void ReleaseAutomator() => automator.Stop();
        public void ReleaseBoth() { ReleaseBasic(); ReleaseAutomator(); }
        public int ExtraPort() { var listener = Reserve(); var port = Port(listener); listener.Stop(); return port; }
        public Task<int> Device() => Wait("start-device", Api.StartDeviceAsync("DEMO-001", Token));

        public async Task<Exception> Failure(Task operation)
        {
            try { await Wait("expected-startup-failure", operation); }
            catch (Exception error) when (operation.IsCompleted)
            {
                Console.WriteLine("API-STARTUP-OBSERVE original startup exception: " + error);
                return error;
            }
            throw new InvalidOperationException("PRODUCT: expected startup rejection did not occur");
        }

        public async Task PortFailure(int? occupiedPort = null)
        {
            var error = await Failure(Runtime.StartApisAsync(Token));
            var port = occupiedPort ?? AutomatorPort;
            bool HasBindError(Exception current) => current is IOException or SocketException ||
                (current is AggregateException aggregate && aggregate.InnerExceptions.Any(HasBindError)) ||
                (current.InnerException is not null && HasBindError(current.InnerException));
            Check(HasBindError(error) && error.ToString().Contains(port.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal), "Occupied-port startup error was replaced: " + error);
        }

        public async Task Healths(int? devicePort = null)
        {
            Check(Api.RunningBasicPort == BasicPort && Api.RunningAutomatorPort == AutomatorPort, "Expected gateway ports are not running");
            await Health(BasicPort, "basic");
            await Health(AutomatorPort, "automator");
            if (devicePort.HasValue) await ExistingDevice(devicePort.Value);
        }

        public async Task ExistingDevice(int port)
        {
            Check(Api.DevicePorts.Count == 1 && Api.DevicePorts.TryGetValue("DEMO-001", out var actual) && actual == port, "Existing device registration/port was lost");
            await Health(port, "device");
        }

        public Task Health(int port, string kind) => Wait("health-" + kind, ReadHealth(port, kind));
        private async Task ReadHealth(int port, string kind)
        {
            using var response = await http.GetAsync($"http://127.0.0.1:{port}/" + (kind == "automator" ? "api/health" : "health"), Token);
            var body = await response.Content.ReadAsStringAsync(Token);
            Check(response.StatusCode == HttpStatusCode.OK, $"{kind} health returned {(int)response.StatusCode}: {body}");
            using var json = JsonDocument.Parse(body);
            var root = json.RootElement;
            if (kind == "automator") Check(root.GetProperty("code").GetInt32() == 0 && root.GetProperty("data").GetProperty("running").GetBoolean(), "Automator health envelope changed");
            else
            {
                Check(root.GetProperty("ok").GetBoolean() && root.GetProperty("data").GetProperty("port").GetInt32() == port, "Basic/device health envelope or port changed");
                if (kind == "device") Check(root.GetProperty("data").GetProperty("serial").GetString() == "DEMO-001", "Device health answered for another serial");
            }
        }

        public async Task Blocker(int port)
        {
            var listener = port == BasicPort ? basic : automator;
            Check(listener.Server.IsBound && Port(listener) == port, "The fixture's occupied listener was closed");
            var client = new TcpClient(AddressFamily.InterNetwork);
            clients.Add(client);
            await Wait("occupied-listener-connect", client.ConnectAsync(IPAddress.Loopback, port, Token).AsTask());
            Check(listener.Pending(), "The successful TCP connection was not queued on the fixture's listener");
        }

        public void Released(int port)
        {
            var listener = new TcpListener(IPAddress.Loopback, port);
            try { listener.Start(); }
            catch (SocketException error) { throw new InvalidOperationException("PRODUCT: rolled-back port is still bound: " + port, error); }
            finally { listener.Stop(); }
        }

        public async Task<List<Exception>> Cleanup()
        {
            deadline.Cancel();
            List<Exception> errors = [];
            var original = tasks.ToArray();
            var drain = Track("operation-drain", Task.WhenAll(original.Select(item => item.Task)));
            try { await drain.WaitAsync(TimeSpan.FromSeconds(5)); }
            catch (Exception error) when (drain.IsCompleted) { Console.WriteLine("API-STARTUP-OBSERVE drained operation faults: " + error.GetType().Name); }
            catch (Exception error) { errors.Add(new InvalidOperationException("CLEANUP: original operations remain pending", error)); }
            if (original.Any(item => !item.Task.IsCompleted)) return Retain(errors);
            if (runtime is null && creation?.IsCompletedSuccessfully == true) runtime = creation.Result;
            if (runtime is not null)
            {
                var disposal = Track("runtime-dispose", runtime.DisposeAsync().AsTask());
                try { await disposal.WaitAsync(TimeSpan.FromSeconds(5)); }
                catch (Exception error) { errors.Add(new InvalidOperationException("CLEANUP: runtime disposal failed or timed out", error)); }
                if (!disposal.IsCompleted) return Retain(errors);
            }
            basic.Stop(); automator.Stop();
            foreach (var client in clients) client.Dispose();
            http.Dispose(); deadline.Dispose();
            if (errors.Count != 0) cleanupBlocked = true;
            Report(errors, false);
            return errors;
        }

        private List<Exception> Retain(List<Exception> errors)
        {
            cleanupBlocked = true;
            Retained.Add(this);
            if (errors.Count == 0) errors.Add(new InvalidOperationException("CLEANUP: unresolved original owner retained"));
            Report(errors, true);
            return errors;
        }

        private void Report(List<Exception> errors, bool retained) => Console.WriteLine("API-STARTUP-CLEANUP " + JsonSerializer.Serialize(new
        {
            taskCount = tasks.Count, tasks = tasks.Select(item => new { item.Name, status = item.Task.Status.ToString(), terminal = item.Task.IsCompleted }).ToArray(),
            pending = tasks.Count(item => !item.Task.IsCompleted), retained, errors = errors.Select(error => error.ToString()).ToArray()
        }));

        private static TcpListener Reserve() { var listener = new TcpListener(IPAddress.Loopback, 0); listener.Start(); return listener; }
        private static int Port(TcpListener listener) => ((IPEndPoint)listener.LocalEndpoint).Port;
    }
}
