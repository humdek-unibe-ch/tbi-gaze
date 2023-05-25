using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CustomCalibrationLibrary.Converters
{
    public class PointPositionConverter : IValueConverter
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

            double coefficient;
            Point point = (Point)value;

            if (double.TryParse(parameter.ToString(), out coefficient))
            {
                return new Point(point.X * coefficient, point.Y * coefficient);
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

            Point point = (Point)value;
            double coefficient;

            if (double.TryParse(parameter.ToString(), out coefficient))
            {
                if (coefficient != 0)
                {
                    return new Point(point.X / coefficient, point.Y / coefficient);
                }
            }

            return 0;
        }
    }
}
