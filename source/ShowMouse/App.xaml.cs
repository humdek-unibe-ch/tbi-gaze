using System.Windows;
using GazeUtilityLibrary;

namespace ShowMouse
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private TrackerLogger _logger;
        private GazeConfiguration _config;

        public App()
        {
            InitializeComponent();

            _logger = new TrackerLogger(null);
            _config = new GazeConfiguration(_logger);
        }

        private void OnApplicationStartup(object sender, StartupEventArgs e)
        {
            MouseHider hider = new MouseHider(_logger);
            hider.ShowCursor(_config.Config.MouseStandardIconPath);
            Shutdown();
        }
    }
}
