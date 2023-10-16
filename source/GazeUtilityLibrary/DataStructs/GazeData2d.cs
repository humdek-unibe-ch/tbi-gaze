/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
﻿using System.Numerics;

namespace GazeUtilityLibrary.DataStructs
{
    /// <summary>
    /// The 2d gaze data set.
    /// </summary>
    public class GazeData2d
    {
        private Vector2 _gazePoint;
        /// <summary>
        /// The 2d gaze point.
        /// </summary>
        public Vector2 GazePoint { get { return _gazePoint; } }

        private bool _isGazePointValid;
        /// <summary>
        /// The validity flag of the 2d gaze point.
        /// </summary>
        public bool IsGazePointValid { get { return _isGazePointValid; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="GazeData2d"/> class.
        /// </summary>
        /// <param name="gazePoint">The 2d coordinates of the gaze point.</param>
        /// <param name="isGazePointValid">The validity of the 2d gaze point.</param>
        public GazeData2d(Vector2 gazePoint, bool isGazePointValid)
        {
            _gazePoint = gazePoint;
            _isGazePointValid = isGazePointValid;
        }
    }
}
