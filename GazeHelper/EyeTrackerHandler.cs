using System;
using System.ComponentModel;
using System.Timers;
using System.Windows;
using System.Windows.Threading;
using Tobii.Interaction.Framework;

namespace GazeHelper
{
    /// <summary>
    /// The common interface for the Tobii eyetracker Engines Core and Pro
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    /// <seealso cref="System.IDisposable" />
    public abstract class EyeTrackerHandler : INotifyPropertyChanged, IDisposable
    {
        private EyeTrackingDeviceStatus state;

        protected Timer dialogBoxTimer;
        protected TrackerLogger logger;
        protected TrackerMessageBox trackerMessageBox;

        public event EventHandler TrackerEnabled;
        public event EventHandler TrackerDisabled;
        public event PropertyChangedEventHandler PropertyChanged;
        public delegate void GazeDataHandler(Object sender, GazeDataArgs e);
        public event GazeDataHandler GazeDataReceived;

        public EyeTrackingDeviceStatus State
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
        /// Initializes a new instance of the <see cref="EyeTrackerHandler"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="ready_timer">The ready timer.</param>
        public EyeTrackerHandler(TrackerLogger logger, int ready_timer)
        {
            this.logger = logger;
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
        protected bool IsReady() { return (State == EyeTrackingDeviceStatus.Tracking); }

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
        /// <param name="tracker">The eyetracker.</param>
        protected void OnTrackerDisabledTimeout(Object source, System.Timers.ElapsedEventArgs e)
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
        private void OnUpdateState(EyeTrackingDeviceStatus state)
        {
            logger.Debug($"Eye tracker changed state: {state}");
            if (IsReady() && (state != EyeTrackingDeviceStatus.Tracking))
            {
                dialogBoxTimer.Start();
                logger.Info($"Eye tracker stopped tracking: New state is \"{state}\"");
            }
            this.state = state;
            if (IsReady())
            {
                Application.Current.Dispatcher.Invoke(callback: () => { trackerMessageBox?.Close(); });
                dialogBoxTimer.Stop();
                logger.Info("Eye tracker is ready");
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

        public GazeDataArgs(TimeSpan timestamp, double xCoord, double yCoord)
        {
            this.timestamp = timestamp;
            this.xCoord = xCoord;
            this.yCoord = yCoord;
        }
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
