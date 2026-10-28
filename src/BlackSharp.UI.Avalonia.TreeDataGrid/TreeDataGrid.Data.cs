using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Metadata;
using Avalonia.Styling;
using Avalonia.VisualTree;
using BlackSharp.UI.Avalonia.TreeDataGrid.Models;
using BlackSharp.UI.Avalonia.TreeDataGrid.Primitives;
using BlackSharp.UI.Avalonia.TreeDataGrid.Selection;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;

namespace BlackSharp.UI.Avalonia.TreeDataGrid
{
    public partial class TreeDataGrid
    {
        #region Fields

        public static readonly StyledProperty<bool>                              CanUserReorderColumnsProperty =
            AvaloniaProperty.Register<TreeDataGrid, bool>(nameof(CanUserReorderColumns), false);

        public static readonly StyledProperty<bool>                              AllowTriStateSortingProperty  =
            AvaloniaProperty.Register<TreeDataGrid, bool>(nameof(AllowTriStateSorting), false);

        public static readonly StyledProperty<int>                               FrozenColumnCountProperty     =
            AvaloniaProperty.Register<TreeDataGrid, int>(nameof(FrozenColumnCount), 0);

        public static readonly StyledProperty<TreeDataGridGridLinesVisibility>   GridLinesVisibilityProperty   =
            AvaloniaProperty.Register<TreeDataGrid, TreeDataGridGridLinesVisibility>(nameof(GridLinesVisibility), TreeDataGridGridLinesVisibility.None);

        public static readonly StyledProperty<IEnumerable?>                      ItemsSourceProperty           =
            AvaloniaProperty.Register<TreeDataGrid, IEnumerable?>(nameof(ItemsSource), null);

        public static readonly StyledProperty<TreeDataGridSelectionMode>         SelectionModeProperty         =
            AvaloniaProperty.Register<TreeDataGrid, TreeDataGridSelectionMode>(nameof(SelectionMode), TreeDataGridSelectionMode.Row);

        public static readonly StyledProperty<ControlTheme?>                     RowThemeProperty              =
            AvaloniaProperty.Register<TreeDataGrid, ControlTheme?>(nameof(RowTheme), null);

        public static readonly StyledProperty<ControlTheme?>                     ColumnHeaderThemeProperty     =
            AvaloniaProperty.Register<TreeDataGrid, ControlTheme?>(nameof(ColumnHeaderTheme), null);

        public static readonly DirectProperty<TreeDataGrid, TreeDataGridSource?> SourceProperty                =
            AvaloniaProperty.RegisterDirect<TreeDataGrid, TreeDataGridSource?>(nameof(Source), x => x.Source, (x, value) => x.Source = value);

        public static readonly DirectProperty<TreeDataGrid, TreeDataGridColumns> ColumnsProperty               =
            AvaloniaProperty.RegisterDirect<TreeDataGrid, TreeDataGridColumns>(nameof(Columns), x => x.Columns);

        public static readonly DirectProperty<TreeDataGrid, TreeDataGridRows?>   RowsProperty                  =
            AvaloniaProperty.RegisterDirect<TreeDataGrid, TreeDataGridRows?>(nameof(Rows), x => x.Rows);

        public static readonly DirectProperty<TreeDataGrid, IndexPath>           SelectedIndexProperty         =
            AvaloniaProperty.RegisterDirect<TreeDataGrid, IndexPath>(nameof(SelectedIndex), x => x.SelectedIndex, (x, value) => x.SelectedIndex = value,
                defaultBindingMode: BindingMode.TwoWay);

        public static readonly DirectProperty<TreeDataGrid, object?>             SelectedItemProperty          =
            AvaloniaProperty.RegisterDirect<TreeDataGrid, object?>(nameof(SelectedItem), x => x.SelectedItem, (x, value) => x.SelectedItem = value,
                defaultBindingMode: BindingMode.TwoWay);

        private TreeDataGridColumns?               _xamlColumns;

        private TreeDataGridSource?                _ownedSource;

        private TreeDataGridSelectionModel?        _observedSelection;

        private IndexPath                          _selectedIndex;

        private object?                            _selectedItem;

        private bool                               _updatingItemsSource;

        private bool                               _applyingSourceDefaults;

        private readonly HashSet<AvaloniaProperty> _sourceDefaults         = [];

        #endregion

        #region Properties

        public bool CanUserReorderColumns
        {
            get => GetValue(CanUserReorderColumnsProperty);
            set => SetValue(CanUserReorderColumnsProperty, value);
        }

