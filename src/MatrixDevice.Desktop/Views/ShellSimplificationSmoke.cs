using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Threading;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    internal async Task VerifyShellSimplificationAsync()
    {
        if (!ViewModel.Demo) throw new InvalidOperationException("外壳验收仅允许演示工作区。");
        if (WorkspacePages.Length != 12 || tabs.Items.Count != 12 || navigation.Count != 12)
            throw new InvalidOperationException("统一导航应包含 12 个独立页面。");
        if (WorkspacePages.Any(page => page.Tag == "雷电实例") || tabs.Items.Cast<TabItem>().Any(page => Equals(page.Tag, "雷电实例")) || navigation.ContainsKey("雷电实例"))
            throw new InvalidOperationException("旧雷电实例独立入口仍然存在。");
        if (!navigation.Keys.ToHashSet(StringComparer.Ordinal).SetEquals(WorkspacePages.Select(page => page.Tag)))
            throw new InvalidOperationException("导航入口与页面定义不一致。");
        if (workspace.RowDefinitions.Count != 4 || Grid.GetRow(tabs) != 2)
            throw new InvalidOperationException("删除摘要后仍保留空白行。");

        var previousTag = (tabs.SelectedItem as TabItem)?.Tag?.ToString() ?? WorkspacePages[0].Tag;
        var previousDevice = ViewModel.CurrentDevice;
        var directoryWasVisible = directoryWindow?.IsVisible == true;
        try
        {
            foreach (var page in WorkspacePages)
            {
                NavigateTo(page.Tag);
                await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
                UpdateLayout();
                VerifyNoRemovedShellControls(Shell, page.Label);
                if (!navigation.TryGetValue(page.Tag, out var link) || link.IsChecked != true || !link.IsVisible)
                    throw new InvalidOperationException($"页面 {page.Label} 没有唯一有效导航入口。");
            }

            NavigateTo("节点");
            await Dispatcher.InvokeAsync(UpdateLayout, DispatcherPriority.Render);
            var target = VisualDescendants<ComboBox>(Shell).Single(input => input.Name == "SingleDeviceTarget");
            if (!target.IsVisible || target.IsEditable || !target.Items.Cast<DeviceRow>().SequenceEqual(ViewModel.Devices) || BindingOperations.GetBinding(target, Selector.SelectedItemProperty)?.Path.Path != nameof(WorkspaceViewModel.CurrentDevice))
                throw new InvalidOperationException("单设备目标没有绑定到明确可选择的设备列表。");
            if (ViewModel.Devices.FirstOrDefault(row => !ReferenceEquals(row, previousDevice)) is { } other)
            {
                target.SelectedItem = other;
                if (!ReferenceEquals(ViewModel.CurrentDevice, other)) throw new InvalidOperationException("选择目标未更新单设备操作上下文。");
                target.SelectedItem = previousDevice;
                if (!ReferenceEquals(ViewModel.CurrentDevice, previousDevice)) throw new InvalidOperationException("单设备操作目标未恢复。");
            }
            NavigateTo("应用");
            await Dispatcher.InvokeAsync(UpdateLayout, DispatcherPriority.Render);
            if (singleTargetChoice.Visibility != Visibility.Collapsed || singleTargetLabel.Visibility != Visibility.Collapsed)
                throw new InvalidOperationException("应用页仍显示无关的单设备目标，可能与应用勾选对象混淆。");
            NavigateTo("节点");
            await Dispatcher.InvokeAsync(UpdateLayout, DispatcherPriority.Render);
            if (VisualDescendants<ComboBox>(Shell).Any(input => BindingOperations.GetBinding(input, Selector.SelectedItemProperty)?.Path.Path == "GroupFilter"))
                throw new InvalidOperationException("全局设备分组入口仍然存在。");
            if (previousDevice is not null && ViewModel.Devices.FirstOrDefault(row => !ReferenceEquals(row, previousDevice)) is { } filteredDevice)
            {
                var previousInfo = filteredDevice.Info;
                var previousStatus = ViewModel.StatusFilter;
                var main = ViewModel.MainDevice;
                var selected = ViewModel.Devices.Select(row => row.IsSelected).ToArray();
                try
                {
                    await ViewModel.LoadNodesAsync();
                    var nodes = ViewModel.Nodes.ToArray();
                    var image = ViewModel.DebugImage;
                    var parameters = ViewModel.Parameters.ToArray();
                    filteredDevice.Info = previousInfo with { State = "Offline" };
                    ViewModel.StatusFilter = "Offline";
                    await Dispatcher.InvokeAsync(UpdateLayout, DispatcherPriority.Render);
                    if (ViewModel.DeviceView.Cast<DeviceRow>().Contains(previousDevice) || !ViewModel.DeviceView.Cast<DeviceRow>().Contains(filteredDevice))
                        throw new InvalidOperationException("单设备目标隐藏反例没有形成。");
                    foreach (var tag in new[] { "应用", "节点" })
                    {
                        NavigateTo(tag);
                        await Dispatcher.InvokeAsync(UpdateLayout, DispatcherPriority.Render);
                        if (!target.Items.Contains(previousDevice) || !ReferenceEquals(target.SelectedItem, previousDevice)
                            || !ReferenceEquals(ViewModel.CurrentDevice, previousDevice) || !ReferenceEquals(ViewModel.MainDevice, main)
                            || !selected.SequenceEqual(ViewModel.Devices.Select(row => row.IsSelected)) || !nodes.SequenceEqual(ViewModel.Nodes)
                            || !ReferenceEquals(ViewModel.DebugImage, image) || !parameters.SequenceEqual(ViewModel.Parameters))
                            throw new InvalidOperationException("状态筛选改写了单设备目标、主控、勾选或节点参数上下文。");
                    }
                }
                finally
                {
                    filteredDevice.Info = previousInfo;
                    ViewModel.StatusFilter = previousStatus;
                }
            }
            if (!VisualDescendants<Button>(Shell).Any(button => button.ToolTip as string == "环境设置") || !VisualDescendants<Button>(Shell).Any(button => button.ToolTip as string == "设备详情"))
                throw new InvalidOperationException("设置或设备属性入口缺失。");

            ShowDeviceDirectory();
            await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
            directoryWindow!.UpdateLayout();
            VerifyNoRemovedShellControls(directoryWindow, "设备详情");
            var directoryList = VisualDescendants<ListBox>(directoryWindow).Single(list => ReferenceEquals(list.ItemsSource, ViewModel.DeviceView));
            if (BindingOperations.GetBinding(directoryList, Selector.SelectedItemProperty)?.Path.Path != nameof(WorkspaceViewModel.CurrentDevice))
                throw new InvalidOperationException("设备目录已丢失明确选择功能。");
            var inputs = VisualDescendants<TextBox>(directoryWindow).Select(input => BindingOperations.GetBinding(input, TextBox.TextProperty)?.Path.Path).ToHashSet();
            if (!inputs.Contains(nameof(WorkspaceViewModel.DeviceName)) || inputs.Contains("DeviceGroup"))
                throw new InvalidOperationException("设备名称编辑缺失或仍保留分组输入。");
            var status = VisualDescendants<ComboBox>(directoryWindow).Single(input => BindingOperations.GetBinding(input, Selector.SelectedItemProperty)?.Path.Path == nameof(WorkspaceViewModel.StatusFilter));
            if (!status.IsVisible) throw new InvalidOperationException("设备详情缺少有效状态筛选。");
            var captions = VisualDescendants<TextBlock>(directoryWindow).Select(text => text.Text).ToArray();
            if (!captions.Any(caption => caption.StartsWith("画面", StringComparison.Ordinal)) || !captions.Any(caption => caption.StartsWith("节点", StringComparison.Ordinal)))
                throw new InvalidOperationException("设备子系统状态缺失。");
            if (ViewModel.CurrentDevice is { } metadataTarget)
            {
                var info = metadataTarget.Info;
                var name = ViewModel.DeviceName;
                var metadata = ViewModel.Runtime.Metadata.GetMetadata(metadataTarget.Serial);
                const string retainedGroup = "  保留原分组验收  ";
                try
                {
                    metadataTarget.Info = info with { GroupId = retainedGroup };
                    ViewModel.DeviceName = "名称保存验收";
                    await ViewModel.SaveMetadataAsync();
                    var saved = ViewModel.Runtime.Metadata.GetMetadata(metadataTarget.Serial);
                    if (metadataTarget.Info.GroupId != retainedGroup || saved.GroupId != retainedGroup || saved.Name != "名称保存验收")
                        throw new InvalidOperationException("仅保存名称时修改或裁剪了既有分组数据。");
                }
                finally
                {
                    ViewModel.Runtime.Metadata.UpsertMetadata(metadataTarget.Serial, metadata.Name, metadata.GroupId);
                    metadataTarget.Info = info; ViewModel.DeviceName = name;
                }
            }
        }
        finally
        {
            ViewModel.CurrentDevice = previousDevice;
            if (!directoryWasVisible) directoryWindow?.Hide();
            NavigateTo(previousTag);
            UpdateLayout();
        }
    }

    private static void VerifyNoRemovedShellControls(DependencyObject source, string page)
    {
        var searchPaths = new HashSet<string>(StringComparer.Ordinal) { "Search", "AppSearch", "NodeSearch" };
        if (VisualDescendants<TextBox>(source).Any(input => searchPaths.Contains(BindingOperations.GetBinding(input, TextBox.TextProperty)?.Path.Path ?? "") || (input.ToolTip as string)?.Contains("搜索", StringComparison.Ordinal) == true))
            throw new InvalidOperationException($"页面 {page} 仍有搜索输入或提示。");
        if (VisualDescendants<TextBlock>(source).Any(text => text.Text.Contains("当前设备", StringComparison.Ordinal) || text.Text is "高级管理" or "全部设备" || BindingOperations.GetBinding(text, TextBlock.TextProperty)?.Path.Path == "DeviceContext"))
            throw new InvalidOperationException($"页面 {page} 仍有旧设备摘要或独立标签。");
        if (VisualDescendants<ComboBox>(source).Any(input => BindingOperations.GetBinding(input, Selector.SelectedItemProperty)?.Path.Path == "GroupFilter"
            || input.Items.Cast<object>().Any(item => item as string == "全部分组"))
            || VisualDescendants<TextBox>(source).Any(input => BindingOperations.GetBinding(input, TextBox.TextProperty)?.Path.Path == "DeviceGroup"))
            throw new InvalidOperationException($"页面 {page} 仍有已删除的分组控件。");
    }
}
