using Avalonia.Controls;
using BlackSharp.UI.Avalonia.TreeDataGrid.Models;
using System.ComponentModel;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Tests
{
    internal class LayoutTestColumn<TModel> : ColumnBase<TModel> where TModel : class
    {
        public LayoutTestColumn(string header, GridLength? width = null, ColumnOptions<TModel>? options = null)
            : base(header, width, options ?? DefaultOptions())
        {
        }

        public override ICell CreateCell(IRow<TModel> row)
        {
            var indexable = (IModelIndexableRow)row;
            return new LayoutTestCell($"{Header} Row {indexable.ModelIndex}");
        }

        public override Comparison<TModel?>? GetComparison(ListSortDirection direction)
        {
            throw new NotImplementedException();
        }

        private static ColumnOptions<TModel> DefaultOptions()
        {
            return new ColumnOptions<TModel>
            {
                MinWidth = new GridLength(0, GridUnitType.Pixel)
            };
        }
    }
}
