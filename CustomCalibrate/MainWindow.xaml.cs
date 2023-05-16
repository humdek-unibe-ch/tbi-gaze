using GazeUtilityLibrary;
using System;
using System.Windows;

namespace CustomCalibrate
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            TrackerLogger logger = new TrackerLogger();
            logger.Info($"Starting \"{AppDomain.CurrentDomain.BaseDirectory}CustomCalibrate.exe\"");
            Main.Content = new CustomCalibrationLibrary.Views.CalibrationCollection(logger);
        }
    }
}
