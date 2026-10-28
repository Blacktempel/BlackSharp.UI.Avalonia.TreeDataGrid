using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System.Globalization;

namespace BlackSharp.UI.Avalonia.TreeDataGrid
{
    public sealed class TreeDataGridTextColumn : TreeDataGridBoundColumn
    {
        #region Fields

        public static readonly StyledProperty<bool>          IsTextSearchEnabledProperty =
            AvaloniaProperty.Register<TreeDataGridTextColumn, bool>(nameof(IsTextSearchEnabled), true);

        public static readonly StyledProperty<TextAlignment> TextAlignmentProperty       =
            AvaloniaProperty.Register<TreeDataGridTextColumn, TextAlignment>(nameof(TextAlignment), TextAlignment.Left);

        public static readonly StyledProperty<TextTrimming>  TextTrimmingProperty        =
            AvaloniaProperty.Register<TreeDataGridTextColumn, TextTrimming>(nameof(TextTrimming), TextTrimming.CharacterEllipsis);

        public static readonly StyledProperty<TextWrapping>  TextWrappingProperty        =
            AvaloniaProperty.Register<TreeDataGridTextColumn, TextWrapping>(nameof(TextWrapping), TextWrapping.NoWrap);

        public static readonly StyledProperty<bool>          UseDrawnCellsProperty       =
            AvaloniaProperty.Register<TreeDataGridTextColumn, bool>(nameof(UseDrawnCells), false);

        private TreeDataGridTextCellRenderer? _renderer;

        #endregion

        #region Properties

        public bool IsTextSearchEnabled
        {
            get => GetValue(IsTextSearchEnabledProperty);
            set => SetValue(IsTextSearchEnabledProperty, value);
        }

        public TextAlignment TextAlignment
        {
            get => GetValue(TextAlignmentProperty);
            set => SetValue(TextAlignmentProperty, value);
        }

        public TextTrimming TextTrimming
        {
            get => GetValue(TextTrimmingProperty);
            set => SetValue(TextTrimmingProperty, value);
        }

        public TextWrapping TextWrapping
        {
            get => GetValue(TextWrappingProperty);
            set => SetValue(TextWrappingProperty, value);
        }

        public bool UseDrawnCells
        {
            get => GetValue(UseDrawnCellsProperty);
            set => SetValue(UseDrawnCellsProperty, value);
        }

        internal string? StringFormat { get; set; }

        internal CultureInfo? Culture { get; set; }

        public override CellRenderer? CellRenderer => UseDrawnCells ? _renderer ??= new TreeDataGridTextCellRenderer(this) : null;

        #endregion

        #region Internal

        internal void ClearTextLayout(Control owner) => _renderer?.ClearCache(owner);

        #endregion
    }
}
