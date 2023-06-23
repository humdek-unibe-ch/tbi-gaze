using System;
using System.ComponentModel;
using System.Timers;
using System.Windows;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Numerics;
using GazeUtilityLibrary.DataStructs;

namespace GazeUtilityLibrary.Tracker
{
    /// <summary>
    /// The common interface for the Tobii eyetracker Engines Core and Pro
    /// </summary>
    /// <seealso cref="INotifyPropertyChanged" />
    /// <seealso cref="IDisposable" />
    public abstract class BaseTracker : INotifyPropertyChanged, IDisposable
    {
        public enum DeviceStatus
        {
            Configuring,
            Initializing,
            InvalidConfiguration,
            DeviceNotConnected,
            Tracking
        }
        private DeviceStatus state;

        /// <summary>
        /// Timer to control the apperance of the dialog box
        /// </summary>
        protected Timer? dialogBoxTimer;
        /// <summary>
        /// The logger
        /// </summary>
        protected TrackerLogger logger;
        /// <summary>
        /// The dialog box that is controlled by the dialogBoxTimer
        /// </summary>
        protected TrackerMessageBox? trackerMessageBox;
        /// <summary>
        /// The name of the tracker device
        /// </summary>
        public readonly string DeviceName;
        /// <summary>
        /// drift compensation handler
        /// </summary>
        protected DriftCompensation? driftCompensation;
        /// <summary>
        /// The scrren area structure holding the metrics of the screen in 3d space.
        /// </summary>
        protected ScreenArea? screenArea = null;
        /// <summary>
        /// The gaze configuration item
        /// </summary>
        protected ConfigItem config;

        /// <summary>
        /// Occurs when [tracker enabled].
        /// </summary>
        public event EventHandler? TrackerEnabled;
        /// <summary>
        /// Occurs when [tracker disabled].
        /// </summary>
        public event EventHandler? TrackerDisabled;
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;
        /// <summary>
        /// Occurs when [gaze data received].
        /// </summary>
        public event GazeDataHandler? GazeDataReceived;
        /// <summary>
        /// Occurs when drift compensation was computed.
        /// </summary>
        public event DriftCompensationEventHandler? DriftCompensationComputed;
        /// <summary>
        /// Occurs when [user position data received].
        /// </summary>
        public event UserPositionDataHandler? UserPositionDataReceived;

        /// <summary>
        /// Event handler for gaze data events of the eyetracker
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="gazeData">The e.</param>
        public delegate void GazeDataHandler(object sender, GazeData gazeData);

        /// <summary>
        /// Event handler for drift compensation events
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="driftCompensation">The drift compensation quaternion</param>
        public delegate void DriftCompensationEventHandler(object sender, Quaternion driftCompensation);

        /// <summary>
        /// Event handler for user position data events of the eyetracker
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        public delegate void UserPositionDataHandler(object sender, UserPositionData e);

