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
                    border.Background = Application.Current.Resources["accentcolor_dark"] as SolidColorBrush;
                    border.BorderBrush = Application.Current.Resources["accentcolor_dark"] as SolidColorBrush;
                    ellipse.Fill = new SolidColorBrush(Colors.White);

                    // storyboard
                }
                else
                {
                    border.Background = Application.Current.Resources["settingsPage_background"] as SolidColorBrush;
                    border.BorderBrush = Application.Current.Resources["settingsPage_foregroundSecondary"] as SolidColorBrush;
                    ellipse.Fill = Application.Current.Resources["settingsPage_foregroundSecondary"] as SolidColorBrush;

                    // storyboard
                }
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if ((string)this.Tag != "ToggleSwitchButton")
            this.IsActive = !this.IsActive;
        }
    }
}
