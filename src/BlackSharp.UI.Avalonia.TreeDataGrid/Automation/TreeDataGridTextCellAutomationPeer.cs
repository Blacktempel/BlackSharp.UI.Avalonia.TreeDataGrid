using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using BlackSharp.UI.Avalonia.TreeDataGrid.Primitives;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Automation.Peers
{
    public class TreeDataGridTextCellAutomationPeer : TreeDataGridCellAutomationPeer, IValueProvider
    {
        #region Constructor

        public TreeDataGridTextCellAutomationPeer(TreeDataGridTextCell owner) : base(owner)
        {
            owner.PropertyChanged += OnOwnerPropertyChanged;
        }

        #endregion

        #region Properties

        public new TreeDataGridTextCell Owner => (TreeDataGridTextCell)base.Owner;

        public bool IsReadOnly => Owner.IsReadOnly;

        public string Value => Owner.Value ?? string.Empty;

        #endregion

        #region Public

        public void SetValue(string? value)
        {
            if (!IsReadOnly && Owner.IsEnabled)
            {
                Owner.Value = value;
            }
        }

        #endregion

        #region Protected

        protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Edit;

        #endregion

        #region Private

        private void OnOwnerPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs change)
        {
            if (change.Property == TreeDataGridTextCell.ValueProperty)
            {
                RaisePropertyChangedEvent(ValuePatternIdentifiers.ValueProperty, change.OldValue, change.NewValue);
            }
            else if (change.Property == TreeDataGridEditableCell.IsReadOnlyProperty)
            {
                RaisePropertyChangedEvent(ValuePatternIdentifiers.IsReadOnlyProperty, change.OldValue, change.NewValue);
            }
        }

        #endregion
    }
}
