using System.Text.Json;
using MatrixDevice.Core;

namespace MatrixDevice.Features;

public static class ScriptExecutionCompiler
{
    public static ScriptPackage Compile(ScriptPackage package, IReadOnlyDictionary<string, JsonElement>? parameterOverrides = null)
    {
        ScriptPackageService.Validate(package);
        if (package.ExecutionContract is null)
        {
            if (parameterOverrides is { Count: > 0 }) throw Invalid("旧脚本包不接受参数覆盖；请显式声明 version=1 的 executionContract。");
            return ScriptPackageService.CloneForExecution(package);
        }

        var parameters = package.Parameters.ToDictionary(parameter => parameter.Key, StringComparer.Ordinal);
        var overrides = new Dictionary<string, JsonElement>(StringComparer.Ordinal);
        if (parameterOverrides is not null)
        {
            foreach (var (key, value) in parameterOverrides)
            {
                if (string.IsNullOrWhiteSpace(key) || !parameters.TryGetValue(key, out var parameter))
                    throw Invalid($"覆盖参数 '{key}' 未声明（区分大小写）。");
                RequireValue(key, value, "覆盖值");
                try { ScriptPackageService.ValidateValue(parameter, value, "Value"); }
                catch (DeviceException error) { throw Invalid($"覆盖参数 {key} 的值 {value.GetRawText()} 不符合声明：{error.Message}"); }
                overrides.Add(key, value.Clone());
            }
        }

        var snapshot = ScriptPackageService.CloneForExecution(package);
        var steps = snapshot.Steps.ToList();
        foreach (var binding in snapshot.ExecutionContract!.Bindings!)
        {
            var parameter = parameters[binding.ParameterKey];
            var value = overrides.TryGetValue(binding.ParameterKey, out var supplied) ? supplied
                : parameter.Value.HasValue ? parameter.Value.Value
                : parameter.DefaultValue.HasValue ? parameter.DefaultValue.Value
                : throw Invalid($"步骤 {binding.StepIndex} 字段 {binding.Field} 的参数 {binding.ParameterKey} 缺少 Value 和 DefaultValue。");
            RequireValue(binding.ParameterKey, value, $"步骤 {binding.StepIndex} 字段 {binding.Field}");
            var step = steps[binding.StepIndex];
            if (binding.Field == "value")
            {
                if (value.ValueKind != JsonValueKind.String) throw Invalid($"步骤 {binding.StepIndex} 字段 value 的参数 {binding.ParameterKey} 必须为完整 JSON 字符串，实际为 {value.GetRawText()}。");
                steps[binding.StepIndex] = step with { Value = value.GetString() };
            }
            else
            {
                if (value.ValueKind != JsonValueKind.Number || !value.TryGetInt32(out var duration) || duration < 0)
                    throw Invalid($"步骤 {binding.StepIndex} 字段 durationMs 的参数 {binding.ParameterKey} 必须为非负 Int32 整数，实际为 {value.GetRawText()}。");
                steps[binding.StepIndex] = step with { DurationMs = duration };
            }
        }
        var resolved = snapshot with
        {
            Steps = steps,
            Parameters = snapshot.Parameters.Select(parameter => overrides.TryGetValue(parameter.Key, out var value) ? parameter with { Value = value.Clone() } : parameter).ToList(),
            ExecutionContract = null
        };
        try { ScriptPackageService.Validate(resolved); }
        catch (DeviceException error) { throw Invalid($"参数绑定后的步骤或参数无效：{error.Message}"); }
        return resolved;
    }

    private static void RequireValue(string key, JsonElement value, string source)
    {
        if (value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            throw Invalid($"参数 {key} 的{source}为 {value.ValueKind.ToString().ToLowerInvariant()}，不能回退或执行。");
    }
    private static DeviceException Invalid(string message) => new(message, 400, 1007);
}
