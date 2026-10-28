using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using BlackSharp.UI.Avalonia.TreeDataGrid.Experimental.Data.Core;
using BlackSharp.UI.Avalonia.TreeDataGrid.Models;
using BlackSharp.UI.Avalonia.TreeDataGrid.Selection;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Primitives
{
    public class TreeDataGridRowsPresenter : TreeDataGridPresenterBase<object>, IChildIndexProvider
    {
        #region Fields

        internal static readonly DirectProperty<TreeDataGridRowsPresenter, IColumns?> ColumnsProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridRowsPresenter, IColumns?>(
                nameof(Columns),
                o => o.Columns,
                (o, v) => o.Columns = v);

        private IColumns?                                                        _columns;

        private TreeDataGridColumnLayoutSubscription<TreeDataGridRowsPresenter>? _columnLayoutSubscription;

        #endregion

        #region Properties

        internal IColumns? Columns
        {
            get => _columns;
            set => SetAndRaise(ColumnsProperty, ref _columns, value);
        }

        protected override Orientation Orientation => Orientation.Vertical;

        #endregion

        #region Public

        /// <inheritdoc/>
        public int GetChildIndex(ILogical child)
        {
            if (child is TreeDataGridRow row)
            {
                return row.RowIndex;
            }

            return -1;

        }

        /// <inheritdoc/>
        public bool TryGetTotalCount(out int count)
        {
            if (Items != null)
            {
                count = Items.Count;

                return true;
            }

            count = 0;

            return false;
        }

        #endregion

        #region Internal

        internal void UpdateSelection(IEngineSelectionInteraction? selection)
        {
            foreach (var element in RealizedElements)
            {
                if (element is TreeDataGridRow { RowIndex: >= 0 } row)
                {
                    row.UpdateSelection(selection);
                }
            }
        }

        #endregion

        #region Protected

        protected override (int index, double position) GetElementAt(double position)
        {
            return ((IRows)Items!).GetRowAt(position);
        }

        protected override void RealizeElement(Control element, object item, int index)
        {
            var rowModel = (IRow)item;

            var row = (TreeDataGridRow)element;

            row.Realize(ElementFactory, GetSelection(), Columns, (IRows?)Items, index);
            ChildIndexChanged?.Invoke(this, new ChildIndexChangedEventArgs(element, index));
        }

        protected override void UpdateElementIndex(Control element, int oldIndex, int newIndex)
        {
            ((TreeDataGridRow)element).UpdateIndex(newIndex);
            ChildIndexChanged?.Invoke(this, new ChildIndexChangedEventArgs(element, newIndex));
        }

        protected override void UnrealizeElement(Control element)
        {
            ((TreeDataGridRow)element).Unrealize();
            ChildIndexChanged?.Invoke(this, new ChildIndexChangedEventArgs(element, ((TreeDataGridRow)element).RowIndex));
        }

        protected override void UnrealizeElementOnItemRemoved(Control element)
        {
            ((TreeDataGridRow)element).UnrealizeOnItemRemoved();
            ChildIndexChanged?.Invoke(this, new ChildIndexChangedEventArgs(element, ((TreeDataGridRow)element).RowIndex));
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var result = base.MeasureOverride(availableSize);

            // The scroll viewer measures horizontally without a constraint. Use its actual viewport
            // for star columns so an empty grid does not acquire an estimated horizontal overflow.
            if (Columns is not null && (Items is null || Items.Count == 0))
            {
                var width = double.IsFinite(Viewport.Width) && Viewport.Width > 0 ? Viewport.Width : availableSize.Width;

                result = result.WithWidth(Columns.GetEstimatedWidth(width));
            }

            return result;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            Columns?.CommitActualWidths();

            return base.ArrangeOverride(finalSize);
        }

        protected override void OnEffectiveViewportChanged(object? sender, EffectiveViewportChangedEventArgs e)
        {
            var previousWidth = Viewport.Width;

            base.OnEffectiveViewportChanged(sender, e);
            Columns?.ViewportChanged(Viewport);

            if (!MathWrapper.AreClose(previousWidth, Viewport.Width))
            {
                // The base presenter only tracks vertical viewport changes; empty rows also depend on width.
                InvalidateMeasure();
            }
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            ObserveColumnLayout(Columns);
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            ObserveColumnLayout(null);
            base.OnDetachedFromVisualTree(e);
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            if (change.Property == ColumnsProperty)
            {
                var newValue = change.GetNewValue<IColumns>();

                ObserveColumnLayout(newValue);

                // Source replacement passes through null, so the new columns need the existing viewport
                // even when the previous property value is null.
                if (newValue != null)
                {
                    newValue.ViewportChanged(Viewport);
                }
            }

            base.OnPropertyChanged(change);
        }

        #endregion

        #region Private

        private void ObserveColumnLayout(IColumns? columns)
        {
            _columnLayoutSubscription?.Dispose();

            _columnLayoutSubscription = columns is not null ?
                new(this, columns, static owner => owner.OnColumnLayoutInvalidated(null, EventArgs.Empty)) : null;
        }

        private void OnColumnLayoutInvalidated(object? sender, EventArgs e)
        {
            InvalidateMeasure();

            foreach (var element in RealizedElements)
            {
                if (element is TreeDataGridRow row)
                {
                    row.CellsPresenter?.InvalidateMeasure();
                }
            }
        }

        private IEngineSelectionInteraction? GetSelection()
        {
            return this.FindAncestorOfType<TreeDataGrid>()?.SelectionInteraction;
        }

        #endregion

        #region Events

        /// <inheritdoc/>
        public event EventHandler<ChildIndexChangedEventArgs>? ChildIndexChanged;

        #endregion
    }
}
