using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using MatrixDevice.Core;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    internal async Task RunDeviceLayoutAcceptanceAsync(string[] args)
    {
        if (!ViewModel.Demo || !Path.GetFullPath(ViewModel.DataDirectory).Contains(Path.DirectorySeparatorChar + "artifacts" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("设备正文验收仅限 artifacts 内隔离 Demo 工作区。");
        string? Argument(string flag) { var index = Array.IndexOf(args, flag); return index < 0 ? null : args[index + 1]; }
        var width = double.Parse(Argument("--width") ?? "1440", CultureInfo.InvariantCulture);
        var height = double.Parse(Argument("--height") ?? "900", CultureInfo.InvariantCulture);
        var screenshot = Argument("--screenshot");
        var checks = new List<string>(); var observations = new List<object>(); var phase = "initial";
        Exception? failure = null;
        void Check(bool condition, string message) { if (!condition) throw new InvalidOperationException(phase + ": " + message); checks.Add(message); }
        async Task Settle()
        {
            await Dispatcher.InvokeAsync(UpdateLayout, DispatcherPriority.Render);
            await Dispatcher.InvokeAsync(UpdateLayout, DispatcherPriority.ContextIdle);
            await Dispatcher.InvokeAsync(UpdateLayout, DispatcherPriority.ApplicationIdle);
        }
        async Task Until(Func<bool> condition, string message)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            while (!condition())
            {
                if (watch.Elapsed >= TimeSpan.FromSeconds(3)) throw new InvalidOperationException(phase + ": " + message);
                await Settle(); await Task.Delay(10);
            }
        }
        async Task Invoke(Button button, bool idle = true)
        {
            Check(button.IsEnabled && button.IsVisible && DeviceProbeContains(new Rect(Shell.RenderSize), DeviceProbeBounds(button, Shell)), "实际按钮可见且可执行: " + AutomationProperties.GetName(button));
            ((IInvokeProvider)new ButtonAutomationPeer(button).GetPattern(PatternInterface.Invoke)).Invoke();
            await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Input);
            if (idle) await Settle();
        }
        void Save(string suffix)
        {
            if (screenshot is null) return;
            var path = Path.Combine(Path.GetDirectoryName(screenshot)!, Path.GetFileNameWithoutExtension(screenshot) + "-" + suffix + ".png");
            Directory.CreateDirectory(Path.GetDirectoryName(path)!); SaveRender(path);
        }
        var originalTag = (tabs.SelectedItem as TabItem)?.Tag?.ToString() ?? "设备墙 / 主控";
        var originalFilter = deviceStatusFilter; var originalCurrent = ViewModel.CurrentDevice; var originalMain = ViewModel.MainDevice;
        var originalRows = unifiedDevices.ToArray(); var originalInfos = ViewModel.Devices.ToDictionary(row => row, row => row.Info);
        var originalSelection = originalRows.ToDictionary(row => row.Key, row => row.IsSelected);
        var originalSyncMouse = ViewModel.SyncMouse; var originalSyncKeyboard = ViewModel.SyncKeyboard;
        var originalUpdating = updatingUnifiedDirectory; var originalBarrier = demoMainCaptureBarrier;
        var captureDispatcherTimer = typeof(DispatcherTimerAdapter).GetField("timer", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.GetValue(captureTimer) as DispatcherTimer
            ?? throw new InvalidOperationException("已核验的captureTimer内部DispatcherTimer字段不存在，无法隔离专项计数。");
        var originalCaptureTimerEnabled = captureDispatcherTimer.IsEnabled;
        var gridIdentity = wallGrid; var sourceIdentity = wallGrid.ItemsSource; var panelIdentity = wallMainPanel;
        var a = ViewModel.Devices[0]; var b = ViewModel.Devices[1];
        var fixtureRows = new List<UnifiedDeviceRow>(); var fixtureInvocations = 0; Window? inactiveWindow = null;
        var contextEvents = new Queue<object>(); var contextEventCount = 0;
        var watchedDevices = new List<(DeviceRow Row, PropertyChangedEventHandler Handler)>();
        var watchedFixtures = new List<(UnifiedDeviceRow Row, PropertyChangedEventHandler Handler)>();
        int? ReferenceId(object? value) => value is null ? null : System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(value);
        string? InputIdentity(IInputElement? value) => value is null ? null : value.GetType().FullName + "#" + ReferenceId(value) + (value is FrameworkElement element ? "/" + element.Name + "/" + AutomationProperties.GetName(element) : "");
        void RecordContextEvent(string reason, string? property = null, object? sender = null, string? key = null, string? detail = null, string[]? addedKeys = null, string[]? removedKeys = null)
        {
            var main = ViewModel.MainDevice; var current = ViewModel.CurrentDevice;
            contextEventCount++;
            if (contextEvents.Count == 128) contextEvents.Dequeue();
            contextEvents.Enqueue(new
            {
                sequence = contextEventCount, phase, reason, property, senderId = ReferenceId(sender), key, detail, addedKeys, removedKeys, observedAt = DateTimeOffset.UtcNow,
                mainId = ReferenceId(main), mainSerial = main?.Serial, mainIsA = ReferenceEquals(main, a), currentId = ReferenceId(current), currentSerial = current?.Serial, currentIsB = ReferenceEquals(current, b),
                expectedAId = ReferenceId(a), expectedASerial = a.Serial, expectedBId = ReferenceId(b), expectedBSerial = b.Serial,
                inputGeneration = ViewModel.Runtime.Input.CurrentGeneration, active = IsActive, focus = InputIdentity(Keyboard.FocusedElement), capture = InputIdentity(Mouse.Captured),
                actualWidth = ActualWidth, actualHeight = ActualHeight, left = double.IsFinite(Left) ? (double?)Left : null, top = double.IsFinite(Top) ? (double?)Top : null,
                hiddenSelected = fixtureRows.Count > 0 ? (bool?)fixtureRows[0].IsSelected : null, deviceStatusFilter, updatingUnifiedDirectory, refreshingUnifiedFilter, selectedKey = (wallGrid.SelectedItem as UnifiedDeviceRow)?.Key, currentKey = (wallGrid.CurrentItem as UnifiedDeviceRow)?.Key,
                selectedDevices = ViewModel.Devices.Where(row => row.IsSelected).Select(row => new { id = ReferenceId(row), row.Serial }).ToArray(), selectedFixtureKeys = fixtureRows.Where(row => row.IsSelected).Select(row => row.Key).ToArray()
            });
        }
        EventHandler activatedObserver = (_, _) => RecordContextEvent("window-activated");
        EventHandler deactivatedObserver = (_, _) => RecordContextEvent("window-deactivated");
        EventHandler locationObserver = (_, _) => RecordContextEvent("window-location-changed");
        SizeChangedEventHandler sizeObserver = (_, change) => RecordContextEvent("window-size-changed", detail: change.PreviousSize.ToString(CultureInfo.InvariantCulture) + "->" + change.NewSize.ToString(CultureInfo.InvariantCulture));
        SelectionChangedEventHandler gridSelectionObserver = (_, change) => RecordContextEvent("grid-selection-changed", sender: wallGrid, detail: "originalSource=" + change.OriginalSource?.GetType().FullName,
            addedKeys: change.AddedItems.Cast<object>().Select(item => item is UnifiedDeviceRow row ? row.Key : item.GetType().FullName ?? item.GetType().Name).ToArray(),
            removedKeys: change.RemovedItems.Cast<object>().Select(item => item is UnifiedDeviceRow row ? row.Key : item.GetType().FullName ?? item.GetType().Name).ToArray());
        SelectionChangedEventHandler filterSelectionObserver = (_, _) => RecordContextEvent("filter-selection-changed", sender: deviceStatusChoice, detail: "selectedItem=" + deviceStatusChoice.SelectedItem?.ToString());
        PropertyChangedEventHandler viewModelObserver = (_, change) =>
        {
            if (change.PropertyName is nameof(WorkspaceViewModel.MainDevice) or nameof(WorkspaceViewModel.CurrentDevice)) RecordContextEvent("viewmodel-context-changed", change.PropertyName, ViewModel);
        };
        void WatchDevice(DeviceRow row)
        {
            if (watchedDevices.Any(item => ReferenceEquals(item.Row, row))) return;
            PropertyChangedEventHandler observer = (_, change) => { if (change.PropertyName == nameof(DeviceRow.IsSelected)) RecordContextEvent("device-selection-changed", change.PropertyName, row, row.Serial); };
            row.PropertyChanged += observer; watchedDevices.Add((row, observer));
        }
        void WatchFixture(UnifiedDeviceRow row)
        {
            PropertyChangedEventHandler observer = (_, change) => { if (change.PropertyName == nameof(UnifiedDeviceRow.IsSelected)) RecordContextEvent("fixture-selection-changed", change.PropertyName, row, row.Key); };
            row.PropertyChanged += observer; watchedFixtures.Add((row, observer));
        }
        if (!GetAppearanceCursorPosition(out var pointer)) throw new InvalidOperationException("无法保存验收指针。");
        try
        {
            Activated += activatedObserver; Deactivated += deactivatedObserver; SizeChanged += sizeObserver; LocationChanged += locationObserver; ViewModel.PropertyChanged += viewModelObserver; wallGrid.SelectionChanged += gridSelectionObserver; deviceStatusChoice.SelectionChanged += filterSelectionObserver;
            foreach (var device in ViewModel.Devices) WatchDevice(device);
            RecordContextEvent("fixture-observers-attached");
            captureTimer.Stop();
            Check(!captureDispatcherTimer.IsEnabled, "仅Demo专项暂停自动captureTimer；显式CaptureTickAsync仍走真实接纳路径");
            observations.Add(new { phase = "timer-isolation", originalCaptureTimerEnabled, actualCaptureTimerEnabled = captureDispatcherTimer.IsEnabled, scope = "Demo fixture isolation; not automatic-timer acceptance" });
            phase = "window"; await Settle(); Activate(); await Until(() => IsActive, "验收窗口未激活");
            Check(IsLoaded && IsVisible && new WindowInteropHelper(this).Handle != IntPtr.Zero && Math.Abs(ActualWidth - width) < .5 && Math.Abs(ActualHeight - height) < .5, "真实 HWND 与请求逻辑尺寸一致");
            NavigateTo("设备墙 / 主控"); CloseMainControl(); deviceStatusChoice.SelectedItem = "全部状态"; ClearWallSelection();
            await Settle();
            // These rows exercise the real templates, not discovery or emulator commands.
            updatingUnifiedDirectory = true;
            for (var index = 0; index < 34; index++)
            {
                var fixtureIndex = index; var protectedRow = index == 2; var connected = index % 3 == 1;
                var device = connected ? new DeviceRow(a.Info with { Serial = "GEOMETRY-" + index, Name = index == 1 ? "长名称几何夹具-" + new string('测', 70) : "几何夹具-" + index }) : null;
                var fixture = new UnifiedDeviceRow("geometry:" + index, _ => { fixtureInvocations++; return Task.CompletedTask; }, row => row.Connected,
                    _ => { fixtureInvocations++; return Task.CompletedTask; }, _ => fixtureIndex == 1, _ => { fixtureInvocations++; return Task.CompletedTask; });
                fixture.Update(new LdPlayerInstance(protectedRow ? 99999 : 1000 + index, "几何夹具-" + index, connected ? 10 : 0, connected ? 11 : 0, connected, connected ? 12 : -1, connected ? 13 : -1, 1280, 720, 240), device, connected, "");
                fixtureRows.Add(fixture);
                WatchFixture(fixture); if (device is not null) WatchDevice(device);
                if (index < 17) unifiedDevices.Insert(index, fixture); else unifiedDevices.Add(fixture);
            }
            RefreshUnifiedFilter(); await Settle();
            Check(unifiedDevices.Select(row => row.Key).Distinct().Count() == unifiedDevices.Count && fixtureRows.Count >= 30 && wallGrid.EnableRowVirtualization, "至少30唯一Key夹具保留行虚拟化");
            var scroll = DeviceListScrollViewer() ?? throw new InvalidOperationException("缺少真实 DataGrid ScrollViewer");
            void RecordScroll(string stage)
            {
                var viewport = DeviceProbeViewport(wallGrid, scroll);
                observations.Add(new
                {
                    phase = phase + "-scroll-state", stage, actualWindowWidth = ActualWidth, actualWindowHeight = ActualHeight, workspaceWidth = wallWorkspace.ActualWidth, listVisibility = wallList.Visibility.ToString(),
                    scroll.HorizontalOffset, scroll.VerticalOffset, scroll.ScrollableWidth, scroll.ScrollableHeight, scroll.ViewportWidth, scroll.ViewportHeight, scroll.ExtentWidth, scroll.ExtentHeight, pixelViewport = viewport,
                    hiddenOffset = hiddenListOffset is { } hidden ? new { hidden.Horizontal, hidden.Vertical } : null,
                    selectedKey = (wallGrid.SelectedItem as UnifiedDeviceRow)?.Key, currentKey = (wallGrid.CurrentItem as UnifiedDeviceRow)?.Key,
                    visibleRows = VisualDescendants<DataGridRow>(wallGrid).Where(row => row.IsVisible && row.Item is UnifiedDeviceRow).Select(row => new { key = ((UnifiedDeviceRow)row.Item).Key, bounds = DeviceProbeBounds(row, wallGrid) }).Where(row => DeviceProbeIntersection(row.bounds, viewport).Width > 0 && DeviceProbeIntersection(row.bounds, viewport).Height > 0).OrderBy(row => row.bounds.Top).Take(4).ToArray()
                });
            }
            var paths = new[] { "Number", "Name", "State", "Model", "Brand", "Android", "Serial", "Resolution", "AndroidState", "ProcessId", "VBoxProcessId", "Dpi", "Generation", "Binding" };
            Check(wallGrid.Columns.OfType<DataGridTextColumn>().Select(column => (column.Binding as Binding)?.Path.Path).ToHashSet().SetEquals(paths), "原14信息字段与绑定全部保留");
            Check(!wallSelectionColumn.CanUserResize && !wallActionColumn.CanUserResize && wallGrid.Columns.OfType<DataGridTextColumn>().All(column => column.CanUserResize), "选择/操作列固定且14信息列仍可调宽");
            phase = "list";
            DeviceProbe44(wallSelectAll, wallGrid, new Rect(wallGrid.RenderSize), Check);
            foreach (var item in unifiedDeviceView.Cast<UnifiedDeviceRow>().ToArray())
            {
                wallGrid.ScrollIntoView(item); scroll.ScrollToLeftEnd(); await Settle();
                var row = wallGrid.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow ?? throw new InvalidOperationException("虚拟行未生成: " + item.Key);
                var viewport = DeviceProbeViewport(wallGrid, scroll);
                var tile = VisualDescendants<UnifiedDeviceTile>(row).Single();
                var buttons = VisualDescendants<Button>(tile).Where(button => button.Visibility == Visibility.Visible).ToArray();
                foreach (var button in buttons) DeviceProbe44(button, wallGrid, viewport, Check);
                var checkbox = VisualDescendants<CheckBox>(row).Single(); DeviceProbe44(checkbox, wallGrid, viewport, Check);
                Check(checkbox.IsEnabled == item.CanSelect, "行选择保护保持: " + item.Key);
                if ((bool)wallGrid.Tag)
                {
                    var frozen = wallGrid.Columns.Where(column => column.DisplayIndex < wallGrid.FrozenColumnCount).Sum(column => column.ActualWidth);
                    Check(wallGrid.FrozenColumnCount == 2 && wallSelectionColumn.DisplayIndex == 0 && wallActionColumn.DisplayIndex == 1 && viewport.Width - frozen >= 120, "紧凑冻结选择/操作且信息视口至少120");
                }
                observations.Add(new { phase, item.Key, item.Name, item.State, buttons = buttons.Select(button => new { button.Name, button.ActualWidth, button.ActualHeight, bounds = DeviceProbeBounds(button, wallGrid) }).ToArray(), viewport });
            }
            var three = fixtureRows[1]; wallGrid.ScrollIntoView(three); scroll.ScrollToLeftEnd(); await Settle();
            var threeVisual = (DataGridRow)wallGrid.ItemContainerGenerator.ContainerFromItem(three);
            Check(VisualDescendants<UnifiedDeviceTile>(threeVisual).SelectMany(tile => VisualDescendants<Button>(tile)).Count(button => button.Visibility == Visibility.Visible) == 3, "纯几何三动作均保留且未调用窗口恢复");
            foreach (var column in wallGrid.Columns.OfType<DataGridTextColumn>())
            {
                wallGrid.ScrollIntoView(three, column); await Settle();
                var text = column.GetCellContent(three) as TextBlock ?? throw new InvalidOperationException("信息字段未生成真实文本");
                var path = ((Binding)column.Binding).Path.Path; var expected = typeof(UnifiedDeviceRow).GetProperty(path)!.GetValue(three)?.ToString();
                var viewport = DeviceProbeViewport(wallGrid, scroll); var frozen = wallGrid.Columns.Where(item => item.DisplayIndex < wallGrid.FrozenColumnCount).Sum(item => item.ActualWidth);
                viewport.X += frozen; viewport.Width -= frozen;
                observations.Add(new { phase = "field-before-check", path, expected, actualText = text.Text, tooltipType = text.ToolTip?.GetType().FullName, tooltipValue = text.ToolTip?.ToString(), tooltipBinding = BindingOperations.GetBindingBase(text, ToolTipProperty)?.ToString(), bounds = DeviceProbeBounds(text, wallGrid), viewport });
                Check(text.Text == expected && DeviceProbeIntersection(DeviceProbeBounds(text, wallGrid), viewport).Width > 0, "横滚字段进入视口且Text原值完整，非全值像素证明: " + path);
                if (path != "State") Check(text.ToolTip?.ToString() == expected, "长信息完整ToolTip原值: " + path);
                var start = wallGrid.Columns.Where(item => item.DisplayIndex >= wallGrid.FrozenColumnCount && item.DisplayIndex < column.DisplayIndex).Sum(item => item.ActualWidth);
                scroll.ScrollToHorizontalOffset(start); await Settle();
                var cell = DeviceProbeCell(text); var left = DeviceProbeBounds(cell, wallGrid);
                Check(left.Left >= viewport.Left - 1 && left.Left <= viewport.Right + 1, "字段左边界可横滚呈现: " + path);
                scroll.ScrollToHorizontalOffset(start + Math.Max(0, column.ActualWidth - viewport.Width)); await Settle();
                var right = DeviceProbeBounds(cell, wallGrid);
                Check(right.Right <= viewport.Right + 1 && right.Right >= viewport.Left - 1, "字段右边界可横滚呈现: " + path);
                observations.Add(new { phase = "field-edges", path, expected, tooltipType = text.ToolTip?.GetType().FullName, tooltipValue = text.ToolTip?.ToString(), left, right, viewport, fullStringPixelsClaimed = false });
            }
            scroll.ScrollToRightEnd(); await Settle(); Save("list-last");
            fixtureRows[0].IsSelected = true; deviceStatusChoice.SelectedItem = "已连接"; await Settle();
            Check(fixtureRows[0].IsSelected && !unifiedDeviceView.Cast<UnifiedDeviceRow>().Contains(fixtureRows[0]), "建立隐藏勾选与实际筛选前置");
            var originalA = unifiedDevices.Single(row => row.Device == a);
            wallGrid.ScrollIntoView(originalA); scroll.ScrollToVerticalOffset(wallGrid.Items.IndexOf(originalA)); scroll.ScrollToLeftEnd(); await Settle();
            var mainButton = VisualDescendants<Button>((DataGridRow)wallGrid.ItemContainerGenerator.ContainerFromItem(originalA)).Single(button => button.Name == "DeviceMainAction");
            var anchor = DeviceProbeAnchor(wallGrid, scroll); var horizontal = scroll.HorizontalOffset; var vertical = scroll.VerticalOffset;
            phase = "main";
            RecordScroll("before-open");
            await Invoke(mainButton); await Until(() => mainControlOpen && !ViewModel.Busy && !captureInProgress, "真实控制入口未完成");
            RecordScroll("after-open");
            ViewModel.CurrentDevice = b; var generation = ViewModel.Runtime.Input.CurrentGeneration;
            RecordContextEvent("main-context-baseline", detail: "expectedInputGeneration=" + generation.ToString(CultureInfo.InvariantCulture));
            a.Info = a.Info with { Name = "主控长标题-" + new string('测', 90) }; await Settle();
            var title = VisualDescendants<TextBlock>(wallMainPanel).Single(text => BindingOperations.GetBinding(text, TextBlock.TextProperty)?.Path.Path == nameof(WorkspaceViewModel.MainContext));
            Check(title.Text == ViewModel.MainContext && title.ToolTip as string == ViewModel.MainContext && title.TextWrapping == TextWrapping.NoWrap && title.ActualHeight <= 44 && mainBodyScroll.ActualHeight >= 100, "真实A长MainContext保留完整原值且不挤光正文");
            Check(ReferenceEquals(ViewModel.MainDevice, a), "真实控制入口指定主控A");
            Check((wallWorkspace.ActualWidth < 920) == (wallList.Visibility == Visibility.Collapsed), "按实际工作区切同区域/侧栏");
            DeviceProbe44(mainControlBack, Shell, new Rect(Shell.RenderSize), Check);
            var cast = VisualDescendants<Expander>(wallMainPanel).Single(); cast.IsExpanded = true; await Settle();
            foreach (var control in VisualDescendants<Control>(mainBodyScroll).Where(control => control is System.Windows.Controls.Button or System.Windows.Controls.CheckBox).ToArray())
            {
                control.BringIntoView(); await Settle(); var clip = DeviceProbeViewport(mainBodyScroll, mainBodyScroll);
                Check(DeviceProbeContains(clip, DeviceProbeBounds(control, mainBodyScroll)), "正文控件可以完整滚入视口: " + AutomationProperties.GetName(control));
                if (control is CheckBox) DeviceProbe44(control, mainBodyScroll, clip, Check);
            }
            mainBodyScroll.ScrollToEnd(); await Settle(); Save("main-end");
            DeviceProbe44(mainControlBack, Shell, new Rect(Shell.RenderSize), Check);
            foreach (var boundary in new[] { 919d, 920d })
            {
                RecordScroll("before-workspace-boundary-" + boundary.ToString(CultureInfo.InvariantCulture));
                for (var attempt = 0; attempt < 4 && Math.Abs(wallWorkspace.ActualWidth - boundary) > .5; attempt++) { Width += boundary - wallWorkspace.ActualWidth; await Settle(); RecordScroll("workspace-boundary-" + boundary.ToString(CultureInfo.InvariantCulture) + "-attempt-" + attempt); }
                RecordScroll("after-workspace-boundary-" + boundary.ToString(CultureInfo.InvariantCulture));
                Check(Math.Abs(wallWorkspace.ActualWidth - boundary) <= .5 && (wallList.Visibility == Visibility.Collapsed) == (boundary < 920), "实际工作区919/920布局边界: " + boundary);
                var actualMain = ViewModel.MainDevice; var actualCurrent = ViewModel.CurrentDevice; var actualGeneration = ViewModel.Runtime.Input.CurrentGeneration; var hiddenSelected = fixtureRows[0].IsSelected; var actualFilter = deviceStatusFilter;
                var sameMain = ReferenceEquals(actualMain, a); var sameCurrent = ReferenceEquals(actualCurrent, b); var sameGeneration = actualGeneration == generation; var sameHiddenSelection = hiddenSelected; var sameFilter = actualFilter == "已连接";
                observations.Add(new { phase = phase + "-boundary-context", boundary, sameMain, sameCurrent, sameGeneration, sameHiddenSelection, sameFilter, expected = new { mainId = ReferenceId(a), mainSerial = a.Serial, currentId = ReferenceId(b), currentSerial = b.Serial, inputGeneration = generation, hiddenRowKey = fixtureRows[0].Key, hiddenSelected = true, filter = "已连接" }, actual = new { mainId = ReferenceId(actualMain), mainSerial = actualMain?.Serial, currentId = ReferenceId(actualCurrent), currentSerial = actualCurrent?.Serial, inputGeneration = actualGeneration, hiddenRowKey = fixtureRows[0].Key, hiddenSelected, filter = actualFilter, active = IsActive, focus = InputIdentity(Keyboard.FocusedElement), capture = InputIdentity(Mouse.Captured) } });
                Check(sameMain && sameCurrent && sameGeneration && sameHiddenSelection && sameFilter, $"重排保留A/B/代次/隐藏选择与筛选; main={sameMain} ({ReferenceId(a)}:{a.Serial}->{ReferenceId(actualMain)}:{actualMain?.Serial}); current={sameCurrent} ({ReferenceId(b)}:{b.Serial}->{ReferenceId(actualCurrent)}:{actualCurrent?.Serial}); generation={sameGeneration} ({generation}->{actualGeneration}); hiddenSelection={sameHiddenSelection} (true->{hiddenSelected}); filter={sameFilter} (已连接->{actualFilter}); active={IsActive}; focus={InputIdentity(Keyboard.FocusedElement)}; capture={InputIdentity(Mouse.Captured)}");
            }
            Width = width; await Settle();
            RecordScroll("requested-width-restored-before-return");
            await Invoke(mainControlBack); await Settle();
            RecordScroll("after-ordinary-return-before-anchor-assert");
            Check(ReferenceEquals(wallGrid, gridIdentity) && ReferenceEquals(wallGrid.ItemsSource, sourceIdentity) && ReferenceEquals(wallMainPanel, panelIdentity), "返回保留同Grid/ItemsSource/主控对象");
            var returnedMain = ViewModel.MainDevice; var returnedCurrent = ViewModel.CurrentDevice; var returnedGeneration = ViewModel.Runtime.Input.CurrentGeneration;
            var sameReturnedMain = ReferenceEquals(returnedMain, a); var sameReturnedCurrent = ReferenceEquals(returnedCurrent, b); var sameReturnedGeneration = returnedGeneration == generation;
            observations.Add(new { phase = phase + "-return-context", sameReturnedMain, sameReturnedCurrent, sameReturnedGeneration, expected = new { mainId = ReferenceId(a), mainSerial = a.Serial, currentId = ReferenceId(b), currentSerial = b.Serial, inputGeneration = generation, hiddenSelected = true, filter = "已连接" }, actual = new { mainId = ReferenceId(returnedMain), mainSerial = returnedMain?.Serial, currentId = ReferenceId(returnedCurrent), currentSerial = returnedCurrent?.Serial, inputGeneration = returnedGeneration, hiddenSelected = fixtureRows[0].IsSelected, filter = deviceStatusFilter, active = IsActive, focus = InputIdentity(Keyboard.FocusedElement), capture = InputIdentity(Mouse.Captured) } });
            Check(sameReturnedMain && sameReturnedCurrent && sameReturnedGeneration, $"返回保留MainA/CurrentB及输入代次; main={sameReturnedMain} ({ReferenceId(a)}:{a.Serial}->{ReferenceId(returnedMain)}:{returnedMain?.Serial}); current={sameReturnedCurrent} ({ReferenceId(b)}:{b.Serial}->{ReferenceId(returnedCurrent)}:{returnedCurrent?.Serial}); generation={sameReturnedGeneration} ({generation}->{returnedGeneration}); hiddenSelection={fixtureRows[0].IsSelected}; filter={deviceStatusFilter}; active={IsActive}; focus={InputIdentity(Keyboard.FocusedElement)}; capture={InputIdentity(Mouse.Captured)}");
            DeviceProbeCheckAnchor(wallGrid, scroll, anchor, horizontal, vertical, Check);
            cast.IsExpanded = false; deviceStatusChoice.SelectedItem = "全部状态"; await Settle();
            for (var boundaryIndex = 0; boundaryIndex < 2; boundaryIndex++)
            {
                var boundary = boundaryIndex == 0 ? 599d : 600d;
                for (var attempt = 0; attempt < 4 && Math.Abs(wallGrid.ActualWidth - boundary) > .5; attempt++) { Width += boundary - wallGrid.ActualWidth; await Settle(); }
                Check(Math.Abs(wallGrid.ActualWidth - boundary) <= .5 && wallGrid.FrozenColumnCount == (boundary < 600 ? 2 : 0), "真实列表599/600冻结边界: " + boundary);
                Check(!wallSelectionColumn.CanUserResize && !wallActionColumn.CanUserResize && wallGrid.Columns.OfType<DataGridTextColumn>().All(column => column.CanUserResize), "两模式均仅信息列可调宽: " + boundary);
            }
            Width = width; await Settle();
            if (wallWorkspace.ActualWidth >= 920) { Width = 768; await Settle(); }
            phase = "rapid-reopen";
            wallGrid.ScrollIntoView(originalA); scroll.ScrollToVerticalOffset(wallGrid.Items.IndexOf(originalA)); await Settle();
            anchor = DeviceProbeAnchor(wallGrid, scroll); horizontal = scroll.HorizontalOffset; vertical = scroll.VerticalOffset;
            mainButton = VisualDescendants<Button>((DataGridRow)wallGrid.ItemContainerGenerator.ContainerFromItem(originalA)).Single(button => button.Name == "DeviceMainAction");
            await Invoke(mainButton); Check(mainControlOpen && wallList.Visibility == Visibility.Collapsed, "快速切换前实际隐藏列表");
            await Invoke(mainControlBack, false); Check(!mainControlOpen && wallList.IsVisible, "快速返回已执行且列表实际可见");
            await Invoke(mainButton, false); Check(mainControlOpen && mainControlBack.IsVisible, "快速重开实际完成且返回可见，未等待idle");
            await Invoke(mainControlBack, false);
            await Settle(); observations.Add(new { phase, expectedKey = anchor.Key, expectedY = anchor.Y, horizontal, vertical, actual = DeviceProbeAnchor(wallGrid, scroll).Key, scroll.HorizontalOffset, scroll.VerticalOffset });
            DeviceProbeCheckAnchor(wallGrid, scroll, anchor, horizontal, vertical, Check);
            phase = "routed-wheel-before-idle";
            await Invoke(mainButton); await Invoke(mainControlBack, false);
            var beforeWheel = scroll.VerticalOffset;
            var wheel = new MouseWheelEventArgs(Mouse.PrimaryDevice, Environment.TickCount, -120) { RoutedEvent = Mouse.MouseWheelEvent };
            DeviceProbePresenter(scroll).RaiseEvent(wheel); await Dispatcher.InvokeAsync(UpdateLayout, DispatcherPriority.Render);
            Check(wheel.Handled && scroll.VerticalOffset > beforeWheel, "真实ScrollContentPresenter滚轮路由已改变offset，未等待idle");
            var newerAnchor = DeviceProbeAnchor(wallGrid, scroll); var newerVertical = scroll.VerticalOffset;
            await Settle(); DeviceProbeCheckAnchor(wallGrid, scroll, newerAnchor, scroll.HorizontalOffset, newerVertical, Check);
            Check(fixtureInvocations == 0, "纯几何模拟动作从未调用");
            phase = "restore-layout-fixture";
            CloseMainControl(); unifiedDevices.Clear(); foreach (var row in originalRows) unifiedDevices.Add(row);
            foreach (var pair in originalInfos) pair.Key.Info = pair.Value;
            updatingUnifiedDirectory = originalUpdating; deviceStatusChoice.SelectedItem = "全部状态"; UpdateUnifiedDirectory(); ClearWallSelection();
            Width = width; Height = height; await Settle();
            Check(ReferenceEquals(wallGrid, gridIdentity) && ReferenceEquals(wallGrid.ItemsSource, sourceIdentity) && ReferenceEquals(wallMainPanel, panelIdentity), "几何夹具移除后仍为原对象");

            async Task GoPage(string tag)
            {
                if (!navigation[tag].IsVisible) await Invoke(navigationMenuButton);
                Check(navigation[tag].IsVisible && DeviceProbeContains(new Rect(Shell.RenderSize), DeviceProbeBounds(navigation[tag], Shell)), "实际导航入口可见: " + tag);
                navigation[tag].RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent)); await Settle();
                Check(Equals((tabs.SelectedItem as TabItem)?.Tag, tag), "实际导航Click路由已切页: " + tag);
            }
            async Task MainReady()
            {
                Activate(); await Until(() => IsActive, "主窗口未恢复激活");
                if (!Equals((tabs.SelectedItem as TabItem)?.Tag, "设备墙 / 主控")) await GoPage("设备墙 / 主控");
                if (!mainControlOpen)
                {
                    var item = unifiedDevices.Single(row => ReferenceEquals(row.Device, a)); wallGrid.ScrollIntoView(item); DeviceListScrollViewer()!.ScrollToLeftEnd(); await Settle();
                    var actual = (DataGridRow)wallGrid.ItemContainerGenerator.ContainerFromItem(item);
                    var button = VisualDescendants<Button>(actual).Single(control => control.Name == "DeviceMainAction");
                    await Invoke(button); await Until(() => mainControlOpen && !captureInProgress && !ViewModel.Busy, "控制入口仍在进行");
                }
                ViewModel.CurrentDevice = b; mainBodyScroll.ScrollToTop(); mainControlImage.BringIntoView(); await Settle();
                Check(ReferenceEquals(ViewModel.MainDevice, a) && ReferenceEquals(ViewModel.CurrentDevice, b) && mainControlImage.IsVisible && MainImagePixels(mainControlImage).Width > 0, "实际主控A画面与CurrentB已就绪");
            }
            async Task<(int A, int B)> Counts()
            {
                async Task<int> Count(string serial)
                {
                    var root = await ViewModel.Runtime.Backend.DumpAsync(serial);
                    var node = root.ChildNodeList.Single(item => item.ResourceId == "com.matrix.demo:id/tap");
                    return int.Parse(node.Text!.AsSpan(4), CultureInfo.InvariantCulture);
                }
                return (await Count(a.Serial), await Count(b.Serial));
            }
            Point InPicture(double x, double y)
            {
                var pixels = MainImagePixels(mainControlImage); var scale = Math.Min(mainControlImage.ActualWidth / pixels.Width, mainControlImage.ActualHeight / pixels.Height);
                return new Point((mainControlImage.ActualWidth - pixels.Width * scale) / 2 + pixels.Width * scale * x, (mainControlImage.ActualHeight - pixels.Height * scale) / 2 + pixels.Height * scale * y);
            }
            async Task<Point?> Position(Point point)
            {
                var screenPoint = mainControlImage.PointToScreen(point);
                Check(SetAppearanceCursorPosition((int)Math.Round(screenPoint.X), (int)Math.Round(screenPoint.Y)), "真实指针定位成功");
                var wait = System.Diagnostics.Stopwatch.StartNew(); Point mousePoint; bool pointerKnown; AppearancePointer actualPointer;
                do
                {
                    await Dispatcher.InvokeAsync(Mouse.Synchronize, DispatcherPriority.Input);
                    await Dispatcher.InvokeAsync(UpdateLayout, DispatcherPriority.Render);
                    pointerKnown = GetAppearanceCursorPosition(out actualPointer); mousePoint = Mouse.GetPosition(mainControlImage);
                    if (pointerKnown && Math.Abs(actualPointer.X - Math.Round(screenPoint.X)) <= 1 && Math.Abs(actualPointer.Y - Math.Round(screenPoint.Y)) <= 1 && (mousePoint - point).Length <= 1.5) break;
                    await Task.Delay(30);
                } while (wait.Elapsed < TimeSpan.FromMilliseconds(600));
                var mapped = MapImage(mainControlImage, mousePoint);
                observations.Add(new
                {
                    phase = phase + "-pointer", requestedLocal = point, requestedScreen = screenPoint, pointerKnown, actualScreen = new { actualPointer.X, actualPointer.Y }, actualLocal = mousePoint, mapped,
                    waitMilliseconds = wait.ElapsedMilliseconds, singlePositionAttempt = true, mainControlImage.RenderSize, pixels = MainImagePixels(mainControlImage),
                    bodyViewport = DeviceProbeViewport(mainBodyScroll, mainBodyScroll), imageBoundsInBody = DeviceProbeBounds(mainControlImage, mainBodyScroll), imageBoundsInShell = DeviceProbeBounds(mainControlImage, Shell),
                    mouseDirectlyOver = Mouse.DirectlyOver?.GetType().FullName, mainBodyScroll.VerticalOffset, mainBodyScroll.ScrollableHeight
                });
                Check(pointerKnown && Math.Abs(actualPointer.X - Math.Round(screenPoint.X)) <= 1 && Math.Abs(actualPointer.Y - Math.Round(screenPoint.Y)) <= 1 && (mousePoint - point).Length <= 1.5, "一次指针定位后OS与WPF坐标在有界等待内一致");
                return mapped;
            }
            void MouseRoute(RoutedEvent routedEvent) => mainControlImage.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left) { RoutedEvent = routedEvent });
            async Task Gesture(string name, Point from, Point to, bool sync, InputKind kind)
            {
                phase = name; await MainReady(); ClearWallSelection(); b.IsSelected = sync; ViewModel.SyncMouse = sync; ViewModel.UpdateSelection(); await Settle();
                var before = await Counts(); var dispatches = mainGestureDispatchCount;
                var start = await Position(InPicture(from.X, from.Y)); Check(start is not null, "Down坐标在真实画面内");
                MouseRoute(UIElement.MouseLeftButtonDownEvent); Check(gestureStart is not null && mainControlImage.IsMouseCaptured, "真实Image Down建立手势与鼠标捕获");
                var end = await Position(InPicture(to.X, to.Y)); Check(end is not null, "Up坐标在真实画面内");
                MouseRoute(UIElement.MouseLeftButtonUpEvent); await mainGestureOperation.WaitAsync(TimeSpan.FromSeconds(3)); await Settle();
                var after = await Counts(); var snapshot = lastMainGestureSnapshot;
                Check(mainGestureDispatchCount == dispatches + 1 && after.A == before.A + 1 && after.B == before.B + (sync ? 1 : 0), "真实路由只对MainA及明确同步B产生一次Demo副作用");
                Check(snapshot is not null && snapshot.Kind == kind && snapshot.SourceSerial == a.Serial && snapshot.TargetSerials.SequenceEqual(sync ? new[] { b.Serial } : Array.Empty<string>()) && snapshot.SourceGeneration == a.Info.Generation && snapshot.Generation == ViewModel.Runtime.Input.CurrentGeneration, "派发不可变snapshot种类/源/目标/代次正确");
                Check(snapshot!.X == (int)start!.Value.X && snapshot.Y == (int)start.Value.Y && snapshot.X2 == (int)end!.Value.X && snapshot.Y2 == (int)end.Value.Y && snapshot.SourceWidth == (int)MainImagePixels(mainControlImage).Width && snapshot.SourceHeight == (int)MainImagePixels(mainControlImage).Height, "snapshot保存真实指针映射与原图尺寸");
                Check(gestureStart is null && !mainControlImage.IsMouseCaptured && ReferenceEquals(ViewModel.MainDevice, a) && ReferenceEquals(ViewModel.CurrentDevice, b), "正常Up释放捕获且不丢主控/Current");
                observations.Add(new { phase, beforeA = before.A, beforeB = before.B, afterA = after.A, afterB = after.B, dispatches, mainGestureDispatchCount, snapshot });
            }
            await Gesture("normal-tap", new(.5, .5), new(.5, .5), false, InputKind.Tap);
            await Gesture("normal-swipe", new(.5, .7), new(.5, .3), false, InputKind.Swipe);
            await Gesture("normal-sync", new(.5, .5), new(.5, .5), true, InputKind.Tap);
            var immutable = lastMainGestureSnapshot!; ClearWallSelection(); CancelMainGesture();
            Check(immutable.TargetSerials.SequenceEqual(new[] { b.Serial }), "正常LostCapture和之后清选未改变已保存同步目标");

            async Task Negative(string name, Func<Task> interrupt, Func<Task>? prepare = null, bool positionAfterInterrupt = true)
            {
                phase = name; await MainReady(); ClearWallSelection(); b.IsSelected = true; ViewModel.SyncMouse = true; ViewModel.UpdateSelection(); await Settle();
                if (prepare is not null) await prepare();
                var before = await Counts(); var dispatches = mainGestureDispatchCount;
                var savedScreenshot = a.Screenshot;
                Check(await Position(InPicture(.5, .5)) is not null, "负例起点有效"); MouseRoute(UIElement.MouseLeftButtonDownEvent);
                Check(gestureStart is not null && mainControlImage.IsMouseCaptured, "负例实际Down前置成立");
                try
                {
                    await interrupt();
                    if (positionAfterInterrupt && mainControlImage.IsVisible) await Position(InPicture(.5, .5));
                    MouseRoute(UIElement.MouseLeftButtonUpEvent); await mainGestureOperation.WaitAsync(TimeSpan.FromSeconds(3)); await Settle();
                    Check(gestureStart is null && !mainControlImage.IsMouseCaptured, "中断Up明确清除手势并释放鼠标捕获: " + name);
                    var after = await Counts(); Check(mainGestureDispatchCount == dispatches && after == before, "中断后零新派发且A/B Demo计数不变: " + name);
                    observations.Add(new { phase, beforeA = before.A, beforeB = before.B, afterA = after.A, afterB = after.B, dispatches, mainGestureDispatchCount, active = IsActive, mainControlOpen, selectedPage = (tabs.SelectedItem as TabItem)?.Tag });
                }
                finally
                {
                    a.Screenshot = savedScreenshot;
                    a.Info = originalInfos[a]; b.Info = originalInfos[b]; ViewModel.SetExternalBusy(false);
                    if (inactiveWindow is not null) { inactiveWindow.Close(); inactiveWindow = null; }
                    Width = width; Height = height; await MainReady();
                }
            }
            await Negative("resize-cancel", async () => { Width += 31; await Settle(); Check(gestureStart is null, "真实resize取消未完成手势"); });
            await Negative("body-scroll-cancel", async () =>
            {
                var beforeOffset = mainBodyScroll.VerticalOffset;
                mainBodyScroll.ScrollToEnd(); await Settle();
                Check(mainBodyScroll.VerticalOffset > beforeOffset && gestureStart is null, "主控正文实际滚动取消未完成手势");
                observations.Add(new { phase, actualWidth = ActualWidth, actualHeight = ActualHeight, beforeOffset, afterOffset = mainBodyScroll.VerticalOffset, mainBodyScroll.ScrollableHeight, interruption = "programmatic actual ScrollViewer offset; not a physical wheel" });
            }, async () =>
            {
                VisualDescendants<Expander>(wallMainPanel).Single().IsExpanded = true; await Settle();
                if (mainBodyScroll.ScrollableHeight <= 1)
                {
                    Height = 640; await Settle();
                    observations.Add(new { phase, requestedHeight = height, actualHeight = ActualHeight, constrainedBodyFixture = true });
                }
                mainBodyScroll.ScrollToTop(); mainControlImage.BringIntoView(); await Settle();
                Check(mainBodyScroll.ScrollableHeight > mainBodyScroll.VerticalOffset + 1, "展开正文后存在Down到末端的实际滚动距离");
            });
            await Negative("source-pixels-reject", async () =>
            {
                var originalPixels = MainImagePixels(mainControlImage); var originalBounds = MainImageBounds(mainControlImage); var originalRenderSize = mainControlImage.RenderSize;
                var pixelWidth = checked((int)originalPixels.Width * 2); var pixelHeight = checked((int)originalPixels.Height * 2); var stride = checked(pixelWidth * 4);
                var replacement = BitmapSource.Create(pixelWidth, pixelHeight, 96, 96, PixelFormats.Bgra32, null, new byte[checked(stride * pixelHeight)], stride); replacement.Freeze();
                a.Screenshot = replacement; await Settle();
                Check(ReferenceEquals(mainControlImage.Source, replacement) && MainImagePixels(mainControlImage) != originalPixels && pixelWidth * originalPixels.Height == pixelHeight * originalPixels.Width, "Demo替图只改变像素尺寸而保持纵横比，非设备旋转");
                Check(mainControlImage.RenderSize == originalRenderSize && MainImageBounds(mainControlImage) == originalBounds && gestureStart is not null && mainControlImage.IsMouseCaptured, "布局和Down捕获未变，隔离原图像素冻结守卫");
                observations.Add(new { phase, originalPixels, replacementPixels = MainImagePixels(mainControlImage), originalBounds, originalRenderSize, actualRenderSize = mainControlImage.RenderSize, demoReplacementIsNotDeviceRotation = true });
            });
            await Negative("lost-capture-cancel", async () => { Mouse.Capture(null); await Settle(); Check(gestureStart is null, "真实LostCapture取消手势"); });
            await Negative("navigation-cancel", async () => { await GoPage("应用"); Check(gestureStart is null, "离页取消手势"); });
            await Negative("close-cancel", async () => { await Invoke(mainControlBack); Check(gestureStart is null && !mainControlOpen, "实际返回取消手势"); });
            await Negative("deactivate-cancel", async () =>
            {
                inactiveWindow = new Window { Owner = this, Title = "Demo focus fixture", Width = 240, Height = 140, ShowInTaskbar = false, WindowStartupLocation = WindowStartupLocation.CenterOwner, Content = new TextBlock { Text = "Isolated focus fixture" } };
                inactiveWindow.Show(); inactiveWindow.Activate(); await Until(() => !IsActive && inactiveWindow.IsActive, "自有窗口未使主窗口失活"); Check(gestureStart is null, "真实失活取消手势");
            });
            await Negative("offline-reject", () => { a.Info = a.Info with { State = "Offline" }; return Task.CompletedTask; });
            await Negative("busy-reject", () => { ViewModel.SetExternalBusy(true); return Task.CompletedTask; });
            await Negative("source-generation-reject", () => { a.Info = a.Info with { Generation = a.Info.Generation + 1 }; return Task.CompletedTask; });
            await Negative("target-generation-reject", () => { b.Info = b.Info with { Generation = b.Info.Generation + 1 }; return Task.CompletedTask; });
            await Negative("input-generation-reject", () => ViewModel.Runtime.Input.ReleaseAsync());
            Point VisibleParentBlank()
            {
                var parent = mainControlImage.Parent as Grid ?? throw new InvalidOperationException("默认Image缺少实际父screen Grid。");
                var viewport = DeviceProbeViewport(mainBodyScroll, mainBodyScroll); var parentBounds = DeviceProbeBounds(parent, mainBodyScroll); var imageBounds = DeviceProbeBounds(mainControlImage, mainBodyScroll);
                var visible = DeviceProbeIntersection(parentBounds, viewport); Check(visible.Width > 8 && visible.Height > 8, "默认父screen Grid存在实际可见区域");
                var candidates = new[] { new Point(visible.Left + 3, visible.Top + visible.Height / 2), new Point(visible.Right - 3, visible.Top + visible.Height / 2), new Point(visible.Left + visible.Width / 2, visible.Top + 3), new Point(visible.Left + visible.Width / 2, visible.Bottom - 3) };
                var available = candidates.Where(point => !imageBounds.Contains(point)).ToArray(); Check(available.Length > 0, "默认Image外且父Grid内存在可视留白，非内部letterbox");
                var blank = available[0]; var parentPoint = mainBodyScroll.TranslatePoint(blank, parent); var hit = parent.InputHitTest(parentPoint) as DependencyObject;
                observations.Add(new { phase, parentBounds, imageBounds, viewport, visible, blankInBody = blank, parentPoint, hitType = hit?.GetType().FullName, defaultInternalLetterbox = "NOT-APPLICABLE: intrinsic-aspect Image, testing its actual parent blank", scope = "Default parent Grid geometry and WPF routed events, not physical clicks or forced Image width" });
                Check(hit is not null && !ReferenceEquals(hit, mainControlImage) && !mainControlImage.IsAncestorOf(hit), "父Grid实际留白命中不归Image");
                return mainBodyScroll.TranslatePoint(blank, mainControlImage);
            }
            phase = "parent-blank-reject"; await MainReady(); var blankBefore = await Counts(); var blankDispatch = mainGestureDispatchCount;
            var blankPoint = VisibleParentBlank(); Check(await Position(blankPoint) is null, "实际父留白相对Image越界且MapImage为空");
            var screenParent = (Grid)mainControlImage.Parent;
            screenParent.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left) { RoutedEvent = UIElement.MouseLeftButtonDownEvent });
            screenParent.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left) { RoutedEvent = UIElement.MouseLeftButtonUpEvent });
            await mainGestureOperation.WaitAsync(TimeSpan.FromSeconds(3));
            Check(gestureStart is null && !mainControlImage.IsMouseCaptured && mainGestureDispatchCount == blankDispatch && await Counts() == blankBefore, "父Grid留白路由不建立Image手势且A/B零副作用");
            await Negative("captured-drag-outside-reject", async () =>
            {
                var outside = VisibleParentBlank(); Check(await Position(outside) is null, "已捕获后一次移动到真实父留白且MapImage为空");
                var actual = Mouse.GetPosition(mainControlImage);
                Check((actual.X < 0 || actual.Y < 0 || actual.X >= mainControlImage.ActualWidth || actual.Y >= mainControlImage.ActualHeight) && gestureStart is not null && mainControlImage.IsMouseCaptured, "Up前确实拖出Image边界且原捕获仍在");
            }, positionAfterInterrupt: false);
            await Gesture("healthy-after-negatives", new(.5, .5), new(.5, .5), false, InputKind.Tap);

            foreach (var action in new[] { "close", "navigate", "deactivate" })
            {
                phase = "capture-" + action; await MainReady(); var before = mainCaptureDispatchCount; var beforeA = a.CapturedAt; var beforeB = b.CapturedAt;
                var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously); var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously); Task? pending = null;
                demoMainCaptureBarrier = () => { entered.TrySetResult(); return release.Task; };
                try
                {
                    pending = CaptureTickAsync(); await entered.Task.WaitAsync(TimeSpan.FromSeconds(3));
                    Check(mainCaptureDispatchCount == before + 1 && captureInProgress && !pending.IsCompleted && lastMainCaptureTargets.SequenceEqual(new[] { a.Serial }), "仅一次MainA Desktop接纳任务已被屏障持有，非ADB wire计数");
                    if (action == "close") await Invoke(mainControlBack);
                    else if (action == "navigate") await GoPage("应用");
                    else
                    {
                        inactiveWindow = new Window { Owner = this, Title = "Demo capture focus fixture", Width = 240, Height = 140, ShowInTaskbar = false, WindowStartupLocation = WindowStartupLocation.CenterOwner, Content = new TextBlock { Text = "Isolated capture fixture" } };
                        inactiveWindow.Show(); inactiveWindow.Activate(); await Until(() => !IsActive && inactiveWindow.IsActive, "捕获隔离窗口未激活");
                    }
                    await CaptureTickAsync(); await CaptureTickAsync(); Check(mainCaptureDispatchCount == before + 1, "中断后不再接纳第二任务");
                    release.TrySetResult(); await pending.WaitAsync(TimeSpan.FromSeconds(3)); demoMainCaptureBarrier = null; await Settle();
                    Check(!captureInProgress && a.CapturedAt > beforeA && b.CapturedAt == beforeB, "已接纳任务可以完成真实Demo截图，finally清理且B未截图");
                    await CaptureTickAsync(); Check(mainCaptureDispatchCount == before + 1, "首任务完成后中断守卫仍拒绝新任务，不只是in-progress阻挡");
                    if (inactiveWindow is not null) { inactiveWindow.Close(); inactiveWindow = null; }
                    var resume = mainCaptureDispatchCount; await MainReady();
                    if (action != "close") await CaptureTickAsync();
                    Check(mainCaptureDispatchCount == resume + 1 && lastMainCaptureTargets.SequenceEqual(new[] { a.Serial }), "恢复正常状态仅新增一次MainA接纳任务");
                    observations.Add(new { phase, before, after = mainCaptureDispatchCount, beforeA, afterA = a.CapturedAt, beforeB, afterB = b.CapturedAt, pendingCompleted = pending.IsCompleted, admissionIsNotWireCount = true });
                }
                finally
                {
                    release.TrySetResult(); if (pending is not null) await pending.WaitAsync(TimeSpan.FromSeconds(3)); demoMainCaptureBarrier = originalBarrier;
                    if (inactiveWindow is not null) { inactiveWindow.Close(); inactiveWindow = null; }
                }
            }
            phase = "complete"; Check(fixtureInvocations == 0, "所有纯几何fixture动作始终未执行"); Save("final");
        }
        catch (Exception ex) { failure = ex; Console.Error.WriteLine(ex); }
        finally
        {
            Activated -= activatedObserver; Deactivated -= deactivatedObserver; SizeChanged -= sizeObserver; LocationChanged -= locationObserver; ViewModel.PropertyChanged -= viewModelObserver; wallGrid.SelectionChanged -= gridSelectionObserver; deviceStatusChoice.SelectionChanged -= filterSelectionObserver;
            foreach (var watched in watchedDevices) watched.Row.PropertyChanged -= watched.Handler;
            foreach (var watched in watchedFixtures) watched.Row.PropertyChanged -= watched.Handler;
            observations.Add(new { phase = "context-event-timeline", eventCount = contextEventCount, retainedCount = contextEvents.Count, droppedCount = contextEventCount - contextEvents.Count, maximum = 128, events = contextEvents.ToArray(), observersRemovedBeforeStateRestoration = true });
            try
            {
                if (inactiveWindow is not null) { inactiveWindow.Close(); inactiveWindow = null; }
                CancelMainGesture(); CloseMainControl(); demoMainCaptureBarrier = originalBarrier; ViewModel.SetExternalBusy(false);
                updatingUnifiedDirectory = true; unifiedDevices.Clear(); foreach (var row in originalRows) unifiedDevices.Add(row);
                foreach (var pair in originalInfos) pair.Key.Info = pair.Value;
                foreach (var row in originalRows) row.IsSelected = originalSelection[row.Key];
                ViewModel.SyncMouse = originalSyncMouse; ViewModel.SyncKeyboard = originalSyncKeyboard; ViewModel.UpdateSelection();
                deviceStatusChoice.SelectedItem = originalFilter; updatingUnifiedDirectory = originalUpdating; UpdateUnifiedDirectory();
                if (originalMain is not null) await ViewModel.SetMainAsync(originalMain);
                ViewModel.CurrentDevice = originalCurrent; Width = width; Height = height; NavigateTo(originalTag);
                SetAppearanceCursorPosition(pointer.X, pointer.Y); await Settle();
            }
            catch (Exception cleanupError) { failure = failure is null ? cleanupError : new AggregateException("验收与恢复均失败", failure, cleanupError); }
            try
            {
                if (originalCaptureTimerEnabled) captureTimer.Start(); else captureTimer.Stop();
                Check(captureDispatcherTimer.IsEnabled == originalCaptureTimerEnabled, "专项结束恢复captureTimer原运行态");
            }
            catch (Exception timerError) { failure = failure is null ? timerError : new AggregateException("验收与定时器恢复均失败", failure, timerError); }
            try { if (screenshot is not null) { Directory.CreateDirectory(Path.GetDirectoryName(screenshot)!); SaveRender(screenshot); } }
            catch (Exception screenshotError) { failure = failure is null ? screenshotError : new AggregateException("验收与最终截图均失败", failure, screenshotError); }
            try { File.WriteAllText(Path.Combine(ViewModel.DataDirectory, "device-layout.json"), JsonSerializer.Serialize(new
            {
                status = failure is null ? "PASS-OFFLINE-WPF" : "FAIL", scope = "Isolated Demo, actual WPF controls and routed events; not physical clicks, Android, ADB wire, DPI or discovery proof",
                width, height, actualWidth = ActualWidth, actualHeight = ActualHeight, phase, checks, observations, fixtureInvocations, error = failure?.ToString()
            }, JsonDefaults.Options)); }
            catch (Exception reportError) { Console.Error.WriteLine(reportError); failure = failure is null ? reportError : new AggregateException("验收与报告保存均失败", failure, reportError); }
        }
        if (failure is not null) ExceptionDispatchInfo.Capture(failure).Throw();
    }

    private static Rect DeviceProbeBounds(FrameworkElement element, Visual ancestor) => element.TransformToAncestor(ancestor).TransformBounds(new Rect(element.RenderSize));
    private static bool DeviceProbeContains(Rect viewport, Rect bounds) { viewport.Inflate(.5, .5); return bounds.Width > 0 && bounds.Height > 0 && viewport.Contains(bounds); }
    private static Rect DeviceProbeIntersection(Rect first, Rect second) => Rect.Intersect(first, second);
    private static DataGridCell DeviceProbeCell(DependencyObject element)
    {
        for (var current = element; current is not null; current = VisualTreeHelper.GetParent(current)) if (current is DataGridCell cell) return cell;
        throw new InvalidOperationException("控件不在实际DataGridCell内。");
    }
    private static ScrollContentPresenter DeviceProbePresenter(ScrollViewer scroll) => VisualDescendants<ScrollContentPresenter>(scroll).Single(item => ReferenceEquals(item.TemplatedParent, scroll));
    private static Rect DeviceProbeViewport(FrameworkElement ancestor, ScrollViewer scroll)
    {
        var presenter = DeviceProbePresenter(scroll);
        return DeviceProbeBounds(presenter, ancestor);
    }
    private static void DeviceProbe44(Control control, FrameworkElement ancestor, Rect viewport, Action<bool, string> check)
    {
        var bounds = DeviceProbeBounds(control, ancestor);
        check(control.IsVisible && control.ActualWidth >= 43.9 && control.ActualHeight >= 43.9 && DeviceProbeContains(viewport, bounds), "完整44命中位于视口: " + AutomationProperties.GetName(control));
        DependencyObject? parent = control;
        while (parent is not null && parent is not DataGridCell && parent is not DataGridColumnHeader) parent = VisualTreeHelper.GetParent(parent);
        if (parent is FrameworkElement cell) check(DeviceProbeContains(DeviceProbeBounds(cell, ancestor), bounds), "完整命中未越过单元格: " + control.Name);
        foreach (var point in new[] { new Point(2, 2), new Point(control.ActualWidth / 2, control.ActualHeight / 2), new Point(control.ActualWidth - 2, control.ActualHeight - 2) })
        {
            var location = control.TranslatePoint(point, ancestor);
            DependencyObject? target = null;
            if (control.IsEnabled) target = ancestor.InputHitTest(location) as DependencyObject;
            else VisualTreeHelper.HitTest(ancestor,
                item => item is UIElement element && (!element.IsVisible || !element.IsHitTestVisible) ? HitTestFilterBehavior.ContinueSkipSelfAndChildren : HitTestFilterBehavior.Continue,
                result => { target = result.VisualHit; return HitTestResultBehavior.Stop; }, new PointHitTestParameters(location));
            var ancestors = new List<string>();
            for (var current = target; current is not null; current = VisualTreeHelper.GetParent(current))
                ancestors.Add(current.GetType().Name + ":" + (current as FrameworkElement)?.Name);
            check(target is not null && (ReferenceEquals(control, target) || control.IsAncestorOf(target)),
                $"控件命中归属: type={control.GetType().Name}; name={control.Name}; automation={AutomationProperties.GetName(control)}; key={(control.DataContext as UnifiedDeviceRow)?.Key}; enabled={control.IsEnabled}; bounds={bounds}; viewport={viewport}; point={point}; location={location}; visual={string.Join('>', ancestors)}");
        }
    }
    private static (string Key, double Y) DeviceProbeAnchor(DataGrid grid, ScrollViewer scroll)
    {
        var viewport = DeviceProbeViewport(grid, scroll);
        var row = VisualDescendants<DataGridRow>(grid).Where(item => item.IsVisible && item.Item is UnifiedDeviceRow && DeviceProbeBounds(item, grid) is var bounds && bounds.Top >= viewport.Top - .5 && bounds.Bottom <= viewport.Bottom + .5 && Rect.Intersect(bounds, viewport).Width > 0).OrderBy(item => DeviceProbeBounds(item, grid).Top).First();
        return (((UnifiedDeviceRow)row.Item).Key, DeviceProbeBounds(row, grid).Top - viewport.Top);
    }
    private static void DeviceProbeCheckAnchor(DataGrid grid, ScrollViewer scroll, (string Key, double Y) anchor, double horizontal, double vertical, Action<bool, string> check)
    {
        var actual = DeviceProbeAnchor(grid, scroll);
        var details = FormattableString.Invariant($" expectedKey={anchor.Key}; expectedY={anchor.Y}; expectedH={horizontal}; expectedV={vertical}; actualKey={actual.Key}; actualY={actual.Y}; actualH={scroll.HorizontalOffset}; actualV={scroll.VerticalOffset}; scrollMaxH={scroll.ScrollableWidth}; scrollMaxV={scroll.ScrollableHeight}; viewportW={scroll.ViewportWidth}; viewportH={scroll.ViewportHeight}; extentW={scroll.ExtentWidth}; extentH={scroll.ExtentHeight}; pixelViewport={DeviceProbeViewport(grid, scroll)}; selectedKey={(grid.SelectedItem as UnifiedDeviceRow)?.Key}; currentKey={(grid.CurrentItem as UnifiedDeviceRow)?.Key}");
        check(actual.Key == anchor.Key && Math.Abs(actual.Y - anchor.Y) <= 1, "滚动首个完整行Key与相对Y恢复:" + details);
        check(Math.Abs(scroll.VerticalOffset - Math.Min(vertical, scroll.ScrollableHeight)) <= .1 && Math.Abs(scroll.HorizontalOffset - Math.Min(horizontal, scroll.ScrollableWidth)) <= 1, "横纵滚动恢复且仅允许合法钳制:" + details);
    }
}
