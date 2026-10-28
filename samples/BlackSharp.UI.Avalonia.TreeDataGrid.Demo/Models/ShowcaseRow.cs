using BlackSharp.UI.Avalonia.TreeDataGrid.Models;
using System.Collections.ObjectModel;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Demo.Models
{
    public class ShowcaseRow : NotifyingBase
    {
        #region Fields

        private string _name        = string.Empty;

        private string _category    = "Sensor";

        private double _temperature;

        private bool   _enabled     = true;

        private bool?  _checked;

        private bool   _expanded;

        #endregion

        #region Properties

        public static string[] Categories { get; } = ["Sensor", "Controller", "Fan", "Power"];

        public string Name
        {
            get => _name;
            set => RaiseAndSetIfChanged(ref _name, value);
        }

        public string Category
        {
            get => _category;
            set => RaiseAndSetIfChanged(ref _category, value);
        }

        public double Temperature
        {
            get => _temperature;
            set => RaiseAndSetIfChanged(ref _temperature, value);
        }

        public bool Enabled
        {
            get => _enabled;
            set => RaiseAndSetIfChanged(ref _enabled, value);
        }

        public bool? Checked
        {
            get => _checked;
            set => RaiseAndSetIfChanged(ref _checked, value);
        }

        public bool Expanded
        {
            get => _expanded;
            set => RaiseAndSetIfChanged(ref _expanded, value);
        }

        public ObservableCollection<ShowcaseRow> Children { get; } = [];

        #endregion
    }
}
