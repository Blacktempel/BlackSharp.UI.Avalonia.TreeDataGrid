using BlackSharp.UI.Avalonia.TreeDataGrid.Models;
using System.Diagnostics.CodeAnalysis;

namespace BlackSharp.UI.Avalonia.TreeDataGrid
{
    public sealed class HierarchicalTreeDataGridSource<TModel> : TreeDataGridSource<TModel>
        where TModel : class
    {
        #region Constructor

        public HierarchicalTreeDataGridSource(TModel item)
            : this(new[] { item })
        {
        }

        public HierarchicalTreeDataGridSource(IEnumerable<TModel> items)
            : this(new EngineHierarchicalTreeDataGridSource<TModel>(items))
        {
        }

        internal HierarchicalTreeDataGridSource(IEnumerable<TModel> items, TreeDataGridColumns columns)
            : this(new EngineHierarchicalTreeDataGridSource<TModel>(items), columns)
        {
        }

        private HierarchicalTreeDataGridSource(EngineHierarchicalTreeDataGridSource<TModel> engine, TreeDataGridColumns? columns = null)
            : base(engine, engine.Columns, columns)
        {
            _engine               = engine;
            _engine.RowExpanding  += OnRowExpanding;
            _engine.RowExpanded   += OnRowExpanded;
            _engine.RowCollapsing += OnRowCollapsing;
            _engine.RowCollapsed  += OnRowCollapsed;
        }

        #endregion

        #region Fields

        private readonly EngineHierarchicalTreeDataGridSource<TModel> _engine;

        #endregion

        #region Public

        public void Collapse(IndexPath index) => _engine.Collapse(index);

        public void CollapseAll() => _engine.CollapseAll();

        public void Expand(IndexPath index) => _engine.Expand(index);

        public void ExpandAll() => _engine.ExpandAll();

        public void ExpandCollapseRecursive(Func<TModel, bool> predicate) => _engine.ExpandCollapseRecursive(predicate);

        public void ExpandCollapseRecursive(ITreeDataGridRowModel row, Func<TModel, bool> predicate)
        {
            if (row is TreeDataGridRowModel { Inner: HierarchicalRow<TModel> inner })
            {
                _engine.ExpandCollapseRecursive(inner, predicate);
            }
        }

        public bool TryGetModelAt(IndexPath index, [NotNullWhen(true)] out TModel? result) => _engine.TryGetModelAt(index, out result);

        public void Sort(Comparison<TModel>? comparison) => _engine.Sort(comparison);

        #endregion

        #region Private

        private void OnRowExpanding(object? sender, RowEventArgs<HierarchicalRow<TModel>> e) =>
            RowExpanding?.Invoke(this, new TreeDataGridRowModelEventArgs(new TreeDataGridRowModel(e.Row, e.Row.ModelIndexPath)));

        private void OnRowExpanded(object? sender, RowEventArgs<HierarchicalRow<TModel>> e) =>
            RowExpanded?.Invoke(this, new TreeDataGridRowModelEventArgs(new TreeDataGridRowModel(e.Row, e.Row.ModelIndexPath)));

        private void OnRowCollapsing(object? sender, RowEventArgs<HierarchicalRow<TModel>> e) =>
            RowCollapsing?.Invoke(this, new TreeDataGridRowModelEventArgs(new TreeDataGridRowModel(e.Row, e.Row.ModelIndexPath)));

        private void OnRowCollapsed(object? sender, RowEventArgs<HierarchicalRow<TModel>> e) =>
            RowCollapsed?.Invoke(this, new TreeDataGridRowModelEventArgs(new TreeDataGridRowModel(e.Row, e.Row.ModelIndexPath)));

        #endregion

        #region Events

        public event EventHandler<TreeDataGridRowModelEventArgs>? RowExpanding;

        public event EventHandler<TreeDataGridRowModelEventArgs>? RowExpanded;

        public event EventHandler<TreeDataGridRowModelEventArgs>? RowCollapsing;

        public event EventHandler<TreeDataGridRowModelEventArgs>? RowCollapsed;

        #endregion
    }
}
