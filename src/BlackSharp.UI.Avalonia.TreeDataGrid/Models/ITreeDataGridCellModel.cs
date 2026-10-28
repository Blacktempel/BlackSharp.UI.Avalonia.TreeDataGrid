using Avalonia.Layout;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Models
{
    public interface ITreeDataGridCellModel
    {
        #region Properties

        bool CanEdit { get; }

        BeginEditGestures EditGestures { get; }

        object? Value { get; }

        HorizontalAlignment HorizontalContentAlignment { get; }

        VerticalAlignment VerticalContentAlignment { get; }

        #endregion
    }
}
