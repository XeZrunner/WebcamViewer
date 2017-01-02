using System.Windows;
using System.Windows.Controls;

namespace WebcamViewer
{
    class Popups
    {
        /// <summary>
        /// Windows 10 UWP ContentDialog-style dialog ftw
        /// </summary>
        public class MessageDialog
        {

            public MessagedialogWindow dialogWindow = new MessagedialogWindow();

            public string Title = "";

            /// <summary>
            /// The content of the dialog.
            /// Can be any UIElement, preferably a Grid or StackPanel with items inside.
            /// </summary>
            public object Content = new Control();

            /// <summary>
            /// The theme of the dialog.
            /// If this is true, the dialog is going to be dark-themed. If false, it's going to be light-themed. If it's null, it's determined by the theme.
            /// </summary>
            public bool? IsDarkTheme = null;

            /// <summary>
            /// If enabled, the content part of the dialog is not going to have a margin.
            /// </summary>
            public bool Content_DisableMargin = false;

            public string FirstButtonContent = "OK";
            public string SecondButtonContent = "";

            public RoutedEventHandler FirstButtonClickEvent = null;
            public RoutedEventHandler SecondButtonClickEvent = null;

            /// <summary>
            /// Shows the dialog as modal, meaning that it blocks any interaction with the program and gives focus for the dialog.
            /// </summary>
            public void ShowDialog()
            {
                dialogWindow.SetupDialog(
                    Title,
                    Content,

                    IsDarkTheme,

                    Content_DisableMargin,

                    FirstButtonContent,
                    SecondButtonContent,

                    FirstButtonClickEvent,
                    SecondButtonClickEvent
                    );

                MainWindow mainwindow = Application.Current.MainWindow as MainWindow;

                dialogWindow.Owner = mainwindow;
                if (mainwindow.Width <= 482)
                    dialogWindow.MaxWidth = mainwindow.Width - 2;
                dialogWindow.MaxHeight = mainwindow.Height - 2;

                mainwindow.global_Dim();

                dialogWindow.ShowDialog();

                mainwindow.global_UnDim();
            }

            int ResultDialog_result;

            void ResultDialog_FirstButtonClickEvent(object sender, RoutedEventArgs e)
            {
                ResultDialog_result = 0;
                dialogWindow.Close();
            }

            void ResultDialog_SecondButtonClickEvent(object sender, RoutedEventArgs e)
            {
                ResultDialog_result = 1;
                dialogWindow.Close();
            }

            /// <summary>
            /// Shows the dialog as modal, and returns an integer of either 0 or 1.
            /// If it returns 0, it means the first (right-hand side) button was clicked. If it returns 1, it means the second (left-hand side) button was clicked.
            /// </summary>
            /// <returns></returns>
            public int ShowDialogWithResult()
            {
                dialogWindow.SetupDialog(
                    Title,
                    Content,

                    IsDarkTheme,

                    Content_DisableMargin,

                    FirstButtonContent,
                    SecondButtonContent,

                    ResultDialog_FirstButtonClickEvent,
                    ResultDialog_SecondButtonClickEvent
                    );

                MainWindow mainwindow = Application.Current.MainWindow as MainWindow;

                dialogWindow.Owner = mainwindow;
                dialogWindow.MaxHeight = mainwindow.Height - 2;

                mainwindow.global_Dim();

                dialogWindow.ShowDialog();

                mainwindow.global_UnDim();

                return ResultDialog_result;
            }

            /// <summary>
            /// Shows the dialog without blocking interaction with the program.
            /// </summary>
            public void Show()
            {
                dialogWindow.SetupDialog(
                    Title,
                    Content,

                    IsDarkTheme,

                    Content_DisableMargin,

                    FirstButtonContent,
                    SecondButtonContent,

                    FirstButtonClickEvent,
                    SecondButtonClickEvent
                    );

                MainWindow mainwindow = Application.Current.MainWindow as MainWindow;

                dialogWindow.Owner = mainwindow;
                dialogWindow.MaxHeight = mainwindow.Height - 2;

                mainwindow.global_Dim();

                dialogWindow.Show();

                mainwindow.global_UnDim();
            }

            /// <summary>
            /// Closes the dialog.
            /// </summary>
            public void Close()
            {
                dialogWindow.Close();
            }
        }

        /// <summary>
        /// Windows 8.x MessageDialog-style dialog
        /// </summary>
        public class MessageDialog_FullWidth
        {

            Messagedialog_FullWidthWindow dialogWindow = new Messagedialog_FullWidthWindow();

            public string Title = "";

            /// <summary>
            /// The content of the dialog.
            /// Can be any UIElement, preferably a Grid or StackPanel with items inside.
            /// </summary>
            public object Content = new Control();

            /// <summary>
            /// The theme of the dialog.
            /// If this is true, the dialog is going to be dark-themed. If false, it's going to be light-themed. If it's null, it's determined by the theme.
            /// </summary>
            public bool? IsDarkTheme = null;

            /// <summary>
            /// If enabled, the content part of the dialog is not going to have a margin.
            /// </summary>
            public bool Content_DisableMargin = false;

