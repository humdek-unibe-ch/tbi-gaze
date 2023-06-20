using System;
using System.Numerics;

namespace GazeUtilityLibrary.DataStructs
{
    /// <summary>
    /// The class definition of a gaze data set
    /// </summary>
    public class GazeData
    {
        private TimeSpan _timestamp;
        public TimeSpan Timestamp { get { return _timestamp; } }

        private GazeDataCollection? _left = null;
        public GazeDataCollection? Left { get { return _left; } }

        private GazeDataCollection? _right = null;
        public GazeDataCollection? Right { get { return _right; } }

        private GazeDataCollection _combined;
        public GazeDataCollection Combined { get { return _combined; } }
        public DriftCompensationData? DriftCompensation { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GazeDataArgs"/> class.
        /// </summary>
        /// <param name="timestamp">The timestamp.</param>
        /// <param name="gazePoint2d">The 2d coordinates of the combined gaze point.</param>
        /// <param name="isGazePoint2dValid">The validity of the combined 2d gaze point.</param>
        public GazeData(TimeSpan timestamp, Vector2 gazePoint2d, bool isGazePoint2dValid)
        {
            _timestamp = timestamp;
            _combined = new GazeDataCollection(gazePoint2d, isGazePoint2dValid);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GazeDataArgs"/> class.
        /// </summary>
        /// <param name="timestamp">The timestamp.</param>
        /// <param name="gazePoint2dLeft">The 2d coordinates of the left gaze point.</param>
        /// <param name="isGazePoint2dValidLeft">The validity of the left 2d gaze point.</param>
        /// <param name="gazePoint2dRight">The 2d coordinates of the right gaze point.</param>
        /// <param name="isGazePoint2dValidRight">The validity of the right 2d gaze point.</param>
        public GazeData(TimeSpan timestamp, Vector2 gazePoint2dLeft, bool isGazePoint2dValidLeft, Vector2 gazePoint2dRight, bool isGazePoint2dValidRight)
        {
            _timestamp = timestamp;
            _left = new GazeDataCollection(gazePoint2dLeft, isGazePoint2dValidLeft);
            _right = new GazeDataCollection(gazePoint2dRight, isGazePoint2dValidRight);
            Vector2 gazePoint2dCombined = new Vector2(GazeFilter(gazePoint2dLeft.X, gazePoint2dRight.X), GazeFilter(gazePoint2dLeft.Y, gazePoint2dRight.Y));
            _combined = new GazeDataCollection(gazePoint2dCombined, isGazePoint2dValidLeft & isGazePoint2dValidRight);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GazeDataArgs"/> class.
        /// </summary>
        /// <param name="timestamp">The timestamp.</param>
        /// <param name="gazePoint2dLeft">The 2d coordinates of the left gaze point.</param>
        /// <param name="isGazePoint2dValidLeft">The validity of the left 2d gaze point.</param>
        /// <param name="gazePoint2dRight">The 2d coordinates of the right gaze point.</param>
        /// <param name="isGazePoint2dValidRight">The validity of the right 2d gaze point.</param>
        /// <param name="gazePoint3dLeft">The 3d coordinates of the left gaze point.</param>
        /// <param name="isGazePoint3dValidLeft">The validity of the left 3d gaze point.</param>
        /// <param name="gazePoint3dRight">The 3d coordinates of the right gaze point.</param>
        /// <param name="isGazePoint3dValidRight">The validity of the right 3d gaze point.</param>
        /// <param name="gazeOrigin3dLeft">The 3d coordinates of the left gaze origin.</param>
        /// <param name="isGazeOrigin3dValidLeft">The validity of the left 3d gaze origin.</param>
        /// <param name="gazeOrigin3dRight">The 3d coordinates of the right gaze origin.</param>
        /// <param name="isGazeOrigin3dValidRight">The validity of the right 3d gaze origin.</param>
        /// <param name="pupilDiameterLeft">The pupil diameter the left eye.</param>
        /// <param name="isPupilDiameterValidLeft">The validity of the left pupil diameter.</param>
        /// <param name="pupilDiameterRight">The pupil diameter the left eye.</param>
        /// <param name="isPupilDiameterValidRight">The validity of the left pupil diameter.</param>
        public GazeData(TimeSpan timestamp, Vector2 gazePoint2dLeft, bool isGazePoint2dValidLeft, Vector2 gazePoint2dRight, bool isGazePoint2dValidRight,
            Vector3 gazePoint3dLeft, bool isGazePoint3dValidLeft, Vector3 gazePoint3dRight, bool isGazePoint3dValidRight,
            Vector3 gazeOrigin3dLeft, bool isGazeOrigin3dValidLeft, Vector3 gazeOrigin3dRight, bool isGazeOrigin3dValidRight,
            float pupilDiameterLeft, bool isPupilDiameterValidLeft, float pupilDiameterRight, bool isPupilDiameterValidRight)
        {
            _timestamp = timestamp;
            _left = new GazeDataCollection(gazePoint2dLeft, isGazePoint2dValidLeft, gazePoint3dLeft, isGazePoint3dValidLeft, gazeOrigin3dLeft, isGazeOrigin3dValidLeft, pupilDiameterLeft, isPupilDiameterValidLeft);
            _right = new GazeDataCollection(gazePoint2dRight, isGazePoint2dValidRight, gazePoint3dRight, isGazePoint3dValidRight, gazeOrigin3dRight, isGazeOrigin3dValidRight, pupilDiameterRight, isPupilDiameterValidRight);
            Vector2 gazePoint2dCombined = new Vector2(GazeFilter(gazePoint2dLeft.X, gazePoint2dRight.X), GazeFilter(gazePoint2dLeft.Y, gazePoint2dRight.Y));
            Vector3 gazePoint3dCombined = new Vector3(GazeFilter(gazePoint3dLeft.X, gazePoint3dRight.X), GazeFilter(gazePoint3dLeft.Y, gazePoint3dRight.Y), GazeFilter(gazePoint3dLeft.Z, gazePoint3dRight.Z));
            Vector3 gazeOrigin3dCombined = new Vector3(GazeFilter(gazeOrigin3dLeft.X, gazeOrigin3dRight.X), GazeFilter(gazeOrigin3dLeft.Y, gazeOrigin3dRight.Y), GazeFilter(gazeOrigin3dLeft.Z, gazeOrigin3dRight.Z));
            _combined = new GazeDataCollection(
                gazePoint2dCombined, isGazePoint2dValidLeft & isGazePoint2dValidRight,
                gazePoint3dCombined, isGazePoint3dValidLeft & isGazePoint3dValidRight,
                gazeOrigin3dCombined, isGazeOrigin3dValidLeft & isGazeOrigin3dValidRight,
                GazeFilter(pupilDiameterLeft, pupilDiameterRight), isPupilDiameterValidLeft & isPupilDiameterValidRight
            );
        }

        /// <summary>
        /// Combines the data values form the left and the right eye.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns></returns>
        private float GazeFilter(float left, float right)
        {
            if (float.IsNaN(left)) return right;
            if (float.IsNaN(right)) return left;
            return (left + right) / 2;
        }

        /// <summary>
        /// Called when [gaze data received].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="data">The data.</param>
        public string[] Prepare(ConfigItem config, ref TimeSpan? delta)
        {
            string[] formattedValues = new string[Enum.GetNames(typeof(GazeOutputValue)).Length];
            // write the coordinates to the log file
            if (config.DataLogWriteOutput)
            {
                formattedValues[(int)GazeOutputValue.DataTimeStamp] = GazeDataConverter.GetValueString(_timestamp, config.DataLogFormatTimeStamp, ref delta);

                formattedValues[(int)GazeOutputValue.CombinedGazePoint2dCompensatedX] = GazeDataConverter.GetValueString(DriftCompensation?.GazePosition2d.X, config.DataLogFormatNormalizedPoint);
                formattedValues[(int)GazeOutputValue.CombinedGazePoint2dCompensatedY] = GazeDataConverter.GetValueString(DriftCompensation?.GazePosition2d.Y, config.DataLogFormatNormalizedPoint);
                formattedValues[(int)GazeOutputValue.CombinedGazePoint2dX] = GazeDataConverter.GetValueString(_combined.GazeData2d.GazePoint.X, config.DataLogFormatNormalizedPoint);
                formattedValues[(int)GazeOutputValue.CombinedGazePoint2dY] = GazeDataConverter.GetValueString(_combined.GazeData2d.GazePoint.Y, config.DataLogFormatNormalizedPoint);
                formattedValues[(int)GazeOutputValue.CombinedGazePoint2dIsValid] = GazeDataConverter.GetValueString(_combined.GazeData2d.IsGazePointValid);
                formattedValues[(int)GazeOutputValue.CombinedGazePoint3dCompensatedX] = GazeDataConverter.GetValueString(DriftCompensation?.GazePosition3d.X, config.DataLogFormatOrigin);
                formattedValues[(int)GazeOutputValue.CombinedGazePoint3dCompensatedY] = GazeDataConverter.GetValueString(DriftCompensation?.GazePosition3d.Y, config.DataLogFormatOrigin);
                formattedValues[(int)GazeOutputValue.CombinedGazePoint3dCompensatedZ] = GazeDataConverter.GetValueString(DriftCompensation?.GazePosition3d.Z, config.DataLogFormatOrigin);
                formattedValues[(int)GazeOutputValue.CombinedGazePoint3dX] = GazeDataConverter.GetValueString(_combined.GazeData3d?.GazePoint.X, config.DataLogFormatOrigin);
                formattedValues[(int)GazeOutputValue.CombinedGazePoint3dY] = GazeDataConverter.GetValueString(_combined.GazeData3d?.GazePoint.Y, config.DataLogFormatOrigin);
                formattedValues[(int)GazeOutputValue.CombinedGazePoint3dZ] = GazeDataConverter.GetValueString(_combined.GazeData3d?.GazePoint.Z, config.DataLogFormatOrigin);
                formattedValues[(int)GazeOutputValue.CombinedGazePoint3dIsValid] = GazeDataConverter.GetValueString(_combined.GazeData3d?.IsGazePointValid);
                formattedValues[(int)GazeOutputValue.CombinedGazeOrigin3dX] = GazeDataConverter.GetValueString(_combined.GazeData3d?.GazeOrigin.X, config.DataLogFormatOrigin);
                formattedValues[(int)GazeOutputValue.CombinedGazeOrigin3dY] = GazeDataConverter.GetValueString(_combined.GazeData3d?.GazeOrigin.Y, config.DataLogFormatOrigin);
                formattedValues[(int)GazeOutputValue.CombinedGazeOrigin3dZ] = GazeDataConverter.GetValueString(_combined.GazeData3d?.GazeOrigin.Z, config.DataLogFormatOrigin);
                formattedValues[(int)GazeOutputValue.CombinedGazeOrigin3dIsValid] = GazeDataConverter.GetValueString(_combined.GazeData3d?.IsGazeOriginValid);
                formattedValues[(int)GazeOutputValue.CombinedGazeDistance] = GazeDataConverter.GetValueString(_combined.GazeData3d?.GazeDistance, config.DataLogFormatOrigin);
                formattedValues[(int)GazeOutputValue.CombinedPupilDiameter] = GazeDataConverter.GetValueString(_combined.EyeData?.PupilDiameter, config.DataLogFormatDiameter);
                formattedValues[(int)GazeOutputValue.CombinedPupilDiameterIsValid] = GazeDataConverter.GetValueString(_combined.EyeData?.IsPupilDiameterValid);

                formattedValues[(int)GazeOutputValue.LeftGazePoint2dX] = GazeDataConverter.GetValueString(_left?.GazeData2d.GazePoint.X, config.DataLogFormatNormalizedPoint);
                formattedValues[(int)GazeOutputValue.LeftGazePoint2dY] = GazeDataConverter.GetValueString(_left?.GazeData2d.GazePoint.Y, config.DataLogFormatNormalizedPoint);
                formattedValues[(int)GazeOutputValue.LeftGazePoint2dIsValid] = GazeDataConverter.GetValueString(_left?.GazeData2d.IsGazePointValid);
                formattedValues[(int)GazeOutputValue.LeftGazePoint3dX] = GazeDataConverter.GetValueString(_left?.GazeData3d?.GazeOrigin.X, config.DataLogFormatOrigin);
                formattedValues[(int)GazeOutputValue.LeftGazePoint3dY] = GazeDataConverter.GetValueString(_left?.GazeData3d?.GazeOrigin.Y, config.DataLogFormatOrigin);
                formattedValues[(int)GazeOutputValue.LeftGazePoint3dZ] = GazeDataConverter.GetValueString(_left?.GazeData3d?.GazeOrigin.Z, config.DataLogFormatOrigin);
                formattedValues[(int)GazeOutputValue.LeftGazePoint3dIsValid] = GazeDataConverter.GetValueString(_left?.GazeData3d?.IsGazePointValid);
                formattedValues[(int)GazeOutputValue.LeftGazeOrigin3dX] = GazeDataConverter.GetValueString(_left?.GazeData3d?.GazeOrigin.X, config.DataLogFormatOrigin);
                formattedValues[(int)GazeOutputValue.LeftGazeOrigin3dY] = GazeDataConverter.GetValueString(_left?.GazeData3d?.GazeOrigin.Y, config.DataLogFormatOrigin);
                formattedValues[(int)GazeOutputValue.LeftGazeOrigin3dZ] = GazeDataConverter.GetValueString(_left?.GazeData3d?.GazeOrigin.Z, config.DataLogFormatOrigin);
                formattedValues[(int)GazeOutputValue.LeftGazeOrigin3dIsValid] = GazeDataConverter.GetValueString(_left?.GazeData3d?.IsGazeOriginValid);
                formattedValues[(int)GazeOutputValue.LeftGazeDistance] = GazeDataConverter.GetValueString(_left?.GazeData3d?.GazeDistance, config.DataLogFormatOrigin);
                formattedValues[(int)GazeOutputValue.LeftPupilDiameter] = GazeDataConverter.GetValueString(_left?.EyeData?.PupilDiameter, config.DataLogFormatDiameter);
                formattedValues[(int)GazeOutputValue.LeftPupilDiameterIsValid] = GazeDataConverter.GetValueString(_left?.EyeData?.IsPupilDiameterValid);

                formattedValues[(int)GazeOutputValue.RightGazePoint2dX] = GazeDataConverter.GetValueString(_right?.GazeData2d.GazePoint.X, config.DataLogFormatNormalizedPoint);
                formattedValues[(int)GazeOutputValue.RightGazePoint2dY] = GazeDataConverter.GetValueString(_right?.GazeData2d.GazePoint.Y, config.DataLogFormatNormalizedPoint);
                formattedValues[(int)GazeOutputValue.RightGazePoint2dIsValid] = GazeDataConverter.GetValueString(_right?.GazeData2d.IsGazePointValid);
                formattedValues[(int)GazeOutputValue.RightGazePoint3dX] = GazeDataConverter.GetValueString(_right?.GazeData3d?.GazeOrigin.X, config.DataLogFormatOrigin);
                formattedValues[(int)GazeOutputValue.RightGazePoint3dY] = GazeDataConverter.GetValueString(_right?.GazeData3d?.GazeOrigin.Y, config.DataLogFormatOrigin);
                formattedValues[(int)GazeOutputValue.RightGazePoint3dZ] = GazeDataConverter.GetValueString(_right?.GazeData3d?.GazeOrigin.Z, config.DataLogFormatOrigin);
                formattedValues[(int)GazeOutputValue.RightGazePoint3dIsValid] = GazeDataConverter.GetValueString(_right?.GazeData3d?.IsGazePointValid);
                formattedValues[(int)GazeOutputValue.RightGazeOrigin3dX] = GazeDataConverter.GetValueString(_right?.GazeData3d?.GazeOrigin.X, config.DataLogFormatOrigin);
                formattedValues[(int)GazeOutputValue.RightGazeOrigin3dY] = GazeDataConverter.GetValueString(_right?.GazeData3d?.GazeOrigin.Y, config.DataLogFormatOrigin);
                formattedValues[(int)GazeOutputValue.RightGazeOrigin3dZ] = GazeDataConverter.GetValueString(_right?.GazeData3d?.GazeOrigin.Z, config.DataLogFormatOrigin);
                formattedValues[(int)GazeOutputValue.RightGazeOrigin3dIsValid] = GazeDataConverter.GetValueString(_right?.GazeData3d?.IsGazeOriginValid);
                formattedValues[(int)GazeOutputValue.RightGazeDistance] = GazeDataConverter.GetValueString(_right?.GazeData3d?.GazeDistance, config.DataLogFormatOrigin);
                formattedValues[(int)GazeOutputValue.RightPupilDiameter] = GazeDataConverter.GetValueString(_right?.EyeData?.PupilDiameter, config.DataLogFormatDiameter);
                formattedValues[(int)GazeOutputValue.RightPupilDiameterIsValid] = GazeDataConverter.GetValueString(_right?.EyeData?.IsPupilDiameterValid);
            }
            return formattedValues;
        }
    }
}
