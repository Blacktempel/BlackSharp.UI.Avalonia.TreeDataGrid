using Avalonia.Input;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Models
{
    /// <summary>
    /// Holds information about an automatic row drag/drop operation carried out
    /// by <see cref="BlackSharp.UI.Avalonia.TreeDataGrid.TreeDataGrid.AutoDragDropRows"/>.
    /// </summary>
    /// <param name="source">The source of the drag operation/</param>
    /// <param name="indexes">The indexes being dragged.</param>
    internal class DragInfo(ITreeDataGridSource source, IEnumerable<IndexPath> indexes)
    {
        #region Fields

        /// <summary>
        /// Defines the data format for dragging TreeDataGrid rows.
        /// </summary>
        public static readonly DataFormat<DragInfo> Format = DataFormat.CreateInProcessFormat<DragInfo>("TreeDataGridDragInfo");

        #endregion

        #region Properties

        /// <summary>
        /// Gets the <see cref="ITreeDataGridSource"/> that rows are being dragged from.
        /// </summary>
        public ITreeDataGridSource Source { get; } = source;

        /// <summary>
        /// Gets or sets the model indexes of the rows being dragged.
        /// </summary>
        public IEnumerable<IndexPath> Indexes { get; } = indexes;

        #endregion
    }
}
