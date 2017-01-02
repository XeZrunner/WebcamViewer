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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WebcamViewer.Pages.Internal_development_page
{
    public partial class Page
    {
        public Page()
        {
            InitializeComponent();
        }

        MainWindow mainwindow = Application.Current.MainWindow as MainWindow;

        private void page_Loaded(object sender, RoutedEventArgs e)
        {
            if (mainwindow.current_page == 4)
            {
                titlebaraccentRectangle.Visibility = Visibility.Collapsed;
                accentOverlayRectangle.Visibility = Visibility.Collapsed;
            }
        }

        private void page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.IsVisible == true)
            {

            }
        }

        private void page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (mainwindow.Width <= 482)
            {
                frame.Margin = new Thickness(0);
                menu_compactWidth = 0;
                DoubleAnimation menuAnim = new DoubleAnimation(menu_compactWidth, TimeSpan.FromSeconds(0.1));
                menuAnim.Completed += (s, ev) => { menuGrid.Visibility = Visibility.Collapsed; };
                menuGrid.BeginAnimation(WidthProperty, menuAnim);

                ThicknessAnimation contentAnim = new ThicknessAnimation(new Thickness(menu_compactWidth, 0, 0, 0), TimeSpan.FromSeconds(.1));
                frame.BeginAnimation(MarginProperty, contentAnim);
            }
            else
            {
                menuGrid.Visibility = Visibility.Visible;
                frame.Margin = new Thickness(48, 0, 0, 0);
                menu_compactWidth = 48;
                DoubleAnimation menuAnim = new DoubleAnimation(menu_compactWidth, TimeSpan.FromSeconds(0.1));
                ThicknessAnimation contentAnim = new ThicknessAnimation(new Thickness(menu_compactWidth, 0, 0, 0), TimeSpan.FromSeconds(.1));
                menuGrid.BeginAnimation(WidthProperty, menuAnim);
                frame.BeginAnimation(MarginProperty, contentAnim);
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

        #region Subpages arrays
        object[] subpages = new object[]
        {
            new Subpages.StatusPage(), // 0
            new Subpages.DefaultCamerasPage(), // 1
            new Subpages.UserConfigurationPage(), // 2
            new Subpages.JSONTestingPage(), //3
            null,
            null,
            new Subpages.DebugLogPage() // 6
        };

        string[] subpages_titles = new string[]
        {
            // everything is all caps
            "STATUS",
            "DEFAULT CAMERAS",
            "USER CONFIGURATION",
            "JSON Testing",
            "",
            "",
            "DEBUG LOG"
        };
        #endregion

        private bool isMenuOpen = false;
        //private bool isLoggedIn = false;

        double menu_compactWidth = 48;
        double menu_expandedWidth = 290;

        private void menuButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isMenuOpen) // open the menu
            {
                DoubleAnimation menuOpen_Anim = new DoubleAnimation(menu_expandedWidth, TimeSpan.FromSeconds(0.3));
                menuGrid.BeginAnimation(WidthProperty, menuOpen_Anim);

                if (!menuGrid.IsVisible)
                    menuGrid.Visibility = Visibility.Visible;

                if (Properties.Settings.Default.ui_theme != 1)
                {
                    menuGrid_darkFilter.Visibility = Visibility.Visible; menuGrid_darkFilter.Opacity = 0;

                    DoubleAnimation menuOpen_darkfilterAnim = new DoubleAnimation(1, TimeSpan.FromSeconds(0.5));
                    menuGrid_darkFilter.BeginAnimation(OpacityProperty, menuOpen_darkfilterAnim);
                }

                isMenuOpen = true;
            }
            else // close the menu
            {
                DoubleAnimation menuClose_Anim = new DoubleAnimation(menu_compactWidth, TimeSpan.FromSeconds(0.3));
                menuClose_Anim.Completed += (s, ev) =>
                {
                    if (mainwindow.Width <= 482)
                        menuGrid.Visibility = Visibility.Collapsed;
                };

                menuGrid.BeginAnimation(WidthProperty, menuClose_Anim);

                if (Properties.Settings.Default.ui_theme != 1)
                {
                    DoubleAnimation menuOpen_darkfilterAnim = new DoubleAnimation(0, TimeSpan.FromSeconds(0.5));
                    menuGrid_darkFilter.BeginAnimation(OpacityProperty, menuOpen_darkfilterAnim);

                    System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
                    timer.Interval = TimeSpan.FromSeconds(0.5);
                    timer.Tick += (s, ev) => { timer.Stop(); menuGrid_darkFilter.Visibility = Visibility.Hidden; };
                    timer.Start();
                }

                isMenuOpen = false;
            }
        }

        /// <summary>
        /// This function is used by all the menu items (excluding the hamburger menu one and a few external action ones) to load the appropriate subpage. It uses the
        /// button's tag to load the appropriately tagged subpage.
        /// </summary>
        private void menuItemButton_Click(object sender, RoutedEventArgs e)
        {
            User_controls.settingsPage_TabButton sBtn = sender as User_controls.settingsPage_TabButton;
            int sBtn_tag = int.Parse((string)sBtn.Tag);

            try
            {
                if (subpages[sBtn_tag] == null)
                {
                    TextMessageDialog_FullWidth("Navigation error", "The requested subpage is null.");
                    return;
                }
                else
                {
                    frame.Navigate(subpages[sBtn_tag]);

                    subpage_title.Text = subpages_titles[sBtn_tag];

                    sBtn.IsActive = true;

                    foreach (FrameworkElement btn in menuGrid_buttonsPanel.Children)
                    {
                        if (btn.GetType() == (typeof(User_controls.settingsPage_TabButton)))
                        {
                            if ((string)btn.Tag != sBtn_tag.ToString())
                            {
                                User_controls.settingsPage_TabButton button = btn as User_controls.settingsPage_TabButton;
                                button.IsActive = false;
                            }
                        }
                    }
                }

                if (isMenuOpen)
                    menuButton_Click(this, new RoutedEventArgs());
            }
            catch (Exception ex)
            {
                TextMessageDialog_FullWidth("Cannot navigate to subpage", "The subpage most likely doesn't exist.\n\nError: " + ex.Message);
            }
        }
    }
}