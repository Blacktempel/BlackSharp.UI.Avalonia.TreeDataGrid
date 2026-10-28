using Avalonia.Reactive;
using BlackSharp.UI.Avalonia.TreeDataGrid.Experimental.Data;
using BlackSharp.UI.Avalonia.TreeDataGrid.Experimental.Data.Core;
using System.ComponentModel;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Models
{
    internal class ExpanderCell<TModel> : NotifyingBase,
        IExpanderCell,
        IDisposable
        where TModel : class
    {
        #region Constructor

        public ExpanderCell(
            ICell inner,
            IExpanderRow<TModel> row,
            IObservable<bool> showExpander,
            TypedBindingExpression<TModel, bool>? isExpanded)
        {
            _inner              = inner;
            Row                 = row;
            row.PropertyChanged += RowPropertyChanged;

            var expanderSubscription = showExpander.Subscribe(x => Row.UpdateShowExpander(this, x));

            if (isExpanded is not null)
            {
                var isExpandedSubscription = isExpanded.Subscribe(x =>
                {
                    if (x.HasValue)
                    {
                        IsExpanded = x.Value;
                    }
                });

                _subscription = new CompositeDisposable(expanderSubscription, isExpandedSubscription);
            }
            else
            {
                _subscription = expanderSubscription;
            }
        }

        #endregion

        #region Fields

        private readonly ICell       _inner;

        private readonly IDisposable _subscription;

        #endregion

        #region Properties

        /// <inheritdoc/>
        public bool CanEdit => _inner.CanEdit;

        public ICell Content => _inner;

        /// <inheritdoc/>
        public BeginEditGestures EditGestures => _inner.EditGestures;

        public IExpanderRow<TModel> Row { get; }

        public bool ShowExpander => Row.ShowExpander;

        /// <inheritdoc/>
        public object? Value => _inner.Value;

        /// <inheritdoc/>
        public bool IsExpanded
        {
            get => Row.IsExpanded;
            set => Row.IsExpanded = value;
        }

        #endregion

        #region Public

        /// <inheritdoc/>
        public void Dispose()
        {
            Row.PropertyChanged -= RowPropertyChanged;

            _subscription?.Dispose();
            (_inner as IDisposable)?.Dispose();
        }

        #endregion

        #region Private

        private void RowPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Row.IsExpanded) ||
                e.PropertyName == nameof(Row.ShowExpander))
            {
                RaisePropertyChanged(e.PropertyName);
            }
        }

        #endregion

        #region IExpanderCell

        object IExpanderCell.Content => Content;

        IRow IExpanderCell.Row => Row;

        #endregion
    }
}
