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
    public partial class webcamPage_menuGrid_CameraActionButton : UserControl
    {
        public webcamPage_menuGrid_CameraActionButton()
        {
            InitializeComponent();
        }

        Theming Theming = new Theming();

        private void usercontrol_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //grid.Width = this.ActualWidth * 2;
        }

        public event RoutedEventHandler Click;

        [Description("Text of the button"), Category("Appearance")]
        public string Text
        {
            get { return textTextBlock.Text; }
            set { textTextBlock.Text = value.ToUpper(); }
        }

        [Description("Alignment of the text"), Category("Appearance")]
        public HorizontalAlignment TextAlignment
        {
            get { return textTextBlock.HorizontalAlignment; }
            set { textTextBlock.HorizontalAlignment = value; }
        }

        /*
        static FrameworkPropertyMetadata ColorPropertyMetaData = new FrameworkPropertyMetadata(Theming.AccentColor.GetAccentColor(Properties.Settings.Default.ui_accent, Properties.Settings.Default.ui_theme) as SolidColorBrush, new PropertyChangedCallback(ColorProperty_Changed));

        static void ColorProperty_Changed(DependencyObject dobj, DependencyPropertyChangedEventArgs e)
        {
            webcamPage_menuGrid_CameraActionButton main = dobj as webcamPage_menuGrid_CameraActionButton;
        }

        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register("Color", typeof(SolidColorBrush), typeof(webcamPage_menuGrid_CameraActionButton), ColorPropertyMetaData);

        /// <summary>
        /// The color of the ripple.
        /// </summary>
        /// 
        [Description("Mouse over color"), Category("Brush")]
        public SolidColorBrush Color
        {
            get { return GetValue(ColorProperty) as SolidColorBrush; }
            set
            {
                SetValue(ColorProperty, value);
            }
        }

        */

        /*
        [Description("Ripple brush"), Category("Appearance")]
        public Brush RippleBrush
        {
            get { return ellipse.Fill; }
            set { ellipse.Fill = value; }
        }
        */

        private void button_Click(object sender, RoutedEventArgs e)
        {
            //bubble the event up to the parent
            if (this.Click != null)
                this.Click(this, e);
        }
    }
}
