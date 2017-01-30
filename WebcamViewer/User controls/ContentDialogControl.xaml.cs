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

namespace WebcamViewer.User_controls
{
    public partial class ContentDialogControl : UserControl
    {
        public ContentDialogControl()
        {
            InitializeComponent();

            anim_In = FindResource("Anim_In") as Storyboard;
            anim_Out = FindResource("Anim_Out") as Storyboard;
        }

        private void usercontrol_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // animate in and out
            if (this.IsVisible)
                anim_In.Begin();
        }

        private MainWindow mainwindow = Application.Current.MainWindow as MainWindow;

        private Storyboard anim_In;
        private Storyboard anim_Out;

        private string m_Text = "Text";
        private _Theme m_Theme = _Theme.Auto;

        private bool _NoMarginDialog = false;

        private bool _wasBackgroundSet = false;
        private bool _wasForegroundSet = false;

        #region Enums
        public enum _Theme
        {
            Light,
            Dark,
            Auto
        }
        #endregion

        TextBlock textLabel = new TextBlock() { FontSize = 14, Margin = new Thickness(5), TextWrapping = TextWrapping.Wrap };

        [Description("The title of the ContentDialog."), Category("Common")]
        public string Title
        {
            get { return titleLabel.Content as string; }
            set
            {
                titleLabel.Content = value;

                if (value == "")
                    rowdefinition_titleLabel.Height = new GridLength(0, GridUnitType.Pixel);
                else
                    rowdefinition_titleLabel.Height = new GridLength(1, GridUnitType.Auto);
            }
        }

        [Description("Get or set the children of the ContentDialog's content grid."), Category("Common")]
        public UIElement ContentGrid
        {
            get
            {
                return contentGrid.Children[0] as Grid;
            }
            set
            {
                contentGrid.Children.Clear();
                contentGrid.Children.Add(value);
            }
        }

        [Description("The text of the ContentDialog."), Category("Common")]
        public string Text
        {
            get { return m_Text; }
            set
            {
                m_Text = value;

                contentGrid.Children.Clear();

                Grid textGrid = new Grid();

                textLabel.Text = m_Text;

                textGrid.Children.Add(textLabel);

                contentGrid.Children.Add(textGrid);

                contentGrid_ScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            }
        }

        [Description("Disable the top margin of the ContentDialog."), Category("Common")]
        public bool NoMarginDialog
        {
            get { return _NoMarginDialog; }
            set
            {
                _NoMarginDialog = value;

                if (_NoMarginDialog)
                {
                    rowdefinition_titleLabel.Height = new GridLength(0, GridUnitType.Pixel);
                    contentGrid.Margin = new Thickness(0);
                }
                else
                {
                    rowdefinition_titleLabel.Height = new GridLength(1, GridUnitType.Auto);
                    contentGrid.Margin = new Thickness(24, 0, 24, 0);
                }
            }
        }

        [Description("The left-side button's text"), Category("Common")]
        public string Button0_Text
        {
            get { return button0.Text; }
            set { button0.Text = value; }
        }

        [Description("The right-side button's text"), Category("Common")]
        public string Button1_Text
        {
            get { return button1.Text; }
            set { button1.Text = value; }
        }

        #region DependencyProperties (Colors)

        #region BackgroundBrush

        static FrameworkPropertyMetadata BackgroundBrushPropertyMetaData = new FrameworkPropertyMetadata(Application.Current.Resources["settingsPage_background"] as Brush, new PropertyChangedCallback(BackgroundBrushProperty_Changed));

        static void BackgroundBrushProperty_Changed(DependencyObject dobj, DependencyPropertyChangedEventArgs e)
        {
            ContentDialogControl main = dobj as ContentDialogControl;

            main.contentdialog_Border.Background = e.NewValue as SolidColorBrush;

            main._wasBackgroundSet = true;
        }

        public static readonly DependencyProperty BackgroundBrushProperty = DependencyProperty.Register("BackgroundBrush", typeof(Brush), typeof(ContentDialogControl), BackgroundBrushPropertyMetaData);

        /// <summary>
        /// The background color.
        /// </summary>
        /// 
        [Description("The background brush of the ContentDialog"), Category("Brush")]
        public Brush BackgroundBrush
        {
            get { return GetValue(BackgroundBrushProperty) as Brush; }
            set
            {
                SetValue(BackgroundBrushProperty, value);
            }
        }

        #endregion

        #region ForegroundBrush

        static FrameworkPropertyMetadata ForegroundBrushPropertyMetaData = new FrameworkPropertyMetadata(Application.Current.Resources["settingsPage_foregroundText"] as Brush, new PropertyChangedCallback(ForegroundBrushProperty_Changed));

        static void ForegroundBrushProperty_Changed(DependencyObject dobj, DependencyPropertyChangedEventArgs e)
        {
            ContentDialogControl main = dobj as ContentDialogControl;

            main.titleLabel.Foreground = e.NewValue as SolidColorBrush;
            main.textLabel.Foreground = e.NewValue as SolidColorBrush;

            main._wasForegroundSet = true;
        }

        public static readonly DependencyProperty ForegroundBrushProperty = DependencyProperty.Register("ForegroundBrush", typeof(Brush), typeof(ContentDialogControl), ForegroundBrushPropertyMetaData);

