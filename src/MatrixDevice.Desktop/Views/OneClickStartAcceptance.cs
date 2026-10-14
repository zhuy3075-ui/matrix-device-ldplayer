using System.IO;
using System.Text.Json;
using MatrixDevice.Core;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    internal async Task RunOneClickStartAcceptanceAsync(string output, string approvedCandidate)
    {
        var runtime = ViewModel.Runtime;
        if (ViewModel.Demo || !Path.IsPathFullyQualified(output) || !ViewModel.DataDirectory.Contains(Path.DirectorySeparatorChar + "artifacts" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) || runtime.Settings.AdbAllowedSerials.Length != 0)
            throw new InvalidOperationException("冷启动验收必须使用无初始授权的隔离artifacts工作区。");
        var runId = Guid.NewGuid().ToString("N");
        var confirm = ViewModel.Confirm; var confirmations = 0;
        var passed = false; var hidden = false; var colors = 0;
        var othersUnchanged = false; var otherCount = 0; var playerPid = 0; var vboxPid = 0;
        string? failure = null;
        Directory.CreateDirectory(output);
        var resultPath = Path.Combine(output, "result.json");
        await File.WriteAllTextAsync(resultPath, JsonSerializer.Serialize(new { runId, passed = false, state = "running" }, JsonDefaults.Options));
        try
        {
            var target = runtime.LdPlayer!.Instances.Single(item => item.Index == 1);
            if (!target.Name.StartsWith("MatrixAcceptance-", StringComparison.Ordinal) || target.AndroidStarted || target.ProcessId > 0 || target.VBoxProcessId > 0)
                throw new InvalidOperationException("仅允许已关闭的专用测试实例1，不能为验收关闭其它实例。");
            var original = runtime.LdPlayer.Instances.Where(item => item.Index != 1).ToArray();
            otherCount = original.Length;
            ViewModel.Confirm = (_, _) => { confirmations++; throw new InvalidOperationException("一键启动出现确认框。"); };
            await ConnectLdInstancesAsync([1], new Dictionary<int, string> { [1] = approvedCandidate });
            if (confirmations != 0 || !LdInstanceConnected(1) || ViewModel.Runtime.Settings.LdSerialBindings.GetValueOrDefault(1) != approvedCandidate || ViewModel.Devices.Count != 1)
                throw new InvalidOperationException("一键冷启动未完成指定实例连接：" + ldFeedback.Text);
            hidden = ldWindowVisibility?.IsHidden(1) == true;
            if (!hidden) throw new InvalidOperationException("新启动的雷电窗口未保持隐藏。");
            var png = await ViewModel.Runtime.Backend.ScreenshotAsync(approvedCandidate);
            var size = PngImage.ReadDimensions(png);
            colors = Enumerable.Range(0, 16).Select(i => PngImage.ColorAt(png, i * (size.Width - 1) / 15, i * (size.Height - 1) / 15)).Distinct().Count();
            if (colors < 2) throw new InvalidOperationException("后台截图未检出有效画面。");
            await File.WriteAllBytesAsync(Path.Combine(output, "hidden-device.png"), png);
            await ViewModel.Runtime.RefreshLdPlayerAsync();
            var launched = ViewModel.Runtime.LdPlayer!.Instances.Single(item => item.Index == 1);
            playerPid = launched.ProcessId; vboxPid = launched.VBoxProcessId;
            if (!launched.AndroidStarted || playerPid <= 0 || vboxPid <= 0)
                throw new InvalidOperationException("截图后实例未保持运行。");
            foreach (var before in original)
            {
                var after = ViewModel.Runtime.LdPlayer!.Instances.SingleOrDefault(item => item.Index == before.Index);
                if (after is null || after.Name != before.Name || after.ProcessId != before.ProcessId || after.VBoxProcessId != before.VBoxProcessId || after.AndroidStarted != before.AndroidStarted)
                    throw new InvalidOperationException("测试期间其它实例状态发生变化，不能标记互不干扰通过。");
            }
            othersUnchanged = true;
            SetWallViewMode("list"); UpdateUnifiedDirectory();
            await Dispatcher.InvokeAsync(() => { }, System.Windows.Threading.DispatcherPriority.ContextIdle);
            await Dispatcher.InvokeAsync(UpdateLayout, System.Windows.Threading.DispatcherPriority.Render);
            SaveRender(Path.Combine(output, "connected.png"));
            passed = true;
        }
        catch (Exception ex) { failure = ex.ToString(); throw; }
        finally
        {
            ViewModel.Confirm = confirm;
            Exception? restoreFailure = null;
            try { if (ldWindowVisibility is not null) await ldWindowVisibility.ShowAsync(1); }
            catch (Exception ex) { restoreFailure = ex; passed = false; }
            await File.WriteAllTextAsync(resultPath, JsonSerializer.Serialize(new
            {
                runId, passed, state = "finished", failure, restorationFailure = restoreFailure?.ToString(),
                index = 1, startupConfirmationDialogs = confirmations, hiddenVerified = hidden,
                screenshotSampleColors = colors, restored = ldWindowVisibility?.IsHidden(1) != true,
                emulatorShutdownNotRequested = true, otherInstanceCount = otherCount, othersUnchanged,
                launchAttempted = coldLaunched.Contains(1), observedPlayerPid = playerPid,
                observedVBoxPid = vboxPid, trueHeadless = false
            }, JsonDefaults.Options));
            if (restoreFailure is not null) throw new InvalidOperationException("验收窗口恢复失败，详见本轮result.json。", restoreFailure);
        }
    }
}
