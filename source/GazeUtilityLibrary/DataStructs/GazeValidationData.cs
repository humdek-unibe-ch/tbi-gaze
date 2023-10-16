/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
﻿using System;
using System.Collections.Generic;
using System.Numerics;

namespace GazeUtilityLibrary.DataStructs
{
    /// <summary>
    /// A validation point.
    /// </summary>
    public class GazeValidationPoint
    {
        private Vector2 _point;
        /// <summary>
        /// The validation point.
        /// </summary>
        public Vector2 Point { get { return _point; } }

        private GazeValidationData _result;
        /// <summary>
        /// The validation result of this point.
        /// </summary>
        public GazeValidationData Result { get { return _result; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="GazeValidationPoint"/> class.
        /// </summary>
        /// <param name="point">The validation point.</param>
        /// <param name="result">The validation result of this point.</param>
        public GazeValidationPoint(Vector2 point, GazeValidationData result)
        {
            _point = point;
            _result = result;
        }

        /// <summary>
        /// Prepare a list of formatted calibration data values
        /// </summary>
        /// <param name="config">The gaze configuration structure</param>
        /// <returns>A list of formatted values. Each index corresponds to a specific value. This allows to reorder the list according to a format string.</returns>
        public string[] Prepare(ConfigItem config)
        {
            string[] formattedValues = new string[Enum.GetNames(typeof(ValidationOutputValue)).Length];

            formattedValues[(int)ValidationOutputValue.Point2dX] = GazeDataConverter.FormatDouble(_point.X, config.DataLogFormatNormalizedPoint);
            formattedValues[(int)ValidationOutputValue.Point2dY] = GazeDataConverter.FormatDouble(_point.X, config.DataLogFormatNormalizedPoint);
            formattedValues[(int)ValidationOutputValue.LeftAccuracy] = GazeDataConverter.FormatDouble(_result.AccuracyLeft, config.DataLogFormatValidation);
            formattedValues[(int)ValidationOutputValue.LeftPrecision] = GazeDataConverter.FormatDouble(_result.PrecisionLeft, config.DataLogFormatValidation);
            formattedValues[(int)ValidationOutputValue.LeftPrecisionRMS] = GazeDataConverter.FormatDouble(_result.PrecisionRmsLeft, config.DataLogFormatValidation);
            formattedValues[(int)ValidationOutputValue.RightAccuracy] = GazeDataConverter.FormatDouble(_result.AccuracyRight, config.DataLogFormatValidation);
            formattedValues[(int)ValidationOutputValue.RightPrecision] = GazeDataConverter.FormatDouble(_result.PrecisionRight, config.DataLogFormatValidation);
            formattedValues[(int)ValidationOutputValue.RightPrecisionRMS] = GazeDataConverter.FormatDouble(_result.PrecisionRmsRight, config.DataLogFormatValidation);

            return formattedValues;
        }
    }

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

        private List<GazeValidationPoint> _points;
        /// <summary>
        /// The list of all 
        /// </summary>
        public List<GazeValidationPoint> Points { get { return _points; } }

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
            _points = new List<GazeValidationPoint>();
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
            _points = new List<GazeValidationPoint>();
        }

        /// <summary>
        /// Add a new validation point to the list.
        /// </summary>
        /// <param name="point">The validation point coordinates.</param>
        /// <param name="accuracyLeft">The accuracy in degrees averaged over all collected points for the left eye.</param>
        /// <param name="accuracyRight">The accuracy in degrees averaged over all collected points for the right eye.</param>
        /// <param name="precisionLeft">The precision (standard deviation) in degrees averaged over all collected points for the left eye.</param>
        /// <param name="precisionRight">The precision (standard deviation) in degrees averaged over all collected points for the right eye.</param>
        /// <param name="precisionRmsLeft">The precision (root mean square of sample-to-sample error) in degrees averaged over all collected points for the left eye.</param>
        /// <param name="precisionRmsRight">The precision (root mean square of sample-to-sample error) in degrees averaged over all collected points for the right eye.</param>
        public void AddPoint(Vector2 point, float accuracyLeft, float accuracyRight, float precisionLeft,
            float precisionRight, float precisionRmsLeft, float precisionRmsRight)
        {
            _points.Add(new GazeValidationPoint(point, new GazeValidationData(accuracyLeft, accuracyRight,
                precisionLeft, precisionRight, precisionRmsLeft, precisionRmsRight)));
        }
    }
}
