using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Selection;
using Avalonia.Input;
using BlackSharp.UI.Avalonia.TreeDataGrid.Models;
using BlackSharp.UI.Avalonia.TreeDataGrid.Primitives;
using System.Collections;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Selection
{
    internal class EngineTreeDataGridCellSelectionModel<TModel> : ITreeDataGridCellSelectionModel<TModel>,
        IEngineSelectionInteraction
        where TModel : class
    {
        #region Constructor

        public EngineTreeDataGridCellSelectionModel(ITreeDataGridSource<TModel> source)
        {
            _source                           = source;
            _selectedColumns                  = new TreeDataGridColumnSelectionModel(source.Columns);
            _selectedRows                     = new EngineTreeDataGridRowSelectionModel<TModel>(source);
            _selectedColumns.SelectionChanged += OnSelectedColumnsSelectionChanged;
            _selectedRows.SelectionChanged    += OnSelectedRowsSelectionChanged;
            _selectedRows.IndexesChanged       += OnSelectedRowsIndexesChanged;
            ((IEngineSelectionInteraction)_selectedRows).SelectionChanged += OnSelectedRowsViewChanged;
            _selectedIndexes                  = new(_selectedColumns, _selectedRows);
        }

        #endregion

        #region Fields

        private static readonly Point s_InvalidPoint = new(double.NegativeInfinity, double.NegativeInfinity);

        private readonly ITreeDataGridColumnSelectionModel               _selectedColumns;

        private readonly ITreeDataGridRowSelectionModel<TModel>          _selectedRows;

        private readonly SelectedCellIndexes                             _selectedIndexes;

        private readonly ITreeDataGridSource<TModel>                     _source;

        private EventHandler<TreeDataGridCellSelectionChangedEventArgs>? _untypedSelectionChanged;

        private EventHandler?                                            _viewSelectionChanged;

        private Point                                                    _pressedPoint            = s_InvalidPoint;

        private (int x, int y)                                           _rangeAnchor             = (-1, -1);

        private bool                                                     _columnsChanged;

        private bool                                                     _rowsChanged;

        private bool                                                     _rowIndexesChanged;

        #endregion

        #region Properties

        public int Count => _selectedColumns.Count * _selectedRows.Count;

        /// <inheritdoc/>
        public bool SingleSelect
        {
            get => _selectedRows.SingleSelect;
            set => _selectedColumns.SingleSelect = _selectedRows.SingleSelect = value;
        }

        /// <inheritdoc/>
        public CellIndex SelectedIndex
        {
            get => new(_selectedColumns.SelectedIndex, _selectedRows.SelectedIndex);
            set
            {
                var rowIndex = _source.Rows.ModelIndexToRowIndex(value.RowIndex);

                Select(value.ColumnIndex, rowIndex, value.RowIndex);
            }
        }

        /// <inheritdoc/>
        public IReadOnlyList<CellIndex> SelectedIndexes => _selectedIndexes;

        #endregion

        #region Public

        /// <inheritdoc/>
        public void Clear()
        {
            BeginBatchUpdate();

            _selectedColumns.Clear();
            _selectedRows.Clear();

            EndBatchUpdate();
        }

        public bool IsSelected(CellIndex index) => IsSelected(index.ColumnIndex, index.RowIndex);

        /// <summary>
        /// Checks whether the specified cell is selected.
        /// </summary>
        /// <param name="columnIndex">The column index of the cell.</param>
        /// <param name="rowIndex">The row index of the cell.</param>
        public bool IsSelected(int columnIndex, IndexPath rowIndex)
        {
            return _selectedColumns.IsSelected(columnIndex) && _selectedRows.IsSelected(rowIndex);
        }

        /// <inheritdoc/>
        public void SetSelectedRange(CellIndex start, int columnCount, int rowCount)
        {
            SetSelectedRange(
                start.ColumnIndex,
                _source.Rows.ModelIndexToRowIndex(start.RowIndex),
                start.RowIndex,
                columnCount,
                rowCount);
        }

        public void BeginBatchUpdate()
        {
            _selectedColumns.BeginBatchUpdate();
            _selectedRows.BeginBatchUpdate();
        }

        public void EndBatchUpdate()
        {
            _columnsChanged = _rowsChanged = false;
            _selectedColumns.EndBatchUpdate();
            _selectedRows.EndBatchUpdate();

            if (_columnsChanged || _rowsChanged)
            {
                var e = new TreeDataGridCellSelectionChangedEventArgs<TModel>();

                _viewSelectionChanged?.Invoke(this, EventArgs.Empty);
                SelectionChanged?.Invoke(this, e);
                _untypedSelectionChanged?.Invoke(this, e);
            }
        }

        #endregion

        #region Private

        private void PointerSelect(TreeDataGrid sender, TreeDataGridCell cell, PointerEventArgs e)
        {
            var point         = e.GetCurrentPoint(sender);
            var isRightButton = point.Properties.PointerUpdateKind is PointerUpdateKind.RightButtonPressed or
                PointerUpdateKind.RightButtonReleased;

            UpdateSelection(
                sender,
                cell.ColumnIndex,
                cell.RowIndex,
                rangeModifier: e.KeyModifiers.HasFlag(KeyModifiers.Shift),
                rightButton: isRightButton);
            e.Handled = true;
        }

        private (int x, int y) GetAnchor()
        {
            var anchorModelIndex = _selectedRows.AnchorIndex;

            return (_selectedColumns.AnchorIndex, _source.Rows.ModelIndexToRowIndex(anchorModelIndex));
        }

        private bool IsSelected(int columnIndex, int rowIndex)
        {
            var modelIndex = _source.Rows.RowIndexToModelIndex(rowIndex);

            return _selectedColumns.IsSelected(columnIndex) && _selectedRows.IsSelected(modelIndex);
        }

        private void Select(int columnIndex, int rowIndex)
        {
            var modelIndex = _source.Rows.RowIndexToModelIndex(rowIndex);

            Select(columnIndex, rowIndex, modelIndex);
        }

        private void Select(int columnIndex, int rowIndex, IndexPath modelndex)
        {
            BeginBatchUpdate();
            _selectedColumns.SelectedIndex = columnIndex;
            _selectedRows.SelectedIndex    = modelndex;
            _rangeAnchor                   = (columnIndex, rowIndex);
            EndBatchUpdate();
        }

        private void SelectFromAnchorTo(int columnIndex, int rowIndex)
        {
            var anchorColumnIndex = _selectedColumns.AnchorIndex;
            var anchorRowIndex    = _source.Rows.ModelIndexToRowIndex(_selectedRows.AnchorIndex);

            SetSelectedRange(
                anchorColumnIndex,
                anchorRowIndex,
                _selectedRows.AnchorIndex,
                (columnIndex - anchorColumnIndex) + 1,
                (rowIndex - anchorRowIndex) + 1);
        }

        private void SetSelectedRange(
            int columnIndex,
            int rowIndex,
            IndexPath modelIndex,
            int columnCount,
            int rowCount)
        {
            var endColumnIndex = columnIndex + columnCount - 1;
            var endRowIndex    = rowIndex + rowCount - 1;

            BeginBatchUpdate();

            _selectedColumns.SelectedIndex = columnIndex;
            _selectedColumns.SelectRange(columnIndex, endColumnIndex);
            _selectedRows.SelectedIndex = rowIndex;

            for (var i = Math.Min(rowIndex, endRowIndex); i <= Math.Max(rowIndex, endRowIndex); ++i)
            {
                _selectedRows.Select(_source.Rows.RowIndexToModelIndex(i));
            }

            _selectedRows.AnchorIndex = modelIndex;
            _rangeAnchor              = (endColumnIndex, endRowIndex);

            EndBatchUpdate();
        }

        private void UpdateSelection(
            TreeDataGrid treeDataGrid,
            int columnIndex,
            int rowIndex,
            bool rangeModifier = false,
            bool rightButton   = false)
        {
            var modelIndex = _source.Rows.RowIndexToModelIndex(rowIndex);

            if (modelIndex == default)
            {
                return;
            }

            var multi = !SingleSelect;
            var range = multi && rangeModifier;

            if (rightButton)
            {
                if (IsSelected(columnIndex, modelIndex) == false && !treeDataGrid.QueryCancelSelection())
                {
                    Select(columnIndex, rowIndex, modelIndex);
                }
            }
            else if (range)
            {
                if (!treeDataGrid.QueryCancelSelection())
                {
                    SelectFromAnchorTo(columnIndex, rowIndex);
                }
            }
            else if (_selectedColumns.SelectedIndex != columnIndex ||
                _selectedRows.SelectedIndex != modelIndex ||
                Count > 1)
            {
                if (!treeDataGrid.QueryCancelSelection())
                {
                    Select(columnIndex, rowIndex, modelIndex);
                }
            }
        }

        private void OnSelectedColumnsSelectionChanged(object? sender, SelectionModelSelectionChangedEventArgs e)
        {
            _columnsChanged = true;
        }

        private void OnSelectedRowsIndexesChanged(object? sender, TreeSelectionModelIndexesChangedEventArgs e)
        {
            _rowIndexesChanged = true;
            IndexesChanged?.Invoke(this, e);
        }

        private void OnSelectedRowsViewChanged(object? sender, EventArgs e)
        {
            if (_rowIndexesChanged)
            {
                _rowIndexesChanged = false;
                _rangeAnchor       = GetAnchor();
                _viewSelectionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void OnSelectedRowsSelectionChanged(object? sender, TreeSelectionModelSelectionChangedEventArgs<TModel> e)
        {
            _rowsChanged = true;
        }

        #endregion

        #region IEngineSelectionInteraction

        bool IEngineSelectionInteraction.IsCellSelected(int columnIndex, int rowIndex)
        {
            return IsSelected(columnIndex, rowIndex);
        }

        void IEngineSelectionInteraction.OnKeyDown(TreeDataGrid sender, KeyEventArgs e)
        {
            var direction = e.Key.ToNavigationDirection();
            var shift     = e.KeyModifiers.HasFlag(KeyModifiers.Shift);

            if (sender.RowsPresenter is null ||
                sender.CoreColumns is null ||
                sender.CoreRows is null ||
                e.Handled || !direction.HasValue)
            {
                return;
            }

            var (x, y) = direction switch
            {
                NavigationDirection.Up => (0, -1),
                NavigationDirection.Down => (0, 1),
                NavigationDirection.Left => (-1, 0),
                NavigationDirection.Right => (1, 0),
                _ => (0, 0)
            };

            var anchor = shift ? _rangeAnchor : GetAnchor();

            var columnIndex = Math.Clamp(anchor.x + x, 0, sender.CoreColumns.Count - 1);
            var rowIndex    = Math.Clamp(anchor.y + y, 0, sender.CoreRows.Count - 1);

            if (!shift)
            {
                Select(columnIndex, rowIndex);
            }
            else
            {
                SelectFromAnchorTo(columnIndex, rowIndex);
            }

            sender.ColumnHeadersPresenter?.BringIntoView(columnIndex);
            sender.RowsPresenter?.BringIntoView(
                rowIndex,
                sender.ColumnHeadersPresenter?.TryGetElement(columnIndex)?.Bounds);

            if (sender.TryGetCell(columnIndex, rowIndex) is { Focusable: true } targetCell)
            {
                targetCell.Focus();
            }
        }

        void IEngineSelectionInteraction.OnPointerPressed(TreeDataGrid sender, PointerPressedEventArgs e)
        {
            // Select a cell on pointer pressed if:
            //
            // - It's a mouse click, not touch: we don't want to select on touch scroll gesture start
            // - It's a pen secondary button press, we don't want to select on primary button scroll gesture start
            // - The cell isn't already selected: we don't want to deselect an existing multiple selection
            //   if the user is trying to drag multiple cells
            //
            // Otherwise select on pointer release.
            var pointerSupportSelectionOnPress = e.Pointer.Type switch
            {
                PointerType.Mouse => true,
                PointerType.Pen => e.GetCurrentPoint(null).Properties.IsRightButtonPressed,
                _ => false
            };

            if (!e.Handled &&
                pointerSupportSelectionOnPress &&
                e.Source is Control source &&
                sender.TryGetCell(source, out var cell) &&
                !IsSelected(cell.ColumnIndex, cell.RowIndex))
            {
                PointerSelect(sender, cell, e);
            }
            else
            {
                _pressedPoint = e.GetPosition(sender);
            }
        }

        void IEngineSelectionInteraction.OnPointerReleased(TreeDataGrid sender, PointerReleasedEventArgs e)
        {
            if (!e.Handled &&
                _pressedPoint != s_InvalidPoint &&
                e.Source is Control source &&
                sender.TryGetCell(source, out var cell))
            {
                var p = e.GetPosition(sender);

                if (Math.Abs(p.X - _pressedPoint.X) <= 3 || Math.Abs(p.Y - _pressedPoint.Y) <= 3)
                {
                    PointerSelect(sender, cell, e);
                }
            }
        }

        bool IEngineSelectionInteraction.IsRowSelected(IRow rowModel) => false;

        bool IEngineSelectionInteraction.IsRowSelected(int rowIndex) => false;

        void IEngineSelectionInteraction.OnPreviewKeyDown(TreeDataGrid sender, KeyEventArgs e)
        {
        }

        void IEngineSelectionInteraction.OnKeyUp(TreeDataGrid sender, KeyEventArgs e)
        {
        }

        void IEngineSelectionInteraction.OnTextInput(TreeDataGrid sender, TextInputEventArgs e)
        {
        }

        void IEngineSelectionInteraction.OnPointerMoved(TreeDataGrid sender, PointerEventArgs e)
        {
        }

        #endregion

        #region ITreeDataGridSelection

        IEnumerable? ITreeDataGridSelection.Source
        {
            get => ((ITreeDataGridSelection)_selectedRows).Source;
            set => ((ITreeDataGridSelection)_selectedRows).Source = value;
        }

        #endregion

        #region Events

        /// <inheritdoc/>
        public event EventHandler<TreeDataGridCellSelectionChangedEventArgs<TModel>>? SelectionChanged;

        internal event EventHandler<TreeSelectionModelIndexesChangedEventArgs>? IndexesChanged;

        event EventHandler? IEngineSelectionInteraction.SelectionChanged
        {
            add => _viewSelectionChanged += value;
            remove => _viewSelectionChanged -= value;
        }

        event EventHandler<TreeDataGridCellSelectionChangedEventArgs>? ITreeDataGridCellSelectionModel.SelectionChanged
        {
            add => _untypedSelectionChanged += value;
            remove => _untypedSelectionChanged -= value;
        }

        #endregion
    }
}
