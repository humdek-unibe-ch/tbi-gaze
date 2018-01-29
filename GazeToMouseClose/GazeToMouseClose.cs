/**
 * Simple wrapper to send WM_CLOSE signal to GazeToMouse.exe
 * 
 * @file    Program.cs
 * @author  Simon Maurer, simon.maurer@humdek.unibe.ch
 * @date    January 2018
 */

using System.Diagnostics;
using GazeHelper;

namespace GazeToMouseClose
{
    /**
     * @brief Main entry point of the program GazeToMouseClose
     */
    static class GazeToMouseClose
    {
        static void Main()
        {
            Logger logger = new Logger();
            logger.Info("Sending WM_CLOSE signal to process \"GazeToMouse.exe\"");
            Process.Start("taskkill", "/IM GazeToMouse.exe");
        }
    }
}