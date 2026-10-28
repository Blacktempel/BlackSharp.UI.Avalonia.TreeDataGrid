using Avalonia.Controls;
using BlackSharp.UI.Avalonia.TreeDataGrid.Models;
using System.Diagnostics;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Primitives
{
    public class TreeDataGridElementFactory
    {
        #region Fields

        private readonly Dictionary<object, List<Control>> _recyclePool = [];

        #endregion

        #region Public

        public Control GetOrCreateElement(object? data, Control parent)
        {
            var recycleKey = GetDataRecycleKey(data);

            if (_recyclePool.TryGetValue(recycleKey, out var elements) && elements.Count > 0)
            {
                // First look for an element with the same parent.
                for (var i = 0; i < elements.Count; i++)
                {
                    var e = elements[i];

                    if (e.Parent == parent)
                    {
                        parent.InvalidateMeasure();
                        elements.RemoveAt(i);

                        return e;
                    }
                }

                // Next look for an element with no parent or an element that we can reparent.
                for (var i = 0; i < elements.Count; i++)
                {
                    var e           = elements[i];
                    var parentPanel = e.Parent as Panel;

                    if (e.Parent is null || parentPanel is not null)
                    {
                        parent.InvalidateMeasure();
                        parentPanel?.Children.Remove(e);

                        Debug.Assert(e.Parent is null);

                        elements.RemoveAt(i);

                        return e;
                    }
                }
            }

            // Otherwise create a new element.
            return CreateElement(data);
        }

        public void RecycleElement(Control element)
        {
            var recycleKey = GetElementRecycleKey(element);

            if (!_recyclePool.TryGetValue(recycleKey, out var elements))
            {
                elements = [];
                _recyclePool.Add(recycleKey, elements);
            }

            elements.Add(element);
        }

        #endregion

        #region Internal

        internal string GetRecycleKeyForData(object? data) => GetDataRecycleKey(data);

        #endregion

        #region Protected

        protected virtual Control CreateElement(object? data)
        {
            return data switch
            {
                TreeDataGridRowHeaderCellModel => new TreeDataGridRowHeaderCell(),
                TextCell<object?> { Column: TreeDataGridTextColumn { UseDrawnCells: true } } => new TreeDataGridDrawnTextCell(),
                CheckBoxCell => new TreeDataGridCheckBoxCell(),
                TemplateCell => new TreeDataGridTemplateCell(),
                IExpanderCell => new TreeDataGridExpanderCell(),
                ICell => new TreeDataGridTextCell(),
                IColumn => new TreeDataGridColumnHeader(),
                IRow => new TreeDataGridRow(),
                _ => throw new NotSupportedException(),
            };
        }

        protected virtual string GetDataRecycleKey(object? data)
        {
            return data switch
            {
                TreeDataGridRowHeaderCellModel => typeof(TreeDataGridRowHeaderCell).FullName!,
                TextCell<object?> { Column: TreeDataGridTextColumn { UseDrawnCells: true } } => typeof(TreeDataGridDrawnTextCell).FullName!,
                CheckBoxCell => typeof(TreeDataGridCheckBoxCell).FullName!,
                TemplateCell => typeof(TreeDataGridTemplateCell).FullName!,
                IExpanderCell => typeof(TreeDataGridExpanderCell).FullName!,
                ICell => typeof(TreeDataGridTextCell).FullName!,
                IColumn => typeof(TreeDataGridColumnHeader).FullName!,
                IRow => typeof(TreeDataGridRow).FullName!,
                _ => throw new NotSupportedException(),
            };
        }

        protected virtual string GetElementRecycleKey(Control element)
        {
            return element.GetType().FullName!;
        }

        #endregion
    }
}
