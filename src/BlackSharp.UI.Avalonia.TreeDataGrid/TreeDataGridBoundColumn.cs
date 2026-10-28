using Avalonia;
using Avalonia.Data;

namespace BlackSharp.UI.Avalonia.TreeDataGrid
{
    public abstract class TreeDataGridBoundColumn : TreeDataGridColumn
    {
        #region Constructor

        private protected TreeDataGridBoundColumn()
        {
        }

        #endregion

        #region Fields

        public static readonly StyledProperty<BindingBase?> BindingProperty    =
            AvaloniaProperty.Register<TreeDataGridBoundColumn, BindingBase?>(nameof(Binding));

        public static readonly StyledProperty<bool>         IsReadOnlyProperty =
            AvaloniaProperty.Register<TreeDataGridBoundColumn, bool>(nameof(IsReadOnly), false);

        #endregion

        #region Properties

        [AssignBinding]
        public BindingBase? Binding
        {
            get => GetValue(BindingProperty);
            set => SetValue(BindingProperty, value);
        }

        public bool IsReadOnly
        {
            get => GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }

        internal Type? ValueType { get; set; }

        #endregion

        #region Internal

        internal override object? GetCellValue(object model)
        {
            if (ValueSelector is not null)
            {
                return ValueSelector(model);
            }

            using var binding = new TreeDataGridBindingValue<object?>(model, Binding);

            return binding.Value;
        }

        #endregion

        #region Protected

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            if (change.Property == BindingProperty)
            {
                ValueSelector = null;
                ValueType     = null;
            }

            base.OnPropertyChanged(change);
        }

        #endregion
    }
}
