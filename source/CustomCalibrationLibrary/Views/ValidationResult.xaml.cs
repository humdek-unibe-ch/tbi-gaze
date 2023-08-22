using CustomCalibrationLibrary.Models;
using CustomCalibrationLibrary.ViewModels;
using System.Windows.Controls;

namespace CustomCalibrationLibrary.Views
{
    /// <summary>
    /// Interaction logic for ValidationResult.xaml
    /// </summary>
    public partial class ValidationResult : Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationResult"/> class.
        /// </summary>
        /// <param name="model">The calibration model.</param>
        public ValidationResult(CalibrationModel model)
        {
            InitializeComponent();
            DataContext = new ValidationResultViewModel(model);
            Focus();
        }
    }
}
