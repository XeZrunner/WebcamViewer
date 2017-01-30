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

namespace WebcamViewer.Pages.Home_page.Controls
{
    public partial class ErrorPanel_LinkButton : UserControl
    {
        public ErrorPanel_LinkButton()
        {
            InitializeComponent();
        }

        public event RoutedEventHandler Click;

        [Description("The title of the button."), Category("Common")]
        public string Title
        {
            get { return titleLabel.Content as string; }
            set { titleLabel.Content = value; }
        }

        [Description("The description of the button."), Category("Common")]
        public string Description
        {
            get { return descriptionLabel.Content as string; }
            set { descriptionLabel.Content = value; }
        }

        [Description("The icon of the button."), Category("Common")]
        public string Icon
        {
            get { return iconLabel.Content as string; }
            set { iconLabel.Content = value; }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (this.Click != null)
                this.Click(this, e);
        }
    }
}
