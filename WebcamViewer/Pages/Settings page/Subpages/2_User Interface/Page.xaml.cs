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
                        if (btn is settingsPage_ToggleSwitchButton)
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

        private void page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            sectionsPanel.MaxWidth = this.ActualWidth - 20;
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
            if (!Keyboard.IsKeyDown(Key.LeftShift))
            {
                Popups.ContentDialog dlg = new Popups.ContentDialog();
                dlg.Title = "";

                dlg.NoMarginDialog = true;

                Controls.settingsPage_UserInterfacePage_AccentControl content = new Controls.settingsPage_UserInterfacePage_AccentControl();

                dlg.ContentGrid = content;

                dlg.Button0_Text = "Go back";
                dlg.Button1_Text = "Apply changes";

                dlg.Button1_Click += (s, ev) =>
                {
                    Theming.AccentColor.SetAccentColor(content.chosenAccent, true);

                    mainwindow.GetUserConfiguration(true);

                    mainwindow.UpdateDefaultStyle();
                };

                dlg.ShowDialog();
            }
            else
            {
                Popups.ContentDialog dialog = new Popups.ContentDialog();
                dialog.Title = "Set a custom accent color";
                dialog.Button0_Text = "Cancel";
                dialog.Button1_Text = "Apply";

                //dialog.Theme = ContentDialogControl._Theme.Light;

                StackPanel contentPanel = new StackPanel();
                dialog.ContentGrid = contentPanel;

                Xceed.Wpf.Toolkit.ColorCanvas picker = new Xceed.Wpf.Toolkit.ColorCanvas() { Background = Brushes.Transparent, BorderBrush = Brushes.Transparent, Margin = new Thickness(0, 10, 0, 0) };
                Label lbl0 = new Label() { Content = "The color picker is experimental as of now.", FontSize = 14, Margin = new Thickness(0, 10, 0, 0) };
                Label lbl_darkerr = new Label() { Content = "The dark theme is not supported on this dialog.", FontSize = 14, Foreground = Brushes.DarkOrange, Margin = new Thickness(0, 5, 0, 0) };

                contentPanel.Children.Add(picker);
                contentPanel.Children.Add(lbl0);
                if (Properties.Settings.Default.ui_theme == 1)
                    contentPanel.Children.Add(lbl_darkerr);

                dialog.ShowDialog();
            }
        }

        private void settingsPage_UserInterfacePage_UI_ThemeButton_Click(object sender, RoutedEventArgs e)
        {
            Popups.ContentDialog dlg = new Popups.ContentDialog();
            dlg.Title = "";

            dlg.NoMarginDialog = true;

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
            if (Properties.Settings.Default.home_webcamimageBackgroundMode_BlackOverride)
                content.checkbox_homepageBlackOverride.IsChecked = true;
            #endregion

            dlg.ContentGrid = content;

            dlg.Button0_Text = "Go back";
            dlg.Button1_Text = "Apply changes";

            dlg.Button1_Click += (s, ev) =>
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

                if (content.checkbox_homepageBlackOverride.IsChecked == true)
                {
                    Properties.Settings.Default.home_webcamimageBackgroundMode_BlackOverride = true;
                    Properties.Settings.Default.Save();
                }
                else
                {
                    Properties.Settings.Default.home_webcamimageBackgroundMode_BlackOverride = false;
                    Properties.Settings.Default.Save();
                }

                mainwindow.GetUserConfiguration(true);
            };

            dlg.ShowDialog();
        }

        private void settingsPage_UserInterfacePage_UI_LanguageButton_Click(object sender, RoutedEventArgs e)
        {
            Popups.ContentDialog dlg = new Popups.ContentDialog();
            dlg.Title = "";

            dlg.NoMarginDialog = true;

            Controls.settingsPage_UserInterfacePage_LanguageControl content = new Controls.settingsPage_UserInterfacePage_LanguageControl();
            #region control code
            foreach (RadioButton btn in content.mainStackPanel.Children)
            {
                if ((string)btn.Tag == Properties.Settings.Default.app_language)
                    btn.IsChecked = true;
            }
            #endregion

            dlg.ContentGrid = content;

            dlg.Button0_Text = "Go back";
            dlg.Button1_Text = "Apply changes";

            dlg.Button1_Click += (s, ev) =>
            {
                // change and save
                string chosenlanguage = "default";
                foreach (RadioButton btn in content.mainStackPanel.Children)
                {
                    if (btn.IsChecked == true)
                        chosenlanguage = btn.Tag as string;
                }

                Properties.Settings.Default.app_language = chosenlanguage;
                Properties.Settings.Default.Save();

                // restart popup
                Popups.ContentDialog dlg2 = new Popups.ContentDialog()
                {
                    Title = "Restart required to change language",
                    Button0_Text = "Restart later",
                    Button1_Text = "Restart now"
                };

                StackPanel panel = new StackPanel();
                Label lbl0 = new Label { Content = "The app needs to be restarted in order to change the language. Would you like to restart now?" };
                CheckBox box0 = new CheckBox { Content = "Bring me back to the same camera", Margin = new Thickness(0, 5, 0, 5), IsChecked = true };

                panel.Children.Add(lbl0);
                panel.Children.Add(box0);

                dlg2.ContentGrid = panel;

                dlg2.Button1_Click += (s1, ev1) =>
                {
                    // bring back to same camera
                    System.Windows.Forms.Application.Restart();
                    Application.Current.Shutdown();
                    return;
                };

                mainwindow.GetUserConfiguration(true);
            };

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
    }
}
