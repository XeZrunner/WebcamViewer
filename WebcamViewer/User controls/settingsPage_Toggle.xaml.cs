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
    public partial class settingsPage_Toggle : UserControl
    {
        public settingsPage_Toggle()
        {
            InitializeComponent();

            s = (Storyboard)FindResource("longMouseDownAnimation");
        }

        bool _IsActive;

        [Description("Active state of the toggle"), Category("Common")]
        public bool IsActive
        {
            get { return _IsActive; }
            set
            {
                _IsActive = value;

                if (_IsActive == true)
                {
                    // animation
                    Storyboard s_ToggleIn = (Storyboard)FindResource("Toggle_In");
                    s_ToggleIn.Begin();

                    // target brush
                    Rectangle temp_rec = new Rectangle();
                    temp_rec.SetResourceReference(Rectangle.FillProperty, "accentcolor_dark");

                    // original foregroundsecondary brush
                    Rectangle original_rec = new Rectangle();
                    original_rec.SetResourceReference(Rectangle.FillProperty, "settingsPage_foregroundSecondary");

                    // settings background brush
                    Rectangle settingsbackground_rec = new Rectangle();
                    settingsbackground_rec.SetResourceReference(Rectangle.FillProperty, "settingsPage_background");

                    // remove Frozen™
                    border.Background = settingsbackground_rec.Fill;
                    border.BorderBrush = original_rec.Fill;
                    ellipse.Fill = settingsbackground_rec.Fill;

                    // set!
                    border.Background = temp_rec.Fill;
                    border.BorderBrush = temp_rec.Fill;
                    ellipse.Fill = Brushes.White;

                    // animate!
                    if (!DoAnimation)
                        s_ToggleIn.SkipToFill();
                }
                else
                {
                    // animation
                    Storyboard s_ToggleOut = (Storyboard)FindResource("Toggle_Out");
                    s_ToggleOut.Begin();

                    // target brush
                    Rectangle temp_rec = new Rectangle();
                    temp_rec.SetResourceReference(Rectangle.FillProperty, "settingsPage_foregroundSecondary");

                    // original foregroundsecondary brush
                    Rectangle original_rec = new Rectangle();
                    original_rec.SetResourceReference(Rectangle.FillProperty, "accentcolor_dark");

                    // settings background brush
                    Rectangle settingsbackground_rec = new Rectangle();
                    settingsbackground_rec.SetResourceReference(Rectangle.FillProperty, "settingsPage_background");

                    // remove Frozen™
                    border.Background = settingsbackground_rec.Fill;
                    border.BorderBrush = original_rec.Fill;
                    ellipse.Fill = original_rec.Fill;

                    // set!
                    border.Background = settingsbackground_rec.Fill;
                    border.BorderBrush = temp_rec.Fill;
                    ellipse.Fill = temp_rec.Fill;

                    // animate!
                    if (!DoAnimation)
                        s_ToggleOut.SkipToFill();
                }
                DoAnimation = false;
            }
        }

        /// <summary>
        /// Whether to play the animation of the toggle
        /// </summary>
        public bool DoAnimation = false;

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (!doneDownAnim)
            {
                TranslateTransform myTranslate = new TranslateTransform();
                myTranslate.X = Mouse.GetPosition(this).X - this.ActualWidth / 2;
                myTranslate.Y = Mouse.GetPosition(this).Y - this.ActualHeight / 2;

                ellipse_Ripple.RenderTransform = myTranslate;

                s.SpeedRatio = 3;
                s.Begin();

                //contentGrid_DownRectangle.Visibility = Visibility.Visible;
            }

            DoAnimation = true;
            if ((string)this.Tag != "ToggleSwitchButton")
            {
                this.IsActive = !this.IsActive;
            }
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            grid.Width = this.ActualWidth * 2;
            grid.Height = this.ActualHeight * 2;

            FrameworkElement _parent = Parent as FrameworkElement;
            this.MaxWidth = _parent.ActualWidth;

            //grid.Margin = new Thickness(-this.ActualWidth / 2, -this.ActualHeight / 2, -this.ActualWidth / 2, -this.ActualHeight / 2);

            var s_WidthHeight = (EasingDoubleKeyFrame)this.Resources["s_WidthHeightKeyFrame"];
            var s_Margin = (EasingThicknessKeyFrame)this.Resources["s_MarginKeyFrame"];

            s_WidthHeight.Value = this.ActualWidth * 2;
            s_Margin.Value = new Thickness(-this.ActualWidth);
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

                translateX = e.MouseDevice.GetPosition(this).X - this.ActualWidth / 2;
                translateY = e.MouseDevice.GetPosition(this).Y - this.ActualHeight / 2;

                TranslateTransform myTranslate = new TranslateTransform();
                myTranslate.X = translateX;
                myTranslate.Y = translateY;

                ellipse_Ripple.RenderTransform = myTranslate;

                s.SpeedRatio = 0.5;
                s.Begin();

                doneDownAnim = true;
            };
            LongDowntimer.Start();

            //contentGrid_DownRectangle.Visibility = Visibility.Visible;
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

                //contentGrid_DownRectangle.Visibility = Visibility.Hidden;
            };
            timer.Start();
        }

        private void button_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                translateX = e.MouseDevice.GetPosition(this).X - this.ActualWidth / 2;
                translateY = e.MouseDevice.GetPosition(this).Y - this.ActualHeight / 2;

                if ((translateX >= this.ActualWidth / 2 || translateX <= -this.ActualWidth / 2) || (translateY >= this.ActualHeight / 2 || translateY <= -this.ActualHeight / 2))
                {
                    s.SetSpeedRatio(4);

                    //contentGrid_DownRectangle.Visibility = Visibility.Hidden;

                    return;
                }

                TranslateTransform myTranslate = new TranslateTransform();
                myTranslate.X = translateX;
                myTranslate.Y = translateY;

                ellipse_Ripple.RenderTransform = myTranslate;
            }
        }
    }
}
