using Avalonia.Input;
using BlackSharp.UI.Avalonia.TreeDataGrid.Models;
using System.Collections;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Selection
{
    public class TreeDataGridCellSelectionModel<TModel> : TreeDataGridSelectionModel,
        ITreeDataGridCellSelection,
        ITreeDataGridSelectionInteraction
        where TModel : class
    {
        #region Constructor

        public TreeDataGridCellSelectionModel(TreeDataGridSource<TModel> source)
        {
            _source                = source;
            _core                  = new EngineTreeDataGridCellSelectionModel<TModel>(source);
            _core.SelectionChanged += OnSelectionChanged;
            _core.IndexesChanged   += OnIndexesChanged;
        }

        #endregion

        #region Fields

        private readonly TreeDataGridSource<TModel>                   _source;

        private readonly EngineTreeDataGridCellSelectionModel<TModel> _core;

        private CellIndex[]                                           _previousIndexes = [];

        private TModel?[]                                             _previousItems   = [];

        #endregion

        #region Properties

        public int Count => _core.Count;

        public override bool SingleSelect
        {
            get => _core.SingleSelect;
            set => _core.SingleSelect = value;
        }

        public override bool IsCellSelectionModel => true;

        public CellIndex SelectedIndex
        {
            get => _core.SelectedIndex;
            set => _core.SelectedIndex = value;
        }

        public IReadOnlyList<CellIndex> SelectedIndexes => _core.SelectedIndexes;

        public override IEnumerable? Source => ((ITreeDataGridSelection)_core).Source;

        public override IndexPath SelectedRowIndex
        {
            get => SelectedIndex.RowIndex;
            set => SelectedIndex = new CellIndex(Math.Max(0, SelectedIndex.ColumnIndex), value);
        }

        public override object? SelectedRowItem
        {
            get
            {
                if (SelectedRowIndex.Count == 0 || _source.IsDisposed)
                {
                    return null;
                }

                var index = _source.ModelIndexToRowIndex(SelectedRowIndex);

                return index >= 0 ? _source.Rows[index].Model : null;
            }
        }

        internal override ITreeDataGridSelection Core => _core;

        internal override TreeDataGridSource Owner => _source;

        #endregion

        #region Public

        public void BeginBatchUpdate() => _core.BeginBatchUpdate();

        public void EndBatchUpdate() => _core.EndBatchUpdate();

        public IDisposable BatchUpdate()
        {
            BeginBatchUpdate();

            return new BatchUpdateOperation(this);
        }

        public void Clear() => _core.Clear();

        public bool IsSelected(CellIndex index) => _core.IsSelected(index);

        public bool IsSelected(int columnIndex, IndexPath rowIndex) => _core.IsSelected(columnIndex, rowIndex);

        public void SetSelectedRange(CellIndex start, int columnCount, int rowCount) => _core.SetSelectedRange(start, columnCount, rowCount);

        public void SelectAll()
        {
            if (SingleSelect)
            {
                throw new InvalidOperationException("SelectAll requires multiple selection.");
            }

            if (_source.Rows.Count > 0 && _source.Columns.Count > 0)
            {
                SetSelectedRange(new CellIndex(0, _source.RowIndexToModelIndex(0)), _source.Columns.Count, _source.Rows.Count);
            }
        }

        #endregion

        #region Internal

        internal override void SelectAllCore() => SelectAll();

        internal override void ClearCore() => Clear();

        internal override void SelectRowCore(IndexPath index)
        {
            if (SingleSelect)
            {
                SelectedIndex = new CellIndex(0, index);
            }
            else
            {
                SetSelectedRange(new CellIndex(0, index), _source.Columns.Count, 1);
            }
        }

        protected internal override void SetSource(IEnumerable? source)
        {
            if (!ReferenceEquals(Source, source))
            {
                Clear();
                ((ITreeDataGridSelection)_core).Source = source;
            }
        }

        #endregion

        #region Private

        private void OnIndexesChanged(object? sender, TreeSelectionModelIndexesChangedEventArgs e)
        {
            _previousIndexes = SelectedIndexes.ToArray();

            RaiseIndexesChanged(new TreeDataGridIndexesChangedEventArgs(e.ParentIndex, e.StartIndex, e.EndIndex, e.Delta));
            RaisePropertyChanged(nameof(SelectedIndex));
            RaisePropertyChanged(nameof(SelectedIndexes));
            RaisePropertyChanged(nameof(SelectedRowIndex));
        }

        private void OnSelectionChanged(object? sender, TreeDataGridCellSelectionChangedEventArgs<TModel> e)
        {
            var indexes = SelectedIndexes.ToArray();
            var added   = indexes.Except(_previousIndexes).ToArray();
            var removed = _previousIndexes.Except(indexes).ToArray();
            var paths   = indexes.Select(x => x.RowIndex).Distinct().ToArray();
            var items   = paths.Select(x => _source.ModelIndexToRowIndex(x)).Select(x => x >= 0 ? (TModel?)_source.Rows[x].Model : null).ToArray();

            var args    = new TreeDataGridSelectionChangedEventArgs<TModel>(
                _previousIndexes.Select(x => x.RowIndex).Except(paths).Distinct().ToArray(),
                paths.Except(_previousIndexes.Select(x => x.RowIndex)).ToArray(),
                _previousItems.Except(items).ToArray(), items.Except(_previousItems).ToArray(), removed, added);

            _previousIndexes = indexes;
            _previousItems   = items;

            RaiseUntypedSelectionChanged(args);

            SelectionChanged?.Invoke(this, args);

            RaisePropertyChanged(nameof(SelectedIndex));
            RaisePropertyChanged(nameof(SelectedIndexes));
            RaisePropertyChanged(nameof(SelectedRowIndex));
            RaisePropertyChanged(nameof(SelectedRowItem));
        }

        #endregion

        #region ITreeDataGridSelectionInteraction

        bool ITreeDataGridSelectionInteraction.IsCellSelected(int columnIndex, int rowIndex) =>
            ((IEngineSelectionInteraction)_core).IsCellSelected(columnIndex, rowIndex);

        bool ITreeDataGridSelectionInteraction.IsRowSelected(ITreeDataGridRowModel row) =>
            ((IEngineSelectionInteraction)_core).IsRowSelected(_source.ModelIndexToRowIndex(row.ModelIndexPath));

        bool ITreeDataGridSelectionInteraction.IsRowSelected(int rowIndex) =>
            ((IEngineSelectionInteraction)_core).IsRowSelected(rowIndex);

        void ITreeDataGridSelectionInteraction.OnKeyDown(TreeDataGrid sender, KeyEventArgs e) =>
            ((IEngineSelectionInteraction)_core).OnKeyDown(sender, e);

        void ITreeDataGridSelectionInteraction.OnPreviewKeyDown(TreeDataGrid sender, KeyEventArgs e) =>
            ((IEngineSelectionInteraction)_core).OnPreviewKeyDown(sender, e);

        void ITreeDataGridSelectionInteraction.OnKeyUp(TreeDataGrid sender, KeyEventArgs e) =>
            ((IEngineSelectionInteraction)_core).OnKeyUp(sender, e);

        void ITreeDataGridSelectionInteraction.OnTextInput(TreeDataGrid sender, TextInputEventArgs e) =>
            ((IEngineSelectionInteraction)_core).OnTextInput(sender, e);

        void ITreeDataGridSelectionInteraction.OnPointerPressed(TreeDataGrid sender, PointerPressedEventArgs e) =>
            ((IEngineSelectionInteraction)_core).OnPointerPressed(sender, e);

        void ITreeDataGridSelectionInteraction.OnPointerMoved(TreeDataGrid sender, PointerEventArgs e) =>
            ((IEngineSelectionInteraction)_core).OnPointerMoved(sender, e);

        void ITreeDataGridSelectionInteraction.OnPointerReleased(TreeDataGrid sender, PointerReleasedEventArgs e) =>
            ((IEngineSelectionInteraction)_core).OnPointerReleased(sender, e);

        #endregion

        #region Events

        event EventHandler? ITreeDataGridSelectionInteraction.SelectionChanged
        {
            add => ((IEngineSelectionInteraction)_core).SelectionChanged += value;
            remove => ((IEngineSelectionInteraction)_core).SelectionChanged -= value;
        }

        public new event EventHandler<TreeDataGridSelectionChangedEventArgs<TModel>>? SelectionChanged;

        #endregion

        #region Nested Types

        private sealed class BatchUpdateOperation(TreeDataGridCellSelectionModel<TModel> owner) : IDisposable
        {
            #region Fields

            private TreeDataGridCellSelectionModel<TModel>? _owner = owner;

            #endregion

            #region Public

            public void Dispose()
            {
                _owner?.EndBatchUpdate();
                _owner = null;
            }

            #endregion
        }

        #endregion
    }
}
