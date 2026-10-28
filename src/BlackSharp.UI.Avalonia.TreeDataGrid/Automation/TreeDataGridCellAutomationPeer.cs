using Avalonia.Automation.Peers;
using BlackSharp.UI.Avalonia.TreeDataGrid.Primitives;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Automation.Peers
{
    public class TreeDataGridCellAutomationPeer : ControlAutomationPeer
    {
        #region Constructor

        public TreeDataGridCellAutomationPeer(TreeDataGridCell owner) : base(owner)
        {
        }

        #endregion

        #region Properties

        public new TreeDataGridCell Owner => (TreeDataGridCell)base.Owner;

        #endregion

        #region Protected

        protected override bool IsContentElementCore() => true;

        protected override bool IsControlElementCore() => true;

        protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.DataItem;

        protected override string GetNameCore() => Owner.Model?.Value?.ToString() ?? string.Empty;

        #endregion
    }
}
