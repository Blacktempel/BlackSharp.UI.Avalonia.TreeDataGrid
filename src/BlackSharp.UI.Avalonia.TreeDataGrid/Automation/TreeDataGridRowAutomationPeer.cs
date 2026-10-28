using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Avalonia.VisualTree;
using BlackSharp.UI.Avalonia.TreeDataGrid.Models;
using BlackSharp.UI.Avalonia.TreeDataGrid.Primitives;
using BlackSharp.UI.Avalonia.TreeDataGrid.Selection;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Automation.Peers
{
    public class TreeDataGridRowAutomationPeer : ControlAutomationPeer, ISelectionItemProvider, IExpandCollapseProvider
    {
        #region Constructor

        public TreeDataGridRowAutomationPeer(TreeDataGridRow owner) : base(owner)
        {
            owner.PropertyChanged += OnOwnerPropertyChanged;
        }

        #endregion

        #region Properties

        public new TreeDataGridRow Owner => (TreeDataGridRow)base.Owner;

        private TreeDataGrid? Grid => Owner.FindAncestorOfType<TreeDataGrid>();

        private IndexPath Index => Grid?.Source?.RowIndexToModelIndex(Owner.RowIndex) ?? default;

        private IExpander? Expander => Grid?.Source is { } source && (uint)Owner.RowIndex < source.Rows.Count ?
            source.Rows[Owner.RowIndex] as IExpander : null;

        public bool IsSelected => Owner.IsSelected;

        public ISelectionProvider? SelectionContainer => Grid is { } grid ? GetOrCreate(grid) as ISelectionProvider : null;

        public ExpandCollapseState ExpandCollapseState => Expander is not { ShowExpander: true } ?
            ExpandCollapseState.LeafNode : Expander.IsExpanded ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed;

        public bool ShowsMenu => false;

        #endregion

        #region Public

        public void Select()
        {
            if (Grid?.Selection is { } selection && Owner.IsEnabled)
            {
                selection.SelectedRowIndex = Index;
            }
        }

        public void AddToSelection()
        {
            if (Grid?.Selection is ITreeDataGridRowSelection selection && Owner.IsEnabled)
            {
                selection.Select(Index);
            }
        }

        public void RemoveFromSelection()
        {
            if (Grid?.Selection is ITreeDataGridRowSelection selection && Owner.IsEnabled)
            {
                selection.Deselect(Index);
            }
        }

        public void Expand()
        {
            if (Expander is { } expander && Owner.IsEnabled)
            {
                expander.IsExpanded = true;
            }
        }

        public void Collapse()
        {
            if (Expander is { } expander && Owner.IsEnabled)
            {
                expander.IsExpanded = false;
            }
        }

        #endregion

        #region Protected

        protected override bool IsContentElementCore() => true;

        protected override bool IsControlElementCore() => true;

        protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.DataItem;

        protected override string GetNameCore() =>
            Owner.CellsPresenter?.GetRealizedElements().OfType<TreeDataGridCell>().FirstOrDefault()?.Model?.Value?.ToString() ?? Owner.Model?.ToString() ?? string.Empty;

        protected override IReadOnlyList<AutomationPeer> GetOrCreateChildrenCore() => GetChildrenCore();

        protected override IReadOnlyList<AutomationPeer> GetChildrenCore() =>
            Owner.CellsPresenter?.GetRealizedElements().Select(x => GetOrCreate(x)).ToArray() ?? [];

        protected override object? GetProviderCore(Type providerType) =>
            providerType == typeof(IExpandCollapseProvider) && Expander is not { ShowExpander: true } ? null : base.GetProviderCore(providerType);

        #endregion

        #region Private

        private void OnOwnerPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs change)
        {
            if (change.Property == TreeDataGridRow.IsSelectedProperty)
            {
                RaisePropertyChangedEvent(SelectionItemPatternIdentifiers.IsSelectedProperty, change.OldValue, change.NewValue);
            }
        }

        #endregion
    }
}
