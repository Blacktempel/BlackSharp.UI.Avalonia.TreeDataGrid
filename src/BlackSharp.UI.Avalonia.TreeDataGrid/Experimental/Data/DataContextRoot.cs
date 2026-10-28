using Avalonia;
using BlackSharp.UI.Avalonia.TreeDataGrid.Experimental.Data.Core;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Experimental.Data
{
    internal class DataContextRoot<T> : SingleSubscriberObservableBase<T?>
        where T : class
    {
        #region Constructor

        public DataContextRoot(StyledElement source)
        {
            _source = source;
        }

        #endregion

        #region Fields

        private readonly StyledElement _source;

        #endregion

        #region Protected

        protected override void Subscribed()
        {
            _source.PropertyChanged += PropertyChanged;
            PublishValue();
        }

        protected override void Unsubscribed()
        {
            _source.PropertyChanged -= PropertyChanged;
        }

        #endregion

        #region Private

        private void PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property == StyledElement.DataContextProperty)
            {
                PublishValue();
            }
        }

        private void PublishValue()
        {
            if (_source.DataContext is null)
            {
                PublishNext(null);
            }
            else if (_source.DataContext is T value)
            {
                PublishNext(value);
            }
            else
            {
                // TODO: Log DataContext is unexpected type.
            }
        }

        #endregion
    }
}
