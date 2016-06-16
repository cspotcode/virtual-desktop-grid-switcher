using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
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

        /** When this is not zero, certain actions will send a message to this hwnd */
        private IntPtr hwndMessageTarget = IntPtr.Zero;

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

            VDMHelper = VdmHelperFactory.CreateInstance();
            VDMHelper.Init();

            foregroundWindowChangedDelegate = new WinAPI.WinEventDelegate(ForegroundWindowChanged);
            fgWindowHook = WinAPI.SetWinEventHook(WinAPI.EVENT_SYSTEM_FOREGROUND, WinAPI.EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, foregroundWindowChangedDelegate, 0, 0, WinAPI.WINEVENT_OUTOFCONTEXT);

            // Create a custom message ID for other processes to trigger actions.
            commandWindowMessage = RegisterWindowMessage("VIRTUALDESKTOPGRIDSWITCHER_COMMAND");
            Application.AddMessageFilter(this);

            Start();
        }

        public void Dispose()
        {
            Stop();

            VDMHelper.Dispose();

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

                _current = desktopIdLookup[VirtualDesktop.Current];

                activeWindows = new IntPtr[desktops.Length];
                lastActiveBrowserWindows = new IntPtr[desktops.Length];

                VirtualDesktop.CurrentChanged += VirtualDesktop_CurrentChanged;
            } catch { }

            sysTrayProcess.ShowIconForDesktop(Current);

            settings.Apply += Restart;

            return true;
        }

        private void Stop() {
            settings.Apply -= Restart;
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

                _current = newDesktop;
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

                SendSwitchedDesktopMessage();
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
                if (desktop != null && desktopIdLookup[desktop] == _current) {
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
                                    MoveWindowAndSwitchDesktop(hwnd, lastMoveOnNewWindowOpenedFromDesktop);

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
            if(stickyWindows.Contains(hwnd)) {
                stickyWindows.Remove(hwnd);
            }
            else {
                stickyWindows.Add(hwnd);
            }
        }

        private static bool IsWindowTopMost(IntPtr hWnd) {
            return WinAPI.GetWindowLongPtr(hWnd, WinAPI.GWL_EXSTYLE).AND(WinAPI.WS_EX_TOPMOST) == WinAPI.WS_EX_TOPMOST;
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

        private int AdjacentDesktop(int start, int columnDelta, int rowDelta) {
            int targetColumn = ColumnOf(start) + columnDelta;
            int targetRow = RowOf(start) + rowDelta;
            if (settings.WrapAround) {
                if (targetColumn < 0) {
                    targetColumn += settings.Columns;
                }
                if (targetColumn >= settings.Columns) {
                    targetColumn -= settings.Columns;
                }
                if (targetRow < 0) {
                    targetRow += settings.Rows;
                }
                if (targetRow >= settings.Rows) {
                    targetRow -= settings.Rows;
                }
            }
            else {
                if (targetColumn < 0 || targetColumn >= settings.Columns || targetRow < 0 || targetRow >= settings.Rows) {
                    return start;
                }
            }
            return targetRow * settings.Columns + targetColumn;
        }

        public int Left {
            get { return AdjacentDesktop(Current, -1, 0); }
        }

        public int Right {
            get { return AdjacentDesktop(Current, 1, 0); }
        }

        public int Up {
            get { return AdjacentDesktop(Current, 0, -1); }
        }

        public int Down {
            get { return AdjacentDesktop(Current, 0, 1); }
        }

        private void SendSwitchedDesktopMessage() {
            if (hwndMessageTarget != IntPtr.Zero) {
                WinAPI.PostMessage(hwndMessageTarget, commandWindowMessage, (IntPtr)SWITCHED_DESKTOP, (IntPtr)Current);
            }
        }

        public void Switch(int index) {
            activeWindows[Current] = WinAPI.GetForegroundWindow();
            Debug.WriteLine("Switch Active " + Current + " " + activeWindows[Current]);

            MoveStickyWindows(index);

            Current = index;
        }

        /// <summary>
        /// Move a window to a desktop and switch to that desktop
        /// </summary>
        public void Move(IntPtr hwnd, int index) {
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
            MoveWindowAndSwitchDesktop(hwnd, index);
        }

        /// <summary>
        /// Move the foreground window to a desktop and switch to that desktop
        /// </summary>
        public void MoveForeground(int index) {
            var hwnd = WinAPI.GetForegroundWindow();
            Move(hwnd, index);
        }

        private void MoveWindowAndSwitchDesktop(IntPtr hwnd, int index) {
            if (hwnd != IntPtr.Zero) {
                MoveStickyWindows(index);
                MoveWindow(hwnd, index);

                activeWindows[index] = hwnd;
                WinAPI.SetForegroundWindow(hwnd);
                Current = index;
            }
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
                    VDMHelper.MoveWindowToDesktop(hwnd, desktops[index].Id);
                }
            }
        }

        private void MoveStickyWindows(int index) {
            stickyWindows.RemoveWhere(stickyWindow => {
                // MoveWindow doesn't throw an error when window no longer exists.
                MoveWindow(stickyWindow, index);
                // Remove windows that no longer exist.
                return !WinAPI.IsWindow(stickyWindow);
            });
        }

        /// <summary>
        /// Returns the column of a desktop (leftmost column = 0)
        /// </summary>
        private int ColumnOf(int index) {
            return index % settings.Columns;
        }

        /// <summary>
        /// Returns the row of a desktop (top row = 0)
        /// </summary>
        private int RowOf(int index) {
            return index / settings.Columns;
        }

        private HashSet<IntPtr> stickyWindows = new HashSet<IntPtr>();

        public bool PreFilterMessage(ref Message message)
        {
            // Ignore unknown messages
            if (message.Msg != commandWindowMessage)
            { return false; }

            int command = message.WParam.ToInt32();
            int value = message.LParam.ToInt32();
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
                case GO_TO:
                    Switch(value);
                    break;
                case MOVE_UP:
                    MoveForeground(Up);
                    break;
                case MOVE_LEFT:
                    MoveForeground(Left);
                    break;
                case MOVE_RIGHT:
                    MoveForeground(Right);
                    break;
                case MOVE_DOWN:
                    MoveForeground(Down);
                    break;
                case MOVE_TO:
                    MoveForeground(value);
                    break;
                case TOGGLE_STICKY:
                    ToggleWindowSticky(WinAPI.GetForegroundWindow());
                    break;
                case DEBUG_SHOW_CURRENT_WINDOW_HWND:
                    var hwnd = WinAPI.GetForegroundWindow();
                    MessageBox.Show("Current window is: " + hwnd);
                    break;
                case SET_HWND_MESSAGE_TARGET:
                    hwndMessageTarget = message.LParam;
                    break;
                case QUIT:
                    Application.Exit();
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
        const int TOGGLE_STICKY = 10;
        const int DEBUG_SHOW_CURRENT_WINDOW_HWND = 11;
        const int SET_HWND_MESSAGE_TARGET = 12;
        const int SWITCHED_DESKTOP = 13;
        const int QUIT = 14;
        const int GO_TO = 15;
        const int MOVE_TO = 16;
    }
}
