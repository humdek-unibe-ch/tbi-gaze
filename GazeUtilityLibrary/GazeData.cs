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
        /// Called when [gaze data received].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="data">The data.</param>
        static public string[] PrepareGazeData(GazeDataArgs data, ConfigItem Config, ref TimeSpan? delta)
        {
            string[] formatted_values = new string[Enum.GetNames(typeof(GazeOutputValue)).Length];
            // write the coordinates to the log file
            if (Config.DataLogWriteOutput)
            {
                formatted_values[(int)GazeOutputValue.DataTimeStamp] = GetValueString(data.Timestamp, Config.DataLogFormatTimeStamp, ref delta);

                formatted_values[(int)GazeOutputValue.CombinedGazePoint2dCompensatedX] = GetValueString(data.DriftCompensation?.GazePosition2d.X, Config.DataLogFormatNormalizedPoint);
                formatted_values[(int)GazeOutputValue.CombinedGazePoint2dCompensatedY] = GetValueString(data.DriftCompensation?.GazePosition2d.Y, Config.DataLogFormatNormalizedPoint);
                formatted_values[(int)GazeOutputValue.CombinedGazePoint2dX] = GetValueString(data.Combined.GazeData2d.GazePoint.X, Config.DataLogFormatNormalizedPoint);
                formatted_values[(int)GazeOutputValue.CombinedGazePoint2dY] = GetValueString(data.Combined.GazeData2d.GazePoint.Y, Config.DataLogFormatNormalizedPoint);
                formatted_values[(int)GazeOutputValue.CombinedGazePoint2dIsValid] = GetValueString(data.Combined.GazeData2d.IsGazePointValid);
                formatted_values[(int)GazeOutputValue.CombinedGazePoint3dCompensatedX] = GetValueString(data.DriftCompensation?.GazePosition3d.X, Config.DataLogFormatOrigin);
                formatted_values[(int)GazeOutputValue.CombinedGazePoint3dCompensatedY] = GetValueString(data.DriftCompensation?.GazePosition3d.Y, Config.DataLogFormatOrigin);
                formatted_values[(int)GazeOutputValue.CombinedGazePoint3dCompensatedZ] = GetValueString(data.DriftCompensation?.GazePosition3d.Z, Config.DataLogFormatOrigin);
                formatted_values[(int)GazeOutputValue.CombinedGazePoint3dX] = GetValueString(data.Combined.GazeData3d?.GazePoint.X, Config.DataLogFormatOrigin);
                formatted_values[(int)GazeOutputValue.CombinedGazePoint3dY] = GetValueString(data.Combined.GazeData3d?.GazePoint.Y, Config.DataLogFormatOrigin);
                formatted_values[(int)GazeOutputValue.CombinedGazePoint3dZ] = GetValueString(data.Combined.GazeData3d?.GazePoint.Z, Config.DataLogFormatOrigin);
                formatted_values[(int)GazeOutputValue.CombinedGazePoint3dIsValid] = GetValueString(data.Combined.GazeData3d?.IsGazePointValid);
                formatted_values[(int)GazeOutputValue.CombinedGazeOrigin3dX] = GetValueString(data.Combined.GazeData3d?.GazeOrigin.X, Config.DataLogFormatOrigin);
                formatted_values[(int)GazeOutputValue.CombinedGazeOrigin3dY] = GetValueString(data.Combined.GazeData3d?.GazeOrigin.Y, Config.DataLogFormatOrigin);
                formatted_values[(int)GazeOutputValue.CombinedGazeOrigin3dZ] = GetValueString(data.Combined.GazeData3d?.GazeOrigin.Z, Config.DataLogFormatOrigin);
                formatted_values[(int)GazeOutputValue.CombinedGazeOrigin3dIsValid] = GetValueString(data.Combined.GazeData3d?.IsGazeOriginValid);
                formatted_values[(int)GazeOutputValue.CombinedGazeDistance] = GetValueString(data.Combined.GazeData3d?.GazeDistance, Config.DataLogFormatOrigin);
                formatted_values[(int)GazeOutputValue.CombinedPupilDiameter] = GetValueString(data.Combined.EyeData?.PupilDiameter, Config.DataLogFormatDiameter);
                formatted_values[(int)GazeOutputValue.CombinedPupilDiameterIsValid] = GetValueString(data.Combined.EyeData?.IsPupilDiameterValid);

                formatted_values[(int)GazeOutputValue.LeftGazePoint2dX] = GetValueString(data.Left?.GazeData2d.GazePoint.X, Config.DataLogFormatNormalizedPoint);
                formatted_values[(int)GazeOutputValue.LeftGazePoint2dY] = GetValueString(data.Left?.GazeData2d.GazePoint.Y, Config.DataLogFormatNormalizedPoint);
                formatted_values[(int)GazeOutputValue.LeftGazePoint2dIsValid] = GetValueString(data.Left?.GazeData2d.IsGazePointValid);
                formatted_values[(int)GazeOutputValue.LeftGazePoint3dX] = GetValueString(data.Left?.GazeData3d?.GazeOrigin.X, Config.DataLogFormatOrigin);
                formatted_values[(int)GazeOutputValue.LeftGazePoint3dY] = GetValueString(data.Left?.GazeData3d?.GazeOrigin.Y, Config.DataLogFormatOrigin);
                formatted_values[(int)GazeOutputValue.LeftGazePoint3dZ] = GetValueString(data.Left?.GazeData3d?.GazeOrigin.Z, Config.DataLogFormatOrigin);
                formatted_values[(int)GazeOutputValue.LeftGazePoint3dIsValid] = GetValueString(data.Left?.GazeData3d?.IsGazePointValid);
                formatted_values[(int)GazeOutputValue.LeftGazeOrigin3dX] = GetValueString(data.Left?.GazeData3d?.GazeOrigin.X, Config.DataLogFormatOrigin);
                formatted_values[(int)GazeOutputValue.LeftGazeOrigin3dY] = GetValueString(data.Left?.GazeData3d?.GazeOrigin.Y, Config.DataLogFormatOrigin);
                formatted_values[(int)GazeOutputValue.LeftGazeOrigin3dZ] = GetValueString(data.Left?.GazeData3d?.GazeOrigin.Z, Config.DataLogFormatOrigin);
                formatted_values[(int)GazeOutputValue.LeftGazeOrigin3dIsValid] = GetValueString(data.Left?.GazeData3d?.IsGazeOriginValid);
                formatted_values[(int)GazeOutputValue.LeftGazeDistance] = GetValueString(data.Left?.GazeData3d?.GazeDistance, Config.DataLogFormatOrigin);
                formatted_values[(int)GazeOutputValue.LeftPupilDiameter] = GetValueString(data.Left?.EyeData?.PupilDiameter, Config.DataLogFormatDiameter);
                formatted_values[(int)GazeOutputValue.LeftPupilDiameterIsValid] = GetValueString(data.Left?.EyeData?.IsPupilDiameterValid);

                formatted_values[(int)GazeOutputValue.RightGazePoint2dX] = GetValueString(data.Right?.GazeData2d.GazePoint.X, Config.DataLogFormatNormalizedPoint);
                formatted_values[(int)GazeOutputValue.RightGazePoint2dY] = GetValueString(data.Right?.GazeData2d.GazePoint.Y, Config.DataLogFormatNormalizedPoint);
                formatted_values[(int)GazeOutputValue.RightGazePoint2dIsValid] = GetValueString(data.Right?.GazeData2d.IsGazePointValid);
                formatted_values[(int)GazeOutputValue.RightGazePoint3dX] = GetValueString(data.Right?.GazeData3d?.GazeOrigin.X, Config.DataLogFormatOrigin);
                formatted_values[(int)GazeOutputValue.RightGazePoint3dY] = GetValueString(data.Right?.GazeData3d?.GazeOrigin.Y, Config.DataLogFormatOrigin);
                formatted_values[(int)GazeOutputValue.RightGazePoint3dZ] = GetValueString(data.Right?.GazeData3d?.GazeOrigin.Z, Config.DataLogFormatOrigin);
                formatted_values[(int)GazeOutputValue.RightGazePoint3dIsValid] = GetValueString(data.Right?.GazeData3d?.IsGazePointValid);
                formatted_values[(int)GazeOutputValue.RightGazeOrigin3dX] = GetValueString(data.Right?.GazeData3d?.GazeOrigin.X, Config.DataLogFormatOrigin);
                formatted_values[(int)GazeOutputValue.RightGazeOrigin3dY] = GetValueString(data.Right?.GazeData3d?.GazeOrigin.Y, Config.DataLogFormatOrigin);
                formatted_values[(int)GazeOutputValue.RightGazeOrigin3dZ] = GetValueString(data.Right?.GazeData3d?.GazeOrigin.Z, Config.DataLogFormatOrigin);
                formatted_values[(int)GazeOutputValue.RightGazeOrigin3dIsValid] = GetValueString(data.Right?.GazeData3d?.IsGazeOriginValid);
                formatted_values[(int)GazeOutputValue.RightGazeDistance] = GetValueString(data.Right?.GazeData3d?.GazeDistance, Config.DataLogFormatOrigin);
                formatted_values[(int)GazeOutputValue.RightPupilDiameter] = GetValueString(data.Right?.EyeData?.PupilDiameter, Config.DataLogFormatDiameter);
                formatted_values[(int)GazeOutputValue.RightPupilDiameterIsValid] = GetValueString(data.Right?.EyeData?.IsPupilDiameterValid);
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
