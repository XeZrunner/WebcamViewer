using System.Windows;
using System.Windows.Controls;

namespace WebcamViewer.Updates.Updates.Pages.Settings_page.Subpages._1_Home.Controls
{
    public partial class settingsPage_HomePage_ImageSizingDialogControl : UserControl
    {
        public settingsPage_HomePage_ImageSizingDialogControl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (RadioButton btn in mainStackPanel.Children)
            {
                if ((string)btn.Tag == Properties.Settings.Default.home_imagesizing)
                    btn.IsChecked = true;

            }
        }
    }
}
