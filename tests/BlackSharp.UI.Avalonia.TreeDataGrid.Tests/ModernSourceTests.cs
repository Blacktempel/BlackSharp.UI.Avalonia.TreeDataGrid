using Avalonia.Data;
using Avalonia.Headless.XUnit;
using BlackSharp.UI.Avalonia.TreeDataGrid.Models;
using BlackSharp.UI.Avalonia.TreeDataGrid.Selection;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Xunit;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Tests
{
    public class ModernSourceTests
    {
        #region Public

        [AvaloniaFact]
        public void Flat_Filter_Preserves_Model_Indexes_And_Tracks_Collection_Changes()
        {
            var items        = new ObservableCollection<Model>(Enumerable.Range(0, 6).Select(x => new Model { Number = x }));
            using var source = new FlatTreeDataGridSource<Model>(items).WithTextColumn("Number", x => x.Number);

            source.Filter(x => x.Number % 2 == 0);
            Assert.Equal(new[] { 0, 2, 4 }, source.Rows.Select(x => ((Model)x.Model!).Number));
            Assert.Equal(new IndexPath(2), source.RowIndexToModelIndex(1));
            Assert.Equal(-1, source.ModelIndexToRowIndex(1));
            items.Insert(0, new Model { Number = 8 });
            Assert.Equal(new IndexPath(3), source.RowIndexToModelIndex(2));
            source.SortBy(source.Columns[0], ListSortDirection.Descending);
            Assert.Equal(new[] { 8, 4, 2, 0 }, source.Rows.Select(x => ((Model)x.Model!).Number));
            source.ClearSort(source.Columns[0]);
            Assert.Equal(new[] { 8, 0, 2, 4 }, source.Rows.Select(x => ((Model)x.Model!).Number));
            source.Filter(null);
            Assert.Equal(7, source.Rows.Count);
            Assert.False(source.IsFiltered);
        }

        [AvaloniaFact]
        public void Flat_Row_Models_Are_Stable_Snapshots()
        {
            using var source = new FlatTreeDataGridSource<Model>(new[] { new Model { Number = 1 }, new Model { Number = 2 } });
            var first        = source.Rows[0];
            var second       = source.Rows[1];

            Assert.Equal(1, ((Model)first.Model!).Number);
            Assert.Equal(2, ((Model)second.Model!).Number);
            Assert.Equal(new IndexPath(0), first.ModelIndexPath);
        }

        [AvaloniaFact]
        public void Standard_Binding_Updates_Text_And_Writes_Edits_Back()
        {
            var model        = new Model { Text = "before" };
            using var source = new FlatTreeDataGridSource<Model>(new[] { model }).WithTextColumn("Text", x => x.Text, x => x.IsReadOnly = false);
            var core         = (ITreeDataGridSource)source;
            var cell         = Assert.IsAssignableFrom<ITextCell>(core.Rows.RealizeCell(core.Columns[0], 0, 0));

            Assert.Equal("before", cell.Text);
            model.Text = "external";
            Assert.Equal("external", cell.Text);
            ((IEditableObject)cell).BeginEdit();
            cell.Text = "edited";
            Assert.Equal("external", model.Text);
            ((IEditableObject)cell).EndEdit();
            Assert.Equal("edited", model.Text);
            core.Rows.UnrealizeCell(cell, 0, 0);
        }

        [AvaloniaFact]
        public void Hierarchical_Filter_Does_Not_Retain_An_Unmatched_Parent()
        {
            var root         = new Model { Number = 1, Children = { new Model { Number = 2 }, new Model { Number = 3 } } };
            using var source = new HierarchicalTreeDataGridSource<Model>(new[] { root })
                .WithHierarchicalExpanderTextColumn("Number", x => x.Number, x => x.Children);

            source.ExpandAll();
            source.Filter(x => x.Number != 3);
            Assert.Equal(new[] { 1, 2 }, source.Rows.Select(x => ((Model)x.Model!).Number));
            source.Filter(x => x.Number == 2);
            Assert.Empty(source.Rows);
            source.Filter(null);
            Assert.Equal(3, source.Rows.Count);
            source.RowSelection!.Select(new IndexPath(0, 1));
            Assert.Same(root.Children[1], source.RowSelection.SelectedItem);
        }

        [AvaloniaFact]
        public void Row_SelectAll_And_SelectRange_Use_Visible_Sort_Order()
        {
            using var source = new FlatTreeDataGridSource<Model>(Enumerable.Range(0, 5).Select(x => new Model { Number = x }))
                .WithTextColumn("Number", x => x.Number);
            var selection = source.RowSelection!;

            selection.SingleSelect = false;
            source.Filter(x => x.Number != 2);
            source.SortBy(source.Columns[0], ListSortDirection.Descending);
            selection.SelectRange(4, 1);
            Assert.Equal(new[] { 1, 3, 4 }, selection.SelectedIndexes.Select(x => x[0]).OrderBy(x => x));
            selection.Clear();
            selection.SelectAll();
            Assert.Equal(4, selection.Count);
        }

        [AvaloniaFact]
        public void Columns_Cannot_Belong_To_Multiple_Sources()
        {
            var column = new TreeDataGridTextColumn();
            var first  = new TreeDataGridColumns { column };
            var second = new TreeDataGridColumns();

            Assert.Throws<InvalidOperationException>(() => second.Add(column));
            first.Clear();
            second.Add(column);
            Assert.Single(second);
        }

        [AvaloniaFact]
        public void Control_Exposes_Modern_Source_And_Selection()
        {
            var items        = new[] { new Model { Number = 1 }, new Model { Number = 2 } };
            using var source = new FlatTreeDataGridSource<Model>(items).WithTextColumn("Number", x => x.Number);
            var grid         = new TreeDataGrid { Source = source };

            Assert.Same(source.Columns, grid.Columns);
            Assert.Same(source.Rows, grid.Rows);
            grid.SelectedIndex = 1;
            Assert.Same(items[1], grid.SelectedItem);
            grid.SelectedItem = items[0];
            Assert.Equal(new IndexPath(0), source.RowSelection!.SelectedIndex);
            grid.Source = null;
        }

        [AvaloniaFact]
        public void Control_ItemsSource_Uses_Observable_Xaml_Columns()
        {
            var items = new ObservableCollection<Model> { new() { Text = "one" } };
            var grid  = new TreeDataGrid();

            grid.Columns.Add(new TreeDataGridTextColumn { Header = "Text", Binding = new Binding(nameof(Model.Text)) });
            grid.ItemsSource = items;
            Assert.Single(grid.Rows!);
            items.Add(new Model { Text = "two" });
            Assert.Equal(2, grid.Rows!.Count);
            grid.Columns.Add(new TreeDataGridTextColumn { Header = "Number", Binding = new Binding(nameof(Model.Number)) });
            Assert.Equal(2, grid.Columns.Count);
            grid.ItemsSource = null;
        }

        [AvaloniaFact]
        public void Replacing_And_Clearing_An_Expander_Rebuilds_Rows_And_Selection()
        {
            var root         = new Model { Children = { new Model { Text = "child" } } };
            using var source = new HierarchicalTreeDataGridSource<Model>(root)
                .WithHierarchicalExpanderTextColumn("Text", x => x.Text, x => x.Children);

            source.ExpandAll();
            source.RowSelection!.SelectedIndex = new IndexPath(0, 0);
            Assert.Equal(2, source.Rows.Count);
            source.Columns[0] = new TreeDataGridTextColumn { Binding = new Binding(nameof(Model.Text)) };
            Assert.Single(source.Rows);
            Assert.Equal(0, source.RowSelection.Count);
            source.Columns.Clear();
            Assert.Single(source.Rows);
            Assert.True(source.TryGetModelAt(0, out var model));
            Assert.Same(root, model);
            Assert.False(source.TryGetModelAt(new IndexPath(-1), out _));
            Assert.False(source.TryGetModelAt(new IndexPath(0, 0), out _));
        }

        [AvaloniaFact]
        public void Invalid_Expander_Addition_Does_Not_Mutate_Columns()
        {
            using var flat = new FlatTreeDataGridSource<Model>([]);
            Assert.Throws<ArgumentException>(() => flat.Columns.Add(new TreeDataGridHierarchicalExpanderColumn()));
            Assert.Empty(flat.Columns);
            using var tree = new HierarchicalTreeDataGridSource<Model>([])
                .WithHierarchicalExpanderTextColumn("Text", x => x.Text, x => x.Children);
            Assert.Throws<InvalidOperationException>(() => tree.Columns.Add(new TreeDataGridHierarchicalExpanderColumn()));
            Assert.Single(tree.Columns);
        }

        [AvaloniaFact]
        public void Replacing_Binding_Uses_The_New_Value_For_Sorting()
        {
            var first        = new Model { Text = "b", Number = 1 };
            var second       = new Model { Text = "a", Number = 2 };
            using var source = new FlatTreeDataGridSource<Model>(new[] { first, second }).WithTextColumn("Text", x => x.Text);
            var column       = (TreeDataGridTextColumn)source.Columns[0];

            source.SortBy(column, ListSortDirection.Ascending);
            Assert.Same(second, source.Rows[0].Model);
            column.Binding = new Binding(nameof(Model.Number));
            source.SortBy(column, ListSortDirection.Ascending);
            Assert.Same(first, source.Rows[0].Model);
            source.Columns.Remove(column);
            Assert.False(source.IsSorted);
        }

        [AvaloniaFact]
        public void Replacing_ChildrenBinding_Rebuilds_Existing_Rows()
        {
            var root         = new Model { Children = { new Model() } };
            using var source = new HierarchicalTreeDataGridSource<Model>(root)
                .WithHierarchicalExpanderTextColumn("Text", x => x.Text, x => x.Children);

            source.ExpandAll();
            Assert.Equal(2, source.Rows.Count);
            ((TreeDataGridHierarchicalExpanderColumn)source.Columns[0]).ChildrenBinding = null;
            Assert.Single(source.Rows);
            ((TreeDataGridHierarchicalExpanderColumn)source.Columns[0]).ChildrenBinding = new Binding(nameof(Model.Children));
            source.ExpandAll();
            Assert.Equal(2, source.Rows.Count);
        }

        [AvaloniaFact]
        public void Replaced_Selection_Is_Empty_And_Can_Be_Reattached()
        {
            using var source = new FlatTreeDataGridSource<Model>(new[] { new Model() }).WithTextColumn(x => x.Text);
            var cells        = new TreeDataGridCellSelectionModel<Model>(source);

            source.Selection    = cells;
            cells.SelectedIndex = new CellIndex(0, 0);
            source.Selection    = new TreeDataGridRowSelectionModel<Model>(source);
            Assert.Equal(0, cells.Count);
            Assert.Null(cells.Source);
            Assert.Null(cells.SelectedRowItem);
            source.Selection    = cells;
            cells.SelectedIndex = new CellIndex(0, 0);
            Assert.NotNull(cells.SelectedRowItem);
        }

        [AvaloniaFact]
        public void Source_Defaults_Refresh_When_Source_Changes_And_Respect_Control_Overrides()
        {
            using var first  = new FlatTreeDataGridSource<Model>([]) { CanUserReorderColumns = true, FrozenColumnCount = 2 };
            using var second = new FlatTreeDataGridSource<Model>([]);
            var grid         = new TreeDataGrid { Source = first };

            Assert.True(grid.CanUserReorderColumns);
            Assert.Equal(2, grid.FrozenColumnCount);
            first.FrozenColumnCount = 3;
            Assert.Equal(3, grid.FrozenColumnCount);
            grid.FrozenColumnCount = 1;
            grid.Source            = second;
            Assert.False(grid.CanUserReorderColumns);
            Assert.Equal(1, grid.FrozenColumnCount);
            grid.Source = null;
        }

        [AvaloniaFact]
        public void Fluent_Text_Defaults_To_Editable_When_Expression_Is_Writable()
        {
            using var source = new FlatTreeDataGridSource<Model>(new[] { new Model() })
                .WithTextColumn(x => x.Text).WithTextColumn("Computed", x => x.Text.Length);

            Assert.Equal("Text", source.Columns[0].Header);
            Assert.False(((TreeDataGridTextColumn)source.Columns[0]).IsReadOnly);
            Assert.True(((TreeDataGridTextColumn)source.Columns[1]).IsReadOnly);
        }

        #endregion

        #region Nested Types

        private sealed class Model : INotifyPropertyChanged
        {
            #region Fields

            private string _text = string.Empty;

            #endregion

            #region Properties

            public int Number { get; set; }

            public ObservableCollection<Model> Children { get; } = [];

            public string Text
            {
                get => _text;
                set
                {
                    _text = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
                }
            }

            #endregion

            #region Events

            public event PropertyChangedEventHandler? PropertyChanged;

            #endregion
        }

        #endregion
    }
}
