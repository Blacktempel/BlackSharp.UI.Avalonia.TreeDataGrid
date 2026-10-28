using System.ComponentModel;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Models
{
    internal sealed class TreeDataGridRowHeaderCellModel : TextCell<object?>
    {
        #region Constructor

        internal TreeDataGridRowHeaderCellModel(IRow row) : base(row.Header)
        {
            // Flat sources use a shared temporary row. Only persistent notifying rows can be retained.
            if (row is INotifyPropertyChanged notifications)
            {
                _row = row;
                notifications.PropertyChanged += OnRowPropertyChanged;
            }
        }

        #endregion

        #region Fields

        private readonly IRow? _row;

        #endregion

        #region Public

        public override void Dispose()
        {
            if (_row is INotifyPropertyChanged notifications)
            {
                notifications.PropertyChanged -= OnRowPropertyChanged;
            }

            base.Dispose();
        }

        #endregion

        #region Private

        private void OnRowPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IRow.Header))
            {
                Value = _row!.Header;
            }
        }

        #endregion
    }
}
