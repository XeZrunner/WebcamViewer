using System;
using System.Collections.Generic;
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

namespace WebcamViewer.Pages.Settings_page.Subpages._3_About_and_updates.Controls
{
    /// <summary>
    /// Interaction logic for Updates_Control.xaml
    /// </summary>
    public partial class Updates_Control : UserControl
    {
        public Updates_Control()
        {
            InitializeComponent();
        }

        Updates.UpdatesEngine updatesEngine = new Updates.UpdatesEngine();

        private void debug_TestProgressButton_Click(object sender, RoutedEventArgs e)
        {
            debugPanel.Visibility = Visibility.Collapsed;
            progressPanel.Visibility = Visibility.Visible;

            debugLabel.Visibility = Visibility.Visible;
        }

        private void debug_TestUpdateOverviewButton_Click(object sender, RoutedEventArgs e)
        {
            debugPanel.Visibility = Visibility.Collapsed;
            updateoverviewPanel.Visibility = Visibility.Visible;

            debugLabel.Visibility = Visibility.Visible;
        }

        private void debug_TestInstallConfirmDialogButton_Click(object sender, RoutedEventArgs e)
        {
            Popups.MessageDialog dialog = new Popups.MessageDialog()
            {
                Title = "The update null is ready to install...",
                Content = "The update is %f %s in size. It is already downloaded, installation shouldn't take long.\n\n" +
                "The application will close and a command prompt window will appear, showing the progress of the installation.\n\n" +
                "Would you like to continue with the installation?",
                FirstButtonContent = "Yes, install update",
                SecondButtonContent = "Not yet"
            };

            if (dialog.ShowDialogWithResult() == 0)
            {
                // clicked install
            }
            else
            {
                // clicked not yet
            }
        }

        private void Label_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            progressPanel.Visibility = Visibility.Collapsed;
            updateoverviewPanel.Visibility = Visibility.Collapsed;
            debugPanel.Visibility = Visibility.Visible;

            debugLabel.Visibility = Visibility.Collapsed;
        }

        private void debug_DebugInfoButton_Click(object sender, RoutedEventArgs e)
        {
            Popups.ContentDialog dlg = new Popups.ContentDialog();
            dlg.Title = "";
            dlg.Text =
                "Update Debug Info\n" +
                "-------------------------\n" +
                "\nUpdate available: " +
                "\nVersion available: " +
                "\nOn channel: " +
                "\n" +
                "-------------------------\n" +
                "Update plugin version: ";
            dlg.Button0_Text = "Close";

            dlg.ShowDialog();
        }
    }
}
