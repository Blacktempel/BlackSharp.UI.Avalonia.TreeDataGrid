using Avalonia.Threading;
using BlackSharp.UI.Avalonia.TreeDataGrid.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Demo.Models
{
    public class FileTreeNodeModel : NotifyingBase, IEditableObject, IDisposable
    {
        #region Constructor

        public FileTreeNodeModel(
            string path,
            bool isDirectory,
            bool isRoot = false)
        {
            _path       = path;
            _name       = isRoot ? path : System.IO.Path.GetFileName(Path);
            _isExpanded = isRoot;
            IsDirectory = isDirectory;
            HasChildren = isDirectory;

            if (!isDirectory)
            {
                var info = new FileInfo(path);

                try
                {
                    Size     = info.Length;
                    Modified = info.LastWriteTimeUtc;
                }
                catch (IOException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }
            }
        }

        #endregion

        #region Fields

        private string                                   _path;

        private string                                   _name;

        private string?                                  _undoName;

        private long?                                    _size;

        private DateTimeOffset?                          _modified;

        private FileSystemWatcher?                       _watcher;

        private bool                                     _disposed;

        private ObservableCollection<FileTreeNodeModel>? _children;

        private bool                                     _hasChildren = true;

        private bool                                     _isExpanded;

        #endregion

        #region Properties

        public string Path
        {
            get => _path;
            private set => this.RaiseAndSetIfChanged(ref _path, value);
        }

        public string Name
        {
            get => _name;
            private set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        public long? Size
        {
            get => _size;
            private set => this.RaiseAndSetIfChanged(ref _size, value);
        }

        public DateTimeOffset? Modified
        {
            get => _modified;
            private set => this.RaiseAndSetIfChanged(ref _modified, value);
        }

        public bool HasChildren
        {
            get => _hasChildren;
            private set => this.RaiseAndSetIfChanged(ref _hasChildren, value);
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set => this.RaiseAndSetIfChanged(ref _isExpanded, value);
        }

        public bool IsChecked { get; set; }

        public bool IsDirectory { get; }

        public IReadOnlyList<FileTreeNodeModel> Children => _children ??= LoadChildren();

        #endregion

        #region Public

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            if (_watcher is { } watcher)
            {
                watcher.EnableRaisingEvents = false;
                watcher.Changed             -= OnChanged;
                watcher.Created             -= OnCreated;
                watcher.Deleted             -= OnDeleted;
                watcher.Renamed             -= OnRenamed;
                watcher.Dispose();
                _watcher = null;
            }

            if (_children is not null)
            {
                foreach (var child in _children)
                {
                    child.Dispose();
                }

                _children.Clear();
            }
        }

        public static Comparison<FileTreeNodeModel?> SortAscending<T>(Func<FileTreeNodeModel, T> selector)
        {
            return (x, y) =>
            {
                if (x is null && y is null)
                {
                    return 0;
                }
                else if (x is null)
                {
                    return -1;
                }
                else if (y is null)
                {
                    return 1;
                }

                if (x.IsDirectory == y.IsDirectory)
                {
                    return Comparer<T>.Default.Compare(selector(x), selector(y));
                }
                else if (x.IsDirectory)
                {
                    return -1;
                }
                else
                {
                    return 1;
                }
            };
        }

        public static Comparison<FileTreeNodeModel?> SortDescending<T>(Func<FileTreeNodeModel, T> selector)
        {
            return (x, y) =>
            {
                if (x is null && y is null)
                {
                    return 0;
                }
                else if (x is null)
                {
                    return 1;
                }
                else if (y is null)
                {
                    return -1;
                }

                if (x.IsDirectory == y.IsDirectory)
                {
                    return Comparer<T>.Default.Compare(selector(y), selector(x));
                }
                else if (x.IsDirectory)
                {
                    return -1;
                }
                else
                {
                    return 1;
                }
            };
        }

        #endregion

        #region Private

        private ObservableCollection<FileTreeNodeModel> LoadChildren()
        {
            var result = new ObservableCollection<FileTreeNodeModel>();

            if (!IsDirectory || _disposed)
            {
                return result;
            }

            try
            {
                var options = new EnumerationOptions { IgnoreInaccessible = true };

                foreach (var directory in Directory.EnumerateDirectories(Path, "*", options))
                {
                    result.Add(new FileTreeNodeModel(directory, true));
                }

                foreach (var file in Directory.EnumerateFiles(Path, "*", options))
                {
                    result.Add(new FileTreeNodeModel(file, false));
                }

                _watcher = new FileSystemWatcher(Path)
                {
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.Size | NotifyFilters.LastWrite,
                };
                _watcher.Changed             += OnChanged;
                _watcher.Created             += OnCreated;
                _watcher.Deleted             += OnDeleted;
                _watcher.Renamed             += OnRenamed;
                _watcher.EnableRaisingEvents = true;
            }
            catch (IOException exception)
            {
                System.Diagnostics.Trace.WriteLine(exception.Message);
            }
            catch (UnauthorizedAccessException exception)
            {
                System.Diagnostics.Trace.WriteLine(exception.Message);
            }

            HasChildren = result.Count > 0;

            return result;
        }

        private void PostFileChange(Action action)
        {
            if (_disposed)
            {
                return;
            }

            Dispatcher.UIThread.Post(() =>
            {
                if (_disposed)
                {
                    return;
                }

                try
                {
                    action();
                    HasChildren = _children?.Count > 0;
                }
                catch (IOException exception)
                {
                    System.Diagnostics.Trace.WriteLine(exception.Message);
                }
                catch (UnauthorizedAccessException exception)
                {
                    System.Diagnostics.Trace.WriteLine(exception.Message);
                }
            });
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Changed && File.Exists(e.FullPath))
            {
                PostFileChange(() =>
                {
                    foreach (var child in _children!)
                    {
                        if (child.Path == e.FullPath)
                        {
                            if (!child.IsDirectory)
                            {
                                var info = new FileInfo(e.FullPath);

                                child.Size     = info.Length;
                                child.Modified = info.LastWriteTimeUtc;
                            }

                            break;
                        }
                    }
                });
            }
        }

        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            PostFileChange(() =>
            {
                var node = new FileTreeNodeModel(
                    e.FullPath,
                    File.GetAttributes(e.FullPath).HasFlag(FileAttributes.Directory));

                if (!_children!.Any(x => string.Equals(x.Path, node.Path, StringComparison.OrdinalIgnoreCase)))
                {
                    _children!.Add(node);
                }
                else
                {
                    node.Dispose();
                }
            });
        }

        private void OnDeleted(object sender, FileSystemEventArgs e)
        {
            PostFileChange(() =>
            {
                for (var i = 0; i < _children!.Count; ++i)
                {
                    if (_children[i].Path == e.FullPath)
                    {
                        _children[i].Dispose();
                        _children.RemoveAt(i);
                        System.Diagnostics.Debug.WriteLine($"Removed {e.FullPath}");
                        break;
                    }
                }
            });
        }

        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            PostFileChange(() =>
            {
                for (var i = 0; i < _children!.Count; ++i)
                {
                    var child = _children[i];

                    if (child.Path == e.OldFullPath)
                    {
                        var replacement = new FileTreeNodeModel(e.FullPath, child.IsDirectory)
                        {
                            IsExpanded = child.IsExpanded,
                            IsChecked  = child.IsChecked,
                        };

                        _children[i] = replacement;
                        child.Dispose();
                        break;
                    }
                }
            });
        }

        #endregion

        #region IEditableObject

        void IEditableObject.BeginEdit() => _undoName = _name;

        void IEditableObject.CancelEdit() => _name = _undoName!;

        void IEditableObject.EndEdit() => _undoName = null;

        #endregion
    }
}
