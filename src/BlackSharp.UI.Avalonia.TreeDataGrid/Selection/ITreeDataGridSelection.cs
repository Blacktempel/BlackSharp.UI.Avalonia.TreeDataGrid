using System.Collections;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Selection
{
    internal interface ITreeDataGridSelection
    {
        #region Properties

        IEnumerable? Source { get; set; }

        #endregion
    }
}
