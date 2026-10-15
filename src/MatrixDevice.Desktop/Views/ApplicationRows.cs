using System.IO;
using MatrixDevice.Core;
using MatrixDevice.Features;

namespace MatrixDevice.Desktop;

internal sealed class ApplicationTargetRow(DeviceInfo info, bool protectedTarget) : ObservableObject
{
    private bool selected;
    public DeviceInfo Info { get; private set; } = info;
    public bool Protected { get; private set; } = protectedTarget;
    public string Serial => Info.Serial;
    public bool Available => Info.Online && !Protected;
    public string Caption => $"{Info.Name}\n{Serial} · {(Protected ? "受保护 / 未核验" : Info.Online ? "已连接" : "未连接")}";
    public bool IsSelected { get => selected; set => Set(ref selected, value); }
    public void Update(DeviceInfo value, bool protectedValue)
    {
        if (Info == value && Protected == protectedValue) return;
        Info = value; Protected = protectedValue; Changed(nameof(Info)); Changed(nameof(Available)); Changed(nameof(Caption));
    }
}

internal sealed class InstalledApplicationRow(ApplicationTarget target, AppInfo app) : ObservableObject
{
    private bool selected;
    public ApplicationTarget Target { get; } = target;
    public AppInfo App { get; } = app;
    public string Serial => Target.Serial;
    public string TargetName => string.IsNullOrWhiteSpace(Target.Name) ? Target.Serial : Target.Name;
    public string PackageName => App.PackageName;
    public string Name => App.Name ?? "未提供";
    public string Version => string.IsNullOrWhiteSpace(App.VersionName) ? "未提供" : App.VersionName;
    public string Kind => App.IsSystem ? "系统" : "用户";
    public bool IsSelected { get => selected; set => Set(ref selected, value); }
}

internal sealed class ApplicationTaskRow(ApplicationBatchItem item) : ObservableObject
{
    public string Serial => item.Target.Serial;
    public string Subject => item.PackageName ?? Path.GetFileName(item.LocalPath) ?? "应用列表";
    public string Operation => item.Operation switch { ApplicationOperation.List => "读取", ApplicationOperation.Install => "安装", ApplicationOperation.Uninstall => "卸载", ApplicationOperation.Start => "启动", ApplicationOperation.Stop => "停止", _ => "清数据" };
    public ApplicationTaskState ResultState { get; private set; } = ApplicationTaskState.Waiting;
    public string State => ResultState switch { ApplicationTaskState.Waiting => "等待", ApplicationTaskState.Running => "执行中", ApplicationTaskState.Succeeded => "已完成", ApplicationTaskState.CompletedUnverified => "命令完成", ApplicationTaskState.Failed => "失败", _ => "已取消" };
    public string Message { get; private set; } = "等待";
    public bool Terminal => ResultState is not (ApplicationTaskState.Waiting or ApplicationTaskState.Running);
    public void SetResult(ApplicationItemResult result) { ResultState = result.State; Message = result.Message; Changed(nameof(ResultState)); Changed(nameof(State)); Changed(nameof(Message)); Changed(nameof(Terminal)); }
}
