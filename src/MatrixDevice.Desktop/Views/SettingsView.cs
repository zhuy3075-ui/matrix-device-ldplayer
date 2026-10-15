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
    private FrameworkElement BuildSettingsPage()
    {
        var page = Page();
        var toolbar = Toolbar();
        toolbar.Children.Add(Button("\uE74E", "保存设置", ViewModel.SaveSettingsCommand));
        var mode = BoundText(nameof(WorkspaceViewModel.ModeLabel)); mode.Foreground = Brushes.DimGray; toolbar.Children.Add(mode);
        DockPanel.SetDock(toolbar, Dock.Top);
        page.Children.Add(toolbar);
        var notice = BoundText(nameof(WorkspaceViewModel.SettingsNotice), true);
        notice.Margin = new Thickness(0, 8, 0, 0);
        DockPanel.SetDock(notice, Dock.Top);
        page.Children.Add(notice);
        var grid = new Grid { Margin = new Thickness(0, 10, 0, 0), MaxWidth = 980 };
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        var left = new StackPanel();
        left.Children.Add(Label("连接与接口"));
        left.Children.Add(PathField("ADB 路径", "Settings.AdbPath"));
        left.Children.Add(Field("授权设备串号 JSON", BoundInput("Settings.AdbSerialsJson", multiline: true)));
        left.Children.Add(PathField("scrcpy 路径", "Settings.ScrcpyPath"));
        left.Children.Add(PathField("监控视频组件", "Settings.MonitorServerPath"));
        left.Children.Add(PathField("监控 FFmpeg", "Settings.MonitorFfmpegPath"));
        left.Children.Add(PathField("雷电命令行路径", "Settings.LdConsolePath"));
        var bindings = BoundInput("Settings.LdBindingsJson", multiline: true); bindings.Height = 85; bindings.ToolTip = "例如：{\"0\":\"实际发现的设备序列号\"}；仅保存显式绑定配置，不表示已核验";
        left.Children.Add(Field("实例绑定（JSON）", bindings));
        left.Children.Add(Field("授权 Agent 地址", BoundInput("Settings.AgentBaseUrl")));
        left.Children.Add(Field("Agent 设备 serial", BoundInput("Settings.AgentSerial")));
        left.Children.Add(Field("普通 API 端口", BoundInput("Settings.BasicApiPort")));
        left.Children.Add(Field("Automator 端口", BoundInput("Settings.AutomatorApiPort")));
        left.Children.Add(Check("启动时连接设备", "Settings.AutoConnect"));
        left.Children.Add(Check("启动时打开 API", "Settings.AutoStartApi"));
        var language = new TextBlock { Text = "简体中文", VerticalAlignment = VerticalAlignment.Center };
        left.Children.Add(Field("语言 / 区域", language));
        left.Children.Add(Field("文件交换目录", BoundInput("Settings.FileRoot")));
        left.Children.Add(Field("命令超时（秒）", BoundInput("Settings.CommandTimeoutSeconds")));
        left.Children.Add(Button("\uE777", "恢复遗留投屏窗口", async () => await ViewModel.ExecuteAsync("恢复遗留窗口", async () =>
        {
            var directory = System.IO.Path.Combine(ViewModel.DataDirectory, "window-recovery");
            var failures = NativeWindowHost.RecoverPending(directory);
            if (failures.Count > 0) throw new InvalidOperationException(string.Join("\n", failures));
            await Task.CompletedTask;
        })));
        grid.Children.Add(left);
        var right = new StackPanel { Margin = new Thickness(25, 0, 0, 0) };
        right.Children.Add(Label("投屏与窗体"));
        right.Children.Add(Field("主屏最大尺寸（px）", BoundInput("Settings.MainMaxSize")));
        right.Children.Add(UnavailableSetting("小屏最大尺寸（px）", BoundInput("Settings.MiniMaxSize"), "当前未接入小屏视频编码参数；值仅保留用于文档对照"));
        right.Children.Add(Field("主屏帧率（FPS）", BoundInput("Settings.MainMaxFps")));
        right.Children.Add(UnavailableSetting("小屏帧率（FPS）", BoundInput("Settings.MiniMaxFps"), "当前未接入小屏视频编码参数；值仅保留用于文档对照"));
        right.Children.Add(Field("主屏码率（bit/s）", BoundInput("Settings.MainBitRate")));
        right.Children.Add(UnavailableSetting("小屏码率（bit/s）", BoundInput("Settings.MiniBitRate"), "当前未接入小屏视频编码参数；值仅保留用于文档对照"));
        var hardware = Check("硬件解码偏好", "Settings.ForceHardwareDecode"); hardware.IsEnabled = false; ToolTipService.SetShowOnDisabled(hardware, true); LocalizationService.Bind(hardware, ToolTipProperty, "当前未接入硬件解码选择；值仅保留用于文档对照"); right.Children.Add(hardware);
        right.Children.Add(Check("显示 FPS", "Settings.ShowFps"));
        right.Children.Add(Field("窗体宽度", BoundInput("Settings.WindowWidth")));
        right.Children.Add(Field("窗体高度", BoundInput("Settings.WindowHeight")));
        Grid.SetColumn(right, 1);
        grid.Children.Add(right);
        page.Children.Add(new ScrollViewer { Content = grid, VerticalScrollBarVisibility = ScrollBarVisibility.Auto });
        return page;
    }

    private FrameworkElement PathField(string label, string binding)
    {
        var row = new DockPanel(); var browse = Button("\uE8B7", "浏览", () =>
        {
            var dialog = new OpenFileDialog { Filter = binding.EndsWith("MonitorServerPath", StringComparison.Ordinal) ? "官方视频组件|scrcpy-server*|所有文件|*.*" : "程序|*.exe|所有文件|*.*" };
            if (dialog.ShowDialog(this) != true) return;
            if (binding.EndsWith("AdbPath", StringComparison.Ordinal)) { ViewModel.Settings.AdbPath = dialog.FileName; ViewModel.Settings.Changed(nameof(SettingsEditor.AdbPath)); }
            else if (binding.EndsWith("LdConsolePath", StringComparison.Ordinal)) { ViewModel.Settings.LdConsolePath = dialog.FileName; ViewModel.Settings.Changed(nameof(SettingsEditor.LdConsolePath)); }
            else if (binding.EndsWith("MonitorServerPath", StringComparison.Ordinal)) { ViewModel.Settings.MonitorServerPath = dialog.FileName; ViewModel.Settings.Changed(nameof(SettingsEditor.MonitorServerPath)); }
            else if (binding.EndsWith("MonitorFfmpegPath", StringComparison.Ordinal)) { ViewModel.Settings.MonitorFfmpegPath = dialog.FileName; ViewModel.Settings.Changed(nameof(SettingsEditor.MonitorFfmpegPath)); }
            else { ViewModel.Settings.ScrcpyPath = dialog.FileName; ViewModel.Settings.Changed(nameof(SettingsEditor.ScrcpyPath)); }
        }); DockPanel.SetDock(browse, Dock.Right); row.Children.Add(browse); row.Children.Add(BoundInput(binding)); return Field(label, row);
    }

    private static FrameworkElement UnavailableSetting(string label, FrameworkElement control, string reason)
    {
        var row = Field(label, control); row.IsEnabled = false;
        ToolTipService.SetShowOnDisabled(row, true); LocalizationService.Bind(row, ToolTipProperty, reason);
        return row;
    }

}
