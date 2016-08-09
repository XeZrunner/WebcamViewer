using MahApps.Metro.Controls;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WebcamViewer
{
    public partial class ConfigurationEditorWindow : MetroWindow
    {

        bool darkmode = true;

        public ConfigurationEditorWindow(bool disable = false)
        {
            InitializeComponent();

            // get accent color from parent
            Window mainwindow = Application.Current.MainWindow;
            SolidColorBrush background = mainwindow.Resources["res_accentBackground"] as SolidColorBrush;
            SolidColorBrush foreground = mainwindow.Resources["res_accentForeground"] as SolidColorBrush;

            this.Resources["res_accentBackground"] = background;
            this.Resources["res_accentForeground"] = foreground;
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void titlebarGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }
    }
}
