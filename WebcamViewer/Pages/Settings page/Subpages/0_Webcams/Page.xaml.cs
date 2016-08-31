using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

namespace WebcamViewer.Pages.Settings_page.Subpages._0_Webcams
{
    /// <summary>
    /// Interaction logic for Page.xaml
    /// </summary>
    public partial class Page : UserControl
    {
        public Page()
        {
            InitializeComponent();
        }

        private void page_Loaded(object sender, RoutedEventArgs e)
        {
            // first entry actions
            if (!settingsPage_WebcamEditorPage_createdDragDropManager)
            {
                dragdropManager = new ListViewDragDropManager<settingsPage_WebcamEditorPage_Camera>(settingsPage_WebcamEditorPage_ListView);

                dragdropManager.ProcessDrop += settingsPage_WebcamEditorPage_ListView_dragdropManager_ProcessDrop;

                settingsPage_WebcamEditorPage_createdDragDropManager = true;
            }

            // IsVisibleChanged handles the populating/refreshing

            // disable some stuff
            settingsPage_WebcamEditorPage_ItemEditor_NameTextBox.Clear();
            settingsPage_WebcamEditorPage_ItemEditor_UrlTextBox.Clear();
            settingsPage_WebcamEditorPage_ItemEditor_SaveLocationTextBox.Clear();
            settingsPage_WebcamEditorPage_ItemEditor_RefreshRateTextBox.Clear();

            settingsPage_WebcamEditorPage_DeleteCameraButton.IsEnabled = false;
        }

