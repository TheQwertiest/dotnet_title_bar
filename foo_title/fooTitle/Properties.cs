/*
    Copyright 2005 - 2006 Roman Plasil
	http://foo-title.sourceforge.net
    This file is part of foo_title.

    foo_title is free software; you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published by
    the Free Software Foundation; either version 2.1 of the License, or
    (at your option) any later version.

    foo_title is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with foo_title; if not, write to the Free Software
    Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
*/
using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

using fooTitle.Config;

namespace fooTitle {
    class Properties : fooManagedWrapper.CManagedPrefPage_v3 {
    //class Properties : System.Windows.Forms.Form {

        public override void Reset() {
            ConfValuesManager.GetInstance().Reset();
        }

        public override void Apply() {
            ConfValuesManager.GetInstance().SaveTo(Main.GetInstance().Config);
        }

        public override bool HasChanged() {
            return ConfValuesManager.GetInstance().HasChanged();
        }

        class SkinListEntry {
            public string path;

            public SkinListEntry(string _path) {
                path = _path;
            }

            public override string ToString() {
                return Path.GetFileName(path);
            }
        }

        protected AutoWrapperCreator autoWrapperCreator = new AutoWrapperCreator();
        protected RadioGroupWrapper showWhenWrapper;
        protected RadioGroupWrapper windowPositionWrapper;
        private CheckBox restoreTopmostCheckbox;
        private Label opacityNormalLabel;
        private Label opacityMouseOverLabel;
        private GroupBox groupBox1;
        private Label label15;
        private Label label14;
        private Label artLoadMaxLabelRight;
        private NumericUpDown artLoadMaxNumber;
        private Label artLoadMaxLabelLeft;
        private Label artLoadEveryLabelRight;
        private NumericUpDown artLoadEveryNumber;
        private Label artLoadEveryLabelLeft;
        private GroupBox groupBox3;
        private Button openSkinDirBtn;
        private GroupBox groupBox4;
        private CheckBox edgeSnapCheckBox;
        private NumericUpDown posYnumbox;
        private Label label4;
        private NumericUpDown posXnumbox;
        private Label label8;
        private Label label2;
        private GroupBox groupBox5;
        private RadioButton enableDraggingPropsOpenRadio;
        private RadioButton enableDraggingAlwaysRadio;
        protected RadioGroupWrapper popupShowingWrapper;
        private RadioButton enableDraggingNeverRadio;
        private GroupBox groupBox6;
        private GroupBox groupBox2;
        private Label label3;
        private TrackBar updateIntervalTrackBar;
        private Label label1;
        private Label label7;
        private Label label16;
        private Label label13;
        private NumericUpDown timeBeforeFadeTextBox;
        private CheckBox checkBox1;
        private GroupBox groupBox7;
        private Label label17;
        private Label label18;
        private NumericUpDown numericUpDown1;
        private Label label19;
        private Label label20;
        private NumericUpDown numericUpDown2;
        private CheckBox checkBox2;
        private CheckBox checkBox3;
        private CheckBox checkBox4;
        protected RadioGroupWrapper enableDraggingWrapper;

        public Properties(Main _main) : base(new Guid(1414, 548, 7868, 98, 46, 78, 12, 35, 14, 47, 68), fooManagedWrapper.CManagedPrefPage_v3.guid_display) {
            main = _main;
            InitializeComponent();

            showWhenWrapper = new RadioGroupWrapper("display/showWhen", this);
            showWhenWrapper.AddRadioButton(alwaysRadio);
            showWhenWrapper.AddRadioButton(minimizedRadio);
            showWhenWrapper.AddRadioButton(neverRadio);

            windowPositionWrapper = new RadioGroupWrapper("display/windowPosition", this);
            windowPositionWrapper.AddRadioButton(alwaysOnTopRadio);
            windowPositionWrapper.AddRadioButton(onDesktopRadio);
            windowPositionWrapper.AddRadioButton(normalRadio);

            popupShowingWrapper = new RadioGroupWrapper("showControl/popupShowing", this);
            popupShowingWrapper.AddRadioButton(allTheTimeRadio);
            popupShowingWrapper.AddRadioButton(onlyWhenRadio);

            enableDraggingWrapper = new RadioGroupWrapper("display/enableDragging", this);
            enableDraggingWrapper.AddRadioButton(enableDraggingAlwaysRadio);
            enableDraggingWrapper.AddRadioButton(enableDraggingPropsOpenRadio);
            enableDraggingWrapper.AddRadioButton(enableDraggingNeverRadio);

            autoWrapperCreator.CreateWrappers(this);
        }

        protected void addSkin(string path) {
            SkinListEntry current = new SkinListEntry(path);
            skinsList.Items.Add(current);
            if (path == main.SkinPath.Value) {
                skinsList.SelectedItem = current;
            }
        }

        protected void fillSkinList() {
            skinsList.Items.Clear();

            try {
                foreach (string path in System.IO.Directory.GetDirectories(Main.UserDataDir)) {
                    addSkin(path);
                }
            } catch (Exception) {
                fooManagedWrapper.CConsole.Write(String.Format("Failed to read from {0}.", Main.UserDataDir));
            }
        }

