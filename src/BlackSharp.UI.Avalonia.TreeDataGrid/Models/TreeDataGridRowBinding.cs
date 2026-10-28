using Avalonia.Reactive;
using BlackSharp.UI.Avalonia.TreeDataGrid.Experimental.Data;
using BlackSharp.UI.Avalonia.TreeDataGrid.Models;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;

namespace BlackSharp.UI.Avalonia.TreeDataGrid
{
    internal interface ITreeDataGridRowBinding<TModel>
    {
        #region Private

        IDisposable Subscribe(HierarchicalRow<TModel> row);

        #endregion
    }

    internal sealed class TreeDataGridRowBinding<TModel> : IDisposable
        where TModel : class
    {
        #region Constructor

        internal TreeDataGridRowBinding(TreeDataGridHierarchicalExpanderColumn column, HierarchicalRow<TModel> row)
        {
            _column              = column;
            _row                 = row;
            _row.PropertyChanged += OnRowPropertyChanged;

            if (column.HasChildrenBinding is null || row.IsExpanded)
            {
                SubscribeToChildren();
            }

            if (column.HasChildrenBinding is not null)
            {
                _hasChildren             = new TreeDataGridBindingValue<bool>(row.Model!, column.HasChildrenBinding);
                _hasChildrenSubscription = _hasChildren.Subscribe(x => row.SetShowExpander(x.Value));
            }

            if (column.IsExpandedBinding is not null)
            {
                _expanded             = new TreeDataGridBindingValue<bool>(row.Model!, column.IsExpandedBinding);
                _expandedSubscription = _expanded.Subscribe(x => row.IsExpanded = x.Value);
            }
        }

        #endregion

        #region Fields

        private readonly TreeDataGridHierarchicalExpanderColumn _column;

        private readonly HierarchicalRow<TModel>                _row;

        private TreeDataGridBindingValue<IEnumerable?>?         _children;

        private IDisposable?                                    _childrenSubscription;

        private bool                                            _receivedChildren;

        private readonly TreeDataGridBindingValue<bool>?        _hasChildren;

        private readonly IDisposable?                           _hasChildrenSubscription;

        private readonly TreeDataGridBindingValue<bool>?        _expanded;

        private readonly IDisposable?                           _expandedSubscription;

        private INotifyCollectionChanged?                       _childCollection;

        private IEnumerable?                                    _childModels;

        #endregion

        #region Public

        public void Dispose()
        {
            _row.PropertyChanged -= OnRowPropertyChanged;

            if (_childCollection is not null)
            {
                _childCollection.CollectionChanged -= OnChildrenChanged;
                _childCollection                   = null;
            }

            _expandedSubscription?.Dispose();
            _hasChildrenSubscription?.Dispose();
            _childrenSubscription?.Dispose();
            _expanded?.Dispose();
            _hasChildren?.Dispose();
            _children?.Dispose();

            _childModels = null;
        }

        #endregion

        #region Private

        private void SubscribeToChildren()
        {
            if (_children is null)
            {
                _children             = new TreeDataGridBindingValue<IEnumerable?>(_row.Model!, _column.ChildrenBinding);
                _childrenSubscription = _children.Subscribe(x => OnChildrenReplaced(x.Value));
            }
        }

        private void OnChildrenReplaced(IEnumerable? children)
        {
            if (ReferenceEquals(_childModels, children))
            {
                return;
            }

            if (_childCollection is not null)
            {
                _childCollection.CollectionChanged -= OnChildrenChanged;
            }

            var refresh = _receivedChildren;

            _receivedChildren = true;
            _childModels      = children;
            _childCollection  = children as INotifyCollectionChanged;

            if (_childCollection is not null)
            {
                _childCollection.CollectionChanged += OnChildrenChanged;
            }

            _row.SetShowExpander(HasChildren());
            if (refresh)
            {
                _row.RefreshChildren();
            }
        }

        private void OnChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e) =>
            _row.SetShowExpander(HasChildren());

        private bool HasChildren() => _column.HasChildrenBinding is not null ? _column.HasChildren(_row.Model!) :
            TreeDataGridHierarchicalExpanderColumn.HasAnyChildren(_childModels);

        private void OnRowPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IExpander.IsExpanded))
            {
                if (_row.IsExpanded)
                {
                    SubscribeToChildren();
                }

                if (_expanded is not null)
                {
                    _expanded.Value = _row.IsExpanded;
                }
            }
        }

        #endregion
    }
}
