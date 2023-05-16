using CustomCalibrate.Models;
using CustomCalibrate.Views;  
using System.ComponentModel;
using System.Windows.Controls;
using GazeUtilityLibrary;

namespace CustomCalibrationLibrary.Views
{
    /// <summary>
    /// Interaction logic for CalibrationCollection.xaml
    /// </summary>
    public partial class CalibrationCollection : Page
    {
        private CalibrationModel _model;
        public CalibrationCollection(TrackerLogger logger)
        {
            InitializeComponent();
            _model = new CalibrationModel(logger);
            _model.PropertyChanged += OnPropertyChanged;
            Main.Content = new Calibration(_model);
        }

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender == null)
            {
                return;
            }
            switch (((CalibrationModel)sender).Status)
            {
                case CalibrationModel.CalibrationStatus.DataCollection:
                    Main.Content = new Calibration(_model);
                    break;
                case CalibrationModel.CalibrationStatus.DataResult:
                    Main.Content = new CalibrationResult(_model);
                    break;
            }
        }
    }
}
