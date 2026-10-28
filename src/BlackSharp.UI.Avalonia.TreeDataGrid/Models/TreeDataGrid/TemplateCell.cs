using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using BlackSharp.UI.Avalonia.TreeDataGrid.Experimental.Data;
using System.ComponentModel;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Models
{
    internal class TemplateCell : IEditableCell, IEditableObject, IDisposable
    {
        #region Constructor

        public TemplateCell(
            object? value,
            Func<Control, IDataTemplate> getCellTemplate,
            Func<Control, IDataTemplate>? getCellEditingTemplate,
            IObservable<BindingValue<bool>> isReadOnlyObservable,
            ITemplateCellOptions? options)
        {
            GetCellTemplate        = getCellTemplate;
            GetCellEditingTemplate = getCellEditingTemplate;
            Value                  = value;
            _options               = options;

            _subscription = isReadOnlyObservable.Subscribe(x =>
            {
                if (x.HasValue)
                {
                    IsReadOnly = x.Value;
                }
            });
        }

        #endregion

        #region Fields

        private ITemplateCellOptions? _options;

        private readonly IDisposable  _subscription;

        #endregion

        #region Properties

        /// <inheritdoc/>
        public bool CanEdit => !IsReadOnly;

        /// <inheritdoc/>
        public BeginEditGestures EditGestures => _options?.BeginEditGestures ?? BeginEditGestures.Default;

        public Func<Control, IDataTemplate> GetCellTemplate { get; }

        public Func<Control, IDataTemplate>? GetCellEditingTemplate { get; }

        /// <inheritdoc/>
        public bool IsReadOnly { get; private set; }

        /// <inheritdoc/>
        public object? Value { get; }

        #endregion

        #region IDisposable

        void IDisposable.Dispose() => _subscription.Dispose();

        #endregion

        #region IEditableObject

        void IEditableObject.BeginEdit() => (Value as IEditableObject)?.BeginEdit();

        void IEditableObject.CancelEdit() => (Value as IEditableObject)?.CancelEdit();

        void IEditableObject.EndEdit() => (Value as IEditableObject)?.EndEdit();

        #endregion
    }
}
