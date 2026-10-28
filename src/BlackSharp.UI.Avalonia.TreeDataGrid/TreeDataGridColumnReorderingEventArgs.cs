using Avalonia.Interactivity;

namespace BlackSharp.UI.Avalonia.TreeDataGrid
{
    public sealed class TreeDataGridColumnReorderingEventArgs : CancelRoutedEventArgs
    {
        #region Constructor

        public TreeDataGridColumnReorderingEventArgs(TreeDataGridColumn column, int oldIndex, int newIndex)
            : base(TreeDataGrid.ColumnReorderingEvent)
        {
            Column   = column;
            OldIndex = oldIndex;
            NewIndex = newIndex;
        }

        #endregion

        #region Properties

        public TreeDataGridColumn Column { get; }

        public int OldIndex { get; }

        public int NewIndex { get; }

        #endregion
    }

    public sealed class TreeDataGridColumnReorderedEventArgs : RoutedEventArgs
    {
        #region Constructor

        public TreeDataGridColumnReorderedEventArgs(TreeDataGridColumn column, int oldIndex, int newIndex)
            : base(TreeDataGrid.ColumnReorderedEvent)
        {
            Column   = column;
            OldIndex = oldIndex;
            NewIndex = newIndex;
        }

        #endregion

        #region Properties

        public TreeDataGridColumn Column { get; }

        public int OldIndex { get; }

        public int NewIndex { get; }

        #endregion
    }
}
