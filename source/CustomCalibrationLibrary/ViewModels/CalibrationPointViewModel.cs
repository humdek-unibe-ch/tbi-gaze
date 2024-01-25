/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
﻿using GazeUtilityLibrary.DataStructs;
using System.Windows;
using System.Windows.Media;

namespace CustomCalibrationLibrary.ViewModels
{
    /// <summary>
    /// The view model for a calibration point.
    /// </summary>
    class CalibrationPointViewModel : CalibrationPoint
    {
        private Brush _pointColor;
        /// <summary>
        /// The color of the calibration point.
        /// </summary>
        public Brush PointColor { get { return HasFailed ? new SolidColorBrush(Colors.Red) : _pointColor; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="CalibrationPointViewModel"/> class.
        /// </summary>
        /// <param name="point">The position of the calibration point.</param>
        /// <param name="index">The index of the calibration point.</param>
        /// <param name="pointColor">The base color or the calibration point.</param>
        public CalibrationPointViewModel(Point point, int index, Color pointColor) : base(point, index)
        {
            _pointColor = new SolidColorBrush(pointColor);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CalibrationPointViewModel"/> class.
        /// </summary>
        /// <param name="point">The calibration point object.</param>
        /// <param name="pointColor">The base color or the calibration point.</param>
        public CalibrationPointViewModel(CalibrationPoint point, Color pointColor) : base(point.Position, point.Index)
        {
            _pointColor = new SolidColorBrush(pointColor);
            GazePositionAverage = point.GazePositionAverage;
            GazePositionLeft = point.GazePositionLeft;
            GazePositionRight = point.GazePositionRight;
            HasData = point.HasData;
            HasFailed = point.HasFailed;
        }
    }
}
