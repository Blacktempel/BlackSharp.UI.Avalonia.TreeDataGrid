using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using BlackSharp.UI.Avalonia.TreeDataGrid.Models;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Primitives
{
    public class TreeDataGridColumnHeadersPresenter : TreeDataGridColumnarPresenterBase<object>, IChildIndexProvider
    {
        #region Fields

        private TreeDataGridColumnLayoutSubscription<TreeDataGridColumnHeadersPresenter>? _columnLayoutSubscription;

        #endregion

        #region Properties

        protected override Orientation Orientation => Orientation.Horizontal;

        #endregion

        #region Public

        /// <inheritdoc/>
        public int GetChildIndex(ILogical child)
        {
            if (child is TreeDataGridColumnHeader header)
            {
                return header.ColumnIndex;
            }

            return -1;
        }

        /// <inheritdoc/>
        public bool TryGetTotalCount(out int count)
        {
            if (Items is null)
            {
                count = 0;

                return false;
            }

            count = Items.Count;

            return true;
        }

        #endregion

        #region Protected

        protected override AutomationPeer OnCreateAutomationPeer() =>
            new Automation.Peers.TreeDataGridColumnHeadersPresenterAutomationPeer(this);

        protected override Size ArrangeOverride(Size finalSize)
        {
            (Items as IColumns)?.CommitActualWidths();

            return base.ArrangeOverride(finalSize);
        }

        protected override Size MeasureElement(int index, Control element, Size availableSize)
        {
            if (Items is IColumns columns)
            {
                element.Measure(availableSize);

                return columns.CellMeasured(index, -1, element.DesiredSize);
            }

            return default;
        }

        protected override void RealizeElement(Control element, object item, int index)
        {
            var column = (IColumn)item;

            ((TreeDataGridColumnHeader)element).Realize((IColumns)Items!, index);
            ChildIndexChanged?.Invoke(this, new ChildIndexChangedEventArgs(element, index));
        }

        protected override void UpdateElementIndex(Control element, int oldIndex, int newIndex)
        {
            ((TreeDataGridColumnHeader)element).UpdateColumnIndex(newIndex);
            ChildIndexChanged?.Invoke(this, new ChildIndexChangedEventArgs(element, newIndex));
        }

        protected override void UnrealizeElement(Control element)
        {
            ((TreeDataGridColumnHeader)element).Unrealize();
            ChildIndexChanged?.Invoke(this, new ChildIndexChangedEventArgs(element, ((TreeDataGridColumnHeader)element).ColumnIndex));
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            ObserveColumnLayout(Items as IColumns);
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            ObserveColumnLayout(null);
            base.OnDetachedFromVisualTree(e);
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            if (change.Property == ItemsProperty)
            {
                var oldValue = change.GetOldValue<IReadOnlyList<object>?>();
                var newValue = change.GetNewValue<IReadOnlyList<object>?>();

                ObserveColumnLayout(newValue as IColumns);
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
        }

        #endregion

        #region Events

        /// <inheritdoc/>
        public event EventHandler<ChildIndexChangedEventArgs>? ChildIndexChanged;

        #endregion
    }
}
