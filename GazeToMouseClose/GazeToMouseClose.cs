using System.Diagnostics;
using GazeHelper;

/// <summary>
/// Simple wrapper to send WM_CLOSE signal to GazeToMouse.exe
/// </summary>
namespace GazeToMouseClose
{
    static class GazeToMouseClose
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        static void Main()
        {
            TrackerLogger logger = new TrackerLogger();
            logger.Info("Sending WM_CLOSE signal to process \"GazeToMouse.exe\"");
            Process.Start("taskkill", "/T /IM GazeToMouse.exe");
        }
    }
}