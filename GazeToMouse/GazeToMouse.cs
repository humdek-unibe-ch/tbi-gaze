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
        private static TrackerHandler tracker;
        private static StreamWriter sw;
        private static TimeSpan delta;
        private static TrackerLogger logger;
        private static MouseHider hider;
        private static ConfigItem config;
        private static GazeDataError gazeDataError = 0;
        private static GazeConfigError gazeConfigError = 0;
        private static string outputFilePath;

        /// <summary>
        /// Error values of the gaze output data
        /// </summary>
        [Flags]
        private enum GazeDataError
        {
            FallbackToCore = 0x01,
            DeviceInterrupt = 0x02
        }

        /// <summary>
        /// Error values of the configuration
        /// </summary>
        private enum GazeConfigError
        {
            FallbackToDefaultConfigName = 0x01,
            FallbackToCurrentOutputDir = 0x02,
            FallbackToDefualtConfig = 0x04,

            FallbackToDefaultDiameterFormat = 0x08,
            FallbackToDefaultOriginFormat = 0x10,
            FallbackToDefaultTimestampFormat = 0x20,
            OmitColumnTitles = 0x40,
            FallbackToDefualtColumnOrder = 0x80
        }

        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            Application app = new Application();
            app.Exit += new ExitEventHandler(OnApplicationExit);
            // we need a window to gracefully terminate the program with WM_CLOSE
            Window window = new Window
            {
                Visibility = Visibility.Hidden,
                WindowStyle = WindowStyle.None
            };

            string starttime = $"{DateTime.Now:yyyyMMddTHHmmss}";

            logger = new TrackerLogger();
            logger.Info($"Starting \"{AppDomain.CurrentDomain.BaseDirectory}GazeToMouse.exe\"");

            JsonConfigParser parser = new JsonConfigParser(logger);
            ConfigItem default_config = parser.GetDefaultConfig();
            config = parser.ParseJsonConfig();
            if( config == null )
            {
                logger.Warning("Using default configuration values");
                config = default_config;
                gazeConfigError |= GazeConfigError.FallbackToDefualtConfig;
            }

            // check configuration name
            if(!CheckConfigName(config.ConfigName))
            {
                config.ConfigName = default_config.ConfigName;
                logger.Warning($"Using the default config name: \"{config.ConfigName}\"");
                gazeConfigError |= GazeConfigError.FallbackToDefaultConfigName;
            }

            if (config.DataLogWriteOutput)
            {
                string gazeFilePostfix = $"{Environment.MachineName}_{config.ConfigName}_gaze";
                string gazeFileName = $"{starttime}_{gazeFilePostfix}";

                // create gaze data file
                if (config.DataLogPath == "") config.DataLogPath = Directory.GetCurrentDirectory();
                sw = CreateGazeOutputFile(config.DataLogPath, gazeFileName);
                if (sw == null)
                {
                    // something went wrong, write to the current directory
                    config.DataLogPath = Directory.GetCurrentDirectory();
                    outputFilePath = $"{config.DataLogPath}\\{gazeFileName}";
                    logger.Warning($"Writing gaze data to the current directory: \"{outputFilePath}\"");
                    gazeConfigError |= GazeConfigError.FallbackToCurrentOutputDir;
                    sw = new StreamWriter(gazeFileName);
                }

                // check output data format
                if (!CheckDataLogFormat(DateTime.Now.TimeOfDay, config.DataLogFormatTimeStamp))
                {
                    config.DataLogFormatTimeStamp = default_config.DataLogFormatTimeStamp;
                    logger.Warning($"Using the default output format for timestamps: \"{config.DataLogFormatTimeStamp}\"");
                    gazeConfigError |= GazeConfigError.FallbackToDefaultTimestampFormat;
                }
                if (!CheckDataLogFormat(1.000000, config.DataLogFormatDiameter))
                {
                    config.DataLogFormatDiameter = default_config.DataLogFormatDiameter;
                    logger.Warning($"Using the default output format for pupil diameters: \"{config.DataLogFormatDiameter}\"");
                    gazeConfigError |= GazeConfigError.FallbackToDefaultDiameterFormat;
                }
                if (!CheckDataLogFormat(1.000000, config.DataLogFormatOrigin))
                {
                    config.DataLogFormatOrigin = default_config.DataLogFormatOrigin;
                    logger.Warning($"Using the default output format for gaze origin values: \"{config.DataLogFormatOrigin}\"");
                    gazeConfigError |= GazeConfigError.FallbackToDefaultOriginFormat;
                }
                if (!CheckDataLogColumnOrder(config.DataLogColumnOrder))
                {
                    config.DataLogColumnOrder = default_config.DataLogColumnOrder;
                    logger.Warning($"Using the default column order: \"{config.DataLogColumnOrder}\"");
                    gazeConfigError |= GazeConfigError.FallbackToDefualtColumnOrder;
                }
                if (!CheckDataLogColumnTitles(config.DataLogColumnOrder, config.DataLogColumnTitle))
                {
                    logger.Warning($"Column titles are omitted");
                    gazeConfigError |= GazeConfigError.OmitColumnTitles;
                }

                // delete old files
                DeleteOldGazeLogFiles(config.DataLogPath, config.DataLogCount, $"*_{gazeFilePostfix}");
            }
            parser.SerializeJsonConfig(config, $"{config.DataLogPath}\\{starttime}"
                + $"_{Environment.MachineName}_{config.ConfigName}_config{GetGazeConfigErrorString()}.json");
            
            // hide the mouse cursor
            hider = new MouseHider(logger);
            if (config.MouseControl && config.MouseHide) hider.HideCursor();


            // intitialise the tracker device 
            if(config.TrackerDevice == 1)
            {
                EyeTrackerPro tracker_pro = new EyeTrackerPro(logger, config.ReadyTimer, config.LicensePath);
                if (tracker_pro.IsLicenseOk())
                {
                    tracker = tracker_pro;
                }
                else
                {
                    tracker_pro.Dispose();
                    config.TrackerDevice = 0;
                    logger.Warning("Fall back to Tobii Core SDK");
                    gazeDataError |= GazeDataError.FallbackToCore;
                }
            }
            if(config.TrackerDevice == 0)
            {
                tracker = new EyeTrackerCore(logger, config.ReadyTimer, config.GazeFilterCore);
            }
            else if(config.TrackerDevice == 2) {
                tracker = new MouseTracker(logger, config.ReadyTimer);
            }
            tracker.GazeDataReceived += OnGazeDataReceived;
            tracker.TrackerEnabled += OnTrackerEnabled;
            tracker.TrackerDisabled += OnTrackerDisabled;

            app.Run(window);
        }

        /// <summary>
        /// Checks the name of the configuration.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        static bool CheckConfigName( string name )
        {
            if(!Uri.IsWellFormedUriString(name, UriKind.Relative))
            {
                logger.Error($"The config file name \"{name}\" is invalid and cannot be used as file name postfix");
                return false;
            }
            return true;
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
                string str = value.ToString(format);
                logger.Debug($"Use format \"{format}\": {str}");
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
        /// Converts a integer value to a binary string.
        /// </summary>
        /// <param name="val">The value.</param>
        /// <param name="len">The length of the binary string.</param>
        /// <returns>a binary string of specified length, left-padded with '0'</returns>
        static string ConvertToBinString( int val, int len )
        {
            string val_bin = Convert.ToString(val, 2);
            return val_bin.PadLeft(len, '0');
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
        /// Gets the gaze error string.
        /// </summary>
        /// <returns>the error string with binary error values if errors ocurred, the empty srting otherwise</returns>
        static string GetGazeConfigErrorString()
        {
            int formatError = ((int)gazeConfigError & 0xF8) >> 3; 
            int configError = ((int)gazeConfigError & 0x07);
            string confErrorStr = "_err"
                + $"-{ConvertToBinString(configError, 3)}"
                + $"-{ConvertToBinString(formatError, 5)}";
            if (gazeConfigError == 0) confErrorStr = "";
            return confErrorStr;

        }

        /// <summary>
        /// Gets the gaze error string.
        /// </summary>
        /// <returns>the error string with binary error values if errors ocurred, the empty srting otherwise</returns>
        static string GetGazeDataErrorString()
        {
            string confErrorStr = $"_err-{ConvertToBinString((int)gazeDataError, 2)}";
            if (gazeDataError == 0) confErrorStr = "";
            return confErrorStr;

        }

        /// <summary>
        /// Gets the gaze data value string.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>a string containing the gaze data value if the value is not null, the empty string otherwise</returns>
        static string GetGazeDataValueString(bool? data)
        {
            return (data == null) ? "" : ((bool)data).ToString();
        }

        /// <summary>
        /// Gets the gaze data value string.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="format">The format of the data will be converted to.</param>
        /// <returns>
        /// a string containing the gaze data value if the value is not null, the empty string otherwise
        /// </returns>
        static string GetGazeDataValueString(double? data, string format = "")
        {
            return (data == null) ? "" : ((double)data).ToString(format);
        }

        /// <summary>
        /// Computes the eye tracker timestamp.
        /// </summary>
        /// <param name="ts">The timestamp.</param>
        /// <param name="format">The format the timestamp is converted to.</param>
        /// <returns>
        /// a string containing the timestamp 
        /// </returns>
        static string GetGazeDataValueString(TimeSpan ts, string format)
        {
            TimeSpan res = ts;
            if (!tracking) delta = res - DateTime.Now.TimeOfDay; ;
            res -= delta;
            return res.ToString(format);
        }

        /// <summary>
        /// Determines whether the gaze data set is valid.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="ignore_invalid">if set to <c>true</c> [ignore invalid].</param>
        /// <returns>
        ///   <c>true</c> if at least one value of the gaze data is valid; otherwise, <c>false</c>.
        /// </returns>
        static bool IsDataValid(GazeDataArgs data, bool ignore_invalid)
        {
            if (!ignore_invalid) return true; // don't check, log everything
            if ((data.IsValidCoordLeft == true)
                || (data.IsValidCoordRight == true)
                || (data.IsValidDiaLeft == true)
                || (data.IsValidDiaRight == true)
                || (data.IsValidOriginLeft == true)
                || (data.IsValidOriginRight == true)
                || ((data.IsValidCoordLeft == null)
                    && (data.IsValidCoordRight == null)
                    && (data.IsValidDiaLeft == null)
                    && (data.IsValidDiaRight == null)
                    && (data.IsValidOriginLeft == null)
                    && (data.IsValidOriginRight == null)))
                return true; // at least one value is valid or Core SDK is used
            else return false; // all vaules of this data set are invalid
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
                File.Move(outputFilePath, $"{outputFilePath}{GetGazeDataErrorString()}.txt");
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
            if (config.DataLogWriteOutput && IsDataValid(data, config.DataLogIgnoreInvalid))
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
            gazeDataError |= GazeDataError.DeviceInterrupt;
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
