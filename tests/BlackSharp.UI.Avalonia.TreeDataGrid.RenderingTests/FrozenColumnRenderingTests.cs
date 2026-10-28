using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using BlackSharp.UI.Avalonia.TreeDataGrid.Demo;
using BlackSharp.UI.Avalonia.TreeDataGrid.Demo.Models;
using BlackSharp.UI.Avalonia.TreeDataGrid.Primitives;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Xunit;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.RenderingTests
{
    public class FrozenColumnRenderingTests
    {
        #region Public

        [AvaloniaFact]
        public void Demo_Frozen_Cells_Render_In_Light_Theme() => VerifyDemo(false, 1.0);

        [AvaloniaFact]
        public void Demo_Frozen_Cells_Render_In_Dark_Theme() => VerifyDemo(true, 1.5);

        #endregion

        #region Private

        private static void VerifyDemo(bool dark, double scale)
        {
            var window = new MainWindow
            {
                Width                 = 1200,
                Height                = 800,
                RequestedThemeVariant = dark ? ThemeVariant.Dark : ThemeVariant.Light,
            };

            try
            {
                window.Show();
                window.SetRenderScaling(scale);
                Layout(window);
                var grid   = window.FindControl<TreeDataGrid>("features")!;
                var source = Assert.IsType<HierarchicalTreeDataGridSource<ShowcaseRow>>(grid.Source);

                for (var pass = 0; pass < 3; ++pass)
                {
                    source.Collapse(0);
                    Layout(window);
                    source.Expand(0);
                    Layout(window);
                }

                var options = window.GetVisualDescendants().OfType<CheckBox>().ToArray();
                options.Single(x => Equals(x.Content, "Double-tap expansion")).IsChecked = true;
                options.Single(x => Equals(x.Content, "Left arrow to parent")).IsChecked = true;
                options.Single(x => Equals(x.Content, "Ctrl+arrows scroll")).IsChecked = true;
                Assert.True(grid.ExpandCollapseOnDoubleTap);
                Assert.True(grid.SelectParentOnLeftArrow);
                Assert.True(grid.PreserveSelectionOnCtrlArrowScroll);

                var items = Assert.IsType<ObservableCollection<ShowcaseRow>>(source.Items);
                var selected = items[0].Children[0];
                var rowCount = source.Rows.Count;

                grid.SelectedIndex = new IndexPath(0, 0);
                items.Move(0, 3);
                Layout(window);
                Assert.Equal(rowCount, source.Rows.Count);
                Assert.Equal(new IndexPath(3, 0), grid.SelectedIndex);
                Assert.Same(selected, grid.SelectedItem);
                Assert.Equal(new IndexPath(3).ToString(), ((TreeDataGridTextCell)grid.TryGetCell(0, 3)!).Value);
                Assert.Equal(new IndexPath(3, 0).ToString(), ((TreeDataGridTextCell)grid.TryGetCell(0, 4)!).Value);
                AssertNoOrphanedControls(grid);
                SaveFrame(window, dark, scale, "moved");
                items.Move(3, 0);
                Layout(window);
                AssertNoOrphanedControls(grid);

                grid.SelectedIndex = 3;
                SaveFrame(window, dark, scale, "expanded");
                AssertNoOrphanedControls(grid);
                grid.Scroll!.Offset = new Vector(600, 350);
                Layout(window);
                SaveFrame(window, dark, scale, "scrolled");
                AssertNoOrphanedControls(grid);
                var firstRow = grid.RowsPresenter!.GetRealizedElements().OfType<TreeDataGridRow>().First();

                for (var i = 0; i < grid.FrozenColumnCount; ++i)
                {
                    var header = grid.ColumnHeadersPresenter!.TryGetElement(i)!;
                    var cell   = firstRow.TryGetCell(i)!;

                    Assert.Equal(source.Columns[i].Header, ((TreeDataGridColumnHeader)header).Header);
                    Assert.Equal(cell.TranslatePoint(default, grid)!.Value.X,
                        header.TranslatePoint(default, grid)!.Value.X, 3);
                }
                source.SortBy(source.Columns[1], ListSortDirection.Descending);
                Layout(window);
                AssertNoOrphanedControls(grid);
                source.ClearSort(source.Columns[1]);
                source.Filter(row => row.Name.Contains("001", StringComparison.Ordinal));
                Layout(window);
                AssertNoOrphanedControls(grid);
                source.Filter(null);
                grid.Scroll.Offset = default;
                Layout(window);

                var text = Assert.IsType<TreeDataGridTextColumn>(Assert.IsType<TreeDataGridHierarchicalExpanderColumn>(source.Columns[1]).Inner);

                text.UseDrawnCells = false;
                Layout(window);
                SaveFrame(window, dark, scale, "controls");
                AssertNoOrphanedControls(grid);
                text.UseDrawnCells = true;
                grid.FrozenColumnCount = 0;
                Layout(window);
                grid.FrozenColumnCount = 2;
                Layout(window);
                AssertNoOrphanedControls(grid);
                var tabs = window.FindControl<TabControl>("tabs")!;

                foreach (var tab in tabs.Items.Cast<TabItem>())
                {
                    tabs.SelectedItem = tab;
                    Layout(window);
                    var current = ((Control)tab.Content!).GetVisualDescendants().OfType<TreeDataGrid>().First();

                    AssertNoOrphanedControls(current);
                    SaveFrame(window, dark, scale, $"tab-{tab.Header}".Replace('/', '-'));
                }
            }
            finally
            {
                window.Close();
            }
        }

        private static void AssertNoOrphanedControls(TreeDataGrid grid)
        {
            AssertPresenter(grid.RowsPresenter!);
            AssertPresenter(grid.ColumnHeadersPresenter!);

            foreach (var row in grid.RowsPresenter!.GetRealizedElements().OfType<TreeDataGridRow>())
            {
                AssertPresenter(row.CellsPresenter!);
                var cells = row.CellsPresenter!.GetRealizedElements().OfType<TreeDataGridCell>().ToArray();

                Assert.Equal(cells.Length, cells.Select(cell => cell.ColumnIndex).Distinct().Count());

                if (row.Model is ShowcaseRow model)
                {
                    Assert.Equal(model.Name, row.TryGetCell(1) is TreeDataGridCell name ? name.Model!.Value : null);

                    foreach (var content in cells.SelectMany(cell => cell.GetVisualDescendants().OfType<TreeDataGridDrawnCellContent>()))
                    {
                        Assert.True(content.Bounds.Width > 0 && content.Bounds.Height > 0,
                            $"Drawn content has empty bounds in row {row.RowIndex}: {content.Bounds}.");
                    }
                }
            }
        }

        private static void AssertPresenter<TItem>(TreeDataGridPresenterBase<TItem> presenter)
        {
            var visible  = presenter.GetVisualChildren().OfType<Control>().Where(child => child.IsVisible).ToArray();
            var realized = presenter.GetRealizedElements().ToArray();

            Assert.Equal(realized.Length, visible.Length);

            foreach (var child in visible)
            {
                Assert.Contains(child, realized);
            }
        }

        private static void Layout(Window window)
        {
            for (var frame = 0; frame < 3; ++frame)
            {
                window.UpdateLayout();
                Dispatcher.UIThread.RunJobs();
                AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            }

            window.UpdateLayout();
        }

        private static void AssertHeaderTextIsRendered(Bitmap frame, Control header, Window window, double scale, bool dark)
        {
            var position = header.TranslatePoint(default, window)!.Value;
            var rect     = new PixelRect(
                (int)((position.X + 4) * scale), (int)((position.Y + 3) * scale),
                (int)((header.Bounds.Width - 10) * scale), (int)((header.Bounds.Height - 6) * scale));
            var stride   = rect.Width * 4;
            var pixels   = new byte[stride * rect.Height];
            var buffer   = Marshal.AllocHGlobal(pixels.Length);

            try
            {
                frame.CopyPixels(rect, buffer, pixels.Length, stride);
                Marshal.Copy(buffer, pixels, 0, pixels.Length);
                var foregroundPixels = 0;

                for (var pixel = 0; pixel < pixels.Length; pixel += 4)
                {
                    var intensity = (pixels[pixel] + pixels[pixel + 1] + pixels[pixel + 2]) / 3;

                    if (dark ? intensity > 170 : intensity < 85)
                    {
                        ++foregroundPixels;
                    }
                }

                Assert.True(foregroundPixels > 4, "The frozen header text is missing from the compositor frame.");
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        private static void SaveFrame(Window window, bool dark, double scale, string state)
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);

            while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "BlackSharp.UI.Avalonia.TreeDataGrid.sln")))
            {
                directory = directory.Parent;
            }

            Assert.NotNull(directory);
            var output = Path.Combine(directory.FullName, "artifacts", "rendering", $"net{Environment.Version.Major}.0");

            Directory.CreateDirectory(output);
            using var frame = window.CaptureRenderedFrame();

            Assert.NotNull(frame);
            Assert.Equal((int)(window.Bounds.Width * scale), frame.PixelSize.Width);
            Assert.Equal((int)(window.Bounds.Height * scale), frame.PixelSize.Height);
            frame.Save(Path.Combine(output, $"features-{(dark ? "dark" : "light")}-{state}.png"), new PngBitmapEncoderOptions());

            if (state == "scrolled")
            {
                var grid = window.FindControl<TreeDataGrid>("features")!;

                for (var i = 0; i < grid.FrozenColumnCount; ++i)
                {
                    AssertHeaderTextIsRendered(frame, grid.ColumnHeadersPresenter!.TryGetElement(i)!, window, scale, dark);
                }
            }
        }

        #endregion
    }
}
