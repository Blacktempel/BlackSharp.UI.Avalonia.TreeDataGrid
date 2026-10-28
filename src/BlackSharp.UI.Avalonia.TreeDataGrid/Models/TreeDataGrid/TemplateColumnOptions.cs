namespace BlackSharp.UI.Avalonia.TreeDataGrid.Models
{
    /// <summary>
    /// Holds less commonly-used options for a <see cref="TemplateColumn{TModel}"/>.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    internal class TemplateColumnOptions<TModel> : ColumnOptions<TModel>, ITemplateCellOptions
    {
        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether the column takes part in text searches.
        /// </summary>
        public bool IsTextSearchEnabled { get; set; }

        /// <summary>
        /// Gets or sets a function which selects the search text from a model.
        /// </summary>
        public Func<TModel, string?>? TextSearchValueSelector { get; set; }

        #endregion
    }
}
