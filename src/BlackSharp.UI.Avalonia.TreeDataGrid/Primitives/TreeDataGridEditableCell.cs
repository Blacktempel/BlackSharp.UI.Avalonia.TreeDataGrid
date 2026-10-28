using Avalonia;
using BlackSharp.UI.Avalonia.TreeDataGrid.Models;
using BlackSharp.UI.Avalonia.TreeDataGrid.Selection;
using System.ComponentModel;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Primitives
{
    public class TreeDataGridEditableCell : TreeDataGridCell
    {
        #region Fields

        public static readonly DirectProperty<TreeDataGridEditableCell, bool> IsReadOnlyProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridEditableCell, bool>(
                nameof(IsReadOnly),
                o => o.IsReadOnly,
                (o, v) => o.IsReadOnly = v);

        private bool _isReadOnly;

        #endregion

        #region Properties

        public bool IsReadOnly
        {
            get => _isReadOnly;
            set
            {
                if (SetAndRaise(IsReadOnlyProperty, ref _isReadOnly, value) && value && IsEditing)
                {
                    CancelEdit();
                }
            }
        }

        #endregion

        #region Public

        public override void Unrealize()
        {
            UnsubscribeFromModelChanges();
            base.Unrealize();
        }

        #endregion

        #region Internal

        internal override void Realize(
            TreeDataGridElementFactory factory,
            IEngineSelectionInteraction? selection,
            ICell model,
            int columnIndex,
            int rowIndex)
        {
            if (model is IEditableCell cell)
            {
                IsReadOnly = cell.IsReadOnly;
            }
            else
            {
                throw new InvalidOperationException("Invalid cell model.");
            }

            base.Realize(factory, selection, model, columnIndex, rowIndex);
            SubscribeToModelChanges();
        }

        #endregion

        #region Protected

        protected override void OnModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            base.OnModelPropertyChanged(sender, e);

            if (e.PropertyName == nameof(IEditableCell.IsReadOnly) && Model is IEditableCell cell)
            {
                IsReadOnly = cell.IsReadOnly;
            }
        }

        #endregion
    }
}
