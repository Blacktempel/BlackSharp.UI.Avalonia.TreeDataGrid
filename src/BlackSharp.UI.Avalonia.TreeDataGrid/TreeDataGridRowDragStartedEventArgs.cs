using Avalonia.Input;
using Avalonia.Interactivity;

namespace BlackSharp.UI.Avalonia.TreeDataGrid
{
    /// <summary>
    /// Provides data for the <see cref="TreeDataGrid.RowDragStarted"/> event.
    /// </summary>
    public class TreeDataGridRowDragStartedEventArgs : RoutedEventArgs
    {
        #region Constructor

        public TreeDataGridRowDragStartedEventArgs(IEnumerable<object> models)
            : base(TreeDataGrid.RowDragStartedEvent)
        {
            Models = models;
        }

        #endregion

        #region Properties

        public DragDropEffects AllowedEffects { get; set; }

        public IEnumerable<object> Models { get; }

        #endregion
    }
}
