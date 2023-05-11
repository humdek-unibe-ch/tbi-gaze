using CustomCalibrate.Models;
using CustomCalibrate.Views;
using System.ComponentModel;
using System.Windows;

namespace CustomCalibrate
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CalibrationModel _model;
        public MainWindow()
        {
            InitializeComponent();
            _model = new CalibrationModel();
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
