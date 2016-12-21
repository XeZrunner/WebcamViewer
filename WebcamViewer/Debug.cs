using System;
using System.IO;

namespace WebcamViewer.Updates
{
    class Debug
    {
        /// <summary>
        /// Logs to the app's directory into log.txt, but only if logging is enabled.
        /// </summary>
        /// <param name="text">The text to log into the log.</param>
        public void Log(string text)
        {
            if (Properties.Settings.Default.app_logging)
            {
                using (StreamWriter file = new StreamWriter(Environment.CurrentDirectory + @"\log.txt", true)) // make sure we append;
                    file.WriteLine(DateTime.Now + " | " + text);
            }
        }
    }
}
