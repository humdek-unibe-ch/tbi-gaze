using System;
using System.ComponentModel;
using System.Timers;
using System.Windows;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace GazeUtilityLibrary
{
    public class DriftCompensation
    {
        private double _xCoordLeft;
        public double XCoordLeft { get { return _xCoordLeft; } }

        private double _yCoordLeft;
        public double YCoordLeft { get { return _yCoordLeft; } }
        private double _xCoordRight;
        public double XCoordRight { get { return _xCoordRight; } }
        private double _yCoordRight;
        public double YCoordRight { get { return _yCoordRight; } }

        public DriftCompensation()
        {
            _xCoordLeft = 0;
            _yCoordLeft = 0;
            _xCoordRight = 0;
            _yCoordRight = 0;
        }
        public DriftCompensation(double xCoordLeft, double yCoordLeft, double xCoordRight, double yCoordRight)
        {
            _xCoordLeft = xCoordLeft;
            _yCoordLeft = yCoordLeft;
            _xCoordRight = xCoordRight;
            _yCoordRight = yCoordRight;
        }
    }
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

        protected DriftCompensation driftCompensation;
        public DriftCompensation DriftCompensation
        {
            get { return driftCompensation; }
        }

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
        /// Occurs when [user position data received].
        /// </summary>
        public event UserPositionDataHandler? UserPositionDataReceived;

        private List<GazeDataArgs> driftCompensationSamples;

        /// <summary>
        /// Event handler for gaze data events of the eyetracker
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        public delegate void GazeDataHandler(Object sender, GazeDataArgs e);

        /// <summary>
        /// Event handler for user position data events of the eyetracker
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        public delegate void UserPositionDataHandler(Object sender, UserPositionDataArgs e);

        private double normalizedDispersionThreshold;
        public double NormalDispersionThreshold
        {
            get { return normalizedDispersionThreshold; }
            set { normalizedDispersionThreshold = AngleToDist(value); }}

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
            driftCompensationSamples = new List<GazeDataArgs>();
            driftCompensation = new DriftCompensation();
            this.DeviceName = device_name;
            this.logger = logger;
            logger.Info($"Using {DeviceName}");
            normalizedDispersionThreshold = AngleToDist(1);
            if (ready_timer > 0)
            {
                dialogBoxTimer = new Timer
                {
                    Interval = ready_timer,
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

        public virtual string PatternReplace(string pattern)
        {
            return pattern;
        }

        public bool UpdateDriftCompensation(GazeDataArgs args)
        {
            if (args.IsValidOriginLeft != true || args.IsValidOriginRight != true || args.IsValidCoordLeft != true || args.IsValidCoordRight != true )
            {
                return false;
            }

            driftCompensationSamples.Add(args);
            if (driftCompensationSamples.Count > GetFixationFrameCount())
            {
                if (IsFixation(ref driftCompensationSamples))
                {
                    driftCompensation = ComputeDriftCompensation(ref driftCompensationSamples);
                    driftCompensationSamples.Clear();
                    return true;
                }
                else
                {
                    driftCompensationSamples.RemoveAt(0);    
                }
            }
            return false;
        }

        abstract public Task InitCalibration();
        abstract public Task FinishCalibration();
        abstract public Task<List<CalibrationDataArgs>> ApplyCalibration();
        abstract public Task<bool> CollectCalibrationData(Point point);

        private double ComputeDispersion(ref List<GazeDataArgs> samples)
        {
            double xMax = samples.Max(sample => sample.XGazeLeft ?? double.MinValue);
            double yMax = samples.Max(sample => sample.YGazeLeft ?? double.MinValue);
            double zMax = samples.Max(sample => sample.ZGazeLeft ?? double.MinValue);
            double xMin = samples.Min(sample => sample.XGazeLeft ?? double.MaxValue);
            double yMin = samples.Min(sample => sample.YGazeLeft ?? double.MaxValue);
            double zMin = samples.Min(sample => sample.ZGazeLeft ?? double.MaxValue);
            double dispersionLeft = xMax - xMin + yMax - yMin + zMax - zMin;
            xMax = samples.Max(sample => sample.XGazeRight ?? double.MinValue);
            yMax = samples.Max(sample => sample.YGazeRight ?? double.MinValue);
            zMax = samples.Max(sample => sample.ZGazeRight ?? double.MinValue);
            xMin = samples.Min(sample => sample.XGazeRight ?? double.MaxValue);
            yMin = samples.Min(sample => sample.YGazeRight ?? double.MaxValue);
            zMin = samples.Min(sample => sample.ZGazeRight ?? double.MaxValue);
            double dispersionRight = xMax - xMin + yMax - yMin + zMax - zMin;
            return Math.Max(dispersionLeft, dispersionRight);
        }
        private DriftCompensation ComputeDriftCompensation(ref List<GazeDataArgs> samples)
        {
            return new DriftCompensation(
                0.5 - samples.Average(sample => sample.XCoordLeftRaw ?? 0),
                0.5 - samples.Average(sample => sample.YCoordLeftRaw ?? 0),
                0.5 - samples.Average(sample => sample.XCoordRightRaw ?? 0),
                0.5 - samples.Average(sample => sample.YCoordRightRaw ?? 0)
            );
        }

        protected double ComputeMaxDeviation(ref List<GazeDataArgs> samples, double normalizedDispersionThreshold)
        {
            double dist = samples.Average(sample => sample.DistGaze ?? 0);
            return dist * normalizedDispersionThreshold;
        }

        private bool IsFixation(ref List<GazeDataArgs> samples)
        {
            double dispersion = ComputeDispersion(ref samples);
            double maxDeviation = ComputeMaxDeviation(ref samples, normalizedDispersionThreshold);
            return dispersion <= maxDeviation;
        }
        abstract protected int GetFixationFrameCount();

        private double AngleToDist(double angle)
        {
            return Math.Sqrt(2 * (1 - Math.Cos(angle * Math.PI / 180)));
        }

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
        protected bool IsReady() { return (State == DeviceStatus.Tracking); }

        virtual public bool IsInitialised() { return true; }

        /// <summary>
        /// Called when [gaze data received].
        /// </summary>
        /// <param name="e">The gaze data event data.</param>
        protected virtual void OnGazeDataReceived(GazeDataArgs e) { GazeDataReceived?.Invoke(this, e); }

        /// <summary>
        /// Called when [user position data received].
        /// </summary>
        /// <param name="e">The gaze data event data.</param>
        protected virtual void OnUserPositionDataReceived(UserPositionDataArgs e) { UserPositionDataReceived?.Invoke(this, e); }

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
            if (IsReady() && (state != DeviceStatus.Tracking))
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

    /// <summary>
    /// The event argument class for Tobii eyetracker data
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public class GazeDataArgs : EventArgs
    {
        private TimeSpan _timestamp;
        public TimeSpan Timestamp { get { return _timestamp; } }

        private double _xCoord;
        public double XCoord { get { return _xCoord; } }

        private double? _xCoordLeft = null;
        public double? XCoordLeft { get { return _xCoordLeft; } }

        private double? _xCoordLeftRaw = null;
        public double? XCoordLeftRaw { get { return _xCoordLeftRaw; } }

        private double? _xCoordRight = null;
        public double? XCoordRight { get { return _xCoordRight; } }

        private double? _xCoordRightRaw = null;
        public double? XCoordRightRaw { get { return _xCoordRightRaw; } }

        private double _yCoord;
        public double YCoord { get { return _yCoord; } }

        private double? _yCoordLeft = null;
        public double? YCoordLeft { get { return _yCoordLeft; } }

        private double? _yCoordLeftRaw = null;
        public double? YCoordLeftRaw { get { return _yCoordLeftRaw; } }

        private double? _yCoordRight = null;
        public double? YCoordRight { get { return _yCoordRight; } }

        private double? _yCoordRightRaw = null;
        public double? YCoordRightRaw { get { return _yCoordRightRaw; } }

        private bool? _isValidCoordLeft = null;
        public bool? IsValidCoordLeft { get { return _isValidCoordLeft; } }

        private bool? _isValidCoordRight = null;
        public bool? IsValidCoordRight { get { return _isValidCoordRight; } }

        private double? _dia = null;
        public double? Dia { get { return _dia; } }

        private double? _diaLeft = null;
        public double? DiaLeft { get { return _diaLeft; } }

        private double? _diaRight = null;
        public double? DiaRight { get { return _diaRight; } }

        private bool? _isValidDiaLeft = null;
        public bool? IsValidDiaLeft { get { return _isValidDiaLeft; } }

        private bool? _isValidDiaRight = null;
        public bool? IsValidDiaRight { get { return _isValidDiaRight; } }

        private double? _xGazeLeft = null;
        public double? XGazeLeft { get { return _xGazeLeft; } }

        private double? _xGazeRight = null;
        public double? XGazeRight { get { return _xGazeRight; } }

        private double? _yGazeLeft = null;
        public double? YGazeLeft { get { return _yGazeLeft; } }

        private double? _yGazeRight = null;
        public double? YGazeRight { get { return _yGazeRight; } }

        private double? _zGazeLeft = null;
        public double? ZGazeLeft { get { return _zGazeLeft; } }

        private double? _zGazeRight = null;
        public double? ZGazeRight { get { return _zGazeRight; } }

        private double? _distGaze = null;
        public double? DistGaze { get { return _distGaze; } }

        private double? _distGazeLeft = null;
        public double? DistGazeLeft { get { return _distGazeLeft; } }

        private double? _distGazeRight = null;
        public double? DistGazeRight { get { return _distGazeRight; } }

        private double? _xOriginLeft = null;
        public double? XOriginLeft { get { return _xOriginLeft; } }

        private double? _xOriginRight = null;
        public double? XOriginRight { get { return _xOriginRight; } }

        private double? _yOriginLeft = null;
        public double? YOriginLeft { get { return _yOriginLeft; } }

        private double? _yOriginRight = null;
        public double? YOriginRight { get { return _yOriginRight; } }

        private double? _zOriginLeft = null;
        public double? ZOriginLeft { get { return _zOriginLeft; } }

        private double? _zOriginRight = null;
        public double? ZOriginRight { get { return _zOriginRight; } }

        private double? _distOrigin = null;
        public double? DistOrigin { get { return _distOrigin; } }

        private double? _distOriginLeft = null;
        public double? DistOriginLeft { get { return _distOriginLeft; } }

        private double? _distOriginRight = null;
        public double? DistOriginRight { get { return _distOriginRight; } }

        private bool? _isValidOriginLeft = null;
        public bool? IsValidOriginLeft { get { return _isValidOriginLeft; } }

        private bool? _isValidOriginRight = null;
        public bool? IsValidOriginRight { get { return _isValidOriginRight; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="GazeDataArgs"/> class.
        /// </summary>
        /// <param name="timestamp">The timestamp.</param>
        /// <param name="xCoord">The x coord of the gaze point.</param>
        /// <param name="yCoord">The y coord of the gaze point.</param>
        public GazeDataArgs(TimeSpan timestamp, double xCoord, double yCoord)
        {
            this._timestamp = timestamp;
            this._xCoord = xCoord;
            this._yCoord = yCoord;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="GazeDataArgs"/> class.
        /// </summary>
        /// <param name="timestamp">The timestamp.</param>
        /// <param name="xCoord">The x coord of the gaze point.</param>
        /// <param name="xCoordLeft">The x coord of the gaze point of the left eye, drift corrected</param>
        /// <param name="xCoordLeftRaw">The x coord of the gaze point of the left eye.</param>
        /// <param name="xCoordRight">The x coord of the gaze point of the right eye, drif corrected.</param>
        /// <param name="xCoordRightRaw">The x coord of the gaze point of the right eye.</param>
        /// <param name="yCoord">The y coord of the gaze point.</param>
        /// <param name="yCoordLeft">The y coord of the gaze point of the left eye, drift corrected.</param>
        /// <param name="yCoordLeftRaw">The y coord of the gaze point of the left eye.</param>
        /// <param name="yCoordRight">The y coord of the gaze point of the right eye, drift corrected.</param>
        /// <param name="yCoordRightRaw">The y coord of the gaze point of the right eye.</param>
        /// <param name="isValidCoordLeft">if set to <c>true</c> the gaze point coordinate of the left eye is valid.</param>
        /// <param name="isValidCoordRight">if set to <c>true</c> the gaze point coordinate of the right eye is valid.</param>
        /// <param name="dia">The average diameter of the pupils.</param>
        /// <param name="diaLeft">The diameter of the left pupil.</param>
        /// <param name="diaRight">The diameter of the right pupil.</param>
        /// <param name="isValidDiaLeft">if set to <c>true</c> the diameter of the left pupil is valid.</param>
        /// <param name="isValidDiaRight">if set to <c>true</c> the diameter of the right pupil is valid.</param>
        /// <param name="xGazeLeft">The x coord of the gaze position of the left eye.</param>
        /// <param name="yGazeLeft">The y coord of the gaze position of the left eye.</param>
        /// <param name="zGazeLeft">The z coord of the gaze position of the left eye.</param>
        /// <param name="xGazeRight">The x coord of the gaze position of the right eye.</param>
        /// <param name="yGazeRight">The y coord of the gaze position of the right eye.</param>
        /// <param name="zGazeRight">The z coord of the gaze position of the right eye.</param>
        /// <param name="distGaze">The distance of the eye origin to the gaze point.</param>
        /// <param name="distGazeLeft">The distance of the left eye origin to the gaze point.</param>
        /// <param name="distGazeRight">The distance of the right eye origin to the gaze point.</param>
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
        public GazeDataArgs(TimeSpan timestamp, double xCoord, double xCoordLeft, double xCoordLeftRaw, double xCoordRight, double xCoordRightRaw,
            double yCoord, double yCoordLeft, double yCoordLeftRaw, double yCoordRight, double yCoordRightRaw,
            bool isValidCoordLeft, bool isValidCoordRight,
            double dia, double diaLeft, double diaRight, bool isValidDiaLeft, bool isValidDiaRight,
            double xGazeLeft, double yGazeLeft, double zGazeLeft, double xGazeRight, double yGazeRight, double zGazeRight,
            double distGaze, double distGazeLeft, double distGazeRight,
            double xOriginLeft, double yOriginLeft, double zOriginLeft, double xOriginRight, double yOriginRight, double zOriginRight,
            double distOrigin, double distOriginLeft, double distOriginRight, bool isValidOriginLeft, bool isValidOriginRight)
        {
            this._timestamp = timestamp;
            this._xCoord = xCoord;
            this._xCoordLeft = xCoordLeft;
            this._xCoordLeftRaw = xCoordLeftRaw;
            this._xCoordRight = xCoordRight;
            this._xCoordRightRaw = xCoordRightRaw;
            this._yCoord = yCoord;
            this._yCoordLeft = yCoordLeft;
            this._yCoordLeftRaw = yCoordLeftRaw;
            this._yCoordRight = yCoordRight;
            this._yCoordRightRaw = yCoordRightRaw;
            this._isValidCoordLeft = isValidCoordLeft;
            this._isValidCoordRight = isValidCoordRight;
            this._dia = dia;
            this._diaLeft = diaLeft;
            this._diaRight = diaRight;
            this._isValidDiaLeft = isValidDiaLeft;
            this._isValidDiaRight = isValidDiaRight;
            this._xGazeLeft = xGazeLeft;
            this._xGazeRight = xGazeRight;
            this._yGazeLeft = yGazeLeft;
            this._yGazeRight = yGazeRight;
            this._zGazeLeft = zGazeLeft;
            this._zGazeRight = zGazeRight;
            this._distGaze = distGaze;
            this._distGazeLeft = distGazeLeft;
            this._distGazeRight = distGazeRight;
            this._xOriginLeft = xOriginLeft;
            this._xOriginRight = xOriginRight;
            this._yOriginLeft = yOriginLeft;
            this._yOriginRight = yOriginRight;
            this._zOriginLeft = zOriginLeft;
            this._zOriginRight = zOriginRight;
            this._distOrigin = distOrigin;
            this._distOriginLeft = distOriginLeft;
            this._distOriginRight = distOriginRight;
            this._isValidOriginLeft = isValidOriginLeft;
            this._isValidOriginRight = isValidOriginRight;
        }
    }

    /// <summary>
    /// The event argument class for Tobii eyetracker data
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public class CalibrationDataArgs : EventArgs
    {
        private double _xCoord;
        public double XCoord { get { return _xCoord; } }
        private double _yCoord;
        public double YCoord { get { return _yCoord; } }
        private double _xCoordLeft;
        public double XCoordLeft { get { return _xCoordLeft; } }
        private double _yCoordLeft;
        public double YCoordLeft { get { return _yCoordLeft; } }
        private bool _validityLeft;
        public bool ValidityLeft { get { return _validityLeft; } }
        private double _xCoordRight;
        public double XCoordRight { get { return _xCoordRight; } }
        private double _yCoordRight;
        public double YCoordRight { get { return _yCoordRight; } }
        private bool _validityRight;
        public bool ValidityRight { get { return _validityRight; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="GazeDataArgs"/> class.
        /// </summary>
        /// <param name="xCoord">The x coord of the calibration point.</param>
        /// <param name="yCoord">The y coord of the calibration point.</param>
        /// <param name="xCoordLeft">The x coord of the gaze point of the left eye.</param>
        /// <param name="yCoordLeft">The y coord of the gaze point of the left eye.</param>
        /// <param name="validityLeft">the validity of gaze point coordinate of the left eye.</param>
        /// <param name="xCoordRight">The x coord of the gaze point of the right eye.</param>
        /// <param name="yCoordRight">The y coord of the gaze point of the right eye.</param>
        /// <param name="validityRight">the validity of gaze point coordinate of the right eye.</param>
        public CalibrationDataArgs(double xCoord, double yCoord, double xCoordLeft, double yCoordLeft, bool validityLeft, double xCoordRight, double yCoordRight, bool validityRight)
        {
            _xCoord = xCoord;
            _yCoord = yCoord;
            _xCoordLeft = xCoordLeft;
            _yCoordLeft = yCoordLeft;
            _validityLeft = validityLeft;
            _xCoordRight = xCoordRight;
            _yCoordRight = yCoordRight;
            _validityRight = validityRight;
        }
    }

    public class UserPositionDataArgs: EventArgs
    {
        private double _xCoordLeft;
        public double XCoordLeft { get { return _xCoordLeft; } }
        private double _yCoordLeft;
        public double YCoordLeft { get { return _yCoordLeft; } }
        private double _zCoordLeft;
        public double ZCoordLeft { get { return _zCoordLeft; } }
        private double _xCoordRight;
        public double XCoordRight { get { return _xCoordRight; } }
        private double _yCoordRight;
        public double YCoordRight { get { return _yCoordRight; } }
        private double _zCoordRight;
        public double ZCoordRight { get { return _zCoordRight; } }
        public UserPositionDataArgs()
        {
            _xCoordLeft = 0.5;
            _yCoordLeft = 0.5;
            _zCoordLeft = 0.5;
            _xCoordRight = 0.5;
            _yCoordRight = 0.5;
            _zCoordRight = 0.5;
        }
        public UserPositionDataArgs(double xCoordLeft, double yCoordLeft, double zCoordLeft, double xCoordRight, double yCoordRight, double zCoordRight)
        {
            _xCoordLeft = xCoordLeft;
            _yCoordLeft = yCoordLeft;
            _zCoordLeft = zCoordLeft;
            _xCoordRight = xCoordRight;
            _yCoordRight = yCoordRight;
            _zCoordRight = zCoordRight;
        }
    }
}
