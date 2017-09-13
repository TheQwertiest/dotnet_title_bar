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
using System.Drawing;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using fooTitle.Config;
using fooTitle.CustomControl;
using fooTitle.Layers;

namespace fooTitle {
   class Properties : fooManagedWrapper.CManagedPrefPage_v3 {
     //   class Properties : Form {

        public override void Reset()
        {
            ConfValuesManager.GetInstance().Reset();
        }

        public override void Apply()
        {
            ConfValuesManager.GetInstance().SaveTo(Main.GetInstance().Config);
        }

        public override bool HasChanged()
        {
            return ConfValuesManager.GetInstance().HasChanged();
        }

        private class SkinListEntry : ListViewItem
        {
            public readonly string Path;

            public SkinListEntry(string path, string name, string author) : base(new[] { name, author })
            {
                Path = path;
            }
        }

        private readonly AutoWrapperCreator autoWrapperCreator = new AutoWrapperCreator();
        private RadioGroupWrapper showWhenWrapper;
        private RadioGroupWrapper windowPositionWrapper;
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
        private RadioGroupWrapper popupShowingWrapper;
        private RadioButton enableDraggingNeverRadio;
        private GroupBox groupBox2;
        private Label label3;
        private TrackBar updateIntervalTrackBar;
        private Label label1;
        private CheckBox checkBox4;
        private TabPage tabPage1;
        private TableLayoutPanel tabLayout1;
        private HorizontalFillDockGroupBox zOrderBox;
        private CheckBox restoreTopmostCheckbox;
        private RadioButton onDesktopRadio;
        private RadioButton normalRadio;
        private RadioButton alwaysOnTopRadio;
        private HorizontalFillDockGroupBox installedSkinsBox;
        private HorizontalFillDockGroupBox enableWhenBox;
        private RadioButton neverRadio;
        private RadioButton minimizedRadio;
        private RadioButton alwaysRadio;
        private HorizontalFillDockGroupBox triggersBox;
        private Label label9;
        private Label label10;
        private NumericUpDown songEndNum;
        private Label label11;
        private Label label12;
        private NumericUpDown songStartNum;
        private CheckBox songEndCheckBox;
        private CheckBox songStartCheckBox;
        private HorizontalFillDockGroupBox popupPeekBox;
        private Label label21;
        private Label label22;
        private NumericUpDown beforeFadeNum;
        private Label label23;
        private HorizontalFillDockGroupBox showWhenBox;
        private CheckBox notPlayingCheckBox;
        private RadioButton onlyTriggerRadio;
        private RadioButton allTheTimeRadio;
        private Label versionLabel;
        private TableLayoutPanel installedSkinsLayout;
        private Button openSkinDirBtn;
        private Button applySkinBtn;
        private HorizontalFillDockListView skinsList;
        private ColumnHeader nameColumn;
        private ColumnHeader authorColumn;
        private RadioGroupWrapper enableDraggingWrapper;

        public Properties(Main main) : base(new Guid(1414, 548, 7868, 98, 46, 78, 12, 35, 14, 47, 68), fooManagedWrapper.CManagedPrefPage_v3.guid_display)
        {
            _main = main;
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
            popupShowingWrapper.AddRadioButton(onlyTriggerRadio);

            enableDraggingWrapper = new RadioGroupWrapper("display/enableDragging", this);
            enableDraggingWrapper.AddRadioButton(enableDraggingAlwaysRadio);
            enableDraggingWrapper.AddRadioButton(enableDraggingPropsOpenRadio);
            enableDraggingWrapper.AddRadioButton(enableDraggingNeverRadio);

            autoWrapperCreator.CreateWrappers(this);
            //this.Font = new Font(this.Font.FontFamily, this.Font.Size * 120 / 96);
        }        

        #region SkinList_Handling

        private CancellationTokenSource _cts;

