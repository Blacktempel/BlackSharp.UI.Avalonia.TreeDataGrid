using System.Collections;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Selection
{
    internal class TreeSelectionChangedItems<T> : IReadOnlyList<T>
    {
        #region Constructor

        public TreeSelectionChangedItems(TreeSelectionModelBase<T> owner, IndexRanges ranges)
        {
            _owner  = owner;
            _ranges = ranges;
        }

        #endregion

        #region Fields

        private readonly TreeSelectionModelBase<T> _owner;

        private readonly IndexRanges               _ranges;

        #endregion

        #region Properties

        public T this[int index] => _owner.GetSelectedItemAt(_ranges[index]);

        public int Count => _ranges.Count;

        #endregion

        #region Public

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var index in _ranges)
            {
                yield return _owner.GetSelectedItemAt(index);
            }
        }

        public static TreeSelectionChangedItems<T>? Create(TreeSelectionModelBase<T> owner, IndexRanges? ranges)
        {
            return ranges is not null ? new TreeSelectionChangedItems<T>(owner, ranges) : null;
        }

        #endregion

        #region IEnumerable

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }
}
