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
    public partial class mobilegrid_ItemControl : UserControl
    {
        public mobilegrid_ItemControl()
        {
            InitializeComponent();
        }

        public event RoutedEventHandler Click;

        [Description("The icon of the button"), Category("Common")]
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

        [Description("The description of the button"), Category("Common")]
        public string Description
        {
            get { return descTextBlock.Text; }
            set { descTextBlock.Text = value; }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //bubble the event up to the parent
            if (this.Click != null)
                this.Click(this, e);
        }
    }
}
