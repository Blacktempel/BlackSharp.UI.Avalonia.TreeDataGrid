namespace BlackSharp.UI.Avalonia.TreeDataGrid.Selection
{
    /// <summary>
    /// Holds data for the <see cref="ITreeSelectionModel.IndexesChanged"/> event.
    /// </summary>
    internal class TreeSelectionModelIndexesChangedEventArgs : EventArgs
    {
        #region Constructor

        public TreeSelectionModelIndexesChangedEventArgs(
            IndexPath parentIndex,
            int startIndex,
            int endIndex,
            int delta)
        {
            ParentIndex = parentIndex;
            StartIndex  = startIndex;
            EndIndex    = endIndex;
            Delta       = delta;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the index of the parent item.
        /// </summary>
        public IndexPath ParentIndex { get; }

        /// <summary>
        /// Gets the inclusive start index of the range of indexes that changed.
        /// </summary>
        public int StartIndex { get; }

        /// <summary>
        /// Gets the exclusive end index of the range of indexes that changed.
        /// </summary>
        public int EndIndex { get; }

        /// <summary>
        /// Gets the delta of the change; i.e. the number of indexes added or removed.
        /// </summary>
        public int Delta { get; }

        #endregion
    }
}
