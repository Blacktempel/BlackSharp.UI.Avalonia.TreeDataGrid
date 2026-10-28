using Avalonia.Layout;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Models
{
    /// <summary>
    /// Represents a cell in an <see cref="ITreeDataGridSource"/>.
    /// </summary>
    internal interface ICell : Models.ITreeDataGridCellModel
    {
        #region Properties

        /// <summary>
        /// Gets a value indicating whether the cell can enter edit mode.
        /// </summary>
        new bool CanEdit { get; }

        /// <summary>
        /// Gets the gesture(s) that will cause the cell to enter edit mode.
        /// </summary>
        new BeginEditGestures EditGestures { get; }

        /// <summary>
        /// Gets the value of the cell.
        /// </summary>
        new object? Value { get; }

        #endregion

        #region Models.ITreeDataGridCellModel

        bool Models.ITreeDataGridCellModel.CanEdit => CanEdit;

        BeginEditGestures Models.ITreeDataGridCellModel.EditGestures => EditGestures;

        object? Models.ITreeDataGridCellModel.Value => Value;

        HorizontalAlignment Models.ITreeDataGridCellModel.HorizontalContentAlignment => HorizontalAlignment.Stretch;

        VerticalAlignment Models.ITreeDataGridCellModel.VerticalContentAlignment => VerticalAlignment.Center;

        #endregion
    }
}
