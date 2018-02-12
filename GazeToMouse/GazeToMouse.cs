/**
 * Converts the stream of gaze points, captured by a Tobii eyetracker, to mouse cursor location
 * 
 * @file    GazeToMouse.cs
 * @author  Simon Maurer, simon.maurer@humdek.unibe.ch
 * @date    January 2018
 */

using System;
using System.IO;
using System.Windows.Forms;
using Tobii.Interaction;
using Tobii.Interaction.Framework;
using GazeHelper;

namespace GazeToMouse
{
    /**
     * @brief Main entry point of the application
     */
    class GazeToMouse
    {
        private static StreamWriter sw;
        private static TimeSpan ts_delta;
        private static bool hasRun = false;
        private static Host host;
        private static Logger logger;
        private static MouseHider hider;
        private static JsonConfigParser.ConfigItem config;
        /**
         * @brief Helper class to be added to the application's message pump to filter out a message
         */
        private class ExitMessageFilter : IMessageFilter
        {
            /**
             * @brief filters out a message before it is dispatched
             * 
             * The current implementation only cares about the signal that is sent to the application by taskkill (WM_CLOSE).
             * Once WM_CLOSE is received, the event "ApplicationExit" is invoked.
             * 
             * @param m the pre-filtered message
             * @return  true if the received message corresponds to WM_CLOSE
             *          false if any other message is pre-filtered
             */
            public bool PreFilterMessage(ref Message m)
            {
                if (m.Msg == /*WM_CLOSE*/ 0x10)
                {
                    Application.Exit();
                    return true;
                }
                return false;
            }
        }

        /**
         * @brief The main programm entry
         */
        static void Main(string[] args)
        {
            // load configuration
            JsonConfigParser parser = new JsonConfigParser();
            config = parser.ParseJsonConfig();
            logger = new Logger();
            logger.Info($"Starting \"{AppDomain.CurrentDomain.BaseDirectory}GazeToMouse.exe\"");

            DateTime now = DateTime.Now;
            if (config.WriteDataLog)
            {
                string gazeFileName = $"{Environment.MachineName}_data.txt";
                sw = CreateGazeOutputFile(now, gazeFileName);

                // check output data format
                if(!CheckOutputFormat(config.OutputFormat))
                {
                    JsonConfigParser.ConfigItem default_config = parser.GetDefaultConfig();
                    config.OutputFormat = default_config.OutputFormat;
                    logger.Warning($"Using default output format of the form: \"{GetFromatSample(config.OutputFormat)}\"");
                }

                // delete old files
                DeleteOldGazeLogFiles(config.OutputPath, config.OutputCount, $"*_{gazeFileName}");
            }

            Application.ApplicationExit += new EventHandler(OnApplicationExit);

            // hide the mouse cursor
            hider = new MouseHider();
            if (config.HideMouse)
            {
                logger.Info("Hiding the main mouse cursor");
                hider.HideCursor();
            }

            // initialize host. Make sure that the Tobii service is running
            host = new Host();

            // check filter settings
            GazePointDataMode filter = GetFilterSettings();

            // create stream
            var gazePointDataStream = host.Streams.CreateGazePointDataStream(filter);
            // whenever a new gaze point is available, run gaze2mouse
            gazePointDataStream.GazePoint((x, y, ts) => Gaze2mouse(x, y, ts, now.TimeOfDay));

            // add message filter to the application's message pump
            Application.AddMessageFilter(new ExitMessageFilter());
            Application.Run();
        }

        /**
         * @brief checks whether a string formatting is applicable
         * 
         * @param format    format string to be checked
         * @return true if format is ok, false if not
         */
        static bool CheckOutputFormat(string format)
        {
            try
            {
                logger.Info($"Output format is of the from: \"{GetFromatSample(format)}\"");
                return true;
            }
            catch (FormatException)
            {
                logger.Error($"Output format string was not in a correct format");
                return false;
            }
        }

        /**
         * @brief takes a valid format string as parameter and returns the string with sample gaze values
         * 
         * @return a formatted string of sample gaze values
         */
        static string GetFromatSample( string format )
        {
            TimeSpan ts = DateTime.Now.TimeOfDay;
            double x = 1000.000000;
            double y = 1000.000000;
            return String.Format(format, ts, x, y);
        }