        /// <summary>
        /// Gets or sets the state of the eyetracker device.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        public DeviceStatus State
        {
            get { return state; }
            set
            {
                if (state == value) return;
                OnUpdateState(value);
                OnPropertyChanged("State");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EyeTrackerHandler" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="ready_timer">The ready timer.</param>
        /// <param name="device_name">Name of the device.</param>
        public BaseTracker(TrackerLogger logger, ConfigItem config, string deviceName)
        {
            this.config = config;
            DeviceName = deviceName;
            this.logger = logger;
            logger.Info($"Using {DeviceName}");
            if (config.ReadyTimer > 0)
            {
                dialogBoxTimer = new Timer
                {
                    Interval = config.ReadyTimer,
                    AutoReset = false,
                    Enabled = true
                };
                dialogBoxTimer.Elapsed += OnTrackerDisabledTimeout;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Replaces a patten string with information from the eye tracker.
        /// This is device specific and may be overwritten by the device class.
        /// </summary>
        /// <returns>
        ///   The string where patterns were replaced.
        /// </returns>
        public virtual string PatternReplace(string pattern)
        {
            return pattern;
        }

        /// <summary>
        /// Initialise the calibartion process. This is device specific and must be overwritten by the device class.
        /// </summary>
        /// <returns>An async handler</returns>
        abstract public Task InitCalibration();

        /// <summary>
        /// Finish the calibartion process. This is device specific and must be overwritten by the device class.
        /// </summary>
        /// <returns>An async handler</returns>
        abstract public Task FinishCalibration();

        /// <summary>
        /// Apply the calibration data. This is device specific and must be overwritten by the device class.
        /// </summary>
        /// <returns>The calibration data result wrapped by an async handler.</returns>
        abstract public Task<List<GazeCalibrationData>> ApplyCalibration();

        /// <summary>
        /// Collect calibration data on a calibration point. This is device specific and must be overwritten by the device class.
        /// </summary>
        /// <param name="point">The calibration point for which to collect data</param>
        /// <returns>True on success, false on failure, wrapped by an async handler.</returns>
        abstract public Task<bool> CollectCalibrationData(Point point);

        /// <summary>
        /// Start the drift compensation process.
        /// </summary>
        public void StartDriftCompensation()
        {
            driftCompensation?.Start();
        }

        /// <summary>
        /// Reset the drift compensation value
        /// </summary>
        public void ResetDriftCompensation()
        {
            driftCompensation?.Reset();
        }

        /// <summary>
        /// Initialise the drift compensation. This is device specific and must be overwritten by the device class.
        /// </summary>
        abstract protected void InitDriftCompensation();

        /// <summary>
        /// Get the number of required gaze samples to compute a fixation.
        /// This is device specific and must be overwritten by the device because the duration of fixation point
        /// detection depends on the frame rate of the device.
        /// </summary>
        /// <returns>The number of gaze samples to require for fixation detection.</returns>
        abstract protected int GetFixationFrameCount();

        /// <summary>
        /// Get the unit vector pointing in the direction of the gaze vector.
        /// This is device specific as the gaze data are represented in a coordinate system as defined by the device.
        /// </summary>
        /// <returns>The unit vector</returns>
        abstract protected Vector3 GetUnitDirection();

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                dialogBoxTimer?.Dispose();
            }
        }

        /// <summary>
        /// Determines whether this eye tracker is ready.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is ready; otherwise, <c>false</c>.
        /// </returns>
        protected bool IsReady() { return State == DeviceStatus.Tracking; }

        /// <summary>
        /// Checks wheter the device is connected and initialised.
        /// This is device specific and may be overwritten. Otherwise true is always returned.
        /// </summary>
        /// <returns>True</returns>
        virtual public bool IsInitialised() { return true; }

        /// <summary>
        /// Called when [gaze data received].
        /// </summary>
        /// <param name="data">The gaze data event data.</param>
        protected virtual void OnGazeDataReceived(GazeData gazeData)
        {
            if (driftCompensation != null)
            {
                if ((gazeData.Combined.GazeData3d?.IsGazePointValid ?? false) && (gazeData.Combined.GazeData3d?.IsGazeOriginValid ?? false))
                {
                    if (driftCompensation.Update(gazeData))
                    {
                        DriftCompensationComputed?.Invoke(this, driftCompensation.Q);
                        logger?.Info($"Add drift compensation [{driftCompensation.Q.X}, {driftCompensation.Q.Y}, {driftCompensation.Q.Z}, {driftCompensation.Q.W}]");
                    }
                }

                if (screenArea != null && gazeData.Combined.GazeData3d != null)
                {
                    gazeData.DriftCompensation = new DriftCompensationData(screenArea, driftCompensation.Q, gazeData.Combined.GazeData3d);
                }
            }
            GazeDataReceived?.Invoke(this, gazeData);
        }

        /// <summary>
        /// Called when [user position data received].
        /// </summary>
        /// <param name="e">The gaze data event data.</param>
        protected virtual void OnUserPositionDataReceived(UserPositionData e) { UserPositionDataReceived?.Invoke(this, e); }

        /// <summary>
        /// Called when when the state property of EyeTracker is changing.
        /// </summary>
        /// <param name="property_name">Name of the property in WPF.</param>
        protected virtual void OnPropertyChanged(string property_name) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property_name)); }

        /// <summary>
        /// Raises the <see cref="E:TrackerDisabled" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected virtual void OnTrackerDisabled(EventArgs e) { TrackerDisabled?.Invoke(this, e); }

        /// <summary>
        /// Called after a specified amount of time of the eyetracker not being ready.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="e">The <see cref="ElapsedEventArgs"/> instance containing the event data.</param>
        protected void OnTrackerDisabledTimeout(object? source, ElapsedEventArgs e)
        {
            OnTrackerDisabled(new EventArgs());
            // EyeTracker instance runs in a worker thread. Create message box in ui thread.
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                trackerMessageBox = new TrackerMessageBox();
                trackerMessageBox.lbStatus.DataContext = this;
                bool? dialogResult = trackerMessageBox.ShowDialog();
                switch (dialogResult)
                {
                    case true:
                        // Abort button was pressed
                        Application.Current.Shutdown();
                        break;
                    case false:
                        if (!IsReady()) dialogBoxTimer?.Start();
                        break;
                }
            }));
        }

        /// <summary>
        /// Raises the <see cref="E:TrackerEnabled" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected virtual void OnTrackerEnabled(EventArgs e) { TrackerEnabled?.Invoke(this, e); }


        /// <summary>
        /// Called when the state of eye tracker is updated.
        /// </summary>
        /// <param name="deviceState">State of the eyetracker device.</param>
        private void OnUpdateState(DeviceStatus state)
        {
            logger.Debug($"{DeviceName} changed state: {state}");
            if (IsReady() && state != DeviceStatus.Tracking)
            {
                logger.Info($"{DeviceName} stopped tracking: New state is \"{state}\"");
                if (dialogBoxTimer == null)
                {
                    OnTrackerDisabled(new EventArgs());
                }
                else
                {
                    dialogBoxTimer?.Start();
                }
            }
            this.state = state;
            if (IsReady())
            {
                Application.Current.Dispatcher.Invoke(callback: () => { trackerMessageBox?.Close(); });
                dialogBoxTimer?.Stop();
                logger.Info($"{DeviceName} is ready");
                OnTrackerEnabled(new EventArgs());
            }
        }
    }
}
