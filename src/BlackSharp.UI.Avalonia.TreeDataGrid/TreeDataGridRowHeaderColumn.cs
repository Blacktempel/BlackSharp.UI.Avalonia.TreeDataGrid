using Avalonia.Controls;

namespace BlackSharp.UI.Avalonia.TreeDataGrid
{
    public sealed class TreeDataGridRowHeaderColumn : TreeDataGridColumn
    {
        #region Constructor

        public TreeDataGridRowHeaderColumn()
        {
            CanUserReorder = false;
            CanUserSort    = false;
            Width          = new GridLength(40);
        }

        #endregion
    }
}
