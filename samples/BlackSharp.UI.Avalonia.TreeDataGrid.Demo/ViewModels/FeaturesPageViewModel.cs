using Avalonia.Controls;
using Avalonia.Media;
using BlackSharp.UI.Avalonia.TreeDataGrid.Demo.Models;
using BlackSharp.UI.Avalonia.TreeDataGrid.Models;
using BlackSharp.UI.Avalonia.TreeDataGrid.Selection;
using System.Collections.ObjectModel;
using System.Globalization;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Demo.ViewModels
{
    internal class FeaturesPageViewModel : NotifyingBase, IDisposable
    {
        #region Constructor

        public FeaturesPageViewModel()
        {
            Items  = CreateItems();
            Source = new HierarchicalTreeDataGridSource<ShowcaseRow>(Items)
                .WithRowHeaderColumn("#")
                .WithHierarchicalExpanderTextColumn("Name", x => x.Name, x => x.Children, x => x.Expanded,
                    options: x =>
                    {
                        x.Width             = new GridLength(240);
                        x.IsReadOnly        = false;
                        x.UseDrawnCells     = true;
                        x.BeginEditGestures |= BeginEditGestures.TextInput;
                    })
                .WithCheckBoxColumn("Enabled", x => x.Enabled)
                .WithThreeStateCheckBoxColumn("Checked", x => x.Checked)
                .WithTextColumn("Temperature", x => x.Temperature, x =>
                {
                    x.Width         = new GridLength(160);
                    x.IsReadOnly    = false;
                    x.StringFormat  = "{0:N1} °C";
                    x.Culture       = CultureInfo.GetCultureInfo("de-DE");
                    x.TextAlignment = TextAlignment.Right;
                })
                .WithTemplateColumnFromResourceKeys("Category", "FeatureCategoryCell", "FeatureCategoryEdit", x => x.Width = new GridLength(180));

            for (var i = 0; i < 8; ++i)
            {
                Source.WithTextColumn($"Value {i + 1}", x => x.Name, x =>
                {
                    x.Width         = new GridLength(140);
                    x.UseDrawnCells = true;
                });
            }

            Source.AllowTriStateSorting  = true;
            Source.CanUserReorderColumns = true;
            Source.FrozenColumnCount     = 2;
            Source.RowSelection!.SingleSelect = false;
            Source.Selection!.SelectionChanged += OnSelectionChanged;
        }

        #endregion

        #region Fields

        private string _filter        = string.Empty;

        private bool   _drawnCells    = true;

        private bool   _cellSelection;

        private string _status        = "F2 or typing edits a cell. Tab commits and moves to the next editor. Drag a header to reorder.";

        #endregion

        #region Properties

        public ObservableCollection<ShowcaseRow> Items { get; }

        public HierarchicalTreeDataGridSource<ShowcaseRow> Source { get; }

        public string Filter
        {
            get => _filter;
            set
            {
                if (RaiseAndSetIfChanged(ref _filter, value))
                {
                    Source.Filter(string.IsNullOrWhiteSpace(value) ? null : Matches);
                }
            }
        }

        public bool DrawnCells
        {
            get => _drawnCells;
            set
            {
                if (RaiseAndSetIfChanged(ref _drawnCells, value))
                {
                    foreach (var column in Source.Columns)
                    {
                        var text = column as TreeDataGridTextColumn ?? (column as TreeDataGridHierarchicalExpanderColumn)?.Inner as TreeDataGridTextColumn;

                        if (text is not null)
                        {
                            text.UseDrawnCells = value;
                        }
                    }
                }
            }
        }

        public bool CellSelection
        {
            get => _cellSelection;
            set
            {
                if (RaiseAndSetIfChanged(ref _cellSelection, value))
                {
                    Source.Selection!.SelectionChanged -= OnSelectionChanged;
                    Source.Selection = value ? new TreeDataGridCellSelectionModel<ShowcaseRow>(Source) { SingleSelect = false } :
                        new TreeDataGridRowSelectionModel<ShowcaseRow>(Source) { SingleSelect = false };
                    Source.Selection.SelectionChanged += OnSelectionChanged;
                }
            }
        }

        public decimal FrozenColumns
        {
            get => Source.FrozenColumnCount;
            set
            {
                Source.FrozenColumnCount = (int)value;
                RaisePropertyChanged();
            }
        }

        public string Status
        {
            get => _status;
            set => RaiseAndSetIfChanged(ref _status, value);
        }

        #endregion

        #region Public

        public void ExpandAll() => Source.ExpandAll();

        public void CollapseAll() => Source.CollapseAll();

        public void ClearSort()
        {
            if (Source.Columns.FirstOrDefault(x => x.SortDirection is not null) is { } column)
            {
                Source.ClearSort(column);
            }
        }

        public void SelectAll()
        {
            Source.RowSelection?.SelectAll();
            Source.CellSelection?.SelectAll();
        }

        public void RefreshFilter() => Source.RefreshFilter();

        public void ChangeOffscreenExpansion()
        {
            Items[^1].Expanded = !Items[^1].Expanded;
            Status = $"Last group's model changed: Expanded = {Items[^1].Expanded}. Scroll to the last group to inspect it.";
        }

        public void Dispose() => Source.Dispose();

        #endregion

        #region Private

        private bool Matches(ShowcaseRow row) =>
            row.Name.Contains(Filter, StringComparison.OrdinalIgnoreCase) || row.Children.Any(Matches);

        private void OnSelectionChanged(object? sender, TreeDataGridSelectionChangedEventArgs e) =>
            Status = $"Selected: {(Source.Selection?.SelectedRowItem as ShowcaseRow)?.Name ?? "none"}. Added rows: {e.SelectedIndexes.Count}; added cells: {e.SelectedCellIndexes.Count}.";

        private static ObservableCollection<ShowcaseRow> CreateItems()
        {
            var items = new ObservableCollection<ShowcaseRow>();

            for (var i = 0; i < 100; ++i)
            {
                var group = new ShowcaseRow { Name = $"Group {i:000}", Category = "Controller", Expanded = i == 0 };

                for (var j = 0; j < 10; ++j)
                {
                    group.Children.Add(new ShowcaseRow
                    {
                        Name        = $"Sensor {i:000}.{j:00}",
                        Temperature = 20 + ((i * 13 + j * 7) % 650) / 10.0,
                        Enabled     = j % 3 != 0,
                        Checked     = j % 3 == 0 ? null : j % 2 == 0,
                        Category    = ShowcaseRow.Categories[j % ShowcaseRow.Categories.Length],
                    });
                }

                items.Add(group);
            }

            return items;
        }

        #endregion
    }
}
