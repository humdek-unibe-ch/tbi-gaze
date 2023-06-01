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
            NamedPipeClient.SendSignal("GAZE_RECORDING_DISABLE");
            Current.Shutdown();
        }
    }
}
