namespace BlackSharp.UI.Avalonia.TreeDataGrid.Selection
{
    /// <summary>
    /// Provides data for the <see cref="ITreeDataGridCellSelectionModel.SelectionChanged"/> event.
    /// </summary>
    internal class TreeDataGridCellSelectionChangedEventArgs : EventArgs
    {
    }

    /// <summary>
    /// Provides data for the <see cref="ITreeDataGridCellSelectionModel{T}.SelectionChanged"/> event.
    /// </summary>
    /// <typeparam name="T">The model type.</typeparam>
    internal class TreeDataGridCellSelectionChangedEventArgs<T> : TreeDataGridCellSelectionChangedEventArgs
        where T : class
    {
    }
}
