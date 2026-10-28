using BlackSharp.UI.Avalonia.TreeDataGrid.Models;
using BlackSharp.UI.Avalonia.TreeDataGrid.Selection;
using System.Collections.Specialized;
using System.ComponentModel;

namespace BlackSharp.UI.Avalonia.TreeDataGrid
{
    public abstract class TreeDataGridSource<TModel> : TreeDataGridSource,
        ITreeDataGridSource<TModel>,
        IDisposable
        where TModel : class
    {
        #region Constructor

        internal TreeDataGridSource(ITreeDataGridSource<TModel> engine, ColumnList<TModel> columns, TreeDataGridColumns? publicColumns = null)
        {
            _engine                         = engine;
            _engineColumns                  = columns;
            Columns                         = publicColumns ?? [];
            Columns.SourceOwner             = this;
            Columns.AllowExpanderTransition = publicColumns is not null;

            foreach (var column in Columns)
            {
                _engineColumns.Add(CreateAdapter(column));
            }

            Columns.CollectionChanged += OnColumnsChanged;
            _engine.Sorted            += OnSorted;
            _engine.PropertyChanged   += OnEnginePropertyChanged;
            _selection                = new TreeDataGridRowSelectionModel<TModel>(this);
        }

        #endregion

        #region Fields

        private readonly ITreeDataGridSource<TModel> _engine;

        private readonly ColumnList<TModel>          _engineColumns;

        private TreeDataGridProjectedRows<TModel>?   _rows;

        private TreeDataGridSelectionModel?          _selection;

        private Func<TModel, bool>?                  _filter;

        private bool                                 _disposed;

        #endregion

        #region Properties

        public new virtual IEnumerable<TModel> Items
        {
            get => _engine.Items;
            set
            {
                ThrowIfDisposed();
                ArgumentNullException.ThrowIfNull(value);

                _engine.Items = value;

                Selection?.SetSource(value);
                RaisePropertyChanged();
            }
        }

        public TreeDataGridCellSelectionModel<TModel>? CellSelection => Selection as TreeDataGridCellSelectionModel<TModel>;

        public TreeDataGridRowSelectionModel<TModel>? RowSelection => Selection as TreeDataGridRowSelectionModel<TModel>;

        public sealed override TreeDataGridColumns Columns { get; }

        public sealed override TreeDataGridRows Rows
        {
            get
            {
                ThrowIfDisposed();

                if (_rows is null)
                {
                    _rows = new TreeDataGridProjectedRows<TModel>(_engine.Rows);
                    _rows.Filter(_filter);
                }

                return _rows;
            }
        }

        public sealed override TreeDataGridSelectionModel? Selection
        {
            get => _selection;
            set
            {
                ThrowIfDisposed();

                if (value is not null && !ReferenceEquals(value.Owner, this))
                {
                    throw new InvalidOperationException("The selection model must belong to this source.");
                }

                if (ReferenceEquals(_selection, value))
                {
                    return;
                }

                _selection?.SetSource(null);
                value?.SetSource(Items);

                if (RaiseAndSetIfChanged(ref _selection, value))
                {
                    RaisePropertyChanged(nameof(RowSelection));
                    RaisePropertyChanged(nameof(CellSelection));
                }
            }
        }

        public sealed override bool IsFiltered => _filter is not null;

        public sealed override bool IsHierarchical => _engine.IsHierarchical;

        public sealed override bool IsSorted => _engine.IsSorted;

        internal sealed override ITreeDataGridSource Engine => _engine;

        internal sealed override IColumns EngineColumns => _engineColumns;

        #endregion

        #region Public

        public sealed override void ClearSort(TreeDataGridColumn column)
        {
            ThrowIfDisposed();

            if (Columns.Contains(column) && column.SortDirection is not null)
            {
                _engine.Unsort();
            }
        }

        public sealed override bool SortBy(TreeDataGridColumn column, ListSortDirection direction)
        {
            ThrowIfDisposed();
            var index = Columns.IndexOf(column);

            return index >= 0 && _engine.SortBy(_engineColumns[index], direction);
        }

        public void Filter(Func<TModel, bool>? filter)
        {
            ThrowIfDisposed();

            _filter = filter;
            _rows?.Filter(filter);

            RaisePropertyChanged(nameof(IsFiltered));
        }

        public void RefreshFilter()
        {
            ThrowIfDisposed();
            _rows?.Refresh();
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _selection?.SetSource(null);

            _disposed  = true;
            IsDisposed = true;

            RaisePropertyChanged(nameof(Rows));

            Columns.CollectionChanged -= OnColumnsChanged;
            _engine.Sorted            -= OnSorted;
            _engine.PropertyChanged   -= OnEnginePropertyChanged;

            Columns.SourceOwner       = null;

            _rows?.Dispose();
            _rows = null;

            foreach (var column in _engineColumns.OfType<IDisposable>())
            {
                column.Dispose();
            }

            (_engine as IDisposable)?.Dispose();
            _engine.Items = Array.Empty<TModel>();
            _filter       = null;
        }

        #endregion

        #region Internal

        internal sealed override TreeDataGridSelectionModel CreateSelection(bool cell) =>
            cell ? new TreeDataGridCellSelectionModel<TModel>(this) : new TreeDataGridRowSelectionModel<TModel>(this);

        #endregion

        #region Protected

        protected sealed override IEnumerable<object> GetItems() => Items;

        #endregion

        #region Private

        private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(_disposed, this);

        private void OnEnginePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Rows) && _rows is not null)
            {
                _rows.Dispose();

                _rows = null;

                RaisePropertyChanged(nameof(Rows));
                _selection?.ClearCore();
            }
        }

        private void OnSorted()
        {
            _rows?.Refresh();

            RaisePropertyChanged(nameof(IsSorted));
            RaiseSorted();
        }

        private TreeDataGridColumnAdapter<TModel> CreateAdapter(TreeDataGridColumn column) =>
            column is TreeDataGridHierarchicalExpanderColumn expander ?
                new TreeDataGridExpanderColumnAdapter<TModel>(expander, InvalidateColumnLayout, RebuildRows) : new TreeDataGridColumnAdapter<TModel>(column, InvalidateColumnLayout);

        private void RebuildRows()
        {
            if (_engine is EngineHierarchicalTreeDataGridSource<TModel> hierarchical)
            {
                hierarchical.RebuildRows();
            }
        }

        private void InvalidateColumnLayout(bool refreshCells)
        {
            ((IColumns)_engineColumns).InvalidateLayout();

            if (refreshCells)
            {
                _rows?.Refresh();
            }
        }

        private void OnColumnsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (_disposed)
            {
                return;
            }

            if (e.Action == NotifyCollectionChangedAction.Move)
            {
                _engineColumns.Move(e.OldStartingIndex, e.NewStartingIndex);

                return;
            }

            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                _engine.Unsort();

                foreach (var adapter in _engineColumns.OfType<IDisposable>())
                {
                    adapter.Dispose();
                }

                _engineColumns.Clear();
            }
            else
            {
                if (e.OldItems is not null)
                {
                    if (e.OldItems.Cast<TreeDataGridColumn>().Any(x => x.SortDirection is not null))
                    {
                        _engine.Unsort();
                    }

                    for (var i = 0; i < e.OldItems.Count; ++i)
                    {
                        (_engineColumns[e.OldStartingIndex] as IDisposable)?.Dispose();
                        _engineColumns.RemoveAt(e.OldStartingIndex);
                    }
                }

                if (e.NewItems is not null)
                {
                    var index = e.NewStartingIndex;

                    foreach (TreeDataGridColumn column in e.NewItems)
                    {
                        _engineColumns.Insert(index++, CreateAdapter(column));
                    }
                }
            }

            ((IColumns)_engineColumns).InvalidateLayout();
        }

        #endregion

        #region ITreeDataGridSource<TModel>

        void ITreeDataGridSource<TModel>.Sort(Comparison<TModel>? comparison) => _engine.Sort(comparison);

        #endregion
    }
}
