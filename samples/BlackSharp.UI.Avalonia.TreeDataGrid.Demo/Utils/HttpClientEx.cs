namespace BlackSharp.UI.Avalonia.TreeDataGrid.Demo.Utils;

internal class HttpClientEx : HttpClient
{
    #region Constructor

    public HttpClientEx()
    {
        DefaultRequestHeaders.Add("User-Agent", UserAgent);
    }

    #endregion

    #region Fields

    private const string UserAgent = @"AvaloniaTreeDataGridSample/1.0 (https://avaloniaui.net; team@avaloniaui.net)";

    #endregion
}
