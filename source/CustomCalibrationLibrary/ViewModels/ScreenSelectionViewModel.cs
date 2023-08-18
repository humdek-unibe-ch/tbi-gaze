using CustomCalibrationLibrary.Commands;
using CustomCalibrationLibrary.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using WpfScreenHelper.Enum;
using WpfScreenHelper;

namespace CustomCalibrationLibrary.ViewModels
{
    class Monitor
    {
        private string _name;
        public string Name { get { return _name;  } }
        private int _index;
        public int Index { get { return _index; } }
        public Monitor(int index, string name)
        {
            _index = index;
            _name = name;
        }
    }
    class ScreenSelectionViewModel
    {
        private Window _window;
        private ObservableCollection<Monitor> _monitors = new ObservableCollection<Monitor>();
        public ObservableCollection<Monitor> Monitors
        {
            get { return _monitors; }
        }

        private ICommand _calibrationStartCommand;
        /// <summary>
        /// Command to start the calibration
        /// </summary>
        public ICommand CalibrationStartCommand { get { return _calibrationStartCommand; } }

        private ICommand _calibrationAbortCommand;
        /// <summary>
        /// Command to abort the calibration
        /// </summary>
        public ICommand CalibrationAbortCommand { get { return _calibrationAbortCommand; } }

        public ScreenSelectionViewModel(CalibrationModel model, Window window)
        {
            _window = window;
            foreach (var screen in Screen.AllScreens)
            {
                _monitors.Add(new Monitor(_monitors.Count, screen.DeviceName));
            }

            _calibrationStartCommand = new CalibrationCommand(model, CalibrationEventType.Init);
            _calibrationAbortCommand = new CalibrationCommand(model, CalibrationEventType.Abort);
        }

        public void SwitchScreen(int index)
        {
            this._window.SetWindowPosition(WindowPositions.Center, Screen.AllScreens.ElementAt(index));
        }
    }
}
