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

        #region UI

        #region enums: Progress UI
        public enum ProgressType
        {
            Progressring,
            Image_Saving,
            BothSave
        }

        public enum ProgressStyles
        {
            ProgressArc,
            ProgressBar
        }

        public enum BothSaveItemStatus
        {
            Progress,
            Completed,
            Failed
        }
        #endregion

        /// <summary>
        /// Shows the progress UI.
        /// </summary>
        /// <param name="progresstype">The type of progress UI to show.</param>
        public void ShowProgressUI(ProgressType progresstype, ProgressStyles? progressstyle = ProgressStyles.ProgressArc)
        {
            DoubleAnimation anim_opacity = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(.3));

            progressGrid.Visibility = Visibility.Visible;
            progressGrid.BeginAnimation(OpacityProperty, anim_opacity);

            #region show the type of progress UI

            progressGrid_ArcProgressUI.Visibility = Visibility.Collapsed;
            progressGrid_SavePanelUI.Visibility = Visibility.Collapsed;
            progressGrid_BothSaveUI.Visibility = Visibility.Collapsed;

            if (progresstype == ProgressType.Progressring)
            {
                progressGrid_ArcProgressUI.Visibility = Visibility.Visible;
            }

            if (progresstype == ProgressType.Image_Saving)
            {
                progressGrid_SavePanelUI.Visibility = Visibility.Visible;

                if (progressstyle != null)
                {
                    if (progressstyle == ProgressStyles.ProgressArc)
                        UpdateProgressStatusStyle(ProgressStyles.ProgressArc);
                    else
                        UpdateProgressStatusStyle(ProgressStyles.ProgressBar);
                }
                else
                    UpdateProgressStatusStyle(ProgressStyles.ProgressArc);
            }

            if (progresstype == ProgressType.BothSave)
            {
                progressGrid_BothSaveUI.Visibility = Visibility.Visible;
                UpdateBothSaveStatus(BothSaveItemStatus.Progress, BothSaveItemStatus.Progress);
            }
            #endregion
        }

        /// <summary>
        /// Hides the progress UI.
        /// </summary>
        public void HideProgressUI()
        {
            DoubleAnimation anim_opacity = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(.3));
            anim_opacity.Completed += (s, ev) => { progressGrid.Visibility = Visibility.Hidden; };

            progressGrid.BeginAnimation(OpacityProperty, anim_opacity);

            #region hide all types of the progress UI
            // *hide* all grids
            #endregion
        }
        
        public void UpdateProgressStatus(string text)
        {
            progressGrid_statusLabel.Content = text;
        }

        public void UpdateProgressStatusStyle(ProgressStyles style)
        {
            progressGrid_ArcProgress.Visibility = Visibility.Collapsed;
            progressGrid_ProgressBar.Visibility = Visibility.Collapsed;

            if (style == ProgressStyles.ProgressArc)
                progressGrid_ArcProgress.Visibility = Visibility.Visible;
            else
                progressGrid_ProgressBar.Visibility = Visibility.Visible;
        }

        public void UpdateBothSaveStatus(BothSaveItemStatus? localsave = null, BothSaveItemStatus? archiveorgsave = null)
        {
            if (localsave != null)
            {
                if (localsave == BothSaveItemStatus.Progress)
                    progressGrid_BothSave_LocalIndicator.Content = "\ue10c";
                if (localsave == BothSaveItemStatus.Completed)
                    progressGrid_BothSave_LocalIndicator.Content = "\ue73e";
                if (localsave == BothSaveItemStatus.Failed)
                    progressGrid_BothSave_LocalIndicator.Content = "\ue8bb";
            }

            if (archiveorgsave != null)
            {
                if (archiveorgsave == BothSaveItemStatus.Progress)
                    progressGrid_BothSave_ArchiveOrgIndicator.Content = "\ue10c";
                if (archiveorgsave == BothSaveItemStatus.Completed)
                    progressGrid_BothSave_ArchiveOrgIndicator.Content = "\ue73e";
                if (archiveorgsave == BothSaveItemStatus.Failed)
                    progressGrid_BothSave_ArchiveOrgIndicator.Content = "\ue8bb";
            }

            if ((string)progressGrid_BothSave_LocalIndicator.Content == "\ue73e" & (string)progressGrid_BothSave_ArchiveOrgIndicator.Content == "\ue73e")
                HideProgressUI();
        }

        /// <summary>
        /// Shows the error "tips" panel.
        /// </summary>
        public void ShowErrorUI()
        {
            DoubleAnimation anim_opacity = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(.3));

            errorGrid.Visibility = Visibility.Visible;
            errorGrid.BeginAnimation(OpacityProperty, anim_opacity);
        }

        /// <summary>
        /// Hides the error panel.
        /// </summary>
        public void HideErrorUI()
        {
            DoubleAnimation anim_opacity = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(.3));
            anim_opacity.Completed += (s, ev) => { errorGrid.Visibility = Visibility.Hidden; };

            errorGrid.BeginAnimation(OpacityProperty, anim_opacity);
        }

        #endregion

        #region Engine

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

            // Show progress UI
            ShowProgressUI(WebcamImageControl.ProgressType.Progressring);

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
                    new Popups.ContentDialog
                    {
                        Title = "Cannot load camera image...",
                        Text = "Error: " + ex.Message
                    }.ShowDialog();

                    // ShowErrorPanel();
                }
                finally
                {
                    HideProgressUI();
                }
            }
        }

        #endregion

        private void progressGrid_MoreInfoButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void progressGrid_CancelButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void progressGrid_BothSave_CloseButton_Click(object sender, RoutedEventArgs e)
        {
            HideProgressUI();
        }
    }
}
