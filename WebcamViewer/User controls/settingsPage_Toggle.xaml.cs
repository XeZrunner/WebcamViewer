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

namespace WebcamViewer.User_controls
{
    public partial class settingsPage_Toggle : UserControl
    {
        public settingsPage_Toggle()
        {
            InitializeComponent();
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
                    Rectangle temp_rec = new Rectangle(); temp_rec.Fill = Application.Current.Resources["accentcolor_dark"] as SolidColorBrush;
                    SolidColorBrush temp_brush = (SolidColorBrush)temp_rec.Fill;

                    // original foregroundsecondary brush
                    Rectangle original_rec = new Rectangle(); original_rec.Fill = Application.Current.Resources["settingsPage_foregroundSecondary"] as SolidColorBrush;
                    SolidColorBrush original_brush = (SolidColorBrush)original_rec.Fill;

                    // settings background brush
                    Rectangle settingsbackground_rec = new Rectangle(); settingsbackground_rec.Fill = new SolidColorBrush(Colors.Transparent); //settingsbackground_rec.Fill = Application.Current.Resources["settingsPage_background"] as SolidColorBrush;
                    SolidColorBrush settingsbackground_brush = (SolidColorBrush)original_rec.Fill;

                    // animations
                    ColorAnimation animation = new ColorAnimation(temp_brush.Color, new TimeSpan(0, 0, 0, 0, 500).Duration());
                    ColorAnimation animation_ellipseFill = new ColorAnimation(Colors.White, new TimeSpan(0, 0, 0, 0, 500).Duration());

                    // remove Frozen™
                    border.Background = new SolidColorBrush();
                    border.BorderBrush = new SolidColorBrush(original_brush.Color);
                    ellipse.Fill = new SolidColorBrush(settingsbackground_brush.Color);
                    
                    // animate!
                    if (DoAnimation)
                    {
                        border.Background.BeginAnimation(SolidColorBrush.ColorProperty, animation);
                        border.BorderBrush.BeginAnimation(SolidColorBrush.ColorProperty, animation);
                        ellipse.Fill.BeginAnimation(SolidColorBrush.ColorProperty, animation_ellipseFill);
                    }
                    else
                    {
                        border.Background = temp_brush;
                        border.BorderBrush = temp_brush;
                        ellipse.Fill = Brushes.White;

                        s_ToggleIn.SkipToFill();
                    }
                }
                else
                {
                    // animation
                    Storyboard s_ToggleOut = (Storyboard)FindResource("Toggle_Out");
                    s_ToggleOut.Begin();

                    // target brush
                    Rectangle temp_rec = new Rectangle(); temp_rec.Fill = Application.Current.Resources["settingsPage_foregroundSecondary"] as SolidColorBrush;
                    SolidColorBrush temp_brush = (SolidColorBrush)temp_rec.Fill;

                    // original foregroundsecondary brush
                    Rectangle original_rec = new Rectangle(); original_rec.Fill = Application.Current.Resources["accentcolor_dark"] as SolidColorBrush;
                    SolidColorBrush original_brush = (SolidColorBrush)original_rec.Fill;

                    // settings background brush
                    Rectangle settingsbackground_rec = new Rectangle(); settingsbackground_rec.Fill = new SolidColorBrush(Colors.Transparent); //settingsbackground_rec.Fill = Application.Current.Resources["settingsPage_background"] as SolidColorBrush;
                    SolidColorBrush settingsbackground_brush = (SolidColorBrush)settingsbackground_rec.Fill;

                    // animations
                    ColorAnimation animation = new ColorAnimation(temp_brush.Color, new TimeSpan(0, 0, 0, 0, 500).Duration());
                    ColorAnimation animation_background = new ColorAnimation(settingsbackground_brush.Color, new TimeSpan(0, 0, 0, 0, 250).Duration());
                    ColorAnimation animation_ellipseFill = new ColorAnimation(temp_brush.Color, new TimeSpan(0, 0, 0, 0, 500).Duration());

                    // remove Frozen™
                    border.Background = new SolidColorBrush(original_brush.Color);
                    border.BorderBrush = new SolidColorBrush(original_brush.Color);
                    ellipse.Fill = new SolidColorBrush(original_brush.Color);

                    // animate!
                    if (DoAnimation)
                    {
                        border.Background.BeginAnimation(SolidColorBrush.ColorProperty, animation_background);
                        border.BorderBrush.BeginAnimation(SolidColorBrush.ColorProperty, animation);
                        ellipse.Fill.BeginAnimation(SolidColorBrush.ColorProperty, animation_ellipseFill);
                    }
                    else
                    {
                        border.Background = settingsbackground_brush;
                        border.BorderBrush = temp_brush;
                        ellipse.Fill = temp_brush;

                        s_ToggleOut.SkipToFill();
                    }
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
            DoAnimation = true;
            if ((string)this.Tag != "ToggleSwitchButton")
            {
                this.IsActive = !this.IsActive;
            }
        }
    }
}
