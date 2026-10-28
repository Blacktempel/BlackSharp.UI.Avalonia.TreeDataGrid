using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Avalonia.Controls;
using Avalonia.VisualTree;
using BlackSharp.UI.Avalonia.TreeDataGrid.Primitives;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Automation.Peers
{
    public class TreeDataGridAutomationPeer : ControlAutomationPeer, ISelectionProvider, IScrollProvider
    {
        #region Constructor

        public TreeDataGridAutomationPeer(TreeDataGrid owner) : base(owner)
        {
        }

        #endregion

        #region Properties

        public new TreeDataGrid Owner => (TreeDataGrid)base.Owner;

        public bool CanSelectMultiple => Owner.Selection?.SingleSelect == false;

        public bool IsSelectionRequired => false;

        private IScrollProvider? ScrollProvider => Owner.Scroll is ScrollViewer viewer ? new ScrollViewerAutomationPeer(viewer) : null;

        public bool HorizontallyScrollable => ScrollProvider?.HorizontallyScrollable ?? false;

        public double HorizontalScrollPercent => ScrollProvider?.HorizontalScrollPercent ?? -1;

        public double HorizontalViewSize => ScrollProvider?.HorizontalViewSize ?? 100;

        public bool VerticallyScrollable => ScrollProvider?.VerticallyScrollable ?? false;

        public double VerticalScrollPercent => ScrollProvider?.VerticalScrollPercent ?? -1;

        public double VerticalViewSize => ScrollProvider?.VerticalViewSize ?? 100;

        #endregion

        #region Public

        public IReadOnlyList<AutomationPeer> GetSelection()
        {
            if (Owner.Selection?.IsCellSelectionModel == true)
            {
                return Owner.GetVisualDescendants().OfType<TreeDataGridCell>()
                    .Where(x => x.IsSelected).Select(x => GetOrCreate(x)).ToArray();
            }

            return Owner.RowsPresenter?.GetRealizedElements().OfType<TreeDataGridRow>()
                .Where(x => x.IsSelected).Select(x => GetOrCreate(x)).ToArray() ?? [];
        }

        public void Scroll(ScrollAmount horizontalAmount, ScrollAmount verticalAmount) =>
            ScrollProvider?.Scroll(horizontalAmount, verticalAmount);

        public void SetScrollPercent(double horizontalPercent, double verticalPercent) =>
            ScrollProvider?.SetScrollPercent(horizontalPercent, verticalPercent);

        #endregion

        #region Protected

        protected override bool IsContentElementCore() => true;

        protected override bool IsControlElementCore() => true;

        protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.DataGrid;

        protected override string GetClassNameCore() => nameof(TreeDataGrid);

        protected override IReadOnlyList<AutomationPeer> GetOrCreateChildrenCore() => GetChildrenCore();

        protected override IReadOnlyList<AutomationPeer> GetChildrenCore() =>
            (Owner.ColumnHeadersPresenter?.GetRealizedElements() ?? [])
                .Concat(Owner.RowsPresenter?.GetRealizedElements() ?? []).Select(x => GetOrCreate(x)).ToArray();

        #endregion
    }
}
