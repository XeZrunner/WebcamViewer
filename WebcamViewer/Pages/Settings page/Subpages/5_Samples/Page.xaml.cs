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

namespace WebcamViewer.Pages.Settings_page.Subpages._5_Samples
{
    public partial class Page
    {
        public Page()
        {
            InitializeComponent();
        }


        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await AsyncWaitFor(TimeSpan.FromSeconds(.3));
            menu_control.SetActive(0); // grab first page
        }

        private async Task AsyncWaitFor(TimeSpan timeToWait)
        {
            await Task.Delay(timeToWait);
        }

        private void PageSideMenuNavigationControl_SelectionChanged(object sender, EventArgs e)
        {
            User_controls.settingsPage_TabButton sBtn = sender as User_controls.settingsPage_TabButton;

            int id_destination = (int)sBtn.Tag;

            foreach (Grid page in pageContentGrid.Children)
            {
                int pageID = int.Parse((string)page.Tag);

                if (pageID == id_destination)
                    page.Visibility = Visibility.Visible;
                else
                    page.Visibility = Visibility.Collapsed;
            }
        }
    }
}
