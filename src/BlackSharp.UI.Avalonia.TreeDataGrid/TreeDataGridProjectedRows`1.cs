using BlackSharp.UI.Avalonia.TreeDataGrid.Models;
using System.Collections.Specialized;

namespace BlackSharp.UI.Avalonia.TreeDataGrid
{
    internal sealed class TreeDataGridProjectedRows<TModel> : TreeDataGridRows, IRows, IDisposable
        where TModel : class
    {
        #region Constructor

        internal TreeDataGridProjectedRows(IRows rows)
        {
            _rows                   = rows;
            _rows.CollectionChanged += OnRowsChanged;
        }

        #endregion

        #region Fields

        private readonly IRows      _rows;

        private List<int>?          _visibleIndexes;

        private Func<TModel, bool>? _filter;

        #endregion

        #region Properties

        public override int Count => _visibleIndexes?.Count ?? _rows.Count;

        public override ITreeDataGridRowModel this[int index]
        {
            get
            {
                var actual = GetActualIndex(index);

                return new TreeDataGridRowModel(_rows[actual], _rows.RowIndexToModelIndex(actual));
            }
        }

        #endregion

        #region Public

        public override IEnumerator<ITreeDataGridRowModel> GetEnumerator()
        {
            for (var i = 0; i < Count; ++i)
            {
                yield return this[i];
            }
        }

        public int ModelIndexToRowIndex(IndexPath index)
        {
            var actual = _rows.ModelIndexToRowIndex(index);

            if (_visibleIndexes is null)
            {
                return actual;
            }

            var visible = _visibleIndexes.BinarySearch(actual);

            return visible >= 0 ? visible : -1;
        }

        public IndexPath RowIndexToModelIndex(int index) =>
            (uint)index < Count ? _rows.RowIndexToModelIndex(GetActualIndex(index)) : default;

        public ICell RealizeCell(IColumn column, int columnIndex, int rowIndex) =>
            _rows.RealizeCell(column, columnIndex, GetActualIndex(rowIndex));

        public void UnrealizeCell(ICell cell, int columnIndex, int rowIndex) =>
            (cell as IDisposable)?.Dispose();

        public (int index, double y) GetRowAt(double y) =>
            _filter is null ? _rows.GetRowAt(y) : y == 0 && Count > 0 ? (0, 0) : (-1, -1);

        public void Dispose()
        {
            _rows.CollectionChanged -= OnRowsChanged;
            _filter                 = null;
            _visibleIndexes         = null;
        }

        #endregion

        #region Internal

        internal void Filter(Func<TModel, bool>? predicate)
        {
            _filter = predicate;
            Refresh();
        }

        internal void Refresh()
        {
            RebuildFilter();
            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        #endregion

        #region Private

        private int GetActualIndex(int index) => _visibleIndexes?[index] ?? index;

        private void RebuildFilter()
        {
            _visibleIndexes = null;

            if (_filter is null)
            {
                return;
            }

            var indexes = new List<int>();
            var parents = new HashSet<IndexPath>();

            for (var i = 0; i < _rows.Count; ++i)
            {
                var path = _rows.RowIndexToModelIndex(i);

                if ((path.Count <= 1 || parents.Contains(path[..^1])) && _filter((TModel)_rows[i].Model!))
                {
                    indexes.Add(i);
                    parents.Add(path);
                }
            }

            _visibleIndexes = indexes;
        }

        private void OnRowsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            RebuildFilter();
            RaiseCollectionChanged(_filter is null ? e : new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        #endregion

        #region IEnumerable<IRow>

        IEnumerator<IRow> IEnumerable<IRow>.GetEnumerator()
        {
            for (var i = 0; i < Count; ++i)
            {
                yield return _rows[GetActualIndex(i)];
            }
        }

        #endregion

        #region IReadOnlyList<IRow>

        IRow IReadOnlyList<IRow>.this[int index] => _rows[GetActualIndex(index)];

        #endregion
    }
}
