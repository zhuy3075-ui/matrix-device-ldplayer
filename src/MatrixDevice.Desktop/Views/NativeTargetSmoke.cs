using System.Diagnostics;
using System.IO;
using System.Text.Json;
using MatrixDevice.Core;
using MatrixDevice.Features;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    private async Task VerifyNativeMainTargetAsync()
    {
        if (!ViewModel.Demo) throw new InvalidOperationException("投屏目标离线验收仅限演示模式。");
        var previousCurrent = ViewModel.CurrentDevice;
        var previousMain = ViewModel.MainDevice;
        var main = ViewModel.Devices[0];
        var other = ViewModel.Devices[1];
        var mainInfo = main.Info;
        var otherInfo = other.Info;
        var otherSelected = other.IsSelected;
        var syncMouse = ViewModel.SyncMouse;
        var syncKeyboard = ViewModel.SyncKeyboard;
        var cases = new List<string>();
        using var self = Process.GetCurrentProcess();
        var executable = self.MainModule?.FileName ?? throw new InvalidOperationException("当前进程路径不可用。");
        void Check(bool condition, string name)
        {
            if (!condition) throw new InvalidOperationException("主控投屏离线验收失败：" + name);
            cases.Add(name);
        }
        void Reject(Action action, string name)
        {
            try { action(); }
            catch (DeviceException) { cases.Add(name); return; }
            throw new InvalidOperationException("主控投屏未拒绝失效上下文：" + name);
        }
        try
        {
            Check(ViewModel.MainDevice is null && !CanEmbedScrcpy(), "没有明确主控时不回退到当前设备");
            await ViewModel.SetMainAsync(main);
            ViewModel.CurrentDevice = other;
            var target = MainScreenSnapshot() ?? throw new InvalidOperationException("主控投屏快照缺失。");
            var otherTarget = target with { Row = other, Serial = other.Serial, Generation = other.Info.Generation, TransportId = other.Info.TransportId };
            screenProcesses.Add(other.Serial, new(otherTarget, self, self.Id, self.StartTime.ToUniversalTime().Ticks, executable));
            Check(!CanEmbedScrcpy(), "主控 A 无投屏时，不嵌入当前设备 B 的进程");
            var session = new ScrcpySession(target, self, self.Id, self.StartTime.ToUniversalTime().Ticks, executable);
            screenProcesses.Add(main.Serial, session);
            Check(CanEmbedScrcpy(), "当前设备 B 不覆盖主控 A 的投屏目标");
            Check(!CanLaunchScrcpy(), "演示模式始终禁止启动外部客户端");
            other.Info = otherInfo with { State = "Offline" };
            Check(CanEmbedScrcpy(), "当前 B 离线不阻止在线主控 A 投屏");
            other.IsSelected = true;
            ViewModel.SyncMouse = ViewModel.SyncKeyboard = true;
            Check(CanEmbedScrcpy(), "开启同步并勾选离线 B 不耦合单主控投屏");
            other.IsSelected = otherSelected;
            ViewModel.SyncMouse = syncMouse; ViewModel.SyncKeyboard = syncKeyboard;
            ViewModel.CurrentDevice = null;
            Check(CanEmbedScrcpy(), "当前设备为空仍使用明确主控 A");
            ViewModel.CurrentDevice = other;
            other.Info = otherInfo;
            main.Info = mainInfo with { State = "Offline" };
            Check(!CanEmbedScrcpy(), "主控 A 离线时不回退到在线 B");
            Reject(() => RequireMainScreenTarget(target), "离线源拒绝旧快照");
            main.Info = mainInfo with { Generation = mainInfo.Generation + 1 };
            Check(!CanEmbedScrcpy(), "主控代次变化后旧投屏不可嵌入");
            Reject(() => RequireMainScreenTarget(target), "拒绝旧主控代次");
            main.Info = mainInfo with { TransportId = (mainInfo.TransportId ?? 0) + 1 };
            Reject(() => RequireMainScreenTarget(target), "拒绝旧 transport");
            main.Info = mainInfo;
            Reject(() => RequireMainScreenTarget(target with { Runtime = null! }), "拒绝旧 runtime");
            ViewModel.Devices.Remove(main);
            try { Check(!CanEmbedScrcpy(), "已移出当前工作区的主控不可投屏"); }
            finally { ViewModel.Devices.Insert(0, main); }
            ViewModel.SetExternalBusy(true);
            try { Check(!CanEmbedScrcpy(), "外部忙状态禁用投屏"); }
            finally { ViewModel.SetExternalBusy(false); }
            screenProcesses[main.Serial] = session with { BirthUtcTicks = session.BirthUtcTicks + 1 };
            Check(!CanEmbedScrcpy(), "拒绝 birth 不匹配的进程");
            Reject(() => RequireScrcpySession(target, screenProcesses[main.Serial]), "窗口等待后重验进程 birth");
            screenProcesses[main.Serial] = session with { ProcessId = session.ProcessId + 1 };
            Check(!CanEmbedScrcpy(), "拒绝 PID 不匹配的进程");
            screenProcesses[main.Serial] = session with { ExecutablePath = executable + ".not-owned" };
            Check(!CanEmbedScrcpy(), "拒绝可执行文件路径不匹配的进程");
            screenProcesses[main.Serial] = session with { };
            Reject(() => RequireScrcpySession(target, session), "窗口等待后拒绝已替换的所有权记录");
            screenProcesses[main.Serial] = session;
            var package = new ScriptPackage
            {
                Manifest = new ScriptManifest { Id = "native-main-gate", Name = "主控门禁离线夹具", Version = "1.0" },
                Steps = [new ScriptStep { Action = "wait", DurationMs = 30000 }]
            };
            var otherScript = ViewModel.Runtime.Scripts.RunAsync(other.Serial, package);
            try { Check(CanEmbedScrcpy(), "当前 B 的脚本不阻止主控 A 投屏"); }
            finally { ViewModel.Runtime.Scripts.Stop(other.Serial); await otherScript; }
            var mainScript = ViewModel.Runtime.Scripts.RunAsync(main.Serial, package);
            try { Check(!CanEmbedScrcpy(), "主控 A 的活动脚本阻止投屏"); }
            finally { ViewModel.Runtime.Scripts.Stop(main.Serial); await mainScript; }
            await ViewModel.SetMainAsync(other);
            await Task.Yield();
            Reject(() => RequireMainScreenTarget(target), "异步让出后切换主控拒绝旧快照守卫");
            await ViewModel.SetMainAsync(main);
            Reject(() => RequireMainScreenTarget(target), "主控切走再切回仍拒绝旧输入代次");
            Check(CanEmbedScrcpy(), "重新选择相同有效设备后仍可使用其自有投屏");
            File.WriteAllText(Path.Combine(ViewModel.DataDirectory, "native-main-target.json"), JsonSerializer.Serialize(new
            {
                mode = "demo", status = "PASS-OFFLINE",
                fixtureScope = "仅主控与会话守卫；外部进程启动、ADB、真实确认弹窗和嵌入路径均未调用；此处不是进程或网络探针计数。",
                actualEmbeddingExercised = false, confirmationTimingExercised = false, cases
            }, JsonDefaults.Options));
        }
        finally
        {
            foreach (var key in screenProcesses.Where(pair => ReferenceEquals(pair.Value.Process, self)).Select(pair => pair.Key).ToArray()) screenProcesses.Remove(key);
            main.Info = mainInfo;
            other.Info = otherInfo;
            other.IsSelected = otherSelected;
            ViewModel.SyncMouse = syncMouse; ViewModel.SyncKeyboard = syncKeyboard;
            ViewModel.SetExternalBusy(false);
            if (previousMain is not null) await ViewModel.SetMainAsync(previousMain);
            ViewModel.CurrentDevice = previousCurrent;
        }
    }
}
