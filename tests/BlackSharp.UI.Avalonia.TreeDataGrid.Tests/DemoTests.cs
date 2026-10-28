using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using Avalonia.Threading;
using Avalonia.VisualTree;
using BlackSharp.UI.Avalonia.TreeDataGrid.Demo;
using BlackSharp.UI.Avalonia.TreeDataGrid.Demo.Models;
using BlackSharp.UI.Avalonia.TreeDataGrid.Primitives;
using System.Runtime.CompilerServices;
using Xunit;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Tests
{
    public class DemoTests
    {
        #region Public

        [AvaloniaFact]
        public void Every_Demo_Tab_Creates_And_Lays_Out_Its_Grid()
        {
            var app    = Application.Current!;
            var fluent = new FluentTheme();
            var theme  = new StyleInclude(new Uri("avares://BlackSharp.UI.Avalonia.TreeDataGrid/"))
            {
                Source = new Uri("avares://BlackSharp.UI.Avalonia.TreeDataGrid/Themes/Fluent.axaml"),
            };

            app.Styles.Add(fluent);
            app.Styles.Add(theme);
            MainWindow? window = null;

            try
            {
                window = new MainWindow
                {
                    RequestedThemeVariant = ThemeVariant.Light,
                    Width                 = 1200,
                    Height                = 800,
                };
                window.Show();
                var tabs = window.FindControl<TabControl>("tabs")!;

                foreach (var tab in tabs.Items.Cast<TabItem>())
                {
                    tabs.SelectedItem = tab;
                    window.UpdateLayout();
                    Dispatcher.UIThread.RunJobs();
                    var grid = ((Control)tab.Content!).GetVisualDescendants().OfType<TreeDataGrid>().FirstOrDefault();

                    Assert.NotNull(grid);
                    Assert.NotNull(grid.Source);
                    Assert.NotNull(grid.RowsPresenter);
                    Assert.NotEmpty(grid.Columns);

                    if (grid.Name == "dragDrop")
                    {
                        var toggle = window.FindControl<CheckBox>("customDragPreview")!;
                        Assert.Null(grid.DragPreviewTemplate);
                        toggle.IsChecked = true;
                        window.UpdateLayout();
                        Assert.NotNull(grid.DragPreviewTemplate);
                        var model = Assert.IsType<DragDropItem>(grid.Rows![0].Model);
                        var control = grid.DragPreviewTemplate.Build(model);
                        Assert.NotNull(control);
                        toggle.IsChecked = false;
                        window.UpdateLayout();
                        Assert.Null(grid.DragPreviewTemplate);
                    }

                    if (grid.Name == "xamlFeatures")
                    {
                        var model = Assert.IsType<ShowcaseRow>(grid.Rows![0].Model);
                        var nameCell = Assert.IsAssignableFrom<TreeDataGridCell>(grid.TryGetCell(1, 0));

                        Assert.Equal(110, grid.Rows.Count);
                        Assert.Equal(model.Name, nameCell.Model!.Value);
                        Assert.Equal(model.Enabled, Assert.IsType<TreeDataGridCheckBoxCell>(grid.TryGetCell(2, 0)).Value);
                    }
                }
            }
            finally
            {
                window?.Close();
                (window?.DataContext as IDisposable)?.Dispose();
                app.Styles.Remove(theme);
                app.Styles.Remove(fluent);
            }
        }

        [AvaloniaFact]
        public void Disposed_File_Model_And_Its_Watcher_Can_Be_Collected()
        {
            var directory = Path.Combine(Path.GetTempPath(), "BlackSharp.UI.Avalonia.TreeDataGrid.DemoTest-" + Guid.NewGuid().ToString("N"));
            var file      = Path.Combine(directory, "sample.txt");

            Directory.CreateDirectory(directory);

            try
            {
                File.WriteAllText(file, "sample");
                var reference = CreateDisposedFileReference(directory);

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                Assert.False(reference.IsAlive);
            }
            finally
            {
                File.Delete(file);
                Directory.Delete(directory);
            }
        }

        [AvaloniaFact]
        public void Demo_Feature_Commands_And_Dynamic_Shrinking_Work()
        {
            using var features = new BlackSharp.UI.Avalonia.TreeDataGrid.Demo.ViewModels.FeaturesPageViewModel();
            features.ExpandAll();
            Assert.Equal(1100, features.Source.Rows.Count);
            features.CellSelection = true;
            features.SelectAll();
            Assert.Equal(features.Source.Rows.Count * features.Source.Columns.Count, features.Source.CellSelection!.Count);
            features.Filter = "Group 001";
            Assert.True(features.Source.IsFiltered);
            features.DrawnCells    = false;
            features.DrawnCells    = true;
            features.FrozenColumns = 1;
            Assert.Equal(1, features.Source.FrozenColumnCount);
            features.CollapseAll();
            features.Filter = string.Empty;
            Assert.Equal(100, features.Source.Rows.Count);
            var dynamic = new BlackSharp.UI.Avalonia.TreeDataGrid.Demo.ViewModels.DynamicColumnsViewModel();
            using (dynamic.Source)
            {
                dynamic.Source.SortBy(dynamic.Source.Columns[4], System.ComponentModel.ListSortDirection.Ascending);
                dynamic.ColumnCount = 2;
                Assert.Equal(2, dynamic.Source.Columns.Count);
                Assert.False(dynamic.Source.IsSorted);
                dynamic.ColumnCount = 6;
                Assert.Equal(6, dynamic.Source.Columns.Count);
            }
        }

        #endregion

        #region Private

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference CreateDisposedFileReference(string directory)
        {
            var model = new FileTreeNodeModel(directory, true);

            Assert.Single(model.Children);
            model.Dispose();

            return new WeakReference(model);
        }

        #endregion
    }
}
