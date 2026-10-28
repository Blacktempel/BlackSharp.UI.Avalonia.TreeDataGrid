using Avalonia.Input;
using BlackSharp.UI.Avalonia.TreeDataGrid.Models;
using System.Collections;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Selection
{
    public class TreeDataGridRowSelectionModel<TModel> : TreeDataGridSelectionModel,
        ITreeDataGridRowSelection,
        ITreeDataGridSelectionInteraction
        where TModel : class
    {
        #region Constructor

        public TreeDataGridRowSelectionModel(TreeDataGridSource<TModel> source)
        {
            _source                = source;
            _core                  = new EngineTreeDataGridRowSelectionModel<TModel>(source);
            _core.SelectionChanged += OnSelectionChanged;
            _core.PropertyChanged  += (s, e) => RaisePropertyChanged(e);
            _core.IndexesChanged   += (s, e) => RaiseIndexesChanged(new TreeDataGridIndexesChangedEventArgs(e.ParentIndex, e.StartIndex, e.EndIndex, e.Delta));
            _core.SourceReset      += (s, e) => RaiseSourceReset(new TreeDataGridItemsResetEventArgs(e.ParentIndex));
        }

        #endregion

        #region Fields

        private readonly TreeDataGridSource<TModel>                  _source;

        private readonly EngineTreeDataGridRowSelectionModel<TModel> _core;

        #endregion

        #region Properties

        public int Count => _core.Count;

        public override bool SingleSelect
        {
            get => _core.SingleSelect;
            set => _core.SingleSelect = value;
        }

        public override bool IsCellSelectionModel => false;

        public IndexPath SelectedIndex
        {
            get => _core.SelectedIndex;
            set => _core.SelectedIndex = value;
        }

        public IReadOnlyList<IndexPath> SelectedIndexes => _core.SelectedIndexes;

        public TModel? SelectedItem => _core.SelectedItem;

        public IReadOnlyList<TModel?> SelectedItems => _core.SelectedItems;

        public IndexPath AnchorIndex
        {
            get => _core.AnchorIndex;
            set => _core.AnchorIndex = value;
        }

        public IndexPath RangeAnchorIndex
        {
            get => _core.RangeAnchorIndex;
            set => _core.RangeAnchorIndex = value;
        }

        public override IEnumerable? Source => ((ITreeDataGridSelection)_core).Source;

        public override IndexPath SelectedRowIndex
        {
            get => SelectedIndex;
            set => SelectedIndex = value;
        }

        public override object? SelectedRowItem => SelectedItem;

        internal override ITreeDataGridSelection Core => _core;

        internal override TreeDataGridSource Owner => _source;

        #endregion

        #region Public

        public BatchUpdateOperation BatchUpdate() => new(_core.BatchUpdate());

        public void BeginBatchUpdate() => _core.BeginBatchUpdate();

        public void EndBatchUpdate() => _core.EndBatchUpdate();

        public void Clear() => _core.Clear();

        public void Deselect(IndexPath index) => _core.Deselect(index);

        public bool IsSelected(IndexPath index) => _core.IsSelected(index);

        public void Select(IndexPath index) => _core.Select(index);

        public void SelectAll()
        {
            if (SingleSelect)
            {
                throw new InvalidOperationException("SelectAll requires multiple selection.");
            }

            using var operation = BatchUpdate();

            for (var i = 0; i < _source.Rows.Count; ++i)
            {
                Select(_source.RowIndexToModelIndex(i));
            }
        }

        public void SelectRange(IndexPath start, IndexPath end)
        {
            var first = _source.ModelIndexToRowIndex(start);
            var last  = _source.ModelIndexToRowIndex(end);

            if (first < 0 || last < 0)
            {
                return;
            }

            using var operation = BatchUpdate();

            for (var i = Math.Min(first, last); i <= Math.Max(first, last); ++i)
            {
                Select(_source.RowIndexToModelIndex(i));
            }
        }

        #endregion

        #region Internal

        internal override void SelectAllCore() => SelectAll();

        internal override void ClearCore() => Clear();

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

        private void OnSelectionChanged(object? sender, TreeSelectionModelSelectionChangedEventArgs<TModel> e)
        {
            var args = new TreeDataGridSelectionChangedEventArgs<TModel>(e.DeselectedIndexes, e.SelectedIndexes, e.DeselectedItems, e.SelectedItems);

            RaiseUntypedSelectionChanged(args);

            SelectionChanged?.Invoke(this, args);

            RaisePropertyChanged(nameof(SelectedRowIndex));
            RaisePropertyChanged(nameof(SelectedRowItem));
        }

        #endregion

        #region ITreeDataGridRowSelection

        IReadOnlyList<object?> ITreeDataGridRowSelection.SelectedItems => SelectedItems;

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

        public struct BatchUpdateOperation : IDisposable
        {
            #region Constructor

            internal BatchUpdateOperation(IDisposable operation) => _operation = operation;

            #endregion

            #region Fields

            private IDisposable? _operation;

            #endregion

            #region Public

            public void Dispose()
            {
                _operation?.Dispose();
                _operation = null;
            }

            #endregion
        }

        #endregion
    }
}
