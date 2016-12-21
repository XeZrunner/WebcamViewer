using MahApps.Metro.Controls;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using WebcamViewer.Updates.User_controls;

namespace WebcamViewer.Updates
{
    public partial class MainWindow : MetroWindow
    {
        // init
        public MainWindow()
        {
            //Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("fr"); // testing MUI

            // this has to be in this method, before InitializeComponent() in order to do magic.
            // app - language
            if (Properties.Settings.Default.app_language != "default")
                Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(Properties.Settings.Default.app_language);

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

            // STARTUP SPLASH
            ShowSplashPage();
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

            // Debug overlay
            webcamPage_debugoverlay_windowsizeTextBlock.Text = string.Format("Window size: {0} ({1})", this.Width + "x" + this.Height, (this.Width - 2) + "x" + (this.Height - 2 - 32));

            // Webcam page menu
            if (this.Width <= 482)
            {
                webcamPage_menuGrid.Width = Double.NaN;
                webcamPage_menuGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
            }
            else
            {
                webcamPage_menuGrid.Width = 320;
                webcamPage_menuGrid.HorizontalAlignment = HorizontalAlignment.Left;
            }
        }

        private void titlebarGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            if (splashPage.Visibility == Visibility.Visible) // if we're on the splash
                return; // don't do anything

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
            if (current_page == 4) // if we're on zSettings
            {
                SwitchToPage(0);

                // anim
                DoubleAnimation opacityAnim = new DoubleAnimation(0, TimeSpan.FromSeconds(.3));

                zSettingsFrame.BeginAnimation(UIElement.OpacityProperty, opacityAnim);

                DispatcherTimer visibilityTimer = new DispatcherTimer();
                visibilityTimer.Interval = TimeSpan.FromSeconds(.3);
                visibilityTimer.Tick += (s, ev) => { visibilityTimer.Stop(); zSettingsFrame.Visibility = Visibility.Collapsed; zSettingsFrame_Rec.Visibility = Visibility.Collapsed; };
                visibilityTimer.Start();
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

        #region Splash page

        DispatcherTimer startSplash_timeoutTimer;

        bool splash_wasTMenuOn;
        bool splash_wasTBackOn;
        int splash_prevTitlebarStyle;

        public void ShowSplashPage(string progresstext = "")
        {
            // titlebar buttons
            if (webcamPage_menuButton.Visibility == Visibility.Visible)
                splash_wasTMenuOn = true;
            else
                splash_wasTMenuOn = false;

            if (backButton.Visibility == Visibility.Visible)
                splash_wasTBackOn = true;
            else
                splash_wasTBackOn = false;

            webcamPage_menuButton.Visibility = Visibility.Collapsed;
            backButton.Visibility = Visibility.Collapsed;

            // titlebar style
            splash_prevTitlebarStyle = titlebarStyle;

            SetTitlebarButtonsStyle(0);

            // -------------//
            // SHOW SPLASH  //
            // -------------//
            DoubleAnimation anim = new DoubleAnimation(1, TimeSpan.FromSeconds(0.5)); // fade animation

            splashPage.Opacity = 0;
            splashPage.Visibility = Visibility.Visible;

            splashPage.BeginAnimation(OpacityProperty, anim);

            if (progresstext != null)
            {
                splashPage_ProgressTextBlock.Visibility = Visibility.Visible;
                splashPage_ProgressTextBlock.Text = progresstext;
            }
            else
                splashPage_ProgressTextBlock.Visibility = Visibility.Collapsed;

            // Skip splash timeout
            if (startSplash_timeoutTimer == null)
            {
                startSplash_timeoutTimer = new DispatcherTimer();
                startSplash_timeoutTimer.Interval = TimeSpan.FromSeconds(10);
                startSplash_timeoutTimer.Tick += (s, ev) =>
                {
                    startSplash_timeoutTimer.Stop();

                    splash_SkipButton.Visibility = Visibility.Visible;
                };
            }
            startSplash_timeoutTimer.Start();

            // log
            Debug.Log("SPLASH: Splash showed.");
        }

        public void HideSplashPage()
        {
            if (splashPage.Visibility == Visibility.Visible)
            {
                // reset titlebar buttons
                if (splash_wasTMenuOn)
                    webcamPage_menuButton.Visibility = Visibility.Visible;
                if (splash_wasTBackOn)
                    backButton.Visibility = Visibility.Visible;

                // reset titlebar style
                SetTitlebarButtonsStyle(splash_prevTitlebarStyle);

                DoubleAnimation anim = new DoubleAnimation(0, TimeSpan.FromSeconds(0.5)); // fade animation

                DispatcherTimer timer = new DispatcherTimer(); // closes page
                timer.Interval = TimeSpan.FromSeconds(0.5);
                timer.Tick += (s, ev) => { timer.Stop(); splashPage.Visibility = Visibility.Collapsed; };

                splashPage.BeginAnimation(OpacityProperty, anim);
                timer.Start();

                startSplash_timeoutTimer.Stop();
                splash_SkipButton.Visibility = Visibility.Collapsed;

                Debug.Log("SPLASH: Splash hidden.");
            }
            else
                Debug.Log("SPLASH: Splash is already hidden.");
        }

        private void splash_SkipButton_Click(object sender, RoutedEventArgs e)
        {
            HideSplashPage();

            Debug.Log("SPLASH: Skipped splash.");
        }

        #endregion

        #region Webcam page

        // titlebar menu button //
        public void webcamPage_menuButton_Click(object sender, RoutedEventArgs e)
        {
            if (splashPage.Visibility == Visibility.Visible)
                return;

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
                DoubleAnimation menuGrid_movementAnimation = new DoubleAnimation(-webcamPage_menuGrid.ActualWidth, 0, new TimeSpan(0, 0, 0, 0, 450).Duration());

                DoubleAnimation menuGrid_blurbehindBorder_opacityAnimation = new DoubleAnimation(0, 1, new TimeSpan(0, 0, 0, 0, 0).Duration()); // 300

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

                DoubleAnimation menuGrid_blurbehind_radiusAnimation = new DoubleAnimation(7, new TimeSpan(0, 0, 0, 0, 400).Duration());

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
                DoubleAnimation menuGrid_movementAnimation = new DoubleAnimation(0, -webcamPage_menuGrid.ActualWidth, new TimeSpan(0, 0, 0, 0, 450).Duration());

                DoubleAnimation menuGrid_blurbehindBorder_opacityAnimation = new DoubleAnimation(1, 0, new TimeSpan(0, 0, 0, 0, 0).Duration()); // 300

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
                    UriToDownload = (new Uri(currentImageUri + camera_dummy()));
                }
                catch (Exception ex)
                {
                    TextMessageDialog_FullWidth("Cannot begin local save...", "An error occured while trying to save the image.\nError: " + ex.Message);
                    return;
                }

                whereToDownload = saveFileDialog.FileName;

                //webcamPage_CloseMenu();

                SaveImageFile();

                RAction = new RetryableAction() { action = (int)RActions.LocalSave, param = currentImageUri + camera_dummy() };
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
            if (Properties.Settings.Default.experiment_zOverview)
            {
                webcamPage_MainContentGrid_SwitchTozOverview();
            }
            else
            {
                if (Keyboard.IsKeyDown(Key.LeftShift))
                {
                    webcamPage_MainContentGrid_SwitchTozOverview();
                }
                else
                {
                    if (webcamPage_MainContentGrid_OverviewGrid.Visibility != Visibility.Visible & zOverviewGrid.Visibility != Visibility.Visible)
                        webcamPage_MainContentGrid_SwitchToOverview();
                    else
                        webcamPage_MainContentGrid_SwitchToCameraView();
                }
            }

            webcamPage_CloseMenu();
        }

