using GazeUtilityLibrary;
using GazeUtilityLibrary.DataStructs;
using GazeUtilityLibrary.Tracker;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace CustomCalibrationLibrary.Models
{
    /// <summary>
    /// A calibration point class holding several metrics connected to a calibration point.
    /// </summary>
    public class CalibrationPoint : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private int _index;
        /// <summary>
        /// The index of the calibration point.
        /// </summary>
        public int Index { get { return _index; } }

        private bool _hasData;
        /// <summary>
        /// Flag to indicate whether data has been collected for this calibration point.
        /// </summary>
        public bool HasData
        {
            get { return _hasData; }
            set { _hasData = value; OnPropertyChanged(); }
        }

        private Point _position;
        /// <summary>
        /// The position of the calibration point.
        /// </summary>
        public Point Position
        {
            get { return _position; }
            set { _position = value; OnPropertyChanged(); }
        }

        private Point _gazePositionAverage;
        /// <summary>
        /// The average between the left and the right gaze point.
        /// </summary>
        public Point GazePositionAverage
        {
            get { return _gazePositionAverage; }
            set { _gazePositionAverage = value; OnPropertyChanged(); }
        }

        private Point _gazePositionLeft;
        /// <summary>
        /// The left gaze point.
        /// </summary>
        public Point GazePositionLeft
        {
            get { return _gazePositionLeft; }
            set { _gazePositionLeft = value; OnPropertyChanged(); }
        }

        private Point _gazePositionRight;
        /// <summary>
        /// The right gaze point.
        /// </summary>
        public Point GazePositionRight
        {
            get { return _gazePositionRight; }
            set { _gazePositionRight = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Called when when the state property of EyeTracker is changing.
        /// </summary>
        /// <param name="property_name">Name of the property in WPF.</param>
        private void OnPropertyChanged([CallerMemberName] string? property_name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property_name));
        }

        public CalibrationPoint(Point position, int index)
        {
            _hasData = false;
            _position = position;
            _index = index;
        }
    }

    /// <summary>
    /// Events to trigger changes in the calibration process.
    /// </summary>
    public enum CalibrationEventType
    {
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
        HeadPosition,
        DataCollection,
        Computing,
        DataResult,
        Error,
        Disconnect
    }

    /// <summary>
    /// The model for the calibration process.
    /// </summary>
    public class CalibrationModel : INotifyPropertyChanged
    {
        private string _error = "No Error";
        /// <summary>
        /// The error message of the calibration process.
        /// </summary>
        public string Error { get { return _error; } set { _error = value; OnPropertyChanged(); } }

        /// <summary>
        /// Events to trigger changes in the calibration process.
        /// </summary>
        public event EventHandler<CalibrationEventType>? CalibrationEvent;
        public void OnCalibrationEvent(CalibrationEventType type)
        {
            CalibrationEvent?.Invoke(this, type);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

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

        public event EventHandler<UserPositionData>? UserPositionGuideChanged;
        private UserPositionData _userPositionGuide;
        /// <summary>
        /// The user position giude values.
        /// </summary>
        public UserPositionData UserPositionGuide
        {
            get { return _userPositionGuide; }
            set { _userPositionGuide = value; OnUserPositionGuideChanged(); }
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


        public CalibrationModel(TrackerLogger logger, double[][] points)
        {
            _logger = logger;

            _points = new Point[points.Length];
            for (int i = 0; i < points.Length; i++ )
            {
                _points[i] = new Point(points[i][0], points[i][1]);
            }
            _lastStatus = CalibrationStatus.HeadPosition;
            _status = CalibrationStatus.Computing;
            _gazePoint = new Point(0, 0);
            _userPositionGuide = new UserPositionData();
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
            OnGazePointChanged();
        }

        /// <summary>
        /// Initialise the calibration.
        /// </summary>
        public void InitCalibration()
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
            for (int i = 0; i < points.Count; i++)
            {
                double xAvg = (points[i].XCoordLeft + points[i].XCoordRight) / 2;
                double yAvg = (points[i].YCoordLeft + points[i].YCoordRight) / 2;
                CalibrationPoints[i].GazePositionAverage = new Point(xAvg, yAvg);
                CalibrationPoints[i].GazePositionLeft = new Point(points[i].XCoordLeft, points[i].YCoordLeft);
                CalibrationPoints[i].GazePositionRight = new Point(points[i].XCoordRight, points[i].YCoordRight);
                _logger.Debug($"Calibration at [{points[i].XCoord}, {points[i].YCoord}]: [{points[i].XCoordLeft}, {points[i].YCoordLeft}], [{xAvg}, {yAvg}], [{points[i].XCoordRight}, {points[i].YCoordRight}]");
            }
        }
    }
}
