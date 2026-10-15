using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using MatrixDevice.Core;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    private async Task VerifyAppleAppearanceAsync(string? screenshotPath)
    {
        if (!ViewModel.Demo) throw new InvalidOperationException("共享外观验收仅允许演示工作区。");
        var cases = new List<string>();
        var contrast = new List<object>();
        void Check(bool condition, string name)
        {
            if (!condition) throw new InvalidOperationException("共享外观验收失败：" + name);
            cases.Add(name);
        }
        void Contrast(string name, Brush foreground, Brush background, double minimum)
        {
            var ratio = AppearanceColors(foreground).SelectMany(front => AppearanceColors(background).Select(back => AppearanceContrast(front, back))).Min();
            contrast.Add(new { name, ratio, minimum });
            Check(ratio >= minimum, name + "对比度");
        }
        async Task Render() => await Dispatcher.InvokeAsync(UpdateLayout, DispatcherPriority.ApplicationIdle);
        var expected = new Dictionary<string, string>
        {
            ["设备墙 / 主控"] = "设备", ["实时监控"] = "监控", ["节点"] = "界面检查", ["截图 / 取色"] = "截屏",
            ["Shell"] = "终端", ["应用"] = "应用", ["文件"] = "文件", ["文本"] = "文本",
            ["脚本"] = "自动化", ["API"] = "接口", ["扩展"] = "工具", ["设置"] = "设置"
        };
        Check(tabs.Items.Cast<TabItem>().Select(item => item.Tag as string).SequenceEqual(new[] { "设备墙 / 主控", "节点", "截图 / 取色", "Shell", "应用", "文件", "文本", "脚本", "API", "扩展", "设置", "实时监控" }), "保留12个内部Tag及原页序");
        foreach (var (tag, caption) in expected)
        {
            navigation[tag].RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            await Render();
            var tab = tabs.Items.Cast<TabItem>().Single(item => Equals(item.Tag, tag));
            Check(navigation[tag].ToolTip as string == caption && tab.Header as string == caption && Title == "矩阵设备 · " + caption + " · 演示模式", "名称、标题及页签：" + caption);
            Check(AutomationProperties.GetName(navigation[tag]) == caption && navigation[tag].IsChecked == true, "导航选中及可访问名称：" + caption);
            if (tag is "设备墙 / 主控" or "实时监控" or "应用")
                Check(VisualDescendants<TextBlock>(Shell).Single(text => text.Name == "PageHeading").Text == caption, "正文标题：" + caption);
            foreach (var button in VisualDescendants<Button>(Shell).Where(button => button.IsVisible && button.ToolTip is string))
                Check(!string.IsNullOrWhiteSpace(AutomationProperties.GetName(button)) && AutomationProperties.GetName(button).Any(character => character is >= '\u4e00' and <= '\u9fff'), "按钮中文名称：" + button.ToolTip);
            Contrast("选中导航“" + caption + "”", navigation[tag].Foreground, navigation[tag].Background, 4.5);
        }
        foreach (var (key, value) in new Dictionary<string, string>
        {
            ["CanvasBrush"] = "#FFF5F5F7", ["TextBrush"] = "#FF1D1D1F", ["MutedBrush"] = "#FF626269", ["AccentBrush"] = "#FF3769C8",
            ["AccentHoverBrush"] = "#FF2D5CB4", ["AccentPressedBrush"] = "#FF244D99", ["SuccessBrush"] = "#FF237A57", ["WarningBrush"] = "#FF8A5B12", ["DangerBrush"] = "#FFB33B45"
        }) Check(ThemeBrush(key) is SolidColorBrush brush && brush.Color.ToString() == value, "颜色token：" + key);
        Contrast("正文", ThemeBrush("TextBrush"), ThemeBrush("SurfaceBrush"), 4.5);
        Contrast("次级正文", ThemeBrush("MutedBrush"), ThemeBrush("CanvasBrush"), 4.5);
        foreach (var key in new[] { "AccentBrush", "AccentHoverBrush", "AccentPressedBrush", "SuccessBrush", "WarningBrush", "DangerBrush" }) Contrast(key, ThemeBrush(key), ThemeBrush("SurfaceBrush"), 4.5);
        Contrast("输入边界", ThemeBrush("InputBorderBrush"), ThemeBrush("SurfaceBrush"), 3);
        Check(FontFamily.Source.Contains("Segoe UI", StringComparison.Ordinal) && FontFamily.Source.Contains("Microsoft YaHei UI", StringComparison.Ordinal), "系统无衬线字体回退");
        NavigateTo("设置"); await Render();
        var path = VisualDescendants<TextBox>(Shell).Single(input => BindingOperations.GetBinding(input, TextBox.TextProperty)?.Path.Path == "Settings.AdbPath");
        Check(path.Focusable && AutomationProperties.GetName(path) == "ADB 路径", "路径字段可访问名称位于真实输入框");
        foreach (var name in new[] { "WorkspaceSettings", "DeviceDetails" })
        {
            var tool = VisualDescendants<Button>(Shell).Single(button => button.Name == name);
            Check(AutomationProperties.GetAutomationId(tool) == name && !string.IsNullOrWhiteSpace(AutomationProperties.GetName(tool)), "全局工具稳定ID：" + name);
        }
        var collapse = VisualDescendants<Button>(Shell).Single(button => button.Name == "NavigationToggle");
        SetSidebarCollapsed(true); Check(AutomationProperties.GetName(collapse) == "展开导航", "折叠后导航动作名称更新");
        SetSidebarCollapsed(false); Check(AutomationProperties.GetName(collapse) == "折叠导航", "展开后导航动作名称更新");
        NavigateTo("实时监控"); await Render();
        monitorPauseChoice.IsChecked = true; Check(AutomationProperties.GetName(monitorPauseChoice) == "恢复监控", "暂停后动作名称为恢复");
        monitorPauseChoice.IsChecked = false; Check(AutomationProperties.GetName(monitorPauseChoice) == "暂停监控", "恢复后动作名称为暂停");
        NavigateTo("API"); await Render();
        Check(VisualDescendants<Button>(Shell).Any(button => AutomationProperties.GetName(button) == "停止设备接口") && !VisualDescendants<Button>(Shell).Any(button => button.ToolTip as string == "停止所选设备"), "接口停止动作不误称关闭设备");

        var probe = Button("\uE74E", "保存", () => Task.CompletedTask);
        var primary = Button("\uE768", "运行", new ActionCommand(() => { })); primary.SetResourceReference(StyleProperty, "PrimaryButton");
        var toggle = WallToggle("\uE8EA", "选择视图");
        var input = BoundInput(nameof(WorkspaceViewModel.ShellInput));
        var check = MainWindow.Check("同步点击滑动", nameof(WorkspaceViewModel.SyncMouse));
        var combo = new ComboBox { ItemsSource = new[] { "标准" }, SelectedIndex = 0 };
        AutomationProperties.SetName(combo, "界面选项");
        var sample = new StackPanel(); sample.Children.Add(probe); sample.Children.Add(primary); sample.Children.Add(toggle); sample.Children.Add(Field("命令", input)); sample.Children.Add(check); sample.Children.Add(combo);
        var overlay = new Border { Child = sample, Background = ThemeBrush("SurfaceBrush"), BorderBrush = ThemeBrush("BorderBrush"), BorderThickness = new Thickness(1), Padding = new Thickness(20), Width = 360, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        Grid.SetColumnSpan(overlay, 2); Panel.SetZIndex(overlay, 1000); Shell.Children.Add(overlay);
        var animationMilliseconds = new List<double>();
        var syncMouse = ViewModel.SyncMouse;
        if (!GetAppearanceCursorPosition(out var pointer)) throw new InvalidOperationException("无法保存验收指针位置。");
        async Task Hover(FrameworkElement control)
        {
            var point = control.PointToScreen(new Point(control.ActualWidth / 2, control.ActualHeight / 2));
            if (!SetAppearanceCursorPosition((int)Math.Round(point.X), (int)Math.Round(point.Y))) throw new InvalidOperationException("无法移动验收指针。");
            Mouse.Synchronize(); await Render(); await Task.Delay(30);
            Check(control.IsMouseOver, "真实指针悬停：" + control.GetType().Name);
        }
        try
        {
            await Render();
            Check(probe.IsEnabled && probe.Opacity == 1 && AutomationProperties.GetName(probe) == "保存", "普通按钮正常状态");
            var popup = (Popup)combo.Template.FindName("PART_Popup", combo);
            Contrast("实际普通按钮", probe.Foreground, probe.Background, 4.5);
            Contrast("实际主要按钮", primary.Foreground, primary.Background, 4.5);
            var originalSize = probe.RenderSize;
            toggle.IsChecked = true; await Render();
            Check(toggle.IsChecked == true && toggle.Background is SolidColorBrush { Color.A: 255 }, "真实分段控件选中表面可见");
            Check(navigation["API"].Background is LinearGradientBrush, "真实选中导航使用轻微渐变"); Contrast("实际选中控件", toggle.Foreground, toggle.Background, 4.5);
            MotionPreferences.Current.SetFixturePreference(true);
            Activate(); probe.Focus(); Keyboard.Focus(probe); await Render();
            Check(probe.IsKeyboardFocused && ReferenceEquals(probe.BorderBrush, ThemeBrush("AccentBrush")), "真实键盘焦点边界可见");
            var feedback = (Border)probe.Template.FindName("FocusFeedback", probe);
            await Task.Delay(170);
            Check(feedback.HasAnimatedProperties && feedback.Opacity > 0 && feedback.Opacity <= .061, $"模板实例的局部淡入反馈运行（animated={feedback.HasAnimatedProperties}; opacity={feedback.Opacity}; enabled={MotionPreferences.Current.AnimationsEnabled}; focus={probe.IsKeyboardFocusWithin}）");
            foreach (var trigger in probe.Template.Triggers.OfType<MultiDataTrigger>())
                foreach (var begin in trigger.EnterActions.OfType<BeginStoryboard>())
                    animationMilliseconds.AddRange(begin.Storyboard.Children.OfType<DoubleAnimation>().Select(animation => animation.Duration.TimeSpan.TotalMilliseconds));
            Check(animationMilliseconds.Count != 0 && animationMilliseconds.All(duration => duration is >= 100 and <= 160), "动画时长在100至160毫秒");
            Contrast("焦点可见边界", probe.BorderBrush, ThemeBrush("SurfaceBrush"), 3);
            Check(probe.RenderSize == originalSize, "焦点与动效不改变控件尺寸");
            Contrast("实际输入默认边界", input.BorderBrush, input.Background, 3);
            var checkBorder = (Border)check.Template.FindName("CheckBorder", check);
            check.IsChecked = false; await Render(); Contrast("实际复选未选边界", checkBorder.BorderBrush, checkBorder.Background, 3);
            await Hover(input); Contrast("实际输入悬停边界", input.BorderBrush, input.Background, 3);
            await Hover(combo); Contrast("实际选项悬停边界", combo.BorderBrush, combo.Background, 3);
            await Hover(check); Contrast("实际复选悬停边界", checkBorder.BorderBrush, checkBorder.Background, 3);
            check.IsChecked = true; await Render();
            Contrast("实际复选勾选", ((TextBlock)check.Template.FindName("CheckMark", check)).Foreground, checkBorder.Background, 4.5);
            probe.Focus(); Keyboard.Focus(probe); await Render();
            var source = PresentationSource.FromVisual(this) ?? throw new InvalidOperationException("验收窗口未连接显示源。");
            probe.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, source, Environment.TickCount, Key.Space) { RoutedEvent = Keyboard.KeyDownEvent });
            Check(probe.IsPressed, "实际Space路由按下状态");
            Contrast("实际按下按钮", probe.Foreground, probe.Background, 4.5);
            probe.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, source, Environment.TickCount, Key.Space) { RoutedEvent = Keyboard.KeyUpEvent });
            Check(!probe.IsPressed, "实际Space路由释放状态");
            MotionPreferences.Current.SetFixturePreference(false); await Render();
            Check(!feedback.HasAnimatedProperties && feedback.Opacity == 0 && popup.PopupAnimation == PopupAnimation.None, "减少动画立即移除局部动画和实际Popup淡入");
            MotionPreferences.Current.SetFixturePreference(true); await Render(); await Task.Delay(170);
            Check(feedback.HasAnimatedProperties && popup.PopupAnimation == PopupAnimation.Fade, "偏好变化后当前控件和实际Popup动态恢复动画");
            primary.Focus(); Keyboard.Focus(primary); await Render(); await Task.Delay(170);
            var primaryFeedback = (Border)primary.Template.FindName("FocusFeedback", primary);
            Check(primaryFeedback.HasAnimatedProperties, "主要按钮实例反馈运行");
            var primaryContrast = AppearanceColors(primary.Foreground).SelectMany(front => AppearanceColors(primary.Background).Select(back => AppearanceContrast(AppearanceOverlay(front, primaryFeedback.Opacity), AppearanceOverlay(back, primaryFeedback.Opacity)))).Min();
            contrast.Add(new { name = "主要按钮白色反馈层合成", ratio = primaryContrast, minimum = 4.5, overlayOpacity = primaryFeedback.Opacity });
            Check(primaryContrast >= 4.5, "反馈层合成后的主要按钮文字对比度");
            toggle.Focus(); Keyboard.Focus(toggle); await Render(); await Task.Delay(170);
            var toggleFeedback = (Border)toggle.Template.FindName("FocusFeedback", toggle);
            Check(toggle.IsKeyboardFocused && toggleFeedback.HasAnimatedProperties && toggleFeedback.Opacity > 0, "实际Toggle模板局部反馈运行");
            MotionPreferences.Current.SetFixturePreference(false); await Render();
            Check(!toggleFeedback.HasAnimatedProperties && toggleFeedback.Opacity == 0 && popup.PopupAnimation == PopupAnimation.None, "减少动画同时移除Toggle与实际Popup反馈");
            MotionPreferences.Current.SetFixturePreference(true); await Render();
            ViewModel.SetExternalBusy(true); await Render();
            Check(!probe.IsEnabled && probe.Opacity < .51, "真实繁忙门禁呈现禁用状态");
            ViewModel.SetExternalBusy(false); await Render();
            primary.IsEnabled = false; Check(primary.Opacity < .51, "主要按钮独立禁用状态");
            primary.IsEnabled = true;
            if (screenshotPath is not null)
            {
                if (!Path.IsPathFullyQualified(screenshotPath)) throw new InvalidOperationException("外观截图必须为绝对路径。");
                Directory.CreateDirectory(Path.GetDirectoryName(screenshotPath)!);
                SaveRender(Path.Combine(Path.GetDirectoryName(screenshotPath)!, "appearance-states.png"));
            }
        }
        finally
        {
            ViewModel.SetExternalBusy(false); ViewModel.SyncMouse = syncMouse; MotionPreferences.Current.SetFixturePreference(null); Shell.Children.Remove(overlay);
            SetAppearanceCursorPosition(pointer.X, pointer.Y);
            NavigateTo("设备墙 / 主控");
        }
        Check(MotionPreferences.Current.AnimationsEnabled == SystemParameters.ClientAreaAnimation, "恢复并遵循真实系统动画偏好");
        File.WriteAllText(Path.Combine(ViewModel.DataDirectory, "appearance.json"), JsonSerializer.Serialize(new
        {
            mode = "demo", status = "PASS-OFFLINE", cases, contrast, animationMilliseconds,
            motionPreferenceScope = "仅模拟应用内偏好分支并恢复系统读取；未修改系统设置，未宣称实测操作系统偏好切换。",
            requestedSystemAnimation = SystemParameters.ClientAreaAnimation
        }, JsonDefaults.Options));
    }

    private static IEnumerable<Color> AppearanceColors(Brush brush) => brush switch
    {
        SolidColorBrush solid => [solid.Color], GradientBrush gradient => gradient.GradientStops.Select(stop => stop.Color),
        _ => throw new InvalidOperationException("无法验证颜色对比的画刷：" + brush.GetType().Name)
    };
    private static double AppearanceContrast(Color foreground, Color background)
    {
        static double Luminance(Color color)
        {
            static double Linear(byte channel) { var value = channel / 255.0; return value <= .04045 ? value / 12.92 : Math.Pow((value + .055) / 1.055, 2.4); }
            return .2126 * Linear(color.R) + .7152 * Linear(color.G) + .0722 * Linear(color.B);
        }
        var first = Luminance(foreground); var second = Luminance(background);
        return (Math.Max(first, second) + .05) / (Math.Min(first, second) + .05);
    }
    private static Color AppearanceOverlay(Color color, double whiteOpacity) => Color.FromRgb(
        (byte)Math.Round(color.R * (1 - whiteOpacity) + 255 * whiteOpacity),
        (byte)Math.Round(color.G * (1 - whiteOpacity) + 255 * whiteOpacity),
        (byte)Math.Round(color.B * (1 - whiteOpacity) + 255 * whiteOpacity));
    [StructLayout(LayoutKind.Sequential)]
    private struct AppearancePointer { public int X; public int Y; }
    [DllImport("user32.dll", EntryPoint = "GetCursorPos")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetAppearanceCursorPosition(out AppearancePointer pointer);
    [DllImport("user32.dll", EntryPoint = "SetCursorPos")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetAppearanceCursorPosition(int x, int y);
}
