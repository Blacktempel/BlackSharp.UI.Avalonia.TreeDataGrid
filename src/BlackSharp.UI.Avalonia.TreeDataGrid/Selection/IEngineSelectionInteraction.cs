using Avalonia.Input;
using BlackSharp.UI.Avalonia.TreeDataGrid.Models;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Selection
{
    internal interface IEngineSelectionInteraction
    {
        #region Private

        bool IsCellSelected(int columnIndex, int rowIndex);

        bool IsRowSelected(IRow rowModel);

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
