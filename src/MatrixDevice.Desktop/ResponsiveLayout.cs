using System.Windows;
using System.Windows.Controls;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Media;

namespace MatrixDevice.Desktop;

internal static class ResponsiveLayout
{
    private static readonly ConditionalWeakTable<Window, OwnedWindowFit> ownedWindows = new();
    public static FrameworkElement Field(string label, FrameworkElement input, double labelWidth)
    {
        var row = new Grid { Margin = new Thickness(0, 0, 0, 9) };
        row.ColumnDefinitions.Add(new() { Width = new GridLength(labelWidth) }); row.ColumnDefinitions.Add(new());
        row.RowDefinitions.Add(new() { Height = GridLength.Auto }); row.RowDefinitions.Add(new() { Height = new GridLength(0) });
        var caption = new TextBlock { VerticalAlignment = VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap };
        LocalizationService.Bind(caption, TextBlock.TextProperty, label); row.Children.Add(caption);
        if (input is Control) LocalizationService.Bind(input, System.Windows.Automation.AutomationProperties.NameProperty, label);
        Grid.SetColumn(input, 1); row.Children.Add(input);
        bool? stacked = null;
        void Arrange()
        {
            if (row.ActualWidth <= 0 || stacked == (row.ActualWidth < 460)) return;
            stacked = row.ActualWidth < 460;
            row.ColumnDefinitions[0].Width = new GridLength(stacked == true ? 0 : labelWidth);
            row.RowDefinitions[1].Height = stacked == true ? GridLength.Auto : new GridLength(0);
            Grid.SetColumnSpan(caption, stacked == true ? 2 : 1);
            Grid.SetRow(input, stacked == true ? 1 : 0); Grid.SetColumn(input, stacked == true ? 0 : 1); Grid.SetColumnSpan(input, stacked == true ? 2 : 1);
            caption.Margin = stacked == true ? new Thickness(0, 0, 0, 6) : new Thickness(0);
        }
        row.SizeChanged += (_, _) => Arrange(); row.Loaded += (_, _) => Arrange();
        return row;
    }

    public static WrapPanel Toolbar()
    {
        var panel = new WrapPanel { Orientation = Orientation.Horizontal };
        var originalCaps = new Dictionary<FrameworkElement, double>();
        void Constrain()
        {
            if (panel.ActualWidth <= 0) return;
            foreach (var child in panel.Children.OfType<FrameworkElement>())
            {
                if (!originalCaps.TryGetValue(child, out var cap)) originalCaps[child] = cap = child.MaxWidth;
                child.MaxWidth = Math.Max(child.MinWidth, Math.Min(cap, Math.Max(0, panel.ActualWidth - child.Margin.Left - child.Margin.Right)));
            }
        }
        panel.SizeChanged += (_, _) => Constrain(); panel.Loaded += (_, _) => Constrain();
        return panel;
    }

    public static void FitOwnedWindow(Window window, Window owner, double preferredWidth, double preferredHeight)
    {
        var fit = ownedWindows.GetValue(window, _ => new(window, owner));
        fit.PreferredWidth = preferredWidth; fit.PreferredHeight = preferredHeight; fit.Apply();
    }
    internal static string? FollowFailure(Window window) => ownedWindows.TryGetValue(window, out var fit) ? fit.LastFollowFailure : null;

    internal static Rect OwnerWorkArea(Window owner)
    {
        var ownerWidth = owner.ActualWidth > 0 ? owner.ActualWidth : owner.Width;
        var ownerHeight = owner.ActualHeight > 0 ? owner.ActualHeight : owner.Height;
        var handle = new WindowInteropHelper(owner).Handle;
        var monitor = MonitorFromWindow(handle, 2); var info = new MonitorInfo { Size = Marshal.SizeOf<MonitorInfo>() };
        if (handle != IntPtr.Zero && monitor != IntPtr.Zero && GetMonitorInfo(monitor, ref info) && GetWindowRect(handle, out var bounds))
        {
            var dpi = VisualTreeHelper.GetDpi(owner);
            return new Rect(owner.Left + (info.Work.Left - bounds.Left) / dpi.DpiScaleX, owner.Top + (info.Work.Top - bounds.Top) / dpi.DpiScaleY,
                (info.Work.Right - info.Work.Left) / dpi.DpiScaleX, (info.Work.Bottom - info.Work.Top) / dpi.DpiScaleY);
        }
        return new Rect(owner.Left, owner.Top, ownerWidth, ownerHeight);
    }

