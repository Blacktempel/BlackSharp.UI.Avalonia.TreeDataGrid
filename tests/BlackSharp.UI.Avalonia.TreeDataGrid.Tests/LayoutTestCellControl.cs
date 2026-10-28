using Avalonia;
using BlackSharp.UI.Avalonia.TreeDataGrid.Primitives;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Tests
{
    internal class LayoutTestCellControl : TreeDataGridTextCell
    {
        public List<Size> MeasureConstraints { get; } = [];

        protected override Size MeasureOverride(Size availableSize)
        {
            MeasureConstraints.Add(availableSize);
            return new Size(10, 10);
        }
    }
}
