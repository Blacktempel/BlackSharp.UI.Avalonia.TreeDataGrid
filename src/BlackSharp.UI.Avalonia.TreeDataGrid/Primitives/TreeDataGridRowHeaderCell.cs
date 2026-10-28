using Avalonia.Automation.Peers;
using Avalonia.Input;
using Avalonia.VisualTree;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Primitives
{
    public class TreeDataGridRowHeaderCell : TreeDataGridTextCell
    {
        #region Properties

        protected override Type StyleKeyOverride => typeof(TreeDataGridTextCell);

        #endregion

        #region Protected

        protected override AutomationPeer OnCreateAutomationPeer() =>
            new Automation.Peers.TreeDataGridRowHeaderCellAutomationPeer(this);

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed &&
                this.FindAncestorOfType<TreeDataGrid>() is { Source: { } source, Selection: { } selection } &&
                (uint)RowIndex < source.Rows.Count)
            {
                selection.SelectRowCore(source.RowIndexToModelIndex(RowIndex));
                e.Handled = true;
            }

            base.OnPointerPressed(e);
        }

        #endregion
    }
}
