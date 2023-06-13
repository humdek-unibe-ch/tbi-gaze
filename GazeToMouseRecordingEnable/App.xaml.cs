using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO.Pipes;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using GazeUtilityLibrary;

namespace GazeToMouseRecordingEnable
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            TrackerLogger logger = new TrackerLogger();
            NamedPipeClient.SendSignal("GAZE_RECORDING_ENABLE", logger);
            Current.Shutdown();
        }
    }
}
