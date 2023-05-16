using CustomCalibrate.Models;
using CustomCalibrate.ViewModels;
using System.Windows.Controls;

namespace CustomCalibrate.Views
{
    /// <summary>
    /// Interaction logic for CalibrationResult.xaml
    /// </summary>
    public partial class CalibrationResult : Page
    {
        private CalibrationModel _model;
        public CalibrationResult(CalibrationModel model)
        {
            InitializeComponent();
            _model = model;
            DataContext = new CalibrationViewModel(_model);
        }
    }
}
