using Avalonia.Controls;
using BlackSharp.UI.Avalonia.TreeDataGrid.Demo.Models;
using BlackSharp.UI.Avalonia.TreeDataGrid.Models;
using System.Collections.ObjectModel;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Demo.ViewModels
{
    internal class DragDropPageViewModel : NotifyingBase
    {
        #region Constructor

        public DragDropPageViewModel()
        {
            _data      = DragDropItem.CreateRandomItems();
            var source = new HierarchicalTreeDataGridSource<DragDropItem>(_data)
                .WithHierarchicalExpanderTextColumn("Name", x => x.Name, x => x.Children, options: x => x.Width = GridLength.Star)
                .WithCheckBoxColumn("Allow Drag", x => x.AllowDrag)
                .WithCheckBoxColumn("Allow Drop", x => x.AllowDrop);

            source.AllowTriStateSorting = true;
            source.RowSelection!.SingleSelect = false;
            Source = source;
        }

        #endregion

        #region Fields

        private ObservableCollection<DragDropItem> _data;

        #endregion

        #region Properties

        public TreeDataGridSource<DragDropItem> Source { get; }

        #endregion
    }
}
