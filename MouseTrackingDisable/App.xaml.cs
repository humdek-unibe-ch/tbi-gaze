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
            NamedPipeClient.SendSignal("MOUSE_TRACKING_DISABLE");
            Current.Shutdown();
        }
    }
}
