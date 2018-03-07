using System;
using System.IO;
using System.Windows;
using GazeHelper;

namespace GazeToMouse
{
    /// <summary>
    /// Converts gaze data to mouse coordinates
    /// </summary>
    /// <seealso cref="System.Windows.Application" />
    class GazeToMouse : Application
    {
        private static bool tracking = false;
        private static bool usePro = false;
        private static EyeTrackerHandler tracker;
        private static StreamWriter sw;
        private static TimeSpan delta;
        private static TrackerLogger logger;
        private static MouseHider hider;
        private static JsonConfigParser.ConfigItem config;

        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            Application app = new Application();
            app.Exit += new ExitEventHandler(OnApplicationExit);
            // we need a window to gracefully terminate the program with WM_CLOSE
            //trackerMessageBox.Show();
            Window window = new Window
            {
                Visibility = Visibility.Hidden,
                WindowStyle = WindowStyle.None
            };

            logger = new TrackerLogger();
            logger.Info($"Starting \"{AppDomain.CurrentDomain.BaseDirectory}GazeToMouse.exe\"");

            JsonConfigParser parser = new JsonConfigParser(logger);
            config = parser.ParseJsonConfig();

            if (config.WriteDataLog)
            {
                string gazeFilePostfix = $"_{Environment.MachineName}_gaze.txt";
                string gazeFileName = $"{DateTime.Now:yyyyMMddTHHmmss}{gazeFilePostfix}";

                // create gaze data file
                if (config.OutputPath == "") config.OutputPath = Directory.GetCurrentDirectory();
                sw = CreateGazeOutputFile(config.OutputPath, gazeFileName);
                if (sw == null)
                {
                    // something went wrong, write to the current directory
                    config.OutputPath = Directory.GetCurrentDirectory();
                    string outputFilePath = $"{config.OutputPath}\\{gazeFileName}";
                    logger.Warning($"Writing gaze data to the current directory: \"{outputFilePath}\"");
                    sw = new StreamWriter(gazeFileName);
                }

                // check output data format
                if (config.WriteDataLog && !CheckOutputFormat(config.OutputFormat))
                {
                    // something is wrong with the configured format, use the default format
                    JsonConfigParser.ConfigItem default_config = parser.GetDefaultConfig();
                    config.OutputFormat = default_config.OutputFormat;
                    WriteOutputHeader(config.OutputFormat);
                    logger.Warning($"Using default output format");
                }

                // delete old files
                DeleteOldGazeLogFiles(config.OutputPath, config.OutputCount, $"*{gazeFilePostfix}");
            }
            
            // hide the mouse cursor
            hider = new MouseHider(logger);
            if (config.ControlMouse && config.HideMouse) hider.HideCursor();


            // initialize host. Make sure that the Tobii service is running
            if(usePro)
            {
                tracker = new EyeTrackerPro(logger, config.ReadyTimer);
                ((EyeTrackerPro)tracker).GazeDataReceived += OnGazeData;
            }
            else
            {
                tracker = new EyeTrackerCore(logger, config.ReadyTimer, config.GazeFilter);
                ((EyeTrackerCore)tracker).GazeDataReceived += OnGazeData;
            }

            tracker.TrackerEnabled += OnTrackerEnabled;
            tracker.TrackerDisabled += OnTrackerDisabled;
            app.Run(window);
        }

        /// <summary>
        /// Checks whether a string formatting is applicable.
        /// </summary>
        /// <param name="format">format string to be checked.</param>
        /// <returns><c>true</c> if the format is ok; otherwise, <c>false</c></returns>
        static bool CheckOutputFormat(string format)
        {
            try
            {
                // try format without pupil information
                WriteOutputHeader(config.OutputFormat);
                return true;
            }
            catch (FormatException)
            {
                try
                {
                    // try format with pupil information
                    WriteOutputHeader(config.OutputFormat, true);
                    usePro = true; // need Tobii SDK Pro
                    return true;
                }
                catch
                {
                    logger.Error($"Output format string was not in a correct format");
                    return false;
                }
            }
        }

        /// <summary>
        /// Createa and Opens a data stream to a file where gaze data will be stored.
        /// </summary>
        /// <param name="file_path">The file path.</param>
        /// <param name="file_name">Name of the gaze data file.</param>
        /// <returns>the stream handler</returns>
        static StreamWriter CreateGazeOutputFile(string file_path, string file_name)
        {
            try
            {
                string outputFilePath;
                sw = new StreamWriter($"{file_path}\\{file_name}");
                FileInfo fi = new FileInfo($"{file_path}\\{file_name}");
                outputFilePath = fi.FullName;
                logger.Info($"Writing gaze data to \"{outputFilePath}\"");
                return sw;
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
                return null;
            }
        }

