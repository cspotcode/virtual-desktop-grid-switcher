using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VirtualDesktopGridSwitcher.Settings;

namespace VirtualDesktopGridSwitcher {
    class SysTrayProcess : IDisposable {

        private NotifyIcon notifyIcon;
        private Icon[] desktopIcons;

        private ContextMenus contextMenu;

        public SysTrayProcess(SettingValues settings) {
            notifyIcon = new NotifyIcon();
            notifyIcon.Visible = true;
            notifyIcon.Text = "Virtual Desktop Grid Switcher";

            contextMenu = new ContextMenus(settings);
            notifyIcon.ContextMenuStrip = contextMenu.MenuStrip;

            LoadIconImages();
        }

        public void Dispose() {
            notifyIcon.Dispose();
        }

        public void ShowIconForDesktop(int desktopIndex) {
            notifyIcon.Icon = desktopIcons[desktopIndex];
        }

        private void LoadIconImages() {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var iconDir = Path.Combine(baseDir, "Icons");

            var icons = new List<Icon>();
            foreach (var f in Directory.GetFiles(iconDir, "*.ico").OrderBy(f => int.Parse(Path.GetFileNameWithoutExtension(f)))) {
                icons.Add(new Icon(f));
            }

            desktopIcons = icons.ToArray();
        }

    }
}
