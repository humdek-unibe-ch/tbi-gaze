﻿using System;
using System.IO.Pipes;
using System.IO;
using System.Windows;
using GazeUtilityLibrary;

namespace GazeToMouseClose
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            NamedPipeClient.SendSignal("TERMINATE");
            Current.Shutdown();
        }
    }
}