        /// <summary>
        /// Deletes the old gaze log files.
        /// </summary>
        /// <param name="output_path">The Path to the folder with the old output files.</param>
        /// <param name="output_count">The number of files that are allowd in the path.</param>
        /// <param name="filter">The output data file filter.</param>
        static void DeleteOldGazeLogFiles(string output_path, int output_count, string filter)
        {
            string[] gazeLogFiles = Directory.GetFiles(output_path, filter);
            if ((output_count > 0) && (gazeLogFiles.GetLength(0) > output_count))
            {
                Array.Sort(gazeLogFiles);
                Array.Reverse(gazeLogFiles);
                for (int i = output_count; i < gazeLogFiles.GetLength(0); i++)
                {
                    File.Delete(gazeLogFiles[i]);
                    logger.Info($"Removing old gaze data file \"{gazeLogFiles[i]}\"");
                }
            }
        }

        /// <summary>
        /// Sets the mouse pointer to the location of the gaze point and logs the coordiantes.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="x">The x-coordinate of the gaze point.</param>
        /// <param name="y">The y-coordinate of the gaye point.</param>
        /// <param name="ts">The timestamp of the the capture instant of the gaye point.
        /// Note that the timestamp reference represents an arbitrary point in time.</param>
        static void OnGazeData(double x, double y, double ts)
        {
            // write the coordinates to the log file
            if (config.WriteDataLog)
            {
                // create a time reference that corresponds to the local machine
                TimeSpan ts_rec = ComputeEyeTrackerTimestamp(ts);
                sw.WriteLine(config.OutputFormat, ts_rec, x, y);
                tracking = true;
            }
            // set the cursor position to the gaze position
            if (config.ControlMouse)
            {
                UpdateMousePosition(Convert.ToInt32(x), Convert.ToInt32(y));
            }
        }

        static void OnGazeData(double? x, double? y, double? dia, long ts)
        {
            if ((x == null) && (y == null)) return;
            // write the coordinates to the log file
            if (config.WriteDataLog)
            {
                TimeSpan ts_rec = ComputeEyeTrackerTimestamp(ts/1000);
                sw.WriteLine(config.OutputFormat, ts_rec, x, y, dia);
                tracking = true;
            }
            // set the cursor position to the gaze position
            if (config.ControlMouse)
            {
                UpdateMousePosition(Convert.ToInt32(x), Convert.ToInt32(y));
            }
        }

        /// <summary>
        /// Computes the eye tracker timestamp.
        /// </summary>
        /// <param name="ts">The ts.</param>
        /// <returns></returns>
        static TimeSpan ComputeEyeTrackerTimestamp( double ts )
        {
            TimeSpan res = TimeSpan.FromMilliseconds(ts);
            if (!tracking) delta = res - DateTime.Now.TimeOfDay; ;
            res -= delta;
            return res;
        }

        /// <summary>
        /// Updates the mouse position.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        static void UpdateMousePosition(int x, int y)
        {
            System.Windows.Forms.Cursor.Position = new System.Drawing.Point(x, y);
        }

        /// <summary>
        /// Takes a valid format string as parameter and returns the string with sample gaze values.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="pupil">if set to <c>true</c> use format with pupil diameter.</param>
        /// <returns>
        /// a formatted string of sample gaze values.
        /// </returns>
        static void WriteOutputHeader(string format, bool pupil=false)
        {
            string formatted;
            string ts_title = "Timestamp";
            TimeSpan ts = DateTime.Now.TimeOfDay;
            string x_title = "x-coord";
            double x = 1000.000000;
            string y_title = "y-coord";
            double y = 1000.000000;
            string dia_title = "diameter";
            double dia = 1.000000;
            if (pupil)
            {
                formatted = String.Format(format, ts, x, y, dia);
                sw.WriteLine(config.OutputFormat, ts_title, x_title, y_title, dia_title);
            }
            else
            {
                formatted = String.Format(format, ts, x, y);
                sw.WriteLine(config.OutputFormat, ts_title, x_title, y_title);
            }
            logger.Info($"Output format is of the from: \"{formatted}\"");
            
        }

        /// <summary>
        /// Called when the eye tracker is ready.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        static void OnTrackerEnabled(object sender, EventArgs e)
        {
            if (config.ControlMouse && config.HideMouse) hider.HideCursor();
            if (tracking) return;
        }

        /// <summary>
        /// Called when the eye tracker changes from ready to any other state.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        static void OnTrackerDisabled(object sender, EventArgs e)
        {
            if (config.ControlMouse && config.HideMouse) hider.ShowCursor(config.StandardMouseIconPath);
        }

        /// <summary>
        /// Called when [application exit].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        static void OnApplicationExit(object sender, EventArgs e)
        {
            if (config.ControlMouse && config.HideMouse) hider.ShowCursor(config.StandardMouseIconPath);

            if (config.WriteDataLog)
            {
                sw.Close();
                sw.Dispose();
            }
            tracker.Dispose();
            logger.Info($"\"{AppDomain.CurrentDomain.BaseDirectory}GazeToMouse.exe\" terminated gracefully{Environment.NewLine}");
        }
    }
}
