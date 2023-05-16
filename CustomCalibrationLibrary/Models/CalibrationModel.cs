using GazeUtilityLibrary;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace CustomCalibrate.Models
{
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

    public class CalibrationModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private CalibrationStatus _status;
        public enum CalibrationStatus
        {
            HeadPosition,
            DataCollection,
            DataResult
        }
        public CalibrationStatus Status
        {
            get { return _status; }
            set { _status = value; OnPropertyChanged(); }
        }
        private void OnPropertyChanged([CallerMemberName] string? property_name = null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property_name)); }
        private Point[] _points;
        private ObservableCollection<CalibrationPoint> _calibrationPoints = new ObservableCollection<CalibrationPoint>();
        public ObservableCollection<CalibrationPoint> CalibrationPoints
        {
            get { return _calibrationPoints; }
        }

        private int _index = 0;
        public int Index { get { return _index == 0 ? _index : _index - 1; } }

        private TrackerLogger _logger;


        public CalibrationModel(TrackerLogger logger)
        {
            _logger = logger;
            double XDelta = 0.1;
            double YDelta = 0.1;
            Point Center = new Point(0.5, 0.5);
            _points = new Point[8];
            _points[0] = new Point(Center.X + Center.X * 0.3, Center.Y); // center right
            _points[1] = new Point(Center.X - Center.X * 0.3, Center.Y); // center left
            _points[2] = new Point(1 - XDelta, 1 - YDelta); // bottom right
            _points[3] = new Point(XDelta, 1 - YDelta); // bottom left
            _points[4] = new Point(Center.X, YDelta);// top middle
            _points[5] = new Point(XDelta, YDelta); // top left
            _points[6] = new Point(1 - XDelta, YDelta); // top right
            _points[7] = new Point(Center.X, 1 - YDelta); // bottom middle
            _status = CalibrationStatus.DataCollection;
        }

        public void NextCalibrationPoint()
        {
            _calibrationPoints.Add(new CalibrationPoint(_points[_index], _index)); // center right
            _index++;
            if (_index == _points.Length)
            {
                _index = 0;
            }
        }

        public void GazeDataCollected()
        {
            CalibrationPoints[Index].HasData = true;
            CalibrationPoints[Index].GazePositionAverage = new Point(CalibrationPoints[Index].Position.X, CalibrationPoints[Index].Position.Y + 0.01);
            CalibrationPoints[Index].GazePositionLeft = new Point(CalibrationPoints[Index].Position.X - 0.1, CalibrationPoints[Index].Position.Y + 0.01);
            CalibrationPoints[Index].GazePositionRight = new Point(CalibrationPoints[Index].Position.X + 0.1, CalibrationPoints[Index].Position.Y + 0.01);
        }
    }
}
