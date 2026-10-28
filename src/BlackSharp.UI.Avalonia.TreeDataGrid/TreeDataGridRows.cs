using BlackSharp.UI.Avalonia.TreeDataGrid.Models;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;

namespace BlackSharp.UI.Avalonia.TreeDataGrid
{
    public abstract class TreeDataGridRows : IReadOnlyList<ITreeDataGridRowModel>,
        INotifyCollectionChanged,
        INotifyPropertyChanged
    {
        #region Properties

        public abstract ITreeDataGridRowModel this[int index] { get; }

        public abstract int Count { get; }

        #endregion

        #region Public

        public abstract IEnumerator<ITreeDataGridRowModel> GetEnumerator();

        #endregion

        #region Protected

        protected void RaiseCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
            CollectionChanged?.Invoke(this, args);
        }

        #endregion

        #region IEnumerable

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion

        #region Events

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion
    }
}
