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
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;

namespace fooTitle {
   class Properties : fooManagedWrapper.CManagedPrefPage{
//    class Properties : System.Windows.Forms.Form {

        public Properties(Main _main)
            : base(new Guid(1414, 548, 7868, 98, 46, 78, 12, 35, 14, 47, 68), fooManagedWrapper.CManagedPrefPage.guid_display) {
           main = _main;
            InitializeComponent();
        }

        protected void fillSkinList(string baseDir) {
            skinsList.Items.Clear();
            foreach (string name in System.IO.Directory.GetDirectories(baseDir)) {
                DirectoryInfo di = new DirectoryInfo(name);
                skinsList.Items.Add(di.Name); // can't use just name, because it contains the baseDir as a prefix
            }
        }

        public void UpdateValues() {
            fillSkinList(Main.DataDir);
            updateIntervalTrackBar.Value = main.UpdateInterval;
            albumArtFilenames.Text = main.AlbumArtFilenames;
            if (main.ShowWhen == ShowWhenEnum.Always)
                alwaysRadio.Checked = true;
            else if (main.ShowWhen == ShowWhenEnum.Never)
                neverRadio.Checked = true;
            else if (main.ShowWhen == ShowWhenEnum.WhenMinimized)
                minimizedRadio.Checked = true;
            normalOpacityTrackBar.Value = main.NormalOpacity;
            overOpacityTrackBar.Value = main.OverOpacity;
            fadeLengthTrackBar.Value = main.FadeLength;
            fadeLengthLabel.Text = main.FadeLength.ToString() + " ms";
            Assembly myAssembly = Assembly.GetExecutingAssembly();
            versionLabel.Text = "Version: " + myAssembly.GetName().Version.ToString();

            if (main.WindowPosition == Win32.WindowPosition.Bottom)
                onDesktopRadio.Checked = true;
            else if (main.WindowPosition == Win32.WindowPosition.NoTopmost)
                normalRadio.Checked = true;
            else if (main.WindowPosition == Win32.WindowPosition.Topmost)
                alwaysOnTopRadio.Checked = true;
        }

        #region Windows Form Designer generated code

        private System.Windows.Forms.ListBox skinsList;
        private System.Windows.Forms.Button applySkinBtn;
        private System.Windows.Forms.TrackBar updateIntervalTrackBar;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox albumArtFilenames;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox albumArtGroup;
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

        protected Main main;

