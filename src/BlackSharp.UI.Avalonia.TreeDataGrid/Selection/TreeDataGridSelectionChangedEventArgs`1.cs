namespace BlackSharp.UI.Avalonia.TreeDataGrid.Selection
{
    public class TreeDataGridSelectionChangedEventArgs<TModel> : TreeDataGridSelectionChangedEventArgs
        where TModel : class
    {
        #region Constructor

        public TreeDataGridSelectionChangedEventArgs(IReadOnlyList<IndexPath>? deselectedIndexes = null,
            IReadOnlyList<IndexPath>? selectedIndexes     = null, IReadOnlyList<TModel?>? deselectedItems = null,
            IReadOnlyList<TModel?>? selectedItems         = null, IReadOnlyList<CellIndex>? deselectedCellIndexes = null,
            IReadOnlyList<CellIndex>? selectedCellIndexes = null)
        {
            DeselectedIndexes     = deselectedIndexes ?? [];
            SelectedIndexes       = selectedIndexes ?? [];
            _deselectedItems      = deselectedItems ?? [];
            _selectedItems        = selectedItems ?? [];
            DeselectedCellIndexes = deselectedCellIndexes ?? [];
            SelectedCellIndexes   = selectedCellIndexes ?? [];
        }

        #endregion

        #region Fields

        private readonly IReadOnlyList<TModel?> _deselectedItems;

        private readonly IReadOnlyList<TModel?> _selectedItems;

        #endregion

        #region Properties

        public override IReadOnlyList<IndexPath> DeselectedIndexes { get; }

        public override IReadOnlyList<IndexPath> SelectedIndexes { get; }

        public new IReadOnlyList<TModel?> DeselectedItems => _deselectedItems;

        public new IReadOnlyList<TModel?> SelectedItems => _selectedItems;

        public override IReadOnlyList<CellIndex> DeselectedCellIndexes { get; }

        public override IReadOnlyList<CellIndex> SelectedCellIndexes { get; }

        #endregion

        #region Protected

        protected override IReadOnlyList<object?> GetDeselectedItems() => _deselectedItems;

        protected override IReadOnlyList<object?> GetSelectedItems() => _selectedItems;

        #endregion
    }
}
