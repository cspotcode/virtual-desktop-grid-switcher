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

    class VirtualDesktopGridManager: IDisposable, IMessageFilter {

        #region Interop
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern uint RegisterWindowMessage(string lpProcName);
        #endregion

        private uint commandWindowMessage;

        private SettingValues settings;
        private SysTrayProcess sysTrayProcess;

        private IVdmHelper VDMHelper;

        private Dictionary<VirtualDesktop, int> desktopIdLookup;
        private VirtualDesktop[] desktops;
        private IntPtr[] activeWindows;
        private IntPtr[] lastActiveBrowserWindows;
        private IntPtr lastMoveOnNewWindowHwnd;
        private int lastMoveOnNewWindowOpenedFromDesktop;
        private DateTime lastMoveOnNewWindowOpenedTime = DateTime.MinValue;

        private IntPtr movingWindow = IntPtr.Zero;

        private WinAPI.WinEventDelegate foregroundWindowChangedDelegate;
        private IntPtr fgWindowHook;

        Mutex callbackMutex = new Mutex();

        public VirtualDesktopGridManager(SysTrayProcess sysTrayProcess, SettingValues settings)
        {
            this.settings = settings;
            this.sysTrayProcess = sysTrayProcess;

            this.VDMHelper = VdmHelperFactory.CreateInstance();
            this.VDMHelper.Init();

            foregroundWindowChangedDelegate = new WinAPI.WinEventDelegate(ForegroundWindowChanged);
            fgWindowHook = WinAPI.SetWinEventHook(WinAPI.EVENT_SYSTEM_FOREGROUND, WinAPI.EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, foregroundWindowChangedDelegate, 0, 0, WinAPI.WINEVENT_OUTOFCONTEXT);

            // Create a custom message ID for other processes to trigger actions.
            this.commandWindowMessage = RegisterWindowMessage("VIRTUALDESKTOPGRIDSWITCHER_COMMAND");
            Application.AddMessageFilter(this);

            Start();
        }

        public void Dispose()
        {
            Stop();

            this.VDMHelper.Dispose();

            WinAPI.UnhookWinEvent(fgWindowHook);
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
            lock (callbackMutex) {

                var newDesktop = desktopIdLookup[VirtualDesktop.Current];
                Debug.WriteLine("Switched to " + newDesktop);

                this._current = newDesktop;
                sysTrayProcess.ShowIconForDesktop(Current);

                var fgHwnd = WinAPI.GetForegroundWindow();
                var lastActiveWindow = activeWindows[Current];

                var browserInfo = settings.GetBrowserToActivateInfo();
                if (browserInfo != null) {
                    if (lastActiveBrowserWindows[Current] != activeWindows[Current]) {
                        FindActivateBrowserWindow(lastActiveBrowserWindows[Current], browserInfo);
                    }
                }

                if (!ActivateWindow(lastActiveWindow)) {
                    Debug.WriteLine("Reactivate " + Current + " " + fgHwnd);
                    WinAPI.SetForegroundWindow(fgHwnd);
                }

                movingWindow = IntPtr.Zero;
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

            //var prevHwnd = hwnd;
            //do {
            //    prevHwnd = WinAPI.GetWindow(prevHwnd, WinAPI.GWCMD.GW_HWNDPREV);
            //} while (prevHwnd != IntPtr.Zero && VirtualDesktop.FromHwnd(prevHwnd) != desktops[Current]);

            WinAPI.SetForegroundWindow(hwnd);
            if (winInfo.ShowCmd == WinAPI.ShowWindowCommands.ShowMinimized) {
                WinAPI.ShowWindow(hwnd, WinAPI.ShowWindowCommands.Restore);
                //System.Threading.Thread.Sleep(1000);
                WinAPI.ShowWindow(hwnd, winInfo.ShowCmd);
            }

            //if (prevHwnd != IntPtr.Zero) {
            //    Debug.WriteLine("Browser behind " + prevHwnd + " " + GetWindowExeName(prevHwnd) + " " + GetWindowTitle(prevHwnd));
            //    WinAPI.SetWindowPos(hwnd, prevHwnd, 0, 0, 0, 0, WinAPI.SWPFlags.SWP_NOMOVE | WinAPI.SWPFlags.SWP_NOSIZE | WinAPI.SWPFlags.SWP_NOACTIVATE);
            //}
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

        void ForegroundWindowChanged(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime) {
            lock (callbackMutex) {

                if (desktops != null) {

                    var windowDesktopId = Current;
                    var windowDesktop = VirtualDesktop.FromHwnd(hwnd);
                    if (windowDesktop != null) {
                        windowDesktopId = desktopIdLookup[windowDesktop];
                    }

                    if (IsMoveOnNewWindowType(hwnd)) {
                        if (windowDesktopId != Current) {
                            Debug.WriteLine("Opened MoveOnNewWindow from " + Current);
                            lastMoveOnNewWindowHwnd = hwnd;
                            lastMoveOnNewWindowOpenedFromDesktop = Current;
                            lastMoveOnNewWindowOpenedTime = DateTime.Now;
                        } else {
                            var delay = (DateTime.Now - lastMoveOnNewWindowOpenedTime).TotalMilliseconds;
                            if (delay < settings.MoveOnNewWindowDetectTimeoutMs &&
                                lastMoveOnNewWindowOpenedFromDesktop != Current) {

                                // work around browser activtation
                                if (lastMoveOnNewWindowHwnd == hwnd) {
                                    Debug.WriteLine((int)delay + " Reset Move New Window Timeout");
                                    lastMoveOnNewWindowOpenedTime = DateTime.Now;
                                    return;
                                } else {

                                    lastMoveOnNewWindowHwnd = IntPtr.Zero;
                                    lastMoveOnNewWindowOpenedTime = DateTime.MinValue;

                                    Debug.WriteLine((int)delay + " Move New Window " + hwnd + " to " + lastMoveOnNewWindowOpenedFromDesktop);
                                    MoveWindow(hwnd, lastMoveOnNewWindowOpenedFromDesktop);

                                    return;
                                }
                            } else if (lastMoveOnNewWindowOpenedTime != DateTime.MinValue) {
                                Debug.WriteLine("Timeout New Window Move " + delay);
                                lastMoveOnNewWindowOpenedTime = DateTime.MinValue;
                            }
                        }
                    }

                    if (windowDesktop != null && movingWindow == IntPtr.Zero) {
                        activeWindows[windowDesktopId] = hwnd;
                    }

                    Debug.WriteLine("Foreground " + Current + " " + (windowDesktop != null ? windowDesktopId.ToString() : "?") + " " + hwnd + " " + GetWindowTitle(hwnd));

                    if (IsWindowDefaultBrowser(hwnd, settings.GetBrowserToActivateInfo())) {
                        Debug.WriteLine("Browser " + Current + " " + desktopIdLookup[VirtualDesktop.Current] + " " + windowDesktopId + " " + hwnd);
                        lastActiveBrowserWindows[windowDesktopId] = hwnd;
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

        private bool IsMoveOnNewWindowType(IntPtr hwnd) {
            return (
                settings.MoveOnNewWindowExeNames != null &&
                settings.MoveOnNewWindowExeNames.Contains(GetWindowExeName(hwnd)));
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
                    _current = value;
                    desktops[value].Switch();
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

        public void Move(int index) {
            var hwnd = WinAPI.GetForegroundWindow();
            if (hwnd != IntPtr.Zero) {
                if (IsWindowDefaultBrowser(hwnd, settings.GetBrowserToActivateInfo())) {
                    for (int i = 0; i < lastActiveBrowserWindows.Length; ++i) {
                        if (lastActiveBrowserWindows[i] == hwnd) {
                            Debug.WriteLine("Browser " + i + " cleared");
                            lastActiveBrowserWindows[i] = IntPtr.Zero;
                        }
                    }
                }
            }

            MoveWindow(hwnd, index);
        }

        private void MoveWindow(IntPtr hwnd, int index) {
            if (hwnd != IntPtr.Zero) {
                for (int i = 0; i < activeWindows.Length; ++i) {
                    if (activeWindows[i] == hwnd) {
                        activeWindows[i] = IntPtr.Zero;
                    }
                }

                movingWindow = hwnd;

                Debug.WriteLine("Move " + hwnd + " from " + Current + " to " + index);
                if (!VirtualDesktopHelper.MoveToDesktop(hwnd, desktops[index])) {
                    this.VDMHelper.MoveWindowToDesktop(hwnd, desktops[index].Id);
                }

                activeWindows[index] = hwnd;
                WinAPI.SetForegroundWindow(hwnd);
                Current = index;
            }
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

        public bool PreFilterMessage(ref Message message)
        {
            // Ignore unknown messages
            if (message.Msg != this.commandWindowMessage)
            { return false; }

            int command = message.WParam.ToInt32();
            switch(command)
            {
                case GO_UP:
                    Switch(Up);
                    break;
                case GO_LEFT:
                    Switch(Left);
                    break;
                case GO_RIGHT:
                    Switch(Right);
                    break;
                case GO_DOWN:
                    Switch(Down);
                    break;
                case MOVE_UP:
                    Move(Up);
                    break;
                case MOVE_LEFT:
                    Move(Left);
                    break;
                case MOVE_RIGHT:
                    Move(Right);
                    break;
                case MOVE_DOWN:
                    Move(Down);
                    break;
                case TOGGLE_ALWAYS_ON_TOP:
                    ToggleWindowAlwaysOnTop(WinAPI.GetForegroundWindow());
                    break;
                case TOGGLE_STICKY:
                    ToggleWindowSticky(WinAPI.GetForegroundWindow());
                    break;
                default:
                    // Do nothing
                    break;
            }
            return true;
        }

        const int GO_UP = 1;
        const int GO_LEFT = 2;
        const int GO_RIGHT = 3;
        const int GO_DOWN = 4;
        const int MOVE_UP = 5;
        const int MOVE_LEFT = 6;
        const int MOVE_RIGHT = 7;
        const int MOVE_DOWN = 8;
        const int TOGGLE_ALWAYS_ON_TOP = 9;
        const int TOGGLE_STICKY = 10;
    }
}
