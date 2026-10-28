namespace BlackSharp.UI.Avalonia.TreeDataGrid.Models
{
    internal abstract class RowEventArgs : EventArgs
    {
        #region Properties

        public IRow Row => GetUntypedRow();

        #endregion

        #region Public

        public static RowEventArgs<T> Create<T>(T row) where T : IRow
        {
            return new RowEventArgs<T>(row);
        }

        #endregion

        #region Protected

        protected abstract IRow GetUntypedRow();

        #endregion
    }

    internal class RowEventArgs<TRow> : RowEventArgs
        where TRow : IRow
    {
        #region Constructor

        public RowEventArgs(TRow row) => Row = row;

        #endregion

        #region Properties

        public new TRow Row { get; }

        #endregion

        #region Protected

        protected override IRow GetUntypedRow() => Row;

        #endregion
    }
}
