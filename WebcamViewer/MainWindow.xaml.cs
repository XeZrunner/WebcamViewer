using MahApps.Metro.Controls;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
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

            archivebrowser.ProgressChanged += archivebrowser_ProgressChanged;
            archivebrowser.DocumentCompleted += Archivebrowser_DocumentCompleted;
        }

        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            // Reset configuration
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
                if (Keyboard.IsKeyDown(Key.LeftShift))
                {
                    Thread.Sleep(1000);
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
                    MessageDialog("Let's sing Happy birthday!",
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

            if (GetUserCameras() == true)
                LoadImage(Properties.Settings.Default.camera_names[0]);

            if (SystemParameters.PrimaryScreenWidth == 800 && SystemParameters.PrimaryScreenHeight == 600)
            {
                this.Width = 640;
                this.Height = 560;
            }

            if (SystemParameters.PrimaryScreenWidth <= 640 && SystemParameters.PrimaryScreenHeight <= 480)
            {
                MessageDialog("Low screen resolution", "Your resolution is lower than the minimum requirement 800x600.\nPlease increase your resolution for the best experience.", true);
                this.Width = 320;
                this.Height = 320;
            }
        }

        private void window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            debugoverlay_windowsize.Content = "Window size: " + this.Width + "x" + this.Height
                + " (" + this.Width + "x" + (this.Height - titlebarGrid.Height - debugmenuPage_ActionBar.Height) + " without UI elements)";
            webcamPage_menuCameraList.Height = this.Height - 170;

            // Make the maximize button change if we Aero snap or keyboard maximize or whatever else
            if (WindowState == WindowState.Maximized)
                maximizeButton.Content = "\ue923";
            else
                maximizeButton.Content = "\ue922";
        }

        #region Dialogs

        void MessageDialog(string Title, string Message, bool DarkMode = false)
        {
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
        }

        #endregion

        bool UI_SLOW_MOTION = false;
        bool UI_AUTOSIZE_WINDOW = false;

        bool READONLY_MODE = false;
        bool DISABLE_ARCHIVEORG = false;
        bool DISABLE_LOCALSAVE = false;
        bool DISABLE_WEBCAMEDITOR = false;

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

                        DebugLog("switched to page webcamimagePage");

                        break;
                    }
                case 1:
                    {
                        if (settingsPage.Visibility == Visibility.Collapsed)
                        {
                            var board = (Storyboard)FindResource("settingsPage_In");
                            if (UI_SLOW_MOTION) board.SpeedRatio = 0.15;
                            board.Begin();
                        }

                        DebugLog("switched to page settingsPage");

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

                        DebugLog("switched to page debugmenuPage");

                        break;
                    }
            }
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

            DebugLog("GetUserCameras() : " + counter);
            DebugLog("-----------------------------------------");

            if (counter == 0)
            {
                MessageDialog("No cameras found...", "You don't have any cameras set up. Please set up at least 1 camera to use this application."); SwitchToPage(1); settingsPage_MainPage_WebcamEditorButton_Click(this, new RoutedEventArgs()); settingsPage_WebcamEditorPage_ActionBar_moreButton_Click(this, new RoutedEventArgs()); webcamPage_ActionBarCameraNameLabel.Content = "";
                return false;
            }
            else
                return true;
        }

        void DebugLog(string message, int prirority = 0)
        {
            string finaltexttowrite = "";

            switch (prirority)
            {
                case 0: //info
                    {
                        finaltexttowrite = "[INFO] ";

                        break;
                    }
                case 1: //error
                    {
                        finaltexttowrite = "[ERROR] ";

                        break;
                    }
                case 2: //warning
                    {
                        finaltexttowrite = "[WARNING] ";

                        break;
                    }
            }

            finaltexttowrite += message;

            if (logviewerGrid_textbox.Text == "")
                logviewerGrid_textbox.Text += finaltexttowrite;
            else
                logviewerGrid_textbox.Text += "\n" + finaltexttowrite;
        }

        private void WebcamButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            LoadImage(button.Content.ToString());

            DebugLog("webcam - attempting to load image " + button.Content.ToString());

            var closemenuboard = (Storyboard)FindResource("webcamPage_MenuClose");
            closemenuboard.Begin();
        }

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

        Uri currentcameraUrl;
        BitmapImage bimage;

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
                        HideProgressUI(); webcamPage_progressring.Visibility = Visibility.Collapsed;

                        if (UI_AUTOSIZE_WINDOW)
                            titlebarGrid_contextmenu_SetImageSizeForWindow_Click(this, new RoutedEventArgs());

                        // Debug overlay
                        debugoverlay_cameraname.Content = "Camera name: " + cameraname;
                        debugoverlay_cameraurl.Content = "Camera url: " + currentcameraUrl;
                        debugoverlay_cameraresolution.Content = "Image resolution: " + bimage.PixelWidth + "x" + bimage.PixelHeight;

                        DebugLog("");
                        DebugLog("-----webcam image-----");
                        DebugLog("Camera name: " + cameraname);
                        DebugLog("Camera url: " + currentcameraUrl);
                        DebugLog("Camera resolution: " + bimage.PixelWidth + "x" + bimage.PixelHeight);
                        DebugLog("-----webcam image-----");
                        DebugLog("");
                    };
                    bimage.DownloadFailed += (s, ev) =>
                    {
                        MessageDialog("Could not load image", "An error occured while trying to load the webcam image...\n" + ev.ErrorException.Message);
                        webcamimage.Visibility = Visibility.Hidden;
                        HideProgressUI();

                        currentcameraUrl = new Uri(Properties.Settings.Default.camera_urls[Properties.Settings.Default.camera_names.IndexOf(cameraname)]);
                        webcamPage_ActionBarCameraNameLabel.Content = cameraname;
                        webcamPage_MenuCameraNameLabel.Text = "Error";
                        webcamPage_MenuCameraUrlLabel.Text = "http://www.somethinghappened.com";

                        webcamPage_ActionBar_menuButton_Click(this, new RoutedEventArgs());

                        DebugLog("webcam - couldn't load image: " + ev.ErrorException.Message, 1);
                    };
                    break;
                }
            }
        }

        private void SetTitlebarColor(Color backgroundcolor, bool blackforegroundcolor = false)
        {
            titlebarGrid.Background = new SolidColorBrush(backgroundcolor);
            if (blackforegroundcolor)
            {
                window_titleLabel.Foreground = new SolidColorBrush(Colors.Black);
                // title text
                closeButton.Foreground = new SolidColorBrush(Colors.Black);
                maximizeButton.Foreground = new SolidColorBrush(Colors.Black);
                minimizeButton.Foreground = new SolidColorBrush(Colors.Black);
            }
            else
            {
                window_titleLabel.Foreground = new SolidColorBrush(Colors.White);
                // title text
                closeButton.Foreground = new SolidColorBrush(Colors.White);
                maximizeButton.Foreground = new SolidColorBrush(Colors.White);
                minimizeButton.Foreground = new SolidColorBrush(Colors.White);
            }
        } // legacy

        private void titlebarGrid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void titlebarGrid_contextmenu_ResetWindowSize_Click(object sender, RoutedEventArgs e)
        {
            this.Width = 800;
            this.Height = 680;
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
            if (Keyboard.IsKeyDown(Key.LeftShift))
            {
                SwitchToPage(2);
                return;
            }
            this.WindowState = WindowState.Minimized;
        }

        private void webcamPage_ActionBar_menuButton_Click(object sender, RoutedEventArgs e)
        {
            var animation = (Storyboard)FindResource("webcamPage_MenuOpen");
            if (UI_SLOW_MOTION) animation.SpeedRatio = 0.15;
            animation.Begin();
        }

        private void webcamPage_menu_backButton_Click(object sender, RoutedEventArgs e)
        {
            var animation = (Storyboard)FindResource("webcamPage_MenuClose");
            if (UI_SLOW_MOTION) animation.SpeedRatio = 0.15;
            animation.Begin();
        }

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
                DebugLog("Debug overlay enabled.");
            }
            else
            {
                Button btn = sender as Button; btn.Content = "Show debug overlay";
                webcamPage_DebugOverlay.Visibility = Visibility.Collapsed;
                DebugLog("Debug overlay disabled.");
            }
        }

        private void debugmenu_exitButton_Click(object sender, RoutedEventArgs e)
        {
            SwitchToPage(0);
        }

        SaveFileDialog saveFileDialog = new SaveFileDialog();

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
                    MessageDialog("Cannot begin local save...", "An error occured while trying to save the image.\nError: " + ex.Message);
                    return;
                }

                whereToDownload = saveFileDialog.FileName;

                var closemenuboard = (Storyboard)FindResource("webcamPage_MenuClose");
                closemenuboard.Begin();

                SaveImageFile();
            }
        }

        #region Save image

        // ----- IMAGE SAVING ----- start

        WebClient Client = new WebClient();

        string whereToDownload;
        Uri UriToDownload;

        void SaveImageFile()
        {
            ShowProgressUI(2); webcamPage_progresslabel.Content = "preparing to download image...";
            webcamimageBlur.Radius = 10;

            DebugLog("local save - preparing...");

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

                DebugLog("local save - saved");

                if (e.Error != null)
                {
                    MessageDialog("Cannot download image", "An error occured while trying to download the image.\nError: " + e.Error.Message);
                    File.Delete(whereToDownload);
                    DebugLog("local save - failed", 1);
                }
            }));
        }

        // ----- IMAGE SAVING ----- end

        #endregion

        System.Windows.Forms.WebBrowser archivebrowser = new System.Windows.Forms.WebBrowser();

        private void webcamPage_saveimageonarchiveButton_Click(object sender, RoutedEventArgs e)
        {
            archivebrowser.Url = new Uri("http://web.archive.org/save/" + currentcameraUrl);
            ShowProgressUI(1); webcamPage_progresslabel.Content = "connecting to archive.org...";
            webcamimageBlur.Radius = 10;

            DebugLog("archive.org - saving image to archive.org...");

            var closemenuboard = (Storyboard)FindResource("webcamPage_MenuClose");
            closemenuboard.Begin();
        }

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
            DebugLog("archive.org - successfully saved");
            HideProgressUI();
            webcamimageBlur.Radius = 0;
        }

        private void webcamPage_menu_settingsButton_Click(object sender, RoutedEventArgs e)
        {
            SwitchToPage(1);
        }

        private void settingsPage_ActionBar_backButton_Click(object sender, RoutedEventArgs e)
        {
            SwitchToPage(0);

            if (webcamPage_menuCameraList.Children.Count != Properties.Settings.Default.camera_names.Count)
            {
                GetUserCameras();
                DebugLog("camera added or removed - need to reload user config");
            }
        }

        private void settingsPage_MainPage_WebcamEditorButton_Click(object sender, RoutedEventArgs e)
        {
            settingsPage_WebcamEditorPage.Opacity = 0;
            settingsPage_WebcamEditorPage.Visibility = Visibility.Visible;

            DoubleAnimation main_opacityanimation = new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 300));
            settingsPage_MainPage.BeginAnimation(Grid.OpacityProperty, main_opacityanimation);

            DoubleAnimation opacityanimation = new DoubleAnimation(1.0, new TimeSpan(0, 0, 0, 0, 300));
            settingsPage_WebcamEditorPage.BeginAnimation(Grid.OpacityProperty, opacityanimation);

            // Fetch settings

            int namecounter = 0;
            int urlcounter = 0;

            WebcamEditor_NamesBox.Text = "";
            WebcamEditor_UrlsBox.Text = "";

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

            DebugLog("");
            DebugLog("-----webcam editor-----");
            DebugLog("Camera names count: " + namecounter);
            DebugLog("Camera urls count: " + urlcounter);
            DebugLog("-----webcam editor-----");
            DebugLog("");
        }

        private void settingsPage_MainPage_AboutButton_Click(object sender, RoutedEventArgs e)
        {
            settingsPage_AboutPage.Opacity = 0;
            settingsPage_AboutPage.Visibility = Visibility.Visible;

            DoubleAnimation main_opacityanimation = new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 300));
            settingsPage_MainPage.BeginAnimation(Grid.OpacityProperty, main_opacityanimation);

            DoubleAnimation opacityanimation = new DoubleAnimation(1.0, new TimeSpan(0, 0, 0, 0, 300));
            settingsPage_AboutPage.BeginAnimation(Grid.OpacityProperty, opacityanimation);

            Storyboard board = (Storyboard)FindResource("AboutPage_TransitionIn");
            if (UI_SLOW_MOTION == true)
                board.SpeedRatio = 0.3;
            board.Begin();
        }

        private void settingsPage_WebcamEditorPage_ActionBar_backButton_Click(object sender, RoutedEventArgs e)
        {
            DoubleAnimation main_opacityanimation = new DoubleAnimation(1.0, new TimeSpan(0, 0, 0, 0, 300));
            settingsPage_MainPage.BeginAnimation(Grid.OpacityProperty, main_opacityanimation);

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

        private void WebcamEditor_SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Before we do anything, check if the names are the same amount as the URLs
            string[] check_names = WebcamEditor_NamesBox.Text.Split('\n');
            string[] check_urls = WebcamEditor_UrlsBox.Text.Split('\n');

            if (check_names.Length != check_urls.Length)
            {
                MessageDialog("Oh, noes!",
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
            DebugLog("Configuration saved.");
        }

        private void AboutPage_BackgroundCreditsLabel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("http://imgur.com/fO1Hsvu");
        }

        private void settingsPage_AboutPage_ActionBar_backButton_Click(object sender, RoutedEventArgs e)
        {
            Storyboard board = (Storyboard)FindResource("AboutPage_TransitionOut");
            if (UI_SLOW_MOTION)
                board.SpeedRatio = 0.3;
            board.Begin();

            DispatcherTimer mainTimer = new DispatcherTimer();
            mainTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            mainTimer.Tick += (s1, ev1) =>
            {
                DoubleAnimation main_opacityanimation = new DoubleAnimation(1.0, new TimeSpan(0, 0, 0, 0, 300));
                settingsPage_MainPage.BeginAnimation(Grid.OpacityProperty, main_opacityanimation);

                DoubleAnimation opacityanimation = new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 300));
                settingsPage_AboutPage.BeginAnimation(Grid.OpacityProperty, opacityanimation);

                DispatcherTimer timer = new DispatcherTimer();
                timer.Interval = new TimeSpan(0, 0, 0, 0, 300);
                timer.Tick += (s2, ev2) => { timer.Stop(); settingsPage_AboutPage.Visibility = Visibility.Collapsed; };
                timer.Start();

                mainTimer.Stop();
            };
            mainTimer.Start();
        }

        private void settingsPage_AboutPage_ActionBar_moreButton_Click(object sender, RoutedEventArgs e)
        {
            MessageDialog(
                "Technical information and credits",
                "Webcam Viewer version 1.0\n" +
                "Build number: " + Properties.Settings.Default.buildid + "\n\n" +
                "Partially based on code from previous versions of WViewer.\n\n" +
                "Credits:\n\n" +
                "Application icon by Freepik at flaticon.com\n" +
                "Window and control base by MahApps.Metro"
                );
        }

        private void webcamPage_saveallButton_Click(object sender, RoutedEventArgs e)
        {
            webcamPage_saveimageonarchiveButton_Click(sender, e);
            webcamPage_saveimageButton_Click(sender, e);
        }

        private void debugmenu_slowmotionButton_Click(object sender, RoutedEventArgs e)
        {
            if (UI_SLOW_MOTION == false)
            {
                debugmenu_slowmotionButton.Content = "Disable slow motion animations";
                UI_SLOW_MOTION = true;
                DebugLog("Slowmotion enabled.");
            }
            else
            {
                MessageDialog("Restart required", "You need to restart the application to reset the animation speed.", true);
            }
        }

        private void debugmenuPage_ActionBar_infoButton_Click(object sender, RoutedEventArgs e)
        {
            MessageDialog("Debug menu",
                "This page contains debugging functions, mainly used for testing purposes while developing the application.\n\nEditing some settings might cause some issues or may even make the program unusable.\n\nTo reset your settings, hold down Control and Shift while launching the program."
                , true);
        }

        private void debugmenu_changelogButton_Click(object sender, RoutedEventArgs e)
        {
            MessageDialog("Changelog", Properties.Settings.Default.changelog, true);
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

        private void debugmenu_viewconfigButton_Click(object sender, RoutedEventArgs e)
        {
            string cameranames = "";
            string cameraurls = "";

            int namecounter = 0;
            foreach (string name in Properties.Settings.Default.camera_names)
            {
                namecounter++;

                if (namecounter != Properties.Settings.Default.camera_names.Count)
                    cameranames += name + "\n";
                else
                    cameranames += name;

            }

            int urlcounter = 0;
            foreach (string url in Properties.Settings.Default.camera_urls)
            {
                urlcounter++;

                if (urlcounter != Properties.Settings.Default.camera_urls.Count)
                    cameraurls += url + "\n";
                else
                    cameraurls += url;

            }

            MessageDialog("User configuration dump", "camera_names:\n\n" + cameranames + "\n\ncamera_urls:\n\n" + cameraurls, true);
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

        private void titlebarGrid_contextmenu_SetImageSizeForWindow_Click(object sender, RoutedEventArgs e)
        {
            if (bimage != null)
            {
                this.Width = bimage.PixelWidth;
                this.Height = bimage.PixelHeight + 80;
                CenterWindowOnScreen();

            }
            else
                MessageDialog("Cannot set window size", "There's no image loaded.");
        }

        private void debugmenu_alwayssizewindowButton_Click(object sender, RoutedEventArgs e)
        {
            UI_AUTOSIZE_WINDOW = !UI_AUTOSIZE_WINDOW;
            if (UI_AUTOSIZE_WINDOW == true)
                debugmenu_alwayssizewindowButton.Content = "Always set the window size to the image size (ON)";
            else
                debugmenu_alwayssizewindowButton.Content = "Always set the window size to the image size";
        }

        private void debugmenu_resetconfigButton_Click(object sender, RoutedEventArgs e)
        {
            ResetSettingsWindow wnd = new ResetSettingsWindow(true);
            wnd.Owner = this; wnd.ShowDialog();
        }
    }
}
