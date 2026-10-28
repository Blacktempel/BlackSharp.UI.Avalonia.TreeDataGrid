using Avalonia.Controls.Selection;
using BlackSharp.UI.Avalonia.TreeDataGrid.Models;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Selection
{
    internal class TreeDataGridColumnSelectionModel : SelectionModel<IColumn>,
        ITreeDataGridColumnSelectionModel
    {
        #region Constructor

        public TreeDataGridColumnSelectionModel(IColumns columns)
            : base(columns)
        {
        }

        #endregion
    }
}
