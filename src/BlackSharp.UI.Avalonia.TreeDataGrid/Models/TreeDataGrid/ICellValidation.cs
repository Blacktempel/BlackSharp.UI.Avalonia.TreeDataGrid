namespace BlackSharp.UI.Avalonia.TreeDataGrid.Models
{
    internal interface ICellValidation
    {
        #region Properties

        Exception? ValidationError { get; }

        #endregion
    }
}
