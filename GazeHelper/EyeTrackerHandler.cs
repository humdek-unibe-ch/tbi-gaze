using System;
using System.ComponentModel;
using System.Timers;
using System.Windows;
using System.Windows.Threading;
using Tobii.Interaction.Framework;

namespace GazeHelper
{
    public abstract class EyeTrackerHandler : INotifyPropertyChanged, IDisposable
    {
        private EyeTrackingDeviceStatus state;

        protected Timer dialogBoxTimer;
        protected TrackerLogger logger;
        protected TrackerMessageBox trackerMessageBox;

        public event EventHandler TrackerEnabled;
        public event EventHandler TrackerDisabled;
        public event PropertyChangedEventHandler PropertyChanged;

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
}
