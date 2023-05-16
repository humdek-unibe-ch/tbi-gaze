using System.Windows;

namespace GazeUtilityLibrary
{
    /// <summary>
    /// Interaction logic for TrackerMessageBox.xaml
    /// </summary>
    public partial class TrackerMessageBox : Window
    {
        public TrackerMessageBox()
        {
            InitializeComponent();
        }

        private void Abort_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
