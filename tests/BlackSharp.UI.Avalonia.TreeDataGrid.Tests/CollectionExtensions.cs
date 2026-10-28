namespace BlackSharp.UI.Avalonia.TreeDataGrid.Tests
{
    internal static class CollectionExtensions
    {
        public static int CollectionChangedSubscriberCount<T>(this AvaloniaListDebug<T> list)
        {
            return list.GetCollectionChangedSubscribers()?.Length ?? 0;
        }
    }
}
