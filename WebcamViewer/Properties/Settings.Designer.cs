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
  <string>Michalovce - Námestie osloboditeľov 01 [SK]</string>
  <string>Nowy Targ - Ugazdy - Kamera na Tatry [PL]</string>
  <string>Glavmore (Camera 2) [RU]</string>
  <string>Poprad - Funsat Meteo [SK]</string>
  <string>Michalovce - Busová stanica (gecom.sk) [SK]</string>
  <string>Zywiec - Zywiec Lake (megasurf.pl) [PL]</string>
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
  <string>http://gecom.sk/img/autobuska-aktualne.jpg</string>
  <string>http://www.megasurf.pl/cam/Video01.jpg</string>
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
        [global::System.Configuration.DefaultSettingValueAttribute("20160802-01@webcamviewer_internal/update2/prerelease/beta/compiled/Debug/webcamed" +
            "itorv2_preview/")]
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
        [global::System.Configuration.DefaultSettingValueAttribute(" (Update 2 Beta - Internal Preview)")]
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
        [global::System.Configuration.DefaultSettingValueAttribute(@"Update pre-release builds
| Alpha - these (usually) only contain one or two specific changes that the new update is going to bring.
| Beta - contains almost all (if not all) changes that the new update is going to bring, though 
  they might not be the finalized.
| Bugtest Release - not always released, this is the first finalized version of the update, sent out to a few people to find
  bugs and instabilities or other non-working functions of the update.
| Release - the final version

Changes for Update 2 Beta:

This is an Internal Preview build. This build is only meant for internal testing, do not publish to public.
")]
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
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool birthday_notice {
            get {
                return ((bool)(this["birthday_notice"]));
            }
            set {
                this["birthday_notice"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool blur_image {
            get {
                return ((bool)(this["blur_image"]));
            }
            set {
                this["blur_image"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public int accentcolor {
            get {
                return ((int)(this["accentcolor"]));
            }
            set {
                this["accentcolor"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::System.Collections.Specialized.StringCollection savelocations {
            get {
                return ((global::System.Collections.Specialized.StringCollection)(this["savelocations"]));
            }
            set {
                this["savelocations"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool home_dynamiccoloring {
            get {
                return ((bool)(this["home_dynamiccoloring"]));
            }
            set {
                this["home_dynamiccoloring"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool home_weather {
            get {
                return ((bool)(this["home_weather"]));
            }
            set {
                this["home_weather"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("metric")]
        public string weather_units {
            get {
                return ((string)(this["weather_units"]));
            }
            set {
                this["weather_units"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool window_autosize {
            get {
                return ((bool)(this["window_autosize"]));
            }
            set {
                this["window_autosize"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool window_aeroborder {
            get {
                return ((bool)(this["window_aeroborder"]));
            }
            set {
                this["window_aeroborder"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("2")]
        public int imagesizing {
            get {
                return ((int)(this["imagesizing"]));
            }
            set {
                this["imagesizing"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool home_menu_blurbehind {
            get {
                return ((bool)(this["home_menu_blurbehind"]));
            }
            set {
                this["home_menu_blurbehind"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool settings_showaccentcolor {
            get {
                return ((bool)(this["settings_showaccentcolor"]));
            }
            set {
                this["settings_showaccentcolor"] = value;
            }
        }
    }
}
