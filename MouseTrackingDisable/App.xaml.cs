using GazeUtilityLibrary;
using System.Windows;

namespace MouseTrackingDisable
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            TrackerLogger logger = new TrackerLogger();
            NamedPipeClient.SendSignal("MOUSE_TRACKING_DISABLE", logger);
            Current.Shutdown();
        }
    }
}
