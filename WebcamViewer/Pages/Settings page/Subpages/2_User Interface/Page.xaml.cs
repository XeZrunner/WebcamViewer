﻿using System;
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
using WebcamViewer.User_controls;

namespace WebcamViewer.Pages.Settings_page.Subpages._2_User_Interface
{
    public partial class Page : UserControl
    {
        public Page()
        {
            InitializeComponent();
        }

        MainWindow mainwindow = Application.Current.MainWindow as MainWindow;

        Theming Theming = new Theming();

        private void page_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (UIElement _panel in sectionsPanel.Children)
            {
                if (_panel.GetType() == (typeof(StackPanel)))
                {
                    StackPanel panel = _panel as StackPanel;

                    foreach (UIElement btn in panel.Children)
                    {
                        if (btn.GetType() == (typeof(settingsPage_ToggleSwitchButton)))
                        {
                            settingsPage_ToggleSwitchButton button = btn as settingsPage_ToggleSwitchButton;
                            if (button.Tag != null & (string)button.Tag != "")
                            {
                                button.IsActive = (bool)Properties.Settings.Default[(string)button.Tag];
                            }
                        }
                    }
                }
            }

            #region Accent color button
            string accentcolorButton_descriptionText = settingsPage_UserInterfacePage_UI_AccentColorButton.Description.Substring(0, 65);
            settingsPage_UserInterfacePage_UI_AccentColorButton.Description = accentcolorButton_descriptionText + "Blue"; // "Blue" will equal current color from the constant array
            #endregion
        }

        #region Dialogs

        /// <summary>
        /// Displays a Windows 10 UWP ContentDialog-style text message dialog.
        /// </summary>
        /// <param name="Title">The title of the dialog</param>
        /// <param name="Content">The main text of the dialog</param>
        /// <param name="DarkMode">Determintes whether to style the window light or dark. (0 = dark, 1 = light, null = automatic from theme)</param>
        void TextMessageDialog(string Title, string Content, bool? DarkMode = null)
        {
            Popups.MessageDialog dlg = new Popups.MessageDialog();

            dlg.Title = Title;
            dlg.Content = Content;

            dlg.IsDarkTheme = DarkMode;

            dlg.ShowDialog();
        }

        /// <summary>
        /// Displays a Windows 8.x MessageDialog-style text message dialog.
        /// </summary>
        /// <param name="Title">The title of the dialog</param>
        /// <param name="Content">The main text of the dialog</param>
        /// <param name="DarkMode">Determintes whether to style the window light or dark. (0 = dark, 1 = light, null = automatic from theme)</param>
        void TextMessageDialog_FullWidth(string Title, string Content, bool? DarkMode = null)
        {
            Popups.MessageDialog_FullWidth dlg = new Popups.MessageDialog_FullWidth();

            dlg.Title = Title;
            dlg.Content = Content;

            dlg.IsDarkTheme = DarkMode;

            dlg.ShowDialog();
        }

        #endregion

        void settingsPage_ToggleButtonClick(object sender, RoutedEventArgs e)
        {
            settingsPage_ToggleSwitchButton sBtn = sender as settingsPage_ToggleSwitchButton;

            try
            {
                Properties.Settings.Default[(string)sBtn.Tag] = sBtn.IsActive;
                Properties.Settings.Default.Save();

                mainwindow.GetUserConfiguration(true);
            }
            catch (Exception ex)
            {
                sBtn.IsActive = !sBtn.IsActive;

                if (sBtn.Tag != null)
                    TextMessageDialog_FullWidth("Invalid property", "The setting " + sBtn.Tag.ToString() + " (probably) doesn't exist.\nCheck the button's Tag.\n\nExact error message : " + ex.Message);
                else
                    TextMessageDialog_FullWidth("No Tag on the button", "The button " + sBtn.Name + "does not contain a Tag. (the button's Tag equals null)");
            }
        }

        #region Buttons

        private void Rectangle_Loaded(object sender, RoutedEventArgs e)
        {
            Rectangle r = sender as Rectangle;
            r.Name = "settingsPage_UserInterfacePage_UI_AccentColorButton_Rectangle";

            // create binding for accent color
            Binding binding = new Binding();
            binding.Source = Application.Current.Resources;
            binding.Path = new PropertyPath("accentcolor_dark");

            // Bind accent color to the temp brush
            SolidColorBrush brush = new SolidColorBrush();
            BindingOperations.SetBinding(brush, SolidColorBrush.ColorProperty, binding);

            // create new rectangle
            r = new Rectangle { Fill = brush };
        }

        private void settingsPage_UserInterfacePage_UI_AccentColorButton_Click(object sender, RoutedEventArgs e)
        {
            Popups.MessageDialog dlg = new Popups.MessageDialog();
            dlg.Title = "";

            dlg.Content_DisableMargin = true;

            Controls.settingsPage_UserInterfacePage_AccentControl content = new Controls.settingsPage_UserInterfacePage_AccentControl();

            dlg.Content = content;

            dlg.FirstButtonContent = "Go back";
            dlg.SecondButtonContent = "Apply changes";

            if (dlg.ShowDialogWithResult() == 1)
            {
                Theming.AccentColor.SetAccentColor(content.chosenAccent, true);

                mainwindow.GetUserConfiguration(true);

                mainwindow.UpdateDefaultStyle();
            }
        }

        private void settingsPage_UserInterfacePage_UI_ThemeButton_Click(object sender, RoutedEventArgs e)
        {
            Popups.MessageDialog dlg = new Popups.MessageDialog();
            dlg.Title = "";

            dlg.Content_DisableMargin = true;

            Controls.settingsPage_UserInterfacePage_ThemeControl content = new Controls.settingsPage_UserInterfacePage_ThemeControl();
            #region control code
            switch (Properties.Settings.Default.ui_theme)
            {
                case 0: //light theme
                    {
                        content.radiobutton_Light.IsChecked = true;
                        break;
                    }
                case 1: // dark theme
                    {
                        content.radiobutton_Dark.IsChecked = true;
                        break;
                    }
            }
            #endregion

            dlg.Content = content;

            dlg.FirstButtonContent = "Go back";
            dlg.SecondButtonContent = "Apply changes";

            if (dlg.ShowDialogWithResult() == 1)
            {

                if (content.radiobutton_Light.IsChecked == true)
                {
                    Properties.Settings.Default.ui_theme = 0;
                    Properties.Settings.Default.Save();
                }
                else
                {
                    Properties.Settings.Default.ui_theme = 1;
                    Properties.Settings.Default.Save();
                }

                mainwindow.GetUserConfiguration(true);

                mainwindow.UpdateDefaultStyle();
            }
        }

        #endregion

        private void page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            sectionsPanel.MaxWidth = this.ActualWidth - 20;
        }
    }
}
