using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using GlobalHotkeyWithDotNET;
using VDMHelperCLR.Common;
using VirtualDesktopGridSwitcher.Settings;
using WindowsDesktop;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using System.Threading;

namespace VirtualDesktopGridSwitcher {

    class VirtualDesktopGridManager: IDisposable {

        private SettingValues settings;
        private SysTrayProcess sysTrayProcess;

        private IVdmHelper VDMHelper;

        private Dictionary<VirtualDesktop, int> desktopIdLookup;
        private VirtualDesktop[] desktops;
        private IntPtr[] activeWindows;
        private IntPtr[] lastActiveBrowserWindows;

        private WinAPI.WinEventDelegate foregroundWindowChangedDelegate;

        public VirtualDesktopGridManager(SysTrayProcess sysTrayProcess, SettingValues settings)
        {
            this.settings = settings;
            this.sysTrayProcess = sysTrayProcess;

            this.VDMHelper = VdmHelperFactory.CreateInstance();
            this.VDMHelper.Init();

            foregroundWindowChangedDelegate = new WinAPI.WinEventDelegate(ForegroundWindowChanged);
            WinAPI.SetWinEventHook(WinAPI.EVENT_SYSTEM_FOREGROUND, WinAPI.EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, foregroundWindowChangedDelegate, 0, 0, WinAPI.WINEVENT_OUTOFCONTEXT);

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
                lastActiveBrowserWindows = new IntPtr[desktops.Length];

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
                lastActiveBrowserWindows = null;
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
        
        private void VirtualDesktop_CurrentChanged(object sender, VirtualDesktopChangedEventArgs e)
        {
            var newDesktop = desktopIdLookup[VirtualDesktop.Current];
            Debug.WriteLine("Switched from " + Current + " to " + newDesktop);

            if (this._current != newDesktop) {
                this._current = newDesktop;
                sysTrayProcess.ShowIconForDesktop(this._current);

                var fgHwnd = WinAPI.GetForegroundWindow();
                var lastActiveWindow = activeWindows[newDesktop];

                var browserInfo = settings.GetBrowserToActivateInfo();
                if (browserInfo != null) {
                    if (lastActiveBrowserWindows[Current] != activeWindows[newDesktop]) {
                        FindActivateBrowserWindow(lastActiveBrowserWindows[Current], browserInfo);
                    }
                }

                if (!ActivateWindow(lastActiveWindow)) {
                    Debug.WriteLine("Reactivate " + Current + " " + fgHwnd);
                    WinAPI.SetForegroundWindow(fgHwnd);
                }
            }
        }

        private class BrowserWinInfo {
            public IntPtr last;
            public IntPtr top;
            public IntPtr topOnDesktop;
        }

        private bool IterateFindTopLevelBrowserOnCurrentDesktop(IntPtr hwnd, BrowserWinInfo browserWinInfo, SettingValues.BrowserInfo browserInfo) {
            if (hwnd == IntPtr.Zero) {
                // None left to check
                return false;
            }

            browserWinInfo.last = hwnd;
            if (browserWinInfo.top == IntPtr.Zero && IsWindowDefaultBrowser(hwnd, browserInfo)) {
                browserWinInfo.top = hwnd;
                if (VirtualDesktop.FromHwnd(hwnd) == desktops[Current]) {
                    // Already top level so nothing to do
                    browserWinInfo.topOnDesktop = IntPtr.Zero;
                    return false;
                }
            }
            
            if (VirtualDesktop.FromHwnd(hwnd) == desktops[Current] && IsWindowDefaultBrowser(hwnd, browserInfo)) {
                browserWinInfo.topOnDesktop = hwnd;
                return false;
            }
            // Keep going
            return true;
        }

        private bool FindActivateBrowserWindow(IntPtr hwnd, SettingValues.BrowserInfo browserInfo)
        {
            if (WinAPI.IsWindow(hwnd)) {
                var desktop = VirtualDesktop.FromHwnd(hwnd);
                if (desktop != null && desktopIdLookup[desktop] == Current) {
                    Debug.WriteLine("Activate Known Browser " + Current + " " + hwnd);
                    ActivateBrowserWindow(hwnd);
                    return true;
                }
            }

            // Our last active record was not valid so find topmost on this desktop 
            BrowserWinInfo browserWinInfo = new BrowserWinInfo(); ;
            bool notFinished = true;
            do {
                var window = WinAPI.FindWindowEx(IntPtr.Zero, browserWinInfo.last, browserInfo.ClassName, null);
                notFinished = IterateFindTopLevelBrowserOnCurrentDesktop(window, browserWinInfo, browserInfo);
            } while (notFinished);
            if (browserWinInfo.topOnDesktop != IntPtr.Zero) {
                Debug.WriteLine("Activate Unknown Browser " + Current + " " + browserWinInfo.topOnDesktop);
                ActivateBrowserWindow(browserWinInfo.topOnDesktop);
                return true;
            }

            return false;
        }

        private void ActivateBrowserWindow(IntPtr hwnd) {
            WinAPI.WindowPlacement winInfo = new WinAPI.WindowPlacement();
            WinAPI.GetWindowPlacement(hwnd, ref winInfo);

            var prevHwnd = hwnd;
            do {
                prevHwnd = WinAPI.GetWindow(prevHwnd, WinAPI.GWCMD.GW_HWNDPREV);
            } while (prevHwnd != IntPtr.Zero && VirtualDesktop.FromHwnd(prevHwnd) != desktops[Current]);

            WinAPI.SetForegroundWindow(hwnd);
            if (winInfo.ShowCmd == WinAPI.ShowWindowCommands.ShowMinimized) {
                WinAPI.ShowWindow(hwnd, WinAPI.ShowWindowCommands.Restore);
                //System.Threading.Thread.Sleep(1000);
                WinAPI.ShowWindow(hwnd, winInfo.ShowCmd);
            }

            if (prevHwnd != IntPtr.Zero) {
                Debug.WriteLine("Browser behind " + prevHwnd + " " + GetWindowExeName(prevHwnd) + " " + GetWindowTitle(prevHwnd));
                WinAPI.SetWindowPos(hwnd, prevHwnd, 0, 0, 0, 0, WinAPI.SWPFlags.SWP_NOMOVE | WinAPI.SWPFlags.SWP_NOSIZE | WinAPI.SWPFlags.SWP_NOACTIVATE);
            }
        }

        private bool ActivateWindow(IntPtr hwnd) {
            if (hwnd != IntPtr.Zero) {
                var desktop = VirtualDesktop.FromHwnd(hwnd);
                if (desktop != null && desktopIdLookup[desktop] == this._current) {
                    Debug.WriteLine("Activate " + Current + " " + hwnd);
                    WinAPI.SetForegroundWindow(hwnd);
                    return true;
                }
            }
            return false;
        }

        Mutex mutex = new Mutex();
        void ForegroundWindowChanged(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime) {
            lock (mutex) {
                if (desktops != null) {

                    var desktopId = Current;
                    var desktop = VirtualDesktop.FromHwnd(hwnd);
                    if (desktop != null) {
                        desktopId = desktopIdLookup[desktop];
                        activeWindows[desktopId] = hwnd;
                        Debug.WriteLine("Active " + desktopId + " " + hwnd + " " + GetWindowTitle(hwnd));
                    }

                    Debug.WriteLine("Foreground " + Current + " " + (desktop != null ? desktopIdLookup[desktop].ToString() : "?") + " " + hwnd + " " + GetWindowTitle(hwnd));

                    if (IsWindowDefaultBrowser(hwnd, settings.GetBrowserToActivateInfo())) {
                        Debug.WriteLine("Browser " + Current + " " + desktopIdLookup[VirtualDesktop.Current] + " " + desktopId + " " + hwnd);
                        lastActiveBrowserWindows[desktopId] = hwnd;
                    }
                }
                //ReleaseModifierKeys();
            }
        }

        private static string GetWindowTitle(IntPtr hwnd) {
            StringBuilder title = new StringBuilder(1024);
            WinAPI.GetWindowText(hwnd, title, title.Capacity);
            return title.ToString();
        }

        private static string GetWindowExeName(IntPtr hwnd) {
            uint pid = 0;
            WinAPI.GetWindowThreadProcessId(hwnd, out pid);

            IntPtr pic = WinAPI.OpenProcess(WinAPI.ProcessAccessFlags.All, true, (int)pid);
            if (pic == IntPtr.Zero) {
                return null;
            }

            StringBuilder exeDevicePath = new StringBuilder(1024);
            if (WinAPI.GetProcessImageFileName(pic, exeDevicePath, exeDevicePath.Capacity) == 0) { 
                return null;
            }
            var exeName = Path.GetFileName(exeDevicePath.ToString());

            return exeName;
        }

        private bool IsWindowDefaultBrowser(IntPtr hwnd, SettingValues.BrowserInfo browserInfo) {
            return (browserInfo != null && GetWindowExeName(hwnd) == browserInfo.ExeName);
        }

        private void ToggleWindowSticky(IntPtr hwnd) {
            WinAPI.SetWindowLongPtr(hwnd, WinAPI.GWL_EXSTYLE,
              WinAPI.GetWindowLongPtr(hwnd, WinAPI.GWL_EXSTYLE).XOR(WinAPI.WS_EX_TOOLWINDOW));
        }

        private static bool IsWindowTopMost(IntPtr hWnd) {
            return WinAPI.GetWindowLongPtr(hWnd, WinAPI.GWL_EXSTYLE).AND(WinAPI.WS_EX_TOPMOST) == WinAPI.WS_EX_TOPMOST;
        }

        private void ToggleWindowAlwaysOnTop(IntPtr hwnd) {
            WinAPI.SetWindowPos(hwnd,
              IsWindowTopMost(hwnd) ? WinAPI.HWND_NOTOPMOST : WinAPI.HWND_TOPMOST,
              0, 0, 0, 0,
              WinAPI.SWPFlags.SWP_SHOWWINDOW | WinAPI.SWPFlags.SWP_NOSIZE | WinAPI.SWPFlags.SWP_NOMOVE);
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
            activeWindows[Current] = WinAPI.GetForegroundWindow();
            Debug.WriteLine("Switch Active " + Current + " " + activeWindows[Current]);
            Current = index;
        }

        public void Move(int index)
        {
            var hwnd = WinAPI.GetForegroundWindow();
            if (hwnd != IntPtr.Zero) {
                if (IsWindowDefaultBrowser(hwnd, settings.GetBrowserToActivateInfo())) {
                    for (int i = 0; i < lastActiveBrowserWindows.Length; ++i) {
                        if (lastActiveBrowserWindows[i] == hwnd) {
                            Debug.WriteLine("Browser " + i + " cleared");
                            lastActiveBrowserWindows[i] = IntPtr.Zero;
                        }
                        if (activeWindows[i] == hwnd) {
                            activeWindows[i] = IntPtr.Zero;
                        }
                    }
                }
                Debug.WriteLine("Move " + hwnd + " from " + Current + " to " + index);
                if (!VirtualDesktopHelper.MoveToDesktop(hwnd, desktops[index])) {
                    this.VDMHelper.MoveWindowToDesktop(hwnd, desktops[index].Id);
                }
            }
            WinAPI.SetForegroundWindow(hwnd);
            Current = index;
            activeWindows[index] = hwnd;
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

            RegisterToggleStickyHotKey();
            RegisterToggleAlwaysOnTopHotKey();
        }

        private void RegisterSwitchHotkey(Keys keycode, Action action) {
            Hotkey hk = new Hotkey() {
                Control = settings.SwitchModifiers.Ctrl,
                Windows = settings.SwitchModifiers.Win,
                Alt = settings.SwitchModifiers.Alt,
                Shift = settings.SwitchModifiers.Shift,
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
                Control = settings.MoveModifiers.Ctrl,
                Windows = settings.MoveModifiers.Win,
                Alt = settings.MoveModifiers.Alt,
                Shift = settings.MoveModifiers.Shift,
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

        private void RegisterToggleStickyHotKey() {
            Hotkey hk = new Hotkey() {
                Control = settings.StickyWindowHotKey.Modifiers.Ctrl,
                Windows = settings.StickyWindowHotKey.Modifiers.Win,
                Alt = settings.StickyWindowHotKey.Modifiers.Alt,
                Shift = settings.StickyWindowHotKey.Modifiers.Shift,
                KeyCode = settings.StickyWindowHotKey.Key
            };
            hk.Pressed += delegate { ToggleWindowSticky(WinAPI.GetForegroundWindow()); };
            if (hk.Register(null)) {
                hotkeys.Add(hk);
            } else {
                MessageBox.Show("Failed to register toggle sticky window hotkey for " + hk.KeyCode,
                                "Warning",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
            }
        }

        private void RegisterToggleAlwaysOnTopHotKey() {
            Hotkey hk = new Hotkey() {
                Control = settings.AlwaysOnTopHotkey.Modifiers.Ctrl,
                Windows = settings.AlwaysOnTopHotkey.Modifiers.Win,
                Alt = settings.AlwaysOnTopHotkey.Modifiers.Alt,
                Shift = settings.AlwaysOnTopHotkey.Modifiers.Shift,
                KeyCode = settings.AlwaysOnTopHotkey.Key
            };
            hk.Pressed += delegate { ToggleWindowAlwaysOnTop(WinAPI.GetForegroundWindow()); };
            if (hk.Register(null)) {
                hotkeys.Add(hk);
            } else {
                MessageBox.Show("Failed to register toggle window always on top hotkey for " + hk.KeyCode,
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

            var activeHWnd = WinAPI.GetForegroundWindow();
            if (IsKeyPressed(WinAPI.GetAsyncKeyState(VK_MENU))) {
                WinAPI.PostMessage(activeHWnd, WM_KEYUP, (IntPtr)VK_MENU, (IntPtr)0xC0380001);
                WinAPI.PostMessage(activeHWnd, WM_KEYUP, (IntPtr)VK_MENU, (IntPtr)0xC1380001);
            }
            if (IsKeyPressed(WinAPI.GetAsyncKeyState(VK_CONTROL))) {
                WinAPI.PostMessage(activeHWnd, WM_KEYUP, (IntPtr)VK_CONTROL, (IntPtr)0xC01D0001);
                WinAPI.PostMessage(activeHWnd, WM_KEYUP, (IntPtr)VK_CONTROL, (IntPtr)0xC11D0001);
            }
            if (IsKeyPressed(WinAPI.GetAsyncKeyState(VK_SHIFT))) {
                WinAPI.PostMessage(activeHWnd, WM_KEYUP, (IntPtr)VK_SHIFT, (IntPtr)0xC02A0001);
                WinAPI.PostMessage(activeHWnd, WM_KEYUP, (IntPtr)VK_SHIFT, (IntPtr)0xC0360001);
            }
            if (IsKeyPressed(WinAPI.GetAsyncKeyState(VK_LWIN))) {
                WinAPI.PostMessage(activeHWnd, WM_KEYUP, (IntPtr)VK_LWIN, (IntPtr)0xC15B0001);
            }
            if (IsKeyPressed(WinAPI.GetAsyncKeyState(VK_RWIN))) {
                WinAPI.PostMessage(activeHWnd, WM_KEYUP, (IntPtr)VK_RWIN, (IntPtr)0xC15C0001);
            }
        }

        private bool IsKeyPressed(short keystate) {
            return (keystate & 0x8000) != 0;
        }
    }

}
