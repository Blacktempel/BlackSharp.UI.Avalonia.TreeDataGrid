using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using BlackSharp.UI.Avalonia.TreeDataGrid.Demo.Models;
using BlackSharp.UI.Avalonia.TreeDataGrid.Demo.Utils;
using BlackSharp.UI.Avalonia.TreeDataGrid.Models;
using BlackSharp.UI.Avalonia.TreeDataGrid.Selection;
using System.Globalization;
using System.Runtime.InteropServices;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Demo.ViewModels
{
    public class FilesPageViewModel : NotifyingBase, IDisposable
    {
        #region Constructor

        public FilesPageViewModel()
        {
            Drives = DriveInfo.GetDrives().Select(x => x.Name).ToList();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _selectedDrive = "C:\\";
            }
            else
            {
                _selectedDrive = Drives.FirstOrDefault() ?? "/";
            }

            _source           = _treeSource = CreateTreeSource();
            _root             = new FileTreeNodeModel(_selectedDrive, isDirectory: true, isRoot: true);
            _treeSource.Items = [_root];

            this.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(SelectedDrive))
                {
                    var previousRoot = _root;

                    _root = new FileTreeNodeModel(_selectedDrive, isDirectory: true, isRoot: true);
                    _treeSource!.Items = [_root];

                    if (_flatSource is not null)
                    {
                        _flatSource.Items = _root.Children;
                    }

                    previousRoot?.Dispose();
                }
            };
        }

        #endregion

        #region Fields

        private static IconConverter? s_iconConverter;

        private readonly HierarchicalTreeDataGridSource<FileTreeNodeModel>? _treeSource;

        private FlatTreeDataGridSource<FileTreeNodeModel>?                  _flatSource;

        private TreeDataGridSource<FileTreeNodeModel>                       _source;

        private bool                                                        _cellSelection;

        private FileTreeNodeModel?                                          _root;

        private string                                                      _selectedDrive;

        private string?                                                     _selectedPath;

        #endregion

        #region Properties

        public bool CellSelection
        {
            get => _cellSelection;
            set
            {
                if (_cellSelection != value)
                {
                    _cellSelection = value;

                    Source.Selection!.SelectionChanged -= SelectionChanged;

                    if (_cellSelection)
                    {
                        Source.Selection = new TreeDataGridCellSelectionModel<FileTreeNodeModel>(Source) { SingleSelect = false };
                    }
                    else
                    {
                        Source.Selection = new TreeDataGridRowSelectionModel<FileTreeNodeModel>(Source) { SingleSelect = false };
                    }

                    Source.Selection!.SelectionChanged += SelectionChanged;
                    this.RaisePropertyChanged();
                }
            }
        }

        public IList<string> Drives { get; }

        public bool FlatList
        {
            get => Source != _treeSource;
            set
            {
                if (value != FlatList)
                {
                    Source          = value ? _flatSource ??= CreateFlatSource() : _treeSource!;
                    var desiredMode = _cellSelection;

                    _cellSelection = Source.Selection!.IsCellSelectionModel;
                    CellSelection  = desiredMode;
                    RaisePropertyChanged(nameof(FlatList));
                }
            }
        }

        public string SelectedDrive
        {
            get => _selectedDrive;
            set => this.RaiseAndSetIfChanged(ref _selectedDrive, value);
        }

        public string? SelectedPath
        {
            get => _selectedPath;
            set => SetSelectedPath(value);
        }

        public TreeDataGridSource<FileTreeNodeModel> Source
        {
            get => _source;
            private set => this.RaiseAndSetIfChanged(ref _source, value);
        }

        public static IMultiValueConverter FileIconConverter
        {
            get
            {
                if (s_iconConverter is null)
                {
                    using (var fileStream = AssetLoader.Open(new Uri("avares://BlackSharp.UI.Avalonia.TreeDataGrid.Demo/Assets/file.png")))
                    {
                        using (var folderStream = AssetLoader.Open(new Uri("avares://BlackSharp.UI.Avalonia.TreeDataGrid.Demo/Assets/folder.png")))
                        {
                            using (var folderOpenStream = AssetLoader.Open(new Uri("avares://BlackSharp.UI.Avalonia.TreeDataGrid.Demo/Assets/folder-open.png")))
                            {
                                var fileIcon       = new Bitmap(fileStream);
                                var folderIcon     = new Bitmap(folderStream);
                                var folderOpenIcon = new Bitmap(folderOpenStream);

                                s_iconConverter = new IconConverter(fileIcon, folderOpenIcon, folderIcon);
                            }
                        }
                    }
                }

                return s_iconConverter;
            }
        }

        #endregion

        #region Public

        public void Dispose()
        {
            _treeSource?.Dispose();
            _flatSource?.Dispose();
            _root?.Dispose();
        }

        #endregion

        #region Private

        private FlatTreeDataGridSource<FileTreeNodeModel> CreateFlatSource()
        {
            var result = new FlatTreeDataGridSource<FileTreeNodeModel>(_root?.Children ?? [])
                .WithCheckBoxColumn(x => x.IsChecked, x => x.CanUserResize = false)
                .WithColumn(CreateNameColumn())
                .WithTextColumn("Size", x => x.Size, x => ConfigureSort(x, FileTreeNodeModel.SortAscending(node => node.Size)))
                .WithTextColumn("Modified", x => x.Modified, x => ConfigureSort(x, FileTreeNodeModel.SortAscending(node => node.Modified)));

            result.RowSelection!.SingleSelect = false;
            result.Selection!.SelectionChanged += SelectionChanged;

            return result;
        }

        private HierarchicalTreeDataGridSource<FileTreeNodeModel> CreateTreeSource()
        {
            var result = new HierarchicalTreeDataGridSource<FileTreeNodeModel>([])
                .WithCheckBoxColumn(x => x.IsChecked, x => x.CanUserResize = false)
                .WithHierarchicalExpanderColumn("Name", CreateNameColumn(), x => x.Children,
                    x => x.IsExpanded, x => x.HasChildren, x =>
                    {
                        x.Width = GridLength.Star;
                        ConfigureSort(x, FileTreeNodeModel.SortAscending(node => node.Name));
                    })
                .WithTextColumn("Size", x => x.Size, x => ConfigureSort(x, FileTreeNodeModel.SortAscending(node => node.Size)))
                .WithTextColumn("Modified", x => x.Modified, x => ConfigureSort(x, FileTreeNodeModel.SortAscending(node => node.Modified)));

            result.RowSelection!.SingleSelect = false;
            result.Selection!.SelectionChanged += SelectionChanged;

            return result;
        }

        private static TreeDataGridTemplateColumn CreateNameColumn() => new("FileNameCell")
        {
            Header           = "Name",
            Width            = GridLength.Star,
            CompareAscending = (a, b) => FileTreeNodeModel.SortAscending(x => x.Name)((FileTreeNodeModel?)a, (FileTreeNodeModel?)b),
        };

        private static void ConfigureSort(ColumnCreateOptions options, Comparison<FileTreeNodeModel?> comparison) =>
            options.CompareAscending = (a, b) => comparison((FileTreeNodeModel?)a, (FileTreeNodeModel?)b);

        private void SetSelectedPath(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                if (Source.Selection is { } selection)
                {
                    selection.SelectedRowIndex = default;
                }

                return;
            }

            FlatList = false;

            var path         = value;
            var components   = new Stack<string>();
            DirectoryInfo? d = null;

            if (File.Exists(path))
            {
                var f = new FileInfo(path);

                components.Push(f.Name);
                d = f.Directory;
            }
            else if (Directory.Exists(path))
            {
                d = new DirectoryInfo(path);
            }

            while (d is not null)
            {
                components.Push(d.Name);
                d = d.Parent;
            }

            var index = IndexPath.Unselected;

            if (components.Count > 0)
            {
                var drive      = components.Pop();
                var driveIndex = Drives.FindIndex(x => string.Equals(x, drive, StringComparison.OrdinalIgnoreCase));

                if (driveIndex >= 0)
                {
                    SelectedDrive = Drives[driveIndex];
                }

                FileTreeNodeModel? node = _root;

                index = new IndexPath(0);

                while (node is not null && components.Count > 0)
                {
                    node.IsExpanded = true;

                    var component = components.Pop();
                    var i         = node.Children.FindIndex(x => string.Equals(x.Name, component, StringComparison.OrdinalIgnoreCase));

                    node  = i >= 0 ? node.Children[i] : null;
                    index = i >= 0 ? index.Append(i) : default;
                }
            }

            if (Source.Selection is { } pathSelection)
            {
                pathSelection.SelectedRowIndex = index;
            }
        }

        private void SelectionChanged(object? sender, TreeDataGridSelectionChangedEventArgs e)
        {
            var selectedPath = (Source.Selection?.SelectedRowItem as FileTreeNodeModel)?.Path;

            this.RaiseAndSetIfChanged(ref _selectedPath, selectedPath, nameof(SelectedPath));

            foreach (var i in e.DeselectedItems)
            {
                System.Diagnostics.Trace.WriteLine($"Deselected '{(i as FileTreeNodeModel)?.Path}'");
            }

            foreach (var i in e.SelectedItems)
            {
                System.Diagnostics.Trace.WriteLine($"Selected '{(i as FileTreeNodeModel)?.Path}'");
            }
        }

        #endregion

        #region Nested Types

        private class IconConverter : IMultiValueConverter
        {
            #region Constructor

            public IconConverter(Bitmap file, Bitmap folderExpanded, Bitmap folderCollapsed)
            {
                _file            = file;
                _folderExpanded  = folderExpanded;
                _folderCollapsed = folderCollapsed;
            }

            #endregion

            #region Fields

            private readonly Bitmap _file;

            private readonly Bitmap _folderExpanded;

            private readonly Bitmap _folderCollapsed;

            #endregion

            #region Public

            public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
            {
                if (values.Count == 2 &&
                    values[0] is bool isDirectory &&
                    values[1] is bool isExpanded)
                {
                    if (!isDirectory)
                    {
                        return _file;
                    }
                    else
                    {
                        return isExpanded ? _folderExpanded : _folderCollapsed;
                    }
                }

                return null;
            }

            #endregion
        }

        #endregion
    }
}
