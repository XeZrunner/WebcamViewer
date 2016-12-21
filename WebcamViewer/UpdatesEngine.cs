using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WebcamViewer
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
            public string Version { get; set; } = "x.x.x.x";

            public int Filesize { get; set; }

            [JsonProperty("downloadUrl")]
            public string DownloadUrl { get; set; }

            [JsonProperty("isMajor")]
            public bool IsMajor { get; set; }

            [JsonProperty("channel")]
            public string Channel { get; set; }
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