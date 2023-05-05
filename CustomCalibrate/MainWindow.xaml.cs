using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace CustomCalibrate
{
    public class CalibrationPoint : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private double _X;
        public double X
        {
            get { return _X; }
            set { _X = value; OnPropertyChanged(); }
        }

        private double _Y;
        public double Y
        {
            get { return _Y; }
            set { _Y = value; OnPropertyChanged(); }
        }

        private double _Width;
        public double Width
        {
            get { return _Width; }
            set { _Width = value; OnPropertyChanged(); }
        }

        private double _Height;
        public double Height
        {
            get { return _Height; }
            set { _Height = value; OnPropertyChanged(); }
        }

        private double _InitialDiameter;
        public double InitialDiameter
        {
            get { return _InitialDiameter; }
            set { _InitialDiameter = value; OnPropertyChanged(); }
        }

        private System.Windows.Media.Color _Color;
        public System.Windows.Media.Color Color
        {
            get { return _Color; }
            set { _Color = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Called when when the state property of EyeTracker is changing.
        /// </summary>
        /// <param name="property_name">Name of the property in WPF.</param>
        private void OnPropertyChanged([CallerMemberName] string? property_name = null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property_name)); }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ObservableCollection<CalibrationPoint> calibrationPoints = new ObservableCollection<CalibrationPoint>();
        public ObservableCollection<CalibrationPoint> CalibrationPoints
        {
            get { return calibrationPoints; }
        }

        private CalibrationPoint CreateCalibrationPoint(Point Position)
        {
            return new CalibrationPoint
            {
                X = Position.X,
                Y = Position.Y,
                Height = 15,
                Width = 20,
                InitialDiameter = 50,
                Color = Colors.White
            };
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        public void OnLoad(object sender, RoutedEventArgs e)
        {
            Points.ItemsSource = calibrationPoints;
            double XDelta = ActualWidth * 0.1;
            double YDelta = ActualHeight * 0.1;
            Point Center = new Point(ActualWidth / 2, ActualHeight / 2);
            Point TopLeft = new Point(XDelta, YDelta);
            Point TopMiddle = new Point(Center.X, YDelta);
            Point TopRight = new Point(ActualWidth - XDelta, YDelta);
            Point BottomLeft = new Point(XDelta, ActualHeight - YDelta);
            Point BottomMiddle = new Point(Center.X, ActualHeight - YDelta);
            Point BottomRight = new Point(ActualWidth - XDelta, ActualHeight - YDelta);
            Point CenterLeft = new Point(Center.X + Center.X * 0.3, Center.Y);
            Point CenterRight = new Point(Center.X - Center.X * 0.3, Center.Y);

            CalibrationPoints.Add(CreateCalibrationPoint(TopLeft));
            CalibrationPoints.Add(CreateCalibrationPoint(TopMiddle));
            CalibrationPoints.Add(CreateCalibrationPoint(TopRight));
            CalibrationPoints.Add(CreateCalibrationPoint(BottomLeft));
            CalibrationPoints.Add(CreateCalibrationPoint(BottomMiddle));
            CalibrationPoints.Add(CreateCalibrationPoint(BottomRight));
            CalibrationPoints.Add(CreateCalibrationPoint(CenterLeft));
            CalibrationPoints.Add(CreateCalibrationPoint(CenterRight));
        }
    }
}
