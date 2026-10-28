using Avalonia;
using Avalonia.Data;
using BlackSharp.UI.Avalonia.TreeDataGrid.Experimental.Data;
using BlackSharp.UI.Avalonia.TreeDataGrid.Models;
using System.Collections;
using System.ComponentModel;

namespace BlackSharp.UI.Avalonia.TreeDataGrid
{
    internal class TreeDataGridColumnAdapter<TModel> : ColumnBase<TModel>,
        ITextSearchableColumn<TModel>,
        IDisposable
        where TModel : class
    {
        #region Constructor

        public TreeDataGridColumnAdapter(TreeDataGridColumn column, Action<bool>? invalidateLayout = null, Action? rebuildRows = null)
            : base(column.Header, column.Width, new ColumnOptions<TModel>())
        {
            Column                 = column;
            _invalidateLayout      = invalidateLayout;
            _rebuildRows           = rebuildRows;

            Column.PropertyChanged += OnColumnPropertyChanged;
            PropertyChanged        += OnAdapterPropertyChanged;

            SubscribeToInner();
            UpdateOptions();
        }

        #endregion

        #region Fields

        private readonly Action<bool>? _invalidateLayout;

        private readonly Action?       _rebuildRows;

        private TreeDataGridColumn?    _innerColumn;

        #endregion

        #region Properties

        internal TreeDataGridColumn Column { get; }

        public bool IsTextSearchEnabled => Column is TreeDataGridTextColumn { IsTextSearchEnabled: true } or TreeDataGridTemplateColumn { TextSearchBinding: not null } or TreeDataGridHierarchicalExpanderColumn { Inner: TreeDataGridTextColumn { IsTextSearchEnabled: true } };

        #endregion

        #region Public

        public string? SelectValue(TModel model)
        {
            if (Column is TreeDataGridTemplateColumn { TextSearchBinding: { } searchBinding })
            {
                using var binding = new TreeDataGridBindingValue<object?>(model, searchBinding);

                return binding.Value?.ToString();
            }

            return Column.GetCellValue(model)?.ToString();
        }

        public override ICell CreateCell(IRow<TModel> row) => CreateCell(Column, row);

        public override Comparison<TModel?>? GetComparison(ListSortDirection direction)
        {
            var comparison = direction == ListSortDirection.Ascending ? Column.CompareAscending : Column.CompareDescending;

            if (comparison is not null)
            {
                return (x, y) => comparison(x, y);
            }

            var other = direction == ListSortDirection.Ascending ? Column.CompareDescending : Column.CompareAscending;

            if (other is not null)
            {
                return (x, y) => other(y, x);
            }

            if (Column is TreeDataGridTemplateColumn or TreeDataGridRowHeaderColumn)
            {
                return null;
            }

            return (x, y) => direction == ListSortDirection.Ascending ? Compare(x, y) : Compare(y, x);
        }

        public void Dispose()
        {
            Column.PropertyChanged -= OnColumnPropertyChanged;

            if (_innerColumn is not null)
            {
                _innerColumn.PropertyChanged -= OnColumnPropertyChanged;
            }

            PropertyChanged -= OnAdapterPropertyChanged;
        }

        #endregion

        #region Protected

        protected static ICell CreateCell(TreeDataGridColumn column, IRow<TModel> row)
        {
            switch (column)
            {
                case TreeDataGridTextColumn text:
                    var textBinding = new TreeDataGridBindingValue<object?>(row.Model!, text.Binding);
                    var options     = new TextColumnOptions<TModel>
                    {
                        BeginEditGestures = text.BeginEditGestures,
                        TextAlignment     = text.TextAlignment,
                        TextTrimming      = text.TextTrimming,
                        TextWrapping      = text.TextWrapping,
                        StringFormat      = text.StringFormat ?? "{0}",
                        Culture           = text.Culture ?? System.Globalization.CultureInfo.CurrentCulture,
                    };

                    return new TextCell<object?>(textBinding, textBinding, ReadOnly(text.IsReadOnly), options) { Column = text };

                case TreeDataGridCheckBoxColumn checkBox:
                    var checkBinding = new TreeDataGridBindingValue<bool?>(row.Model!, checkBox.Binding);

                    return new CheckBoxCell(checkBinding, checkBinding, ReadOnly(checkBox.IsReadOnly), checkBox.IsThreeState);

                case TreeDataGridTemplateColumn template:
                    return new TemplateCell(row.Model, template.GetCellTemplate,
                        template.HasEditingTemplate ? template.GetEditingTemplate : null,
                        ReadOnly(!template.HasEditingTemplate), new TemplateColumnOptions<TModel>
                        {
                            BeginEditGestures = template.BeginEditGestures,
                        });

                case TreeDataGridRowHeaderColumn:
                    return new TreeDataGridRowHeaderCellModel(row) { Column = column };

                default:
                    throw new InvalidOperationException("An expander column must contain a text, check box, template or row header column.");
            }
        }

        #endregion

        #region Private

        private static IObservable<BindingValue<bool>> ReadOnly(bool value) =>
            ObservableEx.SingleValue(new BindingValue<bool>(value));

        private int Compare(TModel? x, TModel? y)
        {
            if (x is null || y is null)
            {
                return x is null ? (y is null ? 0 : -1) : 1;
            }

            return Comparer.DefaultInvariant.Compare(Column.GetCellValue(x), Column.GetCellValue(y));
        }

        private void OnColumnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            SubscribeToInner();
            UpdateOptions();

            if (e.Property == TreeDataGridHierarchicalExpanderColumn.ChildrenBindingProperty ||
                e.Property == TreeDataGridHierarchicalExpanderColumn.HasChildrenBindingProperty ||
                e.Property == TreeDataGridHierarchicalExpanderColumn.IsExpandedBindingProperty)
            {
                _rebuildRows?.Invoke();
            }

            var layoutOnly = e.Property == TreeDataGridColumn.ActualWidthProperty ||
                e.Property == TreeDataGridColumn.WidthProperty || e.Property == TreeDataGridColumn.MinWidthProperty ||
                e.Property == TreeDataGridColumn.MaxWidthProperty || e.Property == TreeDataGridColumn.SortDirectionProperty ||
                e.Property == TreeDataGridColumn.HeaderProperty || e.Property == TreeDataGridColumn.TagProperty;

            RaisePropertyChanged(e.Property.Name);
            _invalidateLayout?.Invoke(!layoutOnly);
        }

