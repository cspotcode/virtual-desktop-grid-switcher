; Uncomment the next line if you don't want this script appearing in your system tray
;#NoTrayIcon
#SingleInstance Force
#Persistent
Initialize()
return

;;;;;;;;;;;
; Desktop names.  Will appear in the overlay while you switch desktops.
DesktopNames() {
    global
    desktopName := Object()
    ; Example: desktopName[4] := "Work Documents"
    desktopName[0] := ""
    desktopName[1] := ""
    desktopName[2] := ""
    desktopName[3] := ""
    desktopName[4] := ""
    desktopName[5] := ""
    desktopName[6] := ""
    desktopName[7] := ""
    desktopName[8] := ""
}

Options() {
    global
    overlayEnabled := 1 ; 1 = enabled, 0 = disabled
    hideOverlayDelay := 1000 ; milliseconds for overlay to remain visible before auto-hiding
}

;;;;;;;
; Hotkeys.  Customize these to your liking.

^#Left::SendCommand(GO_LEFT)
^#Right::SendCommand(GO_RIGHT)
^#Down::SendCommand(GO_DOWN)
^#Up::SendCommand(GO_UP)
#!Left::SendCommand(MOVE_LEFT)
#!Right::SendCommand(MOVE_RIGHT)
#!Down::SendCommand(MOVE_DOWN)
#!Up::SendCommand(MOVE_UP)
^#F1::SendCommand(GO_TO, 0)
^#F2::SendCommand(GO_TO, 1)
^#F3::SendCommand(GO_TO, 2)
^#F4::SendCommand(GO_TO, 3)
^#F5::SendCommand(GO_TO, 4)
^#F6::SendCommand(GO_TO, 5)
^#F7::SendCommand(GO_TO, 6)
^#F8::SendCommand(GO_TO, 7)
^#F9::SendCommand(GO_TO, 8)
^#F10::SendCommand(GO_TO, 9)
^#F11::SendCommand(GO_TO, 10)
^#F12::SendCommand(GO_TO, 11)
#!F1::SendCommand(MOVE_TO, 0)
#!F2::SendCommand(MOVE_TO, 1)
#!F3::SendCommand(MOVE_TO, 2)
#!F4::SendCommand(MOVE_TO, 3)
#!F5::SendCommand(MOVE_TO, 4)
#!F6::SendCommand(MOVE_TO, 5)
#!F7::SendCommand(MOVE_TO, 6)
#!F8::SendCommand(MOVE_TO, 7)
#!F9::SendCommand(MOVE_TO, 8)
#!F10::SendCommand(MOVE_TO, 9)
#!F11::SendCommand(MOVE_TO, 10)
#!F12::SendCommand(MOVE_TO, 11)
^#A::ToggleAlwaysOnTop()
^#S::SendCommand(TOGGLE_STICKY)
^#D::SendCommand(DEBUG_SHOW_CURRENT_WINDOW_HWND)
#If overlayVisible
; When either Ctrl or Win is released, the overlay is hidden.
~*Ctrl Up::HideOverlay()
~*LWin Up::HideOverlay()
#If

; Called once when the script is launched.
Initialize() {
    global
    CommandConstants()
    DesktopNames()
    Options()

    MsgNum := DllCall("RegisterWindowMessage", Str, "VIRTUALDESKTOPGRIDSWITCHER_COMMAND")

    exeName := "VirtualDesktopGridSwitcher.exe"
    ; Use this when running via Visual Studio's debugger
    ;exeName := "VirtualDesktopGridSwitcher.vshost.exe"

    ; Launch VirtualDesktopGridManager if it's not already running.
    DetectHiddenWindows, On
    If(!WinExist("ahk_exe " . exeName)) {
        Run "../VirtualDesktopGridSwitcher.exe"
        While(GetTargetHwnd() == "") {
            Sleep, 50
        }
    }

    ; Run function when this script is exited
    OnExit("OnExitFn")

    overlayVisible := 0

    ; Register listener to receive messages from .exe
    OnMessage(MsgNum, "ReceiveMessage")
    myHwnd := HexToDec(A_ScriptHwnd)
    SendCommand(SET_HWND_MESSAGE_TARGET, myHwnd)
}

CommandConstants() {
    global
    ; All commands
    GO_UP := 1
    GO_LEFT := 2
    GO_RIGHT := 3
    GO_DOWN := 4
    MOVE_UP := 5
    MOVE_LEFT := 6
    MOVE_RIGHT := 7
    MOVE_DOWN := 8
    TOGGLE_STICKY := 10
    DEBUG_SHOW_CURRENT_WINDOW_HWND := 11
    SET_HWND_MESSAGE_TARGET := 12
    SWITCHED_DESKTOP := 13
    QUIT := 14
    GO_TO := 15
    MOVE_TO := 16
}

; Return an HWND that will receive our commands.
GetTargetHwnd() {
    global exeName
    DetectHiddenWindows, On
    WinGet, a, List, ahk_exe %exeName%
    Loop, %a% {
        id := a%A_Index%
        WinGetClass, cls, ahk_id %id%
        ; Skip the "GDI+" window
        If(RegExMatch(cls, "^GDI") == 0) {
            return id
        }
    }
}

; Send a command to VirtualDesktopGridSwitcher
SendCommand(cmd, value := 0) {
    ; All variables are global unless declared to be local
    global
    local id
    id := GetTargetHwnd()
    PostMessage, % MsgNum, % cmd, % value, , ahk_id %id%
}

; Called when VirtualDesktopGridSwitcher sends us a message
ReceiveMessage(command, value, msg, hwnd) {
    ; All variables are global unless declared to be local
    global
    local num
    if (command == SWITCHED_DESKTOP) {
        num := value + 1
        ShowOverlay("Desktop " + num, desktopName[value], "../Icons/" . num . ".ico")
    } else {
        MsgBox, % command, % value
    }
}

HexToDec(hex) {
    result := hex + 0
    ; result .= ""
    return result
}

ShowOverlay(title, message, icon := "") {
    global overlayVisible, overlayEnabled, hideOverlayDelay
    if(!overlayEnabled) {
        return
    }
    overlayVisible := 1
    SysGet, primary, MonitorPrimary
    SysGet, monitor, Monitor, % primary
    width := 200
    height := 120
    padding := 100
    x := monitorLeft + ((monitorRight - monitorLeft) / 2) - (width / 2)
    y := monitorBottom - height - padding
    iconSize := 64
    textColor := "303030"
    windowColor := "FFFFFF"
    SplashImage, % icon, B1 X%x% Y%y% W%width% H%height% ZH%iconSize% ZW%iconSize% CW%windowColor% CT%textColor%, % message, % title, ,
    SetTimer, HideOverlay, % -hideOverlayDelay
}

HideOverlay() {
    global overlayVisible
    overlayVisible := 0
    SplashImage, Off
}

ToggleAlwaysOnTop(hwnd := "") {
    if(hwnd == "") {
        hwnd := WinExist("A")
    }
    WinSet, AlwaysOnTop, Toggle, ahk_id %hwnd%
    ;WinGet, ExStyle, ExStyle, ahk_id %hwnd%
    ;if(ExStyle & 0x8) {
    ;    message := "Always on Top Enabled"
    ;} else {
    ;    message := "Always on Top Disabled"
    ;}
    ;TrayTip, , %message%, 1, 0x10
}

OnExitFn(reason, exitCode) {
    global
    ; Kill the Exe.  Expectation is that this .ahk script will manage the exe's
    ; lifetime, both starting and stopping it.
    If(reason != "Reload" && reason != "Single") {
        SendCommand(QUIT)
    }
}
