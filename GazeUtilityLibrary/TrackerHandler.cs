using System;
using System.ComponentModel;
using System.Timers;
using System.Windows;
using System.Windows.Threading;

namespace GazeUtilityLibrary
{
    /// <summary>
    /// The common interface for the Tobii eyetracker Engines Core and Pro
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    /// <seealso cref="System.IDisposable" />
    public abstract class TrackerHandler : INotifyPropertyChanged, IDisposable
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
        protected Timer dialogBoxTimer;
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
        /// Event handler for gaze data events of the eyetracker
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        public delegate void GazeDataHandler(Object sender, GazeDataArgs e);

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
        public TrackerHandler(TrackerLogger logger, int ready_timer, string device_name)
        {
            this.DeviceName = device_name;
            this.logger = logger;
            logger.Info($"Using {DeviceName}");
            dialogBoxTimer = new Timer
            {
                Interval = ready_timer,
                AutoReset = false,
                Enabled = true
            };
            dialogBoxTimer.Elapsed += OnTrackerDisabledTimeout;
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

        public virtual string PatternReplace(string pattern)
        {
            return pattern;
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                dialogBoxTimer.Dispose();
            }
        }

        /// <summary>
        /// Determines whether this eye tracker is ready.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is ready; otherwise, <c>false</c>.
        /// </returns>
        protected bool IsReady() { return (State == DeviceStatus.Tracking); }

