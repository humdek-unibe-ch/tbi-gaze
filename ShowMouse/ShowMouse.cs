/**
 * @brief Simple tool to restore the mousepointer
 * 
 * @file    Program.cs
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
            JsonConfigParser parser = new JsonConfigParser();
            JsonConfigParser.ConfigItem config = parser.ParseJsonConfig();
            MouseHider hider = new MouseHider();
            hider.ShowCursor(config.StandardMouseIconPath);
        }
    }
}
