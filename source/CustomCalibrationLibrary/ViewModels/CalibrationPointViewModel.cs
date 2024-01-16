/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
﻿using GazeUtilityLibrary.DataStructs;
using System.Windows;

namespace CustomCalibrationLibrary.ViewModels
{
    /// <summary>
    /// The view model for a calibration point.
    /// </summary>
    class CalibrationPointViewModel : CalibrationPoint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CalibrationPointViewModel"/> class.
        /// </summary>
        /// <param name="point">The position of the calibration point.</param>
        /// <param name="index">The index of the calibration point.</param>
        public CalibrationPointViewModel(Point point, int index) : base(point, index) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CalibrationPointViewModel"/> class.
        /// </summary>
        /// <param name="point">The calibration point object.</param>
        public CalibrationPointViewModel(CalibrationPoint point) : base(point.Position, point.Index)
        {
            GazePositionAverage = point.GazePositionAverage;
            GazePositionLeft = point.GazePositionLeft;
            GazePositionRight = point.GazePositionRight;
            HasData = point.HasData;
            HasFailed = point.HasFailed;
        }

    }
}
