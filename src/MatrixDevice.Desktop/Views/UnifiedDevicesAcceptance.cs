using System.Diagnostics;
using System.IO;
using System.Text.Json;
using MatrixDevice.Core;
using MatrixDevice.Host;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    internal async Task RunUnifiedAccessAcceptanceAsync(string output, string approvedCandidate)
    {
        if (ViewModel.Demo || !Path.IsPathFullyQualified(output) || !ViewModel.DataDirectory.Contains(Path.DirectorySeparatorChar + "artifacts" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("接入验收必须使用artifacts下的独立配置及绝对证据路径。");
        var runtime = ViewModel.Runtime;
        var service = runtime.LdPlayer ?? throw new InvalidOperationException("未配置雷电。");
        if (runtime.Settings.AdbAllowedSerials.Length != 1 || runtime.Settings.LdSerialBindings.Count != 1 || !runtime.Settings.LdSerialBindings.ContainsKey(2))
            throw new InvalidOperationException("验收初始配置必须仅授权独立测试实例2。");
        var target = service.Instances.Single(item => item.Index == 1);
        if (!target.Name.StartsWith("MatrixAcceptance-", StringComparison.Ordinal) || !target.AndroidStarted)
            throw new InvalidOperationException("只允许已运行的独立验收实例1；本验收不会启动模拟器。");
        var previouslyOnline = ViewModel.Devices.Where(row => row.Info.Online).Select(row => row.Serial).ToArray();
        if (previouslyOnline.Length != 1) throw new InvalidOperationException("原授权实例2未就绪。");
        Directory.CreateDirectory(output);
        var completed = false;
        await RunLdWorkspaceOperationAsync("独立实例接入验收", async ct =>
        {
            using var process = Process.GetProcessById(target.VBoxProcessId);
            var proofs = await LdConnectionAdmission.ResolveAsync((AdbDeviceBackend)runtime.Backend, service, [1], [target], new Dictionary<int, long> { [1] = process.StartTime.ToUniversalTime().Ticks }, ct);
            if (proofs.Single().Serial != approvedCandidate) throw new InvalidOperationException("验收目标串号与复制配置中预先批准的串号不符。");
            if (proofs.Single().Serial == previouslyOnline[0]) throw new InvalidOperationException("接入目标与原设备串号重复。");
            var before = ViewModel.Runtime;
            using (var cancelled = new CancellationTokenSource())
            {
                cancelled.Cancel();
                try { await ViewModel.ApplyLdConnectionsAsync(proofs, cancelled.Token); throw new InvalidOperationException("取消未生效。"); }
                catch (OperationCanceledException) when (cancelled.IsCancellationRequested) { }
            }
            if (!ReferenceEquals(before, ViewModel.Runtime) || ViewModel.Runtime.Settings.AdbAllowedSerials.Length != 1)
                throw new InvalidOperationException("取消接入改变了旧授权或运行时。");
            await ViewModel.ApplyLdConnectionsAsync(proofs, ct);
            if (ReferenceEquals(before, ViewModel.Runtime) || ViewModel.Runtime.Settings.LdSerialBindings.Count != 2 || ViewModel.Devices.Count(row => row.Info.Online) != 2)
                throw new InvalidOperationException("运行时未完成接入替换。");
            foreach (var index in new[] { 1, 2 }) if (!LdInstanceConnected(index)) throw new InvalidOperationException("实例未进入已连接状态：" + index);
            await ViewModel.RefreshAutomaticScreensAsync(ViewModel.Devices.Select(row => row.Serial).ToArray(), ct);
            if (ViewModel.Devices.Any(row => row.Screenshot is null)) throw new InvalidOperationException("接入后截图为空。");
            UpdateLdPlayerRows(); UpdateUnifiedDirectory(); NavigateTo("设备墙 / 主控");
            await Dispatcher.InvokeAsync(UpdateLayout, System.Windows.Threading.DispatcherPriority.Render);
            SaveRender(Path.Combine(output, "connected.png"));
            await File.WriteAllTextAsync(Path.Combine(output, "result.json"), JsonSerializer.Serialize(new
            {
                passed = true, existingIndex = 2, admittedIndex = 1, cancellationPreservedRuntime = true,
                originalAuthorizationPreserved = true, localInstances = service.Instances.Count,
                connectedDevices = ViewModel.Devices.Count(row => row.Info.Online),
                runtimeReplaced = true, screenshotsAvailable = true,
                emulatorLaunchNotRequested = true, scopeLimitedToApprovedIndices = true
            }, JsonDefaults.Options));
            completed = true;
        });
        if (!completed) throw new InvalidOperationException("接入验收未完成：" + ldFeedback.Text);
    }
}
