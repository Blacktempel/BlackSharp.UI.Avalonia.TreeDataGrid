using Avalonia.Automation.Peers;
using BlackSharp.UI.Avalonia.TreeDataGrid.Primitives;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Automation.Peers
{
    public class TreeDataGridColumnHeadersPresenterAutomationPeer : ControlAutomationPeer
    {
        #region Constructor

        public TreeDataGridColumnHeadersPresenterAutomationPeer(TreeDataGridColumnHeadersPresenter owner) : base(owner)
        {
        }

        #endregion

        #region Protected

        protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Header;

        protected override bool IsContentElementCore() => false;

        protected override bool IsControlElementCore() => true;

        protected override IReadOnlyList<AutomationPeer> GetOrCreateChildrenCore() =>
            ((TreeDataGridColumnHeadersPresenter)Owner).GetRealizedElements().Select(GetOrCreate).ToArray();

        #endregion
    }
}
