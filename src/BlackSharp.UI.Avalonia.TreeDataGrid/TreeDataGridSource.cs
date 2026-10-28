using Avalonia.Input;
using BlackSharp.UI.Avalonia.TreeDataGrid.Models;
using BlackSharp.UI.Avalonia.TreeDataGrid.Selection;
using System.ComponentModel;

namespace BlackSharp.UI.Avalonia.TreeDataGrid
{
    public abstract class TreeDataGridSource : NotifyingBase, ITreeDataGridSource
    {
        #region Fields

        private bool _autoDragDropRows      = false;

        private bool _canUserReorderColumns = false;

        private bool _canUserResizeColumns  = true;

        private bool _canUserSortColumns    = true;

        private bool _allowTriStateSorting  = false;

        private int  _frozenColumnCount     = 0;

        private bool _showColumnHeaders     = true;

        #endregion

        #region Properties

        public bool AutoDragDropRows
        {
            get => _autoDragDropRows;
            set => RaiseAndSetIfChanged(ref _autoDragDropRows, value);
        }

        public bool CanUserReorderColumns
        {
            get => _canUserReorderColumns;
            set => RaiseAndSetIfChanged(ref _canUserReorderColumns, value);
        }

        public bool CanUserResizeColumns
        {
            get => _canUserResizeColumns;
            set => RaiseAndSetIfChanged(ref _canUserResizeColumns, value);
        }

        public bool CanUserSortColumns
        {
            get => _canUserSortColumns;
            set => RaiseAndSetIfChanged(ref _canUserSortColumns, value);
        }

        public bool AllowTriStateSorting
        {
            get => _allowTriStateSorting;
            set => RaiseAndSetIfChanged(ref _allowTriStateSorting, value);
        }

        public int FrozenColumnCount
        {
            get => _frozenColumnCount;
            set => RaiseAndSetIfChanged(ref _frozenColumnCount, value);
        }

        public bool ShowColumnHeaders
        {
            get => _showColumnHeaders;
            set => RaiseAndSetIfChanged(ref _showColumnHeaders, value);
        }

        public abstract TreeDataGridColumns Columns { get; }

        public abstract TreeDataGridRows Rows { get; }

        public abstract TreeDataGridSelectionModel? Selection { get; set; }

        public abstract bool IsFiltered { get; }

        public abstract bool IsHierarchical { get; }

        public abstract bool IsSorted { get; }

        public IEnumerable<object> Items => GetItems();

        internal bool IsDisposed
        {
            get; private protected set;
        }

        internal abstract ITreeDataGridSource Engine { get; }

        internal abstract IColumns EngineColumns { get; }

        #endregion

        #region Public

        public abstract void ClearSort(TreeDataGridColumn column);

        public abstract bool SortBy(TreeDataGridColumn column, ListSortDirection direction);

        public int ModelIndexToRowIndex(IndexPath modelIndex) => ((IRows)Rows).ModelIndexToRowIndex(modelIndex);

        public IndexPath RowIndexToModelIndex(int rowIndex) => ((IRows)Rows).RowIndexToModelIndex(rowIndex);

        #endregion

        #region Internal

        internal abstract TreeDataGridSelectionModel CreateSelection(bool cell);

        #endregion

        #region Protected

        protected abstract IEnumerable<object> GetItems();

        protected void RaiseSorted() => Sorted?.Invoke();

        #endregion

        #region ITreeDataGridSource

        IColumns ITreeDataGridSource.Columns => EngineColumns;

        IRows ITreeDataGridSource.Rows => (IRows)Rows;

        ITreeDataGridSelection? ITreeDataGridSource.Selection
        {
            get => Selection?.Core;
            set => throw new NotSupportedException("Use the TreeDataGridSelectionModel API to change selection.");
        }

        bool ITreeDataGridSource.SortBy(IColumn column, ListSortDirection direction)
        {
            var index = Enumerable.Range(0, EngineColumns.Count).FirstOrDefault(x => ReferenceEquals(EngineColumns[x], column), -1);

            return index >= 0 && SortBy(Columns[index], direction);
        }

        void ITreeDataGridSource.Unsort()
        {
            var column = Columns.FirstOrDefault(x => x.SortDirection is not null);

            if (column is not null)
            {
                ClearSort(column);
            }
        }

        IEnumerable<object>? ITreeDataGridSource.GetModelChildren(object model) => Engine.GetModelChildren(model);

        void ITreeDataGridSource.DragDropRows(ITreeDataGridSource source, IEnumerable<IndexPath> indexes,
            IndexPath targetIndex, TreeDataGridRowDropPosition position, DragDropEffects effects) =>
            Engine.DragDropRows(source is TreeDataGridSource modern ? modern.Engine : source,
                indexes, targetIndex, position, effects);

        #endregion

        #region Events

        public event Action? Sorted;

        #endregion
    }
}
