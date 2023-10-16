/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
﻿
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GazeUtilityLibrary.DataStructs
{
    /// <summary>
    /// The user position to be rendered on the screen.
    /// </summary>
    public class UserPositionData : INotifyPropertyChanged
    {
        private double _xCoordLeft;
        /// <summary>
        /// The normalized x coordinate of the left eye.
        /// </summary>
        public double XCoordLeft
        {
            get { return _xCoordLeft; }
            set { _xCoordLeft = value; OnPropertyChanged(); }
        }

        private double _yCoordLeft;
        /// <summary>
        /// The normalized y coordinate of the left eye.
        /// </summary>
        public double YCoordLeft
        {
            get { return _yCoordLeft; }
            set { _yCoordLeft = value; OnPropertyChanged(); }
        }

        private double _zCoordLeft;
        /// <summary>
        /// The normalized z coordinate of the left eye.
        /// </summary>
        public double ZCoordLeft
        {
            get { return _zCoordLeft; }
            set { _zCoordLeft = value; OnPropertyChanged(); }
        }

        private double _xCoordRight;
        /// <summary>
        /// The normalized x coordinate of the right eye.
        /// </summary>
        public double XCoordRight
        {
            get { return _xCoordRight; }
            set { _xCoordRight = value; OnPropertyChanged(); }
        }

        private double _yCoordRight;
        /// <summary>
        /// The normalized y coordinate of the right eye.
        /// </summary>
        public double YCoordRight
        {
            get { return _yCoordRight; }
            set { _yCoordRight = value; OnPropertyChanged(); }
        }

        private double _zCoordRight;
        /// <summary>
        /// The normalized z coordinate of the right eye.
        /// </summary>
        public double ZCoordRight 
        {
            get { return _zCoordRight; }
            set { _zCoordRight = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// The property change event handler.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Called when when the state property of EyeTracker is changing.
        /// </summary>
        /// <param name="property_name">Name of the property in WPF.</param>
        private void OnPropertyChanged([CallerMemberName] string? property_name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property_name));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserPositionData"/> class.
        /// </summary>
        public UserPositionData()
        {
            _xCoordLeft = 0.5;
            _yCoordLeft = 0.5;
            _zCoordLeft = 0.5;
            _xCoordRight = 0.5;
            _yCoordRight = 0.5;
            _zCoordRight = 0.5;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserPositionData"/> class.
        /// </summary>
        /// <param name="xCoordLeft">The normalized x coordinate of the left eye.</param>
        /// <param name="yCoordLeft">The normalized y coordinate of the left eye.</param>
        /// <param name="zCoordLeft">The normalized z coordinate of the left eye.</param>
        /// <param name="xCoordRight">The normalized x coordinate of the right eye.</param>
        /// <param name="yCoordRight">The normalized y coordinate of the right eye.</param>
        /// <param name="zCoordRight">The normalized z coordinate of the right eye.</param>
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
