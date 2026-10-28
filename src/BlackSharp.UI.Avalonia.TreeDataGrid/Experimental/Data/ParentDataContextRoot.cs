using Avalonia;
using Avalonia.VisualTree;
using BlackSharp.UI.Avalonia.TreeDataGrid.Experimental.Data.Core;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Experimental.Data
{
    internal class ParentDataContextRoot<T> : SingleSubscriberObservableBase<T?>
        where T : class
    {
        #region Constructor

        public ParentDataContextRoot(Visual source)
        {
            _source = source;
        }

        #endregion

        #region Fields

        private readonly Visual _source;

        #endregion

        #region Protected

        protected override void Subscribed()
        {
            _source.PropertyChanged += SourcePropertyChanged;
            StartListeningToDataContext(_source.GetVisualParent());
            PublishValue();
        }

        protected override void Unsubscribed()
        {
            _source.PropertyChanged -= SourcePropertyChanged;
        }

        #endregion

        #region Private

        private void PublishValue()
        {
            var parent = _source.GetVisualParent() as StyledElement;

            if (parent?.DataContext is null)
            {
                PublishNext(null);
            }
            else if (parent.DataContext is T value)
            {
                PublishNext(value);
            }
            else
            {
                // TODO: Log DataContext is unexpected type.
            }
        }

        private void StartListeningToDataContext(Visual? visual)
        {
            if (visual is StyledElement styled)
            {
                styled.PropertyChanged += ParentPropertyChanged;
            }
        }

        private void StopListeningToDataContext(Visual? visual)
        {
            if (visual is StyledElement styled)
            {
                styled.PropertyChanged -= ParentPropertyChanged;
            }
        }

        private void SourcePropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            // TODO: Double check this with @grokys
            if (e.Property == Visual.VisualParentProperty)
            {
                StopListeningToDataContext(_source.GetVisualParent());
                StartListeningToDataContext(_source.GetVisualParent());
                PublishValue();
            }
        }

        private void ParentPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property == StyledElement.DataContextProperty)
            {
                PublishValue();
            }
        }

        #endregion
    }
}
