using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Data;
using Avalonia.Media;
using BlackSharp.UI.Avalonia.TreeDataGrid.Models;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace BlackSharp.UI.Avalonia.TreeDataGrid
{
    internal sealed class TreeDataGridTextCellRenderer : CellRenderer
    {
        #region Constructor

        internal TreeDataGridTextCellRenderer(TreeDataGridTextColumn column) => _column = column;

        #endregion

        #region Fields

        private readonly TreeDataGridTextColumn _column;

        private readonly ConditionalWeakTable<Control, TextLayouts> _layouts = new();

        #endregion

        #region Public

        public override Size MeasureContent(Control owner, object? dataContext, Size availableSize)
        {
            var text = GetOrCreateText(owner, GetText(dataContext), availableSize, false);

            return new Size(text.WidthIncludingTrailingWhitespace, text.Height);
        }

        public override void RenderContent(Control owner, DrawingContext context, object? dataContext, Rect bounds, bool isSelected) =>
            RenderText(owner, context, GetText(dataContext), bounds);

        public override Control? CreateEditControl(object? dataContext)
        {
            if (_column.IsReadOnly || _column.Binding is null)
            {
                return null;
            }

            var editor = new TextBox { DataContext = dataContext };

            editor.Bind(TextBox.TextProperty, _column.Binding);

            return editor;
        }

        public override void CommitEdit(Control control)
        {
            if (control is TextBox editor)
            {
                BindingOperations.GetBindingExpressionBase(editor, TextBox.TextProperty)?.UpdateSource();
            }
        }

        #endregion

        #region Internal

        internal void ClearCache(Control owner) => _layouts.Remove(owner);

        internal FormattedText GetOrCreateText(Control owner, string value, Size constraint, bool rendering) =>
            GetOrCreateLayout(owner, value, constraint, rendering).Text;

        internal Size MeasureCell(Control owner, ITextCell cell, Size availableSize)
        {
            var text = GetOrCreateText(owner, cell.Text ?? string.Empty, availableSize, false);

            return new Size(text.WidthIncludingTrailingWhitespace, text.Height);
        }

        internal void RenderCell(Control owner, DrawingContext context, ITextCell cell, Rect bounds) =>
            RenderText(owner, context, cell.Text ?? string.Empty, bounds);

        #endregion

        #region Private

        private TextLayout GetOrCreateLayout(Control owner, string value, Size constraint, bool rendering)
        {
            var foreground = owner.GetValue(TextElement.ForegroundProperty);

            var key = new TextLayoutKey(value, _column.Culture ?? CultureInfo.CurrentCulture, owner.FlowDirection,
                new Typeface(owner.GetValue(TextElement.FontFamilyProperty), owner.GetValue(TextElement.FontStyleProperty),
                    owner.GetValue(TextElement.FontWeightProperty), owner.GetValue(TextElement.FontStretchProperty)),
                owner.GetValue(TextElement.FontSizeProperty), foreground,
                (foreground as ISolidColorBrush)?.Color, foreground?.Opacity ?? 1,
                foreground?.Transform?.Value ?? Matrix.Identity, foreground?.TransformOrigin ?? RelativePoint.TopLeft,
                _column.TextAlignment, _column.TextTrimming, _column.TextWrapping,
                double.IsFinite(constraint.Width) && constraint.Width > 0 ? constraint.Width : double.PositiveInfinity);

            // Mutable gradient/image brushes keep the original uncached behavior. Solid brushes are keyed by value.
            if (foreground is not null and not ISolidColorBrush and not IImmutableBrush)
            {
                _layouts.Remove(owner);

                return new TextLayout(key, CreateText(key));
            }

            var layouts = _layouts.GetValue(owner, static _ => new TextLayouts());
            var current = rendering ? layouts.Rendered : layouts.Measured;
            var other   = rendering ? layouts.Measured : layouts.Rendered;

            if (current is not null && current.Key.Matches(key))
            {
                return current;
            }

            var layout = other is not null && other.Key.Matches(key) ? other : new TextLayout(key, CreateText(key));

            if (rendering)
            {
                layouts.Rendered = layout;
            }
            else
            {
                layouts.Measured = layout;
            }

            return layout;
        }

        private string GetText(object? model)
        {
            var value = model is not null ? _column.GetCellValue(model) : null;

            return _column.StringFormat is { } format ?
                string.Format(_column.Culture ?? CultureInfo.CurrentCulture, format, value) :
                Convert.ToString(value, _column.Culture ?? CultureInfo.CurrentCulture) ?? string.Empty;
        }

        private void RenderText(Control owner, DrawingContext context, string value, Rect bounds)
        {
            var layout = GetOrCreateLayout(owner, value, bounds.Size, true);

            if (layout.Drawing is null)
            {
                var drawing = new DrawingGroup();

                using (var recording = drawing.Open())
                {
                    recording.DrawText(layout.Text, default);
                }

                layout.Drawing = drawing;
            }

            using (context.PushClip(bounds))
            using (context.PushTransform(Matrix.CreateTranslation(bounds.X, bounds.Y)))
            {
                layout.Drawing.Draw(context);
            }
        }

        private static FormattedText CreateText(TextLayoutKey key)
        {
            var text = new FormattedText(key.Value, key.Culture, key.FlowDirection, key.Typeface, key.FontSize, key.Foreground)
            {
                TextAlignment = key.Alignment,
                Trimming      = key.Trimming,
                MaxLineCount  = key.Wrapping == TextWrapping.Wrap ? int.MaxValue : 1,
            };

            if (double.IsFinite(key.Width))
            {
                text.MaxTextWidth = key.Width;
            }

            return text;
        }

        #endregion

        #region Nested Types

        private sealed class TextLayouts
        {
            public TextLayout? Measured { get; set; }

            public TextLayout? Rendered { get; set; }
        }

        private sealed record TextLayout(TextLayoutKey Key, FormattedText Text)
        {
            public DrawingGroup? Drawing { get; set; }
        }

        private readonly record struct TextLayoutKey(string Value, CultureInfo Culture, FlowDirection FlowDirection,
            Typeface Typeface, double FontSize, IBrush? Foreground, Color? Color, double Opacity, Matrix Transform,
            RelativePoint TransformOrigin, TextAlignment Alignment, TextTrimming Trimming, TextWrapping Wrapping, double Width)
        {
            public bool Matches(TextLayoutKey other) => Value == other.Value && ReferenceEquals(Culture, other.Culture) &&
                FlowDirection == other.FlowDirection && Typeface == other.Typeface && FontSize == other.FontSize &&
                ReferenceEquals(Foreground, other.Foreground) && Color == other.Color && Opacity == other.Opacity &&
                Transform == other.Transform && TransformOrigin == other.TransformOrigin && Alignment == other.Alignment &&
                Trimming == other.Trimming && Wrapping == other.Wrapping && Width == other.Width;
        }

        #endregion
    }
}
