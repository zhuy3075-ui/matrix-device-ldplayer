namespace MatrixDevice.Core;

public sealed record LdPlayerInstance(
    int Index, string Name, long TopWindow, long BindWindow, bool AndroidStarted,
    int ProcessId, int VBoxProcessId, int? Width = null, int? Height = null, int? Dpi = null,
    long Generation = 1, bool IsStarting = false);

public sealed record LdPlayerCommandResult(
    int Index, string Operation, bool Accepted, bool Verified, string Message,
    LdPlayerInstance? Instance);

public sealed record LdPlayerCapability(
    string Operation, bool Supported, bool RequiresConfirmation, string Description,
    IReadOnlyList<string> Limitations);

public interface ILdConsoleRunner
{
    Task<string> RunAsync(IReadOnlyList<string> arguments, CancellationToken ct = default);
}
