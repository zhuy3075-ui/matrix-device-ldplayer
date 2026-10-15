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
    private FrameworkElement BuildScreenshotPage()
    {
        var page = Page();
        var toolbar = Toolbar(); toolbar.Children.Add(Button("\uE722", "刷新截图", ViewModel.ScreenshotCommand)); toolbar.Children.Add(Button("\uE74E", "保存 PNG", async () => await SaveImageAsync(false))); toolbar.Children.Add(Button("\uE7A8", "保存裁剪", async () => await SaveImageAsync(true)));
        screenshotMode = new ComboBox { ItemsSource = new[] { "取色", "裁剪" }, ItemTemplate = CaptionChoices(), SelectedIndex = 0, Width = 115 }; toolbar.Children.Add(screenshotMode);
        var zoom = new Slider { Minimum = .1, Maximum = 3, Value = 1, Width = 150, VerticalAlignment = VerticalAlignment.Center, ToolTip = "缩放" }; zoom.ValueChanged += (_, _) => { screenshotScale.ScaleX = zoom.Value; screenshotScale.ScaleY = zoom.Value; }; toolbar.Children.Add(zoom); toolbar.Children.Add(Button("\uE740", "原始大小", () => zoom.Value = 1));
        DockPanel.SetDock(toolbar, Dock.Top); page.Children.Add(toolbar);
        var colors = new DataGrid { ItemsSource = ViewModel.Colors, IsReadOnly = true, Width = 300, Margin = new Thickness(10, 10, 0, 0) }; TextColumn(colors, "坐标", nameof(PixelSample.Position), 70); TextColumn(colors, "HEX", nameof(PixelSample.Hex), 80); TextColumn(colors, "RGB", nameof(PixelSample.Rgb), 140); colors.MouseDoubleClick += (_, _) => { if (colors.SelectedItem is PixelSample color) Clipboard.SetText($"{color.Hex} {color.Rgb}"); }; DockPanel.SetDock(colors, Dock.Right); page.Children.Add(colors);
        screenshotCanvas = new Canvas { Background = ThemeBrush("CanvasBrush"), LayoutTransform = screenshotScale };
        screenshotImage = new Image { Stretch = Stretch.None }; screenshotCanvas.Children.Add(screenshotImage);
        var cropColor = ((SolidColorBrush)ThemeBrush("DangerBrush")).Color;
        cropRectangle = new Rectangle { Stroke = ThemeBrush("DangerBrush"), StrokeThickness = 2, Fill = new SolidColorBrush(Color.FromArgb(35, cropColor.R, cropColor.G, cropColor.B)), Visibility = Visibility.Collapsed, IsHitTestVisible = false }; screenshotCanvas.Children.Add(cropRectangle);
        screenshotCanvas.MouseLeftButtonDown += ScreenshotDown; screenshotCanvas.MouseMove += ScreenshotMove; screenshotCanvas.MouseLeftButtonUp += ScreenshotUp;
        screenshotScroll = new ScrollViewer { Content = screenshotCanvas, HorizontalScrollBarVisibility = ScrollBarVisibility.Auto, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Margin = new Thickness(0, 10, 0, 0) }; page.Children.Add(screenshotScroll);
        return page;
    }

    private async Task SaveImageAsync(bool useCrop)
    {
        if (useCrop && crop is null) { MessageBox.Show(this, "尚未选择裁剪区域", "保存裁剪", MessageBoxButton.OK, MessageBoxImage.Information); return; }
        var dialog = new SaveFileDialog { Filter = "PNG|*.png", FileName = useCrop ? "crop.png" : "screenshot.png" };
        if (dialog.ShowDialog(this) == true) await ViewModel.SaveScreenshotAsync(dialog.FileName, useCrop ? crop : null);
    }

    private Point ClampScreenshot(Point point) => ViewModel.DebugImage is { } image ? new Point(Math.Clamp(point.X, 0, image.PixelWidth - 1), Math.Clamp(point.Y, 0, image.PixelHeight - 1)) : default;
    private void ScreenshotDown(object sender, MouseButtonEventArgs e)
    {
        if (ViewModel.DebugImage is null) return;
        var point = ClampScreenshot(e.GetPosition(screenshotCanvas));
        if (screenshotMode.SelectedIndex == 0) ViewModel.SamplePixel((int)point.X, (int)point.Y);
        else { cropStart = point; crop = null; cropRectangle.Visibility = Visibility.Visible; cropRectangle.Width = cropRectangle.Height = 0; screenshotCanvas.CaptureMouse(); }
    }
    private void ScreenshotMove(object sender, MouseEventArgs e)
    {
        if (cropStart is not { } start) return;
        var point = ClampScreenshot(e.GetPosition(screenshotCanvas));
        Canvas.SetLeft(cropRectangle, Math.Min(start.X, point.X)); Canvas.SetTop(cropRectangle, Math.Min(start.Y, point.Y)); cropRectangle.Width = Math.Abs(start.X - point.X); cropRectangle.Height = Math.Abs(start.Y - point.Y);
    }
    private void ScreenshotUp(object sender, MouseButtonEventArgs e)
    {
        if (cropStart is not { } start || ViewModel.DebugImage is not { } image) return;
        var end = ClampScreenshot(e.GetPosition(screenshotCanvas)); var x = (int)Math.Min(start.X, end.X); var y = (int)Math.Min(start.Y, end.Y); var width = (int)Math.Abs(start.X - end.X); var height = (int)Math.Abs(start.Y - end.Y);
        crop = width > 0 && height > 0 ? new Int32Rect(x, y, Math.Min(width, image.PixelWidth - x), Math.Min(height, image.PixelHeight - y)) : null;
        cropStart = null; screenshotCanvas.ReleaseMouseCapture();
    }

    private static Point? MapImage(Image image, Point point)
    {
        if (image.Source is not BitmapSource source || source.PixelWidth <= 0 || source.PixelHeight <= 0 || image.ActualWidth <= 0 || image.ActualHeight <= 0) return null;
        var scale = Math.Min(image.ActualWidth / source.PixelWidth, image.ActualHeight / source.PixelHeight); var left = (image.ActualWidth - source.PixelWidth * scale) / 2; var top = (image.ActualHeight - source.PixelHeight * scale) / 2;
        var x = (point.X - left) / scale; var y = (point.Y - top) / scale;
        return x < 0 || y < 0 || x >= source.PixelWidth || y >= source.PixelHeight ? null : new Point(x, y);
    }
}
