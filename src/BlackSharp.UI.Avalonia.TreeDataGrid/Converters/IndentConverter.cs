using Avalonia;
using Avalonia.Data.Converters;
using System.Globalization;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Converters
{
    public class IndentConverter : IValueConverter, IMultiValueConverter
    {
        #region Fields

        private const double ChevronHorizontalMargin = 8;

        private const double DefaultIndentStep = 20;

        #endregion

        #region Properties

        /// <summary>
        /// Singleton instance
        /// </summary>
        public static IndentConverter Instance { get; } = new IndentConverter();

        #endregion

        #region Public

        /// <inheritdoc cref="IndentConverter"/>
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int indent)
            {
                return new Thickness(DefaultIndentStep * indent, 0, 0, 0);
            }

            return new Thickness();
        }

        /// <summary>
        /// Converts an indentation level and chevron width into a left padding that keeps tree levels aligned.
        /// </summary>
        public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values.Count == 2 && values[0] is int indent && values[1] is double chevronWidth &&
                double.IsFinite(chevronWidth) && chevronWidth >= 0)
            {
                return new Thickness((chevronWidth + ChevronHorizontalMargin) * indent, 0, 0, 0);
            }

            return Convert(values.Count > 0 ? values[0] : null, targetType, parameter, culture);
        }

        /// <summary>
        /// Convert-back is not supported (one-way binding only).
        /// </summary>
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