        async void webcamPage_menuGrid_cameraButton_Click(object sender, RoutedEventArgs e)
        {
            webcamPage_menuGrid_CameraButton sBtn = sender as webcamPage_menuGrid_CameraButton;

            foreach (webcamPage_menuGrid_CameraButton button in webcamPage_menuGrid_cameraListStackPanel.Children)
            {
                if (button != sBtn)
                {
                    button.IsActive = false;
                }
                else
                {
                    button.IsActive = true;
                }
            }

            await LoadImage(Properties.Settings.Default.camera_urls[(int)sBtn.Tag]);

            webcamPage_CloseMenu();
        }

        int settingsPage_lastPageID = 0;

        private void webcamPage_SettingsShortcutButton_Click(object sender, RoutedEventArgs e)
        {
            SwitchToPage(1);

            settingsPage_leftGrid_TabButtonClick(settingsPage_leftGrid_TabButtonStackPanel.Children[settingsPage_lastPageID], new RoutedEventArgs());

            if (this.Width <= 482)
            {
                TextMessageDialog_FullWidth("Low resolution", "This version of Settings doesn't work well with low or mobile resolutions.\nWe advise using the newer zSettings on low resolutions.");
            }
        }

        #endregion

        #region Webcam engine

        Uri currentImageUri;
        //BitmapImage bimage;
        string camera_dummy()
        {
            return "?dummy=" + DateTime.Now.Ticks;
        }

