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
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("hu-HU");
            InitializeComponent();

            archivebrowser.ScriptErrorsSuppressed = true;
            archivebrowser.ProgressChanged += archivebrowser_ProgressChanged;
            archivebrowser.DocumentCompleted += Archivebrowser_DocumentCompleted;

            webcamPage_menuGrid_progressInStoryboard = (Storyboard)FindResource("webcamPage_menuGrid_progressIn");
            webcamPage_menuGrid_progressOutStoryboard = (Storyboard)FindResource("webcamPage_menuGrid_progressOut");
        }

        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            webcamPage.Visibility = Visibility.Visible; webcamPage_menuButton.Visibility = Visibility.Visible;

            GetUserConfiguration();
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
            if (WindowState == WindowState.Maximized)
                maximizeButton.Content = "\ue923";
            else
                maximizeButton.Content = "\ue922";
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
                    webcamPage_CloseMenu();
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
        }

        #endregion

        #region Dialogs

        void TextMessageDialog(string Title, string Content, bool? DarkMode = true)
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

                backButton.Visibility = Visibility.Visible;
                webcamPage_menuButton.Visibility = Visibility.Collapsed;
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

                backButton.Visibility = Visibility.Collapsed;
                webcamPage_menuButton.Visibility = Visibility.Visible;
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
                    TextMessageDialog("Cannot begin local save...", "An error occured while trying to save the image.\nError: " + ex.Message);
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
        }

        private void webcampage_menuGrid_archiveorgSaveButton_Click(object sender, RoutedEventArgs e)
        {
            archivebrowser.Url = new Uri("http://web.archive.org/save/" + currentImageUri);
            //webcamPage_ShowProgressUI(3); webcamPage_progressBar.IsIndeterminate = true; webcamPage_progressLabel.Content = "connecting to archive.org...";
            webcamPage_ShowProgressUI(4); webcamPage_menuGrid_SetProgressText("Preparing to save on archive.org...");

            //webcamPage_CloseMenu();
        }

        void webcamPage_menuGrid_cameraButton_Click(object sender, RoutedEventArgs e)
        {
            webcamPage_menuGrid_CameraButton sBtn = sender as webcamPage_menuGrid_CameraButton;

            //webcamPage_CloseMenu();
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
                webcamPage_ShowProgressUI(4);

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

                webcamPage_CloseMenu();
            };
            bimage.DownloadFailed += (s, ev) =>
            {
                TextMessageDialog("Could not load image", "An error occured while trying to load the webcam image...\n" + ev.ErrorException.Message);
                webcamPage_Image.Visibility = Visibility.Hidden;
                webcamPage_HideProgressUI();
            };

            currentImageUri = new Uri(Properties.Settings.Default.camera_urls[Properties.Settings.Default.camera_urls.IndexOf(Url)]);
            webcamPage_menuGrid_cameraNameLabel.Text = Properties.Settings.Default.camera_names[(Properties.Settings.Default.camera_urls.IndexOf(Url))].ToUpper();
            webcamPage_menuGrid_cameraUrlLabel.Text = currentImageUri.ToString();

            webcamPage_menuGrid_cameraNameLabel.ToolTip = Properties.Settings.Default.camera_names[(Properties.Settings.Default.camera_urls.IndexOf(Url))];
            webcamPage_menuGrid_cameraUrlLabel.ToolTip = currentImageUri;
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
            webcamPage_ShowProgressUI(4); webcamPage_menuGrid_SetProgressText("Preparing to download image...");

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
                webcamPage_HideProgressUI();
                //refreshtimer.Start();

                if (e.Error != null)
                {
                    TextMessageDialog("Cannot download image", "An error occured while trying to download the image.\nError: " + e.Error.Message);
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
        }

        // ----- ----- //

        #endregion

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
        #endregion

        #region User interface page

        #endregion

        #region Debug menu page

        private void settingsPage_DebugMenuPage_Home_ProgressDebugButton_Click(object sender, RoutedEventArgs e)
        {
            // Create progress UI debug dialog
            Popups.MessageDialog dlg = new Popups.MessageDialog();
            dlg.Title = "";

            dlg.IsDarkTheme = true;

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
                TextMessageDialog("Invalid button tag", "You shouldn't see this error.\n\nGot tag: " + sBtn.Tag.ToString());
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
        }

        /// <summary>
        /// This function (re)loads the appropiate settings for a page once it opens or when a change was made by a function inside the page.
        /// </summary>
        void settingsPage_DoPageEvents(int page)
        {
            switch (page)
            {
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
                case 1: // User interface
                    {
                        // Set the toggles
                        foreach (object item in settingsPage_UserInterfacePage_MainStackPanel.Children)
                        {
                            if (item.GetType() == (typeof(ToggleSwitchButton)))
                            {
                                settingsPage_ToggleSwitchButton btn = item as settingsPage_ToggleSwitchButton;
                                if ((string)btn.Tag != "") // if it has a tag
                                {
                                    btn.IsActive = (bool)Properties.Settings.Default[(string)btn.Tag];
                                }
                            }
                        }

                        break;
                    }
                case 2:
                    {


                        break;
                    }
                case 3:
                    {


                        break;
                    }
                case 4:
                    {


                        break;
                    }
                case 5:
                    {


                        break;
                    }
            }
        }

        /// <summary>
        /// Toggles the setting, which is set in the sender button's Tag property, in Properties.Settings.Default.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void settingsPage_ToggleButtonClick(object sender, RoutedEventArgs e)
        {
            settingsPage_ToggleSwitchButton sBtn = sender as settingsPage_ToggleSwitchButton;

            try
            {
                Properties.Settings.Default[(string)sBtn.Tag] = sBtn.IsActive;
                Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                sBtn.IsActive = !sBtn.IsActive;
                TextMessageDialog("Invalid property", "The setting " + sBtn.Tag.ToString() + " (probably) doesn't exist.\nCheck the button's Tag.\n\nExact error message : " + ex.Message);
            }
        }

        #endregion

        // -----

        int current_page;

        /// <summary>
        /// Transition to a page with it's respective animation.
        /// </summary>
        /// <param name="page">0 = Home, 1 = Settings</param>
        void SwitchToPage(int page)
        {
            switch (page)
            {
                case 0: // Webcam page
                    {
                        current_page = 0;

                        Storyboard s = (Storyboard)FindResource("webcamPage_In");
                        s.Begin();

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

                        SetTitlebarButtonsStyle(0);

                        break;
                    }
                case 1: // Settings page
                    {
                        current_page = 1;

                        Storyboard s = (Storyboard)FindResource("settingsPage_In");
                        s.Begin();

                        webcamPage_menuButton.Visibility = Visibility.Collapsed;
                        backButton.Visibility = Visibility.Visible;

                        //SetTitlebarButtonsStyle(1);

                        break;
                    }
            }
        }

        /// <summary>
        /// Sets the style of the titlebar buttons to a theme. Use when the theme changes.
        /// </summary>
        /// <param name="style">The theme to change the buttons to. 1 = Dark, 2 = Light</param>
        void SetTitlebarButtonsStyle(int style)
        {
            // 0: Dark
            // 1: Light

            Style darkStyle = Application.Current.Resources["titlebarButton"] as Style;
            Style lightStyle = Application.Current.Resources["titlebarButton_Light"] as Style;

            Style styleToApply = null;

            switch (style)
            {
                case 0:
                    {
                        styleToApply = darkStyle;
                        break;
                    }
                case 1:
                    {
                        styleToApply = lightStyle;
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

        #region Configuration

        void GetUserConfiguration()
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
        }

        public class settingsPage_WebcamEditorPage_Camera
        {
            public string Name { get; set; }

            public string Url { get; set; }

            public string SaveLocation { get; set; }

            public int RefreshRate { get; set; }
        }

        #endregion
    }
}