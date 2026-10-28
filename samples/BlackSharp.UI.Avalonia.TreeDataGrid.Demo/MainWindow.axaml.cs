using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Avalonia.VisualTree;
using BlackSharp.UI.Avalonia.TreeDataGrid.Demo.Models;
using BlackSharp.UI.Avalonia.TreeDataGrid.Demo.ViewModels;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Demo
{
    public partial class MainWindow : Window
    {
        #region Constructor

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();

            _tabs = this.FindControl<TabControl>("tabs");

            _realizedCountTimer = DispatcherTimer.Run(() =>
            {
                UpdateRealizedCount();

                return true;
            }, TimeSpan.FromMilliseconds(500));

            Activated += MainWindow_Activated;
        }

        #endregion

        #region Fields

        private readonly TabControl? _tabs;

        private readonly IDisposable _realizedCountTimer;

        #endregion

        #region Public

        public void AddCountryClick(object sender, RoutedEventArgs e)
        {
            var countries         = this.FindControl<TreeDataGrid>("countries");
            var countryTextBox    = this.FindControl<TextBox>("countryTextBox");
            var regionTextBox     = this.FindControl<TextBox>("regionTextBox");
            var populationTextBox = this.FindControl<TextBox>("populationTextBox");
            var areaTextBox       = this.FindControl<TextBox>("areaTextBox");
            var gdpTextBox        = this.FindControl<TextBox>("gdpTextBox");

            if (countries is null
                || countryTextBox?.Text is null
                || regionTextBox?.Text is null
                || populationTextBox?.Text is null
                || areaTextBox?.Text is null
                || gdpTextBox?.Text is null)
            {
                return;
            }

            var country = new Country(
                countryTextBox.Text,
                regionTextBox.Text,
                int.TryParse(populationTextBox.Text, out var population) ? population : 0,
                int.TryParse(areaTextBox.Text, out var area) ? area : 0,
                0,
                0,
                null,
                null,
                int.TryParse(gdpTextBox.Text, out var gdp) ? gdp : 0,
                null,
                null,
                null,
                null);
            var vm = (MainWindowViewModel)DataContext!;

            vm.Countries.AddCountry(country);

            var index = vm.Countries.Source.Rows.Count - 1;

            countries.RowsPresenter!.BringIntoView(index);
            countries.TryGetRow(index)?.Focus();
        }

        #endregion

        #region Protected

        protected override void OnClosed(EventArgs e)
        {
            _realizedCountTimer.Dispose();
            (DataContext as IDisposable)?.Dispose();
            base.OnClosed(e);
        }

        #endregion

        #region Private

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void Features_ColumnReordered(object? sender, TreeDataGridColumnReorderedEventArgs e)
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.Features.Status = $"Moved '{e.Column.Header}' from {e.OldIndex} to {e.NewIndex}.";
            }
        }

        private void MainWindow_Activated(object? sender, EventArgs e)
        {
            if (Program.Stopwatch is { } stopwatch)
            {
                stopwatch.Stop();
                System.Diagnostics.Debug.WriteLine("Startup time: " + stopwatch.Elapsed);
            }
        }

        private void SelectedPath_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var vm = (MainWindowViewModel)DataContext!;

                vm.Files.SelectedPath = ((TextBox)sender!).Text;
            }
        }

        private void DragDrop_RowDragStarted(object? sender, TreeDataGridRowDragStartedEventArgs e)
        {
            foreach (DragDropItem i in e.Models)
            {
                if (!i.AllowDrag)
                {
                    e.AllowedEffects = DragDropEffects.None;
                }
            }
        }

        private void DragDrop_RowDragOver(object? sender, TreeDataGridRowDragEventArgs e)
        {
            if (e.Position == TreeDataGridRowDropPosition.Inside &&
                e.TargetRow?.Model is DragDropItem i &&
                !i.AllowDrop)
            {
                e.Inner.DragEffects = DragDropEffects.None;
            }
        }

        private void UpdateRealizedCount()
        {
            var tabItem = (TabItem?)_tabs?.SelectedItem;

            if (tabItem is null)
            {
                return;
            }

            if (tabItem.Content is not Control content)
            {
                return;
            }

            var descendants  = content.GetLogicalDescendants().ToArray();
            var treeDataGrid = descendants.OfType<TreeDataGrid>().FirstOrDefault();
            var textBlock    = descendants.OfType<TextBlock>().FirstOrDefault(x => x.Classes.Contains("realized-count"));

            if (treeDataGrid?.RowsPresenter is not { } rows || textBlock is null)
            {
                return;
            }

            var realizedRowCount   = rows.GetRealizedElements().Count();
            var unrealizedRowCount = rows.GetVisualChildren().Count() - realizedRowCount;

            textBlock.Text = $"{realizedRowCount} rows realized ({unrealizedRowCount} unrealized)";
        }

        #endregion
    }
}
