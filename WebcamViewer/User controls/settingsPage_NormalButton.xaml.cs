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
    public partial class settingsPage_NormalButton : UserControl
    {
        public settingsPage_NormalButton()
        {
            InitializeComponent();
        }

        private void usercontrol_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            
        }

        public event RoutedEventHandler Click;

        private _Theme m_Theme = _Theme.Auto;

        [Description("Text of the button"), Category("Appearance")]
        public string Text
        {
            get { return textTextBlock.Text; }
            set { textTextBlock.Text = value; }
        }

        [Description("Text alignment"), Category("Appearance")]
        public HorizontalAlignment TextAlignment
        {
            get { return textTextBlock.HorizontalAlignment; }
            set { textTextBlock.HorizontalAlignment = value; }
        }

        public _Theme Theme
        {
            get { return m_Theme; }
            set { m_Theme = value; UpdateTheming(); }
        }

        public enum _Theme
        {
            Light,
            Dark,
            Auto
        }

        private void UpdateTheming()
        {
            if (m_Theme == _Theme.Auto)
            {
                rippledrawable.SetResourceReference(XeZrunner.UI.ControlEffects.RippleDrawable.ColorProperty, "settingsPage_foregroundSecondary3");
                rippledrawable.SetResourceReference(XeZrunner.UI.ControlEffects.RippleDrawable.FillColorProperty, "settingsPage_foregroundSecondary3");
            }
            else if (m_Theme == _Theme.Light)
            {
                rippledrawable.SetResourceReference(XeZrunner.UI.ControlEffects.RippleDrawable.ColorProperty, "settingsPage_Light_foregroundSecondary3");
                rippledrawable.SetResourceReference(XeZrunner.UI.ControlEffects.RippleDrawable.FillColorProperty, "settingsPage_Light_foregroundSecondary3");
            }
            else
            {
                rippledrawable.SetResourceReference(XeZrunner.UI.ControlEffects.RippleDrawable.ColorProperty, "settingsPage_Dark_foregroundSecondary3");
                rippledrawable.SetResourceReference(XeZrunner.UI.ControlEffects.RippleDrawable.FillColorProperty, "settingsPage_Dark_foregroundSecondary3");
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
