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
    /// The view model class of the drift compensation view.
    /// </summary>
    class DriftCompensationViewModel: ColoredViewModel
    {
        /// <summary>
        /// The point on the screen which the participant is supposed to fixate.
        /// </summary>
        public CalibrationPoint FixationPoint { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public DriftCompensationViewModel(Color backgroundColor) : base(backgroundColor, backgroundColor)
        {
            FixationPoint = new CalibrationPoint(new Point(0.5, 0.5), 0);
        }
    }
}
