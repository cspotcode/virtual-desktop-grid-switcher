using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using GlobalHotkeyWithDotNET;
using VDMHelperCLR.Common;
using VirtualDesktopGridSwitcher.Settings;
using WindowsDesktop;


namespace VirtualDesktopGridSwitcher {

    class VirtualDesktopGridManager: IDisposable {

        private SettingValues settings;
        private SysTrayProcess sysTrayProcess;

        private IVdmHelper VDMHelper;

        private Dictionary<VirtualDesktop, int> desktopIdLookup;
        private VirtualDesktop[] desktops;
        private IntPtr[] activeWindows;

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern IntPtr SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(int vKey); 

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

        [DllImport("user32.dll")]
        static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        private const uint WINEVENT_OUTOFCONTEXT = 0;
        private const uint EVENT_SYSTEM_FOREGROUND = 3;

        private WinEventDelegate foregroundWindowChangedDelegate;

        public VirtualDesktopGridManager(SysTrayProcess sysTrayProcess, SettingValues settings)
        {
            this.settings = settings;
            this.sysTrayProcess = sysTrayProcess;

            this.VDMHelper = VdmHelperFactory.CreateInstance();
            this.VDMHelper.Init();

            foregroundWindowChangedDelegate = new WinEventDelegate(ForegroundWindowChanged);
            SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, foregroundWindowChangedDelegate, 0, 0, WINEVENT_OUTOFCONTEXT);

            Start();
        }

        public void Dispose()
        {
            Stop();

            this.VDMHelper.Dispose();
        }

