/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
﻿using GazeUtilityLibrary;
using GazeUtilityLibrary.DataStructs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace CustomCalibrationLibrary.Models
{
    /// <summary>
    /// Events to trigger changes in the calibration process.
    /// </summary>
    public enum CalibrationEventType
    {
        Init,
        Start,
        Accept,
        Restart,
        Abort
    }        
    
    /// <summary>
    /// The status of the calibarion process.
    /// </summary>
    public enum CalibrationStatus
    {
        ScreenSelection,
        HeadPosition,
        DataCollection,
        Computing,
        CalibrationResult,
        ValidationResult,
        Error,
        Disconnect
    }

    /// <summary>
    /// The model for the calibration process.
    /// </summary>
    public class CalibrationModel : INotifyPropertyChanged
    {
        private int _retries;
        /// <summary>
        /// The number of automatic calibration retries to perform.
        /// </summary>
        public int Retries { get { return _retries; } }

        private int _retryCount;
        /// <summary>
        /// The automatic aclibration retry counter.
        /// </summary>
        public int RetryCount
        {
            get { return _retryCount; }
            set { _retryCount = value; }
        }

        private double _accuracyThreshold;
        /// <summary>
        /// The accuracy threshold.
        /// </summary>
        public double AccuracyThreshold { get { return _accuracyThreshold; } }

        private Color _backgroundColor;
        /// <summary>
        /// The background color of the canvas.
        /// </summary>
        public Color BackgroundColor { get { return _backgroundColor; } }

        private Color _frameColor;
        /// <summary>
        /// The background color of the frame.
        /// </summary>
        public Color FrameColor { get { return _frameColor; } }

        private double _calibrationAccuracyLeft;
        /// <summary>
        /// The approximated accuracy of the current calibration for the left eye.
        /// </summary>
        public double CalibrationAccuracyLeft { get { return _calibrationAccuracyLeft; } }

        private double _calibrationAccuracyRight;
        /// <summary>
        /// The approximated accuracy of the current calibration for the right eye.
        /// </summary>
        public double CalibrationAccuracyRight { get { return _calibrationAccuracyRight; } }

        private string _error = "No Error";
        /// <summary>
        /// The error message of the calibration process.
        /// </summary>
        public string Error { get { return _error; } set { _error = value; OnPropertyChanged(); } }

        /// <summary>
        /// Event to trigger changes in the calibration process.
        /// </summary>
        public event EventHandler<CalibrationEventType>? CalibrationEvent;
        /// <summary>
        /// The calibraion event change handler.
        /// </summary>
        /// <param name="type"></param>
        public void OnCalibrationEvent(CalibrationEventType type)
        {
            CalibrationEvent?.Invoke(this, type);
        }
        /// <summary>
        /// Event to trigger property changes in this class.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        private GazeValidationData _validationData;
        /// <summary>
        /// The data returned by a successful validation process.
        /// </summary>
        public GazeValidationData ValidationData
        {
            get { return _validationData; }
            set
            {
                _validationData = value;
                OnPropertyChanged();
            }
        }

        private Cursor _cursorType = Cursors.Arrow;
        /// <summary>
        /// The data returned by a successful validation process.
        /// </summary>
        public Cursor CursorType
        {
            get { return _cursorType; }
            set
            {
                _cursorType = value;
                OnPropertyChanged();
            }
        }

        private CalibrationStatus _status;

        /// <summary>
        /// The status of the calibarion process.
        /// </summary>
        public CalibrationStatus Status
        {
            get { return _status; }
            set
            {
                _lastStatus = _status;
                _status = value;
                OnPropertyChanged();
            }
        }

        private CalibrationStatus _lastStatus;
        /// <summary>
        /// The calibration status before an error occured.
        /// </summary>
        public CalibrationStatus LastStatus { get { return _lastStatus; } }
        private void OnPropertyChanged([CallerMemberName] string? property_name = null)
        {
            Application.Current.Dispatcher.Invoke(() => {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property_name));
            });
        }
        private Point[] _points;
        /// <summary>
        /// All calibration points.
        /// </summary>
        public Point[] Points { get { return _points; } }
        private ObservableCollection<CalibrationPoint> _calibrationPoints = new ObservableCollection<CalibrationPoint>();
        /// <summary>
        /// The calibration points to be added during the calibration process.
        /// </summary>
        public ObservableCollection<CalibrationPoint> CalibrationPoints
        {
            get { return _calibrationPoints; }
        }

        /// <summary>
        /// Event to trigger gaze point changes.
        /// </summary>
        public event EventHandler<Point>? GazePointChanged;
        private Point _gazePoint;
        /// <summary>
        /// The gaze point position.
        /// </summary>
        public Point GazePoint { get { return _gazePoint; } }

        private void OnGazePointChanged([CallerMemberName] string? property_name = null)
        {
            GazePointChanged?.Invoke(this, _gazePoint);
        }

        /// <summary>
        /// Event to trigger user position guide changes.
        /// </summary>
        public event EventHandler<UserPositionData>? UserPositionGuideChanged;
        private UserPositionData _userPositionGuide;
        /// <summary>
        /// The user position giude values.
        /// </summary>
        public UserPositionData UserPositionGuide
        {
            get { return _userPositionGuide; }
            set {
                _userPositionGuide = value;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    OnUserPositionGuideChanged();
                });
            }
        }
        private void OnUserPositionGuideChanged([CallerMemberName] string? property_name = null)
        {
            UserPositionGuideChanged?.Invoke(this, _userPositionGuide);
        }

        private int _index = -1;
        /// <summary>
        /// The index of the current calibration point
        /// </summary>
        public int Index { get { return _index; } }

        private TrackerLogger _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger">The log handler</param>
        /// <param name="points">Calibration points</param>
        /// <param name="backgroundColor">The background color of the canvas</param>
        /// <param name="frameColor">The background color if the user interaction frame</param>
        /// <param name="accuracyThreshold">The accuracy threshold</param>
        public CalibrationModel(TrackerLogger logger, double[][] points, Color backgroundColor, Color frameColor, double accuracyThreshold, int retries)
        {
            _logger = logger;
            _backgroundColor = backgroundColor;
            _frameColor = frameColor;
            _accuracyThreshold = accuracyThreshold;
            _retries = retries;
            _retryCount = 0;

            _points = new Point[points.Length];
            for (int i = 0; i < points.Length; i++ )
            {
                _points[i] = new Point(points[i][0], points[i][1]);
            }
            _lastStatus = CalibrationStatus.HeadPosition;
            _status = CalibrationStatus.Computing;
            _gazePoint = new Point(0, 0);
            _userPositionGuide = new UserPositionData();
            _validationData = new GazeValidationData();
            _calibrationAccuracyLeft = double.PositiveInfinity;
            _calibrationAccuracyRight = double.PositiveInfinity;
        }

        /// <summary>
        /// Update the normalized gaze point on the screen.
        /// </summary>
        /// <param name="x">The x coordinate</param>
        /// <param name="y">The y coordinate</param>
        public void UpdateGazePoint(double x, double y)
        {
            _gazePoint.X = x;
            _gazePoint.Y = y;
            Application.Current.Dispatcher.Invoke(() =>
            {
                OnGazePointChanged();
            });
        }

        /// <summary>
        /// Prepare the calibration window.
        /// </summary>
        public void PrepareCalibration()
        {
            _index = -1;
            _calibrationPoints.Clear();
        }

        /// <summary>
        /// Trigger the next calibration point.
        /// </summary>
        public void NextCalibrationPoint()
        {
            _index++;
            _calibrationPoints.Add(new CalibrationPoint(_points[_index], _index));
        }

        /// <summary>
        /// Remove and re-add the current calibration point
        /// </summary>
        public void RedoCalibrationPoint()
        {
            _calibrationPoints.RemoveAt(_index);
            _calibrationPoints.Add(new CalibrationPoint(_points[_index], _index));
        }

        /// <summary>
        /// Trigger the data collected events.
        /// </summary>
        public void GazeDataCollected()
        {
            CalibrationPoints[Index].HasData = true;
        }

        /// <summary>
        /// Updates the calibration results on the screen.
        /// </summary>
        /// <param name="points"></param>
        public void SetCalibrationResult(List<GazeCalibrationData> points)
        {
            _calibrationAccuracyLeft = 0;
            _calibrationAccuracyRight = 0;
            int validCountLeft = 0;
            int validCountRight = 0;
            for (int i = 0; i < points.Count; i++)
            {
                if (points[i].ValidityLeft && points[i].AccuracyLeft > 0)
                {
                    _calibrationAccuracyLeft += points[i].AccuracyLeft;
                    validCountLeft++;
                }
                if (points[i].ValidityRight && points[i].AccuracyRight > 0)
                {
                    _calibrationAccuracyRight += points[i].AccuracyRight;
                    validCountRight++;
                }
                double xAvg = (points[i].XCoordLeft + points[i].XCoordRight) / 2;
                double yAvg = (points[i].YCoordLeft + points[i].YCoordRight) / 2;
                CalibrationPoints[i].GazePositionAverage = new Point(xAvg, yAvg);
                CalibrationPoints[i].GazePositionLeft = new Point(points[i].XCoordLeft, points[i].YCoordLeft);
                CalibrationPoints[i].GazePositionRight = new Point(points[i].XCoordRight, points[i].YCoordRight);
                _logger.Debug($"Calibration at [{points[i].XCoord}, {points[i].YCoord}]: [{points[i].XCoordLeft}, {points[i].YCoordLeft}], [{xAvg}, {yAvg}], [{points[i].XCoordRight}, {points[i].YCoordRight}]");
            }

            if(validCountLeft > 0 )
            {
                _calibrationAccuracyLeft /= validCountLeft;
            }

            if (validCountRight > 0)
            {
                _calibrationAccuracyRight /= validCountRight;
            }
        }
    }
}
