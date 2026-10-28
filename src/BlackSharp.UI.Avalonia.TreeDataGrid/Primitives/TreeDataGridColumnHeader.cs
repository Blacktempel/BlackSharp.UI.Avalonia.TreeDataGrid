using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using BlackSharp.UI.Avalonia.TreeDataGrid.Experimental.Data.Core;
using BlackSharp.UI.Avalonia.TreeDataGrid.Models;
using System.ComponentModel;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Primitives
{
    public class TreeDataGridColumnHeader : Button
    {
        #region Fields

        public static readonly DirectProperty<TreeDataGridColumnHeader, bool>               CanUserResizeProperty  =
            AvaloniaProperty.RegisterDirect<TreeDataGridColumnHeader, bool>(
                nameof(CanUserResize),
                x => x.CanUserResize);

        public static readonly DirectProperty<TreeDataGridColumnHeader, object?>            HeaderProperty         =
            AvaloniaProperty.RegisterDirect<TreeDataGridColumnHeader, object?>(
                nameof(Header),
                o => o.Header);

        public static readonly DirectProperty<TreeDataGridColumnHeader, ListSortDirection?> SortDirectionProperty  =
            AvaloniaProperty.RegisterDirect<TreeDataGridColumnHeader, ListSortDirection?>(
                nameof(SortDirection),
                o => o.SortDirection);

        private bool               _canUserResize;

        private IColumns?          _columns;

        private object?            _header;

        private IColumn?           _model;

        private ListSortDirection? _sortDirection;

        private TreeDataGrid?      _owner;

        private Thumb?             _resizer;

        private Point?             _columnDragStart;

        private bool               _columnDragging;

        public static readonly DirectProperty<TreeDataGridColumnHeader, bool>               CanUserReorderProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridColumnHeader, bool>(nameof(CanUserReorder), x => x.CanUserReorder);

        private bool               _canUserReorder;

        #endregion

        #region Properties

        public bool CanUserReorder
        {
            get => _canUserReorder;
            private set => SetAndRaise(CanUserReorderProperty, ref _canUserReorder, value);
        }

        public bool CanUserResize
        {
            get => _canUserResize;
            private set => SetAndRaise(CanUserResizeProperty, ref _canUserResize, value);
        }

        public int ColumnIndex { get; private set; }

        public object? Header
        {
            get => _header;
            private set => SetAndRaise(HeaderProperty, ref _header, value);
        }

        public ListSortDirection? SortDirection
        {
            get => _sortDirection;
            private set => SetAndRaise(SortDirectionProperty, ref _sortDirection, value);
        }

        #endregion

        #region Public

        public void UpdateColumnIndex(int columnIndex)
        {
            ColumnIndex = columnIndex;
        }

        public void Unrealize()
        {
            if (_model is INotifyPropertyChanged oldInpc)
            {
                oldInpc.PropertyChanged -= OnModelPropertyChanged;
            }

            _columns    = null;
            _model      = null;
            ColumnIndex = -1;
            UpdatePropertiesFromModel();
        }

        #endregion

        #region Internal

        internal void Realize(IColumns columns, int columnIndex)
        {
            if (_model is object)
            {
                throw new InvalidOperationException("Column header is already realized.");
            }

            _columns    = columns;
            _model      = columns[columnIndex];
            ColumnIndex = columnIndex;
            UpdatePropertiesFromModel();

            if (_model is INotifyPropertyChanged newInpc)
            {
                newInpc.PropertyChanged += OnModelPropertyChanged;
            }
        }

        #endregion

        #region Protected

        protected override AutomationPeer OnCreateAutomationPeer() =>
            new Automation.Peers.TreeDataGridColumnHeaderAutomationPeer(this);

        protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
        {
            _columnDragging  = false;
            _columnDragStart = null;

            base.OnPointerCaptureLost(e);
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);
            var control = e.Source as Control;

            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed &&
                control is not Thumb && control?.FindAncestorOfType<Thumb>() is null &&
                this.FindAncestorOfType<TreeDataGrid>() is { Source: { } source } owner &&
                (uint)ColumnIndex < source.Columns.Count &&
                (source.Columns[ColumnIndex].CanUserReorder ?? owner.CanUserReorderColumns))
            {
                _columnDragStart = e.GetPosition(this);
            }
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            base.OnPointerMoved(e);

            if (_columnDragStart is { } start && e.GetCurrentPoint(this).Properties.IsLeftButtonPressed &&
                Math.Abs(e.GetPosition(this).X - start.X) >= 4)
            {
                _columnDragging = true;

                e.Pointer.Capture(this);
                e.Handled = true;
            }
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            var dragging = _columnDragging;

            _columnDragging  = false;
            _columnDragStart = null;

            if (dragging && this.FindAncestorOfType<TreeDataGrid>() is { ColumnHeadersPresenter: { } presenter } owner)
            {
                var position = e.GetPosition(presenter);

                var target   = presenter.GetRealizedElements().OfType<TreeDataGridColumnHeader>()
                    .FirstOrDefault(x => x.IsVisible && x.Bounds.Contains(position));

                // Releasing capture also clears Button's pressed state before its click handling.
                e.Pointer.Capture(null);

                if (target is not null)
                {
                    owner.TryReorderColumn(ColumnIndex, target.ColumnIndex);
                }

                e.Handled = true;
            }

            base.OnPointerReleased(e);
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            if (this.FindAncestorOfType<TreeDataGrid>()?.ColumnHeaderTheme is { } theme)
            {
                SetCurrentValue(ThemeProperty, theme);
            }

            if (_resizer is not null)
            {
                _resizer.DragDelta    -= ResizerDragDelta;
                _resizer.DoubleTapped -= ResizerDoubleTapped;
            }

            _resizer = e.NameScope.Find<Thumb>("PART_Resizer");

            if (_resizer is not null)
            {
                _resizer.DragDelta    += ResizerDragDelta;
                _resizer.DoubleTapped += ResizerDoubleTapped;
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var result = base.MeasureOverride(availableSize);

            // HACKFIX for #83. Seems that cells are getting truncated at times due to DPI scaling.
            // New text stack in Avalonia 11.0 should fix this but until then a hack to add a pixel
            // to cell size seems to fix it.
            result = result.Inflate(new Thickness(1, 0));

            return result;
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            if (change.Property == CanUserResizeProperty)
            {
                PseudoClasses.Set(":resizable", change.GetNewValue<bool>());
            }
            else if (change.Property == DataContextProperty)
            {
                var oldModel = change.GetOldValue<object?>() as IColumn;
                var newModel = change.GetNewValue<object?>() as IColumn;

                if (oldModel is INotifyPropertyChanged oldInpc)
                {
                    oldInpc.PropertyChanged -= OnModelPropertyChanged;
                }

                if (newModel is INotifyPropertyChanged newInpc)
                {
                    newInpc.PropertyChanged += OnModelPropertyChanged;
                }

                UpdatePropertiesFromModel();
            }
            else if (change.Property == ParentProperty)
            {
                _owner?.PropertyChanged -= OnOwnerPropertyChanged;

                _owner = change.GetNewValue<StyledElement>()?.TemplatedParent as TreeDataGrid;

                _owner?.PropertyChanged += OnOwnerPropertyChanged;

                UpdatePropertiesFromModel();
            }

            base.OnPropertyChanged(change);
        }

        #endregion

        #region Private

        private void ResizerDoubleTapped(object? sender, RoutedEventArgs e)
        {
            _columns?.SetColumnWidth(ColumnIndex, GridLength.Auto);
        }

        private void OnModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TreeDataGridColumn.CanUserReorder) || e.PropertyName == nameof(IColumn.CanUserResize) ||
                e.PropertyName == nameof(IColumn.Header) ||
                e.PropertyName == nameof(IColumn.SortDirection)
                || e.PropertyName == nameof(IColumn.IsVisible)
                )
            {
                UpdatePropertiesFromModel();
            }
        }

        private void OnOwnerPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (_owner is null)
            {
                return;
            }

            if (e.Property == TreeDataGrid.CanUserResizeColumnsProperty || e.Property == TreeDataGrid.CanUserReorderColumnsProperty)
            {
                UpdatePropertiesFromModel();
            }
        }

        private void ResizerDragDelta(object? sender, VectorEventArgs e)
        {
            if (_columns is null || _model is null || !CanUserResize || MathWrapper.IsZero(e.Vector.X))
            {
                return;
            }

            var adjacentColumnIndex = ColumnIndex + 1;

            while (adjacentColumnIndex < _columns.Count && !_columns[adjacentColumnIndex].IsVisible)
            {
                ++adjacentColumnIndex;
            }

            if (adjacentColumnIndex < _columns.Count &&
                !(_columns[adjacentColumnIndex].CanUserResize ?? _owner?.CanUserResizeColumns ?? false))
            {
                return;
            }

            _columns.ResizeColumn(ColumnIndex, adjacentColumnIndex < _columns.Count ? adjacentColumnIndex : -1, e.Vector.X);
        }

        private void UpdatePropertiesFromModel()
        {
            var oldVisibility = IsVisible;

            CanUserReorder = _owner?.Source is { } source && (uint)ColumnIndex < source.Columns.Count &&
                (source.Columns[ColumnIndex].CanUserReorder ?? _owner.CanUserReorderColumns);
            CanUserResize = _model?.CanUserResize ?? _owner?.CanUserResizeColumns ?? false;
            Header        = _model?.Header;
            Tag           = _model?.Tag;
            SortDirection = _model?.SortDirection;
            IsVisible     = _model?.IsVisible == true;

            if (IsVisible != oldVisibility)
            {
                _columns?.InvalidateLayout();
            }
        }

        #endregion
    }
}
