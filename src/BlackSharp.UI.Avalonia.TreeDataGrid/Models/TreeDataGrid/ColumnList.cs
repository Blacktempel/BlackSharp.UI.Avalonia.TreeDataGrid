using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Models
{
    /// <summary>
    /// An implementation of <see cref="IColumns"/> that stores its columns in a list.
    /// </summary>
    internal class ColumnList<TModel> : AvaloniaList<IColumn<TModel>>, IColumns
    {
        #region Fields

        private bool   _initialized;

        /// <summary>
        /// The pixel width represented by one star in the most recent column layout.
        /// </summary>
        private double _starUnit;

        private double _viewportWidth;

        #endregion

        #region Public

        /// <inheritdoc/>
        public Size CellMeasured(int columnIndex, int rowIndex, Size size)
        {
            var column = (IUpdateColumnLayout)this[columnIndex];

            _initialized = true;

            return new Size(column.CellMeasured(size.Width, rowIndex), size.Height);
        }

        /// <inheritdoc/>
        public (int index, double x) GetColumnAt(double x)
        {
            var start = 0.0;

            for (var i = 0; i < Count; ++i)
            {
                var column = this[i];
                var end    = start + column.ActualWidth;

                if (x >= start && x < end)
                {
                    return (i, start);
                }

                if (double.IsNaN(column.ActualWidth))
                {
                    return (-1, -1);
                }

                start += column.ActualWidth;
            }

            return (-1, -1);
        }

        /// <inheritdoc/>
        public double GetEstimatedWidth(double constraint)
        {
            var hasStar                = false;
            var totalMeasured          = 0.0;
            var measuredCount          = 0;
            var unmeasuredCount        = 0;
            var unmeasuredMinimumWidth = 0.0;

            for (var i = 0; i < Count; ++i)
            {
                var column = (IUpdateColumnLayout)this[i];

                if (column.Width.IsStar)
                {
                    hasStar       = true;
                    totalMeasured += column.MinActualWidth;
                }
                else if (!double.IsNaN(column.ActualWidth))
                {
                    totalMeasured += column.ActualWidth;
                    ++measuredCount;
                }
                else
                {
                    ++unmeasuredCount;
                    unmeasuredMinimumWidth += column.MinActualWidth;
                }
            }

            if (hasStar)
            {
                // Unmeasured auto columns cannot be estimated from the widths of star columns:
                // doing so creates a spurious horizontal extent before their cells are realized.
                var minimumWidth = totalMeasured + unmeasuredMinimumWidth;

                return double.IsInfinity(constraint) ? minimumWidth : Math.Max(minimumWidth, constraint);
            }

            // If there are a mix of measured and unmeasured columns then use the measured columns
            // to estimate the size of the unmeasured columns.
            if (measuredCount > 0 && unmeasuredCount > 0)
            {
                var estimated = (totalMeasured / measuredCount) * unmeasuredCount;

                return totalMeasured + estimated;
            }

            return totalMeasured;
        }

        /// <inheritdoc/>
        public void CommitActualWidths() => UpdateColumnSizes();

        /// <inheritdoc/>
        public void SetColumnWidth(int columnIndex, GridLength width)
        {
            var column = this[columnIndex];

            if (width != column.Width)
            {
                ((IUpdateColumnLayout)column).SetWidth(width);
                LayoutInvalidated?.Invoke(this, EventArgs.Empty);
                UpdateColumnSizes();
            }
        }

        /// <inheritdoc/>
        public void ResizeColumn(int columnIndex, int adjacentColumnIndex, double delta)
        {
            if (!double.IsFinite(delta) || delta == 0)
            {
                return;
            }

            var column      = (IUpdateColumnLayout)this[columnIndex];
            var columnWidth = column.ActualWidth;

            if (!double.IsFinite(columnWidth))
            {
                return;
            }

            if (adjacentColumnIndex < 0)
            {
                var singleWidth = Math.Clamp(columnWidth + delta, column.MinActualWidth, column.MaxActualWidth);

                SetColumnWidth(columnIndex, new GridLength(singleWidth, GridUnitType.Pixel));

                return;
            }

            var adjacent      = (IUpdateColumnLayout)this[adjacentColumnIndex];
            var adjacentWidth = double.IsFinite(adjacent.ActualWidth) ? adjacent.ActualWidth : adjacent.MinActualWidth;

            var minimumDelta  = Math.Max(column.MinActualWidth - columnWidth, adjacentWidth - adjacent.MaxActualWidth);
            var maximumDelta  = Math.Min(column.MaxActualWidth - columnWidth, adjacentWidth - adjacent.MinActualWidth);

            if (!double.IsFinite(adjacentWidth) || minimumDelta > maximumDelta)
            {
                return;
            }

            delta = Math.Clamp(delta, minimumDelta, maximumDelta);

            if (delta == 0)
            {
                return;
            }

            var width                = GetResizedWidth(column, columnWidth + delta);
            var resizedAdjacentWidth = GetResizedWidth(adjacent, adjacentWidth - delta);

            // Apply both sides of the dragged boundary before recalculating star widths.
            adjacent.SetWidth(resizedAdjacentWidth);
            column.SetWidth(width);

            LayoutInvalidated?.Invoke(this, EventArgs.Empty);

            UpdateColumnSizes();
        }

        /// <inheritdoc/>
        public void ViewportChanged(Rect viewport)
        {
            if (_viewportWidth != viewport.Width)
            {
                _viewportWidth = viewport.Width;

                if (_initialized)
                {
                    UpdateColumnSizes();
                }
            }
        }

        #endregion

        #region Private

        private GridLength GetResizedWidth(IUpdateColumnLayout column, double width)
        {
            if (column.Width.IsStar && _starUnit > 0)
            {
                // Keep star sizing responsive to later viewport changes while changing its weight.
                var weight = width / _starUnit;

                if (double.IsFinite(weight))
                {
                    return new GridLength(weight, GridUnitType.Star);
                }
            }

            return new GridLength(width, GridUnitType.Pixel);
        }

        private void UpdateColumnSizes()
        {
            var availableSpace = _viewportWidth;
            var invalidated    = false;

            // First commit the actual width for all non-star width columns and get a total of the
            // number of stars for star width columns.
            for (var i = 0; i < Count; ++i)
            {
                var column = (IUpdateColumnLayout)this[i];

                if (!column.Width.IsStar)
                {
                    invalidated    |= column.CommitActualWidth();

                    // Reserve the minimum for an auto column that has not been realized yet.
                    availableSpace -= double.IsNaN(column.ActualWidth) ? column.MinActualWidth : column.ActualWidth;
                }
            }

            var stars = this.OfType<IUpdateColumnLayout>().Where(x => x.Width.IsStar).ToArray();

            if (stars.Length > 0)
            {
                var minimum = stars.Sum(x => Math.Min(x.MinActualWidth, x.MaxActualWidth));
                var maximum = stars.Sum(x => x.Width.Value > 0 ? x.MaxActualWidth : Math.Min(x.MinActualWidth, x.MaxActualWidth));
                var target  = Math.Min(Math.Max(availableSpace, minimum), maximum);
                var weight  = stars.Where(x => x.Width.Value > 0).Select(x => x.Width.Value).DefaultIfEmpty(1).Min();
                var lower   = 0.0;
                var upper   = Math.Max(0, target / weight);

                // Find the common star unit after applying every column's minimum and maximum.
                // Using the previous ActualWidth here gives incorrect results on the first layout.
                for (var pass = 0; pass < 64; ++pass)
                {
                    var unit = lower + (upper - lower) / 2;
                    var used = stars.Sum(x => Math.Min(Math.Max(unit * x.Width.Value, x.MinActualWidth), x.MaxActualWidth));

                    if (used < target)
                    {
                        lower = unit;
                    }
                    else
                    {
                        upper = unit;
                    }
                }

                _starUnit = upper;

                foreach (var column in stars)
                {
                    column.CalculateStarWidth(upper, 1);
                    invalidated |= column.CommitActualWidth();
                }
            }
            else
            {
                _starUnit = 0;
            }

            if (invalidated)
            {
                LayoutInvalidated?.Invoke(this, EventArgs.Empty);
            }
        }

        #endregion

        #region IColumns

        void IColumns.InvalidateLayout() =>
            LayoutInvalidated?.Invoke(this, EventArgs.Empty);

        #endregion

        #region IEnumerable<IColumn>

        IEnumerator<IColumn> IEnumerable<IColumn>.GetEnumerator() => GetEnumerator();

        #endregion

        #region IReadOnlyList<IColumn>

        IColumn IReadOnlyList<IColumn>.this[int index] => this[index];

        #endregion

        #region Events

        /// <inheritdoc/>
        public event EventHandler? LayoutInvalidated;

        #endregion
    }
}
