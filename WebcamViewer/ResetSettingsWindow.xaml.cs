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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WebcamViewer
{
    public partial class ResetSettingsWindow : MetroWindow
    {
        public ResetSettingsWindow(bool darkmode = false)
        {
            InitializeComponent();

            if (darkmode == true)
            {
                grid.Background = new SolidColorBrush(Color.FromArgb(255, 20, 20, 20));
                titleLabel.Foreground = new SolidColorBrush(Colors.White); messageLabel.Foreground = new SolidColorBrush(Colors.White); cancelButton.Background = new SolidColorBrush(Color.FromArgb(255, 42, 43, 45)); cancelButton.Foreground = new SolidColorBrush(Colors.White); cancelButton.Style = this.Resources["DarkButtonStyle"] as Style;
                continueButton.Background = new SolidColorBrush(Color.FromArgb(255, 42, 43, 45)); continueButton.Foreground = new SolidColorBrush(Colors.White); continueButton.Style = this.Resources["DarkButtonStyle"] as Style;
                windowCloseButton.Foreground = new SolidColorBrush(Colors.White);
            }

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

        private void continueButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Reset(); Properties.Settings.Default.Save();
            System.Windows.Forms.Application.Restart(); Application.Current.Shutdown();
        }
    }
}
