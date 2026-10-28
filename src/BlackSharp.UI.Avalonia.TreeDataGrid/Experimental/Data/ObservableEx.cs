using Avalonia.Reactive;
using BlackSharp.UI.Avalonia.TreeDataGrid.Experimental.Data.Core;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Experimental.Data
{
    internal static class ObservableEx
    {
        #region Public

        public static IObservable<T> SingleValue<T>(T value)
        {
            return new SingleValueImpl<T>(value);
        }

        public static IDisposable Subscribe<T>(this IObservable<T> observable, Action<T> onNext)
        {
            return observable.Subscribe(new AnonymousObserver<T>(onNext));
        }

        #endregion

        #region Nested Types

        private sealed class SingleValueImpl<T> : IObservable<T>
        {
            #region Constructor

            public SingleValueImpl(T value)
            {
                _value = value;
            }

            #endregion

            #region Fields

            private readonly T _value;

            #endregion

            #region Public

            public IDisposable Subscribe(IObserver<T> observer)
            {
                observer.OnNext(_value);

                return Disposable.Empty;
            }

            #endregion
        }

        #endregion
    }
}
