using Avalonia.Controls;
using Avalonia.Layout;

namespace BlackSharp.UI.Avalonia.TreeDataGrid
{
    public class ColumnCreateOptions
    {
        #region Properties

        public bool? AllowTriStateSorting { get; set; }

        public BeginEditGestures BeginEditGestures { get; set; } = BeginEditGestures.Default;

        public bool? CanUserResize { get; set; }

        public bool? CanUserSort { get; set; }

        public Comparison<object?>? CompareAscending { get; set; }

        public Comparison<object?>? CompareDescending { get; set; }

        public GridLength? MaxWidth { get; set; }

        public GridLength MinWidth { get; set; } = new GridLength(30);

        public HorizontalAlignment HorizontalContentAlignment { get; set; } = HorizontalAlignment.Stretch;

        public VerticalAlignment VerticalContentAlignment { get; set; } = VerticalAlignment.Center;

        public GridLength? Width { get; set; }

        #endregion
    }
}
