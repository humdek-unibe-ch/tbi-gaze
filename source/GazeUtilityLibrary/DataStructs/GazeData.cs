using Newtonsoft.Json.Linq;
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
        /// <summary>
        /// The timestamp of the data sample.
        /// </summary>
        public TimeSpan Timestamp { get { return _timestamp; } }

        private long _timestampDevice;
        /// <summary>
        /// The device timestamp of the data sample.
        /// </summary>
        public long TimestampDevice { get { return _timestampDevice; } }

        private GazeDataCollection? _left = null;
        /// <summary>
        /// The gaze data set, including 2d and (optionally) 3d gaze data as well as optional eye data of the left eye.
        /// </summary>
        public GazeDataCollection? Left { get { return _left; } }

        private GazeDataCollection? _right = null;
        /// <summary>
        /// The gaze data set, including 2d and (optionally) 3d gaze data as well as optional eye data of the right eye.
        /// </summary>
        public GazeDataCollection? Right { get { return _right; } }

        private GazeDataCollection _combined;
        /// <summary>
        /// The gaze data set, including 2d and (optionally) 3d gaze data as well as optional eye data of the combined eyes.
        /// </summary>
        public GazeDataCollection Combined { get { return _combined; } }

        /// <summary>
        /// The drift compensation information.
        /// </summary>
        public DriftCompensationData? DriftCompensation { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GazeDataArgs"/> class.
        /// </summary>
        /// <param name="timestamp">The timestamp.</param>
        /// <param name="timestampDevice">The device timestamp.</param>
        /// <param name="gazePoint2d">The 2d coordinates of the combined gaze point.</param>
        /// <param name="isGazePoint2dValid">The validity of the combined 2d gaze point.</param>
        public GazeData(TimeSpan timestamp, long timestampDevice, Vector2 gazePoint2d, bool isGazePoint2dValid)
        {
            _timestamp = timestamp;
            _timestampDevice = timestampDevice;
            _combined = new GazeDataCollection(gazePoint2d, isGazePoint2dValid);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GazeDataArgs"/> class.
        /// </summary>
        /// <param name="timestamp">The timestamp.</param>
        /// <param name="timestampDevice">The device timestamp.</param>
        /// <param name="gazePoint2dLeft">The 2d coordinates of the left gaze point.</param>
        /// <param name="isGazePoint2dValidLeft">The validity of the left 2d gaze point.</param>
        /// <param name="gazePoint2dRight">The 2d coordinates of the right gaze point.</param>
        /// <param name="isGazePoint2dValidRight">The validity of the right 2d gaze point.</param>
        public GazeData(TimeSpan timestamp, long timestampDevice, Vector2 gazePoint2dLeft, bool isGazePoint2dValidLeft, Vector2 gazePoint2dRight, bool isGazePoint2dValidRight)
        {
            _timestamp = timestamp;
            _timestampDevice = timestampDevice;
            _left = new GazeDataCollection(gazePoint2dLeft, isGazePoint2dValidLeft);
            _right = new GazeDataCollection(gazePoint2dRight, isGazePoint2dValidRight);
            Vector2 gazePoint2dCombined = new Vector2(GazeFilter(gazePoint2dLeft.X, gazePoint2dRight.X), GazeFilter(gazePoint2dLeft.Y, gazePoint2dRight.Y));
            _combined = new GazeDataCollection(gazePoint2dCombined, isGazePoint2dValidLeft & isGazePoint2dValidRight);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GazeDataArgs"/> class.
        /// </summary>
        /// <param name="timestamp">The timestamp.</param>
        /// <param name="timestampDevice">The device timestamp.</param>
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
        public GazeData(TimeSpan timestamp, long timestampDevice, Vector2 gazePoint2dLeft, bool isGazePoint2dValidLeft, Vector2 gazePoint2dRight, bool isGazePoint2dValidRight,
            Vector3 gazePoint3dLeft, bool isGazePoint3dValidLeft, Vector3 gazePoint3dRight, bool isGazePoint3dValidRight,
            Vector3 gazeOrigin3dLeft, bool isGazeOrigin3dValidLeft, Vector3 gazeOrigin3dRight, bool isGazeOrigin3dValidRight,
            float pupilDiameterLeft, bool isPupilDiameterValidLeft, float pupilDiameterRight, bool isPupilDiameterValidRight)
        {

            _timestamp = timestamp;
            _timestampDevice = timestampDevice;
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
        /// Prepare a list of formatted gaze data values
        /// </summary>
        /// <param name="config">The gaze configuration structure</param>
        /// <param name="trialId">The ID of the current trial.</param>
        /// <param name="tag">An arbitrary tag to associate with the data sample.</param>
        /// <param name="startTime">The system time to use toi compute the relative timestamp</param>
        /// <returns>A list of formatted values. Each index corresponds to a specific value. This allows to reorder the list according to a format string.</returns>
        public string[] Prepare(ConfigItem config, int trialId, string tag, TimeSpan startTime)
        {
            string[] formattedValues = new string[Enum.GetNames(typeof(GazeOutputValue)).Length];

            formattedValues[(int)GazeOutputValue.DataTimeStamp] = GazeDataConverter.FormatTimestamp(_timestamp, config.DataLogFormatTimeStamp);
            formattedValues[(int)GazeOutputValue.DataTimeStampRelative] = GazeDataConverter.FormatDouble((_timestamp - startTime).TotalMilliseconds, config.DataLogFormatTimeStampRelative);
            formattedValues[(int)GazeOutputValue.DataTimeStampDevice] = GazeDataConverter.FormatDouble(_timestampDevice);

            formattedValues[(int)GazeOutputValue.CombinedGazePoint2dCompensatedX] = GazeDataConverter.FormatDouble(DriftCompensation?.GazePosition2d.X, config.DataLogFormatNormalizedPoint);
            formattedValues[(int)GazeOutputValue.CombinedGazePoint2dCompensatedY] = GazeDataConverter.FormatDouble(DriftCompensation?.GazePosition2d.Y, config.DataLogFormatNormalizedPoint);
            formattedValues[(int)GazeOutputValue.CombinedGazePoint2dX] = GazeDataConverter.FormatDouble(_combined.GazeData2d.GazePoint.X, config.DataLogFormatNormalizedPoint);
            formattedValues[(int)GazeOutputValue.CombinedGazePoint2dY] = GazeDataConverter.FormatDouble(_combined.GazeData2d.GazePoint.Y, config.DataLogFormatNormalizedPoint);
            formattedValues[(int)GazeOutputValue.CombinedGazePoint2dIsValid] = GazeDataConverter.FormatBoolean(_combined.GazeData2d.IsGazePointValid);
            formattedValues[(int)GazeOutputValue.CombinedGazePoint3dCompensatedX] = GazeDataConverter.FormatDouble(DriftCompensation?.GazePosition3d.X, config.DataLogFormatOrigin);
            formattedValues[(int)GazeOutputValue.CombinedGazePoint3dCompensatedY] = GazeDataConverter.FormatDouble(DriftCompensation?.GazePosition3d.Y, config.DataLogFormatOrigin);
            formattedValues[(int)GazeOutputValue.CombinedGazePoint3dCompensatedZ] = GazeDataConverter.FormatDouble(DriftCompensation?.GazePosition3d.Z, config.DataLogFormatOrigin);
            formattedValues[(int)GazeOutputValue.CombinedGazePoint3dX] = GazeDataConverter.FormatDouble(_combined.GazeData3d?.GazePoint.X, config.DataLogFormatOrigin);
            formattedValues[(int)GazeOutputValue.CombinedGazePoint3dY] = GazeDataConverter.FormatDouble(_combined.GazeData3d?.GazePoint.Y, config.DataLogFormatOrigin);
            formattedValues[(int)GazeOutputValue.CombinedGazePoint3dZ] = GazeDataConverter.FormatDouble(_combined.GazeData3d?.GazePoint.Z, config.DataLogFormatOrigin);
            formattedValues[(int)GazeOutputValue.CombinedGazePoint3dIsValid] = GazeDataConverter.FormatBoolean(_combined.GazeData3d?.IsGazePointValid);
            formattedValues[(int)GazeOutputValue.CombinedGazeOrigin3dX] = GazeDataConverter.FormatDouble(_combined.GazeData3d?.GazeOrigin.X, config.DataLogFormatOrigin);
            formattedValues[(int)GazeOutputValue.CombinedGazeOrigin3dY] = GazeDataConverter.FormatDouble(_combined.GazeData3d?.GazeOrigin.Y, config.DataLogFormatOrigin);
            formattedValues[(int)GazeOutputValue.CombinedGazeOrigin3dZ] = GazeDataConverter.FormatDouble(_combined.GazeData3d?.GazeOrigin.Z, config.DataLogFormatOrigin);
            formattedValues[(int)GazeOutputValue.CombinedGazeOrigin3dIsValid] = GazeDataConverter.FormatBoolean(_combined.GazeData3d?.IsGazeOriginValid);
            formattedValues[(int)GazeOutputValue.CombinedGazeDistance] = GazeDataConverter.FormatDouble(_combined.GazeData3d?.GazeDistance, config.DataLogFormatOrigin);
            formattedValues[(int)GazeOutputValue.CombinedPupilDiameter] = GazeDataConverter.FormatDouble(_combined.EyeData?.PupilDiameter, config.DataLogFormatDiameter);
            formattedValues[(int)GazeOutputValue.CombinedPupilDiameterIsValid] = GazeDataConverter.FormatBoolean(_combined.EyeData?.IsPupilDiameterValid);

            formattedValues[(int)GazeOutputValue.LeftGazePoint2dX] = GazeDataConverter.FormatDouble(_left?.GazeData2d.GazePoint.X, config.DataLogFormatNormalizedPoint);
            formattedValues[(int)GazeOutputValue.LeftGazePoint2dY] = GazeDataConverter.FormatDouble(_left?.GazeData2d.GazePoint.Y, config.DataLogFormatNormalizedPoint);
            formattedValues[(int)GazeOutputValue.LeftGazePoint2dIsValid] = GazeDataConverter.FormatBoolean(_left?.GazeData2d.IsGazePointValid);
            formattedValues[(int)GazeOutputValue.LeftGazePoint3dX] = GazeDataConverter.FormatDouble(_left?.GazeData3d?.GazeOrigin.X, config.DataLogFormatOrigin);
            formattedValues[(int)GazeOutputValue.LeftGazePoint3dY] = GazeDataConverter.FormatDouble(_left?.GazeData3d?.GazeOrigin.Y, config.DataLogFormatOrigin);
            formattedValues[(int)GazeOutputValue.LeftGazePoint3dZ] = GazeDataConverter.FormatDouble(_left?.GazeData3d?.GazeOrigin.Z, config.DataLogFormatOrigin);
            formattedValues[(int)GazeOutputValue.LeftGazePoint3dIsValid] = GazeDataConverter.FormatBoolean(_left?.GazeData3d?.IsGazePointValid);
            formattedValues[(int)GazeOutputValue.LeftGazeOrigin3dX] = GazeDataConverter.FormatDouble(_left?.GazeData3d?.GazeOrigin.X, config.DataLogFormatOrigin);
            formattedValues[(int)GazeOutputValue.LeftGazeOrigin3dY] = GazeDataConverter.FormatDouble(_left?.GazeData3d?.GazeOrigin.Y, config.DataLogFormatOrigin);
            formattedValues[(int)GazeOutputValue.LeftGazeOrigin3dZ] = GazeDataConverter.FormatDouble(_left?.GazeData3d?.GazeOrigin.Z, config.DataLogFormatOrigin);
            formattedValues[(int)GazeOutputValue.LeftGazeOrigin3dIsValid] = GazeDataConverter.FormatBoolean(_left?.GazeData3d?.IsGazeOriginValid);
            formattedValues[(int)GazeOutputValue.LeftGazeDistance] = GazeDataConverter.FormatDouble(_left?.GazeData3d?.GazeDistance, config.DataLogFormatOrigin);
            formattedValues[(int)GazeOutputValue.LeftPupilDiameter] = GazeDataConverter.FormatDouble(_left?.EyeData?.PupilDiameter, config.DataLogFormatDiameter);
            formattedValues[(int)GazeOutputValue.LeftPupilDiameterIsValid] = GazeDataConverter.FormatBoolean(_left?.EyeData?.IsPupilDiameterValid);

            formattedValues[(int)GazeOutputValue.RightGazePoint2dX] = GazeDataConverter.FormatDouble(_right?.GazeData2d.GazePoint.X, config.DataLogFormatNormalizedPoint);
            formattedValues[(int)GazeOutputValue.RightGazePoint2dY] = GazeDataConverter.FormatDouble(_right?.GazeData2d.GazePoint.Y, config.DataLogFormatNormalizedPoint);
            formattedValues[(int)GazeOutputValue.RightGazePoint2dIsValid] = GazeDataConverter.FormatBoolean(_right?.GazeData2d.IsGazePointValid);
            formattedValues[(int)GazeOutputValue.RightGazePoint3dX] = GazeDataConverter.FormatDouble(_right?.GazeData3d?.GazeOrigin.X, config.DataLogFormatOrigin);
            formattedValues[(int)GazeOutputValue.RightGazePoint3dY] = GazeDataConverter.FormatDouble(_right?.GazeData3d?.GazeOrigin.Y, config.DataLogFormatOrigin);
            formattedValues[(int)GazeOutputValue.RightGazePoint3dZ] = GazeDataConverter.FormatDouble(_right?.GazeData3d?.GazeOrigin.Z, config.DataLogFormatOrigin);
            formattedValues[(int)GazeOutputValue.RightGazePoint3dIsValid] = GazeDataConverter.FormatBoolean(_right?.GazeData3d?.IsGazePointValid);
            formattedValues[(int)GazeOutputValue.RightGazeOrigin3dX] = GazeDataConverter.FormatDouble(_right?.GazeData3d?.GazeOrigin.X, config.DataLogFormatOrigin);
            formattedValues[(int)GazeOutputValue.RightGazeOrigin3dY] = GazeDataConverter.FormatDouble(_right?.GazeData3d?.GazeOrigin.Y, config.DataLogFormatOrigin);
            formattedValues[(int)GazeOutputValue.RightGazeOrigin3dZ] = GazeDataConverter.FormatDouble(_right?.GazeData3d?.GazeOrigin.Z, config.DataLogFormatOrigin);
            formattedValues[(int)GazeOutputValue.RightGazeOrigin3dIsValid] = GazeDataConverter.FormatBoolean(_right?.GazeData3d?.IsGazeOriginValid);
            formattedValues[(int)GazeOutputValue.RightGazeDistance] = GazeDataConverter.FormatDouble(_right?.GazeData3d?.GazeDistance, config.DataLogFormatOrigin);
            formattedValues[(int)GazeOutputValue.RightPupilDiameter] = GazeDataConverter.FormatDouble(_right?.EyeData?.PupilDiameter, config.DataLogFormatDiameter);
            formattedValues[(int)GazeOutputValue.RightPupilDiameterIsValid] = GazeDataConverter.FormatBoolean(_right?.EyeData?.IsPupilDiameterValid);

            formattedValues[(int)GazeOutputValue.Tag] = tag;
            formattedValues[(int)GazeOutputValue.TrialId] = trialId.ToString();

            return formattedValues;
        }
    }
}
