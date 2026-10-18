using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using MatrixDevice.Core;
using MatrixDevice.Features;
using MatrixDevice.Host;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    private static readonly List<object> deletedDeviceRetainedOwners = [];

    internal static async Task RunDeletedDeviceVerificationAsync(string directory)
    {
        if (!Application.Current.Dispatcher.CheckAccess() || Application.Current.Windows.Count != 0 || Application.Current.MainWindow is not null)
            throw new InvalidOperationException("PRECONDITION-FAIL: deleted-devices requires its own STA with no existing window.");
        Directory.CreateDirectory(directory);
        MatrixRuntime? runtime = null; WorkspaceViewModel? vm = null; MainWindow? window = null;
        var tasks = new List<(string Name, Task Task)>(); var checks = new List<object>(); var productFailures = new List<string>();
        var cleanup = new List<string>(); Exception? error = null; object? before = null; object? after = null;
        var closed = false; var shutdownRequests = 0; var console = new DeletedDevicesConsole(); var ownershipUnresolved = false;
        var backend = new DemoDeviceBackend();
        var constructionOwners = new List<(string Name, Func<Task> Dispose)> { ("backend", () => backend.DisposeAsync().AsTask()) };
        Task Track(string name, Task task) { tasks.Add((name, task)); return task; }
        void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException("FIXTURE: " + message); }
        void Check(string name, bool passed) { checks.Add(new { name, passed }); if (!passed) productFailures.Add(name); }
        try
        {
            backend.SetOnline("DEMO-002", false);
            var settings = new AppSettings { AutoConnect = false, AutoStartApi = false, SyncMouse = false, SyncKeyboard = false,
                AgentBaseUrl = null, AgentSerial = null, AdbPath = "", LdConsolePath = "", LdSerialBindings = new() { [1] = "DEMO-001" } };
            var catalog = new DeviceCatalog(backend); var writes = new DeviceWriteCoordinator();
            var automation = new AutomationService(backend, catalog);
            var metadata = new CatalogRepository(Path.Combine(directory, "db.sqlite3")); constructionOwners.Add(("metadata", () => { metadata.Dispose(); return Task.CompletedTask; }));
            var log = new RollingLog(Path.Combine(directory, "logs")); constructionOwners.Add(("log", () => log.DisposeAsync().AsTask()));
            var service = new LdPlayerService(console, true); constructionOwners.Add(("ld-directory", () => service.DisposeAsync().AsTask()));
            var scripts = new ScriptScheduler(backend, catalog, true, writes); constructionOwners.Add(("scripts", () => scripts.DisposeAsync().AsTask()));
            var input = new InputCoordinator(catalog, backend, writes); constructionOwners.Add(("input", () => input.DisposeAsync().AsTask()));
            var api = new ApiServiceManager(catalog, backend, automation, log, writes) { LdPlayer = service }; constructionOwners.Add(("api", () => api.DisposeAsync().AsTask()));
            runtime = new MatrixRuntime
            {
                Backend = backend, Devices = catalog, Writes = writes, Automation = automation, Scripts = scripts, Input = input, Api = api,
                SettingsStore = new SettingsStore(Path.Combine(directory, "AppConfig.json")), Metadata = metadata,
                Log = log, Settings = settings, FrozenConnectionSettings = settings with { LdSerialBindings = new(settings.LdSerialBindings) }, LdPlayer = service, IsDemo = true
            };
            constructionOwners.Clear();
            Require(runtime.IsDemo && runtime.Backend is DemoDeviceBackend, "Only the owned Demo backend may be constructed.");
            vm = new WorkspaceViewModel(runtime, directory, false);
            await Track("initial-devices", vm.RefreshDevicesAsync()).WaitAsync(TimeSpan.FromSeconds(10));
            await Track("initial-directory", runtime.RefreshLdPlayerAsync()).WaitAsync(TimeSpan.FromSeconds(10));
            var bound = vm.Devices.Single(row => row.Serial == "DEMO-001"); var ordinary = vm.Devices.Single(row => row.Serial == "DEMO-002");
            bound.IsSelected = true;
            await Track("original-set-main", vm.SetMainAsync(bound)).WaitAsync(TimeSpan.FromSeconds(10));
            Require(vm.MainDevice == bound && vm.CurrentDevice == bound && bound.Info.Online, "Original main/current setup failed.");
            backend.SetOnline("DEMO-001", false);
            await Track("offline-catalog", runtime.RefreshAsync()).WaitAsync(TimeSpan.FromSeconds(10));
            // Invoke the same method on both private-red and internal-green production sources.
            var apply = typeof(WorkspaceViewModel).GetMethod("ApplyCachedDeviceSnapshotAsync", BindingFlags.Instance | BindingFlags.NonPublic)
                ?? throw new InvalidOperationException("FIXTURE: Original cached snapshot method is missing.");
            await Track("original-apply-cached", (Task)(apply.Invoke(vm, null) ?? throw new InvalidOperationException("FIXTURE: Cached snapshot returned no Task."))).WaitAsync(TimeSpan.FromSeconds(10));
            Require(!bound.Info.Online && !ordinary.Info.Online && vm.MainDevice == bound && vm.CurrentDevice == bound && vm.Targets().SequenceEqual([bound]), "Stale offline selection was not established.");
            window = new MainWindow(vm) { ShowActivated = false };
            window.captureTimer.Stop(); window.directoryTimer.Stop(); window.monitorTimer.Stop(); window.closing = true;
            window.applicationReadPending = false; window.applicationReadTimer?.Stop(); vm.DeviceChanged = null; vm.CurrentDebugReadyChanged = null;
            window.Closed += (_, _) => closed = true; Application.Current.MainWindow = window;
            window.UpdateUnifiedDirectory();
            Require(window.unifiedDevices.Count == 2 && window.unifiedDevices.Single(row => row.Index == 1).Device == bound && window.deviceSummaries.Count > 0, "Original LD and ordinary rows were not established.");
            window.unifiedDevices.Single(row => row.Index == 1).IsSelected = true;
            Require(bound.IsSelected && window.unifiedDevices.Single(row => row.Index == 1).IsSelected && vm.CurrentDevice == bound && vm.MainDevice == bound
                && vm.Targets().Count == 0 && vm.TargetContext == "已选目标包含 1 台未连接设备", "Stale offline selection must exist and remain gated after owned directory setup.");
            before = new { devices = vm.Devices.Select(row => row.Serial).ToArray(), keys = window.unifiedDevices.Select(row => row.Key).ToArray(), current = vm.CurrentDevice?.Serial, main = vm.MainDevice?.Serial, targets = vm.Targets().Select(row => row.Serial).ToArray() };
            var originalCatalog = runtime.Devices.Snapshot.ToArray();
            console.List = ""; window.lastGuestDiscovery = DateTimeOffset.UtcNow.AddHours(1);
            window.SetLaunchFeedback("已启动并连接 1 台设备。");
            await Track("original-directory-refresh", window.RefreshDirectoryCoreAsync()).WaitAsync(TimeSpan.FromSeconds(10));
            Check("successful-empty-ld-directory", runtime.LdPlayerError is null && service.Instances.Count == 0);
            Check("deleted-offline-bound-row-removed", !vm.Devices.Contains(bound));
            Check("ordinary-offline-row-reference-preserved", vm.Devices.Count == 1 && ReferenceEquals(vm.Devices[0], ordinary) && !ordinary.Info.Online);
            Check("old-current-cleared", vm.CurrentDevice is null);
            Check("old-main-cleared", vm.MainDevice is null);
            Check("old-target-selection-cleared", vm.Targets().Count == 0 && vm.Devices.All(row => !row.IsSelected));
            Check("unified-directory-has-only-ordinary-row", window.unifiedDevices.Count == 1 && window.unifiedDevices[0].Instance is null && ReferenceEquals(window.unifiedDevices[0].Device, ordinary) && !window.unifiedDevices[0].IsSelected);
            Check("summary-reports-zero-local-instances", window.deviceSummaries.All(summary => summary.Text.StartsWith("本机 0 台", StringComparison.Ordinal)));
            Check("old-connected-success-feedback-cleared", !window.ldFeedback.Text.Contains("已启动并连接", StringComparison.Ordinal) && !window.ldFeedback.Text.StartsWith("自动更新失败：", StringComparison.Ordinal));
            Check("catalog-identities-and-generations-unchanged", runtime.Devices.Snapshot.SequenceEqual(originalCatalog));
            Check("frozen-binding-not-deleted", runtime.FrozenConnectionSettings.LdSerialBindings[1] == "DEMO-001" && runtime.Settings.LdSerialBindings[1] == "DEMO-001");
            Check("fake-console-only-list2", console.Calls.Count == 2 && console.Calls.All(call => call.SequenceEqual(["list2"])));
            after = new { devices = vm.Devices.Select(row => row.Serial).ToArray(), keys = window.unifiedDevices.Select(row => row.Key).ToArray(), current = vm.CurrentDevice?.Serial, main = vm.MainDevice?.Serial, targets = vm.Targets().Select(row => row.Serial).ToArray(), feedback = window.ldFeedback.Text };
        }
        catch (Exception failure) { error = failure; }
        finally
        {
            if (window is not null)
            {
                window.closing = true; window.captureTimer.Stop(); window.directoryTimer.Stop(); window.monitorTimer.Stop(); window.directoryCancellation.Cancel();
                window.applicationReadPending = false; window.applicationReadTimer?.Stop();
                _ = Track("window-stop-monitor", window.StopMonitorAsync());
                foreach (var operation in new[] { window.directoryOperation, window.captureOperation, window.debugAutoOperation, window.monitorReconcile, window.monitorSampling, window.scanOperation, window.mainGestureOperation })
                    if (operation is not null && !tasks.Any(item => ReferenceEquals(item.Task, operation))) _ = Track("original-window-operation", operation);
            }
            foreach (var item in tasks.ToArray()) try { await item.Task.WaitAsync(TimeSpan.FromSeconds(15)); } catch (Exception failure) { cleanup.Add(item.Name + ": " + failure); }
            if (tasks.All(item => item.Task.IsCompleted))
            {
                shutdownRequests++;
                if (runtime is null)
                {
                    foreach (var resource in constructionOwners.AsEnumerable().Reverse())
                    {
                        try { await Track("partial-construction-" + resource.Name, resource.Dispose()).WaitAsync(TimeSpan.FromSeconds(15)); }
                        catch (Exception failure) { cleanup.Add(resource.Name + ": " + failure); ownershipUnresolved = true; break; }
                    }
                }
                else
                {
                    try
                    {
                        var shutdown = vm is not null ? vm.ShutdownAsync() : runtime.DisposeAsync().AsTask();
                        await Track("single-owner-shutdown", shutdown).WaitAsync(TimeSpan.FromSeconds(15));
                    }
                    catch (Exception failure) { cleanup.Add("shutdown: " + failure); ownershipUnresolved = true; }
                }
                if (!ownershipUnresolved && tasks.All(item => item.Task.IsCompleted) && window is not null)
                {
                    try
                    {
                        window.AbortStartup(); LocalizationService.Current.LanguageChanged -= window.OnLanguageChanged;
                        if (!closed || window.IsVisible) cleanup.Add("Owned window did not close.");
                        if (ReferenceEquals(Application.Current.MainWindow, window)) Application.Current.MainWindow = null;
                    }
                    catch (Exception failure) { cleanup.Add("close: " + failure); }
                }
            }
            if (ownershipUnresolved || tasks.Any(item => !item.Task.IsCompleted)) deletedDeviceRetainedOwners.Add(new { runtime, vm, window, tasks, resources = constructionOwners.ToArray() });
        }
        var passed = error is null && productFailures.Count == 0 && cleanup.Count == 0 && deletedDeviceRetainedOwners.Count == 0 && closed && shutdownRequests == 1 && Application.Current.Windows.Count == 0 && Application.Current.MainWindow is null;
        await File.WriteAllTextAsync(Path.Combine(directory, "deleted-devices.json"), JsonSerializer.Serialize(new
        {
            status = passed ? "PASS-OWN-WPF-DELETED-DEVICES" : "FAIL", checks, productFailures, error = error?.ToString(), cleanup, before, after,
            tasks = tasks.Select(item => new { item.Name, status = item.Task.Status.ToString(), terminal = item.Task.IsCompleted }).ToArray(),
            shutdownRequests, closed, retainedOwners = deletedDeviceRetainedOwners.Count, windows = Application.Current.Windows.Count, mainWindowNull = Application.Current.MainWindow is null,
            scope = "Unshown owned WPF window, real non-auto-select VM and Demo backend. Original cached projection and directory refresh; no native GUI, real LD/ADB, emulator mutation or A07 lifecycle.",
            notRun = new[] { "physical GUI", "real emulator/ADB", "native input", "A07 lifecycle", "full-goal acceptance" }
        }, JsonDefaults.Options));
        if (!passed) throw new InvalidOperationException("Deleted-devices verification failed; inspect deleted-devices.json.", error);
    }

    private sealed class DeletedDevicesConsole : ILdConsoleRunner
    {
        internal string List = "1,bound-demo,0,0,0,0,0,360,640,240";
        internal List<string[]> Calls { get; } = [];
        public Task<string> RunAsync(IReadOnlyList<string> arguments, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested(); Calls.Add(arguments.ToArray());
            if (!arguments.SequenceEqual(["list2"])) throw new InvalidOperationException("Only fake list2 is permitted.");
            return Task.FromResult(List);
        }
    }

    private async Task VerifyUnifiedDeviceActionsAsync(string? screenshot)
    {
        if (!ViewModel.Demo) throw new InvalidOperationException("统一实例操作验收仅限演示模式。");
        NavigateTo("设备墙 / 主控"); SetWallViewMode("list"); UpdateUnifiedDirectory();
        await Dispatcher.InvokeAsync(UpdateLayout, DispatcherPriority.Render);
        var previousStatus = deviceStatusFilter;
        var previousCurrent = ViewModel.CurrentDevice;
        var previousMain = ViewModel.MainDevice;
        var selection = unifiedDevices.ToDictionary(row => row.Key, row => row.IsSelected);
        var cold = unifiedDevices.Single(row => row.Instance is { Index: > 0, AndroidStarted: false });
        var other = unifiedDevices.First(row => row.Instance is null && row.Device is not null);
        var otherInfo = other.Device!.Info;
        var more = VisualDescendants<Button>(Shell).Single(button => button.Name == "InstanceActions");
        var menu = more.ContextMenu ?? throw new InvalidOperationException("更多操作按钮没有实际菜单。");
        var menuActions = new[] { "资源配置", "关闭实例", "重启实例" }.ToDictionary(label => label,
            label => menu.Items.OfType<MenuItem>().Single(item => item.Header as string == label));
        if (menuActions.Any(pair => !ReferenceEquals(pair.Value.Command, ldCommandsByLabel[pair.Key])))
            throw new InvalidOperationException("更多菜单条目没有绑定实际生命周期命令。");
        async Task CheckMenuAsync(bool configuration, bool running)
        {
            menu.IsOpen = false;
            await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
            more.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
            var opened = System.Diagnostics.Stopwatch.StartNew();
            while (true)
            {
                await Dispatcher.InvokeAsync(() => { UpdateLayout(); menu.UpdateLayout(); }, DispatcherPriority.ApplicationIdle);
                if (menu.IsOpen && menu.IsLoaded && menu.IsVisible && menu.ActualWidth > 0 && menu.ActualHeight > 0) break;
                if (opened.Elapsed >= TimeSpan.FromSeconds(1))
                    throw new InvalidOperationException($"更多菜单打开超时：open={menu.IsOpen}, loaded={menu.IsLoaded}, visible={menu.IsVisible}, size={menu.ActualWidth}x{menu.ActualHeight}, active={IsActive}。");
                await Task.Delay(20);
            }
            foreach (var pair in menuActions)
            {
                var expected = pair.Key == "资源配置" ? configuration : running;
                if (pair.Value.Command!.CanExecute(null) != expected || pair.Value.IsEnabled != expected)
                    throw new InvalidOperationException("更多菜单的可用状态与实际实例目标不一致：" + pair.Key);
            }
        }
        try
        {
            ClearWallSelection(); await CheckMenuAsync(false, false);
            cold.IsSelected = true; await CheckMenuAsync(true, false);
            if (ldSelection is not { } coldSelection || coldSelection.Index != cold.Index || coldSelection.Generation != cold.Instance?.Generation)
                throw new InvalidOperationException("统一目录单选与实际实例命令目标不一致。");
            cold.IsSelected = false; other.IsSelected = true;
            if (ldSelection is not null || ldCommandsByLabel["资源配置"].CanExecute(null) || ldCommandsByLabel["关闭实例"].CanExecute(null))
                throw new InvalidOperationException("非雷电行仍沿用旧实例目标。");
            if (!ReferenceEquals(previousCurrent, ViewModel.CurrentDevice) || !ReferenceEquals(previousMain, ViewModel.MainDevice)
                || !other.Device.IsSelected)
                throw new InvalidOperationException("目录勾选改写当前设备、主控或没有同步对应群控目标。");

            cold.IsSelected = true;
            await CheckMenuAsync(false, false);
            if (ldSelection is not null || ldCommandsByLabel["资源配置"].CanExecute(null))
                throw new InvalidOperationException("多选目录仍保留单实例命令目标。");
            other.Device.Info = otherInfo with { State = "Ready" };
            deviceStatusChoice.SelectedItem = "已连接";
            await Dispatcher.InvokeAsync(UpdateLayout, DispatcherPriority.Render);
            if (unifiedDeviceView.Cast<UnifiedDeviceRow>().Contains(cold)) throw new InvalidOperationException("隐藏目标反例没有形成。");
            other.IsSelected = false;
            if (!cold.IsSelected || ldSelection is not { } hiddenSelection || hiddenSelection.Index != cold.Index || hiddenSelection.Generation != cold.Instance?.Generation)
                throw new InvalidOperationException("状态筛选后的唯一隐藏勾选目标与实例命令快照不一致。");
            var selectAll = VisualDescendants<CheckBox>(Shell).Single(input => input.Name == "SelectAllDevices");
            selectAll.IsChecked = false; selectAll.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
            if (unifiedDevices.Any(row => row.IsSelected) || ldSelection is not null)
                throw new InvalidOperationException("表头取消选择没有清除隐藏目标或实例命令快照。");

            var beforeEmptyInfos = ViewModel.Devices.ToDictionary(row => row, row => row.Info);
            var beforeEmptyStatus = deviceStatusFilter;
            try
            {
                ClearWallSelection(); cold.IsSelected = true;
                foreach (var row in ViewModel.Devices) row.Info = row.Info with { State = "Ready" };
                deviceStatusChoice.SelectedItem = "离线";
                await Dispatcher.InvokeAsync(UpdateLayout, DispatcherPriority.Render);
                if (!unifiedDeviceView.IsEmpty || !cold.IsSelected || selectAll.IsChecked is not null || !selectAll.IsEnabled)
                    throw new InvalidOperationException("空可见列表的隐藏勾选未显示为可取消的表头中间态。");
                selectAll.IsChecked = false; selectAll.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
                if (unifiedDevices.Any(row => row.IsSelected) || ViewModel.Devices.Any(row => row.IsSelected) || ldSelection is not null)
                    throw new InvalidOperationException("空可见列表中的表头取消未清除隐藏目标或实例命令快照。");
            }
            finally
            {
                foreach (var pair in beforeEmptyInfos) pair.Key.Info = pair.Value;
                deviceStatusChoice.SelectedItem = beforeEmptyStatus;
            }

            deviceStatusChoice.SelectedItem = previousStatus;
            other.Device.Info = otherInfo;
            var confirm = ViewModel.Confirm;
            ViewModel.Confirm = (_, _) => true;
            try
            {
                cold.IsSelected = true;
                await ExecuteLdPlayerAsync("launch");
                var running = ViewModel.Runtime.LdPlayer!.Instances.Single(item => item.Index == cold.Index);
                var projected = unifiedDevices.Single(row => row.Index == cold.Index);
                if (!running.AndroidStarted || projected.Instance != running || projected.AndroidState != "已启动" || projected.ProcessId != running.ProcessId.ToString())
                    throw new InvalidOperationException("模拟命令后统一列表仍显示旧进程或安卓状态。");
                await CheckMenuAsync(false, true);
                if (screenshot is not null) SaveRender(Path.Combine(Path.GetDirectoryName(screenshot)!, "instance-actions-menu.png"), menu);
                await ExecuteLdPlayerAsync("quit");
                var stopped = ViewModel.Runtime.LdPlayer!.Instances.Single(item => item.Index == cold.Index);
                if (stopped.AndroidStarted || projected.Instance != stopped || projected.ProcessId != "-")
                    throw new InvalidOperationException("模拟关闭后统一列表未即时更新。");
            }
            finally { ViewModel.Confirm = confirm; }
            wallActionsViewport.ScrollToTop();
            await Dispatcher.InvokeAsync(UpdateLayout, DispatcherPriority.Render);
            await Task.Delay(60);
            if (screenshot is not null) SaveRender(Path.Combine(Path.GetDirectoryName(screenshot)!, "merged-device-actions.png"));
            var settings = VisualDescendants<Button>(Shell).Single(input => input.Name == "WorkspaceSettings");
            var settingsTop = settings.TranslatePoint(new Point(), Shell).Y;
            var listTop = wallWorkspace.TranslatePoint(new Point(), Shell).Y;
            var footer = workspace.Children.OfType<FrameworkElement>().Single(child => Grid.GetRow(child) == 3);
            var footerTop = footer.TranslatePoint(new Point(), Shell).Y;
            var visibleListHeight = Math.Min(listTop + wallWorkspace.ActualHeight, footerTop) - Math.Max(listTop, 0);
            if (wallWorkspace.ActualHeight < 140 || visibleListHeight < 140 || listTop + wallWorkspace.ActualHeight > footerTop + 1
                || settingsTop < 0 || settingsTop + settings.ActualHeight > Shell.ActualHeight)
                throw new InvalidOperationException($"统一页操作区挤占列表或裁切全局工具栏：列表高 {wallWorkspace.ActualHeight:F1}，设置顶 {settingsTop:F1}。");
            var create = VisualDescendants<Button>(Shell).Single(button => button.ToolTip as string == "新建空白实例");
            var actionTop = create.TranslatePoint(new Point(), wallActionsViewport).Y;
            if (actionTop < 0 || actionTop + create.ActualHeight > wallActionsViewport.ActualHeight)
                throw new InvalidOperationException("实例生命周期操作超出可见操作区。");
        }
        finally
        {
            menu.IsOpen = false;
            wallActionsViewport.ScrollToTop();
            other.Device.Info = otherInfo;
            deviceStatusChoice.SelectedItem = previousStatus;
            foreach (var row in unifiedDevices.ToArray()) row.IsSelected = selection.GetValueOrDefault(row.Key);
        }
    }
}
