using GazeUtilityLibrary;
using System.Windows;

namespace MouseTrackingEnable
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            TrackerLogger logger = new TrackerLogger();
            NamedPipeClient.SendSignal("MOUSE_TRACKING_ENABLE", logger);
            Current.Shutdown();
        }
    }
}
