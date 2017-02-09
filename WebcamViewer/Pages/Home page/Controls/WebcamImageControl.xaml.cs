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

namespace WebcamViewer.Pages.Home_page.Controls
{
    /// <summary>
    /// Interaction logic for WebcamImageControl.xaml
    /// </summary>
    public partial class WebcamImageControl : UserControl
    {
        public WebcamImageControl()
        {
            InitializeComponent();
        }

        Debug Debug = new Debug();

        #region UI



        #endregion

        #region Engine

        public event EventHandler LoadCameraStarted;
        public event EventHandler LoadCameraCompleted;
        public event EventHandler LoadCameraFailed;

        /// <summary>
        /// Load a camera either from a string that's an URL to an image, or if you
        /// really want to, you can give it a Webcam and it will grab the URL from it.
        /// </summary>
        /// <param name="url">The string that this method will attempt to download the image from.</param>
        /// <param name="webcam">The Webcam class that this method will get the string of the URL from, and attempt to download the image from that string...</param>
        /// <returns></returns>
        public async Task LoadCamera(string url = "")
        {
            BitmapImage image;

            // 'Started' event
            //LoadCameraStarted(this, new EventArgs());

            using (WebClient client = new WebClient())
            {
                try
                {
                    var bytes = await client.DownloadDataTaskAsync(url);

                    image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.None;
                    image.StreamSource = new MemoryStream(bytes);
                    image.EndInit();

                    webcamimage.Source = image;
                }
                catch (WebException ex)
                {
                    // 'Failed' event
                    //LoadCameraFailed(ex, new EventArgs());

                    // Log error
                    Debug.Log("HOME: Could not load camera\n" + ex.Message + "\n");
                }
                finally
                {
                    // 'Completed' event
                    //LoadCameraCompleted(this, new EventArgs());
                }
            }
        }

        #endregion
    }
}
