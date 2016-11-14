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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WebcamViewer.User_controls
{
    public partial class ArcProgress : UserControl
    {
        public ArcProgress()
        {
            InitializeComponent();

            s = FindResource("ArcAnim") as Storyboard;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_IsActive == true)
                StartAnim();
            else
                StopAnim();
        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.IsVisible == true)
            {
                if (_IsActive == true)
                    StartAnim();
                else
                    StopAnim();
            }
            else
            {
                StopAnim();
            }
        }

        private bool _IsActive = true;

        Storyboard s;

        [Description("The active state of the progress animation"), Category("Common")]
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
                        StartAnim();
                    }
                    else
                    {
                        StopAnim();
                    }
                }
            }
        }

        [Description("Color"), Category("Brush")]
        public SolidColorBrush Color
        {
            get { return arc.Stroke as SolidColorBrush; }
            set { arc.Stroke = value; }
        }

        private void StartAnim()
        {
            arc.Visibility = Visibility.Visible;
            s.Begin();
        }

        private void StopAnim()
        {
            s.Stop();
            arc.Visibility = Visibility.Hidden;
        }
    }
}
