using System.Collections;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Selection
{
    internal class SelectedCellIndexes : IReadOnlyList<CellIndex>
    {
        #region Constructor

        public SelectedCellIndexes(
            ITreeDataGridColumnSelectionModel selectedColumns,
            ITreeDataGridRowSelectionModel selectedRows)
        {
            _selectedColumns = selectedColumns;
            _selectedRows    = selectedRows;
        }

        #endregion

        #region Fields

        private readonly ITreeDataGridColumnSelectionModel _selectedColumns;

        private readonly ITreeDataGridRowSelectionModel    _selectedRows;

        #endregion

        #region Properties

        public CellIndex this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                {
                    throw new IndexOutOfRangeException("The index was out of range.");
                }

                var column = _selectedColumns.SelectedIndexes[index % _selectedColumns.Count];
                var row    = _selectedRows.SelectedIndexes[index / _selectedColumns.Count];

                return new(column, row);
            }
        }

        public int Count => _selectedColumns.Count * _selectedRows.Count;

        #endregion

        #region Public

        public IEnumerator<CellIndex> GetEnumerator()
        {
            for (var i = 0; i < Count; ++i)
            {
                yield return this[i];
            }
        }

        #endregion

        #region IEnumerable

        IEnumerator IEnumerable.GetEnumerator()
        {
            for (var i = 0; i < Count; ++i)
            {
                yield return this[i];
            }
        }

        #endregion
    }
}
