using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CustomCalibrationLibrary.Converters
{
    /// <summary>
    /// Converter class to convert the proximito of a normailezed coordinate to the center point
    /// (0.5) into colors.
    /// </summary>
    public class ProximityColorConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return 0;
            }

            double number;

            if (double.TryParse(value.ToString(), out number))
            {
                if ((number > 0.2 && number < 0.4) || (number > 0.6 && number < 0.8))
                {
                    return new SolidColorBrush(Colors.Orange);
                }
                else if (number >= 0.4 && number <= 0.6)
                {
                    return new SolidColorBrush(Colors.Green);
                }
                else
                {
                    return new SolidColorBrush(Colors.Red);
                }
            }

            return new SolidColorBrush(Colors.White);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return 0;
        }
    }
}