        public async Task LoadImage(string Url, bool Autorefresh = false)
        {
            StopRefresh();

            RAction = new RetryableAction() { action = (int)RActions.ImageLoad, param = Url };

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

            BitmapImage image = null;

            using (WebClient client = new WebClient())
            {
                try
                {
                    var bytes = await client.DownloadDataTaskAsync(Url + camera_dummy());

                    image = new BitmapImage();
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

                    Debug.Log("WEBCAMENGINE: Couldn't load image: " + ex.Message);

                    /*
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
                    */

                    ShowSavePanel(1, ex.Message, true);
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

                    // debug overlay
                    webcamPage_debugoverlay_cameranameTextBlock.Text = string.Format("Camera name: {0}", Properties.Settings.Default.camera_names[Properties.Settings.Default.camera_urls.IndexOf(Url)]);
                    webcamPage_debugoverlay_cameraurlTextBlock.Text = string.Format("Camera URL: {0}", currentImageUri.ToString());
                    webcamPage_debugoverlay_imagesizeTextBlock.Text = string.Format("Image resolution: {0}", webcamPage_Image.Source.Width + "x" + webcamPage_Image.Source.Height);
                    webcamPage_debugoverlay_imagesizingmodeTextBlock.Text = string.Format("Image sizing mode: {0}", home_imagesizing);

                    string filesizeInKilobytes = "0KB";
                    if (image != null)
                    {
                        if (image.StreamSource.Length >= (1 << 10))
                            filesizeInKilobytes = string.Format("{0}KB", image.StreamSource.Length >> 10);
                    }
                    else
                        filesizeInKilobytes = "null";

                    webcamPage_debugoverlay_imagefilesizeTextBlock.Text = string.Format("Image file size: {0}", filesizeInKilobytes);
                    #endregion
                    // get rid of splashpage if visible
                    HideSplashPage();
                }

                // start refreshtimer
                StartRefresh();

                Debug.Log("WEBCAMENGINE: Image loaded.");
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
            //webcamPage_ShowProgressUI(4); webcamPage_menuGrid_SetProgressText(Properties.Resources.webcamPage_LocalSaveProgressPreparing);
            ShowSavePanel(); UpdateSavePanelStatus(Properties.Resources.webcamPage_LocalSaveProgressPreparing);
            //webcamPage_menuGrid_HideCamerasPanel(); webcamPage_menuGrid_HideOverviewButton();

            Thread thread = new Thread(() =>
            {
                WebClient client = new WebClient();
                client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
                client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);
                client.DownloadFileAsync(UriToDownload, whereToDownload);
            });
            thread.Start();

            Debug.Log("SAVEIMAGE: Saving image to local: " + whereToDownload);
        }

        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn / totalBytes * 100;
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                UpdateSavePanelStatus(Properties.Resources.webcamPage_LocalSaveProgress + e.ProgressPercentage + "%");

