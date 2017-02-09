using System;
using System.Collections.Generic;
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

namespace WebcamViewer.Pages.Home_page.Controls
{
    public partial class ProgressUI_BothSaveItem : UserControl
    {
        public ProgressUI_BothSaveItem()
        {
            InitializeComponent();
        }

        public event RoutedEventHandler Click;

        public enum State
        {
            Progress,
            Completed,
            Failed
        }

        private State _status;

        // e712 : PROGRESS
        // e73e : COMPLETED
        // ea83 : FAILED

        /// <summary>
        /// The state of the item
        /// </summary>
        public State Status
        {
            get { return _status; }
            set
            {
                _status = value;

                switch (_status)
                {
                    case State.Progress:
                        {
                            activeIndicator.Visibility = Visibility.Visible;
                            statusIcon.Content = "\ue712";
                            statusLabel.Content = "Preparing...";

                            break;
                        }
                    case State.Completed:
                        {
                            activeIndicator.Visibility = Visibility.Hidden;
                            statusIcon.Content = "\ue73e";
                            statusLabel.Content = "Completed!";

                            break;
                        }
                    case State.Failed:
                        {
                            activeIndicator.Visibility = Visibility.Visible;
                            activeIndicator.Fill = Brushes.Red;

                            statusIcon.Content = "\uea83";
                            statusLabel.Content = "Failed - click for more info";

                            break;
                        }
                }
            }
        }


    }
}
