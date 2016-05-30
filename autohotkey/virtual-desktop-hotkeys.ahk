#persistent
MsgNum := DllCall("RegisterWindowMessage", Str, "VIRTUALDESKTOPGRIDSWITCHER_COMMAND")

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
return

; Return an HWND that will receive our commands.
GetTargetHwnd() {
    DetectHiddenWindows, On
    WinGet, a, List, ahk_exe VirtualDesktopGridSwitcher.exe
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
SendCommand(cmd) {
    ; All variables are global unless declared to be local
    global
    local id
    id := GetTargetHwnd()
    PostMessage, %MsgNum%, %cmd%, 0, , ahk_id %id%
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
^#A::SendCommand(TOGGLE_ALWAYS_ON_TOP)
^#S::SendCommand(TOGGLE_STICKY)