using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media.Imaging;
using Avalonia.Themes.Fluent;
using Avalonia.Threading;
using Avalonia.VisualTree;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using Xunit;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Tests
{
    public class RowDragPreviewTests
    {
        #region Public

        [AvaloniaTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void Drag_Shows_The_Initiating_Row_And_Cleans_Up_On_Completion(bool customTemplate)
        {
            var items = new ObservableCollection<Model> { new("First"), new("Second") };
            using var source = CreateSource(items);
            var grid = new DragGrid { Source = source, AutoDragDropRows = true, FontSize = 20 };
            var window = Show(grid);
            Model? templateModel = null;

            if (customTemplate)
            {
                grid.DragPreviewTemplate = new FuncDataTemplate<Model>((model, _) =>
                {
                    templateModel = model;
                    return new TextBlock { Text = "Preview: " + model!.Name };
                });
            }

            try
            {
                source.RowSelection!.SingleSelect = false;
                source.RowSelection.Select(0);
                source.RowSelection.Select(1);
                var release = StartDrag(window, grid, 1);
                Assert.Equal(1, grid.Attempts);
                var preview = Assert.Single(Previews(window));
                Assert.False(preview.IsHitTestVisible);
                Assert.Same(items[1], preview.DataContext);
                Assert.Equal(0.88, preview.Opacity);
                Assert.Equal(2, source.RowSelection.Count);

                if (customTemplate)
                {
                    Assert.Same(items[1], templateModel);
                    Assert.Equal("Preview: Second", Assert.Single(preview.GetVisualDescendants().OfType<TextBlock>()).Text);
                    Assert.Equal(grid.FontSize, Assert.Single(preview.GetVisualDescendants().OfType<TextBlock>()).FontSize);
                }
                else
                {
                    Assert.IsType<RenderTargetBitmap>(Assert.IsType<Image>(preview.Child).Source);
                }

                grid.Complete(DragDropEffects.Move);
                Layout(window);
                Assert.Empty(Previews(window));
                Assert.Null(preview.Child);
                Assert.Null(preview.DataContext);
                window.MouseUp(release, MouseButton.Left);
            }
            finally
            {
                grid.Complete(DragDropEffects.None);
                window.Close();
            }
        }

        [AvaloniaTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void Opting_Out_Or_Cancelling_Start_Does_Not_Show_A_Preview(bool cancelStart)
        {
            using var source = CreateSource(new[] { new Model("First") });
            var grid = new DragGrid { Source = source, AutoDragDropRows = true, ShowDragPreview = false };

            if (cancelStart)
            {
                grid.ShowDragPreview = true;
                grid.RowDragStarted += (s, e) => e.AllowedEffects = DragDropEffects.None;
            }

            var window = Show(grid);

            try
            {
                var release = StartDrag(window, grid, 0);
                Assert.Equal(cancelStart ? 0 : 1, grid.Attempts);
                Assert.Empty(Previews(window));
                grid.Complete(DragDropEffects.None);
                window.MouseUp(release, MouseButton.Left);
            }
            finally
            {
                grid.Complete(DragDropEffects.None);
                window.Close();
            }
        }

        [AvaloniaTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void Failed_And_Cancelled_Platform_Drags_Remove_The_Preview(bool cancel)
        {
            using var source = CreateSource(new[] { new Model("First") });
            var grid = new DragGrid { Source = source, AutoDragDropRows = true };
            var window = Show(grid);

            try
            {
                var release = StartDrag(window, grid, 0);
                Assert.Single(Previews(window));
                grid.Fail(cancel);
                Layout(window);
                Assert.Empty(Previews(window));
                window.MouseUp(release, MouseButton.Left);
                StartDrag(window, grid, 0);
                Assert.Equal(2, grid.Attempts);
            }
            finally
            {
                grid.Complete(DragDropEffects.None);
                window.Close();
            }
        }

        [AvaloniaFact]
        public void Preview_Follows_DragOver_Hides_On_Leave_And_Does_Not_Block_The_Drop()
        {
            var items = new ObservableCollection<Model> { new("First"), new("Second"), new("Third") };
            using var source = CreateSource(items);
            var grid = new DragGrid { Source = source, AutoDragDropRows = true };
            var window = Show(grid);

            try
            {
                var first = items[0];
                var release = StartDrag(window, grid, 0);
                var preview = Assert.Single(Previews(window));
                var before = new Point(Canvas.GetLeft(preview), Canvas.GetTop(preview));
                var row = grid.TryGetRow(1)!;
                row.RaiseEvent(new DragEventArgs(DragDrop.DragOverEvent, grid.Transfer!, row,
                    new Point(180, row.Bounds.Height - 1), KeyModifiers.None)
                {
                    DragEffects = DragDropEffects.Move,
                });
                Layout(window);
                Assert.NotEqual(before, new Point(Canvas.GetLeft(preview), Canvas.GetTop(preview)));
                window.RaiseEvent(new DragEventArgs(DragDrop.DragLeaveEvent, grid.Transfer!, window, default, KeyModifiers.None));
                Layout(window);
                Assert.False(preview.IsVisible);
                row.RaiseEvent(new DragEventArgs(DragDrop.DragOverEvent, grid.Transfer!, row,
                    new Point(180, row.Bounds.Height - 1), KeyModifiers.None)
                {
                    DragEffects = DragDropEffects.Move,
                });
                Assert.True(preview.IsVisible);
                row.RaiseEvent(new DragEventArgs(DragDrop.DropEvent, grid.Transfer!, row,
                    new Point(180, row.Bounds.Height - 1), KeyModifiers.None)
                {
                    DragEffects = DragDropEffects.Move,
                });
                grid.Complete(DragDropEffects.Move);
                Layout(window);
                Assert.Same(first, items[1]);
                Assert.Empty(Previews(window));
                window.MouseUp(release, MouseButton.Left);
            }
            finally
            {
                grid.Complete(DragDropEffects.None);
                window.Close();
            }
        }

        [AvaloniaTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void Preview_Moves_Horizontally_And_Stays_Visible_Across_Row_And_Drop_Target_Boundaries(bool customTemplate)
        {
            using var source = CreateSource(new[] { new Model("First"), new Model("Second"), new Model("Third") });
            var grid = new DragGrid { Source = source, AutoDragDropRows = true };

            if (customTemplate)
            {
                grid.DragPreviewTemplate = new FuncDataTemplate<Model>((model, _) =>
                    new TextBlock { Text = model!.Name, Width = 180, Height = 32 });
            }

            var window = Show(grid);

            try
            {
                var release = StartDrag(window, grid, 0);
                var preview = Assert.Single(Previews(window));
                SendDrag(DragDrop.DragEnterEvent, window, grid, new Point(70, 80));
                Layout(window);
                var before = Canvas.GetLeft(preview);
                SendDrag(DragDrop.DragOverEvent, window, grid, new Point(190, 100));
                Layout(window);
                Assert.Equal(120, Canvas.GetLeft(preview) - before, 5);
                Assert.True(preview.IsVisible);

                var row = grid.TryGetRow(1)!;
                DragDrop.SetAllowDrop(grid.TryGetCell(0, 1)!, true);

                for (var pixel = -2; pixel <= 2; ++pixel)
                {
                    var point = new Point(190 + pixel, row.Bounds.Height + pixel);
                    row.RaiseEvent(new DragEventArgs(DragDrop.DragLeaveEvent, grid.Transfer!, row, point, KeyModifiers.None));
                    Assert.True(preview.IsVisible);
                    var position = row.TranslatePoint(point, window)!.Value;
                    Assert.Contains(grid, ((Visual)window.InputHitTest(position)!).GetSelfAndVisualAncestors());
                    SendDrag(DragDrop.DragOverEvent, window, grid, position);
                    Layout(window);
                    Assert.True(preview.IsVisible);
                    Assert.Equal(position.X + 12, Canvas.GetLeft(preview), 5);
                }

                // A window leave may carry the last position inside it. Without a following Enter it must hide.
                SendDrag(DragDrop.DragLeaveEvent, window, grid, new Point(190, 100));
                Layout(window);
                Assert.False(preview.IsVisible);
                SendDrag(DragDrop.DragEnterEvent, window, grid, new Point(150, 90));
                Assert.True(preview.IsVisible);
                Assert.Equal(162, Canvas.GetLeft(preview), 5);
                SendDrag(DragDrop.DragLeaveEvent, window, grid, new Point(window.Bounds.Width + 1, 90));
                Layout(window);
                Assert.False(preview.IsVisible);
                SendDrag(DragDrop.DragEnterEvent, window, grid, new Point(150, 90));
                Assert.True(preview.IsVisible);
                grid.Complete(DragDropEffects.None);
                Layout(window);
                Assert.Empty(Previews(window));
                window.MouseUp(release, MouseButton.Left);
            }
            finally
            {
                SendDrag(DragDrop.DragLeaveEvent, window, grid, default);
                grid.Complete(DragDropEffects.None);
                window.Close();
            }
        }

        [AvaloniaFact]
        public void Clearing_The_Source_In_RowDragStarted_Aborts_The_Drag()
        {
            using var source = CreateSource(new[] { new Model("First") });
            var grid = new DragGrid { Source = source, AutoDragDropRows = true };
            grid.RowDragStarted += (s, e) => grid.Source = null;
            var window = Show(grid);

            try
            {
                var release = StartDrag(window, grid, 0);
                Assert.Equal(0, grid.Attempts);
                Assert.Empty(Previews(window));
                window.MouseUp(release, MouseButton.Left);
            }
            finally
            {
                grid.Complete(DragDropEffects.None);
                window.Close();
            }
        }

        [AvaloniaFact]
        public void Detaching_The_Grid_Removes_The_Preview_Before_The_Platform_Drag_Completes()
        {
            using var source = CreateSource(new[] { new Model("First") });
            var grid = new DragGrid { Source = source, AutoDragDropRows = true };
            var window = Show(grid);

            try
            {
                StartDrag(window, grid, 0);
                Assert.Single(Previews(window));
                window.Content = null;
                Layout(window);
                Assert.Empty(Previews(window));
            }
            finally
            {
                grid.Complete(DragDropEffects.None);
                window.Close();
            }
        }

        [AvaloniaFact]
        public void A_Broken_Preview_Template_Does_Not_Prevent_The_Drag()
        {
            using var source = CreateSource(new[] { new Model("First") });
            var grid = new DragGrid
            {
                Source = source,
                AutoDragDropRows = true,
                DragPreviewTemplate = new FuncDataTemplate<Model>((model, _) => throw new InvalidOperationException("Broken template")),
            };
            var window = Show(grid);

            try
            {
                StartDrag(window, grid, 0);
                Assert.Equal(1, grid.Attempts);
                Assert.Empty(Previews(window));
            }
            finally
            {
                grid.Complete(DragDropEffects.None);
                window.Close();
            }
        }

        [AvaloniaFact]
        public void Completed_Drag_Does_Not_Retain_Preview_Controls_While_The_Window_Stays_Open()
        {
            using var source = CreateSource(new[] { new Model("First") });
            var grid = new DragGrid { Source = source, AutoDragDropRows = true };
            var window = Show(grid);

            try
            {
                var reference = StartAndFinishDrag(window, grid);
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                Assert.False(reference.IsAlive);
                GC.KeepAlive(window);
            }
            finally
            {
                grid.Complete(DragDropEffects.None);
                window.Close();
            }
        }

        #endregion

        #region Private

        private static void SendDrag(RoutedEvent<DragEventArgs> routedEvent, Window window, DragGrid grid, Point position)
        {
            var type = routedEvent == DragDrop.DragEnterEvent ? RawDragEventType.DragEnter :
                routedEvent == DragDrop.DragOverEvent ? RawDragEventType.DragOver : RawDragEventType.DragLeave;
            window.DragDrop(position, type, grid.Transfer!, DragDropEffects.Move, RawInputModifiers.None);
        }

        private static FlatTreeDataGridSource<Model> CreateSource(System.Collections.Generic.IEnumerable<Model> items) =>
            new FlatTreeDataGridSource<Model>(items)
                .WithTextColumn(x => x.Name, options => { options.IsReadOnly = true; options.Width = GridLength.Star; });

        private static Border[] Previews(Window window) => window.GetVisualDescendants().OfType<Border>()
            .Where(x => x.Classes.Contains("tree-data-grid-row-drag-preview")).ToArray();

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference StartAndFinishDrag(Window window, DragGrid grid)
        {
            var release = StartDrag(window, grid, 0);
            var reference = new WeakReference(Assert.Single(Previews(window)));
            window.RaiseEvent(new DragEventArgs(DragDrop.DragLeaveEvent, grid.Transfer!, window, default, KeyModifiers.None));
            grid.Complete(DragDropEffects.None);
            window.MouseUp(release, MouseButton.Left);
            Layout(window);

            return reference;
        }

        private static Point StartDrag(Window window, TreeDataGrid grid, int index)
        {
            var cell = grid.TryGetCell(0, index)!;
            var start = cell.TranslatePoint(new Point(30, cell.Bounds.Height / 2), window)!.Value;
            var end = start + new Vector(18, 0);
            window.MouseDown(start, MouseButton.Left);
            window.MouseMove(end, RawInputModifiers.LeftMouseButton);
            Layout(window);

            return end;
        }

        private static Window Show(TreeDataGrid grid)
        {
            var app = Application.Current!;
            var fluent = new FluentTheme();
            var theme = new StyleInclude(new Uri("avares://BlackSharp.UI.Avalonia.TreeDataGrid/"))
            {
                Source = new Uri("avares://BlackSharp.UI.Avalonia.TreeDataGrid/Themes/Fluent.axaml"),
            };
            app.Styles.Add(fluent);
            app.Styles.Add(theme);
            var window = new Window { Width = 480, Height = 300, Content = grid };
            window.Closed += (s, e) =>
            {
                app.Styles.Remove(theme);
                app.Styles.Remove(fluent);
            };
            window.Show();
            Layout(window);

            return window;
        }

        private static void Layout(Window window)
        {
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
        }

        #endregion

        #region Nested Types

        private sealed record Model(string Name);

        private sealed class DragGrid : TreeDataGrid
        {
            private TaskCompletionSource<DragDropEffects>? _completion;

            public int Attempts { get; private set; }

            public IDataTransfer? Transfer { get; private set; }

            protected override Type StyleKeyOverride => typeof(TreeDataGrid);

            public void Complete(DragDropEffects effect) => _completion?.TrySetResult(effect);

            public void Fail(bool cancel)
            {
                if (cancel)
                {
                    _completion?.TrySetCanceled();
                }
                else
                {
                    _completion?.TrySetException(new InvalidOperationException("Platform drag failure"));
                }
            }

            protected override Task<DragDropEffects> DoRowDragDropAsync(
                PointerPressedEventArgs trigger, IDataTransfer dataTransfer, DragDropEffects allowedEffects)
            {
                ++Attempts;
                Transfer = dataTransfer;
                _completion = new TaskCompletionSource<DragDropEffects>();

                return _completion.Task;
            }
        }

        #endregion
    }
}