        public void UpdateValues() {
            fillSkinList();

            Assembly myAssembly = Assembly.GetExecutingAssembly();
            versionLabel.Text = "Version: " + myAssembly.GetName().Version.ToString();
        }

        public void DiscardChanges() {
            ConfValuesManager.GetInstance().LoadFrom(Main.GetInstance().Config);
        }

        #region Windows Form Designer generated code
        private SafeTabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.ListBox skinsList;
        private System.Windows.Forms.Button applySkinBtn;
        private System.Windows.Forms.GroupBox showWhenBox;
        private System.Windows.Forms.RadioButton neverRadio;
        private System.Windows.Forms.RadioButton minimizedRadio;
        private System.Windows.Forms.RadioButton alwaysRadio;
        private System.Windows.Forms.GroupBox opacityOpts;
        private System.Windows.Forms.TrackBar normalOpacityTrackBar;
        private System.Windows.Forms.TrackBar overOpacityTrackBar;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label versionLabel;
        private System.Windows.Forms.GroupBox zOrderBox;
        private System.Windows.Forms.RadioButton onDesktopRadio;
        private System.Windows.Forms.RadioButton normalRadio;
        private System.Windows.Forms.RadioButton alwaysOnTopRadio;
        private System.Windows.Forms.GroupBox popupBox;
        private System.Windows.Forms.RadioButton onlyWhenRadio;
        private System.Windows.Forms.RadioButton allTheTimeRadio;

        protected Main main;

