using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace BlackSharp.UI.Avalonia.TreeDataGrid
{
    public abstract class CellRenderer
    {
        #region Public

        public abstract Size MeasureContent(Control owner, object? dataContext, Size availableSize);

        public abstract void RenderContent(Control owner, DrawingContext context, object? dataContext, Rect bounds, bool isSelected);

        public virtual Control? CreateEditControl(object? dataContext) => null;

        public virtual void CommitEdit(Control control)
        {
        }

        #endregion
    }
}
