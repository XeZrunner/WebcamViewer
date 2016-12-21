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

namespace WebcamViewer.Updates.Updates.User_controls
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

                    // set!
                    border.SetResourceReference(Border.BackgroundProperty, "accentcolor_dark");
                    border.SetResourceReference(Border.BorderBrushProperty, "accentcolor_dark");
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

                    // set!
                    border.SetResourceReference(Border.BackgroundProperty, "settingsPage_background");
                    border.SetResourceReference(Border.BorderBrushProperty, "settingsPage_foregroundSecondary");
                    ellipse.SetResourceReference(Ellipse.FillProperty, "settingsPage_foregroundSecondary");

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

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            
        }
    }
}