        public bool AllowTriStateSorting
        {
            get => GetValue(AllowTriStateSortingProperty);
            set => SetValue(AllowTriStateSortingProperty, value);
        }

        public int FrozenColumnCount
        {
            get => GetValue(FrozenColumnCountProperty);
            set => SetValue(FrozenColumnCountProperty, value);
        }

        public TreeDataGridGridLinesVisibility GridLinesVisibility
        {
            get => GetValue(GridLinesVisibilityProperty);
            set => SetValue(GridLinesVisibilityProperty, value);
        }

        public IEnumerable? ItemsSource
        {
            get => GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public TreeDataGridSelectionMode SelectionMode
        {
            get => GetValue(SelectionModeProperty);
            set => SetValue(SelectionModeProperty, value);
        }

        public ControlTheme? RowTheme
        {
            get => GetValue(RowThemeProperty);
            set => SetValue(RowThemeProperty, value);
        }

        public ControlTheme? ColumnHeaderTheme
        {
            get => GetValue(ColumnHeaderThemeProperty);
            set => SetValue(ColumnHeaderThemeProperty, value);
        }

        public TreeDataGridSource? Source
        {
            get => CoreSource as TreeDataGridSource;
            set => CoreSource = value;
        }

        [Content]
        public TreeDataGridColumns Columns
        {
            get
            {
                if (Source is { } source)
                {
                    return source.Columns;
                }

                if (_xamlColumns is null)
                {
                    _xamlColumns            = [];
                    _xamlColumnSubscription = new ColumnsSubscription(this, _xamlColumns);
                }

                return _xamlColumns;
            }
        }

        public TreeDataGridRows? Rows => Source is { IsDisposed: false } source ? source.Rows : null;

        public TreeDataGridSelectionModel? Selection => Source?.Selection;

        public IndexPath SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (Selection is { } selection)
                {
                    selection.SelectedRowIndex = value;
                }
                else
                {
                    SetAndRaise(SelectedIndexProperty, ref _selectedIndex, value);
                }
            }
        }

