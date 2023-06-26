using GazeUtilityLibrary;
using System;
using System.Windows;

namespace GazeControl
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void OnApplicationStartup(object sender, StartupEventArgs e)
        {
            TrackerLogger logger = new TrackerLogger(null);

            string? command = null;
            for (int i = 0; i < e.Args.Length; i++)
            {
                if (e.Args[i].StartsWith("/"))
                {
                    switch (e.Args[i].Substring(1))
                    {
                        case "command":
                            i++;
                            command = e.Args[i];
                            break;
                    }
                }
            }

            switch (command)
            {
                case "GAZE_RECORDING_DISABLE":
                case "GAZE_RECORDING_ENABLE":
                case "MOUSE_TRACKING_DISABLE":
                case "MOUSE_TRACKING_ENABLE":
                case "RESET_DRIFT_COMPENSATION":
                case "TERMINATE":
                    try
                    {
                        NamedPipeClient.SendSignal(command, logger);
                    }
                    catch (Exception error)
                    {
                        logger.Error(error.Message);
                    }
                    break;
                case "DRIFT_COMPENSATION":
                case "CUSTOM_CALIBRATE":
                    try
                    {
                        NamedPipeClient.SendRequest(command, logger);
                    }
                    catch (Exception error)
                    {
                        logger.Error(error.Message);
                    }
                    break;
                default:
                    logger.Error($"unknown command: {command}");
                    break;
            }

            Current.Shutdown();
        }
    }
}
