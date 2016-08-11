using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace WebcamViewer.User_controls
{
    public partial class settingsPage_TabButton : UserControl
    {
        public settingsPage_TabButton()
        {
            InitializeComponent();

            s = (Storyboard)FindResource("longMouseDownAnimation");
        }

        public event RoutedEventHandler Click;

        bool _IsActive = false;

        [Description("Icon of the button"), Category("Appearance")]
        public string IconText
        {
            get { return iconTextBlock.Text; }
            set { iconTextBlock.Text = value; }
        }

        [Description("Title text of the button"), Category("Appearance")]
        public string Title
        {
            get { return titleTextBlock.Text; }
            set { titleTextBlock.Text = value; }
        }

        [Description("Description text of the button"), Category("Appearance")]
        public string Description
        {
            get { return descriptionTextBlock.Text; }
            set { descriptionTextBlock.Text = value; }
        }

        [Description("Active state of the button"), Category("Appearance")]
        public bool IsActive
        {
            get { return _IsActive; }
            set
            { _IsActive = value;

                if (_IsActive == true)
                {
                    activestateRectangle.Visibility = Visibility.Visible;
                    iconTextBlock.Foreground = Application.Current.Resources["accentcolor_dark"] as SolidColorBrush;
                    titleTextBlock.Foreground = Application.Current.Resources["accentcolor_dark"] as SolidColorBrush;
                    descriptionTextBlock.Foreground = Application.Current.Resources["accentcolor_dark"] as SolidColorBrush;
                }
                else
                {
                    activestateRectangle.Visibility = Visibility.Hidden;
                    iconTextBlock.Foreground = Application.Current.Resources["settingsPage_foregroundText"] as SolidColorBrush;
                    titleTextBlock.Foreground = Application.Current.Resources["settingsPage_foregroundText"] as SolidColorBrush;
                    descriptionTextBlock.Foreground = Application.Current.Resources["settingsPage_foregroundSecondary"] as SolidColorBrush;
                }
            }
        }

        [Description("Ripple brush"), Category("Appearance")]
        public Brush RippleBrush
        {
            get { return ellipse.Fill; }
            set { ellipse.Fill = value; }
        }

        DispatcherTimer LongDowntimer = new DispatcherTimer();

        Storyboard s;

        double translateX;
        double translateY;

        bool doneDownAnim;

        private void Button_MouseDown(object sender, MouseButtonEventArgs e)
        {

            LongDowntimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            LongDowntimer.Tick += (s1, ev) =>
            {
                LongDowntimer.Stop();

                translateX = e.MouseDevice.GetPosition(button).X - 145;
                translateY = e.MouseDevice.GetPosition(button).Y - 30;

                TranslateTransform myTranslate = new TranslateTransform();
                myTranslate.X = translateX;
                myTranslate.Y = translateY;

                ellipse.RenderTransform = myTranslate;

                s.SpeedRatio = 0.5;
                s.Begin();

                doneDownAnim = true;
            };
            LongDowntimer.Start();
        }

        private void button_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            LongDowntimer.Stop();

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            timer.Tick += (s1, ev) =>
            {
                timer.Stop();
                if (doneDownAnim == true)
                    s.SetSpeedRatio(3);
                doneDownAnim = false;
            };
            timer.Start();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (!doneDownAnim)
            {
                TranslateTransform myTranslate = new TranslateTransform();
                myTranslate.X = Mouse.GetPosition(button).X - 145;
                myTranslate.Y = Mouse.GetPosition(button).Y - 30;

                ellipse.RenderTransform = myTranslate;

                s.SpeedRatio = 3;
                s.Begin();
            }

            //bubble the event up to the parent
            if (this.Click != null)
                this.Click(this, e);

        }

        private void button_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                translateX = e.MouseDevice.GetPosition(button).X - 145;
                translateY = e.MouseDevice.GetPosition(button).Y - 30;

                if ((translateX >= 145 || translateX <= -145) || (translateY >= 30 || translateY <= -30))
                {
                    s.SetSpeedRatio(4); return;
                }

                TranslateTransform myTranslate = new TranslateTransform();
                myTranslate.X = translateX;
                myTranslate.Y = translateY;

                ellipse.RenderTransform = myTranslate;
            }
        }
    }
}
