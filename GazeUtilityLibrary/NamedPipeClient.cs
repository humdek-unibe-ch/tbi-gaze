﻿using System;
using System.IO.Pipes;
using System.IO;
using System.Text;

namespace GazeUtilityLibrary
{
    public static class NamedPipeClient
    {
        /// <summary>
        /// Sends a signal through the named gaze pipe.
        /// </summary>
        /// <param name="signal">The signal to be sent.</param>
        public static void SendSignal(string signal, TrackerLogger logger)
        {
            string pipeName = "tobii_gaze";
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

        /// <summary>
        /// Sends a signal through the named gaze pipe and awaits a reply.
        /// </summary>
        /// <param name="signal">The request to be sent.</param>
        public static void SendRequest(string signal, TrackerLogger logger)
        {
            string pipeName = "tobii_gaze";
            using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(pipeName))
            using (StreamReader sr = new StreamReader(pipeClient))
            using (StreamWriter sw = new StreamWriter(pipeClient))
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

                logger.Info($"Sending {signal} request to pipe {pipeName}");
                sw.WriteLine(signal);
                sw.Flush();

                logger.Debug($"Awaiting {signal} reply from {pipeName}");
                string? msg = sr.ReadLine();
                
                if (msg != null)
                {
                    if (msg.StartsWith("SUCCESS"))
                    {
                        logger.Info($"Request {signal} on {pipeName} was succesful");
                    }
                    else if (msg.StartsWith("FAILED"))
                    {
                        logger.Info($"Request {signal} on {pipeName} failed");
                    }
                }
            }
        }
    }
}
