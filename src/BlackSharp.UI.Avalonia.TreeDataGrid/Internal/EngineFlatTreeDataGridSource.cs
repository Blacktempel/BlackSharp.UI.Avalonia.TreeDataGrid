using Avalonia.Input;
using BlackSharp.UI.Avalonia.TreeDataGrid.Models;
using BlackSharp.UI.Avalonia.TreeDataGrid.Selection;
using System.ComponentModel;

namespace BlackSharp.UI.Avalonia.TreeDataGrid
{
    /// <summary>
    /// A data source for a <see cref="TreeDataGrid"/> which displays a flat grid.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    internal class EngineFlatTreeDataGridSource<TModel> : NotifyingBase,
        ITreeDataGridSource<TModel>,
        IDisposable
        where TModel : class
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="EngineFlatTreeDataGridSource{TModel}"/> class
        /// with the specified items.
        /// </summary>
        /// <param name="items">The items to display.</param>
        public EngineFlatTreeDataGridSource(IEnumerable<TModel> items)
        {
            _items     = items;
            _itemsView = TreeDataGridItemsSourceView<TModel>.GetOrCreate(items);
            Columns    = [];
        }

        #endregion

        #region Fields

        private IEnumerable<TModel>                 _items;

        private TreeDataGridItemsSourceView<TModel> _itemsView;

        private AnonymousSortableRows<TModel>?      _rows;

        private IComparer<TModel>?                  _comparer;

        private ITreeDataGridSelection?             _selection;

        private bool                                _isSelectionSet;

        #endregion

        #region Properties

        /// <inheritdoc cref="ITreeDataGridSource.Columns"/>
        public ColumnList<TModel> Columns { get; }

        /// <inheritdoc/>
        public IRows Rows => _rows ??= CreateRows();

        /// <inheritdoc/>
        public IEnumerable<TModel> Items
        {
            get => _items;
            set
            {
                if (_items != value)
                {
                    _items     = value;
                    _itemsView = TreeDataGridItemsSourceView<TModel>.GetOrCreate(value);
                    _rows?.SetItems(_itemsView);

                    if (_selection is object)
                    {
                        _selection.Source = value;
                    }

                    RaisePropertyChanged();
                }
            }
        }

        /// <inheritdoc/>
        public ITreeDataGridSelection? Selection
        {
            get
            {
                if (_selection == null && !_isSelectionSet)
                {
                    _selection = new EngineTreeDataGridRowSelectionModel<TModel>(this);
                }

                return _selection;
            }
            set
            {
                if (_selection != value)
                {
                    if (value?.Source != _items)
                    {
                        throw new InvalidOperationException("Selection source must be set to Items.");
                    }

                    _selection      = value;
                    _isSelectionSet = true;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets the cell selection model for the typed source, if available.
        /// </summary>
        public ITreeDataGridCellSelectionModel<TModel>? CellSelection => Selection as ITreeDataGridCellSelectionModel<TModel>;

        /// <summary>
        /// Gets the row selection model for the typed source, if available.
        /// </summary>
        public ITreeDataGridRowSelectionModel<TModel>? RowSelection => Selection as ITreeDataGridRowSelectionModel<TModel>;

        /// <summary>
        /// Gets a value indicating whether the source is hierarchical (always false for flat source).
        /// </summary>
        public bool IsHierarchical => false;

        /// <inheritdoc/>
        public bool IsSorted => _comparer is not null;

        #endregion

        #region Public

        /// <inheritdoc/>
        public void Dispose()
        {
            _rows?.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public bool SortBy(IColumn? column, ListSortDirection direction)
        {
            if (column is IColumn<TModel> typedColumn &&
                Columns.Contains(typedColumn))
            {
                var comparer = typedColumn.GetComparison(direction);

                if (comparer is null)
                {
                    return false;
                }

                Sort(comparer);

                foreach (var c in Columns)
                {
                    c.SortDirection = c == column ? direction : null;
                }

                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public void Sort(Comparison<TModel>? comparison)
        {
            _comparer = comparison is not null ? new FuncComparer<TModel>(comparison!) : null;
            _rows?.Sort(_comparer);
            Sorted?.Invoke();
        }

        /// <inheritdoc/>
        public void Unsort()
        {
            Sort(null);

            foreach (var c in Columns)
            {
                c.SortDirection = null;
            }
        }

        #endregion

        #region Private

        private AnonymousSortableRows<TModel> CreateRows()
        {
            return new AnonymousSortableRows<TModel>(_itemsView, _comparer);
        }

        #endregion

        #region ITreeDataGridSource

        IColumns ITreeDataGridSource.Columns => Columns;

        IEnumerable<object> ITreeDataGridSource.Items => Items;

        void ITreeDataGridSource.DragDropRows(
            ITreeDataGridSource source,
            IEnumerable<IndexPath> indexes,
            IndexPath targetIndex,
            TreeDataGridRowDropPosition position,
            DragDropEffects effects)
        {
            if (effects != DragDropEffects.Move)
            {
                throw new NotSupportedException("Only move is currently supported for drag/drop.");
            }

            if (position == TreeDataGridRowDropPosition.Inside)
            {
                throw new ArgumentException("Invalid drop position.", nameof(position));
            }

            if (indexes.Any(x => x.Count != 1))
            {
                throw new ArgumentException("Invalid source index.", nameof(indexes));
            }

            if (targetIndex.Count != 1)
            {
                throw new ArgumentException("Invalid target index.", nameof(targetIndex));
            }

            if (_items is not IList<TModel> items)
            {
                throw new InvalidOperationException("Items does not implement IList<T>.");
            }

            if (position == TreeDataGridRowDropPosition.None)
            {
                return;
            }

            var ti = targetIndex[0];

            if (position == TreeDataGridRowDropPosition.After)
            {
                ++ti;
            }

            var sourceItems = new List<TModel>();

            foreach (var src in indexes.OrderByDescending(x => x))
            {
                var i = src[0];

                sourceItems.Add(items[i]);
                items.RemoveAt(i);

                if (i < ti)
                {
                    --ti;
                }
            }

            for (var si = sourceItems.Count - 1; si >= 0; --si)
            {
                items.Insert(ti++, sourceItems[si]);
            }
        }

        IEnumerable<object> ITreeDataGridSource.GetModelChildren(object model)
        {
            return [];
        }

        #endregion

        #region Events

        /// <inheritdoc/>
        public event Action? Sorted;

        #endregion
    }
}
