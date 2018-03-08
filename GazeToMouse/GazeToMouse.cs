using System;
using System.Collections.Generic;
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
                if (config.WriteDataLog && !CheckOutputFormat(config.OutputOrder, config.FormatTimeStamp, config.FormatDiameter))
                {
                    // something is wrong with the configured format, use the default format
                    ConfigItem default_config = parser.GetDefaultConfig();
                    config.OutputOrder = default_config.OutputOrder;
                    config.FormatTimeStamp = default_config.FormatTimeStamp;
                    config.FormatDiameter = default_config.FormatDiameter;
                    logger.Warning($"Using default output format of the from: \"{GetFormatSample(config.OutputOrder, config.FormatTimeStamp, config.FormatDiameter)}\"");
                }

                if (config.WriteDataLog) sw.WriteLine(config.OutputOrder, config.ValueTitle);

                // delete old files
                DeleteOldGazeLogFiles(config.OutputPath, config.OutputCount, $"*{gazeFilePostfix}");
            }
            
            // hide the mouse cursor
            hider = new MouseHider(logger);
            if (config.ControlMouse && config.HideMouse) hider.HideCursor();


            // initialize host. Make sure that the Tobii service is running
            if(config.TobiiSDK == 1)
            {
                tracker = new EyeTrackerPro(logger, config.ReadyTimer);
            }
            else if(config.TobiiSDK == 0)
            {
                tracker = new EyeTrackerCore(logger, config.ReadyTimer, config.GazeFilter);
            }
            tracker.GazeDataReceived += OnGazeDataReceived;
            tracker.TrackerEnabled += OnTrackerEnabled;
            tracker.TrackerDisabled += OnTrackerDisabled;
            app.Run(window);
        }

        /// <summary>
        /// Checks whether a string formatting is applicable.
        /// </summary>
        /// <param name="format">format string to be checked.</param>
        /// <returns><c>true</c> if the format is ok; otherwise, <c>false</c></returns>
        static bool CheckOutputFormat(string order, string format_timestamp, string format_diameter)
        {
            try
            {
                logger.Info($"Output format is of the from: \"{GetFormatSample(order, format_timestamp, format_diameter)}\"");
                return true;
            }
            catch (FormatException)
            {
                logger.Error($"Output format string was not in a correct format");
                return false;
            }
        }


        /// <summary>
        /// Takes a valid format string as parameter and returns the string with sample gaze values.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns>a formatted string of sample gaze values.</returns>
        static string GetFormatSample(string order, string format_timestamp, string format_diameter)
        {
            string[] formatted_values = new string[Enum.GetNames(typeof(GazeOutputValue)).Length];
            formatted_values[(int)GazeOutputValue.DataTimeStamp] = DateTime.Now.TimeOfDay.ToString(format_timestamp);
            formatted_values[(int)GazeOutputValue.XCoord] = 1000.ToString();
            formatted_values[(int)GazeOutputValue.XCoordLeft] = 1000.ToString();
            formatted_values[(int)GazeOutputValue.XCoordRight] = 1000.ToString();
            formatted_values[(int)GazeOutputValue.YCoord] = 1000.ToString();
            formatted_values[(int)GazeOutputValue.YCoordLeft] = 1000.ToString();
            formatted_values[(int)GazeOutputValue.YCoordRight] = 1000.ToString();
            formatted_values[(int)GazeOutputValue.PupilDia] = 1.000000.ToString(format_diameter);
            formatted_values[(int)GazeOutputValue.PupilDiaLeft] = 1.000000.ToString(format_diameter);
            formatted_values[(int)GazeOutputValue.PupilDiaRight] = 1.000000.ToString(format_diameter);
            formatted_values[(int)GazeOutputValue.ValidLeft] = true.ToString();
            formatted_values[(int)GazeOutputValue.ValidRight] = true.ToString();
            return String.Format(order, formatted_values);
        }

        static int GetConfigValueCount()
        {
            return config.ValueTitle.Length;
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
        /// Called when [gaze data received].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="data">The data.</param>
        static void OnGazeDataReceived(Object sender, GazeDataArgs data)
        {
            
            // write the coordinates to the log file
            if (config.WriteDataLog)
            {
                string[] formatted_values = new string[Enum.GetNames(typeof(GazeOutputValue)).Length];
                formatted_values[(int)GazeOutputValue.DataTimeStamp] = GetGazeDataValueString(data.Timestamp, config.FormatTimeStamp);
                formatted_values[(int)GazeOutputValue.XCoord] = GetGazeDataValueString(data.XCoord);
                formatted_values[(int)GazeOutputValue.XCoordLeft] = GetGazeDataValueString(data.XCoordLeft);
                formatted_values[(int)GazeOutputValue.XCoordRight] = GetGazeDataValueString(data.XCoordRight);
                formatted_values[(int)GazeOutputValue.YCoord] = GetGazeDataValueString(data.YCoord);
                formatted_values[(int)GazeOutputValue.YCoordLeft] = GetGazeDataValueString(data.YCoordLeft);
                formatted_values[(int)GazeOutputValue.YCoordRight] = GetGazeDataValueString(data.YCoordRight);
                formatted_values[(int)GazeOutputValue.PupilDia] = GetGazeDataValueString(data.Dia, config.FormatDiameter);
                formatted_values[(int)GazeOutputValue.PupilDiaLeft] = GetGazeDataValueString(data.DiaLeft, config.FormatDiameter);
                formatted_values[(int)GazeOutputValue.PupilDiaRight] = GetGazeDataValueString(data.DiaRight, config.FormatDiameter);
                formatted_values[(int)GazeOutputValue.ValidLeft] = GetGazeDataValueString(data.ValidLeft);
                formatted_values[(int)GazeOutputValue.ValidRight] = GetGazeDataValueString(data.ValidRight);
                sw.WriteLine(String.Format(config.OutputOrder, formatted_values));
                tracking = true;
            }
            // set the cursor position to the gaze position
            if (config.ControlMouse)
            {
                if (double.IsNaN(data.XCoord) || double.IsNaN(data.YCoord)) return;
                UpdateMousePosition(Convert.ToInt32(data.XCoord), Convert.ToInt32(data.YCoord));
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
        static string GetGazeDataValueString(double? data, string format="")
        {
            return (data == null) ? "" : ((double)data).ToString(format);
        }

        /// <summary>
        /// Computes the eye tracker timestamp.
        /// </summary>
        /// <param name="ts">The ts.</param>
        /// <returns></returns>
        static string GetGazeDataValueString( TimeSpan ts, string format )
        {
            TimeSpan res = ts;
            if (!tracking) delta = res - DateTime.Now.TimeOfDay; ;
            res -= delta;
            return res.ToString(format);
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
