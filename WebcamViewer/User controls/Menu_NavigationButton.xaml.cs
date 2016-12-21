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
    public partial class Menu_NavigationButton : UserControl
    {
        public Menu_NavigationButton()
        {
            InitializeComponent();

            s = (Storyboard)FindResource("longMouseDownAnimation");
        }

        private void usercontrol_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            grid.Width = this.ActualWidth * 2;
        }

        public event RoutedEventHandler Click;

        private bool _IsActive = false;

        [Description("The icon of the button"), Category("Common")]
        public string IconText
        {
            get { return iconTextBlock.Text; }
            set { iconTextBlock.Text = value; }
        }

        [Description("The text of the button"), Category("Common")]
        public string Text
        {
            get { return textTextBlock.Text; }
            set { textTextBlock.Text = value; }
        }

        [Description("Active state of the button"), Category("Common")]
        public bool IsActive
        {
            get { return _IsActive; }
            set
            {
                _IsActive = value;

                if (_IsActive)
                    rectangle.Visibility = Visibility.Visible;
                else
                    rectangle.Visibility = Visibility.Hidden;
            }
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

                translateX = e.MouseDevice.GetPosition(button).X - this.ActualWidth / 2;
                translateY = e.MouseDevice.GetPosition(button).Y - 24;

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
                myTranslate.X = Mouse.GetPosition(button).X - this.ActualWidth / 2;
                myTranslate.Y = Mouse.GetPosition(button).Y - 24;

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
                translateX = e.MouseDevice.GetPosition(button).X - this.ActualWidth / 2;
                translateY = e.MouseDevice.GetPosition(button).Y - 24;

                if ((translateX >= (this.ActualWidth / 2) || translateX <= -(this.ActualWidth / 2)) || (translateY >= 24 || translateY <= -24))
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
