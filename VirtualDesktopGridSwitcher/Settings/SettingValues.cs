using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace VirtualDesktopGridSwitcher.Settings {

    public class SettingValues {

        public class Modifiers {
            public bool Ctrl;
            public bool Win;
            public bool Alt;
            public bool Shift;
        }

        public class Hotkey {
            public Keys Key;
            public Modifiers Modifiers;
        }

        public class BrowserInfo {
            public string ProgID;
            public string ExeName;
            public string ClassName;
        }

        public int Columns = 3;
        public int Rows = 3;

        public bool WrapAround = false;

        public Modifiers SwitchModifiers = 
            new Modifiers {
//                Ctrl = true, Win = false, Alt = true, Shift = false
                Ctrl = true, Win = true, Alt = true, Shift = true
            };

        // To support old XML format
        /// <summary>
        /// OBSOLETE
        /// </summary>
        public bool CtrlModifierSwitch {
            set { SwitchModifiers.Ctrl = value; }
            private get { return SwitchModifiers.Ctrl; }
        }
        /// <summary>
        /// OBSOLETE
        /// </summary>
        public bool WinModifierSwitch {
            set { SwitchModifiers.Win = value; }
        }
        /// <summary>
        /// OBSOLETE
        /// </summary>
        public bool AltModifierSwitch {
            set { SwitchModifiers.Alt = value; }
        }
        /// <summary>
        /// OBSOLETE
        /// </summary>
        public bool ShiftModifierSwitch {
            set { SwitchModifiers.Shift = value; }
        }

        public Modifiers MoveModifiers =
            new Modifiers {
                Ctrl = true, Win = false, Alt = true, Shift = true
            };

        // To support old XML format
        /// <summary>
        /// OBSOLETE
        /// </summary>
        public bool CtrlModifierMove {
            set { MoveModifiers.Ctrl = value; }
        }
        /// <summary>
        /// OBSOLETE
        /// </summary>
        public bool WinModifierMove {
            set { MoveModifiers.Win = value; }
        }
        /// <summary>
        /// OBSOLETE
        /// </summary>
        public bool AltModifierMove {
            set { MoveModifiers.Alt = value; }
        }
        /// <summary>
        /// OBSOLETE
        /// </summary>
        public bool ShiftModifierMove {
            set { MoveModifiers.Shift = value; }
        }

        public bool FKeysForNumbers = false;

        public Hotkey StickyWindowHotKey =
            new Hotkey {
                Key = Keys.Space,
                Modifiers = new Modifiers {
                    Ctrl = true, Win = false, Alt = true, Shift = true
                }
            };


        public Hotkey AlwaysOnTopHotkey =
            new Hotkey {
                Key = Keys.Space,
                Modifiers = new Modifiers {
                    Ctrl = true, Win = false, Alt = true, Shift = false
                }
            };

        public bool ActivateWebBrowserOnSwitch = true;

        public List<BrowserInfo> BrowserInfoList =
            new List<BrowserInfo> {
                // Edge works without us interfering
                //new BrowserInfo { ProgID = "AppXq0fevzme2pys62n3e0fbqa7peapykr8v", ExeName = "ApplicationFrameHost.exe", ClassName = "ApplicationFrameWindow" },

                // IE doesn't work whatever you do - always uses oldest window!
                //new BrowserInfo { ProgID = "IE.HTTP", ExeName = "iexplore.exe", ClassName = "IEFrame" },

                new BrowserInfo { ProgID = "ChromeHTML", ExeName = "chrome.exe", ClassName = "Chrome_WidgetWin_1" },

                new BrowserInfo { ProgID = "FirefoxURL", ExeName = "firefox.exe", ClassName = "MozillaWindowClass" },
                
                // Opera works without us interfering
                //new BrowserInfo { ProgID = "OperaStable" , ExeName = "opera.exe", ClassName = "Chrome_WidgetWin_1" }
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