        /// <summary>
        /// The foreground color.
        /// </summary>
        /// 
        [Description("The foreground brush of the ContentDialog"), Category("Brush")]
        public Brush ForegroundBrush
        {
            get { return GetValue(ForegroundBrushProperty) as Brush; }
            set
            {
                SetValue(ForegroundBrushProperty, value);
            }
        }

        #endregion

        #region DialogBorderBrush

        static FrameworkPropertyMetadata DialogBorderBrushPropertyMetaData = new FrameworkPropertyMetadata(Application.Current.Resources["accentcolor_dark"] as SolidColorBrush, new PropertyChangedCallback(DialogBorderBrushProperty_Changed));

        static void DialogBorderBrushProperty_Changed(DependencyObject dobj, DependencyPropertyChangedEventArgs e)
        {
            ContentDialogControl main = dobj as ContentDialogControl;

            main.contentdialog_Border.BorderBrush = e.NewValue as SolidColorBrush;
        }

        public static readonly DependencyProperty DialogBorderBrushProperty = DependencyProperty.Register("DialogBorderBrush", typeof(SolidColorBrush), typeof(ContentDialogControl), DialogBorderBrushPropertyMetaData);

        /// <summary>
        /// The border color.
        /// </summary>
        /// 
        [Description("The border brush of the ContentDialog"), Category("Brush")]
        public SolidColorBrush DialogBorderBrush
        {
            get { return GetValue(DialogBorderBrushProperty) as SolidColorBrush; }
            set
            {
                SetValue(DialogBorderBrushProperty, value);
            }
        }

        #endregion

        #endregion

        public _Theme Theme
        {
            get { return m_Theme; }
            set
            {
                m_Theme = value;

                UpdateTheming();
            }
        }

        private void UpdateTheming()
        {
            if (!_wasBackgroundSet)
            {
                if (m_Theme == _Theme.Auto)
                    contentdialog_Border.SetResourceReference(BackgroundProperty, "settingsPage_background");
                else if (m_Theme == _Theme.Light)
                    contentdialog_Border.SetResourceReference(BackgroundProperty, "settingsPage_Light_background");
                else
                    contentdialog_Border.SetResourceReference(BackgroundProperty, "settingsPage_Dark_background");
            }

            if (!_wasForegroundSet)
            {
                if (m_Theme == _Theme.Auto)
                {
                    titleLabel.SetResourceReference(ForegroundProperty, "settingsPage_foregroundText");
                    textLabel.SetResourceReference(ForegroundProperty, "settingsPage_foregroundText");
                }
                else if (m_Theme == _Theme.Light)
                {
                    titleLabel.SetResourceReference(ForegroundProperty, "settingsPage_Light_foregroundText");
                    textLabel.SetResourceReference(ForegroundProperty, "settingsPage_Light_foregroundText");
                }
                else
                {
                    titleLabel.SetResourceReference(ForegroundProperty, "settingsPage_Dark_foregroundText");
                    textLabel.SetResourceReference(ForegroundProperty, "settingsPage_Dark_foregroundText");
                }
            }

            // Buttons
            // This could be implemented soo much more intelligently, but this is was made in a rush
            if (m_Theme == _Theme.Auto)
            {
                button0.SetResourceReference(BackgroundProperty, "settingsPage_backgroundSecondary3");
                button1.SetResourceReference(BackgroundProperty, "settingsPage_backgroundSecondary3");

                button0.SetResourceReference(ForegroundProperty, "settingsPage_foregroundText");
                button1.SetResourceReference(ForegroundProperty, "settingsPage_foregroundText");

                button0.Theme = settingsPage_NormalButton._Theme.Auto;
                button1.Theme = settingsPage_NormalButton._Theme.Auto;
            }
            else if (m_Theme == _Theme.Light)
            {
                button0.SetResourceReference(BackgroundProperty, "settingsPage_Light_backgroundSecondary3");
                button1.SetResourceReference(BackgroundProperty, "settingsPage_Light_backgroundSecondary3");

                button0.SetResourceReference(ForegroundProperty, "settingsPage_Light_foregroundText");
                button1.SetResourceReference(ForegroundProperty, "settingsPage_Light_foregroundText");

                button0.Theme = settingsPage_NormalButton._Theme.Light;
                button1.Theme = settingsPage_NormalButton._Theme.Light;
            }
            else
            {
                button0.SetResourceReference(BackgroundProperty, "settingsPage_Dark_backgroundSecondary3");
                button1.SetResourceReference(BackgroundProperty, "settingsPage_Dark_backgroundSecondary3");

                button0.SetResourceReference(ForegroundProperty, "settingsPage_Dark_foregroundText");
                button1.SetResourceReference(ForegroundProperty, "settingsPage_Dark_foregroundText");

                button0.Theme = settingsPage_NormalButton._Theme.Dark;
                button1.Theme = settingsPage_NormalButton._Theme.Dark;
            }

        }

        public void FadeOut()
        {
            anim_Out.Begin();
        }

        #region Action buttons

        private void button0_Click(object sender, RoutedEventArgs e)
        {

        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {

        }

        #endregion
    }
}
