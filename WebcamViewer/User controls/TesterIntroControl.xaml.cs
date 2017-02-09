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

namespace WebcamViewer.User_controls
{
    /// <summary>
    /// Interaction logic for TesterIntroControl.xaml
    /// </summary>
    public partial class TesterIntroControl : UserControl
    {
        public TesterIntroControl()
        {
            InitializeComponent();

            buildID_block.Text = Properties.Settings.Default.buildid;
            channel_block.Text = Properties.Settings.Default.app_prereleasechannel;
            debugmode_block.Text = Properties.Settings.Default.app_debugmode;
        }
    }
}
