﻿using CustomCalibrationLibrary.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Tobii.Research;

namespace CustomCalibrationLibrary.ViewModels
{
    class UserPositionGuideViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private UserPositionGuide _left;
        public UserPositionGuide Left
        {
            get { return _left; }
            set { _left = value; OnPropertyChanged(); }
        }

        private UserPositionGuide _right;
        public UserPositionGuide Right
        {
            get { return _right; }
            set { _right = value; OnPropertyChanged(); }
        }
        /// <summary>
        /// Called when when the state property of EyeTracker is changing.
        /// </summary>
        /// <param name="property_name">Name of the property in WPF.</param>
        private void OnPropertyChanged([CallerMemberName] string? property_name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property_name));
        }

        public UserPositionGuideViewModel(CalibrationModel model)
        {
            _left = new UserPositionGuide(new NormalizedPoint3D((float)0.5, (float)0.5, (float)0.5), Validity.Invalid);
            _right = new UserPositionGuide(new NormalizedPoint3D((float)0.5, (float)0.5, (float)0.5), Validity.Invalid);
            model.UserPositionGuideChanged += OnUserPositionGuideChanged;
        }

        private void OnUserPositionGuideChanged(object? sender, UserPositionGuideEventArgs position)
        {
            if (sender == null)
            {
                return;
            }
            NormalizedPoint3D point = position.LeftEye.UserPosition;
            Left = new UserPositionGuide(new NormalizedPoint3D(1 - point.X, point.Y, point.Z), position.LeftEye.Validity);
            point = position.RightEye.UserPosition;
            Right = new UserPositionGuide(new NormalizedPoint3D(1 - point.X, point.Y, point.Z), position.RightEye.Validity);
        }
    }
}
