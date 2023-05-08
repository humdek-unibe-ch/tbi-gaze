using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace CustomCalibrate
{
    public class CalibrationPoint : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private bool _HasData;
        public bool HasData
        {
            get { return _HasData; }
            set { _HasData = value; OnPropertyChanged(); }
        }

        private Point _Position;
        public Point Position
        {
            get { return _Position; }
            set { _Position = value; OnPropertyChanged(); }
        }

        private Point _GazePositionAverage;
        public Point GazePositionAverage
        {
            get { return _GazePositionAverage; }
            set { _GazePositionAverage = value; OnPropertyChanged(); }
        }

        private Point _GazePositionLeft;
        public Point GazePositionLeft
        {
            get { return _GazePositionLeft; }
            set { _GazePositionLeft = value; OnPropertyChanged(); }
        }

        private Point _GazePositionRight;
        public Point GazePositionRight
        {
            get { return _GazePositionRight; }
            set { _GazePositionRight = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Called when when the state property of EyeTracker is changing.
        /// </summary>
        /// <param name="property_name">Name of the property in WPF.</param>
        private void OnPropertyChanged([CallerMemberName] string? property_name = null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property_name)); }
    }
    /// <summary>
    /// Interaction logic for Calibration.xaml
    /// </summary>
    public partial class Calibration : Page
    {
        private Point[]? Positions;
        private int Index = 0;

        private ObservableCollection<CalibrationPoint> calibrationPoints = new ObservableCollection<CalibrationPoint>();
        public ObservableCollection<CalibrationPoint> CalibrationPoints
        {
            get { return calibrationPoints; }
        }
        public Calibration()
        {
            InitializeComponent();
        }

        private CalibrationPoint CreateCalibrationPoint(Point Position)
        {
            return new CalibrationPoint
            {
                Position = Position,
                HasData = false
            };
        }

        public void NextCalibrationPoint(object sender, RoutedEventArgs e)
        {
            if (Positions == null)
            {
                return;
            }
            CalibrationPoints.Insert(Index, CreateCalibrationPoint(Positions[Index]));
            Index++;
            if (Index == Positions.Length)
            {
                Index = 0;
            }
        }

        public void GazeDataCollected(object sender, RoutedEventArgs e)
        {
            CalibrationPoints[Index - 1].HasData = true;
            CalibrationPoints[Index - 1].GazePositionAverage = new Point(CalibrationPoints[Index - 1].Position.X, CalibrationPoints[Index - 1].Position.Y + 5);
            CalibrationPoints[Index - 1].GazePositionLeft = new Point(CalibrationPoints[Index - 1].Position.X - 10, CalibrationPoints[Index - 1].Position.Y + 5);
            CalibrationPoints[Index - 1].GazePositionRight = new Point(CalibrationPoints[Index - 1].Position.X + 10, CalibrationPoints[Index - 1].Position.Y + 5);
        }

        public void OnLoad(object sender, RoutedEventArgs e)
        {
            Points.ItemsSource = CalibrationPoints;
            double XDelta = ActualWidth * 0.1;
            double YDelta = ActualHeight * 0.1;
            Point Center = new Point(ActualWidth / 2, ActualHeight / 2);
            Positions = new Point[8];
            Positions[5] = new Point(XDelta, YDelta); // top left
            Positions[4] = new Point(Center.X, YDelta); // top middle
            Positions[6] = new Point(ActualWidth - XDelta, YDelta); // top right
            Positions[3] = new Point(XDelta, ActualHeight - YDelta); // bottom left
            Positions[7] = new Point(Center.X, ActualHeight - YDelta); // bottom middle
            Positions[2] = new Point(ActualWidth - XDelta, ActualHeight - YDelta); // bottom right
            Positions[0] = new Point(Center.X + Center.X * 0.3, Center.Y); // center right
            Positions[1] = new Point(Center.X - Center.X * 0.3, Center.Y); // center left
        }
    }
}
