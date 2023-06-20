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
        /// Called when [gaze data received].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="data">The data.</param>
        public string[] Prepare(ConfigItem Config)
        {
            string[] formatted_values = new string[Enum.GetNames(typeof(CalibrationOutputValue)).Length];
            // write the coordinates to the log file
            if (Config.DataLogWriteOutput)
            {
                formatted_values[(int)CalibrationOutputValue.XCoord] = GazeDataConverter.GetValueString(_xCoord, Config.DataLogFormatNormalizedPoint);
                formatted_values[(int)CalibrationOutputValue.YCoord] = GazeDataConverter.GetValueString(_yCoord, Config.DataLogFormatNormalizedPoint);
                formatted_values[(int)CalibrationOutputValue.XCoordLeft] = GazeDataConverter.GetValueString(_xCoordLeft, Config.DataLogFormatNormalizedPoint);
                formatted_values[(int)CalibrationOutputValue.YCoordLeft] = GazeDataConverter.GetValueString(_yCoordLeft, Config.DataLogFormatNormalizedPoint);
                formatted_values[(int)CalibrationOutputValue.ValidCoordLeft] = GazeDataConverter.GetValueString(_validityLeft);
                formatted_values[(int)CalibrationOutputValue.XCoordRight] = GazeDataConverter.GetValueString(_xCoordRight, Config.DataLogFormatNormalizedPoint);
                formatted_values[(int)CalibrationOutputValue.YCoordRight] = GazeDataConverter.GetValueString(_yCoordRight, Config.DataLogFormatNormalizedPoint);
                formatted_values[(int)CalibrationOutputValue.ValidCoordRight] = GazeDataConverter.GetValueString(_validityRight);
            }
            return formatted_values;
        }
    }
}
