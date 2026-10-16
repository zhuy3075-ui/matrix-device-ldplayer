using System.IO;
using System.Runtime.ExceptionServices;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using MatrixDevice.Core;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    internal async Task VerifyWorkspaceAccessibilityAsync()
    {
        var directory = Path.Combine(ViewModel.DataDirectory, "workspace-accessibility");
        if (!ViewModel.Demo || !ViewModel.Runtime.IsDemo || ViewModel.Runtime.Backend is not DemoDeviceBackend
            || !Path.IsPathFullyQualified(directory) || !directory.Contains(Path.DirectorySeparatorChar + "artifacts" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
            || Directory.Exists(directory) || File.Exists(directory)) throw new InvalidOperationException("工作区无障碍契约仅限 fresh artifacts Demo profile 内自有新子目录。");
        Directory.CreateDirectory(directory);
        var phases = new[] { "设备墙 / 主控", "应用", "设备墙 / 主控" };
        var reports = new List<object>(); var checks = new List<string>(); var failures = new List<Exception>(); var failureClasses = new HashSet<string>(StringComparer.Ordinal);
        var tasks = new List<Task>(); var identities = new Dictionary<object, int>(ReferenceEqualityComparer.Instance); using var lifetime = new CancellationTokenSource();
        var originalTab = tabs.SelectedItem; var originalTemplate = tabs.Template; var originalStyle = tabs.Style; var originalPadding = tabs.Padding;
        var originalTabs = tabs.Items.Cast<TabItem>().ToArray(); var originalContents = originalTabs.Select(tab => (FrameworkElement)tab.Content).ToArray();
        var originalTag = (originalTab as TabItem)?.Tag as string; var current = ViewModel.CurrentDevice; var main = ViewModel.MainDevice;
        var rows = ViewModel.Devices.ToArray(); var infos = rows.Select(row => row.Info).ToArray(); var selected = rows.Select(row => row.IsSelected).ToArray();
        var unified = unifiedDevices.ToArray(); var unifiedSelection = unified.Select(row => row.IsSelected).ToArray();
        var appTargets = applicationTargets.ToArray(); var appSelection = appTargets.Select(row => row.IsSelected).ToArray();
        var text = ViewModel.TextInput; var shell = ViewModel.ShellInput; var generation = ViewModel.Runtime.Input.CurrentGeneration; var filter = deviceStatusFilter;
        var gestureDispatches = mainGestureDispatchCount; var captureDispatches = mainCaptureDispatchCount; var status = ViewModel.Status; var businessError = ViewModel.Error;
        var readTimer = applicationReadTimer; var reconcile = monitorReconcile; var sampling = monitorSampling;
        var stage = "precondition"; var prepared = false; var restored = false; var drained = false; AutomationPeer? rootPeer = null; ContentPresenter? presenter = null;
        Exception? failure = null;
        try
        {
            Require(Dispatcher.CheckAccess() && Thread.CurrentThread.GetApartmentState() == ApartmentState.STA && !ShowActivated, "自有 STA 窗口，不请求激活");
            Require(originalTag == phases[0] && !navigationDrawerOpen && !mainControlOpen && gestureStart is null && !mainControlImage.IsMouseCaptured && mainGestureOperation.IsCompleted, "初始设备页且无导航浮层/主控/手势任务");
            Require(originalTabs.Length == 12 && WorkspacePages.Length == 12 && navigation.Count == 12 && originalTabs.Select(tab => (string)tab.Tag).ToHashSet(StringComparer.Ordinal).SetEquals(WorkspacePages.Select(page => page.Tag)) && navigation.Keys.ToHashSet(StringComparer.Ordinal).SetEquals(WorkspacePages.Select(page => page.Tag)), "仅检查原十二页和导航结构，不访问其它十页");
            Scheduling();
            await Flush();
            Require(IsLoaded && IsVisible, "原固定 ApplicationIdle 推进后窗口真实加载可见"); originalStyle = tabs.Style; originalPadding = tabs.Padding;
            presenter = tabs.Template.FindName("PART_SelectedContentHost", tabs) as ContentPresenter ?? throw new WorkspaceAccessibilityPrecondition("原选中内容宿主不存在");
            Require(ReferenceEquals(presenter.Content, originalTab is TabItem first ? first.Content : null), "真实宿主绑定到原选中正文");
            rootPeer = UIElementAutomationPeer.CreatePeerForElement(tabs) ?? throw new WorkspaceAccessibilityPrecondition("真实 tabs 未创建根 peer");
            Require(rootPeer is UIElementAutomationPeer elementPeer && ReferenceEquals(elementPeer.Owner, tabs), "根 peer 确实由实际 tabs 创建，不另造正文或叶子 peer");
            prepared = true;
            for (var index = 0; index < phases.Length; index++)
            {
                stage = "page-" + index + "-" + phases[index];
                NavigateTo(phases[index]); await Flush(); Scheduling(); Context();
                var tab = (TabItem)tabs.SelectedItem; var content = (FrameworkElement)tab.Content;
                Require((string)tab.Tag == phases[index] && ReferenceEquals(presenter.Content, content) && ReferenceEquals(tabs.Template.FindName("PART_SelectedContentHost", tabs), presenter) && content.IsLoaded && content.IsVisible && content.ActualWidth > 0 && content.ActualHeight > 0, "当前正文真实可见且原宿主/SelectedItem/Tag一致：" + phases[index]);
                var readButton = phases[index] == "应用" ? VisualDescendants<Button>(content).Single(button => ReferenceEquals(button.Command, applicationCommands[0])) : null;
                var expected = phases[index] == "应用" ? new FrameworkElement[] { readButton!, applicationCancel, applicationSystemToggle } : new FrameworkElement[] { wallSelectAll };
                foreach (var control in expected) Require(content.IsAncestorOf(control) && control.IsLoaded && control.IsVisible && control.ActualWidth > 0 && control.ActualHeight > 0, "预期控件确属当前真实可视正文：" + control.GetType().Name + "/" + control.Name);
                var tree = ReadTree(rootPeer); var descendants = tree.Skip(1).ToArray();
                var residue = descendants.Where(node => node.Owner is Visual owner && originalContents.Where(page => !ReferenceEquals(page, content)).Any(page => page.IsAncestorOf(owner))).ToArray();
                var expectedResults = new List<object>();
                foreach (var control in expected)
                {
                    var matches = descendants.Where(node => ReferenceEquals(node.Owner, control)).ToArray();
                    var actual = matches.FirstOrDefault(); var name = AutomationProperties.GetName(control);
                    if (name.Length == 0 && control is ContentControl { Content: string caption }) name = caption;
                    var id = AutomationProperties.GetAutomationId(control); if (id.Length == 0) id = control.Name;
                    var role = control is CheckBox ? AutomationControlType.CheckBox : AutomationControlType.Button;
                    Product(matches.Length == 1, "实际根 peer 可达唯一正文 owner：" + control.GetType().Name + "/" + id + "/" + name);
                    if (actual is not null) Product(actual.Name == name && (id.Length == 0 || actual.AutomationId == id) && actual.Role == role.ToString() && actual.Enabled == control.IsEnabled, "正文 peer 的真实名称/ID/角色/禁用状态与原控件一致：" + id + "/" + name);
                    expectedResults.Add(new { ownerId = Id(control), type = control.GetType().FullName, expectedName = name, expectedAutomationId = id, expectedRole = role.ToString(), control.IsEnabled, matches = matches.Length, actual = actual is null ? null : Node(actual) });
                }
                Product(residue.Length == 0, "当前根树没有任何前页实际 owner 残留：" + phases[index]);
                Product(ReferenceEquals(UIElementAutomationPeer.FromElement(tabs), rootPeer), "三阶段使用同一个实际根 peer，无手动缓存失效或替换");
                Product(ReferenceEquals(tabs.Style, Application.Current.TryFindResource(typeof(TabControl))) && tabs.Padding == originalPadding, "原 TabControl 隐式 Style 与 Padding 保留");
                if (readButton is not null) Product(!readButton.IsEnabled && !applicationCancel.IsEnabled && applicationSystemToggle.IsEnabled, "无目标读取/停止后续仍禁用，系统应用开关仍可用，peer不授信");
                reports.Add(new { stage, tag = phases[index], selectedTabId = Id(tab), contentId = Id(content), presenterId = Id(presenter), rootPeerId = Id(rootPeer), rootPeerType = rootPeer.GetType().FullName, rootRole = rootPeer.GetAutomationControlType().ToString(), selectedContentWidth = content.ActualWidth, selectedContentHeight = content.ActualHeight, hostWidth = presenter.ActualWidth, hostHeight = presenter.ActualHeight, expectedResults, residue = residue.Select(Node).ToArray(), nodes = tree.Select(Node).ToArray(), schedulingGatesHeld = true });
            }
        }
        catch (Exception error) { failures.Add(error); failureClasses.Add(error is WorkspaceAccessibilityPrecondition ? "PRECONDITION-FAIL" : "FIXTURE-FAIL"); }
        finally
        {
            try
            {
                stage = "cleanup";
                if (prepared && originalTag is not null) { NavigateTo(originalTag); await Flush(); }
                Context(); Scheduling();
                Require(ReferenceEquals(tabs.SelectedItem, originalTab), "恢复原选中 TabItem 引用");
                restored = true;
            }
            catch (Exception error) { failures.Add(error); failureClasses.Add("CLEANUP-FAIL"); }
            finally
            {
                lifetime.Cancel();
                try { await Task.WhenAll(tasks).WaitAsync(TimeSpan.FromSeconds(5)); Require(tasks.All(task => task.IsCompleted), "全部自有 Dispatcher 原任务已进入终态"); drained = true; }
                catch (Exception error) { failures.Add(error); failureClasses.Add("CLEANUP-FAIL"); }
            }
            if (failures.Count != 0) failure = new AggregateException("工作区正文无障碍契约未全部通过", failures);
            failure = SaveDeviceLayoutReport(() => Path.Combine(directory, "summary.json"), () => new
            {
                status = failure is null ? "PASS-WORKSPACE-ACCESSIBILITY" : failureClasses.Count == 1 ? failureClasses.Single() : "MIXED-FAIL", failureClasses = failureClasses.OrderBy(value => value).ToArray(), reports, checks,
                plannedPages = phases, completedPageCount = reports.Count, notRunPages = phases.Skip(reports.Count).ToArray(), error = failure?.ToString(), restored, drained,
                tasks = tasks.Select(task => new { status = task.Status.ToString(), task.IsCompleted }).ToArray(), offlineGeometry = "NOT-RUN", offlineGeometryReason = failure is null ? "App calls the unchanged diagnostic after this fixture returns" : "Accessibility failure blocks the following diagnostic",
                scope = "Actual tabs root peer, selected real owner/ID/name/state and Device/Apps/Device only; twelve navigation entries checked structurally. No peer Invoke/selection/value pattern, focus, activation, mouse/native input, backend method invocation or other ten-page navigation. Original no-target/pending/timer/API/debug/monitor scheduling gates remain closed; this is not ADB wire instrumentation or external Sky acceptance. Hidden model SelectionPattern is not asserted as a public contract; root role is observed, not the red failure oracle."
            }, failure);
        }
        if (failure is not null) ExceptionDispatchInfo.Capture(failure).Throw();

        void Require(bool value, string message) { if (!value) throw new WorkspaceAccessibilityPrecondition(stage + ": " + message); checks.Add("PRECONDITION " + message); }
        void Product(bool value, string message) { checks.Add((value ? "PRODUCT PASS " : "PRODUCT FAIL ") + message); if (!value) { failures.Add(new InvalidOperationException(stage + ": " + message)); failureClasses.Add("PRODUCT-FAIL"); } }
        int Id(object? value) { if (value is null) return 0; if (!identities.TryGetValue(value, out var id)) { id = identities.Count + 1; identities.Add(value, id); } return id; }
        async Task Flush()
        {
            using var deadline = CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token); deadline.CancelAfter(TimeSpan.FromSeconds(5));
            var task = Dispatcher.InvokeAsync(UpdateLayout, DispatcherPriority.ApplicationIdle, deadline.Token).Task; tasks.Add(task);
            try { await task.WaitAsync(TimeSpan.FromSeconds(5)); } catch { deadline.Cancel(); throw; }
        }
        void Scheduling()
        {
            Require(!ViewModel.Busy && !ViewModel.Transferring && !ViewModel.HasActiveOperations && !ViewModel.Runtime.Api.BasicRunning && !ViewModel.Runtime.Api.AutomatorRunning && !ViewModel.Runtime.Api.HasActiveRequests && !ViewModel.Runtime.Settings.AutoStartApi && !ViewModel.Runtime.Settings.AutoConnect, "业务/接口/自动接入关闭");
            Require(!applicationRunning && !applicationReadPending && applicationReadTimer?.IsEnabled != true && applicationTargets.All(target => !target.IsSelected) && applicationTasks.Count == 0 && applicationRows.Count == 0, "应用无目标且无待执行读取；Loaded/可见变化不能消费任何 List 意图");
            Require(!DebugAutomaticFixturePermitted && !ReconnectLoadingFixturePermitted && !captureInProgress && captureOperation is null && !monitorWasActive && !monitorTimer.IsEnabled && monitorReconcile.IsCompleted && monitorSampling.IsCompleted, "Demo截图/节点许可关闭、未进入监控且无原后台任务");
        }
        void Context()
        {
            Require(ReferenceEquals(ViewModel.CurrentDevice, current) && ReferenceEquals(ViewModel.MainDevice, main) && rows.SequenceEqual(ViewModel.Devices) && rows.Select(row => row.Info).Zip(infos).All(pair => ReferenceEquals(pair.First, pair.Second)) && selected.SequenceEqual(rows.Select(row => row.IsSelected)) && unified.SequenceEqual(unifiedDevices) && unifiedSelection.SequenceEqual(unified.Select(row => row.IsSelected)), "Current/Main/行与Info引用/群控勾选保持");
            Require(appTargets.SequenceEqual(applicationTargets) && appSelection.SequenceEqual(appTargets.Select(row => row.IsSelected)) && ReferenceEquals(applicationReadTimer, readTimer) && ReferenceEquals(monitorReconcile, reconcile) && ReferenceEquals(monitorSampling, sampling) && ViewModel.TextInput == text && ViewModel.ShellInput == shell && ViewModel.Runtime.Input.CurrentGeneration == generation && deviceStatusFilter == filter, "应用勾选/原调度任务/输入与代次/筛选保持");
            Require(mainGestureDispatchCount == gestureDispatches && mainCaptureDispatchCount == captureDispatches && ViewModel.Status == status && ViewModel.Error == businessError, "原输入/截图派发计数及业务状态未改变");
            Require(originalTabs.SequenceEqual(tabs.Items.Cast<TabItem>()) && originalContents.Zip(originalTabs).All(pair => ReferenceEquals(pair.First, pair.Second.Content)) && ReferenceEquals(tabs.Template, originalTemplate) && ReferenceEquals(tabs.Style, originalStyle) && tabs.Padding == originalPadding, "内部 TabItems/Content/Template/Style/Padding未被替换");
        }
        List<WorkspacePeerNode> ReadTree(AutomationPeer root)
        {
            var found = new List<WorkspacePeerNode>(); var seen = new HashSet<AutomationPeer>(ReferenceEqualityComparer.Instance); var pending = new Queue<(AutomationPeer Peer, int Parent, int Depth)>(); pending.Enqueue((root, 0, 0));
            while (pending.Count != 0)
            {
                var (peer, parent, depth) = pending.Dequeue(); Require(depth <= 64 && found.Count < 8192 && seen.Add(peer), "真实 peer 遍历有界且无重复/环");
                var owner = (peer as UIElementAutomationPeer)?.Owner;
                found.Add(new(peer, owner, parent, depth, peer.GetAutomationId(), peer.GetName(), peer.GetAutomationControlType().ToString(), peer.IsEnabled(), peer.IsOffscreen()));
                foreach (var child in peer.GetChildren() ?? []) pending.Enqueue((child, Id(peer), depth + 1));
            }
            return found;
        }
        object Node(WorkspacePeerNode node) => new { peerId = Id(node.Peer), ownerId = Id(node.Owner), node.Parent, node.Depth, peerType = node.Peer.GetType().FullName, ownerType = node.Owner?.GetType().FullName, ownerName = (node.Owner as FrameworkElement)?.Name, node.AutomationId, node.Name, node.Role, node.Enabled, node.Offscreen };
    }
    private sealed record WorkspacePeerNode(AutomationPeer Peer, UIElement? Owner, int Parent, int Depth, string AutomationId, string Name, string Role, bool Enabled, bool Offscreen);
    private sealed class WorkspaceAccessibilityPrecondition(string message) : InvalidOperationException(message) { }
}
