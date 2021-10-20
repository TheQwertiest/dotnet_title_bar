using fooTitle.Config;
using Qwr.ComponentInterface;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace fooTitle
{
    class Preferences : IPreferencesPage
    {
        public IPreferencesPageCallback PreferencesCallback;
        private ContainerControl _impl;

        public static PreferencesPageInfo Info
        {
            get
            {
                PreferencesPageInfo info;
                info.Name = Constants.ComponentName;
                info.Guid = Guids.PreferencesPage_Main;
                info.ParentGuid = Main.Get().Fb2kUtils.Fb2kGuid(Fb2kGuidId.PrefPage_Display);
                info.HelpUrl = "https://theqwertiest.github.io/dotnet_title_bar/";

                return info;
            }
        }

        public void Initialize(IntPtr parentHandle, IPreferencesPageCallback callback)
        {
            PreferencesCallback = callback;

            _impl = new PreferencesForm(this);
            Win32.SetParent(_impl.Handle, parentHandle);
            _impl.Anchor = (AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom);
            _impl.Show();
        }

        public void Reset()
        {
            ConfValuesManager.GetInstance().Reset();
        }

        public void Apply()
        {
            ConfValuesManager.GetInstance().SaveTo(Main.Get().Config);
        }

        public PreferencesPageState State()
        {
            PreferencesPageState state = PreferencesPageState.IsResettable;
            if (ConfValuesManager.GetInstance().HasChanged())
            {
                state |= PreferencesPageState.HasChanged;
            }

            return state;
        }

        IntPtr IPreferencesPage.Handle()
        {
            return _impl.Handle;
        }

        public static bool IsOpen => PreferencesForm.IsOpen;
    }
}
