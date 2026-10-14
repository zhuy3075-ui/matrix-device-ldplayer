using System.IO;
using MatrixDevice.Core;

namespace MatrixDevice.Desktop;

public static class UiErrorPresentation
{
    public static string Describe(Exception error)
    {
        if (error is TimeoutException || error is TaskCanceledException { InnerException: TimeoutException }) return "操作超时，请核验设备状态后重试";
        if (error is OperationCanceledException) return "操作已停止";
        if (error is FileNotFoundException missing) return "找不到所需文件：" + (missing.FileName ?? "请检查配置路径");
        if (error.Message.TrimStart().FirstOrDefault() is >= '\u4E00' and <= '\u9FFF') return error.Message;
        if (error is DirectoryNotFoundException) return "指定目录不存在，请检查路径";
        if (error is UnauthorizedAccessException) return "没有访问目标文件或设备的权限";
        if (error is System.Text.Json.JsonException) return "数据格式无效，请检查输入的 JSON 内容";
        if (error is ArgumentException) return "参数无效，请检查输入值";
        if (error is DeviceException device)
        {
            if (device.Message.StartsWith("UI dump did not create", StringComparison.Ordinal)) return "节点读取失败：页面可能仍在变化，请暂停动态内容或切换到静止页面后重试";
            if (device.Message.StartsWith("ADB server owner changed", StringComparison.Ordinal)) return "ADB 服务已变化，请重启本程序并重新发现设备；本程序未重启 ADB";
            if (device.Message.StartsWith("Global Unicode input requires", StringComparison.Ordinal)) return "中文输入需要明确绑定雷电实例与授权设备串号；节点指定文本仍需要兼容手机服务";
            if (device.Message.StartsWith("LDPlayer serial/index binding mismatch", StringComparison.Ordinal)) return "雷电索引与设备串号的进程归属不匹配，已拒绝发送文字，请重新核验绑定";
            var message = device.Code switch
            {
                1002 => "找不到目标设备",
                1007 => "请求参数无效",
                2001 => "当前运行环境不支持此能力",
                1003 => "设备当前状态不允许执行此操作",
                _ => "设备命令未能完成"
            };
            return $"{message}（错误码 {device.Code}）";
        }
        if (error is IOException) return "文件读取、写入或本机端口操作失败，请查看诊断日志";
        return "操作未能完成，详细原因已记录到本机诊断日志";
    }
}
