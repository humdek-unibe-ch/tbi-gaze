using CustomCalibrationLibrary.Models;
using CustomCalibrationLibrary.ViewModels;
using System.Windows.Controls;

namespace CustomCalibrationLibrary.Views
{
    /// <summary>
    /// Interaction logic for Calibration.xaml
    /// </summary>
    public partial class Calibration : Page
    {
        private CalibrationModel _model;

        public Calibration(CalibrationModel model)
        {
            InitializeComponent();
            _model = model;
            DataContext = new CalibrationViewModel(_model);
        }
    }
}
