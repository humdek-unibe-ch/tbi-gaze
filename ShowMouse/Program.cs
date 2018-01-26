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
    static class Program
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