        private void InitializeComponent() {
            this.skinsList = new System.Windows.Forms.ListBox();
            this.applySkinBtn = new System.Windows.Forms.Button();
            this.updateIntervalTrackBar = new System.Windows.Forms.TrackBar();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.albumArtFilenames = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.albumArtGroup = new System.Windows.Forms.GroupBox();
            this.showWhenBox = new System.Windows.Forms.GroupBox();
            this.neverRadio = new System.Windows.Forms.RadioButton();
            this.minimizedRadio = new System.Windows.Forms.RadioButton();
            this.alwaysRadio = new System.Windows.Forms.RadioButton();
            this.opacityOpts = new System.Windows.Forms.GroupBox();
            this.fadeLengthLabel = new System.Windows.Forms.Label();
            this.fadeLengthTrackBar = new System.Windows.Forms.TrackBar();
            this.label7 = new System.Windows.Forms.Label();
            this.normalOpacityTrackBar = new System.Windows.Forms.TrackBar();
            this.overOpacityTrackBar = new System.Windows.Forms.TrackBar();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.versionLabel = new System.Windows.Forms.Label();
            this.zOrderBox = new System.Windows.Forms.GroupBox();
            this.onDesktopRadio = new System.Windows.Forms.RadioButton();
            this.normalRadio = new System.Windows.Forms.RadioButton();
            this.alwaysOnTopRadio = new System.Windows.Forms.RadioButton();
            ((System.ComponentModel.ISupportInitialize)(this.updateIntervalTrackBar)).BeginInit();
            this.albumArtGroup.SuspendLayout();
            this.showWhenBox.SuspendLayout();
            this.opacityOpts.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.fadeLengthTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.normalOpacityTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.overOpacityTrackBar)).BeginInit();
            this.zOrderBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // skinsList
            // 
            this.skinsList.FormattingEnabled = true;
            this.skinsList.Location = new System.Drawing.Point(12, 26);
            this.skinsList.Name = "skinsList";
            this.skinsList.Size = new System.Drawing.Size(152, 160);
            this.skinsList.TabIndex = 0;
            // 
            // applySkinBtn
            // 
            this.applySkinBtn.Location = new System.Drawing.Point(12, 191);
            this.applySkinBtn.Name = "applySkinBtn";
            this.applySkinBtn.Size = new System.Drawing.Size(75, 23);
            this.applySkinBtn.TabIndex = 1;
            this.applySkinBtn.Text = "Apply skin";
            this.applySkinBtn.UseVisualStyleBackColor = true;
            this.applySkinBtn.Click += new System.EventHandler(this.applySkinBtn_Click);
            // 
            // updateIntervalTrackBar
            // 
            this.updateIntervalTrackBar.LargeChange = 100;
            this.updateIntervalTrackBar.Location = new System.Drawing.Point(170, 25);
            this.updateIntervalTrackBar.Maximum = 500;
            this.updateIntervalTrackBar.Minimum = 50;
            this.updateIntervalTrackBar.Name = "updateIntervalTrackBar";
            this.updateIntervalTrackBar.Size = new System.Drawing.Size(152, 40);
            this.updateIntervalTrackBar.SmallChange = 10;
            this.updateIntervalTrackBar.TabIndex = 2;
            this.updateIntervalTrackBar.TickFrequency = 50;
            this.updateIntervalTrackBar.Value = 50;
            this.updateIntervalTrackBar.ValueChanged += new System.EventHandler(this.trackBar1_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(167, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(82, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Update interval:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(76, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Installed skins:";
            // 
            // albumArtFilenames
            // 
            this.albumArtFilenames.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.albumArtFilenames.Location = new System.Drawing.Point(6, 32);
            this.albumArtFilenames.Name = "albumArtFilenames";
            this.albumArtFilenames.Size = new System.Drawing.Size(231, 20);
            this.albumArtFilenames.TabIndex = 5;
            this.albumArtFilenames.TextChanged += new System.EventHandler(this.albumArtFilenames_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 16);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(101, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Album art filenames:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 55);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(230, 39);
            this.label4.TabIndex = 7;
            this.label4.Text = "Enter the names of possible album art images \r\nwithout extensions, separated by s" +
                "emicolons.\r\nIt\'s possible to use foobar title formatting strings.";
            // 
            // albumArtGroup
            // 
            this.albumArtGroup.Controls.Add(this.albumArtFilenames);
            this.albumArtGroup.Controls.Add(this.label4);
            this.albumArtGroup.Controls.Add(this.label3);
            this.albumArtGroup.Location = new System.Drawing.Point(170, 71);
            this.albumArtGroup.Name = "albumArtGroup";
            this.albumArtGroup.Size = new System.Drawing.Size(243, 100);
            this.albumArtGroup.TabIndex = 8;
            this.albumArtGroup.TabStop = false;
            this.albumArtGroup.Text = "Album art";
            // 
            // showWhenBox
            // 
            this.showWhenBox.Controls.Add(this.neverRadio);
            this.showWhenBox.Controls.Add(this.minimizedRadio);
            this.showWhenBox.Controls.Add(this.alwaysRadio);
            this.showWhenBox.Location = new System.Drawing.Point(170, 177);
            this.showWhenBox.Name = "showWhenBox";
            this.showWhenBox.Size = new System.Drawing.Size(243, 86);
            this.showWhenBox.TabIndex = 9;
            this.showWhenBox.TabStop = false;
            this.showWhenBox.Text = "Show foo_title when:";
            // 
            // neverRadio
            // 
            this.neverRadio.AutoSize = true;
            this.neverRadio.Location = new System.Drawing.Point(4, 57);
            this.neverRadio.Name = "neverRadio";
            this.neverRadio.Size = new System.Drawing.Size(54, 17);
            this.neverRadio.TabIndex = 2;
            this.neverRadio.TabStop = true;
            this.neverRadio.Text = "Never";
            this.neverRadio.UseVisualStyleBackColor = true;
            this.neverRadio.CheckedChanged += new System.EventHandler(this.neverRadio_CheckedChanged);
            // 
            // minimizedRadio
            // 
            this.minimizedRadio.AutoSize = true;
            this.minimizedRadio.Location = new System.Drawing.Point(4, 34);
            this.minimizedRadio.Name = "minimizedRadio";
            this.minimizedRadio.Size = new System.Drawing.Size(169, 17);
            this.minimizedRadio.TabIndex = 1;
            this.minimizedRadio.TabStop = true;
            this.minimizedRadio.Text = "When foobar2000 is minimized";
            this.minimizedRadio.UseVisualStyleBackColor = true;
            this.minimizedRadio.CheckedChanged += new System.EventHandler(this.minimizedRadio_CheckedChanged);
            // 
            // alwaysRadio
            // 
            this.alwaysRadio.AutoSize = true;
            this.alwaysRadio.Location = new System.Drawing.Point(4, 11);
            this.alwaysRadio.Name = "alwaysRadio";
            this.alwaysRadio.Size = new System.Drawing.Size(58, 17);
            this.alwaysRadio.TabIndex = 0;
            this.alwaysRadio.TabStop = true;
            this.alwaysRadio.Text = "Always";
            this.alwaysRadio.UseVisualStyleBackColor = true;
            this.alwaysRadio.CheckedChanged += new System.EventHandler(this.alwaysRadio_CheckedChanged);
            // 
            // opacityOpts
            // 
            this.opacityOpts.Controls.Add(this.fadeLengthLabel);
            this.opacityOpts.Controls.Add(this.fadeLengthTrackBar);
            this.opacityOpts.Controls.Add(this.label7);
            this.opacityOpts.Controls.Add(this.normalOpacityTrackBar);
            this.opacityOpts.Controls.Add(this.overOpacityTrackBar);
            this.opacityOpts.Controls.Add(this.label6);
            this.opacityOpts.Controls.Add(this.label5);
            this.opacityOpts.Location = new System.Drawing.Point(170, 269);
            this.opacityOpts.Name = "opacityOpts";
            this.opacityOpts.Size = new System.Drawing.Size(243, 153);
            this.opacityOpts.TabIndex = 10;
            this.opacityOpts.TabStop = false;
            this.opacityOpts.Text = "Opacity";
            // 
            // fadeLengthLabel
            // 
            this.fadeLengthLabel.AutoSize = true;
            this.fadeLengthLabel.Location = new System.Drawing.Point(179, 133);
            this.fadeLengthLabel.Name = "fadeLengthLabel";
            this.fadeLengthLabel.Size = new System.Drawing.Size(35, 13);
            this.fadeLengthLabel.TabIndex = 5;
            this.fadeLengthLabel.Text = "label8";
            // 
            // fadeLengthTrackBar
            // 
            this.fadeLengthTrackBar.LargeChange = 100;
            this.fadeLengthTrackBar.Location = new System.Drawing.Point(182, 20);
            this.fadeLengthTrackBar.Maximum = 2000;
            this.fadeLengthTrackBar.Name = "fadeLengthTrackBar";
            this.fadeLengthTrackBar.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.fadeLengthTrackBar.Size = new System.Drawing.Size(40, 110);
            this.fadeLengthTrackBar.SmallChange = 10;
            this.fadeLengthTrackBar.TabIndex = 3;
            this.fadeLengthTrackBar.TickFrequency = 100;
            this.fadeLengthTrackBar.ValueChanged += new System.EventHandler(this.fadeLengthTrackBar_ValueChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(137, 20);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(39, 26);
            this.label7.TabIndex = 4;
            this.label7.Text = "Fade \r\nlength:";
            // 
            // normalOpacityTrackBar
            // 
            this.normalOpacityTrackBar.Location = new System.Drawing.Point(6, 36);
            this.normalOpacityTrackBar.Maximum = 255;
            this.normalOpacityTrackBar.Minimum = 5;
            this.normalOpacityTrackBar.Name = "normalOpacityTrackBar";
            this.normalOpacityTrackBar.Size = new System.Drawing.Size(104, 40);
            this.normalOpacityTrackBar.TabIndex = 3;
            this.normalOpacityTrackBar.TickFrequency = 16;
            this.normalOpacityTrackBar.Value = 5;
            this.normalOpacityTrackBar.ValueChanged += new System.EventHandler(this.normalOpacityTrackBar_ValueChanged);
            // 
            // overOpacityTrackBar
            // 
            this.overOpacityTrackBar.Location = new System.Drawing.Point(6, 90);
            this.overOpacityTrackBar.Maximum = 255;
            this.overOpacityTrackBar.Minimum = 5;
            this.overOpacityTrackBar.Name = "overOpacityTrackBar";
            this.overOpacityTrackBar.Size = new System.Drawing.Size(104, 40);
            this.overOpacityTrackBar.TabIndex = 2;
            this.overOpacityTrackBar.TickFrequency = 16;
            this.overOpacityTrackBar.Value = 5;
            this.overOpacityTrackBar.ValueChanged += new System.EventHandler(this.overOpacityTrackBar_ValueChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(9, 74);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(119, 13);
            this.label6.TabIndex = 1;
            this.label6.Text = "Opacity on mouse over:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(9, 20);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(80, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "Normal opacity:";
            // 
            // versionLabel
            // 
            this.versionLabel.AutoSize = true;
            this.versionLabel.Location = new System.Drawing.Point(339, 15);
            this.versionLabel.Name = "versionLabel";
            this.versionLabel.Size = new System.Drawing.Size(35, 13);
            this.versionLabel.TabIndex = 11;
            this.versionLabel.Text = "label8";
            // 
            // zOrderBox
            // 
            this.zOrderBox.Controls.Add(this.onDesktopRadio);
            this.zOrderBox.Controls.Add(this.normalRadio);
            this.zOrderBox.Controls.Add(this.alwaysOnTopRadio);
            this.zOrderBox.Location = new System.Drawing.Point(15, 220);
            this.zOrderBox.Name = "zOrderBox";
            this.zOrderBox.Size = new System.Drawing.Size(149, 95);
            this.zOrderBox.TabIndex = 12;
            this.zOrderBox.TabStop = false;
            this.zOrderBox.Text = "Z-order";
            // 
            // onDesktopRadio
            // 
            this.onDesktopRadio.AutoSize = true;
            this.onDesktopRadio.Location = new System.Drawing.Point(6, 65);
            this.onDesktopRadio.Name = "onDesktopRadio";
            this.onDesktopRadio.Size = new System.Drawing.Size(80, 17);
            this.onDesktopRadio.TabIndex = 2;
            this.onDesktopRadio.TabStop = true;
            this.onDesktopRadio.Text = "On desktop";
            this.onDesktopRadio.UseVisualStyleBackColor = true;
            this.onDesktopRadio.CheckedChanged += new System.EventHandler(this.onDesktopRadio_CheckedChanged);
            // 
            // normalRadio
            // 
            this.normalRadio.AutoSize = true;
            this.normalRadio.Location = new System.Drawing.Point(6, 42);
            this.normalRadio.Name = "normalRadio";
            this.normalRadio.Size = new System.Drawing.Size(58, 17);
            this.normalRadio.TabIndex = 1;
            this.normalRadio.TabStop = true;
            this.normalRadio.Text = "Normal";
            this.normalRadio.UseVisualStyleBackColor = true;
            this.normalRadio.CheckedChanged += new System.EventHandler(this.normalRadio_CheckedChanged);
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
            this.alwaysOnTopRadio.CheckedChanged += new System.EventHandler(this.alwaysOnTopRadio_CheckedChanged);
            // 
            // Properties
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(469, 444);
            this.Controls.Add(this.zOrderBox);
            this.Controls.Add(this.versionLabel);
            this.Controls.Add(this.opacityOpts);
            this.Controls.Add(this.showWhenBox);
            this.Controls.Add(this.albumArtGroup);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.updateIntervalTrackBar);
            this.Controls.Add(this.applySkinBtn);
            this.Controls.Add(this.skinsList);
            this.Name = "Properties";
            this.Text = "foo_title";
            this.VisibleChanged += new System.EventHandler(this.Properties_VisibleChanged);
            ((System.ComponentModel.ISupportInitialize)(this.updateIntervalTrackBar)).EndInit();
            this.albumArtGroup.ResumeLayout(false);
            this.albumArtGroup.PerformLayout();
            this.showWhenBox.ResumeLayout(false);
            this.showWhenBox.PerformLayout();
            this.opacityOpts.ResumeLayout(false);
            this.opacityOpts.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.fadeLengthTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.normalOpacityTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.overOpacityTrackBar)).EndInit();
            this.zOrderBox.ResumeLayout(false);
            this.zOrderBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        void onDesktopRadio_CheckedChanged(object sender, EventArgs e) {
            if (onDesktopRadio.Checked)
                main.WindowPosition = Win32.WindowPosition.Bottom;
        }

        void normalRadio_CheckedChanged(object sender, EventArgs e) {
            if (normalRadio.Checked)
                main.WindowPosition = Win32.WindowPosition.NoTopmost;
        }

        void alwaysOnTopRadio_CheckedChanged(object sender, EventArgs e) {
            if (alwaysOnTopRadio.Checked)
                main.WindowPosition = Win32.WindowPosition.Topmost;
        }


        void Properties_VisibleChanged(object sender, EventArgs e) {
            if (Visible)
                UpdateValues();
        }

        private void applySkinBtn_Click(object sender, EventArgs e) {
            main.SkinName = (string)skinsList.SelectedItem;
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e) {
            main.UpdateInterval = updateIntervalTrackBar.Value;
        }

        private void albumArtFilenames_TextChanged(object sender, EventArgs e) {
            main.AlbumArtFilenames = albumArtFilenames.Text;
        }

        private void alwaysRadio_CheckedChanged(object sender, EventArgs e) {
            if (alwaysRadio.Checked)
                Main.GetInstance().ShowWhen = ShowWhenEnum.Always;
        }

        void neverRadio_CheckedChanged(object sender, EventArgs e) {
            if (neverRadio.Checked)
                Main.GetInstance().ShowWhen = ShowWhenEnum.Never;
        }

        void minimizedRadio_CheckedChanged(object sender, EventArgs e) {
            if (minimizedRadio.Checked)
                Main.GetInstance().ShowWhen = ShowWhenEnum.WhenMinimized;
        }




        private void normalOpacityTrackBar_ValueChanged(object sender, EventArgs e) {
            main.NormalOpacity = normalOpacityTrackBar.Value;
        }

        private void overOpacityTrackBar_ValueChanged(object sender, EventArgs e) {
            main.OverOpacity = overOpacityTrackBar.Value;
        }

        private void fadeLengthTrackBar_ValueChanged(object sender, EventArgs e) {
            main.FadeLength = fadeLengthTrackBar.Value;
            fadeLengthLabel.Text = fadeLengthTrackBar.Value.ToString() + " ms";
        }
    }
}
