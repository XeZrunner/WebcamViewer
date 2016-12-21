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
using System.Windows.Threading;

namespace WebcamViewer.Updates.Updates.User_controls
{
    public partial class settingsPage_ToggleSwitchButton : UserControl
    {
        public settingsPage_ToggleSwitchButton()
        {
            InitializeComponent();
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //FrameworkElement _parent = Parent as FrameworkElement;
            //this.MaxWidth = _parent.ActualWidth;

            //var s_WidthHeight = (EasingDoubleKeyFrame)this.Resources["s_WidthHeightKeyFrame"];
            var s_Margin = (EasingThicknessKeyFrame)this.Resources["s_MarginKeyFrame"];

            //s_WidthHeight.Value = this.ActualWidth * 2;
            s_Margin.Value = new Thickness(-this.ActualWidth);

            double textGridMaxWidth = 0;
            if (IsToggleButton == true)
                textGridMaxWidth = this.ActualWidth - 130;
            else
                textGridMaxWidth = this.ActualWidth - 30;

            //textGrid.MaxWidth = textGridMaxWidth;
        }

        bool _IsToggleButton = true;

        int _Theme;

        public event RoutedEventHandler Click;

        [Description("Title of the button"), Category("Common")]
        public string Title
        {
            get { return titleTextBlock.Text; }
            set { titleTextBlock.Text = value.ToUpper(); }
        }

        [Description("Description of the button"), Category("Common")]
        public string Description
        {
            get { return descriptionTextBlock.Text; }
            set { descriptionTextBlock.Text = value; }
        }

        [Description("Active state of the toggle"), Category("Common")]
        public bool IsActive
        {
            get { return toggle.IsActive; }
            set { toggle.IsActive = value; }
        }

        [Description("Determines whether to display the toggle"), Category("Common")]
        public bool IsToggleButton
        {
            get { return _IsToggleButton; }
            set
            {
                _IsToggleButton = value;

                if (_IsToggleButton == true)
                {
                    toggle.Visibility = Visibility.Visible;
                    contentGrid_columnDefinition1.Width = new GridLength(100);
                }
                else
                {
                    toggle.Visibility = Visibility.Collapsed;
                    contentGrid_columnDefinition1.Width = new GridLength(0);
                }
            }
        }

        [Description("The grid on the right side (only works with IsToggleButton being false, and only with one Grid)"), Category("Common")]
        public Grid RightSideGrid
        {
            get { return (Grid)rightSide_GridContainer.Children[0]; }
            set
            {
                if (_IsToggleButton == false)
                {
                    if (value != null)
                    {
                        rightSide_GridContainer.Visibility = Visibility.Visible;

                        rightSide_GridContainer.Children.Clear();
                        rightSide_GridContainer.Children.Add(value);

                        contentGrid_columnDefinition1.Width = new GridLength(100);
                    }
                    else
                    {
                        rightSide_GridContainer.Visibility = Visibility.Collapsed;

                        rightSide_GridContainer.Children.Clear();

                        contentGrid_columnDefinition1.Width = new GridLength(0);
                    }
                }
            }
        }

        /// <summary>
        /// Force the theme of the button.
        /// 0 = light, 1 = dark
        /// </summary>
        [Description("Force the theme of the button"), Category("Appearance")]
        public int Theme
        {
            get { return _Theme; }
            set
            {
                _Theme = value;

                if (_Theme == 0)
                {
                    // light theme
                    titleTextBlock.Foreground = Application.Current.Resources["settingsPage_Light_foregroundText"] as SolidColorBrush;
                    descriptionTextBlock.Foreground = Application.Current.Resources["settingsPage_Light_foregroundSecondary2"] as SolidColorBrush;
                }
                else if (_Theme == 1)
                {
                    // dark theme
                    titleTextBlock.Foreground = Application.Current.Resources["settingsPage_Dark_foregroundText"] as SolidColorBrush;
                    descriptionTextBlock.Foreground = Application.Current.Resources["settingsPage_Dark_foregroundSecondary2"] as SolidColorBrush;
                }
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (_IsToggleButton)
            {
                toggle.DoAnimation = true;
                this.IsActive = !this.IsActive;
            }

            //bubble the event up to the parent
            if (this.Click != null)
                this.Click(this, e);

        }
    }
}