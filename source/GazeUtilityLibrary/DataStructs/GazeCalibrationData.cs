using System;

namespace GazeUtilityLibrary.DataStructs
{
    /// <summary>
    /// The event argument class for Tobii eyetracker data
    /// </summary>
    public class GazeCalibrationData
    {
        private double _xCoord;
        public double XCoord { get { return _xCoord; } }

        private double _yCoord;
        public double YCoord { get { return _yCoord; } }

        private double _xCoordLeft;
        public double XCoordLeft { get { return _xCoordLeft; } }

        private double _yCoordLeft;
        public double YCoordLeft { get { return _yCoordLeft; } }

        private bool _validityLeft;
        public bool ValidityLeft { get { return _validityLeft; } }

        private double _xCoordRight;
        public double XCoordRight { get { return _xCoordRight; } }

        private double _yCoordRight;
        public double YCoordRight { get { return _yCoordRight; } }

        private bool _validityRight;
        public bool ValidityRight { get { return _validityRight; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="GazeDataArgs"/> class.
        /// </summary>
        /// <param name="xCoord">The x coord of the calibration point.</param>
        /// <param name="yCoord">The y coord of the calibration point.</param>
        /// <param name="xCoordLeft">The x coord of the gaze point of the left eye.</param>
        /// <param name="yCoordLeft">The y coord of the gaze point of the left eye.</param>
        /// <param name="validityLeft">the validity of gaze point coordinate of the left eye.</param>
        /// <param name="xCoordRight">The x coord of the gaze point of the right eye.</param>
        /// <param name="yCoordRight">The y coord of the gaze point of the right eye.</param>
        /// <param name="validityRight">the validity of gaze point coordinate of the right eye.</param>
        public GazeCalibrationData(double xCoord, double yCoord, double xCoordLeft, double yCoordLeft, bool validityLeft, double xCoordRight, double yCoordRight, bool validityRight)
        {
            _xCoord = xCoord;
            _yCoord = yCoord;
            _xCoordLeft = xCoordLeft;
            _yCoordLeft = yCoordLeft;
            _validityLeft = validityLeft;
            _xCoordRight = xCoordRight;
            _yCoordRight = yCoordRight;
            _validityRight = validityRight;
        }

        /// <summary>
        /// Prepare a list of formatted calibration data values
        /// </summary>
        /// <param name="config">The gaze configuration structure</param>
        /// <returns>A list of formatted values. Each index corresponds to a specific value. This allows to reorder the list according to a format string.</returns>
        public string[] Prepare(ConfigItem config)
        {
            string[] formattedValues = new string[Enum.GetNames(typeof(CalibrationOutputValue)).Length];

            formattedValues[(int)CalibrationOutputValue.XCoord] = GazeDataConverter.FormatDouble(_xCoord, config.DataLogFormatNormalizedPoint);
            formattedValues[(int)CalibrationOutputValue.YCoord] = GazeDataConverter.FormatDouble(_yCoord, config.DataLogFormatNormalizedPoint);
            formattedValues[(int)CalibrationOutputValue.XCoordLeft] = GazeDataConverter.FormatDouble(_xCoordLeft, config.DataLogFormatNormalizedPoint);
            formattedValues[(int)CalibrationOutputValue.YCoordLeft] = GazeDataConverter.FormatDouble(_yCoordLeft, config.DataLogFormatNormalizedPoint);
            formattedValues[(int)CalibrationOutputValue.ValidCoordLeft] = GazeDataConverter.FormatBoolean(_validityLeft);
            formattedValues[(int)CalibrationOutputValue.XCoordRight] = GazeDataConverter.FormatDouble(_xCoordRight, config.DataLogFormatNormalizedPoint);
            formattedValues[(int)CalibrationOutputValue.YCoordRight] = GazeDataConverter.FormatDouble(_yCoordRight, config.DataLogFormatNormalizedPoint);
            formattedValues[(int)CalibrationOutputValue.ValidCoordRight] = GazeDataConverter.FormatBoolean(_validityRight);

            return formattedValues;
        }
    }
}
