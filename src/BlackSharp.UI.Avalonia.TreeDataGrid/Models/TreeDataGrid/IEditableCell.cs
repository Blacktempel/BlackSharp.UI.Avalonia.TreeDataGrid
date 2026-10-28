namespace BlackSharp.UI.Avalonia.TreeDataGrid.Models
{
    /// <summary>
    /// Represents a cell in an <see cref="ITreeDataGridSource"/> that supports editing.
    /// </summary>
    internal interface IEditableCell : ICell
    {
        #region Properties

        /// <summary>
        /// Gets a value indicating whether the cell is read-only and cannot be edited.
        /// </summary>
        bool IsReadOnly { get; }

        #endregion
    }
}
