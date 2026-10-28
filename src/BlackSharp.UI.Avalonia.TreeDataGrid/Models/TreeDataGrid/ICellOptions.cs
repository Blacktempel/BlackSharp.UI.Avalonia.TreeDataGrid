namespace BlackSharp.UI.Avalonia.TreeDataGrid.Models
{
    /// <summary>
    /// Holds less commonly-used options for an <see cref="IColumn"/>.
    /// </summary>
    internal interface ICellOptions
    {
        #region Properties

        /// <summary>
        /// Gets the gesture(s) that will cause the cell to enter edit mode.
        /// </summary>
        BeginEditGestures BeginEditGestures { get; }

        #endregion
    }
}
