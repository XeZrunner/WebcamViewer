﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WebcamViewer.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "14.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(@"<?xml version=""1.0"" encoding=""utf-16""?>
<ArrayOfString xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <string>Michalovce, Slovakia - Námestie osloboditeľov 01</string>
  <string>Nowy Targ, Poland - Restaurant ""U gazdy"" (Kamera na Tatry)</string>
  <string>Gelendzhik, Russia - Glavmore (Camera 2)</string>
  <string>Poprad, Slovakia - Weather station ""Funsat""</string>
  <string>Michalovce, Slovakia - Busová stanica</string>
  <string>Żywiec, Poland - Żywiec lake</string>
  <string>Levoča, Slovakia - www.pocasielevoca.sk</string>
</ArrayOfString>")]
        public global::System.Collections.Specialized.StringCollection camera_names {
            get {
                return ((global::System.Collections.Specialized.StringCollection)(this["camera_names"]));
            }
            set {
                this["camera_names"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(@"<?xml version=""1.0"" encoding=""utf-16""?>
<ArrayOfString xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <string>http://www.michalovce.sk/webcam/michalovce.sk-camera-001.jpg</string>
  <string>http://www.ugazdy.pl/kamerka/obrazek.jpg</string>
  <string>http://static.glavmore.ru/cam/cam-1.jpg</string>
  <string>http://funsat.sk.d.websupport.sk/webcam/funsat.jpg</string>
  <string>http://gecom.sk/img/kam-autobuska-orig.jpg</string>
  <string>http://www.megasurf.pl/cam/Video01.jpg</string>
  <string>http://www.pocasielevoca.sk/data/snimek.jpg</string>
</ArrayOfString>")]
        public global::System.Collections.Specialized.StringCollection camera_urls {
            get {
                return ((global::System.Collections.Specialized.StringCollection)(this["camera_urls"]));
            }
            set {
                this["camera_urls"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(@"<?xml version=""1.0"" encoding=""utf-16""?>
<ArrayOfString xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <string>3</string>
  <string>3</string>
  <string>30</string>
  <string>10</string>
  <string>20</string>
  <string>3</string>
  <string>30</string>
</ArrayOfString>")]
        public global::System.Collections.Specialized.StringCollection camera_refreshrates {
            get {
                return ((global::System.Collections.Specialized.StringCollection)(this["camera_refreshrates"]));
            }
            set {
                this["camera_refreshrates"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Update 2 Beta")]
        public string versionid {
            get {
                return ((string)(this["versionid"]));
            }
            set {
                this["versionid"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("20161216-01")]
        public string buildid {
            get {
                return ((string)(this["buildid"]));
            }
            set {
                this["buildid"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1.2 Prerelease")]
        public string webcamengine_version {
            get {
                return ((string)(this["webcamengine_version"]));
            }
            set {
                this["webcamengine_version"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Webcam Viewer Update 2 Beta - 2016.12.16; feature level 1\r\n\r\nNew features:\r\n- New" +
            " image saving panel, replacing the old \"in-menu\" one completely\r\n\r\nDeveloper:\r\n-" +
            " Added a new UI Controls Testing page to Settings with all the UI controls liste" +
            "d\r\n\r\nEarly testing:\r\n- Experimental: zSettings Early Preview\r\nAccess in menu by " +
            "clicking on zSettings.\r\n---------------------------------------------\r\n\r\nWebcam " +
            "Viewer Update 2 Beta - 2016.12.03; feature level 1\r\n\r\nNew features:\r\n- Experimen" +
            "tal: new Overview!\r\nYou can test the new Overview by holding down Shift on your " +
            "keyboard while clicking on the Overview button in the menu.\r\nSome features may n" +
            "ot work just yet, it is going to get developed more and more in future builds.\r\n" +
            "---------------------------------------------\r\n\r\nWebcam Viewer Update 2 Beta - 2" +
            "016.11.16; feature level 1\r\n\r\nNew features:\r\n- Added the ability to change the i" +
            "mage stretching mode.\r\n\r\nDeveloper:\r\n- Added the default cameras page, with all " +
            "the default cameras listed.\r\n\r\nLogging:\r\n- Most basic things are logged now.\r\n--" +
            "-------------------------------------------\r\n\r\nWebcam Viewer Update 2 Beta - 201" +
            "6.11.12; feature level 1\r\n\r\nUser interface:\r\n- Unified RippleDrawable inclusion\r" +
            "\nAll hard-coded ripple effects are replaced with the new XeZrunner.UI RippleDraw" +
            "able control which is further similiar to Google\'s Android Lollipop ripple.\r\nThe" +
            " new ripple can no longer be moved by the user, in favor of the ripple slowly \"e" +
            "xpanding\" towards the center of the control, much like on the actual operating s" +
            "ystem.\r\n---------------------------------------------")]
        public string changelog {
            get {
                return ((string)(this["changelog"]));
            }
            set {
                this["changelog"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool app_logging {
            get {
                return ((bool)(this["app_logging"]));
            }
            set {
                this["app_logging"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("developer")]
        public string app_debugmode {
            get {
                return ((string)(this["app_debugmode"]));
            }
            set {
                this["app_debugmode"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool app_firstrun {
            get {
                return ((bool)(this["app_firstrun"]));
            }
            set {
                this["app_firstrun"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1")]
        public int ui_accent {
            get {
                return ((int)(this["ui_accent"]));
            }
            set {
                this["ui_accent"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public int ui_theme {
            get {
                return ((int)(this["ui_theme"]));
            }
            set {
                this["ui_theme"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1")]
        public double ui_animationspeed {
            get {
                return ((double)(this["ui_animationspeed"]));
            }
            set {
                this["ui_animationspeed"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool home_blurbehind {
            get {
                return ((bool)(this["home_blurbehind"]));
            }
            set {
                this["home_blurbehind"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool home_debugoverlay {
            get {
                return ((bool)(this["home_debugoverlay"]));
            }
            set {
                this["home_debugoverlay"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool home_archiveorg {
            get {
                return ((bool)(this["home_archiveorg"]));
            }
            set {
                this["home_archiveorg"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool home_refreshenabled {
            get {
                return ((bool)(this["home_refreshenabled"]));
            }
            set {
                this["home_refreshenabled"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool settings_showtitlebarcolor {
            get {
                return ((bool)(this["settings_showtitlebarcolor"]));
            }
            set {
                this["settings_showtitlebarcolor"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool settings_experiment_UpdateUI {
            get {
                return ((bool)(this["settings_experiment_UpdateUI"]));
            }
            set {
                this["settings_experiment_UpdateUI"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool experiment_NewFileBrowserUX {
            get {
                return ((bool)(this["experiment_NewFileBrowserUX"]));
            }
            set {
                this["experiment_NewFileBrowserUX"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool home_webcamimageBackgroundMode {
            get {
                return ((bool)(this["home_webcamimageBackgroundMode"]));
            }
            set {
                this["home_webcamimageBackgroundMode"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool home_webcamimageBackgroundMode_BlackOverride {
            get {
                return ((bool)(this["home_webcamimageBackgroundMode_BlackOverride"]));
            }
            set {
                this["home_webcamimageBackgroundMode_BlackOverride"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("default")]
        public string app_language {
            get {
                return ((string)(this["app_language"]));
            }
            set {
                this["app_language"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("update2-development")]
        public string app_prereleasechannel {
            get {
                return ((string)(this["app_prereleasechannel"]));
            }
            set {
                this["app_prereleasechannel"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("uniform")]
        public string home_imagesizing {
            get {
                return ((string)(this["home_imagesizing"]));
            }
            set {
                this["home_imagesizing"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool experiment_zOverview {
            get {
                return ((bool)(this["experiment_zOverview"]));
            }
            set {
                this["experiment_zOverview"] = value;
            }
        }
    }
}
