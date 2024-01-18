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
    /// Allows to attach a color property.
    /// </summary>
    public class BrushExtension
    {
        public static DependencyProperty BrushProperty =
            DependencyProperty.RegisterAttached("Brush",
                                                typeof(Brush),
                                                typeof(BrushExtension),
                                                new PropertyMetadata(null));
        public static Brush GetBrush(DependencyObject target)
        {
            return (Brush)target.GetValue(BrushProperty);
        }
        public static void SetBrush(DependencyObject target, Brush value)
        {
            target.SetValue(BrushProperty, value);
        }
    }
}
