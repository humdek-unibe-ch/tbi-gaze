using System;
using System.IO;

namespace GazeUtilityLibrary
{
    /// <summary>
    /// Simple logger class.
    /// </summary>
    public class TrackerLogger
    {
        private string _logPath;
        private string _logFile;
        private string _logFileBak;
        private string _dateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff";
        private const int _maxLogSize = 1000000; // 1 MB
        private readonly object _syncObject = new object();
        private enum LogLevel
        {
            Debug,
            Info,
            Warning,
            Error
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackerLogger"/> class.
        /// </summary>
        public TrackerLogger(string? logPath, EOutputType type = EOutputType.gaze)
        {
            _logPath = logPath ?? AppDomain.CurrentDomain.BaseDirectory;
            _logFile = $"{_logPath}\\{Environment.MachineName}_{type.ToString()}.log";
            _logFileBak = $"{_logFile}.0";
        }

        /// <summary>
        /// Writes the log messages to the log file.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="message">The message.</param>
        private void WriteLog(LogLevel level, string message)
        {
            string prefix = DateTime.Now.ToString(_dateTimeFormat);
            switch (level)
            {
                case LogLevel.Debug: prefix += " [DEBUG]   "; break;
                case LogLevel.Info: prefix += " [INFO]    "; break;
                case LogLevel.Warning: prefix += " [WARNING] "; break;
                case LogLevel.Error: prefix += " [ERROR]   "; break;
            }

            // use a file rotation of two files. If a log file gets too big, a
            // new log file is created whil the existing one is moved to the
            // backup file (overwriting an existing one)
            FileInfo fi = new FileInfo(_logFile);
            if (fi.Exists && fi.Length > _maxLogSize)
            {
                File.Delete(_logFileBak);
                File.Move(_logFile, _logFileBak);
            }

            try
            {
                lock(_syncObject)
                {
                    StreamWriter sw = new StreamWriter(_logFile, true);
                    sw.WriteLine(prefix + message);
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                DumpFatal(e);
            }
        }

        /// <summary>
        /// Dumps exception to a new file if it is not possible to write to the main log file.
        /// </summary>
        /// <param name="e">The exception.</param>
        public void DumpFatal(Exception e)
        {
            StreamWriter sw = new StreamWriter($"{_logPath}\\{DateTime.Now.ToString("yyyyMMddTHHmmss")}_{Environment.MachineName}_fatal.log", true);
            sw.WriteLine(e.ToString());
            sw.Close();
        }

        /// <summary>
        /// wrapper function for debug level logging.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Debug(string message)
        {
#if DEBUG
            WriteLog(LogLevel.Debug, message);
#endif
        }

        /// <summary>
        /// wrapper function for info level logging
        /// </summary>
        /// <param name="message">The message.</param>
        public void Info(string message) { WriteLog(LogLevel.Info, message); }

        /// <summary>
        /// wrapper function for warning level logging
        /// </summary>
        /// <param name="message">The message.</param>
        public void Warning(string message) { WriteLog(LogLevel.Warning, message); }

        /// <summary>
        /// wrapper function for error level logging
        /// </summary>
        /// <param name="message">The message.</param>
        public void Error(string message) { WriteLog(LogLevel.Error, message); }
    }
}