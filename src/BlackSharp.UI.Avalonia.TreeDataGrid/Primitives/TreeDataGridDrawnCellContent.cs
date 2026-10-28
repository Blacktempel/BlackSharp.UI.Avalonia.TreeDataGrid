using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using Avalonia.VisualTree;
using BlackSharp.UI.Avalonia.TreeDataGrid.Models;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Primitives
{
    public class TreeDataGridDrawnCellContent : Control
    {
        #region Constructor

        static TreeDataGridDrawnCellContent()
        {
            AffectsMeasure<TreeDataGridDrawnCellContent>(TextElement.FontFamilyProperty, TextElement.FontSizeProperty,
                TextElement.FontStyleProperty, TextElement.FontWeightProperty, TextElement.FontStretchProperty, FlowDirectionProperty);

            AffectsRender<TreeDataGridDrawnCellContent>(TextElement.ForegroundProperty);
        }

        #endregion

        #region Public

        public override void Render(DrawingContext context)
        {
            base.Render(context);

            if (this.FindAncestorOfType<TreeDataGridDrawnTextCell>() is { Model: TextCell<object?> { Column: { } column } } cell)
            {
                if (column.CellRenderer is TreeDataGridTextCellRenderer renderer && cell.Model is ITextCell text)
                {
                    renderer.RenderCell(cell, context, text, new Rect(Bounds.Size));
                }
            }
        }

        #endregion

        #region Protected

        protected override Size MeasureOverride(Size availableSize)
        {
            if (this.FindAncestorOfType<TreeDataGridDrawnTextCell>() is { Model: TextCell<object?> { Column: { } column } } cell)
            {
                return column.CellRenderer is TreeDataGridTextCellRenderer renderer && cell.Model is ITextCell text ?
                    renderer.MeasureCell(cell, text, availableSize) : default;
            }

            return default;
        }

        #endregion
    }
}
