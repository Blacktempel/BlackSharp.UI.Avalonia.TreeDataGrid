using BlackSharp.UI.Avalonia.TreeDataGrid.Models;
using BlackSharp.UI.Avalonia.TreeDataGrid.Selection;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Primitives
{
    internal interface ITreeDataGridCell
    {
        #region Properties

        int ColumnIndex { get; }

        #endregion

        #region Private

        void Realize(
            TreeDataGridElementFactory factory,
            IEngineSelectionInteraction? selection,
            ICell model,
            int columnIndex,
            int rowIndex);

        void Unrealize();

        #endregion
    }
}
