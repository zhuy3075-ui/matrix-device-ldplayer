using System.Text.Json;
using MatrixDevice.Core;

namespace MatrixDevice.Features;

public sealed record NodeOverlay(AndroidNode SourceNode, int Depth, int RenderOrder)
{
    public NodeBounds Bounds => SourceNode.Bounds;
    public bool IsSelected { get; internal set; }
}
public sealed record ShellHistoryEntry(int Index, DateTimeOffset Time, string CommandText, string OutputText, string? Error = null);
public sealed record DebugRefreshResult(string Serial, byte[]? Screenshot, AndroidNode? Root, string? ScreenshotError, string? DumpError);

public sealed class DebugWorkspace(IDeviceBackend backend)
{
    private readonly SemaphoreSlim refreshGate = new(1, 1);
    private readonly SemaphoreSlim shellGate = new(1, 1);
    private readonly object gate = new();
    private List<NodeOverlay> overlays = [];
    private readonly List<ShellHistoryEntry> history = [];
    private byte[]? screenshot;
    private AndroidNode? root;
    private string? serial;
    private AndroidNode? selectedNode;
    public bool IsLoading => refreshGate.CurrentCount == 0;
    public bool IsExecuting => shellGate.CurrentCount == 0;
    public string? Serial { get { lock (gate) return serial; } }
    public byte[]? Screenshot { get { lock (gate) return screenshot?.ToArray(); } }
    public AndroidNode? Root { get { lock (gate) return root; } }
    public AndroidNode? SelectedNode { get { lock (gate) return selectedNode; } }
    public IReadOnlyList<NodeOverlay> Overlays { get { lock (gate) return overlays.ToArray(); } }
    public IReadOnlyList<ShellHistoryEntry> ShellHistory { get { lock (gate) return history.ToArray(); } }
    public event Action<AndroidNode?>? SelectionChanged;

    public async Task<DebugRefreshResult> RefreshAsync(string deviceSerial, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceSerial);
        if (!await refreshGate.WaitAsync(0, ct).ConfigureAwait(false)) throw new DeviceException("A debug refresh is already in progress.");
        try
        {
            byte[]? image = null; AndroidNode? tree = null;
            string? imageError = null; string? treeError = null;
            try { image = await backend.ScreenshotAsync(deviceSerial, ct).ConfigureAwait(false); }
            catch (Exception error) when (error is not OperationCanceledException) { imageError = error.Message; }
            try { tree = await backend.DumpAsync(deviceSerial, ct).ConfigureAwait(false); }
            catch (Exception error) when (error is not OperationCanceledException) { treeError = error.Message; }
            var rectangles = tree is null ? [] : BuildOverlays(tree);
            lock (gate)
            {
                serial = deviceSerial;
                screenshot = image;
                root = tree;
                overlays = rectangles;
                selectedNode = null;
            }
            SelectionChanged?.Invoke(null);
            return new(deviceSerial, image, tree, imageError, treeError);
        }
        finally { refreshGate.Release(); }
    }

    public void SelectNode(AndroidNode? node)
    {
        lock (gate)
        {
            if (node is not null && !overlays.Any(overlay => ReferenceEquals(overlay.SourceNode, node)))
                throw new DeviceException("The selected node does not belong to the current debug snapshot.", 400, 1007);
            selectedNode = node;
            foreach (var overlay in overlays) overlay.IsSelected = ReferenceEquals(overlay.SourceNode, node);
        }
        SelectionChanged?.Invoke(node);
    }

    public void SelectOverlay(NodeOverlay overlay) => SelectNode(overlay.SourceNode);

    public async Task<ShellHistoryEntry> ExecuteShellAsync(string deviceSerial, string command, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceSerial);
        ArgumentException.ThrowIfNullOrWhiteSpace(command);
        if (!await shellGate.WaitAsync(0, ct).ConfigureAwait(false)) throw new DeviceException("A Shell command is already in progress.");
        var text = command.Trim();
        try
        {
            var output = await backend.ShellAsync(deviceSerial, text, ct: ct).ConfigureAwait(false);
            lock (gate)
            {
                var entry = new ShellHistoryEntry(history.Count + 1, DateTimeOffset.UtcNow, text, output);
                history.Add(entry);
                return entry;
            }
        }
        catch (Exception error)
        {
            lock (gate) history.Add(new(history.Count + 1, DateTimeOffset.UtcNow, text, "", error.Message));
            throw;
        }
        finally { shellGate.Release(); }
    }

    public string ExportStructure()
    {
        lock (gate)
            return JsonSerializer.Serialize(root ?? throw new DeviceException("There is no node snapshot to export.", 400, 1007), JsonDefaults.Options);
    }

    public void ExportStructure(string path) => File.WriteAllText(path, ExportStructure(), new System.Text.UTF8Encoding(false));

    public byte[] CropScreenshot(int x, int y, int width, int height)
    {
        var image = Screenshot ?? throw new DeviceException("There is no screenshot to crop.", 400, 1007);
        return PngImage.Crop(image, x, y, width, height);
    }

    public (byte R, byte G, byte B, string Hex) ColorAt(int x, int y)
    {
        var image = Screenshot ?? throw new DeviceException("There is no screenshot for color selection.", 400, 1007);
        var color = PngImage.ColorAt(image, x, y);
        return (color.R, color.G, color.B, $"#{color.R:X2}{color.G:X2}{color.B:X2}");
    }

    private static List<NodeOverlay> BuildOverlays(AndroidNode root)
    {
        var result = new List<NodeOverlay>();
        var pending = new Stack<(AndroidNode Node, int Depth)>();
        var visited = new HashSet<AndroidNode>(ReferenceEqualityComparer.Instance);
        pending.Push((root, 0));
        while (pending.TryPop(out var item))
        {
            if (!visited.Add(item.Node)) throw new DeviceException("Node tree contains a cycle or repeated node.", 400, 1007);
            if (visited.Count > 100000) throw new DeviceException("Node snapshot is too large.", 400, 1007);
            result.Add(new(item.Node, item.Depth, result.Count));
            for (var index = item.Node.ChildNodeList.Count - 1; index >= 0; index--)
                if (item.Node.ChildNodeList[index] is { } child) pending.Push((child, item.Depth + 1));
        }
        return result;
    }
}
