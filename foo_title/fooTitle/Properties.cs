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
        private CheckBox showWhenNotPlayingCheckbox;
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
        private GroupBox groupBox2;
        private Label label3;
        private Label label1;
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
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.NumericUpDown beforeSongEndsStayTextBox;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.NumericUpDown onSongStartStayTextBox;
        private System.Windows.Forms.ListBox skinsList;
        private System.Windows.Forms.Button applySkinBtn;
        private System.Windows.Forms.TrackBar updateIntervalTrackBar;
        private System.Windows.Forms.GroupBox showWhenBox;
        private System.Windows.Forms.RadioButton neverRadio;
        private System.Windows.Forms.RadioButton minimizedRadio;
        private System.Windows.Forms.RadioButton alwaysRadio;
        private System.Windows.Forms.GroupBox opacityOpts;
        private System.Windows.Forms.TrackBar normalOpacityTrackBar;
        private System.Windows.Forms.TrackBar overOpacityTrackBar;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TrackBar fadeLengthTrackBar;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label fadeLengthLabel;
        private System.Windows.Forms.Label versionLabel;
        private System.Windows.Forms.GroupBox zOrderBox;
        private System.Windows.Forms.RadioButton onDesktopRadio;
        private System.Windows.Forms.RadioButton normalRadio;
        private System.Windows.Forms.RadioButton alwaysOnTopRadio;
        private System.Windows.Forms.GroupBox popupBox;
        private System.Windows.Forms.RadioButton onlyWhenRadio;
        private System.Windows.Forms.RadioButton allTheTimeRadio;
        private System.Windows.Forms.CheckBox beforeSongEndsCheckbox;
        private System.Windows.Forms.CheckBox onSongStartCheckbox;

        protected Main main;

        private void InitializeComponent() {
            this.skinsList = new System.Windows.Forms.ListBox();
            this.applySkinBtn = new System.Windows.Forms.Button();
            this.updateIntervalTrackBar = new System.Windows.Forms.TrackBar();
            this.showWhenBox = new System.Windows.Forms.GroupBox();
            this.neverRadio = new System.Windows.Forms.RadioButton();
            this.minimizedRadio = new System.Windows.Forms.RadioButton();
            this.alwaysRadio = new System.Windows.Forms.RadioButton();
            this.opacityOpts = new System.Windows.Forms.GroupBox();
            this.opacityNormalLabel = new System.Windows.Forms.Label();
            this.opacityMouseOverLabel = new System.Windows.Forms.Label();
            this.fadeLengthLabel = new System.Windows.Forms.Label();
            this.fadeLengthTrackBar = new System.Windows.Forms.TrackBar();
            this.label7 = new System.Windows.Forms.Label();
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
            this.showWhenNotPlayingCheckbox = new System.Windows.Forms.CheckBox();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.beforeSongEndsStayTextBox = new System.Windows.Forms.NumericUpDown();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.onSongStartStayTextBox = new System.Windows.Forms.NumericUpDown();
            this.beforeSongEndsCheckbox = new System.Windows.Forms.CheckBox();
            this.onSongStartCheckbox = new System.Windows.Forms.CheckBox();
            this.onlyWhenRadio = new System.Windows.Forms.RadioButton();
            this.allTheTimeRadio = new System.Windows.Forms.RadioButton();
            this.tabControl1 = new fooTitle.SafeTabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.openSkinDirBtn = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
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
            this.enableDraggingNeverRadio = new System.Windows.Forms.RadioButton();
            ((System.ComponentModel.ISupportInitialize)(this.updateIntervalTrackBar)).BeginInit();
            this.showWhenBox.SuspendLayout();
            this.opacityOpts.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.fadeLengthTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.normalOpacityTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.overOpacityTrackBar)).BeginInit();
            this.zOrderBox.SuspendLayout();
            this.popupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.beforeSongEndsStayTextBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.onSongStartStayTextBox)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.tabPage2.SuspendLayout();
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
            this.opacityOpts.Controls.Add(this.fadeLengthLabel);
            this.opacityOpts.Controls.Add(this.fadeLengthTrackBar);
            this.opacityOpts.Controls.Add(this.label7);
            this.opacityOpts.Controls.Add(this.normalOpacityTrackBar);
            this.opacityOpts.Controls.Add(this.overOpacityTrackBar);
            this.opacityOpts.Controls.Add(this.label6);
            this.opacityOpts.Controls.Add(this.label5);
            this.opacityOpts.Location = new System.Drawing.Point(6, 6);
            this.opacityOpts.Name = "opacityOpts";
            this.opacityOpts.Size = new System.Drawing.Size(226, 213);
            this.opacityOpts.TabIndex = 10;
            this.opacityOpts.TabStop = false;
            this.opacityOpts.Text = "Opacity";
            // 
            // opacityNormalLabel
            // 
            this.opacityNormalLabel.AutoSize = true;
            this.opacityNormalLabel.Location = new System.Drawing.Point(171, 16);
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
            this.opacityMouseOverLabel.Location = new System.Drawing.Point(171, 80);
            this.opacityMouseOverLabel.Name = "opacityMouseOverLabel";
            this.opacityMouseOverLabel.Size = new System.Drawing.Size(35, 13);
            this.opacityMouseOverLabel.TabIndex = 6;
            this.opacityMouseOverLabel.Tag = "display/overOpacity";
            this.opacityMouseOverLabel.Text = "label8";
            this.opacityMouseOverLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // fadeLengthLabel
            // 
            this.fadeLengthLabel.AutoSize = true;
            this.fadeLengthLabel.Location = new System.Drawing.Point(171, 144);
            this.fadeLengthLabel.Name = "fadeLengthLabel";
            this.fadeLengthLabel.Size = new System.Drawing.Size(35, 13);
            this.fadeLengthLabel.TabIndex = 5;
            this.fadeLengthLabel.Tag = "display/fadeLength";
            this.fadeLengthLabel.Text = "label8";
            this.fadeLengthLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // fadeLengthTrackBar
            // 
            this.fadeLengthTrackBar.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.fadeLengthTrackBar.LargeChange = 100;
            this.fadeLengthTrackBar.Location = new System.Drawing.Point(6, 160);
            this.fadeLengthTrackBar.Maximum = 2000;
            this.fadeLengthTrackBar.Name = "fadeLengthTrackBar";
            this.fadeLengthTrackBar.Size = new System.Drawing.Size(200, 45);
            this.fadeLengthTrackBar.SmallChange = 10;
            this.fadeLengthTrackBar.TabIndex = 3;
            this.fadeLengthTrackBar.Tag = "display/fadeLength";
            this.fadeLengthTrackBar.TickFrequency = 100;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 144);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(66, 13);
            this.label7.TabIndex = 4;
            this.label7.Text = "Fade length:";
            // 
            // normalOpacityTrackBar
            // 
            this.normalOpacityTrackBar.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.normalOpacityTrackBar.Location = new System.Drawing.Point(6, 32);
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
            this.overOpacityTrackBar.Location = new System.Drawing.Point(6, 96);
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
            this.label6.Location = new System.Drawing.Point(6, 80);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(119, 13);
            this.label6.TabIndex = 1;
            this.label6.Text = "Opacity on mouse over:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 16);
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
            this.popupBox.Controls.Add(this.showWhenNotPlayingCheckbox);
            this.popupBox.Controls.Add(this.label11);
            this.popupBox.Controls.Add(this.label12);
            this.popupBox.Controls.Add(this.beforeSongEndsStayTextBox);
            this.popupBox.Controls.Add(this.label10);
            this.popupBox.Controls.Add(this.label9);
            this.popupBox.Controls.Add(this.onSongStartStayTextBox);
            this.popupBox.Controls.Add(this.beforeSongEndsCheckbox);
            this.popupBox.Controls.Add(this.onSongStartCheckbox);
            this.popupBox.Controls.Add(this.onlyWhenRadio);
            this.popupBox.Controls.Add(this.allTheTimeRadio);
            this.popupBox.Location = new System.Drawing.Point(238, 96);
            this.popupBox.Name = "popupBox";
            this.popupBox.Size = new System.Drawing.Size(225, 188);
            this.popupBox.TabIndex = 13;
            this.popupBox.TabStop = false;
            this.popupBox.Text = "Show Popup";
            // 
            // showWhenNotPlayingCheckbox
            // 
            this.showWhenNotPlayingCheckbox.AutoSize = true;
            this.showWhenNotPlayingCheckbox.Location = new System.Drawing.Point(16, 163);
            this.showWhenNotPlayingCheckbox.Name = "showWhenNotPlayingCheckbox";
            this.showWhenNotPlayingCheckbox.Size = new System.Drawing.Size(136, 17);
            this.showWhenNotPlayingCheckbox.TabIndex = 21;
            this.showWhenNotPlayingCheckbox.Tag = "showControl/showWhenNotPlaying";
            this.showWhenNotPlayingCheckbox.Text = "Show when not playing";
            this.showWhenNotPlayingCheckbox.UseVisualStyleBackColor = true;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(120, 137);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(47, 13);
            this.label11.TabIndex = 20;
            this.label11.Text = "seconds";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(16, 137);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(28, 13);
            this.label12.TabIndex = 19;
            this.label12.Text = "Stay";
            // 
            // beforeSongEndsStayTextBox
            // 
            this.beforeSongEndsStayTextBox.Location = new System.Drawing.Point(50, 134);
            this.beforeSongEndsStayTextBox.Name = "beforeSongEndsStayTextBox";
            this.beforeSongEndsStayTextBox.Size = new System.Drawing.Size(65, 20);
            this.beforeSongEndsStayTextBox.TabIndex = 18;
            this.beforeSongEndsStayTextBox.Tag = "showControl/beforeSongEndsStay";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(120, 88);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(47, 13);
            this.label10.TabIndex = 17;
            this.label10.Text = "seconds";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(16, 88);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(28, 13);
            this.label9.TabIndex = 16;
            this.label9.Text = "Stay";
            // 
            // onSongStartStayTextBox
            // 
            this.onSongStartStayTextBox.Location = new System.Drawing.Point(50, 85);
            this.onSongStartStayTextBox.Name = "onSongStartStayTextBox";
            this.onSongStartStayTextBox.Size = new System.Drawing.Size(65, 20);
            this.onSongStartStayTextBox.TabIndex = 15;
            this.onSongStartStayTextBox.Tag = "showControl/onSongStartStay";
            // 
            // beforeSongEndsCheckbox
            // 
            this.beforeSongEndsCheckbox.AutoSize = true;
            this.beforeSongEndsCheckbox.Location = new System.Drawing.Point(16, 114);
            this.beforeSongEndsCheckbox.Name = "beforeSongEndsCheckbox";
            this.beforeSongEndsCheckbox.Size = new System.Drawing.Size(109, 17);
            this.beforeSongEndsCheckbox.TabIndex = 14;
            this.beforeSongEndsCheckbox.Tag = "showControl/beforeSongEnds";
            this.beforeSongEndsCheckbox.Text = "Before song ends";
            this.beforeSongEndsCheckbox.UseVisualStyleBackColor = true;
            // 
            // onSongStartCheckbox
            // 
            this.onSongStartCheckbox.AutoSize = true;
            this.onSongStartCheckbox.Location = new System.Drawing.Point(16, 65);
            this.onSongStartCheckbox.Name = "onSongStartCheckbox";
            this.onSongStartCheckbox.Size = new System.Drawing.Size(182, 17);
            this.onSongStartCheckbox.TabIndex = 3;
            this.onSongStartCheckbox.Tag = "showControl/onSongStart";
            this.onSongStartCheckbox.Text = "On song start / track title change";
            this.onSongStartCheckbox.UseVisualStyleBackColor = true;
            // 
            // onlyWhenRadio
            // 
            this.onlyWhenRadio.AutoSize = true;
            this.onlyWhenRadio.Location = new System.Drawing.Point(6, 42);
            this.onlyWhenRadio.Name = "onlyWhenRadio";
            this.onlyWhenRadio.Size = new System.Drawing.Size(75, 17);
            this.onlyWhenRadio.TabIndex = 1;
            this.onlyWhenRadio.TabStop = true;
            this.onlyWhenRadio.Text = "Only when";
            this.onlyWhenRadio.UseVisualStyleBackColor = true;
            // 
            // allTheTimeRadio
            // 
            this.allTheTimeRadio.AutoSize = true;
            this.allTheTimeRadio.Checked = true;
            this.allTheTimeRadio.Location = new System.Drawing.Point(6, 19);
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
            this.tabPage1.Controls.Add(this.groupBox3);
            this.tabPage1.Controls.Add(this.groupBox2);
            this.tabPage1.Controls.Add(this.zOrderBox);
            this.tabPage1.Controls.Add(this.versionLabel);
            this.tabPage1.Controls.Add(this.popupBox);
            this.tabPage1.Controls.Add(this.showWhenBox);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(469, 431);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Appearance";
            this.tabPage1.UseVisualStyleBackColor = true;
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
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.updateIntervalTrackBar);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Location = new System.Drawing.Point(238, 6);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(225, 84);
            this.groupBox2.TabIndex = 15;
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
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(82, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Update interval:";
            // 
            // tabPage2
            // 
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
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.enableDraggingNeverRadio);
            this.groupBox5.Controls.Add(this.enableDraggingPropsOpenRadio);
            this.groupBox5.Controls.Add(this.enableDraggingAlwaysRadio);
            this.groupBox5.Location = new System.Drawing.Point(238, 290);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(225, 90);
            this.groupBox5.TabIndex = 13;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Enable Dragging";
            // 
            // enableDraggingPropsOpenRadio
            // 
            this.enableDraggingPropsOpenRadio.AutoSize = true;
            this.enableDraggingPropsOpenRadio.Location = new System.Drawing.Point(6, 42);
            this.enableDraggingPropsOpenRadio.Name = "enableDraggingPropsOpenRadio";
            this.enableDraggingPropsOpenRadio.Size = new System.Drawing.Size(198, 17);
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
            this.groupBox4.Controls.Add(this.label2);
            this.groupBox4.Controls.Add(this.edgeSnapCheckBox);
            this.groupBox4.Controls.Add(this.posYnumbox);
            this.groupBox4.Controls.Add(this.label4);
            this.groupBox4.Controls.Add(this.posXnumbox);
            this.groupBox4.Controls.Add(this.label8);
            this.groupBox4.Location = new System.Drawing.Point(238, 180);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(225, 104);
            this.groupBox4.TabIndex = 12;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Position";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.SystemColors.GrayText;
            this.label2.Location = new System.Drawing.Point(6, 71);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(195, 26);
            this.label2.TabIndex = 13;
            this.label2.Text = "If edge snapping is enabled you can hold\r\nCTRL to disable it while dragging.";
            // 
            // edgeSnapCheckBox
            // 
            this.edgeSnapCheckBox.AutoSize = true;
            this.edgeSnapCheckBox.Location = new System.Drawing.Point(9, 51);
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
            ((System.ComponentModel.ISupportInitialize)(this.updateIntervalTrackBar)).EndInit();
            this.showWhenBox.ResumeLayout(false);
            this.showWhenBox.PerformLayout();
            this.opacityOpts.ResumeLayout(false);
            this.opacityOpts.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.fadeLengthTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.normalOpacityTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.overOpacityTrackBar)).EndInit();
            this.zOrderBox.ResumeLayout(false);
            this.zOrderBox.PerformLayout();
            this.popupBox.ResumeLayout(false);
            this.popupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.beforeSongEndsStayTextBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.onSongStartStayTextBox)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.tabPage2.ResumeLayout(false);
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
