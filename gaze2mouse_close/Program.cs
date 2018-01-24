using System.Diagnostics;
using System.Windows.Forms;

namespace gaze2mouse_close
{
    static class Program
    {
        static void Main()
        {
            Cursor.Show();
            Process.Start("taskkill", "/IM gaze2mouse.exe");
        }
    }
}
