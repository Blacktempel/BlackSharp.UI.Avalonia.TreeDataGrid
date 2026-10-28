using Avalonia;
using Avalonia.Data;
using System.Collections;

namespace BlackSharp.UI.Avalonia.TreeDataGrid
{
    public sealed class TreeDataGridHierarchicalExpanderColumn : TreeDataGridColumn
    {
        #region Fields

        public static readonly StyledProperty<BindingBase?>        ChildrenBindingProperty    =
            AvaloniaProperty.Register<TreeDataGridHierarchicalExpanderColumn, BindingBase?>(nameof(ChildrenBinding));

        public static readonly StyledProperty<BindingBase?>        HasChildrenBindingProperty =
            AvaloniaProperty.Register<TreeDataGridHierarchicalExpanderColumn, BindingBase?>(nameof(HasChildrenBinding));

        public static readonly StyledProperty<BindingBase?>        IsExpandedBindingProperty  =
            AvaloniaProperty.Register<TreeDataGridHierarchicalExpanderColumn, BindingBase?>(nameof(IsExpandedBinding));

        public static readonly StyledProperty<TreeDataGridColumn?> InnerProperty              =
            AvaloniaProperty.Register<TreeDataGridHierarchicalExpanderColumn, TreeDataGridColumn?>(nameof(Inner));

        #endregion

        #region Properties

        [AssignBinding]
        public BindingBase? ChildrenBinding
        {
            get => GetValue(ChildrenBindingProperty);
            set => SetValue(ChildrenBindingProperty, value);
        }

        [AssignBinding]
        public BindingBase? HasChildrenBinding
        {
            get => GetValue(HasChildrenBindingProperty);
            set => SetValue(HasChildrenBindingProperty, value);
        }

        [AssignBinding]
        public BindingBase? IsExpandedBinding
        {
            get => GetValue(IsExpandedBindingProperty);
            set => SetValue(IsExpandedBindingProperty, value);
        }

        public TreeDataGridColumn? Inner
        {
            get => GetValue(InnerProperty);
            set => SetValue(InnerProperty, value);
        }

        internal Func<object, IEnumerable?>? ChildrenSelector { get; set; }

        internal Func<object, bool>? HasChildrenSelector { get; set; }

        internal Func<object, bool>? IsExpandedSelector { get; set; }

        #endregion

        #region Public

        public IEnumerable GetChildModels(object model)
        {
            if (ChildrenSelector is { } selector)
            {
                try
                {
                    return selector(model) ?? Array.Empty<object>();
                }
                catch (Exception)
                {
                    // Retain the binding engine's handling of getter errors instead of propagating them.
                }
            }

            using var binding = new TreeDataGridBindingValue<IEnumerable?>(model, ChildrenBinding);

            return binding.Value ?? Array.Empty<object>();
        }

        public bool GetModelIsExpanded(object model)
        {
            if (IsExpandedSelector is { } selector)
            {
                try
                {
                    return selector(model);
                }
                catch (Exception)
                {
                    // Fall back to the binding engine's unset/error handling.
                }
            }

            using var binding = new TreeDataGridBindingValue<bool>(model, IsExpandedBinding);

            return binding.Value;
        }

        public bool HasChildren(object model)
        {
            if (HasChildrenBinding is not null)
            {
                if (HasChildrenSelector is { } selector)
                {
                    try
                    {
                        return selector(model);
                    }
                    catch (Exception)
                    {
                        // Fall back to the binding engine's unset/error handling.
                    }
                }

                using var binding = new TreeDataGridBindingValue<bool>(model, HasChildrenBinding);

                return binding.Value;
            }

            return HasAnyChildren(GetChildModels(model));
        }

        #endregion

        #region Internal

        internal static bool HasAnyChildren(IEnumerable? children)
        {
            if (children is ICollection collection)
            {
                return collection.Count > 0;
            }

            if (children is System.Collections.Generic.IReadOnlyCollection<object> readOnly)
            {
                return readOnly.Count > 0;
            }

            var enumerator = children?.GetEnumerator();

            try
            {
                return enumerator?.MoveNext() ?? false;
            }
            finally
            {
                (enumerator as IDisposable)?.Dispose();
            }
        }

        internal override object? GetCellValue(object model) => Inner?.GetCellValue(model);

        #endregion

        #region Protected

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            if (change.Property == ChildrenBindingProperty)
            {
                ChildrenSelector = null;
            }
            else if (change.Property == HasChildrenBindingProperty)
            {
                HasChildrenSelector = null;
            }
            else if (change.Property == IsExpandedBindingProperty)
            {
                IsExpandedSelector = null;
            }

            base.OnPropertyChanged(change);
        }

        #endregion
    }
}
