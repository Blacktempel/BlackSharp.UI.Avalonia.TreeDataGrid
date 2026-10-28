using Avalonia.Interactivity;
using Avalonia.VisualTree;
using BlackSharp.UI.Avalonia.TreeDataGrid.Primitives;

namespace BlackSharp.UI.Avalonia.TreeDataGrid
{
    public partial class TreeDataGrid
    {
        #region Fields

        public static readonly RoutedEvent<TreeDataGridColumnReorderingEventArgs> ColumnReorderingEvent =
            RoutedEvent.Register<TreeDataGrid, TreeDataGridColumnReorderingEventArgs>(nameof(ColumnReordering), RoutingStrategies.Bubble);

        public static readonly RoutedEvent<TreeDataGridColumnReorderedEventArgs>  ColumnReorderedEvent  =
            RoutedEvent.Register<TreeDataGrid, TreeDataGridColumnReorderedEventArgs>(nameof(ColumnReordered), RoutingStrategies.Bubble);

        #endregion

        #region Internal

        internal bool TryReorderColumn(int oldIndex, int newIndex)
        {
            if (Source is not { } source || (uint)oldIndex >= source.Columns.Count ||
                (uint)newIndex >= source.Columns.Count || oldIndex == newIndex)
            {
                return false;
            }

            var column = source.Columns[oldIndex];
            var frozen = Math.Clamp(FrozenColumnCount, 0, source.Columns.Count);

            if (!(column.CanUserReorder ?? CanUserReorderColumns) || (oldIndex < frozen) != (newIndex < frozen))
            {
                return false;
            }

            var args = new TreeDataGridColumnReorderingEventArgs(column, oldIndex, newIndex);

            RaiseEvent(args);

            if (args.Cancel)
            {
                return false;
            }

            CommitEdit();

            if (this.GetVisualDescendants().OfType<TreeDataGridCell>().Any(x => x.IsEditing))
            {
                return false;
            }

            source.Columns.Move(oldIndex, newIndex);
            RaiseEvent(new TreeDataGridColumnReorderedEventArgs(column, oldIndex, newIndex));

            return true;
        }

        #endregion

        #region Events

        public event EventHandler<TreeDataGridColumnReorderingEventArgs>? ColumnReordering
        {
            add => AddHandler(ColumnReorderingEvent, value);
            remove => RemoveHandler(ColumnReorderingEvent, value);
        }

        public event EventHandler<TreeDataGridColumnReorderedEventArgs>? ColumnReordered
        {
            add => AddHandler(ColumnReorderedEvent, value);
            remove => RemoveHandler(ColumnReorderedEvent, value);
        }

        #endregion
    }
}
