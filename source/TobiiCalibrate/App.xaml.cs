using GazeUtilityLibrary;
using GazeUtilityLibrary.Tracker;
using System;
using System.Diagnostics;
using System.Windows;

namespace TobiiCalibrate
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private TrackerLogger _logger;
        private GazeConfiguration _config;
        private BaseTracker _tracker;
        private bool _isCalibrated = false;

        public App()
        {
            InitializeComponent();

            _logger = new TrackerLogger(null);
            _config = new GazeConfiguration(_logger);
            _tracker = new EyeTrackerPro(_logger, _config.Config);
            _tracker.TrackerEnabled += OnTrackerEnabled;
        }

        private void OnApplicationStartup(object sender, StartupEventArgs e)
        {
            _logger.Info("Preparing to start Tobii eyetracker calibration");
        }

        /// <summary>
        /// Called when [application exit].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OnApplicationExit(object sender, EventArgs e)
        {
            if (!_isCalibrated)
            {
                _logger.Error("Unable to start Tobii eyetracker calibration");
            }
            _logger.Info($"\"{AppDomain.CurrentDomain.BaseDirectory}TobiiCalibrate.exe\" terminated gracefully{Environment.NewLine}");
        }

        /// <summary>
        /// Called when the eye tracker is ready.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OnTrackerEnabled(object? sender, EventArgs e)
        {
            if (_isCalibrated)
            {
                // calibration was successful, eye tracker switched to state ready
                Current.Dispatcher.Invoke(callback: () => { Current.Shutdown(); });
            }
            else
            {
                string executablePath = $"{_tracker.PatternReplace(_config.Config.TobiiApplicationPath)}\\{_tracker.PatternReplace(_config.Config.TobiiCalibrate)}";
                string arguments = $"{_tracker.PatternReplace(_config.Config.TobiiCalibrateArguments)}";
                StartCalibration(executablePath, arguments);
            }
        }

        /// <summary>
        /// Start the Tobii calibration tool.
        /// </summary>
        /// <param name="executablePath">The path to the executable.</param>
        /// <param name="arguments">The arguments with which the executable will be invoked.</param>
        private void StartCalibration(string executablePath, string arguments)
        {
            _logger.Info($"Starting Tobii eyetracker calibration \"{executablePath} {arguments}\"");
            Process tobii_calibrate = new Process();
            // Redirect the output stream of the child process.
            tobii_calibrate.StartInfo.UseShellExecute = false;
            tobii_calibrate.StartInfo.RedirectStandardError = true;
            tobii_calibrate.StartInfo.RedirectStandardOutput = true;
            tobii_calibrate.StartInfo.FileName = executablePath;
            tobii_calibrate.StartInfo.Arguments = arguments;
            tobii_calibrate.Start();

            string stdOutput = tobii_calibrate.StandardOutput.ReadToEnd();

            tobii_calibrate.WaitForExit();
            int exitCode = tobii_calibrate.ExitCode;
            if (exitCode == 0)
            {
                _logger.Info("Eye Tracker Manager was called successfully!");
                _isCalibrated = true;
            }
            else
            {
                _logger.Error($"Eye Tracker Manager call returned the error code: {exitCode}");
                foreach (string line in stdOutput.Split(Environment.NewLine.ToCharArray()))
                {
                    if (line.StartsWith("ETM Error:"))
                    {
                        _logger.Error(line);
                    }
                }
            }
            Current.Dispatcher.Invoke(callback: () => { Application.Current.Shutdown(); });
        }
    }
}
