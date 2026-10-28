namespace BlackSharp.UI.Avalonia.TreeDataGrid.Models
{
    public class TreeDataGridRowModelEventArgs(ITreeDataGridRowModel row) : EventArgs
    {
        #region Properties

        public ITreeDataGridRowModel Row { get; } = row;

        #endregion
    }
}
