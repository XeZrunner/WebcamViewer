using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace WebcamViewer.Updates.Updates.User_controls
{
    public partial class webcamPage_Overview_CameraButton : UserControl
    {
        public webcamPage_Overview_CameraButton()
        {
            InitializeComponent();
        }

        private void usercontrol_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            
        }

        private void image_SourceUpdated(object sender, System.Windows.Data.DataTransferEventArgs e)
        {
            double finalSize = (image.Source.Width + image.Source.Height / 8) + 30;
            this.Width = finalSize;
            this.Height = finalSize;
        }

        public event RoutedEventHandler Click;

        public string ErrorMessage;

        private bool _IsError = false;

        private bool _MobileView = false;

        [Description("The Source property of the Image of the button"), Category("Common")]
        public ImageSource Image
        {
            get { return image.Source; }
            set { image.Source = value; mobileView_image.Source = value; }
        }

        [Description("The name of the camera"), Category("Common")]
        public string CameraName
        {
            get { return textblock.Text; }
            set { textblock.Text = value; mobileView_titleLabel.Content = value; }
        }

        [Description("Mobile-view only: Description / Info"), Category("Common")]
        public string MobileDescription
        {
            get { return mobileView_descriptionLabel.Content as string; }
            set { mobileView_descriptionLabel.Content = value; }
        }

        [Description("The error state of the button"), Category("Miscellaneous")]
        public bool IsError
        {
            get { return _IsError; }
            set
            {
                _IsError = value;

                if (_IsError)
                {
                    errorGrid.Visibility = Visibility.Visible;
                }
                else
                {
                    errorGrid.Visibility = Visibility.Hidden;
                }
            }
        }

        [Description("Determines whether to use the mobile view"), Category("Common")]
        public bool UseMobileView
        {
            get { return mobileView.IsVisible; }
            set { _MobileView = value; UpdateView(); }
        }

        /// <summary>
        /// Updates the view (mobile or desktop)
        /// </summary>
        private void UpdateView()
        {
            if (_MobileView)
            {
                // switch to mobile view
                mobileView.Visibility = Visibility.Visible;
                deskopView.Visibility = Visibility.Collapsed;
                this.Height = 60; this.Width = Double.NaN;
            }
            else
            {
                // switch to desktop view
                mobileView.Visibility = Visibility.Collapsed;
                deskopView.Visibility = Visibility.Visible;
                this.Height = 263; this.Width = 263;
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            //bubble the event up to the parent
            if (this.Click != null)
                this.Click(this, e);
        }
    }
}
