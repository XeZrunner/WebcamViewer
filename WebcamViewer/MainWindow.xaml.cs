using MahApps.Metro.Controls;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WebcamViewer
{
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            webcamPage_menuCameraList.Height = webcamPage_menu.Height - 170;

            webcamimagePage.Visibility = Visibility.Visible;

            saveFileDialog.Filter = "JPG image (*.jpg)|*.jpg|All files (*.*)|*.*";
            saveFileDialog.Title = "Save image";
            saveFileDialog.DefaultExt = "jpg";

            settingsPage_UserInterfacePage_ImageBlurToggleButton_Toggle.OnSwitchBrush.Opacity = 0.7;

            if (!DISABLE_WEBCAMEDITOR)
                settingsPage_MainPage_WebcamEditorButton_Click(this, new RoutedEventArgs());
            else
                settingsPage_MainPage_UserInterfaceButton_Click(this, new RoutedEventArgs());

            archivebrowser.ProgressChanged += archivebrowser_ProgressChanged;
            archivebrowser.DocumentCompleted += Archivebrowser_DocumentCompleted;

            // Command line arguments
            string[] args = Environment.GetCommandLineArgs();
            foreach (string arg in args)
            {
                if (arg == "-readonly")
                    READONLY_MODE = true;
                if (arg == "-disable_archive")
                    DISABLE_ARCHIVEORG = true;
                if (arg == "-disable_localsave")
                    DISABLE_LOCALSAVE = true;
                if (arg == "-disable_webcameditor")
                    DISABLE_WEBCAMEDITOR = true;
            }

            // Check if we have MDL2 Assets installed
            using (System.Drawing.Font font = new System.Drawing.Font("Segoe MDL2 Assets", 12))
            {
                if (font.Name != "Segoe MDL2 Assets")
                {
                    this.Show(); webcamimagePage.Visibility = Visibility.Collapsed;
                    TextMessageDialog("Can't find Segoe MDL2 Assets", "It seems that you didn't install the Segoe MDL2 Assets font that is needed to display icons in the app.\n"
                        + "The font is included in the zip that you download from GitHub.\n"
                        + "Open the font file named \"Segoe MDL2 Assets.ttf\", then click Install to install it.", true);
                    Application.Current.Shutdown();
                }
            }

#if !DEBUG
            webcamPage_menu_debugmenuButton.Visibility = Visibility.Collapsed;
            settingsPage_MainPage_DefaultConfigDebugButton.Visibility = Visibility.Collapsed;
#endif

            if (UPDATE_AVAIL_FEED)
                infoButton.Visibility = Visibility.Visible;

        }

        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            // Reset configuration
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
                if (Keyboard.IsKeyDown(Key.LeftShift))
                {
                    ResetSettingsWindow wnd = new ResetSettingsWindow(true);
                    wnd.Owner = this; webcamimagePage.Visibility = Visibility.Collapsed; wnd.Show();
                    wnd.Closed += (s, ev) => { SwitchToPage(0); }; // in the case of canceling the window
                }

            #region :)
            if (DateTime.Now.Month == 10 && DateTime.Now.Day == 12)
            {
                if (Properties.Settings.Default.birthday_notice == false)
                {
                    webcamimagePage.Visibility = Visibility.Collapsed;
                    TextMessageDialog("Let's sing Happy birthday!",
                        "Today's the birthday of Felix, the creator of Webcam Viewer.\nHappy birthday, myself! :D");
                    SetTitlebarColor(Color.FromRgb(0, 103, 179)); // blue's my favorite
                    SwitchToPage(0);

                    Properties.Settings.Default.birthday_notice = true; Properties.Settings.Default.Save(); // notify only once
                }
            }
            else
            {
                Properties.Settings.Default.birthday_notice = false; Properties.Settings.Default.Save();
            }
            #endregion

            GetUserCameras();

            GetUserSettings();

            if (SystemParameters.PrimaryScreenWidth == 800 && SystemParameters.PrimaryScreenHeight == 600)
            {
                this.Width = 640;
                this.Height = 560;
            }

            if (SystemParameters.PrimaryScreenWidth <= 640 && SystemParameters.PrimaryScreenHeight <= 480)
            {
                TextMessageDialog("Low screen resolution", "Your resolution is lower than the minimum requirement 800x600.\nPlease increase your resolution for the best experience.", true);
                this.Width = 320;
                this.Height = 320;
            }

            // disable stuff
            if (READONLY_MODE)
            {
                webcamPage_savePanel.Visibility = Visibility.Collapsed;
                webcamPage_menu_infoandactionsGrid.Height -= 37;

                infoButton.Visibility = Visibility.Visible;
            }
            if (DISABLE_ARCHIVEORG)
            {
                webcamPage_saveimageonarchiveButton.Visibility = Visibility.Collapsed;
                webcamPage_saveallButton.Visibility = Visibility.Collapsed;

                infoButton.Visibility = Visibility.Visible;
            }
            if (DISABLE_LOCALSAVE)
            {
                webcamPage_saveimageButton.Visibility = Visibility.Collapsed;
                webcamPage_saveallButton.Visibility = Visibility.Collapsed;

                infoButton.Visibility = Visibility.Visible;
            }
            if (DISABLE_WEBCAMEDITOR)
            {
                settingsPage_MainPage_WebcamEditorButton.Visibility = Visibility.Collapsed;

                infoButton.Visibility = Visibility.Visible;
            }

            if (Properties.Settings.Default.defaultconfig_heartbeat)
                Configuration.DefaultConfig_Heartbeat();

            SetAeroBorder();

            CenterWindowOnScreen();
        }

        #region Variables & class declarations

        bool UI_SLOW_MOTION = false;
        bool UI_HOME_BLURIMAGE = true;
        bool UI_AUTOSIZE_WINDOW = false;
        bool UI_AEROBORDER = false;

        bool READONLY_MODE = false;
        bool DISABLE_ARCHIVEORG = false;
        bool DISABLE_LOCALSAVE = false;
        bool DISABLE_WEBCAMEDITOR = false;

        bool UPDATE_AVAIL_FEED = false;

        bool UI_SETTINGSPAGE_SHOWACCENTCOLOR = false;

        int SETTINGS_LAST_CAMERA = 0;

        int CURRENT_PAGE = 0;

        Configuration Configuration = new Configuration();

        Properties.Settings Settings = Properties.Settings.Default;

        #endregion

        #region Configuration

        void GetUserSettings()
        {
            UI_HOME_BLURIMAGE = Properties.Settings.Default.blur_image;
            UI_AUTOSIZE_WINDOW = Properties.Settings.Default.window_autosize;
            UI_AEROBORDER = Properties.Settings.Default.window_aeroborder;
            UI_SETTINGSPAGE_SHOWACCENTCOLOR = Properties.Settings.Default.settings_showaccentcolor;

            // accent color
            SetAccentColor(Properties.Settings.Default.accentcolor);

            // aero border
            SetAeroBorder();

            if (Properties.Settings.Default.home_menu_blurbehind == false)
                webcamPage_menu_blurbehindBorder.Visibility = Visibility.Collapsed;
            else
                webcamPage_menu_blurbehindBorder.Visibility = Visibility.Visible;

            UI_SETTINGSPAGE_SHOWACCENTCOLOR = Properties.Settings.Default.settings_showaccentcolor;

            #region Image sizing

            switch (Properties.Settings.Default.imagesizing)
            {
                case 0:
                    {
                        webcamimage.Stretch = Stretch.None;

                        break;
                    }
                case 1:
                    {
                        webcamimage.Stretch = Stretch.Fill;

                        break;
                    }
                case 2:
                    {
                        webcamimage.Stretch = Stretch.Uniform;

                        break;
                    }
                case 3:
                    {
                        webcamimage.Stretch = Stretch.UniformToFill;

                        break;
                    }
            }

            #endregion
        }

        bool GetUserCameras()
        {
            webcamPage_menuCameraList.Children.Clear();

            int counter = 0;

            foreach (string camera in Properties.Settings.Default.camera_names)
            {
                Button button = new Button(); button.Content = camera; button.Style = this.Resources["MenuCameraButtonStyle"] as Style; button.Click += WebcamButton_Click;
                webcamPage_menuCameraList.Children.Add(button);
                counter++;
            }

            if (counter == 0)
            {
                TextMessageDialog("No cameras found...", "You don't have any cameras set up. Please set up at least 1 camera to use this application."); SwitchToPage(1); settingsPage_MainPage_WebcamEditorButton_Click(this, new RoutedEventArgs()); settingsPage_WebcamEditorPage_ActionBar_moreButton_Click(this, new RoutedEventArgs()); webcamPage_ActionBarCameraNameLabel.Content = "";
                return false;
            }
            else
            {
                if (!Keyboard.IsKeyDown(Key.LeftAlt)) try { LoadImage(Properties.Settings.Default.camera_names[SETTINGS_LAST_CAMERA]); } catch { SETTINGS_LAST_CAMERA = 0; LoadImage(Properties.Settings.Default.camera_names[SETTINGS_LAST_CAMERA]); }
                return true;
            }
        }

        #endregion

        #region Window, titlebar & appearance

        private void window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            debugoverlay_windowsize.Content = "Window size: " + this.Width + "x" + this.Height
                + " (" + this.Width + "x" + (this.Height - titlebarGrid.Height - debugmenuPage_ActionBar.Height) + " without UI elements)";
            if (webcamPage_menu_debugmenuButton.Visibility != Visibility.Visible)
                webcamPage_menuCameraList.Height = this.Height - 45 - 90 - 40;
            else
                webcamPage_menuCameraList.Height = this.Height - 45 - 90 - 40 - 40;

            // Make the maximize button change if we Aero snap or keyboard maximize or whatever else
            if (WindowState == WindowState.Maximized)
                maximizeButton.Content = "\ue923";
            else
                maximizeButton.Content = "\ue922";
        }

        void SetAeroBorder()
        {
            if (Properties.Settings.Default.window_aeroborder)
            {
                this.BorderBrush = SystemParameters.WindowGlassBrush;
                this.GlowBrush = SystemParameters.WindowGlassBrush;
            }
            else
            {
                this.BorderBrush = this.Resources["res_accentBackground"] as SolidColorBrush;
                this.GlowBrush = this.Resources["res_accentBackground"] as SolidColorBrush;
            }
        }

        void SetAccentColor(int accentcolor)
        {
            SolidColorBrush backgroundcolor = (SolidColorBrush)FindResource("res_Background" + accentcolor.ToString());
            SolidColorBrush foregroundcolor = (SolidColorBrush)FindResource("res_Foreground" + accentcolor.ToString());

            if (accentcolor == 5)
            {
                backgroundcolor = new SolidColorBrush(AccentColorSet.ActiveSet["SystemAccent"]);
                foregroundcolor = SystemParameters.WindowGlassBrush as SolidColorBrush;
            }

            this.Resources["res_accentBackground"] = new SolidColorBrush(backgroundcolor.Color);
            this.Resources["res_accentForeground"] = new SolidColorBrush(foregroundcolor.Color);

            int lastSettingsPage = 0;

            if (settingsPage_WebcamEditorPage.Visibility == Visibility.Visible) lastSettingsPage = 0;
            if (settingsPage_UserInterfacePage.Visibility == Visibility.Visible) lastSettingsPage = 1;
            if (settingsPage_AboutPage.Visibility == Visibility.Visible) lastSettingsPage = 2;
            if (settingsPage_DefaultConfigurationDebugPage.Visibility == Visibility.Visible) lastSettingsPage = 3;

            settingsPage_SetActiveButton(lastSettingsPage);

            settingsPage_UserInterfacePage_AccentColorEditorButton_DescriptionLabel.Content = "Current color: " + colorDefinitions[Properties.Settings.Default.accentcolor];
        }

        private void CenterWindowOnScreen()
        {
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;
            double windowWidth = this.Width;
            double windowHeight = this.Height;
            this.Left = (screenWidth / 2) - (windowWidth / 2);
            this.Top = (screenHeight / 2) - (windowHeight / 2);
        }

        #region Titlebar

        int titlebarGrid_doubleclickcounter = 0;

        private void SetTitlebarColor(Color backgroundcolor, bool blackforegroundcolor = false) // legacy
        {
            titlebarGrid.Background = new SolidColorBrush(backgroundcolor);
            if (blackforegroundcolor)
            {
                titlebarGrid_titleLabel.Foreground = new SolidColorBrush(Colors.Black);
                // title text
                closeButton.Foreground = new SolidColorBrush(Colors.Black);
                maximizeButton.Foreground = new SolidColorBrush(Colors.Black);
                minimizeButton.Foreground = new SolidColorBrush(Colors.Black);
            }
            else
            {
                titlebarGrid_titleLabel.Foreground = new SolidColorBrush(Colors.White);
                // title text
                closeButton.Foreground = new SolidColorBrush(Colors.White);
                maximizeButton.Foreground = new SolidColorBrush(Colors.White);
                minimizeButton.Foreground = new SolidColorBrush(Colors.White);
            }
        }

        private void titlebarGrid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();

                // double click titlebar to maximize/restore
                titlebarGrid_doubleclickcounter++;

                if (titlebarGrid_doubleclickcounter == 2)
                    maximizeButton_Click(this, new RoutedEventArgs());

                DispatcherTimer timer = new DispatcherTimer();
                timer.Interval = new TimeSpan(0, 0, 0, 0, 500);
                timer.Tick += (s, ev) => { titlebarGrid_doubleclickcounter = 0; timer.Stop(); };
                timer.Start();
            }
        }

        private void titlebarGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Thickness newMargin = new Thickness(0, e.NewSize.Height, 0, 0);

            webcamimagePage_ContentGrid.Margin = newMargin;
            settingsPage_ContentGrid.Margin = newMargin;
            debugmenuPage_ContentGrid.Margin = newMargin;
        }

        // ----- Context menu ----- //

        private void titlebarGrid_contextmenu_ResetWindowSize_Click(object sender, RoutedEventArgs e)
        {
#if DEBUG
            // Result dialog debug
            Popups.MessageDialog dlg = new Popups.MessageDialog();
            dlg.Title = "Are you sure you want to reset the window size?";
            dlg.Content = "The window size will be reset to 800x675.\nContinue?";
            dlg.FirstButtonContent = "Continue";
            dlg.SecondButtonContent = "Cancel";

            if (dlg.ShowDialogWithResult() == 1)
                return;
#endif

            this.Width = 800;
            this.Height = 600 + 30 + 45;
            CenterWindowOnScreen();
        }

        private void titlebarGrid_contextmenu_SetImageSizeForWindow_Click(object sender, RoutedEventArgs e)
        {
            if (bimage != null)
            {
                this.Width = bimage.PixelWidth;
                this.Height = bimage.PixelHeight + 30 + 45;
                CenterWindowOnScreen();

            }
            else
                TextMessageDialog("Cannot set window size", "There's no image loaded.");
        }

        // ----- Window controls ----- //

        int debugmenuPage_PreviousPage = 0;

        private void titlebar_backButton_Click(object sender, RoutedEventArgs e)
        {
            if (CURRENT_PAGE == 0)
            {
                if (webcamPage_menu.Opacity == 1d)
                {
                    var animation = (Storyboard)FindResource("webcamPage_MenuClose");
                    if (UI_SLOW_MOTION) animation.SpeedRatio = 0.15;
                    animation.Begin();

                    titlebar_backButton.Visibility = Visibility.Collapsed;
                }
            }

            if (CURRENT_PAGE == 1)
            {
                SwitchToPage(0);

                GetUserCameras();
                GetUserSettings();

                //if (webcamPage_menu.Visibility != Visibility.Visible)
                //    titlebar_backButton.Visibility = Visibility.Collapsed;
            }

            if (CURRENT_PAGE == 2)
            {
                SwitchToPage(debugmenuPage_PreviousPage);

                //if (debugmenuPage_PreviousPage == 0)
                //{
                //    if (webcamPage_menu.Visibility != Visibility.Visible)
                //        titlebar_backButton.Visibility = Visibility.Collapsed;
                //}
                //else
                //    titlebar_backButton.Visibility = Visibility.Visible;
            }
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void maximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                if (logviewerGrid.Visibility == Visibility.Collapsed)
                    logviewerGrid.Visibility = Visibility.Visible;
                else
                    logviewerGrid.Visibility = Visibility.Collapsed;

                return;
            }

            switch (this.WindowState)
            {
                case WindowState.Normal:
                    {
                        this.WindowState = WindowState.Maximized;
                        maximizeButton.Content = "\ue923";
                        break;
                    }
                case WindowState.Maximized:
                    {
                        this.WindowState = WindowState.Normal;
                        maximizeButton.Content = "\ue922";
                        break;
                    }
            }
        }

        private void minimizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftShift) & Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                if (webcamimagePage.Visibility == Visibility.Visible)
                    debugmenuPage_PreviousPage = 0;
                if (settingsPage.Visibility == Visibility.Visible)
                    debugmenuPage_PreviousPage = 1;

                SwitchToPage(2);
                return;
            }

            if (Keyboard.IsKeyDown(Key.LeftShift))
            {
                if (webcamPage_menu_debugmenuButton.Visibility == Visibility.Collapsed)
                    webcamPage_menu_debugmenuButton.Visibility = Visibility.Visible;
                else
                    webcamPage_menu_debugmenuButton.Visibility = Visibility.Collapsed;

                return;
            }

            this.WindowState = WindowState.Minimized;
        }

        private void infoButton_Click(object sender, RoutedEventArgs e)
        {
            if (READONLY_MODE | DISABLE_ARCHIVEORG | DISABLE_LOCALSAVE | DISABLE_WEBCAMEDITOR)
                TextMessageDialog("Certain functions have been disabled.", "You've probably used one or more of the command line switches that disable certain things in the program.\n" +
                    "READONLY: " + READONLY_MODE + "\n" +
                    "DISABLE_ARCHIVEORG: " + DISABLE_ARCHIVEORG + "\n" +
                    "DISABLE_LOCALSAVE: " + DISABLE_LOCALSAVE + "\n" +
                    "DISABLE_WEBCAMEDITOR: " + DISABLE_WEBCAMEDITOR);
            if (UPDATE_AVAIL_FEED)
            {
                TextMessageDialog("UPDATE_AVAIL_FEED", "");
            }
        }

        private void weatherButton_Click(object sender, RoutedEventArgs e)
        {
            if (webcamPage_weatherGrid.Visibility == Visibility.Collapsed)
            {
                Storyboard board = (Storyboard)FindResource("home_weather_in");
                if (UI_SLOW_MOTION) board.SpeedRatio = 0.10;
                board.Begin();
            }
            else
            {
                Storyboard board = (Storyboard)FindResource("home_weather_out");
                if (UI_SLOW_MOTION) board.SpeedRatio = 0.10;
                board.Begin();
            }
        }

        // ----- ----- //

        #endregion

        #endregion

        #region Dialogs

        void TextMessageDialog(string Title, string Content, bool DarkMode = false)
        {

            /*

            MessagedialogWindow dialog = null;
            bool DarkListnerNeeded = true;

            if (webcamimagePage.Visibility == Visibility.Visible)
            {
                dialog = new MessagedialogWindow(Title, Message, true);
                DarkListnerNeeded = false;
            }

            if (DarkListnerNeeded)
            {
                if (DarkMode == true)
                    dialog = new MessagedialogWindow(Title, Message, true);
                else
                    dialog = new MessagedialogWindow(Title, Message);
            }

            dialog.Owner = this;
            dialog.ShowDialog();

            */


            var dialog = new Popups.MessageDialog();

            dialog.Title = Title;
            dialog.Content = Content;

            dialog.ShowDialog();
        }

        #endregion

        #region Page UI logic

        void SwitchToPage(int page)
        {
            /// Pages
            /// 0: Webcam image
            /// 1: Settings
            /// 2: Debug menu
            /// 3: Feedback

            switch (page)
            {
                case 0:
                    {
                        if (webcamimagePage.Visibility == Visibility.Collapsed)
                        {
                            var animation = (Storyboard)FindResource("webcamPage_In");
                            if (UI_SLOW_MOTION) animation.SpeedRatio = 0.15;
                            animation.Begin();
                        }

                        CURRENT_PAGE = 0;
                        titlebar_backButton.Visibility = Visibility.Collapsed;
                        titlebar_backButton_Click(this, new RoutedEventArgs());

                        break;
                    }
                case 1:
                    {
                        if (settingsPage.Visibility == Visibility.Collapsed)
                        {
                            var board = (Storyboard)FindResource("settingsPage_In");
                            if (UI_SLOW_MOTION) board.SpeedRatio = 0.15;
                            board.Begin();

                            settingsPage_UpdateTitlebarState();
                        }

                        CURRENT_PAGE = 1;
                        titlebar_backButton.Visibility = Visibility.Visible;

                        break;
                    }
                case 2:
                    {
                        if (debugmenuPage.Visibility == Visibility.Collapsed)
                        {
                            var animation = (Storyboard)FindResource("debugmenuPage_In");
                            if (UI_SLOW_MOTION) animation.SpeedRatio = 0.15;
                            animation.Begin();
                        }

                        CURRENT_PAGE = 2;
                        titlebar_backButton.Visibility = Visibility.Visible;

                        break;
                    }
            }
        }

        #endregion

        #region Home page

        #region Webcam logic

        Uri currentcameraUrl;
        BitmapImage bimage;

        bool InitialProgressRingDone = false;

        void LoadImage(string cameraname)
        {
            // Show progress UI
            ShowProgressUI(0);

            foreach (string camera in Properties.Settings.Default.camera_names)
            {
                if (camera == cameraname)
                {
                    bimage = new BitmapImage(new Uri(Properties.Settings.Default.camera_urls[Properties.Settings.Default.camera_names.IndexOf(cameraname)] + "?&dummy=" + DateTime.Now.Hour + DateTime.Now.Minute + DateTime.Now.Second));
                    bimage.DownloadCompleted += (s, e) =>
                    {
                        if (webcamimage.Visibility != Visibility.Visible)
                            webcamimage.Visibility = Visibility.Visible;

                        webcamimage.Source = bimage;

                        currentcameraUrl = new Uri(Properties.Settings.Default.camera_urls[Properties.Settings.Default.camera_names.IndexOf(cameraname)]);
                        webcamPage_ActionBarCameraNameLabel.Content = cameraname;
                        webcamPage_MenuCameraNameLabel.Text = cameraname.ToUpper();
                        webcamPage_MenuCameraUrlLabel.Text = currentcameraUrl.ToString();

                        // Hide progress UI, some kind of bug aswell

                        HideProgressUI();

                        if (InitialProgressRingDone == false)
                        {
                            InitialProgressRingDone = true;
                            webcamPage_progressring.Visibility = Visibility.Collapsed;
                        }

                        if (UI_AUTOSIZE_WINDOW)
                            titlebarGrid_contextmenu_SetImageSizeForWindow_Click(this, new RoutedEventArgs());

                        // Debug overlay
                        debugoverlay_cameraname.Content = "Camera name: " + cameraname;
                        debugoverlay_cameraurl.Content = "Camera url: " + currentcameraUrl;
                        debugoverlay_cameraresolution.Content = "Image resolution: " + bimage.PixelWidth + "x" + bimage.PixelHeight;
                    };
                    bimage.DownloadFailed += (s, ev) =>
                    {
                        TextMessageDialog("Could not load image", "An error occured while trying to load the webcam image...\n" + ev.ErrorException.Message);
                        webcamimage.Visibility = Visibility.Hidden;
                        HideProgressUI();

                        currentcameraUrl = new Uri(Properties.Settings.Default.camera_urls[Properties.Settings.Default.camera_names.IndexOf(cameraname)]);
                        webcamPage_ActionBarCameraNameLabel.Content = cameraname;
                        webcamPage_MenuCameraNameLabel.Text = "Error";
                        webcamPage_MenuCameraUrlLabel.Text = "http://www.somethinghappened.com";

                        webcamPage_ActionBar_menuButton_Click(this, new RoutedEventArgs());
                    };
                    break;
                }
            }
        }

        // ----- Image saving ----- //

        SaveFileDialog saveFileDialog = new SaveFileDialog();

        #region Save image

        // ----- IMAGE SAVING ----- start

        WebClient Client = new WebClient();

        string whereToDownload;
        Uri UriToDownload;

        void SaveImageFile()
        {
            ShowProgressUI(2); webcamPage_progresslabel.Content = "preparing to download image...";
            if (UI_HOME_BLURIMAGE) webcamimageBlur.Radius = 10;

            Thread thread = new Thread(() =>
            {
                WebClient client = new WebClient();
                client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
                client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);
                client.DownloadFileAsync(UriToDownload, whereToDownload);
            });
            thread.Start();
        }

        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn / totalBytes * 100;
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                if (Keyboard.IsKeyDown(Key.LeftShift))
                    webcamPage_progresslabel.Content = "downloading image: " + e.ProgressPercentage + "%";
                else
                    webcamPage_progresslabel.Visibility = Visibility.Collapsed;
                webcamPage_progressbar.IsIndeterminate = false;
                webcamPage_progressbar.Maximum = e.TotalBytesToReceive;
                webcamPage_progressbar.Value = e.BytesReceived;
            }));
        }
        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                HideProgressUI();
                webcamimageBlur.Radius = 0;
                //refreshtimer.Start();

                if (e.Error != null)
                {
                    TextMessageDialog("Cannot download image", "An error occured while trying to download the image.\nError: " + e.Error.Message);
                    File.Delete(whereToDownload);
                }
            }));
        }

        // ----- IMAGE SAVING ----- end

        #endregion

        System.Windows.Forms.WebBrowser archivebrowser = new System.Windows.Forms.WebBrowser();

        private void archivebrowser_ProgressChanged(object sender, System.Windows.Forms.WebBrowserProgressChangedEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftShift))
                webcamPage_progresslabel.Visibility = Visibility.Visible;
            else
                webcamPage_progresslabel.Visibility = Visibility.Collapsed;
            webcamPage_progresslabel.Content = "saving to archive.org... (" + e.CurrentProgress + " / " + e.MaximumProgress + ")";
            webcamPage_progressbar.IsIndeterminate = false;
            webcamPage_progressbar.Maximum = e.MaximumProgress;
            webcamPage_progressbar.Value = e.CurrentProgress;
        }

        private void Archivebrowser_DocumentCompleted(object sender, System.Windows.Forms.WebBrowserDocumentCompletedEventArgs e)
        {
            webcamPage_progresslabel.Content = "saved...";
            HideProgressUI();
            webcamimageBlur.Radius = 0;
        }

        // ----- ----- //

        #endregion

        #region Menu

        private void webcamPage_ActionBar_menuButton_Click(object sender, RoutedEventArgs e)
        {
            var animation = (Storyboard)FindResource("webcamPage_MenuOpen");
            if (UI_SLOW_MOTION) animation.SpeedRatio = 0.15;
            animation.Begin();

            titlebar_backButton.Visibility = Visibility.Visible;
        }

        private void webcamPage_saveimageButton_Click(object sender, RoutedEventArgs e)
        {
            Nullable<bool> result = saveFileDialog.ShowDialog();

            if (result == true)
            {
                try
                {
                    UriToDownload = (new Uri(currentcameraUrl + "?&dummy=" + DateTime.Now.Hour + DateTime.Now.Minute + DateTime.Now.Second));
                }
                catch (Exception ex)
                {
                    TextMessageDialog("Cannot begin local save...", "An error occured while trying to save the image.\nError: " + ex.Message);
                    return;
                }

                whereToDownload = saveFileDialog.FileName;

                var closemenuboard = (Storyboard)FindResource("webcamPage_MenuClose");
                closemenuboard.Begin();

                titlebar_backButton.Visibility = Visibility.Collapsed;

                SaveImageFile();
            }
        }

        private void webcamPage_saveallButton_Click(object sender, RoutedEventArgs e)
        {
            webcamPage_saveimageonarchiveButton_Click(sender, e);
            webcamPage_saveimageButton_Click(sender, e);
        }

        private void webcamPage_saveimageonarchiveButton_Click(object sender, RoutedEventArgs e)
        {
            archivebrowser.Url = new Uri("http://web.archive.org/save/" + currentcameraUrl);
            ShowProgressUI(1); webcamPage_progresslabel.Content = "connecting to archive.org...";
            if (UI_HOME_BLURIMAGE) webcamimageBlur.Radius = 10;

            var closemenuboard = (Storyboard)FindResource("webcamPage_MenuClose");
            closemenuboard.Begin();

            titlebar_backButton.Visibility = Visibility.Collapsed;
        }

        private void WebcamButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            LoadImage(button.Content.ToString());

            var closemenuboard = (Storyboard)FindResource("webcamPage_MenuClose");
            closemenuboard.Begin();

            titlebar_backButton.Visibility = Visibility.Collapsed;
        }

        private void webcamPage_menu_settingsButton_Click(object sender, RoutedEventArgs e)
        {
            SwitchToPage(1);

            settingsPage_UserInterfacePage_AccentColorCancelButton_Click(this, new RoutedEventArgs());

            int settingsPage_lastcameraCounter = 0;

            foreach (string camera in Properties.Settings.Default.camera_names)
            {
                if (camera == (string)webcamPage_ActionBarCameraNameLabel.Content)
                    SETTINGS_LAST_CAMERA = settingsPage_lastcameraCounter;
                else
                    settingsPage_lastcameraCounter++;
            }
        }

        private void webcamPage_menu_debugmenuButton_Click(object sender, RoutedEventArgs e)
        {
            SwitchToPage(2);
        }

        #endregion

        void ShowProgressUI(int mode)
        {
            if (webcamPage_dimGrid.Visibility != Visibility.Visible)
            {
                var board = (Storyboard)FindResource("webcamPage_progressShow");
                if (UI_SLOW_MOTION) board.SpeedRatio = 0.3;
                board.Begin();
            }

            switch (mode)
            {
                case 0:
                    {
                        // Progressring only
                        webcamPage_progressring.Visibility = Visibility.Visible;
                        webcamPage_progressring.IsActive = true;

                        break;
                    }
                case 1:
                    {
                        // Progressbar only, no text
                        webcamPage_progressbar.Visibility = Visibility.Visible;
                        webcamPage_progressbar.IsIndeterminate = true;

                        break;
                    }
                case 2:
                    {
                        // Progressbar only, with text
                        webcamPage_progressbar.Visibility = Visibility.Visible;
                        webcamPage_progressbar.IsIndeterminate = true;
                        webcamPage_progresslabel.Visibility = Visibility.Visible;

                        break;
                    }
                case 3:
                    {
                        // Progressring only, with text
                        webcamPage_progressring.Visibility = Visibility.Visible;
                        webcamPage_progressring.IsActive = true;
                        webcamPage_progresslabel.Visibility = Visibility.Visible;

                        break;
                    }
            }
        }

        void HideProgressUI()
        {
            if (webcamPage_dimGrid.Visibility == Visibility.Visible)
            {
                var board = (Storyboard)FindResource("webcamPage_progressHide");
                if (UI_SLOW_MOTION) board.SpeedRatio = 0.3;
                board.Begin();

                board.Completed += (s, ev) =>
                {
                    webcamPage_progressring.Visibility = Visibility.Collapsed;
                    webcamPage_progressring.IsActive = false;

                    webcamPage_progressbar.Visibility = Visibility.Collapsed;
                    webcamPage_progressbar.IsIndeterminate = false;

                    webcamPage_progresslabel.Content = "";
                    webcamPage_progresslabel.Visibility = Visibility.Collapsed;
                };
            }
        }

        #endregion

        #region Settings page // to do: actual pages, or at least better management/switching

        private void settingsPage_ActionBar_backButton_Click(object sender, RoutedEventArgs e) // legacy
        {
            SwitchToPage(0);
        }

        #region Settings pages UI & logic

        #region Webcam editor page

        private void settingsPage_MainPage_WebcamEditorButton_Click(object sender, RoutedEventArgs e)
        {
            // Close other pages
            settingsPage_AboutPage_ActionBar_backButton_Click(this, new RoutedEventArgs());
            settingsPage_UserInterfacePage_ActionBar_backButton_Click(this, new RoutedEventArgs());
            settingsPage_DefaultConfigurationDebugPage_ActionBar_backButton_Click(this, new RoutedEventArgs());

            // Set button look to active
            settingsPage_SetActiveButton(0);

            settingsPage_WebcamEditorPage.Opacity = 0;
            settingsPage_WebcamEditorPage.Visibility = Visibility.Visible;

            DoubleAnimation opacityanimation = new DoubleAnimation(1.0, new TimeSpan(0, 0, 0, 0, 300));
            settingsPage_WebcamEditorPage.BeginAnimation(Grid.OpacityProperty, opacityanimation);

            // prevent visually unloading the page (glitch)
            settingsPage_MainPage_WebcamEditorButton.IsEnabled = false;
            settingsPage_MainPage_UserInterfaceButton.IsEnabled = false;
            settingsPage_MainPage_AboutButton.IsEnabled = false;

            DispatcherTimer enableTimer = new DispatcherTimer();
            enableTimer.Interval = opacityanimation.Duration.TimeSpan;
            enableTimer.Tick += (s, ev) =>
            {
                enableTimer.Stop();
                settingsPage_MainPage_WebcamEditorButton.IsEnabled = true;
                settingsPage_MainPage_UserInterfaceButton.IsEnabled = true;
                settingsPage_MainPage_AboutButton.IsEnabled = true;
            };
            enableTimer.Start();

            // Fetch settings

            int namecounter = 0;
            int urlcounter = 0;

            WebcamEditor_NamesBox.Text = "";
            WebcamEditor_UrlsBox.Text = "";

            settingsPage_WebcamEditor_namesLinesStackPanel.Children.Clear();
            settingsPage_WebcamEditor_urlsLinesStackPanel.Children.Clear();

            foreach (string name in Properties.Settings.Default.camera_names)
            {
                namecounter++;
                if (namecounter != Properties.Settings.Default.camera_names.Count)
                    WebcamEditor_NamesBox.Text += name + "\r\n";
                else
                    WebcamEditor_NamesBox.Text += name;
            }

            foreach (string url in Properties.Settings.Default.camera_urls)
            {
                urlcounter++;
                if (urlcounter != Properties.Settings.Default.camera_names.Count)
                    WebcamEditor_UrlsBox.Text += url + "\r\n";
                else
                    WebcamEditor_UrlsBox.Text += url;
            }

            WebcamEditor_DoLineUI();
        }

        private void settingsPage_WebcamEditorPage_ActionBar_backButton_Click(object sender, RoutedEventArgs e)
        {
            DoubleAnimation opacityanimation = new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 300));
            settingsPage_WebcamEditorPage.BeginAnimation(Grid.OpacityProperty, opacityanimation);

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 0, 300);
            timer.Tick += (s, ev) => { timer.Stop(); settingsPage_WebcamEditorPage.Visibility = Visibility.Collapsed; };
            timer.Start();
        }

        private void settingsPage_WebcamEditorPage_ActionBar_moreButton_Click(object sender, RoutedEventArgs e)
        {
            if (settingsPage_WebcamEditorPage_ActionBar_resetconfigurationButton.Visibility == Visibility.Collapsed)
            {
                settingsPage_WebcamEditorPage_ActionBar_resetconfigurationButton.Visibility = Visibility.Visible;
                TranslateTransform trans0 = new TranslateTransform();
                settingsPage_WebcamEditorPage_ActionBar_resetconfigurationButton.RenderTransform = trans0;
                DoubleAnimation anim0 = new DoubleAnimation(50, 0, new TimeSpan(0, 0, 0, 0, 300));
                DoubleAnimation fanim0 = new DoubleAnimation(0, 1.0, new TimeSpan(0, 0, 0, 0, 350));
                trans0.BeginAnimation(TranslateTransform.XProperty, anim0);
                settingsPage_WebcamEditorPage_ActionBar_resetconfigurationButton.BeginAnimation(OpacityProperty, fanim0);
            }
            else
            {
                TranslateTransform trans1 = new TranslateTransform();
                settingsPage_WebcamEditorPage_ActionBar_resetconfigurationButton.RenderTransform = trans1;
                DoubleAnimation anim1 = new DoubleAnimation(0, 50, new TimeSpan(0, 0, 0, 0, 300));
                DoubleAnimation fanim1 = new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 350));
                trans1.BeginAnimation(TranslateTransform.XProperty, anim1);
                settingsPage_WebcamEditorPage_ActionBar_resetconfigurationButton.BeginAnimation(Button.OpacityProperty, fanim1);

                DispatcherTimer timer = new DispatcherTimer();
                timer.Interval = new TimeSpan(0, 0, 0, 0, 350);
                timer.Start();
                timer.Tick += (s, ev) => { timer.Stop(); settingsPage_WebcamEditorPage_ActionBar_resetconfigurationButton.Visibility = Visibility.Collapsed; };
            }
        }

        private void settingsPage_WebcamEditorPage_ActionBar_resetconfigurationButton_Click(object sender, RoutedEventArgs e)
        {
            ResetSettingsWindow dialog = new ResetSettingsWindow();

            settingsPage_dimGrid.Opacity = 0;
            settingsPage_dimGrid.Visibility = Visibility.Visible;
            DoubleAnimation dimanim = new DoubleAnimation(1.0, new TimeSpan(0, 0, 0, 0, 500));
            settingsPage_dimGrid.BeginAnimation(Grid.OpacityProperty, dimanim);

            dialog.Owner = this; dialog.ShowDialog();

            DoubleAnimation dimanim_out = new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 500));
            settingsPage_dimGrid.BeginAnimation(Grid.OpacityProperty, dimanim_out);
            DispatcherTimer dimanim_out_timer = new DispatcherTimer();
            dimanim_out_timer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            dimanim_out_timer.Tick += (s, ev) => { dimanim_out_timer.Stop(); settingsPage_dimGrid.Visibility = Visibility.Collapsed; };
            dimanim_out_timer.Start();
        }

        private void WebcamEditor_Boxes_TextChanged(object sender, TextChangedEventArgs e)
        {
            settingsPage_WebcamEditor_namesLinesStackPanel.Visibility = Visibility.Collapsed;
            settingsPage_WebcamEditor_urlsLinesStackPanel.Visibility = Visibility.Collapsed;
            WebcamEditor_EditorGrid.SetValue(Grid.ColumnProperty, 0);
            WebcamEditor_EditorGrid.SetValue(Grid.ColumnSpanProperty, 2);
        }

        private void WebcamEditor_SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Before we do anything, check if the names are the same amount as the URLs
            string[] check_names = WebcamEditor_NamesBox.Text.Split('\n');
            string[] check_urls = WebcamEditor_UrlsBox.Text.Split('\n');

            if (check_names.Length != check_urls.Length)
            {
                TextMessageDialog("Oh, noes!",
                    "Something's not right...\nYou seem to have more entries in one box than the other.\n\n(Names count: " + check_names.Length.ToString() + " | Urls count: " + check_urls.Length.ToString() + ")"
                    );

                return;
            }


            // Clear out the existing values
            Properties.Settings.Default.camera_names.Clear();
            Properties.Settings.Default.camera_urls.Clear();

            char[] delimiters = new char[] { '\r', '\n' };
            string[] names = WebcamEditor_NamesBox.Text.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            string[] urls = WebcamEditor_UrlsBox.Text.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

            // Add the new values
            foreach (string name in names)
            {
                Properties.Settings.Default.camera_names.Add(name);
            }

            foreach (string url in urls)
            {
                Properties.Settings.Default.camera_urls.Add(url);
            }

            Properties.Settings.Default.Save();

            WebcamEditor_DoLineUI();
        }

        void WebcamEditor_DoLineUI()
        {
            int namecounter = 0;
            int urlcounter = 0;

            settingsPage_WebcamEditor_namesLinesStackPanel.Children.Clear();
            settingsPage_WebcamEditor_urlsLinesStackPanel.Children.Clear();

            string[] names = WebcamEditor_NamesBox.Text.Split('\n');
            string[] urls = WebcamEditor_UrlsBox.Text.Split('\n');

            foreach (string name in names)
            {
                namecounter++;

                Button name_linebutton = new Button();
                name_linebutton.Style = this.Resources["ActionBarButtonStyle_Light"] as Style;
                name_linebutton.Foreground = new SolidColorBrush(Colors.Black);
                name_linebutton.FontSize = 9;
                name_linebutton.Height = 21.5; name_linebutton.Width = 25;
                name_linebutton.Content = namecounter;
                name_linebutton.Click += webcameditor_nameLineButton_Click;

                settingsPage_WebcamEditor_namesLinesStackPanel.Children.Add(name_linebutton);

                Rectangle seperator = new Rectangle();
                seperator.Height = 1;
                seperator.Fill = new SolidColorBrush(Colors.Gray);

                settingsPage_WebcamEditor_namesLinesStackPanel.Children.Add(seperator);
            }

            foreach (string url in urls)
            {
                urlcounter++;

                Button url_linebutton = new Button();
                url_linebutton.Style = this.Resources["ActionBarButtonStyle_Light"] as Style;
                url_linebutton.Foreground = new SolidColorBrush(Colors.Black);
                url_linebutton.FontSize = 9;
                url_linebutton.Height = 21.5; url_linebutton.Width = 25;
                url_linebutton.Content = urlcounter;
                url_linebutton.Click += webcameditor_urlLineButton_Click;

                settingsPage_WebcamEditor_urlsLinesStackPanel.Children.Add(url_linebutton);

                Rectangle seperator = new Rectangle();
                seperator.Height = 1;
                seperator.Fill = new SolidColorBrush(Colors.Gray);

                settingsPage_WebcamEditor_urlsLinesStackPanel.Children.Add(seperator);
            }

            settingsPage_WebcamEditor_namesLinesStackPanel.Visibility = Visibility.Visible;
            settingsPage_WebcamEditor_urlsLinesStackPanel.Visibility = Visibility.Visible;
            WebcamEditor_EditorGrid.SetValue(Grid.ColumnProperty, 1);
            WebcamEditor_EditorGrid.SetValue(Grid.ColumnSpanProperty, 1);
        }

        void webcameditor_nameLineButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            int index = int.Parse(btn.Content.ToString()) - 1;

            try
            {
                WebcamEditor_NamesBox.Select(WebcamEditor_NamesBox.Text.IndexOf(Properties.Settings.Default.camera_names[index]), Properties.Settings.Default.camera_names[index].Length);
            }
            catch
            {
                return;
            }
            WebcamEditor_NamesBox.Focus();

            foreach (var frchange_resetButton in settingsPage_WebcamEditor_namesLinesStackPanel.Children)
            {
                if (frchange_resetButton is Button)
                {
                    Button button = frchange_resetButton as Button;
                    button.Foreground = new SolidColorBrush(Colors.Black);
                }
                else
                {
                    Rectangle rec = frchange_resetButton as Rectangle;
                    rec.Fill = new SolidColorBrush(Colors.Gray);
                }
            }

            foreach (var frchange_resetButton in settingsPage_WebcamEditor_urlsLinesStackPanel.Children)
            {
                if (frchange_resetButton is Button)
                {
                    Button button = frchange_resetButton as Button;
                    button.Foreground = new SolidColorBrush(Colors.Black);
                }
                else
                {
                    Rectangle rec = frchange_resetButton as Rectangle;
                    rec.Fill = new SolidColorBrush(Colors.Gray);
                }
            }

            int urlcounter = 0;
            foreach (var button in settingsPage_WebcamEditor_urlsLinesStackPanel.Children)
            {
                if (button is Button)
                {
                    Button urlbutton = button as Button;
                    if (urlbutton.Content.ToString() != btn.Content.ToString())
                        urlcounter++;
                    else
                    {
                        urlbutton.Foreground = this.Resources["res_accentForeground"] as SolidColorBrush;
                        break;
                    }
                }
            }

            btn.Foreground = this.Resources["res_accentForeground"] as SolidColorBrush;

            int indexofPrevRectangle;
            int indexofNextRectangle;

            indexofPrevRectangle = int.Parse(btn.Content.ToString()) * 2 - 3;
            indexofNextRectangle = int.Parse(btn.Content.ToString()) * 2 - 1;

            Rectangle rec_1 = new Rectangle();

            if (indexofPrevRectangle > 0)
                rec_1 = settingsPage_WebcamEditor_namesLinesStackPanel.Children[indexofPrevRectangle] as Rectangle;

            Rectangle rec_2 = settingsPage_WebcamEditor_namesLinesStackPanel.Children[indexofNextRectangle] as Rectangle;

            Rectangle rec_1_url = new Rectangle();

            if (indexofPrevRectangle > 0)
                rec_1_url = settingsPage_WebcamEditor_urlsLinesStackPanel.Children[indexofPrevRectangle] as Rectangle;

            Rectangle rec_2_url = settingsPage_WebcamEditor_urlsLinesStackPanel.Children[indexofNextRectangle] as Rectangle;

            rec_1.Fill = this.Resources["res_accentForeground"] as SolidColorBrush;
            rec_2.Fill = this.Resources["res_accentForeground"] as SolidColorBrush;

            rec_1_url.Fill = this.Resources["res_accentForeground"] as SolidColorBrush;
            rec_2_url.Fill = this.Resources["res_accentForeground"] as SolidColorBrush;
        }

        void webcameditor_urlLineButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            int index = int.Parse(btn.Content.ToString()) - 1;

            try
            {
                WebcamEditor_UrlsBox.Select(WebcamEditor_UrlsBox.Text.IndexOf(Properties.Settings.Default.camera_urls[index]), Properties.Settings.Default.camera_urls[index].Length);
            }
            catch
            {
                return;
            }
            WebcamEditor_UrlsBox.Focus();

            foreach (var frchange_resetButton in settingsPage_WebcamEditor_urlsLinesStackPanel.Children)
            {
                if (frchange_resetButton is Button)
                {
                    Button button = frchange_resetButton as Button;
                    button.Foreground = new SolidColorBrush(Colors.Black);
                }
                else
                {
                    Rectangle rec = frchange_resetButton as Rectangle;
                    rec.Fill = new SolidColorBrush(Colors.Gray);
                }
            }

            foreach (var frchange_resetButton in settingsPage_WebcamEditor_namesLinesStackPanel.Children)
            {
                if (frchange_resetButton is Button)
                {
                    Button button = frchange_resetButton as Button;
                    button.Foreground = new SolidColorBrush(Colors.Black);
                }
                else
                {
                    Rectangle rec = frchange_resetButton as Rectangle;
                    rec.Fill = new SolidColorBrush(Colors.Gray);
                }
            }

            int namecounter = 0;
            foreach (var button in settingsPage_WebcamEditor_namesLinesStackPanel.Children)
            {
                if (button is Button)
                {
                    Button namebutton = button as Button;
                    if (namebutton.Content.ToString() != btn.Content.ToString())
                        namecounter++;
                    else
                    {
                        namebutton.Foreground = this.Resources["res_accentForeground"] as SolidColorBrush;
                        break;
                    }
                }
            }

            btn.Foreground = this.Resources["res_accentForeground"] as SolidColorBrush;

            int indexofPrevRectangle;
            int indexofNextRectangle;

            indexofPrevRectangle = int.Parse(btn.Content.ToString()) * 2 - 3;
            indexofNextRectangle = int.Parse(btn.Content.ToString()) * 2 - 1;

            Rectangle rec_1 = new Rectangle();

            if (indexofPrevRectangle > 0)
                rec_1 = settingsPage_WebcamEditor_namesLinesStackPanel.Children[indexofPrevRectangle] as Rectangle;

            Rectangle rec_2 = settingsPage_WebcamEditor_namesLinesStackPanel.Children[indexofNextRectangle] as Rectangle;

            Rectangle rec_1_url = new Rectangle();

            if (indexofPrevRectangle > 0)
                rec_1_url = settingsPage_WebcamEditor_urlsLinesStackPanel.Children[indexofPrevRectangle] as Rectangle;

            Rectangle rec_2_url = settingsPage_WebcamEditor_urlsLinesStackPanel.Children[indexofNextRectangle] as Rectangle;

            rec_1.Fill = this.Resources["res_accentForeground"] as SolidColorBrush;
            rec_2.Fill = this.Resources["res_accentForeground"] as SolidColorBrush;

            rec_1_url.Fill = this.Resources["res_accentForeground"] as SolidColorBrush;
            rec_2_url.Fill = this.Resources["res_accentForeground"] as SolidColorBrush;
        }

        #endregion

        #region User interface page

        private void settingsPage_MainPage_UserInterfaceButton_Click(object sender, RoutedEventArgs e)
        {
            // Close other pages

            settingsPage_WebcamEditorPage_ActionBar_backButton_Click(this, new RoutedEventArgs());
            settingsPage_AboutPage_ActionBar_backButton_Click(this, new RoutedEventArgs());
            settingsPage_DefaultConfigurationDebugPage_ActionBar_backButton_Click(this, new RoutedEventArgs());

            // Set button look to active
            settingsPage_SetActiveButton(1);

            settingsPage_UserInterfacePage.Opacity = 0;
            settingsPage_UserInterfacePage.Visibility = Visibility.Visible;

            DoubleAnimation opacityanimation = new DoubleAnimation(1.0, new TimeSpan(0, 0, 0, 0, 300));
            settingsPage_UserInterfacePage.BeginAnimation(Grid.OpacityProperty, opacityanimation);

            // prevent visually unloading the page (glitch)
            settingsPage_MainPage_WebcamEditorButton.IsEnabled = false;
            settingsPage_MainPage_UserInterfaceButton.IsEnabled = false;
            settingsPage_MainPage_AboutButton.IsEnabled = false;

            DispatcherTimer enableTimer = new DispatcherTimer();
            enableTimer.Interval = opacityanimation.Duration.TimeSpan;
            enableTimer.Tick += (s, ev) =>
            {
                enableTimer.Stop();
                settingsPage_MainPage_WebcamEditorButton.IsEnabled = true;
                settingsPage_MainPage_UserInterfaceButton.IsEnabled = true;
                settingsPage_MainPage_AboutButton.IsEnabled = true;
            };
            enableTimer.Start();

            // Get the settings
            settingsPage_UserInterfacePage_ImageBlurToggleButton_Toggle.IsChecked = UI_HOME_BLURIMAGE;
            settingsPage_UserInterfacePage_MenuBlurBehindButton_Toggle.IsChecked = Properties.Settings.Default.home_menu_blurbehind;
            settingsPage_UserInterfacePage_AutosizeToggleButton_Toggle.IsChecked = UI_AUTOSIZE_WINDOW;
            settingsPage_UserInterfacePage_SettingsShowColorOnTitlebarButton_Toggle.IsChecked = UI_SETTINGSPAGE_SHOWACCENTCOLOR;

            settingsPage_UserInterfacePage_AccentColorEditorButton_DescriptionLabel.Content = "Current color: " + colorDefinitions[Properties.Settings.Default.accentcolor];
            settingsPage_UserInterfacePage_ImageSizingButton_DescriptionLabel.Content = "Current mode: " + imagesizingDefinitions[Properties.Settings.Default.imagesizing];
        }

        private void settingsPage_UserInterfacePage_ActionBar_backButton_Click(object sender, RoutedEventArgs e)
        {
            DoubleAnimation opacityanimation = new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 300));
            settingsPage_UserInterfacePage.BeginAnimation(Grid.OpacityProperty, opacityanimation);

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 0, 300);
            timer.Tick += (s, ev) => { timer.Stop(); settingsPage_UserInterfacePage.Visibility = Visibility.Collapsed; };
            timer.Start();
        }

        private void settingsPage_UserInterfacePage_ActionBar_moreButton_Click(object sender, RoutedEventArgs e)
        {
            // show grid
            Storyboard board = (Storyboard)FindResource("settingsPage_UserInterfacePage_AccentColorDialogIn");
            if (UI_SLOW_MOTION) board.SpeedRatio = 0.10;
            board.Begin();

            // get config
            settingsPage_UserInterfacePage_SetActiveAccentColorButton(Properties.Settings.Default.accentcolor);

            settingsPage_UserInterfacePage_AccentColorAcceptButton.IsEnabled = false;

            if (Properties.Settings.Default.accentcolor == 5)
            {
                settingsPage_UserInterfacePage_AccentColorUseAeroBorder_CheckBox.IsChecked = false;
                settingsPage_UserInterfacePage_AccentColorUseAeroBorder_CheckBox.IsEnabled = false;
            }
            else
            {
                settingsPage_UserInterfacePage_AccentColorUseAeroBorder_CheckBox.IsEnabled = true;
            }

            settingsPage_UserInterfacePage_AccentColorUseAeroBorder_CheckBox.IsChecked = Properties.Settings.Default.window_aeroborder;

            string major = Environment.OSVersion.Version.ToString().Substring(0, 2);

            if (major == "10")
            {
                AccentColorButton_5.Background = new SolidColorBrush(AccentColorSet.ActiveSet["SystemAccent"]);
            }
            else
            {
                AccentColorButton_5.Visibility = Visibility.Collapsed;
            }
        }

        // ----- Buttons ----- //

        // > ----- Personalization & effects ----- //

        private void settingsPage_UserInterfacePage_ImageBlurToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if (e.Source == settingsPage_UserInterfacePage_ImageBlurToggleButton)
                settingsPage_UserInterfacePage_ImageBlurToggleButton_Toggle.IsChecked = !settingsPage_UserInterfacePage_ImageBlurToggleButton_Toggle.IsChecked;

            if (settingsPage_UserInterfacePage_ImageBlurToggleButton_Toggle.IsChecked == true)
            {
                Properties.Settings.Default.blur_image = true; Properties.Settings.Default.Save();
            }
            else
            {
                Properties.Settings.Default.blur_image = false; Properties.Settings.Default.Save();
            }
        }

        private void settingsPage_UserInterfacePage_MenuBlurBehindButton_Click(object sender, RoutedEventArgs e)
        {
            if (e.Source == settingsPage_UserInterfacePage_MenuBlurBehindButton)
                settingsPage_UserInterfacePage_MenuBlurBehindButton_Toggle.IsChecked = !settingsPage_UserInterfacePage_MenuBlurBehindButton_Toggle.IsChecked;

            if (settingsPage_UserInterfacePage_MenuBlurBehindButton_Toggle.IsChecked == true)
            {
                Properties.Settings.Default.home_menu_blurbehind = true; Properties.Settings.Default.Save();
            }
            else
            {
                Properties.Settings.Default.home_menu_blurbehind = false; Properties.Settings.Default.Save();
            }
        }

        private void settingsPage_UserInterfacePage_AccentColorButtonClick(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            string button_accentid = button.Name.Substring(18, 1);

            settingsPage_UserInterfacePage_SetActiveAccentColorButton(int.Parse(button_accentid));
            settingsPage_UserInterfacePage_AccentColorToSet = int.Parse(button_accentid);

            if (int.Parse(button_accentid) != Properties.Settings.Default.accentcolor)
            {
                settingsPage_UserInterfacePage_AccentColorAcceptButton.IsEnabled = true;
                settingsPage_UserInterfacePage_AccentChanged = true;
            }
            else
                settingsPage_UserInterfacePage_AccentChanged = false;

            if (button_accentid == "5")
            {
                settingsPage_UserInterfacePage_AccentColorUseAeroBorder_CheckBox.IsChecked = false;
                settingsPage_UserInterfacePage_AccentColorUseAeroBorder_CheckBox.IsEnabled = false;
            }
            else
            {
                settingsPage_UserInterfacePage_AccentColorUseAeroBorder_CheckBox.IsEnabled = true;

                if (Properties.Settings.Default.window_aeroborder == true)
                    settingsPage_UserInterfacePage_AccentColorUseAeroBorder_CheckBox.IsChecked = true;
            }
        }

        private void settingsPage_UserInterfacePage_ImageSizingButton_Click(object sender, RoutedEventArgs e)
        {
            // show grid
            Storyboard board = (Storyboard)FindResource("settingsPage_UserInterfacePage_ImageSizingDialogIn");
            if (UI_SLOW_MOTION) board.SpeedRatio = 0.10;
            board.Begin();

            // get config

            foreach (RadioButton rbtn in settingsPage_UserInterfacePage_ImageSizingStackPanel.Children)
                rbtn.IsChecked = false;

            foreach (RadioButton rbtn in settingsPage_UserInterfacePage_ImageSizingStackPanel.Children)
            {
                if (rbtn.Content as string == imagesizingDefinitions[Properties.Settings.Default.imagesizing])
                {
                    rbtn.IsChecked = true;
                }
            }
        }

        // > ----- Window ----- //

        private void settingsPage_UserInterfacePage_AutosizeToggleButton_Toggle_Click(object sender, RoutedEventArgs e)
        {
            if (e.Source == settingsPage_UserInterfacePage_AutosizeToggleButton)
                settingsPage_UserInterfacePage_AutosizeToggleButton_Toggle.IsChecked = !settingsPage_UserInterfacePage_AutosizeToggleButton_Toggle.IsChecked;

            if (settingsPage_UserInterfacePage_AutosizeToggleButton_Toggle.IsChecked == true)
            {
                Properties.Settings.Default.window_autosize = true; Properties.Settings.Default.Save();
            }
            else
            {
                Properties.Settings.Default.window_autosize = false; Properties.Settings.Default.Save();
            }
        }

        // > ----- Settings page ----- //

        private void settingsPage_UserInterfacePage_SettingsShowColorOnTitlebarButton_Click(object sender, RoutedEventArgs e)
        {
            if (e.Source == settingsPage_UserInterfacePage_SettingsShowColorOnTitlebarButton)
                settingsPage_UserInterfacePage_SettingsShowColorOnTitlebarButton_Toggle.IsChecked = !settingsPage_UserInterfacePage_SettingsShowColorOnTitlebarButton_Toggle.IsChecked;

            if (settingsPage_UserInterfacePage_SettingsShowColorOnTitlebarButton_Toggle.IsChecked == true)
            {
                Properties.Settings.Default.settings_showaccentcolor = true; Properties.Settings.Default.Save();
            }
            else
            {
                Properties.Settings.Default.settings_showaccentcolor = false; Properties.Settings.Default.Save();
            }

            settingsPage_UpdateTitlebarState();
        }

        // ----- ----- //

        #endregion

        #region User interface page : Accent color dialog

        int settingsPage_UserInterfacePage_AccentColorToSet;
        bool settingsPage_UserInterfacePage_AccentChanged;

        string[] colorDefinitions = { "Orange", "Red", "Green", "Blue", "Gray", "System accent color" };

        private void settingsPage_UserInterfacePage_AccentColorCancelButton_Click(object sender, RoutedEventArgs e)
        {
            // hide grid
            Storyboard board = (Storyboard)FindResource("settingsPage_UserInterfacePage_AccentColorDialogOut");
            if (UI_SLOW_MOTION) board.SpeedRatio = 0.10;
            board.Begin();
        }

        private void settingsPage_UserInterfacePage_AccentColorAcceptButton_Click(object sender, RoutedEventArgs e)
        {
            // save and hide
            if (settingsPage_UserInterfacePage_AccentChanged)
                Properties.Settings.Default.accentcolor = settingsPage_UserInterfacePage_AccentColorToSet;

            if (Properties.Settings.Default.accentcolor == 5)
            {
                settingsPage_UserInterfacePage_AccentColorUseAeroBorder_CheckBox.IsChecked = false;
                settingsPage_UserInterfacePage_AccentColorUseAeroBorder_CheckBox.IsEnabled = false;
                Properties.Settings.Default.window_aeroborder = false;
            }

            Properties.Settings.Default.Save();

            Storyboard board = (Storyboard)FindResource("settingsPage_UserInterfacePage_AccentColorDialogOut");
            board.Begin();

            if (settingsPage_UserInterfacePage_AccentChanged)
                SetAccentColor(settingsPage_UserInterfacePage_AccentColorToSet);
            SetAeroBorder();

            settingsPage_UserInterfacePage_AccentChanged = false;
        }

        private void settingsPage_UserInterfacePage_AccentColorUseAeroBorder_CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (settingsPage_UserInterfacePage_AccentColorUseAeroBorder_CheckBox.IsChecked == false)
            {
                Properties.Settings.Default.window_aeroborder = false;
            }
            else
            {
                Properties.Settings.Default.window_aeroborder = true;
            }
            settingsPage_UserInterfacePage_AccentColorAcceptButton.IsEnabled = !settingsPage_UserInterfacePage_AccentColorAcceptButton.IsEnabled;
        }

        void settingsPage_UserInterfacePage_SetActiveAccentColorButton(int accent)
        {
            AccentColorButton_0.BorderThickness = new Thickness(0);
            AccentColorButton_1.BorderThickness = new Thickness(0);
            AccentColorButton_2.BorderThickness = new Thickness(0);
            AccentColorButton_3.BorderThickness = new Thickness(0);
            AccentColorButton_4.BorderThickness = new Thickness(0);
            AccentColorButton_5.BorderThickness = new Thickness(0);

            Button targetbutton = (Button)FindName("AccentColorButton_" + accent);
            targetbutton.BorderThickness = new Thickness(3);
        }

        #endregion

        #region User interface page : Image sizing dialog

        string[] imagesizingDefinitions = { "None", "Fill", "Uniform", "Uniform to fill" };

        private void settingsPage_UserInterfacePage_ImageSizingCancelButton_Click(object sender, RoutedEventArgs e)
        {
            // hide grid
            Storyboard board = (Storyboard)FindResource("settingsPage_UserInterfacePage_ImageSizingDialogOut");
            if (UI_SLOW_MOTION) board.SpeedRatio = 0.10;
            board.Begin();
        }

        private void settingsPage_UserInterfacePage_ImageSizingAcceptButton_Click(object sender, RoutedEventArgs e)
        {
            // save and hide

            int counter = 0;

            foreach (RadioButton rbtn in settingsPage_UserInterfacePage_ImageSizingStackPanel.Children)
            {
                if (rbtn.IsChecked != true)
                    counter++;
                else
                    break;
            }

            Properties.Settings.Default.imagesizing = counter;
            Properties.Settings.Default.Save();

            Storyboard board = (Storyboard)FindResource("settingsPage_UserInterfacePage_ImageSizingDialogOut");
            if (UI_SLOW_MOTION) board.SpeedRatio = 0.10;
            board.Begin();

            settingsPage_UserInterfacePage_ImageSizingButton_DescriptionLabel.Content = "Current mode: " + imagesizingDefinitions[counter];
        }

        private void imagesizingRadioButton_Click(object sender, RoutedEventArgs e)
        {
            RadioButton rbtn = sender as RadioButton;
            string rbtn_name = rbtn.Name;
            int id = int.Parse(rbtn_name.Substring(23, 1));

            int fid = 0;
            foreach (RadioButton btn in settingsPage_UserInterfacePage_ImageSizingStackPanel.Children)
            {
                if (!btn.IsChecked == true)
                    fid++;
            }

            if (id != fid)
                settingsPage_UserInterfacePage_ImageSizingAcceptButton.IsEnabled = true;
            else
                settingsPage_UserInterfacePage_ImageSizingAcceptButton.IsEnabled = false;
        }

        #endregion

        #region About page

        private void settingsPage_MainPage_AboutButton_Click(object sender, RoutedEventArgs e)
        {
            // Close other pages

            settingsPage_WebcamEditorPage_ActionBar_backButton_Click(this, new RoutedEventArgs());
            settingsPage_UserInterfacePage_ActionBar_backButton_Click(this, new RoutedEventArgs());
            settingsPage_DefaultConfigurationDebugPage_ActionBar_backButton_Click(this, new RoutedEventArgs());

            // Set button look to active
            settingsPage_SetActiveButton(2);

            settingsPage_AboutPage.Opacity = 0;
            settingsPage_AboutPage.Visibility = Visibility.Visible;

            DoubleAnimation opacityanimation = new DoubleAnimation(1.0, new TimeSpan(0, 0, 0, 0, 300));
            settingsPage_AboutPage.BeginAnimation(Grid.OpacityProperty, opacityanimation);

            // prevent visually unloading the page (glitch)
            settingsPage_MainPage_WebcamEditorButton.IsEnabled = false;
            settingsPage_MainPage_UserInterfaceButton.IsEnabled = false;
            settingsPage_MainPage_AboutButton.IsEnabled = false;

            DispatcherTimer enableTimer = new DispatcherTimer();
            enableTimer.Interval = opacityanimation.Duration.TimeSpan;
            enableTimer.Tick += (s, ev) =>
            {
                enableTimer.Stop();
                settingsPage_MainPage_WebcamEditorButton.IsEnabled = true;
                settingsPage_MainPage_UserInterfaceButton.IsEnabled = true;
                settingsPage_MainPage_AboutButton.IsEnabled = true;
            };
            enableTimer.Start();

            // Get some info
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fileversioninfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            string versionnumber;

            if (fileversioninfo.FileVersion.Substring(6, 1) == "0")
                versionnumber = fileversioninfo.FileVersion.Substring(0, 5);
            else
                versionnumber = fileversioninfo.FileVersion;

            aboutPage_versionnumberLabel.Text = "Version: " + versionnumber + Properties.Settings.Default.versionid;
            aboutPage_buildidLabel.Text = "Build ID: " + Properties.Settings.Default.buildid;
        }

        private void settingsPage_AboutPage_ActionBar_backButton_Click(object sender, RoutedEventArgs e)
        {

            DoubleAnimation opacityanimation = new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 300));
            settingsPage_AboutPage.BeginAnimation(Grid.OpacityProperty, opacityanimation);

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 0, 300);
            timer.Tick += (s2, ev2) => { timer.Stop(); settingsPage_AboutPage.Visibility = Visibility.Collapsed; };
            timer.Start();
        }

        private void settingsPage_AboutPage_ActionBar_moreButton_Click(object sender, RoutedEventArgs e)
        {
            TextMessageDialog(
                "Technical information and credits",
                "Webcam Viewer version 1.0.1\n" + // get actual version number later, for now, it's just hard-coded like this
                "Build number: " + Properties.Settings.Default.buildid + "\n\n" +
                "Partially based on code from previous versions of WViewer.\n\n" +
                "Credits:\n\n" +
                "Application icon by Freepik at flaticon.com\n" +
                "Window and control base by MahApps.Metro"
                );
        } // legacy

        private void aboutPage_githubLinkLabel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Process.Start("https://github.com/xezrunner/webcamviewer");
        }

        private void aboutPage_officialpageLinkLabel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Process.Start("https://xezrunner.github.io/WebcamViewer");
        }

        #endregion

        #region Default configuration file debug page

        private void settingsPage_MainPage_DefaultConfigDebugButton_Click(object sender, RoutedEventArgs e)
        {
            // Close other pages

            settingsPage_WebcamEditorPage_ActionBar_backButton_Click(this, new RoutedEventArgs());
            settingsPage_UserInterfacePage_ActionBar_backButton_Click(this, new RoutedEventArgs());
            settingsPage_AboutPage_ActionBar_backButton_Click(this, new RoutedEventArgs());

            // Set button look to active
            settingsPage_SetActiveButton(3);

            settingsPage_DefaultConfigurationDebugPage.Opacity = 0;
            settingsPage_DefaultConfigurationDebugPage.Visibility = Visibility.Visible;

            DoubleAnimation opacityanimation = new DoubleAnimation(1.0, new TimeSpan(0, 0, 0, 0, 300));
            settingsPage_DefaultConfigurationDebugPage.BeginAnimation(Grid.OpacityProperty, opacityanimation);

            // prevent visually unloading the page (glitch)
            settingsPage_MainPage_WebcamEditorButton.IsEnabled = false;
            settingsPage_MainPage_UserInterfaceButton.IsEnabled = false;
            settingsPage_MainPage_AboutButton.IsEnabled = false;
            settingsPage_MainPage_DefaultConfigDebugButton.IsEnabled = false;

            DispatcherTimer enableTimer = new DispatcherTimer();
            enableTimer.Interval = opacityanimation.Duration.TimeSpan;
            enableTimer.Tick += (s, ev) =>
            {
                enableTimer.Stop();
                settingsPage_MainPage_WebcamEditorButton.IsEnabled = true;
                settingsPage_MainPage_UserInterfaceButton.IsEnabled = true;
                settingsPage_MainPage_AboutButton.IsEnabled = true;
                settingsPage_MainPage_DefaultConfigDebugButton.IsEnabled = true;
            };
            enableTimer.Start();

            // Do some stuff

            settingsPage_DefaultConfigurationDebugPage_ReadFileFromUri(Configuration.defaultconfig_file_URL);

        }

        private void settingsPage_DefaultConfigurationDebugPage_ActionBar_backButton_Click(object sender, RoutedEventArgs e)
        {
            DoubleAnimation opacityanimation = new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 300));
            settingsPage_DefaultConfigurationDebugPage.BeginAnimation(Grid.OpacityProperty, opacityanimation);

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 0, 300);
            timer.Tick += (s, ev) => { timer.Stop(); settingsPage_DefaultConfigurationDebugPage.Visibility = Visibility.Collapsed; };
            timer.Start();
        }

        async void settingsPage_DefaultConfigurationDebugPage_ReadFileFromUri(string uri)
        {
            settingsPage_DefaultConfigurationDebugPage_progressring.Visibility = Visibility.Visible;
            settingsPage_DefaultConfigurationDebugPage_MainLabel.Visibility = Visibility.Collapsed;

            WebClient client = new WebClient();
            Stream stream = null;

            try
            {
                stream = await client.OpenReadTaskAsync(new Uri(uri));
            }
            catch (Exception ex)
            {
                TextMessageDialog("Could not grab configuration file", "Check your internet connection and try again.\nError: " + ex.Message);

                settingsPage_DefaultConfigurationDebugPage_MainLabel.Content = "Could not grab configuration file\n\n" + ex.Message;

                settingsPage_DefaultConfigurationDebugPage_progressring.Visibility = Visibility.Collapsed;

                return;
            }

            StreamReader reader = new StreamReader(stream);

            settingsPage_DefaultConfigurationDebugPage_MainLabel.Content = "";
            settingsPage_DefaultConfigurationDebugPage_MainLabel.Content = await reader.ReadToEndAsync();

            settingsPage_DefaultConfigurationDebugPage_progressring.Visibility = Visibility.Collapsed;
            settingsPage_DefaultConfigurationDebugPage_MainLabel.Visibility = Visibility.Visible;
        }

        #endregion

        #endregion

        void settingsPage_UpdateTitlebarState()
        {
            if (Properties.Settings.Default.settings_showaccentcolor == false)
            {
                settings_rectangle.Fill = new SolidColorBrush(Colors.White);
                settings_rectangle_dim.Visibility = Visibility.Collapsed;

                Storyboard foregroundboard = (Storyboard)FindResource("titlebarToBlack");
                foregroundboard.Begin();
            }
            else
            {
                settings_rectangle.Fill = this.Resources["res_accentBackground"] as SolidColorBrush;
                settings_rectangle_dim.Visibility = Visibility.Visible;

                Storyboard foregroundboard = (Storyboard)FindResource("titlebarToWhite");
                foregroundboard.Begin();
            }
        }

        void settingsPage_SetActiveButton(int button)
        {
            settingsPage_MainPage_WebcamEditorButton_rectangle.Visibility = Visibility.Hidden;
            settingsPage_MainPage_WebcamEditorButton.Foreground = new SolidColorBrush(Colors.Black);

            settingsPage_MainPage_UserInterfaceButton_rectangle.Visibility = Visibility.Hidden;
            settingsPage_MainPage_UserInterfaceButton.Foreground = new SolidColorBrush(Colors.Black);

            settingsPage_MainPage_AboutButton_rectangle.Visibility = Visibility.Hidden;
            settingsPage_MainPage_AboutButton.Foreground = new SolidColorBrush(Colors.Black);

            settingsPage_MainPage_DefaultConfigDebugButton_rectangle.Visibility = Visibility.Hidden;
            settingsPage_MainPage_DefaultConfigDebugButton.Foreground = new SolidColorBrush(Colors.Black);

            switch (button)
            {
                case 0:
                    {
                        settingsPage_MainPage_WebcamEditorButton_rectangle.Visibility = Visibility.Visible;
                        settingsPage_MainPage_WebcamEditorButton.Foreground = (SolidColorBrush)this.Resources["res_accentForeground"];

                        break;
                    }
                case 1:
                    {
                        settingsPage_MainPage_UserInterfaceButton_rectangle.Visibility = Visibility.Visible;
                        settingsPage_MainPage_UserInterfaceButton.Foreground = (SolidColorBrush)this.Resources["res_accentForeground"];

                        break;
                    }
                case 2:
                    {
                        settingsPage_MainPage_AboutButton_rectangle.Visibility = Visibility.Visible;
                        settingsPage_MainPage_AboutButton.Foreground = (SolidColorBrush)this.Resources["res_accentForeground"];

                        break;
                    }
                case 3:
                    {
                        settingsPage_MainPage_DefaultConfigDebugButton_rectangle.Visibility = Visibility.Visible;
                        settingsPage_MainPage_DefaultConfigDebugButton.Foreground = (SolidColorBrush)this.Resources["res_accentForeground"];

                        break;
                    }
            }
        }

        #endregion

        #region Debug menu page

        private void debugmenuPage_ActionBar_infoButton_Click(object sender, RoutedEventArgs e)
        {
            TextMessageDialog("Debug menu",
                "This page contains debugging functions, mainly used for testing purposes while developing the application.\n\nEditing some settings might cause some issues or may even make the program unusable.\n\nTo reset your settings, hold down Control and Shift while launching the program."
                , true);
        }

        // ----- ----- ----- ----- -----//

        // ----- User interface ----- //

        private void debugmenu_showprogressringButton_Click(object sender, RoutedEventArgs e)
        {
            SwitchToPage(0);

            ShowProgressUI(3);
            webcamPage_progresslabel.Content = "Automatically closes in 10 seconds...";

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 10);
            timer.Tick += (s, ev) => { timer.Stop(); HideProgressUI(); };
            timer.Start();
        }

        private void debugmenu_showprogressbarButton_Click(object sender, RoutedEventArgs e)
        {
            SwitchToPage(0);

            ShowProgressUI(2);
            webcamPage_progressbar.IsIndeterminate = true;
            webcamPage_progresslabel.Content = "Automatically closes in 10 seconds...";

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 10);
            timer.Tick += (s, ev) => { timer.Stop(); HideProgressUI(); };
            timer.Start();
        }

        private void debugmenu_imagedebugButton_Click(object sender, RoutedEventArgs e)
        {
            if (webcamPage_DebugOverlay.Visibility == Visibility.Collapsed)
            {
                Button btn = sender as Button; btn.Content = "Hide debug overlay";
                webcamPage_DebugOverlay.Visibility = Visibility.Visible;
            }
            else
            {
                Button btn = sender as Button; btn.Content = "Show debug overlay";
                webcamPage_DebugOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private void debugmenu_slowmotionButton_Click(object sender, RoutedEventArgs e)
        {
            if (UI_SLOW_MOTION == false)
            {
                debugmenu_slowmotionButton.Content = "Disable slow motion animations";
                UI_SLOW_MOTION = true;
            }
            else
            {
                TextMessageDialog("Restart required", "You need to restart the application to reset the animation speed.", true);
            }
        }

        // ----- Configuration ----- //

        private void debugmenu_editconfigButton_Click(object sender, RoutedEventArgs e)
        {
            // create dialog for editing configuration

            ConfigurationEditorWindow window = new ConfigurationEditorWindow();
            window.Owner = this;
            window.ShowDialog();

            //string cameranames = "";
            //string cameraurls = "";

            //int namecounter = 0;
            //foreach (string name in Properties.Settings.Default.camera_names)
            //{
            //    namecounter++;

            //    if (namecounter != Properties.Settings.Default.camera_names.Count)
            //        cameranames += name + "\n";
            //    else
            //        cameranames += name;

            //}

            //int urlcounter = 0;
            //foreach (string url in Properties.Settings.Default.camera_urls)
            //{
            //    urlcounter++;

            //    if (urlcounter != Properties.Settings.Default.camera_urls.Count)
            //        cameraurls += url + "\n";
            //    else
            //        cameraurls += url;

            //}

            //MessageDialog("User configuration dump", "camera_names:\n\n" + cameranames + "\n\ncamera_urls:\n\n" + cameraurls, true);
        }

        private void debugmenu_resetconfigButton_Click(object sender, RoutedEventArgs e)
        {
            ResetSettingsWindow wnd = new ResetSettingsWindow(true);
            wnd.Owner = this; wnd.ShowDialog();
        }

        private void debugmenu_changefeedconfigurationButton_Click(object sender, RoutedEventArgs e)
        {
            Configuration.ApplyDefaultConfigurationFile();
        }

        // ----- Prerelease features ----- //

        private void debugmenu_changelogButton_Click(object sender, RoutedEventArgs e)
        {
            TextMessageDialog("Changelog", Properties.Settings.Default.changelog, true);
        }

        private void debugmenu_weatheruiButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            string button_content = button.Content as string;

            if (button_content.Contains("Enable"))
            {
                button.Content = "Disable Weather UI Demo";
                weatherButton.Visibility = Visibility.Visible;
            }
            else
            {
                button.Content = "Enable Weather UI Demo";
                weatherButton.Visibility = Visibility.Collapsed;
                webcamPage_weatherGrid.Visibility = Visibility.Collapsed;
            }

        }

        private void debugmenu_feeddebugButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            if (feedUpdateDebugPanel.Visibility == Visibility.Collapsed)
            {
                feedUpdateDebugPanel.Visibility = Visibility.Visible;
                button.Content = "Disable feed update UI debug";

            }
            else
            {
                feedUpdateDebugPanel.Visibility = Visibility.Collapsed;
                button.Content = "Enable feed update UI debug";
            }
        }

        // > ----- Feed debug buttons ----- //

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Storyboard s = FindResource("titlebar_updatestatusIn") as Storyboard;
            s.Begin();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Storyboard s = FindResource("titlebar_updatestatusOut") as Storyboard;
            s.Begin();
        }

        // ----- ----- //

        private void debugmenu_disabledebugmenuUIButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            if (webcamPage_menu_debugmenuButton.Visibility == Visibility.Visible)
            {
                button.Content = "Enable debug menu entry from UI";
                webcamPage_menu_debugmenuButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                button.Content = "Disable debug menu entry from UI";
                webcamPage_menu_debugmenuButton.Visibility = Visibility.Visible;
            }
        }

        // ----- ----- //

        private void debugmenu_exitButton_Click(object sender, RoutedEventArgs e)
        {
            SwitchToPage(0);
        }

        #endregion
    }
}
