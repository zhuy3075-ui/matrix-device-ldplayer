using System.Collections.ObjectModel;
using System.Windows;
using MatrixDevice.Features;

namespace MatrixDevice.Desktop;

public sealed partial class WorkspaceViewModel
{
    private IReadOnlyList<DeviceRow> RequireScriptTargets()
    {
        if (Busy || Transferring) throw new InvalidOperationException("请等待当前操作结束再运行脚本");
        if (pendingLaunchSelection > 0) throw new InvalidOperationException("所选目标包含未连接设备，请先接入全部目标");
        var targets = Targets();
        if (targets.Count == 0) throw new InvalidOperationException("请先选择目标设备");
        if (targets.Any(row => !row.Info.Online)) throw new InvalidOperationException("所选目标包含未连接设备，请先接入全部目标");
        return targets;
    }
    private ScriptJobRow? selectedScriptJob;
    private Action<ScriptJobStatus>? scriptJobChanged;
    private Action<Guid, ScriptLogEntry>? scriptJobLogAdded;
    private readonly HashSet<Task> scriptJobObservers = [];
    public ObservableCollection<ScriptJobRow> ScriptJobs { get; } = [];
    public ActionCommand CancelScriptJobCommand { get; }
    public ScriptJobRow? SelectedScriptJob
    {
        get => selectedScriptJob;
        set { if (Set(ref selectedScriptJob, value)) { LoadScriptLogs(); RefreshCommands(); } }
    }
    public string ScriptLogContext => SelectedScriptJob is { } job ? $"任务日志 · {job.JobId} · {job.Serial}" : "设备日志 · 最近 500 条";

    private void AttachScriptJobs()
    {
        var source = runtime.Scripts;
        scriptJobChanged = _ => Application.Current.Dispatcher.BeginInvoke(() =>
        {
            if (!ReferenceEquals(source, runtime.Scripts)) return;
            RefreshScriptJobs();
        });
        scriptJobLogAdded = (id, _) => Application.Current.Dispatcher.BeginInvoke(() =>
        {
            if (ReferenceEquals(source, runtime.Scripts) && SelectedScriptJob?.JobId == id) LoadScriptLogs();
        });
        source.JobChanged += scriptJobChanged;
        source.JobLogAdded += scriptJobLogAdded;
        RefreshScriptJobs();
    }

    private void DetachScriptJobs(ScriptScheduler source)
    {
        if (scriptJobChanged is not null) source.JobChanged -= scriptJobChanged;
        if (scriptJobLogAdded is not null) source.JobLogAdded -= scriptJobLogAdded;
    }

    private void RefreshScriptJobs()
    {
        var jobs = runtime.Scripts.GetJobs();
        var retained = jobs.Select(job => job.JobId).ToHashSet();
        foreach (var row in ScriptJobs.Where(row => !retained.Contains(row.JobId)).ToArray()) ScriptJobs.Remove(row);
        for (var index = 0; index < jobs.Count; index++)
        {
            var job = jobs[index];
            var row = ScriptJobs.FirstOrDefault(item => item.JobId == job.JobId);
            if (row is null) { row = new ScriptJobRow(job); ScriptJobs.Add(row); }
            else row.Update(job);
            var previousIndex = ScriptJobs.IndexOf(row);
            if (previousIndex != index) ScriptJobs.Move(previousIndex, index);
        }
        if (SelectedScriptJob is { } selected && !retained.Contains(selected.JobId)) SelectedScriptJob = null;
        RefreshCommands();
    }

    public void CancelSelectedScriptJob()
    {
        if (SelectedScriptJob is not { } job) return;
        var accepted = runtime.Scripts.CancelJob(job.JobId);
        Status = accepted ? $"已请求取消任务 {job.JobId}" : "该任务已结束或不在当前会话中";
        RefreshScriptJobs();
    }

    private async Task ObserveScriptJobAsync(ScriptScheduler source, ScriptJobHandle handle, string serial)
    {
        try { await handle.Completion; }
        catch (Exception ex)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                if (!ReferenceEquals(source, runtime.Scripts)) return;
                Error = $"脚本任务 {handle.JobId} · {serial}：{PresentError(ex)}";
                AddActivity(Error);
            });
        }
    }

    private void TrackScriptJob(ScriptScheduler source, ScriptJobHandle handle, string serial)
    {
        scriptJobObservers.RemoveWhere(task => task.IsCompletedSuccessfully);
        scriptJobObservers.Add(ObserveScriptJobAsync(source, handle, serial));
    }

    private async Task DrainScriptJobObserversAsync()
    {
        await Task.WhenAll(scriptJobObservers.ToArray());
        scriptJobObservers.Clear();
    }
}

public sealed class ScriptJobRow(ScriptJobStatus status) : ObservableObject
{
    public ScriptJobStatus Status { get; private set; } = status;
    public Guid JobId => Status.JobId;
    public string Serial => Status.Target.Serial;
    public string Identity => $"{Serial} · 代次 {Status.Target.Generation} · {(Status.Target.IsDemo ? "演示" : "传输 " + Status.Target.TransportId)}";
    public string Package => $"{Status.PackageId} · {Status.PackageVersion}";
    public string State => Status.State switch
    {
        ScriptRunState.Running => "运行中", ScriptRunState.Cancelling => "取消中", ScriptRunState.Completed => "已完成",
        ScriptRunState.Cancelled => "已取消", ScriptRunState.Failed => "失败", _ => "未运行"
    };
    public string Progress => $"{Math.Max(0, Status.StepIndex + 1)}/{Status.StepCount}";
    public string Started => Status.StartedUtc.ToLocalTime().ToString("MM-dd HH:mm:ss");
    public string Ended => Status.EndedUtc?.ToLocalTime().ToString("MM-dd HH:mm:ss") ?? "";
    public string Result => Status.Error ?? Status.Result ?? "";
    public bool CanCancel => Status.State == ScriptRunState.Running;
    public void Update(ScriptJobStatus value)
    {
        Status = value;
        foreach (var property in new[] { nameof(Status), nameof(State), nameof(Progress), nameof(Ended), nameof(Result), nameof(CanCancel) }) Changed(property);
    }
}
