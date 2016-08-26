using MahApps.Metro.Controls;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using WebcamViewer.User_controls;

namespace WebcamViewer
{
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            archivebrowser.ScriptErrorsSuppressed = true; // disables the script errors that could occur
            archivebrowser.ProgressChanged += archivebrowser_ProgressChanged;
            archivebrowser.DocumentCompleted += Archivebrowser_DocumentCompleted;

            webcamPage_menuGrid_progressInStoryboard = (Storyboard)FindResource("webcamPage_menuGrid_progressIn");
            webcamPage_menuGrid_progressOutStoryboard = (Storyboard)FindResource("webcamPage_menuGrid_progressOut");
        }

        private async void window_Loaded(object sender, RoutedEventArgs e)
        {
            await GetUserConfiguration(true);

            //webcamPage.Visibility = Visibility.Visible; webcamPage_menuButton.Visibility = Visibility.Visible; SetTitlebarButtonsStyle(0);
            SwitchToPage(0, true);

            webcamPage_menuGrid_CameraButton firstCameraButton = webcamPage_menuGrid_cameraListStackPanel.Children[0] as webcamPage_menuGrid_CameraButton;
            webcamPage_menuGrid_cameraButton_Click(firstCameraButton, new RoutedEventArgs());
        }

        #region Window & titlebar

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void maximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
                this.WindowState = WindowState.Maximized;
            else
                this.WindowState = WindowState.Normal;
        }

        private void minimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // We have to set the container grid's margin to 1 when in normal state because otherwise the border is considered as if it would be an overlay INSIDE the window, not on the outside...
            if (WindowState == WindowState.Maximized)
            {
                maximizeButton.Content = "\ue923";
                grid.Margin = new Thickness(0);
            }
            else
            {
                maximizeButton.Content = "\ue922";
                grid.Margin = new Thickness(1);
            }
        }

        private void titlebarGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            if (current_page == 0) // if we're on Webcam page
            {
                if (webcamPage_menuGrid.Visibility == Visibility.Visible)
                {
                    webcamPage_CloseMenu();
                    backButton.Visibility = Visibility.Collapsed;
                }
            }

            if (current_page == 1) // if we're on Settings page
            {
                SwitchToPage(0);

                foreach (Grid page in settingsPage_PagesGrid.Children) // get last settings page so we know which page to return to when we click on the Settings shortcut
                {
                    if (page.Visibility == Visibility.Visible)
                        settingsPage_lastPageID = int.Parse((string)page.Tag);
                }
            }
            if (current_page == 2) // if we're on Customizations delivery settings page
            {
                SwitchToPage(1);
            }
        }

        /// <summary>
        /// Dims the program UI. (note: it also dims (thus blocks) the titlebar)
        /// </summary>
        void global_Dim()
        {
            global_dimGrid.Opacity = 0;
            global_dimGrid.Visibility = Visibility.Visible;

            DoubleAnimation animation = new DoubleAnimation(1, TimeSpan.FromSeconds(0.3));

            global_dimGrid.BeginAnimation(Grid.OpacityProperty, animation);

            // remove inactive border
            this.NonActiveBorderBrush = this.BorderBrush;
            this.NonActiveGlowBrush = this.GlowBrush;
        }

        void global_UnDim()
        {
            DoubleAnimation animation = new DoubleAnimation(0, TimeSpan.FromSeconds(0.3));

            global_dimGrid.BeginAnimation(Grid.OpacityProperty, animation);

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(0.5);
            timer.Tick += (s, ev) => { timer.Stop(); global_dimGrid.Visibility = Visibility.Collapsed; };
            timer.Start();

            // restore inactive border
            this.NonActiveBorderBrush = Application.Current.Resources["App_NonActiveBorderBrush"] as SolidColorBrush;
            this.NonActiveGlowBrush = Application.Current.Resources["App_NonActiveGlowBrush"] as SolidColorBrush;
        }

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

            if (DarkMode == null)
            {
                if (current_page == 0)
                    dlg.IsDarkTheme = true;
                else
                    dlg.IsDarkTheme = false;
            }
            else
                dlg.IsDarkTheme = DarkMode.Value;

            global_Dim();

            dlg.ShowDialog();

            global_UnDim();
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

            if (DarkMode == null)
            {
                if (current_page == 0)
                    dlg.IsDarkTheme = true;
                else
                    dlg.IsDarkTheme = false;
            }
            else
                dlg.IsDarkTheme = DarkMode.Value;

            global_Dim();

            dlg.ShowDialog();

            global_UnDim();
        }

        #endregion

        #region Pages

        // Pages

        #region Webcam page

        // titlebar menu button //
        private void webcamPage_menuButton_Click(object sender, RoutedEventArgs e)
        {
            webcamPage_OpenMenu();
        }
        // -------------------- //

        #region Side menu

        bool AllowBackButtonLogic = true;

        void webcamPage_OpenMenu()
        {
            if (webcamPage_menuGrid.Visibility == Visibility.Hidden || webcamPage_menuGrid.Visibility == Visibility.Collapsed)
            {
                webcamPage_menuGrid.Opacity = 0;
                webcamPage_menuGrid.Visibility = Visibility.Visible;

                webcamPage_menuGrid_blurbehindBorder.Opacity = 0;

                DoubleAnimation menuGrid_opacityAnimation = new DoubleAnimation(0, 1, new TimeSpan(0, 0, 0, 0, 450).Duration());
                DoubleAnimation menuGrid_movementAnimation = new DoubleAnimation(-320, 0, new TimeSpan(0, 0, 0, 0, 450).Duration());

                DoubleAnimation menuGrid_blurbehindBorder_opacityAnimation = new DoubleAnimation(0, 1, new TimeSpan(0, 0, 0, 0, 300).Duration());

                QuarticEase qEase = new QuarticEase(); qEase.EasingMode = EasingMode.EaseOut;
                menuGrid_movementAnimation.EasingFunction = qEase;

                webcamPage_menuGrid.BeginAnimation(Grid.OpacityProperty, menuGrid_opacityAnimation);

                TranslateTransform tTf = new TranslateTransform();

                tTf.BeginAnimation(TranslateTransform.XProperty, menuGrid_movementAnimation);

                webcamPage_menuGrid.RenderTransform = tTf;

                DoubleAnimation menuGrid_blurbehind_radiusAnimation = new DoubleAnimation(5, new TimeSpan(0, 0, 0, 0, 300).Duration());

                DispatcherTimer timer = new DispatcherTimer();
                timer.Interval = menuGrid_movementAnimation.Duration.TimeSpan;
                timer.Tick += (s, ev) => { timer.Stop(); webcamPage_menuGrid_blurbehindBorder.BeginAnimation(Border.OpacityProperty, menuGrid_blurbehindBorder_opacityAnimation); webcamPage_menuGrid_blurbehind.BeginAnimation(System.Windows.Media.Effects.BlurEffect.RadiusProperty, menuGrid_blurbehind_radiusAnimation); };
                timer.Start();

                if (AllowBackButtonLogic)
                {
                    backButton.Visibility = Visibility.Visible;
                    webcamPage_menuButton.Visibility = Visibility.Collapsed;
                }
            }
        }

        void webcamPage_CloseMenu()
        {
            if (webcamPage_menuGrid.Visibility == Visibility.Visible)
            {
                DoubleAnimation menuGrid_opacityAnimation = new DoubleAnimation(1, 0, new TimeSpan(0, 0, 0, 0, 450).Duration());
                DoubleAnimation menuGrid_movementAnimation = new DoubleAnimation(0, -320, new TimeSpan(0, 0, 0, 0, 450).Duration());

                DoubleAnimation menuGrid_blurbehindBorder_opacityAnimation = new DoubleAnimation(1, 0, new TimeSpan(0, 0, 0, 0, 300).Duration());

                QuarticEase qEase = new QuarticEase(); qEase.EasingMode = EasingMode.EaseOut;
                menuGrid_movementAnimation.EasingFunction = qEase;

                webcamPage_menuGrid.BeginAnimation(Grid.OpacityProperty, menuGrid_opacityAnimation);

                TranslateTransform tTf = new TranslateTransform();

                webcamPage_menuGrid.RenderTransform = tTf;

                tTf.BeginAnimation(TranslateTransform.XProperty, menuGrid_movementAnimation);

                DoubleAnimation menuGrid_blurbehind_radiusAnimation = new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 100));

                webcamPage_menuGrid_blurbehind.BeginAnimation(System.Windows.Media.Effects.BlurEffect.RadiusProperty, menuGrid_blurbehind_radiusAnimation);
                webcamPage_menuGrid_blurbehindBorder.BeginAnimation(Border.OpacityProperty, menuGrid_blurbehindBorder_opacityAnimation);

                DispatcherTimer menucloseAnim_VisibilityTimer = new DispatcherTimer();
                menucloseAnim_VisibilityTimer.Interval = menuGrid_movementAnimation.Duration.TimeSpan;
                menucloseAnim_VisibilityTimer.Tick += (s, ev) => { menucloseAnim_VisibilityTimer.Stop(); webcamPage_menuGrid.Visibility = Visibility.Hidden; };
                menucloseAnim_VisibilityTimer.Start();

                if (AllowBackButtonLogic)
                {
                    backButton.Visibility = Visibility.Collapsed;
                    webcamPage_menuButton.Visibility = Visibility.Visible;
                }
            }
        }

        void webcamPage_menuGrid_HideCamerasPanel()
        {
            DoubleAnimation anim = new DoubleAnimation(0, TimeSpan.FromSeconds(.5));

            webcamPage_menuGrid_cameraListStackPanel.BeginAnimation(OpacityProperty, anim);

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(.5);
            timer.Tick += (s, ev) => { timer.Stop(); webcamPage_menuGrid_cameraListStackPanel.Visibility = Visibility.Collapsed; menuGrid_rowdefinition_cameralist.Height = new GridLength(0); };
            timer.Start();
        }

        void webcamPage_menuGrid_ShowCamerasPanel()
        {
            DoubleAnimation anim = new DoubleAnimation(1, TimeSpan.FromSeconds(.5));

            menuGrid_rowdefinition_cameralist.Height = new GridLength(1, GridUnitType.Star);

            webcamPage_menuGrid_cameraListStackPanel.Visibility = Visibility.Visible;

            webcamPage_menuGrid_cameraListStackPanel.BeginAnimation(OpacityProperty, anim);
        }

        void webcamPage_menuGrid_HideOverviewButton(bool force = false)
        {
            if (force == true || home_experiment_Overview)
            {
                DoubleAnimation anim = new DoubleAnimation(0, TimeSpan.FromSeconds(.5));

                webcamPage_menuGrid_overviewButton.BeginAnimation(OpacityProperty, anim);

                DispatcherTimer timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(.5);
                timer.Tick += (s, ev) => { timer.Stop(); webcamPage_menuGrid_overviewButton.Visibility = Visibility.Collapsed; menuGrid_rowdefinition_overviewButton.Height = new GridLength(0); };
                timer.Start();
            }
        }

        void webcamPage_menuGrid_ShowOverviewButton(bool force = false)
        {
            if (force == true || home_experiment_Overview)
            {
                DoubleAnimation anim = new DoubleAnimation(1, TimeSpan.FromSeconds(.5));

                menuGrid_rowdefinition_overviewButton.Height = new GridLength(47);

                webcamPage_menuGrid_overviewButton.Visibility = Visibility.Visible;

                webcamPage_menuGrid_overviewButton.BeginAnimation(OpacityProperty, anim);
            }
        }

        private void webcamPage_menuGrid_localSaveButton_Click(object sender, RoutedEventArgs e)
        {
            saveFileDialog.Filter = "JPG image (*.jpg)|*.jpg|All files (*.*)|*.*";
            saveFileDialog.Title = "Save image";
            saveFileDialog.DefaultExt = "jpg";

            Nullable<bool> result = saveFileDialog.ShowDialog();

            if (result == true)
            {
                try
                {
                    UriToDownload = (new Uri(currentImageUri + "?&dummy=" + DateTime.Now.Hour + DateTime.Now.Minute + DateTime.Now.Second));
                }
                catch (Exception ex)
                {
                    TextMessageDialog_FullWidth("Cannot begin local save...", "An error occured while trying to save the image.\nError: " + ex.Message);
                    return;
                }

                whereToDownload = saveFileDialog.FileName;

                //webcamPage_CloseMenu();

                SaveImageFile();
            }
        }

        private void webcamPage_menuGrid_BothSaveButton_Click(object sender, RoutedEventArgs e)
        {
            webcampage_menuGrid_archiveorgSaveButton_Click(this, new RoutedEventArgs());
            webcamPage_menuGrid_localSaveButton_Click(this, new RoutedEventArgs());

            // kindof ugly
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(0.5);
            timer.Tick += (s1, ev) =>
            {
                webcamPage_menuGrid_ShowCamerasPanel(); webcamPage_menuGrid_ShowOverviewButton();

                if (webcamPage_menuGrid_cameraListStackPanel.Opacity == 1)
                {
                    timer.Stop();
                }

            };
            timer.Start();
        }

        private void webcampage_menuGrid_archiveorgSaveButton_Click(object sender, RoutedEventArgs e)
        {
            archivebrowser.Url = new Uri("http://web.archive.org/save/" + currentImageUri);
            //webcamPage_ShowProgressUI(3); webcamPage_progressBar.IsIndeterminate = true; webcamPage_progressLabel.Content = "connecting to archive.org...";
            webcamPage_ShowProgressUI(4); webcamPage_menuGrid_SetProgressText("Preparing to save on archive.org...");
            webcamPage_menuGrid_HideCamerasPanel(); webcamPage_menuGrid_HideOverviewButton();

            //webcamPage_CloseMenu();
        }

        private void webcamPage_menuGrid_overviewButton_Click(object sender, RoutedEventArgs e)
        {
            if (webcamPage_MainContentGrid_OverviewGrid.Visibility != Visibility.Visible)
                webcamPage_MainContentGrid_SwitchToOverview();
            else
                webcamPage_MainContentGrid_SwitchToCameraView();

            webcamPage_CloseMenu();
        }

        void webcamPage_menuGrid_cameraButton_Click(object sender, RoutedEventArgs e)
        {
            webcamPage_menuGrid_CameraButton sBtn = sender as webcamPage_menuGrid_CameraButton;

            webcamPage_CloseMenu();
            LoadImage(Properties.Settings.Default.camera_urls[(int)sBtn.Tag]);
        }

        int settingsPage_lastPageID = 0;

        private void webcamPage_SettingsShortcutButton_Click(object sender, RoutedEventArgs e)
        {
            SwitchToPage(1);

            settingsPage_leftGrid_TabButtonClick(settingsPage_leftGrid_TabButtonStackPanel.Children[settingsPage_lastPageID], new RoutedEventArgs());
        }

        #endregion

        #region Webcam engine

        Uri currentImageUri;
        BitmapImage bimage;

        bool FirstCameraLoaded = false;

        void LoadImage(string Url, bool UseOldProgressUI = false)
        {
            if (FirstCameraLoaded == false)
                webcamPage_ShowProgressUI(0);
            else
                webcamPage_ShowProgressUI(0); // used to be 4 but we don't want that

            FirstCameraLoaded = true;

            bimage = new BitmapImage(new Uri(Url + "?&dummy=" + DateTime.Now.Hour + DateTime.Now.Minute + DateTime.Now.Second));
            bimage.DownloadCompleted += (s, e) =>
            {
                if (webcamPage_Image.Visibility != Visibility.Visible)
                    webcamPage_Image.Visibility = Visibility.Visible;

                webcamPage_Image.Source = bimage;

                webcamPage_HideProgressUI();

                //if (UI_AUTOSIZE_WINDOW)
                //    titlebarGrid_contextmenu_SetImageSizeForWindow_Click(this, new RoutedEventArgs());

                // Debug overlay
                //debugoverlay_cameraname.Content = "Camera name: " + cameraname;
                //debugoverlay_cameraurl.Content = "Camera url: " + currentcameraUrl;
                //debugoverlay_cameraresolution.Content = "Image resolution: " + bimage.PixelWidth + "x" + bimage.PixelHeight;
            };
            bimage.DownloadFailed += (s, ev) =>
            {
                TextMessageDialog_FullWidth("Could not load image", "An error occured while trying to load the webcam image...\n" + ev.ErrorException.Message);
                webcamPage_Image.Visibility = Visibility.Hidden;
                webcamPage_HideProgressUI();
            };

            currentImageUri = new Uri(Properties.Settings.Default.camera_urls[Properties.Settings.Default.camera_urls.IndexOf(Url)]);
            webcamPage_menuGrid_cameraNameLabel.Text = Properties.Settings.Default.camera_names[(Properties.Settings.Default.camera_urls.IndexOf(Url))].ToUpper();
            webcamPage_menuGrid_cameraUrlLabel.Text = currentImageUri.ToString();

            webcamPage_menuGrid_cameraNameLabel.ToolTip = Properties.Settings.Default.camera_names[(Properties.Settings.Default.camera_urls.IndexOf(Url))];
            webcamPage_menuGrid_cameraUrlLabel.ToolTip = currentImageUri.ToString();
        }

        // image saving //

        #region Image saving

        WebClient Client = new WebClient();

        SaveFileDialog saveFileDialog = new SaveFileDialog();

        string whereToDownload;
        Uri UriToDownload;

        void SaveImageFile()
        {
            //webcamPage_ShowProgressUI(3); webcamPage_progressBar.IsIndeterminate = true; webcamPage_progressLabel.Content = "preparing to download image...";
            webcamPage_ShowProgressUI(4); webcamPage_menuGrid_SetProgressText("Preparing to download image..."); webcamPage_menuGrid_HideCamerasPanel(); webcamPage_menuGrid_HideOverviewButton();

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
                {
                    webcamPage_progressLabel.Content = "downloading image: " + e.ProgressPercentage + "%";
                }
                else
                {
                    webcamPage_progressLabel.Visibility = Visibility.Collapsed;
                    webcamPage_menuGrid_SetProgressText("Downloading image: " + e.ProgressPercentage + "%...");
                }

                webcamPage_progressBar.IsIndeterminate = false;
                webcamPage_progressBar.Maximum = e.TotalBytesToReceive;
                webcamPage_progressBar.Value = e.BytesReceived;
            }));
        }
        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                webcamPage_HideProgressUI(); webcamPage_menuGrid_ShowCamerasPanel(); webcamPage_menuGrid_ShowOverviewButton();
                //refreshtimer.Start();

                if (e.Error != null)
                {
                    TextMessageDialog_FullWidth("Cannot download image", "An error occured while trying to download the image.\nError: " + e.Error.Message);
                    File.Delete(whereToDownload);
                }
            }));
        }

        #endregion

        System.Windows.Forms.WebBrowser archivebrowser = new System.Windows.Forms.WebBrowser();

        private void archivebrowser_ProgressChanged(object sender, System.Windows.Forms.WebBrowserProgressChangedEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftShift))
                webcamPage_progressLabel.Visibility = Visibility.Visible;
            else
                webcamPage_progressLabel.Visibility = Visibility.Collapsed;

            webcamPage_progressLabel.Content = "saving to archive.org... (" + e.CurrentProgress + " / " + e.MaximumProgress + ")";

            webcamPage_menuGrid_SetProgressText("Saving to archive.org - " + e.CurrentProgress + " / " + e.MaximumProgress);

            if (e.CurrentProgress >= 1000)
                webcamPage_progressBar.IsIndeterminate = false;

            webcamPage_progressBar.Maximum = e.MaximumProgress;
            webcamPage_progressBar.Value = e.CurrentProgress;
        }

        private void Archivebrowser_DocumentCompleted(object sender, System.Windows.Forms.WebBrowserDocumentCompletedEventArgs e)
        {
            webcamPage_progressLabel.Content = "saved...";
            webcamPage_menuGrid_SetProgressText("Saved to archive.org!");
            webcamPage_HideProgressUI();
            webcamPage_menuGrid_ShowCamerasPanel(); webcamPage_menuGrid_ShowOverviewButton();
        }

        // ----- ----- //

        #endregion

        #region Overview

        private void webcamPage_MainContentGrid_menuButton_Click(object sender, RoutedEventArgs e)
        {
            if (webcamPage_menuGrid.Visibility != Visibility.Visible)
            {
                webcamPage_OpenMenu();
                backButton.Visibility = Visibility.Visible;
            }
        }

        private void webcamPage_MainContentGrid_moreButton_Click(object sender, RoutedEventArgs e)
        {

        }

        void webcamPage_MainContentGrid_Overview_LoadContent()
        {

            // Clear out existing children
            webcamPage_MainContentGrid_OverviewGrid_WrapPanel.Children.Clear();

            int cameracounter = 0;
            foreach (string camera in Properties.Settings.Default.camera_names)
            {

                webcamPage_Overview_CameraButton button = new webcamPage_Overview_CameraButton()
                {
                    Image = new BitmapImage((new Uri(Properties.Settings.Default.camera_urls[cameracounter]))),
                    CameraName = camera,
                    Tag = cameracounter
                };

                button.Click += webcampage_MainContentGrid_Overview_CameraButtonClick;

                webcamPage_MainContentGrid_OverviewGrid_WrapPanel.Children.Add(button);

                cameracounter++;
            }
        }

        void webcampage_MainContentGrid_Overview_CameraButtonClick(object sender, RoutedEventArgs e)
        {
            webcamPage_Overview_CameraButton button = sender as webcamPage_Overview_CameraButton;
            webcamPage_MainContentGrid_SwitchToCameraView();
            LoadImage(Properties.Settings.Default.camera_urls[(int)button.Tag]);
        }

        #endregion

        void webcamPage_MainContentGrid_SwitchToOverview()
        {
            Storyboard s = (Storyboard)FindResource("webcamPage_OverviewIn");
            s.Begin();

            webcamPage_menuGrid_overviewButton.IconText = "\ue72b";
            webcamPage_menuGrid_overviewButton.Text = "Back to camera view";

            AllowBackButtonLogic = false; // disable backbutton logic
            webcamPage_menuGrid_HideCamerasPanel(); // hide the menu cameras
            #region Hide the infogrid

            DoubleAnimation anim = new DoubleAnimation(0, TimeSpan.FromSeconds(.5));
            webcamPage_menuGrid_infoGrid.BeginAnimation(OpacityProperty, anim);

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(.5);
            timer.Tick += (s1, ev) => { timer.Stop(); webcamPage_menuGrid_infoGrid.Visibility = Visibility.Collapsed; menuGrid_rowdefinition_infoGrid.Height = new GridLength(0); };
            timer.Start();

            #endregion

            // hide "back" buttons
            backButton.Visibility = Visibility.Collapsed;
            webcamPage_menuButton.Visibility = Visibility.Collapsed;

            webcamPage_MainContentGrid_Overview_LoadContent();
        }

        void webcamPage_MainContentGrid_SwitchToCameraView()
        {
            Storyboard s = (Storyboard)FindResource("webcamPage_OverviewOut");
            s.Begin();

            webcamPage_menuGrid_overviewButton.IconText = "\ue80a";
            webcamPage_menuGrid_overviewButton.Text = "Overview";

            AllowBackButtonLogic = true; // enable backbutton logic
            webcamPage_menuGrid_ShowCamerasPanel(); // show the menu cameras
            #region Show the infogrid

            DoubleAnimation anim = new DoubleAnimation(1, TimeSpan.FromSeconds(.5));

            menuGrid_rowdefinition_infoGrid.Height = GridLength.Auto;
            webcamPage_menuGrid_infoGrid.Visibility = Visibility.Visible;

            webcamPage_menuGrid_infoGrid.BeginAnimation(OpacityProperty, anim);

            #endregion

            // show menu button
            backButton.Visibility = Visibility.Collapsed;
            webcamPage_menuButton.Visibility = Visibility.Visible;
        }

        Storyboard webcamPage_menuGrid_progressInStoryboard;
        Storyboard webcamPage_menuGrid_progressOutStoryboard;

        bool IsMenuProgressHappening = false;

        /// <summary>
        /// Shows the progress UI.
        /// </summary>
        /// <param name="mode">The kind of progress UI to show. 0 = ProgressRing only, 1 = ProgressRing + text, 2 = ProgressBar only, 3 = ProgressBar + text, 4 = sidemenu ProgressRing</param>
        void webcamPage_ShowProgressUI(int mode)
        {
            if (mode <= 3)
            {
                webcamPage_Dim.Opacity = 0;
                webcamPage_Dim.Visibility = Visibility.Visible;

                DoubleAnimation webcamPage_Dim_OpacityAnimation = new DoubleAnimation(0, 1, new TimeSpan(0, 0, 0, 0, 300).Duration());
                webcamPage_Dim.BeginAnimation(Grid.OpacityProperty, webcamPage_Dim_OpacityAnimation);
            }
            else
            {
                IsMenuProgressHappening = true;

                webcamPage_menuGrid_progressOutStoryboard.Stop();
                webcamPage_menuGrid_progressInStoryboard.Begin();
            }

            switch (mode)
            {
                case 0:
                    {
                        webcamPage_progressRing.Visibility = Visibility.Visible;

                        break;
                    }
                case 1:
                    {
                        webcamPage_progressRing.Visibility = Visibility.Visible;
                        webcamPage_progressLabel.Visibility = Visibility.Visible;

                        break;
                    }
                case 2:
                    {
                        webcamPage_progressBar.Visibility = Visibility.Visible;

                        break;
                    }
                case 3:
                    {
                        webcamPage_progressBar.Visibility = Visibility.Visible;
                        webcamPage_progressLabel.Visibility = Visibility.Visible;

                        break;
                    }
                case 4:
                    {
                        // New in-menu progress UI
                        // The storyboard handles the Visiblity and IsActive so no need to set it here :)
                        webcamPage_menuGrid_SetProgressText("Please wait...");

                        break;
                    }
            }
        }

        void webcamPage_HideProgressUI()
        {
            if (webcamPage_Dim.Visibility == Visibility.Visible)
            {
                DoubleAnimation webcamPage_Dim_OpacityAnimation = new DoubleAnimation(1, 0, new TimeSpan(0, 0, 0, 0, 300).Duration());
                webcamPage_Dim.BeginAnimation(Grid.OpacityProperty, webcamPage_Dim_OpacityAnimation);

                DispatcherTimer timer = new DispatcherTimer();
                timer.Interval = webcamPage_Dim_OpacityAnimation.Duration.TimeSpan;
                timer.Tick += (s, ev) =>
                {
                    timer.Stop(); webcamPage_Dim.Visibility = Visibility.Collapsed;

                    webcamPage_progressRing.Visibility = Visibility.Collapsed;
                    webcamPage_progressBar.Visibility = Visibility.Collapsed;
                    webcamPage_progressLabel.Visibility = Visibility.Collapsed;
                };
                timer.Start();
            }
            else if (IsMenuProgressHappening == true)
            {
                // The storyboard handles the Visiblity and IsActive so no need to set it here :)

                webcamPage_menuGrid_progressInStoryboard.Stop();
                webcamPage_menuGrid_progressOutStoryboard.Begin();

                IsMenuProgressHappening = false;
            }
        }

        void webcamPage_menuGrid_SetProgressText(string text)
        {
            webcamPage_menuGrid_progressPanel_progressTextBlock.Text = text;
        }

        // ------------ //

        #endregion

        #region Settings page

        #region Webcam editor page

        ListViewDragDropManager<settingsPage_WebcamEditorPage_Camera> dragdropManager = new ListViewDragDropManager<settingsPage_WebcamEditorPage_Camera>();

        bool settingsPage_WebcamEditorPage_createdDragDropManager = false;

        private void settingsPage_WebcamEditorPage_ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (settingsPage_WebcamEditorPage_ListView_handleSelectionChange)
            {
                settingsPage_WebcamEditorPage_ItemEditorGrid.Visibility = Visibility.Visible;
                settingsPage_WebcamEditorPage_ItemEditorGrid_Main.Visibility = Visibility.Visible;
                settingsPage_WebcamEditorPage_ItemEditorGrid_Disabled.Visibility = Visibility.Collapsed;

                settingsPage_WebcamEditorPage_ItemEditor_NameTextBox.Text = Properties.Settings.Default.camera_names[settingsPage_WebcamEditorPage_ListView.SelectedIndex];
                settingsPage_WebcamEditorPage_ItemEditor_UrlTextBox.Text = Properties.Settings.Default.camera_urls[settingsPage_WebcamEditorPage_ListView.SelectedIndex];
            }
        }

        bool settingsPage_WebcamEditorPage_ListView_handleSelectionChange = true;

        private void settingsPage_WebcamEditorPage_ListView_dragdropManager_ProcessDrop(object sender, ProcessDropEventArgs<settingsPage_WebcamEditorPage_Camera> e)
        {
            settingsPage_WebcamEditorPage_ListView_handleSelectionChange = false;

            settingsPage_WebcamEditorPage_ItemEditorGrid_Main.Visibility = Visibility.Collapsed;
            settingsPage_WebcamEditorPage_ItemEditorGrid_Disabled.Visibility = Visibility.Visible;

            e.ItemsSource.Move(e.OldIndex, e.NewIndex);

            e.Effects = DragDropEffects.Move;
        }

        private void settingsPage_WebcamEditorPage_SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.camera_names.Clear();
            Properties.Settings.Default.camera_urls.Clear();

            foreach (settingsPage_WebcamEditorPage_Camera item in settingsPage_WebcamEditorPage_ListView.Items)
            {
                // names
                Properties.Settings.Default.camera_names.Add(item.Name);

                // urls
                Properties.Settings.Default.camera_urls.Add(item.Url);
            }

            settingsPage_WebcamEditorPage_ListView_handleSelectionChange = true;

            if (settingsPage_WebcamEditorPage_ListView.SelectedIndex != null)
            {
                settingsPage_WebcamEditorPage_ItemEditorGrid_Main.Visibility = Visibility.Visible;
                settingsPage_WebcamEditorPage_ItemEditorGrid_Disabled.Visibility = Visibility.Collapsed;
            }
        }

        // Class
        public class settingsPage_WebcamEditorPage_Camera
        {
            public string Name { get; set; }

            public string Url { get; set; }

            public string SaveLocation { get; set; }

            public int RefreshRate { get; set; }
        }

        #endregion

        #region Home page



        #endregion

        #region Settings page



        #endregion

        #region User interface page

        private void settingsPage_UserInterfacePage_UI_ThemeButton_Click(object sender, RoutedEventArgs e)
        {
            Popups.MessageDialog dlg = new Popups.MessageDialog();
            dlg.Title = "";

            // will need to be true
            dlg.Content_DisableMargin = false;

            dlg.IsDarkTheme = true;

            // Introduction-style
            dlg.Content = "Choose a theme\n\n" + "/Dialog controls/settingsPage_UserInterfacePage_ThemeControl.xaml\n\n" + "Todo: Dialog controls class";

            dlg.FirstButtonContent = "Apply theme";
            dlg.SecondButtonContent = "Go back";

            dlg.ShowDialog();
        }

        #endregion

        #region About page

        private void settingsPage_AboutPage_GithubLink_Click(object sender, MouseButtonEventArgs e)
        {
            TextBlock textblock = sender as TextBlock;

            // Go to the project Github page
            Process.Start("https://" + textblock.Text);
        }

        private void settingsPage_AboutPage_CreditsButton_Click(object sender, RoutedEventArgs e)
        {
            Popups.MessageDialog dialog = new Popups.MessageDialog();

            dialog.Content = new settingsPage_AboutPage_CreditsControl();

            dialog.Content_DisableMargin = true;

            dialog.IsDarkTheme = false;

            dialog.FirstButtonContent = "Close";

            dialog.ShowDialog();
        }

        #endregion

        #region Debug menu page

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
            dlg.FirstButtonContent = "Accept";
            dlg.SecondButtonContent = "Cancel";

            if (dlg.ShowDialogWithResult() == 0)
            {
                SwitchToPage(0);

                #region Timers
                int ui_clocktimer_countdown = 10;

                DispatcherTimer ui_clocktimer = new DispatcherTimer();
                ui_clocktimer.Interval = new TimeSpan(0, 0, 1);
                ui_clocktimer.Tick += (s, ev) =>
                {
                    if (checkbox_countdown.IsChecked == true)
                    {
                        if (webcamPage_progressLabel.Visibility == Visibility.Visible)
                            webcamPage_progressLabel.Content = textbox.Text + " - " + ui_clocktimer_countdown;
                        if (webcamPage_menuGrid_progressPanel_progressTextBlock.Visibility == Visibility.Visible)
                            webcamPage_menuGrid_SetProgressText(textbox.Text + " - " + ui_clocktimer_countdown);
                    }
                    ui_clocktimer_countdown--;
                };
                ui_clocktimer.Start();

                DispatcherTimer timer = new DispatcherTimer();
                timer.Interval = new TimeSpan(0, 0, 10);
                timer.Tick += (s, ev) => { timer.Stop(); ui_clocktimer.Stop(); webcamPage_HideProgressUI(); };
                timer.Start();
                #endregion

                if (webcamPage_progressLabel.Visibility == Visibility.Visible)
                    webcamPage_progressLabel.Content = textbox.Text;
                if (webcamPage_menuGrid_progressPanel_progressTextBlock.Visibility == Visibility.Visible)
                    webcamPage_menuGrid_SetProgressText(textbox.Text);

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
                                    webcamPage_ShowProgressUI(1);
                                    webcamPage_CloseMenu();
                                    break;
                                }
                            case 1:
                                {
                                    webcamPage_ShowProgressUI(3);
                                    webcamPage_CloseMenu();
                                    break;
                                }
                            case 2:
                                {
                                    webcamPage_ShowProgressUI(4);
                                    webcamPage_OpenMenu();
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
                Text = "Title"
            };

            TextBox titleBox = new TextBox()
            {
                Margin = new Thickness(0, 10, 0, 10),
                Text = "Title"
            };

            TextBlock text2 = new TextBlock()
            {
                Text = "Message"
            };

            TextBox messageBox = new TextBox()
            {
                Margin = new Thickness(0, 10, 0, 10),
                AcceptsReturn = true,
                Text = "Message"
            };

            dlg_contentPanel.Children.Add(text0);
            dlg_contentPanel.Children.Add(styleDropDownButton);
            dlg_contentPanel.Children.Add(text1);
            dlg_contentPanel.Children.Add(titleBox);
            dlg_contentPanel.Children.Add(text2);
            dlg_contentPanel.Children.Add(messageBox);

            dlg.Content = dlg_contentPanel;

            #endregion

            dlg.FirstButtonContent = "Show dialog";
            dlg.SecondButtonContent = "Cancel";

            if (dlg.ShowDialogWithResult() == 0)
            {
                if (styleDropDownButton.SelectedIndex == 0)
                    TextMessageDialog(titleBox.Text, messageBox.Text);
                else
                    TextMessageDialog_FullWidth(titleBox.Text, messageBox.Text);
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
            global_Dim();

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

                await GetUserConfiguration(true);
                #endregion
            }

            global_UnDim();
        }

        private async void settingsPage_DebugMenuPage_Configuration_ResetConfigurationButton_Click(object sender, RoutedEventArgs e)
        {
            Popups.MessageDialog_FullWidth dlg = new Popups.MessageDialog_FullWidth();
            dlg.Title = "";
            dlg.Content = "Resetting the configuration will reset all user settings to their defaults.";

            dlg.FirstButtonContent = "Confirm";
            dlg.SecondButtonContent = "Go back";

            global_Dim();

            if (dlg.ShowDialogWithResult() == 0)
            {
                // Reset configuration
                Properties.Settings.Default.Reset();
                Properties.Settings.Default.Save();

                await GetUserConfiguration(true);

                /* no need to restart anymore
                TextMessageDialog("Configuration has been reset...", "We'll restart Webcam Viewer for you...");

                System.Windows.Forms.Application.Restart();
                Application.Current.Shutdown();
                */
            }

            global_UnDim();
        }

        #endregion

        #region Prerelease settings page

        private void settingsPage_PrereleaseProgramSettingsPage_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (settingsPage_PrereleaseProgramSettingsPage.Visibility != Visibility.Visible)
                SetTitlebarButtonsStyle(2);
        }

        private void settingsPage_PrereleaseProgramSettingsPage_IntroductionPanel_SignUpButton_Click(object sender, RoutedEventArgs e)
        {
            User_controls.ProgressRing ring = settingsPage_PrereleaseProgramSettingsPage_IntroductionPanel_SignUpButton.RightSideGrid.Children[0] as User_controls.ProgressRing;
            ring.IsActive = true;

            // fake timer for UI
            DispatcherTimer faketimer = new DispatcherTimer();
            faketimer.Interval = TimeSpan.FromSeconds(3);
            faketimer.Tick += (s, ev) =>
            {
                faketimer.Stop();

                TextMessageDialog("", "An error occured while trying to contact the prerelease account database.", true);

                ring.IsActive = false;
            };
            faketimer.Start();
        }

        private void settingsPage_PrereleaseProgramSettingsPage_IntroductionPanel_LoginButton_Click(object sender, RoutedEventArgs e)
        {
            User_controls.ProgressRing ring = settingsPage_PrereleaseProgramSettingsPage_IntroductionPanel_LoginButton.RightSideGrid.Children[0] as User_controls.ProgressRing;
            ring.IsActive = true;

            // fake timer for UI
            DispatcherTimer faketimer = new DispatcherTimer();
            faketimer.Interval = TimeSpan.FromSeconds(3);
            faketimer.Tick += (s, ev) =>
            {
                faketimer.Stop();

                TextMessageDialog("", "An error occured while trying to contact the prerelease account database.", true);

                ring.IsActive = false;
            };
            faketimer.Start();
        }

        #endregion

        #region Default customizations debug page

        bool customizationdeliveryPage_visited = false;

        private void settingsPage_DefaultCustomizationsDebugPage_SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SwitchToPage(2);

            // Go to status page the first time
            if (customizationdeliveryPage_visited == false)
            {
                Menu_NavigationButton btn = customizationsdeliveryPage_MenuGrid_TopStackPanel.Children[0] as Menu_NavigationButton;
                Menu_NavigationButton_Click(btn, new RoutedEventArgs());

                customizationdeliveryPage_visited = true;
            }
        }

        #endregion


        /// Tab button Tags
        /// 0: Webcams
        /// 1: User interface
        /// 2: About & updates
        /// 3: Debug menu
        /// 4: Beta program settings
        /// 5: Default customizations debug

        void settingsPage_leftGrid_TabButtonClick(object sender, RoutedEventArgs e)
        {
            settingsPage_TabButton sBtn = sender as settingsPage_TabButton;

            int PageID;

            // See if the button tag is an integer
            try
            {
                PageID = int.Parse((string)sBtn.Tag);
            }
            catch
            {
                TextMessageDialog_FullWidth("Invalid button tag", "You shouldn't see this error.\n\nGot tag: " + sBtn.Tag.ToString());
                return;
            }

            // Activate the desired button and deactivate the others
            foreach (settingsPage_TabButton button in settingsPage_leftGrid_TabButtonStackPanel.Children)
            {
                if ((string)button.Tag == PageID.ToString())
                    button.IsActive = true;
                else
                    button.IsActive = false;
            }

            // Open the desired page and close those the others
            foreach (Grid page in settingsPage_PagesGrid.Children)
            {
                if ((string)page.Tag == PageID.ToString())
                {
                    page.Visibility = Visibility.Visible;
                }
                else
                {
                    page.Visibility = Visibility.Collapsed;
                }
            }

            // Page events
            settingsPage_DoPageEvents(PageID);

            // Page toggles
            // There should be a different way of getting toggles here, but for now, this is "working".
            Grid grid = settingsPage_PagesGrid.Children[PageID] as Grid; // the Page™
            ScrollViewer grid_scrollview = null; // the page's main scrollviewer

            foreach (object viewer in grid.Children) // get the main scrollviewer
            {
                if (viewer.GetType() == (typeof(ScrollViewer))) // is there a scrollviewer?
                    grid_scrollview = viewer as ScrollViewer; // yee, continue
                                                              // if a page is not using a scrollviewer as it's content, it's PROBABLY not the right format. This is ugly and shouldn't be this way, but I'll rewrite it one day. Hopefully.
            }

            // 100% total legit understandable explanations™ by XeZrunner:

            if (grid_scrollview != null)
            {
                StackPanel grid_scrollview_mainStackPanel = grid_scrollview.Content as StackPanel; // get the main stackpanel of the main scrollviewer

                foreach (object subStackPanel in grid_scrollview_mainStackPanel.Children) // for each sub stack panel:
                {
                    if (subStackPanel.GetType() == (typeof(StackPanel))) // if it really is a stackpanel:
                    {
                        StackPanel _subStackPanel = subStackPanel as StackPanel;
                        foreach (object button in _subStackPanel.Children) // for each toggleswitchbutton in a substackpanel:
                        {
                            if (button.GetType() == (typeof(settingsPage_ToggleSwitchButton))) // if it really is a toggleswitchbutton
                            {
                                settingsPage_ToggleSwitchButton _button = button as settingsPage_ToggleSwitchButton;
                                if ((string)_button.Tag != "" & _button.Tag != null) // if the tag is something
                                {
                                    _button.IsActive = (bool)Properties.Settings.Default[(string)_button.Tag]; // check or uncheck the Button™
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This function (re)loads the appropiate settings for a page once it opens or when a change was made by a function inside the page.
        /// </summary>
        void settingsPage_DoPageEvents(int page)
        {
            switch (page)
            {
                #region Webcam editor
                case 0: // Webcam Editor
                    {
                        if (!settingsPage_WebcamEditorPage_createdDragDropManager)
                        {
                            dragdropManager = new ListViewDragDropManager<settingsPage_WebcamEditorPage_Camera>(settingsPage_WebcamEditorPage_ListView);

                            dragdropManager.ProcessDrop += settingsPage_WebcamEditorPage_ListView_dragdropManager_ProcessDrop;

                            settingsPage_WebcamEditorPage_createdDragDropManager = true;
                        }

                        ObservableCollection<settingsPage_WebcamEditorPage_Camera> items = new ObservableCollection<settingsPage_WebcamEditorPage_Camera>();

                        foreach (string cameraname in Properties.Settings.Default.camera_names)
                        {
                            items.Add(new settingsPage_WebcamEditorPage_Camera() { Name = cameraname, Url = Properties.Settings.Default.camera_urls[Properties.Settings.Default.camera_names.IndexOf(cameraname)], SaveLocation = "null", RefreshRate = 0 });
                        }

                        settingsPage_WebcamEditorPage_ListView_handleSelectionChange = false;

                        settingsPage_WebcamEditorPage_ListView.ItemsSource = items;

                        settingsPage_WebcamEditorPage_ListView_handleSelectionChange = true;

                        // disable some stuff
                        settingsPage_WebcamEditorPage_ItemEditorGrid.Visibility = Visibility.Collapsed;

                        break;
                    }
                #endregion

                #region Home
                case 1:
                    {
                        break;
                    }
                #endregion

                #region Settings
                case 2:
                    {
                        break;
                    }
                #endregion

                #region User interface page
                case 3: // User interface
                    {
                        // Set some info
                        #region Accent color button
                        string accentcolorButton_descriptionText = settingsPage_UserInterfacePage_UI_AccentColorButton.Description.Substring(0, 65);
                        settingsPage_UserInterfacePage_UI_AccentColorButton.Description = accentcolorButton_descriptionText + "Blue"; // "Blue" will equal current color from the constant array
                        #endregion

                        break;
                    }
                #endregion

                #region About page
                case 4:
                    {
                        #region Version info

                        #region Version number hackz
                        System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                        FileVersionInfo fileversioninfo = FileVersionInfo.GetVersionInfo(assembly.Location);
                        string versionnumber;

                        if (fileversioninfo.FileVersion.Substring(6, 1) == "0")
                            versionnumber = fileversioninfo.FileVersion.Substring(0, 5);
                        else
                            versionnumber = fileversioninfo.FileVersion;
                        #endregion

                        // Main version labels (on the top of the page, or "Hero labels")
                        // set settingsPage_AboutPage_ApplicationNameTextBlock.Text to localizated application name

                        settingsPage_AboutPage_VersionNumberTextBlock.Text = versionnumber + " (" + Properties.Settings.Default.versionid + ")";

                        // Debug version info
                        settingsPage_AboutPage_DebugVersionInfoGrid_VersionNumberTextBlock.Text = String.Format("Version: {0}", versionnumber);
                        settingsPage_AboutPage_DebugVersionInfoGrid_VersionNameTextBlock.Text = String.Format("Version name: {0}", Properties.Settings.Default.versionid);
                        settingsPage_AboutPage_DebugVersionInfoGrid_BuildIDTextBlock.Text = String.Format("Build ID: {0}", Properties.Settings.Default.buildid);

                        #endregion

                        break;
                    }
                #endregion

                #region Debug menu page
                case 5:
                    {


                        break;
                    }
                #endregion

                #region Prerelease program settings page
                case 6:
                    {
                        SetTitlebarButtonsStyle(0);

                        break;
                    }
                #endregion

                #region Default customizations debug page
                case 7:
                    {


                        break;
                    }
                    #endregion
            }
        }

        /// <summary>
        /// Toggles the setting, which is set in the sender button's Tag property, in Properties.Settings.Default.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async void settingsPage_ToggleButtonClick(object sender, RoutedEventArgs e)
        {
            settingsPage_ToggleSwitchButton sBtn = sender as settingsPage_ToggleSwitchButton;

            try
            {
                Properties.Settings.Default[(string)sBtn.Tag] = sBtn.IsActive;
                Properties.Settings.Default.Save();

                await GetUserConfiguration(true);
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

        #endregion

        #region Customizations delivery settings page

        private void customizationsdeliveryPage_AppBar_menuButton_Click(object sender, RoutedEventArgs e)
        {
            if (customizationsdeliveryPage_MenuGrid.Width == 48)
            {
                DoubleAnimation anim = new DoubleAnimation(330, TimeSpan.FromSeconds(0.5));
                anim.EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut };

                customizationsdeliveryPage_MenuGrid.BeginAnimation(Grid.WidthProperty, anim);
            }
            else
            {
                DoubleAnimation anim = new DoubleAnimation(48, TimeSpan.FromSeconds(0.5));
                anim.EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut };

                customizationsdeliveryPage_MenuGrid.BeginAnimation(Grid.WidthProperty, anim);
            }
        }

        private void Menu_NavigationButton_Click(object sender, RoutedEventArgs e)
        {
            Menu_NavigationButton button = sender as Menu_NavigationButton;

            foreach (Menu_NavigationButton btn in customizationsdeliveryPage_MenuGrid_TopStackPanel.Children)
            {
                if (btn.Tag == button.Tag)
                    btn.IsActive = true;
                else
                    btn.IsActive = false;
            }

            if ((string)button.Tag != "" & button.Tag != null)
            {
                foreach (Grid page in customizationsdeliveryPage_PagesGrid.Children)
                {
                    if ((string)page.Tag == (string)button.Tag)
                    {
                        page.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        page.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        #endregion

        int current_page;

        /// <summary>
        /// Transition to a page with it's respective animation.
        /// </summary>
        /// <param name="page">0 = Home, 1 = Settings, 2 = Customizations delivery settings</param>
        async void SwitchToPage(int page, bool noanim = false)
        {
            switch (page)
            {
                case 0: // Webcam page
                    {
                        current_page = 0;

                        Storyboard s = (Storyboard)FindResource("webcamPage_In");
                        s.Begin();
                        if (noanim) s.SkipToFill();

                        if (webcamPage_menuGrid.Visibility == Visibility.Visible)
                        {
                            webcamPage_menuButton.Visibility = Visibility.Collapsed;
                            backButton.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            webcamPage_menuButton.Visibility = Visibility.Visible;
                            backButton.Visibility = Visibility.Collapsed;
                        }

                        await GetUserConfiguration(true);

                        SetTitlebarButtonsStyle(0);

                        break;
                    }
                case 1: // Settings page
                    {
                        current_page = 1;

                        Storyboard s = (Storyboard)FindResource("settingsPage_In");
                        s.Begin();
                        if (noanim) s.SkipToFill();

                        webcamPage_menuButton.Visibility = Visibility.Collapsed;
                        backButton.Visibility = Visibility.Visible;

                        SetTitlebarButtonsStyle(2);

                        break;
                    }
                case 2: // Customizations delivery settings page
                    {
                        current_page = 2;

                        Storyboard s = (Storyboard)FindResource("customizationsdeliveryPage_In");
                        s.Begin();
                        if (noanim) s.SkipToFill();

                        webcamPage_menuButton.Visibility = Visibility.Collapsed;
                        backButton.Visibility = Visibility.Visible;

                        SetTitlebarButtonsStyle(0);

                        break;
                    }
            }
        }

        /// <summary>
        /// Sets the style of the titlebar buttons to a theme. Use when the theme changes.
        /// </summary>
        /// <param name="style">The theme to change the buttons to. 0 = Dark, 1 = Light, 2 = Settings theme</param>
        void SetTitlebarButtonsStyle(int style, bool windowcontrolsonly = false)
        {
            // 0: Dark
            // 1: Light

            // Styles
            Style darkStyle = Application.Current.Resources["titlebarButton"] as Style;
            Style lightStyle = Application.Current.Resources["titlebarButton_Light"] as Style;

            Style styleToApply = null;

            // Storyboards
            Storyboard s_toDark = (Storyboard)FindResource("titlebar_toDark");
            Storyboard s_toLight = (Storyboard)FindResource("titlebar_toLight");

            switch (style)
            {
                case 0:
                    {
                        styleToApply = darkStyle;
                        s_toDark.Begin();

                        break;
                    }
                case 1:
                    {
                        styleToApply = lightStyle;
                        s_toLight.Begin();

                        break;
                    }
                case 2:
                    {
                        SolidColorBrush settingsBackground_Brush = (SolidColorBrush)Application.Current.Resources["settingsPage_background"];

                        if (settings_showtitlebarcolor)
                            SetTitlebarButtonsStyle(0);
                        else
                        {
                            if (settingsBackground_Brush.Color == Colors.White)
                            {
                                styleToApply = lightStyle;
                                s_toLight.Begin();
                            }
                            else
                            {
                                styleToApply = darkStyle;
                                s_toDark.Begin();
                            }
                        }

                        break;
                    }
            }

            // Apply the style to the buttons
            closeButton.Style = styleToApply;
            maximizeButton.Style = styleToApply;
            minimizeButton.Style = styleToApply;

            backButton.Style = styleToApply;
            webcamPage_menuButton.Style = styleToApply;
        }

        #endregion

        #region Variables

        bool home_blurbehind;
        bool home_archiveorg;
        bool home_debugoverlay;

        bool home_experiment_Overview;

        bool settings_showtitlebarcolor;
        bool settings_experiment_UpdateUI;

        #endregion

        #region Configuration

        /// <summary>
        /// Gets the user's settings from Properties.Settings.Default and applies the settings if desired.
        /// </summary>
        async Task<int> GetUserConfiguration(bool applysettings = false)
        {
            #region Populate camera list
            webcamPage_menuGrid_cameraListStackPanel.Children.Clear();

            int cameracounter = 0;
            foreach (string camera in Properties.Settings.Default.camera_names)
            {
                webcamPage_menuGrid_CameraButton button = new webcamPage_menuGrid_CameraButton();
                button.Text = camera; button.Tag = cameracounter;
                button.Click += webcamPage_menuGrid_cameraButton_Click;

                webcamPage_menuGrid_cameraListStackPanel.Children.Add(button);

                cameracounter++;
            }

            #endregion

            // Get settings
            home_blurbehind = Properties.Settings.Default.home_blurbehind;
            home_archiveorg = Properties.Settings.Default.home_archiveorg;
            home_debugoverlay = Properties.Settings.Default.home_debugoverlay;

            settings_showtitlebarcolor = Properties.Settings.Default.settings_showtitlebarcolor;
            settings_experiment_UpdateUI = Properties.Settings.Default.settings_experiment_UpdateUI;
            home_experiment_Overview = Properties.Settings.Default.home_experiment_Overview;

            if (applysettings)
            {
                await ApplyUserConfiguration();
            }

            return 0;
        }

        #region Suppress async warning
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        #endregion
        /// <summary>
        /// Applies the user's settings from the variables, NOT Properties.Settings.Default. That means, you first have to GetUserConfiguration and then you can ApplyUserConfiguration.
        /// </summary>
        async Task ApplyUserConfiguration()
        {
            #region Apply configuration

            // home - archive.org
            if (home_archiveorg)
            {
                webcampage_menuGrid_archiveorgSaveButton.Visibility = Visibility.Visible;
                webcamPage_menuGrid_BothSaveButton.Visibility = Visibility.Visible;
            }
            else
            {
                webcampage_menuGrid_archiveorgSaveButton.Visibility = Visibility.Collapsed;
                webcamPage_menuGrid_BothSaveButton.Visibility = Visibility.Collapsed;
            }

            // home - menu blurbehind
            if (home_blurbehind)
                webcamPage_menuGrid_blurbehindBorder.Visibility = Visibility.Visible;
            else
                webcamPage_menuGrid_blurbehindBorder.Visibility = Visibility.Collapsed;

            // home - debug overlay
            if (home_debugoverlay)
                webcamPage_debugoverlayGrid.Visibility = Visibility.Visible;
            else
                webcamPage_debugoverlayGrid.Visibility = Visibility.Collapsed;

            // settings - show titlebar color
            if (settings_showtitlebarcolor)
            {
                settingsPage_rectangle.Fill = Application.Current.Resources["accentcolor_dark"] as SolidColorBrush;
                SetTitlebarButtonsStyle(0);
            }
            else
            {
                settingsPage_rectangle.Fill = Application.Current.Resources["settingsPage_background"] as SolidColorBrush;
                SetTitlebarButtonsStyle(2);
            }

            // EXPERIMENT
            // settings - update ui
            if (settings_experiment_UpdateUI)
                settingsPage_AboutPage_UpdatesGrid.Visibility = Visibility.Visible;
            else
                settingsPage_AboutPage_UpdatesGrid.Visibility = Visibility.Collapsed;

            // EXPERIMENT
            // home - overview
            if (home_experiment_Overview)
                webcamPage_menuGrid_ShowOverviewButton();
            else
                webcamPage_menuGrid_HideOverviewButton(true);

            #endregion
        }

        #endregion
    }
}