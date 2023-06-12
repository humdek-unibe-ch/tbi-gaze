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

namespace GazeToMouse
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static bool _isFirstSample = true;
        private TrackerHandler? _tracker = null;
        private static TimeSpan delta;
        private TrackerLogger? _logger = null;
        private MouseHider? _hider = null;
        private GazeDataError _error = new GazeDataError();
        private GazeConfiguration? _config = null;
        private bool _isRecording = true;
        private bool _isMouseTracking = false;
        private bool _isDriftCompensationOn = false;
        private Dispatcher? _dispatcher = null;
        public Dispatcher? CurrentDispatcher { get { return _dispatcher; } }
        private TaskCompletionSource<bool> _gazeCollection = new TaskCompletionSource<bool>();
        private DriftCompensationWindow _window = new DriftCompensationWindow();

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

        public async Task<bool> CompensateDrift()
        {
            Current.Dispatcher.Invoke(() => {
                _window.Show();
            });
            await Task.Delay(1000);
            _isDriftCompensationOn = true;
            bool res = await _gazeCollection.Task;
            _isDriftCompensationOn = false;
            Current.Dispatcher.Invoke(() => {
                _window.Hide();
            });
            _gazeCollection = new TaskCompletionSource<bool>();
            return res;
        }

        /// <summary>
        /// Called when [application starts].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OnApplicationStartup(object sender, StartupEventArgs e)
        {
            _logger = new TrackerLogger();

            _window.WindowStyle = WindowStyle.None;
            _window.WindowState = WindowState.Maximized;
            _window.ResizeMode = ResizeMode.NoResize;

            string? subjectCode = null;
            for(int i =0; i < e.Args.Length; i++)
            {
                if (e.Args[i].StartsWith("/"))
                {
                    switch (e.Args[i].Substring(1))
                    {
                        case "subject":
                            i++;
                            subjectCode = e.Args[i];
                            break;
                    }
                }
            }

            _dispatcher = Dispatcher.CurrentDispatcher;
            _dispatcher.ShutdownStarted += OnDispatcherShutdownStarted;
            ThreadPool.QueueUserWorkItem(HandlePipeSignals, this);

            _logger.Info($"Starting \"{AppDomain.CurrentDomain.BaseDirectory}GazeToMouse.exe\" {String.Join(" ", e.Args)}");

            _config = new GazeConfiguration(_logger);
            if (!_config.InitConfig())
            {
                Current.Shutdown();
                return;
            }
            if (!_config.PrepareGazeOutputFile(subjectCode))
            {
                Current.Shutdown();
                return;
            }
            if (!_config.DumpCurrentConfigurationFile())
            {
                Current.Shutdown();
                return;
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
                    _error.Error = EGazeDataError.FallbackToMouse;
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
                return;
            }
            _tracker.GazeDataReceived += OnGazeDataReceived;
            _tracker.TrackerEnabled += OnTrackerEnabled;
            _tracker.TrackerDisabled += OnTrackerDisabled;
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
                            app.Dispatcher.InvokeShutdown();
                        }
                        else if (msg.StartsWith("GAZE_RECORDING_DISABLE"))
                        {
                            app.Dispatcher.Invoke(() =>
                            {
                                app.GazeRecordingDisable();
                            });
                        }
                        else if (msg.StartsWith("GAZE_RECORDING_ENABLE"))
                        {
                            app.Dispatcher.Invoke(() =>
                            {
                                app.GazeRecordingEnable();
                            });
                        }
                        else if (msg.StartsWith("MOUSE_TRACKING_DISABLE"))
                        {
                            app.Dispatcher.Invoke(() =>
                            {
                                app.MouseTrackingDisable();
                            });
                        }
                        else if (msg.StartsWith("MOUSE_TRACKING_ENABLE"))
                        {
                            app.Dispatcher.Invoke(() =>
                            {
                                app.MouseTrackingEnable();
                            });
                        }
                        else if (msg.StartsWith("DRIFT_COMPENSATION"))
                        {
                            bool res = await app.Dispatcher.Invoke(() =>
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
                    }
                }
            }
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
        /// Prforms application cleanup duties.
        /// </summary>
        private void Cleanup()
        {
            if (_config != null && _config.Config.MouseControl && _config.Config.MouseControlHide)
            {
                _hider?.ShowCursor(_config.Config.MouseStandardIconPath);
            }
            _tracker?.Dispose();
            _config?.CleanupGazeOutputFile(_error.GetGazeDataErrorString());
            _logger?.Info($"\"{AppDomain.CurrentDomain.BaseDirectory}GazeToMouse.exe\" terminated gracefully{Environment.NewLine}");
        }

        /// <summary>
        /// Gets the gaze data value string.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>a string containing the gaze data value if the value is not null, the empty string otherwise</returns>
        private string GetGazeDataValueString(bool? data)
        {
            return (data == null) ? "" : ((bool)data).ToString();
        }

        /// <summary>
        /// Gets the gaze data value string.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="format">The format of the data will be converted to.</param>
        /// <returns>
        /// a string containing the gaze data value if the value is not null, the empty string otherwise
        /// </returns>
        private string GetGazeDataValueString(double? data, string format = "")
        {
            return (data == null) ? "" : ((double)data).ToString(format);
        }

        /// <summary>
        /// Computes the eye tracker timestamp.
        /// </summary>
        /// <param name="ts">The timestamp.</param>
        /// <param name="format">The format the timestamp is converted to.</param>
        /// <returns>
        /// a string containing the timestamp 
        /// </returns>
        private string GetGazeDataValueString(TimeSpan ts, string format)
        {
            TimeSpan res = ts;
            if (_isFirstSample)
            {
                delta = res - DateTime.Now.TimeOfDay;
                _isFirstSample = false;
            }
            res -= delta;
            return res.ToString(format);
        }

        /// <summary>
        /// Determines whether the gaze data set is valid.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="ignore_invalid">if set to <c>true</c> [ignore invalid].</param>
        /// <returns>
        ///   <c>true</c> if at least one value of the gaze data is valid; otherwise, <c>false</c>.
        /// </returns>
        private bool IsDataValid(GazeDataArgs data, bool ignore_invalid)
        {
            if (!ignore_invalid) return true; // don't check, log everything
            if ((data.IsValidCoordLeft == true)
                || (data.IsValidCoordRight == true)
                || (data.IsValidDiaLeft == true)
                || (data.IsValidDiaRight == true)
                || (data.IsValidOriginLeft == true)
                || (data.IsValidOriginRight == true)
                || ((data.IsValidCoordLeft == null)
                    && (data.IsValidCoordRight == null)
                    && (data.IsValidDiaLeft == null)
                    && (data.IsValidDiaRight == null)
                    && (data.IsValidOriginLeft == null)
                    && (data.IsValidOriginRight == null)))
                return true; // at least one value is valid or Core SDK is used
            else return false; // all vaules of this data set are invalid
        }

        /// <summary>
        /// Called when [gaze data received].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="data">The data.</param>
        private void OnGazeDataReceived(Object? sender, GazeDataArgs data)
        {
            // write the coordinates to the log file
            if (_config != null && _config.Config.DataLogWriteOutput && IsDataValid(data, _config.Config.DataLogIgnoreInvalid))
            {
                string[] formatted_values = new string[Enum.GetNames(typeof(GazeOutputValue)).Length];
                formatted_values[(int)GazeOutputValue.DataTimeStamp] = GetGazeDataValueString(data.Timestamp, _config.Config.DataLogFormatTimeStamp);
                formatted_values[(int)GazeOutputValue.XCoord] = GetGazeDataValueString(data.XCoord);
                formatted_values[(int)GazeOutputValue.XCoordLeft] = GetGazeDataValueString(data.XCoordLeft);
                formatted_values[(int)GazeOutputValue.XCoordRight] = GetGazeDataValueString(data.XCoordRight);
                formatted_values[(int)GazeOutputValue.YCoord] = GetGazeDataValueString(data.YCoord);
                formatted_values[(int)GazeOutputValue.YCoordLeft] = GetGazeDataValueString(data.YCoordLeft);
                formatted_values[(int)GazeOutputValue.YCoordRight] = GetGazeDataValueString(data.YCoordRight);
                formatted_values[(int)GazeOutputValue.ValidCoordLeft] = GetGazeDataValueString(data.IsValidCoordLeft);
                formatted_values[(int)GazeOutputValue.ValidCoordRight] = GetGazeDataValueString(data.IsValidCoordRight);
                formatted_values[(int)GazeOutputValue.PupilDia] = GetGazeDataValueString(data.Dia, _config.Config.DataLogFormatDiameter);
                formatted_values[(int)GazeOutputValue.PupilDiaLeft] = GetGazeDataValueString(data.DiaLeft, _config.Config.DataLogFormatDiameter);
                formatted_values[(int)GazeOutputValue.PupilDiaRight] = GetGazeDataValueString(data.DiaRight, _config.Config.DataLogFormatDiameter);
                formatted_values[(int)GazeOutputValue.ValidPupilLeft] = GetGazeDataValueString(data.IsValidDiaLeft);
                formatted_values[(int)GazeOutputValue.ValidPupilRight] = GetGazeDataValueString(data.IsValidDiaRight);
                formatted_values[(int)GazeOutputValue.XOriginLeft] = GetGazeDataValueString(data.XOriginLeft, _config.Config.DataLogFormatOrigin);
                formatted_values[(int)GazeOutputValue.XOriginRight] = GetGazeDataValueString(data.XOriginRight, _config.Config.DataLogFormatOrigin);
                formatted_values[(int)GazeOutputValue.YOriginLeft] = GetGazeDataValueString(data.YOriginLeft, _config.Config.DataLogFormatOrigin);
                formatted_values[(int)GazeOutputValue.YOriginRight] = GetGazeDataValueString(data.YOriginRight, _config.Config.DataLogFormatOrigin);
                formatted_values[(int)GazeOutputValue.ZOriginLeft] = GetGazeDataValueString(data.ZOriginLeft, _config.Config.DataLogFormatOrigin);
                formatted_values[(int)GazeOutputValue.ZOriginRight] = GetGazeDataValueString(data.ZOriginRight, _config.Config.DataLogFormatOrigin);
                formatted_values[(int)GazeOutputValue.DistOrigin] = GetGazeDataValueString(data.DistOrigin, _config.Config.DataLogFormatOrigin);
                formatted_values[(int)GazeOutputValue.DistOriginLeft] = GetGazeDataValueString(data.DistOriginLeft, _config.Config.DataLogFormatOrigin);
                formatted_values[(int)GazeOutputValue.DistOriginRight] = GetGazeDataValueString(data.DistOriginRight, _config.Config.DataLogFormatOrigin);
                formatted_values[(int)GazeOutputValue.ValidOriginLeft] = GetGazeDataValueString(data.IsValidOriginLeft);
                formatted_values[(int)GazeOutputValue.ValidOriginRight] = GetGazeDataValueString(data.IsValidOriginRight);
                if (_isDriftCompensationOn)
                {
                    if (data.IsValidCoordLeft == true && data.IsValidCoordRight == true
                        && data.XCoordLeft != null && data.YCoordLeft != null
                        && data.XCoordRight != null && data.YCoordRight != null
                        && _tracker != null)
                    {
                        if( _tracker.UpdateDriftCompensation(data))
                        {
                            _logger?.Info($"Add drift compensation [{_tracker.DriftCompensation.XCoordLeft}, {_tracker.DriftCompensation.YCoordLeft}], [{_tracker.DriftCompensation.XCoordRight}, {_tracker.DriftCompensation.YCoordRight}]");
                            _gazeCollection.SetResult(true);
                        }
                    }
                }
                if (_isRecording)
                {
                    _config.WriteToGazeOutput(formatted_values);
                }
            }
            // set the cursor position to the gaze position
            if (_isMouseTracking)
            {
                if (double.IsNaN(data.XCoord) || double.IsNaN(data.YCoord)) return;
                UpdateMousePosition(Convert.ToInt32(data.XCoord * SystemParameters.PrimaryScreenWidth), Convert.ToInt32(data.YCoord * SystemParameters.PrimaryScreenHeight));
            }
        }

        /// <summary>
        /// Called when the eye tracker is ready.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OnTrackerEnabled(object? sender, EventArgs e)
        {
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
            _error.Error = EGazeDataError.DeviceInterrupt;
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
