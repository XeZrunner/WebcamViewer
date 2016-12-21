using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace WebcamViewer.Updates.Updates.User_controls
{
    public partial class WindowControlButton : UserControl
    {
        public WindowControlButton()
        {
            InitializeComponent();
        }

        [Description("The icon of the button"), Category("Common")]
        public string TextIcon
        {
            get { return iconTextBlock.Text; }
            set { iconTextBlock.Text = value; iconTextBlock_White.Text = value; }
        }

        public event RoutedEventHandler Click;

        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            if ((string)Tag == "closeButton")
            {
                overRectangle_Close.Visibility = Visibility.Visible;

                iconTextBlock.Visibility = Visibility.Collapsed;
                iconTextBlock_White.Visibility = Visibility.Visible;
            }
            else if ((string)Tag == "dark")
                overRectangle_Dark.Visibility = Visibility.Visible;
            else if ((string)Tag == "light")
                overRectangle_Light.Visibility = Visibility.Visible;

        }

        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            foreach (object _rec in button_grid.Children)
            {
                // get rectangles
                if (_rec.GetType() == (typeof(Rectangle)))
                {
                    Rectangle rec = _rec as Rectangle;
                    rec.Visibility = Visibility.Hidden;
                }
            }
            if ((string)Tag == "closeButton")
            {
                iconTextBlock.Visibility = Visibility.Visible;
                iconTextBlock_White.Visibility = Visibility.Collapsed;
            }
        }

        private void Button_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if ((string)Tag == "closeButton")
            {
                downRectangle_Close.Visibility = Visibility.Visible;

                iconTextBlock.Visibility = Visibility.Collapsed;
                iconTextBlock_White.Visibility = Visibility.Visible;
            }
            else if ((string)Tag == "dark")
                downRectangle_Dark.Visibility = Visibility.Visible;
            else if ((string)Tag == "light")
                downRectangle_Light.Visibility = Visibility.Visible;
        }

        private void Button_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if ((string)Tag == "closeButton")
            {
                downRectangle_Close.Visibility = Visibility.Hidden;

                iconTextBlock.Visibility = Visibility.Visible;
                iconTextBlock_White.Visibility = Visibility.Collapsed;
            }
            else if ((string)Tag == "dark")
                downRectangle_Dark.Visibility = Visibility.Hidden;
            else if ((string)Tag == "light")
                downRectangle_Light.Visibility = Visibility.Hidden;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //bubble the event up to the parent
            if (this.Click != null)
                this.Click(this, e);
        }
    }
}
