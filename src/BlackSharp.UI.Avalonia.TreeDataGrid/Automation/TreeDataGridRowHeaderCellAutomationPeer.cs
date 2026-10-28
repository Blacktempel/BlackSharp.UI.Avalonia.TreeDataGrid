using Avalonia.Automation.Peers;
using BlackSharp.UI.Avalonia.TreeDataGrid.Primitives;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Automation.Peers
{
    public class TreeDataGridRowHeaderCellAutomationPeer : TreeDataGridCellAutomationPeer
    {
        #region Constructor

        public TreeDataGridRowHeaderCellAutomationPeer(TreeDataGridRowHeaderCell owner) : base(owner)
        {
        }

        #endregion

        #region Properties

        public new TreeDataGridRowHeaderCell Owner => (TreeDataGridRowHeaderCell)base.Owner;

        #endregion

        #region Protected

        protected override string GetNameCore() => Owner.Value ?? string.Empty;

        protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.HeaderItem;

        #endregion
    }
}
