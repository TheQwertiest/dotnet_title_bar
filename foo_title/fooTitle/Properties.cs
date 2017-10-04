/*
*  This file is part of foo_title.
*  Copyright 2005 - 2006 Roman Plasil (http://foo-title.sourceforge.net)
*  Copyright 2016 Miha Lepej (https://github.com/LepkoQQ/foo_title)
*  Copyright 2017 TheQwertiest (https://github.com/TheQwertiest/foo_title)
*
*  This library is free software; you can redistribute it and/or
*  modify it under the terms of the GNU Lesser General Public
*  License as published by the Free Software Foundation; either
*  version 2.1 of the License, or (at your option) any later version.
*
*  This library is distributed in the hope that it will be useful,
*  but WITHOUT ANY WARRANTY; without even the implied warranty of
*  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
*
*  See the file COPYING included with this distribution for more
*  information.
*/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using fooTitle.Config;
using fooTitle.Layers;

namespace fooTitle {
   class Properties : fooManagedWrapper.CManagedPrefPage_v3 {
        //class Properties : Form {

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
        private RadioGroupWrapper popupShowingWrapper;
        private TabPage tabPage1;
        private TableLayoutPanel tabLayout1;
        private GroupBox zOrderBox;
        private CheckBox restoreTopmostCheckbox;
        private RadioButton onDesktopRadio;
        private RadioButton normalRadio;
        private RadioButton alwaysOnTopRadio;
        private GroupBox installedSkinsBox;
        private GroupBox enableWhenBox;
        private RadioButton neverRadio;
        private RadioButton minimizedRadio;
        private RadioButton alwaysRadio;
        private GroupBox triggersBox;
        private Label label9;
        private Label label10;
        private NumericUpDown songEndNum;
        private Label label11;
        private Label label12;
        private NumericUpDown songStartNum;
        private CheckBox songEndCheckBox;
        private CheckBox songStartCheckBox;
        private GroupBox popupPeekBox;
        private Label label21;
        private Label label22;
        private NumericUpDown beforeFadeNum;
        private Label label23;
        private GroupBox showWhenBox;
        private CheckBox notPlayingCheckBox;
        private RadioButton onlyTriggerRadio;
        private RadioButton allTheTimeRadio;
        private Label versionLabel;
        private TableLayoutPanel installedSkinsLayout;
        private Button openSkinDirBtn;
        private Button applySkinBtn;
        private ListView skinsList;
        private ColumnHeader nameColumn;
        private ColumnHeader authorColumn;
        private TabPage tabPage2;
        private TableLayoutPanel tabLayout2;
        private GroupBox opacityBox;
        private Label label7;
        private Label label13;
        private TrackBar normalOpacityBar;
        private TrackBar overOpacityBar;
        private Label label16;
        private Label label17;
        private GroupBox albumArtBox;
        private Label label18;
        private Label label19;
        private Label label20;
        private NumericUpDown artMaxTriesNum;
        private Label label24;
        private Label label25;
        private NumericUpDown artReloadEveryNum;
        private Label label26;
        private GroupBox anchorBox;
        private CheckBox showAnchorCheckBox;
        private Label label27;
        private CheckBox edgeSnapCheckBox_2;
        private NumericUpDown positionYNum;
        private Label label28;
        private NumericUpDown positionXBox;
        private Label label29;
        private GroupBox draggingWhenBox;
        private RadioButton dragNeverRadio;
        private RadioButton dragOnPrefRadio;
        private RadioButton dragAlwaysRadio;
        private GroupBox refreshRateBox;
        private Label label30;
        private TrackBar refreshRateBar;
        private Label label31;
        private TableLayoutPanel tableLayoutPanel2;
        private TableLayoutPanel tableLayoutPanel3;
        private TableLayoutPanel tableLayoutPanel4;
        private TableLayoutPanel tableLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel5;
        private RadioGroupWrapper enableDraggingWrapper;

