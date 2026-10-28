using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using BlackSharp.UI.Avalonia.TreeDataGrid.Models;
using BlackSharp.UI.Avalonia.TreeDataGrid.Selection;
using System.ComponentModel;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Primitives
{
    [PseudoClasses(":editing")]
    public abstract class TreeDataGridCell : TemplatedControl, ITreeDataGridCell
    {
        #region Constructor

        static TreeDataGridCell()
        {
            FocusableProperty.OverrideDefaultValue<TreeDataGridCell>(true);
            DoubleTappedEvent.AddClassHandler<TreeDataGridCell>((x, e) => x.OnDoubleTapped(e));
        }

        #endregion

        #region Fields

        public static readonly StyledProperty<HorizontalAlignment>    HorizontalContentAlignmentProperty =
            ContentControl.HorizontalContentAlignmentProperty.AddOwner<TreeDataGridCell>();

        public static readonly StyledProperty<VerticalAlignment>      VerticalContentAlignmentProperty   =
            ContentControl.VerticalContentAlignmentProperty.AddOwner<TreeDataGridCell>();

        public static readonly DirectProperty<TreeDataGridCell, bool> IsSelectedProperty                 =
            AvaloniaProperty.RegisterDirect<TreeDataGridCell, bool>(
                nameof(IsSelected),
                o => o.IsSelected);

        private static readonly Point                                 s_invalidPoint                     = new Point(double.NaN, double.NaN);

        private bool          _isSelected;

        private TreeDataGrid? _treeDataGrid;

        private Point         _pressedPoint = s_invalidPoint;

        #endregion

        #region Properties

        public HorizontalAlignment HorizontalContentAlignment
        {
            get => GetValue(HorizontalContentAlignmentProperty);
            set => SetValue(HorizontalContentAlignmentProperty, value);
        }

        public VerticalAlignment VerticalContentAlignment
        {
            get => GetValue(VerticalContentAlignmentProperty);
            set => SetValue(VerticalContentAlignmentProperty, value);
        }

        public int ColumnIndex { get; private set; } = -1;

        public int RowIndex { get; private set; } = -1;

        public bool IsEditing { get; private set; }

        public Models.ITreeDataGridCellModel? Model { get; private set; }

        public bool IsSelected
        {
            get => _isSelected;
            private set => SetAndRaise(IsSelectedProperty, ref _isSelected, value);
        }

        public bool IsEffectivelySelected
        {
            get => IsSelected || this.FindAncestorOfType<TreeDataGridRow>()?.IsSelected == true;
        }

        #endregion

        #region Public

        public virtual void Unrealize()
        {
            CancelEdit();

            _treeDataGrid?.RaiseCellClearing(this, ColumnIndex, RowIndex);

            ColumnIndex = RowIndex = -1;
            Model       = null;
        }

        #endregion

        #region Internal

        internal virtual void Realize(
            TreeDataGridElementFactory factory,
            IEngineSelectionInteraction? selection,
            ICell model,
            int columnIndex,
            int rowIndex)
        {
            if (ColumnIndex >= 0 || RowIndex >= 0)
            {
                throw new InvalidOperationException("Cell is already realized.");
            }

            if (columnIndex < 0)
            {
                throw new IndexOutOfRangeException("Invalid column index.");
            }

            if (rowIndex < 0)
            {
                throw new IndexOutOfRangeException("Invalid row index.");
            }

            ColumnIndex = columnIndex;
            RowIndex    = rowIndex;
            Model       = model;
            IsSelected  = selection?.IsCellSelected(columnIndex, rowIndex) ?? false;
            UpdateColumnPresentation();

            _treeDataGrid?.RaiseCellPrepared(this, columnIndex, RowIndex);
        }

        internal void UpdateColumnPresentation()
        {
            if (_treeDataGrid is not { Source: { } source } owner || (uint)ColumnIndex >= source.Columns.Count)
            {
                return;
            }

            var column = source.Columns[ColumnIndex];
            var inner  = this.FindAncestorOfType<TreeDataGridExpanderCell>() is not null;

            if (inner && column is TreeDataGridHierarchicalExpanderColumn { Inner: { } innerColumn })
            {
                column = innerColumn;
            }

            HorizontalContentAlignment = column.HorizontalContentAlignment;
            VerticalContentAlignment   = column.VerticalContentAlignment;
            var lines                  = this.FindAncestorOfType<TreeDataGridCellsPresenter>()?.GridLinesVisibility ?? owner.GridLinesVisibility;

            BorderThickness = inner ? default : new Thickness(0, 0,
                lines.HasFlag(TreeDataGridGridLinesVisibility.Vertical) ? 1 : 0,
                lines.HasFlag(TreeDataGridGridLinesVisibility.Horizontal) ? 1 : 0);
        }

        internal void UpdateRowIndex(int index)
        {
            if (RowIndex == -1)
            {
                throw new InvalidOperationException("Cell is not realized.");
            }

            RowIndex = index;

            if (Model is TreeDataGridRowHeaderCellModel header && _treeDataGrid?.CoreRows is { } rows)
            {
                header.Value = rows[index].Header;
            }
        }

        internal void UpdateSelection(IEngineSelectionInteraction? selection)
        {
            IsSelected = selection?.IsCellSelected(ColumnIndex, RowIndex) ?? false;
        }

        protected internal void BeginEdit()
        {
            if (!IsEditing)
            {
                IsEditing = true;
                (Model as IEditableObject)?.BeginEdit();
                UpdateValue();
                PseudoClasses.Add(":editing");
            }
        }

        protected internal void CancelEdit()
        {
            if (EndEditCore())
            {
                (Model as IEditableObject)?.CancelEdit();
                UpdateValue();
            }
        }

        protected internal void EndEdit()
        {
            if (!IsEditing)
            {
                return;
            }

            (Model as IEditableObject)?.EndEdit();

            if (Model is ICellValidation { ValidationError: not null })
            {
                (Model as IEditableObject)?.BeginEdit();

                return;
            }

            EndEditCore();
            UpdateValue();
            RaiseCellValueChanged();
        }

        #endregion

        #region Protected

        protected override AutomationPeer OnCreateAutomationPeer() =>
            new Automation.Peers.TreeDataGridCellAutomationPeer(this);

        protected void SubscribeToModelChanges()
        {
            if (Model is INotifyPropertyChanged inpc)
            {
                inpc.PropertyChanged += OnModelPropertyChanged;
            }
        }

        protected void UnsubscribeFromModelChanges()
        {
            if (Model is INotifyPropertyChanged inpc)
            {
                inpc.PropertyChanged -= OnModelPropertyChanged;
            }
        }

        protected virtual void UpdateValue()
        {
        }

        protected void RaiseCellValueChanged()
        {
            if (!IsEditing && ColumnIndex != -1 && RowIndex != -1)
            {
                _treeDataGrid?.RaiseCellValueChanged(this, ColumnIndex, RowIndex);
            }
        }

        protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            _treeDataGrid = this.FindLogicalAncestorOfType<TreeDataGrid>();
            UpdateColumnPresentation();
            base.OnAttachedToLogicalTree(e);
        }

        protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            _treeDataGrid = null;
            base.OnDetachedFromLogicalTree(e);
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);

            // The cell may be realized before being parented. In this case raise the CellPrepared event here.
            if (_treeDataGrid is not null && ColumnIndex >= 0 && RowIndex >= 0)
            {
                _treeDataGrid.RaiseCellPrepared(this, ColumnIndex, RowIndex);
            }
        }

        protected override void OnLostFocus(FocusChangedEventArgs e)
        {
            base.OnLostFocus(e);

            var focused         = TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement() as Control;
            var belongsToEditor = focused?.GetLogicalAncestors().Contains(this) == true;

            if (!IsKeyboardFocusWithin && !belongsToEditor && IsEditing)
            {
                EndEdit();
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

        protected override void OnDoubleTapped(TappedEventArgs e)
        {
            if (Model is not null &&
                !e.Handled &&
                !IsEditing &&
                Model.CanEdit &&
                IsEnabledEditGesture(BeginEditGestures.DoubleTap, Model.EditGestures))
            {
                BeginEdit();
                e.Handled = true;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (Model is null || e.Handled)
            {
                return;
            }

            if (e.Key == Key.F2 &&
                !IsEditing &&
                Model.CanEdit &&
                IsEnabledEditGesture(BeginEditGestures.F2, Model.EditGestures))
            {
                BeginEdit();
                e.Handled = true;
            }
            else if (e.Key == Key.Tab && IsEditing)
            {
                EndEdit();
                if (!IsEditing)
                {
                    _treeDataGrid?.MoveToNextEditableCell(ColumnIndex, RowIndex, e.KeyModifiers.HasFlag(KeyModifiers.Shift));
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Enter && IsEditing)
            {
                EndEdit();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape && IsEditing)
            {
                CancelEdit();
                e.Handled = true;
            }
        }

        protected override void OnTextInput(TextInputEventArgs e)
        {
            base.OnTextInput(e);

            if (!e.Handled && !IsEditing && Model is { CanEdit: true } model &&
                !string.IsNullOrEmpty(e.Text) && !char.IsControl(e.Text[0]) &&
                IsEnabledEditGesture(BeginEditGestures.TextInput, model.EditGestures))
            {
                BeginEdit();
                ApplyTemplate();

                if (this.GetVisualDescendants().OfType<TextBox>().FirstOrDefault() is { } editor)
                {
                    editor.Text       = e.Text;
                    editor.CaretIndex = e.Text.Length;
                }

                e.Handled = true;
            }
        }

        protected virtual void OnModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);

            if (Model is not null &&
                !e.Handled &&
                !IsEditing &&
                Model.CanEdit &&
                IsEnabledEditGesture(BeginEditGestures.Tap, Model.EditGestures))
            {
                _pressedPoint = e.GetCurrentPoint(null).Position;
                e.Handled     = true;
            }
            else
            {
                _pressedPoint = s_invalidPoint;
            }
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);

            if (Model is not null &&
                !e.Handled &&
                !IsEditing &&
                !double.IsNaN(_pressedPoint.X) &&
                Model.CanEdit &&
                IsEnabledEditGesture(BeginEditGestures.Tap, Model.EditGestures))
            {
                var point    = e.GetCurrentPoint(this);
                var settings = this.GetPlatformSettings();
                var tapSize  = settings?.GetTapSize(point.Pointer.Type) ?? new Size(4, 4);

                var tapRect  = new Rect(_pressedPoint, new Size())
                       .Inflate(new Thickness(tapSize.Width, tapSize.Height));

                if (new Rect(Bounds.Size).ContainsExclusive(point.Position) &&
                    tapRect.ContainsExclusive(e.GetCurrentPoint(null).Position))
                {
                    BeginEdit();
                    e.Handled = true;
                }
            }

            _pressedPoint = s_invalidPoint;
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            if (change.Property == IsSelectedProperty)
            {
                PseudoClasses.Set(":selected", IsSelected);
            }

            base.OnPropertyChanged(change);
        }

        #endregion

        #region Private

        private bool EndEditCore()
        {
            if (IsEditing)
            {
                var restoreFocus = IsKeyboardFocusWithin;

                IsEditing = false;
                PseudoClasses.Remove(":editing");

                if (restoreFocus)
                {
                    Focus();
                }

                return true;
            }

            return false;
        }

        private bool IsEnabledEditGesture(BeginEditGestures gesture, BeginEditGestures enabledGestures)
        {
            if (!enabledGestures.HasFlag(gesture))
            {
                return false;
            }

            return enabledGestures.HasFlag(BeginEditGestures.WhenSelected) ?
                IsEffectivelySelected : true;
        }

        #endregion

        #region ITreeDataGridCell

        void ITreeDataGridCell.Realize(TreeDataGridElementFactory factory, IEngineSelectionInteraction? selection,
            ICell model, int columnIndex, int rowIndex) => Realize(factory, selection, model, columnIndex, rowIndex);

        #endregion
    }
}