                webcamPage_saveGrid_progressBar.IsIndeterminate = false;
                webcamPage_saveGrid_progressBar.Maximum = e.TotalBytesToReceive;
                webcamPage_saveGrid_progressBar.Value = e.BytesReceived;
            }));
        }
        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(new Action(async () =>
            {
                UpdateSavePanelStatus(Properties.Resources.webcamPage_LocalSaveProgressFinished);
                //webcamPage_HideProgressUI(0);
                await HideSavePanel();
                StartRefresh();
                //webcamPage_menuGrid_ShowCamerasPanel(); webcamPage_menuGrid_ShowOverviewButton();
                //refreshtimer.Start();

                if (e.Error != null)
                {
                    //TextMessageDialog_FullWidth("Cannot download image", "An error occured while trying to download the image.\nError: " + e.Error.Message);
                    ShowSavePanel(1, e.Error.Message);
                    File.Delete(whereToDownload);

                    Debug.Log("SAVEIMAGE: Could not save image: " + e.Error.Message);
                }

                Debug.Log("SAVEIMAGE: Save finished.");
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

            RAction = new RetryableAction() { action = (int)RActions.ArchiveOrgSave };

            /*
            archivebrowser.Url = new Uri("http://web.archive.org/save/" + currentImageUri);
            //webcamPage_ShowProgressUI(3); webcamPage_progressBar.IsIndeterminate = true; webcamPage_progressLabel.Content = "connecting to archive.org...";
            webcamPage_ShowProgressUI(4); webcamPage_menuGrid_SetProgressText("Preparing to save on archive.org...");
            //webcamPage_menuGrid_HideCamerasPanel(); webcamPage_menuGrid_HideOverviewButton();
            */

            StopRefresh();

            //webcamPage_ShowProgressUI(4);
            ShowSavePanel(); webcamPage_saveGrid_progressBar.Visibility = Visibility.Hidden;
            UpdateSavePanelStatus(Properties.Resources.webcamPage_ArchiveOrgProgress); // Saving to archive.org...

            try
            {
                WebRequest request = WebRequest.Create(uri);

                Debug.Log("ARCHIVEORG: Initiated archive.org saving using WebRequest...");

                using (WebResponse _response = await request.GetResponseAsync())
                {
                    HttpWebResponse response = _response as HttpWebResponse;

                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        Debug.Log("ARCHIVEORG: Error: " + (int)response.StatusCode + " " + response.StatusCode.ToString());

                        await HideSavePanel();

                        /*
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
                        */
                        ShowSavePanel(1, "Status code: " + (int)response.StatusCode + " " + response.StatusCode.ToString() +
                            "Status code description: " + response.StatusDescription);
                    }
                    else
                    {
                        UpdateSavePanelStatus(Properties.Resources.webcamPage_ArchiveOrgProgressFinished); // Saved!
                        await HideSavePanel();
                    }
                }
            }
            catch (WebException ex)
            {
                Debug.Log("ARCHIVEORG: Couldn't connect to archive.org - " + ex.Message);

                /*
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
                */
                await HideSavePanel();
                ShowSavePanel(1, ex.Message);
            }

            //webcamPage_HideProgressUI(0);
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

        private void webcamPage_MainContentGrid_OverviewGrid_refreshButton_Click(object sender, RoutedEventArgs e)
        {
            webcamPage_MainContentGrid_Overview_LoadContent();
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

            // show overview debug
            webcamPage_menuGrid_overviewDebugButton.Visibility = Visibility.Visible;

            webcamPage_MainContentGrid_Overview_LoadContent();

            Debug.Log("WEBCAMPAGE: Switched to overview.");
        }

        public void webcamPage_MainContentGrid_SwitchToCameraView()
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

            // hide overview debug
            webcamPage_menuGrid_overviewDebugButton.Visibility = Visibility.Collapsed;

            DoubleAnimation anim_zOverview = new DoubleAnimation(0, TimeSpan.FromSeconds(.3));
            zOverviewGrid.BeginAnimation(OpacityProperty, anim_zOverview);

            DispatcherTimer anim_zOverview_Timer = new DispatcherTimer();
            anim_zOverview_Timer.Interval = TimeSpan.FromSeconds(.3);
            anim_zOverview_Timer.Tick += (s1, ev) => { anim_zOverview_Timer.Stop(); zOverviewGrid.Visibility = Visibility.Collapsed; };
            anim_zOverview_Timer.Start();

            Debug.Log("WEBCAMPAGE: Switched to camera view.");
        }

        Pages.Home_page.Overview.Page zOverview_page;

        public void webcamPage_MainContentGrid_SwitchTozOverview()
        {
            DoubleAnimation anim_zOverview = new DoubleAnimation(1, TimeSpan.FromSeconds(0.3));

            zOverviewGrid.Visibility = Visibility.Visible;
            zOverviewGrid.Opacity = 0;

            if (zOverviewFrame.Content == null)
            {
                zOverview_page = new Pages.Home_page.Overview.Page();
                zOverviewFrame.Navigate(zOverview_page);
            }

            zOverviewGrid.BeginAnimation(OpacityProperty, anim_zOverview);

            zOverview_page.RefreshOverview();

            // ----- ----- ----- //

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

            // show overview debug
            webcamPage_menuGrid_overviewDebugButton.Visibility = Visibility.Visible;

            Debug.Log("WEBCAMPAGE: Switched to overview.");
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

            Debug.Log("OVERVIEW: LoadContent() - number of cameras added: " + cameracounter);

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

                    byte[] bytes;
                    try
                    {
                        bytes = await client.DownloadDataTaskAsync(url);
                    }
                    catch (WebException ex)
                    {
                        button.IsError = true;

                        if (ex.Message != null)
                        {
                            button.ErrorMessage = ex.Message;
                        }

                        return;
                    }

                    var image = new BitmapImage();
                    try
                    {
                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.StreamSource = new MemoryStream(bytes);
                        image.EndInit();
                    }
                    catch (WebException ex)
                    {
                        button.IsError = true;

                        if (ex.Message != null)
                        {
                            button.ErrorMessage = ex.Message;
                        }

                        return;
                    }

                    button.Image = image;
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

                    foreach (webcamPage_menuGrid_CameraButton btn in webcamPage_menuGrid_cameraListStackPanel.Children)
                    {
                        if (btn.Text == button.CameraName)
                            btn.IsActive = true;
                        else
                            btn.IsActive = false;
                    }
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

        private void webcamPage_menuGrid_overviewDebugButton_Click(object sender, RoutedEventArgs e)
        {
            int numTotal = webcamPage_MainContentGrid_OverviewGrid_WrapPanel.Children.Count;
            int numLoaded = 0;
            int numError = 0;

            // numLoaded
            foreach (User_controls.webcamPage_Overview_CameraButton button in webcamPage_MainContentGrid_OverviewGrid_WrapPanel.Children)
            {
                if (button.image.IsLoaded == true)
                    numLoaded++;
            }

            // numError
            foreach (User_controls.webcamPage_Overview_CameraButton button in webcamPage_MainContentGrid_OverviewGrid_WrapPanel.Children)
            {
                if (button.IsError == true)
                    numError++;
            }

            TextMessageDialog(
            "",
            "Overview debug info\n\n" +
            "-------------------------\n" +
            "Total number of cameras: " + numTotal + "\n" +
            "Loaded cameras: " + numLoaded + "\n" +
            "Problematic cameras: " + numError + "\n" +
            "-------------------------\n" + "\n\n" +
            "Overview version: 1.0 Alpha" + "\n" +
            "Ripple effect: wv-2.0"
            );
        }

        #endregion

        #region Save panel

        private bool savePanel_wasMenuOpen;

        /// <summary>
        /// Shows the "Save panel".
        /// </summary>
        /// <param name="mode">0 = "Save image" panel, 1 = Error panel</param>
        /// <param name="exception">Error panel: the error to show</param>
        /// <param name="error_retryable">Error panel: whether to show the retry button</param>
        public void ShowSavePanel(int mode = 0, string error_exception = "", bool error_retryable = true)
        {
            // "create" save panel
            webcamPage_saveGrid.Visibility = Visibility.Visible;

            if (mode == 0) // save panel
            {
                webcamPage_saveGrid_SavePanel.Visibility = Visibility.Visible;
                webcamPage_saveGrid_ErrorPanel.Visibility = Visibility.Hidden;
            }
            else if (mode == 1) // error panel
            {
                webcamPage_saveGrid_SavePanel.Visibility = Visibility.Hidden;
                webcamPage_saveGrid_ErrorPanel.Visibility = Visibility.Visible;
            }

            // animate in
            DoubleAnimation anim_main = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.3)); // from 0 to 1, no need to do .Opacity above
            webcamPage_saveGrid.BeginAnimation(OpacityProperty, anim_main);

            if (mode == 0) // save panel
            {
                // remove status text
                UpdateSavePanelStatus("");

                // make progressbar indeterminate
                webcamPage_saveGrid_progressBar.Visibility = Visibility.Visible;
                webcamPage_saveGrid_progressBar.Value = 0;
                webcamPage_saveGrid_progressBar.IsIndeterminate = true;
            }
            else if (mode == 1) // error panel
            {
                webcamPage_saveGrid_ErrorPanel_errorTextBlock.Text = String.Format("Error: {0}", error_exception);
                if (error_retryable)
                    webcamPage_saveGrid_ErrorPanel_retryButton.Visibility = Visibility.Visible;
                else
                    webcamPage_saveGrid_ErrorPanel_retryButton.Visibility = Visibility.Collapsed;
            }

            // close menu
            if (webcamPage_menuGrid.IsVisible)
            { webcamPage_CloseMenu(); savePanel_wasMenuOpen = true; }
        }

        public async Task HideSavePanel()
        {
            // animate out
            DoubleAnimation anim_main = new DoubleAnimation(0, TimeSpan.FromSeconds(0.3));
            webcamPage_saveGrid.BeginAnimation(OpacityProperty, anim_main);

            // "destroy" save panel
            DispatcherTimer timer_main = new DispatcherTimer();
            timer_main.Interval = anim_main.Duration.TimeSpan;
            timer_main.Tick += (s, ev) =>
            {
                timer_main.Stop();
                webcamPage_saveGrid.Visibility = Visibility.Hidden; // make Hidden instead of Collapsed so that the indeterminate progressbar animation doesn't glitch out
            };
            timer_main.Start();

            // show menu
            if (savePanel_wasMenuOpen)
                if (webcamPage_menuGrid.Visibility == Visibility.Visible)
                {
                    #region Wait for the menu to close then open it again
                    // When saving, if one's internet connection is too fast and the saving finishes before the menu closing animation would finish, the menu
                    // is not going to open back up. This checks every 60 milliseconds if the menu is closed, and then it opens it back up again.
                    DispatcherTimer timer = new DispatcherTimer();
                    timer.Interval = TimeSpan.FromMilliseconds(60);
                    timer.Tick += (s, ev) =>
                    {
                        if (webcamPage_menuGrid.IsVisible != true)
                        {
                            timer.Stop();
                            webcamPage_OpenMenu();
                        }
                    };
                    timer.Start();
                    #endregion
                }
                else
                    webcamPage_OpenMenu();

            await Task.Delay(anim_main.Duration.TimeSpan.Milliseconds + 60);
        }

        public void UpdateSavePanelStatus(string newtext = "")
        {
            webcamPage_saveGrid_statusLabel.Content = newtext;
        }

        #region Button events

        private async void webcamPage_saveGrid_ErrorPanel_retryButton_Click(object sender, RoutedEventArgs e)
        {
            await HideSavePanel();
            Error_TryAgain();
        }

        private async void webcamPage_saveGrid_ErrorPanel_okButton_Click(object sender, RoutedEventArgs e)
        {
            await HideSavePanel();
        }

        #endregion

        #endregion

        #region Error handling

        RetryableAction RAction;

        public enum RActions
        {
            LocalSave,
            ArchiveOrgSave,
            ImageLoad
        };

        class RetryableAction
        {
            public int action;
            public string param;
        }

        async void Error_TryAgain()
        {
            if (RAction.action == (int)RActions.ArchiveOrgSave) // archive.org save
            {
                await SaveOnArchiveOrg();
            }
            if (RAction.action == (int)RActions.LocalSave) // local save
            {
                SaveImageFile();
            }
            if (RAction.action == (int)RActions.ImageLoad) // image load
            {
                await LoadImage(RAction.param);
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

        private void customizationdeliveryPage_CloseDialogButton_Click(object sender, RoutedEventArgs e)
        {
            SwitchToPage(1);
        }

        #endregion

        public int current_page;

        /// <summary>
        /// Transition to a page with it's respective animation.
        /// </summary>
        /// <param name="page">0 = Home, 1 = Settings, 2 = Customizations delivery settings</param>
        /// <param name="noanim">Play no animation.</param>
        public void SwitchToPage(int page, bool noanim = false)
        {
            switch (page)
            {
                default:
                    {
                        TextMessageDialog("Cannot switch to this page.", "The page \"" + page.ToString() + "\" doesn't exist.");
                        break;
                    }
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

                        // background
                        DispatcherTimer timer1 = new DispatcherTimer();
                        timer1.Interval = TimeSpan.FromSeconds(1.5);
                        timer1.Tick += (s1, ev) =>
                        {
                            timer1.Stop();

                            webcamPage_menuGrid.Visibility = Visibility.Collapsed;
                            backButton.Visibility = Visibility.Collapsed;
                            webcamPage_menuButton.Visibility = Visibility.Collapsed;

                            webcamPage.Visibility = Visibility.Visible;
                            webcamPage.Opacity = 0;

                            DoubleAnimation anim = new DoubleAnimation(1, TimeSpan.FromSeconds(.5));
                            webcamPage.BeginAnimation(OpacityProperty, anim);
                        };
                        timer1.Start();

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

                        SetTitlebarButtonsStyle(0);

                        if (internalsettingsPage_Container_frame.Content == null) // we want to return to an existing instance once we were there already
                            internalsettingsPage_Container_frame.Navigate(new Pages.Internal_development_page.Page());

                        // change title
                        titleLabel.Content = Properties.Resources.App_Title + " - Internal settings";

                        StopRefresh();

                        break;
                    }
                case 4: // zSettings
                    {
                        current_page = 4;

                        // temporary
                        zSettingsFrame.Visibility = Visibility.Visible; zSettingsFrame.Opacity = 0; zSettingsFrame_Rec.Visibility = Visibility.Visible;
                        if (zSettingsFrame.Content == null)
                        {
                            Pages.Settings_page.Page _page = new Pages.Settings_page.Page();
                            zSettingsFrame.Navigate(_page);
                        }

                        DoubleAnimation opacityAnim = new DoubleAnimation(1, TimeSpan.FromSeconds(.3));
                        zSettingsFrame.BeginAnimation(UIElement.OpacityProperty, opacityAnim);

                        webcamPage.Visibility = Visibility.Collapsed;

                        SetTitlebarButtonsStyle(2);

                        break;
                    }
            }

            Debug.Log("CORE_PAGEUI: Switched to page " + page);
        }

        public int titlebarStyle;

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

                        titlebarStyle = 0;

                        break;
                    }
                case 1:
                    {
                        styleString = "light";
                        s_toLight.Begin();

                        titlebarStyle = 1;

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

                        titlebarStyle = 2;

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

            //Debug.Log("CORE_TITLEBAR: Set style to " + styleString + "(" + style + ")");
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
        bool home_webcamimagebackgroundmode;
        string home_imagesizing;

        bool settings_showtitlebarcolor;
        bool settings_experiment_UpdateUI;

        bool app_firstrun;
        string app_language;

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

                if (currentImageUri != null) // make current camera button active when repopulating
                {
                    if (button.Text == Properties.Settings.Default.camera_names[Properties.Settings.Default.camera_urls.IndexOf(currentImageUri.ToString())])
                        button.IsActive = true;
                }

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
            home_webcamimagebackgroundmode = Properties.Settings.Default.home_webcamimageBackgroundMode;
            home_imagesizing = Properties.Settings.Default.home_imagesizing;

            settings_showtitlebarcolor = Properties.Settings.Default.settings_showtitlebarcolor;
            settings_experiment_UpdateUI = Properties.Settings.Default.settings_experiment_UpdateUI;

            app_firstrun = Properties.Settings.Default.app_firstrun;
            app_language = Properties.Settings.Default.app_language;

            debugmode = Properties.Settings.Default.app_debugmode;

            if (applysettings)
            {
                ApplyUserConfiguration();
            }
        }

        /// <summary>
        /// Applies the user's settings from the variables, NOT Properties.Settings.Default. That means, you first have to GetUserConfiguration and then you can ApplyUserConfiguration.
        /// </summary>
        public void ApplyUserConfiguration()
        {
            // ui - theme
            Theming.Theme.SetTheme(ui_theme);
            webcamPage_debugoverlay_appthemeTextBlock.Text = string.Format("UI theme: {0}", Theming.Theme.ThemeNames[Properties.Settings.Default.ui_theme]);

            // ui - accent
            Theming.AccentColor.SetAccentColor(ui_accent);
            webcamPage_debugoverlay_appaccentcolorTextBlock.Text = string.Format("UI accent color: {0}", Theming.AccentColor.AccentColorNames[Properties.Settings.Default.ui_accent]);

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
            {
                webcamPage_menuGrid_blurbehindBorder.Visibility = Visibility.Visible;
                zOverviewBlurEffect.Radius = 10;
            }
            else
            {
                webcamPage_menuGrid_blurbehindBorder.Visibility = Visibility.Collapsed;
                zOverviewBlurEffect.Radius = 0;
            }

            // home - debug overlay
            if (home_debugoverlay)
                webcamPage_debugoverlayGrid.Visibility = Visibility.Visible;
            else
                webcamPage_debugoverlayGrid.Visibility = Visibility.Collapsed;

            // home - webcam image background mode
            if (home_webcamimagebackgroundmode == false)
            {
                if (!Properties.Settings.Default.home_webcamimageBackgroundMode_BlackOverride)
                    webcamPage_mainBackgroundRectangle.Fill = Application.Current.Resources["settingsPage_background"] as SolidColorBrush;
                else
                    webcamPage_mainBackgroundRectangle.Fill = Brushes.Black;
            }
            else
                webcamPage_mainBackgroundRectangle.Fill = Application.Current.Resources["accentcolor_dark"] as SolidColorBrush;

            // home - image sizing mode
            switch (home_imagesizing)
            {
                case "none":
                    {
                        webcamPage_Image.Stretch = Stretch.None;

                        break;
                    }
                case "uniform":
                    {
                        webcamPage_Image.Stretch = Stretch.Uniform;

                        break;
                    }
                case "uniformtofill":
                    {
                        webcamPage_Image.Stretch = Stretch.UniformToFill;

                        break;
                    }
                case "fill":
                    {
                        webcamPage_Image.Stretch = Stretch.Fill;

                        break;
                    }
            }

            // settings - show titlebar color
            if (settings_showtitlebarcolor)
            {
                settingsPage_rectangle.Fill = Application.Current.Resources["accentcolor_dark"] as SolidColorBrush;
                settingsPage_rectangle.Visibility = Visibility.Visible;
                internalsettingsPage_Rectangle.Fill = Application.Current.Resources["accentcolor_dark"] as SolidColorBrush;
                webcamPage_Rectangle.Fill = Application.Current.Resources["accentcolor_dark"] as SolidColorBrush;
                SetTitlebarButtonsStyle(0);
            }
            else
            {
                settingsPage_rectangle.Fill = Application.Current.Resources["settingsPage_background"] as SolidColorBrush;
                settingsPage_rectangle.Visibility = Visibility.Hidden;
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
                settingsPage_leftGrid_RippleDrawableTestTabButton.Visibility = Visibility.Collapsed;
            }
            else if (debugmode == "testing")
            {

            }
            settingsPage_leftGrid_DebugSettingsTabButton.Description = "Debug mode: " + debugmode;
            webcamPage_debugoverlay_debugmodeTextBlock.Text = string.Format("Debug mode: {0}", Properties.Settings.Default.app_debugmode);
            webcamPage_debugoverlay_prereleasechannelTextBlock.Text = string.Format("Prerelease channel: {0}", Properties.Settings.Default.app_prereleasechannel);
        }

        #endregion

        private void webcamPage_zSettingsShortcutButton_Click(object sender, RoutedEventArgs e)
        {
            SwitchToPage(4);
            webcamPage_CloseMenu();
        }
    }
}