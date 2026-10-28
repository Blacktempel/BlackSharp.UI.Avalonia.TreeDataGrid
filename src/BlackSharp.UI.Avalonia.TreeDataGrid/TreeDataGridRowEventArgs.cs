using BlackSharp.UI.Avalonia.TreeDataGrid.Primitives;

namespace BlackSharp.UI.Avalonia.TreeDataGrid
{
    public class TreeDataGridRowEventArgs
    {
        #region Constructor

        public TreeDataGridRowEventArgs(TreeDataGridRow row, int rowIndex)
        {
            Row      = row;
            RowIndex = rowIndex;
        }

        internal TreeDataGridRowEventArgs()
        {
            Row = null!;
        }

        #endregion

        #region Properties

        public TreeDataGridRow Row { get; private set; }

        public int RowIndex { get; private set; }

        #endregion

        #region Internal

        internal void Update(TreeDataGridRow? row, int rowIndex)
        {
            if (row is object && Row is object)
            {
                throw new NotSupportedException("Nested TreeDataGrid row prepared/clearing detected.");
            }

            Row      = row!;
            RowIndex = rowIndex;
        }

        #endregion
    }
}
