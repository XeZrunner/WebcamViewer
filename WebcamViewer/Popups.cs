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

        public class MessageDialog
        {
            MessagedialogWindow dialogWindow = new MessagedialogWindow();

            public string Title = "";
            public object Content = new Control();

            public bool IsDarkTheme = false;

            public bool IsFirstButtonEnabled = true;
            public bool IsSecondButtonEnabled = true;
            public bool IsThirdButtonEnabled = true;

            public string FirstButtonContent = "OK";
            public string SecondButtonContent = "";
            public string ThirdButtonContent = "";

            public RoutedEventHandler FirstButtonClickEvent;
            public RoutedEventHandler SecondButtonClickEvent;
            public RoutedEventHandler ThirdButtonClickEvent;

            public void ShowDialog()
            {
                dialogWindow.SetupDialog(
                    Title,
                    Content,

                    IsDarkTheme,

                    FirstButtonContent,
                    SecondButtonContent,
                    ThirdButtonContent,

                    IsFirstButtonEnabled,
                    IsSecondButtonEnabled,
                    IsThirdButtonEnabled,

                    FirstButtonClickEvent,
                    SecondButtonClickEvent,
                    ThirdButtonClickEvent
                    );

                dialogWindow.Owner = Application.Current.MainWindow;
                dialogWindow.ShowDialog();

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

            void ResultDialog_ThirdButtonClickEvent(object sender, RoutedEventArgs e)
            {
                ResultDialog_result = 2;
                dialogWindow.Close();
            }

            public int ShowDialogWithResult()
            {
                dialogWindow.SetupDialog(
                    Title,
                    Content,

                    IsDarkTheme,

                    FirstButtonContent,
                    SecondButtonContent,
                    ThirdButtonContent,

                    IsFirstButtonEnabled,
                    IsSecondButtonEnabled,
                    IsThirdButtonEnabled,

                    ResultDialog_FirstButtonClickEvent,
                    ResultDialog_SecondButtonClickEvent,
                    ResultDialog_ThirdButtonClickEvent
                    );

                dialogWindow.Owner = Application.Current.MainWindow;
                dialogWindow.ShowDialog();

                return ResultDialog_result;
            }

            public void Show()
            {
                dialogWindow.SetupDialog(
                    Title,
                    Content,

                    IsDarkTheme,

                    FirstButtonContent,
                    SecondButtonContent,
                    ThirdButtonContent,

                    IsFirstButtonEnabled,
                    IsSecondButtonEnabled,
                    IsThirdButtonEnabled,

                    FirstButtonClickEvent,
                    SecondButtonClickEvent,
                    ThirdButtonClickEvent
                    );

                dialogWindow.Owner = Application.Current.MainWindow;
                dialogWindow.Show();
            }

            public void Close()
            {
                dialogWindow.Close();
            }
        }

    }
}
