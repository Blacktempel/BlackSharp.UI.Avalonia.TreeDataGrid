using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.VisualTree;
using BlackSharp.UI.Avalonia.TreeDataGrid.Primitives;

namespace BlackSharp.UI.Avalonia.TreeDataGrid
{
    public partial class TreeDataGrid
    {
        #region Fields

        public static readonly StyledProperty<bool> ExpandCollapseOnDoubleTapProperty          =
            AvaloniaProperty.Register<TreeDataGrid, bool>(nameof(ExpandCollapseOnDoubleTap));

        public static readonly StyledProperty<bool> SelectParentOnLeftArrowProperty            =
            AvaloniaProperty.Register<TreeDataGrid, bool>(nameof(SelectParentOnLeftArrow));

        public static readonly StyledProperty<bool> PreserveSelectionOnCtrlArrowScrollProperty =
            AvaloniaProperty.Register<TreeDataGrid, bool>(nameof(PreserveSelectionOnCtrlArrowScroll));

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets whether double-tapping a noninteractive part of a row toggles its expansion.
        /// Cell editing gestures take precedence. The default is false.
        /// </summary>
        public bool ExpandCollapseOnDoubleTap
        {
            get => GetValue(ExpandCollapseOnDoubleTapProperty);
            set => SetValue(ExpandCollapseOnDoubleTapProperty, value);
        }

        /// <summary>
        /// Gets or sets whether an unmodified Left arrow selects the parent of a collapsed row or leaf.
        /// An expanded row is collapsed first. Editors retain their own arrow keys. The default is false.
        /// </summary>
        public bool SelectParentOnLeftArrow
        {
            get => GetValue(SelectParentOnLeftArrowProperty);
            set => SetValue(SelectParentOnLeftArrowProperty, value);
        }

        /// <summary>
        /// Gets or sets whether Ctrl+Up and Ctrl+Down scroll without changing selection or focus.
        /// Shortcuts inside interactive controls are left to those controls. The default is false.
        /// </summary>
        public bool PreserveSelectionOnCtrlArrowScroll
        {
            get => GetValue(PreserveSelectionOnCtrlArrowScrollProperty);
            set => SetValue(PreserveSelectionOnCtrlArrowScrollProperty, value);
        }

        #endregion

        #region Protected

        protected override void OnDoubleTapped(TappedEventArgs e)
        {
            base.OnDoubleTapped(e);

            if (!ExpandCollapseOnDoubleTap || e.Handled || IsInputInsideInteractiveControl(e.Source) ||
                GetInputRow(e.Source) is not { RowIndex: >= 0 } row ||
                CoreRows is not { } rows || row.RowIndex >= rows.Count)
            {
                return;
            }

            if (row.CellsPresenter is { } cells)
            {
                foreach (var cell in cells.GetRealizedElements())
                {
                    if (cell is TreeDataGridCell { IsEditing: true } ||
                        cell.GetVisualDescendants().OfType<TreeDataGridCell>().Any(x => x.IsEditing))
                    {
                        return;
                    }
                }
            }

            if (rows[row.RowIndex] is Models.IExpander { ShowExpander: true } expander)
            {
                expander.IsExpanded = !expander.IsExpanded;
                e.Handled          = true;
            }
        }

        #endregion

        #region Private

        private bool HandleSelectionPreservingScroll(KeyEventArgs e)
        {
            if (!PreserveSelectionOnCtrlArrowScroll || e.Handled || e.KeyModifiers != KeyModifiers.Control ||
                e.Key is not (Key.Up or Key.Down) || IsInputInsideInteractiveControl(e.Source) ||
                Scroll is not ScrollViewer viewer)
            {
                return false;
            }

            if (e.Key == Key.Up)
            {
                viewer.LineUp();
            }
            else
            {
                viewer.LineDown();
            }

            e.Handled = true;

            return true;
        }

        private bool HandleParentNavigation(KeyEventArgs e)
        {
            if (!SelectParentOnLeftArrow || e.Handled || e.Key != Key.Left || e.KeyModifiers != KeyModifiers.None ||
                IsInputInsideInteractiveControl(e.Source) || Selection is not { } selection || Source is not { } source)
            {
                return false;
            }

            var path = selection.SelectedRowIndex;
            var row  = source.ModelIndexToRowIndex(path);

            if (row < 0 || Rows is not { } rows || row >= rows.Count)
            {
                return false;
            }

            if (rows[row] is TreeDataGridRowModel { IsExpanded: true } expanded)
            {
                expanded.IsExpanded = false;
                e.Handled          = true;

                return true;
            }

            if (path.Count <= 1 || source.ModelIndexToRowIndex(path[..^1]) < 0 || QueryCancelSelection())
            {
                return false;
            }

            var column = TryGetCell(e.Source as Control, out var currentCell) ? currentCell.ColumnIndex : 0;

            selection.SelectRowCore(path[..^1]);

            if (BringRowIntoView(path[..^1]) is TreeDataGridRow parent)
            {
                var target = parent.TryGetCell(column) ?? parent.CellsPresenter?.GetRealizedElements().FirstOrDefault(x => x.Focusable);

                target?.Focus();
            }

            e.Handled = true;

            return true;
        }

        private TreeDataGridRow? GetInputRow(object? source)
        {
            for (var visual = source as Visual; visual is not null && visual != this; visual = visual.GetVisualParent())
            {
                if (visual is TreeDataGridRow row)
                {
                    return row.FindAncestorOfType<TreeDataGrid>() == this ? row : null;
                }
            }

            return null;
        }

        private bool IsInputInsideInteractiveControl(object? source)
        {
            for (var visual = source as Visual; visual is not null && visual != this; visual = visual.GetVisualParent())
            {
                if (visual is Button or TextBox or NumericUpDown or SelectingItemsControl or RangeBase or
                    TreeDataGridCell { IsEditing: true })
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}
