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
        public Disconnect(CalibrationModel model)
        {
            InitializeComponent();
            Focus();
            _calibrationAbortCommand = new CalibrationCommand(model, CalibrationEventType.Abort);
            DataContext = this;
        }
    }
}
