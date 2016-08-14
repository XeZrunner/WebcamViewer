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
        }

        public void SetupDialog(string Title, object Content, bool DarkTheme, string FirstButtonContent, string SecondButtonContent, RoutedEventHandler FirstButtonClickEvent, RoutedEventHandler SecondButtonClickEvent)
        {
            // Title
            titleLabel.Content = Title;

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

    }
}
