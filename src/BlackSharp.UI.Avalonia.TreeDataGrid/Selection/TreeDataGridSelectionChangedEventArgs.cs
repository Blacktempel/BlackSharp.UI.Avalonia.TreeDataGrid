namespace BlackSharp.UI.Avalonia.TreeDataGrid.Selection
{
    public abstract class TreeDataGridSelectionChangedEventArgs : EventArgs
    {
        #region Properties

        public abstract IReadOnlyList<IndexPath> DeselectedIndexes { get; }

        public abstract IReadOnlyList<IndexPath> SelectedIndexes { get; }

        public IReadOnlyList<object?> DeselectedItems => GetDeselectedItems();

        public IReadOnlyList<object?> SelectedItems => GetSelectedItems();

        public abstract IReadOnlyList<CellIndex> DeselectedCellIndexes { get; }

        public abstract IReadOnlyList<CellIndex> SelectedCellIndexes { get; }

        #endregion

        #region Protected

        protected abstract IReadOnlyList<object?> GetDeselectedItems();

        protected abstract IReadOnlyList<object?> GetSelectedItems();

        #endregion
    }
}
