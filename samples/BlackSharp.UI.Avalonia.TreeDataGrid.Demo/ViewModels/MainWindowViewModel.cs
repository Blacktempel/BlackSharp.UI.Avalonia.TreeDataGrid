namespace BlackSharp.UI.Avalonia.TreeDataGrid.Demo.ViewModels
{
    internal class MainWindowViewModel : IDisposable
    {
        #region Fields

        private CountriesPageViewModel?  _countries;

        private FilesPageViewModel?      _files;

        private WikipediaPageViewModel?  _wikipedia;

        private DragDropPageViewModel?   _dragDrop;

        private DynamicColumnsViewModel? _dynamic;

        private FeaturesPageViewModel?   _features;

        #endregion

        #region Properties

        public FeaturesPageViewModel Features => _features ??= new FeaturesPageViewModel();

        public CountriesPageViewModel Countries => _countries ??= new CountriesPageViewModel();

        public FilesPageViewModel Files => _files ??= new FilesPageViewModel();

        public WikipediaPageViewModel Wikipedia => _wikipedia ??= new WikipediaPageViewModel();

        public DragDropPageViewModel DragDrop => _dragDrop ??= new DragDropPageViewModel();

        public DynamicColumnsViewModel Dynamic => _dynamic ??= new DynamicColumnsViewModel();

        #endregion

        #region Public

        public void Dispose()
        {
            _features?.Dispose();
            _countries?.Source.Dispose();
            _files?.Dispose();
            _wikipedia?.Dispose();
            _dragDrop?.Source.Dispose();
            _dynamic?.Source.Dispose();
        }

        #endregion
    }
}
