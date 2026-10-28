using Avalonia.Media;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Models
{
    /// <summary>
    /// Represents a text cell in an <see cref="ITreeDataGridSource"/>.
    /// </summary>
    internal interface ITextCell : IEditableCell
    {
        #region Properties

        /// <summary>
        /// Gets or sets the cell's value as a string.
        /// </summary>
        string? Text { get; set; }

        /// <summary>
        /// Gets the cell's text trimming mode.
        /// </summary>
        TextTrimming TextTrimming { get; }

        /// <summary>
        /// Gets the cell's text wrapping mode.
        /// </summary>
        TextWrapping TextWrapping { get; }

        /// <summary>
        /// Gets the cell's text alignment mode.
        /// </summary>
        TextAlignment TextAlignment { get; }

        #endregion
    }
}
