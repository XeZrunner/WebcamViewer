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
    public partial class settingsPage_TabButton : UserControl
    {
        public settingsPage_TabButton()
        {
            InitializeComponent();

            rippleDrawable.Speed = (float)Properties.Settings.Default.ui_animationspeed;
        }

        private void usercontrol_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

        public event RoutedEventHandler Click;

        bool _IsActive = false;
        bool _ShowDescription = false;

        [Description("Icon of the button"), Category("Appearance")]
        public string IconText
        {
            get { return iconTextBlock.Text; }
            set { iconTextBlock.Text = value; }
        }

        [Description("Title text of the button"), Category("Appearance")]
        public string Title
        {
            get { return titleTextBlock.Text; }
            set { titleTextBlock.Text = value; }
        }

        [Description("Description text of the button"), Category("Appearance")]
        public string Description
        {
            get { return descriptionTextBlock.Text; }
            set { descriptionTextBlock.Text = value; }
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

                    iconTextBlock.SetResourceReference(Control.ForegroundProperty, "accentcolor_dark");
                    titleTextBlock.SetResourceReference(Control.ForegroundProperty, "accentcolor_dark");
                    descriptionTextBlock.SetResourceReference(Control.ForegroundProperty, "accentcolor_dark");

                }
                else
                {
                    activestateRectangle.Visibility = Visibility.Hidden;

                    // set foreground to be bound to the usercontrol's Foreground
                    Binding foreBinding = new Binding();
                    foreBinding.Path = new PropertyPath("Foreground");
                    foreBinding.Source = usercontrol;

                    BindingOperations.SetBinding(iconTextBlock, TextBlock.ForegroundProperty, foreBinding);
                    titleTextBlock.SetResourceReference(Control.ForegroundProperty, "settingsPage_foregroundSecondary");
                    descriptionTextBlock.SetResourceReference(Control.ForegroundProperty, "settingsPage_foregroundSecondary2");
                }
            }
        }

        [Description("Show the description text of the button"), Category("Miscellaneous")]
        public bool ShowDescription
        {
            get { return _ShowDescription; }
            set
            {
                _ShowDescription = value;

                if (_ShowDescription == true)
                    descriptionTextBlock.Visibility = Visibility.Visible;
                else
                    descriptionTextBlock.Visibility = Visibility.Collapsed;
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
