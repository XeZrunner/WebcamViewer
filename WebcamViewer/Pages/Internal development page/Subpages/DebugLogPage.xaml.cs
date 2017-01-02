using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WebcamViewer.Pages.Internal_development_page.Subpages
{
    public partial class DebugLogPage
    {
        public DebugLogPage()
        {
            InitializeComponent();
        }

        Debug Debug = new Debug();

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private async void page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.IsVisible == true)
            {
                if (Properties.Settings.Default.app_logging)
                {
                    await ReadLog();
                }
            }
        }

        private async Task ReadLog()
        {
            textblock.Text = ""; // clear
            try
            {
                using (StreamReader reader = new StreamReader(Environment.CurrentDirectory + @"\log.txt"))
                {
                    progressring.Visibility = Visibility.Visible; progressring.IsActive = true;
                    textblock.Text = await reader.ReadToEndAsync();
                    progressring.Visibility = Visibility.Collapsed; progressring.IsActive = false;
                }
            }
            catch
            {
                textblock.Text = "Failed to read log file";
            }
        }

        private void deleteLogFileButton_Click(object sender, RoutedEventArgs e)
        {
            Popups.MessageDialog dlg = new Popups.MessageDialog();
            dlg.Title = "Delete log file";
            dlg.Content = "Are you sure you want to delete the log file?";
            dlg.FirstButtonContent = "Sure!";
            dlg.SecondButtonContent = "Nope...";

            if (dlg.ShowDialogWithResult() == 0)
            {
                File.Delete(Environment.CurrentDirectory + @"\log.txt");
                textblock.Text = "Log file deleted";
            }
        }

        private async void toggleLoggingButton_Click(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.app_logging)
            {
                Debug.Log("Logging disabled.");
            }
            else
            {
                Debug.Log("Logging enabled.");
            }

            await ReadLog();

            Properties.Settings.Default.app_logging = !Properties.Settings.Default.app_logging;
            Properties.Settings.Default.Save();
        }

        private async void testLogEntryButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.Log("TEST");
            await ReadLog();
        }

        private async void refreshButton_Click(object sender, RoutedEventArgs e)
        {
            await ReadLog();
        }
    }
}