        private async Task<List<ListViewItem>> GetSkinItems(CancellationToken token)
        {
            try
            {
                List<ListViewItem> itemsLocal = new List<ListViewItem>();
                var dirList = System.IO.Directory.GetDirectories(Main.UserDataDir);
                int i = 0;
                foreach (string path in dirList)
                {
                    token.ThrowIfCancellationRequested();

                    Skin.SkinInfo skinInfo = Skin.GetSkinInfo(path);
                    if (skinInfo?.Name != null)
                    {
                        var current = new SkinListEntry(path, skinInfo.Name, skinInfo.Author);
                        if (path == _main.SkinPath.Value)
                        {
                            current.Selected = true;
                            current.EnsureVisible();
                        }
                        itemsLocal.Add(current);
                        await Task.Delay(1, token);
                    }

                    ++i;
                    _skinListProgressTimer.Progress = (float)(i * 100) / dirList.Length;
                }

                return itemsLocal;
            }
            catch (Exception e)
            {
                fooManagedWrapper.CConsole.Write($"Failed to read from {Main.UserDataDir}:\n{e}");
                return new List<ListViewItem>();
            }
        }

        private ProgressTimer _skinListProgressTimer;

        private void UpdateSkinListProgress(object sender, float progress)
        {
            skinsList.Items[0].SubItems[0].Text = $"Loading skins... ({(int)progress}%)";
        }

        private void StartSkinListFill()
        {
            skinsList.Items.Clear();
            skinsList.Items.Add(new SkinListEntry("", "", ""));
            UpdateSkinListProgress(null, 0);
            ResizeListView(skinsList);
        }

        private void CompleteSkinListFill(ListViewItem[] items)
        {
            skinsList.Items.Clear();
            if (items.Length == 0)
            {
                return;
            }

            skinsList.BeginUpdate();
            skinsList.Items.AddRange(items);
            if (skinsList.SelectedIndices.Count != 0)
            {
                skinsList.EnsureVisible(skinsList.SelectedIndices[0]);
            }
            ResizeListView(skinsList);
            skinsList.EndUpdate();
        }

        private async Task FillSkinListAsync(CancellationToken token)
        {
            StartSkinListFill();

            _skinListProgressTimer = new ProgressTimer(UpdateSkinListProgress);
            _skinListProgressTimer.Start();
            List<ListViewItem> items = await GetSkinItems(token);
            _skinListProgressTimer.Stop();

            CompleteSkinListFill(items.ToArray());
        }

        #endregion //SkinList

        public void DiscardChanges() {
            ConfValuesManager.GetInstance().LoadFrom(Main.GetInstance().Config);
        }

        #region Windows Form Designer generated code
        private SafeTabControl tabControl1;
        private TabPage tabPage2;
        private GroupBox opacityOpts;
        private TrackBar normalOpacityTrackBar;
        private TrackBar overOpacityTrackBar;
        private Label label6;
        private Label label5;

        private Main _main;

