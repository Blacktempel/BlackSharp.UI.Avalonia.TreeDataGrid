namespace BlackSharp.UI.Avalonia.TreeDataGrid
{
    public sealed class FlatTreeDataGridSource<TModel> : TreeDataGridSource<TModel>
        where TModel : class
    {
        #region Constructor

        public FlatTreeDataGridSource(IEnumerable<TModel> items)
            : this(new EngineFlatTreeDataGridSource<TModel>(items))
        {
        }

        internal FlatTreeDataGridSource(IEnumerable<TModel> items, TreeDataGridColumns columns)
            : this(new EngineFlatTreeDataGridSource<TModel>(items), columns)
        {
        }

        private FlatTreeDataGridSource(EngineFlatTreeDataGridSource<TModel> engine, TreeDataGridColumns? columns = null)
            : base(engine, engine.Columns, columns)
        {
        }

        #endregion
    }
}
