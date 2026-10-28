using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Data;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using BlackSharp.UI.Avalonia.TreeDataGrid.Models;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using Xunit;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Tests
{
    public class PerformanceRegressionTests
    {
        #region Public

        [AvaloniaFact]
        public void Hierarchy_Selectors_Read_Live_Values_And_Are_Invalidated_When_Bindings_Change()
        {
            var model = new Model { Children = { new Model() }, Expanded = true };
            using var source = new HierarchicalTreeDataGridSource<Model>(new[] { model })
                .WithHierarchicalExpanderTextColumn(x => x.Name, x => x.Children, x => x.Expanded, x => x.HasChildren);
            var column = (TreeDataGridHierarchicalExpanderColumn)source.Columns[0];
            Assert.Same(model.Children, column.GetChildModels(model));
            Assert.True(column.HasChildren(model));
            Assert.True(column.GetModelIsExpanded(model));
            model.Children.Clear();
            model.Expanded = false;
            Assert.False(column.HasChildren(model));
            Assert.False(column.GetModelIsExpanded(model));
            model.OtherChildren.Add(new Model());
            column.ChildrenBinding = CompiledBinding.Create((Model x) => x.OtherChildren);
            column.HasChildrenBinding = CompiledBinding.Create((Model x) => x.OtherHasChildren);
            column.IsExpandedBinding = CompiledBinding.Create((Model x) => x.OtherExpanded);
            Assert.Null(column.ChildrenSelector);
            Assert.Null(column.HasChildrenSelector);
            Assert.Null(column.IsExpandedSelector);
            Assert.Same(model.OtherChildren, column.GetChildModels(model));
            Assert.True(column.HasChildren(model));
            Assert.True(column.GetModelIsExpanded(model));
        }

        [AvaloniaFact]
        public void Complex_Null_Binding_Paths_Keep_Their_Unset_Value_Handling()
        {
            using var source = new HierarchicalTreeDataGridSource<Model>(new[] { new Model() })
                .WithHierarchicalExpanderTextColumn(x => x.Name, x => x.Parent!.Children);
            var column = (TreeDataGridHierarchicalExpanderColumn)source.Columns[0];
            Assert.Null(column.ChildrenSelector);
            Assert.Empty(column.GetChildModels(source.Items.First()).Cast<object>());
            Assert.False(column.HasChildren(source.Items.First()));
        }

        [AvaloniaFact]
        public void Failing_Fluent_Getter_Keeps_The_Binding_Fallback_Behavior()
        {
            var model = new ThrowingModel();
            using var source = new HierarchicalTreeDataGridSource<ThrowingModel>(new[] { model })
                .WithHierarchicalExpanderTextColumn(x => x.Name, x => x.Children);
            var optimized = (TreeDataGridHierarchicalExpanderColumn)source.Columns[0];
            var standard = new TreeDataGridHierarchicalExpanderColumn { ChildrenBinding = optimized.ChildrenBinding };
            var expected = Record.Exception(() => standard.GetChildModels(model));
            var actual = Record.Exception(() => optimized.GetChildModels(model));
            Assert.Equal(expected?.GetType(), actual?.GetType());

            if (expected is null)
            {
                Assert.Empty(optimized.GetChildModels(model).Cast<object>());
            }
        }

        [AvaloniaTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void Computed_HasChildren_Updates_On_Collection_Changes_Without_Property_Notifications(bool bindingOnly)
        {
            var model = new Model { Children = { new Model() } };
            using var source = new HierarchicalTreeDataGridSource<Model>(new[] { model })
                .WithHierarchicalExpanderTextColumn(x => x.Name, x => x.Children, hasChildren: x => x.HasChildren);
            var column = (TreeDataGridHierarchicalExpanderColumn)source.Columns[0];

            if (bindingOnly)
            {
                column.HasChildrenBinding = CompiledBinding.Create((Model x) => x.HasChildren);
            }

            var row = (IExpander)((ITreeDataGridSource)source).Rows[0];
            row.IsExpanded = true;
            Assert.True(row.ShowExpander);
            model.Children.Clear();
            Assert.False(row.ShowExpander);
            model.Children.Add(new Model());
            Assert.True(row.ShowExpander);
        }

        [AvaloniaFact]
        public void HasChildren_Does_Not_Enumerate_A_ReadOnly_Collection_With_A_Known_Count()
        {
            var items = new CountingCollection();
            var column = new TreeDataGridHierarchicalExpanderColumn
            {
                ChildrenBinding = CompiledBinding.Create((CountingCollection x) => x),
            };
            Assert.True(column.HasChildren(items));
            Assert.Equal(0, items.Enumerations);
        }

        [AvaloniaTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void Filtered_Index_Lookups_Remain_Correct_After_Sort_Move_And_Insert(bool sorted)
        {
            var items = new ObservableCollection<Model>(Enumerable.Range(0, 100).Select(x => new Model { Number = x }));
            using var source = new FlatTreeDataGridSource<Model>(items).WithTextColumn(x => x.Number);
            source.Filter(x => x.Number % 3 == 0);

            if (sorted)
            {
                source.SortBy(source.Columns[0], ListSortDirection.Descending);
            }

            Check();
            items.Move(0, 40);
            Check();
            items.Insert(0, new Model { Number = 102 });
            Check();
            items.RemoveAt(20);
            Check();
            source.Filter(null);
            Check();

            void Check()
            {
                var visible = source.Rows.Select(x => (Model)x.Model!).ToList();

                for (var i = 0; i < items.Count; ++i)
                {
                    Assert.Equal(visible.IndexOf(items[i]), source.ModelIndexToRowIndex(new IndexPath(i)));
                }

                Assert.Equal(-1, source.ModelIndexToRowIndex(new IndexPath(200)));
            }
        }

        [AvaloniaFact]
        public void Default_String_Format_Retains_Custom_Culture_Formatting()
        {
            var model = new Model();
            using var source = new FlatTreeDataGridSource<Model>(new[] { model })
                .WithTextColumn(x => x.Name, options => options.Culture = new CustomCulture());
            var core = (ITreeDataGridSource)source;
            var cell = core.Rows.RealizeCell(core.Columns[0], 0, 0);
            Assert.Equal("formatted: Sensor", ((ITextCell)cell).Text);
            core.Rows.UnrealizeCell(cell, 0, 0);
        }

        [AvaloniaFact]
        public void Text_Layout_Reuses_Matching_Constraints_And_Tracks_Font_Brush_And_Column_Changes()
        {
            var column = new TreeDataGridTextColumn { UseDrawnCells = true };
            var renderer = (TreeDataGridTextCellRenderer)column.CellRenderer!;
            var owner = new Border();
            var brush = new SolidColorBrush(Colors.Blue);
            TextElement.SetForeground(owner, brush);
            TextElement.SetFontSize(owner, 14);
            var size = new Size(120, 30);
            var first = renderer.GetOrCreateText(owner, "Sensor", size, false);
            Assert.Same(first, renderer.GetOrCreateText(owner, "Sensor", size, true));
            renderer.GetOrCreateText(owner, "Sensor", new Size(double.PositiveInfinity, 30), false);
            Assert.Same(first, renderer.GetOrCreateText(owner, "Sensor", size, true));
            Assert.NotSame(first, renderer.GetOrCreateText(owner, "Updated", size, true));
            var before = renderer.GetOrCreateText(owner, "Sensor", size, true);
            TextElement.SetFontSize(owner, 22);
            var font = renderer.GetOrCreateText(owner, "Sensor", size, true);
            Assert.NotSame(before, font);
            brush.Color = Colors.Red;
            var color = renderer.GetOrCreateText(owner, "Sensor", size, true);
            Assert.NotSame(font, color);
            column.TextWrapping = TextWrapping.Wrap;
            var wrapped = renderer.GetOrCreateText(owner, "Sensor", size, true);
            Assert.NotSame(color, wrapped);
            column.TextAlignment = TextAlignment.Right;
            Assert.NotSame(wrapped, renderer.GetOrCreateText(owner, "Sensor", size, true));
        }

        [AvaloniaFact]
        public void Text_Layout_Cache_Does_Not_Keep_Owner_Controls_Alive()
        {
            var (renderer, reference) = CreateOwnerReference();
            Collect();
            Assert.False(reference.IsAlive);
            GC.KeepAlive(renderer);
        }

        [AvaloniaFact]
        public void Clearing_Text_Layout_Cache_Releases_Previous_Text()
        {
            var (renderer, owner, reference) = CreateClearedTextReference();
            Collect();
            Assert.False(reference.IsAlive);
            GC.KeepAlive(renderer);
            GC.KeepAlive(owner);
        }

        #endregion

        #region Private

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static (TreeDataGridTextCellRenderer, WeakReference) CreateOwnerReference()
        {
            var column = new TreeDataGridTextColumn { UseDrawnCells = true };
            var renderer = (TreeDataGridTextCellRenderer)column.CellRenderer!;
            var owner = new Border();
            renderer.GetOrCreateText(owner, "Sensor", new Size(120, 30), true);
            return (renderer, new WeakReference(owner));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static (TreeDataGridTextCellRenderer, Control, WeakReference) CreateClearedTextReference()
        {
            var column = new TreeDataGridTextColumn { UseDrawnCells = true };
            var renderer = (TreeDataGridTextCellRenderer)column.CellRenderer!;
            var owner = new Border();
            var text = new string('x', 5000);
            renderer.GetOrCreateText(owner, text, new Size(120, 30), true);
            renderer.ClearCache(owner);
            return (renderer, owner, new WeakReference(text));
        }

        private static void Collect()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        #endregion

        #region Nested Types

        private sealed class Model
        {
            public string Name { get; set; } = "Sensor";
            public int Number { get; set; }
            public ObservableCollection<Model> Children { get; } = [];
            public ObservableCollection<Model> OtherChildren { get; } = [];
            public bool Expanded { get; set; }
            public bool OtherExpanded => true;
            public bool HasChildren => Children.Count > 0;
            public bool OtherHasChildren => OtherChildren.Count > 0;
            public Model? Parent { get; set; }
        }

        private sealed class ThrowingModel
        {
            public string Name => "Broken getter";
            public IEnumerable<ThrowingModel> Children => throw new InvalidOperationException("Getter failure");
        }

        private sealed class CountingCollection : IReadOnlyCollection<object>
        {
            public int Count => 1;
            public int Enumerations { get; private set; }
            public IEnumerator<object> GetEnumerator()
            {
                ++Enumerations;
                return new[] { new object() }.AsEnumerable().GetEnumerator();
            }
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private sealed class CustomCulture : CultureInfo, ICustomFormatter
        {
            public CustomCulture() : base("en-US") { }
            public override object? GetFormat(Type? formatType) => formatType == typeof(ICustomFormatter) ? this : base.GetFormat(formatType!);
            public string Format(string? format, object? arg, IFormatProvider? formatProvider) => "formatted: " + arg;
        }

        #endregion
    }
}
