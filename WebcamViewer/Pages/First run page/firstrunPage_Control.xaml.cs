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
using System.Windows.Threading;

namespace WebcamViewer.Pages.First_run_page
{
    /// <summary>
    /// Interaction logic for firstrunPage_UserControl.xaml
    /// </summary>
    public partial class firstrunPage_Control : UserControl
    {
        public firstrunPage_Control()
        {
            InitializeComponent();
        }

        private void WelcomePage_CommandBar_NextButton_Click(object sender, RoutedEventArgs e)
        {
            WelcomePage_CommandBar_ProgressPanel.Visibility = Visibility.Visible;

            DispatcherTimer timer1 = new DispatcherTimer();
            timer1.Interval = TimeSpan.FromSeconds(3);
            timer1.Tick += (s, ev) => 
            {
                timer1.Stop();

                WelcomePage_CommandBar_ProgressPanel.Visibility = Visibility.Collapsed;
                WelcomePage.Visibility = Visibility.Collapsed;

                AccentColorPage.Visibility = Visibility.Visible;
            };
            timer1.Start();
        }

        private void AccentColorPage_CommandBar_NextButton_Click(object sender, RoutedEventArgs e)
        {
            AccentColorPage_CommandBar_ProgressPanel.Visibility = Visibility.Visible;

            DispatcherTimer timer1 = new DispatcherTimer();
            timer1.Interval = TimeSpan.FromSeconds(1);
            timer1.Tick += (s, ev) =>
            {
                timer1.Stop();

                WelcomePage_CommandBar_ProgressPanel.Visibility = Visibility.Collapsed;

                DoubleAnimation anim1 = new DoubleAnimation(0, TimeSpan.FromSeconds(.5));
                AccentColorPage.BeginAnimation(OpacityProperty, anim1);

                DispatcherTimer timer_opa1 = new DispatcherTimer();
                timer_opa1.Interval = TimeSpan.FromSeconds(.5);
                timer_opa1.Tick += (s1, ev1) => { timer1.Stop(); AccentColorPage.Visibility = Visibility.Collapsed; };

                Transition_ProgressPage.Visibility = Visibility.Visible; Transition_ProgressPage.Opacity = 0;

                DoubleAnimation anim2 = new DoubleAnimation(1, TimeSpan.FromSeconds(.5));
                Transition_ProgressPage.BeginAnimation(OpacityProperty, anim2);
            };
            timer1.Start();
        }
    }
}
