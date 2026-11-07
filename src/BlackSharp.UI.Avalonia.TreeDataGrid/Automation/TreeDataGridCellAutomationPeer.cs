using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.VisualTree;
using BlackSharp.UI.Avalonia.TreeDataGrid.Primitives;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Automation.Peers
{
    public class TreeDataGridCellAutomationPeer : ControlAutomationPeer
    {
        #region Constructor

        public TreeDataGridCellAutomationPeer(TreeDataGridCell owner) : base(owner)
        {
        }

        #endregion

        #region Properties

        public new TreeDataGridCell Owner => (TreeDataGridCell)base.Owner;

        #endregion

        #region Protected

        protected override bool IsContentElementCore() => true;

        protected override bool IsControlElementCore() => true;

        protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.DataItem;

        /// <inheritdoc/>
        protected override string GetNameCore()
        {
            var name = base.GetNameCore();

            if (!string.IsNullOrWhiteSpace(name))
            {
                return name;
            }

            if (Owner is TreeDataGridTemplateCell or TreeDataGridExpanderCell)
            {
                // Read accessible names and text from visible template controls.
                foreach (var descendant in Owner.GetVisualDescendants())
                {
                    if (descendant is Control { IsEffectivelyVisible: true } control)
                    {
                        var controlName = AutomationProperties.GetName(control);

                        if (!string.IsNullOrWhiteSpace(controlName))
                        {
                            return controlName;
                        }

                        if (control is TextBlock { Text: { } text } && !string.IsNullOrWhiteSpace(text))
                        {
                            return text;
                        }
                    }
                }

                return string.Empty;
            }

            return Owner.Model?.Value?.ToString() ?? string.Empty;
        }

        #endregion
    }
}
