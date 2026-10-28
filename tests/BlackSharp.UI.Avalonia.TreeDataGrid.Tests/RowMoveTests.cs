using Avalonia.Headless.XUnit;
using BlackSharp.UI.Avalonia.TreeDataGrid.Models;
using BlackSharp.UI.Avalonia.TreeDataGrid.Selection;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using Xunit;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Tests
{
    public class RowMoveTests
    {
        #region Public

        [AvaloniaTheory]
        [InlineData(false, false, false, 0, 3, 1)]
        [InlineData(false, false, false, 3, 0, 1)]
        [InlineData(false, true, false, 0, 2, 2)]
        [InlineData(false, true, false, 2, 0, 2)]
        [InlineData(true, false, false, 0, 3, 1)]
        [InlineData(true, false, false, 3, 0, 1)]
        [InlineData(true, true, false, 0, 2, 2)]
        [InlineData(true, true, false, 2, 0, 2)]
        [InlineData(false, false, true, 0, 3, 1)]
        [InlineData(false, true, true, 3, 0, 1)]
        [InlineData(false, true, true, 0, 2, 2)]
        [InlineData(true, false, true, 0, 3, 1)]
        [InlineData(true, true, true, 3, 0, 1)]
        [InlineData(true, true, true, 2, 0, 2)]
        public void Moving_Branches_Preserves_Row_Identity_Expansion_And_Selection(
            bool nested, bool sorted, bool equalKeys, int oldIndex, int newIndex, int count)
        {
            var branches = new MovingCollection<Node>(Enumerable.Range(0, 4).Select(i => new Node
            {
                Number = equalKeys ? 0 : 3 - i,
                Children = { new Node { Children = { new Node() } }, new Node() },
            }));
            var roots = nested ? new MovingCollection<Node>(new[] { new Node { Children = branches } }) : branches;
            using var source = new HierarchicalTreeDataGridSource<Node>(roots)
                .WithHierarchicalExpanderTextColumn(x => x.Number, x => x.Children);

            source.ExpandAll();

            if (sorted)
            {
                source.Sort((a, b) => a.Number.CompareTo(b.Number));
            }

            var rows      = source.Engine.Rows;
            var identities = rows.ToDictionary(x => (Node)x.Model!, x => x);
            var expanded   = rows.OfType<HierarchicalRow<Node>>().Where(x => x.IsExpanded).Select(x => x.Model).ToArray();
            var moved      = branches[oldIndex];
            var selected   = moved.Children[0].Children[0];
            var sibling    = branches[(oldIndex + count) % branches.Count].Children[1];
            var selection  = source.RowSelection!;

            selection.SingleSelect = false;
            selection.Select(FindPath(roots, selected));
            selection.Select(FindPath(roots, sibling));
            var anchorItem = sibling;
            var selectedItem = selection.SelectedItem;
            var selectionChanges = 0;
            var actions = new List<NotifyCollectionChangedAction>();

            selection.SelectionChanged += (s, e) => ++selectionChanges;
            source.Rows.CollectionChanged += (s, e) => actions.Add(e.Action);
            branches.MoveRange(oldIndex, newIndex, count);

            Assert.Equal(identities.Count, rows.Count);
            Assert.Equal(Flatten(roots, sorted), rows.Select(x => (Node)x.Model!));

            foreach (var row in rows.OfType<HierarchicalRow<Node>>())
            {
                Assert.Same(identities[row.Model], row);
                Assert.Equal(FindPath(roots, row.Model), row.ModelIndexPath);
                Assert.Equal(expanded.Contains(row.Model), row.IsExpanded);
            }

            Assert.Equal(2, selection.Count);
            Assert.Same(selectedItem, selection.SelectedItem);
            Assert.Equal(FindPath(roots, selectedItem!), selection.SelectedIndex);
            Assert.Equal(FindPath(roots, anchorItem!), selection.AnchorIndex);
            Assert.True(selection.IsSelected(FindPath(roots, selected)));
            Assert.True(selection.IsSelected(FindPath(roots, sibling)));
            Assert.Equal(0, selectionChanges);
            Assert.All(actions, action => Assert.Equal(NotifyCollectionChangedAction.Move, action));

            // Child subscriptions must follow the same model after its ancestor moves.
            moved.Children.Add(new Node());
            Assert.Equal(identities.Count + 1, rows.Count);
            Assert.Equal(Flatten(roots, sorted), rows.Select(x => (Node)x.Model!));
        }

        [AvaloniaFact]
        public void Disposed_Row_Header_Unsubscribes_From_The_Retained_Row()
        {
            var (source, reference) = CreateDisposedRowHeaderReference();

            try
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                Assert.False(reference.IsAlive);
                GC.KeepAlive(source);
            }
            finally
            {
                source.Dispose();
            }
        }

        [AvaloniaFact]
        public void All_Block_Moves_Preserve_Mixed_Selection_And_Anchors()
        {
            for (var count = 1; count <= 3; ++count)
            {
                for (var oldIndex = 0; oldIndex <= 4 - count; ++oldIndex)
                {
                    for (var newIndex = 0; newIndex <= 4 - count; ++newIndex)
                    {
                        for (var mask = 1; mask < 16; ++mask)
                        {
                            var items = new MovingCollection<Node>(Enumerable.Range(0, 4)
                                .Select(i => new Node { Children = { new Node() } }));
                            using var source = new HierarchicalTreeDataGridSource<Node>(items)
                                .WithHierarchicalExpanderTextColumn(x => x.Number, x => x.Children);
                            var selection = source.RowSelection!;
                            var selected = new List<Node>();
                            selection.SingleSelect = false;

                            for (var index = 0; index < items.Count; ++index)
                            {
                                if ((mask & (1 << index)) != 0)
                                {
                                    var node = index % 2 == 0 ? items[index] : items[index].Children[0];
                                    selected.Add(node);
                                    selection.Select(FindPath(items, node));
                                }
                            }

                            selection.RangeAnchorIndex = FindPath(items, selected[0]);
                            items.MoveRange(oldIndex, newIndex, count);

                            Assert.Equal(selected.Count, selection.Count);
                            Assert.Same(selected[0], selection.SelectedItem);
                            Assert.Equal(FindPath(items, selected[0]), selection.SelectedIndex);
                            Assert.Equal(FindPath(items, selected[^1]), selection.AnchorIndex);
                            Assert.Equal(FindPath(items, selected[0]), selection.RangeAnchorIndex);
                            Assert.All(selected, node => Assert.True(selection.IsSelected(FindPath(items, node))));
                            Assert.All(selection.SelectedItems, item => Assert.Contains(item, selected));
                        }
                    }
                }
            }
        }

        [AvaloniaTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void Sorted_Flat_Moves_Use_Only_Move_Notifications(bool equalKeys)
        {
            var items = new ObservableCollection<Node>(Enumerable.Range(0, 4)
                .Select(i => new Node { Number = equalKeys ? 0 : 3 - i }));
            using var source = new FlatTreeDataGridSource<Node>(items).WithTextColumn(x => x.Number);
            source.SortBy(source.Columns[0], System.ComponentModel.ListSortDirection.Ascending);
            var selected = items[0];
            source.RowSelection!.Select(0);
            var actions = new List<NotifyCollectionChangedAction>();
            _ = source.Rows[0];
            source.Rows.CollectionChanged += (s, e) => actions.Add(e.Action);

            items.Move(0, 3);

            Assert.Equal(items.OrderBy(x => x.Number), source.Rows.Select(x => (Node)x.Model!));
            Assert.Same(selected, source.RowSelection.SelectedItem);
            Assert.Equal(new IndexPath(3), source.RowSelection.SelectedIndex);
            Assert.All(actions, action => Assert.Equal(NotifyCollectionChangedAction.Move, action));
        }

        [AvaloniaFact]
        public void Moving_Selected_Flat_Item_Preserves_Cell_Selection_And_Reports_New_Indexes()
        {
            var items = new ObservableCollection<Node> { new Node(), new Node(), new Node() };
            using var source = new FlatTreeDataGridSource<Node>(items).WithTextColumn(x => x.Number);
            var selection = new TreeDataGridCellSelectionModel<Node>(source);

            source.Selection = selection;
            selection.SelectedIndex = new CellIndex(0, new IndexPath(0));
            var changed = 0;
            var indexesChanged = 0;
            selection.SelectionChanged += (s, e) => ++changed;
            selection.IndexesChanged += (s, e) => ++indexesChanged;
            var item = items[0];

            items.Move(0, 2);

            Assert.Equal(new CellIndex(0, new IndexPath(2)), selection.SelectedIndex);
            Assert.Same(item, selection.SelectedRowItem);
            Assert.Equal(1, selection.Count);
            Assert.Equal(0, changed);
            Assert.Equal(1, indexesChanged);
        }

        #endregion

        #region Private

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static (HierarchicalTreeDataGridSource<Node>, WeakReference) CreateDisposedRowHeaderReference()
        {
            var source = new HierarchicalTreeDataGridSource<Node>(new[] { new Node() })
                .WithRowHeaderColumn().WithHierarchicalExpanderTextColumn(x => x.Number, x => x.Children);
            var core = (ITreeDataGridSource)source;
            var cell = core.Rows.RealizeCell(core.Columns[0], 0, 0);
            var reference = new WeakReference(cell);

            core.Rows.UnrealizeCell(cell, 0, 0);

            return (source, reference);
        }

        private static IEnumerable<Node> Flatten(IEnumerable<Node> nodes, bool sorted)
        {
            foreach (var node in sorted ? nodes.OrderBy(x => x.Number) : nodes)
            {
                yield return node;

                foreach (var child in Flatten(node.Children, sorted))
                {
                    yield return child;
                }
            }
        }

        private static IndexPath FindPath(IEnumerable<Node> nodes, Node target, IndexPath parent = default)
        {
            var index = 0;

            foreach (var node in nodes)
            {
                var path = parent.Append(index++);

                if (ReferenceEquals(node, target))
                {
                    return path;
                }

                var child = FindPath(node.Children, target, path);

                if (child != default)
                {
                    return child;
                }
            }

            return default;
        }

        #endregion

        #region Nested Types

        private sealed class Node
        {
            public int Number { get; init; }

            public MovingCollection<Node> Children { get; init; } = new(Array.Empty<Node>());
        }

        private sealed class MovingCollection<T>(IEnumerable<T> items) : ObservableCollection<T>(items)
        {
            public void MoveRange(int oldIndex, int newIndex, int count)
            {
                var moved = this.Skip(oldIndex).Take(count).ToArray();

                for (var i = 0; i < count; ++i)
                {
                    Items.RemoveAt(oldIndex);
                }

                for (var i = 0; i < count; ++i)
                {
                    Items.Insert(newIndex + i, moved[i]);
                }

                OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Move, moved, newIndex, oldIndex));
            }
        }

        #endregion
    }
}
