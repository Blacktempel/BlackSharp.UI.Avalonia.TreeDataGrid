namespace BlackSharp.UI.Avalonia.TreeDataGrid.Selection
{
    internal interface ITreeDataGridRowSelectionModel : ITreeSelectionModel, ITreeDataGridSelection
    {
    }

    internal interface ITreeDataGridRowSelectionModel<T> : ITreeDataGridRowSelectionModel
    {
        #region Properties

        new T? SelectedItem { get; }

        new IReadOnlyList<T?> SelectedItems { get; }

        #endregion

        #region Events

        new event EventHandler<TreeSelectionModelSelectionChangedEventArgs<T>>? SelectionChanged;

        #endregion
    }
}
