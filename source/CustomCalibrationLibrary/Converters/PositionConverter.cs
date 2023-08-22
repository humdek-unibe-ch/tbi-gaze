using System;
using System.Windows.Data;
using System.Globalization;
using System.Windows;

namespace CustomCalibrationLibrary.Converters
{
    /// <summary>
    /// Converter class to convert a normalized coordinate to a pixel coordinate.
    /// </summary>
    public class PositionConverter: DependencyObject, IValueConverter
    {
        /// <summary>
        /// The position offset.
        /// </summary>
        public string Offset
        {
            get => (string)GetValue(OffsetProperty) ?? "0";
            set => SetValue(OffsetProperty, value);
        }

        /// <summary>
        /// The custom offset property of the value converter.
        /// </summary>
        public static readonly DependencyProperty OffsetProperty =
        DependencyProperty.Register("Offset", typeof(string), typeof(PositionConverter), new PropertyMetadata(null));

        /// <summary>
        /// Value converter.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="targetType">The type of the target value.</param>
        /// <param name="parameter">The conversion parameter.</param>
        /// <param name="culture">The language localisation.</param>
        /// <returns>The converted value object</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return 0;
            }
            if (parameter == null)
            {
                parameter = 1;
            }

            double number;
            double coefficient;
            double offset;
            if (!double.TryParse(Offset, out offset))
            {
                offset = 0;
            }

            if (double.TryParse(value.ToString(), out number) && double.TryParse(parameter.ToString(), out coefficient))
            {
                return (number * coefficient) - offset;
            }

            return 0;
        }

        /// <summary>
        /// Reverted value converter.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="targetType">The type of the target value.</param>
        /// <param name="parameter">The conversion parameter.</param>
        /// <param name="culture">The language localisation.</param>
        /// <returns>The converted value object</returns>
        /// <exception cref="NotSupportedException"></exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
