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
    public partial class settingsPage_ToggleSwitchButton : UserControl
    {
        public settingsPage_ToggleSwitchButton()
        {
            InitializeComponent();

            s = (Storyboard)FindResource("longMouseDownAnimation");
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            grid.Width = this.ActualWidth * 2;
            grid.Height = this.ActualHeight * 2;

            grid.Margin = new Thickness(-this.ActualWidth / 2, -this.ActualHeight / 2, -this.ActualWidth / 2, -this.ActualHeight / 2);

            var s_WidthHeight = (EasingDoubleKeyFrame)this.Resources["s_WidthHeightKeyFrame"];
            var s_Margin = (EasingThicknessKeyFrame)this.Resources["s_MarginKeyFrame"];

            s_WidthHeight.Value = this.ActualWidth * 2;
            s_Margin.Value = new Thickness(-this.ActualWidth);
        }

        bool _IsToggleButton = true;

        int? _Theme = null;

        public event RoutedEventHandler Click;

        [Description("Title of the button"), Category("Common")]
        public string Title
        {
            get { return titleTextBlock.Text; }
            set { titleTextBlock.Text = value.ToUpper(); }
        }

        [Description("Description of the button"), Category("Common")]
        public string Description
        {
            get { return descriptionTextBlock.Text; }
            set { descriptionTextBlock.Text = value; }
        }

        [Description("Active state of the toggle"), Category("Common")]
        public bool IsActive
        {
            get { return toggle.IsActive; }
            set { toggle.IsActive = value; }
        }

        [Description("Determines whether to display the toggle"), Category("Common")]
        public bool IsToggleButton
        {
            get { return _IsToggleButton; }
            set
            {
                _IsToggleButton = value;

                if (_IsToggleButton == true)
                {
                    toggle.Visibility = Visibility.Visible;
                    contentGrid_columnDefinition1.Width = new GridLength(100);
                }
                else
                {
                    toggle.Visibility = Visibility.Collapsed;
                    contentGrid_columnDefinition1.Width = new GridLength(0);
                }
            }
        }

        [Description("The grid on the right side (only works with IsToggleButton being false, and only with one Grid)"), Category("Common")]
        public Grid RightSideGrid
        {
            get { return (Grid)rightSide_GridContainer.Children[0]; }
            set
            {
                if (_IsToggleButton == false)
                {
                    if (value != null)
                    {
                        rightSide_GridContainer.Visibility = Visibility.Visible;

                        rightSide_GridContainer.Children.Clear();
                        rightSide_GridContainer.Children.Add(value);

                        contentGrid_columnDefinition1.Width = new GridLength(100);
                    }
                    else
                    {
                        rightSide_GridContainer.Visibility = Visibility.Collapsed;

                        rightSide_GridContainer.Children.Clear();

                        contentGrid_columnDefinition1.Width = new GridLength(0);
                    }
                }
            }
        }

        /// <summary>
        /// Force the theme of the button.
        /// 0 = light, 1 = dark
        /// </summary>
        [Description("Force the theme of the button"), Category("Appearance")]
        public int Theme
        {
            get { return _Theme.Value; }
            set
            {
                _Theme = value;

                if (_Theme == 0)
                {
                    // light theme
                    titleTextBlock.Foreground = Application.Current.Resources["settingsPage_Light_foregroundText"] as SolidColorBrush;
                    descriptionTextBlock.Foreground = Application.Current.Resources["settingsPage_Light_foregroundSecondary2"] as SolidColorBrush;
                    RippleBrush = Application.Current.Resources["settingsPage_Light_backgroundSecondary2"] as SolidColorBrush;
                }
                else
                {
                    // dark theme
                    titleTextBlock.Foreground = Application.Current.Resources["settingsPage_Dark_foregroundText"] as SolidColorBrush;
                    descriptionTextBlock.Foreground = Application.Current.Resources["settingsPage_Dark_foregroundSecondary2"] as SolidColorBrush;
                    RippleBrush = Application.Current.Resources["settingsPage_Dark_backgroundSecondary2"] as SolidColorBrush;
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

                translateX = e.MouseDevice.GetPosition(this).X - this.ActualWidth / 2;
                translateY = e.MouseDevice.GetPosition(this).Y - this.ActualHeight / 2;

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
                myTranslate.X = Mouse.GetPosition(this).X - this.ActualWidth / 2;
                myTranslate.Y = Mouse.GetPosition(this).Y - this.ActualHeight / 2;

                ellipse.RenderTransform = myTranslate;

                s.SpeedRatio = 3;
                s.Begin();
            }

            if (_IsToggleButton)
            {
                toggle.DoAnimation = true;
                this.IsActive = !this.IsActive;
            }

            //bubble the event up to the parent
            if (this.Click != null)
                this.Click(this, e);

        }

        private void button_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                translateX = e.MouseDevice.GetPosition(this).X - this.ActualWidth / 2;
                translateY = e.MouseDevice.GetPosition(this).Y - this.ActualHeight / 2;

                if ((translateX >= this.ActualWidth / 2 || translateX <= -this.ActualWidth / 2) || (translateY >= this.ActualHeight / 2 || translateY <= -this.ActualHeight / 2))
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