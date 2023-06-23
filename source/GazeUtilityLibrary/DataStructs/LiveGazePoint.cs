using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GazeUtilityLibrary.DataStructs
{
    public class LiveGazePoint : INotifyPropertyChanged
    {
        private double _x = 0;
        public double X
        {
            get { return _x; }
            set { _x = value; OnPropertyChanged(); }
        }

        private double _y = 0;
        public double Y
        {
            get { return _y; }
            set { _y = value; OnPropertyChanged(); }
        }

        private bool _visibility = false;
        public bool Visibility
        {
            get { return _visibility; }
            set { _visibility = value; OnPropertyChanged(); }
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
    }
}
