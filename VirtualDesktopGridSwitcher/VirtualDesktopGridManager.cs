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

namespace VirtualDesktopGridSwitcher {

    class VirtualDesktopGridManager: IDisposable {

        private SettingValues settings;
        private SysTrayProcess sysTrayProcess;

        private IVdmHelper VDMHelper;

        private Dictionary<VirtualDesktop, int> desktopIdLookup;
        private VirtualDesktop[] desktops;
        private IntPtr[] lastActiveBrowserWindows;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindow(IntPtr hWnd);

        public delegate bool CallBackPtr(IntPtr hwnd, int lParam);

        [DllImport("user32.dll")]
        private static extern int EnumWindows(CallBackPtr callPtr, int lPar);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);

        enum GWCMD : uint {
            GW_HWNDFIRST = 0,
            GW_HWNDLAST = 1,
            GW_HWNDNEXT = 2,
            GW_HWNDPREV = 3,
            GW_OWNER = 4,
            GW_CHILD = 5,
            GW_ENABLEDPOPUP = 6
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr GetWindow(IntPtr hWnd, GWCMD uCmd);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern IntPtr SetForegroundWindow(IntPtr hWnd);

        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        static readonly IntPtr HWND_TOP = new IntPtr(0);
        static readonly IntPtr HWND_BOTTOM = new IntPtr(1);

        [Flags()]
        private enum SWPFlags : uint {
            SWP_ASYNCWINDOWPOS = 0x4000,
            SWP_DEFERERASE = 0x2000,
            SWP_DRAWFRAME = 0x0020,
            SWP_FRAMECHANGED = 0x0020,
            SWP_HIDEWINDOW = 0x0080,
            SWP_NOACTIVATE = 0x0010,
            SWP_NOCOPYBITS = 0x0100,
            SWP_NOMOVE = 0x0002,
            SWP_NOOWNERZORDER = 0x0200,
            SWP_NOREDRAW = 0x0008,
            SWP_NOREPOSITION = 0x0200,
            SWP_NOSENDCHANGING = 0x0400,
            SWP_NOSIZE = 0x0001,
            SWP_NOZORDER = 0x0004,
            SWP_SHOWWINDOW = 0x0040,
        }

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, SWPFlags uFlags);

        public enum ShowWindowCommands : int {
            /// <summary>
            /// Hides the window and activates another window.
            /// </summary>
            Hide = 0,
            /// <summary>
            /// Activates and displays a window. If the window is minimized or 
            /// maximized, the system restores it to its original size and position.
            /// An application should specify this flag when displaying the window 
            /// for the first time.
            /// </summary>
            Normal = 1,
            /// <summary>
            /// Activates the window and displays it as a minimized window.
            /// </summary>
            ShowMinimized = 2,
            /// <summary>
            /// Maximizes the specified window.
            /// </summary>
            Maximize = 3, // is this the right value?
                          /// <summary>
                          /// Activates the window and displays it as a maximized window.
                          /// </summary>       
            ShowMaximized = 3,
            /// <summary>
            /// Displays a window in its most recent size and position. This value 
            /// is similar to <see cref="Win32.ShowWindowCommand.Normal"/>, except 
            /// the window is not activated.
            /// </summary>
            ShowNoActivate = 4,
            /// <summary>
            /// Activates the window and displays it in its current size and position. 
            /// </summary>
            Show = 5,
            /// <summary>
            /// Minimizes the specified window and activates the next top-level 
            /// window in the Z order.
            /// </summary>
            Minimize = 6,
            /// <summary>
            /// Displays the window as a minimized window. This value is similar to
            /// <see cref="Win32.ShowWindowCommand.ShowMinimized"/>, except the 
            /// window is not activated.
            /// </summary>
            ShowMinNoActive = 7,
            /// <summary>
            /// Displays the window in its current size and position. This value is 
            /// similar to <see cref="Win32.ShowWindowCommand.Show"/>, except the 
            /// window is not activated.
            /// </summary>
            ShowNA = 8,
            /// <summary>
            /// Activates and displays the window. If the window is minimized or 
            /// maximized, the system restores it to its original size and position. 
            /// An application should specify this flag when restoring a minimized window.
            /// </summary>
            Restore = 9,
            /// <summary>
            /// Sets the show state based on the SW_* value specified in the 
            /// STARTUPINFO structure passed to the CreateProcess function by the 
            /// program that started the application.
            /// </summary>
            ShowDefault = 10,
            /// <summary>
            ///  <b>Windows 2000/XP:</b> Minimizes a window, even if the thread 
            /// that owns the window is not responding. This flag should only be 
            /// used when minimizing windows from a different thread.
            /// </summary>
            ForceMinimize = 11
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ShowWindow(IntPtr hWnd, ShowWindowCommands nCmdShow);

