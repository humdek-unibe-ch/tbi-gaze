using System.Threading.Tasks;
using System.Threading;
using Tobii.Interaction;
using Tobii.Interaction.Framework;
using GazeHelper;

namespace GazeHelper
{
    public class EyeTracker
    {
        private const int MAX_WAIT_TIME = 10000;    // 10 seconds
        private const int PERIOD_WAIT_TIME = 500;   // 0.5 seconds
        private EyeTrackingDeviceStatus state;
        private Logger logger;

        public EyeTracker(Logger logger)
        {
            this.logger = logger;
            Host host = new Host();
            var deviceStateObserver = host.States.CreateEyeTrackingDeviceStatusObserver();
            deviceStateObserver.WhenChanged(deviceState => UpdateDeviceStatus(deviceState));
        }

        public delegate void IsReady();
        public delegate void IsNotReady(string state);

        public void WhenReady( IsReady sucess, IsNotReady fail)
        {
            bool ready = false;
            int counter = 0;
            while (counter < MAX_WAIT_TIME)
            {
                counter += PERIOD_WAIT_TIME;
                logger.Debug($"Checking state: {state}");
                if (state == EyeTrackingDeviceStatus.Tracking)
                {
                    ready = true;
                    break;
                }
                Thread.Sleep(PERIOD_WAIT_TIME);
            }
            if (ready) sucess();
            else fail(state.ToString());
        }

        private void UpdateDeviceStatus( EngineStateValue<EyeTrackingDeviceStatus> deviceState )
        {
            state = deviceState.Value;
            logger.Debug($"Eye Tracker changed state: {state}");
        }
    }
}
