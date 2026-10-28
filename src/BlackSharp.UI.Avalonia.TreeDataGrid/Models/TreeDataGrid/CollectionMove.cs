namespace BlackSharp.UI.Avalonia.TreeDataGrid.Models
{
    internal static class CollectionMove
    {
        #region Public

        public static int MapIndex(int index, int oldIndex, int newIndex, int count)
        {
            if (index >= oldIndex && index < oldIndex + count)
            {
                return index + newIndex - oldIndex;
            }

            if (oldIndex < newIndex && index >= oldIndex + count && index < newIndex + count)
            {
                return index - count;
            }

            if (newIndex < oldIndex && index >= newIndex && index < oldIndex)
            {
                return index + count;
            }

            return index;
        }

        public static bool MapPath(IndexPath parent, int oldIndex, int newIndex, int count, ref IndexPath path)
        {
            if (!parent.IsAncestorOf(path))
            {
                return false;
            }

            var index  = path[parent.Count];
            var mapped = MapIndex(index, oldIndex, newIndex, count);

            if (mapped == index)
            {
                return false;
            }

            var indexes = path.ToArray();

            indexes[parent.Count] = mapped;
            path                  = new IndexPath(indexes);

            return true;
        }

        public static void MoveRange<T>(List<T> items, int oldIndex, int newIndex, int count)
        {
            if (oldIndex == newIndex || count == 0)
            {
                return;
            }

            var moved = items.GetRange(oldIndex, count);

            items.RemoveRange(oldIndex, count);
            items.InsertRange(newIndex, moved);
        }

        public static void ApplySortOrder(List<int> indexes, IReadOnlyList<int> sorted, Action<int, int, int> moved)
        {
            for (var i = 0; i < sorted.Count; ++i)
            {
                if (indexes[i] == sorted[i])
                {
                    continue;
                }

                var from   = indexes.IndexOf(sorted[i], i + 1);
                var length = 1;

                while (from + length < sorted.Count && indexes[from + length] == sorted[i + length])
                {
                    ++length;
                }

                MoveRange(indexes, from, i, length);
                moved(from, i, length);
            }
        }

        #endregion
    }
}
