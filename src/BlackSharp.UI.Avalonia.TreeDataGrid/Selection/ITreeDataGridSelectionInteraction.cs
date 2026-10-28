using Avalonia.Input;
using BlackSharp.UI.Avalonia.TreeDataGrid.Models;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Selection
{
    public interface ITreeDataGridRowSelection
    {
        #region Properties

        int Count { get; }

        IReadOnlyList<IndexPath> SelectedIndexes { get; }

        IReadOnlyList<object?> SelectedItems { get; }

        IndexPath SelectedIndex { get; set; }

        #endregion

        #region Private

        bool IsSelected(IndexPath index);

        void Select(IndexPath index);

        void Deselect(IndexPath index);

        #endregion
    }

    public interface ITreeDataGridCellSelection
    {
        #region Properties

        CellIndex SelectedIndex { get; set; }

        IReadOnlyList<CellIndex> SelectedIndexes { get; }

        #endregion

        #region Private

        bool IsSelected(CellIndex index);

        #endregion
    }

    public interface ITreeDataGridSelectionInteraction
    {
        #region Public

        bool IsCellSelected(int columnIndex, int rowIndex);

        bool IsRowSelected(ITreeDataGridRowModel rowModel);

        bool IsRowSelected(int rowIndex);

        void OnKeyDown(TreeDataGrid sender, KeyEventArgs e);

        void OnPreviewKeyDown(TreeDataGrid sender, KeyEventArgs e);

        void OnKeyUp(TreeDataGrid sender, KeyEventArgs e);

        void OnTextInput(TreeDataGrid sender, TextInputEventArgs e);

        void OnPointerPressed(TreeDataGrid sender, PointerPressedEventArgs e);

        void OnPointerMoved(TreeDataGrid sender, PointerEventArgs e);

        void OnPointerReleased(TreeDataGrid sender, PointerReleasedEventArgs e);

        #endregion

        #region Events

        event EventHandler? SelectionChanged;

        #endregion
    }
}
