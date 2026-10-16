using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;

namespace MatrixDevice.Desktop;

internal sealed class WorkspaceTabControl : TabControl
{
    public WorkspaceTabControl()
    {
        SetResourceReference(StyleProperty, typeof(TabControl));
        AutomationProperties.SetName(this, "工作区正文");
    }

    protected override AutomationPeer OnCreateAutomationPeer() => new WorkspaceContentAutomationPeer(this);

    private sealed class WorkspaceContentAutomationPeer(WorkspaceTabControl owner) : FrameworkElementAutomationPeer(owner)
    {
        protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Pane;
    }
}
