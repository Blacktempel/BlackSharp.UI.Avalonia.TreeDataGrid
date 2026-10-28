using Avalonia.Controls.Selection;
using BlackSharp.UI.Avalonia.TreeDataGrid.Models;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Selection
{
    internal interface ITreeDataGridColumnSelectionModel : ISelectionModel
    {
        #region Properties

        new IReadOnlyList<IColumn?> SelectedItems { get; }

        new IColumn? SelectedItem { get; set; }

        #endregion
    }
}
