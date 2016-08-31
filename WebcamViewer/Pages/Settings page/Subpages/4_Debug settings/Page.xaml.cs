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
using System.Windows.Threading;
using WebcamViewer.User_controls;

namespace WebcamViewer.Pages.Settings_page.Subpages._4_Debug_settings
{
    public partial class Page : UserControl
    {
        public Page()
        {
            InitializeComponent();
        }

        MainWindow mainwindow = Application.Current.MainWindow as MainWindow;

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

            mainwindow.global_Dim();

            dlg.ShowDialog();

            mainwindow.global_UnDim();
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

            mainwindow.global_Dim();

            dlg.ShowDialog();

            mainwindow.global_UnDim();
        }

        #endregion

        async void settingsPage_ToggleButtonClick(object sender, RoutedEventArgs e)
        {
            settingsPage_ToggleSwitchButton sBtn = sender as settingsPage_ToggleSwitchButton;

            try
            {
                Properties.Settings.Default[(string)sBtn.Tag] = sBtn.IsActive;
                Properties.Settings.Default.Save();

                await mainwindow.GetUserConfiguration(true);
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

        private void settingsPage_DebugMenuPage_Home_ProgressDebugButton_Click(object sender, RoutedEventArgs e)
        {
            // Create progress UI debug dialog
            Popups.MessageDialog dlg = new Popups.MessageDialog();
            dlg.Title = "";

            #region Create content
            StackPanel stackpanel = new StackPanel();

            Label lbl = new Label { Content = "Choose a progress type. It will automatically end after 10 seconds." };

            StackPanel rbtn_stackpanel = new StackPanel { Margin = new Thickness(0, 5, 0, 10) };

            RadioButton rbtn_0 = new RadioButton { Content = "Modal progressring", IsChecked = true };
            RadioButton rbtn_1 = new RadioButton { Content = "Modal progressbar (indeterminate)" };
            RadioButton rbtn_2 = new RadioButton { Content = "In-menu progressring" };

            rbtn_stackpanel.Children.Add(rbtn_0);
            rbtn_stackpanel.Children.Add(rbtn_1);
            rbtn_stackpanel.Children.Add(rbtn_2);

            TextBox textbox = new TextBox { Text = "Progress UI Debug", Margin = new Thickness(0, 0, 0, 10) };
            CheckBox checkbox_countdown = new CheckBox { Content = "Show countdown", IsChecked = true, Margin = new Thickness(0, 0, 0, 5) };
            #endregion

            // Add stuff to the content stackpanel
            stackpanel.Children.Add(lbl);

            stackpanel.Children.Add(rbtn_stackpanel);

            stackpanel.Children.Add(textbox);

            stackpanel.Children.Add(checkbox_countdown);

            //  Make the stackpanel the content
            dlg.Content = stackpanel;

            // Buttons
            dlg.FirstButtonContent = "Cancel";
            dlg.SecondButtonContent = "Accept";

            if (dlg.ShowDialogWithResult() == 1)
            {
                mainwindow.SwitchToPage(0);

                #region Timers
                int ui_clocktimer_countdown = 10;

                DispatcherTimer ui_clocktimer = new DispatcherTimer();
                ui_clocktimer.Interval = new TimeSpan(0, 0, 1);
                ui_clocktimer.Tick += (s, ev) =>
                {
                    if (checkbox_countdown.IsChecked == true)
                    {
                        if (mainwindow.webcamPage_progressLabel.Visibility == Visibility.Visible)
                            mainwindow.webcamPage_progressLabel.Content = textbox.Text + " - " + ui_clocktimer_countdown;
                        if (mainwindow.webcamPage_menuGrid_progressPanel_progressTextBlock.Visibility == Visibility.Visible)
                            mainwindow.webcamPage_menuGrid_SetProgressText(textbox.Text + " - " + ui_clocktimer_countdown);
                    }
                    ui_clocktimer_countdown--;
                };
                ui_clocktimer.Start();

                DispatcherTimer timer = new DispatcherTimer();
                timer.Interval = new TimeSpan(0, 0, 10);
                timer.Tick += (s, ev) => { timer.Stop(); ui_clocktimer.Stop(); mainwindow.webcamPage_HideProgressUI(); };
                timer.Start();
                #endregion

                if (mainwindow.webcamPage_progressLabel.Visibility == Visibility.Visible)
                    mainwindow.webcamPage_progressLabel.Content = textbox.Text;
                if (mainwindow.webcamPage_menuGrid_progressPanel_progressTextBlock.Visibility == Visibility.Visible)
                    mainwindow.webcamPage_menuGrid_SetProgressText(textbox.Text);

                int counter = 0;
                foreach (RadioButton rbtn in rbtn_stackpanel.Children)
                {
                    if (rbtn.IsChecked == true)
                    {
                        #region Show appropriate progress UI
                        switch (counter)
                        {
                            case 0:
                                {
                                    mainwindow.webcamPage_ShowProgressUI(1);
                                    mainwindow.webcamPage_CloseMenu();
                                    break;
                                }
                            case 1:
                                {
                                    mainwindow.webcamPage_ShowProgressUI(3);
                                    mainwindow.webcamPage_CloseMenu();
                                    break;
                                }
                            case 2:
                                {
                                    mainwindow.webcamPage_ShowProgressUI(4);
                                    mainwindow.webcamPage_OpenMenu();
                                    break;
                                }
                        }
                        #endregion
                    }
                    else
                        counter++;
                }
            }
        }

        private void settingsPage_DebugMenuPage_Home_MessageDialogDebugButton_Click(object sender, RoutedEventArgs e)
        {
            Popups.MessageDialog dlg = new Popups.MessageDialog();
            dlg.Title = "Message Dialog debug";

            #region Content

            StackPanel dlg_contentPanel = new StackPanel();

            TextBlock text0 = new TextBlock
            {
                Margin = new Thickness(5,0,0,0),
                Text = "Choose the style to debug"
            };

            ComboBox styleDropDownButton = new ComboBox()
            {
                Margin = new Thickness(0, 10, 0, 10),
                SelectedIndex = 0
            };

            styleDropDownButton.Items.Add("Windows 10 UWP ContentDialog");
            styleDropDownButton.Items.Add("Windows 8.x MessageDialog");

            TextBlock text1 = new TextBlock()
            {
                Margin = new Thickness(5, 0, 0, 0),
                Text = "Title"
            };

            TextBox titleBox = new TextBox()
            {
                Margin = new Thickness(0, 10, 0, 10),
                Text = "Title"
            };

            TextBlock text2 = new TextBlock()
            {
                Margin = new Thickness(5, 0, 0, 0),
                Text = "Message"
            };

            TextBox messageBox = new TextBox()
            {
                Margin = new Thickness(0, 10, 0, 10),
                AcceptsReturn = true,
                Text = "Message"
            };

            TextBlock text3 = new TextBlock()
            {
                Margin = new Thickness(5, 0, 0, 0),
                Text = "Theme"
            };

            ComboBox themeDropDownButton = new ComboBox()
            {
                Margin = new Thickness(0, 10, 0, 10),
                SelectedIndex = 0
            };

            themeDropDownButton.Items.Add("App theme");
            themeDropDownButton.Items.Add("Light theme");
            themeDropDownButton.Items.Add("Dark theme");

            TextBlock text4 = new TextBlock()
            {
                Margin = new Thickness(5, 0, 0, 0),
                Text = "Right-hand side button text"
            };

            TextBox firstbuttonTextBox = new TextBox()
            {
                Margin = new Thickness(0, 10, 0, 0),
                Text = "OK"
            };

            TextBlock text5 = new TextBlock()
            {
                Margin = new Thickness(5, 0, 0, 0),
                Text = "Left-hands ide button text"
            };

            TextBox secondbuttonTextBox = new TextBox()
            {
                Margin = new Thickness(0, 10, 0, 10),
                Text = ""
            };

            dlg_contentPanel.Children.Add(text0);
            dlg_contentPanel.Children.Add(styleDropDownButton);
            dlg_contentPanel.Children.Add(text1);
            dlg_contentPanel.Children.Add(titleBox);
            dlg_contentPanel.Children.Add(text2);
            dlg_contentPanel.Children.Add(messageBox);
            dlg_contentPanel.Children.Add(text3);
            dlg_contentPanel.Children.Add(themeDropDownButton);
            dlg_contentPanel.Children.Add(text4);
            dlg_contentPanel.Children.Add(firstbuttonTextBox);
            dlg_contentPanel.Children.Add(text5);
            dlg_contentPanel.Children.Add(secondbuttonTextBox);

            dlg.Content = dlg_contentPanel;

            #endregion

            dlg.FirstButtonContent = "Cancel";
            dlg.SecondButtonContent = "Show dialog";

            if (dlg.ShowDialogWithResult() == 1)
            {
                bool? theme;
                if (themeDropDownButton.SelectedIndex == 0)
                    theme = null;
                else if (themeDropDownButton.SelectedIndex == 1)
                    theme = false;
                else
                    theme = true;

                if (styleDropDownButton.SelectedIndex == 0)
                {
                    Popups.MessageDialog dialog = new Popups.MessageDialog();
                    dialog.Title = titleBox.Text;
                    dialog.Content = messageBox.Text;
                    dialog.IsDarkTheme = theme;
                    dialog.FirstButtonContent = firstbuttonTextBox.Text;
                    dialog.SecondButtonContent = secondbuttonTextBox.Text;

                    mainwindow.global_Dim();

                    dialog.ShowDialog();

                    mainwindow.global_UnDim();
                }
                else
                {
                    Popups.MessageDialog_FullWidth dialog = new Popups.MessageDialog_FullWidth();
                    dialog.Title = titleBox.Text;
                    dialog.Content = messageBox.Text;
                    dialog.IsDarkTheme = theme;
                    dialog.FirstButtonContent = firstbuttonTextBox.Text;
                    dialog.SecondButtonContent = secondbuttonTextBox.Text;

                    mainwindow.global_Dim();

                    dialog.ShowDialog();

                    mainwindow.global_UnDim();
                }
            }

        }

        private async void settingsPage_DebugMenuPage_Experiments_ExperimentsButton_Click(object sender, RoutedEventArgs e)
        {
            Popups.MessageDialog_FullWidth dialog = new Popups.MessageDialog_FullWidth();
            dialog.Title = "Experiments";

            // Content //
            StackPanel panel = new StackPanel();

            Label label0 = new Label() { Content = "Toggle experiments on or off" };
            Label label1 = new Label() { Content = "You will have to enter your Prerelease Credentials to apply any changes" };

            // EXPERIMENT - update ui
            CheckBox box0 = new CheckBox() { Content = "Updates UI Testing", Margin = new Thickness(0, 10, 0, 0), Tag = "settings_experiment_UpdateUI" };
            if (Properties.Settings.Default.settings_experiment_UpdateUI) box0.IsChecked = true;
            else box0.IsChecked = false;
            // /EXPERIMENT

            // EXPERIMENT - home overview
            CheckBox box1 = new CheckBox() { Content = "Home Overview", Margin = new Thickness(0, 5, 0, 10), Tag = "home_experiment_Overview" };
            if (Properties.Settings.Default.home_experiment_Overview) box1.IsChecked = true;
            else box1.IsChecked = false;
            // /EXPERIMENT

            panel.Children.Add(label0);
            panel.Children.Add(label1);
            panel.Children.Add(box0);
            panel.Children.Add(box1);
            // Content //

            dialog.Content = panel;

            dialog.FirstButtonContent = "Apply experiments";
            dialog.SecondButtonContent = "Go back";

            // Show dialog
            mainwindow.global_Dim();

            if (dialog.ShowDialogWithResult() == 0)
            {
                #region Save changes

                foreach (object item in panel.Children)
                {
                    if (item.GetType() == (typeof(CheckBox)))
                    {
                        CheckBox box = item as CheckBox;

                        Properties.Settings.Default[(string)box.Tag] = box.IsChecked.Value;
                        Properties.Settings.Default.Save();
                    }
                }

                await mainwindow.GetUserConfiguration(true);
                #endregion
            }

            mainwindow.global_UnDim();
        }

        private async void settingsPage_DebugMenuPage_Configuration_ResetConfigurationButton_Click(object sender, RoutedEventArgs e)
        {
            Popups.MessageDialog_FullWidth dlg = new Popups.MessageDialog_FullWidth();
            dlg.Title = "";
            dlg.Content = "Resetting the configuration will reset all user settings to their defaults.";

            dlg.FirstButtonContent = "Confirm";
            dlg.SecondButtonContent = "Go back";

            mainwindow.global_Dim();

            if (dlg.ShowDialogWithResult() == 0)
            {
                // Reset configuration
                Properties.Settings.Default.Reset();
                Properties.Settings.Default.Save();

                await mainwindow.GetUserConfiguration(true);

                /* no need to restart anymore
                TextMessageDialog("Configuration has been reset...", "We'll restart Webcam Viewer for you...");

                System.Windows.Forms.Application.Restart();
                Application.Current.Shutdown();
                */
            }

            mainwindow.global_UnDim();
        }

        private void settingsPage_DebugMenuPage_Configuration_CustomizationsDeliverySettingsButton_Click(object sender, RoutedEventArgs e)
        {
            mainwindow.SwitchToPage(2);

            // Go to status page the first time

            bool customizationdeliveryPage_visited = false;

            if (customizationdeliveryPage_visited == false)
            {
                Menu_NavigationButton btn = mainwindow.customizationsdeliveryPage_MenuGrid_TopStackPanel.Children[0] as Menu_NavigationButton;
                mainwindow.Menu_NavigationButton_Click(btn, new RoutedEventArgs());

                customizationdeliveryPage_visited = true;
            }
        }

        #endregion
    }
}
