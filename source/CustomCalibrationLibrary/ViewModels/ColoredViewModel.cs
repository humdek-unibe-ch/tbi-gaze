/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
using System.Windows.Media;

namespace CustomCalibrationLibrary.ViewModels
{
    /// <summary>
    /// The base view model for coloring the view.
    /// </summary>
    public class ColoredViewModel
    {
        private Color _backgroundColor;
        /// <summary>
        /// The background color of the canvas.
        /// </summary>
        public Color BackgroundColor
        {
            get { return _backgroundColor; }
        }

        private Color _frameColor;
        /// <summary>
        /// The background color of the frame.
        /// </summary>
        public Color FrameColor
        {
            get { return _frameColor; }
        }

        private Color _foregroundColor;
        /// <summary>
        /// The text and calibration point color.
        /// </summary>
        public Color ForegroundColor
        {
            get { return _foregroundColor; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="backgroundColor">The background color of the view.</param>
        /// <param name="frameColor">The frame color of the view.</param>
        /// <param name="foregroundColor">The foreground color of the view.</param>
        public ColoredViewModel(Color backgroundColor, Color frameColor, Color foregroundColor)
        {
            _backgroundColor = backgroundColor;
            _frameColor = frameColor;
            _foregroundColor = foregroundColor;
        }
    }
}
