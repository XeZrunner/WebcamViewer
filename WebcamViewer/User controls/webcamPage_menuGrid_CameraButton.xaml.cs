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
    public partial class webcamPage_menuGrid_CameraButton : UserControl
    {
        public webcamPage_menuGrid_CameraButton()
        {
            InitializeComponent();
        }

        private bool _IsActive = false;

        public event RoutedEventHandler Click;

        [Description("Text of the button"), Category("Appearance")]
        public string Text
        {
            get { return textTextBlock.Text; }
            set { textTextBlock.Text = value; }
        }

        [Description("Active state of the button"), Category("Appearance")]
        public bool IsActive
        {
            get { return _IsActive; }
            set
            {
                _IsActive = value;

                if (_IsActive == true)
                {
                    activestateRectangle.Visibility = Visibility.Visible;

                    textTextBlock.SetResourceReference(Control.ForegroundProperty, "accentcolor_dark");

                }
                else
                {
                    activestateRectangle.Visibility = Visibility.Hidden;

                    // set foreground to be bound to the usercontrol's Foreground
                    Binding foreBinding = new Binding();
                    foreBinding.Path = new PropertyPath("Foreground");
                    foreBinding.Source = usercontrol;

                    BindingOperations.SetBinding(textTextBlock, TextBlock.ForegroundProperty, foreBinding);
                }
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            //bubble the event up to the parent
            if (this.Click != null)
                this.Click(this, e);
        }
    }
}
