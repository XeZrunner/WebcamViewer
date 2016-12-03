using System;
using System.Windows;
using System.Windows.Controls;
using WebcamViewer.User_controls;

namespace WebcamViewer.Pages.Settings_page.Subpages._1_Home
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
            foreach (FrameworkElement btn in settingsPage_HomeSettingsPage_MainStackPanel.Children)
            {
                if (btn.Tag != null & (string)btn.Tag != "" & btn.GetType() == (typeof(settingsPage_ToggleSwitchButton)))
                {
                    settingsPage_ToggleSwitchButton button = btn as settingsPage_ToggleSwitchButton;
                    button.IsActive = (bool)Properties.Settings.Default[(string)button.Tag];
                }
            }
        }

        private void page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            settingsPage_HomeSettingsPage_MainStackPanel.MaxWidth = this.ActualWidth - 20;
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

        private void settingsPage_UserInterfacePage_Home_ImageSizingButton_Click(object sender, RoutedEventArgs e)
        {
            Popups.MessageDialog dlg = new Popups.MessageDialog();
            dlg.Title = "";
            dlg.Content_DisableMargin = true;
            dlg.FirstButtonContent = "Go back";
            dlg.SecondButtonContent = "Accept";

            var dialogcontrol = new Controls.settingsPage_HomePage_ImageSizingDialogControl();

            dlg.Content = dialogcontrol;

            if (dlg.ShowDialogWithResult() == 1)
            {
                // save and close

                string s = "uniform";

                foreach (RadioButton btn in dialogcontrol.mainStackPanel.Children)
                {
                    if (btn.IsChecked == true)
                        s = btn.Tag as string;
                }

                Properties.Settings.Default.home_imagesizing = s;
                Properties.Settings.Default.Save();

                mainwindow.GetUserConfiguration(true);
            }
        }
    }
}