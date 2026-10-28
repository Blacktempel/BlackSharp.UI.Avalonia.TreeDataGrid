namespace BlackSharp.UI.Avalonia.TreeDataGrid.Selection
{
    internal class TreeSelectionModelSourceResetEventArgs : EventArgs
    {
        #region Constructor

        public TreeSelectionModelSourceResetEventArgs(IndexPath parentIndex)
        {
            ParentIndex = parentIndex;
        }

        #endregion

        #region Properties

        public IndexPath ParentIndex { get; }

        #endregion
    }
}
