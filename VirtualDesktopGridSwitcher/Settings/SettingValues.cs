using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace VirtualDesktopGridSwitcher.Settings {

    public class SettingValues {

        public class BrowserInfo {
            public string ProgID;
            public string ExeName;
            public string ClassName;
        }

        public int Columns = 3;
        public int Rows = 3;

        public bool WrapAround = false;

        public bool ActivateWebBrowserOnSwitch = true;

        public List<BrowserInfo> BrowserInfoList = new List<BrowserInfo>();

        public int MoveOnNewWindowDetectTimeoutMs = 500;

        [XmlArrayItem(ElementName = "ExeName")]
        public List<string> MoveOnNewWindowExeNames = new List<string>();

        private static string SettingsFileName { 
            get {
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                return Path.Combine(baseDir, "VirtualDesktopGridSwitcher.Settings");
            }
        }

        private void SetListDefaults() {
            if (BrowserInfoList.Count == 0) {
                BrowserInfoList =
                    new List<BrowserInfo> {
                        // Edge works without us interfering
                        //new BrowserInfo { ProgID = "AppXq0fevzme2pys62n3e0fbqa7peapykr8v", ExeName = "ApplicationFrameHost.exe", ClassName = "ApplicationFrameWindow" },

                        // IE doesn't work whatever you do - always uses oldest window!
                        //new BrowserInfo { ProgID = "IE.HTTP", ExeName = "iexplore.exe", ClassName = "IEFrame" },

                        new BrowserInfo { ProgID = "ChromeHTML", ExeName = "chrome.exe", ClassName = "Chrome_WidgetWin_1" },

                        new BrowserInfo { ProgID = "FirefoxURL", ExeName = "firefox.exe", ClassName = "MozillaWindowClass" }

                        // Opera works without us interfering
                        //new BrowserInfo { ProgID = "OperaStable" , ExeName = "opera.exe", ClassName = "Chrome_WidgetWin_1" }
                    };
            }

            if (MoveOnNewWindowExeNames.Count == 0) {
                MoveOnNewWindowExeNames = new List<string>() { "WINWORD.EXE", "EXCEL.EXE" };
            }
        }

        public static SettingValues Load() {
            SettingValues settings;
            if (!File.Exists(SettingsFileName)) {
                settings = new SettingValues();
            } else {
                XmlSerializer serializer = new XmlSerializer(typeof(SettingValues));
                FileStream fs = new FileStream(SettingsFileName, FileMode.Open);
                settings = (SettingValues)serializer.Deserialize(fs);
                fs.Close();

                LoadOldSettings(settings);
            }

            settings.SetListDefaults();
            return settings;
        }

        private static void LoadOldSettings(SettingValues settings) {
            // Backward compatibility
            XDocument xdoc = XDocument.Load(SettingsFileName);
        }

        public bool Save() {
            try {
                XmlSerializer serializer =
                    new XmlSerializer(typeof(SettingValues));
                TextWriter writer = new StreamWriter(SettingsFileName);
                serializer.Serialize(writer, this);
                writer.Close();
            } catch {
                return false;
            }
            return true;
        }

        public event ApplyHandler Apply;
        public delegate bool ApplyHandler();

        public bool ApplySettings() {
            if (this.Apply != null) {
                return this.Apply();
            }
            return true;
        }

        public BrowserInfo GetBrowserToActivateInfo() {
            if (ActivateWebBrowserOnSwitch) {
                const string userChoice = @"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\http\UserChoice";
                using (RegistryKey userChoiceKey = Registry.CurrentUser.OpenSubKey(userChoice)) {
                    if (userChoiceKey != null) {
                        object progIdValue = userChoiceKey.GetValue("Progid");
                        if (progIdValue != null) {
                            return BrowserInfoList.Where(v => v.ProgID == progIdValue.ToString()).FirstOrDefault();
                        }
                    }
                }
            }
            return null;
        }
    }
}
