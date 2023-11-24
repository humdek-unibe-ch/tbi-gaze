/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
using GazeControlLibrary;
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
        private TrackerLogger logger = new TrackerLogger(null, EOutputType.control);

        private void Logger(LogLevel level, string msg)
        {
            switch (level)
            {
                case LogLevel.error:
                    logger.Error(msg);
                    break;
                case LogLevel.info:
                    logger.Info(msg);
                    break;
                case LogLevel.warning:
                    logger.Warning(msg);
                    break;
                case LogLevel.debug:
                    logger.Debug(msg);
                    break;
            }
        }

        private void OnApplicationStartup(object sender, StartupEventArgs e)
        {
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

            NamedPipeClient.HandleCommands(command, reset, trialId, label, Logger);

            Current.Shutdown();
        }
    }
}
