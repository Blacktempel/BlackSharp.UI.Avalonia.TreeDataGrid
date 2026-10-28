using System.Collections;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Models
{
    /// <summary>
    /// Provides a base class for read-only lists that implement both generic and non-generic list interfaces.
    /// </summary>
    /// <typeparam name="T">The type of elements contained in the list.</typeparam>
    public abstract class ReadOnlyListBase<T> : IReadOnlyList<T>, IList
    {
        #region Properties

        /// <inheritdoc />
        public abstract T this[int index] { get; }

        /// <inheritdoc />
        public abstract int Count { get; }

        #endregion

        #region Public

        /// <inheritdoc />
        public abstract IEnumerator<T> GetEnumerator();

        #endregion

        #region IList

        object? IList.this[int index]
        {
            get => this[index];
            set => throw new NotSupportedException();
        }

        bool IList.IsFixedSize => false;

        bool IList.IsReadOnly  => true;

        #endregion

        #region ICollection

        bool ICollection.IsSynchronized => false;

        object ICollection.SyncRoot     => this;

        #endregion

        #region IList

        int IList.Add(object? value) => throw new NotSupportedException();

        void IList.Clear() => throw new NotSupportedException();

        void IList.Insert(int index, object? value) => throw new NotSupportedException();

        void IList.Remove(object? value) => throw new NotSupportedException();

        void IList.RemoveAt(int index) => throw new NotSupportedException();

        bool IList.Contains(object? value) => ((IList)this).IndexOf(value) != -1;

        #endregion

        #region IEnumerable

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion

        #region ICollection

        void ICollection.CopyTo(Array array, int index)
        {
            for (var i = 0; i < Count; ++i)
            {
                array.SetValue(this[i], i + index);
            }
        }

        #endregion

        #region IList

        int IList.IndexOf(object? value)
        {
            for (var i = 0; i < Count; ++i)
            {
                if (Equals(this[i], value))
                {
                    return i;
                }
            }

            return -1;
        }

        #endregion
    }
}
