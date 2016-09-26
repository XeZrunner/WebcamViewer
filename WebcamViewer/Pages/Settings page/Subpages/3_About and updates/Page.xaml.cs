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
using WebcamViewer.Pages.Settings_page.Subpages._2_User_Interface;

namespace WebcamViewer.Pages.Settings_page.Subpages._3_About_and_updates
{
    public partial class Page : UserControl
    {
        public Page()
        {
            InitializeComponent();
        }

        #region Dialogs

        /// <summary>
        /// Displays a Windows 10 UWP ContentDialog-style text message dialog.
        /// </summary>
        /// <param name="Title">The title of the dialog</param>
        /// <param name="Content">The main text of the dialog</param>
        /// <param name="DarkMode">Determintes whether to style the window light or dark. (0 = dark, 1 = light, null = automatic from theme)</param>
        void TextMessageDialog(string Title, string Content, bool? DarkMode = null)
        {
            Popups.MessageDialog dlg = new Popups.MessageDialog();

            dlg.Title = Title;
            dlg.Content = Content;

            dlg.IsDarkTheme = DarkMode;

            dlg.ShowDialog();
        }

        /// <summary>
        /// Displays a Windows 8.x MessageDialog-style text message dialog.
        /// </summary>
        /// <param name="Title">The title of the dialog</param>
        /// <param name="Content">The main text of the dialog</param>
        /// <param name="DarkMode">Determintes whether to style the window light or dark. (0 = dark, 1 = light, null = automatic from theme)</param>
        void TextMessageDialog_FullWidth(string Title, string Content, bool? DarkMode = null)
        {
            Popups.MessageDialog_FullWidth dlg = new Popups.MessageDialog_FullWidth();

            dlg.Title = Title;
            dlg.Content = Content;

            dlg.IsDarkTheme = DarkMode;

            dlg.ShowDialog();
        }

        #endregion

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
            settingsPage_AboutPage_DebugVersionInfoGrid_WebcamEngineVersionTextBlock.Text = String.Format("Webcam engine version: {0}", Properties.Settings.Default.webcamengine_version);
        }

        private void page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            stackpanel.MaxWidth = this.ActualWidth - 20;
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

            dialog.Content = new Pages.Settings_page.Subpages._3_About_and_updates.Controls.settingsPage_AboutPage_CreditsControl();

            dialog.Content_DisableMargin = true;

            dialog.FirstButtonContent = "Close";

            dialog.ShowDialog();
        }

        private void settingsPage_AboutPage_ChangelogButton_Click(object sender, RoutedEventArgs e)
        {
            TextMessageDialog_FullWidth(res.Resources.Changelog, Properties.Settings.Default.changelog);
        }
    }
}
