using Avalonia.Controls;
using BlackSharp.UI.Avalonia.TreeDataGrid.Models;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Primitives
{
    internal sealed class TreeDataGridColumnLayoutSubscription<TPresenter> : IDisposable
        where TPresenter : Control
    {
        #region Constructor

        internal TreeDataGridColumnLayoutSubscription(TPresenter owner, IColumns columns, Action<TPresenter> invalidate)
        {
            _owner                    = new WeakReference<TPresenter>(owner);
            _columns                  = columns;
            _invalidate               = invalidate;
            columns.LayoutInvalidated += OnLayoutInvalidated;
        }

        #endregion

        #region Fields

        private readonly WeakReference<TPresenter> _owner;
        private readonly Action<TPresenter>        _invalidate;
        private IColumns?                          _columns;

        #endregion

        #region Public

        public void Dispose()
        {
            if (_columns is { } columns)
            {
                columns.LayoutInvalidated -= OnLayoutInvalidated;
                _columns                  = null;
            }
        }

        #endregion

        #region Private

        private void OnLayoutInvalidated(object? sender, EventArgs args)
        {
            if (_owner.TryGetTarget(out var owner))
            {
                _invalidate(owner);
            }
            else
            {
                Dispose();
            }
        }

        #endregion
    }
}
