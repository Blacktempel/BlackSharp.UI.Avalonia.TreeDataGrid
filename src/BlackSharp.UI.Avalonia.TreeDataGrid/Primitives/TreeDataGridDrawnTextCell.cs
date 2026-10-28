using Avalonia.VisualTree;
using BlackSharp.UI.Avalonia.TreeDataGrid.Models;
using BlackSharp.UI.Avalonia.TreeDataGrid.Selection;
using System.ComponentModel;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Primitives
{
    public class TreeDataGridDrawnTextCell : TreeDataGridTextCell
    {
        #region Public

        public override void Unrealize()
        {
            ClearLayout();
            base.Unrealize();

            InvalidateContent();
        }

        #endregion

        #region Internal

        internal override void Realize(TreeDataGridElementFactory factory, IEngineSelectionInteraction? selection,
            ICell model, int columnIndex, int rowIndex)
        {
            ClearLayout();
            base.Realize(factory, selection, model, columnIndex, rowIndex);

            InvalidateContent();
        }

        #endregion

        #region Protected

        protected override void OnModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            base.OnModelPropertyChanged(sender, e);

            if (e.PropertyName == nameof(ITextCell.Value))
            {
                ClearLayout();
            }

            InvalidateContent();
        }

        #endregion

        #region Private

        private void ClearLayout()
        {
            if (Model is TextCell<object?> { Column: TreeDataGridTextColumn column })
            {
                column.ClearTextLayout(this);
            }
        }

        private void InvalidateContent()
        {
            InvalidateMeasure();
            InvalidateVisual();

            foreach (var child in this.GetVisualDescendants())
            {
                if (child is TreeDataGridDrawnCellContent content)
                {
                    content.InvalidateMeasure();
                    content.InvalidateVisual();
                }
            }
        }

        #endregion
    }
}
