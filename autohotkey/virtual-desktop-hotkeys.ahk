; Uncomment if you don't want this script appearing in your system tray
; #NoTrayIcon
#SingleInstance Force
#Persistent
MsgNum := DllCall("RegisterWindowMessage", Str, "VIRTUALDESKTOPGRIDSWITCHER_COMMAND")

exeName := "VirtualDesktopGridSwitcher.exe"

; Launch VirtualDesktopGridManager if it's not already running.
DetectHiddenWindows, On
If !WinExist("ahk_exe " . exeName) {
    Run "../VirtualDesktopGridSwitcher.exe"
}

; All commands
GO_UP := 1
GO_LEFT := 2
GO_RIGHT := 3
GO_DOWN := 4
MOVE_UP := 5
MOVE_LEFT := 6
MOVE_RIGHT := 7
MOVE_DOWN := 8
TOGGLE_ALWAYS_ON_TOP := 9
TOGGLE_STICKY := 10
DEBUG_SHOW_CURRENT_WINDOW_HWND := 11
SET_HWND_MESSAGE_TARGET := 12
SWITCHED_DESKTOP := 13
QUIT := 14

OnMessage(MsgNum, "ReceiveMessage")
myHwnd := HexToDec(A_ScriptHwnd)
SendCommand(SET_HWND_MESSAGE_TARGET, myHwnd)

desktopName := Object()
desktopName[0] := "Main"
desktopName[1] := "Work"
desktopName[2] := ""
desktopName[3] := "Chat"
desktopName[4] := ""
desktopName[5] := ""
desktopName[6] := ""
desktopName[7] := "BF"
desktopName[8] := "Music"
return

; Return an HWND that will receive our commands.
GetTargetHwnd() {
    global exeName
    DetectHiddenWindows, On
    WinGet, a, List, ahk_exe %exeName%
    Loop, %a% {
        id := a%A_Index%
        WinGetClass, cls, ahk_id %id%
        ; Skip the "GDI+" window
        If(RegExMatch(cls, "^GDI") = 0) {
            return id
        }
    }
}

; Send a command to VirtualDesktopGridSwitcher
SendCommand(cmd, value) {
    ; All variables are global unless declared to be local
    global
    local id
    id := GetTargetHwnd()
    PostMessage, %MsgNum%, %cmd%, %value%, , ahk_id %id%
}

; Called when VirtualDesktopGridSwitcher sends us a message
ReceiveMessage(cmd, value, msg, hwnd) {
    ; All variables are global unless declared to be local
    global
	local num
	if (cmd = SWITCHED_DESKTOP) {
	    num := value + 1
	    ShowOverlay("Desktop " + num, desktopName[value])
	} else {
	    MsgBox, %cmd%, %value%
	}
}

HexToDec(hex) {
	result := hex + 0
	; result .= ""
	return result
}

ShowOverlay(title, message) {
    SysGet, primary, MonitorPrimary
    SysGet, monitor, Monitor, %primary%
	width := 200
	height := 50
	padding := 100
	x := monitorLeft + ((monitorRight - monitorLeft) / 2) - (width / 2)
	y := monitorBottom - height - padding
    SplashImage, Icons/1.ico, BX%x%Y%y%W%width%H%height%, %message%, %title%, , 
}

HideOverlay() {
    SplashImage, Off
}

ShouldHideOverlay() {
	if(GetKeyState("Ctrl", "P") = 0 && GetKeyState("LWin", "P") = 0) {
	    return 1
	} else {
	    return 0
	}
}

CheckHideOverlay() {
    if(ShouldHideOverlay() = 1) {
	    HideOverlay()
	}
}

;;;;;;;
; Hotkeys.  Customize these to your liking.

^#Left::SendCommand(GO_LEFT, 0)
^#Right::SendCommand(GO_RIGHT, 0)
^#Down::SendCommand(GO_DOWN, 0)
^#Up::SendCommand(GO_UP, 0)
#!Left::SendCommand(MOVE_LEFT, 0)
#!Right::SendCommand(MOVE_RIGHT, 0)
#!Down::SendCommand(MOVE_DOWN, 0)
#!Up::SendCommand(MOVE_UP, 0)
^#A::SendCommand(TOGGLE_ALWAYS_ON_TOP, 0)
^#S::SendCommand(TOGGLE_STICKY, 0)
^#D::SendCommand(DEBUG_SHOW_CURRENT_WINDOW_HWND, 0)
^#F::
    MouseGetPos,,,id
    MsgBox, %id%
return
~*Ctrl Up::CheckHideOverlay()
~*LWin Up::CheckHideOverlay()