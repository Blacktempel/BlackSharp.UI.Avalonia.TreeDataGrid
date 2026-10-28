namespace BlackSharp.UI.Avalonia.TreeDataGrid.Models
{
    internal interface ITextSearchableColumn<TModel>
    {
        #region Properties

        bool IsTextSearchEnabled { get; }

        #endregion

        #region Private

        string? SelectValue(TModel model);

        #endregion
    }
}
