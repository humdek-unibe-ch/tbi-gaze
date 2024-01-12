/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
﻿using System;
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
using GazeUtilityLibrary.Tracker;
using GazeUtilityLibrary.DataStructs;
using Newtonsoft.Json;
using System.Reflection;
using WpfScreenHelper;
using System.Linq;
using System.Diagnostics;
using GazeControlLibrary;
using System.Windows.Media;

namespace GazeToMouse
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private object _lock = new object();
        private TimeSpan _startTime;
        /// <summary>
        /// The start time of the application.
        /// </summary>
        public TimeSpan StartTime
        {
            get {
                lock(_lock)
                {
                    return _startTime;
                }
            }
            set {
                lock (_lock)
                {
                    _startTime = value;
                }
            }
        }
        private BaseTracker? _tracker = null;
        private TrackerLogger _logger;
        public TrackerLogger Logger { get { return _logger;  } }
        private MouseHider? _hider = null;
        private GazeDataError _gazeError = new GazeDataError();
        private CalibrationDataError _calibrationError = new CalibrationDataError();
        private CalibrationDataError _validationError = new CalibrationDataError();
        private GazeConfiguration _config;
        private bool _isRecording = true;
        private bool _isMouseTracking = false;
        private bool _isCalibrationOn = false;
        private bool _isValidationOn = false;
        private Dispatcher _dispatcher;
        private Dispatcher CustomDispatcher { get { return _dispatcher; } }
        private TaskCompletionSource<bool> _processCompletion = new TaskCompletionSource<bool>();
        private DriftCompensationWindow? _fixationWindow = null;
        private CalibrationWindow _calibrationWindow = new CalibrationWindow();
        private CalibrationWindow _validationWindow = new CalibrationWindow();
        private CalibrationModel _calibrationModel;
        private CalibrationModel _validationModel;
        private string? _subjectCode = null;
        private TimeSpan _tagTime;
        private string _lastTag = "";
        /// <summary>
        /// The last tag to annotate gaze data.
        /// </summary>
        public string LastTag
        {
            get
            {
                lock (_lastTag)
                {
                    return _lastTag;
                }
            }
            set
            {
                lock (_lastTag)
                {
                    _lastTag = value;
                }
            }
        }
        private string _tag = "";
        /// <summary>
        /// An arbitary tag to annotate gaze data.
        /// </summary>
        public string Tag
        {
            get
            {
                lock (_tag)
                {
                    return _tag;
                }
            }
            set
            {
                lock (_tag)
                {
                    _tagTime = TimeSpan.FromMilliseconds(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);
                    LastTag = _tag;
                    _tag = value;
                }
            }
        }
        private TimeSpan _trialIdTime;
        private int _lastTrialId = 0;
        private int _trialId = 0;
        /// <summary>
        /// The trial ID to annotate gaze data.
        /// </summary>
        public int TrialId
        {
            get
            {
                return _trialId;
            }
            set
            {
                _trialIdTime = TimeSpan.FromMilliseconds(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);
                Interlocked.Exchange(ref _lastTrialId, _trialId);
                Interlocked.Exchange(ref _trialId, value);
            }
        }
        private string? _outputPath = null;

        [DllImport("User32.dll")]
        private static extern bool SetCursorPos(int x, int y);

        /// <summary>
        /// Enable gaze recordings to disk.
        /// </summary>
        public void GazeRecordingEnable()
        {
            _isRecording = true;
        }

        /// <summary>
        /// Disable gaze recordings.
        /// </summary>
        public void GazeRecordingDisable()
        {
            _isRecording = false;
        }

        /// <summary>
        /// Enable mouse tracking which updates the mouse position to the current gaze point.
        /// </summary>
        public void MouseTrackingEnable()
        {
            _isMouseTracking = true;
        }

        /// <summary>
        /// Disable mouse tracking.
        /// </summary>
        public void MouseTrackingDisable()
        {
            _isMouseTracking = false;
        }

        /// <summary>
        /// Reset the current drift compensation offset to zero.
        /// </summary>
        public void ResetDriftCompensation()
        {
            _tracker?.ResetDriftCompensation();
        }

        /// <summary>
        /// Start the drift compensation process
        /// </summary>
        /// <returns>True on success, false on failure</returns>
        public async Task<bool> CompensateDrift()
        {
            if (_fixationWindow != null)
            {
                Current.Dispatcher.Invoke(() =>
                {
                    _fixationWindow.Topmost = true;
                    _fixationWindow.Show();
                    _fixationWindow.Activate();
                });
            }
            System.Timers.Timer? timer = null;
            if (_config.Config.DriftCompensationTimer > 0)
            {
                timer = new System.Timers.Timer(_config.Config.ReadyTimer);
                timer.Elapsed += (sender, e) =>
                {
                    _logger.Warning("drift compensation timed out");
                    _processCompletion.SetResult(false);
                };
                timer.Start();
            }
            await Task.Delay(500);
            _tracker?.StartDriftCompensation();
            bool res = await _processCompletion.Task;

            if (_fixationWindow != null)
            {
                Current.Dispatcher.Invoke(() =>
                {
                    _fixationWindow.Hide();
                });
            }
            timer?.Dispose();
            _processCompletion = new TaskCompletionSource<bool>();
            return res;
        }

        /// <summary>
        /// Start the gaze calibration process
        /// </summary>
        /// <returns>True on success, false on failure</returns>
        public async Task<bool> CustomCalibrate()
        {
            if (_tracker == null)
            {
                return false;
            }

            Current.Dispatcher.Invoke(() =>
            {
                _calibrationWindow.Topmost = true;
                _calibrationWindow.Show();
                _calibrationWindow.Activate();
            });
            if (Screen.AllScreens.Count() > 1)
            {
                _calibrationModel.Status = CalibrationStatus.ScreenSelection;
            }
            else
            {
                OnCalibrationEvent(null, CalibrationEventType.Init);
            }
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

        /// <summary>
        /// Start the gaze calibration process
        /// </summary>
        /// <returns>True on success, false on failure</returns>
        public async Task<bool> CalibrationValidate()
        {
            if (_tracker == null)
            {
                return false;
            }
            Current.Dispatcher.Invoke(() => {
                _validationWindow.Topmost = true;
                _validationWindow.Show();
                _validationWindow.Activate();
            });
            if (Screen.AllScreens.Count() > 1)
            {
                _validationModel.Status = CalibrationStatus.ScreenSelection;
            }
            else
            {
                OnValidationEvent(null, CalibrationEventType.Init);
            }
            _isValidationOn = true;
            bool res = await _processCompletion.Task;
            _isValidationOn = false;
            Current.Dispatcher.Invoke(() => {
                _validationWindow.Hide();
            });
            _processCompletion = new TaskCompletionSource<bool>();
            return res;
        }

        /// <summary>
        /// Constructor: initialised logger, gaze configuration, pipe server, and calibration model
        /// </summary>
        public App()
        {
            _logger = new TrackerLogger(null);
            _dispatcher = Dispatcher.CurrentDispatcher;
            _dispatcher.ShutdownStarted += OnDispatcherShutdownStarted;
            ThreadPool.QueueUserWorkItem(HandlePipeSignals, this);

            Process[] gazeProcesses = Process.GetProcessesByName("Gaze");
            int currentProcessId = Process.GetCurrentProcess().Id;
            foreach (Process gazeProcess in gazeProcesses)
            {
                if (gazeProcess.Id != currentProcessId)
                {
                    _logger.Warning($"A Process 'Gaze' with ID {gazeProcess.Id} is already running: killing the process.");
                    gazeProcess.Kill();
                }
            }

            _config = new GazeConfiguration(_logger);
            _startTime = TimeSpan.FromMilliseconds(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);

            Color backgroundColor = (Color)ColorConverter.ConvertFromString(_config.Config.BackgroundColor);
            Color frameColor = (Color)ColorConverter.ConvertFromString(_config.Config.FrameColor);

            if (!Init(backgroundColor))
            {
                _logger.Error("Failed to initialise the gaze process, aborting");
                Current.Shutdown();
            }
            
            _calibrationModel = new CalibrationModel(_logger, _config.Config.CalibrationPoints, backgroundColor, frameColor);
            _calibrationModel.CalibrationEvent += OnCalibrationEvent;
            _calibrationWindow.Content = new CalibrationFrame(_calibrationModel, _calibrationWindow);
            _validationModel = new CalibrationModel(_logger, _config.Config.ValidationPoints, backgroundColor, frameColor);
            _validationModel.CalibrationEvent += OnValidationEvent;
            _validationWindow.Content = new CalibrationFrame(_validationModel, _validationWindow);

            // hide the mouse cursor on calibration window
            if (_config.Config.MouseCalibrationHide)
            {
                _calibrationModel.CursorType = Cursors.None;
                _validationModel.CursorType = Cursors.None;
                if (_fixationWindow != null)
                {
                    _fixationWindow.Cursor = Cursors.None;
                }
            }
        }

        /// <summary>
        /// Initialise the application: configure windows, apply gaze configurations, connect to tracking device 
        /// </summary>
        /// <returns>True on success, false on failure</returns>
        private bool Init(Color backgroundColor)
        {
            if (!_config.InitConfig())
            {
                return false;
            }

            if (_config.Config.DriftCompensationWindowShow)
            {
                _fixationWindow = new DriftCompensationWindow(backgroundColor);
                _fixationWindow.WindowStyle = WindowStyle.None;
                _fixationWindow.WindowState = WindowState.Maximized;
                _fixationWindow.ResizeMode = ResizeMode.NoResize;
                _fixationWindow.WindowStartupLocation = WindowStartupLocation.Manual;
                _fixationWindow.Title = "FixationWindow";
                _fixationWindow.ShowInTaskbar = false;
            }

            _calibrationWindow.WindowStyle = WindowStyle.None;
            _calibrationWindow.WindowState = WindowState.Maximized;
            _calibrationWindow.ResizeMode = ResizeMode.NoResize;
            _calibrationWindow.WindowStartupLocation = WindowStartupLocation.Manual;
            _calibrationWindow.Title = "CalibrationWindow";
            _calibrationWindow.ShowInTaskbar = false;

            _validationWindow.WindowStyle = WindowStyle.None;
            _validationWindow.WindowState = WindowState.Maximized;
            _validationWindow.ResizeMode = ResizeMode.NoResize;
            _validationWindow.WindowStartupLocation = WindowStartupLocation.Manual;
            _validationWindow.Title = "ValidationWindow";
            _validationWindow.ShowInTaskbar = false;

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
                EyeTrackerPro tracker_pro = new EyeTrackerPro(_logger, _config.Config);
                if (tracker_pro.IsLicenseOk())
                {
                    _tracker = tracker_pro;
                }
                else
                {
                    tracker_pro.Dispose();
                    _gazeError.Error = EGazeDataError.FallbackToMouse;
                    _tracker = new MouseTracker(_logger, _config.Config);
                }
            }
            else if (_config.Config.TrackerDevice == 2)
            {
                _tracker = new MouseTracker(_logger, _config.Config);
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

            if (_tracker.ScreenArea != null)
            {
                _config.Config.ScreenArea = new ConfigScreenArea(_tracker.ScreenArea);
            }

            _tracker.GazeDataReceived += OnGazeDataReceived;
            _tracker.TrackerEnabled += OnTrackerEnabled;
            _tracker.TrackerDisabled += OnTrackerDisabled;
            _tracker.DriftCompensationComputed += (sender, e) => { _processCompletion.SetResult(true); };

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
        /// Sets the error property and logs the error message
        /// </summary>
        /// <param name="error">The error message.</param>
        private void HandleValidationError(string error)
        {
            _validationModel.Error = error;
            _logger.Error(_validationModel.Error);
        }

        /// <summary>
        /// Handler passed to the thread which listens to the termination signal.
        /// </summary>
        /// <param name="data">The context passed to the handler.</param>
        private async void HandlePipeSignals(object? data)
        {
            bool res = false;
            bool reply = false;
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
                    app.Logger.Debug($"Pipe server started on pipe '{pipeName}'");
                    StreamReader sr = new StreamReader(pipeServer);
                    StreamWriter sw = new StreamWriter(pipeServer);
                    app.Logger.Debug("Wait for a client to connect...");
                    pipeServer.WaitForConnection();
                    string? line = null;
                    while (pipeServer.IsConnected)
                    {
                        app.Logger.Debug("Client connected");
                        line = sr.ReadLine();
                        if (line == null)
                        {
                            pipeServer.Close();
                            app.Logger.Debug("Pipe server stopped: end of input stream reached");
                            break;
                        }
                        PipeCommand? msg = JsonConvert.DeserializeObject<PipeCommand>(line);

                        if (msg == null)
                        {
                            pipeServer.Close();
                            app.Logger.Warning("Pipe server stopped: unable to parse pipe message");
                            break;
                        }

                        app.CustomDispatcher.Invoke(() =>
                        {
                            if (msg.ResetStartTime == true)
                            {
                                app.Logger.Info("Pipe command: Reset start time");
                                app.StartTime = TimeSpan.FromMilliseconds(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);
                            }
                            if (msg.Label != null)
                            {
                                app.Logger.Info($"Pipe command: Set new label {msg.Label}");
                                app.Tag = msg.Label;
                            }
                            if (msg.TrialId != null)
                            {
                                app.Logger.Info($"Pipe command: Set new trial ID {msg.TrialId}");
                                app.TrialId = msg.TrialId ?? 0;
                            }
                        });

                        if (msg.Command != null)
                        {
                            app.Logger.Info($"Pipe command: {msg.Command}");
                            switch (msg.Command)
                            {
                                case "TERMINATE":
                                    app.CustomDispatcher.InvokeShutdown();
                                    pipeServer.Close();
                                    return;
                                case "GAZE_RECORDING_DISABLE":
                                    app.CustomDispatcher.Invoke(() => app.GazeRecordingDisable());
                                    break;
                                case "GAZE_RECORDING_ENABLE":
                                    app.CustomDispatcher.Invoke(() => app.GazeRecordingEnable());
                                    break;
                                case "MOUSE_TRACKING_DISABLE":
                                    app.CustomDispatcher.Invoke(() => app.MouseTrackingDisable());
                                    break;
                                case "MOUSE_TRACKING_ENABLE":
                                    app.CustomDispatcher.Invoke(() => app.MouseTrackingEnable());
                                    break;
                                case "RESET_DRIFT_COMPENSATION":
                                    app.CustomDispatcher.Invoke(() => app.ResetDriftCompensation());
                                    break;
                                case "DRIFT_COMPENSATION":
                                    res = await app.CustomDispatcher.Invoke(() => app.CompensateDrift());
                                    reply = true;
                                    break;
                                case "CUSTOM_CALIBRATE":
                                    res = await app.CustomDispatcher.Invoke(() => app.CustomCalibrate());
                                    reply = true;
                                    break;
                                case "VALIDATE":
                                    res = await app.CustomDispatcher.Invoke(() => app.CalibrationValidate());
                                    reply = true;
                                    break;
                            }

                            if (reply)
                            {
                                app.Logger.Info($"Pipe command result: {res}");
                                if (res)
                                {
                                    sw.WriteLine("SUCCESS");
                                }
                                else
                                {
                                    sw.WriteLine("FAILED");
                                }
                                sw.Flush();
                                reply = false;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Collect the calibration data.
        /// </summary>
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

                bool res = await _tracker.CollectCalibrationDataAsync(point);
                _calibrationModel.GazeDataCollected();

                if (!res)
                {
                    HandleCalibrationError($"Failed to collect data for calibration point at [{point.X}, {point.Y}]");
                    // Wait a little.
                    await Task.Delay(700);
                    continue;
                }

                _logger.Debug($"Calibration data collected at point [{point.X}, {point.Y}]");

                // Wait a little.
                await Task.Delay(700);
            }
        }

        /// <summary>
        /// Collect the validation data.
        /// </summary>
        /// <returns></returns>
        private async Task CollectValidationData()
        {
            if (_tracker == null)
            {
                return;
            }

            // Collect data.
            foreach (Point point in _validationModel.Points)
            {
                _logger.Debug($"Show validation point at [{point.X}, {point.Y}]");
                _validationModel.NextCalibrationPoint();

                // Wait a little for user to focus.
                await Task.Delay(1000);

                bool res = await _tracker.CollectValidationDataAsync(point);
                _validationModel.GazeDataCollected();

                if (!res)
                {
                    HandleValidationError($"Failed to collect data for validation point at [{point.X}, {point.Y}]");
                    break;
                }

                _logger.Debug($"Validation data collected at point [{point.X}, {point.Y}]");

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
            if (_tracker == null)
            {
                return;
            }

            if (!_config.PrepareCalibrationOutputFile(_subjectCode))
            {
                throw new Exception("Failed to prepare calibration output file");
            }

            _calibrationModel.InitCalibration();
            _calibrationModel.Status = CalibrationStatus.DataCollection;

            await _tracker.InitCalibrationAsync();

            await CollectCalibrationData();

            _calibrationModel.Status = CalibrationStatus.Computing;

            List<GazeCalibrationData> calibrationResult = await _tracker.ApplyCalibration();
            foreach(GazeCalibrationData item in calibrationResult)
            {
                string[] formattedValues = item.Prepare(_config.Config);
                _config.WriteToCalibrationOutput(formattedValues);
            }

            _calibrationModel.SetCalibrationResult(calibrationResult);
            _calibrationModel.Status = CalibrationStatus.CalibrationResult;

            await _tracker.FinishCalibrationAsync();

            _config.CleanupCalibrationOutputFile(_calibrationError.GetCalibrationDataErrorString());
        }

        /// <summary>
        /// Calibrat the eyetracker.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private async Task Validate()
        {
            if (_tracker == null)
            {
                return;
            }

            if (!_config.PrepareValidationOutputFile(_subjectCode))
            {
                throw new Exception("Failed to prepare validation output file");
            }

            _validationModel.InitCalibration();
            _validationModel.Status = CalibrationStatus.DataCollection;

            _tracker.InitValidation();

            await CollectValidationData();

            _validationModel.Status = CalibrationStatus.Computing;

            GazeValidationData? validationResult = _tracker.ComputeValidation();
            if(validationResult != null)
            {
                string[] formattedValues;
                foreach (GazeValidationPoint point in validationResult.Points)
                {
                    formattedValues = point.Prepare(_config.Config);
                    _config.WriteToValidationOutput(formattedValues);
                }
                _validationModel.ValidationData = validationResult;
            }

            _validationModel.Status = CalibrationStatus.ValidationResult;

            _tracker.FinishValidation();

            _config.CleanupValidationOutputFile(_validationError.GetCalibrationDataErrorString());
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
                        case "outputPath":
                            i++;
                            _outputPath = e.Args[i];
                            break;
                    }
                }
            }

            if (!_config.PrepareGazeOutputFile(_subjectCode, _outputPath))
            {
                Current.Shutdown();
            }
            if (!_config.DumpCurrentConfigurationFile())
            {
                Current.Shutdown();
            }

            _logger.Info($"Starting \"{AppDomain.CurrentDomain.BaseDirectory}Gaze.exe\" {String.Join(" ", e.Args)}");
            _logger.Info($"Version {Assembly.GetExecutingAssembly().GetName().Version}");
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
                case CalibrationEventType.Init:
                    _calibrationModel.Status = CalibrationStatus.HeadPosition;
                    break;
                case CalibrationEventType.Accept:
                    _processCompletion.SetResult(true);
                    break;
                case CalibrationEventType.Abort:
                    _processCompletion.SetResult(false);
                    break;
                case CalibrationEventType.Restart:
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
                    break;
            }
        }

        /// <summary>
        /// Called on a calibration event from the calibration model.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="type">The calibration event type.</param>
        private async void OnValidationEvent(object? sender, CalibrationEventType type)
        {
            _logger.Info($"Received validation user event {type.ToString()}");
            switch (type)
            {
                case CalibrationEventType.Init:
                    goto case CalibrationEventType.Start;
                case CalibrationEventType.Accept:
                    _processCompletion.SetResult(true);
                    break;
                case CalibrationEventType.Abort:
                    _processCompletion.SetResult(false);
                    break;
                case CalibrationEventType.Restart:
                    goto case CalibrationEventType.Start;
                case CalibrationEventType.Start:
                    _validationModel.Error = "No Error";
                    try
                    {
                        await Validate();
                    }
                    catch (Exception ex)
                    {
                        HandleCalibrationError($"Validation failed due to an exception: {ex.Message}");
                        _validationModel.Status = CalibrationStatus.Error;
                        _config.CleanupValidationOutputFile(_validationError.GetCalibrationDataErrorString());
                    }
                    break;
            }
        }

        /// <summary>
        /// Called when [gaze data received].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="data">The data.</param>
        private void OnGazeDataReceived(Object? sender, GazeData data)
        {
            // write the coordinates to the log file
            if (_config != null && _config.Config.DataLogWriteOutput)
            {
                string[] formatted_values = data.Prepare(
                    _config.Config,
                    data.Timestamp < _trialIdTime ? _lastTrialId : this.TrialId,
                    data.Timestamp < _tagTime ? this.LastTag : this.Tag,
                    this.StartTime);
                if (_isRecording)
                {
                    _config.WriteToGazeOutput(formatted_values);
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
        private void OnUserPositionGuideReceived(object? sender, UserPositionData e)
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
            if (_isValidationOn)
            {
                switch (_validationModel.LastStatus)
                {
                    case CalibrationStatus.DataCollection:
                        _validationModel.Status = CalibrationStatus.Error;
                        break;
                    default:
                        _validationModel.Status = _validationModel.LastStatus;
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

            if (_isValidationOn)
            {
                switch (_validationModel.Status)
                {
                    case CalibrationStatus.DataCollection:
                        _validationModel.Error = "Connection to the device interrupted, validation aborted.";
                        break;
                }
                _validationModel.Status = CalibrationStatus.Disconnect;
                _validationError.Error = ECalibrationDataError.DeviceInterrupt;
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
