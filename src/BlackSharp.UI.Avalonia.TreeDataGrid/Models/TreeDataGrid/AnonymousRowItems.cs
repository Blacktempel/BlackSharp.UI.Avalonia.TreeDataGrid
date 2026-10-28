using System.Collections;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Models
{
    /// <summary>
    /// Exposes a list of <typeparamref name="TModel"/> as anonymous rows.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    internal class AnonymousRowItems<TModel> : IList
    {
        #region Constructor

        public AnonymousRowItems(IList items)
        {
            _items = items;
            _row   = new AnonymousRow<TModel>();
        }

        #endregion

        #region Fields

        private readonly IList                _items;

        private readonly AnonymousRow<TModel> _row;

        #endregion

        #region Properties

        public object? this[int index]
        {
            get => _row.Update(index, (TModel)_items[index]!);
            set => throw new NotSupportedException();
        }

        #endregion

        #region Public

        public IEnumerator GetEnumerator()
        {
            var count = _items.Count;

            for (var i = 0; i < count; ++i)
            {
                yield return this[i];
            }
        }

        #endregion

        #region IList

        bool IList.IsFixedSize => true;

        bool IList.IsReadOnly  => true;

        #endregion

        #region ICollection

        int ICollection.Count           => _items.Count;

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
