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
    public partial class MessagedialogWindow : MetroWindow
    {
        public MessagedialogWindow()
        {
            InitializeComponent();

            //if (Title != "")
            //    titleLabel.Content = Title;
            //else
            //    titleLabel.Visibility = Visibility.Collapsed;

            //if (Message != "")
            //    messageLabel.Content = Message;
            //else
            //    messageLabel.Visibility = Visibility.Collapsed;

            //if (darkmode == true)
            //{
            //    grid.Background = new SolidColorBrush(Color.FromArgb(255, 56, 56, 56));
            //    titleLabel.Foreground = new SolidColorBrush(Colors.White); messageLabel.Foreground = new SolidColorBrush(Colors.White); closeButton.Style = this.Resources["UWPButtonStyle_Dark"] as Style; windowCloseButton.Foreground = new SolidColorBrush(Colors.White);
            //}

            // get accent color from parent
            Window mainwindow = Application.Current.MainWindow;
            SolidColorBrush background = mainwindow.Resources["res_accentBackground"] as SolidColorBrush;
            SolidColorBrush foreground = mainwindow.Resources["res_accentForeground"] as SolidColorBrush;

            this.Resources["res_accentBackground"] = background;
            this.Resources["res_accentForeground"] = foreground;
        }

        public void SetupDialog(string Title, object Content, bool darkmode, string FirstButtonContent, string SecondButtonContent, string ThirdButtonContent, bool FirstButtonEnabled, bool SecondButtonEnabled, bool ThirdButtonEnabled, RoutedEventHandler FirstButtonClick, RoutedEventHandler SecondButtonClick, RoutedEventHandler ThirdButtonClick)
        {
            // Content
            titleLabel.Content = Title;
            titlebarGrid_titleLabel.Content = Title;

            if (!(Content is string))
                main_contentGrid.Children.Add((UIElement)Content);
            else
            {
                Label textLabel = new Label(); textLabel.Content = Content; textLabel.FontSize = 14;
                main_contentGrid.Children.Add((UIElement)textLabel);
            }

            // Dark mode
            if (darkmode)
            {
                // dark dialog
            }

            // Button visiblity
            if (FirstButtonContent == "")
                firstButton.Visibility = Visibility.Collapsed;

            if (SecondButtonContent != "")
                secondButton.Visibility = Visibility.Visible;
            else
                secondButton.Visibility = Visibility.Collapsed;

            if (ThirdButtonContent != "")
            {
                thirdButton.Visibility = Visibility.Visible;
                ThirdButtonColumn.Width = new GridLength(0, GridUnitType.Star);
            }
            else
            {
                thirdButton.Visibility = Visibility.Collapsed;
                ThirdButtonColumn.Width = new GridLength(0, GridUnitType.Auto);
            }

            // Button text
            firstButton.Content = FirstButtonContent;
            secondButton.Content = SecondButtonContent;
            thirdButton.Content = ThirdButtonContent;

            // Button enabled state
            firstButton.IsEnabled = FirstButtonEnabled;
            secondButton.IsEnabled = SecondButtonEnabled;
            thirdButton.IsEnabled = ThirdButtonEnabled;

            // Button click events
            if (FirstButtonClick == null)
                firstButton.Click += (s, ev) => { this.Close(); };
            else
                firstButton.Click += FirstButtonClick;

            if (SecondButtonClick != null)
                secondButton.Click += SecondButtonClick;

            if (ThirdButtonClick != null)
                thirdButton.Click += ThirdButtonClick;
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
