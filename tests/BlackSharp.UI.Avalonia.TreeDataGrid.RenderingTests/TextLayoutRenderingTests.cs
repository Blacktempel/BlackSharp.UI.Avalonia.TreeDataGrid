using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using BlackSharp.UI.Avalonia.TreeDataGrid.Primitives;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Xunit;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.RenderingTests
{
    public class TextLayoutRenderingTests
    {
        #region Public

        [AvaloniaTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void Cached_Text_Updates_For_Mutable_Brush_Font_Content_And_Wrapping(bool dark)
        {
            var model = new Model();
            using var source = new FlatTreeDataGridSource<Model>(new[] { model })
                .WithTextColumn(x => x.Name, options =>
                {
                    options.IsReadOnly = true;
                    options.UseDrawnCells = true;
                    options.Width = new GridLength(300);
                });
            var brush = new SolidColorBrush(Colors.Blue);
            var grid = new TreeDataGrid { Source = source, FontSize = 14, Foreground = brush };
            var window = new Window
            {
                Width = 480,
                Height = 300,
                Content = grid,
                RequestedThemeVariant = dark ? ThemeVariant.Dark : ThemeVariant.Light,
            };

            try
            {
                window.Show();
                window.SetRenderScaling(dark ? 1.5 : 1.0);
                Layout(window);
                var cell = Assert.IsType<TreeDataGridDrawnTextCell>(grid.TryGetCell(0, 0));
                var originalHeight = cell.Bounds.Height;
                var before = CaptureCell(window, cell, dark ? 1.5 : 1.0);
                Assert.True(CountColor(before, false) > 5);
                brush.Color = Colors.Red;
                Layout(window);
                var red = CaptureCell(window, cell, dark ? 1.5 : 1.0);
                Assert.True(CountColor(red, true) > 5);
                Assert.Equal(0, CountColor(red, false));
                grid.FontSize = 24;
                Layout(window);
                Assert.True(cell.Bounds.Height > originalHeight);
                model.Name = "Updated hardware sensor with a longer label";
                Layout(window);
                Assert.Equal(model.Name, cell.Value);
                Assert.True(CountColor(CaptureCell(window, cell, dark ? 1.5 : 1.0), true) > 5);
                var singleLine = cell.Bounds.Height;
                ((TreeDataGridTextColumn)source.Columns[0]).TextWrapping = TextWrapping.Wrap;
                source.Columns[0].Width = new GridLength(90);
                Layout(window);
                cell = Assert.IsType<TreeDataGridDrawnTextCell>(grid.TryGetCell(0, 0));
                Assert.True(cell.Bounds.Height > singleLine);
                grid.FlowDirection = FlowDirection.RightToLeft;
                Layout(window);
                Assert.Equal(model.Name, cell.Value);
                Assert.True(CountColor(CaptureCell(window, cell, dark ? 1.5 : 1.0), true) > 5);
            }
            finally
            {
                window.Close();
            }
        }

        #endregion

        #region Private

        private static byte[] CaptureCell(Window window, Control cell, double scale)
        {
            using var frame = window.CaptureRenderedFrame();
            Assert.NotNull(frame);
            var origin = cell.TranslatePoint(default, window)!.Value;
            var opposite = cell.TranslatePoint(new Point(cell.Bounds.Width, cell.Bounds.Height), window)!.Value;
            var left     = Math.Max(0, (int)Math.Ceiling(Math.Min(origin.X, opposite.X) * scale));
            var top      = Math.Max(0, (int)Math.Ceiling(Math.Min(origin.Y, opposite.Y) * scale));
            var right    = Math.Min(frame.PixelSize.Width, (int)Math.Floor(Math.Max(origin.X, opposite.X) * scale) - 2);
            var bottom   = Math.Min(frame.PixelSize.Height, (int)Math.Floor(Math.Max(origin.Y, opposite.Y) * scale) - 2);
            Assert.True(right > left && bottom > top, $"Cell {cell.Bounds} at {origin} is outside frame {frame.PixelSize}.");
            var rect = new PixelRect(left, top, right - left, bottom - top);
            var pixels = new byte[rect.Width * rect.Height * 4];
            var pointer = Marshal.AllocHGlobal(pixels.Length);

            try
            {
                frame.CopyPixels(rect, pointer, pixels.Length, rect.Width * 4);
                Marshal.Copy(pointer, pixels, 0, pixels.Length);

                return pixels;
            }
            finally
            {
                Marshal.FreeHGlobal(pointer);
            }
        }

        private static int CountColor(byte[] pixels, bool red)
        {
            var count = 0;

            for (var i = 0; i < pixels.Length; i += 4)
            {
                if (red ? pixels[i + 2] > 150 && pixels[i + 1] < 90 && pixels[i] < 90 :
                    pixels[i] > 150 && pixels[i + 1] < 90 && pixels[i + 2] < 90)
                {
                    ++count;
                }
            }

            return count;
        }

        private static void Layout(Window window)
        {
            for (var i = 0; i < 3; ++i)
            {
                window.UpdateLayout();
                Dispatcher.UIThread.RunJobs();
                AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            }
        }

        #endregion

        #region Nested Types

        private sealed class Model : INotifyPropertyChanged
        {
            private string _name = "Hardware sensor";

            public string Name
            {
                get => _name;
                set
                {
                    _name = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
                }
            }

            public event PropertyChangedEventHandler? PropertyChanged;
        }

        #endregion
    }
}
