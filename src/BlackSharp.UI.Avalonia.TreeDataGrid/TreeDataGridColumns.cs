using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace BlackSharp.UI.Avalonia.TreeDataGrid
{
    public sealed class TreeDataGridColumns : Collection<TreeDataGridColumn>,
        IReadOnlyList<TreeDataGridColumn>,
        INotifyCollectionChanged,
        INotifyPropertyChanged
    {
        #region Properties

        internal bool AllowExpanderTransition { get; set; }

        internal TreeDataGridSource? SourceOwner { get; set; }

        #endregion

        #region Public

        public void AddRange(IEnumerable<TreeDataGridColumn> items)
        {
            ArgumentNullException.ThrowIfNull(items);

            foreach (var item in items)
            {
                Add(item);
            }
        }

        public void Move(int oldIndex, int newIndex)
        {
            if ((uint)oldIndex >= Count || (uint)newIndex >= Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (oldIndex == newIndex)
            {
                return;
            }

            var item = this[oldIndex];

            Items.RemoveAt(oldIndex);
            Items.Insert(newIndex, item);
            OnChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, newIndex, oldIndex));
        }

        #endregion

        #region Protected

        protected override void ClearItems()
        {
            foreach (var column in this)
            {
                column.Owner = null;
            }

            base.ClearItems();
            OnChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        protected override void InsertItem(int index, TreeDataGridColumn item)
        {
            Validate(item);
            base.InsertItem(index, item);
            item.Owner = this;
            OnChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }

        protected override void RemoveItem(int index)
        {
            var item = this[index];

            base.RemoveItem(index);
            item.Owner = null;
            OnChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
        }

        protected override void SetItem(int index, TreeDataGridColumn item)
        {
            if (ReferenceEquals(item, this[index]))
            {
                return;
            }

            Validate(item, index);
            var old = this[index];

            base.SetItem(index, item);
            old.Owner  = null;
            item.Owner = this;
            OnChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, old, index));
        }

        #endregion

        #region Private

        private void Validate(TreeDataGridColumn item, int replacingIndex = -1)
        {
            ArgumentNullException.ThrowIfNull(item);

            if (item is TreeDataGridHierarchicalExpanderColumn)
            {
                if (SourceOwner is { IsHierarchical: false } && !AllowExpanderTransition)
                {
                    throw new ArgumentException("A hierarchical expander column requires a hierarchical source.", nameof(item));
                }

                for (var i = 0; i < Count; ++i)
                {
                    if (i != replacingIndex && this[i] is TreeDataGridHierarchicalExpanderColumn)
                    {
                        throw new InvalidOperationException("Only one hierarchical expander column is allowed.");
                    }
                }
            }

            if (item.Owner is not null)
            {
                throw new InvalidOperationException("A column can only belong to one collection and cannot be added twice.");
            }
        }

        private void OnChanged(NotifyCollectionChangedEventArgs args)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
            CollectionChanged?.Invoke(this, args);
        }

        #endregion

        #region Events

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion
    }
}
