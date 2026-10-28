namespace BlackSharp.UI.Avalonia.TreeDataGrid.Experimental.Data.Core;

internal class CompositeDisposable(IDisposable disposable1, IDisposable disposable2) : IDisposable
{
    #region Fields

    private readonly IDisposable _disposable1 = disposable1;

    private readonly IDisposable _disposable2 = disposable2;

    #endregion

    #region Public

    public void Dispose()
    {
        _disposable1.Dispose();
        _disposable2.Dispose();
    }

    #endregion
}
