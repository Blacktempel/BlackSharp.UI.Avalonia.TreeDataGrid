namespace BlackSharp.UI.Avalonia.TreeDataGrid.Selection
{
    public class TreeDataGridIndexesChangedEventArgs : EventArgs
    {
        #region Constructor

        public TreeDataGridIndexesChangedEventArgs(IndexPath parentIndex, int startIndex, int endIndex, int delta)
        {
            ParentIndex = parentIndex;
            StartIndex  = startIndex;
            EndIndex    = endIndex;
            Delta       = delta;
        }

        #endregion

        #region Properties

        public IndexPath ParentIndex { get; }

        public int StartIndex { get; }

        public int EndIndex { get; }

        public int Delta { get; }

        #endregion
    }
}
