using CustomCalibrationLibrary.Models;
using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace CustomCalibrationLibrary.Views
{
    /// <summary>
    /// Interaction logic for CalibrationCollection.xaml
    /// </summary>
    public partial class CalibrationCollection : Frame
    {
        private CalibrationModel _model;
        private Computing _computingView;

        public CalibrationCollection(CalibrationModel model)
        {
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
                case CalibrationModel.CalibrationStatus.HeadPosition:
                    this.Content = new UserPositionGuide(_model);
                    break;
                case CalibrationModel.CalibrationStatus.DataCollection:
                    this.Content = new Calibration(_model);
                    break;
                case CalibrationModel.CalibrationStatus.Computing:
                    this.Content = _computingView;
                    break;
                case CalibrationModel.CalibrationStatus.DataResult:
                    this.Content = new CalibrationResult(_model);
                    break;
                case CalibrationModel.CalibrationStatus.Error:
                    this.Content = new CalibrationFailed(_model);
                    break;
            }
        }
    }

}
