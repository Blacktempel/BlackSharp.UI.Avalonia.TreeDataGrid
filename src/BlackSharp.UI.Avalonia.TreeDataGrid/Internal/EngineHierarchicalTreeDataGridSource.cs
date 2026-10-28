using Avalonia.Input;
using BlackSharp.UI.Avalonia.TreeDataGrid.Models;
using BlackSharp.UI.Avalonia.TreeDataGrid.Selection;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace BlackSharp.UI.Avalonia.TreeDataGrid
{
    /// <summary>
    /// A data source for a <see cref="TreeDataGrid"/> which displays a hierarchial tree where each
    /// row may have multiple columns.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    internal class EngineHierarchicalTreeDataGridSource<TModel> : NotifyingBase,
        ITreeDataGridSource<TModel>,
        IDisposable,
        IExpanderRowController<TModel>
        where TModel : class
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="EngineHierarchicalTreeDataGridSource{TModel}"/>
        /// class with a single root item.
        /// </summary>
        /// <param name="item">The root item.</param>
        public EngineHierarchicalTreeDataGridSource(TModel item)
            : this(new[] { item })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EngineHierarchicalTreeDataGridSource{TModel}"/>
        /// class with the specified items.
        /// </summary>
        /// <param name="items">The root items for the tree.</param>
        public EngineHierarchicalTreeDataGridSource(IEnumerable<TModel> items)
        {
            _items                    = items;
            _itemsView                = TreeDataGridItemsSourceView<TModel>.GetOrCreate(items);
            Columns                   = [];
            Columns.CollectionChanged += OnColumnsCollectionChanged;
        }

        #endregion

        #region Fields

        private IEnumerable<TModel>                 _items;

        private TreeDataGridItemsSourceView<TModel> _itemsView;

        private IExpanderColumn<TModel>?            _expanderColumn;

        private HierarchicalRows<TModel>?           _rows;

        private AnonymousSortableRows<TModel>?      _flatRows;

        private Comparison<TModel>?                 _comparison;

        private ITreeDataGridSelection?             _selection;

        private bool                                _isSelectionSet;

        #endregion

        #region Properties

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
                    _flatRows?.SetItems(_itemsView);

                    if (_selection is object)
                    {
                        _selection.Source = value;
                    }
                }
            }
        }

        /// <inheritdoc/>
        public IRows Rows => _expanderColumn is null ?
            _flatRows ??= new AnonymousSortableRows<TModel>(_itemsView, _comparison is null ? null : new FuncComparer<TModel>((x, y) => _comparison(x!, y!))) :
            GetOrCreateRows();

        /// <inheritdoc cref="ITreeDataGridSource.Columns"/>
        public ColumnList<TModel> Columns { get; }

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
        /// Gets the typed cell selection model, if available.
        /// </summary>
        public ITreeDataGridCellSelectionModel<TModel>? CellSelection => Selection as ITreeDataGridCellSelectionModel<TModel>;

        /// <summary>
        /// Gets the typed row selection model, if available.
        /// </summary>
        public ITreeDataGridRowSelectionModel<TModel>? RowSelection => Selection as ITreeDataGridRowSelectionModel<TModel>;

        /// <summary>
        /// Gets a value indicating whether the source is hierarchical (always true for this source).
        /// </summary>
        public bool IsHierarchical => true;

        /// <inheritdoc/>
        public bool IsSorted => _comparison is not null;

        #endregion

        #region Public

        /// <inheritdoc/>
        public void Dispose()
        {
            _rows?.Dispose();
            _flatRows?.Dispose();

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Collapses the row at the specified index.
        /// </summary>
        /// <param name="index">The index path of the row to collapse.</param>
        public void Collapse(IndexPath index) => GetOrCreateRows().Collapse(index);

        /// <summary>
        /// Collapses all rows.
        /// </summary>
        public void CollapseAll() => GetOrCreateRows().ExpandCollapseRecursive(_ => false);

        /// <summary>
        /// Expands the row at the specified index.
        /// </summary>
        /// <param name="index">The index path of the row to expand.</param>
        public void Expand(IndexPath index) => GetOrCreateRows().Expand(index);

        /// <summary>
        /// Expands all rows.
        /// </summary>
        public void ExpandAll() => GetOrCreateRows().ExpandCollapseRecursive(_ => true);

        /// <summary>
        /// Expands or collapses rows according to a condition.
        /// </summary>
        /// <param name="predicate">
        /// A function which is passed a model instance and returns a boolean value representing
        /// the desired expanded state of the row.
        /// </param>
        public void ExpandCollapseRecursive(Func<TModel, bool> predicate)
        {
            GetOrCreateRows().ExpandCollapseRecursive(predicate);
        }

        /// <summary>
        /// Expands or collapses rows according to a condition, starting from the specified row.
        /// </summary>
        /// <param name="row">
        /// The row from which to start expanding or collapsing.
        /// </param>
        /// <param name="predicate">
        /// A function which is passed a model instance and returns a boolean value representing
        /// the desired expanded state of the row.
        /// </param>
        public void ExpandCollapseRecursive(HierarchicalRow<TModel> row, Func<TModel, bool> predicate)
        {
            GetOrCreateRows().ExpandCollapseRecursive(predicate, row);
        }

        /// <summary>
        /// Tries to get the model at the given tree path.
        /// </summary>
        /// <param name="index">Hierarchical index path</param>
        /// <param name="result">The found model, or null</param>
        /// <returns>true if model was found</returns>
        /// <exception cref="InvalidOperationException">No expander column set</exception>
        public bool TryGetModelAt(IndexPath index, [NotNullWhen(true)] out TModel? result)
        {
            var items = (IEnumerable<TModel>?)Items;
            var count = index.Count;

            for (var depth = 0; depth < count; ++depth)
            {
                var i = index[depth];

                if (i >= 0 && i < items?.Count())
                {
                    var e = items.ElementAt(i)!;

                    if (depth < count - 1)
                    {
                        items = _expanderColumn?.GetChildModels(e);
                    }
                    else
                    {
                        result = e;

                        return true;
                    }
                }
                else
                {
                    break;
                }
            }

            result = default;

            return false;
        }

        /// <inheritdoc/>
        public void Sort(Comparison<TModel>? comparison)
        {
            _comparison = comparison;

            _rows?.Sort(_comparison);
            _flatRows?.Sort(_comparison is null ? null : new FuncComparer<TModel>((x, y) => _comparison(x!, y!)));

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

        /// <inheritdoc/>
        public bool SortBy(IColumn? column, ListSortDirection direction)
        {
            if (column is IColumn<TModel> columnBase &&
                Columns.Contains(columnBase) &&
                columnBase.GetComparison(direction) is Comparison<TModel> comparison)
            {
                Sort(comparison);

                foreach (var c in Columns)
                {
                    c.SortDirection = c == column ? direction : null;
                }

                return true;
            }

            return false;
        }

        #endregion

        #region Internal

        internal IEnumerable<TModel>? GetModelChildren(TModel model)
        {
            return _expanderColumn?.GetChildModels(model);
        }

        internal int GetRowIndex(in IndexPath index, int fromRowIndex = 0)
        {
            var result = -1;

            _rows?.TryGetRowIndex(index, out result, fromRowIndex);

            return result;
        }

        internal void RebuildRows()
        {
            _rows?.Dispose();
            _flatRows?.Dispose();

            _rows     = null;
            _flatRows = null;

            RaisePropertyChanged(nameof(Rows));
        }

        #endregion

        #region Private

        private HierarchicalRows<TModel> GetOrCreateRows()
        {
            if (_rows is null)
            {
                if (Columns.Count == 0)
                {
                    throw new InvalidOperationException("No columns defined.");
                }

                if (_expanderColumn is null)
                {
                    throw new InvalidOperationException("No expander column defined.");
                }

                _rows = new HierarchicalRows<TModel>(this, _itemsView, _expanderColumn, _comparison);
            }

            return _rows;
        }

        private void OnColumnsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            var expander = Columns.OfType<IExpanderColumn<TModel>>().SingleOrDefault();

            if (!ReferenceEquals(expander, _expanderColumn))
            {
                _expanderColumn = expander;
                RebuildRows();
            }
        }

        #endregion

        #region IExpanderRowController<TModel>

        void IExpanderRowController<TModel>.OnBeginExpandCollapse(IExpanderRow<TModel> row)
        {
            if (row is HierarchicalRow<TModel> r)
            {
                if (!row.IsExpanded)
                {
                    RowExpanding?.Invoke(this, RowEventArgs.Create(r));
                }
                else
                {
                    RowCollapsing?.Invoke(this, RowEventArgs.Create(r));
                }
            }
        }

        void IExpanderRowController<TModel>.OnEndExpandCollapse(IExpanderRow<TModel> row)
        {
            if (row is HierarchicalRow<TModel> r)
            {
                if (row.IsExpanded)
                {
                    RowExpanded?.Invoke(this, RowEventArgs.Create(r));
                }
                else
                {
                    RowCollapsed?.Invoke(this, RowEventArgs.Create(r));
                }
            }
        }

        void IExpanderRowController<TModel>.OnChildCollectionChanged(
            IExpanderRow<TModel> row,
            NotifyCollectionChangedEventArgs e)
        {
        }

        #endregion

        #region ITreeDataGridSource

        IEnumerable<object> ITreeDataGridSource.Items => Items;

        IColumns ITreeDataGridSource.Columns => Columns;

        IEnumerable<object>? ITreeDataGridSource.GetModelChildren(object model)
        {
            return GetModelChildren((TModel)model);
        }

        void ITreeDataGridSource.DragDropRows(
            ITreeDataGridSource source,
            IEnumerable<IndexPath> indexes,
            IndexPath targetIndex,
            TreeDataGridRowDropPosition position,
            DragDropEffects effects)
        {
            IList<TModel> GetItems(IndexPath path)
            {
                IEnumerable<TModel>? children;

                if (path.Count == 0)
                {
                    children = _items;
                }
                else if (TryGetModelAt(path, out var parent))
                {
                    children = GetModelChildren(parent);
                }
                else
                {
                    throw new IndexOutOfRangeException();
                }

                if (children is null)
                {
                    throw new InvalidOperationException("The requested drop target has no children.");
                }

                return children as IList<TModel> ??
                    throw new InvalidOperationException("Items does not implement IList<T>.");
            }

            if (effects != DragDropEffects.Move)
            {
                throw new NotSupportedException("Only move is currently supported for drag/drop.");
            }

            IList<TModel> targetItems;
            int ti;

            if (position == TreeDataGridRowDropPosition.Inside)
            {
                targetItems = GetItems(targetIndex);
                ti          = targetItems.Count;
            }
            else
            {
                targetItems = GetItems(targetIndex[..^1]);
                ti          = targetIndex[^1];
            }

            if (position == TreeDataGridRowDropPosition.After)
            {
                ++ti;
            }

            var sourceItems = new List<TModel>();

            foreach (var g in indexes.GroupBy(x => x[..^1]))
            {
                var items = GetItems(g.Key);

                foreach (var i in g.Select(x => x[^1]).OrderByDescending(x => x))
                {
                    sourceItems.Add(items[i]);

                    if (items == targetItems && i < ti)
                    {
                        --ti;
                    }

                    items.RemoveAt(i);
                }
            }

            for (var si = sourceItems.Count - 1; si >= 0; --si)
            {
                targetItems.Insert(ti++, sourceItems[si]);
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when a row is about to expand.
        /// </summary>
        public event EventHandler<RowEventArgs<HierarchicalRow<TModel>>>? RowExpanding;

        /// <summary>
        /// Occurs when a row has expanded.
        /// </summary>
        public event EventHandler<RowEventArgs<HierarchicalRow<TModel>>>? RowExpanded;

        /// <summary>
        /// Occurs when a row is about to collapse.
        /// </summary>
        public event EventHandler<RowEventArgs<HierarchicalRow<TModel>>>? RowCollapsing;

        /// <summary>
        /// Occurs when a row has collapsed.
        /// </summary>
        public event EventHandler<RowEventArgs<HierarchicalRow<TModel>>>? RowCollapsed;

        /// <inheritdoc/>
        public event Action? Sorted;

        #endregion
    }
}
