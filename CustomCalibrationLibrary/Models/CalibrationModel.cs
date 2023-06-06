using GazeUtilityLibrary;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Tobii.Research;

namespace CustomCalibrationLibrary.Models
{
    /// <summary>
    /// A calibration point class holding several metrics connected to a calibration point.
    /// </summary>
    public class CalibrationPoint : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private int _index;
        public int Index { get { return _index; } }

        private bool _hasData;
        public bool HasData
        {
            get { return _hasData; }
            set { _hasData = value; OnPropertyChanged(); }
        }

        private Point _position;
        public Point Position
        {
            get { return _position; }
            set { _position = value; OnPropertyChanged(); }
        }

        private Point _gazePositionAverage;
        public Point GazePositionAverage
        {
            get { return _gazePositionAverage; }
            set { _gazePositionAverage = value; OnPropertyChanged(); }
        }

        private Point _gazePositionLeft;
        public Point GazePositionLeft
        {
            get { return _gazePositionLeft; }
            set { _gazePositionLeft = value; OnPropertyChanged(); }
        }

        private Point _gazePositionRight;
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

    public enum CalibrationEventType
    {
        Start,
        Accept,
        Restart,
        Abort
    }

    /// <summary>
    /// The model for the calibration process.
    /// </summary>
    public class CalibrationModel : INotifyPropertyChanged
    {
        private string _error = "No Error";
        public string Error { get { return _error; } set { _error = value; OnPropertyChanged(); } }
        public event EventHandler<CalibrationEventType>? CalibrationEvent;
        public void OnCalibrationEvent(CalibrationEventType type)
        {
            CalibrationEvent?.Invoke(this, type);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private CalibrationStatus _status;
        public enum CalibrationStatus
        {
            HeadPosition,
            DataCollection,
            Computing,
            DataResult,
            Error,
            Disconnect
        }
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
        public CalibrationStatus LastStatus { get { return _lastStatus; } }
        private void OnPropertyChanged([CallerMemberName] string? property_name = null)
        {
            Application.Current.Dispatcher.Invoke(() => {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property_name));
            });
        }
        private Point[] _points;
        public Point[] Points { get { return _points; } }
        private ObservableCollection<CalibrationPoint> _calibrationPoints = new ObservableCollection<CalibrationPoint>();
        public ObservableCollection<CalibrationPoint> CalibrationPoints
        {
            get { return _calibrationPoints; }
        }

        public event EventHandler<Point>? GazePointChanged;
        private Point _gazePoint;
        public Point GazePoint { get { return _gazePoint; } }

        private void OnGazePointChanged([CallerMemberName] string? property_name = null)
        {
            GazePointChanged?.Invoke(this, _gazePoint);
        }

        public event EventHandler<UserPositionGuideEventArgs>? UserPositionGuideChanged;
        private UserPositionGuideEventArgs _userPositionGuide;
        public UserPositionGuideEventArgs UserPositionGuide
        {
            get { return _userPositionGuide; }
            set { _userPositionGuide = value; OnUserPositionGuideChanged(); }
        }
        private void OnUserPositionGuideChanged([CallerMemberName] string? property_name = null)
        {
            UserPositionGuideChanged?.Invoke(this, _userPositionGuide);
        }

        private int _index = -1;
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
            _userPositionGuide = new UserPositionGuideEventArgs(
                new UserPositionGuide(new NormalizedPoint3D((float)0.5, (float)0.5, (float)0.5), Validity.Invalid),
                new UserPositionGuide(new NormalizedPoint3D((float)0.5, (float)0.5, (float)0.5), Validity.Invalid)
            );
        }

        public void UpdateGazePoint(double x, double y)
        {
            _gazePoint.X = x;
            _gazePoint.Y = y;
            OnGazePointChanged();
        }

        public void InitCalibration()
        {
            _index = -1;
            _calibrationPoints.Clear();
        }

        public void NextCalibrationPoint()
        {
            _index++;
            _calibrationPoints.Add(new CalibrationPoint(_points[_index], _index));
        }

        public void RedoCalibrationPoint()
        {
            _calibrationPoints.RemoveAt(_index);
            _calibrationPoints.Add(new CalibrationPoint(_points[_index], _index));
        }

        public void GazeDataCollected()
        {
            CalibrationPoints[Index].HasData = true;
        }

        public void SetCalibrationResult(CalibrationPointCollection points)
        {
            for (int i = 0; i < points.Count; i++)
            {
                double xLeft = 0;
                double yLeft = 0;
                double xRight = 0;
                double yRight = 0;
                double xLeftSum = 0;
                double yLeftSum = 0;
                double xRightSum = 0;
                double yRightSum = 0;
                int leftCount = 0;
                int rightCount = 0;
                foreach (var point in points[i].CalibrationSamples)
                {
                    if (point.LeftEye.Validity == CalibrationEyeValidity.ValidAndUsed)
                    {
                        xLeftSum += point.LeftEye.PositionOnDisplayArea.X;
                        yLeftSum += point.LeftEye.PositionOnDisplayArea.Y;
                        leftCount++;
                    }
                    if (point.RightEye.Validity == CalibrationEyeValidity.ValidAndUsed)
                    {
                        xRightSum += point.RightEye.PositionOnDisplayArea.X;
                        yRightSum += point.RightEye.PositionOnDisplayArea.Y;
                        rightCount++;
                    }
                }
                if (leftCount > 0)
                {
                    xLeft = xLeftSum / leftCount;
                    yLeft = yLeftSum / leftCount;
                }
                if (rightCount > 0)
                {
                    xRight = xRightSum / rightCount;
                    yRight = yRightSum / rightCount;
                }
                double xAvg = (xLeft + xRight) / 2;
                double yAvg = (yLeft + yRight) / 2;
                CalibrationPoints[i].GazePositionAverage = new Point(xAvg, yAvg);
                CalibrationPoints[i].GazePositionLeft = new Point(xLeft, yLeft);
                CalibrationPoints[i].GazePositionRight = new Point(xRight, yRight);
                _logger.Debug($"Calibration at [{points[i].PositionOnDisplayArea.X}, {points[i].PositionOnDisplayArea.Y}]: [{xLeft}, {yLeft}], [{xAvg}, {yAvg}], [{xRight}, {yRight}]");
            }
        }
    }
}