        /// <summary>
        /// Contains information about the placement of a window on the screen.
        /// </summary>
        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public struct WindowPlacement {
            /// <summary>
            /// The length of the structure, in bytes. Before calling the GetWindowPlacement or SetWindowPlacement functions, set this member to sizeof(WINDOWPLACEMENT).
            /// <para>
            /// GetWindowPlacement and SetWindowPlacement fail if this member is not set correctly.
            /// </para>
            /// </summary>
            public int Length;

            /// <summary>
            /// Specifies flags that control the position of the minimized window and the method by which the window is restored.
            /// </summary>
            public int Flags;

            /// <summary>
            /// The current show state of the window.
            /// </summary>
            public ShowWindowCommands ShowCmd;

            /// <summary>
            /// The coordinates of the window's upper-left corner when the window is minimized.
            /// </summary>
            public Point MinPosition;

            /// <summary>
            /// The coordinates of the window's upper-left corner when the window is maximized.
            /// </summary>
            public Point MaxPosition;

            /// <summary>
            /// The window's coordinates when the window is in the restored position.
            /// </summary>
            public Rectangle NormalPosition;

            /// <summary>
            /// Gets the default (empty) value.
            /// </summary>
            public static WindowPlacement Default {
                get {
                    WindowPlacement result = new WindowPlacement();
                    result.Length = Marshal.SizeOf(result);
                    return result;
                }
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetWindowPlacement(IntPtr hWnd, ref WindowPlacement lpwndpl);

        [DllImport("user32.dll", SetLastError= true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [Flags]
        public enum ProcessAccessFlags : uint {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VirtualMemoryOperation = 0x00000008,
            VirtualMemoryRead = 0x00000010,
            VirtualMemoryWrite = 0x00000020,
            DuplicateHandle = 0x00000040,
            CreateProcess = 0x000000080,
            SetQuota = 0x00000100,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            QueryLimitedInformation = 0x00001000,
            Synchronize = 0x00100000
        }

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(ProcessAccessFlags processAccess, bool bInheritHandle, int processId);

        [DllImport("psapi.dll")]
        static extern uint GetProcessImageFileName(IntPtr hProcess, [Out] StringBuilder lpImageFileName, [In] [MarshalAs(UnmanagedType.U4)] int nSize);

        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(int vKey); 

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        private const uint WINEVENT_OUTOFCONTEXT = 0;
        private const uint EVENT_SYSTEM_FOREGROUND = 3;

        [DllImport("user32.dll")]
        static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

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

                var browserInfo = settings.GetBrowserToActivateInfo();
                if (browserInfo != null) {
                    var currentHwnd = GetForegroundWindow();
                    if (lastActiveBrowserWindows[Current] != currentHwnd) {
                        if (FindActivateBrowserWindow(lastActiveBrowserWindows[Current], browserInfo)) {
                            Debug.WriteLine("Reactivate " + Current + " " + currentHwnd);
                            SetForegroundWindow(currentHwnd);
                        }
                    }
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
            if (IsWindow(hwnd)) {
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
                var window = FindWindowEx(IntPtr.Zero, browserWinInfo.last, browserInfo.ClassName, null);
                notFinished = IterateFindTopLevelBrowserOnCurrentDesktop(window, browserWinInfo, browserInfo);
            } while (notFinished);
            if (browserWinInfo.topOnDesktop != IntPtr.Zero) {
                Debug.WriteLine("Activate Unknown Browser " + Current + " " + browserWinInfo.topOnDesktop);
                ActivateBrowserWindow(browserWinInfo.topOnDesktop);
                return true;
            }

            return false;
        }

        private static void ActivateBrowserWindow(IntPtr hwnd) {
            WindowPlacement winInfo = new WindowPlacement();
            GetWindowPlacement(hwnd, ref winInfo);

            SetForegroundWindow(hwnd);
            //SetWindowPos(hwnd, HWND_TOP, 0, 0, 0, 0, SWPFlags.SWP_NOMOVE | SWPFlags.SWP_NOSIZE | SWPFlags.SWP_SHOWWINDOW);
            if (winInfo.ShowCmd == ShowWindowCommands.ShowMinimized) {
                ShowWindow(hwnd, ShowWindowCommands.Restore);
                //System.Threading.Thread.Sleep(1000);
                ShowWindow(hwnd, winInfo.ShowCmd);
            }
        }

        void ForegroundWindowChanged(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime) {
            if (desktops != null)
            {
                var desktopId = Current;
                var desktop = VirtualDesktop.FromHwnd(hwnd);
                if (desktop != null) {
                    desktopId = desktopIdLookup[desktop];
                }
                Debug.WriteLine("Foreground " + Current + " " + (desktop != null ? desktopIdLookup[desktop].ToString() : "?") + " " + hwnd);

                if (IsWindowDefaultBrowser(hwnd, settings.GetBrowserToActivateInfo())) {
                    Debug.WriteLine("Browser " + Current + " " + desktopIdLookup[VirtualDesktop.Current] + " " + desktopId + " " + hwnd);
                    lastActiveBrowserWindows[desktopId] = hwnd;
                }
            }
            //ReleaseModifierKeys();
        }

        private static string GetWindowExeName(IntPtr hwnd) {
            uint pid = 0;
            GetWindowThreadProcessId(hwnd, out pid);

            IntPtr pic = OpenProcess(ProcessAccessFlags.All, true, (int)pid);

            StringBuilder exeDevicePath = new StringBuilder(1024);
            GetProcessImageFileName(pic, exeDevicePath, exeDevicePath.Capacity);
            var exeName = Path.GetFileName(exeDevicePath.ToString());

            return exeName;
        }

        private bool IsWindowDefaultBrowser(IntPtr hwnd, SettingValues.BrowserInfo browserInfo) {
            return (browserInfo != null && GetWindowExeName(hwnd) == browserInfo.ExeName);
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

        public void Move(int index)
        {
            var hwnd = GetForegroundWindow();
            if (hwnd != IntPtr.Zero) {
                if (IsWindowDefaultBrowser(hwnd, settings.GetBrowserToActivateInfo())) {
                    for (int i = 0; i < lastActiveBrowserWindows.Length; ++i) {
                        if (lastActiveBrowserWindows[i] == hwnd) {
                            Debug.WriteLine("Browser " + i + " cleared");
                            lastActiveBrowserWindows[i] = IntPtr.Zero;
                        }
                    }
                }
                Debug.WriteLine("Move " + hwnd + " from " + Current + " to " + index);
                if (VirtualDesktopHelper.MoveToDesktop(hwnd, desktops[index])) {
                    this.VDMHelper.MoveWindowToDesktop(hwnd, desktops[index].Id);
                }
            }
            Current = index;
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
