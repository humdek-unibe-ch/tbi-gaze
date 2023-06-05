using CustomCalibrationLibrary.Models;
using CustomCalibrationLibrary.Views;
using GazeUtilityLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Tobii.Research;

namespace CustomCalibrate
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TrackerLogger _logger;
        private GazeConfiguration _config;
        private GazeDataError _error = new GazeDataError();
        private CalibrationModel _calibrationModel;
        private EyeTrackerPro? _tracker;
        private List<NormalizedPoint2D> _pointsToCalibrate = new List<NormalizedPoint2D>();
        private bool _restartCalibration = true;

        public MainWindow()
        {
            _logger = new TrackerLogger(EOutputType.calibration);
            _config = new GazeConfiguration(_logger);

            Left = SystemParameters.PrimaryScreenWidth;
            Top = 0;

            if (!Init())
            {
                _logger.Error("Failed to initialise the calibration process, aborting");
                Close();
            }

            if (_config.Config.MouseCalibrationHide)
            {
                Cursor = Cursors.None;
            }

            _calibrationModel = new CalibrationModel(_logger, _config.Config.CalibrationPoints);
            _calibrationModel.CalibrationEvent += OnCalibrationEvent;
            foreach (Point point in _calibrationModel.Points)
            {
                _pointsToCalibrate.Add(new NormalizedPoint2D((float)point.X, (float)point.Y));
            }

            _logger.Info($"Starting \"{AppDomain.CurrentDomain.BaseDirectory}CustomCalibrate.exe\"");
            this.Content = new CalibrationCollection(_calibrationModel);
        }

        /// <summary>
        /// Is called on application shutdown.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnApplicationShutdown(object sender, CancelEventArgs e)
        {
            _tracker?.Dispose();
            _logger?.Info($"\"{AppDomain.CurrentDomain.BaseDirectory}CustomCalibration.exe\" terminated gracefully{Environment.NewLine}");
        }

        /// <summary>
        /// Initialise the eye tracker device.
        /// </summary>
        /// <returns></returns>
        private bool Init()
        {
            if (!_config.InitConfig())
            {
                return false;
            }
            if (_config.Config.TrackerDevice != 1)
            {
                _logger.Warning("Custom calibration only works with Tobii Pro SDK");
                return false;
            }

            try
            {
                _tracker = new EyeTrackerPro(_logger, 0, _config.Config.LicensePath);
            }
            catch (Exception ex)
            {
                _logger.Warning($"Failed to instantiate Tobii Pro SDK: {ex.Message}");
                return false;
            }
            if (!_tracker.IsLicenseOk())
            {
                return false;
            }

            _tracker.TrackerEnabled += OnTrackerEnabled;
            _tracker.TrackerDisabled += OnTrackerDisabled;
            _tracker.GazeDataReceived += OnGazeDataReceived;

            if (_tracker.Device == null)
            {
                _logger.Warning("Failed to initialise the tracker device");
                return false;
            }

            _tracker.Device.UserPositionGuideReceived += OnUserPositionGuideReceived;

            return true;
        }

        /// <summary>
        /// Sets the error property and logs the error message
        /// </summary>
        /// <param name="error">The error message.</param>
        private void HandleCalibrationError(string error)
        {
            _calibrationModel.Error = error;
            _logger.Error(_calibrationModel.Error);
        }

        /// <summary>
        /// Collect the calibration data.
        /// </summary>
        /// <param name="calibration">The Tobii calibration object.</param>
        /// <returns></returns>
        private async Task CollectCalibrationData(ScreenBasedCalibration calibration)
        {
            // Collect data.
            foreach (NormalizedPoint2D point in _pointsToCalibrate)
            {
                _logger.Debug($"Show calibration point at [{point.X}, {point.Y}]");
                _calibrationModel.NextCalibrationPoint();

                // Wait a little for user to focus.
                //System.Threading.Thread.Sleep(1000);
                await Task.Delay(1000);

                int fail_count = 0;
                CalibrationStatus status = CalibrationStatus.Failure;
                status = await calibration.CollectDataAsync(point);
                while (status != CalibrationStatus.Success && fail_count < 3)
                {
                    // Try again if it didn't go well the first time.
                    // Not all eye tracker models will fail at this point, but instead fail on ComputeAndApply.
                    _logger.Warning($"Data collection failed, retry #{fail_count}");
                    fail_count++;
                    status = await calibration.CollectDataAsync(point);
                }

                _calibrationModel.GazeDataCollected();
                if (status != CalibrationStatus.Success)
                {
                    HandleCalibrationError($"Failed to collect data for calibration point at [{point.X}, {point.Y}]");
                    break;
                }

                _logger.Debug($"Calibration data collected at point [{point.X}, {point.Y}]");

                // Wait a little.
                //System.Threading.Thread.Sleep(700);
                await Task.Delay(700);
            }
        }

        /// <summary>
        /// Calibrat the eyetracker.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private async Task Calibrate()
        {
            if (!_restartCalibration)
            {
                return;
            }

            if (_tracker == null)
            {
                throw new Exception("No tracker connected.");
            }

            if (!_config.PrepareCalibrationOutputFile())
            {
                throw new Exception("Failed to prepare calibration output file");
            }

            _calibrationModel.InitCalibration();
            _calibrationModel.Status = CalibrationModel.CalibrationStatus.DataCollection;

            ScreenBasedCalibration screenBasedCalibration = new ScreenBasedCalibration(_tracker.Device);
            await screenBasedCalibration.EnterCalibrationModeAsync();

            await CollectCalibrationData(screenBasedCalibration);

            _calibrationModel.Status = CalibrationModel.CalibrationStatus.Computing;

            Tobii.Research.CalibrationResult calibrationResult;

            calibrationResult = await screenBasedCalibration.ComputeAndApplyAsync();

            _logger.Info($"Calibration returned {calibrationResult.Status} and collected {calibrationResult.CalibrationPoints.Count} points.");


            foreach (Tobii.Research.CalibrationPoint point in calibrationResult.CalibrationPoints )
            {
                foreach (CalibrationSample sample in point.CalibrationSamples)
                {
                    string[] formatted_values = GazeData.PrepareCalibrationData(
                        new CalibrationDataArgs(
                            point.PositionOnDisplayArea.X,
                            point.PositionOnDisplayArea.Y,
                            sample.LeftEye.PositionOnDisplayArea.X,
                            sample.LeftEye.PositionOnDisplayArea.Y,
                            sample.LeftEye.Validity,
                            sample.RightEye.PositionOnDisplayArea.X,
                            sample.RightEye.PositionOnDisplayArea.Y,
                            sample.RightEye.Validity
                        ),
                        _config.Config
                    );
                    _config.WriteToCalibrationOutput( formatted_values );
                }
            }
            _calibrationModel.SetCalibrationResult(calibrationResult.CalibrationPoints);
            _calibrationModel.Status = CalibrationModel.CalibrationStatus.DataResult;

            await screenBasedCalibration.LeaveCalibrationModeAsync();

            _config.CleanupCalibrationOutputFile(_error.GetGazeDataErrorString());
        }

        /// <summary>
        /// Called when [gaze data received].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="data">The data.</param>
        private void OnGazeDataReceived(object? sender, GazeDataArgs data)
        {
            if((data.IsValidCoordLeft ?? false) || (data.IsValidCoordRight ?? false))
            {
                _calibrationModel.UpdateGazePoint(data.XCoord, data.YCoord);
            }
        }

        /// <summary>
        /// Called when user position data is received.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUserPositionGuideReceived(object? sender, UserPositionGuideEventArgs e)
        {
            _calibrationModel.UserPositionGuide = e;
        }

        /// <summary>
        /// Called when the eye tracker is ready.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OnTrackerEnabled(object? sender, EventArgs e)
        {
            _logger.Info("Connection to the device enabled");
            switch (_calibrationModel.LastStatus)
            {
                case CalibrationModel.CalibrationStatus.DataCollection:
                    _calibrationModel.Status = CalibrationModel.CalibrationStatus.Error;
                    break;
                default:
                    _calibrationModel.Status = _calibrationModel.LastStatus;
                    break;
            }
        }

        /// <summary>
        /// Called when the eye tracker changes from ready to any other state.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OnTrackerDisabled(object? sender, EventArgs e)
        {
            _logger.Warning("Connection to the device interrupted");
            switch (_calibrationModel.Status)
            {
                case CalibrationModel.CalibrationStatus.DataCollection:
                    _calibrationModel.Error = "Connection to the device interrupted, calibration aborted.";
                    break;
            }
            _calibrationModel.Status = CalibrationModel.CalibrationStatus.Disconnect;
            _error.Error = EGazeDataError.DeviceInterrupt;
        }

        /// <summary>
        /// Called on a calibration event from the calibration model.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="type">The calibration event type.</param>
        private async void OnCalibrationEvent(object? sender, CalibrationEventType type)
        {
            _logger.Info($"Received calibration user event {type.ToString()}");
            switch(type)
            {
                case CalibrationEventType.Accept:
                case CalibrationEventType.Abort:
                    Close();
                    break;
                case CalibrationEventType.Restart:
                    _restartCalibration = true;
                    goto case CalibrationEventType.Start;
                case CalibrationEventType.Start:
                    _calibrationModel.Error = "No Error";
                    try
                    {
                        await Calibrate();
                    }
                    catch (Exception ex)
                    {
                        HandleCalibrationError($"Calibration failed due to an exception: {ex.Message}");
                        _calibrationModel.Status = CalibrationModel.CalibrationStatus.Error;
                        _config.CleanupCalibrationOutputFile(_error.GetGazeDataErrorString());
                    }
                    finally
                    {
                        _restartCalibration = false;
                    }
                    break;
            }
        }
    }
}