using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace WebcamViewer.User_controls
{
    public partial class webcamPage_Overview_CameraButton : UserControl
    {
        public webcamPage_Overview_CameraButton()
        {
            InitializeComponent();

            s = (Storyboard)FindResource("longMouseDownAnimation");
        }

        private void usercontrol_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            grid.Width = this.ActualWidth * 2;
            grid.Margin = new Thickness( -(this.ActualWidth / 2) );
        }

        private void image_SourceUpdated(object sender, System.Windows.Data.DataTransferEventArgs e)
        {
            double finalSize = (image.Source.Width + image.Source.Height / 8) + 30;
            this.Width = finalSize;
            this.Height = finalSize;
        }

        public event RoutedEventHandler Click;

        [Description("The Source property of the Image of the button"), Category("Common")]
        public ImageSource Image
        {
            get { return image.Source; }
            set { image.Source = value; }
        }

        [Description("The name of the camera"), Category("Common")]
        public string CameraName
        {
            get { return textblock.Text; }
            set { textblock.Text = value; }
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
                translateY = e.MouseDevice.GetPosition(button).Y - this.ActualHeight / 2;

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
                myTranslate.Y = Mouse.GetPosition(button).Y - this.ActualHeight / 2;

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
                translateY = e.MouseDevice.GetPosition(button).Y - this.ActualHeight / 2;

                if ( (translateX >= (this.ActualWidth / 2) || translateX <= -(this.ActualWidth / 2)) || (translateY >= this.ActualHeight / 2 || translateY <= -(this.ActualHeight / 2)) )
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
