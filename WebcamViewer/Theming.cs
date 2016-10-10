using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace WebcamViewer
{
    class Theming
    {
        // Configuration.Settings Settings = new Configuration.Settings();

        public _Theme Theme = new Theming._Theme();
        public _AccentColor AccentColor = new Theming._AccentColor();

        public class _AccentColor
        {
            Configuration.Settings Settings = new Configuration.Settings();
            public enum ColorType : int
            {
                Dark = 0,
                Light = 1
            }
            
            public string[] AccentColorNames = { "Orange", "Red", "Green", "Blue", "Gray" };

            /// <summary>
            /// Returns the SolidColorBrush for an accent color.
            /// </summary>
            /// <param name="accent">The accent color to get</param>
            /// <param name="type">The type of accent color to get. 0 = dark, 1 = light</param>
            public SolidColorBrush GetAccentColor(int accent, int type)
            {
                string[] typeToName = { "dark", "light" };
                return Application.Current.Resources["accentcolor_" + typeToName[type] + accent] as SolidColorBrush;
            }
            /// <summary>
            /// Returns the SolidColorBrush for an accent color using the enum ColorType.
            /// </summary>
            /// <param name="accent">The accent color to get</param>
            /// <param name="type">The type of accent color to get. ColorType.Dark = Dark ; ColorType.Light = Light ; pretty obvious nah ?</param>
            public SolidColorBrush GetAccentColor(int accent, ColorType type)
            {
                string[] typeToName = { "dark", "light" };
                return Application.Current.Resources["accentcolor_" + typeToName[(int)type] + accent] as SolidColorBrush;
                
            }

            /// <summary>
            /// Sets the accent color.
            /// </summary>
            /// <param name="accent">The accent color to set.</param>
            /// <param name="permanent">Whether to save it in user settings.</param>
            public void SetAccentColor(int accent, bool permanent = false)
            {
                // Set the resources.
                Application.Current.Resources["accentcolor_dark"] = Application.Current.Resources["accentcolor_dark" + accent];
                Application.Current.Resources["accentcolor_light"] = Application.Current.Resources["accentcolor_light" + accent];

                // Set the user setting
                if (permanent)
                {
                    Settings.SetSetting("ui_accent", accent, true);
                }
            }
        }

        public class _Theme
        {
            Configuration.Settings Settings = new Configuration.Settings();

            public string[] ThemeNames = { "Light", "Dark" };

            /// <summary>
            /// Returns the current theme being used.
            /// </summary>
            public int GetCurrentTheme()
            {
                return Properties.Settings.Default.ui_theme;
            }

            /// <summary>
            /// Sets the theme.
            /// </summary>
            /// <param name="theme">The theme to set. Duh.</param>
            /// <param name="permanent">Whether to save it permanently into user settings.</param>
            public void SetTheme(int theme, bool permanent = false)
            {
                Application.Current.Resources["webcamPage_Foreground"] = Application.Current.Resources[string.Format("webcamPage_{0}_Foreground", ThemeNames[theme])];
                Application.Current.Resources["webcamPage_menuBackground"] = Application.Current.Resources[string.Format("webcamPage_{0}_menuBackground", ThemeNames[theme])];
                Application.Current.Resources["webcamPage_menuBackgroundSecondary"] = Application.Current.Resources[string.Format("webcamPage_{0}_menuBackgroundSecondary", ThemeNames[theme])];

                Application.Current.Resources["settingsPage_background"] = Application.Current.Resources[string.Format("settingsPage_{0}_background", ThemeNames[theme])];
                Application.Current.Resources["settingsPage_backgroundSecondary"] = Application.Current.Resources[string.Format("settingsPage_{0}_backgroundSecondary", ThemeNames[theme]) ];
                Application.Current.Resources["settingsPage_backgroundSecondary2"] = Application.Current.Resources[string.Format("settingsPage_{0}_backgroundSecondary2", ThemeNames[theme]) ];
                Application.Current.Resources["settingsPage_backgroundSecondary3"] = Application.Current.Resources[string.Format("settingsPage_{0}_backgroundSecondary3", ThemeNames[theme]) ];
                Application.Current.Resources["settingsPage_backgroundWebcamItemEditor"] = Application.Current.Resources[string.Format("settingsPage_{0}_backgroundWebcamItemEditor", ThemeNames[theme]) ];

                Application.Current.Resources["settingsPage_foregroundText"] = Application.Current.Resources[string.Format("settingsPage_{0}_foregroundText", ThemeNames[theme]) ];
                Application.Current.Resources["settingsPage_foregroundSecondary"] = Application.Current.Resources[string.Format("settingsPage_{0}_foregroundSecondary", ThemeNames[theme]) ];
                Application.Current.Resources["settingsPage_foregroundSecondary2"] = Application.Current.Resources[string.Format("settingsPage_{0}_foregroundSecondary2", ThemeNames[theme])];
                Application.Current.Resources["settingsPage_foregroundSecondary3"] = Application.Current.Resources[string.Format("settingsPage_{0}_foregroundSecondary3", ThemeNames[theme]) ];

                Application.Current.Resources["MessageDialog_Background"] = Application.Current.Resources[string.Format("MessageDialog_{0}_Background", ThemeNames[theme]) ];
                Application.Current.Resources["MessageDialog_ForegroundText"] = Application.Current.Resources[string.Format("MessageDialog_{0}_ForegroundText", ThemeNames[theme])];
                Application.Current.Resources["MessageDialog_ForegroundSecondary"] = Application.Current.Resources[string.Format("MessageDialog_{0}_ForegroundSecondary", ThemeNames[theme])];

                Application.Current.Resources["MessageDialog_FullWidth_Background"] = Application.Current.Resources[string.Format("MessageDialog_FullWidth_{0}_Background", ThemeNames[theme])];
                Application.Current.Resources["MessageDialog_FullWidth_ForegroundText"] = Application.Current.Resources[string.Format("MessageDialog_FullWidth_{0}_ForegroundText", ThemeNames[theme])];
                Application.Current.Resources["MessageDialog_FullWidth_ForegroundSecondary"] = Application.Current.Resources[string.Format("MessageDialog_FullWidth_{0}_ForegroundSecondary", ThemeNames[theme])];

                // swap the accent colors
                if (theme != Properties.Settings.Default.ui_theme)
                {
                    SolidColorBrush prev_dark = Application.Current.Resources["accentcolor_dark"] as SolidColorBrush;
                    SolidColorBrush prev_light = Application.Current.Resources["accentcolor_light"] as SolidColorBrush;

                    Application.Current.Resources["accentcolor_light"] = prev_dark;
                    Application.Current.Resources["accentcolor_dark"] = prev_light;
                }

                Settings.SetSetting("ui_theme", theme, permanent);
            }
        }
    }
}
