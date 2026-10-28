namespace BlackSharp.UI.Avalonia.TreeDataGrid
{
    [Flags]
    public enum TreeDataGridGridLinesVisibility
    {
        None       = 0,
        Horizontal = 1,
        Vertical   = 2,
        All        = Horizontal | Vertical,
    }
}
