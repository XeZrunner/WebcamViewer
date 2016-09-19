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
    public partial class settingsPage_AccentColorButton : UserControl
    {
        public settingsPage_AccentColorButton()
        {
            InitializeComponent();
        }

        public event RoutedEventHandler Click;

        private int _accent = 0;

        [Description("The accent color to use\n(ui_accent setting)"), Category("Common")]
        public int accent
        {
            get { return _accent; }
            set
            {
                _accent = value;

                UpdateAccentOnButton();
            }
        }

        private void UpdateAccentOnButton()
        {
            this.Background = Application.Current.Resources["accentcolor_dark" + _accent] as SolidColorBrush;
            rectangle.Fill = Application.Current.Resources["accentcolor_light" + _accent] as SolidColorBrush;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //bubble the event up to the parent
            if (this.Click != null)
                this.Click(this, e);
        }
    }
}
