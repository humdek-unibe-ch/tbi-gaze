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

        /// <summary>
        /// Initializes a new instance of the <see cref="Calibration"/> class.
        /// </summary>
        /// <param name="model">The calibration model</param>
        public Calibration(CalibrationModel model)
        {
            InitializeComponent();
            _model = model;
            DataContext = new CalibrationViewModel(_model);
        }
    }
}
