using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;

namespace BlackSharp.UI.Avalonia.TreeDataGrid
{
    public sealed class TreeDataGridTemplateColumn : TreeDataGridColumn
    {
        #region Constructor

        public TreeDataGridTemplateColumn()
        {
        }

        public TreeDataGridTemplateColumn(object cellTemplateResourceKey, object? cellEditingTemplateResourceKey = null)
        {
            _cellTemplateResourceKey        = cellTemplateResourceKey;
            _cellEditingTemplateResourceKey = cellEditingTemplateResourceKey;
        }

        #endregion

        #region Fields

        private readonly object? _cellTemplateResourceKey;

        private readonly object? _cellEditingTemplateResourceKey;

        public static readonly StyledProperty<IDataTemplate?> CellTemplateProperty        =
            AvaloniaProperty.Register<TreeDataGridTemplateColumn, IDataTemplate?>(nameof(CellTemplate));

        public static readonly StyledProperty<IDataTemplate?> CellEditingTemplateProperty =
            AvaloniaProperty.Register<TreeDataGridTemplateColumn, IDataTemplate?>(nameof(CellEditingTemplate));

        #endregion

        #region Properties

        public IDataTemplate? CellTemplate
        {
            get => GetValue(CellTemplateProperty);
            set => SetValue(CellTemplateProperty, value);
        }

        public IDataTemplate? CellEditingTemplate
        {
            get => GetValue(CellEditingTemplateProperty);
            set => SetValue(CellEditingTemplateProperty, value);
        }

        internal BindingBase? TextSearchBinding { get; set; }

        internal bool HasEditingTemplate => CellEditingTemplate is not null || _cellEditingTemplateResourceKey is not null;

        #endregion

        #region Internal

        internal IDataTemplate GetCellTemplate(Control anchor) =>
            CellTemplate ?? FindTemplate(anchor, _cellTemplateResourceKey);

        internal IDataTemplate GetEditingTemplate(Control anchor) =>
            CellEditingTemplate ?? FindTemplate(anchor, _cellEditingTemplateResourceKey);

        #endregion

        #region Private

        private static IDataTemplate FindTemplate(Control anchor, object? key)
        {
            if (key is not null && anchor.TryFindResource(key, out var resource) && resource is IDataTemplate template)
            {
                return template;
            }

            throw new InvalidOperationException($"The cell template resource '{key}' was not found.");
        }

        #endregion
    }
}
