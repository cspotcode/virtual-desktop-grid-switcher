using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GlobalHotkeyWithDotNET;
using VirtualDesktopGridSwitcher.Settings;
using WindowsDesktop;


namespace VirtualDesktopGridSwitcher {
    class VirtualDesktopGridManager: IDisposable {

        private SettingValues settings;
        private SysTrayProcess sysTrayProcess;

        private Dictionary<VirtualDesktop, int> desktopIdLookup;
        private VirtualDesktop[] desktops;

        public VirtualDesktopGridManager(SysTrayProcess sysTrayProcess, SettingValues settings) {
            this.settings = settings;
            this.sysTrayProcess = sysTrayProcess;

            Start();
        }

        public void Dispose()
        {
            Stop();
        }

        private bool Start() {
            try {
                var curDesktops = VirtualDesktop.GetDesktops().ToList();

                //while (curDesktops.Count() > settings.Rows * settings.Columns) {
                //    var last = curDesktops.Last();
                //    last.Remove();
                //    curDesktops.Remove(last);
                //}

                while (curDesktops.Count() < settings.Rows * settings.Columns) {
                    curDesktops.Add(VirtualDesktop.Create());
                }

                desktops = VirtualDesktop.GetDesktops();
                desktopIdLookup = new Dictionary<VirtualDesktop, int>();
                int index = 0;
                desktops.ToList().ForEach(d => desktopIdLookup.Add(d, index++));

                this._current = desktopIdLookup[VirtualDesktop.Current];

                VirtualDesktop.CurrentChanged += VirtualDesktop_CurrentChanged;
            } catch { }

            sysTrayProcess.ShowIconForDesktop(Current);

            settings.Apply += Restart;

            try {
                RegisterHotKeys();
            } catch {
                return false;
            }

            return true;
        }

        private void Stop() {
            settings.Apply += Restart;
            UnregisterHotKeys();
            if (desktops != null) {
                VirtualDesktop.CurrentChanged -= VirtualDesktop_CurrentChanged;
                desktops = null;
                desktopIdLookup = null;
            }
        }

        public bool Restart()
        {
            Stop();
            return Start();
        }
        
        public int DesktopCount { 
            get {
                return settings.Rows * settings.Columns;
            }
        }
        
        private void VirtualDesktop_CurrentChanged(object sender, VirtualDesktopChangedEventArgs e) {
            this._current = desktopIdLookup[VirtualDesktop.Current];
            sysTrayProcess.ShowIconForDesktop(this._current);
        }

        private int _current;
        public int Current {
            get {
                return _current;
            }
            private set {
                if (desktops != null) {
                    desktops[value].Switch();
                } else {
                    _current = value;
                    sysTrayProcess.ShowIconForDesktop(this._current);
                }
            }
        }

        public int Left {
            get {
                if (ColumnOf(Current - 1) < ColumnOf(Current)) {
                    return Current - 1;
                } else {
                    if (settings.WrapAround) {
                        return Current + settings.Columns - 1;
                    } else {
                        return Current;
                    }
                }
            }
        }

        public int Right {
            get {
                if (ColumnOf(Current + 1) > ColumnOf(Current)) {
                    return Current + 1;
                } else {
                    if (settings.WrapAround) {
                        return Current - settings.Columns + 1;
                    } else {
                        return Current;
                    }
                }
            }
        }

        public int Up {
            get {
                if (RowOf(Current - settings.Columns) < RowOf(Current)) {
                    return Current - settings.Columns;
                } else {
                    if (settings.WrapAround) {
                        return ((settings.Rows-1) * settings.Columns) + ColumnOf(Current);
                    } else {
                        return Current;
                    }
                }
            }
        }

        public int Down {
            get {
                if (RowOf(Current + settings.Columns) > RowOf(Current)) {
                    return Current + settings.Columns;
                } else {
                    if (settings.WrapAround) {
                        return ColumnOf(Current);
                    } else {
                        return Current;
                    }
                }
            }
        }

        public void Switch(int index) {
            Current = index;
        }

        public int SwitchLeft() {
            Current = Left;
            return Current;
        }

        public int SwitchRight() {
            Current = Right;
            return Current;
        }

        public int SwitchUp() {
            Current = Up;
            return Current;
        }

        public int SwitchDown() {
            Current = Down;
            return Current;
        }
        
        private int ColumnOf(int index) {
            return ((index + settings.Columns) % settings.Columns);
        }

        private int RowOf(int index) {
            return ((index / settings.Columns) + settings.Rows) % settings.Rows;
        }

        private List<Hotkey> hotkeys;

        private void RegisterHotKeys() {
            
            hotkeys = new List<Hotkey>();

            Hotkey hkLeft = new Hotkey() {
                Control = true,
                Alt = true,
                KeyCode = Keys.Left
            };
            hkLeft.Pressed += delegate {
                this.SwitchLeft();
            };
            RegisterHotkey(hkLeft);

            Hotkey hkRight = new Hotkey() {
                Control = true,
                Alt = true,
                KeyCode = Keys.Right
            };
            hkRight.Pressed += delegate {
                this.SwitchRight();
            };
            RegisterHotkey(hkRight);

            Hotkey hkUp = new Hotkey() {
                Control = true,
                Alt = true,
                KeyCode = Keys.Up
            };
            hkUp.Pressed += delegate {
                this.SwitchUp();
            };
            RegisterHotkey(hkUp);

            Hotkey hkDown = new Hotkey() {
                Control = true,
                Alt = true,
                KeyCode = Keys.Down
            };
            hkDown.Pressed += delegate {
                this.SwitchDown();
            };
            RegisterHotkey(hkDown);

            for (int keyNumber = 1; keyNumber <= Math.Min(DesktopCount, settings.FKeysForNumbers ?  12 : 9) ; ++keyNumber) {
                Hotkey hkIndex = new Hotkey() {
                    Control = true,
                    Alt = true,
                    KeyCode = (Keys)Enum.Parse(typeof(Keys), (settings.FKeysForNumbers ? "F" : "D") + keyNumber.ToString())
                };
                var desktopIndex = keyNumber - 1;
                hkIndex.Pressed += delegate {
                    this.Switch(desktopIndex);
                };
                RegisterHotkey(hkIndex);
            }

        }

        private void RegisterHotkey(Hotkey hkLeft) {
            if (hkLeft.Register(null)) {
                hotkeys.Add(hkLeft);
            } else {
                MessageBox.Show("Failed to register hotkey for " + hkLeft.KeyCode,
                                "Warning",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
            }
        }

        private void UnregisterHotKeys() {
            hotkeys.ForEach(hk => hk.Unregister());
            hotkeys = null;
        }

    }
}
