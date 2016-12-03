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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WebcamViewer.Pages.Home_page.Overview
{
    public partial class Page
    {
        public Page()
        {
            InitializeComponent();

            if (app_debugmode == "release")
                mainView_debugMenuButton.Visibility = Visibility.Collapsed;
        }

        string app_debugmode = Properties.Settings.Default.app_debugmode;

        MainWindow mainwindow = Application.Current.MainWindow as MainWindow;

        #region Classes

        public class OverviewCamera
        {
            public string Name;
            public string Url;
            public int Width;
            public int Height;
            public int FileSize;
            public BitmapImage Image;
            public string Error;
        }

        #endregion

        #region Main view

        private void mainView_backButton_Click(object sender, RoutedEventArgs e)
        {
            mainwindow.webcamPage_MainContentGrid_SwitchToCameraView();
        }

        string camera_dummy()
        {
            return "?dummy=" + DateTime.Now.Ticks;
        }

        public async void RefreshOverview()
        {
            mainView_wrappanel.Children.Clear();

            List<OverviewCamera> cameralist = new List<OverviewCamera>();

            mainView_progressarc.Visibility = Visibility.Visible;

            #region Get cameras
            foreach (string camera in Properties.Settings.Default.camera_names)
            {
                BitmapImage image = null;

                using (WebClient client = new WebClient())
                {
                    int counter = 0;
                    try
                    {
                        string url = Properties.Settings.Default.camera_urls[Properties.Settings.Default.camera_names.IndexOf(camera)];

                        var bytes = await client.DownloadDataTaskAsync(url + camera_dummy());

                        image = new BitmapImage();
                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.None;
                        image.StreamSource = new MemoryStream(bytes);
                        image.EndInit();

                        int imageWidth = image.PixelWidth;
                        int imageHeight = image.PixelHeight;

                        cameralist.Add(new OverviewCamera()
                        {
                            Name = camera,
                            Url = Properties.Settings.Default.camera_urls[counter],
                            Width = imageWidth,
                            Height = imageHeight,
                            Image = image
                        }
                        );
                    }
                    catch (Exception ex)
                    {
                        cameralist.Add(new OverviewCamera()
                        {
                            Name = camera,
                            Url = Properties.Settings.Default.camera_urls[counter],
                            Width = 0,
                            Height = 0,
                            Image = null,
                            Error = ex.Message
                        }
                        );
                    }
                    counter++;
                }
            }
            #endregion

            #region Add cameras to WrapPanel
            foreach (OverviewCamera camera in cameralist)
            {
                User_controls.webcamPage_Overview_CameraButton button = new User_controls.webcamPage_Overview_CameraButton()
                {
                    CameraName = camera.Name,
                    Image = camera.Image
                };

                if (camera.Error != null)
                    button.IsError = true;

                mainView_wrappanel.Children.Add(button);
            }
            #endregion

            mainView_progressarc.Visibility = Visibility.Hidden;
        }

        #endregion

        private void mainView_refreshButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshOverview();
        }

        private void mainView_debugMenuButton_Click(object sender, RoutedEventArgs e)
        {
            if (mainwindow.webcamPage_menuGrid.IsVisible == false)
                mainwindow.webcamPage_OpenMenu();
            else
                mainwindow.webcamPage_CloseMenu();
        }
    }
}
