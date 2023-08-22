using CustomCalibrationLibrary.Models;
using CustomCalibrationLibrary.ViewModels;

namespace CustomCalibrationLibrary.Views
{
    /// <summary>
    /// Interaction logic for CalibrationResult.xaml
    /// </summary>
    public partial class CalibrationResult : System.Windows.Controls.Page
    {
        private CalibrationModel _model;

        /// <summary>
        /// Initializes a new instance of the <see cref="CalibrationResult"/> class.
        /// </summary>
        /// <param name="model">The calibration model.</param>
        public CalibrationResult(CalibrationModel model)
        {
            InitializeComponent();
            _model = model;
            DataContext = new CalibrationResultViewModel(_model);
            Focus();
        }
    }
}
