﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace CustomCalibrationLibrary.Converters
{
    /// <summary>
    /// Converts True to Hidden and False to Visible
    /// </summary>
    public class HasDataToVisibilityConverter: IValueConverter
    {
        /// <summary>
        /// Value converter.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="targetType">The type of the target value.</param>
        /// <param name="parameter">The conversion parameter.</param>
        /// <param name="culture">The language localisation.</param>
        /// <returns>The converted value object</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(Visibility))
                throw new InvalidOperationException("The target must be of type visibility");

            return (bool)value ? Visibility.Hidden : Visibility.Visible;
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
        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
