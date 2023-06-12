using GazeUtilityLibrary;
using System;
using System.Windows;

namespace DriftCompensation
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            TrackerLogger logger = new TrackerLogger();
            try
            {
                NamedPipeClient.SendRequest("DRIFT_COMPENSATION", logger);
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
            }
            Current.Shutdown();
        }
    }
}