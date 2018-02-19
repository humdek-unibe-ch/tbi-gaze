using System;
using Tobii.Interaction;
using Tobii.Interaction.Framework;

namespace GazeHelper
{
    public class EyeTracker
    {
        private EyeTrackingDeviceStatus state;
        private Logger logger;
        private Host host;
        public event EventHandler RaiseTrackerReady;

        public EyeTracker(Logger logger)
        {
            this.logger = logger;
            host = new Host();
            var deviceStateObserver = host.States.CreateEyeTrackingDeviceStatusObserver();
            deviceStateObserver.WhenChanged(deviceState => UpdateDeviceStatus(deviceState));
        }

        public void Dispose()
        {
            host.DisableConnection();
            host.Dispose();
        }

        /**
         * @brief create a gaze point stream with the Tobii API
         * 
         * @param gaze_filter_mode the filter settings to create the stream
         */
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
            catch(Exception ex)
            {
                logger.Error("Unable to create gaze point data stream");
                logger.DumpFatal(ex);
                throw;
            }
        }

        /**
         * @brief when the tracker becomes ready, all event handler registert to RaiseTrackerReady are invoked
         * 
         * @param e event
         */
        protected virtual void OnTrackerReady( EventArgs e )
        {
            // EventHandler handler = RaiseTrackerReady;
            // if (handler != null) handler(this, e);
            // simplified version of the above
            RaiseTrackerReady?.Invoke(this, e);
        }

        /**
         * @brief is executed once the state of eye tracker is updated
         * 
         * @param deviceState   the state of the eye tracker
         */
        private void UpdateDeviceStatus( EngineStateValue<EyeTrackingDeviceStatus> deviceState )
        {
            logger.Debug($"Eye tracker changed state: {deviceState.Value}");
            if ( (state == EyeTrackingDeviceStatus.Tracking)
                && (deviceState.Value != EyeTrackingDeviceStatus.Tracking) )
            {
                logger.Info($"Eye tracker stopped tracking: New state is \"{deviceState.Value}\"");
            }
            state = deviceState.Value;
            if (state == EyeTrackingDeviceStatus.Tracking)
            {
                logger.Info("Eye tracker is ready");
                OnTrackerReady(new EventArgs());
            }
        }
    }
}
