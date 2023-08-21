using System;

namespace GazeUtilityLibrary.DataStructs
{
    public class GazeValidationData
    {
        private float _accuracyLeft;
        public float AccuracyLeft { get { return _accuracyLeft; } }
        private float _accuracyRight;
        public float AccuracyRight { get { return _accuracyRight; } }
        private float _precisionLeft;
        public float PrecisionLeft { get { return _precisionLeft; } }
        private float _precisionRight;
        public float PrecisionRight { get { return _precisionRight; } }
        private float _precisionRmsLeft;
        public float PrecisionRmsLeft { get { return _precisionRmsLeft; } }
        private float _precisionRmsRight;
        public float PrecisionRmsRight { get { return _precisionRmsRight; } }
        public GazeValidationData()
        {
            _accuracyLeft = 0;
            _accuracyRight = 0;
            _precisionLeft = 0;
            _precisionRight = 0;
            _precisionRmsLeft = 0;
            _precisionRmsRight = 0;
        }

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
