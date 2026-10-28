using Avalonia.Controls;
using BlackSharp.UI.Avalonia.TreeDataGrid.Models;

namespace BlackSharp.UI.Avalonia.TreeDataGrid
{
    internal sealed class TreeDataGridRowModel : ITreeDataGridRowModel, IExpander
    {
        #region Constructor

        internal TreeDataGridRowModel(IRow row, IndexPath path)
        {
            _row           = row;
            Model          = row.Model;
            Header         = row.Header;
            ModelIndexPath = path;
        }

        #endregion

        #region Fields

        private readonly IRow _row;

        #endregion

        #region Properties

        public object? Header { get; }

        public GridLength Height
        {
            get => _row.Height;
            set => _row.Height = value;
        }

        public object? Model { get; }

        public int ModelIndex => ModelIndexPath.Count > 0 ? ModelIndexPath[^1] : -1;

        public IndexPath ModelIndexPath { get; }

        public bool IsExpanded
        {
            get => _row is IExpander { IsExpanded: true };
            set
            {
                if (_row is IExpander expander)
                {
                    expander.IsExpanded = value;
                }
            }
        }

        public bool ShowExpander => _row is IExpander { ShowExpander: true };

        internal IRow Inner => _row;

        #endregion
    }
}
