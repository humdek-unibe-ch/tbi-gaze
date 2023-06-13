using GazeUtilityLibrary;
using System.Windows;

namespace GazeToMouseRecordingDisable
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            TrackerLogger logger = new TrackerLogger();
            NamedPipeClient.SendSignal("GAZE_RECORDING_DISABLE", logger);
            Current.Shutdown();
        }
    }
}
