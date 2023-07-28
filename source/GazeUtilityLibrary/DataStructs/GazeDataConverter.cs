using System;

namespace GazeUtilityLibrary.DataStructs
{
    /// <summary>
    /// enummerates output values produced by the eyetracker
    /// </summary>
    public enum GazeOutputValue
    {
        DataTimeStamp = 0, // timestamp of the gaze data item (uses ValueFormat.TimeStamp)

        CombinedGazePoint2dCompensatedX,
        CombinedGazePoint2dCompensatedY,
        CombinedGazePoint2dX,
        CombinedGazePoint2dY,
        CombinedGazePoint2dIsValid,
        CombinedGazePoint3dCompensatedX,
        CombinedGazePoint3dCompensatedY,
        CombinedGazePoint3dCompensatedZ,
        CombinedGazePoint3dX,
        CombinedGazePoint3dY,
        CombinedGazePoint3dZ,
        CombinedGazePoint3dIsValid,
        CombinedGazeOrigin3dX,
        CombinedGazeOrigin3dY,
        CombinedGazeOrigin3dZ,
        CombinedGazeOrigin3dIsValid,
        CombinedGazeDistance,
        CombinedPupilDiameter,
        CombinedPupilDiameterIsValid,

        LeftGazePoint2dX,
        LeftGazePoint2dY,
        LeftGazePoint2dIsValid,
        LeftGazePoint3dX,
        LeftGazePoint3dY,
        LeftGazePoint3dZ,
        LeftGazePoint3dIsValid,
        LeftGazeOrigin3dX,
        LeftGazeOrigin3dY,
        LeftGazeOrigin3dZ,
        LeftGazeOrigin3dIsValid,
        LeftGazeDistance,
        LeftPupilDiameter,
        LeftPupilDiameterIsValid,

        RightGazePoint2dX,
        RightGazePoint2dY,
        RightGazePoint2dIsValid,
        RightGazePoint3dX,
        RightGazePoint3dY,
        RightGazePoint3dZ,
        RightGazePoint3dIsValid,
        RightGazeOrigin3dX,
        RightGazeOrigin3dY,
        RightGazeOrigin3dZ,
        RightGazeOrigin3dIsValid,
        RightGazeDistance,
        RightPupilDiameter,
        RightPupilDiameterIsValid,

        Tag
    }

    /// <summary>
    /// enummerates output values produced by the eyetracker
    /// </summary>
    public enum CalibrationOutputValue
    {
        XCoord, // x-coordinate of the calibration point (normalised value)
        YCoord, // y-coordinate of the gaze calibration (normalised value)
        XCoordLeft, // x-coordinate of the gaze point of the left eye (normalised value)
        YCoordLeft, // y-coordinate of the gaze point of the left eye (normalised value)
        ValidCoordLeft, // validity of the gaze data of the left eye
        XCoordRight, // x-coordinate of the gaze point of the right eye (normalised value)
        YCoordRight, // y-coordinate of the gaze point of the right eye (normalised value)
        ValidCoordRight // validity of the gaze data of the right eye
    }

    static public class GazeDataConverter
    {
        /// <summary>
        /// Gets the gaze data value string.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>a string containing the gaze data value if the value is not null, the empty string otherwise</returns>
        static public string GetValueString(bool? data)
        {
            return data == null ? "" : ((bool)data).ToString();
        }

        /// <summary>
        /// Gets the gaze data value string.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="format">The format of the data will be converted to.</param>
        /// <returns>
        /// a string containing the gaze data value if the value is not null, the empty string otherwise
        /// </returns>
        static public string GetValueString(double? data, string format = "")
        {
            return data == null ? "" : ((double)data).ToString(format);
        }

        /// <summary>
        /// Computes the eye tracker timestamp.
        /// </summary>
        /// <param name="ts">The timestamp.</param>
        /// <param name="format">The format the timestamp is converted to.</param>
        /// <returns>
        /// a string containing the timestamp 
        /// </returns>
        static public string GetValueString(TimeSpan ts, string format, ref TimeSpan? delta)
        {
            TimeSpan res = ts;
            if (delta == null)
            {
                delta = res - DateTime.Now.TimeOfDay;
            }
            res -= (TimeSpan)delta;
            return res.ToString(format);
        }
    }
}
