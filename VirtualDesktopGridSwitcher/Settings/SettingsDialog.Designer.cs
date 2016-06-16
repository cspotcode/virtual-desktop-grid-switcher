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
            this.textBoxRows = new System.Windows.Forms.TextBox();
            this.textBoxColumns = new System.Windows.Forms.TextBox();
            this.buttonApply = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.checkBoxActivateWebBrowser = new System.Windows.Forms.CheckBox();
            this.toolTipSettingsDialog = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // labelRows
            // 
            this.labelRows.AutoSize = true;
            this.labelRows.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelRows.Location = new System.Drawing.Point(161, 11);
            this.labelRows.Name = "labelRows";
            this.labelRows.Size = new System.Drawing.Size(48, 16);
            this.labelRows.TabIndex = 2;
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
            this.labelColumns.TabIndex = 0;
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
            this.checkBoxWrapAround.TabIndex = 4;
            this.checkBoxWrapAround.Text = "Wrap Around";
            this.toolTipSettingsDialog.SetToolTip(this.checkBoxWrapAround, "Switch/Move to opposite side of grid when move beyond edge");
            this.checkBoxWrapAround.UseVisualStyleBackColor = true;
            // 
            // textBoxRows
            // 
            this.textBoxRows.Location = new System.Drawing.Point(215, 10);
            this.textBoxRows.Name = "textBoxRows";
            this.textBoxRows.Size = new System.Drawing.Size(49, 20);
            this.textBoxRows.TabIndex = 3;
            this.toolTipSettingsDialog.SetToolTip(this.textBoxRows, "Number of rows in desktop grid");
            // 
            // textBoxColumns
            // 
            this.textBoxColumns.Location = new System.Drawing.Point(86, 10);
            this.textBoxColumns.Name = "textBoxColumns";
            this.textBoxColumns.Size = new System.Drawing.Size(49, 20);
            this.textBoxColumns.TabIndex = 1;
            this.toolTipSettingsDialog.SetToolTip(this.textBoxColumns, "Number of columns in desktop grid");
            // 
            // buttonApply
            // 
            this.buttonApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonApply.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonApply.Font = new System.Drawing.Font("Verdana", 9.75F);
            this.buttonApply.Location = new System.Drawing.Point(310, 71);
            this.buttonApply.Name = "buttonApply";
            this.buttonApply.Size = new System.Drawing.Size(75, 23);
            this.buttonApply.TabIndex = 30;
            this.buttonApply.Text = "&Apply";
            this.buttonApply.UseVisualStyleBackColor = true;
            this.buttonApply.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Font = new System.Drawing.Font("Verdana", 9.75F);
            this.buttonCancel.Location = new System.Drawing.Point(393, 71);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 31;
            this.buttonCancel.Text = "&Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // checkBoxActivateWebBrowser
            // 
            this.checkBoxActivateWebBrowser.AutoSize = true;
            this.checkBoxActivateWebBrowser.Font = new System.Drawing.Font("Verdana", 9.75F);
            this.checkBoxActivateWebBrowser.Location = new System.Drawing.Point(15, 36);
            this.checkBoxActivateWebBrowser.Name = "checkBoxActivateWebBrowser";
            this.checkBoxActivateWebBrowser.Size = new System.Drawing.Size(203, 20);
            this.checkBoxActivateWebBrowser.TabIndex = 29;
            this.checkBoxActivateWebBrowser.Text = "Default Browser Activation";
            this.toolTipSettingsDialog.SetToolTip(this.checkBoxActivateWebBrowser, "On desktop switch, last active default browser window on that desktop is activate" +
        "d before active window so that links open in current desktop");
            this.checkBoxActivateWebBrowser.UseVisualStyleBackColor = true;
            // 
            // SettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(480, 106);
            this.Controls.Add(this.checkBoxActivateWebBrowser);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonApply);
            this.Controls.Add(this.textBoxColumns);
            this.Controls.Add(this.textBoxRows);
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
        private System.Windows.Forms.TextBox textBoxRows;
        private System.Windows.Forms.TextBox textBoxColumns;
        private System.Windows.Forms.Button buttonApply;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.CheckBox checkBoxActivateWebBrowser;
        private System.Windows.Forms.ToolTip toolTipSettingsDialog;
    }
}