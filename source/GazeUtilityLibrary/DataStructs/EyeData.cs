/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
﻿
namespace GazeUtilityLibrary.DataStructs
{
    /// <summary>
    /// The eye data set, including pupil information.
    /// </summary>
    public class EyeData
    {
        private float _pupilDiameter;
        /// <summary>
        /// The diameter of the pupil
        /// </summary>
        public float PupilDiameter { get { return _pupilDiameter; } }

        private bool _isPupilDiameterValid;
        /// <summary>
        /// The validity flag of th epupil diameter
        /// </summary>
        public bool IsPupilDiameterValid { get { return _isPupilDiameterValid; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="EyeData"/> class.
        /// </summary>
        /// <param name="pupilDiameter">The pupil diameter.</param>
        /// <param name="isPupilDiameterValid">The validity of the pupil diameter.</param>
        public EyeData(float pupilDiameter, bool isPupilDiameterValid)
        {
            _pupilDiameter = pupilDiameter;
            _isPupilDiameterValid = isPupilDiameterValid;
        }
    }
}
