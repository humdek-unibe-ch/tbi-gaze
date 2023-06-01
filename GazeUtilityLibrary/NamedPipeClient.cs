using System;
using System.IO.Pipes;
using System.IO;

namespace GazeUtilityLibrary
{
    public static class NamedPipeClient
    {
        /// <summary>
        /// Sends the a signal through the named gaze pipe.
        /// </summary>
        /// <param name="signal">The signal to be sent.</param>
        public static void SendSignal(string signal)
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
                logger.Debug($"Connected to pipe {pipeName}...");
                logger.Debug($"There are currently {pipeClient.NumberOfServerInstances} pipe server instances open.");

                using (StreamWriter sw = new StreamWriter(pipeClient))
                {
                    logger.Info($"Sending {signal} signal to pipe {pipeName}");
                    sw.WriteLine(signal);
                }
            }
        }
    }
}