            public string FirstButtonContent = "OK";
            public string SecondButtonContent = "";

            public RoutedEventHandler FirstButtonClickEvent = null;
            public RoutedEventHandler SecondButtonClickEvent = null;

            /// <summary>
            /// Shows the dialog as modal, meaning that it blocks any interaction with the program and gives focus for the dialog.
            /// </summary>
            public void ShowDialog()
            {
                dialogWindow.SetupDialog(
                    Title,
                    Content,

                    IsDarkTheme,

                    Content_DisableMargin,

                    FirstButtonContent,
                    SecondButtonContent,

                    FirstButtonClickEvent,
                    SecondButtonClickEvent
                    );

                MainWindow mainwindow = Application.Current.MainWindow as MainWindow;

                dialogWindow.Owner = mainwindow;
                dialogWindow.MaxHeight = mainwindow.Height - 2;

                mainwindow.global_Dim();

                dialogWindow.ShowDialog();

                mainwindow.global_UnDim();
            }

            int ResultDialog_result;

            void ResultDialog_FirstButtonClickEvent(object sender, RoutedEventArgs e)
            {
                ResultDialog_result = 0;
                dialogWindow.Close();
            }

            void ResultDialog_SecondButtonClickEvent(object sender, RoutedEventArgs e)
            {
                ResultDialog_result = 1;
                dialogWindow.Close();
            }

            /// <summary>
            /// Shows the dialog as modal, and returns an integer of either 0 or 1.
            /// If it returns 0, it means the first (left-hand side) button was clicked. If it returns 1, it means the second (right-hand side) button was clicked.
            /// </summary>
            /// <returns></returns>
            public int ShowDialogWithResult()
            {
                dialogWindow.SetupDialog(
                    Title,
                    Content,

                    IsDarkTheme,

                    Content_DisableMargin,

                    FirstButtonContent,
                    SecondButtonContent,

                    ResultDialog_FirstButtonClickEvent,
                    ResultDialog_SecondButtonClickEvent
                    );

                MainWindow mainwindow = Application.Current.MainWindow as MainWindow;

                dialogWindow.Owner = mainwindow;
                dialogWindow.MaxHeight = mainwindow.Height - 2;

                mainwindow.global_Dim();

                dialogWindow.ShowDialog();

                mainwindow.global_UnDim();

                return ResultDialog_result;
            }

            /// <summary>
            /// Shows the dialog without blocking interaction with the program.
            /// </summary>
            public void Show()
            {
                dialogWindow.SetupDialog(
                    Title,
                    Content,

                    IsDarkTheme,

                    Content_DisableMargin,

                    FirstButtonContent,
                    SecondButtonContent,

                    FirstButtonClickEvent,
                    SecondButtonClickEvent
                    );

                MainWindow mainwindow = Application.Current.MainWindow as MainWindow;

                dialogWindow.Owner = mainwindow;
                dialogWindow.MaxHeight = mainwindow.Height - 2;

                mainwindow.global_Dim();

                dialogWindow.Show();

                mainwindow.global_UnDim();
            }

            /// <summary>
            /// Closes the dialog.
            /// </summary>
            public void Close()
            {
                dialogWindow.Close();
            }
        }

        public class ContentDialog
        {
            private MainWindow mainwindow = Application.Current.MainWindow as MainWindow;

            public string Title = "Title";
            public string Text = "";

            public Grid ContentGrid = null;

            public string Button0_Text = "";
            public string Button1_Text = "OK";

            public RoutedEventHandler Button0_Click = null;
            public RoutedEventHandler Button1_Click = null;

            public User_controls.ContentDialogControl._Theme Theme = User_controls.ContentDialogControl._Theme.Auto;

            public void ShowDialog()
            {
                User_controls.ContentDialogControl dialog = new User_controls.ContentDialogControl();
                dialog.Title = Title;

                if (Text != null && ContentGrid == null)
                    dialog.Text = Text;
                else if (Text == null && ContentGrid != null)
                    dialog.ContentGrid = ContentGrid;
                else
                    dialog.Text = "Popups.cs: could not decide beetween Text or ContentGrid.";

                dialog.Theme = Theme;

                if (Button0_Text != "" & Button0_Text != null)
                    dialog.Button0_Text = Button0_Text;
                else
                    dialog.button0.Visibility = Visibility.Collapsed;

                if (Button1_Text != "" & Button1_Text != null)
                    dialog.Button1_Text = Button1_Text;
                else
                {
                    dialog.button1.Visibility = Visibility.Collapsed;
                    Grid.SetColumn(dialog.button0, 2);
                }

                // button click events

                // Button 0
                if (Button0_Click == null) // no click event, make it close the dialog down
                    dialog.button0.Click += (s, ev) => { mainwindow.CloseContentDialog(); };
                else
                    dialog.button0.Click += Button0_Click;

                // Button 1
                if (Button1_Click == null) // no click event, make it close the dialog down
                    dialog.button1.Click += (s, ev) => { mainwindow.CloseContentDialog(); };
                else
                    dialog.button1.Click += Button1_Click;

                // Show the dialog
                mainwindow.ShowContentDialog(dialog);
            }
        }

    }
}
