using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using BlackSharp.UI.Avalonia.TreeDataGrid.Primitives;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Automation.Peers
{
    public class TreeDataGridCheckBoxCellAutomationPeer : TreeDataGridCellAutomationPeer, IToggleProvider
    {
        #region Constructor

        public TreeDataGridCheckBoxCellAutomationPeer(TreeDataGridCheckBoxCell owner) : base(owner)
        {
            owner.PropertyChanged += OnOwnerPropertyChanged;
        }

        #endregion

        #region Properties

        public new TreeDataGridCheckBoxCell Owner => (TreeDataGridCheckBoxCell)base.Owner;

        public ToggleState ToggleState => Owner.Value switch
        {
            true  => ToggleState.On,
            false => ToggleState.Off,
            _     => ToggleState.Indeterminate,
        };

        #endregion

        #region Public

        public void Toggle()
        {
            if (!Owner.IsReadOnly && Owner.IsEnabled)
            {
                Owner.Value = Owner.Value switch
                {
                    false => true,
                    true  => Owner.IsThreeState ? null : false,
                    _     => false,
                };
            }
        }

        #endregion

        #region Protected

        protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.CheckBox;

        #endregion

        #region Private

        private static ToggleState ToToggleState(object? value) => value switch
        {
            true  => ToggleState.On,
            false => ToggleState.Off,
            _     => ToggleState.Indeterminate,
        };

        private void OnOwnerPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs change)
        {
            if (change.Property == TreeDataGridCheckBoxCell.ValueProperty)
            {
                RaisePropertyChangedEvent(TogglePatternIdentifiers.ToggleStateProperty,
                    ToToggleState(change.OldValue), ToToggleState(change.NewValue));
            }
        }

        #endregion
    }
}
