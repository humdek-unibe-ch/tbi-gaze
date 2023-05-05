using System;
using System.Diagnostics;
using System.Windows;
using GazeHelper;

/// <summary>
/// Simple wrapper to run Tobii eyetracker calibration
/// </summary>
namespace TobiiCalibrate
{
  
    class TobiiCalibrate
    {
        static TrackerLogger logger;
        private static ConfigItem config;
        private static bool isCalibrated = false;

        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            Application app = new Application
            {
                ShutdownMode = ShutdownMode.OnExplicitShutdown
            };
            app.Exit += new ExitEventHandler(OnApplicationExit);
            logger = new TrackerLogger();
            JsonConfigParser parser = new JsonConfigParser(logger);
            config = parser.ParseJsonConfig();
            if (config == null)
            {
                logger.Warning("Using default configuration values");
                config = parser.GetDefaultConfig();
            }
            TrackerHandler tracker = new EyeTrackerPro(logger, config.ReadyTimer, config.LicensePath);
            tracker.TrackerEnabled += OnTrackerReady;
            logger.Info("Preparing to start Tobii eyetracker calibration");
            app.Run();
        }

        /// <summary>
        /// is executed once the tracker is ready
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        static void OnTrackerReady(object sender, EventArgs e)
        {
            TrackerHandler tracker = (TrackerHandler)sender;
            if (isCalibrated)
            {
                // calibration was successful, eye tracker switched to state ready
                Application.Current.Dispatcher.Invoke(callback: () => { Application.Current.Shutdown(); });
            }
            else
            {
                string executablePath = $"{tracker.PatternReplace(config.TobiiApplicationPath)}\\{tracker.PatternReplace(config.TobiiCalibrate)}";
                string arguments = $"{tracker.PatternReplace(config.TobiiCalibrateArguments)}";
                StartCalibration(executablePath, arguments);
            }
        }

        static void StartCalibration(string executablePath, string arguments)
        {
            logger.Info($"Starting Tobii eyetracker calibration \"{executablePath} {arguments}\"");
            Process tobii_calibrate = new Process();
            // Redirect the output stream of the child process.
            tobii_calibrate.StartInfo.UseShellExecute = false;
            tobii_calibrate.StartInfo.RedirectStandardError = true;
            tobii_calibrate.StartInfo.RedirectStandardOutput = true;
            tobii_calibrate.StartInfo.FileName = executablePath;
            tobii_calibrate.StartInfo.Arguments = arguments;
            tobii_calibrate.Start();

            string stdOutput = tobii_calibrate.StandardOutput.ReadToEnd();

            tobii_calibrate.WaitForExit();
            int exitCode = tobii_calibrate.ExitCode;
            if (exitCode == 0)
            {
                logger.Info("Eye Tracker Manager was called successfully!");
                isCalibrated = true;
            }
            else
            {
                logger.Error($"Eye Tracker Manager call returned the error code: {exitCode}");
                foreach (string line in stdOutput.Split(Environment.NewLine.ToCharArray()))
                {
                    if (line.StartsWith("ETM Error:"))
                    {
                        logger.Error(line);
                    }
                }
            }
            Application.Current.Dispatcher.Invoke(callback: () => { Application.Current.Shutdown(); });
        }

        /// <summary>
        /// Called when [application exit].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        static void OnApplicationExit(object sender, EventArgs e)
        {
            if (!isCalibrated) logger.Error("Unable to start Tobii eyetracker calibration");
            logger.Info($"\"{AppDomain.CurrentDomain.BaseDirectory}TobiiCalibrate.exe\" terminated gracefully{Environment.NewLine}");
        }
    }
}