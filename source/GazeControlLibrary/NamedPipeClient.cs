/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
using System.Runtime.InteropServices;
using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Linq;
using Newtonsoft.Json;

namespace GazeControlLibrary
{
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error
    }
    /// <summary>
    /// The named pipe client handler.
    /// </summary>
    public static class NamedPipeClient
    {
        public delegate void Logger(LogLevel level, string msg);

        public static void HandleCommandsC(IntPtr command, int trialId, int reset, IntPtr label)
        {
            HandleCommands(Marshal.PtrToStringAnsi(command), reset == 0 ? false : true, trialId, Marshal.PtrToStringAnsi(label));
        }

        /// <summary>
        /// Sends command messages or command request to a named pipe tobii_gaze according to the command provided.
        /// </summary>
        /// <param name="command">The command to be sent to the named pipe.</param>
        /// <param name="reset">If set to true the relative timestamp will be reset with this command.</param>
        /// <param name="trialId">If set the gaze data will be annotated with this trial ID.</param>
        /// <param name="label">If set the gaze data will be annotated with this label.</param>
        /// <param name="logger">The application logger.</param>
        public static void HandleCommands(string command, bool reset = false, int? trialId = 0, string label = null, Logger logger = null)
        {
            string pipeName = "tobii_gaze";

            if (!AwaitServer(pipeName, logger))
            {
                if (logger != null) logger(LogLevel.Warning, $"No pipe server '{pipeName}' available");
            }

            switch (command)
            {
                case null:
                case "GAZE_RECORDING_DISABLE":
                case "GAZE_RECORDING_ENABLE":
                case "MOUSE_TRACKING_DISABLE":
                case "MOUSE_TRACKING_ENABLE":
                case "RESET_DRIFT_COMPENSATION":
                case "TERMINATE":
                    try
                    {
                        SendSignal(pipeName, command, reset, trialId, label, logger);
                    }
                    catch (Exception error)
                    {
                        if (logger != null) logger(LogLevel.Error, error.Message);
                    }
                    break;
                case "DRIFT_COMPENSATION":
                case "CUSTOM_CALIBRATE":
                case "VALIDATE":
                    try
                    {
                        SendRequest(pipeName, command, reset, trialId, label, logger);
                    }
                    catch (Exception error)
                    {
                        if (logger != null) logger(LogLevel.Error, error.Message);
                    }
                    break;
                default:
                    if (logger != null) logger(LogLevel.Error, $"unknown command: {command}");
                    break;
            }
        }

        /// <summary>
        /// Periodically check if a pipe file exists. If the file exists, the server is running. Otherwise, no pipe server is running.
        /// </summary>
        /// <param name="pipeName">The name of the pipe to check.</param>
        /// <param name="logger">The application logger.</param>
        /// <returns>True if the server pipe file was found, false otherwise.</returns>
        private static bool AwaitServer(string pipeName, Logger logger)
        {
            int count = 0;
            int countMax = 10;
            while(!Directory.GetFiles(@"\\.\pipe\").ToList().Contains($"\\\\.\\pipe\\{pipeName}") && count < countMax)
            {
                count++;
                if (logger != null) logger(LogLevel.Info, $"Awaiting pipe server {pipeName} ({count})...");
                Thread.Sleep(1000);
            }

            return count != countMax;
        }

        /// <summary>
        /// Sends a signal through the named gaze pipe.
        /// </summary>
        /// <param name="pipeName">The name of the pipe.</param>
        /// <param name="signal">The signal to be sent.</param>
        /// <param name="reset">If set to true the relative timestamp will be reset with this command.</param>
        /// <param name="trialId">If set the gaze data will be annotated with this trial ID.</param>
        /// <param name="label">If set the gaze data will be annotated with this label.</param>
        /// <param name="logger">The application logger.</param>
        private static void SendSignal(string pipeName, string signal, bool reset, int? trialId, string label, Logger logger)
        {
            using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", pipeName, PipeDirection.Out))
            {
                if (logger != null) logger(LogLevel.Debug, $"Attempting to connect to pipe {pipeName}...");
                try
                {
                    pipeClient.Connect(1000);
                }
                catch (Exception ex)
                {
                    if (logger != null) logger(LogLevel.Error, $"Connection to pipe {pipeName} failed: {ex.Message}");
                    if (logger != null) logger(LogLevel.Warning, $"Failed to send signal {signal}");
                    return;
                }
                if (logger != null) logger(LogLevel.Debug, $"Connected to pipe {pipeName}...");
                if (logger != null) logger(LogLevel.Debug, $"There are currently {pipeClient.NumberOfServerInstances} pipe server instances open.");

                using (StreamWriter sw = new StreamWriter(pipeClient))
                {
                    if (logger != null) logger(LogLevel.Info, $"Sending {signal} signal to pipe {pipeName}");
                    sw.WriteLine(JsonConvert.SerializeObject(new PipeCommand(signal, reset, trialId, label)));
                }
            }
        }

        /// <summary>
        /// Sends a signal through the named gaze pipe and awaits a reply.
        /// </summary>
        /// <param name="pipeName">The name of the pipe.</param>
        /// <param name="signal">The signal to be sent.</param>
        /// <param name="reset">If set to true the relative timestamp will be reset with this command.</param>
        /// <param name="trialId">If set the gaze data will be annotated with this trial ID.</param>
        /// <param name="label">If set the gaze data will be annotated with this label.</param>
        /// <param name="logger">The application logger.</param>
        private static void SendRequest(string pipeName, string signal, bool reset, int? trialId, string label, Logger logger)
        {
            using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(pipeName))
            using (StreamReader sr = new StreamReader(pipeClient))
            using (StreamWriter sw = new StreamWriter(pipeClient))
            {
                if (logger != null) logger(LogLevel.Debug, $"Attempting to connect to pipe {pipeName}...");
                try
                {
                    pipeClient.Connect(1000);
                }
                catch (Exception ex)
                {
                    if (logger != null) logger(LogLevel.Error, $"Connection to pipe {pipeName} failed: {ex.Message}");
                    if (logger != null) logger(LogLevel.Warning, $"Failed to send signal request {signal}");
                    return;
                }
                if (logger != null) logger(LogLevel.Debug, $"Connected to pipe {pipeName}...");
                if (logger != null) logger(LogLevel.Debug, $"There are currently {pipeClient.NumberOfServerInstances} pipe server instances open.");

                if (logger != null) logger(LogLevel.Info, $"Sending {signal} request to pipe {pipeName}");
                sw.WriteLine(JsonConvert.SerializeObject(new PipeCommand(signal, reset, trialId, label)));
                sw.Flush();

                if (logger != null) logger(LogLevel.Debug, $"Awaiting {signal} reply from {pipeName}");
                string msg = sr.ReadLine();
                
                if (msg != null)
                {
                    if (msg.StartsWith("SUCCESS"))
                    {
                        if (logger != null) logger(LogLevel.Info, $"Request {signal} on {pipeName} was succesful");
                    }
                    else if (msg.StartsWith("FAILED"))
                    {
                        if (logger != null) logger(LogLevel.Info, $"Request {signal} on {pipeName} failed");
                    }
                }
            }
        }
    }
}
