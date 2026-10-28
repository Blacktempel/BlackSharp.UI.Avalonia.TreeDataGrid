using Avalonia.Controls;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Models
{
    public interface ITreeDataGridRowModel
    {
        #region Properties

        object? Header { get; }

        GridLength Height { get; set; }

        object? Model { get; }

        int ModelIndex { get; }

        IndexPath ModelIndexPath { get; }

        #endregion
    }
}
