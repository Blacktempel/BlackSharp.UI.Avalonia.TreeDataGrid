using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using System.ComponentModel;

namespace BlackSharp.UI.Avalonia.TreeDataGrid
{
    public abstract class TreeDataGridColumn : AvaloniaObject
    {
        #region Constructor

        private protected TreeDataGridColumn()
        {
        }

        #endregion

        #region Fields

        public static readonly StyledProperty<bool?>                      AllowTriStateSortingProperty       =
            AvaloniaProperty.Register<TreeDataGridColumn, bool?>(nameof(AllowTriStateSorting));

        public static readonly StyledProperty<BeginEditGestures>          BeginEditGesturesProperty          =
            AvaloniaProperty.Register<TreeDataGridColumn, BeginEditGestures>(nameof(BeginEditGestures), BeginEditGestures.Default);

        public static readonly StyledProperty<bool?>                      CanUserReorderProperty             =
            AvaloniaProperty.Register<TreeDataGridColumn, bool?>(nameof(CanUserReorder));

        public static readonly StyledProperty<bool?>                      CanUserResizeProperty              =
            AvaloniaProperty.Register<TreeDataGridColumn, bool?>(nameof(CanUserResize));

        public static readonly StyledProperty<bool?>                      CanUserSortProperty                =
            AvaloniaProperty.Register<TreeDataGridColumn, bool?>(nameof(CanUserSort));

        public static readonly StyledProperty<Comparison<object?>?>       CompareAscendingProperty           =
            AvaloniaProperty.Register<TreeDataGridColumn, Comparison<object?>?>(nameof(CompareAscending));

        public static readonly StyledProperty<Comparison<object?>?>       CompareDescendingProperty          =
            AvaloniaProperty.Register<TreeDataGridColumn, Comparison<object?>?>(nameof(CompareDescending));

        public static readonly StyledProperty<object?>                    HeaderProperty                     =
            AvaloniaProperty.Register<TreeDataGridColumn, object?>(nameof(Header));

        public static readonly StyledProperty<bool>                       IsVisibleProperty                  =
            AvaloniaProperty.Register<TreeDataGridColumn, bool>(nameof(IsVisible), true);

        public static readonly StyledProperty<GridLength?>                MaxWidthProperty                   =
            AvaloniaProperty.Register<TreeDataGridColumn, GridLength?>(nameof(MaxWidth));

        public static readonly StyledProperty<GridLength>                 MinWidthProperty                   =
            AvaloniaProperty.Register<TreeDataGridColumn, GridLength>(nameof(MinWidth), new GridLength(30));

        public static readonly StyledProperty<ListSortDirection?>         SortDirectionProperty              =
            AvaloniaProperty.Register<TreeDataGridColumn, ListSortDirection?>(nameof(SortDirection));

        public static readonly StyledProperty<object?>                    TagProperty                        =
            AvaloniaProperty.Register<TreeDataGridColumn, object?>(nameof(Tag));

        public static readonly StyledProperty<HorizontalAlignment>        HorizontalContentAlignmentProperty =
            AvaloniaProperty.Register<TreeDataGridColumn, HorizontalAlignment>(nameof(HorizontalContentAlignment), HorizontalAlignment.Stretch);

        public static readonly StyledProperty<VerticalAlignment>          VerticalContentAlignmentProperty   =
            AvaloniaProperty.Register<TreeDataGridColumn, VerticalAlignment>(nameof(VerticalContentAlignment), VerticalAlignment.Center);

        public static readonly StyledProperty<GridLength>                 WidthProperty                      =
            AvaloniaProperty.Register<TreeDataGridColumn, GridLength>(nameof(Width), GridLength.Auto);

        public static readonly DirectProperty<TreeDataGridColumn, double> ActualWidthProperty                =
            AvaloniaProperty.RegisterDirect<TreeDataGridColumn, double>(nameof(ActualWidth), x => x.ActualWidth);

        private double _actualWidth = double.NaN;

        #endregion

        #region Properties

        public bool? AllowTriStateSorting
        {
            get => GetValue(AllowTriStateSortingProperty);
            set => SetValue(AllowTriStateSortingProperty, value);
        }

        public BeginEditGestures BeginEditGestures
        {
            get => GetValue(BeginEditGesturesProperty);
            set => SetValue(BeginEditGesturesProperty, value);
        }

        public bool? CanUserReorder
        {
            get => GetValue(CanUserReorderProperty);
            set => SetValue(CanUserReorderProperty, value);
        }

        public bool? CanUserResize
        {
            get => GetValue(CanUserResizeProperty);
            set => SetValue(CanUserResizeProperty, value);
        }

        public bool? CanUserSort
        {
            get => GetValue(CanUserSortProperty);
            set => SetValue(CanUserSortProperty, value);
        }

        public Comparison<object?>? CompareAscending
        {
            get => GetValue(CompareAscendingProperty);
            set => SetValue(CompareAscendingProperty, value);
        }

        public Comparison<object?>? CompareDescending
        {
            get => GetValue(CompareDescendingProperty);
            set => SetValue(CompareDescendingProperty, value);
        }

        public object? Header
        {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public bool IsVisible
        {
            get => GetValue(IsVisibleProperty);
            set => SetValue(IsVisibleProperty, value);
        }

        public GridLength? MaxWidth
        {
            get => GetValue(MaxWidthProperty);
            set => SetValue(MaxWidthProperty, value);
        }

        public GridLength MinWidth
        {
            get => GetValue(MinWidthProperty);
            set => SetValue(MinWidthProperty, value);
        }

        public ListSortDirection? SortDirection
        {
            get => GetValue(SortDirectionProperty);
            set => SetValue(SortDirectionProperty, value);
        }

        public object? Tag
        {
            get => GetValue(TagProperty);
            set => SetValue(TagProperty, value);
        }

        public HorizontalAlignment HorizontalContentAlignment
        {
            get => GetValue(HorizontalContentAlignmentProperty);
            set => SetValue(HorizontalContentAlignmentProperty, value);
        }

        public VerticalAlignment VerticalContentAlignment
        {
            get => GetValue(VerticalContentAlignmentProperty);
            set => SetValue(VerticalContentAlignmentProperty, value);
        }

        public GridLength Width
        {
            get => GetValue(WidthProperty);
            set => SetValue(WidthProperty, value);
        }

        public double ActualWidth => _actualWidth;

        public virtual CellRenderer? CellRenderer => null;

        internal Func<object, object?>? ValueSelector { get; set; }

        internal object? Owner { get; set; }

        #endregion

        #region Internal

        internal void SetActualWidth(double width) => SetAndRaise(ActualWidthProperty, ref _actualWidth, width);

        internal virtual object? GetCellValue(object model) => ValueSelector?.Invoke(model);

        #endregion
    }
}
