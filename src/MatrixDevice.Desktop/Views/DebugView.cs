using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MatrixDevice.Core;
using Microsoft.Win32;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    private FrameworkElement BuildNodesPage()
    {
        var page = Page();
        var toolbar = Toolbar(); toolbar.Children.Add(Button("\uE72C", "刷新截图和节点", ViewModel.DumpCommand)); toolbar.Children.Add(Button("\uE74E", "导出 JSON", async () => { var dialog = new SaveFileDialog { Filter = "JSON|*.json", FileName = "ui-tree.json" }; if (dialog.ShowDialog(this) == true) await ViewModel.ExportNodesAsync(dialog.FileName); }));
        toolbar.Children.Add(Button("\uE8A3", "展开", () => SetExpanded(true))); toolbar.Children.Add(Button("\uE8A4", "折叠", () => SetExpanded(false)));
        foreach (var button in toolbar.Children.OfType<Button>()) { button.MinWidth = 44; button.MinHeight = 44; }
        DockPanel.SetDock(toolbar, Dock.Top); page.Children.Add(toolbar);
        var panes = new Grid { Visibility = Visibility.Collapsed, Margin = new Thickness(0, 8, 0, 0) };
        DockPanel.SetDock(panes, Dock.Top); page.Children.Add(panes);
        var grid = new Grid { Margin = new Thickness(0, 10, 0, 0), ClipToBounds = true };
        grid.ColumnDefinitions.Add(new ColumnDefinition()); grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(300) }); grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(270) });
        grid.RowDefinitions.Add(new RowDefinition()); grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0) });
        nodeCanvas = new Canvas { Background = ThemeBrush("CanvasBrush") };
        var viewbox = new Viewbox { Child = nodeCanvas, Stretch = Stretch.Uniform, VerticalAlignment = VerticalAlignment.Center }; grid.Children.Add(viewbox);
        nodeTree = new TreeView { Margin = new Thickness(8, 0, 8, 0), BorderBrush = ThemeBrush("BorderBrush") }; nodeTree.SelectedItemChanged += (_, _) => { if (nodeTree.SelectedItem is TreeViewItem { Tag: NodeRow row }) ViewModel.SelectedNode = row; }; Grid.SetColumn(nodeTree, 1); grid.Children.Add(nodeTree);
        var properties = new DockPanel(); var label = Label("节点属性"); DockPanel.SetDock(label, Dock.Top); properties.Children.Add(label); var text = BoundInput(nameof(WorkspaceViewModel.NodeProperties), readOnly: true, multiline: true); text.FontFamily = new FontFamily("Consolas"); properties.Children.Add(text); Grid.SetColumn(properties, 2); grid.Children.Add(properties);
        // Reflow may clamp offsets; retain the original intent until that viewport returns.
        (ScrollViewer Scroll, double Horizontal, double Vertical, double Width, double Height, double ExtentWidth, double ExtentHeight, string Value, int Start, int Length, int Caret)? propertyIntent = null;
        long propertyScrollTicket = 0;
        var propertyBaselineReady = false;
        System.Windows.Threading.DispatcherOperation? propertyBaselineOperation = null;
        System.Windows.Threading.DispatcherOperation? propertyRestoreOperation = null;
        text.Loaded += (_, _) => QueuePropertyBaseline();
        text.IsVisibleChanged += (_, _) => { if (text.IsVisible) { if (propertyIntent is not null) QueuePropertyRestore(); else QueuePropertyBaseline(); } };
        text.TextChanged += (_, _) => CancelPropertyRestore();
        text.SelectionChanged += (_, _) => { if (propertyIntent is { } saved && !PropertyContentMatches(saved.Value, saved.Start, saved.Length, saved.Caret)) CancelPropertyRestore(); };
        text.AddHandler(Mouse.PreviewMouseDownEvent, new MouseButtonEventHandler((_, _) => CancelPropertyRestore()), true);
        text.AddHandler(Mouse.PreviewMouseWheelEvent, new MouseWheelEventHandler((_, _) => CancelPropertyRestore()), true);
        text.AddHandler(Keyboard.PreviewKeyDownEvent, new KeyEventHandler((_, args) => { if (args.Key is Key.Up or Key.Down or Key.Left or Key.Right or Key.PageUp or Key.PageDown or Key.Home or Key.End || args.Key == Key.A && Keyboard.Modifiers.HasFlag(ModifierKeys.Control)) CancelPropertyRestore(); }), true);
        text.AddHandler(ScrollViewer.ScrollChangedEvent, new ScrollChangedEventHandler((_, args) =>
        {
            if (args.OriginalSource is not ScrollViewer scroll || !ReferenceEquals(text.Template?.FindName("PART_ContentHost", text), scroll)) return;
            if (!propertyBaselineReady) { QueuePropertyBaseline(); return; }
            var geometryChanged = args.ViewportWidthChange != 0 || args.ViewportHeightChange != 0 || args.ExtentWidthChange != 0 || args.ExtentHeightChange != 0;
            if (geometryChanged && propertyIntent is null)
            {
                var width = args.ViewportWidth - args.ViewportWidthChange;
                var height = args.ViewportHeight - args.ViewportHeightChange;
                if (width > 0 && height > 0)
                    propertyIntent = (scroll, args.HorizontalOffset - args.HorizontalChange, args.VerticalOffset - args.VerticalChange, width, height, args.ExtentWidth - args.ExtentWidthChange, args.ExtentHeight - args.ExtentHeightChange, text.Text, text.SelectionStart, text.SelectionLength, text.CaretIndex);
            }
            if (propertyIntent is not { } saved) return;
            if (!PropertyContentMatches(saved.Value, saved.Start, saved.Length, saved.Caret)) { CancelPropertyRestore(); return; }
            var originalGeometry = Math.Abs(scroll.ViewportWidth - saved.Width) < .5 && Math.Abs(scroll.ViewportHeight - saved.Height) < .5 && Math.Abs(scroll.ExtentWidth - saved.ExtentWidth) < .5 && Math.Abs(scroll.ExtentHeight - saved.ExtentHeight) < .5;
            if (originalGeometry && Math.Abs(scroll.HorizontalOffset - Math.Min(saved.Horizontal, scroll.ScrollableWidth)) < .5 && Math.Abs(scroll.VerticalOffset - Math.Min(saved.Vertical, scroll.ScrollableHeight)) < .5)
            {
                propertyIntent = null; propertyScrollTicket++; propertyRestoreOperation?.Abort(); propertyRestoreOperation = null;
            }
            else if (geometryChanged) QueuePropertyRestore();
        }), true);
        var paneButtons = new Dictionary<string, ToggleButton>();
        var selectedPane = "画面";
        var layout = -1;
        var updatingPanes = false;
        foreach (var caption in new[] { "画面", "节点", "属性" })
        {
            panes.ColumnDefinitions.Add(new ColumnDefinition());
            var button = new ToggleButton { MinWidth = 44, MinHeight = 44, Height = 44, Padding = new Thickness(6, 0, 6, 0), Margin = new Thickness(0, 0, caption == "属性" ? 0 : 4, 0), IsChecked = caption == selectedPane };
            button.SetResourceReference(StyleProperty, "SegmentedToggle");
            LocalizationService.Bind(button, ContentControl.ContentProperty, caption);
            LocalizationService.Bind(button, System.Windows.Automation.AutomationProperties.NameProperty, caption);
            LocalizationService.Bind(button, ToolTipProperty, caption);
            button.Checked += (_, _) => { if (!updatingPanes) { selectedPane = caption; ArrangeNodes(); } };
            button.Unchecked += (_, _) => { if (!updatingPanes && selectedPane == caption) ArrangeNodes(); };
            Grid.SetColumn(button, paneButtons.Count); paneButtons.Add(caption, button); panes.Children.Add(button);
        }
        grid.SizeChanged += (_, _) => ArrangeNodes();
        grid.Loaded += (_, _) => ArrangeNodes();
        page.Children.Add(grid); return page;

        void ArrangeNodes()
        {
            if (grid.ActualWidth <= 0) return;
            var treeFocus = nodeTree.IsKeyboardFocusWithin;
            var propertyFocus = text.IsKeyboardFocusWithin;
            var next = grid.ActualWidth >= 920 ? 2 : grid.ActualWidth >= 640 ? 1 : 0;
            if (next != layout)
            {
                if (next == 0)
                {
                    if (propertyFocus) selectedPane = "属性";
                    else if (treeFocus) selectedPane = "节点";
                }
                layout = next;
                grid.ColumnDefinitions[1].Width = new GridLength(next == 0 ? 0 : 300);
                grid.ColumnDefinitions[2].Width = new GridLength(next == 2 ? 270 : 0);
                grid.RowDefinitions[1].Height = next == 1 ? new GridLength(1, GridUnitType.Star) : new GridLength(0);
                Grid.SetColumn(nodeTree, next == 0 ? 0 : 1);
                Grid.SetColumn(properties, next == 2 ? 2 : next == 1 ? 1 : 0);
                Grid.SetRow(properties, next == 1 ? 1 : 0);
                Grid.SetRowSpan(viewbox, next == 1 ? 2 : 1);
                nodeTree.Margin = next == 0 ? new Thickness(0) : new Thickness(8, 0, 8, 0);
            }
            panes.Visibility = layout == 0 ? Visibility.Visible : Visibility.Collapsed;
            viewbox.Visibility = layout != 0 || selectedPane == "画面" ? Visibility.Visible : Visibility.Collapsed;
            nodeTree.Visibility = layout != 0 || selectedPane == "节点" ? Visibility.Visible : Visibility.Collapsed;
            properties.Visibility = layout != 0 || selectedPane == "属性" ? Visibility.Visible : Visibility.Collapsed;
            updatingPanes = true;
            try { foreach (var (caption, button) in paneButtons) button.IsChecked = caption == selectedPane; }
            finally { updatingPanes = false; }
            if (layout == 0 && (treeFocus && selectedPane != "节点" || propertyFocus && selectedPane != "属性")) paneButtons[selectedPane].Focus();
        }

        bool PropertyContentMatches(string value, int start, int length, int caret) => text.Text == value && text.SelectionStart == start && text.SelectionLength == length && text.CaretIndex == caret;

        void QueuePropertyBaseline()
        {
            if (propertyBaselineReady || propertyIntent is not null) return;
            propertyBaselineOperation?.Abort();
            var ticket = propertyScrollTicket;
            var value = text.Text;
            propertyBaselineOperation = Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ContextIdle, (Action)(() =>
            {
                propertyBaselineOperation = null;
                if (ticket == propertyScrollTicket && text.IsLoaded && text.IsVisible && text.Text == value && text.Template?.FindName("PART_ContentHost", text) is ScrollViewer { ViewportWidth: > 0, ViewportHeight: > 0 }) propertyBaselineReady = true;
            }));
        }

        void CancelPropertyRestore()
        {
            propertyIntent = null; propertyScrollTicket++; propertyBaselineReady = false;
            propertyRestoreOperation?.Abort(); propertyRestoreOperation = null;
            propertyBaselineOperation?.Abort(); propertyBaselineOperation = null;
            QueuePropertyBaseline();
        }

        void QueuePropertyRestore()
        {
            if (propertyIntent is null) return;
            propertyRestoreOperation?.Abort();
            var ticket = propertyScrollTicket;
            propertyRestoreOperation = Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ContextIdle, (Action)(() =>
            {
                propertyRestoreOperation = null;
                if (ticket != propertyScrollTicket || !text.IsLoaded || !text.IsVisible || propertyIntent is not { } saved || !ReferenceEquals(text.Template?.FindName("PART_ContentHost", text), saved.Scroll) || !PropertyContentMatches(saved.Value, saved.Start, saved.Length, saved.Caret)) return;
                saved.Scroll.ScrollToHorizontalOffset(Math.Min(saved.Horizontal, saved.Scroll.ScrollableWidth));
                saved.Scroll.ScrollToVerticalOffset(Math.Min(saved.Vertical, saved.Scroll.ScrollableHeight));
            }));
        }
    }

    private void RebuildNodes()
    {
        if (nodeTree is null || nodeCanvas is null) return;
        nodeTree.Items.Clear(); nodeCanvas.Children.Clear(); nodeTreeItems.Clear(); nodeRectangles.Clear();
        if (ViewModel.DebugImage is { } screenshot) { nodeCanvas.Width = screenshot.PixelWidth; nodeCanvas.Height = screenshot.PixelHeight; nodeCanvas.Children.Add(new Image { Source = screenshot, Width = screenshot.PixelWidth, Height = screenshot.PixelHeight }); }
        else { nodeCanvas.Width = 1; nodeCanvas.Height = 1; }
        TreeViewItem Build(NodeRow row)
        {
            var children = row.Children.Select(Build).ToList();
            var caption = $"{row.Label}   {row.Id}";
            var header = new Border { MinHeight = 44, Background = Brushes.Transparent, Child = new TextBlock { Text = caption, VerticalAlignment = VerticalAlignment.Center } };
            var item = new TreeViewItem { Header = header, Tag = row, IsExpanded = true, ToolTip = row.Bounds, Padding = new Thickness(7, 0, 7, 0) }; System.Windows.Automation.AutomationProperties.SetName(item, caption); nodeTreeItems[row] = item; foreach (var child in children) item.Items.Add(child);
            if (row.Source.Bounds.Width > 0 && row.Source.Bounds.Height > 0)
            {
                var bounds = row.Source.Bounds;
                var rectangle = new Rectangle { Width = bounds.Width, Height = bounds.Height, Stroke = ThemeBrush("SuccessBrush"), StrokeThickness = 1.5, Fill = new SolidColorBrush(Color.FromArgb(1, 35, 122, 87)), Tag = row, ToolTip = $"{row.Label}\n{row.Id}\n{row.Bounds}" };
                Canvas.SetLeft(rectangle, bounds.Left); Canvas.SetTop(rectangle, bounds.Top); Panel.SetZIndex(rectangle, row.Depth);
                rectangle.MouseLeftButtonDown += (_, args) => { ViewModel.SelectedNode = row; args.Handled = true; };
                nodeCanvas.Children.Add(rectangle); nodeRectangles[row] = rectangle;
            }
            return item;
        }
        foreach (var node in ViewModel.Nodes) nodeTree.Items.Add(Build(node));
        HighlightNode();
    }

    private void HighlightNode()
    {
        foreach (var pair in nodeRectangles) { var selected = ReferenceEquals(pair.Key, ViewModel.SelectedNode); pair.Value.Stroke = ThemeBrush(selected ? "DangerBrush" : "SuccessBrush"); pair.Value.StrokeThickness = selected ? 4 : 1.5; var tint = ((SolidColorBrush)pair.Value.Stroke).Color; pair.Value.Fill = new SolidColorBrush(Color.FromArgb(selected ? (byte)35 : (byte)1, tint.R, tint.G, tint.B)); }
        if (ViewModel.SelectedNode is { } node && nodeTreeItems.TryGetValue(node, out var item) && !item.IsSelected) { item.IsSelected = true; item.BringIntoView(); }
    }
    private void SetExpanded(bool expanded) { foreach (var item in nodeTreeItems.Values) item.IsExpanded = expanded; }

}