        public Properties(Main main) : base(new Guid(1414, 548, 7868, 98, 46, 78, 12, 35, 14, 47, 68), fooManagedWrapper.CManagedPrefPage_v3.guid_display)
        {
            _main = main;
            InitializeComponent();

            showWhenWrapper = new RadioGroupWrapper("showControl/enableWhen", this);
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
            enableDraggingWrapper.AddRadioButton(dragAlwaysRadio);
            enableDraggingWrapper.AddRadioButton(dragOnPrefRadio);
            enableDraggingWrapper.AddRadioButton(dragNeverRadio);

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
                        if (path == _main.SkinPath)
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

        private Main _main;

        private void InitializeComponent() {
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
            this.tabLayout2 = new System.Windows.Forms.TableLayoutPanel();
            this.opacityBox = new fooTitle.CustomControl.HorizontalFillDockGroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.label16 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.overOpacityBar = new fooTitle.CustomControl.HorizontalFillDockTrackBar();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.label7 = new System.Windows.Forms.Label();
            this.normalOpacityBar = new fooTitle.CustomControl.HorizontalFillDockTrackBar();
            this.label17 = new System.Windows.Forms.Label();
            this.albumArtBox = new fooTitle.CustomControl.HorizontalFillDockGroupBox();
            this.label18 = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.label20 = new System.Windows.Forms.Label();
            this.artMaxTriesNum = new System.Windows.Forms.NumericUpDown();
            this.label24 = new System.Windows.Forms.Label();
            this.label25 = new System.Windows.Forms.Label();
            this.artReloadEveryNum = new System.Windows.Forms.NumericUpDown();
            this.label26 = new System.Windows.Forms.Label();
            this.anchorBox = new fooTitle.CustomControl.HorizontalFillDockGroupBox();
            this.showAnchorCheckBox = new System.Windows.Forms.CheckBox();
            this.label27 = new System.Windows.Forms.Label();
            this.edgeSnapCheckBox_2 = new System.Windows.Forms.CheckBox();
            this.positionYNum = new System.Windows.Forms.NumericUpDown();
            this.label28 = new System.Windows.Forms.Label();
            this.positionXBox = new System.Windows.Forms.NumericUpDown();
            this.label29 = new System.Windows.Forms.Label();
            this.draggingWhenBox = new fooTitle.CustomControl.HorizontalFillDockGroupBox();
            this.dragNeverRadio = new System.Windows.Forms.RadioButton();
            this.dragOnPrefRadio = new System.Windows.Forms.RadioButton();
            this.dragAlwaysRadio = new System.Windows.Forms.RadioButton();
            this.refreshRateBox = new fooTitle.CustomControl.HorizontalFillDockGroupBox();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label31 = new System.Windows.Forms.Label();
            this.label30 = new System.Windows.Forms.Label();
            this.refreshRateBar = new fooTitle.CustomControl.HorizontalFillDockTrackBar();
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
            this.tabLayout2.SuspendLayout();
            this.opacityBox.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.overOpacityBar)).BeginInit();
            this.tableLayoutPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.normalOpacityBar)).BeginInit();
            this.albumArtBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.artMaxTriesNum)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.artReloadEveryNum)).BeginInit();
            this.anchorBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.positionYNum)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.positionXBox)).BeginInit();
            this.draggingWhenBox.SuspendLayout();
            this.refreshRateBox.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.refreshRateBar)).BeginInit();
            this.SuspendLayout();
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
            this.tabPage1.Text = "Main";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabLayout1
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
            this.installedSkinsBox.Name = "installedSkinsBox";
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
            this.versionLabel.Location = new System.Drawing.Point(6, 390);
            this.versionLabel.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
            this.versionLabel.Name = "versionLabel";
            this.versionLabel.Size = new System.Drawing.Size(31, 13);
            this.versionLabel.TabIndex = 27;
            this.versionLabel.Text = "0.0.0";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.tabLayout2);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(493, 475);
            this.tabPage2.TabIndex = 3;
            this.tabPage2.Text = "Advanced";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // tabLayout2
            // 
            this.tabLayout2.ColumnCount = 2;
            this.tabLayout2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tabLayout2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tabLayout2.Controls.Add(this.opacityBox, 0, 0);
            this.tabLayout2.Controls.Add(this.albumArtBox, 1, 0);
            this.tabLayout2.Controls.Add(this.anchorBox, 1, 1);
            this.tabLayout2.Controls.Add(this.draggingWhenBox, 1, 2);
            this.tabLayout2.Controls.Add(this.refreshRateBox, 0, 1);
            this.tabLayout2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabLayout2.Location = new System.Drawing.Point(3, 3);
            this.tabLayout2.Name = "tabLayout2";
            this.tabLayout2.RowCount = 3;
            this.tabLayout2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tabLayout2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tabLayout2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tabLayout2.Size = new System.Drawing.Size(487, 469);
            this.tabLayout2.TabIndex = 0;
            // 
            // opacityBox
            // 
            this.opacityBox.Controls.Add(this.tableLayoutPanel2);
            this.opacityBox.Location = new System.Drawing.Point(3, 3);
            this.opacityBox.Name = "opacityBox";
            this.opacityBox.Size = new System.Drawing.Size(237, 168);
            this.opacityBox.TabIndex = 11;
            this.opacityBox.TabStop = false;
            this.opacityBox.Text = "Opacity";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel4, 0, 3);
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel3, 0, 1);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 16);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 4;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 45F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 45F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(231, 149);
            this.tableLayoutPanel2.TabIndex = 9;
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 2;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.Controls.Add(this.label16, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.label13, 1, 0);
            this.tableLayoutPanel4.Controls.Add(this.overOpacityBar, 0, 1);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(3, 84);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 2;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(225, 62);
            this.tableLayoutPanel4.TabIndex = 1;
            // 
            // label16
            // 
            this.label16.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(3, 0);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(93, 13);
            this.label16.TabIndex = 1;
            this.label16.Text = "Opacity on trigger:";
            // 
            // label13
            // 
            this.label13.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(182, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(40, 13);
            this.label13.TabIndex = 6;
            this.label13.Tag = "display/overOpacity";
            this.label13.Text = "dummy";
            this.label13.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // overOpacityBar
            // 
            this.overOpacityBar.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.tableLayoutPanel4.SetColumnSpan(this.overOpacityBar, 2);
            this.overOpacityBar.Location = new System.Drawing.Point(3, 16);
            this.overOpacityBar.Maximum = 255;
            this.overOpacityBar.Minimum = 5;
            this.overOpacityBar.Name = "overOpacityBar";
            this.overOpacityBar.Size = new System.Drawing.Size(219, 45);
            this.overOpacityBar.TabIndex = 2;
            this.overOpacityBar.Tag = "display/overOpacity";
            this.overOpacityBar.TickFrequency = 16;
            this.overOpacityBar.Value = 5;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Controls.Add(this.label7, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.normalOpacityBar, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.label17, 0, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 10);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 2;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(225, 61);
            this.tableLayoutPanel3.TabIndex = 0;
            // 
            // label7
            // 
            this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(182, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(40, 13);
            this.label7.TabIndex = 7;
            this.label7.Tag = "display/normalOpacity";
            this.label7.Text = "dummy";
            this.label7.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // normalOpacityBar
            // 
            this.normalOpacityBar.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.tableLayoutPanel3.SetColumnSpan(this.normalOpacityBar, 2);
            this.normalOpacityBar.Location = new System.Drawing.Point(3, 16);
            this.normalOpacityBar.Maximum = 255;
            this.normalOpacityBar.Minimum = 5;
            this.normalOpacityBar.Name = "normalOpacityBar";
            this.normalOpacityBar.Size = new System.Drawing.Size(219, 45);
            this.normalOpacityBar.TabIndex = 3;
            this.normalOpacityBar.Tag = "display/normalOpacity";
            this.normalOpacityBar.TickFrequency = 16;
            this.normalOpacityBar.Value = 5;
            // 
            // label17
            // 
            this.label17.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(3, 0);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(80, 13);
            this.label17.TabIndex = 0;
            this.label17.Text = "Normal opacity:";
            // 
            // albumArtBox
            // 
            this.albumArtBox.Controls.Add(this.label18);
            this.albumArtBox.Controls.Add(this.label19);
            this.albumArtBox.Controls.Add(this.label20);
            this.albumArtBox.Controls.Add(this.artMaxTriesNum);
            this.albumArtBox.Controls.Add(this.label24);
            this.albumArtBox.Controls.Add(this.label25);
            this.albumArtBox.Controls.Add(this.artReloadEveryNum);
            this.albumArtBox.Controls.Add(this.label26);
            this.albumArtBox.Location = new System.Drawing.Point(246, 3);
            this.albumArtBox.Name = "albumArtBox";
            this.albumArtBox.Size = new System.Drawing.Size(238, 167);
            this.albumArtBox.TabIndex = 12;
            this.albumArtBox.TabStop = false;
            this.albumArtBox.Text = "Album Art Reloading";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(6, 16);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(172, 13);
            this.label18.TabIndex = 7;
            this.label18.Text = "If there is no album art loaded retry:";
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.ForeColor = System.Drawing.SystemColors.GrayText;
            this.label19.Location = new System.Drawing.Point(6, 94);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(205, 65);
            this.label19.TabIndex = 6;
            this.label19.Text = "* 0 = never, -1 = no maximum\r\n\r\nUse this if you use an external album art\r\nloader" +
    " that starts loading art after the song\r\nhas already started playing.";
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(134, 68);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(85, 13);
            this.label20.TabIndex = 5;
            this.label20.Text = "time(s) per song*";
            // 
            // artMaxTriesNum
            // 
            this.artMaxTriesNum.Location = new System.Drawing.Point(63, 66);
            this.artMaxTriesNum.Name = "artMaxTriesNum";
            this.artMaxTriesNum.Size = new System.Drawing.Size(65, 20);
            this.artMaxTriesNum.TabIndex = 4;
            this.artMaxTriesNum.Tag = "display/artLoadMaxTimes";
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Location = new System.Drawing.Point(6, 68);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(51, 13);
            this.label24.TabIndex = 3;
            this.label24.Text = "Maximum";
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.Location = new System.Drawing.Point(134, 42);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(53, 13);
            this.label25.TabIndex = 2;
            this.label25.Text = "second(s)";
            // 
            // artReloadEveryNum
            // 
            this.artReloadEveryNum.Location = new System.Drawing.Point(63, 40);
            this.artReloadEveryNum.Name = "artReloadEveryNum";
            this.artReloadEveryNum.Size = new System.Drawing.Size(65, 20);
            this.artReloadEveryNum.TabIndex = 1;
            this.artReloadEveryNum.Tag = "display/artLoadEvery";
            // 
            // label26
            // 
            this.label26.AutoSize = true;
            this.label26.Location = new System.Drawing.Point(6, 42);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(34, 13);
            this.label26.TabIndex = 0;
            this.label26.Text = "Every";
            // 
            // anchorBox
            // 
            this.anchorBox.Controls.Add(this.showAnchorCheckBox);
            this.anchorBox.Controls.Add(this.label27);
            this.anchorBox.Controls.Add(this.edgeSnapCheckBox_2);
            this.anchorBox.Controls.Add(this.positionYNum);
            this.anchorBox.Controls.Add(this.label28);
            this.anchorBox.Controls.Add(this.positionXBox);
            this.anchorBox.Controls.Add(this.label29);
            this.anchorBox.Location = new System.Drawing.Point(246, 177);
            this.anchorBox.Name = "anchorBox";
            this.anchorBox.Size = new System.Drawing.Size(238, 128);
            this.anchorBox.TabIndex = 13;
            this.anchorBox.TabStop = false;
            this.anchorBox.Text = "Anchor Position";
            // 
            // showAnchorCheckBox
            // 
            this.showAnchorCheckBox.AutoSize = true;
            this.showAnchorCheckBox.Location = new System.Drawing.Point(9, 51);
            this.showAnchorCheckBox.Name = "showAnchorCheckBox";
            this.showAnchorCheckBox.Size = new System.Drawing.Size(96, 17);
            this.showAnchorCheckBox.TabIndex = 14;
            this.showAnchorCheckBox.Tag = "display/drawAnchor";
            this.showAnchorCheckBox.Text = "Display anchor";
            this.showAnchorCheckBox.UseVisualStyleBackColor = true;
            // 
            // label27
            // 
            this.label27.AutoSize = true;
            this.label27.ForeColor = System.Drawing.SystemColors.GrayText;
            this.label27.Location = new System.Drawing.Point(6, 94);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(201, 26);
            this.label27.TabIndex = 13;
            this.label27.Text = "If edge snapping is enabled you can hold\r\nCTRL to disable it while dragging.";
            // 
            // edgeSnapCheckBox_2
            // 
            this.edgeSnapCheckBox_2.AutoSize = true;
            this.edgeSnapCheckBox_2.Location = new System.Drawing.Point(9, 74);
            this.edgeSnapCheckBox_2.Name = "edgeSnapCheckBox_2";
            this.edgeSnapCheckBox_2.Size = new System.Drawing.Size(132, 17);
            this.edgeSnapCheckBox_2.TabIndex = 12;
            this.edgeSnapCheckBox_2.Tag = "display/edgeSnap";
            this.edgeSnapCheckBox_2.Text = "Enable edge snapping";
            this.edgeSnapCheckBox_2.UseVisualStyleBackColor = true;
            // 
            // positionYNum
            // 
            this.positionYNum.Location = new System.Drawing.Point(137, 24);
            this.positionYNum.Name = "positionYNum";
            this.positionYNum.Size = new System.Drawing.Size(65, 20);
            this.positionYNum.TabIndex = 11;
            this.positionYNum.Tag = "display/positionY";
            // 
            // label28
            // 
            this.label28.AutoSize = true;
            this.label28.Location = new System.Drawing.Point(114, 26);
            this.label28.Name = "label28";
            this.label28.Size = new System.Drawing.Size(17, 13);
            this.label28.TabIndex = 10;
            this.label28.Text = "Y:";
            // 
            // positionXBox
            // 
            this.positionXBox.Location = new System.Drawing.Point(29, 24);
            this.positionXBox.Name = "positionXBox";
            this.positionXBox.Size = new System.Drawing.Size(65, 20);
            this.positionXBox.TabIndex = 9;
            this.positionXBox.Tag = "display/positionX";
            // 
            // label29
            // 
            this.label29.AutoSize = true;
            this.label29.Location = new System.Drawing.Point(6, 26);
            this.label29.Name = "label29";
            this.label29.Size = new System.Drawing.Size(17, 13);
            this.label29.TabIndex = 8;
            this.label29.Text = "X:";
            // 
            // draggingWhenBox
            // 
            this.draggingWhenBox.Controls.Add(this.dragNeverRadio);
            this.draggingWhenBox.Controls.Add(this.dragOnPrefRadio);
            this.draggingWhenBox.Controls.Add(this.dragAlwaysRadio);
            this.draggingWhenBox.Location = new System.Drawing.Point(246, 311);
            this.draggingWhenBox.Name = "draggingWhenBox";
            this.draggingWhenBox.Size = new System.Drawing.Size(238, 90);
            this.draggingWhenBox.TabIndex = 14;
            this.draggingWhenBox.TabStop = false;
            this.draggingWhenBox.Text = "Enable Dragging";
            // 
            // dragNeverRadio
            // 
            this.dragNeverRadio.AutoSize = true;
            this.dragNeverRadio.Location = new System.Drawing.Point(6, 65);
            this.dragNeverRadio.Name = "dragNeverRadio";
            this.dragNeverRadio.Size = new System.Drawing.Size(54, 17);
            this.dragNeverRadio.TabIndex = 16;
            this.dragNeverRadio.TabStop = true;
            this.dragNeverRadio.Text = "Never";
            this.dragNeverRadio.UseVisualStyleBackColor = true;
            // 
            // dragOnPrefRadio
            // 
            this.dragOnPrefRadio.AutoSize = true;
            this.dragOnPrefRadio.Location = new System.Drawing.Point(6, 42);
            this.dragOnPrefRadio.Name = "dragOnPrefRadio";
            this.dragOnPrefRadio.Size = new System.Drawing.Size(208, 17);
            this.dragOnPrefRadio.TabIndex = 15;
            this.dragOnPrefRadio.TabStop = true;
            this.dragOnPrefRadio.Text = "Only when these preferences are open";
            this.dragOnPrefRadio.UseVisualStyleBackColor = true;
            // 
            // dragAlwaysRadio
            // 
            this.dragAlwaysRadio.AutoSize = true;
            this.dragAlwaysRadio.Location = new System.Drawing.Point(6, 19);
            this.dragAlwaysRadio.Name = "dragAlwaysRadio";
            this.dragAlwaysRadio.Size = new System.Drawing.Size(58, 17);
            this.dragAlwaysRadio.TabIndex = 14;
            this.dragAlwaysRadio.TabStop = true;
            this.dragAlwaysRadio.Text = "Always";
            this.dragAlwaysRadio.UseVisualStyleBackColor = true;
            // 
            // refreshRateBox
            // 
            this.refreshRateBox.Controls.Add(this.tableLayoutPanel5);
            this.refreshRateBox.Location = new System.Drawing.Point(3, 177);
            this.refreshRateBox.Name = "refreshRateBox";
            this.refreshRateBox.Size = new System.Drawing.Size(237, 84);
            this.refreshRateBox.TabIndex = 17;
            this.refreshRateBox.TabStop = false;
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 1;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel5.Controls.Add(this.tableLayoutPanel1, 0, 0);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(3, 16);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 1;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(231, 65);
            this.tableLayoutPanel5.TabIndex = 10;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.label31, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.label30, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.refreshRateBar, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(225, 59);
            this.tableLayoutPanel1.TabIndex = 9;
            // 
            // label31
            // 
            this.label31.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label31.AutoSize = true;
            this.label31.Location = new System.Drawing.Point(3, 0);
            this.label31.Name = "label31";
            this.label31.Size = new System.Drawing.Size(126, 13);
            this.label31.TabIndex = 3;
            this.label31.Text = "Max refresh rate (in FPS):";
            // 
            // label30
            // 
            this.label30.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label30.AutoSize = true;
            this.label30.Location = new System.Drawing.Point(187, 0);
            this.label30.Name = "label30";
            this.label30.Size = new System.Drawing.Size(35, 13);
            this.label30.TabIndex = 8;
            this.label30.Tag = "display/refreshRate";
            this.label30.Text = "label8";
            this.label30.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // refreshRateBar
            // 
            this.refreshRateBar.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.tableLayoutPanel1.SetColumnSpan(this.refreshRateBar, 2);
            this.refreshRateBar.LargeChange = 100;
            this.refreshRateBar.Location = new System.Drawing.Point(3, 16);
            this.refreshRateBar.Maximum = 500;
            this.refreshRateBar.Minimum = 50;
            this.refreshRateBar.Name = "refreshRateBar";
            this.refreshRateBar.Size = new System.Drawing.Size(219, 45);
            this.refreshRateBar.SmallChange = 10;
            this.refreshRateBar.TabIndex = 2;
            this.refreshRateBar.Tag = "display/refreshRate";
            this.refreshRateBar.TickFrequency = 50;
            this.refreshRateBar.Value = 50;
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
            this.tabLayout2.ResumeLayout(false);
            this.opacityBox.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.overOpacityBar)).EndInit();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.normalOpacityBar)).EndInit();
            this.albumArtBox.ResumeLayout(false);
            this.albumArtBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.artMaxTriesNum)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.artReloadEveryNum)).EndInit();
            this.anchorBox.ResumeLayout(false);
            this.anchorBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.positionYNum)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.positionXBox)).EndInit();
            this.draggingWhenBox.ResumeLayout(false);
            this.draggingWhenBox.PerformLayout();
            this.refreshRateBox.ResumeLayout(false);
            this.tableLayoutPanel5.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.refreshRateBar)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion

        public static bool IsOpen { get; private set; } = false;

        private Task _fillSkinTask;
        private void Properties_HandleCreated(object sender, EventArgs e) {
            Assembly myAssembly = Assembly.GetExecutingAssembly();
            versionLabel.Text = "Version: " + myAssembly.GetName().Version;
            IsOpen = true;
        }

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

            _main.SkinPath = ((SkinListEntry)skinsList.SelectedItems[0]).Path;
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
