using BlackSharp.UI.Avalonia.TreeDataGrid.Models;
using System.Collections.ObjectModel;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Demo.Models
{
    public class DragDropItem : NotifyingBase
    {
        #region Constructor

        public DragDropItem(string name) => Name = name;

        #endregion

        #region Fields

        private static Random s_random = new Random(0);

        private ObservableCollection<DragDropItem>? _children;

        private bool                                _allowDrag = true;

        private bool                                _allowDrop = true;

        #endregion

        #region Properties

        public string Name { get; }

        public bool AllowDrag
        {
            get => _allowDrag;
            set => this.RaiseAndSetIfChanged(ref _allowDrag, value);
        }

        public bool AllowDrop
        {
            get => _allowDrop;
            set => this.RaiseAndSetIfChanged(ref _allowDrop, value);
        }

        public ObservableCollection<DragDropItem> Children => _children ??= CreateRandomItems();

        #endregion

        #region Public

        public static ObservableCollection<DragDropItem> CreateRandomItems()
        {
            var names = new Bogus.DataSets.Name();
            var count = s_random.Next(10);

            return new ObservableCollection<DragDropItem>(Enumerable.Range(0, count)
                .Select(x => new DragDropItem(names.FullName())));
        }

        #endregion
    }
}
