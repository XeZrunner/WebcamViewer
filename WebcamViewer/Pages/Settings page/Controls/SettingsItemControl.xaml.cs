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

namespace WebcamViewer.Pages.Settings_page.Controls
{
    public partial class SettingsItemControl : UserControl
    {
        public SettingsItemControl()
        {
            InitializeComponent();
        }

        public event RoutedEventHandler Click;

        public enum ViewModes
        {
            Desktop,
            Mobile
        }

        private ViewModes _viewmode = ViewModes.Desktop;

        [Description("The view mode of the button"), Category("Common")]
        public ViewModes ViewMode
        {
            get { return _viewmode; }
            set { _viewmode = value; UpdateView(); }
        }

        [Description("The icon of the button."), Category("Common")]
        public string IconText
        {
            get { return iconTextBlock.Text; }
            set { iconTextBlock.Text = value; }
        }

        [Description("The title of the button"), Category("Common")]
        public string Title
        {
            get { return titleTextBlock.Text; }
            set { titleTextBlock.Text = value; }
        }

        [Description("The description of the button."), Category("Common")]
        public string Description
        {
            get { return descTextBlock.Text; }
            set { descTextBlock.Text = value; }
        }


        private void UpdateView()
        {
            if (_viewmode == ViewModes.Desktop)
            {
                desktopGrid.Visibility = Visibility.Visible;
                mobileGrid.Visibility = Visibility.Collapsed;
                this.Width = 200;
                this.Height = 200;

                button.Clip = borderRect;
            }
            else
            {
                desktopGrid.Visibility = Visibility.Collapsed;
                mobileGrid.Visibility = Visibility.Visible;
                this.Width = Double.NaN;
                this.Height = 70;

                button.Clip = null;
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (this.Click != null)
                this.Click(this, e);
        }
    }
}
