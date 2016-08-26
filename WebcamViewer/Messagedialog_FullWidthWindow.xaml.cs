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
    public partial class Messagedialog_FullWidthWindow : MetroWindow
    {
        public Messagedialog_FullWidthWindow()
        {
            InitializeComponent();
        }

        public void SetupDialog(string Title, object Content, bool DarkTheme, bool Content_DisableMargin, string FirstButtonContent, string SecondButtonContent, RoutedEventHandler FirstButtonClickEvent, RoutedEventHandler SecondButtonClickEvent)
        {
            // Title
            if (Title != "")
                titleLabel.Content = Title;
            else
            {
                titleLabel.Visibility = Visibility.Collapsed;
                if (Content_DisableMargin == false)
                    rowdefinition_0.Height = new GridLength(32);
            }

            // Content
            if (!(Content is string))
                contentGrid.Children.Add((UIElement)Content);
            else
            {
                TextBlock textblock = new TextBlock(); textblock.FontSize = 14; textblock.Margin = new Thickness(5); textblock.TextWrapping = TextWrapping.Wrap;
                textblock.Text = Content as string;

                contentGrid.Children.Add(textblock);
            }

            // Dark theme
            /*
            if (DarkTheme)
            {
                Application.Current.Resources["MessageDialog_ForegroundText"] = Application.Current.Resources["MessageDialog_Dark_ForegroundText"];
                Application.Current.Resources["MessageDialog_ForegroundSecondary"] = Application.Current.Resources["MessageDialog_Dark_ForegroundSecondary"];
                Application.Current.Resources["MessageDialog_Background"] = Application.Current.Resources["MessageDialog_Dark_Background"];

                firstButton.Style = Application.Current.Resources["UWPButtonStyle_Dark"] as Style;
                secondButton.Style = Application.Current.Resources["UWPButtonStyle_Dark"] as Style;
            }
            else
            {
                Application.Current.Resources["MessageDialog_ForegroundText"] = Application.Current.Resources["MessageDialog_Light_ForegroundText"];
                Application.Current.Resources["MessageDialog_ForegroundSecondary"] = Application.Current.Resources["MessageDialog_Light_ForegroundSecondary"];
                Application.Current.Resources["MessageDialog_Background"] = Application.Current.Resources["MessageDialog_Light_Background"];

                firstButton.Style = Application.Current.Resources["UWPButtonStyle"] as Style;
                secondButton.Style = Application.Current.Resources["UWPButtonStyle"] as Style;
            }
            */

            // Content_DisableMargin
            if (Content_DisableMargin)
                contentGrid.Margin = new Thickness(0);
            else
                contentGrid.Margin = new Thickness(0, 0, 0, 15);

            // Button text
            firstButton.Content = FirstButtonContent;
            secondButton.Content = SecondButtonContent;


            // ----- Button visibility ------ //

            // First button
            if (FirstButtonContent != "")
                firstButton.Visibility = Visibility.Visible;
            else
                firstButton.Visibility = Visibility.Collapsed;

            // Second button
            if (SecondButtonContent != "")
                secondButton.Visibility = Visibility.Visible;
            else
                secondButton.Visibility = Visibility.Collapsed;

            // ----- Button visibility ----- //


            // Button click events
            if (FirstButtonClickEvent == null)
                firstButton.Click += (s, ev) => { this.Close(); };
            else
                firstButton.Click += FirstButtonClickEvent;

            if (SecondButtonClickEvent != null)
                secondButton.Click += SecondButtonClickEvent;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Size
            this.Width = Owner.Width - 2;

            double parentWidth = this.Owner.Width;
            double parentHeight = this.Owner.Height;
            double windowWidth = this.Width;
            double windowHeight = this.Height;
            this.Left = parentWidth / 2;
            this.Top = (parentHeight / 2) - (windowHeight / 2);
        }
    }
}
