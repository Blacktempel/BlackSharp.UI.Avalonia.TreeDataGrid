using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using BlackSharp.UI.Avalonia.TreeDataGrid.Models;
using BlackSharp.UI.Avalonia.TreeDataGrid.Selection;
using System.ComponentModel;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Primitives
{
    public class TreeDataGridTextCell : TreeDataGridEditableCell
    {
        #region Fields

        public static readonly DirectProperty<TreeDataGridTextCell, TextTrimming>  TextTrimmingProperty  =
            AvaloniaProperty.RegisterDirect<TreeDataGridTextCell, TextTrimming>(
                nameof(TextTrimming),
                o => o.TextTrimming);

        public static readonly DirectProperty<TreeDataGridTextCell, TextWrapping>  TextWrappingProperty  =
            AvaloniaProperty.RegisterDirect<TreeDataGridTextCell, TextWrapping>(
                nameof(TextWrapping),
                o => o.TextWrapping);

        public static readonly DirectProperty<TreeDataGridTextCell, string?>       ValueProperty         =
            AvaloniaProperty.RegisterDirect<TreeDataGridTextCell, string?>(
                nameof(Value),
                o => o.Value,
                (o, v) => o.Value = v);

        public static readonly DirectProperty<TreeDataGridTextCell, TextAlignment> TextAlignmentProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridTextCell, TextAlignment>(
                nameof(TextAlignment),
                o => o.TextAlignment,
                (o, v) => o.TextAlignment = v);

        private string?       _value;

        private TextBox?      _edit;

        private bool          _modelValueChanging;

        private TextTrimming  _textTrimming       = TextTrimming.CharacterEllipsis;

        private TextWrapping  _textWrapping       = TextWrapping.NoWrap;

        private TextAlignment _textAlignment      = TextAlignment.Left;

        #endregion

        #region Properties

        public TextTrimming TextTrimming
        {
            get => _textTrimming;
            set => SetAndRaise(TextTrimmingProperty, ref _textTrimming, value);
        }

        public TextWrapping TextWrapping
        {
            get => _textWrapping;
            set => SetAndRaise(TextWrappingProperty, ref _textWrapping, value);
        }

        public string? Value
        {
            get => _value;
            set
            {
                if (SetAndRaise(ValueProperty, ref _value, value) && Model is ITextCell cell)
                {
                    if (!_modelValueChanging)
                    {
                        cell.Text = _value;
                    }

                    RaiseCellValueChanged();
                }
            }
        }

        public TextAlignment TextAlignment
        {
            get => _textAlignment;
            set => SetAndRaise(TextAlignmentProperty, ref _textAlignment, value);
        }

        #endregion

        #region Internal

        internal override void Realize(
            TreeDataGridElementFactory factory,
            IEngineSelectionInteraction? selection,
            ICell model,
            int columnIndex,
            int rowIndex)
        {
            Value         = (model as ITextCell)?.Text;
            TextTrimming  = (model as ITextCell)?.TextTrimming ?? TextTrimming.CharacterEllipsis;
            TextWrapping  = (model as ITextCell)?.TextWrapping ?? TextWrapping.NoWrap;
            TextAlignment = (model as ITextCell)?.TextAlignment ?? TextAlignment.Left;

            base.Realize(factory, selection, model, columnIndex, rowIndex);
        }

        #endregion

        #region Protected

        protected override AutomationPeer OnCreateAutomationPeer() =>
            new Automation.Peers.TreeDataGridTextCellAutomationPeer(this);

        protected override void UpdateValue()
        {
            Value = (Model as ITextCell)?.Text;
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            _edit = e.NameScope.Find<TextBox>("PART_Edit");

            if (_edit is not null)
            {
                DataValidationErrors.SetError(_edit, (Model as ICellValidation)?.ValidationError);
                _edit.SelectAll();
                _edit.Focus();
            }
        }

        protected override void OnModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            base.OnModelPropertyChanged(sender, e);

            if (e.PropertyName == nameof(ICellValidation.ValidationError))
            {
                var error = (Model as ICellValidation)?.ValidationError;

                DataValidationErrors.SetError(this, error);

                if (_edit is not null)
                {
                    DataValidationErrors.SetError(_edit, error);
                }
            }

            if (e.PropertyName == nameof(ITextCell.Value))
            {
                try
                {
                    _modelValueChanging = true;
                    UpdateValue();
                }
                finally
                {
                    _modelValueChanging = false;
                }
            }
        }

        #endregion
    }
}
