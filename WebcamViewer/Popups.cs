using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            MessagedialogWindow dialogWindow = new MessagedialogWindow();

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
                ResultDialog_result = 1;
                dialogWindow.Close();
            }

            void ResultDialog_SecondButtonClickEvent(object sender, RoutedEventArgs e)
            {
                ResultDialog_result = 0;
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

    }
}
