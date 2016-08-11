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
    public partial class settingsPage_NormalButton : UserControl
    {
        public settingsPage_NormalButton()
        {
            InitializeComponent();

            s = (Storyboard)FindResource("longMouseDownAnimation");
            grid.Width = usercontrol.Width * 2;
            grid.Height = usercontrol.Height * 2;
            grid.Margin = new Thickness(-usercontrol.ActualWidth / 2, -usercontrol.ActualHeight / 2, -usercontrol.ActualWidth / 2, -usercontrol.ActualHeight / 2);
        }

        public event RoutedEventHandler Click;

        [Description("Text of the button"), Category("Appearance")]
        public string Text
        {
            get { return textTextBlock.Text; }
            set { textTextBlock.Text = value; }
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

                translateX = e.MouseDevice.GetPosition(button).X - button.ActualWidth / 2;
                translateY = e.MouseDevice.GetPosition(button).Y - button.ActualHeight ;

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
                myTranslate.X = Mouse.GetPosition(button).X - button.ActualWidth / 2;
                myTranslate.Y = Mouse.GetPosition(button).Y - button.ActualHeight;

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
                translateX = e.MouseDevice.GetPosition(button).X - button.ActualWidth / 2;
                translateY = e.MouseDevice.GetPosition(button).Y - button.ActualHeight;

                if ((translateX >= (button.ActualWidth / 2) || translateX <= -(button.ActualWidth / 2)) || (translateY >= (button.ActualHeight) || translateY <= -(button.ActualHeight)))
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
