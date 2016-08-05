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
    // Default configuration file: https://raw.githubusercontent.com/XeZrunner/WebcamViewer/gh-pages/DefaultConfiguration.txt

    public class Configuration
    {
        public string defaultconfig_file_URL = "https://raw.githubusercontent.com/XeZrunner/WebcamViewer/gh-pages/DefaultConfiguration.txt?dummy=" + DateTime.Now.Hour + DateTime.Now.Minute + DateTime.Now.Second + DateTime.Now.Millisecond;

        Properties.Settings Settings = Properties.Settings.Default;

        bool showDebugDialogs = false;

        #region Dialogs

        void TextMessageDialog(string Title, string Content, bool DarkMode = false)
        {
            var dialog = new Popups.MessageDialog();

            dialog.Title = Title;
            dialog.Content = Content;

            dialog.ShowDialog();
        }

        #endregion

        List<string> PropertyNames = new List<string>();
        List<string[]> PropertyValues = new List<string[]>();

        public void ReadConfigurationFile(string configfile_path, bool applyConfig)
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

                try
                {
                    stream = client.OpenRead(configfile_path_Uri);
                }
                catch (Exception ex)
                {
                    TextMessageDialog("Could not grab the configuration file", "Make sure you have a working internet connection, or that the file exists at the URL you provided, and try again.\nError: " + ex.Message);
                    return;
                }
            }

            StreamReader reader = new StreamReader(stream);

            string configfile_Content = reader.ReadToEnd();

            string[] configfile_Content_linesArray = configfile_Content.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

            progressdialog.Close();

            if (showDebugDialogs)
                TextMessageDialog("", configfile_Content, true);

            bool shouldContinue = false;

            int lineNumber = 0;
            int continueFrom_LineNumber = 0;

            foreach (string line in configfile_Content_linesArray)
            {
                if (shouldContinue)
                {
                    if (lineNumber != continueFrom_LineNumber)
                    {
                        // linenumber++ at the end
                    }
                    else
                        shouldContinue = false;
                }
                else
                {
                    if (line.StartsWith("\"")) // it's a property name, begings with "
                    {
                        PropertyNames.Add(line.Substring(1, line.Length - 2));
                    }
                }

                List<string> valuesToAdd = new List<string>();

                if (line.StartsWith("{")) // it's first identifier of a value, begins with {
                {
                    int actuallinecounter = 0;
                    int linecounter = lineNumber;
                    int startPoint = 0;
                    //int endPoint = 0;
                    int IndexToAddTo = 0;

                    int number = 1;

                    if (configfile_Content_linesArray[linecounter + 1].StartsWith("}"))
                        number -= 1;

                    foreach (string line2 in configfile_Content_linesArray)
                    {
                        if (actuallinecounter == linecounter)
                        {
                            if (configfile_Content_linesArray[linecounter + number].StartsWith("  ")) // two spaces
                            {
                                startPoint = linecounter + 1;

                                valuesToAdd.Add(configfile_Content_linesArray[startPoint].Remove(0, 2));
                            }
                            else if ((configfile_Content_linesArray[linecounter + 1]).StartsWith("}"))
                            {
                                //endPoint = linecounter;
                                actuallinecounter++;

                                break;
                            }

                            linecounter++;
                            actuallinecounter++;
                        }
                        else
                        {
                            actuallinecounter++;
                        }
                    }

                    IndexToAddTo++;

                    string[] _finalvaluesToAdd = new string[valuesToAdd.Count];
                    valuesToAdd.CopyTo(_finalvaluesToAdd);

                    PropertyValues.Add(_finalvaluesToAdd);

                    continueFrom_LineNumber = actuallinecounter;
                    shouldContinue = true;
                }

                lineNumber++;

            }

            if (showDebugDialogs)
            {
                string propertyvalues_string = "";

                foreach (string[] array in PropertyValues)
                {
                    propertyvalues_string += String.Join("\n", array);
                    propertyvalues_string += "\n";
                }

                TextMessageDialog("", String.Join("\n", PropertyNames.ToArray()) + "\n\n----------\n\n" + String.Join("\n", propertyvalues_string));
            }


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

                    string[] s_names = PropertyValues[PropertyNames.IndexOf("camera_names")];

                    string[] s_urls = PropertyValues[PropertyNames.IndexOf("camera_urls")];

                    if (s_names.Length != s_urls.Length)
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

                    string[] Value = PropertyValues[counter];

                    if (name == "camera_names" || name == "camera_urls")
                    {
                        if (invalid_cameracustomizations)
                        {
                            counter++;
                            break;
                        }
                        else
                        {
                            System.Collections.Specialized.StringCollection camera_namesCollection = Settings[name] as System.Collections.Specialized.StringCollection;
                            camera_namesCollection.Clear();
                            camera_namesCollection.AddRange(Value);
                        }

                        counter++;
                    }
                    else
                    {
                        bool bool_tryParse_result;
                        int int_tryParse_result;

                        //if (Value[0] == "true" || Value[0] == "false")
                            if (bool.TryParse(Value[0], out bool_tryParse_result) == true)
                                Settings[name] = bool.Parse(Value[0]);
                            else if (int.TryParse(Value[0], out int_tryParse_result) == true)
                                Settings[name] = int.Parse(Value[0]);

                        counter++;
                    }
                }

                Settings.Save();
            }
            else
                return;
        }

        void ResetOfficialCameraCustomizations() // TODO
        {
            ReadConfigurationFile(defaultconfig_file_URL, true);

            if (PropertyNames.Contains("camera_names") & PropertyNames.Contains("camera_urls"))
            {
                foreach (string propertyname in PropertyNames)
                {
                    if (propertyname == "camera_names")
                    {
                        //Settings["camera_names"] = PropertyValues[PropertyNames.IndexOf("camera_names")];
                    }

                    if (propertyname == "camera_urls")
                        {
                            //Settings["camera_urls"] = PropertyValues[PropertyNames.IndexOf("camera_urls")];
                        }
                }
            }
        }

        public bool DefaultConfig_Heartbeat()
        {
            // compares the values from the read stuff from ^^ up there to the local config and informs user through titlebar info button whether they'd like to update the changed values
            // returns true if there are changes, false if there's nothing
            ReadConfigurationFile(defaultconfig_file_URL, false);

            bool valueToReturn = false;

            foreach (string property in PropertyNames)
            {
                if (Settings[property] != PropertyValues[PropertyNames.IndexOf(property)])
                {
                    valueToReturn = true;
                }
                else
                    valueToReturn = false;
            }

            return valueToReturn;
        }
    }
}
