using System.Diagnostics;
using System.Windows;
using GazeUtilityLibrary;

namespace GazeToMouseClose
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private TrackerLogger _logger;

        public App()
        {
            _logger = new TrackerLogger(); _logger.Info("Sending WM_CLOSE signal to process \"GazeToMouse.exe\"");
            //Process.Start("taskkill", "/T /IM GazeToMouse");
            Process[] processes = Process.GetProcessesByName("GazeToMouse");
            foreach (Process process in processes)
            {
                //process.CloseMainWindow();
                process.Kill();
                //process.Close();
            }
            Current.Shutdown();
        }
    }
}
