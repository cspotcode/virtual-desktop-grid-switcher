namespace VirtualDesktopGridSwitcher.Settings {
    partial class SettingsDialog {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsDialog));
            this.labelRows = new System.Windows.Forms.Label();
            this.labelColumns = new System.Windows.Forms.Label();
            this.checkBoxWrapAround = new System.Windows.Forms.CheckBox();
            this.labelKeyModifiers = new System.Windows.Forms.Label();
            this.textBoxRows = new System.Windows.Forms.TextBox();
            this.textBoxColumns = new System.Windows.Forms.TextBox();
            this.checkBoxCtrlModifierSwitch = new System.Windows.Forms.CheckBox();
            this.checkBoxShiftModifierSwitch = new System.Windows.Forms.CheckBox();
            this.checkBoxAltModifierSwitch = new System.Windows.Forms.CheckBox();
            this.checkBoxWinModifierSwitch = new System.Windows.Forms.CheckBox();
            this.checkBoxFKeys = new System.Windows.Forms.CheckBox();
            this.buttonApply = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.checkBoxWinModifierMove = new System.Windows.Forms.CheckBox();
            this.checkBoxAltModifierMove = new System.Windows.Forms.CheckBox();
            this.checkBoxShiftModifierMove = new System.Windows.Forms.CheckBox();
            this.checkBoxCtrlModifierMove = new System.Windows.Forms.CheckBox();
            this.labelMoveToDesktopModifiers = new System.Windows.Forms.Label();
            this.checkBoxActivateWebBrowser = new System.Windows.Forms.CheckBox();
            this.toolTipSettingsDialog = new System.Windows.Forms.ToolTip(this.components);
            this.checkBoxWinModifierSticky = new System.Windows.Forms.CheckBox();
            this.checkBoxAltModifierSticky = new System.Windows.Forms.CheckBox();
            this.checkBoxShiftModifierSticky = new System.Windows.Forms.CheckBox();
            this.checkBoxCtrlModifierSticky = new System.Windows.Forms.CheckBox();
            this.labelToggleSticky = new System.Windows.Forms.Label();
            this.checkBoxWinModifierAlwaysOnTop = new System.Windows.Forms.CheckBox();
            this.checkBoxAltModifierAlwaysOnTop = new System.Windows.Forms.CheckBox();
            this.checkBoxShiftModifierAlwaysOnTop = new System.Windows.Forms.CheckBox();
            this.checkBoxCtrlModifierAlwaysOnTop = new System.Windows.Forms.CheckBox();
            this.labelToggleAkwaysOnTop = new System.Windows.Forms.Label();
            this.comboBoxKeySticky = new System.Windows.Forms.ComboBox();
            this.comboBoxAlwaysOnTopKey = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // labelRows
            // 
            this.labelRows.AutoSize = true;
            this.labelRows.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelRows.Location = new System.Drawing.Point(161, 11);
            this.labelRows.Name = "labelRows";
            this.labelRows.Size = new System.Drawing.Size(48, 16);
            this.labelRows.TabIndex = 0;
            this.labelRows.Text = "Rows:";
            this.toolTipSettingsDialog.SetToolTip(this.labelRows, "Number of rows in desktop grid");
            // 
            // labelColumns
            // 
            this.labelColumns.AutoSize = true;
            this.labelColumns.Font = new System.Drawing.Font("Verdana", 9.75F);
            this.labelColumns.Location = new System.Drawing.Point(12, 11);
            this.labelColumns.Name = "labelColumns";
            this.labelColumns.Size = new System.Drawing.Size(68, 16);
            this.labelColumns.TabIndex = 1;
            this.labelColumns.Text = "Columns:";
            this.toolTipSettingsDialog.SetToolTip(this.labelColumns, "Number of columns in desktop grid");
            // 
            // checkBoxWrapAround
            // 
            this.checkBoxWrapAround.AutoSize = true;
            this.checkBoxWrapAround.Font = new System.Drawing.Font("Verdana", 9.75F);
            this.checkBoxWrapAround.Location = new System.Drawing.Point(318, 10);
            this.checkBoxWrapAround.Name = "checkBoxWrapAround";
            this.checkBoxWrapAround.Size = new System.Drawing.Size(112, 20);
            this.checkBoxWrapAround.TabIndex = 2;
            this.checkBoxWrapAround.Text = "Wrap Around";
            this.toolTipSettingsDialog.SetToolTip(this.checkBoxWrapAround, "Switch/Move to opposite side of grid when move beyond edge");
            this.checkBoxWrapAround.UseVisualStyleBackColor = true;
            // 
            // labelKeyModifiers
            // 
            this.labelKeyModifiers.AutoSize = true;
            this.labelKeyModifiers.Font = new System.Drawing.Font("Verdana", 9.75F);
            this.labelKeyModifiers.Location = new System.Drawing.Point(12, 38);
            this.labelKeyModifiers.Name = "labelKeyModifiers";
            this.labelKeyModifiers.Size = new System.Drawing.Size(180, 16);
            this.labelKeyModifiers.TabIndex = 3;
            this.labelKeyModifiers.Text = "Switch Desktop Modifiers:";
            this.toolTipSettingsDialog.SetToolTip(this.labelKeyModifiers, "Key modifier combination to use with arrow keys to switch to another desktop");
            // 
            // textBoxRows
            // 
            this.textBoxRows.Location = new System.Drawing.Point(215, 10);
            this.textBoxRows.Name = "textBoxRows";
            this.textBoxRows.Size = new System.Drawing.Size(49, 20);
            this.textBoxRows.TabIndex = 4;
            this.toolTipSettingsDialog.SetToolTip(this.textBoxRows, "Number of rows in desktop grid");
            // 
            // textBoxColumns
            // 
            this.textBoxColumns.Location = new System.Drawing.Point(86, 10);
            this.textBoxColumns.Name = "textBoxColumns";
            this.textBoxColumns.Size = new System.Drawing.Size(49, 20);
            this.textBoxColumns.TabIndex = 5;
            this.toolTipSettingsDialog.SetToolTip(this.textBoxColumns, "Number of columns in desktop grid");
            // 
            // checkBoxCtrlModifierSwitch
            // 
            this.checkBoxCtrlModifierSwitch.AutoSize = true;
            this.checkBoxCtrlModifierSwitch.Font = new System.Drawing.Font("Verdana", 9.75F);
            this.checkBoxCtrlModifierSwitch.Location = new System.Drawing.Point(206, 37);
            this.checkBoxCtrlModifierSwitch.Name = "checkBoxCtrlModifierSwitch";
            this.checkBoxCtrlModifierSwitch.Size = new System.Drawing.Size(50, 20);
            this.checkBoxCtrlModifierSwitch.TabIndex = 6;
            this.checkBoxCtrlModifierSwitch.Text = "Ctrl";
            this.toolTipSettingsDialog.SetToolTip(this.checkBoxCtrlModifierSwitch, "Key modifier combination to use with arrow keys to switch to another desktop");
            this.checkBoxCtrlModifierSwitch.UseVisualStyleBackColor = true;
            // 
            // checkBoxShiftModifierSwitch
            // 
            this.checkBoxShiftModifierSwitch.AutoSize = true;
            this.checkBoxShiftModifierSwitch.Font = new System.Drawing.Font("Verdana", 9.75F);
            this.checkBoxShiftModifierSwitch.Location = new System.Drawing.Point(370, 37);
            this.checkBoxShiftModifierSwitch.Name = "checkBoxShiftModifierSwitch";
            this.checkBoxShiftModifierSwitch.Size = new System.Drawing.Size(58, 20);
            this.checkBoxShiftModifierSwitch.TabIndex = 7;
            this.checkBoxShiftModifierSwitch.Text = "Shift";
            this.toolTipSettingsDialog.SetToolTip(this.checkBoxShiftModifierSwitch, "Key modifier combination to use with arrow keys to switch to another desktop");
            this.checkBoxShiftModifierSwitch.UseVisualStyleBackColor = true;
            // 
            // checkBoxAltModifierSwitch
            // 
            this.checkBoxAltModifierSwitch.AutoSize = true;
            this.checkBoxAltModifierSwitch.Font = new System.Drawing.Font("Verdana", 9.75F);
            this.checkBoxAltModifierSwitch.Location = new System.Drawing.Point(318, 37);
            this.checkBoxAltModifierSwitch.Name = "checkBoxAltModifierSwitch";
            this.checkBoxAltModifierSwitch.Size = new System.Drawing.Size(45, 20);
            this.checkBoxAltModifierSwitch.TabIndex = 8;
            this.checkBoxAltModifierSwitch.Text = "Alt";
            this.toolTipSettingsDialog.SetToolTip(this.checkBoxAltModifierSwitch, "Key modifier combination to use with arrow keys to switch to another desktop");
            this.checkBoxAltModifierSwitch.UseVisualStyleBackColor = true;
            // 
            // checkBoxWinModifierSwitch
            // 
            this.checkBoxWinModifierSwitch.AutoSize = true;
            this.checkBoxWinModifierSwitch.Font = new System.Drawing.Font("Verdana", 9.75F);
            this.checkBoxWinModifierSwitch.Location = new System.Drawing.Point(262, 37);
            this.checkBoxWinModifierSwitch.Name = "checkBoxWinModifierSwitch";
            this.checkBoxWinModifierSwitch.Size = new System.Drawing.Size(51, 20);
            this.checkBoxWinModifierSwitch.TabIndex = 9;
            this.checkBoxWinModifierSwitch.Text = "Win";
            this.toolTipSettingsDialog.SetToolTip(this.checkBoxWinModifierSwitch, "Key modifier combination to use with arrow keys to switch to another desktop");
            this.checkBoxWinModifierSwitch.UseVisualStyleBackColor = true;
            // 
            // checkBoxFKeys
            // 
            this.checkBoxFKeys.AutoSize = true;
            this.checkBoxFKeys.Font = new System.Drawing.Font("Verdana", 9.75F);
            this.checkBoxFKeys.Location = new System.Drawing.Point(15, 87);
            this.checkBoxFKeys.Name = "checkBoxFKeys";
            this.checkBoxFKeys.Size = new System.Drawing.Size(150, 20);
            this.checkBoxFKeys.TabIndex = 10;
            this.checkBoxFKeys.Text = "F1-12 for Numbers";
            this.toolTipSettingsDialog.SetToolTip(this.checkBoxFKeys, "Use normal number keys or F number keys with modifiers to jump to specific deskto" +
        "p");
            this.checkBoxFKeys.UseVisualStyleBackColor = true;
            // 
            // buttonApply
            // 
            this.buttonApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonApply.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonApply.Font = new System.Drawing.Font("Verdana", 9.75F);
            this.buttonApply.Location = new System.Drawing.Point(402, 189);
            this.buttonApply.Name = "buttonApply";
            this.buttonApply.Size = new System.Drawing.Size(75, 23);
            this.buttonApply.TabIndex = 11;
            this.buttonApply.Text = "&Apply";
            this.buttonApply.UseVisualStyleBackColor = true;
            this.buttonApply.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Font = new System.Drawing.Font("Verdana", 9.75F);
            this.buttonCancel.Location = new System.Drawing.Point(485, 189);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 12;
            this.buttonCancel.Text = "&Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // checkBoxWinModifierMove
            // 
            this.checkBoxWinModifierMove.AutoSize = true;
            this.checkBoxWinModifierMove.Font = new System.Drawing.Font("Verdana", 9.75F);
            this.checkBoxWinModifierMove.Location = new System.Drawing.Point(262, 63);
            this.checkBoxWinModifierMove.Name = "checkBoxWinModifierMove";
            this.checkBoxWinModifierMove.Size = new System.Drawing.Size(51, 20);
            this.checkBoxWinModifierMove.TabIndex = 17;
            this.checkBoxWinModifierMove.Text = "Win";
            this.toolTipSettingsDialog.SetToolTip(this.checkBoxWinModifierMove, "Key modifier combination to use with arrow keys to move a window to another deskt" +
        "op");
            this.checkBoxWinModifierMove.UseVisualStyleBackColor = true;
            // 
            // checkBoxAltModifierMove
            // 
            this.checkBoxAltModifierMove.AutoSize = true;
            this.checkBoxAltModifierMove.Font = new System.Drawing.Font("Verdana", 9.75F);
            this.checkBoxAltModifierMove.Location = new System.Drawing.Point(318, 63);
            this.checkBoxAltModifierMove.Name = "checkBoxAltModifierMove";
            this.checkBoxAltModifierMove.Size = new System.Drawing.Size(45, 20);
            this.checkBoxAltModifierMove.TabIndex = 16;
            this.checkBoxAltModifierMove.Text = "Alt";
            this.toolTipSettingsDialog.SetToolTip(this.checkBoxAltModifierMove, "Key modifier combination to use with arrow keys to move a window to another deskt" +
        "op");
            this.checkBoxAltModifierMove.UseVisualStyleBackColor = true;
            // 
            // checkBoxShiftModifierMove
            // 
            this.checkBoxShiftModifierMove.AutoSize = true;
            this.checkBoxShiftModifierMove.Font = new System.Drawing.Font("Verdana", 9.75F);
            this.checkBoxShiftModifierMove.Location = new System.Drawing.Point(370, 63);
            this.checkBoxShiftModifierMove.Name = "checkBoxShiftModifierMove";
            this.checkBoxShiftModifierMove.Size = new System.Drawing.Size(58, 20);
            this.checkBoxShiftModifierMove.TabIndex = 15;
            this.checkBoxShiftModifierMove.Text = "Shift";
            this.toolTipSettingsDialog.SetToolTip(this.checkBoxShiftModifierMove, "Key modifier combination to use with arrow keys to move a window to another deskt" +
        "op");
            this.checkBoxShiftModifierMove.UseVisualStyleBackColor = true;
            // 
            // checkBoxCtrlModifierMove
            // 
            this.checkBoxCtrlModifierMove.AutoSize = true;
            this.checkBoxCtrlModifierMove.Font = new System.Drawing.Font("Verdana", 9.75F);
            this.checkBoxCtrlModifierMove.Location = new System.Drawing.Point(206, 63);
            this.checkBoxCtrlModifierMove.Name = "checkBoxCtrlModifierMove";
            this.checkBoxCtrlModifierMove.Size = new System.Drawing.Size(50, 20);
            this.checkBoxCtrlModifierMove.TabIndex = 14;
            this.checkBoxCtrlModifierMove.Text = "Ctrl";
            this.toolTipSettingsDialog.SetToolTip(this.checkBoxCtrlModifierMove, "Key modifier combination to use with arrow keys to move a window to another deskt" +
        "op");
            this.checkBoxCtrlModifierMove.UseVisualStyleBackColor = true;
            // 
            // labelMoveToDesktopModifiers
            // 
            this.labelMoveToDesktopModifiers.AutoSize = true;
            this.labelMoveToDesktopModifiers.Font = new System.Drawing.Font("Verdana", 9.75F);
            this.labelMoveToDesktopModifiers.Location = new System.Drawing.Point(12, 64);
            this.labelMoveToDesktopModifiers.Name = "labelMoveToDesktopModifiers";
            this.labelMoveToDesktopModifiers.Size = new System.Drawing.Size(191, 16);
            this.labelMoveToDesktopModifiers.TabIndex = 13;
            this.labelMoveToDesktopModifiers.Text = "Move To Desktop Modifiers:";
            this.toolTipSettingsDialog.SetToolTip(this.labelMoveToDesktopModifiers, "Key modifier combination to use with arrow keys to move a window to another deskt" +
        "op");
            // 
            // checkBoxActivateWebBrowser
            // 
            this.checkBoxActivateWebBrowser.AutoSize = true;
            this.checkBoxActivateWebBrowser.Font = new System.Drawing.Font("Verdana", 9.75F);
            this.checkBoxActivateWebBrowser.Location = new System.Drawing.Point(15, 167);
            this.checkBoxActivateWebBrowser.Name = "checkBoxActivateWebBrowser";
            this.checkBoxActivateWebBrowser.Size = new System.Drawing.Size(203, 20);
            this.checkBoxActivateWebBrowser.TabIndex = 18;
            this.checkBoxActivateWebBrowser.Text = "Default Browser Activation";
            this.toolTipSettingsDialog.SetToolTip(this.checkBoxActivateWebBrowser, "On desktop switch, last active default browser window on that desktop is activate" +
        "d before active window so that links open in current desktop");
            this.checkBoxActivateWebBrowser.UseVisualStyleBackColor = true;
            // 
            // checkBoxWinModifierSticky
            // 
            this.checkBoxWinModifierSticky.AutoSize = true;
            this.checkBoxWinModifierSticky.Font = new System.Drawing.Font("Verdana", 9.75F);
            this.checkBoxWinModifierSticky.Location = new System.Drawing.Point(231, 141);
            this.checkBoxWinModifierSticky.Name = "checkBoxWinModifierSticky";
            this.checkBoxWinModifierSticky.Size = new System.Drawing.Size(51, 20);
            this.checkBoxWinModifierSticky.TabIndex = 24;
            this.checkBoxWinModifierSticky.Text = "Win";
            this.toolTipSettingsDialog.SetToolTip(this.checkBoxWinModifierSticky, "Key modifier combination to use with arrow keys to move a window to another deskt" +
        "op");
            this.checkBoxWinModifierSticky.UseVisualStyleBackColor = true;
            // 
            // checkBoxAltModifierSticky
            // 
            this.checkBoxAltModifierSticky.AutoSize = true;
            this.checkBoxAltModifierSticky.Font = new System.Drawing.Font("Verdana", 9.75F);
            this.checkBoxAltModifierSticky.Location = new System.Drawing.Point(287, 141);
            this.checkBoxAltModifierSticky.Name = "checkBoxAltModifierSticky";
            this.checkBoxAltModifierSticky.Size = new System.Drawing.Size(45, 20);
            this.checkBoxAltModifierSticky.TabIndex = 23;
            this.checkBoxAltModifierSticky.Text = "Alt";
            this.toolTipSettingsDialog.SetToolTip(this.checkBoxAltModifierSticky, "Key modifier combination to use with arrow keys to move a window to another deskt" +
        "op");
            this.checkBoxAltModifierSticky.UseVisualStyleBackColor = true;
            // 
            // checkBoxShiftModifierSticky
            // 
            this.checkBoxShiftModifierSticky.AutoSize = true;
            this.checkBoxShiftModifierSticky.Font = new System.Drawing.Font("Verdana", 9.75F);
            this.checkBoxShiftModifierSticky.Location = new System.Drawing.Point(339, 141);
            this.checkBoxShiftModifierSticky.Name = "checkBoxShiftModifierSticky";
            this.checkBoxShiftModifierSticky.Size = new System.Drawing.Size(58, 20);
            this.checkBoxShiftModifierSticky.TabIndex = 22;
            this.checkBoxShiftModifierSticky.Text = "Shift";
            this.toolTipSettingsDialog.SetToolTip(this.checkBoxShiftModifierSticky, "Key modifier combination to use with arrow keys to move a window to another deskt" +
        "op");
            this.checkBoxShiftModifierSticky.UseVisualStyleBackColor = true;
            // 
            // checkBoxCtrlModifierSticky
            // 
            this.checkBoxCtrlModifierSticky.AutoSize = true;
            this.checkBoxCtrlModifierSticky.Font = new System.Drawing.Font("Verdana", 9.75F);
            this.checkBoxCtrlModifierSticky.Location = new System.Drawing.Point(175, 141);
            this.checkBoxCtrlModifierSticky.Name = "checkBoxCtrlModifierSticky";
            this.checkBoxCtrlModifierSticky.Size = new System.Drawing.Size(50, 20);
            this.checkBoxCtrlModifierSticky.TabIndex = 21;
            this.checkBoxCtrlModifierSticky.Text = "Ctrl";
            this.toolTipSettingsDialog.SetToolTip(this.checkBoxCtrlModifierSticky, "Key modifier combination to use with arrow keys to move a window to another deskt" +
        "op");
            this.checkBoxCtrlModifierSticky.UseVisualStyleBackColor = true;
            // 
            // labelToggleSticky
            // 
            this.labelToggleSticky.AutoSize = true;
            this.labelToggleSticky.Font = new System.Drawing.Font("Verdana", 9.75F);
            this.labelToggleSticky.Location = new System.Drawing.Point(12, 142);
            this.labelToggleSticky.Name = "labelToggleSticky";
            this.labelToggleSticky.Size = new System.Drawing.Size(159, 16);
            this.labelToggleSticky.TabIndex = 20;
            this.labelToggleSticky.Text = "Toggle Sticky Window:";
            this.toolTipSettingsDialog.SetToolTip(this.labelToggleSticky, "Key modifier combination to use with arrow keys to move a window to another deskt" +
        "op");
            // 
            // checkBoxWinModifierAlwaysOnTop
            // 
            this.checkBoxWinModifierAlwaysOnTop.AutoSize = true;
            this.checkBoxWinModifierAlwaysOnTop.Font = new System.Drawing.Font("Verdana", 9.75F);
            this.checkBoxWinModifierAlwaysOnTop.Location = new System.Drawing.Point(231, 111);
            this.checkBoxWinModifierAlwaysOnTop.Name = "checkBoxWinModifierAlwaysOnTop";
            this.checkBoxWinModifierAlwaysOnTop.Size = new System.Drawing.Size(51, 20);
            this.checkBoxWinModifierAlwaysOnTop.TabIndex = 30;
            this.checkBoxWinModifierAlwaysOnTop.Text = "Win";
            this.toolTipSettingsDialog.SetToolTip(this.checkBoxWinModifierAlwaysOnTop, "Key modifier combination to use with arrow keys to move a window to another deskt" +
        "op");
            this.checkBoxWinModifierAlwaysOnTop.UseVisualStyleBackColor = true;
            // 
            // checkBoxAltModifierAlwaysOnTop
            // 
            this.checkBoxAltModifierAlwaysOnTop.AutoSize = true;
            this.checkBoxAltModifierAlwaysOnTop.Font = new System.Drawing.Font("Verdana", 9.75F);
            this.checkBoxAltModifierAlwaysOnTop.Location = new System.Drawing.Point(287, 111);
            this.checkBoxAltModifierAlwaysOnTop.Name = "checkBoxAltModifierAlwaysOnTop";
            this.checkBoxAltModifierAlwaysOnTop.Size = new System.Drawing.Size(45, 20);
            this.checkBoxAltModifierAlwaysOnTop.TabIndex = 29;
            this.checkBoxAltModifierAlwaysOnTop.Text = "Alt";
            this.toolTipSettingsDialog.SetToolTip(this.checkBoxAltModifierAlwaysOnTop, "Key modifier combination to use with arrow keys to move a window to another deskt" +
        "op");
            this.checkBoxAltModifierAlwaysOnTop.UseVisualStyleBackColor = true;
            // 
            // checkBoxShiftModifierAlwaysOnTop
            // 
            this.checkBoxShiftModifierAlwaysOnTop.AutoSize = true;
            this.checkBoxShiftModifierAlwaysOnTop.Font = new System.Drawing.Font("Verdana", 9.75F);
            this.checkBoxShiftModifierAlwaysOnTop.Location = new System.Drawing.Point(339, 111);
            this.checkBoxShiftModifierAlwaysOnTop.Name = "checkBoxShiftModifierAlwaysOnTop";
            this.checkBoxShiftModifierAlwaysOnTop.Size = new System.Drawing.Size(58, 20);
            this.checkBoxShiftModifierAlwaysOnTop.TabIndex = 28;
            this.checkBoxShiftModifierAlwaysOnTop.Text = "Shift";
            this.toolTipSettingsDialog.SetToolTip(this.checkBoxShiftModifierAlwaysOnTop, "Key modifier combination to use with arrow keys to move a window to another deskt" +
        "op");
            this.checkBoxShiftModifierAlwaysOnTop.UseVisualStyleBackColor = true;
            // 
            // checkBoxCtrlModifierAlwaysOnTop
            // 
            this.checkBoxCtrlModifierAlwaysOnTop.AutoSize = true;
            this.checkBoxCtrlModifierAlwaysOnTop.Font = new System.Drawing.Font("Verdana", 9.75F);
            this.checkBoxCtrlModifierAlwaysOnTop.Location = new System.Drawing.Point(175, 111);
            this.checkBoxCtrlModifierAlwaysOnTop.Name = "checkBoxCtrlModifierAlwaysOnTop";
            this.checkBoxCtrlModifierAlwaysOnTop.Size = new System.Drawing.Size(50, 20);
            this.checkBoxCtrlModifierAlwaysOnTop.TabIndex = 27;
            this.checkBoxCtrlModifierAlwaysOnTop.Text = "Ctrl";
            this.toolTipSettingsDialog.SetToolTip(this.checkBoxCtrlModifierAlwaysOnTop, "Key modifier combination to use with arrow keys to move a window to another deskt" +
        "op");
            this.checkBoxCtrlModifierAlwaysOnTop.UseVisualStyleBackColor = true;
            // 
            // labelToggleAkwaysOnTop
            // 
            this.labelToggleAkwaysOnTop.AutoSize = true;
            this.labelToggleAkwaysOnTop.Font = new System.Drawing.Font("Verdana", 9.75F);
            this.labelToggleAkwaysOnTop.Location = new System.Drawing.Point(12, 112);
            this.labelToggleAkwaysOnTop.Name = "labelToggleAkwaysOnTop";
            this.labelToggleAkwaysOnTop.Size = new System.Drawing.Size(160, 16);
            this.labelToggleAkwaysOnTop.TabIndex = 26;
            this.labelToggleAkwaysOnTop.Text = "Toggle Always On Top:";
            this.toolTipSettingsDialog.SetToolTip(this.labelToggleAkwaysOnTop, "Key modifier combination to use with arrow keys to move a window to another deskt" +
        "op");
            // 
            // comboBoxKeySticky
            // 
            this.comboBoxKeySticky.Font = new System.Drawing.Font("Verdana", 9.75F);
            this.comboBoxKeySticky.FormattingEnabled = true;
            this.comboBoxKeySticky.Location = new System.Drawing.Point(402, 139);
            this.comboBoxKeySticky.Name = "comboBoxKeySticky";
            this.comboBoxKeySticky.Size = new System.Drawing.Size(158, 24);
            this.comboBoxKeySticky.TabIndex = 19;
            // 
            // comboBoxAlwaysOnTopKey
            // 
            this.comboBoxAlwaysOnTopKey.Font = new System.Drawing.Font("Verdana", 9.75F);
            this.comboBoxAlwaysOnTopKey.FormattingEnabled = true;
            this.comboBoxAlwaysOnTopKey.Location = new System.Drawing.Point(402, 109);
            this.comboBoxAlwaysOnTopKey.Name = "comboBoxAlwaysOnTopKey";
            this.comboBoxAlwaysOnTopKey.Size = new System.Drawing.Size(158, 24);
            this.comboBoxAlwaysOnTopKey.TabIndex = 25;
            // 
            // SettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(572, 224);
            this.Controls.Add(this.checkBoxWinModifierAlwaysOnTop);
            this.Controls.Add(this.checkBoxAltModifierAlwaysOnTop);
            this.Controls.Add(this.checkBoxShiftModifierAlwaysOnTop);
            this.Controls.Add(this.checkBoxCtrlModifierAlwaysOnTop);
            this.Controls.Add(this.labelToggleAkwaysOnTop);
            this.Controls.Add(this.comboBoxAlwaysOnTopKey);
            this.Controls.Add(this.checkBoxWinModifierSticky);
            this.Controls.Add(this.checkBoxAltModifierSticky);
            this.Controls.Add(this.checkBoxShiftModifierSticky);
            this.Controls.Add(this.checkBoxCtrlModifierSticky);
            this.Controls.Add(this.labelToggleSticky);
            this.Controls.Add(this.comboBoxKeySticky);
            this.Controls.Add(this.checkBoxActivateWebBrowser);
            this.Controls.Add(this.checkBoxWinModifierMove);
            this.Controls.Add(this.checkBoxAltModifierMove);
            this.Controls.Add(this.checkBoxShiftModifierMove);
            this.Controls.Add(this.checkBoxCtrlModifierMove);
            this.Controls.Add(this.labelMoveToDesktopModifiers);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonApply);
            this.Controls.Add(this.checkBoxFKeys);
            this.Controls.Add(this.checkBoxWinModifierSwitch);
            this.Controls.Add(this.checkBoxAltModifierSwitch);
            this.Controls.Add(this.checkBoxShiftModifierSwitch);
            this.Controls.Add(this.checkBoxCtrlModifierSwitch);
            this.Controls.Add(this.textBoxColumns);
            this.Controls.Add(this.textBoxRows);
            this.Controls.Add(this.labelKeyModifiers);
            this.Controls.Add(this.checkBoxWrapAround);
            this.Controls.Add(this.labelColumns);
            this.Controls.Add(this.labelRows);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SettingsDialog";
            this.Text = "Virtual Desktop Grid Switcher Settings";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelRows;
        private System.Windows.Forms.Label labelColumns;
        private System.Windows.Forms.CheckBox checkBoxWrapAround;
        private System.Windows.Forms.Label labelKeyModifiers;
        private System.Windows.Forms.TextBox textBoxRows;
        private System.Windows.Forms.TextBox textBoxColumns;
        private System.Windows.Forms.CheckBox checkBoxCtrlModifierSwitch;
        private System.Windows.Forms.CheckBox checkBoxShiftModifierSwitch;
        private System.Windows.Forms.CheckBox checkBoxAltModifierSwitch;
        private System.Windows.Forms.CheckBox checkBoxWinModifierSwitch;
        private System.Windows.Forms.CheckBox checkBoxFKeys;
        private System.Windows.Forms.Button buttonApply;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.CheckBox checkBoxWinModifierMove;
        private System.Windows.Forms.CheckBox checkBoxAltModifierMove;
        private System.Windows.Forms.CheckBox checkBoxShiftModifierMove;
        private System.Windows.Forms.CheckBox checkBoxCtrlModifierMove;
        private System.Windows.Forms.Label labelMoveToDesktopModifiers;
        private System.Windows.Forms.CheckBox checkBoxActivateWebBrowser;
        private System.Windows.Forms.ToolTip toolTipSettingsDialog;
        private System.Windows.Forms.ComboBox comboBoxKeySticky;
        private System.Windows.Forms.CheckBox checkBoxWinModifierSticky;
        private System.Windows.Forms.CheckBox checkBoxAltModifierSticky;
        private System.Windows.Forms.CheckBox checkBoxShiftModifierSticky;
        private System.Windows.Forms.CheckBox checkBoxCtrlModifierSticky;
        private System.Windows.Forms.Label labelToggleSticky;
        private System.Windows.Forms.CheckBox checkBoxWinModifierAlwaysOnTop;
        private System.Windows.Forms.CheckBox checkBoxAltModifierAlwaysOnTop;
        private System.Windows.Forms.CheckBox checkBoxShiftModifierAlwaysOnTop;
        private System.Windows.Forms.CheckBox checkBoxCtrlModifierAlwaysOnTop;
        private System.Windows.Forms.Label labelToggleAkwaysOnTop;
        private System.Windows.Forms.ComboBox comboBoxAlwaysOnTopKey;
    }
}