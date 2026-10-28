using Avalonia;

namespace BlackSharp.UI.Avalonia.TreeDataGrid
{
    public sealed class TreeDataGridCheckBoxColumn : TreeDataGridBoundColumn
    {
        #region Fields

        public static readonly StyledProperty<bool> IsThreeStateProperty =
            AvaloniaProperty.Register<TreeDataGridCheckBoxColumn, bool>(nameof(IsThreeState), false);

        #endregion

        #region Properties

        public bool IsThreeState
        {
            get => GetValue(IsThreeStateProperty);
            set => SetValue(IsThreeStateProperty, value);
        }

        #endregion
    }
}
