namespace BlackSharp.UI.Avalonia.TreeDataGrid.Selection
{
    public class TreeDataGridItemsResetEventArgs : EventArgs
    {
        #region Constructor

        public TreeDataGridItemsResetEventArgs(IndexPath parentIndex) => ParentIndex = parentIndex;

        #endregion

        #region Properties

        public IndexPath ParentIndex { get; }

        #endregion
    }
}
