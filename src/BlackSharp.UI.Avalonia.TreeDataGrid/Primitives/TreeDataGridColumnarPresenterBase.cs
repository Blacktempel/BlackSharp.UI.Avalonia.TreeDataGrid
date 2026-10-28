using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using BlackSharp.UI.Avalonia.TreeDataGrid.Models;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Primitives
{
    /// <summary>
    /// Base class for presenters which display data in virtualized columns.
    /// </summary>
    /// <typeparam name="TItem">The item type.</typeparam>
    /// <remarks>
    /// Implements common layout functionality between <see cref="TreeDataGridCellsPresenter"/>
    /// and <see cref="TreeDataGridColumnHeadersPresenter"/>.
    /// </remarks>
    public abstract class TreeDataGridColumnarPresenterBase<TItem> : TreeDataGridPresenterBase<TItem>
    {
        #region Fields

        public static readonly StyledProperty<int> FrozenColumnCountProperty =
            TreeDataGridPresenterProperties.FrozenColumnCountProperty;

        #endregion

        #region Properties

        public int FrozenColumnCount
        {
            get => GetValue(FrozenColumnCountProperty);
            set => SetValue(FrozenColumnCountProperty, value);
        }

        private protected IColumns? Columns => Items as IColumns;

        protected override int PinnedCount => FrozenColumnCount;

        #endregion

        #region Protected

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == FrozenColumnCountProperty)
            {
                InvalidateMeasure();
                InvalidateArrange();
            }
        }

        protected override Rect ArrangeElement(int index, Control element, Rect rect)
        {
            var count        = Math.Clamp(PinnedCount, 0, Columns?.Count ?? 0);
            var scrollOffset = double.IsFinite(Viewport.X) ? Math.Max(0, Viewport.X) : 0;

            if (index < count)
            {
                element.ZIndex = 1;
                element.Clip   = null;

                if (element.RenderTransform is TranslateTransform translation)
                {
                    translation.X = scrollOffset;
                }
                else
                {
                    element.RenderTransform = new TranslateTransform(scrollOffset, 0);
                }

                element.Arrange(rect);
            }
            else
            {
                var frozenWidth = 0.0;

                for (var i = 0; i < count; ++i)
                {
                    var width = Columns![i].ActualWidth;

                    frozenWidth += double.IsFinite(width) ? width : 0;
                }

                var clip = Math.Clamp(scrollOffset + frozenWidth - rect.X, 0, rect.Width);

                element.ZIndex          = 0;
                element.RenderTransform = null;
                element.Clip            = count > 0 ? new RectangleGeometry(new Rect(clip, 0, rect.Width - clip, rect.Height)) : null;
                element.Arrange(rect);
            }

            return rect;
        }

        protected sealed override Size GetInitialConstraint(Control element, int index, Size availableSize)
        {
            var column = (IUpdateColumnLayout)Columns![index];

            return new Size(Math.Min(availableSize.Width, column.MaxActualWidth), availableSize.Height);
        }

        protected override (int index, double position) GetOrEstimateAnchorElementForViewport(
            double viewportStart,
            double viewportEnd,
            int itemCount)
        {
            if (Columns?.GetColumnAt(viewportStart) is var (index, position) && index >= 0)
            {
                return (index, position);
            }

            return base.GetOrEstimateAnchorElementForViewport(viewportStart, viewportEnd, itemCount);
        }

        protected sealed override bool NeedsFinalMeasurePass(int firstIndex, IReadOnlyList<Control?> elements)
        {
            var columns = Columns!;

            columns.CommitActualWidths();

            // We need to do a second measure pass if any of the controls were measured with a width
            // that is greater than the final column width.
            for (var i = 0; i < elements.Count; i++)
            {
                var e = elements[i];

                if (e is not null)
                {
                    var previous = LayoutInformation.GetPreviousMeasureConstraint(e)!.Value;

                    if (previous.Width > columns[i + firstIndex].ActualWidth)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        protected sealed override (int index, double position) GetElementAt(double position)
        {
            return ((IColumns)Items!).GetColumnAt(position);
        }

        protected sealed override Size GetFinalConstraint(Control element, int index, Size availableSize)
        {
            var column = Columns![index];
            var width  = column.ActualWidth;

            return new(double.IsNaN(width) ? 0d : width, double.PositiveInfinity);
        }

        protected sealed override double CalculateSizeU(Size availableSize)
        {
            return Columns?.GetEstimatedWidth(availableSize.Width) ?? 0;
        }

        #endregion
    }
}
