using System;

namespace GazeUtilityLibrary
{
    static public class GazeData
    {
        /// <summary>
        /// Gets the gaze data value string.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>a string containing the gaze data value if the value is not null, the empty string otherwise</returns>
        static private string GetValueString(bool? data)
        {
            return (data == null) ? "" : ((bool)data).ToString();
        }

        /// <summary>
        /// Gets the gaze data value string.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="format">The format of the data will be converted to.</param>
        /// <returns>
        /// a string containing the gaze data value if the value is not null, the empty string otherwise
        /// </returns>
        static private string GetValueString(double? data, string format = "")
        {
            return (data == null) ? "" : ((double)data).ToString(format);
        }

        /// <summary>
        /// Computes the eye tracker timestamp.
        /// </summary>
        /// <param name="ts">The timestamp.</param>
        /// <param name="format">The format the timestamp is converted to.</param>
        /// <returns>
        /// a string containing the timestamp 
        /// </returns>
        static private string GetValueString(TimeSpan ts, string format, ref TimeSpan? delta)
        {
            TimeSpan res = ts;
            if (delta == null)
            {
                delta = res - DateTime.Now.TimeOfDay;
            }
            res -= (TimeSpan)delta;
            return res.ToString(format);
        }

        /// <summary>
        /// Determines whether the gaze data set is valid.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="ignore_invalid">if set to <c>true</c> [ignore invalid].</param>
        /// <returns>
        ///   <c>true</c> if at least one value of the gaze data is valid; otherwise, <c>false</c>.
        /// </returns>
        static private bool IsGazeDataValid(GazeDataArgs data, bool ignore_invalid)
        {
            if (!ignore_invalid) return true; // don't check, log everything
            if ((data.IsValidCoordLeft == true)
                || (data.IsValidCoordRight == true)
                || (data.IsValidDiaLeft == true)
                || (data.IsValidDiaRight == true)
                || (data.IsValidOriginLeft == true)
                || (data.IsValidOriginRight == true)
                || ((data.IsValidCoordLeft == null)
                    && (data.IsValidCoordRight == null)
                    && (data.IsValidDiaLeft == null)
                    && (data.IsValidDiaRight == null)
                    && (data.IsValidOriginLeft == null)
                    && (data.IsValidOriginRight == null)))
                return true; // at least one value is valid or Core SDK is used
            else return false; // all vaules of this data set are invalid
        }

