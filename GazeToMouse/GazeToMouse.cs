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
        private static EyeTrackerHandler tracker;
        private static StreamWriter sw;
        private static TimeSpan delta;
        private static TrackerLogger logger;
        private static MouseHider hider;
        private static ConfigItem config;

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

            if (config.DataLogWriteOutput)
            {
                string gazeFilePostfix = $"_{Environment.MachineName}_gaze.txt";
                string gazeFileName = $"{DateTime.Now:yyyyMMddTHHmmss}{gazeFilePostfix}";

                // create gaze data file
                if (config.DataLogPath == "") config.DataLogPath = Directory.GetCurrentDirectory();
                sw = CreateGazeOutputFile(config.DataLogPath, gazeFileName);
                if (sw == null)
                {
                    // something went wrong, write to the current directory
                    config.DataLogPath = Directory.GetCurrentDirectory();
                    string outputFilePath = $"{config.DataLogPath}\\{gazeFileName}";
                    logger.Warning($"Writing gaze data to the current directory: \"{outputFilePath}\"");
                    sw = new StreamWriter(gazeFileName);
                }

                if (config.DataLogWriteOutput)
                {
                    // check output data format
                    ConfigItem default_config = parser.GetDefaultConfig();
                    if (!CheckDataLogFormat(DateTime.Now.TimeOfDay, config.DataLogFormatTimeStamp))
                    {
                        config.DataLogFormatTimeStamp = default_config.DataLogFormatTimeStamp;
                        logger.Warning($"Using the default output format for timestamps: \"{config.DataLogFormatTimeStamp}\"");
                    }
                    if (!CheckDataLogFormat(1.000000, config.DataLogFormatDiameter))
                    {
                        config.DataLogFormatDiameter = default_config.DataLogFormatDiameter;
                        logger.Warning($"Using the default output format for pupil diameters: \"{config.DataLogFormatDiameter}\"");
                    }
                    if (!CheckDataLogFormat(1.000000, config.DataLogFormatOrigin))
                    {
                        config.DataLogFormatOrigin = default_config.DataLogFormatOrigin;
                        logger.Warning($"Using the default output format for gaze origin values: \"{config.DataLogFormatOrigin}\"");
                    }
                    if (!CheckDataLogColumnOrder(config.DataLogColumnOrder))
                    {
                        config.DataLogColumnOrder = default_config.DataLogColumnOrder;
                        logger.Warning($"Using the default column order: \"{config.DataLogFormatOrigin}\"");
                    }
                    if (!CheckDataLogColumnTitles(config.DataLogColumnOrder, config.DataLogColumnTitle))
                    {
                        logger.Warning($"Column titles are omitted");
                    }
                }

                // delete old files
                DeleteOldGazeLogFiles(config.DataLogPath, config.DataLogCount, $"*{gazeFilePostfix}");
            }
            
            // hide the mouse cursor
            hider = new MouseHider(logger);
            if (config.MouseControl && config.MouseHide) hider.HideCursor();


            // initialize host. Make sure that the Tobii service is running
            if(config.TobiiSDK == 1)
            {
                tracker = new EyeTrackerPro(logger, config.ReadyTimer, config.LicensePath);
            }
            else if(config.TobiiSDK == 0)
            {
                tracker = new EyeTrackerCore(logger, config.ReadyTimer, config.GazeFilterCore);
            }
            tracker.GazeDataReceived += OnGazeDataReceived;
            tracker.TrackerEnabled += OnTrackerEnabled;
            tracker.TrackerDisabled += OnTrackerDisabled;
            app.Run(window);
        }

        /// <summary>
        /// Checks the data log format.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="format">The format.</param>
        /// <returns></returns>
        static bool CheckDataLogFormat(dynamic value, string format)
        {
            try
            {
                value.ToString(format);
                return true;
            }
            catch (FormatException)
            {
                logger.Error($"The output format string \"{format}\" is not valid");
                return false;
            }
        }

        /// <summary>
        /// Checks the data log column order.
        /// </summary>
        /// <param name="order">The order.</param>
        /// <returns></returns>
        static bool CheckDataLogColumnOrder(string order)
        {
            try
            {
                int max_col = Enum.GetNames(typeof(GazeOutputValue)).Length;
                string[] values = new string[max_col];
                for (int i = 0; i < max_col; i++) values[i] = "";
                String.Format(order, values);
                return true;
            }
            catch (FormatException)
            {
                logger.Error($"The column order string \"{order}\" is not valid");
                return false;
            }
        }

        /// <summary>
        /// Checks the data log column titles.
        /// </summary>
        /// <param name="order">The order.</param>
        /// <param name="titles">The titles.</param>
        /// <returns></returns>
        static bool CheckDataLogColumnTitles(string order, string[] titles)
        {
            try
            {
                sw.WriteLine(order, titles);
                return true;
            }
            catch (FormatException)
            {
                logger.Error($"Column titles do not match with the column order");
                return false;
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
        /// Gets the gaze data value string.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        static string GetGazeDataValueString(bool? data)
        {
            return (data == null) ? "" : ((bool)data).ToString();
        }

        /// <summary>
        /// Gets the gaze data value string.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        static string GetGazeDataValueString(double? data, string format = "")
        {
            return (data == null) ? "" : ((double)data).ToString(format);
        }

        /// <summary>
        /// Computes the eye tracker timestamp.
        /// </summary>
        /// <param name="ts">The ts.</param>
        /// <returns></returns>
        static string GetGazeDataValueString(TimeSpan ts, string format)
        {
            TimeSpan res = ts;
            if (!tracking) delta = res - DateTime.Now.TimeOfDay; ;
            res -= delta;
            return res.ToString(format);
        }

        /// <summary>
        /// Called when [application exit].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        static void OnApplicationExit(object sender, EventArgs e)
        {
            if (config.MouseControl && config.MouseHide) hider.ShowCursor(config.MouseStandardIconPath);

            if (config.DataLogWriteOutput)
            {
                sw.Close();
                sw.Dispose();
            }
            tracker.Dispose();
            logger.Info($"\"{AppDomain.CurrentDomain.BaseDirectory}GazeToMouse.exe\" terminated gracefully{Environment.NewLine}");
        }

        /// <summary>
        /// Called when [gaze data received].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="data">The data.</param>
        static void OnGazeDataReceived(Object sender, GazeDataArgs data)
        {
            
            // write the coordinates to the log file
            if (config.DataLogWriteOutput)
            {
                string[] formatted_values = new string[Enum.GetNames(typeof(GazeOutputValue)).Length];
                formatted_values[(int)GazeOutputValue.DataTimeStamp] = GetGazeDataValueString(data.Timestamp, config.DataLogFormatTimeStamp);
                formatted_values[(int)GazeOutputValue.XCoord] = GetGazeDataValueString(data.XCoord);
                formatted_values[(int)GazeOutputValue.XCoordLeft] = GetGazeDataValueString(data.XCoordLeft);
                formatted_values[(int)GazeOutputValue.XCoordRight] = GetGazeDataValueString(data.XCoordRight);
                formatted_values[(int)GazeOutputValue.YCoord] = GetGazeDataValueString(data.YCoord);
                formatted_values[(int)GazeOutputValue.YCoordLeft] = GetGazeDataValueString(data.YCoordLeft);
                formatted_values[(int)GazeOutputValue.YCoordRight] = GetGazeDataValueString(data.YCoordRight);
                formatted_values[(int)GazeOutputValue.ValidCoordLeft] = GetGazeDataValueString(data.IsValidCoordLeft);
                formatted_values[(int)GazeOutputValue.ValidCoordRight] = GetGazeDataValueString(data.IsValidCoordRight);
                formatted_values[(int)GazeOutputValue.PupilDia] = GetGazeDataValueString(data.Dia, config.DataLogFormatDiameter);
                formatted_values[(int)GazeOutputValue.PupilDiaLeft] = GetGazeDataValueString(data.DiaLeft, config.DataLogFormatDiameter);
                formatted_values[(int)GazeOutputValue.PupilDiaRight] = GetGazeDataValueString(data.DiaRight, config.DataLogFormatDiameter);
                formatted_values[(int)GazeOutputValue.ValidPupilLeft] = GetGazeDataValueString(data.IsValidDiaLeft);
                formatted_values[(int)GazeOutputValue.ValidPupilRight] = GetGazeDataValueString(data.IsValidDiaRight);
                formatted_values[(int)GazeOutputValue.XOriginLeft] = GetGazeDataValueString(data.XOriginLeft, config.DataLogFormatOrigin);
                formatted_values[(int)GazeOutputValue.XOriginRight] = GetGazeDataValueString(data.XOriginRight, config.DataLogFormatOrigin);
                formatted_values[(int)GazeOutputValue.YOriginLeft] = GetGazeDataValueString(data.YOriginLeft, config.DataLogFormatOrigin);
                formatted_values[(int)GazeOutputValue.YOriginRight] = GetGazeDataValueString(data.YOriginRight, config.DataLogFormatOrigin);
                formatted_values[(int)GazeOutputValue.ZOriginLeft] = GetGazeDataValueString(data.ZOriginLeft, config.DataLogFormatOrigin);
                formatted_values[(int)GazeOutputValue.ZOriginRight] = GetGazeDataValueString(data.ZOriginRight, config.DataLogFormatOrigin);
                formatted_values[(int)GazeOutputValue.DistOrigin] = GetGazeDataValueString(data.DistOrigin, config.DataLogFormatOrigin);
                formatted_values[(int)GazeOutputValue.DistOriginLeft] = GetGazeDataValueString(data.DistOriginLeft, config.DataLogFormatOrigin);
                formatted_values[(int)GazeOutputValue.DistOriginRight] = GetGazeDataValueString(data.DistOriginRight, config.DataLogFormatOrigin);
                formatted_values[(int)GazeOutputValue.ValidOriginLeft] = GetGazeDataValueString(data.IsValidOriginLeft);
                formatted_values[(int)GazeOutputValue.ValidOriginRight] = GetGazeDataValueString(data.IsValidOriginRight);
                sw.WriteLine(String.Format(config.DataLogColumnOrder, formatted_values));
                tracking = true;
            }
            // set the cursor position to the gaze position
            if (config.MouseControl)
            {
                if (double.IsNaN(data.XCoord) || double.IsNaN(data.YCoord)) return;
                UpdateMousePosition(Convert.ToInt32(data.XCoord), Convert.ToInt32(data.YCoord));
            }
        }

        /// <summary>
        /// Called when the eye tracker is ready.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        static void OnTrackerEnabled(object sender, EventArgs e)
        {
            if (config.MouseControl && config.MouseHide) hider.HideCursor();
            if (tracking) return;
        }

        /// <summary>
        /// Called when the eye tracker changes from ready to any other state.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        static void OnTrackerDisabled(object sender, EventArgs e)
        {
            if (config.MouseControl && config.MouseHide) hider.ShowCursor(config.MouseStandardIconPath);
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
    }
}
