using CustomCalibrationLibrary.Models;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace CustomCalibrationLibrary.Views
{
    /// <summary>
    /// Interaction logic for CalibrationCollection.xaml
    /// </summary>
    public partial class CalibrationFrame : Frame
    {
        private CalibrationModel _model;
        private Window _window;
        private Computing _computingView;

        public CalibrationFrame(CalibrationModel model, Window window)
        {
            _window = window;
            _computingView = new Computing();
            InitializeComponent();
            _model = model;
            _model.PropertyChanged += OnPropertyChanged;
            this.Content = _computingView;
        }

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender == null || e.PropertyName != "Status")
            {
                return;
            }
            switch (((CalibrationModel)sender).Status)
            {
                case CalibrationStatus.ScreenSelection:
                    this.Content = new ScreenSelection(_model, _window);
                    break;
                case CalibrationStatus.HeadPosition:
                    this.Content = new UserPositionGuide(_model);
                    break;
                case CalibrationStatus.DataCollection:
                    this.Content = new Calibration(_model);
                    break;
                case CalibrationStatus.Computing:
                    this.Content = _computingView;
                    break;
                case CalibrationStatus.CalibrationResult:
                    this.Content = new CalibrationResult(_model);
                    break;
                case CalibrationStatus.ValidationResult:
                    this.Content = new ValidationResult(_model);
                    break;
                case CalibrationStatus.Error:
                    this.Content = new CalibrationFailed(_model);
                    break;
                case CalibrationStatus.Disconnect:
                    this.Content = new Disconnect(_model);
                    break;
            }
        }
    }

}
