using Avalonia.Controls;

namespace BlackSharp.UI.Avalonia.TreeDataGrid
{
    public class TreeDataGridCellEventArgs
    {
        #region Constructor

        public TreeDataGridCellEventArgs(Control cell, int columnIndex, int rowIndex)
        {
            Cell        = cell;
            ColumnIndex = columnIndex;
            RowIndex    = rowIndex;
        }

        internal TreeDataGridCellEventArgs()
        {
            Cell = null!;
        }

        #endregion

        #region Properties

        public Control Cell { get; private set; }

        public int ColumnIndex { get; private set; }

        public int RowIndex { get; private set; }

        #endregion

        #region Internal

        internal void Update(Control? cell, int columnIndex, int rowIndex)
        {
            if (cell is object && Cell is object)
            {
                throw new NotSupportedException("Nested TreeDataGrid cell prepared/clearing detected.");
            }

            Cell        = cell!;
            ColumnIndex = columnIndex;
            RowIndex    = rowIndex;
        }

        #endregion
    }
}
