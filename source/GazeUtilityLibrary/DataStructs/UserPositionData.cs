
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GazeUtilityLibrary.DataStructs
{
    public class UserPositionData : INotifyPropertyChanged
    {
        private double _xCoordLeft;
        public double XCoordLeft
        {
            get { return _xCoordLeft; }
            set { _xCoordLeft = value; OnPropertyChanged(); }
        }

        private double _yCoordLeft;
        public double YCoordLeft
        {
            get { return _yCoordLeft; }
            set { _yCoordLeft = value; OnPropertyChanged(); }
        }

        private double _zCoordLeft;
        public double ZCoordLeft
        {
            get { return _zCoordLeft; }
            set { _zCoordLeft = value; OnPropertyChanged(); }
        }

        private double _xCoordRight;
        public double XCoordRight
        {
            get { return _xCoordRight; }
            set { _xCoordRight = value; OnPropertyChanged(); }
        }

        private double _yCoordRight;
        public double YCoordRight
        {
            get { return _yCoordRight; }
            set { _yCoordRight = value; OnPropertyChanged(); }
        }

        private double _zCoordRight;
        public double ZCoordRight 
        {
            get { return _zCoordRight; }
            set { _zCoordRight = value; OnPropertyChanged(); }
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Called when when the state property of EyeTracker is changing.
        /// </summary>
        /// <param name="property_name">Name of the property in WPF.</param>
        private void OnPropertyChanged([CallerMemberName] string? property_name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property_name));
        }

        public UserPositionData()
        {
            _xCoordLeft = 0.5;
            _yCoordLeft = 0.5;
            _zCoordLeft = 0.5;
            _xCoordRight = 0.5;
            _yCoordRight = 0.5;
            _zCoordRight = 0.5;
        }
        public UserPositionData(double xCoordLeft, double yCoordLeft, double zCoordLeft, double xCoordRight, double yCoordRight, double zCoordRight)
        {
            _xCoordLeft = xCoordLeft;
            _yCoordLeft = yCoordLeft;
            _zCoordLeft = zCoordLeft;
            _xCoordRight = xCoordRight;
            _yCoordRight = yCoordRight;
            _zCoordRight = zCoordRight;
        }
    }
}
