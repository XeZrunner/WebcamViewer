using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
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
using System.Windows.Threading;
using WebcamViewer.Pages.Home_page.Controls;
using WebcamViewer.User_controls;

namespace WebcamViewer.Pages.Home_page
{
    /// <summary>
    /// Page title: Home; Page ID: 0
    /// </summary>
    public partial class Page
    {
        public Page()
        {
            InitializeComponent();

            hamburger_menu.Visibility = Visibility.Hidden;

            #region Determine debug

#if DEBUG // always do debug when I'm in Debug
            m_isDebugEnabled = true;
#endif

#if RELEASE
            m_isDebugEnabled = false;
            m_showProgressDebugInfo = false;
#endif

            #endregion
        }

        MainWindow mainwindow = Application.Current.MainWindow as MainWindow;

        Debug Debug = new Debug();

        // Load user configuration and the first (or arg) camera
        // fx: Loading the first camera should be it's own method.
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!thispageOpenedAtLeastOnce)
            {
                mainwindow.webcamPage_menuButton.Click += async (s, ev) =>
                {
                    mainwindow.webcamPage_menuButton.Visibility = Visibility.Collapsed;
                    mainwindow.backButton.Visibility = Visibility.Visible;
                    await ShowMenu();
                };

                mainwindow.backButton.Click += async (s1, ev1) =>
                {
                    mainwindow.webcamPage_menuButton.Visibility = Visibility.Visible;
                    mainwindow.backButton.Visibility = Visibility.Collapsed;
                    await HideMenu();
                };

                thispageOpenedAtLeastOnce = true;
            }

