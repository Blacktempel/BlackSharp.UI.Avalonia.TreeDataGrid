using Avalonia;
using Avalonia.Styling;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Primitives
{
    internal static class TreeDataGridPresenterProperties
    {
        #region Constructor

        static TreeDataGridPresenterProperties()
        {
            ItemContainerThemeProperty.AddOwner<TreeDataGridColumnHeadersPresenter>();
            ItemContainerThemeProperty.AddOwner<TreeDataGridCellsPresenter>();
            FrozenColumnCountProperty .AddOwner<TreeDataGridCellsPresenter>();
        }

        #endregion

        #region Fields

        internal static readonly StyledProperty<ControlTheme?> ItemContainerThemeProperty =
            AvaloniaProperty.Register<TreeDataGridRowsPresenter, ControlTheme?>(nameof(TreeDataGridRowsPresenter.ItemContainerTheme));

        internal static readonly StyledProperty<int>           FrozenColumnCountProperty  =
            TreeDataGrid.FrozenColumnCountProperty.AddOwner<TreeDataGridColumnHeadersPresenter>();

        #endregion
    }
}
