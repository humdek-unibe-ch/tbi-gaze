﻿using GazeHelper;

/// <summary>
/// Simple tool to restore the mousepointer
/// </summary>
namespace ShowMouse
{
    static class ShowMouse
    {

        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        static void Main()
        {
            TrackerLogger logger = new TrackerLogger();
            JsonConfigParser parser = new JsonConfigParser(logger);
            ConfigItem config = parser.ParseJsonConfig();
            MouseHider hider = new MouseHider(logger);
            hider.ShowCursor(config.StandardMouseIconPath);
        }
    }
}
