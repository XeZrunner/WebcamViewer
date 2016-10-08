using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebcamViewer
{
    class Debug
    {
        public void Log(string text)
        {
            if (Properties.Settings.Default.app_logging)
            {
                using (StreamWriter file = new StreamWriter(Environment.CurrentDirectory + @"\log.txt", true)) // make sure we append;
                {
                    file.WriteLine(DateTime.Now + " | " + text);
                }
            }
        }
    }
}
