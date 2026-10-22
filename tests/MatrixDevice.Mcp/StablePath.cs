using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace MatrixDevice.Mcp;

internal sealed class StablePath : IDisposable
{
    private readonly List<SafeFileHandle> handles = [];
    internal string FullPath { get; private init; } = "";
    internal SafeFileHandle Leaf => handles[^1];
    internal static string Identity(SafeFileHandle handle)
    {
        if (!GetFileInformationByHandle(handle, out var info)) throw new Win32Exception(Marshal.GetLastWin32Error());
        return $"{info.Volume:x8}:{info.IndexHigh:x8}{info.IndexLow:x8}:{info.Creation.dwHighDateTime:x8}{info.Creation.dwLowDateTime:x8}";
    }

    internal static string Absolute(string value)
    {
        if (!OperatingSystem.IsWindows() || !Path.IsPathFullyQualified(value) || value.StartsWith("\\", StringComparison.Ordinal)
            || value.Length < 3 || value[1] != ':' || value.AsSpan(2).Contains(':') || value.Contains('/')
            || value.Length == 3 || value.EndsWith('\\')
            || value.Split('\\').Skip(1).Any(s => s is "." or ".." || s.Length == 0 || s.EndsWith('.') || s.EndsWith(' ') || s.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0
                || IsReservedName(s)))
            throw new ToolFailure("INVALID_ARGUMENT", "Expected an absolute local Windows path without ambiguous segments.");
        return Path.GetFullPath(value).TrimEnd('\\');
    }

    private static bool IsReservedName(string value)
    {
        var name = value.Split('.')[0].ToUpperInvariant();
        return name is "CON" or "PRN" or "AUX" or "NUL" || name.Length == 4 && name[3] is >= '0' and <= '9' && (name.StartsWith("COM", StringComparison.Ordinal) || name.StartsWith("LPT", StringComparison.Ordinal));
    }

    internal static bool DeleteOwned(string path, string? identity)
    {
        if (identity is null) return false;
        using var handle = CreateFileW(path, 0x10000 | 0x80, 7, IntPtr.Zero, 3, 0x00200000, IntPtr.Zero);
        if (handle.IsInvalid || !GetFileInformationByHandle(handle, out var info) || (info.Attributes & (0x400 | 0x10)) != 0 || Identity(handle) != identity) return false;
        var disposition = new FileDisposition { Delete = true };
        return SetFileInformationByHandle(handle, 4, ref disposition, 1);
    }

    internal static StablePath Open(string path, bool directory)
    {
        path = Absolute(path);
        var result = new StablePath { FullPath = path };
        try
        {
            var volume = Path.GetPathRoot(path)!;
            var segments = path[volume.Length..].Split('\\', StringSplitOptions.RemoveEmptyEntries);
            var current = volume;
        result.Add(current, true);
            for (var i = 0; i < segments.Length; i++)
            {
                current = Path.Combine(current, segments[i]);
                result.Add(current, i < segments.Length - 1 || directory);
            }
            return result;
        }
        catch { result.Dispose(); throw; }
    }

    private void Add(string path, bool directory)
    {
        const uint genericRead = 0x80000000, readAttributes = 0x80, shareRead = 1, shareWrite = 2;
        var handle = CreateFileW(path, directory ? readAttributes : genericRead, directory ? shareRead | shareWrite : shareRead,
            IntPtr.Zero, 3, 0x00200000u | (directory ? 0x02000000u : 0), IntPtr.Zero);
        if (handle.IsInvalid) { handle.Dispose(); throw new ToolFailure("INVALID_ARGUMENT", "Cannot safely open path: " + new Win32Exception(Marshal.GetLastWin32Error()).Message); }
        handles.Add(handle);
        if (!GetFileInformationByHandle(handle, out var info)) throw new Win32Exception(Marshal.GetLastWin32Error());
        if ((info.Attributes & 0x400) != 0 || ((info.Attributes & 0x10) != 0) != directory) throw new ToolFailure("INVALID_ARGUMENT", "Reparse points and unexpected path kinds are not allowed.");
        var final = new StringBuilder(32768);
        var length = GetFinalPathNameByHandleW(handle, final, (uint)final.Capacity, 0);
        if (length == 0 || length >= final.Capacity) throw new ToolFailure("INVALID_ARGUMENT", "Cannot prove final file handle path.");
        var actual = final.ToString();
        if (actual.StartsWith("\\\\?\\", StringComparison.Ordinal)) actual = actual[4..];
        if (!string.Equals(actual.TrimEnd('\\'), Path.GetFullPath(path).TrimEnd('\\'), StringComparison.OrdinalIgnoreCase))
            throw new ToolFailure("INVALID_ARGUMENT", "Final file handle path does not match the authorized path.");
    }

    internal static void CreateFreshDirectory(string path)
    {
        if (!CreateDirectoryW(path, IntPtr.Zero)) throw new ToolFailure("INVALID_ARGUMENT", "Data directory must be fresh; creation failed: " + new Win32Exception(Marshal.GetLastWin32Error()).Message);
    }

    public void Dispose() { for (var i = handles.Count - 1; i >= 0; i--) handles[i].Dispose(); handles.Clear(); }

    [StructLayout(LayoutKind.Sequential)] private struct FileInformation
    { internal uint Attributes; internal System.Runtime.InteropServices.ComTypes.FILETIME Creation, Access, Write; internal uint Volume, SizeHigh, SizeLow, Links, IndexHigh, IndexLow; }
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)] private static extern SafeFileHandle CreateFileW(string name, uint access, uint share, IntPtr security, uint disposition, uint flags, IntPtr template);
    [DllImport("kernel32.dll", SetLastError = true)] private static extern bool GetFileInformationByHandle(SafeFileHandle handle, out FileInformation information);
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)] private static extern uint GetFinalPathNameByHandleW(SafeFileHandle handle, StringBuilder buffer, uint capacity, uint flags);
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)] private static extern bool CreateDirectoryW(string path, IntPtr security);
    [StructLayout(LayoutKind.Sequential)] private struct FileDisposition { [MarshalAs(UnmanagedType.U1)] internal bool Delete; }
    [DllImport("kernel32.dll", SetLastError = true)] private static extern bool SetFileInformationByHandle(SafeFileHandle handle, int informationClass, ref FileDisposition disposition, uint size);
}
