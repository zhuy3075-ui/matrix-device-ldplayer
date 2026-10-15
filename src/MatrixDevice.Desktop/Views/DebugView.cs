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
        DockPanel.SetDock(toolbar, Dock.Top); page.Children.Add(toolbar);
        var grid = new Grid { Margin = new Thickness(0, 10, 0, 0) }; grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(300) }); grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(270) });
        nodeCanvas = new Canvas { Background = new SolidColorBrush(Color.FromRgb(232, 237, 240)) };
        var viewbox = new Viewbox { Child = nodeCanvas, Stretch = Stretch.Uniform, VerticalAlignment = VerticalAlignment.Center }; grid.Children.Add(viewbox);
        nodeTree = new TreeView { Margin = new Thickness(8, 0, 8, 0), BorderBrush = new SolidColorBrush(Color.FromRgb(212, 221, 225)) }; nodeTree.SelectedItemChanged += (_, _) => { if (nodeTree.SelectedItem is TreeViewItem { Tag: NodeRow row }) ViewModel.SelectedNode = row; }; Grid.SetColumn(nodeTree, 1); grid.Children.Add(nodeTree);
        var properties = new DockPanel(); var label = Label("节点属性"); DockPanel.SetDock(label, Dock.Top); properties.Children.Add(label); var text = BoundInput(nameof(WorkspaceViewModel.NodeProperties), readOnly: true, multiline: true); text.FontFamily = new FontFamily("Consolas"); properties.Children.Add(text); Grid.SetColumn(properties, 2); grid.Children.Add(properties);
        page.Children.Add(grid); return page;
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
            var item = new TreeViewItem { Header = $"{row.Label}   {row.Id}", Tag = row, IsExpanded = true, ToolTip = row.Bounds }; nodeTreeItems[row] = item; foreach (var child in children) item.Items.Add(child);
            if (row.Source.Bounds.Width > 0 && row.Source.Bounds.Height > 0)
            {
                var bounds = row.Source.Bounds;
                var rectangle = new Rectangle { Width = bounds.Width, Height = bounds.Height, Stroke = new SolidColorBrush(Color.FromArgb(170, 10, 129, 142)), StrokeThickness = 1.5, Fill = new SolidColorBrush(Color.FromArgb(1, 10, 129, 142)), Tag = row, ToolTip = $"{row.Label}\n{row.Id}\n{row.Bounds}" };
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
        foreach (var pair in nodeRectangles) { var selected = ReferenceEquals(pair.Key, ViewModel.SelectedNode); pair.Value.Stroke = selected ? new SolidColorBrush(Color.FromRgb(215, 55, 92)) : new SolidColorBrush(Color.FromArgb(170, 10, 129, 142)); pair.Value.StrokeThickness = selected ? 4 : 1.5; pair.Value.Fill = new SolidColorBrush(Color.FromArgb(selected ? (byte)35 : (byte)1, 215, 55, 92)); }
        if (ViewModel.SelectedNode is { } node && nodeTreeItems.TryGetValue(node, out var item) && !item.IsSelected) { item.IsSelected = true; item.BringIntoView(); }
    }
    private void SetExpanded(bool expanded) { foreach (var item in nodeTreeItems.Values) item.IsExpanded = expanded; }

}
