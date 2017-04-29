using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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

namespace WebcamViewer.WebcamViewerM_EE
{
    public partial class Home : Page
    {
        public Home()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ShowProgressUI(ProgressTypes.Progressring); // show the progress UI
            UpdateProgressUIStatus("Getting ready...");

            GetCameras(); // get the cameras
            await LoadWebcam(cameras[0]); // load the first camera

            await Task.Delay(1000);

            Storyboard successBoard = (Storyboard)FindResource("particle");
            successBoard.Begin();
        }

        MainWindow mainwindow = Application.Current.MainWindow as MainWindow;

        #region Webcam engine

        string ver_name = "Material Webcam Viewer Easter Egg Webcam Engine";
        string ver_version = "1.0";

        string current_url;

        /// <summary>
        /// A generic Webcam class containing basic properties for the user cameras such as Name, Url...
        /// </summary>
        class Webcam
        {
            public string Name;
            public string Url;
        }

        /// <summary>
        /// The array containing the user cameras. 
        /// Populate with GetCameras().
        /// </summary>
        Webcam[] cameras;

        /// <summary>
        /// The dummy that's put after the URL of a camera to make sure we load the latest image and not some cached
        /// version or something.
        /// </summary>
        string camera_dummy()
        {
            return "?dummy=" + DateTime.Now.Ticks;
        }

        /// <summary>
        /// Loads the user cameras into the 'cameras' array and populates the menu.
        /// </summary>
        void GetCameras()
        {
            cameras = new Webcam[Properties.Settings.Default.camera_names.Count];

            int counter = 0;
            foreach (string cam in Properties.Settings.Default.camera_names)
            {
                string url = Properties.Settings.Default.camera_urls[Properties.Settings.Default.camera_names.IndexOf(cam)];

                Webcam camera = new Webcam()
                {
                    Name = cam,
                    Url = url
                };

                cameras[counter] = camera;

                counter++;
            }

            AddCamerasIntoMenu();
        }

        /// <summary>
        /// Adds the array's cameras to the UI menu. 
        /// Automatically called by GetCameras().
        /// </summary>
        void AddCamerasIntoMenu()
        {
            int counter = 0;
            foreach (Webcam camera in cameras)
            {
                User_controls.webcamPage_menuGrid_CameraButton btn = new User_controls.webcamPage_menuGrid_CameraButton()
                {
                    Text = camera.Name,
                    Tag = counter,
                };

                btn.Click += menu_webcamButton_Click;

                menu_cameraList.Children.Add(btn);

                counter++;
            }
        }

        /// <summary>
        /// Load the image from the Webcam class given this method.
        /// </summary>
        /// <param name="webcam">The Webcam class to load the camera image from.</param>
        async Task LoadWebcam(Webcam webcam)
        {
            // Show progress UI
            ShowProgressUI(ProgressTypes.Progressring);
            UpdateProgressUIStatus("Loading image...");

            // Set the current image URL
            current_url = webcam.Url;

            BitmapImage image = null;

            // Get the latest image
            using (WebClient client = new WebClient())
            {
                try
                {
                    var bytes = await client.DownloadDataTaskAsync(webcam.Url + camera_dummy());

                    image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.None;
                    image.StreamSource = new MemoryStream(bytes);
                    image.EndInit();

                    webcamimage.Source = image;
                }
                catch (WebException ex)
                {
                    Popups.ContentDialog dialog = new Popups.ContentDialog() { Title = "Could not load webcam image...", Text = "Error: " + ex.Message };
                    dialog.ShowDialog();
                }
                finally
                {
                    // Hide progress UI and update info
                    HideProgressUI();

                    actionBar_cameraLabel.Content = webcam.Name.ToUpper();
                }
            }
        }

        #endregion

        #region User interface elements

        #region Progress UI

        public enum ProgressTypes
        {
            Progressring,
            Progressbar
        }

        /// <summary>
        /// Show the progress UI.
        /// </summary>
        /// <param name="progressType">The type of progress control to show.</param>
        public void ShowProgressUI(ProgressTypes progressType = ProgressTypes.Progressring)
        {
            // Animate the progressgrid in
            progressGrid.Visibility = Visibility.Visible;

            DoubleAnimation anim_in = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(.3));
            progressGrid.BeginAnimation(OpacityProperty, anim_in);

            switch (progressType)
            {
                case ProgressTypes.Progressring:
                    {
                        progressGrid_progressRing.Visibility = Visibility.Visible;
                        progressGrid_progressBar.Visibility = Visibility.Hidden;

                        break;
                    }
                case ProgressTypes.Progressbar:
                    {
                        progressGrid_progressRing.Visibility = Visibility.Hidden;
                        progressGrid_progressBar.Visibility = Visibility.Visible;

                        break;
                    }
            }
        }

        /// <summary>
        /// Hide the progress UI.
        /// </summary>
        public void HideProgressUI()
        {
            // animate the progressgrid out
            DoubleAnimation anim_out = new DoubleAnimation(0, TimeSpan.FromSeconds(.3));
            anim_out.Completed += (s, ev) => { progressGrid.Visibility = Visibility.Hidden; };

            progressGrid.BeginAnimation(OpacityProperty, anim_out);
        }

        /// <summary>
        /// Update the Progress UI Status text.
        /// </summary>
        /// <param name="status">The text to update the status to.</param>
        public void UpdateProgressUIStatus(string status = "")
        {
            progressGrid_progressLabel.Content = status;
        }

        #endregion

        #region Menu

        bool _isMenuOpen = false;

        public void ShowMenu()
        {
            menuGrid.Visibility = Visibility.Visible;

            // animation
            DoubleAnimation anim = new DoubleAnimation(-300, 0, TimeSpan.FromSeconds(.3));
            menu_translateTransform.BeginAnimation(TranslateTransform.XProperty, anim);

            _isMenuOpen = true;
        }

        public void HideMenu()
        {
            // animation
            DoubleAnimation anim = new DoubleAnimation(0, -300, TimeSpan.FromSeconds(.3));
            anim.Completed += (s, ev) => { menuGrid.Visibility = Visibility.Hidden; };

            menu_translateTransform.BeginAnimation(TranslateTransform.XProperty, anim);

            _isMenuOpen = false;
        }

        #endregion

        #endregion

        #region UI Click events

        private void menuButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isMenuOpen)
                ShowMenu();
            else
                HideMenu();
        }

        private async void menu_webcamButton_Click(object sender, RoutedEventArgs e)
        {
            User_controls.webcamPage_menuGrid_CameraButton button = sender as User_controls.webcamPage_menuGrid_CameraButton;
            int button_Tag = (int)button.Tag;

            button.IsActive = true;

            foreach (User_controls.webcamPage_menuGrid_CameraButton buttonx in menu_cameraList.Children)
                if (buttonx.Tag != button.Tag)
                    buttonx.IsActive = false;

            await LoadWebcam(cameras[button_Tag]);
        }

        private void menu_localsaveButton_Click(object sender, RoutedEventArgs e)
        {
            mainwindow.TextContentDialog("Local saving is not available right now.", "Due to limitations of this page, local saving is not available.", "OK", User_controls.ContentDialogControl._Theme.Dark);
        }

        private void menu_archiveorgsaveButton_Click(object sender, RoutedEventArgs e)
        {
            mainwindow.TextContentDialog("Archive.org saving is not available right now.", "Due to limitations of this page, archive.org saving is not available.", "OK", User_controls.ContentDialogControl._Theme.Dark);
        }

        private void menu_settingsButton_Click(object sender, RoutedEventArgs e)
        {
            // switch to settings page
        }

        #endregion
    }
}