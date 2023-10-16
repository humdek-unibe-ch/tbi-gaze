/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
﻿using System;
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
