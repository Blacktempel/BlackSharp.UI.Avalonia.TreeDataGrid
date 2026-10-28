using Avalonia.Data;
using BlackSharp.UI.Avalonia.TreeDataGrid.Experimental.Data;
using BlackSharp.UI.Avalonia.TreeDataGrid.Experimental.Data.Core;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Models
{
    internal class CheckBoxCell : NotifyingBase, IEditableCell, IDisposable
    {
        #region Constructor

        public CheckBoxCell(bool? value)
        {
            _value     = value;
            IsReadOnly = true;
        }

        public CheckBoxCell(
            IObserver<BindingValue<bool?>> bindingObserver,
            IObservable<BindingValue<bool?>> bindingObservable,
            IObservable<BindingValue<bool>> isReadOnlyObservable,
            bool isThreeState)
        {
            _binding     = bindingObserver;
            IsThreeState = isThreeState;

            _subscription = new CompositeDisposable(
                bindingObservable.Subscribe(x =>
                {
                    if (x.HasValue)
                    {
                        Value = x.Value;
                    }
                }),
                isReadOnlyObservable.Subscribe(x =>
                {
                    if (x.HasValue)
                    {
                        IsReadOnly = x.Value;
                    }
                }));
        }

        #endregion

        #region Fields

        private readonly IObserver<BindingValue<bool?>>? _binding;

        private readonly IDisposable?                    _subscription;

        private bool?                                    _value;

        #endregion

        #region Properties

        public bool CanEdit => false;

        public BeginEditGestures EditGestures => BeginEditGestures.None;

        public bool SingleTapEdit => false;

        /// <inheritdoc/>
        public bool IsReadOnly { get; private set; }

        public bool IsThreeState { get; }

        /// <inheritdoc cref="ICell.Value"/>
        public bool? Value
        {
            get => _value;
            set
            {
                if (RaiseAndSetIfChanged(ref _value, value) && !IsReadOnly)
                {
                    _binding!.OnNext(value);
                }
            }
        }

        #endregion

        #region Public

        /// <inheritdoc/>
        public void Dispose()
        {
            _subscription?.Dispose();
            GC.SuppressFinalize(this);
        }

        #endregion

        #region ICell

        object? ICell.Value => Value;

        #endregion
    }
}
