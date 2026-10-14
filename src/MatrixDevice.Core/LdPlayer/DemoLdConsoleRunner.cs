using System.Globalization;

namespace MatrixDevice.Core;

public sealed class DemoLdConsoleRunner : ILdConsoleRunner
{
    private readonly object _gate = new();
    private readonly Dictionary<int, LdPlayerInstance> _instances = new()
    {
        [0] = new(0, "雷电模拟器（模拟）", 10001, 10002, true, 20001, 20002, 1600, 900, 240),
        [1] = new(1, "雷电模拟器-1（模拟）", 0, 0, false, -1, -1, 1280, 720, 240)
    };
    public IReadOnlyList<string>? LastArguments { get; private set; }
    public void RemoveInstance(int index) { lock (_gate) _instances.Remove(index); }
    public void AddInstance(LdPlayerInstance instance)
    {
        if (instance.Index < 0 || instance.Index == 99999 || instance.Name.IndexOfAny(['\0', '\r', '\n']) >= 0) throw new ArgumentException("模拟实例索引或名称不合法。");
        lock (_gate) _instances.Add(instance.Index, instance);
    }
    public Task<string> RunAsync(IReadOnlyList<string> arguments, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        if (arguments.Count == 0) throw new DeviceException("模拟 CLI 参数为空。", 400, 1007);
        lock (_gate)
        {
            LastArguments = arguments.ToArray();
            if (arguments[0] == "list2") return Task.FromResult(string.Join('\n', _instances.Values.OrderBy(instance => instance.Index).Select(instance =>
            {
                var fields = new List<object?> { instance.Index, instance.Name, instance.TopWindow, instance.BindWindow, instance.AndroidStarted ? 1 : 0, instance.ProcessId, instance.VBoxProcessId };
                if (instance.Width is not null || instance.Height is not null || instance.Dpi is not null) fields.AddRange([instance.Width, instance.Height, instance.Dpi]);
                return string.Join(',', fields);
            })));
            if (arguments.Count < 3 || arguments[1] != "--index" || !int.TryParse(arguments[2], NumberStyles.None, CultureInfo.InvariantCulture, out var index) || !_instances.TryGetValue(index, out var instance))
                throw new DeviceException("模拟雷电实例不存在或参数不合法。", 404, 1002);
            switch (arguments[0])
            {
                case "launch": case "reboot": _instances[index] = instance with { AndroidStarted = true, TopWindow = 10001 + index * 10, BindWindow = 10002 + index * 10, ProcessId = 20001 + index * 10, VBoxProcessId = 20002 + index * 10 }; break;
                case "quit": _instances[index] = instance with { AndroidStarted = false, TopWindow = 0, BindWindow = 0, ProcessId = -1, VBoxProcessId = -1 }; break;
                case "runapp": case "killapp": case "installapp": case "action": break;
                default: throw new DeviceException("模拟 CLI 未开放命令：" + arguments[0], 501, 2001);
            }
            return Task.FromResult("模拟命令已返回；未连接真实模拟器。");
        }
    }
}
