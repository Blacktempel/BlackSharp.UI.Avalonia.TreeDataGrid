using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using BlackSharp.UI.Avalonia.TreeDataGrid.Demo.Models;
using BlackSharp.UI.Avalonia.TreeDataGrid.Models;
using BlackSharp.UI.Avalonia.TreeDataGrid.Selection;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Demo.ViewModels
{
    internal class DynamicColumnsViewModel : NotifyingBase
    {
        #region Constructor

        public DynamicColumnsViewModel()
        {
            var _data = DataRow.CreateRandomItems();

            Source           = new HierarchicalTreeDataGridSource<DataRow>(_data);
            Source.Selection = new TreeDataGridCellSelectionModel<DataRow>(Source);

            ColumnCount = 20;
        }

        #endregion

        #region Properties

        public int ColumnCount
        {
            get => field;
            set
            {
                if (value > 0)
                {
                    OnColumnCountChanged(field = value);
                }
            }
        }

        public HierarchicalTreeDataGridSource<DataRow> Source { get; }

        #endregion

        #region Private

        private void OnColumnCountChanged(int newValue)
        {
            foreach (var column in Source.Columns)
            {
                if (column.SortDirection is not null)
                {
                    Source.ClearSort(column);
                }
            }

            DataRow.SetColumnCount(Source.Items, newValue - 1);

            var columns = Source.Columns;

            if (columns.Count == 0)
            {
                Source.WithHierarchicalExpanderTextColumn("Name", row => row.Name, row => row.Children);
            }

            for (int i = columns.Count - 1; i >= newValue; i--)
            {
                columns.RemoveAt(i);
            }

            for (var i = columns.Count; i < newValue; i++)
            {
                var cellIndex = i - 1;

                Source.WithTemplateColumn(DataRow.NewColumnName(),
                    new FuncDataTemplate<DataRow>((row, _) =>
                    {
                        var text = new TextBlock();

                        text.Bind(TextBlock.TextProperty, CompiledBinding.Create<DataRow, string?>(x => x.Cells[cellIndex].Value));

                        return text;
                    }, supportsRecycling: true),
                    new FuncDataTemplate<DataRow>((row, _) =>
                    {
                        var editor = new TextBox();

                        editor.Bind(TextBox.TextProperty, CompiledBinding.Create<DataRow, string?>(x => x.Cells[cellIndex].Value, mode: BindingMode.TwoWay));
                        editor.Bind(TextBox.IsReadOnlyProperty, CompiledBinding.Create<DataRow, bool>(x => x.Cells[cellIndex].ReadOnly));

                        return editor;
                    }, supportsRecycling: true),
                    options =>
                    {
                        options.TextSearchBinding = CompiledBinding.Create<DataRow, string?>(x => x.Cells[cellIndex].Value);
                        options.CompareAscending  = (a, b) => Comparer<string?>.Default.Compare(
                            ((DataRow?)a)?.Cells[cellIndex].Value, ((DataRow?)b)?.Cells[cellIndex].Value);
                    });
            }
        }

        #endregion
    }
}
