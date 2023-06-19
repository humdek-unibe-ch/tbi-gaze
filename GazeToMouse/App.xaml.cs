using System;
using System.IO.Pipes;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using GazeUtilityLibrary;
using System.Threading;
using System.Windows.Threading;
using System.Threading.Tasks;
using CustomCalibrationLibrary.Views;
using System.Windows.Input;
using CustomCalibrationLibrary.Models;
using System.Collections.Generic;

namespace GazeToMouse
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static bool _isFirstSample = true;
        private TrackerHandler? _tracker = null;
        private static TimeSpan? _delta = null;
        private TrackerLogger _logger;
        private MouseHider? _hider = null;
        private GazeDataError _gazeError = new GazeDataError();
        private CalibrationDataError _calibrationError = new CalibrationDataError();
        private GazeConfiguration _config;
        private bool _isRecording = true;
        private bool _isMouseTracking = false;
        private bool _isDriftCompensationOn = false;
        private bool _isCalibrationOn = false;
        private Dispatcher _dispatcher;
        private Dispatcher CustomDispatcher { get { return _dispatcher; } }
        private TaskCompletionSource<bool> _processCompletion = new TaskCompletionSource<bool>();
        private DriftCompensationWindow _fixationWindow = new DriftCompensationWindow();
        private CalibrationWindow _calibrationWindow = new CalibrationWindow();
        private CalibrationModel _calibrationModel;
        string? _subjectCode = null;
        private bool _restartCalibration = true;

        [DllImport("User32.dll")]
        private static extern bool SetCursorPos(int x, int y);

        public void GazeRecordingEnable()
        {
            _isRecording = true;
        }

        public void GazeRecordingDisable()
        {
            _isRecording = false;
        }
        public void MouseTrackingEnable()
        {
            _isMouseTracking = true;
        }

        public void MouseTrackingDisable()
        {
            _isMouseTracking = false;
        }

        public void ResetDriftCompensation()
        {
            _tracker?.ResetDriftCompensation();
        }

        public async Task<bool> CompensateDrift()
        {
            Current.Dispatcher.Invoke(() => {
                _fixationWindow.Show();
            });
            System.Timers.Timer? timer = null;
            if (_config.Config.DriftCompensationTimer > 0)
            {
                timer = new System.Timers.Timer(_config.Config.ReadyTimer);
                timer.Elapsed += (sender, e) =>
                {
                    _processCompletion.SetResult(false);
                };
                timer.Start();
            }
            await Task.Delay(500);
            _isDriftCompensationOn = true;
            bool res = await _processCompletion.Task;
            _isDriftCompensationOn = false;
            Current.Dispatcher.Invoke(() => {
                _fixationWindow.Hide();
            });
            timer?.Dispose();
            _processCompletion = new TaskCompletionSource<bool>();
            return res;
        }

        public async Task<bool> CustomCalibrate()
        {
            if (_tracker == null)
            {
                return false;
            }
            Current.Dispatcher.Invoke(() => {
                _calibrationWindow.Show();
            });
            _calibrationModel.Status = CalibrationStatus.HeadPosition;
            _tracker.UserPositionDataReceived += OnUserPositionGuideReceived;
            _isCalibrationOn = true;
            bool res = await _processCompletion.Task;
            _isCalibrationOn = false;
            _tracker.UserPositionDataReceived -= OnUserPositionGuideReceived;
            Current.Dispatcher.Invoke(() => {
                _calibrationWindow.Hide();
            });
            _processCompletion = new TaskCompletionSource<bool>();
            return res;
        }

        public App()
        {
            _logger = new TrackerLogger();
            _config = new GazeConfiguration(_logger);

            if (!Init())
            {
                _logger.Error("Failed to initialise the gaze process, aborting");
                Current.Shutdown();
            }

            _dispatcher = Dispatcher.CurrentDispatcher;
            _dispatcher.ShutdownStarted += OnDispatcherShutdownStarted;
            ThreadPool.QueueUserWorkItem(HandlePipeSignals, this);

            _calibrationModel = new CalibrationModel(_logger, _config.Config.CalibrationPoints);
            _calibrationModel.CalibrationEvent += OnCalibrationEvent;
            _calibrationWindow.Content = new CalibrationFrame(_calibrationModel);
        }

        private bool Init()
        {
            if (!_config.InitConfig())
            {
                return false;
            }

            _fixationWindow.WindowStyle = WindowStyle.None;
            _fixationWindow.WindowState = WindowState.Maximized;
            _fixationWindow.ResizeMode = ResizeMode.NoResize;

            _calibrationWindow.WindowStyle = WindowStyle.None;
            _calibrationWindow.WindowState = WindowState.Maximized;
            _calibrationWindow.ResizeMode = ResizeMode.NoResize;

            // hide the mouse cursor on calibration window
            if (_config.Config.MouseCalibrationHide)
            {
                _calibrationWindow.Cursor = Cursors.None;
            }

            // hide the mouse cursor
            _hider = new MouseHider(_logger);
            if (_config.Config.MouseControl && _config.Config.MouseControlHide)
            {
                _hider.HideCursor();
            }

            if (_config.Config.MouseControl)
            {
                _isMouseTracking = true;
            }

            if (_config.Config.DataLogDisabledOnStartup)
            {
                _isRecording = false;
            }

            // intitialise the tracker device 
            if (_config.Config.TrackerDevice == 1)
            {
                EyeTrackerPro tracker_pro = new EyeTrackerPro(_logger, _config.Config.ReadyTimer, _config.Config.LicensePath);
                if (tracker_pro.IsLicenseOk())
                {
                    _tracker = tracker_pro;
                }
                else
                {
                    tracker_pro.Dispose();
                    _gazeError.Error = EGazeDataError.FallbackToMouse;
                    _tracker = new MouseTracker(_logger, _config.Config.ReadyTimer);
                }
            }
            else if (_config.Config.TrackerDevice == 2)
            {
                _tracker = new MouseTracker(_logger, _config.Config.ReadyTimer);
            }
            else
            {
                _logger.Error($"Unknown tracker configuration option {_config.Config.TrackerDevice}");
                return false;
            }

            if (!_tracker.IsInitialised())
            {
                _logger.Warning("Failed to initialise the tracker device");
                return false;
            }

            _tracker.NormalDispersionThreshold = _config.Config.DispersionThreshold;
            _tracker.GazeDataReceived += OnGazeDataReceived;
            _tracker.TrackerEnabled += OnTrackerEnabled;
            _tracker.TrackerDisabled += OnTrackerDisabled;

            return true;
        }

        /// <summary>
        /// Prforms application cleanup duties.
        /// </summary>
        private void Cleanup()
        {
            if (_config != null && _config.Config.MouseControl && _config.Config.MouseControlHide)
            {
                _hider?.ShowCursor(_config.Config.MouseStandardIconPath);
            }
            _tracker?.Dispose();
            _config?.CleanupGazeOutputFile(_gazeError.GetGazeDataErrorString());
            _logger?.Info($"\"{AppDomain.CurrentDomain.BaseDirectory}Gaze.exe\" terminated gracefully{Environment.NewLine}");
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
        /// Handler passed to the thread which listens to the termination signal.
        /// </summary>
        /// <param name="data">The context passed to the handler.</param>
        private async void HandlePipeSignals(object? data)
        {
            if (data == null)
            {
                return;
            }
            App app = (App)data;
            string pipeName = "tobii_gaze";

            while (true)
            {
                using (NamedPipeServerStream pipeServer = new NamedPipeServerStream(pipeName, PipeDirection.InOut))
                {
                    StreamReader sr = new StreamReader(pipeServer);
                    StreamWriter sw = new StreamWriter(pipeServer);
                    // Wait for a client to connect
                    pipeServer.WaitForConnection();
                    string? msg = null;
                    while (pipeServer.IsConnected)
                    {
                        msg = sr.ReadLine();
                        if (msg == null)
                        {
                            break;
                        }

                        if (msg.StartsWith("TERMINATE"))
                        {
                            app.CustomDispatcher.InvokeShutdown();
                        }
                        else if (msg.StartsWith("GAZE_RECORDING_DISABLE"))
                        {
                            app.CustomDispatcher.Invoke(() =>
                            {
                                app.GazeRecordingDisable();
                            });
                        }
                        else if (msg.StartsWith("GAZE_RECORDING_ENABLE"))
                        {
                            app.CustomDispatcher.Invoke(() =>
                            {
                                app.GazeRecordingEnable();
                            });
                        }
                        else if (msg.StartsWith("MOUSE_TRACKING_DISABLE"))
                        {
                            app.CustomDispatcher.Invoke(() =>
                            {
                                app.MouseTrackingDisable();
                            });
                        }
                        else if (msg.StartsWith("MOUSE_TRACKING_ENABLE"))
                        {
                            app.CustomDispatcher.Invoke(() =>
                            {
                                app.MouseTrackingEnable();
                            });
                        }
                        else if (msg.StartsWith("DRIFT_COMPENSATION"))
                        {
                            bool res = await app.CustomDispatcher.Invoke(() =>
                            {
                                return app.CompensateDrift();
                            });
                            if (res)
                            {
                                sw.WriteLine("SUCCESS");
                            }
                            else
                            {
                                sw.WriteLine("FAILED");
                            }
                            sw.Flush();
                        }
                        else if (msg.StartsWith("RESET_DRIFT_COMPENSATION"))
                        {
                            app.CustomDispatcher.Invoke(() =>
                            {
                                app.ResetDriftCompensation();
                            });
                        }
                        else if (msg.StartsWith("CUSTOM_CALIBRATE"))
                        {
                            bool res = await app.CustomDispatcher.Invoke(() =>
                            {
                                return app.CustomCalibrate();
                            });
                            if (res)
                            {
                                sw.WriteLine("SUCCESS");
                            }
                            else
                            {
                                sw.WriteLine("FAILED");
                            }
                            sw.Flush();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Collect the calibration data.
        /// </summary>
        /// <param name="calibration">The Tobii calibration object.</param>
        /// <returns></returns>
        private async Task CollectCalibrationData()
        {
            if (_tracker == null)
            {
                return;
            }

            // Collect data.
            foreach (Point point in _calibrationModel.Points)
            {
                _logger.Debug($"Show calibration point at [{point.X}, {point.Y}]");
                _calibrationModel.NextCalibrationPoint();

                // Wait a little for user to focus.
                await Task.Delay(1000);

                bool res = await _tracker.CollectCalibrationData(point);
                _calibrationModel.GazeDataCollected();

                if (!res)
                {
                    HandleCalibrationError($"Failed to collect data for calibration point at [{point.X}, {point.Y}]");
                    break;
                }

                _logger.Debug($"Calibration data collected at point [{point.X}, {point.Y}]");

                // Wait a little.
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
            if (!_restartCalibration || _tracker == null)
            {
                return;
            }

            if (!_config.PrepareCalibrationOutputFile(_subjectCode))
            {
                throw new Exception("Failed to prepare calibration output file");
            }

            _calibrationModel.InitCalibration();
            _calibrationModel.Status = CalibrationStatus.DataCollection;

            await _tracker.InitCalibration();

            await CollectCalibrationData();

            _calibrationModel.Status = CalibrationStatus.Computing;

            List<CalibrationDataArgs> calibrationResult = await _tracker.ApplyCalibration();
            foreach(CalibrationDataArgs item in calibrationResult)
            {
                string[] formattedValues = GazeData.PrepareCalibrationData(item, _config.Config);
                _config.WriteToCalibrationOutput(formattedValues);
            }

            _calibrationModel.SetCalibrationResult(calibrationResult);
            _calibrationModel.Status = CalibrationStatus.DataResult;

            await _tracker.FinishCalibration();

            _config.CleanupCalibrationOutputFile(_calibrationError.GetCalibrationDataErrorString());
        }

        /// <summary>
        /// Called when [application starts].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OnApplicationStartup(object sender, StartupEventArgs e)
        {
            for (int i = 0; i < e.Args.Length; i++)
            {
                if (e.Args[i].StartsWith("/"))
                {
                    switch (e.Args[i].Substring(1))
                    {
                        case "subject":
                            i++;
                            _subjectCode = e.Args[i];
                            break;
                    }
                }
            }

            if (!_config.PrepareGazeOutputFile(_subjectCode))
            {
                Current.Shutdown();
            }
            if (!_config.DumpCurrentConfigurationFile())
            {
                Current.Shutdown();
            }

            _logger.Info($"Starting \"{AppDomain.CurrentDomain.BaseDirectory}GazeToMouse.exe\" {String.Join(" ", e.Args)}");
        }

        /// <summary>
        /// Called when [application exit].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OnApplicationExit(object? sender, ExitEventArgs e)
        {
            Cleanup();
        }

        /// <summary>
        /// Called when application shutdown is triggered through pipe.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OnDispatcherShutdownStarted(object? sender, EventArgs e)
        {
            Cleanup();
        }

        /// <summary>
        /// Called on a calibration event from the calibration model.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="type">The calibration event type.</param>
        private async void OnCalibrationEvent(object? sender, CalibrationEventType type)
        {
            _logger.Info($"Received calibration user event {type.ToString()}");
            switch (type)
            {
                case CalibrationEventType.Accept:
                    _processCompletion.SetResult(true);
                    break;
                case CalibrationEventType.Abort:
                    _processCompletion.SetResult(false);
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
                        _calibrationModel.Status = CalibrationStatus.Error;
                        _config.CleanupCalibrationOutputFile(_calibrationError.GetCalibrationDataErrorString());
                    }
                    finally
                    {
                        _restartCalibration = false;
                    }
                    break;
            }
        }

        /// <summary>
        /// Called when [gaze data received].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="data">The data.</param>
        private void OnGazeDataReceived(Object? sender, GazeDataArgs data)
        {
            // write the coordinates to the log file
            if (_config != null && _config.Config.DataLogWriteOutput)
            {
                if (_tracker != null && _tracker.ScreenArea != null && data.Combined.GazeData3d != null)
                {
                    data.DriftCompensation = new DriftCompensation(_tracker.ScreenArea, _tracker.DriftCompensation, data.Combined.GazeData3d);
                }
                string[] formatted_values = GazeData.PrepareGazeData(data, _config.Config, ref _delta);
                if (_isRecording)
                {
                    _config.WriteToGazeOutput(formatted_values);
                }
            }
            if (_isDriftCompensationOn)
            {
                if ((data.Combined.GazeData3d?.IsGazePointValid ?? false) && (data.Combined.GazeData3d?.IsGazeOriginValid ?? false))
                {
                    if (_tracker != null && _tracker.UpdateDriftCompensation(data))
                    {
                        _logger?.Info($"Add drift compensation [{_tracker.DriftCompensation.X}, {_tracker.DriftCompensation.Y}, {_tracker.DriftCompensation.Z}, {_tracker.DriftCompensation.W}]");
                        _processCompletion.SetResult(true);
                    }
                }
            }
            if (_isCalibrationOn && data.Combined.GazeData2d.IsGazePointValid == true)
            {
                _calibrationModel.UpdateGazePoint(data.Combined.GazeData2d.GazePoint.X, data.Combined.GazeData2d.GazePoint.Y);
            }
            // set the cursor position to the gaze position
            if (_isMouseTracking)
            {
                if (double.IsNaN(data.Combined.GazeData2d.GazePoint.X) || double.IsNaN(data.Combined.GazeData2d.GazePoint.Y))
                {
                    return;
                }
                UpdateMousePosition(Convert.ToInt32((data.DriftCompensation?.GazePosition2d.X ?? data.Combined.GazeData2d.GazePoint.X) * SystemParameters.PrimaryScreenWidth),
                    Convert.ToInt32((data.DriftCompensation?.GazePosition2d.Y ?? data.Combined.GazeData2d.GazePoint.Y) * SystemParameters.PrimaryScreenHeight));
            }
        }

        /// <summary>
        /// Called when user position data is received.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUserPositionGuideReceived(object? sender, UserPositionDataArgs e)
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
            if (_isCalibrationOn)
            {
                switch (_calibrationModel.LastStatus)
                {
                    case CalibrationStatus.DataCollection:
                        _calibrationModel.Status = CalibrationStatus.Error;
                        break;
                    default:
                        _calibrationModel.Status = _calibrationModel.LastStatus;
                        break;
                }
            }
            if (_config != null && _config.Config.MouseControl && _config.Config.MouseControlHide)
            {
                _hider?.HideCursor();
            }
        }

        /// <summary>
        /// Called when the eye tracker changes from ready to any other state.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OnTrackerDisabled(object? sender, EventArgs e)
        {
            if (_config != null && _config.Config.MouseControl && _config.Config.MouseControlHide)
            {
                _hider?.ShowCursor(_config.Config.MouseStandardIconPath);
            }
            _gazeError.Error = EGazeDataError.DeviceInterrupt; _logger.Warning("Connection to the device interrupted");

            if (_isCalibrationOn)
            {
                switch (_calibrationModel.Status)
                {
                    case CalibrationStatus.DataCollection:
                        _calibrationModel.Error = "Connection to the device interrupted, calibration aborted.";
                        break;
                }
                _calibrationModel.Status = CalibrationStatus.Disconnect;
                _calibrationError.Error = ECalibrationDataError.DeviceInterrupt;
            }
        }

        /// <summary>
        /// Updates the mouse position..
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        private void UpdateMousePosition(int x, int y)
        {
            SetCursorPos(x, y);
        }
    }
}
