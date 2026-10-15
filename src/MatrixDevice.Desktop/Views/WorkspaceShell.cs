using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Input;

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
    private FrameworkElement singleTargetLabel = null!;
    private FrameworkElement singleTargetChoice = null!;
    private Border navigationPane = null!, navigationBackdrop = null!;
    private Button navigationMenuButton = null!;
    private Grid workspaceToolbar = null!, singleTargetGroup = null!, workspaceFooter = null!;
    private TextBlock workspaceStatus = null!;
    private bool navigationDrawerOpen;
    private bool? sidebarPreference;
    private IInputElement? navigationReturnFocus;
    private static readonly (string Tag, string Icon)[] WorkspacePages =
    [
        ("设备墙 / 主控", "\uE8EA"), ("实时监控", "\uE9D9"),
        ("节点", "\uE8A5"), ("截图 / 取色", "\uE722"), ("Shell", "\uE756"), ("应用", "\uE71D"),
        ("文件", "\uE8B7"), ("文本", "\uE8D2"), ("脚本", "\uE8A5"), ("API", "\uE9D9"),
        ("扩展", "\uE713"), ("设置", "\uE713")
    ];

    private static Brush WorkspaceBrush(string value) => (Brush)new BrushConverter().ConvertFromString(value)!;
    private static Brush ThemeBrush(string key) => (Brush)Application.Current.FindResource(key);
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
        navigationPane = BuildNavigation(); Shell.Children.Add(navigationPane);
        workspace = new Grid();
        workspace.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        workspace.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        workspace.RowDefinitions.Add(new RowDefinition());
        workspace.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        Grid.SetColumn(workspace, 1); Shell.Children.Add(workspace);
        workspace.Children.Add(BuildWorkspaceToolbar());

        var notice = new Border { Background = ThemeBrush("DangerSurfaceBrush"), Padding = new Thickness(20, 9, 20, 9) };
        var error = BoundText(nameof(WorkspaceViewModel.Error), true); error.Foreground = ThemeBrush("DangerBrush"); notice.Child = new ScrollViewer { Content = error, MaxHeight = 100, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled };
        notice.SetBinding(VisibilityProperty, new Binding(nameof(WorkspaceViewModel.Error)) { Converter = new NonEmptyVisibilityConverter() });
        Grid.SetRow(notice, 1); workspace.Children.Add(notice);
        var footer = new Grid { Margin = new Thickness(20, 8, 20, 10) };
        workspaceFooter = footer; footer.RowDefinitions.Add(new() { Height = GridLength.Auto }); footer.RowDefinitions.Add(new() { Height = GridLength.Auto });
        footer.ColumnDefinitions.Add(new ColumnDefinition()); footer.ColumnDefinitions.Add(new ColumnDefinition());
        workspaceCounts = BoundText(nameof(WorkspaceViewModel.Counts)); workspaceCounts.FontSize = 11; workspaceCounts.Foreground = ThemeBrush("MutedBrush"); footer.Children.Add(workspaceCounts);
        workspaceCounts.TextTrimming = TextTrimming.CharacterEllipsis; workspaceCounts.SetBinding(ToolTipProperty, new Binding(nameof(TextBlock.Text)) { Source = workspaceCounts });
        var status = BoundText(nameof(WorkspaceViewModel.Status)); status.FontSize = 11; status.TextAlignment = TextAlignment.Right;
        workspaceStatus = status;
        status.TextTrimming = TextTrimming.CharacterEllipsis; status.SetBinding(ToolTipProperty, new Binding(nameof(WorkspaceViewModel.Status)));
        Grid.SetColumn(status, 1); footer.Children.Add(status); Grid.SetRow(footer, 3); workspace.Children.Add(footer);
        navigationBackdrop = new Border { Background = new SolidColorBrush(Color.FromArgb(50, 0, 0, 0)), Visibility = Visibility.Collapsed };
        Grid.SetColumnSpan(navigationBackdrop, 2); Panel.SetZIndex(navigationBackdrop, 10); Shell.Children.Add(navigationBackdrop);
        navigationBackdrop.MouseDown += (_, args) => { CloseNavigationDrawer(); args.Handled = true; };
    }

    private Border BuildNavigation()
    {
        var sidebar = new Border { Background = ThemeBrush("SurfaceBrush"), BorderBrush = ThemeBrush("BorderBrush"), BorderThickness = new Thickness(0, 0, 1, 0) };
        var dock = new DockPanel(); sidebar.Child = dock;
        var brand = new Grid { Margin = new Thickness(18, 24, 12, 22) };
        brand.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) }); brand.ColumnDefinitions.Add(new ColumnDefinition());
        var mark = new Border { Width = 36, Height = 36, CornerRadius = new CornerRadius(8), Background = ThemeBrush("AccentBrush") };
        var icon = ShellGlyph("\uE80A", 20); icon.Foreground = Brushes.White; mark.Child = icon; brand.Children.Add(mark);
        var identity = new StackPanel { Margin = new Thickness(9, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center };
        identity.Children.Add(new TextBlock { Text = "矩阵设备", FontSize = 19, FontWeight = FontWeights.SemiBold });
        identity.Children.Add(new TextBlock { Text = "Android 设备工作台", FontSize = 11, Foreground = ThemeBrush("MutedBrush"), Margin = new Thickness(0, 4, 0, 0) });
        Grid.SetColumn(identity, 1); brand.Children.Add(identity); sidebarCaptions.Add(identity);
        DockPanel.SetDock(brand, Dock.Top); dock.Children.Add(brand);

        var bottom = new StackPanel(); DockPanel.SetDock(bottom, Dock.Bottom); dock.Children.Add(bottom);
        var collapseContent = new StackPanel { Orientation = Orientation.Horizontal };
        collapseGlyph = ShellGlyph("\uE76B", 14); collapseGlyph.Width = 24; collapseContent.Children.Add(collapseGlyph);
        var collapseText = new TextBlock { Text = "折叠侧栏", Margin = new Thickness(9, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center };
        collapseContent.Children.Add(collapseText); sidebarCaptions.Add(collapseText);
        var collapse = new Button { Content = collapseContent, ToolTip = "展开或折叠导航", Margin = new Thickness(12, 4, 12, 12), HorizontalContentAlignment = HorizontalAlignment.Left };
        collapse.Name = "NavigationToggle";
        System.Windows.Automation.AutomationProperties.SetAutomationId(collapse, collapse.Name);
        System.Windows.Automation.AutomationProperties.SetName(collapse, "折叠导航");
        collapse.SetResourceReference(StyleProperty, "SubtleButton"); collapse.Click += (_, _) => { if (ActualWidth < 700) CloseNavigationDrawer(); else SetSidebarCollapsed(!sidebarCollapsed); }; bottom.Children.Add(collapse);
        var statistics = new StackPanel { Margin = new Thickness(18, 15, 14, 16) };
        statistics.Children.Add(new TextBlock { Text = "本机工作区", FontWeight = FontWeights.SemiBold, FontSize = 12 });
        var mode = BoundText(nameof(WorkspaceViewModel.ModeLabel), true); mode.Foreground = ThemeBrush("MutedBrush"); mode.FontSize = 11; mode.Margin = new Thickness(0, 7, 0, 5); statistics.Children.Add(mode);
        var summary = BoundText(nameof(WorkspaceViewModel.ApiSummary), true); summary.FontSize = 11; summary.Foreground = ThemeBrush("MutedBrush"); statistics.Children.Add(summary);
        sidebarStatistics = new Border { BorderBrush = ThemeBrush("BorderBrush"), BorderThickness = new Thickness(0, 1, 0, 0), Child = statistics }; bottom.Children.Add(sidebarStatistics);

        var links = new StackPanel { Margin = new Thickness(10, 0, 10, 8) };
        foreach (var item in WorkspacePages)
        {
            var label = LocalizationService.PageDisplayName(item.Tag);
            var content = new StackPanel { Orientation = Orientation.Horizontal };
            var glyph = ShellGlyph(item.Icon); glyph.Width = 28; content.Children.Add(glyph);
            var caption = new TextBlock { Text = label, Margin = new Thickness(10, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center };
            content.Children.Add(caption); sidebarCaptions.Add(caption);
            var link = new ToggleButton { Content = content, ToolTip = label, Margin = new Thickness(0, 2, 0, 2), MinHeight = 40, HorizontalContentAlignment = HorizontalAlignment.Left };
            link.SetResourceReference(StyleProperty, "NavigationButton");
            System.Windows.Automation.AutomationProperties.SetName(link, label);
            System.Windows.Automation.AutomationProperties.SetAutomationId(link, "Navigation." + item.Tag);
            link.Click += (_, _) => NavigateTo(item.Tag);
            navigation.Add(item.Tag, link); links.Children.Add(link);
        }
        dock.Children.Add(new ScrollViewer { Content = links, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled });
        return sidebar;
    }

    private FrameworkElement BuildWorkspaceToolbar()
    {
        var surface = new Border { Background = ThemeBrush("SurfaceBrush"), BorderBrush = ThemeBrush("BorderBrush"), BorderThickness = new Thickness(0, 0, 0, 1), Padding = new Thickness(20, 10, 20, 10) };
        var row = new Grid(); surface.Child = row;
        workspaceToolbar = row;
        row.RowDefinitions.Add(new() { Height = GridLength.Auto }); row.RowDefinitions.Add(new() { Height = GridLength.Auto });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        row.ColumnDefinitions.Add(new ColumnDefinition());
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        navigationMenuButton = new Button { Name = "WorkspaceNavigation", Content = ShellGlyph("\uE700"), Width = 38, Height = 38, ToolTip = "导航", Visibility = Visibility.Collapsed };
        navigationMenuButton.SetResourceReference(StyleProperty, "IconButton"); System.Windows.Automation.AutomationProperties.SetName(navigationMenuButton, "打开导航"); System.Windows.Automation.AutomationProperties.SetAutomationId(navigationMenuButton, navigationMenuButton.Name);
        navigationMenuButton.Click += (_, _) => { if (navigationDrawerOpen) CloseNavigationDrawer(); else OpenNavigationDrawer(); }; row.Children.Add(navigationMenuButton);
        singleTargetGroup = new Grid(); singleTargetGroup.ColumnDefinitions.Add(new() { Width = GridLength.Auto }); singleTargetGroup.ColumnDefinitions.Add(new());
        Grid.SetColumn(singleTargetGroup, 1); row.Children.Add(singleTargetGroup);
        var targetLabel = new TextBlock { Text = "单设备目标", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(2, 0, 10, 0), Foreground = ThemeBrush("MutedBrush") };
        singleTargetLabel = targetLabel;
        singleTargetGroup.Children.Add(targetLabel);
        var target = new ComboBox { Name = "SingleDeviceTarget", ItemsSource = new ListCollectionView(ViewModel.Devices), ItemTemplate = OperationTargetTemplate(), IsSynchronizedWithCurrentItem = false, MinHeight = 38, MaxWidth = 520, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0, 0, 10, 0), ToolTip = "节点、截图、命令等单设备操作目标" };
        singleTargetChoice = target;
        target.SetBinding(Selector.SelectedItemProperty, TwoWay(nameof(WorkspaceViewModel.CurrentDevice)));
        System.Windows.Automation.AutomationProperties.SetName(target, "单设备目标"); Grid.SetColumn(target, 1); singleTargetGroup.Children.Add(target);
        var details = new Button { Name = "DeviceDetails", Content = ShellGlyph("\uE8A7"), ToolTip = "设备详情", MinHeight = 38, Margin = new Thickness(0, 0, 5, 0) };
        System.Windows.Automation.AutomationProperties.SetName(details, "设备详情"); System.Windows.Automation.AutomationProperties.SetAutomationId(details, details.Name);
        details.SetResourceReference(StyleProperty, "IconButton"); details.Click += (_, _) => ShowDeviceDirectory(); Grid.SetColumn(details, 2); row.Children.Add(details);
        var settings = new Button { Name = "WorkspaceSettings", Content = ShellGlyph("\uE713"), ToolTip = "设置", MinHeight = 38 };
        System.Windows.Automation.AutomationProperties.SetName(settings, "设置"); System.Windows.Automation.AutomationProperties.SetAutomationId(settings, settings.Name);
        settings.SetResourceReference(StyleProperty, "IconButton"); settings.Click += (_, _) => NavigateTo("设置"); Grid.SetColumn(settings, 4); row.Children.Add(settings);
        var advanced = new ContextMenu();
        var network = new MenuItem { Header = "网络连接（高级）" }; network.Click += (_, _) => ShowScanner(); advanced.Items.Add(network);
        var refresh = new MenuItem { Header = "立即更新目录" }; refresh.Click += async (_, _) => await UpdateDeviceDirectoryAsync(); advanced.Items.Add(refresh);
        var more = new Button { Name = "WorkspaceMore", Content = ShellGlyph("\uE712"), ToolTip = "更多", MinHeight = 38, ContextMenu = advanced };
        System.Windows.Automation.AutomationProperties.SetName(more, "更多操作"); System.Windows.Automation.AutomationProperties.SetAutomationId(more, more.Name); more.SetResourceReference(StyleProperty, "IconButton");
        more.Click += (_, _) => { advanced.PlacementTarget = more; advanced.Placement = PlacementMode.Bottom; advanced.IsOpen = true; };
        advanced.PreviewKeyDown += (_, args) => { if (args.Key == Key.Escape) { advanced.IsOpen = false; args.Handled = true; } };
        advanced.Closed += (_, _) => more.Focus();
        Grid.SetColumn(more, 3); row.Children.Add(more);
        return surface;
    }

    private static DataTemplate OperationTargetTemplate()
    {
        var text = new FrameworkElementFactory(typeof(TextBlock));
        text.SetValue(TextBlock.TextTrimmingProperty, TextTrimming.CharacterEllipsis);
        var caption = new MultiBinding { StringFormat = "{0} · {1}" };
        caption.Bindings.Add(new Binding(nameof(DeviceRow.Name))); caption.Bindings.Add(new Binding(nameof(DeviceRow.Serial)));
        text.SetBinding(TextBlock.TextProperty, caption);
        return new DataTemplate { VisualTree = text };
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
        Grid.SetRow(tabs, 2); workspace.Children.Add(tabs); UpdateNavigation();
    }

    internal void NavigateTo(string tag)
    {
        var page = tabs.Items.Cast<TabItem>().SingleOrDefault(item => Equals(item.Tag, tag))
            ?? throw new ArgumentException("导航页面不存在：" + tag);
        tabs.SelectedItem = page; UpdateNavigation();
        if (navigationDrawerOpen) CloseNavigationDrawer();
    }

    private void UpdateNavigation()
    {
        var selected = (tabs.SelectedItem as TabItem)?.Tag?.ToString();
        var targetVisibility = selected is "节点" or "截图 / 取色" or "Shell" or "文件" or "文本" or "脚本" or "API" or "扩展" ? Visibility.Visible : Visibility.Collapsed;
        singleTargetLabel.Visibility = singleTargetChoice.Visibility = targetVisibility;
        singleTargetGroup.Visibility = targetVisibility;
        foreach (var item in navigation) item.Value.IsChecked = item.Key == selected;
        if (selected is not null && navigation.TryGetValue(selected, out var active) && active.IsLoaded) active.BringIntoView();
        var label = LocalizationService.PageDisplayName(selected ?? "设备墙 / 主控");
        Title = "矩阵设备 · " + label + (ViewModel.Demo ? " · 演示模式" : "");
    }

    private void SetSidebarCollapsed(bool collapsed)
    {
        sidebarPreference = collapsed; ApplyResponsiveWorkspace();
    }

    private void ApplyResponsiveWorkspace()
    {
        if (ActualWidth <= 0 || workspaceToolbar is null) return;
        var narrow = ActualWidth < 700;
        if (!narrow && navigationDrawerOpen) { navigationDrawerOpen = false; navigationReturnFocus = null; }
        sidebarCollapsed = narrow ? !navigationDrawerOpen : sidebarPreference ?? ActualWidth < 1100;
        sidebarColumn.Width = new GridLength(narrow ? 0 : sidebarCollapsed ? 72 : 212);
        navigationPane.Visibility = !narrow || navigationDrawerOpen ? Visibility.Visible : Visibility.Collapsed;
        navigationPane.Width = narrow ? Math.Min(280, Math.Max(1, Shell.ActualWidth - 48)) : double.NaN;
        navigationPane.HorizontalAlignment = narrow ? HorizontalAlignment.Left : HorizontalAlignment.Stretch;
        Grid.SetColumnSpan(navigationPane, narrow ? 2 : 1); Panel.SetZIndex(navigationPane, narrow ? 20 : 0);
        navigationBackdrop.Visibility = narrow && navigationDrawerOpen ? Visibility.Visible : Visibility.Collapsed;
        workspace.IsEnabled = !navigationDrawerOpen;
        KeyboardNavigation.SetTabNavigation(navigationPane, narrow ? KeyboardNavigationMode.Cycle : KeyboardNavigationMode.Continue);
        navigationMenuButton.Visibility = narrow ? Visibility.Visible : Visibility.Collapsed;
        Grid.SetRow(singleTargetGroup, narrow ? 1 : 0); Grid.SetColumn(singleTargetGroup, narrow ? 0 : 1); Grid.SetColumnSpan(singleTargetGroup, narrow ? 5 : 1);
        singleTargetGroup.Margin = narrow ? new Thickness(0, 8, 0, 0) : new Thickness(0);
        Grid.SetRow(workspaceStatus, narrow ? 1 : 0); Grid.SetColumn(workspaceStatus, narrow ? 0 : 1); Grid.SetColumnSpan(workspaceStatus, narrow ? 2 : 1);
        Grid.SetColumnSpan(workspaceCounts, narrow ? 2 : 1); workspaceStatus.TextAlignment = narrow ? TextAlignment.Left : TextAlignment.Right;
        foreach (var caption in sidebarCaptions) caption.Visibility = sidebarCollapsed ? Visibility.Collapsed : Visibility.Visible;
        sidebarStatistics.Visibility = sidebarCollapsed ? Visibility.Collapsed : Visibility.Visible;
        foreach (var link in navigation.Values) link.Padding = sidebarCollapsed ? new Thickness(8, 9, 8, 9) : new Thickness(14, 9, 14, 9);
        collapseGlyph.Text = sidebarCollapsed ? "\uE76C" : "\uE76B";
        if (collapseGlyph.Parent is StackPanel { Parent: Button collapse }) System.Windows.Automation.AutomationProperties.SetName(collapse, narrow ? "关闭导航" : sidebarCollapsed ? "展开导航" : "折叠导航");
    }

    private void OpenNavigationDrawer()
    {
        if (ActualWidth >= 700) return;
        navigationReturnFocus = Keyboard.FocusedElement; navigationDrawerOpen = true; ApplyResponsiveWorkspace();
        if ((tabs.SelectedItem as TabItem)?.Tag is string tag) { navigation[tag].BringIntoView(); navigation[tag].Focus(); }
    }

    private void CloseNavigationDrawer()
    {
        if (!navigationDrawerOpen) return;
        navigationDrawerOpen = false; ApplyResponsiveWorkspace();
        var previous = navigationReturnFocus as UIElement;
        void ReturnFocus()
        {
            if (navigationDrawerOpen || ActualWidth >= 700 || closing) return;
            if (previous is { IsVisible: true, IsEnabled: true }) previous.Focus(); else navigationMenuButton.Focus();
        }
        ReturnFocus();
        Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Input, (Action)ReturnFocus);
        navigationReturnFocus = null;
    }

    private void ShowDeviceDirectory()
    {
        if (directoryWindow is null)
        {
            directoryWindow = new Window { Owner = this, DataContext = ViewModel, Title = "设备详情", Width = 420, Height = 640, MinWidth = 360, MinHeight = 480, WindowStartupLocation = WindowStartupLocation.CenterOwner };
            directoryWindow.Content = BuildDirectory();
            directoryWindow.Closing += (_, args) => { if (!closing) { args.Cancel = true; directoryWindow.Hide(); } };
        }
        ResponsiveLayout.FitOwnedWindow(directoryWindow, this, 420, 640); directoryWindow.Show(); directoryWindow.Activate();
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
        ResponsiveLayout.FitOwnedWindow(scannerWindow, this, 460, 420); scannerWindow.Show(); scannerWindow.Activate();
    }
}
