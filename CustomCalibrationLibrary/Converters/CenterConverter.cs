using System;
using System.Globalization;
using System.Windows.Data;

namespace CustomCalibrationLibrary.Converters
{
    public class CenterConverter : IValueConverter
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
                return -number / 2;
            }

            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return 0;
            }

            double number;

            if (double.TryParse(value.ToString(), out number))
            {

                return number / 2;
            }

            return 0;
        }
    }
}
