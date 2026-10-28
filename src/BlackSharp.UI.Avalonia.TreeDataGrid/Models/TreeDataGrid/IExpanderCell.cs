namespace BlackSharp.UI.Avalonia.TreeDataGrid.Models
{
    /// <summary>
    /// Represents a cell in a <see cref="EngineHierarchicalTreeDataGridSource{TModel}"/> which displays
    /// an expander to reveal nested data.
    /// </summary>
    internal interface IExpanderCell : ICell, IExpander
    {
        #region Properties

        /// <summary>
        /// Gets the cell content.
        /// </summary>
        object? Content { get; }

        /// <summary>
        /// Gets the row that the cell belongs to.
        /// </summary>
        IRow Row { get; }

        #endregion
    }
}