        public object? SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (Selection is { } selection && Source is { } source)
                {
                    selection.SelectedRowIndex = value is null ? default : FindModel(source.Items, value, default, new HashSet<object>(ReferenceEqualityComparer.Instance));
                }
                else
                {
                    SetAndRaise(SelectedItemProperty, ref _selectedItem, value);
                }
            }
        }

        #endregion

        #region Public

        public bool BeginEdit(int columnIndex, int rowIndex)
        {
            if (TryGetCell(columnIndex, rowIndex) is TreeDataGridCell { Model.CanEdit: true } cell)
            {
                CommitEdit();

                if (this.GetVisualDescendants().OfType<TreeDataGridCell>().Any(x => x.IsEditing))
                {
                    return false;
                }

                cell.BeginEdit();

                return cell.IsEditing;
            }

            return false;
        }

        public bool CommitEdit()
        {
            var editing = this.GetVisualDescendants().OfType<TreeDataGridCell>().Where(x => x.IsEditing).ToArray();

            foreach (var cell in editing)
            {
                cell.EndEdit();
            }

            return editing.Length > 0 && editing.All(x => !x.IsEditing);
        }

        public bool CancelEdit()
        {
            var editing = this.GetVisualDescendants().OfType<TreeDataGridCell>().Where(x => x.IsEditing).ToArray();

            foreach (var cell in editing)
            {
                cell.CancelEdit();
            }

            return editing.Length > 0;
        }

        public Control? BringRowIntoView(IndexPath index, Rect? rect = null) =>
            Source is { } source ? RowsPresenter?.BringIntoView(source.ModelIndexToRowIndex(index), rect) : null;

        #endregion

        #region Internal

        internal void MoveToNextEditableCell(int columnIndex, int rowIndex, bool reverse)
        {
            if (Source is not { } source || source.Columns.Count == 0)
            {
                return;
            }

            var step  = reverse ? -1 : 1;
            var index = rowIndex * source.Columns.Count + columnIndex + step;
            var count = source.Rows.Count * source.Columns.Count;

            for (; index >= 0 && index < count; index += step)
            {
                var column = index % source.Columns.Count;
                var row    = index / source.Columns.Count;

                if (!source.Columns[column].IsVisible)
                {
                    continue;
                }

                RowsPresenter?.BringIntoView(row);
                TryGetRow(row)?.CellsPresenter?.BringIntoView(column);

                if (TryGetCell(column, row) is TreeDataGridCell { Model.CanEdit: true } cell)
                {
                    cell.Focus();
                    cell.BeginEdit();

                    return;
                }
            }
        }

        #endregion

        #region Private

        private IndexPath FindModel(IEnumerable items, object model, IndexPath parent, HashSet<object> visited)
        {
            var index = 0;

            foreach (var item in items)
            {
                var path = parent.Append(index++);

                if (ReferenceEquals(item, model) || Equals(item, model))
                {
                    return path;
                }

                if (item is not null && visited.Add(item) && CoreSource?.IsHierarchical == true &&
                    CoreSource.GetModelChildren(item) is { } children)
                {
                    var result = FindModel(children, model, path, visited);

                    if (result.Count > 0)
                    {
                        return result;
                    }
                }
            }

            return default;
        }

        private void OnCoreSourceChanging(ITreeDataGridSource? oldSource, ITreeDataGridSource? newSource)
        {
            CancelEdit();
            _previousRows = CoreRows as TreeDataGridRows;

            if (_observedSelection is not null)
            {
                _modernSelectionSubscription?.Dispose();
                _modernSelectionSubscription = null;
                _observedSelection           = null;
            }
        }

        private void OnCoreSourceChanged(ITreeDataGridSource? oldSource, ITreeDataGridSource? newSource)
        {
            RaisePropertyChanged(SourceProperty, oldSource as TreeDataGridSource, newSource as TreeDataGridSource);
            RaisePropertyChanged(ColumnsProperty, (oldSource as TreeDataGridSource)?.Columns ?? _xamlColumns!, Columns);
            RaisePropertyChanged(RowsProperty, _previousRows, Rows);
            ApplySourceDefaults();
            ObserveSelection();
        }

        private void ApplySourceDefaults()
        {
            if (Source is not { } source)
            {
                return;
            }

            ApplySourceDefault(AutoDragDropRowsProperty, source.AutoDragDropRows);
            ApplySourceDefault(CanUserReorderColumnsProperty, source.CanUserReorderColumns);
            ApplySourceDefault(CanUserResizeColumnsProperty, source.CanUserResizeColumns);
            ApplySourceDefault(CanUserSortColumnsProperty, source.CanUserSortColumns);
            ApplySourceDefault(AllowTriStateSortingProperty, source.AllowTriStateSorting);
            ApplySourceDefault(FrozenColumnCountProperty, source.FrozenColumnCount);
            ApplySourceDefault(ShowColumnHeadersProperty, source.ShowColumnHeaders);
        }

        private void ApplySourceDefault<T>(StyledProperty<T> property, T value)
        {
            if (!IsSet(property) || _sourceDefaults.Contains(property))
            {
                _applyingSourceDefaults = true;

                try
                {
                    SetCurrentValue(property, value);
                    _sourceDefaults.Add(property);
                }
                finally
                {
                    _applyingSourceDefaults = false;
                }
            }
        }

        private void ObserveSelection()
        {
            if (!ReferenceEquals(_observedSelection, Selection))
            {
                if (_observedSelection is not null)
                {
                    _modernSelectionSubscription?.Dispose();
                    _modernSelectionSubscription = null;
                }

                _observedSelection = Selection;

                if (_observedSelection is not null)
                {
                    _modernSelectionSubscription = new SelectionSubscription(this, _observedSelection);
                }
            }

            if (IsSet(SelectionModeProperty) && Source is { } source)
            {
                var cell = SelectionMode.HasFlag(TreeDataGridSelectionMode.Cell);

                if ((SelectionMode & (TreeDataGridSelectionMode.Cell | TreeDataGridSelectionMode.Row)) == 0)
                {
                    source.Selection = null;
                }
                else if (source.Selection is null || source.Selection.IsCellSelectionModel != cell)
                {
                    source.Selection = source.CreateSelection(cell);
                }

                if (source.Selection is { } selection)
                {
                    selection.SingleSelect = !SelectionMode.HasFlag(TreeDataGridSelectionMode.Multiple);
                }
            }

            UpdateSelectedProperties();
        }

        private void UpdateSelectedProperties()
        {
            SetAndRaise(SelectedIndexProperty, ref _selectedIndex, Selection?.SelectedRowIndex ?? default);
            SetAndRaise(SelectedItemProperty, ref _selectedItem, Selection?.SelectedRowItem);
        }

        private void OnModernSelectionChanged(object? sender, TreeDataGridSelectionChangedEventArgs e)
        {
            UpdateSelectedProperties();
            SelectionChanged?.Invoke(this, e);
        }

        private void OnModernSourcePropertyChanged(PropertyChangedEventArgs e)
        {
            if (Source is { } source)
            {
                switch (e.PropertyName)
                {
                    case nameof(TreeDataGridSource.AutoDragDropRows):
                        ApplySourceDefault(AutoDragDropRowsProperty, source.AutoDragDropRows);
                        break;
                    case nameof(TreeDataGridSource.CanUserReorderColumns):
                        ApplySourceDefault(CanUserReorderColumnsProperty, source.CanUserReorderColumns);
                        break;
                    case nameof(TreeDataGridSource.CanUserResizeColumns):
                        ApplySourceDefault(CanUserResizeColumnsProperty, source.CanUserResizeColumns);
                        break;
                    case nameof(TreeDataGridSource.CanUserSortColumns):
                        ApplySourceDefault(CanUserSortColumnsProperty, source.CanUserSortColumns);
                        break;
                    case nameof(TreeDataGridSource.AllowTriStateSorting):
                        ApplySourceDefault(AllowTriStateSortingProperty, source.AllowTriStateSorting);
                        break;
                    case nameof(TreeDataGridSource.FrozenColumnCount):
                        ApplySourceDefault(FrozenColumnCountProperty, source.FrozenColumnCount);
                        break;
                    case nameof(TreeDataGridSource.ShowColumnHeaders):
                        ApplySourceDefault(ShowColumnHeadersProperty, source.ShowColumnHeaders);
                        break;
                }
            }

            if (e.PropertyName == nameof(TreeDataGridSource.Rows))
            {
                var previous = CoreRows as TreeDataGridRows;

                CoreRows = (IRows?)Rows;
                RaisePropertyChanged(RowsProperty, previous, Rows);
            }

            if (e.PropertyName == nameof(TreeDataGridSource.Selection))
            {
                ObserveSelection();
            }
        }

        private void OnXamlColumnsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (ItemsSource is not null && !_updatingItemsSource &&
                (_ownedSource is null || _ownedSource.IsHierarchical != (_xamlColumns?.Any(x => x is TreeDataGridHierarchicalExpanderColumn) ?? false)))
            {
                RebuildItemsSource();
            }
        }

        private void RebuildItemsSource()
        {
            _updatingItemsSource = true;

            try
            {
                var columns = _xamlColumns ?? Columns;
                var items   = ItemsSource as IEnumerable<object> ?? ItemsSource?.Cast<object>() ?? [];
                var old     = _ownedSource;

                CoreSource = null;
                (old as IDisposable)?.Dispose();

                if (ItemsSource is null)
                {
                    _ownedSource = null;

                    return;
                }

                _ownedSource = columns.Any(x => x is TreeDataGridHierarchicalExpanderColumn) ?
                    new HierarchicalTreeDataGridSource<object>(items, columns) : new FlatTreeDataGridSource<object>(items, columns);
                Source = _ownedSource;
            }
            finally
            {
                _updatingItemsSource = false;
            }
        }

        private void OnModernPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            if (!_applyingSourceDefaults)
            {
                _sourceDefaults.Remove(change.Property);
            }

            if (change.Property == ItemsSourceProperty)
            {
                RebuildItemsSource();
            }
            else if (change.Property == SelectionModeProperty)
            {
                ObserveSelection();
            }
            else if (change.Property == GridLinesVisibilityProperty)
            {
                foreach (var cell in this.GetVisualDescendants().OfType<TreeDataGridCell>())
                {
                    cell.UpdateColumnPresentation();
                }
            }
            else if (change.Property == FrozenColumnCountProperty)
            {
                InvalidateMeasure();
                RowsPresenter?.InvalidateMeasure();
                ColumnHeadersPresenter?.InvalidateMeasure();
            }
        }

        private bool HandleModernKeyDown(KeyEventArgs e)
        {
            if (e.Handled || Selection is not { } selection)
            {
                return false;
            }

            if (e.Key == Key.A && e.KeyModifiers.HasFlag(KeyModifiers.Control) && !selection.SingleSelect)
            {
                selection.SelectAllCore();
                e.Handled = true;

                return true;
            }

            return false;
        }

        #endregion

        #region Events

        public event EventHandler<TreeDataGridSelectionChangedEventArgs>? SelectionChanged;

        #endregion
    }
}
