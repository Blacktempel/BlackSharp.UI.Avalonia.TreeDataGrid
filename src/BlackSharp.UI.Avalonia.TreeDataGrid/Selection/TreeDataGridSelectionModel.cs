using BlackSharp.UI.Avalonia.TreeDataGrid.Models;
using System.Collections;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Selection
{
    public abstract class TreeDataGridSelectionModel : NotifyingBase
    {
        #region Properties

        public abstract bool SingleSelect { get; set; }

        public abstract bool IsCellSelectionModel { get; }

        public abstract IEnumerable? Source { get; }

        public abstract IndexPath SelectedRowIndex { get; set; }

        public abstract object? SelectedRowItem { get; }

        protected bool HasUntypedSelectionChangedHandlers => SelectionChanged is not null;

        internal abstract ITreeDataGridSelection Core { get; }

        internal abstract TreeDataGridSource Owner { get; }

        #endregion

        #region Internal

        internal abstract void SelectAllCore();

        internal abstract void ClearCore();

        internal virtual void SelectRowCore(IndexPath index) => SelectedRowIndex = index;

        protected internal abstract void SetSource(IEnumerable? source);

        #endregion

        #region Protected

        protected void RaiseUntypedSelectionChanged(TreeDataGridSelectionChangedEventArgs args) => SelectionChanged?.Invoke(this, args);

        protected void RaiseIndexesChanged(TreeDataGridIndexesChangedEventArgs args) => IndexesChanged?.Invoke(this, args);

        protected void RaiseSourceReset(TreeDataGridItemsResetEventArgs args) => SourceReset?.Invoke(this, args);

        #endregion

        #region Events

        public event EventHandler<TreeDataGridSelectionChangedEventArgs>? SelectionChanged;

        public event EventHandler<TreeDataGridIndexesChangedEventArgs>? IndexesChanged;

        public event EventHandler<TreeDataGridItemsResetEventArgs>? SourceReset;

        #endregion
    }
}
