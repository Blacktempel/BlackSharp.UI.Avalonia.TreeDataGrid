using Avalonia.Data;
using Avalonia.Media;
using BlackSharp.UI.Avalonia.TreeDataGrid.Experimental.Data;
using BlackSharp.UI.Avalonia.TreeDataGrid.Experimental.Data.Core;
using System.ComponentModel;
using System.Globalization;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Models
{
    internal class TextCell<T> : NotifyingBase, ITextCell, IDisposable, IEditableObject, ICellValidation
    {
        #region Constructor

        public TextCell(T? value)
        {
            _value     = value;
            IsReadOnly = true;
        }

        public TextCell(
            IObserver<BindingValue<T>> bindingObserver,
            IObservable<BindingValue<T>> bindingObservable,
            IObservable<BindingValue<bool>> isReadOnlyObservable,
            ITextCellOptions? options = null)
        {
            _binding = bindingObserver;
            _options = options;

            _subscription = new CompositeDisposable(
                bindingObservable.Subscribe(x =>
                {
                    if (x.HasValue)
                    {
                        Value = x.Value;
                    }
                }),
                isReadOnlyObservable.Subscribe(x =>
                {
                    if (x.HasValue)
                    {
                        IsReadOnly = x.Value;
                    }
                }));
        }

        #endregion

        #region Fields

        private readonly IObserver<BindingValue<T>>? _binding;

        private readonly ITextCellOptions?           _options;

        private readonly IDisposable?                _subscription;

        private Exception?                           _validationError;

        private string?                              _editText;

        private T?                                   _value;

        private bool                                 _isEditing;

        #endregion

        #region Properties

        /// <inheritdoc/>
        internal TreeDataGridColumn? Column { get; init; }

        public Exception? ValidationError
        {
            get => _validationError;
            private set => RaiseAndSetIfChanged(ref _validationError, value);
        }

        public bool CanEdit => !IsReadOnly;

        /// <inheritdoc/>
        public BeginEditGestures EditGestures => _options?.BeginEditGestures ?? BeginEditGestures.Default;

        /// <inheritdoc/>
        public TextTrimming TextTrimming => _options?.TextTrimming ?? TextTrimming.None;

        /// <inheritdoc/>
        public TextWrapping TextWrapping => _options?.TextWrapping ?? TextWrapping.NoWrap;

        /// <inheritdoc/>
        public TextAlignment TextAlignment => _options?.TextAlignment ?? TextAlignment.Left;

        /// <inheritdoc/>
        public bool IsReadOnly { get; private set; }

        /// <inheritdoc/>
        public string? Text
        {
            get
            {
                if (_isEditing)
                {
                    return _editText;
                }
                else if (_options?.StringFormat is { } format)
                {
                    var culture = _options.Culture ?? CultureInfo.CurrentCulture;

                    if (format == "{0}" && _value is string value && culture.GetFormat(typeof(ICustomFormatter)) is null)
                    {
                        return value;
                    }

                    return string.Format(culture, format, _value);
                }
                else
                {
                    return _value?.ToString();
                }
            }
            set
            {
                if (_isEditing)
                {
                    _editText = value;
                }
                else
                {
                    try
                    {
                        var valueType = typeof(T) == typeof(object) && Column is TreeDataGridBoundColumn { ValueType: { } declaredType } ? declaredType : typeof(T);
                        var type      = Nullable.GetUnderlyingType(valueType) ?? valueType;

                        if (type == typeof(object) && _value is not null)
                        {
                            type = _value.GetType();
                        }

                        var converted = string.IsNullOrEmpty(value) && Nullable.GetUnderlyingType(valueType) is not null ? null :
                            type.IsEnum ? Enum.Parse(type, value!, ignoreCase: true) :
                            Convert.ChangeType(value, type, _options?.Culture ?? CultureInfo.CurrentCulture);

                        ValidationError = null;
                        Value           = (T?)converted;
                    }
                    catch (Exception exception) when (exception is FormatException or InvalidCastException or OverflowException or ArgumentException)
                    {
                        ValidationError = exception;
                    }
                }
            }
        }

        /// <inheritdoc cref="ICell.Value"/>
        public T? Value
        {
            get => _value;
            set
            {
                if (RaiseAndSetIfChanged(ref _value, value) && !IsReadOnly && !_isEditing)
                {
                    _binding!.OnNext(value!);
                }
            }
        }

        #endregion

        #region Public

        /// <inheritdoc/>
        public void BeginEdit()
        {
            if (!_isEditing && !IsReadOnly)
            {
                _editText = _value is IFormattable formatted ?
                    formatted.ToString(null, _options?.Culture ?? CultureInfo.CurrentCulture) : _value?.ToString();
                _isEditing = true;
            }
        }

        /// <inheritdoc/>
        public void CancelEdit()
        {
            if (_isEditing)
            {
                _isEditing      = false;
                _editText       = null;
                ValidationError = null;
            }
        }

        /// <inheritdoc/>
        public void EndEdit()
        {
            if (_isEditing)
            {
                var text = _editText;

                _isEditing = false;
                _editText  = null;
                Text       = text;
            }
        }

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            _subscription?.Dispose();
            GC.SuppressFinalize(this);
        }

        #endregion

        #region ICell

        object? ICell.Value => Value;

        #endregion
    }
}
