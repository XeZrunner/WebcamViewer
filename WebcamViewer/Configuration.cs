using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WebcamViewer
{
    // Default configuration file: https://raw.githubusercontent.com/XeZrunner/WebcamViewer/gh-pages/DefaultConfiguration.webcamviewer_configfile

    public class Configuration
    {
        public string defaultconfig_file_URL = "https://raw.githubusercontent.com/XeZrunner/WebcamViewer/gh-pages/DefaultConfiguration.webcamviewer_configfile";

        List<string> PropertyNames = new List<string>();
        List<string> PropertyValues = new List<string>();

        Properties.Settings Settings = Properties.Settings.Default;

        #region Dialogs

        void TextMessageDialog(string Title, string Content, bool DarkMode = false)
        {
            var dialog = new Popups.MessageDialog();

            dialog.Title = Title;
            dialog.Content = Content;

            dialog.ShowDialog();
        }

        #endregion

        public async void ReadConfigurationFile(string configfile_path, bool applyConfig = false)
        {
            Stream stream = null;
            Popups.MessageDialog progressdialog = new Popups.MessageDialog();

            if (!(configfile_path.Contains("http") || configfile_path.Contains("ftp")))
            {
                try
                {
                    stream = File.OpenRead(configfile_path);
                }
                catch (Exception ex)
                {
                    TextMessageDialog("Cannot read the configuration file", "Error: " + ex.Message);
                }
            }
            else
            {
                WebClient client = new WebClient();

                Uri configfile_path_Uri = new Uri(configfile_path);

                #region Progress dialog
                MahApps.Metro.Controls.ProgressRing ring = new MahApps.Metro.Controls.ProgressRing();
                ring.Width = 40; ring.Height = 40; ring.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black); ring.IsLarge = false; ring.Margin = new Thickness(0, 10, 0, 0); ring.EllipseDiameterScale = 0.9;

                progressdialog.Title = "Reading configuration file...";
                progressdialog.Content = ring;

                progressdialog.IsDarkTheme = true;

                progressdialog.FirstButtonContent = "";
                #endregion

                progressdialog.Show();

                try
                {
                    stream = await client.OpenReadTaskAsync(configfile_path_Uri);
                }
                catch (Exception ex)
                {
                    TextMessageDialog("Could not grab the configuration file", "Make sure you have a working internet connection, or that the file exists at the URL you provided, and try again.\nError: " + ex.Message);
                    return;
                }
            }

            StreamReader reader = new StreamReader(stream);

            string configfile_Content = await reader.ReadToEndAsync();

            string[] configfile_Content_linesArray = configfile_Content.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

            progressdialog.Close();

#if DEBUG
            TextMessageDialog("", configfile_Content, true);
#endif

            List<string> PropertyNames = new List<string>();
            List<string[]> PropertyValues = new List<string[]>();

            int countofItemsInsidePropertyFinalAdd = 0;

            int lineNumber = 0;
            foreach (string line in configfile_Content_linesArray)
            {
                if (PropertyNames.Count > 0 & PropertyValues.Count > 0)
                    if (line.Contains(PropertyNames[lineNumber]) || line.Contains(String.Join("\n", PropertyValues[lineNumber])))
                        break;

                if (line.StartsWith("\"")) // it's a property name, begings with "
                {
                    if (!line.EndsWith(";"))
                    {
                        PropertyNames.Add(line.Substring(1, line.Length - 2));
                    }
                    else
                    {
                        int charcounter = 0;
                        int endofvalue = 0;
                        foreach (char c in line)
                        {
                            if (c == ';')
                            {
                                endofvalue = charcounter;
                                charcounter = 0;
                                charcounter++;
                            }
                            else
                                charcounter++;
                        }

                        PropertyNames.Add(line.Substring(1, line.Length - charcounter - 2));
                    }
                }

                if (line.StartsWith("{")) // it's first identifier of a value, begins with {
                {
                    if (!line.EndsWith(";"))
                    {
                        int linecounter = 0;
                        int startPoint = 0;
                        int endPoint = 0;

                        foreach (string line2 in configfile_Content_linesArray)
                        {
                            if (!line.StartsWith("{"))
                            {
                                linecounter++;
                            }
                            else
                            {
                                if (configfile_Content_linesArray[linecounter + 1].StartsWith("  ")) // two spaces
                                {
                                    linecounter++;
                                    startPoint = linecounter;
                                }
                                else if ((configfile_Content_linesArray[linecounter + 1]).StartsWith("}"))
                                {
                                    linecounter++;
                                    endPoint = linecounter;
                                }
                            }

                        }

                        string[] finalToAdd = new string[endPoint - startPoint];

                        if (!configfile_Content_linesArray[lineNumber + 1].EndsWith(";"))
                        {
                            for (int i = startPoint; i < endPoint; i++)
                            {
                                finalToAdd[i] = configfile_Content_linesArray[i];
                            }
                        }

                        PropertyValues.Add(finalToAdd);
                    }
                    else
                    {
                        bool foundstartofvalue = false;
                        int startofvalue = 0;
                        int charcounter = 0;
                        int endofvalue = 0;

                        foreach (char c in configfile_Content_linesArray[lineNumber + 1])
                        {
                            if (foundstartofvalue == false)
                            {
                                if (c != ' ')
                                {
                                    charcounter++;
                                    startofvalue = charcounter;
                                    foundstartofvalue = true;
                                }
                                else
                                    charcounter++;
                            }
                            if (c == ';')
                            {
                                endofvalue = charcounter - startofvalue;
                            }
                        }

                        int linecounter = 0;

                        int startPoint = 0;
                        int endPoint = 0;

                        foreach (string line2 in configfile_Content_linesArray)
                        {
                            if (!line.StartsWith("{"))
                            {
                                linecounter++;
                            }
                            else
                            {
                                if (configfile_Content_linesArray[linecounter + 1].StartsWith("  ")) // two spaces
                                {
                                    linecounter++;
                                    startPoint = linecounter;
                                }
                                else if ((configfile_Content_linesArray[linecounter + 1]).StartsWith("}"))
                                {
                                    linecounter++;
                                    endPoint = linecounter;
                                }
                            }


                            string[] finalToAdd = new string[endPoint - startPoint];

                            if (!configfile_Content_linesArray[lineNumber + 1].EndsWith(";"))
                            {
                                for (int i = startPoint; i < endPoint; i++)
                                {
                                    finalToAdd[i] = configfile_Content_linesArray[i].Substring(startofvalue, endofvalue);
                                }
                            }

                            PropertyValues.Add(finalToAdd);

                            countofItemsInsidePropertyFinalAdd = finalToAdd.Length;
                        }
                    }

                    lineNumber += countofItemsInsidePropertyFinalAdd;

                }
            }

#if DEBUG
            string propertyvalues_string = "";

            foreach (string[] array in PropertyValues)
            {
                propertyvalues_string += String.Join("\n", array);
            }

            TextMessageDialog("", String.Join("\n", PropertyNames.ToArray()) + "\n\n----------\n\n" + String.Join("\n", propertyvalues_string, true));
#endif

            if (applyConfig)
                ApplyConfigFileSettings();

        }

        public void ApplyDefaultConfigurationFile()
        {
            ReadConfigurationFile(defaultconfig_file_URL, true);
        }

        void ApplyConfigFileSettings()
        {
            Popups.MessageDialog confirmDialog = new Popups.MessageDialog();

            confirmDialog.Title = "Are you sure you want to apply this configuration file?";
            confirmDialog.Content = "Make sure you backup your current configuration, there's no way to get your current configuration back once you continue.";

            confirmDialog.FirstButtonContent = "Continue";
            confirmDialog.SecondButtonContent = "Cancel";

            if (confirmDialog.ShowDialogWithResult() == 0)
            {
                bool invalid_cameracustomizations = false;

                if (PropertyNames.Contains("camera_names") || PropertyNames.Contains("camera_urls"))
                {
                    if (PropertyNames.Contains("camera_names") & !PropertyNames.Contains("camera_urls") || !PropertyNames.Contains("camera_names") & PropertyNames.Contains("camera_urls"))
                    {
                        Popups.MessageDialog dlg = new Popups.MessageDialog();
                        dlg.Title = "The camera customizations are invalid";
                        dlg.Content = "Check the configuration file and try again.\nShould we apply the rest of the settings? (if there are any)";

                        dlg.FirstButtonContent = "Yes, apply them.";
                        dlg.SecondButtonContent = "Cancel";

                        if (dlg.ShowDialogWithResult() == 1)
                            return;
                        else
                            invalid_cameracustomizations = true;
                    }
                }

                if (PropertyNames.Contains("camera_names") & PropertyNames.Contains("camera_urls"))
                {
                    string[] stringSeparators = new string[] { "\n" };

                    string s_names = PropertyValues[PropertyNames.IndexOf("camera_names")];
                    string[] sArray_names = s_names.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

                    string s_urls = PropertyValues[PropertyNames.IndexOf("camera_urls")];
                    string[] sArray_urls = s_urls.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

                    if (sArray_names.Length != sArray_urls.Length)
                    {
                        Popups.MessageDialog dlg = new Popups.MessageDialog();
                        dlg.Title = "The camera customizations are invalid";
                        dlg.Content = "There are more or less camera entries in one of the sections. Check the configuration file and try again.\nShould we apply the rest of the settings? (if there are any)";

                        dlg.FirstButtonContent = "Yes, apply them.";
                        dlg.SecondButtonContent = "Cancel";

                        if (dlg.ShowDialogWithResult() == 1)
                            return;
                        else
                            invalid_cameracustomizations = true;
                    }
                }

                int counter = 0;
                foreach (string name in PropertyNames)
                {
                    if (invalid_cameracustomizations)
                    {
                        if (name == "camera_names" || name == "camera_urls")
                        {
                            counter++;
                            return;
                        }
                    }

                    Settings[name] = PropertyValues[counter];
                    counter++;
                }

                Settings.Save();
            }
            else
                return;
        }

        void ResetOfficialCameraCustomizations()
        {
            ReadConfigurationFile(defaultconfig_file_URL);

            if (PropertyNames.Contains("camera_names") & PropertyNames.Contains("camera_urls"))
            {
                foreach (string propertyname in PropertyNames)
                {
                    if (propertyname == "camera_names")
                        Settings["camera_names"] = PropertyValues[PropertyNames.IndexOf("camera_names")];

                    if (propertyname == "camera_urls")
                        Settings["camera_urls"] = PropertyValues[PropertyNames.IndexOf("camera_urls")];
                }
            }
        }

        public void DefaultConfig_Heartbeat()
        {
            // compares the values from the read stuff from ^^ up there to the local config and informs user through titlebar info button whether they'd like to update the changed values
        }
    }
}
