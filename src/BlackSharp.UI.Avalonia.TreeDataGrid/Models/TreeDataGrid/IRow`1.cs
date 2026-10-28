namespace BlackSharp.UI.Avalonia.TreeDataGrid.Models
{
    /// <summary>
    /// Represents a row in an <see cref="ITreeDataGridSource"/>.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    internal interface IRow<TModel> : IRow
    {
        #region Properties

        /// <summary>
        /// Gets the row model.
        /// </summary>
        new TModel Model { get; }

        #endregion

        #region Private

        /// <summary>
        /// Updates the model index due to a change in the data source.
        /// </summary>
        /// <param name="delta">The index delta.</param>
        void UpdateModelIndex(int delta);

        #endregion

        #region IRow

        /// <summary>
        /// Gets the untyped row model.
        /// </summary>
        object? IRow.Model => Model;

        #endregion
    }
}