        /// <summary>
        /// Called when [gaze data received].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="data">The data.</param>
        static public string[] PrepareGazeData(GazeDataArgs data, ConfigItem Config, ref TimeSpan? delta)
        {
            string[] formatted_values = new string[Enum.GetNames(typeof(GazeOutputValue)).Length];
            // write the coordinates to the log file
            if (Config.DataLogWriteOutput && IsGazeDataValid(data, Config.DataLogIgnoreInvalid))
            {
                formatted_values[(int)GazeOutputValue.DataTimeStamp] = GetValueString(data.Timestamp, Config.DataLogFormatTimeStamp, ref delta);
                formatted_values[(int)GazeOutputValue.XCoord] = GetValueString(data.XCoord, Config.DataLogFormatNormalizedPoint);
                formatted_values[(int)GazeOutputValue.XCoordLeft] = GetValueString(data.XCoordLeft, Config.DataLogFormatNormalizedPoint);
                formatted_values[(int)GazeOutputValue.XCoordRight] = GetValueString(data.XCoordRight, Config.DataLogFormatNormalizedPoint);
                formatted_values[(int)GazeOutputValue.YCoord] = GetValueString(data.YCoord, Config.DataLogFormatNormalizedPoint);
                formatted_values[(int)GazeOutputValue.YCoordLeft] = GetValueString(data.YCoordLeft, Config.DataLogFormatNormalizedPoint);
                formatted_values[(int)GazeOutputValue.YCoordRight] = GetValueString(data.YCoordRight, Config.DataLogFormatNormalizedPoint);
                formatted_values[(int)GazeOutputValue.ValidCoordLeft] = GetValueString(data.IsValidCoordLeft);
                formatted_values[(int)GazeOutputValue.ValidCoordRight] = GetValueString(data.IsValidCoordRight);
                formatted_values[(int)GazeOutputValue.PupilDia] = GetValueString(data.Dia, Config.DataLogFormatDiameter);
                formatted_values[(int)GazeOutputValue.PupilDiaLeft] = GetValueString(data.DiaLeft, Config.DataLogFormatDiameter);
                formatted_values[(int)GazeOutputValue.PupilDiaRight] = GetValueString(data.DiaRight, Config.DataLogFormatDiameter);
                formatted_values[(int)GazeOutputValue.ValidPupilLeft] = GetValueString(data.IsValidDiaLeft);
                formatted_values[(int)GazeOutputValue.ValidPupilRight] = GetValueString(data.IsValidDiaRight);
                formatted_values[(int)GazeOutputValue.XOriginLeft] = GetValueString(data.XOriginLeft, Config.DataLogFormatOrigin);
                formatted_values[(int)GazeOutputValue.XOriginRight] = GetValueString(data.XOriginRight, Config.DataLogFormatOrigin);
                formatted_values[(int)GazeOutputValue.YOriginLeft] = GetValueString(data.YOriginLeft, Config.DataLogFormatOrigin);
                formatted_values[(int)GazeOutputValue.YOriginRight] = GetValueString(data.YOriginRight, Config.DataLogFormatOrigin);
                formatted_values[(int)GazeOutputValue.ZOriginLeft] = GetValueString(data.ZOriginLeft, Config.DataLogFormatOrigin);
                formatted_values[(int)GazeOutputValue.ZOriginRight] = GetValueString(data.ZOriginRight, Config.DataLogFormatOrigin);
                formatted_values[(int)GazeOutputValue.DistOrigin] = GetValueString(data.DistOrigin, Config.DataLogFormatOrigin);
                formatted_values[(int)GazeOutputValue.DistOriginLeft] = GetValueString(data.DistOriginLeft, Config.DataLogFormatOrigin);
                formatted_values[(int)GazeOutputValue.DistOriginRight] = GetValueString(data.DistOriginRight, Config.DataLogFormatOrigin);
                formatted_values[(int)GazeOutputValue.ValidOriginLeft] = GetValueString(data.IsValidOriginLeft);
                formatted_values[(int)GazeOutputValue.ValidOriginRight] = GetValueString(data.IsValidOriginRight);
            }
            return formatted_values;
        }

        /// <summary>
        /// Called when [gaze data received].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="data">The data.</param>
        static public string[] PrepareCalibrationData(CalibrationDataArgs data, ConfigItem Config)
        {
            string[] formatted_values = new string[Enum.GetNames(typeof(CalibrationOutputValue)).Length];
            // write the coordinates to the log file
            if (Config.DataLogWriteOutput)
            {
                formatted_values[(int)CalibrationOutputValue.XCoord] = GetValueString(data.XCoord, Config.DataLogFormatNormalizedPoint);
                formatted_values[(int)CalibrationOutputValue.YCoord] = GetValueString(data.YCoord, Config.DataLogFormatNormalizedPoint);
                formatted_values[(int)CalibrationOutputValue.XCoordLeft] = GetValueString(data.XCoordLeft, Config.DataLogFormatNormalizedPoint);
                formatted_values[(int)CalibrationOutputValue.YCoordLeft] = GetValueString(data.YCoordLeft, Config.DataLogFormatNormalizedPoint);
                formatted_values[(int)CalibrationOutputValue.ValidCoordLeft] = GetValueString(data.ValidityLeft);
                formatted_values[(int)CalibrationOutputValue.XCoordRight] = GetValueString(data.XCoordRight, Config.DataLogFormatNormalizedPoint);
                formatted_values[(int)CalibrationOutputValue.YCoordRight] = GetValueString(data.YCoordRight, Config.DataLogFormatNormalizedPoint);
                formatted_values[(int)CalibrationOutputValue.ValidCoordRight] = GetValueString(data.ValidityRight);
            }
            return formatted_values;
        }
    }
}
