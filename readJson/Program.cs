using System;
using System.IO;
using Newtonsoft.Json;
using System.Windows.Forms;

namespace readJson
{
    static class Program
    {
        public class Item
        {
            public string Outfile { get; set; }
        }

        static void Main(string[] args)
        {
            StreamReader r = new StreamReader("config.json");
            string json = r.ReadToEnd();
            Item item = JsonConvert.DeserializeObject<Item>(json);
            MessageBox.Show("Path: " + item.Outfile);
        }
    }
}