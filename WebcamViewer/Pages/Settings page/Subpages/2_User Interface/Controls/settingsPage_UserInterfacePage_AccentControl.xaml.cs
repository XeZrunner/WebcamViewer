using System;
using System.Collections.Generic;
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

namespace WebcamViewer.Pages.Settings_page.Subpages._2_User_Interface.Controls
{
    public partial class settingsPage_UserInterfacePage_AccentControl : UserControl
    {
        public settingsPage_UserInterfacePage_AccentControl()
        {
            InitializeComponent();
        }

        public int chosenAccent = Properties.Settings.Default.ui_accent;

        Theming Theming = new Theming();

        void AccentButtonClick(object sender, RoutedEventArgs e)
        {
            User_controls.settingsPage_AccentColorButton btn = sender as User_controls.settingsPage_AccentColorButton;
            chosenAccent = btn.accent;

            Theming.AccentColor.SetAccentColor(chosenAccent); // to preview
        }
    }
}
