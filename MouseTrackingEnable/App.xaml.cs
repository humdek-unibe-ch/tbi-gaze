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
            NamedPipeClient.SendSignal("MOUSE_TRACKING_ENABLE");
            Current.Shutdown();
        }
    }
}