        private bool Start() {
            try {
                var curDesktops = VirtualDesktop.GetDesktops().ToList();

                while (curDesktops.Count() > settings.Rows * settings.Columns) {
                    var last = curDesktops.Last();
                    last.Remove();
                    curDesktops.Remove(last);
                }

                while (curDesktops.Count() < settings.Rows * settings.Columns) {
                    curDesktops.Add(VirtualDesktop.Create());
                }

                desktops = VirtualDesktop.GetDesktops();
                desktopIdLookup = new Dictionary<VirtualDesktop, int>();
                int index = 0;
                desktops.ToList().ForEach(d => desktopIdLookup.Add(d, index++));

                this._current = desktopIdLookup[VirtualDesktop.Current];

                activeWindows = new IntPtr[desktops.Length];

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
            settings.Apply -= Restart;
            UnregisterHotKeys();
            if (desktops != null) {
                VirtualDesktop.CurrentChanged -= VirtualDesktop_CurrentChanged;
                desktops = null;
                desktopIdLookup = null;
                activeWindows = null;
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
            if (activeWindows[Current] != IntPtr.Zero) {
                var desktop = VirtualDesktop.FromHwnd(activeWindows[Current]);
                if (desktop != null && desktopIdLookup[desktop] == this._current) {
                    SetForegroundWindow(activeWindows[Current]);
                }
            }
        }

        void ForegroundWindowChanged(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime) {
            if (desktops != null) {
                activeWindows[desktopIdLookup[VirtualDesktop.Current]] = hwnd;
            }
            //ReleaseModifierKeys();
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
            activeWindows[Current] = GetForegroundWindow();
            Current = index;
        }

        public void Move(int index) {
            var window = GetForegroundWindow();
            this.VDMHelper.MoveWindowToDesktop(window, desktops[index].Id);
            activeWindows[Current] = IntPtr.Zero;
            // VDMHelper sets window as foreground so avoid setting in changed event
            activeWindows[index] = IntPtr.Zero;
            Current = index;
            activeWindows[index] = window;
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

            RegisterSwitchHotkey(Keys.Left, delegate { this.Switch(Left); });
            RegisterSwitchHotkey(Keys.Right, delegate { this.Switch(Right); });
            RegisterSwitchHotkey(Keys.Up, delegate { this.Switch(Up); });
            RegisterSwitchHotkey(Keys.Down, delegate { this.Switch(Down); });

            RegisterMoveHotkey(Keys.Left, delegate { Move(Left); });
            RegisterMoveHotkey(Keys.Right, delegate { Move(Right); });
            RegisterMoveHotkey(Keys.Up, delegate { Move(Up); });
            RegisterMoveHotkey(Keys.Down, delegate { Move(Down); });

            for (int keyNumber = 1; keyNumber <= Math.Min(DesktopCount, settings.FKeysForNumbers ?  12 : 9) ; ++keyNumber) {
                var desktopIndex = keyNumber - 1;
                Keys keycode =
                    (Keys)Enum.Parse(typeof(Keys), (settings.FKeysForNumbers ? "F" : "D") + keyNumber.ToString());
                
                RegisterSwitchHotkey(keycode, delegate { this.Switch(desktopIndex); });

                RegisterMoveHotkey(keycode, delegate { this.Move(desktopIndex); });
            }

        }

        private void RegisterSwitchHotkey(Keys keycode, Action action) {
            Hotkey hk = new Hotkey() {
                Control = settings.CtrlModifierSwitch,
                Windows = settings.WinModifierSwitch,
                Alt = settings.AltModifierSwitch,
                Shift = settings.ShiftModifierSwitch,
                KeyCode = keycode
            };
            hk.Pressed += delegate { action(); };
            if (hk.Register(null)) {
                hotkeys.Add(hk);
            } else {
                MessageBox.Show("Failed to register switch hotkey for " + hk.KeyCode,
                                "Warning",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
            }
        }

        private void RegisterMoveHotkey(Keys keycode, Action action)
        {
            Hotkey hk = new Hotkey()
            {
                Control = settings.CtrlModifierMove,
                Windows = settings.WinModifierMove,
                Alt = settings.AltModifierMove,
                Shift = settings.ShiftModifierMove,
                KeyCode = keycode
            };
            hk.Pressed += delegate { action(); };
            if (hk.Register(null))
            {
                hotkeys.Add(hk);
            }
            else
            {
                MessageBox.Show("Failed to register move window hotkey for " + hk.KeyCode,
                                "Warning",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
            }
        }

        private void UnregisterHotKeys() {
            hotkeys.ForEach(hk => hk.Unregister());
            hotkeys = null;
        }


        private void ReleaseModifierKeys() {
            const int WM_KEYUP = 0x101;
            const int VK_SHIFT = 0x10;
            const int VK_CONTROL = 0x11;
            const int VK_MENU = 0x12;
            const int VK_LWIN = 0x5B;
            const int VK_RWIN = 0x5C;

            var activeHWnd = GetForegroundWindow();
            if (IsKeyPressed(GetAsyncKeyState(VK_MENU))) {
                PostMessage(activeHWnd, WM_KEYUP, (IntPtr)VK_MENU, (IntPtr)0xC0380001);
                PostMessage(activeHWnd, WM_KEYUP, (IntPtr)VK_MENU, (IntPtr)0xC1380001);
            }
            if (IsKeyPressed(GetAsyncKeyState(VK_CONTROL))) {
                PostMessage(activeHWnd, WM_KEYUP, (IntPtr)VK_CONTROL, (IntPtr)0xC01D0001);
                PostMessage(activeHWnd, WM_KEYUP, (IntPtr)VK_CONTROL, (IntPtr)0xC11D0001);
            }
            if (IsKeyPressed(GetAsyncKeyState(VK_SHIFT))) {
                PostMessage(activeHWnd, WM_KEYUP, (IntPtr)VK_SHIFT, (IntPtr)0xC02A0001);
                PostMessage(activeHWnd, WM_KEYUP, (IntPtr)VK_SHIFT, (IntPtr)0xC0360001);
            }
            if (IsKeyPressed(GetAsyncKeyState(VK_LWIN))) {
                PostMessage(activeHWnd, WM_KEYUP, (IntPtr)VK_LWIN, (IntPtr)0xC15B0001);
            }
            if (IsKeyPressed(GetAsyncKeyState(VK_RWIN))) {
                PostMessage(activeHWnd, WM_KEYUP, (IntPtr)VK_RWIN, (IntPtr)0xC15C0001);
            }
        }

        private bool IsKeyPressed(short keystate) {
            return (keystate & 0x8000) != 0;
        }
    }
}
