using CustomCalibrationLibrary.Commands;
using CustomCalibrationLibrary.Models;
using System.Windows.Controls;
using System.Windows.Input;

namespace CustomCalibrationLibrary.Views
{
    /// <summary>
    /// Interaction logic for Disconnect.xaml
    /// </summary>
    public partial class Disconnect : Page
    {
        private ICommand _calibrationAbortCommand;
        /// <summary>
        /// Command to abort the calibration
        /// </summary>
        public ICommand CalibrationAbortCommand { get { return _calibrationAbortCommand; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="Disconnect"/> class.
        /// </summary>
        /// <param name="model">The calibration model</param>
        public Disconnect(CalibrationModel model)
        {
            InitializeComponent();
            Focus();
            _calibrationAbortCommand = new CalibrationCommand(model, CalibrationEventType.Abort);
            DataContext = this;
        }
    }
}
