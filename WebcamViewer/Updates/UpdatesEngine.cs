using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;

namespace WebcamViewer.Updates
{
    [JsonObject(MemberSerialization.OptIn, Title = "UpdateEngine")]
    class UpdatesEngine
    {

        #region Classes
        public class Update
        {
            [JsonProperty("title")]
            public string Title { get; set; } = "Title";

            [JsonProperty("description")]
            public string Description { get; set; } = "Description";

            [JsonProperty("version")]
            public Version VersionNumber { get; set; } = new Version(1,0,0,0);

            public int Filesize { get; set; }

            [JsonProperty("downloadUrl")]
            public string DownloadUrl { get; set; }

            [JsonProperty("isMajor")]
            public bool IsMajor { get; set; }

            [JsonProperty("channel")]
            public string Channel { get; set; }
        }
        public struct Version
        {
            public Version(ushort[] array)
            {
                if (array.Length < 4)
                    throw new ArgumentOutOfRangeException(nameof(array));
                if (array.Length > 4)
                    Array.Resize(ref array, 4);
                _versionNumbers = array;
            }
            public Version(ushort major, ushort minor, ushort build, ushort revision)
            {
                _versionNumbers = new[] { major, minor, build, revision };
            }

            public static Version FromString(string toConvert)
            {
                if (toConvert.Any(c => c != '.' || !char.IsDigit(c)))
                    throw new ArgumentException(nameof(toConvert));
                List<ushort> temp = toConvert
                    .Split('.')
                    .Select(shorts => Convert.ToUInt16(shorts))
                    .ToList();
                if (temp.Count < 4)
                    throw new ArgumentOutOfRangeException(nameof(toConvert));
                ushort[] verTemp = temp.Skip(temp.Count - 4).ToArray();
                return new Version(verTemp);
            }

            /// <summary>
            /// Sets the version, use it like this : 
            /// <code>SetVersion(major: 2, revision: 4)</code>
            /// </summary>
            /// <param name="major">The major number</param>
            /// <param name="minor">The minor number</param>
            /// <param name="build">The build number</param>
            /// <param name="revision">The revision number</param>
            public void SetVersion(ushort? major = null, ushort? minor = null, ushort? build = null, ushort? revision = null)
            {
                _versionNumbers = new[]
                                 {
                                     major ?? Major,
                                     minor ?? Minor,
                                     build ?? Build,
                                     revision ?? Revision
                                 };
            }
            private ushort[] _versionNumbers;
            public ushort Major => _versionNumbers[0];
            public ushort Minor => _versionNumbers[1];
            public ushort Build => _versionNumbers[2];
            public ushort Revision => _versionNumbers[3];
            public override string ToString()
            {
                return $"{Major}.{Minor}.{Build}.{Revision}";
            }
        }
        #endregion

        public void Test_CreateCurrentVersionFile(string location)
        {
            Popups.MessageDialog dialog = new Popups.MessageDialog();
            dialog.Title = "Choose where to save the file";
            dialog.Content = new TextBox();
            TextBox textbox = dialog.Content as TextBox;
            dialog.FirstButtonContent = "Save";
            dialog.SecondButtonContent = "Don't save";
            
            if (dialog.ShowDialogWithResult() == 0)
            {
                try
                {
                    using (StreamWriter sw = new StreamWriter(textbox.Text, false))
                    {
                        Update[] list;

                        Update update1 = new Update() { Channel = "release" };
                        Update update2 = new Update() { Channel = "beta" };
                        Update update3 = new Update() { Channel = "developer" };

                        list = new Update[3];

                        list[0] = update1;
                        list[1] = update2;
                        list[2] = update3;

                        JsonSerializer.Create(new JsonSerializerSettings { Formatting = Formatting.Indented }).Serialize(sw, list);
                        MessageBox.Show("CreateJSONFile(): Ding!");
                    }
                }
                catch (Exception ex)
                {
                    Popups.MessageDialog edialog = new Popups.MessageDialog() { Title = "Error", Content = "Could not save file.\n" + ex.Message };
                }
            }
        }
    }
}