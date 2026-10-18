using System.Diagnostics;
using System.Net;
using MatrixDevice.Core;

namespace MatrixDevice.Host;

public sealed record LdConnectionProof(int Index, string Name, long Generation, string Serial, uint TransportId, int VBoxPid, long VBoxBirth);

public static class LdConnectionAdmission
{
    public static async Task<IReadOnlyList<LdConnectionProof>> ResolveAsync(AdbDeviceBackend backend, LdPlayerService service, int[] indices, IReadOnlyList<LdPlayerInstance> expectedInstances, IReadOnlyDictionary<int, long> expectedBirths, CancellationToken ct = default)
    {
        if (indices.Length == 0 || indices.Any(index => index < 0 || index == 99999) || indices.Distinct().Count() != indices.Length)
            throw new DeviceException("请选择不重复的有效雷电实例，编号必须非负且不能为桌面编号99999。", 400, 1007);
        var instances = await service.RefreshAsync(ct).ConfigureAwait(false);
        RequireExpectedInstances(instances, indices, expectedInstances);
        var registered = await backend.ListRegisteredDevicesAsync(ct).ConfigureAwait(false);
        var proofs = new List<LdConnectionProof>();
        foreach (var index in indices)
        {
            var instance = instances.SingleOrDefault(item => item.Index == index) ?? throw new DeviceException($"实例已不存在：{index}", 404, 1002);
            if (!instance.AndroidStarted || instance.ProcessId <= 0 || instance.VBoxProcessId <= 0) throw new DeviceException($"实例{index}尚未完成启动。", 409, 1003);
            using var process = Process.GetProcessById(instance.VBoxProcessId);
            var birth = process.StartTime.ToUniversalTime().Ticks;
            if (!expectedBirths.TryGetValue(index, out var expectedBirth) || birth != expectedBirth) throw new DeviceException($"实例{index}的虚拟机进程已变化，必须重新确认。", 409, 1003);
            var matches = new List<(AdbRegisteredDevice Device, string Endpoint)>();
            foreach (var device in registered.Where(item => item.State == "device" && item.TransportId is > 0).OrderBy(item => item.Serial.StartsWith("emulator-", StringComparison.Ordinal) ? 0 : 1))
            {
                var endpoint = LocalEndpoint(device.Serial);
                if (endpoint is null) continue;
                try { await LdPlayerAdbBinding.VerifyAsync(device.Serial, instance.VBoxProcessId, birth, ct).ConfigureAwait(false); }
                catch (DeviceException) { continue; }
                matches.Add((device, endpoint));
            }
            var endpoints = matches.GroupBy(item => item.Endpoint, StringComparer.Ordinal).ToArray();
            if (endpoints.Length == 0) throw new DeviceException($"实例{index}尚未注册可核验的ADB连接。", 409, 2002);
            if (endpoints.Length != 1) throw new DeviceException($"实例{index}存在多个ADB端点，不能唯一接入。", 409, 1003);
            var chosen = endpoints[0].First().Device;
            var current = (await service.RefreshAsync(ct).ConfigureAwait(false)).Single(item => item.Index == index);
            if (current.Generation != instance.Generation || process.HasExited || process.StartTime.ToUniversalTime().Ticks != birth)
                throw new DeviceException($"实例{index}在接入核验期间发生变化，请重试接入。", 409, 1003);
            proofs.Add(new(index, instance.Name, instance.Generation, chosen.Serial, chosen.TransportId!.Value, instance.VBoxProcessId, birth));
        }
        if (proofs.Select(proof => proof.Serial).Distinct(StringComparer.Ordinal).Count() != proofs.Count) throw new DeviceException("多个实例不能接入同一设备连接。", 409, 1003);
        return proofs.AsReadOnly();
    }

    public static void RequireExpectedInstances(IReadOnlyList<LdPlayerInstance> actual, int[] indices, IReadOnlyList<LdPlayerInstance> expected)
    {
        if (expected.Count != indices.Length || expected.Select(item => item.Index).Distinct().Count() != expected.Count || expected.Any(item => !indices.Contains(item.Index))) throw new DeviceException("接入实例快照不完整或重复。", 409, 1003);
        foreach (var frozen in expected)
        {
            var current = actual.SingleOrDefault(item => item.Index == frozen.Index);
            if (current is null || current.Generation != frozen.Generation || current.Name != frozen.Name || current.ProcessId != frozen.ProcessId || current.VBoxProcessId != frozen.VBoxProcessId)
                throw new DeviceException($"实例 {frozen.Index} 在确认后发生变化，必须重新确认接入。", 409, 1003);
        }
    }

    private static string? LocalEndpoint(string serial)
    {
        if (serial.StartsWith("emulator-", StringComparison.Ordinal) && int.TryParse(serial[9..], out var port) && port is > 0 and < 65535) return "127.0.0.1:" + (port + 1);
        return IPEndPoint.TryParse(serial, out var endpoint) && IPAddress.IsLoopback(endpoint.Address) && endpoint.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork ? endpoint.ToString() : null;
    }

    public static AppSettings Apply(AppSettings settings, IReadOnlyList<LdConnectionProof> proofs)
    {
        if (proofs.Count == 0 || proofs.Any(proof => proof.Index < 0 || proof.Index == 99999 || proof.Generation <= 0 || string.IsNullOrWhiteSpace(proof.Name) || LocalEndpoint(proof.Serial) is null || proof.TransportId == 0 || proof.VBoxPid <= 0 || proof.VBoxBirth <= 0)) throw new DeviceException("接入凭据无效。", 400, 1007);
        if (proofs.Select(proof => proof.Index).Distinct().Count() != proofs.Count || proofs.Select(proof => proof.Serial).Distinct(StringComparer.Ordinal).Count() != proofs.Count) throw new DeviceException("接入凭据存在重复实例或串号。", 409, 1003);
        var bindings = new Dictionary<int, string>(settings.LdSerialBindings);
        var replaced = proofs.Where(proof => bindings.ContainsKey(proof.Index)).Select(proof => bindings[proof.Index]).ToHashSet(StringComparer.Ordinal);
        foreach (var proof in proofs) bindings[proof.Index] = proof.Serial;
        if (bindings.Values.Distinct(StringComparer.Ordinal).Count() != bindings.Count) throw new DeviceException("该连接已绑定其它实例，请先核对原绑定。", 409, 1003);
        return settings with { LdSerialBindings = bindings, AdbAllowedSerials = settings.AdbAllowedSerials.Where(serial => !replaced.Contains(serial)).Concat(proofs.Select(proof => proof.Serial)).Distinct(StringComparer.Ordinal).ToArray(), AutoStartApi = false };
    }
}
