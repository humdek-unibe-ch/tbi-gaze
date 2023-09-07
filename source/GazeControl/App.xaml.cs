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
            int? trialId = null;
            string? label = null;
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
                        case "label":
                            i++;
                            label = e.Args[i];
                            break;
                        case "reset":
                            reset = true;
                            break;
                        case "trialId":
                            i++;
                            try
                            {
                                trialId = int.Parse(e.Args[i]);
                            }
                            catch
                            {
                                trialId = 0;
                            }
                            break;

                    }
                }
            }

            switch (command)
            {
                case null:
                case "GAZE_RECORDING_DISABLE":
                case "GAZE_RECORDING_ENABLE":
                case "MOUSE_TRACKING_DISABLE":
                case "MOUSE_TRACKING_ENABLE":
                case "RESET_DRIFT_COMPENSATION":
                case "TERMINATE":
                    try
                    {
                        NamedPipeClient.SendSignal(command, reset, trialId, label, logger);
                    }
                    catch (Exception error)
                    {
                        logger.Error(error.Message);
                    }
                    break;
                case "DRIFT_COMPENSATION":
                case "CUSTOM_CALIBRATE":
                case "VALIDATE":
                    try
                    {
                        NamedPipeClient.SendRequest(command, reset, trialId, label, logger);
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
