using Avalonia.Controls;
using Avalonia.Media;
using BlackSharp.UI.Avalonia.TreeDataGrid.Demo.Models;
using BlackSharp.UI.Avalonia.TreeDataGrid.Models;
using BlackSharp.UI.Avalonia.TreeDataGrid.Selection;
using System.Collections.ObjectModel;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Demo.ViewModels
{
    internal class CountriesPageViewModel : NotifyingBase
    {
        #region Constructor

        public CountriesPageViewModel()
        {
            _data = new ObservableCollection<Country>(Countries.All);

            Source = new FlatTreeDataGridSource<Country>(_data)
                .WithTextColumn("Country", x => x.Name, x =>
                {
                    x.IsReadOnly = false;
                    x.Width      = new GridLength(6, GridUnitType.Star);
                })
                .WithTemplateColumnFromResourceKeys("Region", "RegionCell", "RegionEditCell")
                .WithTextColumn("Population", x => x.Population, x => x.Width = new GridLength(3, GridUnitType.Star))
                .WithTextColumn("Area", x => x.Area, x => x.Width = new GridLength(3, GridUnitType.Star))
                .WithTextColumn("GDP", x => x.GDP, x =>
                {
                    x.Width         = new GridLength(3, GridUnitType.Star);
                    x.MaxWidth      = new GridLength(150);
                    x.TextAlignment = TextAlignment.Right;
                });
            Source.AllowTriStateSorting  = true;
            Source.CanUserReorderColumns = true;
            Source.RowSelection!.SingleSelect = false;
        }

        #endregion

        #region Fields

        private readonly ObservableCollection<Country> _data;

        private bool                                   _cellSelection;

        #endregion

        #region Properties

        public bool CellSelection
        {
            get => _cellSelection;
            set
            {
                if (_cellSelection != value)
                {
                    _cellSelection = value;

                    if (_cellSelection)
                    {
                        Source.Selection = new TreeDataGridCellSelectionModel<Country>(Source) { SingleSelect = false };
                    }
                    else
                    {
                        Source.Selection = new TreeDataGridRowSelectionModel<Country>(Source) { SingleSelect = false };
                    }

                    this.RaisePropertyChanged();
                }
            }
        }

        public FlatTreeDataGridSource<Country> Source { get; }

        #endregion

        #region Public

        public void AddCountry(Country country) => _data.Add(country);

        public void RemoveSelected()
        {
            var selection = Source.Selection switch
            {
                ITreeDataGridRowSelection rows => rows.SelectedIndexes.ToList(),
                ITreeDataGridCellSelection cells => cells.SelectedIndexes.Select(x => x.RowIndex).Distinct().ToList(),
                _ => [],
            };

            selection.Sort();

            for (var i = selection.Count - 1; i >= 0; --i)
            {
                _data.RemoveAt(selection[i][0]);
            }
        }

        #endregion
    }
}