        private void OnAdapterPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ActualWidth))
            {
                Column.SetActualWidth(ActualWidth);
            }
            else if (e.PropertyName == nameof(Width))
            {
                Column.Width = Width;
            }
            else if (e.PropertyName == nameof(SortDirection))
            {
                Column.SortDirection = SortDirection;
            }
        }

        private void SubscribeToInner()
        {
            var inner = (Column as TreeDataGridHierarchicalExpanderColumn)?.Inner;

            if (!ReferenceEquals(inner, _innerColumn))
            {
                if (_innerColumn is not null)
                {
                    _innerColumn.PropertyChanged -= OnColumnPropertyChanged;
                }

                _innerColumn = inner;

                if (_innerColumn is not null)
                {
                    _innerColumn.PropertyChanged += OnColumnPropertyChanged;
                }
            }
        }

        private void UpdateOptions()
        {
            Header                      = Column.Header;
            SortDirection               = Column.SortDirection;
            Options.CanUserResizeColumn = Column.CanUserResize;
            Options.CanUserSortColumn   = Column.CanUserSort;
            Options.MinWidth            = Column.MinWidth;
            Options.MaxWidth            = Column.MaxWidth;
            IsVisible                   = Column.IsVisible;

            if (Width != Column.Width)
            {
                ((IUpdateColumnLayout)this).SetWidth(Column.Width);
            }
        }

        #endregion
    }

    internal sealed class TreeDataGridExpanderColumnAdapter<TModel> : TreeDataGridColumnAdapter<TModel>,
        IExpanderColumn<TModel>,
        ITreeDataGridRowBinding<TModel>
        where TModel : class
    {
        #region Constructor

        public TreeDataGridExpanderColumnAdapter(TreeDataGridHierarchicalExpanderColumn column, Action<bool>? invalidateLayout = null, Action? rebuildRows = null)
            : base(column, invalidateLayout, rebuildRows)
        {
            _column = column;
        }

        #endregion

        #region Fields

        private readonly TreeDataGridHierarchicalExpanderColumn _column;

        #endregion

        #region Public

        public override ICell CreateCell(IRow<TModel> row)
        {
            if (row is not IExpanderRow<TModel> expander || _column.Inner is not { } inner)
            {
                throw new InvalidOperationException("An expander column requires hierarchical rows and an inner column.");
            }

            return new ExpanderCell<TModel>(CreateCell(inner, row), expander,
                ObservableEx.SingleValue(HasChildren(row.Model!)), null);
        }

        public IDisposable Subscribe(HierarchicalRow<TModel> row) => new TreeDataGridRowBinding<TModel>(_column, row);

        public bool HasChildren(TModel model) => _column.HasChildren(model);

        public IEnumerable<TModel> GetChildModels(TModel model)
        {
            var children = _column.GetChildModels(model);

            return children as IEnumerable<TModel> ?? children.Cast<TModel>();
        }

        public void SetModelIsExpanded(IExpanderRow<TModel> row)
        {
            if (_column.IsExpandedBinding is null)
            {
                return;
            }

            using var binding = new TreeDataGridBindingValue<bool>(row.Model!, _column.IsExpandedBinding);

            binding.Value = row.IsExpanded;
        }

        #endregion
    }
}
