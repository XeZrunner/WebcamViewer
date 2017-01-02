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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WebcamViewer
{
    /// <summary>
    /// Interaction logic for Video_RippleTestWindow.xaml
    /// </summary>
    public partial class Video_RippleTestWindow : Window
    {
        public Video_RippleTestWindow()
        {
            InitializeComponent();

            plainrippleButton_Click(this, new RoutedEventArgs());

            menuGrid.Width = 48;
            dim.Visibility = Visibility.Hidden;
        }

        private void changeripplecolorButton_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void AppBar_Button_Click(object sender, RoutedEventArgs e)
        {
            if (menuGrid.Width == 48)
            {
                DoubleAnimation anim = new DoubleAnimation(280, TimeSpan.FromSeconds(.2));
                menuGrid.BeginAnimation(WidthProperty, anim);

                dim.Visibility = Visibility.Visible;
            }
            else
            {
                DoubleAnimation anim = new DoubleAnimation(48, TimeSpan.FromSeconds(.2));
                menuGrid.BeginAnimation(WidthProperty, anim);

                dim.Visibility = Visibility.Hidden;
            }
        }

        private void plainrippleButton_Click(object sender, RoutedEventArgs e)
        {
            plainrippleButton.IsActive = true;
            usercontrolsButton.IsActive = false;
            ellipseButton.IsActive = false;

            plainRipplePage.Visibility = Visibility.Visible;
            controlsPage.Visibility = Visibility.Collapsed;
            ellipsePage.Visibility = Visibility.Collapsed;

            AppBar_Button_Click(this, new RoutedEventArgs());
        }

        bool firsttime = true;

        private void usercontrolsButton_Click(object sender, RoutedEventArgs e)
        {
            plainrippleButton.IsActive = false;
            usercontrolsButton.IsActive = true;
            ellipseButton.IsActive = false;

            plainRipplePage.Visibility = Visibility.Collapsed;
            controlsPage.Visibility = Visibility.Visible;
            ellipsePage.Visibility = Visibility.Collapsed;

            if (firsttime)
            {
                controlsPage_mainStackPanel.Visibility = Visibility.Collapsed;
                controlsPage_ArcProgress.Visibility = Visibility.Visible; controlsPage_ArcProgress.IsActive = true;

                DispatcherTimer timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(2);
                timer.Tick += (s, ev) => { timer.Stop(); controlsPage_ArcProgress.Visibility = Visibility.Collapsed; controlsPage_mainStackPanel.Visibility = Visibility.Visible; };
                timer.Start();
            }

            AppBar_Button_Click(this, new RoutedEventArgs());
        }

        private void ellipseButton_Click(object sender, RoutedEventArgs e)
        {
            plainrippleButton.IsActive = false;
            usercontrolsButton.IsActive = false;
            ellipseButton.IsActive = true;

            plainRipplePage.Visibility = Visibility.Collapsed;
            controlsPage.Visibility = Visibility.Collapsed;
            ellipsePage.Visibility = Visibility.Visible;

            AppBar_Button_Click(this, new RoutedEventArgs());
        }
    }
}
