using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace VirtualDesktopGridSwitcher.Settings {

    public class SettingValues {
        
        public event ApplyHandler Apply;
        public delegate bool ApplyHandler();

        public int Columns = 3;
        public int Rows = 3;

        public bool WrapAround = false;

        public bool CtrlModifierSwitch = true;
        public bool WinModifierSwitch = false;
        public bool AltModifierSwitch = true;
        public bool ShiftModifierSwitch = false;

        public bool CtrlModifierMove = true;
        public bool WinModifierMove = false;
        public bool AltModifierMove = true;
        public bool ShiftModifierMove = true;

        public bool FKeysForNumbers = false;

        public List<string[]> WebBrowserProgIDToExe =
            new List<string[]> {
                new string[] { "AppXq0fevzme2pys62n3e0fbqa7peapykr8v", "ApplicationFrameHost.exe" }, // Edge
                new string [] { "IE.HTTP", "iexplore.exe" },
                new string[] { "ChromeHTML", "chrome.exe" },
                new string[] { "FirefoxURL", "firefox.exe" },
                new string[] { "OperaStable" , "opera.exe"}
            };

        private static string SettingsFileName { 
            get {
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                return Path.Combine(baseDir, "VirtualDesktopGridSwitcher.Settings");
            }
        }


        public static SettingValues Load() {
            if (!File.Exists(SettingsFileName)) {
                return new SettingValues();
            } else {
                XmlSerializer serializer = new XmlSerializer(typeof(SettingValues));
                FileStream fs = new FileStream(SettingsFileName, FileMode.Open);
                var settings = (SettingValues)serializer.Deserialize(fs);
                fs.Close();
                return settings;
            }
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

        public bool ApplySettings() {
            if (this.Apply != null) {
                return this.Apply();
            }
            return true;
        }
    }
}
