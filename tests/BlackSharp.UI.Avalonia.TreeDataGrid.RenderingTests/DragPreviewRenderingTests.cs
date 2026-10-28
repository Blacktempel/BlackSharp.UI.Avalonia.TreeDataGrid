using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using Xunit;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.RenderingTests
{
    public class DragPreviewRenderingTests
    {
        #region Public

        [AvaloniaTheory]
        [InlineData(false, 1.0, false)]
        [InlineData(false, 1.0, true)]
        [InlineData(true, 1.5, false)]
        [InlineData(true, 1.5, true)]
        public void Default_Preview_Renders_The_Row_And_Survives_Source_Recycling(bool dark, double scale, bool scrolled)
        {
            var items = new ObservableCollection<Model>(Enumerable.Range(0, 80).Select(x => new Model($"Item {x:000}")));
            using var source = new FlatTreeDataGridSource<Model>(items)
                .WithTextColumn(x => x.Name, options =>
                {
                    options.IsReadOnly = true;
                    options.UseDrawnCells = true;
                    options.Width = new GridLength(180);
                })
                .WithCheckBoxColumn("Active", x => x.Active, options => options.IsReadOnly = true);

            for (var i = 0; i < 8; ++i)
            {
                source.WithTextColumn($"Value {i}", x => x.Name, options =>
                {
                    options.Width = new GridLength(150);
                    options.IsReadOnly = true;
                });
            }

            var grid = new DragGrid
            {
                Source = source,
                AutoDragDropRows = true,
                DragPreviewOpacity = 1,
                FrozenColumnCount = 1,
            };
            var window = Show(grid, dark, scale);

            try
            {
                if (scrolled)
                {
                    grid.Scroll!.Offset = new Vector(500, 0);
                    Layout(window);
                }

                var row = grid.TryGetRow(0)!;
                var originalModel = row.Model;
                var release = StartDrag(window, grid);
                var preview = Assert.Single(Previews(window));
                var snapshot = Assert.IsType<RenderTargetBitmap>(Assert.IsType<Image>(preview.Child).Source);
                Assert.Same(originalModel, preview.DataContext);
                Assert.InRange(snapshot.PixelSize.Width, (int)(400 * scale), (int)(window.Bounds.Width * scale));
                Assert.InRange(snapshot.PixelSize.Height, (int)(15 * scale), (int)(80 * scale));
                var pixels = ReadPixels(snapshot);
                var name = $"drag-{(dark ? "dark" : "light")}-{(scrolled ? "scrolled" : "start")}";
                Save(window, name, snapshot);
                AssertPreviewGlyphsMatchSnapshot(window, Assert.IsType<Image>(preview.Child), snapshot, scale, dark);
                var opaquePixels = 0;
                var textPixels = 0;

                for (var i = 0; i < pixels.Length; i += 4)
                {
                    opaquePixels += pixels[i + 3] > 0 ? 1 : 0;
                    var foreground = dark ? pixels[i] > 190 && pixels[i + 1] > 190 && pixels[i + 2] > 190 :
                        pixels[i] < 80 && pixels[i + 1] < 80 && pixels[i + 2] < 80;
                    textPixels += foreground && pixels[i + 3] > 0 ? 1 : 0;
                }

                Assert.InRange(preview.Bounds.Height, row.Bounds.Height + 1, row.Bounds.Height + 3);
                Assert.True(Canvas.GetTop(preview) > 0, "The preview was placed at the window origin instead of below the pointer.");
                Assert.True(opaquePixels > 100, "The default row preview is empty.");
                Assert.True(textPixels > 20, "The row preview does not contain the selected row's foreground text.");
                window.FocusManager!.Focus(null);
                grid.Scroll!.Offset = new Vector(scrolled ? 500 : 0, 1000);
                Layout(window);
                Assert.NotSame(originalModel, row.Model);
                Assert.Same(originalModel, preview.DataContext);
                Assert.Equal(pixels, ReadPixels(snapshot));
                Save(window, name + "-recycled");
                grid.Complete();
                Layout(window);
                Assert.Empty(Previews(window));
                Assert.Null(preview.Child);
                window.MouseUp(release, MouseButton.Left);
            }
            finally
            {
                grid.Complete();
                window.Close();
            }
        }

        [AvaloniaFact]
        public void Custom_Template_Renders_In_Place_Of_The_Row_Snapshot()
        {
            using var source = new FlatTreeDataGridSource<Model>(new[] { new Model("Item 000") })
                .WithTextColumn(x => x.Name, options => options.IsReadOnly = true);
            var grid = new DragGrid
            {
                Source = source,
                AutoDragDropRows = true,
                DragPreviewTemplate = new FuncDataTemplate<Model>((model, _) => new Border
                {
                    Padding = new Thickness(14, 8),
                    Background = Brushes.DarkSlateBlue,
                    Child = new TextBlock { Text = "Custom preview: " + model!.Name, Foreground = Brushes.White },
                }),
            };
            var window = Show(grid, true, 1.5);

            try
            {
                var release = StartDrag(window, grid);
                var preview = Assert.Single(Previews(window));
                Assert.IsType<ContentControl>(preview.Child);
                Assert.Equal("Custom preview: Item 000", Assert.Single(preview.GetVisualDescendants().OfType<TextBlock>()).Text);
                Save(window, "drag-custom-template");
                grid.Complete();
                Layout(window);
                Assert.Empty(Previews(window));
                window.MouseUp(release, MouseButton.Left);
            }
            finally
            {
                grid.Complete();
                window.Close();
            }
        }

        #endregion

        #region Private

        private static byte[] ReadPixels(Bitmap bitmap, PixelRect? area = null)
        {
            var rect = area ?? new PixelRect(bitmap.PixelSize);
            var stride = rect.Width * 4;
            var pixels = new byte[stride * rect.Height];
            var pointer = Marshal.AllocHGlobal(pixels.Length);

            try
            {
                bitmap.CopyPixels(rect, pointer, pixels.Length, stride);
                Marshal.Copy(pointer, pixels, 0, pixels.Length);

                return pixels;
            }
            finally
            {
                Marshal.FreeHGlobal(pointer);
            }
        }

        private static void AssertPreviewGlyphsMatchSnapshot(Window window, Image image, Bitmap snapshot, double scale, bool dark)
        {
            var width = (int)(175 * scale);
            var height = snapshot.PixelSize.Height - 2;
            var captured = ReadPixels(snapshot, new PixelRect(0, 0, width, height));
            var expected = ForegroundDistribution(captured);

            try
            {
                foreach (var offset in new[] { 0, 0.25, 0.5, 0.75 })
                {
                    // Exercise subpixel bitmap sampling as well as the platform's normal text rasterization.
                    image.RenderTransform = new TranslateTransform(offset / scale, 0);
                    Layout(window);
                    using var frame = window.CaptureRenderedFrame();
                    Assert.NotNull(frame);
                    var position = image.TranslatePoint(default, window)!.Value;
                    var displayed = ReadPixels(frame, new PixelRect(
                        (int)Math.Ceiling(position.X * scale), (int)Math.Ceiling(position.Y * scale), width, height));
                    var actual = ForegroundDistribution(displayed);

                    // Coverage-weighted moments tolerate antialiasing/resampling without accepting a DPI scale error.
                    Assert.True(Math.Abs(expected.Center - actual.Center) <= 1.5,
                        $"Offset {offset}: expected {expected}; actual {actual}; image {image.Bounds}; at {position}; snapshot {snapshot.PixelSize}.");
                    Assert.InRange(Math.Abs(expected.Spread - actual.Spread), 0, 1.5);
                }
            }
            finally
            {
                image.RenderTransform = null;
                Layout(window);
            }

            (double Center, double Spread) ForegroundDistribution(byte[] pixels)
            {
                var backgroundOffset = ((height / 2) * width + width - 1) * 4;
                var foreground = dark ? 255 : 0;
                var channel = Enumerable.Range(0, 3)
                    .MaxBy(i => Math.Abs(foreground - pixels[backgroundOffset + i]));
                var background = pixels[backgroundOffset + channel];
                var contrast = foreground - background;
                Assert.NotEqual(0, contrast);
                double weight = 0;
                double sum = 0;
                double squared = 0;

                for (var y = 2; y < height - 2; ++y)
                {
                    for (var x = (int)Math.Ceiling(2 * scale); x < width - 2; ++x)
                    {
                        var offset = (y * width + x) * 4;
                        // Preserve partial glyph coverage instead of dropping antialiased edge pixels at a cutoff.
                        var coverage = Math.Clamp((double)(pixels[offset + channel] - background) / contrast, 0, 1) *
                            pixels[offset + 3] / 255;
                        weight += coverage;
                        sum += x * coverage;
                        squared += x * x * coverage;
                    }
                }

                Assert.True(weight > 20, "The preview's name column contains no rendered text.");
                var center = sum / weight;

                return (center, Math.Sqrt(Math.Max(0, squared / weight - center * center)));
            }
        }

        private static Border[] Previews(Window window) => window.GetVisualDescendants().OfType<Border>()
            .Where(x => x.Classes.Contains("tree-data-grid-row-drag-preview")).ToArray();

        private static Point StartDrag(Window window, TreeDataGrid grid)
        {
            var cell = grid.TryGetCell(0, 0)!;
            var start = cell.TranslatePoint(new Point(40, cell.Bounds.Height / 2), window)!.Value;
            var end = start + new Vector(18, 0);
            window.MouseDown(start, MouseButton.Left);
            window.MouseMove(end, RawInputModifiers.LeftMouseButton);
            Layout(window);

            return end;
        }

        private static Window Show(TreeDataGrid grid, bool dark, double scale)
        {
            var window = new Window
            {
                Width = 640,
                Height = 400,
                Content = grid,
                RequestedThemeVariant = dark ? ThemeVariant.Dark : ThemeVariant.Light,
            };
            window.Show();
            window.SetRenderScaling(scale);
            Layout(window);

            return window;
        }

        private static void Layout(Window window)
        {
            for (var pass = 0; pass < 3; ++pass)
            {
                window.UpdateLayout();
                Dispatcher.UIThread.RunJobs();
                AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            }
        }

        private static void Save(Window window, string name, Bitmap? snapshot = null)
        {
            var root = new DirectoryInfo(AppContext.BaseDirectory);

            while (root is not null && !File.Exists(Path.Combine(root.FullName, "BlackSharp.UI.Avalonia.TreeDataGrid.sln")))
            {
                root = root.Parent;
            }

            Assert.NotNull(root);
            var output = Path.Combine(root.FullName, "artifacts", "rendering", $"net{Environment.Version.Major}.0");
            Directory.CreateDirectory(output);
            using var frame = window.CaptureRenderedFrame();
            Assert.NotNull(frame);
            frame.Save(Path.Combine(output, name + ".png"), new PngBitmapEncoderOptions());
            snapshot?.Save(Path.Combine(output, name + "-snapshot.png"), new PngBitmapEncoderOptions());
        }

        #endregion

        #region Nested Types

        private sealed record Model(string Name)
        {
            public bool Active => true;
        }

        private sealed class DragGrid : TreeDataGrid
        {
            private readonly TaskCompletionSource<DragDropEffects> _completion = new();

            protected override Type StyleKeyOverride => typeof(TreeDataGrid);

            public void Complete() => _completion.TrySetResult(DragDropEffects.None);

            protected override Task<DragDropEffects> DoRowDragDropAsync(
                PointerPressedEventArgs trigger, IDataTransfer dataTransfer, DragDropEffects allowedEffects) => _completion.Task;
        }

        #endregion
    }
}
