Virtual Desktop Grid Switcher User Guide
========================================

Virtual Desktop Grid Switcher allows you to switch and move the current active window between Windows 10 Virtual Desktops in a virtual grid layout using arrow keys. This is helpful if like me you use more than a few desktops. The default layout is a 3x3 grid of desktops.

You can also make a window always visible on top of other windows or sticky (visible on all desktops) using a keyboard shortcut when that window is active.

Once the application is running you will see a new icon in your system tray.

[[img src=image1.png alt=SystemTray]]

If you right click this you can exit or modify the settings.

[[img src=image2.png alt=Settings]]

Grid Layout
-----------

You can change the Columns and Rows in your grid. If you do this you will probably want to change the icons used for each desktop. These can be found in the Icons folder of your installation. Alternative icon sets are available from VirtuaWin which inspired the development of this program at http://virtuawin.sourceforge.net/?page\_id=48.

When you increase the number of desktops required they automatically created for you. Reducing the number required leaves them for you to delete using the usual method (but you will not be able to access them via the arrow keys).

DO NOT PUT TOO LARGE NUMBERS IN THE ROWS AND COLUMNS AS WINDOWS WILL GRIND TO A HALT with so many desktops and you will have a hard time deleting them all. Even restarting won’t help!

You can enable Wrap Around mode which means that if you go right from the rightmost desktop it wraps around to the leftmost in the same row and vice versa and same for up and down in columns.

Key Assignment
--------------

You can change the key combinations for switching desktops, moving the currently active window to another desktop and switching to that desktop, and the Always on Top and Sticky window features.

Note if another program is already using a key combination you will be warned that it could not be assigned. You will either need to change the key combinations or find out what is using it already and stop it from doing so. Often your graphics software has some of these keys assigned.

You can also switch/move to a particular desktop by number. The default is to use the plain number keys. You can switch to the F1-12 keys but these are very commonly already assigned for other uses.

Default Browser Activation
--------------------------

Some web browsers do not open links clicked in other programs on the same desktop if there is another browser window open on another desktop which has been used more recently - currently Chrome and Firefox and Internet Explorer. Edge and Opera (which uses Chrome's engine) work as you would want - they even open links in a new window on the current desktop if there is a window on another desktop already open.

Default Browser Activation attempts to make Chrome and Firefox do the right thing too if they are your default browser - on switching desktops it detects if there is a browser window on the new desktop and activates it (top one if more than one) and then re-activates the window you were last using on that desktop (if it knows).

You may notice a "flash" due to the 2 windows being activated especially if the browser window was minimised. If the browser was minimised it is re-minimised also. The other side effect is that the browser may now be on top of windows that it was not and is the 2<sup>nd</sup> window in the ALT-Tab order. I have attempted to put the window back underneath other windows but not found a fast enough way to detect the window to put it under.

Default Browser Activation can be enabled and disabled in settings. If you have another browser you want to try this with you can manually add a BrowserInfo section in the VirtualDesktopGridSwitcher.Settings file in your installation folder - click Apply in settings if you don't have one. You will need to know the Class Name and executable name for your browser - AutoHotKey or Visual Studio Window Spy can do this or contact support for assistance.

Opening Word/Excel Documents
----------------------------

Word and Excel documents opened from windows explorer or menus can end up opening on another desktop if there is another document open on another window. Virtual Desktop Grid Switcher attempts to detect the switching desktop to the other document and then the new document in quick succession and moves the new document window to the original desktop.

If this is not working for you, you may need to increase the MoveOnNewWindowDetectTimeoutMs value in the VirtualDesktopGridSwitcher.Settings file. This is in milliseconds and determines what "quick succession" means. You can also add the executable name for other programs you think might benefit from this to MoveOnNewWindowExeNames (or remove word and excel if it is causing problems for you or prefer the original behaviour).

Acrobat Reader could also do with this (if you have disabled opening in tabs in settings) but it is immune to being moved to another desktop by Virtual Desktop Grid Switcher.

Support
-------

If you have questions please ask them on the SourceForge Discussion Page <https://sourceforge.net/p/virtual-desktop-grid-switcher/discussion/>

If you think something is not working correctly raise a ticket on the SourceForge Tickets Page <https://sourceforge.net/p/virtual-desktop-grid-switcher/tickets/>