        static GazePointDataMode GetFilterSettings()
        {
            GazePointDataMode filter;
            switch (config.GazeFilter)
            {
                case 0: filter = GazePointDataMode.Unfiltered; break;
                case 1: filter = GazePointDataMode.LightlyFiltered; break;
                default:
                    filter = GazePointDataMode.Unfiltered;
                    logger.Error($"Unkonwn filter setting: \"{config.GazeFilter}\"");
                    logger.Warning("Using unfiltered mode");
                    break;
            }
            return filter;
        }

        /**
         * @brief create and open a data stream to a file where gaze data will be stored
         * 
         * @param now           the current timestamp
         * @param output_path   path to the directory where the gaze data file will be stored
         * @param output_name   name postfix of the gaze data file
         */
        static StreamWriter CreateGazeOutputFile(DateTime now, string output_name)
        {
            // create output file
            string outputFile = $"{now:yyyyMMddTHHmmss}_{output_name}";
            string outputFilePath;
            if (config.OutputPath == "") config.OutputPath = Directory.GetCurrentDirectory();
            try
            {
                sw = new StreamWriter($"{config.OutputPath}\\{outputFile}");
                FileInfo fi = new FileInfo($"{config.OutputPath}\\{outputFile}");
                outputFilePath = fi.FullName;
                logger.Info($"Writing gaze data to \"{outputFilePath}\"");
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
                config.OutputPath = Directory.GetCurrentDirectory();
                outputFilePath = $"{config.OutputPath}\\{outputFile}";
                logger.Warning($"Writing gaze data to the current directory: \"{outputFilePath}\"");
                sw = new StreamWriter($"{outputFile}");
            }
            return sw;
        }

        /**
         * @brief remove old gaze data log files
         * 
         * @param output_path   path to the folder with the old output files
         * @param output_count  number of files that are allowd in the path
         * @param filter        output data file filter
         */
        static void DeleteOldGazeLogFiles(string output_path, int output_count, string filter)
        {
            string[] gazeLogFiles = Directory.GetFiles(output_path, filter);
            if ((output_count > 0) && (gazeLogFiles.GetLength(0) > output_count))
            {
                Array.Sort(gazeLogFiles);
                Array.Reverse(gazeLogFiles);
                for (int i = output_count; i< gazeLogFiles.GetLength(0); i++)
                {
                    File.Delete(gazeLogFiles[i]);
                    logger.Info($"Removing old gaze data file \"{gazeLogFiles[i]}\"");
                }
            }
        }

        /**
         * @brief set the mouse pointer to the location of the gaze point and log the coordiantes
         * 
         * @param x     the x-coordinate of the gaze point
         * @param y     the y-coordinate of the gaye point
         * @param ts    the timestamp of the the capture instant of the gaye point
         *              Note that the timestamp reference represents an arbitrary point in time
         */
        static void Gaze2mouse(double x, double y, double ts, TimeSpan now)
        {
            // write the coordinates to the log file
            if (config.WriteDataLog)
            {
                // create a time reference that corresponds to the local machine
                TimeSpan ts_rec = TimeSpan.FromMilliseconds(ts);
                if (!hasRun) ts_delta = ts_rec - now;
                ts_rec -= ts_delta;
                sw.WriteLine(config.OutputFormat, ts_rec, x, y);
                hasRun = true;
            }

            // set the cursor position to the gaze position
            if (config.ControlMouse) Cursor.Position = new System.Drawing.Point(Convert.ToInt32(x), Convert.ToInt32(y));
        }

        /**
         * @brief cleanup. To be run on application exit.
         * 
         * @param sender    sender of the signal
         * @param e         signal event arguments
         */
        static void OnApplicationExit(object sender, EventArgs e)
        {
            if (config.HideMouse)
            {
                logger.Info("Restoring the main mouse cursor");
                hider.ShowCursor(config.StandardMouseIconPath);
            }

            if (config.WriteDataLog)
            {
                sw.Close();
                sw.Dispose();
            }
            host.DisableConnection();
            logger.Info($"\"{AppDomain.CurrentDomain.BaseDirectory}GazeToMouse.exe\" terminated gracefully{Environment.NewLine}");
        }
    }
}
