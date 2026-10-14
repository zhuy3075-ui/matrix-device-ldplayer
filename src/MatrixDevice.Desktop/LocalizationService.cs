using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MatrixDevice.Desktop;

public sealed class LocalizationService : INotifyPropertyChanged
{
    public static LocalizationService Current { get; } = new();
    public string Language => "zh-CN";
    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler? LanguageChanged;
    public string this[string key] => Get(key);

    private static readonly IReadOnlyDictionary<string, string> Chinese = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["Shell"] = "命令终端", ["API"] = "接口服务", ["Home"] = "主页", ["serial"] = "设备序列号",
        ["Ready"] = "就绪", ["Offline"] = "离线", ["Unauthorized"] = "等待授权", ["Discovered"] = "已发现", ["Error"] = "异常",
        ["Idle"] = "未运行", ["Running"] = "运行中", ["Cancelling"] = "正在停止", ["Completed"] = "已完成", ["Cancelled"] = "已停止",
        ["Failed"] = "失败", ["TimedOut"] = "超时", ["Lost"] = "结果待核对", ["HEX"] = "十六进制色值", ["RGB"] = "三色值",
        ["普通 API"] = "普通接口", ["Automator API"] = "自动化接口", ["当前设备 API"] = "当前设备接口", ["普通 API 地址"] = "普通接口地址",
        ["普通 API 端口"] = "普通接口端口", ["Automator 端口"] = "自动化接口端口", ["授权 Agent 地址"] = "自动化服务地址",
        ["Agent 设备 serial"] = "自动化服务设备序列号", ["语言 / 区域"] = "界面语言", ["%"] = "进度（%）",
        ["English"] = "简体中文", ["Demo Devices"] = "演示设备"
    };

    public string Get(string key) => Chinese.TryGetValue(key, out var translated) ? translated : key;
    public void SetLanguage(string value)
    {
        var culture = CultureInfo.GetCultureInfo("zh-CN");
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = culture;
        CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = culture;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Language)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
        LanguageChanged?.Invoke(this, EventArgs.Empty);
    }

    public static void Bind(DependencyObject target, DependencyProperty property, string key) =>
        BindingOperations.SetBinding(target, property, new Binding($"[{key}]") { Source = Current, Mode = BindingMode.OneWay });
}

public sealed class CaptionChoiceConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        var key = values.Length == 0 ? "" : values[0]?.ToString() ?? "";
        return LocalizationService.Current.Get(key);
    }
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotSupportedException();
}

public sealed class SubsystemCaptionConverter(string label) : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) =>
        LocalizationService.Current.Get(label) + "：" + LocalizationService.Current.Get(values.Length > 0 && values[0] is string value ? value : "未选择");
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotSupportedException();
}

public sealed class StateCaptionConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) => LocalizationService.Current.Get(values.Length > 0 ? values[0]?.ToString() ?? "" : "");
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotSupportedException();
}