        private void InitializeComponent() {
            this.opacityOpts = new System.Windows.Forms.GroupBox();
            this.opacityNormalLabel = new System.Windows.Forms.Label();
            this.opacityMouseOverLabel = new System.Windows.Forms.Label();
            this.normalOpacityTrackBar = new System.Windows.Forms.TrackBar();
            this.overOpacityTrackBar = new System.Windows.Forms.TrackBar();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.tabControl1 = new fooTitle.SafeTabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabLayout1 = new System.Windows.Forms.TableLayoutPanel();
            this.zOrderBox = new fooTitle.CustomControl.HorizontalFillDockGroupBox();
            this.restoreTopmostCheckbox = new System.Windows.Forms.CheckBox();
            this.onDesktopRadio = new System.Windows.Forms.RadioButton();
            this.normalRadio = new System.Windows.Forms.RadioButton();
            this.alwaysOnTopRadio = new System.Windows.Forms.RadioButton();
            this.installedSkinsBox = new fooTitle.CustomControl.HorizontalFillDockGroupBox();
            this.installedSkinsLayout = new System.Windows.Forms.TableLayoutPanel();
            this.openSkinDirBtn = new System.Windows.Forms.Button();
            this.applySkinBtn = new System.Windows.Forms.Button();
            this.skinsList = new fooTitle.CustomControl.HorizontalFillDockListView();
            this.nameColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.authorColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.enableWhenBox = new fooTitle.CustomControl.HorizontalFillDockGroupBox();
            this.neverRadio = new System.Windows.Forms.RadioButton();
            this.minimizedRadio = new System.Windows.Forms.RadioButton();
            this.alwaysRadio = new System.Windows.Forms.RadioButton();
            this.triggersBox = new fooTitle.CustomControl.HorizontalFillDockGroupBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.songEndNum = new System.Windows.Forms.NumericUpDown();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.songStartNum = new System.Windows.Forms.NumericUpDown();
            this.songEndCheckBox = new System.Windows.Forms.CheckBox();
            this.songStartCheckBox = new System.Windows.Forms.CheckBox();
            this.popupPeekBox = new fooTitle.CustomControl.HorizontalFillDockGroupBox();
            this.label21 = new System.Windows.Forms.Label();
            this.label22 = new System.Windows.Forms.Label();
            this.beforeFadeNum = new System.Windows.Forms.NumericUpDown();
            this.label23 = new System.Windows.Forms.Label();
            this.showWhenBox = new fooTitle.CustomControl.HorizontalFillDockGroupBox();
            this.notPlayingCheckBox = new System.Windows.Forms.CheckBox();
            this.onlyTriggerRadio = new System.Windows.Forms.RadioButton();
            this.allTheTimeRadio = new System.Windows.Forms.RadioButton();
            this.versionLabel = new System.Windows.Forms.Label();
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
            this.checkBox4 = new System.Windows.Forms.CheckBox();
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
            this.opacityOpts.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.normalOpacityTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.overOpacityTrackBar)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabLayout1.SuspendLayout();
            this.zOrderBox.SuspendLayout();
            this.installedSkinsBox.SuspendLayout();
            this.installedSkinsLayout.SuspendLayout();
            this.enableWhenBox.SuspendLayout();
            this.triggersBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.songEndNum)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.songStartNum)).BeginInit();
            this.popupPeekBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.beforeFadeNum)).BeginInit();
            this.showWhenBox.SuspendLayout();
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
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(501, 501);
            this.tabControl1.TabIndex = 14;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.tabLayout1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(493, 475);
            this.tabPage1.TabIndex = 1;
            this.tabPage1.Text = "Appearance";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            this.tabLayout1.ColumnCount = 2;
            this.tabLayout1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tabLayout1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tabLayout1.Controls.Add(this.zOrderBox, 0, 5);
            this.tabLayout1.Controls.Add(this.installedSkinsBox, 0, 0);
            this.tabLayout1.Controls.Add(this.enableWhenBox, 0, 4);
            this.tabLayout1.Controls.Add(this.triggersBox, 1, 1);
            this.tabLayout1.Controls.Add(this.popupPeekBox, 1, 2);
            this.tabLayout1.Controls.Add(this.showWhenBox, 1, 0);
            this.tabLayout1.Controls.Add(this.versionLabel, 0, 6);
            this.tabLayout1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabLayout1.Location = new System.Drawing.Point(3, 3);
            this.tabLayout1.Name = "tabLayout1";
            this.tabLayout1.RowCount = 7;
            this.tabLayout1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tabLayout1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tabLayout1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tabLayout1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tabLayout1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tabLayout1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tabLayout1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tabLayout1.Size = new System.Drawing.Size(487, 469);
            this.tabLayout1.TabIndex = 0;
            // 
            // zOrderBox
            // 
            this.zOrderBox.Controls.Add(this.restoreTopmostCheckbox);
            this.zOrderBox.Controls.Add(this.onDesktopRadio);
            this.zOrderBox.Controls.Add(this.normalRadio);
            this.zOrderBox.Controls.Add(this.alwaysOnTopRadio);
            this.zOrderBox.Location = new System.Drawing.Point(246, 287);
            this.zOrderBox.Name = "zOrderBox";
            this.tabLayout1.SetRowSpan(this.zOrderBox, 2);
            this.zOrderBox.Size = new System.Drawing.Size(238, 125);
            this.zOrderBox.TabIndex = 25;
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
            // installedSkinsBox
            // 
            this.installedSkinsBox.Controls.Add(this.installedSkinsLayout);
            this.installedSkinsBox.Location = new System.Drawing.Point(3, 3);
            this.installedSkinsBox.Name = "installedSkinsBoxs";
            this.tabLayout1.SetRowSpan(this.installedSkinsBox, 3);
            this.installedSkinsBox.Size = new System.Drawing.Size(237, 278);
            this.installedSkinsBox.TabIndex = 17;
            this.installedSkinsBox.TabStop = false;
            this.installedSkinsBox.Text = "Installed skins";
            // 
            // installedSkinsLayout
            // 
            this.installedSkinsLayout.ColumnCount = 2;
            this.installedSkinsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.installedSkinsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.installedSkinsLayout.Controls.Add(this.openSkinDirBtn, 1, 1);
            this.installedSkinsLayout.Controls.Add(this.applySkinBtn, 0, 1);
            this.installedSkinsLayout.Controls.Add(this.skinsList, 0, 0);
            this.installedSkinsLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.installedSkinsLayout.Location = new System.Drawing.Point(3, 16);
            this.installedSkinsLayout.Name = "installedSkinsLayout";
            this.installedSkinsLayout.RowCount = 2;
            this.installedSkinsLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.installedSkinsLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.installedSkinsLayout.Size = new System.Drawing.Size(231, 259);
            this.installedSkinsLayout.TabIndex = 0;
            // 
            // openSkinDirBtn
            // 
            this.openSkinDirBtn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.openSkinDirBtn.Location = new System.Drawing.Point(118, 234);
            this.openSkinDirBtn.Name = "openSkinDirBtn";
            this.openSkinDirBtn.Size = new System.Drawing.Size(110, 23);
            this.openSkinDirBtn.TabIndex = 3;
            this.openSkinDirBtn.Text = "Open directory";
            this.openSkinDirBtn.UseVisualStyleBackColor = true;
            this.openSkinDirBtn.Click += new System.EventHandler(this.openSkinDirBtn_Click);
            // 
            // applySkinBtn
            // 
            this.applySkinBtn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.applySkinBtn.Location = new System.Drawing.Point(3, 234);
            this.applySkinBtn.Name = "applySkinBtn";
            this.applySkinBtn.Size = new System.Drawing.Size(109, 23);
            this.applySkinBtn.TabIndex = 2;
            this.applySkinBtn.Text = "Apply skin";
            this.applySkinBtn.UseVisualStyleBackColor = true;
            this.applySkinBtn.Click += new System.EventHandler(this.applySkinBtn_Click);
            // 
            // skinsList
            // 
            this.skinsList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.nameColumn,
            this.authorColumn});
            this.installedSkinsLayout.SetColumnSpan(this.skinsList, 2);
            this.skinsList.FullRowSelect = true;
            this.skinsList.HideSelection = false;
            this.skinsList.Location = new System.Drawing.Point(3, 3);
            this.skinsList.MultiSelect = false;
            this.skinsList.Name = "skinsList";
            this.skinsList.Size = new System.Drawing.Size(225, 225);
            this.skinsList.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.skinsList.TabIndex = 1;
            this.skinsList.UseCompatibleStateImageBehavior = false;
            this.skinsList.View = System.Windows.Forms.View.Details;
            this.skinsList.DoubleClick += new System.EventHandler(this.skinsList_DoubleClick);
            this.skinsList.DoubleBuffered(true);
            // 
            // nameColumn
            // 
            this.nameColumn.Text = "Name";
            // 
            // authorColumn
            // 
            this.authorColumn.Text = "Author";
            // 
            // enableWhenBox
            // 
            this.enableWhenBox.Controls.Add(this.neverRadio);
            this.enableWhenBox.Controls.Add(this.minimizedRadio);
            this.enableWhenBox.Controls.Add(this.alwaysRadio);
            this.enableWhenBox.Location = new System.Drawing.Point(3, 287);
            this.enableWhenBox.Name = "enableWhenBox";
            this.tabLayout1.SetRowSpan(this.enableWhenBox, 2);
            this.enableWhenBox.Size = new System.Drawing.Size(237, 89);
            this.enableWhenBox.TabIndex = 18;
            this.enableWhenBox.TabStop = false;
            this.enableWhenBox.Text = "Enabled";
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
            // triggersBox
            // 
            this.triggersBox.Controls.Add(this.label9);
            this.triggersBox.Controls.Add(this.label10);
            this.triggersBox.Controls.Add(this.songEndNum);
            this.triggersBox.Controls.Add(this.label11);
            this.triggersBox.Controls.Add(this.label12);
            this.triggersBox.Controls.Add(this.songStartNum);
            this.triggersBox.Controls.Add(this.songEndCheckBox);
            this.triggersBox.Controls.Add(this.songStartCheckBox);
            this.triggersBox.Location = new System.Drawing.Point(246, 93);
            this.triggersBox.Name = "triggersBox";
            this.triggersBox.Size = new System.Drawing.Size(238, 118);
            this.triggersBox.TabIndex = 23;
            this.triggersBox.TabStop = false;
            this.triggersBox.Text = "Popup Triggers";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(120, 91);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(47, 13);
            this.label9.TabIndex = 20;
            this.label9.Text = "seconds";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(16, 91);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(28, 13);
            this.label10.TabIndex = 19;
            this.label10.Text = "Stay";
            // 
            // songEndNum
            // 
            this.songEndNum.Location = new System.Drawing.Point(50, 88);
            this.songEndNum.Name = "songEndNum";
            this.songEndNum.Size = new System.Drawing.Size(65, 20);
            this.songEndNum.TabIndex = 18;
            this.songEndNum.Tag = "showControl/beforeSongEndsStay";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(120, 42);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(47, 13);
            this.label11.TabIndex = 17;
            this.label11.Text = "seconds";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(16, 42);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(28, 13);
            this.label12.TabIndex = 16;
            this.label12.Text = "Stay";
            // 
            // songStartNum
            // 
            this.songStartNum.Location = new System.Drawing.Point(50, 39);
            this.songStartNum.Name = "songStartNum";
            this.songStartNum.Size = new System.Drawing.Size(65, 20);
            this.songStartNum.TabIndex = 15;
            this.songStartNum.Tag = "showControl/onSongStartStay";
            // 
            // songEndCheckBox
            // 
            this.songEndCheckBox.AutoSize = true;
            this.songEndCheckBox.Location = new System.Drawing.Point(16, 68);
            this.songEndCheckBox.Name = "songEndCheckBox";
            this.songEndCheckBox.Size = new System.Drawing.Size(109, 17);
            this.songEndCheckBox.TabIndex = 14;
            this.songEndCheckBox.Tag = "showControl/beforeSongEnds";
            this.songEndCheckBox.Text = "Before song ends";
            this.songEndCheckBox.UseVisualStyleBackColor = true;
            // 
            // songStartCheckBox
            // 
            this.songStartCheckBox.AutoSize = true;
            this.songStartCheckBox.Location = new System.Drawing.Point(16, 19);
            this.songStartCheckBox.Name = "songStartCheckBox";
            this.songStartCheckBox.Size = new System.Drawing.Size(182, 17);
            this.songStartCheckBox.TabIndex = 3;
            this.songStartCheckBox.Tag = "showControl/onSongStart";
            this.songStartCheckBox.Text = "On song start / track title change";
            this.songStartCheckBox.UseVisualStyleBackColor = true;
            // 
            // popupPeekBox
            // 
            this.popupPeekBox.Controls.Add(this.label21);
            this.popupPeekBox.Controls.Add(this.label22);
            this.popupPeekBox.Controls.Add(this.beforeFadeNum);
            this.popupPeekBox.Controls.Add(this.label23);
            this.popupPeekBox.Location = new System.Drawing.Point(246, 217);
            this.popupPeekBox.Name = "popupPeekBox";
            this.popupPeekBox.Size = new System.Drawing.Size(238, 64);
            this.popupPeekBox.TabIndex = 24;
            this.popupPeekBox.TabStop = false;
            this.popupPeekBox.Text = "Popup Peek";
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(121, 38);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(47, 13);
            this.label21.TabIndex = 21;
            this.label21.Text = "seconds";
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(16, 38);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(28, 13);
            this.label22.TabIndex = 20;
            this.label22.Text = "Stay";
            // 
            // beforeFadeNum
            // 
            this.beforeFadeNum.Location = new System.Drawing.Point(50, 36);
            this.beforeFadeNum.Name = "beforeFadeNum";
            this.beforeFadeNum.Size = new System.Drawing.Size(65, 20);
            this.beforeFadeNum.TabIndex = 19;
            this.beforeFadeNum.Tag = "showControl/timeBeforeFade";
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Location = new System.Drawing.Point(16, 17);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(115, 13);
            this.label23.TabIndex = 0;
            this.label23.Text = "Time before fade away";
            // 
            // showWhenBox
            // 
            this.showWhenBox.Controls.Add(this.notPlayingCheckBox);
            this.showWhenBox.Controls.Add(this.onlyTriggerRadio);
            this.showWhenBox.Controls.Add(this.allTheTimeRadio);
            this.showWhenBox.Location = new System.Drawing.Point(246, 3);
            this.showWhenBox.Name = "showWhenBox";
            this.showWhenBox.Size = new System.Drawing.Size(238, 84);
            this.showWhenBox.TabIndex = 26;
            this.showWhenBox.TabStop = false;
            this.showWhenBox.Text = "Show Popup";
            // 
            // notPlayingCheckBox
            // 
            this.notPlayingCheckBox.AutoSize = true;
            this.notPlayingCheckBox.Location = new System.Drawing.Point(16, 61);
            this.notPlayingCheckBox.Name = "notPlayingCheckBox";
            this.notPlayingCheckBox.Size = new System.Drawing.Size(136, 17);
            this.notPlayingCheckBox.TabIndex = 22;
            this.notPlayingCheckBox.Tag = "showControl/showWhenNotPlaying";
            this.notPlayingCheckBox.Text = "Show when not playing";
            this.notPlayingCheckBox.UseVisualStyleBackColor = true;
            // 
            // onlyTriggerRadio
            // 
            this.onlyTriggerRadio.AutoSize = true;
            this.onlyTriggerRadio.Location = new System.Drawing.Point(6, 39);
            this.onlyTriggerRadio.Name = "onlyTriggerRadio";
            this.onlyTriggerRadio.Size = new System.Drawing.Size(71, 17);
            this.onlyTriggerRadio.TabIndex = 1;
            this.onlyTriggerRadio.TabStop = true;
            this.onlyTriggerRadio.Text = "On trigger";
            this.onlyTriggerRadio.UseVisualStyleBackColor = true;
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
            // versionLabel
            // 
            this.versionLabel.AutoSize = true;
            this.versionLabel.Location = new System.Drawing.Point(10, 390);
            this.versionLabel.Margin = new System.Windows.Forms.Padding(10, 0, 3, 0);
            this.versionLabel.Name = "versionLabel";
            this.versionLabel.Size = new System.Drawing.Size(31, 13);
            this.versionLabel.TabIndex = 27;
            this.versionLabel.Text = "0.0.0";
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
            this.tabPage2.Size = new System.Drawing.Size(493, 475);
            this.tabPage2.TabIndex = 2;
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
            this.label3.Tag = "display/refreshRate";
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
            this.updateIntervalTrackBar.Tag = "display/refreshRate";
            this.updateIntervalTrackBar.TickFrequency = 50;
            this.updateIntervalTrackBar.Value = 50;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(126, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Max refresh rate (in FPS):";
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
    " that starts loading art after the song\r\nhas already started playing.";
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
            // Properties
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(501, 501);
            this.Controls.Add(this.tabControl1);
            this.Name = "Properties";
            this.Text = "foo_title";
            this.HandleCreated += new System.EventHandler(this.Properties_HandleCreated);
            this.HandleCreated += new System.EventHandler(this.Properties_HandleCreated_Async);
            this.HandleDestroyed += new System.EventHandler(this.Properties_HandleDestroyed);
            this.opacityOpts.ResumeLayout(false);
            this.opacityOpts.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.normalOpacityTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.overOpacityTrackBar)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabLayout1.ResumeLayout(false);
            this.tabLayout1.PerformLayout();
            this.zOrderBox.ResumeLayout(false);
            this.zOrderBox.PerformLayout();
            this.installedSkinsBox.ResumeLayout(false);
            this.installedSkinsLayout.ResumeLayout(false);
            this.enableWhenBox.ResumeLayout(false);
            this.enableWhenBox.PerformLayout();
            this.triggersBox.ResumeLayout(false);
            this.triggersBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.songEndNum)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.songStartNum)).EndInit();
            this.popupPeekBox.ResumeLayout(false);
            this.popupPeekBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.beforeFadeNum)).EndInit();
            this.showWhenBox.ResumeLayout(false);
            this.showWhenBox.PerformLayout();
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

        public static bool IsOpen { get; private set; } = false;

        private void Properties_HandleCreated(object sender, EventArgs e) {
            Assembly myAssembly = Assembly.GetExecutingAssembly();
            versionLabel.Text = "Version: " + myAssembly.GetName().Version;
            IsOpen = true;
        }

        private Task _fillSkinTask;
        private async void Properties_HandleCreated_Async(object sender, EventArgs e)
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _cts = null;
            }
            _cts = new CancellationTokenSource();
            try
            {
                _fillSkinTask = FillSkinListAsync(_cts.Token);
                await _fillSkinTask;
            }
            finally
            {
                _cts = null;
            }
        }

        private void Properties_HandleDestroyed(object sender, EventArgs e) {
            DiscardChanges();

            _cts?.Cancel();
            while (_fillSkinTask != null && _fillSkinTask.Status == TaskStatus.Running)
            {
                Thread.Sleep(10);
            }
            _cts = null;

            IsOpen = false;
        }

        private void skinsList_DoubleClick(object sender, EventArgs e)
        {
            applySkinBtn_Click(null, null);
        }

        private void applySkinBtn_Click(object sender, EventArgs e) {
            if (skinsList.SelectedItems.Count == 0) {
                return;
            }

            _main.SkinPath.ForceUpdate(((SkinListEntry)skinsList.SelectedItems[0]).Path);
            OnChange(); // If the control is not wrapped in a ControlWrapper we need to manually call OnChange
        }

        private void openSkinDirBtn_Click(object sender, EventArgs e) {
            try {
                if (skinsList.SelectedItems.Count == 0)
                {
                    System.Diagnostics.Process.Start(Main.UserDataDir);
                }
                else
                {
                    System.Diagnostics.Process.Start(
                        "explorer.exe",
                        $"/select, \"{((SkinListEntry) skinsList.SelectedItems[0]).Path}\"");
                }
            } catch (Exception ex) {
                MessageBox.Show("foo_title - There was an error opening directory " + Main.UserDataDir + ":\n" + ex.Message, "foo_title");
            }
        }

        private void ResizeListView(ListView lv)
        {
            if (lv == null || lv.Columns.Count < 2)
                return;

            lv.Columns[0].Width = CalculateColumnWidth(lv.Columns[0], lv.Font);
            lv.Columns[1].Width = CalculateColumnWidth(lv.Columns[1], lv.Font);

            bool scrollBarDisplayed = (lv.Items.Count > 0) && (lv.ClientSize.Height < (lv.Items.Count + 1) * lv.Items[0].Bounds.Height);

            if (lv.Columns[0].Width + lv.Columns[1].Width < lv.ClientSize.Width)
            {
                lv.Columns[0].Width = lv.ClientSize.Width - lv.Columns[1].Width - (scrollBarDisplayed ? SystemInformation.VerticalScrollBarWidth : 0);
            }
            else if ( (lv.ClientSize.Width - lv.Columns[0].Width) > 15)
            {
                lv.Columns[1].Width = lv.ClientSize.Width - lv.Columns[0].Width - (scrollBarDisplayed ? SystemInformation.VerticalScrollBarWidth : 0);
            }
        }

        private int CalculateColumnWidth(ColumnHeader column, Font font)
        {
            column.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
            column.AutoResize(ColumnHeaderAutoResizeStyle.None);
            return Math.Max(column.Width, TextRenderer.MeasureText(column.Text, font).Width + 10);
        }
    }
}
