using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.IO;
using System.Windows;
using GazeUtilityLibrary;

namespace GazeToMouseClose
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            SendTerminationSignal();
            Current.Shutdown();
        }

        /// <summary>
        /// Sends the termination signal through a named pipe.
        /// </summary>
        private void SendTerminationSignal()
        {
            string pipeName = "tobii_gaze";
            TrackerLogger logger = new TrackerLogger();
            using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", pipeName, PipeDirection.Out))
            {
                logger.Debug($"Attempting to connect to pipe {pipeName}...");
                try
                {
                    pipeClient.Connect(1000);
                }
                catch (Exception ex)
                {
                    logger.Error($"Connection to pipe {pipeName} failed: {ex.Message}");
                    return;
                }
                logger.Info($"Connected to pipe {pipeName}...");
                logger.Debug($"There are currently {pipeClient.NumberOfServerInstances} pipe server instances open.");

                using (StreamWriter sw = new StreamWriter(pipeClient))
                {
                    logger.Info($"Sending TERMINATE signal to pipe {pipeName}");
                    sw.WriteLine("TERMINATE");
                }
            }
        }
    }
}
