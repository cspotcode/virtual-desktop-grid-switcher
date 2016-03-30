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

        public int Rows = 3;
        public int Columns = 3;

        public bool WrapAround = false;

        public bool CtrlModifier = true;
        public bool WinModifier = true;
        public bool AltModifier = false;
        public bool ShiftModifier = false;

        public bool FKeysForNumbers = true;

        private const string SettingsFileName = "VirtualDesktopSwitcher.Settings";

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
