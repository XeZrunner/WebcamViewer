using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WebcamViewer.Pages.Settings_page
{
    public partial class Page
    {
        public Page()
        {
            InitializeComponent();

            SubViewIn_Anim = (Storyboard)FindResource("SubViewIn");
            SubViewOut_Anim = (Storyboard)FindResource("SubViewOut");
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Splash!
            ShowSplash();
            await async_Wait(TimeSpan.FromSeconds(.3));
            HideSplash();
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateView();
        }

        MainWindow mainwindow = Application.Current.MainWindow as MainWindow;

        #region Storyboards

        Storyboard SubViewIn_Anim;
        Storyboard SubViewOut_Anim;

        #endregion

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

        #region View

        public enum ViewModes
        {
            Desktop,
            Mobile
        }

        public void UpdateView(ViewModes? manualOverride = null)
        {
            if (manualOverride == null)
            {
                if (this.ActualWidth <= 482)
                {
                    UpdateView(ViewModes.Mobile);
                }
                else
                {
                    UpdateView(ViewModes.Desktop);
                }
            }
            else
            {
                if (manualOverride == ViewModes.Mobile)
                {
                    foreach (Controls.SettingsItemControl item in view_desktopgrid_WrapPanel.Children)
                        item.ViewMode = Controls.SettingsItemControl.ViewModes.Mobile;

                    view_desktopgrid_WrapPanel.HorizontalAlignment = HorizontalAlignment.Stretch;
                    view_desktopgrid_WrapPanel.VerticalAlignment = VerticalAlignment.Stretch;
                    view_desktopgrid_WrapPanel.Orientation = Orientation.Vertical;
                    view_desktopgrid_WrapPanel.ItemWidth = mainwindow.Width - 2;
                }
                else
                {
                    foreach (Controls.SettingsItemControl item in view_desktopgrid_WrapPanel.Children)
                        item.ViewMode = Controls.SettingsItemControl.ViewModes.Desktop;

                    view_desktopgrid_WrapPanel.HorizontalAlignment = HorizontalAlignment.Center;
                    view_desktopgrid_WrapPanel.VerticalAlignment = VerticalAlignment.Center;
                    view_desktopgrid_WrapPanel.Orientation = Orientation.Horizontal;
                    view_desktopgrid_WrapPanel.ItemWidth = Double.NaN;
                }

            }
        }

        public void ShowSplash()
        {
            splashGrid.Visibility = Visibility.Visible;

            DoubleAnimation anim = new DoubleAnimation(1, TimeSpan.FromSeconds(.3));
            splashGrid.BeginAnimation(OpacityProperty, anim);
        }

        public void HideSplash()
        {
            DoubleAnimation anim = new DoubleAnimation(0, TimeSpan.FromSeconds(.3));
            anim.Completed += (s, ev) => { splashGrid.Visibility = Visibility.Collapsed; };
            splashGrid.BeginAnimation(OpacityProperty, anim);
        }

        async Task async_Wait(TimeSpan timeToWait)
        {
            await Task.Delay(timeToWait);
        }

        #endregion

        #region Engine

        object currentSettingPage;
        int? currentSettingPageID;
        object[] SettingsPagesInMem;

        #region string[] SettingsPages_Titles
        string[] SettingsPages_Titles = new string[]
        {
            "Webcam editor",
            "Camera settings",
            "User interface",
            "About & updates",
            "Debug settings",
            "Internal settings",
            "Ripple debug",
            null,
            null,
            "Samples"
        };
        #endregion

        void LoadSettingsPageIntoMemory(int ID)
        {
            // This should be efficient enough, minor lags when loading the pages for the first time ever,
            // but the pages will be preserved in memory.

            if (SettingsPagesInMem == null)
                SettingsPagesInMem = new object[view_desktopgrid_WrapPanel.Children.Count];

            switch (ID)
            {
                case 0:
                    {
                        if (SettingsPagesInMem[ID] == null)
                            currentSettingPage = new Subpages._0_Webcams.Page();
                        else
                            currentSettingPage = SettingsPagesInMem[ID];
                        break;
                    }
                case 1:
                    {
                        if (SettingsPagesInMem[ID] == null)
                            currentSettingPage = new Subpages._1_Home.Page();
                        else
                            currentSettingPage = SettingsPagesInMem[ID]; break;
                    }
                case 2:
                    {
                        if (SettingsPagesInMem[ID] == null)
                            currentSettingPage = new Subpages._2_User_Interface.Page();
                        else
                            currentSettingPage = SettingsPagesInMem[ID]; break;
                    }
                case 3:
                    {
                        if (SettingsPagesInMem[ID] == null)
                            currentSettingPage = new Subpages._3_About_and_updates.Page();
                        else
                            currentSettingPage = SettingsPagesInMem[ID]; break;
                    }
                case 4:
                    {
                        if (SettingsPagesInMem[ID] == null)
                            currentSettingPage = new Subpages._4_Debug_settings.Page();
                        else
                            currentSettingPage = SettingsPagesInMem[ID]; break;
                    }
                case 5:
                    {
                        if (SettingsPagesInMem[ID] == null)
                            currentSettingPage = new Internal_development_page.Page();
                        else
                            currentSettingPage = SettingsPagesInMem[ID]; break;
                    }
                case 6:
                    {
                        if (SettingsPagesInMem[ID] == null)
                            currentSettingPage = new XeZrunner.UI.ControlEffects.RippleDrawable() { Color = Application.Current.Resources["accentcolor_dark"] as SolidColorBrush, FillColor = Application.Current.Resources["settingsPage_foregroundSecondary3"] as SolidColorBrush };
                        else
                            currentSettingPage = SettingsPagesInMem[ID]; break;
                    }
                case 9:
                    {
                        if (SettingsPagesInMem[ID] == null)
                            currentSettingPage = new Subpages._5_Samples.Page();
                        else
                            currentSettingPage = SettingsPagesInMem[ID]; break;
                    }
            }

            if (SettingsPagesInMem[ID] == null)
                SettingsPagesInMem[ID] = currentSettingPage;

            currentSettingPageID = ID;
        }

        #endregion

        #region Debug buttons

        private void mobileViewButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateView(ViewModes.Mobile);
        }

        private void desktopViewButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateView(ViewModes.Desktop);
        }

        private void debugButton_Click(object sender, RoutedEventArgs e)
        {
            Popups.MessageDialog dialog = new Popups.MessageDialog();
            dialog.Title = "Settings debug";
            dialog.FirstButtonContent = "Close";

            #region Create content
            StackPanel panel = new StackPanel();

            Label lbl0 = new Label() { Content = String.Format("Number of tiles: {0}", view_desktopgrid_WrapPanel.Children.Count) };
            Label lbl1 = new Label() { Content = "Number of pages loaded into memory: " };
            Label lbl2 = new Label() { Content = "Last visited page: " };

            Label lbl_sep0 = new Label() { Content = "---------------------------------------------", Margin = new Thickness(0, 5, 0, 5) };

            User_controls.settingsPage_NormalButton btn0 = new User_controls.settingsPage_NormalButton() { Text = "Unload last visited page", HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0, 10, 0, 0) };
            User_controls.settingsPage_NormalButton btn1 = new User_controls.settingsPage_NormalButton() { Text = "Clear all pages loaded into memory", HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0, 5, 0, 0) };
            User_controls.settingsPage_NormalButton btn2 = new User_controls.settingsPage_NormalButton() { Text = "Hide debug buttons", HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0, 5, 0, 0) };

            panel.Children.Add(lbl0);
            panel.Children.Add(lbl1);
            panel.Children.Add(lbl2);
            panel.Children.Add(lbl_sep0);

            panel.Children.Add(btn0);
            panel.Children.Add(btn1);
            panel.Children.Add(btn2);

            dialog.Content = panel;
            #endregion

            #region Do stuff

            btn0.Click += (s, ev) =>
            {
                currentSettingPage = null;
                lbl2.Content = "Last visited page: null";
            };

            btn1.Click += (s, ev) =>
            {
                SettingsPagesInMem = null;
                lbl1.Content = "Number of pages loaded into memory: 0";
            };

            btn2.Click += (s, ev) =>
            {
                if (debugButton.IsVisible) {

                    debugToolbar.Visibility = Visibility.Collapsed;

                    btn2.Text = "Show debug buttons"; }

                else {

                    debugToolbar.Visibility = Visibility.Visible;

                    btn2.Text = "Hide debug buttons"; }
            };

            int inmemPagesCounter = 0;
            if (SettingsPagesInMem != null)
            {
                foreach (object page in SettingsPagesInMem)
                {
                    if (page != null)
                        inmemPagesCounter++;
                }
            }
            string lbl1content = lbl1.Content as string;
            lbl1content += inmemPagesCounter;
            lbl1.Content = lbl1content;

            string lbl2content = lbl2.Content as string;
            if (currentSettingPageID != null)
                lbl2content += SettingsPages_Titles[currentSettingPageID.Value];
            else
                lbl2content += "null";
            lbl2.Content = lbl2content;

            #endregion

            // ---------------

            dialog.ShowDialog();
        }

        #endregion

        private void view_subpagegrid_backButton_Click(object sender, RoutedEventArgs e)
        {
            SubViewOut_Anim.Begin();
        }

        private void SettingsItemButton_Click(object sender, RoutedEventArgs e)
        {
            Controls.SettingsItemControl sButton = sender as Controls.SettingsItemControl;

            int sButton_Tag = int.Parse((string)sButton.Tag);

            LoadSettingsPageIntoMemory(sButton_Tag);

            try
            {
                view_desktop_subpagegrid_Frame.Navigate(currentSettingPage);
                view_subpagegrid_activeSubpageLabel.Content = SettingsPages_Titles[sButton_Tag].ToUpper();
            }
            catch (Exception ex)
            {
                TextMessageDialog("Could not load subpage", "Make sure the button's Tag is a correct page ID.\n\nError: " + ex.Message + "\nButton tag: " + sButton.Tag.ToString());

                return;
            }

            SubViewIn_Anim.Begin();
        }
    }
}