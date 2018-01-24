/**
 * 
 */

using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Tobii.Interaction;
using Tobii.Interaction.Framework;

namespace gazeDataStream
{
    class Program
    {
        private static FileStream fs;
        private static StreamWriter sw;
        private static TimeSpan ts_start;
        private static TimeSpan ts_delta;
        private static bool hasRun = false;
        private static Host host;
        
        private class TestMessageFilter : IMessageFilter
        {
            public bool PreFilterMessage(ref Message m)
            {
                if (m.Msg == /*WM_CLOSE*/ 0x10)
                {
                    Application.Exit();
                    return true;
                }
                return false;
            }
        }
        
        static void Main(string[] args)
        {
            // open files
            fs = File.Open("cursor.txt", FileMode.Create);
            sw = new StreamWriter(fs);
            ts_start = DateTime.Now.TimeOfDay;
            sw.WriteLine("Timestamp; X; Y;");

            Application.ApplicationExit += new EventHandler(OnApplicationExit);

            // initialize host. Make sure that the Tobii service is running
            host = new Host();

            // create stream
            var gazePointDataStream = host.Streams.CreateGazePointDataStream(GazePointDataMode.Unfiltered);
            gazePointDataStream.GazePoint((x, y, ts) => gaze2mouse( x, y, ts ));

            Application.AddMessageFilter(new TestMessageFilter());
            Application.Run();
        }

        static void gaze2mouse(double x, double y, double ts)
        {
            TimeSpan ts_rec = TimeSpan.FromMilliseconds(ts);
            if (!hasRun) ts_delta = ts_rec - ts_start;
            ts_rec -= ts_delta;
            Cursor.Position = new System.Drawing.Point( Convert.ToInt32(x), Convert.ToInt32(y) );
            sw.WriteLine("{0}\t{1:0.0}\t{2:0.0}", ts_rec, x, y);
            hasRun = true;
        }

        static void OnApplicationExit(object sender, EventArgs e)
        {
            sw.WriteLine("Programm was terminated");
            sw.Close();
            fs.Close();
            sw.Dispose();
            fs.Dispose();
            host.DisableConnection();
        }
    }
}
