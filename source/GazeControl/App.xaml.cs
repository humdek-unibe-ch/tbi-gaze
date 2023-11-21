/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
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
            TrackerLogger logger = new TrackerLogger(null, EOutputType.control);

            string? command = null;
            int? trialId = null;
            string? label = null;
            bool reset = false;
            string pipeName = "tobii_gaze";
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

            if (!NamedPipeClient.AwaitServer(pipeName, logger))
            {
                logger.Warning($"No pipe server '{pipeName}' available");
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
                        NamedPipeClient.SendSignal(pipeName, command, reset, trialId, label, logger);
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
                        NamedPipeClient.SendRequest(pipeName, command, reset, trialId, label, logger);
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
