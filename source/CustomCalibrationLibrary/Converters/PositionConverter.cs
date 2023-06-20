using System;
using System.Windows.Data;
using System.Globalization;

namespace CustomCalibrationLibrary.Converters
{
    /// <summary>
    /// Converter class to convert a normalized coordinate to a pixel coordinate.
    /// </summary>
    public class PositionConverter: IValueConverter
    {

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

            if (double.TryParse(value.ToString(), out number) && double.TryParse(parameter.ToString(), out coefficient))
            {
                return number * coefficient;
            }

            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
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

            if (double.TryParse(value.ToString(), out number) && double.TryParse(parameter.ToString(), out coefficient))
            {
                if (coefficient != 0)
                {
                    return number / coefficient;
                }
            }

            return 0;
        }
    }
}
