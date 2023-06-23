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
        public string Offset
        {
            get => (string)GetValue(OffsetProperty) ?? "0";
            set => SetValue(OffsetProperty, value);
        }

        public static readonly DependencyProperty OffsetProperty =
        DependencyProperty.Register("Offset", typeof(string), typeof(PositionConverter), new PropertyMetadata(null));

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

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
