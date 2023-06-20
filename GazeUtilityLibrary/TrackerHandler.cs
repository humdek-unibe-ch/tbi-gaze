using System;
using System.ComponentModel;
using System.Timers;
using System.Windows;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Numerics;
using static GazeUtilityLibrary.TrackerHandler;
using System.Windows.Markup;

namespace GazeUtilityLibrary
{
    /// <summary>
    /// The common interface for the Tobii eyetracker Engines Core and Pro
    /// </summary>
    /// <seealso cref="INotifyPropertyChanged" />
    /// <seealso cref="IDisposable" />
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

        protected DriftCompensation? driftCompensation;

        protected ScreenArea? screenArea = null;

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
        /// <param name="e">The e.</param>
        public delegate void GazeDataHandler(Object sender, GazeDataArgs e);

        /// <summary>
        /// Event handler for drift compensation events
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="driftCompensation">The drift compensation quaternion</param>
        public delegate void DriftCompensationEventHandler(Object sender, Quaternion driftCompensation);

        /// <summary>
        /// Event handler for user position data events of the eyetracker
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        public delegate void UserPositionDataHandler(Object sender, UserPositionDataArgs e);

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
        public TrackerHandler(TrackerLogger logger, ConfigItem config, string deviceName)
        {
            this.config = config;
            this.DeviceName = deviceName;
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

        public virtual string PatternReplace(string pattern)
        {
            return pattern;
        }

        abstract public Task InitCalibration();
        abstract public Task FinishCalibration();
        abstract public Task<List<CalibrationDataArgs>> ApplyCalibration();
        abstract public Task<bool> CollectCalibrationData(Point point);


        abstract protected void InitDriftCompensation();
        public void StartDriftCompensation()
        {
            driftCompensation?.Start();
        }
        public void ResetDriftCompensation()
        {
            driftCompensation?.Reset();
        }
        abstract protected int GetFixationFrameCount();
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
        protected bool IsReady() { return (State == DeviceStatus.Tracking); }

        virtual public bool IsInitialised() { return true; }

        /// <summary>
        /// Called when [gaze data received].
        /// </summary>
        /// <param name="data">The gaze data event data.</param>
        protected virtual void OnGazeDataReceived(GazeDataArgs gazeData)
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
                    gazeData.DriftCompensation = new DriftCompensationSample(screenArea, driftCompensation.Q, gazeData.Combined.GazeData3d);
                }
            }
            GazeDataReceived?.Invoke(this, gazeData);
        }

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
    /// The 2d gaze data set.
    /// </summary>
    public class GazeData2d
    {
        private Vector2 _gazePoint;
        public Vector2 GazePoint { get { return _gazePoint; } }

        private bool _isGazePointValid;
        public bool IsGazePointValid { get { return _isGazePointValid; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="GazeData2d"/> class.
        /// </summary>
        /// <param name="gazePoint">The 2d coordinates of the gaze point.</param>
        /// <param name="isGazePointValid">The validity of the 2d gaze point.</param>
        public GazeData2d(Vector2 gazePoint, bool isGazePointValid)
        {
            _gazePoint = gazePoint;
            _isGazePointValid = isGazePointValid;
        }
    }

    /// <summary>
    /// The 3d gaze data set.
    /// </summary>
    public class GazeData3d
    {
        private Vector3 _gazePoint;
        public Vector3 GazePoint { get { return _gazePoint; } }

        private bool _isGazePointValid;
        public bool IsGazePointValid { get { return _isGazePointValid; } }

        private Vector3 _gazeOrigin;
        public Vector3 GazeOrigin { get { return _gazeOrigin; } }

        private Vector3 _gazeDirection;
        public Vector3 GazeDirection { get { return _gazeDirection; } }

        private float _gazeDistance;
        public float GazeDistance { get { return _gazeDistance; } }

        private bool _isGazeOriginValid;
        public bool IsGazeOriginValid { get { return _isGazeOriginValid; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="GazeData3d"/> class.
        /// </summary>
        /// <param name="gazePoint">The 3d coordinates of the gaze point.</param>
        /// <param name="isGazePointValid">The validity of the 3d gaze point.</param>
        /// <param name="gazeOrigin">The 3d coordinates of the gaze origin.</param>
        /// <param name="isGazeOriginValid">The validity of the 3d gaze origin.</param>
        public GazeData3d(Vector3 gazePoint, bool isGazePointValid, Vector3 gazeOrigin, bool isGazeOriginValid)
        {
            _gazePoint = gazePoint;
            _isGazePointValid = isGazePointValid;
            _gazeOrigin = gazeOrigin;
            _gazeDirection = gazePoint - gazeOrigin;
            _gazeDistance = _gazeDirection.Length();
            _isGazeOriginValid = isGazeOriginValid;
        }
    }

    /// <summary>
    /// The eye data set, including pupil information.
    /// </summary>
    public class EyeData
    {
        private float _pupilDiameter;
        public float PupilDiameter { get { return _pupilDiameter; } }

        private bool _isPupilDiameterValid;
        public bool IsPupilDiameterValid { get { return _isPupilDiameterValid; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="EyeData"/> class.
        /// </summary>
        /// <param name="pupilDiameter">The pupil diameter.</param>
        /// <param name="isPupilDiameterValid">The validity of the pupil diameter.</param>
        public EyeData(float pupilDiameter, bool isPupilDiameterValid)
        {
            _pupilDiameter = pupilDiameter;
            _isPupilDiameterValid = isPupilDiameterValid;
        }
    }

    /// <summary>
    /// The gaze data set, including 2d and (optionally) 3d gaze data as well as optional eye data.
    /// </summary>
    public class GazeDataItem
    {
        private GazeData2d _gazeData2d;
        public GazeData2d GazeData2d { get { return _gazeData2d; } }

        private GazeData3d? _gazeData3d = null;
        public GazeData3d? GazeData3d { get { return _gazeData3d; } }

        private EyeData? _eyeData = null;
        public EyeData? EyeData { get { return _eyeData; } }


        /// <summary>
        /// Initializes a new instance of the <see cref="GazeDataItem"/> class.
        /// </summary>
        /// <param name="gazePoint2d">The 2d coordinates of the gaze point.</param>
        /// <param name="isGazePoint2dValid">The validity of the 2d gaze point.</param>
        public GazeDataItem(Vector2 gazePoint2d, bool isGazePoint2dValid)
        {
            _gazeData2d = new GazeData2d(gazePoint2d, isGazePoint2dValid);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GazeDataItem"/> class.
        /// </summary>
        /// <param name="gazePoint2d">The 2d coordinates of the gaze point.</param>
        /// <param name="isGazePoint2dValid">The validity of the 2d gaze point.</param>
        /// <param name="gazePoint3d">The 3d coordinates of the gaze point.</param>
        /// <param name="isGazePoint3dValid">The validity of the 3d gaze point.</param>
        /// <param name="gazeOrigin3d">The 3d coordinates of the gaze origin.</param>
        /// <param name="isGazeOrigin3dValid">The validity of the 3d gaze origin.</param>
        /// <param name="pupilDiameter">The pupil diameter.</param>
        /// <param name="isPupilDiameterValid">The validity of the pupil diameter.</param>
        public GazeDataItem(Vector2 gazePoint2d, bool isGazePoint2dValid, Vector3 gazePoint3d, bool isGazePoint3dValid, Vector3 gazeOrigin3d, bool isGazeOrigin3dValid, float pupilDiameter, bool isPupilDiameterValid)
        {
            _gazeData2d = new GazeData2d(gazePoint2d, isGazePoint2dValid);
            _gazeData3d = new GazeData3d(gazePoint3d, isGazePoint3dValid, gazeOrigin3d, isGazeOrigin3dValid);
            _eyeData = new EyeData(pupilDiameter, isPupilDiameterValid);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class DriftCompensationSample
    {
        private Vector2 _gazePosition2d;
        public Vector2 GazePosition2d { get { return _gazePosition2d; } }

        private Vector3 _gazePosition3d;
        public Vector3 GazePosition3d { get { return _gazePosition3d; } }

        private Quaternion _compensation;
        public Quaternion Compensation { get { return _compensation; } }

        public DriftCompensationSample(ScreenArea screen, Quaternion driftCompensation, GazeData3d gazeData)
        {
            _compensation = driftCompensation;
            Vector3 newGazeDirection = Vector3.Transform(gazeData.GazeDirection, _compensation);
            _gazePosition3d = screen.GetIntersectionPoint(gazeData.GazeOrigin, newGazeDirection) ?? Vector3.Zero;
            _gazePosition2d = screen.GetPoint2dNormalized(_gazePosition3d);
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

        private GazeDataItem? _left = null;
        public GazeDataItem? Left { get { return _left; } }

        private GazeDataItem? _right = null;
        public GazeDataItem? Right { get { return _right; } }

        private GazeDataItem _combined;
        public GazeDataItem Combined { get { return _combined; } }
        public DriftCompensationSample? DriftCompensation { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GazeDataArgs"/> class.
        /// </summary>
        /// <param name="timestamp">The timestamp.</param>
        /// <param name="gazePoint2d">The 2d coordinates of the combined gaze point.</param>
        /// <param name="isGazePoint2dValid">The validity of the combined 2d gaze point.</param>
        public GazeDataArgs(TimeSpan timestamp, Vector2 gazePoint2d, bool isGazePoint2dValid)
        {
            _timestamp = timestamp;
            _combined = new GazeDataItem(gazePoint2d, isGazePoint2dValid);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GazeDataArgs"/> class.
        /// </summary>
        /// <param name="timestamp">The timestamp.</param>
        /// <param name="gazePoint2dLeft">The 2d coordinates of the left gaze point.</param>
        /// <param name="isGazePoint2dValidLeft">The validity of the left 2d gaze point.</param>
        /// <param name="gazePoint2dRight">The 2d coordinates of the right gaze point.</param>
        /// <param name="isGazePoint2dValidRight">The validity of the right 2d gaze point.</param>
        public GazeDataArgs(TimeSpan timestamp, Vector2 gazePoint2dLeft, bool isGazePoint2dValidLeft, Vector2 gazePoint2dRight, bool isGazePoint2dValidRight)
        {
            _timestamp = timestamp;
            _left = new GazeDataItem(gazePoint2dLeft, isGazePoint2dValidLeft);
            _right = new GazeDataItem(gazePoint2dRight, isGazePoint2dValidRight);
            Vector2 gazePoint2dCombined = new Vector2(GazeFilter(gazePoint2dLeft.X, gazePoint2dRight.X), GazeFilter(gazePoint2dLeft.Y, gazePoint2dRight.Y));
            _combined = new GazeDataItem(gazePoint2dCombined, isGazePoint2dValidLeft & isGazePoint2dValidRight);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GazeDataArgs"/> class.
        /// </summary>
        /// <param name="timestamp">The timestamp.</param>
        /// <param name="gazePoint2dLeft">The 2d coordinates of the left gaze point.</param>
        /// <param name="isGazePoint2dValidLeft">The validity of the left 2d gaze point.</param>
        /// <param name="gazePoint2dRight">The 2d coordinates of the right gaze point.</param>
        /// <param name="isGazePoint2dValidRight">The validity of the right 2d gaze point.</param>
        /// <param name="gazePoint3dLeft">The 3d coordinates of the left gaze point.</param>
        /// <param name="isGazePoint3dValidLeft">The validity of the left 3d gaze point.</param>
        /// <param name="gazePoint3dRight">The 3d coordinates of the right gaze point.</param>
        /// <param name="isGazePoint3dValidRight">The validity of the right 3d gaze point.</param>
        /// <param name="gazeOrigin3dLeft">The 3d coordinates of the left gaze origin.</param>
        /// <param name="isGazeOrigin3dValidLeft">The validity of the left 3d gaze origin.</param>
        /// <param name="gazeOrigin3dRight">The 3d coordinates of the right gaze origin.</param>
        /// <param name="isGazeOrigin3dValidRight">The validity of the right 3d gaze origin.</param>
        /// <param name="pupilDiameterLeft">The pupil diameter the left eye.</param>
        /// <param name="isPupilDiameterValidLeft">The validity of the left pupil diameter.</param>
        /// <param name="pupilDiameterRight">The pupil diameter the left eye.</param>
        /// <param name="isPupilDiameterValidRight">The validity of the left pupil diameter.</param>
        public GazeDataArgs(TimeSpan timestamp, Vector2 gazePoint2dLeft, bool isGazePoint2dValidLeft, Vector2 gazePoint2dRight, bool isGazePoint2dValidRight,
            Vector3 gazePoint3dLeft, bool isGazePoint3dValidLeft, Vector3 gazePoint3dRight, bool isGazePoint3dValidRight,
            Vector3 gazeOrigin3dLeft, bool isGazeOrigin3dValidLeft, Vector3 gazeOrigin3dRight, bool isGazeOrigin3dValidRight,
            float pupilDiameterLeft, bool isPupilDiameterValidLeft, float pupilDiameterRight, bool isPupilDiameterValidRight)
        {
            _timestamp = timestamp;
            _left = new GazeDataItem(gazePoint2dLeft, isGazePoint2dValidLeft, gazePoint3dLeft, isGazePoint3dValidLeft, gazeOrigin3dLeft, isGazeOrigin3dValidLeft, pupilDiameterLeft, isPupilDiameterValidLeft);
            _right = new GazeDataItem(gazePoint2dRight, isGazePoint2dValidRight, gazePoint3dRight, isGazePoint3dValidRight, gazeOrigin3dRight, isGazeOrigin3dValidRight, pupilDiameterRight, isPupilDiameterValidRight);
            Vector2 gazePoint2dCombined = new Vector2(GazeFilter(gazePoint2dLeft.X, gazePoint2dRight.X), GazeFilter(gazePoint2dLeft.Y, gazePoint2dRight.Y));
            Vector3 gazePoint3dCombined = new Vector3(GazeFilter(gazePoint3dLeft.X, gazePoint3dRight.X), GazeFilter(gazePoint3dLeft.Y, gazePoint3dRight.Y), GazeFilter(gazePoint3dLeft.Z, gazePoint3dRight.Z));
            Vector3 gazeOrigin3dCombined = new Vector3(GazeFilter(gazeOrigin3dLeft.X, gazeOrigin3dRight.X), GazeFilter(gazeOrigin3dLeft.Y, gazeOrigin3dRight.Y), GazeFilter(gazeOrigin3dLeft.Z, gazeOrigin3dRight.Z));
            _combined = new GazeDataItem(
                gazePoint2dCombined, isGazePoint2dValidLeft & isGazePoint2dValidRight,
                gazePoint3dCombined, isGazePoint3dValidLeft & isGazePoint3dValidRight,
                gazeOrigin3dCombined, isGazeOrigin3dValidLeft & isGazeOrigin3dValidRight,
                GazeFilter(pupilDiameterLeft, pupilDiameterRight), isPupilDiameterValidLeft & isPupilDiameterValidRight
            );
        }

        /// <summary>
        /// Combines the data values form the left and the right eye.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns></returns>
        private float GazeFilter(float left, float right)
        {
            if (float.IsNaN(left)) return right;
            if (float.IsNaN(right)) return left;
            return (left + right) / 2;
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

    public class ScreenTriangle
    {
        private Vector3 _v1;
        public Vector3 V1 { get { return _v1; } }

        private Vector3 _v2;
        public Vector3 V2 { get { return _v2; } }

        private Vector3 _v3;
        public Vector3 V3 { get { return _v3; } }

        private Vector3 _e1;
        public Vector3 E1 { get { return _e1; } }

        private Vector3 _e2;
        public Vector3 E2 { get { return _e2; } }

        public ScreenTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            _v1 = v1;
            _v2 = v2;
            _v3 = v3;
            _e1 = v2 - v1;
            _e2 = v3 - v1;
        }

        public Vector3? GetIntersectionPoint(Vector3 origin, Vector3 direction)
        {
            Vector3 p, t, q, directionScaled;
            float det, invDet, u, v, distTmp;
            float epsilon = 0.0000001f;

            // Möller-Trumbore
            p = Vector3.Cross(direction, _e2);
            det = Vector3.Dot(_e1, p);

            if (Math.Abs(det) < epsilon)
            {
                return null; // line is parallel to the triangle
            }

            invDet = 1 / det;

            t = origin - _v1;
            u = invDet * Vector3.Dot(t, p);
            if (u < 0 || u > 1)
            {
                return null;
            }

            q = Vector3.Cross(t, _e1);
            v = invDet * Vector3.Dot(direction, q);
            if (u < 0 || u + v > 1)
            {
                return null;
            }

            distTmp = invDet * Vector3.Dot(_e2, q);
            if (distTmp > epsilon)
            {
                directionScaled = Vector3.Multiply(direction, distTmp);
                return origin + directionScaled;
            }

            return null;
        }
    }

    public class ScreenArea
    {

        private float _width;
        public float Width { get { return _width; } }

        private float _height;
        public float Height { get { return _height; } }

        private Vector3 _bottomLeft;
        public Vector3 BottomLeft { get { return _bottomLeft; } }

        private Vector3 _bottomRight;
        public Vector3 BottomRight { get { return _bottomRight; } }

        private Vector3 _topLeft;
        public Vector3 TopLeft { get { return _topLeft; } }

        private Vector3 _topRight;
        public Vector3 TopRight { get { return _topRight; } }

        private Vector3 _center;
        public Vector3 Center { get { return _center; } }

        private Vector2 _origin;
        private Vector3 _norm;

        private Matrix4x4 _m;

        public ScreenArea(Vector3 bottomLeft, Vector3 bottomRight, Vector3 topLeft, Vector3 topRight, float width, float height)
        {
            Vector3 bottomCenter = Vector3.Lerp(bottomLeft, bottomRight, 0.5f);
            Vector3 topCenter = Vector3.Lerp(topLeft, topRight, 0.5f);
            _center = Vector3.Lerp(bottomCenter, topCenter, 0.5f);
            _bottomLeft = bottomLeft;
            _topLeft = topLeft;
            _bottomRight = bottomRight;
            _topRight = topRight;
            _width = width;
            _height = height;
            Vector3 e1 = topRight - topLeft;
            Vector3 e2 = bottomLeft - topLeft;
            _norm = Vector3.Cross(e1, e2);
            Vector3 u = topLeft + Vector3.Normalize(e1);
            Vector3 v = topLeft + Vector3.Normalize(e2);
            Vector3 n = topLeft + Vector3.Normalize(_norm);
            Matrix4x4 s = new Matrix4x4(
                topLeft.X, u.X, v.X, n.X,
                topLeft.Y, u.Y, v.Y, n.Y,
                topLeft.Z, u.Z, v.Z, n.Z,
                1, 1, 1, 1
            );
            Matrix4x4 d = new Matrix4x4(
                0, 1, 0, 0,
                0, 0, 1, 0,
                0, 0, 0, 1,
                1, 1, 1, 1
            );
            Matrix4x4 sInv;
            Matrix4x4.Invert(s, out sInv);
            _m = Matrix4x4.Transpose(d * sInv);
            _origin = GetPoint2d(topLeft);
        }

        public Vector3? GetIntersectionPoint(Vector3 gazeOrigin, Vector3 gazeDirection)
        {
            float d = Vector3.Dot(_norm, gazeOrigin - _topLeft);
            float n = Vector3.Dot(-gazeDirection, _norm);

            if (n == 0)
            {
                return null;
            }

            return gazeOrigin + d/n * gazeDirection;
        }

        public Vector2 GetPoint2d(Vector3 point)
        {
            Vector4 v = Vector4.Transform(point, _m);
            return new Vector2(v.X, v.Y);
        }

        public Vector2 GetPoint2dNormalized(Vector3 point3d)
        {
            Vector2 point2dOffset = GetPoint2d(point3d);
            Vector2 point2d = point2dOffset - _origin;
            return new Vector2(point2d.X / _width, point2d.Y / _height);
        }
    }

    public class DriftCompensation
    {
        private bool _isCollecting = false;
        private Vector3 _fixationPoint;
        private int _fixationFrameCount;
        private double _normalizedDispersionThreshold;
        private Quaternion _q;
        public Quaternion Q { get { return _q; } }

        private List<GazeDataArgs> _samples;

        public DriftCompensation(Vector3 fixationPoint, int fixationFrameCount, double dispersionThreashold)
        {
            _q = Quaternion.Identity;
            _samples = new List<GazeDataArgs>();
            _normalizedDispersionThreshold = AngleToDist(dispersionThreashold);
            _fixationPoint = fixationPoint;
            _fixationFrameCount = fixationFrameCount;
        }

        public void Reset()
        {
            _samples.Clear();
            _q = Quaternion.Identity;
        }

        public void Start()
        {
            _isCollecting = true;
        }

        public bool Update(GazeDataArgs args)
        {
            if (args.Combined.GazeData3d == null || !args.Combined.GazeData3d.IsGazePointValid || !args.Combined.GazeData3d.IsGazeOriginValid || !_isCollecting)
            {
                return false;
            }

            _samples.Add(args);
            if (_samples.Count >= _fixationFrameCount)
            {
                if (Dispersion() <= MaxDeviation())
                {
                    _q = Compute();
                    _samples.Clear();
                    _isCollecting = false;
                    return true;
                }
                else
                {
                    _samples.RemoveAt(0);
                }
            }
            return false;
        }

        private Quaternion Compute()
        {
            Vector3 oAvg = Vector3.Zero;
            Vector3 gAvg = Vector3.Zero;

            foreach (GazeDataArgs sample in _samples)
            {
                oAvg += sample.Combined.GazeData3d!.GazeOrigin;
                gAvg += sample.Combined.GazeData3d!.GazePoint;
            }
            gAvg /= _fixationFrameCount;
            oAvg /= _fixationFrameCount;

            Vector3 gDir = Vector3.Normalize(gAvg - oAvg);
            Vector3 cDir = Vector3.Normalize(_fixationPoint - oAvg);

            return CreateQuaternionFromVectors(gDir, cDir);
        }

        private double MaxDeviation()
        {
            double dist = _samples.Average(sample => sample.Combined?.GazeData3d?.GazeDistance ?? 0);
            return dist * _normalizedDispersionThreshold;
        }

        private double Dispersion()
        {
            float xMax = _samples.Max(sample => sample.Combined.GazeData3d?.GazePoint.X ?? 0);
            float yMax = _samples.Max(sample => sample.Combined.GazeData3d?.GazePoint.Y ?? 0);
            float zMax = _samples.Max(sample => sample.Combined.GazeData3d?.GazePoint.Z ?? 0);
            float xMin = _samples.Min(sample => sample.Combined.GazeData3d?.GazePoint.X ?? 0);
            float yMin = _samples.Min(sample => sample.Combined.GazeData3d?.GazePoint.Y ?? 0);
            float zMin = _samples.Min(sample => sample.Combined.GazeData3d?.GazePoint.Z ?? 0);
            float dispersion = xMax - xMin + yMax - yMin + zMax - zMin;
            return dispersion;
        }

        private Quaternion CreateQuaternionFromVectors(Vector3 v1, Vector3 v2)
        {
            float dot = Vector3.Dot(v1, v2);
            if (dot > 0.999999)
            {
                return Quaternion.Identity;
            }
            else
            {
                Vector3 axis = Vector3.Cross(v1, v2);
                return Quaternion.Normalize(new Quaternion(axis, 1 + dot));

            }
        }

        private double AngleToDist(double angle)
        {
            return Math.Sqrt(2 * (1 - Math.Cos(angle * Math.PI / 180)));
        }

    }
}
