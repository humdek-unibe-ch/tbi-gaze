using CustomCalibrationLibrary.Models;
using CustomCalibrationLibrary.ViewModels;
using System.Windows.Controls;

namespace CustomCalibrationLibrary.Views
{
    /// <summary>
    /// Interaction logic for UserPositionGuide.xaml
    /// </summary>
    public partial class UserPositionGuide : Page
    {
        private CalibrationModel _model;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserPositionGuide"/> class.
        /// </summary>
        /// <param name="model">The calibration model.</param>
        public UserPositionGuide(CalibrationModel model)
        {
            InitializeComponent();
            _model = model;
            DataContext = new UserPositionGuideViewModel(_model);
            Focus();
        }
    }
}
