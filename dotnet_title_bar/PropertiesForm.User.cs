using fooTitle.Config;
using fooTitle.Layers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace fooTitle
{
    partial class PropertiesForm : UserControl
    {
        private readonly Properties _parent;
        private readonly Main _main;
        private readonly AutoWrapperCreator _autoWrapperCreator = new();
        private CancellationTokenSource _cts;
        private ProgressTimer _skinListProgressTimer;
        private Task _fillSkinTask;

        public PropertiesForm(Properties parent)
        {
            _main = Main.Get();
            _parent = parent;
            var preferencesCallback = _parent.PreferencesCallback;

            InitializeComponent();
            Font = new Font(new FontFamily("Microsoft Sans Serif"), 8f);

            showWhenWrapper = new RadioGroupWrapper(Configs.ShowControl_EnableWhen.Name, preferencesCallback);
            showWhenWrapper.AddRadioButton(alwaysRadio);
            showWhenWrapper.AddRadioButton(minimizedRadio);
            showWhenWrapper.AddRadioButton(neverRadio);

            windowPositionWrapper = new RadioGroupWrapper(Configs.Display_WindowPosition.Name, preferencesCallback);
            windowPositionWrapper.AddRadioButton(alwaysOnTopRadio);
            windowPositionWrapper.AddRadioButton(onDesktopRadio);
            windowPositionWrapper.AddRadioButton(normalRadio);

            popupShowingWrapper = new RadioGroupWrapper(Configs.ShowControl_ShowPopupWhen.Name, preferencesCallback);
            popupShowingWrapper.AddRadioButton(allTheTimeRadio);
            popupShowingWrapper.AddRadioButton(onlyTriggerRadio);

            enableDraggingWrapper = new RadioGroupWrapper(Configs.Display_IsDraggingEnabled.Name, preferencesCallback);
            enableDraggingWrapper.AddRadioButton(dragAlwaysRadio);
            enableDraggingWrapper.AddRadioButton(dragOnPrefRadio);
            enableDraggingWrapper.AddRadioButton(dragNeverRadio);

            _autoWrapperCreator.CreateWrappers(this, preferencesCallback);
        }

        public static bool IsOpen { get; private set; } = false;

        private async void Properties_HandleCreated(object sender, EventArgs e)
        {
            var myAssembly = Assembly.GetExecutingAssembly();
            versionLabel.Text = "Version: " + myAssembly.GetName().Version;
            IsOpen = true;

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

        private void Properties_HandleDestroyed(object sender, EventArgs e)
        {
            DiscardChanges();

            _cts?.Cancel();
            while (_fillSkinTask != null && _fillSkinTask.Status == TaskStatus.Running)
            {
                Thread.Sleep(10);
            }
            _cts = null;

            IsOpen = false;
        }

        private void SkinsList_DoubleClick(object sender, EventArgs e)
        {
            ApplySkinBtn_Click(null, null);
        }

        private void ApplySkinBtn_Click(object sender, EventArgs e)
        {
            if (skinsList.SelectedItems.Count == 0)
            {
                return;
            }

            var skinEntry = (SkinListEntry)skinsList.SelectedItems[0];
            if (SkinUtils.IsCurrentSkin(skinEntry.DirType, skinEntry.SkinDir))
            { // reload even if it's not changed
                _main.ReloadSkin();
            }
            else
            {
                Configs.Base_SkinDirType.Value = skinEntry.DirType;
                Configs.Base_CurrentSkinName.Value = skinEntry.SkinDir;
            }

            // since the control is not wrapped in a ControlWrapper we need to manually invoke callback
            _parent.PreferencesCallback.OnStateChanged();
        }

        private void OpenSkinDirBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (skinsList.SelectedItems.Count == 0)
                {
                    var psi = new System.Diagnostics.ProcessStartInfo() { FileName = Main.ComponentSkinsDir, UseShellExecute = true };
                    System.Diagnostics.Process.Start(psi);
                }
                else
                {
                    var skinEntry = (SkinListEntry)skinsList.SelectedItems[0];
                    var skinFullPath = SkinUtils.GenerateSkinPath(skinEntry.DirType, skinEntry.SkinDir);
                    System.Diagnostics.Process.Start("explorer.exe", $"/select, \"{skinFullPath}\"");
                }
            }
            catch (Exception ex)
            {
                Utils.ReportErrorWithPopup($"There was an error opening directory {Main.ComponentSkinsDir}:\n"
                                           + $"{ex.Message}\n"
                                           + $"{ex}");
            }
        }

        private async Task<List<ListViewItem>> GetSkinItems(CancellationToken token)
        {
            try
            {
                var skinDirGroup = new[] { SkinDirType.Profile, SkinDirType.ProfileOld, SkinDirType.Component }
                                       .Select(dirType => (DirType: dirType, DirPath: SkinUtils.SkinEnumToRootPath(dirType)))
                                       .Where(dir => Directory.Exists(dir.DirPath))
                                       .Select(dir => (
                                                   DirType: dir.DirType,
                                                   SkinDirs: Directory.GetDirectories(dir.DirPath).Select(x => Path.GetFileName(x)).ToArray()))
                                       .ToArray();
                var totalLength = skinDirGroup.Select(x => x.SkinDirs.Length).Sum();

                var items = new List<ListViewItem>();
                int i = 0;
                foreach (var (dirType, skinDirs) in skinDirGroup)
                {
                    var curItems = new List<ListViewItem>();
                    foreach (var skinDir in skinDirs)
                    {
                        token.ThrowIfCancellationRequested();

                        var skinInfo = Skin.GetSkinInfo(SkinUtils.GenerateSkinPath(dirType, skinDir));
                        if (skinInfo?.Name != null)
                        {
                            var current = new SkinListEntry(dirType, skinDir, skinInfo.Name, skinInfo.Author);
                            if (SkinUtils.IsCurrentSkin(dirType, skinDir))
                            {
                                current.Selected = true;
                                current.EnsureVisible();
                            }
                            curItems.Add(current);
                            await Task.Delay(1, token);
                        }

                        _skinListProgressTimer.Progress = (float)((++i) * 100) / totalLength;
                    }

                    items.AddRange(curItems.OrderBy(x => x.Text.ToLower(), new Utils.ExplorerLikeSort()));
                }

                return items;
            }
            catch (Exception e)
            {
                Console.Get().LogError($"Skin enumeration failed:\n\n"
                                       + $"{e}");
                return new List<ListViewItem>();
            }
        }

        private void StartSkinListFill()
        {
            skinsList.Items.Clear();
            skinsList.Items.Add(new SkinListEntry(SkinDirType.Component, "", "", ""));
            UpdateSkinListProgress(null, 0);
            ResizeListView(skinsList);
        }

        private void UpdateSkinListProgress(object sender, float progress)
        {
            skinsList.Items[0].SubItems[0].Text = $"Loading skins... ({(int)progress}%)";
        }

        private void ResizeListView(ListView lv)
        {
            if (lv == null || lv.Columns.Count < 2)
            {
                return;
            }

            lv.Columns[0].Width = CalculateColumnWidth(lv.Columns[0], lv.Font);
            lv.Columns[1].Width = CalculateColumnWidth(lv.Columns[1], lv.Font);

            bool scrollBarDisplayed = (lv.Items.Count > 0) && (lv.ClientSize.Height < (lv.Items.Count + 1) * lv.Items[0].Bounds.Height);

            if (lv.Columns[0].Width + lv.Columns[1].Width < lv.ClientSize.Width)
            {
                lv.Columns[0].Width = lv.ClientSize.Width - lv.Columns[1].Width - (scrollBarDisplayed ? SystemInformation.VerticalScrollBarWidth : 0);
            }
            else if ((lv.ClientSize.Width - lv.Columns[0].Width) > 15)
            {
                lv.Columns[1].Width = lv.ClientSize.Width - lv.Columns[0].Width - (scrollBarDisplayed ? SystemInformation.VerticalScrollBarWidth : 0);
            }
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

        private void DiscardChanges()
        {
            ConfValuesManager.GetInstance().LoadFrom(Main.Get().Config);
        }

        private int CalculateColumnWidth(ColumnHeader column, Font font)
        {
            column.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
            column.AutoResize(ColumnHeaderAutoResizeStyle.None);
            return Math.Max(column.Width, TextRenderer.MeasureText(column.Text, font).Width + 10);
        }
        private class SkinListEntry : ListViewItem
        {
            public readonly SkinDirType DirType;
            public readonly string SkinDir;

            public SkinListEntry(SkinDirType dirType, string skinDir, string name, string author)
                : base(new[] { name, author })
            {
                DirType = dirType;
                SkinDir = skinDir;
            }
        }
    }

}
