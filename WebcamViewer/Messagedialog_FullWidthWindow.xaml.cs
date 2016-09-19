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

        public void SetupDialog(string Title, object Content, bool? DarkTheme, bool Content_DisableMargin, string FirstButtonContent, string SecondButtonContent, RoutedEventHandler FirstButtonClickEvent, RoutedEventHandler SecondButtonClickEvent)
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

            // Content_DisableMargin
            if (Content_DisableMargin)
                contentGrid.Margin = new Thickness(0);
            else
                contentGrid.Margin = new Thickness(0, 0, 0, 15);

            // Theme

            Application.Current.Resources["MessageDialog_FullWidth_DarkButton_Light_BorderBrush"] = Application.Current.Resources["accentcolor_dark"]; // set the light darkbutton borderbrush color to accentcolor_dark

            if (DarkTheme == true)
            {
                Application.Current.Resources["MessageDialog_FullWidth_ForegroundText"] = Application.Current.Resources["MessageDialog_FullWidth_Dark_ForegroundText"];
                Application.Current.Resources["MessageDialog_FullWidth_ForegroundSecondary"] = Application.Current.Resources["MessageDialog_FullWidth_Dark_ForegroundSecondary"];
                Application.Current.Resources["MessageDialog_FullWidth_Background"] = Application.Current.Resources["MessageDialog_FullWidth_Dark_Background"];

                Application.Current.Resources["MessageDialog_FullWidth_Button_Background"] = Application.Current.Resources["MessageDialog_FullWidth_Button_Dark_Background"];
                Application.Current.Resources["MessageDialog_FullWidth_Button_BorderBrush"] = Application.Current.Resources["MessageDialog_FullWidth_Button_Dark_BorderBrush"];
                Application.Current.Resources["MessageDialog_FullWidth_Button_OverBackground"] = Application.Current.Resources["MessageDialog_FullWidth_DarkButton_Dark_OverBackground"];
                Application.Current.Resources["MessageDialog_FullWidth_Button_OverBorderBrush"] = Application.Current.Resources["MessageDialog_FullWidth_Button_Dark_OverBorderBrush"];

                Application.Current.Resources["MessageDialog_FullWidth_DarkButton_BorderBrush"] = Application.Current.Resources["MessageDialog_FullWidth_Button_Dark_BorderBrush"];
            }
            else if (DarkTheme == false)
            {
                Application.Current.Resources["MessageDialog_FullWidth_ForegroundText"] = Application.Current.Resources["MessageDialog_FullWidth_Light_ForegroundText"];
                Application.Current.Resources["MessageDialog_FullWidth_ForegroundSecondary"] = Application.Current.Resources["MessageDialog_FullWidth_Light_ForegroundSecondary"];
                Application.Current.Resources["MessageDialog_FullWidth_Background"] = Application.Current.Resources["MessageDialog_FullWidth_Light_Background"];

                Application.Current.Resources["MessageDialog_FullWidth_Button_Background"] = Application.Current.Resources["MessageDialog_FullWidth_Button_Light_Background"];
                Application.Current.Resources["MessageDialog_FullWidth_Button_BorderBrush"] = Application.Current.Resources["MessageDialog_FullWidth_Button_Light_BorderBrush"];
                Application.Current.Resources["MessageDialog_FullWidth_Button_OverBackground"] = Application.Current.Resources["MessageDialog_FullWidth_Button_Light_OverBackground"];
                Application.Current.Resources["MessageDialog_FullWidth_Button_OverBorderBrush"] = Application.Current.Resources["MessageDialog_FullWidth_Button_Light_OverBorderBrush"];

                Application.Current.Resources["MessageDialog_FullWidth_DarkButton_BorderBrush"] = Application.Current.Resources["MessageDialog_FullWidth_DarkButton_Light_BorderBrush"];
            }
            else
            {
                string themeString;

                if (Properties.Settings.Default.ui_theme == 0)
                    themeString = "Light";
                else
                    themeString = "Dark";

                Application.Current.Resources["MessageDialog_FullWidth_Background"] = Application.Current.Resources["MessageDialog_FullWidth_" + themeString + "_Background"];
                Application.Current.Resources["MessageDialog_FullWidth_ForegroundText"] = Application.Current.Resources["MessageDialog_FullWidth_" + themeString + "_ForegroundText"];
                Application.Current.Resources["MessageDialog_FullWidth_ForegroundSecondary"] = Application.Current.Resources["MessageDialog_FullWidth_" + themeString + "_ForegroundSecondary"];

                Application.Current.Resources["MessageDialog_FullWidth_Button_Background"] = Application.Current.Resources["MessageDialog_FullWidth_Button_" + themeString + "_Background"];
                Application.Current.Resources["MessageDialog_FullWidth_Button_BorderBrush"] = Application.Current.Resources["MessageDialog_FullWidth_Button_" + themeString + "_BorderBrush"];
                Application.Current.Resources["MessageDialog_FullWidth_Button_OverBackground"] = Application.Current.Resources["MessageDialog_FullWidth_Button_" + themeString + "_OverBackground"];
                Application.Current.Resources["MessageDialog_FullWidth_Button_OverBorderBrush"] = Application.Current.Resources["MessageDialog_FullWidth_Button_" + themeString + "_OverBorderBrush"];

                Application.Current.Resources["MessageDialog_FullWidth_DarkButton_BorderBrush"] = Application.Current.Resources["MessageDialog_FullWidth_DarkButton_" + themeString + "_BorderBrush"];
            }

            // Button text
            firstButton.Content = FirstButtonContent;
            secondButton.Content = SecondButtonContent;


            // Button visibility
            //
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

            // Button focus
            if (firstButton.Visibility == Visibility.Visible & secondButton.Visibility == Visibility.Visible)
                firstButton.Focus();
            if (firstButton.Visibility == Visibility.Visible & secondButton.Visibility != Visibility.Visible)
                firstButton.Focus();
            if (firstButton.Visibility != Visibility.Visible & secondButton.Visibility == Visibility.Visible)
                secondButton.Focus();

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

            if (this.ActualWidth <= 500)
                buttonsGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
        }
    }
}
