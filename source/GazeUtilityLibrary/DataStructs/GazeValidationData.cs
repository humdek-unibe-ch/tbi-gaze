using System;

namespace GazeUtilityLibrary.DataStructs
{
    /// <summary>
    /// The gaze validation data structure
    /// </summary>
    public class GazeValidationData
    {
        private float _accuracyLeft;
        /// <summary>
        /// The accuracy in degrees averaged over all collected points for the left eye.
        /// </summary>
        public float AccuracyLeft { get { return _accuracyLeft; } }

        private float _accuracyRight;
        /// <summary>
        /// The accuracy in degrees averaged over all collected points for the right eye.
        /// </summary>
        public float AccuracyRight { get { return _accuracyRight; } }

        private float _precisionLeft;
        /// <summary>
        /// The precision (standard deviation) in degrees averaged over all collected points for the left eye.
        /// </summary>
        public float PrecisionLeft { get { return _precisionLeft; } }

        private float _precisionRight;
        /// <summary>
        /// The precision (standard deviation) in degrees averaged over all collected points for the right eye.
        /// </summary>
        public float PrecisionRight { get { return _precisionRight; } }

        private float _precisionRmsLeft;
        /// <summary>
        /// The precision (root mean square of sample-to-sample error) in degrees averaged over all collected points for the left eye.
        /// </summary>
        public float PrecisionRmsLeft { get { return _precisionRmsLeft; } }

        private float _precisionRmsRight;
        /// <summary>
        /// The precision (root mean square of sample-to-sample error) in degrees averaged over all collected points for the right eye.
        /// </summary>
        public float PrecisionRmsRight { get { return _precisionRmsRight; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="GazeValidationData"/> class.
        /// </summary>
        public GazeValidationData()
        {
            _accuracyLeft = 0;
            _accuracyRight = 0;
            _precisionLeft = 0;
            _precisionRight = 0;
            _precisionRmsLeft = 0;
            _precisionRmsRight = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GazeValidationData"/> class.
        /// </summary>
        /// <param name="accuracyLeft">The accuracy in degrees averaged over all collected points for the left eye.</param>
        /// <param name="accuracyRight">The accuracy in degrees averaged over all collected points for the right eye.</param>
        /// <param name="precisionLeft">The precision (standard deviation) in degrees averaged over all collected points for the left eye.</param>
        /// <param name="precisionRight">The precision (standard deviation) in degrees averaged over all collected points for the right eye.</param>
        /// <param name="precisionRmsLeft">The precision (root mean square of sample-to-sample error) in degrees averaged over all collected points for the left eye.</param>
        /// <param name="precisionRmsRight">The precision (root mean square of sample-to-sample error) in degrees averaged over all collected points for the right eye.</param>
        public GazeValidationData(float accuracyLeft, float accuracyRight, float precisionLeft,
            float precisionRight, float precisionRmsLeft, float precisionRmsRight)
        {
            _accuracyLeft = accuracyLeft;
            _accuracyRight = accuracyRight;
            _precisionLeft = precisionLeft;
            _precisionRight = precisionRight;
            _precisionRmsLeft = precisionRmsLeft;
            _precisionRmsRight = precisionRmsRight;
        }

        /// <summary>
        /// Prepare a list of formatted calibration data values
        /// </summary>
        /// <param name="config">The gaze configuration structure</param>
        /// <returns>A list of formatted values. Each index corresponds to a specific value. This allows to reorder the list according to a format string.</returns>
        public string[] Prepare(ConfigItem config)
        {
            string[] formattedValues = new string[Enum.GetNames(typeof(ValidationOutputValue)).Length];

            formattedValues[(int)ValidationOutputValue.LeftAccuracy] = GazeDataConverter.FormatDouble(_accuracyLeft);
            formattedValues[(int)ValidationOutputValue.LeftPrecision] = GazeDataConverter.FormatDouble(_precisionLeft);
            formattedValues[(int)ValidationOutputValue.LeftPrecisionRMS] = GazeDataConverter.FormatDouble(_precisionRmsLeft);
            formattedValues[(int)ValidationOutputValue.RightAccuracy] = GazeDataConverter.FormatDouble(_accuracyRight);
            formattedValues[(int)ValidationOutputValue.RightPrecision] = GazeDataConverter.FormatDouble(_precisionRight);
            formattedValues[(int)ValidationOutputValue.RightPrecisionRMS] = GazeDataConverter.FormatDouble(_precisionRmsRight);

            return formattedValues;
        }
    }
}