        private void page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.Visibility == Visibility.Visible)
                RefreshItemList();
        }

        MainWindow mainwindow = Application.Current.MainWindow as MainWindow;

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

            mainwindow.global_Dim();

            dlg.ShowDialog();

            mainwindow.global_UnDim();
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

            mainwindow.global_Dim();

            dlg.ShowDialog();

            mainwindow.global_UnDim();
        }

        #endregion

        ListViewDragDropManager<settingsPage_WebcamEditorPage_Camera> dragdropManager = new ListViewDragDropManager<settingsPage_WebcamEditorPage_Camera>();

        ObservableCollection<settingsPage_WebcamEditorPage_Camera> WebcamEditor_ListView_items = new ObservableCollection<settingsPage_WebcamEditorPage_Camera>();

        bool settingsPage_WebcamEditorPage_createdDragDropManager = false;

        private void settingsPage_WebcamEditorPage_ListView_dragdropManager_ProcessDrop(object sender, ProcessDropEventArgs<settingsPage_WebcamEditorPage_Camera> e)
        {
            settingsPage_WebcamEditorPage_ListView_handleSelectionChange = false;

            settingsPage_WebcamEditorPage_ItemEditorGrid_Main.Visibility = Visibility.Collapsed;

            CameraEditingButtons.Visibility = Visibility.Collapsed;

            settingsPage_WebcamEditorPage_ItemEditorGrid_Disabled.Visibility = Visibility.Visible;

            e.ItemsSource.Move(e.OldIndex, e.NewIndex);

            e.Effects = DragDropEffects.Move;
        }

        bool settingsPage_WebcamEditorPage_ListView_handleSelectionChange = true;

        private void settingsPage_WebcamEditorPage_ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (settingsPage_WebcamEditorPage_ListView_handleSelectionChange)
            {
                settingsPage_WebcamEditorPage_ItemEditorGrid_Main.Visibility = Visibility.Visible;
                CameraEditingButtons.Visibility = Visibility.Visible;
                settingsPage_WebcamEditorPage_ItemEditorGrid_Disabled.Visibility = Visibility.Collapsed;

                settingsPage_WebcamEditorPage_DeleteCameraButton.IsEnabled = true;

                RefreshItemEditor();

                previousIndex = settingsPage_WebcamEditorPage_ListView.SelectedIndex;
            }
        }

        int previousIndex = -1;
        bool comingfromNewCamera = false;

        void RefreshItemList()
        {
            settingsPage_WebcamEditorPage_ListView_handleSelectionChange = false;

            WebcamEditor_ListView_items.Clear();

            foreach (string cameraname in Properties.Settings.Default.camera_names)
            {
                WebcamEditor_ListView_items.Add(new settingsPage_WebcamEditorPage_Camera() { Name = cameraname, Url = Properties.Settings.Default.camera_urls[Properties.Settings.Default.camera_names.IndexOf(cameraname)], SaveLocation = "null", RefreshRate = 0 });
            }

            settingsPage_WebcamEditorPage_ListView.ItemsSource = WebcamEditor_ListView_items;

            settingsPage_WebcamEditorPage_ListView_handleSelectionChange = true;

            if (previousIndex != -1)
            {
                if (comingfromNewCamera == false)
                {
                    if (previousIndex < Properties.Settings.Default.camera_urls.Count) // if the previous index is NOT more than or equal to the count of indexes
                        settingsPage_WebcamEditorPage_ListView.SelectedIndex = previousIndex;
                    else // if the previous index is more than or equal to the count of indexes
                        settingsPage_WebcamEditorPage_ListView.SelectedIndex = previousIndex - 1;
                }
                else
                {
                    settingsPage_WebcamEditorPage_ListView.SelectedIndex = WebcamEditor_ListView_items.Count - 1;
                    comingfromNewCamera = false;
                }

                RefreshItemEditor();
            }
        }

        void RefreshItemEditor()
        {
            settingsPage_WebcamEditorPage_ItemEditor_NameTextBox.Text = Properties.Settings.Default.camera_names[settingsPage_WebcamEditorPage_ListView.SelectedIndex];
            settingsPage_WebcamEditorPage_ItemEditor_UrlTextBox.Text = Properties.Settings.Default.camera_urls[settingsPage_WebcamEditorPage_ListView.SelectedIndex];
        }

        // button click events
        private void settingsPage_WebcamEditorPage_SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.camera_names.Clear();
            Properties.Settings.Default.camera_urls.Clear();

            settingsPage_WebcamEditorPage_Camera targetListViewItem = settingsPage_WebcamEditorPage_ListView.Items[settingsPage_WebcamEditorPage_ListView.SelectedIndex] as settingsPage_WebcamEditorPage_Camera;
            targetListViewItem.Name = settingsPage_WebcamEditorPage_ItemEditor_NameTextBox.Text;
            targetListViewItem.Url = settingsPage_WebcamEditorPage_ItemEditor_UrlTextBox.Text;

            foreach (settingsPage_WebcamEditorPage_Camera item in settingsPage_WebcamEditorPage_ListView.Items)
            {
                // names
                Properties.Settings.Default.camera_names.Add(item.Name);

                // urls
                Properties.Settings.Default.camera_urls.Add(item.Url);
            }

            Properties.Settings.Default.Save();

            settingsPage_WebcamEditorPage_ListView_handleSelectionChange = true;

            settingsPage_WebcamEditorPage_ItemEditorGrid_Main.Visibility = Visibility.Visible;
            CameraEditingButtons.Visibility = Visibility.Visible;
            settingsPage_WebcamEditorPage_ItemEditorGrid_Disabled.Visibility = Visibility.Collapsed;

            RefreshItemEditor();
        }

        private void settingsPage_WebcamEditorPage_NewCameraButton_Click(object sender, RoutedEventArgs e)
        {
            settingsPage_WebcamEditorPage_Camera newcamera = new settingsPage_WebcamEditorPage_Camera()
            {
                Name = String.Format("Camera #{0}", Properties.Settings.Default.camera_names.Count + 1),
                Url = "https://cameraurlgoeshere.com/camera-69.jpg [REMOVE ANY ? parameters such as \"?dummy=xyzxyzxyz&ds=1\"]",
                SaveLocation = "",
                RefreshRate = 5
            };

            Properties.Settings.Default.camera_names.Add(newcamera.Name);
            Properties.Settings.Default.camera_urls.Add(newcamera.Url);

            Properties.Settings.Default.Save();

            comingfromNewCamera = true; RefreshItemList();

            RefreshItemEditor();
        }

        private void settingsPage_WebcamEditorPage_DeleteCameraButton_Click(object sender, RoutedEventArgs e)
        {
            Popups.MessageDialog dlg = new Popups.MessageDialog();
            dlg.Title = "";
            dlg.Content = "Are you sure you want to remove this camera?\nYou can NOT undo this action.";
            dlg.FirstButtonContent = "Cancel";
            dlg.SecondButtonContent = "Remove";

            mainwindow.global_Dim();

            if (dlg.ShowDialogWithResult() == 1)
            {
                Properties.Settings.Default.camera_names.RemoveAt(settingsPage_WebcamEditorPage_ListView.SelectedIndex);
                Properties.Settings.Default.camera_urls.RemoveAt(settingsPage_WebcamEditorPage_ListView.SelectedIndex);

                RefreshItemList();
            }

            mainwindow.global_UnDim();
        }

        // Class
        public class settingsPage_WebcamEditorPage_Camera
        {
            public string Name { get; set; }

            public string Url { get; set; }

            public string SaveLocation { get; set; }

            public int RefreshRate { get; set; }
        }
    }
}
