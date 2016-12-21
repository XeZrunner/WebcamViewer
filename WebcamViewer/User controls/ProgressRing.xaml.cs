using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Threading;

namespace WebcamViewer.Updates.Updates.User_controls
{
    public partial class ProgressRing : UserControl
    {
        public ProgressRing()
        {
            InitializeComponent();

            mainTimer.Interval = TimeSpan.FromMilliseconds(_Framerate); //30 FPS?
            mainTimer.Tick += MainTimer_Tick;
        }

        #region Events

        private void usercontrol_Loaded(object sender, RoutedEventArgs e)
        {
            if (_IsActive == true)
                StartRing();
            else
                StopRing();
        }

        private void usercontrol_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.IsVisible == true)
            {
                if (_IsActive == true)
                    StartRing();
                else
                    StopRing();
            }
            else
            {
                StopRing();
            }
        }

        #endregion

        private bool _IsActive = true;

        DispatcherTimer mainTimer = new DispatcherTimer();

        private double _Framerate = 30;

        private char _char; // ue051 - ue0cb

        [Description("The active state of the progressring"), Category("Common")]
        public bool IsActive
        {
            get { return _IsActive; }
            set
            {
                _IsActive = value;

                if (this.Visibility == Visibility.Visible)
                {
                    if (_IsActive == true)
                    {
                        StartRing();
                    }
                    else
                    {
                        StopRing();
                    }
                }
            }
        }

        [Description("The size of the progressring font"), Category("Common")]
        public double RingFontSize
        {
            get { return ring.FontSize; }
            set { ring.FontSize = value; }
        }

        [Description("The framerate of the progressring"), Category("Common")]
        public double Framerate
        {
            get { return _Framerate; }
            set { _Framerate = value; }
        }

        private void MainTimer_Tick(object sender, EventArgs e)
        {
            _char++;
            ring.Content = _char.ToString();

            if (_char == "\ue0cb"[0]) // reached the final character
            {
                mainTimer.Stop();

                DispatcherTimer endtimer = new DispatcherTimer();
                endtimer.Interval = TimeSpan.FromSeconds(.5);
                endtimer.Tick += (s, ev) => 
                {
                    endtimer.Stop();
                    _char = "\ue051"[0];
                    mainTimer.Start();
                };
                endtimer.Start();
            }
        }

        private void StartRing()
        {
            // reset character
            _char = "\ue051"[0];

            mainTimer.Start();
        }

        private void StopRing()
        {
            mainTimer.Stop();

            ring.Content = "";
            _char = "\ue051"[0];
        }
    }
}
