using Avalonia.Media;
using System.Globalization;

namespace BlackSharp.UI.Avalonia.TreeDataGrid
{
    public class TextColumnCreateOptions : ColumnCreateOptions
    {
        #region Properties

        public bool IsReadOnly { get; set; } = false;

        public bool IsTextSearchEnabled { get; set; } = true;

        public bool UseDrawnCells { get; set; } = false;

        public string? StringFormat { get; set; }

        public CultureInfo? Culture { get; set; }

        public TextTrimming TextTrimming { get; set; } = TextTrimming.CharacterEllipsis;

        public TextWrapping TextWrapping { get; set; } = TextWrapping.NoWrap;

        public TextAlignment TextAlignment { get; set; } = TextAlignment.Left;

        #endregion
    }
}
