namespace MatrixDevice.Features;

public sealed record ScriptJobTarget(string Serial, long Generation, uint? TransportId, bool IsDemo);

public sealed record ScriptJobStatus(Guid JobId, ScriptJobTarget Target, string PackageId, string PackageVersion,
    ScriptRunState State, int StepIndex, int StepCount, DateTimeOffset StartedUtc,
    DateTimeOffset? EndedUtc = null, string? Error = null, string? Result = null);

public sealed record ScriptJobHandle(Guid JobId, Task Completion);
