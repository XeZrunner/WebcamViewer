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
    public partial class AppBar_Button : UserControl
    {
        public AppBar_Button()
        {
            InitializeComponent();
        }

        private void usercontrol_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

        public event RoutedEventHandler Click;

        [Description("The icon of the button"), Category("Common")]
        public string IconText
        {
            get { return iconTextBlock.Text; }
            set { iconTextBlock.Text = value; }
        }

        [Description("The color of the ripple"), Category("Common")]
        public SolidColorBrush RippleBrush
        {
            get { return ripple.Color; }
            set { ripple.Color = value; }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            //bubble the event up to the parent
            if (this.Click != null)
                this.Click(this, e);
        }
    }
}