            LoadUserCameras();
            await LoadCamera(null, _Cameras[0]);
        }

        // Look for size changes so the page can switch beetween Mobile and Desktop views
        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.ActualWidth <= 545 & m_currentViewType != ViewType.Mobile)
                UpdateView(ViewType.Mobile);
            if (this.ActualWidth >= 545 & m_currentViewType != ViewType.Desktop)
                UpdateView(ViewType.Desktop);
        }

        private bool thispageOpenedAtLeastOnce = false;

        private ViewType m_currentViewType;
        private bool m_isDebugEnabled = true;
        private bool m_showProgressDebugInfo = false;

        #region Enums

        public enum ViewType
        {
            Desktop,
            Mobile
        }

        #endregion

        #region Dialogs

        public void TextContentDialog(string Title = "", string Text = "", string buttontext = "OK", ContentDialogControl._Theme Theme = ContentDialogControl._Theme.Auto)
        {
            Popups.ContentDialog dialog = new Popups.ContentDialog()
            {
                Title = Title,
                Text = Text,
                Button0_Text = buttontext,
                Theme = Theme
            };

            dialog.ShowDialog();
        }

        #endregion

        #region UI

        #region Page View

        public void UpdateView(ViewType view)
        {
            if (view == ViewType.Mobile)
            {

            }
            if (view == ViewType.Desktop)
            {

            }
        }

        #endregion

        #region Hamburger menu UI

        // the menu should not be open at start
        private bool m_isMenuOpen = false;

        /// <summary>
        /// Get the status of the menu.
        /// </summary>
        public bool isMenuOpen { get { return m_isMenuOpen; } }

        public async Task ShowMenu()
        {
            DoubleAnimation anim_opacity = new DoubleAnimation(1, TimeSpan.FromSeconds(.3));
            DoubleAnimation anim_translate = new DoubleAnimation(-hamburger_menu.Width, 0, TimeSpan.FromSeconds(.3));

            anim_opacity.SpeedRatio = Properties.Settings.Default.ui_animationspeed;
            anim_translate.SpeedRatio = Properties.Settings.Default.ui_animationspeed;

            hamburger_menu.Visibility = Visibility.Visible;
            hamburger_menu.BeginAnimation(OpacityProperty, anim_opacity);

            TranslateTransform transform = new TranslateTransform();
            transform.BeginAnimation(TranslateTransform.XProperty, anim_translate);

            hamburger_menu.RenderTransform = transform;

            // asynchrounous
            double secondsToWaitForAsync = anim_translate.Duration.TimeSpan.Milliseconds / Properties.Settings.Default.ui_animationspeed;
            await Task.Delay(TimeSpan.FromMilliseconds(secondsToWaitForAsync));

            m_isMenuOpen = true;
        }

        public async Task HideMenu()
        {
            DoubleAnimation anim_opacity = new DoubleAnimation(0, TimeSpan.FromSeconds(.3));
            DoubleAnimation anim_translate = new DoubleAnimation(-hamburger_menu.Width, TimeSpan.FromSeconds(.3));
            anim_translate.Completed += (s, ev) => { hamburger_menu.Visibility = Visibility.Hidden; };

            anim_opacity.SpeedRatio = Properties.Settings.Default.ui_animationspeed;
            anim_translate.SpeedRatio = Properties.Settings.Default.ui_animationspeed;

            hamburger_menu.Visibility = Visibility.Visible;
            hamburger_menu.BeginAnimation(OpacityProperty, anim_opacity);

            TranslateTransform transform = new TranslateTransform();
            transform.BeginAnimation(TranslateTransform.XProperty, anim_translate);

            hamburger_menu.RenderTransform = transform;

            // asynchrounous
            double secondsToWaitForAsync = anim_translate.Duration.TimeSpan.Milliseconds / Properties.Settings.Default.ui_animationspeed;
            await Task.Delay(TimeSpan.FromMilliseconds(secondsToWaitForAsync));

            m_isMenuOpen = false;
        }

        /// <summary>
        /// Adds a Webcam to the camera list.
        /// </summary>
        /// <param name="webcam">The Webcam to add to the list.</param>
        public void AddCameraToMenu(Webcam webcam)
        {
            User_controls.webcamPage_menuGrid_CameraButton button = new User_controls.webcamPage_menuGrid_CameraButton()
            {
                Text = webcam.Name,
                Tag = webcam.Url
            };

            button.Click += CameraButton_ClickEvent;

            menu_cameralist.Children.Add(button);
        }

        private async void CameraButton_ClickEvent(object sender, RoutedEventArgs e)
        {
            User_controls.webcamPage_menuGrid_CameraButton sButton = sender as User_controls.webcamPage_menuGrid_CameraButton;
            await LoadCamera((string)sButton.Tag);

            await HideMenu();
        }

        #region Buttons 

        // image saving
        private void menu_cameraactions_localsaveButton_Click(object sender, RoutedEventArgs e)
        {
            //SaveImage_Local(currentCameraUrl);
        }

        private async void menu_cameraactions_archivesaveButton_Click(object sender, RoutedEventArgs e)
        {
            //await SaveImage_ArchiveOrg(currentCameraUrl);
        }

        private async void menu_cameraactions_bothsaveButton_Click(object sender, RoutedEventArgs e)
        {
            //await SaveImage_BothSave(currentCameraUrl);
        }

        // menu bottom navigation
        private void menu_debugButton_Click(object sender, RoutedEventArgs e)
        {
            ShowDebugMenu();
        }

        private void menu_settingsButton_Click(object sender, RoutedEventArgs e)
        {
            mainwindow.SwitchToPage(5);
        }

        #endregion

        #endregion

        #endregion

        #region Progress UI

        #region Enums

        public enum ProgressUI_Type
        {
            Generic,
            ImageSave,
            ImageSave_BothSave
        }

        #endregion

        private double _animationspeed = Properties.Settings.Default.ui_animationspeed;

        private bool _IsProgressUIVisible = false;

        /// <summary>
        /// Shows the Progress UI. Make sure you use UpdateProgressUI() to show the appropriate
        /// UI. This just shows the progress UI container.
        /// </summary>
        public void ShowProgressUI()
        {
            // set Visibility to Visible
            progressUI.Visibility = Visibility.Visible;

            // animate! opacity
            DoubleAnimation anim_opacity = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(.3 * _animationspeed));
            progressUI.BeginAnimation(OpacityProperty, anim_opacity);

            _IsProgressUIVisible = true;
        }

        /// <summary>
        /// Updates the Progress UI type.
        /// </summary>
        /// <param name="UIType">The type of UI to update to.</param>
        public void UpdateProgressUI(ProgressUI_Type UIType)
        {
            // Get animation ready
            DoubleAnimation anim_in = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(.3 * _animationspeed));

            switch (UIType)
            {
                case ProgressUI_Type.Generic:
                    {
                        // set UI visible
                        progressUI_Generic.Visibility = Visibility.Visible;

                        // animate!
                        progressUI_Generic.BeginAnimation(OpacityProperty, anim_in);

                        break;
                    }
                case ProgressUI_Type.ImageSave:
                    {
                        // set UI visible
                        progressUI_ImageSave.Visibility = Visibility.Visible;

                        // animate!
                        progressUI_ImageSave.BeginAnimation(OpacityProperty, anim_in);

                        break;
                    }
                case ProgressUI_Type.ImageSave_BothSave:
                    {
                        // set UI visible
                        progressUI_ImageSave.Visibility = Visibility.Visible;
                        progressUI_ImageSave_BothSaveUI.Visibility = Visibility.Visible;

                        // animate!
                        progressUI_ImageSave.BeginAnimation(OpacityProperty, anim_in);
                        progressUI_ImageSave_BothSaveUI.BeginAnimation(OpacityProperty, anim_in);

                        break;
                    }
            }

            // Hide all other types
            switch (UIType)
            {
                case ProgressUI_Type.Generic:
                    {
                        // remove ImageSave

                        DoubleAnimation anim_out = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(.3 * _animationspeed));
                        anim_out.Completed += (s, ev) =>
                        {
                            progressUI_ImageSave.Visibility = Visibility.Collapsed;
                        };

                        progressUI_ImageSave.BeginAnimation(OpacityProperty, anim_out);

                        break;
                    }
                case ProgressUI_Type.ImageSave:
                    {
                        // remove Generic and ImageSave_BothSave
                        DoubleAnimation anim_out = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(.3 * _animationspeed));
                        anim_out.Completed += (s, ev) =>
                        {
                            progressUI_Generic.Visibility = Visibility.Collapsed;
                            progressUI_ImageSave_BothSaveUI.Visibility = Visibility.Collapsed;
                        };

                        progressUI_Generic.BeginAnimation(OpacityProperty, anim_out);
                        progressUI_ImageSave_BothSaveUI.BeginAnimation(OpacityProperty, anim_out);

                        break;
                    }
                case ProgressUI_Type.ImageSave_BothSave:
                    {
                        // remove Generic

                        DoubleAnimation anim_out = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(.3 * _animationspeed));
                        anim_out.Completed += (s, ev) =>
                        {
                            progressUI_Generic.Visibility = Visibility.Collapsed;
                        };

                        progressUI_Generic.BeginAnimation(OpacityProperty, anim_out);

                        break;
                    }
            }
        }

        /// <summary>
        /// Hides the Progress UI.
        /// </summary>
        public void HideProgressUI()
        {
            // animate! opacity
            DoubleAnimation anim_opacity = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(.3 * _animationspeed));

            // set Completed event to hide the UI once animation completes
            anim_opacity.Completed += (s, ev) =>
            {
                progressUI.Visibility = Visibility.Hidden;
            };

            progressUI.BeginAnimation(OpacityProperty, anim_opacity);

            _IsProgressUIVisible = false;
        }

        #region Buttons

        private void progressUI_ImageSave_BothSaveUI_OKButton_Click(object sender, RoutedEventArgs e)
        {
            HideProgressUI();
        }

        private void progressUI_ImageSave_BothSaveUI_CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // do some black magic to cancel the save attempts...
        }

        #endregion

        #endregion

        #region Engine

        /// <summary>
        /// An array containing the loaded user cameras.
        /// </summary>
        public Webcam[] _Cameras;

        #region Public stuff

        /// <summary>
        /// Get the current loaded camera Name.
        /// </summary>
        public string currentCameraName { get { return m_currentCameraName; } }

        /// <summary>
        /// Get the current loaded camera URL.
        /// </summary>
        public string currentCameraUrl { get { return m_currentCameraUrl; } }

        /// <summary>
        /// Get the current loaded camera owner name.
        /// </summary>
        public string currentCameraOwner { get { return m_currentCameraOwner; } }

        /// <summary>
        /// Get the current loaded camera ID.
        /// </summary>
        public int currentCameraID { get { return m_currentCameraID; } }

        #endregion

        private string m_currentCameraName = "";
        private string m_currentCameraUrl = "";
        private string m_currentCameraOwner = "";
        private int m_currentCameraID = 0;

        /// <summary>
        /// The dummy that's put after the URL of a camera to make sure we load the latest image and not some cached
        /// version or something.
        /// </summary>
        string camera_dummy()
        {
            return "?dummy=" + DateTime.Now.Ticks;
        }

        /// <summary>
        /// Loads the user cameras into the camera array and into the menu.
        /// </summary>
        public void LoadUserCameras()
        {
            Debug.Log("LoadUserCameras() started.");

            // init array
            _Cameras = new Webcam[Properties.Settings.Default.camera_names.Count];

            // add cameras to the array
            int counter = 0;
            foreach (string name in Properties.Settings.Default.camera_names)
            {
                Webcam camera = new Webcam // to be filled out later with more stuff
                {
                    Name = name,
                    Url = Properties.Settings.Default.camera_urls[counter],
                    SaveLocation = null,
                    RefreshRate = Properties.Settings.Default.camera_refreshrates[counter]
                };

                _Cameras[counter] = camera;

                counter++;
            }

            // add cameras to the menu

            menu_cameralist.Children.Clear(); // clear out existing children

            foreach (Webcam camera in _Cameras)
            {
                AddCameraToMenu(camera);
            }

            Debug.Log(String.Format("Successfully added {0} cameras to the camera array and menu.", counter));
        }

        public async Task LoadCamera(string url = null, Webcam webcam = null)
        {
            #region Set URL
            if (url != "" & url != null & webcam == null) // we have an URL
                m_currentCameraUrl = url;
            else if (webcam != null & url == "" || url == null) // we have a Webcam
                m_currentCameraUrl = webcam.Url;
            #endregion

            await webcamimage.LoadCamera(m_currentCameraUrl + camera_dummy());

            #region Set name
            m_currentCameraName = Properties.Settings.Default.camera_names[Properties.Settings.Default.camera_urls.IndexOf(m_currentCameraUrl)];
            #endregion

            #region Set ID
            int counter = 0;
            foreach (string camera in Properties.Settings.Default.camera_urls)
            {
                if (camera == m_currentCameraUrl)
                    m_currentCameraID = counter;

                counter++;
            }
            #endregion

            #region Set owner
            m_currentCameraOwner = Properties.Settings.Default.camera_owners[Properties.Settings.Default.camera_urls.IndexOf(m_currentCameraUrl)];
            #endregion

            #region Set camera button state active
            foreach (User_controls.webcamPage_menuGrid_CameraButton button in menu_cameralist.Children)
            {
                if ((string)button.Tag != m_currentCameraUrl)
                    button.IsActive = false;
                else
                    button.IsActive = true;
            }
            #endregion

            #region Update info panel stuff

            menu_camerainfo_Name.Content = currentCameraName;
            menu_camerainfo_Url.Content = currentCameraUrl;
            menu_camerainfo_OwnerName.Content = currentCameraOwner;

            #endregion

            // UpdateDebugOverlayCameraInfo();
        }

        #region Image saving

        private bool m_IsBothSave = false;

        #region Local save

        string localsave_destination = "";



        #endregion

        #region Archive.org save



        #endregion

        #region Both save


        #endregion

        #endregion

        #endregion

        #region Debug stuff

        public void ShowDebugMenu()
        {
            Popups.ContentDialog dialog = new Popups.ContentDialog();
            dialog.Title = "Home debug";
            dialog.Button0_Text = "Close";

            #region Create content
            StackPanel panel = new StackPanel();

            Label lbl0_usercameraCount = new Label() { Content = String.Format("Number of user cameras: {0}", menu_cameralist.Children.Count) };
            Label lbl1_seperator0 = new Label() { Content = "------------------------------" };
            TextBlock lbl2_currentcameraName = new TextBlock() { Text = String.Format("Current camera Name: {0}", currentCameraName), Padding = new Thickness(5), Foreground = Application.Current.Resources["settingsPage_foregroundText"] as SolidColorBrush };
            TextBlock lbl3_currentcameraUrl = new TextBlock() { Text = String.Format("Current camera Url: {0}", currentCameraUrl), Padding = new Thickness(5), Foreground = Application.Current.Resources["settingsPage_foregroundText"] as SolidColorBrush };
            Label lbl4_currentcameraID = new Label() { Content = String.Format("Current camera ID: {0}", currentCameraID) };
            Label lbl5_seperator1 = new Label() { Content = "------------------------------" };

            User_controls.settingsPage_NormalButton btn0_clearCameraList = new User_controls.settingsPage_NormalButton { Text = "Clear camera array and list", Margin = new Thickness(0, 10, 0, 0), HorizontalAlignment = HorizontalAlignment.Stretch };
            User_controls.settingsPage_NormalButton btn1_getUserConfig = new User_controls.settingsPage_NormalButton { Text = "Get user cameras", Margin = new Thickness(0, 5, 0, 0), HorizontalAlignment = HorizontalAlignment.Stretch };
            User_controls.settingsPage_NormalButton btn2_addErrorTestCamera = new User_controls.settingsPage_NormalButton { Text = "Add an error testing camera", Margin = new Thickness(0, 5, 0, 0), HorizontalAlignment = HorizontalAlignment.Stretch };

            User_controls.settingsPage_NormalButton btn3_ProgressUIDebug = new User_controls.settingsPage_NormalButton { Text = "Progress UI Debug", Margin = new Thickness(0, 10, 0, 0), HorizontalAlignment = HorizontalAlignment.Stretch };

            // add to panel
            panel.Children.Add(lbl0_usercameraCount);
            panel.Children.Add(lbl1_seperator0);
            panel.Children.Add(lbl2_currentcameraName);
            panel.Children.Add(lbl3_currentcameraUrl);
            panel.Children.Add(lbl4_currentcameraID);
            panel.Children.Add(lbl5_seperator1);

            panel.Children.Add(btn0_clearCameraList);
            panel.Children.Add(btn1_getUserConfig);
            panel.Children.Add(btn2_addErrorTestCamera);

            panel.Children.Add(btn3_ProgressUIDebug);

            #endregion
            dialog.ContentGrid = panel;

            #region Button click events

            btn0_clearCameraList.Click += (s, ev) =>
            {
                lbl0_usercameraCount.Content = "Number of user cameras: {0}" + 0;
                lbl2_currentcameraName.Text = "Current camera Name: ";
                lbl3_currentcameraUrl.Text = "Current camera Url: ";
                lbl4_currentcameraID.Content = "Current camera ID: ";

                // clear array
                _Cameras = null;

                // clear list
                menu_cameralist.Children.Clear();
            };

            btn1_getUserConfig.Click += (s, ev) =>
            {
                LoadUserCameras();

                lbl0_usercameraCount.Content = "Number of user cameras: {0}" + _Cameras.Count();
                lbl2_currentcameraName.Text = "Current camera Name: ";
                lbl3_currentcameraUrl.Text = "Current camera Url: ";
                lbl4_currentcameraID.Content = "Current camera ID: ";
            };

            btn2_addErrorTestCamera.Click += (s, ev) =>
            {
                lbl0_usercameraCount.Content = String.Format("Number of user cameras: {0}", (menu_cameralist.Children.Count + 1));

                // create error camera
                Webcam errorWebcam = new Webcam()
                {
                    Name = "Error testing camera",
                    Url = "",
                    SaveLocation = null,
                    RefreshRate = "0"
                };

                // add it to the list
                AddCameraToMenu(errorWebcam);
            };

            btn3_ProgressUIDebug.Click += async (s, ev) =>
            {
                dialog.Close();

                await Task.Delay(1000);

                Popups.ContentDialog dlg = new Popups.ContentDialog();
                dlg.Title = "HOME: Progress UI Debug";

                dlg.Button0_Text = "Cancel";
                dlg.Button1_Text = "Accept";

                #region Create content

                StackPanel pd_panel = new StackPanel();

                RadioButton btn0 = new RadioButton() { Content = "Generic", Margin = new Thickness(0, 0, 0, 0) };
                RadioButton btn1 = new RadioButton() { Content = "ImageSave", Margin = new Thickness(0, 5, 0, 0) };
                RadioButton btn2 = new RadioButton() { Content = "ImageSave_BothSave", Margin = new Thickness(0, 5, 0, 0) };

                TextBox box0 = new TextBox() { Text = "Status text" };

                pd_panel.Children.Add(btn0);
                pd_panel.Children.Add(btn1);
                pd_panel.Children.Add(btn2);

                pd_panel.Children.Add(box0);

                #endregion

                dlg.ContentGrid = pd_panel;

                dlg.Button1_Click += (s1, ev1) =>
                {
                    ShowProgressUI();
                    if (btn0.IsChecked == true)
                        UpdateProgressUI(ProgressUI_Type.Generic);
                    if (btn1.IsChecked == true)
                        UpdateProgressUI(ProgressUI_Type.ImageSave);
                    if (btn2.IsChecked == true)
                        UpdateProgressUI(ProgressUI_Type.ImageSave_BothSave);

                    DispatcherTimer timer = new DispatcherTimer();
                    timer.Interval = TimeSpan.FromSeconds(5);
                    timer.Tick += (s2, ev2) => { timer.Stop(); HideProgressUI(); };
                    timer.Start();
                };

                dlg.ShowDialog();
            };

            #endregion

            // Show the dialog
            dialog.ShowDialog();
        }

        #endregion
    }
}
