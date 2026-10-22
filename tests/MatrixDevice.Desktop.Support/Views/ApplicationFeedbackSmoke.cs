using System.IO;
using System.Runtime.ExceptionServices;
using System.Text.Json;
using System.Windows.Controls;
using MatrixDevice.Core;
using MatrixDevice.Features;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    internal void VerifyApplicationFeedbackContract()
    {
        var directory = Path.Combine(ViewModel.DataDirectory, "application-feedback");
        if (!ViewModel.Demo || !ViewModel.Runtime.IsDemo || ViewModel.Runtime.Backend is not DemoDeviceBackend
            || !Path.IsPathFullyQualified(directory) || !directory.Contains(Path.DirectorySeparatorChar + "artifacts" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
            || Directory.Exists(directory) || File.Exists(directory)) throw new InvalidOperationException("应用反馈契约仅限 fresh artifacts Demo profile 内自有新子目录。");
        Directory.CreateDirectory(directory);
        var names = new[] { "list-success", "install-command-unverified", "mixed-terminal-failure", "cancelled-read", "empty", "waiting-running" };
        var reports = new List<object>(); var completed = new List<string>(); var failures = new List<Exception>(); var classes = new HashSet<string>(StringComparer.Ordinal);
        var runtime = ViewModel.Runtime; var current = ViewModel.CurrentDevice; var main = ViewModel.MainDevice; var selectedTab = tabs.SelectedItem;
        var devices = ViewModel.Devices.ToArray(); var infos = devices.Select(row => row.Info).ToArray();
        var targets = applicationTargets.ToArray(); var applications = applicationRows.ToArray(); var tasks = applicationTasks.ToArray();
        object? entry = null, exit = null; var contextUnchanged = false; Exception? failure = null;
        try
        {
            Require(Dispatcher.CheckAccess() && Thread.CurrentThread.GetApartmentState() == ApartmentState.STA && !ShowActivated, "同步自有 STA 窗口，不请求激活");
            Require(!ViewModel.Busy && !ViewModel.Transferring && !ViewModel.HasActiveOperations && !runtime.Api.BasicRunning && !runtime.Api.AutomatorRunning && !runtime.Api.HasActiveRequests
                && !runtime.Settings.AutoStartApi && !runtime.Settings.AutoConnect && !applicationRunning && !applicationReadPending && applicationReadTimer?.IsEnabled != true
                && targets.All(row => !row.IsSelected) && tasks.Length == 0 && applications.Length == 0, "业务/API/自动接入/应用任务无在途，应用目标为空选择");
            Require(!DebugAutomaticFixturePermitted && !ReconnectLoadingFixturePermitted && !captureInProgress && captureOperation is null
                && !monitorWasActive && !monitorTimer.IsEnabled && monitorReconcile.IsCompleted && monitorSampling.IsCompleted, "没有自动读取许可或原后台任务");
            entry = Values();
            Case(names[0], [Row(ApplicationOperation.List, ApplicationTaskState.Succeeded), Row(ApplicationOperation.List, ApplicationTaskState.Succeeded)], value =>
            {
                Contains(value, "进度 2/2", "已完成 2", "失败 0", "取消 0"); NoInstall(value);
            });
            Case(names[1], [Row(ApplicationOperation.Install, ApplicationTaskState.CompletedUnverified)], value =>
            {
                Contains(value, "进度 1/1", "已完成 0", "安装命令完成（包名未核验）1", "失败 0", "取消 0");
                Product(!value.Contains("已核验安装", StringComparison.Ordinal), "安装命令完成不升级为已核验安装");
            });
            Case(names[2], [Row(ApplicationOperation.List, ApplicationTaskState.Succeeded), Row(ApplicationOperation.Install, ApplicationTaskState.CompletedUnverified), Row(ApplicationOperation.Uninstall, ApplicationTaskState.Failed), Row(ApplicationOperation.Stop, ApplicationTaskState.Cancelled)], value =>
                Contains(value, "进度 4/4", "已完成 1", "安装命令完成（包名未核验）1", "失败 1", "取消 1"));
            Case(names[3], [Row(ApplicationOperation.List, ApplicationTaskState.Succeeded), Row(ApplicationOperation.List, ApplicationTaskState.Cancelled)], value =>
            {
                Contains(value, "进度 2/2", "已完成 1", "失败 0", "取消 1"); NoInstall(value);
            });
            Case(names[4], [], value => { Contains(value, "进度 0/0", "已完成 0", "失败 0", "取消 0"); NoInstall(value); });
            Case(names[5], [Row(ApplicationOperation.List, ApplicationTaskState.Waiting), Row(ApplicationOperation.List, ApplicationTaskState.Running)], value =>
            {
                Contains(value, "进度 0/2", "已完成 0", "失败 0", "取消 0"); NoInstall(value);
            });
        }
        catch (Exception error) { failures.Add(error); classes.Add(error is ApplicationFeedbackPrecondition ? "PRECONDITION-FAIL" : "FIXTURE-FAIL"); }
        finally
        {
            if (entry is not null)
            {
                try
                {
                    exit = Values();
                    contextUnchanged = ReferenceEquals(runtime, ViewModel.Runtime) && ReferenceEquals(current, ViewModel.CurrentDevice) && ReferenceEquals(main, ViewModel.MainDevice)
                        && ReferenceEquals(selectedTab, tabs.SelectedItem) && devices.SequenceEqual(ViewModel.Devices) && devices.Select(row => row.Info).Zip(infos).All(pair => ReferenceEquals(pair.First, pair.Second))
                        && targets.SequenceEqual(applicationTargets) && applications.SequenceEqual(applicationRows) && tasks.SequenceEqual(applicationTasks)
                        && JsonSerializer.Serialize(entry, JsonDefaults.Options) == JsonSerializer.Serialize(exit, JsonDefaults.Options);
                    if (!contextUnchanged) throw new InvalidOperationException("纯摘要契约改变了所记录的原业务引用或状态。");
                }
                catch (Exception error) { failures.Add(error); classes.Add("FIXTURE-FAIL"); }
            }
            if (failures.Count > 0) failure = new AggregateException("应用批次反馈契约未全部通过", failures);
            failure = SaveDeviceLayoutReport(() => Path.Combine(directory, "summary.json"), () => new
            {
                status = failure is null ? "PASS-APPLICATION-FEEDBACK" : classes.Count == 1 ? classes.Single() : "MIXED-FAIL", failureClasses = classes.OrderBy(value => value).ToArray(),
                plannedCaseCount = names.Length, caseCount = reports.Count, reports, notRunCases = names.Except(completed).ToArray(), entry, exit, contextUnchanged,
                synchronous = true, asynchronousTasksCreated = 0, backendCallsRequested = 0, nativeInputRequested = 0,
                offlineGeometry = "NOT-RUN", offlineGeometryReason = failure is null ? "App calls the unchanged geometry diagnostic only after this fixture returns" : "Feedback contract failure blocks the following geometry call",
                error = failure?.ToString()
            }, failure);
        }
        if (failure is not null) ExceptionDispatchInfo.Capture(failure).Throw();

        void Case(string name, ApplicationTaskRow[] rows, Action<string> verify)
        {
            var before = JsonSerializer.Serialize(rows.Select(RowValues).ToArray(), JsonDefaults.Options); string? actual = null; Exception? error = null; var unchanged = false;
            try
            {
                actual = ApplicationBatchSummary(rows);
                unchanged = before == JsonSerializer.Serialize(rows.Select(RowValues).ToArray(), JsonDefaults.Options);
                if (!unchanged) throw new InvalidOperationException("实际摘要函数改变了自有输入任务行。");
                verify(actual);
            }
            catch (Exception caught) { error = caught; failures.Add(caught); classes.Add(caught is ApplicationFeedbackProductFailure ? "PRODUCT-FAIL" : "FIXTURE-FAIL"); }
            reports.Add(new { name, status = error is null ? "PASS" : error is ApplicationFeedbackProductFailure ? "PRODUCT-FAIL" : "FIXTURE-FAIL", input = rows.Select(RowValues).ToArray(), actual, inputUnchanged = unchanged, error = error?.ToString() });
            completed.Add(name);
        }
        object Values() => new
        {
            current = ViewModel.CurrentDevice?.Serial, main = ViewModel.MainDevice?.Serial, tag = (tabs.SelectedItem as TabItem)?.Tag as string,
            ViewModel.TextInput, ViewModel.ShellInput, ViewModel.Status, ViewModel.Error, ViewModel.Busy, ViewModel.Transferring, ViewModel.HasActiveOperations,
            generation = runtime.Input.CurrentGeneration, mainGestureDispatchCount, mainCaptureDispatchCount, applicationRunning, applicationReadPending,
            timerEnabled = applicationReadTimer?.IsEnabled == true, result = applicationResult.Text, progress = applicationProgress.Value,
            devices = ViewModel.Devices.Select(row => new { row.Serial, row.Info, row.IsSelected }).ToArray(), targets = applicationTargets.Select(row => new { row.Serial, row.Info, row.IsSelected, row.Available }).ToArray(),
            applications = applicationRows.Select(row => new { row.Serial, row.Target, row.PackageName, row.IsSelected }).ToArray(), tasks = applicationTasks.Select(RowValues).ToArray(),
            runtime.Api.BasicRunning, runtime.Api.AutomatorRunning, runtime.Api.HasActiveRequests, ports = runtime.Api.DevicePorts.OrderBy(pair => pair.Key, StringComparer.Ordinal).ToArray(),
            captureInProgress, capturePending = captureOperation is not null, monitorWasActive, monitorTimerEnabled = monitorTimer.IsEnabled, reconcile = monitorReconcile.Status.ToString(), sampling = monitorSampling.Status.ToString()
        };
        static object RowValues(ApplicationTaskRow row) => new { row.Serial, row.Operation, row.Subject, row.ResultState, row.State, row.Message, row.Terminal };
        static ApplicationTaskRow Row(ApplicationOperation operation, ApplicationTaskState state)
        {
            var item = new ApplicationBatchItem(new("FEEDBACK-001", "自有摘要任务", 1, null), operation,
                operation is ApplicationOperation.List or ApplicationOperation.Install ? null : "com.matrix.feedback", operation == ApplicationOperation.Install ? "fixture.apk" : null);
            var row = new ApplicationTaskRow(item); row.SetResult(new(0, item, state, "feedback-original-message")); return row;
        }
        static void Product(bool value, string message) { if (!value) throw new ApplicationFeedbackProductFailure(message); }
        static void Contains(string actual, params string[] expected) { foreach (var value in expected) Product(actual.Contains(value, StringComparison.Ordinal), "原摘要应保留：" + value); }
        static void NoInstall(string actual) => Product(!actual.Contains("安装", StringComparison.Ordinal), "无安装未核验项时不显示安装专属零计数段");
        static void Require(bool value, string message) { if (!value) throw new ApplicationFeedbackPrecondition(message); }
    }
    private sealed class ApplicationFeedbackProductFailure(string message) : Exception(message);
    private sealed class ApplicationFeedbackPrecondition(string message) : Exception(message);
}
