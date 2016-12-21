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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WebcamViewer.Pages.Settings_page
{
    public partial class Page
    {
        public Page()
        {
            InitializeComponent();

            SubViewIn_Anim = (Storyboard)FindResource("SubViewIn");
            SubViewOut_Anim = (Storyboard)FindResource("SubViewOut");
        }

        Storyboard SubViewIn_Anim;
        Storyboard SubViewOut_Anim;

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width <= 482)
            {
                view_mobilegrid.Visibility = Visibility.Visible;
                view_desktopgrid.Visibility = Visibility.Collapsed;
            }
            else
            {
                view_mobilegrid.Visibility = Visibility.Collapsed;
                view_desktopgrid.Visibility = Visibility.Visible;
            }
        }

        private void webcameditorTile_Click(object sender, RoutedEventArgs e)
        {
            view_subpagegrid_activeSubpageLabel.Content = "WEBCAM EDITOR";
            view_desktop_subpagegrid_Frame.Content = new Pages.Settings_page.Subpages._0_Webcams.Page();

            SubViewIn_Anim.Begin();
        }

        private void camerasettingsTile_Click(object sender, RoutedEventArgs e)
        {
            SubViewIn_Anim.Begin();

            view_subpagegrid_activeSubpageLabel.Content = "CAMERA SETTINGS";
            view_desktop_subpagegrid_Frame.Content = new Pages.Settings_page.Subpages._1_Home.Page();
        }

        private void userinterfaceTile_Click(object sender, RoutedEventArgs e)
        {
            SubViewIn_Anim.Begin();

            view_subpagegrid_activeSubpageLabel.Content = "USER INTERFACE";
            view_desktop_subpagegrid_Frame.Content = new Pages.Settings_page.Subpages._2_User_Interface.Page();
        }

        private void aboutTile_Click(object sender, RoutedEventArgs e)
        {
            SubViewIn_Anim.Begin();

            view_subpagegrid_activeSubpageLabel.Content = "ABOUT & UPDATES";
            view_desktop_subpagegrid_Frame.Content = new Pages.Settings_page.Subpages._3_About_and_updates.Page();
        }

        private void rippledebugTile_Click(object sender, RoutedEventArgs e)
        {
            SubViewIn_Anim.Begin();

            view_subpagegrid_activeSubpageLabel.Content = "";

            XeZrunner.UI.ControlEffects.RippleDrawable rdrawable = new XeZrunner.UI.ControlEffects.RippleDrawable();
            rdrawable.SetResourceReference(XeZrunner.UI.ControlEffects.RippleDrawable.FillColorProperty, "settingsPage_foregroundSecondary3");
            rdrawable.SetResourceReference(XeZrunner.UI.ControlEffects.RippleDrawable.ColorProperty, "accentcolor_dark");
            view_desktop_subpagegrid_Frame.Content = rdrawable;

            view_desktop_subpagegrid_Frame.Content = rdrawable;
        }

        private void internalsettingsTile_Click(object sender, RoutedEventArgs e)
        {
            SubViewIn_Anim.Begin();

            view_subpagegrid_activeSubpageLabel.Content = "INTERNAL SETTINGS";
            //view_subpagegrid_ActionBarRow.Height = new GridLength(0);
            Pages.Internal_development_page.Page page = new Internal_development_page.Page();
            page.titlebaraccentRectangle.Visibility = Visibility.Collapsed; // remove hardcoded titlebar ribbon
            view_desktop_subpagegrid_Frame.Content = page;
        }

        private void view_subpagegrid_backButton_Click(object sender, RoutedEventArgs e)
        {
            SubViewOut_Anim.Begin();
        }

        private void mobileViewButton_Click(object sender, RoutedEventArgs e)
        {
            view_desktopgrid.Visibility = Visibility.Collapsed;
            view_mobilegrid.Visibility = Visibility.Visible;
        }

        private void desktopViewButton_Click(object sender, RoutedEventArgs e)
        {
            view_desktopgrid.Visibility = Visibility.Visible;
            view_mobilegrid.Visibility = Visibility.Collapsed;
        }
    }
}
