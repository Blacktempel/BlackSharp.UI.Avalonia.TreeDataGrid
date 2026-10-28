namespace BlackSharp.UI.Avalonia.TreeDataGrid.Models
{
    internal class FuncComparer<T> : IComparer<T>
    {
        #region Constructor

        public FuncComparer(Comparison<T?> func) => _func = func;

        #endregion

        #region Fields

        private readonly Comparison<T?> _func;

        #endregion

        #region Public

        public int Compare(T? x, T? y) => _func(x, y);

        #endregion
    }
}
