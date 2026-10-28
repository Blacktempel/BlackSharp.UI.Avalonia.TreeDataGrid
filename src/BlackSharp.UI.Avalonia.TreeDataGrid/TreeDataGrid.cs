using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using BlackSharp.UI.Avalonia.TreeDataGrid.Experimental.Data.Core;
using BlackSharp.UI.Avalonia.TreeDataGrid.Models;
using BlackSharp.UI.Avalonia.TreeDataGrid.Primitives;
using BlackSharp.UI.Avalonia.TreeDataGrid.Selection;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace BlackSharp.UI.Avalonia.TreeDataGrid
{
    public partial class TreeDataGrid : TemplatedControl
    {
        #region Constructor

        public TreeDataGrid()
        {
            AddHandler(TreeDataGridColumnHeader.ClickEvent, OnClick);
            AddHandler(KeyDownEvent, OnPreviewKeyDown, RoutingStrategies.Tunnel);
        }

        static TreeDataGrid()
        {
            DragDrop.DragOverEvent.AddClassHandler<TreeDataGrid>((x, e) => x.OnDragOver(e));
            DragDrop.DragLeaveEvent.AddClassHandler<TreeDataGrid>((x, e) => x.OnDragLeave(e));
            DragDrop.DropEvent.AddClassHandler<TreeDataGrid>((x, e) => x.OnDrop(e));
        }

        #endregion

        #region Fields

        public static readonly StyledProperty<bool>                                     AutoDragDropRowsProperty     =
            AvaloniaProperty.Register<TreeDataGrid, bool>(nameof(AutoDragDropRows));

        public static readonly StyledProperty<bool>                                     CanUserResizeColumnsProperty =
            AvaloniaProperty.Register<TreeDataGrid, bool>(nameof(CanUserResizeColumns), true);

        public static readonly StyledProperty<bool>                                     CanUserSortColumnsProperty   =
            AvaloniaProperty.Register<TreeDataGrid, bool>(nameof(CanUserSortColumns), true);

        internal static readonly DirectProperty<TreeDataGrid, IColumns?>                CoreColumnsProperty          =
            AvaloniaProperty.RegisterDirect<TreeDataGrid, IColumns?>(
                nameof(CoreColumns),
                o => o.CoreColumns);

        public static readonly DirectProperty<TreeDataGrid, TreeDataGridElementFactory> ElementFactoryProperty       =
            AvaloniaProperty.RegisterDirect<TreeDataGrid, TreeDataGridElementFactory>(
                nameof(ElementFactory),
                o => o.ElementFactory,
                (o, v) => o.ElementFactory = v);

        /// <summary>
        /// Defines the height of the expand/collapse chevron.
        /// </summary>
        public static readonly StyledProperty<double>                                   ExpanderChevronHeightProperty =
            AvaloniaProperty.Register<TreeDataGrid, double>(nameof(ExpanderChevronHeight), 12,
                validate: value => double.IsFinite(value) && value >= 0);

        /// <summary>
        /// Defines the width of the expand/collapse chevron.
        /// </summary>
        public static readonly StyledProperty<double>                                   ExpanderChevronWidthProperty  =
            AvaloniaProperty.Register<TreeDataGrid, double>(nameof(ExpanderChevronWidth), 12,
                validate: value => double.IsFinite(value) && value >= 0);

        internal static readonly DirectProperty<TreeDataGrid, IRows?>                   CoreRowsProperty             =
            AvaloniaProperty.RegisterDirect<TreeDataGrid, IRows?>(
                nameof(CoreRows),
                o => o.CoreRows,
                (o, v) => o.CoreRows = v);

        public static readonly DirectProperty<TreeDataGrid, IScrollable?>               ScrollProperty               =
            AvaloniaProperty.RegisterDirect<TreeDataGrid, IScrollable?>(
                nameof(Scroll),
                o => o.Scroll);

        public static readonly StyledProperty<bool>                                     ShowColumnHeadersProperty    =
            AvaloniaProperty.Register<TreeDataGrid, bool>(nameof(ShowColumnHeaders), true);

        internal static readonly DirectProperty<TreeDataGrid, ITreeDataGridSource?>     CoreSourceProperty           =
            AvaloniaProperty.RegisterDirect<TreeDataGrid, ITreeDataGridSource?>(
                nameof(CoreSource),
                o => o.CoreSource,
                (o, v) => o.CoreSource = v);

        public static readonly RoutedEvent<TreeDataGridRowDragStartedEventArgs>         RowDragStartedEvent          =
            RoutedEvent.Register<TreeDataGrid, TreeDataGridRowDragStartedEventArgs>(
                nameof(RowDragStarted),
                RoutingStrategies.Bubble);

        public static readonly RoutedEvent<TreeDataGridRowDragEventArgs>                RowDragOverEvent             =
            RoutedEvent.Register<TreeDataGrid, TreeDataGridRowDragEventArgs>(
                nameof(RowDragOver),
                RoutingStrategies.Bubble);

        public static readonly RoutedEvent<TreeDataGridRowDragEventArgs>                RowDropEvent                 =
            RoutedEvent.Register<TreeDataGrid, TreeDataGridRowDragEventArgs>(
                nameof(RowDrop),
                RoutingStrategies.Bubble);

        private const double AutoScrollMargin = 60;

        private const int    AutoScrollSpeed  = 50;

        private TreeDataGridElementFactory?  _elementFactory;

        private ITreeDataGridSource?         _source;

        private IColumns?                    _columns;

        private IRows?                       _rows;

        private IScrollable?                 _scroll;

        private IScrollable?                 _headerScroll;

        private IEngineSelectionInteraction? _selection;

        private TreeDataGridCellEventArgs?   _cellArgs;

        private TreeDataGridRowEventArgs?    _rowArgs;

        private Canvas?                      _dragAdorner;

        private bool                         _hideDragAdorner;

        private IDisposable?                 _hideDragAdornerTimer;

        private DispatcherTimer?             _autoScrollTimer;

        private bool                         _autoScrollDirection;

        #endregion

        #region Properties

        public bool AutoDragDropRows
        {
            get => GetValue(AutoDragDropRowsProperty);
            set => SetValue(AutoDragDropRowsProperty, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the user can resize columns.
        /// </summary>
        public bool CanUserResizeColumns
        {
            get => GetValue(CanUserResizeColumnsProperty);
            set => SetValue(CanUserResizeColumnsProperty, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the user can sort columns via the UI.
        /// </summary>
        public bool CanUserSortColumns
        {
            get => GetValue(CanUserSortColumnsProperty);
            set => SetValue(CanUserSortColumnsProperty, value);
        }

        /// <summary>
        /// Gets the collection of columns displayed by the grid.
        /// </summary>
        internal IColumns? CoreColumns
        {
            get => _columns;
            private set => SetAndRaise(CoreColumnsProperty, ref _columns, value);
        }

        /// <summary>
        /// Gets or sets the element factory used to create UI elements for cells, rows and headers.
        /// </summary>
        public TreeDataGridElementFactory ElementFactory
        {
            get => _elementFactory ??= CreateDefaultElementFactory();
            set
            {
                _ = value ?? throw new ArgumentNullException(nameof(value));
                SetAndRaise(ElementFactoryProperty, ref _elementFactory!, value);
            }
        }

        /// <summary>
        /// Gets or sets the height of the expand/collapse chevron.
        /// </summary>
        public double ExpanderChevronHeight
        {
            get => GetValue(ExpanderChevronHeightProperty);
            set => SetValue(ExpanderChevronHeightProperty, value);
        }

        /// <summary>
        /// Gets or sets the width of the expand/collapse chevron and its tree indentation step.
        /// </summary>
        public double ExpanderChevronWidth
        {
            get => GetValue(ExpanderChevronWidthProperty);
            set => SetValue(ExpanderChevronWidthProperty, value);
        }

        /// <summary>
        /// Gets the rows collection provided by the current source.
        /// </summary>
        internal IRows? CoreRows
        {
            get => _rows;
            private set => SetAndRaise(CoreRowsProperty, ref _rows, value);
        }

        public TreeDataGridColumnHeadersPresenter? ColumnHeadersPresenter { get; private set; }

        public TreeDataGridRowsPresenter? RowsPresenter { get; private set; }

        /// <summary>
        /// Gets the scrollable that contains the rows presenter.
        /// </summary>
        public IScrollable? Scroll
        {
            get => _scroll;
            private set => SetAndRaise(ScrollProperty, ref _scroll, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether column headers are visible.
        /// </summary>
        public bool ShowColumnHeaders
        {
            get => GetValue(ShowColumnHeadersProperty);
            set => SetValue(ShowColumnHeadersProperty, value);
        }

        /// <summary>
        /// Gets the current cell selection model, if the source exposes one.
        /// </summary>
        internal ITreeDataGridCellSelectionModel? ColumnSelection => CoreSource?.Selection as ITreeDataGridCellSelectionModel;

        /// <summary>
        /// Gets the current row selection model, if the source exposes one.
        /// </summary>
        internal ITreeDataGridRowSelectionModel? RowSelection => CoreSource?.Selection as ITreeDataGridRowSelectionModel;

        /// <summary>
        /// Gets or sets the data source that provides rows and columns for the grid.
        /// </summary>
        internal ITreeDataGridSource? CoreSource
        {
            get => _source;
            set
            {
                if (_source != value)
                {
                    if (_source != null)
                    {
                        _sourceSubscription?.Dispose();
                        _sourceSubscription = null;
                    }

                    var oldSource = _source;

                    OnCoreSourceChanging(oldSource, value);

                    _source              = value;
                    CoreColumns          = _source?.Columns;
                    CoreRows             = _source?.Rows;
                    SelectionInteraction = _source?.Selection as IEngineSelectionInteraction;
                    OnCoreSourceChanged(oldSource, value);

                    if (_source != null)
                    {
                        _sourceSubscription = new SourceSubscription(this, _source);
                    }

                    RaisePropertyChanged(
                        CoreSourceProperty,
                        oldSource,
                        _source);
                }
            }
        }

        internal IEngineSelectionInteraction? SelectionInteraction
        {
            get => _selection;
            set
            {
                if (_selection != value)
                {
                    _interactionSubscription?.Dispose();
                    _interactionSubscription = null;
                    _selection               = value;
                    if (_selection is not null)
                    {
                        _interactionSubscription = new SelectionSubscription(this, _selection);
                    }
                }
            }
        }

        #endregion

        #region Public

        /// <summary>
        /// Attempts to get the cell control at the specified column and row indexes.
        /// </summary>
        /// <param name="columnIndex">The zero-based column index.</param>
        /// <param name="rowIndex">The zero-based row index.</param>
        /// <returns>The cell control if found; otherwise <c>null</c>.</returns>
        public Control? TryGetCell(int columnIndex, int rowIndex)
        {
            if (TryGetRow(rowIndex) is TreeDataGridRow row &&
                row.TryGetCell(columnIndex) is Control cell)
            {
                return cell;
            }

            return null;
        }

        /// <summary>
        /// Attempts to get the realized row element for the specified row index.
        /// </summary>
        /// <param name="rowIndex">The zero-based row index.</param>
        public TreeDataGridRow? TryGetRow(int rowIndex)
        {
            return RowsPresenter?.TryGetElement(rowIndex) as TreeDataGridRow;
        }

        /// <summary>
        /// Attempts to find a cell element by walking up the visual tree from an element.
        /// </summary>
        /// <param name="element">The starting element.</param>
        /// <param name="result">The found <see cref="TreeDataGridCell"/>, if any.</param>
        /// <returns><c>true</c> if a cell was found; otherwise <c>false</c>.</returns>
        public bool TryGetCell(Control? element, [NotNullWhen(true)] out TreeDataGridCell? result)
        {
            if (element.FindAncestorOfType<TreeDataGridCell>(true) is { } cell &&
                cell.ColumnIndex >= 0 &&
                cell.RowIndex >= 0)
            {
                result = cell;

                return true;
            }

            result = null;

            return false;
        }

        /// <summary>
        /// Attempts to find a row element by walking up the visual tree from an element.
        /// </summary>
        /// <param name="element">The starting element.</param>
        /// <param name="result">The found <see cref="TreeDataGridRow"/>, if any.</param>
        /// <returns><c>true</c> if a row was found; otherwise <c>false</c>.</returns>
        public bool TryGetRow(Control? element, [NotNullWhen(true)] out TreeDataGridRow? result)
        {
            if (element is TreeDataGridRow row && row.RowIndex >= 0)
            {
                result = row;

                return true;
            }

            do
            {
                result = element?.FindAncestorOfType<TreeDataGridRow>();

                if (result?.RowIndex >= 0)
                {
                    break;
                }

                element = result;
            } while (result is not null);

            return result is not null;
        }

        /// <summary>
        /// Attempts to obtain the model instance for the row that contains the given element.
        /// </summary>
        /// <typeparam name="TModel">The expected model type.</typeparam>
        /// <param name="element">The element contained within the row.</param>
        /// <param name="result">The model instance if found and of the requested type.</param>
        /// <returns><c>true</c> if the model was found and cast to <typeparamref name="TModel"/>; otherwise <c>false</c>.</returns>
        public bool TryGetRowModel<TModel>(Control element, [NotNullWhen(true)] out TModel? result)
            where TModel : notnull
        {
            if (CoreSource is object &&
                TryGetRow(element, out var row) &&
                row.RowIndex < CoreSource.Rows.Count &&
                CoreSource.Rows[row.RowIndex] is IRow<TModel> rowWithModel)
            {
                result = rowWithModel.Model;

                return true;
            }

            result = default;

            return false;
        }

        public bool QueryCancelSelection()
        {
            if (SelectionChanging is null)
            {
                return false;
            }

            var e = new CancelEventArgs();

            SelectionChanging(this, e);

            return e.Cancel;
        }

        #endregion

        #region Internal

        internal void RaiseCellClearing(TreeDataGridCell cell, int columnIndex, int rowIndex)
        {
            if (CellClearing is not null)
            {
                _cellArgs ??= new TreeDataGridCellEventArgs();
                _cellArgs.Update(cell, columnIndex, rowIndex);
                CellClearing(this, _cellArgs);
                _cellArgs.Update(null, -1, -1);
            }
        }

        internal void RaiseCellPrepared(TreeDataGridCell cell, int columnIndex, int rowIndex)
        {
            if (CellPrepared is not null)
            {
                _cellArgs ??= new TreeDataGridCellEventArgs();
                _cellArgs.Update(cell, columnIndex, rowIndex);
                CellPrepared(this, _cellArgs);
                _cellArgs.Update(null, -1, -1);
            }
        }

        internal void RaiseCellValueChanged(TreeDataGridCell cell, int columnIndex, int rowIndex)
        {
            if (CellValueChanged is not null)
            {
                _cellArgs ??= new TreeDataGridCellEventArgs();
                _cellArgs.Update(cell, columnIndex, rowIndex);
                CellValueChanged(this, _cellArgs);
                _cellArgs.Update(null, -1, -1);
            }
        }

        internal void RaiseRowClearing(TreeDataGridRow row, int rowIndex)
        {
            if (RowClearing is not null)
            {
                _rowArgs ??= new TreeDataGridRowEventArgs();
                _rowArgs.Update(row, rowIndex);
                RowClearing(this, _rowArgs);
                _rowArgs.Update(null, -1);
            }
        }

        internal void RaiseRowPrepared(TreeDataGridRow row, int rowIndex)
        {
            if (RowTheme is { } theme)
            {
                row.SetCurrentValue(ThemeProperty, theme);
            }

            if (RowPrepared is not null)
            {
                _rowArgs ??= new TreeDataGridRowEventArgs();
                _rowArgs.Update(row, rowIndex);
                RowPrepared(this, _rowArgs);
                _rowArgs.Update(null, -1);
            }
        }

        internal void RaiseRowDragStarted(PointerPressedEventArgs trigger, Point position)
        {
            if (_rowDragActive || _source is null || RowSelection is null || RowSelection.SelectedIndexes.Count == 0)
            {
                return;
            }

            var source = _source;

            var allowedEffects = AutoDragDropRows ?
                DragDropEffects.Move :
                DragDropEffects.None;
            var route = BuildEventRoute(RowDragStartedEvent);

            if (route.HasHandlers)
            {
                var e = new TreeDataGridRowDragStartedEventArgs(RowSelection.SelectedItems!);

                e.AllowedEffects = allowedEffects;
                RaiseEvent(e);
                allowedEffects = e.AllowedEffects;
            }

            if (allowedEffects != DragDropEffects.None && ReferenceEquals(source, _source) &&
                RowSelection is { } selection && selection.SelectedIndexes.Count > 0)
            {
                var dataTransfer = new DataTransfer();
                var info         = new DragInfo(source, selection.SelectedIndexes.ToList());
                var item         = new DataTransferItem();

                item.Set(DragInfo.Format, info);
                dataTransfer.Add(item);
                TryGetRow(trigger.Source as Control, out var row);
                _ = DragRowsAsync(trigger, position, row, dataTransfer, allowedEffects);
            }
        }

        #endregion

        #region Protected

        protected override AutomationPeer OnCreateAutomationPeer() =>
            new Automation.Peers.TreeDataGridAutomationPeer(this);

        protected virtual TreeDataGridElementFactory CreateDefaultElementFactory() => new TreeDataGridElementFactory();

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            if (Scroll is ScrollViewer s && _headerScroll is ScrollViewer h)
            {
                s.ScrollChanged -= OnScrollChanged;
                h.ScrollChanged -= OnHeaderScrollChanged;
            }

            base.OnApplyTemplate(e);
            ColumnHeadersPresenter = e.NameScope.Find<TreeDataGridColumnHeadersPresenter>("PART_ColumnHeadersPresenter");
            RowsPresenter          = e.NameScope.Find<TreeDataGridRowsPresenter>("PART_RowsPresenter");
            Scroll                 = e.NameScope.Find<ScrollViewer>("PART_ScrollViewer");
            _headerScroll          = e.NameScope.Find<ScrollViewer>("PART_HeaderScrollViewer");

            if (Scroll is ScrollViewer s1 && _headerScroll is ScrollViewer h1)
            {
                s1.ScrollChanged += OnScrollChanged;
                h1.ScrollChanged += OnHeaderScrollChanged;
            }
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);
            StopDrag();
            _hideDragAdornerTimer?.Dispose();
            _hideDragAdornerTimer = null;
            RemoveDragAdorner();
            RemoveRowDragPreview();
        }

        protected void OnPreviewKeyDown(object? o, KeyEventArgs e)
        {
            if (!HandleSelectionPreservingScroll(e) && !e.Handled && !IsInputInsideInteractiveControl(e.Source))
            {
                _selection?.OnPreviewKeyDown(this, e);
            }
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            OnModernPropertyChanged(change);

            if (change.Property == AutoDragDropRowsProperty)
            {
                DragDrop.SetAllowDrop(this, change.GetNewValue<bool>());
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (!IsInputInsideInteractiveControl(e.Source) && !HandleParentNavigation(e) && !HandleModernKeyDown(e))
            {
                _selection?.OnKeyDown(this, e);
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            _selection?.OnKeyUp(this, e);
        }

        protected override void OnTextInput(TextInputEventArgs e)
        {
            base.OnTextInput(e);

            if (e.Handled || e.Text is { Length: > 0 } && char.IsControl(e.Text[0]))
            {
                return;
            }

            _selection?.OnTextInput(this, e);
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);
            _selection?.OnPointerPressed(this, e);
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            base.OnPointerMoved(e);
            _selection?.OnPointerMoved(this, e);
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);
            _selection?.OnPointerReleased(this, e);
        }

        #endregion

        #region Private

        private void OnClick(object? sender, RoutedEventArgs e)
        {
            if (Source is { } source && e.Source is TreeDataGridColumnHeader header &&
                (uint)header.ColumnIndex < source.Columns.Count)
            {
                var column = source.Columns[header.ColumnIndex];

                if (!(column.CanUserSort ?? CanUserSortColumns))
                {
                    return;
                }

                if (column.SortDirection == ListSortDirection.Descending &&
                    (column.AllowTriStateSorting ?? AllowTriStateSorting))
                {
                    source.ClearSort(column);
                }
                else
                {
                    source.SortBy(column, column.SortDirection == ListSortDirection.Ascending ?
                        ListSortDirection.Descending : ListSortDirection.Ascending);
                }
            }
            else if (_source is not null && e.Source is TreeDataGridColumnHeader engineHeader &&
                (uint)engineHeader.ColumnIndex < _source.Columns.Count && CanUserSortColumns)
            {
                var column = _source.Columns[engineHeader.ColumnIndex];

                _source.SortBy(column, column.SortDirection == ListSortDirection.Ascending ?
                    ListSortDirection.Descending : ListSortDirection.Ascending);
            }
        }

        private Canvas? GetOrCreateDragAdorner()
        {
            _hideDragAdorner = false;

            if (_dragAdorner is not null)
            {
                return _dragAdorner;
            }

            var adornerLayer = AdornerLayer.GetAdornerLayer(this);

            if (adornerLayer is null)
            {
                return null;
            }

            _dragAdorner ??= new Canvas
            {
                Children =
                {
                    new Rectangle
                    {
                        Stroke          = TextElement.GetForeground(this),
                        StrokeThickness = 2,
                    },
                },
                IsHitTestVisible = false,
            };

            adornerLayer.Children.Add(_dragAdorner);
            AdornerLayer.SetAdornedElement(_dragAdorner, this);

            return _dragAdorner;
        }

        private void ShowDragAdorner(TreeDataGridRow row, TreeDataGridRowDropPosition position)
        {
            if (position == TreeDataGridRowDropPosition.None ||
                row.TransformToVisual(this) is not { } transform)
            {
                HideDragAdorner();

                return;
            }

            var adorner = GetOrCreateDragAdorner();

            if (adorner is null)
            {
                return;
            }

            var rectangle = (Rectangle)adorner.Children[0];
            var rowBounds = new Rect(row.Bounds.Size).TransformToAABB(transform);

            Canvas.SetLeft(rectangle, rowBounds.Left);
            rectangle.Width = rowBounds.Width;

            switch (position)
            {
                case TreeDataGridRowDropPosition.Before:
                    Canvas.SetTop(rectangle, rowBounds.Top);
                    rectangle.Height = 0;
                    break;
                case TreeDataGridRowDropPosition.After:
                    Canvas.SetTop(rectangle, rowBounds.Bottom);
                    rectangle.Height = 0;
                    break;
                case TreeDataGridRowDropPosition.Inside:
                    Canvas.SetTop(rectangle, rowBounds.Top);
                    rectangle.Height = rowBounds.Height;
                    break;
            }
        }

        private void HideDragAdorner()
        {
            if (_dragAdorner is null)
            {
                return;
            }

            _hideDragAdorner = true;
            _hideDragAdornerTimer?.Dispose();
            _hideDragAdornerTimer = DispatcherTimer.RunOnce(() =>
            {
                _hideDragAdornerTimer = null;

                if (_hideDragAdorner)
                {
                    RemoveDragAdorner();
                }
            }, TimeSpan.FromMilliseconds(50));
        }

        private void RemoveDragAdorner()
        {
            if (_dragAdorner?.Parent is AdornerLayer layer)
            {
                layer.Children.Remove(_dragAdorner);
            }

            _dragAdorner = null;
        }

        private void StopDrag()
        {
            HideDragAdorner();
            _autoScrollTimer?.Stop();
        }

        private void AutoScroll(bool direction)
        {
            if (_autoScrollTimer is null)
            {
                _autoScrollTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(AutoScrollSpeed),
                };
                _autoScrollTimer.Tick += OnAutoScrollTick;
            }

            _autoScrollDirection = direction;

            if (!_autoScrollTimer.IsEnabled)
            {
                OnAutoScrollTick(null, EventArgs.Empty);
            }

            _autoScrollTimer.Start();
        }

        [MemberNotNullWhen(true, nameof(_source))]
        private bool CalculateAutoDragDrop(
            TreeDataGridRow? targetRow,
            DragEventArgs e,
            [NotNullWhen(true)] out DragInfo? data,
            out TreeDataGridRowDropPosition position)
        {
            if (!AutoDragDropRows ||
                e.DataTransfer.TryGetValues(DragInfo.Format)?.FirstOrDefault() is not DragInfo di ||
                _source is null ||
                targetRow is null ||
                di.Source != _source)
            {
                data     = null;
                position = TreeDataGridRowDropPosition.None;

                return false;
            }

            var targetIndex = _source.Rows.RowIndexToModelIndex(targetRow.RowIndex);

            position = GetDropPosition(_source, e, targetRow);

            // We can't drop rows into themselves or their descendents.
            foreach (var sourceIndex in di.Indexes)
            {
                if (sourceIndex.IsAncestorOf(targetIndex) ||
                    (sourceIndex == targetIndex && position == TreeDataGridRowDropPosition.Inside))
                {
                    data     = null;
                    position = TreeDataGridRowDropPosition.None;

                    return false;
                }
            }

            data = di;

            return true;
        }

        private void OnDragOver(DragEventArgs e)
        {
            if (!TryGetRow(e.Source as Control, out var row))
            {
                e.DragEffects = DragDropEffects.None;
            }

            if (!CalculateAutoDragDrop(row, e, out _, out var adorner))
            {
                e.DragEffects = DragDropEffects.None;
            }

            var route = BuildEventRoute(RowDragOverEvent);

            if (route.HasHandlers)
            {
                var ev = new TreeDataGridRowDragEventArgs(RowDragOverEvent, row, e);

                ev.Position = adorner;
                RaiseEvent(ev);
                adorner = ev.Position;
            }

            if (row != null)
            {
                ShowDragAdorner(row, adorner);
            }

            if (Scroll is ScrollViewer scroller)
            {
                var rowsPosition = e.GetPosition(scroller);

                if (rowsPosition.Y < AutoScrollMargin)
                {
                    AutoScroll(false);
                }
                else if (rowsPosition.Y > Bounds.Height - AutoScrollMargin)
                {
                    AutoScroll(true);
                }
                else
                {
                    _autoScrollTimer?.Stop();
                }
            }
        }

        private void OnDragLeave(RoutedEventArgs e)
        {
            StopDrag();
        }

        private void OnDrop(DragEventArgs e)
        {
            StopDrag();

            TryGetRow(e.Source as Control, out var row);

            var autoDrop = CalculateAutoDragDrop(row, e, out var data, out var position);
            var route    = BuildEventRoute(RowDropEvent);

            if (route.HasHandlers)
            {
                var ev = new TreeDataGridRowDragEventArgs(RowDropEvent, row, e);

                ev.Position = position;
                RaiseEvent(ev);

                if (ev.Handled || e.DragEffects != DragDropEffects.Move)
                {
                    return;
                }

                position = ev.Position;
            }

            if (autoDrop &&
                _source is not null &&
                row is not null &&
                position != TreeDataGridRowDropPosition.None)
            {
                var targetIndex = _source.Rows.RowIndexToModelIndex(row.RowIndex);

                _source.DragDropRows(_source, data!.Indexes, targetIndex, position, e.DragEffects);
            }
        }

        private void OnScrollChanged(object? sender, ScrollChangedEventArgs e)
        {
            if (Scroll is not null && _headerScroll is not null && !MathWrapper.IsZero(e.OffsetDelta.X))
            {
                _headerScroll.Offset = _headerScroll.Offset.WithX(Scroll.Offset.X);
            }
        }

        private void OnHeaderScrollChanged(object? sender, ScrollChangedEventArgs e)
        {
            if (Scroll is not null && _headerScroll is not null && !MathWrapper.IsZero(e.OffsetDelta.X))
            {
                Scroll.Offset = Scroll.Offset.WithX(_headerScroll.Offset.X);
            }
        }

        private void OnAutoScrollTick(object? sender, EventArgs e)
        {
            if (Scroll is ScrollViewer scroll)
            {
                if (!_autoScrollDirection)
                {
                    scroll.LineUp();
                }
                else
                {
                    scroll.LineDown();
                }
            }
        }

        private void OnSelectionInteractionChanged(object? sender, EventArgs e)
        {
            RowsPresenter?.UpdateSelection(SelectionInteraction);
        }

        private void OnSourcePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            OnModernSourcePropertyChanged(e);

            if (e.PropertyName == nameof(ITreeDataGridSource.Selection))
            {
                SelectionInteraction = CoreSource?.Selection as IEngineSelectionInteraction;
                RowsPresenter?.UpdateSelection(SelectionInteraction);
            }
        }

        private void OnSourceSorted()
        {
            RowsPresenter?.RecycleAllElements();
            RowsPresenter?.InvalidateMeasure();
        }

        private static TreeDataGridRowDropPosition GetDropPosition(
            ITreeDataGridSource source,
            DragEventArgs e,
            TreeDataGridRow row)
        {
            var rowY = e.GetPosition(row).Y / row.Bounds.Height;

            if (source.IsHierarchical)
            {
                if (rowY < 0.33)
                {
                    return TreeDataGridRowDropPosition.Before;
                }
                else if (rowY > 0.66)
                {
                    return TreeDataGridRowDropPosition.After;
                }
                else
                {
                    return TreeDataGridRowDropPosition.Inside;
                }
            }
            else
            {
                if (rowY < 0.5)
                {
                    return TreeDataGridRowDropPosition.Before;
                }
                else
                {
                    return TreeDataGridRowDropPosition.After;
                }
            }
        }

        #endregion

        #region Events

        public event EventHandler<TreeDataGridCellEventArgs>? CellClearing;

        public event EventHandler<TreeDataGridCellEventArgs>? CellPrepared;

        public event EventHandler<TreeDataGridCellEventArgs>? CellValueChanged;

        public event EventHandler<TreeDataGridRowEventArgs>? RowClearing;

        public event EventHandler<TreeDataGridRowEventArgs>? RowPrepared;

        /// <summary>
        /// Occurs when a row drag operation starts.
        /// </summary>
        public event EventHandler<TreeDataGridRowDragStartedEventArgs>? RowDragStarted
        {
            add => AddHandler(RowDragStartedEvent, value!);
            remove => RemoveHandler(RowDragStartedEvent, value!);
        }

        /// <summary>
        /// Occurs while a dragged row is being dragged over the control.
        /// </summary>
        public event EventHandler<TreeDataGridRowDragEventArgs>? RowDragOver
        {
            add => AddHandler(RowDragOverEvent, value!);
            remove => RemoveHandler(RowDragOverEvent, value!);
        }

        /// <summary>
        /// Occurs when a dragged row is dropped onto the control.
        /// </summary>
        public event EventHandler<TreeDataGridRowDragEventArgs>? RowDrop
        {
            add => AddHandler(RowDropEvent, value!);
            remove => RemoveHandler(RowDropEvent, value!);
        }

        /// <summary>
        /// Occurs before the selection changes; handlers can cancel the change.
        /// </summary>
        public event CancelEventHandler? SelectionChanging;

        #endregion
    }
}
