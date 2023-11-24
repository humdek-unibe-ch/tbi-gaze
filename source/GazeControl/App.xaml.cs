/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
using GazeControlLibrary;
using GazeUtilityLibrary;
using System.Windows;

namespace GazeControl
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private TrackerLogger logger = new TrackerLogger(null, EOutputType.control);

        /// <summary>
        /// Logger function to be passed to the pipe handler.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="msg">The log message.</param>
        private void Logger(LogLevel level, string msg)
        {
            switch (level)
            {
                case LogLevel.Debug:
                    logger.Debug(msg);
                    break;
                case LogLevel.Info:
                    logger.Info(msg);
                    break;
                case LogLevel.Warning:
                    logger.Warning(msg);
                    break;
                case LogLevel.Error:
                    logger.Error(msg);
                    break;
            }
        }

        /// <summary>
        /// Called when [application starts].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="StartupEventArgs"/> instance containing the event data.</param>
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
