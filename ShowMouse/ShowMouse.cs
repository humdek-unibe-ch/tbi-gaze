/**
 * @brief Simple tool to restore the mousepointer
 * 
 * @file    ShowMouse.cs
 * @author  Simon Maurer, simon.maurer@humdek.unibe.ch
 * @date    January 2018
 */

using System.Runtime.InteropServices;
using GazeHelper;

namespace ShowMouse
{
    /**
     * @brief Main entry point to the program ShowMouse
     */
    static class ShowMouse
    {
        [DllImport("user32.dll")]
        static extern bool ShowCursor(bool show);
        static void Main()
        {
            Logger logger = new Logger();
            JsonConfigParser parser = new JsonConfigParser(logger);
            JsonConfigParser.ConfigItem config = parser.ParseJsonConfig();
            MouseHider hider = new MouseHider(logger);
            hider.ShowCursor(config.StandardMouseIconPath);
        }
    }
}
