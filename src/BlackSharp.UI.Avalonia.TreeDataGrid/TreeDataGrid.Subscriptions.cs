using BlackSharp.UI.Avalonia.TreeDataGrid.Selection;
using System.Collections.Specialized;
using System.ComponentModel;

namespace BlackSharp.UI.Avalonia.TreeDataGrid
{
    public partial class TreeDataGrid
    {
        #region Fields

        private SourceSubscription?    _sourceSubscription;

        private SelectionSubscription? _interactionSubscription;

        private SelectionSubscription? _modernSelectionSubscription;

        private TreeDataGridRows?      _previousRows;

        private ColumnsSubscription?   _xamlColumnSubscription;

        #endregion

        #region Nested Types

        private sealed class ColumnsSubscription : IDisposable
        {
            #region Constructor

            internal ColumnsSubscription(TreeDataGrid owner, TreeDataGridColumns columns)
            {
                _owner                    = new WeakReference<TreeDataGrid>(owner);
                _columns                  = columns;
                columns.CollectionChanged += OnCollectionChanged;
            }

            #endregion

            #region Fields

            private readonly WeakReference<TreeDataGrid> _owner;
            private TreeDataGridColumns?                 _columns;

            #endregion

            #region Public

            public void Dispose()
            {
                if (_columns is { } columns)
                {
                    columns.CollectionChanged -= OnCollectionChanged;
                    _columns                  = null;
                }
            }

            #endregion

            #region Private

            private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
            {
                if (_owner.TryGetTarget(out var owner))
                {
                    owner.OnXamlColumnsChanged(sender, args);
                }
                else
                {
                    Dispose();
                }
            }

            #endregion
        }

        private sealed class SourceSubscription : IDisposable
        {
            #region Constructor

            internal SourceSubscription(TreeDataGrid owner, ITreeDataGridSource source)
            {
                _owner                 = new WeakReference<TreeDataGrid>(owner);
                _source                = source;
                source.PropertyChanged += OnPropertyChanged;
                source.Sorted          += OnSorted;
            }

            #endregion

            #region Fields

            private readonly WeakReference<TreeDataGrid> _owner;

            private ITreeDataGridSource?                 _source;

            #endregion

            #region Public

            public void Dispose()
            {
                if (_source is { } source)
                {
                    source.PropertyChanged -= OnPropertyChanged;
                    source.Sorted          -= OnSorted;
                    _source                = null;
                }
            }

            #endregion

            #region Private

            private void OnPropertyChanged(object? sender, PropertyChangedEventArgs args)
            {
                if (_owner.TryGetTarget(out var owner))
                {
                    owner.OnSourcePropertyChanged(sender, args);
                }
                else
                {
                    Dispose();
                }
            }

            private void OnSorted()
            {
                if (_owner.TryGetTarget(out var owner))
                {
                    owner.OnSourceSorted();
                }
                else
                {
                    Dispose();
                }
            }

            #endregion
        }

        private sealed class SelectionSubscription : IDisposable
        {
            #region Constructor

            internal SelectionSubscription(TreeDataGrid owner, IEngineSelectionInteraction selection)
            {
                _owner                     = new WeakReference<TreeDataGrid>(owner);
                _interaction               = selection;
                selection.SelectionChanged += OnInteractionChanged;
            }

            internal SelectionSubscription(TreeDataGrid owner, TreeDataGridSelectionModel selection)
            {
                _owner                     = new WeakReference<TreeDataGrid>(owner);
                _selection                 = selection;
                selection.SelectionChanged += OnSelectionChanged;
                selection.IndexesChanged   += OnIndexesChanged;
            }

            #endregion

            #region Fields

            private readonly WeakReference<TreeDataGrid> _owner;

            private IEngineSelectionInteraction?         _interaction;

            private TreeDataGridSelectionModel?          _selection;

            #endregion

            #region Public

            public void Dispose()
            {
                if (_interaction is { } interaction)
                {
                    interaction.SelectionChanged -= OnInteractionChanged;
                    _interaction                 = null;
                }

                if (_selection is { } selection)
                {
                    selection.SelectionChanged -= OnSelectionChanged;
                    selection.IndexesChanged   -= OnIndexesChanged;
                    _selection                 = null;
                }
            }

            #endregion

            #region Private

            private void OnInteractionChanged(object? sender, EventArgs args)
            {
                if (_owner.TryGetTarget(out var owner))
                {
                    owner.OnSelectionInteractionChanged(sender, args);
                }
                else
                {
                    Dispose();
                }
            }

            private void OnIndexesChanged(object? sender, TreeDataGridIndexesChangedEventArgs args)
            {
                if (_owner.TryGetTarget(out var owner))
                {
                    owner.UpdateSelectedProperties();
                }
                else
                {
                    Dispose();
                }
            }

            private void OnSelectionChanged(object? sender, TreeDataGridSelectionChangedEventArgs args)
            {
                if (_owner.TryGetTarget(out var owner))
                {
                    owner.OnModernSelectionChanged(sender, args);
                }
                else
                {
                    Dispose();
                }
            }

            #endregion
        }

        #endregion
    }
}
