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
                mainView_debugButton.Visibility = Visibility.Collapsed;
        }

        string app_debugmode = Properties.Settings.Default.app_debugmode;

        MainWindow mainwindow = Application.Current.MainWindow as MainWindow;

        #region Classes

        public class OverviewCamera
        {
            public string Name;
            public string Url;
            public string Description;
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

            int counter = 0;
            foreach (string camera in Properties.Settings.Default.camera_names)
            {
                BitmapImage image = null;

                using (WebClient client = new WebClient())
                {
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

                        string filesizeInKilobytes = "0KB";
                        if (image != null)
                        {
                            if (image.StreamSource.Length >= (1 << 10))
                                filesizeInKilobytes = string.Format("{0}KB", image.StreamSource.Length >> 10);
                        }
                        else
                            filesizeInKilobytes = "null";

                        string dimensions = (int)image.Width + "x" + (int)image.Height;

                        string description = String.Format("Dimensions: {0}\nFile size: {1}", dimensions, filesizeInKilobytes);

                        cameralist.Add(new OverviewCamera()
                        {
                            Name = camera,
                            Url = Properties.Settings.Default.camera_urls[counter],
                            Description = description,
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
                }

                counter++;
            }

            #endregion

            #region Add cameras to WrapPanel
            foreach (OverviewCamera camera in cameralist)
            {
                User_controls.webcamPage_Overview_CameraButton button = new User_controls.webcamPage_Overview_CameraButton()
                {
                    CameraName = camera.Name,
                    MobileDescription = camera.Description,
                    Image = camera.Image,
                    Tag = camera.Url
                };

                if (camera.Error != null)
                {
                    button.IsError = true;
                    button.ErrorMessage = camera.Error;
                }

                // Foreground color
                button.SetResourceReference(User_controls.webcamPage_Overview_CameraButton.ForegroundProperty, "settingsPage_foregroundText");

                // Click event
                button.Click += overviewCameraButton_Click;

                mainView_wrappanel.Children.Add(button);
            }
            #endregion

            if (mainView_wrappanel.Children.Count == 0) // no cameras
                mainView_nocamerasLabel.Visibility = Visibility.Visible;
            else
                mainView_nocamerasLabel.Visibility = Visibility.Collapsed;

            mainView_progressarc.Visibility = Visibility.Hidden;

            UpdateViewSizing();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index">The index of the button to refresh.</param>
        public async Task RefreshCamera(int index)
        {
            User_controls.webcamPage_Overview_CameraButton sButton = mainView_wrappanel.Children[index] as User_controls.webcamPage_Overview_CameraButton;
            User_controls.webcamPage_Overview_CameraButton newButton = null;

            #region Recreate button

            if ((string)sButton.Tag != "ERRORTEST")
            {
                BitmapImage image = null;

                string url = Properties.Settings.Default.camera_urls[Properties.Settings.Default.camera_urls.IndexOf((string)sButton.Tag)];

                using (WebClient client = new WebClient())
                {
                    try
                    {
                        var bytes = await client.DownloadDataTaskAsync(url + camera_dummy());

                        image = new BitmapImage();
                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.None;
                        image.StreamSource = new MemoryStream(bytes);
                        image.EndInit();

                        int imageWidth = image.PixelWidth;
                        int imageHeight = image.PixelHeight;

                        string filesizeInKilobytes = "0KB";
                        if (image != null)
                        {
                            if (image.StreamSource.Length >= (1 << 10))
                                filesizeInKilobytes = string.Format("{0}KB", image.StreamSource.Length >> 10);
                        }
                        else
                            filesizeInKilobytes = "null";

                        string dimensions = (int)image.Width + "x" + (int)image.Height;

                        string description = String.Format("Dimensions: {0}\nFile size: {1}", dimensions, filesizeInKilobytes);

                        newButton = new User_controls.webcamPage_Overview_CameraButton()
                        {
                            CameraName = sButton.Name,
                            MobileDescription = description,
                            Image = image,
                            Tag = url
                        };
                    }
                    catch (Exception ex)
                    {
                        newButton = new User_controls.webcamPage_Overview_CameraButton()
                        {
                            CameraName = sButton.Name,
                            Image = null,
                            Tag = url,
                            IsError = true,
                            ErrorMessage = ex.Message
                        };
                    }
                }
            }
            else // ERRORTEST
            {
                newButton = sButton;

                await Task.Delay(TimeSpan.FromSeconds(5));
            }

            #endregion

            mainView_wrappanel.Children.RemoveAt(index);
            mainView_wrappanel.Children.Insert(index, newButton);
        }

        private async void overviewCameraButton_Click(object sender, RoutedEventArgs e)
        {
            User_controls.webcamPage_Overview_CameraButton sButton = sender as User_controls.webcamPage_Overview_CameraButton;

            if (sButton.IsError)
            {
                Popups.MessageDialog dialog = new Popups.MessageDialog();
                dialog.Title = "This camera is not feeling well...";
                dialog.Content = "We couldn't get the image from the camera.\nHere's the error it gave us: \n\n" + sButton.ErrorMessage;
                dialog.FirstButtonContent = "Reload camera";
                dialog.SecondButtonContent = "This is OK";


                dialog.FirstButtonClickEvent += async (s, ev) =>
                {
                    #region Add retry progress
                    StackPanel panel = new StackPanel() { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                    User_controls.ArcProgress progress = new User_controls.ArcProgress() { Height = 30, Width = 30, Color = Application.Current.Resources["accentcolor_light"] as SolidColorBrush };
                    Label lbl0 = new Label() { Content = "Retrying...", FontSize = 14, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(10, 0, 0, 0) };

                    panel.Children.Add(progress);
                    panel.Children.Add(lbl0);

                    dialog.dialogWindow.contentGrid.Children.Clear();
                    dialog.dialogWindow.contentGrid.Children.Add(panel);

                    dialog.dialogWindow.rowdefinition_2.Height = new GridLength(0);
                    #endregion

                    int counter = 0;
                    foreach (User_controls.webcamPage_Overview_CameraButton btn in mainView_wrappanel.Children)
                    {
                        if (btn != sButton)
                            counter++;
                    }

                    await RefreshCamera(counter);

                    dialog.Close();
                };

                dialog.SecondButtonClickEvent += (s1, ev1) => { dialog.Close(); };

                dialog.ShowDialog();

                return;
            }

            // switch back
            mainwindow.webcamPage_MainContentGrid_SwitchToCameraView();

            await mainwindow.LoadImage((string)sButton.Tag);

            // activate camera button in menu
            foreach (User_controls.webcamPage_menuGrid_CameraButton btn in mainwindow.webcamPage_menuGrid_cameraListStackPanel.Children)
            {
                if (btn.Text == sButton.CameraName)
                    btn.IsActive = true;
                else
                    btn.IsActive = false;
            }
        }

        private void mainView_refreshButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshOverview();
        }

        private void mainView_debugButton_Click(object sender, RoutedEventArgs e)
        {
            Popups.MessageDialog dialog = new Popups.MessageDialog();
            dialog.Title = "Overview debug";
            dialog.FirstButtonContent = "Close";

            // create content
            StackPanel panel = new StackPanel();
            Label lbl0 = new Label() { Content = "Total count of user cameras: " + Properties.Settings.Default.camera_names.Count };
            Label lbl1 = new Label() { Content = "Count of cameras in Overview: " + mainView_wrappanel.Children.Count };

            int countProblematicCameras = 0;
            foreach (User_controls.webcamPage_Overview_CameraButton button in mainView_wrappanel.Children)
                if (button.IsError)
                    countProblematicCameras++;

            int countErrorTestCameras = 0;
            foreach (User_controls.webcamPage_Overview_CameraButton button in mainView_wrappanel.Children)
                if ((string)button.Tag == "ERRORTEST")
                    countErrorTestCameras++;

            Label lbl2 = new Label() { Content = "Problematic cameras: " + countProblematicCameras };

            Label lbl3 = new Label() { Content = "Error testing cameras: " + countErrorTestCameras };

            Label lbl4 = new Label() { Content = "------------------------------" };

            Label lbl5 = new Label() { Content = "app_debugmode: " + app_debugmode };

            User_controls.settingsPage_NormalButton btn0 = new User_controls.settingsPage_NormalButton() { Text = "Clear overview", Margin = new Thickness(0, 10, 0, 0), HorizontalAlignment = HorizontalAlignment.Stretch };
            User_controls.settingsPage_NormalButton btn1 = new User_controls.settingsPage_NormalButton() { Text = "Hide debug buttons", Margin = new Thickness(0, 5, 0, 0), HorizontalAlignment = HorizontalAlignment.Stretch };
            User_controls.settingsPage_NormalButton btn2 = new User_controls.settingsPage_NormalButton() { Text = "Spawn error camera", Margin = new Thickness(0, 5, 0, 0), HorizontalAlignment = HorizontalAlignment.Stretch };

            btn0.Click += (s, ev) => { mainView_wrappanel.Children.Clear(); mainView_nocamerasLabel.Visibility = Visibility.Visible; };
            btn1.Click += (s, ev) =>
            {
                if (mainView_debugButton.Visibility == Visibility.Visible)
                { mainView_debugButton.Visibility = Visibility.Collapsed; btn1.Text = "Show debug buttons"; }
                else
                { mainView_debugButton.Visibility = Visibility.Visible; btn1.Text = "Hide debug buttons"; }
            };
            btn2.Click += (s, ev) =>
            {
                User_controls.webcamPage_Overview_CameraButton button = new User_controls.webcamPage_Overview_CameraButton()
                {
                    CameraName = "Error testing camera",
                    Image = null,
                    IsError = true,
                    ErrorMessage = "Imagine there's an error...",
                    Tag = "ERRORTEST"
                };

                // Foreground color
                button.SetResourceReference(User_controls.webcamPage_Overview_CameraButton.ForegroundProperty, "settingsPage_foregroundText");

                // Click event
                button.Click += overviewCameraButton_Click;

                mainView_wrappanel.Children.Add(button);

                string lbl3Content = (string)lbl3.Content;
                int currentErrorTestCameras = int.Parse(lbl3Content.Substring(lbl3Content.Length - 1, 1));

                lbl3Content.Replace(currentErrorTestCameras.ToString(), (currentErrorTestCameras + 1).ToString());

                lbl3.Content = lbl3Content;
            };

            // add everything to panel
            panel.Children.Add(lbl0);
            panel.Children.Add(lbl1);
            panel.Children.Add(lbl2);
            panel.Children.Add(lbl3);
            panel.Children.Add(lbl4);
            panel.Children.Add(lbl5);
            panel.Children.Add(btn0);
            panel.Children.Add(btn1);
            panel.Children.Add(btn2);

            // add panel to dialog content
            dialog.Content = panel;

            dialog.ShowDialog();
        }

        #endregion

        void UpdateViewSizing()
        {
            if (this.ActualWidth <= 545)
            {
                foreach (User_controls.webcamPage_Overview_CameraButton btn in mainView_wrappanel.Children)
                {
                    btn.UseMobileView = true;
                    mainView_wrappanel.HorizontalAlignment = HorizontalAlignment.Stretch;
                    mainView_wrappanel.VerticalAlignment = VerticalAlignment.Top;
                    mainView_wrappanel.ItemWidth = this.ActualWidth;
                }
            }
            else
            {
                foreach (User_controls.webcamPage_Overview_CameraButton btn in mainView_wrappanel.Children)
                {
                    btn.UseMobileView = false;
                    mainView_wrappanel.HorizontalAlignment = HorizontalAlignment.Center;
                    mainView_wrappanel.VerticalAlignment = VerticalAlignment.Center;
                    mainView_wrappanel.ItemWidth = Double.NaN;
                }
            }
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateViewSizing();
        }
    }
}
