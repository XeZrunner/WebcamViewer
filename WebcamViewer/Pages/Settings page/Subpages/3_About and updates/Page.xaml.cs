using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace WebcamViewer.Pages.Settings_page.Subpages._3_About_and_updates
{
    public partial class Page : UserControl
    {
        public Page()
        {
            InitializeComponent();
        }

        private void page_Loaded(object sender, RoutedEventArgs e)
        {
            #region Version number hackz
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fileversioninfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            string versionnumber;

            if (fileversioninfo.FileVersion.Substring(6, 1) == "0")
                versionnumber = fileversioninfo.FileVersion.Substring(0, 5);
            else
                versionnumber = fileversioninfo.FileVersion;
            #endregion

            // Main version labels (on the top of the page, or "Hero labels")
            // set settingsPage_AboutPage_ApplicationNameTextBlock.Text to localizated application name

            settingsPage_AboutPage_VersionNumberTextBlock.Text = versionnumber + " (" + Properties.Settings.Default.versionid + ")";

            // Debug version info
            settingsPage_AboutPage_DebugVersionInfoGrid_VersionNumberTextBlock.Text = String.Format("Version: {0}", versionnumber);
            settingsPage_AboutPage_DebugVersionInfoGrid_VersionNameTextBlock.Text = String.Format("Version name: {0}", Properties.Settings.Default.versionid);
            settingsPage_AboutPage_DebugVersionInfoGrid_BuildIDTextBlock.Text = String.Format("Build ID: {0}", Properties.Settings.Default.buildid);
        }

        private void settingsPage_AboutPage_GithubLink_Click(object sender, MouseButtonEventArgs e)
        {
            TextBlock textblock = sender as TextBlock;

            // Go to the project Github page
            Process.Start("https://" + textblock.Text);
        }

        private void settingsPage_AboutPage_CreditsButton_Click(object sender, RoutedEventArgs e)
        {
            Popups.MessageDialog dialog = new Popups.MessageDialog();

            dialog.Content = new Dialog_controls.settingsPage_AboutPage_CreditsControl();

            dialog.Content_DisableMargin = true;

            dialog.FirstButtonContent = "Close";

            dialog.ShowDialog();
        }
    }
}
