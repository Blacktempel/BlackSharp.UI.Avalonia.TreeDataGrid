using System.Collections;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Models
{
    internal class ListSpan : IList
    {
        #region Constructor

        public ListSpan(IList items, int index, int count)
        {
            _items = items;
            _index = index;
            _count = count;
        }

        #endregion

        #region Fields

        private readonly IList _items;

        private readonly int   _index;

        private readonly int   _count;

        #endregion

        #region Properties

        public object? this[int index]
        {
            get
            {
                if (index >= _count)
                {
                    throw new ArgumentOutOfRangeException();
                }

                return _items[_index + index];
            }
            set => throw new NotSupportedException();
        }

        #endregion

        #region Public

        public IEnumerator GetEnumerator()
        {
            for (var i = 0; i < _count; ++i)
            {
                yield return _items[_index + i];
            }
        }

        #endregion

        #region IList

        bool IList.IsFixedSize => true;

        bool IList.IsReadOnly  => true;

        #endregion

        #region ICollection

        int ICollection.Count           => _count;

        bool ICollection.IsSynchronized => false;

        object ICollection.SyncRoot     => this;

        #endregion

        #region IList

        int IList.Add(object? value) => throw new NotSupportedException();

        void IList.Clear() => throw new NotSupportedException();

        bool IList.Contains(object? value) => throw new NotSupportedException();

        #endregion

        #region ICollection

        void ICollection.CopyTo(Array array, int index) => throw new NotSupportedException();

        #endregion

        #region IList

        int IList.IndexOf(object? value) => throw new NotSupportedException();

        void IList.Insert(int index, object? value) => throw new NotSupportedException();

        void IList.Remove(object? value) => throw new NotSupportedException();

        void IList.RemoveAt(int index) => throw new NotSupportedException();

        #endregion
    }
}
