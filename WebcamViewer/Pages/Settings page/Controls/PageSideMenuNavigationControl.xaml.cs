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
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using WebcamViewer.User_controls;

namespace WebcamViewer.Pages.Settings_page.Controls
{
    public partial class PageSideMenuNavigationControl : UserControl
    {
        public PageSideMenuNavigationControl()
        {
            InitializeComponent();
        }

        private void usercontrol_Loaded(object sender, RoutedEventArgs e)
        {
            if (GetItemsPanel() != null)
            {
                if (GetItemsPanel().Children.Count != 0)
                {
                    int counter = 0;
                    foreach (object btn in GetItemsPanel().Children)
                    {
                        if (btn.GetType() == (typeof(settingsPage_TabButton)))
                        {
                            settingsPage_TabButton button = btn as settingsPage_TabButton;

                            button.Click += Button_Click;
                            // assign id
                            button.Tag = counter;

                            counter++;
                        }
                    }
                }
            }
        }

        private StackPanel GetItemsPanel()
        {
            return sideMenuNavigation_ItemsStackPanel.Children[0] as StackPanel;
        }

        public StackPanel ItemsStackPanel
        {
            get { return GetItemsPanel(); }
            set
            {
                sideMenuNavigation_ItemsStackPanel.Children.Clear();
                sideMenuNavigation_ItemsStackPanel.Children.Add(value);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            settingsPage_TabButton sButton = sender as settingsPage_TabButton;

            // set ID
            currentSelectedID = (int)sButton.Tag;

            foreach (settingsPage_TabButton button in GetItemsPanel().Children)
            {
                if (button != sButton)
                    button.IsActive = false;
                else
                    button.IsActive = true;
            }

            // fire event
            SelectionChanged?.Invoke(sButton, new EventArgs());
        }

        public void AddItem(settingsPage_TabButton button)
        {
            GetItemsPanel().Children.Add(button);
        }

        public int? currentSelectedID;

        public event EventHandler SelectionChanged;
        
        /// <summary>
        /// Set the button active UI-only, no SelectionChanged event
        /// </summary>
        public void _SetButtonActiveState(int ID)
        {
            foreach (settingsPage_TabButton button in GetItemsPanel().Children)
            {
                if ((int)button.Tag != ID)
                    button.IsActive = false;
                else
                    button.IsActive = true;
            }
        }

        /// <summary>
        /// Go to the page specified, and fire the SelectionChanged event.
        /// </summary>
        public void SetActive(int ID)
        {
            if (ID >= GetItemsPanel().Children.Count - 1)
                throw new Exception("That ID is not!");
            else
            {
                foreach (settingsPage_TabButton button in GetItemsPanel().Children)
                {
                    if ((int)button.Tag == ID)
                    {
                        SelectionChanged(button, new RoutedEventArgs());
                        _SetButtonActiveState(ID);
                    }
                }
            }
        }
    }
}
