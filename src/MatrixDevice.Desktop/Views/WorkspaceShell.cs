using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    private ColumnDefinition sidebarColumn = null!;
    private Grid workspace = null!;
    private readonly Dictionary<string, ToggleButton> navigation = [];
    private readonly List<FrameworkElement> sidebarCaptions = [];
    private FrameworkElement sidebarStatistics = null!;
    private TextBlock collapseGlyph = null!;
    private TextBlock workspaceCounts = null!;
    private Window? directoryWindow;
    private Window? scannerWindow;
    private bool sidebarCollapsed;
    private static readonly (string Tag, string Label, string Icon)[] WorkspacePages =
    [
        ("设备墙 / 主控", "设备管理", "\uE8EA"), ("雷电实例", "雷电实例", "\uE7F4"),
        ("节点", "节点调试", "\uE8A5"), ("截图 / 取色", "截图取色", "\uE722"),
        ("Shell", "命令终端", "\uE756"), ("应用", "应用管理", "\uE71D"),
        ("文件", "文件传输", "\uE8B7"), ("文本", "文本输入", "\uE8D2"),
        ("脚本", "脚本管理", "\uE8A5"), ("API", "接口服务", "\uE9D9"),
        ("扩展", "扩展工具", "\uE713"), ("设置", "环境设置", "\uE713")
    ];

    private static Brush WorkspaceBrush(string value) => (Brush)new BrushConverter().ConvertFromString(value)!;
    private static TextBlock ShellGlyph(string value, double size = 17) => new()
    {
        Text = value, FontFamily = new FontFamily("Segoe MDL2 Assets"), FontSize = size,
        VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center
    };

    private void BuildWorkspaceLayout()
    {
        sidebarColumn = new ColumnDefinition { Width = new GridLength(212) };
        Shell.ColumnDefinitions.Add(sidebarColumn);
        Shell.ColumnDefinitions.Add(new ColumnDefinition());
        Shell.Children.Add(BuildNavigation());
        workspace = new Grid();
        workspace.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        workspace.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        workspace.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        workspace.RowDefinitions.Add(new RowDefinition());
        workspace.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        Grid.SetColumn(workspace, 1); Shell.Children.Add(workspace);
        workspace.Children.Add(BuildSearchBar());

        var notice = new Border { Background = WorkspaceBrush("#FFF4F2"), Padding = new Thickness(20, 9, 20, 9) };
        var error = BoundText(nameof(WorkspaceViewModel.Error), true); error.Foreground = WorkspaceBrush("#AE3F39"); notice.Child = error;
        notice.SetBinding(VisibilityProperty, new Binding(nameof(WorkspaceViewModel.Error)) { Converter = new NonEmptyVisibilityConverter() });
        Grid.SetRow(notice, 1); workspace.Children.Add(notice);
        var context = BuildWorkspaceContext(); Grid.SetRow(context, 2); workspace.Children.Add(context);
        var footer = new Grid { Margin = new Thickness(20, 8, 20, 10) };
        footer.ColumnDefinitions.Add(new ColumnDefinition()); footer.ColumnDefinitions.Add(new ColumnDefinition());
        workspaceCounts = BoundText(nameof(WorkspaceViewModel.Counts)); workspaceCounts.FontSize = 11; workspaceCounts.Foreground = WorkspaceBrush("#63718A"); footer.Children.Add(workspaceCounts);
        var status = BoundText(nameof(WorkspaceViewModel.Status)); status.FontSize = 11; status.TextAlignment = TextAlignment.Right;
        status.TextTrimming = TextTrimming.CharacterEllipsis; status.SetBinding(ToolTipProperty, new Binding(nameof(WorkspaceViewModel.Status)));
        Grid.SetColumn(status, 1); footer.Children.Add(status); Grid.SetRow(footer, 4); workspace.Children.Add(footer);
    }

    private Border BuildNavigation()
    {
        var sidebar = new Border { Background = Brushes.White, BorderBrush = WorkspaceBrush("#E4E8F0"), BorderThickness = new Thickness(0, 0, 1, 0) };
        var dock = new DockPanel(); sidebar.Child = dock;
        var brand = new Grid { Margin = new Thickness(18, 24, 12, 22) };
        brand.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) }); brand.ColumnDefinitions.Add(new ColumnDefinition());
        var mark = new Border { Width = 36, Height = 36, CornerRadius = new CornerRadius(8), Background = WorkspaceBrush("#3274F6") };
        var icon = ShellGlyph("\uE80A", 20); icon.Foreground = Brushes.White; mark.Child = icon; brand.Children.Add(mark);
        var identity = new StackPanel { Margin = new Thickness(9, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center };
        identity.Children.Add(new TextBlock { Text = "矩阵设备", FontSize = 19, FontWeight = FontWeights.SemiBold });
        identity.Children.Add(new TextBlock { Text = "Android 设备工作台", FontSize = 11, Foreground = WorkspaceBrush("#63718A"), Margin = new Thickness(0, 4, 0, 0) });
        Grid.SetColumn(identity, 1); brand.Children.Add(identity); sidebarCaptions.Add(identity);
        DockPanel.SetDock(brand, Dock.Top); dock.Children.Add(brand);

        var bottom = new StackPanel(); DockPanel.SetDock(bottom, Dock.Bottom); dock.Children.Add(bottom);
        var collapseContent = new StackPanel { Orientation = Orientation.Horizontal };
        collapseGlyph = ShellGlyph("\uE76B", 14); collapseGlyph.Width = 24; collapseContent.Children.Add(collapseGlyph);
        var collapseText = new TextBlock { Text = "折叠侧栏", Margin = new Thickness(9, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center };
        collapseContent.Children.Add(collapseText); sidebarCaptions.Add(collapseText);
        var collapse = new Button { Content = collapseContent, ToolTip = "展开或折叠导航", Margin = new Thickness(12, 4, 12, 12), HorizontalContentAlignment = HorizontalAlignment.Left };
        collapse.SetResourceReference(StyleProperty, "SubtleButton"); collapse.Click += (_, _) => SetSidebarCollapsed(!sidebarCollapsed); bottom.Children.Add(collapse);
        var statistics = new StackPanel { Margin = new Thickness(18, 15, 14, 16) };
        statistics.Children.Add(new TextBlock { Text = "本机工作区", FontWeight = FontWeights.SemiBold, FontSize = 12 });
        var mode = BoundText(nameof(WorkspaceViewModel.ModeLabel), true); mode.Foreground = WorkspaceBrush("#63718A"); mode.FontSize = 11; mode.Margin = new Thickness(0, 7, 0, 5); statistics.Children.Add(mode);
        var summary = BoundText(nameof(WorkspaceViewModel.ApiSummary), true); summary.FontSize = 11; summary.Foreground = WorkspaceBrush("#63718A"); statistics.Children.Add(summary);
        sidebarStatistics = new Border { BorderBrush = WorkspaceBrush("#E4E8F0"), BorderThickness = new Thickness(0, 1, 0, 0), Child = statistics }; bottom.Children.Add(sidebarStatistics);

        var links = new StackPanel { Margin = new Thickness(10, 0, 10, 8) };
        foreach (var item in WorkspacePages)
        {
            if (item.Tag == "雷电实例") continue;
            var content = new StackPanel { Orientation = Orientation.Horizontal };
            var glyph = ShellGlyph(item.Icon); glyph.Width = 28; content.Children.Add(glyph);
            var caption = new TextBlock { Text = item.Label, Margin = new Thickness(10, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center };
            content.Children.Add(caption); sidebarCaptions.Add(caption);
            var link = new ToggleButton { Content = content, ToolTip = item.Label, Margin = new Thickness(0, 2, 0, 2), MinHeight = 40, HorizontalContentAlignment = HorizontalAlignment.Left };
            link.SetResourceReference(StyleProperty, "NavigationButton");
            System.Windows.Automation.AutomationProperties.SetName(link, item.Label);
            link.Click += (_, _) => NavigateTo(item.Tag);
            navigation.Add(item.Tag, link); links.Children.Add(link);
        }
        dock.Children.Add(new ScrollViewer { Content = links, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled });
        return sidebar;
    }

    private FrameworkElement BuildSearchBar()
    {
        var surface = new Border { Background = Brushes.White, BorderBrush = WorkspaceBrush("#E4E8F0"), BorderThickness = new Thickness(0, 0, 0, 1), Padding = new Thickness(20, 15, 20, 15) };
        var row = new Grid(); surface.Child = row;
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(158) }); row.ColumnDefinitions.Add(new ColumnDefinition());
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var group = new ComboBox { ItemsSource = ViewModel.Groups, ItemTemplate = CaptionChoices(), ToolTip = "设备分组", Margin = new Thickness(0, 0, 12, 0), MinHeight = 38 };
        group.SetBinding(Selector.SelectedItemProperty, TwoWay(nameof(WorkspaceViewModel.GroupFilter))); row.Children.Add(group);

        var search = new Grid { Margin = new Thickness(0, 0, 12, 0) };
        var input = BoundInput(nameof(WorkspaceViewModel.Search)); input.MinHeight = 38; input.Padding = new Thickness(33, 6, 30, 6);
        input.ToolTip = "搜索设备名称或序列号"; System.Windows.Automation.AutomationProperties.SetName(input, "搜索设备"); search.Children.Add(input);
        var searchIcon = ShellGlyph("\uE721", 14); searchIcon.Margin = new Thickness(12, 0, 0, 0); searchIcon.HorizontalAlignment = HorizontalAlignment.Left;
        searchIcon.Foreground = WorkspaceBrush("#8B96AB"); searchIcon.IsHitTestVisible = false; search.Children.Add(searchIcon);
        var hint = new TextBlock { Text = "搜索设备名称、序列号", Foreground = WorkspaceBrush("#63718A"), Margin = new Thickness(33, 0, 28, 0), VerticalAlignment = VerticalAlignment.Center, IsHitTestVisible = false, TextTrimming = TextTrimming.CharacterEllipsis };
        search.Children.Add(hint); input.TextChanged += (_, _) => hint.Visibility = input.Text.Length == 0 ? Visibility.Visible : Visibility.Collapsed;
        hint.Visibility = input.Text.Length == 0 ? Visibility.Visible : Visibility.Collapsed;
        Grid.SetColumn(search, 1); row.Children.Add(search);
        var settings = new Button { Content = ShellGlyph("\uE713"), ToolTip = "环境设置", MinHeight = 38 };
        settings.SetResourceReference(StyleProperty, "IconButton"); settings.Click += (_, _) => NavigateTo("设置"); Grid.SetColumn(settings, 4); row.Children.Add(settings);
        var advanced = new ContextMenu();
        var network = new MenuItem { Header = "网络连接（高级）" }; network.Click += (_, _) => ShowScanner(); advanced.Items.Add(network);
        var refresh = new MenuItem { Header = "立即更新目录" }; refresh.Click += async (_, _) => await UpdateDeviceDirectoryAsync(); advanced.Items.Add(refresh);
        settings.ContextMenu = advanced;
        return surface;
    }

    private FrameworkElement BuildWorkspaceContext()
    {
        var surface = new Border { Background = Brushes.White, BorderBrush = WorkspaceBrush("#E4E8F0"), BorderThickness = new Thickness(0, 0, 0, 1), Padding = new Thickness(20, 10, 20, 10) };
        var row = new Grid(); surface.Child = row;
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) }); row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) }); row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        foreach (var (label, path, index) in new[] { ("当前设备", nameof(WorkspaceViewModel.DeviceContext), 0), ("主控设备", nameof(WorkspaceViewModel.MainContext), 1), ("操作目标", nameof(WorkspaceViewModel.TargetContext), 2) })
        {
            var content = new StackPanel { Margin = new Thickness(0, 0, 15, 0) };
            content.Children.Add(new TextBlock { Text = label, FontSize = 11, Foreground = WorkspaceBrush("#63718A"), Margin = new Thickness(0, 0, 0, 4) });
            var value = BoundText(path); value.FontSize = 12; value.TextTrimming = TextTrimming.CharacterEllipsis; value.SetBinding(ToolTipProperty, new Binding(path)); content.Children.Add(value);
            Grid.SetColumn(content, index); row.Children.Add(content);
        }
        var details = new Button { Content = ShellGlyph("\uE8A7"), ToolTip = "设备详情与分组", VerticalAlignment = VerticalAlignment.Center };
        details.SetResourceReference(StyleProperty, "IconButton"); details.Click += (_, _) => ShowDeviceDirectory(); Grid.SetColumn(details, 3); row.Children.Add(details);
        return surface;
    }

    private static ControlTemplate WorkspaceContentTemplate()
    {
        var presenter = new FrameworkElementFactory(typeof(ContentPresenter), "PART_SelectedContentHost");
        presenter.SetBinding(ContentPresenter.ContentProperty, new Binding(nameof(TabControl.SelectedContent)) { RelativeSource = RelativeSource.TemplatedParent });
        presenter.SetBinding(ContentPresenter.ContentTemplateProperty, new Binding(nameof(TabControl.SelectedContentTemplate)) { RelativeSource = RelativeSource.TemplatedParent });
        presenter.SetValue(HorizontalAlignmentProperty, HorizontalAlignment.Stretch); presenter.SetValue(VerticalAlignmentProperty, VerticalAlignment.Stretch);
        return new ControlTemplate(typeof(TabControl)) { VisualTree = presenter };
    }

    private void AttachWorkspacePages()
    {
        Grid.SetRow(tabs, 3); workspace.Children.Add(tabs); UpdateNavigation();
    }

    internal void NavigateTo(string tag)
    {
        var page = tabs.Items.Cast<TabItem>().SingleOrDefault(item => Equals(item.Tag, tag))
            ?? throw new ArgumentException("导航页面不存在：" + tag);
        tabs.SelectedItem = page; UpdateNavigation();
    }

    private void UpdateNavigation()
    {
        var selected = (tabs.SelectedItem as TabItem)?.Tag?.ToString();
        var activeTag = selected == "雷电实例" ? "设备墙 / 主控" : selected;
        foreach (var item in navigation) item.Value.IsChecked = item.Key == activeTag;
        foreach (var mode in deviceModes) mode.Button.IsChecked = mode.Tag == selected;
        if (activeTag is not null && navigation.TryGetValue(activeTag, out var active) && active.IsLoaded) active.BringIntoView();
        var label = WorkspacePages.FirstOrDefault(page => page.Tag == selected).Label ?? "设备管理";
        Title = "矩阵设备 · " + label + (ViewModel.Demo ? " · 演示模式" : "");
    }

    private void SetSidebarCollapsed(bool collapsed)
    {
        sidebarCollapsed = collapsed; sidebarColumn.Width = new GridLength(collapsed ? 72 : 212);
        foreach (var caption in sidebarCaptions) caption.Visibility = collapsed ? Visibility.Collapsed : Visibility.Visible;
        sidebarStatistics.Visibility = collapsed ? Visibility.Collapsed : Visibility.Visible;
        collapseGlyph.Text = collapsed ? "\uE76C" : "\uE76B";
    }

    private void ShowDeviceDirectory()
    {
        if (directoryWindow is null)
        {
            directoryWindow = new Window { Owner = this, DataContext = ViewModel, Title = "设备与分组", Width = 420, Height = 640, MinWidth = 360, MinHeight = 480, WindowStartupLocation = WindowStartupLocation.CenterOwner };
            directoryWindow.Content = BuildDirectory();
            directoryWindow.Closing += (_, args) => { if (!closing) { args.Cancel = true; directoryWindow.Hide(); } };
        }
        directoryWindow.Show(); directoryWindow.Activate();
    }

    private void ShowScanner()
    {
        if (scannerWindow is null)
        {
            var scanner = BuildScanner();
            if (scanner is Expander expander) expander.IsExpanded = true;
            scannerWindow = new Window { Owner = this, DataContext = ViewModel, Title = "网络连接（高级）", Width = 460, Height = 420, MinWidth = 360, MinHeight = 360, WindowStartupLocation = WindowStartupLocation.CenterOwner };
            scannerWindow.Content = new ScrollViewer { Content = new Border { Padding = new Thickness(20), Child = scanner }, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled };
            scannerWindow.Closing += (_, args) => { if (!closing) { scanCancellation?.Cancel(); args.Cancel = true; scannerWindow.Hide(); } };
        }
        scannerWindow.Show(); scannerWindow.Activate();
    }
}
