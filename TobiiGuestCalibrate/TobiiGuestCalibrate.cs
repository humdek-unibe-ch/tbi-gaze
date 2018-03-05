using System;
using System.Diagnostics;
using System.Windows;
using GazeHelper;

/// <summary>
/// Simple wrapper to run Tobii eyetracker guest calibration
/// </summary>
namespace TobiiGuestCalibrate
{
    static class TobiiGuestCalibrate
    {
        static TrackerLogger logger;
        private static JsonConfigParser.ConfigItem config;
        private static bool isCalibrated = false;

        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application app = new Application
            {
                ShutdownMode = ShutdownMode.OnExplicitShutdown
            };
            app.Exit += new ExitEventHandler(OnApplicationExit);
            logger = new TrackerLogger();
            JsonConfigParser parser = new JsonConfigParser(logger);
            config = parser.ParseJsonConfig();
            EyeTracker tracker = new EyeTracker(logger, config.ReadyTimer);
            tracker.TrackerEnabled += OnTrackerReady;
            logger.Info($"Starting \"{AppDomain.CurrentDomain.BaseDirectory}TobiiGuestCalibrate.exe\"");
            logger.Info("Preparing to start Tobii eyetracker guest calibration");
            app.Run();
        }

        /// <summary>
        /// is executed once the tracker is ready
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        static void OnTrackerReady(object sender, EventArgs e)
        {
            if(isCalibrated)
            {
                // calibration was successful, eye tracker switched to state ready
                Application.Current.Dispatcher.Invoke(callback: () => { Application.Current.Shutdown(); });
            }
            else
            {
                // not yet calibrated, perfrom calibration
                logger.Info($"Starting Tobii eyetracker calibration \"{config.TobiiPath}\\{config.TobiiGuestCalibrate}  {config.TobiiGuestCalibrateArguments}\"");
                Process tobii_calibrate = Process.Start($"{config.TobiiPath}\\{config.TobiiGuestCalibrate}", config.TobiiGuestCalibrateArguments);
                tobii_calibrate.WaitForExit();
                isCalibrated = true;
                logger.Info($"\"{config.TobiiPath}\\{config.TobiiGuestCalibrate}  {config.TobiiGuestCalibrateArguments}\" terminated ");
            }
        }

        /// <summary>
        /// Called when [application exit].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        static void OnApplicationExit(object sender, EventArgs e)
        {
            if (!isCalibrated) logger.Error("Unable to start Tobii eyetracker guest calibration");
            logger.Info($"\"{AppDomain.CurrentDomain.BaseDirectory}TobiiGuestCalibrate.exe\" terminated gracefully{Environment.NewLine}");
        }
    }
}