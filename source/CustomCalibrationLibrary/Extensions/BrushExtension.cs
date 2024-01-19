/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
using System.Windows;
using System.Windows.Media;

namespace CustomCalibrationLibrary.Extensions
{
    /// <summary>
    /// Allows to attach a brush property.
    /// </summary>
    public class BrushExtension
    {
        /// <summary>
        /// The brush color dependency property.
        /// </summary>
        public static DependencyProperty BrushProperty = DependencyProperty.RegisterAttached("Brush", typeof(Brush), typeof(BrushExtension), new PropertyMetadata(null));

        /// <summary>
        /// Get the brush property value of the target.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns>The brush of the target.</returns>
        public static Brush GetBrush(DependencyObject target)
        {
            return (Brush)target.GetValue(BrushProperty);
        }

        /// <summary>
        /// Setting the brush property value of a target
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="value">The brush property value.</param>
        public static void SetBrush(DependencyObject target, Brush value)
        {
            target.SetValue(BrushProperty, value);
        }
    }
}
