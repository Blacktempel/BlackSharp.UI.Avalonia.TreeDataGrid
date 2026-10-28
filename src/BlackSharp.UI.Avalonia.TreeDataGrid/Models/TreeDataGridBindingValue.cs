using Avalonia;
using Avalonia.Data;

namespace BlackSharp.UI.Avalonia.TreeDataGrid
{
    internal sealed class TreeDataGridBindingValue<T> : StyledElement,
        IObservable<BindingValue<T>>,
        IObserver<BindingValue<T>>,
        IDisposable
    {
        #region Constructor

        public TreeDataGridBindingValue(object model, BindingBase? binding)
        {
            DataContext = model;

            if (binding is not null)
            {
                _binding = Bind(ValueProperty, binding);
            }
        }

        #endregion

        #region Fields

        // Internal binding hosts are never instantiated from XAML.
#pragma warning disable AVP1002
        public static readonly StyledProperty<T> ValueProperty =
            AvaloniaProperty.Register<TreeDataGridBindingValue<T>, T>(nameof(Value), defaultBindingMode: BindingMode.TwoWay);
#pragma warning restore AVP1002

        private IDisposable? _binding;

        #endregion

        #region Properties

        public T Value
        {
            get => GetValue(ValueProperty);
            set => SetCurrentValue(ValueProperty, value);
        }

        #endregion

        #region Public

        public IDisposable Subscribe(IObserver<BindingValue<T>> observer) =>
            new Subscription(this, this.GetBindingObservable(ValueProperty).Subscribe(observer));

        public void OnNext(BindingValue<T> value)
        {
            if (value.HasValue)
            {
                Value = value.Value;
            }
        }

        public void OnError(Exception error)
        {
        }

        public void OnCompleted()
        {
        }

        public void Dispose()
        {
            _binding?.Dispose();
            _binding = null;

            ClearValue(ValueProperty);
            ClearValue(DataContextProperty);
        }

        #endregion

        #region Nested Types

        private sealed class Subscription(TreeDataGridBindingValue<T> owner, IDisposable subscription) : IDisposable
        {
            #region Public

            public void Dispose()
            {
                subscription.Dispose();
                owner.Dispose();
            }

            #endregion
        }

        #endregion
    }
}