        /// <summary>
        /// Called when [gaze data received].
        /// </summary>
        /// <param name="e">The gaze data event data.</param>
        protected virtual void OnGazeDataReceived(GazeDataArgs e) { GazeDataReceived?.Invoke(this, e); }

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
        /// <param name="e">The <see cref="System.Timers.ElapsedEventArgs"/> instance containing the event data.</param>
        protected void OnTrackerDisabledTimeout(Object? source, System.Timers.ElapsedEventArgs e)
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
                        if (!IsReady()) dialogBoxTimer.Start();
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
            if (IsReady() && (state != DeviceStatus.Tracking))
            {
                dialogBoxTimer.Start();
                logger.Info($"{DeviceName} stopped tracking: New state is \"{state}\"");
            }
            this.state = state;
            if (IsReady())
            {
                Application.Current.Dispatcher.Invoke(callback: () => { trackerMessageBox?.Close(); });
                dialogBoxTimer.Stop();
                logger.Info($"{DeviceName} is ready");
                OnTrackerEnabled(new EventArgs());
            }
        }
    }

    /// <summary>
    /// The event argument class for Tobii eyetracker data
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public class GazeDataArgs : EventArgs
    {
        private TimeSpan timestamp;
        private double xCoord;
        private double? xCoordLeft = null;
        private double? xCoordRight = null;
        private double yCoord;
        private double? yCoordLeft = null;
        private double? yCoordRight = null;
        private bool? isValidCoordLeft = null;
        private bool? isValidCoordRight = null;
        private double? dia = null;
        private double? diaLeft = null;
        private double? diaRight = null;
        private bool? isValidDiaLeft = null;
        private bool? isValidDiaRight = null;
        private double? xOriginLeft = null;
        private double? xOriginRight = null;
        private double? yOriginLeft = null;
        private double? yOriginRight = null;
        private double? zOriginLeft = null;
        private double? zOriginRight = null;
        private double? distOrigin = null;
        private double? distOriginLeft = null;
        private double? distOriginRight = null;
        private bool? isValidOriginLeft = null;
        private bool? isValidOriginRight = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="GazeDataArgs"/> class.
        /// </summary>
        /// <param name="timestamp">The timestamp.</param>
        /// <param name="xCoord">The x coord of the gaze point.</param>
        /// <param name="yCoord">The y coord of the gaze point.</param>
        public GazeDataArgs(TimeSpan timestamp, double xCoord, double yCoord)
        {
            this.timestamp = timestamp;
            this.xCoord = xCoord;
            this.yCoord = yCoord;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="GazeDataArgs"/> class.
        /// </summary>
        /// <param name="timestamp">The timestamp.</param>
        /// <param name="xCoord">The x coord of the gaze point.</param>
        /// <param name="xCoordLeft">The x coord of the gaze point of the left eye.</param>
        /// <param name="xCoordRight">The x coord of the gaze point of the right eye.</param>
        /// <param name="yCoord">The y coord of the gaze point.</param>
        /// <param name="yCoordLeft">The y coord of the gaze point of the left eye.</param>
        /// <param name="yCoordRight">The y coord of the gaze point of the right eye.</param>
        /// <param name="isValidCoordLeft">if set to <c>true</c> the gaze point coordinate of the left eye is valid.</param>
        /// <param name="isValidCoordRight">if set to <c>true</c> the gaze point coordinate of the right eye is valid.</param>
        /// <param name="dia">The average diameter of the pupils.</param>
        /// <param name="diaLeft">The diameter of the left pupil.</param>
        /// <param name="diaRight">The diameter of the right pupil.</param>
        /// <param name="isValidDiaLeft">if set to <c>true</c> the diameter of the left pupil is valid.</param>
        /// <param name="isValidDiaRight">if set to <c>true</c> the diameter of the right pupil is valid.</param>
        /// <param name="xOriginLeft">The x coord of the origin position of the left eye.</param>
        /// <param name="yOriginLeft">The y coord of the origin position of the left eye.</param>
        /// <param name="zOriginLeft">The z coord of the origin position of the left eye.</param>
        /// <param name="xOriginRight">The x coord of the origin position of the right eye.</param>
        /// <param name="yOriginRight">The y coord of the origin position of the right eye.</param>
        /// <param name="zOriginRight">The z coord of the origin position of the right eye.</param>
        /// <param name="distOrigin">The distance of the eye origin to the tracker.</param>
        /// <param name="distOriginLeft">The distance of the left eye origin to the tracker.</param>
        /// <param name="distOriginRight">The distance of the right eye origin to the tracker.</param>
        /// <param name="isValidOriginLeft">if set to <c>true</c> the origin point of the left eye is valid.</param>
        /// <param name="isValidOriginRight">if set to <c>true</c> the origin point of the right eye is valid.</param>
        public GazeDataArgs(TimeSpan timestamp, double xCoord, double xCoordLeft, double xCoordRight, double yCoord, double yCoordLeft, double yCoordRight,
            bool isValidCoordLeft, bool isValidCoordRight, double dia, double diaLeft, double diaRight, bool isValidDiaLeft, bool isValidDiaRight,
            double xOriginLeft, double yOriginLeft, double zOriginLeft, double xOriginRight, double yOriginRight, double zOriginRight,
            double distOrigin, double distOriginLeft, double distOriginRight, bool isValidOriginLeft, bool isValidOriginRight)
        {
            this.timestamp = timestamp;
            this.xCoord = xCoord;
            this.xCoordLeft = xCoordLeft;
            this.xCoordRight = xCoordRight;
            this.yCoord = yCoord;
            this.yCoordLeft = yCoordLeft;
            this.yCoordRight = yCoordRight;
            this.isValidCoordLeft = isValidCoordLeft;
            this.isValidCoordRight = isValidCoordRight;
            this.dia = dia;
            this.diaLeft = diaLeft;
            this.diaRight = diaRight;
            this.isValidDiaLeft = isValidDiaLeft;
            this.isValidDiaRight = isValidDiaRight;
            this.xOriginLeft = xOriginLeft;
            this.xOriginRight = xOriginRight;
            this.yOriginLeft = yOriginLeft;
            this.yOriginRight = yOriginRight;
            this.zOriginLeft = zOriginLeft;
            this.zOriginRight = zOriginRight;
            this.distOrigin = distOrigin;
            this.distOriginLeft = distOriginLeft;
            this.distOriginRight = distOriginRight;
            this.isValidOriginLeft = isValidOriginLeft;
            this.isValidOriginRight = isValidOriginRight;
        }
        public TimeSpan Timestamp { get { return timestamp; } }
        public double XCoord { get { return xCoord; } }
        public double? XCoordLeft { get { return xCoordLeft; } }
        public double? XCoordRight { get { return xCoordRight; } }
        public double YCoord { get { return yCoord; } }
        public double? YCoordLeft { get { return yCoordLeft; } }
        public double? YCoordRight { get { return yCoordRight; } }
        public bool? IsValidCoordLeft { get { return isValidCoordLeft; } }
        public bool? IsValidCoordRight { get { return isValidCoordRight; } }
        public double? Dia { get { return dia; } }
        public double? DiaLeft { get { return diaLeft; } }
        public double? DiaRight { get { return diaRight; } }
        public bool? IsValidDiaLeft { get { return isValidDiaLeft; } }
        public bool? IsValidDiaRight { get { return isValidDiaRight; } }
        public double? XOriginLeft { get { return xOriginLeft; } }
        public double? XOriginRight { get { return xOriginRight; } }
        public double? YOriginLeft { get { return yOriginLeft; } }
        public double? YOriginRight { get { return yOriginRight; } }
        public double? ZOriginLeft { get { return zOriginLeft; } }
        public double? ZOriginRight { get { return zOriginRight; } }
        public double? DistOrigin { get { return distOrigin; } }
        public double? DistOriginLeft { get { return distOriginLeft; } }
        public double? DistOriginRight { get { return distOriginRight; } }
        public bool? IsValidOriginLeft { get { return isValidOriginLeft; } }
        public bool? IsValidOriginRight { get { return isValidOriginRight; } }
    }
}
