using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using Avalonia.Threading;
using Avalonia.VisualTree;
using BlackSharp.UI.Avalonia.TreeDataGrid.Primitives;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xunit;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Tests
{
    public class ModernControlTests
    {
        #region Public

        [AvaloniaFact]
        public void ScrollViewer_Tracks_AllowAutoHide_On_Grid()
        {
            // Arrange
            using var source = CreateInteractionSource(new Model());
            var grid = new TreeDataGrid { Source = source };
            ScrollViewer.SetAllowAutoHide(grid, false);

            // Act
            var window = Show(grid);

            try
            {
                var scrollViewer = Assert.IsType<ScrollViewer>(grid.Scroll);

                // Assert
                Assert.False(scrollViewer.AllowAutoHide);

                // Act
                ScrollViewer.SetAllowAutoHide(grid, true);
                Layout(window);

                // Assert
                Assert.True(scrollViewer.AllowAutoHide);
            }
            finally
            {
                window.Close();
            }
        }

        [AvaloniaFact]
        public void ScrollViewer_Tracks_Scrolling_Properties_On_Grid()
        {
            // Arrange
            using var source = CreateInteractionSource(new Model());
            var grid = new TreeDataGrid { Source = source };

            // Act
            var window = Show(grid);

            try
            {
                var scrollViewer = Assert.IsType<ScrollViewer>(grid.Scroll);

                // Assert
                Assert.Equal(ScrollBarVisibility.Auto, scrollViewer.HorizontalScrollBarVisibility);
                Assert.Equal(ScrollBarVisibility.Auto, scrollViewer.VerticalScrollBarVisibility);

                // Act
                ScrollViewer.SetHorizontalScrollBarVisibility(grid, ScrollBarVisibility.Disabled);
                ScrollViewer.SetVerticalScrollBarVisibility(grid, ScrollBarVisibility.Visible);
                ScrollViewer.SetHorizontalSnapPointsType(grid, SnapPointsType.Mandatory);
                ScrollViewer.SetVerticalSnapPointsType(grid, SnapPointsType.MandatorySingle);
                ScrollViewer.SetHorizontalSnapPointsAlignment(grid, SnapPointsAlignment.Center);
                ScrollViewer.SetVerticalSnapPointsAlignment(grid, SnapPointsAlignment.Far);
                ScrollViewer.SetIsScrollChainingEnabled(grid, false);
                ScrollViewer.SetIsScrollInertiaEnabled(grid, false);
                ScrollViewer.SetIsDeferredScrollingEnabled(grid, true);
                Layout(window);

                // Assert
                Assert.Equal(ScrollBarVisibility.Disabled, scrollViewer.HorizontalScrollBarVisibility);
                Assert.Equal(ScrollBarVisibility.Visible, scrollViewer.VerticalScrollBarVisibility);
                Assert.Equal(SnapPointsType.Mandatory, scrollViewer.HorizontalSnapPointsType);
                Assert.Equal(SnapPointsType.MandatorySingle, scrollViewer.VerticalSnapPointsType);
                Assert.Equal(SnapPointsAlignment.Center, scrollViewer.HorizontalSnapPointsAlignment);
                Assert.Equal(SnapPointsAlignment.Far, scrollViewer.VerticalSnapPointsAlignment);
                Assert.False(scrollViewer.IsScrollChainingEnabled);
                Assert.False(scrollViewer.IsScrollInertiaEnabled);
                Assert.True(scrollViewer.IsDeferredScrollingEnabled);
            }
            finally
            {
                window.Close();
            }
        }

        [AvaloniaTheory]
        [InlineData(false, 0)]
        [InlineData(true, 0)]
        [InlineData(true, 1)]
        public void DoubleTap_Toggles_From_Any_ReadOnly_Cell_When_Enabled(bool enabled, int column)
        {
            var root = new Model { Children = { new Model() } };
            using var source = CreateInteractionSource(root);
            var grid = new TreeDataGrid { Source = source, ExpandCollapseOnDoubleTap = enabled };
            var window = Show(grid);

            try
            {
                DoubleTap(window, grid.TryGetCell(column, 0)!);
                Layout(window);
                Assert.Equal(enabled ? 2 : 1, source.Rows.Count);

                if (enabled)
                {
                    DoubleTap(window, grid.TryGetCell(column, 0)!);
                    Layout(window);
                    Assert.Single(source.Rows);
                }
            }
            finally
            {
                window.Close();
            }
        }

        [AvaloniaFact]
        public void DoubleTap_Editing_Takes_Precedence_Over_Expansion()
        {
            var root = new Model { Children = { new Model() } };
            using var source = CreateInteractionSource(root, editable: true);
            var grid = new TreeDataGrid { Source = source, ExpandCollapseOnDoubleTap = true };
            var window = Show(grid);

            try
            {
                var cell = Assert.IsType<TreeDataGridTextCell>(grid.TryGetCell(1, 0));

                DoubleTap(window, cell);
                Layout(window);
                Assert.True(cell.IsEditing);
                Assert.Single(source.Rows);
                var editor = cell.GetVisualDescendants().OfType<TextBox>().Single();

                editor.Text = "edited";
                DoubleTap(window, editor);
                Assert.True(cell.IsEditing);
                Assert.Single(source.Rows);
                Assert.True(grid.CommitEdit());
                Assert.Equal("edited", root.Other);
            }
            finally
            {
                window.Close();
            }
        }

        [AvaloniaTheory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public void Embedded_Controls_Retain_DoubleTap_And_Arrow_Keys(int controlKind)
        {
            var root = new Model { Children = { new Model() } };
            using var source = CreateInteractionSource(root);
            source.Columns[1] = new TreeDataGridTemplateColumn
            {
                Width = new GridLength(120),
                CellTemplate = new FuncDataTemplate<Model>((model, _) => controlKind switch
                {
                    0 => new Button { Content = "Action" },
                    1 => new TextBox { Text = "editable" },
                    _ => new NumericUpDown { Value = 50 },
                }),
            };
            var grid = new TreeDataGrid
            {
                Source = source,
                ExpandCollapseOnDoubleTap = true,
                SelectParentOnLeftArrow = true,
                PreserveSelectionOnCtrlArrowScroll = true,
            };
            var window = Show(grid);

            try
            {
                var editor = grid.TryGetCell(1, 0)!.GetVisualDescendants()
                    .First(x => x is Button or TextBox or NumericUpDown) as Control;
                DoubleTap(window, editor!);
                Assert.Single(source.Rows);
                source.Expand(0);
                Layout(window);
                grid.SelectedIndex = new IndexPath(0, 0);
                editor!.RaiseEvent(new KeyEventArgs { RoutedEvent = InputElement.KeyDownEvent, Key = Key.Left });
                Assert.Equal(new IndexPath(0, 0), grid.SelectedIndex);
                var scroll = new KeyEventArgs
                {
                    RoutedEvent = InputElement.KeyDownEvent,
                    Key = Key.Down,
                    KeyModifiers = KeyModifiers.Control,
                };
                editor.RaiseEvent(scroll);
                Assert.Equal(new IndexPath(0, 0), grid.SelectedIndex);
                Assert.Equal(2, source.Rows.Count);
            }
            finally
            {
                window.Close();
            }
        }

        [AvaloniaTheory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void LeftArrow_Closes_First_Then_Optionally_Selects_Parent(bool enabled, bool cells)
        {
            var child = new Model { Children = { new Model() } };
            var root = new Model { Children = { child } };
            using var source = CreateInteractionSource(root);
            source.ExpandAll();
            var grid = new TreeDataGrid
            {
                Source = source,
                SelectParentOnLeftArrow = enabled,
                SelectionMode = cells ? TreeDataGridSelectionMode.Cell : TreeDataGridSelectionMode.Row,
            };
            var window = Show(grid);

            try
            {
                grid.SelectedIndex = new IndexPath(0, 0);
                var cell = grid.TryGetCell(0, 1)!;
                cell.Focus();
                cell.RaiseEvent(new KeyEventArgs { RoutedEvent = InputElement.KeyDownEvent, Key = Key.Left });
                Layout(window);
                Assert.Equal(2, source.Rows.Count);
                Assert.Same(child, grid.SelectedItem);
                grid.TryGetCell(0, 1)!.RaiseEvent(new KeyEventArgs { RoutedEvent = InputElement.KeyDownEvent, Key = Key.Left });
                Assert.Same(enabled ? root : child, grid.SelectedItem);

                if (enabled)
                {
                    Assert.True(grid.TryGetCell(0, 0)!.IsKeyboardFocusWithin);
                }

                grid.TryGetCell(0, 0)!.RaiseEvent(new KeyEventArgs { RoutedEvent = InputElement.KeyDownEvent, Key = Key.Left });
                Assert.NotEqual(default, grid.SelectedIndex);
            }
            finally
            {
                window.Close();
            }
        }

        [AvaloniaTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void CtrlArrow_Scrolls_Without_Moving_Selection_Or_Focus_When_Enabled(bool enabled)
        {
            using var source = new FlatTreeDataGridSource<Model>(Enumerable.Range(0, 100).Select(x => new Model()))
                .WithTextColumn(x => x.Text);
            var grid = new TreeDataGrid { Source = source, PreserveSelectionOnCtrlArrowScroll = enabled };
            var window = Show(grid);

            try
            {
                grid.SelectedIndex = 0;
                Assert.True(grid.TryGetCell(0, 0)!.Focus());
                grid.Scroll!.Offset = new Vector(0, 300);
                Layout(window);
                var before = grid.Scroll.Offset.Y;
                var focus = Assert.IsAssignableFrom<Control>(window.FocusManager!.GetFocusedElement());
                var down = new KeyEventArgs
                {
                    RoutedEvent = InputElement.KeyDownEvent, Key = Key.Down, KeyModifiers = KeyModifiers.Control,
                };
                focus.RaiseEvent(down);
                Layout(window);
                Assert.Equal(new IndexPath(0), grid.SelectedIndex);

                if (enabled)
                {
                    Assert.True(down.Handled);
                    Assert.Same(focus, window.FocusManager.GetFocusedElement());
                    Assert.True(grid.Scroll.Offset.Y > before);
                    focus.RaiseEvent(new KeyEventArgs
                    {
                        RoutedEvent = InputElement.KeyDownEvent, Key = Key.Up, KeyModifiers = KeyModifiers.Control,
                    });
                    Layout(window);
                    Assert.Equal(before, grid.Scroll.Offset.Y);
                }
            }
            finally
            {
                window.Close();
            }
        }

        [AvaloniaFact]
        public void Focused_Editor_Survives_Moves_Out_Of_And_Back_Into_The_Viewport()
        {
            var items = new ObservableCollection<Model>(Enumerable.Range(0, 100).Select(x => new Model()));
            var moved = items[0];
            using var source = new FlatTreeDataGridSource<Model>(items).WithTextColumn(x => x.Text);
            var grid = new TreeDataGrid { Source = source };
            var window = Show(grid);

            try
            {
                grid.SelectedIndex = 0;
                var cell = Assert.IsType<TreeDataGridTextCell>(grid.TryGetCell(0, 0));
                Assert.True(grid.BeginEdit(0, 0));
                Layout(window);
                var editor = cell.GetVisualDescendants().OfType<TextBox>().Single();
                editor.Focus();
                editor.Text = "pending";

                items.Move(0, 99);
                Layout(window);
                Assert.True(cell.IsEditing);
                Assert.Equal(99, cell.RowIndex);
                Assert.Equal(new IndexPath(99), grid.SelectedIndex);
                items.Move(99, 98);
                Assert.Equal(98, cell.RowIndex);
                items.Move(98, 0);
                Layout(window);
                Assert.Same(cell, grid.TryGetCell(0, 0));
                Assert.True(cell.IsEditing);
                Assert.Same(editor, window.FocusManager!.GetFocusedElement());
                Assert.True(grid.CommitEdit());
                Assert.Equal("pending", moved.Text);
            }
            finally
            {
                window.Close();
            }
        }

        [AvaloniaFact]
        public void Moving_A_Visible_Branch_Preserves_Containers_And_Grid_Selection()
        {
            var moved = new Model { Children = { new Model() } };
            var items = new ObservableCollection<Model> { moved, new Model(), new Model() };
            using var source = new HierarchicalTreeDataGridSource<Model>(items)
                .WithHierarchicalExpanderTextColumn(x => x.Text, x => x.Children);
            source.Expand(0);
            var grid = new TreeDataGrid { Source = source };
            var window = Show(grid);

            try
            {
                grid.SelectedIndex = new IndexPath(0, 0);
                var row = grid.TryGetRow(0);
                var cell = grid.TryGetCell(0, 0);
                var child = grid.TryGetRow(1);
                items.Move(0, 2);
                Layout(window);
                Assert.Same(row, grid.TryGetRow(2));
                Assert.Same(cell, grid.TryGetCell(0, 2));
                Assert.Same(child, grid.TryGetRow(3));
                Assert.Equal(new IndexPath(2, 0), grid.SelectedIndex);
                Assert.Same(moved.Children[0], grid.SelectedItem);
                Assert.True(grid.TryGetRow(3)!.IsSelected);
            }
            finally
            {
                window.Close();
            }
        }

        [AvaloniaFact]
        public void Frozen_Columns_Remain_Visible_Without_Realizing_Intervening_Columns()
        {
            using var source = new FlatTreeDataGridSource<Model>(new[] { new Model() });

            for (var i = 0; i < 30; ++i)
            {
                source.WithTextColumn(i, x => x.Text, x => x.Width = new GridLength(100));
            }

            source.FrozenColumnCount = 1;
            var grid                 = new TreeDataGrid { Source = source };
            var window               = Show(grid);

            try
            {
                var before = grid.TryGetCell(0, 0)!.TranslatePoint(default, grid)!.Value;

                grid.Scroll!.Offset = new Vector(1800, 0);
                Layout(window);
                var after = grid.TryGetCell(0, 0)!.TranslatePoint(default, grid)!.Value;

                Assert.Equal(before.X, after.X, 2);
                Assert.InRange(grid.TryGetRow(0)!.CellsPresenter!.GetRealizedElements().Count(), 2, 7);
                Assert.NotNull(grid.TryGetCell(18, 0));
            }
            finally
            {
                window.Close();
            }
        }

        [AvaloniaFact]
        public void Frozen_Cells_Have_No_Duplicate_Visible_Controls_After_Relayout()
        {
            using var source = new FlatTreeDataGridSource<Model>(new[] { new Model() });

            for (var i = 0; i < 6; ++i)
            {
                source.WithTextColumn(i, x => x.Text, x => x.Width = new GridLength(100));
            }

            var grid   = new TreeDataGrid { Source = source, FrozenColumnCount = 2 };
            var window = Show(grid);

            try
            {
                for (var pass = 0; pass < 3; ++pass)
                {
                    grid.ColumnHeadersPresenter!.InvalidateMeasure();
                    grid.TryGetRow(0)!.CellsPresenter!.InvalidateMeasure();
                    Layout(window);
                    AssertAllVisibleChildrenAreRealized(grid.ColumnHeadersPresenter);
                    AssertAllVisibleChildrenAreRealized(grid.TryGetRow(0)!.CellsPresenter!);
                }
            }
            finally
            {
                window.Close();
            }
        }

        [AvaloniaFact]
        public void TextInput_Starts_Editing_And_Tab_Commits_Then_Moves_To_Next_Editable_Cell()
        {
            var model        = new Model { Text = "before" };
            using var source = new FlatTreeDataGridSource<Model>(new[] { model })
                .WithTextColumn("First", x => x.Text, x =>
                {
                    x.IsReadOnly        = false;
                    x.Width             = new GridLength(110);
                    x.BeginEditGestures |= BeginEditGestures.TextInput;
                })
                .WithTextColumn("Second", x => x.Other, x => x.IsReadOnly = false);
            var grid   = new TreeDataGrid { Source = source };
            var window = Show(grid);

            try
            {
                var cell = Assert.IsType<TreeDataGridTextCell>(grid.TryGetCell(0, 0));

                cell.Focus();
                window.KeyTextInput("Z");
                Layout(window);
                Assert.True(cell.IsEditing);
                Assert.Equal("Z", cell.GetVisualDescendants().OfType<TextBox>().First().Text);
                cell.RaiseEvent(new KeyEventArgs { RoutedEvent = InputElement.KeyDownEvent, Key = Key.Tab });
                Layout(window);
                Assert.Equal("Z", model.Text);
                Assert.False(cell.IsEditing);
                Assert.True(Assert.IsType<TreeDataGridTextCell>(grid.TryGetCell(1, 0)).IsEditing);
                grid.CancelEdit();
            }
            finally
            {
                window.Close();
            }
        }

        [AvaloniaFact]
        public void Drawn_Text_Cells_Are_Editable_And_Use_The_Renderer()
        {
            var model        = new Model { Text = "drawn" };
            using var source = new FlatTreeDataGridSource<Model>(new[] { model })
                .WithTextColumn("Text", x => x.Text, x => { x.UseDrawnCells = true; x.IsReadOnly = false; });
            var grid   = new TreeDataGrid { Source = source };
            var window = Show(grid);

            try
            {
                var cell = Assert.IsType<TreeDataGridDrawnTextCell>(grid.TryGetCell(0, 0));

                Assert.Single(cell.GetVisualDescendants().OfType<TreeDataGridDrawnCellContent>());
                Assert.True(grid.BeginEdit(0, 0));
                Layout(window);
                var editor = Assert.Single(cell.GetVisualDescendants().OfType<TextBox>());

                editor.Text = "changed";
                Assert.True(grid.CommitEdit());
                Assert.Equal("changed", model.Text);
            }
            finally
            {
                window.Close();
            }
        }

        [AvaloniaFact]
        public void Header_Sorting_Cycles_Ascending_Descending_Unsorted()
        {
            using var source = new FlatTreeDataGridSource<Model>(new[] { new Model { Text = "b" }, new Model { Text = "a" } })
                .WithTextColumn("Text", x => x.Text);
            var grid   = new TreeDataGrid { Source = source, AllowTriStateSorting = true };
            var window = Show(grid);

            try
            {
                var header = Assert.IsType<TreeDataGridColumnHeader>(grid.ColumnHeadersPresenter!.TryGetElement(0));

                header.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Assert.Equal(ListSortDirection.Ascending, source.Columns[0].SortDirection);
                header.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Assert.Equal(ListSortDirection.Descending, source.Columns[0].SortDirection);
                header.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Assert.False(source.IsSorted);
                Assert.Null(source.Columns[0].SortDirection);
            }
            finally
            {
                window.Close();
            }
        }

        [AvaloniaFact]
        public void Reordering_Is_Cancellable_And_Cannot_Cross_The_Frozen_Boundary()
        {
            using var source = new FlatTreeDataGridSource<Model>(new[] { new Model() })
                .WithTextColumn("First", x => x.Text).WithTextColumn("Second", x => x.Text).WithTextColumn("Third", x => x.Text);
            var grid   = new TreeDataGrid { Source = source, CanUserReorderColumns = true, FrozenColumnCount = 1 };
            var first  = source.Columns[1];
            var raised = 0;

            Assert.False(grid.TryReorderColumn(1, 0));
            grid.ColumnReordered += (s, e) => ++raised;
            Assert.True(grid.TryReorderColumn(1, 2));
            Assert.Same(first, source.Columns[2]);
            Assert.Equal(1, raised);
            grid.ColumnReordering += (s, e) => e.Cancel = true;
            Assert.False(grid.TryReorderColumn(2, 1));
            Assert.Equal(1, raised);
            grid.Source = null;
        }

        [AvaloniaFact]
        public void Automation_Providers_Select_Edit_And_Toggle()
        {
            var model        = new Model { Text = "before" };
            using var source = new FlatTreeDataGridSource<Model>(new[] { model })
                .WithTextColumn("Text", x => x.Text, x => x.IsReadOnly = false)
                .WithCheckBoxColumn("Flag", x => x.Flag);
            var grid   = new TreeDataGrid { Source = source };
            var window = Show(grid);

            try
            {
                var peer = ControlAutomationPeer.CreatePeerForElement(grid)!;

                Assert.True(peer.IsContentElement());
                Assert.True(peer.IsControlElement());
                Assert.NotNull(peer.GetProvider<ISelectionProvider>());
                Assert.NotNull(peer.GetProvider<IScrollProvider>());
                ControlAutomationPeer.CreatePeerForElement(grid.TryGetRow(0)!)!.GetProvider<ISelectionItemProvider>()!.Select();
                Assert.Same(model, grid.SelectedItem);
                var value = ControlAutomationPeer.CreatePeerForElement(grid.TryGetCell(0, 0)!)!.GetProvider<IValueProvider>()!;

                value.SetValue("automation");
                Assert.Equal("automation", model.Text);
                ControlAutomationPeer.CreatePeerForElement(grid.TryGetCell(1, 0)!)!.GetProvider<IToggleProvider>()!.Toggle();
                Assert.True(model.Flag);
            }
            finally
            {
                window.Close();
            }
        }

        [AvaloniaFact]
        public void Expander_Binding_Changes_Apply_Without_Realizing_A_Cell()
        {
            var root         = new Model { Children = { new Model() } };
            using var source = new HierarchicalTreeDataGridSource<Model>(new[] { root })
                .WithHierarchicalExpanderTextColumn("Text", x => x.Text, x => x.Children, x => x.Expanded);

            Assert.Single(source.Rows);
            root.Expanded = true;
            Assert.Equal(2, source.Rows.Count);
            root.Expanded = false;
            Assert.Single(source.Rows);
        }

        [AvaloniaFact]
        public void Long_Lived_Source_Does_Not_Keep_An_Unused_Grid_Alive()
        {
            using var source = new FlatTreeDataGridSource<Model>(new[] { new Model() }).WithTextColumn("Text", x => x.Text);
            var reference    = CreateGridReference(source);

            Collect();
            Assert.False(reference.IsAlive);
            GC.KeepAlive(source);
        }

        [AvaloniaFact]
        public void Disposed_Source_Releases_Its_Last_Realized_Model()
        {
            var (source, reference) = CreateDisposedSourceReference();

            Collect();
            Assert.False(reference.IsAlive);
            GC.KeepAlive(source);
        }

        [AvaloniaFact]
        public void Old_Public_Interfaces_And_Generic_Columns_Are_Removed()
        {
            var names = typeof(TreeDataGrid).Assembly.GetExportedTypes().Select(x => x.Name).ToArray();

            Assert.DoesNotContain("ITreeDataGridSource", names);
            Assert.DoesNotContain("IColumn", names);
            Assert.DoesNotContain("TextColumn`2", names);
            Assert.DoesNotContain(names, x => x.StartsWith("Legacy", StringComparison.Ordinal));
        }

        [AvaloniaFact]
        public void Invalid_Number_Stays_In_The_Editor_And_Cancel_Restores_The_Value()
        {
            var model        = new Model { Number = 12.5 };
            using var source = new FlatTreeDataGridSource<Model>(new[] { model }).WithTextColumn("Number", x => x.Number, x =>
            {
                x.IsReadOnly   = false;
                x.Culture      = System.Globalization.CultureInfo.GetCultureInfo("de-DE");
                x.StringFormat = "{0:N1} °C";
            });
            var grid   = new TreeDataGrid { Source = source };
            var window = Show(grid);

            try
            {
                Assert.True(grid.BeginEdit(0, 0));
                Layout(window);
                var cell   = Assert.IsType<TreeDataGridTextCell>(grid.TryGetCell(0, 0));
                var editor = cell.GetVisualDescendants().OfType<TextBox>().First();

                Assert.Equal("12,5", editor.Text);
                editor.Text = "invalid";
                Assert.False(grid.CommitEdit());
                Assert.True(cell.IsEditing);
                Assert.Equal(12.5, model.Number);
                Assert.True(DataValidationErrors.GetHasErrors(editor));
                grid.CancelEdit();
                Assert.Equal("12,5 °C", cell.Value);
                Assert.True(grid.BeginEdit(0, 0));
                Layout(window);
                cell.GetVisualDescendants().OfType<TextBox>().First().Text = "13,5";
                Assert.True(grid.CommitEdit());
                Assert.Equal(13.5, model.Number);
            }
            finally
            {
                window.Close();
            }
        }

        [AvaloniaFact]
        public void Changing_Drawn_Mode_Recycles_The_Inner_Expander_Control()
        {
            using var source = new HierarchicalTreeDataGridSource<Model>(new[] { new Model() })
                .WithHierarchicalExpanderTextColumn("Text", x => x.Text, x => x.Children, options: x => x.UseDrawnCells = true);
            var grid   = new TreeDataGrid { Source = source };
            var window = Show(grid);

            try
            {
                Assert.Single(grid.GetVisualDescendants().OfType<TreeDataGridDrawnTextCell>());
                var column = (TreeDataGridTextColumn)((TreeDataGridHierarchicalExpanderColumn)source.Columns[0]).Inner!;

                column.UseDrawnCells = false;
                Layout(window);
                Assert.Empty(grid.GetVisualDescendants().OfType<TreeDataGridDrawnTextCell>());
                Assert.Single(grid.GetVisualDescendants().OfType<TreeDataGridTextCell>());
                column.UseDrawnCells = true;
                Layout(window);
                Assert.Single(grid.GetVisualDescendants().OfType<TreeDataGridDrawnTextCell>());
            }
            finally
            {
                window.Close();
            }
        }

        [AvaloniaFact]
        public void Star_Columns_Respect_Both_Minimum_And_Maximum_On_First_Layout()
        {
            using var source = new FlatTreeDataGridSource<Model>(new[] { new Model() })
                .WithTextColumn("Minimum", x => x.Text, x => { x.Width = GridLength.Star; x.MinWidth = new GridLength(200); })
                .WithTextColumn("Maximum", x => x.Text, x => { x.Width = new GridLength(9, GridUnitType.Star); x.MaxWidth = new GridLength(200); });
            var grid   = new TreeDataGrid { Source = source };
            var window = Show(grid);

            try
            {
                Assert.Equal(200, source.Columns[0].ActualWidth, 6);
                Assert.InRange(source.Columns[1].ActualWidth, 30, 100);
                Assert.InRange(source.Columns.Sum(x => x.ActualWidth), 280, 300);
            }
            finally
            {
                window.Close();
            }
        }

        [AvaloniaFact]
        public void Pointer_Drag_Reorders_A_Header_Without_Sorting()
        {
            using var source = new FlatTreeDataGridSource<Model>(new[] { new Model() })
                .WithTextColumn("First", x => x.Text, x => x.Width = new GridLength(100))
                .WithTextColumn("Second", x => x.Other, x => x.Width = new GridLength(100));
            var grid   = new TreeDataGrid { Source = source, CanUserReorderColumns = true };
            var window = Show(grid);

            try
            {
                var header = grid.ColumnHeadersPresenter!.TryGetElement(0)!;
                var start  = header.TranslatePoint(new Point(35, 10), window)!.Value;
                var end    = start + new Vector(110, 0);

                window.MouseDown(start, MouseButton.Left);
                window.MouseMove(end, RawInputModifiers.LeftMouseButton);
                window.MouseUp(end, MouseButton.Left);
                Layout(window);
                Assert.Equal("Second", source.Columns[0].Header);
                Assert.False(source.IsSorted);
            }
            finally
            {
                window.Close();
            }
        }

        [AvaloniaFact]
        public void Row_Header_Selects_All_Columns_In_Multiple_Cell_Mode()
        {
            using var source = new FlatTreeDataGridSource<Model>(new[] { new Model() })
                .WithRowHeaderColumn().WithTextColumn(x => x.Text).WithTextColumn(x => x.Other);
            var grid   = new TreeDataGrid { Source = source, SelectionMode = TreeDataGridSelectionMode.Cell | TreeDataGridSelectionMode.Multiple };
            var window = Show(grid);

            try
            {
                var header = grid.TryGetCell(0, 0)!;
                var point  = header.TranslatePoint(new Point(10, 10), window)!.Value;

                window.MouseDown(point, MouseButton.Left);
                window.MouseUp(point, MouseButton.Left);
                Assert.Equal(3, source.CellSelection!.Count);
            }
            finally
            {
                window.Close();
            }
        }

        [AvaloniaFact]
        public void Disposing_An_Attached_Source_Clears_Realized_Cells_And_Selection()
        {
            var source = new FlatTreeDataGridSource<Model>(new[] { new Model() }).WithTextColumn(x => x.Text);
            var grid   = new TreeDataGrid { Source = source };
            var window = Show(grid);

            try
            {
                grid.SelectedIndex = 0;
                source.Dispose();
                Layout(window);
                Assert.Null(grid.Rows);
                Assert.Null(grid.SelectedItem);
                Assert.Empty(grid.RowsPresenter!.GetRealizedElements());
            }
            finally
            {
                window.Close();
            }
        }

        [AvaloniaFact]
        public void Owned_Xaml_Columns_Do_Not_Retain_An_Unused_Grid()
        {
            var (source, reference) = CreateOwnedGridReference();

            Collect();
            Assert.False(reference.IsAlive);
            GC.KeepAlive(source);
            ((IDisposable)source).Dispose();
        }

        [AvaloniaFact]
        public void Detached_Grid_With_Focused_Editor_Is_Collectible_While_Source_Stays_Alive()
        {
            using var source = new FlatTreeDataGridSource<Model>(new[] { new Model() })
                .WithTextColumn(x => x.Text);
            var reference = CreateClosedGridReference(source);

            for (var frame = 0; frame < 3; ++frame)
            {
                AvaloniaHeadlessPlatform.ForceRenderTimerTick();
                Dispatcher.UIThread.RunJobs();
            }
            Collect();
            Assert.False(reference.IsAlive);
            GC.KeepAlive(source);
        }

        [AvaloniaFact]
        public void Frozen_Count_And_Grid_Lines_Can_Change_After_Scrolling()
        {
            using var source = new FlatTreeDataGridSource<Model>(new[] { new Model() });
            for (var i = 0; i < 20; ++i)
            {
                source.WithTextColumn(i, x => x.Text, x => x.Width = new GridLength(100));
            }
            var grid   = new TreeDataGrid { Source = source };
            var window = Show(grid);
            try
            {
                grid.Scroll!.Offset = new Vector(1200, 0);
                Layout(window);
                grid.FrozenColumnCount   = 2;
                grid.GridLinesVisibility = TreeDataGridGridLinesVisibility.All;
                Layout(window);
                var first  = Assert.IsAssignableFrom<TreeDataGridCell>(grid.TryGetCell(0, 0));
                var second = Assert.IsAssignableFrom<TreeDataGridCell>(grid.TryGetCell(1, 0));
                Assert.Equal(0, first.TranslatePoint(default, grid)!.Value.X, 5);
                Assert.Equal(100, second.TranslatePoint(default, grid)!.Value.X, 5);
                Assert.Equal(new Thickness(0, 0, 1, 1), first.BorderThickness);
                Assert.NotNull(first.BorderBrush);
                grid.FrozenColumnCount = 0;
                Layout(window);
                Assert.Null(grid.TryGetCell(0, 0));
            }
            finally
            {
                window.Close();
            }
        }

        [AvaloniaFact]
        public void Replaced_Hierarchical_Model_Is_Collectible_While_Its_Children_Stay_Alive()
        {
            var (source, children, reference) = CreateReplacedHierarchyReference();
            using (source)
            {
                Collect();
                Assert.False(reference.IsAlive);
                GC.KeepAlive(children);
            }
        }

        #endregion

        #region Private

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static (TreeDataGridSource, WeakReference) CreateOwnedGridReference()
        {
            var grid = new TreeDataGrid();

            grid.Columns.Add(new TreeDataGridTextColumn());
            grid.ItemsSource = new[] { new Model() };

            return (grid.Source!, new WeakReference(grid));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference CreateClosedGridReference(TreeDataGridSource source)
        {
            var grid   = new TreeDataGrid { Source = source };
            var window = Show(grid);

            Assert.True(grid.BeginEdit(0, 0));
            Layout(window);
            var cell = Assert.IsAssignableFrom<TreeDataGridCell>(grid.TryGetCell(0, 0));

            cell.GetVisualDescendants().OfType<TextBox>().First().Focus();
            // The headless compositor retains the closed host until its queued render batches finish.
            window.FocusManager?.Focus(null);
            window.Content = null;
            Layout(window);
            window.Close();
            Assert.Null(cell.Model);
            Assert.False(cell.IsEditing);

            return new WeakReference(grid);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static (HierarchicalTreeDataGridSource<Model>, ObservableCollection<Model>, WeakReference) CreateReplacedHierarchyReference()
        {
            var model  = new Model { Children = { new Model() } };
            var items  = new ObservableCollection<Model> { model };
            var source = new HierarchicalTreeDataGridSource<Model>(items)
                .WithHierarchicalExpanderTextColumn(x => x.Text, x => x.Children);
            source.ExpandAll();
            Assert.Equal(2, source.Rows.Count);
            items[0] = new Model();
            return (source, model.Children, new WeakReference(model));
        }

        private static HierarchicalTreeDataGridSource<Model> CreateInteractionSource(Model root, bool editable = false) =>
            new HierarchicalTreeDataGridSource<Model>(new[] { root })
                .WithHierarchicalExpanderTextColumn(x => x.Text, x => x.Children, options: options =>
                {
                    options.Width = new GridLength(150);
                    options.IsReadOnly = true;
                })
                .WithTextColumn(x => x.Other, options =>
                {
                    options.Width = new GridLength(120);
                    options.IsReadOnly = !editable;
                });

        private static void DoubleTap(TestWindow window, Control target)
        {
            var point = target.TranslatePoint(new Point(target.Bounds.Width / 2, target.Bounds.Height / 2), window)!.Value;

            for (var i = 0; i < 2; ++i)
            {
                window.MouseDown(point, MouseButton.Left);
                window.MouseUp(point, MouseButton.Left);
            }
        }

        private static void AssertAllVisibleChildrenAreRealized(TreeDataGridPresenterBase<object> presenter)
        {
            var visible  = presenter.GetVisualChildren().OfType<Control>().Where(x => x.IsVisible).ToArray();
            var realized = presenter.GetRealizedElements().ToArray();

            Assert.Equal(realized.Length, visible.Length);

            foreach (var child in visible)
            {
                Assert.Contains(child, realized);
            }
        }

        private static TestWindow Show(TreeDataGrid grid)
        {
            var app    = Application.Current!;
            var fluent = new FluentTheme();
            var theme  = new StyleInclude(new Uri("avares://BlackSharp.UI.Avalonia.TreeDataGrid/"))
            {
                Source = new Uri("avares://BlackSharp.UI.Avalonia.TreeDataGrid/Themes/Fluent.axaml"),
            };

            app.Styles.Add(fluent);
            app.Styles.Add(theme);
            var window = new TestWindow(new Size(300, 240))
            {
                RequestedThemeVariant = ThemeVariant.Light,
            };

            window.Closed += (s, e) =>
            {
                app.Styles.Remove(theme);
                app.Styles.Remove(fluent);
            };

            try
            {
                window.Content = grid;
                Layout(window);

                return window;
            }
            catch
            {
                window.Close();
                throw;
            }
        }

        private static void Layout(TestWindow window)
        {
            window.Measure(new Size(300, 240));
            window.Arrange(new Rect(0, 0, 300, 240));
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference CreateGridReference(TreeDataGridSource source) => new(new TreeDataGrid { Source = source });

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static (TreeDataGridSource, WeakReference) CreateDisposedSourceReference()
        {
            var model     = new Model();
            var reference = new WeakReference(model);
            var source    = new FlatTreeDataGridSource<Model>(new[] { model }).WithTextColumn("Text", x => x.Text);

            _ = source.Rows[0];
            source.Dispose();

            return (source, reference);
        }

        private static void Collect()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        #endregion

        #region Nested Types

        private sealed class Model : INotifyPropertyChanged
        {
            #region Fields

            private string _text     = "value";
            private bool   _expanded;

            #endregion

            #region Properties

            public string Text
            {
                get => _text;
                set
                {
                    _text = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
                }
            }

            public string Other { get; set; } = "other";

            public double Number { get; set; }

            public bool Flag { get; set; }

            public ObservableCollection<Model> Children { get; } = [];

            public bool Expanded
            {
                get => _expanded;
                set
                {
                    _expanded = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Expanded)));
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
