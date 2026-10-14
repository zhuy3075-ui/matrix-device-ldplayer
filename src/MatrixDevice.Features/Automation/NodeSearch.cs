using MatrixDevice.Core;

namespace MatrixDevice.Features;

public static class NodeSearch
{
    public static IReadOnlyList<AndroidNode> FindAll(AndroidNode root, NodeSelector selector)
    {
        ArgumentNullException.ThrowIfNull(root);
        ArgumentNullException.ThrowIfNull(selector);
        var found = new List<AndroidNode>();
        var pending = new Stack<AndroidNode>();
        pending.Push(root);
        var visited = new HashSet<AndroidNode>(ReferenceEqualityComparer.Instance);
        while (pending.TryPop(out var node))
        {
            if (!visited.Add(node)) throw new DeviceException("Node tree contains a repeated reference or cycle.", 400, 1007);
            if (visited.Count > 100000) throw new DeviceException("Node tree exceeds 100000 nodes.", 400, 1007);
            if (Matches(node, selector)) found.Add(node);
            for (var index = node.ChildNodeList.Count - 1; index >= 0; index--)
                if (node.ChildNodeList[index] is { } child) pending.Push(child);
        }
        return found;
    }

    public static AndroidNode? Find(AndroidNode root, NodeSelector selector) => FindAll(root, selector).FirstOrDefault();

    public static bool Matches(AndroidNode node, NodeSelector selector) =>
        StringMatch(node.Text, selector.Text) &&
        (selector.TextContains is null || (node.Text?.Contains(selector.TextContains, StringComparison.Ordinal) ?? false)) &&
        StringMatch(node.ResourceId, selector.ResourceId) && StringMatch(node.ClassName, selector.ClassName) &&
        StringMatch(node.PackageName, selector.PackageName) &&
        Match(node.Index, selector.Index) && Match(node.Instance, selector.Instance) &&
        Match(node.Clickable, selector.Clickable) && Match(node.Enabled, selector.Enabled) &&
        Match(node.Focusable, selector.Focusable) && Match(node.Focused, selector.Focused) &&
        Match(node.Scrollable, selector.Scrollable) && Match(node.LongClickable, selector.LongClickable) &&
        Match(node.Checkable, selector.Checkable) && Match(node.Checked, selector.Checked) &&
        Match(node.Selected, selector.Selected) && Match(node.VisibleToUser, selector.VisibleToUser);

    private static bool StringMatch(string? actual, string? expected) => expected is null || string.Equals(actual, expected, StringComparison.Ordinal);
    private static bool Match<T>(T actual, T? expected) where T : struct => expected is null || EqualityComparer<T>.Default.Equals(actual, expected.Value);
}
