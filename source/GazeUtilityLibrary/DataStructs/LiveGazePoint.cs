using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GazeUtilityLibrary.DataStructs
{
    /// <summary>
    /// The live gaze point used for verification during the calibration process.
    /// </summary>
    public class LiveGazePoint : INotifyPropertyChanged
    {
        private double _x = 0;
        /// <summary>
        /// The normalized x coordinate on the screen
        /// </summary>
        public double X
        {
            get { return _x; }
            set { _x = value; OnPropertyChanged(); }
        }

        private double _y = 0;
        /// <summary>
        /// The normalized y coordinate on the screen
        /// </summary>
        public double Y
        {
            get { return _y; }
            set { _y = value; OnPropertyChanged(); }
        }

        private bool _visibility = false;
        /// <summary>
        /// The visiblity flag.
        /// </summary>
        public bool Visibility
        {
            get { return _visibility; }
            set { _visibility = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;


        /// <summary>
        /// Called when when a property of LiveGazePoint is changing.
        /// </summary>
        /// <param name="property_name">Name of the property in WPF.</param>
        private void OnPropertyChanged([CallerMemberName] string? property_name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property_name));
        }
    }
}
