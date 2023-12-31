/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
﻿using System;

namespace GazeUtilityLibrary
{
    /// <summary>
    /// Error values of the configuration
    /// </summary>
    public enum EGazeConfigError
    {
        FallbackToDefaultConfigName = 0x001,
        FallbackToCurrentOutputDir = 0x002,
        FallbackToDefaultConfig = 0x004,
        FallbackToDefaultDiameterFormat = 0x008,
        FallbackToDefaultOriginFormat = 0x010,
        FallbackToDefaultTimestampFormat = 0x020,
        OmitColumnTitles = 0x040,
        FallbackToDefualtColumnOrder = 0x080,
        FallbackToDefaultNormalizedFormat = 0x100
    }
    /// <summary>
    /// Error values of the gaze output data
    /// </summary>
    public enum EGazeDataError
    {
        FallbackToMouse = 0x01,
        DeviceInterrupt = 0x02
    }
    /// <summary>
    /// Error values of the gaze output data
    /// </summary>
    public enum ECalibrationDataError
    {
        DeviceNotSupported = 0x01,
        DeviceInterrupt = 0x02
    }

    /// <summary>
    /// The base error class to convert error flags to binary strings.
    /// </summary>
    public class GazeError
    {
        /// <summary>
        /// Converts a integer value to a binary string.
        /// </summary>
        /// <param name="val">The value.</param>
        /// <param name="len">The length of the binary string.</param>
        /// <returns>a binary string of specified length, left-padded with '0'</returns>
        protected string ConvertToBinString(int val, int len)
        {
            string val_bin = Convert.ToString(val, 2);
            return val_bin.PadLeft(len, '0');
        }
    }

    /// <summary>
    /// The gaze config error class to convert error flags to binary strings.
    /// </summary>
    public class GazeConfigError : GazeError
    {

        private EGazeConfigError _error = 0;
        /// <summary>
        /// The error flags.
        /// </summary>
        public EGazeConfigError Error { set { _error |= value; } }

        /// <summary>
        /// Gets the gaze error string.
        /// </summary>
        /// <returns>the error string with binary error values if errors ocurred, the empty srting otherwise</returns>
        public string GetGazeConfigErrorString()
        {
            int formatError = ((int)_error & 0xF8) >> 3;
            int configError = ((int)_error & 0x07);
            string confErrorStr = "_err"
                + $"-{ConvertToBinString(configError, 3)}"
                + $"-{ConvertToBinString(formatError, 5)}";
            if (_error == 0) confErrorStr = "";
            return confErrorStr;

        }
    }

    /// <summary>
    /// The gaze data error class to convert error flags to binary strings.
    /// </summary>
    public class GazeDataError : GazeError
    {

        private EGazeDataError _error = 0;
        /// <summary>
        /// The error flags.
        /// </summary>
        public EGazeDataError Error { set { _error |= value; } }

        /// <summary>
        /// Gets the gaze error string.
        /// </summary>
        /// <returns>the error string with binary error values if errors ocurred, the empty srting otherwise</returns>
        public string GetGazeDataErrorString()
        {
            string confErrorStr = $"_err-{ConvertToBinString((int)_error, 2)}";
            if (_error == 0) confErrorStr = "";
            return confErrorStr;

        }
    }

    /// <summary>
    /// The calibration data error class to convert error flags to binary strings.
    /// </summary>
    public class CalibrationDataError : GazeError
    {

        private ECalibrationDataError _error = 0;
        /// <summary>
        /// The error flags.
        /// </summary>
        public ECalibrationDataError Error { set { _error |= value; } }

        /// <summary>
        /// Gets the gaze error string.
        /// </summary>
        /// <returns>the error string with binary error values if errors ocurred, the empty srting otherwise</returns>
        public string GetCalibrationDataErrorString()
        {
            string confErrorStr = $"_err-{ConvertToBinString((int)_error, 2)}";
            if (_error == 0) confErrorStr = "";
            return confErrorStr;

        }
    }
}