    internal readonly record struct OwnedBounds(Rect Bounds, double MinimumWidth, double MinimumHeight, double MaximumWidth, double MaximumHeight);
    internal static OwnedBounds CalculateOwnedBounds(Rect owner, Rect work, double preferredWidth, double preferredHeight, double minimumWidth, double minimumHeight)
    {
        static bool Valid(Rect rectangle) => !rectangle.IsEmpty && double.IsFinite(rectangle.X) && double.IsFinite(rectangle.Y) && double.IsFinite(rectangle.Width) && double.IsFinite(rectangle.Height) && rectangle.Width > 0 && rectangle.Height > 0;
        if (!Valid(owner) || !Valid(work) || !double.IsFinite(preferredWidth) || !double.IsFinite(preferredHeight) || preferredWidth <= 0 || preferredHeight <= 0 || !double.IsFinite(minimumWidth) || !double.IsFinite(minimumHeight) || minimumWidth < 0 || minimumHeight < 0)
            throw new InvalidOperationException($"弹窗边界无效：owner={owner}; work={work}; preferred={preferredWidth}×{preferredHeight}。");
        var available = Rect.Intersect(owner, work);
        if (available.IsEmpty || available.Width <= 24 || available.Height <= 24) throw new InvalidOperationException("主窗口没有足够的可见工作区，无法安全显示弹窗。");
        var maxWidth = available.Width - 24; var maxHeight = available.Height - 24;
        var minWidth = Math.Min(minimumWidth, maxWidth); var minHeight = Math.Min(minimumHeight, maxHeight);
        var width = Math.Clamp(preferredWidth, minWidth, maxWidth); var height = Math.Clamp(preferredHeight, minHeight, maxHeight);
        return new(new Rect(Math.Clamp(owner.X + (owner.Width - width) / 2, available.Left + 12, available.Right - width - 12),
            Math.Clamp(owner.Y + (owner.Height - height) / 2, available.Top + 12, available.Bottom - height - 12), width, height), minWidth, minHeight, maxWidth, maxHeight);
    }

    private sealed class OwnedWindowFit
    {
        private readonly Window window, owner;
        private readonly double minimumWidth, minimumHeight;
        public double PreferredWidth { get; set; }
        public double PreferredHeight { get; set; }
        public string? LastFollowFailure { get; private set; }
        public OwnedWindowFit(Window window, Window owner)
        {
            this.window = window; this.owner = owner; minimumWidth = window.MinWidth; minimumHeight = window.MinHeight;
            owner.SizeChanged += OwnerChanged; owner.LocationChanged += OwnerMoved;
            window.Closed += (_, _) => { owner.SizeChanged -= OwnerChanged; owner.LocationChanged -= OwnerMoved; ownedWindows.Remove(window); };
        }
        private void OwnerChanged(object sender, SizeChangedEventArgs args) => Follow();
        private void OwnerMoved(object? sender, EventArgs args) => Follow();
        private void Follow()
        {
            if (!window.IsVisible) return;
            try { Apply(); LastFollowFailure = null; }
            catch (InvalidOperationException exception)
            {
                // Moving the owner outside a work area must not discard the last valid child bounds.
                if (LastFollowFailure != exception.Message) System.Diagnostics.Trace.TraceWarning("弹窗跟随暂停：{0}；{1}", window.Title, exception.Message);
                LastFollowFailure = exception.Message;
            }
        }
        public void Apply()
        {
            if (!owner.IsLoaded) throw new InvalidOperationException("主窗口尚未完成布局，无法计算弹窗边界。");
            var fit = CalculateOwnedBounds(new Rect(owner.Left, owner.Top, owner.ActualWidth, owner.ActualHeight), OwnerWorkArea(owner), PreferredWidth, PreferredHeight, minimumWidth, minimumHeight);
            window.MinWidth = Math.Min(window.MinWidth, fit.MinimumWidth); window.MinHeight = Math.Min(window.MinHeight, fit.MinimumHeight);
            window.MaxWidth = fit.MaximumWidth; window.MaxHeight = fit.MaximumHeight;
            window.MinWidth = fit.MinimumWidth; window.MinHeight = fit.MinimumHeight; window.SizeToContent = SizeToContent.Manual;
            window.Width = fit.Bounds.Width; window.Height = fit.Bounds.Height;
            window.WindowStartupLocation = WindowStartupLocation.Manual;
            window.Left = fit.Bounds.Left; window.Top = fit.Bounds.Top;
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRect { public int Left, Top, Right, Bottom; }
    [StructLayout(LayoutKind.Sequential)]
    private struct MonitorInfo { public int Size; public NativeRect Monitor, Work; public int Flags; }
    [DllImport("user32.dll")] private static extern IntPtr MonitorFromWindow(IntPtr window, uint flags);
    [DllImport("user32.dll", EntryPoint = "GetMonitorInfoW")]
    [return: MarshalAs(UnmanagedType.Bool)] private static extern bool GetMonitorInfo(IntPtr monitor, ref MonitorInfo info);
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)] private static extern bool GetWindowRect(IntPtr window, out NativeRect bounds);
}
