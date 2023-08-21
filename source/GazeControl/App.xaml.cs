using GazeUtilityLibrary;
using System;
using System.Threading;
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
            string? value = null;
            bool reset = false;
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
                        case "value":
                            i++;
                            value = e.Args[i];
                            break;
                        case "reset":
                            reset = true;
                            break;

                    }
                }
            }

            switch (command)
            {
                case "RESET_START_TIME":
                    reset = true;
                    goto send_signal;
                case "GAZE_RECORDING_DISABLE":
                case "GAZE_RECORDING_ENABLE":
                case "MOUSE_TRACKING_DISABLE":
                case "MOUSE_TRACKING_ENABLE":
                case "RESET_DRIFT_COMPENSATION":
                case "SET_TAG":
                case "SET_TRIAL_ID":
                case "TERMINATE":
                send_signal:
                    try
                    {
                        NamedPipeClient.SendSignal(command, reset, value, logger);
                    }
                    catch (Exception error)
                    {
                        logger.Error(error.Message);
                    }
                    break;
                case "DRIFT_COMPENSATION":
                case "CUSTOM_CALIBRATE":
                case "CALIBRATION_VALIDATE":
                    try
                    {
                        NamedPipeClient.SendRequest(command, reset, value, logger);
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
