using MahApps.Metro.Controls;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
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
using System.Windows.Threading;
using WebcamViewer.User_controls;

namespace WebcamViewer
{
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("fr"); // testing MUI
            InitializeComponent();

            webcamPage_menuGrid_progressInStoryboard = (Storyboard)FindResource("webcamPage_menuGrid_progressIn");
            webcamPage_menuGrid_progressOutStoryboard = (Storyboard)FindResource("webcamPage_menuGrid_progressOut");
        }

        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            // Get the user configuration, also populates the webcam list
            GetUserConfiguration();

            // Switch to the webcam page
            SwitchToPage(0, true);

            // Load the first camera
            webcamPage_menuGrid_CameraButton firstCameraButton = webcamPage_menuGrid_cameraListStackPanel.Children[0] as webcamPage_menuGrid_CameraButton;
            webcamPage_menuGrid_cameraButton_Click(firstCameraButton, new RoutedEventArgs());

            // Set up autorefreshing
            refreshtimer.Tick += Refreshtimer_Tick;

            // Set up logging
            if (Properties.Settings.Default.app_logging)
            {
                Debug.Log("");
                Debug.Log("--------------------");
                Debug.Log("Webcam Viewer " + Properties.Settings.Default.versionid);
                Debug.Log("App started at: " + DateTime.Now);
                Debug.Log("--------------------");
            }
        }

        #region Classes

        Theming Theming = new Theming();
        Configuration Configuration = new Configuration();
        Debug Debug = new Debug();

        #endregion

        #region Titlebar & window

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
                maximizeButton.TextIcon = "\ue923";
                grid.Margin = new Thickness(0);
            }
            else
            {
                maximizeButton.TextIcon = "\ue922";
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
            if (current_page == 3) // if we're on Internal settings page
            {
                SwitchToPage(1);

                // change title
                titleLabel.Content = Properties.Resources.App_Title;
            }
        }

        // Context menu

        private void titlebarGrid_ContextMenu_SetWindowSizeItem_Click(object sender, RoutedEventArgs e)
        {
            Popups.MessageDialog dlg = new Popups.MessageDialog
            {
                Title = "Set window size",
                SecondButtonContent = "Accept",
                FirstButtonContent = "Cancel"
            };

            StackPanel dlg_panel = new StackPanel();

            TextBlock text0 = new TextBlock { Text = "Enter the desired width and height for the window below. (32 pixels for the titlebar is automatically added!)" };

            TextBox boxWidth = new TextBox { Margin = new Thickness(0, 10, 0, 0) }; TextBoxHelper.SetWatermark(boxWidth, "Width");

            TextBox boxHeight = new TextBox { Margin = new Thickness(0, 10, 0, 0) }; TextBoxHelper.SetWatermark(boxHeight, "Height");

            dlg_panel.Children.Add(text0);
            dlg_panel.Children.Add(boxWidth);
            dlg_panel.Children.Add(boxHeight);

            dlg.Content = dlg_panel;

            if (dlg.ShowDialogWithResult() == 1)
            {
                double width;
                double height;

                try
                {
                    width = double.Parse(boxWidth.Text);
                    height = double.Parse(boxHeight.Text);
                }
                catch
                {
                    TextMessageDialog("Invalid resolution", "Double check the values and try again.");
                    return;
                }

                if (!(width > this.MinWidth & height > this.MinHeight /* */ & width < this.MaxWidth & height < this.MaxHeight))
                {
                    TextMessageDialog("Invalid resolution", "You've exceeded either the minimum or maximum size of the window.");
                    return;
                }
                else
                {
                    this.Width = double.Parse(boxWidth.Text) + 2;
                    this.Height = double.Parse(boxHeight.Text) + titlebarGrid.Height + 2;
                    CenterWindowOnScreen();
                }
            }

        }

        private void titlebarGrid_ContextMenu_SizeWindowToImageItem_Click(object sender, RoutedEventArgs e)
        {
            this.Width = webcamPage_Image.Source.Width + 2;
            this.Height = webcamPage_Image.Source.Height + titlebarGrid.Height + 2;
            CenterWindowOnScreen();
        }

        private void titlebarGrid_ContextMenu_ResetWindowSizeItem_Click(object sender, RoutedEventArgs e)
        {
            this.Width = 802;
            this.Height = 634;
            CenterWindowOnScreen();
        }

        // ----- window ----- //
        void CenterWindowOnScreen()
        {
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;
            double windowWidth = this.Width;
            double windowHeight = this.Height;
            this.Left = (screenWidth / 2) - (windowWidth / 2);
            this.Top = (screenHeight / 2) - (windowHeight / 2);
        }

        // Global dimming

        Storyboard globaldim_board_dim = new Storyboard();
        Storyboard globaldim_board_undim = new Storyboard();

        DispatcherTimer globaldim_undim_timer = new DispatcherTimer();
        bool globaldim_undim_timer_appliedTickEvent = false;

        /// <summary>
        /// Dims the program UI. (note: it also dims (thus blocks) the titlebar)
        /// </summary>
        public void global_Dim()
        {
            global_dimGrid.Opacity = 0;
            global_dimGrid.Visibility = Visibility.Visible;

            DoubleAnimation animation_dim = new DoubleAnimation(1, TimeSpan.FromSeconds(0.3));

            globaldim_board_dim.Children.Clear();
            globaldim_board_dim.Children.Add(animation_dim);
            animation_dim.SetValue(Storyboard.TargetNameProperty, global_dimGrid.Name);
            animation_dim.SetValue(Storyboard.TargetPropertyProperty, new PropertyPath(FrameworkElement.OpacityProperty));

            // stop undim if running
            globaldim_board_undim.Stop();
            globaldim_undim_timer.Stop();

            globaldim_board_dim.Begin(this);

            // remove inactive border
            this.NonActiveBorderBrush = this.BorderBrush;
            this.NonActiveGlowBrush = this.GlowBrush;
        }

        public void global_UnDim()
        {
            DoubleAnimation animation_undim = new DoubleAnimation(0, TimeSpan.FromSeconds(0.3));

            globaldim_board_undim.Children.Clear();
            globaldim_board_undim.Children.Add(animation_undim);
            animation_undim.SetValue(Storyboard.TargetNameProperty, global_dimGrid.Name);
            animation_undim.SetValue(Storyboard.TargetPropertyProperty, new PropertyPath(FrameworkElement.OpacityProperty));

            // stop dim if running
            globaldim_board_dim.Stop();

            globaldim_board_undim.Begin(this);

            globaldim_undim_timer.Interval = TimeSpan.FromSeconds(0.5);
            if (globaldim_undim_timer_appliedTickEvent == false)
                globaldim_undim_timer.Tick += (s, ev) => { globaldim_undim_timer.Stop(); global_dimGrid.Visibility = Visibility.Collapsed; };
            globaldim_undim_timer.Start();

            globaldim_undim_timer_appliedTickEvent = true;

            // restore inactive border
            this.NonActiveBorderBrush = Application.Current.Resources["App_NonActiveBorderBrush"] as SolidColorBrush;
            this.NonActiveGlowBrush = Application.Current.Resources["App_NonActiveGlowBrush"] as SolidColorBrush;
        }
        // ----- window ----- //

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

        public void webcamPage_OpenMenu()
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

                // Titlebar text theming

                if (ui_theme != 1)
                {
                    backButton.Foreground = Application.Current.Resources["webcamPage_Foreground"] as SolidColorBrush;
                    titleLabel.Foreground = Application.Current.Resources["webcamPage_Foreground"] as SolidColorBrush;
                }

                // Blur

                DoubleAnimation menuGrid_blurbehind_radiusAnimation = new DoubleAnimation(5, new TimeSpan(0, 0, 0, 0, 300).Duration());

                DispatcherTimer timer = new DispatcherTimer();
                timer.Interval = menuGrid_movementAnimation.Duration.TimeSpan;
                timer.Tick += (s, ev) => { timer.Stop(); webcamPage_menuGrid_blurbehindBorder.BeginAnimation(Border.OpacityProperty, menuGrid_blurbehindBorder_opacityAnimation); webcamPage_menuGrid_blurbehind.BeginAnimation(System.Windows.Media.Effects.BlurEffect.RadiusProperty, menuGrid_blurbehind_radiusAnimation); };
                timer.Start();

                // Back button

                if (AllowBackButtonLogic)
                {
                    backButton.Visibility = Visibility.Visible;
                    webcamPage_menuButton.Visibility = Visibility.Collapsed;
                }
            }
        }

        public void webcamPage_CloseMenu()
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

                // Titlebar theming

                // Titlebar text theming

                if (ui_theme != 1)
                {
                    SetTitlebarButtonsStyle(0);
                }

                // Blur

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

        void webcamPage_menuGrid_HideOverviewButton()
        {
            DoubleAnimation anim = new DoubleAnimation(0, TimeSpan.FromSeconds(.5));

            webcamPage_menuGrid_overviewButton.BeginAnimation(OpacityProperty, anim);

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(.5);
            timer.Tick += (s, ev) => { timer.Stop(); webcamPage_menuGrid_overviewButton.Visibility = Visibility.Collapsed; menuGrid_rowdefinition_overviewButton.Height = new GridLength(0); };
            timer.Start();
        }

        void webcamPage_menuGrid_ShowOverviewButton()
        {
            DoubleAnimation anim = new DoubleAnimation(1, TimeSpan.FromSeconds(.5));

            menuGrid_rowdefinition_overviewButton.Height = new GridLength(47);

            webcamPage_menuGrid_overviewButton.Visibility = Visibility.Visible;

            webcamPage_menuGrid_overviewButton.BeginAnimation(OpacityProperty, anim);
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

        private async void webcamPage_menuGrid_BothSaveButton_Click(object sender, RoutedEventArgs e)
        {
            webcamPage_menuGrid_localSaveButton_Click(this, new RoutedEventArgs());
            await SaveOnArchiveOrg();
        }

        private async void webcampage_menuGrid_archiveorgSaveButton_Click(object sender, RoutedEventArgs e)
        {
            await SaveOnArchiveOrg();
        }

        private void webcamPage_menuGrid_overviewButton_Click(object sender, RoutedEventArgs e)
        {
            if (webcamPage_MainContentGrid_OverviewGrid.Visibility != Visibility.Visible)
                webcamPage_MainContentGrid_SwitchToOverview();
            else
                webcamPage_MainContentGrid_SwitchToCameraView();

            webcamPage_CloseMenu();
        }

        async void webcamPage_menuGrid_cameraButton_Click(object sender, RoutedEventArgs e)
        {
            webcamPage_menuGrid_CameraButton sBtn = sender as webcamPage_menuGrid_CameraButton;

            try
            {
                await LoadImage(Properties.Settings.Default.camera_urls[(int)sBtn.Tag]);
            }
            catch (Exception ex)
            {
                webcamPage_Image.Visibility = Visibility.Collapsed;

                Popups.MessageDialog_FullWidth dlg = new Popups.MessageDialog_FullWidth()
                {
                    Title = "Could not load webcam image...",
                    Content = "An error occured while trying to load the webcam image...\n\n" +
                    "Error: " + ex.Message,
                    FirstButtonContent = "Close"
                };

                dlg.ShowDialog();
            }

            webcamPage_CloseMenu();
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
        //BitmapImage bimage;
        string camera_dummy()
        {
            return "?dummy=" + DateTime.Now.Ticks;
        }

        async Task LoadImage(string Url, bool Autorefresh = false)
        {
            StopRefresh();

            if (!Autorefresh)
                webcamPage_ShowProgressUI(0);

            #region Legacy
            /*
            bimage = new BitmapImage(new Uri(Url + camera_dummy));
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
            */
            #endregion

            using (WebClient client = new WebClient())
            {
                try
                {
                    var bytes = await client.DownloadDataTaskAsync(Url + camera_dummy());

                    var image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.None;
                    image.StreamSource = new MemoryStream(bytes);
                    image.EndInit();

                    webcamPage_Image.Source = image;

                    if (webcamPage_Image.Visibility != Visibility.Visible)
                        webcamPage_Image.Visibility = Visibility.Visible;
                }
                catch (WebException ex)
                {
                    webcamPage_Image.Visibility = Visibility.Collapsed;

                    Popups.MessageDialog_FullWidth dlg = new Popups.MessageDialog_FullWidth()
                    {
                        Title = "Could not load webcam image...",
                        Content = "An error occured while trying to load the webcam image...\n\n" +
                        "Error: " + ex.Message,
                        SecondButtonContent = "Close",
                        FirstButtonContent = "Try again"
                    };

                    if (dlg.ShowDialogWithResult() == 0)
                    {
                        await LoadImage(Url);
                        return;
                    }
                }
                finally
                {
                    webcamPage_HideProgressUI(1);
                    #region Update info
                    currentImageUri = new Uri(Properties.Settings.Default.camera_urls[Properties.Settings.Default.camera_urls.IndexOf(Url)]);
                    webcamPage_menuGrid_cameraNameLabel.Text = Properties.Settings.Default.camera_names[(Properties.Settings.Default.camera_urls.IndexOf(Url))].ToUpper();
                    webcamPage_menuGrid_cameraUrlLabel.Text = currentImageUri.ToString();

                    webcamPage_menuGrid_cameraNameLabel.ToolTip = Properties.Settings.Default.camera_names[(Properties.Settings.Default.camera_urls.IndexOf(Url))];
                    webcamPage_menuGrid_cameraUrlLabel.ToolTip = currentImageUri.ToString();
                    #endregion
                }

                // start refreshtimer
                StartRefresh();
            }
        }

        #region Refreshing

        DispatcherTimer refreshtimer = new DispatcherTimer();

        public void StartRefresh()
        {
            int interval = 0;

            try
            {
                interval = int.Parse(Properties.Settings.Default.camera_refreshrates[Properties.Settings.Default.camera_urls.IndexOf(currentImageUri.ToString())]);
            }
            catch
            {
                // cannot parse interval
            }

            if (interval != 0)
            {
                // set the timer to tick at interval
                refreshtimer.Interval = TimeSpan.FromSeconds(interval);

                // start the refreshtimer
                if (home_refreshenabled) // if refreshing is enabled
                    refreshtimer.Start();
                else
                    refreshtimer.Stop();
            }
            else
                StopRefresh();
        }

        public void StopRefresh()
        {
            refreshtimer.Stop();
        }

        private async void Refreshtimer_Tick(object sender, EventArgs e)
        {
            await LoadImage(currentImageUri.ToString(), true);
        }

        #endregion

        // image saving //

        #region Image saving

        WebClient Client = new WebClient();

        SaveFileDialog saveFileDialog = new SaveFileDialog();

        string whereToDownload;
        Uri UriToDownload;

        void SaveImageFile()
        {
            StopRefresh();

            //webcamPage_ShowProgressUI(3); webcamPage_progressBar.IsIndeterminate = true; webcamPage_progressLabel.Content = "preparing to download image...";
            webcamPage_ShowProgressUI(4); webcamPage_menuGrid_SetProgressText(Properties.Resources.webcamPage_LocalSaveProgressPreparing);
            //webcamPage_menuGrid_HideCamerasPanel(); webcamPage_menuGrid_HideOverviewButton();

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
                webcamPage_menuGrid_SetProgressText(Properties.Resources.webcamPage_LocalSaveProgress + e.ProgressPercentage + "%");

                webcamPage_progressBar.IsIndeterminate = false;
                webcamPage_progressBar.Maximum = e.TotalBytesToReceive;
                webcamPage_progressBar.Value = e.BytesReceived;
            }));
        }
        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                webcamPage_menuGrid_SetProgressText(Properties.Resources.webcamPage_LocalSaveProgressFinished);
                webcamPage_HideProgressUI(0);
                StartRefresh();
                //webcamPage_menuGrid_ShowCamerasPanel(); webcamPage_menuGrid_ShowOverviewButton();
                //refreshtimer.Start();

                if (e.Error != null)
                {
                    TextMessageDialog_FullWidth("Cannot download image", "An error occured while trying to download the image.\nError: " + e.Error.Message);
                    File.Delete(whereToDownload);
                }
            }));
        }

        #endregion

        /* legacy archive.org saving
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

        */

        async Task SaveOnArchiveOrg()
        {
            string uri = "http://web.archive.org/save/" + currentImageUri;

            /*
            archivebrowser.Url = new Uri("http://web.archive.org/save/" + currentImageUri);
            //webcamPage_ShowProgressUI(3); webcamPage_progressBar.IsIndeterminate = true; webcamPage_progressLabel.Content = "connecting to archive.org...";
            webcamPage_ShowProgressUI(4); webcamPage_menuGrid_SetProgressText("Preparing to save on archive.org...");
            //webcamPage_menuGrid_HideCamerasPanel(); webcamPage_menuGrid_HideOverviewButton();
            */

            StopRefresh();

            webcamPage_ShowProgressUI(4); webcamPage_menuGrid_SetProgressText(Properties.Resources.webcamPage_ArchiveOrgProgress); // Saving to archive.org...

            try
            {
                WebRequest request = WebRequest.Create(uri);

                using (WebResponse _response = await request.GetResponseAsync())
                {
                    HttpWebResponse response = _response as HttpWebResponse;

                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        Popups.MessageDialog_FullWidth dlg = new Popups.MessageDialog_FullWidth();
                        dlg.Title = "The server returned an error...";

                        dlg.Content = "Try again...?\n\n" +
                            "Status code: " + (int)response.StatusCode + " " + response.StatusCode.ToString() +
                            "Status code description: " + response.StatusDescription;

                        dlg.FirstButtonContent = "Try again";
                        dlg.SecondButtonContent = "OK";

                        if (dlg.ShowDialogWithResult() == 0)
                        {
                            webcampage_menuGrid_archiveorgSaveButton_Click(this, new RoutedEventArgs());
                            return;
                        }
                    }
                    else
                        webcamPage_menuGrid_SetProgressText(Properties.Resources.webcamPage_ArchiveOrgProgressFinished); // Saved!
                }
            }
            catch (WebException ex)
            {
                Popups.MessageDialog_FullWidth dlg = new Popups.MessageDialog_FullWidth();

                dlg.Title = "Could not connect to archive.org...";
                dlg.Content = "Error: " + ex.Message;

                dlg.FirstButtonContent = "Try again";
                dlg.SecondButtonContent = "OK";

                if (dlg.ShowDialogWithResult() == 0)
                {
                    webcampage_menuGrid_archiveorgSaveButton_Click(this, new RoutedEventArgs());
                    return;
                }
            }

            webcamPage_HideProgressUI(0);
            StartRefresh();
        }

        // ----- ----- //

        #endregion

        #region Overview

        private void webcamPage_MainContentGrid_OverviewGrid_menuButton_Click(object sender, RoutedEventArgs e)
        {
            if (webcamPage_menuGrid.Visibility != Visibility.Visible)
            {
                webcamPage_OpenMenu();
                backButton.Visibility = Visibility.Visible;
            }
        }

        private void webcamPage_MainContentGrid_OverviewGrid_moreButton_Click(object sender, RoutedEventArgs e)
        {

        }

        void webcamPage_MainContentGrid_SwitchToOverview()
        {
            Storyboard s = (Storyboard)FindResource("webcamPage_OverviewIn");
            s.Begin(); s.SetSpeedRatio(ui_animationspeed);

            webcamPage_menuGrid_overviewButton.IconText = "\ue72b";
            webcamPage_menuGrid_overviewButton.Text = Properties.Resources.webcamPage_OverviewGoBack;

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
            s.Begin(); s.SetSpeedRatio(ui_animationspeed);

            webcamPage_menuGrid_overviewButton.IconText = "\ue80a";
            webcamPage_menuGrid_overviewButton.Text = Properties.Resources.webcamPage_Overview;

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

        async void webcamPage_MainContentGrid_Overview_LoadContent()
        {
            // Progressbar
            webcamPage_MainContentGrid_OverviewGrid_ProgressBar.Visibility = Visibility.Visible;

            //webcamPage_OverviewGrid_ProgressBarRow.Height = new GridLength(5);

            // Clear out existing children
            webcamPage_MainContentGrid_OverviewGrid_WrapPanel.Children.Clear();

            // Add cameras
            int cameracounter = 0;
            foreach (string camera in Properties.Settings.Default.camera_names)
            {
                webcamPage_Overview_CameraButton button = new webcamPage_Overview_CameraButton()
                {
                    CameraName = camera,
                    Tag = cameracounter
                };

                button.Click += webcampage_MainContentGrid_Overview_CameraButtonClick;

                webcamPage_MainContentGrid_OverviewGrid_WrapPanel.Children.Add(button);

                cameracounter++;
            }

            // Download camera images to buttons
            int counter = 0;
            foreach (webcamPage_Overview_CameraButton button in webcamPage_MainContentGrid_OverviewGrid_WrapPanel.Children)
            {
                using (WebClient client = new WebClient())
                {
                    Uri url;

                    try
                    {
                        url = new Uri(Properties.Settings.Default.camera_urls[counter] + camera_dummy());
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message != null)
                        {
                            button.IsError = true;
                            button.ErrorMessage = ex.Message;
                        }

                        counter++;

                        if (counter == Properties.Settings.Default.camera_names.Count)
                        {
                            // Hide progressbar
                            webcamPage_MainContentGrid_OverviewGrid_ProgressBar.Visibility = Visibility.Hidden;
                            //webcamPage_OverviewGrid_ProgressBarRow.Height = new GridLength(0);
                        }

                        return;
                    }

                    // this for some reason goes infinitely on the new camera template
                    try
                    {
                        var bytes = await client.DownloadDataTaskAsync(url);

                        var image = new BitmapImage();
                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.StreamSource = new MemoryStream(bytes);
                        image.EndInit();

                        button.Image = image;
                    }
                    catch (WebException ex)
                    {
                        button.IsError = true;

                        if (ex.Message != null)
                        {
                            button.ErrorMessage = ex.Message;
                        }
                    }
                }

                counter++;

                if (counter == Properties.Settings.Default.camera_names.Count)
                {
                    // Hide progressbar
                    webcamPage_MainContentGrid_OverviewGrid_ProgressBar.Visibility = Visibility.Hidden;
                    //webcamPage_OverviewGrid_ProgressBarRow.Height = new GridLength(0);
                }
            }
        }

        async void webcampage_MainContentGrid_Overview_CameraButtonClick(object sender, RoutedEventArgs e)



        {
            webcamPage_Overview_CameraButton button = sender as webcamPage_Overview_CameraButton;

            if (button.IsError != true)
            {
                webcamPage_MainContentGrid_SwitchToCameraView();

                try
                {
                    await LoadImage(Properties.Settings.Default.camera_urls[(int)button.Tag]);
                }
                catch (Exception ex)
                {
                    webcamPage_Image.Visibility = Visibility.Collapsed;

                    Popups.MessageDialog_FullWidth dlg = new Popups.MessageDialog_FullWidth()
                    {
                        Title = "Could not load webcam image...",
                        Content = "An error occured while trying to load the webcam image...\n\n" +
                        "Error: " + ex.Message,

                        IsDarkTheme = true,
                        FirstButtonContent = "Close"
                    };

                    dlg.ShowDialog();
                }
            }
            else
            {
                Popups.MessageDialog dlg = new Popups.MessageDialog();
                dlg.Title = "Could not load image...";
                dlg.Content = "Could not load webcam image...\n\nError: " + button.ErrorMessage + "\n\nWould you like to reload overview?";
                dlg.IsDarkTheme = true;
                dlg.FirstButtonContent = "Close";
                dlg.SecondButtonContent = "Reload overview";

                if (dlg.ShowDialogWithResult() == 1)
                {
                    webcamPage_MainContentGrid_Overview_LoadContent();
                }
            }
        }

        #endregion

        Storyboard webcamPage_menuGrid_progressInStoryboard;
        Storyboard webcamPage_menuGrid_progressOutStoryboard;

        bool IsMenuProgressHappening = false;

        /// <summary>
        /// Shows the progress UI.
        /// </summary>
        /// <param name="mode">The kind of progress UI to show. 0 = ProgressRing only, 1 = ProgressRing + text, 2 = ProgressBar only, 3 = ProgressBar + text, 4 = sidemenu ProgressRing</param>
        public void webcamPage_ShowProgressUI(int mode)
        {
            if (mode <= 3)
            {
                webcamPage_Dim.Opacity = 0;
                webcamPage_Dim.Visibility = Visibility.Visible;

                DoubleAnimation webcamPage_Dim_OpacityAnimation = new DoubleAnimation(0, 1, new TimeSpan(0, 0, 0, 0, 300).Duration());
                webcamPage_Dim.BeginAnimation(Grid.OpacityProperty, webcamPage_Dim_OpacityAnimation);

                if (ui_theme != 1)
                {
                    SetTitlebarButtonsStyle(1);
                }
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
                case 5:
                    {
                        // Autorefresh only
                        // shows nothing

                        break;
                    }
            }
        }

        /// <summary>
        /// Hides the progress UI.
        /// </summary>
        /// <param name="mode">The kind of progress UI to hide. 0 = in-menu, 1 = webcam page, 2 = all (it is 2 by default)</param>
        public void webcamPage_HideProgressUI(int mode = 2)
        {
            switch (mode)
            {
                case 0:
                    {
                        if (IsMenuProgressHappening == true)
                        {
                            // The storyboard handles the Visiblity and IsActive so no need to set it here :)

                            webcamPage_menuGrid_progressInStoryboard.Stop();
                            webcamPage_menuGrid_progressOutStoryboard.Begin();

                            IsMenuProgressHappening = false;
                        }

                        break;
                    }
                case 1:
                    {
                        if (webcamPage_Dim.Visibility == Visibility.Visible)
                        {
                            DoubleAnimation webcamPage_Dim_OpacityAnimation = new DoubleAnimation(1, 0, new TimeSpan(0, 0, 0, 0, 300).Duration());
                            webcamPage_Dim.BeginAnimation(Grid.OpacityProperty, webcamPage_Dim_OpacityAnimation);

                            if (ui_theme != 1)
                            {
                                SetTitlebarButtonsStyle(0);
                            }

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

                        break;
                    }
                case 2:
                    {
                        webcamPage_HideProgressUI(0);
                        webcamPage_HideProgressUI(1);

                        break;
                    }
            }
        }

        public void webcamPage_menuGrid_SetProgressText(string text)
        {
            webcamPage_menuGrid_progressPanel_progressTextBlock.Text = text;
        }

        // ------------ //

        #endregion

        #region Settings page

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

            // legacy - modular settings pages
            /*
            // Page toggles
            // There should be a different way of getting toggles here, but for now, this is "working".

            // 100% total understandable explanations™ by XeZrunner:

            Grid grid = settingsPage_PagesGrid.Children[PageID] as Grid; // the Page™
            ScrollViewer grid_scrollview = null; // the page's main scrollviewer

            foreach (object viewer in grid.Children) // get the main scrollviewer
            {
                if (viewer.GetType() == (typeof(ScrollViewer))) // is there a scrollviewer?
                    grid_scrollview = viewer as ScrollViewer; // yee, continue
                                                              // if a page is not using a scrollviewer as it's content, it's PROBABLY not the right format. This is ugly and shouldn't be this way, but I'll rewrite it one day. Hopefully.
            }

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
            */
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

        public void Menu_NavigationButton_Click(object sender, RoutedEventArgs e)
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

        private void Menu_NavigationButton_Click_1(object sender, RoutedEventArgs e)
        {
            Popups.MessageDialog_FullWidth dlg = new Popups.MessageDialog_FullWidth()
            {
                Title = "Internal login",
                FirstButtonContent = "Log in",
                SecondButtonContent = "Cancel"
            };

            StackPanel panel = new StackPanel();

            Label lbl0 = new Label() { Content = "Log in to internal server" };

            TextBox usernamebox = new TextBox() { Margin = new Thickness(0, 10, 0, 0) };

            PasswordBox passwordbox = new PasswordBox() { Margin = new Thickness(0, 10, 0, 0) };

            panel.Children.Add(lbl0);
            panel.Children.Add(usernamebox);
            panel.Children.Add(passwordbox);

            dlg.Content = panel;

            if (dlg.ShowDialogWithResult() == 1)
            {
                // do stuff
            }
        }

        private void Menu_NavigationButton_Click_2(object sender, RoutedEventArgs e)
        {
            SwitchToPage(1);            
        }

        #endregion

        public int current_page;

        /// <summary>
        /// Transition to a page with it's respective animation.
        /// </summary>
        /// <param name="page">0 = Home, 1 = Settings, 2 = Customizations delivery settings</param>
        public void SwitchToPage(int page, bool noanim = false)
        {
            switch (page)
            {
                case 0: // Webcam page
                    {
                        current_page = 0;

                        Storyboard s = (Storyboard)FindResource("webcamPage_In");
                        s.Begin(); s.SetSpeedRatio(ui_animationspeed);
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

                        GetUserConfiguration(true);

                        // Titlebar theming
                        if (webcamPage_Dim.Visibility == Visibility.Visible & ui_theme != 1)
                        {
                            SetTitlebarButtonsStyle(1);
                        }
                        else if (webcamPage_menuGrid.Visibility == Visibility.Visible & ui_theme != 1)
                        {
                            SetTitlebarButtonsStyle(0);
                            backButton.Foreground = Application.Current.Resources["webcamPage_Foreground"] as SolidColorBrush;
                            titleLabel.Foreground = Application.Current.Resources["webcamPage_Foreground"] as SolidColorBrush;
                        }
                        else
                        {
                            SetTitlebarButtonsStyle(0);
                        }

                        StartRefresh();

                        break;
                    }
                case 1: // Settings page
                    {
                        current_page = 1;

                        Storyboard s = (Storyboard)FindResource("settingsPage_In");
                        s.Begin(); s.SetSpeedRatio(ui_animationspeed);
                        if (noanim) s.SkipToFill();

                        webcamPage_menuButton.Visibility = Visibility.Collapsed;
                        backButton.Visibility = Visibility.Visible;

                        SetTitlebarButtonsStyle(2);

                        StopRefresh();

                        break;
                    }
                case 2: // Customizations delivery settings page
                    {
                        current_page = 2;

                        Storyboard s = (Storyboard)FindResource("customizationsdeliveryPage_In");
                        s.Begin(); s.SetSpeedRatio(ui_animationspeed);
                        if (noanim) s.SkipToFill();

                        webcamPage_menuButton.Visibility = Visibility.Collapsed;
                        backButton.Visibility = Visibility.Visible;

                        SetTitlebarButtonsStyle(2);

                        StopRefresh();

                        break;
                    }
                case 3: // Internal settings page
                    {
                        current_page = 3;

                        Storyboard s = (Storyboard)FindResource("internalsettingsPage_In");
                        s.Begin(); s.SetSpeedRatio(ui_animationspeed);
                        if (noanim) s.SkipToFill();

                        webcamPage_menuButton.Visibility = Visibility.Collapsed;
                        backButton.Visibility = Visibility.Visible;

                        SetTitlebarButtonsStyle(2);

                        if (internalsettingsPage_Container_frame.Content == null) // we want to return to an existing instance once we were there already
                            internalsettingsPage_Container_frame.Navigate(new Pages.Internal_development_page.Page());

                        // change title
                        titleLabel.Content = Properties.Resources.App_Title + " - Internal settings";

                        StopRefresh();

                        break;
                    }
            }
        }

        /// <summary>
        /// Sets the style of the titlebar buttons to a theme. Use when the theme changes.
        /// </summary>
        /// <param name="style">The theme to change the buttons to. 0 = Dark, 1 = Light, 2 = Settings theme</param>
        void SetTitlebarButtonsStyle(int style)
        {
            // 0: Dark
            // 1: Light

            string styleString = "";

            // Storyboards
            Storyboard s_toDark = (Storyboard)FindResource("titlebar_toDark");
            Storyboard s_toLight = (Storyboard)FindResource("titlebar_toLight");

            switch (style)
            {
                case 0:
                    {
                        styleString = "dark";
                        s_toDark.Begin();

                        break;
                    }
                case 1:
                    {
                        styleString = "light";
                        s_toLight.Begin();

                        break;
                    }
                case 2:
                    {
                        SolidColorBrush settingsBackground_Brush = (SolidColorBrush)Application.Current.Resources["settingsPage_background"];

                        if (settings_showtitlebarcolor)
                        {
                            SetTitlebarButtonsStyle(0);
                            return;
                        }
                        else
                        {
                            if (ui_theme == 1)
                            {
                                styleString = "dark";
                                s_toDark.Begin();
                            }
                            else
                            {
                                styleString = "light";
                                s_toLight.Begin();
                            }
                        }

                        break;
                    }
            }

            // Apply the style to the buttons

            switch (styleString)
            {
                #region Dark
                case "dark":
                    {
                        foreach (User_controls.WindowControlButton button in windowcontrol_stackpanel.Children)
                        {
                            string _btnTag = button.Tag as string;
                            if (_btnTag != "closeButton")
                            {
                                button.Tag = "dark";
                            }
                        }

                        foreach (User_controls.WindowControlButton button in titlebarGrid_leftcontrols.Children)
                        {
                            string _btnTag = button.Tag as string;
                            if (_btnTag != "closeButton")
                            {
                                button.Tag = "dark";
                            }
                        }

                        break;
                    }
                #endregion

                #region Light
                case "light":
                    {
                        foreach (User_controls.WindowControlButton button in windowcontrol_stackpanel.Children)
                        {
                            string _btnTag = button.Tag as string;
                            if (_btnTag != "closeButton")
                            {
                                button.Tag = "light";
                            }
                        }

                        foreach (User_controls.WindowControlButton button in titlebarGrid_leftcontrols.Children)
                        {
                            string _btnTag = button.Tag as string;
                            if (_btnTag != "closeButton")
                            {
                                button.Tag = "light";
                            }
                        }

                        break;
                    }
                    #endregion
            }
        }

        #endregion

        #region Theming

        void UI_ApplyTheme(int theme)
        {

            string themeString;

            if (theme == 0)
                themeString = "Light";
            else
                themeString = "Dark";

            // set the resources

            Application.Current.Resources["webcamPage_Foreground"] = Application.Current.Resources["webcamPage_" + themeString + "_Foreground"];
            Application.Current.Resources["webcamPage_menuBackground"] = Application.Current.Resources["webcamPage_" + themeString + "_menuBackground"];
            Application.Current.Resources["webcamPage_menuBackgroundSecondary"] = Application.Current.Resources["webcamPage_" + themeString + "_menuBackgroundSecondary"];

            Application.Current.Resources["settingsPage_background"] = Application.Current.Resources["settingsPage_" + themeString + "_background"];
            Application.Current.Resources["settingsPage_backgroundSecondary"] = Application.Current.Resources["settingsPage_" + themeString + "_backgroundSecondary"];
            Application.Current.Resources["settingsPage_backgroundSecondary2"] = Application.Current.Resources["settingsPage_" + themeString + "_backgroundSecondary2"];
            Application.Current.Resources["settingsPage_backgroundSecondary3"] = Application.Current.Resources["settingsPage_" + themeString + "_backgroundSecondary3"];
            Application.Current.Resources["settingsPage_backgroundWebcamItemEditor"] = Application.Current.Resources["settingsPage_" + themeString + "_backgroundWebcamItemEditor"];

            Application.Current.Resources["settingsPage_foregroundText"] = Application.Current.Resources["settingsPage_" + themeString + "_foregroundText"];
            Application.Current.Resources["settingsPage_foregroundSecondary"] = Application.Current.Resources["settingsPage_" + themeString + "_foregroundSecondary"];
            Application.Current.Resources["settingsPage_foregroundSecondary2"] = Application.Current.Resources["settingsPage_" + themeString + "_foregroundSecondary2"];
            Application.Current.Resources["settingsPage_foregroundSecondary3"] = Application.Current.Resources["settingsPage_" + themeString + "_foregroundSecondary3"];

            Application.Current.Resources["MessageDialog_Background"] = Application.Current.Resources["MessageDialog_" + themeString + "_Background"];
            Application.Current.Resources["MessageDialog_ForegroundText"] = Application.Current.Resources["MessageDialog_" + themeString + "_ForegroundText"];
            Application.Current.Resources["MessageDialog_ForegroundSecondary"] = Application.Current.Resources["MessageDialog_" + themeString + "_ForegroundSecondary"];

            Application.Current.Resources["MessageDialog_FullWidth_Background"] = Application.Current.Resources["MessageDialog_FullWidth_" + themeString + "_Background"];
            Application.Current.Resources["MessageDialog_FullWidth_ForegroundText"] = Application.Current.Resources["MessageDialog_FullWidth_" + themeString + "_ForegroundText"];
            Application.Current.Resources["MessageDialog_FullWidth_ForegroundSecondary"] = Application.Current.Resources["MessageDialog_FullWidth_" + themeString + "_ForegroundSecondary"];
        }

        void UI_ApplyAccentColor(int color)
        {

        }

        #endregion

        #region Configuration

        #region Variables

        int ui_theme;
        int ui_accent;
        double ui_animationspeed;

        bool home_refreshenabled;
        bool home_blurbehind;
        bool home_archiveorg;
        bool home_debugoverlay;

        bool settings_showtitlebarcolor;
        bool settings_experiment_UpdateUI;

        bool app_firstrun;

        string debugmode;

        #endregion

        /// <summary>
        /// Gets the user's settings from Properties.Settings.Default and applies the settings if desired.
        /// </summary>
        public void GetUserConfiguration(bool applysettings = false)
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
            ui_theme = Properties.Settings.Default.ui_theme;
            ui_accent = Properties.Settings.Default.ui_accent;
            ui_animationspeed = Properties.Settings.Default.ui_animationspeed;

            home_refreshenabled = Properties.Settings.Default.home_refreshenabled;
            home_blurbehind = Properties.Settings.Default.home_blurbehind;
            home_archiveorg = Properties.Settings.Default.home_archiveorg;
            home_debugoverlay = Properties.Settings.Default.home_debugoverlay;

            settings_showtitlebarcolor = Properties.Settings.Default.settings_showtitlebarcolor;
            settings_experiment_UpdateUI = Properties.Settings.Default.settings_experiment_UpdateUI;

            app_firstrun = Properties.Settings.Default.app_firstrun;

            debugmode = Properties.Settings.Default.app_debugmode;

            if (applysettings)
            {
                ApplyUserConfiguration();
            }
        }

        #region Suppress async warning
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        #endregion
        /// <summary>
        /// Applies the user's settings from the variables, NOT Properties.Settings.Default. That means, you first have to GetUserConfiguration and then you can ApplyUserConfiguration.
        /// </summary>
        public void ApplyUserConfiguration()
        {
            #region Apply configuration

            // ui - theme
            Theming.Theme.SetTheme(ui_theme);

            // ui - accent
            Theming.AccentColor.SetAccentColor(ui_accent);

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
                internalsettingsPage_Rectangle.Fill = Application.Current.Resources["accentcolor_dark"] as SolidColorBrush;
                webcamPage_Rectangle.Fill = Application.Current.Resources["accentcolor_dark"] as SolidColorBrush;
                SetTitlebarButtonsStyle(0);
            }
            else
            {
                settingsPage_rectangle.Fill = Application.Current.Resources["settingsPage_background"] as SolidColorBrush;
                internalsettingsPage_Rectangle.Fill = Application.Current.Resources["settingsPage_background"] as SolidColorBrush;
                webcamPage_Rectangle.Fill = System.Windows.Media.Brushes.Black;
                if (current_page == 1)
                    SetTitlebarButtonsStyle(2);
                else
                    SetTitlebarButtonsStyle(0);
            }

            // EXPERIMENT
            // settings - update ui
            if (settings_experiment_UpdateUI)
                settingsPage_AboutPage_Control.settingsPage_AboutPage_UpdatesControl.Visibility = Visibility.Visible;
            else
                settingsPage_AboutPage_Control.settingsPage_AboutPage_UpdatesControl.Visibility = Visibility.Collapsed;

            // Debug mode
            if (debugmode == "release")
            {
                settingsPage_leftGrid_DebugSettingsTabButton.Visibility = Visibility.Collapsed;
            }
            else if (debugmode == "testing")
            {
                
            }
            settingsPage_leftGrid_DebugSettingsTabButton.Description = "Debug mode: " + debugmode;

            // App - first run UX
            if (app_firstrun)
                App_StartFirstRunUX();

            #endregion
        }

        void App_StartFirstRunUX()
        {
            Pages.First_run_page.firstrunPage_Control viewhost = new Pages.First_run_page.firstrunPage_Control();
            firstrunPage_ViewHost.Content = viewhost;
        }

        #endregion
    }
}