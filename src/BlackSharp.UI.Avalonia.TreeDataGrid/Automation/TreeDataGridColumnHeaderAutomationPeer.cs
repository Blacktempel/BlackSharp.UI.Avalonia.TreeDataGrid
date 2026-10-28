using Avalonia.Automation.Peers;
using BlackSharp.UI.Avalonia.TreeDataGrid.Primitives;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Automation.Peers
{
    public class TreeDataGridColumnHeaderAutomationPeer : ButtonAutomationPeer
    {
        #region Constructor

        public TreeDataGridColumnHeaderAutomationPeer(TreeDataGridColumnHeader owner) : base(owner)
        {
        }

        #endregion

        #region Properties

        public new TreeDataGridColumnHeader Owner => (TreeDataGridColumnHeader)base.Owner;

        #endregion

        #region Protected

        protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.HeaderItem;

        protected override string GetNameCore() => Owner.Header?.ToString() ?? string.Empty;

        protected override string GetItemStatusCore() => Owner.SortDirection?.ToString() ?? string.Empty;

        #endregion
    }
}
