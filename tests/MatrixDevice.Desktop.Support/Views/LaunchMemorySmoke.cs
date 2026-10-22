using System.Runtime.InteropServices;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using MatrixDevice.Core;
using MatrixDevice.Host;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    private async Task VerifyLaunchMemoryUiAsync()
    {
        if (!ViewModel.Demo) throw new InvalidOperationException("内存提示离线验收仅限演示模式。");
        var originalConfirm = ViewModel.Confirm;
        var confirmations = 0; var accept = true;
        CancellationTokenSource? cancelDuringConfirmation = null;
        ViewModel.Confirm = (title, message) =>
        {
            if (!Dispatcher.CheckAccess() || title != "启动资源风险" || !message.Contains("实例 2：离线内存测试", StringComparison.Ordinal) || !message.Contains("不是雷电官方", StringComparison.Ordinal))
                throw new InvalidOperationException("资源风险提示线程或目标范围错误。");
            confirmations++; cancelDuringConfirmation?.Cancel(); return accept;
        };
        try
        {
            var good = LdLaunchMemory.Assess(new(6144), new((ulong)(7.8 * 1073741824), 16UL * 1073741824, 75));
            if (!await Task.Run(() => ConfirmLaunchMemoryAsync(2, "离线内存测试", good, CancellationToken.None)) || confirmations != 0)
                throw new InvalidOperationException("7.8 GiB/75%的6 GiB实例被拦截或出现普通启动确认。");
            foreach (var nonblocking in new[]
            {
                LdLaunchMemory.Assess(new(null, "实例 RAM 未知"), new(11UL * 1073741824, 20UL * 1073741824, 65)),
                LdLaunchMemory.Assess(new(6144), null),
                LdLaunchMemory.Assess(new(6144), new(6UL * 1073741824, 6UL * 1073741824, 99))
            })
                if (!await Task.Run(() => ConfirmLaunchMemoryAsync(2, "离线内存测试", nonblocking, CancellationToken.None)) || confirmations != 0)
                    throw new InvalidOperationException("未知、采样失败或仅估算余量不足仍触发资源确认。");
            var risk = LdLaunchMemory.Assess(new(8192), new((ulong)(7.8 * 1073741824), 16UL * 1073741824, 75));
            if (!await Task.Run(() => ConfirmLaunchMemoryAsync(2, "离线内存测试", risk, CancellationToken.None)) || confirmations != 1)
                throw new InvalidOperationException("用户无法确认继续风险启动。");
            accept = false;
            if (await Task.Run(() => ConfirmLaunchMemoryAsync(2, "离线内存测试", risk, CancellationToken.None)) || confirmations != 2)
                throw new InvalidOperationException("用户取消资源风险提示仍被接受。");
            using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
            try
            {
                await ConfirmLaunchMemoryAsync(2, "离线内存测试", risk, cancellation.Token);
                throw new InvalidOperationException("已取消的资源检查仍可继续。");
            }
            catch (OperationCanceledException) when (cancellation.IsCancellationRequested) { }
            if (confirmations != 2) throw new InvalidOperationException("已取消的资源检查仍弹出提示。");
            using var duringConfirmation = new CancellationTokenSource();
            accept = true; cancelDuringConfirmation = duringConfirmation;
            try
            {
                await Task.Run(() => ConfirmLaunchMemoryAsync(2, "离线内存测试", risk, duringConfirmation.Token));
                throw new InvalidOperationException("提示期间取消后仍然接受启动。");
            }
            catch (OperationCanceledException) when (duringConfirmation.IsCancellationRequested) { }
            if (confirmations != 3) throw new InvalidOperationException("提示期间取消验收未覆盖确认回调。");
        }
        finally { ViewModel.Confirm = originalConfirm; }
    }

}