        private void InitializeComponent() {
            this.skinsList = new System.Windows.Forms.ListBox();
            this.applySkinBtn = new System.Windows.Forms.Button();
            this.showWhenBox = new System.Windows.Forms.GroupBox();
            this.neverRadio = new System.Windows.Forms.RadioButton();
            this.minimizedRadio = new System.Windows.Forms.RadioButton();
            this.alwaysRadio = new System.Windows.Forms.RadioButton();
            this.opacityOpts = new System.Windows.Forms.GroupBox();
            this.opacityNormalLabel = new System.Windows.Forms.Label();
            this.opacityMouseOverLabel = new System.Windows.Forms.Label();
            this.normalOpacityTrackBar = new System.Windows.Forms.TrackBar();
            this.overOpacityTrackBar = new System.Windows.Forms.TrackBar();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.versionLabel = new System.Windows.Forms.Label();
            this.zOrderBox = new System.Windows.Forms.GroupBox();
            this.restoreTopmostCheckbox = new System.Windows.Forms.CheckBox();
            this.onDesktopRadio = new System.Windows.Forms.RadioButton();
            this.normalRadio = new System.Windows.Forms.RadioButton();
            this.alwaysOnTopRadio = new System.Windows.Forms.RadioButton();
            this.popupBox = new System.Windows.Forms.GroupBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.onlyWhenRadio = new System.Windows.Forms.RadioButton();
            this.allTheTimeRadio = new System.Windows.Forms.RadioButton();
            this.tabControl1 = new fooTitle.SafeTabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.label17 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.label19 = new System.Windows.Forms.Label();
            this.label20 = new System.Windows.Forms.Label();
            this.numericUpDown2 = new System.Windows.Forms.NumericUpDown();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.checkBox3 = new System.Windows.Forms.CheckBox();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.label16 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.timeBeforeFadeTextBox = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.openSkinDirBtn = new System.Windows.Forms.Button();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.updateIntervalTrackBar = new System.Windows.Forms.TrackBar();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.enableDraggingNeverRadio = new System.Windows.Forms.RadioButton();
            this.enableDraggingPropsOpenRadio = new System.Windows.Forms.RadioButton();
            this.enableDraggingAlwaysRadio = new System.Windows.Forms.RadioButton();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.edgeSnapCheckBox = new System.Windows.Forms.CheckBox();
            this.posYnumbox = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.posXnumbox = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label15 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.artLoadMaxLabelRight = new System.Windows.Forms.Label();
            this.artLoadMaxNumber = new System.Windows.Forms.NumericUpDown();
            this.artLoadMaxLabelLeft = new System.Windows.Forms.Label();
            this.artLoadEveryLabelRight = new System.Windows.Forms.Label();
            this.artLoadEveryNumber = new System.Windows.Forms.NumericUpDown();
            this.artLoadEveryLabelLeft = new System.Windows.Forms.Label();
            this.checkBox4 = new System.Windows.Forms.CheckBox();
            this.showWhenBox.SuspendLayout();
            this.opacityOpts.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.normalOpacityTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.overOpacityTrackBar)).BeginInit();
            this.zOrderBox.SuspendLayout();
            this.popupBox.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.groupBox7.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).BeginInit();
            this.groupBox6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.timeBeforeFadeTextBox)).BeginInit();
            this.groupBox3.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.updateIntervalTrackBar)).BeginInit();
            this.groupBox5.SuspendLayout();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.posYnumbox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.posXnumbox)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.artLoadMaxNumber)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.artLoadEveryNumber)).BeginInit();
            this.SuspendLayout();
            // 
            // skinsList
            // 
            this.skinsList.FormattingEnabled = true;
            this.skinsList.Location = new System.Drawing.Point(6, 19);
            this.skinsList.Name = "skinsList";
            this.skinsList.Size = new System.Drawing.Size(214, 225);
            this.skinsList.TabIndex = 0;
            // 
            // applySkinBtn
            // 
            this.applySkinBtn.Location = new System.Drawing.Point(6, 249);
            this.applySkinBtn.Name = "applySkinBtn";
            this.applySkinBtn.Size = new System.Drawing.Size(104, 23);
            this.applySkinBtn.TabIndex = 1;
            this.applySkinBtn.Text = "Apply skin";
            this.applySkinBtn.UseVisualStyleBackColor = true;
            this.applySkinBtn.Click += new System.EventHandler(this.applySkinBtn_Click);
            // 
            // showWhenBox
            // 
            this.showWhenBox.Controls.Add(this.neverRadio);
            this.showWhenBox.Controls.Add(this.minimizedRadio);
            this.showWhenBox.Controls.Add(this.alwaysRadio);
            this.showWhenBox.Location = new System.Drawing.Point(6, 290);
            this.showWhenBox.Name = "showWhenBox";
            this.showWhenBox.Size = new System.Drawing.Size(226, 89);
            this.showWhenBox.TabIndex = 9;
            this.showWhenBox.TabStop = false;
            this.showWhenBox.Text = "Enabled";
            // 
            // neverRadio
            // 
            this.neverRadio.AutoSize = true;
            this.neverRadio.Location = new System.Drawing.Point(6, 65);
            this.neverRadio.Name = "neverRadio";
            this.neverRadio.Size = new System.Drawing.Size(54, 17);
            this.neverRadio.TabIndex = 2;
            this.neverRadio.TabStop = true;
            this.neverRadio.Text = "Never";
            this.neverRadio.UseVisualStyleBackColor = true;
            // 
            // minimizedRadio
            // 
            this.minimizedRadio.AutoSize = true;
            this.minimizedRadio.Location = new System.Drawing.Point(6, 42);
            this.minimizedRadio.Name = "minimizedRadio";
            this.minimizedRadio.Size = new System.Drawing.Size(169, 17);
            this.minimizedRadio.TabIndex = 1;
            this.minimizedRadio.TabStop = true;
            this.minimizedRadio.Text = "When foobar2000 is minimized";
            this.minimizedRadio.UseVisualStyleBackColor = true;
            // 
            // alwaysRadio
            // 
            this.alwaysRadio.AutoSize = true;
            this.alwaysRadio.Location = new System.Drawing.Point(6, 19);
            this.alwaysRadio.Name = "alwaysRadio";
            this.alwaysRadio.Size = new System.Drawing.Size(58, 17);
            this.alwaysRadio.TabIndex = 0;
            this.alwaysRadio.TabStop = true;
            this.alwaysRadio.Text = "Always";
            this.alwaysRadio.UseVisualStyleBackColor = true;
            // 
            // opacityOpts
            // 
            this.opacityOpts.Controls.Add(this.opacityNormalLabel);
            this.opacityOpts.Controls.Add(this.opacityMouseOverLabel);
            this.opacityOpts.Controls.Add(this.normalOpacityTrackBar);
            this.opacityOpts.Controls.Add(this.overOpacityTrackBar);
            this.opacityOpts.Controls.Add(this.label6);
            this.opacityOpts.Controls.Add(this.label5);
            this.opacityOpts.Location = new System.Drawing.Point(6, 6);
            this.opacityOpts.Name = "opacityOpts";
            this.opacityOpts.Size = new System.Drawing.Size(226, 168);
            this.opacityOpts.TabIndex = 10;
            this.opacityOpts.TabStop = false;
            this.opacityOpts.Text = "Opacity";
            // 
            // opacityNormalLabel
            // 
            this.opacityNormalLabel.AutoSize = true;
            this.opacityNormalLabel.Location = new System.Drawing.Point(171, 26);
            this.opacityNormalLabel.Name = "opacityNormalLabel";
            this.opacityNormalLabel.Size = new System.Drawing.Size(35, 13);
            this.opacityNormalLabel.TabIndex = 7;
            this.opacityNormalLabel.Tag = "display/normalOpacity";
            this.opacityNormalLabel.Text = "label8";
            this.opacityNormalLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // opacityMouseOverLabel
            // 
            this.opacityMouseOverLabel.AutoSize = true;
            this.opacityMouseOverLabel.Location = new System.Drawing.Point(171, 95);
            this.opacityMouseOverLabel.Name = "opacityMouseOverLabel";
            this.opacityMouseOverLabel.Size = new System.Drawing.Size(35, 13);
            this.opacityMouseOverLabel.TabIndex = 6;
            this.opacityMouseOverLabel.Tag = "display/overOpacity";
            this.opacityMouseOverLabel.Text = "label8";
            this.opacityMouseOverLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // normalOpacityTrackBar
            // 
            this.normalOpacityTrackBar.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.normalOpacityTrackBar.Location = new System.Drawing.Point(6, 42);
            this.normalOpacityTrackBar.Maximum = 255;
            this.normalOpacityTrackBar.Minimum = 5;
            this.normalOpacityTrackBar.Name = "normalOpacityTrackBar";
            this.normalOpacityTrackBar.Size = new System.Drawing.Size(200, 45);
            this.normalOpacityTrackBar.TabIndex = 3;
            this.normalOpacityTrackBar.Tag = "display/normalOpacity";
            this.normalOpacityTrackBar.TickFrequency = 16;
            this.normalOpacityTrackBar.Value = 5;
            // 
            // overOpacityTrackBar
            // 
            this.overOpacityTrackBar.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.overOpacityTrackBar.Location = new System.Drawing.Point(6, 111);
            this.overOpacityTrackBar.Maximum = 255;
            this.overOpacityTrackBar.Minimum = 5;
            this.overOpacityTrackBar.Name = "overOpacityTrackBar";
            this.overOpacityTrackBar.Size = new System.Drawing.Size(200, 45);
            this.overOpacityTrackBar.TabIndex = 2;
            this.overOpacityTrackBar.Tag = "display/overOpacity";
            this.overOpacityTrackBar.TickFrequency = 16;
            this.overOpacityTrackBar.Value = 5;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 95);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(93, 13);
            this.label6.TabIndex = 1;
            this.label6.Text = "Opacity on trigger:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 26);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(80, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "Normal opacity:";
            // 
            // versionLabel
            // 
            this.versionLabel.AutoSize = true;
            this.versionLabel.Location = new System.Drawing.Point(9, 393);
            this.versionLabel.Name = "versionLabel";
            this.versionLabel.Size = new System.Drawing.Size(35, 13);
            this.versionLabel.TabIndex = 11;
            this.versionLabel.Text = "label8";
            // 
            // zOrderBox
            // 
            this.zOrderBox.Controls.Add(this.restoreTopmostCheckbox);
            this.zOrderBox.Controls.Add(this.onDesktopRadio);
            this.zOrderBox.Controls.Add(this.normalRadio);
            this.zOrderBox.Controls.Add(this.alwaysOnTopRadio);
            this.zOrderBox.Location = new System.Drawing.Point(238, 290);
            this.zOrderBox.Name = "zOrderBox";
            this.zOrderBox.Size = new System.Drawing.Size(225, 125);
            this.zOrderBox.TabIndex = 12;
            this.zOrderBox.TabStop = false;
            this.zOrderBox.Text = "Position";
            // 
            // restoreTopmostCheckbox
            // 
            this.restoreTopmostCheckbox.AutoSize = true;
            this.restoreTopmostCheckbox.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.restoreTopmostCheckbox.Location = new System.Drawing.Point(16, 42);
            this.restoreTopmostCheckbox.Name = "restoreTopmostCheckbox";
            this.restoreTopmostCheckbox.Size = new System.Drawing.Size(205, 30);
            this.restoreTopmostCheckbox.TabIndex = 1;
            this.restoreTopmostCheckbox.Tag = "display/reShowOnTop";
            this.restoreTopmostCheckbox.Text = "Restore topmost position every minute\r\nto work around the Windows problem.";
            this.restoreTopmostCheckbox.UseVisualStyleBackColor = true;
            // 
            // onDesktopRadio
            // 
            this.onDesktopRadio.AutoSize = true;
            this.onDesktopRadio.Location = new System.Drawing.Point(6, 101);
            this.onDesktopRadio.Name = "onDesktopRadio";
            this.onDesktopRadio.Size = new System.Drawing.Size(80, 17);
            this.onDesktopRadio.TabIndex = 2;
            this.onDesktopRadio.TabStop = true;
            this.onDesktopRadio.Text = "On desktop";
            this.onDesktopRadio.UseVisualStyleBackColor = true;
            // 
            // normalRadio
            // 
            this.normalRadio.AutoSize = true;
            this.normalRadio.Location = new System.Drawing.Point(6, 78);
            this.normalRadio.Name = "normalRadio";
            this.normalRadio.Size = new System.Drawing.Size(58, 17);
            this.normalRadio.TabIndex = 1;
            this.normalRadio.TabStop = true;
            this.normalRadio.Text = "Normal";
            this.normalRadio.UseVisualStyleBackColor = true;
            // 
            // alwaysOnTopRadio
            // 
            this.alwaysOnTopRadio.AutoSize = true;
            this.alwaysOnTopRadio.Location = new System.Drawing.Point(6, 19);
            this.alwaysOnTopRadio.Name = "alwaysOnTopRadio";
            this.alwaysOnTopRadio.Size = new System.Drawing.Size(91, 17);
            this.alwaysOnTopRadio.TabIndex = 0;
            this.alwaysOnTopRadio.TabStop = true;
            this.alwaysOnTopRadio.Text = "Always on top";
            this.alwaysOnTopRadio.UseVisualStyleBackColor = true;
            // 
            // popupBox
            // 
            this.popupBox.Controls.Add(this.checkBox1);
            this.popupBox.Controls.Add(this.onlyWhenRadio);
            this.popupBox.Controls.Add(this.allTheTimeRadio);
            this.popupBox.Location = new System.Drawing.Point(238, 6);
            this.popupBox.Name = "popupBox";
            this.popupBox.Size = new System.Drawing.Size(225, 84);
            this.popupBox.TabIndex = 13;
            this.popupBox.TabStop = false;
            this.popupBox.Text = "Show Popup";
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(16, 61);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(136, 17);
            this.checkBox1.TabIndex = 22;
            this.checkBox1.Tag = "showControl/showWhenNotPlaying";
            this.checkBox1.Text = "Show when not playing";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // onlyWhenRadio
            // 
            this.onlyWhenRadio.AutoSize = true;
            this.onlyWhenRadio.Location = new System.Drawing.Point(6, 39);
            this.onlyWhenRadio.Name = "onlyWhenRadio";
            this.onlyWhenRadio.Size = new System.Drawing.Size(71, 17);
            this.onlyWhenRadio.TabIndex = 1;
            this.onlyWhenRadio.TabStop = true;
            this.onlyWhenRadio.Text = "On trigger";
            this.onlyWhenRadio.UseVisualStyleBackColor = true;
            // 
            // allTheTimeRadio
            // 
            this.allTheTimeRadio.AutoSize = true;
            this.allTheTimeRadio.Checked = true;
            this.allTheTimeRadio.Location = new System.Drawing.Point(6, 16);
            this.allTheTimeRadio.Name = "allTheTimeRadio";
            this.allTheTimeRadio.Size = new System.Drawing.Size(58, 17);
            this.allTheTimeRadio.TabIndex = 0;
            this.allTheTimeRadio.TabStop = true;
            this.allTheTimeRadio.Text = "Always";
            this.allTheTimeRadio.UseVisualStyleBackColor = true;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(12, 3);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(477, 457);
            this.tabControl1.TabIndex = 14;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.groupBox7);
            this.tabPage1.Controls.Add(this.groupBox6);
            this.tabPage1.Controls.Add(this.groupBox3);
            this.tabPage1.Controls.Add(this.popupBox);
            this.tabPage1.Controls.Add(this.zOrderBox);
            this.tabPage1.Controls.Add(this.versionLabel);
            this.tabPage1.Controls.Add(this.showWhenBox);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(469, 431);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Appearance";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // groupBox7
            // 
            this.groupBox7.Controls.Add(this.label17);
            this.groupBox7.Controls.Add(this.label18);
            this.groupBox7.Controls.Add(this.numericUpDown1);
            this.groupBox7.Controls.Add(this.label19);
            this.groupBox7.Controls.Add(this.label20);
            this.groupBox7.Controls.Add(this.numericUpDown2);
            this.groupBox7.Controls.Add(this.checkBox2);
            this.groupBox7.Controls.Add(this.checkBox3);
            this.groupBox7.Location = new System.Drawing.Point(238, 96);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(225, 118);
            this.groupBox7.TabIndex = 22;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "Popup Triggers";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(120, 91);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(47, 13);
            this.label17.TabIndex = 20;
            this.label17.Text = "seconds";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(16, 91);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(28, 13);
            this.label18.TabIndex = 19;
            this.label18.Text = "Stay";
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Location = new System.Drawing.Point(50, 88);
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(65, 20);
            this.numericUpDown1.TabIndex = 18;
            this.numericUpDown1.Tag = "showControl/beforeSongEndsStay";
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(120, 42);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(47, 13);
            this.label19.TabIndex = 17;
            this.label19.Text = "seconds";
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(16, 42);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(28, 13);
            this.label20.TabIndex = 16;
            this.label20.Text = "Stay";
            // 
            // numericUpDown2
            // 
            this.numericUpDown2.Location = new System.Drawing.Point(50, 39);
            this.numericUpDown2.Name = "numericUpDown2";
            this.numericUpDown2.Size = new System.Drawing.Size(65, 20);
            this.numericUpDown2.TabIndex = 15;
            this.numericUpDown2.Tag = "showControl/onSongStartStay";
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Location = new System.Drawing.Point(16, 68);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(109, 17);
            this.checkBox2.TabIndex = 14;
            this.checkBox2.Tag = "showControl/beforeSongEnds";
            this.checkBox2.Text = "Before song ends";
            this.checkBox2.UseVisualStyleBackColor = true;
            // 
            // checkBox3
            // 
            this.checkBox3.AutoSize = true;
            this.checkBox3.Location = new System.Drawing.Point(16, 19);
            this.checkBox3.Name = "checkBox3";
            this.checkBox3.Size = new System.Drawing.Size(182, 17);
            this.checkBox3.TabIndex = 3;
            this.checkBox3.Tag = "showControl/onSongStart";
            this.checkBox3.Text = "On song start / track title change";
            this.checkBox3.UseVisualStyleBackColor = true;
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.label16);
            this.groupBox6.Controls.Add(this.label13);
            this.groupBox6.Controls.Add(this.timeBeforeFadeTextBox);
            this.groupBox6.Controls.Add(this.label7);
            this.groupBox6.Location = new System.Drawing.Point(238, 220);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(225, 64);
            this.groupBox6.TabIndex = 17;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Popup Peek";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(121, 38);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(47, 13);
            this.label16.TabIndex = 21;
            this.label16.Text = "seconds";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(16, 38);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(28, 13);
            this.label13.TabIndex = 20;
            this.label13.Text = "Stay";
            // 
            // timeBeforeFadeTextBox
            // 
            this.timeBeforeFadeTextBox.Location = new System.Drawing.Point(50, 36);
            this.timeBeforeFadeTextBox.Name = "timeBeforeFadeTextBox";
            this.timeBeforeFadeTextBox.Size = new System.Drawing.Size(65, 20);
            this.timeBeforeFadeTextBox.TabIndex = 19;
            this.timeBeforeFadeTextBox.Tag = "showControl/timeBeforeFade";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(16, 17);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(115, 13);
            this.label7.TabIndex = 0;
            this.label7.Text = "Time before fade away";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.openSkinDirBtn);
            this.groupBox3.Controls.Add(this.skinsList);
            this.groupBox3.Controls.Add(this.applySkinBtn);
            this.groupBox3.Location = new System.Drawing.Point(6, 6);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(226, 278);
            this.groupBox3.TabIndex = 16;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Installed skins";
            // 
            // openSkinDirBtn
            // 
            this.openSkinDirBtn.Location = new System.Drawing.Point(116, 249);
            this.openSkinDirBtn.Name = "openSkinDirBtn";
            this.openSkinDirBtn.Size = new System.Drawing.Size(104, 23);
            this.openSkinDirBtn.TabIndex = 2;
            this.openSkinDirBtn.Text = "Open directory";
            this.openSkinDirBtn.UseVisualStyleBackColor = true;
            this.openSkinDirBtn.Click += new System.EventHandler(this.openSkinDirBtn_Click);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.groupBox2);
            this.tabPage2.Controls.Add(this.groupBox5);
            this.tabPage2.Controls.Add(this.groupBox4);
            this.tabPage2.Controls.Add(this.groupBox1);
            this.tabPage2.Controls.Add(this.opacityOpts);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(469, 431);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Misc";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.updateIntervalTrackBar);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Location = new System.Drawing.Point(6, 180);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(226, 84);
            this.groupBox2.TabIndex = 16;
            this.groupBox2.TabStop = false;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(163, 16);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(35, 13);
            this.label3.TabIndex = 8;
            this.label3.Tag = "display/updateInterval";
            this.label3.Text = "label8";
            this.label3.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // updateIntervalTrackBar
            // 
            this.updateIntervalTrackBar.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.updateIntervalTrackBar.LargeChange = 100;
            this.updateIntervalTrackBar.Location = new System.Drawing.Point(6, 32);
            this.updateIntervalTrackBar.Maximum = 500;
            this.updateIntervalTrackBar.Minimum = 50;
            this.updateIntervalTrackBar.Name = "updateIntervalTrackBar";
            this.updateIntervalTrackBar.Size = new System.Drawing.Size(200, 45);
            this.updateIntervalTrackBar.SmallChange = 10;
            this.updateIntervalTrackBar.TabIndex = 2;
            this.updateIntervalTrackBar.Tag = "display/updateInterval";
            this.updateIntervalTrackBar.TickFrequency = 50;
            this.updateIntervalTrackBar.Value = 50;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(82, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Update interval:";
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.enableDraggingNeverRadio);
            this.groupBox5.Controls.Add(this.enableDraggingPropsOpenRadio);
            this.groupBox5.Controls.Add(this.enableDraggingAlwaysRadio);
            this.groupBox5.Location = new System.Drawing.Point(238, 314);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(225, 90);
            this.groupBox5.TabIndex = 13;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Enable Dragging";
            // 
            // enableDraggingNeverRadio
            // 
            this.enableDraggingNeverRadio.AutoSize = true;
            this.enableDraggingNeverRadio.Location = new System.Drawing.Point(6, 65);
            this.enableDraggingNeverRadio.Name = "enableDraggingNeverRadio";
            this.enableDraggingNeverRadio.Size = new System.Drawing.Size(54, 17);
            this.enableDraggingNeverRadio.TabIndex = 16;
            this.enableDraggingNeverRadio.TabStop = true;
            this.enableDraggingNeverRadio.Text = "Never";
            this.enableDraggingNeverRadio.UseVisualStyleBackColor = true;
            // 
            // enableDraggingPropsOpenRadio
            // 
            this.enableDraggingPropsOpenRadio.AutoSize = true;
            this.enableDraggingPropsOpenRadio.Location = new System.Drawing.Point(6, 42);
            this.enableDraggingPropsOpenRadio.Name = "enableDraggingPropsOpenRadio";
            this.enableDraggingPropsOpenRadio.Size = new System.Drawing.Size(208, 17);
            this.enableDraggingPropsOpenRadio.TabIndex = 15;
            this.enableDraggingPropsOpenRadio.TabStop = true;
            this.enableDraggingPropsOpenRadio.Text = "Only when these preferences are open";
            this.enableDraggingPropsOpenRadio.UseVisualStyleBackColor = true;
            // 
            // enableDraggingAlwaysRadio
            // 
            this.enableDraggingAlwaysRadio.AutoSize = true;
            this.enableDraggingAlwaysRadio.Location = new System.Drawing.Point(6, 19);
            this.enableDraggingAlwaysRadio.Name = "enableDraggingAlwaysRadio";
            this.enableDraggingAlwaysRadio.Size = new System.Drawing.Size(58, 17);
            this.enableDraggingAlwaysRadio.TabIndex = 14;
            this.enableDraggingAlwaysRadio.TabStop = true;
            this.enableDraggingAlwaysRadio.Text = "Always";
            this.enableDraggingAlwaysRadio.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.checkBox4);
            this.groupBox4.Controls.Add(this.label2);
            this.groupBox4.Controls.Add(this.edgeSnapCheckBox);
            this.groupBox4.Controls.Add(this.posYnumbox);
            this.groupBox4.Controls.Add(this.label4);
            this.groupBox4.Controls.Add(this.posXnumbox);
            this.groupBox4.Controls.Add(this.label8);
            this.groupBox4.Location = new System.Drawing.Point(238, 180);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(225, 128);
            this.groupBox4.TabIndex = 12;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Anchor Position";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.SystemColors.GrayText;
            this.label2.Location = new System.Drawing.Point(6, 94);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(201, 26);
            this.label2.TabIndex = 13;
            this.label2.Text = "If edge snapping is enabled you can hold\r\nCTRL to disable it while dragging.";
            // 
            // edgeSnapCheckBox
            // 
            this.edgeSnapCheckBox.AutoSize = true;
            this.edgeSnapCheckBox.Location = new System.Drawing.Point(9, 74);
            this.edgeSnapCheckBox.Name = "edgeSnapCheckBox";
            this.edgeSnapCheckBox.Size = new System.Drawing.Size(132, 17);
            this.edgeSnapCheckBox.TabIndex = 12;
            this.edgeSnapCheckBox.Tag = "display/edgeSnap";
            this.edgeSnapCheckBox.Text = "Enable edge snapping";
            this.edgeSnapCheckBox.UseVisualStyleBackColor = true;
            // 
            // posYnumbox
            // 
            this.posYnumbox.Location = new System.Drawing.Point(137, 24);
            this.posYnumbox.Name = "posYnumbox";
            this.posYnumbox.Size = new System.Drawing.Size(65, 20);
            this.posYnumbox.TabIndex = 11;
            this.posYnumbox.Tag = "display/positionY";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(114, 26);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(17, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "Y:";
            // 
            // posXnumbox
            // 
            this.posXnumbox.Location = new System.Drawing.Point(29, 24);
            this.posXnumbox.Name = "posXnumbox";
            this.posXnumbox.Size = new System.Drawing.Size(65, 20);
            this.posXnumbox.TabIndex = 9;
            this.posXnumbox.Tag = "display/positionX";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 26);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(17, 13);
            this.label8.TabIndex = 8;
            this.label8.Text = "X:";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label15);
            this.groupBox1.Controls.Add(this.label14);
            this.groupBox1.Controls.Add(this.artLoadMaxLabelRight);
            this.groupBox1.Controls.Add(this.artLoadMaxNumber);
            this.groupBox1.Controls.Add(this.artLoadMaxLabelLeft);
            this.groupBox1.Controls.Add(this.artLoadEveryLabelRight);
            this.groupBox1.Controls.Add(this.artLoadEveryNumber);
            this.groupBox1.Controls.Add(this.artLoadEveryLabelLeft);
            this.groupBox1.Location = new System.Drawing.Point(238, 6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(225, 167);
            this.groupBox1.TabIndex = 11;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Album Art Reloading";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(6, 16);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(172, 13);
            this.label15.TabIndex = 7;
            this.label15.Text = "If there is no album art loaded retry:";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.ForeColor = System.Drawing.SystemColors.GrayText;
            this.label14.Location = new System.Drawing.Point(6, 94);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(205, 65);
            this.label14.TabIndex = 6;
            this.label14.Text = "* 0 = never, -1 = no maximum\r\n\r\nUse this if you use an external album art\r\nloader" +
    " that starts loading art after the song\r\nhas alread started playing.";
            // 
            // artLoadMaxLabelRight
            // 
            this.artLoadMaxLabelRight.AutoSize = true;
            this.artLoadMaxLabelRight.Location = new System.Drawing.Point(134, 68);
            this.artLoadMaxLabelRight.Name = "artLoadMaxLabelRight";
            this.artLoadMaxLabelRight.Size = new System.Drawing.Size(85, 13);
            this.artLoadMaxLabelRight.TabIndex = 5;
            this.artLoadMaxLabelRight.Text = "time(s) per song*";
            // 
            // artLoadMaxNumber
            // 
            this.artLoadMaxNumber.Location = new System.Drawing.Point(63, 66);
            this.artLoadMaxNumber.Name = "artLoadMaxNumber";
            this.artLoadMaxNumber.Size = new System.Drawing.Size(65, 20);
            this.artLoadMaxNumber.TabIndex = 4;
            this.artLoadMaxNumber.Tag = "display/artLoadMaxTimes";
            // 
            // artLoadMaxLabelLeft
            // 
            this.artLoadMaxLabelLeft.AutoSize = true;
            this.artLoadMaxLabelLeft.Location = new System.Drawing.Point(6, 68);
            this.artLoadMaxLabelLeft.Name = "artLoadMaxLabelLeft";
            this.artLoadMaxLabelLeft.Size = new System.Drawing.Size(51, 13);
            this.artLoadMaxLabelLeft.TabIndex = 3;
            this.artLoadMaxLabelLeft.Text = "Maximum";
            // 
            // artLoadEveryLabelRight
            // 
            this.artLoadEveryLabelRight.AutoSize = true;
            this.artLoadEveryLabelRight.Location = new System.Drawing.Point(134, 42);
            this.artLoadEveryLabelRight.Name = "artLoadEveryLabelRight";
            this.artLoadEveryLabelRight.Size = new System.Drawing.Size(53, 13);
            this.artLoadEveryLabelRight.TabIndex = 2;
            this.artLoadEveryLabelRight.Text = "second(s)";
            // 
            // artLoadEveryNumber
            // 
            this.artLoadEveryNumber.Location = new System.Drawing.Point(63, 40);
            this.artLoadEveryNumber.Name = "artLoadEveryNumber";
            this.artLoadEveryNumber.Size = new System.Drawing.Size(65, 20);
            this.artLoadEveryNumber.TabIndex = 1;
            this.artLoadEveryNumber.Tag = "display/artLoadEvery";
            // 
            // artLoadEveryLabelLeft
            // 
            this.artLoadEveryLabelLeft.AutoSize = true;
            this.artLoadEveryLabelLeft.Location = new System.Drawing.Point(6, 42);
            this.artLoadEveryLabelLeft.Name = "artLoadEveryLabelLeft";
            this.artLoadEveryLabelLeft.Size = new System.Drawing.Size(34, 13);
            this.artLoadEveryLabelLeft.TabIndex = 0;
            this.artLoadEveryLabelLeft.Text = "Every";
            // 
            // checkBox4
            // 
            this.checkBox4.AutoSize = true;
            this.checkBox4.Location = new System.Drawing.Point(9, 51);
            this.checkBox4.Name = "checkBox4";
            this.checkBox4.Size = new System.Drawing.Size(96, 17);
            this.checkBox4.TabIndex = 14;
            this.checkBox4.Tag = "display/drawAnchor";
            this.checkBox4.Text = "Display anchor";
            this.checkBox4.UseVisualStyleBackColor = true;
            // 
            // Properties
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(501, 501);
            this.Controls.Add(this.tabControl1);
            this.Name = "Properties";
            this.Text = "foo_title";
            this.HandleCreated += new System.EventHandler(this.Properties_HandleCreated);
            this.HandleDestroyed += new System.EventHandler(this.Properties_HandleDestroyed);
            this.showWhenBox.ResumeLayout(false);
            this.showWhenBox.PerformLayout();
            this.opacityOpts.ResumeLayout(false);
            this.opacityOpts.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.normalOpacityTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.overOpacityTrackBar)).EndInit();
            this.zOrderBox.ResumeLayout(false);
            this.zOrderBox.PerformLayout();
            this.popupBox.ResumeLayout(false);
            this.popupBox.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.groupBox7.ResumeLayout(false);
            this.groupBox7.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).EndInit();
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.timeBeforeFadeTextBox)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.updateIntervalTrackBar)).EndInit();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.posYnumbox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.posXnumbox)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.artLoadMaxNumber)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.artLoadEveryNumber)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion

        private static bool hasHandle = false;
        public static bool IsOpen {
            get { return hasHandle; }
        }

        private void Properties_HandleCreated(object sender, EventArgs e) {
            UpdateValues();
            hasHandle = true;
        }

        private void Properties_HandleDestroyed(object sender, EventArgs e) {
            DiscardChanges();
            hasHandle = false;
        }

        private void applySkinBtn_Click(object sender, EventArgs e) {
            if (skinsList.SelectedItem == null) {
                return;
            }

            main.SkinPath.ForceUpdate(((SkinListEntry)skinsList.SelectedItem).path);
            OnChange(); // If the control is not wrapped in a ControlWrapper we need to manualy call OnChange
        }

        private void openSkinDirBtn_Click(object sender, EventArgs e) {
            try {
                System.Diagnostics.Process.Start(Main.UserDataDir);
            } catch (Exception ex) {
                MessageBox.Show("foo_title - There was an error opening directory " + Main.UserDataDir + ":\n" + ex.Message, "foo_title");
            }
        }
    }
}
