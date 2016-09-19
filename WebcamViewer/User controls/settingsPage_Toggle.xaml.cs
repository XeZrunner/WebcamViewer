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
            DoAnimation = true;
            if ((string)this.Tag != "ToggleSwitchButton")
            {
                this.IsActive = !this.IsActive;
            }
        }
    }
}
