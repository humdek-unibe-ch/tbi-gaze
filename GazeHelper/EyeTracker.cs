using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;
using System.Threading;
using Tobii.Interaction;
using Tobii.Interaction.Framework;

namespace GazeHelper
{
    /// <summary>
    /// Class to interact with the eyetracker device
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public class EyeTracker : IDisposable, INotifyPropertyChanged
    {
        private TrackerLogger logger;
        private Host host;

        public event EventHandler TrackerEnabled;
        public event EventHandler TrackerDisabled;
        public event PropertyChangedEventHandler PropertyChanged;

        private int readyTimer;
        private Timer dialogBoxTimer;
        private TrackerMessageBox trackerMessageBox;

        private EyeTrackingDeviceStatus state;
        public EyeTrackingDeviceStatus State
        {
            get { return state; }
            set
            {
                if (state == value) return;
                state = value;
                OnPropertyChanged("State");
            }
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="EyeTracker"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public EyeTracker(TrackerLogger logger, int ready_timer)
        {
            this.logger = logger;
            this.readyTimer = ready_timer;
            host = new Host();
            var deviceStateObserver = host.States.CreateEyeTrackingDeviceStatusObserver();
            deviceStateObserver.WhenChanged(deviceState => OnUpdateDeviceStatus(deviceState));
            dialogBoxTimer = new Timer(OnTrackerDisabledTimeout, this, ready_timer, Timeout.Infinite);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
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
                host.DisableConnection();
                host.Dispose();
                dialogBoxTimer.Dispose();
            }
        }

        /// <summary>
        /// Creates the gaze point data stream with the Tobii API.
        /// </summary>
        /// <param name="gaze_filter_mode">The gaze filter mode.</param>
        /// <returns>the gaze point data stream</returns>
        public GazePointDataStream CreateGazePointDataStream(int gaze_filter_mode)
        {
            GazePointDataMode filter;
            switch (gaze_filter_mode)
            {
                case 0: filter = GazePointDataMode.Unfiltered; break;
                case 1: filter = GazePointDataMode.LightlyFiltered; break;
                default:
                    filter = GazePointDataMode.Unfiltered;
                    logger.Error($"Unkonwn filter setting: \"{gaze_filter_mode}\"");
                    logger.Warning("Using unfiltered mode");
                    break;
            }
            try
            {
                return host.Streams.CreateGazePointDataStream(filter);
            }
            catch (Exception ex)
            {
                logger.Error("Unable to create gaze point data stream");
                logger.DumpFatal(ex);
                throw;
            }
        }

        /// <summary>
        /// Determines whether this eye tracker is ready.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is ready; otherwise, <c>false</c>.
        /// </returns>
        private bool IsReady() { return (State == EyeTrackingDeviceStatus.Tracking); }

        /// <summary>
        /// Raises the <see cref="E:TrackerEnabled" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected virtual void OnTrackerEnabled(EventArgs e) { TrackerEnabled?.Invoke(this, e); }

        /// <summary>
        /// Raises the <see cref="E:TrackerDisabled" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected virtual void OnTrackerDisabled(EventArgs e) { TrackerDisabled?.Invoke(this, e); }

        /// <summary>
        /// Called when when the state property of EyeTracker is changing.
        /// </summary>
        /// <param name="property_name">Name of the property in WPF.</param>
        protected virtual void OnPropertyChanged(string property_name) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property_name)); }

        /// <summary>
        /// Called after a specified amount of time of the eyetracker not being ready.
        /// </summary>
        /// <param name="tracker">The eyetracker.</param>
        protected virtual void OnTrackerDisabledTimeout(Object tracker)
        {
            OnTrackerDisabled(new EventArgs());
            // the timer callback function is called in a new thread
            // hence, I have to use the dipatcher to execute on the main thread
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action( () =>
            {
                trackerMessageBox = new TrackerMessageBox();
                trackerMessageBox.lbStatus.DataContext = tracker;
                bool? dialogResult = trackerMessageBox.ShowDialog();
                switch (dialogResult)
                {
                    case true:
                        // Abort button was pressed
                        Application.Current.Shutdown();
                        break;
                    case false:
                        if(!IsReady()) dialogBoxTimer.Change(readyTimer, Timeout.Infinite);
                        break;
                }
            }));
        }

        /// <summary>
        /// Called when the state of eye tracker is updated.
        /// </summary>
        /// <param name="deviceState">State of the eyetracker device.</param>
        private void OnUpdateDeviceStatus(EngineStateValue<EyeTrackingDeviceStatus> deviceState)
        {
            logger.Debug($"Eye tracker changed state: {deviceState.Value}");
            if (IsReady() && (deviceState.Value != EyeTrackingDeviceStatus.Tracking))
            {
                dialogBoxTimer.Change(readyTimer, Timeout.Infinite);
                logger.Info($"Eye tracker stopped tracking: New state is \"{deviceState.Value}\"");
            }
            State = deviceState.Value;
            if (IsReady())
            {
                Application.Current.Dispatcher.Invoke(callback: () => { trackerMessageBox?.Close(); });
                dialogBoxTimer.Change(Timeout.Infinite, Timeout.Infinite);
                logger.Info("Eye tracker is ready");
                OnTrackerEnabled(new EventArgs());
            }
        }
    }
}