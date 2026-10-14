using MatrixDevice.Core;

namespace MatrixDevice.Features;

public sealed class ClipboardService(IDeviceBackend backend)
{
    public Task SendTextAsync(string serial, string text, CancellationToken ct = default) => backend.TextAsync(serial, text, ct);
    public Task PushAsync(string serial, string text, CancellationToken ct = default) =>
        throw new DeviceException("Private JiXingKeyboard clipboard protocol was not recovered; sending input text is available separately.", 501, 2001);
    public Task<string> PullAsync(string serial, CancellationToken ct = default) =>
        throw new DeviceException("Reading the device clipboard requires an undocumented device-side service.", 501, 2001);
    public Task InstallKeyboardAsync(string serial, string apkPath, CancellationToken ct = default)
    {
        if (!File.Exists(apkPath)) throw new FileNotFoundException("A caller-provided compatible keyboard APK is required.", apkPath);
        return backend.InstallAsync(serial, apkPath, ct);
    }
}
