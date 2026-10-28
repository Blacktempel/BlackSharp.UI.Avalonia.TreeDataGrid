using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace BlackSharp.UI.Avalonia.TreeDataGrid
{
    public static class TreeDataGridSourceExtensions
    {
        #region Public

        public static FlatTreeDataGridSource<TModel> WithColumn<TModel>(this FlatTreeDataGridSource<TModel> source,
            TreeDataGridColumn column)
            where TModel : class
        {
            source.Columns.Add(column);

            return source;
        }

        public static FlatTreeDataGridSource<TModel> WithTextColumn<TModel, TValue>(this FlatTreeDataGridSource<TModel> source,
            Expression<Func<TModel, TValue>> expression, Action<TextColumnCreateOptions>? options = null)
            where TModel : class
        {
            return WithTextColumn(source, GetHeader(expression), expression, options);
        }

        public static FlatTreeDataGridSource<TModel> WithTextColumn<TModel, TValue>(this FlatTreeDataGridSource<TModel> source,
            object? header, Expression<Func<TModel, TValue>> expression, Action<TextColumnCreateOptions>? options = null)
            where TModel : class
        {
            return source.WithColumn(CreateTextColumn(header, expression, options));
        }

        public static FlatTreeDataGridSource<TModel> WithCheckBoxColumn<TModel>(this FlatTreeDataGridSource<TModel> source,
            Expression<Func<TModel, bool>> expression, Action<CheckBoxColumnCreateOptions>? options = null)
            where TModel : class
        {
            return WithCheckBoxColumn(source, null, expression, options);
        }

        public static FlatTreeDataGridSource<TModel> WithCheckBoxColumn<TModel>(this FlatTreeDataGridSource<TModel> source,
            object? header, Expression<Func<TModel, bool>> expression, Action<CheckBoxColumnCreateOptions>? options = null)
            where TModel : class
        {
            var settings = new CheckBoxColumnCreateOptions();

            options?.Invoke(settings);
            settings.IsReadOnly |= !CanWrite(expression.Body);
            var getter          = expression.Compile(preferInterpretation: !RuntimeFeature.IsDynamicCodeSupported);
            var column          = new TreeDataGridCheckBoxColumn
            {
                Header        = header,
                Binding       = CompiledBinding.Create(expression, mode: settings.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay),
                IsReadOnly    = settings.IsReadOnly,
                IsThreeState  = false,
                ValueSelector = model => getter((TModel)model),
            };

            Apply(column, settings);

            return source.WithColumn(column);
        }

        public static FlatTreeDataGridSource<TModel> WithThreeStateCheckBoxColumn<TModel>(this FlatTreeDataGridSource<TModel> source,
            Expression<Func<TModel, bool?>> expression, Action<CheckBoxColumnCreateOptions>? options = null)
            where TModel : class
        {
            return WithThreeStateCheckBoxColumn(source, null, expression, options);
        }

        public static FlatTreeDataGridSource<TModel> WithThreeStateCheckBoxColumn<TModel>(this FlatTreeDataGridSource<TModel> source,
            object? header, Expression<Func<TModel, bool?>> expression, Action<CheckBoxColumnCreateOptions>? options = null)
            where TModel : class
        {
            var settings = new CheckBoxColumnCreateOptions();

            options?.Invoke(settings);
            settings.IsReadOnly |= !CanWrite(expression.Body);
            var getter          = expression.Compile(preferInterpretation: !RuntimeFeature.IsDynamicCodeSupported);
            var column          = new TreeDataGridCheckBoxColumn
            {
                Header        = header,
                Binding       = CompiledBinding.Create(expression, mode: settings.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay),
                IsReadOnly    = settings.IsReadOnly,
                IsThreeState  = true,
                ValueSelector = model => getter((TModel)model),
            };

            Apply(column, settings);

            return source.WithColumn(column);
        }

        public static FlatTreeDataGridSource<TModel> WithTemplateColumn<TModel>(this FlatTreeDataGridSource<TModel> source,
            object? header, IDataTemplate cellTemplate, IDataTemplate? cellEditingTemplate = null, Action<TemplateColumnCreateOptions>? options = null)
            where TModel : class
        {
            var column = new TreeDataGridTemplateColumn
            {
                Header              = header,
                CellTemplate        = cellTemplate,
                CellEditingTemplate = cellEditingTemplate,
            };

            ConfigureTemplate(column, options);

            return source.WithColumn(column);
        }

        public static FlatTreeDataGridSource<TModel> WithTemplateColumnFromResourceKeys<TModel>(this FlatTreeDataGridSource<TModel> source,
            object? header, object cellTemplateResourceKey, object? cellEditingTemplateResourceKey = null, Action<TemplateColumnCreateOptions>? options = null)
            where TModel : class
        {
            var column = new TreeDataGridTemplateColumn(cellTemplateResourceKey, cellEditingTemplateResourceKey)
            {
                Header = header,
            };

            ConfigureTemplate(column, options);

            return source.WithColumn(column);
        }

        public static FlatTreeDataGridSource<TModel> WithRowHeaderColumn<TModel>(this FlatTreeDataGridSource<TModel> source,
            object? header = null, GridLength? width = null)
            where TModel : class
        {
            return source.WithColumn(new TreeDataGridRowHeaderColumn
            {
                Header = header,
                Width  = width ?? new GridLength(40),
            });
        }

        public static HierarchicalTreeDataGridSource<TModel> WithColumn<TModel>(this HierarchicalTreeDataGridSource<TModel> source,
            TreeDataGridColumn column)
            where TModel : class
        {
            source.Columns.Add(column);

            return source;
        }

        public static HierarchicalTreeDataGridSource<TModel> WithTextColumn<TModel, TValue>(this HierarchicalTreeDataGridSource<TModel> source,
            Expression<Func<TModel, TValue>> expression, Action<TextColumnCreateOptions>? options = null)
            where TModel : class
        {
            return WithTextColumn(source, GetHeader(expression), expression, options);
        }

        public static HierarchicalTreeDataGridSource<TModel> WithTextColumn<TModel, TValue>(this HierarchicalTreeDataGridSource<TModel> source,
            object? header, Expression<Func<TModel, TValue>> expression, Action<TextColumnCreateOptions>? options = null)
            where TModel : class
        {
            return source.WithColumn(CreateTextColumn(header, expression, options));
        }

        public static HierarchicalTreeDataGridSource<TModel> WithCheckBoxColumn<TModel>(this HierarchicalTreeDataGridSource<TModel> source,
            Expression<Func<TModel, bool>> expression, Action<CheckBoxColumnCreateOptions>? options = null)
            where TModel : class
        {
            return WithCheckBoxColumn(source, null, expression, options);
        }

        public static HierarchicalTreeDataGridSource<TModel> WithCheckBoxColumn<TModel>(this HierarchicalTreeDataGridSource<TModel> source,
            object? header, Expression<Func<TModel, bool>> expression, Action<CheckBoxColumnCreateOptions>? options = null)
            where TModel : class
        {
            var settings = new CheckBoxColumnCreateOptions();

            options?.Invoke(settings);
            settings.IsReadOnly |= !CanWrite(expression.Body);
            var getter          = expression.Compile(preferInterpretation: !RuntimeFeature.IsDynamicCodeSupported);
            var column          = new TreeDataGridCheckBoxColumn
            {
                Header        = header,
                Binding       = CompiledBinding.Create(expression, mode: settings.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay),
                IsReadOnly    = settings.IsReadOnly,
                IsThreeState  = false,
                ValueSelector = model => getter((TModel)model),
            };

            Apply(column, settings);

            return source.WithColumn(column);
        }

        public static HierarchicalTreeDataGridSource<TModel> WithThreeStateCheckBoxColumn<TModel>(this HierarchicalTreeDataGridSource<TModel> source,
            Expression<Func<TModel, bool?>> expression, Action<CheckBoxColumnCreateOptions>? options = null)
            where TModel : class
        {
            return WithThreeStateCheckBoxColumn(source, null, expression, options);
        }

        public static HierarchicalTreeDataGridSource<TModel> WithThreeStateCheckBoxColumn<TModel>(this HierarchicalTreeDataGridSource<TModel> source,
            object? header, Expression<Func<TModel, bool?>> expression, Action<CheckBoxColumnCreateOptions>? options = null)
            where TModel : class
        {
            var settings = new CheckBoxColumnCreateOptions();

            options?.Invoke(settings);
            settings.IsReadOnly |= !CanWrite(expression.Body);
            var getter          = expression.Compile(preferInterpretation: !RuntimeFeature.IsDynamicCodeSupported);
            var column          = new TreeDataGridCheckBoxColumn
            {
                Header        = header,
                Binding       = CompiledBinding.Create(expression, mode: settings.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay),
                IsReadOnly    = settings.IsReadOnly,
                IsThreeState  = true,
                ValueSelector = model => getter((TModel)model),
            };

            Apply(column, settings);

            return source.WithColumn(column);
        }

        public static HierarchicalTreeDataGridSource<TModel> WithTemplateColumn<TModel>(this HierarchicalTreeDataGridSource<TModel> source,
            object? header, IDataTemplate cellTemplate, IDataTemplate? cellEditingTemplate = null, Action<TemplateColumnCreateOptions>? options = null)
            where TModel : class
        {
            var column = new TreeDataGridTemplateColumn
            {
                Header              = header,
                CellTemplate        = cellTemplate,
                CellEditingTemplate = cellEditingTemplate,
            };

            ConfigureTemplate(column, options);

            return source.WithColumn(column);
        }

        public static HierarchicalTreeDataGridSource<TModel> WithTemplateColumnFromResourceKeys<TModel>(this HierarchicalTreeDataGridSource<TModel> source,
            object? header, object cellTemplateResourceKey, object? cellEditingTemplateResourceKey = null, Action<TemplateColumnCreateOptions>? options = null)
            where TModel : class
        {
            var column = new TreeDataGridTemplateColumn(cellTemplateResourceKey, cellEditingTemplateResourceKey)
            {
                Header = header,
            };

            ConfigureTemplate(column, options);

            return source.WithColumn(column);
        }

        public static HierarchicalTreeDataGridSource<TModel> WithRowHeaderColumn<TModel>(this HierarchicalTreeDataGridSource<TModel> source,
            object? header = null, GridLength? width = null)
            where TModel : class
        {
            return source.WithColumn(new TreeDataGridRowHeaderColumn
            {
                Header = header,
                Width  = width ?? new GridLength(40),
            });
        }

        public static HierarchicalTreeDataGridSource<TModel> WithHierarchicalExpanderColumn<TModel>(
            this HierarchicalTreeDataGridSource<TModel> source, object? header, TreeDataGridColumn inner,
            Expression<Func<TModel, IEnumerable<TModel>>> children,
            Expression<Func<TModel, bool>>? isExpanded = null, Expression<Func<TModel, bool>>? hasChildren = null,
            Action<HierarchicalExpanderColumnCreateOptions<TModel>>? options = null)
            where TModel : class
        {
            var column   = CreateExpander(header, inner, children, isExpanded, hasChildren);
            var settings = new HierarchicalExpanderColumnCreateOptions<TModel>();

            options?.Invoke(settings);
            Apply(column, settings);

            return source.WithColumn(column);
        }

        public static HierarchicalTreeDataGridSource<TModel> WithHierarchicalExpanderTextColumn<TModel, TValue>(
            this HierarchicalTreeDataGridSource<TModel> source, object? header, Expression<Func<TModel, TValue>> expression,
            Expression<Func<TModel, IEnumerable<TModel>>> children,
            Expression<Func<TModel, bool>>? isExpanded = null, Expression<Func<TModel, bool>>? hasChildren = null,
            Action<HierarchicalExpanderTextColumnCreateOptions<TModel>>? options = null)
            where TModel : class
        {
            var settings = new HierarchicalExpanderTextColumnCreateOptions<TModel>();

            options?.Invoke(settings);
            var inner  = CreateTextColumn(header, expression, x => CopyTextOptions(settings, x));
            var column = CreateExpander(header, inner, children, isExpanded, hasChildren);

            Apply(column, settings);

            return source.WithColumn(column);
        }

        public static HierarchicalTreeDataGridSource<TModel> WithHierarchicalExpanderTextColumn<TModel, TValue>(
            this HierarchicalTreeDataGridSource<TModel> source, Expression<Func<TModel, TValue>> expression,
            Expression<Func<TModel, IEnumerable<TModel>>> children,
            Expression<Func<TModel, bool>>? isExpanded = null, Expression<Func<TModel, bool>>? hasChildren = null,
            Action<HierarchicalExpanderTextColumnCreateOptions<TModel>>? options = null)
            where TModel : class => WithHierarchicalExpanderTextColumn(source, GetHeader(expression), expression, children, isExpanded, hasChildren, options);

        #endregion

        #region Private

        private static object? GetHeader(LambdaExpression expression) =>
            expression.Body is MemberExpression member ? member.Member.Name : null;

        private static bool CanWrite(Expression expression) => expression switch
        {
            MemberExpression { Member: System.Reflection.PropertyInfo property } => property.SetMethod is { IsPublic: true },
            MemberExpression { Member: System.Reflection.FieldInfo field } => !field.IsInitOnly && !field.IsLiteral,
            IndexExpression { Indexer.SetMethod.IsPublic: true } => true,
            BinaryExpression { NodeType: ExpressionType.ArrayIndex } => true,
            _ => false,
        };

        private static TreeDataGridHierarchicalExpanderColumn CreateExpander<TModel>(object? header,
            TreeDataGridColumn inner, Expression<Func<TModel, IEnumerable<TModel>>> children,
            Expression<Func<TModel, bool>>? isExpanded, Expression<Func<TModel, bool>>? hasChildren)
            where TModel : class
        {
            var column = new TreeDataGridHierarchicalExpanderColumn
            {
                Header             = header,
                Inner              = inner,
                Width              = inner.Width,
                ChildrenBinding    = CompiledBinding.Create(children),
                IsExpandedBinding  = isExpanded is null ? null : CompiledBinding.Create(isExpanded, mode: BindingMode.TwoWay),
                HasChildrenBinding = hasChildren is null ? null : CompiledBinding.Create(hasChildren),
            };

            // Only direct member access uses the fast path. Complex binding paths retain binding-engine semantics.
            if (children.Body is MemberExpression { Expression: ParameterExpression })
            {
                var getter = children.Compile(preferInterpretation: !RuntimeFeature.IsDynamicCodeSupported);
                column.ChildrenSelector = model => getter((TModel)model);
            }

            if (hasChildren?.Body is MemberExpression { Expression: ParameterExpression })
            {
                var getter = hasChildren.Compile(preferInterpretation: !RuntimeFeature.IsDynamicCodeSupported);
                column.HasChildrenSelector = model => getter((TModel)model);
            }

            if (isExpanded?.Body is MemberExpression { Expression: ParameterExpression })
            {
                var getter = isExpanded.Compile(preferInterpretation: !RuntimeFeature.IsDynamicCodeSupported);
                column.IsExpandedSelector = model => getter((TModel)model);
            }

            return column;
        }

        private static TreeDataGridTextColumn CreateTextColumn<TModel, TValue>(object? header,
            Expression<Func<TModel, TValue>> expression, Action<TextColumnCreateOptions>? options)
        {
            var settings = new TextColumnCreateOptions();

            options?.Invoke(settings);
            settings.IsReadOnly |= !CanWrite(expression.Body);

            var getter          = expression.Compile(preferInterpretation: !RuntimeFeature.IsDynamicCodeSupported);
            var column          = new TreeDataGridTextColumn
            {
                Header              = header,
                Binding             = CompiledBinding.Create(expression, mode: settings.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay),
                IsReadOnly          = settings.IsReadOnly,
                IsTextSearchEnabled = settings.IsTextSearchEnabled,
                UseDrawnCells       = settings.UseDrawnCells,
                TextAlignment       = settings.TextAlignment,
                TextTrimming        = settings.TextTrimming,
                TextWrapping        = settings.TextWrapping,
                StringFormat        = settings.StringFormat,
                Culture             = settings.Culture,
                ValueSelector       = model => getter((TModel)model),
            };

            column.ValueType = typeof(TValue);
            Apply(column, settings);

            return column;
        }

        private static void ConfigureTemplate(TreeDataGridTemplateColumn column, Action<TemplateColumnCreateOptions>? options)
        {
            var settings = new TemplateColumnCreateOptions();

            options?.Invoke(settings);
            Apply(column, settings);
            column.TextSearchBinding = settings.TextSearchBinding;
        }

        private static void Apply(TreeDataGridColumn column, ColumnCreateOptions options)
        {
            column.AllowTriStateSorting       = options.AllowTriStateSorting;
            column.BeginEditGestures          = options.BeginEditGestures;
            column.CanUserResize              = options.CanUserResize;
            column.CanUserSort                = options.CanUserSort;
            column.CompareAscending           = options.CompareAscending;
            column.CompareDescending          = options.CompareDescending;
            column.MaxWidth                   = options.MaxWidth;
            column.MinWidth                   = options.MinWidth;
            column.HorizontalContentAlignment = options.HorizontalContentAlignment;
            column.VerticalContentAlignment   = options.VerticalContentAlignment;
            column.Width                      = options.Width ?? GridLength.Auto;
        }

        private static void CopyTextOptions(TextColumnCreateOptions source, TextColumnCreateOptions target)
        {
            target.AllowTriStateSorting       = source.AllowTriStateSorting;
            target.BeginEditGestures          = source.BeginEditGestures;
            target.CanUserResize              = source.CanUserResize;
            target.CanUserSort                = source.CanUserSort;
            target.CompareAscending           = source.CompareAscending;
            target.CompareDescending          = source.CompareDescending;
            target.MaxWidth                   = source.MaxWidth;
            target.MinWidth                   = source.MinWidth;
            target.HorizontalContentAlignment = source.HorizontalContentAlignment;
            target.VerticalContentAlignment   = source.VerticalContentAlignment;
            target.Width                      = source.Width;
            target.IsReadOnly                 = source.IsReadOnly;
            target.IsTextSearchEnabled        = source.IsTextSearchEnabled;
            target.UseDrawnCells              = source.UseDrawnCells;
            target.StringFormat               = source.StringFormat;
            target.Culture                    = source.Culture;
            target.TextTrimming               = source.TextTrimming;
            target.TextWrapping               = source.TextWrapping;
            target.TextAlignment              = source.TextAlignment;
        }

        #endregion
    }
}
