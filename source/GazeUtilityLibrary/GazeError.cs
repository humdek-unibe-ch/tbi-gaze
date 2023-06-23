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

    public class GazeConfigError : GazeError
    {

        private EGazeConfigError _error = 0;
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
    public class GazeDataError : GazeError
    {

        private EGazeDataError _error = 0;
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
    public class CalibrationDataError : GazeError
    {

        private ECalibrationDataError _error = 0;
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