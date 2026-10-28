using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using BlackSharp.UI.Avalonia.TreeDataGrid.Models;
using Xunit;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Tests.Models
{
    public class ColumnListTests
    {
        [AvaloniaFact]
        public void Columns_Are_Sized_At_End_Of_Measure()
        {
            var target = new ColumnList<Model>
            {
                new TextColumn<Model, string?>(null, x => x.Name, new GridLength(100, GridUnitType.Pixel)),
                new TextColumn<Model, string?>(null, x => x.Name, GridLength.Auto),
                new TextColumn<Model, string?>(null, x => x.Name, new GridLength(1, GridUnitType.Star)),
                new TextColumn<Model, string?>(null, x => x.Name, new GridLength(3, GridUnitType.Star)),
            };

            target.ViewportChanged(new Rect(0, 0, 500, 500));

            for (var row = 0; row < 10; ++row)
            {
                for (var col = 0; col < target.Count; ++col)
                {
                    target.CellMeasured(col, row, new Size(51 + row, 10));
                }
            }

            target.CommitActualWidths();

            Assert.Equal(100, target[0].ActualWidth);
            Assert.Equal(60, target[1].ActualWidth);
            Assert.Equal(85, target[2].ActualWidth);
            Assert.Equal(255, target[3].ActualWidth);
        }

        [AvaloniaFact]
        public void Layout_Is_Invalidated_At_End_Of_Measure_If_AutoSized_Column_Changes_Width()
        {
            var target = new ColumnList<Model>
            {
                new TextColumn<Model, string?>(null, x => x.Name, GridLength.Auto),
                new TextColumn<Model, string?>(null, x => x.Country, GridLength.Auto),
            };


            target.ViewportChanged(new Rect(0, 0, 500, 500));

            for (var row = 0; row < 10; ++row)
            {
                for (var col = 0; col < target.Count; ++col)
                {
                    target.CellMeasured(col, row, new Size(40, 10));
                }
            }

            target.CommitActualWidths();

            target.CellMeasured(0, 1, new Size(50, 10));

            var raised = 0;
            target.LayoutInvalidated += (s, e) => ++raised;

            target.CommitActualWidths();

            Assert.Equal(1, raised);
        }

        /// <summary>
        /// Verifies that resizing a model column also updates its declarative column width.
        /// </summary>
        [AvaloniaFact]
        public void Resizing_Adapted_Column_Updates_Declarative_Width()
        {
            // Arrange
            var column = new TreeDataGridTextColumn { Width = new GridLength(1, GridUnitType.Star) };
            using var adapter = new TreeDataGridColumnAdapter<Model>(column);
            var target = new ColumnList<Model> { adapter };
            var width  = new GridLength(180, GridUnitType.Pixel);

            target.ViewportChanged(new Rect(0, 0, 500, 100));

            // Act
            target.SetColumnWidth(0, width);

            // Assert
            Assert.Equal(width, adapter.Width);
            Assert.Equal(width, column.Width);
            Assert.Equal(180, adapter.ActualWidth);
        }

        /// <summary>
        /// Verifies that dragging the boundary after a star column moves that boundary, not an earlier one.
        /// </summary>
        [AvaloniaFact]
        public void Resizing_Middle_Star_Column_Keeps_Preceding_Column_In_Place()
        {
            // Arrange
            var target = new ColumnList<Model>
            {
                new TextColumn<Model, string?>(null, x => x.Name, new GridLength(40, GridUnitType.Pixel)),
                new TextColumn<Model, string?>(null, x => x.Name, new GridLength(7, GridUnitType.Star)),
                new TextColumn<Model, string?>(null, x => x.Name, new GridLength(6, GridUnitType.Star)),
                new TextColumn<Model, string?>(null, x => x.Name, new GridLength(300, GridUnitType.Pixel)),
                new TextColumn<Model, string?>(null, x => x.Name, new GridLength(40, GridUnitType.Pixel)),
            };

            target.ViewportChanged(new Rect(0, 0, 1100, 100));

            for (var columnIndex = 0; columnIndex < target.Count; ++columnIndex)
            {
                target.CellMeasured(columnIndex, 0, new Size(20, 10));
            }

            target.CommitActualWidths();

            var nameWidth   = target[1].ActualWidth;
            var detailWidth = target[2].ActualWidth;
            var valueWidth  = target[3].ActualWidth;

            // Act
            target.ResizeColumn(2, 3, -50);

            // Assert
            Assert.Equal(nameWidth, target[1].ActualWidth, 2);
            Assert.Equal(detailWidth - 50, target[2].ActualWidth, 2);
            Assert.Equal(valueWidth + 50, target[3].ActualWidth, 2);
            Assert.True(target[2].Width.IsStar);

            target.ResizeColumn(2, 3, 50);

            Assert.Equal(nameWidth, target[1].ActualWidth, 2);
            Assert.Equal(detailWidth, target[2].ActualWidth, 2);
            Assert.Equal(valueWidth, target[3].ActualWidth, 2);

            target.ViewportChanged(new Rect(0, 0, 1200, 100));

            Assert.Equal(1200, ((IColumns)target).Sum(x => x.ActualWidth), 2);
        }

        /// <summary>
        /// Verifies that the dragged boundary stops when either adjoining column reaches its minimum.
        /// </summary>
        [AvaloniaFact]
        public void Resizing_Adjacent_Columns_Respects_Both_Minimum_Widths()
        {
            // Arrange
            var options = new TextColumnOptions<Model> { MinWidth = new GridLength(240, GridUnitType.Pixel) };
            var target  = new ColumnList<Model>
            {
                new TextColumn<Model, string?>(null, x => x.Name, new GridLength(6, GridUnitType.Star), options),
                new TextColumn<Model, string?>(null, x => x.Name, new GridLength(300, GridUnitType.Pixel),
                    new TextColumnOptions<Model> { MinWidth = new GridLength(240, GridUnitType.Pixel) }),
            };

            target.ViewportChanged(new Rect(0, 0, 600, 100));
            target.CellMeasured(0, 0, new Size(20, 10));
            target.CellMeasured(1, 0, new Size(20, 10));
            target.CommitActualWidths();

            // Act
            target.ResizeColumn(0, 1, 200);

            // Assert
            Assert.Equal(360, target[0].ActualWidth, 2);
            Assert.Equal(240, target[1].ActualWidth, 2);

            target.ResizeColumn(0, 1, -200);

            Assert.Equal(240, target[0].ActualWidth, 2);
            Assert.Equal(360, target[1].ActualWidth, 2);
        }

        /// <summary>
        /// Verifies that a star column at its minimum can grow without shifting columns to its left.
        /// </summary>
        [AvaloniaFact]
        public void Resizing_Constrained_Star_Column_Keeps_Preceding_Star_Width()
        {
            // Arrange
            var target = new ColumnList<Model>
            {
                new TextColumn<Model, string?>(null, x => x.Name, new GridLength(40, GridUnitType.Pixel)),
                new TextColumn<Model, string?>(null, x => x.Name, new GridLength(7, GridUnitType.Star)),
                new TextColumn<Model, string?>(null, x => x.Name, new GridLength(6, GridUnitType.Star),
                    new TextColumnOptions<Model> { MinWidth = new GridLength(400, GridUnitType.Pixel) }),
                new TextColumn<Model, string?>(null, x => x.Name, new GridLength(300, GridUnitType.Pixel),
                    new TextColumnOptions<Model> { MinWidth = new GridLength(240, GridUnitType.Pixel) }),
                new TextColumn<Model, string?>(null, x => x.Name, new GridLength(40, GridUnitType.Pixel)),
            };

            target.ViewportChanged(new Rect(0, 0, 1100, 100));

            for (var columnIndex = 0; columnIndex < target.Count; ++columnIndex)
            {
                target.CellMeasured(columnIndex, 0, new Size(20, 10));
            }

            target.CommitActualWidths();

            var precedingWidth = target[1].ActualWidth;

            // Act
            target.ResizeColumn(2, 3, 40);

            // Assert
            Assert.Equal(precedingWidth, target[1].ActualWidth, 2);
            Assert.Equal(440, target[2].ActualWidth, 2);
            Assert.Equal(260, target[3].ActualWidth, 2);
        }

        private class Model
        {
            public string? Name { get; set; }
            public string? Country { get; set; }
        }
    }
}
