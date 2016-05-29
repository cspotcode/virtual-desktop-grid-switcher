using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace VirtualDesktopGridSwitcher.Settings {
    public partial class SettingsDialog : Form {

        private SettingValues settings;

        public SettingsDialog(SettingValues settings) {
            
            this.settings = settings;

            InitializeComponent();

            List<Keys> stickyKeyValues = Enum.GetValues(typeof(Keys)).Cast<Keys>().Where(v => v > 0).ToList();
            comboBoxKeySticky.DataSource = stickyKeyValues;

            List<Keys> alwaysOnTopKeyValues = Enum.GetValues(typeof(Keys)).Cast<Keys>().Where(v => v > 0).ToList();
            comboBoxAlwaysOnTopKey.DataSource = alwaysOnTopKeyValues;

            LoadValues();
        }

        protected override void OnVisibleChanged(EventArgs e) {
            base.OnVisibleChanged(e);

            if (this.Visible) {
                LoadValues();
            }
        }

        private void LoadValues() {
            textBoxRows.Text = settings.Rows.ToString();
            textBoxColumns.Text = settings.Columns.ToString();

            checkBoxWrapAround.Checked = settings.WrapAround;

            checkBoxCtrlModifierSwitch.Checked = settings.SwitchModifiers.Ctrl;
            checkBoxWinModifierSwitch.Checked = settings.SwitchModifiers.Win;
            checkBoxAltModifierSwitch.Checked = settings.SwitchModifiers.Alt;
            checkBoxShiftModifierSwitch.Checked = settings.SwitchModifiers.Shift;

            checkBoxCtrlModifierMove.Checked = settings.MoveModifiers.Ctrl;
            checkBoxWinModifierMove.Checked = settings.MoveModifiers.Win;
            checkBoxAltModifierMove.Checked = settings.MoveModifiers.Alt;
            checkBoxShiftModifierMove.Checked = settings.MoveModifiers.Shift;

            checkBoxFKeys.Checked = settings.FKeysForNumbers;

            checkBoxCtrlModifierSticky.Checked = settings.StickyWindowHotKey.Modifiers.Ctrl;
            checkBoxWinModifierSticky.Checked = settings.StickyWindowHotKey.Modifiers.Win;
            checkBoxAltModifierSticky.Checked = settings.StickyWindowHotKey.Modifiers.Alt;
            checkBoxShiftModifierSticky.Checked = settings.StickyWindowHotKey.Modifiers.Shift;
            comboBoxKeySticky.SelectedItem = settings.StickyWindowHotKey.Key;

            checkBoxCtrlModifierAlwaysOnTop.Checked = settings.AlwaysOnTopHotkey.Modifiers.Ctrl;
            checkBoxWinModifierAlwaysOnTop.Checked = settings.AlwaysOnTopHotkey.Modifiers.Win;
            checkBoxAltModifierAlwaysOnTop.Checked = settings.AlwaysOnTopHotkey.Modifiers.Alt;
            checkBoxShiftModifierAlwaysOnTop.Checked = settings.AlwaysOnTopHotkey.Modifiers.Shift;
            comboBoxAlwaysOnTopKey.SelectedItem = settings.AlwaysOnTopHotkey.Key;

            checkBoxActivateWebBrowser.Checked = settings.ActivateWebBrowserOnSwitch;
        }

        private bool SaveValues() {
            try {
                var rows = int.Parse(textBoxRows.Text);
                var cols = int.Parse(textBoxColumns.Text);

                if (rows * cols > 20) {
                    var result = 
                        MessageBox.Show(this,
                                        (rows * cols) + " desktops is not recommended for windows performance. Continue?", 
                                        "Warning",
                                        MessageBoxButtons.YesNo,
                                        MessageBoxIcon.Warning);
                    if (result != DialogResult.Yes) {
                        return false;
                    }
                }

                if (rows * cols < settings.Rows * settings.Columns) {
                    MessageBox.Show(this, "Unrequired desktops will not be removed");
                } else if (rows * cols > settings.Rows * settings.Columns) {
                    MessageBox.Show(this, "More desktops will be created to fill the grid if necessary");
                }

                settings.Rows = rows;
                settings.Columns = cols;
            } catch {
                MessageBox.Show(this, "Values for Rows and Columns must be numbers only");
                return false;
            }

            settings.WrapAround = checkBoxWrapAround.Checked;

            settings.SwitchModifiers.Ctrl = checkBoxCtrlModifierSwitch.Checked;
            settings.SwitchModifiers.Win = checkBoxWinModifierSwitch.Checked;
            settings.SwitchModifiers.Alt = checkBoxAltModifierSwitch.Checked;
            settings.SwitchModifiers.Shift = checkBoxShiftModifierSwitch.Checked;

            settings.MoveModifiers.Ctrl = checkBoxCtrlModifierMove.Checked;
            settings.MoveModifiers.Win = checkBoxWinModifierMove.Checked;
            settings.MoveModifiers.Alt = checkBoxAltModifierMove.Checked;
            settings.MoveModifiers.Shift = checkBoxShiftModifierMove.Checked;

            settings.FKeysForNumbers = checkBoxFKeys.Checked;

            settings.StickyWindowHotKey.Modifiers.Ctrl = checkBoxCtrlModifierSticky.Checked;
            settings.StickyWindowHotKey.Modifiers.Win = checkBoxWinModifierSticky.Checked;
            settings.StickyWindowHotKey.Modifiers.Alt = checkBoxAltModifierSticky.Checked;
            settings.StickyWindowHotKey.Modifiers.Shift = checkBoxShiftModifierSticky.Checked;
            settings.StickyWindowHotKey.Key = (Keys)comboBoxKeySticky.SelectedItem;

            settings.AlwaysOnTopHotkey.Modifiers.Ctrl = checkBoxCtrlModifierAlwaysOnTop.Checked;
            settings.AlwaysOnTopHotkey.Modifiers.Win = checkBoxWinModifierAlwaysOnTop.Checked;
            settings.AlwaysOnTopHotkey.Modifiers.Alt = checkBoxAltModifierAlwaysOnTop.Checked;
            settings.AlwaysOnTopHotkey.Modifiers.Shift = checkBoxShiftModifierAlwaysOnTop.Checked;
            settings.AlwaysOnTopHotkey.Key = (Keys)comboBoxAlwaysOnTopKey.SelectedItem;

            settings.ActivateWebBrowserOnSwitch = checkBoxActivateWebBrowser.Checked;

            if (!settings.ApplySettings()) {
                MessageBox.Show(this, "Failed to apply settings", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (!settings.Save()) {
                MessageBox.Show(this, "Failed to save settings to file", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        private void buttonOK_Click(object sender, EventArgs e) {
            SaveValues();
        }

        private void comboBoxKey_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode != Keys.ControlKey &&
                e.KeyCode != Keys.Menu &&
                e.KeyCode != Keys.ShiftKey &&
                e.KeyCode != Keys.LWin &&
                e.KeyCode != Keys.RWin) {
                ((ComboBox)sender).SelectedItem = e.KeyCode;
                e.Handled = true;
            }
        }

        private void comboBoxKey_KeyPress(object sender, KeyPressEventArgs e) {
            e.Handled = true;
        }
    }
}
